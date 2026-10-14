# Coin Resource Design Document

## Overview
The **Coin** is the primary soft‑currency in Wool‑Sort. It is earned as a reward (e.g., after completing a level) and can be spent to purchase boosters via the **MoreBoosterPopup**. A free‑coin reward is also offered via watching an ad (`Configs.FREE_COIN_ADS`).

## Where Coin Is Used
| Feature | File | How it is used |
|---------|------|----------------|
| Win reward | `Assets/_Game/Scripts/Gameplay/WinPopup.cs` | `Show(int coinReward = 60)` sets `coinText.text = "x" + coinReward;` and displays the amount to the player.
| Booster purchase | `Assets/_Game/Scripts/Gameplay/MoreBoosterPopupUI.cs` | `BuyWithCoin()` is called when the player taps the **Coin** button on a booster card. The method currently logs a stub message; in production it will call an `EconomyService` to check/consume the player's coin balance.
| Free‑coin via ad | `Assets/_Game/Scripts/Utils/Configs.cs` | Constant `FREE_COIN_ADS = 50` defines how many coins the player receives after watching a reward‑ad.

## How Coins Are Purchased / Earned
1. **Earned from level win** – `WinPopup.Show()` receives a reward amount (default 60) and displays it. The actual increment of the player’s balance should be performed by a game‑logic service (not yet implemented).
2. **Earned from ads** – The constant `FREE_COIN_ADS` is used by the ad flow (not present in the current code) to grant a fixed amount of coins.
3. **Spent in boosters** – In `MoreBoosterPopupUI` the UI shows a **Coin** button (`_coinBtnImg`). When pressed, `BuyWithCoin()` is invoked. In a full implementation it will:
   * Call `EconomyService.CheckCoin(cost)`
   * If enough, `EconomyService.ConsumeCoin(cost)`
   * Grant the selected booster.

## UI Display of Coin Balance
The project currently shows coin information in two places:
| UI Component | File | Description |
|--------------|------|-------------|
| Win popup reward text | `WinPopup.cs` (`coinText` field) | Shows the amount earned for the just‑finished level. |
| Booster purchase button icon | `MoreBoosterPopupUI.cs` (`_coinBtnImg` with `coinIcon`) | Visual cue that the purchase will use coins. |

A persistent **coin counter** (e.g., top‑right HUD) is not present yet. If added later, it should:
* Bind to a `TextMeshProUGUI` (e.g., `coinCounterText`).
* Listen for balance change events from the `EconomyService` and update its text.

## Implementation Notes & Future Work
* **EconomyService** – Introduce a service that stores the player’s coin balance (PlayerPrefs, JSON, or a server‑side value). All coin modifications (reward, ad, purchase) must go through this service to keep the balance consistent.
* **Localization** – Use a localization key for the coin text (e.g., `UI_COIN_REWARD`).
* **Visual effects** – `EffectUIManager.ShowGetCoinEffect(Vector3 pos)` is already prepared to play a particle effect when a coin is collected.
* **Testing** – Add unit tests for `EconomyService` checking `AddCoin`, `ConsumeCoin`, and edge cases (negative balance, overflow).
* **UI HUD** – Create a prefab `CoinHUD` with an icon and a `TextMeshProUGUI` that subscribes to the balance events.

---
*Document generated on $(date).*
