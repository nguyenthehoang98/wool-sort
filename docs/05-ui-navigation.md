# F05 — Home, chọn màn và HUD

## Màn hình chính

### Entry/loading
Hiển thị progress, khởi tạo save/config/SDK/audio rồi chuyển scene.

### Home
Có level carousel khoảng 7 item quanh level hiện tại, nút Play, shop, menu bar và các popup daily/settings.

### Gameplay HUD
Có level/mode, bộ đếm len còn lại, coin bar, heart, ba booster, setting, remove ads, slider scale và khu vực board/box/waiting.

### Kết quả
- Win popup: hiển thị phần thưởng, nhận coin thường hoặc nhân đôi qua ad, chuyển màn tiếp theo.
- Lose/Continue popup: revive bằng thêm box hoặc clean, hoặc restart.
- Setting in-game: tiếp tục, restart, về Home.

## Yêu cầu UX khi làm lại

- Safe area cho điện thoại tai thỏ và màn hình tỷ lệ dài.
- Gameplay object được đặt theo anchor UI, không hard-code theo một độ phân giải.
- Chỉ một popup modal nhận input tại một thời điểm.
- Nút đang chờ ad/purchase phải có loading và không cho double tap.
- Không che board bằng banner ở thời điểm người chơi cần đọc state.
- Màu không phải tín hiệu duy nhất; thêm pattern/shape nếu cần accessibility.

## Trạng thái UI

`Loading → Home/Gameplay → Playing → Modal → Playing → Win/Lose → Home/NextLevel`.

UI chỉ render state và gửi intent. Không tự quyết định luật thắng/thua hay tự cộng tài nguyên.
