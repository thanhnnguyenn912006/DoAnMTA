using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SurvivorGame.Enemies;
using SurvivorGame.Systems;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Hiển thị tên và thanh máu Boss khi Boss xuất hiện, tự ẩn khi Boss bị tiêu diệt.
    /// Gắn script này lên đối tượng luôn bật (Canvas), KHÔNG gắn lên Panel bị tắt sẵn.
    /// </summary>
    public class BossHealthBarUI : MonoBehaviour
    {
        [Header("Tham chiếu hệ thống")]
        [SerializeField] private BossSpawner bossSpawner;

        [Header("Thành phần UI")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TMP_Text bossNameText;
        [SerializeField] private TMP_Text healthValueText;

        private Health _currentBossHealth;
        private int _lastDisplayedHp = -1;
        private int _lastDisplayedMaxHp = -1;

        private void Awake()
        {
            if (bossSpawner == null)
            {
                bossSpawner = FindAnyObjectByType<BossSpawner>();
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (bossSpawner != null)
            {
                bossSpawner.OnBossSpawned += HandleBossSpawned;
                bossSpawner.OnBossDefeated += HandleBossDefeated;
            }
        }

        private void OnDisable()
        {
            if (bossSpawner != null)
            {
                bossSpawner.OnBossSpawned -= HandleBossSpawned;
                bossSpawner.OnBossDefeated -= HandleBossDefeated;
            }
        }

        private void Update()
        {
            if (_currentBossHealth == null) return;

            int currentHp = Mathf.Max(0, _currentBossHealth.CurrentHealth);
            int maxHp = Mathf.Max(1, _currentBossHealth.MaxHealth);

            if (currentHp != _lastDisplayedHp || maxHp != _lastDisplayedMaxHp)
            {
                RefreshHealthVisual(currentHp, maxHp);
            }
        }

        private void HandleBossSpawned(EnemyController boss, Health bossHealth)
        {
            _currentBossHealth = bossHealth;
            _lastDisplayedHp = -1;
            _lastDisplayedMaxHp = -1;

            if (bossNameText != null && boss != null && boss.Data != null)
            {
                bossNameText.text = boss.Data.EnemyName;
            }

            if (_currentBossHealth != null)
            {
                RefreshHealthVisual(_currentBossHealth.CurrentHealth, _currentBossHealth.MaxHealth);
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
        }

        private void HandleBossDefeated(EnemyController boss)
        {
            _currentBossHealth = null;

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void RefreshHealthVisual(int currentHp, int maxHp)
        {
            _lastDisplayedHp = currentHp;
            _lastDisplayedMaxHp = maxHp;

            if (healthSlider != null)
            {
                healthSlider.minValue = 0f;
                healthSlider.maxValue = 1f;
                healthSlider.value = (float)currentHp / maxHp;
            }

            if (healthValueText != null)
            {
                healthValueText.text = $"{currentHp} / {maxHp}";
            }
        }
    }
}