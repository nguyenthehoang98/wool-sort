---
feature: ARCH
name: migration
---

# ARCH — Technical migration plan

## Target modules

```text
Game.Core
  PuzzleState, LevelDefinition, PuzzleResolver, OutcomeRules, domain events
Game.Application
  LevelSessionService, ProgressionService, EconomyService, BoosterService, TutorialService
Game.Unity
  BoardView, BoxView, SlotView, YarnView, RopeView, HUD, scene composition
Game.Platform
  PlayerPrefsSaveStore, AdsAdapter, IapAdapter, AnalyticsAdapter,
  LocalizationAdapter, AudioAdapter
Game.Editor
  LevelValidator, preview/snapshot tools
```

Use asmdef boundaries when they reduce dependency direction. Do not create an asmdef for every folder.

## Dependency rule

```text
Core <- Application <- Unity/Platform
Core must not reference UnityEngine, SDK, PlayerPrefs, DOTween, UI or scene objects.
```

## Increment order

1. Add core models and resolver tests for F01.
2. Add level mapper/validator for F02.
3. Add session controller that translates resolver events to existing views.
4. Add save/economy/progression facade and migration tests for F03.
5. Port booster/revive with operation lock for F04.
6. Port navigation/HUD/tutorial intents for F05/F06.
7. Add platform adapters and failure tests for F07/F08/F09.
8. Remove old direct calls only after feature parity.

## Do not do

- Do not rewrite all scenes/assets before proving core resolver.
- Do not replace one singleton with a new global singleton.
- Do not add abstraction for one call site unless it is a platform boundary.
- Do not delete backup levels or old save fields without an explicit migration decision.

## Verification per slice

- Compile affected assembly.
- Run pure unit tests.
- Run one scene smoke test.
- Test scene reload and app pause/resume when stateful.
- Record known unverified runtime steps in the feature doc.
