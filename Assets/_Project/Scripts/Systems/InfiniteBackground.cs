using UnityEngine;

namespace SurvivorGame.Systems
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class InfiniteBackground : MonoBehaviour
    {
        [Header("Tham chiếu")]
        [SerializeField] private Camera targetCamera;

        [Header("Cấu hình")]
        [SerializeField, Min(0)] private int extraTilesEachSide = 2;

        private SpriteRenderer _renderer;
        private Vector2 _tileSize;
        private Vector2 _lastSize;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            _renderer.drawMode = SpriteDrawMode.Tiled;
            if (_renderer.sprite != null)
            {
                _tileSize = _renderer.sprite.bounds.size;
            }
        }

        private void LateUpdate()
        {
            if (targetCamera == null || _renderer.sprite == null) return;

            if (_tileSize.x <= 0f || _tileSize.y <= 0f)
            {
                _tileSize = _renderer.sprite.bounds.size;
            }

            ResizeIfNeeded();

            Vector3 camPos = targetCamera.transform.position;
            float snappedX = Mathf.Round(camPos.x / _tileSize.x) * _tileSize.x;
            float snappedY = Mathf.Round(camPos.y / _tileSize.y) * _tileSize.y;
            transform.position = new Vector3(snappedX, snappedY, transform.position.z);
        }

        private void ResizeIfNeeded()
        {
            float height = targetCamera.orthographicSize * 2f;
            float width = height * targetCamera.aspect;

            int tilesX = Mathf.CeilToInt(width / _tileSize.x) + extraTilesEachSide * 2;
            int tilesY = Mathf.CeilToInt(height / _tileSize.y) + extraTilesEachSide * 2;
            Vector2 newSize = new Vector2(tilesX * _tileSize.x, tilesY * _tileSize.y);

            if (newSize != _lastSize)
            {
                _renderer.size = newSize;
                _lastSize = newSize;
            }
        }
    }
}