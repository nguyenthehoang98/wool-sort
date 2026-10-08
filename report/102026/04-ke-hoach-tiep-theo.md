# KẾ HOẠCH TIẾP THEO (đề xuất sau báo cáo 10/2026)

> Thứ tự ưu tiên theo nguyên tắc: **chặn phát hành trước, tính năng hút tiền sau, polish cuối**.
> Tham chiếu backlog: `tasks/todo.md` (W2→W9), tài liệu: `tasks/plan.md`.

## Giai đoạn A — Khóa core hiện tại (1–2 ngày, làm ngay)

| # | Việc | Vì sao trước | Xong khi nào |
|---|---|---|---|
| A1 | Commit `Level6.prefab` đang sửa dở (hoặc hoàn tất rồi commit) | File treo ngoài git, dễ mất/khó review | `git status` sạch |
| A2 | Chạy `Quét full` tab 3 cho cả 7 level, xử lý hết lỗi đỏ còn lại | Không để content lỗi lọt vào build nội bộ | 7/7 level báo OK |
| A3 | Quyết định số phận `Resources/Images/` (đã dỡ): giữ flow matrix-only thì xóa hẳn fallback đọc ảnh trong `JamImage`/`BoxImageColorValidator`, tránh log lỗi gây nhiễu | Code còn 2 luồng đọc ảnh/texture thừa | 1 luồng duy nhất |
| A4 | Bổ sung seed cho pool random (`Random.InitState` theo level + số lần chơi) | Bước rẻ nhất tiến tới deterministic W5-04; QA lặp lại được ván lỗi | Cùng seed → cùng layout inner |

## Giai đoạn B — Thành "sản phẩm tối thiểu phát hành được" (1–2 tuần)

| # | Việc (map backlog) | Nội dung chính |
|---|---|---|
| B1 (W3) | Save + progression tối thiểu | `GameData` JSON: level hiện tại, coin, heart + hồi giờ; lưu khi thắng/thua; load khi mở game |
| B2 (W3) | Entry + Home tối thiểu | Entry route (level 1 → Gameplay, còn lại → Home); Home list 7 level + nút Play; quay lại Home từ pause |
| B3 (W3) | Luật heart/coin tối thiểu + chốt câu hỏi mở | Cần sếp chốt: trừ heart khi nào? thua có mất gì? replay màn cũ được gì? (mở tại `docs/03-progression-resources.md`) |
| B4 (W6) | Tutorial 3 level đầu chạy thật | Tap → box → waiting → hold/rotate, data-driven + skip, không lặp sau reload |
| B5 (W8) | Build nội bộ + test tay theo checklist | APK debug, chạy hết 7 level trên 1–2 máy thật, ghi bug log có reproduction |

Xong giai đoạn B là có bản **demo được cho sếp/tester ngoài**, dù chưa có ads/IAP.

## Giai đoạn C — Kiếm tiền và hoàn thiện (theo W6–W7)

1. Booster theo thứ tự dễ→khó: Add Slot → Clean → Magic Box → Revive (giữ đúng state machine + chống double-tap trong `ai-docs/04-boosters-contract.md`).
2. Rewarded ads cho revive/x2 + interstitial có frequency cap; IAP No Ads + coin pack; analytics event tối thiểu (`level_started/completed/failed`, `booster_used`).
3. Localization key, audio theo sự kiện, settings toggle, safe-area/multi-resolution.

## Giai đoạn D — Bàn giao đồ án (W9, dùng luôn báo cáo này)

* W9-01/02: chốt Android build config + APK/AAB + smoke log.
* W9-03: báo cáo này + 3 file chi tiết chính là khung evidence pack; bổ sung ảnh màn hình 7 level + video 1 ván full.
* W9-04/05: hướng dẫn cài/chơi, slide bảo vệ, known limitations (lấy từ file 03 + mục 8 file 02).

## Việc cần sếp quyết (không tự chốt được)

1. **Câu hỏi economy** (`docs/03-progression-resources.md`): trừ heart lúc bấm Play hay lúc thua? hết heart thì sao? replay màn cũ có thưởng không?
2. **Ma trận máy test Android** (todo `[!]`): cần ít nhất 1–2 máy thật trước W8.
3. **Lịch 5 ngày/tuần** trong `plan.md` có còn đúng không (todo `[!]`) — ảnh hưởng ngày bàn giao các tuần còn lại.
4. Scope booster: làm cả 3 + revive, hay chỉ Add Slot + Revive cho kịp bảo vệ?
