using System;
using TMPro;
using UnityEngine;

namespace SurvivorGame.Systems
{
    /// <summary>
    /// Hiển thị con số sát thương nảy lên tại vị trí trúng đòn rồi mờ dần và tự trả về Object Pool.
    /// Sử dụng TMP.SetText để hạn chế cấp phát chuỗi rác (GC Alloc).
    /// </summary>
    [RequireComponent(typeof(TextMeshPro))]
    public class DamagePopup : MonoBehaviour
    {
        [Header("Cấu hình chuyển động")]
        [SerializeField, Min(0.1f)] private float lifetime = 0.65f;
        [SerializeField] private float floatSpeed = 1.6f;
        [SerializeField] private float horizontalDriftRange = 0.55f;
        [SerializeField] private int sortingOrder = 50;

        private TextMeshPro _tmp;
        private Action<DamagePopup> _returnToPoolCallback;

        private Vector3 _velocity;
        private Color _baseColor;
        private float _baseScale;
        private float _timer;
        private bool _isActive;

        private void Awake()
        {
            _tmp = GetComponent<TextMeshPro>();

            // Đảm bảo chữ sát thương luôn nổi lên trên toàn bộ quái, đạn và nhân vật
            int entitiesLayerId = SortingLayer.NameToID("Entities");
            if (entitiesLayerId != 0)
            {
                _tmp.sortingLayerID = entitiesLayerId;
            }
            _tmp.sortingOrder = sortingOrder;
        }

        public void Initialize(
            int damageAmount,
            Color color,
            bool isPlayerDamage,
            float scaleMultiplier,
            Action<DamagePopup> returnToPoolCallback)
        {
            _returnToPoolCallback = returnToPoolCallback;
            _baseColor = color;
            _baseScale = Mathf.Max(0.5f, scaleMultiplier);
            _timer = 0f;
            _isActive = true;

            if (isPlayerDamage)
            {
                _tmp.SetText("-{0}", damageAmount);
            }
            else
            {
                _tmp.SetText("{0}", damageAmount);
            }

            _tmp.color = _baseColor;
            transform.localScale = Vector3.one * (_baseScale * 0.7f);

            float driftX = UnityEngine.Random.Range(-horizontalDriftRange, horizontalDriftRange);
            _velocity = new Vector3(driftX, floatSpeed, 0f);
        }

        private void Update()
        {
            if (!_isActive) return;

            _timer += Time.deltaTime;
            float progress = Mathf.Clamp01(_timer / lifetime);

            // Bay lên và giảm tốc nhẹ
            transform.position += _velocity * Time.deltaTime;
            _velocity.y = Mathf.Lerp(floatSpeed, 0.3f, progress);

            // Hiệu ứng nảy to nhanh lúc đầu rồi thu gọn nhẹ
            float scaleFactor = progress < 0.2f
                ? Mathf.Lerp(0.7f, 1.2f, progress / 0.2f)
                : Mathf.Lerp(1.2f, 0.9f, (progress - 0.2f) / 0.8f);
            transform.localScale = Vector3.one * (_baseScale * scaleFactor);

            // Mờ dần trong nửa sau vòng đời
            if (progress > 0.45f)
            {
                float fadeProgress = (progress - 0.45f) / 0.55f;
                Color c = _baseColor;
                c.a = Mathf.Lerp(_baseColor.a, 0f, fadeProgress);
                _tmp.color = c;
            }

            if (progress >= 1f)
            {
                Despawn();
            }
        }

        private void Despawn()
        {
            _isActive = false;

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