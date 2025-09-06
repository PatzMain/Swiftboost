using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Swiftboost.Core;

namespace Swiftboost.Monitoring
{
    public class PerformanceLogger : MonoBehaviour
    {
        [Header("Logging Settings")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private bool logToFile = true;
        [SerializeField] private bool logToConsole = true;
        [SerializeField] private int maxLogEntries = 1000;
        
        [Header("Log Intervals")]
        [SerializeField] private float performanceLogInterval = 5f;
        [SerializeField] private float sessionSummaryInterval = 60f;
        
        // Performance data storage
        private List<PerformanceLogEntry> performanceLog;
        private List<GameSessionSummary> sessionSummaries;
        private string logFilePath;
        
        // References
        private UsageMonitor usageMonitor;
        private DeviceProfiler deviceProfiler;
        private GameSession currentGameSession;
        
        // Coroutines
        private Coroutine performanceLoggingCoroutine;
        private Coroutine sessionSummaryCoroutine;
        
        public bool IsLogging => enableLogging;
        public int LogEntryCount => performanceLog?.Count ?? 0;
        
        private void Start()
        {
            InitializeLogger();
        }
        
        private void InitializeLogger()
        {
            performanceLog = new List<PerformanceLogEntry>();
            sessionSummaries = new List<GameSessionSummary>();
            
            // Get references
            usageMonitor = SystemManager.Instance?.UsageMonitor;
            deviceProfiler = SystemManager.Instance?.DeviceProfiler;
            
            if (usageMonitor == null || deviceProfiler == null)
            {
                Debug.LogError("PerformanceLogger: Required components not found!");
                return;
            }
            
            // Setup log file path
            SetupLogFile();
            
            // Start logging if enabled
            if (enableLogging)
            {
                StartLogging();
            }
            
            Debug.Log($"PerformanceLogger initialized. Log file: {logFilePath}");
        }
        
        private void SetupLogFile()
        {
            string logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
            
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string deviceName = deviceProfiler?.GetCurrentDevice()?.GetDeviceIdentifier() ?? "unknown_device";
            logFilePath = Path.Combine(logDirectory, $"performance_log_{deviceName}_{timestamp}.txt");
        }
        
        public void StartLogging()
        {
            if (enableLogging && performanceLoggingCoroutine == null)
            {
                performanceLoggingCoroutine = StartCoroutine(PerformanceLoggingRoutine());
                sessionSummaryCoroutine = StartCoroutine(SessionSummaryRoutine());
                Debug.Log("Performance logging started");
            }
        }
        
        public void StopLogging()
        {
            if (performanceLoggingCoroutine != null)
            {
                StopCoroutine(performanceLoggingCoroutine);
                performanceLoggingCoroutine = null;
            }
            
            if (sessionSummaryCoroutine != null)
            {
                StopCoroutine(sessionSummaryCoroutine);
                sessionSummaryCoroutine = null;
            }
            
            Debug.Log("Performance logging stopped");
        }
        
        private System.Collections.IEnumerator PerformanceLoggingRoutine()
        {
            while (enableLogging)
            {
                yield return new WaitForSeconds(performanceLogInterval);
                LogCurrentPerformance();
            }
        }
        
        private System.Collections.IEnumerator SessionSummaryRoutine()
        {
            while (enableLogging)
            {
                yield return new WaitForSeconds(sessionSummaryInterval);
                GenerateSessionSummary();
            }
        }
        
        private void LogCurrentPerformance()
        {
            if (usageMonitor == null) return;
            
            var metrics = usageMonitor.GetCurrentMetrics();
            var logEntry = new PerformanceLogEntry
            {
                timestamp = DateTime.Now,
                fps = metrics.fps,
                ramUsage = metrics.ramUsage,
                cpuUsage = metrics.cpuUsage,
                gpuUsage = metrics.gpuUsage,
                temperature = metrics.temperature,
                deviceName = metrics.deviceName,
                sessionId = currentGameSession?.gameName ?? "System"
            };
            
            AddLogEntry(logEntry);
            
            // Add to current game session if active
            if (currentGameSession != null)
            {
                currentGameSession.AddPerformanceDataPoint(
                    metrics.fps,
                    metrics.ramUsage,
                    metrics.cpuUsage,
                    metrics.gpuUsage,
                    metrics.temperature
                );
            }
        }
        
        private void AddLogEntry(PerformanceLogEntry entry)
        {
            performanceLog.Add(entry);
            
            // Remove old entries if we exceed the limit
            if (performanceLog.Count > maxLogEntries)
            {
                performanceLog.RemoveAt(0);
            }
            
            // Log to console if enabled
            if (logToConsole)
            {
                Debug.Log($"Performance: FPS={entry.fps:F1}, RAM={entry.ramUsage:F1}%, CPU={entry.cpuUsage:F1}%, Temp={entry.temperature:F1}°C");
            }
            
            // Log to file if enabled
            if (logToFile)
            {
                WriteToLogFile(entry);
            }
        }
        
        private void WriteToLogFile(PerformanceLogEntry entry)
        {
            try
            {
                string logLine = $"{entry.timestamp:yyyy-MM-dd HH:mm:ss.fff}|{entry.deviceName}|{entry.sessionId}|" +
                               $"FPS:{entry.fps:F2}|RAM:{entry.ramUsage:F2}|CPU:{entry.cpuUsage:F2}|" +
                               $"GPU:{entry.gpuUsage:F2}|TEMP:{entry.temperature:F2}";
                
                File.AppendAllText(logFilePath, logLine + Environment.NewLine);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write to log file: {e.Message}");
            }
        }
        
        public void StartGameSession(GameSession gameSession)
        {
            currentGameSession = gameSession;
            
            var startEntry = new PerformanceLogEntry
            {
                timestamp = DateTime.Now,
                deviceName = deviceProfiler?.GetCurrentDevice()?.GetFullDeviceName() ?? "Unknown",
                sessionId = gameSession.gameName,
                fps = 0,
                ramUsage = 0,
                cpuUsage = 0,
                gpuUsage = 0,
                temperature = 0
            };
            
            AddLogEntry(startEntry);
            
            if (logToFile)
            {
                string sessionStartLog = $"=== GAME SESSION STARTED: {gameSession.gameName} ===";
                File.AppendAllText(logFilePath, sessionStartLog + Environment.NewLine);
            }
            
            Debug.Log($"Started logging for game session: {gameSession.gameName}");
        }
        
        public void EndGameSession()
        {
            if (currentGameSession == null) return;
            
            var endEntry = new PerformanceLogEntry
            {
                timestamp = DateTime.Now,
                deviceName = deviceProfiler?.GetCurrentDevice()?.GetFullDeviceName() ?? "Unknown",
                sessionId = currentGameSession.gameName,
                fps = 0,
                ramUsage = 0,
                cpuUsage = 0,
                gpuUsage = 0,
                temperature = 0
            };
            
            AddLogEntry(endEntry);
            
            if (logToFile)
            {
                string sessionEndLog = $"=== GAME SESSION ENDED: {currentGameSession.gameName} ===";
                File.AppendAllText(logFilePath, sessionEndLog + Environment.NewLine);
                
                // Write session report to file
                string sessionReport = currentGameSession.GenerateSessionReport();
                File.AppendAllText(logFilePath, sessionReport + Environment.NewLine);
            }
            
            Debug.Log($"Ended logging for game session: {currentGameSession.gameName}");
            currentGameSession = null;
        }
        
        private void GenerateSessionSummary()
        {
            if (performanceLog.Count == 0) return;
            
            var recentEntries = performanceLog.FindAll(entry => 
                (DateTime.Now - entry.timestamp).TotalMinutes <= 1);
            
            if (recentEntries.Count == 0) return;
            
            var summary = new GameSessionSummary
            {
                timestamp = DateTime.Now,
                deviceName = deviceProfiler?.GetCurrentDevice()?.GetFullDeviceName() ?? "Unknown",
                sessionId = currentGameSession?.gameName ?? "System",
                entryCount = recentEntries.Count,
                averageFPS = recentEntries.Average(e => e.fps),
                averageRAM = recentEntries.Average(e => e.ramUsage),
                averageCPU = recentEntries.Average(e => e.cpuUsage),
                averageTemperature = recentEntries.Average(e => e.temperature),
                maxFPS = recentEntries.Max(e => e.fps),
                minFPS = recentEntries.Min(e => e.fps),
                maxTemperature = recentEntries.Max(e => e.temperature)
            };
            
            sessionSummaries.Add(summary);
            
            // Keep only recent summaries
            if (sessionSummaries.Count > 100)
            {
                sessionSummaries.RemoveAt(0);
            }
        }
        
        public List<PerformanceLogEntry> GetRecentEntries(int count = 100)
        {
            if (performanceLog.Count <= count)
                return new List<PerformanceLogEntry>(performanceLog);
            
            return performanceLog.GetRange(performanceLog.Count - count, count);
        }
        
        public List<GameSessionSummary> GetSessionSummaries(int count = 20)
        {
            if (sessionSummaries.Count <= count)
                return new List<GameSessionSummary>(sessionSummaries);
            
            return sessionSummaries.GetRange(sessionSummaries.Count - count, count);
        }
        
        public string ExportLogData()
        {
            var deviceInfo = deviceProfiler?.GetCurrentDevice();
            
            string export = $"Swiftboost Performance Log Export\n";
            export += $"Device: {deviceInfo?.GetFullDeviceName() ?? "Unknown"}\n";
            export += $"Export Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
            export += $"Total Entries: {performanceLog.Count}\n\n";
            
            export += "Recent Performance Data:\n";
            export += "Timestamp|Device|Session|FPS|RAM%|CPU%|GPU%|Temp°C\n";
            
            var recentEntries = GetRecentEntries(50);
            foreach (var entry in recentEntries)
            {
                export += $"{entry.timestamp:yyyy-MM-dd HH:mm:ss}|{entry.deviceName}|{entry.sessionId}|" +
                         $"{entry.fps:F1}|{entry.ramUsage:F1}|{entry.cpuUsage:F1}|{entry.gpuUsage:F1}|{entry.temperature:F1}\n";
            }
            
            return export;
        }
        
        public void ClearLogs()
        {
            performanceLog.Clear();
            sessionSummaries.Clear();
            Debug.Log("Performance logs cleared");
        }
        
        private void OnDestroy()
        {
            StopLogging();
        }
    }
    
    [System.Serializable]
    public class PerformanceLogEntry
    {
        public DateTime timestamp;
        public string deviceName;
        public string sessionId;
        public float fps;
        public float ramUsage;
        public float cpuUsage;
        public float gpuUsage;
        public float temperature;
    }
    
    [System.Serializable]
    public class GameSessionSummary
    {
        public DateTime timestamp;
        public string deviceName;
        public string sessionId;
        public int entryCount;
        public float averageFPS;
        public float averageRAM;
        public float averageCPU;
        public float averageTemperature;
        public float maxFPS;
        public float minFPS;
        public float maxTemperature;
        
        public string GetSummaryText()
        {
            return $"[{timestamp:HH:mm:ss}] {sessionId} - FPS: {averageFPS:F1} avg ({minFPS:F0}-{maxFPS:F0}), " +
                   $"RAM: {averageRAM:F1}%, CPU: {averageCPU:F1}%, Temp: {averageTemperature:F1}°C (max: {maxTemperature:F1}°C)";
        }
    }
}