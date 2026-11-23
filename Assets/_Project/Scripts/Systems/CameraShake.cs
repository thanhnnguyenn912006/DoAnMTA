using UnityEngine;

namespace SurvivorGame.Systems
{
    /// <summary>
    /// Hiệu ứng rung Camera 2D nhẹ khi nhân vật trúng đòn hoặc hạ Boss.
    /// Chạy sau CameraFollow (ExecutionOrder = 100) và tự hoàn trả độ lệch để không gây trôi Camera.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        private float _shakeTimer;
        private float _shakeDuration;
        private float _shakeIntensity;
        private Vector3 _appliedOffset;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void OnDisable()
        {
            ClearAppliedOffset();
        }

        private void OnDestroy()
        {
            ClearAppliedOffset();
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Kích hoạt rung Camera; ưu tiên giữ cường độ mạnh hơn nếu đang có rung trước đó.
        /// </summary>
        public void Shake(float intensity, float duration)
        {
            if (intensity <= 0f || duration <= 0f) return;

            if (intensity >= _shakeIntensity || _shakeTimer <= 0f)
            {
                _shakeIntensity = intensity;
                _shakeDuration = duration;
                _shakeTimer = duration;
            }
        }

        private void Update()
        {
            // Gỡ độ lệch của frame trước để CameraFollow luôn tính toán từ vị trí sạch
            ClearAppliedOffset();

            if (_shakeTimer > 0f)
            {
                _shakeTimer = Mathf.Max(0f, _shakeTimer - Time.deltaTime);
                if (_shakeTimer <= 0f)
                {
                    _shakeIntensity = 0f;
                }
            }
        }

        private void LateUpdate()
        {
            if (_shakeTimer <= 0f || Time.timeScale <= 0f) return;

            float damping = _shakeDuration > 0f ? (_shakeTimer / _shakeDuration) : 0f;
            Vector2 randomOffset = Random.insideUnitCircle * (_shakeIntensity * damping);

            _appliedOffset = new Vector3(randomOffset.x, randomOffset.y, 0f);
            transform.position += _appliedOffset;
        }

        private void ClearAppliedOffset()
        {
            if (_appliedOffset != Vector3.zero)
            {
                transform.position -= _appliedOffset;
                _appliedOffset = Vector3.zero;
            }
        }
    }
}