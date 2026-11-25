using System.Collections.Generic;
using UnityEngine;

namespace SurvivorGame.Systems
{
    /// <summary>
    /// Rải các chi tiết trang trí nền (bụi cỏ, hoa, đá) theo từng ô chunk quanh Camera.
    /// Dùng hàm băm tất định (deterministic) để vật thể luôn xuất hiện đúng vị trí cũ khi quay lại,
    /// đồng thời tái sử dụng toàn bộ SpriteRenderer qua Pool và không gắn Collider2D.
    /// </summary>
    public class BackgroundDecorator : MonoBehaviour
    {
        [Header("Tham chiếu")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Sprite[] decorationSprites;

        [Header("Cấu hình Chunk & Mật độ")]
        [SerializeField, Min(4f)] private float chunkSize = 8f;
        [SerializeField, Range(1, 3)] private int chunkRadiusX = 2;
        [SerializeField, Range(1, 3)] private int chunkRadiusY = 2;
        [SerializeField, Range(0, 12)] private int minPropsPerChunk = 3;
        [SerializeField, Range(1, 16)] private int maxPropsPerChunk = 6;
        [SerializeField] private int worldSeed = 2026;

        [Header("Hiển thị")]
        [SerializeField] private string sortingLayerName = "Background";
        [SerializeField] private int sortingOrder = 1;
        [SerializeField] private Color propTint = Color.white;
        [SerializeField, Range(0.5f, 1.5f)] private float minScale = 0.85f;
        [SerializeField, Range(0.5f, 1.5f)] private float maxScale = 1.15f;

        private readonly Dictionary<Vector2Int, List<SpriteRenderer>> _activeChunks =
            new Dictionary<Vector2Int, List<SpriteRenderer>>(32);
        private readonly Stack<SpriteRenderer> _rendererPool = new Stack<SpriteRenderer>(160);
        private readonly Stack<List<SpriteRenderer>> _listPool = new Stack<List<SpriteRenderer>>(32);
        private readonly List<Vector2Int> _chunksToRemove = new List<Vector2Int>(32);

        private Vector2Int _lastCenterChunk;
        private bool _hasInitialized;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void Start()
        {
            RefreshChunks(force: true);
        }

        private void LateUpdate()
        {
            RefreshChunks(force: false);
        }

        private void RefreshChunks(bool force)
        {
            if (targetCamera == null || decorationSprites == null || decorationSprites.Length == 0)
            {
                return;
            }

            Vector3 camPos = targetCamera.transform.position;
            Vector2Int centerChunk = new Vector2Int(
                Mathf.FloorToInt(camPos.x / chunkSize),
                Mathf.FloorToInt(camPos.y / chunkSize));

            if (!force && _hasInitialized && centerChunk == _lastCenterChunk)
            {
                return;
            }

            _hasInitialized = true;
            _lastCenterChunk = centerChunk;

            int minX = centerChunk.x - chunkRadiusX;
            int maxX = centerChunk.x + chunkRadiusX;
            int minY = centerChunk.y - chunkRadiusY;
            int maxY = centerChunk.y + chunkRadiusY;

            // 1. Thu hồi các chunk đã đi ra khỏi vùng quanh Camera
            _chunksToRemove.Clear();
            foreach (KeyValuePair<Vector2Int, List<SpriteRenderer>> kvp in _activeChunks)
            {
                Vector2Int coord = kvp.Key;
                if (coord.x < minX || coord.x > maxX || coord.y < minY || coord.y > maxY)
                {
                    _chunksToRemove.Add(coord);
                }
            }

            for (int i = 0; i < _chunksToRemove.Count; i++)
            {
                ReleaseChunk(_chunksToRemove[i]);
            }

            // 2. Sinh trang trí cho các chunk mới lọt vào vùng nhìn thấy
            for (int cy = minY; cy <= maxY; cy++)
            {
                for (int cx = minX; cx <= maxX; cx++)
                {
                    Vector2Int coord = new Vector2Int(cx, cy);
                    if (!_activeChunks.ContainsKey(coord))
                    {
                        PopulateChunk(coord);
                    }
                }
            }
        }

        private void PopulateChunk(Vector2Int chunkCoord)
        {
            List<SpriteRenderer> spawned = _listPool.Count > 0
                ? _listPool.Pop()
                : new List<SpriteRenderer>(maxPropsPerChunk);
            spawned.Clear();

            int safeMin = Mathf.Min(minPropsPerChunk, maxPropsPerChunk);
            int safeMax = Mathf.Max(minPropsPerChunk, maxPropsPerChunk);
            int countRange = safeMax - safeMin + 1;

            uint baseHash = ComputeHash(worldSeed, chunkCoord.x, chunkCoord.y, 0);
            int propCount = safeMin + (int)(baseHash % (uint)countRange);

            float originX = chunkCoord.x * chunkSize;
            float originY = chunkCoord.y * chunkSize;
            float padding = 0.45f;
            float usableSpan = Mathf.Max(0.5f, chunkSize - padding * 2f);

            for (int i = 0; i < propCount; i++)
            {
                uint h1 = ComputeHash(worldSeed, chunkCoord.x, chunkCoord.y, i * 4 + 1);
                uint h2 = ComputeHash(worldSeed, chunkCoord.x, chunkCoord.y, i * 4 + 2);
                uint h3 = ComputeHash(worldSeed, chunkCoord.x, chunkCoord.y, i * 4 + 3);
                uint h4 = ComputeHash(worldSeed, chunkCoord.x, chunkCoord.y, i * 4 + 4);

                Sprite sprite = decorationSprites[h1 % (uint)decorationSprites.Length];
                if (sprite == null) continue;

                float x = originX + padding + HashTo01(h2) * usableSpan;
                float y = originY + padding + HashTo01(h3) * usableSpan;
                float scale = Mathf.Lerp(minScale, maxScale, HashTo01(h4));
                bool flipX = (h1 & 1u) == 1u;

                SpriteRenderer sr = RentRenderer();
                sr.sprite = sprite;
                sr.color = propTint;
                sr.flipX = flipX;
                sr.sortingLayerName = sortingLayerName;
                sr.sortingOrder = sortingOrder;

                Transform t = sr.transform;
                t.position = new Vector3(x, y, 0f);
                t.localScale = new Vector3(scale, scale, 1f);

                sr.gameObject.SetActive(true);
                spawned.Add(sr);
            }

            _activeChunks.Add(chunkCoord, spawned);
        }

        private void ReleaseChunk(Vector2Int chunkCoord)
        {
            if (!_activeChunks.TryGetValue(chunkCoord, out List<SpriteRenderer> list))
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                SpriteRenderer sr = list[i];
                if (sr != null)
                {
                    sr.gameObject.SetActive(false);
                    _rendererPool.Push(sr);
                }
            }

            list.Clear();
            _listPool.Push(list);
            _activeChunks.Remove(chunkCoord);
        }

        private SpriteRenderer RentRenderer()
        {
            if (_rendererPool.Count > 0)
            {
                return _rendererPool.Pop();
            }

            GameObject obj = new GameObject("DecorProp", typeof(SpriteRenderer));
            obj.transform.SetParent(transform, false);
            return obj.GetComponent<SpriteRenderer>();
        }

        private static uint ComputeHash(int seed, int x, int y, int salt)
        {
            unchecked
            {
                uint h = (uint)seed * 374761393u;
                h += (uint)x * 668265263u;
                h = (h ^ (h >> 13)) * 1274126177u;
                h += (uint)y * 2246822519u;
                h = (h ^ (h >> 15)) * 3266489917u;
                h += (uint)salt * 668265263u;
                h ^= h >> 16;
                return h;
            }
        }

        private static float HashTo01(uint hash)
        {
            return (hash & 0x00FFFFFFu) / 16777215f;
        }
    }
}