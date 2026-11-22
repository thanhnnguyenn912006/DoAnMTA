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
        private int _damage = 10;
        private float _hitCooldown = 0.35f;
        private LayerMask _enemyLayer;

        // Lưu thời điểm gây sát thương gần nhất lên từng quái (tối đa ~150 quái từ Pool)
        private readonly Dictionary<Health, float> _lastHitTimes = new Dictionary<Health, float>();

        public void Configure(int damage, float hitCooldown, LayerMask enemyLayer)
        {
            _damage = Mathf.Max(1, damage);
            _hitCooldown = Mathf.Max(0.05f, hitCooldown);
            _enemyLayer = enemyLayer;
        }

        private void OnDisable()
        {
            _lastHitTimes.Clear();
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
            enemyHealth.TakeDamage(_damage);
        }
    }
}