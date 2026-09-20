# F06 — UI/UX và các màn hình

## Luồng màn hình

`Loading → Home → Level Select/Play → Gameplay → Win hoặc Lose → Home/Next Level`.

## Loading

- Logo/game identity.
- Progress tối thiểu, không cần hiển thị kỹ thuật.
- Có thể bỏ qua phần chờ không cần thiết khi dữ liệu đã sẵn sàng.

## Home

- Nhân vật/board preview tạo cảm giác game thân thiện.
- Level hiện tại là CTA lớn nhất.
- Coin/heart ở vùng dễ nhìn nhưng không lấn át nút Play.
- Shop, Daily Reward, Settings nằm ở menu dễ tìm.

## Gameplay HUD

- Top: level/mode, coin, heart, settings.
- Center: board len và hộp.
- Bottom: vùng chờ và ba booster.
- Counter len còn lại phải đọc được trong một glance.
- Warning vùng chờ chỉ xuất hiện khi thật sự nguy hiểm.

## Win

- Cho người chơi thấy board đã được dọn sạch.
- Hiển thị reward và nút nhận.
- Offer nhân đôi bằng ad là lựa chọn, không phải điều kiện bắt buộc.
- CTA kế tiếp rõ ràng.

## Lose/Revive

- Nói rõ vùng chờ đã đầy hoặc không còn nước đi.
- Hiển thị lựa chọn revive trước restart.
- Phân biệt nút “xem quảng cáo”, “dùng coin”, “bỏ qua”.

## Settings

- Sound, music, vibration.
- Language.
- Restore purchase nếu có IAP.
- Nút về Home/restart trong gameplay.

## Yêu cầu mobile

- Tương thích safe area và màn hình dọc.
- Touch target đủ lớn.
- Không dùng màu đơn độc để truyền state quan trọng.
- Popup có thứ tự đóng/back rõ ràng.

![Settings reference](assets/screenshots/Popup%20Setting%20Home.png)

![Shop reference](assets/screenshots/Shop%201.png)
