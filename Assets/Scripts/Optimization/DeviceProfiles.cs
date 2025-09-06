using System.Collections.Generic;
using UnityEngine;
using Swiftboost.Game;

namespace Swiftboost.Optimization
{
    [System.Serializable]
    public class BrandProfile
    {
        [Header("Brand Information")]
        public string brandName;
        public float thermalThreshold;
        public float aggressiveOptimization;
        public bool supportsAdvancedGPU;
        public float batteryOptimizationMultiplier;
        
        [Header("System Configuration")]
        public List<string> preferredCPUGovernors;
        public List<string> customFeatures;
        
        [Header("Performance Characteristics")]
        public float performanceMultiplier = 1.0f;
        public float powerEfficiencyRating = 1.0f;
        public bool hasCustomThermalManagement = false;
        
        public BrandProfile()
        {
            preferredCPUGovernors = new List<string>();
            customFeatures = new List<string>();
        }
        
        public string GetBrandDescription()
        {
            var desc = $"Brand: {brandName}\n";
            desc += $"Thermal Threshold: {thermalThreshold}°C\n";
            desc += $"Optimization Aggressiveness: {aggressiveOptimization:F2}\n";
            desc += $"Advanced GPU: {(supportsAdvancedGPU ? "Supported" : "Not Supported")}\n";
            desc += $"Battery Multiplier: {batteryOptimizationMultiplier:F2}\n";
            desc += $"Custom Features: {string.Join(", ", customFeatures)}\n";
            return desc;
        }
    }
    
    [System.Serializable]
    public class DeviceProfile
    {
        [Header("Device Identification")]
        public string brandName;
        public string modelName;
        public string codename;
        public string chipset;
        public int ramCapacity;
        public string displayResolution;
        
        [Header("Performance Rating")]
        public int performanceScore; // 0-100
        public string performanceCategory; // Budget, Mid-range, Flagship, Gaming
        
        [Header("Hardware Capabilities")]
        public bool supports5G = false;
        public bool supportsHighRefreshRate = false;
        public bool supportsWirelessCharging = false;
        public bool supportsFastCharging = false;
        public int maxRefreshRate = 60;
        
        [Header("Optimization Profiles")]
        public ThermalProfile thermalCharacteristics;
        public OptimizationProfile optimizationProfile;
        public PowerProfile powerProfile;
        
        [Header("Known Issues")]
        public List<string> knownIssues;
        public List<string> recommendedSettings;
        
        public DeviceProfile()
        {
            knownIssues = new List<string>();
            recommendedSettings = new List<string>();
        }
        
        public string GetDeviceDisplayName()
        {
            return $"{brandName} {modelName}";
        }
        
        public string GetProfileIdentifier()
        {
            return $"{brandName}_{modelName}_{codename}".Replace(" ", "_").ToLower();
        }
        
        public string GetPerformanceCategory()
        {
            if (performanceScore >= 90)
                return "Flagship Gaming";
            else if (performanceScore >= 80)
                return "High Performance";
            else if (performanceScore >= 70)
                return "Mid-range Plus";
            else if (performanceScore >= 60)
                return "Mid-range";
            else if (performanceScore >= 50)
                return "Budget Plus";
            else
                return "Budget";
        }
        
        public bool IsHighEndDevice()
        {
            return performanceScore >= 85 && ramCapacity >= 8192;
        }
        
        public bool IsMidRangeDevice()
        {
            return performanceScore >= 65 && performanceScore < 85;
        }
        
        public bool IsBudgetDevice()
        {
            return performanceScore < 65 || ramCapacity < 6144;
        }
        
        public string GetDeviceInfo()
        {
            var info = $"Device Profile: {GetDeviceDisplayName()}\n";
            info += $"Codename: {codename}\n";
            info += $"Chipset: {chipset}\n";
            info += $"RAM: {ramCapacity}MB\n";
            info += $"Performance Score: {performanceScore}/100 ({GetPerformanceCategory()})\n";
            info += $"Max Refresh Rate: {maxRefreshRate}Hz\n";
            
            if (thermalCharacteristics != null)
            {
                info += $"Thermal Thresholds: {thermalCharacteristics.normalThreshold}°C / {thermalCharacteristics.warningThreshold}°C / {thermalCharacteristics.criticalThreshold}°C\n";
            }
            
            if (knownIssues.Count > 0)
            {
                info += $"Known Issues: {string.Join(", ", knownIssues)}\n";
            }
            
            return info;
        }
    }
    
    [System.Serializable]
    public class ThermalProfile
    {
        [Header("Temperature Thresholds")]
        public float normalThreshold = 40f;
        public float warningThreshold = 45f;
        public float criticalThreshold = 50f;
        public float emergencyThreshold = 55f;
        
        [Header("Thermal Characteristics")]
        public float coolingEfficiency = 1.0f; // How quickly device cools down
        public float heatupRate = 1.0f; // How quickly device heats up
        public float sustainedPerformanceTemp = 42f; // Temperature at which sustained performance drops
        
        [Header("Thermal Management")]
        public bool hasActiveCooling = false;
        public bool hasVaporChamber = false;
        public bool hasHeatPipe = false;
        public float thermalMass = 1.0f; // Relative thermal mass
        
        public string GetThermalDescription()
        {
            var desc = $"Thermal Profile:\n";
            desc += $"Normal: {normalThreshold}°C, Warning: {warningThreshold}°C, Critical: {criticalThreshold}°C\n";
            desc += $"Cooling Efficiency: {coolingEfficiency:F2}\n";
            desc += $"Sustained Performance Temp: {sustainedPerformanceTemp}°C\n";
            desc += $"Active Cooling: {(hasActiveCooling ? "Yes" : "No")}\n";
            desc += $"Vapor Chamber: {(hasVaporChamber ? "Yes" : "No")}\n";
            return desc;
        }
    }
    
    [System.Serializable]
    public class OptimizationProfile
    {
        [Header("Optimization Multipliers")]
        public float drsMultiplier = 1.0f;
        public float lodMultiplier = 1.0f;
        public float thermalMultiplier = 1.0f;
        public float memoryMultiplier = 1.0f;
        
        [Header("Performance Settings")]
        public OptimizationMode recommendedMode = OptimizationMode.Balanced;
        public int recommendedTargetFPS = 60;
        public float recommendedAggressiveness = 0.7f;
        
        [Header("Feature Support")]
        public bool supportsDRS = true;
        public bool supportsAdvancedLOD = true;
        public bool supportsThermalThrottling = true;
        public bool supportsMemoryOptimization = true;
        
        [Header("Game-Specific Settings")]
        public Dictionary<string, GameOptimizationSetting> gameSpecificSettings;
        
        public OptimizationProfile()
        {
            gameSpecificSettings = new Dictionary<string, GameOptimizationSetting>();
        }
        
        public string GetOptimizationDescription()
        {
            var desc = $"Optimization Profile:\n";
            desc += $"Recommended Mode: {recommendedMode}\n";
            desc += $"Target FPS: {recommendedTargetFPS}\n";
            desc += $"Aggressiveness: {recommendedAggressiveness:F2}\n";
            desc += $"DRS Multiplier: {drsMultiplier:F2}\n";
            desc += $"LOD Multiplier: {lodMultiplier:F2}\n";
            desc += $"Thermal Multiplier: {thermalMultiplier:F2}\n";
            return desc;
        }
    }
    
    [System.Serializable]
    public class PowerProfile
    {
        [Header("Battery Characteristics")]
        public int batteryCapacity; // mAh
        public float batteryEfficiencyRating = 1.0f;
        public bool supportsFastCharging = false;
        public bool supportsWirelessCharging = false;
        public int fastChargingWattage = 0;
        
        [Header("Power Management")]
        public float idlePowerConsumption = 1.0f; // Relative to baseline
        public float activePowerConsumption = 1.0f; // Relative to baseline
        public float screenPowerConsumption = 1.0f; // Relative to baseline
        
        [Header("Battery Optimization")]
        public bool hasAdaptiveBattery = false;
        public bool hasBatteryOptimization = false;
        public float lowPowerModeThreshold = 20f; // Percentage
        
        public string GetPowerDescription()
        {
            var desc = $"Power Profile:\n";
            desc += $"Battery: {batteryCapacity}mAh\n";
            desc += $"Efficiency Rating: {batteryEfficiencyRating:F2}\n";
            desc += $"Fast Charging: {(supportsFastCharging ? $"{fastChargingWattage}W" : "No")}\n";
            desc += $"Wireless Charging: {(supportsWirelessCharging ? "Yes" : "No")}\n";
            desc += $"Adaptive Battery: {(hasAdaptiveBattery ? "Yes" : "No")}\n";
            return desc;
        }
    }
    
    [System.Serializable]
    public class GameOptimizationSetting
    {
        public string gameName;
        public OptimizationMode preferredMode;
        public int targetFPS;
        public float aggressiveness;
        public bool enableDRS;
        public bool enableLOD;
        public bool enableThermalThrottling;
        public List<string> specificSettings;
        
        public GameOptimizationSetting()
        {
            specificSettings = new List<string>();
        }
    }
    
    [System.Serializable]
    public class DeviceCapabilityScore
    {
        [Header("Performance Scores")]
        public float cpuScore = 50f;        // 0-100
        public float gpuScore = 50f;        // 0-100
        public float ramScore = 50f;        // 0-100
        public float storageScore = 50f;    // 0-100
        public float displayScore = 50f;    // 0-100
        public float thermalScore = 50f;    // 0-100
        public float batteryScore = 50f;    // 0-100
        public float overallScore = 50f;    // Calculated average
        
        public void CalculateOverallScore()
        {
            overallScore = (cpuScore + gpuScore + ramScore + storageScore + 
                           displayScore + thermalScore + batteryScore) / 7f;
        }
        
        public string GetScoreDescription()
        {
            CalculateOverallScore();
            
            var desc = $"Device Capability Scores:\n";
            desc += $"Overall: {overallScore:F1}/100\n";
            desc += $"CPU: {cpuScore:F1}/100\n";
            desc += $"GPU: {gpuScore:F1}/100\n";
            desc += $"RAM: {ramScore:F1}/100\n";
            desc += $"Storage: {storageScore:F1}/100\n";
            desc += $"Display: {displayScore:F1}/100\n";
            desc += $"Thermal: {thermalScore:F1}/100\n";
            desc += $"Battery: {batteryScore:F1}/100\n";
            
            return desc;
        }
        
        public string GetPerformanceRating()
        {
            CalculateOverallScore();
            
            if (overallScore >= 90f)
                return "Flagship Gaming Beast";
            else if (overallScore >= 80f)
                return "High Performance Gaming";
            else if (overallScore >= 70f)
                return "Solid Gaming Experience";
            else if (overallScore >= 60f)
                return "Casual Gaming Ready";
            else if (overallScore >= 50f)
                return "Basic Gaming Capable";
            else
                return "Limited Gaming Performance";
        }
    }
}