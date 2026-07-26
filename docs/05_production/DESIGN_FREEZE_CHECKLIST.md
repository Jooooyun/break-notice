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
- [x] 주인공 Turnaround
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
- [x] 첫 구현 티켓 세분화

## Approval

현재 체크 상태: `32 / 32`

## Checklist-Local Completion Record

- Checked: `32`
- Unchecked: `0`
- Checklist completion: `32 / 32`
- Meaning: documentary evidence prerequisite only

이 체크리스트의 완료는 요구·설계·기술·아트·Codex 증거가 문서상
모였음을 뜻한다. Full Design Lock, G4/G5 passage, Unity, package,
implementation, Graybox, build 또는 Execution Authorization Relay를
승인하지 않는다.

증거 소스는 G1/G2/G3 passage 기록, 승인된 파괴 예산,
Office Core Kit, CHR_00 Player Turnaround production support와
IMP-0001 ticket decomposition이다. 체크 항목의 실질 권위는 각 승인
contract, specification, lock과 ADR에 남는다.

현재 Phase, Gate, Lock, Graybox, provisioning, Relay, ticket readiness와
Current Next Authorization Boundary는
`docs/00_project/DESIGN_STATUS.md`만 따른다. 이 문서는 독립적인
live-state board 또는 next-task authority가 아니다.
