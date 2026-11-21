using System.Collections;
using UnityEngine;

namespace SurvivorGame.Systems
{
    /// <summary>
    /// Tạo hiệu ứng chớp màu khi trúng đòn và nhấp nháy mờ trong thời gian miễn nhiễm.
    /// Lắng nghe sự kiện OnDamaged từ component Health.
    /// </summary>
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class DamageFlash : MonoBehaviour
    {
        [Header("Cấu hình hiệu ứng")]
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float flashDuration = 0.1f;
        [SerializeField] private float blinkInterval = 0.08f;
        [SerializeField, Range(0.1f, 0.9f)] private float blinkAlpha = 0.35f;

        private Health _health;
        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;
        private Coroutine _flashCoroutine;

        // Cache sẵn WaitForSeconds để không cấp phát bộ nhớ mới mỗi lần trúng đòn
        private WaitForSeconds _flashWait;
        private WaitForSeconds _blinkWait;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _originalColor = _spriteRenderer.color;

            _flashWait = new WaitForSeconds(flashDuration);
            _blinkWait = new WaitForSeconds(blinkInterval);
        }

        private void OnEnable()
        {
            _health.OnDamaged += HandleDamaged;
            ResetColor();
        }

        private void OnDisable()
        {
            _health.OnDamaged -= HandleDamaged;
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
                _flashCoroutine = null;
            }
            ResetColor();
        }

        /// <summary>
        /// Cập nhật lại màu gốc khi đổi màu sprite động (ví dụ khi sinh loại quái khác nhau từ Pool).
        /// </summary>
        public void SetBaseColor(Color newBaseColor)
        {
            _originalColor = newBaseColor;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _originalColor;
            }
        }

        private void HandleDamaged(int damageAmount)
        {
            if (!gameObject.activeInHierarchy) return;

            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }
            _flashCoroutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            // Bước 1: Chớp màu trắng (hoặc đỏ) ngắn khi vừa dính đòn
            _spriteRenderer.color = flashColor;
            yield return _flashWait;

            // Bước 2: Nếu đối tượng có thời gian miễn nhiễm (Player), nhấp nháy mờ đến khi hết miễn nhiễm
            bool isFaded = false;
            Color fadedColor = new Color(_originalColor.r, _originalColor.g, _originalColor.b, blinkAlpha);

            while (_health.IsInvincible && !_health.IsDead)
            {
                isFaded = !isFaded;
                _spriteRenderer.color = isFaded ? fadedColor : _originalColor;
                yield return _blinkWait;
            }

            ResetColor();
            _flashCoroutine = null;
        }

        private void ResetColor()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _originalColor;
            }
        }
    }
}