# BREAK NOTICE Showcase Audio Delivery Checklist v001

- 상태: `PREPARATION CHECKLIST / NO AUDIO BINARY DELIVERED`
- Ticket: `SHOWCASE-AUDIO-SOURCE-001`
- Relay: `EAR-CONTEST-0002-AUDIO-SOURCE-001`
- Runtime Integration: `NOT AUTHORIZED`

이 체크리스트는 향후 source acquisition, 편집, Human review와 S9 integration을
분리한다. 현재 체크 가능한 것은 문서·Cue·license 계획뿐이다.

## A. Authority와 Scope

- [x] `AGENTS.md` expected SHA-256 일치
- [x] 최초 MASTER EXECUTION PROMPT expected SHA-256 일치
- [x] immutable base `f51315aea91dd00293a4f5c387f1cb748865fd11` 확인
- [x] `CONTEST-0001` C0–C4 accepted foundation 보존
- [x] `CONTEST-0002 / PRT-RM-002` Gameplay Lock만 사용
- [x] Binder 5, Chair 3, Drawer 1, Copier 1 수량 변경 없음
- [x] active Ally 1명, 동일 Runner 2명 유지
- [x] Q six-level priority 변경 없음
- [x] PASS/BRACE/BREAK, TEAM READY와 HANDOVER CRASH 의미 변경 없음
- [x] 새 Gameplay, Room, Enemy, Object, Finisher 없음
- [x] `Game/**`, Unity, Runtime, Scene, Prefab, Input, Package 변경 없음
- [x] Audio binary, `.meta`, AudioMixer, AudioSource 없음

## B. Cue Coverage

- [x] Operations Floor room tone, fluorescent와 distant alarm 방향
- [x] Player walk/fast movement
- [x] Binder/Chair acquisition과 carry settle
- [x] LMB Charge Start/Build/Full/Cancel과 Light/Medium Release
- [x] Ally READY
- [x] Q Supply accept와 REGROUP
- [x] Binder airborne supply, Catch와 Miss
- [x] Chair floor-supported feed, caster roll과 visible release
- [x] no-candidate rejection과 path/supply-lane rejection 분리
- [x] Strap deploy/assist lock/tension/redirect/explicit release/rejection
- [x] PASS/BRACE/BREAK와 TEAM READY
- [x] Runner approach, Body Check, Shove, Grab, Rescue와 Stagger
- [x] Drawer impact, movement, route-open과 Parking Bay settle
- [x] HANDOVER CRASH의 Chair → Runner → Drawer 인과 순서
- [x] Reset accept와 baseline restored
- [x] Exit activation, short Exit run, clear와 result card
- [x] UI confirm/reject
- [x] silence, priority, ducking과 loop-stop 규칙

## C. Cue Matrix 품질

- [ ] 모든 Cue ID가 최종 Human review를 통과함
- [x] Cue ID 중복이 없어야 함
- [x] 필수 14개 column이 포함돼야 함
- [x] 각 cue에 exact expected filename이 있어야 함
- [x] 실제 Runtime hook은 전부 `PROPOSED / REVIEW REQUIRED`로 표시
- [x] source 미확정 cue는 `MISSING` 또는 `REVIEW REQUIRED`
- [x] optional HANDOVER CRASH cue는 `CONDITIONAL`
- [x] Gameplay success 전에 badge/route-open cue가 재생된다고 쓰지 않음
- [x] 오디오만으로 critical state를 전달한다고 쓰지 않음

## D. Source Acquisition — 별도 Relay 필요

- [ ] exact source acquisition Relay 승인
- [ ] source별 exact allowlist 승인
- [ ] Git LFS 또는 binary 전달 방법 승인
- [ ] selected source URL 고정
- [ ] original creator/uploader 고정
- [ ] original title과 filename 고정
- [ ] download timestamp 기록
- [ ] original bytes와 SHA-256 기록
- [ ] sample rate/bit depth/channels/duration 기록
- [ ] source page snapshot 저장
- [ ] canonical license text 저장
- [ ] pack 내부 license/readme 저장
- [ ] commercial-use permission 확인
- [ ] modification permission 확인
- [ ] attribution obligation 확인
- [ ] standalone redistribution 제한 확인
- [ ] performer/voice/privacy/property/IP risk 확인
- [ ] Human Owner source selection 기록

현재 상태: `0 / NOT STARTED / NOT AUTHORIZED`.

## E. License Evidence — 별도 Relay 필요

- [ ] CC0는 개별 asset 적용 표시와 CC0 canonical deed를 함께 보존
- [ ] CC BY 4.0는 title, creator, URL, license URL와 변경 고지 문구 완성
- [ ] commercial EULA는 download 당시 원문, receipt/acceptance와 selected file
      path를 함께 보존
- [ ] Freesound user-upload provenance 추가 검토
- [ ] NC, Sampling+, editorial-only, ripped content 제외
- [ ] 모든 composite layer의 license chain 완성
- [ ] Credits 반영 여부 Human Owner 결정
- [ ] `AUDIO_SOURCE_LICENSE_LEDGER_v001.csv` 작성

현재 상태: `MISSING / PAGE-LEVEL PRE-RESEARCH ONLY`.

## F. Source Editing — 별도 Relay 필요

- [ ] original 파일 byte-preserved
- [ ] derived 파일만 trim/edit/resample
- [ ] edit tool과 version 기록
- [ ] 48 kHz WAV export
- [ ] 16-bit 또는 24-bit 기록
- [ ] 3D cue의 mono suitability 확인
- [ ] ambience/stereo cue의 phase와 loop seam 확인
- [ ] clipping 0
- [ ] DC offset 문제 0
- [ ] audible denoise artifact 0
- [ ] source noise/hiss를 정직하게 기록
- [ ] transient가 무너지는 lossy source 제외
- [ ] 각 variation이 같은 source의 단순 gain 차이만은 아님
- [ ] derived bytes와 SHA-256 기록

현재 상태: `NOT STARTED / NOT AUTHORIZED`.

## G. Preview Review — 별도 Relay 필요

각 cue family는 source 파일보다 먼저 작은 preview reel로 검토할 수 있다.

- [ ] Ambience: 최소 60초 seam/priority preview
- [ ] Binder: pickup → carry → release → airborne → impact/Catch preview
- [ ] Chair: acquire → feed/roll → skid → impact preview
- [ ] Charge: tap, partial hold, full hold, cancel, release preview
- [ ] Ally: READY, Q accept, no-candidate reject, path reject preview
- [ ] Strap: deploy, lock, tension, redirect, release, reject preview
- [ ] Team: PASS, BRACE, BREAK, TEAM READY preview
- [ ] Runner: approach, telegraph, miss, hit, shove, Grab, Rescue, Stagger preview
- [ ] Drawer: impact, scrape, route-open, settle preview
- [ ] HANDOVER CRASH: causal phase order preview
- [ ] Reset/Exit/UI preview
- [ ] gameplay-camera listening level preview
- [ ] headphones와 ordinary laptop speaker 모두 확인
- [ ] Human Owner `PASS`, `REVISION REQUIRED` 또는 `REJECT` 기록

현재 상태: `PREVIEW FILE 0`.

## H. Runtime Integration — S9 Relay 필요

- [ ] S9 one-shot Human Owner Relay
- [ ] exact immutable integration baseline
- [ ] selected technically accepted source만 사용
- [ ] Runtime filename과 source ledger mapping
- [ ] `.meta`와 Unity import setting 포함
- [ ] AudioMixer group과 bus routing 검토
- [ ] 2D/3D, mono/stereo와 rolloff 검토
- [ ] voice limit, retrigger와 stealing 검토
- [ ] Reset에서 stale loop 0
- [ ] Charge cancel/release state mismatch 0
- [ ] supply/path rejection side effect 0
- [ ] PASS/BRACE/BREAK duplicate award sound 0
- [ ] TEAM READY premature cue 0
- [ ] Drawer impact와 route-open authority 혼동 0
- [ ] HANDOVER CRASH causal order 보존
- [ ] browser compression/latency 검증
- [ ] Web build, HTTP, Chrome/Edge와 console 검증
- [ ] Human manual audio acceptance

현재 상태: `S9 NOT AUTHORIZED / 0 INTEGRATED`.

## I. Exact Expected Delivery Names

최종 cue별 primary expected filename은
`SHOWCASE_AUDIO_CUE_MATRIX_v001.csv`의 `Expected Filename` 열이 유일한
현재 목록이다. variation은 Human review 뒤 primary stem에
`_02`, `_03` 순으로 추가하며 기존 file을 덮어쓰지 않는다.

예상 source/evidence 이름은 `SHOWCASE_AUDIO_LICENSE_PLAN_v001.md`의
`예상 evidence 파일명`과 `예상 source 원본 파일명`을 따른다. 그 경로들은
현재 Relay allowlist에 없으므로 생성하지 않는다.

## J. Current Delivery Decision

- Source files: `0`
- Audio binaries: `0`
- License evidence files: `0`
- Preview files: `0`
- Runtime assets: `0`
- Technical review pass: `NO`
- Human Owner approval: `NO / NOT REQUESTED FOR AUDIO BINARIES`

다음 단계는 `SOURCE ACQUISITION + LICENSE EVIDENCE CAPTURE ONLY`의 별도
baseline-bound one-shot Relay다. Runtime integration과 같은 Relay로 합치지
않는다.
