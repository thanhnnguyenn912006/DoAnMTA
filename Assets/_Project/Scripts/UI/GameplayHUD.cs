using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SurvivorGame.Enemies;
using SurvivorGame.Player;
using SurvivorGame.Systems;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Hiển thị thông số trực tiếp trong trận: Thanh máu Player (lắng nghe sự kiện), Thanh hồi chiêu Lướt,
    /// Banner thông báo Wave/Boss, Đồng hồ sinh tồn, Điểm số và Số quái đã hạ.
    /// </summary>
    public class GameplayHUD : MonoBehaviour
    {
        private static readonly Color DashReadyColor = new Color(0.133f, 0.773f, 0.369f, 1f);   // #22C55E
        private static readonly Color DashCoolingColor = new Color(0.392f, 0.455f, 0.545f, 1f); // #64748B
        private static readonly Color WaveBannerColor = new Color(0.980f, 0.800f, 0.082f, 1f);  // #FACC15
        private static readonly Color BossBannerColor = new Color(0.937f, 0.267f, 0.267f, 1f);  // #EF4444

        [Header("Tham chiếu hệ thống")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Health playerHealth;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private BossSpawner bossSpawner;

        [Header("Thanh máu nhân vật")]
        [SerializeField] private Slider playerHealthSlider;
        [SerializeField] private TMP_Text playerHealthText;

        [Header("Thanh hồi chiêu Lướt (tùy chọn)")]
        [SerializeField] private Slider dashCooldownSlider;
        [SerializeField] private TMP_Text dashStatusText;

        [Header("Banner thông báo Wave / Boss")]
        [SerializeField] private TMP_Text announcementBannerText;
        [SerializeField, Min(0.5f)] private float bannerHoldDuration = 2.8f;
        [SerializeField, Min(0.1f)] private float bannerFadeDuration = 0.6f;

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

        private bool _isShowingBanner;
        private float _bannerTimer;
        private Color _bannerBaseColor = WaveBannerColor;

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindAnyObjectByType<GameManager>();
            }

            if (enemySpawner == null)
            {
                enemySpawner = FindAnyObjectByType<EnemySpawner>();
            }

            if (bossSpawner == null)
            {
                bossSpawner = FindAnyObjectByType<BossSpawner>();
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

            HideAnnouncementBanner();
        }

        private void OnEnable()
        {
            if (gameManager != null)
            {
                gameManager.OnScoreAndKillsChanged += HandleScoreAndKillsChanged;
                gameManager.OnGameStateChanged += HandleGameStateChanged;
            }

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += HandleHealthChanged;
            }

            if (enemySpawner != null)
            {
                enemySpawner.OnPhaseChanged += HandlePhaseChanged;
            }

            if (bossSpawner != null)
            {
                bossSpawner.OnBossWarning += HandleBossWarning;
                bossSpawner.OnBossSpawned += HandleBossSpawned;
            }
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.OnScoreAndKillsChanged -= HandleScoreAndKillsChanged;
                gameManager.OnGameStateChanged -= HandleGameStateChanged;
            }

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= HandleHealthChanged;
            }

            if (enemySpawner != null)
            {
                enemySpawner.OnPhaseChanged -= HandlePhaseChanged;
            }

            if (bossSpawner != null)
            {
                bossSpawner.OnBossWarning -= HandleBossWarning;
                bossSpawner.OnBossSpawned -= HandleBossSpawned;
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
            UpdateAnnouncementBanner();
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

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver || state == GameState.Victory)
            {
                HideAnnouncementBanner();
            }
        }

        private void HandlePhaseChanged(int phaseIndex, string phaseName)
        {
            string message = string.IsNullOrEmpty(phaseName)
                ? $"ĐỢT QUÁI MỚI - GIAI ĐOẠN {phaseIndex + 1}!"
                : $"ĐỢT QUÁI MỚI: {phaseName.ToUpperInvariant()}!";
            ShowAnnouncementBanner(message, WaveBannerColor);
        }

        private void HandleBossWarning(string message)
        {
            ShowAnnouncementBanner(
                string.IsNullOrEmpty(message) ? "CẢNH BÁO: BOSS SẮP XUẤT HIỆN!" : message,
                BossBannerColor);
        }

        private void HandleBossSpawned(EnemyController boss, Health bossHealth)
        {
            string bossName = (boss != null && boss.Data != null && !string.IsNullOrEmpty(boss.Data.EnemyName))
                ? boss.Data.EnemyName.ToUpperInvariant()
                : "BOSS";
            ShowAnnouncementBanner($"BOSS XUẤT HIỆN: {bossName}!", BossBannerColor);
        }

        private void ShowAnnouncementBanner(string message, Color color)
        {
            if (announcementBannerText == null || string.IsNullOrEmpty(message)) return;
            if (gameManager != null &&
                (gameManager.CurrentState == GameState.GameOver || gameManager.CurrentState == GameState.Victory))
            {
                return;
            }

            _bannerBaseColor = color;
            _bannerTimer = 0f;
            _isShowingBanner = true;

            announcementBannerText.text = message;
            announcementBannerText.color = color;
            announcementBannerText.gameObject.SetActive(true);
        }

        private void UpdateAnnouncementBanner()
        {
            if (!_isShowingBanner || announcementBannerText == null) return;

            _bannerTimer += Time.deltaTime;
            if (_bannerTimer <= bannerHoldDuration) return;

            float fadeT = Mathf.Clamp01((_bannerTimer - bannerHoldDuration) / Mathf.Max(0.05f, bannerFadeDuration));
            Color c = _bannerBaseColor;
            c.a = Mathf.Lerp(_bannerBaseColor.a, 0f, fadeT);
            announcementBannerText.color = c;

            if (fadeT >= 1f)
            {
                HideAnnouncementBanner();
            }
        }

        private void HideAnnouncementBanner()
        {
            _isShowingBanner = false;
            _bannerTimer = 0f;
            if (announcementBannerText != null)
            {
                announcementBannerText.gameObject.SetActive(false);
            }
        }
    }
}
