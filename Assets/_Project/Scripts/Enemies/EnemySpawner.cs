using UnityEngine;
using SurvivorGame.Data;
using SurvivorGame.Systems;

namespace SurvivorGame.Enemies
{
    /// <summary>
    /// Quản lý việc sinh quái tự động ngoài rìa màn hình Camera theo đường cong độ khó và giới hạn số lượng tối đa.
    /// Hỗ trợ Object Pooling cho nhiều loại Prefab quái khác nhau thông qua EnemyData.CustomPrefab.
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

        [Header("Trạng thái (chỉ đọc để theo dõi trong Inspector)")]
        [SerializeField] private float elapsedTime;
        [SerializeField] private int activeEnemyCount;
        [SerializeField] private bool isSpawning = true;

        private float _spawnTimer;
        private ComponentPool<EnemyController> _defaultEnemyPool;

        public float ElapsedTime => elapsedTime;
        public int ActiveEnemyCount => activeEnemyCount;
        public int MaxActiveEnemies => spawnConfig != null ? spawnConfig.MaxActiveEnemies : 150;
        public Transform PlayerTransform => playerTransform;

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
            if (!isSpawning || spawnConfig == null || enemyPrefab == null || playerTransform == null || mainCamera == null)
            {
                return;
            }

            elapsedTime += Time.deltaTime;
            _spawnTimer -= Time.deltaTime;

            if (_spawnTimer > 0f) return;

            if (!spawnConfig.TryGetPhaseForTime(elapsedTime, out SpawnPhase currentPhase))
            {
                return;
            }

            _spawnTimer = currentPhase.spawnInterval;
            SpawnBatch(currentPhase);
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

            activeEnemyCount++;
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

            activeEnemyCount = Mathf.Max(0, activeEnemyCount - 1);
        }

        public void SetSpawning(bool active)
        {
            isSpawning = active;
        }

        [ContextMenu("Test: Tua nhanh +45 giây")]
        private void TestSkipTime()
        {
            elapsedTime += 45f;
            Debug.Log($"[EnemySpawner] Đã tua thời gian lên: {elapsedTime:F0}s | HP x{spawnConfig.GetHpMultiplier(elapsedTime):F2}");
        }

        [ContextMenu("Test: Stress Test (Nhảy thẳng lên 90s - Giai đoạn cao trào)")]
        private void TestJumpToHighIntensity()
        {
            elapsedTime = 90f;
            Debug.Log($"[EnemySpawner] Stress Test kích hoạt ở 90s | HP x{spawnConfig.GetHpMultiplier(elapsedTime):F2}");
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
        }
    }
}