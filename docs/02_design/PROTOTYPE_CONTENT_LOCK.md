# G2 Prototype Enemy, Room, and Object Content Lock

상태: `OWNER-APPROVED G2 PROTOTYPE CONTENT LOCK`

Decision Source: `TASK-0042 delegated Owner decision`

## Authority and Scope Boundary

이 문서는 첫 Prototype Core의 적, 방, 동료 위치, 물체 목록과 검증용
배치를 고정한다. 승인 범위는 정확히 한 개의 authored validation room,
네 명의 인간 actor, 네 개의 object definition과 일곱 개의 object
instance, 그리고 아래에 열거한 validation affordance다.

이 잠금은 제품용 encounter, Vertical Slice room, final art, 정확한
geometry, 수치, AI, animation, physics, prefab, scene, package 또는 Unity
구현을 승인하지 않는다. `PRT-*`, `ENY-*`, `OBJ-*` ID는 문서상 참조
식별자일 뿐 runtime ID, prefab 이름, enum, ScriptableObject, 파일명,
database key 또는 serialized asset 이름이 아니다.

이 문서의 Owner-approved Prototype 콘텐츠 경계는 제품용 encounter,
Unity, Graybox 또는 Gate 권한을 만들지 않는다. 현재 전역 Phase,
Gate, Lock, Graybox, Relay와 다음 권한 경계는
[`docs/00_project/DESIGN_STATUS.md`](../00_project/DESIGN_STATUS.md)를
따른다.

## Prototype Room

Prototype room은 정확히 하나다.

| ID | 이름 | 수량 | 역할 |
|---|---|---:|---|
| `PRT-RM-001` | Open Operations Prototype Room | 1 | one-room causal gameplay validation |

`PRT-RM-001`은 일반적이고 현실적인 corporate open-office를 사용한
authored validation room이다. final-product room, Vertical Slice room,
정확한 architecture 또는 production layout이 아니다.

다음은 이 방에 포함하지 않는다.

- 여러 방, room transition 또는 procedural layout;
- LGT_01 Records Archive 조명 변주;
- FIN_01 Team Finisher;
- cinematic presentation;
- production-art density.

정확한 치수, 동선 폭, 좌표, 재질, 조명, camera 수치와 collision
geometry는 `DRAFT`다.

## Exact Actor Content

Prototype에는 정확히 네 명의 인간 actor만 존재한다.

| Actor | 수량 | Prototype 역할 |
|---|---:|---|
| 승인된 `CHR_00` | 1 | player |
| 승인된 `CHR_01` | 1 | active support ally |
| `ENY-P-001 — Compliance Runner` | 2 | 동일한 Prototype functional enemy archetype |

다섯 번째 인물, 두 번째 ally, CHR_02, FIN_01 보조 직원 또는 다른 enemy
archetype은 존재하지 않는다.

## Enemy Archetype Boundary

`ENY-P-001 — Compliance Runner`는 Prototype에 한정된 기능 표기이며
final enemy 이름이나 Identity Lock이 아니다.

- 두 instance는 같은 archetype을 사용한다.
- 평범한 corporate compliance/security pressure로 읽힌다.
- 물리적으로 이동 가능한 causal target이다.
- 읽을 수 있는 한 가지 close-range pressure pattern만 가진다.
- nonlethal presentation을 유지한다.

정확한 얼굴, uniform, 직무, 계급, stats, timing, reaction, ragdoll,
AI, navigation과 Unity 구현은 `DRAFT`다. Guard, Asset Mover,
Interceptor, Elite, boss, ranged enemy, firearm, shield, suppression
specialist 또는 두 번째 enemy archetype을 Prototype에 추가하지 않는다.
`Suppressed` 기능도 Prototype에서는 비활성이다.

## Fixed Ally Position

Prototype의 ally position은 정확히 하나다.

| ID | 이름 | 수량 | 상태 |
|---|---|---:|---|
| `PRT-RP-001` | Fixed Ally Support Position | 1 | encounter entry와 Reset에서 `READY` |

CHR_01은 encounter 동안 `PRT-RP-001`에 머물며 여기서 승인된 physical
supply를 수행한다. Prototype에는 position transition, `RELOCATING`,
active `SUPPRESSED`, free-follow, teleport 또는 두 번째 ally가 없다.
정확한 좌표, support-zone 형상, facing과 reach는 `DRAFT`다.

## Exact Object Content

Prototype object content는 정확히 네 개의 definition과 일곱 개의
instance다.

| ID | Object | Weight Class | 수량 | Direct Pool | Ally Pool | Prototype 역할 |
|---|---|---|---:|---:|---:|---|
| `OBJ-P-L-001` | File Binder | `LIGHT` | 3 | 1 | 2 | direct/relay acquisition과 throw |
| `OBJ-P-M-001` | Wheeled Office Chair | `MEDIUM` | 2 | 1 | 1 | direct handling과 GEAR_01 redirect |
| `OBJ-P-M-002` | Rolling Drawer Cabinet | `MEDIUM` | 1 | 0 | 0 | attributed collision과 side-lane clearance |
| `OBJ-P-H-001` | Copier-Class Asset | `HEAVY` | 1 | 0 | 0 | recognition-only landmark |

Gameplay-interactive definition은 Binder, Chair, Drawer의 세 개다.
Copier는 Heavy class를 읽히게 하는 recognition-only landmark다.
추가 interactive object, 다섯 번째 definition 또는 여덟 번째 instance를
추가하지 않는다. 장식 clutter는 non-interactive이고 gameplay physics,
acquisition, supply 또는 causal role을 갖지 않는다.

## File Binder Boundary

`OBJ-P-L-001`은 `LIGHT`이며 다음에만 사용한다.

- ally pool 두 개, direct-acquisition pool 한 개;
- Scoop, Carry, Direct Throw;
- airborne physical ally supply, Normal Catch와 Queued Throw;
- player action 이후 causal initiator.

Binder는 Strap, HeavyAction 또는 required breakage를 사용하지 않는다.
replenishment, duplication, hidden inventory, exact values, Behavior Tag와
final model은 승인하지 않는다. 빗나간 Binder는 Reset 전까지 방의 실제
물체로 남는다.

## Wheeled Office Chair Boundary

`OBJ-P-M-001`은 `MEDIUM`이며 direct pool과 ally pool에 각각 한 개씩
배치한다. 두 instance는 같은 definition과 eligibility를 사용한다.

- 이동 가능한 floor-supported object다.
- 조건부 Scoop, Carry와 Direct Throw가 가능하다.
- CHR_01은 grounded feed로 공급한다.
- 하중 지지 금속 Anchor를 통해 GEAR_01 Strap Redirect가 가능하다.
- causal displacement를 만들 수 있다.

Chair는 Catch, Queued Throw 또는 HeavyAction에 eligible하지 않다.
캐스터 축, plastic housing 또는 취약 돌출부에는 연결하지 않는다.
exact model, caster construction, breakage, force, friction, trajectory,
timing과 Unity 구현은 `DRAFT`다.

## Rolling Drawer Cabinet Boundary

`OBJ-P-M-002`는 `MEDIUM` 한 개이며 authored room-state object다.

- 초기 상태에서 side lane을 막는다.
- player-owned origin에 귀속되는 collision으로 이동할 수 있다.
- 이동 결과가 side lane을 열면 meaningful local room-state change다.
- 그 상태는 Reset까지 유지된다.

Drawer는 acquisition, supply, Scoop, Carry, Throw, Queued Throw, Catch,
Relay, Strap 또는 HeavyAction 대상이 아니다. 정확한 displacement,
lane width와 이동 거리는 `DRAFT`다. 부수적인 visibility 변화는 두 번째
필수 room-state result가 아니다.

## Copier-Class Heavy Boundary

`OBJ-P-H-001`은 `HEAVY` 한 개이며 정적이고 floor-supported인
recognition-only landmark다. Prototype에서 Heavy class 후보가 Light와
Medium과 다르게 읽힌다는 증거만 제공한다.

Copier에는 Scoop, Carry, Throw, Catch, Relay, Strap, HeavyAction,
Momentum, movement, tip, break, toner 또는 room-state 기능이 없다.
floor support에 필요한 ordinary fixed collision은 가질 수 있으나
authored impact surface나 causal target은 아니고 interactive object도
아니다. 정확한 model, mass, dimension, material과 collider는 `DRAFT`다.

## Fixed Environment Content

`PRT-RM-001`은 다음 authored content를 정확히 가진다.

- player start 1개;
- `PRT-RP-001` fixed ally position 1개;
- Compliance Runner start 2개;
- direct-acquisition pool 1개;
- ally supply pool 1개;
- primary open movement lane 1개;
- readable ally supply lane 1개;
- Medium redirect lane 1개;
- Drawer가 막는 side lane 1개;
- Reset baseline 1개;
- non-interactive authored impact surface 2개:
  - solid wall 1개;
  - fixed desk, column 또는 동등한 고정 표면 1개.

두 impact surface는 Structural Feature도 breakable도 아니며 object
definition/instance count에 포함하지 않는다. lane은 명확히 읽히는 한
서로 겹칠 수 있다. 정확한 geometry, 좌표, 거리와 크기는 `DRAFT`다.

## Exact Ally Supply Pool

Ally supply pool은 정확히 세 개의 실제 물체다.

- `OBJ-P-L-001` File Binder 2개;
- `OBJ-P-M-001` Wheeled Office Chair 1개.

한 시점에는 현재 context의 candidate 한 개와 reservation 한 개,
`ACTIVE_RELAY` 한 개만 허용한다. replenishment, retrieval, hidden
inventory, destroyed-ammo replacement, Heavy supply, spawn 또는 duplicate는
없다. Drawer와 Copier는 ally candidate가 될 수 없다. Reset은
encounter-entry pool assignment를 복원한다.

## Exact Direct-Acquisition Pool

Direct-acquisition pool은 정확히 두 개의 실제 물체다.

- `OBJ-P-L-001` File Binder 1개;
- `OBJ-P-M-001` Wheeled Office Chair 1개.

Drawer와 Copier는 direct-acquisition candidate가 아니다. 정확한
좌표, interaction range와 target ranking은 `DRAFT`다.

## Required Validation Affordances

Prototype room은 고정된 실행 순서 없이 다음 일곱 affordance를 모두
제공해야 한다.

1. direct Light Binder Scoop, Throw와 readable physical result;
2. ally Binder airborne supply, Normal Catch, Queued Throw와
   player-owned result. Perfect Catch는 비활성;
3. direct Medium Chair Scoop, Carry, Throw와 launch;
4. ally Chair grounded feed, player GEAR_01 Strap attach와 redirect,
   floor support 유지;
5. Runner 또는 Drawer를 이용한 최소 Depth `1` causal opportunity;
6. Drawer 이동으로 side lane이 열리고 Reset까지 유지되는
   non-Structural room-state change;
7. Reset으로 actor, object, pool, lane과 causal baseline 복원.

모든 affordance를 하나의 Origin, 한 번의 attempt 또는 한 고정 순서로
증명할 필요는 없다.

## Prototype Feature Exclusions

Prototype에서 다음은 비활성 또는 제외다.

- Perfect Catch;
- Momentum/Pip;
- HeavyAction;
- Structural Feature;
- breakable, glass, debris와 destruction VFX;
- Policy Violation;
- Incident Report;
- LGT_01;
- FIN_01;
- Relay Position transition;
- Suppressed;
- 두 번째 ally와 CHR_02;
- Guard, Asset Mover, Interceptor, Elite, boss;
- firearm, ranged pressure와 shield;
- Behavior Tag requirement;
- final reward와 progression system;
- permanent player death;
- Life 또는 Continue system;
- full Campaign Game Over;
- production checkpoint 또는 save implementation;
- failure에서 파생되는 alternate ending;
- multi-room structure.

Meaningful result는 actor/object displacement, side-lane clearance와
causal attribution으로 검증한다. 어떤 object도 파손될 필요가 없다.

G4-A를 위한 minimum nonlethal encounter failure와 encounter-entry
restart는 Prototype에 명시적으로 포함한다. 이는 위 permanent death,
Campaign Game Over, production save 또는 alternate ending 제외와
충돌하지 않는다.

## G4-A Core Room Evidence Boundary

Decision Source: `CR-0001 / ADR-0021 / FGD-001 §19`

G4-A는 Story chapter, micro Vertical Slice 또는 campaign production이
아니다. 정확히 이 Lock의 one authored room, actor `4`(CHR_00 ×1,
CHR_01 ×1, Compliance Runner ×2), object definition `4`, object
instance `7`과 Prototype destruction cap `0`만 사용하는 bounded
Test-Harness evidence brief다.

Runner 시작 배치는 random generation이나 새 AI behavior가 아닌
Test-Harness placement variant다.

- `Variant A`: Runner 1 begins between the player and the direct Drawer approach. Runner 2 pressures the central movement lane.
- `Variant B`: Runner 1 begins on CHR_01’s supply lane. Runner 2 begins near the Drawer objective.
- `Variant C`: Runner 1 begins between the player and the Direct Pool. Runner 2 begins between the player and the Strap Anchor lane.

Minimum Player Failure의 active G4-A 범위는 다음 다섯 결과만 소유한다.

- validated enemy pressure가 continued play를 막을 수 있음;
- 결과는 nonlethal임;
- player가 killed되지 않음;
- 현재 encounter가 authored entry baseline으로 돌아감;
- 같은 encounter를 다시 시도할 수 있음.

완료된 higher-level Story, rescue, As-Built/map과 shortcut 진행 보존은
FGD-001의 승인된 full-game target으로 유지한다. 그러나 이는 G4-A
behavioral PASS evidence가 아니며 later requirement·technical
decomposition 대상이다. Exact damage, Stability, threshold,
accumulation, animation, Bark, AI와 recovery timing은 `DRAFT`다.

G4-A readiness 분류는 다음과 같다.

- Direction and Decision Rule: `OWNER-APPROVED`;
- Detailed Evidence Protocol: `PENDING`;
- Execution: `NOT STARTED / NOT AUTHORIZED`;
- Targeted C3 Re-Audit: `REQUIRED BEFORE G4-A IMPLEMENTATION`.

후속 상세 protocol은 observer, session, explanation, variant order,
`YES/NO` evidence와 result reporting을 정의해야 한다. 이 문서는 해당
protocol을 생성하거나 G4-A 실행을 승인하지 않는다.

Evidence questions는 FGD-001 §19.6의 `Q1`~`Q5`다.

- `Q1`: under pressure에서 core Relay, Throw, Strap을 자발적으로 쓰는가?
- `Q2`: 같은 Drawer objective에 최소 두 valid physical approach가 있는가?
- `Q3`: Player–Ally–Object causality가 설명 없이 읽히는가?
- `Q4`: failure/Reset 뒤 layout variant가 다른 접근 이유를 만드는가?
- `Q5`: camera가 player, active object, enemy pressure, exit objective를 동시에 읽히게 하는가?

`Q1`, `Q3`, `Q5`는 mandatory다. PASS rule이 충족돼도
`Q2 = NO`이면 G4-A core viability PASS는 유지할 수 있지만 G4-B는
blocked이며 layout을 정확히 한 번 수정해 Q2만 재검사한다. Q2가 다시
NO이면 Story/Campaign/Map/Level production integration은 계속 차단된다.
범위 안 retune과 retest는 최대 한 번, exploratory evidence round도
최대 한 번이다.

G4-A production-speed evidence는 empty project에서 playable Core Room까지
actual work time, round 동안 실제 weekly development time, rework,
tool/asset wait, reusable output과 external-asset dependency를 기록한다.
한 sample을 full-campaign schedule로 직접 곱하지 않는다.

G4-A에는 Story NPC, rescue NPC production, Cart, As-Built/Survey/Map UI,
glass, Momentum/Pip, HeavyAction, destruction, campaign level 또는
promotional-wall Finale를 추가하지 않는다. 이 brief는
`OWNER-APPROVED / NOT STARTED / NOT AUTHORIZED`다.

## Reset Content Baseline

완료된 `ResetEncounter`는 다음 encounter-entry content baseline을
복원한다.

- CHR_00은 player start;
- CHR_01은 `PRT-RP-001`에서 `READY`;
- active relay와 reservation 없음;
- 두 Compliance Runner는 각 start;
- Binder와 Chair의 direct/ally pool assignment;
- Drawer는 side lane을 막는 초기 상태;
- Copier는 Heavy recognition landmark 상태;
- active causal origin과 persistent room-state change 없음.

snapshot technology, restore order, event, prefab reload, pooling과 Unity
구현은 승인하지 않는다.

## Sequence-Agnostic Boundary

이 잠금은 Binder, Runner, Chair와 Drawer의 고정 처리 순서, 첫 action,
첫 target, branch 또는 PGT_02/PGT_03의 이미지 순서를 요구하지 않는다.
서로 다른 유효 시도에서 일곱 affordance를 검증할 수 있으며 causal
attribution은 기존 G2 Chain 계약을 따른다.

## Final-Art, Technical, and Production Deferrals

모든 final character, enemy, object, room, lighting, VFX, animation과
audio art는 `DRAFT` 또는 later production scope다. Primitive/Graybox
표현도 별도 Design Readiness와 Implementation authority 이후에만 만들
수 있으며 이 잠금은 실행 권한이 아니다.

정확한 stats, damage, Stability, timing, distance, velocity, force,
friction, mass, AI, navigation, animation, IK, Rigidbody, collider,
physics material, camera, reset architecture, component, scene, prefab,
package와 Unity 구현은 G3/G4 이후 별도 결정으로 남는다.
