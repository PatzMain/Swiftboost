using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Swiftboost.Core;
using Swiftboost.Game;

namespace Swiftboost.Optimization
{
    public class ThermalController : MonoBehaviour
    {
        [Header("Thermal Thresholds")]
        [SerializeField] private float normalThreshold = 40f;
        [SerializeField] private float warningThreshold = 45f;
        [SerializeField] private float criticalThreshold = 50f;
        [SerializeField] private float emergencyThreshold = 55f;
        
        [Header("Thermal Management")]
        [SerializeField] private bool thermalThrottlingEnabled = true;
        [SerializeField] private float temperatureCheckInterval = 2f;
        [SerializeField] private float currentTemperature = 25f;
        [SerializeField] private ThermalState currentThermalState = ThermalState.Normal;
        
        [Header("Throttling Settings")]
        [SerializeField] private float throttleStrength = 0f; // 0-1, 0 = no throttling, 1 = maximum throttling
        [SerializeField] private float throttleRecoveryRate = 0.1f; // How fast to recover from throttling
        [SerializeField] private float throttleApplicationRate = 0.2f; // How fast to apply throttling
        
        [Header("Device Specific")]
        [SerializeField] private float deviceThermalMultiplier = 1.0f;
        [SerializeField] private bool useDeviceProfile = true;
        
        // State tracking
        private float[] temperatureHistory = new float[10];
        private int temperatureHistoryIndex = 0;
        private List<ThermalEvent> thermalEvents;
        private Coroutine thermalMonitoringCoroutine;
        private DeviceProfiler.DeviceInfo currentDevice;
        
        // References
        private AndroidBridge androidBridge;
        private OptimizationHandler optimizationHandler;
        private DRSController drsController;
        private LODManager lodManager;
        
        // Events
        public System.Action<ThermalState> OnThermalStateChanged;
        public System.Action<float> OnTemperatureChanged;
        public System.Action<ThermalEvent> OnThermalEventTriggered;
        
        public enum ThermalState
        {
            Normal,      // Temperature is fine
            Warm,        // Starting to get warm
            Warning,     // Getting hot, start throttling
            Critical,    // Very hot, aggressive throttling
            Emergency    // Dangerously hot, maximum throttling
        }
        
        public ThermalState CurrentThermalState => currentThermalState;
        public float CurrentTemperature => currentTemperature;
        public float ThrottleStrength => throttleStrength;
        public bool IsThrottling => throttleStrength > 0.01f;
        
        private void Start()
        {
            InitializeThermalController();
        }
        
        private void InitializeThermalController()
        {
            // Initialize collections
            thermalEvents = new List<ThermalEvent>();
            
            // Initialize temperature history
            for (int i = 0; i < temperatureHistory.Length; i++)
            {
                temperatureHistory[i] = 25f; // Start with room temperature
            }
            
            // Get references
            androidBridge = SystemManager.Instance?.AndroidBridge;
            optimizationHandler = GetComponent<OptimizationHandler>();
            drsController = GetComponent<DRSController>();
            lodManager = GetComponent<LODManager>();
            
            if (androidBridge == null)
            {
                Debug.LogError("ThermalController: AndroidBridge not found!");
                return;
            }
            
            Debug.Log("Thermal Controller initialized");
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
            
            // Apply brand-specific thermal settings
            switch (brand)
            {
                case "samsung":
                    if (currentDevice.model.Contains("S23") || currentDevice.model.Contains("S24"))
                    {
                        // Samsung flagships have good thermal management
                        normalThreshold = 38f;
                        warningThreshold = 42f;
                        criticalThreshold = 48f;
                        deviceThermalMultiplier = 1.2f;
                    }
                    else
                    {
                        normalThreshold = 36f;
                        warningThreshold = 40f;
                        criticalThreshold = 46f;
                        deviceThermalMultiplier = 1.0f;
                    }
                    break;
                    
                case "oneplus":
                    // OnePlus devices generally have aggressive cooling
                    normalThreshold = 40f;
                    warningThreshold = 45f;
                    criticalThreshold = 52f;
                    deviceThermalMultiplier = 1.3f;
                    break;
                    
                case "google":
                    // Tensor chips can get hot quickly
                    normalThreshold = 35f;
                    warningThreshold = 40f;
                    criticalThreshold = 45f;
                    deviceThermalMultiplier = 0.8f;
                    break;
                    
                case "xiaomi":
                    normalThreshold = 39f;
                    warningThreshold = 44f;
                    criticalThreshold = 50f;
                    deviceThermalMultiplier = 1.1f;
                    break;
                    
                default:
                    // Conservative settings for unknown devices
                    normalThreshold = 38f;
                    warningThreshold = 43f;
                    criticalThreshold = 48f;
                    deviceThermalMultiplier = 1.0f;
                    break;
            }
            
            emergencyThreshold = criticalThreshold + 7f;
            
            Debug.Log($"Applied thermal settings for {brand}: Normal={normalThreshold:F1}°C, Warning={warningThreshold:F1}°C, Critical={criticalThreshold:F1}°C");
        }
        
        public void SetMode(OptimizationMode mode)
        {
            switch (mode)
            {
                case OptimizationMode.BatterySaver:
                    // More aggressive thermal management for battery saving
                    temperatureCheckInterval = 1.5f;
                    throttleApplicationRate = 0.3f;
                    throttleRecoveryRate = 0.05f;
                    break;
                    
                case OptimizationMode.Balanced:
                    // Balanced thermal management
                    temperatureCheckInterval = 2f;
                    throttleApplicationRate = 0.2f;
                    throttleRecoveryRate = 0.1f;
                    break;
                    
                case OptimizationMode.HighPerformance:
                    // Less aggressive thermal management
                    temperatureCheckInterval = 3f;
                    throttleApplicationRate = 0.15f;
                    throttleRecoveryRate = 0.15f;
                    break;
            }
            
            Debug.Log($"Thermal mode set to {mode} - Check interval: {temperatureCheckInterval:F1}s");
        }
        
        public void SetAggressiveness(float aggressiveness)
        {
            // Adjust thermal response based on aggressiveness
            float deviceAdjustedAggressiveness = aggressiveness * deviceThermalMultiplier;
            
            throttleApplicationRate = Mathf.Lerp(0.1f, 0.4f, deviceAdjustedAggressiveness);
            throttleRecoveryRate = Mathf.Lerp(0.05f, 0.2f, 1f - deviceAdjustedAggressiveness);
            
            Debug.Log($"Thermal aggressiveness set to {aggressiveness:F2} (device adjusted: {deviceAdjustedAggressiveness:F2})");
        }
        
        public void StartOptimization()
        {
            if (thermalMonitoringCoroutine != null)
            {
                Debug.LogWarning("Thermal monitoring already running");
                return;
            }
            
            thermalMonitoringCoroutine = StartCoroutine(ThermalMonitoringRoutine());
            Debug.Log("Thermal monitoring started");
        }
        
        public void StopOptimization()
        {
            if (thermalMonitoringCoroutine != null)
            {
                StopCoroutine(thermalMonitoringCoroutine);
                thermalMonitoringCoroutine = null;
            }
            
            // Reset throttling
            SetThrottleStrength(0f);
            
            Debug.Log("Thermal monitoring stopped");
        }
        
        private IEnumerator ThermalMonitoringRoutine()
        {
            while (thermalThrottlingEnabled)
            {
                yield return new WaitForSeconds(temperatureCheckInterval);
                
                UpdateTemperature();
                EvaluateThermalState();
                ApplyThermalThrottling();
            }
        }
        
        private void UpdateTemperature()
        {
            if (androidBridge != null)
            {
                float newTemperature = androidBridge.GetDeviceTemperature();
                
                // Add to history
                temperatureHistory[temperatureHistoryIndex] = newTemperature;
                temperatureHistoryIndex = (temperatureHistoryIndex + 1) % temperatureHistory.Length;
                
                // Update current temperature (smoothed)
                currentTemperature = GetAverageTemperature();
                
                OnTemperatureChanged?.Invoke(currentTemperature);
            }
        }
        
        private float GetAverageTemperature()
        {
            float total = 0f;
            for (int i = 0; i < temperatureHistory.Length; i++)
            {
                total += temperatureHistory[i];
            }
            return total / temperatureHistory.Length;
        }
        
        private void EvaluateThermalState()
        {
            ThermalState newState = currentThermalState;
            
            if (currentTemperature >= emergencyThreshold)
            {
                newState = ThermalState.Emergency;
            }
            else if (currentTemperature >= criticalThreshold)
            {
                newState = ThermalState.Critical;
            }
            else if (currentTemperature >= warningThreshold)
            {
                newState = ThermalState.Warning;
            }
            else if (currentTemperature >= normalThreshold)
            {
                newState = ThermalState.Warm;
            }
            else
            {
                newState = ThermalState.Normal;
            }
            
            // Only change state if it's different
            if (newState != currentThermalState)
            {
                ThermalState previousState = currentThermalState;
                currentThermalState = newState;
                
                OnThermalStateChanged?.Invoke(currentThermalState);
                
                // Log thermal event
                var thermalEvent = new ThermalEvent
                {
                    timestamp = System.DateTime.Now,
                    temperature = currentTemperature,
                    previousState = previousState,
                    newState = currentThermalState,
                    actionTaken = GetThermalActionDescription(currentThermalState)
                };
                
                thermalEvents.Add(thermalEvent);
                OnThermalEventTriggered?.Invoke(thermalEvent);
                
                Debug.Log($"Thermal state changed: {previousState} -> {currentThermalState} ({currentTemperature:F1}°C)");
            }
        }
        
        private void ApplyThermalThrottling()
        {
            float targetThrottleStrength = CalculateTargetThrottleStrength();
            
            // Gradually adjust throttle strength
            if (targetThrottleStrength > throttleStrength)
            {
                // Apply throttling more quickly when heating up
                throttleStrength = Mathf.MoveTowards(throttleStrength, targetThrottleStrength, throttleApplicationRate * Time.deltaTime);
            }
            else
            {
                // Recover from throttling more slowly when cooling down
                throttleStrength = Mathf.MoveTowards(throttleStrength, targetThrottleStrength, throttleRecoveryRate * Time.deltaTime);
            }
            
            // Apply throttling to other systems
            if (throttleStrength > 0.01f)
            {
                ApplyThrottlingToSystems();
            }
        }
        
        private float CalculateTargetThrottleStrength()
        {
            switch (currentThermalState)
            {
                case ThermalState.Normal:
                    return 0f;
                    
                case ThermalState.Warm:
                    return 0.1f;
                    
                case ThermalState.Warning:
                    // Linear interpolation between warning and critical thresholds
                    float warningProgress = (currentTemperature - warningThreshold) / (criticalThreshold - warningThreshold);
                    return Mathf.Lerp(0.2f, 0.6f, warningProgress);
                    
                case ThermalState.Critical:
                    // Linear interpolation between critical and emergency thresholds
                    float criticalProgress = (currentTemperature - criticalThreshold) / (emergencyThreshold - criticalThreshold);
                    return Mathf.Lerp(0.6f, 0.9f, criticalProgress);
                    
                case ThermalState.Emergency:
                    return 1.0f;
                    
                default:
                    return 0f;
            }
        }
        
        private void ApplyThrottlingToSystems()
        {
            // Apply throttling to DRS
            if (drsController != null)
            {
                drsController.ApplyOptimization(throttleStrength);
            }
            
            // Apply throttling to LOD
            if (lodManager != null)
            {
                lodManager.ApplyOptimization(throttleStrength);
            }
            
            // Reduce target frame rate based on throttling
            if (optimizationHandler != null)
            {
                float throttledFPS = Mathf.Lerp(60f, 30f, throttleStrength);
                // This would need to be implemented in OptimizationHandler
            }
            
            // Apply CPU frequency scaling (would require native implementation)
            if (throttleStrength > 0.7f)
            {
                ApplyCPUThrottling();
            }
        }
        
        private void ApplyCPUThrottling()
        {
            // This would require native Android implementation
            // For now, we can reduce Unity's target frame rate and time scale
            
            if (throttleStrength > 0.9f)
            {
                Application.targetFrameRate = 20; // Emergency throttling
            }
            else if (throttleStrength > 0.7f)
            {
                Application.targetFrameRate = 30; // Heavy throttling
            }
            else if (throttleStrength > 0.4f)
            {
                Application.targetFrameRate = 45; // Moderate throttling
            }
            
            Debug.Log($"Applied CPU throttling - Target FPS: {Application.targetFrameRate}");
        }
        
        public void ApplyOptimization(float strength)
        {
            if (!thermalThrottlingEnabled) return;
            
            // Force apply thermal optimization
            SetThrottleStrength(strength);
            ApplyThrottlingToSystems();
            
            Debug.Log($"Applied thermal optimization (strength: {strength:F2})");
        }
        
        public void SetThrottleStrength(float strength)
        {
            throttleStrength = Mathf.Clamp01(strength);
        }
        
        public void SetThreshold(float temperature)
        {
            warningThreshold = temperature;
            criticalThreshold = temperature + 5f;
            emergencyThreshold = temperature + 10f;
            
            Debug.Log($"Thermal thresholds updated - Warning: {warningThreshold:F1}°C, Critical: {criticalThreshold:F1}°C");
        }
        
        public void SetBrandSpecificThreshold(float threshold)
        {
            warningThreshold = threshold;
            criticalThreshold = threshold + 5f;
            emergencyThreshold = threshold + 10f;
            
            Debug.Log($"Brand-specific thermal threshold set to {threshold:F1}°C");
        }
        
        private string GetThermalActionDescription(ThermalState state)
        {
            switch (state)
            {
                case ThermalState.Normal:
                    return "No throttling needed";
                case ThermalState.Warm:
                    return "Light performance monitoring";
                case ThermalState.Warning:
                    return "Applied moderate throttling";
                case ThermalState.Critical:
                    return "Applied aggressive throttling";
                case ThermalState.Emergency:
                    return "Applied maximum throttling";
                default:
                    return "Unknown action";
            }
        }
        
        public List<ThermalEvent> GetRecentThermalEvents(int count = 10)
        {
            if (thermalEvents.Count <= count)
                return new List<ThermalEvent>(thermalEvents);
            
            return thermalEvents.GetRange(thermalEvents.Count - count, count);
        }
        
        public string GetThermalStatus()
        {
            var status = $"Thermal Status:\n";
            status += $"Current Temperature: {currentTemperature:F1}°C\n";
            status += $"Thermal State: {currentThermalState}\n";
            status += $"Throttle Strength: {throttleStrength:F2}\n";
            status += $"Thresholds: Normal={normalThreshold:F1}, Warning={warningThreshold:F1}, Critical={criticalThreshold:F1}°C\n";
            status += $"Device Multiplier: {deviceThermalMultiplier:F2}\n";
            status += $"Total Events: {thermalEvents.Count}\n";
            
            if (currentDevice != null)
            {
                status += $"Device: {currentDevice.GetFullDeviceName()}\n";
            }
            
            return status;
        }
        
        public void SetThermalThrottlingEnabled(bool enabled)
        {
            thermalThrottlingEnabled = enabled;
            
            if (!enabled)
            {
                StopOptimization();
            }
            
            Debug.Log($"Thermal throttling {(enabled ? "enabled" : "disabled")}");
        }
        
        private void OnDestroy()
        {
            if (thermalMonitoringCoroutine != null)
            {
                StopOptimization();
            }
        }
    }
    
    [System.Serializable]
    public class ThermalEvent
    {
        public System.DateTime timestamp;
        public float temperature;
        public ThermalController.ThermalState previousState;
        public ThermalController.ThermalState newState;
        public string actionTaken;
        
        public string GetEventDescription()
        {
            return $"[{timestamp:HH:mm:ss}] {temperature:F1}°C - {previousState} → {newState}: {actionTaken}";
        }
    }
}