# 00 — Phân tích trạng thái hiện tại

## Snapshot đã đọc

- Project Unity: `6000.3.21f1` trong `ProjectSettings/ProjectVersion.txt`.
- Build scenes: `EntryScene`, `Home`, `Gameplay`.
- Core gameplay nằm trong `Assets/_Game/Scripts`.
- 156 C# file trong `Assets/_Game`; có thêm SDK/package lớn bên ngoài.
- 300 level definition trong `Resources/Configs/GeneralConfig.asset`, nhưng thứ tự chơi hiện tại chỉ có 88 entry.
- 94 prefab trong `Assets/Resources/Levels`, 172 level FBX trong `Assets/_Game/Mesh`, 83 prefab backup.
- 21 màu len thường (`YarnColor`), thêm `None` và `Rainbow`.
- GameConfig hiện tại: 5 slot chờ ban đầu, tối đa 7; dissolve 0.85 giây; hold 0.3 giây.
- Booster: `AddSlot`, `Clean`, `MagicBox`.
- Tài nguyên: coin, heart, infinite heart, booster inventory, remove ads.

## Luồng scene

1. `EntryScene`: load `GameData`, heart, config, SDK, audio; sau loading chuyển vào `Gameplay` nếu level = 1, ngược lại vào `Home`.
2. `Home`: danh sách các level gần level hiện tại, shop/menu, daily reward và nút play.
3. `Gameplay`: load prefab `Resources/Levels/Level{realLevel}`, khởi tạo board, box, waiting area, HUD và tutorial.

## Kiến trúc đang có

- Nhiều singleton runtime: `GameManager`, `BoxManager`, `SlotManager`, `WaitingAreaManager`, `BoosterManager`, `YarnRopeController`, `GameData`.
- `GameManager` đang giữ cả level lifecycle, dữ liệu YarnPart, thắng/thua, chọn màu box, tutorial và analytics.
- Sự kiện global (`UnityEventBus`, `GameConst`) kết hợp callback trực tiếp.
- Animation/FX có vai trò trong logic: box chỉ đóng sau animation slot; rope tự retract/despawn.
- `GameData` lưu JSON trong `PlayerPrefs`; IAP, ad và analytics gọi trực tiếp từ gameplay/UI.
- Level là prefab 3D; layer len được spawn dần khi click, không tạo toàn bộ visual từ đầu.

## Điểm tốt cần giữ

- Core puzzle có chiến lược data rõ: `YarnPartInfo`, layer, màu, box/slot.
- Level order cho phép thay đổi thứ tự chơi mà không đổi asset ID.
- Có pooling cho box/rope.
- Có tutorial theo step, daily reward, heart, booster, localization, IAP và analytics.
- Cảm giác chơi được đầu tư: dissolve, rope sag/sway, bounce, rotate/zoom, warning và VFX.

## Nợ kỹ thuật cần xử lý khi redesign

| Vấn đề | Bằng chứng | Hướng xử lý |
|---|---|---|
| Logic và presentation trộn nhau | `GameManager.cs`, `Box.cs`, `Slot.cs` gọi trực tiếp animation, sound, SDK | Đưa luật vào pure state/resolver; animation là adapter phản hồi event |
| Singleton/global state dày | Nhiều `Instance`, `GlobalBus`, `GameConst` | Chỉ giữ một composition root; inject service theo feature |
| Save phân tán | `GameData.cs`, nhiều `PlayerPrefs` key | Một `SaveStore`, versioned schema, ghi nguyên tử |
| Luật khó kiểm thử | `SlotManager` quyết định slot dựa trên runtime list và thứ tự | Resolver thuần C# nhận state và trả command/result |
| UI khóa/mở ở nhiều nơi | `GameManager`, `GameUIController`, popup, booster | Một `InteractionGate` theo state session |
| Phụ thuộc SDK trong core | `KitSDK` xuất hiện ở popup/gameplay | Adapter `AdsService`, `PurchaseService`, `AnalyticsService` |
| Dữ liệu level thiếu một nguồn chuẩn | FBX, prefab, `GeneralConfig`, backup tồn tại song song | Chuẩn hóa `LevelDefinition` và validator Editor |
| Có dấu hiệu cấu hình không khớp | `WaitingAreaManager.RefreshLayout()` đang đặt `canAdd = false` dù có AddSlot | Xác định lại rule sản phẩm và thêm test cho booster |

## Mâu thuẫn cần chốt

Tài liệu Word ghi Unity `2022.3.50f1`, còn project hiện tại ghi Unity `6000.3.21f1`. Redesign phải lấy `ProjectVersion.txt` làm nguồn kỹ thuật; chỉ dùng tài liệu Word cho bối cảnh đồ án.

## Nguyên tắc redesign

1. Core game không biết Unity UI, DOTween, SDK hay PlayerPrefs.
2. Một command hợp lệ chỉ được xử lý một lần; animation không tự quyết định luật.
3. Mọi thay đổi resource/progression đi qua một ledger/service duy nhất.
4. Level phải deterministic nếu dùng cùng seed và cùng input.
5. Đầu tiên thay logic cốt lõi; giữ asset/animation cũ để giảm rủi ro, chưa xây hệ thống server hay Addressables nếu chưa cần.
