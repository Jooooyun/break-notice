# Office Core Kit

> **OWNER-APPROVED OFFICE CORE KIT ASSET-FAMILY AND REUSE LOCK**
>
> Decision Source:
> `docs/09_adrs/ADR-0017-OFFICE-CORE-KIT-SCOPE-LOCK.md` and
> `TASK-0056 delegated Owner decision`

## Authority and Gate Boundary

이 문서는 Owner-approved Art/Production Support 범위 Lock이다. 이
Lock은 실제 모델, 텍스처, 재질, 데칼 atlas, 조명 asset, Unity folder,
Prefab, ScriptableObject, Scene, room layout, Graybox 또는 구현을
만들거나 승인하지 않는다. 현재 전역 Phase, Gate, Lock, Graybox,
Relay와 다음 권한 경계는
[`docs/00_project/DESIGN_STATUS.md`](../00_project/DESIGN_STATUS.md)를
따른다.

## Kit Definition

Office Core Kit은 정확한 room layout과 object instance 배치에서
독립된, 제한적이고 재사용 가능한 production-family inventory이자
Art/Production Support scope lock이다. 향후 Prototype 및 G5에서
선택된 Later-Slice asset의 source가 될 수 있다.

Office Core Kit은 Unity folder, Prefab inventory, 완성된 production
asset delivery, room-count 결정, 모든 후보의 Vertical Slice 편입
약속 또는 modeling·implementation 시작 권한이 아니다.

이 문서의 모든 `OCK-*` ID는 문서 참조 ID일 뿐이다. file name,
Prefab name, ScriptableObject ID, runtime enum, serialized key 또는
database ID를 승인하지 않는다.

## Exact Prototype-Required Families

Prototype-required family는 정확히 `16`개다.

### Architecture and Fixed Fixture Families — Exactly 7

| ID | Family | Prototype responsibility |
|---|---|---|
| `OCK-A-001` | Solid Office Wall System | Static opaque room shell and fixed wall impact surface |
| `OCK-A-002` | Fixed Office Door and Frame | Non-interactive authored fixed-state doorway; no room transition |
| `OCK-A-003` | Low Cubicle Partition System | Static open-office spatial divider |
| `OCK-A-004` | Office Floor Surface | Static grounded floor presentation and collision surface |
| `OCK-A-005` | Suspended Ceiling System | Static ceiling shell; no falling-tile gameplay |
| `OCK-A-006` | Fixed Column / Impact Surface | Static non-Structural collision and spatial-reading fixture |
| `OCK-A-007` | Neutral Fluorescent Ceiling Fixture | Visual fixture family only; no LGT_01 behavior or flicker gameplay |

### Furniture and Approved Gameplay-Object Families — Exactly 5

| ID | Family | Authority mapping |
|---|---|---|
| `OCK-F-001` | Fixed Work Desk | Non-interactive authored impact surface |
| `OCK-F-002` | Wheeled Office Chair | Maps only to `OBJ-P-M-001` |
| `OCK-F-003` | Rolling Drawer Cabinet | Maps only to `OBJ-P-M-002` |
| `OCK-F-004` | Copier-Class Asset | Maps only to recognition-only `OBJ-P-H-001` |
| `OCK-P-001` | File Binder | Maps only to `OBJ-P-L-001` |

### Decorative and Dressing Families — Exactly 4

| ID | Family | Boundary |
|---|---|---|
| `OCK-D-001` | Workstation Electronics Cluster | Monitor, keyboard, phone and restrained cable dressing; non-interactive |
| `OCK-D-002` | Paper and Archive Box Cluster | Paper stacks and closed archive boxes; non-interactive |
| `OCK-D-003` | Office Plant and Waste Bin Cluster | Non-interactive office dressing |
| `OCK-D-004` | Signage, Asset Tag, Tape and Wear Decal Set | Shared visual variation only |

합계는 `7 + 5 + 4 = 16 families`이며 열일곱 번째
Prototype-required family를 추가하지 않는다.

## Prototype Gameplay Mapping

Gameplay-interactive definition은 정확히 세 개다.

- File Binder → `OBJ-P-L-001`
- Wheeled Office Chair → `OBJ-P-M-001`
- Rolling Drawer Cabinet → `OBJ-P-M-002`

Recognition-only definition은 정확히 하나다.

- Copier-Class Asset → `OBJ-P-H-001`

나머지 정확히 `12`개 family는 fixed, non-interactive 또는 decorative다.
Prototype instance는 File Binder `3`, Wheeled Office Chair `2`,
Rolling Drawer Cabinet `1`, Copier-Class Asset `1`의 정확히 `7`개를
유지한다. 이 Kit Lock은 gameplay-object definition 또는 instance를
추가하지 않는다.

## Collision and Interaction Boundary

wall, fixed door/frame, low partition, floor, 기술적으로 필요한 ceiling
shell, column과 fixed desk는 향후 단순한 static collision을 받을 수
있다. 이 family들은 Scoop, Carry, Throw, Catch, Ally Relay, Cargo
Strap, HeavyAction, Causal-carrier authority 또는 Prototype Breakable
state를 받지 않는다.

Decorative family는 gameplay Rigidbody-style body, gameplay collision,
interaction prompt, target highlight, Carry, Throw, Relay, Strap
eligibility 또는 Causal Result authority를 갖지 않는다. keyboard,
monitor와 paper detail은 새 gameplay-object definition이 아니다.

Neutral Fluorescent Ceiling Fixture는 visual fixture family일 뿐이며
정확한 Lux, color temperature, flicker, emergency-light gameplay, URP lighting
configuration 또는 LGT_01 activation을 승인하지 않는다.

## Prototype Destruction Boundary

모든 Prototype-required Office Core Kit family의 상태는 다음과 같다.

- Breakable state: `NONE`
- Structural Feature state: `NONE`
- runtime fracture: `PROHIBITED`
- destruction fragment budget: `0`

Office Core Kit은 Prototype에 breakage를 추가하지 않는다.
non-destructive impact mark, sound 또는 restrained feedback은 별도
Draft presentation work다.

## Exact Later-Slice Extension Candidates

Later-Slice extension candidate는 정확히 `8`개다. G5 scope selection을
통해서만 검토할 수 있다.

| ID | Candidate family | Boundary |
|---|---|---|
| `OCK-X-A-001` | Glass Partition System | Static by default; Breakable only through separate G5 approval and ADR-0016 caps |
| `OCK-X-A-002` | Meeting Room Frame and Double-Door System | Static authored architecture |
| `OCK-X-A-003` | Archive Shelving Bay | Static by default; not automatically Structural |
| `OCK-X-A-004` | Exposed Ceiling and Cable-Tray System | Static by default; not automatically Structural |
| `OCK-X-F-001` | Records Cart | Potential Later-Slice movable asset; no current content inclusion |
| `OCK-X-F-002` | Mobile Whiteboard | Potential Later-Slice movable asset; no current content inclusion |
| `OCK-X-F-003` | Metal Storage Cabinet | Potential Later-Slice Heavy or fixed candidate; classification requires G5 decision |
| `OCK-X-F-004` | Conference Table | Potential Later-Slice Heavy prop; does not activate FIN_01 |

아홉 번째 extension candidate를 추가하지 않는다. 이 family들은
Prototype content가 아니며, Vertical Slice content, Breakable identity,
Structural Feature 또는 asset 제작 권한으로 자동 승인되지 않는다.
기존 Owner-approved mapping이 없는 exact Weight Class assignment는
계속 deferred다.

## LGT_01 and FIN_01 Boundaries

Archive Shelving, Fluorescent Fixture 또는 archive-themed room의 존재는
LGT_01을 활성화하지 않는다. LGT_01은 계속 Records Archive-local
`COULD`이며 정확한 lighting과 rendering 구현은 별도다.

Conference Table의 존재는 FIN_01을 활성화하거나 CHR_02, 두 번째
normal-combat ally, Vertical Slice Team Finisher 또는 final-product
scope를 만들지 않는다. FIN_01은 계속 `OUT / COULD`다.

## Structural and Breakable Boundaries

Office Core Kit family는 별도 G5 content 결정이 있을 때만 ordinary
Breakable 또는 Structural Feature가 될 수 있다.

- Glass Partition은 자동 Breakable이 아니다.
- Archive Shelving은 자동 Structural이 아니다.
- Cable Tray는 자동 Structural이 아니다.
- Wall과 Door는 자동 destructible이 아니다.
- Metal Storage Cabinet은 자동 Heavy 또는 Structural이 아니다.
- Conference Table은 자동 Finisher object가 아니다.
- material appearance는 gameplay state를 정의하지 않는다.

선택된 Breakable 또는 Structural family는
`docs/04_technical/DESTRUCTION_BUDGET_AND_IMPLEMENTATION_RULES.md`와
ADR-0016의 상한을 우회할 수 없다.

## Geometry and Variant Scope Caps

Prototype family의 hard production-family limit은 다음과 같다.

- 각 furniture 또는 gameplay-object family: 정확히 하나의 base
  geometry family
- 각 architecture system: 하나의 base system과 최대 세 개의 필요한
  modular shape piece
- 각 decorative cluster family: 최대 세 개의 non-interactive prop form
- 각 family: 최대 두 개의 finish 또는 material variant
- department name 또는 room color만을 위한 unique geometry 금지

G5에서 선택된 Later-Slice extension에는 하나의 base geometry family,
최대 두 개의 finish 또는 material variant를 허용한다. color/decal만
다른 두 번째 family와 Change Control 없는 room-only duplicate는
금지한다.

이 cap은 topology, polygon count, UV, texture resolution, LOD, shader,
collider, Prefab 또는 구현 방식을 승인하지 않는다.

## Reuse and Department Variation

department와 room의 차이는 shared material, decal, signage, asset tag,
paper, wear/dirt, 별도 승인된 lighting과 arrangement로 만든다. department
마다 새 Kit을 만들거나 department 이름만을 위해 wall, desk, chair,
copier 또는 trim의 unique geometry를 만들지 않는다.

재사용 결과는 계속 `Grounded Corporate Action Realism`의 권위를
따른다.

## Shared Material Responsibility

Kit은 painted wall/ceiling, carpet/vinyl, laminate/composite, painted
metal, worn plastic/rubber, paper/cardboard와 office fabric/upholstery의
shared material responsibility를 가진다. 이는 재사용 책임만 고정하며
texture count, resolution, shader, material instance, rendering 또는
import 설정을 승인하지 않는다. Glass는 Later-Slice candidate
material이다.

## Explicit Exclusions

이 Lock은 character/player turnaround, enemy, GEAR_01 model, animation,
VFX, audio, UI, camera, final layout·dimension·room count, enemy count,
final Breakable identity, Structural inclusion, exact LGT_01, FIN_01,
Unity asset 또는 구현을 승인하지 않는다.

## Change-Control Boundary

다음 변경에는 Change Control과 Owner 승인이 필요하다.

- 열일곱 번째 Prototype-required family
- 아홉 번째 Later-Slice extension candidate
- family당 세 번째 finish 또는 material variant
- 새 gameplay-interactive definition
- 새 department-specific Kit
- department 전용 unique geometry
- 새 Breakable 또는 Structural classification
- 승인된 Prototype content count 위반
- LGT_01 또는 FIN_01의 자동 활성화
