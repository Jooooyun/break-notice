# Waterfall Development Plan with Validation Gate

순수 폭포수는 게임의 재미를 문서만으로 증명할 수 없다는 문제가 있다. 본 프로젝트는 설계를 고정하되, 본 제작 전 단 한 번의 Graybox Validation Gate를 둔다.

## Current Gate Status

- G1 Requirements: `PASSED`
- G2 Design: `PASSED / OWNER-APPROVED BASELINE`
- G3 Technical: `NOT PASSED / CURRENT BASELINE WORK`
- G3 Technical Decisions:
  `OWNER-APPROVED / BASELINE APPLIED`
- G3 Causal Result-Family Alignment:
  `OWNER-APPROVED / CORRECTED`
- G3 Technical Final Audit:
  `REVISION REQUIRED / HISTORICAL BASELINE AUDIT`
- G3 Residual Active Status and Package-Authority Correction:
  `OWNER-APPROVED / COMPLETE`
- G3 Technical Final Audit Remediation:
  `CORRECTIONS APPLIED / VERIFIED BY RE-AUDIT`
- G3 Technical Baseline Re-Audit:
  `READY FOR G3 GATE DECISION`
- G3 Technical checklist: `6 / 6`
- Design Freeze: `28 / 32`
- Production Visual Lock sequence: `COMPLETE`
- Current Phase label: `PRE-PRODUCTION / G3 TECHNICAL BASELINE`
- Phase label review: `COMPLETED / OWNER-APPROVED`
- Full Design Lock: `INCOMPLETE`
- Graybox: `NOT STARTED`
- Implementation Gate: `CLOSED`

## Phase 0 — Charter

산출물: Project Charter, 제약, 성공 기준.

Gate G0: 프로젝트 목적과 1인 범위 승인.

## Phase 1 — Requirements

산출물: Product/NFR, RTM, Non-goals.

Gate G1: 모든 MUST 요구사항에 ID와 검증 방식 존재.

Status: `PASSED` — Owner-approved G1 Baseline 동기화와 검증 완료.

## Phase 2 — Game & Art Design

산출물: GDD, Gameplay Spec, Ally/Physics, Art Direction, Canon 이미지.

Gate G2: 핵심 루프, 카메라, 장비, 동료, 파괴 예산의 모순 제거.

Status: PASSED — Owner-approved G2 contracts, exact Prototype content,
remediation and successful Re-Audit verified.

## Phase 3 — Technical Design

산출물: 아키텍처, 데이터, 성능 예산, 테스트 설계, Codex 프로토콜.

Gate G3: 모든 핵심 요구사항이 구현 책임과 테스트에 매핑.

Status: NOT PASSED / OWNER-APPROVED BASELINE / READY FOR G3 GATE DECISION

책임, 데이터, 이벤트, 검증, 성능 예산, Reset과 test-harness 방향,
exact Editor/package documentation lock은 적용되었고 Technical
checklist는 `6 / 6`다. Causal Result Family taxonomy는 Owner-approved
G2 계약과 정렬된 상태로 교정되었다. 역사적 Final Audit은
`REVISION REQUIRED`이고 두 잔여 활성 권한 오류의 교정은 완료됐지만,
remediation은 `CORRECTIONS APPLIED / VERIFIED BY RE-AUDIT`다. Re-Audit은
`READY FOR G3 GATE DECISION`이고 다음 단계는
`TASK-0054 — Promote G3 Technical Baseline and Record G3 Passage`이며
G3는 아직 통과하지 않았다. G4, Graybox와 Unity 구현 권한은 없다.

## Phase 4 — Graybox Validation

`G4 DRAFT CONTENT CANDIDATE — NOT G1-APPROVED VALUES`

한 방, 한 동료, 적 3명, Light 2, Medium 2, Heavy 1.

Gate G4: 5분 재미와 재플레이 행동 통과.

실패 시 수치·카메라·배치·타이밍만 조정한다. 핵심 정체성을 바꾸려면 C3 Change Request가 필요하다.

## Phase 5 — Design Freeze

Graybox 결과를 반영해 문서 최종 동결.

Gate G5: 본 제작 범위, 에셋 목록, 일정, 리스크 승인.

## Phase 6 — Vertical Slice Production

`G5/G6 DRAFT CONTENT CANDIDATE — NOT G1-APPROVED VALUES`

3개 방, 10~15분 Run, 핵심 VFX/Audio/UI, 12개 업그레이드.

Gate G6: 기능 완성, 성능 예산, 인수 테스트 통과.

## Phase 7 — Verification

외부 플레이테스트, 버그 수정, UX, 성능.

Gate G7: Production Decision.

## Phase 8 — Full Production Decision

- 확장
- 범위 축소 후 출시
- 재설계
- 중단

을 데이터로 결정한다.
