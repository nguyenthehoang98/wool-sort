# Reskin hàng loạt

Tool: `tools/reskin.py` (Python, chạy ngoài Unity — không cần mở Editor).

Nguyên tắc: **ghi đè nội dung file PNG**, không xoá rồi tạo lại. Nhờ vậy `.meta`
giữ nguyên → **GUID không đổi** → mọi prefab / scene / material trỏ tới ảnh đó
vẫn đúng, và `spriteBorder` (9-slice) + `spritePixelsToUnits` (PPU) không bị mất.

## Dùng

```bash
# 1. xem kế hoạch (dry-run, KHÔNG thay gì)
python tools/reskin.py --src D:/new_skin

# 2. thực thi
python tools/reskin.py --src D:/new_skin --apply

# 3. kèm copy sang bản runtime fallback của booster
python tools/reskin.py --src D:/new_skin --apply --mirror-booster

# 4. khi ảnh mới có tên khác → dùng CSV map
python tools/reskin.py --src D:/new_skin --map map.csv --apply
```

`map.csv` — mỗi dòng: `đường_dẫn_ảnh_cũ,file_ảnh_mới`
```
_Game/BoosterUI_Picked/Icon_Slot_3.png,my_new_slot_icon.png
```

## Cách khớp file

| Ưu tiên | Quy tắc |
|---|---|
| 1 | Khớp **đường dẫn tương đối** trong `game/Assets` — nếu bạn đặt ảnh mới trong thư mục cấu trúc y hệt (`_Game/Textures/UIV2/Frame_Tooltips.png`) thì khớp tuyệt đối |
| 2 | Khớp theo **CSV map** |
| 3 | Khớp theo **tên file**. Nếu trùng nhiều nơi → chọn bản **không nằm trong `Resources/`** (bản editable), và in ra các bản trùng còn lại. Muốn thay hết thì thêm `--all-matches` |

## Cờnh báo

- **9-slice**: nếu ảnh có `spriteBorder` khác 0 mà kích thước PNG mới khác cũ → cảnh báo, vì `spriteBorder` đang tính theo pixel cũ.
- **Kích thước khác**: in ra `cũ → mới`.
- **Không tìm thấy ảnh cũ**: file mới sẽ không được đưa vào project (tool không tự tạo asset mới) — tự thêm vào Unity nếu cần.
- **Backup**: mặc định lưu bản gốc vào `.reskin_backup/<timestamp>/` (đã gitignore). Tắt bằng `--no-backup`.

## Lưu ý khi reskin

1. **PPU = 100** cho toàn bộ ảnh UI. Đổi PNG mà giữ PPU thì UI giữ nguyên kích thước trên màn hình; muốn đổi kích thước hiển thị thì sửa PPU trong Inspector.
2. **Ảnh 9-slice** (`Frame_Bar_1/2/3`, `Frame_Popup_1/2/3/4`, `Button_Green`, `Button_Blue`, `Button_Close`, `slot2`, `btn2Green`) — giữ nguyên `spriteBorder`.
3. **Booster**: sửa ở `_Game/BoosterUI_Picked/` thì phải copy đè sang `_Game/Resources/BoosterUI/` (tool làm việc này với `--mirror-booster`), vì `BoosterBarUI.ResolveSprites()` load bản trong `Resources/` khi field Inspector còn trống.
4. **Unity đang mở** sẽ tự reimport file đã đổi. Nếu thấy chưa cập nhật, focus Unity hoặc `Assets → Refresh`.

## Tầng "1 file đổi cả bộ" (không cần tool)

Một số texture được **nhiều material dùng chung** → đổi đúng 1 file là đổi cả bộ:

| Texture | Số material dùng | Đổi → |
|---|---|---|
| `_Game/Textures/Pattern/Pattern_basecolor.png` | 21 (`ThreadMat_*`) | màu toàn bộ len |
| `_Game/Textures/Pattern/Pattern_normal.png` | 21 | normal toàn bộ len |
| `_Game/Textures/Fabric_Pattern/Fabric_Pattern_basecolor.png` | 21 (`StringMat_*`) | hoạ tiết toàn bộ sợi |
| `_Game/Textures/Fabric_Pattern/Fabric_Pattern_normal.png` | 21 | |
| `_Game/Textures/MasterColor.png` | 3 (`_Texture1` của spool) | màu cuộn chỉ |
| `_Game/Textures/RainbowTexture.png` | `_TextureSample0` | màu cầu vồng |
| `_Game/Textures/Gameplay/Pattern.png` | `MaskPattern.mat` | nền bàn chơi |
| `_Game/Textures/UI/BGR_Gameplay.png` | Gameplay | nền gameplay |

Màu từng cuộn len vẫn tint riêng qua `_Color0` trong material → sửa trong Inspector hoặc
sửa `_Game/Configs/YarnColorDatabase.asset`.
