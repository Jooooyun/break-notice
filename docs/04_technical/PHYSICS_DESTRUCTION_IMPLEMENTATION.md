# Physics and Destruction Implementation

> **OWNER-APPROVED G3 POLICY + OWNER-APPROVED DESIGN FREEZE
> DESTRUCTION CONTRACT — EXACT TUNING AND IMPLEMENTATION REMAIN DRAFT**
>
> Authority:
> - `docs/04_technical/G3_TECHNICAL_BASELINE.md` —
>   `## Physics Activation, Collision, and Fault Recovery`;
>   `## Cargo Strap Technical Envelope`
> - `docs/04_technical/DESTRUCTION_BUDGET_AND_IMPLEMENTATION_RULES.md`
> - `docs/09_adrs/ADR-0016-BOUNDED-DESTRUCTION-BUDGET.md`

## Bounded Rigidbody Activation

object는 authored safe state에서 시작하며 interaction 또는 measured
trajectory risk가 있을 때만 bounded Dynamic window에 들어간다. ownership
transition과 physical release는 명시적으로 기록한다. 모든 object를
영구 Dynamic/CCD로 두지 않는다.

## Collision Detection Policy

CCD는 실제 trajectory risk와 profiling 근거로 선택한다. primitive 또는
simple compound collider가 기본이다. Discrete, Continuous Speculative,
Continuous Dynamic의 구체 배정과 collider 수는 tuning candidate이며
승인값이 아니다.

raw callback은 normalized collision fact만 만든다. Momentum, UI,
audio 또는 unrelated system을 직접 변경하지 않는다.

## Physical Continuity

released object는 hidden inventory, floating state 또는 teleport 없이
연속된 실제 경로를 유지한다. safe velocity/angular-velocity policy는
필수지만 정확한 cap은 later evidence다.

## Fault Detection and Recovery

- out-of-bounds, invalid ownership, non-finite state와 unrecoverable
  penetration을 관찰 가능한 fault로 기록한다.
- reason, affected runtime identity와 counter를 남긴다.
- recovery는 quarantine 또는 authored baseline restore를 사용한다.
- object를 기록 없이 숨기거나 정상 경로에서 teleport하지 않는다.
- unsafe restore는 audited complete Scene reload fallback으로 넘긴다.

## Prototype Destruction Boundary

Prototype의 Breakable, Structural Feature, Break Event, Large Fragment,
Causal-carrier fragment, small dynamic debris, destruction-specific
particle와 runtime fracture cap은 모두 `0`이다. non-destructive impact
feedback은 breakage가 아니며 Prototype에 파괴를 추가하지 않는다.

## Authored Destruction State

Later-Slice의 conditional destruction은 다음 순서를 사용한다.

`pre-authored intact state`
→ `approved broken state`
→ `preallocated pooled fragments`

- runtime mesh fracture, Boolean cutting, procedural shattering와 recursive
  destruction을 사용하지 않는다.
- active Break Event 중 fragment나 emitter를 Instantiate/Destroy하지
  않는다.
- bounded Dynamic activation과 physical continuity를 유지한다.
- fragment는 pool에 남고 Reset 전까지 settled aftermath로 보일 수 있다.
- permanent Dynamic/CCD, silent disappearance와 normal-path teleport를
  사용하지 않는다.

## Atomic Admission and Exact Caps

`Normalized Impact Fact → Break Eligibility Validation → Budget Reservation
→ BREAK_STATE_CHANGE → Fragment Activation → Causal Attribution
→ Presentation` 순서를 사용한다.

- budget reservation은 atomic이며 partial break를 허용하지 않는다.
- capacity 부족 시 현재 authored state와 room state를 유지하고
  observable `DESTRUCTION_BUDGET_REJECTED`를 기록한다.
- fixed simulation step당 새 Break Event는 최대 `1`개다.
- ordinary/conditional Structural event의 Large Fragment cap은 각각
  `4`/`8`이다.
- room pool/awake cap은 `16`/`12`다.
- event당 Causal-carrier fragment는 최대 `2`다.
- small dynamic debris, recursively breakable fragment와
  player-interactive fragment는 `0`이다.

## Fragment Collision and Interaction

Causal-carrier fragment만 기존 Origin, parent 하나와 승인된 Chain
depth/uniqueness를 이어받을 수 있다. non-carrier fragment는 approved
static environment layer와만 충돌하며 actor, gameplay prop, Meaningful
Result와 lineage에 영향을 주지 않는다.

모든 destruction fragment는 Scoop, Carry, Throw, Catch, Relay, Cargo
Strap, HeavyAction, Policy candidate와 persistent inventory에서
제외된다. 파손된 가구를 자동 ammunition으로 만들지 않는다.

## Room State, Reset and Fault

breakage는 pre-authored collider/occluder pair와 approved local room-state
flag 하나만 변경할 수 있다. runtime NavMesh rebuilding, procedural
topology, arbitrary wall destruction와 room-wide collapse는 금지한다.

Reset은 intact/Structural state, pool, emitter, marker, collider,
occluder, room-state flag, fragment attribution, pending request,
reservation과 fault counter를 baseline으로 복원한다.

non-finite, out-of-bounds 또는 unrecoverable fragment는 observable reason과
runtime identity를 남긴 뒤 quarantine/baseline restore를 사용하고,
안전하지 않으면 audited complete Scene reload fallback을 사용한다.

## Conditional Structural Profile

PR-F-024가 별도 G5 승인을 받은 경우에만 room당 Structural Feature와
Structural Event는 각각 최대 `1`, Large Fragment는 최대 `8`,
Causal-carrier fragment는 최대 `2`, Small Material Response는 최대
`64`다. 두 번째 chained Structural Feature, recursive Structural
destruction, room-wide collapse와 automatic Heavy-to-Structural
conversion은 없다. PR-F-024는 계속 `SHOULD / PARTIAL`이다.

## Cargo Strap Envelope

- simplified segmented visual;
- logical attachment constraint;
- bounded redirect response;
- load-bearing metal Anchor 검사;
- eligible floor-supported Medium 검사;
- active 중 interaction exclusion;
- explicit player `StrapRelease`;
- completed release 후 stored state.

general-purpose rope, grapple, remote/Heavy towing, caster 또는 wheel-axis
Anchor, plastic housing Anchor, necktie attachment와 automatic Safety
Release는 승인되지 않았다. exact force equation, tension, distance,
angle과 release threshold는 later tuning이다.
