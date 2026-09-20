---
feature: F05
name: ui-and-navigation
---

# F05 — UI/navigation contract

## States

```text
Booting -> Loading -> Home | Gameplay
Gameplay: Preparing -> Playing -> Modal -> Playing
Playing -> WinResolving -> WinModal
Playing -> LoseResolving -> ReviveModal | Restart
Modal -> Home | Gameplay | WinModal | LoseModal
```

Only `NavigationService` changes scene or modal stack. Only `GameplaySessionController` owns play/pause/input gate for the current session.

## Intent boundary

UI sends intents:

```text
PlayLevel(orderIndex)
TapPiece(pieceId)
HoldPiece(pieceId, active)
UseBooster(boosterId)
ClaimReward(rewardId, method)
OpenSettings()
RestartLevel()
ExitToHome()
```

UI does not call `GameData.AddCoin`, `PlayerPrefs`, `KitSDK` or `GameManager` domain methods directly.

## View model requirements

HUD observes: level order/mode, remaining pieces, coin, heart, booster counts, waiting occupancy, input gate, ad/purchase busy state. Views are disposable on scene reload and cannot retain old session references.

## Mobile UX acceptance

- Safe area and anchor-based placement at 16:9, 19.5:9 and small-width device.
- Modal blocks gameplay raycasts.
- Buttons disable during pending action and show progress.
- Minimum touch target 44 logical points.
- No color-only critical state.
- Back button behavior is explicit for each state.
