# BÁO CÁO KỸ THUẬT ĐẦY ĐỦ — WOOL SORT (break theo feature)

> Mục đích: đọc là biết tính năng đó **là gì, làm nhiệm vụ gì**, còn thiếu gì, việc tiếp theo là gì.
> Quy ước: ✅ đã chạy · ◐ một phần/stub · ❌ chưa làm. Đường dẫn là bằng chứng trong `game/`.

---

## NHÓM A — CORE GAMEPLAY (luật chơi)

### A1. Tap & chọn slot — ✅
**Là gì:** người chơi bấm sợi len, hệ thống tìm chỗ đặt.
**Nhiệm vụ:** chỉ nhận tap ở lớp ngoài cùng còn đứng; tìm box cùng màu đầy nhất trước, hết thì sang waiting trái→phải; tap vô lệ = no-op, không trừ gì.
**Bằng chứng:** `Gameplay/YarnPart.cs` (tap/hold), `Gameplay/SlotManager.cs:69-136`.
**Việc còn lại:**
* Tách luật chọn slot ra hàm thuần (không phụ thuộc list runtime) để test tự động được (W4-01).

### A2. Lớp len trong (inner layer) — ✅
**Là gì:** mỗi sợi len có nhiều lớp chồng nhau; gỡ lớp ngoài mới lộ lớp trong.
**Nhiệm vụ:** luôn giữ 2 lớp ngoài cùng trên màn; tap lớp i hiện lớp i+1 và warm-up sẵn i+2; màu Fixed lấy cấu hình, Random bốc từ pool chung của màn.
**Bằng chứng:** `Gameplay/YarnPartLayerSpawner.cs:38-200`, `Gameplay/YarnPartInfo.cs`, `Gameplay/LevelYarnInfoHolder.cs`.
**Việc còn lại:**
* Thêm seed cho pool random để chơi lại cùng 1 layout (phục vụ QA lặp lại ván lỗi, W5-04).

### A3. Hộp & sinh hộp (box) — ✅
**Là gì:** hộp 3 slot gom 3 sợi cùng màu; đầy thì đóng nắp, bay đi, hộp mới ra.
**Nhiệm vụ:** đầy 3 sợi cùng màu mới đóng; chọn màu hộp mới theo thuật toán ưu tiên màu nông nhất theo phase % đã chơi; hết quota box thì thôi sinh.
**Bằng chứng:** `Gameplay/BoxManager.cs:157-278`, `GameManager.cs:722-879` (`ChooseNextBoxColor`).
**Việc còn lại:** không (logic đủ) — chỉ cần test regression khi thêm màn mới.

### A4. Vùng chờ (waiting) — ✅
**Là gì:** bãi đỗ tạm 5 slot (tối đa 7) khi chưa có hộp nhận.
**Nhiệm vụ:** xếp trái→phải; đầy hết mà bàn còn len → thua; tự chảy ngược vào box khi box trống.
**Bằng chứng:** `Gameplay/WaitingAreaManager.cs:41-194`.
**Việc còn lại:** móc thêm slot bằng booster AddSlot (sang nhóm E).

### A5. Thắng / thua / chơi lại — ✅
**Là gì:** kết thúc ván và cho chơi tiếp.
**Nhiệm vụ:** hết sợi + hết box → Win; waiting đầy → Lose; mỗi sự kiện phát đúng 1 lần; NEXT sang màn (hết màn 7 về 1), RETRY load lại màn đó, không cộng/trừ nhầm gì.
**Bằng chứng:** `GameManager.cs:471-553`, `GameUIController.cs:176-268`.
**Việc còn lại:** gắn thưởng/phạt economy khi có nhóm F (hiện Win/Lose chưa đụng tới coin/heart).

---

## NHÓM B — CONTENT MÀN CHƠI & PIPELINE (nguyên liệu game)

### B1. Dữ liệu 7 màn chơi — ✅
**Là gì:** prefab 3D từng màn (part, màu, lớp trong) + ma trận màu ảnh 84x84.
**Nhiệm vụ:** mỗi màn có đủ part/màu/layer để giải được; tổng sợi và từng màu chia hết cho 3; pool random khớp số slot Random.
**Bằng chứng:** `Resources/Levels/Level1–7.prefab`, `Resources/LevelColorMatrix/Level1–7.asset`.
**Việc còn lại:**
* Commit `Level6.prefab` đang treo; quét full 7 màn hết đỏ; mở rộng lên N màn theo lịch campaign (mỗi màn mới chỉ việc vẽ + extract, không sửa code).

### B2. Level Studio (bộ đồ nghề làm màn) — ✅ (vượt kế hoạch)
**Là gì:** 5 tab Editor: tạo màn, độ khó & màu, validate, prompt ảnh AI, matrix ảnh.
**Nhiệm vụ:** dựng màn từ FBX → đo khó → quét lỗi → tự sửa (`%3`, pool random, vá màu thiếu) → extract/import ma trận màu (JSON gom nhóm, ngưỡng chỉnh được).
**Bằng chứng:** `Assets/Editor/LevelStudio/` (6 file chính).
**Việc còn lại:** viết 1 trang hướng dẫn dùng cho người mới (phục vụ bàn giao W9).

### B3. Tô ảnh lên bảng (board painting) — ✅
**Là gì:** hộp đầy bắn dây lên bảng, ảnh hiện dần từng cụm pixel đúng màu.
**Nhiệm vụ:** đọc ma trận màu đã bake (không cần file ảnh gốc lúc chạy); dây bám đúng pixel đang tô; mỗi màu có sợi phải có ô để tô.
**Bằng chứng:** `JamImage.cs:46-215`, `LevelColorMatrix.cs`, `BoxManager.cs:469-533`.
**Việc còn lại:** dọn luồng đọc ảnh gốc còn sót trong `JamImage`/`BoxImageColorValidator` khi đã chốt flow matrix-only.

---

## NHÓM C — GAME FEELING (cảm giác chơi — bắt buộc, không phải trang trí)

> Nguyên tắc: animation/FX chỉ đi sau luật đã chốt; animation lỗi không được kẹt game (phải có timeout/fallback).

### C1. Dissolve + decor rơi — ✅
**Là gì:** sợi len tan dần theo đường wipe + đồ trang trí trên len rơi theo đúng ngưỡng.
**Nhiệm vụ:** tan hết mới tính là gỡ xong 1 lớp; decor rơi đúng lúc, không rơi sớm/muộn.
**Bằng chứng:** `YarnPart.cs:346-404`, shader `YarnDissolve`.

### C2. Dây (rope) vật lý — ✅
**Là gì:** sợi dây cong võng, đung đưa khi bay từ len vào slot và từ box lên bảng.
**Nhiệm vụ:** nhìn mềm, tới đúng 2 đầu (slot/pointer ảnh); bay xong tự thu về pool.
**Bằng chứng:** `YarnRopeController.cs`, `Rope.cs`, `Board/RopeBoard.cs`.

### C3. Bounce/box/waiting feedback — ✅
**Là gì:** hộp nảy khi nhận sợi, vòng slot co lại, cảnh báo khi chờ còn 1 slot.
**Nhiệm vụ:** người chơi luôn biết "vừa xảy ra gì" và "sắp thua chưa".
**Bằng chứng:** `Box.cs:157-176`, `Box.cs:267-302`, `WaitingAreaManager.cs:260-282` (lưu ý: `PlayWarningLose()` còn rỗng — phải nối thật).

### C4. Camera & bàn chơi — ✅
**Là gì:** kéo xoay bàn, pinch/slider zoom, tự xoay khi rảnh, tự zoom khi gần hết len.
**Nhiệm vụ:** model nhỏ dần vẫn nhìn rõ; zoom không văng khỏi khung; recenter hiện đang tắt cứng (`AutoZoomRecenter = false`) — cần quyết định bật hay bỏ.
**Bằng chứng:** `ModelDragRotate.cs`, `GameManager.cs:350-415`, `ScaleSliderSync.cs`.

### C5. Âm thanh + rung (theo sự kiện) — ❌
**Là gì:** tiếng tap/vào slot/đầy hộp/thắng/thua + rung ngắn, bật tắt độc lập nhạc/hiệu ứng/rung.
**Nhiệm vụ:** mỗi sự kiện đúng 1 tiếng, không chồng âm khi nhiều len vào cùng lúc; toggle có hiệu lực ngay và nhớ sau load.
**Thực tế:** `KitSound` có file nhưng 0 nơi gọi; 2 hàm SFX còn rỗng.
**Việc phải làm:** nối cue vào các event đã có (Tap/SlotFilled/BoxFull/Win/Lose) + màn Settings.

---

## NHÓM D — MÀN HÌNH & ĐIỀU HƯỚNG (UI/UX)

### D1. Entry & Home — ❌
**Là gì:** màn loading (tiến trình + load save/config) rồi rẽ nhánh: màn 1 vào thẳng Gameplay, màn sau vào Home chọn màn.
**Nhiệm vụ:** route đúng 2 nhánh kể cả khi SDK lỗi/mất mạng; Home có carousel ~7 màn, Play, shop, menu, popup daily/settings chặn input xuyên.
**Việc phải làm:** dựng 2 scene + `NavigationService` (chỉ 1 nơi được đổi scene/modal); UI chỉ gửi intent, không tự quyết thắng/thua/cộng tiền.

### D2. HUD trong ván — ◐
**Là gì:** thanh trạng thái trên màn chơi.
**Nhiệm vụ:** hiện level/mode, số len còn lại, coin/heart, 3 booster, nút setting, slider scale.
**Thực tế:** mới có tên level + slider.
**Việc phải làm:** thêm từng ô HUD gắn vào service tương ứng (khi có nhóm F, E).

### D3. Popup kết quả & pause — ◐
**Là gì:** Win (thưởng + NEXT), Lose (revive/restart), Setting (tiếp/restart/về Home).
**Nhiệm vụ:** popup mở thì khóa gameplay; nút có trạng thái chờ khi gọi ad/IAP; không bấm đúp ăn 2 lần.
**Thực tế:** Win/Lose cơ bản đã chạy (`GameUIController.cs:176-268`); thiếu nhánh revive, thiếu Setting.
**Việc phải làm:** thêm nhánh revive (chờ nhóm E) + màn Setting + test double-tap.

---

## NHÓM E — BOOSTER & HỒI SINH (cứu ván) ❌ toàn bộ

| Tính năng | Là gì + nhiệm vụ |
|---|---|
| **E1. Add Slot** | +1 slot chờ (tối đa 7). Đủ 7 thì từ chối + tooltip. Kiểm tra cap, trừ kho, xếp lại layout |
| **E2. Clean** | Gom hết len chờ vào hộp tạm, vùng chờ trống để chơi tiếp. Không mất sợi nào kể cả animation/ad lỗi giữa chừng |
| **E3. Magic Box** | Biến hộp đầu hàng chờ thành hộp cầu vồng (nhận mọi màu). Chỉ tác động đúng hộp đó; đang active bấm nữa thì từ chối |
| **E4. Revive** | Thua mà còn hộp chưa mở → offer thêm hộp; hết hộp → offer Clean. Trả coin/ad, **giữ nguyên ván** (không load lại) |
| **E5. Khung an toàn chung** | Check kho → khóa input suốt giao dịch+animation → trừ 1 lần → apply → mở khóa khi xong/timeout; double-tap chỉ chạy 1 lần |

Thứ tự làm đề xuất: E1 → E2 → E4 → E3 (dễ→khó, Revive cần trước để popup Lose có nhánh thứ 2).

---

## NHÓM F — TIẾN TRÌNH & TIỀN TỆ (giữ chân) ❌ toàn bộ

### F1. Save game
**Là gì:** 1 kho lưu JSON duy nhất (màn hiện tại, coin, heart + giờ hồi, booster, remove-ads, settings, tutorial, daily).
**Nhiệm vụ:** ghi nguyên tử, có phiên bản schema + migration từ save cũ; hỏng save/mất mạng/chỉnh giờ vẫn chơi và tự khôi phục.

### F2. Coin & Heart
**Là gì:** Coin (khởi đầu 100) tiêu cho cứu ván/shop; Heart (tối đa 5, hồi 1/1800s) giới hạn lượt chơi.
**Nhiệm vụ:** thắng +coin/+màn; thua/restart −heart (trừ Infinite Heart); số dư không âm; thiếu thì hủy nguyên tử; 1 giao dịch không cộng 2 lần (kể cả kill app, callback lặp).
**Cần sếp chốt:** trừ heart lúc bấm Play hay lúc thua? hết heart thì sao? replay màn cũ được gì?

### F3. Quà hằng ngày 7 ngày
**Là gì:** điểm danh nhận coin/giờ tim/booster; x2 khi xem ad; ngày lỡ được bù bằng ad.
**Nhiệm vụ:** đúng chu kỳ, chống chỉnh giờ, timer theo giờ UTC.

---

## NHÓM G — HƯỚNG DẪN NGƯỜI MỚI (tutorial) ❌

**Là gì:** 3 màn đầu dạy dần: tap → hộp/màu → vùng chờ; sau đó dạy giữ-xem-trước, xoay bàn, từng booster đúng lúc cần.
**Nhiệm vụ:** data-driven (không code cứng), bước phụ có Skip, xong không hiện lại, hỏng đối tượng thì tự bỏ qua không treo game, không hiện đè popup/ad.
**Thực tế:** mới có 1 marker data (`TutorialTargetMarker.cs`).

---

## NHÓM H — KIẾM TIỀN (ads / IAP / shop) ❌ toàn bộ

| Tính năng | Là gì + nhiệm vụ |
|---|---|
| **H1. Ad thưởng** | Revive, x2 thưởng/daily, mua coin/booster. Chỉ trao sau callback success; ad lỗi thì không trao, không kẹt |
| **H2. Ad chen ngang** | Cuối màn/rời màn, có giới hạn tần suất; không bật sau thao tác quan trọng; không đổi game state |
| **H3. IAP** | No Ads (mua 1 lần, khôi phục được), gói tân thủ, gói coin/booster/tim. Xác thực hóa đơn, giá theo vùng |
| **H4. Shop + thống kê** | Nối UI với kho tiền; ghi event tối thiểu (vào/thắng/thua/dùng booster/tiêu tiền); lỗi SDK không crash game; core tách khỏi SDK qua interface để chơi offline được |

---

## NHÓM I — NGÔN NGỮ, CÀI ĐẶT, DỄ TIẾP CẬN ❌

* Mọi chữ là mã ngôn ngữ (không viết cứng), kiểm tra câu dài/số nhiều; key thiếu chỉ log, không crash.
* Toggle nhạc/âm/rung độc lập, hiệu lực ngay, nhớ sau load.
* Nút ≥44pt, safe-area + đa tỉ lệ màn dọc, banner không che bàn, không dùng mỗi màu sắc để truyền luật (kèm hình/ký hiệu).

---

## NHÓM K — CHẤT LƯỢNG, MÁY THẬT, BUILD (QA)

### K1. Test tự động — ◐
**Là gì:** suite chạy 1 lệnh: resolver, economy, booster, UI contract, regression (pause/kill/relaunch/mất mạng/đổi ngày).
**Nhiệm vụ:** pass/fail theo test ID, lưu log; mục tiêu 0 giao dịch trùng khi retry, 0 kẹt sau 100 lần load liên tiếp.
**Thực tế:** 1 PlayMode test popup; 16 EditMode tests của prototype cũ nằm ở evidence W1.

### K2. Máy thật + hiệu năng — ❌
Đo trên 1–2 máy Android thật: startup, load màn, FPS, memory, animation stall; touch, safe-area, màn dọc; ghi model/OS/build/kết quả.

### K3. Build phát hành — ❌
Chốt version/package/orientation/icon/signing (không commit secret); ra APK/AAB + smoke test Entry→Home/Gameplay→Win/Lose + build log.

---

## THỨ TỰ ĐỀ XUẤT (đọc từ trên xuống là làm trước)

1. **Khóa core:** commit Level6, quét full 7 màn hết đỏ, chốt flow matrix-only, thêm seed pool (A2).
2. **Feeling còn dở:** nối `PlayWarningLose`, cue âm thanh, quyết định recenter (C3–C5).
3. **Thành demo được:** save (F1) → Entry/Home (D1) → heart/coin tối thiểu (F2 + chốt câu hỏi sếp) → tutorial (G) → test máy thật (K2).
4. **Kiếm tiền:** booster E1→E2→E4→E3 → ads/IAP/shop (H) → ngôn ngữ/cài đặt (I).
5. **Bàn giao:** build (K3) + hồ sơ (báo cáo này + ảnh/video + hướng dẫn).
