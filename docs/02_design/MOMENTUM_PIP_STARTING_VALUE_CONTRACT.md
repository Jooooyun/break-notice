# G2 Momentum/Pip Starting-Value and Resource Lifecycle Contract

> **OWNER-APPROVED G2 MOMENTUM/PIP STARTING PROFILE**
>
> Decision Source: `TASK-0039 delegated Owner decision`
>
> 아래 수치는 `OWNER-APPROVED G2 STARTING VALUES — SUBJECT TO
> G4/G6 EVIDENCE-BASED TUNING`이다. 수치 변경에는 검증 근거, 명시적
> tuning revision과 관련 문서 동기화가 필요하다. 이 계약은 G2 통과,
> Graybox 시작, 기술 설계 또는 Unity 구현을 승인하지 않는다.

## Authority and Scope Boundary

- 이 계약은 `PR-F-010`, `PR-F-011`, `PR-F-023`을 위한 Momentum/Pip
  시작 profile과 자원 lifecycle만 승인한다.
- G1 Requirement wording, Applicability, Priority, Authority,
  Trace Status와 Acceptance Test를 변경하지 않는다.
- 기존 9개 logical action, interaction state 이름, Weight-Class
  envelope과 Visual Canon을 변경하지 않는다.
- Chain의 logical 생성·전파·종료·깊이·중복 억제는
  [`CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md)가
  소유한다. 이 Momentum/Pip 계약은 Chain runtime lineage, TTL,
  event 또는 구현을 승인하지 않는다.
- 정확한 Perfect Catch timing, Contact Freeze, damage, force,
  animation, physics, HUD, VFX, audio, data schema와 Unity 구현은
  승인하지 않는다.
- G2 Design은 `NOT PASSED / CURRENT BASELINE WORK`이고
  Implementation Gate는 `CLOSED`다.

## Applicability

- Momentum/Pip은 `VERTICAL SLICE`에서 활성화한다.
- `PROTOTYPE CORE`에서는 Momentum 획득, Pip 변환·저장·소비와
  `HeavyAction`을 활성화하지 않는다.
- Prototype Core에는 식별 가능한 Heavy 후보가 있을 수 있으나,
  실제 Heavy 사용, Pip 소비 또는 Heavy 결과는 통과 조건이 아니다.
- 이 계약의 수치는 Prototype Acceptance criterion이 아니다.

## Resource Representation

Momentum/Pip은 다음 두 논리 자원으로 표현한다.

| Resource | Approved logical range | Meaning |
|---|---:|---|
| Current Momentum | `0` through `99` after conversion | 다음 Pip 변환을 향한 현재 진행도 |
| Banked Pip | `0` through `3` | `HeavyAction`에 사용할 수 있는 저장 단위 |

이 표현은 integer type, field, class, component, serialized asset,
save schema, widget 또는 다른 구현 자료형을 승인하지 않는다.

## Starting Values

- run 시작 Current Momentum: `0`
- run 시작 Banked Pip: `0`
- Pip 변환 threshold: `100 Momentum`
- 최대 Banked Pip: `3`
- 네 번째 Pip 또는 별도 초과 자원은 만들지 않는다.

## Momentum-to-Pip Conversion

유효한 reward를 Current Momentum에 더한 뒤 다음 순서로 처리한다.

1. Banked Pip이 `3` 미만이고 Current Momentum이 `100` 이상인 동안,
   Current Momentum에서 `100`을 빼고 Banked Pip을 `1` 올린다.
2. 변환 후 `100` 미만의 remainder는 보존한다.
3. Banked Pip이 `3`이면 현재 remainder는 보존하되 새 reward 증가는
   Pip을 소비할 때까지 무시한다.
4. Current Momentum과 Banked Pip은 음수가 될 수 없다.

이 순서는 논리 결과 계약이며 반복문, event, 데이터형 또는
실행 순서를 승인하지 않는다.

## Eligible Player-Owned Origins

다음 origin만 유효한 physical result가 발생한 뒤 reward 후보가 된다.

- player direct Light Throw;
- player direct valid Medium Throw;
- Light Catch 뒤의 player Queued Throw;
- eligible Medium에 대한 player GEAR_01 Redirect.

`CallAlly`가 공급한 물체는 플레이어가 Catch 뒤 Throw/Queued Throw를
수행하거나 유효한 GEAR_01 Redirect를 수행해 행동 소유권을 얻은 뒤에만
reward origin이 될 수 있다. 동료가 공급한 물체의 우연한 결과가
플레이어 소유 행동 전에 발생하면 reward를 주지 않는다.
relay 수락, reservation, `PRE-RELEASE`, `RELEASED`와 abort의 권한은
[`ALLY_RELAY_SUPPRESSED_CONTRACT.md`](ALLY_RELAY_SUPPRESSED_CONTRACT.md)의
`## Momentum and Causal Provenance`를 따른다.

## Base Reward Categories

`PR-F-012` 또는 `PR-F-016`에 해당하는 읽을 수 있는 meaningful result에
다음 시작값을 적용한다.

| Base category | Starting reward |
|---|---:|
| Direct meaningful Light result | `5 Momentum` |
| Direct meaningful Medium result | `10 Momentum` |
| Unique downstream meaningful result | `15 Momentum` |
| Meaningful local room-state change | `25 Momentum` |

단순 접촉, 미세 떨림, 장식 반응 또는 상태 변화가 없는 collision은
meaningful result가 아니다.

## Reward Precedence

base category 우선순위는 다음과 같다.

`room-state change 25 > downstream result 15 > direct Medium 10 >
direct Light 5`

- 하나의 distinct result에는 base category를 최대 하나만 적용한다.
- 여러 category에 해당하면 가장 높은 category 하나만 적용한다.
- 동일 origin에서 나중에 발생한 별도의 distinct meaningful result는
  별도 base reward 후보가 될 수 있다.
- distinct result 판정, causal attribution과 duplicate suppression은
  Causal Chain Contract의
  [`## Direct and Downstream Classification`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md#direct-and-downstream-classification)과
  [`## Result Uniqueness and Duplicate Suppression`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md#result-uniqueness-and-duplicate-suppression)을
  따른다.

## Ally Relay Modifier

다음 조건을 모두 충족하면 origin당 정확히 한 번 `+5 Momentum`을
적용한다.

- 물체가 승인된 `ACTIVE_RELAY`를 통해 진입했다.
- relay 요청이 수락되고 예약 물체가 `RELEASED` phase에 진입했다.
- 플레이어가 Catch 뒤 Throw/Queued Throw 또는 GEAR_01 Redirect로
  후속 행동을 소유했다.
- 그 player-owned origin이 하나 이상의 유효한 reward result를
  만들었다.

`CallAlly` 요청, wind-up, supply, Catch 또는 Attach만으로는 reward를
주지 않는다. `PRE-RELEASE` abort와 player ownership 전의 accidental
result도 reward나 provenance를 만들지 않는다. release 뒤 ally가
Suppressed되어도 이미 유효한 provenance 또는 modifier 자격을
제거하지 않는다. modifier는 hop이나 collision마다 반복하지 않는다.

## Perfect Catch Modifier

후속 승인된 Perfect Catch가 발생하고 그 origin의 player-owned
후속 행동이 하나 이상의 유효한 reward result를 만들면 origin당
정확히 한 번 `+5 Momentum`을 적용한다.

- Catch 자체에는 reward를 주지 않는다.
- Perfect Catch 판정 창, threshold, Contact Freeze, animation과
  follow-up 시간은 이 계약에서 승인하지 않는다.
- 같은 origin이 조건을 충족하면 Ally Relay modifier와 Perfect Catch
  modifier를 함께 적용할 수 있다.

## Per-Origin Reward Cap

- 한 causal origin의 base rewards와 modifiers 합계는 최대
  `100 Momentum`이다.
- cap에 도달한 뒤에도 world result는 계속 발생할 수 있지만 같은
  origin에서 추가 Momentum 또는 Pip을 얻지 않는다.
- causal origin, propagation depth, branch와 종료는 Causal Chain
  Contract의 [`## Propagation Depth`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md#propagation-depth),
  [`## Branch Behavior`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md#branch-behavior)와
  [`## Termination Conditions`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md#termination-conditions)을
  따른다. reward cap은 물리적 attribution을 종료하지 않는다.

## HeavyAction Cost and Spend

- `HeavyAction` 시작 비용은 `2 Banked Pips`다.
- `HeavyAction`은 `VERTICAL SLICE ONLY`이며 Banked Pip이 `2` 미만이면
  사용할 수 없다.
- interaction state와 충돌해 차단된 요청은 `0` Pip을 소비한다.
- 유효한 요청이 `HEAVY_COMMITTED`에 들어갈 때 `2` Pips를 소비한다.
- commit 이후의 ordinary interruption은 자동 refund를 만들지 않는다.
- completed Reset은 아래 resource snapshot 규칙을 따른다.

정확한 Heavy 선택, 이동, force, impact, animation, IK, physics,
duration과 cancel behavior는 승인하지 않는다.

## Heavy and Structural Anti-Self-Funding

- `HeavyAction` origin과 그 downstream result는 Momentum을 만들지
  않는다.
- `Structural Feature`는 `PR-F-024`의 별도 room-design role이며
  Heavy와 자동 연결되지 않는다.
- explicit Structural activation 자체와 그 결과에는 이 계약으로
  Momentum을 주지 않는다.
- Structural Feature의 Pip cost는 승인하지 않는다.
- 역사적 `Structural Feature: 3 Pip` 후보는 superseded Draft이며
  활성 authority가 아니다.

## Resource Persistence and Reset

- 새 run은 Current Momentum `0`, Banked Pip `0`으로 시작한다.
- 활성 run 안의 room transition은 Current Momentum과 Banked Pip을
  보존한다.
- encounter 진입 시점의 Current Momentum과 Banked Pip을 논리적
  Reset baseline으로 취급한다.
- completed `ResetEncounter`는 encounter-entry Momentum/Pip baseline을
  복원한다.
- Reset으로 같은 reward를 반복 farming할 수 없다.
- run 종료 또는 새 run 시작은 Current Momentum과 Banked Pip을
  `0 / 0`으로 만든다.
- passive decay, ordinary damage, Knockback 또는 ordinary interruption은
  Current Momentum이나 Banked Pip을 잃게 하지 않는다.

snapshot 저장 형식, persistence component, save system과 event
구현은 G3 Draft다.

## Non-Reward Events

다음은 Momentum을 만들지 않는다.

- `CallAlly` request, wind-up 또는 supply만 발생한 상태;
- Catch만 발생한 상태;
- Strap Attach 또는 Strap Release만 발생한 상태;
- cosmetic VFX, audio 또는 camera response;
- repeated micro-contact, resting contact, jitter 또는 duplicate result;
- invalid 또는 blocked interaction;
- player-owned follow-through 전의 ally accidental result;
- `HeavyAction` origin과 그 downstream result;
- explicit Structural activation과 그 결과;
- 현재 causal origin과 무관한 environment reaction;
- Reset 자체.

## Arithmetic Examples

다음은 계산 설명용이며 actor, room, object, Chain 순서 또는 콘텐츠를
lock하지 않는다.

- direct meaningful Light result: `5`
- direct meaningful Medium result: `10`
- ally-fed Perfect Catch Light가 유효한 direct result를 만든 경우:
  `5 + 5 + 5 = 15`
- ally-fed Redirect origin이 direct Medium result, unique downstream
  result와 meaningful room-state change를 각각 만든 경우:
  `10 + 15 + 25 + 5 = 55`
- 긴 origin의 계산 합계가 `100`을 넘는 경우: reward는
  `100 per origin`에서 멈춘다.

## Feedback Boundary

후속 presentation은 최소한 다음 상태를 구분할 수 있어야 한다.

- Current Momentum progress;
- Banked Pip count;
- maximum Pip capacity;
- `HeavyAction` cost;
- insufficient-resource rejection;
- full-bank state.

이 계약은 HUD layout, color, icon, animation, sound, VFX, screen
position 또는 controller feedback을 승인하지 않는다.

## Chain Contract Boundary, Technical, and Final-Tuning Decisions

Causal Origin, Direct/Downstream, Depth `0`~`4`, Branch, result
uniqueness, duplicate·cycle prevention과 termination의 logical rule은
[`CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md)를
따른다.

다음은 계속 `DRAFT`다.

- runtime lineage field, ID, event, parent storage와 technical data schema;
- TTL, natural-termination grace와 duplicate-suppression algorithm;
- simultaneous-origin ordering과 settlement implementation;
- Perfect Catch timing과 Contact Freeze;
- damage, force, physics와 animation;
- HUD, VFX, audio와 feedback implementation;
- Unity component, data schema, event contract와 save implementation;
- final reward values, threshold, Pip capacity, HeavyAction cost와 cap.

G4/G6 evidence는 위 시작값의 조정을 제안할 수 있다. 수치 변경에는
문서화된 playtest 또는 validation evidence, 명시적 tuning revision과
동기화된 계약·상태 갱신이 필요하다. Momentum의 존재, Pip model,
HeavyAction resource 관계 또는 Prototype/Vertical Slice applicability
변경은 ordinary tuning이 아니라 별도 Owner G2 scope decision이다.
