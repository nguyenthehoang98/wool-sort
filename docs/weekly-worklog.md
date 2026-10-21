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

## Vòng 3 — Dựng scene `Home` + luồng Entry → Home → Gameplay (2026-10-20)

Yêu cầu: dựng `Home` scene nhưng **chỉ làm luồng** — Home có nút vào Gameplay, Shop chưa cần UI, Coming soon rỗng, và 3 nút ở đáy.

### Scene `Home` (dựng mới, không port từ bản gốc)

`Home.unity` của bản gốc phụ thuộc `_Game/_HomeFrame/Resources/HomeFrame.prefab` + `HomeController.cs` + `_HomeFrame/HomeFrame.cs` — cả 3 đã bị prune nên **không port được**. Dựng mới bằng UI native của target, giữ nguyên GUID `cbc1002ad8ffb9f49972eb4440f5da68` (khớp `EditorBuildSettings.asset`).

```
Main Camera                      Camera + AudioListener
EventSystem                      EventSystem + StandaloneInputModule
Canvas                           Canvas + CanvasScaler(1080x1920) + GraphicRaycaster + KitScaleCanvas
└── SafeArea                     KitSafeArea
    ├── CoinBar                  TMP (txtCoin)  ← EconomyService.GetCoin()
    ├── HomePage
    │   ├── TxtLevel             TMP "LEVEL 1"  ← EconomyService.GetLevel()
    │   └── BtnStart             Button + KitButton "PLAY" → EntryFlow.StartCurrentLevel()
    ├── ShopPage                 (rỗng — placeholder, chưa làm UI shop)
    ├── ComingSoonPage           (rỗng — placeholder)
    └── TabBar                   3 nút ở đáy
        ├── BtnTabHome           "HOME"
        ├── BtnTabShop           "SHOP"
        └── BtnTabComingSoon     "SOON"
HomeManager                      HomeUI (script native của target) — nối hết reference ở trên
```

`HomeUI` (đã có sẵn trong target) lo phần chuyển tab: `ShowTab()` bật/tắt `homePage` / `shopPage` / `comingSoonPage` và khoá nút tab đang chọn.

### Sửa code để khép kín luồng

| Vấn đề | Sửa |
|---|---|
| Entry nhảy thẳng Gameplay (level 1) thay vì vào Home — đúng hành vi bản gốc nhưng không đúng luồng mong muốn (Entry là scene boot, Home là menu chính) | `EntryFlow` thêm cờ `alwaysGoHome` (mặc định **bật**) → Entry luôn vào Home. Tắt cờ thì quay về hành vi bản gốc (level 1 → Gameplay). Đã serialize `alwaysGoHome: 1` trong `EntryScene.unity` |
| `GameManager` luôn load `debugStartLevel` (=1) → nút PLAY trên Home mở sai level | `GameManager.Start()` ưu tiên level trong `EconomyService` khi `debugStartLevel <= 1`; `debugStartLevel > 1` vẫn là override để dev test |
| `GameManager.LoadNextLevel()` không ghi level về save → `EconomyService.currentLevel` kẹt ở 1 → Entry **không bao giờ** route sang Home | `LoadNextLevel()` gọi `EconomyService.Instance.SetLevel(nextLevel)` |
| `HomeUI` gọi `DontDestroyOnLoad` → reload scene Home sẽ giữ instance cũ với reference tới UI đã destroy, tab bar chết | Bỏ `DontDestroyOnLoad`, thêm `OnDestroy` reset `Instance` |
| Canvas trong `EntryScene` bị tắt sẵn ở bản gốc (nó nằm dưới `AdLoading`) → màn loading không hiện | Bật Canvas trong `EntryScene` |

### Ghi nhận: người dùng đang dọn project song song

Trong lúc làm, người dùng xóa trong Unity Project window lúc **16:06–16:09**: các folder rỗng còn sót (`_HomeFrame/`, `_SettingManager/`, `_UISystem/`, `Prefab/Utils`, `Textures/Image`) và 2 asset vừa được khôi phục ở Vòng 2 (`_Game/Mesh/woodring.fbx`, gói `TutorialInfo/`). **Không khôi phục lại** — tôn trọng lựa chọn của người dùng. Hệ quả còn lại (ghi nhận, không tự sửa):

- `Slot.prefab` + `SlotWaiting.prefab` trỏ tới `woodring.fbx` → mất mesh, Unity hiện "missing".
- `Readme.asset` trỏ tới `TutorialInfo/Scripts/Readme.cs` → mất script trên asset Readme (chỉ là asset readme của template Unity, không ảnh hưởng gameplay).
- Người dùng cũng gỡ `Home` khỏi `EditorBuildSettings.asset` → `LoadScene("Home")` sẽ fail; đã thêm lại (bắt buộc cho luồng).

### Verification

| Kiểm tra | Kết quả |
|---|---|
| Validator `Home.unity` | PASS — 76 block, 4 root, 11 link cha–con, mọi GUID resolve, 0 key YAML trùng |
| Validator `EntryScene.unity` | PASS — 0 lỗi (kèm fix Canvas active) |
| Compile `Assembly-CSharp` / `-Editor` / `ResultPopupProgressionTests` | PASS — 0 error cả 3 assembly |
| Audit asset toàn repo | 0 thiếu `.meta`, 0 `.meta` mồ côi, 0 GUID trùng; dangling 55 (đều là GUID package + ref cũ của `YarnColorDatabase`) |
| `EditorBuildSettings.asset` | Đủ 3 scene: Entry → Home → Gameplay |

### Còn lại

- Chưa có Play-mode smoke test trong Editor (bắt buộc để chốt W3-01b).
- UI Shop, chọn level, Settings, Daily Reward chưa làm (theo phạm vi đã chốt).

## Vòng 4 — Pick menu bottom gốc (`MenuBar`) + vuốt đổi tab (2026-10-20)

Yêu cầu: pick menu + menu bottom ở Home (3 nút), lấy **code và UI trước**.

### Tìm thấy ở bản gốc

`D:\BB_Project\wool-sort\Assets\_Game\_HomeFrame\`:

| File | Vai trò |
|---|---|
| `Resources/MenuBar.prefab` | prefab menu bottom: `BottomBar` → `Tabs` → `ButtonHome` / `ButtonShop` / `ButtonRanking` |
| `GUI/HomeScreen/MenuBarManager.cs` | quản lý 3 nút, focus/unfocus, `OptionMenu` |
| `GUI/HomeScreen/MenuBarButton.cs` | một nút: tween scale + ẩn/hiện nền focus |
| `GUI/FitPositionWithAds.cs` | đặt thanh bottom (bản gốc đẩy lên khi có banner ads) |
| `SwipUIRaycast.cs` | **kéo thả / vuốt ngang** để đổi tab |

Lưu ý: bản gốc dùng **3 nút Home / Shop / Ranking** (nút Ranking hiện "Coming soon!"). Map sang target: Home / Shop / **ComingSoon**.

### Code đã pick (giữ nguyên path + GUID → prefab không phải sửa)

Đặt đúng path của bản gốc và copy `.meta` để giữ GUID:

| File | Thay đổi so với bản gốc |
|---|---|
| `_Game/_HomeFrame/GUI/HomeScreen/MenuBarButton.cs` | bỏ `SettingManager.PlaySound` + `LFramework.Vibration` (target chưa có 2 hệ này, `_Game/Sounds` rỗng) |
| `_Game/_HomeFrame/GUI/HomeScreen/MenuBarManager.cs` | bỏ `Databuckets`/`IAPPlacement`, `GameConst.OnReloadBanner`, `KitSDK` analytics, `using Dreamteck`; `Singleton<T>` lấy bản target (`HyperSonic`); `MoveContentsBy` giữ nhưng có guard null |
| `_Game/_HomeFrame/GUI/FitPositionWithAds.cs` | bỏ `BravestarsSDK.MediationManager` → luôn đặt bar ở vị trí "no ads" |
| `_Game/_HomeFrame/SwipUIRaycast.cs` | bỏ `FrameManager.instance.CountFramesActive()`; vuốt giờ gọi `OpenHome/OpenShop/OpenRanking` để event về HomeUI |
| `_Game/Scripts/Utils/Extentions.cs` | copy nguyên văn (cần `CheckKillTween` cho DOTween) |

### UI đã pick

`MenuBar.prefab` + 6 texture, copy kèm `.meta` (giữ GUID): `Icon_Shop.png`, `Icon_Lock.png`, `Frame_Bar_1/2/3.png`, `Home_icon_big.png`.

Trong prefab có 1 sprite `5cbe0dc4…` **đã hỏng sẵn ở bản gốc** (không có trong Assets gốc) — nó gán cho 3 Image trên Button với **alpha 0** nên không ảnh hưởng hiển thị; đã set `m_Sprite: {fileID: 0}` để prefab resolve trọn vẹn.

### Nối vào scene `Home`

- Xoá tab bar tự dựng (3 nút GameObject rời) → thay bằng **instance của `MenuBar.prefab`** dưới `SafeArea` (PrefabInstance + stripped Transform + stripped MonoBehaviour để `HomeUI` tham chiếu được `MenuBarManager` bên trong prefab).
- `HomeUI` đổi từ 3 field Button → 1 field `MenuBarManager menuBar`; subscribe `OnClickOpenHome/OpenShop/OpenRanking` → `ShowTab(...)`.
- `CanvasScaler` của `Home` đổi 1080x1920 → **1080x2160** (khớp `Home.unity` gốc và `Gameplay.unity` của target; 3 nút rộng đúng 1080).

### Verification

| Kiểm tra | Kết quả |
|---|---|
| Compile 3 assembly | PASS — 0 error |
| Validator `Home.unity` / `EntryScene.unity` | PASS cả 2 |
| Dependency closure `MenuBar.prefab` | PASS — 11/11 GUID resolve, 0 thiếu |
| Audit asset toàn repo | 0 thiếu `.meta`, 0 `.meta` mồ côi, 0 GUID trùng |

## Vòng 5 — Pick tooltip + nền cho Entry scene (2026-10-20)

Yêu cầu: pick tooltip ở EntryScene + background Entry scene (kèm asset).

### Tooltip

`TooltipManager.prefab` (`_KIT/GUI/Prefab/TooltipManager.prefab`) **đã có** trong target và đã được instance trong `EntryScene` từ Vòng 1, nhưng còn thiếu 1 thứ: font TMP của prefab trỏ tới `_Game/Fonts/BALOO-REGULAR SDF.asset` — thư mục `_Game/Fonts` đã bị prune nên font mất.

Đã pick (kèm `.meta`, giữ GUID):

| File | GUID | Kích thước |
|---|---|---|
| `_Game/Fonts/BALOO-REGULAR SDF.asset` | `201d8ba9…` | 2.1 MB |
| `_Game/Fonts/BALOO-REGULAR.TTF` | `1e49e8a3…` | 630 KB |

Dependency closure của `TooltipManager.prefab` giờ **11/11 GUID resolve** (trước: 9/11).

> **Điều chỉnh sau khi người dùng review: bỏ font mới, dùng font có sẵn của dự án.**
> `_Game/Fonts/BALOO-REGULAR SDF.asset` + `.TTF` (2.7 MB) đã **bị gỡ** cùng folder `_Game/Fonts`; TMP trong `TooltipManager.prefab` được repoint sang `TextMesh Pro/Resources/Fonts & Materials/GoogleSans-Bold SDF.asset` (đúng font mà `Gameplay.unity` và `Home.unity` đang dùng), `m_sharedMaterial` → `fileID 2536004628413606107`. `GoogleSans-Bold SDF.asset` có `m_FallbackFontAssetTable: []` nên không sinh dangling mới. Closure vẫn **11/11 resolve**.

`BALOO-REGULAR SDF.asset` khai báo `NotoSansJP-Bold SDF.asset` làm **fallback font** — font Nhật 8.5 MB + TTF 5.4 MB cho game không có text Nhật (localization đã prune). Thay vì kéo 14 MB vào, đã **cắt entry fallback** (`m_FallbackFontAssetTable: []`), giữ nguyên line ending CRLF.

### Nền Entry

Sprite mà scene gốc trỏ tới (`28fb229f…`) **không tồn tại ở đâu**: không có trong `Assets` của dự án gốc, không phải built-in của Unity → nền Entry **đã hỏng sẵn ở bản gốc** (trước đó mình set null + màu nền để tạm).

Đã pick asset nền thật: `_Game/Textures/Home/background.png` (1082x2160 — khớp canvas 1080x2160), giữ GUID `e6a87fcc…`.

Áp vào `EntryScene`:
- `BG` Image: sprite → `background.png`, màu trắng, `m_RaycastTarget: 0` (nền phủ kín không được chặn click xuống UI).
- `BG` RectTransform: anchor `(0,0)→(1,1)`, sizeDelta 0 → phủ kín canvas (gốc là 400x400 ở giữa).
- `CanvasScaler` Entry: 1080x1920 → **1080x2160** cho khớp `Gameplay` + `Home`.

Vì asset này là nền Home, đã dùng luôn cho `Home.unity` (thêm `BG` là con đầu của `Canvas`, trước `SafeArea`).

### Verification

| Kiểm tra | Kết quả |
|---|---|
| Compile 3 assembly | PASS — 0 error |
| Validator `EntryScene` / `Home` | PASS cả 2 |
| Dependency closure `TooltipManager.prefab` | PASS — 11/11 resolve |
| Audit asset toàn repo | 0 thiếu `.meta`, 0 `.meta` mồ côi, 0 GUID trùng; dangling 55 → **54** |

Còn 2 dangling (đều do người dùng xóa ở Vòng 2, không khôi phục): `Readme.asset` → `TutorialInfo/Scripts/Readme.cs`, và `Slot.prefab`/`SlotWaiting.prefab` → `_Game/Mesh/woodring.fbx`.

## Vòng 6 — Pick nội dung shop (local, không IAP/IAA) (2026-10-20)

Yêu cầu: pick nội dung shop, chỉ giữ phần coin, **không làm IAP, IAA**, chỉ làm local các gói `{coin, booster}`.

### Bản gốc: loại bỏ gì

`HomeShop.cs` + `Shop.prefab` của dự án gốc là **100% IAP/IAA**:

| Phần | Loại | Vì sao |
|---|---|---|
| `Remove Ads Bundle` + `iconRemoveAd2.png` | bỏ | IAP (mua tiền thật) |
| Text `$1.99` | bỏ | giá IAP |
| `KitSDK.OpenAdReward("free_coin")` (xem ads nhận coin) | bỏ | IAA |
| `MediationManager.ToggleIsShowingAds` | bỏ | banner ads |
| `Databuckets.PushEventIAPShow` / `KitSDK.IsNoAds` / `IsBuyBeginnerBundle` | bỏ | analytics + IAP state |

Giữ lại: **gói coin + booster mua bằng coin** — khớp đúng danh sách `ShopService.items` của target (2 gói coin + 3 booster, tất cả trừ bằng `EconomyService` coin).

### Asset đã pick (copy kèm `.meta` → giữ GUID, đặt vào `Resources/Shop`)

| File | Dùng cho |
|---|---|
| `coinShopPack1.png` / `coinShopPack2.png` | icon 2 gói coin |
| `power1_120x120.png` / `power2_120x120.png` / `power3_120x120.png` | icon 3 booster (Add Slot / Clean / Magic Box) |
| `btn2Green.png` | nút BUY (có border → dùng Sliced) |
| `slot2.png` | nền 1 dòng item (không border → Simple) |
| `shopTopBar2.png` | thanh tiêu đề shop |

Đặt trong `_Game/Resources/Shop/` (giống `_Game/Resources/WinLoseUI/` sẵn có) để `Resources.Load` gọi được theo tên.

### Code

- `ShopItem`: thêm `iconResPath` (đường dẫn Resources).
- `ShopService`: mỗi item khai báo `iconResPath`; `Awake()` load icon qua `Resources.Load<Sprite>`, cảnh báo nếu thiếu.
- `HomeUI`: `ShowTab` gọi `shopUI.Refresh()`; `Awake` tự tạo `EconomyService`/`ShopService` nếu thiếu (để mở `Home.unity` test trực tiếp cũng được).

### ShopItem thành prefab (sửa sau review)

Ban đầu `ShopUI` tự dựng item bằng code (`BuildDefaultItem`). Đã đổi sang **prefab**:

- Thêm `_Game/Scripts/Meta/ShopItemView.cs` — MonoBehaviour gắn trên prefab, giữ serialized ref tới `icon / title / amount / cost / btnBuy / btnLabel`, và chứa logic bind + mua (logic trước nằm trong inner class của `ShopUI`).
- Thêm `_Game/Prefab/UI/ShopItem.prefab`:
  ```
  ShopItem        Image slot2 (820x150)
  ├── Icon       Image (sprite gán runtime)
  ├── Title      TMP
  ├── Amount     TMP
  ├── Cost       TMP
  └── BtnBuy     Button + Image btn2Green (Sliced)
      └── Label  TMP "BUY"
  ```
- `ShopUI` giờ **chỉ instantiate prefab rồi bind**: `Instantiate(itemPrefab, itemContainer)` → `GetComponent<ShopItemView>()` → `view.Bind(item, _shop, ShowNotify)`. Xoá hẳn `BuildDefaultItem` và inner class; thiếu prefab thì log lỗi rõ ràng.
- `ShopUI.itemPrefab` trong `Home.unity` trỏ tới prefab (GUID `81fd9d44…`).

### Scene `Home.unity`

```
ShopPage (dưới SafeArea)
├── ShopTopBar      Image shopTopBar2 + TMP "SHOP"
├── ItemContainer   RectTransform + VerticalLayoutGroup  ← itemContainer của ShopUI
└── TxtNotify       TMP (thông báo mua)                  ← txtNotify của ShopUI
```
`HomeUI.shopUI` trỏ tới component `ShopUI` trên `ShopPage`.

### Verification

| Kiểm tra | Kết quả |
|---|---|
| Compile 3 assembly | PASS — 0 error |
| Validator `Home.unity` / `EntryScene.unity` | PASS cả 2 |
| Audit asset toàn repo | 0 thiếu `.meta`, 0 `.meta` mồ côi, 0 GUID trùng |
| Dangling refs | 55 — tất cả là GUID script của **package** (`VerticalLayoutGroup` mới thêm); validator (có PackageCache) PASS |

## Vòng 7 — Coin HUD kiểu Gameplay cho tab Shop (2026-10-20)

Yêu cầu: cho một coin HUD giống Gameplay ở tab Shop; khi mua thì coin phải **± thật**.

### Coin HUD

Target **đã có sẵn** prefab `_Game/Prefab/CoinHUD.prefab` (dùng ở Gameplay):

```
CoinHUD          (anchor top-right, 360x100)
├── Bg          Image Frame_Popup_2.png
├── IconCoin    Image Icon_Coin_S.png (76x76)
└── TxtCoin     TMP "Coins: 0"
+ CoinHUD       (coinText → TxtCoin, iconCoin → IconCoin)
```

Nên thay vì dựng lại, đã **instance prefab này** vào `ShopPage` của `Home.unity` → **chỉ hiện ở tab Shop** (yêu cầu: không hiện ở cả 3 tab). Bỏ luôn `CoinBar` rời (TMP số trần) nên không có 2 màn hiển thị coin.

`ShopTopBar` đặt ở góc trên-trái (560x150 tại `(30,-30)`) để không đè lên coin HUD ở góc trên-phải.

> Hệ quả: tab Home và Coming Soon **không còn hiện số coin** (trước đó là `CoinBar` TMP số trần). Nếu muốn có coin ở 2 tab đó thì cần thêm lại một HUD riêng.

### CoinHUD dùng được ngoài Gameplay

`CoinHUD.cs` cũ chỉ nghe `CoinManager.OnBalanceChanged` — mà `CoinManager` chỉ tồn tại trong scene Gameplay, nên ở Home/Shop HUD sẽ đứng yên ở 0. Đã sửa:

- Thêm nguồn `EconomyService.OnCoinChanged` (nguồn coin chung của Home/Shop/Gameplay); `CoinManager` chỉ còn là lớp delegate trong Gameplay.
- `BindEconomy()` / `UnbindEconomy()` theo dõi instance đang subscribe (tránh subscribe trùng khi HUD bật/tắt nhiều lần).
- `Refresh()` ưu tiên `EconomyService`, fallback `CoinManager` (khi mở `Gameplay.unity` trực tiếp, không qua Entry).
- Bind lại ở `Awake`, `OnEnable` và `Start` vì thứ tự Awake giữa các object trong scene không xác định.

### Coin thật khi mua

`ShopService.Buy()` gọi `EconomyService.SpendCoins` / `AddCoins` / `AddBooster` — cả hai hàm tiền đều `Save()` rồi raise `OnCoinChanged`:

| Mua gói | Coin thay đổi |
|---|---|
| coin nhỏ (190) | −190 rồi +500 |
| coin lớn (490) | −490 rồi +1400 |
| booster (80/120/60) | chỉ trừ, không cộng coin |

Coin HUD subscribe `OnCoinChanged` nên hiển thị đúng ngay sau mỗi lần trừ/cộng; `ShopItemView.OnBuy` cũng khoá nút BUY khi không đủ coin.

### Verification

| Kiểm tra | Kết quả |
|---|---|
| Compile 3 assembly | PASS — 0 error |
| Validator `Home.unity` / `EntryScene.unity` | PASS cả 2 |
| Audit asset toàn repo | 0 thiếu `.meta`, 0 `.meta` mồ côi, 0 GUID trùng |

## Vòng 8 — Fix nút BUY không bấm được + test luồng mua (2026-10-20)

Báo cáo: "bấm buy chưa được". Log Unity cho thấy **chưa từng vào Play mode** (không có dòng "Entering Playmode") → trong Editor item shop **chưa tồn tại** vì được tạo lúc runtime. Nhưng khi rà lại code tìm ra 3 lỗi thật sẽ làm BUY hỏng lúc chạy.

### Lỗi đã sửa

| Lỗi | Nguyên nhân | Sửa |
|---|---|---|
| Shop **không dựng được item** khi mở `Home.unity` trực tiếp | `ShopUI.Awake()` cache `ShopService.Instance` một lần. `HomeUI.Awake` tạo service sau (thứ tự Awake giữa các object trong scene không xác định) → `_shop` null **vĩnh viễn** → `Refresh()` luôn return | `ShopUI` không cache ở Awake, mà resolve `ShopService.Instance` **lúc dùng** (`Refresh`). Tương tự `CoinHUD`, thêm `BindEconomy()` ở `Awake`/`OnEnable`/`Start` |
| Nút BUY **bị khoá** (xám) khi không đủ coin | `btnBuy.interactable = _shop.CanBuy(...)` → mặc định chỉ có 100 coin, 3/5 gói giá ≥ 120 → 3 nút khoá, bấm không có phản hồi gì | Nút **luôn bấm được**; khi không đủ coin sẽ hiện thông báo "Không đủ coin để mua". Giá chuyển **màu đỏ** khi không đủ, trắng khi đủ |
| Thông báo mua bị **menu bar che** | `TxtNotify` ở `y=90` trong khi menu bar (`MenuBar.prefab`) cao 180px ở đáy | `TxtNotify` lên `y=195`; `ItemContainer` đáy từ 0.1 → 0.16 để không đè nhau |

Thêm: `ShopUI` subscribe `EconomyService.OnCoinChanged` → khi coin đổi, trạng thái màu giá của mọi item cập nhật ngay (không phải reload shop).

### Test PlayMode

Thêm `Assets/Tests/PlayMode/ShopPurchaseTests.cs` (cùng asmdef `ResultPopupProgressionTests`): load scene `Home` → bật tab Shop → assert item được dựng ra → bấm BUY dòng "Magic Box" → assert **coin bị trừ** và **booster được cộng**.

### Verification

| Kiểm tra | Kết quả |
|---|---|
| Compile 3 assembly (kể cả test) | PASS — 0 error |
| Validator `Home.unity` / `EntryScene.unity` | PASS cả 2 |
| Audit asset toàn repo | 0 thiếu `.meta`, 0 `.meta` mồ côi, 0 GUID trùng |

> Test PlayMode **chưa chạy** (Editor đang mở nên không `-batchmode` được). Cần chạy trong Test Runner để có bằng chứng runtime.

## Vòng 9 — Nút BUY theo 4 trạng thái (2026-10-20)

Spec từ người dùng cho nút BUY trong `ShopItem`:

| Trạng thái | Hành vi mong muốn |
|---|---|
| Chưa nhấn | Nút ở trạng thái bình thường, bấm được |
| Nhấn + không đủ tiền | **Báo tooltip** |
| Nhấn + đủ tiền (mua booster) | Trừ tiền + cộng item |
| Nhấn + đủ tiền (mua gói coin) | Trừ tiền + cộng coin |

### Đã làm

- `ShopItemView.OnBuy`: kiểm tra `CanBuy` **trước**, không đủ coin → báo qua **`KitTooltipManager.Show()`** (tooltip của dự án, giống bản gốc) thay vì chỉ chữ thông báo; đủ coin → `ShopService.Buy()` (đã trừ coin rồi cộng booster hoặc coin theo loại gói) → báo tooltip "Mua thành công {tên}". `txtNotify` vẫn giữ làm fallback.
- `KitTooltipManager` (`_KIT/GUI/Runtime/`): thêm **singleton guard** trong `Awake`/`OnDestroy`. Trước đó `Instance = this` không kiểm tra nên scene nào có instance cũng ghi đè — giờ bản mới tự huỷ, giữ bản đã tồn tại (instance của `EntryScene` sống sang `Home` do `DontDestroyOnLoad`).
- `Home.unity`: thêm instance **`TooltipManager.prefab`** ở gốc scene (khối PrefabInstance được trích nguyên từ `EntryScene.unity`) → tooltip hoạt động cả khi mở `Home.unity` trực tiếp để test, không phụ thuộc đi qua Entry.

### Verify

| Kiểm tra | Kết quả |
|---|---|
| Compile 3 assembly | PASS — 0 error |
| Validator `Home.unity` / `EntryScene.unity` | PASS cả 2 |
| Audit asset toàn repo | 0 thiếu `.meta`, 0 `.meta` mồ côi, 0 GUID trùng |
| Hierarchy `Home.unity` | 5 root: Main Camera, EventSystem, Canvas, HomeManager, TooltipManager |

## Vòng 10 — Gói coin miễn phí + sửa nhánh +coin (2026-10-20)

Báo cáo: "khi bấm mua thì chưa + coin {mua gói coin miễn phí}".

### Chẩn đoán

| Nguyên nhân | Chi tiết |
|---|---|
| **Gói miễn phí không tồn tại** | `ShopService` **không có trong scene** nào — nó luôn được `EntryBootstrap`/`HomeUI` tạo lúc runtime (`new GameObject("ShopService").AddComponent<ShopService>()`). Vậy `items` luôn là list **hardcode trong C#**, không sửa được qua Inspector (dù field `icon` còn ghi "gán từ project gốc qua Inspector"). Thêm item trong Inspector sẽ bị mất khi chạy. |
| **Nhánh `+coin` không bao giờ chạy được** | Mặc định chỉ có **100 coin** (`EconomyService.defaultCoin`), mà gói coin giá **190 / 490** → `CanBuy` trả false → bấm BUY chỉ báo "không đủ tiền", không vào được nhánh `AddCoins`. |

### Đã làm

1. **Thêm gói "Gói coin miễn phí"** (`coin_pack_free`, giá **0**, cho **+100 coin**) vào `ShopService.items` — nằm đầu danh sách. Icon: copy `_Game/BoosterUI_Picked/Icon_Coin_S.png` → `_Game/Resources/Shop/Icon_Coin.png` (asset mới, GUID riêng).
2. **Gói miễn phí chỉ nhận 1 lần** — cờ mới `PlayerData.freeCoinPackTaken`:
   - `CanBuy` trả `false` nếu đã nhận.
   - `Buy` chặn với lý do `free_pack_already_taken`.
   - Cờ được set **trước** khi gọi `AddCoins`/`AddBooster` (hàm này gọi `Save()`, nên nếu set sau thì cờ không được ghi xuống PlayerPrefs).
3. **`ShopService`**: thêm `LastError` (lý do thất bại) + `GetPriceLabel(item)` → gói đã nhận hiện "Đã nhận" thay vì giá.
4. **`ShopItemView`**: tooltip dùng đúng lý do (`not_enough_coin` → "Không đủ coin để mua", `free_pack_already_taken` → "Bạn đã nhận gói coin miễn phí rồi", …).
5. **Test PlayMode** `FreeCoinPack_AddsCoin_OnlyOnce`: bấm BUY gói miễn phí → coin **tăng**; bấm lần 2 → coin **không đổi**.

### Ghi nhận thêm: khôi phục sửa của user trong Editor

Unity tạo `Assets/_Recovery/0.unity` (bản cứu scene) khi phát hiện `Home.unity` bị đổi trên đĩa. So sánh cho thấy bản đó chứa sửa tay của người dùng: **nền BG → `Frame_Bar_1.png` + Sliced** (giống hệt sửa trong `EntryScene`) — bị lần regenerate của tôi ghi đè mất. Đã:
- Khôi phục lại `Home.unity`: nền `Frame_Bar_1` + `m_Type: 1` (Sliced).
- Sửa generator để dùng `Frame_Bar_1` + Sliced cho `Home` → lần regenerate sau không còn ghi đè lựa chọn này.
- Xoá `Assets/_Recovery/` và thêm `game/Assets/_Recovery` + `.meta` vào `.gitignore` (artifact của Unity, không commit).

> Bài học: khi người dùng đang sửa scene trong Editor, phải sửa scene bằng text edit có chọn lọc thay vì regenerate từ đầu.

### Chỉnh giá gói coin về 0 (2026-10-20)

Theo yêu cầu: `coin_pack_small` 190 → **0**, `coin_pack_big` 490 → **0** (gói booster giữ nguyên 80/120/60).

Vì các gói coin giờ đều có `coinCost == 0`, luật "chỉ nhận 1 lần" (trước đó nhận diện bằng `coinCost <= 0 && coinAmount > 0`) sẽ vô tình khóa cả 3 gói. Đã đổi nhận diện theo **id** (`coin_pack_free`) → chỉ gói "Gói coin miễn phí" mới bị giới hạn 1 lần; "Gói coin nhỏ"/"Gói coin lớn" mua lại được.

| Gói | Giá | Nhận | Số lần |
|---|---|---|---|
| Gói coin miễn phí | 0 coin | +100 coin | 1 lần (`freeCoinPackTaken`) |
| Gói coin nhỏ | 0 coin | +500 coin | không giới hạn |
| Gói coin lớn | 0 coin | +1400 coin | không giới hạn |
| Add Slot | 80 coin | +1 booster | không giới hạn |
| Clean | 120 coin | +1 booster | không giới hạn |
| Magic Box | 60 coin | +1 booster | không giới hạn |

### Bỏ gói coin miễn phí (2026-10-20)

Theo yêu cầu: xoá gói `coin_pack_free` khỏi shop. Kèm dọn code chết:

- `ShopService.items`: bỏ dòng `coin_pack_free`.
- Bỏ toàn bộ luật "chỉ nhận 1 lần": hằng `FreeCoinPackId`, hàm `IsFreeCoinPack`, nhánh chặn trong `CanBuy`/`Buy`, nhánh `"Đã nhận"` trong `GetPriceLabel`, và lý do `"free_pack_already_taken"`.
- `PlayerData`: bỏ cờ `freeCoinPackTaken` (không còn dùng).
- `ShopItemView`: bỏ case `"free_pack_already_taken"` trong `ReasonToMessage`.
- Xoá asset `_Game/Resources/Shop/Icon_Coin.png` + `.meta` (copy làm riêng cho gói miễn phí, không còn tham chiếu — đã verify 0 ref theo GUID).
- Test PlayMode `FreeCoinPack_AddsCoin_OnlyOnce` → đổi thành `CoinPack_AddsCoin_AndIsRepeatable` (mua "Gói coin nhỏ" 2 lần → coin tăng 2 lần).

Shop còn 5 gói: Gói coin nhỏ (0 coin, +500), Gói coin lớn (0 coin, +1400), Add Slot (80), Clean (120), Magic Box (60).

### Verify

| Kiểm tra | Kết quả |
|---|---|
| Compile 3 assembly | PASS — 0 error |
| Audit asset toàn repo | 0 thiếu `.meta`, 0 `.meta` mồ côi, 0 GUID trùng |
| Ref còn lại tới asset đã xoá | 0 (kiểm tra theo GUID `2dd5a8f7…`) |

## Vòng 11 — Màn hình nhận thưởng sau khi mua (2026-10-20)

Yêu cầu: sau khi mua hàng phải có màn hình nhận thưởng; check + pick màn hình nhận thưởng từ dự án gốc.

### Bản gốc: `PurchaseSuccessPopup`

`_Game/Resources/PurchaseSuccessPopup.prefab` (102 KB) + `PurchaseSuccessPopup.cs` + `PurchaseSuccessItem.cs`. Popup dạng lưới 6 ô (No Ads / Coin / InfiHeart / Hole / Clean / Basket), tiêu đề "Purchase Successful!", nút Continue + Close.

**Phụ thuộc đã bị prune nên không port thẳng được:**

| Phụ thuộc | Trạng thái | Xử lý |
|---|---|---|
| `FrameBase` (lớp base của popup) | pruned | viết lại popup không kế thừa base |
| `UIScale`, `UIAnimation` | pruned | bỏ, dùng DOTween trực tiếp |
| `I2 Localize` (3 component) | pruned | bỏ, text viết thẳng tiếng Việt |
| `BALOO-REGULAR SDF` + 3 material outline | pruned | dùng font có sẵn `GoogleSans-Bold SDF` |
| `CoinBar.prefab` | pruned | bỏ |
| `Frame_Popup_1/2`, `Button_Green`, `Button_Close`, `Icon_Slot_3`, `Icon_Spool_2` | có sẵn ở target | dùng lại |
| `Frame_Popup_3.png`, `Frame_Popup_4.png` | thiếu | **đã pick** từ bản gốc vào `_Game/Textures/UIV2/` |

Các ô IAP (No Ads / InfiHeart / Basket) không dùng cho shop local nên bỏ — chỉ hiện đúng gói vừa mua.

### Đã làm

- **Asset**: pick `Frame_Popup_3.png` (278x68, 9-slice) + `Frame_Popup_4.png` (820x142) từ bản gốc → `_Game/Textures/UIV2/` (giữ nguyên GUID).
- **Script** `_Game/Scripts/Meta/PurchaseSuccessPopup.cs`: singleton (`Instance`), hiện/ẩn `shade` + `card`, đổi icon/tên/số lượng theo `ShopItem`, animation scale bằng DOTween, nút OK + Close đóng popup.
- **Prefab** `_Game/Prefab/UI/PurchaseSuccessPopup.prefab`:
  ```
  PurchaseSuccessPopup
  ├── Shade    (đen 0.75, chặn input)
  └── Card
      ├── Panel      Image Frame_Popup_1 (9-slice)
      │   ├── TitleBar  Image Frame_Popup_4 + TxtTitle "Nhận thưởng!"
      │   ├── Icon      Image (icon gói vừa mua)
      │   ├── TxtName   TMP
      │   ├── TxtAmount TMP
      │   ├── BtnOk     Button + Image Button_Green (9-slice) + "OK"
      │   └── BtnClose  Button + Image Button_Close
  ```
  Màu chữ TMP: trắng; tiêu đề 56, tên 44, số lượng 40 (vàng).
- **`Home.unity`**: instance `PurchaseSuccessPopup.prefab` dưới gốc `Canvas` (nằm trên cùng, ngoài `SafeArea` nên không bị che).
- **`ShopItemView.OnBuy`**: mua thành công → `PurchaseSuccessPopup.Instance.Show(item)`.
- **Test PlayMode** `Buy_ShowsRewardPopup`: popup phải ẩn trước khi mua và **hiện** sau khi bấm BUY.

### Verify

| Kiểm tra | Kết quả |
|---|---|
| Compile 3 assembly (kể cả test) | PASS — 0 error |
| Validator `PurchaseSuccessPopup.prefab` / `Home.unity` / `EntryScene.unity` | PASS cả 3 |
| Audit asset toàn repo | 0 thiếu `.meta`, 0 `.meta` mồ côi, 0 GUID trùng |
| Hierarchy `Home.unity` | Canvas → BG / SafeArea / **PurchaseSuccessPopup** |

## Vòng 12 — Đổi icon booster trong shop sang icon của Gameplay (2026-10-20)

Yêu cầu: icon booster trong shop dùng đúng icon của Gameplay; gỡ icon shop cũ.

Mapping lấy từ `Assets/_Game/BoosterUI_Picked/README.md` (bảng "nguồn gốc" của chính dự án) — icon thường, không phải icon khi khoá:

| Booster | Icon Gameplay | Đường dẫn Resources |
|---|---|---|
| Add Slot | `Icon_Slot_3.png` | `BoosterUI/Icon_Slot_3` |
| Clean | `Icon_Broom_2.png` | `BoosterUI/Icon_Broom_2` |
| Magic Box | `Icon_Spool_2.png` | `BoosterUI/Icon_Spool_2` |

Target **đã có sẵn** mirror `Assets/_Game/Resources/BoosterUI/` (19 PNG) nên không cần copy thêm asset.

Đã đổi `ShopService.items` (`iconResPath`) cho 3 gói booster sang `BoosterUI/...`, và **xoá** 3 icon booster cũ của shop: `Resources/Shop/power1_120x120.png`, `power2_120x120.png`, `power3_120x120.png` (+ `.meta`) — trước khi xoá đã verify không còn tham chiếu nào (chỉ chính `.meta` của chúng).

2 icon gói coin (`Shop/coinShopPack1/2.png`) **giữ lại** — không phải icon booster.

### Verify

| Kiểm tra | Kết quả |
|---|---|
| Compile 3 assembly | PASS — 0 error |
| Audit asset toàn repo | 0 thiếu `.meta`, 0 `.meta` mồ côi, 0 GUID trùng |
| 5 đường dẫn `iconResPath` trong `ShopService` | đều tồn tại trong `Resources/` |

## Vòng 13 — Icon shop gán bằng ref trong Inspector (2026-10-21)

Yêu cầu: bỏ load icon theo đường dẫn `Resources`; tạo chỗ giữ ref ảnh để kéo trực tiếp trong Inspector.

### Object nào sinh ra item?

`ShopUI` (component `164dc1da…` trên GameObject `ShopPage` của `Home.unity`) là object sinh item:

```
ShopUI.Refresh()  →  Instantiate(itemPrefab, itemContainer)  →  ShopItemView.Bind(item, shop, ShowNotify)
```

`ShopService` là object **runtime-created** (do `HomeUI.Awake` tạo khi thiếu) nên không thể gán sprite trong Inspector → ref ảnh phải nằm ở `ShopUI` (object có sẵn trong scene).

### Đã làm

- **`ShopUI.cs`**: thêm class `ShopIconRef { string id; Sprite sprite; }` + field `List<ShopIconRef> icons` (header "Icon từng gói"). `Refresh()` gọi `ResolveIcon(item.id)`; nếu có sprite thì gán `item.icon` trước khi `Bind` (để màn nhận thưởng cũng dùng được), nếu thiếu thì `Debug.LogWarning` báo đúng id.
- **`ShopService.cs`**: bỏ field `ShopItem.iconResPath` và hàm `LoadIcons()` (không còn `Resources.Load<Sprite>` nào).
- **`Home.unity`**: component `ShopUI` có sẵn mảng `icons` gồm 5 phần tử, `id` đã điền sẵn, `sprite` để trống cho user kéo vào:

  | id | Sprite cần kéo |
  |---|---|
  | `coin_pack_small` | `Resources/Shop/coinShopPack1.png` |
  | `coin_pack_big` | `Resources/Shop/coinShopPack2.png` |
  | `booster_addslot` | `BoosterUI_Picked/Icon_Slot_3.png` |
  | `booster_clean` | `BoosterUI_Picked/Icon_Broom_2.png` |
  | `booster_magicbox` | `BoosterUI_Picked/Icon_Spool_2.png` |

### Verify

| Kiểm tra | Kết quả |
|---|---|
| Compile 3 assembly | PASS — 0 error |
| Validator `Home.unity` + `EntryScene.unity` | PASS cả 2 |
| Audit asset toàn repo | 0 thiếu `.meta`, 0 `.meta` mồ côi, 0 GUID trùng |
| Grep `iconResPath` / `LoadIcons` trong code | 0 kết quả (chỉ còn trong docs lịch sử) |

## Vòng 14 — Làm rõ vị trí component ShopUI + fix prefab mismatch (2026-10-21)

Báo cáo: "mở home.unity ra nhưng không thấy 'Shop UI' nào cả".

### Chẩn đoán

Đã kiểm tra file scene trên đĩa + `Editor.log`:

| Kiểm tra | Kết quả |
|---|---|
| Project trong `Editor.log` | `C:\Users\Hoang PC\Documents\wool-sort\game` ✓ |
| Scene Unity mở | `Assets/Scenes/Home.unity` ✓ (import lúc 10:43, sau khi sửa file) |
| Lỗi compile trong log | 0 |
| Component `ShopUI` trong file | có, `m_Enabled: 1` |
| Component `ShopUI` có trong `m_Component` của GameObject | có ✓ |
| Component nào trong file mà không GameObject nào trỏ tới | 0 |

→ Component **luôn** nằm trên GameObject `ShopPage` (fileID `100000038`) — dễ bị bỏ qua vì tên GameObject không phải `Shop UI`. Ngoài ra Unity hiển thị header của class `ShopUI` là **"Shop Ui"** (tự tách CamelCase), không phải "Shop UI".

### Phát hiện thêm: Prefab mismatch

`Editor.log` lặp lại nhiều lần:

```
Prefab mismatch: The instance of type Transform in the scene is referencing
a corresponding source object of type RectTransform in the Prefab.
Fixing by replacing Transform with RectTransform. Some references might be lost.
```

Nguyên nhân: 3 prefab instance (`MenuBar`, `CoinHUD`, `PurchaseSuccessPopup`) được ghi bằng stripped `--- !u!4 &… Transform:` trong khi **root của prefab là `RectTransform`** (class 224). Unity phải tự sửa mỗi lần load và có thể mất reference.

### Đã làm

- Sửa generator: 3 stripped transform đổi `!u!4 Transform` → `!u!224 RectTransform` (khớp root prefab) → hết cảnh báo.
- Tách component `ShopUI` ra GameObject riêng tên **`ShopUI`** (child của `ShopPage`):
  ```
  ShopPage
  ├── ShopTopBar / ItemContainer / TxtNotify
  ├── ShopUI        ← component ShopUI nằm ở đây
  └── CoinHUD
  ```
  `HomeUI.shopUI` trỏ tới component mới.

### Verify

| Kiểm tra | Kết quả |
|---|---|
| Compile 3 assembly | PASS — 0 error |
| Validator `Home.unity` + `EntryScene.unity` | PASS cả 2 |
| Audit asset toàn repo | 0 thiếu `.meta`, 0 `.meta` mồ côi, 0 GUID trùng |
| Component mồ côi (không GameObject nào trỏ tới) | 0 |
| Stripped transform trong scene | cả 3 đều `cls=224 RectTransform` ✓ |
| Hierarchy `Home.unity` | `ShopPage → ShopUI` ✓ |

## Vòng 15 — Bảng inventory ảnh để reskin (2026-10-21)

Yêu cầu: thiết kế bảng liệt kê asset **đang dùng** (path) và asset **không dùng** — chỉ ảnh, để reskin.

### Công cụ

`tools/inventory_images.py` (chạy lại bằng `python tools/inventory_images.py`):

1. Index mọi asset trong `game/Assets` → GUID (đọc `.meta`).
2. Dựng đồ thị reference: quét `.unity / .prefab / .asset / .mat / .controller / .anim / …` + `.cs / .shader` tìm `guid:`.
3. Gốc (root) = **mọi scene** + **mọi asset trong `Resources/`** (load runtime theo path) + **mọi file code**.
4. BFS từ các gốc → ảnh **reachable = đang dùng**, còn lại = **không dùng**.
5. Bổ sung metadata từ `.meta`: kích thước (PNG + JPEG), PPU, `textureType`, `spriteBorder` (9-slice), mipmap.

### Kết quả

| Nhóm | Tổng | Đang dùng | Không dùng |
|---|---|---|---|
| Ảnh game (`_Game/`, TMP sprites) | 94 | 87 | 7 |
| Ảnh plugin/editor (DOTween, DemiLib, Dreamteck) | 191 | 4 | 187 |

**Ảnh game KHÔNG dùng (7):**

| Path | Kích thước | Lý do |
|---|---|---|
| `_Game/BoosterUI_Picked/Button_Gray.png` | 356x168 | Không asset nào trỏ tới |
| `_Game/Textures/Home/background.png` | 1082x2160 | Không asset nào trỏ tới (BG đã đổi sang `Frame_Bar_1.png`) |
| `_Game/Textures/UIV2/Frame_Popup_3.png` | 278x68 | Không asset nào trỏ tới (popup nhận thưởng dùng `Frame_Popup_4`) |
| `_Game/Textures/text_diffuse.jpg` | 512x512 | Chỉ bởi `WoodMat_*.mat` — 3 material đó cũng không ai dùng |
| `_Game/Textures/text_normal.png` | 256x256 | Như trên |
| `_Game/Textures/text_roughness_1.jpg` | 512x512 | Như trên |
| `_Game/Textures/text_roughness_2.jpg` | 512x512 | Như trên |

→ Cả bộ texture gỗ + 3 material `WoodMat_Gray/White/Yellow.mat` đều chết (đã verify không asset nào trỏ tới 3 material đó).

Bảng đầy đủ: `docs/asset-inventory-images.md`.

### Ghi chú reskin (đã ghi trong file bảng)

- PPU = 100 cho toàn bộ ảnh UI → đổi PNG mà giữ PPU sẽ làm UI phình/nhỏ.
- Ảnh 9-slice phải giữ nguyên `spriteBorder` khi đổi PNG.
- Sửa ảnh ở `_Game/BoosterUI_Picked/` thì phải **copy đè** sang `_Game/Resources/BoosterUI/` (bản runtime fallback).

## Vòng 16 — Tool reskin hàng loạt (2026-10-21)

Yêu cầu: "có cách nào reskin hàng loạt ko nhỉ" → chọn phương án **tool ghi đè PNG bằng Python**.

### Khảo sát trước khi làm

| Texture | Dùng ở | Kết luận |
|---|---|---|
| `Textures/Pattern/Pattern_basecolor.png` | 21 material `ThreadMat_*` | đổi 1 file = đổi cả bộ len |
| `Textures/Pattern/Pattern_normal.png` | 21 material | |
| `Textures/Fabric_Pattern/Fabric_Pattern_basecolor.png` | 21 material `StringMat_*` | đổi 1 file = đổi cả bộ sợi |
| `Textures/Fabric_Pattern/Fabric_Pattern_normal.png` | 21 material | |
| `Textures/MasterColor.png` | 3 material spool (`_Texture1`) | đổi 1 file = đổi màu cuộn |
| `Textures/RainbowTexture.png` | `_TextureSample0` | màu cầu vồng |

→ Đã có sẵn "tầng reskin 1 file" không cần code.

**Phát hiện thêm — texture đã bị xoá nhưng material vẫn trỏ tới:**

| Property | Số material | Shader sample thật? |
|---|---|---|
| `_MainTex` | 54 | không (rác) |
| `_BumpMap` | 54 | không (rác) |
| `_Height` | 66 | không (rác) |
| `_noise` | 65 | không (rác) |
| `_DissolveNoise` | 23 | **có** → thiếu texture thật |
| `_TextureSample0` | 18 | **có** → thiếu texture thật |

`YarnDissolve.shader` và `ToonyColors_ThreadScrew.shader` thực sự sample `_Texture`,
`_NormalMap`, `_AO`, `_RoughnessMap`, `_DissolveNoise`; `ToonyColors_ThreadScrew_Basket.shader`
và `ToonyColors_SpoolBase.shader` chỉ sample `_TextureSample0`. → một số material đang
thiếu texture thật, cần bổ sung trước khi reskin.

### Tool: `tools/reskin.py`

Ghi đè **nội dung** file PNG, không xoá rồi tạo lại → `.meta` giữ nguyên → GUID không đổi →
prefab/scene/material vẫn trỏ đúng, `spriteBorder` (9-slice) và PPU không mất.

- `--src <folder>` — folder ảnh mới; `--apply` mới thực thi (mặc định chỉ xem kế hoạch).
- Khớp file theo thứ tự: **đường dẫn tương đối** → **CSV map** (`--map`) → **tên file**.
- Trùng tên nhiều nơi → ưu tiên bản **không nằm trong `Resources/`** (bản editable) và in ra
  các bản trùng còn lại; `--all-matches` để thay hết.
- Cảnh báo **9-slice** khi kích thước PNG mới khác cũ (border tính theo pixel cũ).
- `--mirror-booster` copy thêm sang `_Game/Resources/BoosterUI/` (bản runtime fallback).
- Backup bản gốc vào `.reskin_backup/<timestamp>/` (đã thêm vào `.gitignore`), tắt bằng `--no-backup`.

### Verify (chạy trên bản sao tạm, không đụng project thật)

| Kiểm tra | Kết quả |
|---|---|
| Dry-run: khớp chính xác theo đường dẫn | `_Game/Textures/UIV2/Frame_Tooltips.png` ✓ |
| Dry-run: trùng tên → chọn bản editable | `Button_Green.png` → `_Game/BoosterUI_Picked/` ✓ (in ra 2 bản trùng còn lại) |
| Dry-run: cảnh báo 9-slice đổi kích thước | `Button_Green` 356x168 → 400x200 ✓ |
| Dry-run: ảnh không tồn tại | `Icon_Moi_Minh.png` → báo "không tìm thấy" ✓ |
| `--apply`: `.meta` **không đổi** (md5) | cả 2 file ✓ |
| `--apply`: nội dung PNG mới | ✓ |
| `--apply --mirror-booster`: copy sang `Resources/BoosterUI/` | ✓ |
| `--apply`: backup bản gốc | ✓ (đã xoá thư mục test) |

Tài liệu: `docs/reskin.md`.

## Vòng 17 — Reskin: ô co theo ảnh, scroll nhanh, chỉ quét asset dùng trong game (2026-10-21)

### 1. Ô preview co theo ảnh + chiều cao dòng

- `DrawThumb`: `k = min(1, min(boxW/w, boxH/h))` → ảnh **nhỏ giữ nguyên kích thước**
  (không phóng to), ảnh **lớn thu vừa ô**, tỉ lệ không đổi.
- `RowHeight = clamp(max(cao 2 ảnh), 64, ThumbSize) + 40` → ảnh nhỏ thì dòng ngắn.
- Ảnh cũ dùng **texture gốc** (`LoadAssetAtPath`) thay vì `AssetPreview` (cắt viền /
  theo sprite rect nên không thấy trọn ảnh).

### 2. Scroll nhanh

- Bảng vẽ bằng **rect thủ công** (`EditorGUI.*`) thay vì `EditorGUILayout` từng ô →
  kiểm soát chính xác vị trí, hết chồng nhau.
- **Chỉ vẽ dòng trong vùng nhìn thấy**; dòng ngoài vùng chỉ để lại chỗ bằng
  `GUILayout.Space` → không xử lý mục của cả ~300 dòng mỗi frame.
- Cache texture của asset (trước đó gọi `AssetDatabase.LoadAssetAtPath` mỗi dòng
  mỗi frame) + `ClearCaches()` sau khi Apply vì texture bị reimport.

### 3. Chỉ quét asset dùng trong game

- Nút **"Quét ảnh dùng trong game"** → lấy `AssetDatabase.GetDependencies(scene, true)`
  của 3 scene gốc `EntryScene` / `Home` / `Gameplay`, giao nhau → chỉ ảnh thật sự
  dùng trong game.
- Scene gốc sửa được ở ô *"Scene gốc"* (phân tách `;` hoặc `,`).
- Tuỳ chọn *"Kèm ảnh trong Resources/"* cho ảnh chỉ load runtime theo path.

**Số lượng (verify bằng script độc lập):**

| Phạm vi | Số ảnh |
|---|---|
| Toàn project | 285 |
| **Reachable từ Entry + Home + Gameplay** | **70** |
| Bị loại | 215 (ảnh plugin DOTween/DemiLib/Dreamteck, ảnh editor, ảnh không ai dùng, bản mirror `Resources/BoosterUI/`) |

Bản mirror `Resources/BoosterUI/` không được quét (không ai trỏ tới) — đúng mục đích,
vì tuỳ chọn *"Mirror BoosterUI"* sẽ copy đè sang đó khi áp dụng.

### Verify

| Kiểm tra | Kết quả |
|---|---|
| Compile 3 assembly | PASS — 0 error, 0 warning từ file mới |
| Audit asset toàn repo | 0 thiếu `.meta`, 0 `.meta` mồ côi, 0 GUID trùng |
| Số ảnh reachable từ 3 scene (script độc lập) | 70 |

## Vòng 18 — Fix nhầm logic giữa các booster (2026-10-21)

Báo cáo: "bug xảy ra khi sử dụng booster, nó bị nhầm logic giữa các booster".

### Nguyên nhân gốc: enum `BoosterType` lệch 1 bậc

| | Dự án gốc | Target (trước khi sửa) |
|---|---|---|
| AddSlot | 0 | 1 |
| Clean | 1 | 2 |
| MagicBox | 2 | 3 |
| None | — | 0 |

Target thêm `None = 0` ở đầu → **mọi thứ index theo `(int)BoosterType` bị lệch 1**, trong khi scene
`Gameplay.unity`, mảng `Titles`/`Descs`/`contentIcons` của `MoreBoosterPopupUI` và
`Configs.LIST_BOOSTER_COST`/`LIST_BOOSTER_UNLOCK` đều theo thứ tự gốc.

**Hậu quả cụ thể:**

| Nơi | Trước | Sau |
|---|---|---|
| `BoosterBtn_1` (type 0) | `None` → không vào nhánh nào | **AddSlot** ✓ |
| `BoosterBtn_2` (type 1) | chạy **AddSlot** | **Clean** ✓ |
| `BoosterBtn_3` (type 2) | chạy **Clean** | **MagicBox** ✓ |
| `MoreBoosterPopupUI.RefreshUI()` | `Titles[(int)type]` → lệch 1 ô | đúng ô |
| `MoreBoosterPopupUI` với MagicBox | `Titles[3]` → **IndexOutOfRange** | không lỗi |
| `Configs.LIST_BOOSTER_COST[(int)type]` | AddSlot ra giá của Clean; MagicBox → **IndexOutOfRange** | đúng giá |
| `BoosterItem` unlock level | đọc sai ô | đúng ô |
| `BoosterBarUI.CreateButton` | tên nút `BoosterBtn_2` cho AddSlot | `BoosterBtn_1` ✓ khớp scene |

### Đã sửa

- **`BoosterType.cs`**: `AddSlot = 0, Clean = 1, MagicBox = 2, None = -1`.
  `None` là sentinel, đặt `-1` để lỡ dùng nhầm cũng không trỏ vào ô nào trong list.
- **`Configs.cs`**: thêm `GetBoosterCost(BoosterType)` + `GetBoosterUnlockLevel(BoosterType)` có
  kiểm tra biên → không còn index thẳng vào list ở bất kỳ đâu.
- **`BoosterItem.cs`** (2 chỗ) → dùng `Configs.GetBoosterUnlockLevel(type)`.
- **`MoreBoosterPopupUI.cs`**: `RefreshUI()` clamp index; 2 chỗ giá → `Configs.GetBoosterCost(...)`.
- **`ShopItem`**: thêm cờ `IsBooster` (vì mặc định enum giờ là `AddSlot`, gói coin phải khai
  báo tường minh `BoosterType.None`); `ShopService` khai báo `None` cho 2 gói coin;
  `ShopService.Buy` + `ShopItemView` + `PurchaseSuccessPopup` dùng `IsBooster`.
- `PlayerData` / `GameData` lưu booster theo **tên** (switch / Dictionary) → không ảnh hưởng.

### Test

- `BoosterLogicTests.BoosterConfig_HasThreeEntries`: bỏ assert "enum có đúng 3 tên" (giờ có
  thêm `None`) → thay bằng 3 test regression:
  - `BoosterType_ValuesMatchConfigListOrder` — AddSlot=0, Clean=1, MagicBox=2, None<0
  - `Configs_GetBoosterCost_MapsCorrectType` — 80 / 120 / 60
  - `Configs_GetBoosterUnlockLevel_MapsCorrectType` — 1 / 1 / 1

### Verify (script độc lập đọc source + scene)

| Kiểm tra | Kết quả |
|---|---|
| enum vs scene `BoosterBtn_1/2/3` | 0→AddSlot, 1→Clean, 2→MagicBox ✓ |
| enum vs `Titles`/`Descs`/`contentIcons` | ADD SLOT / CLEAN / MAGIC SPOOL ✓ |
| enum vs `LIST_BOOSTER_COST` | 80 / 120 / 60 ✓ |
| Compile 3 assembly | PASS — 0 error |
| Audit asset toàn repo | 0 thiếu `.meta`, 0 `.meta` mồ côi, 0 GUID trùng |

## Vòng 19 — Fix báo thua trước khi sợi len kịp vào hộp (2026-10-21)

Báo cáo: *"khi trên thanh ngang có màu, tôi pick đúng sợi len đó nhưng tốc độ nhanh nó
chưa kịp load xong đã bắn thông báo thua rồi"*.

### Nguyên nhân (race condition)

`Slot.Fill()` đánh dấu ô chờ **đầy ngay** (`IsFilled = true`) và bắn `SlotFilledEvent`
ngay, nhưng chuyển sợi len sang hộp lại chạy **sau khi animation ring-appear xong**
(`WaitAndTriggerBoxBounce` → `OnAnimationFilled` → `TryRefillSlot`).

→ `GameManager.React(SlotFilledEvent)` thấy "vùng chờ đầy" + "chưa có hộp nào đang xử lý"
→ `LoseLevel()` **trước khi sợi kịp vào hộp**. Chơi càng nhanh càng dễ trúng.

Logic này **có sẵn ở dự án gốc** (đã diff `React(SlotFilledEvent)` và
`IsAllWaitingSlotsFilled()` giữa 2 bên → giống hệt), không phải lỗi do port.

### Đã sửa

- **`GameManager.React`**: ô chờ vừa nhận sợi còn đang animation → **không kết luận thua**,
  đặt `isCheckingLose = true` và trì hoãn.
- **`GameManager.RecheckLoseAfterWaitingAnimation(Slot)`** (mới): check lại thua *sau khi*
  animation xong và đã thử đẩy sợi vào hộp — vùng chờ còn trống → không thua; đang có hộp
  xử lý → đợi; vẫn đầy + không hộp nào nhận được → thua.
- **`WaitingAreaManager.TryRefillSlot`**: nhánh không có hộp nào nhận được sợi → gọi check
  lại (trước đây im lặng → thua bị bỏ sót).
- **`WaitingAreaManager.AutoRefillCoroutine`**: nhánh refill thất bại → cũng check lại.

Đặt check lại ở `WaitingAreaManager` chứ không phải `OnAnimationFilled` vì thứ tự gọi là
`OnAnimationFilled` **trước** `TryRefillSlot` — check ở đó vẫn quá sớm.

Chi tiết + checklist test tay: `docs/evidence/lose-race-condition.md`.

### Verify

| Kiểm tra | Kết quả |
|---|---|
| Diff luồng thua với bản gốc | giống hệt → xác nhận lỗi có sẵn ở gốc |
| Compile 3 assembly | PASS — 0 error (warning đều có sẵn từ file cũ) |
| Audit asset toàn repo | 0 thiếu `.meta`, 0 `.meta` mồ côi, 0 GUID trùng |

## Vòng 20 — Fix popup nhận thưởng không hiện khi mua (2026-10-21)

Báo cáo: *"hiện tại khi mua thì ko show popup nhận thưởng"*.

### Chẩn đoán

Instance `PurchaseSuccessPopup` trong `Home.unity` có:

```yaml
propertyPath: m_IsActive
value: 0
```

→ **GameObject gốc của popup đang TẮT** → `Awake()` không chạy → `Instance` vẫn `null`
→ `ShopItemView.OnBuy` gọi `PurchaseSuccessPopup.Instance` → null → **không hiện popup**
(mua vẫn thành công, chỉ thiếu popup).

Vì sao lại tắt? Ở **edit mode** `Awake` không chạy nên `SetVisible(false)` không có tác dụng
→ card hiện lên trong Scene View → dễ bị tắt nhầm để scene gọn. Nhưng để gốc tắt thì
`Instance` không bao giờ được set.

### Đã sửa

- **`PurchaseSuccessPopup.Acquire()`** (mới, static): tìm popup kể cả object **inactive**
  (`FindFirstObjectByType<T>(FindObjectsInactive.Include)`) và **bật lên** nếu đang tắt
  (bật thì `Awake` chạy → set `Instance` + tự ẩn card).
- **`PurchaseSuccessPopup.Show()`**: tự bật `gameObject` nếu đang tắt.
- **`ShopItemView.OnBuy`**: dùng `Acquire()` thay vì `Instance`, và log cảnh báo nếu
  không tìm thấy popup.
- **Test `ShopPurchaseTests.Buy_ShowsRewardPopup`**: tìm popup kèm `FindObjectsInactive.Include`
  và bật lên trước khi assert (trước đó dùng `FindFirstObjectByType` không có Include →
  không thấy popup đang tắt).

Giữ nguyên thiết kế "gốc để tắt trong scene" (Scene View gọn, không chặn input) — giờ
code tự bật được nên không còn phụ thuộc trạng thái active trong scene.

### Verify

| Kiểm tra | Kết quả |
|---|---|
| Prefab `PurchaseSuccessPopup.prefab` — field `icon/txtTitle/txtName/txtAmount/btnOk/btnClose/shade/card/cardRect` | đều còn trỏ đúng ✓ |
| Layout prefab (Card 640×? canh giữa, scale 1) | OK ✓ |
| Compile 3 assembly | PASS — 0 error |
| Audit asset toàn repo | 0 thiếu `.meta`, 0 `.meta` mồ côi, 0 GUID trùng |

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
