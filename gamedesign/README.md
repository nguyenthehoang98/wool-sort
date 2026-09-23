# Wool Sort Master — Game Design Document

Đây là thư mục **Game Design**, không phải tài liệu kỹ thuật cho dev.

Mục tiêu là mô tả game theo góc nhìn sản phẩm: người chơi làm gì, màn hình nào cần có, cảm giác mong muốn, luật tính năng, phần thưởng, điều kiện thắng/thua và tiêu chí chấp nhận.

## File chính

- `Wool-Sort-Game-Design-Document.docx`: bản Word có hình ảnh tham khảo từ project hiện tại.
- `Wool-Sort-Game-Design-Document.md`: bản text dễ diff/cập nhật bằng Git hoặc AI.
- `assets/screenshots/`: screenshot hiện có được trích từ reference `C:/Users/Admin/Documents/Projects/wool-sort/Assets/_Game/Previews`.

## Các feature

| ID | Feature | File |
|---|---|---|
| F01 | Tầm nhìn và người chơi | `00-product-vision.md` |
| F02 | Core loop phân loại len | `01-core-loop.md` |
| F03 | Thiết kế level và độ khó | `02-level-design.md` |
| F04 | Progression, coin, heart | `03-progression-economy.md` |
| F05 | Booster và revive | `04-boosters-revive.md` |
| F06 | UI/UX và các màn hình | `05-ui-ux-screens.md` |
| F07 | Tutorial/onboarding | `06-tutorial-onboarding.md` |
| F08 | Ads, IAP và shop | `07-monetization.md` |
| F09 | Art, animation, audio, localization | `08-art-audio-localization.md` |
| F10 | Quy tắc sản phẩm và tiêu chí nghiệm thu | `09-product-acceptance.md` |

## Quy ước đọc tài liệu

- Nội dung `Hiện tại` mô tả những gì quan sát được từ project và screenshot.
- Nội dung `Thiết kế đề xuất` là hướng redesign, cần được product duyệt trước khi code.
- Nội dung `Không làm ở MVP` là phần cố ý chưa xây để tránh phình scope.
- Tài liệu này không thay thế `ai-docs`; AI dùng `ai-docs` để triển khai sau khi Game Design được chốt.
