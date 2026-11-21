using UnityEngine;

namespace SurvivorGame.Data
{
    public enum WeaponType
    {
        AutoTarget, // Vũ khí 1: Đạn tự nhắm quái gần nhất
        Orbit,      // Vũ khí 2: Vật xoay quanh người
        AreaPulse   // Vũ khí 3: Vùng sát thương định kỳ
    }

    /// <summary>
    /// Thông số chi tiết cho từng cấp độ của vũ khí (từ cấp 1 đến cấp 5).
    /// </summary>
    [System.Serializable]
    public struct WeaponLevelStats
    {
        [Min(1)] public int damage;
        [Min(0.05f)] public float cooldown;        // Thời gian giữa 2 lần tấn công (giây)
        [Min(0.5f)] public float range;            // Tầm bắn / Bán kính xoay / Bán kính vùng nổ
        [Min(0f)] public float projectileSpeed;    // Tốc độ bay của đạn / Tốc độ xoay (độ/giây)
        [Min(1)] public int projectileCount;       // Số lượng đạn mỗi loạt / Số vật xoay
        [TextArea(1, 2)] public string upgradeDescription; // Mô tả hiển thị trên bảng chọn nâng cấp
    }

    /// <summary>
    /// Asset dữ liệu cấu hình cho một loại vũ khí.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData_New", menuName = "Survivor Game/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Thông tin chung")]
        [SerializeField] private string weaponId;
        [SerializeField] private string weaponName;
        [SerializeField] private WeaponType weaponType;
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject projectilePrefab;

        [Header("Chỉ số theo từng cấp (Tối đa 5 cấp)")]
        [SerializeField] private WeaponLevelStats[] levels = new WeaponLevelStats[5];

        public string WeaponId => weaponId;
        public string WeaponName => weaponName;
        public WeaponType Type => weaponType;
        public Sprite Icon => icon;
        public GameObject ProjectilePrefab => projectilePrefab;
        public int MaxLevel => levels != null ? levels.Length : 0;

        /// <summary>
        /// Lấy bộ chỉ số tương ứng với cấp độ hiện tại (tính từ 1 đến MaxLevel).
        /// </summary>
        public WeaponLevelStats GetStats(int level)
        {
            if (levels == null || levels.Length == 0)
            {
                return new WeaponLevelStats
                {
                    damage = 10,
                    cooldown = 1f,
                    range = 6f,
                    projectileSpeed = 10f,
                    projectileCount = 1,
                    upgradeDescription = "Mặc định"
                };
            }

            int index = Mathf.Clamp(level - 1, 0, levels.Length - 1);
            return levels[index];
        }
    }
}