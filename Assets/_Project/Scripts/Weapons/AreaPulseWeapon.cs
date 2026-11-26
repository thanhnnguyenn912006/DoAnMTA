using UnityEngine;
using SurvivorGame.Data;
using SurvivorGame.Enemies;
using SurvivorGame.Systems;

namespace SurvivorGame.Weapons
{
    /// <summary>
    /// Vũ khí 3: Phát sóng sát thương diện rộng định kỳ quanh nhân vật.
    /// Sử dụng WeaponData: Damage = sát thương mỗi nhịp, Cooldown = chu kỳ phát sóng, Range = bán kính vùng ảnh hưởng.
    /// </summary>
    public class AreaPulseWeapon : WeaponBase
    {
        [Header("Cấu hình quét mục tiêu")]
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private bool onlyPulseWhenEnemyInRange = false;

        [Header("Hiệu ứng vòng sóng hiển thị")]
        [SerializeField] private SpriteRenderer pulseVisual;
        [SerializeField] private Color pulseColor = new Color(0.66f, 0.33f, 0.97f, 0.45f); // Tím năng lượng trong suốt
        [SerializeField, Min(0.05f)] private float pulseVisualDuration = 0.35f;

        // Bộ đệm quét cấp phát 1 lần duy nhất để giữ 0 GC Alloc
        private readonly Collider2D[] _hitBuffer = new Collider2D[160];
        private ContactFilter2D _enemyFilter;

        private const float KnockbackDuration = 0.16f;
        private const float EvolvedEchoDelay = 0.08f;
        private const float EvolvedCoreDuration = 0.18f;
        private const float EvolvedMainSpinSpeed = 110f;
        private const float EvolvedEchoSpinSpeed = -160f;
        private const float EvolvedShakeIntensity = 0.08f;
        private const float EvolvedShakeDuration = 0.10f;

        private Sprite _defaultPulseSprite;
        private SpriteRenderer _echoPulseVisual;
        private SpriteRenderer _coreFlashVisual;
        private bool _isAnimatingPulse;
        private bool _isEvolvedPulse;
        private float _pulseAnimTimer;
        private float _currentPulseMaxDiameter;
        private Color _activePulseColor;
        private Color _echoPulseColor = new Color(0.78f, 0.20f, 1f, 0.84f);
        private Color _coreFlashColor = new Color(0.95f, 0.82f, 1f, 0.92f);

        private void Awake()
        {
            _enemyFilter = new ContactFilter2D();
            _enemyFilter.SetLayerMask(enemyLayer);
            _enemyFilter.useTriggers = true;
            _activePulseColor = pulseColor;

            if (pulseVisual != null)
            {
                _defaultPulseSprite = pulseVisual.sprite;
            }

            HideVisual();
        }

        private void OnDisable()
        {
            _isAnimatingPulse = false;
            HideVisual();
        }

        protected override bool Attack()
        {
            WeaponLevelStats stats = CurrentStats;
            float radius = Mathf.Max(0.5f, stats.range);

            int hitCount = Physics2D.OverlapCircle(transform.position, radius, _enemyFilter, _hitBuffer);

            if (onlyPulseWhenEnemyInRange && hitCount == 0)
            {
                return false;
            }

            int damage = GetEffectiveDamage();
            float knockbackForce = IsEvolved && weaponData != null ? weaponData.EvolvedKnockbackForce : 0f;
            Vector2 origin = transform.position;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D col = _hitBuffer[i];
                if (col == null || !col.gameObject.activeInHierarchy) continue;

                if (col.TryGetComponent<Health>(out Health enemyHealth) && !enemyHealth.IsDead)
                {
                    int hpBefore = enemyHealth.CurrentHealth;
                    enemyHealth.TakeDamage(damage);
                    RecordDamageDealt(hpBefore - enemyHealth.CurrentHealth);

                    if (knockbackForce > 0f &&
                        !enemyHealth.IsDead &&
                        col.gameObject.activeInHierarchy &&
                        !col.CompareTag("Boss") &&
                        col.TryGetComponent<EnemyChaseMovement>(out EnemyChaseMovement chaseMovement))
                    {
                        Vector2 pushDir = (Vector2)col.transform.position - origin;
                        pushDir = pushDir.sqrMagnitude > 0.0001f ? pushDir.normalized : Vector2.right;
                        chaseMovement.ApplyKnockback(pushDir * knockbackForce, KnockbackDuration);
                    }
                }
            }

            StartPulseVisual(radius);
            return true;
        }

        private void LateUpdate()
        {
            if (!_isAnimatingPulse || pulseVisual == null) return;

            _pulseAnimTimer += Time.deltaTime;
            float duration = Mathf.Max(0.05f, pulseVisualDuration);

            if (!_isEvolvedPulse)
            {
                float progress = Mathf.Clamp01(_pulseAnimTimer / duration);
                float currentScale = Mathf.Lerp(0.25f, _currentPulseMaxDiameter, progress);
                pulseVisual.transform.localScale = new Vector3(currentScale, currentScale, 1f);

                Color c = _activePulseColor;
                c.a = Mathf.Lerp(_activePulseColor.a, 0f, progress);
                pulseVisual.color = c;

                if (progress >= 1f)
                {
                    _isAnimatingPulse = false;
                    HideVisual();
                }
                return;
            }

            // 1. Sóng địa chấn chính bung nhanh (ease-out) + xoay thuận chiều
            float mainT = Mathf.Clamp01(_pulseAnimTimer / duration);
            if (mainT < 1f)
            {
                float eased = 1f - (1f - mainT) * (1f - mainT);
                float mainScale = Mathf.Lerp(0.30f, _currentPulseMaxDiameter, eased);
                pulseVisual.transform.localScale = new Vector3(mainScale, mainScale, 1f);
                pulseVisual.transform.Rotate(0f, 0f, EvolvedMainSpinSpeed * Time.deltaTime);

                Color c = _activePulseColor;
                c.a = Mathf.Lerp(_activePulseColor.a, 0f, mainT * mainT);
                pulseVisual.color = c;
            }
            else
            {
                pulseVisual.enabled = false;
            }

            // 2. Chớp năng lượng bùng nổ nhanh ở tâm nhân vật
            if (_coreFlashVisual != null)
            {
                float coreT = Mathf.Clamp01(_pulseAnimTimer / EvolvedCoreDuration);
                if (coreT < 1f)
                {
                    float coreEase = 1f - (1f - coreT) * (1f - coreT);
                    float coreScale = Mathf.Lerp(0.20f, _currentPulseMaxDiameter * 0.56f, coreEase);
                    _coreFlashVisual.transform.localScale = new Vector3(coreScale, coreScale, 1f);
                    Color coreCol = _coreFlashColor;
                    coreCol.a = Mathf.Lerp(_coreFlashColor.a, 0f, coreT);
                    _coreFlashVisual.color = coreCol;
                }
                else
                {
                    _coreFlashVisual.enabled = false;
                }
            }

            // 3. Sóng dư chấn cam-đỏ trễ nhịp phía sau + xoay ngược chiều
            if (_echoPulseVisual != null)
            {
                float echoElapsed = _pulseAnimTimer - EvolvedEchoDelay;
                if (echoElapsed >= 0f)
                {
                    float echoT = Mathf.Clamp01(echoElapsed / duration);
                    if (echoT < 1f)
                    {
                        if (!_echoPulseVisual.enabled) _echoPulseVisual.enabled = true;
                        float echoEase = 1f - (1f - echoT) * (1f - echoT);
                        float echoScale = Mathf.Lerp(0.25f, _currentPulseMaxDiameter * 1.05f, echoEase);
                        _echoPulseVisual.transform.localScale = new Vector3(echoScale, echoScale, 1f);
                        _echoPulseVisual.transform.Rotate(0f, 0f, EvolvedEchoSpinSpeed * Time.deltaTime);

                        Color echoCol = _echoPulseColor;
                        echoCol.a = Mathf.Lerp(_echoPulseColor.a, 0f, echoT * echoT);
                        _echoPulseVisual.color = echoCol;
                    }
                    else
                    {
                        _echoPulseVisual.enabled = false;
                    }
                }
            }

            if (_pulseAnimTimer >= duration + EvolvedEchoDelay)
            {
                _isAnimatingPulse = false;
                HideVisual();
            }
        }

        private void StartPulseVisual(float radius)
        {
            if (pulseVisual == null) return;

            if (!pulseVisual.gameObject.activeSelf)
            {
                pulseVisual.gameObject.SetActive(true);
            }

            _currentPulseMaxDiameter = radius * 2f;
            _pulseAnimTimer = 0f;
            _isAnimatingPulse = true;
            _isEvolvedPulse = IsEvolved && weaponData != null;

            if (_isEvolvedPulse)
            {
                Sprite evoSprite = weaponData.EvolvedVisualSprite != null ? weaponData.EvolvedVisualSprite : _defaultPulseSprite;
                pulseVisual.sprite = evoSprite;

                if (weaponData.EvolvedVisualSprite != null)
                {
                    _activePulseColor = new Color(1f, 1f, 1f, 0.96f);
                }
                else
                {
                    Color evo = weaponData.EvolvedTintColor;
                    _activePulseColor = new Color(evo.r, evo.g, evo.b, Mathf.Max(pulseColor.a, 0.82f));
                }

                EnsureEvolvedOverlayVisuals(evoSprite);

                if (_echoPulseVisual != null)
                {
                    _echoPulseVisual.sprite = evoSprite;
                    _echoPulseVisual.transform.localPosition = Vector3.zero;
                    _echoPulseVisual.transform.localRotation = Quaternion.Euler(0f, 0f, 22.5f);
                    _echoPulseVisual.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
                    _echoPulseVisual.color = _echoPulseColor;
                    _echoPulseVisual.enabled = false;
                }

                if (_coreFlashVisual != null)
                {
                    _coreFlashVisual.sprite = evoSprite;
                    _coreFlashVisual.transform.localPosition = Vector3.zero;
                    _coreFlashVisual.transform.localRotation = Quaternion.identity;
                    _coreFlashVisual.transform.localScale = new Vector3(0.20f, 0.20f, 1f);
                    _coreFlashVisual.color = _coreFlashColor;
                    _coreFlashVisual.enabled = true;
                }

                if (CameraShake.Instance != null)
                {
                    CameraShake.Instance.Shake(EvolvedShakeIntensity, EvolvedShakeDuration);
                }
            }
            else
            {
                pulseVisual.sprite = _defaultPulseSprite;
                _activePulseColor = pulseColor;
                if (_echoPulseVisual != null) _echoPulseVisual.enabled = false;
                if (_coreFlashVisual != null) _coreFlashVisual.enabled = false;
            }

            pulseVisual.transform.localPosition = Vector3.zero;
            pulseVisual.transform.localRotation = Quaternion.identity;
            pulseVisual.transform.localScale = new Vector3(0.25f, 0.25f, 1f);
            pulseVisual.color = _activePulseColor;
            pulseVisual.enabled = true;
        }

        private void EnsureEvolvedOverlayVisuals(Sprite sprite)
        {
            Transform parent = pulseVisual.transform.parent != null ? pulseVisual.transform.parent : transform;

            if (_echoPulseVisual == null)
            {
                _echoPulseVisual = CreateOverlayRenderer(parent, "PulseVisual_EvoEcho", sprite, pulseVisual.sortingOrder - 1);
            }

            if (_coreFlashVisual == null)
            {
                _coreFlashVisual = CreateOverlayRenderer(parent, "PulseVisual_EvoCore", sprite, pulseVisual.sortingOrder + 1);
            }
        }

        private SpriteRenderer CreateOverlayRenderer(Transform parent, string objectName, Sprite sprite, int sortingOrder)
        {
            GameObject obj = new GameObject(objectName);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = Vector3.zero;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sharedMaterial = pulseVisual.sharedMaterial;
            sr.sortingLayerID = pulseVisual.sortingLayerID;
            sr.sortingOrder = sortingOrder;
            sr.enabled = false;
            return sr;
        }

        private void HideVisual()
        {
            if (pulseVisual != null)
            {
                pulseVisual.enabled = false;
            }
            if (_echoPulseVisual != null)
            {
                _echoPulseVisual.enabled = false;
            }
            if (_coreFlashVisual != null)
            {
                _coreFlashVisual.enabled = false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (weaponData == null) return;
            Gizmos.color = new Color(0.66f, 0.33f, 0.97f, 1f);
            Gizmos.DrawWireSphere(transform.position, CurrentStats.range);
        }
    }
}