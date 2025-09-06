using System.Collections.Generic;
using UnityEngine;
using Swiftboost.Core;
using Swiftboost.Game;

namespace Swiftboost.Optimization
{
    public enum LODType
    {
        Geometric,    // Mesh complexity reduction
        Texture,      // Texture resolution scaling  
        Shader,       // Shader quality adjustment
        Particle,     // Particle system optimization
        Audio,        // Audio quality adjustment
        Shadow,       // Shadow quality adjustment
        PostProcess   // Post-processing effects
    }
    
    public class LODManager : MonoBehaviour
    {
        [Header("LOD Configuration")]
        [SerializeField] private bool lodEnabled = true;
        [SerializeField] private float currentLODLevel = 1.0f; // 0.0 = lowest quality, 1.0 = highest quality
        [SerializeField] private float targetPerformanceRatio = 0.8f;
        
        [Header("LOD Types")]
        [SerializeField] private bool geometricLOD = true;
        [SerializeField] private bool textureLOD = true;
        [SerializeField] private bool shaderLOD = true;
        [SerializeField] private bool particleLOD = true;
        [SerializeField] private bool audioLOD = false;
        [SerializeField] private bool shadowLOD = true;
        [SerializeField] private bool postProcessLOD = true;
        
        [Header("Quality Levels")]
        [SerializeField] private int maxTextureSize = 2048;
        [SerializeField] private int currentTextureSize = 2048;
        [SerializeField] private float meshQuality = 1.0f;
        [SerializeField] private float particleDensity = 1.0f;
        [SerializeField] private int shadowQuality = 2; // 0=off, 1=low, 2=medium, 3=high
        
        // Device-specific settings
        private DeviceProfiler.DeviceInfo currentDevice;
        private float deviceLODMultiplier = 1.0f;
        private Dictionary<LODType, float> lodLevels;
        
        // LOD Groups tracking
        private List<LODGroup> managedLODGroups;
        private List<ParticleSystem> managedParticleSystems;
        private List<Light> managedLights;
        
        // Events
        public System.Action<LODType, float> OnLODChanged;
        public System.Action<string> OnLODOptimizationApplied;
        
        public bool IsEnabled => lodEnabled;
        public float CurrentLODLevel => currentLODLevel;
        
        private void Start()
        {
            InitializeLODManager();
        }
        
        private void InitializeLODManager()
        {
            // Initialize collections
            lodLevels = new Dictionary<LODType, float>();
            managedLODGroups = new List<LODGroup>();
            managedParticleSystems = new List<ParticleSystem>();
            managedLights = new List<Light>();
            
            // Initialize LOD levels
            foreach (LODType lodType in System.Enum.GetValues(typeof(LODType)))
            {
                lodLevels[lodType] = 1.0f; // Start with highest quality
            }
            
            // Find and register LOD components
            RegisterLODComponents();
            
            Debug.Log("LOD Manager initialized");
        }
        
        public void InitializeForDevice(DeviceProfiler.DeviceInfo deviceInfo)
        {
            currentDevice = deviceInfo;
            ApplyDeviceSpecificSettings();
        }
        
        private void ApplyDeviceSpecificSettings()
        {
            if (currentDevice == null) return;
            
            // Calculate device LOD multiplier based on device capabilities
            deviceLODMultiplier = CalculateDeviceLODMultiplier();
            
            // Set initial texture size based on device
            SetInitialTextureSize();
            
            // Apply device-specific quality presets
            ApplyDeviceQualityPreset();
            
            Debug.Log($"Applied LOD settings for {currentDevice.GetFullDeviceName()} - Multiplier: {deviceLODMultiplier:F2}");
        }
        
        private float CalculateDeviceLODMultiplier()
        {
            if (currentDevice == null) return 1.0f;
            
            string deviceKey = $"{currentDevice.brand}_{currentDevice.model}".ToLower();
            
            // Flagship device multipliers (can handle higher LOD)
            if (deviceKey.Contains("samsung_galaxy_s23") || deviceKey.Contains("samsung_galaxy_s24"))
                return 1.3f;
            else if (deviceKey.Contains("oneplus_11") || deviceKey.Contains("oneplus_12"))
                return 1.2f;
            else if (deviceKey.Contains("google_pixel_7") || deviceKey.Contains("google_pixel_8"))
                return 0.9f; // Conservative for Tensor chips
            else if (deviceKey.Contains("xiaomi_13"))
                return 1.1f;
            
            // RAM-based multiplier
            if (currentDevice.totalRAM >= 12288) // 12GB+
                return 1.2f;
            else if (currentDevice.totalRAM >= 8192) // 8GB+
                return 1.0f;
            else if (currentDevice.totalRAM >= 6144) // 6GB+
                return 0.8f;
            else if (currentDevice.totalRAM >= 4096) // 4GB+
                return 0.6f;
            else
                return 0.4f; // Budget devices
        }
        
        private void SetInitialTextureSize()
        {
            if (currentDevice.totalRAM >= 8192)
                maxTextureSize = 2048;
            else if (currentDevice.totalRAM >= 6144)
                maxTextureSize = 1024;
            else if (currentDevice.totalRAM >= 4096)
                maxTextureSize = 512;
            else
                maxTextureSize = 256;
            
            currentTextureSize = maxTextureSize;
        }
        
        private void ApplyDeviceQualityPreset()
        {
            if (currentDevice.totalRAM >= 8192)
            {
                // High-end preset
                meshQuality = 1.0f;
                particleDensity = 1.0f;
                shadowQuality = 3;
            }
            else if (currentDevice.totalRAM >= 6144)
            {
                // Mid-range preset
                meshQuality = 0.8f;
                particleDensity = 0.7f;
                shadowQuality = 2;
            }
            else if (currentDevice.totalRAM >= 4096)
            {
                // Budget preset
                meshQuality = 0.6f;
                particleDensity = 0.5f;
                shadowQuality = 1;
            }
            else
            {
                // Low-end preset
                meshQuality = 0.4f;
                particleDensity = 0.3f;
                shadowQuality = 0;
            }
        }
        
        private void RegisterLODComponents()
        {
            // Find all LOD Groups in the scene
            LODGroup[] lodGroups = FindObjectsOfType<LODGroup>();
            managedLODGroups.AddRange(lodGroups);
            
            // Find all Particle Systems
            ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>();
            managedParticleSystems.AddRange(particleSystems);
            
            // Find all Lights
            Light[] lights = FindObjectsOfType<Light>();
            managedLights.AddRange(lights);
            
            Debug.Log($"Registered LOD components - Groups: {managedLODGroups.Count}, Particles: {managedParticleSystems.Count}, Lights: {managedLights.Count}");
        }
        
        public void SetMode(OptimizationMode mode)
        {
            switch (mode)
            {
                case OptimizationMode.BatterySaver:
                    targetPerformanceRatio = 0.4f;
                    ApplyQualityPreset(QualityLevel.Low);
                    break;
                    
                case OptimizationMode.Balanced:
                    targetPerformanceRatio = 0.7f;
                    ApplyQualityPreset(QualityLevel.Medium);
                    break;
                    
                case OptimizationMode.HighPerformance:
                    targetPerformanceRatio = 1.0f;
                    ApplyQualityPreset(QualityLevel.High);
                    break;
            }
            
            Debug.Log($"LOD mode set to {mode} - Target ratio: {targetPerformanceRatio:F2}");
        }
        
        private void ApplyQualityPreset(QualityLevel level)
        {
            switch (level)
            {
                case QualityLevel.Low:
                    currentLODLevel = 0.3f;
                    break;
                case QualityLevel.Medium:
                    currentLODLevel = 0.6f;
                    break;
                case QualityLevel.High:
                    currentLODLevel = 1.0f;
                    break;
            }
            
            ApplyLODLevel(currentLODLevel);
        }
        
        public void SetAggressiveness(float aggressiveness)
        {
            // Adjust LOD sensitivity based on aggressiveness
            float deviceAdjustedAggressiveness = aggressiveness * deviceLODMultiplier;
            targetPerformanceRatio = Mathf.Lerp(0.3f, 1.0f, 1f - deviceAdjustedAggressiveness);
            
            Debug.Log($"LOD aggressiveness set to {aggressiveness:F2} (device adjusted: {deviceAdjustedAggressiveness:F2})");
        }
        
        public void StartOptimization()
        {
            Debug.Log("LOD optimization started");
        }
        
        public void StopOptimization()
        {
            // Reset to highest quality when stopping
            ApplyLODLevel(1.0f);
            Debug.Log("LOD optimization stopped - Reset to highest quality");
        }
        
        public void ApplyOptimization(float performanceRatio)
        {
            if (!lodEnabled) return;
            
            float deviceAdjustedRatio = performanceRatio * deviceLODMultiplier;
            ApplyLODOptimization(LODType.Geometric, deviceAdjustedRatio);
            ApplyLODOptimization(LODType.Texture, deviceAdjustedRatio);
            ApplyLODOptimization(LODType.Particle, deviceAdjustedRatio);
            ApplyLODOptimization(LODType.Shader, deviceAdjustedRatio);
            
            OnLODOptimizationApplied?.Invoke($"Applied LOD optimization (ratio: {deviceAdjustedRatio:F2})");
        }
        
        public void ApplyLODOptimization(LODType type, float performanceRatio)
        {
            if (!IsLODTypeEnabled(type)) return;
            
            float adjustedRatio = performanceRatio * deviceLODMultiplier;
            adjustedRatio = Mathf.Clamp01(adjustedRatio);
            
            switch (type)
            {
                case LODType.Geometric:
                    AdjustMeshComplexity(adjustedRatio);
                    break;
                case LODType.Texture:
                    ScaleTextureResolution(adjustedRatio);
                    break;
                case LODType.Shader:
                    AdjustShaderQuality(adjustedRatio);
                    break;
                case LODType.Particle:
                    OptimizeParticleSystems(adjustedRatio);
                    break;
                case LODType.Shadow:
                    AdjustShadowQuality(adjustedRatio);
                    break;
                case LODType.PostProcess:
                    AdjustPostProcessing(adjustedRatio);
                    break;
            }
            
            lodLevels[type] = adjustedRatio;
            OnLODChanged?.Invoke(type, adjustedRatio);
        }
        
        private void AdjustMeshComplexity(float ratio)
        {
            meshQuality = ratio;
            
            foreach (var lodGroup in managedLODGroups)
            {
                if (lodGroup == null) continue;
                
                // Adjust LOD bias based on performance ratio
                float lodBias = Mathf.Lerp(2f, 0f, ratio); // Higher bias = lower quality earlier
                
                // Apply LOD bias (this would require custom LOD implementation)
                // For Unity's built-in LOD system, we can adjust the screen percentage thresholds
                LOD[] lods = lodGroup.GetLODs();
                for (int i = 0; i < lods.Length; i++)
                {
                    lods[i].screenRelativeTransitionHeight *= (1f + lodBias * 0.5f);
                }
                lodGroup.SetLODs(lods);
            }
        }
        
        private void ScaleTextureResolution(float ratio)
        {
            int targetSize = Mathf.RoundToInt(maxTextureSize * ratio);
            targetSize = Mathf.ClosestPowerOfTwo(targetSize);
            targetSize = Mathf.Clamp(targetSize, 64, maxTextureSize);
            
            if (targetSize != currentTextureSize)
            {
                currentTextureSize = targetSize;
                
                // Apply texture scaling globally
                QualitySettings.masterTextureLimit = Mathf.RoundToInt(Mathf.Log(maxTextureSize / currentTextureSize, 2));
                
                Debug.Log($"Texture resolution scaled to {currentTextureSize}px (ratio: {ratio:F2})");
            }
        }
        
        private void AdjustShaderQuality(float ratio)
        {
            // Adjust shader LOD globally
            Shader.globalMaximumLOD = Mathf.RoundToInt(Mathf.Lerp(100, 600, ratio));
            
            // Adjust quality settings
            if (ratio < 0.3f)
                QualitySettings.pixelLightCount = 0;
            else if (ratio < 0.6f)
                QualitySettings.pixelLightCount = 1;
            else if (ratio < 0.8f)
                QualitySettings.pixelLightCount = 2;
            else
                QualitySettings.pixelLightCount = 4;
        }
        
        private void OptimizeParticleSystems(float ratio)
        {
            particleDensity = ratio;
            
            foreach (var particle in managedParticleSystems)
            {
                if (particle == null) continue;
                
                var main = particle.main;
                var emission = particle.emission;
                
                // Scale particle count
                emission.rateOverTimeMultiplier = ratio;
                
                // Adjust max particles
                main.maxParticles = Mathf.RoundToInt(main.maxParticles * ratio);
                
                // Disable particle systems if ratio is very low
                if (ratio < 0.2f)
                {
                    particle.gameObject.SetActive(false);
                }
                else if (!particle.gameObject.activeInHierarchy)
                {
                    particle.gameObject.SetActive(true);
                }
            }
        }
        
        private void AdjustShadowQuality(float ratio)
        {
            if (ratio < 0.2f)
            {
                QualitySettings.shadows = ShadowQuality.Disable;
                shadowQuality = 0;
            }
            else if (ratio < 0.5f)
            {
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.shadowResolution = ShadowResolution.Low;
                shadowQuality = 1;
            }
            else if (ratio < 0.8f)
            {
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowResolution = ShadowResolution.Medium;
                shadowQuality = 2;
            }
            else
            {
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowResolution = ShadowResolution.High;
                shadowQuality = 3;
            }
            
            // Adjust shadow distance
            QualitySettings.shadowDistance = Mathf.Lerp(20f, 150f, ratio);
        }
        
        private void AdjustPostProcessing(float ratio)
        {
            // This would integrate with Unity's Post Processing Stack
            // For now, we'll adjust anti-aliasing as an example
            
            if (ratio < 0.3f)
            {
                QualitySettings.antiAliasing = 0; // No AA
            }
            else if (ratio < 0.6f)
            {
                QualitySettings.antiAliasing = 2; // 2x MSAA
            }
            else if (ratio < 0.8f)
            {
                QualitySettings.antiAliasing = 4; // 4x MSAA
            }
            else
            {
                QualitySettings.antiAliasing = 8; // 8x MSAA
            }
        }
        
        public void ApplyLODLevel(float level)
        {
            currentLODLevel = Mathf.Clamp01(level);
            
            // Apply LOD level to all types
            foreach (LODType lodType in System.Enum.GetValues(typeof(LODType)))
            {
                if (IsLODTypeEnabled(lodType))
                {
                    ApplyLODOptimization(lodType, currentLODLevel);
                }
            }
            
            Debug.Log($"Applied LOD level: {currentLODLevel:F2}");
        }
        
        public void ApplyMemoryOptimization(float strength)
        {
            // Aggressive texture and mesh quality reduction for memory
            float memoryRatio = 1f - strength;
            
            ScaleTextureResolution(memoryRatio);
            AdjustMeshComplexity(memoryRatio);
            
            // Disable non-essential particle systems
            if (strength > 0.7f)
            {
                foreach (var particle in managedParticleSystems)
                {
                    if (particle != null && !particle.main.prewarm) // Keep essential particles
                    {
                        particle.gameObject.SetActive(false);
                    }
                }
            }
            
            Debug.Log($"Applied memory optimization (strength: {strength:F2})");
        }
        
        public void ReduceOptimization(float amount)
        {
            // Gradually increase LOD quality
            float newLevel = Mathf.Min(currentLODLevel + amount, 1.0f);
            ApplyLODLevel(newLevel);
            
            Debug.Log($"Reduced LOD optimization - Level: {newLevel:F2}");
        }
        
        private bool IsLODTypeEnabled(LODType type)
        {
            switch (type)
            {
                case LODType.Geometric: return geometricLOD;
                case LODType.Texture: return textureLOD;
                case LODType.Shader: return shaderLOD;
                case LODType.Particle: return particleLOD;
                case LODType.Audio: return audioLOD;
                case LODType.Shadow: return shadowLOD;
                case LODType.PostProcess: return postProcessLOD;
                default: return false;
            }
        }
        
        public void SetLODTypeEnabled(LODType type, bool enabled)
        {
            switch (type)
            {
                case LODType.Geometric: geometricLOD = enabled; break;
                case LODType.Texture: textureLOD = enabled; break;
                case LODType.Shader: shaderLOD = enabled; break;
                case LODType.Particle: particleLOD = enabled; break;
                case LODType.Audio: audioLOD = enabled; break;
                case LODType.Shadow: shadowLOD = enabled; break;
                case LODType.PostProcess: postProcessLOD = enabled; break;
            }
            
            Debug.Log($"LOD type {type} {(enabled ? "enabled" : "disabled")}");
        }
        
        public string GetLODStatus()
        {
            var status = $"LOD Status:\n";
            status += $"Enabled: {lodEnabled}\n";
            status += $"Current Level: {currentLODLevel:F2}\n";
            status += $"Device Multiplier: {deviceLODMultiplier:F2}\n";
            status += $"Texture Size: {currentTextureSize}px\n";
            status += $"Mesh Quality: {meshQuality:F2}\n";
            status += $"Particle Density: {particleDensity:F2}\n";
            status += $"Shadow Quality: {shadowQuality}\n";
            status += $"Managed Components: {managedLODGroups.Count + managedParticleSystems.Count + managedLights.Count}\n";
            
            if (currentDevice != null)
            {
                status += $"Device: {currentDevice.GetFullDeviceName()}\n";
            }
            
            return status;
        }
        
        private enum QualityLevel
        {
            Low,
            Medium,
            High
        }
    }
}