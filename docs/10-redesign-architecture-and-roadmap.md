# ARCH — Kiến trúc redesign và lộ trình

## Kiến trúc đề xuất

```text
Unity Composition Root
├── Gameplay Core (pure C#)
│   ├── LevelDefinition
│   ├── PuzzleState
│   ├── PuzzleResolver
│   └── OutcomeRules
├── Presentation
│   ├── BoardView / BoxView / SlotView / RopeView
│   ├── GameHudView / PopupView
│   └── Animation adapters
├── Application services
│   ├── LevelSessionService
│   ├── EconomyService
│   ├── ProgressionService
│   └── TutorialService
└── Platform adapters
    ├── SaveStore
    ├── AdsService
    ├── PurchaseService
    ├── AnalyticsService
    ├── LocalizationService
    └── Audio/VibrationService
```

## Lộ trình ít rủi ro

1. Đóng băng snapshot, lập test cho luật hiện tại.
2. Tạo state model/resolver thuần C#; chưa đổi asset.
3. Cho Unity adapter chạy song song và so sánh kết quả với logic cũ trên vài level.
4. Chuyển lifecycle gameplay sang `LevelSessionService`.
5. Chuyển progression/economy/save; thêm migration từ JSON PlayerPrefs cũ.
6. Chuyển booster/revive và UI command.
7. Chuyển tutorial, settings, localization.
8. Đặt ads/IAP/analytics sau interface; test offline.
9. Dùng validator cho level và build smoke test Android.
10. Xóa code cũ chỉ sau khi feature parity và regression pass.

## Tiêu chí chọn công nghệ

- Giữ Unity uGUI, TextMeshPro, ScriptableObject/prefab ở bản đầu.
- Giữ Lean Touch/Dreamteck/Spine/DOTween nếu asset hiện tại cần; bọc chúng khỏi core.
- Không thêm framework DI, backend hoặc event bus mới chỉ để thay singleton; constructor/manual composition đơn giản là đủ.

## Definition of Done cho redesign

- Core resolver có unit test.
- Mỗi feature có acceptance test và fallback offline.
- Save cũ migrate được hoặc có thông báo reset rõ ràng.
- Gameplay không còn phụ thuộc trực tiếp vào SDK.
- Level validator chạy trong CI/Editor.
- Có build Android smoke test và test thiết bị thật.
