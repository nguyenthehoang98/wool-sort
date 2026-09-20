# F03 — Tiến trình, tài nguyên và nhịp chơi

## Trạng thái hiện tại

`GameData` lưu trong JSON của `PlayerPrefs`:

- `level`: order level hiện tại.
- `coin`: mặc định 100.
- `heart`: tối đa 5; hồi 1 heart mỗi 1800 giây.
- `infinite heart`: thời lượng tính bằng phút.
- inventory ba booster.
- remove ads.
- sound, music, vibration.
- cờ tutorial, daily reward, rate-us và thống kê quảng cáo.

Các thay đổi coin/heart/booster đồng thời gửi analytics qua `Databuckets`.

## Luồng tiến trình

- Entry load save.
- Level 1 đi thẳng Gameplay; từ level 2 trở đi vào Home.
- Thắng: nhận coin, tăng order level, có thể cộng heart và mở level tiếp.
- Thua/restart: trừ heart nếu không có infinite heart.
- Home hiển thị các level xung quanh level hiện tại; màn chưa mở không cho chơi.

## Tài nguyên và nguyên tắc mới

Mọi thay đổi phải qua `EconomyService` với:

- `resourceId`: `coin`, `heart`, `infinite_heart`, `booster.add_slot`, `booster.clean`, `booster.magic_box`.
- `delta` âm/dương.
- `reason` và `placement`.
- transaction id để chống cộng/trừ hai lần.
- event ledger để analytics và debug.

Không cho UI tự sửa số coin/heart. Không để quảng cáo trực tiếp cộng tài nguyên nếu callback chưa xác nhận thành công.

## Daily reward

Chu kỳ 7 ngày đang có các phần thưởng coin, heart-time và booster. Có claim thường, claim x2 qua rewarded ad và recover ngày bỏ lỡ qua ad. Khi redesign, timer phải dùng UTC/server time hoặc cơ chế chống chỉnh giờ có tài liệu; không tin tuyệt đối vào system clock trong môi trường cạnh tranh.

## Điều kiện sản phẩm cần chốt

- Người chơi có được chơi khi heart = 0 không?
- Heart có trừ ngay lúc vào màn hay chỉ khi thua?
- Có cho chọn level đã hoàn thành không?
- Coin dùng cho những hành động nào ngoài revive/booster/shop?
- Thưởng thắng có cố định hay phụ thuộc mode?

Bản hiện tại có nhiều popup và cost trong `Configs.cs`; trước khi port cần biến chúng thành data và test boundary.
