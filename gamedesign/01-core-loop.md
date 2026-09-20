# F02 — Core loop phân loại len

## Vòng lặp một lượt

1. Người chơi quan sát các lớp len đang lộ trên board.
2. Tap một lớp ngoài cùng.
3. Len đi vào hộp cùng màu nếu có chỗ.
4. Nếu chưa có hộp phù hợp, len đi vào vùng chờ.
5. Lớp len được chọn tan ra; lớp bên trong hiện dần.
6. Hộp đầy sẽ hoàn tất và rời bàn; hộp tiếp theo xuất hiện.
7. Người chơi lặp lại đến khi board sạch hoặc vùng chờ bị khóa.

## Luật người chơi cần hiểu

- Chỉ lớp len đang lộ mới được chọn.
- Hộp chứa tối đa 3 cuộn trong phiên bản hiện tại.
- Cùng màu sẽ tự hút vào cùng một hộp.
- Vùng chờ có số ô giới hạn.
- Chạm sai hoặc không có chỗ hợp lệ không làm mất tài nguyên.
- Có thể giữ để xem trước lớp bên trong và kéo để xoay board.

## Thứ tự ưu tiên khi đặt len

1. Hộp cùng màu đang có nhiều len nhất.
2. Hộp cầu vồng nếu đang hoạt động.
3. Ô trống phù hợp trong vùng chờ, ưu tiên từ trái sang phải.

## Trạng thái thắng

Tất cả len đã được xử lý, các hộp hoàn tất animation và board không còn hành động chờ.

## Trạng thái thua

Tất cả ô vùng chờ đã đầy và không còn hành động hợp lệ để giải phóng ô hoặc đưa len vào hộp.

## Cảm giác hình ảnh/âm thanh

- Tap: pop nhẹ, âm thanh len và rung ngắn.
- Len vào slot: rope có độ võng và chuyển động mềm.
- Dissolve: các chi tiết trang trí rơi theo thân len.
- Hộp đầy: bounce, âm thanh hoàn tất, sau đó đóng/rời bàn.
- Gần thua: ô trống cuối có warning rõ nhưng không gây hoảng loạn.

## Thiết kế đề xuất

Tách rõ “luật đã xảy ra” và “animation đang trình diễn”. Nếu animation bị skip, game vẫn phải biết len đã vào slot hay chưa.

![Gameplay reference](assets/screenshots/Game%20Play.png)

*Reference: Gameplay Normal hiện tại.*
