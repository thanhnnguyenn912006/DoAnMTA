using UnityEngine;

namespace SurvivorGame.Player
{
    /// <summary>
    /// Quản lý việc đọc phím điều khiển và di chuyển nhân vật bằng Rigidbody2D.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Thông số di chuyển")]
        [SerializeField] private float moveSpeed = 5f;

        private Rigidbody2D _rb;
        private Vector2 _moveInput;

        // Cho phép các hệ thống khác (ví dụ vũ khí, chỉ số bị động) đọc/tăng tốc chạy
        public float MoveSpeed => moveSpeed;
        public Vector2 MoveDirection => _moveInput;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            // Đọc input không có quán tính trượt (GetAxisRaw trả về -1, 0 hoặc 1)
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");

            // Chuẩn hóa vector để đi chéo không nhanh hơn đi thẳng
            _moveInput = new Vector2(moveX, moveY).normalized;
        }

        private void FixedUpdate()
        {
            // Dùng linearVelocity chuẩn của Unity 6 để tránh cảnh báo deprecated
            _rb.linearVelocity = _moveInput * moveSpeed;
        }

        /// <summary>
        /// Tăng tốc độ di chuyển (dùng khi người chơi chọn nâng cấp tốc chạy).
        /// </summary>
        public void AddMoveSpeed(float amount)
        {
            moveSpeed = Mathf.Max(0.5f, moveSpeed + amount);
        }

        /// <summary>
        /// Dừng hẳn nhân vật ngay lập tức (dùng khi chết hoặc thắng).
        /// </summary>
        public void StopMovement()
        {
            _moveInput = Vector2.zero;
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
            }
        }
    }
}