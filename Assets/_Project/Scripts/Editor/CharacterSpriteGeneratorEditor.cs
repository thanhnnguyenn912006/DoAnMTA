using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SurvivorGame.EditorTools
{
    public static class CharacterSpriteGeneratorEditor
    {
        private const string OutputFolder = "Assets/_Project/Art/Sprites/Characters";

        [MenuItem("Tools/Survivor Arena/Tao Bo Sprite Nhan Vat & Quai (12 PNG)")]
        public static void GenerateAllCharacters()
        {
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            // 1. Player: Thợ săn cầm súng (16x16)
            var playerPalette = new Dictionary<char, Color>
            {
                ['K'] = Hex("#181425"), // Viền tối
                ['H'] = Hex("#4E342E"), // Mũ nâu đậm
                ['h'] = Hex("#795548"), // Mũ nâu sáng
                ['R'] = Hex("#D32F2F"), // Dải mũ & khăn đỏ
                ['S'] = Hex("#FFCC99"), // Da
                ['s'] = Hex("#E0A070"), // Bóng da
                ['E'] = Hex("#111111"), // Mắt
                ['C'] = Hex("#2C3E50"), // Áo khoác thợ săn
                ['c'] = Hex("#455A64"), // Nếp áo sáng
                ['B'] = Hex("#5D4037"), // Thắt lưng & ủng
                ['Y'] = Hex("#FFD54F"), // Khóa vàng
                ['G'] = Hex("#B0BEC5"), // Nòng súng sáng
                ['g'] = Hex("#546E7A"), // Thân súng tối
                ['W'] = Hex("#8D6E63"), // Báng súng gỗ
            };

            SaveGrid("Player_Hunter_0.png", new[]
            {
                ".....KKKKKK.....",
                "....KhhhhhhK....",
                "...KHHHHHHHHK...",
                "..KKRRRRRRRRKK..",
                ".KHHHHHHHHHHHHK.",
                "..KKKSSSSSSKKK..",
                "....KsSSSESK....",
                "....KRRRRRRK....",
                "...KCCcCCcCK....",
                "..KCCcCCKKgGGGK.",
                "..KCCcCWWSSggKK.",
                "...KBBYBBBBK....",
                "...KCCCCCCCK....",
                "...KCCK.KCCK....",
                "..KBBK...KBBK...",
                "..KKKK...KKKK...",
            }, playerPalette);

            SaveGrid("Player_Hunter_1.png", new[]
            {
                "................",
                ".....KKKKKK.....",
                "....KhhhhhhK....",
                "...KHHHHHHHHK...",
                "..KKRRRRRRRRKK..",
                ".KHHHHHHHHHHHHK.",
                "..KKKsSSSESKKK..",
                "....KRRRRRRK....",
                "...KCCcCCcCK....",
                "..KCCcCCKKgGGGK.",
                "..KCCcCWWSSggKK.",
                "...KBBYBBBBK....",
                "...KCCCCCCCK....",
                "....KCCKKCCK....",
                "....KBBKKBBK....",
                "....KKKKKKKK....",
            }, playerPalette);

            // 2. Normal Enemy: Zombie (16x16)
            var zombiePalette = new Dictionary<char, Color>
            {
                ['K'] = Hex("#141914"),
                ['G'] = Hex("#689F38"), // Da xanh zombie
                ['g'] = Hex("#4B7A26"), // Bóng da
                ['L'] = Hex("#9CCC65"), // Đỉnh đầu sáng
                ['R'] = Hex("#E53935"), // Mắt đỏ / vết máu
                ['T'] = Hex("#546E7A"), // Áo rách
                ['t'] = Hex("#37474F"),
                ['P'] = Hex("#4E342E"), // Quần nâu
                ['p'] = Hex("#3E2723"),
            };

            SaveGrid("Enemy_Zombie_0.png", new[]
            {
                "................",
                "....KKKKKKK.....",
                "...KLLGGGGGK....",
                "...KLGGGGGgK....",
                "...KGGGRGGRK....",
                "...KgGGGGggK....",
                "....KggKKgK.....",
                "...KKTTTTTKK....",
                "..KTtTTTtKGGGK..",
                "..KTtTTTtKggKK..",
                "...KTTtRTtK.....",
                "...KPPPPPPK.....",
                "...KPPKKPPK.....",
                "..KPPK..KPPK....",
                "..KppK..KppK....",
                "..KKKK..KKKK....",
            }, zombiePalette);

            SaveGrid("Enemy_Zombie_1.png", new[]
            {
                "................",
                "................",
                "....KKKKKKK.....",
                "...KLLGGGGGK....",
                "...KLGGGGGgK....",
                "...KGGGRGGRK....",
                "...KgGGGGggK....",
                "....KggKKgK.....",
                "...KKTTTTTKKKK..",
                "..KTtTTTtKGGgK..",
                "..KTTtRTtKKKK...",
                "...KPPPPPPK.....",
                "...KPPKKPPK.....",
                "...KPPKKPPK.....",
                "...KppKKppK.....",
                "...KKKKKKKK.....",
            }, zombiePalette);

            // 3. Fast Enemy: Dog / Chó hoang (16x16)
            var dogPalette = new Dictionary<char, Color>
            {
                ['K'] = Hex("#1A120E"),
                ['B'] = Hex("#8D5524"), // Lông nâu
                ['b'] = Hex("#5C3317"), // Lông tối
                ['L'] = Hex("#D7A46B"), // Bụng & mõm sáng
                ['R'] = Hex("#FF3D00"), // Mắt dữ
                ['W'] = Hex("#FFFFFF"), // Nanh trắng
                ['N'] = Hex("#212121"), // Mũi đen
            };

            SaveGrid("Enemy_Dog_0.png", new[]
            {
                "................",
                "................",
                "................",
                ".........KK.KK..",
                "........KBBKBBK.",
                ".KK....KBBBBBBK.",
                "KBBK..KBBBRBBNK.",
                ".KBBKKBBBBLLBBK.",
                "..KBBBBBBBKWKK..",
                "..KBBBBBBBBBK...",
                "..KbBLLLLLBbK...",
                ".KBBK.....KBBK..",
                "KbbK.......KbbK.",
                "KKK.........KKK.",
                "................",
                "................",
            }, dogPalette);

            SaveGrid("Enemy_Dog_1.png", new[]
            {
                "................",
                "................",
                ".........KK.KK..",
                "........KBBKBBK.",
                "..KK...KBBBBBBK.",
                ".KBBK.KBBBRBBNK.",
                "..KBBKBBBBLLBBK.",
                "..KBBBBBBBKWKK..",
                "..KBBBBBBBBBK...",
                "...KbBLLLLbK....",
                "....KBBKKBBK....",
                ".....KbbKbbK....",
                ".....KKKKKKK....",
                "................",
                "................",
                "................",
            }, dogPalette);

            // 4. Tank Enemy: Khá béo (20x20)
            var tankPalette = new Dictionary<char, Color>
            {
                ['K'] = Hex("#1A1420"),
                ['S'] = Hex("#D7A484"), // Da
                ['s'] = Hex("#B07B5C"), // Bóng da / nếp bụng
                ['L'] = Hex("#F0C8AC"), // Bụng tròn sáng
                ['E'] = Hex("#E53935"), // Mắt đỏ
                ['A'] = Hex("#607D8B"), // Giáp vai sắt
                ['a'] = Hex("#37474F"),
                ['B'] = Hex("#4E342E"), // Đai lưng & giày
                ['Y'] = Hex("#FFB300"), // Khóa vàng
                ['P'] = Hex("#3E2723"), // Quần
            };

            SaveGrid("Enemy_Tank_0.png", new[]
            {
                "....................",
                ".......KKKKKK.......",
                "......KSSSSSSK......",
                ".....KSSSESESK......",
                ".....KssSSSSsK......",
                "...KKKAAKKKKAAKKK...",
                "..KAAaSSSSSSSSaAAK..",
                ".KAAaSSLLLLLLSSaAAK.",
                ".KSSKSLLLLLLLLSKSSK.",
                ".KSSKSLLLLLLLLSKSSK.",
                ".KssKSSLLLLLLSSKssK.",
                "..KKKssSSssSSssKKK..",
                "..KBBBBBBYYBBBBBBK..",
                "..KPPPPPPPPPPPPPPK..",
                "...KPPPPKKKKPPPPK...",
                "...KPPPK....KPPPK...",
                "...KPPPK....KBBBK...",
                "...KBBBK....KKKKK...",
                "...KKKKK............",
                "....................",
            }, tankPalette);

            SaveGrid("Enemy_Tank_1.png", new[]
            {
                "....................",
                "....................",
                ".......KKKKKK.......",
                "......KSSSSSSK......",
                ".....KSSSESESK......",
                ".....KssSSSSsK......",
                "...KKKAAKKKKAAKKK...",
                "..KAAaSSSSSSSSaAAK..",
                ".KAAaSSLLLLLLSSaAAK.",
                ".KSSKSLLLLLLLLSKSSK.",
                ".KSSKSLLLLLLLLSKSSK.",
                ".KssKSSLLLLLLSSKssK.",
                "..KKKssSSssSSssKKK..",
                "..KBBBBBBYYBBBBBBK..",
                "..KPPPPPPPPPPPPPPK..",
                "...KPPPPKKKKPPPPK...",
                "...KBBBK....KPPPK...",
                "...KKKKK....KBBBK...",
                "............KKKKK...",
                "....................",
            }, tankPalette);

            // 5. Ranged Enemy (Tặng kèm cho Enemy_Ranged): Phù Thủy Xương (16x16)
            var rangedPalette = new Dictionary<char, Color>
            {
                ['K'] = Hex("#140F1E"),
                ['P'] = Hex("#7B1FA2"), // Áo choàng tím
                ['p'] = Hex("#4A148C"), // Áo tối
                ['L'] = Hex("#CE93D8"), // Viền áo
                ['E'] = Hex("#00E5FF"), // Mắt & ngọc phép xanh
                ['C'] = Hex("#E0F7FA"), // Tâm ngọc sáng
                ['W'] = Hex("#6D4C41"), // Trượng gỗ
            };

            SaveGrid("Enemy_Ranged_0.png", new[]
            {
                "................",
                ".....KKKK.......",
                "....KPPPPK...KK.",
                "...KPLPPPLK.KCCK",
                "...KPKEKEPK.KEEK",
                "...KPKKKKPK..KK.",
                "...KPPPPPPK..KW.",
                "..KPPpPPpPPK.KW.",
                "..KPpPPPPPPKKWK.",
                "..KPpPLLLpPK.KW.",
                "..KPPpPPpPPK.KW.",
                "..KPPPPPPPPK.KW.",
                ".KPPpPPPPpPPKKW.",
                ".KppPPPPPPppKKK.",
                ".KKKKKKKKKKKK...",
                "................",
            }, rangedPalette);

            SaveGrid("Enemy_Ranged_1.png", new[]
            {
                ".....KKKK....KK.",
                "....KPPPPK..KEEK",
                "...KPLPPPLK.KCCK",
                "...KPKEKEPK.KEEK",
                "...KPKKKKPK..KK.",
                "...KPPPPPPK..KW.",
                "..KPPpPPpPPK.KW.",
                "..KPpPPPPPPKKWK.",
                "..KPpPLLLpPK.KW.",
                "..KPPpPPpPPK.KW.",
                "..KPPPPPPPPK.KW.",
                "..KppPPPPppK.KW.",
                "..KppPppPppK.KK.",
                "..KKKKKKKKKK....",
                "................",
                "................",
            }, rangedPalette);

            // 6. Boss: Chúa Tể Bóng Tối Ngầu (24x24 - Sừng cong, giáp hắc diện, áo choàng đỏ, đại kiếm rực lửa)
            var bossPalette = new Dictionary<char, Color>
            {
                ['K'] = Hex("#0D0814"), // Viền đen sâu
                ['A'] = Hex("#2A2438"), // Giáp hắc diện
                ['a'] = Hex("#4E4466"), // Cạnh giáp sáng
                ['H'] = Hex("#EEEEEE"), // Sừng cong sáng
                ['h'] = Hex("#9E9E9E"), // Bóng sừng
                ['R'] = Hex("#FF1744"), // Mắt đỏ, lưỡi kiếm & lõi ngực rực lửa
                ['r'] = Hex("#880E4F"), // Áo choàng đỏ sẫm
                ['Y'] = Hex("#FFD54F"), // Vương miện & chuôi kiếm vàng
                ['W'] = Hex("#FFCDD2"), // Mũi kiếm trắng hồng
            };

            SaveGrid("Enemy_Boss_0.png", new[]
            {
                "....KK..........KK......",
                "...KHHK........KHHK.....",
                "...KhHHK..KK..KHHhK.....",
                "....KhHHKKYYKKHHhK......",
                ".....KKAAAAAAAAKK.......",
                "......KAaAAAAaAK........",
                "......KAKRRKRRKK....KK..",
                "......KAKKKKKKAK...KWWK.",
                "...KKKAAaYAAYaAAKK.KWRK.",
                "..KAAaAAaAAAAaAAaAKKWRK.",
                ".KrrKAAAaYAAYaAAAK.KWRK.",
                ".KrrKAAaAAYYAAaAAK.KWRK.",
                "KrrRKAAAARRRRAAAAK.KWRK.",
                "KrRrKAAaAAYYAAaAAKKKYYKK",
                "KrRr.KAAAAAAAAAAKAAKYYK.",
                "KrrK.KAAYAAAAYAAKAAKAK..",
                "KrrK..KAAAAAAAAK...KK...",
                "KrK...KAAaKKaAAK........",
                ".KK...KAAK..KAAK........",
                "......KaaK..KaaK........",
                ".....KAAK....KAAK.......",
                ".....KYYK....KYYK.......",
                ".....KKKK....KKKK.......",
                "........................",
            }, bossPalette);

            SaveGrid("Enemy_Boss_1.png", new[]
            {
                "........................",
                "....KK..........KK......",
                "...KHHK........KHHK.....",
                "...KhHHK..KK..KHHhK.....",
                "....KhHHKKYYKKHHhK......",
                ".....KKAAAAAAAAKK...KK..",
                "......KAaAAAAaAK...KWWK.",
                "......KAKRRKRRKK...KWRK.",
                "......KAKKKKKKAK...KWRK.",
                "...KKKAAaYAAYaAAKK.KWRK.",
                "..KAAaAAaAAAAaAAaAKKWRK.",
                ".KrrKAAAaYAAYaAAAK.KWRK.",
                "KrrRKAAaAAYYAAaAAKKKYYKK",
                "KrRrKAAAARRRRAAAAKAKYYK.",
                "KrrRKAAaAAYYAAaAAKAKAK..",
                "KrrK.KAAAAAAAAAAK..KK...",
                "KrK..KAAYAAAAYAAK.......",
                "KK....KAAAAAAAAK........",
                "......KAAaKKaAAK........",
                "......KAAK.KAAK.........",
                "......KaaK.KaaK.........",
                "......KYYK.KYYK.........",
                "......KKKK.KKKK.........",
                "........................",
            }, bossPalette);

            AssetDatabase.Refresh();

            // Cấu hình tự động toàn bộ ảnh sang Pixel Art Sprite sắc nét
            string[] files = Directory.GetFiles(OutputFolder, "*.png");
            foreach (string file in files)
            {
                string assetPath = file.Replace("\\", "/");
                if (AssetImporter.GetAtPath(assetPath) is TextureImporter importer)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 16f; // Đồng bộ kích thước pixel trên toàn game
                    importer.filterMode = FilterMode.Point;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();
                }
            }

            Debug.Log($"[CharacterSpriteGenerator] Da tao xong 12 Sprite nhan vat & quai tai: {OutputFolder}");
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
                int y = height - 1 - r; // Hàng 0 là đỉnh ảnh
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