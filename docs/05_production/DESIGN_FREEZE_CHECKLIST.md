# Design Freeze Checklist

## Product

- [x] 한 문장 정의 승인
- [x] 타깃 플레이어와 플랫폼 승인
- [x] MUST/SHOULD/WON'T 승인
- [x] 5분 재미 기준 승인

## Gameplay

- [x] 모든 논리 입력 정의
- [x] Scoop/Catch/Throw/Strap 상태 충돌 정의
- [ ] Light/Medium/Heavy 데이터 범위 정의
- [ ] Momentum/Pip 수치 시작값 정의
- [ ] Chain 전파와 종료 정의
- [ ] Ally Relay와 Suppressed 정의
- [ ] Prototype 적·방·물체 목록 고정

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

- [ ] Unity 정확한 버전과 패키지 잠금
- [ ] 아키텍처 검토
- [ ] Event Contract 검토
- [ ] 성능 예산 검토
- [ ] Reset 전략 검토
- [ ] Test harness 계획

## Codex

- [x] AGENTS.md
- [x] Execution Protocol
- [x] Task Template
- [ ] 첫 구현 티켓 세분화

## Approval

현재 체크 상태: `17 / 32`

Product의 네 항목은 G1 Owner 승인으로 완료되었다. Gameplay,
Technical, Production Support와 첫 구현 티켓 항목은 미완료이며
Full Design Lock은 `INCOMPLETE`다.

Logical Input Action Contract와 Interaction State, Arbitration, and
Conflict Contract는 Owner-approved다. G2 Gameplay checklist는
`2 / 7`이며 Light/Medium/Heavy 데이터 범위는 미해결이다.
G2 Design은 계속 `NOT PASSED`다.

모든 필수 항목이 체크되기 전 본 제작을 시작하지 않는다.
