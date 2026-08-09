# CONTEST-0001 — Single-Room Browser-Playable Prototype

- Status: `OWNER-APPROVED / C0-C3 IMPLEMENTATION COMPLETE / C4 NOT AUTHORIZED`
- Change Control: `CR-0003`
- ADR: `ADR-0023`
- Relay: `EAR-CONTEST-0001-001 CONSUMED BY C0 PASS / LATER HUMAN OWNER CONTEST FAST-TRACK CORE LOOP CONSUMED BY C1-C3 / C4 EXCLUDED`
- Baseline: `28b7f70c4b6299159afe5b99ea7b98d1e5399352`
- Branch: `contest/game-builders-2026`
- Current Gate:
  `MAIN PRODUCT CLOSED / CONTEST C0-C3 IMPLEMENTATION COMPLETE / C4 CLOSED`

## Goal

기존 G4-A 이동·카메라 Graybox를 보존하면서, 단일 authored room에서 두
physical solution으로 Drawer objective를 해결하고 EXIT에 도달하는 안정적인
desktop browser prototype과 검증 증거를 만든다.

## Work Packages

| ID | Outcome |
|---|---|
| C0 | Contest branch/tags, authority, art pipeline, separate scene, Web build qualification |
| C1 | Interaction arbitration and singular ownership |
| C2 | Binder/Chair acquisition, carry and physical throw |
| C3 | Authored reset and baseline diff |
| C4 | Fixed ally physical relay, catch and queued throw |
| C5 | Bounded Cargo Strap chair redirect |
| C6 | Drawer objective, route, EXIT and win/retry |
| C7 | Two deterministic nonlethal Compliance Runners and restart |
| C8 | Approved production art, UI, VFX and licensed audio integration |
| C9 | Web release, playtest and submission evidence |

각 work package는 이전 acceptance를 유지하고 관련 test와 browser smoke를
추가한 뒤 one-purpose commit으로 종료한다.

## C0 Allowed Files

- `AGENTS.md`, `MANIFEST.json`, `CHANGELOG.md`
- `docs/00_project/DESIGN_STATUS.md`, `DECISION_LOG.md`, `CR-0003-*`
- `docs/09_adrs/ADR-0023-*`
- 이 ticket과 `docs/10_contest/**`
- `ArtSource/**`
- 승인된 runtime art/audio folder scaffolding
- `Game/Assets/_Project/Code/Editor/Contest/**`
- `Game/Assets/_Project/Code/Runtime/Contest/**` for the C0-only browser
  pointer-capture bridge; no gameplay mutation is allowed
- `Game/Assets/_Project/Scenes/10_Prototype/Contest_SingleRoomGraybox.unity*`
- `Game/ProjectSettings/EditorBuildSettings.asset`
- Web build에 필수인 최소 `Game/ProjectSettings/ProjectSettings.asset` 변경
- `tools/contest/**`
- ignored local test/build/log output

## C0 Forbidden Files and Changes

- `.claude/settings.local.json`
- 두 untracked `docs/05_production/*` advisory 문서
- 23개 untracked `references/exploration/*_REVIEW.png`
- 기존 `references/approved/**` 이미지와 승인 Visual Spec/ADR
- `G4A_LocomotionCameraGraybox.unity`와 기존 prefab/runtime/test/input code
- `Game/Packages/**`
- gameplay interaction, carry, throw, ally, Strap, objective와 enemy logic
- production asset, package/service 추가, public deployment
- `main` 변경, merge/rebase/force-push, 기존 tag 이동

## C0 Operations

1. local/remote baseline과 protected untracked work를 검증한다.
2. 두 contest baseline tag와 contest branch를 생성한다.
3. 승인 Visual Reference 23개를 provenance/evidence 기반으로 분류한다.
4. contest authority·logs·art delivery 구조를 materialize한다.
5. exact Unity Web Build Support만 검증·설치한다.
6. 기존 G4-A scene의 byte content를 보존하고 별도 Contest Scene을 만든다.
7. deterministic EditMode, PlayMode, Web development build, HTTP server와
   release verification automation을 만든다.
8. Chrome와 Edge anonymous browser에서 실행을 확인한다.
9. diff와 protected path를 검사하고 one-purpose commit/push한다.

## C0 Acceptance

- Contest Scene이 Build Settings에 enabled로 등록된다.
- exact Unity `6000.3.20f1 / c9ba695d4f07` Web build가 성공한다.
- 기존 Core `17`, Locomotion/Camera `10`, 합계 `27` test가 유지된다.
- HTTP로 로드한 anonymous Chrome와 Edge에서 Move, Look, Aim이 작동한다.
- pointer capture, Escape release와 resize가 검증된다.
- blocking JavaScript/Unity console error가 `0`이다.
- package manifest/lock drift와 기존 G4-A scene byte drift가 `0`이다.
- contest docs, Visual Inventory와 Asset Needs report가 존재한다.
- C0 commit과 일반 push가 완료된다.

## EAR-CONTEST-0001-001

- Status: `CONSUMED BY HUMAN OWNER C0 PASS / EXPIRED 2026-08-08`
- Exact Ticket: `CONTEST-0001`
- Exact Baseline: `28b7f70c4b6299159afe5b99ea7b98d1e5399352`
- Exact Branch: `contest/game-builders-2026`
- Consumed Gate:
  `OPEN — CONTEST-0001 ONLY / GAME BUILDERS 2026 SINGLE-ROOM BROWSER PROTOTYPE / C0 EXECUTION ONLY`
- Pre-state: tracked/staged clean; Human Owner가 식별한 protected untracked
  work는 존재 가능하며 명시된 원본을 수정·stage하지 않는다.
- Commit Permission: work package별 one-purpose commit
- Push Permission: contest branch와 승인된 contest/art branch의 일반 push
- Forbidden Git: amend of pushed commit, rebase/force-push, automatic main merge
- Expiration: C0 completion, final submission, 2026-08-26 end, Owner revocation,
  identity loss, unauthorized dependency/scope,
  data/license/contributor/security/privacy risk

일반 compile/test/implementation defect는 expiration이 아니며 승인 범위 안에서
수정하고 재검증한다.

## Final C0 Evidence and Acceptance — 2026-08-08

- Aim-smoothed source:
  `d9d74b3d4dfabc9168c8360033af1756a42b705e`
- Unity Web Development build:
  `PASS / build GUID b03f2792cdce44a2ac08ad1049ceaca1 / 0 warnings / 0 errors`
- Existing and added tests:
  `EditMode 23/23 + PlayMode 5/5 = 28/28 PASS`
- Local HTTP verification: `19/19 files HTTP 200`
- Build aggregate SHA-256:
  `6a411450d690ffd4e55949e8ce1b54b9c838a22edd902b850ab87b3b14adc2eb`
- Automated browser qualification:
  `Chrome and Edge WebGL load PASS / 1366×768 and 1920×1080 PASS / clean-tab console error-warning 0 / Edge Escape PASS`
- Human Owner acceptance:
  `PASS / 2026-08-08 / look sensitivity 0.25, pitch -12 to 35, move-look, rapid RMB Aim comfort and pointer capture-release`
- Pointer automation note: the earlier extension-driven `WrongDocumentError`
  was rejected as automation acceptance evidence and is not a product error;
  trusted Human Owner input supplied the required final evidence.
- Remote publication: `COMPLETE / VERIFIED` — the contest branch is current
  through the containing C0-closure evidence commit; art branch heads are
  `b6903e243ff2e24094207541daf93248223caea1` and
  `6b994b4065ca003a28562ab30e5ef6d0007154b7`; baseline tags remain at their
  immutable approved targets; `origin/main` is unchanged.
- Consequence:
  `C0 COMPLETE / EAR-CONTEST-0001-001 CONSUMED AND EXPIRED / C1 NOT STARTED / NEW BOUNDED RELAY REQUIRED`

## C1-C3 Fast-Track Evidence — 2026-08-08

- Human Owner authorization:
  `DIRECT CONTEST FAST-TRACK CORE LOOP / C1-C3 ONLY / ONE CONTINUOUS BOUNDED RUN / C4 AND LATER EXCLUDED`
- Starting baseline:
  `54976681bf3970fcad4e141ab4f3f6dfd1820fa1 / contest/game-builders-2026 / local and origin equal / tracked and staged clean`
- Protected work:
  `26 OF 26 HUMAN OWNER UNTRACKED ITEMS PRESERVED / 0 OPENED FOR TASK WORK / 0 STAGED`
- C1:
  `COMPLETE / deterministic InteractPrimary arbitration, safe rejection, target readability, Light-Medium-Heavy eligibility and singular Carry-Strap-Relay ownership / commit 278d062a6b486a176a7cb9144b75dfb058567ead`
- C2:
  `COMPLETE / visible direct Binder-Chair acquisition, carry and physical throw with player-owned origin identity / commit 083b44e86c76977bd855bf0c2c09baf0001b9750`
- C3:
  `IMPLEMENTATION COMPLETE IN THE CONTAINING COMMIT / authored player-actor-object snapshots, exclusive Reset barrier, carry-throw ownership cleanup, Rigidbody restoration and baseline audit`
- Tests:
  `Unity compile PASS / EditMode 37/37 / PlayMode 15/15 / total 52/52 / existing 28 preserved`
- Web Development build:
  `PASS / GUID a27595036f0046f083dfae44dada1463 / 113733340 bytes / warnings 0 / errors 0 / 19 files / aggregate SHA-256 11bb3eafd9d4ec175ad12437be64428edec0ba18f634e093442f094007aa5d04`
- HTTP and Chrome:
  `localhost HTTP 200 / Chrome clean WebGL load PASS / initial product console error-warning 0`
- Browser input evidence:
  `C0 Human Owner move-look-rapid-RMB-pointer acceptance remains authoritative and the baseline behavior was not modified. Chrome extension-driven pointer-lock input again emitted the already-rejected WrongDocumentError automation diagnostic, so trusted C1-C3 acquire-carry-throw-reset browser manual smoke was not collected in this run.`
- Scope:
  `C4, Cargo Strap, ally-enemy behavior, objective-win, production art integration, new package, second room and all later scope NOT STARTED`

## C1-C3 Carry-Wall Camera Collision Repair Evidence — 2026-08-09

- Human Owner finding:
  `REVISION REQUIRED / camera viewpoint error reproduced while carrying an object near a wall`
- Starting baseline:
  `3803dfcf05ffcfc942d5c1121d38966339227970 / contest/game-builders-2026 / local and origin equal / tracked and staged clean`
- Root cause:
  `Cinemachine obstacle filtering included every layer while the carried Binder or Chair collider remained camera-query-visible, allowing the held object and wall to compete as the current camera obstacle.`
- Repair:
  `Camera collision now uses Unity DefaultRaycastLayers. Carry temporarily assigns the exact built-in Ignore Raycast layer and Throw or authored Reset restores the exact original layer. Physical colliders remain enabled.`
- Tests:
  `Unity compile PASS / EditMode 37/37 / PlayMode 17/17 / total 54/54 / carry layer apply, Throw restore, Reset restore and camera-wall obstacle regression covered`
- Web Development build:
  `PASS / GUID 9d1138587e4543f78ebdd73c52eb483f / 113735698 Unity-reported bytes / warnings 0 / errors 0 / 19 files / 113737452 HTTP-verified bytes / aggregate SHA-256 ce052048e05ef56c875d1d8e1106d1ffe17d170ca6d2b8cab1bac2b494cb492f`
- HTTP and browser:
  `19/19 HTTP 200 / in-app browser clean WebGL load PASS / 960x600 canvas ready / initial product console error-warning 0`
- Manual acceptance:
  `PASS / Human Owner confirmed the post-repair camera is normal on 2026-08-09 for the Binder-Chair carry-near-wall defect scope. Broader C1-C3 manual acceptance is not inferred from this bounded result.`
- Scope:
  `C4 and later, Scene, Prefab, Input Actions, Packages, ProjectSettings, production art and protected Human Owner untracked work unchanged.`

## C2 Carry-Solid-Wall Physical Continuity Repair Evidence — 2026-08-09

- Human Owner finding:
  `REVISION REQUIRED / a carried Binder or Chair passed through a solid wall while the player continued toward it`
- Camera disposition:
  `The prior camera-specific repair and Human Owner PASS remain preserved. This is a separate C2 carried-object physical-continuity defect.`
- Starting baseline:
  `7fc9f41a0ed5c81a44f172009ed0c79764c8ca18 / contest/game-builders-2026 / local and origin equal / tracked and staged clean`
- Repair commit:
  `538f2cc781833555b2bf1839c5c37a815f75b6e3 / pushed and remote-verified`
- Root cause:
  `Carry made the Rigidbody kinematic and moved it directly toward the anchor without a solid-geometry sweep or penetration clamp. The CharacterController therefore stopped at the wall while the carried collider could continue through it.`
- Repair:
  `Root BoxCollider candidates now use bounded non-alloc sweep, wall clamp, wall-plane slide, rotation substeps and predicted-pose penetration rejection. Player collision is restored after acquisition separation, and Throw or authored Reset preserves the existing owner, layer and Rigidbody cleanup contracts. Unsupported root collider shapes are rejected safely for this exact Graybox scope.`
- Tests:
  `Unity compile PASS / targeted carry-wall 7/7 / EditMode 37/37 / PlayMode 19/19 / total 56/56 / Binder and Chair clamp-slide-rotation-player separation-Throw coverage plus blocked-carry Reset restoration`
- Web Development build:
  `PASS / GUID 13076f9f2456415fa1adcf1446dbf054 / 113763737 Unity-reported bytes / warnings 0 / errors 0 / 19 files / 113765491 HTTP-verified bytes / aggregate SHA-256 ee34aac84517eba2a668f85727042b4ca0b8ed8d51580db3a0da9757c7af46ba`
- HTTP and browser:
  `19/19 HTTP 200 / in-app browser cold WebGL load PASS / 960x600 canvas ready / product console errors 0`
- Manual acceptance:
  `PENDING / Human Owner must retest Binder and Chair against a solid wall, including approach, yaw, wall slide, Throw and Reset. Broader C1-C3 manual acceptance is not inferred.`
- Scope:
  `C4 and later, Scene, Prefab, Input Actions, Packages, ProjectSettings, production art and protected Human Owner untracked work unchanged.`

## C0-C3 Pointer Capture and Acquisition Readability Repair Evidence — 2026-08-09

- Human Owner findings:
  `REVISION REQUIRED / WebGL mouse capture did not reliably reflect the browser's real pointer-lock state, and direct pickup selection was not clear enough to predict. Structural fun is not claimed because C4-C7 remain unimplemented.`
- Starting baseline:
  `607282c84a12eff860c90d1f2cd81c6829a56825 / contest/game-builders-2026 / local and origin equal / tracked and staged clean`
- Repair commit:
  `b2cca79de731be91392a0c288a22d1a8f25c9077 / pushed and remote-verified`
- Pointer repair:
  `A WebGL JavaScript bridge requests lock inside the trusted canvas pointerdown event and reports document.pointerLockElement as the sole browser authority. Escape, focus loss, unsupported and request-error states are visible; unlocked Look-Aim and the first capture click's Throw-Aim side effects are blocked.`
- Acquisition-readability repair:
  `Targeting now uses the exact current camera-center ray, nearest solid visibility hit and Collider.ClosestPoint range <= 2.8 m. The reticle and prompt distinguish direct Binder-Chair pickup from Heavy, Drawer and ally-pool rejection, and accepted or rejected E attempts produce bounded presentation-only feedback.`
- Priority and ordering:
  `ThirdPersonCameraRig -100, CinemachineBrain 0, PrototypeInteractionController 100 and InteractionReadabilityPresenter 200 preserve current-frame camera alignment. ResetEncounter blocks the whole logical frame, including simultaneous Reset plus InteractPrimary.`
- Tests:
  `Unity compile PASS / EditMode 41/41 / PlayMode 25/25 / total 66/66 / pointer status contract, exact center-ray selection, occlusion, closest-point range, deterministic tie-break, feedback and same-frame Reset priority covered`
- Web Development build:
  `PASS / GUID 17afceeeb36e4733a1034a54a585ef5e / 113790231 Unity-reported bytes / warnings 0 / errors 0 / 19 files / 113791985 HTTP-verified bytes / aggregate SHA-256 c8d90f0cab2af3fec957f8fb0bf21040f223bd33f5cf9a9f577f4851eee78e45`
- HTTP and browser:
  `19/19 HTTP 200 / in-app browser cold WebGL load PASS / 960x600 canvas ready / pointer-capture and center-reticle instructions visible / blocking console warning-error 0 / dialog 0`
- Manual acceptance:
  `PENDING / Human Owner trusted click capture, first-click safety, Escape release, recapture and E pickup-clarity retest are required. Automated browser input is not accepted as trusted pointer-lock evidence.`
- Fun disposition:
  `NOT CLAIMED / this repair improves C0-C3 usability and feedback only. Ally relay, Cargo Strap, objective-route-win and Runner failure-pressure remain C4-C7 work and are not implemented or authorized here.`
- Scope:
  `C4 and later, Packages, ProjectSettings, production art and protected Human Owner untracked work unchanged.`
