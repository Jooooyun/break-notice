# Design Freeze Checklist

## Product

- [x] 한 문장 정의 승인
- [x] 타깃 플레이어와 플랫폼 승인
- [x] MUST/SHOULD/WON'T 승인
- [x] 5분 재미 기준 승인

## Gameplay

- [x] 모든 논리 입력 정의
- [x] Scoop/Catch/Throw/Strap 상태 충돌 정의
- [x] Light/Medium/Heavy 데이터 범위 정의
- [x] Momentum/Pip 수치 시작값 정의
- [x] Chain 전파와 종료 정의
- [x] Ally Relay와 Suppressed 정의
- [x] Prototype 적·방·물체 목록 고정

## Art

- [x] 주인공 Identity Lock
- [ ] 주인공 Turnaround
- [x] Cargo Strap 작동 시트
- [x] 첫 동료 Identity Lock
- [x] PGT_01 Camera Lock
- [x] PGT_02 Core Loop Lock
- [x] PGT_03 Aftermath Visual Lock
- [x] LGT_01 Night Archive Lighting Lock
- [x] FIN_01 Conference Table Team Finisher Lock
- [x] 정확한 파괴 예산 수치와 구현 규칙
- [x] Office Core Kit 목록

## Technical

- [x] Unity 정확한 버전과 패키지 잠금
- [x] 아키텍처 검토
- [x] Event Contract 검토
- [x] 성능 예산 검토
- [x] Reset 전략 검토
- [x] Test harness 계획

## Codex

- [x] AGENTS.md
- [x] Execution Protocol
- [x] Task Template
- [ ] 첫 구현 티켓 세분화

## Approval

현재 체크 상태: `30 / 32`

Product의 네 항목은 G1 Owner 승인으로 완료되었다. Gameplay의
일곱 항목은 Owner-approved G2 Design Baseline으로 완료되었다.
Technical 여섯 항목은 Owner-approved G3 documentation baseline으로
`6 / 6` 검토되었고 성공한 Re-Audit과 Owner 승인에 따라 G3 Gate는
`PASSED`다. Unity/package checkbox는 exact documentation lock을
뜻하며 Unity 또는 package file 설치를 뜻하지 않는다. 파괴 예산과
구현 규칙은 Owner-approved로 LOCK되었다. Prototype의 모든 파괴
category cap은 `0`이고 Later-Slice의 bounded hard cap, pre-authored
state, preallocated pool, atomic admission, causal-carrier·collision·
navigation·Reset·fault 경계는
`docs/04_technical/DESTRUCTION_BUDGET_AND_IMPLEMENTATION_RULES.md`와
ADR-0016을 따른다. Structural Feature는 `VERTICAL SLICE / SHOULD /
PARTIAL`이며 별도 G5 승인 없이는 활성화되지 않는다. 이는 Unity,
Graybox, 성능 합격값 또는 구현 권한이 아니다.
Office Core Kit checkbox는 exact `16` Prototype-required family와
exact `8` Later-Slice extension candidate, interaction·variant·reuse·
Prototype zero-destruction boundary의 asset-family list가
Owner-approved/LOCKED되었음을 뜻한다. 3D model, texture, material 또는
Unity asset은 생성되지 않았고 G5는 `NOT PASSED`이며 Unity 또는 Graybox
권한이 없다. 체크되지 않은 항목은
`주인공 Turnaround`과 `첫 구현 티켓 세분화`의
정확히 두 개다. Design Freeze는 `30 / 32`, Full Design Lock은
`INCOMPLETE`다.

Logical Input Action Contract, Interaction State, Arbitration, and
Conflict Contract와 Weight-Class Functional Data Range Contract는
Owner-approved다. Momentum/Pip profile도 시작값으로 승인되었지만
final balance는 아니다. Causal Chain propagation과 termination의
logical contract도 Owner-approved다. G2 Gameplay checklist는
`7 / 7`이다. 일반 전투 CHR_01 한 명, Prototype 고정 support
position, 실제 Light/Medium supply, Vertical Slice Relay Position과
조건부 `SHOULD` Suppressed의 logical contract는 Owner-approved다.
`PRT-RM-001` 한 room, Compliance Runner 두 명, `PRT-RP-001`, 네
object definition·일곱 instance와 두 physical pool도 Owner-approved다.
Suppressed의 최종 Vertical Slice 편입은 보장되지 않으며 ally AI,
exact geometry·position·ranking·timing, physical tuning, final art와
technical scope는 미해결이다. Chain runtime lineage와 exact settlement
threshold도 G3/G4 Draft다. Gameplay `7 / 7`과 교정 기준선 Re-Audit의
성공, Owner 승인을 근거로 G2 Design Gate는 `PASSED`다. G3 기술
책임·데이터·이벤트·검증·예산·Reset·test-harness 방향과 exact
Editor/package documentation lock은 Owner-approved다. G3 Causal Result
Family alignment는 `OWNER-APPROVED / CORRECTED`다. G3 Final Audit은
`REVISION REQUIRED / HISTORICAL BASELINE AUDIT`, 잔여 활성 상태·패키지
권한 교정은 `OWNER-APPROVED / COMPLETE`, remediation은
`COMPLETE / VERIFIED BY RE-AUDIT`다. Re-Audit의
`READY FOR G3 GATE DECISION` 판정은 G3 passage에 소비되었고 문서
자체는 변경하지 않는다. 현재 Phase는
`PRE-PRODUCTION / DESIGN FREEZE COMPLETION`이며 다음 작업은
`PENDING DESIGN AUTHORITY REVIEW AFTER TASK-0056`다. 새 numbered
task는 승인되지 않았다.
G3 Technical은 `PASSED / OWNER-APPROVED BASELINE`, Technical Checklist는
`6 / 6`, Design Freeze는 `30 / 32`, G4와 G5는 `NOT PASSED`, Full
Design Lock은 `INCOMPLETE`, Graybox는 `NOT STARTED`, Implementation
Gate는 `CLOSED`다.

모든 필수 항목이 체크되기 전 본 제작을 시작하지 않는다.
