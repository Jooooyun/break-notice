# ADR-0018 — CHR_00 Player Turnaround Approval

- Status: Accepted
- Date: 2026-07-25
- Decision Source: Human Owner `APPROVE` after GPT Pro binary-contract
  `PASS`
- Scope: bounded CHR_00 Production Support registration

## Context

`docs/03_art/PLAYER_TURNAROUND_PRODUCTION_BRIEF.md`는 CHR_00의 정확한
다섯 전신 뷰, neutral modeling pose, 의복·장비 continuity, 후면·측면
가독성과 binary review contract를 Owner-approved 기준으로 고정했다.

후보는 다음 경로에 `REVIEW`로 생성되었다.

`references/exploration/CHR_00_Player_Turnaround_v001_REVIEW.png`

GPT Pro는 이 후보가 binary review contract를 충족한다고 `PASS`했고,
Human Owner는 이후 명시적으로 `APPROVE`했다.

## Decision

후보의 byte-identical copy를 다음 경로에 등록한다.

`references/approved/characters/CHR_00_Player_Turnaround_v001_FINAL.png`

등록 상태는 `OWNER-APPROVED / REGISTERED / LOCKED PRODUCTION SUPPORT`다.
이는 승인된 여덟 Production Visual과 별개의 Production Support 한
개이며 Production Visual Lock sequence나 primary Production Visual
count `8`을 변경하지 않는다.

## Registration Evidence

- Review source:
  `references/exploration/CHR_00_Player_Turnaround_v001_REVIEW.png`
- Approved destination:
  `references/approved/characters/CHR_00_Player_Turnaround_v001_FINAL.png`
- Review result: `PASS — REVIEW CONTRACT SATISFIED`
- Human Owner disposition: `APPROVE`
- SHA-256:
  `8a852d77296ebf39cb3cf81c51d931d141dff63f358182ab5638bbbd4329d214`
- Dimensions: `1774 × 887`
- File size: `2,096,375 bytes`
- Copy relation: `BYTE-IDENTICAL`

Review source는 untracked `REVIEW` 증거로 보존하고 수정·이동·삭제·stage
또는 commit하지 않는다. Approved destination만 tracked Production
Support로 등록한다.

## Identity Authority Precedence

CHR_00 Identity의 주 권위는 계속 다음 세 항목이다.

1. `references/approved/characters/CHR_00_Player_Identity_Lock_v002_FINAL.png`
2. `docs/03_art/PLAYER_VISUAL_SPEC.md`
3. `docs/09_adrs/ADR-0007-PLAYER-VISUAL-LOCK.md`

승인된 Turnaround Production Support는 orthographic과 modeling
continuity만 보조하며 위 세 주 Identity 권위에 종속된다. 충돌이
있으면 주 Identity 권위가 우선하며 Turnaround의 incidental detail로
Identity, 장비 기능 또는 게임플레이를 재정의하지 않는다.

## Canonical Production-Support Content

승인된 Production Support 범위는 다음에 한정한다.

- 정확히 다섯 전신 뷰:
  `FRONT ORTHOGRAPHIC`, `LEFT PROFILE`, `REAR ORTHOGRAPHIC`,
  `RIGHT PROFILE`, `FRONT THREE-QUARTER CONTINUITY VIEW`
- 동일 인물·신체 비율·축척·바닥선과 neutral modeling pose
- 승인된 의복, 하네스, 한쪽 팔 브레이스, 허리 릴, 보관된 검정·황색
  스트랩과 은색 카라비너 continuity
- 후면 하네스 routing과 좌우 profile의 제작 가독성
- 붉은 넥타이와 장비·물체·하네스의 완전한 분리

## Non-Canonical Boundary

다음은 Canon 또는 새 권한이 아니다.

- 정확한 모공, 머리카락 가닥, 주름, 얼룩과 흠집 위치
- bolt, label, screw, stitch와 미세 construction
- 생성 과정의 미세 비대칭
- 정확한 조명, 그림자, typography와 spacing
- topology, polygon count, UV, texture, material, LOD, skeleton, rig,
  skin weight, blendshape와 animation
- gameplay input, timing, state, sequence, 물리값과 장비 내부 작동

## Gate and Implementation Boundary

이 등록으로 `주인공 Turnaround` checklist 항목을 체크하며 Design
Freeze는 `32 / 32`와 `COMPLETE / FINAL PRE-IMPLEMENTATION REVIEW
REQUIRED`가 된다.

그러나 다음 상태는 유지한다.

- G4 Graybox: `NOT PASSED / NOT STARTED / NOT AUTHORIZED`
- G5 Freeze: `NOT PASSED / FINAL PRE-IMPLEMENTATION REVIEW REQUIRED`
- Full Design Lock:
  `INCOMPLETE / FINAL PRE-IMPLEMENTATION REVIEW REQUIRED`
- Graybox: `NOT STARTED`
- IMP-0001: `OWNER-APPROVED DECOMPOSITION / EXECUTION BLOCKED`
- Implementation Gate: `CLOSED`

이 ADR은 3D modeling, rigging, texture, material, Unity asset, gameplay,
Scene, Prefab, package, test, build, Graybox 또는 구현 권한을 부여하지
않으며 Implementation Gate를 개방하지 않는다.

## Consequences

- CHR_00 제작 가독성을 위한 승인 Turnaround 증거가 하나 생긴다.
- 주 Identity 권위와 승인 Production Visual count `8`은 유지된다.
- Design Freeze checklist는 모두 체크되지만 Final Pre-Implementation
  Review와 필요한 Gate passage가 남는다.
- 다음 단일 작업은 `FINAL PRE-IMPLEMENTATION REVIEW REQUIRED`다.
