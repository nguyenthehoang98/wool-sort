# F01 — Vòng lặp phân loại len

## Mục tiêu người chơi

Gỡ hết các lớp len trên board bằng cách chọn lớp đang lộ ra và đưa nó vào slot cùng màu. Mỗi hộp chứa 3 slot. Khi hộp đầy, hộp đóng và nhường chỗ cho hộp tiếp theo. Vùng chờ là nơi tạm chứa len chưa có hộp phù hợp.

## Luồng một lượt

1. Người chơi tap vào `YarnPart` ngoài cùng.
2. Game tìm slot hợp lệ:
   - ưu tiên slot của box nhận đúng màu;
   - trong các box phù hợp, ưu tiên box đã có nhiều slot được lấp;
   - nếu không có, chọn waiting slot trống phù hợp, từ trái sang phải.
3. Slot chuyển sang filled, tạo rope từ len tới slot.
4. Len dissolve; lớp trong đã warm-up sẽ hiện và grow.
5. Nếu toàn bộ slot box đã filled, box khóa tương tác.
6. Khi animation của toàn bộ slot kết thúc, box chạy out animation và box tiếp theo có thể xuất hiện.
7. Khi hết len và các animation kết thúc, thắng.
8. Khi toàn bộ waiting slot đều filled và không còn cách xử lý tự động, thua.

## Tương tác phụ

- Tap một lần: lấy len.
- Hold khoảng 0.3 giây: xem trước lớp bên trong bằng material trong suốt; thả tay để trả visual.
- Kéo trên board: xoay model.
- Slider: thay đổi scale camera/model trong giới hạn level.
- Trong animation đầu level, popup, ad hoặc booster đang chạy: không nhận input gameplay.

## Cảm giác cần giữ

Dissolve và decoration rơi theo tiến độ, rope có võng/đung đưa, slot ring xuất hiện, box bounce, auto-zoom khi board giảm, warning khi waiting còn một slot trống, cảnh báo Hard/Very Hard khi bắt đầu màn.

## Luật không được mơ hồ

- Không đặt len vào slot khác màu.
- `Rainbow` box nhận mọi màu nhưng không biến màu len thành rainbow.
- Tap khi không có slot hợp lệ không làm thay đổi state, không trừ resource.
- Một len đang dissolve không thể bị tap lần nữa.
- Thứ tự box tiếp theo phải deterministic theo level và state; random chỉ dùng khi rule cho phép.
- Win/lose chỉ phát một lần cho mỗi session.

## Tách logic và hiển thị trong bản mới

`PuzzleResolver` xử lý `SelectPiece`, `FillSlot`, `ResolveBox`, `CheckOutcome`. Unity adapter chỉ chạy animation/FX theo event như `PiecePlaced`, `PieceDissolved`, `BoxCompleted`, `SessionWon`, `SessionLost`.

## Definition of Done

- Có thể chơi một level không cần SDK mạng.
- Replay cùng level với cùng input cho cùng kết quả.
- Tap nhanh liên tục không tạo duplicate placement.
- Test được resolver bằng C# thuần, không cần mở scene.
- Animation lỗi hoặc bị skip không làm state kẹt; adapter phải có completion timeout/fallback.
