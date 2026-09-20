# F07 — Quảng cáo, IAP và shop

## Tính năng hiện tại

- Rewarded ad: revive, mua coin/booster/heart, nhân đôi daily reward, nhân đôi coin thắng.
- Interstitial: thường ở cuối level hoặc một số luồng rời màn.
- App-open/banner mediation.
- IAP: No Ads, No Ads Bundle, Beginner Bundle, các gói coin và bundle booster/heart.
- Purchase success popup hiển thị nội dung đã nhận.
- Receipt validation có mặt trong `IAPManager`.

## Nguyên tắc redesign

Core game không gọi trực tiếp `KitSDK`, `MediationManager` hoặc Unity IAP. Dùng interface adapter:

- `IAdsService.ShowRewarded(placement)` trả về `RewardResult`.
- `IAdsService.ShowInterstitial(placement)` không làm thay đổi game state.
- `IPurchaseService.Purchase(productId)` trả về entitlement verified.
- `IEconomyService.Grant(transaction)` là nơi duy nhất cộng phần thưởng.

## Quy tắc an toàn

- Chỉ grant sau callback success và validation.
- Mỗi purchase/được thưởng có transaction id idempotent.
- No Ads là entitlement persistent, restore được.
- Không dùng giá/entitlement hard-code trong UI.
- Khi SDK chưa init hoặc offline: game core vẫn chơi được; chỉ ẩn offer phụ thuộc mạng.
- Không hiển thị quảng cáo ngay sau một input quan trọng hoặc khi người chơi đang xem lose state nếu không có consent/điều kiện rõ.

## Không đưa vào bản đầu

Không xây ad server, remote monetization editor hay AB test framework riêng khi SDK/remote config hiện tại chưa có nhu cầu đó.
