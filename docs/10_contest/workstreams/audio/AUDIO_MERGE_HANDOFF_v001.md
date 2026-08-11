# BREAK NOTICE Showcase Audio Merge Handoff v001

- 상태: `DOCUMENTATION HANDOFF ONLY / NO MERGE AUTHORIZED`
- Ticket: `SHOWCASE-AUDIO-SOURCE-001`
- Relay: `EAR-CONTEST-0002-AUDIO-SOURCE-001`
- Source Branch: `codex/showcase-audio`
- Immutable Base: `f51315aea91dd00293a4f5c387f1cb748865fd11`
- Target lineage: `CONTEST-0002 / PRT-RM-002`
- Runtime Integration: `NOT AUTHORIZED / S9-BOUND`

## 1. 전달 결과

이번 lane은 실제 음원을 만들거나 통합하지 않고 다음만 준비했다.

1. 잠긴 Showcase Gameplay에 맞춘 audio direction.
2. locked event → cue → priority → ducking → source status → future hook을
   연결한 Cue Matrix.
3. CC0 우선 source/license 정책과 page-level 후보 조사.
4. source acquisition, preview, technical review와 S9 integration을 분리한
   delivery checklist.
5. future source, license와 preview 폴더의 빈 전달 구조.

## 2. 변경 파일

- `docs/10_contest/workstreams/audio/SHOWCASE_AUDIO_DIRECTION_v001.md`
- `docs/10_contest/workstreams/audio/SHOWCASE_AUDIO_CUE_MATRIX_v001.csv`
- `docs/10_contest/workstreams/audio/SHOWCASE_AUDIO_LICENSE_PLAN_v001.md`
- `docs/10_contest/workstreams/audio/SHOWCASE_AUDIO_DELIVERY_CHECKLIST_v001.md`
- `docs/10_contest/workstreams/audio/AUDIO_MERGE_HANDOFF_v001.md`
- `ArtSource/Audio/Showcase/DELIVERY.md`
- `ArtSource/Audio/Showcase/Source/.gitkeep`
- `ArtSource/Audio/Showcase/Licenses/.gitkeep`
- `ArtSource/Audio/Showcase/Preview/.gitkeep`

## 3. 보존한 Human Owner 결정

- Operations Floor는 현실적인 corporate ambience를 유지한다.
- Binder와 Chair의 물리적 차이, floor support와 실제 Release를 소리로
  보조한다.
- LMB Charge는 accepted 1.2-second full-charge foundation을 존중하되 Audio
  hook과 단계 timing은 확정하지 않는다.
- Ally READY, Q Supply, airborne Binder Catch와 grounded Chair feed의 물리
  순서를 보존한다.
- candidate/path rejection은 성공 cue와 혼동되지 않는다.
- PASS/BRACE/BREAK 중 distinct 두 개만 TEAM READY를 만든다.
- `HANDOVER CRASH`는 Chair → Runner → Drawer의 잠긴 causal order를 유지한다.
- Drawer route-open은 단순 impact가 아니라 authoritative room-state 변화 뒤에
  들린다.
- Reset, Exit, clear와 UI confirm/reject를 포함한다.
- destruction, firearm, magic, telekinesis, new voice/dialogue와 새 Gameplay를
  만들지 않는다.

## 4. Source와 License 상태

`SELECTED: 0 / DOWNLOADED: 0 / LICENSE EVIDENCE: 0 / PREVIEW: 0`

Page-level 후보:

- Kenney Interface Sounds — official asset page CC0.
- Kenney Impact Sounds — official asset page CC0.
- Freesound individual CC0 candidates `537769`, `518099`, `15475`, `708603`,
  `513771`, `581404`, `108849`, `433884`.
- Sonniss GDC bundle — commercial EULA candidate, not open-source, deferred.

후보를 final source로 승인하지 않는다. Freesound는 user-uploaded platform이므로
각 file의 provenance, download-time page, license text, original bytes와
SHA-256을 별도 evidence bundle로 고정해야 한다.

## 5. Proposed Runtime Hook Handoff

Cue Matrix의 `Future Runtime Hook`은 전부 `PROPOSED / REVIEW REQUIRED`다.
통합 담당자는 실제 code/event authority를 읽고 다음을 먼저 결정해야 한다.

- existing event를 재사용할지 presentation-only adapter를 둘지
- Charge press/build/full/cancel/release의 실제 event boundary
- Ally READY와 Q result의 authoritative owner
- Binder Catch, Chair feed와 Strap의 physical-fact event
- badge award와 TEAM READY의 duplicate suppression
- Runner attack token과 telegraph voice limit
- Drawer impact와 `DrawerRouteOpened` 분리
- Reset barrier의 active-loop stop order
- Exit activation, clear와 result-card order

이 문서는 위 hook 이름을 Runtime에 요구하지 않는다.

## 6. Mix Handoff

우선순위는 `P0 CRITICAL → P1 ACTION → P2 PHYSICS → P3 MOTION → P4 BED`다.
P0/P1 중 P4/P3를 낮추는 제안 범위는 direction 문서에 있으나, exact dB,
attack/release, voice limit, retrigger, stealing, loudness와 compression은
Runtime/playtest review 전 확정할 수 없다.

다음 continuity를 반드시 검증한다.

- Release 전 airborne cue 없음.
- Catch 성공과 Miss가 겹치지 않음.
- Chair feed가 airborne Throw처럼 들리지 않음.
- successful lock 전에 BRACE 없음.
- distinct badge 두 개 전 TEAM READY 없음.
- Drawer impact만으로 Route Open cue가 나지 않음.
- Reset 뒤 charge/tension/caster/grab stale loop 0.
- Finisher 미사용 Direct Chair/Strap route도 완전한 audio path를 가짐.

## 7. Do Not Merge or Integrate

현재 handoff는 다음을 승인하지 않는다.

- 다른 branch로 merge/cherry-pick/rebase
- PR 생성
- `Game/**` 수정
- AudioMixer, AudioSource, Runtime trigger 또는 code
- Scene, Prefab, Input, Package, Import Settings 또는 `.meta`
- WAV/OGG/FLAC/MP3 source 추가
- license PDF/HTML 추가
- Unity Test, build, HTTP 또는 browser smoke
- S1, historical C5 또는 S9 implementation

## 8. 다음 One-Shot 요청

권장 다음 package:

`SHOWCASE-AUDIO-SOURCE-ACQUISITION-001 — SELECTED SOURCE DOWNLOAD AND LICENSE EVIDENCE CAPTURE ONLY`

필수 Human Owner 입력:

1. 이 direction/Cue Matrix의 `PASS`, `REVISION REQUIRED` 또는 `REJECT`.
2. CC0-only로 제한할지 CC BY 4.0 및 Sonniss EULA 후보를 허용할지.
3. exact selected source ID와 cue mapping.
4. binary/LFS 전달 방식.
5. Source/Licenses/Preview의 새 exact allowlist.
6. download, snapshot, hash, optional edit/preview 범위.
7. separate S9 integration은 계속 금지한다는 명시.

## 9. 최종 범위 선언

`RUNTIME CODE MODIFIED: NO`

`UNITY SCENE MODIFIED: NO`

`AUDIO BINARY ADDED: NO`

`LICENSE EVIDENCE FILE ADDED: NO`

`UNITY TEST OR WEB BUILD RUN: NO`

`PR OR MERGE: NO`

`S1 / C5 / S9 AUTHORIZED: NO`
