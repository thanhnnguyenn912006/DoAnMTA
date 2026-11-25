using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SurvivorGame.Systems;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Điều khiển màn hình MainMenu: hiển thị kỷ lục lưu trong PlayerPrefs, bắt đầu trận và thoát game.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Cấu hình chuyển cảnh")]
        [SerializeField] private string gameplaySceneName = "Gameplay";

        [Header("Thành phần UI")]
        [SerializeField] private TMP_Text recordText;
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;

        private void Awake()
        {
            Time.timeScale = 1f;

            if (playButton != null) playButton.onClick.AddListener(HandlePlayClicked);
            if (quitButton != null) quitButton.onClick.AddListener(HandleQuitClicked);
        }

        private void Start()
        {
            RefreshRecordDisplay();
        }

        private void OnDestroy()
        {
            if (playButton != null) playButton.onClick.RemoveListener(HandlePlayClicked);
            if (quitButton != null) quitButton.onClick.RemoveListener(HandleQuitClicked);
        }

        private void RefreshRecordDisplay()
        {
            if (recordText == null) return;

            int highScore = PlayerPrefs.GetInt(GameManager.HighScoreKey, 0);
            float bestTime = PlayerPrefs.GetFloat(GameManager.BestTimeKey, 0f);

            recordText.text = $"KỶ LỤC: {highScore} ĐIỂM   |   LÂU NHẤT: {GameManager.FormatTime(bestTime)}";
        }

        private void HandlePlayClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameplaySceneName);
        }

        private void HandleQuitClicked()
        {
#if UNITY_EDITOR
            Debug.Log("[MainMenuUI] Thoat tro choi.");
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        [ContextMenu("Test: Xoa ky luc PlayerPrefs")]
        private void ResetSavedRecords()
        {
            PlayerPrefs.DeleteKey(GameManager.HighScoreKey);
            PlayerPrefs.DeleteKey(GameManager.BestTimeKey);
            PlayerPrefs.Save();
            RefreshRecordDisplay();
#if UNITY_EDITOR
            Debug.Log("[MainMenuUI] Da xoa ky luc.");
#endif
        }
    }
}