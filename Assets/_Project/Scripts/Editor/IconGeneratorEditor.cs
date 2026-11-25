using System.IO;
using UnityEditor;
using UnityEngine;

namespace SurvivorGame.EditorTools
{
    public static class IconGeneratorEditor
    {
        private const string OutputFolder = "Assets/_Project/Art/Icons";
        private const int Size = 64;

        [MenuItem("Tools/Survivor Arena/Tao 8 Icon Vu Khi & Nang Cap (PNG)")]
        public static void GenerateAllIcons()
        {
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            SaveIcon("Icon_AutoGun.png", DrawAutoGun());
            SaveIcon("Icon_Orbit.png", DrawOrbit());
            SaveIcon("Icon_AreaPulse.png", DrawAreaPulse());
            SaveIcon("Icon_Damage.png", DrawDamage());
            SaveIcon("Icon_Cooldown.png", DrawCooldown());
            SaveIcon("Icon_Magnet.png", DrawMagnet());
            SaveIcon("Icon_MaxHealth.png", DrawMaxHealth());
            SaveIcon("Icon_MoveSpeed.png", DrawMoveSpeed());

            AssetDatabase.Refresh();

            // Chuyển tự động toàn bộ 8 file PNG sang Sprite (2D and UI)
            string[] files = Directory.GetFiles(OutputFolder, "*.png");
            foreach (string file in files)
            {
                string assetPath = file.Replace("\\", "/");
                if (AssetImporter.GetAtPath(assetPath) is TextureImporter importer)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 64f;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();
                }
            }

            Debug.Log($"[IconGenerator] Da tao xong 8 Sprite Icon tai: {OutputFolder}");
        }

        private static Texture2D CreateBlank(Color bg)
        {
            Texture2D tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    bool border = x < 3 || x >= Size - 3 || y < 3 || y >= Size - 3;
                    tex.SetPixel(x, y, border ? new Color(1f, 1f, 1f, 0.35f) : bg);
                }
            }
            return tex;
        }

        private static void DrawCircle(Texture2D tex, float cx, float cy, float r, Color col, bool outlineOnly = false, float thickness = 3f)
        {
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                    bool inside = outlineOnly ? Mathf.Abs(d - r) <= thickness * 0.5f : d <= r;
                    if (inside) tex.SetPixel(x, y, col);
                }
            }
        }

        private static void DrawRect(Texture2D tex, int x0, int y0, int w, int h, Color col)
        {
            for (int y = y0; y < y0 + h; y++)
                for (int x = x0; x < x0 + w; x++)
                    if (x >= 0 && x < Size && y >= 0 && y < Size)
                        tex.SetPixel(x, y, col);
        }

        private static void DrawChevronRight(Texture2D tex, int tipX, int centerY, int halfHeight, int thickness, Color col)
        {
            for (int dy = -halfHeight; dy <= halfHeight; dy++)
            {
                int y = centerY + dy;
                int xRight = tipX - Mathf.Abs(dy);
                DrawRect(tex, xRight - thickness, y, thickness, 1, col);
            }
        }

        // 1. Đạn Tự Nhắm: Tâm ngắm xanh cyan
        private static Texture2D DrawAutoGun()
        {
            var tex = CreateBlank(new Color(0.08f, 0.16f, 0.24f, 1f));
            Color cyan = new Color(0.22f, 0.88f, 1f, 1f);
            DrawCircle(tex, 32, 32, 18, cyan, true, 4f);
            DrawRect(tex, 30, 8, 4, 48, cyan);
            DrawRect(tex, 8, 30, 48, 4, cyan);
            DrawCircle(tex, 32, 32, 5, Color.white);
            tex.Apply();
            return tex;
        }

        // 2. Cầu Năng Lượng: Quỹ đạo + 3 quả cầu tím sáng
        private static Texture2D DrawOrbit()
        {
            var tex = CreateBlank(new Color(0.14f, 0.10f, 0.26f, 1f));
            Color ring = new Color(0.60f, 0.45f, 1f, 0.7f);
            Color orb = new Color(0.85f, 0.72f, 1f, 1f);
            DrawCircle(tex, 32, 32, 18, ring, true, 3f);
            DrawCircle(tex, 32, 50, 6, orb);
            DrawCircle(tex, 16, 23, 6, orb);
            DrawCircle(tex, 48, 23, 6, orb);
            tex.Apply();
            return tex;
        }

        // 3. Sóng Xung Kích: Các vòng tròn đồng tâm vàng cam
        private static Texture2D DrawAreaPulse()
        {
            var tex = CreateBlank(new Color(0.24f, 0.14f, 0.06f, 1f));
            Color outer = new Color(1f, 0.65f, 0.15f, 1f);
            Color inner = new Color(1f, 0.90f, 0.35f, 1f);
            DrawCircle(tex, 32, 32, 22, outer, true, 3.5f);
            DrawCircle(tex, 32, 32, 13, inner, true, 3.5f);
            DrawCircle(tex, 32, 32, 5, Color.white);
            tex.Apply();
            return tex;
        }

        // 4. Sức Mạnh: Mũi tên đỏ hướng lên
        private static Texture2D DrawDamage()
        {
            var tex = CreateBlank(new Color(0.25f, 0.08f, 0.08f, 1f));
            Color red = new Color(1f, 0.32f, 0.32f, 1f);
            DrawRect(tex, 27, 12, 10, 24, red);
            for (int y = 34; y <= 52; y++)
            {
                int half = (52 - y);
                DrawRect(tex, 32 - half, y, half * 2, 1, red);
            }
            tex.Apply();
            return tex;
        }

        // 5. Giảm Hồi Chiêu: Đồng hồ xanh dương
        private static Texture2D DrawCooldown()
        {
            var tex = CreateBlank(new Color(0.08f, 0.18f, 0.22f, 1f));
            Color col = new Color(0.30f, 0.85f, 0.95f, 1f);
            DrawCircle(tex, 32, 32, 20, col, true, 4f);
            DrawRect(tex, 30, 32, 4, 14, Color.white);
            DrawRect(tex, 32, 30, 11, 4, Color.white);
            tex.Apply();
            return tex;
        }

        // 6. Nam Châm: Hình chữ U đỏ với 2 đầu cực màu bạc
        private static Texture2D DrawMagnet()
        {
            var tex = CreateBlank(new Color(0.10f, 0.14f, 0.26f, 1f));
            Color red = new Color(0.95f, 0.25f, 0.28f, 1f);
            Color tip = new Color(0.86f, 0.92f, 0.98f, 1f);

            // Phần cong chữ U bên dưới
            for (int y = 8; y <= 26; y++)
            {
                for (int x = 14; x <= 50; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(32f, 26f));
                    if (d >= 10f && d <= 18f)
                    {
                        tex.SetPixel(x, y, red);
                    }
                }
            }

            // Hai nhánh thẳng bên trái và phải
            DrawRect(tex, 14, 26, 8, 16, red);
            DrawRect(tex, 42, 26, 8, 16, red);

            // Hai đầu cực màu bạc trên cùng
            DrawRect(tex, 14, 42, 8, 9, tip);
            DrawRect(tex, 42, 42, 8, 9, tip);

            tex.Apply();
            return tex;
        }

        // 7. Máu Tối Đa: Chữ thập hồi máu
        private static Texture2D DrawMaxHealth()
        {
            var tex = CreateBlank(new Color(0.24f, 0.08f, 0.14f, 1f));
            Color pink = new Color(1f, 0.35f, 0.50f, 1f);
            DrawRect(tex, 26, 12, 12, 40, pink);
            DrawRect(tex, 12, 26, 40, 12, pink);
            tex.Apply();
            return tex;
        }

        // 8. Tốc Chạy: Hai mũi tên bứt tốc xanh ngọc-lá kèm vệt gió
        private static Texture2D DrawMoveSpeed()
        {
            var tex = CreateBlank(new Color(0.06f, 0.20f, 0.14f, 1f));
            Color trail = new Color(0.20f, 0.80f, 0.50f, 0.85f);
            Color bright = new Color(0.45f, 1f, 0.65f, 1f);

            // Vệt gió phía sau
            DrawRect(tex, 10, 41, 12, 3, trail);
            DrawRect(tex, 8, 31, 14, 3, trail);
            DrawRect(tex, 10, 21, 12, 3, trail);

            // Hai mũi tên kép hướng phải (>>)
            DrawChevronRight(tex, 36, 32, 16, 6, trail);
            DrawChevronRight(tex, 52, 32, 16, 7, bright);

            tex.Apply();
            return tex;
        }

        private static void SaveIcon(string fileName, Texture2D tex)
        {
            byte[] png = tex.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(OutputFolder, fileName), png);
            Object.DestroyImmediate(tex);
        }
    }
}
