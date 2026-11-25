using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SurvivorGame.UI;

namespace SurvivorGame.EditorTools
{
    /// <summary>
    /// Công cụ 1-click đồng bộ bảng màu, sprite 9-slice, Material Outline/Underlay cho HUD
    /// và tắt Raycast Target thừa trên cả 2 Scene MainMenu & Gameplay (Bước 1 - Lỗi 15/15).
    /// </summary>
    public static class UiPolishSetupEditor
    {
        private const string UiArtFolder = "Assets/_Project/Art/UI";
        private const string BoxSpritePath = UiArtFolder + "/UI_RoundedBox.png";
        private const string FillSpritePath = UiArtFolder + "/UI_RoundedFill.png";
        private const string FontAssetPath = "Assets/_Project/Art/Fonts/BeVietnamPro-SemiBold SDF.asset";
        private const string HudMaterialPath = "Assets/_Project/Art/Fonts/BeVietnamPro-SemiBold SDF - HUD Outline.mat";
        private const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string GameplayScenePath = "Assets/_Project/Scenes/Gameplay.unity";

        // Bảng màu thống nhất
        private static readonly Color PanelBg = Hex("#141923", 235f / 255f);
        private static readonly Color CameraBg = Hex("#141923", 1f);
        private static readonly Color CardNormal = Hex("#1E293B", 1f);
        private static readonly Color CardHighlight = Hex("#2E3B52", 1f);
        private static readonly Color CardPressed = Hex("#141923", 1f);

        private static readonly Color PrimaryNormal = Hex("#2E7D5B", 1f);
        private static readonly Color PrimaryHighlight = Hex("#38966D", 1f);
        private static readonly Color PrimaryPressed = Hex("#236146", 1f);

        private static readonly Color SecondaryNormal = Hex("#374151", 1f);
        private static readonly Color SecondaryHighlight = Hex("#4B5563", 1f);
        private static readonly Color SecondaryPressed = Hex("#1F2937", 1f);

        private static readonly Color TextPrimary = Hex("#F8FAFC", 1f);
        private static readonly Color TextSecondary = Hex("#CBD5E1", 1f);
        private static readonly Color TextGold = Hex("#FACC15", 1f);
        private static readonly Color SliderTrackColor = Hex("#1E293B", 0.92f);
        private static readonly Color HpFillColor = Hex("#22C55E", 1f);
        private static readonly Color ExpFillColor = Hex("#38BDF8", 1f);
        private static readonly Color BossFillColor = Hex("#EF4444", 1f);

        [MenuItem("Tools/Survivor Arena/Lam Muot UI & 9-Slice (Buoc 1)")]
        public static void ApplyUiPolish()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Dang chay Play Mode", "Vui long tat Play Mode truoc khi chay cong cu sua UI.", "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            string previousScenePath = SceneManager.GetActiveScene().path;

            // 1. Tạo cặp sprite 9-slice (khung bo góc có viền nhẹ + thanh fill bo góc)
            Sprite boxSprite = EnsureNineSliceSprite(
                BoxSpritePath, 32, 6f, 2f,
                new Color(0.84f, 0.87f, 0.92f, 1f), Color.white,
                new Vector4(8f, 8f, 8f, 8f));

            Sprite fillSprite = EnsureNineSliceSprite(
                FillSpritePath, 24, 5f, 0f,
                Color.white, Color.white,
                new Vector4(6f, 6f, 6f, 6f));

            // 2. Tạo Material Preset Outline + Underlay cho chữ HUD
            TMP_FontAsset baseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (baseFont == null)
            {
                Debug.LogError($"[UiPolishSetup] Khong tim thay Font Asset tai: {FontAssetPath}");
                return;
            }

            Material hudMat = EnsureHudOutlineMaterial(baseFont);

            // 3. Áp dụng cho MainMenu & Gameplay
            PolishMainMenuScene(boxSprite, fillSprite, baseFont, hudMat);
            PolishGameplayScene(boxSprite, fillSprite, baseFont, hudMat);

            AssetDatabase.SaveAssets();

            if (!string.IsNullOrEmpty(previousScenePath) && File.Exists(previousScenePath))
            {
                EditorSceneManager.OpenScene(previousScenePath, OpenSceneMode.Single);
            }

            Debug.Log("[UiPolishSetup] Da hoan tat lam muot UI, 9-slice, Outline/Underlay HUD va tat Raycast Target thua tren ca 2 Scene!");
        }

        private static Sprite EnsureNineSliceSprite(
            string assetPath,
            int size,
            float cornerRadius,
            float borderThickness,
            Color fillColor,
            Color borderColor,
            Vector4 border)
        {
            string dir = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float half = size * 0.5f;
            float center = (size - 1) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - center) - (half - cornerRadius - 0.5f);
                    float dy = Mathf.Abs(y - center) - (half - cornerRadius - 0.5f);
                    float dist = Mathf.Sqrt(Mathf.Max(dx, 0f) * Mathf.Max(dx, 0f) + Mathf.Max(dy, 0f) * Mathf.Max(dy, 0f))
                                 + Mathf.Min(Mathf.Max(dx, dy), 0f) - cornerRadius;

                    float alpha = Mathf.Clamp01(0.5f - dist);
                    float rimMix = borderThickness > 0f ? Mathf.Clamp01(dist + borderThickness) : 0f;

                    Color col = Color.Lerp(fillColor, borderColor, rimMix);
                    col.a *= alpha;
                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(assetPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            if (AssetImporter.GetAtPath(assetPath) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                importer.spriteBorder = border;

                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);

                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static Material EnsureHudOutlineMaterial(TMP_FontAsset baseFont)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(HudMaterialPath);
            if (mat == null)
            {
                mat = new Material(baseFont.material);
                AssetDatabase.CreateAsset(mat, HudMaterialPath);
            }
            else
            {
                mat.shader = baseFont.material.shader;
                mat.CopyPropertiesFromMaterial(baseFont.material);
            }

            mat.name = Path.GetFileNameWithoutExtension(HudMaterialPath);
            mat.EnableKeyword("OUTLINE_ON");
            mat.EnableKeyword("UNDERLAY_ON");
            mat.DisableKeyword("UNDERLAY_INNER");

            mat.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0.04f, 0.06f, 0.10f, 1f));
            mat.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.10f);
            mat.SetFloat(ShaderUtilities.ID_OutlineSoftness, 0.08f);

            mat.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, 0.72f));
            mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.30f);
            mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.35f);
            mat.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0.04f);
            mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.22f);

            ShaderUtilities.UpdateShaderRatios(mat);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void PolishMainMenuScene(Sprite boxSprite, Sprite fillSprite, TMP_FontAsset baseFont, Material hudMat)
        {
            Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.backgroundColor = CameraBg;
            }

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;
            Transform root = canvas.transform;

            StyleTmp(root.Find("GameTitleText"), baseFont, hudMat, TextGold);
            StyleTmp(root.Find("RecordText"), baseFont, baseFont.material, TextSecondary);

            StyleButton(root.Find("PlayButton"), boxSprite, PrimaryNormal, PrimaryHighlight, PrimaryPressed, baseFont, TextPrimary);
            StyleButton(root.Find("QuitButton"), boxSprite, SecondaryNormal, SecondaryHighlight, SecondaryPressed, baseFont, TextPrimary);

            Transform volRoot = root.Find("VolumeSettingsUI");
            if (volRoot != null)
            {
                Transform bgmSlider = volRoot.Find("BgmVolumeSlider");
                Transform sfxSlider = volRoot.Find("SfxVolumeSlider");
                SetAnchoredY(bgmSlider, 42f);
                SetAnchoredY(sfxSlider, -28f);

                StyleSlider(bgmSlider, boxSprite, fillSprite, PrimaryNormal, true);
                StyleSlider(sfxSlider, boxSprite, fillSprite, PrimaryNormal, true);
                StyleTmp(volRoot.Find("BgmVolumeSlider/BgmLabel"), baseFont, baseFont.material, TextPrimary);
                StyleTmp(volRoot.Find("SfxVolumeSlider/SfxLabel"), baseFont, baseFont.material, TextPrimary);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void PolishGameplayScene(Sprite boxSprite, Sprite fillSprite, TMP_FontAsset baseFont, Material hudMat)
        {
            Scene scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;
            Transform root = canvas.transform;

            // Sửa toàn bộ sharedMaterial bị trỏ nhầm sang LiberationSans cũ trên mọi TMP trong Canvas
            TMP_Text[] allTexts = root.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < allTexts.Length; i++)
            {
                allTexts[i].font = baseFont;
                allTexts[i].fontSharedMaterial = baseFont.material;
                allTexts[i].raycastTarget = false;
            }

            // 1. HUD trực tiếp trên nền game -> bật Material Outline + Underlay
            StyleSlider(root.Find("ExpBar"), boxSprite, fillSprite, ExpFillColor, false);
            StyleTmp(root.Find("ExpBar/LevelText"), baseFont, hudMat, TextPrimary);

            Transform hud = root.Find("GameplayHUD");
            if (hud != null)
            {
                StyleSlider(hud.Find("PlayerHealthBar"), boxSprite, fillSprite, HpFillColor, false);
                StyleTmp(hud.Find("PlayerHealthBar/HpText"), baseFont, hudMat, TextPrimary);
                StyleTmp(hud.Find("TimerText"), baseFont, hudMat, TextPrimary);
                StyleTmp(hud.Find("ScoreText"), baseFont, hudMat, TextGold);

                Transform killTf = hud.Find("KillCountText");
                if (killTf != null && killTf.TryGetComponent(out TMP_Text killTmp))
                {
                    killTmp.text = "ĐÃ HẠ: 0";
                }
                StyleTmp(killTf, baseFont, hudMat, TextSecondary);

                Transform weaponBarTf = hud.Find("WeaponBarHUD");
                if (weaponBarTf != null)
                {
                    // Căn mép trái (110px) thẳng hàng với PlayerHealthBar
                    if (weaponBarTf.TryGetComponent(out RectTransform wbRect))
                    {
                        wbRect.anchoredPosition = new Vector2(320f, wbRect.anchoredPosition.y);
                    }

                    if (weaponBarTf.TryGetComponent(out WeaponBarHUD wbHud))
                    {
                        SerializedObject so = new SerializedObject(wbHud);
                        so.FindProperty("slotSprite").objectReferenceValue = boxSprite;
                        so.FindProperty("levelTextMaterial").objectReferenceValue = hudMat;
                        so.FindProperty("weaponSlotBgColor").colorValue = new Color(CardNormal.r, CardNormal.g, CardNormal.b, 0.90f);
                        so.FindProperty("passiveSlotBgColor").colorValue = new Color(CardHighlight.r, CardHighlight.g, CardHighlight.b, 0.90f);
                        so.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
            }

            Transform bossPanel = root.Find("BossHealthPanel");
            if (bossPanel != null)
            {
                StyleSlider(bossPanel.Find("BossHealthSlider"), boxSprite, fillSprite, BossFillColor, false);
                StyleTmp(bossPanel.Find("BossNameText"), baseFont, hudMat, TextGold);
                StyleTmp(bossPanel.Find("BossHealthSlider/BossHpText"), baseFont, hudMat, TextPrimary);
            }

            // 2. PausePanel
            Transform pausePanel = root.Find("PausePanel");
            if (pausePanel != null)
            {
                SetPanelBackdrop(pausePanel, PanelBg);
                StyleTmp(pausePanel.Find("PauseTitleText"), baseFont, hudMat, TextGold);
                StyleButton(pausePanel.Find("ResumeButton"), boxSprite, PrimaryNormal, PrimaryHighlight, PrimaryPressed, baseFont, TextPrimary);
                StyleButton(pausePanel.Find("PauseRestartButton"), boxSprite, SecondaryNormal, SecondaryHighlight, SecondaryPressed, baseFont, TextPrimary);
                StyleButton(pausePanel.Find("PauseMainMenuButton"), boxSprite, SecondaryNormal, SecondaryHighlight, SecondaryPressed, baseFont, TextPrimary);

                Transform bgmSlider = pausePanel.Find("BgmVolumeSlider");
                Transform sfxSlider = pausePanel.Find("SfxVolumeSlider");
                SetAnchoredY(bgmSlider, -210f);
                SetAnchoredY(sfxSlider, -285f);

                StyleSlider(bgmSlider, boxSprite, fillSprite, PrimaryNormal, true);
                StyleSlider(sfxSlider, boxSprite, fillSprite, PrimaryNormal, true);
                StyleTmp(pausePanel.Find("BgmVolumeSlider/BgmLabel"), baseFont, baseFont.material, TextPrimary);
                StyleTmp(pausePanel.Find("SfxVolumeSlider/SfxLabel"), baseFont, baseFont.material, TextPrimary);
            }

            // 3. UpgradePanel & 3 thẻ nâng cấp
            Transform upgradePanel = root.Find("UpgradePanel");
            if (upgradePanel != null)
            {
                SetPanelBackdrop(upgradePanel, PanelBg);
                StyleTmp(upgradePanel.Find("HeaderText"), baseFont, hudMat, TextGold);

                Transform cardsContainer = upgradePanel.Find("CardsContainer");
                if (cardsContainer != null)
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        Transform card = cardsContainer.Find($"UpgradeCard_{i}");
                        if (card == null) continue;

                        StyleButton(card, boxSprite, CardNormal, CardHighlight, CardPressed, baseFont, TextPrimary);
                        StyleTmp(card.Find("TitleText"), baseFont, baseFont.material, TextPrimary);
                        StyleTmp(card.Find("LevelText"), baseFont, baseFont.material, TextGold);
                        StyleTmp(card.Find("DescText"), baseFont, baseFont.material, TextSecondary);

                        // Tách DescText xuống dưới IconImage (Icon nằm ở y=18, cao 64 => đáy -14) để không đè chữ
                        Transform descTf = card.Find("DescText");
                        if (descTf != null && descTf.TryGetComponent(out RectTransform descRect))
                        {
                            descRect.anchoredPosition = new Vector2(0f, -110f);
                            descRect.sizeDelta = new Vector2(330f, 150f);
                        }

                        Transform iconTf = card.Find("IconImage");
                        if (iconTf != null && iconTf.TryGetComponent(out Image iconImg))
                        {
                            iconImg.raycastTarget = false;
                        }
                    }
                }
            }

            // 4. EndGamePanel (Game Over / Victory)
            Transform endPanel = root.Find("EndGamePanel");
            if (endPanel != null)
            {
                SetPanelBackdrop(endPanel, PanelBg);
                StyleTmp(endPanel.Find("EndTitleText"), baseFont, hudMat, Hex("#EF4444", 1f));
                StyleTmp(endPanel.Find("EndStatsText"), baseFont, baseFont.material, TextPrimary);
                StyleButton(endPanel.Find("EndRestartButton"), boxSprite, PrimaryNormal, PrimaryHighlight, PrimaryPressed, baseFont, TextPrimary);
                StyleButton(endPanel.Find("EndMainMenuButton"), boxSprite, SecondaryNormal, SecondaryHighlight, SecondaryPressed, baseFont, TextPrimary);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void SetPanelBackdrop(Transform panelTf, Color color)
        {
            if (panelTf != null && panelTf.TryGetComponent(out Image img))
            {
                img.color = color;
                img.raycastTarget = true;
            }
        }

        private static void StyleButton(
            Transform btnTf,
            Sprite boxSprite,
            Color normal,
            Color highlighted,
            Color pressed,
            TMP_FontAsset font,
            Color textColor)
        {
            if (btnTf == null) return;

            if (btnTf.TryGetComponent(out Image img))
            {
                if (boxSprite != null)
                {
                    img.sprite = boxSprite;
                    img.type = Image.Type.Sliced;
                }
                img.color = Color.white;
                img.raycastTarget = true;
            }

            if (btnTf.TryGetComponent(out Button btn))
            {
                btn.transition = Selectable.Transition.ColorTint;
                ColorBlock cb = btn.colors;
                cb.normalColor = normal;
                cb.highlightedColor = highlighted;
                cb.pressedColor = pressed;
                cb.selectedColor = highlighted;
                cb.disabledColor = new Color(normal.r, normal.g, normal.b, 0.45f);
                cb.colorMultiplier = 1f;
                cb.fadeDuration = 0.08f;
                btn.colors = cb;
            }

            Transform labelTf = btnTf.Find("Text (TMP)");
            if (labelTf != null)
            {
                StyleTmp(labelTf, font, font.material, textColor);
            }
        }

        private static void StyleSlider(Transform sliderTf, Sprite trackSprite, Sprite fillSprite, Color fillColor, bool interactive)
        {
            if (sliderTf == null) return;

            Transform bgTf = sliderTf.Find("Background");
            if (bgTf != null && bgTf.TryGetComponent(out Image bgImg))
            {
                if (trackSprite != null)
                {
                    bgImg.sprite = trackSprite;
                    bgImg.type = Image.Type.Sliced;
                }
                bgImg.color = SliderTrackColor;
                bgImg.raycastTarget = interactive;
            }

            Transform fillTf = sliderTf.Find("Fill Area/Fill");
            if (fillTf != null && fillTf.TryGetComponent(out Image fillImg))
            {
                if (fillSprite != null)
                {
                    fillImg.sprite = fillSprite;
                    fillImg.type = Image.Type.Sliced;
                }
                fillImg.color = fillColor;
                fillImg.raycastTarget = false;
            }

            Transform handleTf = sliderTf.Find("Handle Slide Area/Handle");
            if (handleTf != null && handleTf.TryGetComponent(out Image handleImg))
            {
                handleImg.color = TextPrimary;
                handleImg.raycastTarget = interactive;
            }
        }

        private static void StyleTmp(Transform tf, TMP_FontAsset font, Material sharedMat, Color color)
        {
            if (tf == null || !tf.TryGetComponent(out TMP_Text tmp)) return;

            tmp.font = font;
            if (sharedMat != null)
            {
                tmp.fontSharedMaterial = sharedMat;
            }
            tmp.color = color;
            tmp.raycastTarget = false;
        }

        private static void SetAnchoredY(Transform tf, float y)
        {
            if (tf != null && tf.TryGetComponent(out RectTransform rect))
            {
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
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
