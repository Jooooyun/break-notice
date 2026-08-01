# Design Status Board

현재 단계: `PRE-PRODUCTION / DESIGN FREEZE COMPLETION`

G1 Requirements: `PASSED / C3 PRODUCT-DIRECTION CHANGE APPLIED`

G2 Design:
`PASSED / EXISTING CORE CONTRACTS PRESERVED / POLICY APPLICABILITY DEFERRED`

G3 Technical: `PASSED / OWNER-APPROVED BASELINE`

G3 Technical Baseline:
`OWNER-APPROVED / PASSED`

G3 Technical Decisions:
`OWNER-APPROVED / BASELINE APPLIED`

G3 Causal Result-Family Alignment:
`OWNER-APPROVED / CORRECTED`

G3 Technical Final Audit:
`REVISION REQUIRED / HISTORICAL BASELINE AUDIT`

G3 Residual Active Status and Package-Authority Correction:
`OWNER-APPROVED / COMPLETE`

G3 Technical Final Audit Remediation:
`COMPLETE / VERIFIED BY RE-AUDIT`

G3 Technical Baseline Re-Audit:
`READY FOR G3 GATE DECISION — CONSUMED BY G3 PASSAGE`

G3 Gate Passage Source:
`docs/05_production/G3_TECHNICAL_BASELINE_REAUDIT.md` at
`9e1487e85be5a54ba0824c209deabcf738e1abf0`

G4 Graybox: `NOT PASSED / NOT STARTED / NOT AUTHORIZED`

G5 Production Freeze:
`NOT PASSED / REQUIRES G4 EVIDENCE`

G3 Technical Checklist: `6 / 6`

Design Freeze: `32 / 32`

Design Freeze Completion:
`COMPLETE / DOCUMENTARY PREREQUISITE ONLY`

Final Pre-Implementation Review:
`REVISION REQUIRED / docs/05_production/FINAL_PRE_IMPLEMENTATION_REVIEW.md`

Final Pre-Implementation Review Remediation:
`TASK-0061 CORRECTIONS APPLIED / VERIFIED BY RE-AUDIT`

Final Pre-Implementation Re-Audit:
`READY FOR LIMITED IMPLEMENTATION GATE DECISION — IMP-0001 ONLY`

Final Pre-Implementation Re-Audit Record:
`docs/05_production/FINAL_PRE_IMPLEMENTATION_REAUDIT.md`

Pre-G4 Full Design Lock Semantics:
`OWNER-APPROVED / DEFINED`

Pre-G4 Full Design Lock:
`OWNER-APPROVED / LOCKED / EXTENDED BY CR-0001 AND ADR-0021`

Pre-G4 Full Design Lock Record:
`docs/05_production/PRE_G4_FULL_DESIGN_LOCK.md`

Pre-G4 Full Design Lock ADR:
`docs/09_adrs/ADR-0020-PRE-G4-FULL-DESIGN-LOCK.md`

Frozen Design-Source Baseline SHA:
`f638be24c6895a6969083626fa3f94228b533ec6`

Intended Unity Toolchain:
`OWNER-APPROVED / LOCKED INTENT`

Unity Toolchain Status Record:
`docs/05_production/UNITY_TOOLCHAIN_INTENT_AND_PROVISIONING_STATUS.md`

Unity Toolchain Provisioning:
`OWNER-DEFERRED`

Unity Editor Installation:
`NOT VERIFIED / OWNER-DEFERRED`

Package Materialization:
`NOT STARTED / NOT AUTHORIZED`

Package Resolution:
`NOT VERIFIED`

Historical TASK-0064 Result:
`STOP CONDITION / NO CHANGE`

Limited Implementation Gate Semantics:
`OWNER-APPROVED / DEFINED`

EAR-IMP-0001-001:
`EXPIRED / UNUSED`

Active Execution Authorization Relay:
`ABSENT / NEW RELAY NOT AUTHORIZED`

Active Relay: `ABSENT`

Destruction Budget and Implementation Rules:
`OWNER-APPROVED / LOCKED`

Office Core Kit:
`OWNER-APPROVED / LOCKED`

CHR_00 Player Turnaround Brief:
`OWNER-APPROVED / CONSUMED BY APPROVED TURNAROUND`

CHR_00 Player Turnaround Production Support:
`OWNER-APPROVED / REGISTERED / LOCKED PRODUCTION SUPPORT`

First Implementation Ticket:
`IMP-0001 / OWNER-APPROVED TICKET DECOMPOSITION / EXECUTION BLOCKED / PRE-G4 FULL DESIGN LOCK SATISFIED / TOOLCHAIN PROVISIONING OWNER-DEFERRED`

IMP-0001 Mandatory Precondition Progress:
`6 / 10`

Production Visual Lock sequence: `COMPLETE`

Primary Production Visual Count: `8`

Approved Production Support Count: `1`

Full Design Lock:
`OWNER-APPROVED / LOCKED`

Graybox: `NOT STARTED`

Phase label review: `COMPLETED / OWNER-APPROVED`

Implementation Gate: `CLOSED`

Current Next Authorization Boundary:
`OWNER EXPLICIT UNITY TOOLCHAIN PROVISIONING INSTRUCTION REQUIRED`

## Full-Game Direction — Active C3 Authority

- CR-0001: `OWNER-APPROVED / APPLIED`
- ADR-0021: `ACCEPTED`
- FGD-001: `OWNER-APPROVED / LOCKED FULL-GAME TARGET`
- Working Title: `DESTROY YOUR COMPANY: ZERO DEFECT`
- Public Marketing Title:
  `NOT FINALLY LOCKED / POSITIONING TRACKER OPEN`
- Marketing Tracker: the working title may be mistaken for a
  complete-destruction sandbox and must be resolved before final Public
  Marketing Title Lock; it does not promise arbitrary or complete destruction.
- Primary Product Direction:
  `SINGLE-CAMPAIGN PHYSICS ACTION ADVENTURE`
- Runtime Target: `3.5–5 HOURS / LONG-TERM PRODUCT TARGET`
- Worldline: `ZERO DEFECT / HANDOVER`
- Full Story/Campaign/Map/Level:
  `OWNER-APPROVED / LOCKED TARGET / THREE ACTS / LEVELS 0–6 / HYBRID VERTICAL LOOP`
- As-Built Map Pillar: `OWNER-APPROVED FULL-GAME TARGET`
- Central Atrium Finale and Insolvency Ending:
  `OWNER-APPROVED / LOCKED TARGET`
- Roguelite: `DEFERRED REPLAY LAYER`
- Policy Violation:
  `EXISTING LOGICAL CONTRACT PRESERVED / PRIMARY-CAMPAIGN APPLICABILITY DEFERRED`
- Player Failure / PR-F-025:
  `OWNER-APPROVED MINIMUM G4-A DIRECTION`
- G4-A Brief: `OWNER-APPROVED / NOT STARTED / NOT AUTHORIZED`
- G4-A Role: `FIRST IMPLEMENTATION EVIDENCE GATE`
- Implementation Authority From C3 Registration: `NONE`

FGD-001은 intended full game을 정의하지만 Unity, package, code, Scene,
Prefab, Test, Build, asset, Graybox, G4 passage, Gate opening 또는 Relay
발급 권한을 만들지 않는다.

## Current-State Authority Policy

이 문서는 BREAK NOTICE 저장소의 `SOLE MUTABLE LIVE-STATE AUTHORITY`다.
현재 Phase, Gate, Lock, checklist progress, Graybox, provisioning,
Package resolution, ticket readiness, Relay, execution authorization와
Current Next Authorization Boundary의 변경 가능한 현재값은 이 문서에서만
직접 관리한다.

역할 분리는 다음과 같다.

- `MANIFEST.json`: `DERIVED MACHINE-READABLE MIRROR / NOT INDEPENDENT
  AUTHORITY`
- `AGENTS.md`: `MINIMUM SAFETY-CRITICAL MIRROR`
- `README.md`: `NON-AUTHORITATIVE SUMMARY`
- 승인된 contract, specification, content lock과 budget 문서:
  각 문서가 정의하는 substantive authority
- Accepted ADR: `IMMUTABLE DECISION EVIDENCE`
- Audit/Re-Audit: `IMMUTABLE POINT-IN-TIME EVIDENCE`
- `PRE_G4_FULL_DESIGN_LOCK.md`: `IMMUTABLE FROZEN BASELINE RECORD`
- 승인 Visual Spec의 Gate·비구현 문구:
  `ARTIFACT-LOCAL SAFETY BOUNDARY`
- `Game/README.md`: placeholder-local 설명
- `CHANGELOG.md`: historical change record

현재값이 서로 다르면 live-state에는 이 문서가 우선한다. mechanics,
content, technical contract와 Visual Canon의 실질 내용은 해당 승인
contract, specification, lock과 ADR이 우선한다. Human Owner의 최신
직접 지시는 모든 저장소 문서보다 우선하며, 변경은 승인된 Change
Control을 거쳐 이 보드와 파생 mirror에 동기화한다.

## G4 Evidence-First Documentation Brake

상태: `ACTIVE FOR ADDITIONAL SPECULATIVE DESIGN`

Human Owner가 승인한 첫 G4 evidence report가 존재하고 Human Owner가
명시적으로 이 brake를 해제하거나 수정하기 전까지
`NEW SPECULATIVE DESIGN DOCUMENT CREATION IS FROZEN`이다.

다음 신규 문서는 금지한다.

- World expansion
- Narrative expansion
- Campaign
- Map
- Level
- Encounter content
- exact Vertical Slice content
- Boss
- enemy roster expansion
- Upgrade pool
- Meta progression
- additional Visual concept exploration
- new system architecture not required by the next authorized
  implementation or G4 evidence task

허용 예외는 다음 아홉 종류로 한정한다.

1. live-state or authority-drift correction
2. document deletion, reduction or consolidation
3. Risk Register maintenance
4. Human Owner-approved Change Control
5. one exact G4 Graybox Validation Brief
6. G4 evidence capture and reporting
7. bounded implementation tickets required by the locked Prototype
8. defects blocking an already approved bounded ticket
9. mandatory repository or toolchain evidence requested by the Human
   Owner

이 brake는 Unity, implementation, G4, Graybox, Gate opening 또는 새
gameplay scope를 승인하지 않는다. 종료 조건은
`Human Owner-authorized G4 evidence exists and the Human Owner
explicitly lifts or revises the brake`다.

CR-0001에 따른 FGD-001 등록은 이번 brake의
`CONSUMED HUMAN OWNER-APPROVED CHANGE-CONTROL EXCEPTION`이다. 이제
추가 speculative Story/Campaign/Map/Level 확장과 approved full
campaign의 구현은 G4-A evidence/PASS 전까지 차단된다.

## Active Risk Disposition — R-007

- Risk: `R-007 — 설계 과잉`
- Status: `ACTIVE / TRIGGERED`
- Evidence: `Design Freeze and Pre-G4 Full Design Lock were completed
  while Game remains a placeholder and G4 has not started; FGD-001 was
  registered through C3 Change Control without implementation`
- Immediate response: `G4 EVIDENCE-FIRST DOCUMENTATION BRAKE`
- Owner: `DESIGN AUTHORITY`
- Owning Gate: `PRE-G4 / G4`
- Closure condition: `Human Owner-authorized G4 evidence exists and the
  Human Owner explicitly lifts or revises the brake`

## Pre-G4 Authority and Sequencing Status

| Concept | Meaning | Current state |
|---|---|---|
| Design Freeze Checklist | `32 / 32` documentary evidence prerequisite; no Lock or Gate passage | COMPLETE |
| Pre-G4 Full Design Lock | Owner-approved frozen documentation baseline for empty Bootstrap and later separate G4 authorization; no gameplay, performance, G4/G5/G6 claim | OWNER-APPROVED / LOCKED |
| G5 Production Freeze | post-G4 approval of production scope, asset list, schedule and risk using G4 evidence | NOT PASSED / REQUIRES G4 EVIDENCE |

정확한 순서는 Pre-G4 documentary baseline과 Full Design Lock,
narrowly authorized infrastructure preparation, separately authorized
G4 Graybox, G4 evidence/decision, G5 Production Freeze, later Vertical
Slice production이다.

Limited Gate의 유일한 future open state는
`OPEN — IMP-0001 ONLY / EMPTY BOOTSTRAP`이다. 이 state는 ticket,
baseline과 scope가 고정된 one-shot이며 gameplay, C#, Test, Scene,
Prefab, Graybox, 다른 ticket, G4/G5 passage 또는 상속 가능한 권한을
만들지 않는다. 완료, stop, baseline drift, dirty tracked/staged
pre-state, 다른 ticket/scope, unauthorized Package, scope expansion
또는 revocation에서 자동 만료한다. 현재 state는 `CLOSED`다.

IMP-0001 실행 기준선은 티켓에 저장하지 않는다. 별도
Owner-approved Execution Authorization Relay가 이미 push된 완전한
immutable SHA, `main`, clean local/origin equality, exact Gate state,
allowed/forbidden files, operations, validations, commit/push 권한과
one-shot expiration을 제공해야 한다. 역사적 TASK-0064의
`EAR-IMP-0001-001`은 exact Editor 부재 Stop Condition 뒤
`EXPIRED / UNUSED`가 됐고 Gate는 활성화되지 않았다. 현재 active
Relay와 concrete SHA는 `ABSENT`이며 새 Relay와 provisioning은 Human
Owner의 새 직접 지시 전까지 승인되지 않는다.

| 영역 | 상태 | 다음 산출물 | Gate |
|---|---|---|---|
| G1 Requirements Baseline | OWNER-APPROVED / PASSED | G2 Design baseline | G1 PASSED |
| 제품 비전 | OWNER-APPROVED G1 BASELINE | 상세 설계 정렬 | G1 PASSED |
| G2 Design Baseline Promotion | OWNER-APPROVED / PASSED | G3 Technical Baseline Preflight Audit | G2 PASSED |
| Logical Input Action Contract | OWNER-APPROVED / G2 PARTIAL | Interaction State and Conflict Contract와 함께 유지 | G2 |
| Interaction State and Conflict Contract | OWNER-APPROVED / G2 PARTIAL | Weight-Class Contract와 함께 유지 | G2 |
| Weight-Class Functional Data Range Contract | OWNER-APPROVED / G2 PARTIAL | Momentum/Pip Contract와 함께 유지 | G2 |
| Momentum/Pip Starting Profile | OWNER-APPROVED / G2 STARTING VALUES | Causal Chain Contract와 함께 유지 | G2 |
| Causal Chain Propagation and Termination Contract | OWNER-APPROVED / G2 PARTIAL | Ally Relay and Suppressed Contract와 함께 유지 | G2 |
| 핵심 전투 동사 | REQUIREMENTS OWNER-APPROVED / DESIGN DRAFT | 상세 게임플레이 설계 | G2 |
| Ally Relay and Suppressed Contract | OWNER-APPROVED / G2 PARTIAL | Prototype Content Lock와 함께 유지 | G2 |
| Prototype Enemy, Room, and Object Content Lock | OWNER-APPROVED / G2 GAMEPLAY SET COMPLETE | Final Audit에서 검증 완료 / 현 Lock 유지 | G2 |
| G2 Design Baseline Final Audit | REVISION REQUIRED / HISTORICAL | 후속 Re-Audit에서 차단 해소 확인 | G2 |
| G2 Stale Authority and RTM Source/Trace Honesty Correction | OWNER-APPROVED / COMPLETE / VERIFIED BY RE-AUDIT | G2 passage에 반영 완료 | G2 PASSED |
| Policy Violation Selection and Run-Application Contract | OWNER-APPROVED / G2 PARTIAL | G3 기술 매핑에서 유지 | G2 PASSED |
| G2 Design Baseline Re-Audit | READY FOR G2 GATE DECISION — CONSUMED BY G2 PASSAGE | G3 Technical Baseline Preflight Audit | G2 PASSED |
| G3 Technical Baseline Preflight Audit | READY FOR OWNER G3 TECHNICAL DECISIONS / CONSUMED BY OWNER DECISION | G3 Technical Baseline | G3 |
| G3 Technical Baseline Decisions | OWNER-APPROVED / BASELINE APPLIED | G3 passage 기록 완료 | G3 PASSED |
| G3 Causal Result-Family Alignment | OWNER-APPROVED / CORRECTED | 현 taxonomy 유지 | G3 PASSED |
| G3 Technical Baseline Final Audit | REVISION REQUIRED / HISTORICAL BASELINE AUDIT | Re-Audit에서 blocker closure 검증 완료 | HISTORICAL / G3 PASSED |
| G3 Residual Active Status and Package-Authority Correction | OWNER-APPROVED / COMPLETE | COMPLETE / VERIFIED BY RE-AUDIT | G3 PASSED |
| G3 Technical Baseline Re-Audit | READY FOR G3 GATE DECISION — CONSUMED BY G3 PASSAGE | G3 passage 근거로 소비됨 | G3 PASSED |
| 화물 스트랩 | VISUAL LOCKED / FUNCTIONAL DRAFT | 상세 게임플레이 설계 | G2 |
| CHR_00 Player Identity | LOCKED | 없음 | G2 |
| CHR_01 First Ally Identity | LOCKED | 없음 | G2 |
| 카메라 | VISUAL LOCKED / FUNCTIONAL DRAFT | 상세 카메라 설계 | G2 |
| Chair Redirect Core Loop | VISUAL LOCKED / FUNCTIONAL DRAFT | 상세 Chair Redirect 설계 | G2 |
| PGT_03 Chain Reaction Aftermath | VISUAL LOCKED / FUNCTIONAL DRAFT | 상세 Chain·파괴 결과 설계 | G2 |
| LGT_01 Night Archive Lighting | VISUAL LOCKED / TECHNICAL DRAFT | 정확한 조명·렌더링 설계 | G2 |
| FIN_01 Conference Table Team Finisher | VISUAL LOCKED / FEATURE COULD / FUNCTIONAL DRAFT | 제품 범위 결정 전 구현 금지 | G2 |
| 파괴 예산과 구현 규칙 | OWNER-APPROVED / LOCKED | 현 Lock 유지 | DESIGN FREEZE |
| Office Core Kit asset-family and reuse policy | OWNER-APPROVED / LOCKED | 현 Lock 유지 | DESIGN FREEZE |
| CHR_00 Player Turnaround Production Brief | OWNER-APPROVED / CONSUMED BY APPROVED TURNAROUND | 승인 계약 유지 | DESIGN FREEZE / CHECKED |
| CHR_00 Player Turnaround Production Support | OWNER-APPROVED / REGISTERED / LOCKED PRODUCTION SUPPORT | 현 Production Support 권위 유지 | DESIGN FREEZE / CHECKED |
| First Implementation Ticket | IMP-0001 / EXECUTION BLOCKED / 6 OF 10 / TOOLCHAIN PROVISIONING OWNER-DEFERRED | exact Gate, Relay, SHA와 Editor evidence 전 실행 금지 | DESIGN FREEZE / CHECKED |
| Final Pre-Implementation Review | REVISION REQUIRED — historical decision preserved | TASK-0061 CORRECTIONS APPLIED / VERIFIED BY RE-AUDIT | HISTORICAL |
| Final Pre-Implementation Re-Audit | READY FOR LIMITED IMPLEMENTATION GATE DECISION — IMP-0001 ONLY | consumed by TASK-0063 Pre-G4 Lock decision | PRE-G4 / GATE CLOSED |
| Pre-G4 Full Design Lock | OWNER-APPROVED / LOCKED at design-source baseline `f638be24c6895a6969083626fa3f94228b533ec6` | maintain Lock through Change Control | PRE-G4 |
| Limited Implementation Gate | SEMANTICS OWNER-APPROVED / DEFINED; EAR-IMP-0001-001 EXPIRED / UNUSED; ACTIVE RELAY ABSENT; CURRENT STATE CLOSED | Human Owner의 새 provisioning 지시 전 대기 | CLOSED |
| Vertical Slice enemy·room 확장 | DRAFT | G2 Final Audit 이후 별도 범위 결정 | G2 |
| G3 Technical Baseline | OWNER-APPROVED / PASSED | Design Freeze completion | G3 PASSED |
| 기술 아키텍처 | OWNER-APPROVED G3 DIRECTION / IMPLEMENTATION DRAFT | Design Freeze completion | G3 PASSED |
| Graybox | NOT STARTED / NOT AUTHORIZED | G4 별도 승인 후 | G4 NOT PASSED |
| 본 제작 | PENDING | Implementation Gate 개방 후 | G5 |

## Production Visual Lock Sequence

1. `GEAR_01 — Cargo Strapping System` — `LOCKED`
2. `CHR_01 — First Ally Identity` — `LOCKED`
3. `PGT_01 — Gameplay Camera` — `LOCKED`
4. `PGT_02 — Chair Redirect Core Loop` — `LOCKED`
5. `PGT_03 — Chain Reaction Aftermath` — `LOCKED`
6. `LGT_01 — Night Archive Lighting` — `LOCKED`
7. `FIN_01 — Conference Table Team Finisher` — `LOCKED`

현재 Production Visual Lock sequence는 완료되었다. 다음 Production Visual 대상은 승인되지 않았다.

FIN_01은 PR-F-022 `COULD` 기능의 Visual-only conditional direction이며 승인된 Vertical Slice 또는 최종 제품 범위에 포함되지 않는다. Pre-G4 Full Design Lock은 `OWNER-APPROVED / LOCKED`이고 Implementation Gate는 `CLOSED`다.

`CHR_00 Player Turnaround`의 Production Brief와 binary review contract는
`OWNER-APPROVED / CONSUMED BY APPROVED TURNAROUND`다. 유일한 Identity
권위와 정확한 다섯 뷰, continuity, 최대 세 callout 및 검토 기준은
`docs/03_art/PLAYER_TURNAROUND_PRODUCTION_BRIEF.md`를 따른다. 검토 소스
`references/exploration/CHR_00_Player_Turnaround_v001_REVIEW.png`는
계약을 `PASS`했고 Human Owner가 `APPROVE`했으며 byte-identical 승인본
`references/approved/characters/CHR_00_Player_Turnaround_v001_FINAL.png`가
`LOCKED PRODUCTION SUPPORT`로 등록되었다. 주 Identity 권위는 변경되지
않고 3D, Unity, Graybox 또는 구현 권한은 없다.

G1 요구사항·NFR의 정확한 문구와 disposition, 기술 중립적 검증
의도와 Applicability/Priority 경계는 Owner-approved이며 G1
Requirements Gate는 `PASSED`다. 상세 게임플레이 설계, G2
Light/Medium/Heavy functional envelope과 class-based eligibility는
Owner-approved다. Momentum/Pip의 threshold, capacity, reward starting
profile, per-origin cap, HeavyAction cost와 resource lifecycle도
Owner-approved G2 starting values다. Causal Origin, Direct/Downstream
classification, 최대 Depth `4`, Branch·duplicate·cycle·multiple-origin
경계와 termination·Reset 규칙도 Owner-approved G2 logical design이다.
일반 전투의 CHR_01 한 명, Prototype 고정 support position,
Light/Medium physical supply, Vertical Slice Relay Position 전환,
player agency와 조건부 `VERTICAL SLICE / SHOULD` Suppressed 계약도
Owner-approved G2 logical design이다. `PRT-RM-001` 한 room, CHR_00·
CHR_01·Compliance Runner 두 명, `PRT-RP-001`, 네 object definition·
일곱 instance와 검증 affordance도 Owner-approved다. G2 Gameplay
checklist는 `7 / 7`이며, 교정된 기준선의 Re-Audit과 Owner 승인에
따라 G2 Design Gate는 `PASSED`다.
PR-F-018의 run당 한 Selection Opportunity, 두 Candidate, 단일 선택,
다음 encounter 적용, one Active Policy, room transition·Reset 보존과
run-end clearing의 logical G2 blocker는 Owner-approved
`Policy Violation Selection and Run-Application Contract`로 해소되었다.
실제 Candidate effect와 final pool은 G5 Draft다.
`G2 Design Baseline Final Audit` 판정은 계속 `REVISION REQUIRED`이며,
이는 기준선 `a0cf9b15d120b0842e2ddcfff1be697863581533`의 역사적 감사
결과다. stale non-progression Master GDD authority prose,
PR-F-019·PR-F-024 Trace Status와 NFR-U-001~005 Source role 교정은
Owner-approved 상태로 완료되었다. 새 기준선의
`G2 Design Baseline Re-Audit` 결과 `READY FOR G2 GATE DECISION`은
Owner-approved G2 passage의 근거로 소비되었다. exact
mass·size·speed·force·damage·Contact Freeze, Behavior Tag details,
final Momentum/Pip balance, Chain runtime lineage·threshold·presentation,
break value, final enemy identity·room geometry·object art, Relay Position
coordinates·route, supply ranking·timing, Suppressed
threshold·duration·recovery,
Acceptance Test의 정확한 수치·임계값, 카메라 기능,
조명·렌더링 구현, FIN_01 제품 편입, destruction hard cap을 제외한
exact class·payload·schema·성능 tuning, Unity Bootstrap·구현
ticket과 Vertical Slice의 정확한 콘텐츠는 `DRAFT`, `COULD`,
`DEFERRED` 또는 미승인이다. 승인된 여덟
Visual의 문서화된 Canonical Elements만
`LOCKED`이며 현재 Phase label은 Owner가 승인한
`PRE-PRODUCTION / DESIGN FREEZE COMPLETION`이다. G3 Technical은
`PASSED / OWNER-APPROVED BASELINE`이다. G3 Technical Draft와 빈
Unity 예정 구조를 기준선에 대조한 Preflight Audit 결과는
Owner의 `DEC-G3-001`~`DEC-G3-013: A` 승인으로 소비되었다.
기술 책임·dependency·state/event/causal/physics/Strap/budget/Reset/
harness 정책과 exact documentation lock
(`6000.3.20f1`, URP `17.3.0`, Input System `1.20.0`, Cinemachine
`3.1.7`, Test Framework `1.6.0`)은 Owner-approved G3 baseline이다.
AI Navigation은 Prototype에서 Deferred다. Prototype의 모든 파괴
category cap `0`, Later-Slice의 bounded room·event·fragment·
small-material hard cap, pre-authored state, preallocated pool,
atomic admission, causal-carrier 제한, Reset·fault observability와
runtime fracture·recursive destruction 금지는 Owner-approved다.
정확한 권위는
`docs/04_technical/DESTRUCTION_BUDGET_AND_IMPLEMENTATION_RULES.md`와
`docs/09_adrs/ADR-0016-BOUNDED-DESTRUCTION-BUDGET.md`를 따른다.
Structural Feature는 `VERTICAL SLICE / SHOULD / PARTIAL`이며 별도 G5
승인 없이는 cap `0`이다. Office Core Kit은 정확한 Prototype family
`16`개(architecture/fixed fixture `7`, furniture/gameplay `5`,
decorative/dressing `4`)와 Later-Slice candidate `8`개, interactive
definition `3`, recognition-only definition `1`, fixed/decorative
family `12`의 asset-family 및 reuse policy로
`OWNER-APPROVED / LOCKED`다. Prototype instance `7`과 destruction
`0`은 유지되고, 실제 model·texture·material·layout·Unity asset
creation은 `NOT STARTED / NOT AUTHORIZED`다. LGT_01은 Records
Archive-local `COULD`, FIN_01은 `OUT / COULD`를 유지한다. Technical
checklist는 `6 / 6`, Design Freeze는 `32 / 32`와
`COMPLETE / DOCUMENTARY PREREQUISITE ONLY`다. 실제 Unity project와 package
file은 생성되지 않았고 exact tuning, implementation class와 pass
threshold는 Draft다.
G3 Causal Result Family는 Owner-approved
G2 계약의 정확한 네 family로 교정되었으며 Structural Feature는 별도
optional design role로 유지된다. Final Audit은 active Phase와
AI Navigation package authority drift를 발견해 `REVISION REQUIRED`로
판정한 역사적 기준선 감사다. 두 잔여 권한 오류의 교정은
`OWNER-APPROVED / COMPLETE`, remediation은
`COMPLETE / VERIFIED BY RE-AUDIT`다. 교정 기준선 Re-Audit의
`READY FOR G3 GATE DECISION` 판정은 기준선
`9e1487e85be5a54ba0824c209deabcf738e1abf0`에서 Owner-approved G3
passage의 근거로 소비되었고 Re-Audit 문서 자체는 변경하지 않는다.
G4는 `NOT PASSED / NOT STARTED / NOT AUTHORIZED`, G5는 `NOT PASSED /
REQUIRES G4 EVIDENCE`, Pre-G4 Full Design Lock은
`OWNER-APPROVED / LOCKED`, Graybox는 `NOT STARTED`, Implementation
Gate는 `CLOSED`다. Final Review는 역사적으로 `REVISION REQUIRED`이고
TASK-0061 baseline binding과 limited Gate sequencing correction은
TASK-0062 Re-Audit에서 `48 / 48 PASS`로 검증됐다. 첫 구현 티켓은
`docs/06_codex/IMP-0001-UNITY-REPOSITORY-BOOTSTRAP.md`로
세분화됐지만 concrete execution baseline SHA는 티켓에 저장하지 않고
별도 미래 Execution Authorization Relay만 공급할 수 있으며 실행은
승인되지 않았다. `EAR-IMP-0001-001`은 `EXPIRED / UNUSED`, active
Relay와 concrete execution SHA는 없고 IMP-0001 precondition은
`6 / 10`이다. toolchain provisioning은 `OWNER-DEFERRED`이며 다음
권한 경계는
`OWNER EXPLICIT UNITY TOOLCHAIN PROVISIONING INSTRUCTION REQUIRED`다.
남은 Design Freeze 항목은 `0`개다. Bootstrap, Graybox와 구현 권한은
아직 없고 IMP-0001 실행은 계속 차단된다.
