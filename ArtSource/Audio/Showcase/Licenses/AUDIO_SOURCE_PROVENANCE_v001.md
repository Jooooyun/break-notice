# BREAK NOTICE — Showcase Audio Source Provenance v001

## Common Provenance

- Production pass: `SHOWCASE AUDIO PRODUCTION-SOURCE PASS 001`
- Creation date: `2026-08-12 KST`
- Source class: `PROJECT ORIGINAL / DETERMINISTIC PROCEDURAL SYNTHESIS`
- External URL: `N/A — no internet retrieval`
- External source filename: `N/A`
- Third-party audio bytes or samples: `NONE`
- Third-party license / attribution: `NONE`
- Commercial-use dependency: `NONE OUTSIDE THE PROJECT-ORIGINAL CREATION`
- Modification permission dependency: `NONE OUTSIDE THE PROJECT-ORIGINAL CREATION`
- Creator record:
  `OpenAI Codex — executed under Human Owner relay`
- Generator ID: `BREAK_NOTICE_SHOWCASE_AUDIO_SYNTH_v001`
- Toolchain: `Python 3.13.9 / NumPy 2.4.4`
- Source root: `ArtSource/Audio/Showcase/Source/Original/`
- Export root: `ArtSource/Audio/Showcase/Export/`
- Common format: `PCM WAV / 48,000 Hz / 24-bit`

모든 원본은 고정 seed, oscillator, deterministic filtered-noise, amplitude
envelope와 resonator의 수학적 조합이다. 녹음, sample pack, voice, music,
상표성 sound와 외부 audio model output은 포함하지 않았다.

## Transformation

Source master:

1. cue별 고정 seed로 mono 또는 stereo 부동소수점 신호 생성;
2. DC 제거;
3. one-shot은 짧은 attack/tail fade 적용;
4. 다시 DC 제거;
5. target export peak보다 6 dB 낮은 headroom으로 정규화;
6. 48 kHz / 24-bit PCM WAV 저장.

Export one-shot:

1. 해당 source master decode;
2. 1 ms fade-in과 8 ms fade-out;
3. DC 제거;
4. 아래 기록 peak로 sample-peak 정규화;
5. 48 kHz / 24-bit mono 또는 stereo PCM WAV 저장.

Export loop:

1. integer-period DFT 기반 순환 신호로 source를 생성;
2. source master decode와 DC 제거;
3. 기록 peak로 정규화;
4. fade 없이 48 kHz / 24-bit PCM WAV 저장;
5. last-to-first seam jump를 ordinary p99 sample step과 비교.

Runtime event 이름, trigger frame, spatial import, compression과 mixer 처리는
이 원장에 의해 승인되지 않으며 `PROPOSED / REVIEW REQUIRED`다.

## File Ledger

| # | Cue ID / family | Seed | Original source filename / SHA-256 / bytes | Final destination / SHA-256 / bytes | Duration / ch / peak |
|---:|---|---:|---|---|---|
| 01 | `AUD-UI-001` UI confirm | 1001 | `SRC_ORIG_UI_Confirm_01_master_v001.wav` / `e82389fb3ad88c585c8f47d81a574a986a07c0ab49dd87859b010364ca73d0c9` / 54,764 | `Export/AUD_UI_Confirm_01_v001.wav` / `8da9421e25e12a99c6e7fc331752a236047561156efa2ff5f40869af878adec2` / 54,764 | 0.380 s / 1 / -6.0 dBFS |
| 02 | `AUD-UI-002` UI reject | 1002 | `SRC_ORIG_UI_Reject_01_master_v001.wav` / `c9f951ef48fe947a68c0249bd46b209328e72bc9d33f4d0ba2d9dc889c132de9` / 63,404 | `Export/AUD_UI_Reject_01_v001.wav` / `912fb62e06249d800606699827b353811acf99b1509be7f317bb247fbefaac16` / 63,404 | 0.440 s / 1 / -6.0 dBFS |
| 03 | `AUD-PLY-003` Binder pickup | 1101 | `SRC_ORIG_PLY_Binder_Acquire_01_master_v001.wav` / `dd21c157724b65aefc46a84dbec9d803d16c45c88bc8c6145583321bd27110b4` / 54,764 | `Export/AUD_PLY_Binder_Acquire_01_v001.wav` / `5fbefe0eeaccb7f37b40b22466baea437f8d02c44fab45030c5e88a54425954d` / 54,764 | 0.380 s / 1 / -4.5 dBFS |
| 04 | `AUD-PLY-004` Chair pickup | 1102 | `SRC_ORIG_PLY_Chair_Acquire_01_master_v001.wav` / `8820e6445ff58aa1f4c94e9687e3551bcb90969104fd02263dbb28d143c8d0ac` / 69,164 | `Export/AUD_PLY_Chair_Acquire_01_v001.wav` / `805f4d2541ffcdec95cbf6ffef66e6709c96bc13adcebe7b71a2d323d5031529` / 69,164 | 0.480 s / 1 / -4.0 dBFS |
| 05 | `AUD-PLY-010` Binder throw release | 1201 | `SRC_ORIG_PLY_Binder_ThrowRelease_01_master_v001.wav` / `a49586ae5b7b473eeded365a5bc740a58a51c60f3a5398ebb1c42331ecca10a2` / 80,684 | `Export/AUD_PLY_Binder_ThrowRelease_01_v001.wav` / `9069ef541786d3a31917c96d3ace4500e804e77fb4dff7714041fb647a3e6757` / 80,684 | 0.560 s / 1 / -3.5 dBFS |
| 06 | `AUD-BND-002` Binder impact 01 | 1301 | `SRC_ORIG_BND_Impact_OfficeSurface_01_master_v001.wav` / `6ec50db70b97fc02112360c4392ef994f1cccbe657d02dc94e3664699d9b003e` / 57,644 | `Export/AUD_BND_Impact_OfficeSurface_01_v001.wav` / `2cab43f48ac9b5d738e6ebfc0db0553adffc808a611d1bac2b3c50fa68fc5665` / 57,644 | 0.400 s / 1 / -3.0 dBFS |
| 07 | `AUD-BND-002` Binder impact 02 | 1302 | `SRC_ORIG_BND_Impact_OfficeSurface_02_master_v001.wav` / `04014fd6f291cdffb64b2f913d90902119e9a0af869a48d1d9b9f0f86fcd8557` / 57,644 | `Export/AUD_BND_Impact_OfficeSurface_02_v001.wav` / `d1fc1459fff61cafd011aa4733d423e985508bec896bd7bdb897e0096a27e6d8` / 57,644 | 0.400 s / 1 / -3.0 dBFS |
| 08 | `AUD-BND-002` Binder impact 03 | 1303 | `SRC_ORIG_BND_Impact_OfficeSurface_03_master_v001.wav` / `bf0d34ef8182a0f1847a0bb7dc21ea4112cf996686a40321bec0db7a9a11a3ed` / 57,644 | `Export/AUD_BND_Impact_OfficeSurface_03_v001.wav` / `83593c488d6e18e904957c86797cb25b0395523d0ecd096e6f95df8e5dd01748` / 57,644 | 0.400 s / 1 / -3.0 dBFS |
| 09 | `AUD-CHR-004` Chair impact 01 | 1401 | `SRC_ORIG_CHR_Impact_StaticOffice_01_master_v001.wav` / `26c36381e641320935ccbcf2cbae09bce31db6bf35161ff30031947e29267c1d` / 89,324 | `Export/AUD_CHR_Impact_StaticOffice_01_v001.wav` / `323893d80c2b382e75dbc0cd865bdd8598ea7ba78789b2814d9af4deef5e1bff` / 89,324 | 0.620 s / 1 / -2.5 dBFS |
| 10 | `AUD-CHR-004` Chair impact 02 | 1402 | `SRC_ORIG_CHR_Impact_StaticOffice_02_master_v001.wav` / `3e5faea794d7003a49fe55c857f83c7ab08ae009daf4d4ce0252303bdf654715` / 89,324 | `Export/AUD_CHR_Impact_StaticOffice_02_v001.wav` / `13e38459c7d6796817319a5505998d930925ae223955f0f28254a8ba34a14895` / 89,324 | 0.620 s / 1 / -2.5 dBFS |
| 11 | `AUD-CHR-004` Chair impact 03 | 1403 | `SRC_ORIG_CHR_Impact_StaticOffice_03_master_v001.wav` / `4d69159b7364f3c63b532610848a8dfb24e0d586c6137fe1b86f56455bf6e9d1` / 89,324 | `Export/AUD_CHR_Impact_StaticOffice_03_v001.wav` / `c5a29c85249e876abcfdacb0cba1c9324d0a4728b4660406dc3d09d45bd76dc0` / 89,324 | 0.620 s / 1 / -2.5 dBFS |
| 12 | `AUD-CHR-001` Chair caster loop | 1501 | `SRC_ORIG_CHR_CasterRoll_Loop_master_v001.wav` / `a8ced4f9269115b7635f3f202d4ea30b06dfc3d669d972d6bb76804608600c8e` / 360,044 | `Export/AUD_CHR_CasterRoll_Loop_v001.wav` / `fe5b806518180a1f6720f2fb048cae35acc9f2307965695da18a49ef1c62e393` / 360,044 | 2.500 s / 1 / -10.0 dBFS |
| 13 | `AUD-ALY-001` Ally READY | 1601 | `SRC_ORIG_ALY_Ready_01_master_v001.wav` / `0785603ce1d2d489be0b0e355c62095e359627bbc93730921f830c139fa5280a` / 74,924 | `Export/AUD_ALY_Ready_01_v001.wav` / `683ad3afbd15aa1a398535341714ca83b3a889c52097450b36d4ba767369a815` / 74,924 | 0.520 s / 1 / -5.0 dBFS |
| 14 | `AUD-ALY-003` Ally supply release | 1602 | `SRC_ORIG_ALY_BinderSupply_Release_01_master_v001.wav` / `02f3fca0991aef43637b7fcf8538267bed7fe82d702fc38e95c13d009bde9dd1` / 83,564 | `Export/AUD_ALY_BinderSupply_Release_01_v001.wav` / `41d6ce197c4f5263c4c0f3f4dcc40c5105cc0b8ebc09f6c1397b99820d6089f4` / 83,564 | 0.580 s / 1 / -4.0 dBFS |
| 15 | `AUD-BND-005` Catch confirm | 1701 | `SRC_ORIG_BND_Catch_Secure_01_master_v001.wav` / `b0e66662f950502c5bb46206f0bab3959f77247330b26313e1e2adc2c9567ace` / 72,044 | `Export/AUD_BND_Catch_Secure_01_v001.wav` / `27f692ac46bb9aaf989a0448a28fcd36ebc07370385e0dfd408256d082b88ccc` / 72,044 | 0.500 s / 1 / -4.0 dBFS |
| 16 | `AUD-PLY-010` Charged throw release | 1801 | `SRC_ORIG_PLY_ThrowCharge_Release_master_v001.wav` / `2a92ec52e85cdd9f67ba9fabd974e37990a87184d98802b1e51e32fcd699b037` / 112,364 | `Export/AUD_PLY_ThrowCharge_Release_v001.wav` / `f03a35bf0390bd2cc72ef18d8ad4f94d954b3cc0b68e55eb570caa7ddc01ec37` / 112,364 | 0.780 s / 1 / -2.5 dBFS |
| 17 | `AUD-RST-001` Reset | 1901 | `SRC_ORIG_RST_Reset_Accept_master_v001.wav` / `db56a8bb3b634658867cc0f69d30c9c2be22da2046463b14f2ea6efea8fd7606` / 100,844 | `Export/AUD_RST_Reset_Accept_v001.wav` / `693c6a41714149d3b73de3aacfee0694d59543b6473825e02b727de7460bbae3` / 100,844 | 0.700 s / 1 / -4.0 dBFS |
| 18 | `AUD-PLY-001` Footstep 01 | 2001 | `SRC_ORIG_PLY_Footstep_OfficeFloor_01_master_v001.wav` / `558c321426d3ce7a85389ed2f255ef78ac47f18ea44844fa30f07bceb0f9fa1a` / 49,004 | `Export/AUD_PLY_Footstep_OfficeFloor_01_v001.wav` / `a1d844aa5df093aed178a6ddb3cb2b63f0bfdb7d3a57e4c3932d99f64a3c571c` / 49,004 | 0.340 s / 1 / -4.5 dBFS |
| 19 | `AUD-PLY-001` Footstep 02 | 2002 | `SRC_ORIG_PLY_Footstep_OfficeFloor_02_master_v001.wav` / `e2fecfceec035b3e79edf6a2faa3f0a8b60192fa75e35c7b6fba6011c3044d47` / 49,004 | `Export/AUD_PLY_Footstep_OfficeFloor_02_v001.wav` / `971358e3ca566fb9a710743ee61b9a5f9bf8b06fef39de02162f4283b2ba6bd2` / 49,004 | 0.340 s / 1 / -4.5 dBFS |
| 20 | `AUD-PLY-001` Footstep 03 | 2003 | `SRC_ORIG_PLY_Footstep_OfficeFloor_03_master_v001.wav` / `c1900e49e2e328cf1819824179bcafbc5f949f99d40f5a2c7d6923149a8a8b54` / 49,004 | `Export/AUD_PLY_Footstep_OfficeFloor_03_v001.wav` / `8888ffc1ecc2d0a109281ab4852d022b4053ff382f9d52ad4ae4b95ecdf87d1e` / 49,004 | 0.340 s / 1 / -4.5 dBFS |
| 21 | `AUD-PLY-001` Footstep 04 | 2004 | `SRC_ORIG_PLY_Footstep_OfficeFloor_04_master_v001.wav` / `efb9643d4edd76ad89b050b1c5f28fe29f098c32eb4837b15f9024699eef1265` / 49,004 | `Export/AUD_PLY_Footstep_OfficeFloor_04_v001.wav` / `dd9ee8a0065358b6ecd95929ee7e08ba995e930fd191aa28a631973e17a58cee` / 49,004 | 0.340 s / 1 / -4.5 dBFS |
| 22 | `AUD-AMB-001` HVAC room tone loop | 2101 | `SRC_ORIG_AMB_OperationsFloor_RoomTone_Loop_master_v001.wav` / `9692471d972d9eea1a726a04d10de90400b11bf1a67a0310726732b4886839dc` / 1,728,044 | `Export/AUD_AMB_OperationsFloor_RoomTone_Loop_v001.wav` / `62aa0e21ef89d2849962defcd20d1f2b1ccdcc8b3ae7ceeec12f664fe707ce33` / 1,728,044 | 6.000 s / 2 / -15.0 dBFS |
| 23 | `AUD-EXT-001` Exit activation | 2201 | `SRC_ORIG_EXT_Exit_Activated_master_v001.wav` / `23762e4de1a8a66eb8736bc88058d1acedf9e9f3d15906b1b558cef0f14234cd` / 270,764 | `Export/AUD_EXT_Exit_Activated_v001.wav` / `1fab8f0e64b8c325ad5b96d88465585d422af7b0ba093db0b447959d0c0f43c6` / 270,764 | 0.940 s / 2 / -5.0 dBFS |

## Preview Evidence

| File | SHA-256 | Bytes | Technical note |
|---|---|---:|---|
| `Preview/AUD_Showcase_ProductionSourcePack_v001_Audition.wav` | `ba16a72bb0b17ee84038fb8d24f3aa7f5e4b391b5dbfab3fd7251ea34c6af3fb` | 8,665,964 | 30.090 s, stereo, 48 kHz, 24-bit, -2.5 dBFS |
| `Preview/AUD_Showcase_ProductionSourcePack_v001_Waveforms.png` | `5d47e04bee7549e4178720059b643bfb33b7c55267684e8037ed839274b2320f` | 615,643 | PNG, 2136 × 2613, RGBA |

Human Owner audible decision: `PENDING`

Runtime integration decision: `NOT AUTHORIZED / PENDING SEPARATE RELAY`
