# Tuần 1 — Core puzzle contract

Ngày chốt: 2026-09-20

Đây là contract trước khi viết resolver. `C:/Users/Admin/Documents/Projects/wool-sort` chỉ được dùng để kiểm tra hành vi; target re-implement trong Unity `6000.3.21f1` và không copy code.

## W1-01 — Scope

### In scope cho MVP

- Game puzzle single-player màn hình dọc.
- Người chơi chọn piece/layer đang lộ trên board.
- Piece đi vào box cùng màu; mỗi box có 3 slot.
- Nếu chưa có box phù hợp, piece đi vào waiting theo thứ tự trái → phải.
- Box đầy thì khóa slot, đóng sau khi animation pending hoàn tất.
- Inner layer chỉ lộ theo state của layer trước.
- Win/lose một lần cho mỗi session; restart bắt đầu session mới.
- Kết quả deterministic với cùng level, seed và command sequence.
- Core chạy được không cần SDK, quảng cáo, IAP, mạng hoặc asset final.
- Presentation dùng placeholder và `ThemeProfile` thay vì hard-code art trong luật.

### Out of scope bản đầu

- Multiplayer, account, backend/cloud save bắt buộc.
- Procedural level generation hoàn toàn.
- Battle pass, ranking online, marketplace.
- Ads/IAP/analytics trong resolver; các phần này chỉ là adapter ở phase sau.
- Final art, animation polish và reskin hoàn chỉnh.

### Product acceptance chính

- Tap đúng có feedback; tap sai không mutate state.
- Không double placement, không mất piece.
- Box đầy không nhận piece mới.
- Win/lose không duplicate.
- Animation chỉ trì hoãn presentation, không quyết định luật.
- Level có thể thêm bằng data/validator mà không sửa resolver.

## W1-02 — State, invariants và transitions

### Session state

```text
PuzzleState
- levelOrder: int
- levelAssetId: int
- randomSeed: uint
- pieces: PieceState[]
- boxes: BoxState[]
- waitingSlots: SlotState[]
- consumedPieceIds: Set<PieceId>
- pendingAnimationIds: Set<AnimationId>
- outcome: Playing | Won | Lost
- inputLocked: bool
```

`levelOrder` là thứ tự campaign; `levelAssetId` là khóa asset bất biến. Không dùng hai giá trị này thay thế nhau.

### Piece state

```text
Hidden -> OnBoard -> Selected -> InSlot -> Dissolving -> Consumed
```

- Root piece khởi đầu ở `OnBoard`; inner piece bắt đầu `Hidden`.
- Chỉ piece ở `OnBoard` được `SelectPiece`.
- `Selected`, `InSlot` và `Dissolving` không được chọn lần hai.
- Piece chỉ tồn tại ở một nơi: board, slot hoặc consumed.
- Khi piece trước hoàn tất dissolve, application mới reveal layer kế tiếp.

### Box và slot state

```text
Box: Locked -> Active -> Full -> Closing -> Closed
Slot: Empty -> Filling -> Filled -> Clearing -> Empty
```

- Box có đúng 3 slot.
- `Locked` không nhận input; `Active` có thể nhận màu phù hợp.
- Box `Full` không nhận thêm piece.
- `Rainbow` box nhận mọi màu thường nhưng không đổi màu piece.
- Slot waiting `None` nhận màu thường; thứ tự chọn luôn là index trái → phải.
- Animation state không được tự thay đổi domain state nếu chưa có resolver/application command xác nhận.

### Session invariants

1. ID của piece, box, slot và animation là duy nhất trong session.
2. Một piece không thể nằm đồng thời trong board, slot và `consumedPieceIds`.
3. Slot chứa piece thì piece phải ở `InSlot`, `Dissolving` hoặc `Consumed` theo transition đang chờ.
4. Box chỉ chứa tối đa 3 slot và slot chỉ thuộc một box/waiting area.
5. Command bị reject không đổi state, không phát event mutation và không đổi economy.
6. Terminal outcome chỉ được emit một lần; command sau `Won`/`Lost` đều bị reject.
7. Cùng state + seed + commands cho cùng event/result sequence.
8. `Core` không tham chiếu `UnityEngine`, scene, DOTween, SDK hoặc `PlayerPrefs`.

### State transition rules

| Transition | Điều kiện | Kết quả |
|---|---|---|
| Hidden → OnBoard | Layer trước đã `Consumed` và reveal được phép | Emit `InnerLayerRevealed` |
| OnBoard → Selected | `SelectPiece` hợp lệ | Tạo placement plan, khóa piece |
| Selected → InSlot | Slot đã được reserve | Emit `PiecePlaced`, `SlotFilled` |
| InSlot → Dissolving | Application xác nhận animation bắt đầu | Emit `PieceDissolveStarted` |
| Dissolving → Consumed | `FinishAnimation` hợp lệ | Thêm vào consumed, có thể reveal layer |
| Active → Full | Filled slot count đạt 3 | Emit `BoxBecameFull`, khóa box |
| Full → Closing | Ba slot và pending animation đã sẵn sàng | Emit `BoxCloseRequested` |
| Closing → Closed | `FinishAnimation` của box hợp lệ | Emit `BoxClosed` |
| Playing → Won | Tất cả playable piece consumed và không còn pending animation | Emit `SessionWon` một lần |
| Playing → Lost | Waiting đầy, chưa win và không còn action hợp lệ | Emit `SessionLost` một lần |

## W1-03 — Commands và outcome rules

### Public commands

```text
SelectPiece(pieceId) -> Reject | PlacementPlan
UseHoldPreview(pieceId, enabled) -> Reject | PreviewChanged
FinishAnimation(animationId) -> Event[]
Tick/ResolvePending() -> Event[]
```

`FillSlot`, `ResolveBox` và `CheckOutcome` là transition nội bộ của resolver, không phải API để view tự gọi.

### `SelectPiece` validation order

1. Reject nếu `outcome != Playing`.
2. Reject nếu `inputLocked == true`.
3. Reject nếu piece không tồn tại, không ở `OnBoard`, hoặc đang bị xử lý.
4. Tìm active box tương thích màu.
5. Nếu có nhiều box, chọn box có `filledSlotCount` lớn nhất; hòa thì chọn `boxOrder` nhỏ nhất.
6. Nếu không có box, chọn waiting slot trống đầu tiên theo index.
7. Nếu không có slot, reject không mutation; sau đó `CheckOutcome` có thể phát `SessionLost` nếu điều kiện lose đã đủ.
8. Khi có plan, reserve piece và slot trong cùng một mutation; không cho command thứ hai chen vào.

### Màu và placement

- Box thường chỉ nhận đúng `YarnColor`.
- Rainbow box nhận mọi màu thường.
- Waiting `None` nhận mọi màu thường.
- Không có implicit đổi màu, tự xóa piece hoặc trừ resource khi placement thất bại.
- `RopeRequested` là event cho presentation, không phải lý do để commit state lần hai.

### Input gates

| Gate | SelectPiece | Hold preview | Nguyên nhân mở lại |
|---|---:|---:|---|
| Session loading | Reject | Reject | Session ready |
| Modal/ad/IAP/booster | Reject | Reject | Modal/service operation kết thúc |
| Piece dissolve | Piece đó reject; piece khác theo policy session | Reject piece đó | FinishAnimation |
| Box full/closing | Box đó reject; box khác còn active | Theo piece | BoxClosed hoặc session rule |
| Won/Lost | Reject | Reject | Tạo session mới |

`inputLocked` chỉ là gate phiên; mỗi box vẫn phải tự kiểm tra `BoxState` để tránh nhận piece khi full.

### Outcome rules

- **Win:** tất cả playable piece ở `Consumed` và `pendingAnimationIds` rỗng. Emit `SessionWon` một lần.
- **Lose:** waiting slots đều filled, session chưa win, và không còn active box action tương thích. Emit `SessionLost` một lần.
- Pending animation không được tạo ra hoặc hủy outcome tùy ý từ view; resolver/application quyết định qua `FinishAnimation`.
- `eventId` là idempotency key; consumer bỏ qua event đã xử lý.
- Retry/restart tạo session state mới; không replay mutation của session cũ.

## W1-04 — Test matrix

| ID | Given | When | Then |
|---|---|---|---|
| CORE-01 | Piece màu xanh, box xanh còn slot | Select piece | Box xanh nhận trước waiting |
| CORE-02 | Hai box cùng màu, fill lần lượt 1 và 2 | Select piece | Box fill 2 được chọn |
| CORE-03 | Box hòa fill count | Select piece | `boxOrder` nhỏ hơn được chọn |
| CORE-04 | Không có box phù hợp, waiting còn trống | Select piece | Slot waiting đầu tiên nhận piece |
| CORE-05 | Nhiều waiting slot trống | Select piece | Chọn trái → phải ổn định |
| CORE-06 | Sai màu và không có Rainbow box | Select piece | Reject, state/economy/event mutation không đổi |
| CORE-07 | Box đã đủ 3 slot | Select piece | Reject vào box đó |
| CORE-08 | Piece đang `Dissolving` | Select piece lần hai | Reject, không duplicate placement |
| CORE-09 | Input đang locked | Select piece | Reject, state không đổi |
| CORE-10 | Tất cả piece consumed nhưng còn animation | Resolve pending | Chưa win |
| CORE-11 | Tất cả piece consumed và không pending | Resolve pending | Emit `SessionWon` đúng một lần |
| CORE-12 | Waiting đầy, chưa win, không còn action | Resolve pending | Emit `SessionLost` đúng một lần |
| CORE-13 | Session đã Won/Lost | Select/Finish/Tick | Reject/no-op, không emit terminal lần hai |
| CORE-14 | Cùng state, seed và commands | Replay commands | Event/result sequence giống hệt |
| CORE-15 | Reveal layer trước chưa consumed | Resolve pending | Inner layer vẫn Hidden |
| CORE-16 | Animation event gửi lại cùng eventId | Consume event | Consumer chỉ xử lý một lần |

### Verification boundary

Tuần 1 đã có một pure resolver tối thiểu để kiểm chứng contract và chạy CORE-01..16 không cần `MonoBehaviour`/scene/SDK. W4-01 sẽ tiếp tục mở rộng resolver cho layer/box lifecycle đầy đủ và nối vào Application/Unity adapter.

## Open decisions đã chốt

- Box tie-break dùng `boxOrder` ổn định.
- Waiting fallback luôn left-to-right.
- `Rainbow` chỉ là khả năng nhận màu, không phải màu mới của piece.
- Animation là pending event; không tự quyết định domain outcome.
- Reference behavior có thể khác contract; contract của target được ưu tiên.
