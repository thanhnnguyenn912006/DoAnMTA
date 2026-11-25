using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SurvivorGame.Data;
using SurvivorGame.Player;
using SurvivorGame.Systems;
using SurvivorGame.Weapons;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Hiển thị danh sách vũ khí đang sở hữu và nâng cấp bị động kèm cấp độ hiện tại trên HUD.
    /// Chỉ cập nhật khi trận bắt đầu và khi có thay đổi từ UpgradeManager (không chạy Update mỗi frame).
    /// </summary>
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    public class WeaponBarHUD : MonoBehaviour
    {
        [Header("Tham chiếu (để trống sẽ tự tìm)")]
        [SerializeField] private UpgradeManager upgradeManager;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private Transform weaponsContainer;

        [Header("Danh sách nâng cấp bị động cần theo dõi")]
        [SerializeField] private PassiveUpgradeData[] trackedPassives;

        [Header("Cấu hình ô hiển thị")]
        [SerializeField, Min(24f)] private float slotSize = 46f;
        [SerializeField] private Sprite slotSprite;
        [SerializeField] private Material levelTextMaterial;
        [SerializeField] private Color weaponSlotBgColor = new Color(0.10f, 0.16f, 0.26f, 0.85f);
        [SerializeField] private Color passiveSlotBgColor = new Color(0.20f, 0.14f, 0.28f, 0.85f);
        [SerializeField] private Color normalLevelColor = Color.white;
        [SerializeField] private Color maxLevelColor = new Color(1f, 0.84f, 0.31f, 1f);

        private sealed class SlotView
        {
            public GameObject Root;
            public Image Background;
            public Image Icon;
            public TextMeshProUGUI LevelText;
        }

        private readonly List<SlotView> _slots = new List<SlotView>(8);

        private void Awake()
        {
            HorizontalLayoutGroup layout = GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            if (upgradeManager == null)
            {
                upgradeManager = FindAnyObjectByType<UpgradeManager>();
            }

            if (playerStats == null || weaponsContainer == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    if (playerStats == null) playerStats = playerObj.GetComponent<PlayerStats>();
                    if (weaponsContainer == null) weaponsContainer = playerObj.transform;
                }
            }
        }

        private void OnEnable()
        {
            if (upgradeManager != null)
            {
                upgradeManager.OnUpgradeChoicesReady += HandleChoicesReady;
                upgradeManager.OnUpgradeMenuClosed += Refresh;
            }
        }

        private void OnDisable()
        {
            if (upgradeManager != null)
            {
                upgradeManager.OnUpgradeChoicesReady -= HandleChoicesReady;
                upgradeManager.OnUpgradeMenuClosed -= Refresh;
            }
        }

        private void Start()
        {
            Refresh();
        }

        private void HandleChoicesReady(IReadOnlyList<UpgradeOption> _)
        {
            // Cập nhật ngay cả khi lên nhiều cấp liên tiếp và bảng chọn mở tiếp lượt sau
            Refresh();
        }

        public void Refresh()
        {
            int usedCount = 0;

            // 1. Hiển thị các vũ khí đang được mở khóa (Active)
            if (weaponsContainer != null)
            {
                WeaponBase[] weapons = weaponsContainer.GetComponentsInChildren<WeaponBase>(true);
                for (int i = 0; i < weapons.Length; i++)
                {
                    WeaponBase weapon = weapons[i];
                    if (weapon == null || weapon.Data == null || !weapon.gameObject.activeSelf) continue;

                    SetSlot(
                        usedCount++,
                        weapon.Data.Icon,
                        weapon.IsMaxLevel ? "MAX" : $"Lv.{weapon.CurrentLevel}",
                        weaponSlotBgColor,
                        weapon.IsMaxLevel);
                }
            }

            // 2. Hiển thị các nâng cấp bị động đã sở hữu (Cấp >= 1)
            if (playerStats != null && trackedPassives != null)
            {
                for (int i = 0; i < trackedPassives.Length; i++)
                {
                    PassiveUpgradeData passive = trackedPassives[i];
                    if (passive == null) continue;

                    int lv = playerStats.GetPassiveLevel(passive);
                    if (lv <= 0) continue;

                    bool isMax = playerStats.IsPassiveMaxed(passive);
                    SetSlot(
                        usedCount++,
                        passive.Icon,
                        isMax ? "MAX" : $"Lv.{lv}",
                        passiveSlotBgColor,
                        isMax);
                }
            }

            // Ẩn các ô thừa chưa dùng
            for (int i = usedCount; i < _slots.Count; i++)
            {
                _slots[i].Root.SetActive(false);
            }
        }

        private void SetSlot(int index, Sprite icon, string levelLabel, Color bgColor, bool isMax)
        {
            SlotView slot = GetOrCreateSlot(index);
            slot.Root.SetActive(true);
            slot.Background.color = bgColor;

            bool hasIcon = icon != null;
            slot.Icon.enabled = hasIcon;
            if (hasIcon) slot.Icon.sprite = icon;

            slot.LevelText.text = levelLabel;
            slot.LevelText.color = isMax ? maxLevelColor : normalLevelColor;
        }

        private SlotView GetOrCreateSlot(int index)
        {
            while (_slots.Count <= index)
            {
                _slots.Add(CreateSlot(_slots.Count));
            }

            return _slots[index];
        }

        private SlotView CreateSlot(int index)
        {
            GameObject rootObj = new GameObject(
                $"Slot_{index}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(LayoutElement));
            rootObj.transform.SetParent(transform, false);

            RectTransform rootRect = rootObj.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(slotSize, slotSize);

            LayoutElement layoutElement = rootObj.GetComponent<LayoutElement>();
            layoutElement.preferredWidth = slotSize;
            layoutElement.preferredHeight = slotSize;

            Image bgImage = rootObj.GetComponent<Image>();
            if (slotSprite != null)
            {
                bgImage.sprite = slotSprite;
                bgImage.type = Image.Type.Sliced;
            }
            bgImage.raycastTarget = false;

            // Icon ở giữa ô
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObj.transform.SetParent(rootObj.transform, false);

            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(5f, 5f);
            iconRect.offsetMax = new Vector2(-5f, -5f);

            Image iconImg = iconObj.GetComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // Nhãn cấp độ ở góc dưới-phải
            GameObject labelObj = new GameObject("LevelText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObj.transform.SetParent(rootObj.transform, false);

            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0.45f);
            labelRect.offsetMin = new Vector2(2f, 1f);
            labelRect.offsetMax = new Vector2(-3f, 0f);

            TextMeshProUGUI labelText = labelObj.GetComponent<TextMeshProUGUI>();
            if (levelTextMaterial != null)
            {
                labelText.fontSharedMaterial = levelTextMaterial;
            }
            labelText.fontSize = 13f;
            labelText.fontStyle = FontStyles.Bold;
            labelText.alignment = TextAlignmentOptions.BottomRight;
            labelText.overflowMode = TextOverflowModes.Overflow;
            labelText.raycastTarget = false;

            return new SlotView
            {
                Root = rootObj,
                Background = bgImage,
                Icon = iconImg,
                LevelText = labelText
            };
        }
    }
}