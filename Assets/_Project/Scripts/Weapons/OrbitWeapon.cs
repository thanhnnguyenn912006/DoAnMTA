using System.Collections.Generic;
using UnityEngine;
using SurvivorGame.Data;

namespace SurvivorGame.Weapons
{
    /// <summary>
    /// Vũ khí 2: Tạo các vật thể xoay liên tục quanh người chơi theo quỹ đạo tròn.
    /// Sử dụng WeaponData: Range = bán kính, ProjectileSpeed = tốc độ xoay (độ/giây), ProjectileCount = số vật thể.
    /// </summary>
    public class OrbitWeapon : WeaponBase
    {
        [Header("Cấu hình mục tiêu")]
        [SerializeField] private LayerMask enemyLayer;

        private readonly List<OrbitOrb> _orbs = new List<OrbitOrb>();
        private float _currentAngle;

        private void OnEnable()
        {
            if (weaponData != null)
            {
                RefreshOrbs();
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < _orbs.Count; i++)
            {
                if (_orbs[i] != null)
                {
                    _orbs[i].gameObject.SetActive(false);
                }
            }
        }

        protected override void OnStatsUpdated()
        {
            base.OnStatsUpdated();
            if (isActiveAndEnabled)
            {
                RefreshOrbs();
            }
        }

        /// <summary>
        /// Vũ khí quỹ đạo hoạt động liên tục nên không dùng chu kỳ bắn Attack() mặc định.
        /// </summary>
        protected override bool UsesCooldown => false;

        protected override bool Attack()
        {
            return false;
        }

        private void LateUpdate()
        {
            if (weaponData == null || _orbs.Count == 0) return;

            WeaponLevelStats stats = CurrentStats;
            int activeCount = Mathf.Max(1, stats.projectileCount);
            float radius = Mathf.Max(0.5f, stats.range);
            float angularSpeed = stats.projectileSpeed;

            _currentAngle = (_currentAngle + angularSpeed * Time.deltaTime) % 360f;
            float angleStep = 360f / activeCount;

            for (int i = 0; i < activeCount && i < _orbs.Count; i++)
            {
                OrbitOrb orb = _orbs[i];
                if (orb == null || !orb.gameObject.activeSelf) continue;

                float rad = (_currentAngle + angleStep * i) * Mathf.Deg2Rad;
                Vector3 localOffset = new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0f);
                orb.transform.localPosition = localOffset;
            }
        }

        /// <summary>
        /// Tạo thêm hoặc bật/tắt đủ số lượng quả cầu theo cấp hiện tại và cập nhật sát thương mới.
        /// </summary>
        private void RefreshOrbs()
        {
            if (weaponData == null || weaponData.ProjectilePrefab == null) return;

            WeaponLevelStats stats = CurrentStats;
            int targetCount = Mathf.Max(1, stats.projectileCount);
            int damage = GetEffectiveDamage();
            float hitCooldown = Mathf.Max(0.15f, GetEffectiveCooldown());

            // Tạo thêm nếu chưa đủ số lượng cho cấp mới
            while (_orbs.Count < targetCount)
            {
                GameObject orbObj = Instantiate(weaponData.ProjectilePrefab, transform);
                if (!orbObj.TryGetComponent<OrbitOrb>(out OrbitOrb orb))
                {
                    orb = orbObj.AddComponent<OrbitOrb>();
                }
                _orbs.Add(orb);
            }

            Color? evolvedTint = IsEvolved ? weaponData.EvolvedTintColor : (Color?)null;
            float scaleMultiplier = IsEvolved ? 1.2f : 1f;

            // Bật đủ số lượng và cấu hình lại sát thương/hồi chiêu/ngoại hình cho từng quả cầu
            for (int i = 0; i < _orbs.Count; i++)
            {
                bool shouldBeActive = i < targetCount;
                _orbs[i].gameObject.SetActive(shouldBeActive);

                if (shouldBeActive)
                {
                    _orbs[i].Configure(damage, hitCooldown, enemyLayer, evolvedTint, scaleMultiplier, this);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (weaponData == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, CurrentStats.range);
        }
    }
}