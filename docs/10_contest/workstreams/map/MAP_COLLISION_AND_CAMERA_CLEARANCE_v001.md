# Map Collision and Camera Clearance v001

- Work Package: `SHOWCASE-MAP-HANDOFF-001`
- Room: `PRT-RM-002 — Operations Floor Handover Lockdown`
- Status: `REVIEW / COLLISION-CAMERA HANDOFF ONLY`
- Runtime Boundary: `NO COLLIDER AUTHORED / NO CAMERA IMPLEMENTATION / S1 NOT AUTHORIZED`

> **REVIEW ONLY — NOT A UNITY SCENE — S1 NOT AUTHORIZED**

이 문서는 잠긴 Map v001의 flat-floor, lane, pocket, sightline 요구를 미래 S1
primitive construction과 collision audit 언어로 번역한다. Collider type,
layer, camera probe와 exact physical envelope는 Runtime 구현값이 아니다.

## 1. Source-Locked Clearance

| ID | Requirement | Authority |
|---|---:|---|
| `CLR-ROOM` | `48 m × 34 m`, X `-24~+24`, Z `-17~+17` | `OWNER-APPROVED MAP v001 SOURCE` |
| `CLR-CEILING` | `4.2 m` | `OWNER-APPROVED MAP v001 SOURCE` |
| `CLR-FLAT` | flat floor; threshold, carpet step, cable collider, stair, small lip prohibited | `OWNER-APPROVED MAP v001 SOURCE` |
| `CLR-MAIN` | main route `≥3.5 m` | `OWNER-APPROVED MAP v001 SOURCE` |
| `CLR-WEST` | West lane `4.5~5.5 m`; minimum `4.5 m` | `OWNER-APPROVED MAP v001 SOURCE` |
| `CLR-EAST` | East lane `6~8 m` | `OWNER-APPROVED MAP v001 SOURCE` |
| `CLR-NORTH-CROSS` | North Cross `5 m` | `OWNER-APPROVED MAP v001 SOURCE` |
| `CLR-DRAWER-CLOSED` | initial connector `≤0.8 m` | `OWNER-APPROVED MAP v001 SOURCE` |
| `CLR-DRAWER-OPEN` | open connector `≥3.0 m` | `OWNER-APPROVED MAP v001 SOURCE` |
| `CLR-EXIT-RUN` | Exit Run `3.5~4 m` | `OWNER-APPROVED MAP v001 SOURCE` |
| `CLR-TAG` | Tag Pocket `≥6×6 m` | `OWNER-APPROVED MAP v001 SOURCE` |
| `CLR-FINISHER` | Finisher Pocket `8×7 m` | `OWNER-APPROVED MAP v001 SOURCE` |
| `CLR-PARTITION` | low partition `1.15~1.25 m`; camera rule max `1.25 m` | `OWNER-APPROVED MAP v001 SOURCE` |
| `CLR-COLUMN` | Column `0.7×0.7×4.2 m`, surrounding clearance `≥1.5 m` | `OWNER-APPROVED MAP v001 SOURCE` |
| `CLR-EXIT-OPENING` | Exit opening width `1.8~2.0 m` | `OWNER-APPROVED MAP v001 SOURCE` |

## 2. Proposed Graybox Collision Primitives

아래 값은 원문에 없는 exact construction proposal이다. 모두
`PROPOSED / REVIEW REQUIRED / NOT RUNTIME AUTHORITY`다.

| Element | Proposed primitive | Proposed envelope | Handoff intent |
|---|---|---:|---|
| Perimeter wall | static box segments | `0.30 m` thickness | room 밖 camera/player/object escape 방지; Exit opening 제외 |
| Floor | single static plane/box | top at `Y=0`; proposed `0.20 m` thickness | seam·step 없는 continuous support |
| Ceiling shell | static review plane | underside at `Y=4.2 m` | camera vertical audit marker; gameplay collider 여부는 future review |
| Low partitions | static boxes | proposed `0.15 m` thickness; height within source range | camera가 위로 읽되 actors/objects는 solid continuity 유지 |
| Fixed desks | simplified static boxes | proposed `0.75 m` height; individual footprint follows CSV proposal | no drawers/cables/small protrusion colliders |
| Column | static box | source `0.7×0.7×4.2 m` | both-side route and camera clearance audit |
| Drawer | one bounded dynamic body | exact collider pending | closed connector blocking and authored Parking Bay result |
| Chairs | bounded dynamic body per instance | exact collider pending | floor-supported carry/throw/feed/redirect; caster detail not collision anchor |
| Binders | bounded dynamic body per instance | exact collider pending | visible pickup/throw/catch continuity |
| Copier | static or non-usable recognition proxy | exact footprint pending | not carry/throw/strap/solution target |
| Exit frame | static box segments | proposed opening midpoint `1.9 m` | 1.8~2.0 m source range review; no second Room |

Primitive dimensions must be replaced only through later authorized review. No
production mesh collider or mesh-convex decision is made here.

## 3. Collision Ownership Handoff

| Surface / Actor | Must block | Must not imply | Reset responsibility |
|---|---|---|---|
| Perimeter / fixed architecture | Player, Ally, Runners, carried/released physical objects, camera query | breakable wall, arbitrary destruction, room transition | static authored state |
| Low partitions / desks / Column | actor and object physical continuity | climb, vault, cover system, structural breakage | static authored state |
| Binder / Chair | world collision, carry/throw ownership and Reset restoration | hidden inventory, passive deletion, duplicate spawn | exact transform/velocity/sleep restoration |
| Drawer | physical collision and blocked/open room-state handoff | direct pickup, inventory, unrestricted towing | blocked transform and physical state |
| Copier | recognition collision only as needed | HeavyAction, strap, direct acquisition, Drawer solution | recognition transform |
| Player / Ally / Runner | grounded actor separation | unrestricted ragdoll or lethal response | actor transform/state |
| Camera query | fixed solid occluders; existing carried-object exclusion contract may remain | gameplay collision mutation or wall bypass | presentation state only |

Collision ownership must preserve one mutable-state owner and the existing
physical continuity rule. Camera avoidance may not change gameplay layers in a
way that permits Binder/Chair to pass through walls.

## 4. Proposed Camera Review Envelope

다음 exact 값은 source Canon이 아니며 모두
`PROPOSED / REVIEW REQUIRED / NOT RUNTIME AUTHORITY`다.

| ID | Proposed review value | Purpose |
|---|---:|---|
| `CAM-PROBE-RADIUS` | `0.35 m` | primitive wall/corner camera sweep reserve |
| `CAM-WALL-BUFFER` | `0.20 m` | visible wall penetration 방지용 review margin |
| `CAM-SHOULDER-LATERAL` | `1.00 m` clear side reserve at pinch points | over-the-shoulder composition 전환 여유 |
| `CAM-CORNER-POCKET` | `2.0×2.0 m` clear floor at route turns | wall snap/rapid occlusion recovery review |
| `CAM-VISIBILITY-BAND` | `Y 1.20~2.80 m` free of signs/cables/decorative solids | Player/Ally/Runner silhouette와 target line 유지 |
| `CAM-CARRIED-MEDIUM-TURN` | proposed `2.5 m` clear-radius audit marker | carried Chair near-wall continuity review; physical tuning authority 아님 |

이 envelope는 Cinemachine distance, FOV, damping, Aim blend, camera shake 또는
runtime collision algorithm을 정하지 않는다. 기존 C0–C4 accepted camera와
carry-wall repair를 회귀 dependency로만 보존한다.

## 5. Location-by-Location Clearance

### Z0 / Z1 — Entry and Direct Equipment

- Player와 Ally reset markers 사이를 static prop으로 채우지 않는다.
- `B-D1`과 `C-D1`의 pickup approach가 겹쳐도 서로를 완전히 숨기지 않게 한다.
- Copier spur는 southwest recognition silhouette만 제공하고 direct route를
  좁히지 않는다.
- Proposed camera corner pockets는 room south corners가 아니라 Z0→Z1 route
  turns에만 사용한다.

### Z2 — Central Clash Bowl

- open center를 유지한다. fixed desk, plant, Copier, high partition 또는 작은
  floor collider 금지.
- `ACT-RUNNER-01`은 Z2/Z4 transition에서 Z2 pressure를 제공하되 West/East
  lower branch를 동시에 닫지 않는다.
- carried Binder/Chair와 camera가 perimeter/Island solid를 우회해 관통하지
  않아야 한다.

### Z3 — West Logistics Loop

- source minimum `4.5 m` lane과 `6×6 m` Supply Pocket을 보존한다.
- `ARCH-SUPPLY-DESK-WEST` proposed footprint는 actor pair와 grounded Chair가
  동시에 방향 전환할 수 있게 review한다.
- North Supply Spur는 `B-A2`, `FA-06`, North Cross를 연결하되 새 Zone/Room처럼
  닫힌 alcove가 되지 않는다.

### Z4 — Central Cubicle Ring

- `ARCH-CUBICLE-ISLAND`은 source `16×12 m`; exact gap/piece layout은 제안이다.
- partition height는 `1.15~1.25 m` 안에서 player/target silhouette를 부분적으로
  보존한다.
- desk monitor/cable dressing collider는 이 handoff에서 만들지 않는다.
- four-way reconnect 중 하나라도 fixed desk로 끊기면 failure다.

### Z5 — East Physics Lane

- source `6~8 m` lane을 유지한다.
- `ARCH-COLUMN-EAST` 주변 source `≥1.5 m`는 camera probe가 아니라 physical
  route clearance로 측정한다.
- Column 오른쪽 Direct Throw와 왼쪽 Strap Redirect가 둘 다 유효해야 한다.
- `POI-STRAP-ASSIST`는 Player/Ally/Chair contact와 Ally clear path를 분리한다.
- `POI-REDIRECT-ARC` 이후 Ally가 lane에 남지 않는다.

### Z6 — North Tag Arena / Drawer Gate

- Tag Pocket source minimum `6×6 m`, Finisher Pocket source `8×7 m`를 유지한다.
- `ARCH-DESK-CRASH`와 `ARCH-IMPACT-PARTITION`의 proposed footprints는 네 Drawer
  solution 중 어느 것도 제거하면 안 된다.
- Chair → Runner → Drawer line과 Ally clear exit가 동시에 존재해야 한다.
- Drawer closed connector `≤0.8 m`는 정상 traversal을 막되 false opening으로
  보이지 않게 한다.
- Drawer open connector `≥3.0 m`는 Player+Ally가 충돌 없이 통과할 수 있게 한다.

### Z7 — Exit Run

- source `3.5~4 m` run width와 source opening range `1.8~2.0 m`를 구분한다.
- Player/Ally가 함께 달릴 때 camera와 actor가 장시간 겹치지 않게 한다.
- Security Shutter와 vista는 visual-only이며 collider 뒤에 두 번째 playable
  Room을 만들지 않는다.

## 6. Sightline / Occlusion Gates

| Gate ID | Observer | Required target set | Blocker rule |
|---|---|---|---|
| `LOS-ENTRY` | Player reset | B-D1, C-D1, Cubicle Island, Runner 1 upper body, EXIT sign, Ally | 한 fixed element가 두 개 이상 primary read를 완전 차단하지 않음 |
| `LOS-WEST` | RT-WEST-ENTRY | Supply Desk, B-A1/C-A1, Wide Turn direction | high occluder 금지; low furniture only |
| `LOS-NORTH-SUPPLY` | RT-WEST-TURN | B-A2/FA-06 and North Cross direction | spur가 dead end처럼 보이지 않음 |
| `LOS-EAST` | RT-EAST-ENTRY | C-D2, Column, both Column sides, Assist Pocket | Column은 선택을 만들되 one-sided wall이 아님 |
| `LOS-NORTH-CROSS` | RT-NORTH-CROSS | Drawer, Parking Bay direction, Runner 2, Exit sign | Cubicle/desk가 objective 전체를 숨기지 않음 |
| `LOS-TAG` | POI-TAG-POCKET | staggered Runner, Ally approach, intended collision lane | Q prompt 대상과 결과 lane이 관계없이 보이면 failure |
| `LOS-FINISHER` | POI-FINISHER-POCKET | Player, Ally, valid Chair, Runner 2, Drawer | causal sequence의 한 항이 전체 duration 동안 완전 occluded되면 failure |
| `LOS-EXIT` | RT-PARKING-BAY | active Exit and short vista | 추가 wave/long corridor를 암시하지 않음 |

## 7. Finisher Camera Boundary

Map v001 source values:

- entry blend `0.25 s`.
- slow motion maximum `0.3 s`.
- return blend `0.3 s`.
- whole sequence approximately `2.5 s` and within the source bounded sequence.

Map handoff must reserve the `8×7 m` pocket and clear causal line only. Camera
offset, lens, FOV, damping, shot selection and timeline implementation are not
authorized. Player control returns immediately after the bounded result.

## 8. Collision Handoff Checklist

- [ ] no threshold, stair, cable collider or small floor lip.
- [ ] outer room continuity and Exit-only opening.
- [ ] all fixed architecture uses primitive review collision.
- [ ] low partition never exceeds source `1.25 m`.
- [ ] Binder 5 and Chair 3 retain physical continuity until Reset.
- [ ] Drawer closed/open envelopes are distinct and observable.
- [ ] Copier remains recognition-only.
- [ ] actor/carry/camera near-wall regression is explicitly retested under future Relay.
- [ ] no runtime fracture, breakable, arbitrary destruction or extra gameplay object.
- [ ] no Production Art mesh collider before S9 authorization.
