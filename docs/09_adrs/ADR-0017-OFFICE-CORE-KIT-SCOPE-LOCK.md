# ADR-0017 — Office Core Kit Scope and Reuse Lock

- Status: Accepted
- Date: 2026-07-25
- Decision Source: `TASK-0056 delegated Owner decision`
- Scope: documentation-level Art and Production Support asset-family lock

## Context

기존 Office Core Kit Draft는 여러 책상·의자·가구 변형, Prototype에
승인되지 않은 keyboard·mug·monitor·cart 상호작용과 기본 파손 후보를
폭넓게 열어 두었다. Owner-approved Prototype Content Lock은 네 object
definition과 일곱 instance, 세 gameplay-interactive definition, 하나의
recognition-only copier와 zero-destruction profile을 이미 고정했다.

Design Freeze를 위해 Prototype에 필요한 최소 재사용 family와
Later-Slice 후보, 변형 상한, 상호작용·파괴·재사용 경계를 하나의
Owner-approved production-support 기준으로 고정한다.

## Decision

세부 기준은 `docs/03_art/OFFICE_CORE_KIT.md`를 따른다. Office Core Kit은
bounded reusable production-family inventory이자 Art/Production Support
scope lock이다. Unity folder, Prefab inventory, final asset delivery,
room-count 결정, 모든 후보의 Vertical Slice 편입 약속 또는
modeling·implementation 권한이 아니다. `OCK-*` ID는 문서 참조 ID일
뿐 file, Prefab, ScriptableObject, runtime 또는 serialized identity를
승인하지 않는다.

### Exact Prototype-Required Families

Architecture and fixed fixture family는 정확히 일곱 개다.

- `OCK-A-001 Solid Office Wall System`
- `OCK-A-002 Fixed Office Door and Frame`
- `OCK-A-003 Low Cubicle Partition System`
- `OCK-A-004 Office Floor Surface`
- `OCK-A-005 Suspended Ceiling System`
- `OCK-A-006 Fixed Column / Impact Surface`
- `OCK-A-007 Neutral Fluorescent Ceiling Fixture`

Furniture and approved gameplay-object family는 정확히 다섯 개다.

- `OCK-F-001 Fixed Work Desk`
- `OCK-F-002 Wheeled Office Chair`
- `OCK-F-003 Rolling Drawer Cabinet`
- `OCK-F-004 Copier-Class Asset`
- `OCK-P-001 File Binder`

Decorative and dressing family는 정확히 네 개다.

- `OCK-D-001 Workstation Electronics Cluster`
- `OCK-D-002 Paper and Archive Box Cluster`
- `OCK-D-003 Office Plant and Waste Bin Cluster`
- `OCK-D-004 Signage, Asset Tag, Tape and Wear Decal Set`

Prototype-required family는 총 `16`개이며 열일곱 번째 family를
허용하지 않는다.

### Prototype Gameplay and Collision Boundary

Gameplay-interactive definition은 File Binder, Wheeled Office Chair와
Rolling Drawer Cabinet의 정확히 세 개다. Copier-Class Asset은 정확히
하나의 recognition-only definition이다. 나머지 `12`개 family는 fixed,
non-interactive 또는 decorative다. Binder `3`, Chair `2`, Drawer `1`,
Copier `1`의 정확히 `7` instance를 유지한다.

static architecture와 fixed desk는 향후 단순 static collision을 받을 수
있지만 Scoop, Carry, Throw, Catch, Relay, Cargo Strap, HeavyAction,
Causal-carrier 또는 Prototype Breakable authority를 얻지 않는다.
Decorative family는 gameplay body, collision, prompt, highlight,
interaction 또는 Causal Result authority를 갖지 않는다.

### Prototype Destruction Boundary

모든 Prototype-required family의 Breakable과 Structural Feature state는
`NONE`, runtime fracture는 `PROHIBITED`, destruction fragment budget은
`0`이다. non-destructive impact presentation은 별도 Draft다.

### Exact Later-Slice Extension Candidates

G5에서만 선택할 수 있는 후보는 정확히 다음 여덟 개다.

- `OCK-X-A-001 Glass Partition System`
- `OCK-X-A-002 Meeting Room Frame and Double-Door System`
- `OCK-X-A-003 Archive Shelving Bay`
- `OCK-X-A-004 Exposed Ceiling and Cable-Tray System`
- `OCK-X-F-001 Records Cart`
- `OCK-X-F-002 Mobile Whiteboard`
- `OCK-X-F-003 Metal Storage Cabinet`
- `OCK-X-F-004 Conference Table`

아홉 번째 후보를 허용하지 않는다. 이 family들은 Prototype 또는 자동
Vertical Slice content가 아니며, Breakable identity, Structural
Feature 또는 asset 제작 권한으로 승인되지 않는다. 기존 승인 mapping이
없는 exact Weight Class assignment는 deferred다.

### Structural, Breakable, LGT_01 and FIN_01 Boundary

ordinary Breakable 또는 Structural Feature 분류에는 별도 G5 content
결정이 필요하며, 선택된 family는 ADR-0016과
`DESTRUCTION_BUDGET_AND_IMPLEMENTATION_RULES.md`를 따른다. glass는
자동 Breakable이 아니고 shelf와 cable tray는 자동 Structural이
아니며 wall·door는 자동 destructible이 아니다. metal cabinet은 자동
Heavy/Structural이 아니고 conference table은 자동 Finisher object가
아니다.

archive family 또는 fluorescent fixture는 LGT_01을 활성화하지 않는다.
LGT_01은 Records Archive-local `COULD`다. Conference Table은 FIN_01,
CHR_02, 추가 normal-combat ally 또는 Slice/final-product scope를
활성화하지 않으며 FIN_01은 `OUT / COULD`다.

### Geometry, Variant and Reuse Caps

Prototype의 각 furniture/gameplay family는 base geometry family 하나,
각 architecture system은 base system 하나와 필요한 modular shape piece
최대 세 개, 각 decorative cluster는 non-interactive prop form 최대 세
개, 각 family는 finish/material variant 최대 두 개로 제한한다. G5에서
선택된 extension도 base geometry family 하나와 variant 최대 두 개를
따른다.

department name 또는 room color만을 위한 unique geometry,
color/decal-only second family와 Change Control 없는 room-only duplicate를
허용하지 않는다. department variation은 shared material, decal,
signage, tag, paper, wear/dirt, 별도 승인 lighting과 arrangement를
재사용한다.

shared material responsibility는 painted wall/ceiling, carpet/vinyl,
laminate/composite, painted metal, worn plastic/rubber, paper/cardboard와
office fabric/upholstery를 포함한다. 이는 texture count, resolution,
shader, material instance, import 또는 rendering 결정을 승인하지 않는다.

## Exclusions

이 결정은 실제 model, texture, material, decal atlas, lighting asset,
character/player turnaround, enemy, GEAR_01 model, animation, VFX, audio,
UI, camera, final layout·dimension·room count, final Breakable identity,
Structural inclusion, LGT_01, FIN_01, Unity asset 또는 구현을 승인하지
않는다.

## Change Control

열일곱 번째 Prototype family, 아홉 번째 extension candidate, 세 번째
finish/material variant, 새 interactive definition, department 전용
Kit 또는 geometry, 새 Breakable/Structural 분류, Prototype content count 위반과
LGT_01/FIN_01 자동 활성화에는 Change Control과 Owner 승인이 필요하다.

## Consequences

- Office Core Kit 목록과 reuse policy는 `OWNER-APPROVED / LOCKED`다.
- 실제 asset creation은 `NOT STARTED / NOT AUTHORIZED`다.
- Design Freeze는 `30 / 32`로 진행되지만 Full Design Lock은
  `INCOMPLETE`다.
- G4와 G5는 `NOT PASSED`, Graybox는 `NOT STARTED`, Implementation
  Gate는 `CLOSED`다.
