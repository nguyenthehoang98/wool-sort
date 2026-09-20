# F09 — Analytics và cấu hình từ xa

## Event đang có

`Databuckets` ghi level start/exit/reopen/end, resource earn/spend, IAP show/click/purchase; SDK ngoài còn theo dõi ad và AppsFlyer/Firebase.

## Event contract đề xuất

Mỗi event có `eventName`, `sessionId`, `levelOrder`, `levelAssetId` nếu có, `placement`, `mode`, `result`, timestamp và schema version. Không gửi receipt, device identifier không cần thiết hoặc nội dung nhạy cảm.

Event quan trọng:

- `level_started`, `level_completed`, `level_failed`, `level_restarted`, `level_exited`.
- `piece_selected`, `booster_used`, `revive_used`.
- `resource_earned`, `resource_spent`.
- `ad_requested`, `ad_completed`, `ad_failed`.
- `purchase_started`, `purchase_verified`.

## Remote config

Có thể điều chỉnh offer, ad frequency, reward, mode schedule và feature flag. Giá trị phải có default local, range validation, version và fallback khi fetch lỗi. Remote config không được thay đổi luật puzzle giữa một session đang chơi.

## Privacy và debug

- Queue event offline, retry có giới hạn.
- Không crash gameplay nếu analytics lỗi.
- Có log local cho transaction nhưng không ghi secret/receipt thô.
- Có cờ debug để tắt SDK trong Editor và test deterministic.
