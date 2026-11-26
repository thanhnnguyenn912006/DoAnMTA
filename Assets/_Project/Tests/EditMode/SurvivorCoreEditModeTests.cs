using System.Reflection;
using NUnit.Framework;
using SurvivorGame.Data;
using SurvivorGame.Player;
using SurvivorGame.Systems;
using SurvivorGame.Weapons;
using UnityEditor;
using UnityEngine;

namespace SurvivorGame.Tests.EditMode
{
    public class SurvivorCoreEditModeTests
    {
        private sealed class DummyWeapon : WeaponBase
        {
            protected override bool Attack() => false;
        }

        private static void InvokeLifecycle(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            method?.Invoke(target, null);
        }

        [Test]
        public void PlayerExperience_AddExpAcrossMultipleLevels_IncrementsLevelAndKeepsRemainder()
        {
            GameObject go = new GameObject("PlayerExpTest");
            try
            {
                PlayerExperience exp = go.AddComponent<PlayerExperience>();
                InvokeLifecycle(exp, "Awake");

                int lv1Required = exp.ExpToNextLevel;
                Assert.Greater(lv1Required, 0);

                // Cộng dư đủ để lên ít nhất 2 cấp
                exp.AddExp(lv1Required + 45 + 10);

                Assert.GreaterOrEqual(exp.CurrentLevel, 2);
                Assert.GreaterOrEqual(exp.CurrentExp, 0);
                Assert.Less(exp.CurrentExp, exp.ExpToNextLevel);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GameManager_FormatTime_FormatsMinutesAndSecondsWithLeadingZeros()
        {
            Assert.AreEqual("00:00", GameManager.FormatTime(0f));
            Assert.AreEqual("00:09", GameManager.FormatTime(9.8f));
            Assert.AreEqual("02:30", GameManager.FormatTime(150f));
        }

        [Test]
        public void PlayerExperience_IncreaseMagnetRadius_AddsPositiveBonus()
        {
            GameObject go = new GameObject("PlayerMagnetTest");
            try
            {
                PlayerExperience exp = go.AddComponent<PlayerExperience>();
                InvokeLifecycle(exp, "Awake");

                float initialRadius = exp.MagnetRadius;
                exp.IncreaseMagnetRadius(1.5f);

                Assert.AreEqual(initialRadius + 1.5f, exp.MagnetRadius, 0.0001f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Health_GrantInvincibility_ExtendsWithoutShorteningExistingTimer()
        {
            GameObject go = new GameObject("HealthInvincibilityTest");
            try
            {
                Health health = go.AddComponent<Health>();
                InvokeLifecycle(health, "Awake");
                health.Initialize(100, 0.6f);

                int damagedEventCount = 0;
                health.OnDamaged += _ => damagedEventCount++;

                // Trúng đòn tạo i-frame 0.6s và phát sự kiện OnDamaged đúng 1 lần
                health.TakeDamage(10);
                Assert.AreEqual(90, health.CurrentHealth);
                Assert.AreEqual(1, damagedEventCount);
                Assert.IsTrue(health.IsInvincible);

                FieldInfo timerField = typeof(Health).GetField(
                    "_invincibilityTimer",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(timerField);

                // Grant ngắn hơn (0.18s) không được rút ngắn 0.6s sẵn có và không phát OnDamaged
                health.GrantInvincibility(0.18f);
                Assert.AreEqual(1, damagedEventCount);
                Assert.IsTrue(health.IsInvincible);
                Assert.AreEqual(0.6f, (float)timerField.GetValue(health), 0.0001f);

                // Grant dài hơn (1.1s) mở rộng timer mà vẫn không phát OnDamaged
                health.GrantInvincibility(1.1f);
                Assert.AreEqual(1, damagedEventCount);
                Assert.AreEqual(1.1f, (float)timerField.GetValue(health), 0.0001f);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void WeaponBase_CanEvolveAndEvolve_RequiresMaxLevelAndCatalyst()
        {
            GameObject playerObj = new GameObject("PlayerEvolutionTest");
            PassiveUpgradeData catalyst = ScriptableObject.CreateInstance<PassiveUpgradeData>();
            WeaponData weaponData = ScriptableObject.CreateInstance<WeaponData>();
            try
            {
                SerializedObject passiveSo = new SerializedObject(catalyst);
                passiveSo.FindProperty("upgradeId").stringValue = "test_catalyst";
                passiveSo.FindProperty("upgradeName").stringValue = "Xúc tác";
                passiveSo.FindProperty("upgradeType").enumValueIndex = (int)PassiveUpgradeType.DamagePercent;
                passiveSo.FindProperty("maxLevel").intValue = 5;
                passiveSo.FindProperty("valuePerLevel").floatValue = 0.15f;
                passiveSo.ApplyModifiedPropertiesWithoutUndo();

                SerializedObject weaponSo = new SerializedObject(weaponData);
                weaponSo.FindProperty("weaponName").stringValue = "Test Gun";
                SerializedProperty levelsProp = weaponSo.FindProperty("levels");
                levelsProp.arraySize = 5;
                for (int i = 0; i < 5; i++)
                {
                    SerializedProperty lv = levelsProp.GetArrayElementAtIndex(i);
                    lv.FindPropertyRelative("damage").intValue = 10 + i * 5;
                    lv.FindPropertyRelative("cooldown").floatValue = 1f;
                    lv.FindPropertyRelative("range").floatValue = 5f;
                    lv.FindPropertyRelative("projectileSpeed").floatValue = 10f;
                    lv.FindPropertyRelative("projectileCount").intValue = 1;
                }
                weaponSo.FindProperty("hasEvolution").boolValue = true;
                weaponSo.FindProperty("evolutionCatalyst").objectReferenceValue = catalyst;
                weaponSo.FindProperty("evolvedWeaponName").stringValue = "Test Gun EVO";
                SerializedProperty evoStats = weaponSo.FindProperty("evolvedStats");
                evoStats.FindPropertyRelative("damage").intValue = 99;
                evoStats.FindPropertyRelative("cooldown").floatValue = 0.25f;
                evoStats.FindPropertyRelative("range").floatValue = 10f;
                evoStats.FindPropertyRelative("projectileSpeed").floatValue = 20f;
                evoStats.FindPropertyRelative("projectileCount").intValue = 3;
                weaponSo.ApplyModifiedPropertiesWithoutUndo();

                Health health = playerObj.AddComponent<Health>();
                PlayerExperience exp = playerObj.AddComponent<PlayerExperience>();
                PlayerStats stats = playerObj.AddComponent<PlayerStats>();
                InvokeLifecycle(health, "Awake");
                InvokeLifecycle(exp, "Awake");
                InvokeLifecycle(stats, "Awake");

                DummyWeapon weapon = playerObj.AddComponent<DummyWeapon>();
                weapon.Initialize(weaponData, 1);

                // Chưa đạt Cấp 5 & chưa có bị động
                Assert.IsFalse(weapon.CanEvolve(stats));
                Assert.IsFalse(weapon.Evolve());

                // Lên Cấp 4 và có bị động Cấp 1 -> vẫn chưa đủ Cấp 5
                while (weapon.CurrentLevel < 4) weapon.LevelUp();
                Assert.IsTrue(stats.ApplyPassiveUpgrade(catalyst));
                Assert.IsFalse(weapon.CanEvolve(stats));
                Assert.IsFalse(weapon.Evolve());

                // Cấp 5 nhưng kiểm tra với PlayerStats mới chưa có bị động -> vẫn false
                weapon.LevelUp();
                Assert.IsTrue(weapon.IsMaxLevel);

                GameObject emptyPlayer = new GameObject("EmptyPlayerStatsTest");
                try
                {
                    Health h2 = emptyPlayer.AddComponent<Health>();
                    PlayerExperience e2 = emptyPlayer.AddComponent<PlayerExperience>();
                    PlayerStats emptyStats = emptyPlayer.AddComponent<PlayerStats>();
                    InvokeLifecycle(h2, "Awake");
                    InvokeLifecycle(e2, "Awake");
                    InvokeLifecycle(emptyStats, "Awake");
                    Assert.IsFalse(weapon.CanEvolve(emptyStats));
                }
                finally
                {
                    Object.DestroyImmediate(emptyPlayer);
                }

                // Đủ Cấp 5 + bị động Cấp 1 -> tiến hóa thành công đúng 1 lần duy nhất
                Assert.IsTrue(weapon.CanEvolve(stats));
                Assert.IsTrue(weapon.Evolve());
                Assert.IsTrue(weapon.IsEvolved);
                Assert.AreEqual(99, weapon.CurrentStats.damage);

                Assert.IsFalse(weapon.CanEvolve(stats));
                Assert.IsFalse(weapon.Evolve());
            }
            finally
            {
                Object.DestroyImmediate(playerObj);
                Object.DestroyImmediate(weaponData);
                Object.DestroyImmediate(catalyst);
            }
        }

        [Test]
        public void PlayerStats_ApplyMoveSpeedPassive_IncreasesMovementSpeed()
        {
            GameObject playerObj = new GameObject("PlayerMoveSpeedTest");
            PassiveUpgradeData speedPassive = ScriptableObject.CreateInstance<PassiveUpgradeData>();
            try
            {
                SerializedObject passiveSo = new SerializedObject(speedPassive);
                passiveSo.FindProperty("upgradeId").stringValue = "passive_speed";
                passiveSo.FindProperty("upgradeName").stringValue = "Tốc chạy";
                passiveSo.FindProperty("upgradeType").enumValueIndex = (int)PassiveUpgradeType.MoveSpeed;
                passiveSo.FindProperty("maxLevel").intValue = 5;
                passiveSo.FindProperty("valuePerLevel").floatValue = 0.5f;
                passiveSo.ApplyModifiedPropertiesWithoutUndo();

                playerObj.AddComponent<Rigidbody2D>();
                PlayerMovement movement = playerObj.AddComponent<PlayerMovement>();
                Health health = playerObj.AddComponent<Health>();
                PlayerExperience exp = playerObj.AddComponent<PlayerExperience>();
                PlayerStats stats = playerObj.AddComponent<PlayerStats>();

                InvokeLifecycle(movement, "Awake");
                InvokeLifecycle(health, "Awake");
                InvokeLifecycle(exp, "Awake");
                InvokeLifecycle(stats, "Awake");

                float initialSpeed = movement.MoveSpeed;
                Assert.IsTrue(stats.ApplyPassiveUpgrade(speedPassive));
                Assert.AreEqual(1, stats.GetPassiveLevel(speedPassive));
                Assert.AreEqual(initialSpeed + 0.5f, movement.MoveSpeed, 0.0001f);
            }
            finally
            {
                Object.DestroyImmediate(playerObj);
                Object.DestroyImmediate(speedPassive);
            }
        }

        [Test]
        public void ExpGem_MagnetizeAllActive_PullsActiveGemsZeroAlloc()
        {
            GameObject collectorObj = new GameObject("CollectorTest");
            GameObject gemObj1 = new GameObject("ExpGemActive1");
            GameObject gemObj2 = new GameObject("ExpGemActive2");
            GameObject gemObjInactive = new GameObject("ExpGemInactive");
            ExpGem gem1 = null;
            ExpGem gem2 = null;
            ExpGem gemInactive = null;
            try
            {
                PlayerExperience collector = collectorObj.AddComponent<PlayerExperience>();
                InvokeLifecycle(collector, "Awake");

                gem1 = gemObj1.AddComponent<ExpGem>();
                gem2 = gemObj2.AddComponent<ExpGem>();
                gemInactive = gemObjInactive.AddComponent<ExpGem>();

                InvokeLifecycle(gem1, "Awake");
                InvokeLifecycle(gem2, "Awake");
                InvokeLifecycle(gemInactive, "Awake");

                gem1.Initialize(10, null, null);
                gem2.Initialize(20, null, null);
                gemInactive.Initialize(15, null, null);

                InvokeLifecycle(gem1, "OnEnable");
                InvokeLifecycle(gem2, "OnEnable");
                InvokeLifecycle(gemInactive, "OnEnable");
                gemObjInactive.SetActive(false);

                Assert.IsFalse(gem1.IsMagnetized);
                Assert.IsFalse(gem2.IsMagnetized);
                Assert.IsFalse(gemInactive.IsMagnetized);

                int pulledCount = ExpGem.MagnetizeAllActive(collector);

                Assert.AreEqual(2, pulledCount);
                Assert.IsTrue(gem1.IsMagnetized);
                Assert.IsTrue(gem2.IsMagnetized);
                Assert.IsFalse(gemInactive.IsMagnetized);
            }
            finally
            {
                if (gem1 != null) InvokeLifecycle(gem1, "OnDisable");
                if (gem2 != null) InvokeLifecycle(gem2, "OnDisable");
                if (gemInactive != null) InvokeLifecycle(gemInactive, "OnDisable");

                Object.DestroyImmediate(gemObj1);
                Object.DestroyImmediate(gemObj2);
                Object.DestroyImmediate(gemObjInactive);
                Object.DestroyImmediate(collectorObj);
            }
        }

        [Test]
        public void UpgradeManager_BuildCandidatePool_ShowsEvolutionHintOnlyForOwnedUnevolvedWeapons()
        {
            GameObject playerObj = new GameObject("PlayerHintTest");
            GameObject weaponObj = new GameObject("OwnedWeapon");
            GameObject managerObj = new GameObject("UpgradeManagerHintTest");
            PassiveUpgradeData catalyst = ScriptableObject.CreateInstance<PassiveUpgradeData>();
            WeaponData weaponData = ScriptableObject.CreateInstance<WeaponData>();
            try
            {
                weaponObj.transform.SetParent(playerObj.transform);

                SerializedObject passiveSo = new SerializedObject(catalyst);
                passiveSo.FindProperty("upgradeId").stringValue = "p_cd";
                passiveSo.FindProperty("upgradeName").stringValue = "Tốc độ đánh";
                passiveSo.FindProperty("description").stringValue = "Giảm hồi chiêu";
                passiveSo.FindProperty("upgradeType").enumValueIndex = (int)PassiveUpgradeType.CooldownReduction;
                passiveSo.FindProperty("maxLevel").intValue = 5;
                passiveSo.FindProperty("valuePerLevel").floatValue = 0.1f;
                passiveSo.ApplyModifiedPropertiesWithoutUndo();

                SerializedObject weaponSo = new SerializedObject(weaponData);
                weaponSo.FindProperty("weaponName").stringValue = "Đạn Tự Nhắm";
                SerializedProperty levelsProp = weaponSo.FindProperty("levels");
                levelsProp.arraySize = 5;
                for (int i = 0; i < 5; i++)
                {
                    SerializedProperty lv = levelsProp.GetArrayElementAtIndex(i);
                    lv.FindPropertyRelative("damage").intValue = 15;
                    lv.FindPropertyRelative("cooldown").floatValue = 0.5f;
                    lv.FindPropertyRelative("range").floatValue = 7f;
                    lv.FindPropertyRelative("projectileSpeed").floatValue = 12f;
                    lv.FindPropertyRelative("projectileCount").intValue = 1;
                    lv.FindPropertyRelative("upgradeDescription").stringValue = "Mô tả vũ khí";
                }
                weaponSo.FindProperty("hasEvolution").boolValue = true;
                weaponSo.FindProperty("evolutionCatalyst").objectReferenceValue = catalyst;
                weaponSo.FindProperty("evolvedWeaponName").stringValue = "Liên Thanh Xuyên Thấu";
                weaponSo.ApplyModifiedPropertiesWithoutUndo();

                Health health = playerObj.AddComponent<Health>();
                PlayerExperience exp = playerObj.AddComponent<PlayerExperience>();
                PlayerStats stats = playerObj.AddComponent<PlayerStats>();
                InvokeLifecycle(health, "Awake");
                InvokeLifecycle(exp, "Awake");
                InvokeLifecycle(stats, "Awake");

                DummyWeapon weapon = weaponObj.AddComponent<DummyWeapon>();
                weapon.Initialize(weaponData, 1);

                UpgradeManager manager = managerObj.AddComponent<UpgradeManager>();
                SerializedObject mgrSo = new SerializedObject(manager);
                mgrSo.FindProperty("playerExperience").objectReferenceValue = exp;
                mgrSo.FindProperty("playerStats").objectReferenceValue = stats;
                mgrSo.FindProperty("weaponsContainer").objectReferenceValue = playerObj.transform;
                SerializedProperty passivesProp = mgrSo.FindProperty("availablePassives");
                passivesProp.arraySize = 1;
                passivesProp.GetArrayElementAtIndex(0).objectReferenceValue = catalyst;
                mgrSo.ApplyModifiedPropertiesWithoutUndo();

                FieldInfo poolField = typeof(UpgradeManager).GetField(
                    "_candidatePool",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(poolField);

                // 1. Có vũ khí nhưng CHƯA có bị động: thẻ vũ khí KHÔNG hiện gợi ý; thẻ bị động HIỆN gợi ý cho vũ khí đang có
                InvokeLifecycle(manager, "BuildCandidatePool");
                var pool = (System.Collections.Generic.List<UpgradeOption>)poolField.GetValue(manager);
                UpgradeOption weaponOpt = pool.Find(o => o.IsWeaponUpgrade);
                UpgradeOption passiveOpt = pool.Find(o => !o.IsWeaponUpgrade);

                StringAssert.DoesNotContain("Tiến hóa với:", weaponOpt.Description);
                StringAssert.Contains("Tiến hóa: Đạn Tự Nhắm", passiveOpt.Description);

                // 2. Đã có bị động Cấp 1: thẻ vũ khí bắt đầu HIỆN gợi ý ghép với bị động đang có
                Assert.IsTrue(stats.ApplyPassiveUpgrade(catalyst));
                InvokeLifecycle(manager, "BuildCandidatePool");
                weaponOpt = pool.Find(o => o.IsWeaponUpgrade);
                StringAssert.Contains("Tiến hóa với: Tốc độ đánh", weaponOpt.Description);

                // 3. Khi chưa mở khóa vũ khí (inactive): thẻ bị động không báo tiến hóa vũ khí chưa có
                weaponObj.SetActive(false);
                InvokeLifecycle(manager, "BuildCandidatePool");
                passiveOpt = pool.Find(o => !o.IsWeaponUpgrade);
                StringAssert.DoesNotContain("Tiến hóa:", passiveOpt.Description);

                // 4. Khi vũ khí đã tiến hóa: thẻ bị động không còn hiện gợi ý tiến hóa nữa
                weaponObj.SetActive(true);
                while (!weapon.IsMaxLevel) weapon.LevelUp();
                Assert.IsTrue(weapon.Evolve());
                InvokeLifecycle(manager, "BuildCandidatePool");
                passiveOpt = pool.Find(o => !o.IsWeaponUpgrade);
                StringAssert.DoesNotContain("Tiến hóa:", passiveOpt.Description);
            }
            finally
            {
                Object.DestroyImmediate(managerObj);
                Object.DestroyImmediate(playerObj);
                Object.DestroyImmediate(weaponData);
                Object.DestroyImmediate(catalyst);
            }
        }

        [Test]
        public void WeaponData_EvolvedIcon_UsesDedicatedSpriteWhenAssignedAndFallsBackToBaseIcon()
        {
            WeaponData weaponData = ScriptableObject.CreateInstance<WeaponData>();
            Texture2D tex = new Texture2D(4, 4);
            Sprite baseSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
            Sprite evoSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
            try
            {
                SerializedObject so = new SerializedObject(weaponData);
                so.FindProperty("icon").objectReferenceValue = baseSprite;
                so.FindProperty("evolvedIcon").objectReferenceValue = null;
                so.ApplyModifiedPropertiesWithoutUndo();

                Assert.AreSame(baseSprite, weaponData.EvolvedIcon);

                so.FindProperty("evolvedIcon").objectReferenceValue = evoSprite;
                so.ApplyModifiedPropertiesWithoutUndo();

                Assert.AreSame(evoSprite, weaponData.EvolvedIcon);
            }
            finally
            {
                Object.DestroyImmediate(evoSprite);
                Object.DestroyImmediate(baseSprite);
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(weaponData);
            }
        }
    }
}
