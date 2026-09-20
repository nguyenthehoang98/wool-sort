# F06 — Tutorial và onboarding

## Cơ chế hiện tại

`TutorialSequenceController` ghép các `TutorialBase` theo `levelIndex` và trigger:

- `OnLevelStart`: chạy tuần tự.
- `OnCustomEvent`: đợi event one-shot như clean booster tutorial.

Các tutorial hiện có hướng dẫn tap/hold, fill wool, rotate camera, preview layer, booster add hole, clean, magic box và complete level. `TutorialState` chặn input không hợp lệ theo step.

## Thiết kế lại

Tutorial phải là data-driven:

- `tutorialId`, `trigger`, `steps`, `target`, `completionCondition`, `skippable`.
- Step chỉ mô tả intent; UI/pointer là renderer.
- Khi bắt đầu level, tutorial đọc session state chứ không phụ thuộc hard-code nhiều singleton.
- Cờ đã xem lưu theo version tutorial; sửa tutorial lớn thì có thể chạy lại.

## Nguyên tắc

- Không chặn người chơi vô hạn nếu target biến mất.
- Input của tutorial phải đi qua cùng command pipeline với input bình thường.
- Không hiển thị tutorial nếu popup/ad hoặc animation đầu level đang active.
- Có nút bỏ qua ở những step không bắt buộc.
- Test được tutorial bằng event giả và không cần touch thật.
