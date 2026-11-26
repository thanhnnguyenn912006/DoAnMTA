using System.Collections.Generic;
using UnityEngine;
using SurvivorGame.Data;
using SurvivorGame.Systems;

namespace SurvivorGame.Enemies
{
    /// <summary>
    /// Quản lý việc sinh quái tự động ngoài rìa màn hình Camera theo đường cong độ khó và giới hạn số lượng tối đa.
    /// Hỗ trợ Object Pooling cho nhiều loại Prefab quái khác nhau và dời quái bị rớt lại quá xa ngoài màn hình.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Tham chiếu bắt buộc")]
        [SerializeField] private SpawnConfig spawnConfig;
        [SerializeField] private EnemyController enemyPrefab;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Camera mainCamera;

        [Header("Tối ưu hiệu năng (Object Pooling)")]
        [SerializeField] private bool useObjectPooling = true;
        [SerializeField, Min(0)] private int prewarmCount = 80;

        [Header("Chống kẹt ngoài màn hình")]
        [SerializeField, Min(12f)] private float maxEnemyDistance = 24f;
        [SerializeField, Min(0.1f)] private float relocateCheckInterval = 0.5f;

        [Header("Quái Tinh Anh (Elite - rơi Rương Báu)")]
        [SerializeField] private EnemyData eliteEnemyData;
        [SerializeField] private float[] eliteSpawnTimes = { 70f, 140f, 195f };

        [Header("Trạng thái (chỉ đọc để theo dõi trong Inspector)")]
        [SerializeField] private float elapsedTime;
        [SerializeField] private int activeEnemyCount;
        [SerializeField] private bool isSpawning = true;

        private float _spawnTimer;
        private float _relocateTimer;
        private int _lastAnnouncedPhaseIndex;
        private int _nextEliteSpawnIndex;
        private ComponentPool<EnemyController> _defaultEnemyPool;
        private readonly List<EnemyController> _activeEnemies = new List<EnemyController>(160);

        public float ElapsedTime => elapsedTime;
        public int ActiveEnemyCount => activeEnemyCount;
        public int MaxActiveEnemies => spawnConfig != null ? spawnConfig.MaxActiveEnemies : 150;
        public Transform PlayerTransform => playerTransform;

        // Sự kiện phát khi chuyển sang giai đoạn sinh quái mới (phaseIndex, phaseName)
        public event System.Action<int, string> OnPhaseChanged;
        public event System.Action<EnemyController> OnEliteSpawned;

        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (playerTransform == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                }
            }
        }

        private void Start()
        {
            if (useObjectPooling && PoolManager.Instance != null && enemyPrefab != null)
            {
                _defaultEnemyPool = PoolManager.Instance.GetOrCreatePool(enemyPrefab, prewarmCount);
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

        private void Update()
        {
            if (playerTransform == null || mainCamera == null)
            {
                return;
            }

            UpdateOutOfRangeEnemies();

            if (!isSpawning || spawnConfig == null || enemyPrefab == null)
            {
                return;
            }

            elapsedTime += Time.deltaTime;
            _spawnTimer -= Time.deltaTime;

            if (!spawnConfig.TryGetPhaseForTime(elapsedTime, out int phaseIndex, out SpawnPhase currentPhase))
            {
                return;
            }

            if (phaseIndex > _lastAnnouncedPhaseIndex)
            {
                _lastAnnouncedPhaseIndex = phaseIndex;
                OnPhaseChanged?.Invoke(phaseIndex, currentPhase.phaseName);
            }

            TrySpawnScheduledElite();

            if (_spawnTimer > 0f) return;

            _spawnTimer = currentPhase.spawnInterval;
            SpawnBatch(currentPhase);
        }

        private void TrySpawnScheduledElite()
        {
            if (eliteEnemyData == null || eliteSpawnTimes == null || _nextEliteSpawnIndex >= eliteSpawnTimes.Length)
            {
                return;
            }

            if (elapsedTime < eliteSpawnTimes[_nextEliteSpawnIndex])
            {
                return;
            }

            _nextEliteSpawnIndex++;
            float hpMult = spawnConfig.GetHpMultiplier(elapsedTime);
            float speedMult = spawnConfig.GetSpeedMultiplier(elapsedTime);
            float dmgMult = spawnConfig.GetDamageMultiplier(elapsedTime);

            Vector3 spawnPos = GetSpawnPositionOutsideCamera();
            EnemyController elite = SpawnEnemyInstance(eliteEnemyData, spawnPos, hpMult, speedMult, dmgMult);
            if (elite != null)
            {
                OnEliteSpawned?.Invoke(elite);
            }
        }

        private void UpdateOutOfRangeEnemies()
        {
            if (_activeEnemies.Count == 0) return;

            _relocateTimer -= Time.deltaTime;
            if (_relocateTimer > 0f) return;
            _relocateTimer = relocateCheckInterval;

            Vector3 playerPos = playerTransform.position;
            float maxDistSqr = maxEnemyDistance * maxEnemyDistance;

            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                EnemyController enemy = _activeEnemies[i];
                if (enemy == null || !enemy.gameObject.activeInHierarchy)
                {
                    RemoveActiveEnemyAt(i);
                    continue;
                }

                Vector3 diff = enemy.transform.position - playerPos;
                diff.z = 0f;
                if (diff.sqrMagnitude > maxDistSqr)
                {
                    RelocateEnemy(enemy, GetSpawnPositionOutsideCamera());
                }
            }

            activeEnemyCount = _activeEnemies.Count;
        }

        private static void RelocateEnemy(EnemyController enemy, Vector3 newPos)
        {
            enemy.transform.position = newPos;
            if (enemy.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                rb.position = new Vector2(newPos.x, newPos.y);
                rb.linearVelocity = Vector2.zero;
            }
        }

        private void RemoveActiveEnemyAt(int index)
        {
            int lastIndex = _activeEnemies.Count - 1;
            _activeEnemies[index] = _activeEnemies[lastIndex];
            _activeEnemies.RemoveAt(lastIndex);
        }

        private void SpawnBatch(SpawnPhase phase)
        {
            float hpMult = spawnConfig.GetHpMultiplier(elapsedTime);
            float speedMult = spawnConfig.GetSpeedMultiplier(elapsedTime);
            float dmgMult = spawnConfig.GetDamageMultiplier(elapsedTime);

            for (int i = 0; i < phase.enemiesPerSpawn; i++)
            {
                if (activeEnemyCount >= spawnConfig.MaxActiveEnemies)
                {
                    break;
                }

                EnemyData chosenData = spawnConfig.PickRandomEnemy(phase);
                if (chosenData == null) continue;

                Vector3 spawnPos = GetSpawnPositionOutsideCamera();
                SpawnSingleEnemy(chosenData, spawnPos, hpMult, speedMult, dmgMult);
            }
        }

        protected virtual void SpawnSingleEnemy(
            EnemyData data,
            Vector3 spawnPos,
            float hpMult,
            float speedMult,
            float dmgMult)
        {
            SpawnEnemyInstance(data, spawnPos, hpMult, speedMult, dmgMult);
        }

        private EnemyController SpawnEnemyInstance(
            EnemyData data,
            Vector3 spawnPos,
            float hpMult,
            float speedMult,
            float dmgMult)
        {
            EnemyController targetPrefab = ResolvePrefab(data);
            EnemyController enemy;

            if (useObjectPooling && PoolManager.Instance != null)
            {
                ComponentPool<EnemyController> pool = (targetPrefab == enemyPrefab && _defaultEnemyPool != null)
                    ? _defaultEnemyPool
                    : PoolManager.Instance.GetOrCreatePool(targetPrefab, 20);

                enemy = pool.Get(spawnPos, Quaternion.identity);
                enemy.Initialize(data, playerTransform, hpMult, speedMult, dmgMult, pool.ReleaseCallback);
            }
            else
            {
                enemy = Instantiate(targetPrefab, spawnPos, Quaternion.identity);
                enemy.Initialize(data, playerTransform, hpMult, speedMult, dmgMult, null);
            }

            if (!_activeEnemies.Contains(enemy))
            {
                _activeEnemies.Add(enemy);
            }

            activeEnemyCount = _activeEnemies.Count;
            return enemy;
        }

        private EnemyController ResolvePrefab(EnemyData data)
        {
            if (data != null && data.CustomPrefab != null &&
                data.CustomPrefab.TryGetComponent<EnemyController>(out EnemyController customCtrl))
            {
                return customCtrl;
            }

            return enemyPrefab;
        }

        /// <summary>
        /// Tính tọa độ ngẫu nhiên nằm ngay bên ngoài 1 trong 4 cạnh của Camera 2D.
        /// </summary>
        public Vector3 GetSpawnPositionOutsideCamera()
        {
            float halfHeight = mainCamera.orthographicSize;
            float halfWidth = halfHeight * mainCamera.aspect;
            float margin = spawnConfig != null ? spawnConfig.SpawnMargin : 1.5f;

            Vector3 camPos = mainCamera.transform.position;
            int edge = Random.Range(0, 4);

            float x = camPos.x;
            float y = camPos.y;

            switch (edge)
            {
                case 0:
                    x = camPos.x + Random.Range(-halfWidth - margin, halfWidth + margin);
                    y = camPos.y + halfHeight + margin;
                    break;
                case 1:
                    x = camPos.x + Random.Range(-halfWidth - margin, halfWidth + margin);
                    y = camPos.y - halfHeight - margin;
                    break;
                case 2:
                    x = camPos.x - halfWidth - margin;
                    y = camPos.y + Random.Range(-halfHeight - margin, halfHeight + margin);
                    break;
                default:
                    x = camPos.x + halfWidth + margin;
                    y = camPos.y + Random.Range(-halfHeight - margin, halfHeight + margin);
                    break;
            }

            return new Vector3(x, y, 0f);
        }

        private void HandleEnemyDied(EnemyController deadEnemy)
        {
            // Boss không do EnemySpawner sinh ra nên không được trừ vào bộ đếm giới hạn quái
            if (deadEnemy == null || deadEnemy.CompareTag("Boss")) return;

            int index = _activeEnemies.IndexOf(deadEnemy);
            if (index >= 0)
            {
                RemoveActiveEnemyAt(index);
            }

            activeEnemyCount = _activeEnemies.Count;
        }

        public void SetSpawning(bool active)
        {
            isSpawning = active;
        }

        [ContextMenu("Test: Tua nhanh +45 giây")]
        private void TestSkipTime()
        {
            elapsedTime += 45f;
#if UNITY_EDITOR
            Debug.Log($"[EnemySpawner] Đã tua thời gian lên: {elapsedTime:F0}s | HP x{spawnConfig.GetHpMultiplier(elapsedTime):F2}");
#endif
        }

        [ContextMenu("Test: Stress Test (Nhảy thẳng lên 90s - Giai đoạn cao trào)")]
        private void TestJumpToHighIntensity()
        {
            elapsedTime = 90f;
#if UNITY_EDITOR
            Debug.Log($"[EnemySpawner] Stress Test kích hoạt ở 90s | HP x{spawnConfig.GetHpMultiplier(elapsedTime):F2}");
#endif
        }

        private void OnDrawGizmosSelected()
        {
            Camera cam = mainCamera != null ? mainCamera : Camera.main;
            if (cam == null || spawnConfig == null) return;

            float halfH = cam.orthographicSize + spawnConfig.SpawnMargin;
            float halfW = cam.orthographicSize * cam.aspect + spawnConfig.SpawnMargin;

            Gizmos.color = Color.cyan;
            Vector3 center = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
            Gizmos.DrawWireCube(center, new Vector3(halfW * 2f, halfH * 2f, 0f));

            Gizmos.color = Color.yellow;
            Vector3 playerCenter = playerTransform != null ? playerTransform.position : center;
            Gizmos.DrawWireSphere(playerCenter, maxEnemyDistance);
        }
    }
}