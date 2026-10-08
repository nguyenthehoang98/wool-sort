# BÁO CÁO TIẾN ĐỘ DỰ ÁN WOOL SORT — Tháng 10/2026

> Người lập: team dev · Ngày báo cáo: 08/10/2026
> Nguồn đối chiếu: `docs/`, `ai-docs/`, `gamedesign/`, `tasks/todo.md`, `tasks/plan.md` (kế hoạch 9 tuần), code thực tế tại `game/`, lịch sử git.

## 1. Tóm tắt điều hành (cho sếp đọc 2 phút)

* **Game đã chơi được trọn vòng lặp core trên 1 scene:** vào level → tap len → vào box/waiting → mở lớp trong → full box tô ảnh 84x84 → thắng/thua, chơi lại. **7 level thật (Level1–Level7)** đã dựng xong content + ma trận ảnh.
* **Điểm mạnh nhất kỳ này: pipeline làm level.** Bộ công cụ Level Studio (5 tab) + validator tự quét + 4 nút tự sửa lỗi (`%3`, pool random, vá màu thiếu) + extract/import JSON ma trận ảnh. Nhờ đó dựng Level3→Level7 chỉ trong 2 ngày (07–08/10/2026, xem git log).
* **Phần còn thiếu toàn là hệ thống ngoài core:** màn hình Entry/Home, lưu game (save), tiền/tim (coin/heart), booster/revive, tutorial chạy thật, quảng cáo/IAP, đa ngôn ngữ/âm thanh/cài đặt, build Android. Tức là **có game chơi được, chưa có sản phẩm phát hành được**.
* **Mức hoàn thành ước tính ~35%** so với kế hoạch 9 tuần (chi tiết ở mục 3).

## 2. Số liệu nhanh (đã xác minh)

| Hạng mục | Số liệu | Bằng chứng |
|---|---|---|
| Unity version | 6000.3.21f1 | `game/ProjectSettings/ProjectVersion.txt` |
| Level chơi được | 7 (Level1–Level7) | `game/Assets/Resources/Levels/` (7 prefab) |
| Ma trận ảnh level | 7 file, lưới 84x84 | `game/Assets/Resources/LevelColorMatrix/` |
| Scene | 1 (`Gameplay.unity`), chưa có Entry/Home | `game/Assets/Scenes/` |
| Waiting slot | khởi đầu 5, tối đa 7 | `GameConfig.cs:9-10` |
| Test tự động | 1 PlayMode test (popup thắng/thua) | `game/Assets/Tests/PlayMode/` |
| Tool Editor | ~13 tool, gom trong Level Studio 5 tab | `game/Assets/Editor/` |
| Nhịp làm việc gần nhất | Level3→Level7 + fix tool trong 07–08/10/2026 | `git log` (15 commit) |
| File đang sửa dở (chưa commit) | `Level6.prefab` | `git status` |

## 3. Đối chiếu kế hoạch 9 tuần (`tasks/plan.md`, `tasks/todo.md`, `docs/weekly-worklog.md`)

| Tuần | Nội dung | Trạng thái |
|---|---|---|
| B0 + Clone (CLONE-01→04) | Mirror Assets/Packages/ProjectSettings, mở project, compile | **Xong** (CLONE-04 còn click-through thủ công) |
| W1 scope + luật core | Contract, state machine, 16 EditMode tests (theo worklog) | **Xong trên giấy** |
| W2 baseline + dữ liệu | Inventory scene/prefab/package, LevelDefinition | **Một phần** (validator thay thế một phần) |
| W3 navigation/save | Entry/Home, progression, save/migration | **Chưa làm** |
| W4–W5 core + content | Resolver thuần, deterministic replay, regression 3 level | **Một phần** (core chạy bằng code gameplay trực tiếp, chưa tách resolver thuần; replay chưa deterministic) |
| W6 booster/tutorial/polish | AddSlot/Clean/MagicBox/revive, tutorial runtime, FX/audio/settings | **Chưa làm** (mới có marker/stub) |
| W7 monetization/platform | Ads/IAP/shop/analytics | **Chưa làm** |
| W8 QA/device/performance | Test suite, máy thật, profiling | **Chưa làm** |
| W9 build/bàn giao | APK/AAB, hồ sơ | **Bắt đầu** (báo cáo này là một phần W9-03) |

Chi tiết tính năng đã có xem file `02-tinh-nang-da-hoan-thanh-chi-tiet.md`.
Chi tiết phần thiếu + việc tiếp theo xem `03-con-thieu-theo-tai-lieu.md` và `04-ke-hoach-tiep-theo.md`.

## 4. Rủi ro cần sếp biết sớm

1. **Replay không deterministic:** màu lớp inner random bốc bằng `Random.Range` mỗi lần chơi (`GameManager.cs:669-672`) — cùng level chơi lại ra layout khác. Tài liệu yêu cầu seed + replay (`W5-04`). Không chặn chơi, nhưng chặn QA tự động.
2. **Thư mục `Resources/Images/` đã dỡ bỏ:** game chạy hoàn toàn bằng ma trận màu (đúng chủ trương tool), nhưng level nào thiếu file `.asset` ma trận sẽ lỗi tô ảnh. Validator đã bắt được case này.
3. **Không có save:** tắt game là mất hết tiến trình (mới chỉ lưu RAM + `debugStartLevel`). Đây là việc chặn phát hành, nên ưu tiên ngay sau báo cáo này.
4. **`Level6.prefab` đang sửa dở, chưa commit** — cần commit hoặc hoàn tất trước khi giao build nội bộ.
