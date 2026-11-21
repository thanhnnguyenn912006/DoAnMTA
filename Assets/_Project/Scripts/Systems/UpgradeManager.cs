using System;
using System.Collections.Generic;
using UnityEngine;
using SurvivorGame.Data;
using SurvivorGame.Player;
using SurvivorGame.Weapons;

namespace SurvivorGame.Systems
{
    /// <summary>
    /// Đại diện cho một lựa chọn hiển thị trên thẻ nâng cấp (có thể là Vũ khí hoặc Bị động).
    /// </summary>
    public struct UpgradeOption
    {
        public bool IsWeaponUpgrade;
        public WeaponBase TargetWeapon;
        public PassiveUpgradeData PassiveData;

        public string Title;
        public string LevelText;
        public string Description;
        public Sprite Icon;
    }

    /// <summary>
    /// Lắng nghe sự kiện lên cấp, tạo 3 lựa chọn nâng cấp ngẫu nhiên không trùng lặp và quản lý tạm dừng game.
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        [Header("Tham chiếu bắt buộc")]
        [SerializeField] private PlayerExperience playerExperience;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private Transform weaponsContainer;

        [Header("Danh sách nâng cấp bị động khả dụng")]
        [SerializeField] private PassiveUpgradeData[] availablePassives;
        [SerializeField, Min(1)] private int choicesPerLevelUp = 3;

        private readonly List<UpgradeOption> _candidatePool = new List<UpgradeOption>();
        private readonly List<UpgradeOption> _currentChoices = new List<UpgradeOption>();

        private int _pendingLevelUps;
        private bool _isChoosingUpgrade;

        public bool IsChoosingUpgrade => _isChoosingUpgrade;

        // Sự kiện phát sang UpgradePanelUI
        public event Action<IReadOnlyList<UpgradeOption>> OnUpgradeChoicesReady;
        public event Action OnUpgradeMenuClosed;

        private void Awake()
        {
            if (playerExperience == null || playerStats == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    if (playerExperience == null) playerExperience = playerObj.GetComponent<PlayerExperience>();
                    if (playerStats == null) playerStats = playerObj.GetComponent<PlayerStats>();
                    if (weaponsContainer == null) weaponsContainer = playerObj.transform;
                }
            }
        }

        private void OnEnable()
        {
            if (playerExperience != null)
            {
                playerExperience.OnLevelUp += HandleLevelUp;
            }
        }

        private void OnDisable()
        {
            if (playerExperience != null)
            {
                playerExperience.OnLevelUp -= HandleLevelUp;
            }
        }

        private void OnDestroy()
        {
            // Đảm bảo không bao giờ để kẹt Time.timeScale = 0 khi đổi Scene hoặc tắt Play
            if (_isChoosingUpgrade)
            {
                Time.timeScale = 1f;
            }
        }

        private void HandleLevelUp(int newLevel)
        {
            _pendingLevelUps++;

            if (!_isChoosingUpgrade)
            {
                OpenNextUpgradeChoice();
            }
        }

        private void OpenNextUpgradeChoice()
        {
            BuildCandidatePool();

            if (_candidatePool.Count == 0)
            {
                // Đã nâng tối đa toàn bộ vũ khí và bị động -> không chặn game
                _pendingLevelUps = 0;
                CloseUpgradeMenu();
                return;
            }

            PickRandomChoices();

            // Nếu chưa gắn UI (đang test Phần 2/3), chỉ in ra Console để không làm đứng game
            if (OnUpgradeChoicesReady == null)
            {
                Debug.Log($"[UpgradeManager] Sinh {_currentChoices.Count} lựa chọn: " +
                          $"{_currentChoices[0].Title} ({_currentChoices[0].LevelText})...");
                _pendingLevelUps = 0;
                return;
            }

            _isChoosingUpgrade = true;
            Time.timeScale = 0f;
            OnUpgradeChoicesReady.Invoke(_currentChoices);
        }

        /// <summary>
        /// Gọi từ nút bấm trên thẻ UI khi người chơi chọn 1 trong 3 nâng cấp.
        /// </summary>
        public void SelectUpgrade(int choiceIndex)
        {
            if (!_isChoosingUpgrade || choiceIndex < 0 || choiceIndex >= _currentChoices.Count)
            {
                return;
            }

            UpgradeOption chosen = _currentChoices[choiceIndex];

            if (chosen.IsWeaponUpgrade && chosen.TargetWeapon != null)
            {
                chosen.TargetWeapon.LevelUp();
                Debug.Log($"[UpgradeManager] Đã nâng cấp vũ khí '{chosen.Title}' -> Cấp {chosen.TargetWeapon.CurrentLevel}");
            }
            else if (!chosen.IsWeaponUpgrade && chosen.PassiveData != null && playerStats != null)
            {
                playerStats.ApplyPassiveUpgrade(chosen.PassiveData);
            }

            _pendingLevelUps = Mathf.Max(0, _pendingLevelUps - 1);

            if (_pendingLevelUps > 0)
            {
                OpenNextUpgradeChoice();
            }
            else
            {
                CloseUpgradeMenu();
            }
        }

        private void CloseUpgradeMenu()
        {
            _isChoosingUpgrade = false;
            Time.timeScale = 1f;
            OnUpgradeMenuClosed?.Invoke();
        }

        private void BuildCandidatePool()
        {
            _candidatePool.Clear();

            // 1. Gom các vũ khí đang sở hữu chưa đạt cấp tối đa
            if (weaponsContainer != null)
            {
                WeaponBase[] weapons = weaponsContainer.GetComponentsInChildren<WeaponBase>();
                for (int i = 0; i < weapons.Length; i++)
                {
                    WeaponBase weapon = weapons[i];
                    if (weapon == null || weapon.Data == null || weapon.IsMaxLevel) continue;

                    int nextLevel = weapon.CurrentLevel + 1;
                    WeaponLevelStats nextStats = weapon.Data.GetStats(nextLevel);

                    _candidatePool.Add(new UpgradeOption
                    {
                        IsWeaponUpgrade = true,
                        TargetWeapon = weapon,
                        Title = weapon.Data.WeaponName,
                        LevelText = $"Cap {weapon.CurrentLevel} -> {nextLevel}",
                        Description = nextStats.upgradeDescription,
                        Icon = weapon.Data.Icon
                    });
                }
            }

            // 2. Gom các nâng cấp bị động chưa đạt cấp tối đa
            if (availablePassives != null && playerStats != null)
            {
                for (int i = 0; i < availablePassives.Length; i++)
                {
                    PassiveUpgradeData passive = availablePassives[i];
                    if (passive == null || playerStats.IsPassiveMaxed(passive)) continue;

                    int currentLv = playerStats.GetPassiveLevel(passive);
                    int nextLv = currentLv + 1;

                    _candidatePool.Add(new UpgradeOption
                    {
                        IsWeaponUpgrade = false,
                        PassiveData = passive,
                        Title = passive.UpgradeName,
                        LevelText = currentLv == 0 ? "Moi (Cap 1)" : $"Cap {currentLv} -> {nextLv}",
                        Description = passive.Description,
                        Icon = passive.Icon
                    });
                }
            }
        }

        /// <summary>
        /// Trộn ngẫu nhiên Fisher-Yates và lấy tối đa choicesPerLevelUp lựa chọn không trùng.
        /// </summary>
        private void PickRandomChoices()
        {
            _currentChoices.Clear();

            for (int i = _candidatePool.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                (_candidatePool[i], _candidatePool[randomIndex]) = (_candidatePool[randomIndex], _candidatePool[i]);
            }

            int count = Mathf.Min(choicesPerLevelUp, _candidatePool.Count);
            for (int i = 0; i < count; i++)
            {
                _currentChoices.Add(_candidatePool[i]);
            }
        }
    }
}