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
        private static readonly Color EvolutionNormalColor = new Color(0.522f, 0.302f, 0.055f, 1f);    // #854D0E
        private static readonly Color EvolutionHighlightColor = new Color(0.792f, 0.541f, 0.016f, 1f); // #CA8A04
        private static readonly Color EvolutionPressedColor = new Color(0.361f, 0.204f, 0.035f, 1f);   // #5C3409
        private static readonly Color EvolutionTextColor = new Color(0.992f, 0.878f, 0.278f, 1f);      // #FDE047

        [Header("Thành phần trên thẻ")]
        [SerializeField] private Button selectButton;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image iconImage;

        private int _choiceIndex;
        private Action<int> _onSelectCallback;

        private Image _cardImage;
        private ColorBlock _defaultButtonColors;
        private bool _hasDefaultButtonColors;
        private Color _defaultCardImageColor = Color.white;
        private Color _defaultTitleColor = Color.white;
        private Color _defaultLevelColor = Color.white;
        private bool _hasCachedDefaultColors;

        private void Awake()
        {
            CacheDefaultColorsIfNeeded();

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
            CacheDefaultColorsIfNeeded();

            _choiceIndex = index;
            _onSelectCallback = onSelectCallback;

            if (titleText != null) titleText.text = option.Title;
            if (levelText != null) levelText.text = option.LevelText;
            if (descriptionText != null) descriptionText.text = option.Description;

            ApplyVisualStyle(option.IsWeaponEvolution);

            if (iconImage != null)
            {
                bool hasIcon = option.Icon != null;
                iconImage.enabled = hasIcon;
                if (hasIcon) iconImage.sprite = option.Icon;
            }

            gameObject.SetActive(true);
        }

        private void CacheDefaultColorsIfNeeded()
        {
            if (_hasCachedDefaultColors) return;

            if (selectButton == null)
            {
                selectButton = GetComponent<Button>();
            }

            if (selectButton != null)
            {
                _defaultButtonColors = selectButton.colors;
                _hasDefaultButtonColors = true;
            }

            _cardImage = GetComponent<Image>();
            if (_cardImage != null)
            {
                _defaultCardImageColor = _cardImage.color;
            }

            if (titleText != null) _defaultTitleColor = titleText.color;
            if (levelText != null) _defaultLevelColor = levelText.color;

            _hasCachedDefaultColors = true;
        }

        private void ApplyVisualStyle(bool isEvolution)
        {
            if (isEvolution)
            {
                if (selectButton != null)
                {
                    ColorBlock cb = _defaultButtonColors;
                    cb.normalColor = EvolutionNormalColor;
                    cb.highlightedColor = EvolutionHighlightColor;
                    cb.selectedColor = EvolutionHighlightColor;
                    cb.pressedColor = EvolutionPressedColor;
                    selectButton.colors = cb;
                }
                else if (_cardImage != null)
                {
                    _cardImage.color = EvolutionNormalColor;
                }

                if (titleText != null) titleText.color = EvolutionTextColor;
                if (levelText != null) levelText.color = EvolutionTextColor;
                return;
            }

            if (selectButton != null && _hasDefaultButtonColors)
            {
                selectButton.colors = _defaultButtonColors;
            }
            else if (_cardImage != null)
            {
                _cardImage.color = _defaultCardImageColor;
            }

            if (titleText != null) titleText.color = _defaultTitleColor;
            if (levelText != null) levelText.color = _defaultLevelColor;
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