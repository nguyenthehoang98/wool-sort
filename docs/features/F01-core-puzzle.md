# F01 — Core puzzle contract

## 1. Mục tiêu
Chốt luật puzzle trước khi viết resolver: chọn piece đang lộ, ưu tiên box phù hợp, fallback waiting, xử lý layer và kết thúc session.

## 2. Luồng xử lý
```text
SelectPiece -> placement plan -> reserve piece/slot -> presentation events
FinishAnimation -> consume/reveal/close box -> CheckOutcome -> Win/Lose
```

Chi tiết state, command, invariant và test matrix: [`../week1-core-contract.md`](../week1-core-contract.md).

## 3. Thiết kế OOP
- Domain/Core: `PuzzleState`, `PieceState`, `BoxState`, `SlotState`, resolver contract; không biết Unity.
- Application: điều phối animation completion, session và ports.
- Presentation: board/box/slot/rope views; chỉ render event/command.
- Platform: chưa tham gia core loop.

## 4. File và điểm vào
- Contract: `docs/week1-core-contract.md`
- Product acceptance: `gamedesign/09-product-acceptance.md`
- AI contract: `ai-docs/01-core-gameplay.md`
- Runtime implementation: `game/Assets/_Game/Runtime/Core/PuzzleTypes.cs` và `PuzzleResolver.cs`.
- Test assembly: `game/Assets/_Game/Tests/PuzzleResolverTests.cs`.

## 5. Placeholder và reskin
- Core không chứa màu UI, sprite, font hoặc animation.
- Presentation dùng `ThemeProfile` tại `game/Assets/_Game/Runtime/Presentation/ThemeProfile.cs`.
- Prototype dùng snapshot dữ liệu reference `Level3.prefab` (order 1): 10 root pieces, Pink 6, Yellow 3 tổng cộng, Lime 3; 2 inner Yellow fixed layers.
- Visual slice import có chọn lọc tại `game/Assets/Resources/Reference/`: `Level3.fbx`, `Image3.png`, material, shader và texture dependency. Không copy C# reference.

## 6. Verification
- Contract review: W1-01..W1-04 hoàn tất trong `docs/week1-core-contract.md`.
- Unit/contract test: **16/16 EditMode tests pass**; evidence: `docs/evidence/w1-editmode-tests.md`.
- Unity compile/test: pass với Unity `6000.3.21f1`.
- Có prototype scene `game/Assets/Scenes/SampleScene.unity`; mở scene này và nhấn Play.
- Runtime instantiate model Level3 thật, gán `ThreadMat_*`, tạo box/slot 3D, click yarn bay tới slot.
- `Image3.png` được hiển thị dạng texture runtime và reveal theo từng `BoxClosed` event.
- Nhấn `R` để restart.
- Prototype chưa có Application adapter tách riêng; `CoreGameplayBootstrap` là composition root tạm thời cho vertical slice.

## 7. Thay đổi so với reference
Contract target tách pure resolver khỏi singleton/UI/animation/SDK coupling trong reference. Reference chỉ dùng để xác minh hành vi; visual asset được import có chọn lọc theo yêu cầu, C# vẫn re-implement ở target.
