using System;
using UnityEngine;

namespace SurvivorGame.Player
{
    /// <summary>
    /// Quản lý cấp độ, kinh nghiệm và bán kính hút ngọc EXP (Magnet) của người chơi.
    /// </summary>
    public class PlayerExperience : MonoBehaviour
    {
        [Header("Công thức kinh nghiệm")]
        [SerializeField, Min(1)] private int baseExpToLevel = 20;
        [SerializeField, Min(1)] private int expFlatIncreasePerLevel = 16;
        [SerializeField, Min(1f)] private float expMultiplierPerLevel = 1.03f;

        [Header("Cấu hình hút ngọc (Magnet)")]
        [SerializeField, Min(0.5f)] private float magnetRadius = 2.5f;
        [SerializeField, Min(0.1f)] private float collectRadius = 0.45f;

        [Header("Trạng thái (chỉ đọc để theo dõi)")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int currentExp;
        [SerializeField] private int expToNextLevel;

        public int CurrentLevel => currentLevel;
        public int CurrentExp => currentExp;
        public int ExpToNextLevel => expToNextLevel;
        public float MagnetRadius => magnetRadius;
        public float CollectRadius => collectRadius;

        // Sự kiện cho UI Thanh EXP và Hệ thống Nâng cấp lắng nghe
        public event Action<int, int, int> OnExpChanged; // (currentExp, expToNextLevel, currentLevel)
        public event Action<int> OnLevelUp;              // (newLevel)

        private void Awake()
        {
            currentLevel = 1;
            currentExp = 0;
            expToNextLevel = CalculateExpRequired(currentLevel);
        }

        private void Start()
        {
            OnExpChanged?.Invoke(currentExp, expToNextLevel, currentLevel);
        }

        /// <summary>
        /// Cộng kinh nghiệm khi nhặt ngọc EXP; tự động xử lý nếu lên nhiều cấp cùng lúc.
        /// </summary>
        public void AddExp(int amount)
        {
            if (amount <= 0) return;

            currentExp += amount;

            while (currentExp >= expToNextLevel)
            {
                currentExp -= expToNextLevel;
                currentLevel++;
                expToNextLevel = CalculateExpRequired(currentLevel);

#if UNITY_EDITOR
                Debug.Log($"[PlayerExperience] LÊN CẤP {currentLevel}! (Cần {expToNextLevel} EXP cho cấp tiếp theo)");
#endif
                OnLevelUp?.Invoke(currentLevel);
            }

            OnExpChanged?.Invoke(currentExp, expToNextLevel, currentLevel);
        }

        /// <summary>
        /// Tăng bán kính hút ngọc EXP (dùng cho nâng cấp bị động).
        /// </summary>
        public void IncreaseMagnetRadius(float bonusAmount)
        {
            if (bonusAmount <= 0f) return;
            magnetRadius += bonusAmount;
        }

        /// <summary>
        /// Tính lượng EXP cần thiết để vượt qua một cấp độ cụ thể.
        /// </summary>
        private int CalculateExpRequired(int level)
        {
            if (level <= 1) return baseExpToLevel;

            float scaled = (baseExpToLevel + (level - 1) * expFlatIncreasePerLevel)
                           * Mathf.Pow(expMultiplierPerLevel, level - 1);
            return Mathf.RoundToInt(scaled);
        }

        [ContextMenu("Test: Cộng +50 EXP")]
        private void TestAddExp()
        {
            AddExp(50);
        }

        // Vẽ vòng tròn bán kính hút ngọc trong Scene View để dễ cân chỉnh
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, magnetRadius);
        }
    }
}