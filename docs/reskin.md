# Reskin hàng loạt

Có 2 cách. **Trong Unity là cách dễ dùng nhất** (mục 1); tool Python (mục 2) để chạy
batch/script hoặc khi không muốn mở Editor.

Nguyên tắc chung: **ghi đè nội dung file PNG**, không xoá rồi tạo lại. Nhờ vậy `.meta`
giữ nguyên → **GUID không đổi** → mọi prefab / scene / material trỏ tới ảnh đó
vẫn đúng, và `spriteBorder` (9-slice) + `spritePixelsToUnits` (PPU) không bị mất.

---

## 1. Trong Unity — `Tools / Reskin / Reskin hàng loạt`

File: `game/Assets/Editor/BulkReskinWindow.cs`

**Cách dùng:**

1. Mở Unity → menu **`Tools / Reskin / Reskin hàng loạt`**.
2. Bấm **"Chọn…"** → trỏ tới folder chứa ảnh mới (ở bất kỳ đâu trên máy).
   Tool tự quét và lập bảng khớp.
3. Xem bảng: mỗi dòng có **preview ảnh mới** | **preview ảnh cũ** | **ảnh trong project**
   (dropdown chọn đích nếu trùng tên) | **cảnh báo**.
4. Tích chọn dòng nào muốn thay → bấm **"Áp dụng"**.
   Tool backup bản gốc vào `game/.reskin_backup/<timestamp>/` rồi ghi đè, và
   `AssetDatabase.ImportAsset` để Unity reimport ngay.

**Tuỳ chọn:**

| Checkbox | Tác dụng |
|---|---|
| *Thay cả các bản trùng tên* | Mặc định chỉ thay bản **không nằm trong `Resources/`** (bản editable). Bật lên thì thay hết các bản trùng tên. |
| *Copy thêm sang Resources/BoosterUI* | Sửa ở `_Game/BoosterUI_Picked/` thì copy đè sang `_Game/Resources/BoosterUI/` (bản runtime fallback mà `BoosterBarUI.ResolveSprites()` load). |
| *Chỉ hiện dòng có vấn đề* | Lọc ra dòng có cảnh báo / không khớp. |

**Cảnh báo hiển thị:** khác định dạng file (`.png` vs `.jpg` → bỏ qua) · đổi kích thước ·
**9-slice đổi kích thước** (spriteBorder tính theo pixel cũ).

**Hoàn tác:** nút **"Hoàn tác từ backup"** → khôi phục từ thư mục backup mới nhất.

**Cách khớp file:**

| Ưu tiên | Quy tắc |
|---|---|
| 1 | Khớp **đường dẫn tương đối** trong `game/Assets` — nếu đặt ảnh mới trong thư mục cấu trúc y hệt (`_Game/Textures/UIV2/Frame_Tooltips.png`) thì khớp tuyệt đối |
| 2 | Khớp theo **tên file**. Trùng nhiều nơi → chọn bản **không nằm trong `Resources/`**, có thể đổi đích bằng dropdown |

---

## 2. Tool Python — `tools/reskin.py`

Chạy ngoài Unity, hợp khi batch/script hoặc không muốn mở Editor.

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

Trên Windows: mở PowerShell tại thư mục repo rồi chạy lệnh trên (dùng `py -3` nếu
`python` không có trong PATH).

`map.csv` — mỗi dòng: `đường_dẫn_ảnh_cũ,file_ảnh_mới`
```
_Game/BoosterUI_Picked/Icon_Slot_3.png,my_new_slot_icon.png
```

Cờ khác: `--all-matches` (thay cả các bản trùng tên) · `--no-backup`.
Backup ghi vào **cùng thư mục** `game/.reskin_backup/<timestamp>/` với EditorWindow.

---

## Cảnh báo chung

- **PPU = 100** cho toàn bộ ảnh UI. Đổi PNG mà giữ PPU thì UI giữ nguyên kích thước trên màn hình; muốn đổi kích thước hiển thị thì sửa PPU trong Inspector.
- **Ảnh 9-slice** (`Frame_Bar_1/2/3`, `Frame_Popup_1/2/3/4`, `Button_Green`, `Button_Blue`, `Button_Close`, `slot2`, `btn2Green`) — giữ nguyên `spriteBorder`.
- **Unity đang mở** sẽ tự reimport file đã đổi. Nếu thấy chưa cập nhật, focus Unity hoặc `Assets → Refresh`.

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
