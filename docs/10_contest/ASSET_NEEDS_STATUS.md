# Asset Needs Status

Status vocabulary는 `ART_DELIVERY_STANDARD.md`를 따른다. 승인 visual
direction의 존재와 runtime deliverable의 존재를 혼동하지 않는다.

| Asset ID | Asset Name | Owner | Priority | Blocking Feature | Exact Required Files | Source Path | Expected Runtime Path | Status | Due | Latest Commit/PR | Missing Item | Defect | Fallback | Human Owner Decision |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| CHR00 | Player | Designer A | P0 | C8 character integration | `CHR00_Player_v001.fbx`, 2K PBR set, Humanoid rig, required in-place clips, preview, DELIVERY | `ArtSource/Characters/CHR00_Player/` | `Game/Assets/_Project/Art/Characters/CHR00/` | MISSING | 2026-08-19 | — | All runtime files | — | Capsule Player | Visual reference approved; runtime asset not approved |
| CHR01 | Fixed Ally | Designer A | P0 | C4 presentation / C8 | `CHR01_Ally_v001.fbx`, 2K PBR set, Humanoid rig, Binder/Chair supply clips, preview, DELIVERY | `ArtSource/Characters/CHR01_Ally/` | `Game/Assets/_Project/Art/Characters/CHR01/` | MISSING | 2026-08-11 | — | All runtime files | — | Capsule + code-driven supply | Visual reference approved; runtime asset missing |
| ENY01 | Compliance Runner | Designer A | P0 | C7 presentation / C8 | one FBX, PBR set, Humanoid rig, Move/Pressure/Reaction/Stagger/Reset clips, preview, DELIVERY | `ArtSource/Characters/ENY01_ComplianceRunner/` | `Game/Assets/_Project/Art/Characters/ENY01/` | MISSING | 2026-08-17 | — | Identity and all runtime files | No approved identity visual | Capsule + code-driven movement | One shared model for two instances |
| GEAR01 | Cargo Strap | Designer A | P0 | C5 presentation / C8 | Reel, carabiner, clamp FBX; strap visual; valid chair anchor; texture; preview; DELIVERY | `ArtSource/Equipment/GEAR01_CargoStrap/` | `Game/Assets/_Project/Art/Equipment/GEAR01/` | MISSING | 2026-08-13 | — | All runtime files | — | LineRenderer + primitive reel/clamp | Visual reference approved; runtime asset missing |
| PRP-BINDER | File Binder | Designer B | P0 | C2 readability / C8 | FBX, 1K PBR set, preview, DELIVERY | `ArtSource/Props/PRP_FileBinder/` | `Game/Assets/_Project/Art/Props/Gameplay/` | MISSING | 2026-08-08 | — | All runtime files | — | Primitive box | Three runtime instances |
| PRP-CHAIR | Office Chair | Designer B | P0 | C2/C5 readability / C8 | FBX with load-bearing frame readability, 1–2K PBR set, preview, DELIVERY | `ArtSource/Props/PRP_OfficeChair/` | `Game/Assets/_Project/Art/Props/Gameplay/` | MISSING | 2026-08-08 | — | All runtime files | — | Primitive grounded proxy | Two runtime instances |
| PRP-DRAWER | Drawer Cabinet | Designer B | P0 | C6 objective / C8 | FBX, 1–2K PBR set, preview, DELIVERY | `ArtSource/Props/PRP_DrawerCabinet/` | `Game/Assets/_Project/Art/Props/Gameplay/` | MISSING | 2026-08-15 | — | All runtime files | — | Primitive drawer | One runtime instance |
| PRP-COPIER | Copier | Designer B | P0 | Room recognition / C8 | FBX, 1K PBR set, preview, DELIVERY | `ArtSource/Props/PRP_Copier/` | `Game/Assets/_Project/Art/Props/Gameplay/` | MISSING | 2026-08-15 | — | All runtime files | — | Primitive recognition proxy | Recognition-only; no interaction |
| ENV-OFFICE | Office Core Kit | Designer B | P0 | C8 environment | approved 16-family scope exports, materials/textures, preview, DELIVERY | `ArtSource/Environment/OfficeCoreKit/` | `Game/Assets/_Project/Art/Environment/OfficeCore/` | MISSING | 2026-08-19 | — | All runtime exports | — | Primitive Contest room | No new gameplay definitions |
| UI-CONT | Contest UI | Designer B | P0 | C6/C8/C9 | Title, controls, objective, prompts, ally/carry/strap/route, failure/win/retry/pause/mute/credits exports, source, font license, DELIVERY | `ArtSource/UI/` | `Game/Assets/_Project/Art/UI/` | MISSING | 2026-08-20 | — | All files and font license | — | Text UI | Runtime integration after loop works |
| VFX-CONT | Contest VFX | Designer B | P1 | C8 polish | approved effect textures/flipbooks, previews, DELIVERY | `ArtSource/VFX/` | `Game/Assets/_Project/Art/VFX/` | MISSING | 2026-08-21 | — | All files | — | Code/material feedback | No magic/neon/destruction expansion |
| AUD-CONT | Contest Audio | Designer B | P1 | C8 polish | licensed WAV exports, source/licenses, DELIVERY | `ArtSource/Audio/` | `Game/Assets/_Project/Audio/` | MISSING | 2026-08-21 | — | All files/licenses | — | Silent build | No unlicensed audio |
| MKT-CONT | Thumbnail/Video | Designer B | P0 | C9 submission | 16:9 thumbnail source/export, ≤3 min H.264 gameplay video/source, license evidence, DELIVERY | `ArtSource/Marketing/` | Submission package | MISSING | 2026-08-24 | — | All files | — | Owner-captured minimal gameplay assets | Runtime asset 아님 |

## C1-C3 Completion and C4 Entry Readiness

No new production asset blocks this work package. Graybox fallback remains active.
C1-C3 implementation and automated acceptance are complete. C4 is not
authorized and production assets remain separate from runtime implementation.

Approved directions are available for CHR00, CHR01, GEAR01, camera,
chair-redirect composition, aftermath material response and local lighting.
ENY01 runtime identity/art, all production FBX/texture/rig/animation, Office Kit,
UI/VFX/audio and marketing exports remain missing.

## 11. Approved Visual Reference Coverage

### Designer A

- Required Approved References:
  `CHR_00_Player_Identity_Lock_v002_FINAL.png`,
  `CHR_00_Player_Turnaround_v001_FINAL.png`,
  `CHR_01_First_Ally_Identity_v001_FINAL.png`,
  `GEAR_01_Cargo_Strapping_System_v002_FINAL.png`, PGT_02 action composition
- Available: all above at tracked `references/approved/` paths
- Missing: approved ENY01 identity reference; runtime FBX/rig/animation for all
- Unresolved: none for existing approved reference status
- Referenced Runtime Assets: existing Player/Camera graybox only
- Production Deviation: none approved

### Designer B

- Required Approved References: PGT_01, PGT_02, PGT_03, LGT_01 plus Office
  Core Kit and Art Direction documents
- Available: all listed visual references and documents
- Missing: runtime Office Kit/props/UI/VFX/audio/marketing deliverables
- Unresolved: contest-wide lighting is not defined by local LGT_01
- Referenced Runtime Assets: primitive room and seven object proxies
- Production Deviation: none approved

## Designer A Remote Delivery Audit — 2026-08-10

- Source Branch: `origin/art/characters-animation`
- Source Batch Tip: `6094b41143286b6e1710087a711707416b366c20`
- Corrective Instruction: `6030c4af6b800d0d7e80013382babd7b1cfc58a0`
- Inventory: `159 files / 155 FBX / 4 guides / 131,363,044 path bytes`
- Classification: `REVISION REQUIRED / REVIEW HOLD / NOT INTEGRATED`
- Delivered Candidates: `main_character`, `female_helper`, `enemy_normal`, `enemy_elite`
- Missing A1 Evidence: editable CHR01 DCC source, blockout preview, Binder supply
  pose, Chair push pose, completed `DELIVERY.md`
- Rights Gap: creator, source, exact license, written permission, AI/generated
  element record and third-party IP check are unresolved
- Scope Gap: files are outside allowed `ArtSource/**` paths; ENY01 concept approval
  was skipped; elite, firearm and grenade content conflicts with the locked scope
- Git Gap: no PR; no LFS filter; existing history must not be rewritten
- Integration Decision: `DO NOT IMPORT / DO NOT MERGE / HUMAN OWNER APPROVAL ABSENT`
- Next Request to Designer A: complete only the five-file CHR01 A1 blockout and
  pose package in the approved path with full provenance and license metadata

No new production asset blocks this work package. Graybox fallback remains active.
