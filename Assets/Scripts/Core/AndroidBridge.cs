using System;
using System.Collections.Generic;
using UnityEngine;

namespace Swiftboost.Core
{
    public class AndroidBridge : MonoBehaviour
    {
        private AndroidJavaObject currentActivity;
        private AndroidJavaObject unityPlayer;
        private AndroidJavaClass buildClass;
        private AndroidJavaClass versionClass;
        
        private bool isInitialized = false;
        
        private void Start()
        {
            InitializeBridge();
        }
        
        public void InitializeBridge()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                // Get Unity Player current activity
                using (AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    currentActivity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
                }
                
                // Initialize Build class for device information
                buildClass = new AndroidJavaClass("android.os.Build");
                
                // Initialize Build.VERSION class for Android version info
                versionClass = new AndroidJavaClass("android.os.Build$VERSION");
                
                isInitialized = true;
                Debug.Log("AndroidBridge initialized successfully");
                #else
                Debug.LogWarning("AndroidBridge: Running in editor, using mock data");
                isInitialized = true;
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize AndroidBridge: {e.Message}");
                isInitialized = false;
            }
        }
        
        // Device Information Methods
        public string GetDeviceBrand()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                return buildClass?.GetStatic<string>("BRAND") ?? "Unknown";
                #else
                return "Samsung"; // Mock data for editor
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting device brand: {e.Message}");
                return "Unknown";
            }
        }
        
        public string GetDeviceModel()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                return buildClass?.GetStatic<string>("MODEL") ?? "Unknown";
                #else
                return "Galaxy S23 Ultra"; // Mock data for editor
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting device model: {e.Message}");
                return "Unknown";
            }
        }
        
        public string GetDeviceCodename()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                return buildClass?.GetStatic<string>("DEVICE") ?? "Unknown";
                #else
                return "dm3q"; // Mock data for editor
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting device codename: {e.Message}");
                return "Unknown";
            }
        }
        
        public string GetManufacturer()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                return buildClass?.GetStatic<string>("MANUFACTURER") ?? "Unknown";
                #else
                return "samsung"; // Mock data for editor
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting manufacturer: {e.Message}");
                return "Unknown";
            }
        }
        
        public string GetHardwareInfo()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                return buildClass?.GetStatic<string>("HARDWARE") ?? "Unknown";
                #else
                return "qcom"; // Mock data for editor
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting hardware info: {e.Message}");
                return "Unknown";
            }
        }
        
        public string GetChipsetInfo()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                // Try to get SOC_MODEL first (Android 12+)
                string socModel = buildClass?.GetStatic<string>("SOC_MODEL");
                if (!string.IsNullOrEmpty(socModel))
                    return socModel;
                
                // Fallback to HARDWARE
                return buildClass?.GetStatic<string>("HARDWARE") ?? "Unknown";
                #else
                return "Snapdragon 8 Gen 2"; // Mock data for editor
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting chipset info: {e.Message}");
                return "Unknown";
            }
        }
        
        public string GetAndroidVersion()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                return versionClass?.GetStatic<string>("RELEASE") ?? "Unknown";
                #else
                return "14"; // Mock data for editor
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting Android version: {e.Message}");
                return "Unknown";
            }
        }
        
        public int GetTotalRAM()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                var activityManager = currentActivity?.Call<AndroidJavaObject>("getSystemService", "activity");
                if (activityManager != null)
                {
                    var memoryInfo = new AndroidJavaObject("android.app.ActivityManager$MemoryInfo");
                    activityManager.Call("getMemoryInfo", memoryInfo);
                    long totalMem = memoryInfo.Get<long>("totalMem");
                    return (int)(totalMem / (1024 * 1024)); // Convert to MB
                }
                return 0;
                #else
                return 12288; // Mock data for editor (12GB)
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting total RAM: {e.Message}");
                return 0;
            }
        }
        
        public string GetDisplayResolution()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                var windowManager = currentActivity?.Call<AndroidJavaObject>("getWindowManager");
                var display = windowManager?.Call<AndroidJavaObject>("getDefaultDisplay");
                var displayMetrics = new AndroidJavaObject("android.util.DisplayMetrics");
                display?.Call("getMetrics", displayMetrics);
                
                int width = displayMetrics?.Get<int>("widthPixels") ?? 0;
                int height = displayMetrics?.Get<int>("heightPixels") ?? 0;
                return $"{width}x{height}";
                #else
                return "3088x1440"; // Mock data for editor
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting display resolution: {e.Message}");
                return "Unknown";
            }
        }
        
        public string GetDeviceFingerprint()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                return buildClass?.GetStatic<string>("FINGERPRINT") ?? "Unknown";
                #else
                return "samsung/dm3qxxx/dm3q:14/UP1A.231005.007/S918BXXU4CWJ4:user/release-keys"; // Mock data
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting device fingerprint: {e.Message}");
                return "Unknown";
            }
        }
        
        // Performance Monitoring Methods
        public float GetRAMUsage()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                var activityManager = currentActivity?.Call<AndroidJavaObject>("getSystemService", "activity");
                if (activityManager != null)
                {
                    var memoryInfo = new AndroidJavaObject("android.app.ActivityManager$MemoryInfo");
                    activityManager.Call("getMemoryInfo", memoryInfo);
                    long availMem = memoryInfo.Get<long>("availMem");
                    long totalMem = memoryInfo.Get<long>("totalMem");
                    return ((float)(totalMem - availMem) / totalMem) * 100f;
                }
                return 0f;
                #else
                return UnityEngine.Random.Range(40f, 70f); // Mock data for editor
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting RAM usage: {e.Message}");
                return 0f;
            }
        }
        
        public void ClearBackgroundCache()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                // This would require custom Java plugin for advanced cache clearing
                Debug.Log("Cache clearing requested");
                #else
                Debug.Log("Mock: Cache cleared");
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Error clearing cache: {e.Message}");
            }
        }
        
        // Brand Detection Helpers
        public bool IsSamsungDevice()
        {
            return GetDeviceBrand().ToLower().Contains("samsung");
        }
        
        public bool IsOnePlusDevice()
        {
            return GetDeviceBrand().ToLower().Contains("oneplus");
        }
        
        public bool IsPixelDevice()
        {
            return GetDeviceBrand().ToLower().Contains("google") || GetDeviceModel().ToLower().Contains("pixel");
        }
        
        public bool IsXiaomiDevice()
        {
            return GetDeviceBrand().ToLower().Contains("xiaomi") || GetDeviceBrand().ToLower().Contains("redmi");
        }
        
        // System Information
        public float GetDeviceTemperature()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                // This would require custom Java plugin or root access for accurate temperature
                return 35f; // Placeholder
                #else
                return UnityEngine.Random.Range(30f, 45f); // Mock data for editor
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting device temperature: {e.Message}");
                return 0f;
            }
        }
        
        public int GetRefreshRate()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                var windowManager = currentActivity?.Call<AndroidJavaObject>("getWindowManager");
                var display = windowManager?.Call<AndroidJavaObject>("getDefaultDisplay");
                return display?.Call<int>("getRefreshRate") ?? 60;
                #else
                return 120; // Mock data for editor
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting refresh rate: {e.Message}");
                return 60;
            }
        }
        
        public bool IsBluetoothEnabled()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                var bluetoothAdapter = new AndroidJavaClass("android.bluetooth.BluetoothAdapter");
                var adapter = bluetoothAdapter?.CallStatic<AndroidJavaObject>("getDefaultAdapter");
                return adapter?.Call<bool>("isEnabled") ?? false;
                #else
                return false; // Mock data for editor
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Error checking Bluetooth status: {e.Message}");
                return false;
            }
        }
        
        public float GetBrightnessLevel()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                // This would require additional permissions and setup
                return 0.5f; // Placeholder
                #else
                return 0.7f; // Mock data for editor
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting brightness level: {e.Message}");
                return 0.5f;
            }
        }
        
        public long GetAvailableStorage()
        {
            try
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                var environment = new AndroidJavaClass("android.os.Environment");
                var dataDirectory = environment?.CallStatic<AndroidJavaObject>("getDataDirectory");
                var statFs = new AndroidJavaObject("android.os.StatFs", dataDirectory?.Call<string>("getPath"));
                long blockSize = statFs?.Call<long>("getBlockSizeLong") ?? 0;
                long availableBlocks = statFs?.Call<long>("getAvailableBlocksLong") ?? 0;
                return (availableBlocks * blockSize) / (1024 * 1024 * 1024); // Convert to GB
                #else
                return 128; // Mock data for editor (128GB)
                #endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting available storage: {e.Message}");
                return 0;
            }
        }
        
        public bool IsInitialized()
        {
            return isInitialized;
        }
    }
}