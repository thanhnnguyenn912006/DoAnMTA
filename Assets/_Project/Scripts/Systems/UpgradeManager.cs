using System;
using System.Collections.Generic;
using UnityEngine;
using SurvivorGame.Data;
using SurvivorGame.Player;
using SurvivorGame.Weapons;

namespace SurvivorGame.Systems
{
    /// <summary>
    /// Đại diện cho một lựa chọn hiển thị trên thẻ nâng cấp (Mở khóa vũ khí, Nâng cấp vũ khí hoặc Bị động).
    /// </summary>
    public struct UpgradeOption
    {
        public bool IsWeaponUpgrade;
        public bool IsNewWeaponUnlock;
        public bool IsWeaponEvolution;
        public WeaponBase TargetWeapon;
        public PassiveUpgradeData PassiveData;

        public string Title;
        public string LevelText;
        public string Description;
        public Sprite Icon;
    }

    /// <summary>
    /// Lắng nghe sự kiện lên cấp, tạo 3 lựa chọn nâng cấp ngẫu nhiên không trùng lặp và quản lý tạm dừng game.
    /// Hỗ trợ mở khóa các vũ khí đang ở trạng thái tắt (Inactive) bên trong WeaponsContainer.
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        [Header("Tham chiếu bắt buộc")]
        [SerializeField] private PlayerExperience playerExperience;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private Transform weaponsContainer;
        [SerializeField] private GameManager gameManager;

        [Header("Danh sách nâng cấp bị động khả dụng")]
        [SerializeField] private PassiveUpgradeData[] availablePassives;
        [SerializeField, Min(1)] private int choicesPerLevelUp = 3;

        private readonly List<UpgradeOption> _candidatePool = new List<UpgradeOption>();
        private readonly List<UpgradeOption> _currentChoices = new List<UpgradeOption>();

        private int _pendingLevelUps;
        private bool _isChoosingUpgrade;

        public bool IsChoosingUpgrade => _isChoosingUpgrade;

        // Sự kiện phát sang UpgradePanelUI, WeaponBarHUD và GameplayAudioBridge
        public event Action<IReadOnlyList<UpgradeOption>> OnUpgradeChoicesReady;
        public event Action<UpgradeOption> OnUpgradeSelected;
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

            if (gameManager == null)
            {
                gameManager = FindAnyObjectByType<GameManager>();
            }
        }

        private void OnEnable()
        {
            if (playerExperience != null)
            {
                playerExperience.OnLevelUp += HandleLevelUp;
            }

            if (gameManager == null)
            {
                gameManager = FindAnyObjectByType<GameManager>();
            }

            if (gameManager != null)
            {
                gameManager.OnGameStateChanged += HandleGameStateChanged;
            }
        }

        private void OnDisable()
        {
            if (playerExperience != null)
            {
                playerExperience.OnLevelUp -= HandleLevelUp;
            }

            if (gameManager != null)
            {
                gameManager.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void HandleGameStateChanged(GameState newState)
        {
            if (newState == GameState.GameOver || newState == GameState.Victory)
            {
                CancelAllPendingUpgrades();
            }
        }

        private bool IsGameEnded()
        {
            GameManager gm = gameManager != null ? gameManager : GameManager.Instance;
            return gm != null && (gm.CurrentState == GameState.GameOver || gm.CurrentState == GameState.Victory);
        }

        /// <summary>
        /// Hủy toàn bộ phiên chọn nâng cấp và các lượt lên cấp đang chờ khi trận đấu kết thúc.
        /// </summary>
        public void CancelAllPendingUpgrades()
        {
            _pendingLevelUps = 0;
            _currentChoices.Clear();

            if (_isChoosingUpgrade)
            {
                CloseUpgradeMenu();
            }
            else
            {
                OnUpgradeMenuClosed?.Invoke();
            }
        }

        private void HandleLevelUp(int newLevel)
        {
            if (IsGameEnded()) return;

            _pendingLevelUps++;

            if (!_isChoosingUpgrade)
            {
                OpenNextUpgradeChoice();
            }
        }

        private void OpenNextUpgradeChoice()
        {
            if (IsGameEnded())
            {
                CancelAllPendingUpgrades();
                return;
            }

            BuildCandidatePool();

            if (_candidatePool.Count == 0)
            {
                _pendingLevelUps = 0;
                CloseUpgradeMenu();
                return;
            }

            PickRandomChoices();

            if (OnUpgradeChoicesReady == null)
            {
#if UNITY_EDITOR
                Debug.Log($"[UpgradeManager] Sinh {_currentChoices.Count} lựa chọn: " +
                          $"{_currentChoices[0].Title} ({_currentChoices[0].LevelText})...");
#endif
                _pendingLevelUps = 0;
                return;
            }

            _isChoosingUpgrade = true;
            OnUpgradeChoicesReady.Invoke(_currentChoices);
        }

        /// <summary>
        /// Gọi từ nút bấm trên thẻ UI khi người chơi chọn 1 trong 3 nâng cấp.
        /// </summary>
        public void SelectUpgrade(int choiceIndex)
        {
            if (IsGameEnded())
            {
                CancelAllPendingUpgrades();
                return;
            }

            if (!_isChoosingUpgrade || choiceIndex < 0 || choiceIndex >= _currentChoices.Count)
            {
                return;
            }

            UpgradeOption chosen = _currentChoices[choiceIndex];

            if (chosen.IsWeaponUpgrade && chosen.TargetWeapon != null)
            {
                if (chosen.IsWeaponEvolution)
                {
                    chosen.TargetWeapon.Evolve();
#if UNITY_EDITOR
                    Debug.Log($"[UpgradeManager] Đã TIẾN HÓA vũ khí thành '{chosen.Title}'!");
#endif
                }
                else if (chosen.IsNewWeaponUnlock || !chosen.TargetWeapon.gameObject.activeSelf)
                {
                    // Mở khóa vũ khí mới ở Cấp 1 và đồng bộ ngay các chỉ số bị động đang có
                    chosen.TargetWeapon.gameObject.SetActive(true);
                    if (playerStats != null)
                    {
                        playerStats.ApplyMultipliersToWeapon(chosen.TargetWeapon);
                    }
#if UNITY_EDITOR
                    Debug.Log($"[UpgradeManager] Đã mở khóa vũ khí mới '{chosen.Title}' (Cấp 1)");
#endif
                }
                else
                {
                    chosen.TargetWeapon.LevelUp();
#if UNITY_EDITOR
                    Debug.Log($"[UpgradeManager] Đã nâng cấp vũ khí '{chosen.Title}' -> Cấp {chosen.TargetWeapon.CurrentLevel}");
#endif
                }
            }
            else if (!chosen.IsWeaponUpgrade && chosen.PassiveData != null && playerStats != null)
            {
                playerStats.ApplyPassiveUpgrade(chosen.PassiveData);
            }

            OnUpgradeSelected?.Invoke(chosen);

            _pendingLevelUps = Mathf.Max(0, _pendingLevelUps - 1);

            if (_pendingLevelUps > 0 && !IsGameEnded())
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
            OnUpgradeMenuClosed?.Invoke();
        }

        private void BuildCandidatePool()
        {
            _candidatePool.Clear();

            WeaponBase[] weapons = weaponsContainer != null
                ? weaponsContainer.GetComponentsInChildren<WeaponBase>(true)
                : null;

            // 1. Gom toàn bộ vũ khí (bao gồm cả vũ khí đang tắt để cho phép mở khóa khi lên cấp)
            if (weapons != null)
            {
                for (int i = 0; i < weapons.Length; i++)
                {
                    WeaponBase weapon = weapons[i];
                    if (weapon == null || weapon.Data == null) continue;

                    bool isUnlocked = weapon.gameObject.activeSelf;

                    if (!isUnlocked)
                    {
                        WeaponLevelStats lv1Stats = weapon.Data.GetStats(1);
                        _candidatePool.Add(new UpgradeOption
                        {
                            IsWeaponUpgrade = true,
                            IsNewWeaponUnlock = true,
                            TargetWeapon = weapon,
                            Title = weapon.Data.WeaponName,
                            LevelText = "VŨ KHÍ MỚI (Cấp 1)",
                            Description = AppendWeaponEvolutionHint(lv1Stats.upgradeDescription, weapon.Data),
                            Icon = weapon.Data.Icon
                        });
                        continue;
                    }

                    if (weapon.IsMaxLevel)
                    {
                        if (weapon.CanEvolve(playerStats))
                        {
                            _candidatePool.Add(new UpgradeOption
                            {
                                IsWeaponUpgrade = true,
                                IsNewWeaponUnlock = false,
                                IsWeaponEvolution = true,
                                TargetWeapon = weapon,
                                Title = weapon.Data.EvolvedWeaponName,
                                LevelText = "[TIẾN HÓA]",
                                Description = weapon.Data.EvolvedDescription,
                                Icon = weapon.Data.Icon
                            });
                        }
                        continue;
                    }

                    int nextLevel = weapon.CurrentLevel + 1;
                    WeaponLevelStats nextStats = weapon.Data.GetStats(nextLevel);

                    _candidatePool.Add(new UpgradeOption
                    {
                        IsWeaponUpgrade = true,
                        IsNewWeaponUnlock = false,
                        TargetWeapon = weapon,
                        Title = weapon.Data.WeaponName,
                        LevelText = $"Cấp {weapon.CurrentLevel} -> {nextLevel}",
                        Description = AppendWeaponEvolutionHint(nextStats.upgradeDescription, weapon.Data),
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
                        IsNewWeaponUnlock = false,
                        PassiveData = passive,
                        Title = passive.UpgradeName,
                        LevelText = currentLv == 0 ? "Mới (Cấp 1)" : $"Cấp {currentLv} -> {nextLv}",
                        Description = AppendPassiveEvolutionHint(passive.Description, passive, weapons),
                        Icon = passive.Icon
                    });
                }
            }
        }

        private static string AppendWeaponEvolutionHint(string baseDescription, WeaponData data)
        {
            if (data == null || !data.HasEvolution || data.EvolutionCatalyst == null)
            {
                return baseDescription;
            }

            return $"{baseDescription}\n<size=78%><color=#FACC15>Tiến hóa với: {data.EvolutionCatalyst.UpgradeName}</color></size>";
        }

        private static string AppendPassiveEvolutionHint(
            string baseDescription,
            PassiveUpgradeData passive,
            WeaponBase[] weapons)
        {
            if (passive == null || weapons == null) return baseDescription;

            string matchedWeapons = null;
            for (int i = 0; i < weapons.Length; i++)
            {
                WeaponBase weapon = weapons[i];
                if (weapon == null || !weapon.gameObject.activeSelf || weapon.IsEvolved) continue;

                WeaponData data = weapon.Data;
                if (data == null || !data.HasEvolution || data.EvolutionCatalyst != passive) continue;

                matchedWeapons = matchedWeapons == null
                    ? data.WeaponName
                    : $"{matchedWeapons}, {data.WeaponName}";
            }

            if (string.IsNullOrEmpty(matchedWeapons)) return baseDescription;

            return $"{baseDescription}\n<size=78%><color=#FACC15>Tiến hóa: {matchedWeapons}</color></size>";
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