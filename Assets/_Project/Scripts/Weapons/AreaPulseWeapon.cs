using UnityEngine;
using SurvivorGame.Data;
using SurvivorGame.Enemies;
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

        private const float KnockbackDuration = 0.16f;

        private bool _isAnimatingPulse;
        private float _pulseAnimTimer;
        private float _currentPulseMaxDiameter;
        private Color _activePulseColor;

        private void Awake()
        {
            _enemyFilter = new ContactFilter2D();
            _enemyFilter.SetLayerMask(enemyLayer);
            _enemyFilter.useTriggers = true;
            _activePulseColor = pulseColor;

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
            float knockbackForce = IsEvolved && weaponData != null ? weaponData.EvolvedKnockbackForce : 0f;
            Vector2 origin = transform.position;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D col = _hitBuffer[i];
                if (col == null || !col.gameObject.activeInHierarchy) continue;

                if (col.TryGetComponent<Health>(out Health enemyHealth) && !enemyHealth.IsDead)
                {
                    enemyHealth.TakeDamage(damage);

                    if (knockbackForce > 0f &&
                        !enemyHealth.IsDead &&
                        col.gameObject.activeInHierarchy &&
                        !col.CompareTag("Boss") &&
                        col.TryGetComponent<EnemyChaseMovement>(out EnemyChaseMovement chaseMovement))
                    {
                        Vector2 pushDir = (Vector2)col.transform.position - origin;
                        pushDir = pushDir.sqrMagnitude > 0.0001f ? pushDir.normalized : Vector2.right;
                        chaseMovement.ApplyKnockback(pushDir * knockbackForce, KnockbackDuration);
                    }
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
            Color c = _activePulseColor;
            c.a = Mathf.Lerp(_activePulseColor.a, 0f, progress);
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

            if (!pulseVisual.gameObject.activeSelf)
            {
                pulseVisual.gameObject.SetActive(true);
            }

            _currentPulseMaxDiameter = radius * 2f;
            _pulseAnimTimer = 0f;
            _isAnimatingPulse = true;

            if (IsEvolved && weaponData != null)
            {
                Color evo = weaponData.EvolvedTintColor;
                _activePulseColor = new Color(evo.r, evo.g, evo.b, Mathf.Max(pulseColor.a, 0.78f));
            }
            else
            {
                _activePulseColor = pulseColor;
            }

            pulseVisual.transform.localPosition = Vector3.zero;
            pulseVisual.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
            pulseVisual.color = _activePulseColor;
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