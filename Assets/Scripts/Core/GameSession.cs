using System;
using System.Collections.Generic;
using UnityEngine;

namespace Swiftboost.Core
{
    [System.Serializable]
    public class GameSession
    {
        [Header("Game Information")]
        public string gameName;
        public DateTime startTime;
        public DateTime endTime;
        
        [Header("Device Information")]
        public string deviceBrand;
        public string deviceModel;
        public string deviceCodename;
        public string androidVersion;
        public string manufacturer;
        public string chipset;
        public int totalDeviceRAM;
        public string displayResolution;
        
        [Header("Performance Metrics")]
        public float averageFPS;
        public float minFPS;
        public float maxFPS;
        public float averageRAMUsage;
        public float averageCPUUsage;
        public float averageGPUUsage;
        public float averageTemperature;
        public float maxTemperature;
        public int batteryConsumption; // mAh
        public float batteryPercentageUsed;
        
        [Header("Optimization Applied")]
        public bool drsUsed;
        public bool lodUsed;
        public bool thermalThrottled;
        public int thermalThrottleEvents;
        public bool deviceSpecificOptimizationUsed;
        
        [Header("Device Performance Score")]
        public int devicePerformanceScore; // 0-100
        public string performanceRating; // Excellent, Good, Fair, Poor
        
        [Header("Session Analytics")]
        public List<PerformanceDataPoint> performanceHistory;
        public List<ThermalEvent> thermalEvents;
        public List<OptimizationEvent> optimizationEvents;
        
        public GameSession()
        {
            performanceHistory = new List<PerformanceDataPoint>();
            thermalEvents = new List<ThermalEvent>();
            optimizationEvents = new List<OptimizationEvent>();
        }
        
        public GameSession(string gameName, DeviceProfiler.DeviceInfo deviceInfo)
        {
            this.gameName = gameName;
            startTime = DateTime.Now;
            
            // Copy device information
            deviceBrand = deviceInfo.brand;
            deviceModel = deviceInfo.model;
            deviceCodename = deviceInfo.codename;
            androidVersion = deviceInfo.androidVersion;
            manufacturer = deviceInfo.manufacturer;
            chipset = deviceInfo.chipset;
            totalDeviceRAM = deviceInfo.totalRAM;
            displayResolution = deviceInfo.displayResolution;
            
            performanceHistory = new List<PerformanceDataPoint>();
            thermalEvents = new List<ThermalEvent>();
            optimizationEvents = new List<OptimizationEvent>();
        }
        
        public TimeSpan SessionDuration => endTime - startTime;
        
        public string GetFullDeviceName()
        {
            return $"{deviceBrand} {deviceModel}";
        }
        
        public string GetDeviceIdentifier()
        {
            return $"{deviceBrand}_{deviceModel}_{deviceCodename}".Replace(" ", "_").ToLower();
        }
        
        public void StartSession()
        {
            startTime = DateTime.Now;
            Debug.Log($"Game session started for {gameName} on {GetFullDeviceName()}");
        }
        
        public void EndSession()
        {
            endTime = DateTime.Now;
            CalculateFinalMetrics();
            CalculatePerformanceScore();
            Debug.Log($"Game session ended for {gameName}. Duration: {SessionDuration:mm\\:ss}");
        }
        
        public void AddPerformanceDataPoint(float fps, float ramUsage, float cpuUsage, float gpuUsage, float temperature)
        {
            var dataPoint = new PerformanceDataPoint
            {
                timestamp = DateTime.Now,
                fps = fps,
                ramUsage = ramUsage,
                cpuUsage = cpuUsage,
                gpuUsage = gpuUsage,
                temperature = temperature
            };
            
            performanceHistory.Add(dataPoint);
            
            // Keep only recent data to manage memory
            if (performanceHistory.Count > 1000)
            {
                performanceHistory.RemoveAt(0);
            }
        }
        
        public void AddThermalEvent(float temperature, string eventType, string action)
        {
            var thermalEvent = new ThermalEvent
            {
                timestamp = DateTime.Now,
                temperature = temperature,
                eventType = eventType,
                actionTaken = action
            };
            
            thermalEvents.Add(thermalEvent);
            
            if (eventType == "Throttle")
            {
                thermalThrottleEvents++;
                thermalThrottled = true;
            }
        }
        
        public void AddOptimizationEvent(string optimizationType, string details, float performanceImprovement)
        {
            var optimizationEvent = new OptimizationEvent
            {
                timestamp = DateTime.Now,
                optimizationType = optimizationType,
                details = details,
                performanceImprovement = performanceImprovement
            };
            
            optimizationEvents.Add(optimizationEvent);
            
            // Set optimization flags
            switch (optimizationType.ToLower())
            {
                case "drs":
                    drsUsed = true;
                    break;
                case "lod":
                    lodUsed = true;
                    break;
                case "device_specific":
                    deviceSpecificOptimizationUsed = true;
                    break;
            }
        }
        
        private void CalculateFinalMetrics()
        {
            if (performanceHistory.Count == 0) return;
            
            // Calculate average metrics
            float totalFPS = 0f, totalRAM = 0f, totalCPU = 0f, totalGPU = 0f, totalTemp = 0f;
            minFPS = float.MaxValue;
            maxFPS = float.MinValue;
            maxTemperature = float.MinValue;
            
            foreach (var dataPoint in performanceHistory)
            {
                totalFPS += dataPoint.fps;
                totalRAM += dataPoint.ramUsage;
                totalCPU += dataPoint.cpuUsage;
                totalGPU += dataPoint.gpuUsage;
                totalTemp += dataPoint.temperature;
                
                if (dataPoint.fps < minFPS) minFPS = dataPoint.fps;
                if (dataPoint.fps > maxFPS) maxFPS = dataPoint.fps;
                if (dataPoint.temperature > maxTemperature) maxTemperature = dataPoint.temperature;
            }
            
            int count = performanceHistory.Count;
            averageFPS = totalFPS / count;
            averageRAMUsage = totalRAM / count;
            averageCPUUsage = totalCPU / count;
            averageGPUUsage = totalGPU / count;
            averageTemperature = totalTemp / count;
        }
        
        private void CalculatePerformanceScore()
        {
            float score = 0f;
            
            // FPS Score (40%)
            float fpsScore = Mathf.Clamp01(averageFPS / 60f) * 40f;
            score += fpsScore;
            
            // Thermal Score (25%)
            float thermalScore = Mathf.Clamp01((50f - averageTemperature) / 20f) * 25f;
            score += thermalScore;
            
            // RAM Usage Score (20%)
            float ramScore = Mathf.Clamp01((100f - averageRAMUsage) / 100f) * 20f;
            score += ramScore;
            
            // Stability Score (15%) - based on FPS consistency
            float fpsVariance = maxFPS - minFPS;
            float stabilityScore = Mathf.Clamp01((60f - fpsVariance) / 60f) * 15f;
            score += stabilityScore;
            
            devicePerformanceScore = Mathf.RoundToInt(score);
            
            // Determine performance rating
            if (devicePerformanceScore >= 90)
                performanceRating = "Excellent";
            else if (devicePerformanceScore >= 75)
                performanceRating = "Good";
            else if (devicePerformanceScore >= 60)
                performanceRating = "Fair";
            else
                performanceRating = "Poor";
        }
        
        public string GenerateSessionReport()
        {
            var report = $"═══════════════════════════════════════\n";
            report += $"🎮 GAME SESSION REPORT\n";
            report += $"═══════════════════════════════════════\n";
            report += $"Device: {GetFullDeviceName()}\n";
            report += $"Model: {deviceModel} ({deviceCodename})\n";
            report += $"Chipset: {chipset}\n";
            report += $"Android: {androidVersion}\n";
            report += $"RAM: {totalDeviceRAM}MB | Display: {displayResolution}\n";
            report += $"\n";
            report += $"Game: {gameName}\n";
            report += $"Duration: {SessionDuration:mm\\:ss}\n";
            report += $"Start Time: {startTime:yyyy-MM-dd HH:mm:ss}\n";
            report += $"End Time: {endTime:yyyy-MM-dd HH:mm:ss}\n";
            report += $"\n";
            report += $"📊 PERFORMANCE METRICS\n";
            report += $"───────────────────────────────────────\n";
            report += $"FPS:         {averageFPS:F1} avg ({minFPS:F0}-{maxFPS:F0} range)\n";
            report += $"RAM Usage:   {averageRAMUsage:F1}% avg\n";
            report += $"CPU Usage:   {averageCPUUsage:F1}% avg\n";
            report += $"GPU Usage:   {averageGPUUsage:F1}% avg\n";
            report += $"Temperature: {averageTemperature:F1}°C avg\n";
            report += $"Max Temp:    {maxTemperature:F1}°C\n";
            report += $"\n";
            report += $"🚀 OPTIMIZATIONS APPLIED\n";
            report += $"───────────────────────────────────────\n";
            report += $"{(drsUsed ? "✅" : "❌")} Dynamic Resolution Scaling\n";
            report += $"{(lodUsed ? "✅" : "❌")} Level of Detail Optimization\n";
            report += $"{(thermalThrottled ? "✅" : "❌")} Thermal Throttling ({thermalThrottleEvents} events)\n";
            report += $"{(deviceSpecificOptimizationUsed ? "✅" : "❌")} Device-Specific Optimization\n";
            report += $"\n";
            report += $"🏆 DEVICE PERFORMANCE SCORE: {devicePerformanceScore}/100\n";
            report += $"📈 Rating: {performanceRating} ({GetFullDeviceName()} Optimized)\n";
            report += $"═══════════════════════════════════════\n";
            
            return report;
        }
    }
    
    [System.Serializable]
    public class PerformanceDataPoint
    {
        public DateTime timestamp;
        public float fps;
        public float ramUsage;
        public float cpuUsage;
        public float gpuUsage;
        public float temperature;
    }
    
    [System.Serializable]
    public class ThermalEvent
    {
        public DateTime timestamp;
        public float temperature;
        public string eventType; // Warning, Throttle, Recovery
        public string actionTaken;
    }
    
    [System.Serializable]
    public class OptimizationEvent
    {
        public DateTime timestamp;
        public string optimizationType;
        public string details;
        public float performanceImprovement;
    }
}