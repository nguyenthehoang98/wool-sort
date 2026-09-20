# F08 — Ads, IAP và shop

## Rewarded ads

Các placement hiện có/đề xuất:

- Revive level.
- Nhân đôi coin thắng.
- Nhân đôi daily reward.
- Nhận coin/heart/booster.
- Mua thêm lựa chọn cứu ván.

Ad chỉ được xem là thành công khi SDK trả callback hoàn tất. Đóng sớm hoặc lỗi mạng không nhận reward.

## Interstitial

Có thể xuất hiện ở mốc chuyển level hoặc rời gameplay theo frequency cap. Không xuất hiện khi người chơi đang thao tác quan trọng hoặc vừa mới xem ad khác.

## IAP

Nhóm sản phẩm:

- No Ads.
- No Ads Bundle.
- Beginner Bundle.
- Coin packs.
- Booster/heart bundles.

## Shop UX

- Mỗi card nêu rõ nhận được gì.
- Hiển thị giá/store localized price.
- Purchase button có loading và chống bấm lặp.
- Purchase success hiển thị toàn bộ entitlement đã nhận.
- Restore purchase cho No Ads.

## Ranh giới sản phẩm

Monetization hỗ trợ người chơi, không được thay core puzzle thành pay-to-win bắt buộc. Tất cả offer phải có fallback offline rõ ràng.

![Revive offer reference](assets/screenshots/Popup%20Lose%20Revive%202.png)
