using UnityEngine;

namespace Swiftboost.Settings
{
    [System.Serializable]
    public class MonitoringSettings
    {
        [Header("Device Information")]
        public bool showDeviceInfo = true;
        public bool showBrandLogo = true;
        public bool enableDeviceSpecificOptimizations = true;
        
        [Header("Performance Monitoring")]
        public bool enableFPSMonitoring = true;
        public int fpsUpdateInterval = 1000; // milliseconds
        
        [Header("Hardware Monitoring")]
        public bool enableRAMMonitoring = true;
        public int ramUpdateInterval = 2000;
        
        public bool enableCPUMonitoring = true;
        public int cpuUpdateInterval = 1500;
        
        public bool enableGPUMonitoring = false; // More resource intensive
        public int gpuUpdateInterval = 3000;
        
        [Header("System Monitoring")]
        public bool enableTemperatureMonitoring = true;
        public int temperatureUpdateInterval = 5000;
        
        public bool enableStorageMonitoring = false; // Only when needed
        public int storageUpdateInterval = 10000;
        
        public bool enableBatteryMonitoring = true;
        public int batteryUpdateInterval = 15000;
        
        [Header("Network Monitoring")]
        public bool enableNetworkMonitoring = false;
        public int networkUpdateInterval = 5000;
        
        public bool enableBluetoothMonitoring = false;
        public int bluetoothUpdateInterval = 10000;
        
        [Header("Display Monitoring")]
        public bool enableDisplayMonitoring = true;
        public int displayUpdateInterval = 8000;
        
        [Header("Device-Specific Features")]
        public bool enableBrandSpecificUI = true;
        public bool enableModelSpecificTuning = true;
        public bool logDeviceMetrics = true;
        
        [Header("Optimization Features")]
        public bool enableDRS = true;
        public bool enableGeometricLOD = true;
        public bool enableImageLOD = true;
        public bool enableTextureLOD = true;
        public bool enableShaderLOD = true;
        public bool enableParticleLOD = true;
        public bool enableShadowLOD = true;
        public bool enablePostProcessLOD = false;
        public bool enableTextureDowngrade = false;
        public bool enableAdaptiveFPS = true;
        public bool enableThermalThrottling = true;
        public bool enableDeviceSpecificOptimization = true;
        public bool enableMemoryOptimization = true;
        
        [Header("Advanced Settings")]
        public bool enableAgressiveOptimization = false;
        public float optimizationAggressiveness = 0.7f; // 0.0 - 1.0
        public bool enableExperimentalFeatures = false;
        public bool enableDebugLogging = false;
        
        [Header("Performance Targets")]
        public int targetFPS = 60;
        public float maxTemperatureThreshold = 45f;
        public float maxRAMUsageThreshold = 85f;
        public float maxCPUUsageThreshold = 80f;
        
        [Header("Battery Optimization")]
        public bool enableBatteryOptimization = true;
        public bool reduceMonitoringOnLowBattery = true;
        public int lowBatteryThreshold = 20; // percentage
        
        [Header("Overlay Settings")]
        public bool showPerformanceOverlay = true;
        public bool allowOverlayDragging = true;
        public Vector2 overlayPosition = new Vector2(10, 10);
        public bool overlayMinimizedByDefault = false;
        public float overlayTransparency = 0.8f;
        
        public MonitoringSettings()
        {
            SetDefaultValues();
        }
        
        public void SetDefaultValues()
        {
            showDeviceInfo = true;
            showBrandLogo = true;
            enableDeviceSpecificOptimizations = true;
            
            enableFPSMonitoring = true;
            fpsUpdateInterval = 1000;
            
            enableRAMMonitoring = true;
            ramUpdateInterval = 2000;
            
            enableCPUMonitoring = true;
            cpuUpdateInterval = 1500;
            
            enableGPUMonitoring = false;
            gpuUpdateInterval = 3000;
            
            enableTemperatureMonitoring = true;
            temperatureUpdateInterval = 5000;
            
            enableStorageMonitoring = false;
            storageUpdateInterval = 10000;
            
            enableBatteryMonitoring = true;
            batteryUpdateInterval = 15000;
            
            enableDRS = true;
            enableGeometricLOD = true;
            enableImageLOD = true;
            enableTextureLOD = true;
            enableShaderLOD = true;
            enableParticleLOD = true;
            enableShadowLOD = true;
            enablePostProcessLOD = false;
            
            enableAdaptiveFPS = true;
            enableThermalThrottling = true;
            enableDeviceSpecificOptimization = true;
            enableMemoryOptimization = true;
            
            optimizationAggressiveness = 0.7f;
            targetFPS = 60;
            maxTemperatureThreshold = 45f;
            maxRAMUsageThreshold = 85f;
            maxCPUUsageThreshold = 80f;
            
            showPerformanceOverlay = true;
            allowOverlayDragging = true;
            overlayPosition = new Vector2(10, 10);
            overlayTransparency = 0.8f;
        }
        
        public void ApplyDeviceSpecificSettings(string deviceBrand, string deviceModel, int deviceRAM)
        {
            string brand = deviceBrand.ToLower();
            
            // Adjust monitoring intervals based on device brand
            switch (brand)
            {
                case "samsung":
                    fpsUpdateInterval = 800;
                    ramUpdateInterval = 1500;
                    cpuUpdateInterval = 1200;
                    temperatureUpdateInterval = 4000;
                    enableGPUMonitoring = deviceRAM >= 8192;
                    break;
                    
                case "oneplus":
                    fpsUpdateInterval = 1000;
                    ramUpdateInterval = 2000;
                    cpuUpdateInterval = 1500;
                    temperatureUpdateInterval = 5000;
                    enableGPUMonitoring = deviceRAM >= 8192;
                    break;
                    
                case "google":
                    fpsUpdateInterval = 1200;
                    ramUpdateInterval = 2500;
                    cpuUpdateInterval = 1800;
                    temperatureUpdateInterval = 6000;
                    enableGPUMonitoring = false; // Tensor chips
                    break;
                    
                case "xiaomi":
                    fpsUpdateInterval = 1000;
                    ramUpdateInterval = 2000;
                    cpuUpdateInterval = 1500;
                    temperatureUpdateInterval = 5000;
                    enableGPUMonitoring = deviceRAM >= 8192;
                    break;
                    
                default:
                    // Keep default values
                    break;
            }
            
            // Adjust based on device RAM
            if (deviceRAM < 4096) // Less than 4GB
            {
                // Reduce monitoring frequency for low-RAM devices
                fpsUpdateInterval *= 2;
                ramUpdateInterval *= 2;
                cpuUpdateInterval *= 2;
                temperatureUpdateInterval *= 2;
                
                enableGPUMonitoring = false;
                enableStorageMonitoring = false;
                enableNetworkMonitoring = false;
                enableBluetoothMonitoring = false;
                
                // More conservative optimization
                optimizationAggressiveness = 0.5f;
                targetFPS = 30;
                maxTemperatureThreshold = 40f;
            }
            else if (deviceRAM >= 12288) // 12GB or more
            {
                // Enable more features for high-RAM devices
                enableGPUMonitoring = true;
                enableStorageMonitoring = true;
                enableNetworkMonitoring = true;
                
                // More aggressive optimization possible
                optimizationAggressiveness = 0.8f;
                targetFPS = 120;
                maxTemperatureThreshold = 50f;
            }
            
            Debug.Log($"Applied device-specific monitoring settings for {deviceBrand} {deviceModel} ({deviceRAM}MB RAM)");
        }
        
        public void SetPerformancePreset(PerformancePreset preset)
        {
            switch (preset)
            {
                case PerformancePreset.BatterySaver:
                    targetFPS = 30;
                    optimizationAggressiveness = 0.3f;
                    
                    // Reduce monitoring frequency
                    fpsUpdateInterval *= 2;
                    ramUpdateInterval *= 2;
                    cpuUpdateInterval *= 2;
                    temperatureUpdateInterval *= 2;
                    
                    // Disable resource-intensive monitoring
                    enableGPUMonitoring = false;
                    enableStorageMonitoring = false;
                    enableNetworkMonitoring = false;
                    
                    // Enable aggressive optimizations
                    enableDRS = true;
                    enableGeometricLOD = true;
                    enableTextureLOD = true;
                    enableTextureDowngrade = true;
                    break;
                    
                case PerformancePreset.Balanced:
                    targetFPS = 60;
                    optimizationAggressiveness = 0.7f;
                    
                    // Standard monitoring settings
                    SetDefaultValues();
                    break;
                    
                case PerformancePreset.HighPerformance:
                    targetFPS = 120;
                    optimizationAggressiveness = 1.0f;
                    
                    // Increase monitoring frequency
                    fpsUpdateInterval = (int)(fpsUpdateInterval * 0.8f);
                    ramUpdateInterval = (int)(ramUpdateInterval * 0.8f);
                    cpuUpdateInterval = (int)(cpuUpdateInterval * 0.8f);
                    temperatureUpdateInterval = (int)(temperatureUpdateInterval * 0.8f);
                    
                    // Enable all monitoring
                    enableGPUMonitoring = true;
                    enableStorageMonitoring = true;
                    enableNetworkMonitoring = true;
                    
                    // Less aggressive optimization to maintain quality
                    enableTextureDowngrade = false;
                    enablePostProcessLOD = false;
                    break;
                    
                case PerformancePreset.Quality:
                    targetFPS = 60;
                    optimizationAggressiveness = 0.4f;
                    
                    // Disable quality-reducing optimizations
                    enableDRS = false;
                    enableTextureLOD = false;
                    enableTextureDowngrade = false;
                    enablePostProcessLOD = false;
                    
                    // Keep performance-maintaining optimizations
                    enableGeometricLOD = true;
                    enableParticleLOD = true;
                    enableThermalThrottling = true;
                    break;
            }
            
            Debug.Log($"Applied performance preset: {preset}");
        }
        
        public void SetOverlaySettings(bool show, Vector2 position, bool allowDrag, float transparency)
        {
            showPerformanceOverlay = show;
            overlayPosition = position;
            allowOverlayDragging = allowDrag;
            overlayTransparency = Mathf.Clamp01(transparency);
        }
        
        public void EnableAllOptimizations(bool enable)
        {
            enableDRS = enable;
            enableGeometricLOD = enable;
            enableImageLOD = enable;
            enableTextureLOD = enable;
            enableShaderLOD = enable;
            enableParticleLOD = enable;
            enableShadowLOD = enable;
            enableAdaptiveFPS = enable;
            enableThermalThrottling = enable;
            enableMemoryOptimization = enable;
        }
        
        public void EnableAllMonitoring(bool enable)
        {
            enableFPSMonitoring = enable;
            enableRAMMonitoring = enable;
            enableCPUMonitoring = enable;
            enableTemperatureMonitoring = enable;
            enableBatteryMonitoring = enable;
            
            // Optional monitoring
            if (enable)
            {
                enableGPUMonitoring = true;
                enableStorageMonitoring = true;
                enableNetworkMonitoring = true;
                enableBluetoothMonitoring = true;
                enableDisplayMonitoring = true;
            }
            else
            {
                enableGPUMonitoring = false;
                enableStorageMonitoring = false;
                enableNetworkMonitoring = false;
                enableBluetoothMonitoring = false;
                enableDisplayMonitoring = false;
            }
        }
        
        public bool ValidateSettings()
        {
            // Ensure intervals are reasonable
            fpsUpdateInterval = Mathf.Clamp(fpsUpdateInterval, 100, 10000);
            ramUpdateInterval = Mathf.Clamp(ramUpdateInterval, 500, 30000);
            cpuUpdateInterval = Mathf.Clamp(cpuUpdateInterval, 500, 30000);
            temperatureUpdateInterval = Mathf.Clamp(temperatureUpdateInterval, 1000, 60000);
            
            // Ensure thresholds are reasonable
            optimizationAggressiveness = Mathf.Clamp01(optimizationAggressiveness);
            targetFPS = Mathf.Clamp(targetFPS, 15, 240);
            maxTemperatureThreshold = Mathf.Clamp(maxTemperatureThreshold, 30f, 70f);
            maxRAMUsageThreshold = Mathf.Clamp(maxRAMUsageThreshold, 50f, 95f);
            maxCPUUsageThreshold = Mathf.Clamp(maxCPUUsageThreshold, 50f, 95f);
            
            // Ensure overlay settings are valid
            overlayTransparency = Mathf.Clamp01(overlayTransparency);
            
            return true;
        }
        
        public string GetSettingsDescription()
        {
            var desc = "Monitoring Settings:\n";
            desc += $"Device Info: {(showDeviceInfo ? "Enabled" : "Disabled")}\n";
            desc += $"FPS Monitoring: {(enableFPSMonitoring ? $"Enabled ({fpsUpdateInterval}ms)" : "Disabled")}\n";
            desc += $"RAM Monitoring: {(enableRAMMonitoring ? $"Enabled ({ramUpdateInterval}ms)" : "Disabled")}\n";
            desc += $"CPU Monitoring: {(enableCPUMonitoring ? $"Enabled ({cpuUpdateInterval}ms)" : "Disabled")}\n";
            desc += $"Temperature: {(enableTemperatureMonitoring ? $"Enabled ({temperatureUpdateInterval}ms)" : "Disabled")}\n";
            desc += $"Target FPS: {targetFPS}\n";
            desc += $"Optimization Aggressiveness: {optimizationAggressiveness:F1}\n";
            desc += $"Max Temperature: {maxTemperatureThreshold:F1}°C\n";
            desc += $"Overlay: {(showPerformanceOverlay ? "Enabled" : "Disabled")}\n";
            return desc;
        }
        
        public enum PerformancePreset
        {
            BatterySaver,
            Balanced,
            HighPerformance,
            Quality
        }
    }
}