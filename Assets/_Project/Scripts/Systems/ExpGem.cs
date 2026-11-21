using System;
using UnityEngine;
using SurvivorGame.Player;

namespace SurvivorGame.Systems
{
    /// <summary>
    /// Viên ngọc kinh nghiệm rơi ra khi quái chết.
    /// Sử dụng kiểm tra khoảng cách sqrMagnitude thay vì Collider để tối ưu CPU khi có hàng trăm ngọc trên sân.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ExpGem : MonoBehaviour
    {
        [Header("Hiệu ứng hút (Magnet)")]
        [SerializeField, Min(1f)] private float initialPullSpeed = 4f;
        [SerializeField, Min(1f)] private float pullAcceleration = 22f;
        [SerializeField, Min(1f)] private float maxPullSpeed = 16f;

        [Header("Màu sắc theo giá trị EXP")]
        [SerializeField] private Color smallGemColor = new Color(0.22f, 0.74f, 0.97f); // Xanh dương nhạt (< 15 EXP)
        [SerializeField] private Color mediumGemColor = new Color(0.20f, 0.83f, 0.60f); // Xanh ngọc (15 - 24 EXP)
        [SerializeField] private Color largeGemColor = new Color(0.98f, 0.80f, 0.08f);  // Vàng kim (>= 25 EXP)

        private SpriteRenderer _spriteRenderer;
        private PlayerExperience _collector;
        private Action<ExpGem> _returnToPoolCallback;

        private int _expValue;
        private bool _isMagnetized;
        private bool _isCollected;
        private float _currentPullSpeed;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Khởi tạo viên ngọc với giá trị EXP và callback trả về Object Pool.
        /// </summary>
        public void Initialize(int expValue, PlayerExperience collector, Action<ExpGem> returnToPoolCallback)
        {
            _expValue = Mathf.Max(1, expValue);
            _collector = collector;
            _returnToPoolCallback = returnToPoolCallback;

            _isMagnetized = false;
            _isCollected = false;
            _currentPullSpeed = initialPullSpeed;

            ApplyVisualByValue(_expValue);
        }

        private void Update()
        {
            if (_isCollected || _collector == null) return;

            Vector3 targetPos = _collector.transform.position;
            Vector3 diff = targetPos - transform.position;
            diff.z = 0f;
            float sqrDist = diff.sqrMagnitude;

            // Kiểm tra xem ngọc đã nằm trong tầm hút chưa
            if (!_isMagnetized)
            {
                float magnetRadius = _collector.MagnetRadius;
                if (sqrDist <= magnetRadius * magnetRadius)
                {
                    _isMagnetized = true;
                }
                else
                {
                    return;
                }
            }

            // Gia tốc bay mượt về phía nhân vật
            _currentPullSpeed = Mathf.Min(maxPullSpeed, _currentPullSpeed + pullAcceleration * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, _currentPullSpeed * Time.deltaTime);

            float collectRadius = _collector.CollectRadius;
            if ((targetPos - transform.position).sqrMagnitude <= collectRadius * collectRadius)
            {
                Collect();
            }
        }

        private void ApplyVisualByValue(int value)
        {
            if (value >= 25)
            {
                _spriteRenderer.color = largeGemColor;
                transform.localScale = Vector3.one * 0.42f;
            }
            else if (value >= 15)
            {
                _spriteRenderer.color = mediumGemColor;
                transform.localScale = Vector3.one * 0.34f;
            }
            else
            {
                _spriteRenderer.color = smallGemColor;
                transform.localScale = Vector3.one * 0.28f;
            }
        }

        private void Collect()
        {
            _isCollected = true;
            _collector.AddExp(_expValue);
            Despawn();
        }

        private void Despawn()
        {
            _collector = null;

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