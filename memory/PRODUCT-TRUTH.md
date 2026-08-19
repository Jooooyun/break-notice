# PRODUCT TRUTH — BREAK NOTICE

Rule: every entry carries evidence and date. External claims may use only verified entries.

## Implemented

- CHR00 updated 37-FBX delivery imports as independent Humanoid/Create From This Model assets with no copied source Avatar. Evidence: focused importer contract 1/1 PASS on 2026-08-17.
- Pickup Low Small and Pickup Low Medium have dedicated controller states and natural AnimationEvent-owned contact/completion paths. Evidence: focused Pickup→Carry→Throw PlayMode 2/2 PASS on 2026-08-17.
- Reachable CHR00 pickup/catch/throw physical commits and melee/reaction completion boundaries use measured source-frame timing rather than inherited generic 0.52/0.88 values. Evidence: event contracts 2/2 and focused natural PlayMode paths 4/4 PASS on 2026-08-17.
- One-hit Jab reverses 14→1 before returning to locomotion; Jab uses the animated left hand and Cross uses the right hand. Evidence: focused Jab return and natural combo PlayMode 2/2 PASS on 2026-08-17.
- `PickupLowSmallAction` uses the user-supplied stationary, right-hand `Picking Up.fbx` as an independent Humanoid source; its contact/stabilization/completion boundaries are source frames 37/106/132. Evidence: importer 1/1 and actual v007 Small Pickup→Carry→Charged Throw 1/1 PASS on 2026-08-17.

## Not implemented

- CHR00 Large/Heavy catch and throw runtime playback is not active. Evidence: `DesignerACharacterIntegrationAuthoring.CreatePlayerController` has no Large states (observed 2026-08-17).
- CHR00 Turn, Short ALT strafe, strap candidate, preview, and two-hand reference clips are not runtime states (observed 2026-08-17).
- Jump anticipation/physics alignment is not resolved, and the delivered Knockdown→Getup pose seams are not visually approved (observed 2026-08-17).

## Permanently excluded

<!-- None recorded. -->
