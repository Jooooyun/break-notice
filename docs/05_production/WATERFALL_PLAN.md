# Waterfall Development Plan with Validation Gate

순수 폭포수는 게임의 재미를 문서만으로 증명할 수 없다는 문제가 있다. 본 프로젝트는 설계를 고정하되, 본 제작 전 단 한 번의 Graybox Validation Gate를 둔다.

## Current-State Source

이 문서는 waterfall 단계, 산출물과 Gate 순서를 정의한다. 현재 Phase,
Gate, Design Freeze, Pre-G4 Full Design Lock, Graybox, provisioning,
Package, Relay, ticket readiness와 Current Next Authorization Boundary의
값은 `docs/00_project/DESIGN_STATUS.md`만 따른다. 이 문서는 독립적인
live-state board 또는 next-task authority가 아니다.

## Phase 0 — Charter

산출물: Project Charter, 제약, 성공 기준.

Gate G0: 프로젝트 목적과 1인 범위 승인.

## Phase 1 — Requirements

산출물: Product/NFR, RTM, Non-goals.

Gate G1: 모든 MUST 요구사항에 ID와 검증 방식 존재.

Passage history: Owner-approved G1 Baseline passage가 기록되었다.
현재 Gate 값은 DESIGN_STATUS를 따른다.

## Phase 2 — Game & Art Design

산출물: GDD, Gameplay Spec, Ally/Physics, Art Direction, Canon 이미지.

Gate G2: 핵심 루프, 카메라, 장비, 동료, 파괴 예산의 모순 제거.

Passage history: Owner-approved G2 contracts, exact Prototype content,
remediation과 successful Re-Audit을 근거로 passage가 기록되었다.
현재 Gate 값은 DESIGN_STATUS를 따른다.

## Phase 3 — Technical Design

산출물: 아키텍처, 데이터, 성능 예산, 테스트 설계, Codex 프로토콜.

Gate G3: 모든 핵심 요구사항이 구현 책임과 테스트에 매핑.

Passage history: Owner-approved G3 baseline, corrections와 successful
Re-Audit을 근거로 passage가 기록되었다. 정확한 기술 책임과 contract는
`docs/04_technical/G3_TECHNICAL_BASELINE.md`, passage evidence는
`docs/05_production/G3_TECHNICAL_BASELINE_REAUDIT.md`, frozen Pre-G4
record는 `docs/05_production/PRE_G4_FULL_DESIGN_LOCK.md`를 따른다.
과거 Audit/Re-Audit과 Relay 결과는 immutable point-in-time evidence다.
현재 Gate, provisioning, Relay와 ticket readiness 값은 DESIGN_STATUS를
따른다.

## Pre-G4 Authority Sequence

세 Freeze 개념을 구분한다.

1. Design Freeze Checklist `32 / 32`는 documentary evidence
   prerequisite다.
2. Pre-G4 Full Design Lock은 성공한 Re-Audit 뒤 별도 Owner task가
   고정한 frozen documentation baseline이며 정확한 기록은
   `PRE_G4_FULL_DESIGN_LOCK.md`다. 현재 Lock 값은 DESIGN_STATUS를
   따른다.
3. G5 Production Freeze는 G4 evidence를 반영하는 post-G4 scope,
   asset, schedule과 risk approval다.

정확한 순서는 Pre-G4 documentary baseline과 Full Design Lock,
narrowly authorized infrastructure preparation, separately authorized
G4 Graybox, G4 evidence/decision, G5 Production Freeze, later Vertical
Slice production이다. Empty Bootstrap은 성공한 Re-Audit, 별도 Pre-G4
Full Design Lock과 `EAR-IMP-0001-002` 아래 완료된 Phase-3-to-G4
infrastructure preparation이다. IMP-0001 완료는 Phase 4 진입, G4
시작·통과 또는 G5 passage가 아니다.

ADR-0019의 `OPEN — IMP-0001 ONLY / EMPTY BOOTSTRAP`은 완료된
IMP-0001의 역사적 one-shot state로 보존한다. ADR-0022의
post-Bootstrap future open-state format은 다음과 같다.

`OPEN — <APPROVED TICKET ID> ONLY / <EXACT APPROVED SCOPE>`

모든 future open state는 ticket-, baseline-, branch-, scope-specific,
one-shot, non-inheritable, non-reusable이며 ticket이 아닌 별도
Owner-approved Execution Authorization Relay만 공급한다. 완료·stop·
baseline drift·dirty pre-state·다른 scope·unauthorized dependency·scope
expansion·revocation에서 자동 만료한다. 첫 future instance
`OPEN — IMP-0002 ONLY / RUNTIME IDENTITY AND STATE FOUNDATION`은
정의됐지만 열리지 않았다. 현재 Gate, Relay, concrete SHA와 ticket
readiness 값은 DESIGN_STATUS를 따른다.

CR-0001 full-game target과 TASK-0068 Player-Failure addendum 뒤에도
historical G1/G2/G3 passage는 유지된다. Targeted C3 Re-Audit과 G4-A
Detailed Evidence Protocol 교정은 완료·승인됐지만 evidence는
`NOT COLLECTED`, execution은 `NOT STARTED / NOT AUTHORIZED`다.
CR-0002와 ADR-0022는 G4-A의 bounded per-ticket foundation entry만
정의하고 Story, Campaign, Map, Level, gameplay, external asset,
Graybox 또는 G4 passage를 승인하지 않는다. IMP-0002는 locked ticket
decomposition이며 별도 Relay 전까지 실행할 수 없다.

## Phase 4 — Graybox Validation

Current phase state: DESIGN_STATUS를 따른다.

G4-A의 active Player Failure evidence는 비살상 presentation, current
encounter의 authored entry restart와 반복 가능한 clean baseline에만
한정한다. Campaign-level Story, rescue, As-Built/map, shortcut와 save
persistence는 FGD-001의 later target이며 active G4-A PASS evidence가
아니다. detailed observer/session/variant/reporting protocol은
`docs/07_quality/G4A_DETAILED_EVIDENCE_PROTOCOL.md`에서
`OWNER-APPROVED / CORRECTED / READY FOR FUTURE EXECUTION AUTHORIZATION`지만
실행·evidence collection·G4 passage는 별도 권한 전까지 금지한다.

`G4 DRAFT CONTENT CANDIDATE — NOT G1-APPROVED VALUES`

한 방, 한 동료, 적 3명, Light 2, Medium 2, Heavy 1.

Gate G4: 5분 재미와 재플레이 행동 통과.

실패 시 수치·카메라·배치·타이밍만 조정한다. 핵심 정체성을 바꾸려면 C3 Change Request가 필요하다.

## Phase 5 — Design Freeze

Current phase state: DESIGN_STATUS를 따른다.

Graybox 결과를 반영해 문서 최종 동결.

Design Freeze Checklist 완료는 documentary prerequisite일 뿐 G5
passage, Graybox, Unity 또는 구현 권한이 아니다. exact checklist
evidence는 `DESIGN_FREEZE_CHECKLIST.md`, frozen Pre-G4 baseline은
`PRE_G4_FULL_DESIGN_LOCK.md`, 현재 상태는 DESIGN_STATUS를 따른다.

Gate G5: 본 제작 범위, 에셋 목록, 일정, 리스크 승인.

Final Pre-Implementation Review는 그 자체로 G4 passage, Phase 4 진입,
G5 passage 또는 Implementation Gate 개방 권한을 만들지 않는다.

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
