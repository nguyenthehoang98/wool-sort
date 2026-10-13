# BoosterUI_Picked — asset pick từ dự án gốc

Thư mục này chứa sprite copy trực tiếp từ dự án gốc
`wool-sort (2)/Assets`, để dựng lại UI booster mà **không copy prefab**
(tránh lỗi prefab như đã thống nhất).

## Nguồn gốc

| File ở đây | Nguồn gốc | Dùng cho |
|---|---|---|
| `Button_Boosters.png` | `_Game/Textures/UI/Button_Boosters.png` | `BoosterBtn/Frame` (khung thường) |
| `Button_Boosters_Gray.png` | `_Game/Textures/UI/Button_Boosters_Gray.png` | `BoosterBtn/FrameLock` (khung khóa) |
| `Icon_Lock_2.png` | `_Game/Textures/UI/Icon_Lock_2.png` | `BoosterBtn/FrameLock/ImgLock` |
| `Icon_Plus.png` | `_Game/Textures/UI/Icon_Plus.png` | `BoosterBtn/IconPlus` (hết lượt → mua) |
| `Icon_Noti.png` | `_Game/Textures/UI/Icon_Noti.png` | `BoosterBtn/Notice` (nền badge đếm) |
| `Icon_Slot_3.png` | `_Game/Textures/UI/Icon_Slot_3.png` | AddSlot icon thường + popup content[0] |
| `Icon_Slot_4.png` | `_Game/Textures/UI/Icon_Slot_4.png` | AddSlot icon xám (lock) |
| `Icon_Broom_2.png` | `_Game/Textures/UI/Icon_Broom_2.png` | Clean icon thường + popup content[1] |
| `Icon_Broom_3.png` | `_Game/Textures/UI/Icon_Broom_3.png` | Clean icon xám (lock) |
| `Icon_Spool_2.png` | `_Game/Textures/UI/Icon_Spool_2.png` | MagicBox icon thường + popup content[2] |
| `Icon_Spool_3.png` | `_Game/Textures/UI/Icon_Spool_3.png` | MagicBox icon xám (lock) |
| `Icon_Coin_S.png` | `_Game/Textures/UI/Icon_Coin_S.png` | Popup nút mua coin |
| `Icon_ADS.png` | `_Game/Textures/UIV2/Icon_ADS.png` | Popup nút xem ads |
| `Button_Blue.png` | `_Game/Textures/UIV2/Button_Blue.png` | Popup nút mua coin |
| `Button_Green.png` | `_Game/Textures/UIV2/Button_Green.png` | Popup nút xem ads |
| `Button_Gray.png` | `_Game/Textures/UIV2/Button_Gray.png` | (dự phòng nút disable) |
| `Button_Close.png` | `_Game/Textures/UIV2/Button_Close.png` | Popup nút đóng |
| `Frame_Popup_1/2.png` | `_Game/Textures/UIV2/Frame_Popup_*.png` | Popup card |

Prefab gốc tham khảo (không copy, chỉ đọc số đo):
`_Game/Prefab/UI/BoosterBtn.prefab` (root 200×200, Frame 194×100,
ImgIcon 102×105 tại y≈41.5, Notice 56×56 tại (88.6,−24.6),
IconPlus 55×56 tại (89,−23), TxtNum 40sp auto-size),
`_Game/Resources/MoreBoosterPopup.prefab`
(title ADD SLOT / CLEAN / MAGIC SPOOL).

## Cách dùng

1. Mở scene `Gameplay`, tìm `BoosterBarUI` (do script tự tạo khi Play
   nếu chưa có) hoặc thêm component `BoosterBarUI` vào một GameObject.
2. Kéo sprite từ thư mục này vào các field tương ứng của `BoosterBarUI`
   và `MoreBoosterPopupUI` trong Inspector.
3. Nếu để trống, code tự `Resources.Load("BoosterUI/<tên>")` từ bản mirror
   trong `Assets/_Game/Resources/BoosterUI/`; thiếu nữa vẫn chạy bằng màu
   fallback theo loại booster để test logic.

## Mirror runtime `Assets/_Game/Resources/BoosterUI/`

Bản copy 19 PNG (chỉ PNG, không copy `.meta` để tránh trùng guid) dùng làm
fallback tự load khi Inspector chưa gán. Khi thay sprite: thay ở thư mục này
rồi copy đè PNG sang `Resources/BoosterUI/` (giữ nguyên tên file).

Quy ước: chỉ sửa/thay asset trong thư mục này, không sửa asset gốc.
