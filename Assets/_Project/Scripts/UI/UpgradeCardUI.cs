using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SurvivorGame.Systems;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Điều khiển hiển thị và tương tác bấm chọn của một thẻ nâng cấp.
    /// </summary>
    public class UpgradeCardUI : MonoBehaviour
    {
        [Header("Thành phần trên thẻ")]
        [SerializeField] private Button selectButton;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image iconImage;

        private int _choiceIndex;
        private Action<int> _onSelectCallback;

        private void Awake()
        {
            if (selectButton == null)
            {
                selectButton = GetComponent<Button>();
            }

            if (selectButton != null)
            {
                selectButton.onClick.AddListener(HandleButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (selectButton != null)
            {
                selectButton.onClick.RemoveListener(HandleButtonClicked);
            }
        }

        /// <summary>
        /// Gán dữ liệu từ UpgradeOption lên thẻ và hiện thẻ.
        /// </summary>
        public void Setup(UpgradeOption option, int index, Action<int> onSelectCallback)
        {
            _choiceIndex = index;
            _onSelectCallback = onSelectCallback;

            if (titleText != null) titleText.text = option.Title;
            if (levelText != null) levelText.text = option.LevelText;
            if (descriptionText != null) descriptionText.text = option.Description;

            if (iconImage != null)
            {
                bool hasIcon = option.Icon != null;
                iconImage.enabled = hasIcon;
                if (hasIcon) iconImage.sprite = option.Icon;
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void HandleButtonClicked()
        {
            _onSelectCallback?.Invoke(_choiceIndex);
        }
    }
}