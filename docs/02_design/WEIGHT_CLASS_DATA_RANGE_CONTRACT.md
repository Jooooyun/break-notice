# G2 Weight-Class Functional Data Range Contract

> **OWNER-APPROVED G2 WEIGHT-CLASS FUNCTIONAL DATA RANGE CONTRACT**
>
> Decision Source: `TASK-0038 delegated Owner decision`
>
> 이 문서는 `LIGHT`, `MEDIUM`, `HEAVY`의 player-facing 기능 범위와
> class-based interaction eligibility만 승인한다. 정확한 질량·크기·
> 속도·힘·피해·timing·final Momentum/Pip tuning·animation·physics·
> 기술 데이터 구조와 Unity 구현은 승인하지 않는다.

## Authority and Scope Boundary

- 활성 Weight Class는 정확히 `LIGHT`, `MEDIUM`, `HEAVY` 세 개다.
- Weight Class는 기능적 handling burden, delivery method,
  interaction eligibility, first-order physical result와 gameplay
  role을 분류한다.
- 이 계약은 G1 Requirement wording, Applicability, Priority,
  Authority, Trace Status 또는 Acceptance Test를 변경하지 않는다.
- [`LOGICAL_INPUT_CONTRACT.md`](LOGICAL_INPUT_CONTRACT.md)의 9개 active
  logical action과
  [`INTERACTION_STATE_CONFLICT_CONTRACT.md`](INTERACTION_STATE_CONFLICT_CONTRACT.md)의
  state·arbitration·Reset 규칙을 변경하지 않는다.
- Weight Class별 delivery eligibility는 이 문서가 정하고, 실제
  selection, reservation, release, abort와 Suppressed 중 실행은
  [`ALLY_RELAY_SUPPRESSED_CONTRACT.md`](ALLY_RELAY_SUPPRESSED_CONTRACT.md)가
  정한다.
- `HEAVY`의 active use는 `VERTICAL SLICE ONLY`이고 Prototype Core는
  식별 가능한 후보를 보여줄 수 있을 뿐 사용·자원 소비·공간 변화
  성공을 요구하지 않는다.
- `STRUCTURAL FEATURE`는 네 번째 Weight Class가 아니라 별도
  room-design role이다.
- G2 Design은 계속 `NOT PASSED / CURRENT BASELINE WORK`이며
  Implementation Gate는 `CLOSED`다.

## Classification Principle

Weight Class는 kilogram만으로 정하지 않는다. 다음 player-facing
기능 envelope를 함께 판단한다.

- 물체를 다루는 부담;
- 플레이어 또는 동료가 물체를 전달하는 방식;
- Scoop, Carry, Throw, Catch, Strap과 HeavyAction eligibility;
- 비슷한 setup에서의 first-order physical result;
- 국소 room-state에서 기대되는 역할.

모든 active object definition은 정확히 하나의 Weight Class를 가진다.
한 object ID가 두 Weight Class에 동시에 속할 수 없다.

비슷한 setup 조건에서 first-order physical response는 다음 상대
순서를 유지해야 한다.

`LIGHT < MEDIUM < HEAVY`

- Light는 통상 Medium 또는 Heavy의 direct result를 만들지 않는다.
- Medium은 통상 Heavy의 direct result를 만들지 않는다.
- Heavy는 약한 Light 반응처럼 읽히지 않는다.
- speed, height, geometry, environment와 Behavior Tag는 특정 결과를
  증폭할 수 있지만 runtime에 Weight Class를 바꾸지 않는다.
- Light가 큰 downstream Causal Chain을 시작해도 Light에서 Medium
  또는 Heavy로 재분류되지 않는다.

## Approved Data Dimensions

다음은 Owner-approved G2 design dimension이다.

- `Weight Class`
- `Applicability`
- `Handling Burden`
- `Direct Acquisition Eligibility`
- `Carry Eligibility`
- `Direct Throw Eligibility`
- `Queued Throw Eligibility`
- `Ally Relay Delivery Profile`
- `Catch Eligibility`
- `Strap Eligibility`
- `HeavyAction Eligibility`
- `First-Order Impact Role`
- `Local Room-State Role`

이는 C# field, enum, ScriptableObject schema, database column,
serialized asset 또는 Unity component를 승인하지 않는다.

## Class Envelope Matrix

| Weight Class | Scoop | Carry | Direct Throw | Queued Throw | Ally Delivery | Catch Eligible | Strap Eligible | HeavyAction |
|---|---|---|---|---|---|---|---|---|
| LIGHT | YES | YES | YES | YES | AIRBORNE THROW | YES | NO | NO |
| MEDIUM | CONDITIONAL YES | YES | YES | NO | FLOOR ROLL / PUSH WHEN MOBILE | NO | CONDITIONAL YES | NO |
| HEAVY | NO | NO | NO | NO | NO NORMAL RELAY | NO | NO | YES — VERTICAL SLICE ONLY |

`CONDITIONAL YES`인 Medium은 해당 interaction에 물리적으로 적합한
유효 object여야 한다. Strap eligibility에는 floor support, 실제
mobility, 유효한 load-bearing metal Anchor와 읽을 수 있는 redirect
path가 모두 필요하다.

## LIGHT Functional Envelope

### Applicability and Handling

- `PROTOTYPE CORE`에서 active다.
- 별도 제거 결정이 없는 한 후속 milestone에서도 active다.
- compact하고 handling burden이 낮은 object다.
- one-hand-capable handling으로 표현할 수 있으나 정확한 손·pose와
  animation은 Draft다.

### Acquisition, Carry, Throw, and Relay

- 유효 Light는 Scoop으로 직접 획득할 수 있다.
- 승인된 interaction contract에 따라 한 번에 한 object만 Carry한다.
- Direct Throw eligible이다.
- 유효한 Catch-oriented relay로 공급될 때 Queued Throw eligible이다.
- 동료는 실제 airborne physical throw로 공급한다.
- 물체는 실제 trajectory를 따르며 spawn, teleport, hover 또는 remote
  control되지 않는다.
- release와 miss를 포함한 실행 경계는 Ally Relay and Suppressed
  Contract의 `## LIGHT Supply Envelope`를 따른다.

### Eligibility and Result Role

- 기본 유효 relay profile은 `CATCH_ELIGIBLE`이다.
- 승인 baseline에서 `STRAP_ELIGIBLE`이 아니다.
- normal Cargo Strap Redirect 대상이 아니다.
- `HeavyAction` eligible이 아니다.
- first-order impact role은 interruption, setup, Guard 또는 Stability
  disturbance, small physical reaction과 downstream Causal Chain
  initiation이다.
- direct impact 하나만으로 major displacement, Structural 결과 또는
  Heavy 수준의 room change를 만들 필요가 없다.
- downstream chain이 국소 room-state를 바꿀 수 있으나 그 결과가
  Light를 재분류하지 않는다.

## MEDIUM Functional Envelope

### Applicability and Handling

- `PROTOTYPE CORE`에서 active다.
- readable causal displacement의 primary class다.
- committed two-hand 또는 whole-body handling burden을 가진다.
- 정확한 animation과 movement restriction은 Draft다.

### Acquisition, Carry, Throw, and Relay

- 유효하게 정지했거나 직접 도달 가능한 Medium은 Scoop할 수 있다.
- Carry eligible이며 한 번에 한 object만 소유한다.
- Carry에서 Direct Throw eligible이다.
- direct result는 committed short physical launch 또는 displacement로
  해소될 수 있으나 Quick/Charged interpretation, speed, force와
  animation은 Draft다.
- Queued Throw eligible이 아니며 airborne ally throw를 Catch하지
  않는다.
- mobile Medium의 ally delivery는 floor-supported roll, push 또는
  grounded feed만 사용한다.
- static 또는 부적합한 Medium은 ally-delivery profile 없이
  direct-acquisition-only일 수 있다.
- 동료는 GEAR_01을 부착하거나 리디렉션하지 않으며 exact 실행 경계는
  Ally Relay and Suppressed Contract의 `## MEDIUM Supply Envelope`를
  따른다.

### Eligibility and Result Role

- ally-fed mobile Medium은 기본적으로 `STRAP_ELIGIBLE` 후보이며
  `CATCH_ELIGIBLE`이 아니다.
- Strap eligibility는 floor support, actual mobility, load-bearing
  metal Anchor와 physically readable redirect path를 모두 요구한다.
- caster, wheel axis, plastic housing 또는 fragile protrusion은
  Anchor가 아니다.
- 모든 Medium이 자동으로 Strap eligible인 것은 아니다.
- 유효한 조건을 갖춘 Medium은 normal Cargo Strap Redirect class다.
- `HeavyAction` eligible이 아니다.
- first-order impact role은 meaningful target displacement, furniture
  movement, Causal Chain creation과 secondary collision이다.
- direct result 또는 짧은 chain으로 국소 visibility·route·room-state
  change를 만들 수 있으나 Structural Feature 파괴를 자동 승인하지
  않는다.

## HEAVY Functional Envelope

### Applicability and Handling

- active use는 `VERTICAL SLICE ONLY`다.
- Prototype Core에는 알아볼 수 있는 Heavy 후보가 있을 수 있으나
  interaction, consumption, cost 또는 성공 결과를 요구하지 않는다.
- normal Scoop과 normal Carry를 사용할 수 없다.
- floor-supported, environmentally supported 상태를 유지하거나 dedicated
  Heavy interaction을 요구한다.

### Throw, Relay, Eligibility, and Result Role

- normal Direct Throw와 Queued Throw를 사용할 수 없다.
- normal `CallAlly` relay로 던지거나 굴리거나 밀어 공급하지 않는다.
- 동료의 자동 preparation, 고정부 해제 또는 alignment assistance는
  승인된 기능이 아니며 미래 별도 Owner 결정 전까지 Draft다.
- `CATCH_ELIGIBLE` 또는 normal `STRAP_ELIGIBLE`이 아니다.
- GEAR_01로 remote tow, long-range redirect, room traversal pull,
  grapple 또는 airborne movement를 만들 수 없다.
- future limited Heavy equipment assistance에는 별도 Owner 결정이
  필요하다.
- `HeavyAction`이 유일하게 승인된 player-action entry point이며
  `VERTICAL SLICE ONLY`다.
- Owner-approved 시작 비용은 `2 Banked Pips`이며 정확한 spend와
  lifecycle은
  [`MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md`](MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md)를
  따른다.
- HeavyAction origin과 그 downstream result는 Momentum을 만들지
  않는다.
- final cost tuning과 execution은 G4/G6 evidence 전까지 Draft다.
- first-order role은 strong posture collapse, major object
  displacement, high-commitment payoff와 강한 국소 space-breaking
  결과다.
- room-wide collapse 또는 unrestricted destruction을 승인하지 않는다.
- Heavy는 자동으로 Structural Feature가 되지 않는다.

## Catch and Strap Eligibility Assignment

기존 Interaction State Contract는 추상 object가 Catch eligible, Strap
eligible, 둘 다 또는 둘 다 아님일 수 있는 domain을 정의한다. 이
계약의 class baseline은 다음으로 제한한다.

- Light relay profile: Catch only;
- mobile, floor-supported, valid-anchor Medium relay profile: Strap only;
- Heavy: neither.

현재 승인된 baseline class 중 Catch와 Strap에 동시에 eligible인
class는 없다. object-specific dual-eligibility exception은 별도 Owner
G2 결정이 필요하다.

## Carry, Throw, and Relay Envelope

- Scoop과 direct Carry는 유효 Light와 Medium에만 적용한다.
- Direct Throw는 carried Light와 valid Medium에만 적용한다.
- Queued Throw는 Catch-eligible Light relay에만 적용한다.
- Light ally delivery는 airborne physical throw다.
- mobile Medium ally delivery는 floor-supported roll, push 또는
  grounded feed다.
- Heavy는 normal relay, Scoop, Carry 또는 Throw를 사용하지 않고
  Vertical Slice의 `HeavyAction`만 사용한다.
- exact input timing, target ranking, movement restriction, delivery
  path와 physical force는 Draft다.

## Impact and Room-State Envelope

| Weight Class | First-Order Impact Role | Local Room-State Role |
|---|---|---|
| LIGHT | setup, interruption, small physical reaction | downstream chain을 시작할 수 있으나 Heavy·Structural 결과를 직접 요구하지 않음 |
| MEDIUM | displacement, chain creation, secondary collision | direct result 또는 짧은 chain으로 의미 있는 국소 변화 가능 |
| HEAVY | high-commitment major displacement와 local space-breaking payoff | 강한 국소 변화 가능, room-wide collapse는 금지 |

정확한 damage, Stability change, knockback distance, 파손 형태,
통로 치수와 reward는 이 hierarchy의 승인 범위가 아니다.

## Structural Feature Separation

- `STRUCTURAL FEATURE`는 네 번째 Weight Class가 아니다.
- PR-F-024가 관리하는 별도 room-design role이다.
- Heavy object는 자동으로 Structural Feature가 아니다.
- Structural Feature는 자동으로 `HeavyAction` usable 대상이 아니다.
- Structural Feature의 Pip cost는 승인되지 않았다.
- Heavy와 Structural Feature의 다른 관계는 별도 후속 G2 결정이
  필요하다.
- 이 계약은 room-wide collapse, 모든 벽 파괴 또는 unrestricted
  destruction을 승인하지 않는다.

## Behavior Tag Orthogonality

`Rolling`, `Slab`, `Rebound`, `Fragile`, `Powered`, `Anchored`,
`Crush` 같은 Behavior Tag는 Weight Class와 직교한다.

- `Rolling`은 Strap eligibility를 자동 부여하지 않는다.
- `Fragile`은 Medium을 Catch eligible로 만들지 않는다.
- `Anchored`는 object를 자동 Heavy로 만들지 않는다.
- `Powered`는 추가 damage, VFX 또는 Chain reward를 승인하지 않는다.
- `Crush`는 room-wide destruction을 승인하지 않는다.

Tag는 physical character를 바꿀 수 있지만 class boundary를 조용히
우회할 수 없다. exact tag behavior는 후속 설계 Draft다.

## Class Assignment and Transition Rules

1. visual size만이 아니라 intended player-facing functional envelope로
   class를 배정한다.
2. direct handling role과 direct impact role이 충돌하면 object를
   redesign하지 않는 한 더 높은 handling burden을 사용한다.
3. 하나의 object ID는 안정적인 Weight Class 하나만 가진다.
4. speed, height 또는 collision context는 result magnitude를 바꿀 수
   있으나 runtime Weight Class를 바꾸지 않는다.
5. anchored 또는 supported state는 Weight Class를 바꾸지 않는다.
6. break 후 independently interactive fragment는 자체 candidate object
   ID와 Weight Class가 필요하다.
7. parent object는 breakage만으로 Weight Class가 자동 변경되지 않는다.
8. exact object ID, content count와 milestone inclusion은 별도 Prototype
   Enemy, Room, Object Content Lock 작업 전까지 Draft다.

## Illustrative Non-Binding Examples

다음은 기능 envelope를 설명하는 non-binding candidate 예시다.

- Light: file binder, keyboard, compact office object;
- Medium: wheeled office chair, monitor, rolling records cart;
- Heavy: copier-class asset, steel cabinet.

이 예시는 exact object list, count, model, dimension, room placement,
Behavior Tag, Prototype inclusion 또는 Vertical Slice inclusion을
lock하지 않는다. FIN_01의 conference table은 이 계약으로 제품 범위나
Prototype object로 승격되지 않는다.

## Deferred Numeric and Technical Values

다음은 명시적으로 `DRAFT`다.

- kilogram과 physical dimension;
- Rigidbody mass, drag와 center of mass;
- collider와 collision mode;
- throw velocity, force, impulse와 angular velocity;
- direct damage, Stability value와 knockback distance;
- Contact Freeze;
- break threshold와 debris count;
- Chain multiplier;
- final Momentum reward, Pip capacity와 HeavyAction cost tuning;
- movement penalty와 rotation restriction;
- exact target ranking과 interaction timing;
- animation과 IK;
- physics;
- data architecture;
- Unity implementation.

기존 exact value는 later G2/G3/G4 Draft candidate로 명확히 표시된
상태에서만 유지한다.
