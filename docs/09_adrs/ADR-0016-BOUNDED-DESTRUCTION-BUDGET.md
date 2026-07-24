# ADR-0016 — Bounded Authored Destruction Budget

- Status: Accepted
- Date: 2026-07-25
- Decision Source: `TASK-0055 delegated Owner decision`
- Scope: documentation-level Design Freeze budget and implementation
  policy

## Context

PGT_03 Visual Canon은 국소적이고 통제된 파괴, 제한적이고 읽을 수
있는 큰 조각과 절제된 작은 반응을 승인했지만 정확한 수량과 구현은
승인하지 않았다. G2 Prototype Content Lock은 breakable, Structural,
debris와 destruction VFX를 제외하며, G3 baseline은 Prototype와
Later-Slice measurement ownership만 분리했다. 과거 문서의
`8–15/event`, `25/room` 및 Structural 후보 수치는 Draft였다.

Design Freeze를 위해 Prototype의 zero profile과 Later-Slice의 상한,
authored state, pooling, admission, causal, Reset 및 fault policy를
하나의 Owner-approved 계약으로 고정할 필요가 있다.

## Decision

세부 계약은
`docs/04_technical/DESTRUCTION_BUDGET_AND_IMPLEMENTATION_RULES.md`를
따른다.

### Prototype

Prototype은 다음을 모두 `0`으로 유지한다.

- Breakable Object;
- Structural Feature;
- accepted Destruction Event;
- Large Physical Fragment;
- Causal-carrier fragment;
- small dynamic debris;
- destruction-specific particle element;
- runtime mesh fracture.

bounded non-destructive impact feedback은 destruction으로 분류하지
않으며 Prototype에 breakage를 추가하지 않는다.

### Later-Slice Upper Envelope

- ordinary Breakable: room당 최대 `2`;
- conditional Structural Feature: 별도 G5 승인 시 room당 최대 `1`;
- ordinary/Structural Break Event: 각각 room당 최대 `2`/`1`;
- total authored Break-State transitions: room당 최대 `3`;
- ordinary/Structural Large Fragment: event당 최대 `4`/`8`;
- pooled/awake Large Fragment: room당 최대 `16`/`12`;
- Causal-carrier Large Fragment: event당 최대 `2`;
- small dynamic debris, recursively breakable fragment와
  player-interactive fragment: `0`;
- ordinary/Structural Small Material Response: event당 최대 `32`/`64`;
- concurrent Small Material Response: room당 최대 `96`;
- Primary Impact, Material Burst와 persistent Aftermath marker: event당
  각각 `1`;
- concurrent persistent Aftermath marker: room당 최대 `3`;
- fixed simulation step당 새 Break Event admission: 최대 `1`.

### State, Pool and Admission

- pre-authored `INTACT → BROKEN` state pair를 사용한다;
- fragment와 emitter는 preallocated pool을 사용한다;
- active Break Event 중 Instantiate/Destroy하지 않는다;
- runtime mesh fracture, Boolean cutting, procedural shattering와
  recursive destruction을 사용하지 않는다;
- 필요한 capacity 전부를 atomic reserve한 뒤에만 event를 수락한다;
- partial break를 허용하지 않는다;
- 실패 시 authored state와 room state를 유지하고
  `DESTRUCTION_BUDGET_REJECTED` reason을 기록한다.

### Causal and Interaction Boundary

event당 최대 두 Large Fragment만 기존 Origin을 이어받는 Causal
carrier가 될 수 있다. 다른 Large Fragment는 approved static environment
layer와만 충돌하고 Meaningful Result 또는 lineage를 만들지 않는다.
모든 fragment는 Scoop, Carry, Throw, Catch, Relay, Cargo Strap,
HeavyAction, Policy candidate와 persistent inventory에서 제외된다.

Result Family는 `ACTOR_STATE_CHANGE`, `OBJECT_STATE_CHANGE`,
`BREAK_STATE_CHANGE`, `ROOM_STATE_CHANGE`의 정확한 네 개를 유지한다.
Structural Feature는 Result Family가 아니다.

### Room, Reset and Fault Boundary

파손은 pre-authored collider/occluder pair와 local room-state flag
하나만 변경할 수 있다. runtime NavMesh rebuilding, procedural topology,
arbitrary wall destruction와 room-wide collapse는 금지한다.

Reset은 intact/Structural state, pool, emitter, marker, collider,
occluder, room-state flag, attribution, pending request, reservation과
fault counter를 encounter baseline으로 복원한다. persistent destruction
save는 승인하지 않는다.

non-finite, out-of-bounds 또는 unrecoverable fragment는 observable
reason과 identity를 남기고 quarantine/baseline restore를 사용한다.
안전하지 않으면 audited complete Scene-reload fallback을 사용한다.

## Structural Boundary

PR-F-024는 계속 `VERTICAL SLICE / SHOULD / PARTIAL`이다. 별도 G5
승인이 없으면 Structural cap은 `0`이며, 이 ADR은 exact Structural
asset identity, Heavy-to-Structural conversion 또는 제품 편입을
승인하지 않는다.

## Performance Boundary

위 count는 `HARD DESIGN-FREEZE SCOPE CAPS`다. FPS, hardware,
frame-time, fixed-step, allocation, Reset-time 또는 tuning pass threshold가
아니다. G4/G6 evidence는 더 낮은 cap을 요구할 수 있다. 증가는 Change
Request와 Owner 승인이 필요하다.

## Consequences

- historical `8–15/event`, `25/room`, 일반 `3–5 enemy`와 Structural
  candidate 표는 active authority가 아닌 superseded history로 남는다.
- final Breakable identity, conditional Structural inclusion과 exact asset
  list는 G5 Draft다.
- exact physics/VFX threshold와 performance criterion은 G4/G6 evidence다.
- no Unity project, package, code, Scene, Prefab, test 또는 Graybox
  execution authority가 생기지 않는다.
- G4와 G5는 `NOT PASSED`, Full Design Lock은 `INCOMPLETE`, Graybox는
  `NOT STARTED`, Implementation Gate는 `CLOSED`다.
