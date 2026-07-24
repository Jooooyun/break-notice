# Vertical Slice Asset List

## Characters

| ID | Asset | Status |
|---|---|---|
| CHR-00 | Player | Visual locked, 3D pending |
| CHR-01 | First Ally — facilities/logistics employee | Visual locked, 3D and gameplay pending |
| ENY-01 | Compliance Runner | Design pending |
| ENY-02 | Partition Guard | Design pending |
| ENY-03 | Elite variant | Conditional |

## Player Gear

GEAR-01 Production Visual은 `LOCKED`이며 3D·기능 구현은 `PENDING`이다.

- Industrial harness
- Forearm load-assist brace
- Waist cargo reel
- Black-yellow strap
- Silver carabiner/clamp
- ID badge
- Red necktie

## Gameplay Presentation

| ID | Asset | Status |
|---|---|---|
| PGT-01 | Gameplay Camera | Visual locked; exact camera behavior and implementation pending |
| PGT-02 | Chair Redirect Core Loop | Visual locked; exact mechanics, physics and implementation pending |
| PGT-03 | Chain Reaction Aftermath | Visual locked; hard destruction budget locked; exact Chain, G5 asset identity, VFX implementation pending |
| LGT-01 | Night Archive Lighting | Visual locked; exact lighting values and rendering implementation pending |
| FIN-01 | Conference Table Team Finisher | Visual locked; PR-F-022 remains COULD, outside approved Vertical Slice, exact function and implementation pending |

## Office Core Kit

Status: `OWNER-APPROVED / LOCKED`

Authority:

- `docs/03_art/OFFICE_CORE_KIT.md`
- `docs/09_adrs/ADR-0017-OFFICE-CORE-KIT-SCOPE-LOCK.md`

Actual 3D assets: `NOT CREATED / PRODUCTION PENDING`

이 목록은 production-family 및 reuse scope만 고정한다. model, texture,
material, room layout, Prefab 또는 Unity asset은 생성되지 않았고
제작 권한도 없다.

### Prototype-Required Architecture and Fixed Fixture Families — 7

| ID | Family | Prototype status |
|---|---|---|
| OCK-A-001 | Solid Office Wall System | Fixed / non-interactive |
| OCK-A-002 | Fixed Office Door and Frame | Fixed / non-interactive |
| OCK-A-003 | Low Cubicle Partition System | Fixed / non-interactive |
| OCK-A-004 | Office Floor Surface | Fixed / non-interactive |
| OCK-A-005 | Suspended Ceiling System | Fixed / non-interactive |
| OCK-A-006 | Fixed Column / Impact Surface | Fixed / non-Structural |
| OCK-A-007 | Neutral Fluorescent Ceiling Fixture | Visual only / no LGT_01 activation |

### Prototype-Required Furniture and Gameplay Families — 5

| ID | Family | Prototype mapping |
|---|---|---|
| OCK-F-001 | Fixed Work Desk | Fixed / non-interactive |
| OCK-F-002 | Wheeled Office Chair | `OBJ-P-M-001` / gameplay-interactive |
| OCK-F-003 | Rolling Drawer Cabinet | `OBJ-P-M-002` / gameplay-interactive |
| OCK-F-004 | Copier-Class Asset | `OBJ-P-H-001` / recognition-only |
| OCK-P-001 | File Binder | `OBJ-P-L-001` / gameplay-interactive |

### Prototype-Required Decorative and Dressing Families — 4

| ID | Family | Prototype status |
|---|---|---|
| OCK-D-001 | Workstation Electronics Cluster | Non-interactive |
| OCK-D-002 | Paper and Archive Box Cluster | Non-interactive |
| OCK-D-003 | Office Plant and Waste Bin Cluster | Non-interactive |
| OCK-D-004 | Signage, Asset Tag, Tape and Wear Decal Set | Shared visual variation only |

Prototype Office Core Kit total은 정확히 `16`개 family다.

### Exact Prototype Gameplay Mapping

- Gameplay-interactive definition: File Binder, Wheeled Office Chair,
  Rolling Drawer Cabinet의 정확히 `3`
- Recognition-only definition: Copier-Class Asset의 정확히 `1`
- Fixed/decorative family: 정확히 `12`
- Instance: File Binder `3`, Wheeled Office Chair `2`, Rolling Drawer
  Cabinet `1`, Copier-Class Asset `1` — 총 `7`

Office Core Kit은 새 definition 또는 instance를 추가하지 않는다.

### Later-Slice Extension Candidates — 8

| ID | Candidate family | Current boundary |
|---|---|---|
| OCK-X-A-001 | Glass Partition System | Static default; separate G5 Breakable decision required |
| OCK-X-A-002 | Meeting Room Frame and Double-Door System | Static authored candidate |
| OCK-X-A-003 | Archive Shelving Bay | Static default; not automatically Structural |
| OCK-X-A-004 | Exposed Ceiling and Cable-Tray System | Static default; not automatically Structural |
| OCK-X-F-001 | Records Cart | Potential movable; not included |
| OCK-X-F-002 | Mobile Whiteboard | Potential movable; not included |
| OCK-X-F-003 | Metal Storage Cabinet | Potential Heavy/fixed; G5 classification required |
| OCK-X-F-004 | Conference Table | Potential Heavy; no FIN_01 activation |

이 정확히 여덟 family는 Prototype 또는 자동 Vertical Slice content가
아니며 asset creation, Breakable identity 또는 Structural Feature
승인이 아니다.

## Destruction Scope Status

Status: `OWNER-APPROVED BUDGET / G5 ASSET IDENTITIES DRAFT`

- Prototype에는 Breakable 또는 Structural asset이 `0`개다.
- ordinary Later-Slice Breakable identity는 아직 G5 Draft이며 room당
  편입 상한은 `2`개다.
- Structural Feature는 PR-F-024 `SHOULD / PARTIAL`을 유지하고 별도 G5
  승인을 받은 경우에만 room당 최대 `1`개다.
- glass partition, rack, wall, door, furniture와 electronics 후보는 이
  예산만으로 Breakable 또는 Structural content로 승격되지 않는다.
- exact fragment와 material-response asset은
  `docs/04_technical/DESTRUCTION_BUDGET_AND_IMPLEMENTATION_RULES.md`의
  ordinary `4`, Structural `8`, pool `16`, awake `12`, carrier `2`,
  small-response `32`/`64`와 room-live `96` cap 안에 있어야 한다.
- small dynamic debris, runtime mesh fracture, recursively breakable
  fragment와 player-interactive fragment asset은 만들지 않는다.

final Breakable asset list는 `DRAFT`이고 Structural inclusion은
conditional `SHOULD / PARTIAL`이다. Office Core Kit Lock은 G5를
통과시키지 않는다.

LGT_01은 Records Archive-local `COULD`이고 Office Core Kit의 fixture,
shelving 또는 archive candidate로 자동 활성화되지 않는다. FIN_01은
계속 `OUT / COULD`이며 Conference Table candidate로 자동 활성화되지
않는다.

## Remaining Production-Support Boundary

`CHR_00 Player Turnaround Production Brief`는
`OWNER-APPROVED / READY`다. 정확한 권위는
`docs/03_art/PLAYER_TURNAROUND_PRODUCTION_BRIEF.md`다.

미래 후보 경로
`references/exploration/CHR_00_Player_Turnaround_v001_REVIEW.png`는
예약만 되었고 현재 후보는 `NOT CREATED`, 이미지 생성은
`NOT AUTHORIZED / EXPLICIT OWNER REQUEST REQUIRED`, checklist는
`UNCHECKED`다. Brief 승인은 이미지·모델·texture·material·Unity asset
생성 또는 Visual Candidate 승인이 아니다. Office Core Kit Lock은
character, gameplay feature 또는 Visual Canon authority를 변경하지
않는다. 다음 허가 경계는
`OWNER EXPLICIT IMAGE-GENERATION REQUEST REQUIRED — CHR_00 PLAYER TURNAROUND`다.

## VFX

- Material impacts
- Paper burst
- Toner cloud
- Glass burst
- Dust
- Metal spark
- Chain Link
- Momentum mechanical feedback

## Audio

- Weight-class flight and impact layers
- Material impacts
- Strap reel, attach, tension, release
- Brace motor and lock
- Ally call/pass voice stingers
- Office aftermath ambience
