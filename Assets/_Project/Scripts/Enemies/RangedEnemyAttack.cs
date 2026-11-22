using UnityEngine;
using SurvivorGame.Systems;

namespace SurvivorGame.Enemies
{
    /// <summary>
    /// Hành vi chiến đấu của Quái bắn tầm xa: tiến tới tầm bắn, giữ khoảng cách và bắn đạn từ Object Pool.
    /// </summary>
    [RequireComponent(typeof(EnemyChaseMovement))]
    [RequireComponent(typeof(Health))]
    public class RangedEnemyAttack : MonoBehaviour
    {
        [Header("Đạn & Object Pool")]
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField, Min(0)] private int projectilePrewarmCount = 40;

        [Header("Khoảng cách & Nhịp bắn")]
        [SerializeField, Min(1f)] private float stopDistance = 5.0f;
        [SerializeField, Min(1.5f)] private float resumeChaseDistance = 6.0f;
        [SerializeField, Min(1.5f)] private float attackRange = 7.5f;
        [SerializeField, Min(0.3f)] private float fireCooldown = 2.2f;

        [Header("Chỉ số đạn")]
        [SerializeField, Min(1f)] private float projectileSpeed = 6.2f;
        [SerializeField, Min(1)] private int projectileDamage = 10;
        [SerializeField, Min(0.5f)] private float projectileLifetime = 3.5f;

        private EnemyChaseMovement _movement;
        private Health _health;
        private Transform _target;
        private ComponentPool<EnemyProjectile> _projectilePool;
        private float _fireTimer;

        private void Awake()
        {
            _movement = GetComponent<EnemyChaseMovement>();
            _health = GetComponent<Health>();

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                _target = playerObj.transform;
            }
        }

        private void OnEnable()
        {
            // Tạo độ lệch ngẫu nhiên cho phát bắn đầu tiên khi ra khỏi Pool
            _fireTimer = Random.Range(0.7f, fireCooldown);
        }

        private void Update()
        {
            if (_health.IsDead || _target == null || projectilePrefab == null) return;

            Vector2 toTarget = (Vector2)_target.position - (Vector2)transform.position;
            float sqrDist = toTarget.sqrMagnitude;

            // Giữ khoảng cách với vùng đệm (hysteresis) để tránh rung giật
            if (sqrDist <= stopDistance * stopDistance)
            {
                _movement.SetCanMove(false);
            }
            else if (sqrDist >= resumeChaseDistance * resumeChaseDistance)
            {
                _movement.SetCanMove(true);
            }

            _fireTimer -= Time.deltaTime;
            if (_fireTimer <= 0f && sqrDist <= attackRange * attackRange)
            {
                FireAt(toTarget.normalized);
                _fireTimer = fireCooldown;
            }
        }

        private void FireAt(Vector2 direction)
        {
            if (_projectilePool == null && PoolManager.Instance != null)
            {
                _projectilePool = PoolManager.Instance.GetOrCreatePool(projectilePrefab, projectilePrewarmCount);
            }

            if (_projectilePool != null)
            {
                EnemyProjectile proj = _projectilePool.Get(transform.position, Quaternion.identity);
                proj.Initialize(direction, projectileSpeed, projectileDamage, projectileLifetime, _projectilePool.ReleaseCallback);
            }
            else
            {
                EnemyProjectile proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                proj.Initialize(direction, projectileSpeed, projectileDamage, projectileLifetime, null);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, stopDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}