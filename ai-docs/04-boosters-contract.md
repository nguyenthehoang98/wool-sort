---
feature: F04
name: boosters-and-revive
---

# F04 — Booster/revive contract

## IDs

```text
booster.add_slot
booster.clean
booster.magic_box
revive.add_box
revive.clean
```

## State machine

```text
Idle
 -> ValidatingCost
 -> InputLocked
 -> ApplyingState
 -> PlayingAnimation
 -> Reconcile
 -> Idle

Any transient state -> Failed -> Reconcile -> Idle
```

Never subtract inventory before the service has accepted the command. For ad/coin offers, authorization must complete before `ApplyingState`.

## Add Slot

Preconditions: current waiting count < configured max (current source max = 7). Effect: add one waiting slot, register it in domain state, update layout. Rejected when max reached. The source currently hard-codes `canAdd = false` in `RefreshLayout`; this must be resolved explicitly, not ported blindly.

## Clean

Preconditions: at least one filled waiting slot. Effect: move all filled waiting pieces to a temporary holding state; clear waiting slots; emit `TempStorageFilled`. If animation fails, state still has the pieces and can retry/rollback. Clean must not delete a piece.

## Magic Box

Preconditions: an active first box and missing slots. Effect: mark first box rainbow for the operation, select pieces according to deterministic magic rule, fill only that box, then resolve it. Repeated use while operation is active is rejected.

## Revive

- `revive.add_box` if an unopened box remains.
- otherwise `revive.clean`.
- Revive clears the loss gate once its state effect completes.
- Do not reload the level unless restart is explicitly selected.

## Required adapter boundary

`IBoosterService.Use(BoosterCommand)` returns domain result/events. `BoosterView` handles broom, ghost, rainbow, VFX. `EconomyService` handles inventory cost. `AdsService` only authorizes reward.

## Acceptance

- Double tap starts one booster operation.
- Failed authorization leaves inventory and puzzle state unchanged.
- Clean preserves every moved piece.
- Add Slot rejects at cap.
- Magic Box cannot mutate another box.
- Revive makes the same loss state playable and does not duplicate reward.
