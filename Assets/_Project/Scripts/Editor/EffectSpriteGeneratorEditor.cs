using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SurvivorGame.EditorTools
{
    public static class EffectSpriteGeneratorEditor
    {
        private const string OutputFolder = "Assets/_Project/Art/Sprites/Effects";

        [MenuItem("Tools/Survivor Arena/Tao 4 Sprite Orbit, EXP va AreaPulse")]
        public static void GenerateAllEffects()
        {
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            // 1. Cầu Năng Lượng Orbit (16x16): Cầu plasma xanh điện lõi trắng
            var orbPalette = new Dictionary<char, Color>
            {
                ['K'] = Hex("#0D1B2A"), // Viền xanh đen
                ['B'] = Hex("#2979FF"), // Hào quang xanh dương
                ['C'] = Hex("#00E5FF"), // Năng lượng xanh ngọc
                ['W'] = Hex("#B2EBF2"), // Lớp sáng cận lõi
                ['L'] = Hex("#FFFFFF"), // Lõi plasma trắng
            };

            SaveGrid("Orb_Energy.png", new[]
            {
                ".....KKKKKK.....",
                "...KKBBBBBBKK...",
                "..KBBCCCCCCBBK..",
                ".KBCCWWWWWWCCBK.",
                ".KBCWCCCCCCWCBK.",
                "KBCWCCWWWWCCWCBK",
                "KBCWCWWWWWWCWCBK",
                "KBCWCWWLLWWCWCBK",
                "KBCWCWWLLWWCWCBK",
                "KBCWCWWWWWWCWCBK",
                "KBCWCCWWWWCCWCBK",
                ".KBCWCCCCCCWCBK.",
                ".KBCCWWWWWWCCBK.",
                "..KBBCCCCCCBBK..",
                "...KKBBBBBBKK...",
                ".....KKKKKK.....",
            }, orbPalette);

            // 2. Ngọc EXP Xanh Ngọc (12x12)
            var expBluePalette = new Dictionary<char, Color>
            {
                ['K'] = Hex("#0B1A26"), // Viền tối
                ['W'] = Hex("#FFFFFF"), // Điểm sáng phản chiếu
                ['C'] = Hex("#80DEEA"), // Mặt sáng
                ['B'] = Hex("#00B0FF"), // Thân ngọc xanh
                ['D'] = Hex("#01579B"), // Mặt tối tạo khối
            };

            string[] gemGrid =
            {
                ".....KK.....",
                "....KWWK....",
                "...KWWCCK...",
                "..KWWCCCCK..",
                ".KWWCCBBBBK.",
                ".KWCCCBBBBK.",
                ".KCCCCBBDDK.",
                "..KCCBBDDK..",
                "...KBBDDK...",
                "....KDDK....",
                ".....KK.....",
                "............",
            };

            SaveGrid("Exp_Gem_Blue.png", gemGrid, expBluePalette);

            // 3. Ngọc EXP Vàng Kim (12x12 - dùng cho ngọc giá trị cao / Boss)
            var expGoldPalette = new Dictionary<char, Color>
            {
                ['K'] = Hex("#261606"),
                ['W'] = Hex("#FFFDE7"),
                ['C'] = Hex("#FFE082"),
                ['B'] = Hex("#FFB300"),
                ['D'] = Hex("#E65100"),
            };

            SaveGrid("Exp_Gem_Gold.png", gemGrid, expGoldPalette);
            var expWhitePalette = new Dictionary<char, Color>
            {
                ['K'] = Hex("#222222"), // Viền sẫm
                ['W'] = Hex("#FFFFFF"), // Điểm sáng phản chiếu
                ['C'] = Hex("#F2F2F2"), // Mặt sáng
                ['B'] = Hex("#D6D6D6"), // Thân ngọc
                ['D'] = Hex("#8A8A8A"), // Mặt tối tạo khối
            };

            SaveGrid("Exp_Gem_White.png", gemGrid, expWhitePalette);
            // 4. Sóng Xung Kích AreaPulse (64x64): Vòng sóng điện sắc nét, rỗng tâm
            GenerateAreaPulseRing("VFX_AreaPulse.png", 64);

            AssetDatabase.Refresh();

            ConfigureSprite("Orb_Energy.png", 20f);
            ConfigureSprite("Exp_Gem_Blue.png", 24f);
            ConfigureSprite("Exp_Gem_Gold.png", 20f);
            ConfigureSprite("Exp_Gem_White.png", 24f);
            ConfigureSprite("VFX_AreaPulse.png", 64f); // 64x64 với PPU 64 = đúng 1x1 đơn vị Unity

            Debug.Log($"[EffectSpriteGenerator] Da tao xong 4 Sprite tai: {OutputFolder}");
        }

        private static void GenerateAreaPulseRing(string fileName, int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);

            Color outerRim = new Color(0.08f, 0.40f, 0.78f, 0.92f);
            Color brightEdge = new Color(0.90f, 0.99f, 1.00f, 1.00f);
            Color cyanBand = new Color(0.00f, 0.90f, 1.00f, 0.85f);
            Color innerWave = new Color(0.16f, 0.52f, 1.00f, 1.00f);
            Color innerFill = new Color(0.00f, 0.85f, 1.00f, 0.08f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center.x;
                    float dy = y - center.y;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);
                    float ripple = Mathf.Sin(angle * 12f) * 0.9f;

                    Color col = Color.clear;

                    if (dist <= 31.2f)
                    {
                        if (dist >= 29.8f)
                        {
                            col = outerRim;
                        }
                        else if (dist >= 27.4f)
                        {
                            col = brightEdge;
                        }
                        else if (dist >= 23.8f + ripple * 0.6f)
                        {
                            col = cyanBand;
                        }
                        else if (dist >= 17.5f + ripple)
                        {
                            float t = Mathf.InverseLerp(17.5f + ripple, 23.8f + ripple * 0.6f, dist);
                            col = innerWave;
                            col.a = Mathf.Lerp(0.10f, 0.48f, t);
                        }
                        else
                        {
                            col = innerFill;
                        }
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            File.WriteAllBytes(Path.Combine(OutputFolder, fileName), tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static void ConfigureSprite(string fileName, float pixelsPerUnit)
        {
            string assetPath = Path.Combine(OutputFolder, fileName).Replace("\\", "/");
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

        private static void SaveGrid(string fileName, string[] grid, Dictionary<char, Color> palette)
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

            File.WriteAllBytes(Path.Combine(OutputFolder, fileName), tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out Color c) ? c : Color.magenta;
        }
    }
}