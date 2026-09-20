# F09 — Art, animation, audio và localization

## Art direction

- 3D casual, màu sắc rõ, vật liệu gỗ/len tạo cảm giác thủ công.
- Màu len là tín hiệu chính nhưng cần pattern/shape hỗ trợ người khó phân biệt màu.
- Board đủ tương phản với nền và UI.
- Các mode Hard/Very Hard có badge/VFX khác biệt, không chỉ đổi chữ.

## Animation language

- Motion mềm, có anticipation và overshoot nhẹ.
- Len dissolve theo hướng dọc, decoration rơi ra.
- Rope có độ võng và đung đưa để làm rõ liên kết len–slot.
- Box bounce khi nhận len, close khi đủ.
- Win/lose có animation chữ và VFX ngắn, không kéo dài nhịp chơi.

## Audio/haptics

- Tap/slot/box complete/booster/win/lose có cue riêng.
- Music nhẹ, loop, có thể tắt độc lập SFX.
- Vibration ngắn cho hành động quan trọng; người chơi tắt được.
- Không phát âm thanh chồng vô hạn khi nhiều len được refill.

## Localization

- Default English hiện tại.
- Tất cả CTA, popup, tooltip, reward và warning dùng localization key.
- Kiểm tra text dài, số, plural và safe area ở các ngôn ngữ mục tiêu.
- Không ghép text trong code bằng chuỗi cố định.
