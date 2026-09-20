---
feature: F07
name: monetization
---

# F07 — Ads/IAP/shop contract

## Interfaces

```csharp
interface IAdsService {
    Task<AdResult> ShowRewarded(string placement);
    Task<AdResult> ShowInterstitial(string placement);
}
interface IPurchaseService {
    Task<PurchaseResult> Purchase(string productId);
    Task Restore();
}
interface IEntitlementStore {
    bool Has(string entitlementId);
    void Grant(string transactionId, string entitlementId);
}
```

Actual implementation may use existing Bravestars/mediation/Unity IAP packages. Core must depend on these contracts only.

## Product categories

Current product families: no ads, no ads bundle, beginner bundle, coin packs, booster/heart bundles. Keep product IDs in one catalog adapter; do not scatter strings through popup code.

## Transaction flow

```text
Requested -> SDKPending -> Verified -> Granted -> Presented
                    \-> Failed/Cancelled
```

`Verified` is required for real-money purchase. `Granted` uses a transaction id and is idempotent. `Presented` is cosmetic and may retry without granting again.

## Ad flow

Rewarded ad success authorizes one reward transaction. Failure, close-before-complete, no-fill, timeout and SDK exception authorize none. Interstitial never changes domain state.

## Acceptance

- Offline core starts and plays.
- Repeated purchase callback grants once.
- Restore no-ads is idempotent.
- Ad reward is not granted on failure.
- Test products and real products are separated by build configuration.
- No secret/receipt is logged or committed.
