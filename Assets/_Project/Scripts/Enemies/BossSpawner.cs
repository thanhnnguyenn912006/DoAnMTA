using System;
using UnityEngine;
using SurvivorGame.Data;
using SurvivorGame.Player;
using SurvivorGame.Systems;
using SurvivorGame.Weapons;

namespace SurvivorGame.Enemies
{
    /// <summary>
    /// Quản lý thời điểm triệu hồi Boss, chống bỏ rơi Boss ngoài màn hình và phát sự kiện cho thanh máu Boss.
    /// </summary>
    public class BossSpawner : MonoBehaviour
    {
        [Header("Tham chiếu bắt buộc")]
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private EnemyData bossData;
        [SerializeField] private EnemyController bossPrefab;

        [Header("Lịch xuất hiện Boss")]
        [SerializeField, Min(5f)] private float firstBossTime = 150f;

        [Header("Chế độ Vô tận (mặc định tắt vì hạ Boss đợt 1 là Chiến thắng)")]
        [SerializeField, Tooltip("Bật nếu muốn triệu hồi thêm Boss lặp lại sau khi hạ Boss đầu tiên.")]
        private bool enableEndlessRepeat = false;
        [SerializeField, Min(30f), Tooltip("Chỉ có tác dụng khi bật Enable Endless Repeat.")]
        private float repeatInterval = 120f;
        [SerializeField, Min(0f), Tooltip("Hệ số tăng máu Boss ở các đợt sau khi bật Enable Endless Repeat.")]
        private float hpScalePerEncounter = 0.45f;

        [Header("Chống bỏ rơi Boss ngoài màn hình")]
        [SerializeField, Min(14f)] private float maxBossDistance = 26f;
        [SerializeField, Min(0.1f)] private float relocateCheckInterval = 0.5f;

        [Header("Trạng thái (chỉ đọc)")]
        [SerializeField] private int bossesSpawnedCount;
        [SerializeField] private bool hasActiveBoss;

        private EnemyController _activeBoss;
        private Transform _cachedPlayerTransform;
        private float _nextBossTime;
        private float _relocateTimer;

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
            if (hasActiveBoss)
            {
                UpdateActiveBossDistance();
                return;
            }

            // Mặc định chỉ triệu hồi 1 Boss duy nhất nếu chưa bật chế độ Vô tận
            if (!enableEndlessRepeat && bossesSpawnedCount > 0)
            {
                return;
            }

            if (enemySpawner == null || bossData == null || bossPrefab == null)
            {
                return;
            }

            if (enemySpawner.ElapsedTime >= _nextBossTime)
            {
                SpawnBoss();
            }
        }

        private void UpdateActiveBossDistance()
        {
            if (_activeBoss == null || !_activeBoss.gameObject.activeInHierarchy) return;

            _relocateTimer -= Time.deltaTime;
            if (_relocateTimer > 0f) return;
            _relocateTimer = relocateCheckInterval;

            Transform playerTransform = ResolvePlayerTransform();
            if (playerTransform == null) return;

            Vector3 diff = _activeBoss.transform.position - playerTransform.position;
            diff.z = 0f;
            if (diff.sqrMagnitude <= maxBossDistance * maxBossDistance) return;

            Vector3 newPos = enemySpawner != null
                ? enemySpawner.GetSpawnPositionOutsideCamera()
                : playerTransform.position + new Vector3(10f, 0f, 0f);

            _activeBoss.transform.position = newPos;
            if (_activeBoss.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                rb.position = new Vector2(newPos.x, newPos.y);
                rb.linearVelocity = Vector2.zero;
            }
        }

        private Transform ResolvePlayerTransform()
        {
            if (_cachedPlayerTransform != null) return _cachedPlayerTransform;

            if (enemySpawner != null && enemySpawner.PlayerTransform != null)
            {
                _cachedPlayerTransform = enemySpawner.PlayerTransform;
                return _cachedPlayerTransform;
            }

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                _cachedPlayerTransform = playerObj.transform;
            }

            return _cachedPlayerTransform;
        }

        public void SpawnBoss()
        {
            if (hasActiveBoss || bossData == null || bossPrefab == null) return;

            Transform playerTransform = ResolvePlayerTransform();
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

            if (enableEndlessRepeat && enemySpawner != null)
            {
                _nextBossTime = enemySpawner.ElapsedTime + repeatInterval;
            }

            Health bossHealth = boss.GetComponent<Health>();
#if UNITY_EDITOR
            Debug.Log($"[BossSpawner] BOSS XUAT HIEN: {bossData.EnemyName} (Dot {bossesSpawnedCount}, HP x{hpMult:F2})");
#endif
            OnBossSpawned?.Invoke(boss, bossHealth);
        }

        private void HandleAnyEnemyDied(EnemyController deadEnemy)
        {
            if (!hasActiveBoss || deadEnemy == null || deadEnemy != _activeBoss) return;

            EnemyController defeated = _activeBoss;
            _activeBoss = null;
            hasActiveBoss = false;

#if UNITY_EDITOR
            Debug.Log($"[BossSpawner] DA HA GUC BOSS: {defeated.Data.EnemyName}!");
#endif
            OnBossDefeated?.Invoke(defeated);
        }

        [ContextMenu("Test: Trieu hoi Boss ngay lap tuc")]
        private void TestSpawnBossNow()
        {
            if (!Application.isPlaying) return;
            SpawnBoss();
        }

        [ContextMenu("Test: Bo vu khi Cap 4 & Trieu hoi Boss (Can bang HP)")]
        private void TestMidLateGameBossFight()
        {
            if (!Application.isPlaying) return;

            Transform playerTransform = ResolvePlayerTransform();
            if (playerTransform != null)
            {
                PlayerStats stats = playerTransform.GetComponent<PlayerStats>();
                WeaponBase[] weapons = playerTransform.GetComponentsInChildren<WeaponBase>(true);

                for (int i = 0; i < weapons.Length; i++)
                {
                    WeaponBase weapon = weapons[i];
                    if (weapon == null) continue;

                    if (!weapon.gameObject.activeSelf)
                    {
                        weapon.gameObject.SetActive(true);
                    }

                    if (stats != null)
                    {
                        stats.ApplyMultipliersToWeapon(weapon);
                    }

                    while (!weapon.IsMaxLevel && weapon.CurrentLevel < 4)
                    {
                        weapon.LevelUp();
                    }
                }
            }

            SpawnBoss();
#if UNITY_EDITOR
            Debug.Log("[BossSpawner] Da kich hoat bai test can bang Boss (toan bo vu khi Cap 4).");
#endif
        }
    }
}