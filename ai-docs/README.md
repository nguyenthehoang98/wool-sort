# AI contract — Wool Sort Master redesign

## Role

Bạn là coding agent triển khai redesign Unity/C#. Đọc các file trong thư mục này trước khi sửa code. Các file là contract; không coi chúng là gợi ý.

## Evidence priority

1. Target project và acceptance contract của task.
2. Reference/asset project tại `C:/Users/Admin/Documents/Projects/wool-sort` để xác minh hành vi hiện tại.
3. `docs/00-current-state.md` và các feature docs.
4. File Word chỉ là bối cảnh, không phải technical source of truth.

Reference chỉ để tham khảo và re-implement theo contract; không copy nguyên code hoặc kiến trúc lỗi sang target.

Nếu phát hiện mâu thuẫn: dừng phần implementation liên quan, ghi rõ mâu thuẫn, không đoán silent.

## Hard constraints

- Không sửa `C:/Users/Admin/Documents/Projects/wool-sort` trong task tạo tài liệu hoặc triển khai reference mapping.
- Không commit/copy secret, keystore, license key, receipt hoặc service config vào docs.
- Không thêm backend, multiplayer, DI framework, event bus mới, Addressables hoặc hệ thống AB test nếu task không yêu cầu.
- Core gameplay phải chạy khi ad/IAP/analytics/localization service lỗi.
- Mọi resource mutation phải idempotent.
- Mọi command gameplay phải được validate trước khi mutate state.

## Feature index

| ID | Contract |
|---|---|
| F01 | `01-core-gameplay.md` |
| F02 | `02-level-data.md` |
| F03 | `03-progression-state.md` |
| F04 | `04-boosters-contract.md` |
| F05 | `05-ui-flow-contract.md` |
| F06 | `06-tutorial-contract.md` |
| F07 | `07-monetization-contract.md` |
| F08/F09 | `08-platform-services-contract.md` |
| ARCH | `09-technical-migration-plan.md` |
| QA | `10-acceptance-tests.md` |

## Implementation protocol

Thực thi theo backlog [`../tasks/todo.md`](../tasks/todo.md) và ghi kết quả từng ngày vào [`../docs/weekly-worklog.md`](../docs/weekly-worklog.md). Không đánh dấu task hoàn thành nếu chưa có acceptance/evidence.

1. Đọc contract feature và source caller/callee liên quan.
2. Viết/đổi test nhỏ nhất cho rule.
3. Implement một vertical slice, giữ project compile được.
4. Chạy test/build phù hợp; nếu Unity Editor không khả dụng, ghi verification còn thiếu.
5. Không refactor ngoài feature đang làm.
6. Cập nhật docs nếu contract thay đổi.

## Done criteria

Feature chỉ được coi là xong khi acceptance criteria trong `10-acceptance-tests.md` pass và không còn dependency trực tiếp từ core tới SDK/UI.
