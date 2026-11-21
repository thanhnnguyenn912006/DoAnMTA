using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivorGame.Systems
{
    /// <summary>
    /// Bộ đệm tái sử dụng đối tượng (Object Pool) tổng quát cho mọi Component.
    /// Lưu sẵn ReleaseCallback để tái sử dụng không sinh rác bộ nhớ (0 GC Alloc).
    /// </summary>
    public class ComponentPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _inactiveQueue = new Queue<T>();

        /// <summary>
        /// Callback cố định dùng để trả đối tượng về Pool mà không cấp phát delegate mới.
        /// </summary>
        public Action<T> ReleaseCallback { get; }

        public int TotalCreated { get; private set; }
        public int InactiveCount => _inactiveQueue.Count;
        public int ActiveCount => TotalCreated - _inactiveQueue.Count;

        public ComponentPool(T prefab, Transform parent, int prewarmCount = 0)
        {
            _prefab = prefab;
            _parent = parent;
            ReleaseCallback = Release;

            if (prewarmCount > 0)
            {
                Prewarm(prewarmCount);
            }
        }

        /// <summary>
        /// Tạo trước một số lượng đối tượng ở trạng thái ẩn để tránh giật khung hình lúc bắt đầu giao tranh.
        /// </summary>
        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T instance = CreateInstance();
                instance.gameObject.SetActive(false);
                _inactiveQueue.Enqueue(instance);
            }
        }

        /// <summary>
        /// Lấy một đối tượng từ Pool (hoặc tạo mới nếu Pool tạm hết) và đặt vào đúng vị trí, góc xoay.
        /// </summary>
        public T Get(Vector3 position, Quaternion rotation)
        {
            T item = null;

            while (_inactiveQueue.Count > 0 && item == null)
            {
                item = _inactiveQueue.Dequeue();
            }

            if (item == null)
            {
                item = CreateInstance();
            }

            item.transform.SetPositionAndRotation(position, rotation);
            item.gameObject.SetActive(true);
            return item;
        }

        /// <summary>
        /// Tắt đối tượng và đưa trở lại hàng đợi để tái sử dụng.
        /// </summary>
        public void Release(T item)
        {
            // Chặn lỗi trả 1 đối tượng về Pool 2 lần liên tiếp
            if (item == null || !item.gameObject.activeSelf) return;

            item.gameObject.SetActive(false);
            _inactiveQueue.Enqueue(item);
        }

        private T CreateInstance()
        {
            T instance = UnityEngine.Object.Instantiate(_prefab, _parent);
            TotalCreated++;
            return instance;
        }
    }
}