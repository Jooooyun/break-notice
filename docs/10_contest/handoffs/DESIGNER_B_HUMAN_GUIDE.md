# 디자이너 B 작업지시서

> 담당: **환경 · Gameplay Props · Material · UI · VFX · Marketing**  
> 작업 브랜치: `art/environment-ui-vfx`  
> 병합 대상: `contest/game-builders-2026`  
> 최종 승인자: 조윤성(Human Owner)

---

# 1. 네 역할 한 줄

BREAK NOTICE 공모전판의 한 방짜리 사무실에서 플레이어가 **무엇을 집고, 무엇을 밀고, 어디로 나가야 하는지 즉시 이해하도록** 환경·프롭·UI·이펙트를 만든다.

가장 중요한 기준:

> 예쁜 사무실보다 읽히는 사무실이 먼저다. 장식보다 Gameplay Object와 Route가 먼저다.

---

# 2. 가장 먼저 해야 할 Git 작업

```text
git fetch --all --tags
git switch art/environment-ui-vfx
git pull --ff-only origin art/environment-ui-vfx
git status --short --branch
```

정상 조건:

- 현재 브랜치가 `art/environment-ui-vfx`
- tracked/staged 변경 없음
- 이 문서가 존재함
- `ArtSource/Props/**`, `ArtSource/Environment/**`, `ArtSource/UI/**`, `ArtSource/VFX/**`, `ArtSource/Marketing/**`가 존재함

다음이면 작업을 시작하지 않는다.

- 다른 브랜치
- 모르는 로컬 변경
- Merge Conflict
- 승인 Reference 누락
- 대형 Binary 정책 불명확
- 라이선스가 불명확한 Font·Audio·Texture 사용 필요

`main`이나 `contest/game-builders-2026`에 직접 Push하지 않는다.

---

# 3. 수정 가능한 폴더

## 직접 작업 가능

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

## 수정 금지

```text
Game/Assets/_Project/Code/**
Game/Assets/_Project/Scenes/**
Game/Assets/_Project/Prefabs/**
Game/Packages/**
Game/ProjectSettings/**
tools/**
```

Gameplay Scene 배치, Collider, Rigidbody, Material 생성, UI Runtime 연결, Particle System 연결은 조윤성과 통합 Codex가 담당한다.

---

# 4. 반드시 사용할 승인 레퍼런스

`references/exploration/`의 REVIEW 이미지는 사용하지 않는다. 아래 FINAL만 쓴다.

## 카메라와 전체 가독성

`references/approved/gameplay/PGT_01_Gameplay_Camera_v002_FINAL.png`

가져갈 것:

- Third-person 거리에서 플레이어·동료·물체·통로가 분리됨
- 너무 높은 가구가 카메라와 목표를 가리지 않음
- Interaction 대상과 배경 장식이 명확히 구분됨

복제하지 않을 것:

- 정확한 방 구조
- 정확한 소품 수량
- 이미지 속 적 외형
- 정확한 카메라 수치

## Chair Redirect 핵심 장면

`references/approved/gameplay/PGT_02_Chair_Redirect_Core_Loop_v010_FINAL.png`

가져갈 것:

- 바퀴 달린 의자가 실제 바닥을 유지함
- 스트랩을 연결할 하중 지지 금속 프레임이 보임
- 의자·스트랩·Drawer 목표의 인과관계가 읽힘
- 의자가 장난감이나 마법 물체처럼 보이지 않음

## 충돌 후 결과·재질·VFX

`references/approved/gameplay/PGT_03_Chain_Reaction_Aftermath_v003_FINAL.png`

가져갈 것:

- 충돌 이후 물체 위치 변화가 읽힘
- 종이·먼지·마찰 흔적의 절제된 표현
- 실루엣과 통로 가독성을 해치지 않는 VFX

공모전에서 금지:

- 유리 파괴
- 구조물 파괴
- 런타임 파편
- 파괴를 통해 새 벽 통로 생성

## 지역 조명 참고

`references/approved/lighting/LGT_01_Night_Archive_Lighting_v001_FINAL.png`

주의:

- 이 이미지는 Records Archive 지역의 **로컬 조명 변주**다.
- 공모전 방 전체의 Color Grading을 그대로 복제하지 않는다.
- 차가운 형광등, Shadow Detail, PBR Highlight, After-hours tension만 참고한다.
- 지나친 암부와 Horror 분위기는 금지한다.

## 캐릭터·장비 스케일 보조

- `references/approved/characters/CHR_00_Player_Identity_Lock_v002_FINAL.png`
- `references/approved/characters/CHR_01_First_Ally_Identity_v001_FINAL.png`
- `references/approved/equipment/GEAR_01_Cargo_Strapping_System_v002_FINAL.png`

용도:

- 책상·의자·복사기 크기 관계
- 금속·플라스틱·직물 Material 관계
- Chair Anchor가 GEAR01 Clamp와 연결 가능한지 확인

## Runtime 사용 금지

`references/approved/gameplay/FIN_01_Conference_Table_Team_Finisher_v003_FINAL.png`

공모전 범위에서 회의용 테이블 피니셔, 두 번째 동료, 다인 합동 액션을 만들지 않는다.

---

# 5. 반드시 읽을 문서

1. `docs/03_art/ART_DIRECTION.md`
2. `docs/03_art/OFFICE_CORE_KIT.md`
3. `docs/03_art/CAMERA_SPEC.md`
4. `docs/03_art/CHAIR_REDIRECT_VISUAL_SPEC.md`
5. `docs/03_art/CHAIN_REACTION_AFTERMATH_VISUAL_SPEC.md`
6. `docs/03_art/NIGHT_ARCHIVE_LIGHTING_VISUAL_SPEC.md`
7. `docs/03_art/CARGO_STRAP_SPEC.md`
8. `docs/02_design/PROTOTYPE_CONTENT_LOCK.md`
9. `docs/10_contest/ART_DELIVERY_STANDARD.md`
10. `docs/10_contest/ASSET_NEEDS_STATUS.md`
11. `docs/10_contest/VISUAL_REFERENCE_INVENTORY.md`

핵심 Art Direction:

> **Grounded Corporate Action Realism**  
> **Grounded world. Absurd physics.**

금지:

- 애니메이션·카툰형 비율
- 플라스틱 장난감 재질
- 과한 네온
- 마법 이펙트
- 사이버펑크 사무실
- 무의미한 고채도 UI
- 실제 회사 브랜드 로고

---

# 6. 작업 우선순위와 내부 마감

| 순서 | 자산 | 1차 검토 마감 | 이유 |
|---:|---|---|---|
| 1 | `PRP_OfficeChair` | 2026-08-08 12:00 KST | Throw와 Strap Redirect의 핵심 물체 |
| 2 | `PRP_FileBinder` | 2026-08-08 23:59 KST | Direct Throw와 Ally Catch 핵심 물체 |
| 3 | `PRP_DrawerCabinet` | 2026-08-14 23:59 KST | 통로 개방 Objective |
| 4 | `PRP_Copier` | 2026-08-14 23:59 KST | Heavy recognition landmark |
| 5 | `OfficeCoreKit P0` | 2026-08-18 23:59 KST | 공모전 방 Production Art |
| 6 | `UI Wireframe` | 2026-08-16 23:59 KST | 목표·Prompt·Win/Failure 구조 확인 |
| 7 | `UI Final Export` | 2026-08-20 23:59 KST | Runtime 통합 |
| 8 | `VFX/Audio P1` | 2026-08-21 23:59 KST | 최종 피드백과 완성도 |
| 9 | `Thumbnail/Video` | 2026-08-24 12:00 KST | 제출 패키지 |

모든 날짜는 조윤성의 Gameplay 일정에 따라 갱신될 수 있다. `ASSET_NEEDS_STATUS.md`가 최신 상태표다.

---

# 7. 첫 번째 납품 — Office Chair

경로:

`ArtSource/Props/PRP_OfficeChair/`

필수 파일:

```text
Source/PRP_OfficeChair_A_v001.blend 또는 편집 가능한 원본
Export/PRP_OfficeChair_A_v001.fbx
Textures/PRP_OfficeChair_A_BaseColor_v001.png
Textures/PRP_OfficeChair_A_Normal_v001.png
Textures/PRP_OfficeChair_A_Mask_v001.png
Preview/PRP_OfficeChair_A_Turntable_v001.mp4 또는 이미지 세트
Preview/PRP_OfficeChair_A_AnchorDetail_v001.png
DELIVERY.md
```

반드시 보여야 할 것:

- 5개 바퀴 기반 또는 현실적인 Office Chair 구조
- 좌석 아래 하중 지지 금속 프레임
- GEAR01 Clamp가 연결될 수 있는 명확한 금속 Anchor
- 플라스틱 휠 하우징과 금속 Anchor의 구분
- 바닥에서 굴러갈 수 있는 실루엣

금지:

- 캐스터나 휠 축을 Anchor로 설계
- 레이싱 게이밍 체어
- 미래형·군사형 의자
- 과도한 작은 부품

---

# 8. File Binder

경로:

`ArtSource/Props/PRP_FileBinder/`

필수:

```text
Source/PRP_FileBinder_A_v001.blend
Export/PRP_FileBinder_A_v001.fbx
Textures/PRP_FileBinder_A_BaseColor_v001.png
Textures/PRP_FileBinder_A_Normal_v001.png
Textures/PRP_FileBinder_A_Mask_v001.png
Preview/...
DELIVERY.md
```

조건:

- 한 손으로 들 수 있는 현실적 크기
- Throw 중 회전해도 실루엣이 읽힘
- 브랜드 로고와 읽을 수 없는 AI 글자 금지
- Runtime에서는 동일 Base Mesh 3개 사용
- Material Variation이 필요하면 최대 2개

---

# 9. Drawer Cabinet

`ArtSource/Props/PRP_DrawerCabinet/`

조건:

- 바퀴 달린 실제 Office Drawer Cabinet
- 통로를 막고 있다는 것이 읽히는 덩어리
- Player가 직접 집을 수 없어 보이는 무게감
- 충돌로 밀릴 수 있는 바닥 구조
- 지나치게 얇거나 가벼워 보이지 않음

Runtime 역할:

- 직접 획득 불가
- Strap 대상 아님
- 충돌로 이동
- 일정 거리 이동하면 Side Route Open

---

# 10. Copier

`ArtSource/Props/PRP_Copier/`

조건:

- Heavy recognition-only landmark
- 정적이고 무거워 보임
- 움직임·전도·파괴·상호작용을 암시하지 않음
- 실제 회사 로고 금지
- 종이·토너 VFX는 P1이며 Gameplay 기능이 아님

---

# 11. Office Core Kit P0

공모전 한 방에 필요한 최소 Production Kit부터 만든다.

## Architecture / Fixed Fixture

```text
ENV_Wall_Straight_A_v001
ENV_DoorFrame_A_v001
ENV_LowPartition_Straight_A_v001
ENV_Floor_Module_A_v001
ENV_Ceiling_Module_A_v001
ENV_Column_A_v001
ENV_FluorescentLight_A_v001
ENV_FixedDesk_A_v001
```

## Decorative P1

```text
ENV_WorkstationCluster_A_v001
ENV_PaperBoxCluster_A_v001
ENV_PlantWasteCluster_A_v001
ENV_SignageWearDecal_A_v001
```

P0 Architecture가 끝나기 전 Decorative를 만들지 않는다.

Environment는 Room Layout을 직접 결정하지 않는다. Modular Piece와 Material만 납품한다. Contest Scene 수정은 금지다.

---

# 12. UI 범위

## P0 Wireframe

먼저 편집 가능한 Wireframe을 만든다.

```text
Title
Controls
Current Objective
Interact Prompt
Carry State
Ally Availability
Strap State
Route Open
Failure
Win
Retry
Pause
Mute
Credits
```

UI 원칙:

- 1920×1080 16:9 기준
- 1366×768에서도 읽힘
- 화면 중심 Gameplay를 가리지 않음
- 장문 설명 대신 짧은 동사
- 색상만으로 상태를 구분하지 않음
- 기업 시스템 UI와 Player HUD를 구분
- ZERO DEFECT는 차갑고 행정적인 시스템 인상
- 실제 회사 로고·운영체제 UI 복제 금지

Font는 반드시 사용·배포 라이선스를 기록한다.

---

# 13. VFX 범위

허용:

- Interaction Highlight
- Binder Impact
- Chair Wheel Dust
- Strap Attach/Tension/Release
- Drawer Scrape Dust
- Route Open
- EXIT Activation
- Failure
- Win

금지:

- 마법 오라
- 네온 에너지 줄
- 폭발
- 유리 파괴
- 파편
- Runtime Fracture
- 화면을 가리는 과한 Paper Storm

VFX는 “무엇이 일어났는지”를 보여줘야지, 물리 결과를 숨기면 안 된다.

---

# 14. Audio와 Marketing

Audio를 맡는 경우:

- 직접 제작 또는 정확한 사용 허가 필요
- WAV 48kHz 권장
- `ArtSource/Audio/Licenses/`에 증거 저장
- AI Audio 사용 시 도구·모델·라이선스·사람 수정 기록

Marketing:

- Thumbnail 16:9
- 게임의 핵심 장면은 Chair Redirect 또는 Drawer Route Open
- Final Gameplay Capture를 쓰며 승인 이미지를 게임 화면처럼 속이지 않음
- Demo Video ≤ 3분
- Source Project와 License 기록

---

# 15. WebGL용 제작 목표

| 자산 | Triangle 목표 | Material Slot 목표 | Texture |
|---|---:|---:|---|
| Binder | 1.5k 이하 | 1 | 1K |
| Chair | 8k 이하 | 최대 2 | 1K, 필요 시 2K |
| Drawer | 6k 이하 | 최대 2 | 1K |
| Copier | 8k 이하 | 최대 2 | 1K |
| 환경 모듈 1개 | 4k 이하 | 1~2 | Shared 1K~2K |
| Decorative 1개/Cluster | 3k 이하 | 1 | 512~1K |

목표를 넘으면 `DELIVERY.md`에 이유를 적는다.

권장:

- Shared Material와 Atlas
- 작은 메시 디테일보다 Normal/Roughness/Decal
- Baked Lighting이 들어간 Base Color 금지
- 보이지 않는 후면·내부 면 최소화
- Mesh Collider용 복잡한 고밀도 모델 금지

---

# 16. AI 사용 규칙

AI 사용 가능:

- Concept
- Base Mesh
- Retopology 보조
- Texture 초안
- UI Variation
- VFX Flipbook 초안
- Marketing Layout

사람이 반드시 수정·검수:

- Perspective·Scale
- Topology·UV·Normal
- Baked Shadow 제거
- 읽을 수 없는 AI 문자 제거
- 브랜드·상표 제거
- Material PBR 일관성
- Anchor·Gameplay 역할
- UI 가독성
- 제3자 IP

각 `DELIVERY.md`에 기록:

```text
AI Tool / Model:
AI Input References:
Prompt Summary:
Generated Elements:
Human Corrections:
Third-party IP Check:
```

AI 이미지·모델을 승인 레퍼런스보다 우선하지 않는다.

---

# 17. DELIVERY.md 완료 조건

필수:

- Source
- Export
- Texture
- Preview
- Asset ID
- Creator
- Contribution Date
- License
- Approved Reference
- Triangle Count
- Material Slots
- Texture Resolution
- Scale/Pivot/Axis
- Known Issues
- AI 사용 기록

UI/VFX/Audio/Marketing도 원본·Export·라이선스를 모두 남긴다.

---

# 18. Commit과 PR 규칙

권장 Commit:

```text
art: deliver office chair and anchor detail v001
art: deliver file binder gameplay prop v001
art: deliver drawer and copier props v001
art: deliver office core architecture kit v001
ui: deliver contest UI wireframe v001
ui: deliver contest UI exports v001
vfx: deliver contest physical feedback v001
marketing: deliver thumbnail and video source v001
```

한 PR에 모든 아트를 몰아넣지 않는다.

PR Base:

`contest/game-builders-2026`

PR 본문:

- Asset IDs
- 승인 레퍼런스
- Source/Export/Texture 목록
- AI 사용
- Budget
- Known Issues
- Runtime Integration 요청

`.gitattributes`와 Git LFS 설정을 혼자 변경하지 않는다. 대형 파일 Commit 전 조윤성에게 확인한다.

---

# 19. 오늘 바로 시작할 작업

## Step 1

승인 PGT02와 GEAR01 Reference, `CARGO_STRAP_SPEC.md`를 읽는다.

## Step 2

Office Chair Blockout을 만든다.

첫 Preview:

```text
Preview/PRP_OfficeChair_A_Blockout_v001.png
Preview/PRP_OfficeChair_A_AnchorDetail_v001.png
Preview/PRP_OfficeChair_A_SilhouetteGameplayView_v001.png
```

## Step 3

동시에 File Binder Low-poly Blockout을 만든다.

```text
Preview/PRP_FileBinder_A_Blockout_v001.png
```

## Step 4

조윤성이 Chair Anchor와 크기를 승인한 뒤 최종 Mesh/UV/Texture로 들어간다.

---

# 20. 작업 완료 보고 양식

```text
DESIGNER B DELIVERY REPORT

Asset IDs:
Branch:
Commit:
PR:

Approved References Used:
Source Files:
Export Files:
Textures/UI/VFX/Audio:
Preview:

Triangle Count:
Material Slots:
Texture Resolution:
Scale/Pivot:

AI Tool / Model:
Human Corrections:
License / Ownership:

Gameplay Readability Check:
Known Issues:
Owner Review Needed:
Next Asset:
```

---

# 21. 절대 기억할 것

- PGT02의 Chair와 GEAR01 연결 가독성이 첫 번째다.
- Environment가 Gameplay Object를 가리면 실패다.
- LGT01은 전체 게임 Global Look이 아니다.
- FIN01은 공모전 Runtime 범위가 아니다.
- Scene을 직접 수정하지 않는다.
- 디자인이 늦어져도 Graybox Gameplay는 계속 진행된다.
- 검수 가능한 `v001`을 제때 제출하는 것이 완벽한 미완성본보다 낫다.
- 최종 승인 권한은 조윤성에게 있다.
