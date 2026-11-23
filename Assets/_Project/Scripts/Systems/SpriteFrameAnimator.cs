using UnityEngine;

namespace SurvivorGame.Systems
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFrameAnimator : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField, Min(0.05f)] private float frameInterval = 0.25f;
        [SerializeField] private bool randomizeStart = true;

        private SpriteRenderer _spriteRenderer;
        private float _timer;
        private int _index;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            ResetAnimation();
        }

        public void SetFrames(Sprite[] newFrames, float newInterval)
        {
            frames = (newFrames != null && newFrames.Length > 0) ? newFrames : null;
            frameInterval = Mathf.Max(0.05f, newInterval);
            ResetAnimation();
        }

        private void Update()
        {
            if (frames == null || frames.Length < 2) return;

            _timer += Time.deltaTime;
            if (_timer >= frameInterval)
            {
                _timer -= frameInterval;
                _index = (_index + 1) % frames.Length;
                _spriteRenderer.sprite = frames[_index];
            }
        }

        private void ResetAnimation()
        {
            if (frames == null || frames.Length == 0 || _spriteRenderer == null) return;

            _index = randomizeStart ? Random.Range(0, frames.Length) : 0;
            _timer = randomizeStart ? Random.Range(0f, frameInterval) : 0f;
            _spriteRenderer.sprite = frames[_index];
        }
    }
}