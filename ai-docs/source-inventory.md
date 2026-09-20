# Source inventory — evidence map

## Scenes/build

- `Assets/Scenes/EntryScene.unity`
- `Assets/Scenes/Home.unity`
- `Assets/Scenes/Gameplay.unity`
- `ProjectSettings/EditorBuildSettings.asset`
- `ProjectSettings/ProjectVersion.txt`

## Gameplay

- `Assets/_Game/Scripts/Gameplay/GameManager.cs`
- `Assets/_Game/Scripts/Gameplay/YarnPart.cs`
- `Assets/_Game/Scripts/Gameplay/YarnPartLayerSpawner.cs`
- `Assets/_Game/Scripts/Gameplay/LevelYarnInfoHolder.cs`
- `Assets/_Game/Scripts/Gameplay/Slot.cs`
- `Assets/_Game/Scripts/Gameplay/SlotManager.cs`
- `Assets/_Game/Scripts/Gameplay/Box.cs`
- `Assets/_Game/Scripts/Gameplay/BoxManager.cs`
- `Assets/_Game/Scripts/Gameplay/WaitingAreaManager.cs`
- `Assets/_Game/Scripts/Gameplay/BoosterManager.cs`
- `Assets/_Game/Scripts/Gameplay/YarnRopeController.cs`
- `Assets/_Game/Scripts/Gameplay/GameUIController.cs`

## Data/state

- `Assets/_Game/Scripts/GameData.cs`
- `Assets/_Game/Scripts/GameConfig.cs`
- `Assets/_Game/Scripts/GeneralConfig.cs`
- `Assets/_Game/Scripts/YarnColorDatabase.cs`
- `Assets/_Game/Scripts/Utils/Configs.cs`
- `Assets/_Game/Configs/*.asset`
- `Assets/Resources/Configs/GeneralConfig.asset`

## Product systems

- `Assets/_Game/Scripts/Daily/DailyPopup.cs`
- `Assets/_Game/Scripts/Manager/HeartSystemManager.cs`
- `Assets/_Game/Scripts/LanguageManager.cs`
- `Assets/_Game/_SettingManager/Scripts/SettingManager.cs`
- `Assets/_Game/_IAPManager/IAPManager.cs`
- `Assets/_Game/_TungNS/Databuckets.cs`
- `Assets/_Game/Scripts/Tutorial/TutorialSequenceController.cs`
- `Assets/_Game/Scripts/Tutorial/TutorialBase.cs`

## Sensitive files explicitly excluded

`Assets/google-services.json`, `Assets/liences_key.txt`, `Keystore/*` and generated purchase tangles are not copied into docs and must not be exposed in AI prompts.
