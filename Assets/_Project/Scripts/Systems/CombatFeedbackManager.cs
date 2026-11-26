using UnityEngine;
using SurvivorGame.Enemies;

namespace SurvivorGame.Systems
{
    /// <summary>
    /// Lắng nghe các sự kiện chiến đấu (mất máu, quái chết) để điều phối Số sát thương, Hạt nổ và Rung Camera.
    /// </summary>
    public class CombatFeedbackManager : MonoBehaviour
    {
        [Header("Prefabs hiệu ứng")]
        [SerializeField] private DamagePopup damagePopupPrefab;
        [SerializeField] private DeathBurstEffect deathBurstPrefab;

        [Header("Cấu hình Object Pool")]
        [SerializeField, Min(0)] private int popupPrewarmCount = 50;
        [SerializeField, Min(0)] private int burstPrewarmCount = 30;

        [Header("Màu số sát thương")]
        [SerializeField] private Color normalDamageColor = Color.white;
        [SerializeField] private Color heavyDamageColor = new Color(1f, 0.85f, 0.2f, 1f);  // Vàng nổi bật (>= 30 dmg)
        [SerializeField] private Color playerDamageColor = new Color(0.97f, 0.32f, 0.32f, 1f); // Đỏ khi Player trúng đòn
        [SerializeField] private Color healPopupColor = new Color(0.29f, 0.87f, 0.50f, 1f);    // Xanh lá khi hồi máu
        [SerializeField, Min(1)] private int heavyDamageThreshold = 30;

        [Header("Cường độ rung Camera")]
        [SerializeField] private float playerHitShakeIntensity = 0.22f;
        [SerializeField] private float playerHitShakeDuration = 0.16f;
        [SerializeField] private float bossDeathShakeIntensity = 0.42f;
        [SerializeField] private float bossDeathShakeDuration = 0.35f;

        private ComponentPool<DamagePopup> _popupPool;
        private ComponentPool<DeathBurstEffect> _burstPool;

        private void Start()
        {
            if (PoolManager.Instance == null) return;

            if (damagePopupPrefab != null)
            {
                _popupPool = PoolManager.Instance.GetOrCreatePool(damagePopupPrefab, popupPrewarmCount);
            }

            if (deathBurstPrefab != null)
            {
                _burstPool = PoolManager.Instance.GetOrCreatePool(deathBurstPrefab, burstPrewarmCount);
            }
        }

        private void OnEnable()
        {
            Health.OnAnyDamaged += HandleAnyDamaged;
            Health.OnAnyHealed += HandleAnyHealed;
            EnemyController.OnAnyEnemyDied += HandleEnemyDied;
        }

        private void OnDisable()
        {
            Health.OnAnyDamaged -= HandleAnyDamaged;
            Health.OnAnyHealed -= HandleAnyHealed;
            EnemyController.OnAnyEnemyDied -= HandleEnemyDied;
        }

        private void HandleAnyHealed(Health targetHealth, int healedAmount)
        {
            if (targetHealth == null || healedAmount <= 0 || damagePopupPrefab == null) return;

            Vector3 spawnPos = targetHealth.transform.position + new Vector3(0f, 0.55f, 0f);
            if (_popupPool != null)
            {
                DamagePopup popup = _popupPool.Get(spawnPos, Quaternion.identity);
                popup.InitializeHeal(healedAmount, healPopupColor, 1.1f, _popupPool.ReleaseCallback);
            }
            else
            {
                DamagePopup popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);
                popup.InitializeHeal(healedAmount, healPopupColor, 1.1f, null);
            }
        }

        private void HandleAnyDamaged(Health targetHealth, int damageAmount)
        {
            if (targetHealth == null || damageAmount <= 0) return;

            bool isPlayer = targetHealth.CompareTag("Player");

            SpawnDamagePopup(targetHealth.transform.position, damageAmount, isPlayer);

            if (isPlayer && CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(playerHitShakeIntensity, playerHitShakeDuration);
            }
        }

        private void HandleEnemyDied(EnemyController deadEnemy)
        {
            if (deadEnemy == null) return;

            Color burstColor = deadEnemy.Data != null ? deadEnemy.Data.TintColor : Color.red;
            float scale = deadEnemy.Data != null ? deadEnemy.Data.Scale : 1f;

            SpawnDeathBurst(deadEnemy.transform.position, burstColor, scale);

            // Rung mạnh tạo cảm giác đã tay khi hạ gục Boss
            if (scale >= 1.8f && CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(bossDeathShakeIntensity, bossDeathShakeDuration);
            }
        }

        private void SpawnDamagePopup(Vector3 worldPos, int damageAmount, bool isPlayer)
        {
            if (damagePopupPrefab == null) return;

            Vector3 spawnPos = worldPos + new Vector3(Random.Range(-0.22f, 0.22f), 0.45f, 0f);
            Color color = isPlayer
                ? playerDamageColor
                : (damageAmount >= heavyDamageThreshold ? heavyDamageColor : normalDamageColor);
            float scale = isPlayer ? 1.15f : (damageAmount >= heavyDamageThreshold ? 1.15f : 0.95f);

            if (_popupPool != null)
            {
                DamagePopup popup = _popupPool.Get(spawnPos, Quaternion.identity);
                popup.Initialize(damageAmount, color, isPlayer, scale, _popupPool.ReleaseCallback);
            }
            else
            {
                DamagePopup popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);
                popup.Initialize(damageAmount, color, isPlayer, scale, null);
            }
        }

        private void SpawnDeathBurst(Vector3 worldPos, Color burstColor, float scale)
        {
            if (deathBurstPrefab == null) return;

            if (_burstPool != null)
            {
                DeathBurstEffect effect = _burstPool.Get(worldPos, Quaternion.identity);
                effect.Initialize(burstColor, scale, _burstPool.ReleaseCallback);
            }
            else
            {
                DeathBurstEffect effect = Instantiate(deathBurstPrefab, worldPos, Quaternion.identity);
                effect.Initialize(burstColor, scale, null);
            }
        }
    }
}