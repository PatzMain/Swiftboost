using System.Collections;
using UnityEngine;
using Swiftboost.Monitoring;
using Swiftboost.Game;
using Swiftboost.Settings;
using Swiftboost.Optimization;

namespace Swiftboost.Core
{
    public class SystemManager : MonoBehaviour
    {
        [Header("Core Components")]
        [SerializeField] private AndroidBridge androidBridge;
        [SerializeField] private DeviceProfiler deviceProfiler;
        
        [Header("System Modules")]
        [SerializeField] private UsageMonitor usageMonitor;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private SettingsManager settingsManager;
        [SerializeField] private OptimizationHandler optimizationHandler;
        
        [Header("System Status")]
        [SerializeField] private bool isSystemInitialized = false;
        [SerializeField] private bool isOptimizationActive = false;
        
        public static SystemManager Instance { get; private set; }
        
        // Properties
        public AndroidBridge AndroidBridge => androidBridge;
        public DeviceProfiler DeviceProfiler => deviceProfiler;
        public UsageMonitor UsageMonitor => usageMonitor;
        public GameManager GameManager => gameManager;
        public SettingsManager SettingsManager => settingsManager;
        public OptimizationHandler OptimizationHandler => optimizationHandler;
        
        public bool IsSystemInitialized => isSystemInitialized;
        public bool IsOptimizationActive => isOptimizationActive;
        
        // Events
        public System.Action OnSystemInitialized;
        public System.Action<DeviceProfiler.DeviceInfo> OnDeviceProfiled;
        public System.Action OnOptimizationStarted;
        public System.Action OnOptimizationStopped;
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeSystem()
        {
            Debug.Log("Swiftboost System Manager: Initializing...");
            StartCoroutine(InitializeSystemAsync());
        }
        
        private IEnumerator InitializeSystemAsync()
        {
            // Step 1: Initialize Android Bridge
            if (androidBridge == null)
                androidBridge = FindObjectOfType<AndroidBridge>();
            
            if (androidBridge == null)
            {
                GameObject bridgeGO = new GameObject("AndroidBridge");
                bridgeGO.transform.SetParent(transform);
                androidBridge = bridgeGO.AddComponent<AndroidBridge>();
            }
            
            yield return new WaitForSeconds(0.5f);
            
            // Step 2: Initialize Device Profiler
            if (deviceProfiler == null)
                deviceProfiler = FindObjectOfType<DeviceProfiler>();
            
            if (deviceProfiler == null)
            {
                GameObject profilerGO = new GameObject("DeviceProfiler");
                profilerGO.transform.SetParent(transform);
                deviceProfiler = profilerGO.AddComponent<DeviceProfiler>();
            }
            
            // Wait for device profiling to complete
            yield return new WaitUntil(() => deviceProfiler.IsDeviceProfiled());
            OnDeviceProfiled?.Invoke(deviceProfiler.GetCurrentDevice());
            
            // Step 3: Initialize Settings Manager
            if (settingsManager == null)
                settingsManager = FindObjectOfType<SettingsManager>();
            
            if (settingsManager == null)
            {
                GameObject settingsGO = new GameObject("SettingsManager");
                settingsGO.transform.SetParent(transform);
                settingsManager = settingsGO.AddComponent<SettingsManager>();
            }
            
            yield return new WaitForSeconds(0.2f);
            
            // Step 4: Initialize Usage Monitor
            if (usageMonitor == null)
                usageMonitor = FindObjectOfType<UsageMonitor>();
            
            if (usageMonitor == null)
            {
                GameObject monitorGO = new GameObject("UsageMonitor");
                monitorGO.transform.SetParent(transform);
                usageMonitor = monitorGO.AddComponent<UsageMonitor>();
            }
            
            // Step 5: Initialize Game Manager
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();
            
            if (gameManager == null)
            {
                GameObject gameGO = new GameObject("GameManager");
                gameGO.transform.SetParent(transform);
                gameManager = gameGO.AddComponent<GameManager>();
            }
            
            // Step 6: Initialize Optimization Handler
            if (optimizationHandler == null)
                optimizationHandler = FindObjectOfType<OptimizationHandler>();
            
            if (optimizationHandler == null)
            {
                GameObject optimizationGO = new GameObject("OptimizationHandler");
                optimizationGO.transform.SetParent(transform);
                optimizationHandler = optimizationGO.AddComponent<OptimizationHandler>();
            }
            
            yield return new WaitForSeconds(0.5f);
            
            // Step 7: Initialize device-specific optimizations
            InitializeDeviceSpecificOptimizations();
            
            // Step 8: Start initial cache clearing
            PerformInitialOptimization();
            
            // Mark system as initialized
            isSystemInitialized = true;
            OnSystemInitialized?.Invoke();
            
            Debug.Log($"Swiftboost System Manager: Initialization Complete");
            Debug.Log($"Device: {deviceProfiler.GetCurrentDevice().GetDetailedInfo()}");
        }
        
        private void InitializeDeviceSpecificOptimizations()
        {
            var deviceInfo = deviceProfiler.GetCurrentDevice();
            if (deviceInfo == null) return;
            
            Debug.Log($"Initializing optimizations for: {deviceInfo.GetFullDeviceName()}");
            
            // Apply device-specific settings
            optimizationHandler?.InitializeForDevice(deviceInfo);
        }
        
        private void PerformInitialOptimization()
        {
            Debug.Log("Performing initial system optimization...");
            
            // Clear background cache on app launch
            androidBridge?.ClearBackgroundCache();
            
            // Start background monitoring
            usageMonitor?.StartMonitoring();
            
            Debug.Log("Initial optimization complete");
        }
        
        public void StartOptimization()
        {
            if (!isSystemInitialized)
            {
                Debug.LogWarning("Cannot start optimization: System not initialized");
                return;
            }
            
            if (isOptimizationActive)
            {
                Debug.LogWarning("Optimization already active");
                return;
            }
            
            Debug.Log("Starting system optimization...");
            
            isOptimizationActive = true;
            
            // Start optimization modules
            optimizationHandler?.StartOptimization();
            usageMonitor?.EnableOptimizationMode();
            
            OnOptimizationStarted?.Invoke();
            
            Debug.Log("System optimization started");
        }
        
        public void StopOptimization()
        {
            if (!isOptimizationActive)
            {
                Debug.LogWarning("Optimization not active");
                return;
            }
            
            Debug.Log("Stopping system optimization...");
            
            isOptimizationActive = false;
            
            // Stop optimization modules
            optimizationHandler?.StopOptimization();
            usageMonitor?.DisableOptimizationMode();
            
            OnOptimizationStopped?.Invoke();
            
            Debug.Log("System optimization stopped");
        }
        
        public void RestartOptimization()
        {
            Debug.Log("Restarting system optimization...");
            StopOptimization();
            StartCoroutine(RestartOptimizationDelayed());
        }
        
        private IEnumerator RestartOptimizationDelayed()
        {
            yield return new WaitForSeconds(1f);
            StartOptimization();
        }
        
        public void ClearAllCaches()
        {
            Debug.Log("Clearing all system caches...");
            androidBridge?.ClearBackgroundCache();
            // Additional cache clearing logic here
        }
        
        public DeviceProfiler.DeviceInfo GetCurrentDeviceInfo()
        {
            return deviceProfiler?.GetCurrentDevice();
        }
        
        public string GetSystemStatus()
        {
            var deviceInfo = GetCurrentDeviceInfo();
            var status = $"System Status:\n";
            status += $"Device: {deviceInfo?.GetFullDeviceName() ?? "Unknown"}\n";
            status += $"Initialized: {isSystemInitialized}\n";
            status += $"Optimization: {(isOptimizationActive ? "Active" : "Inactive")}\n";
            status += $"RAM Usage: {androidBridge?.GetRAMUsage() ?? 0:F1}%\n";
            
            return status;
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // App paused - reduce monitoring frequency
                usageMonitor?.SetLowPowerMode(true);
            }
            else
            {
                // App resumed - restore normal monitoring
                usageMonitor?.SetLowPowerMode(false);
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                // App gained focus - perform quick optimization
                if (isOptimizationActive)
                {
                    PerformInitialOptimization();
                }
            }
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                StopOptimization();
                Instance = null;
            }
        }
    }
}