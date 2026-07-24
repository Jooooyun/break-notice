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

## Props

### Light

- File binder
- Keyboard
- Stapler
- Mug

### Medium

- Office chair
- Monitor
- Mobile whiteboard
- Rolling drawer
- Records cart

### Heavy

- Copier
- Metal cabinet
- Conference table

## Structural

- Glass partition
- Ceiling cable tray or archive rack
- Meeting-room wall/door feature

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

Office Core Kit과 final Breakable asset list는 `DRAFT`다.

## Environment

Office Core Kit 문서의 모듈과 Room 1~3 변주.

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
