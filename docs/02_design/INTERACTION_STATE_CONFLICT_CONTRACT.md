# G2 Interaction State, Arbitration, and Conflict Contract

> **OWNER-APPROVED G2 INTERACTION STATE AND CONFLICT CONTRACT**
>
> Decision Source: `TASK-0037 delegated Owner decision`; `TASK-0068 / DL-047`
>
> 이 문서는 논리 상호작용의 상태 소유권, action availability,
> cross-context arbitration, 안전한 거부와 Reset 우선순위를 승인한다.
> 물리 입력 binding, exact timing·range·buffer·target ranking,
> animation·physics·movement 수치와 Unity 구현은 승인하지 않는다.

## Authority and Scope Boundary

- 이 계약은
  [`LOGICAL_INPUT_CONTRACT.md`](LOGICAL_INPUT_CONTRACT.md)의 9개 active
  logical action과 4개 inactive Draft candidate를 변경하지 않는다.
- 이 계약은 G1 Requirement wording, Applicability, Priority,
  Authority 또는 Trace Status를 변경하지 않는다.
- 아래 상태 이름은 논리적 소유권과 충돌을 설명하는 설계 계약이다.
  runtime enum, class, component, state machine 또는 저장 형식을
  승인하지 않는다.
- `HeavyAction`은 `VERTICAL SLICE ONLY`이며 Prototype Core에
  활성화하지 않는다.
- `HeavyAction`의 시작 비용과 Momentum/Pip lifecycle은
  [`MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md`](MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md)를
  따른다.
- 이 문서의 Owner-approved 상태·중재 경계는 구현 또는 Gate 권한을
  만들지 않는다.
- TASK-0068의 C3 addendum은 G4-A에 필요한 최소 비살상
  failure/restart 논리만 추가하며 9개 active logical action, FGD-001,
  Story, Campaign과 exact implementation 값을 변경하지 않는다.
- 현재 전역 Phase, Gate, Lock, Graybox, Relay와 다음 권한 경계는
  [`docs/00_project/DESIGN_STATUS.md`](../00_project/DESIGN_STATUS.md)를
  따른다.

## Interaction State Domains

각 domain은 서로 다른 논리 책임을 가진다. 아래 이름은 구현 자료형이
아니다.

| Domain | Approved state | Logical meaning |
|---|---|---|
| Hand Ownership | `FREE` | Carry, Throw 또는 Heavy 소유권이 없는 손 상태 |
| Hand Ownership | `CARRYING` | 정확히 한 개의 직접 획득 물체를 소유 |
| Hand Ownership | `THROW_COMMITTED` | 투척 결과 해소 전 Throw가 상호작용 소유권을 가짐 |
| Hand Ownership | `HEAVY_COMMITTED` | Vertical Slice Heavy 결과 해소 전 Heavy가 상호작용 소유권을 가짐 |
| Relay | `NONE` | 활성 physical relay 없음 |
| Relay | `ACTIVE_RELAY` | 정확히 한 개의 실제 물체 공급이 진행 중 |
| Incoming Eligibility | `CATCH_ELIGIBLE` | 현재 incoming object가 Catch 결과 후보 |
| Incoming Eligibility | `STRAP_ELIGIBLE` | 현재 incoming object가 Strap Attach 결과 후보 |
| Cargo Strap | `STORED` | GEAR_01 연결이 비활성이고 수납됨 |
| Cargo Strap | `ATTACHED` | 승인된 물리 연결 chain이 유효하게 부착됨 |
| Cargo Strap | `TENSIONED` | 연결이 유지되며 리디렉션 장력이 읽히는 상태 |
| Cargo Strap | `RELEASE_REQUESTED` | 중복되지 않는 명시적 해제 요청이 처리 중 |
| Queued Throw | `CLEAR` | 도착 후 투척 의도 없음 |
| Queued Throw | `ARMED` | 하나의 Catch-eligible relay에 연결된 선행 Throw 의도 |
| Global | `PLAYABLE` | 논리 gameplay action을 받을 수 있는 상태 |
| Global | `FAILURE_PENDING` | 검증된 외부 failure 요청 뒤 비살상 presentation 경계를 해소하고 기존 Reset 장벽으로 넘기기 전까지 새 player interaction을 차단 |
| Global | `RESET_PENDING` | Reset이 모든 상호작용 소유권보다 우선하는 상태 |

Incoming object domain은 추상적으로 `CATCH_ELIGIBLE`,
`STRAP_ELIGIBLE`, 둘 다 또는 둘 다 아님을 표현할 수 있다. 현재
Owner-approved class assignment는
[`WEIGHT_CLASS_DATA_RANGE_CONTRACT.md`](WEIGHT_CLASS_DATA_RANGE_CONTRACT.md)를
따른다.

- Light valid relay profile: `CATCH_ELIGIBLE`, not
  `STRAP_ELIGIBLE`;
- mobile, floor-supported Medium with a valid load-bearing Anchor:
  `STRAP_ELIGIBLE`, not `CATCH_ELIGIBLE`;
- Heavy: neither eligibility;
- current baseline: no class is both eligible.

Object-specific dual eligibility는 별도 Owner G2 결정 없이는 활성화하지
않는다.

## Core Invariants

1. `CARRYING`, active Cargo Strap(`ATTACHED`, `TENSIONED`,
   `RELEASE_REQUESTED`), `THROW_COMMITTED`, `HEAVY_COMMITTED`는 서로
   배타적이다.
2. 플레이어는 동시에 두 개의 carried object를 소유하지 않는다.
3. `CARRYING` 중에는 Scoop, Catch와 새 Strap Attach를 시작할 수 없다.
4. Cargo Strap이 active인 동안 Carry 또는 Throw를 시작할 수 없다.
5. `HEAVY_COMMITTED`는 Carry, `ACTIVE_RELAY`, active Cargo Strap,
   `THROW_COMMITTED` 또는 `RESET_PENDING`과 공존하지 않는다.
6. `CallAlly` 요청과 `ACTIVE_RELAY`는 중첩되지 않는다.
7. `ACTIVE_RELAY` 중 unrelated Scoop은 차단한다.
8. `ACTIVE_RELAY`는 Catch, Strap Attach, 물리적 miss/passage 또는
   Reset으로만 해소한다.
9. 놓친 relay object는 실제 공간에서 계속 이동·충돌하며 사라짐,
   teleport, 공중 정지 또는 원격 제어로 해소하지 않는다.
10. `ResetEncounter`는 모든 interaction ownership보다 우선한다.
11. Scoop, Carry와 Direct Throw의 class envelope는 유효 Light와
    Medium에 한정하고, Queued Throw는 Catch-eligible Light relay에만
    적용한다.
12. normal Strap Redirect는 eligible Medium에만 적용하며 Heavy는
    Vertical Slice의 `HeavyAction`만 사용한다.
13. 검증된 Player Failure가 pending인 동안 모든 새 player interaction
    request는 차단되며 복원은 기존 exclusive Reset barrier만 수행한다.

## Action Availability Matrix

| Condition | Move | Look | Aim | InteractPrimary | Throw | CallAlly | StrapRelease | ResetEncounter | HeavyAction |
|---|---|---|---|---|---|---|---|---|---|
| Neutral / Hands Free | AVAILABLE | AVAILABLE | AVAILABLE | CONTEXT-ONLY | NO EFFECT | AVAILABLE | NO EFFECT | AVAILABLE | VERTICAL SLICE ONLY |
| Active Relay | AVAILABLE | AVAILABLE | AVAILABLE | CONTEXT-ONLY | CONTEXT-ONLY | BLOCKED | NO EFFECT | AVAILABLE | BLOCKED |
| Carrying | DRAFT RESTRICTION | AVAILABLE | AVAILABLE | BLOCKED | AVAILABLE | BLOCKED | NO EFFECT | AVAILABLE | BLOCKED |
| Cargo Strap Active — Attached/Tensioned | DRAFT RESTRICTION | AVAILABLE | AVAILABLE | BLOCKED | BLOCKED | BLOCKED | AVAILABLE | AVAILABLE | BLOCKED |
| Cargo Strap Active — Release Requested | DRAFT RESTRICTION | AVAILABLE | AVAILABLE | BLOCKED | BLOCKED | BLOCKED | NO EFFECT | AVAILABLE | BLOCKED |
| Throw Committed | DRAFT RESTRICTION | AVAILABLE | AVAILABLE | BLOCKED | NO EFFECT | BLOCKED | NO EFFECT | AVAILABLE | BLOCKED |
| Heavy Committed | DRAFT RESTRICTION | AVAILABLE | AVAILABLE | BLOCKED | BLOCKED | BLOCKED | NO EFFECT | AVAILABLE | BLOCKED |
| Failure Pending | BLOCKED | BLOCKED | BLOCKED | BLOCKED | BLOCKED | BLOCKED | BLOCKED | BLOCKED | BLOCKED |
| Reset Pending | BLOCKED | BLOCKED | BLOCKED | BLOCKED | BLOCKED | BLOCKED | BLOCKED | NO EFFECT | BLOCKED |

Controlled values:

- `AVAILABLE`: 현재 상태 소유권과 충돌하지 않는 논리 action이다.
- `CONTEXT-ONLY`: 아래 승인된 arbitration context가 있을 때만
  결과 후보가 된다.
- `BLOCKED`: 다른 상태가 상호작용 소유권을 가지므로 시작하지 않는다.
- `NO EFFECT`: 적용 가능한 상태·대상이 없거나 동일 요청이 이미
  처리 중이며, 다른 action으로 fallback하지 않는다.
- `VERTICAL SLICE ONLY`: Prototype Core에서 활성화하지 않는
  Owner-approved applicability다.
- `DRAFT RESTRICTION`: 논리 intent는 유지되지만 실제 locomotion
  제약과 응답은 후속 G2/G3 Draft다.

`Move`, `Look`, `Aim`은 Global `PLAYABLE` 상태에서 상호작용 소유권을
갖지 않는다. Carry, active Strap, Throw 또는 Heavy 중의 실제
이동·회전·조준 제약은 이 계약에서 결정하지 않는다.

## InteractPrimary Arbitration

전역적으로 `RESET_PENDING`, `THROW_COMMITTED`,
`HEAVY_COMMITTED`, `CARRYING` 또는 active Cargo Strap에서는
`InteractPrimary`를 차단한다.

`ACTIVE_RELAY`가 있을 때 다음 순서만 사용한다.

1. 플레이어가 incoming object의 유효한 하중 지지 금속 Anchor를
   명시적으로 `Aim`하고 해당 object가 `STRAP_ELIGIBLE`이면 Strap
   Attach를 시도한다.
2. 그렇지 않고 object가 `CATCH_ELIGIBLE`이며 후속 G2에서 정의할
   Catch opportunity가 유효하면 Catch를 시도한다.
3. 어느 조건도 충족하지 않으면 아무 interaction도 시작하지 않는다.

`ACTIVE_RELAY` 중에는 unrelated Scoop, 다른 object Attach 또는
context switching으로 fallback하지 않는다.

`ACTIVE_RELAY`가 없을 때 다음 순서만 사용한다.

1. 명시적으로 `Aim`한 유효 Strap Anchor가 있으면 Strap Attach를
   시도한다.
2. 명시적 Strap context가 없고 유효한 Scoop candidate가 있으면
   Scoop을 시도한다.
3. 어느 조건도 충족하지 않으면 아무 interaction도 시작하지 않는다.

명시적 Strap context가 유효하지 않더라도 Scoop으로 fallback하지
않는다. 동일 context 안에서 여러 target의 ranking은 Draft다.

## Catch Outcome Rules

- Normal Catch와 Perfect Catch attempt는 모두 `InteractPrimary`를
  사용한다. Perfect 결과 timing은 별도 G2 Draft다.
- 현재 Catch 결과는 `CATCH_ELIGIBLE` Light relay에만 적용한다.
- 성공한 Catch는 `ACTIVE_RELAY`를 해소한다.
- Queued Throw가 `CLEAR`이면 성공한 Catch는 `CARRYING`으로
  전환한다.
- Queued Throw가 `ARMED`이면 성공한 Catch는
  `THROW_COMMITTED`로 직접 전환할 수 있다.
- 정확한 손 접촉, release frame, animation, Perfect timing과
  Contact Freeze는 승인하지 않는다.

## Throw and Queued Throw Rules

- 유효 Light 또는 Medium을 `CARRYING` 중인 `Throw`는
  `THROW_COMMITTED`로 전환한다.
  Quick/Charged 구분, force와 timing은 Draft다.
- `FREE`, Cargo Strap `STORED`, `ACTIVE_RELAY`이고 incoming object가
  `CATCH_ELIGIBLE`인 Light일 때 `Throw`는 Queued Throw를 `ARMED`할
  수 있다.
- Queued Throw는 하나만 존재하며 중첩 또는 교체하지 않는다.
- relay가 성공 Catch 없이 해소되거나, Strap Attach가 성공하거나,
  Reset이 시작되거나, relay가 무효화되면 Queued Throw를 `CLEAR`한다.
- Strap Attach 성공은 queue를 지우며 Strap Attach 뒤 자동 Throw를
  만들지 않는다.

## CallAlly and Relay Rules

`CallAlly`는 `PLAYABLE`, Hand Ownership `FREE`, Cargo Strap
`STORED`, Relay `NONE`이고 Throw/Heavy가 committed되지 않았을
때만 한 명의 active ally에게 하나의 `ACTIVE_RELAY`를 요청할 수 있다.
추가 수락 조건과 physical supply authority는
[`ALLY_RELAY_SUPPRESSED_CONTRACT.md`](ALLY_RELAY_SUPPRESSED_CONTRACT.md)의
[`## Request-Acceptance Conditions`](ALLY_RELAY_SUPPRESSED_CONTRACT.md#request-acceptance-conditions)을
따른다.

차단된 `CallAlly`는 queue, 기존 relay 교체, carried object drop,
Strap 자동 해제, 다른 class/object fallback 또는 두 번째 relay를
만들지 않는다. `CallAlly`는 ally 선택, class cycle, Relay Position
이동 또는 두 번째 ally 명령이 아니다.

`ACTIVE_RELAY`는 다음 두 design phase를 가진다. 이 phase는 새 runtime
state 이름이 아니다.

- `PRE-RELEASE`: 요청 수락과 한 실제 물체의 reservation 이후, 물체가
  아직 동료 control을 떠나지 않은 상태;
- `RELEASED`: 물체가 동료 control을 떠나 실제 trajectory에 진입한
  상태.

한 시점에는 하나의 `ACTIVE_RELAY`와 한 개의 예약 물체만 존재한다.
`PRE-RELEASE`는 물체·경로·position 무효화, Suppressed, Reset 또는
lifecycle 종료에서만 안전하게 abort한다. abort는 reservation과
relay를 해소하고 물체를 실제 지지 상태로 남기며 unrelated fallback,
Origin 또는 Momentum을 만들지 않는다.

`RELEASED` 물체는 Catch, Strap Attach, 실제 공간의 miss/passage,
world interception 또는 Reset으로 해소한다. Catch나 Attach에
실패해도 삭제·고정·원격 steering되지 않고 물리 경로를 계속 따른다.
release 뒤 ally가 Suppressed되어도 이미 진행 중인 물체, player input,
유효한 provenance 또는 이후 player-owned 행동을 취소하지 않는다.

동료 support availability는 `READY`, `SUPPLY_COMMITTED`,
`RELOCATING`, `SUPPRESSED` 네 design state로 구분한다. 정확히 하나만
관찰되며 runtime enum 또는 구현 state machine을 승인하지 않는다.
`RELOCATING`과 `SUPPRESSED`에서는 새 `CallAlly`를 수락하지 않는다.

class delivery profile은 다음과 같다.

- Light: airborne physical throw and Catch-oriented relay;
- mobile Medium: floor-supported roll, push 또는 grounded feed and
  Strap-oriented relay;
- Heavy: normal `CallAlly` relay 없음.

static 또는 부적합한 Medium은 direct-acquisition-only일 수 있다.

## Cargo Strap State Rules

Strap Attach는 다음 조건을 모두 요구한다.

- Global `PLAYABLE`;
- Hand Ownership `FREE`;
- Cargo Strap `STORED`;
- 명시적 `Aim`;
- 바닥 지지를 유지하고 실제로 이동 가능한 `STRAP_ELIGIBLE` Medium;
- 유효한 하중 지지 금속 Anchor.

물리 연결은
`허리 릴 → 검정·황색 스트랩 → 은색 카라비너 → 교체식 금속 클램프
→ 하중 지지 금속 Anchor`를 유지한다. 캐스터, 휠 축, 플라스틱
하우징, 취약 돌출부와 암적색 넥타이는 Anchor가 아니다. 원격 견인,
grapple, 공중 부유와 Heavy 장거리 견인은 허용하지 않는다.

Cargo Strap이 `ATTACHED`, `TENSIONED` 또는 `RELEASE_REQUESTED`이면
`InteractPrimary`, `Throw`, `CallAlly`, `HeavyAction`을 차단하고
`StrapRelease`만 해당 장비 context에서 사용할 수 있다.
`Move`, `Look`, `Aim` intent는 유지하되 실제 movement constraint는
Draft다.

`StrapRelease`는 `ATTACHED` 또는 `TENSIONED`에서 하나의
`RELEASE_REQUESTED`로 전환할 수 있다. 이미
`RELEASE_REQUESTED`이면 추가 요청은 `NO EFFECT`이며 중첩하지
않는다. 해제 완료 후 `STORED`로 돌아간다. 정확한 해제 timing,
mechanism, 자동 Safety Release rule, retraction과 구현은 Draft다.

## Scoop Rules

Scoop은 `PLAYABLE`, Hand Ownership `FREE`, Cargo Strap `STORED`,
Relay `NONE`, Throw/Heavy non-committed, 명시적 Strap context 없음,
유효한 Light 또는 Medium Scoop candidate를 모두 요구한다. 성공하면
`CARRYING`으로 전환한다. Heavy는 normal Scoop candidate가 아니다.

정확한 range, same-context ranking, 사용 손, alignment, animation,
개별 object eligibility는 Draft다. 물체를 순간이동하거나 hidden
inventory로 바꾸는 결과는 허용하지 않는다.

## HeavyAction Exclusivity

`HeavyAction`은 `VERTICAL SLICE ONLY`다. `PLAYABLE`, Hand Ownership
`FREE`, Relay `NONE`, Cargo Strap `STORED`, Queued Throw `CLEAR`이고
Throw/Heavy non-committed인 neutral state에서만 시작할 수 있으며
Banked Pip이 최소 `2`여야 한다.

state conflict 또는 자원 부족으로 차단된 요청은 `0` Pip을 소비한다.
유효한 요청이 `HEAVY_COMMITTED`에 진입할 때 `2 Banked Pips`를
소비하며, ordinary post-commit interruption은 자동 refund를 만들지
않는다. 시작 후 `HEAVY_COMMITTED`가 결과 해소 전 상호작용 소유권을
독점한다.

Heavy의 유일하게 승인된 player-action entry point는 `HeavyAction`이다.
HeavyAction origin과 그 downstream result는 Momentum을 만들지 않는다.
exact 대상, Structural interaction, physical mass, force, locomotion,
animation, physics, final cost tuning과 구현은 Draft다.

## Player Failure and Encounter-Restart Addendum

이 addendum은 `PR-F-025`의 active G4-A 범위를 현재 encounter에
한정한다. Player Failure는 logical input이 아니며 열 번째 action을
추가하지 않는다. 요청은 active encounter-pressure owner가 외부
pressure 조건을 검증한 뒤에만 제출할 수 있다.

승인된 논리 순서는 정확히 다음과 같다.

```text
PLAYABLE
→ validated external failure request
→ FAILURE_PENDING
→ nonlethal failure presentation boundary
→ RESET_PENDING
→ authored encounter-entry baseline
→ PLAYABLE
```

- failure는 비살상이며 player를 죽이지 않는다. Health, Death, Life,
  Continue 또는 Campaign Game Over system을 승인하지 않는다.
- exact damage, Stability, threshold, accumulation, recovery와 pressure
  판정은 `DRAFT`다.
- pending 상태에서는 모든 새 player interaction request를 차단한다.
- Queued Throw, active Relay, Carry, Cargo Strap, Throw와 Heavy ownership은
  기존 Reset barrier를 통해서만 해소·복원한다.
- failure는 자동 `StrapRelease`를 만들지 않는다.
- failure request, pending 상태와 restart 자체는 Momentum/Pip, Causal
  Origin 또는 Meaningful Result를 만들지 않는다.
- manual `ResetEncounter`와 automatic failure restart는 서로 다른
  initiating cause지만 같은 authored encounter-entry baseline과 같은
  exclusive restoration barrier를 사용한다.
- Fade, Bark, camera, animation, delay와 정확한 presentation timing은
  `DRAFT`다.

Campaign에서 완료된 Story, rescue, As-Built/map과 shortcut 진행을
보존하는 목표는 FGD-001의 승인된 full-game target으로 유지하지만
active G4-A behavioral pass나 이 addendum의 구현 범위가 아니다.

## ResetEncounter Priority and Shared Restoration Barrier

`ResetEncounter`는 Prototype validation의 최상위 action이다. 유효한
요청은 `RESET_PENDING`으로 전환하고 모든 새 interaction을 차단한다.
반복 Reset 요청은 중첩되지 않고 `NO EFFECT`다.

automatic failure restart도 비살상 presentation 경계 뒤 동일한
`RESET_PENDING` restoration barrier로 진입한다. 이는 새 logical input이
아니며 manual Reset의 initiating cause와 구분된다.

Reset 완료는 Relay를 `NONE`, Queued Throw를 `CLEAR`, Hand
Ownership을 `FREE`, Cargo Strap을 `STORED`로 복원하고 Carry,
Throw와 Heavy ownership을 해소한다. 또한 completed Reset은 encounter
진입 시점의 `encounter-entry Momentum/Pip resource baseline`으로
복원한다.

정확한 binding, hold/confirmation, UI, reset time, snapshot/save,
event와 구현은 Draft다.

## Failure and Safe-Rejection Policy

- 유효하지 않은 action은 unrelated action으로 fallback하지 않는다.
- 유효하지 않은 Strap context는 Scoop으로 바뀌지 않는다.
- 유효하지 않은 Catch context는 Strap Attach로 바뀌지 않는다.
- 차단된 `CallAlly`는 두 번째 relay를 만들지 않는다.
- Catch는 이미 carried object를 떨어뜨리지 않는다.
- Throw는 active Strap을 자동 해제하지 않는다.
- Strap Attach는 `CARRYING` 중 사용할 수 없으므로 Carry를 자동
  취소하지 않는다.
- 놓친 relay object는 실제 물리 경로를 계속 따른다.
- 실패는 telekinesis, teleport, suspended object, hidden inventory
  또는 silent equipment swap으로 해결하지 않는다.
- 정확한 UI, VFX, sound, animation과 feedback timing은 Draft다.

## Deferred Decisions

다음은 이 Owner-approved 계약 이후에도 Draft다.

- same-context target ranking;
- interaction distance와 angle;
- Normal/Perfect Catch timing;
- Queued Throw arm, consume와 cancel timing;
- input buffer와 cancel window;
- physical input binding과 tap/hold/release interpretation;
- Quick/Charged 해석;
- Carry/Strap/Throw/Heavy 중 locomotion constraint;
- automatic Safety Release;
- object-specific eligibility exception과 exact object assignment;
- final Momentum/Pip balance와 Heavy cost tuning;
- resource snapshot, persistence와 spend implementation;
- ally supply ranking, exact zone·path·timing과 feedback;
- exact Relay Position count·placement·route·navigation;
- Suppressed threshold·duration·recovery·enemy pressure 판정;
- 외부 Knockback, incapacity와 recovery interaction;
- failure pressure owner의 exact damage·Stability·threshold·accumulation;
- failure Fade·Bark·camera·animation·delay와 recovery timing;
- Campaign progress persistence, save schema와 restart implementation;
- animation과 physics;
- implementation class, component와 technical data structure;
- Unity Input System과 Unity implementation.

`Sprint`, `Evade`, `Vault`, `Shove`는 계속 비활성 Draft candidate다.
Light/Medium/Heavy functional envelope과 class-based eligibility는
[`WEIGHT_CLASS_DATA_RANGE_CONTRACT.md`](WEIGHT_CLASS_DATA_RANGE_CONTRACT.md)에서
Owner-approved다. exact mass·size·speed·force·damage·timing·Momentum/
Pip·break value와 구현은 계속 Draft다.
