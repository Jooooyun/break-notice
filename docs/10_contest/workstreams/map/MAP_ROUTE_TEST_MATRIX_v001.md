# Map Route Test Matrix v001

- Work Package: `SHOWCASE-MAP-HANDOFF-001`
- Room: `PRT-RM-002 — Operations Floor Handover Lockdown`
- Status: `REVIEW-ONLY VALIDATION HANDOFF`
- Runtime Boundary: `NO UNITY TEST / NO WEB BUILD / S1 NOT AUTHORIZED`

> **REVIEW ONLY — NOT A UNITY SCENE — S1 NOT AUTHORIZED**

이 Matrix는 잠긴 48 m × 34 m figure-eight 설계의 미래 Graybox 검증 항목을
정의한다. 이번 work package에서는 문서 정합성만 검사한다. `Future S1` 행은
테스트를 실행했다는 뜻이 아니다.

## 1. Route Graph Contract

| Route ID | Ordered nodes | Locked purpose | Required alternate |
|---|---|---|---|
| `ROUTE-CENTRAL` | `RT-ENTRY-STAGE → RT-SOUTH-CLASH-BOWL → RT-NORTH-CROSS → RT-DRAWER-GATE` | fast/risky direct pressure | West와 East가 모두 열려 있어야 함 |
| `ROUTE-WEST` | `RT-SOUTH-CLASH-BOWL → RT-WEST-ENTRY → RT-WEST-TURN → RT-NORTH-CROSS` | Ally collaboration/supply | Central 또는 East로 재합류 가능 |
| `ROUTE-EAST` | `RT-SOUTH-CLASH-BOWL → RT-EAST-ENTRY → RT-EAST-ARC → RT-NORTH-CROSS` | Chair/Strap/physics | Central 또는 West로 재합류 가능 |
| `ROUTE-EXIT` | `RT-DRAWER-GATE → RT-PARKING-BAY → RT-EXIT` | objective-resolved short run | Drawer 해결 전 정상 통과 불가 |

`RT-NORTH-CROSS`의 exact coordinate는
`PROPOSED / REVIEW REQUIRED / NOT RUNTIME AUTHORITY`다. Node의 기능과
figure-eight reconnection은 잠긴 설계다.

## 2. Static Handoff Validation

| Test ID | Requirement | Method in this package | Pass condition | Result |
|---|---|---|---|---|
| `RTV-001` | Room 48×34 m | CSV room row와 SVG room boundary 비교 | Width `48`, Depth `34`, X `-24~+24`, Z `-17~+17` | `PASS` |
| `RTV-002` | Z0–Z7 exactly eight | 문서·CSV·SVG Zone ID 집계 | `ZONE-Z0`~`ZONE-Z7`, 누락·Z8 없음 | `PASS / 8 OF 8` |
| `RTV-003` | Figure-eight preserved | route node/edge 목록과 SVG polyline 비교 | South와 North에서 West/East 양쪽 재연결 | `PASS / 4 ROUTES` |
| `RTV-004` | Actor count | CSV actor reset marker 집계 | Player 1, Ally 1, Runner 2 | `PASS / 1-1-2` |
| `RTV-005` | Object count | CSV Gameplay Object 집계 | Binder 5, Chair 3, Drawer 1, Copier 1 | `PASS / 5-3-1-1` |
| `RTV-006` | Q priority exact | Layout handoff 텍스트 검사 | BREAK GRAB > TEAM FINISH > ASSIST ATTACH > TAG > SUPPLY > REGROUP | `PASS` |
| `RTV-007` | Drawer four solutions | Layout handoff와 본 Matrix 비교 | Direct Chair, Strap Redirect, Runner Causal, Team Finisher만 존재 | `PASS / 4 OF 4` |
| `RTV-008` | FIN-P-001 identity | 모든 산출물 문자열 검사 | `FIN-P-001 — HANDOVER CRASH` 일치 | `PASS` |
| `RTV-009` | Proposal marking | CSV exact source 외 수치의 Authority Status 검사 | 모든 신규 exact 값에 required phrase 존재 | `PASS / 41 REVIEW ROWS` |
| `RTV-010` | Conflict record | Layout/Merge handoff 검사 | `MAP-CONFLICT-001~004`와 최소 보정이 공개됨 | `PASS / 4 OF 4` |
| `RTV-011` | SVG parse | XML parser | well-formed SVG XML | `PASS` |
| `RTV-012` | CSV parse | RFC-style CSV parser | 15개 required column과 모든 row 동일 field count | `PASS / 65 DATA ROWS` |
| `RTV-013` | Shared ID/coordinate | CSV와 SVG `data-element-id/x/z` 비교 | shared element의 X/Z 오차 `0` | `PASS / 60 SHARED IDS` |
| `RTV-014` | Scope protection | Git changed-path 검사 | exact allowlist 외 변경 `0`; `Game/**` 변경 `0` | `PASS / 6 ALLOWLIST FILES ONLY` |

## 3. Future S1 Traversal Matrix

모든 수동 경로 검증은 primitive Graybox와 별도 S1 Relay가 존재할 때만
실행한다.

| Test ID | Start → End | Required observations | Failure condition | Authority |
|---|---|---|---|---|
| `S1-ROUTE-001` | `ACT-PLAYER-01 → RT-SOUTH-CLASH-BOWL` | B-D1/C-D1 접근, first throw lane, Runner 1 telegraph | direct object가 occluded되거나 solid wall을 통과해야 진입 | Locked intent; execution unauthorized |
| `S1-ROUTE-002` | South Bowl → West → North Cross | lane `≥4.5 m`, Supply Pocket `6×6 m`, Ally Follow anchors 연결 | dead end, Ally teleport, object가 lane 전체 봉쇄 | Source dimensions; execution unauthorized |
| `S1-ROUTE-003` | South Bowl → East → North Cross | lane `6~8 m`, Column 양측 선택, Assist/Redirect pockets | Column 한쪽만 통과 가능하거나 Ally가 redirect lane에 남음 | Source dimensions; execution unauthorized |
| `S1-ROUTE-004` | South Bowl → Central → North Cross | fast/risky path와 partial north sightline | Cubicle Island가 완전 벽이 되거나 central route가 safest route가 됨 | Locked topology; tuning review required |
| `S1-ROUTE-005` | North Cross → Drawer Gate | Drawer, Runner 2, Parking Bay, Exit direction 동시 이해 | objective가 보이지 않거나 Runner가 네 접근을 모두 영구 봉쇄 | Locked objective; execution unauthorized |
| `S1-ROUTE-006` | Drawer unresolved → Exit | initial connector `≤0.8 m` | Player/Ally가 정상 이동으로 Exit 진입 | Source clearance; execution unauthorized |
| `S1-ROUTE-007` | Drawer resolved → Exit | open connector `≥3.0 m`, run width `3.5~4 m` | carried Chair/camera가 통과 불가하거나 extra wave 강제 | Source clearance; execution unauthorized |
| `S1-ROUTE-008` | Z3 North Supply Spur round trip | B-A2와 FA-06 접근 후 North Cross 복귀 | Z8 필요, dead end, new Room처럼 분리 | Conflict resolution proposal |
| `S1-ROUTE-009` | Z3 South Recognition Spur | Copier가 보이지만 interact/strap target이 아님 | Copier가 route objective나 usable Heavy로 읽힘 | Conflict resolution proposal |

## 4. Actor Route Validation

| Test ID | Actor | Validation | Pass condition |
|---|---|---|---|
| `S1-ACTOR-001` | Player | 모든 three approach와 Z7 traversal | threshold·step·small clutter 없이 연속 flat-floor 이동 |
| `S1-ACTOR-002` | Ally | `FA-00~FA-11` 순차/대체 연결 | desk/partition 관통·순간이동·Player 앞 봉쇄 없음 |
| `S1-ACTOR-003` | Ally | blocked side switching/regroup | 반대 후방 또는 valid Follow Anchor를 사용하고 camera/throw lane 회피 |
| `S1-ACTOR-004` | Runner 1 | Z2 pressure from `ACT-RUNNER-01` | reset point는 transition, pressure는 Z2에서 readable; 좌우 모두 봉쇄하지 않음 |
| `S1-ACTOR-005` | Runner 2 | Z6 Gatekeeper | Drawer pressure 제공, Exit/solution 영구 봉쇄 없음 |
| `S1-ACTOR-006` | Runners | one Attack Token | 동시에 한 Runner만 attack commit |

## 5. Q Visibility / Accessibility Matrix

| Test ID | Priority state | Expected single prompt | Spatial prerequisites | Rejection check |
|---|---|---|---|---|
| `S1-Q-001` | Player Grabbed + any lower context | `BREAK GRAB` | Ally `≤7 m`, clear route, not Supply/Finisher | lower Q prompt hidden |
| `S1-Q-002` | valid Finisher + lower contexts | `TEAM FINISH` | TEAM READY, Runner 2 in Pocket and staggered, valid Chair, closed Drawer, Ally `≤7 m` | no prompt outside all conditions |
| `S1-Q-003` | valid Chair Strap Assist + lower contexts | `ASSIST ATTACH` | Player/Ally/Chair contact space and Ally clear exit | no prompt on caster/wheel/plastic/Enemy/Copier |
| `S1-Q-004` | staggered Runner | `TAG SHOVE` or `SET HIM UP` | collision lane visible, Ally path clear | no prompt for normal Runner |
| `S1-Q-005` | eligible supply candidate | `SEND BINDER` or `SEND CHAIR` | physical candidate, path and supply lane clear | no hidden inventory/replenishment |
| `S1-Q-006` | no higher context | `REGROUP` | valid Follow Anchor behind/beside Player | no camera/throw-lane regroup |

## 6. Drawer Solution Route Matrix

| Test ID | Solution | Physical approach | Must preserve | Spatial pass |
|---|---|---|---|---|
| `S1-DRAWER-001` | Direct Chair Throw | Z1/Z5 Chair → readable lane → Drawer | no Strap/Finisher requirement | Chair approach exists with Drawer face visible |
| `S1-DRAWER-002` | Cargo Strap Redirect | Z5 Assist → Redirect Arc → Z6 | grounded Chair, Player final control, Ally clears | continuous floor-supported lane without pinch point |
| `S1-DRAWER-003` | Runner Causal Collision | Tag/impact setup → Runner → Drawer | identical Runner archetype and nonlethal response | causal line does not require new breakable/object |
| `S1-DRAWER-004` | Team Finisher | Finisher Pocket → Chair → Runner 2 → Drawer | optional, approximately 2.5 s, control return | `8×7 m` pocket and clear Ally exit |
| `S1-DRAWER-005` | Non-finisher parity | Direct Chair and Strap routes | Finisher not mandatory | both alternatives remain viable when TEAM READY false |

## 7. Camera / Collision Route Matrix

| Test ID | Location | Required geometry | Failure condition |
|---|---|---|---|
| `S1-CAM-001` | Z0 first frame | Player/Ally/direct objects/Cubicle/Runner/EXIT sign readability | one high occluder hides two or more primary reads |
| `S1-CAM-002` | Z2 | open center and solid boundary continuity | carried Binder/Chair or camera crosses solid wall |
| `S1-CAM-003` | Z3 | `≥4.5 m` lane and `6×6 m` pocket | camera pinches against Supply Desk during Ally pass |
| `S1-CAM-004` | Z4 | partitions `1.15~1.25 m` | full player/target occlusion across entire ring |
| `S1-CAM-005` | Z5 Column | `≥1.5 m` around `0.7×0.7 m` Column | camera or grounded Chair cannot traverse intended side |
| `S1-CAM-006` | Z6 Tag | `≥6×6 m` | Tag actor pair leaves frame at ordinary gameplay distance |
| `S1-CAM-007` | Z6 Finisher | `8×7 m` | Chair/Runner/Drawer causality cannot coexist in frame |
| `S1-CAM-008` | Z7 | `3.5~4 m` run | Ally or carried object repeatedly clips/pushes Player |

## 8. Reset Matrix

| Test ID | Mutation before Reset | Expected authored restoration |
|---|---|---|
| `S1-RESET-001` | all actors displaced | four actor reset markers and inactive action state |
| `S1-RESET-002` | all eight movable objects displaced | Binder 5, Chair 3, Drawer 1 at source markers; Copier recognition marker unchanged/restored |
| `S1-RESET-003` | Drawer open and Exit active | Drawer blocked, connector closed, Exit inactive |
| `S1-RESET-004` | PASS/BRACE/BREAK progress | all badges clear and TEAM READY false |
| `S1-RESET-005` | Supply/Carry/Strap/Finisher active | ownership/reservation/strap/finisher cleared through one reset barrier |
| `S1-RESET-006` | missed objects across routes | no passive deletion or teleport before explicit Reset |

## 9. Target Flow Review

| Time window | Expected milestone | Map signal |
|---|---|---|
| `0:00~0:25` | Pickup | Z0/Z1 direct object readability |
| `0:25~1:10` | first collision | Z2 open bowl |
| `1:10~2:00` | Ally pass | Z3/Z2 supply line |
| `2:00~3:10` | route choice | West/Central/East visibility |
| `3:10~4:20` | Rescue/Tag/Attach | distributed badge opportunities |
| `4:20~5:30` | Drawer | four routes converge at Z6 |
| `5:30~6:20` | optional Finisher | HANDOVER CRASH or alternate clear |
| `6:20~7:00` | Exit/tease | short Z7 run; no continuation |

이 timing은 Map v001 target flow다. 이번 문서는 성공을 주장하지 않으며 실제
blind-play timing은 별도 authorized playtest에서만 판정한다.
