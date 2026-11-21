using System.Collections.Generic;
using UnityEngine;
using SurvivorGame.Systems;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Lắng nghe UpgradeManager để bật/tắt bảng chọn nâng cấp và đổ dữ liệu vào 3 thẻ.
    /// Gắn script này lên đối tượng luôn bật (ví dụ Canvas), KHÔNG gắn lên Panel bị tắt sẵn.
    /// </summary>
    public class UpgradePanelUI : MonoBehaviour
    {
        [Header("Tham chiếu")]
        [SerializeField] private UpgradeManager upgradeManager;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private UpgradeCardUI[] cards;

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
        }

        private void OnDisable()
        {
            if (upgradeManager != null)
            {
                upgradeManager.OnUpgradeChoicesReady -= HandleChoicesReady;
                upgradeManager.OnUpgradeMenuClosed -= HandleMenuClosed;
            }
        }

        private void Update()
        {
            // Hỗ trợ phím tắt 1, 2, 3 chọn nhanh thẻ khi bảng đang mở (chạy được ngay cả khi Time.timeScale = 0)
            if (!_isOpen) return;

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
        }

        private void HandleChoicesReady(IReadOnlyList<UpgradeOption> choices)
        {
            if (panelRoot == null || cards == null) return;

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
            if (!_isOpen || upgradeManager == null) return;
            upgradeManager.SelectUpgrade(index);
        }
    }
}