# G2 Ally Relay, Relay Position, and Suppressed Contract

상태: `OWNER-APPROVED G2 ALLY RELAY AND SUPPRESSED CONTRACT`

Decision Source: `TASK-0041 delegated Owner decision`

## Authority and Scope Boundary

이 문서는 `PR-F-003`, `PR-F-005`, `PR-F-006`, `PR-F-010`,
`PR-F-013`, `PR-F-014`, `PR-F-015`에 필요한 동료 물체 공급,
Relay Position과 Suppressed의 G2 논리 계약이다.

이 계약은 관찰 가능한 설계 책임과 물리적 인과만 승인한다.
runtime enum, state machine, behavior tree, navigation system, animation
graph, physics component, event schema 또는 Unity 구현을 승인하지 않는다.

일반 전투의 동료는 승인된 CHR_01 Visual Canon을 사용한다.
`Facilities Mover`는 승인된 이름, 직무 또는 Character Identity가
아니다. FIN_01의 익명 Finisher-only 보조 직원은 이 계약의 active
ally, CHR_02, 지속 Party Member 또는 일반 전투 기능이 아니다.

이 문서의 Owner-approved Ally Relay와 조건부 Suppressed 경계는
runtime, Unity, Graybox 또는 Gate 권한을 만들지 않는다. 현재 전역
Phase, Gate, Lock, Graybox, Relay와 다음 권한 경계는
[`docs/00_project/DESIGN_STATUS.md`](../00_project/DESIGN_STATUS.md)를
따른다.

## Applicability

| Milestone | Approved boundary |
|---|---|
| `PROTOTYPE CORE` | 정확히 한 명의 active ally, 한 개의 고정된 authored support position, 실제 Light 공급, 실제 이동 가능한 Medium 공급 |
| `PROTOTYPE CORE` 제외 | Relay Position 전환, 활성 Suppressed 기능, Heavy relay |
| `VERTICAL SLICE` | 유한한 authored Relay Positions와 그 사이의 전환, 전체 Light/Medium delivery envelope |
| `VERTICAL SLICE / SHOULD` | Suppressed의 조건부 포함 |

Suppressed는 Vertical Slice의 `SHOULD`이며 Prototype Core의 완료 조건이
아니다. 이 계약은 Suppressed의 최종 Vertical Slice 콘텐츠 편입을
보장하지 않는다.

## Prototype Content Application

Prototype 적용은
[`PROTOTYPE_CONTENT_LOCK.md`](PROTOTYPE_CONTENT_LOCK.md)를 따른다.

- authored support position은 정확히
  `PRT-RP-001 — Fixed Ally Support Position` 한 개다.
- CHR_01은 encounter entry와 Reset에서 `PRT-RP-001`의 `READY`다.
- ally supply pool은 `OBJ-P-L-001` File Binder 두 개와
  `OBJ-P-M-001` Wheeled Office Chair 한 개, 총 세 instance다.
- Binder는 airborne Catch-oriented supply, Chair는 floor-supported
  Strap-oriented grounded feed다.
- `OBJ-P-M-002` Rolling Drawer Cabinet과 `OBJ-P-H-001`
  Copier-Class Asset은 supply candidate가 아니다.
- replenishment, retrieval, hidden inventory, spawn, duplicate 또는
  destroyed-object replacement는 없다.
- Reset은 encounter-entry pool assignment를 복원한다.
- position transition, `RELOCATING`과 active `SUPPRESSED`는 없다.

이 content application은 exact coordinate, support-zone geometry,
candidate ranking, trajectory, timing, AI, navigation, animation,
physics 또는 Unity 구현을 승인하지 않는다.

## Active-Ally Boundary

- 일반 전투에는 정확히 한 명의 active support ally만 존재한다.
- active ally는 승인된 CHR_01 Canon을 유지한다.
- Prototype Core와 Vertical Slice 모두 두 번째 일반 전투 active
  ally, 동료 선택, party switching 또는 다수 동료 명령을 만들지 않는다.
- active ally는 물체를 물리적으로 공급하고, 승인된 Vertical Slice
  범위에서 Relay Position을 전환하며, 조건부 Suppressed 상태에
  들어가거나 회복할 수 있다.
- active ally는 적을 임의로 공격하거나 처치하지 않고 플레이어의
  최종 투척, 리디렉션, HeavyAction 또는 자원 소비 결정을 대신하지
  않는다.

## Ally Support States

동료 지원 가용성은 다음 네 논리 상태로만 구분한다.

| Logical state | Observable design responsibility |
|---|---|
| `READY` | 현재 authored position에서 유효한 요청을 받을 수 있다. |
| `SUPPLY_COMMITTED` | 정확히 한 요청을 수락하고 한 실제 물체를 예약해 release 전후 relay를 수행한다. |
| `RELOCATING` | 승인된 Vertical Slice Relay Position 사이를 물리적으로 전환하며 `CallAlly`를 받지 않는다. |
| `SUPPRESSED` | 적대적 압박으로 지원이 일시 차단되며 회복 가능하다. |

한 시점에 상태는 하나만 관찰된다. 이 이름은 design state이며 runtime
enum, class, serialized field 또는 구현 state machine 이름을 승인하지
않는다.

## Relay Position Contract

- 동료는 한 시점에 정확히 한 current Relay Position을 가진다.
- Relay Position은 room-local authored support position이다.
- Prototype Core는 encounter 전체에서 한 개의 고정 position만 사용한다.
- Vertical Slice는 방마다 유한하게 authoring된 position 후보만 사용할
  수 있다.
- 동료는 authored position 사이에서만 물리적으로 이동한다. 플레이어를
  자유 추적하거나 순간이동하지 않는다.
- 전환은 출발, 이동과 도착이 읽혀야 한다.
- hostile pressure, blocked access, authored room-state 변화 또는 현재
  위치의 supply 기능 상실은 position을 무효화할 수 있다.
- `CallAlly`는 position 이동 명령이 아니다. `MoveAlly`, `AllySwap`,
  `PositionSelect` 또는 두 번째 동료 action을 추가하지 않는다.

정확한 position 수, 좌표, support-zone 형상, 이동 route, navigation,
전환 시간과 animation은 Draft다.

## Room-Local Physical Supply Pool

공급 후보는 현재 방에 실제로 존재하며 다음을 모두 만족해야 한다.

- 현재 Relay Position의 support zone에서 물리적으로 도달 가능하다.
- 다른 상호작용이나 relay가 예약하지 않았다.
- 승인된 Weight delivery envelope에 속한다.
- 동료에서 플레이어 쪽으로 읽을 수 있는 실제 경로가 있다.

공급은 spawn, teleport, remote pull, hidden inventory 또는 물체
duplicate로 만들지 않는다. 파괴되었거나 존재하지 않는 물체를 공급할
수 없다. 정확한 zone, reach, visibility와 path 판정은 Draft다.

## Supply-Candidate Selection

- 공급 요청은 기존 `CallAlly` logical action만 사용한다.
- 요청 순간 gameplay context는 정확히 한 개의 모호하지 않은 current
  candidate를 만들어야 한다.
- context는 기존 `Aim`을 사용해 eligible Light 한 개 또는 이동 가능한
  Medium 한 개와 readable supply lane을 표현할 수 있다.
- candidate가 없거나 둘 이상으로 모호하면 요청 결과는 `NO EFFECT`다.
- class fallback, unrelated object fallback, 메뉴, class cycle 또는
  ally selection은 없다.

exact ranking, highlight, UI, 거리, 각도, line test와 동일 context의
tie-break는 Draft다.

## Request-Acceptance Conditions

`CallAlly` 수락은 기존 Interaction State Contract의 prerequisites와
다음을 모두 요구한다.

- 정확히 한 명의 active ally;
- ally state `READY`;
- 유효한 current Relay Position;
- ally가 `SUPPRESSED`가 아님;
- 정확히 한 개의 유효한 current candidate;
- 물리적으로 가능한 supply path;
- candidate가 아직 예약되지 않음.

수락하면 물체 한 개를 예약하고 ally를 `SUPPLY_COMMITTED`로 바꾸며
정확히 한 개의 `ACTIVE_RELAY`만 만든다. relay queue, replacement,
두 번째 relay는 없다.

차단된 요청은 queue, 다른 후보 자동 선택, spawn, carried-object drop,
Cargo Strap 자동 해제 또는 unrelated fallback을 만들지 않는다.

## Relay Lifecycle

`ACTIVE_RELAY`는 다음 두 논리 phase로 읽는다.

| Phase | Boundary |
|---|---|
| `PRE-RELEASE` | 요청이 수락되고 실제 물체가 예약되었지만 아직 동료의 물리적 control을 떠나 trajectory에 진입하지 않았다. |
| `RELEASED` | 물체가 동료 control을 떠나 연속된 실제 trajectory에 진입했다. Catch, Strap Attach, miss/passage, world interception 또는 Reset이 이후 해소를 담당한다. |

이 phase 이름은 design 설명이며 새 runtime state 이름을 승인하지
않는다. 한 시점에 두 번째 `ACTIVE_RELAY`는 존재할 수 없다.

## Pre-Release Abort

`PRE-RELEASE` relay는 다음 경우에만 안전하게 중단한다.

- 예약 물체가 더 이상 유효하지 않음;
- supply path가 물리적으로 불가능해짐;
- current Relay Position이 무효화됨;
- ally가 `SUPPRESSED`됨;
- Reset 또는 room/encounter lifecycle 종료.

abort는 reservation과 unresolved relay를 해소한다. 동료는 상황에 따라
`READY`, `SUPPRESSED` 또는 `RELOCATING`으로 돌아간다. 물체는 실제
세계의 지지 상태를 유지하며 숨겨진 fallback, Momentum 또는 Causal
Origin을 만들지 않는다. 정확한 handoff, drop과 abort animation은
Draft다.

## Post-Release Physical Resolution

- `RELEASED` 물체는 더 이상 ally inventory가 아니다.
- 물체는 실제 공간에서 연속된 trajectory와 collision을 유지한다.
- world entity가 물체를 가로막을 수 있다.
- miss는 물체를 삭제, 고정 또는 ally에게 자동 반환하지 않는다.
- 동료는 release 뒤 물체를 원격 steering하지 않는다.
- 이후 Catch, Strap Attach, miss/passage와 Reset은 Interaction State
  Contract가 해소한다.
- release 뒤 ally가 Suppressed되어도 이미 진행 중인 물체를 취소하거나
  삭제하지 않고, 입력을 지우거나, 이후 player-owned
  Catch→Throw/Queued Throw 또는 GEAR_01 Redirect의 유효성을 제거하지
  않는다.

## LIGHT Supply Envelope

- Light는 동료 손을 떠난 뒤 실제 airborne physical trajectory로
  공급한다.
- Light relay는 Catch-oriented이며 `CATCH_ELIGIBLE`이고
  `STRAP_ELIGIBLE`이 아니다.
- 플레이어는 Normal Catch, Perfect Catch attempt 또는 Queued Throw를
  시도할 수 있다.
- supply는 Catch 성공을 보장하지 않는다.

exact arc, speed, force, wind-up, timing과 catch alignment는 Draft다.

## MEDIUM Supply Envelope

- 이동 가능한 Medium은 바닥 지지(`floor-supported`) 상태를 유지하는
  roll, push 또는 grounded feed로 공급한다.
- Medium relay는 Catch 대상이 아니라 Strap-oriented다.
- Medium이 Strap candidate가 되려면 실제 이동 가능성, 하중 지지 금속
  Anchor와 읽을 수 있는 경로를 모두 가져야 한다.
- 동료는 GEAR_01을 부착, tension, redirect 또는 release하지 않는다.
- player가 Attach, Redirect와 Release의 최종 주체다.
- static 또는 부적합한 Medium은 relay candidate가 아니다.

exact speed, force, friction, caster behavior, anchor assignment와 timing은
Draft다.

## HEAVY Boundary

Heavy는 normal `CallAlly` relay에 포함되지 않는다.

- 동료는 Heavy를 플레이어에게 던지거나, 굴리거나, 밀어 공급하지 않는다.
- 동료는 Heavy를 원격 견인하거나 GEAR_01에 연결하지 않는다.
- 동료는 `HeavyAction`을 시작하거나 Pip을 소비하지 않는다.
- 자동 Heavy 고정부 해제, preparation 또는 방향 정렬 지원은 승인된
  기능이 아니다.
- 미래 Heavy assistance는 별도 Owner 결정 전까지 Draft다.

## Physical Inventory and Reservation

- 수락된 relay 하나는 실제 물체 하나만 예약한다.
- 하나의 물체를 두 relay 또는 다른 ownership context가 중복 예약할 수
  없다.
- 예약 중에도 물체는 world에 존재한다.
- release는 relay reservation을 끝낸다.
- abort된 물체는 실제 공간에 남는다.
- released 물체는 자동으로 ally 재고에 돌아가지 않는다.
- 재사용은 물체가 여전히 물리적으로 유효하고 후속 room/content rule이
  허용할 때만 가능하다.
- ally는 사용 물체를 자동 회수하지 않는다.
- 파괴된 가구는 자동으로 새로운 공급 물체를 만들지 않는다.
- fragment는 별도로 승인된 candidate identity와 Weight assignment가
  있을 때만 후보가 될 수 있다.

Prototype의 exact inventory와 object content는 후속 Prototype Enemy,
Room, Object Content Lock에서 결정한다.

## Player Agency Boundary

동료는 유효한 `CallAlly`에 따라 선택된 실제 물체를 공급하고, 승인된
Vertical Slice 범위에서 position을 전환하며, Suppressed와 recovery를
표현할 수 있다.

동료는 다음을 하지 않는다.

- `CallAlly` 없이 임의로 물체를 선택하거나 공급;
- 적 공격 또는 처치;
- GEAR_01의 Attach, Redirect 또는 Release;
- `HeavyAction` 시작 또는 resource 소비;
- 최종 Throw 또는 Redirect 방향 결정;
- 두 번째 active ally 생성.

player-owned 행동 전에 ally 공급 물체가 world entity에 우연히
가로막힌 결과는 물리적 world result일 뿐 reward-eligible player
Origin이 아니다.

## Momentum and Causal Provenance

Ally Relay modifier는 다음을 모두 만족한 Origin에만 `+5 Momentum`을
정확히 한 번 적용한다.

1. 승인된 relay 요청이 수락되었다.
2. 예약 물체가 `RELEASED` phase에 진입했다.
3. 플레이어가 Catch 뒤 Throw/Queued Throw 또는 GEAR_01 Redirect로
   행동 소유권을 얻었다.
4. 해당 player-owned Origin이 하나 이상의 eligible Meaningful Result를
   만들었다.

`CallAlly`, reservation, wind-up, release, Catch 또는 Attach만으로는
reward를 주지 않는다. pre-release abort는 provenance를 만들지 않는다.
player ownership 전의 accidental result도 reward를 만들지 않는다.
release 뒤 Suppressed는 이미 유효해진 provenance나 modifier 자격을
제거하지 않는다.

다른 Momentum/Pip 값, Origin당 cap, Chain depth, Meaningful Result
family와 termination은 기존 Owner-approved 계약을 그대로 따른다.

## Relay Position Transition

Relay Position 전환은 `VERTICAL SLICE`에만 적용한다.

- hostile pressure로 현재 position이 contested 또는 unsafe가 됨;
- authored room-state 변화로 현재 position이 무효화됨;
- 현재 위치에서 유효한 supply 기능을 잃음;
- encounter가 authoring된 다른 position을 활성화함

중 하나가 발생하면 전환 후보가 될 수 있다. 동료는 유효한 authored
position 하나로만 이동한다. `RELOCATING` 동안 `CallAlly`는 차단된다.
전환은 순간이동, free-follow, Causal Origin, Momentum, 자동 candidate
선택 또는 object reservation을 만들지 않는다.

exact destination choice, route, navigation, traversal duration,
animation, collision avoidance와 recovery timing은 Draft다.

## Suppressed Applicability

Suppressed는 Prototype Core에서 비활성이다.

Vertical Slice에서는 `SHOULD`인 조건부, 일시적, 회복 가능한 support
availability 상태다. ally death, permanent removal, health system 또는
필수 Vertical Slice inclusion을 뜻하지 않는다.

## Suppressed Entry

Suppressed 진입은 active hostile pressure가 현재 support position을
contested 또는 unsafe하게 만들 때만 가능하다.

후속 설계 증거 후보는 다음과 같이 제한한다.

- 읽을 수 있는 hostile attack line;
- 적이 현재 support zone을 점유함;
- committed hostile interruption.

다음만으로는 Suppressed에 진입하지 않는다.

- 빈 inventory 또는 유효 candidate 없음;
- object path 차단;
- player 거리, camera 또는 Aim;
- furniture collision;
- relay miss;
- Momentum 부족.

exact threat geometry, threshold, duration, enemy action과 판정 timing은
Draft다.

## Suppressed Effects

- 새 `CallAlly`, reservation과 relay 시작을 차단한다.
- ally가 공격, 사망 또는 영구 이탈하지 않는다.
- ally는 grounded protection을 유지한다.
- 유효한 authored position이 있으면 별도 전환 조건 아래 이동할 수 있다.
- `PRE-RELEASE` relay는 안전하게 abort할 수 있다.
- `RELEASED` 물체는 계속 실제 물리 경로를 따른다.
- Suppressed는 이미 유효한 Origin, Ally Relay modifier 자격,
  trajectory 또는 player input을 지우지 않는다.

## Suppressed Recovery

다음을 모두 만족하면 recovery 후보가 된다.

- active hostile pressure가 더 이상 current support를 contest하지 않음;
- ally가 유효한 authored position에 있음;
- unresolved `PRE-RELEASE` relay가 없음.

플레이어는 적을 이동시키거나 room state를 바꾸어 간접적으로 recovery
조건을 만들 수 있다. recovery가 완료되면 ally는 `READY`로 돌아간다.
유효한 position이 없으면 ally는 unavailable 상태를 유지하며 순간이동
또는 free-follow하지 않는다.

exact recovery delay, path, protection pose, AI와 animation은 Draft다.

## Reset and Room Lifecycle

완료된 `ResetEncounter`는 다음 encounter-entry baseline을 복원한다.

- encounter-entry authored Relay Position;
- ally state `READY`;
- reservation 없음;
- unresolved `PRE-RELEASE` relay 없음;
- Suppressed 또는 Relocating 아님;
- 추가 active ally 없음.

기존 Interaction, Momentum/Pip와 Causal Chain Reset 계약은 그대로
우선한다. exact snapshot, restore ordering, event와 구현은 Draft다.

room transition은 이전 방의 reservation과 unresolved relay를 해소하고
새 방의 authored baseline을 사용한다. cross-room hidden inventory는
존재하지 않는다.

## Feedback Boundary

후속 presentation은 최소한 다음을 서로 구분할 수 있어야 한다.

- `READY`;
- request accepted / `SUPPLY_COMMITTED`;
- `RELOCATING`;
- `SUPPRESSED`;
- request `NO EFFECT`;
- `PRE-RELEASE` abort;
- `RELEASED` physical relay.

HUD, icon, color, text, audio, VFX, animation, timing 또는 controller
feedback은 승인하지 않는다. 색상만으로 상태를 구분해서는 안 된다.

## Deferred Design and Technical Decisions

다음은 명시적으로 `DRAFT`다.

- Vertical Slice Relay Position count, 모든 exact coordinates, zones와
  placement. Prototype의 position 수와 ID는 Content Lock에서 고정;
- supply range, reach, path test, candidate ranking와 tie-break;
- highlight, UI, text, icon, audio, VFX와 accessibility treatment;
- Light/Medium trajectory, speed, force, wind-up, cooldown과 timing;
- Vertical Slice inventory, object list, reuse, fragments와 room content.
  Prototype의 exact supply pool은 Content Lock에서 고정;
- abort handoff, drop, recovery와 animation;
- transition destination, route, pathfinding, NavMesh와 traversal;
- suppression threat geometry, threshold, duration, recovery와 enemy action;
- ally health, damage, knockback 또는 combat system;
- Heavy assistance;
- behavior tree, AI, animation, IK, physics, runtime state, event와 data;
- Unity component, architecture와 implementation.
