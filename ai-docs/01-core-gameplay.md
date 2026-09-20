---
feature: F01
name: core-puzzle
source_of_truth: current-source-plus-this-contract
---

# F01 — Core gameplay contract

## Current evidence

- `Assets/_Game/Scripts/Gameplay/GameManager.cs`
- `Gameplay/YarnPart.cs`, `YarnPartLayerSpawner.cs`
- `Gameplay/SlotManager.cs`, `Slot.cs`, `Box.cs`, `BoxManager.cs`
- `Gameplay/WaitingAreaManager.cs`, `YarnRopeController.cs`

## Domain state

```text
PuzzleState
- levelOrder: int
- levelAssetId: int
- pieces: PieceState[]
- boxes: BoxState[]
- waitingSlots: SlotState[]
- consumedPieceIds: Set<PieceId>
- outcome: Playing | Won | Lost
- inputLocked: bool
- randomSeed: uint

PieceState
- id: PieceId
- rootId: PieceId
- layerIndex: int
- color: YarnColor
- state: Hidden | OnBoard | Selected | InSlot | Dissolving | Consumed
- nextLayerId: PieceId?

SlotState
- id: SlotId
- area: Box | Waiting
- boxId: BoxId?
- acceptedColor: YarnColor | None | Rainbow
- pieceId: PieceId?
- animationState: Idle | Filling | Filled | Clearing

BoxState
- id: BoxId
- color: YarnColor | Rainbow
- slots: SlotId[3]
- state: Locked | Active | Full | Closing | Closed
```

## Commands

```text
SelectPiece(pieceId) -> Reject | PlacementPlan
UseHoldPreview(pieceId, enabled) -> Reject | PreviewChanged
FinishAnimation(animationId) -> Event[]
Tick/ResolvePending() -> Event[]
```

`SelectPiece` is the only normal path that places a piece. It must reject if outcome != Playing, input locked, piece not OnBoard, piece already dissolving, or no compatible slot.

## Slot resolution order

1. Active box slot with compatible color.
2. Among compatible boxes, box with greatest `filledSlotCount`.
3. Waiting slot compatible with color, sorted by stable visual order (left-to-right index).
4. No result => reject without mutation.

`Rainbow` box accepts any normal color. A `None` waiting slot accepts any normal color. Do not remove/re-register slots as a substitute for state; UI adapters may cache views only.

## Required events

`PiecePlaced`, `RopeRequested`, `PieceDissolveStarted`, `InnerLayerRevealed`, `SlotFilled`, `BoxBecameFull`, `BoxCloseRequested`, `BoxClosed`, `WaitingWarningChanged`, `SessionWon`, `SessionLost`, `InputLockChanged`.

Event consumers must be idempotent by `eventId`.

## Outcome rules

- Win when every playable piece is `Consumed` and all pending box animations are resolved.
- Lose when waiting slots are all filled, no compatible active box action can resolve, and session has not won.
- Emit each terminal event once.
- Animation can delay presentation but cannot invent a domain result.

## Acceptance

- Same state + seed + commands => same event/result sequence.
- Invalid tap changes no state and no economy.
- A full box cannot accept another piece.
- A piece cannot be placed twice during animation.
- Unit test resolver without `MonoBehaviour`, scene, DOTween, SDK or `PlayerPrefs`.
