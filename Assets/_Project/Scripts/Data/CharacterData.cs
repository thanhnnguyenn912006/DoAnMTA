using UnityEngine;

namespace SurvivorGame.Data
{
    /// <summary>
    /// Dữ liệu cấu hình cho một nhân vật có thể chọn ở MainMenu (ngoại hình, chỉ số nền và vũ khí khởi đầu).
    /// </summary>
    [CreateAssetMenu(fileName = "Character_New", menuName = "Survivor Game/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("Thông tin nhân vật")]
        [SerializeField] private string characterId;
        [SerializeField] private string characterName;
        [SerializeField] private string startingWeaponLabel;
        [SerializeField] private string statSummary;

        [Header("Ngoại hình & Hoạt ảnh (2 frame 16x16)")]
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite[] walkFrames;
        [SerializeField] private Color accentColor = new Color(0.98f, 0.80f, 0.08f, 1f);

        [Header("Chỉ số nền & Vũ khí khởi đầu")]
        [SerializeField] private WeaponType startingWeaponType = WeaponType.AutoTarget;
        [SerializeField, Min(1)] private int maxHealth = 100;
        [SerializeField, Min(1f)] private float moveSpeed = 5f;
        [SerializeField, Min(0.5f)] private float baseDamageMultiplier = 1f;

        public string CharacterId => characterId;
        public string CharacterName => characterName;
        public string StartingWeaponLabel => startingWeaponLabel;
        public string StatSummary => statSummary;
        public Sprite IdleSprite => idleSprite;
        public Sprite[] WalkFrames => walkFrames;
        public Color AccentColor => accentColor;
        public WeaponType StartingWeaponType => startingWeaponType;
        public int MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public float BaseDamageMultiplier => baseDamageMultiplier;
    }
}
