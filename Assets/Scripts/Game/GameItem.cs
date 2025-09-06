using System;
using UnityEngine;
using Swiftboost.Core;

namespace Swiftboost.Game
{
    [System.Serializable]
    public class GameItem : MonoBehaviour
    {
        [Header("Game Information")]
        public string gameName;
        public string packageName;
        public GameGenre genre;
        public string description;
        public Sprite gameIcon;
        
        [Header("Device Compatibility")]
        public int compatibilityScore; // 0-100
        public string performanceRating; // Excellent, Good, Fair, Poor
        public string deviceName; // Device this was tested on
        public bool isInstalled = true;
        
        [Header("Performance Metrics")]
        public float averagePerformanceScore = 0f;
        public float bestFPS = 0f;
        public float averageFPS = 0f;
        public float averageTemperature = 0f;
        public float averageRAMUsage = 0f;
        
        [Header("Usage Statistics")]
        public DateTime lastPlayedDate;
        public TimeSpan totalPlayTime;
        public int totalSessions = 0;
        public bool isFavorite = false;
        
        [Header("Optimization Settings")]
        public bool enableDRS = true;
        public bool enableLOD = true;
        public bool enableThermalThrottling = true;
        public OptimizationPreference optimizationPreference = OptimizationPreference.Balanced;
        
        [Header("UI References")]
        [SerializeField] private UnityEngine.UI.Image iconImage;
        [SerializeField] private TMPro.TextMeshProUGUI nameText;
        [SerializeField] private TMPro.TextMeshProUGUI genreText;
        [SerializeField] private TMPro.TextMeshProUGUI compatibilityText;
        [SerializeField] private TMPro.TextMeshProUGUI performanceText;
        [SerializeField] private UnityEngine.UI.Button launchButton;
        [SerializeField] private UnityEngine.UI.Toggle favoriteToggle;
        
        // Events
        public event Action<GameItem> OnGameLaunchRequested;
        public event Action<GameItem> OnFavoriteToggled;
        
        public enum GameGenre
        {
            Action,
            RPG,
            Strategy,
            Casual,
            Puzzle,
            Racing,
            Sports,
            Simulation,
            Adventure,
            MOBA,
            BR, // Battle Royale
            MMO,
            Sandbox,
            AR, // Augmented Reality
            Platformer
        }
        
        public enum OptimizationPreference
        {
            BatterySaver,
            Balanced,
            Performance,
            Quality
        }
        
        private void Start()
        {
            InitializeGameItem();
        }
        
        private void InitializeGameItem()
        {
            UpdateUI();
            SetupEventListeners();
        }
        
        private void UpdateUI()
        {
            if (nameText != null)
                nameText.text = gameName;
            
            if (genreText != null)
                genreText.text = genre.ToString();
            
            if (compatibilityText != null)
            {
                compatibilityText.text = $"Compatibility: {compatibilityScore}%";
                
                // Color code based on compatibility
                if (compatibilityScore >= 80)
                    compatibilityText.color = Color.green;
                else if (compatibilityScore >= 60)
                    compatibilityText.color = Color.yellow;
                else
                    compatibilityText.color = Color.red;
            }
            
            if (performanceText != null)
                performanceText.text = performanceRating;
            
            if (iconImage != null && gameIcon != null)
                iconImage.sprite = gameIcon;
            
            if (favoriteToggle != null)
                favoriteToggle.isOn = isFavorite;
            
            if (launchButton != null)
                launchButton.interactable = isInstalled;
        }
        
        private void SetupEventListeners()
        {
            if (launchButton != null)
                launchButton.onClick.AddListener(OnLaunchButtonClick);
            
            if (favoriteToggle != null)
                favoriteToggle.onValueChanged.AddListener(OnFavoriteToggleChanged);
        }
        
        private void OnLaunchButtonClick()
        {
            if (isInstalled)
            {
                OnGameLaunchRequested?.Invoke(this);
                Debug.Log($"Launch requested for: {gameName}");
            }
            else
            {
                Debug.LogWarning($"Cannot launch {gameName} - not installed");
            }
        }
        
        private void OnFavoriteToggleChanged(bool value)
        {
            isFavorite = value;
            OnFavoriteToggled?.Invoke(this);
            Debug.Log($"{gameName} favorite status: {isFavorite}");
        }
        
        public void SetPerformanceRating(DeviceProfiler.DeviceInfo deviceInfo)
        {
            if (deviceInfo == null) return;
            
            deviceName = deviceInfo.GetFullDeviceName();
            
            // Calculate performance rating based on device and game requirements
            if (compatibilityScore >= 85)
                performanceRating = "Excellent";
            else if (compatibilityScore >= 70)
                performanceRating = "Good";
            else if (compatibilityScore >= 50)
                performanceRating = "Fair";
            else
                performanceRating = "Poor";
            
            // Adjust based on device tier
            if (deviceInfo.totalRAM >= 12288) // High-end device
            {
                if (performanceRating == "Fair")
                    performanceRating = "Good";
                else if (performanceRating == "Poor")
                    performanceRating = "Fair";
            }
            else if (deviceInfo.totalRAM < 4096) // Budget device
            {
                if (performanceRating == "Excellent")
                    performanceRating = "Good";
                else if (performanceRating == "Good")
                    performanceRating = "Fair";
            }
        }
        
        public void UpdatePerformanceMetrics(GameSession session)
        {
            if (session == null || session.gameName != gameName) return;
            
            // Update performance metrics from session
            if (averagePerformanceScore == 0f)
                averagePerformanceScore = session.devicePerformanceScore;
            else
                averagePerformanceScore = (averagePerformanceScore + session.devicePerformanceScore) / 2f;
            
            if (session.averageFPS > bestFPS)
                bestFPS = session.averageFPS;
            
            if (averageFPS == 0f)
                averageFPS = session.averageFPS;
            else
                averageFPS = (averageFPS + session.averageFPS) / 2f;
            
            if (averageTemperature == 0f)
                averageTemperature = session.averageTemperature;
            else
                averageTemperature = (averageTemperature + session.averageTemperature) / 2f;
            
            if (averageRAMUsage == 0f)
                averageRAMUsage = session.averageRAMUsage;
            else
                averageRAMUsage = (averageRAMUsage + session.averageRAMUsage) / 2f;
            
            lastPlayedDate = session.endTime;
            totalPlayTime += session.SessionDuration;
            totalSessions++;
            
            // Update UI after metrics update
            UpdateUI();
        }
        
        public OptimizationSettings GetOptimizationSettings()
        {
            return new OptimizationSettings
            {
                enableDRS = this.enableDRS,
                enableLOD = this.enableLOD,
                enableThermalThrottling = this.enableThermalThrottling,
                optimizationPreference = this.optimizationPreference,
                gameName = this.gameName,
                genre = this.genre,
                compatibilityScore = this.compatibilityScore
            };
        }
        
        public void SetOptimizationSettings(OptimizationSettings settings)
        {
            enableDRS = settings.enableDRS;
            enableLOD = settings.enableLOD;
            enableThermalThrottling = settings.enableThermalThrottling;
            optimizationPreference = settings.optimizationPreference;
        }
        
        public string GetGameInfo()
        {
            var info = $"Game: {gameName}\n";
            info += $"Genre: {genre}\n";
            info += $"Package: {packageName}\n";
            info += $"Device: {deviceName}\n";
            info += $"Compatibility: {compatibilityScore}% ({performanceRating})\n";
            info += $"Installed: {(isInstalled ? "Yes" : "No")}\n";
            info += $"Favorite: {(isFavorite ? "Yes" : "No")}\n";
            
            if (totalSessions > 0)
            {
                info += $"Total Sessions: {totalSessions}\n";
                info += $"Total Play Time: {totalPlayTime:dd\\:hh\\:mm}\n";
                info += $"Last Played: {lastPlayedDate:yyyy-MM-dd}\n";
                info += $"Average Performance: {averagePerformanceScore:F1}/100\n";
                info += $"Average FPS: {averageFPS:F1} (Best: {bestFPS:F1})\n";
                info += $"Average Temperature: {averageTemperature:F1}°C\n";
                info += $"Average RAM Usage: {averageRAMUsage:F1}%\n";
            }
            
            return info;
        }
        
        public string GetPerformanceRecommendations()
        {
            var recommendations = $"Performance Recommendations for {gameName}:\n\n";
            
            if (compatibilityScore >= 80)
            {
                recommendations += "✅ Your device runs this game excellently!\n";
                recommendations += "• All graphics settings can be set to maximum\n";
                recommendations += "• No optimization needed\n";
            }
            else if (compatibilityScore >= 60)
            {
                recommendations += "⚠️ Your device runs this game well with some adjustments:\n";
                recommendations += "• Enable Dynamic Resolution Scaling\n";
                recommendations += "• Use balanced optimization mode\n";
                recommendations += "• Monitor temperature during long sessions\n";
            }
            else
            {
                recommendations += "❌ Your device may struggle with this game:\n";
                recommendations += "• Enable all optimization features\n";
                recommendations += "• Use performance optimization mode\n";
                recommendations += "• Consider lower graphics settings\n";
                recommendations += "• Monitor temperature and battery closely\n";
            }
            
            // Genre-specific recommendations
            switch (genre)
            {
                case GameGenre.Action:
                case GameGenre.MOBA:
                case GameGenre.BR:
                    recommendations += "\n🎯 Competitive Game Tips:\n";
                    recommendations += "• Prioritize frame rate over graphics quality\n";
                    recommendations += "• Enable high performance mode\n";
                    recommendations += "• Keep device cool for consistent performance\n";
                    break;
                    
                case GameGenre.RPG:
                case GameGenre.Adventure:
                    recommendations += "\n🌟 Story Game Tips:\n";
                    recommendations += "• Balance quality and performance\n";
                    recommendations += "• Enable battery saving during cutscenes\n";
                    recommendations += "• Use adaptive optimization\n";
                    break;
                    
                case GameGenre.Casual:
                case GameGenre.Puzzle:
                    recommendations += "\n🎲 Casual Game Tips:\n";
                    recommendations += "• Prioritize battery life\n";
                    recommendations += "• Use conservative optimization\n";
                    recommendations += "• Low power mode recommended\n";
                    break;
            }
            
            return recommendations;
        }
        
        public bool IsRecommendedForDevice(DeviceProfiler.DeviceInfo deviceInfo)
        {
            if (deviceInfo == null) return false;
            
            // High-end devices can run anything
            if (deviceInfo.totalRAM >= 8192 && deviceInfo.chipset.Contains("Snapdragon 8"))
                return true;
            
            // Mid-range devices - check compatibility
            if (deviceInfo.totalRAM >= 4096)
                return compatibilityScore >= 60;
            
            // Budget devices - only casual games
            return genre == GameGenre.Casual || genre == GameGenre.Puzzle || compatibilityScore >= 80;
        }
        
        private void OnDestroy()
        {
            if (launchButton != null)
                launchButton.onClick.RemoveAllListeners();
            
            if (favoriteToggle != null)
                favoriteToggle.onValueChanged.RemoveAllListeners();
        }
    }
    
    [System.Serializable]
    public class OptimizationSettings
    {
        public bool enableDRS;
        public bool enableLOD;
        public bool enableThermalThrottling;
        public GameItem.OptimizationPreference optimizationPreference;
        public string gameName;
        public GameItem.GameGenre genre;
        public int compatibilityScore;
        
        public string GetSettingsDescription()
        {
            var desc = $"Optimization Settings for {gameName}:\n";
            desc += $"Dynamic Resolution Scaling: {(enableDRS ? "Enabled" : "Disabled")}\n";
            desc += $"Level of Detail: {(enableLOD ? "Enabled" : "Disabled")}\n";
            desc += $"Thermal Throttling: {(enableThermalThrottling ? "Enabled" : "Disabled")}\n";
            desc += $"Preference: {optimizationPreference}\n";
            return desc;
        }
    }
}