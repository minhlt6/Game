# ⚔️ GRIND AND GLORY — 2D Top-Down Action RPG Survival

> **Đồ án Môn học: Thiết kế và Phát triển Trò chơi**  
> **Trường Đại học Thủy Lợi (TLU) — Khoa Công nghệ Thông tin**  
> **Nhóm thực hiện:** Nhóm 12 — *Pixel Gaming*  
> **Nền tảng & Engine:** Unity 6 (`6000.6.2f1` LTS / URP 2D) • Ngôn ngữ: C# • Hệ điều hành: Windows PC

---

## 📖 1. Giới thiệu Tổng quan & Cốt truyện

**Grind and Glory** là tựa game hành động nhập vai sinh tồn góc nhìn từ trên xuống (**2D Top-Down Action RPG Survival**). Trò chơi lấy bối cảnh tại vùng đất giả tưởng **Astral** — một miền đất từng trù phú và yên bình nhưng nay bị thế lực hắc ám từ Cổng Địa Ngục tràn lên xâm chiếm.

Người chơi sẽ hóa thân thành **1 trong 3 nhà mạo hiểm gia** (**Mercy**, **Inirius**, hoặc **Eni**) lên đường giải cứu vùng đất Astral qua **3 màn chơi (Chapters)** đầy thử thách. Trong mỗi màn chơi:
- Nhân vật di chuyển tự do trên bản đồ 2D có giới hạn biên và chướng ngại vật tự nhiên, tự động ngắm bắn và thi triển vũ khí/kỹ năng tiêu diệt hàng trăm quái vật.
- Thu thập **Hạt Kinh Nghiệm (`energy_capsules`)**, **Vàng (`Gold`)**, **Cầu Sinh Lực (`redball`, `item_egg`)** và **Rương Vũ Khí (`item_box`)**.
- Tích lũy **Số lượng quái tiêu diệt (Kill Count)** để triệu hồi **Mini Boss** giữa màn và **Final Boss (Trùm cuối)** của màn chơi.
- Sử dụng Vàng thu thập được để cường hóa chỉ số vĩnh viễn tại **Thương gia xứ Astral (NPC Merchant)**.

---

## 🦸 2. Hệ thống Nhân vật (3 Anh hùng)

Trước mỗi trận đấu, người chơi chọn **1 nhân vật duy nhất** xuất trận với bộ chỉ số khởi điểm, tốc độ tăng trưởng mỗi cấp và vũ khí chính đặc trưng:

| Nhân vật | Mã Asset | Vai trò | Vũ khí chính khởi đầu | HP | Strength / Magic | Endurance (Giáp) | Speed (Tốc chạy) |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **Mercy** | `normal_nuri` | Chiến binh cận chiến / Tầm trung | **Kiếm / Súng Ánh Sáng** (`weapon_00`) | **150** *(+12/cấp)* | **18 STR** *(+3/cấp)* | **12** *(+2/cấp)* | **4.2** *(+0.05/cấp)* |
| **Inirius** | `normal_durun` | Pháp sư nguyên tố Hỏa / Lôi | **Trượng / Súng Nguyên Tố** (`weapon_04`) | **100** *(+8/cấp)* | **25 MAG** *(+4/cấp)* | **6** *(+1/cấp)* | **4.5** *(+0.05/cấp)* |
| **Eni** | `normal_mokoji` | Phù thủy kiểm soát Băng giá | **Sách Phép Cổ Đại** (`weapon_06_1`) | **110** *(+9/cấp)* | **16 MAG** *(+2.5/cấp)* | **8** *(+1.5/cấp)* | **4.8** *(+0.08/cấp)* |

---

## 🔫 3. Hệ thống Vũ khí (9 Loại) & Kỹ năng Nội tại (5 Loại)

### 3.1. Cơ chế Cầm Vũ khí & Súng Chiến thuật Tự động
- **Vũ khí chính** được nhân vật cầm trực tiếp trên tay, tự động xoay theo hướng ngắm mục tiêu, có hiệu ứng giật lùi (**Recoil**) và tia lửa đầu nòng (**Muzzle Flash**) khi khai hỏa.
- **Vũ khí phụ dạng Súng (`weapon_02_1`, `weapon_05_4`, `weapon_08`)** khi trang bị sẽ hiển thị bay lơ lửng quanh vai nhân vật như các tháp súng hộ vệ tự động, cùng xoay ngắm và bắn đạn xuyên phá / tia laser / đạn đại bác.

| Mã ID | Tên Vũ khí | Phân loại | Sát thương cơ bản | Hồi chiêu | Cơ chế & Hiệu ứng nâng cấp (Tối đa Lv.5) |
| :--- | :--- | :--- | :---: | :---: | :--- |
| `weapon_00` | **Kiếm Ánh Sáng** | Vũ khí chính (Mercy) | **65** | `1.2s` | Chém quét bán nguyệt + bắn đạn kiếm khí ánh sáng xuyên mục tiêu (+15% DMG, +10% tầm/cấp). |
| `weapon_04` | **Trượng Nguyên Tố** | Vũ khí chính (Inirius) | **75** | `1.4s` | Bắn cầu lửa nguyên tố gây hiệu ứng **Bỏng lửa (Burn)** (+20% DMG, +1 cầu lửa/cấp). |
| `weapon_06_1` | **Sách Phép Cổ Đại** | Vũ khí chính (Eni) | **55** | `1.0s` | Bắn mũi giáo băng giá gây **Làm chậm (Freezing)** (+15% DMG, +10% làm chậm/cấp). |
| `weapon_02_1` | **Súng Tiểu Liên / Rìu Chiến** | Vũ khí phụ (Vật lý) | **70** | `2.0s` | Súng tiểu liên bắn chùm đạn năng lượng xanh xuyên thấu đám đông (+18% DMG, +1 tia đạn/cấp). |
| `weapon_05_4` | **Súng Laser / Phi Tiêu** | Vũ khí phụ (Vật lý) | **45** | `0.7s` | Súng lục bắn liên thanh tia laser tốc độ cao (+15% DMG, +1 tia laser/cấp). |
| `weapon_08` | **Đại Pháo Phán Quyết** | Vũ khí phụ (Vật lý) | **95** | `2.5s` | Bắn đạn pháo nổ kèm sóng xung kích diện rộng đẩy lùi quái vật (+25% DMG, -0.2s CD/cấp). |
| `hurricane` | **Cuồng Phong** | Vũ khí phụ (Phép) | **60** | `2.8s` | Phóng lốc xoáy 2 tầng cuốn quái vật vào tâm và gây sát thương liên tục (+15% DMG, +15% bán kính/cấp). |
| `thunderbolt` | **Thiên Lôi** | Vũ khí phụ (Phép) | **90** | `2.2s` | Giáng cột sét lớn từ bầu trời kèm vòng điện lan tỏa xuống mục tiêu (+20% DMG, +1 tia sét/cấp). |
| `awaken_aura` | **Hào Quang Thức Tỉnh** | Vũ khí phụ (Phép) | **35/s** | Duy trì | Tạo vòng cổ ngữ 2 lớp xoay quanh chân, đốt cháy mọi kẻ địch áp sát (+15% DMG, +12% bán kính/cấp). |

### 3.2. Năm Kỹ năng Nội tại (Passive Skills — Tối đa Lv.2)
1. **Cường Lực (`cuong_luc`)**: Tăng **+15%** (Lv.1) / **+30%** (Lv.2) tổng sát thương đầu ra cho mọi vũ khí.
2. **Thần Tốc (`than_toc`)**: Tăng **+10%** (Lv.1) / **+20%** (Lv.2) tốc độ di chuyển và tốc độ bắn.
3. **Giáp Cứng (`giap_cung`)**: Cộng thêm **+5** (Lv.1) / **+10** (Lv.2) chỉ số Endurance (Giáp).
4. **Hồi Phục (`hoi_phuc`)**: Tự động hồi **2%** (Lv.1) / **4%** (Lv.2) HP tối đa mỗi 3 giây.
5. **Từ Trường (`tu_truong`)**: Tăng **+30%** (Lv.1) / **+60%** (Lv.2) bán kính hút vật phẩm rơi và hạt EXP.

---

## 📐 4. Công thức Toán học trong Game

- **Công thức Sát thương Vũ khí:**
  $$\text{Damage} = (\text{WeaponBase} + \text{HeroAttack}) \times (1 + 0.2 \times \text{WeaponLevel}) \times \text{Hệ số Cường Lực}$$
- **Công thức Sát thương Nhân vật nhận vào:**
  $$\text{DamageTaken} = \max\left(1,\; \text{MonsterStrength} - \text{EffectiveEndurance} \times 0.8\right)$$
- **Mốc Kinh nghiệm (EXP) Lên cấp:**
  - **Level 1 → 5:** `30 EXP / cấp` (Tích lũy: `30, 60, 90, 120, 150`)
  - **Level 5 → 10:** `60 EXP / cấp` (Tích lũy: `210, 270, 330, 390, 450`)
  - **Level 10 → 20:** `100 EXP / cấp` (Tích lũy: `550 .. 1450`)
  - **Level 20 → 30:** `180 EXP / cấp` (Tích lũy: `1630 .. 3250`)

---

## 🗺️ 5. Ba Màn chơi (Stages) & Hệ thống Quái vật / Boss

Mỗi màn chơi có bản đồ riêng, hệ thống chướng ngại vật tự nhiên, quái thường, **Mini Boss** và **Final Boss** (có cơ chế **Giáp Boss**, **Triệu hồi Cận vệ** và **Cuồng Nộ Enrage < 50% HP**):

| Màn chơi | Tên Bản đồ | Quái thường (Loại 1) | Mini Boss (Loại 2) | Final Boss (Loại 3 — Mục tiêu Thắng) |
| :---: | :--- | :--- | :--- | :--- |
| **Màn 1** | **Khu Rừng Astral** (`chapter_1`) | Slime, Goblin, Wild Boar | **Goblin Boss / Tree Monster** *(Mốc: **100 Kills**)* | **Golem (`mr_rock`)** *(Mốc: **250 Kills**)* |
| **Màn 2** | **Lâu Đài Bóng Tối** (`chapter_5`) | Skeleton, Vampire Bat, Gargoyle | **Bone Gargoyle / Crimson** *(Mốc: **150 Kills**)* | **Grim Reaper (`dark_murderer`)** *(Mốc: **350 Kills**)* |
| **Màn 3** | **Cổng Địa Ngục** (`chapter_7`) | Gargoyle, Lava Slime & Demon Skull | **Guardian of Fire / Drake** *(Mốc: **200 Kills**)* | **Herald of Death (`kaiser_demon`)** *(Mốc: **500 Kills**)* |

---

## 🎮 6. Bảng Điều khiển & Phím tắt (Controls)

| Phím / Thao tác | Chức năng trong Game |
| :--- | :--- |
| **`W`, `A`, `S`, `D`** hoặc **Phím Mũi Tên** | Di chuyển nhân vật 8 hướng. |
| **`Space`** hoặc **`Left Shift`** | **Lướt né (Dash)**: Tăng tốc tức thời, miễn nhiễm sát thương trong lúc lướt và xóa hiệu ứng Bỏng/Chảy máu. |
| **`T`** | Chuyển đổi chế độ ngắm: **Tự động ngắm quái gần nhất (Auto-Aim)** ↔ **Đánh theo hướng di chuyển**. |
| **Con lăn chuột (`Scroll Up / Down`)** | Thu phóng góc nhìn Camera toàn cảnh xa / gần (`5.5` ↔ `11.5`, mặc định `8.2`). |
| **`1`, `2`, `3`** hoặc **Click Chuột Trái** | Chọn nhanh Thẻ bài nâng cấp khi nhân vật Lên cấp (Level Up). |
| **`R`** hoặc **`Esc`** | Mở bảng **Tạm dừng & Cài đặt (Pause / Settings / Công cụ Test nhanh)**. |
| **`F1`** | **Phím tắt Demo/Chấm điểm nhanh**: Mở khóa toàn bộ 3 Màn chơi, hồi đầy HP và cộng ngay **+5,000 Gold**. |

---

## 📂 7. Cấu trúc Thư mục Dự án

```text
Game/
├── Assets/
│   ├── Editor/
│   │   └── GrindAndGloryEditorSetup.cs    # Công cụ khởi tạo Scene, chạy 20 Test Cases tự động & Build .EXE
│   ├── Scripts/
│   │   ├── GameDatabase.cs                # Cơ sở dữ liệu 3 Anh hùng, 9 Vũ khí, 5 Kỹ năng, 13 Quái/Boss, 3 Màn chơi & Công thức
│   │   ├── ResourceCache.cs               # Bộ nạp & đệm tài nguyên Smooth HD (Trilinear + Mipmap) & SpriteAnimator
│   │   ├── AudioAndInput.cs               # Quản lý Nhạc nền (BGM), Hiệu ứng âm thanh (SFX) & Input đa nền tảng
│   │   ├── MapEnvironmentBuilder.cs       # Sinh bản đồ gạch nền, tường biên, vật cản tự nhiên & Y-Sorting an toàn
│   │   ├── WeaponAndProjectiles.cs        # Đạn đạo, tia laser, cầu lửa, lốc xoáy, sấm sét & hiệu ứng nổ va chạm
│   │   ├── CombatAndEntities.cs           # AI Quái vật/Boss (Cuồng nộ, Bão đạn, Xung kích), Vật phẩm rơi & Chữ nổi 96pt
│   │   ├── PlayerController.cs            # Điều khiển nhân vật, Cầm súng xoay ngắm + Súng phụ bay quanh vai & Lên cấp
│   │   ├── GrindAndGloryManager.cs        # Quản lý vòng đời trận đấu, Camera toàn cảnh, nhịp sinh quái & gọi Boss
│   │   └── GrindAndGloryUI.cs             # Giao diện người dùng (Supersampling 2x sắc nét): Menu, Shop, HUD, Thẻ bài
│   ├── Resources/
│   │   ├── Animations/                    # Chuỗi frame hoạt ảnh Smooth HD của 3 Anh hùng, Quái vật, Boss, NPC
│   │   ├── Sprites/                       # Icon Vũ khí, Kỹ năng, Hiệu ứng đạn/chiêu thức (VFX), Banner & Background
│   │   ├── Maps/                          # Gạch nền bản đồ và chướng ngại vật tự nhiên của 3 Màn chơi
│   │   └── Audio/                         # 5 bản nhạc nền (BGM) và 16 hiệu ứng âm thanh chiến đấu (SFX)
│   └── Scenes/
│       ├── SampleScene.unity              # Scene chính tích hợp sẵn hệ thống Grind and Glory
│       └── GrindAndGlory.unity            # Scene dự phòng chuẩn hóa
├── ProjectSettings/                       # Cấu hình dự án Unity 6 (URP 2D)
└── Packages/                              # Cấu hình gói thư viện Unity
```

---

## 🚀 8. Hướng dẫn Mở và Chạy Game trong Unity Editor

1. **Mở Dự án**:
   - Mở **Unity Hub** $\rightarrow$ bấm **Add** $\rightarrow$ chọn thư mục gốc của dự án `Game` $\rightarrow$ mở bằng phiên bản **Unity `6000.6.2f1`** (hoặc Unity 6 LTS tương đương).
2. **Khởi tạo Scene chuẩn (nếu mở lần đầu)**:
   - Trên thanh menu trên cùng của Unity Editor, chọn **`Grind and Glory` $\rightarrow$ `1. Khởi tạo & Cập nhật Scene (Setup Game Scene)`**.
3. **Thiết lập khung hình Game sắc nét nhất**:
   - Trong tab **Game**, chọn tỷ lệ khung hình **`Full HD (1920x1080)`** (hoặc `Free Aspect`) và để thanh trượt **`Scale = 1x`**.
4. **Chạy Game**:
   - Bấm nút **Play (▶)** ở chính giữa phía trên màn hình để trải nghiệm!
5. **Chạy Kiểm thử Tự động (20 Test Cases) hoặc Xuất file `.EXE`**:
   - Vào menu **`Grind and Glory` $\rightarrow$ `2. Chạy Kiểm thử Tự động 20 Test Cases`** để xuất báo cáo kiểm thử ra file `TestResults_Summary.txt`.
   - Vào menu **`Grind and Glory` $\rightarrow$ `4. Đóng gói File Chạy .EXE (Build Windows Standalone)`** để đóng gói bộ cài đặt độc lập tại thư mục `Build_Windows/GrindAndGlory.exe`.
