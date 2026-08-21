# CODEX WORK ORDER — DESIGNER B

> 담당 영역: Environment / Gameplay Props / Materials / UI / VFX / Marketing  
> 실행 브랜치: `art/environment-ui-vfx`  
> PR Base: `contest/game-builders-2026`  
> Human Owner: 조윤성

---

# 0. 최상위 명령

## Human Owner Direct Production Start

- Authority baseline: `fc361a4dc42492e9b997ee42ac8bab3a6fdec2d2`
- Authorized branch: `art/environment-ui-vfx`
- Active package: `B1 — Office Chair editable 3D blockout, anchor and silhouette review batch`
- Start: `IMMEDIATE / DO NOT WAIT FOR C0 OR C1`

계획·조사·`DELIVERY.md` 수정만 하고 종료하지 않는다. B1의 완료 증거에는
실제로 다시 열고 편집할 수 있는 3D Blockout source와 그 source에서 만든
세 Preview가 반드시 함께 있어야 한다. Human Owner review는 Production
Mesh/UV/Texture 진입 Gate이며 Blockout 제작 시작 Gate가 아니다.

이 저장소에서 네 역할은 Gameplay Code나 Scene을 구현하는 것이 아니다.

네 역할은 Designer B를 보조해 다음 Source Art Delivery를 순서대로 만드는 것이다.

1. Office Chair
2. File Binder
3. Drawer Cabinet
4. Copier
5. Office Core Kit P0
6. Contest UI
7. Physical VFX / Licensed Audio
8. Thumbnail / Demo Video

항상 다음 우선순위를 따른다.

```text
Gameplay readability
→ approved reference fidelity
→ correct scale/pivot/export
→ WebGL budget
→ delivery completeness
→ decorative polish
```

환경 장식이 Gameplay Object·Side Route·EXIT를 가리지 않게 한다.

---

# 1. 시작 전 Git 검증

```text
git fetch --all --tags
git switch art/environment-ui-vfx
git pull --ff-only origin art/environment-ui-vfx
git status --short --branch
git rev-parse HEAD
git rev-parse origin/art/environment-ui-vfx
```

정상 조건:

- branch = `art/environment-ui-vfx`
- HEAD = remote branch
- tracked/staged change = 0
- unknown local work = 0 또는 사용자 소유로 식별
- `DESIGNER_B_HUMAN_GUIDE.md` 존재
- `ART_DELIVERY_STANDARD.md` 존재

Hard Stop:

- branch mismatch
- unknown work
- merge conflict
- approved reference missing
- unknown font/audio/texture license
- large binary policy unclear
- Source Art를 위해 Gameplay Scene/Code/Package 수정 필요

금지:

- `git clean`
- `git reset --hard`
- `git add .`
- `git add -A`
- force push
- pushed commit amend

정확한 Delivery 경로만 Stage한다.

---

# 2. 읽어야 할 권위

```text
docs/10_contest/handoffs/DESIGNER_B_HUMAN_GUIDE.md
docs/10_contest/ART_DELIVERY_STANDARD.md
docs/10_contest/ASSET_NEEDS_STATUS.md
docs/10_contest/VISUAL_REFERENCE_INVENTORY.md
docs/03_art/ART_DIRECTION.md
docs/03_art/OFFICE_CORE_KIT.md
docs/03_art/CAMERA_SPEC.md
docs/03_art/CHAIR_REDIRECT_VISUAL_SPEC.md
docs/03_art/CHAIN_REACTION_AFTERMATH_VISUAL_SPEC.md
docs/03_art/NIGHT_ARCHIVE_LIGHTING_VISUAL_SPEC.md
docs/03_art/CARGO_STRAP_SPEC.md
docs/02_design/PROTOTYPE_CONTENT_LOCK.md
```

Authority:

1. Human Owner 최신 지시
2. Approved Specs/ADRs
3. tracked `references/approved/` FINAL
4. Contest delivery 문서
5. non-generative automation validation

---

# 3. 허용 경로

생성·수정 가능:

```text
ArtSource/Environment/OfficeCoreKit/**
ArtSource/Props/PRP_FileBinder/**
ArtSource/Props/PRP_OfficeChair/**
ArtSource/Props/PRP_DrawerCabinet/**
ArtSource/Props/PRP_Copier/**
ArtSource/UI/**
ArtSource/VFX/**
ArtSource/Audio/**
ArtSource/Marketing/**
```

수정 금지:

```text
Game/Assets/_Project/Code/**
Game/Assets/_Project/Scenes/**
Game/Assets/_Project/Prefabs/**
Game/Packages/**
Game/ProjectSettings/**
tools/**
references/approved/**
references/exploration/**
```

Runtime Import, `.meta`, Material, Collider, Rigidbody, UI Binding, Particle System, Scene Placement은 integration branch에서 처리한다.

---

# 4. 승인 레퍼런스 패키지

## Primary

```text
references/approved/gameplay/PGT_01_Gameplay_Camera_v002_FINAL.png
references/approved/gameplay/PGT_02_Chair_Redirect_Core_Loop_v010_FINAL.png
references/approved/gameplay/PGT_03_Chain_Reaction_Aftermath_v003_FINAL.png
references/approved/lighting/LGT_01_Night_Archive_Lighting_v001_FINAL.png
```

## Scale/Material Support

```text
references/approved/characters/CHR_00_Player_Identity_Lock_v002_FINAL.png
references/approved/characters/CHR_01_First_Ally_Identity_v001_FINAL.png
references/approved/equipment/GEAR_01_Cargo_Strapping_System_v002_FINAL.png
```

## Prohibited Use

- `references/exploration/**`
- FIN01 Runtime implementation
- LGT01을 전체 Contest Room Global Grade로 복제
- PGT image의 exact room/object placement를 Canon으로 확정
- glass/breakage/debris implied by PGT03

---

# 5. AI·자동화 도구 사용 경계

현재 authority에서 생성형 AI가 만든 Concept, Base Mesh, Texture, UI,
VFX, Audio 또는 Marketing image를 새 Delivery에 포함하는 것은 금지한다.
별도의 Human Owner 직접 승인이 있을 때만 해당 exact tool·output에 한해
재검토할 수 있다.

현재 허용되는 자동화 보조는 다음 non-generative 검사뿐이다.

- topology·normal·UV와 texture seam 오류 분석
- scale·pivot·axis와 export validation
- naming, hash, budget와 delivery metadata 검증

실제 geometry, texture, UI, VFX와 audio는 Designer가 editable source에서
직접 제작하고 반드시 검사한다.

- object scale
- gameplay function
- anchor readability
- topology/UV/normal
- baked light/shadow
- unreadable AI text
- brand/trademark
- texture seams
- PBR consistency
- UI readability
- third-party IP

각 Delivery에 기록:

```text
AI Tool / Model:
AI Input References:
Prompt Summary:
Generated Elements:
Human Corrections:
Third-party IP Check:
```

생성형 AI를 사용하지 않았다면 위 기록은 `None / not authorized`로
명시한다. Human Owner의 별도 명시 승인 없이 AI 결과물을 생성하거나
납품하지 않는다.

---

# 6. 제작 순서

```text
B1 Chair Blockout/Anchor Approval
→ B2 Chair Production Delivery
→ B3 Binder Production Delivery
→ B4 Drawer/Copier Delivery
→ B5 Office Core Kit P0
→ B6 UI Wireframe
→ B7 UI Final Export
→ B8 VFX/Audio
→ B9 Thumbnail/Video
```

Decorative Kit은 Architecture P0 이후다.

---

# 7. B1 — Chair Blockout and Anchor Approval

## Objective

PGT02와 GEAR01 기준으로 실제 물리 Object와 Strap Anchor로 읽히는 Office Chair를 만든다.

## Required Output

```text
ArtSource/Props/PRP_OfficeChair/Source/PRP_OfficeChair_A_Blockout_v001.blend 또는 동등한 editable DCC source
ArtSource/Props/PRP_OfficeChair/Preview/PRP_OfficeChair_A_Blockout_v001.png
ArtSource/Props/PRP_OfficeChair/Preview/PRP_OfficeChair_A_AnchorDetail_v001.png
ArtSource/Props/PRP_OfficeChair/Preview/PRP_OfficeChair_A_SilhouetteGameplayView_v001.png
ArtSource/Props/PRP_OfficeChair/DELIVERY.md
```

`DELIVERY.md`는 `WORK IN PROGRESS`로 기록한다. 개별 binary는 `25 MiB`
미만으로 정리한다. 초과 파일은 Stage하지 않고 Git LFS 결정을 요청한다.
실제 editable source가 없으면 B1은 실패다.

## Review

- realistic office chair
- grounded wheel base
- metal load-bearing anchor visible
- caster/wheel/plastic not mistaken as anchor
- readable from third-person camera
- not racing chair
- not sci-fi

Human Owner가 Anchor와 Scale을 승인하기 전 Production Mesh/UV/Texture로
넘어가지 않는다. 이 Stop Boundary는 실제 Blockout 제작을 막지 않는다.

Commit:

```text
art: deliver office chair blockout and anchor v001
```

---

# 8. B2 — Chair Production Delivery

Required:

```text
PRP_OfficeChair_A_v001.blend or editable source
PRP_OfficeChair_A_v001.fbx
BaseColor
Normal
Mask
Turntable
Anchor Detail
DELIVERY.md
```

Target:

- <= 8k triangles
- <= 2 materials
- 1K, justified 2K
- Y-up, +Z, scale 1
- pivot at ground center of wheel base

Do not create gameplay collider or Rigidbody.

Commit:

```text
art: deliver office chair gameplay prop v001
```

---

# 9. B3 — Binder Production Delivery

Required:

```text
PRP_FileBinder_A_v001.blend
PRP_FileBinder_A_v001.fbx
1K PBR set
preview
DELIVERY.md
```

Target:

- <= 1.5k triangles
- 1 material
- 1K
- pivot object center
- readable while spinning
- no brand
- no AI gibberish text

Runtime will reuse one base asset for three instances.

Commit:

```text
art: deliver file binder gameplay prop v001
```

---

# 10. B4 — Drawer and Copier Delivery

## Drawer

Role:

- initially blocks Side Route
- direct acquisition prohibited
- moves from physical impact
- remains until Reset

Target:

- <= 6k triangles
- <= 2 materials
- 1K
- pivot ground center
- heavy enough visually, but movable on casters

## Copier

Role:

- Heavy recognition-only
- non-interactive
- static
- no tip/break/move feature

Target:

- <= 8k triangles
- <= 2 materials
- 1K
- no real brand

Required paths follow existing ArtSource folders.

Commit:

```text
art: deliver drawer and copier gameplay props v001
```

---

# 11. B5 — Office Core Kit P0

Create modular assets only. Do not edit Scene or define final layout.

Architecture / Fixed Fixture — Exactly 7:

```text
ENV_Wall_Straight_A_v001
ENV_DoorFrame_A_v001
ENV_LowPartition_Straight_A_v001
ENV_Floor_Module_A_v001
ENV_Ceiling_Module_A_v001
ENV_Column_A_v001
ENV_FluorescentLight_A_v001
```

Furniture / Fixed Non-Interactive — Exactly 1:

```text
ENV_FixedDesk_A_v001
```

Target per module:

- <= 4k triangles
- 1–2 materials
- shared 1K/2K material sets
- lower module origin
- modular dimensions documented

Must preserve:

- open-office readability
- real-world scale
- camera clearance
- gameplay object contrast

Do not create:

- breakable glass
- moving wall
- interactive monitor/keyboard
- department-specific unique geometry
- horror darkness

P1 decoration only after P0:

```text
workstation cluster
paper/archive cluster
plant/waste cluster
signage/tag/tape/wear decal
```

Commit:

```text
art: deliver office core architecture kit v001
```

---

# 12. B6/B7 — Contest UI

First create editable Wireframe, not polished final.

Required states:

```text
Title
Controls
Objective
Interact Prompt
Carry
Ally Availability
Strap
Route Open
Failure
Win
Retry
Pause
Mute
Credits
```

Design target:

- 1920x1080 base
- readable at 1366x768
- Gameplay center unobstructed
- no color-only state
- short action verbs
- ZERO DEFECT system UI separated from player HUD
- exact font license

B6 Commit:

```text
ui: deliver contest UI wireframe v001
```

After Human Owner review, export P0 PNG assets and editable source.

B7 Commit:

```text
ui: deliver contest UI exports v001
```

Do not bind UI to runtime code.

---

# 13. B8 — VFX and Audio

VFX allowed:

```text
interaction highlight
binder impact
chair wheel dust
strap attach/tension/release
drawer scrape
route open
exit activation
failure
win
```

VFX prohibited:

```text
magic aura
neon energy cable
explosion
glass break
debris
runtime fracture
screen-covering paper storm
```

Delivery must document:

- texture/flipbook
- color
- blend
- duration
- loop
- local/world
- intended event

Audio, if included:

- source and license required
- WAV 48kHz recommended
- mono/stereo, loop, trim, peak documented
- no unlicensed library

Commits:

```text
vfx: deliver contest physical feedback v001
audio: deliver licensed contest audio v001
```

---

# 14. B9 — Marketing

Thumbnail:

- 16:9
- actual contest gameplay identity
- Chair Redirect or Route Open as hero moment
- no approved concept image presented as runtime screenshot

Video:

- <= 3 minutes
- H.264 MP4
- actual gameplay first
- source project retained
- font/audio/image rights recorded

Commit:

```text
marketing: deliver contest thumbnail and video source v001
```

---

# 15. Delivery Completeness Check

```text
[ ] editable source
[ ] runtime export
[ ] textures/UI/VFX/audio exports
[ ] preview
[ ] DELIVERY.md
[ ] approved references recorded
[ ] generative AI status recorded as None / not authorized
[ ] ownership/license recorded
[ ] naming valid
[ ] scale/pivot/axis documented
[ ] triangle/material/texture budget documented
[ ] gameplay role preserved
[ ] known issues honest
[ ] only allowed paths changed
```

Source만 있거나 Export만 있으면 `DELIVERED`가 아니다.

---

# 16. PR Policy

One PR = one Delivery Batch.

PR Base:

`contest/game-builders-2026`

PR body:

- Asset IDs
- Approved references
- Source/export list
- Preview
- generative AI status (`None / not authorized`) and human corrections
- geometry/material/texture summary
- license
- known issues
- requested integration

Do not declare `OWNER APPROVED`.

Do not modify `.gitattributes` or LFS rules without Human Owner authorization.

---

# 17. Mandatory Report

```text
DESIGNER B CODEX REPORT

1. Current Branch
2. HEAD / origin Branch
3. Work Package
4. Approved References Read
5. Files Created/Changed
6. Generative AI Tools Used — must be `None / not authorized`
7. Human Corrections Required/Completed
8. Geometry/Texture/UI/VFX Summary
9. Gameplay Readability Check
10. Delivery Completeness
11. Validation Result
12. Commit
13. Push
14. PR
15. License Status
16. Known Issues
17. Human Owner Decision Required
18. Next Exact Work Package

ASSET STATUS
- Chair:
- Binder:
- Drawer:
- Copier:
- Office Kit:
- UI:
- VFX/Audio:
- Marketing:
```

모호하게 “환경 에셋 완료”라고 보고하지 않는다.

---

# 18. 지금 실행할 첫 작업

현재 첫 작업은 `B1 — Chair Blockout and Anchor Approval`이다.

다음만 수행한다.

1. Git 상태 검증
2. Approved PGT02/GEAR01/PGT01과 관련 Specs 읽기
3. 실제 editable `PRP_OfficeChair_A_Blockout_v001` 3D source 제작
4. `PRP_OfficeChair/DELIVERY.md`를 `WORK IN PROGRESS`로 갱신
5. Chair Blockout Preview
6. Anchor Detail Preview
7. Third-person Silhouette Preview
8. 정확한 파일만 Commit/Push
9. PR 생성
10. Mandatory Report

B1을 정확한 Commit·Push·PR로 제출한 뒤에만 별도의 Binder batch를 시작할
수 있다. B1에 Binder 파일을 섞지 않으며, Binder도 editable source 없이
Preview만 제출하지 않는다.

Human Owner가 Chair Anchor/Scale을 승인하기 전 B2로 넘어가지 않는다.

`DELIVERY.md` 또는 계획 보고만 반환하지 않는다. 실제 editable 3D source와
세 Preview가 없으면 B1을 완료·전달·검토 준비 상태로 선언하지 않는다.

Begin with B1 now.
