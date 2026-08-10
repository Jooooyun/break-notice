# CODEX WORK ORDER — DESIGNER A

> 담당 영역: Character / Rig / Animation / GEAR01  
> 실행 브랜치: `art/characters-animation`  
> PR Base: `contest/game-builders-2026`  
> Human Owner: 조윤성

---

# 0A. 2026-08-10 DELIVERY AUDIT — REVISION REQUIRED

Remote commit `6094b41143286b6e1710087a711707416b366c20`까지의
`0810_designerA/**` batch는 파일 존재만 확인되었으며, 승인된 production
delivery로 판정되지 않았다. 현재 상태는
`REVISION REQUIRED / REVIEW HOLD / DO NOT INTEGRATE`다.

확인된 batch:

```text
159 files
155 FBX + 4 usage guides
131,363,044 path bytes
main_character / female_helper / enemy_normal / enemy_elite
ordinary Git blobs; no LFS filter
PR: absent
```

다음 결함 때문에 이 batch를 `DELIVERED`, `TECHNICAL REVIEW`,
`OWNER APPROVED` 또는 Runtime-ready로 보고하면 안 된다.

1. 활성 package는 A1 `CHR01 editable blockout and pose review` 하나뿐인데
   CHR00, ENY01, elite variant와 대량 animation까지 선행 제작됐다.
2. 모든 새 파일이 허용 경로 `ArtSource/Characters/**` 밖의
   `0810_designerA/**`에 있다.
3. A1 필수 editable DCC source와 세 Preview가 없다.
4. CHR00/CHR01/ENY01 `DELIVERY.md`와 contributor/license ledger가
   갱신되지 않았다.
5. creator, original model source, exact license, written permission,
   AI/generated-element 여부와 third-party IP check가 없다. Guide에 적힌
   Mixamo mocap만으로는 모델·텍스처·리그 전체의 사용 권리를 증명하지
   못한다.
6. ENY01은 Concept 승인 전 최종 모델로 진행됐고 `enemy_elite`, rifle,
   grenade, military/police silhouette를 포함한다. 이는 one shared
   Compliance Runner, no firearm, no military armor, no elite/boss 계약과
   충돌한다.
7. CHR00에는 weapon, sword, death, jump, cover 등 현재 minimum 밖 또는
   금지된 clip이 포함되고, 승인 Canon 대조 Preview가 없다.
8. 155개 FBX가 naming/version 규칙과 required clip set으로 정리되지
   않았으며 총 131 MB를 일반 Git에 추가했다. 기존 history를 LFS로
   재작성하지 않는다.

즉시 수행할 교정 작업은 다음 하나뿐이다.

```text
A1 — CHR01 editable 3D blockout and pose review batch
```

필수 경로:

```text
ArtSource/Characters/CHR01_Ally/Source/CHR01_Ally_Blockout_v001.blend
ArtSource/Characters/CHR01_Ally/Preview/CHR01_Ally_Blockout_v001.png
ArtSource/Characters/CHR01_Ally/Preview/CHR01_Supply_Binder_Pose_v001.png
ArtSource/Characters/CHR01_Ally/Preview/CHR01_Push_Chair_Pose_v001.png
ArtSource/Characters/CHR01_Ally/DELIVERY.md
```

`DELIVERY.md`에는 creator, GitHub ID, exact source/license/permission,
AI tool 여부, approved references, source/preview 목록, scale/pivot/axis,
known issues와 `WORK IN PROGRESS`를 정확히 기록한다. 생성형 AI, purchased
base, Mixamo 또는 다른 third-party tool/asset을 사용했다면 정확한 항목과
license evidence를 누락 없이 기록하고, 권리가 불명확하면 즉시 멈춘다.

기존 `0810_designerA/**` binary와 commit history는 Human Owner의 별도
disposition 전까지 삭제·이동·덮어쓰기·history rewrite하지 않는다. 새 대형
binary도 Git LFS 결정을 받기 전 추가하지 않는다. 이 batch로 PR을 만들거나
Runtime에 import하지 않는다. ENY01/CHR00/elite/weapon animation 작업을 더
진행하지 않는다.

A1 필수 다섯 파일만 정확한 허용 경로에 추가하고 validation 결과를 보고한
뒤 Human Owner의 Blockout/Pose 판정을 기다린다.

---

# 0. 최상위 명령

## Human Owner Direct Production Start

- Authority baseline: `fc361a4dc42492e9b997ee42ac8bab3a6fdec2d2`
- Authorized branch: `art/characters-animation`
- Active package: `A1 — CHR01 editable 3D blockout and pose review batch`
- Start: `IMMEDIATE / DO NOT WAIT FOR C0 OR C1`

계획·조사·`DELIVERY.md` 수정만 하고 종료하지 않는다. A1의 완료 증거에는
실제로 다시 열고 편집할 수 있는 3D Blockout source와 그 source에서 만든
세 Preview가 반드시 함께 있어야 한다. Human Owner review는 Final
Sculpt/Retopology/Rig 진입 Gate이며 Blockout 제작 시작 Gate가 아니다.

이 저장소에서 네 역할은 코드를 구현하는 것이 아니다.

네 역할은 Designer A를 보조해 다음 Runtime-ready Source Art Delivery를 만드는 것이다.

1. `CHR01_Ally`
2. `GEAR01_CargoStrap`
3. `ENY01_ComplianceRunner`
4. `CHR00_Player`

항상 다음 우선순위를 따른다.

```text
Approved reference fidelity
→ readable gameplay silhouette
→ correct scale/rig/export
→ WebGL-suitable budget
→ delivery completeness
→ extra polish
```

기능 수와 디테일을 늘리지 않는다.

---

# 1. 시작 전 Git 검증

다음을 실행하거나 동등하게 검증한다.

```text
git fetch --all --tags
git switch art/characters-animation
git pull --ff-only origin art/characters-animation
git status --short --branch
git rev-parse HEAD
git rev-parse origin/art/characters-animation
```

작업 전 정상 조건:

- branch = `art/characters-animation`
- HEAD = `origin/art/characters-animation`
- tracked/staged change = 0
- unknown untracked work = 0 또는 모두 사용자 소유로 식별됨
- `DESIGNER_A_HUMAN_GUIDE.md` 존재
- `ART_DELIVERY_STANDARD.md` 존재

다음이면 Hard Stop한다.

- 다른 branch
- 모르는 작업 존재
- merge conflict
- 승인 reference 누락
- 대형 Binary를 Git에 넣기 위한 정책 불명확
- Source Art를 만들기 위해 code/Scene/Package 수정이 필요함
- 제3자 권리 또는 라이선스 불명확

금지:

- `git clean`
- `git reset --hard`
- `git add .`
- `git add -A`
- force push
- pushed commit amend

Stage는 정확한 자산 경로만 지정한다.

---

# 2. 읽어야 할 권위

작업 전 다음을 읽는다.

```text
docs/10_contest/handoffs/DESIGNER_A_HUMAN_GUIDE.md
docs/10_contest/ART_DELIVERY_STANDARD.md
docs/10_contest/ASSET_NEEDS_STATUS.md
docs/10_contest/VISUAL_REFERENCE_INVENTORY.md
docs/03_art/ART_DIRECTION.md
docs/03_art/PLAYER_VISUAL_SPEC.md
docs/03_art/PLAYER_TURNAROUND_PRODUCTION_BRIEF.md
docs/03_art/ALLY_VISUAL_SPEC.md
docs/03_art/CARGO_STRAP_SPEC.md
docs/03_art/CHAIR_REDIRECT_VISUAL_SPEC.md
docs/02_design/PROTOTYPE_CONTENT_LOCK.md
```

Authority order:

1. Human Owner의 최신 직접 지시
2. Approved Visual Spec / Accepted ADR
3. `references/approved/` FINAL 이미지
4. Contest delivery 문서
5. AI 제안

AI 제안이 승인 레퍼런스와 충돌하면 승인 레퍼런스를 따른다.

---

# 3. 허용 경로

직접 생성·수정 가능:

```text
ArtSource/Characters/CHR00_Player/**
ArtSource/Characters/CHR01_Ally/**
ArtSource/Characters/ENY01_ComplianceRunner/**
ArtSource/Equipment/GEAR01_CargoStrap/**
```

관련 `DELIVERY.md`와 Preview도 포함한다.

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

Runtime Import, `.meta`, Material, Collider, Prefab, Animator Controller, Scene Integration은 통합 브랜치의 Human Owner/Codex가 담당한다.

---

# 4. 승인 레퍼런스 패키지

## CHR00

```text
references/approved/characters/CHR_00_Player_Identity_Lock_v002_FINAL.png
references/approved/characters/CHR_00_Player_Turnaround_v001_FINAL.png
```

Identity Lock이 주 권위다.

## CHR01

```text
references/approved/characters/CHR_01_First_Ally_Identity_v001_FINAL.png
```

## GEAR01

```text
references/approved/equipment/GEAR_01_Cargo_Strapping_System_v002_FINAL.png
references/approved/gameplay/PGT_02_Chair_Redirect_Core_Loop_v010_FINAL.png
```

## 공통 플레이 가독성

```text
references/approved/gameplay/PGT_01_Gameplay_Camera_v002_FINAL.png
references/approved/gameplay/PGT_03_Chain_Reaction_Aftermath_v003_FINAL.png
```

## 사용 금지

- `references/exploration/**`
- `FIN_01_Conference_Table_Team_Finisher_v003_FINAL.png`의 Runtime 기능
- 이미지 속 incidental room/enemy/pose를 새 Canon으로 확정

ENY01 전용 승인 Identity가 없으므로 최종 모델링 전에 Concept Options와 Human Owner 승인이 필요하다.

---

# 5. AI·자동화 도구 사용 경계

현재 authority에서 생성형 AI가 만든 Concept, Base Mesh, Texture, Rig 또는
Animation을 새 Delivery에 포함하는 것은 금지한다. 별도의 Human Owner
직접 승인이 있을 때만 해당 exact tool·output에 한해 재검토할 수 있다.

현재 허용되는 자동화 보조는 다음 non-generative 검사뿐이다.

- topology·normal·UV·weight 오류 분석
- scale/pivot/axis와 export validation
- naming, hash와 delivery metadata 검증

실제 geometry, texture, rig와 animation은 Designer가 editable DCC
source에서 직접 제작한다.

반드시 검사:

- 승인 레퍼런스 일치
- 성인 비율
- 손가락·관절
- 의상 구조
- Mesh topology
- UV overlap
- flipped normal
- Scale/Pivot/Axis
- Humanoid mapping
- skin weight
- clipping
- root motion 여부
- 제3자 IP·브랜드

각 Delivery에 추가 기록:

```text
AI Tool / Model:
AI Input References:
Prompt Summary:
Generated Elements:
Human Corrections:
Third-party IP Check:
```

정확한 도구·모델을 모르면 추측하지 않는다.

---

# 6. 제작 순서

아래 순서를 변경하지 않는다.

```text
A1 CHR01 Blockout and Pose Approval
→ A2 CHR01 Model/Rig/Clips Delivery
→ A3 GEAR01 Hardware and State Delivery
→ A4 ENY01 Concept Approval
→ A5 ENY01 Model/Rig/Clips Delivery
→ A6 CHR00 Model/Rig/Clips Delivery
```

게임 기능이 아직 준비되지 않아도 Source Art 제작은 계속할 수 있다.

---

# 7. A1 — CHR01 Blockout and Pose Approval

## Objective

승인된 CHR01 Identity를 실제 제작 가능한 3D Blockout과 두 핵심 동작으로 변환한다.

## Required Output

```text
ArtSource/Characters/CHR01_Ally/Source/CHR01_Ally_Blockout_v001.blend 또는 동등한 editable DCC source
ArtSource/Characters/CHR01_Ally/Preview/CHR01_Ally_Blockout_v001.png
ArtSource/Characters/CHR01_Ally/Preview/CHR01_Supply_Binder_Pose_v001.png
ArtSource/Characters/CHR01_Ally/Preview/CHR01_Push_Chair_Pose_v001.png
ArtSource/Characters/CHR01_Ally/DELIVERY.md
```

`DELIVERY.md`에는 아직 없는 Export를 없는 것으로 정확히 기록한다.
개별 binary는 `25 MiB` 미만으로 정리한다. 초과 파일은 Stage하지 않고
Git LFS 결정을 요청한다. 실제 editable source가 없으면 A1은 실패다.

## Review Points

- CHR00과 구분되는 실루엣
- 평범하고 현실적인 시설·물류 직원 체형
- 물체 공급 동작이 마법처럼 보이지 않음
- Binder 공급과 Chair Feed 포즈가 다름
- 손·물체 접점이 제작 가능함

## Stop Boundary

Human Owner가 Blockout과 Pose를 승인하기 전 Final Sculpt/Retopology/Rig에
들어가지 않는다. 이 Stop Boundary는 실제 Blockout 제작을 막지 않는다.

## Commit

```text
art: deliver CHR01 blockout and supply poses v001
```

---

# 8. A2 — CHR01 Model/Rig/Animation Delivery

필수 파일은 Human Guide를 따른다.

Target:

- <= 30k triangles
- 2~3 material slots
- max 2K texture
- Humanoid-compatible
- in-place clips

필수 Clip:

```text
CHR01_Idle
CHR01_Call_Response
CHR01_Supply_Binder
CHR01_Push_Chair
CHR01_Reset
```

Validation:

- T-pose 또는 A-pose 기준 명확
- forward +Z
- Y-up
- scale 1,1,1
- pivot feet-ground center
- no root translation in locomotion/supply clips unless explicitly documented
- no missing texture
- no hidden external dependency

Commit:

```text
art: deliver CHR01 ally model rig and supply clips v001
```

---

# 9. A3 — GEAR01 Cargo Strap Delivery

필수:

```text
GEAR01_WaistReel_v001.fbx
GEAR01_Carabiner_v001.fbx
GEAR01_Clamp_v001.fbx
GEAR01_ChairAnchorGuide_v001.fbx 또는 명확한 가이드
PBR texture set
GEAR01_States_v001.png
DELIVERY.md
```

Target:

- combined <= 12k triangles
- 1~2 materials
- max 2K

Must preserve:

```text
waist reel
→ black-yellow strap
→ carabiner
→ clamp
→ load-bearing chair frame
```

Reject:

- necktie connection
- caster/wheel/plastic anchor
- energy cable
- grapple silhouette
- full rope simulation asset

Commit:

```text
art: deliver GEAR01 cargo strap hardware v001
```

---

# 10. A4/A5 — ENY01 Concept then Production

ENY01 has no dedicated approved identity image.

First output:

```text
ArtSource/Characters/ENY01_ComplianceRunner/Preview/ENY01_Concept_Options_v001.png
```

Create 2–3 restrained options only.

Required common properties:

- ordinary corporate compliance/security pressure
- grounded adult proportions
- no firearm
- no shield
- no military armor
- no elite/boss silhouette
- one model reusable for two instances

Do not use exact enemy appearance in PGT images as authority.

After Human Owner selection, create:

```text
ENY01_ComplianceRunner_v001.fbx
PBR set
Humanoid rig
Idle
Move
Pressure
HitReaction
Stagger
Reset
Preview
DELIVERY.md
```

Target:

- <= 20k triangles
- 1~2 materials
- max 2K

Commits:

```text
art: deliver ENY01 concept options v001
art: deliver ENY01 runner model rig and clips v001
```

---

# 11. A6 — CHR00 Player Delivery

References:

```text
CHR_00_Player_Identity_Lock_v002_FINAL.png
CHR_00_Player_Turnaround_v001_FINAL.png
```

Identity Lock dominates Turnaround.

Target:

- <= 35k triangles
- 2~3 materials
- max 2K
- Humanoid-compatible
- in-place

Required clips:

```text
Idle
Walk_F
Walk_B
Strafe_L
Strafe_R
Aim_Idle
Interact
Carry_Light
Carry_Medium
Throw_Light
Throw_Medium
Catch
Strap_Attach
Strap_Tension
Strap_Release
Pressure_Reaction
Failure
```

P0 exclusions:

- facial animation
- lip sync
- cloth simulation
- hair simulation
- cinematic clip

Commit:

```text
art: deliver CHR00 player model rig and clips v001
```

---

# 12. Delivery Completeness Check

각 Asset 작업이 끝날 때 아래를 검증한다.

```text
[ ] source exists
[ ] export exists
[ ] textures exist
[ ] required animations exist
[ ] preview exists
[ ] DELIVERY.md complete
[ ] AI usage recorded
[ ] ownership/license recorded
[ ] naming valid
[ ] scale/pivot/axis documented
[ ] triangle/material/texture budget documented
[ ] known issues honest
[ ] only allowed paths changed
```

Source만 있거나 Export만 있으면 `DELIVERED`가 아니다.

---

# 13. PR Policy

한 PR은 한 Delivery Batch다.

PR Base:

`contest/game-builders-2026`

PR 내용:

- Asset ID
- Approved references used
- Source/export list
- Preview
- AI tool and human correction
- budget summary
- known issues
- requested integration

Designer/Codex는 `OWNER APPROVED` 상태를 선언하지 않는다.

허용 상태:

```text
WORK IN PROGRESS
DELIVERED
TECHNICAL REVIEW
REVISION REQUIRED
```

Human Owner만 `OWNER APPROVED`를 선언한다.

---

# 14. Mandatory Report

모든 작업 종료 보고는 아래 형식이다.

```text
DESIGNER A CODEX REPORT

1. Current Branch
2. HEAD / origin Branch
3. Work Package
4. Approved References Read
5. Files Created/Changed
6. AI Tools Used
7. Human Corrections Required/Completed
8. Geometry Summary
9. Rig/Animation Summary
10. Delivery Completeness
11. Validation Result
12. Commit
13. Push
14. PR
15. Known Issues
16. Human Owner Decision Required
17. Next Exact Work Package

ASSET STATUS
- CHR01:
- GEAR01:
- ENY01:
- CHR00:
```

모호하게 “캐릭터 작업 완료”라고 보고하지 않는다.

---

# 15. 지금 실행할 첫 작업

현재 첫 작업은 `A1 — CHR01 Blockout and Pose Approval`이다.

다음만 수행한다.

1. Git 상태 검증
2. 권위 문서와 승인 레퍼런스 읽기
3. 실제 editable `CHR01_Ally_Blockout_v001` 3D source 제작
4. `CHR01_Ally/DELIVERY.md`를 `WORK IN PROGRESS`로 갱신
5. Blockout Preview
6. Binder Supply Pose Preview
7. Chair Push Pose Preview
8. 정확한 파일만 Commit/Push
9. PR 생성
10. Mandatory Report

Human Owner 승인 전 A2로 넘어가지 않는다.

`DELIVERY.md` 또는 계획 보고만 반환하지 않는다. 실제 editable 3D source와
세 Preview가 없으면 A1을 완료·전달·검토 준비 상태로 선언하지 않는다.

Begin with A1 now.
