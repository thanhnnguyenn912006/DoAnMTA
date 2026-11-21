using System.Collections.Generic;
using UnityEngine;

namespace SurvivorGame.Systems
{
    /// <summary>
    /// Quản lý tập trung toàn bộ Object Pool trong Scene Gameplay.
    /// Tự động gom đối tượng theo từng nhóm con trong Hierarchy để dễ quan sát và gỡ lỗi.
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        private readonly Dictionary<Component, object> _pools = new Dictionary<Component, object>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Lấy Pool đã tồn tại theo Prefab, hoặc tạo mới (kèm Prewarm) nếu chưa có.
        /// </summary>
        public ComponentPool<T> GetOrCreatePool<T>(T prefab, int prewarmCount = 0) where T : Component
        {
            if (prefab == null) return null;

            if (_pools.TryGetValue(prefab, out object existingPool))
            {
                return (ComponentPool<T>)existingPool;
            }

            // Tạo một GameObject con làm thư mục chứa cho loại đối tượng này
            GameObject container = new GameObject($"Pool_{prefab.name}");
            container.transform.SetParent(transform, false);

            var newPool = new ComponentPool<T>(prefab, container.transform, prewarmCount);
            _pools.Add(prefab, newPool);
            return newPool;
        }
    }
}