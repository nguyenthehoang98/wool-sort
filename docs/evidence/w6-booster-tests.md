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

## Checklist thủ công bằng UI thật (thay OnGUI)

> Cập nhật 2026-10-13 — đã dựng UI tương đương gốc bằng code, không copy prefab.

1. Bấm Play scene Gameplay: `BoosterManager.Awake` tự tạo `BoosterBarUI`
   nếu scene chưa có → hiện `BoosterContainer` (0,137) với 3 nút
   AddSlot (−263) / MagicBox (0) / Clean (+263), số đo theo
   `BoosterBtn.prefab` gốc (root 200×200, Frame 194×100, Notice 56×56…).
2. Kéo sprite từ `Assets/_Game/BoosterUI_Picked/` vào field của `BoosterBarUI`
   (`frameNormal`, `iconAddSlot`…) và `MoreBoosterPopupUI` (`cardBg`,
   `contentIcons`…); để trống vẫn chạy màu trơn để test logic.
3. Bấm từng nút khi còn lượt: trừ kho và chạy đúng effect
   (AddSlot +1 slot, Clean bay vào TempBox, MagicBox rainbow + hút sợi).
4. Bấm nút khi hết lượt (kho = 0): mở `MoreBoosterPopupUI`
   (title/desc/content theo gốc, cost từ `Configs.LIST_BOOSTER_COST`,
   nút coin/ads stub → grant + `UseBooster`).
5. Panel OnGUI cũ (`BoosterDebugPanel`) mặc định `showPanel = false`;
   chỉ bật khi cần chẩn đoán, không dùng làm UI chính.

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

## Nút trong scene + unlock level 1 + ảnh gốc (2026-10-13, theo yêu cầu)

- **Unlock mặc định level 1:** `Configs.LIST_BOOSTER_UNLOCK = { 1, 1, 1 }`
  (giá trị gốc `{ 2, 8, 4 }` giữ trong comment). `GameData` stub `Level()=1`
  nên cả 3 booster hiện trạng thái mở ngay.
- **Nút sửa trực tiếp trong scene, không sinh từ code:**
  - `Gameplay.unity` đã chèn sẵn `BoosterContainer` (con của SafeArea,
    anchor (0.5,0), pos (0,137)) + 3 nút `BoosterBtn_1/3/2`
    (AddSlot type 0 tại x=-263 size 180 / MagicBox type 2 tại x=0 /
    Clean type 1 tại x=+263, fileID 810000001+), mỗi nút đủ
    Button + Image + `BoosterItem` (type/buttonBooster/objNormal/objLock/
    objNoti/countText/objPlus đã đấu dây) + Frame/ImgIcon, FrameLock/ImgLock,
    Notice/TxtNum (TMP font BALOO của scene), IconPlus.
  - `BoosterBarUI.autoBuild` mặc định `false`: chỉ `ResolveSprites()` +
    gọi `Setup()` cho nút trong scene. `BoosterManager.Awake` giữ ensure
    (scene đã có sẵn nên không tạo thêm).
  - Verify bằng parse YAML: không có ref nào missing, type 0/2/1 đúng,
    sprite đúng 3 frame + 1 icon/loại.
- **Ảnh theo game gốc:** sprite trong scene trỏ thẳng guid trong
  `BoosterUI_Picked/` (= guid gốc từ `wool-sort (2)`): `Button_Boosters(Gray)`,
  `Icon_Lock_2/Plus/Noti`, `Icon_Slot_3/4`, `Icon_Broom_2/3`, `Icon_Spool_2/3`.
- Trạng thái: code + scene xong, **chờ bạn mở Unity kiểm tra hiển thị
  và bấm thử trong Play mode**.

- Stack: `BoosterItem.OnClick (BoosterItem.cs:146)` → `KitTooltipManager.Instance.Show`
  khi booster ở trạng thái Lock. Nguyên nhân: `KitTooltipManager` sống ở scene
  Entry/Home (`DontDestroyOnLoad`), mở thẳng scene Gameplay thì `Instance == null`.
- Sửa:
  - `BoosterItem.OnClick`: guard `GameManager/BoosterManager == null`, helper
    `ShowTooltip()` (có Instance thì Show, không thì `Debug.Log`, không crash).
  - `BoosterManager.CanUseBooster` + helper `ShowTooltip()` tương tự; `WaitingAreaManager`
    null thì cho phép dùng để test UI thay vì NRE.
  - `BoosterBarUI`: field nào chưa kéo sprite thì tự `Resources.Load("BoosterUI/<tên>")`;
    vẫn thiếu thì nút hiện màu fallback theo loại (AddSlot xanh lá / Clean cam /
    Magic tím) để nhìn thấy và bấm được; log hướng dẫn kéo từ `BoosterUI_Picked/`.
  - `MoreBoosterPopupUI`: `ResolveSprites()` + tự cập nhật lại sprite ở `Refresh()`
    (card/nút/content), thiếu thì màu trơn.
- Asset runtime: copy 19 PNG từ `BoosterUI_Picked/` sang
  `Assets/_Game/Resources/BoosterUI/` (**chỉ PNG, không copy .meta** để tránh trùng guid).
  `BoosterUI_Picked/` vẫn là nơi bạn edit; bản Resources là fallback tự load.
- Trạng thái: code xong, **chờ bạn Play lại scene Gameplay verify hết NRE + nút hiện hình**.

## Fix compile 2026-10-13 — CS0103 `MakeImage`

- Lỗi: `MoreBoosterPopupUI.cs(109,24): error CS0103: The name 'MakeImage' does not exist`.
- Nguyên nhân: `Build()` gọi `MakeImage(...)` nhưng file chỉ định nghĩa
  `MakeText`/`MakeButton`, thiếu hàm dựng `Image`.
- Sửa: bổ sung `MakeImage(parent, name, anchoredRatio, sizeDelta)` static,
  cùng quy ước vị trí card với `MakeText` (`x*380`, `y*980-490`), `preserveAspect = true`.
- Trạng thái: code xong, **chờ mở Unity verify hết lỗi compile** + Play test
  checklist UI thật ở trên.
