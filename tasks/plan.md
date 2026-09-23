# Kế hoạch triển khai Wool Sort Master — clone reference project

> Quyết định mới: đây là bài dựng lại project `wool-sort`, không phải phát triển một implementation tối giản mới. Target sẽ dùng trực tiếp Assets/Packages/ProjectSettings của reference và Unity `2022.3.50f1`.

## Mục tiêu

Chuyển đề cương trong `K61 - Nguyen The Hoang - De cuong DATN.docx` thành kế hoạch có thể giao cho AI: mỗi tuần có kết quả bàn giao, mỗi ngày có một việc kiểm chứng được, và mỗi việc có acceptance criteria cùng bằng chứng cần lưu.

## Giả định lịch

- 9 tuần, 5 ngày làm việc/tuần, tổng cộng 45 ngày.
- Target sẽ khớp reference `wool-sort`: Unity `2022.3.50f1`, scene/build settings/package/assets tương ứng.
- Ngày thực tế được điền khi bắt đầu; không hard-code ngày lịch để có thể dời kế hoạch.
- Mỗi ngày chỉ hoàn thành một slice nhỏ; cuối mỗi tuần có checkpoint.
- `docs/` mô tả phân tích kỹ thuật, `gamedesign/` mô tả sản phẩm, `ai-docs/` là contract cho AI.
- Reference Unity project nằm tại `C:/Users/Admin/Documents/Projects/wool-sort`; đây là nguồn để copy trực tiếp project setup, asset và gameplay code.
- Target Unity project đã được xác nhận tại `puzzle-game/game`; code/scene/build dùng Unity `6000.3.21f1` ở target này.

## Thứ tự nguồn và phạm vi

1. Target Unity project và acceptance contract của task.
2. Reference project tại `C:/Users/Admin/Documents/Projects/wool-sort` để xác minh hành vi/asset/data/package hiện có.
3. `docs/00-current-state.md` và các technical docs.
4. `gamedesign/`.
5. File Word dùng làm bối cảnh và kế hoạch học vụ.

## Quy tắc dùng reference

- Ưu tiên copy trực tiếp `Assets`, `Packages`, `ProjectSettings` và code của reference.
- Không tối giản hoặc thay resolver riêng nếu mục tiêu là tái dựng reference.
- Chỉ sửa path/project identity hoặc lỗi môi trường cần thiết để target mở và chạy được.
- Mọi khác biệt giữa reference và thiết kế mới phải ghi vào worklog/backlog, không sửa reference để làm cho khớp.

Không làm trong MVP: backend riêng, multiplayer, account/cloud save bắt buộc, battle pass, ranking online, marketplace, Addressables hoặc framework DI mới.

## Kết quả theo tuần

| Tuần | Kết quả phải bàn giao | Checkpoint |
|---|---|---|
| 1 | Chốt scope, luật puzzle, state/command/event và acceptance core | Rule không còn mơ hồ; có test plan |
| 2 | Baseline Unity, bản đồ scene/prefab/resource/package và level data contract | Mở được project, xác nhận build scenes |
| 3 | Entry → Home → chọn level → Gameplay, progression và save facade | Smoke test chuyển scene và load/save |
| 4 | Core resolver, level mapper, board/box/slot/waiting vertical slice | Level đầu xử lý được đúng/sai |
| 5 | Level pipeline, validator, win/lose, deterministic replay và gameplay slice | 3 level mẫu pass regression |
| 6 | Booster, revive, tutorial, UI kết quả, FX/audio/settings | Không mất state khi animation/ad fail |
| 7 | Ads/IAP/shop/platform adapters và giao dịch idempotent | Test success/failure/restore pass |
| 8 | Regression, Android device test, profiling và sửa lỗi | Không còn blocker P0/P1 |
| 9 | Android build, tài liệu, minh chứng và bàn giao bảo vệ | Build và hồ sơ có thể tái hiện |

## Phụ thuộc chính

```text
W1 rules/spec
  -> B0 reference/target baseline
  -> W2 level/config baseline
  -> W3 navigation/save
  -> W4 core gameplay slice
  -> W5 content + outcome
  -> W6 booster/tutorial/polish
  -> W7 platform services
  -> W8 QA/device/performance
  -> W9 build/report/defense
```

W1 là phần phân tích tài liệu nên có thể làm trước khi có source. B0 phải pass trước các task inventory, code, scene và build ở W2 trở đi.

Không tích hợp IAP/ads trước khi core gameplay chạy độc lập. Không polish toàn bộ scene trước khi resolver và level data có test.

## Definition of Done

- Task có acceptance criteria và bằng chứng trong worklog.
- Core gameplay chạy được khi ads/IAP/analytics/localization lỗi.
- Unit/acceptance tests liên quan pass; scene smoke test được ghi lại.
- Không còn lỗi P0/P1 chưa có quyết định xử lý.
- Android build dùng Unity `6000.3.21f1`; `ProjectSettings/ProjectVersion.txt` của target phải khớp version này.
- `docs/weekly-worklog.md` có ngày thực tế, trạng thái, evidence và ghi chú cho từng ngày.

## Phase hiện tại: clone project setup và gameplay reference

1. Đồng bộ toàn bộ `Assets/` từ reference.
2. Đồng bộ `Packages/` và `ProjectSettings/` để scene/build/package khớp reference.
3. Mở bằng Unity `2022.3.50f1`, resolve import/compile errors.
4. Verify scene gameplay của reference trước khi chỉnh sửa thêm.

## Slice tiếp theo: reference visual vertical slice

Mục tiêu gần: thay placeholder bằng một vertical slice nhìn gần reference nhưng không bê nguyên C# reference.

1. Import có chọn lọc asset visual Level3: `Level3.fbx`, `Image3.png` và material/shader/texture dependency cần thiết.
2. Gắn visual root của FBX vào level data snapshot; không dùng component script của project reference.
3. Thay piece placeholder bằng model yarn thật, thêm collider click và animation bay tới slot/box.
4. Thêm board image runtime: hiển thị `Image3.png`, tô pixel theo event `BoxClosed` bằng code target.
5. Verify Unity import/compile, EditMode tests, Windows build và manual Play.

Giới hạn slice: chỉ Level3, box/rope gameplay core; bỏ UI/booster/ads/IAP và không import toàn bộ project reference.

## Rủi ro và cách xử lý

| Rủi ro | Mức | Cách xử lý |
|---|---|---|
| Target và reference khác Unity version | Trung bình | Dùng `wool-sort` để kiểm tra hành vi, re-implement theo Unity `6000.3.21f1` và ghi khác biệt |
| API/package khác giữa Unity 6 và reference 2022 | Trung bình | Pin Unity `6000.3.21f1`; kiểm tra API bằng tài liệu Unity 6 trước mỗi feature |
| Level prefab/config không đồng bộ | Cao | Validator trước build; không tự sửa im lặng |
| Animation/SDK làm kẹt gameplay | Cao | State machine và adapter; test timeout/failure/offline |
| Scope tăng do backend/AB test/Addressables | Trung bình | Giữ ngoài MVP, chỉ mở task khi có yêu cầu mới |
| Thiếu bằng chứng tuần 8–9 | Cao | Mỗi test/build bắt buộc lưu log, ảnh hoặc artifact ngay trong ngày |
