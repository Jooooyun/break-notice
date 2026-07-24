# G3 Technical Baseline

> **OWNER-APPROVED G3 TECHNICAL BASELINE — G3 PASSED / IMPLEMENTATION NOT AUTHORIZED**
>
> Decision Source: `ADR-0015`, `DEC-G3-001` through `DEC-G3-013`
>
> 이 문서는 기술 책임과 설계 정책의 Owner-approved 기준선이다.
> 실제 Unity 프로젝트, package resolution, 구현, 테스트 실행, Graybox
> 또는 Gate 통과를 승인하지 않는다.

## Authority and Gate Boundary

- Current Phase: `PRE-PRODUCTION / DESIGN FREEZE COMPLETION`
- G1 Requirements: `PASSED`
- G2 Design: `PASSED / OWNER-APPROVED BASELINE`
- G3 Technical: `PASSED / OWNER-APPROVED BASELINE`
- G3 Gate Passage Source:
  `docs/05_production/G3_TECHNICAL_BASELINE_REAUDIT.md` at
  `9e1487e85be5a54ba0824c209deabcf738e1abf0`
- G4 Graybox: `NOT PASSED / NOT STARTED / NOT AUTHORIZED`
- G5 Freeze: `NOT PASSED`
- Design Freeze: `28 / 32`
- Full Design Lock: `INCOMPLETE`
- Graybox: `NOT STARTED`
- Implementation Gate: `CLOSED`

이 기준선은 exact requirement, G2 contract와 Visual Canon을 변경하지
않는다. 정확한 구현 class, tuning, 성능 ceiling, hardware, pass
threshold와 생성된 Unity/package 파일은 별도 권한과 검증 대상이다.
역사적 Final Audit은 `REVISION REQUIRED`로 유지되며 Re-Audit의
`READY FOR G3 GATE DECISION` 판정은 Owner-approved G3 passage의
근거로 소비되었다. G3 passage는 구현 또는 테스트 완료를 뜻하지
않는다.

## Exact Editor and Package Lock

다음은 Bootstrap 전에 사용할 정확한 문서 기준 lock이다. floating
version은 허용하지 않는다.

| Identifier | Exact lock | Package role | Official evidence verified 2026-07-25 |
|---|---|---|---|
| Unity Editor | `6000.3.20f1` | Editor Core | [Unity 6000.3.20f1 release page](https://unity.com/releases/editor/whats-new/6000.3.20f1) |
| `com.unity.render-pipelines.universal` | `17.3.0` | Editor-matched Core Package | [Universal Render Pipeline 17.3.0](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.3/index.html) |
| `com.unity.inputsystem` | `1.20.0` | Separately installed Released Package | [Input System 1.20.0](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/index.html) |
| `com.unity.cinemachine` | `3.1.7` | Separately installed Released Package | [Cinemachine 3.1.7](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/index.html) |
| `com.unity.test-framework` | `1.6.0` | Editor-matched Core Package | [Unity Test Framework 1.6.0](https://docs.unity3d.com/Packages/com.unity.test-framework@1.6/manual/index.html) |

Unity 6.3 LTS와 URP의 Editor 계열 일치는 [Unity 6.3 Manual의 URP
소개](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/urp-introduction.html)에서도
교차 확인했다. 실제 설치와 transitive dependency는 이 문서에서
추측하지 않으며, 나중에 생성되는 package lock을 그대로 기록한다.

AI Navigation은 Prototype package baseline에 포함하지 않는다. 승인된
Relay Position 구현 wave가 열릴 때까지 `DEFERRED`다.

다음 package 또는 framework는 승인되지 않았다.

- Addressables;
- DOTS 또는 Entities;
- Netcode;
- third-party rope simulation;
- third-party behavior tree;
- third-party save framework;
- generic service locator 또는 dependency-injection framework;
- generic global event-bus framework;
- persistent telemetry 또는 runtime analytics framework.

추가에는 Change Request와 명시적 근거가 필요하다.

## Prototype and Later-Scope Separation

### Prototype Technical Scope

첫 구현 wave가 나중에 승인될 경우에도 다음 기술 소유권만 포함한다.

- authored Prototype room 한 개;
- player 한 명, fixed-position ally 한 명, Compliance Runner 두 명;
- interactive 또는 recognition content instance 일곱 개;
- logical input adapter, locomotion과 gameplay camera;
- interaction arbitration과 singular ownership;
- Scoop, Carry, Throw와 Queued Throw;
- physical Light supply와 floor-supported Medium supply;
- explicit Cargo Strap attach, redirect와 release;
- normalized impact와 depth-limited causal attribution;
- 이해 가능한 room-state change 한 개;
- Reset, observability와 fault recovery.

### Later Vertical Slice

다음은 first implementation wave 밖의 명시적 later owner다.

| Capability | Later technical owner |
|---|---|
| Perfect Catch | Interaction and Ownership + Test and Observability |
| Momentum/Pip | later Progression domain + Causal Attribution |
| HeavyAction | later Interaction/Progression integration |
| Relay Position transition | later Ally Supply and Navigation integration |
| Policy Violation lifecycle/content | later Run/Policy state owner |

### Optional, Conditional, Deferred, or OUT

Suppressed, Behavior Tags, Structural Feature, breakables, LGT_01,
FIN_01, Incident Report, persistent gameplay save, CHR_02, extra active
allies, additional enemy archetypes와 multi-room production sequence는
first-wave dependency graph 밖이다. 제외 기능만을 위한 speculative
interface는 만들지 않는다.

LGT_01은 Records Archive-local `COULD`, FIN_01은 `OUT / COULD`를
유지한다.

## Architecture and Dependency Direction

문서 수준의 책임 domain은 다음과 같다. 정확한 class 이름은
provisional이다.

| Domain | Authoritative responsibility |
|---|---|
| Composition and Encounter Bootstrap | explicit construction, encounter dependencies와 lifecycle root |
| Logical Input Adapter | approved logical intent를 platform input에서 분리 |
| Player Locomotion and Camera Adapter | movement와 one-way camera presentation adapter |
| Interaction and Ownership | arbitration, safe rejection, Carry/Throw ownership |
| Physical Object Lifecycle | activation, physical continuity와 ownership transfer |
| Ally Supply and Reservation | room-local pool, singular reservation와 physical release |
| Cargo Strap | valid attach, active constraint, redirect와 explicit release |
| Impact Normalization | raw collision을 normalized domain fact로 변환 |
| Causal Attribution and Room State | Origin/carrier/result와 room-state flag |
| Reset and Fault Recovery | exclusive Reset, baseline restore, quarantine와 fallback |
| Presentation Adapters | domain fact를 camera/VFX/audio/UI로 표현 |
| Test and Observability | harness ownership, trace fields와 counters |

규칙:

- mutable state마다 authoritative owner는 하나다.
- dependency는 explicit construction과 reference로 연결한다.
- generic service locator, global mutable static registry와 primary
  global event bus를 사용하지 않는다.
- Simulation과 Gameplay은 UI, Camera, VFX 또는 Audio에 의존하지 않는다.
- Presentation은 domain fact를 소비할 뿐 gameplay state를 변경하지 않는다.
- raw physics callback은 Momentum, UI 또는 unrelated system을 직접
  변경하지 않는다.
- optional/later system은 Prototype dependency graph에 들어오지 않는다.

Evade, Vault, fixed Catch/Strap/Scoop priority, settled Quick/Charged
mode, generic ally inventory, automatic Strap release와 first-wave
Momentum/Breakable/Upgrade/Incident 책임은 이 기준선의 active assumption이
아니다.

## Definition Data and Runtime State Ownership

다음 identity category를 분리한다.

- stable content-reference identity;
- transient runtime-entity identity;
- encounter 또는 Reset generation identity;
- Causal Origin identity;
- Result uniqueness identity.

구체 GUID, integer width와 serialization format은 미정이다.

규칙:

- shared definition data에는 runtime state를 저장하지 않는다.
- runtime state는 하나의 owner만 가진다.
- content ID, runtime instance ID, Origin ID와 Result key는 서로
  대체할 수 없다.
- Carry ownership, Strap ownership과 Relay reservation은 각각 singular다.
- encounter state가 room-state flag와 Reset baseline을 소유한다.
- persistent save는 Prototype runtime physics를 소유하지 않는다.

## Ordered Request, Fact, Result, and Presentation Flow

승인된 semantic order는 다음과 같다.

`Request` → `Validation` → `Ownership Commit` → `Physical Action or Physical Fact`
→ `Normalized Domain Fact` → `Domain Result` → `Presentation`

Reset은 새 request와 결과 처리를 막는 exclusive lifecycle barrier다.

Semantic event family:

- interaction request/rejection;
- ownership commit/release;
- Throw commit/physical release;
- Queued Throw armed/cleared;
- Relay reservation, pre-release abort와 physical release;
- Strap attach, redirect, release request와 release completed;
- normalized impact;
- Causal Origin creation과 carrier transfer;
- meaningful Result record와 Origin termination;
- room-state change;
- fault detection/recovery;
- Reset requested/started/completed;
- later Policy와 Momentum lifecycle.

이름과 payload 형식은 provisional이다. 각 family는 explicit owner,
deterministic ordering, 필요한 idempotency, duplicate suppression와
reentrancy boundary를 가진다. presentation-to-gameplay backflow와 raw
collision의 cross-system mutation은 금지한다.

## Causal Identity and Result Uniqueness

- Origin은 initiating carrier와 current attributed carrier를 구분한다.
- Origin은 parent 하나, depth와 provenance를 가진다.
- Result family는 정확히 다음 네 종류를 사용한다.
  - `ACTOR_STATE_CHANGE`
  - `OBJECT_STATE_CHANGE`
  - `BREAK_STATE_CHANGE`
  - `ROOM_STATE_CHANGE`
- 동일 Origin의 uniqueness는 compound target/result-family로 판정한다.
- branch는 같은 Origin을 공유할 수 있지만 Origin merge는 금지한다.
- ambiguous multi-Origin result는 reward를 받지 않는다.
- terminated Origin은 다시 열리지 않는다.
- Reset은 모든 active attribution을 지운다.
- Structural Feature는 별도의 optional design role이며 Result family가
  아니다.

구체 collection type과 payload 구조는 승인하지 않는다.

## Physics Activation, Collision, and Fault Recovery

- Dynamic activation은 필요한 object와 bounded window에만 적용한다.
- CCD는 측정된 trajectory risk에 따라 선택한다.
- collider는 primitive 또는 단순 compound를 기본으로 한다.
- collision은 먼저 normalized fact가 된 뒤 domain 결과를 만든다.
- released object는 teleport 또는 hidden inventory 없이 물리적 연속성을
  유지한다.
- ownership transition은 명시적으로 기록한다.
- fault는 관찰 가능한 reason과 counter를 남긴다.
- recovery는 silent disappearance가 아니라 quarantine 또는 authored
  baseline restore를 사용한다.
- velocity와 angular-velocity safety policy는 두되 정확한 값은 미정이다.

Prototype에는 breakable, Structural Feature와 uncontrolled ragdoll을
넣지 않는다. 모든 object를 영구 Dynamic/CCD로 두거나 floating object,
숨김 inventory, unrestricted destruction, 정상 경로의 teleport recovery를
사용하지 않는다.

## Cargo Strap Technical Envelope

Prototype Cargo Strap은 다음에 한정한다.

- simplified segmented visual;
- logical attachment constraint;
- bounded redirect response;
- load-bearing metal Anchor validity check;
- eligible floor-supported Medium check;
- active 중 다른 interaction exclusion;
- player의 explicit `StrapRelease`;
- completed release 후 stored state 복귀.

general-purpose rope, grapple, remote 또는 Heavy towing, caster/wheel-axis
또는 plastic-housing Anchor, necktie attachment와 automatic Safety
Release는 승인하지 않는다. force equation, tension, distance, angle과
release threshold는 미정이다.

## Performance and Destruction-Budget Ownership

### Prototype Measurement Profile

대표 scenario는 actor 네 명(플레이어, ally 한 명, enemy 두 명), object
instance 일곱 개, required breakable 0개, room 한 개와 동시에 active한
player interaction chain 한 개다. approved depth 안의 causal branch는
허용한다.

| Measurement category | Evidence owner |
|---|---|
| frame time, fixed-step load | G3 instrumentation proposal + G4 evidence |
| awake dynamic bodies, collision callbacks | Physics/Observability |
| normalized impact facts | Impact Normalization |
| active Origins and carriers | Causal Attribution |
| Reset duration and diff | Reset and Fault Recovery |
| physics faults | Fault Recovery counters |
| camera/presentation cost | Presentation instrumentation |
| memory and allocation spikes | G3/G4 profiling |

표는 measurement ownership이며 final pass/fail ceiling이 아니다.

### Later Slice Profile

Momentum, HeavyAction, Relay Position, Policy, conditional Suppressed,
conditional breakable/Structural과 additional content의 budget은 별도
later-Slice profile로 측정한다. 과거의 일반 `3–5 enemy`, fragment 또는
Structural 수치는 승인된 Prototype budget이 아니다. exact ceiling은
G3 Final Audit 이후 별도 proposal과 G4/G6 evidence가 소유한다.

## Reset, Save, and Versioning

authored baseline snapshot/restore가 primary Reset path다. complete Scene
reload는 누락 또는 unsafe recovery를 위한 audited fallback이다.
반복 Reset마다 baseline diff와 subscription count를 확인한다. later
resource가 활성화되면 encounter-entry resource baseline으로 되돌린다.

Reset inventory:

- CHR_00, CHR_01과 Compliance Runner 두 명;
- exact object pools, transform과 physical state;
- Carry와 Strap;
- Relay reservation/phase와 Ally state;
- Causal Origin/Result state;
- room-state flag;
- fault/quarantine state;
- subscription과 duplicate-handler state.

Prototype은 persistent gameplay save와 full Rigidbody-world persistence를
사용하지 않는다. persistent save가 later 승인될 때만 독립 save-schema
version을 정의한다. 최종 serialization format은 미정이다.

## Test Harness and Observability

### EditMode Plan

- logical state transition, arbitration과 safe rejection;
- identity separation;
- causal depth, uniqueness, branch와 termination;
- Reset-plan comparison;
- later Momentum arithmetic와 Policy lifecycle.

### PlayMode Plan

- Scoop, Carry와 Throw continuity;
- physical Light relay와 floor-supported Medium relay;
- Cargo Strap redirect;
- collision continuity와 depth-1 causal result;
- room-state change;
- repeated Reset와 fault recovery;
- camera와 route instrumentation.

### Manual and Playtest

- physical readability와 player agency;
- camera readability;
- voluntary experimentation;
- non-telekinetic interpretation.

관찰 필드는 active logical intent/context, interaction state, ownership,
Strap state/Anchor validity, Ally state/reservation/relay phase, Origin,
parent, carrier, depth, Result family, duplicate/cycle rejection reason,
room-state flag, Reset diff, subscription count, fault counter, active-body,
collision과 frame-spike counter다.

이 계획은 test, fixture, overlay 또는 CI를 생성하거나 실행하지 않는다.

## Bounded Prototype Implementation Sequence

G3 passage와 final pre-implementation review 이후에만 다음 순서를
ticket으로 분해할 수 있다.

1. Unity and repository Bootstrap
2. definition/runtime identity and state foundation
3. locomotion, camera and logical input adapter
4. interaction arbitration and ownership
5. acquisition, Carry, Throw and Queued Throw
6. Reset, diff and fallback
7. fixed Ally supply and Catch
8. Cargo Strap attach, redirect and release
9. impact, causal attribution and room-state change
10. feedback, observability, fault recovery and regression

TASK-0049는 이 순서의 실행이나 첫 구현 ticket 생성을 승인하지 않는다.

## Explicitly Deferred Technical Values

다음은 Draft 또는 later-Gate evidence 항목이다.

- exact physics tuning, collider count와 CCD assignment;
- velocity/angular-velocity cap;
- input timing/buffer와 Perfect Catch timing;
- Contact Freeze;
- Strap force, angle, distance, tension과 release threshold;
- automatic Safety Release;
- final AI, camera, animation, VFX와 audio;
- Policy candidate content와 Behavior Tag effect;
- Structural Feature와 breakable implementation;
- exact frame budget, hardware, resolution, FPS와 acceptance threshold;
- persistent save와 serialization format;
- permanent implementation class names.

## G3 Exit and Pre-Implementation Boundary

G3 Final Audit는 이 기준선, RTM Technical Source, checklist, manifest와
Gate 상태를 다시 검증해야 한다. G3 passage 뒤에도 remaining Design
Freeze work와 final pre-implementation review가 필수다.

이 문서는 Unity Bootstrap, package installation, code, test, Scene,
Prefab, build, Graybox 또는 Implementation Gate 개방 권한을 만들지
않는다.
