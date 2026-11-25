using UnityEngine;

namespace SurvivorGame.Data
{
    public enum PassiveUpgradeType
    {
        DamagePercent,     // Tăng % sát thương cho mọi vũ khí (VD: 0.15 = +15%)
        CooldownReduction, // Giảm % thời gian hồi chiêu (VD: 0.10 = -10%)
        MaxHealth,         // Tăng máu tối đa và hồi đúng lượng vừa tăng (VD: 20 = +20 HP)
        MagnetRadius,      // Tăng bán kính hút ngọc EXP (VD: 1.0 = +1.0 đơn vị)
        MoveSpeed          // Tăng tốc độ di chuyển (VD: 0.5 = +0.5 đơn vị/giây)
    }

    /// <summary>
    /// Dữ liệu cấu hình cho một thẻ nâng cấp bị động.
    /// </summary>
    [CreateAssetMenu(fileName = "Passive_New", menuName = "Survivor Game/Passive Upgrade Data")]
    public class PassiveUpgradeData : ScriptableObject
    {
        [Header("Thông tin hiển thị")]
        [SerializeField] private string upgradeId;
        [SerializeField] private string upgradeName;
        [SerializeField] private Sprite icon;
        [SerializeField, TextArea(1, 2)] private string description;

        [Header("Hiệu ứng chỉ số")]
        [SerializeField] private PassiveUpgradeType upgradeType;
        [SerializeField, Min(1)] private int maxLevel = 5;
        [SerializeField, Min(0.01f)] private float valuePerLevel = 0.15f;

        public string UpgradeId => upgradeId;
        public string UpgradeName => upgradeName;
        public Sprite Icon => icon;
        public string Description => description;
        public PassiveUpgradeType UpgradeType => upgradeType;
        public int MaxLevel => maxLevel;
        public float ValuePerLevel => valuePerLevel;
    }
}
