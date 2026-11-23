using System;
using UnityEngine;

namespace SurvivorGame.Systems
{
    /// <summary>
    /// Quản lý máu, thời gian bất tử sau khi trúng đòn (i-frames) và các sự kiện sát thương/tử vong
    /// dùng chung cho cả Player, Quái và Boss.
    /// </summary>
    public class Health : MonoBehaviour
    {
        public static event Action<Health, int> OnAnyDamaged;

        [Header("Cấu hình máu")]
        [SerializeField, Min(1)] private int maxHealth = 100;
        [SerializeField, Min(0f)] private float invincibilityDuration = 0f;

        private int _currentHealth;
        private float _invincibilityTimer;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => maxHealth;
        public float HealthNormalized => maxHealth > 0 ? (float)_currentHealth / maxHealth : 0f;
        public bool IsDead => _currentHealth <= 0;
        public bool IsInvincible => _invincibilityTimer > 0f;

        public event Action<int> OnDamaged;
        public event Action<int, int> OnHealthChanged; // (currentHealth, maxHealth)
        public event Action OnDied;

        private void Awake()
        {
            _currentHealth = maxHealth;
            _invincibilityTimer = 0f;
        }

        private void Update()
        {
            if (_invincibilityTimer > 0f)
            {
                _invincibilityTimer = Mathf.Max(0f, _invincibilityTimer - Time.deltaTime);
            }
        }

        public void Initialize(int newMaxHealth, bool resetCurrentHealth = true)
        {
            SetMaxHealth(newMaxHealth, resetCurrentHealth);
        }

        public void Initialize(int newMaxHealth, float newInvincibilityDuration)
        {
            invincibilityDuration = Mathf.Max(0f, newInvincibilityDuration);
            SetMaxHealth(newMaxHealth, true);
        }

        public void SetMaxHealth(int newMaxHealth, bool resetCurrentHealth = true)
        {
            maxHealth = Mathf.Max(1, newMaxHealth);
            _currentHealth = resetCurrentHealth ? maxHealth : Mathf.Min(_currentHealth, maxHealth);
            _invincibilityTimer = 0f;
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        public void IncreaseMaxHealth(int bonusAmount, bool healBonus = true)
        {
            if (bonusAmount <= 0) return;

            maxHealth += bonusAmount;
            if (healBonus && !IsDead)
            {
                _currentHealth = Mathf.Min(maxHealth, _currentHealth + bonusAmount);
            }
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        public void ResetHealth()
        {
            _currentHealth = maxHealth;
            _invincibilityTimer = 0f;
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || IsDead || IsInvincible) return;

            _currentHealth = Mathf.Max(0, _currentHealth - amount);

            if (invincibilityDuration > 0f && !IsDead)
            {
                _invincibilityTimer = invincibilityDuration;
            }

            OnDamaged?.Invoke(amount);
            OnAnyDamaged?.Invoke(this, amount);
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);

            if (IsDead)
            {
                OnDied?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || IsDead) return;

            _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }
    }
}