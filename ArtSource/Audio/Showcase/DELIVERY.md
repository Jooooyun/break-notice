# BREAK NOTICE — Showcase Audio Delivery

Asset ID: `AUD-SHOWCASE-002`

Asset Name: `CONTEST-0002 Operations Floor Showcase Audio Bulk Production v002`

Ticket: `SHOWCASE-AUDIO-BULK-PRODUCTION-001`

Relay: `EAR-CONTEST-0002-AUDIO-BULK-PRODUCTION-001`

Contribution Date: `2026-08-12 KST`

Current Status: `DELIVERED / REVIEW_CANDIDATE / HUMAN OWNER LISTENING REQUIRED`

Runtime Status: `NOT INTEGRATED / NO UNITY OR GAMEPLAY CHANGE`

## 1. Human Owner decision preserved

The exact four audible references below are byte-preserved and carry
`HUMAN OWNER AUDIBLE REFERENCE PASS 2026-08-12`:

| Reference | SHA-256 | Result |
|---|---|---|
| Footsteps v002 | `dd6fcaf66e9ef4f26c7744379600b0da71b96a210f54e28cbdf23e0271bfae94` | `PASS / KEEP` |
| Body-contact material v002 | `853ce15e701370b4180d7ec852f2e5f1860703cdaba77b28b3659381911c8791` | `PASS / KEEP` |
| Binder Physical v003 | `21c53a4b6d6878e527f81d04da07ee9ce1a209bc8fd6eeed7381ecb845099dd9` | `PASS / KEEP` |
| Chair Physical v003 | `c314a2fcb601ed9d814921f4d06b04518e0a46ac526b6e3f5d4ebd87df51f8e8` | `PASS / KEEP` |

The body-contact reference does not approve Player Punch gameplay. It is used only as
source material for locked nonlethal Runner Body Check/Shove, Binder/Chair-to-Runner
contact, Off-Balance, Stagger, Rescue and BREAK presentation.

All 196 new exports and all five new audition reels remain `REVIEW_CANDIDATE` until
Human Owner listening. No new file is labeled `FINAL`, `APPROVED` or `LOCKED`.

## 2. Historical v001 preservation

The prior deterministic procedural-synthesis v001 pack remains byte-preserved in its
existing paths as `HISTORICAL / SUPERSEDED SOURCE PASS`.

- Existing v001 source masters: `23`
- Existing v001 review exports: `23`
- Existing v001 audition reel: `1`
- Existing v001 files overwritten or deleted: `0`
- v001 items counted toward ProductionV002 totals: `0`

## 3. ProductionV002 delivery counts

- Logical Cue coverage: `75 / 75`
- REQUIRED family coverage: `67 / 67`
- CONDITIONAL family coverage: `8 / 8`
- Runtime-ready review exports: `196`
- Minimum required exports: `187`
- Variation shortfall: `0`
- Raw CC0 recorded sources: `26`
- Byte-preserved Human Owner references: `4`
- Clean source masters extracted from passed references: `26`
- License evidence HTML files: `28`
- Source/license ledger rows: `257`
- Audition reels: `5`
- Waveform contact sheets: `1`
- Full export manifests: `1 JSON`
- Technical QA reports: `1 JSON`
- ProductionV002 bytes: `139,179,201`
- Showcase audio tree total bytes, including preserved v001: `164,212,273`

### Export count by family

| Family | Count |
|---|---:|
| AMB | 7 |
| PLY | 38 |
| BND | 21 |
| CHR | 20 |
| ALY | 15 |
| STR | 15 |
| TAG | 8 |
| RUN | 35 |
| DRW | 10 |
| FIN | 13 |
| RST | 2 |
| EXT | 5 |
| UI | 7 |
| **Total** | **196** |

Each Matrix row records its exact output count and exact destination filenames in
`SHOWCASE_AUDIO_CUE_MATRIX_v001.csv`.

## 4. Source and license classification

ProductionV002 uses only:

1. official LaSonothèque file-level CC0 recorded Foley with individual page evidence;
2. the four exact Human Owner-passed audible reference reels;
3. project Original deterministic DSP support layers.

No CC BY, NC, ND, Sampling+, paid-EULA raw source, ambiguous aggregator upload,
external generative-audio service, voice model, game/film rip, music or known-brand
sound was used.

Selected source resolutions are reported honestly:

- 23 selected originals: `48 kHz / 24-bit`;
- 3 selected originals (`LASO-0516`, `LASO-0666`, `LASO-0786`): `48 kHz / 16-bit`;
- all delivered exports: `48 kHz / 24-bit PCM container`;
- a 24-bit delivery container is not claimed to increase underlying 16-bit source resolution.

Exact URLs, authors, filenames, recovery timestamps, evidence paths and hashes,
original/destination hashes, sizes, durations, channels, sample rates, bit depths,
peaks and transformations are in:

- `Licenses/ProductionV002/AUDIO_SOURCE_LEDGER_v002.csv`
- `Licenses/ProductionV002/AUDIO_SOURCE_PROVENANCE_v002.md`
- `Licenses/ProductionV002/Evidence/`

## 5. Production format and QA

- Decode: `PASS — 196 / 196 exports + 5 / 5 reels`
- Format: `PASS — PCM WAV / 48,000 Hz / 24-bit`
- Spatial one-shots/loops: `mono-first`
- Ambience, result and intentional UI width: `stereo where listed`
- Clipped files: `0`
- Full-scale files: `0`
- Silent/corrupt files: `0`
- Excessive DC files: `0`
- Maximum measured DC offset: `0.0000988892 full scale`
- Duplicate destination: `0`
- Byte-identical cross-cue export: `0`
- Loop candidates: `20`
- Loop seam: `PASS — 20 / 20`
- Worst loop seam jump: `0.08968 × ordinary p99 sample step`
- Worst loop seam 10 ms RMS delta: `0.01641 dB`
- Worst aligned within-cue variation correlation: `0.96974`
- Contact sheet: `SHOWCASE_AUDIO_WAVEFORM_CONTACT_SHEET_v002.png`
- Machine-readable QA: `SHOWCASE_AUDIO_TECHNICAL_QA_v002.json`

Every export was decoded and inspected for format, nonzero samples, channels, peak,
full-scale/clipping, DC, duplicate hash and duration. Every loop was seam-checked in
the waveform domain. Every one of the `196` runtime-ready review exports is represented
exactly once across the audition reels. The reels preserve relative hierarchy and add
`0.380 s` silence between files without voice or music.

## 6. Audition package

All paths below are relative to `ArtSource/Audio/Showcase/Preview/ProductionV002/`.

### AUDITION_A_PLAYER_BINDER_CHAIR_v002.wav

- Duration: `88.558 s`
- Bytes: `25,504,718`
- SHA-256: `3a24ec7534eaec370fb5663927f3d8696c790c08c3b833fd4665f4c94e9137a4`

| Time | Cue ID | First -> last file | Variations |
|---:|---|---|---:|
| 00:00.500–00:05.223 | `AUD-PLY-001` | `AUD_PLY_Footstep_OfficeFloor_01_v002.wav` -> `AUD_PLY_Footstep_OfficeFloor_06_v002.wav` | 6 |
| 00:05.603–00:10.271 | `AUD-PLY-002` | `AUD_PLY_Footstep_OfficeFloor_Fast_01_v002.wav` -> `AUD_PLY_Footstep_OfficeFloor_Fast_06_v002.wav` | 6 |
| 00:10.651–00:13.198 | `AUD-PLY-003` | `AUD_PLY_Binder_Acquire_01_v002.wav` -> `AUD_PLY_Binder_Acquire_03_v002.wav` | 3 |
| 00:13.578–00:16.132 | `AUD-PLY-004` | `AUD_PLY_Chair_Acquire_01_v002.wav` -> `AUD_PLY_Chair_Acquire_03_v002.wav` | 3 |
| 00:16.512–00:19.059 | `AUD-PLY-005` | `AUD_PLY_Binder_CarrySettle_01_v002.wav` -> `AUD_PLY_Binder_CarrySettle_03_v002.wav` | 3 |
| 00:19.439–00:22.078 | `AUD-PLY-006` | `AUD_PLY_Chair_CarrySettle_01_v002.wav` -> `AUD_PLY_Chair_CarrySettle_03_v002.wav` | 3 |
| 00:22.458–00:22.828 | `AUD-PLY-007` | `AUD_PLY_ThrowCharge_Start_v002.wav` | 1 |
| 00:23.208–00:25.988 | `AUD-PLY-008` | `AUD_PLY_ThrowCharge_Build_Light_Loop_v002.wav` -> `AUD_PLY_ThrowCharge_Build_Medium_Loop_v002.wav` | 2 |
| 00:26.368–00:26.785 | `AUD-PLY-009` | `AUD_PLY_ThrowCharge_Full_v002.wav` | 1 |
| 00:27.165–00:30.587 | `AUD-PLY-010` | `AUD_PLY_Binder_ThrowRelease_01_v002.wav` -> `AUD_PLY_Binder_ThrowRelease_04_v002.wav` | 4 |
| 00:30.967–00:34.646 | `AUD-PLY-011` | `AUD_PLY_Chair_ThrowRelease_01_v002.wav` -> `AUD_PLY_Chair_ThrowRelease_04_v002.wav` | 4 |
| 00:35.026–00:36.129 | `AUD-PLY-012` | `AUD_PLY_ThrowCharge_Cancel_01_v002.wav` -> `AUD_PLY_ThrowCharge_Cancel_02_v002.wav` | 2 |
| 00:36.509–00:39.789 | `AUD-BND-001` | `AUD_BND_AirborneFlutter_01_Loop_v002.wav` -> `AUD_BND_AirborneFlutter_02_Loop_v002.wav` | 2 |
| 00:40.169–00:44.607 | `AUD-BND-002` | `AUD_BND_Impact_OfficeSurface_01_v002.wav` -> `AUD_BND_Impact_OfficeSurface_05_v002.wav` | 5 |
| 00:44.987–00:49.626 | `AUD-BND-003` | `AUD_BND_Impact_Runner_01_v002.wav` -> `AUD_BND_Impact_Runner_04_v002.wav` | 4 |
| 00:50.006–00:53.586 | `AUD-BND-004` | `AUD_BND_Impact_StaticOffice_01_v002.wav` -> `AUD_BND_Impact_StaticOffice_04_v002.wav` | 4 |
| 00:53.966–00:56.531 | `AUD-BND-005` | `AUD_BND_Catch_Secure_01_v002.wav` -> `AUD_BND_Catch_Secure_03_v002.wav` | 3 |
| 00:56.911–01:01.271 | `AUD-BND-006` | `AUD_BND_CatchMiss_Land_01_v002.wav` -> `AUD_BND_CatchMiss_Land_03_v002.wav` | 3 |
| 01:01.651–01:06.431 | `AUD-CHR-001` | `AUD_CHR_CasterRoll_Slow_Loop_v002.wav` -> `AUD_CHR_CasterRoll_Fast_Loop_v002.wav` | 2 |
| 01:06.811–01:10.791 | `AUD-CHR-002` | `AUD_CHR_AllyFeed_Push_01_Loop_v002.wav` -> `AUD_CHR_AllyFeed_Push_02_Loop_v002.wav` | 2 |
| 01:11.171–01:15.065 | `AUD-CHR-003` | `AUD_CHR_WheelSkid_01_v002.wav` -> `AUD_CHR_WheelSkid_04_v002.wav` | 4 |
| 01:15.445–01:20.133 | `AUD-CHR-004` | `AUD_CHR_Impact_StaticOffice_01_v002.wav` -> `AUD_CHR_Impact_StaticOffice_05_v002.wav` | 5 |
| 01:20.513–01:25.278 | `AUD-CHR-005` | `AUD_CHR_Impact_Runner_01_v002.wav` -> `AUD_CHR_Impact_Runner_04_v002.wav` | 4 |
| 01:25.658–01:28.178 | `AUD-CHR-006` | `AUD_CHR_AllyFeed_Release_01_v002.wav` -> `AUD_CHR_AllyFeed_Release_03_v002.wav` | 3 |

### AUDITION_B_ALLY_STRAP_TAGS_v002.wav

- Duration: `41.493 s`
- Bytes: `11,950,010`
- SHA-256: `c1341683130aababe7754fa826119e910d86214a13c0e12d1337be470369528c`

| Time | Cue ID | First -> last file | Variations |
|---:|---|---|---:|
| 00:00.500–00:02.049 | `AUD-ALY-001` | `AUD_ALY_Ready_01_v002.wav` -> `AUD_ALY_Ready_02_v002.wav` | 2 |
| 00:02.429–00:03.637 | `AUD-ALY-002` | `AUD_ALY_QSupply_Accept_01_v002.wav` -> `AUD_ALY_QSupply_Accept_02_v002.wav` | 2 |
| 00:04.017–00:06.555 | `AUD-ALY-003` | `AUD_ALY_BinderSupply_Release_01_v002.wav` -> `AUD_ALY_BinderSupply_Release_03_v002.wav` | 3 |
| 00:06.935–00:08.491 | `AUD-ALY-004` | `AUD_ALY_ChairFeed_Start_01_v002.wav` -> `AUD_ALY_ChairFeed_Start_02_v002.wav` | 2 |
| 00:08.871–00:09.981 | `AUD-ALY-005` | `AUD_ALY_SupplyReject_NoCandidate_01_v002.wav` -> `AUD_ALY_SupplyReject_NoCandidate_02_v002.wav` | 2 |
| 00:10.361–00:11.477 | `AUD-ALY-006` | `AUD_ALY_SupplyReject_PathBlocked_01_v002.wav` -> `AUD_ALY_SupplyReject_PathBlocked_02_v002.wav` | 2 |
| 00:11.857–00:13.175 | `AUD-ALY-007` | `AUD_ALY_Regroup_Accept_01_v002.wav` -> `AUD_ALY_Regroup_Accept_02_v002.wav` | 2 |
| 00:13.555–00:15.531 | `AUD-STR-001` | `AUD_STR_Deploy_CarabinerOpen_01_v002.wav` -> `AUD_STR_Deploy_CarabinerOpen_03_v002.wav` | 3 |
| 00:15.911–00:20.271 | `AUD-STR-002` | `AUD_STR_AssistAttach_Lock_01_v002.wav` -> `AUD_STR_AssistAttach_Lock_03_v002.wav` | 3 |
| 00:20.651–00:24.131 | `AUD-STR-003` | `AUD_STR_Tension_Light_Loop_v002.wav` -> `AUD_STR_Tension_Loaded_Loop_v002.wav` | 2 |
| 00:24.511–00:27.991 | `AUD-STR-004` | `AUD_STR_Redirect_Load_Left_Loop_v002.wav` -> `AUD_STR_Redirect_Load_Right_Loop_v002.wav` | 2 |
| 00:28.371–00:31.081 | `AUD-STR-005` | `AUD_STR_Release_Retract_01_v002.wav` -> `AUD_STR_Release_Retract_03_v002.wav` | 3 |
| 00:31.461–00:33.167 | `AUD-STR-006` | `AUD_STR_Attach_Reject_01_v002.wav` -> `AUD_STR_Attach_Reject_02_v002.wav` | 2 |
| 00:33.547–00:35.041 | `AUD-TAG-001` | `AUD_TAG_PASS_Award_01_v002.wav` -> `AUD_TAG_PASS_Award_02_v002.wav` | 2 |
| 00:35.421–00:36.851 | `AUD-TAG-002` | `AUD_TAG_BRACE_Award_01_v002.wav` -> `AUD_TAG_BRACE_Award_02_v002.wav` | 2 |
| 00:37.231–00:39.255 | `AUD-TAG-003` | `AUD_TAG_BREAK_Award_01_v002.wav` -> `AUD_TAG_BREAK_Award_02_v002.wav` | 2 |
| 00:39.635–00:40.325 | `AUD-TAG-004` | `AUD_TAG_TeamReady_v002.wav` | 1 |
| 00:40.705–00:41.113 | `AUD-TAG-005` | `AUD_TAG_TeamFinish_Eligible_v002.wav` | 1 |

### AUDITION_C_RUNNER_PRESSURE_v002.wav

- Duration: `41.930 s`
- Bytes: `12,075,746`
- SHA-256: `c2951d26ffba4c94a5d3f0882acfca5f32484080b533be5ba04cbcfd931e76b0`

| Time | Cue ID | First -> last file | Variations |
|---:|---|---|---:|
| 00:00.500–00:05.156 | `AUD-RUN-001` | `AUD_RUN_Footstep_OfficeFloor_01_v002.wav` -> `AUD_RUN_Footstep_OfficeFloor_06_v002.wav` | 6 |
| 00:05.536–00:08.912 | `AUD-RUN-002` | `AUD_RUN_BodyCheck_Telegraph_01_v002.wav` -> `AUD_RUN_BodyCheck_Telegraph_03_v002.wav` | 3 |
| 00:09.292–00:11.569 | `AUD-RUN-003` | `AUD_RUN_BodyCheck_Miss_01_v002.wav` -> `AUD_RUN_BodyCheck_Miss_03_v002.wav` | 3 |
| 00:11.949–00:17.071 | `AUD-RUN-004` | `AUD_RUN_BodyCheck_Hit_01_v002.wav` -> `AUD_RUN_BodyCheck_Hit_04_v002.wav` | 4 |
| 00:17.451–00:22.165 | `AUD-RUN-005` | `AUD_RUN_Shove_Hit_01_v002.wav` -> `AUD_RUN_Shove_Hit_04_v002.wav` | 4 |
| 00:22.545–00:25.724 | `AUD-RUN-006` | `AUD_RUN_Grab_Start_01_v002.wav` -> `AUD_RUN_Grab_Start_03_v002.wav` | 3 |
| 00:26.104–00:27.504 | `AUD-RUN-007` | `AUD_RUN_Grab_Hold_Loop_v002.wav` | 1 |
| 00:27.884–00:31.134 | `AUD-RUN-008` | `AUD_RUN_GrabRescue_Release_01_v002.wav` -> `AUD_RUN_GrabRescue_Release_03_v002.wav` | 3 |
| 00:31.514–00:36.220 | `AUD-RUN-009` | `AUD_RUN_OffBalance_Light_01_v002.wav` -> `AUD_RUN_OffBalance_Light_04_v002.wav` | 4 |
| 00:36.600–00:41.550 | `AUD-RUN-010` | `AUD_RUN_Stagger_Medium_01_v002.wav` -> `AUD_RUN_Stagger_Medium_04_v002.wav` | 4 |

### AUDITION_D_DRAWER_RESET_EXIT_UI_AMBIENCE_v002.wav

- Duration: `48.001 s`
- Bytes: `13,824,314`
- SHA-256: `fb5c2528078e4720ae874229545eafc84a410ca83c1a911692eb0e56ca4b1096`

| Time | Cue ID | First -> last file | Variations |
|---:|---|---|---:|
| 00:00.500–00:03.100 | `AUD-AMB-001` | `AUD_AMB_OperationsFloor_RoomTone_Loop_v002.wav` | 1 |
| 00:03.480–00:09.060 | `AUD-AMB-002` | `AUD_AMB_FluorescentHum_01_Loop_v002.wav` -> `AUD_AMB_FluorescentHum_02_Loop_v002.wav` | 2 |
| 00:09.440–00:15.020 | `AUD-AMB-003` | `AUD_AMB_LockdownAlarm_Distant_PreClear_Loop_v002.wav` -> `AUD_AMB_LockdownAlarm_Distant_PostClear_Loop_v002.wav` | 2 |
| 00:15.400–00:17.766 | `AUD-AMB-004` | `AUD_AMB_ExitTease_Distant_01_v002.wav` -> `AUD_AMB_ExitTease_Distant_02_v002.wav` | 2 |
| 00:18.146–00:24.886 | `AUD-DRW-001` | `AUD_DRW_AttributedImpact_01_v002.wav` -> `AUD_DRW_AttributedImpact_04_v002.wav` | 4 |
| 00:25.266–00:29.846 | `AUD-DRW-002` | `AUD_DRW_MoveScrape_Slow_Loop_v002.wav` -> `AUD_DRW_MoveScrape_Fast_Loop_v002.wav` | 2 |
| 00:30.226–00:31.056 | `AUD-DRW-003` | `AUD_DRW_RouteOpen_Confirm_v002.wav` | 1 |
| 00:31.436–00:33.787 | `AUD-DRW-004` | `AUD_DRW_ParkingBay_Settle_01_v002.wav` -> `AUD_DRW_ParkingBay_Settle_03_v002.wav` | 3 |
| 00:34.167–00:34.667 | `AUD-RST-001` | `AUD_RST_Reset_Accept_v002.wav` | 1 |
| 00:35.047–00:35.495 | `AUD-RST-002` | `AUD_RST_Baseline_Restored_v002.wav` | 1 |
| 00:35.875–00:37.175 | `AUD-EXT-001` | `AUD_EXT_Exit_Activated_v002.wav` | 1 |
| 00:37.555–00:40.535 | `AUD-EXT-002` | `AUD_EXT_ExitRun_Start_01_v002.wav` -> `AUD_EXT_ExitRun_Start_02_v002.wav` | 2 |
| 00:40.915–00:41.635 | `AUD-EXT-003` | `AUD_EXT_Clear_Confirm_v002.wav` | 1 |
| 00:42.015–00:42.601 | `AUD-EXT-004` | `AUD_EXT_ResultCard_Show_v002.wav` | 1 |
| 00:42.981–00:44.767 | `AUD-UI-001` | `AUD_UI_Confirm_01_v002.wav` -> `AUD_UI_Confirm_03_v002.wav` | 3 |
| 00:45.147–00:46.929 | `AUD-UI-002` | `AUD_UI_Reject_01_v002.wav` -> `AUD_UI_Reject_03_v002.wav` | 3 |
| 00:47.309–00:47.621 | `AUD-UI-003` | `AUD_UI_QContext_Changed_v002.wav` | 1 |

### AUDITION_E_HANDOVER_CRASH_CONDITIONAL_v002.wav

- Duration: `20.819 s`
- Bytes: `5,995,898`
- SHA-256: `678824fca69511379012fbda013292f8e6ee79ac49b18e12f29a1ed18ea008e2`

| Time | Cue ID | First -> last file | Variations |
|---:|---|---|---:|
| 00:00.500–00:02.000 | `AUD-FIN-001` | `AUD_FIN_HandoverCrash_Commit_v002.wav` | 1 |
| 00:02.380–00:04.115 | `AUD-FIN-002` | `AUD_FIN_HandoverCrash_Setup_01_v002.wav` -> `AUD_FIN_HandoverCrash_Setup_02_v002.wav` | 2 |
| 00:04.495–00:07.959 | `AUD-FIN-003` | `AUD_FIN_HandoverCrash_ChairRunner_01_v002.wav` -> `AUD_FIN_HandoverCrash_ChairRunner_03_v002.wav` | 3 |
| 00:08.339–00:13.599 | `AUD-FIN-004` | `AUD_FIN_HandoverCrash_DrawerCrash_01_v002.wav` -> `AUD_FIN_HandoverCrash_DrawerCrash_03_v002.wav` | 3 |
| 00:13.979–00:19.239 | `AUD-FIN-005` | `AUD_FIN_HandoverCrash_Aftermath_01_v002.wav` -> `AUD_FIN_HandoverCrash_Aftermath_03_v002.wav` | 3 |
| 00:19.619–00:20.439 | `AUD-FIN-006` | `AUD_FIN_HandoverCrash_ControlReturn_v002.wav` | 1 |

The machine-readable complete per-file timestamp index is
`Preview/ProductionV002/SHOWCASE_AUDIO_AUDITION_INDEX_v002.json`; it proves that all
`196 / 196` exports occur exactly once across the five reels.

## 7. Runtime boundary and remaining review

This delivery does not create or modify:

- `Game/**`;
- Unity Scene, AudioClip import setting, `.meta`, AudioSource or AudioMixer;
- Gameplay Runtime event, event hook, code or timing;
- volume balance, spatial rolloff, voice limit, ducking bus or final loudness mix;
- S2–S9 completion state.

All `Future Runtime Hook` values remain `PROPOSED / REVIEW REQUIRED`. The next action
is Human Owner listening to each family and recording `Keep / Revise / Cut`, with
special attention to loudness, masking and loop seam. Runtime integration requires a
separate bounded Human Owner Relay.

No new production asset blocks this work package. Graybox fallback remains active.
