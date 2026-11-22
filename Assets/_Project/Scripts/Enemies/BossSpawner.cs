using System;
using UnityEngine;
using SurvivorGame.Data;
using SurvivorGame.Systems;

namespace SurvivorGame.Enemies
{
    /// <summary>
    /// Quản lý thời điểm triệu hồi Boss, tăng sức mạnh Boss ở các đợt sau và phát sự kiện cho thanh máu Boss.
    /// </summary>
    public class BossSpawner : MonoBehaviour
    {
        [Header("Tham chiếu bắt buộc")]
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private EnemyData bossData;
        [SerializeField] private EnemyController bossPrefab;

        [Header("Lịch xuất hiện Boss")]
        [SerializeField, Min(5f)] private float firstBossTime = 90f;
        [SerializeField, Min(30f)] private float repeatInterval = 120f;
        [SerializeField, Min(0f)] private float hpScalePerEncounter = 0.45f;

        [Header("Trạng thái (chỉ đọc)")]
        [SerializeField] private int bossesSpawnedCount;
        [SerializeField] private bool hasActiveBoss;

        private EnemyController _activeBoss;
        private float _nextBossTime;

        public bool HasActiveBoss => hasActiveBoss;
        public EnemyController ActiveBoss => _activeBoss;

        public event Action<EnemyController, Health> OnBossSpawned;
        public event Action<EnemyController> OnBossDefeated;

        private void Awake()
        {
            if (enemySpawner == null)
            {
                enemySpawner = FindAnyObjectByType<EnemySpawner>();
            }

            _nextBossTime = firstBossTime;
        }

        private void OnEnable()
        {
            EnemyController.OnAnyEnemyDied += HandleAnyEnemyDied;
        }

        private void OnDisable()
        {
            EnemyController.OnAnyEnemyDied -= HandleAnyEnemyDied;
        }

        private void Update()
        {
            if (hasActiveBoss || enemySpawner == null || bossData == null || bossPrefab == null)
            {
                return;
            }

            if (enemySpawner.ElapsedTime >= _nextBossTime)
            {
                SpawnBoss();
            }
        }

        public void SpawnBoss()
        {
            if (hasActiveBoss || bossData == null || bossPrefab == null) return;

            Transform playerTransform = enemySpawner != null ? enemySpawner.PlayerTransform : null;
            if (playerTransform == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) playerTransform = playerObj.transform;
            }

            if (playerTransform == null) return;

            Vector3 spawnPos = enemySpawner != null
                ? enemySpawner.GetSpawnPositionOutsideCamera()
                : playerTransform.position + new Vector3(8f, 0f, 0f);

            float hpMult = 1f + bossesSpawnedCount * hpScalePerEncounter;
            float dmgMult = 1f + bossesSpawnedCount * 0.2f;

            EnemyController boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
            boss.Initialize(bossData, playerTransform, hpMult, 1f, dmgMult, null);

            _activeBoss = boss;
            hasActiveBoss = true;
            bossesSpawnedCount++;

            if (enemySpawner != null)
            {
                _nextBossTime = enemySpawner.ElapsedTime + repeatInterval;
            }

            Health bossHealth = boss.GetComponent<Health>();
            Debug.Log($"[BossSpawner] BOSS XUAT HIEN: {bossData.EnemyName} (Dot {bossesSpawnedCount}, HP x{hpMult:F2})");
            OnBossSpawned?.Invoke(boss, bossHealth);
        }

        private void HandleAnyEnemyDied(EnemyController deadEnemy)
        {
            if (!hasActiveBoss || deadEnemy == null || deadEnemy != _activeBoss) return;

            EnemyController defeated = _activeBoss;
            _activeBoss = null;
            hasActiveBoss = false;

            Debug.Log($"[BossSpawner] DA HA GUC BOSS: {defeated.Data.EnemyName}!");
            OnBossDefeated?.Invoke(defeated);
        }

        [ContextMenu("Test: Trieu hoi Boss ngay lap tuc")]
        private void TestSpawnBossNow()
        {
            SpawnBoss();
        }
    }
}