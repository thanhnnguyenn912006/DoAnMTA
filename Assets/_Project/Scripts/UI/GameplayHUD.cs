using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SurvivorGame.Player;
using SurvivorGame.Systems;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Hiển thị thông số trực tiếp trong trận: Thanh máu Player (lắng nghe sự kiện), Thanh hồi chiêu Lướt,
    /// Đồng hồ sinh tồn, Điểm số và Số quái đã hạ.
    /// </summary>
    public class GameplayHUD : MonoBehaviour
    {
        private static readonly Color DashReadyColor = new Color(0.133f, 0.773f, 0.369f, 1f);   // #22C55E
        private static readonly Color DashCoolingColor = new Color(0.392f, 0.455f, 0.545f, 1f); // #64748B

        [Header("Tham chiếu hệ thống")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Health playerHealth;
        [SerializeField] private PlayerMovement playerMovement;

        [Header("Thanh máu nhân vật")]
        [SerializeField] private Slider playerHealthSlider;
        [SerializeField] private TMP_Text playerHealthText;

        [Header("Thanh hồi chiêu Lướt (tùy chọn)")]
        [SerializeField] private Slider dashCooldownSlider;
        [SerializeField] private TMP_Text dashStatusText;

        [Header("Đồng hồ, Điểm & Số quái hạ")]
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text killCountText;

        private Image _dashFillImage;
        private int _lastHp = -1;
        private int _lastMaxHp = -1;
        private int _lastDisplayedSecond = -1;
        private int _lastDashTenths = -1;
        private bool _hasDashFillColor;
        private bool _lastDashReady;

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindAnyObjectByType<GameManager>();
            }

            if (playerHealth == null || playerMovement == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    if (playerHealth == null) playerHealth = playerObj.GetComponent<Health>();
                    if (playerMovement == null) playerMovement = playerObj.GetComponent<PlayerMovement>();
                }
            }

            if (dashCooldownSlider != null && dashCooldownSlider.fillRect != null)
            {
                _dashFillImage = dashCooldownSlider.fillRect.GetComponent<Image>();
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

            UpdateDashDisplay(force: true);
        }

        private void Update()
        {
            if (gameManager != null)
            {
                UpdateTimerDisplay(gameManager.SurvivalTime);
            }

            UpdateDashDisplay(force: false);
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

        private void UpdateDashDisplay(bool force)
        {
            if (playerMovement == null || (dashCooldownSlider == null && dashStatusText == null))
            {
                return;
            }

            float remaining = playerMovement.DashCooldownRemaining;
            bool isReady = remaining <= 0.001f;

            if (dashCooldownSlider != null)
            {
                dashCooldownSlider.minValue = 0f;
                dashCooldownSlider.maxValue = 1f;
                dashCooldownSlider.value = playerMovement.DashCooldownNormalized;
            }

            if (_dashFillImage != null && (force || !_hasDashFillColor || _lastDashReady != isReady))
            {
                _dashFillImage.color = isReady ? DashReadyColor : DashCoolingColor;
                _hasDashFillColor = true;
                _lastDashReady = isReady;
            }

            if (dashStatusText != null)
            {
                int tenths = isReady ? 0 : Mathf.Max(1, Mathf.CeilToInt(remaining * 10f));
                if (force || tenths != _lastDashTenths)
                {
                    _lastDashTenths = tenths;
                    dashStatusText.text = isReady
                        ? "LƯỚT [Space]"
                        : $"LƯỚT {tenths * 0.1f:0.0}s";
                }
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
