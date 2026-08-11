# BREAK NOTICE Showcase VFX Direction v001

- Status: `REVIEW / SOURCE PREPARATION ONLY / NO RUNTIME INTEGRATION AUTHORITY`
- Ticket: `SHOWCASE-VFX-SOURCE-001`
- Execution Relay: `EAR-CONTEST-0002-VFX-SOURCE-001`
- Showcase Lineage: `CONTEST-0002 / PRT-RM-002`
- Execution Baseline: `f51315aea91dd00293a4f5c387f1cb748865fd11`
- Branch: `codex/showcase-vfx`
- Date: `2026-08-12 KST`
- Owner Approval: `NOT REQUESTED BY THIS DOCUMENT / HUMAN OWNER DECISION REQUIRED`

## 1. 목적과 권위 경계

이 문서는 잠긴 Showcase Gameplay의 상태와 물리 사건을 짧고 명확한
Presentation VFX로 읽히게 만들기 위한 Source-Art 제작 기준이다. Gameplay
상태를 만들거나 변경하지 않으며, 기존 Gameplay Fact를 뒤늦게 표현하는
one-way Presentation만 다룬다.

다음 권위를 보존한다.

- `CONTEST-0001` C0–C4 accepted foundation과 기존 Graybox는 변경하지 않는다.
- `CONTEST-0002` S0만 완료됐고 S1, 역사적 C5와 S9 Runtime Art Integration은
  모두 승인되지 않았다.
- Binder `5`, Chair `3`, Drawer `1`, Copier `1`, mobile CHR_01 한 명과 동일
  Runner 두 명이라는 `PRT-RM-002` Content Lock을 바꾸지 않는다.
- PASS, BRACE, BREAK 중 서로 다른 두 badge가 TEAM READY를 만든다는 잠긴
  Gameplay Fact를 그대로 표시한다.
- `FIN-P-001 — HANDOVER CRASH`는 선택적인 약 2.5초 설계 방향이며, Direct
  Chair와 Strap route는 Finisher 없이도 유효해야 한다.
- 파괴, runtime fracture, 새 Damage/Break mechanic, gameplay debris와 임의의
  screen-wide 효과는 없다.

직접 권위 소스는 다음과 같다.

- `docs/10_contest/showcase/SHOWCASE_GAMEPLAY_LOCK.md`
- `docs/10_contest/showcase/SHOWCASE_ASSET_MATRIX.md`
- `docs/10_contest/owner_directives/2026-08-11/BREAK_NOTICE_CONTEST_SHOWCASE_MAP_DESIGN_v001.txt`
  (`dfe3d2f312bb84dae7df7d596f4a7153f7ec69cfaac3590b93cbc6ab78833834`)
- `docs/10_contest/owner_directives/2026-08-11/BREAK_NOTICE_GAMEPLAY_ANIMATION_COVERAGE_v003_FINAL.txt`
  (`6c5b6aca6e83686331a1d64d967d0489327aac35d285e0f2aa7da9fb1972bb69`)

## 2. 시각 원칙

1. **물리 원인이 먼저다.** 효과는 손의 release, 바닥 contact, wheel 접촉,
   물체 충돌과 authored state change의 위치·방향을 따른다.
2. **짧고 국소적이다.** 효과는 충돌 지점과 상호작용 대상 주변에 머물며,
   목표와 진행 lane을 가리지 않는다.
3. **재질이 형태를 결정한다.** Binder는 평평한 종이 조각, Chair는 낮은
   wheel dust와 방향성 streak, Drawer는 바닥 가까운 마찰 먼지와 짧은
   impact fan을 사용한다.
4. **색상은 보조 수단이다.** bracket 수, tick 수, 방향, stamp, 끊어진 형태,
   위치와 motion으로 상태를 중복 전달한다.
5. **사무실 재료의 범위에 머문다.** 종이 흰색, 차가운 회색, 먼지 갈색,
   절제된 safety amber, muted maintenance green과 warning red만 역할색으로
   사용한다. 정확한 RGB/노출은 `PROPOSED / REVIEW REQUIRED`다.
6. **Gameplay를 속이지 않는다.** 예측 궤적, 피해량, breakability, 통과 가능한
   경로 또는 TEAM READY를 실제 Gameplay Fact보다 먼저 암시하지 않는다.

## 3. 형태 언어

| Cue family | 주 형태 | 보조 motion | 금지 표현 |
|---|---|---|---|
| 획득 가능 | 대상 모서리의 네 개 열린 bracket, class별 1개/2개 하단 tick | 낮은 호흡형 scale, 대상 local-space 고정 | 전신 aura, 벽 너머 glow, 자동 pickup처럼 보이는 흡입 |
| Throw Charge | reticle 바깥의 1·2·3단 기계식 tick ring | 단계 전환 시 한 번의 inward click | 에너지 구체, 번개, 과도한 bloom |
| 물리 contact | contact normal을 따르는 짧은 fan, 재질별 paper/dust/wheel 요소 | 즉시 팽창 후 감쇠 | 폭발, gameplay 파편, 충돌 지점 은폐 |
| Ally / Q | 기존 READY 느낌표, 손/물체 쪽 짧은 outbound chevron | 한 번의 pop 또는 방향 pulse | full trajectory 예측선, 순간이동 trail |
| PASS / BRACE / BREAK | 서로 다른 pictogram과 stamped frame | 한 번 눌렸다 복원되는 stamp | 색상만 다른 동일 아이콘 |
| TEAM READY | 두 개의 맞물린 bracket와 굵은 double-check | 중앙으로 맞물린 뒤 정지 | 화면 전체 flash, 승리로 오인되는 폭발 |
| Runner pressure | 어깨 진행 방향 wedge, 발밑의 짧은 압축선 | telegraph 동안 수축, commit 뒤 제거 | 위험 범위보다 큰 원, 마법식 AoE |
| Drawer / Exit | Drawer contact fan, 열린 route 방향의 바닥 chevron, Exit fixture mask | cause→route→Exit 순서의 짧은 relay | state change 이전 Exit 점등, 전면 bloom |
| 확인 / 거절 | 닫힌 bracket+check / 끊어진 bracket+X | 확인은 inward snap, 거절은 짧은 lateral shake | 장시간 toast, 색상 단독 전달 |

## 4. 역할색과 접근성 백업

| Color role | 사용 | 색상 외 필수 백업 |
|---|---|---|
| Neutral eligible | Binder/Chair 획득 가능 | 열린 bracket, Light 1 tick / Medium 2 ticks, 대상 부착 위치 |
| Safety amber | Charge 진행, Runner telegraph, 주의 상태 | tick 증가, 수축 wedge, 명확한 motion 방향 |
| Muted maintenance green | 확인, badge 획득, route/Exit 활성 | check/stamp, 방향 chevron, UI text |
| Muted warning red | supply/path/UI rejection | 끊어진 bracket, X, lateral shake와 원인 text |
| Paper white / material local color | Binder paper와 일반 contact | 재질 고유 silhouette와 contact-normal 방향 |

모든 상태는 grayscale에서도 silhouette, tick count, icon 또는 motion으로
구분되어야 한다. READY 느낌표와 Q prompt는 world-space cue만으로 끝내지 않고
동일한 UI text 상태를 유지한다. 빠른 flash만으로 필수 정보를 전달하지 않는다.

## 5. 물리 재질 반응

### Binder

- paper plane이 아니라 짧고 불규칙한 평면 조각과 얇은 edge streak를 쓴다.
- pickup·catch에는 먼지 폭발을 쓰지 않는다.
- impact 때만 contact normal 반대 방향으로 작은 paper fan을 낸다.
- 모든 수량·수명은 `PROPOSED / REVIEW REQUIRED`이며 Gameplay collision은 `0`이다.

### Chair

- Chair는 floor-supported 상태를 유지한다.
- wheel feedback은 wheel-floor 접촉점에서 낮고 짧게 발생하며 Chair 본체보다
  높게 솟지 않는다.
- feed와 redirect에는 grounded direction을 보여주는 2선 skid language를 쓰고,
  공중 발사나 rocket trail처럼 보이는 효과를 금지한다.

### Drawer와 고정 표면

- Drawer causal impact는 contact fan과 바닥 가까운 마찰 먼지로만 표시한다.
- route-state 변화는 실제 authored route state가 바뀐 뒤에만 방향 chevron으로
  확인한다.
- crack, fracture, 유리, rubble, 파편 collider, 새 dent state를 만들지 않는다.

### Runner와 비살상 contact

- Runner pressure는 공격 전 readability를 위한 압축 wedge와 발 step accent를
  사용한다.
- contact flash는 차가운 흰색/회색의 짧은 비발광 star-wedge이며 magic hit나
  damage number처럼 보이지 않는다.
- 반응 animation과 physical displacement가 주 정보이고 VFX는 contact 지점만
  보조한다.

## 6. 주요 sequence

### Q Supply와 Binder Catch

`READY Fact → 단일 Q prompt → supply commit → physical release → catch/miss Fact`
순서를 보존한다. outbound chevron은 release를 앞당겨 보이지 않으며, Catch cue는
손과 Binder의 실제 contact가 확정된 뒤에만 발생한다. Miss에는 성공 cue를 쓰지
않는다.

### Chair floor-supported feed

`Ally grip → grounded push → visible hand release → wheel-floor contact → recovery`
순서를 따른다. wheel cue는 바닥 contact가 유효한 동안만 짧게 샘플링하고 path
rejection이면 즉시 중단한다.

### PASS / BRACE / BREAK와 TEAM READY

각 badge는 해당 Gameplay Fact가 확정된 뒤 한 번 stamp 된다. TEAM READY는 서로
다른 두 badge가 확정된 뒤에만 double-bracket가 맞물린다. badge VFX는 decay,
spend, persistence 또는 새 resource를 암시하지 않는다.

### HANDOVER CRASH

다음은 잠긴 인과를 읽히게 하는 Presentation 순서이며 정확한 hook과 timing은
모두 `PROPOSED / REVIEW REQUIRED`다.

1. commit 확인: 작은 paired bracket, screen-wide flash 없음
2. Chair floor skid: wheel contact와 진행 방향만 표시
3. Chair→Runner contact: Runner silhouette를 가리지 않는 짧은 contact wedge
4. Runner→Drawer causal contact: Drawer local impact fan과 낮은 dust
5. route Fact 확인: route chevron
6. Exit Fact 확인: fixture mask와 방향 marker

Chair→Runner→Drawer의 물리 결과가 VFX보다 먼저이며, Finisher VFX가 Drawer를
움직이거나 Exit를 활성화하지 않는다.

## 7. Camera·Occlusion 규칙

- third-person gameplay camera에서 contact point, actor silhouette와 이동 lane이
  동시에 보여야 한다.
- object-local marker는 대상 bounding silhouette 바깥에 두고 mesh 중심을
  덮지 않는다.
- 카메라와 대상 사이에 solid occluder가 있으면 world cue는 숨기고 허용된 UI
  rejection/target-lost 상태만 남긴다.
- near-camera particle, full-screen overlay, screen edge blood/damage vignette,
  long-lived smoke와 volumetric fog를 금지한다.
- 예상 screen coverage와 alpha overdraw 기준은
  `SHOWCASE_VFX_TECHNICAL_DELTA_v001.md`의 `PROPOSED / REVIEW REQUIRED`
  budget을 따른다.

## 8. Source 제작 후보와 현재 상태

이번 Relay에서는 아래 실제 Source/Texture/Flipbook/Preview를 만들지 않는다.
모두 후속 Source 제작 Relay 후보이며 현재 상태는 `MISSING`이다.

Editable Source 후보:

- `ArtSource/VFX/Showcase/Source/VFX_Showcase_IndustrialCueShapes_v001.svg`
- `ArtSource/VFX/Showcase/Source/VFX_Showcase_PaperBits_v001.svg`
- `ArtSource/VFX/Showcase/Source/VFX_Showcase_ContactDust_v001.blend`
- `ArtSource/VFX/Showcase/Source/VFX_Showcase_WheelContact_v001.blend`
- `ArtSource/VFX/Showcase/Source/VFX_Showcase_ImpactMarks_v001.svg`
- `ArtSource/VFX/Showcase/Source/VFX_Showcase_ExitLightMasks_v001.svg`
- `ArtSource/VFX/Showcase/Source/VFX_Showcase_UIConfirmReject_v001.svg`

Texture/Flipbook 후보:

- `ArtSource/VFX/Showcase/Textures/T_VFX_Showcase_IndustrialCueShapes_Atlas_v001.png`
- `ArtSource/VFX/Showcase/Textures/T_VFX_Showcase_PaperBits_Atlas_v001.png`
- `ArtSource/VFX/Showcase/Flipbooks/T_VFX_Showcase_ContactDust_4x4_v001.png`
- `ArtSource/VFX/Showcase/Flipbooks/T_VFX_Showcase_WheelContact_4x4_v001.png`
- `ArtSource/VFX/Showcase/Textures/T_VFX_Showcase_ImpactMarks_Atlas_v001.png`
- `ArtSource/VFX/Showcase/Textures/T_VFX_Showcase_ExitLightMasks_v001.png`
- `ArtSource/VFX/Showcase/Textures/T_VFX_Showcase_UIConfirmReject_Atlas_v001.png`

Preview 후보:

- `ArtSource/VFX/Showcase/Preview/VFX_Showcase_CueBoard_v001.png`
- `ArtSource/VFX/Showcase/Preview/VFX_Showcase_AccessibilityGrayscale_v001.png`
- `ArtSource/VFX/Showcase/Preview/VFX_Showcase_GameplayCameraReadability_v001.mp4`

License evidence 후보:

- `ArtSource/VFX/Showcase/Licenses/VFX_Showcase_Source_License_Ledger_v001.csv`
- `ArtSource/VFX/Showcase/Licenses/VFX_Showcase_Original_Work_Declaration_v001.md`
- `ArtSource/VFX/Showcase/Licenses/VFX_Showcase_External_Source_Evidence_v001.md`

## 9. 외부 Source 사용 조건

후속 Relay에서 인터넷 Source를 검토할 수 있으나, commit 전에 제작자, 원본 URL,
다운로드 날짜, 원본 파일 hash, 정확한 license text, commercial use,
redistribution, derivative-work, attribution과 contest 제출 허용을 각각 증명해야
한다. `NC`, `ND`, editorial-only, attribution 불능, 출처 불명, 권리 충돌 또는
원본 재배포 금지 조건이 전달 방식과 맞지 않는 Source는 사용하지 않는다.

현재 외부 Source를 선택하거나 다운로드하지 않았고 License는 `MISSING`이다.
새 AI VFX 자산도 이 Relay로 승인되지 않는다.

## 10. 완료 정의

이 v001 방향은 다음까지만 완료한다.

- 잠긴 Gameplay Event와 VFX 역할 매핑
- 물리·접근성·occlusion·성능 Source 기준
- 정확한 후보 파일명과 후속 Relay 경계
- Binary/Runtime/Unity 변경 `0`

이 문서는 Production approval, S1/C5/S9 authorization, Runtime hook 확정,
Texture/Flipbook delivery 또는 Integration 완료를 의미하지 않는다.

No new production asset blocks this work package. Graybox fallback remains active.
