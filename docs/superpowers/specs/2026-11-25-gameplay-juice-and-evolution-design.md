# Thiết kế: Nâng cấp Cảm giác Chơi & Tiến hóa Vũ khí (Phương án B)

**Ngày:** 2026-11-25  
**Dự án:** DoAnMTA — Survivor Arena (Unity 6)  
**Trạng thái:** Đã rà soát độc lập và chốt phương án — chờ duyệt triển khai

---

## 1. Mục tiêu, Phạm vi & Ràng buộc

### Mục tiêu (Intent & Goals)
Nâng cấp trải nghiệm chơi trực tiếp ("chơi sướng tay hơn") cho *DoAnMTA — Survivor Arena* mà không phá vỡ cấu trúc code gọn gàng hiện tại hoặc làm hỏng bảng kiểm thử T01–T11:
1. Thêm hành động né chủ động (**Lướt / Dash** bằng `Space` hoặc `Left Shift`), lướt xuyên qua quái và đạn quái (vẫn chặn bởi `Obstacle`), kèm thanh hồi chiêu trên HUD.
2. Thêm **Vật phẩm rơi đặc biệt** (**Bình Máu** hồi HP, **Nam Châm Lớn** hút toàn bộ ngọc EXP đang có trên sân).
3. Mở khóa Bị động **Tốc chạy (`Passive_MoveSpeed`)** dựa trên `PlayerMovement.AddMoveSpeed` sẵn có và đồng bộ danh sách bị động lên `WeaponBarHUD`.
4. Thêm **Tiến hóa cho 3 Vũ khí** (`AutoGun`, `Orbit`, `AreaPulse`) với điều kiện **Vũ khí đạt Cấp tối đa (Cấp 5) + Bị động ghép cặp tương ứng đạt từ Cấp 1 trở lên** (dễ chạm tới trong thời lượng 1 trận `150s`).
5. Thêm **Banner thông báo** khi chuyển giai đoạn quái / cảnh báo Boss / Boss xuất hiện, kèm dòng gợi ý phím tắt ở `MainMenu`.
6. Bổ sung EditMode Test cho các logic cốt lõi mới và cung cấp MenuItem Editor 1-click để sinh asset/icon/prefab và gắn UI lên 2 Scene.

### Không làm trong đợt này (Non-goals)
- Không thêm Quái Tinh Anh (Elite), Rương báu, hay chế độ Vô tận (`enableEndlessRepeat`) hậu Victory.
- Không thêm bảng thống kê sát thương từng vũ khí (Damage Breakdown) ở màn kết thúc.
- Không thay thế Unity Input Manager cổ điển bằng `PlayerInput` Action Map mới (giữ nguyên `Input.GetAxisRaw` / `Input.GetKeyDown` để giữ tương thích tuyệt đối với T01–T11).

### Anti-goals & Ràng buộc kỹ thuật
- **0 GC Alloc trong vòng lặp nóng (`150` quái – ca T10):** Không dùng `FindObjectsByType` khi nhặt Nam Châm, không cấp phát mảng/List mỗi frame khi bắn đạn xuyên hoặc để lại bóng mờ khi Lướt.
- **Bảo toàn hành vi kiểm thử T01–T11:**
  - `T01`: Đi bộ WASD/mũi tên giữ nguyên tốc độ chuẩn hóa 8 hướng; chỉ Dash khi bấm `Space`/`Left Shift`.
  - `T02`: I-frame trúng đòn `0.6s` không bị i-frame ngắn của Dash (`0.24s`) rút ngắn hay ghi đè màu chớp trắng của `DamageFlash`.
  - `T04`, `T05`, `T06`: Không cho kích hoạt Dash khi `Time.timeScale == 0` hoặc `GameState != GameState.Playing`.
  - `T08`: Danh sách tĩnh theo dõi gem/pickup và trạng thái `Physics2D.IgnoreLayerCollision` phải tự dọn/khôi phục sạch khi disable, dừng game hoặc đổi Scene.

---

## 2. Các phương án đã cân nhắc (Approaches)

| Phương án | Nội dung chính | Đánh đổi | Quyết định |
|---|---|---|---|
| **A — Cảm giác tay tối giản** | Dash + Rơi Máu/Nam châm + Banner Wave/Boss | Nhanh, gần như 0 rủi ro cho T05, nhưng vũ khí lên cấp 5 xong hết chiều sâu build | Loại |
| **B — Chơi sướng tay hoàn chỉnh** | Toàn bộ A + `Passive_MoveSpeed` + Tiến hóa 3 vũ khí sẵn có (Cấp 5 + Bị động ghép ≥ Cấp 1) | Đụng thêm `WeaponBase`/`UpgradeManager`/`UpgradeCardUI`, cần test kỹ lại T05/T07/T10 | **Đã chọn** |
| **C — Mở rộng sâu** | Toàn bộ B + Quái Elite + Rương nâng cấp + Endless Mode hậu Victory | Tăng gấp đôi phạm vi UI/flow/cân bằng sau 150s trước mốc `v1.0` | Loại |

---

## 3. Thiết kế chi tiết từng thành phần

### 3.1. Kỹ năng Lướt (Dash), Xuyên quái/Đạn & HUD Cooldown
- **Các file sửa:** `PlayerMovement.cs`, `Health.cs`, `EnemyProjectile.cs`, `GameplayHUD.cs`, `ProceduralSfxFactory.cs`, `AudioManager.cs`.
- **Thông số mặc định trên `PlayerMovement`:**
  - Phím kích hoạt: `KeyCode.Space` hoặc `KeyCode.LeftShift`.
  - `dashSpeed = 14f`, tốc độ thực tế khi lướt `effectiveDashSpeed = Mathf.Max(dashSpeed, moveSpeed * 2.4f)` (đảm bảo khi nâng `MoveSpeed` thì Dash vẫn luôn nhanh hơn đi bộ).
  - `dashDuration = 0.16f`, `dashCooldown = 2.5f`, `dashInvincibilityDuration = 0.24f` *(dư `0.08s` sau khi kết thúc lướt `0.16s` để collider bật lại không gây sát thương tức thì nếu đang lướt sát quái/Boss)*.
  - `dashAfterimageCount = 3`: khởi tạo sẵn 3 child `GameObject` có `SpriteRenderer` tắt trong `Awake()`, copy sprite + `flipX` của Player lúc lướt và mờ dần alpha từ `0.45 -> 0` trong `0.2s` (**0 Instantiate/Destroy, 0 chạm vào `Player.SpriteRenderer` của `DamageFlash`**).
- **Va chạm khi Lướt (xuyên quái & đạn quái, giữ chặn bởi `Obstacle`):**
  - Cache các layer trong `Awake()`: `_playerLayer = gameObject.layer` (`Player` = `6`), `_enemyLayer = LayerMask.NameToLayer("Enemy")` (`7`), `_enemyProjectileLayer = LayerMask.NameToLayer("EnemyProjectile")` (`9`).
  - Khi bắt đầu Dash: bật `SetDashCollisionBypass(true)` gọi `Physics2D.IgnoreLayerCollision(_playerLayer, _enemyLayer, true)` và `Physics2D.IgnoreLayerCollision(_playerLayer, _enemyProjectileLayer, true)` (vẫn giữ nguyên va chạm với Layer `11` `Obstacle`), đồng thời đánh dấu `_isDashCollisionBypassed = true`.
  - Khôi phục chắc chắn (`SetDashCollisionBypass(false)`): luôn gọi lại khi hết `dashDuration`, trong `StopMovement()`, `OnDisable()` và `OnDestroy()` (nếu `_isDashCollisionBypassed == true`), đảm bảo không bao giờ kẹt trạng thái mất va chạm giữa các lượt chơi/Scene.
  - **`EnemyProjectile.cs` (`OnTriggerEnter2D`):** kiểm tra `if (targetHealth.IsDead || targetHealth.IsInvincible) return;` **trước** khi gán `_hasHit = true; Despawn();` để đạn quái bay xuyên qua trong cả khoảng buffer `0.08s` sau Dash thay vì vỡ vô hình trên người chơi.
- **Hướng lướt & khóa an toàn:**
  - Lưu `_lastMoveDirection` (mặc định `Vector2.right`) mỗi khi `_moveInput.sqrMagnitude > 0.01f`.
  - Nếu đang giữ phím di chuyển thì lướt theo `_moveInput.normalized`; nếu đứng yên thì lướt theo `_lastMoveDirection`.
  - Chỉ lướt khi `Time.timeScale > 0f`, `!_isDashing`, `_dashCooldownTimer <= 0f`, `_health == null || !_health.IsDead`, và `GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Playing`.
  - `StopMovement()` reset luôn `_isDashing = false`, `_dashTimer = 0f`, tắt các afterimage và tắt bypass va chạm.
- **Bất tử ngắn trên `Health.cs`:**
  - Thêm `public void GrantInvincibility(float duration)`:
    - Nếu `duration <= 0f || IsDead` thì thoát.
    - `_invincibilityTimer = Mathf.Max(_invincibilityTimer, duration);`
    - **Không** gọi `OnDamaged` hay `OnAnyDamaged` → không trừ máu, không popup, không rung camera, không kích hoạt chớp trắng `DamageFlash`.
- **Thuộc tính đọc cho UI & Âm thanh:**
  - `PlayerMovement` expose: `IsDashing`, `DashCooldownRemaining`, `DashCooldownNormalized` (`1f` khi sẵn sàng, `1f - (remaining / dashCooldown)` khi đang hồi) và `event Action OnDashed`.
  - `GameplayHUD` thêm 2 SerializeField tùy chọn: `Slider dashCooldownSlider`, `TMP_Text dashStatusText` (hiển thị `"LƯỚT [Space]"` khi sẵn sàng, `"LƯỚT {remaining:0.0}s"` khi đang hồi; đổi màu fill giữa `#22C55E` sẵn sàng và `#64748B` đang hồi).
  - `ProceduralSfxFactory` / `AudioManager` thêm `SoundId.Dash` (`CreateSweep` tiếng gió lướt `0.11s`, `680Hz -> 220Hz`, phát trên kênh chiến đấu với `pitchJitter = 0.06f`).

### 3.2. Vật phẩm rơi đặc biệt: Bình Máu & Nam Châm Lớn
- **Các file mới / sửa:** `ExpGem.cs`, `SpecialPickup.cs` (mới), `ExpDropManager.cs`, `ProceduralSfxFactory.cs`, `AudioManager.cs`, `SpecialPickup_Prefab.prefab` (mới).
- **Cơ chế Nam Châm toàn màn hình (0 GC Alloc) trong `ExpGem.cs`:**
  - Thêm `private static readonly List<ExpGem> _activeGems = new List<ExpGem>(128);`.
  - Đăng ký trong `OnEnable()` (`if (!_activeGems.Contains(this)) _activeGems.Add(this)`), hủy đăng ký `O(1)` swap-back trong `OnDisable()`.
  - Thêm `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` xóa `_activeGems` để an toàn khi tắt Domain Reload.
  - Thêm `public static int MagnetizeAllActive(PlayerExperience collector)`: duyệt `_activeGems`, gán `_collector = collector` (nếu đang null), `_isMagnetized = true`, và tăng nhẹ `_currentPullSpeed = Mathf.Max(_currentPullSpeed, initialPullSpeed * 1.6f)` để toàn bộ ngọc lập tức tăng tốc bay về cùng lúc; trả về số ngọc được hút.
- **Component gộp `SpecialPickup.cs` (dùng chung cho 2 loại vật phẩm):**
  - `public enum SpecialPickupType { HealPotion, MapMagnet }`
  - Dùng kiểm tra `sqrMagnitude` tương tự `ExpGem` (không gắn Collider2D vật lý): hút vào khi trong `Mathf.Max(2.5f, playerExperience.MagnetRadius)`, nhặt khi trong `playerExperience.CollectRadius`.
  - Khi nhặt:
    - `HealPotion`: gọi `playerHealth?.Heal(healAmount)` (mặc định `healAmount = 25`), phát `SoundId.PickupHeal`.
    - `MapMagnet`: gọi `ExpGem.MagnetizeAllActive(playerExperience)`, phát `SoundId.PickupMagnet`.
  - Theo dõi `_activePickups` tĩnh tương tự `ExpGem` để `ExpDropManager` giới hạn tối đa `maxActivePickups = 5` trên sân cùng lúc.
- **Tỷ lệ rơi trong `ExpDropManager.cs`:**
  - Thêm `SpecialPickup specialPickupPrefab` + `Sprite healPotionSprite` + `Sprite mapMagnetSprite` + `ComponentPool<SpecialPickup>` (prewarm `6`).
  - Trong `HandleEnemyDied(EnemyController deadEnemy)`:
    - Sau khi thả `ExpGem`, bỏ qua nếu `deadEnemy.CompareTag("Boss")` hoặc `SpecialPickup.ActiveCount >= maxActivePickups`.
    - Quay xác suất rời rạc (1 lần `Random.value`, nhân `1.5f` cho quái có `ExpValue >= 20`):
      - `mapMagnetDropChance = 0.008f` (`0.8%`)
      - `healDropChance = 0.018f` (`1.8%`)
    - Vị trí rơi lệch nhẹ `Random.insideUnitCircle * 0.35f` để không đè chính xác lên viên ngọc EXP.
- **Âm thanh:** Thêm `SoundId.PickupHeal` và `SoundId.PickupMagnet` vào `IsUiOrStateCue` của `AudioManager`.

### 3.3. Mở khóa Bị động Tốc chạy (`Passive_MoveSpeed`)
- **Các file sửa / thêm:** `PassiveUpgradeData.cs`, `PlayerStats.cs`, `IconGeneratorEditor.cs`, `Passive_MoveSpeed.asset`, `Icon_MoveSpeed.png`, gắn vào `UpgradeManager.availablePassives` và `WeaponBarHUD.trackedPassives`.
- Trong `PassiveUpgradeData.cs`: thêm `MoveSpeed` vào `enum PassiveUpgradeType`.
- Trong `PlayerStats.cs`:
  - Cache `TryGetComponent(out _movement)` trong `Awake()` (an toàn cho EditMode test không có `PlayerMovement`).
  - Xử lý `case PassiveUpgradeType.MoveSpeed: _movement?.AddMoveSpeed(data.ValuePerLevel);`.
- Thông số asset `Passive_MoveSpeed.asset`: `upgradeId = "passive_speed"`, `upgradeName = "Tốc chạy"`, `upgradeType = MoveSpeed`, `maxLevel = 5`, `valuePerLevel = 0.5f`, `description = "Tăng +0.5 tốc độ di chuyển"`.
- `[Quyết định đề xuất]` Sửa [IconGeneratorEditor.cs](file:///d:/Unity/Project/DoAnMTA/Assets/_Project/Scripts/Editor/IconGeneratorEditor.cs): bỏ đoạn code tự xóa `Icon_MoveSpeed.png` (dòng 21–25 hiện tại) và bổ sung hàm vẽ `Icon_MoveSpeed.png` theo đúng chuẩn Icon sẵn có (`64x64`, `spritePixelsPerUnit = 64f`, `FilterMode.Bilinear`).
- `[Quyết định đề xuất]` Đồng bộ đủ cả 5 asset `Passive_*` (`Cooldown`, `Damage`, `Magnet`, `MaxHealth`, `MoveSpeed`) vào mảng `WeaponBarHUD.trackedPassives` trên `Gameplay.unity` (hiện đang là `[]` nên chưa hiển thị bị động trên HUD).

### 3.4. Tiến hóa 3 Vũ khí (`Weapon Evolution`)
- **Các file sửa:** `WeaponData.cs`, `WeaponBase.cs`, `Projectile.cs`, `AutoTargetWeapon.cs`, `OrbitWeapon.cs`, `OrbitOrb.cs`, `AreaPulseWeapon.cs`, `EnemyChaseMovement.cs`, `UpgradeManager.cs`, `UpgradeCardUI.cs`, `WeaponBarHUD.cs`, `GameplayAudioBridge.cs`, `AudioManager.cs`, `ProceduralSfxFactory.cs`, 3 asset `WeaponData_*.asset`.
- **Dữ liệu tiến hóa đặt trực tiếp trong `WeaponData.cs`:**
  ```csharp
  [Header("Tiến hóa Vũ khí (Cấp tối đa + Bị động >= Cấp 1)")]
  [SerializeField] private bool hasEvolution;
  [SerializeField] private PassiveUpgradeData evolutionCatalyst;
  [SerializeField] private string evolvedWeaponName;
  [SerializeField, TextArea(1, 2)] private string evolvedDescription;
  [SerializeField] private WeaponLevelStats evolvedStats;
  [SerializeField] private Color evolvedTintColor = new Color(1f, 0.78f, 0.18f, 1f);
  [SerializeField, Min(0)] private int evolvedProjectilePierce = 0;
  [SerializeField, Min(0f)] private float evolvedKnockbackForce = 0f;
  ```
- **3 Công thức Tiến hóa cụ thể (Vũ khí Cấp 5 `IsMaxLevel` + Bị động ghép cặp `>= Cấp 1`):**
  1. `WeaponData_AutoGun` (`Đạn Tự Nhắm`) + `Passive_Cooldown` (`Tốc độ đánh` ≥ Cấp 1) → **Liên Thanh Xuyên Thấu**
     - `evolvedStats`: `damage = 55`, `cooldown = 0.30`, `range = 10`, `projectileSpeed = 19`, `projectileCount = 3`.
     - `evolvedProjectilePierce = 2` (xuyên qua tối đa 2 mục tiêu, chạm mục tiêu thứ 3 mới biến mất; chạm `Obstacle` không có `Health` vẫn despawn ngay).
     - `Projectile.cs`: thêm tham số tùy chọn `int pierceCount = 0, Color? tintColor = null`; lưu mảng cố định `_hitTargets = new Health[8]` + `_hitCount` (reset trong `Initialize()`) để tránh trúng lặp cùng 1 quái và phục hồi `SpriteRenderer.color` về màu gốc khi bắn đạn thường từ Pool.
  2. `WeaponData_Orbit` (`Cầu Năng Lượng`) + `Passive_Damage` (`Sức mạnh` ≥ Cấp 1) → **Vòng Lưỡi Cưa Hỏa Lực**
     - `evolvedStats`: `damage = 50`, `cooldown = 0.18`, `range = 3.0`, `projectileSpeed = 400`, `projectileCount = 6`.
     - `OrbitWeapon.RefreshOrbs()` truyền màu tiến hóa và scale (`1.2f`) qua `OrbitOrb.Configure(...)` (mặc định giữ nguyên giao diện thường khi chưa tiến hóa).
  3. `WeaponData_AreaPulse` (`Sóng Xung Kích`) + `Passive_MaxHealth` (`Sinh lực` ≥ Cấp 1) → **Xung Chấn Đẩy Lùi**
     - `evolvedStats`: `damage = 68`, `cooldown = 1.05`, `range = 5.4`, `projectileSpeed = 0`, `projectileCount = 1`.
     - `evolvedKnockbackForce = 5.5f`: trong `AreaPulseWeapon.Attack()`, với mỗi quái còn sống sau `TakeDamage(damage)` (`!enemyHealth.IsDead && col.gameObject.activeInHierarchy`), **nếu không phải Boss (`!col.CompareTag("Boss")`)**, gọi `EnemyChaseMovement.ApplyKnockback(dir * force, 0.16f)`; đồng thời đổi màu vòng sóng sang `evolvedTintColor`.
     - **`EnemyChaseMovement.cs` (xử lý quái bắn xa & bảo vệ Boss):** trong `FixedUpdate()`, kiểm tra `if (_knockbackTimer > 0f)` **trước** `if (!_canMove || _target == null)` (giảm `_knockbackTimer -= Time.fixedDeltaTime`, giữ `_rb.linearVelocity = _knockbackVelocity`, khi hết giờ nếu `!_canMove` thì đưa `_rb.linearVelocity = Vector2.zero`). Nhờ đó quái bắn xa `Enemy_Ranged` đang đứng yên (`_canMove == false`) vẫn bị đẩy lùi bình thường, còn `Boss` miễn nhiễm knockback hoàn toàn nên không xung đột với `BossAttackController.FixedUpdate()`. Reset `_knockbackTimer = 0f` trong `OnEnable()` và `StopMovement()`.
- **Quy trình trong `WeaponBase`, `UpgradeManager`, `GameplayAudioBridge`, UI:**
  - `WeaponBase`: thêm `IsEvolved`, `CurrentStats` trả về `EvolvedStats` khi `IsEvolved`, `CanEvolve(PlayerStats stats)`, `Evolve()`.
  - `UpgradeManager`:
    - `UpgradeOption` thêm `public bool IsWeaponEvolution;`.
    - `BuildCandidatePool()` kiểm tra `weapon.CanEvolve(playerStats)` khi `weapon.IsMaxLevel`.
    - `[Quyết định đề xuất]` Không dùng ký tự ngôi sao `★` vì font `BeVietnamPro-SemiBold.ttf` chưa có glyph `U+2605` (sẽ hiện ô vuông lỗi). Dùng `LevelText = "[TIẾN HÓA]"` để hiển thị sắc nét trên font hiện tại.
    - `[Quyết định đề xuất]` `UpgradeManager` phát thêm `event Action<UpgradeOption> OnUpgradeSelected` trong `SelectUpgrade()` sau khi áp dụng lựa chọn; `GameplayAudioBridge` chuyển từ nghe `OnUpgradeMenuClosed` sang nghe `OnUpgradeSelected` để phát `SoundId.WeaponEvolve` (khi `option.IsWeaponEvolution == true`) hoặc `SoundId.UpgradeSelect` (khi thường), **tránh phát chồng 2 SFX**.
  - `UpgradeCardUI`: cache màu gốc của nút/thẻ trong `Awake()`; khi `IsWeaponEvolution` tô màu vàng kim (`#854D0E` / `#CA8A04`, chữ `LevelText` `#FDE047`), khi thường **bắt buộc khôi phục màu gốc**.
  - `WeaponBarHUD`: với vũ khí `IsEvolved`, hiển thị nhãn `"EVO"` màu `maxLevelColor` (`#FFD64F`) và nền vàng nâu (`#5C4114` alpha `0.92`).

### 3.5. Banner Thông báo Wave/Boss & Phím tắt MainMenu
- **Các file sửa:** `SpawnConfig.cs`, `EnemySpawner.cs`, `BossSpawner.cs`, `GameplayHUD.cs`, `ProceduralSfxFactory.cs`, `AudioManager.cs`.
- `SpawnConfig.cs`: thêm overload `TryGetPhaseForTime(float elapsedSeconds, out int phaseIndex, out SpawnPhase activePhase)` (giữ nguyên hàm 2 tham số cũ).
- `EnemySpawner.cs`: giữ `_lastAnnouncedPhaseIndex = 0`; mỗi khi `phaseIndex > _lastAnnouncedPhaseIndex`, cập nhật chỉ số và phát `event Action<int, string> OnPhaseChanged` (báo ở `30s` và `75s`).
- `BossSpawner.cs`: thêm `bossWarningLeadTime = 10f`; khi `enemySpawner.ElapsedTime >= _nextBossTime - bossWarningLeadTime` và chưa có Boss, phát `event Action<string> OnBossWarning` (`"CẢNH BÁO: BOSS SẮP XUẤT HIỆN!"`) + `SoundId.BossWarning` (gán vào `IsUiOrStateCue`).
- `GameplayHUD.cs`: thêm `TMP_Text announcementBannerText` (ẩn mặc định), hiện thông báo trong `2.8s` rồi mờ dần và tắt; ẩn ngay nếu chuyển sang `GameOver` hoặc `Victory`.
- `MainMenu.unity`: thêm `ControlsHintText` phía dưới nút `Thoát`: `"WASD / Mũi tên: Di chuyển  |  Space / Shift: Lướt  |  1/2/3: Chọn thẻ  |  ESC: Tạm dừng"` (chỉ dùng dấu `|` và ký tự nằm trong font `BeVietnamPro-SemiBold`).

### 3.6. Công cụ Editor 1-click & Sprite bổ sung
- Cập nhật `IconGeneratorEditor.cs` thêm `Icon_MoveSpeed.png` (`64x64`, `PPU = 64`, `Bilinear`).
- Bổ sung vào `EffectSpriteGeneratorEditor.cs` (hoặc tạo `GameplayJuiceSetupEditor.cs` dưới `Assets/_Project/Scripts/Editor/`):
  - `[Quyết định đề xuất]` Sinh 2 sprite pixel-art 16x16 (`FilterMode.Point`, `PPU = 20`) đặt trong `Assets/_Project/Art/Sprites/Effects/`: `Pickup_Heal.png` (bình máu đỏ viền trắng) và `Pickup_Magnet.png` (nam châm chữ U đỏ-bạc) theo đúng quy ước `EffectSpriteGeneratorEditor`.
  - Tạo `Data/Upgrades/Passive_MoveSpeed.asset`, `Prefabs/SpecialPickup_Prefab.prefab` (`Sorting Layer = Pickups`, giống `ExpGem.prefab`), cập nhật dữ liệu Tiến hóa cho 3 `WeaponData_*.asset`.
  - Nối tự động `Passive_MoveSpeed` vào `UpgradeManager` & đủ 5 `Passive_*` vào `WeaponBarHUD.trackedPassives`, gắn `SpecialPickup_Prefab` + sprites vào `ExpDropManager`, dựng `DashCooldownBar` & `AnnouncementBannerText` trên `Gameplay.unity` và `ControlsHintText` trên `MainMenu.unity` theo chuẩn font/màu/9-slice Bước 1, rồi lưu 2 Scene.

### 3.7. Kiểm thử tự động (EditMode Tests)
Bổ sung vào [SurvivorCoreEditModeTests.cs](file:///d:/Unity/Project/DoAnMTA/Assets/_Project/Tests/EditMode/SurvivorCoreEditModeTests.cs):
1. `Health_GrantInvincibility_ExtendsWithoutShorteningExistingTimer`: trúng đòn có i-frame `0.6s`, gọi `GrantInvincibility(0.18f)` vẫn giữ được trạng thái bất tử dài hơn và không bắn thêm `OnDamaged`.
2. `WeaponBase_CanEvolveAndEvolve_RequiresMaxLevelAndCatalyst`: kiểm tra chưa đạt Cấp 5 hoặc thiếu Bị động Cấp 1 thì `CanEvolve == false`; đủ Vũ khí Cấp 5 + Bị động Cấp 1 thì `CanEvolve == true`, `Evolve()` thành công 1 lần duy nhất và `IsEvolved == true`.
3. `PlayerStats_ApplyMoveSpeedPassive_IncreasesMovementSpeed`: áp dụng `Passive_MoveSpeed` qua `PlayerStats` làm tăng `PlayerMovement.MoveSpeed`.
4. `ExpGem_MagnetizeAllActive_PullsActiveGemsZeroAlloc` *(nhẹ, kiểm tra `MagnetizeAllActive` hút đúng gem đang active và trả count chính xác)*.
