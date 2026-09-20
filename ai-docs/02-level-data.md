---
feature: F02
name: level-content
---

# F02 — Level data contract

## Current evidence

- `Assets/_Game/Scripts/Gameplay/LevelYarnInfoHolder.cs`
- `Gameplay/YarnPart.cs`, `YarnPartInfo.cs`
- `Assets/_Game/Scripts/GeneralConfig.cs`
- `Assets/Resources/Configs/GeneralConfig.asset`
- `Assets/Resources/Levels/Level{id}.prefab`
- `Assets/Resources/Configs/Image/Image{id}.asset`

## Canonical model

```json
{
  "levelAssetId": 3,
  "orderIndex": 1,
  "mode": "Normal",
  "seed": 1003,
  "initialUnlockedBoxes": 2,
  "waitingSlotCount": 5,
  "pieces": [
    {
      "id": "p-001",
      "color": "Pink",
      "layers": ["Pink", "Blue", "Random"]
    }
  ],
  "thumbnailId": "image-3"
}
```

The actual serialized format may remain ScriptableObject/prefab for the first slice, but the runtime must consume an immutable `LevelDefinition` equivalent to this model.

## Invariants

- `orderIndex` is unique and maps to exactly one existing `levelAssetId`.
- `levelAssetId` is not used as progression order.
- Every piece has one root layer; layer indexes are contiguous from 1.
- Random layer consumption equals the declared color pool.
- `YarnColor.None` and `Rainbow` are control values, not normal puzzle colors.
- Box capacity is 3 unless a future definition explicitly changes it.
- A level has a thumbnail or an explicit no-thumbnail flag.

## Validator

Create an Editor/build validator that reports, not silently repairs:

- missing prefab/config/thumbnail;
- duplicate order or ID;
- missing material/color mapping;
- inner list length != `totalInnerCount`;
- invalid random pool count;
- level with no possible terminal state under the selected rules.

## Runtime loading

`LevelRepository.Load(orderIndex)` resolves order → asset ID → definition. It must return a typed error for missing data. Do not call `Resources.Load` from the resolver.

## Current snapshot warnings

- Project version is Unity `6000.3.21f1`; old Word brief says `2022.3.50f1`.
- GeneralConfig has 300 definitions and 88 order entries.
- Asset folders contain more level artifacts than the current order; unreferenced assets are not automatically deleted.
