# F00 — Project foundation and placeholder reskin setup

## 1. Mục tiêu
- Target project là `game/`, dùng Unity `6000.3.21f1`.
- Có placeholder art/theme để làm gameplay trước khi có final art.
- Có ranh giới OOP rõ để reskin không chạm vào luật puzzle.

## 2. Luồng xử lý
```text
ThemeProfile asset -> Presentation views -> placeholder UI/pieces
```

Gameplay state chưa được implement ở foundation; không đưa luật puzzle vào theme.

## 3. Thiết kế OOP
- `Runtime/Core`: pure puzzle state/rules, không phụ thuộc Unity.
- `Runtime/Application`: use cases và ports.
- `Runtime/Presentation`: Unity views, input, animation.
- `Runtime/Platform`: adapter save/ads/IAP/audio/localization.
- `ThemeProfile`: dữ liệu visual dùng chung, không chứa gameplay state.

## 4. File và điểm vào
- Theme contract: `game/Assets/_Game/Runtime/Presentation/ThemeProfile.cs`
- Placeholder theme: `game/Assets/_Game/Data/Themes/Theme_Placeholder.asset`
- Layout rules: `game/Assets/_Game/README.md`
- Placeholder rules: `game/Assets/_Game/Art/Placeholder/README.md`
- Feature summary contract: `docs/feature-summary-template.md`

## 5. Placeholder và reskin
- Theme mặc định dùng palette phẳng, không phụ thuộc asset reference.
- Reskin cơ bản: thay values trong `Theme_Placeholder.asset`.
- Placeholder sprite/prefab được đặt trong `Assets/_Game/Art/Placeholder` và đặt tên với tiền tố `PH_`.
- Khi có view đầu tiên, view phải nhận `ThemeProfile` qua serialized reference; không hard-code màu trong resolver.

## 6. Verification
- `ProjectSettings/ProjectVersion.txt` khớp `6000.3.21f1`.
- Unity batch import/compile và tạo `Theme_Placeholder.asset` pass.
- Chưa có scene smoke test vì tuần 1 mới chốt contract; sẽ bổ sung khi có feature runtime đầu tiên.

## 7. Thay đổi so với reference
- Chỉ dùng reference để tham khảo hành vi và tài liệu.
- Không copy C# hoặc kiến trúc singleton/SDK coupling từ reference.
