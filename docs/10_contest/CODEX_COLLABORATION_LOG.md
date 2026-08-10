# Codex Collaboration Log

추측한 시간·결과를 기록하지 않는다. 각 work package는 실제 evidence와
commit SHA가 확보된 뒤 한 행을 닫는다.

## C0 — Contest Transition and Browser Qualification

- Date: `2026-08-06`
- Goal: contest branch, tags, authority, visual/art pipeline, separate scene,
  exact Web build pipeline과 Chrome/Edge qualification
- Human Owner Decisions:
  - `CR-0003`, `CONTEST-0001`, `EAR-CONTEST-0001-001` 직접 승인
  - 기존 26개 untracked 항목 식별·보호
  - 승인 reference를 production brief와 review에 사용
  - exact `6000.3.20f1` Web Build Support 설치 허용
- Codex Work:
  - local/remote SHA와 protected work 감사
  - 23개 REVIEW 이미지의 SHA-256·dimension·approval mapping 확인
  - contest authority, docs, art/runtime scaffolding과 local automation 작성
  - separate Contest Scene, tests, Web build와 browser evidence 확보
- Problems Found:
  - exact Editor의 Web Build Support 누락
  - Editor가 Hub-managed installation이 아니어서 Hub CLI module install 거부
  - remote push는 실행 환경의 별도 안전 승인 필요
  - Chrome와 Edge 모두 ChatGPT browser extension이 설치되지 않았고
    shared native-host registry registration도 없음
- Applied Resolution:
  - Unity 공식 6000.3.20f1 component installer의 Authenticode와 SHA-256을
    검증한 후 exact Editor root에 Web Build Support만 설치
  - protected untracked 파일은 명시 경로 stage만으로 제외
  - exact browser family를 다른 브라우저로 대체하지 않고 C1 진입 차단
- Tests: `EditMode 23/23 PASS / PlayMode 4/4 PASS / total 27/27`
- Manual Validation:
  `Web build PASS / warnings 0 / errors 0 / HTTP 19/19 PASS`
- Browser Validation:
  `BLOCKED / exact Chrome and Edge extension connections unavailable / Move,
  Look, Aim, pointer capture, Escape, resize and browser console not executed`
- Commit SHA: `PENDING`
- Actual Result: `C0 ENVIRONMENT BLOCKED / C1 NOT STARTED`
- Actual Work Time: `NOT RECORDED`
- Discarded Approach:
  - Hub CLI `install-modules` — Hub-managed Editor가 아니라 사용 불가
  - untracked REVIEW 원본 재등록 — tracked approved counterpart가 이미 존재
- Next: `install/reinstall browser extension, exact Chrome/Edge qualification,
  explicit remote-push approval, then C0 closure`

## C0 Recovery Attempt and Remote Publication — 2026-08-06

- Human Owner Decisions:
  - Computer Use browser extension installation completed
  - normal remote push explicitly approved
- Browser Diagnostics:
  - Chrome extension `1.2.27236.6274`: `INSTALLED / ENABLED`
  - Chrome native host: `VALID`
  - Chrome runtime: `NOT RUNNING / EXACT FAMILY UNAVAILABLE`
  - Edge extension `1.2.27236.6274`: `INSTALLED / ENABLED / CONNECTED`
  - Edge native host: `VALID`
  - Edge local HTTP navigation: `BLOCKED / ADMIN-ENFORCED POLICY COULD NOT BE VERIFIED / SAME-URL RETRY FAILED`
- Remote Publication:
  - contest and both art branches: `PUSHED / VERIFIED / 73d886eb2b6ccc0061e6f484afa663402418ec59`
  - `gb2026-prechallenge`: `PUSHED / TARGET b82bb382fc0439847d6497ee2fadd9c55781a9fa`
  - `gb2026-sprint-start`: `PUSHED / TARGET 28b7f70c4b6299159afe5b99ea7b98d1e5399352`
  - `origin/main`: `UNCHANGED / 28b7f70c4b6299159afe5b99ea7b98d1e5399352`
- Actual Result: `REMOTE PUSH COMPLETE / C0 BROWSER ACCEPTANCE STILL BLOCKED / C1 NOT STARTED`
- Next: `launch Chrome, restore Edge local-URL policy verification, rerun exact Chrome and Edge qualification`

## C0 Aim Transition Remediation — 2026-08-06

- Goal: Human Owner가 보고한 RMB 연타 Aim 멀미를 C0 범위에서 수정하고
  exact Unity와 Chrome·Edge에서 재검증
- Human Owner Decisions:
  - Look sensitivity `0.25`: `PASS / PRESERVE`
  - Pitch `-12°~35°`: `PASS / PRESERVE`
  - Pointer release: `PASS / PRESERVE`
  - rapid RMB Aim comfort: `FAIL / REVISION REQUIRED`
  - 현재 재미 부족은 C0의 feature·asset 공백과 분리해 판단하고 C1 또는
    room expansion을 무단 시작하지 않음
- Codex Work:
  - 즉시 전환되던 Aim CameraDistance, ShoulderOffset과 FOV를 공통
    `0.2초` normalized blend로 변경
  - 빠른 Aim 토글이 presentation 범위를 벗어나지 않는 PlayMode 회귀
    테스트 추가
  - Unity 자동 재직렬화 Scene·ProjectSettings 노이즈와 생성 설정 파일 제거
- Tests: `EditMode 23/23 PASS / PlayMode 5/5 PASS / total 28/28`
- Build:
  `PASS / GUID b03f2792cdce44a2ac08ad1049ceaca1 / warnings 0 / errors 0`
- HTTP:
  `19/19 PASS / 113599442 bytes / SHA-256 6a411450d690ffd4e55949e8ce1b54b9c838a22edd902b850ab87b3b14adc2eb`
- Browser Validation:
  - Chrome: `WebGL load PASS / 1366x768 PASS / 1920x1080 PASS / clean-tab console 0`
  - Edge: `WebGL load PASS / 1366x768 PASS / 1920x1080 PASS / Escape PASS / console 0`
  - Pointer: `AUTOMATION NOT ACCEPTED / extension-driven request returned WrongDocumentError`
- Commit SHA: `d9d74b3d4dfabc9168c8360033af1756a42b705e`
- Actual Result:
  `C0 QUALIFICATION IN PROGRESS / AIM-SMOOTHED BUILD READY FOR HUMAN OWNER RETEST / C1 NOT STARTED`
- Next:
  `Human Owner rapid-RMB Aim and Pointer capture-release retest in the retained Chrome tab; close C0 only after PASS`

## C0 Human Owner Acceptance and Closure — 2026-08-08

- Goal: aim-smoothed C0 build의 trusted Human 입력 판정을 반영하고 C0를
  정확한 증거와 함께 종료
- Human Owner Decision:
  `PASS / APPROVED ALL REQUESTED CHECKS`
- Preserved Results:
  - Look sensitivity `0.25`: `PASS`
  - Pitch `-12°~35°`: `PASS`
  - Move-look: `PASS`
  - rapid RMB Aim with `0.2초` blend: `PASS`
  - Pointer capture-release: `PASS`
- Codex Work:
  - 검증된 WebGL build를 로컬 HTTP `200`으로 복구
  - Chrome clean handoff tab에서 `960×600` Canvas 렌더와 console
    error-warning `0` 재확인
  - Human Owner 최종 판정을 live state, ticket, build log와 derived
    manifest에 반영
- Implementation Source SHA:
  `d9d74b3d4dfabc9168c8360033af1756a42b705e`
- Tests and Build Evidence:
  `EditMode 23/23 / PlayMode 5/5 / total 28/28 / Web build warnings 0 errors 0 / HTTP 19/19`
- Browser Result:
  `PASS / Chrome-Edge load-resize-console / Edge Escape / Human Chrome Aim-Pointer acceptance`
- Actual Result:
  `C0 COMPLETE / EAR-CONTEST-0001-001 CONSUMED AND EXPIRED / C1 NOT STARTED`
- Next:
  `issue a separate clean pushed HEAD-bound execution relay for C1 Interaction Arbitration and Ownership`

## C1-C3 Contest Fast-Track Core Loop — 2026-08-08

- Goal: single-room Contest prototype의 첫 complete playable vertical-slice
  foundation으로 C1 arbitration, C2 acquisition/carry/throw, C3 authored Reset을
  논리·테스트·커밋 단위로 분리해 연속 구현
- Human Owner Decision:
  `DIRECT FAST-TRACK AUTHORIZATION / BASELINE 54976681bf3970fcad4e141ab4f3f6dfd1820fa1 / C1-C3 ONLY / NORMAL PUSH AUTHORIZED / C4 EXCLUDED`
- C1 Result:
  `PASS / commit 278d062a6b486a176a7cb9144b75dfb058567ead / deterministic arbitration, safe rejection, readability and singular ownership`
- C2 Result:
  `PASS / commit 083b44e86c76977bd855bf0c2c09baf0001b9750 / direct Binder-Chair visible acquisition, carry and physical throw`
- C3 Result:
  `PASS AUTOMATED / containing commit / exclusive Reset barrier, authored snapshot restore, ownership cleanup and baseline audit`
- Tests:
  `Unity compile PASS / EditMode 37/37 / PlayMode 15/15 / total 52/52 / existing 28 preserved`
- Build:
  `PASS / GUID a27595036f0046f083dfae44dada1463 / 113733340 bytes / warnings 0 / errors 0 / SHA-256 11bb3eafd9d4ec175ad12437be64428edec0ba18f634e093442f094007aa5d04`
- Browser:
  `Chrome clean WebGL load PASS / initial product console 0 / C0 Human Owner movement-look-rapid-RMB-pointer evidence preserved`
- Manual Limitation:
  `Trusted C1-C3 acquire-carry-throw-reset browser smoke was not collected because extension control re-entered the already-rejected WrongDocumentError pointer-lock diagnostic; automated PlayMode coverage passed.`
- Protected Scope:
  `26 Human Owner untracked items untouched / protected staged 0 / main, packages, production art and C4+ unchanged`
- Next:
  `Human Owner authorization required before C4 fixed ally relay, catch and queued throw`

## C1-C3 Carry-Wall Camera Collision Repair — 2026-08-09

- Goal:
  `Repair the Human Owner-reported camera viewpoint error when a carried Binder or Chair approaches a wall, without entering C4.`
- Human Owner Decision:
  `Proceed with the next task and treat the reported carry-near-wall camera behavior as a defect.`
- Root Cause:
  `The Cinemachine obstacle query included the carried collider, so a nearby wall and held object could alternate as the active obstacle.`
- Repair:
  `DefaultRaycastLayers for the camera; exact Ignore Raycast layer only during Carry; exact original layer restored by Throw and Reset; physical colliders preserved.`
- Tests:
  `Unity compile PASS / EditMode 37/37 / PlayMode 17/17 / total 54/54.`
- Build:
  `PASS / GUID 9d1138587e4543f78ebdd73c52eb483f / 113735698 Unity-reported bytes / warnings 0 / errors 0 / 19 HTTP files / SHA-256 ce052048e05ef56c875d1d8e1106d1ffe17d170ca6d2b8cab1bac2b494cb492f.`
- Browser:
  `In-app browser clean WebGL load PASS / 960x600 canvas ready / initial product console error-warning 0.`
- Manual Status:
  `Pre-repair Human Owner reproduction was REVISION REQUIRED. Post-repair Human Owner Binder-Chair carry-near-wall camera retest PASS on 2026-08-09. Broader C1-C3 manual acceptance is not inferred.`
- Protected Scope:
  `26 Human Owner untracked items unchanged and unstaged / main, Scene, Prefab, Input Actions, Packages, ProjectSettings, production art and C4+ unchanged.`
- Next:
  `Report the bounded manual PASS to GPT Pro and obtain the next bounded work recommendation. C4 remains unauthorized until a new Human Owner directive opens it.`

## C2 Carry-Solid-Wall Physical Continuity Repair — 2026-08-09

- Goal:
  `Repair the Human Owner-reported physical wall penetration of a carried Binder or Chair without entering C4.`
- Human Owner Decision:
  `Treat the carried-object wall penetration as a core bug and proceed with the bounded repair. The earlier camera-specific PASS remains authoritative.`
- Baseline:
  `7fc9f41a0ed5c81a44f172009ed0c79764c8ca18 / contest branch local-origin equal / tracked and staged clean / protected 26 preserved.`
- Repair:
  `Bounded root-BoxCollider sweep and penetration containment, wall-plane slide, rotation substeps, safe unsupported-shape rejection and player-collision restoration; Throw and Reset cleanup preserved.`
- Commit:
  `538f2cc781833555b2bf1839c5c37a815f75b6e3 / pushed and remote-verified.`
- Tests:
  `Targeted 7/7 / EditMode 37/37 / PlayMode 19/19 / total 56/56 PASS.`
- Build:
  `PASS / GUID 13076f9f2456415fa1adcf1446dbf054 / 113763737 Unity-reported bytes / warnings 0 / errors 0 / 19 HTTP files / SHA-256 ee34aac84517eba2a668f85727042b4ca0b8ed8d51580db3a0da9757c7af46ba.`
- Browser:
  `In-app browser cold WebGL load PASS / 960x600 canvas ready / product console errors 0.`
- Manual Status:
  `AUTOMATED PASS / HUMAN OWNER BINDER-CHAIR SOLID-WALL RETEST PENDING / prior camera-specific PASS preserved / broader C1-C3 manual acceptance not inferred.`
- Protected Scope:
  `26 Human Owner untracked items unchanged and unstaged / main, Scene, Prefab, Input Actions, Packages, ProjectSettings, production art and C4+ unchanged.`
- Next:
  `Human Owner manual Binder-Chair wall-containment retest, then report the explicit disposition to GPT Pro. C4 remains unauthorized.`

## C0-C3 Pointer Capture and Pickup Clarity Repair — 2026-08-09

- Goal:
  `Repair the Human Owner-reported WebGL pointer-capture and unclear direct-pickup usability defects without entering C4.`
- Human Owner Decision:
  `Fix the reported issues now. Lack of structural fun must be reported honestly against the still-unimplemented later Contest checkpoints.`
- Baseline:
  `607282c84a12eff860c90d1f2cd81c6829a56825 / contest branch local-origin equal / tracked and staged clean / protected 26 preserved.`
- Work:
  `Added a trusted-event WebGL pointer-lock bridge and real DOM-state UI; blocked unlocked Look-Aim and first-capture mouse side effects; replaced wide body-forward target scoring with exact current camera-center ray, nearest-solid line of sight and 2.8 m closest-point range; added target/rejection/attempt feedback and deterministic camera-controller-presenter ordering; preserved Reset priority.`
- Commit:
  `b2cca79de731be91392a0c288a22d1a8f25c9077 / pushed and remote-verified.`
- Tests:
  `EditMode 41/41 / PlayMode 25/25 / total 66/66 PASS.`
- Build:
  `PASS / GUID 17afceeeb36e4733a1034a54a585ef5e / 113790231 Unity-reported bytes / warnings 0 / errors 0 / 19 HTTP files / 113791985 HTTP bytes / SHA-256 c8d90f0cab2af3fec957f8fb0bf21040f223bd33f5cf9a9f577f4851eee78e45.`
- Browser:
  `In-app browser cold WebGL load PASS / 960x600 canvas / new capture and center-reticle instructions visible / blocking console warning-error 0 / dialog 0.`
- Manual Status:
  `PENDING / trusted click capture, first-click safety, Escape release, recapture and pickup-clarity feel require Human Owner input. Automation is not accepted as trusted pointer evidence.`
- Fun Status:
  `Current C0-C3 remains a movement-pickup-carry-throw-reset sandbox. C4 ally relay, C5 Cargo Strap, C6 objective-route-win and C7 pressure-failure are not implemented, so structural fun is not claimed.`
- Protected Scope:
  `26 Human Owner untracked items unchanged and unstaged / main, Packages, ProjectSettings, production art and C4+ unchanged.`
- Next:
  `Human Owner manual usability retest in the retained WebGL tab. C4 remains unauthorized until a new direct instruction.`

## C0-C3 Embedded Browser Mouse-Look and Grounded Carry Lift-Off Repair — 2026-08-10

- Goal:
  `Restore in-app-browser camera movement and grounded Binder-Chair acquisition without entering C4.`
- Root Cause:
  `The embedded Chromium host rejected standard Pointer Lock, while gameplay incorrectly required the locked state; separately, the carry BoxCast treated authored floor support as a zero-distance blocking overlap during upward acquisition.`
- Work:
  `Added an instance-owned DOM mouse-delta fallback after a real pointer-lock rejection, preserved standards-based Chrome-Edge Pointer Lock, routed fallback look through the logical input owner, kept first-click and Escape safety, and allowed only upward separation from authored support while preserving wall-lateral collision.`
- Commits:
  `ff2c3cc2ff1fa61057798e963f617c8be3d89e28 / 3c9416f697fbb965bf4b06c4b8b06357d5f2370b / pushed and remote-verified.`
- Tests:
  `EditMode 44/44 / PlayMode 26/26 / total 70/70 PASS.`
- Build:
  `PASS / GUID 522b5d42361645c884e5bb293b2876c2 / 113801187 Unity-reported bytes / warnings 0 / errors 0 / 19 HTTP files / 113802941 HTTP bytes / SHA-256 58c4c16572fdf62cac474bf26eb2786e20a55e95ce12cc0053eb6e9b8e9afaf4.`
- Browser:
  `Cold WebGL load PASS / embedded fallback mouse movement and Escape release observed / blocking product console warning-error 0 / dialog 0.`
- Human Owner Manual:
  `PASS / repaired mouse-look and grounded-object acquisition accepted to proceed on 2026-08-10 / broader C1-C3 acceptance not inferred.`
- Designer A Delivery Audit:
  `Remote source batch 6094b41143286b6e1710087a711707416b366c20 contains 159 files, including 155 FBX, but is REVISION REQUIRED because A1 sequence, allowed paths, editable source, previews, license evidence and ENY01 canon are not satisfied. Corrective instruction 6030c4af6b800d0d7e80013382babd7b1cfc58a0 was pushed; no runtime integration or PR was performed.`
- Protected Scope:
  `Protected Human Owner untracked work remained unstaged and unmodified / main, packages, ProjectSettings, production-art runtime and C4+ unchanged.`
- Next:
  `Designer A must return only the CHR01 A1 editable blockout and three pose previews with complete rights metadata. C4 remains closed pending a new Human Owner authorization.`

## C4 Fixed Ally Physical Relay and Designer A Local Unity Review — 2026-08-10

- Goal:
  `Execute GPT Pro's bounded C4 recommendation and make the Designer A delivery directly inspectable without integrating it into Contest runtime art or touching the Web build.`
- Human Owner Decision Preserved:
  `Proceed immediately; no Web build; inspect designer work in Unity Scene.`
- C4 Baseline:
  `04b33ab3e7f48cb461606d9ffbafe4e83f060a93 / contest local-origin-remote equal / tracked-staged clean / protected 26 unchanged.`
- C4 Work:
  `Implemented one fixed ally relay owner, exact two-Binder and one-Chair supply pool, pre-release reservation, physical Light launch and Catch, queued player Throw, grounded Medium feed, miss/settle resolution and full authored Reset restoration.`
- C4 Commit:
  `30376d475ca0e07e9026965a9f11c6cfec241ba5 / normal push complete and remote-verified.`
- Tests:
  `Unity compile PASS / EditMode 62/62 / PlayMode 47/47 / total 109/109.`
- Web:
  `Not run and not modified by Human Owner directive. No new HTTP or browser qualification is claimed.`
- Designer A Review Setup:
  `Created a detached local worktree at C:/Users/jhbjm/Desktop/break-notice-designerA-review from origin/art/characters-animation 6030c4af6b800d0d7e80013382babd7b1cfc58a0, copied the 159-file incoming batch into an untracked _LocalArtReview area, and opened LOCAL_REVIEW_ONLY_DesignerA_Characters in Unity 6000.3.20f1. Four model Prefabs are displayed under neutral lighting with explicit rights, Canon and non-integration warnings.`
- Designer A Repository State:
  `Detached worktree only / tracked dirty 0 / staged 0 / commit 0 / push 0. The local review Scene does not approve rights, Canon, runtime integration, elite enemy or firearm content.`
- Next:
  `Human Owner Unity inspection of Designer A models and C4 manual gameplay acceptance. C5 remains unauthorized.`
