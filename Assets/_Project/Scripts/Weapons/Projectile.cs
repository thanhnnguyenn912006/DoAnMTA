using System;
using UnityEngine;
using SurvivorGame.Systems;

namespace SurvivorGame.Weapons
{
    /// <summary>
    /// Điều khiển hành vi của một viên đạn: bay theo hướng chỉ định, gây sát thương khi chạm mục tiêu và tự hủy khi hết thời gian tồn tại.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("Cấu hình mặc định")]
        [SerializeField] private float defaultLifetime = 3f;

        private static Material s_sharedTrailMaterial;
        private static Gradient s_evolvedTrailGradient;

        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        private TrailRenderer _trailRenderer;
        private Sprite _defaultSprite;
        private Color _defaultColor = Color.white;
        private Vector3 _defaultScale = Vector3.one;
        private bool _hasCachedVisualDefaults;
        private int _damage;
        private float _lifeTimer;
        private bool _hasHit;
        private int _remainingPierces;
        private readonly Health[] _hitTargets = new Health[8];
        private int _hitCount;
        private WeaponBase _sourceWeapon;
        private Action<Projectile> _returnToPoolCallback;

        private void Awake()
        {
            CacheVisualDefaultsIfNeeded();
        }

        private void CacheVisualDefaultsIfNeeded()
        {
            if (_hasCachedVisualDefaults) return;

            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
            {
                _defaultSprite = _spriteRenderer.sprite;
                _defaultColor = _spriteRenderer.color;
            }
            _defaultScale = transform.localScale;
            _hasCachedVisualDefaults = true;
        }

        private void OnDisable()
        {
            if (_trailRenderer != null)
            {
                _trailRenderer.emitting = false;
                _trailRenderer.Clear();
            }
        }

        private void Update()
        {
            _lifeTimer -= Time.deltaTime;
            if (_lifeTimer <= 0f)
            {
                Despawn();
            }
        }

        /// <summary>
        /// Khởi chạy viên đạn với hướng bay, tốc độ, sát thương, số mục tiêu xuyên và hiệu ứng tiến hóa tùy chọn.
        /// </summary>
        public void Initialize(
            Vector2 direction,
            float speed,
            int damage,
            float lifetime = -1f,
            Action<Projectile> returnToPoolCallback = null,
            int pierceCount = 0,
            Color? tintColor = null,
            WeaponBase sourceWeapon = null,
            Sprite overrideSprite = null,
            float scaleMultiplier = 1f,
            bool enableTrail = false)
        {
            CacheVisualDefaultsIfNeeded();

            Vector2 dir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;

            _damage = damage;
            _lifeTimer = lifetime > 0f ? lifetime : defaultLifetime;
            _hasHit = false;
            _remainingPierces = Mathf.Max(0, pierceCount);
            _sourceWeapon = sourceWeapon;
            ClearHitTargets();
            _returnToPoolCallback = returnToPoolCallback;

            if (_spriteRenderer != null)
            {
                if (overrideSprite != null)
                {
                    _spriteRenderer.sprite = overrideSprite;
                    _spriteRenderer.color = Color.white;
                }
                else
                {
                    _spriteRenderer.sprite = _defaultSprite;
                    _spriteRenderer.color = tintColor.HasValue ? tintColor.Value : _defaultColor;
                }
            }

            transform.localScale = _defaultScale * Mathf.Max(0.1f, scaleMultiplier);

            // Xoay đầu viên đạn theo đúng hướng bay
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            ConfigureTrail(enableTrail);

            if (_rb != null)
            {
                _rb.linearVelocity = dir * speed;
            }
        }

        private void ConfigureTrail(bool enableTrail)
        {
            if (!enableTrail)
            {
                if (_trailRenderer != null)
                {
                    _trailRenderer.emitting = false;
                    _trailRenderer.Clear();
                }
                return;
            }

            if (_trailRenderer == null)
            {
                _trailRenderer = GetComponent<TrailRenderer>();
                if (_trailRenderer == null)
                {
                    _trailRenderer = gameObject.AddComponent<TrailRenderer>();
                }

                _trailRenderer.time = 0.14f;
                _trailRenderer.minVertexDistance = 0.05f;
                _trailRenderer.startWidth = 0.24f;
                _trailRenderer.endWidth = 0.02f;
                _trailRenderer.numCapVertices = 2;
                _trailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _trailRenderer.receiveShadows = false;
                _trailRenderer.colorGradient = GetEvolvedTrailGradient();

                Material trailMat = GetSharedTrailMaterial();
                if (trailMat != null)
                {
                    _trailRenderer.sharedMaterial = trailMat;
                }

                if (_spriteRenderer != null)
                {
                    _trailRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
                    _trailRenderer.sortingOrder = _spriteRenderer.sortingOrder - 1;
                }
            }

            _trailRenderer.Clear();
            _trailRenderer.emitting = true;
        }

        private Material GetSharedTrailMaterial()
        {
            if (s_sharedTrailMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    s_sharedTrailMaterial = new Material(shader)
                    {
                        hideFlags = HideFlags.DontSave
                    };
                }
            }
            return s_sharedTrailMaterial != null ? s_sharedTrailMaterial : (_spriteRenderer != null ? _spriteRenderer.sharedMaterial : null);
        }

        private static Gradient GetEvolvedTrailGradient()
        {
            if (s_evolvedTrailGradient == null)
            {
                s_evolvedTrailGradient = new Gradient();
                s_evolvedTrailGradient.SetKeys(
                    new[]
                    {
                        new GradientColorKey(new Color(0.92f, 1f, 1f), 0f),
                        new GradientColorKey(new Color(0f, 0.94f, 1f), 0.35f),
                        new GradientColorKey(new Color(0.15f, 0.39f, 0.92f), 1f)
                    },
                    new[]
                    {
                        new GradientAlphaKey(0.95f, 0f),
                        new GradientAlphaKey(0.65f, 0.45f),
                        new GradientAlphaKey(0f, 1f)
                    });
            }
            return s_evolvedTrailGradient;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasHit) return;

            // Physics 2D Matrix đã lọc chỉ cho PlayerProjectile chạm Enemy hoặc Obstacle
            if (other.TryGetComponent<Health>(out Health targetHealth))
            {
                if (targetHealth.IsDead || HasAlreadyHit(targetHealth)) return;

                if (_hitCount < _hitTargets.Length)
                {
                    _hitTargets[_hitCount++] = targetHealth;
                }

                int hpBefore = targetHealth.CurrentHealth;
                targetHealth.TakeDamage(_damage);
                _sourceWeapon?.RecordDamageDealt(hpBefore - targetHealth.CurrentHealth);

                if (_remainingPierces > 0)
                {
                    _remainingPierces--;
                    return;
                }
            }

            _hasHit = true;
            Despawn();
        }

        private bool HasAlreadyHit(Health targetHealth)
        {
            for (int i = 0; i < _hitCount; i++)
            {
                if (_hitTargets[i] == targetHealth)
                {
                    return true;
                }
            }
            return false;
        }

        private void ClearHitTargets()
        {
            for (int i = 0; i < _hitCount; i++)
            {
                _hitTargets[i] = null;
            }
            _hitCount = 0;
        }

        /// <summary>
        /// Trả đạn về Object Pool (nếu có) hoặc hủy đối tượng.
        /// </summary>
        private void Despawn()
        {
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
            }
            if (_trailRenderer != null)
            {
                _trailRenderer.emitting = false;
                _trailRenderer.Clear();
            }
            _sourceWeapon = null;
            ClearHitTargets();

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