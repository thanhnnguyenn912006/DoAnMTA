using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SurvivorGame.Systems;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Lắng nghe GameManager để bật/tắt màn hình Tạm dừng (Pause), Game Over và Chiến thắng (Victory).
    /// Gắn script này lên đối tượng luôn bật (Canvas), KHÔNG gắn lên Panel bị tắt sẵn.
    /// </summary>
    public class GameStatePanelUI : MonoBehaviour
    {
        [Header("Tham chiếu hệ thống")]
        [SerializeField] private GameManager gameManager;

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
        [SerializeField] private Color gameOverColor = new Color(0.94f, 0.27f, 0.27f, 1f); // Đỏ #EF4444
        [SerializeField] private Color victoryColor = new Color(0.98f, 0.80f, 0.08f, 1f);  // Vàng #FACC15

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindAnyObjectByType<GameManager>();
            }

            if (pausePanelRoot != null) pausePanelRoot.SetActive(false);
            if (endGamePanelRoot != null) endGamePanelRoot.SetActive(false);

            BindButtons();
        }

        private void OnEnable()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged += HandleGameStateChanged;
            }
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            UnbindButtons();
        }

        private void BindButtons()
        {
            if (resumeButton != null) resumeButton.onClick.AddListener(HandleResumeClicked);
            if (pauseRestartButton != null) pauseRestartButton.onClick.AddListener(HandleRestartClicked);
            if (pauseMainMenuButton != null) pauseMainMenuButton.onClick.AddListener(HandleMainMenuClicked);
            if (endRestartButton != null) endRestartButton.onClick.AddListener(HandleRestartClicked);
            if (endMainMenuButton != null) endMainMenuButton.onClick.AddListener(HandleMainMenuClicked);
        }

        private void UnbindButtons()
        {
            if (resumeButton != null) resumeButton.onClick.RemoveListener(HandleResumeClicked);
            if (pauseRestartButton != null) pauseRestartButton.onClick.RemoveListener(HandleRestartClicked);
            if (pauseMainMenuButton != null) pauseMainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);
            if (endRestartButton != null) endRestartButton.onClick.RemoveListener(HandleRestartClicked);
            if (endMainMenuButton != null) endMainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (pausePanelRoot != null)
            {
                pausePanelRoot.SetActive(state == GameState.Paused);
            }

            bool isEndState = state == GameState.GameOver || state == GameState.Victory;
            if (endGamePanelRoot != null)
            {
                endGamePanelRoot.SetActive(isEndState);
            }

            if (isEndState)
            {
                PopulateEndGamePanel(state == GameState.Victory);
            }
        }

        private void PopulateEndGamePanel(bool isVictory)
        {
            if (endTitleText != null)
            {
                endTitleText.text = isVictory ? "CHIEN THANG!" : "GAME OVER";
                endTitleText.color = isVictory ? victoryColor : gameOverColor;
            }

            if (endStatsText != null && gameManager != null)
            {
                string timeStr = GameManager.FormatTime(gameManager.SurvivalTime);
                string newRecordTag = gameManager.IsNewHighScore ? "  (KY LUC MOI!)" : string.Empty;

                endStatsText.text =
                    $"Thoi gian song sot:  {timeStr}\n" +
                    $"So quai da ha:       {gameManager.KillCount}\n" +
                    $"Diem so:             {gameManager.Score}\n" +
                    $"Ky luc cao nhat:     {gameManager.HighScore}{newRecordTag}";
            }
        }

        private void HandleResumeClicked()
        {
            gameManager?.ResumeGame();
        }

        private void HandleRestartClicked()
        {
            gameManager?.RestartGameplay();
        }

        private void HandleMainMenuClicked()
        {
            gameManager?.LoadMainMenu();
        }
    }
}