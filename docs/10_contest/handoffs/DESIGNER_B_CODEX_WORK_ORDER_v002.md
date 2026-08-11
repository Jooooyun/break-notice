# Designer B Codex Work Order v002

- Status: `OWNER-AUTHORIZED SOURCE-ASSET AUDIT HANDOFF / NO RUNTIME INTEGRATION`
- Branch: `art/environment-ui-vfx`
- Required Remote Baseline Before Work: `7536f551fca146e225794219bc219d7f07dd5314`
- Substantive Authority: `docs/10_contest/owner_directives/2026-08-11/BREAK_NOTICE_CONTEST_SHOWCASE_MAP_DESIGN_v001.txt`
- Authority SHA-256: `dfe3d2f312bb84dae7df7d596f4a7153f7ec69cfaac3590b93cbc6ab78833834`
- Showcase Ticket: `CONTEST-0002`
- Due Date: `NOT ASSIGNED / HUMAN OWNER DECISION REQUIRED`

## Objective

Audit the current nine source files, map the Showcase asset list to the locked
Office Core Kit families and prepare clean, versioned source candidates and
previews. Do not place assets in Unity Runtime or author the Gameplay Scene.

## Branch Precondition

The local art branch was observed one commit behind the remote. S0 does not
switch to or fast-forward it. Before Designer B work begins, verify a clean
worktree and obtain separate Human Owner authorization for a normal
fast-forward to `7536f551fca146e225794219bc219d7f07dd5314`.

## Existing Nine-File Batch

Preserve every file and its history. Do not delete, rename or overwrite it.

| File | Current Disposition |
|---|---|
| `ArtSource/Props/PRP_OfficeChair/Source/PRP_OfficeChair_A_Blockout_v001.blend` | `REUSE CANDIDATE / TECHNICAL REVIEW REQUIRED` |
| `ArtSource/Props/PRP_OfficeChair/Source/officeChair.blend` | `REVISION REQUIRED / UNVERSIONED` |
| `ArtSource/Props/PRP_Copier/Source/Copier.blend` | `REVISION REQUIRED / DUPLICATE INTENT UNRESOLVED` |
| `ArtSource/Props/PRP_Copier/Source/copier_2.blend` | `REVISION REQUIRED / DUPLICATE INTENT UNRESOLVED` |
| `ArtSource/Props/PRP_DrawerCabinet/Source/DrawerCabinet.blend` | `REVISION REQUIRED / UNVERSIONED` |
| `ArtSource/Props/PRP_FileBinder/Source/Untitled.blend` | `REVISION REQUIRED / INVALID FINAL NAME` |
| all three `.blend1` files | `ARCHIVE BACKUP / NOT FINAL DELIVERY` |

Current Runtime approval count is `0`. DELIVERY metadata, preview and rights
evidence are missing.

## First Required Output

Use `docs/10_contest/showcase/SHOWCASE_ASSET_MATRIX.md` as the audit contract.
For each Map v001 item record:

- existing Office Core Kit family ID;
- modular piece or Showcase-specific non-interactive variant;
- existing candidate path, if any;
- missing source/export/texture/preview/DELIVERY/license evidence;
- proposed clean versioned future filename;
- expected technical review;
- graybox fallback.

`ENV_SecurityShutter_VistaOnly` belongs under `OCK-A-002` as a non-interactive
fixed-door/frame modular vista piece. Do not create a seventeenth Prototype
family.

## Production Priority

1. Gameplay-object candidate audit: Chair, Drawer, Copier, Binder.
2. Architecture modular mapping: wall, door/frame, low partition, floor,
   ceiling, column, neutral fluorescent fixture and fixed desk.
3. Non-interactive dressing variants under the four existing dressing
   families.
4. Preview-only VFX/UI inventory for PASS/BRACE/BREAK, Drawer result and Exit.

This order requests source assets and previews only. Final runtime export,
material, collider, Prefab and Scene placement belong to the Human Owner/Codex
integration process under a later S9 relay.

## Naming and Delivery

Future candidates must use clean versioned names, the approved ArtSource path,
preview files and complete DELIVERY/rights metadata. Do not rename the current
nine files in place; add a corrected candidate only after its identity and
scope are explicit.

## Git and Integration Rules

- One-purpose commits and normal push to `art/environment-ui-vfx` only.
- No PR, merge, rebase, amend or force push.
- Do not modify Runtime code, Scene, Prefab, Input, Package, tests, build or
  current graybox assets.
- Do not integrate any source into Runtime.
- S0 does not authorize S1–S9 implementation.

## Completion Evidence

Report preserved nine-file inventory, matrix coverage, candidate/revision/
missing counts, preview inventory, rights blockers, exact changed paths,
commit SHA and push result. Do not claim Runtime approval.
