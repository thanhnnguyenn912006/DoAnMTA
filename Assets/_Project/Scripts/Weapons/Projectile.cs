using System;
using UnityEngine;
using SurvivorGame.Systems;

namespace SurvivorGame.Weapons
{
    /// <summary>
    /// Điều khiển hành vi của một viên đạn: bay theo hướng chỉ định, gây sát thương khi chạm mục tiêu và tự hủy khi hết thời gian tồn tại.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("Cấu hình mặc định")]
        [SerializeField] private float defaultLifetime = 3f;

        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        private Color _defaultColor = Color.white;
        private int _damage;
        private float _lifeTimer;
        private bool _hasHit;
        private int _remainingPierces;
        private readonly Health[] _hitTargets = new Health[8];
        private int _hitCount;
        private Action<Projectile> _returnToPoolCallback;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
            {
                _defaultColor = _spriteRenderer.color;
            }
        }

        private void Update()
        {
            _lifeTimer -= Time.deltaTime;
            if (_lifeTimer <= 0f)
            {
                Despawn();
            }
        }

        /// <summary>
        /// Khởi chạy viên đạn với hướng bay, tốc độ, sát thương, số mục tiêu xuyên và màu sắc tùy chọn.
        /// </summary>
        public void Initialize(
            Vector2 direction,
            float speed,
            int damage,
            float lifetime = -1f,
            Action<Projectile> returnToPoolCallback = null,
            int pierceCount = 0,
            Color? tintColor = null)
        {
            Vector2 dir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;

            _damage = damage;
            _lifeTimer = lifetime > 0f ? lifetime : defaultLifetime;
            _hasHit = false;
            _remainingPierces = Mathf.Max(0, pierceCount);
            ClearHitTargets();
            _returnToPoolCallback = returnToPoolCallback;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = tintColor.HasValue ? tintColor.Value : _defaultColor;
            }

            // Xoay đầu viên đạn theo đúng hướng bay
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            _rb.linearVelocity = dir * speed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasHit) return;

            // Physics 2D Matrix đã lọc chỉ cho PlayerProjectile chạm Enemy hoặc Obstacle
            if (other.TryGetComponent<Health>(out Health targetHealth))
            {
                if (targetHealth.IsDead || HasAlreadyHit(targetHealth)) return;

                if (_hitCount < _hitTargets.Length)
                {
                    _hitTargets[_hitCount++] = targetHealth;
                }

                targetHealth.TakeDamage(_damage);

                if (_remainingPierces > 0)
                {
                    _remainingPierces--;
                    return;
                }
            }

            _hasHit = true;
            Despawn();
        }

        private bool HasAlreadyHit(Health targetHealth)
        {
            for (int i = 0; i < _hitCount; i++)
            {
                if (_hitTargets[i] == targetHealth)
                {
                    return true;
                }
            }
            return false;
        }

        private void ClearHitTargets()
        {
            for (int i = 0; i < _hitCount; i++)
            {
                _hitTargets[i] = null;
            }
            _hitCount = 0;
        }

        /// <summary>
        /// Trả đạn về Object Pool (nếu có) hoặc hủy đối tượng.
        /// </summary>
        private void Despawn()
        {
            _rb.linearVelocity = Vector2.zero;
            ClearHitTargets();

            if (_returnToPoolCallback != null)
            {
                _returnToPoolCallback.Invoke(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}