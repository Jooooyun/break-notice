# CONTEST-0001 — Single-Room Browser-Playable Prototype

- Status: `OWNER-APPROVED / EXECUTION ACTIVE / C0 ENVIRONMENT BLOCKED`
- Change Control: `CR-0003`
- ADR: `ADR-0023`
- Relay: `EAR-CONTEST-0001-001 / ACTIVE`
- Baseline: `28b7f70c4b6299159afe5b99ea7b98d1e5399352`
- Branch: `contest/game-builders-2026`
- Gate:
  `OPEN — CONTEST-0001 ONLY / GAME BUILDERS 2026 SINGLE-ROOM BROWSER PROTOTYPE`

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

- Status: `ACTIVE`
- Exact Ticket: `CONTEST-0001`
- Exact Baseline: `28b7f70c4b6299159afe5b99ea7b98d1e5399352`
- Exact Branch: `contest/game-builders-2026`
- Gate: 위 contest-only open state
- Pre-state: tracked/staged clean; Human Owner가 식별한 protected untracked
  work는 존재 가능하며 명시된 원본을 수정·stage하지 않는다.
- Commit Permission: work package별 one-purpose commit
- Push Permission: contest branch와 승인된 contest/art branch의 일반 push
- Forbidden Git: amend of pushed commit, rebase/force-push, automatic main merge
- Expiration: final submission, 2026-08-26 end, Owner revocation, identity loss,
  unauthorized dependency/scope, data/license/contributor/security/privacy risk

일반 compile/test/implementation defect는 expiration이 아니며 승인 범위 안에서
수정하고 재검증한다.

## Current C0 Evidence and Blocker — 2026-08-06

- Unity Web Development build: `PASS / 0 warnings / 0 errors`
- Existing tests: `EditMode 23/23 + PlayMode 4/4 = 27/27 PASS`
- Local HTTP verification: `19/19 files HTTP 200`
- Build aggregate SHA-256:
  `a24d1dd102377ba01cca858a7134439d2d2f54f1fa8994b609f9abb1f88724dd`
- Chrome/Edge qualification: `NOT EXECUTED`
- Blocker: Chrome와 Edge 실행 파일은 설치됐지만 두 family 모두 ChatGPT
  browser extension이 없고 native-host registry registration도 없다.
- Required recovery: Codex app의 **Settings → Computer use**에서 browser
  extension/plugin을 설치 또는 재설치한 뒤 exact Chrome와 Edge 검증을
  다시 실행한다.
- Remote publication: local branch/tag는 존재하지만 push는 실행 환경의
  별도 approval review에서 거부됐다.
- Consequence: C0 acceptance와 push는 미완료이며 C1은 시작하지 않는다.
