using System;
using UnityEngine;

namespace SurvivorGame.Systems
{
    /// <summary>
    /// Quản lý máu, nhận sát thương, hồi máu và thời gian miễn nhiễm (I-frames).
    /// Dùng chung cho Player, Quái thường và Boss.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [Header("Thông số máu")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float invincibilityDuration = 0f;

        private int _currentHealth;
        private float _invincibilityTimer;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => _currentHealth;
        public float InvincibilityDuration => invincibilityDuration;
        public bool IsDead => _currentHealth <= 0;
        public bool IsInvincible => _invincibilityTimer > 0f;

        // Các sự kiện để UI, Audio, VFX lắng nghe mà không tạo phụ thuộc chéo
        public event Action<int, int> OnHealthChanged; // (máu hiện tại, máu tối đa)
        public event Action<int> OnDamaged;            // (lượng sát thương vừa nhận)
        public event Action OnDied;

        private void Awake()
        {
            ResetHealth();
        }

        private void Update()
        {
            if (_invincibilityTimer > 0f)
            {
                _invincibilityTimer -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Khởi tạo lại chỉ số máu từ ScriptableObject (dùng cho Quái/Boss và Object Pooling).
        /// </summary>
        public void Initialize(int newMaxHealth, float newInvincibilityDuration = -1f)
        {
            maxHealth = Mathf.Max(1, newMaxHealth);
            if (newInvincibilityDuration >= 0f)
            {
                invincibilityDuration = newInvincibilityDuration;
            }
            ResetHealth();
        }

        /// <summary>
        /// Hồi đầy máu và xóa trạng thái miễn nhiễm về ban đầu.
        /// </summary>
        public void ResetHealth()
        {
            _currentHealth = maxHealth;
            _invincibilityTimer = 0f;
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        /// <summary>
        /// Nhận sát thương nếu chưa chết và không trong thời gian miễn nhiễm.
        /// </summary>
        public void TakeDamage(int amount)
        {
            if (amount <= 0 || IsDead || IsInvincible) return;

            _currentHealth = Mathf.Max(0, _currentHealth - amount);

            if (invincibilityDuration > 0f && !IsDead)
            {
                _invincibilityTimer = invincibilityDuration;
            }

            OnDamaged?.Invoke(amount);
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);

            if (IsDead)
            {
                OnDied?.Invoke();
            }
        }

        /// <summary>
        /// Hồi máu, không vượt quá máu tối đa.
        /// </summary>
        public void Heal(int amount)
        {
            if (amount <= 0 || IsDead) return;

            _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        /// <summary>
        /// Tăng giới hạn máu tối đa (dùng cho nâng cấp chỉ số bị động).
        /// </summary>
        public void IncreaseMaxHealth(int amount, bool healAddedAmount = true)
        {
            if (amount <= 0) return;

            maxHealth += amount;
            if (healAddedAmount && !IsDead)
            {
                _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
            }
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        // Hai hàm tiện ích chỉ dùng để test nhanh trực tiếp trong Inspector ở GĐ2
        [ContextMenu("Test: Nhận 20 sát thương")]
        private void TestTakeDamage()
        {
            TakeDamage(20);
            Debug.Log($"[{gameObject.name}] Máu còn: {_currentHealth}/{maxHealth} | Miễn nhiễm: {IsInvincible}");
        }

        [ContextMenu("Test: Hồi 20 máu")]
        private void TestHeal()
        {
            Heal(20);
            Debug.Log($"[{gameObject.name}] Máu còn: {_currentHealth}/{maxHealth}");
        }
    }
}