using System.Collections.Generic;
using UnityEngine;
using SurvivorGame.Data;
using SurvivorGame.Systems;
using SurvivorGame.Weapons;

namespace SurvivorGame.Player
{
    /// <summary>
    /// Lưu trữ và áp dụng các nâng cấp bị động lên nhân vật và toàn bộ vũ khí.
    /// </summary>
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(PlayerExperience))]
    public class PlayerStats : MonoBehaviour
    {
        [Header("Vùng chứa vũ khí (để trống sẽ tự tìm trong con của Player)")]
        [SerializeField] private Transform weaponsContainer;

        [Header("Hệ số hiện tại (chỉ đọc để theo dõi)")]
        [SerializeField] private float damageMultiplier = 1f;
        [SerializeField] private float cooldownMultiplier = 1f;

        private const float MinCooldownMultiplier = 0.3f;

        private Health _health;
        private PlayerExperience _experience;
        private PlayerMovement _movement;
        private readonly Dictionary<PassiveUpgradeData, int> _passiveLevels = new Dictionary<PassiveUpgradeData, int>();

        public float DamageMultiplier => damageMultiplier;
        public float CooldownMultiplier => cooldownMultiplier;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _experience = GetComponent<PlayerExperience>();
            TryGetComponent(out _movement);

            if (weaponsContainer == null)
            {
                weaponsContainer = transform;
            }
        }

        public int GetPassiveLevel(PassiveUpgradeData data)
        {
            if (data == null) return 0;
            return _passiveLevels.TryGetValue(data, out int level) ? level : 0;
        }

        public bool IsPassiveMaxed(PassiveUpgradeData data)
        {
            if (data == null) return true;
            return GetPassiveLevel(data) >= data.MaxLevel;
        }

        /// <summary>
        /// Áp dụng 1 cấp nâng cấp bị động và đồng bộ ngay lập tức sang các thành phần liên quan.
        /// </summary>
        public bool ApplyPassiveUpgrade(PassiveUpgradeData data)
        {
            if (data == null || IsPassiveMaxed(data)) return false;

            int newLevel = GetPassiveLevel(data) + 1;
            _passiveLevels[data] = newLevel;

            switch (data.UpgradeType)
            {
                case PassiveUpgradeType.DamagePercent:
                    damageMultiplier += data.ValuePerLevel;
                    SyncAllWeapons();
                    break;

                case PassiveUpgradeType.CooldownReduction:
                    cooldownMultiplier = Mathf.Max(MinCooldownMultiplier, cooldownMultiplier - data.ValuePerLevel);
                    SyncAllWeapons();
                    break;

                case PassiveUpgradeType.MaxHealth:
                    _health.IncreaseMaxHealth(Mathf.RoundToInt(data.ValuePerLevel), true);
                    break;

                case PassiveUpgradeType.MagnetRadius:
                    _experience.IncreaseMagnetRadius(data.ValuePerLevel);
                    break;

                case PassiveUpgradeType.MoveSpeed:
                    _movement?.AddMoveSpeed(data.ValuePerLevel);
                    break;
            }

#if UNITY_EDITOR
            Debug.Log($"[PlayerStats] Nâng cấp '{data.UpgradeName}' lên cấp {newLevel}/{data.MaxLevel}");
#endif
            return true;
        }

        /// <summary>
        /// Đồng bộ hệ số sát thương và hồi chiêu xuống toàn bộ vũ khí đang có.
        /// </summary>
        public void SyncAllWeapons()
        {
            WeaponBase[] weapons = weaponsContainer.GetComponentsInChildren<WeaponBase>();
            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].SetStatMultipliers(damageMultiplier, cooldownMultiplier);
            }
        }

        /// <summary>
        /// Áp dụng hệ số hiện tại cho một vũ khí vừa mới mở khóa (dùng cho GĐ8).
        /// </summary>
        public void ApplyMultipliersToWeapon(WeaponBase weapon)
        {
            if (weapon == null) return;
            weapon.SetStatMultipliers(damageMultiplier, cooldownMultiplier);
        }
    }
}
