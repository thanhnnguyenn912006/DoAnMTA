using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using SurvivorGame.Enemies;

namespace SurvivorGame.Systems
{
    public enum GameState
    {
        Playing,
        Paused,
        GameOver,
        Victory
    }

    /// <summary>
    /// Quản lý vòng lặp trận đấu: trạng thái chơi, tạm dừng (ESC), điểm số, số quái hạ,
    /// điều kiện Thắng/Thua và lưu kỷ lục qua PlayerPrefs.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public const string HighScoreKey = "Survivor_HighScore";
        public const string BestTimeKey = "Survivor_BestTime";

        [Header("Tham chiếu bắt buộc")]
        [SerializeField] private Health playerHealth;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private UpgradeManager upgradeManager;

        [Header("Cấu hình trận đấu")]
        [SerializeField, Min(10f)] private float victorySurvivalTime = 300f; // Sống sót đủ 5 phút (300s) để chiến thắng
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
        }

        private void OnDisable()
        {
            EnemyController.OnAnyEnemyDied -= HandleEnemyDied;
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        private void Update()
        {
            // Bắt phím ESC để tạm dừng / tiếp tục (bỏ qua nếu đang mở bảng chọn nâng cấp)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                bool isUpgrading = upgradeManager != null && upgradeManager.IsChoosingUpgrade;
                if (!isUpgrading && (currentState == GameState.Playing || currentState == GameState.Paused))
                {
                    TogglePause();
                }
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

            // Kiểm tra điều kiện chiến thắng
            if (victorySurvivalTime > 0f && survivalTime >= victorySurvivalTime)
            {
                TriggerVictory();
            }
        }

        private void HandleEnemyDied(EnemyController deadEnemy)
        {
            if (currentState != GameState.Playing) return;

            killCount++;
            int points = (deadEnemy != null && deadEnemy.Data != null) ? deadEnemy.Data.ScoreValue : 10;
            score += points;

            OnScoreAndKillsChanged?.Invoke(score, killCount);
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