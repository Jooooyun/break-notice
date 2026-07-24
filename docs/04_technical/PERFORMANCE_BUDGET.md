# Performance Budget

> **OWNER-APPROVED G3 MEASUREMENT OWNERSHIP + OWNER-APPROVED
> DESTRUCTION SCOPE CAPS**
>
> Authority:
> - `docs/04_technical/G3_TECHNICAL_BASELINE.md` —
>   `## Performance and Destruction-Budget Ownership`
> - `docs/04_technical/DESTRUCTION_BUDGET_AND_IMPLEMENTATION_RULES.md`
> - `docs/09_adrs/ADR-0016-BOUNDED-DESTRUCTION-BUDGET.md`

## Platform Boundary

Windows PC가 baseline platform이다. hardware, resolution, FPS와
pass/fail threshold는 G4/G6 evidence가 소유한다.

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

## Prototype Zero-Destruction Profile

Prototype의 Breakable Object, Structural Feature, Break Event, Large
Physical Fragment, Causal-carrier fragment, small dynamic debris,
destruction-specific particle와 runtime fracture cap은 모두 `0`이다.
non-destructive impact feedback은 destruction으로 분류하지 않는다.

## Later-Slice Destruction Upper-Envelope Profile

| Category | Owner-approved hard scope cap |
|---|---:|
| Ordinary Breakable instances per room | `2` |
| Conditional Structural Feature instances per room | `1` |
| Total authored Break-State transitions per room | `3` |
| Ordinary/Structural Large Fragments per event | `4` / `8` |
| Pooled/awake Large Fragments per room | `16` / `12` |
| Causal-carrier fragments per event | `2` |
| Small dynamic debris | `0` |
| Ordinary/Structural small material elements per event | `32` / `64` |
| Concurrent live small material elements per room | `96` |
| Persistent Aftermath markers per event/room | `1` / `3` |
| New Break Events per fixed simulation step | `1` |
| Runtime mesh fracture | `PROHIBITED` |

Structural Feature가 별도 G5 scope 승인을 받지 않으면 cap은 `0`이다.
cap은 automatic content inclusion이 아니다.

## Measurement Ownership

Later-Slice destruction profiling은 최소한 다음을 측정한다.

- Break admission과 rejection;
- fragment pool occupancy와 awake count;
- Causal-carrier fragment count;
- emitted/live small material count;
- collision callback과 normalized impact count;
- Break Event duration과 frame/fixed-step spike;
- Reset fragment diff와 fault count.

다음 later capability도 Prototype과 별도 profile을 가진다.

- Momentum/Pip;
- HeavyAction;
- Relay Position;
- Policy lifecycle;
- conditional Suppressed;
- conditional breakable/Structural;
- additional actors, objects와 rooms.

## G4/G6 Performance Criteria Boundary

TASK-0055 count는 hard scope/activation cap이다. FPS, frame-time,
fixed-step duration, allocation, target hardware, Reset-time acceptance,
particle lifetime 또는 velocity/force threshold가 아니다. G4/G6 evidence는
더 낮은 cap을 요구할 수 있다. 증가는 Change Control과 Owner 승인이
필요하다.

## Superseded Historical Candidate

아래 과거 값은 historical tuning candidate일 뿐 Prototype baseline,
pass criterion, 제작 범위 또는 active authority가 아니다.

| Item | Former ordinary target | Former burst candidate |
|---|---:|---:|
| active enemies | 3–5 | 6 |
| active allies | 1 | special presentation 2 |
| awake Dynamic Rigidbody | 20–30 | 40 |
| physical large fragments | 8–15/event | 25/room |
| Structural Feature | 1/room | 2 |
| concurrent Chain tracking | 8 | 16 |
| physical paper/small glass | 0 | 0 |

현재 active destruction cap은
`DESTRUCTION_BUDGET_AND_IMPLEMENTATION_RULES.md`만 따른다.

## Risk Categories

- callback storm, complex collider와 uncontrolled dynamic activation;
- free navigation, multiple ragdoll과 bulk fragment activation;
- transparent overdraw, dynamic shadow와 expensive material;
- allocation spike, Reset leak와 duplicate subscription.

## Gate

성능 통과 기준은 재현 가능한 scenario, capture method와 evidence를
함께 제시해야 한다. hard scope cap, 최적화 약속이나 과거 candidate만으로
G4/G6 Gate를 통과하지 않는다.
