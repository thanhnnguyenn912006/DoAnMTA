using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SurvivorGame.EditorTools
{
    public static class EnvironmentSpriteGeneratorEditor
    {
        private const string OutputFolder = "Assets/_Project/Art/Sprites/Environment";

        [MenuItem("Tools/Survivor Arena/Tao 6 Sprite Trang Tri (Co, Hoa, Da)")]
        public static void GenerateEnvironmentProps()
        {
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            // 1. Bụi cỏ nhỏ (16x16)
            var grassPalette = new Dictionary<char, Color>
            {
                ['L'] = Hex("#558B2F"), // Ngọn cỏ sáng
                ['G'] = Hex("#33691E"), // Thân cỏ
                ['g'] = Hex("#254E17"), // Bóng lá
                ['d'] = Hex("#1B3811"), // Bóng gốc dưới mặt đất
            };

            SaveGrid("Prop_GrassTuft_0.png", new[]
            {
                "................",
                "................",
                "................",
                "................",
                "................",
                ".......L........",
                "....L..G...L....",
                "....G..G..G.....",
                ".L..G..G..G.....",
                "..G.g..G.g...L..",
                "..g.g.Gg.g..G...",
                "...ggggggg.g....",
                "...dddddddd.....",
                "....dddddd......",
                "................",
                "................",
            }, grassPalette);

            // 2. Bụi cỏ rậm (16x16)
            SaveGrid("Prop_GrassTuft_1.png", new[]
            {
                "................",
                "................",
                "................",
                "................",
                "................",
                "................",
                ".....LL.........",
                "....LGGLL.......",
                "...LGGGGGLL.....",
                "..LGGGGGGGGL....",
                "..gGGGgGGGGg....",
                "..ggGgggGggg....",
                "...dddddddd.....",
                "....dddddd......",
                "................",
                "................",
            }, grassPalette);

            // 3. Cụm hoa dại trắng - vàng nhạt (16x16)
            var flowerYellowPalette = new Dictionary<char, Color>
            {
                ['W'] = Hex("#ECEFF1"), // Cánh trắng dịu
                ['Y'] = Hex("#FFD54F"), // Nhụy vàng
                ['G'] = Hex("#388E3C"), // Cuống lá
                ['g'] = Hex("#254E17"), // Bóng lá
                ['d'] = Hex("#1B3811"), // Bóng gốc
            };

            string[] flowerGrid =
            {
                "................",
                "................",
                "................",
                "......W.........",
                ".....WYW........",
                "......W...W.....",
                "......G..WYW....",
                "...W..G...W.....",
                "..WYW.G..G......",
                "...W..gG.G......",
                "...G.gggGg......",
                "....ggggg.......",
                "....ddddd.......",
                "................",
                "................",
                "................",
            };

            SaveGrid("Prop_Flower_White.png", flowerGrid, flowerYellowPalette);

            // 4. Cụm hoa dại hồng phấn dịu (16x16 - tránh trùng màu đỏ gắt của đạn/ngọc)
            var flowerPinkPalette = new Dictionary<char, Color>
            {
                ['W'] = Hex("#EF9A9A"), // Cánh hồng nhạt
                ['Y'] = Hex("#FFF59D"), // Nhụy kem
                ['G'] = Hex("#388E3C"),
                ['g'] = Hex("#254E17"),
                ['d'] = Hex("#1B3811"),
            };

            SaveGrid("Prop_Flower_Pink.png", flowerGrid, flowerPinkPalette);

            // 5. Viên đá tròn có rêu (16x16)
            var rockPalette = new Dictionary<char, Color>
            {
                ['K'] = Hex("#1E2426"), // Viền sẫm
                ['L'] = Hex("#78909C"), // Mặt đá sáng
                ['M'] = Hex("#546E7A"), // Thân đá
                ['D'] = Hex("#37474F"), // Bóng đá
                ['G'] = Hex("#436B2F"), // Vệt rêu
            };

            SaveGrid("Prop_Rock_Small.png", new[]
            {
                "................",
                "................",
                "................",
                "................",
                "................",
                "................",
                ".....KKKKK......",
                "....KLLGGMK.....",
                "...KLLMMMDDK....",
                "...KLMMMMDDK....",
                "...KMMMDDDDK....",
                "....KDDDDDK.....",
                ".....KKKKK......",
                "................",
                "................",
                "................",
            }, rockPalette);

            // 6. Cụm 2 viên đá nhỏ (16x16)
            SaveGrid("Prop_Rock_Cluster.png", new[]
            {
                "................",
                "................",
                "................",
                "................",
                "................",
                "....KKKK........",
                "...KLLGMK.......",
                "..KLLMMMDK..KKK.",
                "..KLMMMDDK.KLLMK",
                "..KMMDDDDKKLMMDK",
                "...KDDDDK.KDDDK.",
                "....KKKK...KKK..",
                "................",
                "................",
                "................",
                "................",
            }, rockPalette);

            AssetDatabase.Refresh();

            string[] files = Directory.GetFiles(OutputFolder, "*.png");
            foreach (string file in files)
            {
                string assetPath = file.Replace("\\", "/");
                if (AssetImporter.GetAtPath(assetPath) is TextureImporter importer)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 16f;
                    importer.filterMode = FilterMode.Point;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();
                }
            }

            Debug.Log($"[EnvironmentSpriteGenerator] Da tao xong 6 Sprite trang tri tai: {OutputFolder}");
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