using System;
using UnityEngine;
using SurvivorGame.Data;
using SurvivorGame.Systems;

namespace SurvivorGame.Enemies
{
    /// <summary>
    /// Điều phối các thành phần trên một con quái từ ScriptableObject EnemyData.
    /// Xử lý khởi tạo (hỗ trợ Object Pooling), sự kiện chết và thu hồi đối tượng.
    /// </summary>
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(EnemyChaseMovement))]
    [RequireComponent(typeof(ContactDamage))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Dữ liệu mặc định (dùng khi kéo trực tiếp vào Scene để test)")]
        [SerializeField] private EnemyData defaultEnemyData;

        private Health _health;
        private EnemyChaseMovement _movement;
        private ContactDamage _contactDamage;
        private DamageFlash _damageFlash;
        private SpriteRenderer _spriteRenderer;
        private Collider2D _collider;
        private Sprite _defaultSprite;

        private EnemyData _currentData;
        private bool _isInitialized;
        private Action<EnemyController> _returnToPoolCallback;

        public EnemyData Data => _currentData;
        public Health Health => _health;

        // Sự kiện toàn cục khi có quái bị tiêu diệt (cho hệ thống EXP, Điểm, Âm thanh, VFX lắng nghe)
        public static event Action<EnemyController> OnAnyEnemyDied;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _movement = GetComponent<EnemyChaseMovement>();
            _contactDamage = GetComponent<ContactDamage>();
            TryGetComponent(out _damageFlash);
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();

            // Lưu lại sprite gốc của Prefab để khôi phục khi tái sử dụng từ Pool
            _defaultSprite = _spriteRenderer.sprite;
        }

        private void OnEnable()
        {
            _health.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            _health.OnDied -= HandleDied;
        }

        private void Start()
        {
            // Trường hợp đặt sẵn quái trong Scene để test (chưa qua Spawner)
            if (!_isInitialized && defaultEnemyData != null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                Transform playerTransform = playerObj != null ? playerObj.transform : null;
                Initialize(defaultEnemyData, playerTransform);
            }
        }

        /// <summary>
        /// Khởi tạo toàn bộ chỉ số và ngoại hình quái từ EnemyData.
        /// Hỗ trợ nhân hệ số độ khó (GĐ5) và tái sử dụng từ Object Pool (GĐ6).
        /// </summary>
        public void Initialize(
            EnemyData data,
            Transform target,
            float hpMultiplier = 1f,
            float speedMultiplier = 1f,
            float damageMultiplier = 1f,
            Action<EnemyController> returnToPoolCallback = null)
        {
            if (data == null) return;

            _currentData = data;
            _returnToPoolCallback = returnToPoolCallback;
            _isInitialized = true;

            // 1. Áp dụng ngoại hình (kích thước, hình ảnh, màu sắc)
            transform.localScale = Vector3.one * data.Scale;
            _spriteRenderer.sprite = data.CustomSprite != null ? data.CustomSprite : _defaultSprite;

            if (_damageFlash != null)
            {
                _damageFlash.SetBaseColor(data.TintColor);
            }
            else
            {
                _spriteRenderer.color = data.TintColor;
            }

            // 2. Áp dụng chỉ số chiến đấu (có tính hệ số độ khó theo thời gian)
            int finalHp = Mathf.Max(1, Mathf.RoundToInt(data.MaxHealth * hpMultiplier));
            float finalSpeed = Mathf.Max(0.2f, data.MoveSpeed * speedMultiplier);
            int finalDamage = Mathf.Max(1, Mathf.RoundToInt(data.ContactDamage * damageMultiplier));

            _health.Initialize(finalHp, 0f);
            _contactDamage.SetDamage(finalDamage);
            _movement.SetMoveSpeed(finalSpeed);
            _movement.SetTarget(target);
            _movement.SetCanMove(true);

            // 3. Bật lại va chạm (trường hợp lấy lại từ Object Pool)
            _collider.enabled = true;
        }

        private void HandleDied()
        {
            _movement.StopMovement();
            _collider.enabled = false;

            OnAnyEnemyDied?.Invoke(this);
            Despawn();
        }

        /// <summary>
        /// Thu hồi quái về Object Pool hoặc hủy nếu chưa dùng Pool.
        /// </summary>
        public void Despawn()
        {
            _isInitialized = false;
            _movement.StopMovement();

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