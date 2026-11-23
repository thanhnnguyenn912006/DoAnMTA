using UnityEngine;
using SurvivorGame.Data;
using SurvivorGame.Systems;

namespace SurvivorGame.Weapons
{
    /// <summary>
    /// Vũ khí 3: Phát sóng sát thương diện rộng định kỳ quanh nhân vật.
    /// Sử dụng WeaponData: Damage = sát thương mỗi nhịp, Cooldown = chu kỳ phát sóng, Range = bán kính vùng ảnh hưởng.
    /// </summary>
    public class AreaPulseWeapon : WeaponBase
    {
        [Header("Cấu hình quét mục tiêu")]
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private bool onlyPulseWhenEnemyInRange = false;

        [Header("Hiệu ứng vòng sóng hiển thị")]
        [SerializeField] private SpriteRenderer pulseVisual;
        [SerializeField] private Color pulseColor = new Color(0.66f, 0.33f, 0.97f, 0.45f); // Tím năng lượng trong suốt
        [SerializeField, Min(0.05f)] private float pulseVisualDuration = 0.35f;

        // Bộ đệm quét cấp phát 1 lần duy nhất để giữ 0 GC Alloc
        private readonly Collider2D[] _hitBuffer = new Collider2D[160];
        private ContactFilter2D _enemyFilter;

        private bool _isAnimatingPulse;
        private float _pulseAnimTimer;
        private float _currentPulseMaxDiameter;

        private void Awake()
        {
            _enemyFilter = new ContactFilter2D();
            _enemyFilter.SetLayerMask(enemyLayer);
            _enemyFilter.useTriggers = true;

            HideVisual();
        }

        private void OnDisable()
        {
            _isAnimatingPulse = false;
            HideVisual();
        }

        protected override bool Attack()
        {
            WeaponLevelStats stats = CurrentStats;
            float radius = Mathf.Max(0.5f, stats.range);

            int hitCount = Physics2D.OverlapCircle(transform.position, radius, _enemyFilter, _hitBuffer);

            if (onlyPulseWhenEnemyInRange && hitCount == 0)
            {
                return false;
            }

            int damage = GetEffectiveDamage();

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D col = _hitBuffer[i];
                if (col == null || !col.gameObject.activeInHierarchy) continue;

                if (col.TryGetComponent<Health>(out Health enemyHealth) && !enemyHealth.IsDead)
                {
                    enemyHealth.TakeDamage(damage);
                }
            }

            StartPulseVisual(radius);
            return true;
        }

        private void LateUpdate()
        {
            if (!_isAnimatingPulse || pulseVisual == null) return;

            _pulseAnimTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(_pulseAnimTimer / pulseVisualDuration);

            // Phóng to vòng sóng từ tâm ra đúng đường kính vùng sát thương (radius * 2)
            float currentScale = Mathf.Lerp(0.25f, _currentPulseMaxDiameter, progress);
            pulseVisual.transform.localScale = new Vector3(currentScale, currentScale, 1f);

            // Mờ dần độ trong suốt về 0 khi chạm rìa
            Color c = pulseColor;
            c.a = Mathf.Lerp(pulseColor.a, 0f, progress);
            pulseVisual.color = c;

            if (progress >= 1f)
            {
                _isAnimatingPulse = false;
                HideVisual();
            }
        }

        private void StartPulseVisual(float radius)
        {
            if (pulseVisual == null) return;

            _currentPulseMaxDiameter = radius * 2f;
            _pulseAnimTimer = 0f;
            _isAnimatingPulse = true;

            pulseVisual.transform.localPosition = Vector3.zero;
            pulseVisual.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
            pulseVisual.color = pulseColor;
            pulseVisual.enabled = true;
        }

        private void HideVisual()
        {
            if (pulseVisual != null)
            {
                pulseVisual.enabled = false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (weaponData == null) return;
            Gizmos.color = new Color(0.66f, 0.33f, 0.97f, 1f);
            Gizmos.DrawWireSphere(transform.position, CurrentStats.range);
        }
    }
}