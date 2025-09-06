using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Swiftboost.Core;
using Swiftboost.Game;

namespace Swiftboost.Optimization
{
    public class OptimizationHandler : MonoBehaviour
    {
        [Header("Optimization Settings")]
        [SerializeField] private bool optimizationActive = false;
        [SerializeField] private OptimizationMode currentMode = OptimizationMode.Balanced;
        [SerializeField] private float aggressiveness = 0.7f; // 0-1 range
        
        [Header("Optimization Controllers")]
        [SerializeField] private DRSController drsController;
        [SerializeField] private LODManager lodManager;
        [SerializeField] private ThermalController thermalController;
        
        [Header("Device Optimization")]
        [SerializeField] private bool deviceSpecificOptimization = true;
        [SerializeField] private float deviceOptimizationMultiplier = 1.0f;
        
        [Header("Performance Thresholds")]
        [SerializeField] private float targetFPS = 60f;
        [SerializeField] private float minAcceptableFPS = 30f;
        [SerializeField] private float maxAcceptableTemperature = 45f;
        [SerializeField] private float maxAcceptableRAMUsage = 85f;
        
        // References
        private DeviceProfiler deviceProfiler;
        private AndroidBridge androidBridge;
        private DeviceProfiler.DeviceInfo currentDevice;
        
        // Optimization state
        private Dictionary<string, float> optimizationHistory;
        private Coroutine optimizationCoroutine;
        private DateTime lastOptimizationTime;
        
        // Events
        public event Action<OptimizationMode> OnModeChanged;
        public event Action<string, float> OnOptimizationApplied;
        public event Action<bool> OnOptimizationToggled;
        
        public bool IsOptimizationActive => optimizationActive;
        public OptimizationMode CurrentMode => currentMode;
        public float Aggressiveness => aggressiveness;
        
        private void Start()
        {
            InitializeOptimizationHandler();
        }
        
        private void InitializeOptimizationHandler()
        {
            // Get references
            deviceProfiler = SystemManager.Instance?.DeviceProfiler;
            androidBridge = SystemManager.Instance?.AndroidBridge;
            
            if (deviceProfiler == null || androidBridge == null)
            {
                Debug.LogError("OptimizationHandler: Required components not found!");
                return;
            }
            
            // Initialize optimization history
            optimizationHistory = new Dictionary<string, float>();
            
            // Get device information
            currentDevice = deviceProfiler.GetCurrentDevice();
            
            // Initialize sub-controllers
            InitializeControllers();
            
            // Apply device-specific settings
            ApplyDeviceSpecificSettings();
            
            Debug.Log("OptimizationHandler initialized");
        }
        
        private void InitializeControllers()
        {
            // Initialize DRS Controller
            if (drsController == null)
                drsController = GetComponent<DRSController>();
            
            if (drsController == null)
            {
                drsController = gameObject.AddComponent<DRSController>();
            }
            
            // Initialize LOD Manager
            if (lodManager == null)
                lodManager = GetComponent<LODManager>();
            
            if (lodManager == null)
            {
                lodManager = gameObject.AddComponent<LODManager>();
            }
            
            // Initialize Thermal Controller
            if (thermalController == null)
                thermalController = GetComponent<ThermalController>();
            
            if (thermalController == null)
            {
                thermalController = gameObject.AddComponent<ThermalController>();
            }
        }
        
        private void ApplyDeviceSpecificSettings()
        {
            if (currentDevice == null) return;
            
            string brand = currentDevice.brand.ToLower();
            
            // Apply brand-specific optimization settings
            switch (brand)
            {
                case "samsung":
                    deviceOptimizationMultiplier = 1.3f;
                    maxAcceptableTemperature = 42f;
                    aggressiveness = 0.85f;
                    break;
                case "oneplus":
                    deviceOptimizationMultiplier = 1.2f;
                    maxAcceptableTemperature = 45f;
                    aggressiveness = 0.90f;
                    break;
                case "google":
                    deviceOptimizationMultiplier = 0.9f;
                    maxAcceptableTemperature = 40f;
                    aggressiveness = 0.75f;
                    break;
                case "xiaomi":
                    deviceOptimizationMultiplier = 1.1f;
                    maxAcceptableTemperature = 44f;
                    aggressiveness = 0.80f;
                    break;
                default:
                    deviceOptimizationMultiplier = 1.0f;
                    break;
            }
            
            Debug.Log($"Applied {brand} optimization settings - Multiplier: {deviceOptimizationMultiplier:F2}, Aggressiveness: {aggressiveness:F2}");
        }
        
        public void InitializeForDevice(DeviceProfiler.DeviceInfo deviceInfo)
        {
            currentDevice = deviceInfo;
            ApplyDeviceSpecificSettings();
            
            // Configure controllers for this device
            drsController?.InitializeForDevice(deviceInfo);
            lodManager?.InitializeForDevice(deviceInfo);
            thermalController?.InitializeForDevice(deviceInfo);
        }
        
        public void StartOptimization()
        {
            if (optimizationActive)
            {
                Debug.LogWarning("Optimization already active");
                return;
            }
            
            Debug.Log($"Starting optimization - Mode: {currentMode}, Aggressiveness: {aggressiveness:F2}");
            
            optimizationActive = true;
            lastOptimizationTime = DateTime.Now;
            
            // Start optimization coroutine
            optimizationCoroutine = StartCoroutine(OptimizationRoutine());
            
            // Start sub-controllers
            drsController?.StartOptimization();
            lodManager?.StartOptimization();
            thermalController?.StartOptimization();
            
            OnOptimizationToggled?.Invoke(true);
            
            Debug.Log("Optimization started successfully");
        }
        
        public void StopOptimization()
        {
            if (!optimizationActive)
            {
                Debug.LogWarning("Optimization not active");
                return;
            }
            
            Debug.Log("Stopping optimization");
            
            optimizationActive = false;
            
            // Stop optimization coroutine
            if (optimizationCoroutine != null)
            {
                StopCoroutine(optimizationCoroutine);
                optimizationCoroutine = null;
            }
            
            // Stop sub-controllers
            drsController?.StopOptimization();
            lodManager?.StopOptimization();
            thermalController?.StopOptimization();
            
            OnOptimizationToggled?.Invoke(false);
            
            Debug.Log("Optimization stopped");
        }
        
        public void SetPerformanceMode(OptimizationMode mode)
        {
            if (currentMode == mode) return;
            
            Debug.Log($"Changing optimization mode from {currentMode} to {mode}");
            
            currentMode = mode;
            
            // Apply mode-specific settings
            switch (mode)
            {
                case OptimizationMode.BatterySaver:
                    targetFPS = 30f;
                    aggressiveness = 0.3f;
                    break;
                case OptimizationMode.Balanced:
                    targetFPS = 60f;
                    aggressiveness = 0.7f;
                    break;
                case OptimizationMode.HighPerformance:
                    targetFPS = 120f;
                    aggressiveness = 1.0f;
                    break;
            }
            
            // Apply device-specific multiplier
            aggressiveness *= deviceOptimizationMultiplier;
            aggressiveness = Mathf.Clamp01(aggressiveness);
            
            // Update controllers
            UpdateControllersForMode(mode);
            
            OnModeChanged?.Invoke(mode);
            
            Debug.Log($"Mode changed successfully - Target FPS: {targetFPS}, Aggressiveness: {aggressiveness:F2}");
        }
        
        private void UpdateControllersForMode(OptimizationMode mode)
        {
            // Update DRS Controller
            drsController?.SetMode(mode);
            
            // Update LOD Manager
            lodManager?.SetMode(mode);
            
            // Update Thermal Controller
            thermalController?.SetMode(mode);
        }
        
        public void SetAggressiveness(float newAggressiveness)
        {
            aggressiveness = Mathf.Clamp01(newAggressiveness);
            
            // Apply device multiplier
            float effectiveAggressiveness = aggressiveness * deviceOptimizationMultiplier;
            effectiveAggressiveness = Mathf.Clamp01(effectiveAggressiveness);
            
            // Update controllers
            drsController?.SetAggressiveness(effectiveAggressiveness);
            lodManager?.SetAggressiveness(effectiveAggressiveness);
            thermalController?.SetAggressiveness(effectiveAggressiveness);
            
            Debug.Log($"Aggressiveness set to {aggressiveness:F2} (effective: {effectiveAggressiveness:F2})");
        }
        
        private IEnumerator OptimizationRoutine()
        {
            while (optimizationActive)
            {
                yield return new WaitForSeconds(1f);
                
                PerformOptimizationCycle();
            }
        }
        
        private void PerformOptimizationCycle()
        {
            if (androidBridge == null) return;
            
            // Get current performance metrics
            float currentFPS = Time.frameCount > 0 ? 1f / Time.unscaledDeltaTime : 0f;
            float currentRAM = androidBridge.GetRAMUsage();
            float currentTemp = androidBridge.GetDeviceTemperature();
            
            // Determine if optimization is needed
            bool optimizationNeeded = false;
            List<string> optimizationReasons = new List<string>();
            
            if (currentFPS < targetFPS * 0.9f)
            {
                optimizationNeeded = true;
                optimizationReasons.Add($"Low FPS: {currentFPS:F1} < {targetFPS * 0.9f:F1}");
            }
            
            if (currentRAM > maxAcceptableRAMUsage)
            {
                optimizationNeeded = true;
                optimizationReasons.Add($"High RAM: {currentRAM:F1}% > {maxAcceptableRAMUsage:F1}%");
            }
            
            if (currentTemp > maxAcceptableTemperature)
            {
                optimizationNeeded = true;
                optimizationReasons.Add($"High Temp: {currentTemp:F1}°C > {maxAcceptableTemperature:F1}°C");
            }
            
            if (optimizationNeeded)
            {
                ApplyOptimizations(optimizationReasons);
            }
            else
            {
                // Check if we can reduce optimization level
                ConsiderOptimizationReduction(currentFPS, currentRAM, currentTemp);
            }
            
            // Update optimization history
            UpdateOptimizationHistory(currentFPS, currentRAM, currentTemp);
        }
        
        private void ApplyOptimizations(List<string> reasons)
        {
            float optimizationStrength = CalculateOptimizationStrength(reasons);
            
            Debug.Log($"Applying optimizations (strength: {optimizationStrength:F2}): {string.Join(", ", reasons)}");
            
            // Apply optimizations through controllers
            if (reasons.Exists(r => r.Contains("Low FPS")))
            {
                drsController?.ApplyOptimization(optimizationStrength);
                lodManager?.ApplyOptimization(optimizationStrength);
            }
            
            if (reasons.Exists(r => r.Contains("High RAM")))
            {
                // Apply memory optimization
                ApplyMemoryOptimization(optimizationStrength);
            }
            
            if (reasons.Exists(r => r.Contains("High Temp")))
            {
                thermalController?.ApplyOptimization(optimizationStrength);
            }
            
            // Record optimization event
            OnOptimizationApplied?.Invoke(string.Join(", ", reasons), optimizationStrength);
            
            lastOptimizationTime = DateTime.Now;
        }
        
        private float CalculateOptimizationStrength(List<string> reasons)
        {
            float baseStrength = aggressiveness;
            
            // Increase strength based on number of issues
            float reasonMultiplier = 1f + (reasons.Count - 1) * 0.2f;
            
            // Apply device-specific multiplier
            float deviceMultiplier = deviceOptimizationMultiplier;
            
            float finalStrength = baseStrength * reasonMultiplier * deviceMultiplier;
            return Mathf.Clamp01(finalStrength);
        }
        
        private void ConsiderOptimizationReduction(float fps, float ram, float temp)
        {
            // Only consider reducing if performance is good and it's been a while since last optimization
            if (fps > targetFPS * 1.1f && ram < maxAcceptableRAMUsage * 0.8f && 
                temp < maxAcceptableTemperature * 0.8f &&
                (DateTime.Now - lastOptimizationTime).TotalSeconds > 10)
            {
                float reductionStrength = 0.1f; // Conservative reduction
                
                drsController?.ReduceOptimization(reductionStrength);
                lodManager?.ReduceOptimization(reductionStrength);
                
                Debug.Log($"Reducing optimization level - Performance is good (FPS: {fps:F1}, RAM: {ram:F1}%, Temp: {temp:F1}°C)");
            }
        }
        
        private void ApplyMemoryOptimization(float strength)
        {
            // Clear system caches
            androidBridge?.ClearBackgroundCache();
            
            // Trigger garbage collection
            System.GC.Collect();
            
            // Apply LOD optimizations for memory
            lodManager?.ApplyMemoryOptimization(strength);
            
            Debug.Log($"Applied memory optimization (strength: {strength:F2})");
        }
        
        private void UpdateOptimizationHistory(float fps, float ram, float temp)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            
            if (optimizationHistory.ContainsKey("fps"))
                optimizationHistory["fps"] = fps;
            else
                optimizationHistory.Add("fps", fps);
            
            if (optimizationHistory.ContainsKey("ram"))
                optimizationHistory["ram"] = ram;
            else
                optimizationHistory.Add("ram", ram);
            
            if (optimizationHistory.ContainsKey("temp"))
                optimizationHistory["temp"] = temp;
            else
                optimizationHistory.Add("temp", temp);
        }
        
        public Dictionary<string, float> GetOptimizationHistory()
        {
            return new Dictionary<string, float>(optimizationHistory);
        }
        
        public string GetOptimizationStatus()
        {
            var status = $"Optimization Status:\n";
            status += $"Active: {optimizationActive}\n";
            status += $"Mode: {currentMode}\n";
            status += $"Aggressiveness: {aggressiveness:F2}\n";
            status += $"Device Multiplier: {deviceOptimizationMultiplier:F2}\n";
            status += $"Target FPS: {targetFPS}\n";
            status += $"Max Temperature: {maxAcceptableTemperature:F1}°C\n";
            status += $"Max RAM Usage: {maxAcceptableRAMUsage:F1}%\n";
            
            if (currentDevice != null)
            {
                status += $"Device: {currentDevice.GetFullDeviceName()}\n";
            }
            
            return status;
        }
        
        public void EnableDeviceSpecificOptimization(bool enabled)
        {
            deviceSpecificOptimization = enabled;
            
            if (enabled)
            {
                ApplyDeviceSpecificSettings();
            }
            else
            {
                deviceOptimizationMultiplier = 1.0f;
            }
            
            Debug.Log($"Device-specific optimization: {(enabled ? "Enabled" : "Disabled")}");
        }
        
        private void OnDestroy()
        {
            if (optimizationActive)
            {
                StopOptimization();
            }
        }
    }
}