using UnityEngine;
using SurvivorGame.Systems;

namespace SurvivorGame.Enemies
{
    /// <summary>
    /// Điều khiển hành vi chiến đấu của Boss bằng máy trạng thái (FSM):
    /// Đuổi theo, bắn đạn tỏa tròn 360 độ (dùng Object Pool) và gồng báo hiệu rồi lao tới (Dash).
    /// Tự tăng cường sức mạnh (Pha 2) khi máu dưới 50%.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(EnemyChaseMovement))]
    [RequireComponent(typeof(EnemyController))]
    [RequireComponent(typeof(Health))]
    public class BossAttackController : MonoBehaviour
    {
        private enum BossState
        {
            Chasing,
            WindupDash,
            Dashing,
            Recovering
        }

        [Header("Chiêu 1: Bắn tỏa tròn (Radial Burst)")]
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField, Min(0)] private int projectilePrewarmCount = 40;
        [SerializeField, Min(4)] private int burstProjectileCount = 10;
        [SerializeField, Min(0.5f)] private float burstCooldown = 3.6f;
        [SerializeField, Min(1f)] private float projectileSpeed = 5.8f;
        [SerializeField, Min(1)] private int projectileDamage = 14;
        [SerializeField, Min(0.5f)] private float projectileLifetime = 4.0f;

        [Header("Chiêu 2: Gồng & Lao tới (Telegraph + Dash)")]
        [SerializeField, Min(1f)] private float dashCooldown = 5.5f;
        [SerializeField, Min(0.2f)] private float dashWindupDuration = 0.65f;
        [SerializeField, Min(0.1f)] private float dashDuration = 0.45f;
        [SerializeField, Min(0.1f)] private float dashRecoverDuration = 0.45f;
        [SerializeField, Min(2f)] private float dashSpeed = 12.5f;
        [SerializeField] private Color windupWarningColor = new Color(1f, 0.92f, 0.2f, 1f);

        [Header("Pha 2: Cuồng nộ (< 50% HP)")]
        [SerializeField, Range(0.1f, 0.9f)] private float enrageHealthThreshold = 0.5f;
        [SerializeField, Min(0)] private int enrageExtraProjectiles = 4;
        [SerializeField, Range(0.5f, 1f)] private float enrageCooldownMultiplier = 0.75f;

        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        private EnemyChaseMovement _movement;
        private EnemyController _enemyController;
        private Health _health;
        private Transform _target;
        private ComponentPool<EnemyProjectile> _projectilePool;

        private BossState _state = BossState.Chasing;
        private float _stateTimer;
        private float _burstTimer;
        private float _dashTimer;
        private float _burstAngleOffset;
        private Vector2 _dashDirection;

        private bool IsEnraged =>
            _health != null &&
            _health.MaxHealth > 0 &&
            (float)_health.CurrentHealth / _health.MaxHealth <= enrageHealthThreshold;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _movement = GetComponent<EnemyChaseMovement>();
            _enemyController = GetComponent<EnemyController>();
            _health = GetComponent<Health>();

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                _target = playerObj.transform;
            }
        }

        private void OnEnable()
        {
            _state = BossState.Chasing;
            _burstTimer = 1.8f;
            _dashTimer = 3.5f;
            _burstAngleOffset = 0f;
            _movement.SetCanMove(true);
        }

        private void Update()
        {
            if (_health.IsDead || _target == null) return;

            switch (_state)
            {
                case BossState.Chasing:
                    UpdateChasingState();
                    break;

                case BossState.WindupDash:
                    UpdateWindupState();
                    break;

                case BossState.Dashing:
                    UpdateDashingState();
                    break;

                case BossState.Recovering:
                    UpdateRecoveringState();
                    break;
            }
        }

        private void FixedUpdate()
        {
            if (_health.IsDead) return;

            if (_state == BossState.Dashing)
            {
                _rb.linearVelocity = _dashDirection * dashSpeed;
            }
            else if (_state == BossState.WindupDash || _state == BossState.Recovering)
            {
                _rb.linearVelocity = Vector2.zero;
            }
        }

        private void UpdateChasingState()
        {
            float cdMult = IsEnraged ? enrageCooldownMultiplier : 1f;

            _burstTimer -= Time.deltaTime;
            if (_burstTimer <= 0f)
            {
                FireRadialBurst();
                _burstTimer = burstCooldown * cdMult;
            }

            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f)
            {
                StartDashWindup();
                _dashTimer = dashCooldown * cdMult;
            }
        }

        private void StartDashWindup()
        {
            _state = BossState.WindupDash;
            _stateTimer = dashWindupDuration;
            _movement.SetCanMove(false);
            _rb.linearVelocity = Vector2.zero;

            // Khóa hướng lao tới người chơi tại thời điểm bắt đầu gồng
            Vector2 toTarget = (Vector2)_target.position - (Vector2)transform.position;
            _dashDirection = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : Vector2.right;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = windupWarningColor;
            }
        }

        private void UpdateWindupState()
        {
            // Nhấp nháy cảnh báo nhẹ trong lúc gồng để người chơi dễ nhận biết
            if (_spriteRenderer != null)
            {
                float pulse = Mathf.PingPong(Time.time * 14f, 1f);
                _spriteRenderer.color = Color.Lerp(GetBaseTintColor(), windupWarningColor, pulse);
            }

            _stateTimer -= Time.deltaTime;
            if (_stateTimer <= 0f)
            {
                _state = BossState.Dashing;
                _stateTimer = dashDuration;
                RestoreBaseColor();
            }
        }

        private void UpdateDashingState()
        {
            _stateTimer -= Time.deltaTime;
            if (_stateTimer <= 0f)
            {
                _state = BossState.Recovering;
                _stateTimer = dashRecoverDuration;
                _rb.linearVelocity = Vector2.zero;

                // Nếu đang ở Pha Cuồng nộ, kết thúc cú lao sẽ xả ngay 1 vòng đạn tỏa tròn
                if (IsEnraged)
                {
                    FireRadialBurst();
                }
            }
        }

        private void UpdateRecoveringState()
        {
            _stateTimer -= Time.deltaTime;
            if (_stateTimer <= 0f)
            {
                _state = BossState.Chasing;
                _movement.SetCanMove(true);
            }
        }

        private void FireRadialBurst()
        {
            if (projectilePrefab == null) return;

            if (_projectilePool == null && PoolManager.Instance != null)
            {
                _projectilePool = PoolManager.Instance.GetOrCreatePool(projectilePrefab, projectilePrewarmCount);
            }

            int count = IsEnraged ? burstProjectileCount + enrageExtraProjectiles : burstProjectileCount;
            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                float angleDeg = _burstAngleOffset + i * angleStep;
                float rad = angleDeg * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                if (_projectilePool != null)
                {
                    EnemyProjectile proj = _projectilePool.Get(transform.position, Quaternion.identity);
                    proj.Initialize(dir, projectileSpeed, projectileDamage, projectileLifetime, _projectilePool.ReleaseCallback);
                }
                else
                {
                    EnemyProjectile proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                    proj.Initialize(dir, projectileSpeed, projectileDamage, projectileLifetime, null);
                }
            }

            // Xoay lệch góc mỗi đợt bắn để đường đạn đa dạng hơn
            _burstAngleOffset = (_burstAngleOffset + angleStep * 0.5f) % 360f;
        }

        private Color GetBaseTintColor()
        {
            return (_enemyController != null && _enemyController.Data != null)
                ? _enemyController.Data.TintColor
                : Color.red;
        }

        private void RestoreBaseColor()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = GetBaseTintColor();
            }
        }
    }
}