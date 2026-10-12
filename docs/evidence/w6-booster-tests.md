# W6 Booster test evidence (nháp — chờ chạy Editor)

- Date: 2026-10-12
- Unity: `6000.3.21f1` (theo `game/ProjectSettings/ProjectVersion.txt`)
- Test platform: PlayMode (`game/Assets/Tests/PlayMode`, assembly `ResultPopupProgressionTests`)
- Result: **chưa chạy — cần mở Unity Editor để verify**

## Đã thêm

| File | Loại | Nội dung |
|---|---|---|
| `Assets/_Game/Scripts/Utils/BoosterDebugPanel.cs` | Harness thủ công, editor-only (`#if UNITY_EDITOR`) | Panel OnGUI chữ lớn: đúng 3 nút `Dùng AddSlot/Clean/MagicBox` + hiện Level và số slot waiting |
| `Assets/Tests/PlayMode/BoosterLogicTests.cs` | Unit test, không cần scene (6 test) | Priority 1/2/0/-1 của `YarnPartInfo`, `GameData` trừ kho, config 3 booster |
| `Assets/Tests/PlayMode/BoosterSceneTests.cs` | Integration test, scene Gameplay (3 test) | AddSlot +1 tới trần, spam không vượt trần, `CanUseBooster(Clean)` khớp `IsAllEmpty` |

## Cách chạy test tự động

1. Mở project `game/` bằng Unity `6000.3.21f1`.
2. Menu **Window → General → Test Runner**, tab **PlayMode**, bấm **Run All**.
3. Kỳ vọng: 9 test mới pass (6 logic + 3 scene) + các test cũ vẫn xanh.

Điều kiện tiên quyết cho test scene: scene `Gameplay` load được, level load xong
(`CanInteractWithGame == true` trong 25s), `waitingSlotPrefab` và `vfxUnlockSlot`
được gán trên `WaitingAreaManager` (nếu thiếu, test AddSlot đỏ — đúng ý nghĩa wiring check).

## Checklist thủ công bằng panel (MagicBox/Clean cần mắt thường)

1. Scene Gameplay đã có sẵn GameObject gắn `BoosterDebugPanel` → chỉ cần bấm Play.
2. Bấm **Dùng AddSlot**: số slot waiting +1, tới trần thì nút báo từ chối.
3. Tap vài sợi cho waiting có đồ → bấm **Dùng Clean**: sợi bay vào TempBox; chờ box mới refill thì TempBox tự ẩn.
4. Bấm **Dùng MagicBox**: box đầu chuyển rainbow, hút đúng `RequiredCount` sợi, 2 box sau ra màu min-priority.

## Sửa crash khi load level (2026-10-12)

- `WaitingAreaManager.Initialize` NRE ở dòng gom renderer fade: `addButton` chưa gán trong
  scene (hoặc thiếu `AddWaitingSlotButton`).
- Đã guard null `addButton` trong `Awake`/`Initialize`/`RefreshLayout` + helper
  `AddFadeRenderer` bỏ qua renderer null kèm log tên chỗ thiếu — level load được,
  wiring còn thiếu sẽ hiện rõ trong Console để sửa trong scene.

## Đấu dây scene Gameplay (2026-10-12)

- `BoosterManager` đã có GameObject + script nhưng cả 3 ref đều null → NRE ngay `Awake`.
- Đã đấu trực tiếp trong `Gameplay.unity`: `boxCamera` = OrthoCamera (camera render
  canvas ScreenSpaceCamera chứa TemBox), `tempBoxContainer` = RectTransform của `TemBox`.
- `broomSweep` để null (scene không có object này); code đã guard — Clean chạy rút gọn,
  không animation chổi.
- Đã guard null toàn bộ đường dùng `tempBoxContainer`/`boxCamera`/`broomSweep` trong
  `BoosterManager` để thiếu wiring chỉ log lỗi chứ không crash.

## Ghi chú trung thực

- Chưa chạy được ở đây (không có Unity Editor trong môi trường này).
- Mới verify: quét cú pháp Roslyn 305 file Assets = 0 lỗi; đối chiếu namespace/usings thủ công.
- `GameData` đang là stub (`Level()=1`, kho booster mặc định 99) nên test kho chỉ đúng với stub;
  khi có `GameData` gốc phải chạy lại toàn bộ.
