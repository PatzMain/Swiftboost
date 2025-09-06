using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Swiftboost.Core;
using Swiftboost.Monitoring;

namespace Swiftboost.Game
{
    public class GameManager : MonoBehaviour
    {
        [Header("Game Management")]
        [SerializeField] private List<GameItem> installedGames;
        [SerializeField] private GameItem currentActiveGame;
        [SerializeField] private bool gameSessionActive = false;
        
        [Header("Session Management")]
        [SerializeField] private GameSession currentSession;
        [SerializeField] private List<GameSession> sessionHistory;
        [SerializeField] private int maxSessionHistory = 50;
        
        // References
        private AndroidBridge androidBridge;
        private DeviceProfiler deviceProfiler;
        private PerformanceLogger performanceLogger;
        private MonitoringOverlay monitoringOverlay;
        
        // Events
        public event Action<GameItem> OnGameLaunched;
        public event Action<GameItem> OnGameStopped;
        public event Action<GameSession> OnSessionStarted;
        public event Action<GameSession> OnSessionEnded;
        public event Action<List<GameItem>> OnGamesUpdated;
        
        public List<GameItem> InstalledGames => installedGames;
        public GameItem CurrentActiveGame => currentActiveGame;
        public bool IsGameSessionActive => gameSessionActive;
        public GameSession CurrentSession => currentSession;
        
        private void Start()
        {
            InitializeGameManager();
        }
        
        private void InitializeGameManager()
        {
            // Initialize collections
            if (installedGames == null)
                installedGames = new List<GameItem>();
            
            if (sessionHistory == null)
                sessionHistory = new List<GameSession>();
            
            // Get references
            androidBridge = SystemManager.Instance?.AndroidBridge;
            deviceProfiler = SystemManager.Instance?.DeviceProfiler;
            performanceLogger = FindObjectOfType<PerformanceLogger>();
            monitoringOverlay = FindObjectOfType<MonitoringOverlay>();
            
            if (androidBridge == null || deviceProfiler == null)
            {
                Debug.LogError("GameManager: Required components not found!");
                return;
            }
            
            // Load installed games
            StartCoroutine(LoadInstalledGamesAsync());
            
            Debug.Log("GameManager initialized");
        }
        
        private IEnumerator LoadInstalledGamesAsync()
        {
            yield return new WaitForSeconds(1f); // Wait for system initialization
            
            Debug.Log("Scanning for installed games...");
            
            // In a real implementation, this would scan for installed games
            // For now, we'll create some sample games based on popular titles
            LoadSampleGames();
            
            OnGamesUpdated?.Invoke(installedGames);
            
            Debug.Log($"Found {installedGames.Count} games");
        }
        
        private void LoadSampleGames()
        {
            var deviceInfo = deviceProfiler.GetCurrentDevice();
            
            // Popular mobile games
            var sampleGames = new List<(string name, string packageName, GameItem.GameGenre genre, int compatibilityScore)>
            {
                ("PUBG Mobile", "com.tencent.ig", GameItem.GameGenre.Action, CalculateCompatibilityScore("PUBG Mobile")),
                ("Call of Duty Mobile", "com.activision.callofduty.shooter", GameItem.GameGenre.Action, CalculateCompatibilityScore("Call of Duty Mobile")),
                ("Genshin Impact", "com.miHoYo.GenshinImpact", GameItem.GameGenre.RPG, CalculateCompatibilityScore("Genshin Impact")),
                ("Fortnite", "com.epicgames.fortnite", GameItem.GameGenre.Action, CalculateCompatibilityScore("Fortnite")),
                ("Among Us", "com.innersloth.spacemafia", GameItem.GameGenre.Casual, CalculateCompatibilityScore("Among Us")),
                ("Minecraft", "com.mojang.minecraftpe", GameItem.GameGenre.Sandbox, CalculateCompatibilityScore("Minecraft")),
                ("Clash Royale", "com.supercell.clashroyale", GameItem.GameGenre.Strategy, CalculateCompatibilityScore("Clash Royale")),
                ("Pokemon GO", "com.nianticlabs.pokemongo", GameItem.GameGenre.AR, CalculateCompatibilityScore("Pokemon GO")),
                ("Candy Crush Saga", "com.king.candycrushsaga", GameItem.GameGenre.Puzzle, CalculateCompatibilityScore("Candy Crush Saga")),
                ("Mobile Legends", "com.mobile.legends", GameItem.GameGenre.MOBA, CalculateCompatibilityScore("Mobile Legends"))
            };
            
            foreach (var gameData in sampleGames)
            {
                var gameItem = new GameItem
                {
                    gameName = gameData.name,
                    packageName = gameData.packageName,
                    genre = gameData.genre,
                    compatibilityScore = gameData.compatibilityScore,
                    isInstalled = UnityEngine.Random.value > 0.3f, // 70% chance of being "installed"
                    deviceName = deviceInfo.GetFullDeviceName(),
                    lastPlayedDate = DateTime.Now.AddDays(-UnityEngine.Random.Range(0, 30)),
                    totalPlayTime = TimeSpan.FromHours(UnityEngine.Random.Range(1, 100))
                };
                
                // Set performance rating based on compatibility
                gameItem.SetPerformanceRating(deviceInfo);
                
                installedGames.Add(gameItem);
            }
            
            // Sort by compatibility score
            installedGames.Sort((a, b) => b.compatibilityScore.CompareTo(a.compatibilityScore));
        }
        
        private int CalculateCompatibilityScore(string gameName)
        {
            var deviceInfo = deviceProfiler.GetCurrentDevice();
            if (deviceInfo == null) return 50;
            
            int baseScore = 60;
            
            // RAM-based scoring
            if (deviceInfo.totalRAM >= 12288) // 12GB+
                baseScore += 25;
            else if (deviceInfo.totalRAM >= 8192) // 8GB+
                baseScore += 15;
            else if (deviceInfo.totalRAM >= 6144) // 6GB+
                baseScore += 10;
            else if (deviceInfo.totalRAM >= 4096) // 4GB+
                baseScore += 5;
            
            // Chipset-based scoring
            string chipset = deviceInfo.chipset.ToLower();
            if (chipset.Contains("snapdragon 8") || chipset.Contains("a16") || chipset.Contains("a17"))
                baseScore += 15;
            else if (chipset.Contains("snapdragon 7") || chipset.Contains("tensor") || chipset.Contains("a15"))
                baseScore += 10;
            else if (chipset.Contains("snapdragon 6") || chipset.Contains("a14"))
                baseScore += 5;
            
            // Game-specific adjustments
            switch (gameName.ToLower())
            {
                case "genshin impact":
                case "pubg mobile":
                case "call of duty mobile":
                    baseScore -= 5; // More demanding games
                    break;
                case "among us":
                case "candy crush saga":
                    baseScore += 10; // Less demanding games
                    break;
            }
            
            return Mathf.Clamp(baseScore, 0, 100);
        }
        
        public void LaunchGame(GameItem game)
        {
            if (game == null)
            {
                Debug.LogError("Cannot launch null game");
                return;
            }
            
            if (gameSessionActive)
            {
                Debug.LogWarning($"Game session already active for {currentActiveGame.gameName}");
                return;
            }
            
            Debug.Log($"Launching game: {game.gameName}");
            
            currentActiveGame = game;
            gameSessionActive = true;
            
            // Create new game session
            StartGameSession(game);
            
            // Enable monitoring overlay
            if (monitoringOverlay != null)
            {
                monitoringOverlay.ShowOverlay();
            }
            
            // Apply device-specific optimizations for this game
            ApplyGameOptimizations(game);
            
            OnGameLaunched?.Invoke(game);
            
            Debug.Log($"Game launched successfully: {game.gameName}");
        }
        
        public void StopGame()
        {
            if (!gameSessionActive || currentActiveGame == null)
            {
                Debug.LogWarning("No active game session to stop");
                return;
            }
            
            Debug.Log($"Stopping game: {currentActiveGame.gameName}");
            
            var gameToStop = currentActiveGame;
            
            // End game session
            EndGameSession();
            
            // Hide monitoring overlay
            if (monitoringOverlay != null)
            {
                monitoringOverlay.HideOverlay();
            }
            
            gameSessionActive = false;
            currentActiveGame = null;
            
            OnGameStopped?.Invoke(gameToStop);
            
            Debug.Log("Game stopped successfully");
        }
        
        private void StartGameSession(GameItem game)
        {
            var deviceInfo = deviceProfiler.GetCurrentDevice();
            currentSession = new GameSession(game.gameName, deviceInfo);
            currentSession.StartSession();
            
            // Start performance logging for this session
            if (performanceLogger != null)
            {
                performanceLogger.StartGameSession(currentSession);
            }
            
            OnSessionStarted?.Invoke(currentSession);
            
            Debug.Log($"Game session started for: {game.gameName}");
        }
        
        private void EndGameSession()
        {
            if (currentSession == null) return;
            
            currentSession.EndSession();
            
            // Stop performance logging
            if (performanceLogger != null)
            {
                performanceLogger.EndGameSession();
            }
            
            // Add to session history
            AddToSessionHistory(currentSession);
            
            OnSessionEnded?.Invoke(currentSession);
            
            Debug.Log($"Game session ended: {currentSession.gameName}");
            Debug.Log($"Session duration: {currentSession.SessionDuration:mm\\:ss}");
            Debug.Log($"Performance score: {currentSession.devicePerformanceScore}/100");
            
            currentSession = null;
        }
        
        private void AddToSessionHistory(GameSession session)
        {
            sessionHistory.Add(session);
            
            // Maintain history limit
            if (sessionHistory.Count > maxSessionHistory)
            {
                sessionHistory.RemoveAt(0);
            }
            
            // Update game statistics
            UpdateGameStatistics(session);
        }
        
        private void UpdateGameStatistics(GameSession session)
        {
            var game = installedGames.Find(g => g.gameName == session.gameName);
            if (game != null)
            {
                game.totalPlayTime += session.SessionDuration;
                game.lastPlayedDate = session.endTime;
                game.totalSessions++;
                
                // Update average performance score
                if (game.averagePerformanceScore == 0)
                    game.averagePerformanceScore = session.devicePerformanceScore;
                else
                    game.averagePerformanceScore = (game.averagePerformanceScore + session.devicePerformanceScore) / 2f;
            }
        }
        
        private void ApplyGameOptimizations(GameItem game)
        {
            var optimizationHandler = SystemManager.Instance?.OptimizationHandler;
            if (optimizationHandler == null) return;
            
            Debug.Log($"Applying optimizations for {game.gameName} (Genre: {game.genre})");
            
            // Genre-specific optimizations
            switch (game.genre)
            {
                case GameItem.GameGenre.Action:
                case GameItem.GameGenre.MOBA:
                    // High performance mode for competitive games
                    optimizationHandler.SetPerformanceMode(OptimizationMode.HighPerformance);
                    break;
                    
                case GameItem.GameGenre.RPG:
                    // Balanced mode for RPGs
                    optimizationHandler.SetPerformanceMode(OptimizationMode.Balanced);
                    break;
                    
                case GameItem.GameGenre.Casual:
                case GameItem.GameGenre.Puzzle:
                    // Battery saving mode for casual games
                    optimizationHandler.SetPerformanceMode(OptimizationMode.BatterySaver);
                    break;
                    
                default:
                    optimizationHandler.SetPerformanceMode(OptimizationMode.Balanced);
                    break;
            }
        }
        
        public List<GameSession> GetSessionHistory(int count = 10)
        {
            if (sessionHistory.Count <= count)
                return new List<GameSession>(sessionHistory);
            
            return sessionHistory.GetRange(sessionHistory.Count - count, count);
        }
        
        public List<GameItem> GetGamesByGenre(GameItem.GameGenre genre)
        {
            return installedGames.FindAll(game => game.genre == genre);
        }
        
        public List<GameItem> GetTopPerformingGames(int count = 5)
        {
            var sortedGames = new List<GameItem>(installedGames);
            sortedGames.Sort((a, b) => b.compatibilityScore.CompareTo(a.compatibilityScore));
            
            if (sortedGames.Count <= count)
                return sortedGames;
            
            return sortedGames.GetRange(0, count);
        }
        
        public GameItem GetGameByName(string gameName)
        {
            return installedGames.Find(game => 
                game.gameName.Equals(gameName, StringComparison.OrdinalIgnoreCase));
        }
        
        public void RefreshGameList()
        {
            Debug.Log("Refreshing game list...");
            StartCoroutine(LoadInstalledGamesAsync());
        }
        
        public string GetGameManagerStatus()
        {
            var status = $"Game Manager Status:\n";
            status += $"Installed Games: {installedGames.Count}\n";
            status += $"Active Session: {(gameSessionActive ? currentActiveGame?.gameName : "None")}\n";
            status += $"Session History: {sessionHistory.Count}\n";
            
            if (currentSession != null)
            {
                status += $"Current Session Duration: {(DateTime.Now - currentSession.startTime):mm\\:ss}\n";
            }
            
            return status;
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (gameSessionActive && currentSession != null)
            {
                if (pauseStatus)
                {
                    // Game paused - reduce optimization intensity
                    Debug.Log("Game paused - reducing optimization intensity");
                }
                else
                {
                    // Game resumed - restore optimization
                    Debug.Log("Game resumed - restoring optimization");
                    if (currentActiveGame != null)
                    {
                        ApplyGameOptimizations(currentActiveGame);
                    }
                }
            }
        }
        
        private void OnDestroy()
        {
            if (gameSessionActive)
            {
                StopGame();
            }
        }
    }
    
    public enum OptimizationMode
    {
        BatterySaver,
        Balanced,
        HighPerformance
    }
}