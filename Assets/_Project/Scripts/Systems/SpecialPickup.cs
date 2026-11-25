using System;
using System.Collections.Generic;
using UnityEngine;
using SurvivorGame.Player;

namespace SurvivorGame.Systems
{
    public enum SpecialPickupType
    {
        HealPotion,
        MapMagnet
    }

    /// <summary>
    /// Vật phẩm rơi đặc biệt trên sân (Bình Máu hồi HP hoặc Nam Châm Lớn hút toàn bộ ngọc EXP).
    /// Dùng kiểm tra khoảng cách sqrMagnitude (0 Collider vật lý) và hỗ trợ Object Pooling.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpecialPickup : MonoBehaviour
    {
        private static readonly List<SpecialPickup> _activePickups = new List<SpecialPickup>(16);

        [Header("Hiệu ứng hút")]
        [SerializeField, Min(1f)] private float minMagnetRadius = 2.5f;
        [SerializeField, Min(1f)] private float initialPullSpeed = 5f;
        [SerializeField, Min(1f)] private float pullAcceleration = 24f;
        [SerializeField, Min(1f)] private float maxPullSpeed = 16f;
        [SerializeField, Min(1)] private int defaultHealAmount = 25;

        private SpriteRenderer _spriteRenderer;
        private PlayerExperience _collector;
        private Health _playerHealth;
        private Action<SpecialPickup> _returnToPoolCallback;

        private SpecialPickupType _pickupType;
        private int _healAmount;
        private bool _isMagnetized;
        private bool _isCollected;
        private float _currentPullSpeed;

        public static int ActiveCount => _activePickups.Count;
        public SpecialPickupType PickupType => _pickupType;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _activePickups.Clear();
        }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            if (!_activePickups.Contains(this))
            {
                _activePickups.Add(this);
            }
        }

        private void OnDisable()
        {
            int index = _activePickups.IndexOf(this);
            if (index >= 0)
            {
                int lastIndex = _activePickups.Count - 1;
                _activePickups[index] = _activePickups[lastIndex];
                _activePickups.RemoveAt(lastIndex);
            }
        }

        /// <summary>
        /// Khởi tạo loại vật phẩm, hình ảnh, lượng hồi máu và callback trả về Pool.
        /// </summary>
        public void Initialize(
            SpecialPickupType pickupType,
            Sprite sprite,
            int healAmount,
            PlayerExperience collector,
            Health playerHealth,
            Action<SpecialPickup> returnToPoolCallback)
        {
            _pickupType = pickupType;
            _healAmount = healAmount > 0 ? healAmount : defaultHealAmount;
            _collector = collector;
            _playerHealth = playerHealth != null
                ? playerHealth
                : (collector != null ? collector.GetComponent<Health>() : null);
            _returnToPoolCallback = returnToPoolCallback;

            _isMagnetized = false;
            _isCollected = false;
            _currentPullSpeed = initialPullSpeed;

            if (_spriteRenderer != null)
            {
                if (sprite != null)
                {
                    _spriteRenderer.sprite = sprite;
                    _spriteRenderer.color = Color.white;
                }
                else
                {
                    _spriteRenderer.color = pickupType == SpecialPickupType.HealPotion
                        ? new Color(0.95f, 0.25f, 0.30f, 1f)
                        : new Color(0.30f, 0.72f, 1f, 1f);
                }
            }
        }

        private void Update()
        {
            if (_isCollected || _collector == null) return;

            Vector3 targetPos = _collector.transform.position;
            Vector3 diff = targetPos - transform.position;
            diff.z = 0f;
            float sqrDist = diff.sqrMagnitude;

            if (!_isMagnetized)
            {
                float pullRadius = Mathf.Max(minMagnetRadius, _collector.MagnetRadius);
                if (sqrDist <= pullRadius * pullRadius)
                {
                    _isMagnetized = true;
                }
                else
                {
                    return;
                }
            }

            _currentPullSpeed = Mathf.Min(maxPullSpeed, _currentPullSpeed + pullAcceleration * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, _currentPullSpeed * Time.deltaTime);

            float collectRadius = _collector.CollectRadius;
            if ((targetPos - transform.position).sqrMagnitude <= collectRadius * collectRadius)
            {
                Collect();
            }
        }

        private void Collect()
        {
            _isCollected = true;

            switch (_pickupType)
            {
                case SpecialPickupType.HealPotion:
                    _playerHealth?.Heal(_healAmount);
                    AudioManager.Instance?.PlaySfx(SoundId.PickupHeal, 0.9f, 0.05f, 0f);
                    break;

                case SpecialPickupType.MapMagnet:
                    ExpGem.MagnetizeAllActive(_collector);
                    AudioManager.Instance?.PlaySfx(SoundId.PickupMagnet, 0.95f, 0.05f, 0f);
                    break;
            }

            Despawn();
        }

        private void Despawn()
        {
            _collector = null;
            _playerHealth = null;

            if (_returnToPoolCallback != null)
            {
                _returnToPoolCallback.Invoke(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
