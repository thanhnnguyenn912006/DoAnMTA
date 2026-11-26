using System.Collections.Generic;
using UnityEngine;
using SurvivorGame.Systems;

namespace SurvivorGame.Weapons
{
    /// <summary>
    /// Một vật thể xoay quanh người chơi của OrbitWeapon.
    /// Gây sát thương chạm (Trigger) có thời gian hồi riêng cho từng quái để tránh trừ máu liên tục mỗi frame.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class OrbitOrb : MonoBehaviour
    {
        private static Material s_sharedTrailMaterial;
        private static Gradient s_fireTrailGradient;

        private int _damage = 10;
        private float _hitCooldown = 0.35f;
        private LayerMask _enemyLayer;
        private SpriteRenderer _spriteRenderer;
        private TrailRenderer _trailRenderer;
        private Sprite _defaultSprite;
        private Color _defaultColor = Color.white;
        private Vector3 _defaultScale = Vector3.one;
        private float _selfSpinSpeed;
        private bool _hasCachedVisualDefaults;
        private WeaponBase _sourceWeapon;

        // Lưu thời điểm gây sát thương gần nhất lên từng quái (tối đa ~150 quái từ Pool)
        private readonly Dictionary<Health, float> _lastHitTimes = new Dictionary<Health, float>();

        private void Awake()
        {
            CacheVisualDefaultsIfNeeded();
        }

        private void CacheVisualDefaultsIfNeeded()
        {
            if (_hasCachedVisualDefaults) return;

            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
            {
                _defaultSprite = _spriteRenderer.sprite;
                _defaultColor = _spriteRenderer.color;
            }
            _defaultScale = transform.localScale;
            _hasCachedVisualDefaults = true;
        }

        private void Update()
        {
            if (_selfSpinSpeed != 0f)
            {
                transform.Rotate(0f, 0f, _selfSpinSpeed * Time.deltaTime);
            }
        }

        public void Configure(
            int damage,
            float hitCooldown,
            LayerMask enemyLayer,
            Color? tintColor = null,
            float scaleMultiplier = 1f,
            WeaponBase sourceWeapon = null,
            Sprite overrideSprite = null,
            float selfSpinSpeed = 0f,
            bool enableTrail = false)
        {
            CacheVisualDefaultsIfNeeded();

            _damage = Mathf.Max(1, damage);
            _hitCooldown = Mathf.Max(0.05f, hitCooldown);
            _enemyLayer = enemyLayer;
            _sourceWeapon = sourceWeapon;
            _selfSpinSpeed = selfSpinSpeed;

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

            if (_selfSpinSpeed == 0f)
            {
                transform.localRotation = Quaternion.identity;
            }

            transform.localScale = _defaultScale * Mathf.Max(0.1f, scaleMultiplier);
            ConfigureTrail(enableTrail);
        }

        private void OnDisable()
        {
            _lastHitTimes.Clear();
            if (_trailRenderer != null)
            {
                _trailRenderer.emitting = false;
                _trailRenderer.Clear();
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

                _trailRenderer.time = 0.18f;
                _trailRenderer.minVertexDistance = 0.08f;
                _trailRenderer.startWidth = 0.36f;
                _trailRenderer.endWidth = 0.04f;
                _trailRenderer.numCapVertices = 2;
                _trailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _trailRenderer.receiveShadows = false;
                _trailRenderer.colorGradient = GetFireTrailGradient();

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

        private static Gradient GetFireTrailGradient()
        {
            if (s_fireTrailGradient == null)
            {
                s_fireTrailGradient = new Gradient();
                s_fireTrailGradient.SetKeys(
                    new[]
                    {
                        new GradientColorKey(new Color(1f, 0.94f, 0.54f), 0f),
                        new GradientColorKey(new Color(1f, 0.42f, 0.04f), 0.45f),
                        new GradientColorKey(new Color(0.86f, 0.12f, 0.04f), 1f)
                    },
                    new[]
                    {
                        new GradientAlphaKey(0.9f, 0f),
                        new GradientAlphaKey(0.55f, 0.5f),
                        new GradientAlphaKey(0f, 1f)
                    });
            }
            return s_fireTrailGradient;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryHitEnemy(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryHitEnemy(other);
        }

        private void TryHitEnemy(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & _enemyLayer.value) == 0) return;

            if (!other.TryGetComponent<Health>(out Health enemyHealth) || enemyHealth.IsDead)
            {
                return;
            }

            float now = Time.time;
            if (_lastHitTimes.TryGetValue(enemyHealth, out float lastHitTime) && now - lastHitTime < _hitCooldown)
            {
                return;
            }

            _lastHitTimes[enemyHealth] = now;
            int hpBefore = enemyHealth.CurrentHealth;
            enemyHealth.TakeDamage(_damage);
            _sourceWeapon?.RecordDamageDealt(hpBefore - enemyHealth.CurrentHealth);
        }
    }
}