# F08 — Ngôn ngữ, âm thanh và cài đặt

## Hiện tại

- I2 Localization cung cấp danh sách ngôn ngữ và TextMeshPro target.
- `LanguageManager` lưu lựa chọn trong `PlayerPrefs`; default hiện là English.
- Setting có sound, music, vibration; audio source persistent qua scene.
- Rung dùng LFramework Nice Vibrations và có các loại impact.

## Thiết kế lại

Tạo `SettingsState` với các field `language`, `soundEnabled`, `musicEnabled`, `vibrationEnabled`, sau đó persist một lần qua `SettingsStore`.

Các system nhận event thay đổi setting; popup không thao tác trực tiếp vào AudioSource. Mọi string UI là localization key, không nối chuỗi nội dung trong code. Format số, plural và text dài phải được kiểm tra trên mọi ngôn ngữ.

## Accessibility tối thiểu

- Toggle có label và trạng thái rõ.
- Có thể tắt rung và âm thanh độc lập.
- Không truyền luật bằng màu duy nhất.
- Text/CTA không bị cắt trên màn hình nhỏ hoặc khi font lớn.
- Popup có thứ tự focus/hit target đủ lớn cho touch.
