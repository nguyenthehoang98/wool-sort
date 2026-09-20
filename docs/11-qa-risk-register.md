# QA — Kiểm thử và rủi ro

## Luồng hồi quy bắt buộc

- Entry → Gameplay ở level 1.
- Entry → Home → chọn level → Gameplay ở level > 1.
- Tap các màu đúng/sai, inner layer, hold preview, rotate/scale.
- Box đầy, box đóng, box mới spawn, hết level.
- Waiting đầy, lose popup, revive bằng add box/clean, restart.
- Ba booster, inventory 0, ad success/failure, coin đủ/thiếu.
- Win reward thường/x2, daily reward, missed day/recover.
- Save/load sau pause, kill app, mất mạng, đổi ngày.
- No Ads purchase/restore và purchase callback lặp.
- Đổi ngôn ngữ, sound/music/vibration và safe area.

## Rủi ro cao

| Rủi ro | Tác động | Cách giảm |
|---|---|---|
| Animation không callback | Kẹt win/lose/box | State không phụ thuộc tuyệt đối animation; timeout/fallback |
| Random level không replay được | Không debug được deadlock | Seed per session và ghi command log |
| PlayerPrefs hỏng/chỉnh giờ | Mất tiến trình/gian lận reward | Versioned save, checksum/UTC, migration test |
| SDK ad/IAP lỗi | Không nhận reward hoặc mất doanh thu | Adapter, idempotent transaction, offline fallback |
| Level data lệch asset ID | Crash/màn trống | Editor validator + build preflight |
| Singleton còn state cũ | Bug khi reload scene | Composition root reset và test load level liên tiếp |
| Màu khó phân biệt | UX/accessibility kém | Pattern/shape/contrast test |

## Chỉ số kiểm thử đề xuất

- 100% luật resolver có test branch.
- 0 duplicate economy transaction trong test retry.
- 0 stuck session sau 100 level load liên tiếp trong smoke test.
- App không crash khi ad, IAP, analytics fail.
