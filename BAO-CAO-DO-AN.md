# BÁO CÁO ĐỒ ÁN TỐT NGHIỆP

## Thiết kế và phát triển trò chơi giải đố tương tác trên nền tảng di động

| Thông tin | Nội dung |
|---|---|
| Sinh viên thực hiện | Nguyễn Thế Hoàng |
| Mã sinh viên | V201602127 |
| Lớp | CNTT1 — K61 |
| Ngành | Công nghệ thông tin — Hệ chính quy |
| Giảng viên hướng dẫn | TS. Nguyễn Hiếu Cường |
| Đơn vị | Trường Đại học Giao thông Vận tải Hà Nội |
| Tên đề tài | Thiết kế và phát triển trò chơi giải đố tương tác trên nền tảng di động |
| Sản phẩm | **Wool Sort Master** (Unity, C#, Android) |
| Thời gian thực hiện | 9 tuần — 45 ngày làm việc |

---

## Tóm tắt

Đồ án xây dựng một trò chơi giải đố một người chơi trên di động: người chơi chạm vào các lớp len ngoài cùng của một khối len nhiều lớp, đưa len vào hộp cùng màu hoặc vùng chờ, mở dần các lớp bên trong và hoàn thành bức ảnh của màn chơi. Sản phẩm được phát triển bằng Unity 6 (C#), tái sử dụng có chọn lọc tài nguyên và hành vi của một dự án tham chiếu, đồng thời viết lại lớp dịch vụ (economy, save, shop, daily reward, settings, audio) và toàn bộ quy trình kiểm chứng.

Điểm đặc biệt của đồ án là **quy trình phát triển có AI tham gia ở mức kỹ thuật**: yêu cầu được chuyển thành hợp đồng (contract) có thể kiểm chứng, mỗi task có acceptance criteria và bằng chứng, AI đọc code thật để chẩn đoán và triển khai theo từng vertical slice. Báo cáo mô tả đầy đủ: quy trình 9 tuần, cách viết backlog/worklog, cách tích hợp AI, kiến trúc và tính năng đã làm, bộ công cụ Editor, quy trình kiểm thử, cùng hướng dẫn tái lập từ đầu để người đọc có thể làm lại.

> **Lưu ý về tài liệu:** Toàn bộ số liệu trong báo cáo phản ánh trạng thái repo tại thời điểm viết. Một số file trong `docs/features/` (đặc biệt `00-project-foundation.md`, `F01-core-puzzle.md`) là *thiết kế mong muốn*, không phải mô tả code đang chạy; báo cáo này ưu tiên **trạng thái triển khai thật** và ghi rõ phần nào là kế hoạch.

---

## Mục lục

1. [Chương 1 — Giới thiệu](#chương-1--giới-thiệu)
2. [Chương 2 — Phân tích và thiết kế game](#chương-2--phân-tích-và-thiết-kế-game)
3. [Chương 3 — Quy trình phát triển dự án](#chương-3--quy-trình-phát-triển-dự-án)
4. [Chương 4 — Tích hợp AI vào quy trình phát triển](#chương-4--tích-hợp-ai-vào-quy-trình-phát-triển)
5. [Chương 5 — Kiến trúc và triển khai](#chương-5--kiến-trúc-và-triển-khai)
6. [Chương 6 — Các tính năng đã triển khai](#chương-6--các-tính-năng-đã-triển-khai)
7. [Chương 7 — Editor tool và pipeline nội dung](#chương-7--editor-tool-và-pipeline-nội-dung)
8. [Chương 8 — Kiểm thử và đánh giá](#chương-8--kiểm-thử-và-đánh-giá)
9. [Chương 9 — Hướng dẫn tái lập từ đầu](#chương-9--hướng-dẫn-tái-lập-từ-đầu)
10. [Chương 10 — Kết luận và hướng phát triển](#chương-10--kết-luận-và-hướng-phát-triển)
11. [Phụ lục](#phụ-lục)

---

# Chương 1 — Giới thiệu

## 1.1. Bối cảnh và lý do chọn đề tài

Trò chơi giải đố (puzzle) trên di động có ưu điểm: thời lượng chơi ngắn, thao tác đơn giản, dễ tiếp cận nhiều nhóm người dùng. Dạng puzzle "sắp xếp theo thuộc tính" (sort/match) đang phổ biến vì luật chơi dễ hiểu nhưng tạo được chiều sâu nhờ dữ liệu màn chơi, thứ tự ưu tiên và tài nguyên hỗ trợ.

Đồ án chọn dựng lại một sản phẩm thuộc thể loại này (**Wool Sort Master**) với mục tiêu:

- Có vòng lặp chơi hoàn chỉnh: khởi động → trang chủ → màn chơi → thắng/thua → tiến trình.
- Có hệ thống tài nguyên, vật phẩm hỗ trợ, cửa hàng, cài đặt, phần thưởng hằng ngày.
- Có quy trình nội dung (level, ảnh, độ khó) và công cụ Editor để mở rộng.
- Có kiểm thử và bằng chứng kiểm chứng cho từng luồng.

## 1.2. Mục tiêu của đồ án

1. **Mục tiêu gameplay:** người chơi chạm len → len bay vào hộp cùng màu/vùng chờ → hộp đầy thì đóng → mở lớp bên trong → tô dần bức ảnh → thắng khi hết len, thua khi vùng chờ đầy.
2. **Mục tiêu hệ thống:** lưu tiến trình, coin/booster, shop nội bộ, daily reward 7 ngày, cài đặt âm thanh/rung.
3. **Mục tiêu kỹ thuật:** cấu trúc scene–prefab–configuration rõ ràng, tách service khỏi gameplay, dữ liệu màn chơi có một nguồn chuẩn (`LevelCatalog`), có validator chặn build khi dữ liệu sai.
4. **Mục tiêu quy trình:** mỗi task có acceptance/verify, mỗi phiên có worklog, có bộ công cụ Editor để giảm thao tác tay.
5. **Mục tiêu học thuật:** chứng minh khả năng áp dụng AI có kiểm soát vào phát triển phần mềm, với bằng chứng và giới hạn rõ ràng.

## 1.3. Phạm vi

### Trong phạm vi

- Game một người chơi, Android, Unity 6.
- 3 scene: `EntryScene`, `Home`, `Gameplay`.
- 7 màn chơi playable (`Resources/Levels/Level1..7.prefab`).
- Core puzzle: box/slot/waiting, len nhiều lớp, hiệu ứng dissolve/grow, rope.
- Booster: AddSlot, Clean, MagicBox.
- Kinh tế nội bộ: coin, booster inventory, shop mua bằng coin, daily reward 7 ngày.
- UI: Home 3 tab, HUD coin, Settings, Win/Lose, Purchase Success, Daily Reward.
- Lưu trữ: `PlayerPrefs` JSON qua `SaveService`.
- Công cụ Editor: Level Studio, Level Catalog, Reskin, Yarn tools, Audio/Daily setup.
- Kiểm thử: EditMode/PlayMode tests + checklist tay.

### Ngoài phạm vi (quyết định có chủ đích)

- Không backend, không tài khoản, không multiplayer, không ranking online.
- Không IAP thật, không quảng cáo, không SDK analytics/remote config.
- Không Addressables, không DI framework, không event bus mới.
- Không localization runtime (chuỗi hardcode; tiếng Việt/Anh theo màn hình).
- Không revive UI, tutorial runtime, heart gate (dữ liệu có nhưng chưa nối UI).

> Các mục ngoài phạm vi này được ghi thành task riêng trong `tasks/todo.md` để tránh "scope creep".

## 1.4. Đối tượng sử dụng

- Người dùng Android thích puzzle ngắn, thao tác chạm.
- Người chơi mới cần luật đơn giản, phản hồi trực quan.
- Người chơi quay lại cần tiến trình, phần thưởng ngày, booster.

## 1.5. Công nghệ và môi trường

| Thành phần | Lựa chọn |
|---|---|
| Engine | Unity **6000.3.21f1** (`game/ProjectSettings/ProjectVersion.txt`) |
| Ngôn ngữ | C# |
| UI | Unity UGUI + TextMeshPro |
| Animation/Tween | DOTween, Animator |
| Rope | Dreamteck Splines |
| Input | Lean Touch / Lean Common |
| UI framework nội bộ | `_KIT` (KitSound, KitTooltipManager, KitSafeArea, KitScaleCanvas) |
| Event bus | `www.nulltale.eventsystem` (UnityEventBus) |
| Test | Unity Test Framework 1.6.0 |
| Quản lý phiên bản | Git (GitHub: `nguyenthehoang98/wool-sort`) |
| Nền tảng build | Android |
| Dự án tham chiếu | `D:\BB_Project\wool-sort` — Unity 2022.3.50f1 (chỉ đọc/đối chiếu) |

### Package chính (`game/Packages/manifest.json`)

```json
{
  "com.unity.ugui": "2.0.0",
  "com.unity.feature.2d": "2.0.2",
  "com.unity.feature.mobile": "1.0.0",
  "com.unity.formats.fbx": "5.1.6",
  "com.unity.timeline": "1.8.12",
  "com.unity.visualscripting": "1.9.12",
  "com.unity.test-framework": "1.6.0",
  "com.coffee.softmask-for-ugui": "git",
  "com.coffee.ui-effect": "git",
  "com.coffee.ui-particle": "4.9.0",
  "www.nulltale.eventsystem": "git"
}
```

## 1.6. Kết quả chính

- Bản chơi được từ Entry → Home → Gameplay, thắng/thua, sang màn tiếp theo.
- 7 màn chơi có dữ liệu; catalog + validator chặn build nếu dữ liệu sai.
- 3 booster hoạt động end-to-end; shop coin nội bộ; daily reward 7 ngày; settings; audio.
- Bộ Editor tool giúp tạo/kiểm tra/reskin màn chơi và ảnh.
- Bộ tài liệu 4 tầng (game design, technical docs, AI contracts, feature/evidence).
- Quy trình phát triển có AI với hợp đồng, evidence và kỷ luật worklog.

---

# Chương 2 — Phân tích và thiết kế game

## 2.1. Thể loại và trải nghiệm cốt lõi

- **Thể loại:** puzzle sắp xếp (sort/match) 3D, một người chơi, màn ngắn.
- **Core fantasy:** "gỡ từng lớp len và xếp đúng chỗ để hoàn thiện bức tranh".
- **Trụ cột thiết kế:**
  1. Dễ hiểu — một thao tác chạm.
  2. Phản hồi rõ — len bay, rơi, tan, hộp đóng, ảnh hiện dần.
  3. Tiến trình — màn tăng độ khó, mở booster/tài nguyên.
  4. Phần thưởng — coin, booster, daily reward.

## 2.2. Vòng lặp chơi (core loop)

```text
Entry (load save/config)
   ↓
Home (tab Home/Shop/ComingSoon, PLAY, Daily Reward)
   ↓
Gameplay: load Level{id}
   ↓
Lặp trong màn:
  chạm len ngoài cùng
    → tìm hộp cùng màu (ưu tiên hộp đầy nhất) hoặc slot chờ trái nhất
    → len bay (rope) → slot nhận → animation
    → nếu hộp đủ 3 → đóng hộp → tô ảnh (JamImage)
    → lớp trong được spawn/grow
   ↓
Thắng (hết len) / Thua (vùng chờ đầy)
   ↓
WinPopup / LosePopup → next / retry / home
   ↓
Cập nhật save (level, coin, booster)
```

## 2.3. Luật chơi và mô hình trạng thái

### 2.3.1. Thực thể

| Thực thể | Vai trò | Trạng thái |
|---|---|---|
| `YarnPart` | Một lớp len (visual + logic) | Hidden → OnBoard → Selected → InSlot → Dissolving → Consumed |
| `YarnPartInfo` | Node dữ liệu thuần của lớp len | OnBoard / InQueue / InBox |
| `Box` | Hộp chứa 3 slot cùng màu | Locked → Active → Full → Closing → Closed |
| `Slot` | Một ô trong hộp hoặc vùng chờ | Empty → Filling → Filled → Clearing → Empty |
| `WaitingArea` | Vùng chờ 5–7 ô | — |
| `JamImage` | Bức ảnh 84×84 tô dần | — |

### 2.3.2. Quy tắc đặt len

Thứ tự kiểm tra khi chạm một len:

1. Màn đã kết thúc? → bỏ qua.
2. Input đang bị khoá (animation/intro/popup)? → bỏ qua.
3. Len có hợp lệ và đang là lớp ngoài cùng? → nếu không, bỏ qua.
4. Tìm hộp cùng màu: chọn hộp có **nhiều slot đã đầy nhất**; hoà thì chọn `boxOrder` nhỏ hơn.
5. Không có hộp phù hợp → chọn slot chờ **trái nhất**.
6. Không còn slot → từ chối, **không thay đổi state** (no-op).

Quy tắc này được đặc tả trong `docs/week1-core-contract.md` (CORE-01..16) và là nguồn cho test matrix.

### 2.3.3. Thắng / thua

- **Thua:** khi một slot **vùng chờ** vừa đầy và tất cả slot chờ đều đầy, không còn hộp nào đang xử lý.
  - Có race condition: slot được đánh dấu "filled" ngay khi len bay tới, nhưng việc chuyển len sang hộp chỉ xảy ra sau animation. Vì vậy phải **hoãn kiểm tra thua** nếu slot còn animation (`isCheckingLose`) và kiểm tra lại trong `WaitingAreaManager.TryRefillSlot` / `AutoRefillCoroutine`.
- **Thắng:** khi hộp đóng và `countYarnRemain <= 0`, hoặc khi tất cả hộp đã đóng (`HandleAllBoxesClosed`).
- Kết quả cuối chỉ được phát **một lần**.

### 2.3.4. Input gate

`GameManager.CanInteractWithGame()` trả về `false` khi:

- `isGameOver`
- `isCheckingLose`
- `isAnimatingStartLevel`
- `MoreBoosterPopupUI.IsBlockingGameplayInput`

Ngoài ra `TryBeginYarnTap()` chống double-tap trong cùng frame.

### 2.3.5. Tính tất định (determinism)

- Catalog gán `seed = DefaultSeed(levelId)` (`levelId * 7919 + 104729`).
- Cùng state + seed + chuỗi command phải sinh cùng event.
- Đây là mục tiêu của contract; phần resolver thuần chưa được tách hoàn toàn trong bản triển khai hiện tại (xem Chương 8).

## 2.4. Thiết kế màn chơi và độ khó

- Mỗi màn là một prefab 3D gồm model nhiều lớp len, `YarnPart` cho từng mesh, `LevelYarnInfoHolder` giữ dữ liệu màu/lớp.
- Màu len: 21 màu + `None` + `Rainbow`.
- Độ khó tính theo công thức trong `LevelDifficultyTool`:

```text
score = colors × 8 + yarns × 0.25 + (depth − 1) × 6 + random% × 12
rating: ≤ 70 Dễ · ≤ 110 Trung bình · ≤ 160 Khó · > 160 Rất khó
```

- Ràng buộc dữ liệu: số len mỗi màu phải chia hết cho 3 (mỗi hộp 3 slot) và tổng màu phải cân với pool random.
- Bức ảnh màn chơi là lưới 84×84 = 7.056 ô, mỗi ô thuộc một màu; khi hộp đóng, các ô của màu đó được "in" dần.

## 2.5. Tiến trình và kinh tế

| Tài nguyên | Dùng cho | Ghi chú |
|---|---|---|
| Coin | Mua booster, gói coin | Nguồn: thắng màn, daily reward, shop |
| Booster inventory | AddSlot / Clean / MagicBox | Mua bằng coin |
| Level | Tiến trình chơi | `currentLevel`, `lastPlayedLevel` |
| Heart | Dự phòng (chưa dùng) | Có refill theo thời gian nhưng chưa có gate |

Nguyên tắc: **mọi thay đổi tài nguyên đi qua `EconomyService`** (một ledger), mỗi thay đổi gọi `Save()` và phát event để UI cập nhật.

## 2.6. Booster

| Booster | Tác dụng | Điều kiện | Giá |
|---|---|---|---|
| AddSlot | Thêm 1 slot chờ (tối đa 7) | Chưa đạt max | 80 coin |
| Clean | Dọn len đang chờ vào kho tạm, hồi lại khi có hộp mới | Vùng chờ có len | 120 coin |
| MagicBox | Đổi hộp hiện tại thành Rainbow và kéo len thiếu màu vào | Luôn dùng được | 60 coin |

Chi tiết triển khai ở Chương 6.

## 2.7. UI/UX

```text
EntryScene
  Canvas (1080×2160) → BG, SafeArea → Text (TMP), Splash/loading
  AudioBootstrap, EventSystem, TooltipManager

Home
  Canvas → BG, SafeArea
    HomePage      : TxtLevel, PLAY, LevelList (4 ô), DailyRewardButton
    ShopPage      : TopBar, ItemContainer, TxtNotify, CoinHUD
    ComingSoonPage
    MenuBar       : Home / Shop / Ranking
    SettingsButton + SettingsPopup
  Canvas_2 (Overlays): DailyRewardPopup, PurchaseSuccessPopup
  SplashItem, TooltipManager

Gameplay
  Canvas_1 (Base), Canvas_2 (Overlays)
  Board + Cameras, BGCamera, RopeCamera
  SafeArea: BoosterBar (3 nút), ScaleSlider, TxtLevel, CoinHUD, SettingsButton
  WinPopup, LosePopup, MoreBoosterPopup
  Managers: GameManager, BoxManager, SlotManager, WaitingAreaManager,
            BoosterManager, YarnRopeController, YarnColorManager,
            GameUIController, PoolManager, CoinManager, EffectUIManager
```

Nguyên tắc UI:

- CanvasScaler thống nhất **1080×2160** cho cả 3 scene.
- Popup dạng prefab, có `Acquire()` để tìm cả object inactive.
- Các dòng lặp (shop item, daily item) là prefab + view script, không dựng bằng code.

## 2.8. Âm thanh và cài đặt

- `AudioConfig.asset` chứa 2 BGM + 21 SFX, mỗi clip có volume riêng.
- `AudioService` phát theo sự kiện: click, tap, dissolve, hold, rope, box full/close, win/lose, booster, level start, popup, coin.
- `SettingsService` lưu `sound/music/vibration` vào `PlayerData`, áp dụng ngay qua `KitSound` và `Handheld.Vibrate()`.

## 2.9. Phạm vi monetization

Đồ án **cố ý không tích hợp IAP/ads thật**. Shop được giữ lại nhưng rút gọn thành mua bằng coin nội bộ:

- 2 gói coin (giá 0 để test) và 3 gói booster.
- Không SDK, không receipt, không analytics.
- Lý do: giảm rủi ro build Android, tập trung vào gameplay và quy trình.

---

# Chương 3 — Quy trình phát triển dự án

## 3.1. Kế hoạch 9 tuần (45 ngày)

Kế hoạch nằm trong `tasks/plan.md`, bám theo đề cương DATN:

| Tuần | Kết quả bàn giao | Checkpoint |
|---|---|---|
| 1 | Chốt scope, luật puzzle, state/command/event, acceptance core | Rule không mơ hồ; có test plan |
| 2 | Baseline Unity, bản đồ scene/prefab/resource/package, level data contract | Mở được project, xác nhận build scenes |
| 3 | Entry → Home → chọn level → Gameplay, progression/save facade | Smoke test chuyển scene + load/save |
| 4 | Core resolver, level mapper, board/box/slot/waiting vertical slice | Level đầu xử lý đúng/sai |
| 5 | Level pipeline, validator, win/lose, deterministic replay | 3 level mẫu pass regression |
| 6 | Booster, revive, tutorial, UI kết quả, FX/audio/settings | Không mất state khi animation/ad fail |
| 7 | Ads/IAP/shop/platform adapters, giao dịch idempotent | Test success/failure/restore |
| 8 | Regression, Android device, profiling, sửa lỗi | Không còn blocker P0/P1 |
| 9 | Build Android, tài liệu, minh chứng, bàn giao | Build + hồ sơ tái hiện được |

**Thực tế điều chỉnh:** tuần 7 (IAP/ads) được bỏ khỏi MVP; tuần 8–9 mới dừng ở mức build/tài liệu. Kế hoạch gốc vẫn được giữ để đối chiếu.

## 3.2. Cấu trúc backlog (`tasks/todo.md`)

Quy ước trạng thái:

| Ký hiệu | Nghĩa |
|---|---|
| `[ ]` | chưa làm |
| `[~]` | đang làm / code xong nhưng chưa verify |
| `[x]` | đã verify (có bằng chứng) |
| `[!]` | blocked — cần quyết định |

Mỗi task có:

- **ID** ổn định (`CLONE-01`, `W3-01b`, `W6-05a`…).
- **Mô tả scope** ngắn.
- **Verify:** điều kiện kiểm chứng cụ thể.
- **Deps:** task phụ thuộc.
- **Còn:** phần còn thiếu (nếu có), kèm task sẽ gộp.

Ví dụ:

```text
[x] W3-01f — Coin HUD kiểu Gameplay + coin thật khi mua
    Verify: compile 3 assembly PASS, validator scene PASS, audit 0 lỗi integrity.
    Còn: tab Home/Coming Soon không hiện coin (nói nếu muốn thêm).
```

Nguyên tắc: **không đánh dấu `[x]` chỉ vì đã copy code** — phải có verification.

## 3.3. Worklog và bằng chứng

`docs/weekly-worklog.md` là nhật ký phát triển, gồm 2 định dạng song song:

1. **Bảng tuần W1D1–W9D5** với cột `Ngày thực tế / Status / Evidence / Ghi chú`.
2. **Các "Vòng N"** — mỗi phiên làm việc là một vòng, gồm:
   - Yêu cầu gốc (quote).
   - Chẩn đoán / hiện trạng.
   - Bảng thay đổi trước → sau.
   - `### Verify` (bảng kết quả).
   - `### Còn thiếu`.

Trạng thái worklog: `TODO / PARTIAL / DONE / BLOCKED`. Quy tắc: `BLOCKED` phải ghi blocker và task tiếp theo; không đổi thành `DONE` để che thiếu test.

Bằng chứng có thể là: commit hash, log compile, ảnh, video, build artifact, test result.

## 3.4. Các giai đoạn thực tế

### Giai đoạn 0 — Khảo sát và clone có kiểm soát

- Xác định reference `D:\BB_Project\wool-sort` (Unity 2022.3.50f1) và target `game/` (Unity 6000.3.21f1).
- Clone `Assets`, `Packages`, `ProjectSettings`; prune 20 thư mục SDK/asset không dùng (`BravestarsSDK`, `GameAnalytics`, `I2`, `Firebase`, `Spine`, `GoogleMobileAds`…).
- Mục tiêu: target mở được, compile được, 3 scene tồn tại.

### Giai đoạn 1 — Tài liệu và hợp đồng

- Viết `gamedesign/` (sản phẩm), `docs/` (kỹ thuật), `ai-docs/` (hợp đồng cho AI).
- Chốt state machine, command/event, acceptance test.
- Tạo `tasks/plan.md` + `tasks/todo.md`.

### Giai đoạn 2 — Port scene và dịch vụ nền

- Port `EntryScene` từ reference, bỏ SDK, viết `EntryBootstrap`/`EntryFlow` native.
- Dựng `Home` mới bằng UI native; port `MenuBar`, Tooltip.
- Viết `EconomyService`, `SaveService`, `PlayerData`.
- Viết shop nội bộ + `ShopItemView`, `PurchaseSuccessPopup`.
- Settings + audio.

### Giai đoạn 3 — Hoàn thiện gameplay/UI

- Sửa race condition lose.
- Sửa booster enum off-by-one.
- Thêm SFX, sửa lỗi AudioConfig `fileID` sai.
- Daily Reward 7 ngày + công cụ generate prefab.
- Reskin pipeline + inventory ảnh.

### Giai đoạn 4 — Ổn định và tài liệu bàn giao

- Validator YAML, audit GUID/meta, compile 3 assembly.
- Worklog từng vòng, cập nhật backlog.
- Chuẩn bị báo cáo/bảo vệ.

## 3.5. Vòng lặp làm việc mỗi phiên (Vòng N)

```text
1. Nhận yêu cầu ngắn (tiếng Việt, có thể kèm lỗi compile).
2. Đọc code + tài liệu liên quan, xác định hiện trạng thật.
3. Chẩn đoán nguyên nhân (không sửa mù).
4. Chọn vertical slice nhỏ nhất.
5. Sửa code/prefab/scene có chọn lọc.
6. Verify: compile, validator, test, audit.
7. Ghi worklog "Vòng N" + cập nhật backlog.
8. Báo lại: đã đổi gì, verify gì, còn gì cần quyết định.
```

## 3.6. Definition of Done

- Task có acceptance criteria + bằng chứng trong worklog.
- Core gameplay chạy được khi ads/IAP/analytics/localization lỗi (dù các phần này đã bị cắt, nguyên tắc vẫn giữ).
- Unit/acceptance test liên quan pass; smoke test scene được ghi lại.
- Không còn lỗi P0/P1 chưa có quyết định.
- Android build dùng Unity 6000.3.21f1; `ProjectVersion.txt` khớp.
- Worklog có ngày, trạng thái, evidence, ghi chú.

## 3.7. Quản lý rủi ro

| Rủi ro | Mức | Cách xử lý |
|---|---|---|
| Target khác Unity version reference | Trung bình | Pin Unity 6; kiểm tra API trước mỗi feature |
| Level prefab/config không đồng bộ | Cao | Validator + build gate |
| Animation/SDK làm kẹt gameplay | Cao | Input gate, timeout, kiểm tra deferred |
| Scope tăng (backend/AB test/Addressables) | Trung bình | Giữ ngoài MVP |
| Thiếu bằng chứng tuần 8–9 | Cao | Lưu log/ảnh/artifact ngay trong ngày |
| AI sửa sai/ghi đè file người dùng đang mở | Cao | Sửa text có chọn lọc, không regenerate scene; verify bằng git/validator |

---

# Chương 4 — Tích hợp AI vào quy trình phát triển

## 4.1. Vai trò của AI

AI (coding agent) đóng vai **pair programmer + kỹ sư triển khai**, không phải người quyết định sản phẩm. Con người:

- Chốt scope, ưu tiên, quyết định sản phẩm.
- Chạy Unity, Play mode, cảm nhận UX.
- Duyệt thay đổi và bằng chứng.

AI:

- Đọc code/tài liệu, chẩn đoán lỗi.
- Đề xuất slice nhỏ, viết code/prefab/scene.
- Chạy các kiểm tra tự động (compile, validator, audit).
- Ghi worklog/backlog và báo cáo.

## 4.2. Hệ thống hợp đồng (`ai-docs/`)

Đây là điểm khác biệt cốt lõi so với "chat với AI rồi copy code".

### 4.2.1. Thứ tự ưu tiên bằng chứng

1. Target project + acceptance contract của task.
2. Reference project (chỉ để đối chiếu).
3. `docs/00-current-state.md` + feature docs.
4. File Word/đề cương — chỉ là bối cảnh.

### 4.2.2. Ràng buộc cứng

- Không sửa reference project.
- Không commit secret/keystore/receipt/service config.
- Không thêm backend, multiplayer, DI framework, event bus mới, Addressables, A/B test nếu task không yêu cầu.
- Core gameplay phải chạy khi service ngoài lỗi.
- Mọi thay đổi tài nguyên phải idempotent.
- Mọi command gameplay phải validate trước khi mutate state.
- Nếu phát hiện mâu thuẫn: **dừng, ghi rõ, không đoán**.

### 4.2.3. Feature index

| ID | Contract |
|---|---|
| F01 | `ai-docs/01-core-gameplay.md` |
| F02 | `ai-docs/02-level-data.md` |
| F03 | `ai-docs/03-progression-state.md` |
| F04 | `ai-docs/04-boosters-contract.md` |
| F05 | `ai-docs/05-ui-flow-contract.md` |
| F06 | `ai-docs/06-tutorial-contract.md` |
| F07 | `ai-docs/07-monetization-contract.md` |
| F08/F09 | `ai-docs/08-platform-services-contract.md` |
| ARCH | `ai-docs/09-technical-migration-plan.md` |
| QA | `ai-docs/10-acceptance-tests.md` |

## 4.3. Task template cho AI

`ai-docs/agent-task-template.md` quy định mỗi task phải có:

```text
Feature: Fxx
Scope: ...
Not in scope: ...
Read first: contract + docs/00-current-state.md + source paths
Goal: một hành vi quan sát được
Acceptance: [ ] ...
Implementation rules:
  - Giữ core không phụ thuộc Unity/SDK
  - Ưu tiên tái sử dụng asset/dependency có sẵn
  - Không sửa file ngoài phạm vi
Verification:
  - Chạy test/build
  - Chạy một scenario runtime
  - Báo file đã đổi, file cố ý không đổi, rủi ro chưa verify
```

## 4.4. Mẫu prompt thực tế

Người dùng thường viết **rất ngắn**, có lỗi chính tả, và kỳ vọng AI tự khảo sát:

```text
"check event7d chưa có quà chưa nhận ddc, fix giúp tôi"
"item 1 chưa có nội dung quà (sau khi claim) nên có 3 trạng thái.
 lock: có thể nhận: đã nhận (có thêm masking mở + khóa+)"
"LockMask sẽ bật nếu đang bị lock"
"fix hudcoin ở màn tab home thì sẽ tắt"
```

AI phải:

1. Tìm đúng file/prefab/scene liên quan.
2. Đọc trạng thái thật (kể cả file người dùng vừa sửa trong Unity).
3. Chẩn đoán nguyên nhân gốc.
4. Sửa tối thiểu, không phá phần khác.
5. Verify và báo lại ngắn gọn.

## 4.5. Mô hình "pick" khi tái sử dụng dự án gốc

Được dùng cho Entry scene, MenuBar, Tooltip, Shop, Purchase popup, Settings, Daily Reward:

```text
1. Pick code tối thiểu để tính năng chạy được.
2. Verify compile/test.
3. Pick prefab + toàn bộ dependency closure (sprite, font, material, prefab con).
4. Báo rõ: giữ gì, bỏ gì, vì sao.
```

Nguyên tắc: **không copy nguyên code/kiến trúc lỗi**; chỉ pick hành vi + asset, viết lại phần code target-native khi cần.

## 4.6. Bộ nhớ học tập (`taste`)

Repo có `.commandcode/taste/taste/taste.md` — bộ nhớ các quy ước đã học:

- Trả lời tiếng Việt, ngắn gọn, có bảng trước→sau.
- Mọi kết luận phải verify bằng file/log/git.
- Restore asset từ git phải giữ GUID (`.meta`).
- Trước khi xoá asset phải quét GUID và liệt kê file còn dùng.
- Script xoá hàng loạt phải có `--dry-run` / `--apply`.
- Compile phải chạy qua `csc` + rsp của Unity (Editor đang mở chặn `-batchmode`).
- Test phải nằm trong asmdef có `TestAssemblies` mới được Test Runner nhận.
- Không regenerate scene khi người dùng đang sửa trong Editor.

## 4.7. Quy trình 6 bước khi giao việc cho AI

1. **Đọc contract + source caller/callee.**
2. **Viết/đổi test nhỏ nhất** cho rule.
3. **Implement một vertical slice**, giữ project compile.
4. **Chạy test/build phù hợp**; nếu không chạy được Unity, ghi rõ verification còn thiếu.
5. **Không refactor ngoài feature.**
6. **Cập nhật docs** nếu contract thay đổi.

## 4.8. Kiểm chứng đầu ra AI

### 4.8.1. Compile

Vì Unity Editor đang mở, không thể chạy `-batchmode -runTests`. Cách làm:

- Dùng `csc` của Unity + file `.rsp` trong `Library/Bee/artifacts/...`.
- Build lần lượt: `Assembly-CSharp` → `Assembly-CSharp-Editor` → assembly test.
- Chain `ref.dll` vừa build vào assembly Editor/test.
- Rebuild danh sách source từ disk vì rsp có thể stale (còn file đã xoá, thiếu file mới).

### 4.8.2. Validator YAML

Các script kiểm tra:

- fileID duy nhất trong scene/prefab.
- Mọi GUID tham chiếu resolve được (Assets + `Library/PackageCache`).
- `m_Children` ↔ `m_Father` nhất quán.
- `SceneRoots` khớp root transform.
- Không trùng key trong một block.
- Không component mồ côi.

### 4.8.3. Audit asset toàn repo

- 0 `.meta` thiếu, 0 `.meta` mồ côi, 0 GUID trùng.
- Đếm GUID dangling và phân loại: script package (vô hại), asset người dùng đã xoá, GUID không từng thuộc repo.
- Trước khi xoá asset: grep GUID trong toàn bộ prefab/scene/asset + `ProjectSettings`.

## 4.9. Bài học thực tế (lỗi AI và cách sửa)

| # | Sự cố | Nguyên nhân | Cách xử lý / bài học |
|---|---|---|---|
| 1 | Evidence "16/16 test pass" không thuộc repo | Log lấy từ project khác (`puzzle-game`) | Chỉ dùng bằng chứng trace được về repo/máy hiện tại |
| 2 | Regenerate `Home.unity` ghi đè background người dùng đang sửa | Generator chạy lại từ đầu | Khi người dùng đang mở Editor: sửa text có chọn lọc, không regenerate |
| 3 | Restore từ Recycle Bin làm mất `using` | File cũ hơn bản đã sửa | Sau restore, đọc lại file và re-apply fix |
| 4 | Prefab mismatch | Ghi `!u!4 Transform` cho root là `RectTransform` | Stripped transform phải cùng class với prefab root |
| 5 | Shop không render | `ShopUI` cache service trong `Awake` khi service chưa tạo | Resolve tại thời điểm dùng + bind lại `OnEnable`/`Start` |
| 6 | Booster chạy sai loại | Thêm `None = 0` làm lệch index enum đã serialize | Enum là contract serialized; dùng `None = -1`, thêm test |
| 7 | Popup mua không hiện | Root `m_IsActive: 0` nên `Awake` không chạy | Dùng `Acquire()` tìm cả inactive + `SetActive(true)` |
| 8 | Toàn bộ game im lặng | `AudioConfig` ghi `fileID 7400000` (AnimationClip) thay vì `8300000` (AudioClip) | Audit asset + kiểm tra kiểu fileID |
| 9 | Test không xuất hiện trong Test Runner | Test nằm ngoài asmdef `TestAssemblies` | Mỗi test phải có asmdef đúng |
| 10 | LevelList chỉ hiện 1 nút | `template` trỏ GameObject thay vì component | `[SerializeField]` phải trỏ component fileID |

## 4.10. Ưu và nhược điểm của quy trình AI

**Ưu điểm**

- Tốc độ khảo sát codebase lớn, tìm đúng file nhanh.
- Có thể chạy kiểm tra tự động liên tục (compile, audit, validator).
- Worklog/backlog được cập nhật đều, giảm mất ngữ cảnh.
- Dễ tái lập quy trình cho người khác.

**Nhược điểm / giới hạn**

- AI có thể "tự tin sai" nếu không đọc file thật → bắt buộc verify.
- Không thể thay người dùng cảm nhận UX.
- Không chạy được Unity Play mode trong môi trường không có Editor.
- Có thể ghi đè file người dùng đang mở nếu không cẩn thận.
- Tài liệu do AI viết có thể lệch thực tế → cần review định kỳ.

## 4.11. Cách tái lập quy trình AI

1. Tạo thư mục `ai-docs/` với `README.md` (luật), `agent-task-template.md`, contract từng feature.
2. Tạo `docs/00-current-state.md` mô tả đúng hiện trạng + nợ kỹ thuật.
3. Tạo `tasks/todo.md` với ID, Verify, Deps.
4. Tạo `docs/weekly-worklog.md` để ghi từng phiên.
5. Khi giao việc: dùng template, yêu cầu AI đọc contract + source trước.
6. Bắt buộc AI báo: file đã đổi, verify, rủi ro chưa verify.
7. Duy trì "taste" cho các quy ước lặp lại.

---

# Chương 5 — Kiến trúc và triển khai

## 5.1. Thông tin project

| Thuộc tính | Giá trị |
|---|---|
| Product name | Yarn Sort Master |
| Company | Bravestars |
| Bundle version | 1.6 |
| Unity | 6000.3.21f1 |
| Orientation | Auto |
| Input | Old Input Manager |
| Build scenes | `EntryScene` → `Home` → `Gameplay` |

## 5.2. Cấu trúc thư mục

```text
game/
  Assets/
    Scenes/
      EntryScene.unity
      Home.unity
      Gameplay.unity
    _Game/
      Scripts/
        Core/        # service nền: bootstrap, entry flow, economy, save, shop, daily, settings, audio
        Gameplay/    # GameManager, Box, Slot, WaitingArea, Yarn, Booster, Win/Lose, UI gameplay
        Gameplay/Board/
        Meta/        # HomeUI, ShopUI, LevelList, PurchaseSuccessPopup, DailyReward*, Settings*
        UI/          # CoinHUD, CanvasLoadingUI, EffectUIManager
        Utils/       # Singleton, SplashItem, NumberFormatter, stubs
        Pool/        # PoolManager, PrefabPool
        LevelData/   # LevelCatalog, LevelDefinition, LevelModeType (asmdef WoolSort.LevelData)
        Tutorial/    # marker còn lại
        Editor/      # editor scripts nội bộ _Game
      Configs/       # GameConfig, YarnColorDatabase
      Prefab/        # Box, Slot, Board, CoinHUD, Rope...
      Prefab/UI/     # LevelButton, LevelList, ShopItem, SettingsPopup, DailyRewardItem...
      Prefab/VFX/
      Resources/     # DailyRewardConfig, DailyRewardPopup, Textures...
      Textures/
      Mesh/, Materials/, Shader/, Animations/
    Editor/          # 21 editor tool (Level Studio, Catalog, Reskin, Yarn, Daily...)
    Resources/
      LevelCatalog.asset
      AudioConfig.asset
      Levels/Level1..7.prefab
      LevelColorMatrix/
      Images/
    Tests/
      EditMode/LevelCatalogTests.cs (+ asmdef)
      PlayMode/BoosterLogicTests.cs, BoosterSceneTests.cs, ShopPurchaseTests.cs, ResultPopupProgressionTests.cs
    Plugins/, _KIT/, Demigiant/, Confetti FX/, TextMesh Pro/
  Packages/manifest.json
  ProjectSettings/
```

## 5.3. Assembly definition

| asmdef | Vai trò |
|---|---|
| `WoolSort.LevelData` | Catalog/level data thuần, không phụ thuộc gameplay |
| `LevelCatalogTests` | EditMode test cho catalog |
| `ResultPopupProgressionTests` | PlayMode test (booster, shop, progression) |
| Thư viện ngoài | Dreamteck, LeanTouch, ToolbarExtender |
| `Assembly-CSharp` | Phần lớn gameplay + service |
| `Assembly-CSharp-Editor` | `Assets/Editor` + `_Game/Scripts/Editor` |

## 5.4. Luồng scene và bootstrap

```text
[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]
ServiceBootstrap:
  EnsureEconomy() → EconomyService (DontDestroyOnLoad)
  EnsureShop()    → ShopService
  EnsureDailyReward() → DailyRewardService

EntryScene:
  EntryBootstrap / EntryFlow / CanvasLoadingUI
  autoLoad=0, alwaysGoHome=1
  → loading 3s → Home

Home:
  HomeUI (3 tab), LevelList, Settings, Daily Reward, Shop
  PLAY → EntryFlow.StartCurrentLevel() → Gameplay

Gameplay:
  GameManager.Start():
    đọc level order từ EconomyService.GetPlayLevel()/GetLevel()
    map qua LevelCatalog → asset id
    load Resources/Levels/Level{id}
    khởi tạo board/box/slot/waiting/HUD
    chạy intro sequence
```

Vì service được tạo **trước mọi `Awake` của scene**, các scene mở trực tiếp (Home/Gameplay) vẫn có dữ liệu, tránh NRE do thứ tự `Awake` không xác định.

## 5.5. Lớp service

| Service | File | Trách nhiệm |
|---|---|---|
| `ServiceBootstrap` | `Core/ServiceBootstrap.cs` | Tạo service trước scene load |
| `EntryBootstrap` / `EntryFlow` | `Core/EntryBootstrap.cs`, `Core/EntryFlow.cs` | Điều hướng Entry → Home/Gameplay |
| `EconomyService` | `Core/EconomyService.cs` | Ledger coin/heart/booster/level + event |
| `SaveService` + `PlayerData` | `Core/SaveService.cs`, `Core/PlayerData.cs` | Lưu JSON trong PlayerPrefs, validate |
| `ShopService` | `Core/ShopService.cs` | Danh sách item + giao dịch coin |
| `DailyRewardService` | `Core/DailyRewardService.cs` | Streak 7 ngày, claim 1 lần/ngày |
| `DailyRewardConfig` | `Core/DailyRewardConfig.cs` | ScriptableObject dữ liệu 7 ngày |
| `SettingsService` | `Core/SettingsService.cs` | Sound/Music/Vibration |
| `AudioService` / `AudioConfig` / `AudioBootstrap` | `Core/Audio*.cs` | Phát âm thanh, BGM theo scene |

## 5.6. Gameplay

| Class | Trách nhiệm |
|---|---|
| `GameManager` | Vòng đời màn, cây dữ liệu len, chọn màu hộp, win/lose, input gate, auto-zoom |
| `BoxManager` | Spawn/unlock/close box, pool, in ảnh, rope, refill từ temp box |
| `Box` | 1 hộp 3 slot, material màu, animation, pooling |
| `Slot` | Trạng thái slot hộp/chờ, callback animation, anchor rope |
| `SlotManager` | Registry slot + ưu tiên đặt len |
| `WaitingAreaManager` | 5–7 slot chờ, refill sang hộp, cảnh báo, AddSlot |
| `YarnPart` | 1 lớp len: dissolve, hold preview, tap → đặt |
| `YarnPartInfo` | Node dữ liệu + priority động |
| `YarnPartLayerSpawner` | Spawn lớp trong theo nhu cầu |
| `LevelYarnInfoHolder` | Dữ liệu level: màu, random pool, ngưỡng spawn box |
| `YarnRopeController` / `Rope` / `RopeBoard` | Rope spline có sag/sway, rush |
| `YarnColorManager` / `YarnColorDatabase` | Map màu → material |
| `BoosterManager` | AddSlot/Clean/MagicBox |
| `BoosterItem` / `BoosterBarUI` / `MoreBoosterPopupUI` | UI booster |
| `GameUIController` | HUD, anchor 3D, win/lose delay |
| `WinPopup` / `LosePopup` | Màn kết quả |
| `JamImage` / `LevelColorMatrix` | Tô ảnh 84×84 |
| `CoinManager` | Facade coin (delegate EconomyService) |
| `PoolManager` / `PrefabPool<T>` | Pool `IPoolable` |

## 5.7. Lưu trữ

`PlayerData` (JSON trong `PlayerPrefs["WoolSort_PlayerData_v1"]`):

```csharp
// Progression
public int currentLevel = 1;
public int lastPlayedLevel = 1;

// Resources
public int coin = 100;
public int heart = 5;
public int maxHeart = 5;
public long heartRefillAtUtcTicks = 0;
public int infiniteHeartMinutes = 0;

// Boosters
public int addSlotCount = 0;
public int cleanCount = 0;
public int magicBoxCount = 0;

// Flags
public bool removeAds = false;
public bool sound = true;
public bool music = true;
public bool vibration = true;
public bool tutorialDone = false;
public bool dailyRewardSeen = false;

// Daily reward
public int dailyStreak = 0;

// Lưu dạng UTC ticks của lần claim gần nhất
public long lastDailyClaimUtcTicks = 0;
```

Quy tắc:

- `SaveService.Save()` gọi `PlayerPrefs.Save()` ngay sau mỗi thay đổi.
- `Load()` có try/catch, JSON hỏng → reset sạch.
- `PlayerData.Validate()` clamp âm, level ≥ 1, streak 0..6.
- Chưa có migration phiên bản (dự án mới, chưa phát hành).

## 5.8. Data asset

| Asset | Đường dẫn | Vai trò |
|---|---|---|
| `GameConfig` | `_Game/Configs/GameConfig.asset` | Slot, scale, dissolve, hold, rotate, zoom |
| `YarnColorDatabase` | `_Game/Configs/YarnColorDatabase.asset` | Material theo màu (len/hộp/slot) |
| `LevelCatalog` | `Resources/LevelCatalog.asset` | Nguồn chuẩn thứ tự màn + id + mode + seed |
| `LevelColorMatrix` | `Resources/LevelColorMatrix/{Level}.asset` | Ma trận màu 84×84 |
| `AudioConfig` | `Resources/AudioConfig.asset` | BGM + 21 SFX + volume |
| `DailyRewardConfig` | `_Game/Resources/DailyRewardConfig.asset` | 7 ngày × danh sách quà |
| Level prefab | `Resources/Levels/Level{1..7}.prefab` | Model + YarnPart + holder |

### LevelCatalog

```csharp
class LevelDefinition {
    int orderIndex;          // thứ tự chơi + giá trị lưu
    int levelId;             // asset id → Levels/Level{id}
    int referenceOrderIndex; // vị trí trong order gốc, -1 nếu không có
    LevelModeType modeType;  // Normal / Hard / VeryHard
    GameObject prefab;       // Resources/Levels/Level{id}
    int seed;                // DefaultSeed(levelId) = levelId*7919 + 104729
}
```

- Policy `Sequential`: order = levelId đã sort (đang dùng).
- Policy `ReferenceOrder`: theo order gốc (sẵn sàng khi có đủ content).
- `Validate()` kiểm tra: order liên tục/không trùng, levelId không trùng, prefab tồn tại và đúng tên, mode hợp lệ.
- `LevelCatalogValidator` + `IPreprocessBuildWithReport` **chặn build** khi có lỗi.

## 5.9. Quy ước serialized/prefab

- CanvasScaler **1080×2160** mọi scene.
- Prefab instance: copy block `PrefabInstance` từ scene đang chạy; stripped transform phải cùng class với prefab root.
- `[SerializeField]` trỏ component phải dùng fileID của **component**, không phải GameObject.
- Popup giữ root inactive trong scene; code dùng `Acquire()` tìm cả inactive.
- Scene singleton không dùng `DontDestroyOnLoad`; global service tự bootstrap.

---

# Chương 6 — Các tính năng đã triển khai

## 6.1. Core puzzle

### Luồng xử lý

```text
Tap YarnPart
  → LeanTouch event
  → SlotManager.GetNextAvailableSlot(color)
      ├─ hộp cùng màu, ưu tiên hộp nhiều slot đã đầy nhất
      └─ nếu không có → slot chờ trái nhất
  → Slot.Fill(part) (đánh dấu ngay)
  → SpawnRope + animation
  → OnAnimationFilled
      → WaitingAreaManager.TryRefillSlot (nếu là slot chờ)
      → GameManager.RecheckLoseAfterWaitingAnimation
  → Box đủ 3 slot → OnBoxFull → OnBoxClose
      → BoxManager.PrintImage → JamImage.Print + rope
      → despawn box → spawn box mới
  → Hết len → WinLevel
```

### Điểm quan trọng

- `Slot.Fill` đánh dấu filled ngay nhưng `IsAnimationFilled` chỉ sau animation → mọi logic win/lose phải tôn trọng mốc này.
- `SlotManager.GetNextAvailableSlot` ưu tiên hộp đầy nhất để giảm số hộp mở.
- Màu `Rainbow` nhận mọi màu; hộp `None` không nhận gì.
- Pool cho box/rope giảm allocation.

## 6.2. Level catalog và dữ liệu màn

- 7 level trong `Resources/Levels`.
- Catalog là single source of truth; runtime chỉ đọc qua `LevelCatalogProvider`.
- Build gate chặn build khi dữ liệu sai.
- Mapper sinh catalog từ `tools/level_reference.json` (order/mode gốc) + quét prefab.

## 6.3. Booster

| Booster | Luồng |
|---|---|
| AddSlot | `BoosterManager.UseBoosterAddSlot` → `WaitingAreaManager.AddSlot()` → cap 7 |
| Clean | Thu len chờ vào `tempBoxContainer` → `BoxManager.RefillFromTempBox` khi có hộp mới |
| MagicBox | Lấy box đầu → đổi Rainbow → kéo len thiếu màu vào đúng box → hẹn 2 box min-priority |

- Mua bằng coin qua `MoreBoosterPopupUI`.
- Inventory lưu trong `PlayerData`, thay đổi qua `EconomyService`.
- Tooltip tiếng Việt khi không đủ điều kiện/không đủ coin.

## 6.4. Win / lose / restart

- Win: rush in ảnh/rope, chờ box cuối đóng + in xong → `WinPopup.Show(10 coin)`.
- Win popup: coin bay vào HUD, Next → `LoadNextLevel`, hết catalog quay về đầu.
- Lose: `LosePopup.Show(level, progress%)`; Restart reload level; Home (hiện đang reload — stub).
- Fallback timeout: lose 2.5s, win 5s để tránh kẹt animation.

## 6.5. Economy và save

- `EconomyService` là ledger duy nhất, phát event `OnCoinChanged/OnHeartChanged/OnBoosterChanged`.
- `SaveService` lưu JSON, validate trước khi ghi.
- `CoinManager` chỉ còn là facade cho gameplay cũ, delegate về `EconomyService`.
- Heart có dữ liệu và refill nhưng **chưa có UI tiêu hao**.

## 6.6. Home, navigation, level list

- 3 tab: Home / Shop / Coming Soon (qua `MenuBarManager`).
- `HomeUI` điều phối page, PLAY, coin HUD (chỉ tab Shop), Settings.
- `LevelList` hiển thị 4 ô quanh level hiện tại, màu theo độ khó; **chỉ hiển thị**, không chọn màn.
- `SettingsPopup` + `SettingsButton` ở Home; Gameplay có nút nhưng thiếu instance popup.

## 6.7. Shop và popup mua

- 5 item hardcode trong `ShopService`: 2 gói coin (giá 0), 3 booster (80/120/60).
- `ShopUI` instantiate `ShopItem.prefab`, bind `ShopItemView`.
- Icon gán qua `ShopIconRef { id, sprite }` trong Inspector.
- Nút BUY luôn bấm được; thiếu coin → giá đỏ + tooltip.
- Mua xong → `PurchaseSuccessPopup` hiện icon/tên/số lượng, coin bay về HUD.

## 6.8. Coin HUD và hiệu ứng coin bay

- `CoinHUD` subscribe `EconomyService.OnCoinChanged`.
- Hiển thị số coin dạng K/M qua `NumberFormatter`.
- `SplashItem.SplashCoin(quantity, from, to, ...)` sinh coin bay theo DOTween.
- Coin HUD nằm dưới `SafeArea` nhưng **chỉ bật ở tab Shop** (Home/ComingSoon tắt).
- Daily reward claim cũng dùng coin bay về HUD.

## 6.9. Daily Reward 7 ngày

### Dữ liệu

7 ngày, mỗi ngày nhiều phần thưởng (`coin`, `addslot`, `clean`, `magicbox`):

| Ngày | Phần thưởng |
|---|---|
| 1 | 60 coin |
| 2 | Magic Box ×1 |
| 3 | 80 coin |
| 4 | Add Slot ×1 |
| 5 | 120 coin |
| 6 | Clean ×1 |
| 7 | 240 coin + Magic Box ×2 |

### Logic

- `DailyRewardService`:
  - `Streak` 0..6.
  - `CanClaimToday()`: so sánh ngày local của `lastDailyClaimUtcTicks` với hôm nay.
  - `Claim()`: cấp toàn bộ reward của ngày, tăng streak vòng 7, lưu tick UTC.
  - `NormalizeStreak()`: nghỉ ≥ 2 ngày → reset streak; tick ở tương lai → coi như chưa claim.
  - `TimeUntilNextClaim()`: còn bao lâu tới nửa đêm local.
- Tick lưu bằng `DateTime.UtcNow.Ticks`, đọc bằng `new DateTime(ticks, DateTimeKind.Utc).ToLocalTime().Date` — tránh lỗi `FromBinary` hiểu nhầm local (lệch ngày ở UTC+7).

### UI

- `DailyRewardPopup`: 7 ô, nút CLAIM/CLAIM inactive, countdown, config từ Resources.
- Mỗi ô (`DailyRewardItem`) có 3 trạng thái:
  1. **Lock** — nền xám + `LockMask` (masking tối) + icon ổ khoá; quà vẫn hiển thị nhưng mờ.
  2. **Có thể nhận** — `TodayState` (Slot_Daily_1B) + quà full màu.
  3. **Đã nhận** — quà vẫn hiển thị + tick xanh ở góc.
- Thứ tự render đã sửa để overlay nền nằm **dưới** nội dung quà (trước đây TodayState che mất quà).
- Countdown: `Claim now!` hoặc `Next reward in HH:MM:SS`.
- Claim xong:
  - `PurchaseSuccessPopup.ShowReward(icon, "Daily Reward!", "Day N", "+60 coins", playCoinFly)`.
  - Coin bay về HUD.
- Nút mở nằm ở Home (góc trên trái); popup nằm trong `Resources`.

### Công cụ sinh UI

`DailyRewardSetupEditor` (menu `WoolSort/Daily Reward/...`) tạo config + 3 prefab; auto-setup khi thiếu popup.

## 6.10. Settings và audio

- Settings: 3 toggle sound/music/vibration + version; lưu `PlayerData`; áp dụng ngay.
- `AudioService`: 2 BGM + 21 SFX, volume từng clip.
- `AudioBootstrap`: tự tạo ở mọi scene, đổi BGM theo scene.
- Sự cố đã sửa: `AudioConfig` ghi sai `fileID` (AnimationClip thay vì AudioClip) làm toàn bộ game im lặng.

## 6.11. Các phần chưa hoàn thiện / bị cắt

| Hạng mục | Trạng thái |
|---|---|
| Revive | Có logic màu/box, chưa có UI/flow |
| Hard/VeryHard | Có enum, 7 level đều Normal |
| Tutorial runtime | Chỉ còn marker, chưa có sequence |
| Heart gate | Có dữ liệu, chưa tiêu hao |
| Level selection | `LevelList` chỉ hiển thị |
| Localization | Hardcode chuỗi |
| Settings popup trong Gameplay | Thiếu instance |
| IAP/Ads/Analytics | Cắt khỏi MVP |
| Build Android | Chưa thực hiện trong repo |
| Save migration | Chưa cần (chưa phát hành) |

## 6.12. Bảng truy vết feature → file

| Feature | File chính |
|---|---|
| Entry flow | `Core/EntryBootstrap.cs`, `Core/EntryFlow.cs`, `UI/CanvasLoadingUI.cs` |
| Home | `Meta/HomeUI.cs`, `_HomeFrame/GUI/HomeScreen/MenuBarManager.cs` |
| Level list | `Meta/LevelList.cs`, `Meta/LevelButton.cs` |
| Core gameplay | `Gameplay/GameManager.cs`, `BoxManager.cs`, `Box.cs`, `Slot.cs`, `SlotManager.cs`, `WaitingAreaManager.cs` |
| Yarn/layer | `YarnPart.cs`, `YarnPartInfo.cs`, `YarnPartLayerSpawner.cs`, `LevelYarnInfoHolder.cs` |
| Rope | `YarnRopeController.cs`, `Rope.cs`, `Board/RopeBoard.cs` |
| Ảnh | `JamImage.cs`, `LevelColorMatrix.cs` |
| Booster | `BoosterManager.cs`, `BoosterItem.cs`, `BoosterBarUI.cs`, `MoreBoosterPopupUI.cs` |
| Win/lose | `WinPopup.cs`, `LosePopup.cs`, `GameUIController.cs` |
| Economy/save | `Core/EconomyService.cs`, `Core/SaveService.cs`, `Core/PlayerData.cs` |
| Shop | `Core/ShopService.cs`, `Meta/ShopUI.cs`, `Meta/ShopItemView.cs`, `Meta/PurchaseSuccessPopup.cs` |
| Daily reward | `Core/DailyRewardService.cs`, `Core/DailyRewardConfig.cs`, `Meta/DailyReward*.cs` |
| Settings/audio | `Core/SettingsService.cs`, `Core/AudioService.cs`, `Meta/Settings*.cs` |
| Coin HUD | `UI/CoinHUD.cs`, `Utils/SplashItem.cs` |
| Level data | `LevelData/LevelCatalog.cs`, `LevelDefinition.cs`, `LevelModeType.cs` |

---

# Chương 7 — Editor tool và pipeline nội dung

## 7.1. Tổng quan

Repo có **21 file Editor** dưới `game/Assets/Editor/`, chia nhóm:

- Level Studio (5 tab).
- Level catalog (builder + validator + build gate).
- Yarn/decor/dissolve.
- Reskin + inventory.
- Audio + Daily Reward setup.
- Tiện ích: scene resave, texel density, model export, color usage.

## 7.2. Level Studio — `Tools/Puzzle/Level Studio`

Cửa sổ hub 5 tab:

### Tab 1 — Tạo level (`LevelQuickCreatorWindow`)

- Input: model FBX; chọn mesh, màu len, số lớp trong.
- Template: `Assets/Resources/Levels/Level1.prefab`.
- Output: `Assets/Resources/Levels/Level{N}.prefab` gồm `ModelRoot`, `YarnPart` + `MeshCollider` cho từng mesh, holder rebuild, scale chuẩn `TargetModelMaxSize = 6.5698`.

### Tab 2 — Độ khó & màu (`LevelDifficultyTool`)

- Quét toàn bộ level prefab.
- Tính score theo công thức ở mục 2.4.
- Chỉnh lại số len giữ ràng buộc `%3`.
- Ghi lại prefab.

### Tab 3 — Validate (`BoxImageColorValidator`)

Kiểm tra:

- Material trong `YarnColorDatabase` cho từng màu.
- Holder không stale.
- Tổng len `%3 == 0`; từng màu `%3 == 0`.
- Random pool đủ nhu cầu.
- Matrix ảnh tồn tại/hợp lệ.
- Mô phỏng `JamImage.BuildCellOwners` trên PNG.

Auto-fix: `AutoFixDiv3`, `AutoFixPerColorDiv3`, `AutoFixRandomPool`, `AutoFixMissingColors`.

### Tab 4 — Prompt ảnh (`ColorPromptWindow`)

- Tính bảng hex màu + số ô chính xác (84×84 = 7.056).
- Sinh prompt AI để tạo ảnh màn chơi.
- Lưu setting vào `Assets/ColorPromptSettings.json`.

### Tab 5 — Matrix ảnh (`LevelImageMatrixTool`)

- Input: `Resources/Images/{Level}.png|jpg`, dataset level, quantize slider.
- Output:
  - `Resources/LevelColorMatrix/{Level}.asset`.
  - JSON nhóm để chỉnh tay.
  - `Assets/MatrixExport/{Level}.png` (756px) cho Photoshop.
- Tự động: đọc ảnh, center-crop vuông, chia block, lấy màu trung bình, nearest color, despeckle, gán owner/display.

## 7.3. Level catalog

- `Tools/Level/Build Level Catalog` (`LevelCatalogBuilder`):
  - Đọc `tools/level_reference.json` + quét `Resources/Levels/Level*.prefab`.
  - Ghi `Resources/LevelCatalog.asset`.
  - Batch mode có `EditorApplication.Exit(0)`.
- `Tools/Level/Validate Level Catalog` (`LevelCatalogValidator`):
  - Kiểm tra catalog, log + dialog.
  - `BuildGate : IPreprocessBuildWithReport` chặn build khi có Error.

## 7.4. Yarn / decor / dissolve

| Menu | Chức năng |
|---|---|
| `Tools/Puzzle/3. Quét decor gắn len (scene đang mở)` | Kiểm tra `YarnDecoration` mồ côi, thiếu Rigidbody, list lệch |
| `Tools/Puzzle/4. Quét decor tất cả Level prefab` | Như trên cho toàn bộ prefab |
| `Tools/Puzzle/Chuyển ThreadMat sang YarnDissolve` | Migrate material sang shader `WoolSort/YarnDissolve` |
| `Tools/Puzzle/Quét trục mesh len` | Báo trục wipe + span, phát hiện mesh degenerate |
| `Tools/Puzzle/Chuyển len thành decor` | Chuyển mesh len ↔ decor, có preview 3D, cập nhật holder |

## 7.5. Reskin và inventory ảnh

### Python

- `tools/inventory_images.py`
  - Xây index GUID từ `.meta`.
  - Quét tham chiếu GUID trong scene/prefab/asset/code.
  - BFS từ root (scene, `Resources`, code) → used/unused.
  - Đọc kích thước PNG/JPEG, PPU, textureType, spriteBorder.
  - Output: `docs/asset-inventory-images.md`.

- `tools/reskin.py`
  - `--src` folder ảnh mới, `--assets` mặc định `game/Assets`.
  - `--map map.csv` (old path, new file).
  - Dry-run mặc định; `--apply` mới ghi.
  - Backup `game/.reskin_backup/<timestamp>/`.
  - `--mirror-booster` copy `_Game/BoosterUI_Picked/` → `_Game/Resources/BoosterUI/`.

### Unity

- `Tools/Reskin/Reskin hàng loạt` (`BulkReskinWindow` + `ReskinRuntimeScanner`):
  - Quét dependency từ 3 scene gốc (`EntryScene;Home;Gameplay`) → ~70 ảnh runtime.
  - Phân loại: Running / HiddenAtLoad / Fallback / Unused.
  - Bảng old → new → dest path, preview ảnh, backup, apply, undo.
  - Không xoá/tạo lại asset → giữ GUID, 9-slice, PPU.

## 7.6. Audio và Daily Reward setup

- `WoolSort/Setup Audio`: tạo `AudioConfig.asset`, thêm `AudioBootstrap` vào EntryScene, lưu scene.
- `WoolSort/Daily Reward/1. Build Config + Prefabs`: tạo config + prefab icon/item/popup.
- `WoolSort/Daily Reward/2. Add Popup + Button to Home`: thêm instance + nút vào Home.

## 7.7. Tiện ích khác

| Tool | Chức năng |
|---|---|
| `Tools/Resave Scenes` | Re-serialize scene để sửa lỗi "Prefab mismatch" |
| `Tools/Texel Density Tool` | Đo/chỉnh mật độ texel của mesh |
| `Tools/Model Asset Exporter` | Export GameObject/mesh ra OBJ/FBX |
| `Tools/Color Usage Tracker` | Thống kê số len theo màu, ghi `Assets/ColorUsageReport.txt` |
| `Tools/Level/Build Level Catalog` | Sinh catalog |
| `LevelDependencyTool` | Xuất dependency của level prefab ra `Temp/level-dependencies.txt` |
| `DisableBitcode` | Post-process iOS (di sản SDK cũ) |

## 7.8. Quy trình tạo một level mới (A → Z)

```text
1. Import FBX vào Assets/_Game/Mesh (hoặc thư mục level).
2. Tools/Puzzle/Level Studio → tab "Tạo level":
   - chọn FBX, chọn mesh, gán màu, số lớp
   - tạo Level{N}.prefab trong Resources/Levels.
3. Tab "Độ khó & Màu": chấm điểm, cân lại số len (giữ %3).
4. Tạo ảnh:
   - Tab "Prompt ảnh": copy prompt → tạo ảnh bằng AI
   - đặt ảnh vào Resources/Images/Level{N}.png
   - Tab "Matrix ảnh": extract → Resources/LevelColorMatrix/Level{N}.asset
5. Tab "Validate": chạy kiểm tra, auto-fix nếu cần.
6. Tools/Level/Build Level Catalog: cập nhật catalog.
7. Tools/Level/Validate Level Catalog: đảm bảo 0 error.
8. Play Home/Gameplay để test.
```

---

# Chương 8 — Kiểm thử và đánh giá

## 8.1. Chiến lược kiểm thử

1. **Compile gate** — 3 assembly (`Assembly-CSharp`, `Assembly-CSharp-Editor`, test assembly).
2. **Data validation** — catalog validator chặn build; Level Studio validate prefab/matrix.
3. **Unit/PlayMode tests** — catalog, booster logic, booster scene, shop.
4. **YAML/asset integrity** — validator scene/prefab + audit GUID/meta.
5. **Manual Play mode** — checklist từng luồng.
6. **Editor.log forensics** — xác nhận compile error, prefab mismatch, đã vào Play mode chưa.

## 8.2. Test catalog

### EditMode — `LevelCatalogTests.cs` (22 test)

| Nhóm | Test |
|---|---|
| Catalog | `LC_01` asset tồn tại; `LC_02` catalog hợp lệ; `LC_03` order liên tục; `LC_04` levelId unique; `LC_05` prefab đúng tên; `LC_06` mode hợp lệ; `LC_07` seed tất định |
| Lookup | `LV_01..08` GetByOrder/Id/Next/First/Playable/Mode |
| Validator | `LVAL_01..07` duplicate order/id, thiếu prefab, sai tên, order gãy, catalog rỗng |

### PlayMode

- `BoosterLogicTests` (9): priority, GameData, config, enum mapping, cost/unlock.
- `BoosterSceneTests` (3): AddSlot +1 tới cap, spam không vượt cap, Clean khớp trạng thái chờ.
- `ShopPurchaseTests` (3): mua trừ coin/cộng booster, coin pack lặp lại, hiện popup thưởng.
- `ResultPopupProgressionTests`: Next tăng level, Retry giữ level (test cũ, có thể stale).

### Contract CORE-01..16

Bảng test matrix trong `docs/week1-core-contract.md`, bao phủ:

- Ưu tiên đặt len, hộp đầy nhất, tie-break `boxOrder`.
- Fallback slot chờ trái nhất.
- Từ chối màu sai/hộp đầy mà không đổi state.
- Double-select, input lock.
- Win/lose phát một lần, không win khi còn animation.
- Deterministic replay.

## 8.3. Kết quả hiện tại

| Hạng mục | Kết quả |
|---|---|
| Compile runtime + editor | PASS (theo worklog các vòng) |
| Catalog tests | 22/22 pass (theo `tasks/todo.md`) |
| Scene/prefab validator | 0 fileID trùng, 0 ref gãy ở các scene/prefab đã sửa |
| Asset audit | 0 GUID trùng, 0 `.meta` thiếu (các đợt audit chính) |
| Manual Play | Nhiều luồng đã test; một số còn chờ (audio, daily, reskin) |

> Lưu ý: bằng chứng phải trace được về repo này. `docs/evidence/w1-editmode-tests.md` từng bị phát hiện là log của project khác và đã được đánh dấu trong `tasks/todo.md` (W4-01).

## 8.4. Checklist manual tiêu biểu

### Khởi động / Home

1. Mở Entry → loading → Home.
2. Tab Home/Shop/ComingSoon chuyển đúng.
3. HUD coin chỉ hiện ở Shop.
4. Settings mở/đóng, toggle có hiệu lực.
5. PLAY vào đúng level.

### Gameplay

1. Chạm len ngoài cùng → bay vào hộp cùng màu.
2. Hộp đầy 3 → đóng, ảnh tô dần.
3. Vùng chờ đầy → LosePopup.
4. Hết len → WinPopup + coin bay.
5. Next sang màn tiếp; Retry giữ màn.
6. 3 booster hoạt động, trừ inventory đúng.
7. Rotate/zoom/slider.

### Daily Reward

1. Mở popup từ Home.
2. Ngày 1 sáng, ngày 2–7 khoá (mask + ổ khoá).
3. Claim → quà cộng, popup nhận thưởng + coin bay.
4. Countdown hiện "Next reward in ...".
5. Sau claim, ô hôm nay có tick + vẫn thấy quà.
6. Đóng/mở lại: trạng thái giữ đúng.

### Shop

1. Mua booster đủ coin → trừ coin + cộng booster + popup.
2. Không đủ coin → giá đỏ + tooltip.
3. Mua gói coin → coin bay + HUD cập nhật.

## 8.5. Hạn chế đã biết

- Play mode chưa được chạy tự động trong CI (không có batch test).
- Một số test cũ (`ResultPopupProgressionTests`) có thể stale sau khi UI đổi.
- `Gameplay.unity` còn vài lỗi validator từ đợt rename asset cũ (LeanTouchSimulator).
- `CoinManager` và `GameData` stub còn song song với `EconomyService` (nợ kỹ thuật).
- Revive/tutorial/heart gate chưa hoàn thiện.
- Chưa build APK trong repo.

## 8.6. Rủi ro và hướng xử lý

| Rủi ro | Ảnh hưởng | Hướng |
|---|---|---|
| Race condition animation vs rule | Thua sớm / kẹt | Giữ cơ chế deferred + timeout |
| Dữ liệu level lệch | Không load/không cân màu | Validator + build gate |
| Save trùng nguồn coin | Sai số dư | Hợp nhất về `EconomyService` |
| Test stale | Báo pass giả | Chạy lại Test Runner, xoá/sửa test cũ |
| Thiếu bằng chứng build | Không bảo vệ được | Build APK + log/ảnh ngay |

---

# Chương 9 — Hướng dẫn tái lập từ đầu

## 9.1. Yêu cầu môi trường

| Thành phần | Phiên bản |
|---|---|
| Unity Hub + Unity Editor | **6000.3.21f1** (đúng revision `c02631ffc030`) |
| Git | Bất kỳ |
| Python (cho tools) | 3.10+ |
| Thiết bị/emulator Android | Android 9+ (khuyến nghị) |
| OS phát triển | Windows 10/11 |

> Nếu mở bằng Unity khác version, Unity sẽ upgrade project và có thể phá serialized data. Luôn dùng đúng `ProjectVersion.txt`.

## 9.2. Lấy source và mở project

```bash
git clone git@github.com:nguyenthehoang98/wool-sort.git
cd wool-sort
```

Mở Unity Hub → Add project → chọn thư mục `game/` → mở bằng 6000.3.21f1.

Lần đầu mở:

1. Chờ import toàn bộ asset (có thể lâu).
2. Kiểm tra Console không có compile error.
3. Mở `Assets/Scenes/EntryScene.unity`.

## 9.3. Cấu hình build scenes

`File → Build Profiles` (hoặc Build Settings):

```text
0: Assets/Scenes/EntryScene.unity
1: Assets/Scenes/Home.unity
2: Assets/Scenes/Gameplay.unity
```

Đảm bảo cả 3 scene được tick.

Kiểm tra nhanh:

```text
ProjectSettings/EditorBuildSettings.asset
ProjectSettings/ProjectVersion.txt  → 6000.3.21f1
```

## 9.4. Chạy game

1. Mở `EntryScene`.
2. Nhấn Play.
3. Loading → Home.
4. Bấm PLAY → Gameplay.

### Chạy trực tiếp từng scene

- `Home`: có `HomeUI` tự `EnsureEconomy/EnsureShop`; PLAY vẫn cần `EntryFlow` (HomeUI tự tạo nếu thiếu).
- `Gameplay`: `ServiceBootstrap` tạo service trước scene load; level đọc từ save/catalog.

## 9.5. Tạo level mới

```text
1. Import FBX.
2. Tools/Puzzle/Level Studio → tab Tạo level → chọn mesh/màu/số lớp → tạo prefab.
3. Tab Độ khó & Màu → cân score.
4. Tạo ảnh: tab Prompt ảnh → copy prompt → tạo ảnh → đặt vào Resources/Images.
5. Tab Matrix ảnh → extract matrix.
6. Tab Validate → auto-fix nếu cần.
7. Tools/Level/Build Level Catalog.
8. Tools/Level/Validate Level Catalog (phải 0 error).
9. Play để test.
```

## 9.6. Reskin

### Cách Unity (khuyến nghị)

```text
Tools/Reskin/Reskin hàng loạt
  → "Quét ảnh dùng trong game" (mặc định 3 scene gốc)
  → gán ảnh mới cho từng dòng
  → "Áp dụng" (tự backup + giữ .meta/GUID)
```

Hoàn tác: nút "Hoàn tác từ backup" (dùng thư mục timestamp mới nhất).

### Cách Python

```bash
python tools/reskin.py --src D:/new_skin              # dry-run, xem danh sách
python tools/reskin.py --src D:/new_skin --apply      # ghi thật
python tools/reskin.py --src D:/new_skin --apply --mirror-booster
```

Audit ảnh:

```bash
python tools/inventory_images.py
# → docs/asset-inventory-images.md
```

## 9.7. Build Android

1. `File → Build Profiles → Android`.
2. Kiểm tra:
   - Package name, version code/name.
   - Minimum API level.
   - Scripting backend (IL2CPP khuyến nghị).
   - Keystore (không commit).
3. Build APK/AAB.
4. Cài lên thiết bị, chạy checklist ở mục 8.4.

> Repo chưa có build artifact; cần tự build và lưu log/ảnh làm bằng chứng.

## 9.8. Chạy test

```text
Window → General → Test Runner
  ├─ EditMode → Run All
  └─ PlayMode → Run All
```

Lưu ý: test chỉ được nhận nếu file nằm trong asmdef có `TestAssemblies`.

## 9.9. Troubleshooting

| Triệu chứng | Nguyên nhân thường gặp | Cách xử lý |
|---|---|---|
| Mở project báo upgrade | Sai Unity version | Cài đúng 6000.3.21f1, mở lại |
| Text vô hình | TMP `m_fontAsset` null | Gán font Lilita/GoogleSans trong prefab |
| Popup không hiện | Root inactive → `Awake` chưa chạy | Dùng `Acquire()` hoặc bật root trong scene |
| Bấm nút không ăn | Có object Image/Button đè lên, không listener | Kiểm tra hierarchy + raycast target |
| Booster sai loại | Enum bị đổi giá trị | Giữ `AddSlot=0, Clean=1, MagicBox=2, None=-1` |
| Toàn bộ game im | `AudioConfig` sai fileID | Dùng `8300000` (AudioClip), không phải `7400000` |
| Không load level | Catalog/order sai hoặc thiếu prefab | Chạy Validate Level Catalog, rebuild catalog |
| Ảnh không tô | Thiếu `LevelColorMatrix` hoặc màu không khớp | Chạy Level Studio Validate, auto-fix |
| Prefab mismatch | Stripped transform sai class | Sửa `!u!4` → `!u!224` nếu root là RectTransform |
| Test không xuất hiện | Thiếu asmdef TestAssemblies | Thêm asmdef cho thư mục test |

## 9.10. Checklist bàn giao

- [ ] Mở project bằng đúng Unity version, 0 compile error.
- [ ] 3 scene trong build settings.
- [ ] Chạy Entry → Home → Gameplay.
- [ ] 7 level load được.
- [ ] Win/lose/restart hoạt động.
- [ ] 3 booster hoạt động.
- [ ] Shop mua được, coin cập nhật.
- [ ] Daily reward claim + countdown + popup.
- [ ] Settings + audio.
- [ ] EditMode/PlayMode test chạy.
- [ ] Build APK + log.
- [ ] Worklog/evidence đầy đủ.

---

# Chương 10 — Kết luận và hướng phát triển

## 10.1. Kết quả đạt được

- Hoàn thiện vòng lặp puzzle từ Entry đến Gameplay, thắng/thua, tiến trình.
- Hệ thống tài nguyên/booster/shop/daily reward/settings hoạt động.
- Dữ liệu màn chơi có một nguồn chuẩn và validator chặn build.
- Bộ Editor tool giúp mở rộng nội dung nhanh (level, ảnh, reskin).
- Quy trình phát triển có AI với contract, task template, worklog và bằng chứng.
- Bộ tài liệu 4 tầng giúp người khác tiếp cận và tái lập.

## 10.2. Hạn chế

- Chưa build APK chính thức, chưa test thiết bị thật.
- Revive, tutorial, heart gate, level selection chưa hoàn thiện.
- Một số test/validator còn stale hoặc bị bỏ qua.
- Hai nguồn coin (`EconomyService` và `CoinManager`) chưa hợp nhất hoàn toàn.
- Localization và IAP/ads nằm ngoài phạm vi.

## 10.3. Hướng phát triển

1. Hoàn thiện revive + tutorial + heart gate.
2. Hợp nhất coin về một ledger, xoá stub `GameData`.
3. Thêm level selection thật (click ô màn).
4. Build Android + test matrix thiết bị + profiling.
5. Thêm analytics/remote config dạng adapter (không ảnh hưởng core).
6. Mở rộng pipeline level: nhiều chế độ Hard/VeryHard.
7. Bổ sung CI chạy EditMode test + validator trước merge.
8. Chuẩn hoá tài liệu: cập nhật các file `docs/features/` còn mang tính thiết kế.

---

# Phụ lục

## Phụ lục A — Bản đồ file quan trọng

| Nhóm | Đường dẫn |
|---|---|
| Đề cương | `K61 - Nguyen The Hoang - De cuong DATN.docx` |
| Kế hoạch | `tasks/plan.md`, `tasks/todo.md` |
| Worklog | `docs/weekly-worklog.md` |
| Hợp đồng AI | `ai-docs/README.md`, `ai-docs/agent-task-template.md`, `ai-docs/0*.md` |
| Game design | `gamedesign/README.md`, `gamedesign/0*.md` |
| Phân tích kỹ thuật | `docs/00-current-state.md`, `docs/0*.md`, `docs/10-redesign-architecture-and-roadmap.md` |
| Feature/evidence | `docs/features/`, `docs/evidence/` |
| Công cụ Python | `tools/inventory_images.py`, `tools/reskin.py`, `tools/extract_level_reference.py` |
| Công cụ Unity | `game/Assets/Editor/**` |
| Scene | `game/Assets/Scenes/{EntryScene,Home,Gameplay}.unity` |
| Service | `game/Assets/_Game/Scripts/Core/**` |
| Gameplay | `game/Assets/_Game/Scripts/Gameplay/**` |
| UI/Meta | `game/Assets/_Game/Scripts/Meta/**`, `UI/**` |
| Level data | `game/Assets/_Game/Scripts/LevelData/**`, `game/Assets/Resources/LevelCatalog.asset` |
| Test | `game/Assets/Tests/{EditMode,PlayMode}/**` |

## Phụ lục B — Menu và lệnh

### Menu Editor

```text
Tools/Puzzle/Level Studio
Tools/Puzzle/3. Quét decor gắn len (scene đang mở)
Tools/Puzzle/4. Quét decor tất cả Level prefab
Tools/Puzzle/Chuyển ThreadMat sang YarnDissolve
Tools/Puzzle/Quét trục mesh len
Tools/Puzzle/Chuyển len thành decor
Tools/Level/Build Level Catalog
Tools/Level/Validate Level Catalog
Tools/Resave Scenes
Tools/Reskin/Reskin hàng loạt
Tools/Color Usage Tracker
Tools/Texel Density Tool
Tools/Model Asset Exporter
WoolSort/Setup Audio
WoolSort/Daily Reward/1. Build Config + Prefabs
WoolSort/Daily Reward/2. Add Popup + Button to Home
```

### Python

```bash
python tools/inventory_images.py
python tools/reskin.py --src <folder> [--apply] [--mirror-booster] [--map map.csv]
python tools/extract_level_reference.py --src <GeneralConfig.asset>
```

### Unity batch (khi Editor đóng)

```bash
Unity.exe -batchmode -quit -projectPath game \
  -executeMethod _Game.EditorTools.LevelCatalog.LevelCatalogBuilder.BuildAndSave

Unity.exe -batchmode -quit -projectPath game \
  -executeMethod _Game.EditorTools.SceneResaver.ResaveAllScenes
```

## Phụ lục C — Prompt mẫu giao việc cho AI

### C.1. Task feature mới

```text
Feature: F0x — <tên>
Scope: <mô tả 1 hành vi quan sát được>
Not in scope: <liệt kê>
Read first: ai-docs/<contract>.md, docs/00-current-state.md, <source paths>
Goal: <hành vi>
Acceptance:
  [ ] ...
Implementation rules:
  - Không sửa ngoài phạm vi
  - Không thêm package/SDK mới
  - Giữ compile
Verification:
  - Compile 3 assembly
  - Chạy <test>
  - Báo file đã đổi + rủi ro chưa verify
```

### C.2. Fix bug

```text
Hiện tượng: <mô tả + log>
Kỳ vọng: <hành vi đúng>
Ràng buộc: không đổi UX ngoài phạm vi.
Yêu cầu: tìm nguyên nhân gốc, sửa tối thiểu, verify, ghi worklog.
```

### C.3. Review

```text
Đọc <file/scene/prefab> và kiểm tra:
1. Reference fileID/GUID có resolve?
2. Event/listener có được đăng ký đúng?
3. State có bị che bởi overlay/raycast?
4. Có race condition với animation?
Trả về: findings + mức độ + đề xuất.
```

## Phụ lục D — Thuật ngữ

| Thuật ngữ | Nghĩa |
|---|---|
| Vertical slice | Lát cắt chạy được end-to-end của một feature |
| Contract | Hợp đồng yêu cầu/acceptance cho AI |
| Evidence | Bằng chứng kiểm chứng (log/ảnh/test) |
| Build gate | Hook chặn build khi dữ liệu sai |
| Dependency closure | Tập asset/prefab mà một prefab/scene cần |
| Reskin | Thay nội dung ảnh nhưng giữ GUID/meta |
| Asmdef | Assembly Definition của Unity |
| TMP | TextMeshPro |
| GUID | ID duy nhất của asset trong Unity |

## Phụ lục E — Trạng thái phiên bản

- Branch phát triển: `tesst-toool`.
- Commit gần nhất tại thời điểm viết: `feat(daily-reward): 3-state day cards, claim popup, coin fly, countdown`.
- Repo: `git@github.com:nguyenthehoang98/wool-sort.git`.
- Unity target: `6000.3.21f1`.

---

*Hết báo cáo.*
