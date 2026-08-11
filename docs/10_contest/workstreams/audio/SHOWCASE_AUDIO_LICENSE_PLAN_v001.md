# BREAK NOTICE Showcase Audio License Plan v001

- 상태: `SOURCE CANDIDATES ONLY / NO DOWNLOAD / NO LICENSE EVIDENCE DELIVERED`
- Ticket: `SHOWCASE-AUDIO-SOURCE-001`
- Relay: `EAR-CONTEST-0002-AUDIO-SOURCE-001`
- 조사 기준일: `2026-08-12 KST`
- 법률 판정: `NOT PROVIDED`
- Runtime 승인: `0`

## 1. 목적

상업적 Contest 제출과 이후 포트폴리오 사용을 고려하여, 실제 source를
취득하기 전에 허용 가능한 권리 유형, 증거 묶음, 금지 조건과 후보를
고정한다. 인터넷에 무료로 공개됐다는 사실만으로 사용권을 추론하지 않는다.

이번 Relay에서는 웹 페이지를 읽어 후보만 조사했다. 오디오 파일, page
snapshot, license PDF/HTML, receipt 또는 EULA acceptance evidence를
다운로드하지 않았다. 따라서 아래 후보는 전부 `REVIEW REQUIRED`이며 실제
source 상태는 `MISSING`이다.

## 2. 허용 등급

| 등급 | 조건 | 현재 처리 |
|---|---|---|
| A — ORIGINAL | Human Owner 또는 승인된 제작자가 직접 녹음·제작하고 제작일, 장비, 장소, performer/property consent와 원본 hash를 기록 | 가장 우선; 실제 제작은 별도 Relay |
| A — CC0 | 개별 asset page가 CC0를 명시하고, source URL·creator·원본 filename·download date·license text·page snapshot·hash를 모두 보존 | 후보 가능; 증거 전에는 MISSING |
| B — CC BY 4.0 | 상업 이용·변형 허용이 명확하고 title, creator, URL, license URL, 변경 내역을 Credits에 정확히 표시 | Human Owner 검토 후 조건부 가능 |
| B — COMMERCIAL EULA | game synchronization, modification, 상업 이용과 binary 재배포 경계가 명확하며 download 당시 EULA/receipt를 보존 | open-source로 부르지 않음; 별도 검토 필요 |
| REJECT | NC, ND, Sampling+, editorial-only, ripped content, 권리 상충, 출처 불명, 제3자 IP/음악/음성 위험 | 사용 금지 |

Creative Commons의 CC BY 4.0 요약은 상업적 공유·변형을 허용하지만 적절한
credit, license link와 변경 고지를 요구한다. CC0는 public-domain dedication
도구지만, source uploader가 실제 권리를 가졌는지까지 보증하는 것은 아니다.
따라서 license label과 provenance를 별도로 검토한다.

## 3. 공식 정책 근거

| Source | 확인한 공식 근거 | 이 계획의 판정 |
|---|---|---|
| Creative Commons CC0 | `https://creativecommons.org/publicdomain/zero/1.0/` | CC0 deed와 개별 asset 적용 표시를 함께 보존 |
| Creative Commons CC BY 4.0 | `https://creativecommons.org/licenses/by/4.0/` | 상업 이용 가능하나 attribution·license link·변경 고지 필수 |
| Kenney Support | `https://kenney.nl/support` | asset page의 game asset은 CC0이며 상업 프로젝트 사용 가능; 선택 pack의 포함 license도 별도 보존 |
| Freesound FAQ | `https://freesound.org/help/faq/` | sound마다 CC0/CC BY/CC BY-NC가 다르고 user-uploaded content에는 잘못 업로드된 저작물 위험이 존재하므로 개별 page와 provenance 검토 필수 |
| Sonniss GDC Bundle EULA | `https://sonniss.com/gdc-bundle-license/` | 상업 game synchronization과 modification은 허용되지만 원본 재판매 금지·AI training 금지의 별도 EULA; open-source 아님 |

## 4. 우선 후보 풀

### 4.1 Pack-level 후보

| Candidate ID | URL | 2026-08-12 관찰 상태 | 잠정 용도 | 판정 |
|---|---|---|---|---|
| `KENNEY-INTERFACE-SOUNDS` | `https://kenney.nl/assets/interface-sounds` | 100 files, asset page `Creative Commons CC0` | UI confirm/reject, Charge start/full, result-card | `MISSING / REVIEW REQUIRED` |
| `KENNEY-IMPACT-SOUNDS` | `https://kenney.nl/assets/impact-sounds` | 130 files, asset page `Creative Commons CC0` | Binder/Chair/static impact의 보조 layer | `MISSING / REVIEW REQUIRED` |
| `SONNISS-GDC-2026` | `https://gdc.sonniss.com/` | 7.47 GB+, 347+ files, royalty-free commercial EULA 표시 | exact office/foley 후보가 track list에 있을 때만 | `DEFERRED / NOT OPEN-SOURCE / REVIEW REQUIRED` |

Pack 전체를 저장소에 넣지 않는다. 필요한 selected file만 별도 Relay에서
취득하고 pack license, selected original path와 hash를 묶는다.

### 4.2 Freesound 개별 CC0 후보

| Candidate ID | Creator / Original title | URL | 관찰 format | 잠정 용도 | 판정 |
|---|---|---|---|---|---|
| `FS-537769` | janbezouska / `Factory Fluorescent light buzz` | `https://freesound.org/people/janbezouska/sounds/537769/` | WAV, 48 kHz, 24-bit, stereo, page CC0 | Operations Floor room-tone/fluorescent bed | `MISSING / REVIEW REQUIRED` |
| `FS-518099` | cciecko / `Fluorescent light turn on and hum` | `https://freesound.org/people/cciecko/sounds/518099/` | WAV, 48 kHz, 16-bit, stereo, page CC0 | fixture hum 또는 Exit-light relay layer | `MISSING / REVIEW REQUIRED` |
| `FS-15475` | elonen / `rolling-office-chair.wav` | `https://freesound.org/people/elonen/sounds/15475/` | WAV, 44.1 kHz, 16-bit, mono, page CC0 | caster roll/drag; hiss cleanup review | `MISSING / REVIEW REQUIRED` |
| `FS-708603` | neverak / `Chair Rolling on Wood - Fast and Slow - Stereo 48kHz 24bit - OWI` | `https://freesound.org/people/neverak/sounds/708603/` | page reports WAV, 44.1 kHz, 24-bit, stereo, CC0 | alternate Chair roll texture; floor mismatch review | `MISSING / REVIEW REQUIRED` |
| `FS-513771` | lartti / `rustling paper.wav` | `https://freesound.org/people/lartti/sounds/513771/` | WAV, 44.1 kHz, 16-bit, stereo, page CC0 | Binder grip/carry/aftermath layer | `MISSING / REVIEW REQUIRED` |
| `FS-581404` | SamsterBirdies / `Filing Cabinet Drawer Open/Close` | `https://freesound.org/people/SamsterBirdies/sounds/581404/` | FLAC, 88.2 kHz, 16-bit, stereo, page CC0 | Drawer mechanism/settle layer; movement source 아님 | `MISSING / REVIEW REQUIRED` |
| `FS-108849` | orestes910 / `Ratchet1.wav` | `https://freesound.org/people/orestes910/sounds/108849/` | WAV, 44.1 kHz, 16-bit, stereo, page CC0 | Carabiner/Clamp/Strap ratchet layer | `MISSING / REVIEW REQUIRED` |
| `FS-433884` | jodybruchon / `Footsteps - quiet.wav` | `https://freesound.org/people/jodybruchon/sounds/433884/` | WAV, 96 kHz, 24-bit, stereo, page CC0 | Player/Runner footstep extraction 후보 | `MISSING / REVIEW REQUIRED` |

Freesound FAQ는 user upload에 불법 material이 섞일 가능성을 명시한다. 따라서
위 page의 CC0 표시는 사용 후보 조건이지 최종 권리 승인 증거가 아니다.
voice, music, recognizable broadcast/game sample이 들어간 파일은 license label과
무관하게 제외한다.

## 5. 명시적 제외·충돌 사례

다음은 조사 중 발견했으나 선택하지 않는다.

| Source | 이유 | 처리 |
|---|---|---|
| Freesound `71251 — Chair Rolling.wav` | retired Sampling+ 표시 | `REJECT / DO NOT DOWNLOAD` |
| Freesound `125315 — 02538 rollin office chair.wav` | CC BY-NC 표시 | `REJECT / COMMERCIAL USE INELIGIBLE` |
| Freesound `386929 — Footsteps, Indoor, Soft.wav` | 설명 본문은 CC BY 4.0이라 쓰지만 page badge는 CC0로 보여 내부 권리 문구가 충돌 | `REVIEW REQUIRED / NOT SELECTED` |
| BBC Sound Effects와 유사한 personal/research-only library | 상업 Contest 이용을 자동 허용하지 않음 | `REJECT UNLESS SEPARATE WRITTEN COMMERCIAL LICENSE` |
| YouTube, 영화, 게임, 방송에서 추출한 audio | 원저작권·실연·음원권을 확인할 수 없음 | `REJECT` |

## 6. 실제 취득 시 필수 evidence bundle

각 source마다 다음 필드를 빠짐없이 기록한다.

- Internal Source ID
- Cue ID mapping
- Original title와 filename
- Creator/uploader
- Exact source URL
- Download timestamp with timezone
- Page-reported license와 canonical license URL
- Page snapshot/PDF 또는 legally saved HTML
- Included license/readme 원본
- Original bytes와 SHA-256
- Original sample rate, bit depth, channels, duration
- Commercial-use permission
- Modification permission
- Attribution requirement와 exact credit string
- Redistribution/standalone resale restriction
- Performer, voice, trademark, privacy, property 또는 recognizable-IP risk
- Editing tool/version과 모든 modification
- Derived filename, bytes와 SHA-256
- Human Owner decision
- Runtime integration decision

page만 저장하고 원본 hash를 빠뜨리거나, 원본만 저장하고 download 당시 license
page를 빠뜨리면 `LICENSE EVIDENCE INCOMPLETE`다.

## 7. 예상 evidence 파일명

다음 파일은 이번 Relay에서 만들지 않았으며, source acquisition Relay에서
exact allowlist에 추가돼야 한다.

- `ArtSource/Audio/Showcase/Licenses/LIC_CC0_1.0_Universal_20260812.pdf`
- `ArtSource/Audio/Showcase/Licenses/LIC_Kenney_InterfaceSounds_CC0_v001.pdf`
- `ArtSource/Audio/Showcase/Licenses/LIC_Kenney_ImpactSounds_CC0_v001.pdf`
- `ArtSource/Audio/Showcase/Licenses/LIC_Freesound_FAQ_20260812.pdf`
- `ArtSource/Audio/Showcase/Licenses/LIC_Freesound_537769_CC0_v001.pdf`
- `ArtSource/Audio/Showcase/Licenses/LIC_Freesound_518099_CC0_v001.pdf`
- `ArtSource/Audio/Showcase/Licenses/LIC_Freesound_15475_CC0_v001.pdf`
- `ArtSource/Audio/Showcase/Licenses/LIC_Freesound_708603_CC0_v001.pdf`
- `ArtSource/Audio/Showcase/Licenses/LIC_Freesound_513771_CC0_v001.pdf`
- `ArtSource/Audio/Showcase/Licenses/LIC_Freesound_581404_CC0_v001.pdf`
- `ArtSource/Audio/Showcase/Licenses/LIC_Freesound_108849_CC0_v001.pdf`
- `ArtSource/Audio/Showcase/Licenses/LIC_Freesound_433884_CC0_v001.pdf`
- `ArtSource/Audio/Showcase/Licenses/LIC_Sonniss_GDC_Bundle_EULA_v001.pdf`
- `ArtSource/Audio/Showcase/Licenses/AUDIO_SOURCE_LICENSE_LEDGER_v001.csv`

PDF/HTML 저장 가능 여부와 exact extension은 다음 Relay가 승인해야 한다.
사이트 약관이나 기술 제약상 snapshot 저장이 허용되지 않으면 URL, timestamp,
screen capture와 license text hash의 대체 증거 방식을 Human Owner가 정한다.

## 8. 예상 source 원본 파일명

원격 원본의 실제 filename은 보존하고, 저장소 사본은 Source ID를 앞에 붙여
충돌을 방지한다. 다음은 현재 후보의 예상 저장명이며 실제 download 전
확정할 수 없다.

- `SRC_FS-537769_FactoryFluorescentLightBuzz_original.wav`
- `SRC_FS-518099_FluorescentLightTurnOnAndHum_original.wav`
- `SRC_FS-15475_RollingOfficeChair_original.wav`
- `SRC_FS-708603_ChairRollingFastSlow_original.wav`
- `SRC_FS-513771_RustlingPaper_original.wav`
- `SRC_FS-581404_FilingCabinetDrawerOpenClose_original.flac`
- `SRC_FS-108849_Ratchet1_original.wav`
- `SRC_FS-433884_FootstepsQuiet_original.wav`

Kenney pack은 selected internal file을 먼저 audition한 뒤
`SRC_KENNEY-INTERFACE_<OriginalStem>_original.<ext>` 또는
`SRC_KENNEY-IMPACT_<OriginalStem>_original.<ext>`로 기록한다. 원 pack의
원래 상대 경로와 filename도 ledger에 별도로 보존한다.

## 9. 변환·편집 규칙

- 원본은 수정하지 않는다.
- 44.1/88.2/96 kHz source는 별도 derived WAV를 48 kHz로 변환하되 tool,
  resampler와 setting을 기록한다.
- lossy MP3만 있는 후보는 transient-critical cue에서 우선 제외한다.
- Stereo source를 3D mono로 만들 때 단순 한 채널 삭제가 아니라 phase와
  mono compatibility를 확인한다.
- denoise, EQ, pitch, time-stretch, layering과 trim을 모두 ledger에 기록한다.
- 여러 source를 합친 composite는 모든 원본의 license evidence를 상속한다.
- Content ID 분쟁 대비를 위해 source page 최초 게시일과 download evidence를
  보존한다.

## 10. Stop Conditions

다음이면 source를 취득하거나 편집하지 않고 중지한다.

- asset page license와 포함 license가 다름
- creator/uploader 또는 original URL이 식별되지 않음
- commercial use, modification 또는 game synchronization 권리가 불명확함
- NC, Sampling+, editorial-only 또는 standalone-use 제한이 game build와 충돌함
- voice/performer/property consent가 필요하지만 없음
- source가 다른 게임·영화·음악·유명 IP에서 추출된 정황이 있음
- page snapshot이나 original hash를 보존할 수 없음
- 허용되지 않은 binary path 또는 Git LFS 변경이 필요함
- Runtime/Unity import나 `.meta` 생성이 필요함

## 11. 현재 결론

`SELECTED SOURCE: 0`

`DOWNLOADED AUDIO BINARY: 0`

`DELIVERED LICENSE EVIDENCE: 0`

`RUNTIME APPROVED AUDIO: 0`

CC0 후보와 별도 상업 EULA 후보는 존재하지만, 현재는 페이지 수준 사전 조사에
불과하다. 실제 source 취득·evidence 고정·편집은 별도 Human Owner Relay가
필요하다.
