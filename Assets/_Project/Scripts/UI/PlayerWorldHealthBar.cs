using UnityEngine;
using SurvivorGame.Systems;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Thanh máu mini gắn ngay dưới chân nhân vật (kiểu Vampire Survivors).
    /// Cập nhật hoàn toàn theo sự kiện Health.OnHealthChanged (0 Update mỗi frame, 0 GC).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public class PlayerWorldHealthBar : MonoBehaviour
    {
        private static readonly Color BgColor = new Color(0.06f, 0.09f, 0.16f, 0.88f);
        private static readonly Color HighHpColor = new Color(0.133f, 0.773f, 0.369f, 1f); // #22C55E
        private static readonly Color MidHpColor = new Color(0.980f, 0.800f, 0.082f, 1f);  // #FACC15
        private static readonly Color LowHpColor = new Color(0.937f, 0.267f, 0.267f, 1f);  // #EF4444

        [Header("Cấu hình thanh máu dưới chân")]
        [SerializeField] private Vector3 localOffset = new Vector3(0f, -0.68f, 0f);
        [SerializeField, Min(0.2f)] private float barWidth = 1.05f;
        [SerializeField, Min(0.04f)] private float barHeight = 0.13f;
        [SerializeField] private float innerPadding = 0.03f;

        private static Sprite _sharedWhiteSprite;

        private Health _health;
        private GameObject _barRoot;
        private SpriteRenderer _fillRenderer;
        private Transform _fillTransform;

        private void Awake()
        {
            _health = GetComponent<Health>();
            BuildVisuals();
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.OnHealthChanged += HandleHealthChanged;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnHealthChanged -= HandleHealthChanged;
            }
        }

        private void Start()
        {
            if (_health != null)
            {
                HandleHealthChanged(_health.CurrentHealth, _health.MaxHealth);
            }
        }

        private void BuildVisuals()
        {
            Sprite sprite = GetOrCreateWhiteSprite();
            SpriteRenderer playerSr = GetComponent<SpriteRenderer>();
            int sortingLayerId = playerSr != null ? playerSr.sortingLayerID : SortingLayer.NameToID("Entities");
            int baseOrder = playerSr != null ? playerSr.sortingOrder + 5 : 15;

            _barRoot = new GameObject("WorldHealthBar");
            _barRoot.transform.SetParent(transform, false);
            _barRoot.transform.localPosition = localOffset;

            GameObject bgObj = new GameObject("Bg", typeof(SpriteRenderer));
            bgObj.transform.SetParent(_barRoot.transform, false);
            bgObj.transform.localScale = new Vector3(barWidth, barHeight, 1f);
            SpriteRenderer bgSr = bgObj.GetComponent<SpriteRenderer>();
            bgSr.sprite = sprite;
            bgSr.color = BgColor;
            bgSr.sortingLayerID = sortingLayerId;
            bgSr.sortingOrder = baseOrder;

            GameObject fillObj = new GameObject("Fill", typeof(SpriteRenderer));
            fillObj.transform.SetParent(_barRoot.transform, false);
            _fillTransform = fillObj.transform;
            _fillRenderer = fillObj.GetComponent<SpriteRenderer>();
            _fillRenderer.sprite = sprite;
            _fillRenderer.color = HighHpColor;
            _fillRenderer.sortingLayerID = sortingLayerId;
            _fillRenderer.sortingOrder = baseOrder + 1;
        }

        private void HandleHealthChanged(int currentHp, int maxHp)
        {
            if (_barRoot == null || _fillTransform == null || _fillRenderer == null) return;

            bool alive = currentHp > 0;
            _barRoot.SetActive(alive);
            if (!alive) return;

            float ratio = Mathf.Clamp01((float)currentHp / Mathf.Max(1, maxHp));
            float maxFillWidth = Mathf.Max(0.05f, barWidth - innerPadding * 2f);
            float fillHeight = Mathf.Max(0.03f, barHeight - innerPadding * 2f);
            float currentWidth = maxFillWidth * ratio;

            _fillTransform.localScale = new Vector3(currentWidth, fillHeight, 1f);
            _fillTransform.localPosition = new Vector3((currentWidth - maxFillWidth) * 0.5f, 0f, 0f);

            if (ratio > 0.5f)
            {
                _fillRenderer.color = HighHpColor;
            }
            else if (ratio > 0.25f)
            {
                _fillRenderer.color = MidHpColor;
            }
            else
            {
                _fillRenderer.color = LowHpColor;
            }
        }

        private static Sprite GetOrCreateWhiteSprite()
        {
            if (_sharedWhiteSprite != null) return _sharedWhiteSprite;

            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            Color[] pixels = { Color.white, Color.white, Color.white, Color.white };
            tex.SetPixels(pixels);
            tex.Apply();

            _sharedWhiteSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
            _sharedWhiteSprite.hideFlags = HideFlags.HideAndDontSave;
            return _sharedWhiteSprite;
        }
    }
}
