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
    /// Công cụ Editor 1-click sinh toàn bộ sprite nhân vật/icon/vật phẩm/rương/mũi tên Boss,
    /// tạo dữ liệu 3 nhân vật, Quái Tinh Anh (Elite), Bị động Tốc chạy, thông số Tiến hóa 3 vũ khí,
    /// tạo Prefab SpecialPickup & TreasureChest và nối đầy đủ 2 Scene Gameplay & MainMenu.
    /// </summary>
    public static class GameplayJuiceSetupEditor
    {
        private const string EffectsFolder = "Assets/_Project/Art/Sprites/Effects";
        private const string ProjectilesFolder = "Assets/_Project/Art/Sprites/Projectiles";
        private const string CharactersArtFolder = "Assets/_Project/Art/Sprites/Characters";
        private const string CharactersDataFolder = "Assets/_Project/Data/Characters";
        private const string UiArtFolder = "Assets/_Project/Art/UI";

        private const string HealSpritePath = EffectsFolder + "/Pickup_Heal.png";
        private const string MagnetSpritePath = EffectsFolder + "/Pickup_Magnet.png";
        private const string ChestSpritePath = EffectsFolder + "/Pickup_Chest.png";
        private const string BossArrowSpritePath = UiArtFolder + "/UI_BossArrow.png";

        private const string AutoGunEvoVfxPath = ProjectilesFolder + "/Projectile_AutoGun_Evo.png";
        private const string OrbitEvoVfxPath = EffectsFolder + "/Orb_Sawblade_Evo.png";
        private const string AreaPulseEvoVfxPath = EffectsFolder + "/VFX_AreaPulse_Evo.png";

        private const string MoveSpeedIconPath = "Assets/_Project/Art/Icons/Icon_MoveSpeed.png";
        private const string AutoGunEvoIconPath = "Assets/_Project/Art/Icons/Icon_AutoGun_Evo.png";
        private const string OrbitEvoIconPath = "Assets/_Project/Art/Icons/Icon_Orbit_Evo.png";
        private const string AreaPulseEvoIconPath = "Assets/_Project/Art/Icons/Icon_AreaPulse_Evo.png";

        private const string BoxSpritePath = UiArtFolder + "/UI_RoundedBox.png";
        private const string FillSpritePath = UiArtFolder + "/UI_RoundedFill.png";
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

        private const string CharacterHunterPath = CharactersDataFolder + "/Character_Hunter.asset";
        private const string CharacterMagePath = CharactersDataFolder + "/Character_Mage.asset";
        private const string CharacterKnightPath = CharactersDataFolder + "/Character_Knight.asset";
        private const string EnemyEliteDataPath = "Assets/_Project/Data/Enemies/EnemyData_Elite.asset";

        private const string ExpGemPrefabPath = "Assets/_Project/Prefabs/ExpGem.prefab";
        private const string SpecialPickupPrefabPath = "Assets/_Project/Prefabs/SpecialPickup_Prefab.prefab";
        private const string TreasureChestPrefabPath = "Assets/_Project/Prefabs/TreasureChest_Prefab.prefab";

        private const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string GameplayScenePath = "Assets/_Project/Scenes/Gameplay.unity";

        private const string ControlsHintContent =
            "WASD / Mũi tên: Di chuyển  |  Space / Shift: Lướt  |  1/2/3: Chọn thẻ  |  ESC: Tạm dừng";

        private static readonly Color PanelBackdropColor = Hex("#141923", 0.88f);
        private static readonly Color CardNormalColor = Hex("#1E293B", 1f);
        private static readonly Color CardHighlightColor = Hex("#2E3B52", 1f);
        private static readonly Color CardPressedColor = Hex("#141923", 1f);
        private static readonly Color ChestCardColor = Hex("#2B1D0E", 0.98f);
        private static readonly Color PrimaryNormal = Hex("#2E7D5B", 1f);
        private static readonly Color PrimaryHighlight = Hex("#38966D", 1f);
        private static readonly Color PrimaryPressed = Hex("#236146", 1f);
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

            // 1. Sinh đủ 16 sprite nhân vật/quái (gồm Pháp Sư & Hiệp Sĩ mới) và 11 icon
            CharacterSpriteGeneratorEditor.GenerateAllCharacters();
            IconGeneratorEditor.GenerateAllIcons();

            // 2. Sinh sprite pixel-art Bình Máu, Nam Châm Lớn, Rương Báu, Mũi tên Boss & 3 VFX vũ khí tiến hóa
            GeneratePickupAndUiSprites();
            GenerateEvolvedWeaponVfxSprites();
            Sprite healSprite = AssetDatabase.LoadAssetAtPath<Sprite>(HealSpritePath);
            Sprite chestSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ChestSpritePath);

            // 3. Tạo dữ liệu: Passive_MoveSpeed, 3 CharacterData, EnemyData_Elite & cập nhật 3 WeaponData
            EnsureMoveSpeedPassiveAsset();
            PassiveUpgradeData damagePassive = AssetDatabase.LoadAssetAtPath<PassiveUpgradeData>(PassiveDamagePath);
            PassiveUpgradeData cooldownPassive = AssetDatabase.LoadAssetAtPath<PassiveUpgradeData>(PassiveCooldownPath);
            PassiveUpgradeData maxHealthPassive = AssetDatabase.LoadAssetAtPath<PassiveUpgradeData>(PassiveMaxHealthPath);

            ConfigureWeaponEvolutions(cooldownPassive, damagePassive, maxHealthPassive);
            EnsureCharacterAssets();
            EnsureEliteEnemyAsset();

            // 4. Tạo Prefab SpecialPickup và TreasureChest (Sorting Layer = Pickups, 0 Collider vật lý)
            EnsureSpecialPickupPrefab(healSprite);
            EnsureTreasureChestPrefab(chestSprite);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 5. Nối tham chiếu & dựng UI trên Gameplay.unity và MainMenu.unity
            // (Tự nạp lại Asset/Prefab SAU EditorSceneManager.OpenScene để không bị UnloadUnusedAssets xóa tham chiếu)
            WireGameplayScene();
            WireMainMenuScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!Application.isBatchMode && !string.IsNullOrEmpty(previousScenePath) && File.Exists(previousScenePath))
            {
                EditorSceneManager.OpenScene(previousScenePath, OpenSceneMode.Single);
            }

            Debug.Log("[GameplayJuiceSetup] Hoan tat sinh tai nguyen, 3 nhan vat, Elite/Ruong, Tien hoa va noi 2 Scene!");
        }

        private static void GeneratePickupAndUiSprites()
        {
            if (!Directory.Exists(EffectsFolder)) Directory.CreateDirectory(EffectsFolder);
            if (!Directory.Exists(UiArtFolder)) Directory.CreateDirectory(UiArtFolder);

            // 1. Bình Máu 16x16
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

            // 2. Nam Châm Lớn 16x16
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

            // 3. Rương Báu 16x16 (gỗ nâu viền vàng kim + khóa ngọc đỏ)
            var chestPalette = new Dictionary<char, Color>
            {
                ['K'] = Hex("#14101C", 1f),
                ['W'] = Hex("#FEF08A", 1f),
                ['Y'] = Hex("#FACC15", 1f),
                ['y'] = Hex("#CA8A04", 1f),
                ['B'] = Hex("#92400E", 1f),
                ['b'] = Hex("#78350F", 1f),
                ['R'] = Hex("#EF4444", 1f)
            };

            SavePixelGrid(ChestSpritePath, new[]
            {
                "................",
                "..KKKKKKKKKKKK..",
                ".KWWYYYYYYYYWWK.",
                ".KYBBBBBBBBBBYK.",
                ".KYBYYBBBBYYBYK.",
                ".KYBBBBBBBBBBYK.",
                ".KYYYYYYYYYYYYK.",
                ".KKyyyyRRyyyyKK.",
                ".KYbbbbRRbbbbYK.",
                ".KYbYYbYYbYYbYK.",
                ".KYbbbbbbbbbbYK.",
                ".KYbbbbbbbbbbYK.",
                ".KyyyyyyyyyyyyK.",
                "..KKKKKKKKKKKK..",
                "................",
                "................"
            }, chestPalette);

            // 4. Mũi tên chỉ hướng Boss 32x32 (hướng phải +X để Atan2 xoay trực tiếp)
            GenerateBossArrowSprite(BossArrowSpritePath, 32);

            AssetDatabase.Refresh();
            ConfigurePixelSprite(HealSpritePath, 20f);
            ConfigurePixelSprite(MagnetSpritePath, 20f);
            ConfigurePixelSprite(ChestSpritePath, 20f);
            ConfigureUiSprite(BossArrowSpritePath, 32f);
        }

        private static void GenerateBossArrowSprite(string assetPath, int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color outline = Hex("#14080E", 1f);
            Color rimGold = Hex("#FACC15", 1f);
            Color coreRed = Hex("#EF4444", 1f);
            Color highlight = Hex("#FCA5A5", 1f);

            float cy = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dy = Mathf.Abs(y - cy);
                    // Tam giác mũi tên hướng phải: mũi tại x=29, đáy tại x=5
                    float maxDyAtX = (29f - x) * 0.52f;
                    bool inBody = x >= 5 && x <= 29 && dy <= maxDyAtX;
                    bool inNotch = x < 10 && dy < (10f - x) * 0.7f;

                    Color col = Color.clear;
                    if (inBody && !inNotch)
                    {
                        float edgeDist = Mathf.Min(maxDyAtX - dy, x - 5f);
                        if (edgeDist < 1.6f || (x < 11.5f && dy < (11.5f - x) * 0.7f))
                        {
                            col = outline;
                        }
                        else if (edgeDist < 3.4f)
                        {
                            col = rimGold;
                        }
                        else if (y > cy && dy < maxDyAtX * 0.45f)
                        {
                            col = highlight;
                        }
                        else
                        {
                            col = coreRed;
                        }
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(assetPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void GenerateEvolvedWeaponVfxSprites()
        {
            if (!Directory.Exists(ProjectilesFolder)) Directory.CreateDirectory(ProjectilesFolder);
            if (!Directory.Exists(EffectsFolder)) Directory.CreateDirectory(EffectsFolder);

            // 1. Liên Thanh Xuyên Thấu (28x12): Thương plasma điện quang xanh lam-cyan lõi trắng, hướng sang PHẢI (+X)
            var evoLancePalette = new Dictionary<char, Color>
            {
                ['K'] = Hex("#071630", 1f),
                ['R'] = Hex("#2563EB", 1f),
                ['O'] = Hex("#00B4FF", 1f),
                ['Y'] = Hex("#00F0FF", 1f),
                ['L'] = Hex("#CCFBF1", 1f),
                ['W'] = Hex("#FFFFFF", 1f),
                ['r'] = Hex("#2563EB", 0.55f),
                ['y'] = Hex("#00F0FF", 0.78f)
            };

            SavePixelGrid(AutoGunEvoVfxPath, new[]
            {
                "............KKK.............",
                "...........KRROKK...........",
                "..........KROYYOOKK.........",
                ".....rrKKKROYYLLYYOOKK......",
                "..rryyOOYYYYWWWWWWLLYYOKK...",
                "rryyOOYYWWWWWWWWWWWWWWLLWWK.",
                "rryyOOYYWWWWWWWWWWWWWWLLWWK.",
                "..rryyOOYYYYWWWWWWLLYYOKK...",
                ".....rrKKKROYYLLYYOOKK......",
                "..........KROYYOOKK.........",
                "...........KRROKK...........",
                "............KKK............."
            }, evoLancePalette);

            // 2. Vòng Lưỡi Cưa Hỏa Lực (24x24): Lưỡi cưa 8 răng cong rực lửa, rãnh tản nhiệt & lõi trắng nóng
            GenerateSawbladeSprite(OrbitEvoVfxPath, 24);

            // 3. Xung Chấn Đẩy Lùi (128x128, PPU=128 = đúng 1x1 unit): Vòng địa chấn hỏa diễm 16 gai + 8 tia năng lượng
            GenerateEvolvedAreaPulseSprite(AreaPulseEvoVfxPath, 128);

            AssetDatabase.Refresh();
            ConfigurePixelSprite(AutoGunEvoVfxPath, 20f);
            ConfigurePixelSprite(OrbitEvoVfxPath, 20f);
            ConfigureUiSprite(AreaPulseEvoVfxPath, 128f);
        }

        private static void GenerateSawbladeSprite(string assetPath, int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) * 0.5f;
            const int teeth = 8;
            float sectorAngle = (Mathf.PI * 2f) / teeth;

            Color outline = Hex("#2B0704", 1f);
            Color crimson = Hex("#EF2318", 1f);
            Color fireOrange = Hex("#FF6D00", 1f);
            Color moltenGold = Hex("#FACC15", 1f);
            Color brightTip = Hex("#FFF9C4", 1f);
            Color hubGold = Hex("#FEF08A", 1f);
            Color hotCore = Hex("#FFFFFF", 1f);
            Color ventDark = Hex("#450A0A", 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);
                    if (angle < 0f) angle += Mathf.PI * 2f;

                    float phase = Mathf.Repeat((angle / sectorAngle) + dist * 0.045f, 1f);
                    float toothOuterR = Mathf.Lerp(11.45f, 7.2f, Mathf.Pow(phase, 0.72f));

                    Color col = Color.clear;
                    if (dist <= toothOuterR)
                    {
                        float edgeDist = toothOuterR - dist;
                        if (dist < 2.2f)
                        {
                            col = hotCore;
                        }
                        else if (dist < 3.6f)
                        {
                            col = hubGold;
                        }
                        else if (dist < 7.0f)
                        {
                            float spoke = Mathf.Abs(Mathf.Sin(angle * 2f));
                            if (dist > 4.2f && dist < 6.4f && spoke > 0.86f)
                            {
                                col = ventDark;
                            }
                            else
                            {
                                col = dist < 5.5f ? moltenGold : fireOrange;
                            }
                        }
                        else
                        {
                            if (dist >= 9.6f && phase < 0.22f)
                            {
                                col = brightTip;
                            }
                            else if (edgeDist < 1.05f)
                            {
                                col = outline;
                            }
                            else
                            {
                                col = phase < 0.45f ? fireOrange : crimson;
                            }
                        }
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(assetPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static void GenerateEvolvedAreaPulseSprite(string assetPath, int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) * 0.5f;

            // Tông tím hư không - hồng neon để tách biệt hẳn với đạn xanh lam và lưỡi cưa lửa đỏ-cam
            Color deepVoid = Hex("#7E22CE", 1f);
            Color neonMagenta = Hex("#D946EF", 1f);
            Color vividOrchid = Hex("#C084FC", 1f);
            Color brightCore = Hex("#FAF5FF", 1f);
            Color innerRay = Hex("#F0ABFC", 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);

                    float spikeWave = Mathf.Pow(0.5f + 0.5f * Mathf.Cos(angle * 16f), 2.4f);
                    float ripple = Mathf.Sin(angle * 8f) * 1.6f;
                    float outerEdge = 54.5f + spikeWave * 8.2f;

                    Color col = Color.clear;
                    if (dist <= outerEdge)
                    {
                        float aa = Mathf.Clamp01((outerEdge - dist) * 0.85f);

                        if (dist >= 51.5f + ripple * 0.45f)
                        {
                            float t = Mathf.InverseLerp(51.5f + ripple * 0.45f, outerEdge, dist);
                            col = Color.Lerp(neonMagenta, deepVoid, t);
                            col.a = 0.96f * aa;
                        }
                        else if (dist >= 45.5f + ripple * 0.45f)
                        {
                            float t = Mathf.InverseLerp(45.5f + ripple * 0.45f, 51.5f + ripple * 0.45f, dist);
                            col = Color.Lerp(vividOrchid, brightCore, 1f - Mathf.Abs(t * 2f - 1f));
                            col.a = 1f;
                        }
                        else if (dist >= 34.5f + ripple)
                        {
                            float ringDist = Mathf.Abs(dist - (38.0f + ripple * 0.6f));
                            if (ringDist < 2.2f)
                            {
                                col = Color.Lerp(vividOrchid, brightCore, 1f - ringDist / 2.2f);
                                col.a = 0.86f;
                            }
                            else
                            {
                                float t = Mathf.InverseLerp(34.5f + ripple, 45.5f + ripple * 0.45f, dist);
                                col = Color.Lerp(neonMagenta, vividOrchid, t);
                                col.a = Mathf.Lerp(0.32f, 0.78f, t);
                            }
                        }
                        else
                        {
                            float radialT = Mathf.Clamp01(dist / 34.5f);
                            float ray = dist > 10f ? Mathf.Pow(Mathf.Abs(Mathf.Cos(angle * 4f)), 16f) : 0f;
                            col = Color.Lerp(deepVoid, innerRay, Mathf.Clamp01(ray * 0.9f + radialT * 0.35f));
                            col.a = Mathf.Clamp01(Mathf.Lerp(0.10f, 0.30f, radialT) + ray * 0.54f);
                        }
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(assetPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        private static CharacterData[] EnsureCharacterAssets()
        {
            if (!Directory.Exists(CharactersDataFolder))
            {
                Directory.CreateDirectory(CharactersDataFolder);
            }

            CharacterData hunter = EnsureSingleCharacter(
                CharacterHunterPath,
                "hunter",
                "Thợ Săn",
                "Đạn Tự Nhắm",
                "100 HP  |  Tốc 5.0  |  Cân bằng",
                WeaponType.AutoTarget,
                100,
                5.0f,
                1.0f,
                CharactersArtFolder + "/Player_Hunter_0.png",
                CharactersArtFolder + "/Player_Hunter_1.png",
                Hex("#EF4444", 1f));

            CharacterData mage = EnsureSingleCharacter(
                CharacterMagePath,
                "mage",
                "Pháp Sư",
                "Cầu Năng Lượng",
                "90 HP  |  +10% Sát thương",
                WeaponType.Orbit,
                90,
                5.0f,
                1.1f,
                CharactersArtFolder + "/Player_Mage_0.png",
                CharactersArtFolder + "/Player_Mage_1.png",
                Hex("#38BDF8", 1f));

            CharacterData knight = EnsureSingleCharacter(
                CharacterKnightPath,
                "knight",
                "Hiệp Sĩ",
                "Sóng Xung Kích",
                "120 HP  |  Tốc 4.7  |  Chống chịu",
                WeaponType.AreaPulse,
                120,
                4.7f,
                1.0f,
                CharactersArtFolder + "/Player_Knight_0.png",
                CharactersArtFolder + "/Player_Knight_1.png",
                Hex("#FACC15", 1f));

            return new[] { hunter, mage, knight };
        }

        private static CharacterData EnsureSingleCharacter(
            string assetPath,
            string id,
            string displayName,
            string weaponLabel,
            string statSummary,
            WeaponType weaponType,
            int maxHp,
            float speed,
            float dmgMult,
            string frame0Path,
            string frame1Path,
            Color accent)
        {
            CharacterData asset = AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<CharacterData>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }

            Sprite f0 = AssetDatabase.LoadAssetAtPath<Sprite>(frame0Path);
            Sprite f1 = AssetDatabase.LoadAssetAtPath<Sprite>(frame1Path);

            SerializedObject so = new SerializedObject(asset);
            so.FindProperty("characterId").stringValue = id;
            so.FindProperty("characterName").stringValue = displayName;
            so.FindProperty("startingWeaponLabel").stringValue = weaponLabel;
            so.FindProperty("statSummary").stringValue = statSummary;
            so.FindProperty("idleSprite").objectReferenceValue = f0;
            SetObjectArray(so.FindProperty("walkFrames"), new[] { f0, f1 });
            so.FindProperty("accentColor").colorValue = accent;
            so.FindProperty("startingWeaponType").enumValueIndex = (int)weaponType;
            so.FindProperty("maxHealth").intValue = maxHp;
            so.FindProperty("moveSpeed").floatValue = speed;
            so.FindProperty("baseDamageMultiplier").floatValue = dmgMult;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static EnemyData EnsureEliteEnemyAsset()
        {
            EnemyData asset = AssetDatabase.LoadAssetAtPath<EnemyData>(EnemyEliteDataPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<EnemyData>();
                AssetDatabase.CreateAsset(asset, EnemyEliteDataPath);
            }

            Sprite tank0 = AssetDatabase.LoadAssetAtPath<Sprite>(CharactersArtFolder + "/Enemy_Tank_0.png");
            Sprite tank1 = AssetDatabase.LoadAssetAtPath<Sprite>(CharactersArtFolder + "/Enemy_Tank_1.png");

            SerializedObject so = new SerializedObject(asset);
            so.FindProperty("enemyId").stringValue = "enemy_elite";
            so.FindProperty("enemyName").stringValue = "Quái Tinh Anh";
            so.FindProperty("maxHealth").intValue = 320;
            so.FindProperty("moveSpeed").floatValue = 1.85f;
            so.FindProperty("contactDamage").intValue = 24;
            so.FindProperty("expValue").intValue = 45;
            so.FindProperty("scoreValue").intValue = 120;
            so.FindProperty("customSprite").objectReferenceValue = tank0;
            SetObjectArray(so.FindProperty("walkFrames"), new[] { tank0, tank1 });
            so.FindProperty("frameInterval").floatValue = 0.22f;
            so.FindProperty("tintColor").colorValue = Hex("#FACC15", 1f);
            so.FindProperty("scale").floatValue = 1.45f;
            so.FindProperty("customPrefab").objectReferenceValue = null;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(asset);
            return asset;
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

            Sprite autoGunEvoVfx = AssetDatabase.LoadAssetAtPath<Sprite>(AutoGunEvoVfxPath);
            Sprite orbitEvoVfx = AssetDatabase.LoadAssetAtPath<Sprite>(OrbitEvoVfxPath);
            Sprite areaPulseEvoVfx = AssetDatabase.LoadAssetAtPath<Sprite>(AreaPulseEvoVfxPath);

            ConfigureSingleEvolution(
                WeaponAutoGunPath,
                cooldownPassive,
                "Liên Thanh Xuyên Thấu",
                autoGunEvoIcon,
                autoGunEvoVfx,
                "Tiến hóa: Bắn 3 thương plasma siêu nhanh, sát thương cao và xuyên qua tối đa 2 quái",
                new WeaponLevelStats
                {
                    damage = 55,
                    cooldown = 0.30f,
                    range = 10f,
                    projectileSpeed = 19f,
                    projectileCount = 3,
                    upgradeDescription = "Liên Thanh Xuyên Thấu"
                },
                Hex("#00E5FF", 1f),
                pierce: 2,
                knockback: 0f);

            ConfigureSingleEvolution(
                WeaponOrbitPath,
                damagePassive,
                "Vòng Lưỡi Cưa Hỏa Lực",
                orbitEvoIcon,
                orbitEvoVfx,
                "Tiến hóa: 6 lưỡi cưa hỏa lực cỡ lớn tự xoay siêu tốc kèm vòng lửa bao quanh",
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
                areaPulseEvoVfx,
                "Tiến hóa: Sóng địa chấn hư không kép cực rộng, hồi nhanh và đẩy lùi quái thường",
                new WeaponLevelStats
                {
                    damage = 68,
                    cooldown = 1.05f,
                    range = 5.4f,
                    projectileSpeed = 0f,
                    projectileCount = 1,
                    upgradeDescription = "Xung Chấn Đẩy Lùi"
                },
                Hex("#C084FC", 1f),
                pierce: 0,
                knockback: 5.5f);
        }

        private static void ConfigureSingleEvolution(
            string assetPath,
            PassiveUpgradeData catalyst,
            string evolvedName,
            Sprite evolvedIcon,
            Sprite evolvedVisualSprite,
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
            so.FindProperty("evolvedVisualSprite").objectReferenceValue = evolvedVisualSprite;
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

        private static Material ResolveDefaultSpriteMaterial()
        {
            GameObject expGemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ExpGemPrefabPath);
            if (expGemPrefab != null && expGemPrefab.TryGetComponent(out SpriteRenderer expSr))
            {
                return expSr.sharedMaterial;
            }
            return null;
        }

        private static SpecialPickup EnsureSpecialPickupPrefab(Sprite defaultSprite)
        {
            Material sharedSpriteMat = ResolveDefaultSpriteMaterial();
            GameObject savedPrefab = null;
            GameObject tempObj = new GameObject("SpecialPickup_Prefab", typeof(SpriteRenderer), typeof(SpecialPickup));
            try
            {
                tempObj.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

                SpriteRenderer sr = tempObj.GetComponent<SpriteRenderer>();
                sr.sprite = defaultSprite;
                sr.color = Color.white;
                if (sharedSpriteMat != null) sr.sharedMaterial = sharedSpriteMat;
                sr.sortingLayerName = "Pickups";
                sr.sortingOrder = 3;

                savedPrefab = PrefabUtility.SaveAsPrefabAsset(tempObj, SpecialPickupPrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(tempObj);
            }

            if (savedPrefab == null) savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SpecialPickupPrefabPath);
            return savedPrefab != null ? savedPrefab.GetComponent<SpecialPickup>() : null;
        }

        private static TreasureChest EnsureTreasureChestPrefab(Sprite chestSprite)
        {
            Material sharedSpriteMat = ResolveDefaultSpriteMaterial();
            GameObject savedPrefab = null;
            GameObject tempObj = new GameObject("TreasureChest_Prefab", typeof(SpriteRenderer), typeof(TreasureChest));
            try
            {
                tempObj.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

                SpriteRenderer sr = tempObj.GetComponent<SpriteRenderer>();
                sr.sprite = chestSprite;
                sr.color = Color.white;
                if (sharedSpriteMat != null) sr.sharedMaterial = sharedSpriteMat;
                sr.sortingLayerName = "Pickups";
                sr.sortingOrder = 4;

                savedPrefab = PrefabUtility.SaveAsPrefabAsset(tempObj, TreasureChestPrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(tempObj);
            }

            if (savedPrefab == null) savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TreasureChestPrefabPath);
            return savedPrefab != null ? savedPrefab.GetComponent<TreasureChest>() : null;
        }

        private static void WireGameplayScene()
        {
            Scene scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);

            PassiveUpgradeData[] allPassives =
            {
                AssetDatabase.LoadAssetAtPath<PassiveUpgradeData>(PassiveDamagePath),
                AssetDatabase.LoadAssetAtPath<PassiveUpgradeData>(PassiveCooldownPath),
                AssetDatabase.LoadAssetAtPath<PassiveUpgradeData>(PassiveMaxHealthPath),
                AssetDatabase.LoadAssetAtPath<PassiveUpgradeData>(PassiveMagnetPath),
                AssetDatabase.LoadAssetAtPath<PassiveUpgradeData>(PassiveMoveSpeedPath)
            };

            CharacterData[] allCharacters =
            {
                AssetDatabase.LoadAssetAtPath<CharacterData>(CharacterHunterPath),
                AssetDatabase.LoadAssetAtPath<CharacterData>(CharacterMagePath),
                AssetDatabase.LoadAssetAtPath<CharacterData>(CharacterKnightPath)
            };

            EnemyData eliteEnemyData = AssetDatabase.LoadAssetAtPath<EnemyData>(EnemyEliteDataPath);
            GameObject pickupObj = AssetDatabase.LoadAssetAtPath<GameObject>(SpecialPickupPrefabPath);
            SpecialPickup pickupPrefab = pickupObj != null ? pickupObj.GetComponent<SpecialPickup>() : null;
            GameObject chestObj = AssetDatabase.LoadAssetAtPath<GameObject>(TreasureChestPrefabPath);
            TreasureChest chestPrefab = chestObj != null ? chestObj.GetComponent<TreasureChest>() : null;

            Sprite healSprite = AssetDatabase.LoadAssetAtPath<Sprite>(HealSpritePath);
            Sprite magnetSprite = AssetDatabase.LoadAssetAtPath<Sprite>(MagnetSpritePath);
            Sprite bossArrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BossArrowSpritePath);
            Sprite boxSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BoxSpritePath);
            Sprite fillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(FillSpritePath);
            TMP_FontAsset baseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            Material hudMat = AssetDatabase.LoadAssetAtPath<Material>(HudMaterialPath);

            GameManager gameManager = Object.FindAnyObjectByType<GameManager>();
            UpgradeManager upgradeManager = Object.FindAnyObjectByType<UpgradeManager>();
            Transform weaponsContainer = null;

            if (upgradeManager != null)
            {
                SerializedObject so = new SerializedObject(upgradeManager);
                SetObjectArray(so.FindProperty("availablePassives"), allPassives);
                if (gameManager != null) so.FindProperty("gameManager").objectReferenceValue = gameManager;
                weaponsContainer = so.FindProperty("weaponsContainer").objectReferenceValue as Transform;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(upgradeManager);
            }

            PlayerStats playerStats = Object.FindAnyObjectByType<PlayerStats>();
            PlayerMovement playerMovement = Object.FindAnyObjectByType<PlayerMovement>();
            EnemySpawner enemySpawner = Object.FindAnyObjectByType<EnemySpawner>();
            BossSpawner bossSpawner = Object.FindAnyObjectByType<BossSpawner>();

            // Gắn thanh máu dưới chân Player & bộ chọn nhân vật khởi đầu lên Player
            if (playerMovement != null)
            {
                GameObject playerObj = playerMovement.gameObject;
                if (playerObj.GetComponent<PlayerWorldHealthBar>() == null)
                {
                    playerObj.AddComponent<PlayerWorldHealthBar>();
                }

                PlayerCharacterSelector selector = playerObj.GetComponent<PlayerCharacterSelector>();
                if (selector == null)
                {
                    selector = playerObj.AddComponent<PlayerCharacterSelector>();
                }

                SerializedObject selSo = new SerializedObject(selector);
                SetObjectArray(selSo.FindProperty("availableCharacters"), allCharacters);
                selSo.FindProperty("weaponsContainer").objectReferenceValue =
                    weaponsContainer != null ? weaponsContainer : playerObj.transform;
                selSo.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(selector);
            }

            if (enemySpawner != null)
            {
                SerializedObject so = new SerializedObject(enemySpawner);
                so.FindProperty("eliteEnemyData").objectReferenceValue = eliteEnemyData;
                SerializedProperty timesProp = so.FindProperty("eliteSpawnTimes");
                timesProp.arraySize = 3;
                timesProp.GetArrayElementAtIndex(0).floatValue = 70f;
                timesProp.GetArrayElementAtIndex(1).floatValue = 140f;
                timesProp.GetArrayElementAtIndex(2).floatValue = 195f;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(enemySpawner);
            }

            if (bossSpawner != null)
            {
                SerializedObject so = new SerializedObject(bossSpawner);
                so.FindProperty("firstBossTime").floatValue = 240f;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(bossSpawner);
            }

            PlayerExperience playerExperience = Object.FindAnyObjectByType<PlayerExperience>();
            if (playerExperience != null)
            {
                SerializedObject so = new SerializedObject(playerExperience);
                so.FindProperty("baseExpToLevel").intValue = 20;
                so.FindProperty("expFlatIncreasePerLevel").intValue = 16;
                so.FindProperty("expMultiplierPerLevel").floatValue = 1.03f;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(playerExperience);
            }

            WeaponBarHUD weaponBar = Object.FindAnyObjectByType<WeaponBarHUD>();
            if (weaponBar != null)
            {
                SerializedObject so = new SerializedObject(weaponBar);
                SetObjectArray(so.FindProperty("trackedPassives"), allPassives);
                if (upgradeManager != null) so.FindProperty("upgradeManager").objectReferenceValue = upgradeManager;
                if (playerStats != null) so.FindProperty("playerStats").objectReferenceValue = playerStats;
                if (weaponsContainer != null)
                {
                    so.FindProperty("weaponsContainer").objectReferenceValue = weaponsContainer;
                }
                else if (playerMovement != null)
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
                so.FindProperty("pickupPrewarmCount").intValue = 8;
                so.FindProperty("maxActivePickups").intValue = 6;
                so.FindProperty("healDropChance").floatValue = 0.038f;
                so.FindProperty("treasureChestPrefab").objectReferenceValue = chestPrefab;
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
                    if (!pulseSr.gameObject.activeSelf) pulseSr.gameObject.SetActive(true);
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
                RectTransform bossArrow = EnsureBossOffscreenIndicator(hud.transform, bossArrowSprite);

                SerializedObject so = new SerializedObject(hud);
                if (playerMovement != null) so.FindProperty("playerMovement").objectReferenceValue = playerMovement;
                if (enemySpawner != null) so.FindProperty("enemySpawner").objectReferenceValue = enemySpawner;
                if (bossSpawner != null) so.FindProperty("bossSpawner").objectReferenceValue = bossSpawner;
                so.FindProperty("dashCooldownSlider").objectReferenceValue = dashSlider;
                so.FindProperty("dashStatusText").objectReferenceValue = dashText;
                so.FindProperty("announcementBannerText").objectReferenceValue = bannerText;
                so.FindProperty("bossOffscreenIndicator").objectReferenceValue = bossArrow;
                so.FindProperty("mainCamera").objectReferenceValue = Camera.main;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(hud);
            }

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                EnsureChestRewardPanel(canvas, upgradeManager, gameManager, boxSprite, baseFont, hudMat);
                ConfigureEndGamePanelForBreakdown(canvas.transform, weaponsContainer);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static RectTransform EnsureBossOffscreenIndicator(Transform hudRoot, Sprite arrowSprite)
        {
            GameObject arrowObj = GetOrCreateChild(
                hudRoot,
                "BossOffscreenIndicator",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

            RectTransform rect = arrowObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(46f, 46f);

            Image img = arrowObj.GetComponent<Image>();
            if (arrowSprite != null) img.sprite = arrowSprite;
            img.color = Color.white;
            img.preserveAspect = true;
            img.raycastTarget = false;

            arrowObj.SetActive(false);
            return rect;
        }

        private static void EnsureChestRewardPanel(
            Canvas canvas,
            UpgradeManager upgradeManager,
            GameManager gameManager,
            Sprite boxSprite,
            TMP_FontAsset baseFont,
            Material hudMat)
        {
            Transform root = canvas.transform;
            GameObject panelObj = GetOrCreateChild(
                root,
                "ChestRewardPanel",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = Vector2.zero;

            Image backdrop = panelObj.GetComponent<Image>();
            backdrop.color = PanelBackdropColor;
            backdrop.raycastTarget = true;

            // Khung hộp phần thưởng trung tâm
            GameObject cardObj = GetOrCreateChild(
                panelObj.transform,
                "RewardCard",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            RectTransform cardRect = cardObj.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = Vector2.zero;
            cardRect.sizeDelta = new Vector2(580f, 440f);

            Image cardImg = cardObj.GetComponent<Image>();
            if (boxSprite != null)
            {
                cardImg.sprite = boxSprite;
                cardImg.type = Image.Type.Sliced;
            }
            cardImg.color = ChestCardColor;
            cardImg.raycastTarget = false;

            TMP_Text headerText = EnsurePanelText(cardObj.transform, "ChestHeaderText", new Vector2(0f, 165f), new Vector2(520f, 52f), 34f, FontStyles.Bold, TextGold, baseFont, hudMat);
            headerText.text = "RƯƠNG BÁU TINH ANH!";

            GameObject iconObj = GetOrCreateChild(cardObj.transform, "ChestIconImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(0f, 80f);
            iconRect.sizeDelta = new Vector2(84f, 84f);
            Image iconImg = iconObj.GetComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            TMP_Text titleText = EnsurePanelText(cardObj.transform, "ChestTitleText", new Vector2(0f, 4f), new Vector2(520f, 44f), 30f, FontStyles.Bold, TextPrimary, baseFont, baseFont != null ? baseFont.material : null);
            TMP_Text subtitleText = EnsurePanelText(cardObj.transform, "ChestSubtitleText", new Vector2(0f, -36f), new Vector2(520f, 36f), 22f, FontStyles.Bold, TextGold, baseFont, baseFont != null ? baseFont.material : null);
            TMP_Text descText = EnsurePanelText(cardObj.transform, "ChestDescText", new Vector2(0f, -94f), new Vector2(500f, 76f), 20f, FontStyles.Normal, TextSecondary, baseFont, baseFont != null ? baseFont.material : null);

            // Nút đóng popup
            GameObject btnObj = GetOrCreateChild(cardObj.transform, "ChestCloseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = new Vector2(0f, -168f);
            btnRect.sizeDelta = new Vector2(260f, 58f);

            Image btnImg = btnObj.GetComponent<Image>();
            if (boxSprite != null)
            {
                btnImg.sprite = boxSprite;
                btnImg.type = Image.Type.Sliced;
            }
            btnImg.color = Color.white;
            btnImg.raycastTarget = true;

            Button closeBtn = btnObj.GetComponent<Button>();
            closeBtn.transition = Selectable.Transition.ColorTint;
            ColorBlock cb = closeBtn.colors;
            cb.normalColor = PrimaryNormal;
            cb.highlightedColor = PrimaryHighlight;
            cb.selectedColor = PrimaryHighlight;
            cb.pressedColor = PrimaryPressed;
            cb.fadeDuration = 0.08f;
            closeBtn.colors = cb;

            TMP_Text btnLabel = EnsurePanelText(btnObj.transform, "Text (TMP)", Vector2.zero, Vector2.zero, 24f, FontStyles.Bold, TextPrimary, baseFont, baseFont != null ? baseFont.material : null);
            RectTransform btnLabelRect = btnLabel.GetComponent<RectTransform>();
            btnLabelRect.anchorMin = Vector2.zero;
            btnLabelRect.anchorMax = Vector2.one;
            btnLabelRect.anchoredPosition = Vector2.zero;
            btnLabelRect.sizeDelta = Vector2.zero;
            btnLabel.text = "NHẬN [Space]";

            panelObj.SetActive(false);

            // Gắn ChestRewardPanelUI lên Canvas (luôn bật)
            ChestRewardPanelUI panelUi = canvas.GetComponent<ChestRewardPanelUI>();
            if (panelUi == null) panelUi = canvas.gameObject.AddComponent<ChestRewardPanelUI>();

            SerializedObject uiSo = new SerializedObject(panelUi);
            if (upgradeManager != null) uiSo.FindProperty("upgradeManager").objectReferenceValue = upgradeManager;
            if (gameManager != null) uiSo.FindProperty("gameManager").objectReferenceValue = gameManager;
            uiSo.FindProperty("panelRoot").objectReferenceValue = panelObj;
            uiSo.FindProperty("headerText").objectReferenceValue = headerText;
            uiSo.FindProperty("iconImage").objectReferenceValue = iconImg;
            uiSo.FindProperty("titleText").objectReferenceValue = titleText;
            uiSo.FindProperty("subtitleText").objectReferenceValue = subtitleText;
            uiSo.FindProperty("descriptionText").objectReferenceValue = descText;
            uiSo.FindProperty("closeButton").objectReferenceValue = closeBtn;
            uiSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(panelUi);
        }

        private static void ConfigureEndGamePanelForBreakdown(Transform canvasRoot, Transform weaponsContainer)
        {
            GameStatePanelUI stateUi = Object.FindAnyObjectByType<GameStatePanelUI>();
            if (stateUi != null && weaponsContainer != null)
            {
                SerializedObject so = new SerializedObject(stateUi);
                so.FindProperty("weaponsContainer").objectReferenceValue = weaponsContainer;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(stateUi);
            }

            Transform endPanel = canvasRoot.Find("EndGamePanel");
            if (endPanel == null) return;

            SetRect(endPanel.Find("EndTitleText"), new Vector2(0f, 250f), new Vector2(760f, 84f));
            Transform statsTf = endPanel.Find("EndStatsText");
            SetRect(statsTf, new Vector2(0f, 25f), new Vector2(820f, 330f));
            if (statsTf != null && statsTf.TryGetComponent(out TMP_Text statsTmp))
            {
                statsTmp.richText = true;
                statsTmp.fontSize = 24f;
                statsTmp.alignment = TextAlignmentOptions.Center;
            }
            SetAnchoredY(endPanel.Find("EndRestartButton"), -215f);
            SetAnchoredY(endPanel.Find("EndMainMenuButton"), -215f);
        }

        private static (Slider, TMP_Text) EnsureDashCooldownBar(
            Transform hudRoot,
            Sprite boxSprite,
            Sprite fillSprite,
            TMP_FontAsset baseFont,
            Material hudMat)
        {
            Transform existing = hudRoot.Find("DashCooldownBar");
            GameObject barObj = existing != null
                ? existing.gameObject
                : new GameObject("DashCooldownBar", typeof(RectTransform), typeof(Slider));
            if (existing == null) barObj.transform.SetParent(hudRoot, false);

            RectTransform barRect = barObj.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 1f);
            barRect.anchorMax = new Vector2(0f, 1f);
            barRect.pivot = new Vector2(0.5f, 0.5f);
            barRect.anchoredPosition = new Vector2(390f, -65f);
            barRect.sizeDelta = new Vector2(180f, 20f);

            GameObject bgObj = GetOrCreateChild(barObj.transform, "Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0.25f);
            bgRect.anchorMax = new Vector2(1f, 0.75f);
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = Vector2.zero;

            Image bgImg = bgObj.GetComponent<Image>();
            if (boxSprite != null) { bgImg.sprite = boxSprite; bgImg.type = Image.Type.Sliced; }
            bgImg.color = SliderTrackColor;
            bgImg.raycastTarget = false;

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
            if (fillSprite != null) { fillImg.sprite = fillSprite; fillImg.type = Image.Type.Sliced; }
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

        private static void WireMainMenuScene()
        {
            Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

            CharacterData[] allCharacters =
            {
                AssetDatabase.LoadAssetAtPath<CharacterData>(CharacterHunterPath),
                AssetDatabase.LoadAssetAtPath<CharacterData>(CharacterMagePath),
                AssetDatabase.LoadAssetAtPath<CharacterData>(CharacterKnightPath)
            };
            Sprite boxSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BoxSpritePath);
            TMP_FontAsset baseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform root = canvas.transform;
                SetAnchoredY(root.Find("GameTitleText"), 340f);
                SetAnchoredY(root.Find("RecordText"), 255f);
                SetAnchoredY(root.Find("PlayButton"), -85f);
                SetAnchoredY(root.Find("QuitButton"), -180f);

                // Tạo khu vực chọn 3 nhân vật ở giữa MainMenu
                Button[] charButtons = EnsureCharacterSelectionCards(root, allCharacters, boxSprite, baseFont);

                MainMenuUI menuUi = Object.FindAnyObjectByType<MainMenuUI>();
                if (menuUi != null)
                {
                    SerializedObject so = new SerializedObject(menuUi);
                    SetObjectArray(so.FindProperty("characters"), allCharacters);
                    SetObjectArray(so.FindProperty("characterButtons"), charButtons);
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(menuUi);
                }

                // Dòng gợi ý phím tắt bên dưới nút Thoát
                GameObject hintObj = GetOrCreateChild(
                    root,
                    "ControlsHintText",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(TextMeshProUGUI));

                RectTransform rect = hintObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0f, -260f);
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

        private static Button[] EnsureCharacterSelectionCards(
            Transform canvasRoot,
            CharacterData[] characters,
            Sprite boxSprite,
            TMP_FontAsset baseFont)
        {
            GameObject containerObj = GetOrCreateChild(
                canvasRoot,
                "CharacterSelectContainer",
                typeof(RectTransform),
                typeof(HorizontalLayoutGroup));

            RectTransform contRect = containerObj.GetComponent<RectTransform>();
            contRect.anchorMin = new Vector2(0.5f, 0.5f);
            contRect.anchorMax = new Vector2(0.5f, 0.5f);
            contRect.pivot = new Vector2(0.5f, 0.5f);
            contRect.anchoredPosition = new Vector2(0f, 95f);
            contRect.sizeDelta = new Vector2(960f, 200f);

            HorizontalLayoutGroup layout = containerObj.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 24f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            Material baseMat = baseFont != null ? baseFont.material : null;
            Button[] buttons = new Button[characters.Length];

            for (int i = 0; i < characters.Length; i++)
            {
                CharacterData c = characters[i];
                GameObject cardObj = GetOrCreateChild(
                    containerObj.transform,
                    $"CharacterCard_{i}",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(Button),
                    typeof(LayoutElement));

                RectTransform cardRect = cardObj.GetComponent<RectTransform>();
                cardRect.sizeDelta = new Vector2(300f, 192f);

                LayoutElement le = cardObj.GetComponent<LayoutElement>();
                le.preferredWidth = 300f;
                le.preferredHeight = 192f;

                Image bgImg = cardObj.GetComponent<Image>();
                if (boxSprite != null)
                {
                    bgImg.sprite = boxSprite;
                    bgImg.type = Image.Type.Sliced;
                }
                bgImg.color = Color.white;
                bgImg.raycastTarget = true;

                Button btn = cardObj.GetComponent<Button>();
                btn.transition = Selectable.Transition.ColorTint;
                ColorBlock cb = btn.colors;
                cb.normalColor = CardNormalColor;
                cb.highlightedColor = CardHighlightColor;
                cb.selectedColor = CardHighlightColor;
                cb.pressedColor = CardPressedColor;
                cb.fadeDuration = 0.08f;
                btn.colors = cb;
                buttons[i] = btn;

                // Preview sprite pixel-art
                GameObject previewObj = GetOrCreateChild(cardObj.transform, "PreviewImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                RectTransform pRect = previewObj.GetComponent<RectTransform>();
                pRect.anchorMin = new Vector2(0.5f, 0.5f);
                pRect.anchorMax = new Vector2(0.5f, 0.5f);
                pRect.anchoredPosition = new Vector2(0f, 44f);
                pRect.sizeDelta = new Vector2(68f, 68f);

                Image pImg = previewObj.GetComponent<Image>();
                pImg.sprite = c.IdleSprite;
                pImg.color = Color.white;
                pImg.preserveAspect = true;
                pImg.raycastTarget = false;

                TMP_Text nameTxt = EnsurePanelText(cardObj.transform, "NameText", new Vector2(0f, -12f), new Vector2(280f, 32f), 24f, FontStyles.Bold, TextPrimary, baseFont, baseMat);
                nameTxt.text = c.CharacterName;

                TMP_Text weaponTxt = EnsurePanelText(cardObj.transform, "WeaponText", new Vector2(0f, -42f), new Vector2(280f, 26f), 18f, FontStyles.Bold, TextGold, baseFont, baseMat);
                weaponTxt.text = c.StartingWeaponLabel;

                TMP_Text statTxt = EnsurePanelText(cardObj.transform, "StatText", new Vector2(0f, -70f), new Vector2(280f, 24f), 15f, FontStyles.Normal, TextSecondary, baseFont, baseMat);
                statTxt.text = c.StatSummary;
            }

            return buttons;
        }

        private static TMP_Text EnsurePanelText(
            Transform parent,
            string name,
            Vector2 anchoredPos,
            Vector2 size,
            float fontSize,
            FontStyles style,
            Color color,
            TMP_FontAsset font,
            Material sharedMat)
        {
            GameObject obj = GetOrCreateChild(parent, name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            if (sharedMat != null) tmp.fontSharedMaterial = sharedMat;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void SetRect(Transform tf, Vector2 anchoredPos, Vector2 size)
        {
            if (tf != null && tf.TryGetComponent(out RectTransform rect))
            {
                rect.anchoredPosition = anchoredPos;
                rect.sizeDelta = size;
            }
        }

        private static void SetAnchoredY(Transform tf, float y)
        {
            if (tf != null && tf.TryGetComponent(out RectTransform rect))
            {
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
            }
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

        private static void ConfigureUiSprite(string assetPath, float pixelsPerUnit)
        {
            if (AssetImporter.GetAtPath(assetPath) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.filterMode = FilterMode.Bilinear;
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
