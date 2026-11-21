using UnityEngine;
using SurvivorGame.Data;
using SurvivorGame.Systems;

namespace SurvivorGame.Weapons
{
    /// <summary>
    /// Vũ khí 1: Tự động tìm quái gần nhất trong tầm và bắn đạn về phía đó.
    /// </summary>
    public class AutoTargetWeapon : WeaponBase
    {
        [Header("Cấu hình quét mục tiêu & Bắn")]
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float multiShotSpreadAngle = 12f;
        [SerializeField] private float projectileLifetime = 2.5f;

        // Bộ đệm cấp phát 1 lần duy nhất để tránh sinh rác bộ nhớ (GC) khi quét quái
        private readonly Collider2D[] _hitBuffer = new Collider2D[64];
        private ContactFilter2D _enemyFilter;

        private void Awake()
        {
            _enemyFilter = new ContactFilter2D();
            _enemyFilter.SetLayerMask(enemyLayer);
            _enemyFilter.useTriggers = true;
        }

        protected override bool Attack()
        {
            if (weaponData.ProjectilePrefab == null) return false;

            WeaponLevelStats stats = CurrentStats;
            Transform target = FindNearestEnemy(stats.range);
            if (target == null) return false;

            Vector2 baseDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;
            int damage = GetEffectiveDamage();
            int count = Mathf.Max(1, stats.projectileCount);

            // Nếu bắn nhiều viên (cấp 3 trở lên), xòe đều góc bắn quanh hướng mục tiêu
            float startAngle = -multiShotSpreadAngle * (count - 1) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                float angleOffset = startAngle + multiShotSpreadAngle * i;
                Vector2 shotDir = RotateVector2(baseDirection, angleOffset);
                SpawnProjectile(shotDir, stats.projectileSpeed, damage);
            }

            return true;
        }

        /// <summary>
        /// Quét vùng tròn quanh nhân vật và tìm đối tượng thuộc Layer Enemy gần nhất còn sống.
        /// </summary>
        private Transform FindNearestEnemy(float range)
        {
            int hitCount = Physics2D.OverlapCircle(transform.position, range, _enemyFilter, _hitBuffer);
            if (hitCount == 0) return null;

            Transform nearest = null;
            float minSqrDist = float.MaxValue;
            Vector2 origin = transform.position;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D col = _hitBuffer[i];
                if (col == null || !col.gameObject.activeInHierarchy) continue;

                // Bỏ qua quái đã hết máu
                if (col.TryGetComponent<Health>(out Health enemyHealth) && enemyHealth.IsDead)
                {
                    continue;
                }

                float sqrDist = ((Vector2)col.transform.position - origin).sqrMagnitude;
                if (sqrDist < minSqrDist)
                {
                    minSqrDist = sqrDist;
                    nearest = col.transform;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Tạo viên đạn và phóng đi (sẽ chuyển sang lấy từ Object Pool ở GĐ6).
        /// </summary>
        protected virtual void SpawnProjectile(Vector2 direction, float speed, int damage)
        {
            GameObject obj = Instantiate(weaponData.ProjectilePrefab, transform.position, Quaternion.identity);
            if (obj.TryGetComponent<Projectile>(out Projectile projectile))
            {
                projectile.Initialize(direction, speed, damage, projectileLifetime);
            }
        }

        private static Vector2 RotateVector2(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        // Vẽ vòng tròn tầm bắn trong cửa sổ Scene để dễ cân chỉnh
        private void OnDrawGizmosSelected()
        {
            if (weaponData == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, CurrentStats.range);
        }
    }
}