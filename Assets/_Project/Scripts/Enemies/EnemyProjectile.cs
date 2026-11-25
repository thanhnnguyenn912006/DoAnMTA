using System;
using UnityEngine;
using SurvivorGame.Systems;

namespace SurvivorGame.Enemies
{
    /// <summary>
    /// Đạn bắn ra từ Quái tầm xa hoặc Boss.
    /// Hỗ trợ Object Pooling và chỉ gây sát thương lên mục tiêu thuộc targetLayer (Player).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class EnemyProjectile : MonoBehaviour
    {
        [Header("Cấu hình va chạm")]
        [SerializeField] private LayerMask targetLayer;

        private Rigidbody2D _rb;
        private Vector2 _direction;
        private float _speed;
        private int _damage;
        private float _lifetime;
        private float _timer;
        private bool _hasHit;
        private Action<EnemyProjectile> _returnToPoolCallback;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnDisable()
        {
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
            }
        }

        /// <summary>
        /// Khởi tạo hướng bay, tốc độ, sát thương và callback trả về Pool mỗi lần bắn.
        /// </summary>
        public void Initialize(
            Vector2 direction,
            float speed,
            int damage,
            float lifetime,
            Action<EnemyProjectile> returnToPoolCallback)
        {
            _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
            _speed = Mathf.Max(0.5f, speed);
            _damage = Mathf.Max(1, damage);
            _lifetime = Mathf.Max(0.2f, lifetime);
            _timer = 0f;
            _hasHit = false;
            _returnToPoolCallback = returnToPoolCallback;

            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            if (_rb != null)
            {
                _rb.linearVelocity = _direction * _speed;
            }
        }

        private void Update()
        {
            if (_hasHit) return;

            _timer += Time.deltaTime;
            if (_timer >= _lifetime)
            {
                Despawn();
            }
        }

        private void FixedUpdate()
        {
            if (!_hasHit && _rb != null)
            {
                _rb.linearVelocity = _direction * _speed;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasHit) return;
            if (((1 << other.gameObject.layer) & targetLayer.value) == 0) return;

            if (other.TryGetComponent<Health>(out Health targetHealth))
            {
                if (targetHealth.IsDead || targetHealth.IsInvincible) return;
                targetHealth.TakeDamage(_damage);
            }

            _hasHit = true;
            Despawn();
        }

        private void Despawn()
        {
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
            }

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
