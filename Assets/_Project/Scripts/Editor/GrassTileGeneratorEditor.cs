using System.IO;
using UnityEditor;
using UnityEngine;

namespace SurvivorGame.EditorTools
{
    public static class GrassTileGeneratorEditor
    {
        private const string OutputFolder = "Assets/_Project/Art/Sprites";
        private const string FileName = "Tile_Grass.png";
        private const int Size = 64;

        [MenuItem("Tools/Survivor Arena/Tao Anh O Co Nen (Tile_Grass.png)")]
        public static void GenerateGrassTile()
        {
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            Texture2D tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            Random.State oldState = Random.state;
            Random.InitState(2026); // Cố định seed để họa tiết đều và đẹp

            // Tông cỏ đêm dịu mắt để làm nổi bật nhân vật, đạn và ngọc EXP
            Color baseDark = new Color(0.11f, 0.20f, 0.14f, 1f);
            Color baseMid = new Color(0.13f, 0.23f, 0.16f, 1f);
            Color shade = new Color(0.09f, 0.17f, 0.12f, 1f);
            Color bladeLight = new Color(0.18f, 0.31f, 0.20f, 1f);
            Color pebble = new Color(0.16f, 0.20f, 0.18f, 1f);

            // 1. Nền lấm tấm nhẹ
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float r = Random.value;
                    Color c = r < 0.18f ? shade : (r < 0.38f ? baseMid : baseDark);
                    tex.SetPixel(x, y, c);
                }
            }

            // 2. Rải các cụm ngọn cỏ nhỏ (wrap cạnh để lát liền mạch không lộ đường nối)
            for (int i = 0; i < 36; i++)
            {
                int bx = Random.Range(0, Size);
                int by = Random.Range(0, Size);
                SetWrappedPixel(tex, bx, by, shade);
                SetWrappedPixel(tex, bx, by + 1, bladeLight);
                SetWrappedPixel(tex, bx + 1, by + 1, baseMid);
                if (Random.value > 0.5f)
                {
                    SetWrappedPixel(tex, bx - 1, by + 2, bladeLight);
                }
            }

            // 3. Điểm vài hạt đất/sỏi nhỏ tạo chiều sâu
            for (int i = 0; i < 12; i++)
            {
                int px = Random.Range(0, Size);
                int py = Random.Range(0, Size);
                SetWrappedPixel(tex, px, py, pebble);
                SetWrappedPixel(tex, px + 1, py, shade);
            }

            tex.Apply();
            Random.state = oldState;

            string fullPath = Path.Combine(OutputFolder, FileName);
            File.WriteAllBytes(fullPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.Refresh();

            // Tự động thiết lập Sprite chuẩn cho SpriteDrawMode.Tiled
            string assetPath = fullPath.Replace("\\", "/");
            if (AssetImporter.GetAtPath(assetPath) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32f; // Mỗi ô cỏ rộng 2x2 đơn vị trong Unity
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;

                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);

                importer.SaveAndReimport();
            }

            Debug.Log($"[GrassTileGenerator] Da tao xong anh o co tai: {assetPath}");
        }

        private static void SetWrappedPixel(Texture2D tex, int x, int y, Color col)
        {
            int wx = (x % Size + Size) % Size;
            int wy = (y % Size + Size) % Size;
            tex.SetPixel(wx, wy, col);
        }
    }
}