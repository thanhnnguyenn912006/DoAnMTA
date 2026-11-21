using UnityEngine;
using SurvivorGame.Enemies;
using SurvivorGame.Player;

namespace SurvivorGame.Systems
{
    /// <summary>
    /// Lắng nghe sự kiện quái bị tiêu diệt và thả Ngọc EXP từ Object Pool tại vị trí quái chết.
    /// </summary>
    public class ExpDropManager : MonoBehaviour
    {
        [Header("Tham chiếu bắt buộc")]
        [SerializeField] private ExpGem expGemPrefab;
        [SerializeField] private PlayerExperience playerExperience;

        [Header("Cấu hình Pool & Vị trí rơi")]
        [SerializeField, Min(0)] private int prewarmCount = 80;
        [SerializeField, Min(0f)] private float scatterRadius = 0.2f;

        private ComponentPool<ExpGem> _gemPool;

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
        }

        private void Start()
        {
            if (PoolManager.Instance != null && expGemPrefab != null)
            {
                _gemPool = PoolManager.Instance.GetOrCreatePool(expGemPrefab, prewarmCount);
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
        }
    }
}