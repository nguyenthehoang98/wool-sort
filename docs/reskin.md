# Reskin hàng loạt

Có 2 cách. **Trong Unity là cách dễ dùng nhất** (mục 1); tool Python (mục 2) để chạy
batch/script hoặc khi không muốn mở Editor.

Nguyên tắc chung: **ghi đè nội dung file PNG**, không xoá rồi tạo lại. Nhờ vậy `.meta`
giữ nguyên → **GUID không đổi** → mọi prefab / scene / material trỏ tới ảnh đó
vẫn đúng, và `spriteBorder` (9-slice) + `spritePixelsToUnits` (PPU) không bị mất.

---

## 1. Trong Unity — `Tools / Reskin / Reskin hàng loạt`

File: `game/Assets/Editor/BulkReskinWindow.cs` — giao diện là **bảng**, mỗi dòng là 1 cặp
**ảnh cũ → ảnh mới**:

```
☐  ẢNH CŨ (asset trong project)   ẢNH MỚI (kéo file / bấm …)   LƯU VÀO (đường path đích)      CẢNH BÁO / DÙNG Ở
☐  [preview] Icon_Slot_3.png       [preview] new_icon.png          Assets/_Game/BoosterUI_Picked/…  Dùng ở: Gameplay.unity, …
```

**Cách dùng:**

1. Mở Unity → menu **`Tools / Reskin / Reskin hàng loạt`**.
2. Lấy danh sách dòng (1 trong 3):
   - **"Quét ảnh dùng trong game"** → chỉ lấy ảnh **reachable từ 3 scene gốc**
     (`EntryScene` / `Home` / `Gameplay`) qua `AssetDatabase.GetDependencies(recursive)`,
     nên bỏ hết ảnh plugin, ảnh editor, ảnh không ai dùng. Hiện tại ra **70 ảnh**
     (so với 285 ảnh nếu quét toàn project).
     - Scene gốc sửa được ở ô *"Scene gốc"* (phân tách bằng `;` hoặc `,`).
     - Tick *"Kèm ảnh trong Resources/"* nếu cần thêm ảnh chỉ được load runtime
       theo path (vd bản mirror `Resources/BoosterUI/`).
   - **"Nạp từ folder…"** → chọn folder ảnh mới, tool tự khớp theo tên file và điền cả 2 cột.
   - **"Thêm dòng"** → dòng trống, tự điền tay.
3. Chọn **ảnh mới** cho mỗi dòng:
   - **kéo file từ Explorer** vào ô *ẢNH MỚI* (kéo thẳng từ File Explorer, không cần import),
   - hoặc bấm **"…"** để browse file,
   - hoặc kéo 1 `Texture2D` **đã có trong project** vào ô đó.
4. Cột **"Lưu vào"** = đường path sẽ ghi ảnh. Mặc định = path của ảnh cũ. Sửa được nếu muốn
   ghi ra path khác (khi đó `.meta` của ảnh cũ được copy sang để giữ GUID/9-slice/PPU).
5. **"Áp dụng"** → backup bản gốc vào `game/.reskin_backup/<timestamp>/`, ghi đè, rồi
   `AssetDatabase.ImportAsset` để Unity reimport ngay.

**Quét không đơ:** cả "Quét ảnh trong project" lẫn "Nạp từ folder…" chạy theo lô (60 item/tick)
qua `EditorApplication.update` → có **thanh tiến trình** trong cửa sổ, Editor vẫn thao tác được.
Phần "Dùng ở" (`AssetDatabase.GetDependencies`) cũng được tính luôn trong lúc quét nên lúc vẽ
bảng không còn gọi I/O nào.

**Preview ảnh:** mỗi ô ảnh hiện **ảnh thật** (72×72, `ScaleToFit` → thấy trọn ảnh) trong khung
viền, bên cạnh là ObjectField để chọn. Ảnh cũ dùng `AssetPreview` của Unity (đúng sprite rect),
ảnh mới load trực tiếp từ file.

**Bảng được lưu lại:** nút **"Lưu bảng"** → `Assets/_Game/Configs/ReskinMap.json`.
Mở lại tool (hoặc "Nạp bảng") là có nguyên bảng cũ → tiếp tục làm tiếp.

**Cột "Cảnh báo / Dùng ở":**
- *Dùng ở:* các asset/scene đang trỏ tới ảnh cũ → giúp biết ảnh này ảnh hưởng chỗ nào.
- Cảnh báo: khác định dạng file (`.png` vs `.jpg` → bỏ qua) · đổi kích thước ·
  **9-slice đổi kích thước** (spriteBorder tính theo pixel cũ).

**Tuỳ chọn trên toolbar:**
- *Mirror BoosterUI* — sửa ở `_Game/BoosterUI_Picked/` thì copy đè sang `_Game/Resources/BoosterUI/`
  (bản runtime fallback mà `BoosterBarUI.ResolveSprites()` load).
- *Cột đường path* — ẩn/hiện cột "Lưu vào" nếu không dùng.
- *Cột nơi dùng* — ẩn/hiện cột "Dùng ở".
- Ô tìm kiếm — lọc theo tên/path.

**Hoàn tác:** nút **"Hoàn tác từ backup"** → khôi phục từ thư mục backup mới nhất.

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
