# Asset Needs Status

- Current Authority Phase: `CONTEST-0002 S0 COMPLETE / S1 NOT AUTHORIZED`
- Runtime Integration Boundary: `S9 ONLY / SEPARATE HUMAN OWNER AUTHORIZATION`
- Runtime-approved Designer A/B production assets: `0`
- Graybox Policy: `ACTIVE`

Status vocabulary follows `ART_DELIVERY_STANDARD.md`. Approved visual
direction, source-art candidacy, technical acceptance, Human Owner approval and
Runtime integration are different states and must not be conflated.

## Live Showcase Needs

| Asset ID | Asset / Owner | Blocking Package | Exact Next Evidence | Current Source State | Missing / Defect | Fallback | Human Owner Decision |
|---|---|---|---|---|---|---|---|
| CHR00 | Player / Designer A | S9 only; S1–S8 use graybox | coverage-matrix rows, approved previews, selected FBX/editable source after preview PASS, rights metadata | 0810 batch `REVIEW HOLD / DO NOT INTEGRATE` | provenance/license and technical acceptance unresolved | existing Player proxy | identity visual approved; Runtime asset not approved |
| CHR01 | Mobile Ally / Designer A | S2–S4 animation readability; no production blocker | Follow/Fetch/Supply/Brace/Rescue/Tag/Finisher coverage rows and preview candidates | 0810 batch preserved on REVIEW HOLD | rights, source identity, preview and selected delivery unresolved | existing Ally proxy / code-driven future graybox | mobile Showcase role is design-approved; implementation and Runtime asset not approved |
| ENY01 | Compliance Runner / Designer A | S6 readability; no production blocker | one normal Runner coverage set and preview candidates for body check/shove/grab/stagger/recovery | normal/elite candidates preserved; no integration | identity/provenance/license unresolved; elite/firearm material archive only | two identical Runner proxies | one shared normal archetype only; Runtime asset not approved |
| GEAR01 | Cargo Strap / Designer A | S5 presentation; no production blocker | attach/tension/redirect/release coverage, reel/carabiner/clamp source and previews | no accepted delivery | source, export, rights, preview and DELIVERY missing | line/primitive reel/clamp | visual Canon exists; Runtime asset not approved |
| PRP-BINDER | Binder / Designer B | S3/S7 readability; no production blocker | clean versioned source candidate, preview, export plan, DELIVERY/rights | `Untitled.blend` candidate | invalid final name; `.blend1` archive; no metadata | existing Binder proxy | Showcase count 5; no Runtime asset approved |
| PRP-CHAIR | Chair / Designer B | S3/S5/S7 readability; no production blocker | technical review of named blockout, load-bearing anchor preview, clean export plan, DELIVERY/rights | `PRP_OfficeChair_A_Blockout_v001.blend` reuse candidate | duplicate/unversioned candidate and `.blend1` archive | existing Chair proxy | Showcase count 3; no Runtime asset approved |
| PRP-DRAWER | Drawer / Designer B | S7 objective readability; no production blocker | clean versioned source, Parking Bay/objective preview, export plan, DELIVERY/rights | `DrawerCabinet.blend` candidate | unversioned; no metadata/preview/export | existing Drawer proxy | Showcase count 1; no Runtime asset approved |
| PRP-COPIER | Copier / Designer B | recognition only | resolve two candidates, clean versioned source, preview and rights | `Copier.blend`, `copier_2.blend` | duplicate intent unresolved; `.blend1` archive | existing Copier proxy | Showcase count 1 / recognition-only; no Runtime asset approved |
| ENV-OFFICE | Office Core Kit / Designer B | S1 graybox does not wait; S9 visual integration | Map v001 inventory mapped to existing 16 families, modular previews, versioned sources, DELIVERY/rights | no accepted environment delivery | architecture/dressing source and evidence missing | primitive 48 m × 34 m graybox after S1 authorization | no seventeenth family; shutter maps to `OCK-A-002` |
| UI-SHOWCASE | Q/Readiness/Objective/Result UI / Designer B | S4/S7/S8 readability; graybox text allowed | source/export preview, font rights, state mapping and DELIVERY | MISSING | all deliverables and rights missing | text UI | S9 Runtime integration only |
| VFX-SHOWCASE | Tag/Chair/Drawer/Exit feedback / Designer B | S9 polish | restrained preview textures/flipbooks and DELIVERY/rights | MISSING | all deliverables missing | no effect or code/material feedback | non-destructive and non-magical only |
| AUD-SHOWCASE | Gameplay/UI audio / Designer B | S9 polish | licensed WAV/source/license/DELIVERY | MISSING | exact rights and exports missing | silent build | no unlicensed audio |
| MKT-SHOWCASE | Thumbnail/video / Designer B | release package | source/export and license ledger | MISSING | all files missing | Owner-captured minimal assets | not a Runtime asset |

## Designer A Delivery State

- Remote branch: `origin/art/characters-animation`
- Required head: `6030c4af6b800d0d7e80013382babd7b1cfc58a0`
- Existing binary/history: `PRESERVE`
- Existing batch: `REVISION REQUIRED / REVIEW HOLD / DO NOT INTEGRATE`
- Rights/provenance: `UNRESOLVED`
- First output:
  `ArtSource/Characters/ANIMATION_COVERAGE_MATRIX_v001.csv`
- Required classification of existing 151 Animation FBXs:
  `REUSE CANDIDATE / ARCHIVE ONLY / MISSING / NEW GENERATION REQUIRED`
- New source paths allowed by Animation v003 only:
  `CHR00_Player/**`, `CHR01_Ally/**`, `ENY01_ComplianceRunner/**`,
  `GEAR01_CargoStrap/**`
- AI-assisted animation: allowed only with v003 provenance, license and Human
  cleanup metadata; no new Identity/model/face/costume/texture generation.
- Runtime integration: `NOT AUTHORIZED`.

## Designer B Delivery State

- Remote branch: `origin/art/environment-ui-vfx`
- Required remote head: `7536f551fca146e225794219bc219d7f07dd5314`
- Local branch observed: one commit behind; S0 performs no switch or fast-forward.
- Existing source files: `9 / PRESERVE`
- Runtime approved files: `0`
- DELIVERY/preview/rights evidence: `MISSING`
- Reuse candidate:
  `PRP_OfficeChair_A_Blockout_v001.blend / TECHNICAL REVIEW REQUIRED`
- Revision required: `officeChair.blend`, `Copier.blend`, `copier_2.blend`,
  `DrawerCabinet.blend`, `Untitled.blend`.
- All `.blend1`: `ARCHIVE BACKUP / NOT FINAL DELIVERY`.
- First task: reconcile the remote batch and Map v001 asset list against
  `SHOWCASE_ASSET_MATRIX.md`; create source candidates/previews only.
- Runtime integration: `NOT AUTHORIZED`.

## Approved Visual Reference Coverage

### Designer A

- Available: approved CHR00 Identity and Turnaround, CHR01 Identity, GEAR01,
  PGT_02 action composition.
- Missing: approved ENY01 identity reference; all technically accepted Runtime
  model/rig/animation/equipment exports.
- Production deviation: none approved.

### Designer B

- Available: PGT_01, PGT_02, PGT_03, LGT_01, Office Core Kit and Art Direction.
- Missing: technically accepted Office Kit, gameplay props, UI, VFX, audio and
  marketing deliveries.
- Boundary: LGT_01 is Records Archive-local and not global Showcase lighting.
- Production deviation: none approved.

## Current Readiness Decision

S0 is documentation-only and requires no production asset. S1 graybox must use
primitive fallback if separately authorized. Source candidates may proceed only
under the relevant v002 handoff and art-branch authority; Runtime integration
remains deferred to S9.

No production asset is authorized for Runtime integration by S0.
Graybox fallback remains active.
