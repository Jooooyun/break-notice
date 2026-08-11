# CONTEST-0002 — Operations Floor 5–7 Minute Showcase

- Status: `OWNER-APPROVED DESIGN AND WORK-PACKAGE AUTHORITY / S0 AUTHORITY REGISTRATION COMPLETE / S1 NOT AUTHORIZED / IMPLEMENTATION RELAY ABSENT`
- Change Control: `CR-0004`
- Decision: `ADR-0024`
- Branch: `contest/game-builders-2026-showcase`
- Immutable Baseline: `15c915c4c7552113941149e84ba5cc1fea9c63f1`
- Stable Regression Dependency: `CONTEST-0001 / C0-C4 accepted foundation`
- Proposed S1 Scene: `Game/Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v001.unity`
- S0 Relay: `EAR-CONTEST-0002-S0-001 / CONSUMED / EXPIRED`
- Active Implementation Relay: `ABSENT`

## Authority Sources and Precedence

1. Latest direct Human Owner instruction.
2. `BREAK_NOTICE_GAMEPLAY_ANIMATION_COVERAGE_v003_FINAL.txt`, 54,065 bytes,
   SHA-256 `6c5b6aca6e83686331a1d64d967d0489327aac35d285e0f2aa7da9fb1972bb69`.
3. `BREAK_NOTICE_CONTEST_SHOWCASE_MAP_DESIGN_v001.txt`, 23,050 bytes,
   SHA-256 `dfe3d2f312bb84dae7df7d596f4a7153f7ec69cfaac3590b93cbc6ab78833834`.
4. `CR-0004`, `ADR-0024`, this ticket, Showcase Lock and Asset Matrix.
5. Designer A/B v002 handoffs.
6. Existing contest authority and historical handoffs.

`BREAK_NOTICE_CODEX_AUTHORITY_RECONCILIATION_HANDOFF_v001.txt`, 21,218 bytes,
SHA-256 `a3e3fb0b1690dabd1d61480b5155f22d9720efaa9db0ad11aa52e3a2e57e6d0d`,
is the procedural bridge and audit guide, not independent gameplay Canon.

## Preserved Foundation

- `CONTEST-0001` remains the C0–C4 accepted regression harness.
- `Contest_SingleRoomGraybox.unity` remains byte-identical at 89,175 bytes,
  SHA-256 `0ed96ae20a74a8d91888dd108915eb9eeaa56da7d3ba6581b660a1e966e8de3a`.
- Existing movement, camera, interaction, carry, throw, Reset, C4 relay and
  post-acceptance tuning evidence is not rewritten.
- No Showcase implementation completion is inferred from that dependency.

## PRT-RM-002 Design Lock

`PRT-RM-002 — Operations Floor Handover Lockdown` is a separate 48 m × 34 m
flat open-office Showcase with a central Cubicle Island and figure-eight
topology:

- Z0 Entry Staging
- Z1 Direct Equipment Bay
- Z2 Central Clash Bowl
- Z3 West Logistics Loop
- Z4 Central Cubicle Ring
- Z5 East Physics Lane
- Z6 North Tag Arena / Drawer Gate
- Z7 Exit Run / Next-Area Tease

The target session is 5–7 minutes with an immediate replay impulse. The map
has three approach routes, four Drawer solution families, three Team Readiness
badges and one optional Team Finisher.

## Actor and Object Inventory

- CHR_00 Player ×1
- CHR_01 mobile Ally ×1
- identical ENY01 Compliance Runner ×2; no new enemy archetype
- File Binder ×5
- Wheeled Office Chair ×3
- Rolling Drawer Cabinet ×1
- Copier-Class recognition-only object ×1

Objects remain physical in the room until Reset. Hidden inventory,
auto-replenishment and teleport return are prohibited.

## Mobile Ally Responsibilities

The one approved active ally may perform Follow, Fetch, Supply, Brace, Rescue,
Combat Tag and Finisher Partner roles. Those roles are design authority only.
S0 implements none of them.

## Q Context Priority

Exactly one Q prompt may be active. Highest valid context wins:

1. Player Grabbed — `BREAK GRAB`
2. Finisher Window — `TEAM FINISH`
3. Chair Strap Assist — `ASSIST ATTACH`
4. Staggered Enemy — `TAG SHOVE / SET HIM UP`
5. Supply Candidate — `SEND BINDER / SEND CHAIR`
6. Otherwise — `REGROUP`

## Team Readiness and Objective

- PASS: catch an Ally-supplied Binder.
- BRACE: complete Ally-assisted carabiner attach.
- BREAK: complete Grab Rescue or Combat Tag.
- Two distinct badges produce TEAM READY; no decay, spend or persistence.

Drawer solutions:

1. Direct Chair Throw
2. Cargo Strap Redirect
3. Runner Causal Collision
4. Team Finisher

`FIN-P-001 — HANDOVER CRASH` is an optional approximately 2.5-second bounded
team sequence whose physical result moves the Drawer and returns control. The
Drawer must also be clearable without the Finisher.

## Work Packages

| Package | Scope | Status |
|---|---|---|
| S0 | Authority registration, exact directives/hashes, records and handoffs | `COMPLETE / DOCUMENTATION ONLY` |
| S1 | Separate 48 m × 34 m figure-eight graybox; no Production Art | `NOT AUTHORIZED` |
| S2 | Mobile Ally Follow anchors, side switching, catch-up, blocked/regroup | `NOT AUTHORIZED` |
| S3 | Mobile Fetch, Binder Toss, Chair Feed and Reset adaptation | `NOT AUTHORIZED` |
| S4 | Q arbitration, Rescue, Combat Tag, PASS/BRACE/BREAK, TEAM READY | `NOT AUTHORIZED` |
| S5 | Bounded Cargo Strap, stabilize, attach assist, tension handoff, redirect/release | `NOT AUTHORIZED` |
| S6 | Two Runner pressure: body check, shove, short grab, stagger, one attack token | `NOT AUTHORIZED` |
| S7 | Drawer four-solution hooks, Parking Bay, EXIT and result card | `NOT AUTHORIZED` |
| S8 | HANDOVER CRASH paired sequence, bounded camera and physical result | `NOT AUTHORIZED` |
| S9 | Technically accepted Art/VFX/Audio/UI and Web qualification | `NOT AUTHORIZED` |

Each package requires a separate immutable baseline, clean-state check, exact
allowlist, validation contract, commit/push authority and one-shot Human Owner
relay. Completion or acceptance of one package does not authorize the next.

## S0 Validation Boundary

S0 creates documentation only. No Unity test, manual gameplay smoke, browser
smoke or Web build is required or claimed. No production asset is authorized
for Runtime integration by S0. Graybox fallback remains active.

## Forbidden Inference

This ticket does not authorize S1, historical C5 continuation, Runtime code,
Scene/Prefab/Input/Package modification, Runtime Art, PR, merge, `main`, stable
contest changes, new packages, external services, a second ally, a new enemy
archetype, breakage, runtime fracture or unrestricted destruction.
