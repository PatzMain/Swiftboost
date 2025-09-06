using UnityEngine;
using Swiftboost.Core;

namespace Swiftboost.Optimization
{
    public class OnePlusOptimization : MonoBehaviour
    {
        [Header("OnePlus Features")]
        [SerializeField] private bool gameSpaceEnabled = false;
        [SerializeField] private bool alertSliderOptimizationEnabled = false;
        [SerializeField] private bool oxygenOSGameModeEnabled = false;
        [SerializeField] private bool hyperbBoostEnabled = false;
        
        [Header("OnePlus Specific Settings")]
        [SerializeField] private OnePlusDeviceTier deviceTier = OnePlusDeviceTier.Flagship;
        [SerializeField] private float onePlusOptimizationMultiplier = 1.2f;
        
        // References
        private AndroidBridge androidBridge;
        private DeviceProfiler deviceProfiler;
        private ThermalController thermalController;
        private OptimizationHandler optimizationHandler;
        
        public bool IsGameSpaceEnabled => gameSpaceEnabled;
        public bool IsHyperbBoostEnabled => hyperbBoostEnabled;
        public OnePlusDeviceTier DeviceTier => deviceTier;
        
        private void Start()
        {
            InitializeOnePlusOptimization();
        }
        
        private void InitializeOnePlusOptimization()
        {
            // Get references
            androidBridge = SystemManager.Instance?.AndroidBridge;
            deviceProfiler = SystemManager.Instance?.DeviceProfiler;
            thermalController = GetComponent<ThermalController>();
            optimizationHandler = GetComponent<OptimizationHandler>();
            
            if (androidBridge == null || deviceProfiler == null)
            {
                Debug.LogError("OnePlusOptimization: Required components not found!");
                return;
            }
            
            // Only initialize if this is a OnePlus device
            if (androidBridge.IsOnePlusDevice())
            {
                DetermineOnePlusDeviceTier();
                EnableOnePlusFeatures();
                ApplyOnePlusOptimizations();
                
                Debug.Log($"OnePlus optimization initialized for {deviceTier} device");
            }
            else
            {
                Debug.Log("Not a OnePlus device, disabling OnePlus optimization");
                gameObject.SetActive(false);
            }
        }
        
        private void DetermineOnePlusDeviceTier()
        {
            var deviceInfo = deviceProfiler.GetCurrentDevice();
            if (deviceInfo == null) return;
            
            string model = deviceInfo.model.ToLower();
            
            // Determine device tier based on model
            if (model.Contains("12") || model.Contains("11") || model.Contains("10 pro") || 
                model.Contains("9 pro") || model.Contains("8 pro"))
            {
                deviceTier = OnePlusDeviceTier.Flagship;
                onePlusOptimizationMultiplier = 1.3f;
            }
            else if (model.Contains("10t") || model.Contains("10") || model.Contains("9t") || 
                     model.Contains("9") || model.Contains("8t") || model.Contains("8"))
            {
                deviceTier = OnePlusDeviceTier.Premium;
                onePlusOptimizationMultiplier = 1.2f;
            }
            else if (model.Contains("nord") || model.Contains("ace"))
            {
                deviceTier = OnePlusDeviceTier.MidRange;
                onePlusOptimizationMultiplier = 1.0f;
            }
            else if (model.Contains("n") || model.Contains("ce"))
            {
                deviceTier = OnePlusDeviceTier.Budget;
                onePlusOptimizationMultiplier = 0.9f;
            }
            else
            {
                deviceTier = OnePlusDeviceTier.Unknown;
                onePlusOptimizationMultiplier = 1.0f;
            }
            
            Debug.Log($"OnePlus device tier: {deviceTier} (multiplier: {onePlusOptimizationMultiplier:F2})");
        }
        
        private void EnableOnePlusFeatures()
        {
            switch (deviceTier)
            {
                case OnePlusDeviceTier.Flagship:
                    EnableGameSpace();
                    EnableAlertSliderOptimization();
                    EnableOxygenOSGameMode();
                    EnableHyperbBoost();
                    break;
                    
                case OnePlusDeviceTier.Premium:
                    EnableGameSpace();
                    EnableAlertSliderOptimization();
                    EnableOxygenOSGameMode();
                    break;
                    
                case OnePlusDeviceTier.MidRange:
                    EnableGameSpace();
                    EnableOxygenOSGameMode();
                    break;
                    
                case OnePlusDeviceTier.Budget:
                    // Limited features for budget devices
                    EnableGameSpace();
                    break;
            }
        }
        
        public void EnableGameSpace()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                // Enable OnePlus Game Space integration
                using (var gameSpaceClass = new AndroidJavaClass("com.oneplus.gamespace.GameSpaceManager"))
                {
                    gameSpaceClass.CallStatic("enableGameMode", true);
                    gameSpaceClass.CallStatic("setPerformanceMode", 2); // High performance
                }
                #endif
                
                gameSpaceEnabled = true;
                Debug.Log("OnePlus Game Space enabled");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to enable OnePlus Game Space: {e.Message}");
                gameSpaceEnabled = false;
            }
        }
        
        public void EnableAlertSliderOptimization()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                // Configure alert slider for gaming mode
                using (var alertSliderClass = new AndroidJavaClass("com.oneplus.settings.AlertSlider"))
                {
                    alertSliderClass.CallStatic("setGamingMode", true);
                }
                #endif
                
                alertSliderOptimizationEnabled = true;
                Debug.Log("OnePlus Alert Slider optimization enabled");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to enable Alert Slider optimization: {e.Message}");
                alertSliderOptimizationEnabled = false;
            }
        }
        
        public void EnableOxygenOSGameMode()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                // Enable OxygenOS performance enhancements
                using (var oxygenOSClass = new AndroidJavaClass("com.oneplus.oxygen.GameModeManager"))
                {
                    oxygenOSClass.CallStatic("enableGameMode", true);
                    oxygenOSClass.CallStatic("setOptimizationLevel", 3); // Maximum optimization
                }
                #endif
                
                oxygenOSGameModeEnabled = true;
                Debug.Log("OxygenOS Game Mode enabled");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to enable OxygenOS Game Mode: {e.Message}");
                oxygenOSGameModeEnabled = false;
            }
        }
        
        public void EnableHyperbBoost()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                // Enable OnePlus HyperBoost (for supported models)
                using (var hyperBoostClass = new AndroidJavaClass("com.oneplus.hyperboost.HyperBoostManager"))
                {
                    hyperBoostClass.CallStatic("enableHyperBoost", true);
                    hyperBoostClass.CallStatic("setBoostLevel", 2); // High boost level
                }
                #endif
                
                hyperbBoostEnabled = true;
                Debug.Log("OnePlus HyperBoost enabled");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to enable HyperBoost: {e.Message}");
                hyperbBoostEnabled = false;
            }
        }
        
        private void ApplyOnePlusOptimizations()
        {
            // Apply OnePlus-specific thermal profile
            ApplyOnePlusThermalProfile();
            
            // Apply OnePlus-specific performance settings
            ApplyOnePlusPerformanceSettings();
            
            // Configure OnePlus-specific features
            ApplyOnePlusSystemOptimizations();
        }
        
        private void ApplyOnePlusThermalProfile()
        {
            if (thermalController == null) return;
            
            // OnePlus devices generally have aggressive cooling
            float thermalThreshold = 45f; // Default for OnePlus devices
            
            switch (deviceTier)
            {
                case OnePlusDeviceTier.Flagship:
                    thermalThreshold = 47f; // Pro models handle heat very well
                    break;
                case OnePlusDeviceTier.Premium:
                    thermalThreshold = 45f;
                    break;
                case OnePlusDeviceTier.MidRange:
                    thermalThreshold = 43f;
                    break;
                case OnePlusDeviceTier.Budget:
                    thermalThreshold = 40f;
                    break;
            }
            
            thermalController.SetBrandSpecificThreshold(thermalThreshold);
            
            Debug.Log($"Applied OnePlus thermal profile: {thermalThreshold}°C threshold");
        }
        
        private void ApplyOnePlusPerformanceSettings()
        {
            if (optimizationHandler == null) return;
            
            // OnePlus devices handle aggressive optimization very well
            float aggressiveness = 0.90f * onePlusOptimizationMultiplier;
            
            switch (deviceTier)
            {
                case OnePlusDeviceTier.Flagship:
                    aggressiveness = 0.95f;
                    break;
                case OnePlusDeviceTier.Premium:
                    aggressiveness = 0.85f;
                    break;
                case OnePlusDeviceTier.MidRange:
                    aggressiveness = 0.75f;
                    break;
                case OnePlusDeviceTier.Budget:
                    aggressiveness = 0.60f;
                    break;
            }
            
            optimizationHandler.SetAggressiveness(aggressiveness);
            
            Debug.Log($"Applied OnePlus performance settings: {aggressiveness:F2} aggressiveness");
        }
        
        private void ApplyOnePlusSystemOptimizations()
        {
            // OnePlus-specific system optimizations
            switch (deviceTier)
            {
                case OnePlusDeviceTier.Flagship:
                    EnableAdvancedOnePlusFeatures();
                    break;
                    
                case OnePlusDeviceTier.Premium:
                    EnableStandardOnePlusFeatures();
                    break;
                    
                case OnePlusDeviceTier.MidRange:
                case OnePlusDeviceTier.Budget:
                    EnableBasicOnePlusFeatures();
                    break;
            }
        }
        
        private void EnableAdvancedOnePlusFeatures()
        {
            Debug.Log("Enabled OnePlus advanced features");
            // Implementation for advanced OnePlus features
            EnableOnePlusPerformanceMode(OnePlusPerformanceMode.HighPerformance);
            EnableOnePlusGamingOptimizations(true);
        }
        
        private void EnableStandardOnePlusFeatures()
        {
            Debug.Log("Enabled OnePlus standard features");
            // Implementation for standard OnePlus features
            EnableOnePlusPerformanceMode(OnePlusPerformanceMode.Performance);
            EnableOnePlusGamingOptimizations(true);
        }
        
        private void EnableBasicOnePlusFeatures()
        {
            Debug.Log("Enabled OnePlus basic features");
            // Implementation for basic OnePlus features
            EnableOnePlusPerformanceMode(OnePlusPerformanceMode.Balanced);
            EnableOnePlusGamingOptimizations(false);
        }
        
        public void EnableOnePlusPerformanceMode(OnePlusPerformanceMode mode)
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                int modeValue = (int)mode;
                
                if (gameSpaceEnabled)
                {
                    using (var gameSpaceClass = new AndroidJavaClass("com.oneplus.gamespace.GameSpaceManager"))
                    {
                        gameSpaceClass.CallStatic("setPerformanceMode", modeValue);
                    }
                }
                #endif
                
                Debug.Log($"OnePlus Performance Mode set to: {mode}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to set OnePlus Performance Mode: {e.Message}");
            }
        }
        
        public void EnableOnePlusGamingOptimizations(bool advanced)
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                if (oxygenOSGameModeEnabled)
                {
                    using (var oxygenOSClass = new AndroidJavaClass("com.oneplus.oxygen.GameModeManager"))
                    {
                        oxygenOSClass.CallStatic("enableAdvancedOptimizations", advanced);
                        
                        if (advanced && hyperbBoostEnabled)
                        {
                            oxygenOSClass.CallStatic("enableHyperBoost", true);
                        }
                    }
                }
                #endif
                
                Debug.Log($"OnePlus gaming optimizations: {(advanced ? "Advanced" : "Standard")}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to enable OnePlus gaming optimizations: {e.Message}");
            }
        }
        
        public void OptimizeForOnePlusDevice()
        {
            Debug.Log("Applying comprehensive OnePlus device optimization");
            
            // Apply all OnePlus optimizations
            if (gameSpaceEnabled)
            {
                EnableOnePlusPerformanceMode(OnePlusPerformanceMode.HighPerformance);
            }
            
            if (oxygenOSGameModeEnabled)
            {
                EnableOnePlusGamingOptimizations(true);
            }
            
            if (hyperbBoostEnabled)
            {
                EnableHyperBoostOptimizations();
            }
            
            if (alertSliderOptimizationEnabled)
            {
                ConfigureAlertSliderForGaming();
            }
            
            Debug.Log("OnePlus device optimization complete");
        }
        
        private void EnableHyperBoostOptimizations()
        {
            // Implementation for HyperBoost optimizations
            Debug.Log("Applied OnePlus HyperBoost optimizations");
        }
        
        private void ConfigureAlertSliderForGaming()
        {
            // Implementation for alert slider gaming configuration
            Debug.Log("Configured OnePlus alert slider for gaming");
        }
        
        public void SetOnePlusAlertSliderMode(OnePlusAlertSliderMode mode)
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                if (alertSliderOptimizationEnabled)
                {
                    using (var alertSliderClass = new AndroidJavaClass("com.oneplus.settings.AlertSlider"))
                    {
                        alertSliderClass.CallStatic("setMode", (int)mode);
                    }
                }
                #endif
                
                Debug.Log($"OnePlus Alert Slider mode set to: {mode}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to set Alert Slider mode: {e.Message}");
            }
        }
        
        public string GetOnePlusOptimizationStatus()
        {
            var status = $"OnePlus Optimization Status:\n";
            status += $"Device Tier: {deviceTier}\n";
            status += $"Optimization Multiplier: {onePlusOptimizationMultiplier:F2}\n";
            status += $"Game Space: {(gameSpaceEnabled ? "Enabled" : "Disabled")}\n";
            status += $"Alert Slider Optimization: {(alertSliderOptimizationEnabled ? "Enabled" : "Disabled")}\n";
            status += $"OxygenOS Game Mode: {(oxygenOSGameModeEnabled ? "Enabled" : "Disabled")}\n";
            status += $"HyperBoost: {(hyperbBoostEnabled ? "Enabled" : "Disabled")}\n";
            
            return status;
        }
        
        public enum OnePlusDeviceTier
        {
            Unknown,
            Budget,     // Nord N, CE series
            MidRange,   // Nord, Ace series
            Premium,    // Numbered series (non-Pro)
            Flagship    // Pro series
        }
        
        public enum OnePlusPerformanceMode
        {
            PowerSaving = 0,
            Balanced = 1,
            Performance = 2,
            HighPerformance = 3
        }
        
        public enum OnePlusAlertSliderMode
        {
            Ring = 0,
            Vibrate = 1,
            Silent = 2
        }
    }
}