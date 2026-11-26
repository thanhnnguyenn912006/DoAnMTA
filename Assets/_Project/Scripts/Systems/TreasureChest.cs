using System;
using UnityEngine;
using SurvivorGame.Player;

namespace SurvivorGame.Systems
{
    /// <summary>
    /// Rương Báu rơi ra từ Quái Tinh Anh (Elite).
    /// Dùng kiểm tra khoảng cách sqrMagnitude (0 Collider vật lý) và chỉ mở khi trận ở trạng thái Playing.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class TreasureChest : MonoBehaviour
    {
        [Header("Cấu hình nhặt & Hiệu ứng nhấp nhô")]
        [SerializeField, Min(0.3f)] private float collectRadius = 0.85f;
        [SerializeField, Min(0f)] private float bobAmplitude = 0.08f;
        [SerializeField, Min(0.5f)] private float bobFrequency = 4f;

        private PlayerExperience _collector;
        private Vector3 _basePosition;
        private float _bobTimer;
        private bool _isCollected;

        public static event Action<TreasureChest> OnAnyChestCollected;

        public void Initialize(PlayerExperience collector)
        {
            _collector = collector;
            _basePosition = transform.position;
            _bobTimer = 0f;
            _isCollected = false;
        }

        private void Start()
        {
            if (_collector == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    _collector = playerObj.GetComponent<PlayerExperience>();
                }
            }

            if (_basePosition == Vector3.zero)
            {
                _basePosition = transform.position;
            }
        }

        private void Update()
        {
            if (_isCollected || _collector == null) return;

            _bobTimer += Time.deltaTime;
            float yOffset = Mathf.Sin(_bobTimer * bobFrequency) * bobAmplitude;
            transform.position = new Vector3(_basePosition.x, _basePosition.y + yOffset, _basePosition.z);

            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            Vector3 diff = _collector.transform.position - _basePosition;
            diff.z = 0f;
            float effectiveRadius = Mathf.Max(collectRadius, _collector.CollectRadius);

            if (diff.sqrMagnitude <= effectiveRadius * effectiveRadius)
            {
                Collect();
            }
        }

        private void Collect()
        {
            _isCollected = true;
            AudioManager.Instance?.PlaySfx(SoundId.ChestOpen, 0.95f, 0.1f, 0f);
            OnAnyChestCollected?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
