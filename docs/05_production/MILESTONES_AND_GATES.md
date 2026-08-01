# Milestones and Gates

| Gate | 통과 조건 | 실패 시 |
|---|---|---|
| G0 Charter | 목표·제약·권한 승인 | 프로젝트 재정의 |
| G1 Requirements | MUST 요구사항과 검증 완비 | 요구사항 보완 |
| G2 Design | 시스템·아트·범위 모순 없음 | 설계 수정 |
| G3 Technical | 책임·데이터·테스트·예산 매핑 | 기술 설계 수정 |
| G4 Graybox | 5분 재미 지표 통과 | 수치·카메라·배치 조정, 필요 시 CR |
| G5 Freeze | 실제 제작 범위와 에셋 목록 확정 | 본 제작 금지 |
| G6 Slice | 기능·성능·인수 기준 통과 | 범위 축소 또는 수정 |
| G7 Decision | 시장성·재플레이·유지 가능성 증거 | 확장/축소/중단 결정 |

## Current-State Source

이 문서는 Gate의 통과 조건과 실패 의미를 정의한다. 현재 Phase, 각 Gate,
Design Freeze, Pre-G4 Full Design Lock, Graybox, Implementation Gate,
Relay, ticket readiness와 Current Next Authorization Boundary의 값은
`docs/00_project/DESIGN_STATUS.md`만 따른다. 이 문서는 독립적인
live-state board 또는 next-task authority가 아니다.

## Stable Pre-G4 and Limited-Gate Semantics

- Design Freeze Checklist `32 / 32`는 documentary evidence prerequisite다.
  Full Design Lock이나 Gate passage가 아니다.
- Pre-G4 Full Design Lock은 승인된 frozen documentation baseline이다.
  G4 evidence, performance validation 또는 G5 Production Freeze를
  대신하지 않는다.
- G4 Graybox는 infrastructure preparation 뒤 별도로 승인되는 physical
  gameplay validation이다.
- G5 Production Freeze는 G4 evidence를 사용하는 production scope,
  asset list, schedule과 risk 승인이다.
- 정확한 순서는 Pre-G4 documentary baseline과 Full Design Lock,
  narrowly authorized infrastructure preparation, separately authorized
  G4 Graybox, G4 evidence/decision, G5 Production Freeze, later Vertical
  Slice production이다. Empty Bootstrap은 G4를 시작하지 않는다.

Implementation Gate 상태는 `CLOSED` 또는
`OPEN — IMP-0001 ONLY / EMPTY BOOTSTRAP`만 사용한다. future open
state는 ticket-, baseline-, scope-specific one-shot이며 IMP-0001
empty Bootstrap 외 gameplay, C#, Test, Scene, Prefab, Graybox, 다른
ticket 또는 G4/G5 passage를 허용하지 않는다. 완료, stop, baseline
drift, dirty tracked/staged pre-state, 다른 ticket/scope, unauthorized
Package, scope expansion 또는 revocation에서 자동 만료한다.

IMP-0001 concrete SHA는 티켓에 저장하지 않는다. 별도 Owner-approved
Execution Authorization Relay가 이미 push된 완전한 immutable SHA,
`main`, clean local/origin equality, exact Gate state, 범위, operations,
validations, commit/push 권한과 one-shot expiration을 제공해야 한다.
과거 audit, Lock과 Relay 결과는 immutable point-in-time evidence이며
현재 Gate·Relay 값은 DESIGN_STATUS에서만 확인한다.

## G4-A C3 Readiness Classification

- Player Failure Direction and Decision Rule: `OWNER-APPROVED`.
- Detailed G4-A Evidence Protocol: `PENDING`.
- G4-A Execution: `NOT STARTED / NOT AUTHORIZED`.
- C3 Player-Failure Addendum:
  `APPLIED / TARGETED RE-AUDIT REQUIRED BEFORE G4-A IMPLEMENTATION`.
- Targeted Re-Audit:
  `TASK-0069 — Targeted C3 Full-Game and G4-A Authority Re-Audit`.

후속 detailed protocol은 observer, session, explanation, variant order,
`YES/NO` evidence와 result reporting을 정의해야 한다. 이 분류는 G4를
시작·통과시키거나 Graybox, Unity, toolchain provisioning, Gate 또는
Relay 권한을 만들지 않는다.
