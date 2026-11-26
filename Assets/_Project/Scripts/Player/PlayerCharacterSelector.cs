using UnityEngine;
using SurvivorGame.Data;
using SurvivorGame.Systems;
using SurvivorGame.Weapons;

namespace SurvivorGame.Player
{
    /// <summary>
    /// Đọc nhân vật người chơi đã chọn từ MainMenu (PlayerPrefs) và áp dụng ngoại hình,
    /// chỉ số nền cùng vũ khí khởi đầu duy nhất ngay khi trận đấu khởi tạo.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerCharacterSelector : MonoBehaviour
    {
        public const string SelectedCharacterKey = "Survivor_SelectedCharacterIndex";

        [Header("Danh sách nhân vật (0 = Thợ Săn, 1 = Pháp Sư, 2 = Hiệp Sĩ)")]
        [SerializeField] private CharacterData[] availableCharacters;
        [SerializeField] private Transform weaponsContainer;

        public CharacterData ActiveCharacter { get; private set; }

        private void Awake()
        {
            if (availableCharacters == null || availableCharacters.Length == 0) return;

            int selectedIndex = Mathf.Clamp(
                PlayerPrefs.GetInt(SelectedCharacterKey, 0),
                0,
                availableCharacters.Length - 1);

            ApplyCharacter(availableCharacters[selectedIndex]);
        }

        public void ApplyCharacter(CharacterData character)
        {
            if (character == null) return;

            ActiveCharacter = character;

            if (TryGetComponent(out SpriteRenderer sr) && character.IdleSprite != null)
            {
                sr.sprite = character.IdleSprite;
            }

            if (TryGetComponent(out SpriteFrameAnimator animator) &&
                character.WalkFrames != null &&
                character.WalkFrames.Length > 0)
            {
                animator.SetFrames(character.WalkFrames, 0.25f);
            }

            if (TryGetComponent(out Health health))
            {
                health.SetMaxHealth(character.MaxHealth, true);
            }

            if (TryGetComponent(out PlayerMovement movement))
            {
                movement.SetBaseMoveSpeed(character.MoveSpeed);
            }

            Transform container = weaponsContainer != null ? weaponsContainer : transform;
            WeaponBase[] weapons = container.GetComponentsInChildren<WeaponBase>(true);
            for (int i = 0; i < weapons.Length; i++)
            {
                WeaponBase weapon = weapons[i];
                if (weapon == null || weapon.Data == null) continue;

                bool isStarter = weapon.Data.Type == character.StartingWeaponType;
                weapon.gameObject.SetActive(isStarter);
            }

            if (TryGetComponent(out PlayerStats stats))
            {
                stats.SetBaseDamageMultiplier(character.BaseDamageMultiplier);
            }
        }
    }
}
