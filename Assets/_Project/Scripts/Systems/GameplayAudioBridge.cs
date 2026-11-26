using UnityEngine;
using SurvivorGame.Enemies;
using SurvivorGame.Player;

namespace SurvivorGame.Systems
{
    /// <summary>
    /// Lắng nghe toàn bộ sự kiện trong Scene Gameplay (trúng đòn, hạ quái, lên cấp, Boss, Thắng/Thua)
    /// và chuyển tiếp sang AudioManager để phát SFX tương ứng.
    /// </summary>
    public class GameplayAudioBridge : MonoBehaviour
    {
        [Header("Tham chiếu hệ thống trong Scene (tự tìm nếu để trống)")]
        [SerializeField] private PlayerExperience playerExperience;
        [SerializeField] private UpgradeManager upgradeManager;
        [SerializeField] private BossSpawner bossSpawner;
        [SerializeField] private GameManager gameManager;

        private void Awake()
        {
            if (playerExperience == null) playerExperience = FindAnyObjectByType<PlayerExperience>();
            if (upgradeManager == null) upgradeManager = FindAnyObjectByType<UpgradeManager>();
            if (bossSpawner == null) bossSpawner = FindAnyObjectByType<BossSpawner>();
            if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
        }

        private void OnEnable()
        {
            Health.OnAnyDamaged += HandleAnyDamaged;
            EnemyController.OnAnyEnemyDied += HandleEnemyDied;

            if (playerExperience != null) playerExperience.OnLevelUp += HandleLevelUp;
            if (upgradeManager != null) upgradeManager.OnUpgradeSelected += HandleUpgradeSelected;

            if (bossSpawner != null)
            {
                bossSpawner.OnBossSpawned += HandleBossSpawned;
                bossSpawner.OnBossDefeated += HandleBossDefeated;
            }

            if (gameManager != null) gameManager.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            Health.OnAnyDamaged -= HandleAnyDamaged;
            EnemyController.OnAnyEnemyDied -= HandleEnemyDied;

            if (playerExperience != null) playerExperience.OnLevelUp -= HandleLevelUp;
            if (upgradeManager != null) upgradeManager.OnUpgradeSelected -= HandleUpgradeSelected;

            if (bossSpawner != null)
            {
                bossSpawner.OnBossSpawned -= HandleBossSpawned;
                bossSpawner.OnBossDefeated -= HandleBossDefeated;
            }

            if (gameManager != null) gameManager.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void HandleAnyDamaged(Health targetHealth, int damageAmount)
        {
            if (AudioManager.Instance == null || targetHealth == null) return;

            if (targetHealth.CompareTag("Player"))
            {
                AudioManager.Instance.PlaySfx(SoundId.PlayerHit, 0.95f, 0.12f, 0.05f);
            }
            else
            {
                AudioManager.Instance.PlaySfx(SoundId.EnemyHit, 0.55f, 0.045f, 0.10f);
            }
        }

        private void HandleEnemyDied(EnemyController deadEnemy)
        {
            if (AudioManager.Instance == null) return;
            AudioManager.Instance.PlaySfx(SoundId.EnemyDeath, 0.65f, 0.06f, 0.12f);
        }

        private void HandleLevelUp(int newLevel)
        {
            AudioManager.Instance?.PlaySfx(SoundId.LevelUp, 0.9f, 0.15f);
        }

        private void HandleUpgradeSelected(UpgradeOption option)
        {
            if (option.IsWeaponEvolution)
            {
                AudioManager.Instance?.PlaySfx(SoundId.WeaponEvolve, 0.95f, 0.05f);
            }
            else
            {
                AudioManager.Instance?.PlaySfx(SoundId.UpgradeSelect, 0.85f, 0.05f);
            }
        }

        private void HandleBossSpawned(EnemyController boss, Health bossHealth)
        {
            AudioManager.Instance?.PlaySfx(SoundId.BossSpawn, 1.0f, 0.5f);
        }

        private void HandleBossDefeated(EnemyController boss)
        {
            AudioManager.Instance?.PlaySfx(SoundId.BossDefeat, 1.0f, 0.3f);
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (AudioManager.Instance == null) return;

            if (state == GameState.Victory)
            {
                AudioManager.Instance.PlaySfx(SoundId.Victory, 0.95f);
            }
            else if (state == GameState.GameOver)
            {
                AudioManager.Instance.PlaySfx(SoundId.GameOver, 0.95f);
            }
        }
    }
}