using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Swiftboost.Core;

namespace Swiftboost.Monitoring
{
    public class MonitoringOverlay : MonoBehaviour
    {
        [Header("Overlay Settings")]
        [SerializeField] private bool showOverlay = true;
        [SerializeField] private bool minimizedMode = false;
        [SerializeField] private float updateInterval = 1f;
        [SerializeField] private Vector2 overlayPosition = new Vector2(10, 10);
        
        [Header("UI References")]
        [SerializeField] private Canvas overlayCanvas;
        [SerializeField] private GameObject overlayPanel;
        [SerializeField] private GameObject minimizedPanel;
        
        [Header("Device Information")]
        [SerializeField] private TextMeshProUGUI deviceNameText;
        [SerializeField] private TextMeshProUGUI deviceModelText;
        [SerializeField] private Image brandLogoImage;
        
        [Header("Performance Metrics")]
        [SerializeField] private TextMeshProUGUI fpsText;
        [SerializeField] private TextMeshProUGUI ramText;
        [SerializeField] private TextMeshProUGUI cpuText;
        [SerializeField] private TextMeshProUGUI temperatureText;
        
        [Header("Progress Bars")]
        [SerializeField] private Slider ramProgressBar;
        [SerializeField] private Slider cpuProgressBar;
        [SerializeField] private Slider temperatureProgressBar;
        
        [Header("Color Coding")]
        [SerializeField] private Color goodColor = Color.green;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;
        
        [Header("Minimized Display")]
        [SerializeField] private TextMeshProUGUI minimizedFPSText;
        [SerializeField] private TextMeshProUGUI minimizedTempText;
        
        // References
        private UsageMonitor usageMonitor;
        private DeviceProfiler deviceProfiler;
        private RectTransform overlayRect;
        
        // Update coroutine
        private Coroutine updateCoroutine;
        
        // Drag functionality
        private bool isDragging = false;
        private Vector2 lastMousePosition;
        
        public bool IsVisible => showOverlay && overlayCanvas.gameObject.activeInHierarchy;
        
        private void Start()
        {
            InitializeOverlay();
        }
        
        private void InitializeOverlay()
        {
            // Get references
            usageMonitor = SystemManager.Instance?.UsageMonitor;
            deviceProfiler = SystemManager.Instance?.DeviceProfiler;
            
            if (usageMonitor == null || deviceProfiler == null)
            {
                Debug.LogError("MonitoringOverlay: Required components not found!");
                return;
            }
            
            // Setup canvas
            SetupOverlayCanvas();
            
            // Initialize device information display
            InitializeDeviceDisplay();
            
            // Position overlay
            PositionOverlay();
            
            // Start updates
            if (showOverlay)
            {
                StartOverlayUpdates();
            }
            
            Debug.Log("MonitoringOverlay initialized");
        }
        
        private void SetupOverlayCanvas()
        {
            if (overlayCanvas == null)
            {
                // Create overlay canvas if it doesn't exist
                GameObject canvasGO = new GameObject("MonitoringOverlayCanvas");
                canvasGO.transform.SetParent(transform);
                
                overlayCanvas = canvasGO.AddComponent<Canvas>();
                overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                overlayCanvas.sortingOrder = 1000; // High sorting order to appear on top
                
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }
            
            overlayRect = overlayCanvas.GetComponent<RectTransform>();
        }
        
        private void InitializeDeviceDisplay()
        {
            var deviceInfo = deviceProfiler?.GetCurrentDevice();
            if (deviceInfo == null) return;
            
            if (deviceNameText != null)
            {
                deviceNameText.text = deviceInfo.GetFullDeviceName();
            }
            
            if (deviceModelText != null)
            {
                deviceModelText.text = $"{deviceInfo.chipset} | Android {deviceInfo.androidVersion}";
            }
            
            // Load brand logo
            LoadBrandLogo(deviceInfo.brand);
        }
        
        private void LoadBrandLogo(string brand)
        {
            if (brandLogoImage == null) return;
            
            string logoPath = $"DeviceIcons/{brand.ToLower()}_logo";
            Sprite logoSprite = Resources.Load<Sprite>(logoPath);
            
            if (logoSprite != null)
            {
                brandLogoImage.sprite = logoSprite;
                brandLogoImage.gameObject.SetActive(true);
            }
            else
            {
                brandLogoImage.gameObject.SetActive(false);
                Debug.LogWarning($"Brand logo not found: {logoPath}");
            }
        }
        
        public void ShowOverlay()
        {
            showOverlay = true;
            if (overlayCanvas != null)
            {
                overlayCanvas.gameObject.SetActive(true);
                StartOverlayUpdates();
            }
        }
        
        public void HideOverlay()
        {
            showOverlay = false;
            if (overlayCanvas != null)
            {
                overlayCanvas.gameObject.SetActive(false);
                StopOverlayUpdates();
            }
        }
        
        public void ToggleOverlay()
        {
            if (showOverlay)
                HideOverlay();
            else
                ShowOverlay();
        }
        
        public void SetMinimizedMode(bool minimized)
        {
            minimizedMode = minimized;
            
            if (overlayPanel != null)
                overlayPanel.SetActive(!minimized);
            
            if (minimizedPanel != null)
                minimizedPanel.SetActive(minimized);
        }
        
        public void ToggleMinimizedMode()
        {
            SetMinimizedMode(!minimizedMode);
        }
        
        private void StartOverlayUpdates()
        {
            if (updateCoroutine == null)
            {
                updateCoroutine = StartCoroutine(UpdateOverlayRoutine());
            }
        }
        
        private void StopOverlayUpdates()
        {
            if (updateCoroutine != null)
            {
                StopCoroutine(updateCoroutine);
                updateCoroutine = null;
            }
        }
        
        private System.Collections.IEnumerator UpdateOverlayRoutine()
        {
            while (showOverlay)
            {
                UpdateOverlayDisplay();
                yield return new WaitForSeconds(updateInterval);
            }
        }
        
        private void UpdateOverlayDisplay()
        {
            if (usageMonitor == null) return;
            
            var metrics = usageMonitor.GetCurrentMetrics();
            
            // Update FPS display
            UpdateFPSDisplay(metrics.fps);
            
            // Update RAM display
            UpdateRAMDisplay(metrics.ramUsage);
            
            // Update CPU display
            UpdateCPUDisplay(metrics.cpuUsage);
            
            // Update Temperature display
            UpdateTemperatureDisplay(metrics.temperature);
            
            // Update minimized display
            if (minimizedMode)
            {
                UpdateMinimizedDisplay(metrics);
            }
        }
        
        private void UpdateFPSDisplay(float fps)
        {
            if (fpsText != null)
            {
                fpsText.text = $"FPS: {fps:F1}";
                
                // Color code based on FPS
                if (fps >= 55f)
                    fpsText.color = goodColor;
                else if (fps >= 30f)
                    fpsText.color = warningColor;
                else
                    fpsText.color = criticalColor;
            }
        }
        
        private void UpdateRAMDisplay(float ramUsage)
        {
            if (ramText != null)
            {
                ramText.text = $"RAM: {ramUsage:F1}%";
                
                // Color code based on RAM usage
                if (ramUsage <= 70f)
                    ramText.color = goodColor;
                else if (ramUsage <= 85f)
                    ramText.color = warningColor;
                else
                    ramText.color = criticalColor;
            }
            
            if (ramProgressBar != null)
            {
                ramProgressBar.value = ramUsage / 100f;
                
                // Update progress bar color
                var fillImage = ramProgressBar.fillRect?.GetComponent<Image>();
                if (fillImage != null)
                {
                    if (ramUsage <= 70f)
                        fillImage.color = goodColor;
                    else if (ramUsage <= 85f)
                        fillImage.color = warningColor;
                    else
                        fillImage.color = criticalColor;
                }
            }
        }
        
        private void UpdateCPUDisplay(float cpuUsage)
        {
            if (cpuText != null)
            {
                cpuText.text = $"CPU: {cpuUsage:F1}%";
                
                // Color code based on CPU usage
                if (cpuUsage <= 70f)
                    cpuText.color = goodColor;
                else if (cpuUsage <= 85f)
                    cpuText.color = warningColor;
                else
                    cpuText.color = criticalColor;
            }
            
            if (cpuProgressBar != null)
            {
                cpuProgressBar.value = cpuUsage / 100f;
                
                // Update progress bar color
                var fillImage = cpuProgressBar.fillRect?.GetComponent<Image>();
                if (fillImage != null)
                {
                    if (cpuUsage <= 70f)
                        fillImage.color = goodColor;
                    else if (cpuUsage <= 85f)
                        fillImage.color = warningColor;
                    else
                        fillImage.color = criticalColor;
                }
            }
        }
        
        private void UpdateTemperatureDisplay(float temperature)
        {
            if (temperatureText != null)
            {
                temperatureText.text = $"Temp: {temperature:F1}°C";
                
                // Color code based on temperature
                if (temperature <= 40f)
                    temperatureText.color = goodColor;
                else if (temperature <= 50f)
                    temperatureText.color = warningColor;
                else
                    temperatureText.color = criticalColor;
            }
            
            if (temperatureProgressBar != null)
            {
                // Temperature range: 25°C to 60°C
                float normalizedTemp = Mathf.Clamp01((temperature - 25f) / 35f);
                temperatureProgressBar.value = normalizedTemp;
                
                // Update progress bar color
                var fillImage = temperatureProgressBar.fillRect?.GetComponent<Image>();
                if (fillImage != null)
                {
                    if (temperature <= 40f)
                        fillImage.color = goodColor;
                    else if (temperature <= 50f)
                        fillImage.color = warningColor;
                    else
                        fillImage.color = criticalColor;
                }
            }
        }
        
        private void UpdateMinimizedDisplay(SystemMetrics metrics)
        {
            if (minimizedFPSText != null)
            {
                minimizedFPSText.text = $"{metrics.fps:F0}";
                
                if (metrics.fps >= 55f)
                    minimizedFPSText.color = goodColor;
                else if (metrics.fps >= 30f)
                    minimizedFPSText.color = warningColor;
                else
                    minimizedFPSText.color = criticalColor;
            }
            
            if (minimizedTempText != null)
            {
                minimizedTempText.text = $"{metrics.temperature:F0}°";
                
                if (metrics.temperature <= 40f)
                    minimizedTempText.color = goodColor;
                else if (metrics.temperature <= 50f)
                    minimizedTempText.color = warningColor;
                else
                    minimizedTempText.color = criticalColor;
            }
        }
        
        private void PositionOverlay()
        {
            if (overlayRect != null)
            {
                overlayRect.anchoredPosition = overlayPosition;
            }
        }
        
        public void SetOverlayPosition(Vector2 position)
        {
            overlayPosition = position;
            PositionOverlay();
        }
        
        // Drag functionality
        public void OnBeginDrag()
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }
        
        public void OnDrag()
        {
            if (!isDragging) return;
            
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 deltaPosition = currentMousePosition - lastMousePosition;
            
            overlayPosition += deltaPosition;
            PositionOverlay();
            
            lastMousePosition = currentMousePosition;
        }
        
        public void OnEndDrag()
        {
            isDragging = false;
        }
        
        private void OnDestroy()
        {
            StopOverlayUpdates();
        }
    }
}