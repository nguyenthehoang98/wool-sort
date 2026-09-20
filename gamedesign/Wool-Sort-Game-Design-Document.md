# Wool Sort Master — Game Design Document

> Bản thiết kế sản phẩm dành cho product, game designer, art, QA và người duyệt game. Không phải tài liệu triển khai code.

## 1. Tóm tắt

Wool Sort Master là game puzzle mobile casual về phân loại các lớp len nhiều màu vào hộp tương ứng. Người chơi quan sát, chọn lớp ngoài cùng, tận hưởng animation gỡ len và quản lý vùng chờ hữu hạn.

## 2. Core loop

`Quan sát → chọn len → đưa vào hộp/vùng chờ → mở lớp trong → hoàn tất hộp → lặp lại → thắng hoặc revive khi thua`.

Hộp ưu tiên màu phù hợp và hộp đã có nhiều len nhất. Nếu không có hộp phù hợp, len vào waiting slot từ trái sang phải. Hộp hiện tại có 3 slot; vùng chờ mặc định có 5 slot, tối đa 7.

## 3. Level

Level gồm board 3D, root yarn, các layer ẩn, màu, hộp, thứ tự campaign, thumbnail và mode. Normal/Hard/Very Hard tạo nhịp khó tăng dần. Level thủ công được ưu tiên ở MVP để đảm bảo chất lượng đường giải.

## 4. Progression và economy

Người chơi mở level kế tiếp sau khi thắng. Coin dùng cho offer/cứu ván/shop. Heart giới hạn lượt chơi và hồi theo thời gian. Infinite Heart đến từ daily reward/bundle. Daily reward chạy chu kỳ 7 ngày, có claim thường, x2 bằng ad và recover ngày bỏ lỡ.

## 5. Booster và revive

- **Add Slot:** thêm một waiting slot.
- **Clean:** đưa toàn bộ len chờ vào temporary box.
- **Magic Box:** biến hộp đầu thành hộp cầu vồng.
- **Revive:** thêm hộp nếu còn hộp chưa mở; nếu không thì Clean.

Booster cần feedback hình ảnh rõ, khóa double tap và không làm mất state nếu ad/animation thất bại.

## 6. Màn hình

Loading, Home, Gameplay HUD, Win, Lose/Revive, Shop, Daily Reward, Settings. Gameplay là trọng tâm; popup không được che state quan trọng hoặc nhận input xuyên xuống board.

## 7. Onboarding

Ba level đầu dạy tap, màu/hộp và vùng chờ. Sau đó dạy hold preview, rotate và từng booster tại đúng thời điểm. Tutorial phụ có thể bỏ qua; tutorial đã hoàn thành không lặp lại.

## 8. Monetization

Rewarded ads phục vụ revive, nhân đôi reward, daily reward và offer tài nguyên. Interstitial dùng có frequency cap. IAP gồm No Ads, bundle, coin pack, booster/heart bundle. Reward chỉ trao sau callback success/verified.

## 9. Art/audio/localization

Phong cách 3D casual, màu len rõ, vật liệu gỗ/len, animation mềm và âm thanh xác nhận. Cần hỗ trợ sound/music/vibration toggle, safe area, localization key và tín hiệu không phụ thuộc màu duy nhất.

## 10. Tiêu chí nghiệm thu

Game phải dễ hiểu ở level đầu, không duplicate placement, không mất tài nguyên, save/load ổn định, booster/revive giữ nguyên ván, chạy được trên màn hình dọc, và vẫn chơi được khi ad/IAP/analytics lỗi.

## 11. Tài liệu tham khảo hình ảnh

Các hình trong bản DOCX lấy từ screenshot hiện có trong `Assets/_Game/Previews` của project snapshot. Đây là reference của bản hiện tại; khi redesign có thể dùng làm baseline để so sánh, không mặc định là UI cuối cùng.
