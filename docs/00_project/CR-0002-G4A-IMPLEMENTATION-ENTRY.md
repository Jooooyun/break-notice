# CR-0002: G4-A Implementation Entry

- Requester: `Design Authority under Standing Owner Delegation`
- Date: `2026-08-04`
- Change Class: `C2`
- Status: `APPROVED`
- Reason: `ADR-0019's implementation-gate vocabulary is intentionally limited to IMP-0001 and cannot authorize the next bounded G4-A code ticket`
- Current Authority: `CLOSED or OPEN — IMP-0001 ONLY / EMPTY BOOTSTRAP`
- Approved Change: `post-Bootstrap bounded per-ticket G4-A gate model`
- Gameplay Effect: `NONE`
- Story/Art Effect: `NONE`
- Technical Effect: `permits future exact code tickets only through approved decomposition, immutable baseline, one-shot relay and exact scope`
- Test Effect: `each future implementation ticket owns its exact automated validation`
- Rollback: `Gate remains CLOSED and the new ticket remains unexecuted`
- Decision: `APPROVED`

## 적용 경계

TASK-0072의 성공한 IMP-0001 결과는 `APPROVE WITH NON-BLOCKING NOTES`로
소비한다. Empty Unity Bootstrap과 exact Package lock은 그대로 보존한다.
ADR-0019는 IMP-0001의 역사적·의미론적 권위로 유지하며 수정하지 않는다.

이 변경은 post-Bootstrap G4-A의 미래 implementation ticket마다 별도의
승인된 decomposition, 이미 push된 immutable baseline, branch, exact
scope, one-shot Execution Authorization Relay와 자체 validation을 요구한다.
TASK-0073은 첫 미래 instance를 정의할 뿐 Gate를 열거나 Relay를 발급하지
않는다.

## 승인 결과

- G4-A Foundation Entry: `OWNER-APPROVED / RELAY PENDING`
- IMP-0002: `OWNER-APPROVED TICKET DECOMPOSITION / EXECUTION BLOCKED`
- Implementation Gate: `CLOSED`
- Active Execution Authorization Relay: `ABSENT`
- G4-A Execution: `NOT STARTED / NOT AUTHORIZED`
- G4: `NOT PASSED / NOT STARTED / NOT AUTHORIZED`
- Graybox: `NOT STARTED`
- External AI Asset: `NOT AUTHORIZED`
- Next Authorization Boundary:
  `OWNER IMP-0002 EXECUTION AUTHORIZATION RELAY REQUIRED`

## 명시적 비승인 범위

이 Change Request는 gameplay, locomotion, camera, input, interaction,
physics, ally behavior, Cargo Strap, enemy AI, Scene, Prefab, Graybox,
external asset, G4 passage, G5 passage, Gate opening, Relay 발급 또는
IMP-0002 실행을 승인하지 않는다.
