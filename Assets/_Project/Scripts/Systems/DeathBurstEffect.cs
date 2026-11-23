using System;
using UnityEngine;

namespace SurvivorGame.Systems
{
    /// <summary>
    /// Hiệu ứng hạt nổ bắn tung ra khi quái bị tiêu diệt, tự đổi màu theo quái và trả về Object Pool.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class DeathBurstEffect : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float effectDuration = 0.55f;
        [SerializeField] private int sortingOrder = 12;

        private ParticleSystem _ps;
        private Action<DeathBurstEffect> _returnToPoolCallback;
        private float _timer;
        private bool _isPlaying;

        private void Awake()
        {
            _ps = GetComponent<ParticleSystem>();

            if (TryGetComponent<ParticleSystemRenderer>(out ParticleSystemRenderer psRenderer))
            {
                int entitiesLayerId = SortingLayer.NameToID("Entities");
                if (entitiesLayerId != 0)
                {
                    psRenderer.sortingLayerID = entitiesLayerId;
                }
                psRenderer.sortingOrder = sortingOrder;
            }
        }

        public void Initialize(Color burstColor, float scaleMultiplier, Action<DeathBurstEffect> returnToPoolCallback)
        {
            _returnToPoolCallback = returnToPoolCallback;
            _timer = effectDuration;
            _isPlaying = true;

            transform.localScale = Vector3.one * Mathf.Clamp(scaleMultiplier, 0.7f, 2.5f);

            var main = _ps.main;
            main.startColor = burstColor;

            _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _ps.Play(true);
        }

        private void Update()
        {
            if (!_isPlaying) return;

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                Despawn();
            }
        }

        private void Despawn()
        {
            _isPlaying = false;
            _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

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