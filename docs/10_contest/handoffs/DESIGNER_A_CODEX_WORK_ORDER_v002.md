# Designer A Codex Work Order v002

- Status: `OWNER-AUTHORIZED SOURCE-ART HANDOFF / REVIEW-FIRST / NO RUNTIME INTEGRATION`
- Branch: `art/characters-animation`
- Required Remote Baseline Before Work: `6030c4af6b800d0d7e80013382babd7b1cfc58a0`
- Substantive Authority: `docs/10_contest/owner_directives/2026-08-11/BREAK_NOTICE_GAMEPLAY_ANIMATION_COVERAGE_v003_FINAL.txt`
- Authority SHA-256: `6c5b6aca6e83686331a1d64d967d0489327aac35d285e0f2aa7da9fb1972bb69`
- Showcase Ticket: `CONTEST-0002`
- Due Date: `NOT ASSIGNED / HUMAN OWNER DECISION REQUIRED`

## Objective

Create an auditable animation-coverage plan before generating or exporting
more FBXs. Reuse compliant material where possible, archive incompatible
material honestly, and identify missing motion for CHR00, CHR01, ENY01 and
GEAR01 Showcase roles.

## Existing Batch Protection

- Preserve all `0810_designerA/**` files and commit history byte-for-byte.
- Existing batch status remains `REVISION REQUIRED / REVIEW HOLD / DO NOT INTEGRATE`.
- Rights, provenance and exact license remain unresolved.
- Do not move, rename, delete, overwrite, approve or Runtime-import the batch.
- Firearm, grenade, sword, elite and boss material is `ARCHIVE ONLY`.

## First Required Output

Create exactly:

`ArtSource/Characters/ANIMATION_COVERAGE_MATRIX_v001.csv`

Audit all existing 151 animation FBXs. Each row must classify the clip as one
of:

- `REUSE CANDIDATE`
- `ARCHIVE ONLY`
- `MISSING / NEW GENERATION REQUIRED`

At minimum record original path, original filename, proposed canonical clip,
actor, action family, loop/in-place/root-motion status, skeleton/retarget
compatibility, contact/prop dependency, rights status, preview status, known
defect and proposed next action.

`REUSE CANDIDATE` is not an approval. Human Owner preview PASS is required
before selected FBX and editable-source delivery.

## Allowed New Source Paths

- `ArtSource/Characters/CHR00_Player/**`
- `ArtSource/Characters/CHR01_Ally/**`
- `ArtSource/Characters/ENY01_ComplianceRunner/**`
- `ArtSource/Equipment/GEAR01_CargoStrap/**`

Do not create new character identity, model, face, costume or texture work.
Existing approved CHR00/CHR01/GEAR01 Canon and the shared normal ENY01 scope
must be preserved.

## AI-Assisted Animation Boundary

AI-assisted generation, retargeting, keyframe/loop cleanup, contact correction
and motion variation are allowed only under the v003 metadata contract. Each
candidate must record tool, model/version where available, source input,
license/terms, generation date, Human cleanup, retarget changes and remaining
rights risk. Missing provenance or license means `BLOCKED`.

AI assistance does not authorize Identity/model/face/costume/texture
generation and does not waive contact, loop, gameplay-camera or rights review.

## Preview-First Delivery

1. Submit low-cost front, side and gameplay-camera previews.
2. Include contact view and paired synchronization where required.
3. Use Binder, Chair and GEAR01 only as preview props; do not embed gameplay
   meshes permanently in Character FBX.
4. Wait for Human Owner preview PASS.
5. Export only selected FBX and editable source with complete metadata.

## Showcase Coverage Focus

- CHR00: movement/carry/throw/catch, physical impact response, Cargo Strap,
  rescue/tag/finisher participation.
- CHR01: Follow/Fetch, Binder Toss, Chair Feed/Stabilize, carabiner assist,
  Grab Rescue, Combat Tag and Finisher Partner.
- ENY01: one shared normal Runner body-check/shove/grab/stagger/recovery set;
  no firearm or elite Runtime direction.
- GEAR01: paired attach, tension handoff, redirect and release support.

Exact clip names, counts, phases and preview requirements remain those in the
v003 source directive; this handoff does not reduce them.

## Git and Integration Rules

- One-purpose commits and normal push to `art/characters-animation` only.
- No PR, merge, rebase, amend or force push.
- Do not modify Runtime code, Animator, Scene, Prefab, Input, Package, tests or
  build scripts.
- Do not import selected assets into the Showcase Runtime.
- S0 does not authorize S1–S9 implementation.

## Completion Evidence

Report matrix row count, all three classification counts, exact changed paths,
preview inventory, rights blockers, commit SHA and normal push result. Do not
claim Owner approval or Runtime integration.
