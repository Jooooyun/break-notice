# Map Merge Handoff v001

- Work Package: `SHOWCASE-MAP-HANDOFF-001`
- Execution Relay: `EAR-CONTEST-0002-MAP-HANDOFF-001`
- Branch: `codex/showcase-map-design`
- Immutable Base: `f51315aea91dd00293a4f5c387f1cb748865fd11`
- Target Lineage: `CONTEST-0002 / PRT-RM-002`
- Status: `REVIEW PACKAGE / NO MERGE OR PR AUTHORIZED`

> **REVIEW ONLY — NOT A UNITY SCENE — S1 NOT AUTHORIZED**

## 1. Handoff Summary

잠긴 Map v001의 48 m × 34 m Operations Floor를 다음 구현 준비 자료로
번역했다.

- Z0–Z7 Zone table과 coordinate ledger.
- figure-eight route graph와 세 approach.
- Player/Ally/Runner movement responsibility.
- Binder 5, Chair 3, Drawer 1, Copier 1 placement.
- single-Q priority와 accessibility handoff.
- four Drawer solution routes.
- PASS/BRACE/BREAK, TEAM READY와 `FIN-P-001 — HANDOVER CRASH` staging.
- camera/collision/sightline/occlusion handoff.
- spawn/reset baseline, construction order, route validation matrix.
- 5–7 minute target flow.

이 package는 새 맵 설계를 독자적으로 LOCK하지 않는다. 원문에 없는 exact
수치는 모두 `PROPOSED / REVIEW REQUIRED / NOT RUNTIME AUTHORITY`다.

## 2. Exact File Manifest

| File | Purpose | Runtime effect |
|---|---|---|
| `docs/10_contest/workstreams/map/OPERATIONS_FLOOR_LAYOUT_HANDOFF_v001.md` | authoritative-to-proposed layout translation | none |
| `docs/10_contest/workstreams/map/MAP_ZONE_COORDINATES_v001.csv` | coordinate, ID, clearance and authority ledger | none |
| `docs/10_contest/workstreams/map/MAP_ROUTE_TEST_MATRIX_v001.md` | static and future-S1 route validation contract | none |
| `docs/10_contest/workstreams/map/MAP_COLLISION_AND_CAMERA_CLEARANCE_v001.md` | collision, camera, sightline handoff | none |
| `docs/10_contest/workstreams/map/MAP_MERGE_HANDOFF_v001.md` | reviewer merge boundary and evidence | none |
| `ArtSource/Environment/OfficeCoreKit/Preview/ShowcaseMap/OPERATIONS_FLOOR_LAYOUT_v001.svg` | review-only Top-Down preview | none; not Unity-import authority |

Exact allowlist 밖 파일은 이 package에 포함하면 안 된다.

## 3. Authority Sources Reconfirmed

| Source | SHA-256 |
|---|---|
| `AGENTS.md` | `31785dc3104faf201c004e286d023c0c206c66555626f2a2e24c1aaa07027075` |
| initial MASTER EXECUTION PROMPT | `74a5cdecd7bb931b4d582aff1070bc591c9f4fd8674c18c6ede3f1c1cf391f49` |
| `DESIGN_STATUS.md` | `d5f5f203cab1ece5b4409e2cd0ce58ff1287939532cba2d4ae3ebaf7a6f8baed` |
| Map v001 source | `dfe3d2f312bb84dae7df7d596f4a7153f7ec69cfaac3590b93cbc6ab78833834` |
| Animation v003 source | `6c5b6aca6e83686331a1d64d967d0489327aac35d285e0f2aa7da9fb1972bb69` |
| Reconciliation handoff | `a3e3fb0b1690dabd1d61480b5155f22d9720efaa9db0ad11aa52e3a2e57e6d0d` |

## 4. Human Owner Decisions Preserved

- `PRT-RM-002 — Operations Floor Handover Lockdown` only.
- single contiguous `48 m × 34 m`, flat floor, `4.2 m` ceiling.
- figure-eight with Z0–Z7 exactly eight.
- one Player, one mobile Ally, two identical Runners.
- Binder 5, Chair 3, Drawer 1, Copier 1 recognition-only.
- Q priority:
  `BREAK GRAB > TEAM FINISH > ASSIST ATTACH > TAG > SUPPLY > REGROUP`.
- PASS/BRACE/BREAK 중 서로 다른 두 badge → TEAM READY.
- Drawer solutions exactly four:
  Direct Chair Throw, Cargo Strap Redirect, Runner Causal Collision,
  Team Finisher.
- `FIN-P-001 — HANDOVER CRASH` optional and bounded.
- target playtime 5–7 minutes and short post-climax Exit.
- `CONTEST-0001` and `Contest_SingleRoomGraybox.unity` preserved.

## 5. Conflict Resolution Ledger

Human Owner는 최초 지시가 계속 유효하며 충돌 부분을 기록하고 최소 수정해
진행하라고 직접 지시했다.

| ID | Source conflict | Applied handoff resolution | Runtime authority |
|---|---|---|---|
| `MAP-CONFLICT-001` | Runner 1 `(1.0,+1.5)` vs `(1,+2)` | first spatial coordinate retained; later value recorded as rounded role prose | `NO / REVIEW REQUIRED` |
| `MAP-CONFLICT-002` | Runner 1 coordinate outside stated Z2 numerical bounds | no Zone boundary edit; marker tagged `Z2/Z4 transition`, Z2 remains pressure responsibility | `NO / REVIEW REQUIRED` |
| `MAP-CONFLICT-003` | B-A2 outside all numerical Zone rectangles | coordinate retained; semantic `Z3 North Supply Spur`; no Z8 | `NO / REVIEW REQUIRED` |
| `MAP-CONFLICT-004` | Copier outside all numerical Zone rectangles | coordinate retained; semantic `Z3 South Recognition Spur`; recognition-only | `NO / REVIEW REQUIRED` |

원문 authority 파일은 수정하지 않았다. 위 resolution은 CSV, Markdown과 SVG에
일관되게 표시하며 향후 Human review에서 채택·수정할 수 있다.

## 6. ID and Coordinate Ownership

- CSV가 이 review package의 exact ID/X/Z ledger다.
- SVG shared elements는 `data-element-id`, `data-x`, `data-z`를 사용한다.
- Markdown은 책임과 topology를 설명하고 CSV를 재정의하지 않는다.
- source coordinates는 원문 X/Z를 유지한다.
- proposed center, yaw, footprint, movement area, collider/camera envelope는
  Runtime에 복사하기 전 Human review가 필요하다.
- empty Yaw는 authority 없음, numeric Yaw는 제안값이다.

## 7. Reviewer Order

1. 본 파일의 authority/boundary와 Conflict Ledger를 확인한다.
2. Layout Handoff의 Zone/route/object/Q/Drawer sections를 확인한다.
3. CSV에서 Source와 Proposed status를 분리 확인한다.
4. SVG에서 figure-eight, source object positions, proposed route nodes를 확인한다.
5. Collision/Camera 문서에서 locked clearance와 proposed envelope를 구분한다.
6. Route Matrix의 static validation evidence를 확인한다.
7. 수용할 proposed coordinate를 Human Owner가 별도로 결정한다.
8. S1 Scene authoring이 필요하면 새 immutable baseline과 one-shot Relay를 발급한다.

## 8. Validation Evidence

| Check | Expected | Result |
|---|---|---|
| exact allowlist only | 6 files | `PASS / 6 OF 6` |
| `Game/**` changed | 0 | `PASS / 0` |
| shared authority changed | 0 | `PASS / 0` |
| Room dimensions | 48×34 | `PASS` |
| Zone set | Z0–Z7 exactly 8 | `PASS / 8 OF 8` |
| actor/object count | 1/1/2 and 5/3/1/1 | `PASS` |
| Q priority | exact | `PASS` |
| Drawer solutions | exact four | `PASS / 4 OF 4` |
| FIN-P-001 | exact | `PASS` |
| proposed-value marking | all new exact values | `PASS / 41 REVIEW ROWS` |
| CSV parse | PASS | `PASS / 15 COLUMNS / 65 DATA ROWS` |
| SVG XML parse | PASS | `PASS / 60 SHARED IDS` |
| shared CSV/SVG ID-coordinate match | PASS | `PASS / 60 IDS / 59 GEOMETRY CENTERS` |
| `git diff --check` | PASS | `PASS / STAGED DIFF` |
| Unity/Test/Build/Browser | not run | `NOT AUTHORIZED / NOT RUN` |

## 9. Strict Non-Authorization

이 package는 다음을 승인하거나 수행하지 않는다.

- `Game/**`, Unity Scene, existing Graybox, Prefab, Material, Runtime Art.
- Runtime code, Input Actions, Package, ProjectSettings, Test/Build scripts.
- Unity launch, Web build, HTTP, Chrome/Edge/in-app browser test.
- S1, historical C5, S9 Runtime Art integration.
- new Room, Zone, Gameplay Object, Enemy archetype, boss or Drawer solution.
- PR, merge, cherry-pick, rebase, amend or force push.

## 10. Future S1 Intake Checklist

S1 담당자는 새 Relay를 받기 전 아무 Scene도 만들지 않는다. 새 Relay에는
최소한 다음이 필요하다.

- exact post-merge immutable baseline and branch.
- clean local/upstream/origin equality.
- exact Scene/Prefab/code/test allowlist and forbidden paths.
- accepted disposition for `MAP-CONFLICT-001~004` proposals.
- coordinate ledger version to implement.
- primitive-only construction and no Production Art boundary.
- collision/camera/route validation and Reset obligations.
- commit/push authority and one-shot expiration.

## 11. Asset Readiness Boundary

No production asset is required to review or later primitive-graybox this map.
Office Core Kit and gameplay prop candidates remain separate technical-review
inputs. Runtime integration remains S9-only.

`No new production asset blocks this work package. Graybox fallback remains active.`
