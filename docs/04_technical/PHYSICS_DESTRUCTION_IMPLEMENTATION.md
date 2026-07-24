# Physics and Destruction Implementation

> **OWNER-APPROVED G3 POLICY — EXACT TUNING AND IMPLEMENTATION REMAIN DRAFT**
>
> Authority: `docs/04_technical/G3_TECHNICAL_BASELINE.md` —
> `## Physics Activation, Collision, and Fault Recovery` and
> `## Cargo Strap Technical Envelope`

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

Prototype은 breakable, Structural Feature와 uncontrolled ragdoll을
요구하지 않는다. 기존 파손-prefab, fragment, NavMesh 갱신과 ragdoll
구상은 later conditional design이며 active Prototype implementation
flow가 아니다. unrestricted destruction은 금지한다.

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
