# W1 EditMode test evidence

- Date: 2026-09-20
- Unity: `6000.3.21f1`
- Test platform: EditMode
- Assembly: `PuzzleGame.Core.Tests`
- Result: **16 passed, 0 failed, 0 skipped**
- Raw report during verification: `/c/Users/Admin/AppData/Local/Temp/puzzle-game-w1-tests.xml`

Command:

```text
Unity.exe -batchmode -nographics -projectPath C:/Users/Admin/Documents/Projects/puzzle-game/game -runTests -testPlatform editmode -testResults C:/Users/Admin/AppData/Local/Temp/puzzle-game-w1-tests.xml
```

Coverage: placement priority, box tie-break, waiting fallback, wrong/full input, duplicate selection, input lock, pending animation, win/lose once, inner-layer reveal, Rainbow box, deterministic event types and event deduplication.

Additional verification: Windows player build succeeded. The prototype scene uses a data snapshot from reference `Assets/Resources/Levels/Level3.prefab` (order 1), plus selected visual assets in `game/Assets/Resources/Reference/`. The target does not import the incompatible reference prefab or its C# scripts. Headless player initialization reached the runtime model import path without managed exceptions; the process does not exit automatically because the prototype has no batch-quit hook.

Not covered yet: manual click-through, Application adapter, real animations, prefab/asset validation, Android device behavior.
