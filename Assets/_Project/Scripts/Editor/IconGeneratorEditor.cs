using System.IO;
using UnityEditor;
using UnityEngine;

namespace SurvivorGame.EditorTools
{
    public static class IconGeneratorEditor
    {
        private const string OutputFolder = "Assets/_Project/Art/Icons";
        private const int Size = 64;

        [MenuItem("Tools/Survivor Arena/Tao 11 Icon Vu Khi, Tien Hoa & Nang Cap (PNG)")]
        public static void GenerateAllIcons()
        {
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            SaveIcon("Icon_AutoGun.png", DrawAutoGun());
            SaveIcon("Icon_Orbit.png", DrawOrbit());
            SaveIcon("Icon_AreaPulse.png", DrawAreaPulse());
            SaveIcon("Icon_AutoGun_Evo.png", DrawAutoGunEvo());
            SaveIcon("Icon_Orbit_Evo.png", DrawOrbitEvo());
            SaveIcon("Icon_AreaPulse_Evo.png", DrawAreaPulseEvo());
            SaveIcon("Icon_Damage.png", DrawDamage());
            SaveIcon("Icon_Cooldown.png", DrawCooldown());
            SaveIcon("Icon_Magnet.png", DrawMagnet());
            SaveIcon("Icon_MaxHealth.png", DrawMaxHealth());
            SaveIcon("Icon_MoveSpeed.png", DrawMoveSpeed());

            AssetDatabase.Refresh();

            // Chuyển tự động toàn bộ 11 file PNG sang Sprite (2D and UI)
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

            Debug.Log($"[IconGenerator] Da tao xong 11 Sprite Icon (8 co ban + 3 tien hoa) tai: {OutputFolder}");
        }

        private static Texture2D CreateBlank(Color bg)
        {
            return CreateBlankWithBorder(bg, new Color(1f, 1f, 1f, 0.35f), null);
        }

        private static Texture2D CreateBlankWithBorder(Color bg, Color outerBorder, Color? innerBorder)
        {
            Texture2D tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    bool isOuter = x < 3 || x >= Size - 3 || y < 3 || y >= Size - 3;
                    bool isInner = innerBorder.HasValue && !isOuter && (x == 3 || x == Size - 4 || y == 3 || y == Size - 4);
                    Color col = isOuter ? outerBorder : (isInner ? innerBorder.Value : bg);
                    tex.SetPixel(x, y, col);
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

        // 9. [TIẾN HÓA] Liên Thanh Xuyên Thấu: Tâm ngắm vàng kim + 3 tia đạn xuyên phá rực sáng
        private static Texture2D DrawAutoGunEvo()
        {
            Color goldBorder = new Color(0.98f, 0.80f, 0.08f, 1f);
            Color warmInner = new Color(1f, 0.93f, 0.55f, 0.7f);
            var tex = CreateBlankWithBorder(new Color(0.18f, 0.10f, 0.03f, 1f), goldBorder, warmInner);

            Color ringAmber = new Color(0.92f, 0.52f, 0.06f, 0.9f);
            Color auraOrange = new Color(1f, 0.48f, 0.08f, 1f);
            Color beamGold = new Color(1f, 0.86f, 0.22f, 1f);

            DrawCircle(tex, 32, 32, 21, ringAmber, true, 3.5f);
            DrawCircle(tex, 32, 32, 12, goldBorder, true, 2f);
            DrawRect(tex, 31, 9, 2, 46, ringAmber);

            // 3 tia đạn xuyên phá song song với đầu mũi tên sắc nhọn
            int[] ys = { 18, 32, 46 };
            int[] xStarts = { 12, 8, 12 };
            int[] xEnds = { 49, 54, 49 };
            for (int i = 0; i < ys.Length; i++)
            {
                int cy = ys[i];
                int len = xEnds[i] - xStarts[i];
                DrawRect(tex, xStarts[i], cy - 3, len, 7, auraOrange);
                DrawRect(tex, xStarts[i] + 1, cy - 2, len - 1, 5, beamGold);
                DrawRect(tex, xStarts[i] + 3, cy - 1, len - 3, 2, Color.white);

                // Đầu mũi xuyên phía phải
                for (int dx = 0; dx <= 5; dx++)
                {
                    int halfH = 5 - dx;
                    DrawRect(tex, xEnds[i] - 1 + dx, cy - halfH, 1, halfH * 2 + 1, dx < 3 ? beamGold : Color.white);
                }
            }

            tex.Apply();
            return tex;
        }

        // 10. [TIẾN HÓA] Vòng Lưỡi Cưa Hỏa Lực: Vòng lửa đỏ-vàng + 6 quả cầu hỏa lực rực cháy
        private static Texture2D DrawOrbitEvo()
        {
            Color fireBorder = new Color(1f, 0.52f, 0.12f, 1f);
            Color goldInner = new Color(1f, 0.88f, 0.35f, 0.75f);
            var tex = CreateBlankWithBorder(new Color(0.20f, 0.05f, 0.04f, 1f), fireBorder, goldInner);

            Color flameOuter = new Color(0.95f, 0.28f, 0.08f, 1f);
            Color flameMid = new Color(1f, 0.58f, 0.12f, 1f);
            Color coreGold = new Color(1f, 0.90f, 0.28f, 1f);

            DrawCircle(tex, 32, 32, 19, flameOuter, true, 5f);
            DrawCircle(tex, 32, 32, 19, coreGold, true, 2f);

            // 6 cầu lửa đặt đều mỗi 60 độ trên quỹ đạo
            for (int i = 0; i < 6; i++)
            {
                float rad = (i * 60f + 30f) * Mathf.Deg2Rad;
                float ox = 32f + Mathf.Cos(rad) * 19f;
                float oy = 32f + Mathf.Sin(rad) * 19f;
                DrawCircle(tex, ox, oy, 6.2f, flameOuter);
                DrawCircle(tex, ox, oy, 4.4f, flameMid);
                DrawCircle(tex, ox, oy, 2.7f, coreGold);
                DrawCircle(tex, ox, oy, 1.3f, Color.white);
            }

            // Tâm năng lượng rực sáng
            DrawCircle(tex, 32, 32, 5.5f, flameMid);
            DrawCircle(tex, 32, 32, 3f, Color.white);

            tex.Apply();
            return tex;
        }

        // 11. [TIẾN HÓA] Xung Chấn Đẩy Lùi: 3 vòng sóng vàng-cam dày + 4 mũi tên xung chấn hướng ra ngoài
        private static Texture2D DrawAreaPulseEvo()
        {
            Color goldBorder = new Color(0.98f, 0.80f, 0.08f, 1f);
            Color warmInner = new Color(1f, 0.92f, 0.50f, 0.7f);
            var tex = CreateBlankWithBorder(new Color(0.18f, 0.08f, 0.02f, 1f), goldBorder, warmInner);

            Color outerFlame = new Color(0.96f, 0.42f, 0.08f, 1f);
            Color midGold = new Color(1f, 0.78f, 0.16f, 1f);
            Color innerBright = new Color(1f, 0.94f, 0.55f, 1f);

            DrawCircle(tex, 32, 32, 23, outerFlame, true, 4f);
            DrawCircle(tex, 32, 32, 16, midGold, true, 3.5f);
            DrawCircle(tex, 32, 32, 9.5f, innerBright, true, 3f);
            DrawCircle(tex, 32, 32, 4.5f, Color.white);

            // 4 mũi tên đẩy lùi 4 hướng (Trên / Dưới / Trái / Phải)
            for (int d = 0; d < 7; d++)
            {
                int half = 6 - d;
                // Phải
                DrawRect(tex, 50 + d, 32 - half, 1, half * 2 + 1, Color.white);
                // Trái
                DrawRect(tex, 13 - d, 32 - half, 1, half * 2 + 1, Color.white);
                // Trên
                DrawRect(tex, 32 - half, 50 + d, half * 2 + 1, 1, Color.white);
                // Dưới
                DrawRect(tex, 32 - half, 13 - d, half * 2 + 1, 1, Color.white);
            }

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
