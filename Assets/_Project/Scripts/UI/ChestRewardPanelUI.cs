using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SurvivorGame.Systems;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Hiển thị popup phần thưởng khi người chơi nhặt Rương Báu từ Quái Tinh Anh (tạm dừng game giống Vampire Survivors).
    /// Gắn script này lên đối tượng luôn bật (Canvas), KHÔNG gắn lên Panel bị tắt sẵn.
    /// </summary>
    public class ChestRewardPanelUI : MonoBehaviour
    {
        private static readonly Color NormalHeaderColor = new Color(0.980f, 0.800f, 0.082f, 1f);  // #FACC15
        private static readonly Color EvoHeaderColor = new Color(1f, 0.62f, 0.12f, 1f);           // Cam vàng rực

        [Header("Tham chiếu hệ thống")]
        [SerializeField] private UpgradeManager upgradeManager;
        [SerializeField] private GameManager gameManager;

        [Header("Thành phần Panel Rương")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Button closeButton;

        private bool _isOpen;
        private int _openedFrame = -1;

        private void Awake()
        {
            if (upgradeManager == null) upgradeManager = FindAnyObjectByType<UpgradeManager>();
            if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(HandleCloseClicked);
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (upgradeManager != null)
            {
                upgradeManager.OnChestRewardReady += HandleChestRewardReady;
            }

            if (gameManager != null)
            {
                gameManager.OnGameStateChanged += HandleGameStateChanged;
            }
        }

        private void OnDisable()
        {
            if (upgradeManager != null)
            {
                upgradeManager.OnChestRewardReady -= HandleChestRewardReady;
            }

            if (gameManager != null)
            {
                gameManager.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(HandleCloseClicked);
            }
        }

        private void Update()
        {
            if (!_isOpen || Time.frameCount <= _openedFrame) return;

            if (Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter) ||
                Input.GetKeyDown(KeyCode.Escape))
            {
                HandleCloseClicked();
            }
        }

        private void HandleChestRewardReady(ChestRewardResult reward)
        {
            if (panelRoot == null) return;

            if (headerText != null)
            {
                headerText.text = reward.Header;
                headerText.color = reward.IsEvolution ? EvoHeaderColor : NormalHeaderColor;
            }

            if (titleText != null) titleText.text = reward.Title;
            if (subtitleText != null) subtitleText.text = reward.Subtitle;
            if (descriptionText != null) descriptionText.text = reward.Description;

            if (iconImage != null)
            {
                bool hasIcon = reward.Icon != null;
                iconImage.enabled = hasIcon;
                if (hasIcon) iconImage.sprite = reward.Icon;
            }

            _isOpen = true;
            _openedFrame = Time.frameCount;
            panelRoot.SetActive(true);
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver || state == GameState.Victory)
            {
                HideImmediate();
            }
        }

        private void HandleCloseClicked()
        {
            if (!_isOpen) return;

            AudioManager.Instance?.PlayButtonClick();
            HideImmediate();

            GameManager gm = gameManager != null ? gameManager : GameManager.Instance;
            gm?.EndChestReward();
        }

        private void HideImmediate()
        {
            _isOpen = false;
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }
    }
}
