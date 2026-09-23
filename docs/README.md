# Wool Sort Master — tài liệu thiết kế lại

Bộ tài liệu này được tạo từ việc đọc reference project tại `C:/Users/Admin/Documents/Projects/wool-sort` và dùng để thiết kế lại game bằng AI. Reference chỉ dùng để tham khảo hành vi/asset/data; không copy nguyên code.

## Cách đọc

- `gamedesign/`: tài liệu thiết kế game dành cho product/design/QA, có bản DOCX và hình ảnh.
- `docs/`: bản phân tích và thiết kế hệ thống dành cho người đọc kỹ thuật.
- `ai-docs/`: hợp đồng dành cho AI — dữ kiện có thể kiểm chứng, state machine, interface, acceptance criteria và thứ tự triển khai.
- `features/`: tóm tắt từng feature sau khi làm: luồng xử lý, file/class, placeholder/reskin và bằng chứng QA.
- Đọc [`00-current-state.md`](00-current-state.md) trước để hiểu giới hạn của bản hiện tại.
- Khi triển khai, dùng [`../ai-docs/README.md`](../ai-docs/README.md) làm luật ưu tiên; backlog ở [`../tasks/todo.md`](../tasks/todo.md), worklog theo ngày ở [`weekly-worklog.md`](weekly-worklog.md); không suy diễn tính năng không có trong tài liệu.

## Phạm vi redesign

Nếu chỉ cần hiểu game và duyệt tính năng, đọc `../gamedesign/Wool-Sort-Game-Design-Document.docx`. Nếu cần triển khai, đọc tiếp `docs/` và `../ai-docs/`.

Giữ lại trải nghiệm cốt lõi: người chơi chạm vào các lớp len ngoài cùng, đưa len vào hộp cùng màu hoặc vùng chờ, mở dần lớp bên trong và hoàn thành màn. Thiết kế lại phần logic theo state rõ ràng, tách khỏi UI/animation/SDK và vẫn dùng Unity/C#.

Không nằm trong phạm vi mặc định: backend riêng, multiplayer, xếp hạng online, hệ thống tài khoản. Chỉ thêm khi có yêu cầu sản phẩm cụ thể.

## Tài liệu theo tính năng

| Mã | Tính năng | Tài liệu người đọc | Hợp đồng AI |
|---|---|---|---|
| F01 | Vòng lặp phân loại len | [01-core-gameplay](01-core-gameplay.md) | [AI contract](../ai-docs/01-core-gameplay.md) |
| F02 | Dữ liệu màn chơi | [02-level-content](02-level-content.md) | [AI contract](../ai-docs/02-level-data.md) |
| F03 | Tiến trình và tài nguyên | [03-progression-resources](03-progression-resources.md) | [AI contract](../ai-docs/03-progression-state.md) |
| F04 | Booster và hồi sinh | [04-boosters-and-revive](04-boosters-and-revive.md) | [AI contract](../ai-docs/04-boosters-contract.md) |
| F05 | Home, chọn màn, HUD | [05-ui-navigation](05-ui-navigation.md) | [AI contract](../ai-docs/05-ui-flow-contract.md) |
| F06 | Tutorial | [06-tutorial](06-tutorial.md) | [AI contract](../ai-docs/06-tutorial-contract.md) |
| F07 | IAP và quảng cáo | [07-monetization](07-monetization.md) | [AI contract](../ai-docs/07-monetization-contract.md) |
| F08 | Ngôn ngữ, âm thanh, rung | [08-localization-audio-settings](08-localization-audio-settings.md) | [AI contract](../ai-docs/08-platform-services-contract.md) |
| F09 | Analytics và remote config | [09-analytics-remote-config](09-analytics-remote-config.md) | [AI contract](../ai-docs/08-platform-services-contract.md) |
| ARCH | Kiến trúc và lộ trình | [10-redesign-architecture-and-roadmap](10-redesign-architecture-and-roadmap.md) | [AI contract](../ai-docs/09-technical-migration-plan.md) |
| QA | Kiểm thử và rủi ro | [11-qa-risk-register](11-qa-risk-register.md) | [AI contract](../ai-docs/10-acceptance-tests.md) |

## Ghi chú về nguồn

Đây là tài liệu phân tích snapshot, không phải cam kết rằng mọi asset đều đang được dùng runtime. Khi target project, reference scene và tài liệu Word mâu thuẫn, ưu tiên target/contract của task; dùng reference để xác minh hành vi hiện tại và đánh dấu rõ điểm khác biệt. Không sửa reference project trong quá trình làm tài liệu hoặc triển khai.
