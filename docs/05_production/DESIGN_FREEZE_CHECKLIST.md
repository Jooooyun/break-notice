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
- [ ] 정확한 파괴 예산 수치와 구현 규칙
- [ ] Office Core Kit 목록

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

현재 체크 상태: `28 / 32`

Product의 네 항목은 G1 Owner 승인으로 완료되었다. Gameplay의
일곱 항목은 Owner-approved G2 Design Baseline으로 완료되었다.
Technical 여섯 항목은 Owner-approved G3 documentation baseline으로
`6 / 6` 검토되었다. Unity/package checkbox는 exact documentation
lock을 뜻하며 Unity 또는 package file 설치를 뜻하지 않는다.
체크되지 않은 Art/Production Support 세 항목과 첫 구현 티켓 한
항목은 미완료이며 Full Design Lock은 `INCOMPLETE`다.

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
`CORRECTIONS APPLIED / RE-AUDIT REQUIRED`다. 다음 작업은
`TASK-0053 — G3 Technical Baseline Re-Audit and Gate Readiness`이며
G3 Technical은 `NOT PASSED / CURRENT BASELINE WORK`, Technical
Checklist는 `6 / 6`, Design Freeze는 `28 / 32`, Full Design Lock은
`INCOMPLETE`, Graybox는 `NOT STARTED`, Implementation Gate는 `CLOSED`다.

모든 필수 항목이 체크되기 전 본 제작을 시작하지 않는다.
