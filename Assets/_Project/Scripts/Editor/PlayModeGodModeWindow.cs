using System.Reflection;
using UnityEditor;
using UnityEngine;
using SurvivorGame.Data;
using SurvivorGame.Enemies;
using SurvivorGame.Player;
using SurvivorGame.Systems;
using SurvivorGame.UI;
using SurvivorGame.Weapons;

namespace SurvivorGame.EditorTools
{
    /// <summary>
    /// Cửa sổ God Mode & công cụ test nhanh CHỈ tồn tại trong Unity Editor (không bao giờ biên dịch vào bản Build).
    /// Mở tại: Tools > Survivor Arena > God Mode (Chi Test Trong Editor).
    /// </summary>
    public class PlayModeGodModeWindow : EditorWindow
    {
        private bool _godModeEnabled;

        [MenuItem("Tools/Survivor Arena/God Mode (Chi Test Trong Editor)")]
        public static void OpenWindow()
        {
            PlayModeGodModeWindow window = GetWindow<PlayModeGodModeWindow>("God Mode Test");
            window.minSize = new Vector2(270f, 360f);
            window.Show();
        }

        private void OnEnable()
        {
            EditorApplication.update += SyncGodModeState;
        }

        private void OnDisable()
        {
            EditorApplication.update -= SyncGodModeState;
            SetPlayerGodMode(false);
        }

        private void SyncGodModeState()
        {
            if (!Application.isPlaying) return;

            Health playerHealth = FindPlayerComponent<Health>();
            if (playerHealth != null && playerHealth.EditorGodMode != _godModeEnabled)
            {
                playerHealth.EditorGodMode = _godModeEnabled;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Chỉ chạy trong Play Mode của Unity Editor (thuộc assembly Editor, tự động loại khỏi bản Build).",
                MessageType.Info);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Hãy bấm Play trong Unity để dùng các nút test bên dưới.", MessageType.Warning);
                _godModeEnabled = EditorGUILayout.ToggleLeft("Bật sẵn Bất tử (God Mode) khi vào Play", _godModeEnabled);
                return;
            }

            Health playerHealth = FindPlayerComponent<Health>();
            PlayerExperience playerExp = FindPlayerComponent<PlayerExperience>();
            PlayerStats playerStats = FindPlayerComponent<PlayerStats>();
            EnemySpawner enemySpawner = Object.FindAnyObjectByType<EnemySpawner>();
            BossSpawner bossSpawner = Object.FindAnyObjectByType<BossSpawner>();
            ExpDropManager dropManager = Object.FindAnyObjectByType<ExpDropManager>();
            UpgradeManager upgradeManager = Object.FindAnyObjectByType<UpgradeManager>();

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Sinh tồn & Nhân vật", EditorStyles.boldLabel);

            bool nextGodMode = EditorGUILayout.ToggleLeft("Bất tử (God Mode - 0 sát thương)", _godModeEnabled);
            if (nextGodMode != _godModeEnabled)
            {
                _godModeEnabled = nextGodMode;
                SetPlayerGodMode(_godModeEnabled);
            }

            using (new EditorGUI.DisabledScope(playerHealth == null))
            {
                if (GUILayout.Button("Hồi đầy máu (Full HP)"))
                {
                    playerHealth.Heal(playerHealth.MaxHealth);
                }
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Cấp độ, Vũ khí & Tiến hóa", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(playerExp == null))
            {
                if (GUILayout.Button("+1 Cấp ngay (Level Up)"))
                {
                    playerExp.AddExp(playerExp.ExpToNextLevel);
                }
            }

            using (new EditorGUI.DisabledScope(playerStats == null))
            {
                if (GUILayout.Button("Mở & Max 3 Vũ khí (Cấp 5)"))
                {
                    UnlockAndMaxAllWeapons(playerStats);
                }

                if (GUILayout.Button("+1 Cấp cho toàn bộ 5 Bị động"))
                {
                    GrantOneLevelAllPassives(upgradeManager, playerStats);
                }
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Thời gian, Elite & Boss", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(enemySpawner == null))
            {
                if (GUILayout.Button("Tua nhanh +25 giây"))
                {
                    SetSpawnerElapsedTime(enemySpawner, enemySpawner.ElapsedTime + 25f);
                }

                if (GUILayout.Button("Nhảy tới 48s (Sắp ra Quái Tinh Anh 1)"))
                {
                    SetSpawnerElapsedTime(enemySpawner, 48f);
                }

                if (GUILayout.Button("Nhảy tới 138s (Sắp cảnh báo Boss)"))
                {
                    SetSpawnerElapsedTime(enemySpawner, 138f);
                }
            }

            using (new EditorGUI.DisabledScope(bossSpawner == null))
            {
                if (GUILayout.Button("Triệu hồi Boss ngay"))
                {
                    bossSpawner.SpawnBoss();
                }
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Thả vật phẩm gần Player", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(dropManager == null || playerExp == null))
            {
                if (GUILayout.Button("Thả Bình Máu"))
                {
                    InvokePrivate(dropManager, "TestSpawnHealPotion");
                }

                if (GUILayout.Button("Thả Nam Châm Lớn"))
                {
                    InvokePrivate(dropManager, "TestSpawnMapMagnet");
                }

                if (GUILayout.Button("Thả Rương Báu (Elite)"))
                {
                    SpawnChestNearPlayer(dropManager, playerExp);
                }
            }
        }

        private static void SetPlayerGodMode(bool enabled)
        {
            if (!Application.isPlaying) return;
            Health health = FindPlayerComponent<Health>();
            if (health != null)
            {
                health.EditorGodMode = enabled;
            }
        }

        private static void UnlockAndMaxAllWeapons(PlayerStats playerStats)
        {
            WeaponBase[] weapons = playerStats.GetComponentsInChildren<WeaponBase>(true);
            for (int i = 0; i < weapons.Length; i++)
            {
                WeaponBase w = weapons[i];
                if (w == null) continue;
                if (!w.gameObject.activeSelf) w.gameObject.SetActive(true);
                playerStats.ApplyMultipliersToWeapon(w);
                while (!w.IsMaxLevel) w.LevelUp();
            }

            WeaponBarHUD hud = Object.FindAnyObjectByType<WeaponBarHUD>();
            hud?.Refresh();
        }

        private static void GrantOneLevelAllPassives(UpgradeManager upgradeManager, PlayerStats playerStats)
        {
            if (upgradeManager == null) return;

            FieldInfo field = typeof(UpgradeManager).GetField("availablePassives", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field?.GetValue(upgradeManager) is PassiveUpgradeData[] passives)
            {
                for (int i = 0; i < passives.Length; i++)
                {
                    if (passives[i] != null)
                    {
                        playerStats.ApplyPassiveUpgrade(passives[i]);
                    }
                }
            }

            WeaponBarHUD hud = Object.FindAnyObjectByType<WeaponBarHUD>();
            hud?.Refresh();
        }

        private static void SetSpawnerElapsedTime(EnemySpawner spawner, float seconds)
        {
            FieldInfo field = typeof(EnemySpawner).GetField("elapsedTime", BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(spawner, Mathf.Max(0f, seconds));
        }

        private static void SpawnChestNearPlayer(ExpDropManager dropManager, PlayerExperience playerExp)
        {
            FieldInfo field = typeof(ExpDropManager).GetField("treasureChestPrefab", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field?.GetValue(dropManager) is TreasureChest chestPrefab && chestPrefab != null)
            {
                Vector3 pos = playerExp.transform.position + new Vector3(0f, 2f, 0f);
                TreasureChest chest = Object.Instantiate(chestPrefab, pos, Quaternion.identity);
                chest.Initialize(playerExp);
            }
        }

        private static void InvokePrivate(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            method?.Invoke(target, null);
        }

        private static T FindPlayerComponent<T>() where T : Component
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            return playerObj != null ? playerObj.GetComponent<T>() : null;
        }
    }
}
