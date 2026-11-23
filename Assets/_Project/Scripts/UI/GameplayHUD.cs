using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SurvivorGame.Systems;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Hiển thị thông số trực tiếp trong trận: Thanh máu Player, Đồng hồ sinh tồn, Điểm số và Số quái đã hạ.
    /// Lưu đệm giá trị cũ để hạn chế tối đa cấp phát chuỗi rác (GC Alloc) mỗi khung hình.
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
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.OnScoreAndKillsChanged -= HandleScoreAndKillsChanged;
            }
        }

        private void Start()
        {
            if (gameManager != null)
            {
                HandleScoreAndKillsChanged(gameManager.Score, gameManager.KillCount);
                UpdateTimerDisplay(gameManager.SurvivalTime);
            }

            UpdateHealthDisplay();
        }

        private void Update()
        {
            UpdateHealthDisplay();

            if (gameManager != null)
            {
                UpdateTimerDisplay(gameManager.SurvivalTime);
            }
        }

        private void UpdateHealthDisplay()
        {
            if (playerHealth == null) return;

            int currentHp = Mathf.Max(0, playerHealth.CurrentHealth);
            int maxHp = Mathf.Max(1, playerHealth.MaxHealth);

            if (currentHp == _lastHp && maxHp == _lastMaxHp) return;

            _lastHp = currentHp;
            _lastMaxHp = maxHp;

            if (playerHealthSlider != null)
            {
                playerHealthSlider.minValue = 0f;
                playerHealthSlider.maxValue = 1f;
                playerHealthSlider.value = (float)currentHp / maxHp;
            }

            if (playerHealthText != null)
            {
                playerHealthText.text = $"HP {currentHp} / {maxHp}";
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
                scoreText.text = $"DIEM: {score}";
            }

            if (killCountText != null)
            {
                killCountText.text = $"DA HA: {killCount}";
            }
        }
    }
}