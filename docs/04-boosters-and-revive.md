# F04 — Booster và hồi sinh

## Ba booster

### Add Slot
Thêm một waiting slot, có giới hạn tối đa 7 slot. Dùng khi vùng chờ sắp đầy.

### Clean
Lấy toàn bộ len đang nằm trong waiting area, đưa vào temporary box bằng animation broom/ghost. Sau đó waiting area rỗng và người chơi tiếp tục xử lý temp box.

### Magic Box
Đổi box đầu queue thành rainbow box trong animation. Các slot còn thiếu được lấp bằng các piece phù hợp theo rule magic box; sau đó box hoàn tất như box thường.

## Hồi sinh khi thua

- Nếu còn box chưa unlock: offer `AddBasket`.
- Nếu không còn box để unlock: offer `Clean`.
- Hồi sinh phải giữ nguyên session state cần thiết; không load lại level trừ khi người chơi chọn restart.
- Có thể trả bằng coin hoặc rewarded ad tùy offer.

## Luồng an toàn

1. Kiểm tra booster có trong inventory hoặc người chơi đã xác nhận ad/coin.
2. Trừ resource đúng một lần.
3. Khóa input gameplay trong suốt transaction và animation.
4. Áp dụng state change.
5. Phát animation từ event state.
6. Mở khóa input khi callback thành công hoặc timeout có xử lý.

## Vấn đề cần sửa

`WaitingAreaManager.RefreshLayout()` hiện đặt `canAdd = false`, trong khi project vẫn có AddSlot. Cần quyết định đây là bug hay chủ ý trước khi port. Không được tự động giữ hành vi hiện tại nếu nó làm booster không thể dùng.
