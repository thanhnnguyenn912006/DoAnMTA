using UnityEngine;

namespace SurvivorGame.Systems
{
    /// <summary>
    /// Thành phần dùng chung giúp đối tượng gây sát thương khi chạm vào mục tiêu thuộc Layer chỉ định.
    /// Hoạt động với cả Collider cứng (Collision) và Trigger.
    /// </summary>
    public class ContactDamage : MonoBehaviour
    {
        [Header("Cấu hình sát thương chạm")]
        [SerializeField, Min(1)] private int damage = 10;
        [SerializeField] private LayerMask targetLayer;

        private Health _selfHealth;

        public int Damage => damage;

        private void Awake()
        {
            // Tham chiếu tùy chọn: nếu đối tượng này có máu và đã chết thì ngừng gây sát thương
            TryGetComponent(out _selfHealth);
        }

        /// <summary>
        /// Cập nhật sát thương động từ ScriptableObject hoặc hệ số độ khó theo thời gian.
        /// </summary>
        public void SetDamage(int newDamage)
        {
            damage = Mathf.Max(1, newDamage);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryDealDamage(collision.gameObject);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryDealDamage(other.gameObject);
        }

        private void TryDealDamage(GameObject target)
        {
            if (_selfHealth != null && _selfHealth.IsDead) return;

            // Kiểm tra nhanh bằng phép dịch bit xem Layer của target có nằm trong targetLayer không
            if (((1 << target.layer) & targetLayer.value) == 0) return;

            if (target.TryGetComponent<Health>(out Health targetHealth))
            {
                targetHealth.TakeDamage(damage);
            }
        }
    }
}