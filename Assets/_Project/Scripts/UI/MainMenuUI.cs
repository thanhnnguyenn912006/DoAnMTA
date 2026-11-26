using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SurvivorGame.Data;
using SurvivorGame.Player;
using SurvivorGame.Systems;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Điều khiển màn hình MainMenu: hiển thị kỷ lục, chọn 1 trong 3 nhân vật khởi đầu, bắt đầu trận và thoát game.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        private static readonly Color CardNormalColor = new Color(0.118f, 0.161f, 0.231f, 1f);    // #1E293B
        private static readonly Color CardHighlightColor = new Color(0.180f, 0.231f, 0.322f, 1f); // #2E3B52
        private static readonly Color CardSelectedColor = new Color(0.522f, 0.302f, 0.055f, 1f);  // #854D0E
        private static readonly Color CardSelectedHover = new Color(0.706f, 0.451f, 0.047f, 1f);  // #B4730C

        [Header("Cấu hình chuyển cảnh")]
        [SerializeField] private string gameplaySceneName = "Gameplay";

        [Header("Thành phần UI")]
        [SerializeField] private TMP_Text recordText;
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;

        [Header("Chọn nhân vật (tùy chọn)")]
        [SerializeField] private CharacterData[] characters;
        [SerializeField] private Button[] characterButtons;

        private int _selectedCharacterIndex;

        private void Awake()
        {
            Time.timeScale = 1f;

            if (playButton != null) playButton.onClick.AddListener(HandlePlayClicked);
            if (quitButton != null) quitButton.onClick.AddListener(HandleQuitClicked);

            SetupCharacterButtons();
        }

        private void Start()
        {
            RefreshRecordDisplay();
            RefreshCharacterSelectionVisuals();
        }

        private void OnDestroy()
        {
            if (playButton != null) playButton.onClick.RemoveListener(HandlePlayClicked);
            if (quitButton != null) quitButton.onClick.RemoveListener(HandleQuitClicked);

            if (characterButtons != null)
            {
                for (int i = 0; i < characterButtons.Length; i++)
                {
                    if (characterButtons[i] != null)
                    {
                        characterButtons[i].onClick.RemoveAllListeners();
                    }
                }
            }
        }

        private void SetupCharacterButtons()
        {
            if (characters == null || characters.Length == 0 || characterButtons == null) return;

            _selectedCharacterIndex = Mathf.Clamp(
                PlayerPrefs.GetInt(PlayerCharacterSelector.SelectedCharacterKey, 0),
                0,
                characters.Length - 1);

            int count = Mathf.Min(characters.Length, characterButtons.Length);
            for (int i = 0; i < count; i++)
            {
                int index = i;
                Button btn = characterButtons[i];
                if (btn != null)
                {
                    btn.onClick.AddListener(() => SelectCharacter(index));
                }
            }
        }

        public void SelectCharacter(int index)
        {
            if (characters == null || characters.Length == 0) return;

            _selectedCharacterIndex = Mathf.Clamp(index, 0, characters.Length - 1);
            PlayerPrefs.SetInt(PlayerCharacterSelector.SelectedCharacterKey, _selectedCharacterIndex);
            PlayerPrefs.Save();

            AudioManager.Instance?.PlayButtonClick();
            RefreshCharacterSelectionVisuals();
        }

        private void RefreshCharacterSelectionVisuals()
        {
            if (characterButtons == null || characterButtons.Length == 0) return;

            for (int i = 0; i < characterButtons.Length; i++)
            {
                Button btn = characterButtons[i];
                if (btn == null) continue;

                bool isSelected = i == _selectedCharacterIndex;
                ColorBlock cb = btn.colors;
                cb.normalColor = isSelected ? CardSelectedColor : CardNormalColor;
                cb.highlightedColor = isSelected ? CardSelectedHover : CardHighlightColor;
                cb.selectedColor = cb.highlightedColor;
                cb.pressedColor = isSelected ? CardSelectedColor : new Color(0.08f, 0.10f, 0.14f, 1f);
                btn.colors = cb;
            }
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