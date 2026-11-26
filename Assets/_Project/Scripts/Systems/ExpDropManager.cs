using UnityEngine;
using SurvivorGame.Enemies;
using SurvivorGame.Player;

namespace SurvivorGame.Systems
{
    /// <summary>
    /// Lắng nghe sự kiện quái bị tiêu diệt và thả Ngọc EXP cùng Vật phẩm đặc biệt (Bình Máu / Nam Châm Lớn) từ Object Pool.
    /// </summary>
    public class ExpDropManager : MonoBehaviour
    {
        [Header("Tham chiếu bắt buộc")]
        [SerializeField] private ExpGem expGemPrefab;
        [SerializeField] private PlayerExperience playerExperience;

        [Header("Cấu hình Pool & Vị trí rơi")]
        [SerializeField, Min(0)] private int prewarmCount = 80;
        [SerializeField, Min(0f)] private float scatterRadius = 0.2f;

        [Header("Vật phẩm rơi đặc biệt (Bình Máu & Nam Châm Lớn)")]
        [SerializeField] private SpecialPickup specialPickupPrefab;
        [SerializeField] private Sprite healPotionSprite;
        [SerializeField] private Sprite mapMagnetSprite;
        [SerializeField, Min(0)] private int pickupPrewarmCount = 6;
        [SerializeField, Min(1)] private int maxActivePickups = 5;
        [SerializeField, Range(0f, 1f)] private float mapMagnetDropChance = 0.008f;
        [SerializeField, Range(0f, 1f)] private float healDropChance = 0.018f;
        [SerializeField, Min(1)] private int healAmount = 25;
        [SerializeField, Min(0f)] private float pickupScatterRadius = 0.35f;

        [Header("Rương Báu từ Quái Tinh Anh (Elite)")]
        [SerializeField] private TreasureChest treasureChestPrefab;

        private Health _playerHealth;
        private ComponentPool<ExpGem> _gemPool;
        private ComponentPool<SpecialPickup> _pickupPool;

        private void Awake()
        {
            if (playerExperience == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerExperience = playerObj.GetComponent<PlayerExperience>();
                }
            }

            if (playerExperience != null)
            {
                _playerHealth = playerExperience.GetComponent<Health>();
            }
        }

        private void Start()
        {
            if (PoolManager.Instance != null)
            {
                if (expGemPrefab != null)
                {
                    _gemPool = PoolManager.Instance.GetOrCreatePool(expGemPrefab, prewarmCount);
                }

                if (specialPickupPrefab != null)
                {
                    _pickupPool = PoolManager.Instance.GetOrCreatePool(specialPickupPrefab, pickupPrewarmCount);
                }
            }
        }

        private void OnEnable()
        {
            EnemyController.OnAnyEnemyDied += HandleEnemyDied;
        }

        private void OnDisable()
        {
            EnemyController.OnAnyEnemyDied -= HandleEnemyDied;
        }

        private void HandleEnemyDied(EnemyController deadEnemy)
        {
            if (deadEnemy == null || deadEnemy.Data == null || expGemPrefab == null || playerExperience == null)
            {
                return;
            }

            Vector2 offset = Random.insideUnitCircle * scatterRadius;
            Vector3 dropPos = deadEnemy.transform.position + new Vector3(offset.x, offset.y, 0f);
            int expAmount = deadEnemy.Data.ExpValue;

            if (_gemPool != null)
            {
                ExpGem gem = _gemPool.Get(dropPos, Quaternion.identity);
                gem.Initialize(expAmount, playerExperience, _gemPool.ReleaseCallback);
            }
            else
            {
                ExpGem gem = Instantiate(expGemPrefab, dropPos, Quaternion.identity);
                gem.Initialize(expAmount, playerExperience, null);
            }

            if (deadEnemy.IsElite && treasureChestPrefab != null)
            {
                TreasureChest chest = Instantiate(treasureChestPrefab, deadEnemy.transform.position, Quaternion.identity);
                chest.Initialize(playerExperience);
                return;
            }

            TryDropSpecialPickup(deadEnemy);
        }

        private void TryDropSpecialPickup(EnemyController deadEnemy)
        {
            if (specialPickupPrefab == null || deadEnemy.CompareTag("Boss")) return;
            if (SpecialPickup.ActiveCount >= maxActivePickups) return;

            float chanceMultiplier = deadEnemy.Data.ExpValue >= 20 ? 1.5f : 1f;
            float magnetThreshold = mapMagnetDropChance * chanceMultiplier;
            float healThreshold = magnetThreshold + healDropChance * chanceMultiplier;
            float roll = Random.value;

            if (roll < magnetThreshold)
            {
                SpawnSpecialPickup(SpecialPickupType.MapMagnet, deadEnemy.transform.position);
            }
            else if (roll < healThreshold)
            {
                SpawnSpecialPickup(SpecialPickupType.HealPotion, deadEnemy.transform.position);
            }
        }

        private void SpawnSpecialPickup(SpecialPickupType pickupType, Vector3 originPos)
        {
            Vector2 offset = Random.insideUnitCircle * pickupScatterRadius;
            Vector3 dropPos = originPos + new Vector3(offset.x, offset.y, 0f);
            Sprite sprite = pickupType == SpecialPickupType.HealPotion ? healPotionSprite : mapMagnetSprite;

            if (_pickupPool != null)
            {
                SpecialPickup pickup = _pickupPool.Get(dropPos, Quaternion.identity);
                pickup.Initialize(pickupType, sprite, healAmount, playerExperience, _playerHealth, _pickupPool.ReleaseCallback);
            }
            else
            {
                SpecialPickup pickup = Instantiate(specialPickupPrefab, dropPos, Quaternion.identity);
                pickup.Initialize(pickupType, sprite, healAmount, playerExperience, _playerHealth, null);
            }
        }

        [ContextMenu("Test: Tha Binh Mau gan Player")]
        private void TestSpawnHealPotion()
        {
            if (!Application.isPlaying || specialPickupPrefab == null || playerExperience == null) return;
            SpawnSpecialPickup(SpecialPickupType.HealPotion, playerExperience.transform.position + new Vector3(2.2f, 0f, 0f));
        }

        [ContextMenu("Test: Tha Nam Cham Lon gan Player")]
        private void TestSpawnMapMagnet()
        {
            if (!Application.isPlaying || specialPickupPrefab == null || playerExperience == null) return;
            SpawnSpecialPickup(SpecialPickupType.MapMagnet, playerExperience.transform.position + new Vector3(-2.2f, 0f, 0f));
        }
    }
}
