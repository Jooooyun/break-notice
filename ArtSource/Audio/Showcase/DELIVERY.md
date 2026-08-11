# BREAK NOTICE — Showcase Audio Delivery

Asset ID: `AUD-SHOWCASE-001`

Asset Name: `CONTEST-0002 Operations Floor Showcase Audio`

Creator: `NOT ASSIGNED`

GitHub ID: `N/A`

Contribution Date: `N/A — PREPARATION ONLY`

Current Status: `MISSING / SOURCE-DESIGN-LICENSE PREPARATION COMPLETE / NO BINARY`

Human Owner Status: `NOT REVIEWED`

Runtime Status: `NOT AUTHORIZED / S9-BOUND`

## Work Classification

- Original: `PREFERRED / NOT DELIVERED`
- Purchased: `NONE`
- Open License: `CC0 CANDIDATES IDENTIFIED / NOT DOWNLOADED`
- AI Generated: `NONE`
- Mixed: `NONE`

## Exact License

`MISSING — NO SOURCE SELECTED`

Written Permission: `NONE / NOT APPLICABLE YET`

Reference Used:

- `docs/10_contest/workstreams/audio/SHOWCASE_AUDIO_DIRECTION_v001.md`
- `docs/10_contest/workstreams/audio/SHOWCASE_AUDIO_CUE_MATRIX_v001.csv`
- `docs/10_contest/workstreams/audio/SHOWCASE_AUDIO_LICENSE_PLAN_v001.md`
- `docs/10_contest/workstreams/audio/SHOWCASE_AUDIO_DELIVERY_CHECKLIST_v001.md`

## Current Files

Source Files: `0`

Export Files: `0`

Audio Files: `0`

License Files: `0`

Preview Files: `0`

Unity Runtime Files: `0`

`.meta` Files: `0`

## Folder Roles

- `Source/`: future byte-preserved original recordings or licensed downloads.
- `Licenses/`: future license deed/EULA, asset page evidence and source ledger.
- `Preview/`: future low-size audition and gameplay-context review reels.

현재 `.gitkeep` 외에는 어떤 파일도 두지 않는다. source acquisition Relay의
exact allowlist가 생기기 전에는 binary, snapshot 또는 ledger를 추가하지 않는다.

## Expected Cue Files

각 primary expected filename의 정확한 목록은
`docs/10_contest/workstreams/audio/SHOWCASE_AUDIO_CUE_MATRIX_v001.csv`의
`Expected Filename` 열을 따른다. 주요 family는 다음과 같다.

- `AUD_AMB_*` — Operations Floor ambience와 fixture/alarm/tease.
- `AUD_PLY_*` — Player movement, acquisition, carry, Charge와 Release.
- `AUD_BND_*` — Binder airborne, impact, Catch와 Miss.
- `AUD_CHR_*` — Chair grounded roll/feed/skid/impact.
- `AUD_ALY_*` — READY, Q result, physical supply와 rejection.
- `AUD_STR_*` — Strap deploy/attach/tension/redirect/release.
- `AUD_TAG_*` — PASS/BRACE/BREAK/TEAM READY와 Finisher eligibility.
- `AUD_RUN_*` — Runner pressure, contact, Grab, Rescue와 Stagger.
- `AUD_DRW_*` — Drawer impact/movement/route-open/settle.
- `AUD_FIN_*` — conditional HANDOVER CRASH causal sequence.
- `AUD_RST_*` — Reset barrier.
- `AUD_EXT_*` — Exit, clear와 result card.
- `AUD_UI_*` — generic confirm/reject와 conditional Q-context change.

Variation은 approved primary stem에 `_02`, `_03` 순으로 추가하고 기존 파일을
덮어쓰지 않는다. `final.wav`, `final2.wav`, `new.wav`, 한글명과 공백을 쓰지
않는다.

## Future Technical Delivery

권장 Runtime Export:

- WAV
- 48 kHz
- 24-bit preferred 또는 16-bit accepted
- 3D cue는 mono preferred
- ambience와 의도된 wide cue는 stereo
- trim 상태, loop point, peak/clipping, channel과 duration 기록

정확한 loudness, peak, compression, Unity import, AudioMixer route, spatial
blend, rolloff, occlusion과 voice limit은 `PROPOSED / REVIEW REQUIRED`다.

## Required Source Manifest Fields

- Cue ID
- Internal Source ID
- Original title/filename
- Creator/uploader
- Exact URL
- Download timestamp/timezone
- License name와 canonical URL
- Commercial-use permission
- Modification permission
- Attribution string
- Original bytes/SHA-256
- Sample rate/bit depth/channels/duration
- Editing tool/version와 modification log
- Derived filename/bytes/SHA-256
- Known noise, clipping, loop, phase 또는 provenance issue
- Human Owner decision
- Runtime integration decision

## Known Issues

- 실제 source가 선택되지 않았다.
- license evidence가 없다.
- candidate page status는 download-time evidence가 아니다.
- Freesound 후보는 user-upload provenance 추가 검토가 필요하다.
- exact Runtime hook과 timing이 확정되지 않았다.
- audio preview와 Human Owner listening review가 없다.

## Requested Integration

`NONE — DO NOT INTEGRATE`

Blocking Feature: `NONE FOR THIS SOURCE-DESIGN PACKAGE / AUDIO BINARIES REMAIN MISSING FOR FUTURE S9`

Suggested Runtime Path: `PROPOSED / REVIEW REQUIRED — Game/Assets/_Project/Audio/<Category>/`

No new production asset blocks this work package. Graybox fallback remains active.
