# Waterfall Development Plan with Validation Gate

순수 폭포수는 게임의 재미를 문서만으로 증명할 수 없다는 문제가 있다. 본 프로젝트는 설계를 고정하되, 본 제작 전 단 한 번의 Graybox Validation Gate를 둔다.

## Current Gate Status

- G1 Requirements: `PASSED`
- G2 Design: `PASSED / OWNER-APPROVED BASELINE`
- G3 Technical: `PASSED / OWNER-APPROVED BASELINE`
- G3 Technical Baseline: `OWNER-APPROVED / PASSED`
- G3 Technical Decisions:
  `OWNER-APPROVED / BASELINE APPLIED`
- G3 Causal Result-Family Alignment:
  `OWNER-APPROVED / CORRECTED`
- G3 Technical Final Audit:
  `REVISION REQUIRED / HISTORICAL BASELINE AUDIT`
- G3 Residual Active Status and Package-Authority Correction:
  `OWNER-APPROVED / COMPLETE`
- G3 Technical Final Audit Remediation:
  `COMPLETE / VERIFIED BY RE-AUDIT`
- G3 Technical Baseline Re-Audit:
  `READY FOR G3 GATE DECISION — CONSUMED BY G3 PASSAGE`
- G3 Gate Passage Source:
  `docs/05_production/G3_TECHNICAL_BASELINE_REAUDIT.md` at
  `9e1487e85be5a54ba0824c209deabcf738e1abf0`
- G4 Graybox: `NOT PASSED / NOT STARTED / NOT AUTHORIZED`
- G5 Production Freeze:
  `NOT PASSED / REQUIRES G4 EVIDENCE`
- G3 Technical checklist: `6 / 6`
- Design Freeze: `32 / 32`
- Design Freeze Completion:
  `COMPLETE / DOCUMENTARY PREREQUISITE ONLY`
- Final Pre-Implementation Review:
  `REVISION REQUIRED / docs/05_production/FINAL_PRE_IMPLEMENTATION_REVIEW.md`
- Final Pre-Implementation Review Remediation:
  `TASK-0061 CORRECTIONS APPLIED / RE-AUDIT REQUIRED`
- Pre-G4 Full Design Lock Semantics:
  `OWNER-APPROVED / DEFINED`
- Limited Implementation Gate Semantics:
  `OWNER-APPROVED / DEFINED`
- Destruction Budget and Implementation Rules:
  `OWNER-APPROVED / LOCKED`
- Office Core Kit:
  `OWNER-APPROVED / LOCKED`
- CHR_00 Player Turnaround Brief:
  `OWNER-APPROVED / CONSUMED BY APPROVED TURNAROUND`
- CHR_00 Player Turnaround Production Support:
  `OWNER-APPROVED / REGISTERED / LOCKED PRODUCTION SUPPORT`
- First Implementation Ticket:
  `IMP-0001 / EXECUTION BLOCKED / CORRECTION APPLIED / RE-AUDIT REQUIRED`
- Production Visual Lock sequence: `COMPLETE`
- Primary Production Visual Count: `8`
- Approved Production Support Count: `1`
- Current Phase label: `PRE-PRODUCTION / DESIGN FREEZE COMPLETION`
- Phase label review: `COMPLETED / OWNER-APPROVED`
- Full Design Lock:
  `INCOMPLETE / RE-AUDIT REQUIRED`
- Graybox: `NOT STARTED`
- Implementation Gate: `CLOSED`
- Current Next Task:
  `TASK-0062 — Re-Audit Final Pre-Implementation Readiness after Gate and Baseline Corrections`

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

Status: `PASSED — Owner-approved G3 baseline, corrections and successful Re-Audit verified.`

책임, 데이터, 이벤트, 검증, 성능 예산, Reset과 test-harness 방향,
exact Editor/package documentation lock은 적용되었고 Technical
checklist는 `6 / 6`다. Causal Result Family taxonomy는 Owner-approved
G2 계약과 정렬된 상태로 교정되었다. 역사적 Final Audit은
`REVISION REQUIRED`이고 두 잔여 활성 권한 오류의 교정은 완료됐지만,
remediation은 `COMPLETE / VERIFIED BY RE-AUDIT`다. Re-Audit의
`READY FOR G3 GATE DECISION` 판정은 기준선
`9e1487e85be5a54ba0824c209deabcf738e1abf0`에서 Owner-approved G3
passage의 근거로 소비되었다. Prototype의 모든 파괴 category cap
`0`과 Later-Slice bounded destruction hard cap 및 authored-state·
pooling·atomic-admission·Reset·fault 규칙은 Owner-approved로
LOCK되었다. Structural Feature는 `VERTICAL SLICE / SHOULD / PARTIAL`
및 별도 G5 승인 조건을 유지한다. Office Core Kit은 exact
Prototype-required family `16`, Later-Slice extension candidate `8`,
interaction·variant·reuse와 Prototype destruction `0` 경계로
`OWNER-APPROVED / LOCKED`되었지만 실제 asset creation은
`NOT STARTED / NOT AUTHORIZED`다. LGT_01은 Records Archive-local
`COULD`, FIN_01은 `OUT / COULD`를 유지하고 G5 passage는 없다.
Turnaround Production Brief의 binary review contract는 후보가
`PASS`했고 Human Owner가 `APPROVE`했으며 byte-identical 승인본과
ADR-0018이 `OWNER-APPROVED / REGISTERED / LOCKED PRODUCTION SUPPORT`로
등록되었다. checklist는 `CHECKED`다. Final Pre-Implementation Review는
역사적으로 `REVISION REQUIRED`이고 TASK-0061 correction은
적용됐지만 Re-Audit이 필요하다. 다음 단일 작업은 `TASK-0062 —
Re-Audit Final Pre-Implementation Readiness after Gate and Baseline
Corrections`이며 첫 구현 티켓은
`docs/06_codex/IMP-0001-UNITY-REPOSITORY-BOOTSTRAP.md`로 세분화됐지만
concrete execution baseline SHA가 없고 실행은 차단되어 있다.
G4는 `NOT PASSED / NOT STARTED / NOT AUTHORIZED`, G5는
`NOT PASSED / REQUIRES G4 EVIDENCE`이고 Graybox와 Unity 구현 권한은
없다.

## Pre-G4 Authority Sequence

세 Freeze 개념을 구분한다.

1. Design Freeze Checklist `32 / 32`는 documentary evidence
   prerequisite다.
2. Pre-G4 Full Design Lock은 성공한 Re-Audit 뒤 별도 Owner task가
   최종화할 frozen documentation baseline이다. 현재
   `INCOMPLETE / RE-AUDIT REQUIRED`다.
3. G5 Production Freeze는 G4 evidence를 반영하는 post-G4 scope,
   asset, schedule과 risk approval다.

정확한 순서는 Pre-G4 documentary baseline과 Full Design Lock,
narrowly authorized infrastructure preparation, separately authorized
G4 Graybox, G4 evidence/decision, G5 Production Freeze, later Vertical
Slice production이다. Empty Bootstrap은 성공한 Re-Audit, 별도
Pre-G4 Full Design Lock 최종화와 명시적인 limited Execution
Authorization Relay 뒤에만 가능한 Phase-3-to-G4 infrastructure
preparation이다. Phase 4에 진입하거나 G4를 시작·통과하지 않고 G5도
통과하지 않는다.

future limited Gate state
`OPEN — IMP-0001 ONLY / EMPTY BOOTSTRAP`은 ticket-, baseline-,
scope-specific one-shot이다. gameplay, C#, Test, Scene, Prefab,
Graybox, 다른 ticket, G4/G5 passage 또는 상속 가능한 권한을 만들지
않고 완료·stop·baseline drift·dirty pre-state·다른 scope·unauthorized
Package·scope expansion·revocation에서 자동 만료한다. execution SHA는
티켓이 아닌 별도 Owner-approved Execution Authorization Relay만
공급한다. 현재 Gate는 `CLOSED`이고 Relay는 없다.

## Phase 4 — Graybox Validation

Status: `NOT STARTED / NOT AUTHORIZED`

`G4 DRAFT CONTENT CANDIDATE — NOT G1-APPROVED VALUES`

한 방, 한 동료, 적 3명, Light 2, Medium 2, Heavy 1.

Gate G4: 5분 재미와 재플레이 행동 통과.

실패 시 수치·카메라·배치·타이밍만 조정한다. 핵심 정체성을 바꾸려면 C3 Change Request가 필요하다.

## Phase 5 — Design Freeze

Status: `NOT PASSED / REQUIRES G4 EVIDENCE`

Graybox 결과를 반영해 문서 최종 동결.

`첫 구현 티켓 세분화`와 `CHR_00 Player Turnaround` 등록은 완료됐지만
IMP-0001 실행은 차단되어 있다. Design Freeze checklist의 미완료
항목은 `0`개다.

Turnaround Brief 승인만으로 checklist를 완료하지 않았다. 실제
`REVIEW` 후보가 계약을 통과하고 Human Owner가 승인했으며 승인본과
ADR-0018이 등록된 뒤에만 해당 항목을 체크했다. 이 완료는 G5 passage,
Full Design Lock, Graybox, Unity 또는 구현 권한이 아니다.

Gate G5: 본 제작 범위, 에셋 목록, 일정, 리스크 승인.

Final Pre-Implementation Review는 G4를 통과시키거나 Phase 4에 진입하지
않았다. G5도 통과하지 않았고 Implementation Gate는 계속 `CLOSED`다.

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
