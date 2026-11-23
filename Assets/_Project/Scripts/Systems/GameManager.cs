using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SurvivorGame.Enemies;

namespace SurvivorGame.Systems
{
    public enum GameState
    {
        Playing,
        LevelUp,
        Paused,
        GameOver,
        Victory
    }

    /// <summary>
    /// Quản lý vòng lặp trận đấu (nguồn chân lý duy nhất điều khiển Time.timeScale):
    /// trạng thái chơi, lên cấp (LevelUp), tạm dừng (ESC), điểm số, điều kiện Thắng/Thua và lưu kỷ lục.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        public const string HighScoreKey = "Survivor_HighScore";
        public const string BestTimeKey = "Survivor_BestTime";

        public static GameManager Instance { get; private set; }

        [Header("Tham chiếu bắt buộc")]
        [SerializeField] private Health playerHealth;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private UpgradeManager upgradeManager;

        [Header("Cấu hình trận đấu")]
        [SerializeField, Min(0f)] private float victorySurvivalTime = 0f; // 0 = tắt thắng theo thời gian (thắng khi hạ Boss)
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Trạng thái (chỉ đọc để theo dõi)")]
        [SerializeField] private GameState currentState = GameState.Playing;
        [SerializeField] private int score;
        [SerializeField] private int killCount;
        [SerializeField] private float survivalTime;

        public GameState CurrentState => currentState;
        public int Score => score;
        public int KillCount => killCount;
        public float SurvivalTime => survivalTime;
        public float VictorySurvivalTime => victorySurvivalTime;
        public int HighScore { get; private set; }
        public float BestTime { get; private set; }
        public bool IsNewHighScore { get; private set; }

        public event Action<GameState> OnGameStateChanged;
        public event Action<int, int> OnScoreAndKillsChanged; // (score, killCount)

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[GameManager] Phat hien ban trung tren '{name}', tu dong xoa component thua.");
                Destroy(this);
                return;
            }

            Instance = this;
            Time.timeScale = 1f;
            currentState = GameState.Playing;

            if (playerHealth == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerHealth = playerObj.GetComponent<Health>();
                }
            }

            if (enemySpawner == null)
            {
                enemySpawner = FindAnyObjectByType<EnemySpawner>();
            }

            if (upgradeManager == null)
            {
                upgradeManager = FindAnyObjectByType<UpgradeManager>();
            }

            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
            BestTime = PlayerPrefs.GetFloat(BestTimeKey, 0f);
            IsNewHighScore = false;
        }

        private void Start()
        {
            OnScoreAndKillsChanged?.Invoke(score, killCount);
            OnGameStateChanged?.Invoke(currentState);
        }

        private void OnEnable()
        {
            EnemyController.OnAnyEnemyDied += HandleEnemyDied;

            if (upgradeManager != null)
            {
                upgradeManager.OnUpgradeChoicesReady += HandleUpgradeChoicesReady;
                upgradeManager.OnUpgradeMenuClosed += HandleUpgradeMenuClosed;
            }
        }

        private void OnDisable()
        {
            EnemyController.OnAnyEnemyDied -= HandleEnemyDied;

            if (upgradeManager != null)
            {
                upgradeManager.OnUpgradeChoicesReady -= HandleUpgradeChoicesReady;
                upgradeManager.OnUpgradeMenuClosed -= HandleUpgradeMenuClosed;
            }
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            // Bắt phím ESC để tạm dừng / tiếp tục (trạng thái LevelUp tự động chặn ESC)
            if (Input.GetKeyDown(KeyCode.Escape) &&
                (currentState == GameState.Playing || currentState == GameState.Paused))
            {
                TogglePause();
            }

            // Chết đúng lúc đang mở bảng chọn nâng cấp vẫn phải kết thúc trận
            if (currentState == GameState.LevelUp && playerHealth != null && playerHealth.IsDead)
            {
                TriggerGameOver();
                return;
            }

            if (currentState != GameState.Playing) return;

            // Kiểm tra điều kiện thua (Player hết máu)
            if (playerHealth != null && playerHealth.IsDead)
            {
                TriggerGameOver();
                return;
            }

            // Đồng bộ thời gian sinh tồn với EnemySpawner
            survivalTime = enemySpawner != null ? enemySpawner.ElapsedTime : survivalTime + Time.deltaTime;

            // Kiểm tra điều kiện chiến thắng theo thời gian (chỉ chạy nếu victorySurvivalTime > 0)
            if (victorySurvivalTime > 0f && survivalTime >= victorySurvivalTime)
            {
                TriggerVictory();
            }
        }

        /// <summary>
        /// Bảng chọn nâng cấp vừa mở: chuyển sang trạng thái LevelUp và dừng thời gian.
        /// </summary>
        private void HandleUpgradeChoicesReady(IReadOnlyList<UpgradeOption> choices)
        {
            if (currentState == GameState.Playing)
            {
                SetState(GameState.LevelUp, 0f);
            }
        }

        /// <summary>
        /// Bảng chọn nâng cấp đã đóng: chỉ tiếp tục chơi nếu game vẫn đang ở LevelUp (không ghi đè GameOver/Victory).
        /// </summary>
        private void HandleUpgradeMenuClosed()
        {
            if (currentState == GameState.LevelUp)
            {
                SetState(GameState.Playing, 1f);
            }
        }

        private void HandleEnemyDied(EnemyController deadEnemy)
        {
            if (currentState != GameState.Playing) return;

            killCount++;
            int points = (deadEnemy != null && deadEnemy.Data != null) ? deadEnemy.Data.ScoreValue : 10;
            score += points;

            OnScoreAndKillsChanged?.Invoke(score, killCount);

            // Hạ Boss = chiến thắng. Gọi sau khi đã cộng điểm Boss để kỷ lục được lưu đúng.
            if (deadEnemy != null && deadEnemy.CompareTag("Boss"))
            {
                TriggerVictory();
            }
        }

        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                SetState(GameState.Paused, 0f);
            }
            else if (currentState == GameState.Paused)
            {
                SetState(GameState.Playing, 1f);
            }
        }

        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
            {
                SetState(GameState.Playing, 1f);
            }
        }

        public void TriggerGameOver()
        {
            if (currentState == GameState.GameOver || currentState == GameState.Victory) return;

            SaveRecords();
            if (enemySpawner != null) enemySpawner.SetSpawning(false);
            SetState(GameState.GameOver, 0f);
            Debug.Log($"[GameManager] GAME OVER | Diem: {score} | Da ha: {killCount} | Thoi gian: {FormatTime(survivalTime)}");
        }

        public void TriggerVictory()
        {
            if (currentState == GameState.GameOver || currentState == GameState.Victory) return;

            SaveRecords();
            if (enemySpawner != null) enemySpawner.SetSpawning(false);
            SetState(GameState.Victory, 0f);
            Debug.Log($"[GameManager] CHIEN THANG! | Diem: {score} | Da ha: {killCount} | Thoi gian: {FormatTime(survivalTime)}");
        }

        public void RestartGameplay()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            if (!string.IsNullOrEmpty(mainMenuSceneName))
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }

        private void SetState(GameState newState, float targetTimeScale)
        {
            currentState = newState;
            Time.timeScale = targetTimeScale;
            OnGameStateChanged?.Invoke(currentState);
        }

        private void SaveRecords()
        {
            bool changed = false;

            if (score > HighScore)
            {
                HighScore = score;
                IsNewHighScore = true;
                PlayerPrefs.SetInt(HighScoreKey, HighScore);
                changed = true;
            }

            if (survivalTime > BestTime)
            {
                BestTime = survivalTime;
                PlayerPrefs.SetFloat(BestTimeKey, BestTime);
                changed = true;
            }

            if (changed)
            {
                PlayerPrefs.Save();
            }
        }

        public static string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
            int mins = totalSeconds / 60;
            int secs = totalSeconds % 60;
            return $"{mins:00}:{secs:00}";
        }

        [ContextMenu("Test: Kich hoat Game Over")]
        private void TestGameOver() => TriggerGameOver();

        [ContextMenu("Test: Kich hoat Chien Thang")]
        private void TestVictory() => TriggerVictory();
    }
}