using System;
using System.Collections;
using UnityEngine;
using Swiftboost.Core;

namespace Swiftboost.Monitoring
{
    public class UsageMonitor : MonoBehaviour
    {
        [Header("Monitoring Settings")]
        [SerializeField] private bool isMonitoring = false;
        [SerializeField] private bool optimizationMode = false;
        [SerializeField] private bool lowPowerMode = false;
        
        [Header("Update Intervals (seconds)")]
        [SerializeField] private float fpsUpdateInterval = 1.0f;
        [SerializeField] private float ramUpdateInterval = 2.0f;
        [SerializeField] private float cpuUpdateInterval = 1.5f;
        [SerializeField] private float temperatureUpdateInterval = 5.0f;
        [SerializeField] private float storageUpdateInterval = 10.0f;
        
        [Header("Current Metrics")]
        [SerializeField] private float currentFPS = 0f;
        [SerializeField] private float currentRAMUsage = 0f;
        [SerializeField] private float currentCPUUsage = 0f;
        [SerializeField] private float currentGPUUsage = 0f;
        [SerializeField] private float currentTemperature = 0f;
        [SerializeField] private long currentStorageUsage = 0L;
        [SerializeField] private int currentRefreshRate = 60;
        [SerializeField] private bool bluetoothEnabled = false;
        [SerializeField] private float brightnessLevel = 0.5f;
        
        // References
        private AndroidBridge androidBridge;
        private DeviceProfiler deviceProfiler;
        
        // Events for UI updates
        public event Action<float> OnFPSUpdate;
        public event Action<float> OnRAMUsageUpdate;
        public event Action<float> OnCPUUsageUpdate;
        public event Action<float> OnGPUUsageUpdate;
        public event Action<float> OnTemperatureUpdate;
        public event Action<long> OnStorageUpdate;
        public event Action<SystemMetrics> OnMetricsUpdate;
        
        // Coroutine references
        private Coroutine fpsCoroutine;
        private Coroutine ramCoroutine;
        private Coroutine cpuCoroutine;
        private Coroutine temperatureCoroutine;
        private Coroutine storageCoroutine;
        
        // FPS calculation
        private float[] fpsBuffer = new float[60];
        private int fpsBufferIndex = 0;
        
        public float CurrentFPS => currentFPS;
        public float CurrentRAMUsage => currentRAMUsage;
        public float CurrentCPUUsage => currentCPUUsage;
        public float CurrentGPUUsage => currentGPUUsage;
        public float CurrentTemperature => currentTemperature;
        public long CurrentStorageUsage => currentStorageUsage;
        public bool IsMonitoring => isMonitoring;
        
        private void Start()
        {
            InitializeMonitor();
        }
        
        private void InitializeMonitor()
        {
            androidBridge = SystemManager.Instance?.AndroidBridge;
            deviceProfiler = SystemManager.Instance?.DeviceProfiler;
            
            if (androidBridge == null || deviceProfiler == null)
            {
                Debug.LogError("UsageMonitor: Required components not found!");
                return;
            }
            
            // Apply device-specific monitoring intervals
            ApplyDeviceSpecificSettings();
            
            Debug.Log("UsageMonitor initialized");
        }
        
        private void ApplyDeviceSpecificSettings()
        {
            if (deviceProfiler?.GetCurrentDevice() == null) return;
            
            var deviceInfo = deviceProfiler.GetCurrentDevice();
            string brand = deviceInfo.brand.ToLower();
            
            // Adjust monitoring intervals based on device brand
            switch (brand)
            {
                case "samsung":
                    fpsUpdateInterval = 0.8f;
                    ramUpdateInterval = 1.5f;
                    cpuUpdateInterval = 1.2f;
                    temperatureUpdateInterval = 4.0f;
                    break;
                case "oneplus":
                    fpsUpdateInterval = 1.0f;
                    ramUpdateInterval = 2.0f;
                    cpuUpdateInterval = 1.5f;
                    temperatureUpdateInterval = 5.0f;
                    break;
                case "google":
                    fpsUpdateInterval = 1.2f;
                    ramUpdateInterval = 2.5f;
                    cpuUpdateInterval = 1.8f;
                    temperatureUpdateInterval = 6.0f;
                    break;
                case "xiaomi":
                    fpsUpdateInterval = 1.0f;
                    ramUpdateInterval = 2.0f;
                    cpuUpdateInterval = 1.5f;
                    temperatureUpdateInterval = 5.0f;
                    break;
                default:
                    // Use default values
                    break;
            }
        }
        
        public void StartMonitoring()
        {
            if (isMonitoring)
            {
                Debug.LogWarning("Monitoring already active");
                return;
            }
            
            Debug.Log("Starting system monitoring");
            isMonitoring = true;
            
            // Start monitoring coroutines
            fpsCoroutine = StartCoroutine(MonitorFPS());
            ramCoroutine = StartCoroutine(MonitorRAMUsage());
            cpuCoroutine = StartCoroutine(MonitorCPUUsage());
            temperatureCoroutine = StartCoroutine(MonitorTemperature());
            storageCoroutine = StartCoroutine(MonitorStorage());
            
            // Update additional metrics immediately
            UpdateAdditionalMetrics();
        }
        
        public void StopMonitoring()
        {
            if (!isMonitoring)
            {
                Debug.LogWarning("Monitoring not active");
                return;
            }
            
            Debug.Log("Stopping system monitoring");
            isMonitoring = false;
            
            // Stop all monitoring coroutines
            if (fpsCoroutine != null) StopCoroutine(fpsCoroutine);
            if (ramCoroutine != null) StopCoroutine(ramCoroutine);
            if (cpuCoroutine != null) StopCoroutine(cpuCoroutine);
            if (temperatureCoroutine != null) StopCoroutine(temperatureCoroutine);
            if (storageCoroutine != null) StopCoroutine(storageCoroutine);
        }
        
        public void EnableOptimizationMode()
        {
            optimizationMode = true;
            Debug.Log("Optimization mode enabled - Enhanced monitoring active");
        }
        
        public void DisableOptimizationMode()
        {
            optimizationMode = false;
            Debug.Log("Optimization mode disabled - Standard monitoring active");
        }
        
        public void SetLowPowerMode(bool enabled)
        {
            lowPowerMode = enabled;
            
            if (enabled)
            {
                // Increase update intervals to save power
                fpsUpdateInterval *= 2f;
                ramUpdateInterval *= 2f;
                cpuUpdateInterval *= 2f;
                temperatureUpdateInterval *= 2f;
                Debug.Log("Low power mode enabled");
            }
            else
            {
                // Restore normal intervals
                ApplyDeviceSpecificSettings();
                Debug.Log("Low power mode disabled");
            }
        }
        
        private IEnumerator MonitorFPS()
        {
            while (isMonitoring)
            {
                yield return new WaitForSeconds(fpsUpdateInterval);
                
                float deltaTime = Time.unscaledDeltaTime;
                float fps = deltaTime > 0 ? 1f / deltaTime : 0f;
                
                // Store in buffer for smoothing
                fpsBuffer[fpsBufferIndex] = fps;
                fpsBufferIndex = (fpsBufferIndex + 1) % fpsBuffer.Length;
                
                // Calculate smoothed FPS
                float totalFPS = 0f;
                for (int i = 0; i < fpsBuffer.Length; i++)
                {
                    totalFPS += fpsBuffer[i];
                }
                
                currentFPS = totalFPS / fpsBuffer.Length;
                OnFPSUpdate?.Invoke(currentFPS);
            }
        }
        
        private IEnumerator MonitorRAMUsage()
        {
            while (isMonitoring)
            {
                yield return new WaitForSeconds(ramUpdateInterval);
                
                if (androidBridge != null)
                {
                    currentRAMUsage = androidBridge.GetRAMUsage();
                    OnRAMUsageUpdate?.Invoke(currentRAMUsage);
                }
            }
        }
        
        private IEnumerator MonitorCPUUsage()
        {
            while (isMonitoring)
            {
                yield return new WaitForSeconds(cpuUpdateInterval);
                
                // Mock CPU usage - would require native implementation
                currentCPUUsage = UnityEngine.Random.Range(30f, 80f);
                OnCPUUsageUpdate?.Invoke(currentCPUUsage);
            }
        }
        
        private IEnumerator MonitorTemperature()
        {
            while (isMonitoring)
            {
                yield return new WaitForSeconds(temperatureUpdateInterval);
                
                if (androidBridge != null)
                {
                    currentTemperature = androidBridge.GetDeviceTemperature();
                    OnTemperatureUpdate?.Invoke(currentTemperature);
                }
            }
        }
        
        private IEnumerator MonitorStorage()
        {
            while (isMonitoring)
            {
                yield return new WaitForSeconds(storageUpdateInterval);
                
                if (androidBridge != null)
                {
                    currentStorageUsage = androidBridge.GetAvailableStorage();
                    OnStorageUpdate?.Invoke(currentStorageUsage);
                }
            }
        }
        
        private void UpdateAdditionalMetrics()
        {
            if (androidBridge == null) return;
            
            currentRefreshRate = androidBridge.GetRefreshRate();
            bluetoothEnabled = androidBridge.IsBluetoothEnabled();
            brightnessLevel = androidBridge.GetBrightnessLevel();
        }
        
        public SystemMetrics GetCurrentMetrics()
        {
            var deviceInfo = deviceProfiler?.GetCurrentDevice();
            
            return new SystemMetrics
            {
                fps = currentFPS,
                ramUsage = currentRAMUsage,
                cpuUsage = currentCPUUsage,
                gpuUsage = currentGPUUsage,
                temperature = currentTemperature,
                storageUsage = currentStorageUsage,
                refreshRate = currentRefreshRate,
                bluetoothEnabled = bluetoothEnabled,
                brightnessLevel = brightnessLevel,
                deviceName = deviceInfo?.GetFullDeviceName() ?? "Unknown",
                timestamp = DateTime.Now
            };
        }
        
        public void TriggerMetricsUpdate()
        {
            var metrics = GetCurrentMetrics();
            OnMetricsUpdate?.Invoke(metrics);
        }
        
        private void OnDestroy()
        {
            StopMonitoring();
        }
    }
    
    [System.Serializable]
    public class SystemMetrics
    {
        public float fps;
        public float ramUsage;
        public float cpuUsage;
        public float gpuUsage;
        public float temperature;
        public long storageUsage;
        public int refreshRate;
        public bool bluetoothEnabled;
        public float brightnessLevel;
        public string deviceName;
        public DateTime timestamp;
        
        public string GetFormattedReport()
        {
            var report = $"System Metrics Report - {deviceName}\n";
            report += $"Timestamp: {timestamp:yyyy-MM-dd HH:mm:ss}\n";
            report += $"FPS: {fps:F1}\n";
            report += $"RAM Usage: {ramUsage:F1}%\n";
            report += $"CPU Usage: {cpuUsage:F1}%\n";
            report += $"GPU Usage: {gpuUsage:F1}%\n";
            report += $"Temperature: {temperature:F1}°C\n";
            report += $"Storage Available: {storageUsage}GB\n";
            report += $"Refresh Rate: {refreshRate}Hz\n";
            report += $"Bluetooth: {(bluetoothEnabled ? "On" : "Off")}\n";
            report += $"Brightness: {(brightnessLevel * 100):F0}%\n";
            return report;
        }
    }
}