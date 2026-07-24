# Destruction Budget and Implementation Rules

> **OWNER-APPROVED DESIGN FREEZE DESTRUCTION BUDGET AND IMPLEMENTATION RULES**
>
> Decision Source: `ADR-0016`;
> `TASK-0055 delegated Owner decision`
>
> 이 문서는 Prototype의 zero-destruction profile과 Later-Slice의
> 파괴 상한 및 구현 정책을 문서 수준에서 고정한다. 아래 수치는
> `HARD DESIGN-FREEZE SCOPE CAPS`이며 성능 통과 기준, 자산 편입 승인
> 또는 구현 권한이 아니다.

## Authority and Gate Boundary

- Current Phase: `PRE-PRODUCTION / DESIGN FREEZE COMPLETION`
- G1 Requirements: `PASSED`
- G2 Design: `PASSED / OWNER-APPROVED BASELINE`
- G3 Technical: `PASSED / OWNER-APPROVED BASELINE`
- G4 Graybox: `NOT PASSED / NOT STARTED / NOT AUTHORIZED`
- G5 Freeze: `NOT PASSED`
- Full Design Lock: `INCOMPLETE`
- Graybox: `NOT STARTED`
- Implementation Gate: `CLOSED`

이 계약은 G1 Requirement wording, G2 logical contract, G3 baseline,
승인된 Visual Canon 또는 Prototype content count를 변경하지 않는다.
G4/G6 evidence는 더 낮은 cap을 제안할 수 있다. cap 증가는 Change
Request와 Owner 승인이 필요하다.

이 문서는 다음을 승인하지 않는다.

- 정확한 Breakable 자산 identity;
- PR-F-024 Structural Feature의 G5 편입;
- FPS, hardware, frame-time 또는 Reset-time 통과 기준;
- Unity component, package, class, Scene, Prefab, test 또는 구현;
- Graybox 시작, G4/G5 통과, Full Design Lock 또는 Implementation Gate
  개방.

## Definitions

### Breakable Object

하나의 승인된 authored state transition을 가진 object 또는 fixture다.

`INTACT → BROKEN`

Breakable 여부는 material, Weight Class, visual appearance, placement
또는 PGT_03의 incidental detail로 추론하지 않는다. 정확한 Breakable
content는 G5 scope 승인이 필요하다.

### Large Physical Fragment

다음을 모두 만족하는 pre-authored pooled piece다.

- bounded physical collision을 가진다;
- 후속 구현에서 Rigidbody-equivalent physical body를 사용할 수 있다;
- 읽을 수 있는 aftermath에 기여할 만큼 크다;
- physical-fragment budget에 포함된다.

### Small Material Response

particle, mesh-particle, decal, dust, paper, toner, spark 또는 small-glass
표현 요소다.

- gameplay object가 아니다;
- Rigidbody-style gameplay body가 없다;
- Causal carrier가 될 수 없다;
- small-response budget에 포함된다.

### Structural Feature

PR-F-024의 별도 room-design role이다.

- `VERTICAL SLICE / SHOULD`;
- Weight Class가 아니다;
- 다섯 번째 Causal Result Family가 아니다;
- Prototype active content가 아니다;
- 이 예산만으로 제품 또는 G5 scope에 편입되지 않는다.

### Destruction Event

normalized physical fact, logical validation과 atomic budget admission을
통과한 하나의 `BREAK_STATE_CHANGE`다. raw collision callback은 fragment,
VFX 또는 room state를 직접 만들지 않는다.

## Prototype Zero-Destruction Profile

Owner-approved Prototype Content Lock은 비파괴 상태를 유지한다.

| Prototype destruction category | Hard cap |
|---|---:|
| Authored Breakable Object instances | `0` |
| Structural Feature instances | `0` |
| Accepted Destruction Events | `0` |
| Large Physical Fragment bodies | `0` |
| Causal-carrier fragments | `0` |
| Small dynamic debris bodies | `0` |
| Destruction-specific particle elements | `0` |
| Runtime mesh-fracture operations | `0` |

Prototype의 bounded non-destructive impact feedback은 허용될 수 있다.
이는 Break Event가 아니고 fragment나 `BREAK_STATE_CHANGE`를 만들지
않으며 Structural Feature scope를 충족하지 않는다. 이 계약은 해당
비파괴 feedback의 정확한 수치를 고정하지 않는다.

예산 시스템을 시험한다는 이유로 Prototype에 breakage를 추가하지
않는다.

## Later-Slice Hard Budget Table

아래 수치는 automatic content inclusion이 아닌 authored room별 상한이다.

### Room Content Caps

| Later-Slice category | Hard cap per authored room |
|---|---:|
| Ordinary Breakable Object instances | `2` |
| Conditional Structural Feature instances | `1` |
| Ordinary Break Events | `2` |
| Conditional Structural Break Events | `1` |
| Total authored Break-State transitions | `3` |

Structural Feature가 별도 G5 scope 승인을 받지 않으면
`Structural Feature cap = 0`이다.

### Physical Fragment Caps

| Physical-fragment category | Hard cap |
|---|---:|
| Large fragments from one ordinary Break Event | `4` |
| Large fragments from one conditional Structural Event | `8` |
| Total pooled Large Fragment bodies per room | `16` |
| Simultaneously awake Large Fragment bodies per room | `12` |
| Causal-carrier Large Fragments per Break Event | `2` |
| Recursively breakable fragments | `0` |
| Player-interactive fragments | `0` |
| Small dynamic debris bodies | `0` |
| New Break Events admitted in one fixed simulation step | `1` |

더 작은 content-specific pool은 허용되지만 어느 authored room도
`16` pooled Large Fragment bodies를 초과할 수 없다.

### Small Material Response Caps

| Presentation category | Ordinary Event | Conditional Structural Event |
|---|---:|---:|
| Small material elements emitted | `32` | `64` |
| Primary Impact emitters | `1` | `1` |
| Material Burst emitters | `1` | `1` |
| Persistent Aftermath markers | `1` | `1` |

| Room-wide presentation category | Hard cap |
|---|---:|
| Concurrent live small material elements | `96` |
| Concurrent persistent Aftermath markers | `3` |
| Small responses with gameplay collision | `0` |
| Small responses with causal-carrier authority | `0` |

이 count는 engine-neutral visual element를 뜻한다. 특정 Unity Particle
System, VFX Graph, renderer, shader 또는 pooling component를 승인하지
않는다.

## Material Response Boundary

### Glass

- ordinary authored Break Event를 사용할 수 있다;
- Large Physical Fragment는 ordinary event cap 안에 있어야 한다;
- small glass는 VFX-only다;
- small-glass Rigidbody와 runtime mesh fracture는 없다.

### Wood or Composite Office Furniture

- 별도 G5 승인을 받은 경우 ordinary authored Break Event를 사용할 수
  있다;
- pre-authored large piece를 사용한다;
- dust와 splinter는 VFX-only다;
- 파손 결과가 자동으로 usable ammunition이 되지 않는다.

### Electronics

- ordinary-event fragment cap을 넘지 않는다;
- toner, spark와 screen response는 VFX-only다;
- 분리된 component는 자동으로 interactive object가 되지 않는다.

### Metal

기본 baseline은 deformation, sound와 spark response이며 free-flying
fragment를 요구하지 않는다. Metal Breakable은 explicit G5 asset
승인이 필요하고 동일한 ordinary cap을 따른다.

### Paper, Dust and Toner

- VFX-only다;
- Rigidbody-style body와 collision이 없다;
- Causal Origin 또는 Result authority가 없다.

## Authored State and Pooling

승인된 흐름은 다음과 같다.

`pre-authored intact state`
→ `approved broken state`
→ `preallocated pooled fragments`

다음을 사용하지 않는다.

- runtime mesh fracture;
- runtime Boolean cutting;
- procedural shattering;
- arbitrary wall destruction;
- recursive fragment spawning.

향후 Breakable content definition은 다음 문서 항목을 모두 가져야 한다.

- content-reference ID;
- ordinary 또는 Structural classification;
- Large Fragment count;
- Causal-carrier fragment count;
- Small Material Response cap;
- intact collider/occluder state;
- broken collider/occluder state;
- applicable room-state flag;
- Reset source state;
- owning G5 scope decision.

이는 문서 record이며 C#, serialization schema 또는 asset format이 아니다.
active Break Event 중 fragment나 emitter를 Instantiate/Destroy하지
않는다. event acceptance 전에 fragment와 emitter capacity를 전부
reserve한다.

## Atomic Break Admission

semantic order는 다음과 같다.

`Normalized Impact Fact`
→ `Break Eligibility Validation`
→ `Budget Reservation`
→ `BREAK_STATE_CHANGE`
→ `Fragment Activation`
→ `Causal Attribution`
→ `Presentation`

1. budget reservation은 atomic이다.
2. 필요한 fragment와 presentation capacity가 모두 있을 때만 Break
   Event를 수락한다.
3. partial break는 금지한다.
4. admission 실패 시:
   - object는 현재 authored state를 유지한다;
   - partial fragment set을 보이지 않는다;
   - room-state flag를 변경하지 않는다;
   - Momentum 또는 Causal Result를 만들지 않는다;
   - observable `DESTRUCTION_BUDGET_REJECTED` fault reason을 기록한다.
5. budget exhaustion은 adaptive gameplay mechanic이 아니라 authoring
   또는 implementation fault다.
6. fixed simulation step당 새 Break Event는 최대 `1`개만 수락한다.
7. 추가 valid request는 deterministic normalized-event order를
   유지하고 later step에서 다시 평가한다.

exact event name과 payload field는 provisional이다.

## Causal-Carrier Fragment Boundary

event당 최대 `2` Large Fragments만 Causal carrier가 될 수 있다.

Causal-carrier fragment는:

- 기존 Causal Origin을 이어받는다;
- parent를 정확히 하나 가진다;
- Owner-approved Chain Contract에 따라 depth를 증가시킨다;
- 승인된 Result Family를 만들 수 있다;
- maximum Chain depth `4`를 따른다;
- target/result-family uniqueness를 따른다;
- Origin을 merge하지 않는다.

다른 Large Physical Fragment는:

- approved static environment layer와만 충돌한다;
- actor 또는 gameplay prop에 영향을 주지 않는다;
- Meaningful Result를 만들지 않는다;
- Causal lineage를 갖지 않는다.

Small Material Response는 Causal lineage를 갖지 않는다. Structural
Feature는 optional role이며 `STRUCTURAL` Result Family를 만들지 않는다.
Result Family는 정확히 다음 네 개를 유지한다.

- `ACTOR_STATE_CHANGE`
- `OBJECT_STATE_CHANGE`
- `BREAK_STATE_CHANGE`
- `ROOM_STATE_CHANGE`

## Fragment Interaction and Collision

모든 destruction fragment는 다음에서 제외된다.

- Scoop;
- Carry;
- Direct Throw;
- Queued Throw;
- Catch;
- Ally Relay;
- Cargo Strap;
- HeavyAction;
- Policy Violation candidate ownership;
- persistent inventory.

fragment는 이 계약으로 새 gameplay-object definition이 되지 않는다.
future interactive broken piece에는 별도의 content-reference ID, Weight
Class, G2 compatibility review, G5 scope approval과 budget revision이
필요하다.

Large fragment는 bounded Dynamic activation과 physical continuity를
사용한다. pool에 남고 sleep 후 Reset까지 visibly settled 상태를 유지할
수 있다. silent disappearance, normal-path teleport, permanent Dynamic
또는 permanent CCD를 사용하지 않는다. exact velocity, angular velocity,
sleep와 CCD threshold는 G4 tuning evidence다.

non-finite, out-of-bounds 또는 unrecoverably penetrated fragment는
reason과 runtime identity를 기록하고 quarantine한 뒤, 안전할 때
authored baseline으로 복원한다. 안전한 복원이 불가능하면 audited
complete Scene-reload fallback을 사용한다.

## Room-State and Navigation Boundary

Break Event는 다음 authored local state만 바꿀 수 있다.

- local visibility;
- pre-authored collider state;
- pre-authored occluder state;
- 승인된 local room-state flag 하나.

다음을 사용하지 않는다.

- runtime NavMesh rebuilding;
- procedural room topology;
- arbitrary wall removal;
- room-wide collapse;
- 새 path의 동적 생성.

파손으로 열리는 movement route는 authored state pair를 사용한다. exact
route dimension과 navigation implementation은 G5/G6 책임이다.

## Conditional Structural Envelope

PR-F-024가 별도 G5 inclusion 승인을 받은 경우에만:

- room당 Structural Feature 최대 `1`;
- 해당 feature의 Structural Event 최대 `1`;
- Large Fragments 최대 `8`;
- Causal-carrier fragments 최대 `2`;
- Small Material Responses 최대 `64`;
- chained second Structural Feature 없음;
- room-wide collapse 없음;
- recursive Structural destruction 없음;
- automatic Heavy-to-Structural conversion 없음.

이 예산은 PR-F-024를 `SHOULD / PARTIAL`에서 승격하지 않는다.

## Reset and Fault Recovery

completed Reset은:

- 모든 Breakable을 `INTACT`로 복원한다;
- 조건부 Structural state를 복원한다;
- 모든 fragment를 deactivate하고 pool로 반환한다;
- 모든 destruction emitter와 persistent Aftermath marker를 지운다;
- collider와 occluder state를 복원한다;
- break-specific room-state flag를 지운다;
- fragment Causal attribution을 지운다;
- pending Break request를 지운다;
- destruction-budget reservation을 지운다;
- 새 encounter generation의 budget-fault counter를 초기화한다;
- hidden cross-reset debris inventory를 남기지 않는다.

persistent destruction save는 승인되지 않았다.

## Observability and Future Validation

후속 profiling은 최소한 다음을 측정해야 한다.

- Break admission count;
- fragment pool occupancy;
- awake fragment count;
- Causal-carrier fragment count;
- emitted/live small material count;
- collision callback count;
- normalized impact count;
- Break Event duration;
- frame/fixed-step spike;
- Reset fragment diff;
- budget rejection count.

budget rejection과 fragment fault에는 observable reason, affected runtime
identity와 encounter generation이 필요하다. exact field와 tool은
provisional이다.

## Deferred Tuning and Performance Thresholds

hard cap은 scope와 activation cap이다. 다음을 정하지 않는다.

- FPS;
- frame-time ceiling;
- fixed-step duration;
- allocation ceiling;
- target hardware;
- Reset-time acceptance;
- particle lifetime;
- velocity, force, sleep 또는 CCD threshold.

위 항목은 G4/G6 evidence 책임이다. G4/G6가 더 낮은 cap을 요구할 수
있지만 이 문서의 cap보다 높은 값은 자동 승인되지 않는다.

## Historical Authority Audit

| Existing statement | Classification after TASK-0055 |
|---|---|
| PGT_03의 국소적이고 읽을 수 있는 큰 조각과 절제된 작은 반응 | approved Visual Canon boundary; exact count authority 아님 |
| G2/G3의 Prototype required breakable/fragment `0` | approved G2/G3 policy; 이 계약의 zero profile로 유지 |
| `8–15/event`, `25/room`, Structural `1/room`과 burst `2` | superseded historical candidate |
| 일반 `3–5 enemy`와 content-like budget table | superseded historical candidate; G5/G6 별도 책임 |
| 정확한 force, velocity, sleep, CCD, route와 performance threshold | remaining G4/G5/G6 tuning |
| runtime fracture, small dynamic debris, recursive 또는 room-wide destruction | prohibited |

archive와 historical audit의 인용은 역사 증거로 남을 수 있다. 활성
문서에서는 위 과거 count를 Owner-approved cap처럼 사용할 수 없다.

## Change-Control Boundary

정확한 cap보다 작은 content profile은 허용될 수 있다. 다음에는
승인된 Change Request와 Owner 결정이 필요하다.

- 어떤 hard cap의 증가;
- interactive fragment 도입;
- runtime fracture 또는 procedural shattering 도입;
- recursive destruction;
- Structural Feature를 `MUST`로 승격;
- Prototype에 Breakable 또는 Structural Feature 편입;
- 새 Result Family;
- persistent destruction save.

이 계약은 Design Freeze 문서 잠금이며 구현 티켓이 아니다.
