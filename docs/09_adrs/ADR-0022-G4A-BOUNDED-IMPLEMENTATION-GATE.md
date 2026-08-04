# ADR-0022 — G4-A Bounded Per-Ticket Implementation Gate

- Status: `Accepted`
- Date: `2026-08-04`
- Decision Source:
  - `TASK-0072 successful IMP-0001 completion`
  - `TASK-0073 Owner-delegated Design Authority decision`
  - `docs/00_project/CR-0002-G4A-IMPLEMENTATION-ENTRY.md`

## Context

TASK-0072는 exact Unity `6000.3.20f1 / c9ba695d4f07`, 정확한 네 Direct
Package, stable Package Lock과 Empty Unity Project를 검증하고 IMP-0001을
완료했다. 사용된 one-shot Relay는 소비·만료됐고 Gate는 `CLOSED`로
복귀했다.

ADR-0019는 Pre-G4 Full Design Lock과 IMP-0001-only Empty Bootstrap
Gate의 역사적·의미론적 권위로 계속 유효하다. 다만 그 vocabulary는
의도적으로 IMP-0001에 한정되어 이후 bounded G4-A code ticket을 승인할
수 없다. CR-0002는 이 충돌을 post-Bootstrap per-ticket 모델로 해소한다.

## Decision

ADR-0019는 수정하거나 대체하지 않는다. ADR-0022는 post-Bootstrap
G4-A를 위한 다음 모델을 추가한다.

Default state:

`CLOSED`

Future bounded open-state format:

`OPEN — <APPROVED TICKET ID> ONLY / <EXACT APPROVED SCOPE>`

모든 open state는 다음 조건을 전부 만족해야 한다.

- ticket-specific
- baseline-specific
- branch-specific
- scope-specific
- one-shot
- non-inheritable
- non-reusable
- 별도의 Execution Authorization Relay로 공급
- completion 또는 Stop Condition에서 자동 만료

각 Relay는 승인된 exact ticket ID, 이미 `origin/main`에 push된 완전한
immutable SHA, exact branch, clean local/remote equality, exact Gate
state, allowed/forbidden files, operations, validations, commit/push 권한과
expiration을 독립적으로 제공해야 한다. 이전 ticket의 Gate 또는 Relay를
상속하거나 재사용할 수 없다.

첫 승인된 미래 instance는 다음과 같다.

`OPEN — IMP-0002 ONLY / RUNTIME IDENTITY AND STATE FOUNDATION`

TASK-0073은 이 state를 정의하지만 열지 않는다. 현재 Implementation
Gate는 `CLOSED`이고 active Relay는 `ABSENT`다.

## Non-Authorization Boundary

Gate format 또는 미래 IMP-0002 instance의 정의는 다음을 자동으로
승인하지 않는다.

- another implementation wave
- gameplay
- locomotion
- camera
- input
- interaction
- physics
- ally behavior
- Strap
- enemy AI
- Scene
- Prefab
- Graybox
- external asset
- G4 passage
- G5 passage

각 후속 wave는 자체 Owner-approved Ticket과 별도 one-shot Relay를
필요로 한다. Gate가 `CLOSED`인 동안 어떤 implementation ticket도 실행할
수 없다.

## Consequences

- IMP-0001은 `COMPLETE`이며 ADR-0019의 역사적 범위는 보존된다.
- CR-0002는 `APPROVED / APPLIED`다.
- IMP-0002 decomposition은 Owner-approved지만 execution은 blocked다.
- G4-A Foundation Entry는 `OWNER-APPROVED / RELAY PENDING`이다.
- G4-A Execution, G4와 Graybox는 시작되지 않는다.
- external AI asset pipeline은 승인되지 않는다.
- 다음 단일 권한 경계는
  `OWNER IMP-0002 EXECUTION AUTHORIZATION RELAY REQUIRED`다.

## Rollback

Relay가 발급되지 않거나 future precondition이 충족되지 않으면 Gate는
`CLOSED`로 유지되고 IMP-0002는 실행되지 않는다. Relay가 발급된 뒤
Stop Condition이 발생해도 해당 Relay와 open state는 만료되며 후속
권한으로 상속되지 않는다.
