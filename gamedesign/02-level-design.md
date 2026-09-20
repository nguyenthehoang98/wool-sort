# F03 — Thiết kế level và độ khó

## Cấu trúc level

Mỗi level gồm:

- Board 3D với các root yarn.
- Màu và thứ tự các lớp len bên trong.
- Số lượng hộp ban đầu và hộp có thể mở.
- Số ô vùng chờ.
- Thumbnail/preview.
- Mode: Normal, Hard hoặc Very Hard.

Project hiện tại dùng level order riêng với asset ID; level chơi đầu tiên là order 1 nhưng dùng asset level 3.

## Nhịp độ đề xuất

- **Level 1–3:** dạy tap, hộp màu và thắng.
- **Level 4–7:** dạy vùng chờ, layer trong và hold preview.
- **Level 8–15:** mở booster, thêm màu và quyết định thứ tự.
- **Sau 15:** phối hợp nhiều layer, hộp khóa/mở và mode khó.
- **Hard/Very Hard:** giới thiệu bằng warning ngắn, không dùng cảnh báo cho level thường.

## Tiêu chí một level tốt

- Có ít nhất một đường giải hợp lý không cần booster.
- Người chơi nhìn thấy lý do của mỗi thất bại.
- Không yêu cầu tap pixel-perfect.
- Thời gian giải tăng từ từ, không tăng đột ngột chỉ bằng số màu.
- Level mới đưa vào một biến thể, không đưa cùng lúc nhiều luật mới.

## Biến số độ khó

1. Số màu.
2. Số lớp ẩn.
3. Số hộp đang hoạt động.
4. Mức độ đan xen giữa các màu.
5. Số ô vùng chờ.
6. Tần suất xuất hiện lớp làm lộ ra màu mới.
7. Mode Hard/Very Hard.

## Công cụ level designer cần có

- Preview board ở nhiều góc xoay.
- Thống kê màu/layer/hộp.
- Validator phát hiện màu thiếu, layer sai, asset thiếu.
- Nút test nhanh level và reset.
- Thumbnail tự sinh.

## Không làm ở MVP

Không sinh level procedural hoàn toàn. Ưu tiên level thủ công có thể kiểm duyệt, sau đó mới thử sinh hỗ trợ bằng AI.

![Hard level reference](assets/screenshots/Game%20Play%20Hard.png)

![Very hard level reference](assets/screenshots/Game%20Play%20Super%20Hard.png)
