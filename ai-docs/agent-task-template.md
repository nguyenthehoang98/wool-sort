# AI task template — one feature slice

Copy this file when asking an agent to implement a slice.

```text
Feature: Fxx / short name
Scope: [exact files/modules allowed]
Not in scope: [explicit exclusions]

Read first:
- ai-docs/<feature-contract>.md
- docs/00-current-state.md
- source files: <paths>

Goal:
- <one observable behavior>

Acceptance:
- [ ] <specific testable criterion>
- [ ] <specific testable criterion>

Implementation rules:
- Keep core free of Unity/SDK where contract says so.
- Reuse existing asset/dependency before adding a package.
- Do not touch unrelated modified files.

Verification:
- Run <test/build command>.
- Run one manual/runtime scenario.
- Report files changed, files intentionally untouched, and unverified risks.
```
