using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SurvivorGame.Systems;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Lắng nghe UpgradeManager để bật/tắt bảng chọn nâng cấp, nút Đổi thẻ (Reroll) và đổ dữ liệu vào 3 thẻ.
    /// Gắn script này lên đối tượng luôn bật (ví dụ Canvas), KHÔNG gắn lên Panel bị tắt sẵn.
    /// </summary>
    public class UpgradePanelUI : MonoBehaviour
    {
        [Header("Tham chiếu")]
        [SerializeField] private UpgradeManager upgradeManager;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private UpgradeCardUI[] cards;
        [SerializeField] private Button rerollButton;
        [SerializeField] private TMP_Text rerollButtonText;

        private bool _isOpen;
        private int _activeChoiceCount;

        private void Awake()
        {
            if (upgradeManager == null)
            {
                upgradeManager = FindAnyObjectByType<UpgradeManager>();
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
                upgradeManager.OnUpgradeChoicesReady += HandleChoicesReady;
                upgradeManager.OnUpgradeMenuClosed += HandleMenuClosed;
            }

            if (rerollButton != null)
            {
                rerollButton.onClick.AddListener(HandleRerollClicked);
            }
        }

        private void OnDisable()
        {
            if (upgradeManager != null)
            {
                upgradeManager.OnUpgradeChoicesReady -= HandleChoicesReady;
                upgradeManager.OnUpgradeMenuClosed -= HandleMenuClosed;
            }

            if (rerollButton != null)
            {
                rerollButton.onClick.RemoveListener(HandleRerollClicked);
            }
        }

        private void Update()
        {
            // Chỉ nhận phím tắt 1, 2, 3 và R khi bảng thực sự đang mở và trận chưa kết thúc
            if (!CanAcceptSelection()) return;

            if (_activeChoiceCount >= 1 && Input.GetKeyDown(KeyCode.Alpha1))
            {
                HandleCardSelected(0);
            }
            else if (_activeChoiceCount >= 2 && Input.GetKeyDown(KeyCode.Alpha2))
            {
                HandleCardSelected(1);
            }
            else if (_activeChoiceCount >= 3 && Input.GetKeyDown(KeyCode.Alpha3))
            {
                HandleCardSelected(2);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                HandleRerollClicked();
            }
        }

        private bool CanAcceptSelection()
        {
            if (!_isOpen || panelRoot == null || !panelRoot.activeInHierarchy)
            {
                return false;
            }

            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.LevelUp)
            {
                return false;
            }

            return true;
        }

        private void HandleChoicesReady(IReadOnlyList<UpgradeOption> choices)
        {
            if (panelRoot == null || cards == null) return;

            if (GameManager.Instance != null &&
                (GameManager.Instance.CurrentState == GameState.GameOver ||
                 GameManager.Instance.CurrentState == GameState.Victory))
            {
                return;
            }

            _activeChoiceCount = Mathf.Min(choices.Count, cards.Length);

            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null) continue;

                if (i < _activeChoiceCount)
                {
                    cards[i].Setup(choices[i], i, HandleCardSelected);
                }
                else
                {
                    cards[i].Hide();
                }
            }

            _isOpen = true;
            RefreshRerollButton();
            panelRoot.SetActive(true);
        }

        private void HandleMenuClosed()
        {
            _isOpen = false;
            _activeChoiceCount = 0;

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void HandleCardSelected(int index)
        {
            if (!CanAcceptSelection() || upgradeManager == null) return;
            upgradeManager.SelectUpgrade(index);
        }

        private void HandleRerollClicked()
        {
            if (!CanAcceptSelection() || upgradeManager == null) return;
            if (upgradeManager.TryRerollChoices())
            {
                RefreshRerollButton();
            }
        }

        private void RefreshRerollButton()
        {
            if (rerollButton == null) return;

            int remaining = upgradeManager != null ? upgradeManager.RemainingRerolls : 0;
            bool canReroll = upgradeManager != null && upgradeManager.CanReroll;
            rerollButton.interactable = canReroll;

            if (rerollButtonText != null)
            {
                rerollButtonText.text = remaining > 0
                    ? $"ĐỔI THẺ [R] ({remaining})"
                    : "HẾT LƯỢT ĐỔI (0)";
            }
        }
    }
}