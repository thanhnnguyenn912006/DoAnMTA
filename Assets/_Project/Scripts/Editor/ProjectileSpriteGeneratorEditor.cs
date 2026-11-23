using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SurvivorGame.EditorTools
{
    public static class ProjectileSpriteGeneratorEditor
    {
        private const string OutputFolder = "Assets/_Project/Art/Sprites/Projectiles";

        [MenuItem("Tools/Survivor Arena/Tao 3 Sprite Dan (Player, Ranged, Boss)")]
        public static void GenerateAllProjectiles()
        {
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            // 1. Đạn Player (Thợ săn): Đầu đạn vàng đồng phát sáng, hướng sang PHẢI (12x12)
            var playerBulletPalette = new Dictionary<char, Color>
            {
                ['K'] = Hex("#1A120B"), // Viền đậm
                ['O'] = Hex("#FF7043"), // Đuôi lửa cam
                ['Y'] = Hex("#FFD54F"), // Thân đạn vàng kim
                ['W'] = Hex("#FFFFFF"), // Lõi trắng nóng
            };

            SaveGrid("Projectile_Player.png", new[]
            {
                "............",
                "............",
                "............",
                "....KKKKK...",
                "..KKOYYYYKK.",
                ".KOOYWWWWYYK",
                ".KOOYWWWWYYK",
                "..KKOYYYYKK.",
                "....KKKKK...",
                "............",
                "............",
                "............",
            }, playerBulletPalette);

            // 2. Đạn Quái Bắn Xa (Phù Thủy Xương): Cầu phép xanh ngọc viền tím (12x12)
            var rangedBulletPalette = new Dictionary<char, Color>
            {
                ['K'] = Hex("#1A0F2E"), // Viền tím đen
                ['P'] = Hex("#AB47BC"), // Hào quang tím
                ['C'] = Hex("#00E5FF"), // Năng lượng xanh ngọc
                ['W'] = Hex("#E0F7FA"), // Tâm sáng băng
            };

            SaveGrid("Projectile_Ranged.png", new[]
            {
                "....KKKK....",
                "..KKPPPPKK..",
                ".KPPCCCCPPK.",
                ".KPCCCCCCPK.",
                "KPCCWWWWCCPK",
                "KPCWWWWWWCPK",
                "KPCWWWWWWCPK",
                "KPCCWWWWCCPK",
                ".KPCCCCCCPK.",
                ".KPPCCCCPPK.",
                "..KKPPPPKK..",
                "....KKKK....",
            }, rangedBulletPalette);

            // 3. Đạn Boss (Chúa Tể Bóng Tối): Hỏa cầu hắc ám cỡ lớn có gai lửa (16x16)
            var bossBulletPalette = new Dictionary<char, Color>
            {
                ['K'] = Hex("#12060D"), // Viền đen sâu
                ['D'] = Hex("#880E4F"), // Hắc hỏa đỏ mận
                ['R'] = Hex("#FF1744"), // Lửa đỏ rực
                ['O'] = Hex("#FF6D00"), // Lửa cam
                ['Y'] = Hex("#FFD54F"), // Tâm vàng
                ['W'] = Hex("#FFFDE7"), // Lõi trắng nóng
            };

            SaveGrid("Projectile_Boss.png", new[]
            {
                "......KKKK......",
                ".....KDRRDK.....",
                "..KK.KDRRDK.KK..",
                ".KDDKRROORRKDDK.",
                ".KDRRROOOORRRDK.",
                "KKDRROOYYOORRDKK",
                "KDRROOYYYYOORRDK",
                "KDRROYYWWYYORRDK",
                "KDRROYYWWYYORRDK",
                "KDRROOYYYYOORRDK",
                "KKDRROOYYOORRDKK",
                ".KDRRROOOORRRDK.",
                ".KDDKRROORRKDDK.",
                "..KK.KDRRDK.KK..",
                ".....KDRRDK.....",
                "......KKKK......",
            }, bossBulletPalette);

            AssetDatabase.Refresh();

            // Cấu hình Import chuẩn Pixel Art với tỷ lệ kích thước phù hợp từng loại đạn
            ConfigureSprite("Projectile_Player.png", 20f);
            ConfigureSprite("Projectile_Ranged.png", 20f);
            ConfigureSprite("Projectile_Boss.png", 16f);

            Debug.Log($"[ProjectileSpriteGenerator] Da tao xong 3 Sprite dan tai: {OutputFolder}");
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