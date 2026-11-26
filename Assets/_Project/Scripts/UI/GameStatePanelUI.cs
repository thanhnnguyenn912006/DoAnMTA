using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SurvivorGame.Systems;
using SurvivorGame.Weapons;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Điều khiển hiển thị màn hình Tạm dừng (Pause) và màn hình Kết thúc (Game Over / Victory)
    /// kèm bảng thống kê sát thương từng vũ khí theo trạng thái của GameManager.
    /// </summary>
    public class GameStatePanelUI : MonoBehaviour
    {
        [Header("Tham chiếu hệ thống")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameObject upgradePanelRoot;
        [SerializeField] private Transform weaponsContainer;

        [Header("Màn hình Tạm dừng (Pause)")]
        [SerializeField] private GameObject pausePanelRoot;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseRestartButton;
        [SerializeField] private Button pauseMainMenuButton;

        [Header("Màn hình Kết thúc (Game Over / Victory)")]
        [SerializeField] private GameObject endGamePanelRoot;
        [SerializeField] private TMP_Text endTitleText;
        [SerializeField] private TMP_Text endStatsText;
        [SerializeField] private Button endRestartButton;
        [SerializeField] private Button endMainMenuButton;

        [Header("Màu tiêu đề kết thúc")]
        [SerializeField] private Color gameOverColor = new Color(0.94f, 0.27f, 0.27f, 1f);
        [SerializeField] private Color victoryColor = new Color(0.98f, 0.80f, 0.08f, 1f);

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindAnyObjectByType<GameManager>();
            }

            if (weaponsContainer == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    weaponsContainer = playerObj.transform;
                }
            }
        }

        private void OnEnable()
        {
            if (resumeButton != null) resumeButton.onClick.AddListener(HandleResumeClicked);
            if (pauseRestartButton != null) pauseRestartButton.onClick.AddListener(HandleRestartClicked);
            if (pauseMainMenuButton != null) pauseMainMenuButton.onClick.AddListener(HandleMainMenuClicked);

            if (endRestartButton != null) endRestartButton.onClick.AddListener(HandleRestartClicked);
            if (endMainMenuButton != null) endMainMenuButton.onClick.AddListener(HandleMainMenuClicked);

            if (gameManager != null)
            {
                gameManager.OnGameStateChanged += HandleGameStateChanged;
                HandleGameStateChanged(gameManager.CurrentState);
            }
        }

        private void OnDisable()
        {
            if (resumeButton != null) resumeButton.onClick.RemoveListener(HandleResumeClicked);
            if (pauseRestartButton != null) pauseRestartButton.onClick.RemoveListener(HandleRestartClicked);
            if (pauseMainMenuButton != null) pauseMainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);

            if (endRestartButton != null) endRestartButton.onClick.RemoveListener(HandleRestartClicked);
            if (endMainMenuButton != null) endMainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);

            if (gameManager != null)
            {
                gameManager.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            bool isPaused = state == GameState.Paused;
            bool isEndGame = state == GameState.GameOver || state == GameState.Victory;

            if (pausePanelRoot != null)
            {
                pausePanelRoot.SetActive(isPaused);
            }

            if (endGamePanelRoot != null)
            {
                endGamePanelRoot.SetActive(isEndGame);
            }

            // Ẩn bảng nâng cấp nếu đang mở đúng lúc trận đấu kết thúc
            if (isEndGame && upgradePanelRoot != null)
            {
                upgradePanelRoot.SetActive(false);
            }

            if (isEndGame)
            {
                UpdateEndGameContent(state == GameState.Victory);
            }
        }

        private void UpdateEndGameContent(bool isVictory)
        {
            if (endTitleText != null)
            {
                endTitleText.text = isVictory ? "CHIẾN THẮNG!" : "GAME OVER";
                endTitleText.color = isVictory ? victoryColor : gameOverColor;
            }

            if (endStatsText != null && gameManager != null)
            {
                string timeText = GameManager.FormatTime(gameManager.SurvivalTime);
                string recordSuffix = gameManager.IsNewHighScore ? " (MỚI!)" : string.Empty;

                StringBuilder sb = new StringBuilder(256);
                sb.AppendLine($"Thời gian sống sót: {timeText}");
                sb.AppendLine($"Số quái đã hạ: {gameManager.KillCount}");
                sb.AppendLine($"Điểm số: {gameManager.Score}");
                sb.Append($"Kỷ lục cao nhất: {gameManager.HighScore}{recordSuffix}");

                AppendWeaponDamageBreakdown(sb);
                endStatsText.text = sb.ToString();
            }
        }

        private void AppendWeaponDamageBreakdown(StringBuilder sb)
        {
            if (weaponsContainer == null) return;

            WeaponBase[] weapons = weaponsContainer.GetComponentsInChildren<WeaponBase>(true);
            if (weapons == null || weapons.Length == 0) return;

            // Sắp xếp giảm dần theo sát thương thực tế (số vũ khí rất nhỏ nên sắp xếp tại chỗ, 0 LINQ)
            for (int i = 0; i < weapons.Length - 1; i++)
            {
                for (int j = i + 1; j < weapons.Length; j++)
                {
                    int dmgI = weapons[i] != null ? weapons[i].TotalDamageDealt : -1;
                    int dmgJ = weapons[j] != null ? weapons[j].TotalDamageDealt : -1;
                    if (dmgJ > dmgI)
                    {
                        (weapons[i], weapons[j]) = (weapons[j], weapons[i]);
                    }
                }
            }

            bool hasHeader = false;
            for (int i = 0; i < weapons.Length; i++)
            {
                WeaponBase w = weapons[i];
                if (w == null || w.Data == null) continue;
                if (!w.gameObject.activeSelf && w.TotalDamageDealt <= 0) continue;

                if (!hasHeader)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine("<color=#FACC15>THỐNG KÊ SÁT THƯƠNG VŨ KHÍ</color>");
                    hasHeader = true;
                }

                string displayName = w.IsEvolved ? w.Data.EvolvedWeaponName : w.Data.WeaponName;
                string badge = w.IsEvolved ? "[EVO]" : $"Lv.{w.CurrentLevel}";
                sb.AppendLine($"{displayName} ({badge}): {w.TotalDamageDealt:N0}");
            }
        }

        private void HandleResumeClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            gameManager?.ResumeGame();
        }

        private void HandleRestartClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            gameManager?.RestartGameplay();
        }

        private void HandleMainMenuClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            gameManager?.LoadMainMenu();
        }
    }
}