# TÍNH NĂNG ĐÃ HOÀN THÀNH — CHI TIẾT (kèm bằng chứng code)

> Phạm vi: những gì **chạy được thật** trong `game/` tính đến 08/10/2026. Các stub/hàm rỗng ghi rõ ở mục 8.

## 1. Vòng lặp gameplay chính (GDD §2, `gamedesign/01-core-loop.md`)

* **Tap len → bay vào slot:** tap lớp len ngoài cùng, sợi bay (rope cong võng/đung đưa) vào slot. `Gameplay/YarnPart.cs` (tap/hold), `Gameplay/SlotManager.cs:69-136` (chọn slot), `Gameplay/YarnRopeController.cs:53-75` + `Rope.cs`.
* **Ưu tiên slot đúng luật:** box cùng màu đã đầy nhiều nhất trước, hết box mới sang waiting (trái→phải). `SlotManager.cs:83-135`, `WaitingAreaManager.cs`.
* **Lớp trong (inner layer):** bấm lớp ngoài hiện lớp 2, bấm tiếp hiện lớp 3… màu Fixed/random theo cấu hình. `YarnPartLayerSpawner.cs:38-53,159-200`, dữ liệu `YarnPartInfo.cs`, `LevelYarnInfoHolder.cs`.
* **Nhấn giữ xem trước (hold preview):** giữ 0.3s làm trong suốt lớp ngoài để nhìn lớp trong, thả ra trả lại. `YarnPart.cs:241-320`, `GameConfig.cs:32` (`holdThreshold = 0.3`).
* **Xoay/quay/scale model:** kéo xoay, tự xoay khi rảnh, slider + pinch zoom, auto-zoom khi gần hết len. `ModelDragRotate.cs`, `GameManager.cs:350-415`, `ScaleSliderSync.cs`, `GameConfig.cs:20` (`autoZoomScale`).
* **Hiệu ứng:** tan dần (dissolve shader) + đồ trang trí rơi, box nảy (bounce), vòng slot co lại, cảnh báo khi waiting còn 1 slot trống. `YarnPart.cs:346-404`, `Box.cs:157-176`, `WaitingAreaManager.cs:260-282`.

## 2. Box, waiting, thắng/thua (`docs/01-core-gameplay.md`, GDD §2)

* **Box 3 slot:** đầy 3 sợi cùng màu → đóng nắp → sinh box mới theo thuật toán chọn màu (ưu tiên màu nông nhất theo phase % đã chơi). `BoxManager.cs:157-278`, `GameManager.cs:722-879` (`ChooseNextBoxColor`).
* **Vùng chờ:** 5 slot khởi đầu, tối đa 7; đầy hết thì thua; có tự refill từ waiting vào box khi box trống. `WaitingAreaManager.cs:41-109,191-194`, `GameConfig.cs:9-10`.
* **Thắng:** hết sợi + đóng box cuối → popup "YOU WIN" → NEXT. **Thua:** waiting đầy → popup "LEVEL FAILED" → RETRY (chơi lại đúng level đó). `GameManager.cs:471-553`, `GameUIController.cs:176-268`.
* **Qua màn:** hết Level7 tự quay về Level1 (giữ chỗ cho campaign dài). `GameManager.cs:206-214`.

## 3. Tô ảnh khi đầy box (board painting 84x84)

* Mỗi box đầy bắn 1 dây lên bảng, tô dần từng cụm pixel đúng màu theo tiến trình. `BoxManager.cs:469-533`, `JamImage.cs:46-204`, `LevelColorMatrix.cs` (lưới chuẩn `DefaultGrid = 84`).
* Đọc ma trận màu đã bake (không phụ thuộc file ảnh gốc lúc chạy): `JamImage.cs:95-102`, `BuildCellOwnersFromMatrix`.
* Sửa trong kỳ: dây bám đúng pixel hiển thị với mọi scale/drawMode (`MovePointerTo` dùng `Renderer.bounds`), sample pixel hết lệch nửa ô với ảnh đúng 84x84 (`FloorToInt`).

## 4. Content: 7 level thật

* `Resources/Levels/Level1–Level7.prefab` (mỗi màn десятки part, nhiều lớp trong, nhiều màu) + `Resources/LevelColorMatrix/Level1–Level7.asset`.
* Nhịp 2 ngày dựng Level3→Level7 (git log 07–08/10/2026: `level 3`, `setup level 4`, `done level 5/6`, `level 7`, `Finish 7 level`).
* Lưu ý: `Level6.prefab` đang sửa dở, chưa commit (`git status`).

## 5. Pipeline làm level — Level Studio (vượt cả kế hoạch tooling)

`Assets/Editor/LevelStudio/` gom 5 tab (`LevelStudioWindow.cs`):

| Tab | Có gì |
|---|---|
| 1. Tạo level | Dựng prefab level từ FBX (`LevelQuickCreatorWindow`) |
| 2. Độ khó & Màu | Đo/phân tích độ khó, đồng bộ 21 màu, rebuild holder (`LevelDifficultyTool`) |
| 3. Validate | Quét full + **4 nút tự sửa**: sửa từng màu `%3`, sửa tổng `%3`, sửa pool random, vá màu thiếu matrix (`BoxImageColorValidator`) |
| 4. Prompt ảnh | Sinh prompt vẽ ảnh AI theo màu level (`ColorPromptWindow`) |
| 5. Matrix ảnh | Extract ảnh→matrix (tự gom màu giống nhau, ngưỡng chỉnh được, mặc định 0.15), Export/Import/Validate/Preview JSON tay, vá màu thiếu, xuất PNG sửa PTS (`LevelImageMatrixTool`) |

Tool phụ: decor len, dissolve, mật độ texel, xuất model, theo dõi dùng màu, kiểm tra phụ thuộc level.

## 6. Các lỗi đã sửa trong kỳ (08/10/2026)

1. **Level4 pool random thừa 1** (8 slot Random nhưng pool 9 → sợi ma, tổng ảo 102 vs 101): thêm nút tự sửa pool (bớt pool + bù inner Fixed giữ `%3`) + guard runtime báo `[POOL-THỪA]` đầu game. `BoxImageColorValidator.cs`, `GameManager.cs:921-928`.
2. **Teal 3 sợi không có ô trong ma trận** (box Teal bắn rỗng): nút "Vá màu thiếu" + sửa câu báo lỗi chỉ đúng chỗ sửa (matrix, không phải extract lại).
3. **Dây tô đứng yên ở pivot**: `MovePointerTo` cũ tính theo pixel sprite trong khi scene override scale/drawMode → chuyển sang map theo khung hình đang hiện.
4. **Extract JSON nổ hàng nghìn palette id** (ảnh AI 2115 màu rời rạc): thêm bước gom cụm + thanh trượt ngưỡng; đo thực tế 0.15 → ~20 nhóm.

## 7. Test và kỹ thuật

* Unity `6000.3.21f1` (`ProjectSettings/ProjectVersion.txt`), compile sạch trên target Android (theo `todo.md` CLONE-03).
* 1 PlayMode test: popup thắng/thua lên level/giữ level (`Assets/Tests/PlayMode/ResultPopupProgressionTests.cs`).
* Pooling cho box/rope (`PoolManager`, `PrefabPool`), event bus nội bộ (`UnityEventBus`).

## 8. Stub đã biết (có mặt nhưng chưa chạy — KHÔNG tính là xong)

* `PlayWarningLose()` rỗng, `StopLoopPrintSfx()` rỗng, `ResetData()` rỗng.
* Nhánh Rainbow-Box bị comment (`SlotManager.cs:71-81`); box Rainbow chỉ log lỗi khi tô ảnh.
* Recenter khi auto-zoom bị khóa cứng (`AutoZoomRecenter = false`).
* `failOfferLevel = 10` không ai đọc; Revive/AddBox/MagicBox mới ở mức comment.
* Tutorial chỉ có 1 marker data (`TutorialTargetMarker.cs`), chưa có controller.
