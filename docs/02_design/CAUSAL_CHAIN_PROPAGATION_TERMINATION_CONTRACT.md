# G2 Causal Chain Propagation and Termination Contract

> **OWNER-APPROVED G2 CAUSAL CHAIN PROPAGATION AND TERMINATION CONTRACT**
>
> Decision Source: `TASK-0040 delegated Owner decision`
>
> 이 계약은 인과 Origin의 생성, Direct/Downstream 분류, 물리적 계보,
> Branch, 최대 전파 깊이, Meaningful Result 고유성, 중복·순환 방지와
> 종료를 논리 설계 수준에서 승인한다. 런타임 자료구조, event, ID,
> 시간·속도 threshold, Unity 구현 또는 표현 방식을 승인하지 않는다.

## Authority and Scope Boundary

- 이 계약은 `PR-F-009`, `PR-F-010`, `PR-F-012`, `PR-F-016`에 필요한
  인과 귀속과 Chain 경계를 정의한다.
- G1 Requirement wording, Applicability, Priority, Authority,
  Trace Status와 Acceptance Test를 변경하지 않는다.
- 기존 Logical Input, Interaction State, Weight-Class와 Momentum/Pip
  계약 및 승인된 Visual Canon을 변경하지 않는다.
- 이 계약은 damage, Stability, Knockback, ragdoll, break value,
  animation, physics, HUD, VFX, audio 또는 Unity 구현을 승인하지 않는다.
- 이 문서의 Owner-approved causal 경계는 구현 또는 Gate 권한을
  만들지 않는다.
- 현재 전역 Phase, Gate, Lock, Graybox, Relay와 다음 권한 경계는
  [`docs/00_project/DESIGN_STATUS.md`](../00_project/DESIGN_STATUS.md)를
  따른다.

## Applicability

- Causal attribution은 `PROTOTYPE CORE`와 `VERTICAL SLICE` 모두에서
  활성 설계 기준이다.
- Momentum reward 평가는 `VERTICAL SLICE`에서만 활성화한다.
- Prototype Core는 이 계약으로 인과 귀속과 의미 있는 결과를 검증하지만
  Momentum 획득, Pip 변환·저장·소비 또는 HeavyAction을 요구하지 않는다.
- 이 계약은 Graybox 시작, G2 통과 또는 제품 콘텐츠 편입을 승인하지
  않는다.

## Core Terms

| Term | Owner-approved logical meaning |
|---|---|
| `Causal Origin` | 하나의 유효한 player-owned 시작 행동과 그 행동에서 물리적으로 이어지는 귀속 범위 |
| `Initiating Carrier` | Origin을 시작한 첫 물리적 운반체 |
| `Attributed Carrier` | 물리적 Transfer를 통해 같은 Origin을 전달받은 후속 운반체 |
| `Meaningful Result` | 장식 반응이 아니라 읽을 수 있는 물리적 상태 또는 국소 공간 변화 |
| `Transfer` | 현재 Carrier의 물리적 결과가 다른 적격 Entity를 다음 깊이의 Carrier로 만드는 관계 |
| `Branch` | 하나의 Carrier가 둘 이상의 독립된 후속 Carrier 또는 Result로 갈라지는 같은 Origin의 경로 |
| `Terminated Origin` | 더 이상 Carrier, Result 또는 reward를 받아들이거나 다시 열 수 없는 Origin |

이 용어는 class, component, enum, field, event payload, serialized ID
또는 database schema를 승인하지 않는다.

## Eligible Origin Creation

다음 player-owned 행동만 Momentum reward 자격이 있는 Causal Origin을
시작할 수 있다.

- carried `LIGHT`의 Direct Throw;
- valid carried `MEDIUM`의 Direct Throw;
- 유효한 Catch-oriented `LIGHT` relay 뒤의 Queued Throw;
- eligible `MEDIUM`에 대한 GEAR_01 Redirect.

Origin의 시작 순간은 다음 논리 경계다.

- Direct Throw 또는 Queued Throw: 물체가 player ownership을 떠나
  연속된 물리 trajectory에 진입할 때;
- GEAR_01 Redirect: 승인된 기계식 연결이 대상의 첫 committed
  trajectory change를 만들 때.

Strap Attach만으로는 Origin이 생기지 않는다. `HeavyAction`은 물리적
귀속 검증을 위해 별도의 non-rewarding tracked Origin을 만들 수 있지만,
그 Origin과 모든 downstream result는 Momentum을 만들지 않는다.

다음은 Origin을 만들지 않는다.

- Scoop, Carry 또는 Aim;
- `CallAlly` 요청과 ally wind-up;
- accepted reservation과 `PRE-RELEASE` relay;
- player ownership 전의 ally physical supply;
- Catch만 발생한 상태;
- Strap Attach 또는 Strap Release만 발생한 상태;
- Reset;
- passive motion과 cosmetic VFX;
- explicit Structural activation.

## Origin Provenance

각 Origin은 시작 시 다음 중 하나의 행동 출처를 유지한다.

- Direct Throw;
- Queued Throw;
- GEAR_01 Redirect;
- `HeavyAction — NON-REWARDING`.

Ally Relay와 Perfect Catch modifier 자격은 해당 Origin에만 속하며
후속 Carrier로 별도 복제되거나 다른 Origin으로 이동하지 않는다.
Origin의 행동 출처와 modifier 자격은 Origin 수명 동안 바뀌지 않는다.
이 분류는 runtime type, tag, field 또는 inheritance 구조를 승인하지
않는다.

Ally provenance는 승인된 relay가 수락되고 실제 물체가 `RELEASED`된
뒤 플레이어가 Catch→Throw/Queued Throw 또는 GEAR_01 Redirect로
행동 소유권을 얻을 때만 해당 player-owned Origin에 붙을 수 있다.
`PRE-RELEASE` abort와 player ownership 전 accidental result는 Origin
또는 modifier provenance를 만들지 않는다. release 뒤 ally가
Suppressed되어도 이미 유효한 Origin provenance를 지우지 않는다.
relay lifecycle의 권한은
[`ALLY_RELAY_SUPPRESSED_CONTRACT.md`](ALLY_RELAY_SUPPRESSED_CONTRACT.md)를
따른다.

## Direct and Downstream Classification

- `Direct Result`는 Initiating Carrier 자체가 만든 Meaningful Result다.
- 한 Initiating Carrier는 서로 다른 고유 Result를 둘 이상 만들 수 있다.
- Direct base category는 Initiating Carrier의 승인된 Weight Class에
  따라 `Direct meaningful Light` 또는 `Direct meaningful Medium`으로
  판단한다.
- `Downstream Result`는 Initiating Carrier가 아닌 Attributed Carrier가
  만든 Meaningful Result다.
- 결과가 늦게 발생했다는 이유만으로 Downstream이 되지 않는다.
- Meaningful local room-state change는 Direct 또는 Downstream 경로에서
  발생할 수 있지만 base reward는 가장 높은 room-state category 하나만
  적용한다.
- `HeavyAction` Origin과 explicit Structural activation의 결과는
  Direct/Downstream 분류와 무관하게 Momentum 비보상이다.

## Propagation Depth

최대 전파 깊이는 정확히 `4`다.

- Depth `0`: Initiating Carrier와 그 Direct Result;
- Depth `1`: 첫 번째 Transfer를 받은 Attributed Carrier와 그 Result;
- Depth `2`: 두 번째 Transfer를 받은 Attributed Carrier와 그 Result;
- Depth `3`: 세 번째 Transfer를 받은 Attributed Carrier와 그 Result;
- Depth `4`: 네 번째 Transfer를 받은 Attributed Carrier와 그 Result;
- Depth `4` Carrier는 Meaningful Result를 만들 수 있으나 Depth `5`
  Transfer를 만들 수 없다.

깊이는 collision 횟수, 경과 시간, Entity 수, spectacle 또는 damage가
아니라 Origin 시작 후 발생한 `Transfer count`이며 유효한 Transfer마다
정확히 `1` 증가한다. Depth 한도 뒤에도
물리 simulation은 계속될 수 있지만 해당 분기에는 새 Carrier, reward
또는 Origin 재개가 생기지 않는다.

## Propagation Eligibility

다음은 물리적 연결, 읽을 수 있는 방향, Meaningful Result 또는 의미
있는 이동, 남은 전파 깊이와 non-merge 조건을 모두 만족할 때
Attributed Carrier가 될 수 있다.

- 물리적으로 이동된 actor;
- dynamic prop;
- mobile office asset;
- authored large breakable piece;
- 물리적으로 분리된 authored fixture 또는 equipment element.

접촉만으로는 충분하지 않다. 다음 Carrier는 현재 Carrier의 물리적
결과로 이동 또는 방출되었음이 읽혀야 한다.

## Non-Propagating Elements

다음은 Attributed Carrier가 되거나 전파 깊이를 늘리지 않는다.

- 종이 particle, toner, dust, tiny glass와 cosmetic spark;
- visual burst, audio, camera 또는 HUD effect;
- resting contact, jitter와 vibration;
- 회복 뒤의 자발적 AI movement;
- animation-only movement;
- remote, supernatural 또는 telekinetic motion;
- floating object와 room-wide ambient destruction.

이 요소들은 표현으로 존재할 수 있지만 Origin 계보와 Momentum을
만들지 않는다.

## Meaningful Result Families

Meaningful Result는 다음 네 family 중 정확히 하나로 분류한다.

1. `ACTOR_STATE_CHANGE`
2. `OBJECT_STATE_CHANGE`
3. `BREAK_STATE_CHANGE`
4. `ROOM_STATE_CHANGE`

이 family는 observable result를 구분하는 설계 분류다. exact damage,
Stability, Knockback, ragdoll, break threshold, 이동 거리, 시간,
속도 또는 구현 enum을 승인하지 않는다.

`ROOM_STATE_CHANGE`는 국소 visibility, route, cover, furniture state,
break state 또는 usable space의 읽을 수 있는 변화를 뜻한다. 단순
접촉, 미세 떨림, 장식 반응과 상태 변화가 없는 collision은 Meaningful
Result가 아니다.

## Result Uniqueness and Duplicate Suppression

- 같은 Origin에서 같은 target과 같은 Result family 조합은 최대 한 번
  인정한다.
- 같은 target의 서로 다른 Result family는 각각 별도의 Result가 될 수
  있다.
- 서로 다른 target은 각각 별도의 Result가 될 수 있다.
- 같은 authored local room-state change는 Origin당 최대 한 번 인정한다.
- repeated impact, jitter, resting callback 또는 같은 결과의 반복 보고는
  새 Result나 reward를 만들지 않는다.

target identity, authored room-state identity, suppression key,
comparison order와 storage 방식은 G3 Draft다.

## Carrier Assignment and Cycle Prevention

- 하나의 Entity는 같은 Origin의 Carrier를 최대 한 번만 받는다.
- 각 Carrier는 같은 Origin 안에서 정확히 하나의 parent를 가진다.
- Carrier는 현재 깊이에서 서로 다른 Meaningful Result를 둘 이상 만들 수
  있다.
- 이미 같은 Origin을 받은 Entity는 더 깊은 Carrier로 다시 진입하지
  않는다.
- `A → B → A` 순환은 새 깊이, 새 Carrier assignment, duplicate Result
  또는 reward를 만들지 않는다.
- Branch는 허용하지만 Branch끼리 같은 Origin 안에서 merge하지 않는다.
- 서로 다른 Origin은 절대 하나로 merge하지 않는다.
- modifier는 Carrier assignment나 Transfer 때 복제되지 않는다.

## Multiple-Origin Conflict

- 하나의 Origin은 다른 Origin을 흡수하거나 reparent하지 않는다.
- 이미 Attributed Carrier인 Entity를 다른 Origin의 Carrier로
  reparent하지 않는다.
- 하나의 Meaningful Result는 최대 하나의 Origin에만 reward 귀속된다.
- 동일한 즉시 결과의 Origin 소유권을 읽을 수 있게 결정할 수 없으면
  world result는 유효하게 남지만 Momentum을 주지 않는다.
- 하나의 결과를 둘 이상의 Origin에 동시에 보상하지 않는다.

동시성 판정 순서, tie-break, event ordering과 ownership algorithm은
G3 Draft다.

## Branch Behavior

- 모든 Branch는 같은 Origin, 행동 출처, modifier 자격, `100 Momentum`
  cap, 최대 Depth, duplicate suppression과 termination 상태를 공유한다.
- Branch는 새로운 Origin, modifier, cap 또는 독립 reward pool을 만들지
  않는다.
- Branch는 서로 merge하거나 공유 target을 통해 Origin을 합치지 않는다.
- 한 Branch의 Depth 소진은 다른 유효 Branch를 자동 종료하지 않는다.

## Momentum Integration

Momentum/Pip은 `VERTICAL SLICE`에서만 활성화하며
[`MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md`](MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md)의
시작 profile을 그대로 따른다.

| Base category | Starting reward |
|---|---:|
| Direct meaningful Light result | `5 Momentum` |
| Direct meaningful Medium result | `10 Momentum` |
| Unique downstream meaningful result | `15 Momentum` |
| Meaningful local room-state change | `25 Momentum` |

- base precedence는 `room-state 25 > downstream 15 > direct Medium 10 >
  direct Light 5`다.
- 하나의 distinct Result에는 가장 높은 base category 하나만 적용한다.
- Ally Relay `+5`와 Perfect Catch `+5`는 각 조건을 충족한 Origin에
  각각 최대 한 번 적용한다.
- 한 Origin의 base reward와 modifier 합계는 최대 `100 Momentum`이다.
- reward cap 도달 또는 full bank 상태는 물리적 귀속과 world result를
  종료하지 않는다.
- Origin 종료 자체에는 보상이 없다.
- `HeavyAction` Origin과 그 downstream result, explicit Structural
  activation과 그 결과는 Momentum을 만들지 않는다.
- 이 계약은 어떤 reward 숫자도 변경하지 않는다.

## Player-Facing Chain Boundary

- Origin과 Initiating Carrier는 Depth `0`에서 시작한다.
- Direct Result만 있는 Origin을 player-facing `Chain`으로 표현할 의무는
  없다.
- Depth `1` 이상의 Attributed Carrier가 유효한 Downstream Result를
  만들면 player-facing Chain으로 읽힐 수 있다.
- Chain Link, 문구, HUD, VFX, audio, animation, 표시 시점과 debug
  presentation은 Draft다.

## Termination Conditions

다음 hard condition은 Origin 전체를 종료한다.

- completed `ResetEncounter`;
- room 또는 encounter lifecycle 종료;
- run 종료;
- 후속 승인된 cleanup에 의한 명시적 attribution invalidation.

다음 조건이 모두 충족되면 Origin은 자연 종료된다.

- Meaningful Result를 만들 수 있는 Attributed Carrier가 남지 않음;
- pending authored physical response가 남지 않음;
- 아직 판정되지 않은 attributable Result가 남지 않음.

짧은 정리 유예가 기술적으로 필요할 수 있으나 exact duration,
velocity, sleep, collision 또는 frame 기준은 Draft다. 최대 Depth
도달은 해당 Branch의 전파만 끝내며 다른 Branch나 Origin 전체를
자동 종료하지 않는다. reward cap 또는 full bank도 Origin을 종료하지
않는다.

## Post-Termination Rules

- Terminated Origin은 새 Carrier, Result 또는 reward를 받지 않는다.
- Terminated Origin은 후속 collision으로 다시 열리지 않는다.
- 종료 뒤의 새로운 적격 player action은 별도의 새 Origin이다.
- Ally Relay와 Perfect Catch modifier는 다음 Origin으로 넘어가지
  않는다.
- Origin 종료는 actor나 object를 삭제, 정지, 고정 또는 Reset하지
  않는다.

## Reset Boundary

- `ResetEncounter`는 모든 interaction ownership과 Origin보다 우선한다.
- completed Reset은 모든 활성 Origin을 종료하고 활성 계보를 지운다.
- Momentum/Pip은 승인된 encounter-entry resource baseline으로
  복원한다.
- 물체·actor·break·room의 encounter baseline 복원 범위는 별도 후속
  계약이며 이 문서에서 확정하지 않는다.
- Reset 자체는 Result, Chain 또는 reward를 만들지 않으며 부분 Chain을
  보존하지 않는다.

snapshot, event cleanup, ordering과 restoration implementation은 G3
Draft다.

## Sequence-Agnostic Boundary

- Chain은 적격 Origin, 물리적 Transfer, Meaningful Result와 종료 규칙을
  만족하는 한 특정 actor·object·fixture 순서를 요구하지 않는다.
- 의자→적→유리→카트→복사기 같은 고정 choreography는 승인된 규칙이
  아니다.
- PGT_03는 읽을 수 있는 국소 인과와 aftermath의 Visual Canon
  evidence이며 정확한 Chain 순서, hop 수, target 수 또는 물리 결과를
  고정하지 않는다.
- 이 계약은 고정 room, actor 수, object 수 또는 Branch 수를 요구하지
  않는다.

## Deferred Technical and Presentation Decisions

다음은 계속 `DRAFT`이며 이 계약이 승인하지 않는다.

- runtime class, component, event, field, ID, tag와 data schema;
- lineage storage, parent reference, target/result key와 branch container;
- collision callback, ordering, simultaneous-origin tie-break와 settlement;
- timeout, TTL, grace window, velocity, sleep, distance, frame와 layer 기준;
- maximum simultaneously active origins;
- pooling, cleanup implementation, performance budget와 debug tooling;
- HUD, VFX, audio, animation, camera와 UI wording;
- damage, Stability, Knockback, ragdoll, break threshold와 force;
- Unity, Rigidbody, collider, joint, physics 또는 scene implementation.

이 기술·표현 결정은 G3 또는 후속 Owner-approved presentation 작업에서
별도로 정한다.
