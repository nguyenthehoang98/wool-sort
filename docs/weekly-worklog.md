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
| W3D1 | Entry load config/save và route level 1/later levels | Pass cả hai nhánh, kể cả service fail |  | TODO |  |  |
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
| W5D3 | Win/lose/restart/reward | Win/lose chỉ emit một lần |  | TODO |  |  |
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

## Tổng kết sau mỗi tuần

| Tuần | Kết quả đạt được | Task chưa xong | Blocker | Evidence chính | Quyết định tuần sau |
|---|---|---|---|---|---|
| 1 | Scope, state, command, core resolver và acceptance contract đã chốt | Chưa có Application/Unity scene integration | Không có blocker cho pure core; W4-01 tiếp tục mở rộng/integrate | `docs/week1-core-contract.md`, `docs/features/F01-core-puzzle.md`, `docs/evidence/w1-editmode-tests.md` | Bắt đầu W2 inventory/data; giữ Core thuần C# và reskin qua ThemeProfile. |
| 2 |  |  |  |  |  |
| 3 |  |  |  |  |  |
| 4 |  |  |  |  |  |
| 5 |  |  |  |  |  |
| 6 | Booster playable ở bản core: test AddSlot/Clean/MagicBox, panel debug, đấu dây scene | W6-01..03 còn chờ chạy Test Runner + Play scene | Thiếu source gốc (GameData/SettingManager/shop) nên dùng stub | `docs/evidence/w6-booster-tests.md`, `booster-html.html` | Giữ stub tới khi có source gốc; chạy test rồi mới đóng W6 |
| 7 |  |  |  |  |  |
| 8 |  |  |  |  |  |
| 9 |  |  |  |  |  |
