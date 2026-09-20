---
feature: F03
name: progression-and-economy
---

# F03 — Progression, save and economy contract

## Current evidence

- `Assets/_Game/Scripts/GameData.cs`
- `Scripts/Manager/HeartSystemManager.cs`
- `Scripts/Utils/Configs.cs`
- `Scripts/Daily/DailyPopup.cs`
- `Scripts/_TungNS/Databuckets.cs`

## Save schema

```text
PlayerProfile
- schemaVersion: int
- currentLevelOrder: int
- coin: non-negative int
- heart: 0..maxHeart
- infiniteHeart: { active, startedAtUtc, durationMinutes }
- boosters: map<BoosterId, non-negative int>
- removeAds: bool
- settings: SettingsState
- tutorialFlags: map<TutorialId, version>
- daily: DailyState
- updatedAtUtc
```

## Commands/services

```text
ProgressionService.StartLevel(order)
ProgressionService.CompleteLevel(order, result)
ProgressionService.RestartLevel(order)
EconomyService.TrySpend(resourceId, amount, reason, placement)
EconomyService.Grant(transactionId, resourceId, amount, reason, placement)
SaveStore.Load() / SaveStore.Save(profile)
```

Only services may mutate profile. UI, ads and gameplay views must not write `PlayerPrefs` directly.

## Economy invariants

- All balances remain non-negative.
- Spend fails atomically when balance is insufficient.
- Grant with an existing transaction id is a no-op returning the previous result.
- A failed ad/purchase never grants reward.
- Completion/reward transaction is idempotent after app restart.
- Level progression cannot skip order unless an explicit debug/admin path is enabled.

## Time rules

Current values from source: max heart 5, heart refill 1800 seconds, daily reward 7 days. Make these config values, not constants in UI. Use UTC timestamps plus a documented anti-clock-tamper policy; if offline support is required, mark the result as untrusted rather than silently granting repeated rewards.

## Migration

1. Read old `new_game_data` JSON once.
2. Map known fields to version 1.
3. Clamp invalid balances and write a migration result.
4. Keep backup/rollback during beta.
5. Never migrate secret SDK data into the new profile.

## Acceptance

- Kill/relaunch during grant does not duplicate reward.
- Old profile with missing optional fields loads defaults.
- Offline gameplay works with a loaded profile.
- Unit tests cover heart max, zero balance, duplicate transaction and level completion retry.
