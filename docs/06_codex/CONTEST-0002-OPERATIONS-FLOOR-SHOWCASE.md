# CONTEST-0002 — Operations Floor 5–7 Minute Showcase

- Status: `OWNER-APPROVED DESIGN AND WORK-PACKAGE AUTHORITY / S1-S3 IMPLEMENTATION COMPLETE / S3 HUMAN OWNER APPROVED / DESIGNER A SOURCE AND IMPORT VERIFIED / RUNTIME ANIMATION PRESENTATION MANUAL REFINEMENT DEFERRED / S4 NOT AUTHORIZED / IMPLEMENTATION RELAY ABSENT`
- Change Control: `CR-0004`
- Decision: `ADR-0024`
- Branch: `contest/game-builders-2026-showcase`
- Immutable Baseline: `15c915c4c7552113941149e84ba5cc1fea9c63f1`
- Stable Regression Dependency: `CONTEST-0001 / C0-C4 accepted foundation`
- Active Showcase Scene: `Game/Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v003.unity`
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
Combat Tag and Finisher Partner roles. S2-S3 implement bounded Follow, Fetch
and Supply with explicit commands and authored Reset restoration. Brace,
Rescue, Combat Tag and Finisher Partner remain S4-S8 work and are not inferred.

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
| S1 | Separate 48 m × 34 m figure-eight graybox; no Production Art | `COMPLETE / VERIFIED` |
| S2 | Mobile Ally Follow anchors, side switching, catch-up, blocked/regroup | `COMPLETE / VERIFIED` |
| S3 | Mobile Fetch, Binder Toss, Chair Feed and Reset adaptation | `COMPLETE / HUMAN OWNER APPROVED 2026-08-13` |
| S4 | Q arbitration, Rescue, Combat Tag, PASS/BRACE/BREAK, TEAM READY | `NOT AUTHORIZED` |
| S5 | Bounded Cargo Strap, stabilize, attach assist, tension handoff, redirect/release | `NOT AUTHORIZED` |
| S6 | Two Runner pressure: body check, shove, short grab, stagger, one attack token | `NOT AUTHORIZED` |
| S7 | Drawer four-solution hooks, Parking Bay, EXIT and result card | `NOT AUTHORIZED` |
| S8 | HANDOVER CRASH paired sequence, bounded camera and physical result | `NOT AUTHORIZED` |
| S9 | Technically accepted Art/VFX/Audio/UI and Web qualification | `NOT AUTHORIZED` |

Each package requires a separate immutable baseline, clean-state check, exact
allowlist, validation contract, commit/push authority and one-shot Human Owner
relay. Completion or acceptance of one package does not authorize the next.

## S1-S3 Acceptance and Designer A Integration Evidence

- S1 foundation: `9d974d6`, with bounded playability and map revisions through
  `d263e3e`.
- S2 mobile Ally: `2f14a7f`.
- S3 initial implementation: `0e842d2`.
- S3 explicit-command and carry-continuity repair:
  `fdc49025eefab364d8e3d92d89856fb89b1813e4`.
- Human Owner S3 disposition: `PASS / APPROVED / 2026-08-13`.
- Designer A corrected source: `origin/art/characters-animation` at
  `978c66a80792632d5c4ba9ed95c477f1a6e7cc7a`.
- Conflict-free source merge:
  `d6c86e21b06334898a4e1a901aeff16eabd2528b`.
- Runtime integration:
  `527603c9a4eb182f18b8ee1cc2a746df0823357c`.
- Runtime coverage: four textured Humanoid models, four Animator Controllers
  and 172 Humanoid animation FBX (`CHR00 61 / CHR01 51 / ENY01 60`).
- ENY01 Normal exposes the 54 shared, hit and melee catalog clips but none of
  the six Elite-only clips. ENY01 Elite exposes the same 54 plus exactly six
  Elite-only clips.
- Unity `6000.3.20f1` compile PASS, EditMode `100/100`, PlayMode `72/72`,
  Web Development build GUID `d07ffe2b71964dd994bba2f1cc4a8e10`, warning
  `0`, error `0`, HTTP `19/19`, 178,528,439 verified bytes and aggregate
  SHA-256 `513e87797121a56261af1e12f2fb911e396a37f90dc5585ea6d6b8cda7dd848c`.
- In-app browser cold load, textured actor rendering and product console
  warning-error `0` passed.
- Enemy attack clips are connected as an eligible animation catalog; dynamic
  enemy attack selection and hit resolution remain later combat work and are
  not inferred by this integration.

## Deferred Runtime Animation Presentation Issues

The Human Owner preserves approval of the delivered character models and
animation source files but defers the current Unity runtime motion application
for manual setup. Checkpoint
`94932aabac091d08ef2ae733d68e115b9e66ff6b` is evidence only and is not a
runtime animation acceptance claim.

Open issues recorded on 2026-08-13:

1. Character locomotion remains visually unnatural.
2. The Ally sinks into the floor while seeking and acquiring an object.
3. Physical throw release occurs before the authored throw motion, so the
   object leaves before the animation reads.

The checkpoint passed EditMode `102/102`, PlayMode `73/73`, Web Development
build GUID `4413a8ac1dc148998c31ee466955414d`, Unity warning-error `0/0` and HTTP
`19/19`. These automated results do not close the three Human Owner-observed
presentation defects. Manual Unity animation configuration is deferred and the
defects do not revoke the separate S3 gameplay acceptance.

## S0 Validation Boundary

S0 creates documentation only. No Unity test, manual gameplay smoke, browser
smoke or Web build is required or claimed. No production asset is authorized
for Runtime integration by S0. Graybox fallback remains active.

## Forbidden Inference

S1-S3 completion and the approved Designer A character integration do not
authorize S4-S9, historical C5 continuation, Designer B/VFX/Audio Runtime
integration, PR, `main`, stable contest changes, external services, a second
ally, a new enemy archetype, breakage, runtime fracture or unrestricted
destruction. The Unity built-in Animation module registered by the integration
does not authorize any external package.
