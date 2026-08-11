# Operations Floor Layout Handoff v001

- Work Package: `SHOWCASE-MAP-HANDOFF-001`
- Execution Relay: `EAR-CONTEST-0002-MAP-HANDOFF-001`
- Room: `PRT-RM-002 — Operations Floor Handover Lockdown`
- Source Branch: `codex/showcase-map-design`
- Immutable Base: `f51315aea91dd00293a4f5c387f1cb748865fd11`
- Status: `REVIEW / LOCKED MAP TRANSLATION / NOT IMPLEMENTATION AUTHORITY`
- Runtime Boundary: `S1 NOT AUTHORIZED / C5 NOT AUTHORIZED / S9 NOT AUTHORIZED`

> **REVIEW ONLY — NOT A UNITY SCENE — S1 NOT AUTHORIZED**

이 문서는 잠긴 Map v001을 미래 S1 Graybox 담당자가 검토 가능한 공간 자료로
번역한다. 새 맵 방향, 새 Zone, 새 Gameplay Object, 새 Enemy 또는 새 해결법을
추가하지 않는다. 원문에 없는 exact 수치는 모두
`PROPOSED / REVIEW REQUIRED / NOT RUNTIME AUTHORITY`다.

## 1. Authority와 좌표 규칙

권위 우선순위는 최신 Human Owner 지시, Map v001·Animation v003 원문,
CR-0004, ADR-0024, CONTEST-0002, Showcase Lock/Matrix 순이다.

| 항목 | 값 | 상태 |
|---|---:|---|
| Room footprint | `48 m × 34 m` | `OWNER-APPROVED MAP v001 SOURCE` |
| X 범위 | `-24 m`~`+24 m` | `OWNER-APPROVED MAP v001 SOURCE` |
| Z 범위 | `-17 m`~`+17 m` | `OWNER-APPROVED MAP v001 SOURCE` |
| Floor | `Y = 0 m / flat` | `OWNER-APPROVED MAP v001 SOURCE` |
| Ceiling | `Y = 4.2 m` | `OWNER-APPROVED MAP v001 SOURCE` |
| Forward/North | `+Z` | `OWNER-APPROVED MAP v001 SOURCE` |
| Right/East | `+X` | `HANDOFF COORDINATE CONVENTION` |
| Rotation | Unity-style Yaw, `0° = +Z`, clockwise positive | `PROPOSED / REVIEW REQUIRED / NOT RUNTIME AUTHORITY` |

모든 위치와 파생 중심점은
[`MAP_ZONE_COORDINATES_v001.csv`](MAP_ZONE_COORDINATES_v001.csv)에 모은다.
원문에 직접 적힌 2D 좌표는 X/Z를 보존하고 Y만 flat-floor 기준 `0`으로
파생한다. 원문에 없는 Yaw, footprint, connector center와 clearance 값은
반드시 제안 상태로 유지한다.

## 2. Conflict Resolution Ledger

Human Owner의 최신 지시에 따라 아래 충돌은 작업 중단 대신 기록하고 최소
보정한다. 이 보정은 원문을 수정하거나 Runtime Authority를 만들지 않는다.

| ID | 발견 | Handoff 보정 | 상태 |
|---|---|---|---|
| `MAP-CONFLICT-001` | Runner 1이 Map v001 §9에서 `(X +1.0, Z +1.5)`, §23에서 `(+1, +2)`로 기재됨 | 첫 spatial 정의 `(1.0, 0, 1.5)`를 `ACT-RUNNER-01` 기준점으로 사용하고, 후자는 반올림된 role 설명으로 기록 | `PROPOSED / REVIEW REQUIRED / NOT RUNTIME AUTHORITY` |
| `MAP-CONFLICT-002` | 위 Runner 1 좌표는 §9의 Z2 수치 범위 `Z -8~-2` 밖이며 Z4 안에 있음 | Z2 경계를 넓히지 않고 `Z2/Z4 transition`에 배치한다. Runner 1 pressure region은 Z2, reset point는 transition으로 분리 | `PROPOSED / REVIEW REQUIRED / NOT RUNTIME AUTHORITY` |
| `MAP-CONFLICT-003` | `B-A2 (-7.0, +12.5)`가 선언된 수치 Zone 범위에 포섭되지 않음 | 좌표를 유지하고 새 Zone 없이 `Z3 North Supply Spur`로 의미상 귀속한다 | `PROPOSED / REVIEW REQUIRED / NOT RUNTIME AUTHORITY` |
| `MAP-CONFLICT-004` | `Copier (-20.5, -10.0)`가 선언된 수치 Zone 범위에 포섭되지 않음 | recognition-only 좌표를 유지하고 새 Zone 없이 `Z3 South Recognition Spur`로 의미상 귀속한다 | `PROPOSED / REVIEW REQUIRED / NOT RUNTIME AUTHORITY` |

## 3. Z0–Z7 Zone Table

Zone 범위는 Map v001 원문값이다. Zone은 서로 일부 겹치며 완전한 공간
분할망이 아니라 gameplay responsibility 영역으로 해석한다. 중심점과 폭·깊이
환산은 CSV/SVG 표시를 위한 파생치이며 Runtime Authority가 아니다.

| Zone | Source Bounds X / Z | 책임 | 진입·이탈 | 가독성 요구 |
|---|---|---|---|---|
| `Z0` Entry Staging | `X -7~+7 / Z -17~-13` | Player·Ally 시작, Direct object와 EXIT 방향 소개 | 남쪽 시작 → Z1 | Binder, Chair, Cubicle Island, Runner 1 상체, EXIT sign, Ally를 첫 화면에서 읽음 |
| `Z1` Direct Equipment Bay | `X -13~+13 / Z -13~-8` | 첫 Pickup·Throw 준비 | Z0 ↔ Z2, 좌·우 branch | `B-D1`, `C-D1`이 서로 가리지 않고 중심 reticle로 접근 가능 |
| `Z2` Central Clash Bowl | `X -10~+10 / Z -8~-2` | Direct Throw, first pressure, Supply/Catch, Rescue | Z1 ↔ Z3/Z4/Z5 | 중앙 바닥을 비우고 높은 occluder·작은 collision clutter 금지 |
| `Z3` West Logistics Loop | `X -24~-10 / Z -9~+11` | Follow, Binder Supply, Chair Fetch, Return Pass, Regroup | Z2 west split ↔ North Cross | 최소 `4.5 m` lane, `6 m × 6 m` Supply Pocket |
| `Z4` Central Cubicle Ring | `X -8~+8 / Z -2~+10` | pursuit break, Ally regroup, Bank Shot, loop reconnect | Z2 ↔ North Cross, Z3 ↔ Z5 | `16 m × 12 m` island, low partition `1.15~1.25 m`, partial north sightline |
| `Z5` East Physics Lane | `X +10~+24 / Z -9~+10` | Chair/Strap/physics approach | Z2 east split ↔ North Cross/Z6 | lane `6~8 m`, Column 양쪽 Direct/Redirect 선택, Ally clear path |
| `Z6` North Tag Arena / Drawer Gate | `X +7~+22 / Z +7~+15` | Runner 2, Tag, Drawer, four solutions, Finisher | North Cross ↔ Z7 | Tag pocket `≥6×6 m`, Finisher pocket `8×7 m`, Drawer→Parking Bay가 읽힘 |
| `Z7` Exit Run / Next-Area Tease | `X +16~+24 / Z +12~+17` | short Exit run and 3-second vista | Z6 → Exit only after Drawer resolve | run width `3.5~4 m`, climax 후 `10~20 s` 내 종료 |

### Semantic Spurs — no new Zone

- `Z3 North Supply Spur`: `B-A2`와 `FA-06`을 Z3의 북쪽 supply handoff로
  연결한다. exact polygon은 `PROPOSED / REVIEW REQUIRED / NOT RUNTIME AUTHORITY`.
- `Z3 South Recognition Spur`: Copier recognition sightline을 Z1 진입부에서
  보이게 하되 interaction·route responsibility를 부여하지 않는다.

두 Spur는 Z8이나 두 번째 Room이 아니다.

## 4. Figure-Eight Route Graph

잠긴 topology는 South Bowl, West Loop, East Lane, North Cross의
figure-eight reconnection이다. 아래 node 좌표 중 원문에 없는 값은 제안이다.

```text
                         RT-NORTH-CROSS
                         /             \
              RT-WEST-TURN             RT-EAST-ARC
                    /                       \
          RT-WEST-ENTRY                   RT-EAST-ENTRY
                    \                       /
                     RT-SOUTH-CLASH-BOWL
                              |
                         RT-ENTRY-STAGE

RT-NORTH-CROSS → RT-DRAWER-GATE → RT-PARKING-BAY → RT-EXIT
```

| Route | Ordered nodes | Locked role |
|---|---|---|
| Central fast/risky | `RT-ENTRY-STAGE → RT-SOUTH-CLASH-BOWL → RT-NORTH-CROSS → RT-DRAWER-GATE` | 빠른 pressure 노출과 직접 목표 접근 |
| West Ally/Supply | `RT-SOUTH-CLASH-BOWL → RT-WEST-ENTRY → RT-WEST-TURN → RT-NORTH-CROSS` | Follow·Supply·Return Pass·Regroup |
| East Chair/Strap | `RT-SOUTH-CLASH-BOWL → RT-EAST-ENTRY → RT-EAST-ARC → RT-NORTH-CROSS` | Chair, Column choice, Assist Attach, Redirect |
| Exit | `RT-DRAWER-GATE → RT-PARKING-BAY → RT-EXIT` | Drawer 해결 후 단방향 클라이맥스 종료 |

West와 East는 South Bowl과 North Cross 양쪽에서 다시 연결되어야 한다.
Cubicle Island를 막다른 벽으로 만들거나 North Cross를 한 lane 전용으로 만들면
figure-eight 실패다.

## 5. Actor Movement Regions

| Actor | Count | Reset/Reference | 이동 책임 | 금지 |
|---|---:|---|---|---|
| `ACT-PLAYER-01` | 1 | `(0, 0, -15.0)` | 모든 open route, object approach, Drawer solution, Exit | room boundary·solid desk·partition·closed Drawer connector 관통 |
| `ACT-ALLY-01` | 1 | `(-2.4, 0, -14.2)` | Follow Anchor Network `FA-00~FA-11`, Fetch/Supply/Brace/Rescue/Tag/Finisher Partner | 순간이동, Player 앞 막기, camera/throw/redirect lane 점유, 동시 두 행동 |
| `ACT-RUNNER-01` | 1 | `(1.0, 0, 1.5)` conflict-normalized | Z2 pressure를 중심으로 Z2/Z4 transition 순찰 | West/East 전체 봉쇄, second archetype behavior |
| `ACT-RUNNER-02` | 1 | `(15.0, 0, 13.0)` | Z6 Gatekeeper, Finisher Pocket, Drawer pressure | Exit 영구 봉쇄, new archetype, firearm/elite behavior |

Ally priority는 `Reset/Failure → Finisher → Rescue → Current Assist → Current Supply → Follow → Idle`을 보존한다. 두 Runner는 동일 archetype이며 동시에 한 명만 attack commit한다.

## 6. Gameplay Object Placement

| ID | Object | Source X/Z | Handoff Zone | Role |
|---|---|---:|---|---|
| `B-D1` | Binder | `-6.0 / -11.5` | Z1 | direct first pickup |
| `B-D2` | Binder | `+12.0 / -4.5` | Z5 | east direct/physics support |
| `B-A1` | Binder | `-19.0 / -2.5` | Z3 | west Ally supply |
| `B-A2` | Binder | `-7.0 / +12.5` | Z3 North Supply Spur | north supply; conflict resolution recorded |
| `B-A3` | Binder | `+9.5 / +10.5` | Z6 | north objective support |
| `C-D1` | Chair | `+6.0 / -11.0` | Z1 | direct Chair route |
| `C-A1` | Chair | `-18.0 / +4.0` | Z3 | Ally fetch/feed |
| `C-D2` | Chair | `+13.0 / -2.5` | Z5 | direct/Strap physics route |
| `OBJ-DRAWER-01` | Drawer | `+15.5 / +10.5` | Z6 | initial connector blocker |
| `OBJ-COPIER-01` | Copier | `-20.5 / -10.0` | Z3 South Recognition Spur | recognition-only; no pickup/strap/solution role |
| `POI-PARKING-BAY` | Drawer destination | `+20.0 / +10.5` | Z6 | route-open physical end state |

수량은 Binder 5, Chair 3, Drawer 1, Copier 1을 정확히 유지한다. 사용·실패한
물체는 Reset 전까지 방에 남으며 hidden inventory, 자동 삭제, 자동 보충,
동료 순간 반환을 사용하지 않는다.

## 7. Q Visibility와 접근성

항상 하나의 Q prompt만 표시하며 highest valid context가 이긴다.

| Priority | Prompt | Primary staging | Visibility/approach handoff |
|---:|---|---|---|
| 1 | `BREAK GRAB` | Runner Grab이 가능한 Z2/Z6 | Ally path가 열리고 `7 m` 이내라는 source condition을 보존; occluder 뒤에서 거짓 prompt 금지 |
| 2 | `TEAM FINISH` | `POI-FINISHER-POCKET` | TEAM READY, Runner 2 stagger, valid Chair, closed Drawer, Ally within `7 m` 모두 유효할 때만 |
| 3 | `ASSIST ATTACH` | `POI-STRAP-ASSIST` | Chair load-bearing frame, Player/Ally 양측 접근, redirect exit lane이 동시에 보여야 함 |
| 4 | `TAG SHOVE / SET HIM UP` | Z2/Z6 stagger pockets | Staggered Runner와 collision lane을 같은 camera turn에서 읽을 수 있어야 함 |
| 5 | `SEND BINDER / SEND CHAIR` | Z3 supply pockets, eligible world object | 실제 candidate와 solid path/clear supply lane이 있어야 함 |
| 6 | `REGROUP` | otherwise | Ally follow target이 player·camera·throw lane을 막지 않는 위치로 재결합 |

Q visibility는 UI 구현 권한이 아니다. S1 graybox는 prompt origin과 대상 사이의
solid sightline/approach 공간만 검증한다.

## 8. Drawer Four-Solution Handoff

새 해결법을 추가하지 않는다.

| Solution | Approach | Required spatial chain | Route-open result |
|---|---|---|---|
| Direct Chair Throw | Z1 `C-D1` 또는 Z5 `C-D2` → Z6 | readable wind-up lane → Drawer face → Parking Bay direction | Drawer가 connector에서 Parking Bay 쪽으로 이동 |
| Cargo Strap Redirect | Z5 Assist/Redirect pockets → Z6 | grounded Chair, Ally-assisted attach, Player-owned tension/redirect, Ally clear lane | bounded Chair impact/redirect가 Drawer 이동에 기여 |
| Runner Causal Collision | Z2/Z6 Tag/impact setup | attributed Chair/object result → Runner → Drawer | Runner의 비살상 causal collision으로 Drawer 이동 |
| Team Finisher | Z6 Finisher Pocket | TEAM READY + Runner 2 stagger + valid Chair + Ally → `FIN-P-001` | Chair → Runner → Drawer, Exit activation, immediate control return |

Initial Drawer connector는 `0.8 m 이하`, open state는 `3.0 m 이상`을 보존한다.
정확한 Rigidbody threshold·force·mass·trigger volume은 이 문서의 권한이 아니다.

## 9. PASS / BRACE / BREAK와 HANDOVER CRASH Staging

- PASS: Ally-supplied Binder Catch가 읽히는 West/Central sightline.
- BRACE: Z5 `POI-STRAP-ASSIST`에서 Ally-assisted carabiner attach 완료.
- BREAK: Z2/Z6에서 Grab Rescue 또는 Combat Tag 완료.
- 서로 다른 두 badge가 TEAM READY를 만든다. decay·소비·run persistence 없음.

`FIN-P-001 — HANDOVER CRASH`는 약 `2.5 s`의 bounded sequence다.

1. `0.00~0.35 s`: commit/camera offset.
2. `0.35~0.90 s`: Ally side setup.
3. `0.90~1.30 s`: Player Strap Pull 또는 Direct Chair Throw.
4. `1.30~1.55 s`: Ally clear.
5. `1.55~1.90 s`: Chair → Runner → Drawer.
6. `1.90~2.60 s`: non-destructive presentation, Exit activation, control return.

Finisher 없이 Direct Chair와 Strap route가 모두 유지되어야 한다.

## 10. Sightline, Occlusion, Camera

- 첫 화면: Direct Binder, Direct Chair, Cubicle Island, Runner 1 upper body,
  EXIT sign, Ally를 동시에 이해할 수 있는 composition을 목표로 한다.
- Z2 중앙에는 high desk, copier, plant, high partition, floor clutter를 두지 않는다.
- Z4 low partition은 pursuit를 끊되 Player·Ally·Runner 구분과 North partial
  sightline을 완전히 차단하지 않는다.
- Z5 Column은 Direct/Redirect 선택을 만들지만 `1.5 m 이상` 주변 clearance를
  보존한다.
- 주 통로 `3.5 m 이상`, Tag Pocket `6×6 m 이상`, Finisher Pocket `8×7 m`를
  유지한다.
- 상세 collision/camera handoff는
  [`MAP_COLLISION_AND_CAMERA_CLEARANCE_v001.md`](MAP_COLLISION_AND_CAMERA_CLEARANCE_v001.md)를 따른다.

## 11. Spawn / Reset Baseline

Reset은 다음 authored baseline을 복구해야 한다. 이 문서는 Scene 또는 Reset
code를 만들지 않으며 미래 S1+ 검증 책임만 정의한다.

- Player, Ally, Runner 1/2 위치·방향·이동 상태.
- Binder 5, Chair 3, Drawer 1, Copier 1의 position/rotation과 physical state.
- Drawer blocked, Exit inactive, TEAM READY badges clear.
- no carry, no supply reservation, no active relay, no strap, no finisher.
- all Follow Anchors and route nodes remain static.
- missed/used objects return only through authored Reset, never passive teleport.

Yaw는 원문 미정이므로 CSV의 actor/object Yaw는 모두 review 대상이다.

## 12. Graybox Construction Order — future S1 only

다음은 실행 순서 handoff이며 S1 실행 승인이 아니다.

1. `ROOM-PRT-RM-002` flat floor, perimeter and `4.2 m` ceiling shell.
2. Z0–Z7 review overlays and source-coordinate markers.
3. `ARCH-CUBICLE-ISLAND` low partition ring and deliberate gaps.
4. West/East/South/North route spine and figure-eight reconnection.
5. fixed desks, Column, impact partition and Exit frame as primitive solids.
6. actors and all ten physical/recognition object instances at reset markers.
7. Drawer closed/open clearance volumes and Parking Bay.
8. Q sightline pockets, PASS/BRACE/BREAK staging and Finisher Pocket.
9. camera/carry clearance audit, solid-wall continuity audit and reset marker audit.
10. route matrix execution; no Production Art swap.

## 13. Target 5–7 Minute Flow

| Time | Beat | Map responsibility |
|---|---|---|
| `0:00~0:25` | immediate control/Pickup | Z0–Z1 direct objects readable |
| `0:25~1:10` | first Binder impact | Z2 open clash bowl and first Runner reaction |
| `1:10~2:00` | Ally Supply/Catch/Queued Throw | Z3/Z2 connection and physical pass lane |
| `2:00~3:10` | route choice | West/central/east alternatives visible |
| `3:10~4:20` | Rescue/Tag/Attach | distinct badge opportunities without forced completion of all |
| `4:20~5:30` | Drawer objective | four solution families converge on Z6 |
| `5:30~6:20` | optional Team Finisher | bounded HANDOVER CRASH or non-finisher clear |
| `6:20~7:00` | Exit/tease/result | Z7 short run, 3-second vista, no extra wave |

## 14. Review Acceptance

- 48×34 m, flat floor, Z0–Z7 exactly eight, figure-eight topology preserved.
- Player 1, mobile Ally 1, identical Runner 2 preserved.
- Binder 5, Chair 3, Drawer 1, Copier 1 preserved.
- Q priority, PASS/BRACE/BREAK, four Drawer solutions and FIN-P-001 exact.
- conflict resolutions remain visible and review-only.
- no Unity Scene, Runtime, Prefab, Package, Test, Build or Production Art claim.
