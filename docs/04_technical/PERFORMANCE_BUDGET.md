# Performance Budget

> **OWNER-APPROVED G3 MEASUREMENT OWNERSHIP — EXACT CEILINGS REMAIN DRAFT**
>
> Authority: `docs/04_technical/G3_TECHNICAL_BASELINE.md` —
> `## Performance and Destruction-Budget Ownership`

## Platform Boundary

Windows PC가 baseline platform이다. hardware, resolution, FPS와
pass/fail threshold는 G3 Final Audit 이후 proposal 및 G4/G6 evidence가
소유한다.

## Prototype Measurement Profile

대표 scenario는 actor 네 명(플레이어, fixed ally 한 명, Compliance
Runner 두 명), object instance 일곱 개, one room, required breakable
0개와 동시에 active한 player interaction chain 한 개다. approved depth
안의 causal branch는 허용한다.

| Category | Measurement owner |
|---|---|
| frame time | profiler capture and frame-spike log |
| fixed-step load | physics instrumentation |
| awake dynamic bodies | physical lifecycle counter |
| collision callbacks | physics callback counter |
| normalized impact facts | impact-normalization counter |
| active Origins/carriers | causal observability |
| Reset duration/diff | Reset harness |
| physics faults | fault-recovery counter |
| camera/presentation cost | presentation instrumentation |
| memory/allocation spikes | profiler capture |

이 표는 측정 범주이며 승인된 ceiling이 아니다.

## Later Slice Measurement Profile

다음은 Prototype과 별도 profile을 가진다.

- Momentum/Pip;
- HeavyAction;
- Relay Position;
- Policy lifecycle;
- conditional Suppressed;
- conditional breakable/Structural;
- additional actors, objects와 rooms.

## Superseded Draft Candidate Values

아래 과거 값은 historical tuning candidate일 뿐 Prototype baseline,
pass criterion 또는 제작 범위가 아니다.

| Item | Former ordinary target | Former burst candidate |
|---|---:|---:|
| active enemies | 3–5 | 6 |
| active allies | 1 | special presentation 2 |
| awake Dynamic Rigidbody | 20–30 | 40 |
| physical large fragments | 8–15/event | 25/room |
| Structural Feature | 1/room | 2 |
| concurrent Chain tracking | 8 | 16 |
| physical paper/small glass | 0 | 0 |

## Risk Categories

- callback storm, complex collider와 uncontrolled dynamic activation;
- free navigation, multiple ragdoll과 bulk fragment activation;
- transparent overdraw, dynamic shadow와 expensive material;
- allocation spike, Reset leak와 duplicate subscription.

## Gate

exact ceiling은 재현 가능한 scenario, capture method와 evidence를 함께
제시해야 한다. 최적화 약속이나 과거 candidate만으로 Gate를 통과하지
않는다.
