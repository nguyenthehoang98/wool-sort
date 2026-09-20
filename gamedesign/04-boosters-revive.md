# F05 — Booster và revive

## Add Slot

**Mục đích:** tạo thêm một ô vùng chờ khi người chơi sắp hết chỗ.

- Input: tap icon Add Slot.
- Điều kiện: chưa đạt giới hạn slot.
- Feedback: slot mới bay/scale vào layout, rung nhẹ và cập nhật counter.
- Thất bại: hiển thị tooltip “đã đủ slot”.

## Clean

**Mục đích:** dọn toàn bộ len đang nằm trong vùng chờ vào temporary box.

- Input: tap Clean và xác nhận nếu cần.
- Feedback: broom sweep, ghost của từng len bay vào hộp tạm.
- Kết quả: vùng chờ rỗng; người chơi tiếp tục xử lý len trong hộp tạm.
- Không được làm mất len dù animation bị ngắt.

## Magic Box

**Mục đích:** biến hộp đầu hàng đợi thành hộp cầu vồng để xử lý tình huống khó.

- Input: tap Magic Box.
- Feedback: đổi màu hộp, animation hút len và hoàn tất hộp.
- Chỉ tác động lên hộp được chọn; không làm thay đổi màu gốc của len khác.

## Revive

Khi thua:

- Nếu còn hộp chưa mở: offer thêm hộp.
- Nếu không còn hộp: offer Clean.
- Có thể trả bằng coin hoặc rewarded ad.
- Giữ nguyên board; không bắt người chơi chơi lại trừ khi bấm Restart.

## UX chống lỗi

- Khóa các booster khác trong lúc một booster đang chạy.
- Nút có trạng thái đang xử lý.
- Ad/coin fail thì board và inventory giữ nguyên.
- Cho người chơi hiểu booster thay đổi điều gì trước khi xác nhận.

![Add Slot reference](assets/screenshots/Popup%20More%20Booster%201.png)

![Clean reference](assets/screenshots/Popup%20More%20Booster%202.png)

![Magic Box reference](assets/screenshots/Popup%20More%20Booster%203.png)

![Revive reference](assets/screenshots/Popup%20Lose%20Revive%202.png)
