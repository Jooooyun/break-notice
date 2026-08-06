# 디자이너 A 작업지시서

> 담당: **캐릭터 · 리깅 · 애니메이션 · GEAR01 Cargo Strap**  
> 작업 브랜치: `art/characters-animation`  
> 병합 대상: `contest/game-builders-2026`  
> 최종 승인자: 조윤성(Human Owner)

---

# 1. 네 역할 한 줄

BREAK NOTICE 공모전판에 필요한 **플레이어 1명, 고정 동료 1명, 동일 적 1종, Cargo Strap 장비**를 승인된 레퍼런스에 맞춰 실제 Unity용 자산으로 제작한다.

중요한 원칙은 하나다.

> 새 캐릭터 방향을 만드는 일이 아니라, 이미 승인된 방향을 게임에서 돌아갈 수 있는 모델·리그·애니메이션으로 바꾸는 일이다.

---

# 2. 가장 먼저 해야 할 Git 작업

작업을 시작하기 전에 아래 순서를 지킨다.

```text
git fetch --all --tags
git switch art/characters-animation
git pull --ff-only origin art/characters-animation
git status --short --branch
```

정상 조건:

- 현재 브랜치가 `art/characters-animation`
- tracked/staged 변경 없음
- 이 문서가 브랜치에 존재함
- `docs/10_contest/ART_DELIVERY_STANDARD.md`가 존재함
- `ArtSource/Characters/**`, `ArtSource/Equipment/**` 폴더가 존재함

다음 상황이면 작업을 시작하지 말고 조윤성에게 알린다.

- 다른 브랜치에 있음
- 모르는 로컬 변경이 있음
- Pull 과정에서 충돌 발생
- 승인 레퍼런스가 보이지 않음
- 대형 파일을 Git에 어떻게 넣을지 불명확함

`main`과 `contest/game-builders-2026`에는 직접 Push하지 않는다.

---

# 3. 수정 가능한 폴더

## 직접 작업 가능

```text
ArtSource/Characters/CHR00_Player/**
ArtSource/Characters/CHR01_Ally/**
ArtSource/Characters/ENY01_ComplianceRunner/**
ArtSource/Equipment/GEAR01_CargoStrap/**
```

각 자산 폴더의 `DELIVERY.md`도 직접 작성한다.

## 수정 금지

```text
Game/Assets/_Project/Code/**
Game/Assets/_Project/Scenes/**
Game/Assets/_Project/Prefabs/**
Game/Packages/**
Game/ProjectSettings/**
tools/**
```

Unity Runtime Import, Material, Prefab, Collider, Animator 연결은 조윤성과 통합 Codex가 담당한다.

---

# 4. 반드시 사용할 승인 레퍼런스

`references/exploration/`의 REVIEW 이미지를 기준으로 작업하지 않는다. 아래 `references/approved/` 파일만 쓴다.

## CHR00 플레이어

### 주 Identity

`references/approved/characters/CHR_00_Player_Identity_Lock_v002_FINAL.png`

가져갈 것:

- 현실적인 성인 남성 비율
- 회사원과 시설·자산관리 실무자의 중간 실루엣
- 구겨진 셔츠와 회사 복장
- 암적색 넥타이
- 작업 장비와 회사 복장의 대비
- 슈퍼히어로가 아닌 평범한 현장 근무자 체형

### 제작용 5면도

`references/approved/characters/CHR_00_Player_Turnaround_v001_FINAL.png`

가져갈 것:

- 전면·측면·후면 비율
- 후면 장비 부착 가독성
- 의상 레이어와 실루엣 연속성

우선순위는 **Identity Lock > Turnaround**다. 둘이 다르게 보이면 Identity Lock을 따른다.

## CHR01 고정 동료

`references/approved/characters/CHR_01_First_Ally_Identity_v001_FINAL.png`

가져갈 것:

- 아웃소싱 이사·자산이동 현장 책임자 느낌
- 현실적인 작업복
- 플레이어와 다른 실루엣
- 사무용 물체를 실제로 공급하는 사람이라는 인상

정확한 얼굴 세부는 최종 Canon이 아니다. 전체 직군·복장·실루엣을 우선한다.

## GEAR01 Cargo Strap

`references/approved/equipment/GEAR_01_Cargo_Strapping_System_v002_FINAL.png`

반드시 포함:

- 허리 장착형 수축 릴
- 검정·황색 직조 산업용 스트랩
- 은색 카라비너
- 교체식 금속 클램프
- 의자 좌석 아래의 하중 지지 금속 프레임 연결

절대 금지:

- 넥타이와 스트랩 연결
- 목에서 줄이 나옴
- 캐스터·휠 축·플라스틱 하우징 연결
- 그래플링 장비처럼 보임
- 에너지 케이블
- 마법·염력 표현

## 동작과 포즈 보조 레퍼런스

`references/approved/gameplay/PGT_02_Chair_Redirect_Core_Loop_v010_FINAL.png`

가져갈 것:

- CHR01이 의자를 실제 바닥으로 공급하는 관계
- 플레이어가 스트랩으로 의자의 방향을 바꾸는 몸의 기울기
- 바닥 지지와 장력을 읽을 수 있는 포즈

정확한 방, 자세, 힘, 타이밍은 그대로 복제하지 않는다.

## 보조 가독성 레퍼런스

- `references/approved/gameplay/PGT_01_Gameplay_Camera_v002_FINAL.png`
- `references/approved/gameplay/PGT_03_Chain_Reaction_Aftermath_v003_FINAL.png`

용도:

- 실제 플레이 거리에서 실루엣이 읽히는지 확인
- 지나치게 작은 장비 디테일을 만들지 않도록 확인
- 충돌 반응이 물리적으로 읽히도록 애니메이션 강도 참고

## 사용 금지

`references/approved/gameplay/FIN_01_Conference_Table_Team_Finisher_v003_FINAL.png`

이 이미지는 공모전 Runtime 범위가 아니다. 두 번째 동료, 피니셔, 회의용 테이블 합동 액션을 만들지 않는다.

---

# 5. 반드시 읽을 문서

작업 전 최소한 아래 문서를 읽는다.

1. `docs/03_art/ART_DIRECTION.md`
2. `docs/03_art/PLAYER_VISUAL_SPEC.md`
3. `docs/03_art/PLAYER_TURNAROUND_PRODUCTION_BRIEF.md`
4. `docs/03_art/ALLY_VISUAL_SPEC.md`
5. `docs/03_art/CARGO_STRAP_SPEC.md`
6. `docs/03_art/CHAIR_REDIRECT_VISUAL_SPEC.md`
7. `docs/02_design/PROTOTYPE_CONTENT_LOCK.md`
8. `docs/10_contest/ART_DELIVERY_STANDARD.md`
9. `docs/10_contest/ASSET_NEEDS_STATUS.md`
10. `docs/10_contest/VISUAL_REFERENCE_INVENTORY.md`

핵심 Art Direction:

> **Grounded Corporate Action Realism**  
> **Grounded world. Absurd physics.**

캐릭터 자체를 만화처럼 과장하지 않는다. 과장은 충돌 반응과 동작의 결과에서 나온다.

---

# 6. 작업 우선순위와 내부 마감

게임 개발 순서에 맞춰 아래 순서로 제작한다.

| 순서 | 자산 | 1차 검토 마감 | 왜 먼저 필요한가 |
|---:|---|---|---|
| 1 | `CHR01_Ally` | 2026-08-10 23:59 KST | 동료 공급·Catch 구현과 연결 |
| 2 | `GEAR01_CargoStrap` | 2026-08-12 23:59 KST | Cargo Strap Redirect 구현과 연결 |
| 3 | `ENY01_ComplianceRunner` | 2026-08-16 23:59 KST | 적 압박·실패 흐름과 연결 |
| 4 | `CHR00_Player` | 2026-08-18 23:59 KST | 최종 캐릭터 통합과 영상 촬영 |

마감은 **최종 완벽본**이 아니라 조윤성이 실제로 검토할 수 있는 `v001` 납품 시각이다.

---

# 7. 자산별 정확한 납품물

## 7.1 CHR01 Ally — 첫 작업

경로:

`ArtSource/Characters/CHR01_Ally/`

필수 파일:

```text
Source/CHR01_Ally_v001.blend 또는 편집 가능한 원본
Export/CHR01_Ally_v001.fbx
Textures/CHR01_Ally_BaseColor_v001.png
Textures/CHR01_Ally_Normal_v001.png
Textures/CHR01_Ally_Mask_v001.png
Animations/CHR01_Idle_v001.fbx
Animations/CHR01_Call_Response_v001.fbx
Animations/CHR01_Supply_Binder_v001.fbx
Animations/CHR01_Push_Chair_v001.fbx
Animations/CHR01_Reset_v001.fbx
Preview/CHR01_Ally_Turntable_v001.mp4 또는 이미지 세트
DELIVERY.md
```

Acceptance:

- Humanoid-compatible Rig
- In-place Animation
- 플레이어와 구분되는 실루엣
- 바인더를 던지는 동작과 의자를 미는 동작이 명확히 다름
- 손과 물체의 접점이 검수 가능함

## 7.2 GEAR01 Cargo Strap

경로:

`ArtSource/Equipment/GEAR01_CargoStrap/`

필수 파일:

```text
Source/GEAR01_CargoStrap_v001.blend
Export/GEAR01_WaistReel_v001.fbx
Export/GEAR01_Carabiner_v001.fbx
Export/GEAR01_Clamp_v001.fbx
Export/GEAR01_ChairAnchorGuide_v001.fbx 또는 명확한 부착 위치 가이드
Textures/GEAR01_BaseColor_v001.png
Textures/GEAR01_Normal_v001.png
Textures/GEAR01_Mask_v001.png
Preview/GEAR01_States_v001.png
DELIVERY.md
```

스트랩의 긴 줄 자체는 초기 Runtime에서 Line Renderer를 쓸 수 있다. 따라서 릴·카라비너·클램프·연결 구조의 가독성이 우선이다.

## 7.3 ENY01 Compliance Runner

전용 승인 Identity 이미지가 현재 없다. 승인 Gameplay 이미지 속 적의 정확한 외형은 Canon이 아니다.

따라서 바로 최종 모델을 만들지 않는다.

첫 납품:

```text
Preview/ENY01_Concept_Options_v001.png
```

Concept 조건:

- 평범한 회사 보안·컴플라이언스 인력으로 읽힘
- 총기·방패·군사 장비 없음
- 보스나 특수부대처럼 보이지 않음
- CHR00·CHR01과 실루엣이 구분됨
- 동일 모델 2개를 써도 화면에서 문제없음
- 색상 Variation은 Material 수준에서만 가능

조윤성 승인 후 모델링한다.

최종 필수:

```text
Source/ENY01_ComplianceRunner_v001.blend
Export/ENY01_ComplianceRunner_v001.fbx
Textures/...
Animations/ENY01_Idle_v001.fbx
Animations/ENY01_Move_v001.fbx
Animations/ENY01_Pressure_v001.fbx
Animations/ENY01_HitReaction_v001.fbx
Animations/ENY01_Stagger_v001.fbx
Animations/ENY01_Reset_v001.fbx
Preview/...
DELIVERY.md
```

## 7.4 CHR00 Player

경로:

`ArtSource/Characters/CHR00_Player/`

필수 애니메이션:

```text
CHR00_Idle
CHR00_Walk_F
CHR00_Walk_B
CHR00_Strafe_L
CHR00_Strafe_R
CHR00_Aim_Idle
CHR00_Interact
CHR00_Carry_Light
CHR00_Carry_Medium
CHR00_Throw_Light
CHR00_Throw_Medium
CHR00_Catch
CHR00_Strap_Attach
CHR00_Strap_Tension
CHR00_Strap_Release
CHR00_Pressure_Reaction
CHR00_Failure
```

P0에서는 표정·Lip Sync·Cloth Simulation이 필요 없다.

---

# 8. WebGL용 제작 목표

아래는 품질을 낮추기 위한 수치가 아니라, 브라우저에서 안정적으로 돌리기 위한 Contest 목표다.

| 자산 | Triangle 목표 | Material Slot 목표 | Texture |
|---|---:|---:|---|
| CHR00 | 35k 이하 | 2~3 | 최대 2K |
| CHR01 | 30k 이하 | 2~3 | 최대 2K |
| ENY01 | 20k 이하 | 1~2 | 최대 2K |
| GEAR01 전체 | 12k 이하 | 1~2 | 최대 2K |

목표를 넘을 경우 `DELIVERY.md`에 이유를 적고 조윤성 검토를 받는다.

권장:

- 세 캐릭터가 가능한 한 일관된 Humanoid Skeleton 규칙 사용
- 불필요한 얼굴 Bone 제거
- P0에서 고급 Facial Rig 금지
- Baked Cloth·Hair Simulation 금지
- 작은 볼트보다 실루엣·Roughness·Normal 우선

---

# 9. AI 사용 규칙

AI를 사용해도 된다. 단, AI 결과물을 그대로 최종본으로 취급하지 않는다.

허용 예:

- 레퍼런스 기반 Concept 보조
- Base Mesh 생성
- Retopology 보조
- Texture 초안
- Rig·Animation 보조
- 작업 오류 분석

반드시 사람이 확인:

- 레퍼런스 일치
- Topology
- UV
- Normal
- Scale
- Pivot
- Rig Weight
- Hand/Object 접점
- 깨진 손가락·관절
- 의상·장비 구조
- 저작권·제3자 상표

각 `DELIVERY.md`에 아래 항목을 추가한다.

```text
AI Tool / Model:
AI Input References:
Prompt Summary:
Generated Elements:
Human Corrections:
Third-party IP Check:
```

실존 브랜드 로고, 유명 캐릭터, 타 게임 디자인을 포함하지 않는다.

---

# 10. DELIVERY.md 완료 조건

납품은 파일만 올리는 것으로 끝나지 않는다.

필수 기록:

- Asset ID
- Creator
- Contribution Date
- Source Files
- Export Files
- Texture Files
- Animation Files
- Work Classification
- Exact License
- Reference Used
- Polygon Count
- Material Slots
- Texture Resolution
- Scale
- Pivot
- Rig Type
- Loop Clips
- Known Issues
- Preview Files
- AI 사용 기록

원본만 있고 FBX가 없으면 미완료다. FBX만 있고 원본·권리 정보가 없어도 미완료다.

---

# 11. Commit과 PR 규칙

한 PR에는 한 납품 단위만 넣는다.

권장 Commit:

```text
art: deliver CHR01 ally model and supply clips v001
art: deliver GEAR01 cargo strap hardware v001
art: deliver ENY01 concept options v001
art: deliver ENY01 runner model and clips v001
art: deliver CHR00 player model and clips v001
```

PR Base:

`contest/game-builders-2026`

PR 본문에 포함:

- 무엇을 만들었는지
- 사용한 승인 레퍼런스
- 파일 목록
- AI 사용 여부
- Polygon/Material/Texture 요약
- Known Issues
- 조윤성이 확인해야 할 포인트

대형 Binary를 Commit하기 전에 조윤성에게 Git LFS 적용 여부를 확인한다. `.gitattributes`를 혼자 수정하지 않는다.

---

# 12. 오늘 바로 시작할 작업

## Step 1

승인 레퍼런스와 필수 문서를 읽는다.

## Step 2

`CHR01_Ally`의 제작 계획을 `ArtSource/Characters/CHR01_Ally/DELIVERY.md`에 작성한다.

## Step 3

다음 Preview를 먼저 만든다.

```text
Preview/CHR01_Ally_Blockout_v001.png
Preview/CHR01_Supply_Binder_Pose_v001.png
Preview/CHR01_Push_Chair_Pose_v001.png
```

## Step 4

조윤성에게 Blockout·Pose 승인을 받은 후 Model/Rig/Animation으로 들어간다.

처음부터 완성형 모델을 길게 만들지 않는다. **Blockout → 승인 → Production** 순서로 간다.

---

# 13. 작업 완료 보고 양식

```text
DESIGNER A DELIVERY REPORT

Asset ID:
Branch:
Commit:
PR:

Approved References Used:
Source Files:
Export Files:
Textures:
Animations:
Preview:

Triangle Count:
Material Slots:
Texture Resolution:
Rig Type:
In-place Confirmed:

AI Tool / Model:
Human Corrections:
License / Ownership:

Known Issues:
Owner Review Needed:
Next Asset:
```

---

# 14. 절대 기억할 것

- 승인 레퍼런스는 방향이다. REVIEW 폴더의 구버전은 사용하지 않는다.
- 게임 기능을 만들지 않는다.
- Gameplay Scene을 수정하지 않는다.
- 모델이 늦어져도 프로그래밍은 Placeholder로 계속 진행된다.
- 불필요한 완벽주의보다 정해진 날짜의 검수 가능한 `v001`이 우선이다.
- 최종 승인 권한은 조윤성에게 있다.
