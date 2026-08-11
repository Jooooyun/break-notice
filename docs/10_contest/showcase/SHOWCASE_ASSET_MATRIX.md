# Showcase Asset Matrix

- Status: `S0 AUTHORITY MATRIX / NO RUNTIME ASSET APPROVAL`
- Ticket: `CONTEST-0002`
- Map Source SHA-256: `dfe3d2f312bb84dae7df7d596f4a7153f7ec69cfaac3590b93cbc6ab78833834`
- Designer A Source SHA-256: `6c5b6aca6e83686331a1d64d967d0489327aac35d285e0f2aa7da9fb1972bb69`
- Runtime Integration Boundary: `S9 ONLY / SEPARATE HUMAN OWNER AUTHORIZATION`

Classification vocabulary:

- `EXISTING OFFICE CORE KIT FAMILY`
- `EXISTING FAMILY MODULAR PIECE`
- `SHOWCASE-SPECIFIC NON-INTERACTIVE VARIANT`
- `REUSE CANDIDATE`
- `REVISION REQUIRED`
- `MISSING`
- `DEFERRED / REQUIRES SEPARATE EXCEPTION`

## Office Core Kit Mapping

The existing sixteen-family lock is preserved. Map-design names below are
deliverable or modular-piece names, not new family IDs.

| OCK Family | Showcase Names | Classification | Current State | Graybox Fallback |
|---|---|---|---|---|
| `OCK-A-001` Solid Office Wall System | `ENV_Wall_*`, outer corner | `EXISTING FAMILY MODULAR PIECE` | MISSING | primitive static wall |
| `OCK-A-002` Fixed Office Door and Frame | wall door module, `ENV_ExitDoor_Frame`, `ENV_SecurityShutter_VistaOnly` | `EXISTING FAMILY MODULAR PIECE` | MISSING | primitive fixed frame/shutter |
| `OCK-A-003` Low Cubicle Partition System | 1 m/2 m/3 m, corner, T-junction, end cap | `EXISTING FAMILY MODULAR PIECE` | MISSING | primitive low partition |
| `OCK-A-004` Office Floor Surface | `ENV_Floor_4m` | `EXISTING FAMILY MODULAR PIECE` | MISSING | primitive floor |
| `OCK-A-005` Suspended Ceiling System | `ENV_Ceiling_4m` | `EXISTING FAMILY MODULAR PIECE` | MISSING | primitive ceiling shell |
| `OCK-A-006` Fixed Column / Impact Surface | `ENV_Column_070` | `EXISTING FAMILY MODULAR PIECE` | MISSING | primitive column |
| `OCK-A-007` Neutral Fluorescent Ceiling Fixture | fixture A/B | `EXISTING FAMILY MODULAR PIECE` | MISSING | primitive fixture |
| `OCK-F-001` Fixed Work Desk | single/double/L-shape | `EXISTING FAMILY MODULAR PIECE` | MISSING | static desk primitive |
| `OCK-F-002` Wheeled Office Chair | `PRP_OfficeChair_Gameplay` | `REUSE CANDIDATE` | one named blockout candidate; technical review required | existing Chair proxy |
| `OCK-F-003` Rolling Drawer Cabinet | `PRP_RollingDrawer_Gameplay` | `REVISION REQUIRED` | unversioned source candidate | existing Drawer proxy |
| `OCK-F-004` Copier-Class Asset | `PRP_Copier_Recognition` | `REVISION REQUIRED` | duplicate intent unresolved | existing Copier proxy |
| `OCK-P-001` File Binder | `PRP_FileBinder_Gameplay` | `REVISION REQUIRED` | invalid/unversioned source name | existing Binder proxy |
| `OCK-D-001` Workstation Electronics Cluster | monitor A/B/C, keyboard/phone, visual-only cable | `EXISTING FAMILY MODULAR PIECE` | MISSING | omit or primitives |
| `OCK-D-002` Paper and Archive Box Cluster | paper stack A/B, archive box A/B | `EXISTING FAMILY MODULAR PIECE` | MISSING | omit or primitives |
| `OCK-D-003` Office Plant and Waste Bin Cluster | plant A/B, waste bin A/B | `EXISTING FAMILY MODULAR PIECE` | MISSING | omit or primitives |
| `OCK-D-004` Signage, Tag, Tape and Wear Set | floor tape, asset tag, wayfinding, parking, marks/scuffs | `SHOWCASE-SPECIFIC NON-INTERACTIVE VARIANT` | MISSING | text/primitive markers |

`ENV_SecurityShutter_VistaOnly` is a non-interactive modular vista piece under
`OCK-A-002`. It is not a seventeenth Prototype-required family and grants no
door, transition, breakable or gameplay authority.

## Designer B Remote Candidate Batch

- Remote head: `7536f551fca146e225794219bc219d7f07dd5314`
- Files: `9`
- DELIVERY metadata, preview and rights evidence: `MISSING`
- Runtime approved files: `0`

| Existing Source | Classification | Required Disposition |
|---|---|---|
| `ArtSource/Props/PRP_OfficeChair/Source/PRP_OfficeChair_A_Blockout_v001.blend` | `REUSE CANDIDATE` | preserve; technical review and preview required |
| `ArtSource/Props/PRP_OfficeChair/Source/officeChair.blend` | `REVISION REQUIRED` | preserve; unversioned name |
| `ArtSource/Props/PRP_Copier/Source/Copier.blend` | `REVISION REQUIRED` | preserve; duplicate intent unresolved |
| `ArtSource/Props/PRP_Copier/Source/copier_2.blend` | `REVISION REQUIRED` | preserve; duplicate intent unresolved |
| `ArtSource/Props/PRP_DrawerCabinet/Source/DrawerCabinet.blend` | `REVISION REQUIRED` | preserve; unversioned name |
| `ArtSource/Props/PRP_FileBinder/Source/Untitled.blend` | `REVISION REQUIRED` | preserve; invalid final name |
| all three `.blend1` files | `DEFERRED / REQUIRES SEPARATE EXCEPTION` | archive backup only; never final delivery |

Designer B's first v002 task is candidate/missing-item audit, clean versioned
future naming and preview preparation. Existing files must not be deleted,
renamed or overwritten during that work.

## Character, Animation and Equipment

| ID | Need | Classification | Current State | Graybox Fallback |
|---|---|---|---|---|
| CHR00 | Player model/rig and approved interaction coverage | `REVISION REQUIRED` | existing 0810 batch on REVIEW HOLD; rights unresolved | existing Player proxy |
| CHR01 | Mobile Ally model/rig and Follow/Fetch/Supply/Brace/Rescue/Tag/Finisher coverage | `REVISION REQUIRED` | existing batch not integrated; coverage matrix first | existing Ally proxy |
| ENY01 | one Runner model/rig used twice and approved physical-pressure coverage | `REVISION REQUIRED` | normal candidate unapproved; elite/firearm content archive only | identical Runner proxies |
| GEAR01 | reel, strap, carabiner, clamp and Ally-assisted attach coverage | `MISSING` | no technically accepted Runtime delivery | line/primitive representation |

First required Designer A output is
`ArtSource/Characters/ANIMATION_COVERAGE_MATRIX_v001.csv`, classifying all 151
existing animation FBXs as `REUSE CANDIDATE`, `ARCHIVE ONLY` or
`MISSING / NEW GENERATION REQUIRED`. Preview passes precede selected FBX and
editable-source delivery.

## Showcase Presentation Candidates

| Item | Classification | Boundary |
|---|---|---|
| small paper burst, Chair wheel dust, Drawer/wall impact dust | `SHOWCASE-SPECIFIC NON-INTERACTIVE VARIANT / MISSING` | non-destructive presentation only |
| subtle Tag contact flash | `SHOWCASE-SPECIFIC NON-INTERACTIVE VARIANT / MISSING` | no magic/neon gameplay |
| Exit light activation | `SHOWCASE-SPECIFIC NON-INTERACTIVE VARIANT / MISSING` | result readability only |
| Objective/Q/Readiness/result UI | `MISSING` | S9 integration only after mechanic approval |
| Audio and marketing delivery | `MISSING` | exact rights/license required |

## Runtime Decision

No current Designer A or B candidate is Owner-approved for Runtime. S0 changes
no ArtSource or Unity asset. No production asset is authorized for Runtime
integration by S0. Graybox fallback remains active.
