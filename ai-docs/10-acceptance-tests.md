---
feature: QA
name: acceptance-suite
---

# QA — Acceptance tests for AI implementation

## Core resolver

- [ ] Tap compatible piece fills matching box slot before waiting slot.
- [ ] Box selection prefers the box with the greatest current fill count.
- [ ] Waiting fallback is stable left-to-right.
- [ ] Wrong color/no slot is a no-op.
- [ ] Full box rejects input.
- [ ] Piece can be selected once only.
- [ ] Inner layer appears only according to layer state.
- [ ] Win emits once after pieces and pending animations resolve.
- [ ] Lose emits once when waiting is full and no legal resolution remains.

## Level content

- [ ] Order 1 resolves to asset 3 in current snapshot.
- [ ] Missing level asset returns typed error.
- [ ] Duplicate order/asset/layer/random pool is reported by validator.
- [ ] Same seed + commands reproduces same event log.

## Economy/progression

- [ ] Coin/heart/booster balances never become negative.
- [ ] Duplicate transaction ID grants once.
- [ ] Save/reload keeps completed level and balances.
- [ ] Heart at max does not overflow.
- [ ] Heart timer and daily reward cannot repeatedly grant after relaunch.
- [ ] Old `new_game_data` migrates or fails with a visible safe result.

## Booster/revive

- [ ] Add Slot respects cap and updates layout.
- [ ] Clean moves all waiting pieces without loss.
- [ ] Magic Box touches only selected active box.
- [ ] Booster double tap is ignored.
- [ ] Failed ad/coin authorization changes nothing.
- [ ] Revive clears loss gate without re-granting inventory.

## UI/platform

- [ ] Entry routes level 1 to Gameplay and later levels to Home.
- [ ] Modal blocks gameplay input.
- [ ] Safe area works on target aspect ratios.
- [ ] Language/settings survive scene reload.
- [ ] SDK failures do not block core play.
- [ ] Purchase restore/no-ads is idempotent.
- [ ] Analytics failure is non-fatal.

## Runtime smoke checklist

- [ ] Build in current Unity version from `ProjectVersion.txt`.
- [ ] Test first level on Android device.
- [ ] Test app pause/resume during dissolve, booster, ad and purchase.
- [ ] Test kill/relaunch during reward grant.
- [ ] Test 10 consecutive level loads without stale singleton/listener state.
- [ ] Capture failures with source path, reproduction input sequence, expected/actual result.
