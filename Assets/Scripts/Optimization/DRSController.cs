using System.Collections;
using UnityEngine;
using Swiftboost.Core;
using Swiftboost.Game;

namespace Swiftboost.Optimization
{
    public class DRSController : MonoBehaviour
    {
        [Header("DRS Configuration")]
        [SerializeField] private bool drsEnabled = true;
        [SerializeField] private float targetFPS = 60f;
        [SerializeField] private float minResolutionScale = 0.5f;
        [SerializeField] private float maxResolutionScale = 1.0f;
        [SerializeField] private float currentResolutionScale = 1.0f;
        
        [Header("Adjustment Settings")]
        [SerializeField] private float adjustmentStep = 0.05f;
        [SerializeField] private float stabilizationTime = 3f;
        [SerializeField] private float fpsCheckInterval = 1f;
        
        [Header("Device Specific")]
        [SerializeField] private float deviceSpecificMultiplier = 1.0f;
        [SerializeField] private bool useDeviceProfile = true;
        
        // State tracking
        private float[] fpsHistory = new float[10];
        private int fpsHistoryIndex = 0;
        private float lastAdjustmentTime = 0f;
        private bool isOptimizing = false;
        private DeviceProfiler.DeviceInfo currentDevice;
        private Coroutine drsCoroutine;
        
        // Events
        public System.Action<float> OnResolutionScaleChanged;
        public System.Action<string> OnDRSOptimizationApplied;
        
        public bool IsEnabled => drsEnabled;
        public float CurrentResolutionScale => currentResolutionScale;
        public float TargetFPS => targetFPS;
        
        private void Start()
        {
            InitializeDRS();
        }
        
        private void InitializeDRS()
        {
            // Initialize FPS history
            for (int i = 0; i < fpsHistory.Length; i++)
            {
                fpsHistory[i] = targetFPS;
            }
            
            Debug.Log("DRS Controller initialized");
        }
        
        public void InitializeForDevice(DeviceProfiler.DeviceInfo deviceInfo)
        {
            currentDevice = deviceInfo;
            ApplyDeviceSpecificSettings();
        }
        
        private void ApplyDeviceSpecificSettings()
        {
            if (currentDevice == null || !useDeviceProfile) return;
            
            string brand = currentDevice.brand.ToLower();
            
            // Device-specific DRS settings
            switch (brand)
            {
                case "samsung":
                    if (currentDevice.model.Contains("S23") || currentDevice.model.Contains("S24"))
                    {
                        deviceSpecificMultiplier = 1.2f; // More aggressive on flagship Samsung
                        minResolutionScale = 0.6f;
                        maxResolutionScale = 1.2f;
                    }
                    else
                    {
                        deviceSpecificMultiplier = 1.0f;
                        minResolutionScale = 0.5f;
                        maxResolutionScale = 1.0f;
                    }
                    break;
                    
                case "oneplus":
                    deviceSpecificMultiplier = 1.1f; // OnePlus devices handle heat well
                    minResolutionScale = 0.55f;
                    maxResolutionScale = 1.1f;
                    break;
                    
                case "google":
                    deviceSpecificMultiplier = 0.9f; // Tensor chips need conservative approach
                    minResolutionScale = 0.45f;
                    maxResolutionScale = 1.0f;
                    break;
                    
                case "xiaomi":
                    deviceSpecificMultiplier = 1.0f; // Standard approach
                    minResolutionScale = 0.5f;
                    maxResolutionScale = 1.0f;
                    break;
                    
                default:
                    deviceSpecificMultiplier = 1.0f;
                    break;
            }
            
            Debug.Log($"Applied DRS settings for {brand} - Multiplier: {deviceSpecificMultiplier:F2}, Range: {minResolutionScale:F2}-{maxResolutionScale:F2}");
        }
        
        public void SetMode(OptimizationMode mode)
        {
            switch (mode)
            {
                case OptimizationMode.BatterySaver:
                    targetFPS = 30f;
                    adjustmentStep = 0.03f;
                    stabilizationTime = 5f;
                    break;
                    
                case OptimizationMode.Balanced:
                    targetFPS = 60f;
                    adjustmentStep = 0.05f;
                    stabilizationTime = 3f;
                    break;
                    
                case OptimizationMode.HighPerformance:
                    targetFPS = 120f;
                    adjustmentStep = 0.08f;
                    stabilizationTime = 2f;
                    break;
            }
            
            Debug.Log($"DRS mode set to {mode} - Target FPS: {targetFPS}, Step: {adjustmentStep:F3}");
        }
        
        public void SetAggressiveness(float aggressiveness)
        {
            // Adjust step size based on aggressiveness
            float baseStep = 0.05f;
            adjustmentStep = baseStep * (0.5f + aggressiveness * 0.5f);
            
            // Apply device multiplier
            adjustmentStep *= deviceSpecificMultiplier;
            
            Debug.Log($"DRS aggressiveness set to {aggressiveness:F2} - Step size: {adjustmentStep:F3}");
        }
        
        public void StartOptimization()
        {
            if (isOptimizing || !drsEnabled)
            {
                Debug.LogWarning("DRS optimization already running or disabled");
                return;
            }
            
            isOptimizing = true;
            drsCoroutine = StartCoroutine(DRSOptimizationRoutine());
            
            Debug.Log("DRS optimization started");
        }
        
        public void StopOptimization()
        {
            if (!isOptimizing)
            {
                Debug.LogWarning("DRS optimization not running");
                return;
            }
            
            isOptimizing = false;
            
            if (drsCoroutine != null)
            {
                StopCoroutine(drsCoroutine);
                drsCoroutine = null;
            }
            
            Debug.Log("DRS optimization stopped");
        }
        
        private IEnumerator DRSOptimizationRoutine()
        {
            while (isOptimizing)
            {
                yield return new WaitForSeconds(fpsCheckInterval);
                
                UpdateFPSHistory();
                EvaluatePerformance();
            }
        }
        
        private void UpdateFPSHistory()
        {
            float currentFPS = Time.frameCount > 0 ? 1f / Time.unscaledDeltaTime : 0f;
            
            fpsHistory[fpsHistoryIndex] = currentFPS;
            fpsHistoryIndex = (fpsHistoryIndex + 1) % fpsHistory.Length;
        }
        
        private void EvaluatePerformance()
        {
            float averageFPS = GetAverageFPS();
            float fpsRatio = averageFPS / targetFPS;
            
            // Only adjust if enough time has passed since last adjustment
            if (Time.time - lastAdjustmentTime < stabilizationTime)
                return;
            
            bool adjusted = false;
            
            if (fpsRatio < 0.85f) // Significantly below target
            {
                DecreaseResolution();
                adjusted = true;
            }
            else if (fpsRatio > 1.1f) // Significantly above target
            {
                IncreaseResolution();
                adjusted = true;
            }
            
            if (adjusted)
            {
                lastAdjustmentTime = Time.time;
                OnDRSOptimizationApplied?.Invoke($"Resolution scale adjusted to {currentResolutionScale:F2} (FPS: {averageFPS:F1})");
            }
        }
        
        private float GetAverageFPS()
        {
            float total = 0f;
            for (int i = 0; i < fpsHistory.Length; i++)
            {
                total += fpsHistory[i];
            }
            return total / fpsHistory.Length;
        }
        
        public void DecreaseResolution()
        {
            float newScale = currentResolutionScale - adjustmentStep;
            newScale = Mathf.Max(newScale, minResolutionScale);
            
            if (newScale != currentResolutionScale)
            {
                SetResolutionScale(newScale);
                Debug.Log($"DRS: Decreased resolution scale to {newScale:F2}");
            }
        }
        
        public void IncreaseResolution()
        {
            float newScale = currentResolutionScale + adjustmentStep;
            newScale = Mathf.Min(newScale, maxResolutionScale);
            
            if (newScale != currentResolutionScale)
            {
                SetResolutionScale(newScale);
                Debug.Log($"DRS: Increased resolution scale to {newScale:F2}");
            }
        }
        
        private void SetResolutionScale(float scale)
        {
            currentResolutionScale = Mathf.Clamp(scale, minResolutionScale, maxResolutionScale);
            
            // Apply resolution scaling to the main camera
            ApplyResolutionScale();
            
            OnResolutionScaleChanged?.Invoke(currentResolutionScale);
        }
        
        private void ApplyResolutionScale()
        {
            // Apply resolution scaling through Unity's XR settings or render texture scaling
            try
            {
                #if UNITY_2019_1_OR_NEWER
                UnityEngine.XR.XRSettings.eyeTextureResolutionScale = currentResolutionScale;
                #endif
                
                // Alternative method: Scale render texture
                var cameras = Camera.allCameras;
                foreach (var cam in cameras)
                {
                    if (cam.targetTexture != null)
                    {
                        // Scale existing render texture
                        int newWidth = Mathf.RoundToInt(Screen.width * currentResolutionScale);
                        int newHeight = Mathf.RoundToInt(Screen.height * currentResolutionScale);
                        
                        if (cam.targetTexture.width != newWidth || cam.targetTexture.height != newHeight)
                        {
                            cam.targetTexture.Release();
                            cam.targetTexture = new RenderTexture(newWidth, newHeight, 24);
                        }
                    }
                }
                
                Debug.Log($"Applied resolution scale: {currentResolutionScale:F2}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to apply resolution scale: {e.Message}");
            }
        }
        
        public void ApplyOptimization(float strength)
        {
            if (!drsEnabled) return;
            
            // Apply immediate optimization based on strength
            float targetScale = Mathf.Lerp(maxResolutionScale, minResolutionScale, strength);
            float adjustedScale = Mathf.MoveTowards(currentResolutionScale, targetScale, adjustmentStep * 2f);
            
            SetResolutionScale(adjustedScale);
            
            Debug.Log($"DRS: Applied optimization (strength: {strength:F2}) - Scale: {adjustedScale:F2}");
        }
        
        public void ReduceOptimization(float amount)
        {
            if (!drsEnabled) return;
            
            // Gradually increase resolution scale
            float newScale = currentResolutionScale + (adjustmentStep * amount);
            newScale = Mathf.Min(newScale, maxResolutionScale);
            
            SetResolutionScale(newScale);
            
            Debug.Log($"DRS: Reduced optimization - Scale: {newScale:F2}");
        }
        
        public void SetDRSEnabled(bool enabled)
        {
            drsEnabled = enabled;
            
            if (!enabled && isOptimizing)
            {
                StopOptimization();
                
                // Reset to maximum resolution
                SetResolutionScale(maxResolutionScale);
            }
            
            Debug.Log($"DRS {(enabled ? "enabled" : "disabled")}");
        }
        
        public void SetTargetFPS(float fps)
        {
            targetFPS = Mathf.Max(fps, 15f); // Minimum 15 FPS
            Debug.Log($"DRS target FPS set to {targetFPS}");
        }
        
        public void SetResolutionRange(float min, float max)
        {
            minResolutionScale = Mathf.Clamp(min, 0.25f, 0.95f);
            maxResolutionScale = Mathf.Clamp(max, 0.5f, 2.0f);
            
            // Ensure min is less than max
            if (minResolutionScale >= maxResolutionScale)
            {
                minResolutionScale = maxResolutionScale - 0.1f;
            }
            
            // Clamp current scale to new range
            currentResolutionScale = Mathf.Clamp(currentResolutionScale, minResolutionScale, maxResolutionScale);
            
            Debug.Log($"DRS resolution range set to {minResolutionScale:F2} - {maxResolutionScale:F2}");
        }
        
        public string GetDRSStatus()
        {
            var status = $"DRS Status:\n";
            status += $"Enabled: {drsEnabled}\n";
            status += $"Current Scale: {currentResolutionScale:F2}\n";
            status += $"Range: {minResolutionScale:F2} - {maxResolutionScale:F2}\n";
            status += $"Target FPS: {targetFPS}\n";
            status += $"Average FPS: {GetAverageFPS():F1}\n";
            status += $"Optimization Active: {isOptimizing}\n";
            status += $"Device Multiplier: {deviceSpecificMultiplier:F2}\n";
            
            if (currentDevice != null)
            {
                status += $"Device: {currentDevice.GetFullDeviceName()}\n";
            }
            
            return status;
        }
        
        private void OnDestroy()
        {
            if (isOptimizing)
            {
                StopOptimization();
            }
        }
    }
}