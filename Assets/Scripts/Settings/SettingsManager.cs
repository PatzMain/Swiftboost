using System;
using System.IO;
using UnityEngine;
using Swiftboost.Core;

namespace Swiftboost.Settings
{
    public class SettingsManager : MonoBehaviour
    {
        [Header("Settings Configuration")]
        [SerializeField] private MonitoringSettings monitoringSettings;
        [SerializeField] private bool autoSaveSettings = true;
        [SerializeField] private float autoSaveInterval = 30f;
        
        [Header("Settings File")]
        [SerializeField] private string settingsFileName = "SwiftboostSettings.json";
        [SerializeField] private bool encryptSettings = false;
        
        // References
        private DeviceProfiler deviceProfiler;
        private string settingsFilePath;
        private DateTime lastSaveTime;
        private bool settingsChanged = false;
        
        // Events
        public event Action<MonitoringSettings> OnSettingsChanged;
        public event Action OnSettingsSaved;
        public event Action OnSettingsLoaded;
        
        public MonitoringSettings MonitoringSettings => monitoringSettings;
        public string SettingsFilePath => settingsFilePath;
        
        private void Start()
        {
            InitializeSettingsManager();
        }
        
        private void InitializeSettingsManager()
        {
            // Get references
            deviceProfiler = SystemManager.Instance?.DeviceProfiler;
            
            // Set up settings file path
            settingsFilePath = Path.Combine(Application.persistentDataPath, settingsFileName);
            
            // Initialize settings
            if (monitoringSettings == null)
            {
                monitoringSettings = new MonitoringSettings();
            }
            
            // Load existing settings or create defaults
            LoadSettings();
            
            // Apply device-specific settings
            ApplyDeviceSpecificSettings();
            
            // Start auto-save if enabled
            if (autoSaveSettings)
            {
                InvokeRepeating(nameof(AutoSave), autoSaveInterval, autoSaveInterval);
            }
            
            Debug.Log($"SettingsManager initialized - Settings file: {settingsFilePath}");
        }
        
        private void ApplyDeviceSpecificSettings()
        {
            if (deviceProfiler == null) return;
            
            var deviceInfo = deviceProfiler.GetCurrentDevice();
            if (deviceInfo == null) return;
            
            // Apply device-specific monitoring settings
            monitoringSettings.ApplyDeviceSpecificSettings(
                deviceInfo.brand, 
                deviceInfo.model, 
                deviceInfo.totalRAM
            );
            
            // Validate settings after applying device-specific changes
            monitoringSettings.ValidateSettings();
            
            settingsChanged = true;
            OnSettingsChanged?.Invoke(monitoringSettings);
            
            Debug.Log($"Applied device-specific settings for {deviceInfo.GetFullDeviceName()}");
        }
        
        public void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsFilePath))
                {
                    string jsonData = File.ReadAllText(settingsFilePath);
                    
                    if (encryptSettings)
                    {
                        jsonData = DecryptString(jsonData);
                    }
                    
                    monitoringSettings = JsonUtility.FromJson<MonitoringSettings>(jsonData);
                    
                    // Validate loaded settings
                    if (monitoringSettings == null)
                    {
                        Debug.LogWarning("Failed to deserialize settings, creating defaults");
                        monitoringSettings = new MonitoringSettings();
                    }
                    else
                    {
                        monitoringSettings.ValidateSettings();
                        Debug.Log("Settings loaded successfully");
                    }
                }
                else
                {
                    Debug.Log("Settings file not found, creating default settings");
                    monitoringSettings = new MonitoringSettings();
                }
                
                OnSettingsLoaded?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load settings: {e.Message}");
                monitoringSettings = new MonitoringSettings();
            }
        }
        
        public void SaveSettings()
        {
            try
            {
                // Validate settings before saving
                monitoringSettings.ValidateSettings();
                
                string jsonData = JsonUtility.ToJson(monitoringSettings, true);
                
                if (encryptSettings)
                {
                    jsonData = EncryptString(jsonData);
                }
                
                // Create directory if it doesn't exist
                string directory = Path.GetDirectoryName(settingsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Write to file
                File.WriteAllText(settingsFilePath, jsonData);
                
                lastSaveTime = DateTime.Now;
                settingsChanged = false;
                OnSettingsSaved?.Invoke();
                
                Debug.Log($"Settings saved successfully to {settingsFilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save settings: {e.Message}");
            }
        }
        
        private void AutoSave()
        {
            if (settingsChanged)
            {
                SaveSettings();
            }
        }
        
        public void ResetToDefaults()
        {
            Debug.Log("Resetting settings to defaults");
            
            monitoringSettings.SetDefaultValues();
            
            // Re-apply device-specific settings
            ApplyDeviceSpecificSettings();
            
            settingsChanged = true;
            OnSettingsChanged?.Invoke(monitoringSettings);
            
            Debug.Log("Settings reset to defaults");
        }
        
        public void SetPerformancePreset(MonitoringSettings.PerformancePreset preset)
        {
            monitoringSettings.SetPerformancePreset(preset);
            settingsChanged = true;
            OnSettingsChanged?.Invoke(monitoringSettings);
            
            Debug.Log($"Performance preset changed to: {preset}");
        }
        
        public void SetMonitoringEnabled(bool fpsEnabled, bool ramEnabled, bool cpuEnabled, bool tempEnabled)
        {
            monitoringSettings.enableFPSMonitoring = fpsEnabled;
            monitoringSettings.enableRAMMonitoring = ramEnabled;
            monitoringSettings.enableCPUMonitoring = cpuEnabled;
            monitoringSettings.enableTemperatureMonitoring = tempEnabled;
            
            settingsChanged = true;
            OnSettingsChanged?.Invoke(monitoringSettings);
            
            Debug.Log("Monitoring settings updated");
        }
        
        public void SetOptimizationEnabled(bool drsEnabled, bool lodEnabled, bool thermalEnabled)
        {
            monitoringSettings.enableDRS = drsEnabled;
            monitoringSettings.enableGeometricLOD = lodEnabled;
            monitoringSettings.enableThermalThrottling = thermalEnabled;
            
            settingsChanged = true;
            OnSettingsChanged?.Invoke(monitoringSettings);
            
            Debug.Log("Optimization settings updated");
        }
        
        public void SetTargetFPS(int fps)
        {
            monitoringSettings.targetFPS = Mathf.Clamp(fps, 15, 240);
            settingsChanged = true;
            OnSettingsChanged?.Invoke(monitoringSettings);
            
            Debug.Log($"Target FPS set to: {fps}");
        }
        
        public void SetOptimizationAggressiveness(float aggressiveness)
        {
            monitoringSettings.optimizationAggressiveness = Mathf.Clamp01(aggressiveness);
            settingsChanged = true;
            OnSettingsChanged?.Invoke(monitoringSettings);
            
            Debug.Log($"Optimization aggressiveness set to: {aggressiveness:F2}");
        }
        
        public void SetTemperatureThreshold(float threshold)
        {
            monitoringSettings.maxTemperatureThreshold = Mathf.Clamp(threshold, 30f, 70f);
            settingsChanged = true;
            OnSettingsChanged?.Invoke(monitoringSettings);
            
            Debug.Log($"Temperature threshold set to: {threshold:F1}°C");
        }
        
        public void SetOverlaySettings(bool enabled, Vector2 position, float transparency)
        {
            monitoringSettings.showPerformanceOverlay = enabled;
            monitoringSettings.overlayPosition = position;
            monitoringSettings.overlayTransparency = Mathf.Clamp01(transparency);
            
            settingsChanged = true;
            OnSettingsChanged?.Invoke(monitoringSettings);
            
            Debug.Log($"Overlay settings updated - Enabled: {enabled}, Transparency: {transparency:F2}");
        }
        
        public void SetDeviceSpecificOptimization(bool enabled)
        {
            monitoringSettings.enableDeviceSpecificOptimization = enabled;
            settingsChanged = true;
            OnSettingsChanged?.Invoke(monitoringSettings);
            
            Debug.Log($"Device-specific optimization: {(enabled ? "Enabled" : "Disabled")}");
        }
        
        public void SetBatteryOptimization(bool enabled, int lowBatteryThreshold)
        {
            monitoringSettings.enableBatteryOptimization = enabled;
            monitoringSettings.lowBatteryThreshold = Mathf.Clamp(lowBatteryThreshold, 5, 50);
            
            settingsChanged = true;
            OnSettingsChanged?.Invoke(monitoringSettings);
            
            Debug.Log($"Battery optimization: {(enabled ? "Enabled" : "Disabled")}, Threshold: {lowBatteryThreshold}%");
        }
        
        public void SetDebugSettings(bool enableLogging, bool enableExperimental)
        {
            monitoringSettings.enableDebugLogging = enableLogging;
            monitoringSettings.enableExperimentalFeatures = enableExperimental;
            
            settingsChanged = true;
            OnSettingsChanged?.Invoke(monitoringSettings);
            
            Debug.Log($"Debug settings - Logging: {enableLogging}, Experimental: {enableExperimental}");
        }
        
        public void SetUpdateIntervals(int fpsInterval, int ramInterval, int cpuInterval, int tempInterval)
        {
            monitoringSettings.fpsUpdateInterval = Mathf.Clamp(fpsInterval, 100, 10000);
            monitoringSettings.ramUpdateInterval = Mathf.Clamp(ramInterval, 500, 30000);
            monitoringSettings.cpuUpdateInterval = Mathf.Clamp(cpuInterval, 500, 30000);
            monitoringSettings.temperatureUpdateInterval = Mathf.Clamp(tempInterval, 1000, 60000);
            
            settingsChanged = true;
            OnSettingsChanged?.Invoke(monitoringSettings);
            
            Debug.Log("Update intervals changed");
        }
        
        public void ImportSettings(string jsonData)
        {
            try
            {
                var importedSettings = JsonUtility.FromJson<MonitoringSettings>(jsonData);
                
                if (importedSettings != null)
                {
                    importedSettings.ValidateSettings();
                    monitoringSettings = importedSettings;
                    
                    settingsChanged = true;
                    OnSettingsChanged?.Invoke(monitoringSettings);
                    
                    Debug.Log("Settings imported successfully");
                }
                else
                {
                    Debug.LogError("Failed to import settings - invalid JSON data");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to import settings: {e.Message}");
            }
        }
        
        public string ExportSettings()
        {
            try
            {
                return JsonUtility.ToJson(monitoringSettings, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to export settings: {e.Message}");
                return string.Empty;
            }
        }
        
        public void CreateBackup()
        {
            try
            {
                string backupFileName = $"SwiftboostSettings_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                string backupFilePath = Path.Combine(Application.persistentDataPath, "Backups", backupFileName);
                
                // Create backup directory if it doesn't exist
                string backupDirectory = Path.GetDirectoryName(backupFilePath);
                if (!Directory.Exists(backupDirectory))
                {
                    Directory.CreateDirectory(backupDirectory);
                }
                
                // Save current settings to backup file
                string jsonData = JsonUtility.ToJson(monitoringSettings, true);
                File.WriteAllText(backupFilePath, jsonData);
                
                Debug.Log($"Settings backup created: {backupFilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create backup: {e.Message}");
            }
        }
        
        private string EncryptString(string plainText)
        {
            // Simple XOR encryption for demonstration
            // In production, use proper encryption
            byte[] data = System.Text.Encoding.UTF8.GetBytes(plainText);
            byte key = 0x5A; // Simple key
            
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= key;
            }
            
            return Convert.ToBase64String(data);
        }
        
        private string DecryptString(string cipherText)
        {
            try
            {
                byte[] data = Convert.FromBase64String(cipherText);
                byte key = 0x5A; // Same key as encryption
                
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] ^= key;
                }
                
                return System.Text.Encoding.UTF8.GetString(data);
            }
            catch
            {
                // If decryption fails, return as-is (might be unencrypted)
                return cipherText;
            }
        }
        
        public string GetSettingsInfo()
        {
            var info = $"Settings Manager Info:\n";
            info += $"File Path: {settingsFilePath}\n";
            info += $"Last Save: {(lastSaveTime == default(DateTime) ? "Never" : lastSaveTime.ToString("yyyy-MM-dd HH:mm:ss"))}\n";
            info += $"Auto Save: {(autoSaveSettings ? $"Enabled ({autoSaveInterval}s)" : "Disabled")}\n";
            info += $"Encrypted: {(encryptSettings ? "Yes" : "No")}\n";
            info += $"Changes Pending: {(settingsChanged ? "Yes" : "No")}\n";
            info += $"\n{monitoringSettings.GetSettingsDescription()}";
            
            return info;
        }
        
        public bool HasUnsavedChanges()
        {
            return settingsChanged;
        }
        
        public void MarkAsChanged()
        {
            settingsChanged = true;
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && settingsChanged)
            {
                // Save settings when app is paused
                SaveSettings();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && settingsChanged)
            {
                // Save settings when app loses focus
                SaveSettings();
            }
        }
        
        private void OnDestroy()
        {
            if (settingsChanged)
            {
                SaveSettings();
            }
            
            // Cancel auto-save
            if (autoSaveSettings)
            {
                CancelInvoke(nameof(AutoSave));
            }
        }
    }
}