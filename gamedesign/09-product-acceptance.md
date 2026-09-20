# F10 — Tiêu chí nghiệm thu sản phẩm

## Core loop

- Người chơi mới có thể hiểu và hoàn thành level đầu mà không cần đọc tài liệu ngoài.
- Tap đúng luôn cho feedback rõ.
- Không có double placement hoặc mất len.
- Win/lose chỉ xuất hiện một lần và có thể tiếp tục/restart.

## Progression

- Hoàn thành level mở được level tiếp theo.
- Save/load không làm mất coin, heart, booster hoặc progress.
- Daily reward và timer không tự nhân thưởng sau khi mở lại app.

## Booster

- Add Slot, Clean, Magic Box khác nhau rõ về mục đích.
- Booster không làm board kẹt khi animation bị ngắt.
- Revive giữ nguyên trạng thái ván.

## UX

- Chạy được trên nhiều tỷ lệ màn hình dọc.
- Popup không cho tap xuyên xuống gameplay.
- Nút ad/IAP có loading, success/fail rõ.
- Tắt sound/music/vibration có hiệu lực ngay.

## Dữ liệu và vận hành

- Level thiếu asset được phát hiện trước build.
- Có thể thêm level mà không sửa logic core.
- Analytics/ad/IAP lỗi không làm crash vòng lặp puzzle.
- Có smoke test Android cho Entry → Home/Gameplay → Win/Lose.

## Out of scope bản đầu

- Multiplayer.
- Server account/cloud save bắt buộc.
- Procedural level generation hoàn toàn.
- Battle pass, ranking online, marketplace.
