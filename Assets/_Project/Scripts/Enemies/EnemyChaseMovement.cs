using UnityEngine;

namespace SurvivorGame.Enemies
{
    /// <summary>
    /// Điều khiển quái di chuyển đuổi theo mục tiêu (Player) bằng Rigidbody2D.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyChaseMovement : MonoBehaviour
    {
        [Header("Thông số di chuyển")]
        [SerializeField, Min(0.1f)] private float moveSpeed = 2.5f;
        [SerializeField, Min(0f)] private float stopDistance = 0.1f;

        private Rigidbody2D _rb;
        private Transform _target;
        private bool _canMove = true;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (!_canMove || _target == null)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 toTarget = (Vector2)_target.position - _rb.position;
            float sqrDist = toTarget.sqrMagnitude;

            if (sqrDist <= stopDistance * stopDistance)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            // Chuẩn hóa hướng và gán vận tốc bằng linearVelocity chuẩn Unity 6
            Vector2 direction = toTarget / Mathf.Sqrt(sqrDist);
            _rb.linearVelocity = direction * moveSpeed;
        }

        /// <summary>
        /// Gán mục tiêu cần đuổi theo.
        /// </summary>
        public void SetTarget(Transform target)
        {
            _target = target;
        }

        /// <summary>
        /// Cập nhật tốc độ di chuyển từ EnemyData hoặc hệ số độ khó.
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = Mathf.Max(0.1f, speed);
        }

        /// <summary>
        /// Cho phép hoặc tạm dừng di chuyển.
        /// </summary>
        public void SetCanMove(bool canMove)
        {
            _canMove = canMove;
            if (!_canMove && _rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
            }
        }

        /// <summary>
        /// Dừng hẳn quái ngay lập tức (khi chết hoặc trả về Pool).
        /// </summary>
        public void StopMovement()
        {
            SetCanMove(false);
        }
    }
}