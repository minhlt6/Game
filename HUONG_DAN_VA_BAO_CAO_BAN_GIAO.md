# BÁO CÁO BÀN GIAO, ĐÁNH GIÁ NÂNG CẤP & HƯỚNG DẪN VẬN HÀNH TRONG UNITY
## ĐỀ TÀI: THIẾT KẾ VÀ PHÁT TRIỂN TRÒ CHƠI "GRIND AND GLORY"
**Nhóm sinh viên thực hiện:** Nhóm 12 – Pixel Gaming (Trường Đại học Thủy Lợi – Khoa CNTT)  
**Giảng viên hướng dẫn:** ThS. Trương Xuân Nam  
**Nền tảng xây dựng:** Unity 6 (`6000.6.2f1` – Universal Render Pipeline 2D)

---

## 1. Đánh Giá & Các Hạng Mục Nâng Cấp Sau Khi Hoàn Thành

1. **Cơ chế Ngắm Mục Tiêu Thông Minh (Smart Auto-Aim & Manual Direction Toggle – Phím `T`):**
   - Các vũ khí tầm xa/phép thuật (`Trượng Nguyên Tố`, `Sách Phép Cổ Đại`, `Phi Tiêu Bóng Đêm`, `Cuồng Phong`, `Rìu Chiến Xoay`) tự động khóa mục tiêu quái vật/Boss gần nhất trong tầm (ưu tiên Boss), giúp người chơi vừa di chuyển né đòn vừa tấn công chính xác. Bấm phím **`T`** để chuyển đổi linh hoạt giữa **Tự động ngắm** và **Đánh theo hướng di chuyển**.
2. **Hoạt Ảnh Đa Khung Hình (Spine VFX Animations) Cho Vũ Khí, Kỹ Năng & Vật Phẩm:**
   - Kích hoạt chuỗi frame hoạt ảnh thực tế từ bộ Asset Spine cho `Hào Quang Thức Tỉnh (awaken_aura)`, `Cuồng Phong (hurricane)`, `Thiên Lôi (thunderbolt)`, cầu lửa `fire/flame`, hạt kinh nghiệm `energy_capsules` và rương vũ khí `item_box`.
3. **Hệ Thống Hiệu Ứng Trạng Thái Chiến Đấu (Freezing, Burn, Bleeding & Thanh Tẩy):**
   - `Trượng Nguyên Tố (weapon_04)` gây hiệu ứng **Bỏng lửa (`burn`)** rút máu theo thời gian.
   - `Sách Phép Cổ Đại (weapon_06_1)` gây hiệu ứng **Đóng băng làm chậm (`freezing`)**.
   - Quái vật và Boss hệ Lửa/Bóng tối gây hiệu ứng **Bỏng lửa (`burn`)** hoặc **Chảy máu (`bleeding`)** lên nhân vật; người chơi có thể **Lướt (`Space/Shift`)** hoặc nhặt **Cầu Sinh Lực (`redball`, `item_egg`)** để hóa giải.
4. **Nâng Cấp Giao Diện HUD Trực Quan (Radar Mục Tiêu, Thanh Trạng Thái Lướt & Sát Thương Thực Tế):**
   - **Ô Trang Bị Vũ Khí** hiển thị trực tiếp **Cấp độ (`Lv.1..5`)** kèm **Sát thương thực tế tính theo công thức GDD** ngay trên từng ô vũ khí và trên thẻ bài nâng cấp.
   - **Radar Mục Tiêu** góc trên bên phải chỉ hướng la bàn (`↑ ↗ → ↘ ↓ ↙ ← ↖`) và khoảng cách (`m`) tới **Mini Boss / Final Boss** hoặc **Rương Vũ Khí (`item_box`)** rơi trên bản đồ.
5. **Phím Tắt Mở Khóa Nhanh Phục Vụ Thuyết Trình (`F1`) & Bộ Kiểm Thử Mở Rộng 20 Test Cases:**
   - Bấm phím **`F1`** ở bất kỳ đâu để mở khóa cả 3 Màn chơi và cộng `+5,000 Gold`.
   - Mở rộng bộ kiểm thử tự động trong `Assets/Editor/GrindAndGloryEditorSetup.cs` lên **20 Test Cases (Đạt 20/20 - 100% PASS)** kèm tiện ích đóng gói `.EXE` trên menu **`Grind and Glory`**.
