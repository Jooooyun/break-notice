# G2 Logical Input Action Contract

> **OWNER-APPROVED G2 LOGICAL INPUT ACTION CONTRACT**
>
> Decision Source: `TASK-0036 delegated Owner decision`
>
> 이 문서는 플레이어가 표현할 수 있는 논리 입력 의도의 이름과
> 의미만 승인한다. 실제 키보드·게임패드 바인딩, 입력 timing과
> threshold, 상태 가용성·우선순위·충돌, buffer와 cancel 규칙은
> 승인하지 않는다. 애니메이션·물리·Unity Input System 및 구현
> class·component는 G3 Draft이며 현재 Unity 작업은 허가되지 않았다.

## Authority and Scope Boundary

- Prototype Core의 활성 논리 액션은 정확히 8개다.
- Vertical Slice의 활성 논리 액션은 정확히 1개다.
- 이 계약은 action availability, arbitration 또는 state transition을
  직접 결정하지 않는다. 해당 Owner-approved 기준은
  [`INTERACTION_STATE_CONFLICT_CONTRACT.md`](INTERACTION_STATE_CONFLICT_CONTRACT.md)를
  따른다.
- `InteractPrimary`, `Throw`, `CallAlly`, `HeavyAction`의 class-based
  eligibility와 delivery envelope는
  [`WEIGHT_CLASS_DATA_RANGE_CONTRACT.md`](WEIGHT_CLASS_DATA_RANGE_CONTRACT.md)를
  따른다.
- exact tap/hold/release 해석, 접근성 remapping과 물리적 binding은
  별도 G2/G3 결정 전까지 Draft다.
- G1 Requirement 문구, Priority, Applicability와 Authority는
  변경하지 않는다.
- G2 Design Gate는 여전히 `NOT PASSED`이고 Implementation Gate는
  `CLOSED`다.

## Input Inventory Classification

| 분류 | 이름 | 현재 처리 |
|---|---|---|
| Active Prototype Core logical action | `Move`, `Look`, `Aim`, `InteractPrimary`, `Throw`, `CallAlly`, `StrapRelease`, `ResetEncounter` | 이 계약에서 intent 이름과 의미 승인 |
| Active Vertical Slice logical action | `HeavyAction` | Vertical Slice applicability로만 승인 |
| Context result or action mode | Scoop, Normal Catch, Perfect Catch attempt, Strap Attach, Direct Throw, Quick Throw, Charged Throw, Queued Throw, Cargo Strap Redirect | 별도 input ID가 아니며 아래 context mapping만 승인 |
| Deferred candidate | `Sprint`, `Evade`, `Vault`, `Shove` | 별도 G2 scope decision 전까지 비활성 Draft |
| Prohibited or out of scope | Telekinesis, MagicGrab, Grapple, NecktieAction, AllySwap, PartySelect, SecondAllyCommand, firearm action, FIN_01 전용 production input, debug-only physics manipulation | 이 gameplay logical-action contract에 추가하지 않음 |

일반 pause, menu navigation, UI confirmation, screenshot, developer
console과 accessibility-remapping command는 이 gameplay 논리 입력
계약의 범위 밖이다.

## Active Logical Actions

| Action ID | Applicability | Logical value shape | Player intent | Approved context or responsibility | Explicitly excluded meaning | Requirement or authority source | Status |
|---|---|---|---|---|---|---|---|
| `Move` | `PROTOTYPE CORE` | two-axis continuous intent | 원하는 평면 이동 방향을 표현한다. | 일반적인 플레이어 locomotion 방향 의도 | acceleration, speed, Sprint, root motion, controller technology | ADR-0002; PGT_01 Camera Canon | `OWNER-APPROVED` |
| `Look` | `PROTOTYPE CORE` | two-axis continuous intent | 원하는 camera 및 facing-orientation 변화를 표현한다. | 3D TPS에서의 시야와 방향 조정 의도 | sensitivity, inversion, aim assist, FOV, Cinemachine | ADR-0002; PGT_01 Camera Canon | `OWNER-APPROVED` |
| `Aim` | `PROTOTYPE CORE` | sustained Boolean intent | 의도적인 targeting 또는 directional-focus 요청을 표현한다. | 목표와 진행 방향을 명시하려는 지속 의도 | hold/toggle binding, lock-on, aim assist, camera values | PGT_01 Camera Canon; PR-F-002 | `OWNER-APPROVED` |
| `InteractPrimary` | `PROTOTYPE CORE` | discrete interaction intent | 현재 유효한 primary physical interaction을 요청한다. | Scoop, Normal Catch, Perfect Catch timing attempt, Strap Attach의 가능한 context | context priority, target ranking, range, timing, automation, state conflicts | PR-F-001, PR-F-004, PR-F-006, PR-F-023; ADR-0003 | `OWNER-APPROVED` |
| `Throw` | `PROTOTYPE CORE` | phase-aware throw intent | 직접 투척 또는 도착 전 Queued Throw 의도를 표현한다. | direct throw 및 pre-arrival Queued Throw intent | exact Quick/Charged split, hold duration, charge curve, buffer, impulse, animation | PR-F-005; ADR-0003 | `OWNER-APPROVED` |
| `CallAlly` | `PROTOTYPE CORE` | discrete request intent | 한 명의 활성 동료에게 실제 물체 공급을 요청한다. | 승인된 active ally의 physical object supply 요청 | multiple allies, ally selection, inventory, pathfinding, exact timing, animation | PR-F-003, PR-F-013, PR-F-014; ADR-0004 | `OWNER-APPROVED` |
| `StrapRelease` | `PROTOTYPE CORE` | discrete release intent | 활성 GEAR_01 연결 종료를 명시적으로 요청한다. | 플레이어가 시작하는 explicit connection release intent | automatic Safety Release rule, priority, threshold, timing, implementation | PR-F-006; GEAR_01 Visual Canon | `OWNER-APPROVED` |
| `ResetEncounter` | `PROTOTYPE CORE` | deliberate reset command | 현재 validation encounter의 기준 상태 복원을 요청한다. | 같은 검증 상황을 재실행하기 위한 reset 요청 | exact binding, confirmation, hold duration, reset time, snapshot implementation, save system | PR-F-017; AT-FLOW-001 | `OWNER-APPROVED` |
| `HeavyAction` | `VERTICAL SLICE` | discrete Heavy-use intent | 후속 승인 조건이 충족될 때 Heavy 접근 또는 사용을 요청한다. | Vertical Slice의 Heavy-object access/use 요청 | Prototype activation, cost, Pip count, Structural behavior, animation, physics, implementation | PR-F-011 | `OWNER-APPROVED` |

## Active Action Responsibilities

### Move

평면 이동 방향의 논리 의도만 소유한다. Sprint나 실제 이동 속도,
가속, animation 또는 controller technology를 소유하지 않는다.

### Look

camera 및 facing-orientation 변화 의도만 소유한다. 정확한 감도,
반전, FOV와 camera implementation을 소유하지 않는다.

### Aim

targeting 또는 directional-focus 의도만 소유한다. lock-on, 보조
조준과 실제 camera state를 소유하지 않는다.

### InteractPrimary

Scoop, Normal Catch, Perfect Catch timing attempt와 Strap Attach에
사용될 수 있는 하나의 primary interaction intent다. 어떤 context가
활성인지, 겹칠 때 무엇이 우선인지 또는 어떤 대상이 선택되는지는
이 계약에서 결정하지 않는다.

### Throw

direct throw와 pre-arrival Queued Throw가 공유하는 하나의 throw
intent다. Quick, Charged와 Queued의 press/hold/release 해석, buffer,
charge와 force는 이 계약에서 결정하지 않는다.

### CallAlly

일반 전투의 한 명의 활성 동료에게 물리적 물체 공급을 요청한다.
동료 선택, 자유 추적 AI, 다수 동료, 재고, 경로와 timing은 이
계약에서 결정하지 않는다.

### StrapRelease

활성 GEAR_01 연결을 끝내려는 명시적 플레이어 의도다. 자동 Safety
Release, state priority, threshold와 exact timing은 이 계약에서
결정하지 않는다.

### ResetEncounter

현재 validation encounter 기준 상태 복원을 요청한다. binding,
확인 방식, 복원 시간과 snapshot/save implementation은 이 계약에서
결정하지 않는다.

### HeavyAction

Vertical Slice에서만 Heavy-object access 또는 use를 요청한다.
Prototype Core 활성화, Momentum/Pip 비용, Structural Feature,
animation과 physics는 이 계약에서 결정하지 않는다.

## Context and Mode Mapping

| Context or mode | Logical action |
|---|---|
| Scoop | `InteractPrimary` |
| Normal Catch | `InteractPrimary` |
| Perfect Catch attempt | `InteractPrimary` |
| Strap Attach | `InteractPrimary` |
| Direct Throw | `Throw` |
| Queued Throw | `Throw` |
| Explicit Strap Release | `StrapRelease` |
| Ally supply request | `CallAlly` |
| Heavy request | `HeavyAction` |
| Encounter reset | `ResetEncounter` |

이 mapping은 action arbitration, state availability, interaction
priority, target ranking, input buffer 또는 cancel rule을 결정하지
않는다. `Quick Throw`, `Charged Throw`와 `Queued Throw`는 별도 logical
input ID가 아니라 단일 `Throw` intent의 후보 해석이다.

## Weight-Class Functional Authority

다음 class boundary는
[`WEIGHT_CLASS_DATA_RANGE_CONTRACT.md`](WEIGHT_CLASS_DATA_RANGE_CONTRACT.md)에서
Owner-approved되었으며 이 계약의 9개 logical action ID를 변경하지
않는다.

- `InteractPrimary`: Scoop은 유효 Light/Medium, Catch는
  Catch-eligible Light, Strap Attach는 eligible floor-supported
  Medium에만 적용한다.
- `Throw`: Direct Throw는 carried Light/Medium, Queued Throw는
  Catch-eligible Light relay에만 적용한다.
- `CallAlly`: Light는 airborne physical throw, mobile Medium은
  floor-supported roll/push/grounded feed, Heavy는 normal relay 없음.
- `HeavyAction`: Heavy의 유일하게 승인된 player-action entry
  point이며 `VERTICAL SLICE ONLY`다.

exact object assignment, mass, size, speed, force, timing, movement
restriction, target ranking, Momentum/Pip, animation, physics와
implementation은 Draft다.

## Deferred Logical-Action Candidates

아래 네 동사는 모두
`DRAFT CANDIDATE — NOT ACTIVE IN THE G2 LOGICAL INPUT BASELINE`이다.
`REJECTED` 또는 `WON'T`가 아니며 기존 설계 논의는 삭제하지 않는다.

| Candidate | Reason for deferral |
|---|---|
| `Sprint` | Owner-approved G1 Prototype Core에 필수적이지 않고 locomotion 및 state-conflict 범위를 추가하므로 별도 G2 scope decision이 필요하다. |
| `Evade` | Owner-approved G1 Prototype Core에 필수적이지 않고 combat, animation 및 state-conflict 범위를 추가하므로 별도 G2 scope decision이 필요하다. |
| `Vault` | Owner-approved G1 Prototype Core에 필수적이지 않고 locomotion, animation 및 collision-state 범위를 추가하므로 별도 G2 scope decision이 필요하다. |
| `Shove` | Owner-approved G1 Prototype Core에 필수적이지 않고 combat, physics 및 interaction-priority 범위를 추가하므로 별도 G2 scope decision이 필요하다. |

각 후보의 활성화, Applicability와 Priority는 별도 Owner G2 결정 없이는
정해지지 않는다.

## Prohibited and Out-of-Scope Actions

이 계약은 다음 action을 추가하지 않는다.

- `Telekinesis`
- `MagicGrab`
- `Grapple`
- `NecktieAction`
- `AllySwap`
- `PartySelect`
- `SecondAllyCommand`
- firearm action
- FIN_01-specific production input
- debug-only physics manipulation

FIN_01은 계속 `OUT / COULD`이고 일반 전투에 두 번째 active ally를
추가하지 않는다. 암적색 넥타이는 복장 전용이며 입력 장치나 물체
연결 수단이 아니다.

## Remaining G2 and G3 Decisions

TASK-0037에서 다음 G2 항목은
[`INTERACTION_STATE_CONFLICT_CONTRACT.md`](INTERACTION_STATE_CONFLICT_CONTRACT.md)로
Owner-approved되었다.

- logical state ownership과 action availability;
- cross-context `InteractPrimary` arbitration;
- Scoop, Catch, Throw와 Cargo Strap conflict;
- Queued Throw와 active relay의 안전한 해소;
- `ResetEncounter` 우선순위;
- `HeavyAction` exclusivity.

TASK-0038에서 다음 G2 항목은
[`WEIGHT_CLASS_DATA_RANGE_CONTRACT.md`](WEIGHT_CLASS_DATA_RANGE_CONTRACT.md)로
Owner-approved되었다.

- Light/Medium/Heavy functional class envelope;
- class-based Catch와 Strap assignment;
- class-based Carry, Throw와 Relay eligibility;
- Heavy의 `VERTICAL SLICE ONLY` / `HeavyAction` boundary.

다음 항목은 계속 `DRAFT`다.

- same-context target ranking;
- exact timing, range와 angle;
- input buffering과 cancellation windows;
- tap, hold, release threshold와 physical bindings;
- Carry/Strap/Throw/Heavy 중 movement restriction;
- exact physical mass와 object-specific assignment;
- exact size, speed와 force;
- Momentum/Pip와 Heavy cost;
- accessibility remapping;
- exact feedback, animation과 physics behavior;
- Unity Input System 구성;
- implementation class, component와 data contract.

`Sprint`, `Evade`, `Vault`, `Shove`는 계속 비활성 Draft candidate다.
다음 G2 설계 산출물은 Momentum/Pip starting-value contract다.
