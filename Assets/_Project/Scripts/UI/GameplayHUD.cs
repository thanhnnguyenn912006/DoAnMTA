using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SurvivorGame.Systems;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Hiển thị thông số trực tiếp trong trận: Thanh máu Player (lắng nghe sự kiện), Đồng hồ sinh tồn, Điểm số và Số quái đã hạ.
    /// </summary>
    public class GameplayHUD : MonoBehaviour
    {
        [Header("Tham chiếu hệ thống")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Health playerHealth;

        [Header("Thanh máu nhân vật")]
        [SerializeField] private Slider playerHealthSlider;
        [SerializeField] private TMP_Text playerHealthText;

        [Header("Đồng hồ, Điểm & Số quái hạ")]
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text killCountText;

        private int _lastHp = -1;
        private int _lastMaxHp = -1;
        private int _lastDisplayedSecond = -1;

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindAnyObjectByType<GameManager>();
            }

            if (playerHealth == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerHealth = playerObj.GetComponent<Health>();
                }
            }
        }

        private void OnEnable()
        {
            if (gameManager != null)
            {
                gameManager.OnScoreAndKillsChanged += HandleScoreAndKillsChanged;
            }

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += HandleHealthChanged;
            }
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.OnScoreAndKillsChanged -= HandleScoreAndKillsChanged;
            }

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= HandleHealthChanged;
            }
        }

        private void Start()
        {
            if (gameManager != null)
            {
                HandleScoreAndKillsChanged(gameManager.Score, gameManager.KillCount);
                UpdateTimerDisplay(gameManager.SurvivalTime);
            }

            if (playerHealth != null)
            {
                HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }
        }

        private void Update()
        {
            if (gameManager != null)
            {
                UpdateTimerDisplay(gameManager.SurvivalTime);
            }
        }

        private void HandleHealthChanged(int currentHp, int maxHp)
        {
            int safeHp = Mathf.Max(0, currentHp);
            int safeMaxHp = Mathf.Max(1, maxHp);

            if (safeHp == _lastHp && safeMaxHp == _lastMaxHp) return;

            _lastHp = safeHp;
            _lastMaxHp = safeMaxHp;

            if (playerHealthSlider != null)
            {
                playerHealthSlider.minValue = 0f;
                playerHealthSlider.maxValue = 1f;
                playerHealthSlider.value = (float)safeHp / safeMaxHp;
            }

            if (playerHealthText != null)
            {
                playerHealthText.text = $"HP {safeHp} / {safeMaxHp}";
            }
        }

        private void UpdateTimerDisplay(float elapsedSeconds)
        {
            if (timerText == null) return;

            int wholeSeconds = Mathf.Max(0, Mathf.FloorToInt(elapsedSeconds));
            if (wholeSeconds == _lastDisplayedSecond) return;

            _lastDisplayedSecond = wholeSeconds;
            timerText.text = GameManager.FormatTime(wholeSeconds);
        }

        private void HandleScoreAndKillsChanged(int score, int killCount)
        {
            if (scoreText != null)
            {
                scoreText.text = $"ĐIỂM: {score}";
            }

            if (killCountText != null)
            {
                killCountText.text = $"ĐÃ HẠ: {killCount}";
            }
        }
    }
}