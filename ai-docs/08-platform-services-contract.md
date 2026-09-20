---
features: [F08, F09]
name: platform-services
---

# F08/F09 — Localization, settings, audio, analytics, remote config

## Service boundaries

```text
ILocalizationService.Get(key, args)
ISettingsStore.Load/Save(SettingsState)
IAudioService.PlaySfx(id), PlayMusic(id), SetEnabled(kind, value)
IVibrationService.Play(hapticId)
IAnalyticsService.Track(Event)
IRemoteConfigService.Get(key, localDefault)
```

## Settings

Persist language, sound, music and vibration in one versioned state. Setting UI changes state through `SettingsService`; audio/vibration adapters observe it. Default language is English unless product config says otherwise.

## Localization

Use stable keys, not source strings. Validate missing keys and text overflow in the build/content check. Plurals and number formatting must be locale-aware. Keep I2 integration behind the adapter.

## Analytics

Minimum events: level start/complete/fail/restart/exit, piece select, booster use, revive, resource earn/spend, ad request/complete/fail, purchase start/verified. Every event includes schemaVersion, sessionId, levelOrder and placement where relevant. Analytics failure is non-fatal and may queue offline with bounded retry.

## Remote config

Every key has local default, type/range validation, config version and fetch-failure fallback. Remote config is snapshotted when a level session starts; do not alter an active puzzle mid-session.

## Acceptance

- Core tests run with null/no-op services.
- Toggle settings survives scene reload.
- Missing localization key is observable in validation/log, not a crash.
- Analytics SDK exception cannot block tap/win/lose.
- Invalid remote value falls back to local safe default.
