# ADR-0024 — Preserve CONTEST-0001 and Create Operations Floor Showcase

- Status: `ACCEPTED`
- Date: `2026-08-11`
- Decision Source: `Human Owner Direct Authorization / HOD-CONTEST-0002-S0-001`
- Change Control: `CR-0004`
- Ticket: `CONTEST-0002`
- Baseline: `15c915c4c7552113941149e84ba5cc1fea9c63f1`
- Branch: `contest/game-builders-2026-showcase`

## Context

`CONTEST-0001` and `Contest_SingleRoomGraybox.unity` contain accepted C0–C4
movement, camera, interaction, carry, throw, Reset and ally-supply regression
evidence. The Human Owner's later Showcase direction changes room scale,
object counts, ally mobility and responsibility, Q context, objective routes
and climax structure. Treating that redesign as the old C5 would falsify the
accepted record and destabilize the regression harness.

## Decision

1. Preserve `CONTEST-0001` and its existing Scene without modification.
2. Create a separate `CONTEST-0002` authority lineage on
   `contest/game-builders-2026-showcase` from exact baseline `15c915c4…`.
3. Use `Contest_OperationsFloor_Showcase_v001.unity` only as a future S1
   Scene. S0 creates no Scene.
4. Reuse C0–C4 behavior as a regression dependency, not as proof that the
   Showcase has been implemented.
5. Execute Showcase work in S0–S9 order with a fresh one-shot relay for each
   package.
6. Keep Designer A/B source work on their own art branches; no art branch may
   author or merge the gameplay Scene.

## Rejected Alternatives

- Overwrite or enlarge `Contest_SingleRoomGraybox.unity`.
- Continue historical C5 directly on the stable contest branch.
- Modify `main` or merge the complete contest branch into `main`.
- Merge current art branches or unreviewed binaries into Runtime.
- Infer S1 implementation authority from acceptance of this ADR.

## Consequences

- The accepted single-room build remains a stable regression fixture.
- `PRT-RM-002` can differ from `PRT-RM-001` without retroactive edits.
- Status files must distinguish design registration from implementation.
- The Gameplay Animation v003 and Showcase Map v001 texts are substantive
  Human Owner sources; the Reconciliation handoff is procedural evidence.
- Current Designer A/B binaries remain candidates with unresolved technical,
  naming or rights issues and no Runtime approval.

## Non-Authorization Boundary

ADR acceptance records the split only. S1, C5, Runtime code, Unity Scene or
Prefab edits, packages, Input Actions, build/browser work, Runtime Art
integration, PR and merge remain unauthorized.
