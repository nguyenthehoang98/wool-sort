# Tổng quan Cơ chế Booster (Wool‑Sort)

## 1️⃣ Giới thiệu
Trò chơi có ba loại booster:
- **AddSlot (Add Hole)** – thêm một ô chờ (slot) mới cho len.
- **Clean** – dọn sạch tất cả các mảnh len trong các slot chờ vào một hộp tạm, sau đó sẽ đưa chúng trở lại.
- **MagicBox** – biến hộp đầu tiên có thể sinh ra thành màu cầu vồng và tự động điền các mảnh len còn thiếu.

## 2️⃣ Kiểu dữ liệu cốt lõi
```csharp
public enum BoosterType { AddSlot, Clean, MagicBox }
```
Mỗi booster có:
- **Giá** (`Configs.LIST_BOOSTER_COST`).
- **Mức mở khóa** (`Configs.LIST_BOOSTER_UNLOCK`).
- **Chuỗi bản địa hoá** (VD: `BOOSTER_ADD_HOLE`, `DESC_BOOSTER_CLEAN`).

## 3️⃣ Luồng dữ liệu
- **Số lượng** được lưu trong `GameData.boosters` (`BoosterSaveData`).
- Thay đổi số lượng qua `GameData.AddCount(BoosterType, delta, reason, placement)` – đồng thời gửi sự kiện tới `Databuckets`.
- Cờ đã hiển thị tutorial được lưu trong `GameData.shownBoosterTutorials`.
- Tổng số booster đã dùng được theo dõi bởi `GameData.TotalBoosterUsed`.

## 4️⃣ Giao diện UI
- `BoosterItem` quản lý nút UI (trạng thái bình thường, khóa, thông báo, dấu cộng).
- Logic mở khóa (`Setup`) kiểm tra level người chơi so với `LIST_BOOSTER_UNLOCK` và trạng thái tutorial.
- Khi người chơi nhấn nút, `BoosterManager.UseBooster(type)` được gọi sau khi xác thực.
- `MoreBoosterPopup` cho phép mua booster bằng tiền xu hoặc quảng cáo thưởng và cuối cùng gọi `BoosterManager.UseBooster`.

## 5️⃣ BoosterManager – Logic trung tâm
### `CanUseBooster`
- **AddSlot**: không cho phép nếu `WaitingAreaManager.HasReachMaxSlot()`.
- **Clean**: không cho phép nếu `WaitingAreaManager.IsAllEmpty()`.
- **MagicBox**: luôn cho phép (các ràng buộc đã được bình luận).
- Khi không thể dùng, sẽ hiển thị tooltip tương ứng.

### `UseBooster`
1. Kiểm tra `isLockByUsingBooster` – nếu đang dùng một booster khác thì bỏ qua.
2. Ghi lại số lần mua (trừ trường hợp revive).
3. Rung lại, phát âm thanh đặc trưng.
4. Gửi xử lý cho từng loại:
   - **AddSlot** → `UseBoosterAddSlot()` → `WaitingAreaManager.AddSlot()`.
   - **Clean** → `UseBoosterClean()` →
     * Khóa tương tác.
     * Phát animation chổi quét.
     * Hiện hộp tạm.
     * Di chuyển tất cả len đã đầy sang các *ghost* và đưa chúng về hộp tạm, sau đó mở khóa.
   - **MagicBox** → `UseBoosterMagicBox()` →
     * Khóa.
     * Lấy box đầu tiên (`BoxManager.GetFirstBox()`).
     * Chơi animation đổi màu cầu vồng (`Box.SetRainbowColor`).
     * Sau một khoảng delay ngắn, lặp lại yêu cầu các mảnh len còn thiếu và sinh chúng trực tiếp vào box đang hướng tới.
     * Khi hoàn thành, mở khóa và lên lịch sinh thêm 2 box có ưu tiên thấp (`BoxManager.AddScheduleMinPrioritySpawn(2)`).

### Luồng Hộp Tạm (Booster Clean)
- `ShowTempBoxEntry` di chuyển UI container vào view bằng tween.
- `SpawnAllGhosts` tạo ghost cho mỗi mảnh len đang ở slot, xóa sạch slot và đăng ký ghost.
- `AnimateAllGhosts` bay các ghost tới hộp tạm bằng `YarnGhostService.FlyToTempBox`.
- Khi tất cả ghost tới, `OnCleanComplete` mở khóa.

## 6️⃣ Hệ thống hỗ trợ
- **WaitingAreaManager** – quản lý các slot chờ, thêm slot, kiểm tra trống/đầy, và kích hoạt tutorial Clean.
- **BoxManager / Box** – cung cấp `GetFirstBox()`, `HasAvailableSlot()`, `PlayChangeAnimation()`, `SetRainbowColor()`, và lên lịch sinh box ưu tiên thấp.
- **Databuckets** – ghi lại analytics cho việc nhận, tiêu, mua booster.
- **Tutorials** – `BoosterAddHoleTutorial`, `BoosterCleanTutorial`, `BoosterMagicBoxTutorial` cấp booster miễn phí và hiển thị chỉ dẫn UI khi người chơi chưa từng thấy.

## 7️⃣ Tái tạo nhanh cơ chế
1. **Định nghĩa Enum & Config** – sao chép `BoosterType` và hai danh sách `LIST_BOOSTER_COST`, `LIST_BOOSTER_UNLOCK`.
2. **Lưu trữ số lượng** – triển khai `Dictionary<BoosterType,int>` với load/save (JSON hoặc PlayerPrefs).
3. **UI Button** – một prefab duy nhất với các trạng thái (`Lock/Noti/Plus`). Gắn `OnClick` tới một manager trung tâm.
4. **Manager trung tâm** – thực hiện ba phương thức công khai (`CanUseBooster`, `UseBooster`, các helper riêng cho từng loại). Dùng coroutine hoặc async/await cho delay.
5. **Hộp tạm** – cho Clean, có thể tối giản bằng việc di chuyển nhanh các đối tượng len vào một container ẩn và trả lại; bỏ qua ghost nếu muốn tốc độ.
6. **MagicBox** – đơn giản hoá: không cần animation cầu vồng, chỉ gọi logic sinh mảnh len còn thiếu cho box đầu tiên.
7. **Analytics** – dùng log đơn giản hoặc SDK analytics hiện có.
8. **Logic mở khóa** – giới hạn booster dựa trên level và cờ tutorial.
9. **Mua booster** – có thể tích hợp UI mua (coin hoặc quảng cáo) và gọi `BoosterManager.UseBooster`.

## 8️⃣ Đề xuất cấu trúc thư mục
```
Assets/_Game/Scripts/Gameplay/
│   BoosterType.cs
│   BoosterManager.cs
│   BoosterItem.cs
│   WaitingAreaManager.cs
│   BoxManager.cs
│   Box.cs
│   ...
Assets/_Game/Prefab/UI/BoosterBtn.prefab   // UI button
Assets/_Game/Scripts/UI/Popup/MoreBoosterPopup.cs
```

## 📁 Các file liên quan

- **C# Scripts**
  - `Assets/_Game/Scripts/Gameplay/BoosterType.cs`
  - `Assets/_Game/Scripts/Gameplay/BoosterManager.cs`
  - `Assets/_Game/Scripts/Gameplay/BoosterItem.cs`
  - `Assets/_Game/Scripts/Gameplay/WaitingAreaManager.cs`
  - `Assets/_Game/Scripts/Gameplay/BoxManager.cs`
  - `Assets/_Game/Scripts/Gameplay/Box.cs`
- **Prefabs**
  - `Assets/_Game/Prefab/UI/BoosterBtn.prefab`
  - `Assets/_Game/Prefab/UI/MoreBoosterPopup.prefab` (if exists)
- **Textures / Sprites**
  - `Assets/_Game/Textures/Button_Boosters.png`
  - `Assets/_Game/Textures/FX_Booster_Rainbow.png`

---
*Được tạo tự động từ code hiện có. Điều chỉnh đường dẫn hoặc tên lớp nếu bạn đổi cấu trúc.*