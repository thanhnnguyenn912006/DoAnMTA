using System;
using UnityEngine;
using SurvivorGame.Data;

namespace SurvivorGame.Weapons
{
    /// <summary>
    /// Lớp cơ sở trừu tượng cho mọi loại vũ khí tự động.
    /// Quản lý cấp độ, vòng lặp hồi chiêu, hệ số bị động và sự kiện.
    /// </summary>
    public abstract class WeaponBase : MonoBehaviour
    {
        [Header("Dữ liệu vũ khí")]
        [SerializeField] protected WeaponData weaponData;
        [SerializeField, Min(1)] protected int currentLevel = 1;

        protected float _cooldownTimer;
        protected float _damageMultiplier = 1f;
        protected float _cooldownMultiplier = 1f;

        // Thời gian chờ ngắn khi không tìm thấy mục tiêu để tránh quét vật lý mỗi frame
        private const float RetryDelayWhenNoTarget = 0.1f;

        public WeaponData Data => weaponData;
        public int CurrentLevel => currentLevel;
        public int MaxLevel => weaponData != null ? weaponData.MaxLevel : 1;
        public bool IsMaxLevel => currentLevel >= MaxLevel;
        public WeaponLevelStats CurrentStats => weaponData.GetStats(currentLevel);

        /// <summary>
        /// Một số vũ khí duy trì liên tục (như Vật xoay) không dùng nhịp hồi chiêu thông thường.
        /// </summary>
        protected virtual bool UsesCooldown => true;

        // Sự kiện phát ra cho AudioManager (SFX) và UI lắng nghe
        public event Action<WeaponBase> OnWeaponFired;
        public event Action<WeaponBase, int> OnLevelChanged;

        protected virtual void Start()
        {
            if (weaponData != null)
            {
                currentLevel = Mathf.Clamp(currentLevel, 1, MaxLevel);
                OnStatsUpdated();
            }
        }

        protected virtual void Update()
        {
            if (weaponData == null || !UsesCooldown) return;

            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer > 0f) return;

            if (Attack())
            {
                _cooldownTimer = GetEffectiveCooldown();
                OnWeaponFired?.Invoke(this);
            }
            else
            {
                // Chưa có mục tiêu trong tầm -> chờ một khoảng ngắn rồi kiểm tra lại
                _cooldownTimer = RetryDelayWhenNoTarget;
            }
        }

        /// <summary>
        /// Khởi tạo vũ khí bằng dữ liệu ScriptableObject lúc đang chạy.
        /// </summary>
        public virtual void Initialize(WeaponData data, int startLevel = 1)
        {
            weaponData = data;
            currentLevel = Mathf.Clamp(startLevel, 1, MaxLevel);
            _cooldownTimer = 0f;
            OnStatsUpdated();
            OnLevelChanged?.Invoke(this, currentLevel);
        }

        /// <summary>
        /// Tăng 1 cấp vũ khí nếu chưa đạt cấp tối đa.
        /// </summary>
        public virtual bool LevelUp()
        {
            if (weaponData == null || IsMaxLevel) return false;

            currentLevel++;
            OnStatsUpdated();
            OnLevelChanged?.Invoke(this, currentLevel);
            return true;
        }

        /// <summary>
        /// Cập nhật hệ số sát thương và hồi chiêu từ các nâng cấp bị động.
        /// </summary>
        public virtual void SetStatMultipliers(float damageMultiplier, float cooldownMultiplier)
        {
            _damageMultiplier = Mathf.Max(0.1f, damageMultiplier);
            _cooldownMultiplier = Mathf.Clamp(cooldownMultiplier, 0.2f, 2f);
            OnStatsUpdated();
        }

        /// <summary>
        /// Tính sát thương thực tế sau khi nhân hệ số bị động.
        /// </summary>
        protected int GetEffectiveDamage()
        {
            return Mathf.Max(1, Mathf.RoundToInt(CurrentStats.damage * _damageMultiplier));
        }

        /// <summary>
        /// Tính thời gian hồi chiêu thực tế sau khi nhân hệ số giảm hồi chiêu.
        /// </summary>
        protected float GetEffectiveCooldown()
        {
            return Mathf.Max(0.05f, CurrentStats.cooldown * _cooldownMultiplier);
        }

        /// <summary>
        /// Được gọi mỗi khi vũ khí mới khởi tạo, lên cấp hoặc thay đổi chỉ số bị động.
        /// </summary>
        protected virtual void OnStatsUpdated() { }

        /// <summary>
        /// Logic tấn công riêng của từng loại vũ khí. Trả về true nếu đã thực hiện tấn công thành công.
        /// </summary>
        protected abstract bool Attack();
    }
}