using System;
using UnityEngine;
using SurvivorGame.Systems;

namespace SurvivorGame.Player
{
    /// <summary>
    /// Quản lý việc đọc phím điều khiển, di chuyển 8 hướng bằng Rigidbody2D và kỹ năng Lướt (Dash) xuyên quái/đạn.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Thông số di chuyển")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private SpriteRenderer playerSpriteRenderer;

        [Header("Kỹ năng Lướt (Dash)")]
        [SerializeField, Min(1f)] private float dashSpeed = 14f;
        [SerializeField, Min(0.05f)] private float dashDuration = 0.16f;
        [SerializeField, Min(0.1f)] private float dashCooldown = 2.5f;
        [SerializeField, Min(0f)] private float dashInvincibilityDuration = 0.24f;
        [SerializeField, Range(0, 6)] private int dashAfterimageCount = 3;
        [SerializeField, Min(0.05f)] private float afterimageFadeDuration = 0.2f;
        [SerializeField, Range(0.05f, 1f)] private float afterimageStartAlpha = 0.45f;
        [SerializeField] private Color afterimageTint = new Color(0.75f, 0.92f, 1f, 1f);

        private struct AfterimageSlot
        {
            public SpriteRenderer Renderer;
            public Vector3 WorldPosition;
            public float Timer;
        }

        private Rigidbody2D _rb;
        private Health _health;
        private Vector2 _moveInput;
        private Vector2 _lastMoveDirection = Vector2.right;

        private bool _isDashing;
        private float _dashTimer;
        private float _dashCooldownTimer;
        private Vector2 _dashDirection = Vector2.right;
        private float _effectiveDashSpeed;

        private int _playerLayer;
        private int _enemyLayer;
        private int _enemyProjectileLayer;
        private bool _isDashCollisionBypassed;

        private AfterimageSlot[] _afterimages;
        private int _nextAfterimageIndex;
        private float _afterimageSpawnTimer;

        // Cho phép các hệ thống khác (vũ khí, chỉ số bị động, HUD) đọc/tăng tốc chạy và theo dõi Dash
        public float MoveSpeed => moveSpeed;
        public Vector2 MoveDirection => _isDashing ? _dashDirection : _moveInput;
        public bool IsDashing => _isDashing;
        public float DashCooldownRemaining => Mathf.Max(0f, _dashCooldownTimer);
        public float DashCooldownNormalized =>
            dashCooldown <= 0.0001f ? 1f : Mathf.Clamp01(1f - (_dashCooldownTimer / dashCooldown));

        public event Action OnDashed;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _health = GetComponent<Health>();

            if (playerSpriteRenderer == null)
            {
                playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            _playerLayer = gameObject.layer;
            _enemyLayer = LayerMask.NameToLayer("Enemy");
            _enemyProjectileLayer = LayerMask.NameToLayer("EnemyProjectile");

            CreateAfterimages();
        }

        private void OnEnable()
        {
            SetDashCollisionBypass(false);
        }

        private void OnDisable()
        {
            ResetDashRuntimeState();
        }

        private void OnDestroy()
        {
            ResetDashRuntimeState();
        }

        private void Update()
        {
            if (_dashCooldownTimer > 0f)
            {
                _dashCooldownTimer = Mathf.Max(0f, _dashCooldownTimer - Time.deltaTime);
            }

            if (Time.timeScale <= 0f)
            {
                _moveInput = Vector2.zero;
                return;
            }

            if (_health != null && _health.IsDead)
            {
                _moveInput = Vector2.zero;
                return;
            }

            // Đọc input không có quán tính trượt (GetAxisRaw trả về -1, 0 hoặc 1)
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");

            // Chuẩn hóa vector để đi chéo không nhanh hơn đi thẳng
            _moveInput = new Vector2(moveX, moveY).normalized;

            if (_moveInput.sqrMagnitude > 0.01f)
            {
                _lastMoveDirection = _moveInput;
            }

            if (CanStartDash() && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift)))
            {
                StartDash();
            }

            if (_isDashing)
            {
                _dashTimer -= Time.deltaTime;
                _afterimageSpawnTimer -= Time.deltaTime;

                if (_afterimageSpawnTimer <= 0f)
                {
                    SpawnAfterimage();
                    _afterimageSpawnTimer = Mathf.Max(0.02f, dashDuration / Mathf.Max(1, dashAfterimageCount));
                }

                if (_dashTimer <= 0f)
                {
                    EndDash();
                }
            }
        }

        private void FixedUpdate()
        {
            if (_health != null && _health.IsDead)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            if (_isDashing)
            {
                _rb.linearVelocity = _dashDirection * _effectiveDashSpeed;
                return;
            }

            // Dùng linearVelocity chuẩn của Unity 6 để tránh cảnh báo deprecated
            _rb.linearVelocity = _moveInput * moveSpeed;
        }

        private void LateUpdate()
        {
            UpdateAfterimages();
        }

        private bool CanStartDash()
        {
            if (Time.timeScale <= 0f || _isDashing || _dashCooldownTimer > 0f) return false;
            if (_health != null && _health.IsDead) return false;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return false;
            return true;
        }

        private void StartDash()
        {
            _isDashing = true;
            _dashTimer = dashDuration;
            _dashCooldownTimer = dashCooldown;
            _dashDirection = _moveInput.sqrMagnitude > 0.01f ? _moveInput.normalized : _lastMoveDirection;
            if (_dashDirection.sqrMagnitude < 0.0001f)
            {
                _dashDirection = Vector2.right;
            }

            _effectiveDashSpeed = Mathf.Max(dashSpeed, moveSpeed * 2.4f);
            _afterimageSpawnTimer = Mathf.Max(0.02f, dashDuration / Mathf.Max(1, dashAfterimageCount));

            SetDashCollisionBypass(true);

            if (_health != null && dashInvincibilityDuration > 0f)
            {
                _health.GrantInvincibility(dashInvincibilityDuration);
            }

            SpawnAfterimage();
            AudioManager.Instance?.PlaySfx(SoundId.Dash, 0.9f, 0.05f, 0.06f);
            OnDashed?.Invoke();
        }

        private void EndDash()
        {
            _isDashing = false;
            _dashTimer = 0f;
            SetDashCollisionBypass(false);
        }

        /// <summary>
        /// Tăng tốc độ di chuyển (dùng khi người chơi chọn nâng cấp tốc chạy).
        /// </summary>
        public void AddMoveSpeed(float amount)
        {
            moveSpeed = Mathf.Max(0.5f, moveSpeed + amount);
        }

        /// <summary>
        /// Dừng hẳn nhân vật ngay lập tức (dùng khi chết hoặc thắng).
        /// </summary>
        public void StopMovement()
        {
            _moveInput = Vector2.zero;
            ResetDashRuntimeState();
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
            }
        }

        private void ResetDashRuntimeState()
        {
            _isDashing = false;
            _dashTimer = 0f;
            SetDashCollisionBypass(false);

            if (_afterimages == null) return;
            for (int i = 0; i < _afterimages.Length; i++)
            {
                if (_afterimages[i].Renderer != null)
                {
                    _afterimages[i].Renderer.gameObject.SetActive(false);
                }
                _afterimages[i].Timer = 0f;
            }
        }

        private void SetDashCollisionBypass(bool ignore)
        {
            if (!ignore && !_isDashCollisionBypassed) return;

            if (_enemyLayer >= 0)
            {
                Physics2D.IgnoreLayerCollision(_playerLayer, _enemyLayer, ignore);
            }

            if (_enemyProjectileLayer >= 0)
            {
                Physics2D.IgnoreLayerCollision(_playerLayer, _enemyProjectileLayer, ignore);
            }

            _isDashCollisionBypassed = ignore;
        }

        private void CreateAfterimages()
        {
            if (dashAfterimageCount <= 0)
            {
                _afterimages = Array.Empty<AfterimageSlot>();
                return;
            }

            _afterimages = new AfterimageSlot[dashAfterimageCount];
            for (int i = 0; i < dashAfterimageCount; i++)
            {
                GameObject ghostObj = new GameObject($"DashAfterimage_{i}", typeof(SpriteRenderer));
                ghostObj.transform.SetParent(transform, false);
                ghostObj.SetActive(false);

                SpriteRenderer sr = ghostObj.GetComponent<SpriteRenderer>();
                if (playerSpriteRenderer != null)
                {
                    sr.sortingLayerID = playerSpriteRenderer.sortingLayerID;
                    sr.sortingOrder = playerSpriteRenderer.sortingOrder - 1;
                }

                _afterimages[i] = new AfterimageSlot
                {
                    Renderer = sr,
                    WorldPosition = Vector3.zero,
                    Timer = 0f
                };
            }
        }

        private void SpawnAfterimage()
        {
            if (_afterimages == null || _afterimages.Length == 0 || playerSpriteRenderer == null) return;

            int index = _nextAfterimageIndex;
            _nextAfterimageIndex = (_nextAfterimageIndex + 1) % _afterimages.Length;

            AfterimageSlot slot = _afterimages[index];
            if (slot.Renderer == null) return;

            slot.WorldPosition = playerSpriteRenderer.transform.position;
            slot.Timer = afterimageFadeDuration;

            slot.Renderer.sprite = playerSpriteRenderer.sprite;
            slot.Renderer.flipX = playerSpriteRenderer.flipX;
            slot.Renderer.sortingLayerID = playerSpriteRenderer.sortingLayerID;
            slot.Renderer.sortingOrder = playerSpriteRenderer.sortingOrder - 1;

            Color c = afterimageTint;
            c.a = afterimageStartAlpha;
            slot.Renderer.color = c;

            slot.Renderer.transform.position = slot.WorldPosition;
            slot.Renderer.gameObject.SetActive(true);

            _afterimages[index] = slot;
        }

        private void UpdateAfterimages()
        {
            if (_afterimages == null || _afterimages.Length == 0) return;

            float dt = Time.deltaTime;
            for (int i = 0; i < _afterimages.Length; i++)
            {
                AfterimageSlot slot = _afterimages[i];
                if (slot.Renderer == null || !slot.Renderer.gameObject.activeSelf) continue;

                // Giữ bóng mờ đứng yên tại vị trí thế giới lúc sinh ra dù là con của Player
                slot.Renderer.transform.position = slot.WorldPosition;

                slot.Timer -= dt;
                if (slot.Timer <= 0f)
                {
                    slot.Renderer.gameObject.SetActive(false);
                    slot.Timer = 0f;
                    _afterimages[i] = slot;
                    continue;
                }

                float ratio = Mathf.Clamp01(slot.Timer / Mathf.Max(0.01f, afterimageFadeDuration));
                Color c = afterimageTint;
                c.a = afterimageStartAlpha * ratio;
                slot.Renderer.color = c;
                _afterimages[i] = slot;
            }
        }
    }
}
