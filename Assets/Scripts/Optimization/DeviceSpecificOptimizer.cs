using System.Collections.Generic;
using UnityEngine;
using Swiftboost.Core;
using Swiftboost.Game;

namespace Swiftboost.Optimization
{
    public class DeviceSpecificOptimizer : MonoBehaviour
    {
        [Header("Device Profiles")]
        [SerializeField] private List<BrandProfile> brandProfiles;
        [SerializeField] private List<DeviceProfile> deviceProfiles;
        
        [Header("Current Device")]
        [SerializeField] private BrandProfile currentBrandProfile;
        [SerializeField] private DeviceProfile currentDeviceProfile;
        [SerializeField] private bool deviceProfileFound = false;
        
        [Header("Optimization Settings")]
        [SerializeField] private float optimizationMultiplier = 1.0f;
        [SerializeField] private bool enableAdvancedOptimizations = true;
        
        // References
        private DeviceProfiler deviceProfiler;
        private ThermalController thermalController;
        private OptimizationHandler optimizationHandler;
        
        // Events
        public System.Action<DeviceProfile> OnDeviceProfileApplied;
        public System.Action<BrandProfile> OnBrandProfileApplied;
        
        private void Start()
        {
            InitializeDeviceOptimizer();
        }
        
        private void InitializeDeviceOptimizer()
        {
            // Initialize profiles
            InitializeBrandProfiles();
            InitializeDeviceProfiles();
            
            // Get references
            deviceProfiler = SystemManager.Instance?.DeviceProfiler;
            thermalController = GetComponent<ThermalController>();
            optimizationHandler = GetComponent<OptimizationHandler>();
            
            if (deviceProfiler == null)
            {
                Debug.LogError("DeviceSpecificOptimizer: DeviceProfiler not found!");
                return;
            }
            
            // Apply device-specific optimizations
            ApplyDeviceOptimizations();
            
            Debug.Log("Device-specific optimizer initialized");
        }
        
        private void InitializeBrandProfiles()
        {
            brandProfiles = new List<BrandProfile>
            {
                new BrandProfile 
                { 
                    brandName = "Samsung", 
                    thermalThreshold = 42f, 
                    aggressiveOptimization = 0.85f,
                    supportsAdvancedGPU = true,
                    preferredCPUGovernors = new List<string> { "performance", "ondemand" },
                    batteryOptimizationMultiplier = 1.1f,
                    customFeatures = new List<string> { "GameBooster", "AdaptiveRefreshRate", "Knox" }
                },
                new BrandProfile 
                { 
                    brandName = "OnePlus", 
                    thermalThreshold = 45f, 
                    aggressiveOptimization = 0.90f,
                    supportsAdvancedGPU = true,
                    preferredCPUGovernors = new List<string> { "performance", "interactive" },
                    batteryOptimizationMultiplier = 1.05f,
                    customFeatures = new List<string> { "GameSpace", "AlertSlider", "OxygenOS" }
                },
                new BrandProfile 
                { 
                    brandName = "Google", 
                    thermalThreshold = 40f, 
                    aggressiveOptimization = 0.75f,
                    supportsAdvancedGPU = false,
                    preferredCPUGovernors = new List<string> { "schedutil", "ondemand" },
                    batteryOptimizationMultiplier = 1.25f,
                    customFeatures = new List<string> { "AdaptiveBattery", "StockAndroid", "TensorOptimization" }
                },
                new BrandProfile 
                { 
                    brandName = "Xiaomi", 
                    thermalThreshold = 44f, 
                    aggressiveOptimization = 0.80f,
                    supportsAdvancedGPU = true,
                    preferredCPUGovernors = new List<string> { "performance", "ondemand" },
                    batteryOptimizationMultiplier = 0.95f,
                    customFeatures = new List<string> { "GameTurbo", "MIUI", "PerformanceMode" }
                },
                new BrandProfile 
                { 
                    brandName = "Nothing", 
                    thermalThreshold = 43f, 
                    aggressiveOptimization = 0.78f,
                    supportsAdvancedGPU = true,
                    preferredCPUGovernors = new List<string> { "performance", "schedutil" },
                    batteryOptimizationMultiplier = 1.0f,
                    customFeatures = new List<string> { "NothingOS", "GlyphInterface" }
                },
                new BrandProfile 
                { 
                    brandName = "ASUS", 
                    thermalThreshold = 48f, 
                    aggressiveOptimization = 0.95f,
                    supportsAdvancedGPU = true,
                    preferredCPUGovernors = new List<string> { "performance", "powersave" },
                    batteryOptimizationMultiplier = 0.9f,
                    customFeatures = new List<string> { "ROGPhone", "GameGenie", "AirTriggers" }
                }
            };
        }
        
        private void InitializeDeviceProfiles()
        {
            deviceProfiles = new List<DeviceProfile>
            {
                // Samsung Galaxy S23 Series
                new DeviceProfile
                {
                    brandName = "Samsung",
                    modelName = "Galaxy S23 Ultra",
                    codename = "dm3q",
                    chipset = "Snapdragon 8 Gen 2",
                    ramCapacity = 12288,
                    performanceScore = 95,
                    thermalCharacteristics = new ThermalProfile
                    {
                        normalThreshold = 38f,
                        warningThreshold = 42f,
                        criticalThreshold = 48f,
                        coolingEfficiency = 1.3f
                    },
                    optimizationProfile = new OptimizationProfile
                    {
                        drsMultiplier = 1.2f,
                        lodMultiplier = 1.3f,
                        thermalMultiplier = 1.2f,
                        recommendedMode = OptimizationMode.HighPerformance
                    }
                },
                new DeviceProfile
                {
                    brandName = "Samsung",
                    modelName = "Galaxy S23",
                    codename = "dm1q",
                    chipset = "Snapdragon 8 Gen 2",
                    ramCapacity = 8192,
                    performanceScore = 90,
                    thermalCharacteristics = new ThermalProfile
                    {
                        normalThreshold = 36f,
                        warningThreshold = 40f,
                        criticalThreshold = 46f,
                        coolingEfficiency = 1.2f
                    },
                    optimizationProfile = new OptimizationProfile
                    {
                        drsMultiplier = 1.1f,
                        lodMultiplier = 1.25f,
                        thermalMultiplier = 1.1f,
                        recommendedMode = OptimizationMode.Balanced
                    }
                },
                
                // OnePlus Series
                new DeviceProfile
                {
                    brandName = "OnePlus",
                    modelName = "11",
                    codename = "salami",
                    chipset = "Snapdragon 8 Gen 2",
                    ramCapacity = 12288,
                    performanceScore = 92,
                    thermalCharacteristics = new ThermalProfile
                    {
                        normalThreshold = 40f,
                        warningThreshold = 45f,
                        criticalThreshold = 52f,
                        coolingEfficiency = 1.4f
                    },
                    optimizationProfile = new OptimizationProfile
                    {
                        drsMultiplier = 1.15f,
                        lodMultiplier = 1.2f,
                        thermalMultiplier = 1.3f,
                        recommendedMode = OptimizationMode.HighPerformance
                    }
                },
                
                // Google Pixel Series
                new DeviceProfile
                {
                    brandName = "Google",
                    modelName = "Pixel 7 Pro",
                    codename = "cheetah",
                    chipset = "Google Tensor G2",
                    ramCapacity = 12288,
                    performanceScore = 85,
                    thermalCharacteristics = new ThermalProfile
                    {
                        normalThreshold = 35f,
                        warningThreshold = 40f,
                        criticalThreshold = 45f,
                        coolingEfficiency = 0.9f
                    },
                    optimizationProfile = new OptimizationProfile
                    {
                        drsMultiplier = 0.95f,
                        lodMultiplier = 0.9f,
                        thermalMultiplier = 0.8f,
                        recommendedMode = OptimizationMode.Balanced
                    }
                },
                
                // Xiaomi Series
                new DeviceProfile
                {
                    brandName = "Xiaomi",
                    modelName = "13 Pro",
                    codename = "nuwa",
                    chipset = "Snapdragon 8 Gen 2",
                    ramCapacity = 12288,
                    performanceScore = 88,
                    thermalCharacteristics = new ThermalProfile
                    {
                        normalThreshold = 39f,
                        warningThreshold = 44f,
                        criticalThreshold = 50f,
                        coolingEfficiency = 1.1f
                    },
                    optimizationProfile = new OptimizationProfile
                    {
                        drsMultiplier = 1.1f,
                        lodMultiplier = 1.15f,
                        thermalMultiplier = 1.1f,
                        recommendedMode = OptimizationMode.Balanced
                    }
                }
            };
        }
        
        private void ApplyDeviceOptimizations()
        {
            var deviceInfo = deviceProfiler.GetCurrentDevice();
            if (deviceInfo == null) return;
            
            // Find brand profile
            currentBrandProfile = FindBrandProfile(deviceInfo.brand);
            if (currentBrandProfile != null)
            {
                ApplyBrandSpecificOptimizations(currentBrandProfile);
                OnBrandProfileApplied?.Invoke(currentBrandProfile);
            }
            
            // Find specific device profile
            currentDeviceProfile = FindDeviceProfile(deviceInfo.brand, deviceInfo.model, deviceInfo.codename);
            if (currentDeviceProfile != null)
            {
                ApplyDeviceSpecificOptimizations(currentDeviceProfile);
                deviceProfileFound = true;
                OnDeviceProfileApplied?.Invoke(currentDeviceProfile);
            }
            else
            {
                // Create generic profile based on device specs
                currentDeviceProfile = CreateGenericProfile(deviceInfo);
                ApplyDeviceSpecificOptimizations(currentDeviceProfile);
                deviceProfileFound = false;
            }
            
            Debug.Log($"Applied optimizations for {deviceInfo.GetFullDeviceName()} - Profile found: {deviceProfileFound}");
        }
        
        private BrandProfile FindBrandProfile(string brand)
        {
            return brandProfiles.Find(profile => 
                profile.brandName.Equals(brand, System.StringComparison.OrdinalIgnoreCase));
        }
        
        private DeviceProfile FindDeviceProfile(string brand, string model, string codename)
        {
            return deviceProfiles.Find(profile => 
                profile.brandName.Equals(brand, System.StringComparison.OrdinalIgnoreCase) &&
                (profile.modelName.Equals(model, System.StringComparison.OrdinalIgnoreCase) ||
                 profile.codename.Equals(codename, System.StringComparison.OrdinalIgnoreCase)));
        }
        
        private DeviceProfile CreateGenericProfile(DeviceProfiler.DeviceInfo deviceInfo)
        {
            var genericProfile = new DeviceProfile
            {
                brandName = deviceInfo.brand,
                modelName = deviceInfo.model,
                codename = deviceInfo.codename,
                chipset = deviceInfo.chipset,
                ramCapacity = deviceInfo.totalRAM,
                performanceScore = CalculateGenericPerformanceScore(deviceInfo),
                thermalCharacteristics = CreateGenericThermalProfile(deviceInfo),
                optimizationProfile = CreateGenericOptimizationProfile(deviceInfo)
            };
            
            Debug.Log($"Created generic profile for {deviceInfo.GetFullDeviceName()}");
            return genericProfile;
        }
        
        private int CalculateGenericPerformanceScore(DeviceProfiler.DeviceInfo deviceInfo)
        {
            int score = 50; // Base score
            
            // RAM-based scoring
            if (deviceInfo.totalRAM >= 16384) score += 25;      // 16GB+
            else if (deviceInfo.totalRAM >= 12288) score += 20; // 12GB+
            else if (deviceInfo.totalRAM >= 8192) score += 15;  // 8GB+
            else if (deviceInfo.totalRAM >= 6144) score += 10;  // 6GB+
            else if (deviceInfo.totalRAM >= 4096) score += 5;   // 4GB+
            
            // Chipset-based scoring
            string chipset = deviceInfo.chipset.ToLower();
            if (chipset.Contains("snapdragon 8")) score += 20;
            else if (chipset.Contains("snapdragon 7")) score += 15;
            else if (chipset.Contains("snapdragon 6")) score += 10;
            else if (chipset.Contains("tensor")) score += 15;
            else if (chipset.Contains("exynos 22")) score += 18;
            else if (chipset.Contains("a16") || chipset.Contains("a17")) score += 20;
            else if (chipset.Contains("a15")) score += 18;
            else if (chipset.Contains("a14")) score += 15;
            
            return Mathf.Clamp(score, 30, 100);
        }
        
        private ThermalProfile CreateGenericThermalProfile(DeviceProfiler.DeviceInfo deviceInfo)
        {
            // Conservative thermal profile for unknown devices
            return new ThermalProfile
            {
                normalThreshold = 38f,
                warningThreshold = 43f,
                criticalThreshold = 48f,
                coolingEfficiency = 1.0f
            };
        }
        
        private OptimizationProfile CreateGenericOptimizationProfile(DeviceProfiler.DeviceInfo deviceInfo)
        {
            OptimizationMode recommendedMode = OptimizationMode.Balanced;
            float multiplier = 1.0f;
            
            // Determine recommended mode based on device specs
            if (deviceInfo.totalRAM >= 12288 && deviceInfo.chipset.Contains("Snapdragon 8"))
            {
                recommendedMode = OptimizationMode.HighPerformance;
                multiplier = 1.1f;
            }
            else if (deviceInfo.totalRAM < 6144)
            {
                recommendedMode = OptimizationMode.BatterySaver;
                multiplier = 0.8f;
            }
            
            return new OptimizationProfile
            {
                drsMultiplier = multiplier,
                lodMultiplier = multiplier,
                thermalMultiplier = multiplier,
                recommendedMode = recommendedMode
            };
        }
        
        private void ApplyBrandSpecificOptimizations(BrandProfile brandProfile)
        {
            Debug.Log($"Applying brand optimizations for {brandProfile.brandName}");
            
            // Apply thermal settings
            if (thermalController != null)
            {
                thermalController.SetThreshold(brandProfile.thermalThreshold);
            }
            
            // Apply optimization aggressiveness
            if (optimizationHandler != null)
            {
                optimizationHandler.SetAggressiveness(brandProfile.aggressiveOptimization);
            }
            
            // Enable advanced features if supported
            if (brandProfile.supportsAdvancedGPU && enableAdvancedOptimizations)
            {
                EnableAdvancedGPUOptimizations();
            }
            
            // Apply brand-specific features
            ApplyBrandSpecificFeatures(brandProfile);
        }
        
        private void ApplyDeviceSpecificOptimizations(DeviceProfile deviceProfile)
        {
            Debug.Log($"Applying device optimizations for {deviceProfile.brandName} {deviceProfile.modelName}");
            
            // Apply thermal characteristics
            if (thermalController != null && deviceProfile.thermalCharacteristics != null)
            {
                thermalController.SetThreshold(deviceProfile.thermalCharacteristics.normalThreshold);
            }
            
            // Apply optimization multipliers
            if (deviceProfile.optimizationProfile != null)
            {
                optimizationMultiplier = deviceProfile.optimizationProfile.drsMultiplier;
                
                // Set recommended optimization mode
                if (optimizationHandler != null)
                {
                    optimizationHandler.SetPerformanceMode(deviceProfile.optimizationProfile.recommendedMode);
                }
            }
        }
        
        private void EnableAdvancedGPUOptimizations()
        {
            Debug.Log("Enabling advanced GPU optimizations");
            // Implementation would include advanced GPU optimization features
        }
        
        private void ApplyBrandSpecificFeatures(BrandProfile brandProfile)
        {
            foreach (string feature in brandProfile.customFeatures)
            {
                switch (feature)
                {
                    case "GameBooster":
                        EnableSamsungGameBooster();
                        break;
                    case "GameSpace":
                        EnableOnePlusGameSpace();
                        break;
                    case "GameTurbo":
                        EnableXiaomiGameTurbo();
                        break;
                    case "AdaptiveBattery":
                        EnablePixelAdaptiveBattery();
                        break;
                    case "ROGPhone":
                        EnableASUSROGFeatures();
                        break;
                }
            }
        }
        
        private void EnableSamsungGameBooster()
        {
            Debug.Log("Enabling Samsung Game Booster integration");
            // Implementation for Samsung Game Booster API
        }
        
        private void EnableOnePlusGameSpace()
        {
            Debug.Log("Enabling OnePlus Game Space integration");
            // Implementation for OnePlus Game Space API
        }
        
        private void EnableXiaomiGameTurbo()
        {
            Debug.Log("Enabling Xiaomi Game Turbo integration");
            // Implementation for MIUI Game Turbo API
        }
        
        private void EnablePixelAdaptiveBattery()
        {
            Debug.Log("Enabling Pixel Adaptive Battery optimization");
            // Implementation for Pixel-specific battery optimization
        }
        
        private void EnableASUSROGFeatures()
        {
            Debug.Log("Enabling ASUS ROG Phone gaming features");
            // Implementation for ROG Phone gaming features
        }
        
        public DeviceProfile GetCurrentDeviceProfile()
        {
            return currentDeviceProfile;
        }
        
        public BrandProfile GetCurrentBrandProfile()
        {
            return currentBrandProfile;
        }
        
        public float GetOptimizationMultiplier()
        {
            return optimizationMultiplier;
        }
        
        public bool IsDeviceProfileFound()
        {
            return deviceProfileFound;
        }
        
        public void AddCustomDeviceProfile(DeviceProfile profile)
        {
            deviceProfiles.Add(profile);
            Debug.Log($"Added custom device profile for {profile.brandName} {profile.modelName}");
        }
        
        public void AddCustomBrandProfile(BrandProfile profile)
        {
            brandProfiles.Add(profile);
            Debug.Log($"Added custom brand profile for {profile.brandName}");
        }
        
        public string GetOptimizationStatus()
        {
            var status = $"Device-Specific Optimization Status:\n";
            
            if (currentDeviceProfile != null)
            {
                status += $"Device: {currentDeviceProfile.brandName} {currentDeviceProfile.modelName}\n";
                status += $"Chipset: {currentDeviceProfile.chipset}\n";
                status += $"Performance Score: {currentDeviceProfile.performanceScore}/100\n";
                status += $"Profile Found: {(deviceProfileFound ? "Yes" : "Generic")}\n";
                
                if (currentDeviceProfile.optimizationProfile != null)
                {
                    status += $"Recommended Mode: {currentDeviceProfile.optimizationProfile.recommendedMode}\n";
                    status += $"DRS Multiplier: {currentDeviceProfile.optimizationProfile.drsMultiplier:F2}\n";
                    status += $"LOD Multiplier: {currentDeviceProfile.optimizationProfile.lodMultiplier:F2}\n";
                }
            }
            
            if (currentBrandProfile != null)
            {
                status += $"Brand Features: {string.Join(", ", currentBrandProfile.customFeatures)}\n";
                status += $"Advanced GPU: {(currentBrandProfile.supportsAdvancedGPU ? "Yes" : "No")}\n";
            }
            
            status += $"Optimization Multiplier: {optimizationMultiplier:F2}\n";
            
            return status;
        }
    }
}