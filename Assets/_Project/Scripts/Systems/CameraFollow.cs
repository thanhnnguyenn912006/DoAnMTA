using UnityEngine;

namespace SurvivorGame.Systems
{
    /// <summary>
    /// Điều khiển Camera bám mượt theo mục tiêu (Player) và hỗ trợ giới hạn vùng bản đồ.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Mục tiêu & Độ mượt")]
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.15f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        [Header("Giới hạn bản đồ (Tùy chọn)")]
        [SerializeField] private bool useBounds = false;
        [SerializeField] private Vector2 minBounds = new Vector2(-20f, -20f);
        [SerializeField] private Vector2 maxBounds = new Vector2(20f, 20f);

        private Vector3 _currentVelocity = Vector3.zero;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;

            if (useBounds)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
            }

            // Giữ nguyên trục Z của camera để không làm mất hình 2D
            desiredPosition.z = offset.z;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref _currentVelocity,
                smoothTime
            );
        }

        /// <summary>
        /// Gán lại mục tiêu cần bám theo khi cần khởi tạo động.
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}