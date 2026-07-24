# Design Status Board

현재 단계: `PRE-PRODUCTION / G3 TECHNICAL BASELINE`

G1 Requirements: `PASSED`

G2 Design: `PASSED / OWNER-APPROVED BASELINE`

G3 Technical: `NOT PASSED / CURRENT BASELINE WORK`

G3 Technical Decisions:
`OWNER-APPROVED / BASELINE APPLIED / FINAL AUDIT PENDING`

G3 Causal Result-Family Alignment:
`OWNER-APPROVED / CORRECTED`

G3 Technical Checklist: `6 / 6`

Design Freeze: `28 / 32`

Production Visual Lock sequence: `COMPLETE`

Full Design Lock: `INCOMPLETE`

Graybox: `NOT STARTED`

Phase label review: `COMPLETED / OWNER-APPROVED`

Implementation Gate: `CLOSED`

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
| G3 Technical Baseline Decisions | OWNER-APPROVED / BASELINE APPLIED / FINAL AUDIT PENDING | TASK-0052 — Correct Residual G3 Active Status and Package-Authority Drift | G3 NOT PASSED |
| G3 Causal Result-Family Alignment | OWNER-APPROVED / CORRECTED | TASK-0052 — Correct Residual G3 Active Status and Package-Authority Drift | G3 NOT PASSED |
| G3 Technical Baseline Final Audit | REVISION REQUIRED | TASK-0052 — Correct Residual G3 Active Status and Package-Authority Drift | G3 |
| 화물 스트랩 | VISUAL LOCKED / FUNCTIONAL DRAFT | 상세 게임플레이 설계 | G2 |
| CHR_00 Player Identity | LOCKED | 없음 | G2 |
| CHR_01 First Ally Identity | LOCKED | 없음 | G2 |
| 카메라 | VISUAL LOCKED / FUNCTIONAL DRAFT | 상세 카메라 설계 | G2 |
| Chair Redirect Core Loop | VISUAL LOCKED / FUNCTIONAL DRAFT | 상세 Chair Redirect 설계 | G2 |
| PGT_03 Chain Reaction Aftermath | VISUAL LOCKED / FUNCTIONAL DRAFT | 상세 Chain·파괴 결과 설계 | G2 |
| LGT_01 Night Archive Lighting | VISUAL LOCKED / TECHNICAL DRAFT | 정확한 조명·렌더링 설계 | G2 |
| FIN_01 Conference Table Team Finisher | VISUAL LOCKED / FEATURE COULD / FUNCTIONAL DRAFT | 제품 범위 결정 전 구현 금지 | G2 |
| 파괴 예산 | HIGH-LEVEL BASELINE | 정확한 파편·Particle·VFX·성능 예산 | G2 |
| Vertical Slice enemy·room 확장 | DRAFT | G2 Final Audit 이후 별도 범위 결정 | G2 |
| G3 Technical Baseline | OWNER-APPROVED / FINAL AUDIT PENDING | TASK-0052 — Correct Residual G3 Active Status and Package-Authority Drift | G3 NOT PASSED |
| 기술 아키텍처 | OWNER-APPROVED G3 DIRECTION / IMPLEMENTATION DRAFT | Final Audit에서 책임·데이터·이벤트·검증·예산·Reset·harness 정합성 검증 | G3 |
| Graybox | NOT STARTED | Design Readiness Gate 통과 후 | G4 |
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

FIN_01은 PR-F-022 `COULD` 기능의 Visual-only conditional direction이며 승인된 Vertical Slice 또는 최종 제품 범위에 포함되지 않는다. Full Design Lock은 여전히 미완료이고 Implementation Gate는 `CLOSED`다.

`CHR_00 Player Turnaround`은 향후 3D 모델링용 `PENDING` Production Support 산출물이며 현재 Production Visual Lock 순서를 차단하지 않는다.

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
Acceptance Test의 정확한 수치·임계값, 파괴 예산, 카메라 기능,
조명·렌더링 구현, FIN_01 제품 편입, exact class·payload·schema·
tuning, Unity Bootstrap·구현 ticket과 Vertical Slice의 정확한
콘텐츠는 `DRAFT`, `COULD`, `DEFERRED` 또는 미승인이다. 승인된 여덟
Visual의 문서화된 Canonical Elements만
`LOCKED`이며 현재 Phase label은 Owner가 승인한
`PRE-PRODUCTION / G3 TECHNICAL BASELINE`이다. G3 Technical은
`NOT PASSED / CURRENT BASELINE WORK`이다. G3 Technical Draft와 빈
Unity 예정 구조를 기준선에 대조한 Preflight Audit 결과는
Owner의 `DEC-G3-001`~`DEC-G3-013: A` 승인으로 소비되었다.
기술 책임·dependency·state/event/causal/physics/Strap/budget/Reset/
harness 정책과 exact documentation lock
(`6000.3.20f1`, URP `17.3.0`, Input System `1.20.0`, Cinemachine
`3.1.7`, Test Framework `1.6.0`)은 Owner-approved G3 baseline이다.
AI Navigation은 Prototype에서 Deferred다. Technical checklist는
`6 / 6`, Design Freeze는 `28 / 32`다. 실제 Unity project와 package
file은 생성되지 않았고 exact tuning, implementation class와
pass threshold는 Draft다. G3 Causal Result Family는 Owner-approved
G2 계약의 정확한 네 family로 교정되었으며 Structural Feature는 별도
optional design role로 유지된다. Final Audit은 active Phase와
AI Navigation package authority drift를 발견해 `REVISION REQUIRED`로
판정했다. 다음 단일 산출물은
`TASK-0052 — Correct Residual G3 Active Status and Package-Authority
Drift`다.
Final Audit 전에는 G3를 통과시키지 않으며 Bootstrap, Graybox와
구현 권한은 없다.
