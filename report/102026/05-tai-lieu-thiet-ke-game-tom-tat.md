# TÀI LIỆU THIẾT KẾ GAME — WOOL SORT MASTER (bản tóm tắt)

> Tổng hợp từ: `gamedesign/00–09`, `docs/01–11`, `ai-docs/01–10`, `tasks/plan.md`.
> Cách đọc: đi theo đúng hành trình người chơi, từ mở app đến chơi – thua – nạp tiền – quay lại.

---

## CỤM 1 — MỞ APP & VÀO GAME (Entry)

**Người chơi thấy gì:** màn loading có thanh tiến trình → tự chuyển màn, không cần bấm gì.

| Tình huống | Flow |
|---|---|
| Mở app lần đầu (level 1) | Loading (load config + save + SDK + audio) → vào thẳng **Gameplay level 1** |
| Đã chơi (level ≥ 2) | Loading → vào **Home** |
| Mất mạng / SDK lỗi | Vẫn vào game bình thường, ẩn các offer cần mạng (core không phụ thuộc SDK) |

---

## CỤM 2 — MÀN HÌNH CHÍNH (Home)

**Người chơi thấy gì:** danh sách level dạng carousel (~7 màn quanh level hiện tại), nút Play, shop, thanh menu, popup quà hằng ngày / cài đặt.

* Level chưa mở khóa thì không bấm chơi được.
* Nút Play vào đúng level đã chọn.
* Popup (daily, settings…) nổi lên thì **chặn hết input xuống phía dưới**, không bấm xuyên được.
* Thanh menu: shop – cài đặt – các tính năng phụ.

---

## CỤM 3 — VÒNG LẶP CHƠI CHÍNH (Gameplay — trái tim của game)

Flow mỗi lượt bấm: **Quan sát → chọn len → vào hộp/vùng chờ → mở lớp trong → đầy hộp → tô ảnh → lặp lại → thắng (hoặc thua).**

### 3.1. Luật tap len

* Chỉ tap được **lớp len ngoài cùng** (đang hiện). Lớp trong hiện ra khi lớp ngoài bị gỡ (thứ tự `Hidden → OnBoard`).
* Tap 1 cái: sợi len bay (dây cong võng/đung đưa) vào slot, lớp ngoài tan dần (dissolve), đồ trang trí trên len rơi xuống.
* **Nhấn giữ 0.3 giây:** lớp ngoài thành trong suốt để **nhìn trộm màu lớp trong**, thả tay trả lại như cũ.
* Tap vào chỗ không hợp lệ (không còn slot nhận, len đang tan…) = **không làm gì cả** (no-op), không trừ tài nguyên, không kẹt game.

### 3.2. Luật vào hộp / vùng chờ (slot)

* Mỗi **hộp có 3 slot**, mỗi slot 1 sợi. 3 sợi **cùng màu** đầy hộp → hộp khóa, đóng nắp, bay đi tô ảnh, hộp mới ra.
* Thứ tự tìm chỗ cho sợi mới tap:
  1. Hộp **cùng màu còn trống**, ưu tiên hộp đã có nhiều sợi nhất (hòa thì hộp ra trước).
  2. Không có hộp nào nhận → vào **vùng chờ**, xếp từ **trái sang phải**.
* Hộp cầu vồng (Rainbow) nhận mọi màu nhưng không đổi màu sợi len.
* Vùng chờ **mặc định 5 slot, tối đa 7**. Vùng chờ đầy hết mà trên bàn còn len → **thua**.

### 3.3. Tô ảnh (phần thưởng thị giác)

* Mỗi hộp đầy bắn 1 sợi dây lên bảng, tô dần từng cụm pixel đúng màu len lên ảnh 84x84.
* Tô xong toàn ảnh đúng lúc hết len = thắng trọn vẹn.

### 3.4. Thắng / thua / chơi lại

| Kết quả | Điều kiện | Sau đó |
|---|---|---|
| **Thắng** | Hết sợi len + đóng hộp cuối | Popup YOU WIN → nhận thưởng (thường hoặc x2 khi xem ad) → sang màn mới |
| **Thua** | Vùng chờ đầy | Popup LEVEL FAILED → **hồi sinh** (giữ nguyên ván, xem cụm 4) hoặc **chơi lại từ đầu màn** |
| Chơi lại | Bấm RETRY/Restart | Ván reset hoàn toàn, không cộng/trừ nhầm tài nguyên; mỗi sự kiện thắng/thua chỉ phát **đúng 1 lần** |

### 3.5. Tương tác phụ trong ván

* Kéo xoay bàn len, thanh trượt + chụm tay phóng to/thu nhỏ (có giới hạn theo màn), bàn tự xoay khi rảnh, tự zoom lại khi gần hết len.
* Khóa input khi: animation mở màn, popup mở, đang xem ad, đang chạy booster.
* Cảnh báo khi vùng chờ chỉ còn 1 slot trống; màn Hard/VeryHard có cảnh báo riêng.

---

## CỤM 4 — CỨU VÁN: VÙNG CHỜ, BOOSTER & HỒI SINH

### 4.1. Ba booster trong ván

| Booster | Tác dụng | Luật quan trọng |
|---|---|---|
| **Add Slot** | +1 slot chờ (tối đa 7) | Đủ 7 rồi thì từ chối + hiện tooltip |
| **Clean** | Gom hết len đang chờ vào hộp tạm, vùng chờ trống để chơi tiếp | Không được mất sợi nào kể cả khi animation/ad lỗi giữa chừng |
| **Magic Box** | Biến hộp đầu hàng chờ thành hộp cầu vồng | Chỉ tác động đúng hộp đó; đang active mà bấm nữa thì từ chối |

* Dùng booster tốn 1 lượt trong kho (hoặc xem ad/trả coin). Bấm đúp chỉ tính 1 lần. Ad/coin lỗi → giữ nguyên trạng thái + kho đồ.

### 4.2. Hồi sinh khi thua (Revive)

* Thua mà **còn hộp chưa mở** → gợi ý hồi sinh kiểu **thêm hộp**.
* Thua mà **hết hộp** → gợi ý hồi sinh kiểu **dọn vùng chờ (Clean)**.
* Trả phí bằng coin hoặc xem ad → **giữ nguyên ván đang chơi**, không load lại (trừ khi người chơi tự chọn Restart).

---

## CỤM 5 — TIẾN TRÌNH & TÀI NGUYÊN (chơi lâu dài)

* **Mở màn:** thắng màn hiện tại mới mở màn kế tiếp. Màn có 3 độ khó **Normal / Hard / VeryHard**, khó tăng dần (nhiều màu hơn, nhiều lớp ẩn hơn, màu đan xen nhau, ít slot chờ hơn…).
* **Mỗi màn phải có ít nhất 1 đường giải không cần booster**, thua thì rõ lý do, không đòi hỏi bấm pixel-perfect.
* **Tiền tệ:**
  * **Coin** (khởi đầu 100): thắng màn, offer, shop — dùng để cứu ván/mua đồ.
  * **Heart** (tối đa 5, hồi 1 tim mỗi 1800s): giới hạn lượt chơi; thua/restart tốn tim (trừ khi đang có Infinite Heart).
  * **Infinite Heart** (tính theo phút): từ quà hằng ngày/gói bundle.
* **Quà hằng ngày 7 ngày:** coin / giờ tim / booster; điểm danh thường, **x2 khi xem ad**, ngày bỏ lỡ được bù bằng ad. Chống chỉnh giờ máy để gian lận.
* Quy tắc sắt: số dư không bao giờ âm; trừ tiền thiếu thì hủy nguyên tử (không trừ nửa vời); cùng 1 giao dịch không bao giờ cộng 2 lần (kể cả kill app giữa chừng, callback ad lặp).

---

## CỤM 6 — HƯỚNG DẪN NGƯỜI MỚI (Tutorial)

* 3 màn đầu dạy dần: **màn 1 tap len → màn 2 hộp/màu/hộp đầy → màn 3 vùng chờ giới hạn**; sau đó dạy giữ-xem-trước, xoay bàn, rồi từng booster **đúng lúc cần** (không dồn 3 booster vào 1 popup).
* Bước phụ được **bỏ qua (Skip)**; tutorial đã xong không hiện lại sau khi load lại game.
* Tutorial hỏng (mất đối tượng) thì tự bỏ qua, không treo game; không hiện tutorial đè lên popup/ad/animation mở màn.

---

## CỤM 7 — KIẾM TIỀN (Ads / IAP / Shop)

* **Quảng cáo thưởng (tự nguyện xem):** hồi sinh, x2 thưởng thắng, x2 quà hằng ngày, mua coin/booster/tim. **Chỉ trao thưởng sau khi ad báo thành công.**
* **Quảng cáo chen ngang:** cuối màn/rời màn, có giới hạn tần suất; không bật ngay sau thao tác quan trọng, không bật khi chưa rõ đồng ý.
* **Mua trong app (IAP):** gói tắt quảng cáo (mua 1 lần, khôi phục được khi đổi máy), gói tân thủ, gói coin, gói booster/tim. Đơn hàng phải xác thực hóa đơn, giá hiển thị theo ngôn ngữ/vùng.
* **Shop:** mua coin/booster/tim bằng tiền hoặc xem ad; lỗi mạng/lỗi store không được crash game, không mất/không nhân tài nguyên.

---

## CỤM 8 — CÀI ĐẶT, NGÔN NGỮ, ÂM THANH, DỄ TIẾP CẬN

* Cài đặt: **ngôn ngữ, nhạc, âm hiệu ứng, rung** — bật/tắt độc lập, có hiệu lực ngay, nhớ sau khi load lại.
* Mọi chữ trong game là **mã ngôn ngữ** (không viết cứng), kiểm tra câu dài/số nhiều ở mọi ngôn ngữ.
* Âm riêng cho tap/vào slot/đầy hộp/booster/thắng/thua; rung ngắn tắt được; nhiều len vào cùng lúc không chồng âm.
* Dễ tiếp cận tối thiểu: nút đủ to (≥44pt), popup chặn tap xuyên, banner không che bàn chơi, **không dùng mỗi màu sắc để truyền luật** (kèm hình/ký hiệu), chữ không bị cắt ở màn nhỏ.

---

## CỤM 9 — YÊU CẦU NỀN (kỹ thuật, người chơi không thấy nhưng bắt buộc)

* **Lưu game:** 1 kho lưu duy nhất, có phiên bản schema, ghi nguyên tử; hỏng save/mất mạng/chỉnh giờ vẫn chơi và tự khôi phục.
* **Level:** mỗi màn có ID/tài nguyên/mode/seed riêng; thứ tự chơi tách khỏi ID asset; thêm màn không sửa code core; thiếu asset phát hiện **trước khi build**.
* **Chơi lại determinist:** cùng seed + cùng thao tác cho cùng kết quả (để QA máy lặp lại được ván lỗi).
* **Thống kê + cấu hình từ xa:** ghi sự kiện (vào/thắng/thua/chơi lại/dùng booster/tiêu-tiền…), chỉnh offer/tần suất ad/phần thưởng từ xa mà **không đổi luật puzzle giữa ván đang chơi**; lỗi thống kê không được crash game; không gửi thông tin nhạy cảm.

---

## CỤM 10 — TIÊU CHÍ NGHIỆM THU (đạt là giao được)

1. Người mới tự qua được màn 1 không cần chỉ thêm.
2. Tap đúng có phản hồi ngay; không đặt 2 lần, không mất len.
3. Thắng/thua phát đúng 1 lần; tiếp tục/chơi lại luôn được.
4. Mở màn mới + tắt/mở lại game vẫn giữ đủ tiến trình/tài nguyên.
5. Booster phân biệt rõ nhau; animation/ad lỗi giữa chừng không kẹt, không mất đồ; hồi sinh giữ nguyên ván.
6. Chạy dọc nhiều tỉ lệ màn hình; popup chặn tap xuyên; nút ad/IAP có trạng thái chờ rõ ràng.
7. Tắt nhạc/âm/rung có hiệu lực ngay; thiếu asset báo trước build; thêm level không sửa core.
8. Lỗi mạng/ad/IAP/thống kê không crash game; chơi offline được phần core.
9. **Ngoài phạm vi bản đầu:** chơi mạng, lưu cloud bắt buộc, battle pass, xếp hạng online, chợ đồ, đấu trường.
