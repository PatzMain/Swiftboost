using System;
using System.Collections;
using UnityEngine;

namespace Swiftboost.Core
{
    public class DeviceProfiler : MonoBehaviour
    {
        [System.Serializable]
        public class DeviceInfo
        {
            public string brand;
            public string model;
            public string codename;
            public string androidVersion;
            public string manufacturer;
            public string hardware;
            public string chipset;
            public int totalRAM;
            public string displayResolution;
            public string deviceFingerprint;
            
            public string GetFullDeviceName()
            {
                return $"{brand} {model}";
            }
            
            public string GetDetailedInfo()
            {
                return $"{brand} {model} ({codename}) - Android {androidVersion}";
            }
            
            public string GetDeviceIdentifier()
            {
                return $"{brand}_{model}_{codename}".Replace(" ", "_").ToLower();
            }
        }
        
        private DeviceInfo currentDevice;
        private AndroidBridge androidBridge;
        
        public event Action<DeviceInfo> OnDeviceProfileComplete;
        
        private void Start()
        {
            androidBridge = FindObjectOfType<AndroidBridge>();
            if (androidBridge == null)
            {
                Debug.LogError("AndroidBridge not found! DeviceProfiler requires AndroidBridge to function.");
                return;
            }
            
            StartCoroutine(InitializeDeviceProfileAsync());
        }
        
        private IEnumerator InitializeDeviceProfileAsync()
        {
            yield return new WaitForSeconds(0.5f); // Allow AndroidBridge to initialize
            
            InitializeDeviceProfile();
            OnDeviceProfileComplete?.Invoke(currentDevice);
        }
        
        public void InitializeDeviceProfile()
        {
            if (androidBridge == null)
            {
                Debug.LogError("AndroidBridge not available for device profiling!");
                return;
            }
            
            currentDevice = new DeviceInfo
            {
                brand = androidBridge.GetDeviceBrand(),
                model = androidBridge.GetDeviceModel(),
                codename = androidBridge.GetDeviceCodename(),
                androidVersion = androidBridge.GetAndroidVersion(),
                manufacturer = androidBridge.GetManufacturer(),
                hardware = androidBridge.GetHardwareInfo(),
                chipset = androidBridge.GetChipsetInfo(),
                totalRAM = androidBridge.GetTotalRAM(),
                displayResolution = androidBridge.GetDisplayResolution(),
                deviceFingerprint = androidBridge.GetDeviceFingerprint()
            };
            
            Debug.Log($"Device Profile Complete: {currentDevice.GetDetailedInfo()}");
            Debug.Log($"Chipset: {currentDevice.chipset}, RAM: {currentDevice.totalRAM}MB");
            Debug.Log($"Resolution: {currentDevice.displayResolution}");
        }
        
        public DeviceInfo GetCurrentDevice()
        {
            return currentDevice;
        }
        
        public bool IsDeviceProfiled()
        {
            return currentDevice != null && !string.IsNullOrEmpty(currentDevice.brand);
        }
        
        public bool IsHighEndDevice()
        {
            if (currentDevice == null) return false;
            
            return currentDevice.totalRAM >= 8192 && // 8GB+ RAM
                   (currentDevice.chipset.Contains("Snapdragon 8") ||
                    currentDevice.chipset.Contains("Tensor") ||
                    currentDevice.chipset.Contains("Exynos 22") ||
                    currentDevice.chipset.Contains("A15") ||
                    currentDevice.chipset.Contains("A16") ||
                    currentDevice.chipset.Contains("A17"));
        }
        
        public bool IsMidRangeDevice()
        {
            if (currentDevice == null) return false;
            
            return currentDevice.totalRAM >= 4096 && currentDevice.totalRAM < 8192; // 4-8GB RAM
        }
        
        public bool IsBudgetDevice()
        {
            if (currentDevice == null) return false;
            
            return currentDevice.totalRAM < 4096; // Less than 4GB RAM
        }
    }
}