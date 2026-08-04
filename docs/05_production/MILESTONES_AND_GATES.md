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

Implementation Gate의 default state는 `CLOSED`다. ADR-0019의
`OPEN — IMP-0001 ONLY / EMPTY BOOTSTRAP`은 완료된 IMP-0001의 역사적
one-shot state로 보존한다. ADR-0022가 정의한 post-Bootstrap future
open-state format은 다음과 같다.

`OPEN — <APPROVED TICKET ID> ONLY / <EXACT APPROVED SCOPE>`

모든 future open state는 ticket-, baseline-, branch-, scope-specific,
one-shot, non-inheritable, non-reusable이며 별도 Execution Authorization
Relay가 공급한다. 완료, stop, baseline drift, dirty tracked/staged
pre-state, 다른 ticket/scope, unauthorized dependency, scope expansion
또는 revocation에서 자동 만료한다. 다른 implementation wave,
gameplay, Graybox 또는 G4/G5 passage를 자동으로 허용하지 않는다.

첫 future instance
`OPEN — IMP-0002 ONLY / RUNTIME IDENTITY AND STATE FOUNDATION`은
정의됐지만 TASK-0073에서 열리지 않았다. execution SHA는 티켓에
저장하지 않는다. 별도 Owner-approved Relay가 이미 push된 완전한
immutable SHA, branch, clean local/origin equality, exact Gate state,
범위, operations, validations, commit/push 권한과 expiration을 제공해야
한다. 과거 audit, Lock과 Relay 결과는 immutable point-in-time
evidence이며 현재 Gate·Relay 값은 DESIGN_STATUS에서만 확인한다.

## G4-A Post-Bootstrap Readiness Classification

- Player Failure Direction and Decision Rule: `OWNER-APPROVED`.
- Detailed G4-A Evidence Protocol:
  `OWNER-APPROVED / CORRECTED / READY FOR FUTURE EXECUTION AUTHORIZATION`.
- TASK-0072 Completion Review: `APPROVE WITH NON-BLOCKING NOTES`.
- IMP-0001 Disposition: `APPROVE / COMPLETE`.
- Post-IMP-0001 Design Authority Review: `APPROVE`.
- CR-0002: `APPROVED / APPLIED`.
- ADR-0022: `ACCEPTED`.
- IMP-0002:
  `OWNER-APPROVED TICKET DECOMPOSITION / EXECUTION BLOCKED`.
- G4-A Foundation Entry: `OWNER-APPROVED / RELAY PENDING`.
- G4-A Execution: `NOT STARTED / NOT AUTHORIZED`.

이 분류는 G4를 시작·통과시키거나 Graybox, gameplay, external asset,
Gate opening 또는 Relay 발급 권한을 만들지 않는다. 현재 Gate와 다음
권한 경계의 exact live value는 DESIGN_STATUS를 따른다.
