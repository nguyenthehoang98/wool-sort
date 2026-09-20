---
feature: F06
name: tutorial
---

# F06 — Tutorial contract

## Data

```text
TutorialDefinition
- id
- version
- trigger: level_start | domain_event
- levelOrder?
- steps[]

TutorialStep
- id
- targetQuery
- instructionKey
- completion: tap_target | hold_target | domain_event | dismiss
- required: bool
- canSkip: bool
- timeoutSeconds
```

## Runtime

`TutorialService` owns progress and flags. It subscribes to domain events and exposes a view model. `TutorialView` renders hand/arrow/text and sends dismiss/confirm intents. `TutorialState` must not intercept commands by itself; the application command gate validates whether the tutorial permits the intent.

## Rules

- One active tutorial per session.
- Starting a new level unregisters old event listeners.
- A target that is consumed/closed completes or fails explicitly; never wait forever.
- Tutorial versions are persisted; changing version may reset only that tutorial.
- Mandatory first-session steps can block input; optional later steps cannot.

## Acceptance

- Level-start steps run in stable order.
- Custom event listener is one-shot and removed on completion.
- Reopening a level does not duplicate listeners.
- Skip works when `canSkip = true`.
- Tutorial completes through fake domain events in test.
