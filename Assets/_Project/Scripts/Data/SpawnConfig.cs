using UnityEngine;

namespace SurvivorGame.Data
{
    /// <summary>
    /// Trọng số xuất hiện của một loại quái trong một giai đoạn.
    /// Trọng số càng cao thì tỷ lệ sinh ra càng lớn.
    /// </summary>
    [System.Serializable]
    public struct EnemySpawnWeight
    {
        public EnemyData enemyData;
        [Min(1)] public int weight;
    }

    /// <summary>
    /// Cấu hình một mốc thời gian (Phase) trong đường cong độ khó.
    /// </summary>
    [System.Serializable]
    public struct SpawnPhase
    {
        public string phaseName;
        [Min(0f)] public float startTime;        // Mốc thời gian bắt đầu (giây)
        [Min(0.1f)] public float spawnInterval;  // Thời gian giữa 2 lần sinh (giây)
        [Min(1)] public int enemiesPerSpawn;     // Số lượng quái sinh mỗi nhịp
        public EnemySpawnWeight[] enemyPool;     // Danh sách loại quái và trọng số
    }

    /// <summary>
    /// Asset dữ liệu điều khiển toàn bộ nhịp độ sinh quái và tăng độ khó theo thời gian.
    /// </summary>
    [CreateAssetMenu(fileName = "SpawnConfig_Default", menuName = "Survivor Game/Spawn Config")]
    public class SpawnConfig : ScriptableObject
    {
        [Header("Giới hạn & Vị trí sinh")]
        [SerializeField, Min(1)] private int maxActiveEnemies = 150;
        [SerializeField, Min(0.5f)] private float spawnMargin = 1.5f;

        [Header("Tăng chỉ số quái mỗi phút")]
        [SerializeField, Min(0f)] private float hpScalePerMinute = 0.25f;      // +25% máu/phút
        [SerializeField, Min(0f)] private float speedScalePerMinute = 0.05f;   // +5% tốc độ/phút
        [SerializeField, Min(1f)] private float maxSpeedMultiplier = 1.6f;     // Trần tốc độ tối đa để tránh quái chạy nhanh hơn Player
        [SerializeField, Min(0f)] private float damageScalePerMinute = 0.15f;  // +15% sát thương/phút

        [Header("Các giai đoạn sinh quái (sắp xếp theo Start Time tăng dần)")]
        [SerializeField] private SpawnPhase[] phases;

        public int MaxActiveEnemies => maxActiveEnemies;
        public float SpawnMargin => spawnMargin;

        public float GetHpMultiplier(float elapsedSeconds)
        {
            return 1f + (elapsedSeconds / 60f) * hpScalePerMinute;
        }

        public float GetSpeedMultiplier(float elapsedSeconds)
        {
            float raw = 1f + (elapsedSeconds / 60f) * speedScalePerMinute;
            return Mathf.Min(raw, maxSpeedMultiplier);
        }

        public float GetDamageMultiplier(float elapsedSeconds)
        {
            return 1f + (elapsedSeconds / 60f) * damageScalePerMinute;
        }

        /// <summary>
        /// Tìm giai đoạn có mốc Start Time lớn nhất nhưng nhỏ hơn hoặc bằng thời gian hiện tại.
        /// </summary>
        public bool TryGetPhaseForTime(float elapsedSeconds, out SpawnPhase activePhase)
        {
            return TryGetPhaseForTime(elapsedSeconds, out _, out activePhase);
        }

        /// <summary>
        /// Tìm giai đoạn hiện tại và trả về kèm chỉ số giai đoạn (phaseIndex).
        /// </summary>
        public bool TryGetPhaseForTime(float elapsedSeconds, out int phaseIndex, out SpawnPhase activePhase)
        {
            phaseIndex = -1;
            activePhase = default;
            if (phases == null || phases.Length == 0) return false;

            int bestIndex = 0;
            float bestStartTime = -1f;

            for (int i = 0; i < phases.Length; i++)
            {
                if (phases[i].startTime <= elapsedSeconds && phases[i].startTime >= bestStartTime)
                {
                    bestStartTime = phases[i].startTime;
                    bestIndex = i;
                }
            }

            phaseIndex = bestIndex;
            activePhase = phases[bestIndex];
            return true;
        }

        /// <summary>
        /// Chọn ngẫu nhiên một loại quái theo tỷ lệ trọng số trong giai đoạn hiện tại.
        /// </summary>
        public EnemyData PickRandomEnemy(SpawnPhase phase)
        {
            if (phase.enemyPool == null || phase.enemyPool.Length == 0) return null;

            int totalWeight = 0;
            for (int i = 0; i < phase.enemyPool.Length; i++)
            {
                if (phase.enemyPool[i].enemyData != null)
                {
                    totalWeight += Mathf.Max(1, phase.enemyPool[i].weight);
                }
            }

            if (totalWeight <= 0) return null;

            int roll = Random.Range(0, totalWeight);
            int cumulative = 0;

            for (int i = 0; i < phase.enemyPool.Length; i++)
            {
                if (phase.enemyPool[i].enemyData == null) continue;

                cumulative += Mathf.Max(1, phase.enemyPool[i].weight);
                if (roll < cumulative)
                {
                    return phase.enemyPool[i].enemyData;
                }
            }

            return phase.enemyPool[0].enemyData;
        }
    }
}