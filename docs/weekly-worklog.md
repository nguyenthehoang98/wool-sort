# Worklog triển khai theo tuần/ngày

Worklog này bám theo đề cương 9 tuần trong file DOCX. Đây là **mẫu ghi nhận thực tế**, không phải bằng chứng hoàn thành sẵn.

## Cách ghi

- Mỗi ngày điền `Ngày thực tế`, `Status`, `Evidence` và `Ghi chú/kết quả`.
- Chỉ dùng `DONE` khi acceptance của ngày đã pass.
- `BLOCKED` phải ghi rõ blocker và task tiếp theo; không đổi thành `DONE` để che thiếu source/test.
- Evidence có thể là commit, test log, ảnh, video, build artifact hoặc đường dẫn report.
- Backlog chi tiết nằm ở [`../tasks/todo.md`](../tasks/todo.md).

## Tuần 1 — Khảo sát luật chơi và yêu cầu

| Ngày | Công việc và đầu ra bắt buộc | Acceptance / verification | Ngày thực tế | Status | Evidence | Ghi chú/kết quả |
|---|---|---|---|---|---|---|
| W1D1 | Chốt scope, đối tượng người chơi, MVP và out-of-scope | Đối chiếu DOCX và `gamedesign/09-product-acceptance.md` | 2026-09-20 | DONE | `docs/week1-core-contract.md` | MVP, in/out scope và acceptance đã chốt. |
| W1D2 | Mô hình hóa piece, layer, box, waiting, session state | Mỗi state có invariant và transition | 2026-09-20 | DONE | `docs/week1-core-contract.md` | Có state machine, invariant và transition table. |
| W1D3 | Chốt command, input gate, thứ tự chọn box và win/lose | Wrong color/full/waiting full đều có rule | 2026-09-20 | DONE | `docs/week1-core-contract.md` | Có validation order, tie-break, input gate và outcome rules. |
| W1D4 | Viết core contract và test matrix | Map đủ sang `ai-docs/01-core-gameplay.md` | 2026-09-20 | DONE | `docs/week1-core-contract.md`, `docs/features/F01-core-puzzle.md`, `docs/evidence/w1-editmode-tests.md` | Pure resolver có 16/16 EditMode tests pass; chưa có scene smoke test. |
| W1D5 | Review checkpoint tuần 1 | Không còn rule mơ hồ; ghi open questions | 2026-09-20 | DONE | `docs/week1-core-contract.md` | Đã chốt tie-break, waiting order, Rainbow và animation boundary; chưa có open decision còn thiếu. |

## Tuần 2 — Cấu trúc Unity, scene, prefab và resource

| Ngày | Công việc và đầu ra bắt buộc | Acceptance / verification | Ngày thực tế | Status | Evidence | Ghi chú/kết quả |
|---|---|---|---|---|---|---|
| W2D1 | Chốt Unity `6000.3.21f1`, inspect reference `wool-sort`, ghi scenes/packages và xác nhận target Unity của `puzzle-game` | `game/ProjectVersion.txt` khớp 6000.3.21f1; reference không được copy code |  | TODO | `game/ProjectSettings/ProjectVersion.txt`; người dùng xác nhận giữ Unity 6 | Target đã chốt; inventory scenes/packages còn lại sẽ làm ở W2D1.
| W2D2 | Inventory scene/prefab/resource/mesh/config/backup | Có đường dẫn và vai trò cho asset quan trọng |  | TODO |  |  |
| W2D3 | Lập package/platform matrix | Có version, mục đích, owner và rủi ro |  | TODO |  |  |
| W2D4 | Chốt `LevelDefinition`, order/asset ID, seed và validator rules | Không dùng order như asset ID |  | TODO |  |  |
| W2D5 | Baseline build và checkpoint dữ liệu | Project mở/build được hoặc có log blocker |  | TODO |  |  |

## Tuần 3 — Entry, Home, chọn level và progression

| Ngày | Công việc và đầu ra bắt buộc | Acceptance / verification | Ngày thực tế | Status | Evidence | Ghi chú/kết quả |
|---|---|---|---|---|---|---|
| W3D1 | Entry load config/save và route level 1/later levels | Pass cả hai nhánh, kể cả service fail | 2026-10-20 | PARTIAL | `game/Assets/Scenes/EntryScene.unity` | Scene `Entry` đã có và trỏ đúng GUID trong `EditorBuildSettings.asset`; driver `EntryBootstrap` + `EntryFlow`. Chưa verify nhánh level ≥ 2 vì thiếu scene `Home` (xem W3D2). |
| W3D2 | Home, level selection và modal intents | Popup không nhận input xuyên xuống gameplay |  | TODO |  |  |
| W3D3 | Progression/economy facade | Coin/heart/booster không âm; retry idempotent |  | TODO |  |  |
| W3D4 | Versioned save/load và migration | Reload giữ progress/resource |  | TODO |  |  |
| W3D5 | Navigation smoke checkpoint | Entry → Home/Gameplay → reload không stale state |  | TODO |  |  |

## Tuần 4 — Core gameplay và vertical slice đầu

| Ngày | Công việc và đầu ra bắt buộc | Acceptance / verification | Ngày thực tế | Status | Evidence | Ghi chú/kết quả |
|---|---|---|---|---|---|---|
| W4D1 | Implement/test pure puzzle resolver | Test placement, selection, no-op, outcome pass |  | TODO |  |  |
| W4D2 | Level mapper và validator cơ bản | Level order 1 map đúng real asset 3 |  | TODO |  |  |
| W4D3 | Gameplay session adapter cho board/box/slot/waiting | Event state không phụ thuộc animation callback |  | TODO |  |  |
| W4D4 | Layer spawn và tap/hold/rotate/scale interaction | Piece không bị placement hai lần |  | TODO |  |  |
| W4D5 | Chạy level đầu từ load đến win/lose | Có input/event/outcome log |  | TODO |  |  |

## Tuần 5 — Level content, win/lose và regression core

| Ngày | Công việc và đầu ra bắt buộc | Acceptance / verification | Ngày thực tế | Status | Evidence | Ghi chú/kết quả |
|---|---|---|---|---|---|---|
| W5D1 | Hoàn thiện content validator | Báo missing/duplicate/màu/layer/backup trước build |  | TODO |  |  |
| W5D2 | Cấu hình campaign và Normal/Hard/VeryHard | Thêm level không sửa core |  | TODO |  |  |
| W5D3 | Win/lose/restart/reward | Win/lose chỉ emit một lần, animation scaling added, popups hidden on level start |  | DONE |  |  |
| W5D4 | Deterministic seed và command replay | Cùng seed + input cho cùng event log |  | TODO |  |  |
| W5D5 | Checkpoint 3 level đại diện | Core regression pass; bug có reproduction |  | TODO |  |  |

## Tuần 6 — Booster, tutorial, UI, FX, audio và settings

| Ngày | Công việc và đầu ra bắt buộc | Acceptance / verification | Ngày thực tế | Status | Evidence | Ghi chú/kết quả |
|---|---|---|---|---|---|---|
| W6D1 | Add Slot: cap, cost, inventory, layout | Không vượt tối đa 7 slot |  | TODO |  |  |
| W6D2 | Clean: temporary storage và failure safety | Không mất waiting piece khi animation/ad fail |  | TODO |  |  |
| W6D3 | Magic Box và revive | Đúng active box; double tap chỉ chạy một operation |  | TODO |  |  |
| W6D4 | Tutorial data/runtime/skip/version flag | Không lặp tutorial đã hoàn tất sau reload |  | TODO |  |  |
| W6D5 | Win/lose UI, FX, audio, vibration, localization/settings | Popup khóa input; toggle có hiệu lực ngay |  | TODO |  |  |

## Tuần 7 — IAP, ads và platform services

| Ngày | Công việc và đầu ra bắt buộc | Acceptance / verification | Ngày thực tế | Status | Evidence | Ghi chú/kết quả |
|---|---|---|---|---|---|---|
| W7D1 | Tách Ads/Purchase/Analytics/Localization/Audio adapters | Core chạy với null/no-op services |  | TODO |  |  |
| W7D2 | Rewarded ad flow cho revive/reward | Chỉ grant sau callback success |  | TODO |  |  |
| W7D3 | IAP catalog, verified transaction, pending, restore | Transaction ID và No Ads restore idempotent |  | TODO |  |  |
| W7D4 | Shop và daily reward integration | Giá localized; lỗi SDK không crash core |  | TODO |  |  |
| W7D5 | Monetization failure checkpoint | Success/fail/retry/offline không mất/nhân resource |  | TODO |  |  |

## Tuần 8 — Regression, Android device và performance

| Ngày | Công việc và đầu ra bắt buộc | Acceptance / verification | Ngày thực tế | Status | Evidence | Ghi chú/kết quả |
|---|---|---|---|---|---|---|
| W8D1 | Chạy automated acceptance suite | Có pass/fail theo test ID |  | TODO |  |  |
| W8D2 | Test pause/resume, reload, kill/relaunch, reward | Không stale state hoặc duplicate reward |  | TODO |  |  |
| W8D3 | Test device matrix, touch, safe area, portrait | Ghi model/OS/build/result |  | TODO |  |  |
| W8D4 | Profiling startup/load/FPS/memory/animation | Có baseline và threshold chấp nhận |  | TODO |  |  |
| W8D5 | Sửa P0/P1 và retest | Bug log có reproduction và evidence |  | TODO |  |  |

## Tuần 9 — Android build, báo cáo và bảo vệ

| Ngày | Công việc và đầu ra bắt buộc | Acceptance / verification | Ngày thực tế | Status | Evidence | Ghi chú/kết quả |
|---|---|---|---|---|---|---|
| W9D1 | Chốt Android build config, version, orientation, signing reference | Build reproducible; không commit secret/keystore |  | TODO |  |  |
| W9D2 | Build APK/AAB và release smoke test | Entry → Home/Gameplay → Win/Lose pass |  | TODO |  |  |
| W9D3 | Gom report/evidence pack | Mỗi mục DOCX có tài liệu hoặc minh chứng |  | TODO |  |  |
| W9D4 | Viết hướng dẫn, demo script, slide/checklist bảo vệ | Người khác chạy theo được |  | TODO |  |  |
| W9D5 | Final handover và đóng backlog | Known limitations, build/test instructions đầy đủ |  | TODO |  |  |

## Vòng 1 — Port `EntryScene` từ dự án gốc (2026-10-20)

Nguồn gốc: reference Unity project thật ở `D:\BB_Project\wool-sort` (Unity `2022.3.50f1`), scene `Assets/Scenes/EntryScene.unity` (96 KB, 20 GameObject, 35 MonoBehaviour).
Path `C:/Users/Admin/Documents/Projects/wool-sort` ghi trong `tasks/todo.md` **không tồn tại** trên máy này.

### Đã đổi

| Nhóm | Trước | Sau | Ghi chú |
|---|---|---|---|
| Scene | `game/Assets/Scenes/` chỉ có `Gameplay.unity` | thêm `EntryScene.unity` (31 KB) + `.meta` | Giữ nguyên GUID `cbee448845f34a54d8ca189f7f7b56d7` → `EditorBuildSettings.asset` không phải sửa |
| Driver scene | `GameEntry.cs` (thiếu, cần `BravestarsSDK`) | `EntryBootstrap` + `EntryFlow` (code native của target) | `GameEntry` gốc không compile được: kéo `KitSDK`, `KitSetting`, `UnityEventBus`, `BravestarsSDK` |
| Prefab instance | 12 (11 không tồn tại ở target) | 1 (`_KIT/GUI/Prefab/TooltipManager.prefab`) | Xóa SDKManager, IAPManager, SettingManager, CanvasLoading, GameAnalytics, UMPManager, MediationManager, FrameManager, AppOpenAdManager, CheckQC, DEVTOOL |
| GameObject | 20 | 8 | Xóa `HeartSystemManager`, `InAppReview`, `Databuckets`, `DayChangeService`, `LanguageManager`, `AdLoading`, `NoAdsBanner`, `ImgPanel`, `BtnNoAds`, `Extension`, `Holder` (script hoặc sprite không tồn tại ở target) |
| Font TMP | `LiberationSans SDF.asset` (không có ở target) | `GoogleSans-Bold SDF.asset` | Theo đúng pattern của `Gameplay.unity` |
| Sprite nền | `28fb229f0b2b75c449785d2275d45b28` | `null` + màu nền `{0.212, 0.227, 0.259}` | GUID này **đã hỏng sẵn trong dự án gốc** (không có trong Assets cũng không phải built-in của Unity) → nền trắng; đã đổi sang màu lấy từ `RenderSettings.m_AmbientSkyColor` của scene gốc |
| `.meta` thiếu | 41 asset không có `.meta` | 0 | 8 file `.cs` quan trọng (`Core/*.cs`, `Meta/HomeUI.cs`, `Meta/ShopUI.cs`) **chưa từng được Unity compile** vì thiếu `.meta`; 33 folder + 1 thư mục IDE junk `.idea` đã dọn |
| Lỗi compile | 2 lỗi namespace ẩn | 0 lỗi | `ShopService.cs` và `ShopUI.cs` thiếu `using _Game.Scripts.Gameplay;` (dùng `BoosterType`) |

### Đã giữ

| Nhóm | Giữ lại | Lý do |
|---|---|---|
| Cấu trúc scene | `Canvas` → `BG` → `Text (TMP)`, `SafeArea`, `Main Camera`, `EventSystem`, `SpashManager` | Bố cục và transform lấy nguyên từ scene gốc |
| `TooltipManager` prefab | `_KIT/GUI/Prefab/TooltipManager.prefab` | GUID khớp, có sẵn ở target |
| `KitSafeArea`, `KitScaleCanvas`, `KitButton`, `SplashItem` | nguyên bản | GUID khớp 1-1 với target |
| Routing | level 1 → `Gameplay`, level ≥ 2 → `Home` | Giữ đúng ngữ nghĩa của `GameEntry` gốc, nhưng cài bằng `EntryFlow` |

### Verification

| Kiểm tra | Kết quả |
|---|---|
| Validator scene (fileID, GUID, cây transform, `SceneRoots`) | PASS — 43 block, 7 root, mọi GUID resolve |
| Compile `Assembly-CSharp` (Unity Roslyn + rsp của Bee) | PASS — 0 error, 15 warning (đều pre-existing: `FindObjectOfType` deprecated, field không dùng) |
| Compile `Assembly-CSharp-Editor` | PASS — 0 error, 4 warning |
| Compile `ResultPopupProgressionTests` (asmdef test) | PASS — 0 error |
| Audit asset toàn repo | 0 thiếu `.meta`, 0 `.meta` mồ côi, 0 GUID trùng, 56 dangling ref (đều là GUID của package — không tăng so với trước) |

### Còn thiếu

- Scene `Home.unity` **chưa tồn tại** ở target nhưng `EditorBuildSettings.asset` vẫn trỏ tới → Build Settings báo missing scene. Bản gốc không port được (`HomeFrame.prefab`, `HomeController.cs`, `_HomeFrame/HomeFrame.cs` đã bị prune). Đã tạo task `W3-01b`.
- Chưa có Play-mode smoke test trong Editor (bị chặn bởi W3-01b cho nhánh level ≥ 2).

## Vòng 2 — Kiểm tra sau khi xóa nhầm `_Game/Scripts` (2026-10-20)

### Chẩn đoán

Đọc **Recycle Bin** (giải mã record `$I` → đường dẫn gốc) cho thấy lúc **15:50** folder `game/Assets/_Game/Scripts` (448 KB) bị xóa nhầm rồi được khôi phục; **15:56** xóa tiếp 15 folder rỗng còn sót (`_Game/Editor`, `Icon`, `Media`, `Prefabs`, `Sounds`, `Scripts/{Board,Canh,Configs,Home,Manager,TestSpiral}`, `Textures/{Home,String_Fabric_Pattern,Wool_Pattern_2}`) — **không mất script nào** vì các folder này rỗng.

Đối chiếu `git ls-tree -r HEAD` với đĩa: **3118/3118 entry còn nguyên, 0 file bị mất**. GUID `Scripts.meta` (`723c4225…`) không đổi (diff chỉ là khác biệt LF/CRLF).

### Đã sửa

| Vấn đề | Nguyên nhân | Cách sửa |
|---|---|---|
| `CS0246: 'BoosterType' could not be found` (`ShopService.cs:14`) | Khôi phục folder đã **rollback** fix `using _Game.Scripts.Gameplay;` của Vòng 1 | Thêm lại `using _Game.Scripts.Gameplay;` |
| `CS0246: 'BoosterType' could not be found` (`ShopUI.cs:142`) | Như trên | Thêm lại `using _Game.Scripts.Gameplay;` |
| `CS0246: '_Game' could not be found` (`KitPool.cs:3`) | Lỗi **tạm thời** lúc folder `_Game/Scripts` đang nằm trong Recycle Bin, Unity compile giữa chừng | Tự hết khi folder về lại; hiện 0 lỗi |
| `woodring.fbx` thiếu | Prune trước bỏ sót, nhưng `Slot.prefab` + `SlotWaiting.prefab` vẫn tham chiếu | Copy lại từ reference kèm `.meta` (giữ GUID `26f39453…`) |
| `Frame_Tooltips.png` thiếu | Prune trước bỏ sót, `_KIT/GUI/Prefab/TooltipManager.prefab` vẫn tham chiếu | Copy lại từ reference kèm `.meta` (giữ GUID `8dbbc090…`) |
| `Readme.asset` → `Readme.cs` thiếu | Folder `TutorialInfo/` bị prune nhưng asset `Readme.asset` ở gốc `Assets/` còn lại | Copy lại gói `TutorialInfo/` từ reference (script + editor + icon + layout) |

### Verification

| Kiểm tra | Kết quả |
|---|---|
| Missing script references (scene/prefab/asset → script GUID không resolve) | **0** (trước: 1) |
| Asset thiếu nhưng folder cha còn tồn tại | **0** (trước: 2) |
| Compile `Assembly-CSharp` / `-Editor` / `ResultPopupProgressionTests` | PASS — 0 error cả 3 assembly |
| Audit asset | 0 thiếu `.meta`, 0 `.meta` mồ côi, 0 GUID trùng; dangling 56 → **52** (phần còn lại đều là GUID của package) |
| `EntryScene.unity` validator | PASS — 0 lỗi |

## Tổng kết sau mỗi tuần

| Tuần | Kết quả đạt được | Task chưa xong | Blocker | Evidence chính | Quyết định tuần sau |
|---|---|---|---|---|---|
| 1 | Scope, state, command, core resolver và acceptance contract đã chốt | Chưa có Application/Unity scene integration | Không có blocker cho pure core; W4-01 tiếp tục mở rộng/integrate | `docs/week1-core-contract.md`, `docs/features/F01-core-puzzle.md`, `docs/evidence/w1-editmode-tests.md` | Bắt đầu W2 inventory/data; giữ Core thuần C# và reskin qua ThemeProfile. |
| 2 |  |  |  |  |  |
| 3 | Scene `Entry` đã có trong target, trỏ đúng GUID build settings, driver native (`EntryBootstrap`/`EntryFlow`); compile sạch 3 assembly | W3-01 chưa verify nhánh level ≥ 2; W3-01b (dựng scene `Home`) chưa làm | Thiếu scene `Home` — bản gốc phụ thuộc `HomeFrame.prefab`/`HomeController.cs` đã bị prune | `game/Assets/Scenes/EntryScene.unity`, `docs/weekly-worklog.md` (Vòng 1) | Quyết định cần duyệt: giữ bản port không-SDK hay hồi sinh SDK stack; xem `tasks/todo.md` |
| 4 |  |  |  |  |  |
| 5 |  |  |  |  |  |
| 6 | Booster playable ở bản core: test AddSlot/Clean/MagicBox, panel debug, đấu dây scene | W6-01..03 còn chờ chạy Test Runner + Play scene | Thiếu source gốc (GameData/SettingManager/shop) nên dùng stub | `docs/evidence/w6-booster-tests.md`, `booster-html.html` | Giữ stub tới khi có source gốc; chạy test rồi mới đóng W6 |
| 7 |  |  |  |  |  |
| 8 |  |  |  |  |  |
| 9 |  |  |  |  |  |
