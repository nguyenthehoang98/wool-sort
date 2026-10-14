# AI backlog — Wool Sort Master

> Quyết định mới: dựng lại reference project trực tiếp, không phát triển implementation tối giản. Đồng bộ `Assets/`, `Packages/`, `ProjectSettings/` từ `C:/Users/Admin/Documents/Projects/wool-sort` và dùng Unity `2022.3.50f1`.

## Clone reference project

- [x] **CLONE-01 — Đồng bộ Assets** — Đã mirror toàn bộ Assets và meta từ reference vào `game/Assets/`. Verify: cả hai bên 9,881 files / 1,214.11 MB.
- [x] **CLONE-02 — Đồng bộ project setup** — Đã mirror Packages và ProjectSettings từ reference. Verify: Unity `2022.3.50f1`, package lock và build scenes Entry/Home/Gameplay khớp.
- [x] **CLONE-03 — Compile/import checkpoint** — Đã import bằng Unity `2022.3.50f1`; Android target script compilation pass, không có CS compile error. Windows player không phải target của reference và thiếu mediation factory Standalone.
- [~] **CLONE-04 — Gameplay checkpoint** — Ba scene reference mở được, không có missing component; còn manual click-through level gameplay.

Đây là backlog thực thi theo thứ tự phụ thuộc. Không đánh dấu `[x]` chỉ vì đã copy; phải có verification.

## Quy ước

- `[ ]` chưa làm · `[~]` đang làm · `[x]` đã verify · `[!]` blocked.
- Một task không được bao gồm nhiều feature độc lập.
- Reference project là source của bản clone; không sửa trực tiếp reference.
- Target nằm tại `game/`; mọi thay đổi clone/setup thực hiện trong target.

## Bootstrap

- [x] **B0 — Chốt hướng clone reference** — Target là `game/`; reference `wool-sort` là nguồn copy; dùng Unity `2022.3.50f1` theo reference. **Deps:** none.

## Slice tiếp theo — Reference visual vertical slice

- [x] **VIS-01 — Import asset Level3 có chọn lọc** — Copy model `Level3.fbx`, target image `Image3.png` và visual dependencies cần thiết; không copy code reference. **Verify:** asset import được trên Unity 6.
- [~] **VIS-02 — Real yarn model interaction** — Dùng hierarchy model thật cho piece, collider click và bay tới slot/box. **Verify:** click piece nhìn thấy model bay đúng target.
- [~] **VIS-03 — Board image painting** — Hiển thị target image và tô/mở vùng ảnh theo event box close. **Verify:** ảnh chỉ đổi sau khi box đóng.
- [ ] **VIS-04 — Reference visual checkpoint** — Play Level3 từ click đến win/lose với model/material/image thật. **Verify:** compile, tests, Windows build và manual Play.

## Tuần 1 — Scope và luật core

- [x] **W1-01 — Chốt product scope** — W1D1. Tạo scope in/out và danh sách acceptance chính từ DOCX. **Verify:** đối chiếu `gamedesign/09-product-acceptance.md`; evidence: `docs/week1-core-contract.md`.
- [x] **W1-02 — Mô hình state puzzle** — W1D2. Chốt piece/layer/box/waiting/session state và invariant. **Verify:** mọi trạng thái có transition hợp lệ; evidence: `docs/week1-core-contract.md`.
- [x] **W1-03 — Command và outcome rules** — W1D3. Chốt SelectPiece, FillSlot, ResolveBox, Win/Lose và input gate. **Verify:** wrong color, full box, waiting full có rule rõ; evidence: `docs/week1-core-contract.md`.
- [x] **W1-04 — Core contract/test cases** — W1D4–D5. Hoàn thiện contract và test matrix resolver. **Verify:** map được sang `ai-docs/10-acceptance-tests.md`; evidence: `docs/week1-core-contract.md`, `docs/features/F01-core-puzzle.md`. **Deps:** W1-01..03.

## Tuần 2 — Unity baseline và dữ liệu

- [ ] **W2-01 — Scene/prefab/resource inventory** — W2D2. Lập bản đồ Entry/Home/Gameplay, prefab level, config, mesh và asset backup. **Verify:** không còn thư mục/asset quan trọng không rõ nguồn.
- [ ] **W2-02 — Package/platform matrix** — W2D3. Ghi package, version, Android target, TextMeshPro, IAP, ads, localization, input. **Verify:** mỗi package có owner và lý do dùng.
- [ ] **W2-03 — LevelDefinition contract** — W2D4. Chốt level ID/order/seed/pieces/box/waiting/tutorial/thumbnail. **Verify:** order và asset ID không bị dùng lẫn.
- [ ] **W2-04 — Baseline build và validator plan** — W2D5. Chốt build scene order, validator checks và smoke command. **Verify:** project mở/build được hoặc ghi blocker cụ thể. **Deps:** B0, W2-01..03.

## Tuần 3 — Navigation, progression và save

- [ ] **W3-01 — Entry routing** — W3D1. Load config/save và route level 1 vào Gameplay, level sau vào Home. **Verify:** test cả hai nhánh và SDK fail.
- [ ] **W3-02 — Home và level selection intents** — W3D2. Implement/đặc tả Play, chọn level, Settings, Shop, Daily Reward. **Verify:** modal không cho input xuyên xuống board.
- [ ] **W3-03 — Progression/economy facade** — W3D3. Gom coin, heart, infinite heart, booster inventory và completed level vào service. **Verify:** balance không âm, retry không cộng hai lần.
- [ ] **W3-04 — Save/load migration** — W3D4. Version schema, load/save atomic và migration từ `new_game_data`/PlayerPrefs cũ. **Verify:** save/reload giữ progress và resource.
- [ ] **W3-05 — Navigation smoke checkpoint** — W3D5. Chạy Entry → Home/Gameplay → back/reload. **Verify:** scene không giữ singleton/listener cũ. **Deps:** W3-01..04.

## Tuần 4–5 — Core gameplay và level content

- [~] **W4-01 — Pure resolver** — W4D1. Pure resolver prework đã làm sớm trong W1 tại `game/Assets/_Game/Runtime/Core`; 16 EditMode tests pass. Còn mở rộng lifecycle đầy đủ và nối Application adapter. **Verify:** unit tests cho placement, box selection, no-op và outcome.
- [ ] **W4-02 — Level mapper/validator** — W4D2. Map prefab/config hiện tại sang `LevelDefinition` và báo lỗi thiếu/duplicate asset. **Verify:** level 1 map đúng real asset 3.
- [ ] **W4-03 — Gameplay session adapter** — W4D3. Chuyển domain events thành view commands cho board/box/slot/waiting. **Verify:** animation không tự quyết định state.
- [ ] **W4-04 — Layer spawn và interaction** — W4D4. Xử lý piece ngoài cùng, inner layer, tap/hold/rotate/scale. **Verify:** piece không placement hai lần.
- [ ] **W4-05 — Level 1 vertical slice** — W4D5. Chạy level đầu từ load đến win/lose. **Verify:** log input → event → outcome hoàn chỉnh.
- [ ] **W5-01 — Content pipeline** — W5D1. Validator kiểm tra màu, layer, box, order, thumbnail, backup reference. **Verify:** lỗi hiển thị trước build.
- [ ] **W5-02 — Campaign/difficulty data** — W5D2. Cấu hình Normal/Hard/VeryHard và thứ tự campaign. **Verify:** thêm level không sửa core.
- [~] **W5-03 — Win/lose/restart** — W5D3. Win/lose UI scaling animation added, popups hidden on level start. Verify: win/lose emit once, animation behavior correct.
- [ ] **W5-04 — Deterministic replay** — W5D4. Seed + command log tái hiện cùng event log. **Verify:** cùng input cho cùng kết quả.
- [ ] **W5-05 — Three-level checkpoint** — W5D5. Chạy ít nhất 3 level đại diện. **Verify:** regression core pass; ghi bug còn lại. **Deps:** W4-01..05, W5-01..04.

## Tuần 6 — Booster, tutorial và presentation

- [~] **W6-01 — Add Slot** — W6D1. Validate cap, cost/inventory và layout. **Verify:** không vượt tối đa 7 slot. Nút `BoosterBtn_1` nằm sẵn trong `Gameplay.unity` (unlock lv1 mặc định); test `AddSlot_GrowsByOneUntilCap` + `AddSlot_NeverExceedsMax`; chờ Play verify, evidence: `docs/evidence/w6-booster-tests.md`.
- [~] **W6-02 — Clean** — W6D2. Chuyển waiting pieces sang temporary storage không mất piece. **Verify:** ad/coin fail không mutate. Đã có test `Clean_AvailabilityMatchesWaitingState` + UI thật thay `BoosterDebugPanel` (panel cũ mặc định tắt); chờ chạy Play scene, evidence: `docs/evidence/w6-booster-tests.md`.
- [~] **W6-03 — Magic Box và revive** — W6D3. Chọn đúng active box, revive giữ nguyên board. **Verify:** double tap chỉ tạo một operation. Đã thêm `GameManager.ChooseNextBoxColorRevive`; popup mua `MoreBoosterPopupUI.cs` (fix CS0103 `MakeImage` 2026-10-13, fix NRE `KitTooltipManager` null + tự load sprite `Resources/BoosterUI/` 2026-10-13, chờ Play verify); test tay bằng UI thật, evidence: `docs/evidence/w6-booster-tests.md`.
- [ ] **W6-04 — Tutorial data/runtime** — W6D4. Data-driven steps, flag version, command gate và skip. **Verify:** tutorial không lặp sau reload.
- [ ] **W6-05 — Win/lose UI, FX, audio, settings** — W6D5. Nối view/animation/audio/vibration/localization key. **Verify:** toggle có hiệu lực và popup khóa gameplay. **Deps:** W6-01..04.

## Tuần 7 — Monetization và platform adapters

- [ ] **W7-01 — Adapter boundaries** — W7D1. Tách Ads, Purchase, Analytics, Localization, Audio khỏi core. **Verify:** core chạy với null/no-op services.
- [ ] **W7-02 — Rewarded ad flow** — W7D2. Implement request/loading/success/fail/cancel cho revive và reward. **Verify:** chỉ grant sau callback thành công.
- [ ] **W7-03 — IAP catalog/transaction/restore** — W7D3. Catalog product ID, verified purchase, restore No Ads và pending state. **Verify:** transaction ID idempotent.
- [ ] **W7-04 — Shop và daily reward integration** — W7D4. Kết nối UI với economy facade, localized price và reward. **Verify:** purchase/ad lỗi không làm crash core.
- [ ] **W7-05 — Monetization failure checkpoint** — W7D5. Chạy test success/fail/retry/offline/restore. **Verify:** không mất hoặc nhân đôi tài nguyên. **Deps:** W7-01..04.

## Tuần 8 — QA, thiết bị và hiệu năng

- [ ] **W8-01 — Automated acceptance suite** — W8D1. Chạy resolver, level, economy, booster và UI contract tests. **Verify:** kết quả lưu theo test ID.
- [ ] **W8-02 — Regression/relaunch/pause** — W8D2. Test scene reload, pause/resume, kill/relaunch, save/reward. **Verify:** không stale singleton và không duplicate reward.
- [ ] **W8-03 — Android device matrix** — W8D3. Test thiết bị/tỷ lệ màn hình dọc, safe area, touch và permissions. **Verify:** có model, OS, build, result.
- [ ] **W8-04 — Performance profiling** — W8D4. Đo startup, scene load, FPS, memory và animation stall. **Verify:** ghi baseline và ngưỡng chấp nhận.
- [ ] **W8-05 — Bug-fix/retest checkpoint** — W8D5. Sửa P0/P1 và chạy lại affected tests. **Verify:** bug log có reproduction và evidence. **Deps:** W8-01..04.

## Tuần 9 — Build, báo cáo và bàn giao

- [ ] **W9-01 — Android build configuration** — W9D1. Chốt version, package ID, orientation, scenes, icons, signing reference và build reproducibility. **Verify:** không commit secret/keystore.
- [ ] **W9-02 — Release smoke test** — W9D2. Build APK/AAB và chạy Entry → Home/Gameplay → Win/Lose. **Verify:** lưu artifact hash hoặc build log.
- [ ] **W9-03 — Report/evidence pack** — W9D3. Gom kiến trúc, gameplay, UI, test result, ảnh màn hình và hạn chế. **Verify:** mỗi mục trong DOCX có dẫn chứng.
- [ ] **W9-04 — Hướng dẫn và bảo vệ** — W9D4. Viết cài đặt/sử dụng, demo script, slide/checklist hỏi đáp. **Verify:** người khác làm theo được.
- [ ] **W9-05 — Final handover** — W9D5. Đóng các task đã verify, ghi known limitations và rollback/build instructions. **Verify:** reviewer có thể tái hiện build và test. **Deps:** W9-01..04.

## Blocked / quyết định cần người dùng

- [ ] Chốt Android device matrix và package/platform matrix trước W2D3. `C:/Users/Admin/Documents/Projects/wool-sort` chỉ là reference, không phải nơi AI được phép sửa.
- [!] Xác nhận 5 ngày/tuần có đúng lịch làm việc không; nếu không, chỉ cần đổi cột ngày, không đổi dependency.
- [!] Chốt target Android OS/device matrix và Unity/package version chính thức.
