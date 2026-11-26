# Survivor Arena (`DoAnMTA`)

Game hành động sinh tồn 2D góc nhìn từ trên xuống theo phong cách *Vampire Survivors*, phát triển trên **Unity 6 (`6000.6.8f1`)** với **URP 2D**, tối ưu **Object Pooling (0 GC Alloc trong vòng lặp chiến đấu)** và hệ thống âm thanh/tài nguyên tự sinh trong project.

---

## Tính năng chính

- **Chọn 3 nhân vật khởi đầu** (mỗi nhân vật có bộ sprite pixel-art 2 khung hình, chỉ số và vũ khí riêng):

| Nhân vật | Vũ khí khởi đầu | Chỉ số | Phong cách |
|---|---|---|---|
| **Thợ Săn** | Đạn Tự Nhắm | `100 HP` \| Tốc `5.0` | Cân bằng, tầm xa linh hoạt |
| **Pháp Sư** | Cầu Năng Lượng | `90 HP` \| `+10% Sát thương` | Gây sát thương cao, dọn quái quanh thân |
| **Hiệp Sĩ** | Sóng Xung Kích | `120 HP` \| Tốc `4.7` | Chống chịu tốt, kiểm soát diện rộng |

- **Hệ thống Vũ khí & Tiến hóa (Evolution):** Khi vũ khí đạt **Cấp 5 (tối đa)** và sở hữu **Bị động xúc tác >= Cấp 1**, thẻ `[TIẾN HÓA]` sẽ mở khóa (hoặc tiến hóa ngay khi mở Rương Tinh Anh), biến đổi hoàn toàn ngoại hình và hiệu ứng theo **3 tông màu tách biệt**:

| Vũ khí gốc | Bị động xúc tác | Dạng Tiến hóa | Hiệu ứng & Nâng cấp |
|---|---|---|---|
| **Đạn Tự Nhắm** | Tốc độ đánh | **Liên Thanh Xuyên Thấu** *(Xanh lam – Cyan điện quang)* | Bắn 3 thương plasma siêu nhanh + vệt tia điện quang, xuyên tối đa 2 quái |
| **Cầu Năng Lượng** | Sức mạnh | **Vòng Lưỡi Cưa Hỏa Lực** *(Đỏ – Cam hỏa diệm)* | 6 lưỡi cưa lửa cỡ lớn tự xoay siêu tốc (`-900°/s`) kèm vòng vệt lửa |
| **Sóng Xung Kích** | Máu tối đa | **Xung Chấn Đẩy Lùi** *(Tím hư không – Hồng neon)* | Sóng địa chấn 16 gai xoay kép ngược chiều, chớp tâm, đẩy lùi quái thường + rung nhẹ camera |

- **5 Bị động nâng cấp:** Sức mạnh (`+Sát thương`), Tốc độ đánh (`-Hồi chiêu`), Máu tối đa (`+HP`), Nam châm (`+Tầm hút EXP`), Tốc chạy (`+Tốc độ di chuyển`).
- **Kỹ năng Lướt (Dash):** Lướt nhanh xuyên quái và đạn địch với khung bất tử ngắn (`i-frame`), 3 bóng mờ 0 GC và thanh hồi chiêu trực tiếp trên HUD.
- **Nhịp trận 4 phút:**
  - **4 giai đoạn quái:** `0:00` Khởi động → `0:40` Áp lực → `1:30` Cao trào → `2:45` Bão quái (4 loại quái: Thường, Nhanh, Trâu, Bắn xa).
  - **3 đợt Quái Tinh Anh (`1:10`, `2:20`, `3:15`):** Rơi **Rương Báu** tự động ưu tiên Tiến hóa vũ khí hoặc nâng cấp ngay.
  - **Boss cuối (`4:00`, cảnh báo lúc `3:50`):** Có 2 chiêu (đạn tỏa tròn 360° + gồng vạch đỏ báo hướng lao tới), **Pha 2 Cuồng Nộ** khi `< 50% HP` và mũi tên chỉ hướng ngoài màn hình.
- **UI & Trải nghiệm chiến đấu:**
  - Nút **Đổi thẻ (`Reroll [R]`)** tối đa 3 lượt/trận trên bảng chọn nâng cấp.
  - Thanh máu dưới chân nhân vật, popup sát thương / `+HP` màu xanh lá, **viền đỏ nhấp nháy cảnh báo máu thấp (`< 30% HP`)**.
  - Rơi vật phẩm sinh tồn (**Bình Máu**, **Nam Châm Lớn** hút toàn bộ ngọc EXP trên bản đồ).
  - Nhạc nền & SFX tự tổng hợp (`ProceduralSfxFactory`), tiếng nhặt ngọc EXP tăng cao độ theo chuỗi, bảng thống kê sát thương từng vũ khí cuối trận và lưu kỷ lục bằng `PlayerPrefs`.

---

## Phím điều khiển

| Phím | Chức năng |
|---|---|
| `W A S D` / `Phím mũi tên` | Di chuyển 8 hướng |
| `Space` / `Left Shift` | Lướt (Dash) xuyên quái và đạn |
| `1` / `2` / `3` | Chọn nhanh thẻ nâng cấp |
| `R` | Đổi thẻ nâng cấp (`Reroll`) |
| `Space` / `Enter` | Đóng nhanh popup Rương Báu |
| `ESC` | Tạm dừng / Tiếp tục |

---

## Cấu trúc thư mục chính

```text
Assets/_Project/
├── Art/            # Sprites (Nhân vật, Quái, Đạn, VFX, Môi trường), Icons, UI, Font tiếng Việt
├── Data/           # ScriptableObjects: Characters, Enemies, SpawnConfig, Weapons, Upgrades
├── Prefabs/        # Prefab Player/Enemy/Boss/Projectile/ExpGem/Pickups/VFX
├── Scenes/         # MainMenu.unity (index 0), Gameplay.unity (index 1)
├── Scripts/
│   ├── Data/       # Định nghĩa ScriptableObject
│   ├── Player/     # Di chuyển, Dash, Kinh nghiệm, Chỉ số, Chọn nhân vật
│   ├── Weapons/    # WeaponBase, AutoTarget, Orbit, AreaPulse, Projectile
│   ├── Enemies/    # AI đuổi theo, Bắn xa, Boss FSM, Spawner theo Wave/Elite
│   ├── Systems/    # GameManager, PoolManager, UpgradeManager, ExpDropManager, Audio, CameraShake
│   ├── UI/         # MainMenu, GameplayHUD, UpgradePanel, WeaponBar, BossBar, ChestPopup, EndGame
│   └── Editor/     # Công cụ sinh sprite/icon, thiết lập scene 1-click và God Mode (Editor-only)
└── Tests/EditMode/ # Bộ kiểm thử tự động EditMode (NUnit)
```

---

## Yêu cầu & Cách chạy dự án

1. Cài **Unity Hub** và **Unity `6000.6.8f1`** (URP 2D).
2. Mở thư mục dự án `DoAnMTA` bằng Unity Hub.
3. Mở scene `Assets/_Project/Scenes/MainMenu.unity` và bấm **Play**.
4. **Chạy kiểm thử tự động (EditMode Tests):**
   - Trong Unity: `Window > General > Test Runner > EditMode > Run All`.
5. **Công cụ hỗ trợ trong Unity Editor** *(nằm trong `SurvivorGame.Editor.asmdef`, tự động loại bỏ hoàn toàn khi Build game)*:
   - `Tools > Survivor Arena > Thiet Lap Tien Hoa, Vat Pham & HUD (1-Click)`: Tái tạo toàn bộ tài nguyên/dữ liệu và nối 2 scene `MainMenu` & `Gameplay`.
   - `Tools > Survivor Arena > God Mode (Chi Test Trong Editor)`: Bật bất tử, max vũ khí, nhảy mốc thời gian và gọi Boss để kiểm thử nhanh trong Play Mode.
6. **Xuất bản chạy độc lập (`.exe`):**
   - `File > Build Profiles` → Chọn `Windows` (đã cấu hình sẵn `MainMenu.unity` và `Gameplay.unity`) → **Build**.
