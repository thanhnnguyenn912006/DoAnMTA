using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SurvivorGame.Data;
using SurvivorGame.Enemies;
using SurvivorGame.Player;
using SurvivorGame.Systems;
using SurvivorGame.UI;
using SurvivorGame.Weapons;

namespace SurvivorGame.EditorTools
{
    /// <summary>
    /// Công cụ Editor 1-click sinh sprite Bình Máu / Nam Châm, tạo dữ liệu Bị động Tốc chạy,
    /// điền thông số Tiến hóa cho 3 vũ khí, tạo SpecialPickup_Prefab và nối đầy đủ 2 Scene Gameplay & MainMenu.
    /// </summary>
    public static class GameplayJuiceSetupEditor
    {
        private const string EffectsFolder = "Assets/_Project/Art/Sprites/Effects";
        private const string HealSpritePath = EffectsFolder + "/Pickup_Heal.png";
        private const string MagnetSpritePath = EffectsFolder + "/Pickup_Magnet.png";
        private const string MoveSpeedIconPath = "Assets/_Project/Art/Icons/Icon_MoveSpeed.png";
        private const string AutoGunEvoIconPath = "Assets/_Project/Art/Icons/Icon_AutoGun_Evo.png";
        private const string OrbitEvoIconPath = "Assets/_Project/Art/Icons/Icon_Orbit_Evo.png";
        private const string AreaPulseEvoIconPath = "Assets/_Project/Art/Icons/Icon_AreaPulse_Evo.png";

        private const string BoxSpritePath = "Assets/_Project/Art/UI/UI_RoundedBox.png";
        private const string FillSpritePath = "Assets/_Project/Art/UI/UI_RoundedFill.png";
        private const string FontAssetPath = "Assets/_Project/Art/Fonts/BeVietnamPro-SemiBold SDF.asset";
        private const string HudMaterialPath = "Assets/_Project/Art/Fonts/BeVietnamPro-SemiBold SDF - HUD Outline.mat";

        private const string PassiveDamagePath = "Assets/_Project/Data/Upgrades/Passive_Damage.asset";
        private const string PassiveCooldownPath = "Assets/_Project/Data/Upgrades/Passive_Cooldown.asset";
        private const string PassiveMaxHealthPath = "Assets/_Project/Data/Upgrades/Passive_MaxHealth.asset";
        private const string PassiveMagnetPath = "Assets/_Project/Data/Upgrades/Passive_Magnet.asset";
        private const string PassiveMoveSpeedPath = "Assets/_Project/Data/Upgrades/Passive_MoveSpeed.asset";

        private const string WeaponAutoGunPath = "Assets/_Project/Data/Weapons/WeaponData_AutoGun.asset";
        private const string WeaponOrbitPath = "Assets/_Project/Data/Weapons/WeaponData_Orbit.asset";
        private const string WeaponAreaPulsePath = "Assets/_Project/Data/Weapons/WeaponData_AreaPulse.asset";

        private const string ExpGemPrefabPath = "Assets/_Project/Prefabs/ExpGem.prefab";
        private const string SpecialPickupPrefabPath = "Assets/_Project/Prefabs/SpecialPickup_Prefab.prefab";

        private const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string GameplayScenePath = "Assets/_Project/Scenes/Gameplay.unity";

        private const string ControlsHintContent =
            "WASD / Mũi tên: Di chuyển  |  Space / Shift: Lướt  |  1/2/3: Chọn thẻ  |  ESC: Tạm dừng";

        private static readonly Color SliderTrackColor = Hex("#1E293B", 0.92f);
        private static readonly Color DashReadyColor = Hex("#22C55E", 1f);
        private static readonly Color TextPrimary = Hex("#F8FAFC", 1f);
        private static readonly Color TextSecondary = Hex("#CBD5E1", 1f);
        private static readonly Color TextGold = Hex("#FACC15", 1f);

        [MenuItem("Tools/Survivor Arena/Thiet Lap Tien Hoa, Vat Pham & HUD (1-Click)")]
        public static void RunSetupInteractive()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Dang chay Play Mode", "Vui long tat Play Mode truoc khi chay cong cu thiet lap.", "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            ExecuteSetup();
        }

        /// <summary>
        /// Điểm vào chạy tự động qua -executeMethod ở chế độ batchmode.
        /// </summary>
        public static void RunSetupBatchMode()
        {
            ExecuteSetup();
        }

        private static void ExecuteSetup()
        {
            string previousScenePath = SceneManager.GetActiveScene().path;

            // 1. Sinh đủ 8 icon (bao gồm Icon_MoveSpeed.png 64x64 Bilinear)
            IconGeneratorEditor.GenerateAllIcons();

            // 2. Sinh 2 sprite pixel-art 16x16 (FilterMode.Point, PPU 20) cho Bình Máu & Nam Châm Lớn
            GeneratePickupSprites();
            Sprite healSprite = AssetDatabase.LoadAssetAtPath<Sprite>(HealSpritePath);
            Sprite magnetSprite = AssetDatabase.LoadAssetAtPath<Sprite>(MagnetSpritePath);

            // 3. Tạo Passive_MoveSpeed.asset và cập nhật thông số Tiến hóa cho 3 WeaponData
            PassiveUpgradeData moveSpeedPassive = EnsureMoveSpeedPassiveAsset();
            PassiveUpgradeData damagePassive = AssetDatabase.LoadAssetAtPath<PassiveUpgradeData>(PassiveDamagePath);
            PassiveUpgradeData cooldownPassive = AssetDatabase.LoadAssetAtPath<PassiveUpgradeData>(PassiveCooldownPath);
            PassiveUpgradeData maxHealthPassive = AssetDatabase.LoadAssetAtPath<PassiveUpgradeData>(PassiveMaxHealthPath);
            PassiveUpgradeData magnetPassive = AssetDatabase.LoadAssetAtPath<PassiveUpgradeData>(PassiveMagnetPath);

            ConfigureWeaponEvolutions(cooldownPassive, damagePassive, maxHealthPassive);

            // 4. Tạo SpecialPickup_Prefab.prefab (Sorting Layer = Pickups, 0 Collider vật lý)
            SpecialPickup pickupPrefab = EnsureSpecialPickupPrefab(healSprite);

            // 5. Nối tham chiếu & dựng UI trên Gameplay.unity và MainMenu.unity
            PassiveUpgradeData[] allPassives =
            {
                damagePassive,
                cooldownPassive,
                maxHealthPassive,
                magnetPassive,
                moveSpeedPassive
            };

            Sprite boxSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BoxSpritePath);
            Sprite fillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(FillSpritePath);
            TMP_FontAsset baseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            Material hudMat = AssetDatabase.LoadAssetAtPath<Material>(HudMaterialPath);

            WireGameplayScene(allPassives, pickupPrefab, healSprite, magnetSprite, boxSprite, fillSprite, baseFont, hudMat);
            WireMainMenuScene(baseFont);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!Application.isBatchMode && !string.IsNullOrEmpty(previousScenePath) && File.Exists(previousScenePath))
            {
                EditorSceneManager.OpenScene(previousScenePath, OpenSceneMode.Single);
            }

            Debug.Log("[GameplayJuiceSetup] Hoan tat sinh tai nguyen, du lieu Tien hoa, Prefab va noi 2 Scene!");
        }

        private static void GeneratePickupSprites()
        {
            if (!Directory.Exists(EffectsFolder))
            {
                Directory.CreateDirectory(EffectsFolder);
            }

            // Bình Máu 16x16: bình đỏ viền trắng, nút bần nâu, chữ thập trắng ở tâm
            var healPalette = new Dictionary<char, Color>
            {
                ['W'] = Hex("#F8FAFC", 1f),
                ['C'] = Hex("#D97706", 1f),
                ['G'] = Hex("#CBD5E1", 1f),
                ['R'] = Hex("#EF4444", 1f),
                ['P'] = Hex("#FCA5A5", 1f),
                ['D'] = Hex("#B91C1C", 1f)
            };

            SavePixelGrid(HealSpritePath, new[]
            {
                "......WWWW......",
                ".....WCCCCW.....",
                ".....WCCCCW.....",
                "....WWGGGGWW....",
                "...WGGGGGGGGW...",
                "..WGGRRRRRRGGW..",
                ".WGRRPPWWPPRRDW.",
                ".WGRRRPWWPRRRDW.",
                ".WGRRWWWWWWRRDW.",
                ".WGRRWWWWWWRRDW.",
                ".WGRRRPWWPRRRDW.",
                ".WGRRRRRRRRRDDW.",
                "..WGRRRRRRRDDW..",
                "...WDRRRRRDDW...",
                "....WWDDDDWW....",
                "......WWWW......"
            }, healPalette);

            // Nam Châm Lớn 16x16: chữ U đỏ với 2 đầu cực bạc, viền tối nổi bật
            var magnetPalette = new Dictionary<char, Color>
            {
                ['K'] = Hex("#0F172A", 1f),
                ['W'] = Hex("#FFFFFF", 1f),
                ['S'] = Hex("#E2E8F0", 1f),
                ['M'] = Hex("#94A3B8", 1f),
                ['H'] = Hex("#F87171", 1f),
                ['R'] = Hex("#EF4444", 1f),
                ['D'] = Hex("#B91C1C", 1f)
            };

            SavePixelGrid(MagnetSpritePath, new[]
            {
                "................",
                "..KKKK....KKKK..",
                ".KWWSSK..KWWSSK.",
                ".KWWSMK..KWWSMK.",
                ".KSSMMK..KSSMMK.",
                ".KHRRDK..KHRRDK.",
                ".KHRRDK..KHRRDK.",
                ".KHRRDK..KHRRDK.",
                ".KHRRDK..KHRRDK.",
                ".KHRRDKKKKHRRDK.",
                ".KHRRRRRRRRRRDK.",
                "..KHRRRRRRRRDK..",
                "...KDRRRRRRDK...",
                "....KKDDDDKK....",
                "......KKKK......",
                "................"
            }, magnetPalette);

            AssetDatabase.Refresh();
            ConfigurePixelSprite(HealSpritePath, 20f);
            ConfigurePixelSprite(MagnetSpritePath, 20f);
        }

        private static PassiveUpgradeData EnsureMoveSpeedPassiveAsset()
        {
            PassiveUpgradeData asset = AssetDatabase.LoadAssetAtPath<PassiveUpgradeData>(PassiveMoveSpeedPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<PassiveUpgradeData>();
                AssetDatabase.CreateAsset(asset, PassiveMoveSpeedPath);
            }

            Sprite speedIcon = AssetDatabase.LoadAssetAtPath<Sprite>(MoveSpeedIconPath);

            SerializedObject so = new SerializedObject(asset);
            so.FindProperty("upgradeId").stringValue = "passive_speed";
            so.FindProperty("upgradeName").stringValue = "Tốc chạy";
            so.FindProperty("icon").objectReferenceValue = speedIcon;
            so.FindProperty("description").stringValue = "Tăng +0.5 tốc độ di chuyển";
            so.FindProperty("upgradeType").enumValueIndex = (int)PassiveUpgradeType.MoveSpeed;
            so.FindProperty("maxLevel").intValue = 5;
            so.FindProperty("valuePerLevel").floatValue = 0.5f;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void ConfigureWeaponEvolutions(
            PassiveUpgradeData cooldownPassive,
            PassiveUpgradeData damagePassive,
            PassiveUpgradeData maxHealthPassive)
        {
            Sprite autoGunEvoIcon = AssetDatabase.LoadAssetAtPath<Sprite>(AutoGunEvoIconPath);
            Sprite orbitEvoIcon = AssetDatabase.LoadAssetAtPath<Sprite>(OrbitEvoIconPath);
            Sprite areaPulseEvoIcon = AssetDatabase.LoadAssetAtPath<Sprite>(AreaPulseEvoIconPath);

            ConfigureSingleEvolution(
                WeaponAutoGunPath,
                cooldownPassive,
                "Liên Thanh Xuyên Thấu",
                autoGunEvoIcon,
                "Tiến hóa: Bắn 3 viên siêu nhanh, sát thương cao và xuyên qua tối đa 2 quái",
                new WeaponLevelStats
                {
                    damage = 55,
                    cooldown = 0.30f,
                    range = 10f,
                    projectileSpeed = 19f,
                    projectileCount = 3,
                    upgradeDescription = "Liên Thanh Xuyên Thấu"
                },
                Hex("#FACC15", 1f),
                pierce: 2,
                knockback: 0f);

            ConfigureSingleEvolution(
                WeaponOrbitPath,
                damagePassive,
                "Vòng Lưỡi Cưa Hỏa Lực",
                orbitEvoIcon,
                "Tiến hóa: 6 cầu lửa cỡ lớn xoay cực nhanh với bán kính rộng hơn",
                new WeaponLevelStats
                {
                    damage = 50,
                    cooldown = 0.18f,
                    range = 3.0f,
                    projectileSpeed = 400f,
                    projectileCount = 6,
                    upgradeDescription = "Vòng Lưỡi Cưa Hỏa Lực"
                },
                Hex("#FF6B2B", 1f),
                pierce: 0,
                knockback: 0f);

            ConfigureSingleEvolution(
                WeaponAreaPulsePath,
                maxHealthPassive,
                "Xung Chấn Đẩy Lùi",
                areaPulseEvoIcon,
                "Tiến hóa: Sóng xung kích cực rộng, hồi nhanh và đẩy lùi quái thường",
                new WeaponLevelStats
                {
                    damage = 68,
                    cooldown = 1.05f,
                    range = 5.4f,
                    projectileSpeed = 0f,
                    projectileCount = 1,
                    upgradeDescription = "Xung Chấn Đẩy Lùi"
                },
                Hex("#F59E0B", 1f),
                pierce: 0,
                knockback: 5.5f);
        }

        private static void ConfigureSingleEvolution(
            string assetPath,
            PassiveUpgradeData catalyst,
            string evolvedName,
            Sprite evolvedIcon,
            string evolvedDesc,
            WeaponLevelStats stats,
            Color tintColor,
            int pierce,
            float knockback)
        {
            WeaponData data = AssetDatabase.LoadAssetAtPath<WeaponData>(assetPath);
            if (data == null)
            {
                Debug.LogError($"[GameplayJuiceSetup] Khong tim thay WeaponData tai: {assetPath}");
                return;
            }

            SerializedObject so = new SerializedObject(data);
            so.FindProperty("hasEvolution").boolValue = true;
            so.FindProperty("evolutionCatalyst").objectReferenceValue = catalyst;
            so.FindProperty("evolvedWeaponName").stringValue = evolvedName;
            so.FindProperty("evolvedIcon").objectReferenceValue = evolvedIcon;
            so.FindProperty("evolvedDescription").stringValue = evolvedDesc;

            SerializedProperty statsProp = so.FindProperty("evolvedStats");
            statsProp.FindPropertyRelative("damage").intValue = stats.damage;
            statsProp.FindPropertyRelative("cooldown").floatValue = stats.cooldown;
            statsProp.FindPropertyRelative("range").floatValue = stats.range;
            statsProp.FindPropertyRelative("projectileSpeed").floatValue = stats.projectileSpeed;
            statsProp.FindPropertyRelative("projectileCount").intValue = stats.projectileCount;
            statsProp.FindPropertyRelative("upgradeDescription").stringValue = stats.upgradeDescription;

            so.FindProperty("evolvedTintColor").colorValue = tintColor;
            so.FindProperty("evolvedProjectilePierce").intValue = pierce;
            so.FindProperty("evolvedKnockbackForce").floatValue = knockback;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(data);
        }

        private static SpecialPickup EnsureSpecialPickupPrefab(Sprite defaultSprite)
        {
            Material sharedSpriteMat = null;
            GameObject expGemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ExpGemPrefabPath);
            if (expGemPrefab != null && expGemPrefab.TryGetComponent(out SpriteRenderer expSr))
            {
                sharedSpriteMat = expSr.sharedMaterial;
            }

            GameObject savedPrefab = null;
            GameObject tempObj = new GameObject("SpecialPickup_Prefab", typeof(SpriteRenderer), typeof(SpecialPickup));
            try
            {
                tempObj.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

                SpriteRenderer sr = tempObj.GetComponent<SpriteRenderer>();
                sr.sprite = defaultSprite;
                sr.color = Color.white;
                if (sharedSpriteMat != null)
                {
                    sr.sharedMaterial = sharedSpriteMat;
                }
                sr.sortingLayerName = "Pickups";
                sr.sortingOrder = 3;

                savedPrefab = PrefabUtility.SaveAsPrefabAsset(tempObj, SpecialPickupPrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(tempObj);
            }

            if (savedPrefab == null)
            {
                savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SpecialPickupPrefabPath);
            }

            return savedPrefab != null ? savedPrefab.GetComponent<SpecialPickup>() : null;
        }

        private static void WireGameplayScene(
            PassiveUpgradeData[] allPassives,
            SpecialPickup pickupPrefab,
            Sprite healSprite,
            Sprite magnetSprite,
            Sprite boxSprite,
            Sprite fillSprite,
            TMP_FontAsset baseFont,
            Material hudMat)
        {
            Scene scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);

            UpgradeManager upgradeManager = Object.FindAnyObjectByType<UpgradeManager>();
            if (upgradeManager != null)
            {
                SerializedObject so = new SerializedObject(upgradeManager);
                SetObjectArray(so.FindProperty("availablePassives"), allPassives);
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(upgradeManager);
            }

            PlayerStats playerStats = Object.FindAnyObjectByType<PlayerStats>();
            PlayerMovement playerMovement = Object.FindAnyObjectByType<PlayerMovement>();
            EnemySpawner enemySpawner = Object.FindAnyObjectByType<EnemySpawner>();
            BossSpawner bossSpawner = Object.FindAnyObjectByType<BossSpawner>();

            WeaponBarHUD weaponBar = Object.FindAnyObjectByType<WeaponBarHUD>();
            if (weaponBar != null)
            {
                SerializedObject so = new SerializedObject(weaponBar);
                SetObjectArray(so.FindProperty("trackedPassives"), allPassives);
                if (upgradeManager != null)
                {
                    so.FindProperty("upgradeManager").objectReferenceValue = upgradeManager;
                    SerializedObject umSo = new SerializedObject(upgradeManager);
                    Object umContainer = umSo.FindProperty("weaponsContainer").objectReferenceValue;
                    if (umContainer != null)
                    {
                        so.FindProperty("weaponsContainer").objectReferenceValue = umContainer;
                    }
                }
                if (playerStats != null) so.FindProperty("playerStats").objectReferenceValue = playerStats;
                if (so.FindProperty("weaponsContainer").objectReferenceValue == null && playerMovement != null)
                {
                    so.FindProperty("weaponsContainer").objectReferenceValue = playerMovement.transform;
                }
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(weaponBar);
            }

            ExpDropManager dropManager = Object.FindAnyObjectByType<ExpDropManager>();
            if (dropManager != null)
            {
                SerializedObject so = new SerializedObject(dropManager);
                so.FindProperty("specialPickupPrefab").objectReferenceValue = pickupPrefab;
                so.FindProperty("healPotionSprite").objectReferenceValue = healSprite;
                so.FindProperty("mapMagnetSprite").objectReferenceValue = magnetSprite;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(dropManager);
            }

            // Đảm bảo PulseVisual của AreaPulseWeapon luôn active (chỉ tắt SpriteRenderer khi nghỉ)
            AreaPulseWeapon[] pulseWeapons = Object.FindObjectsByType<AreaPulseWeapon>(FindObjectsInactive.Include);
            for (int i = 0; i < pulseWeapons.Length; i++)
            {
                SerializedObject so = new SerializedObject(pulseWeapons[i]);
                SpriteRenderer pulseSr = so.FindProperty("pulseVisual").objectReferenceValue as SpriteRenderer;
                if (pulseSr != null)
                {
                    if (!pulseSr.gameObject.activeSelf)
                    {
                        pulseSr.gameObject.SetActive(true);
                    }
                    pulseSr.enabled = false;
                    EditorUtility.SetDirty(pulseSr.gameObject);
                    EditorUtility.SetDirty(pulseSr);
                }
            }

            GameplayHUD hud = Object.FindAnyObjectByType<GameplayHUD>();
            if (hud != null)
            {
                (Slider dashSlider, TMP_Text dashText) = EnsureDashCooldownBar(hud.transform, boxSprite, fillSprite, baseFont, hudMat);
                TMP_Text bannerText = EnsureAnnouncementBanner(hud.transform, baseFont, hudMat);

                SerializedObject so = new SerializedObject(hud);
                if (playerMovement != null) so.FindProperty("playerMovement").objectReferenceValue = playerMovement;
                if (enemySpawner != null) so.FindProperty("enemySpawner").objectReferenceValue = enemySpawner;
                if (bossSpawner != null) so.FindProperty("bossSpawner").objectReferenceValue = bossSpawner;
                so.FindProperty("dashCooldownSlider").objectReferenceValue = dashSlider;
                so.FindProperty("dashStatusText").objectReferenceValue = dashText;
                so.FindProperty("announcementBannerText").objectReferenceValue = bannerText;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(hud);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static (Slider, TMP_Text) EnsureDashCooldownBar(
            Transform hudRoot,
            Sprite boxSprite,
            Sprite fillSprite,
            TMP_FontAsset baseFont,
            Material hudMat)
        {
            Transform existing = hudRoot.Find("DashCooldownBar");
            GameObject barObj;
            if (existing != null)
            {
                barObj = existing.gameObject;
            }
            else
            {
                barObj = new GameObject("DashCooldownBar", typeof(RectTransform), typeof(Slider));
                barObj.transform.SetParent(hudRoot, false);
            }

            RectTransform barRect = barObj.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 1f);
            barRect.anchorMax = new Vector2(0f, 1f);
            barRect.pivot = new Vector2(0.5f, 0.5f);
            barRect.anchoredPosition = new Vector2(390f, -65f);
            barRect.sizeDelta = new Vector2(180f, 20f);

            // Background
            GameObject bgObj = GetOrCreateChild(barObj.transform, "Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0.25f);
            bgRect.anchorMax = new Vector2(1f, 0.75f);
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = Vector2.zero;

            Image bgImg = bgObj.GetComponent<Image>();
            if (boxSprite != null)
            {
                bgImg.sprite = boxSprite;
                bgImg.type = Image.Type.Sliced;
            }
            bgImg.color = SliderTrackColor;
            bgImg.raycastTarget = false;

            // Fill Area + Fill
            GameObject fillAreaObj = GetOrCreateChild(barObj.transform, "Fill Area", typeof(RectTransform));
            RectTransform fillAreaRect = fillAreaObj.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRect.anchoredPosition = Vector2.zero;
            fillAreaRect.sizeDelta = Vector2.zero;

            GameObject fillObj = GetOrCreateChild(fillAreaObj.transform, "Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.sizeDelta = Vector2.zero;

            Image fillImg = fillObj.GetComponent<Image>();
            if (fillSprite != null)
            {
                fillImg.sprite = fillSprite;
                fillImg.type = Image.Type.Sliced;
            }
            fillImg.color = DashReadyColor;
            fillImg.raycastTarget = false;

            Slider slider = barObj.GetComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;
            slider.navigation = new Navigation { mode = Navigation.Mode.None };
            slider.fillRect = fillRect;
            slider.handleRect = null;
            slider.targetGraphic = null;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            // DashStatusText nằm ngay bên dưới thanh giống HpText của PlayerHealthBar
            GameObject labelObj = GetOrCreateChild(barObj.transform, "DashStatusText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0f);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = new Vector2(0f, -18f);
            labelRect.sizeDelta = new Vector2(0f, 26f);

            TextMeshProUGUI statusText = labelObj.GetComponent<TextMeshProUGUI>();
            if (baseFont != null) statusText.font = baseFont;
            if (hudMat != null) statusText.fontSharedMaterial = hudMat;
            statusText.text = "LƯỚT [Space]";
            statusText.fontSize = 15f;
            statusText.fontStyle = FontStyles.Bold;
            statusText.alignment = TextAlignmentOptions.Center;
            statusText.overflowMode = TextOverflowModes.Overflow;
            statusText.color = TextPrimary;
            statusText.raycastTarget = false;

            return (slider, statusText);
        }

        private static TMP_Text EnsureAnnouncementBanner(Transform hudRoot, TMP_FontAsset baseFont, Material hudMat)
        {
            GameObject bannerObj = GetOrCreateChild(
                hudRoot,
                "AnnouncementBannerText",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));

            RectTransform rect = bannerObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -195f);
            rect.sizeDelta = new Vector2(1100f, 56f);

            TextMeshProUGUI tmp = bannerObj.GetComponent<TextMeshProUGUI>();
            if (baseFont != null) tmp.font = baseFont;
            if (hudMat != null) tmp.fontSharedMaterial = hudMat;
            tmp.text = string.Empty;
            tmp.fontSize = 32f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.color = TextGold;
            tmp.raycastTarget = false;

            bannerObj.SetActive(false);
            return tmp;
        }

        private static void WireMainMenuScene(TMP_FontAsset baseFont)
        {
            Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                GameObject hintObj = GetOrCreateChild(
                    canvas.transform,
                    "ControlsHintText",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(TextMeshProUGUI));

                RectTransform rect = hintObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0f, -255f);
                rect.sizeDelta = new Vector2(1400f, 44f);

                TextMeshProUGUI tmp = hintObj.GetComponent<TextMeshProUGUI>();
                if (baseFont != null)
                {
                    tmp.font = baseFont;
                    tmp.fontSharedMaterial = baseFont.material;
                }
                tmp.text = ControlsHintContent;
                tmp.fontSize = 22f;
                tmp.fontStyle = FontStyles.Normal;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = TextSecondary;
                tmp.raycastTarget = false;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static GameObject GetOrCreateChild(Transform parent, string name, params System.Type[] components)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    if (existing.GetComponent(components[i]) == null)
                    {
                        existing.gameObject.AddComponent(components[i]);
                    }
                }
                return existing.gameObject;
            }

            GameObject obj = new GameObject(name, components);
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static void SetObjectArray<T>(SerializedProperty arrayProp, T[] values) where T : Object
        {
            if (arrayProp == null || values == null) return;

            int validCount = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != null) validCount++;
            }

            arrayProp.arraySize = validCount;
            int idx = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == null) continue;
                arrayProp.GetArrayElementAtIndex(idx++).objectReferenceValue = values[i];
            }
        }

        private static void SavePixelGrid(string assetPath, string[] grid, Dictionary<char, Color> palette)
        {
            int height = grid.Length;
            int width = grid[0].Length;

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];

            for (int r = 0; r < height; r++)
            {
                string row = grid[r];
                int y = height - 1 - r;
                for (int x = 0; x < width; x++)
                {
                    char c = x < row.Length ? row[x] : '.';
                    pixels[y * width + x] = palette.TryGetValue(c, out Color col) ? col : Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            File.WriteAllBytes(assetPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void ConfigurePixelSprite(string assetPath, float pixelsPerUnit)
        {
            if (AssetImporter.GetAtPath(assetPath) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }

        private static Color Hex(string hex, float alpha)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color c))
            {
                c.a = alpha;
                return c;
            }
            return new Color(1f, 1f, 1f, alpha);
        }
    }
}
