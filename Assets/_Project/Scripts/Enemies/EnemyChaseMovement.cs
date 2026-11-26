using UnityEngine;

namespace SurvivorGame.Enemies
{
    /// <summary>
    /// Điều khiển quái vật đuổi theo mục tiêu (Player) bằng Rigidbody2D trong FixedUpdate.
    /// Khi _canMove = false, script nhường toàn quyền điều khiển vận tốc cho BossAttackController hoặc trạng thái đứng bắn.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyChaseMovement : MonoBehaviour
    {
        [Header("Thông số di chuyển")]
        [SerializeField, Min(0f)] private float moveSpeed = 3f;
        [SerializeField, Min(0f)] private float stopDistance = 0.15f;

        private Rigidbody2D _rb;
        private Transform _target;
        private bool _canMove = true;
        private Vector2 _knockbackVelocity;
        private float _knockbackTimer;

        public float MoveSpeed => moveSpeed;
        public bool CanMove => _canMove;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
        }

        private void OnEnable()
        {
            _canMove = true;
            _knockbackTimer = 0f;
            _knockbackVelocity = Vector2.zero;
        }

        public void SetMoveSpeed(float speed)
        {
            moveSpeed = Mathf.Max(0f, speed);
        }

        public void SetStopDistance(float distance)
        {
            stopDistance = Mathf.Max(0f, distance);
        }

        public void SetTarget(Transform target)
        {
            _target = target;
            if (_target == null && _knockbackTimer <= 0f && _rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
            }
        }

        public void SetCanMove(bool canMove)
        {
            _canMove = canMove;
            if (!_canMove && _knockbackTimer <= 0f && _rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
            }
        }

        /// <summary>
        /// Đẩy lùi quái trong khoảng thời gian ngắn (ưu tiên trước trạng thái đứng yên để quái bắn xa vẫn bị đẩy lùi).
        /// </summary>
        public void ApplyKnockback(Vector2 velocity, float duration)
        {
            if (duration <= 0f) return;

            _knockbackVelocity = velocity;
            _knockbackTimer = duration;
        }

        public void StopMovement()
        {
            _knockbackTimer = 0f;
            _knockbackVelocity = Vector2.zero;
            SetCanMove(false);
        }

        private void FixedUpdate()
        {
            if (_knockbackTimer > 0f)
            {
                _knockbackTimer -= Time.fixedDeltaTime;
                if (_knockbackTimer > 0f)
                {
                    _rb.linearVelocity = _knockbackVelocity;
                    return;
                }

                _knockbackTimer = 0f;
                _knockbackVelocity = Vector2.zero;
                if (!_canMove || _target == null)
                {
                    _rb.linearVelocity = Vector2.zero;
                    return;
                }
            }

            // Khi bị tắt di chuyển thì KHÔNG ghi vận tốc nữa, để Boss lao tới hoặc quái bắn xa tự điều khiển.
            // SetCanMove(false) đã đưa vận tốc về 0 đúng một lần lúc tắt.
            if (!_canMove || _target == null) return;

            Vector2 toTarget = (Vector2)_target.position - _rb.position;
            float sqrDist = toTarget.sqrMagnitude;

            if (sqrDist <= stopDistance * stopDistance)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 direction = toTarget / Mathf.Sqrt(sqrDist);
            _rb.linearVelocity = direction * moveSpeed;
        }
    }
}