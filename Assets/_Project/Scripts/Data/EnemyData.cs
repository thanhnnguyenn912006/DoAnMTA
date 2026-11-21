using UnityEngine;

namespace SurvivorGame.Data
{
    /// <summary>
    /// Dữ liệu cấu hình cho một loại quái (Thường, Nhanh, Trâu, Bắn xa, Boss).
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData_New", menuName = "Survivor Game/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Thông tin cơ bản")]
        [SerializeField] private string enemyId;
        [SerializeField] private string enemyName;

        [Header("Chỉ số chiến đấu")]
        [SerializeField, Min(1)] private int maxHealth = 30;
        [SerializeField, Min(0.1f)] private float moveSpeed = 2.5f;
        [SerializeField, Min(1)] private int contactDamage = 10;

        [Header("Phần thưởng khi tiêu diệt")]
        [SerializeField, Min(1)] private int expValue = 10;
        [SerializeField, Min(1)] private int scoreValue = 10;

        [Header("Hiển thị")]
        [SerializeField] private Sprite customSprite; // Để trống sẽ giữ nguyên hình mặc định của Prefab
        [SerializeField] private Color tintColor = Color.red;
        [SerializeField, Min(0.2f)] private float scale = 1f;

        public string EnemyId => enemyId;
        public string EnemyName => enemyName;
        public int MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public int ContactDamage => contactDamage;
        public int ExpValue => expValue;
        public int ScoreValue => scoreValue;
        public Sprite CustomSprite => customSprite;
        public Color TintColor => tintColor;
        public float Scale => scale;
    }
}