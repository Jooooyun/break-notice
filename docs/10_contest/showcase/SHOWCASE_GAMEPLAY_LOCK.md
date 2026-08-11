# Showcase Gameplay Lock

- Status: `OWNER-APPROVED SHOWCASE DESIGN LOCK / IMPLEMENTATION NOT STARTED / S1 NOT AUTHORIZED`
- Ticket: `CONTEST-0002`
- Room: `PRT-RM-002 — Operations Floor Handover Lockdown`
- Branch: `contest/game-builders-2026-showcase`
- Baseline: `15c915c4c7552113941149e84ba5cc1fea9c63f1`
- Source: `BREAK_NOTICE_CONTEST_SHOWCASE_MAP_DESIGN_v001.txt`
- Source SHA-256: `dfe3d2f312bb84dae7df7d596f4a7153f7ec69cfaac3590b93cbc6ab78833834`

## Lineage Boundary

This lock applies only to `CONTEST-0002 / PRT-RM-002`. It preserves
`CONTEST-0001 / PRT-RM-001` and
`Game/Assets/_Project/Scenes/10_Prototype/Contest_SingleRoomGraybox.unity`
as the accepted C0–C4 regression foundation. S0 creates no Unity Scene.

## Experience Lock

- 48 m × 34 m flat open-office
- 4.2 m ceiling
- central Cubicle Island
- west Ally collaboration/supply loop
- east Chair/Strap/physics lane
- north Drawer objective, optional team finisher and short Exit run
- figure-eight topology with three approaches
- target play time 5–7 minutes
- target response: finish with enough unseen route/tag depth to replay

Zones are Z0 Entry Staging, Z1 Direct Equipment Bay, Z2 Central Clash Bowl,
Z3 West Logistics Loop, Z4 Central Cubicle Ring, Z5 East Physics Lane, Z6
North Tag Arena/Drawer Gate and Z7 Exit Run/Next-Area Tease.

## Content Lock

| Content | Count / Role |
|---|---|
| CHR_00 Player | 1 |
| CHR_01 mobile Ally | 1; Follow, Fetch, Supply, Brace, Rescue, Combat Tag, Finisher Partner |
| ENY01 Compliance Runner | 2 instances of one archetype |
| File Binder | 5 |
| Wheeled Office Chair | 3 |
| Rolling Drawer Cabinet | 1 |
| Copier-Class Asset | 1; recognition-only |

No second active Ally, new enemy archetype, boss, firearm, hidden inventory,
teleport supply, automatic object deletion or replenishment is authorized.

## Ally and Q Lock

The Ally is a moving collaborator, not a stationary vending device. The
approved responsibility vocabulary is Follow, Fetch, Supply, Brace, Rescue,
Combat Tag and Finisher Partner.

Only one Q prompt may be displayed. Context priority is:

1. `BREAK GRAB`
2. `TEAM FINISH`
3. `ASSIST ATTACH`
4. `TAG SHOVE / SET HIM UP`
5. `SEND BINDER / SEND CHAIR`
6. `REGROUP`

Exact runtime arbitration, timing and implementation remain S4+ work and are
not implemented by this lock.

## Team Readiness Lock

- PASS: catch an Ally-supplied Binder.
- BRACE: complete Ally-assisted carabiner attachment.
- BREAK: complete Grab Rescue or Combat Tag.
- Any two distinct badges create TEAM READY.
- Badges do not decay, spend or persist beyond the run.

## Drawer and Finisher Lock

The one Drawer may be resolved through Direct Chair Throw, Strap Redirect,
Runner Causal Collision or Team Finisher. The optional finisher is
`FIN-P-001 — HANDOVER CRASH`, approximately 2.5 seconds, with bounded camera,
physical Chair → Runner → Drawer causality and immediate control return.
Direct Chair and Strap routes must remain viable without the Finisher.

## Physical and Presentation Boundaries

- Floor remains flat; thresholds, steps and small collision clutter are banned.
- Missed or used physical objects remain in the room until Reset.
- Destruction, runtime fracture and arbitrary breakage remain absent.
- Paper, wheel dust, impact dust, subtle contact flash and Exit-light VFX are
  non-destructive presentation candidates only.
- Existing Office Core Kit families are reused; no unauthorized seventeenth
  Prototype family may be inferred.

## Implementation Boundary

S0 registers design authority only. S1 through S9 each require a new one-shot
relay. Runtime code, Scene, Prefab, Input, Package, Art integration, tests,
build, browser work, PR and merge are not authorized here.
