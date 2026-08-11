# BREAK NOTICE Showcase VFX Technical Delta v001

- Status: `REVIEW / DOCUMENTATION-ONLY DELTA / NO RUNTIME CHANGE`
- Ticket: `SHOWCASE-VFX-SOURCE-001`
- Relay: `EAR-CONTEST-0002-VFX-SOURCE-001`
- Baseline: `f51315aea91dd00293a4f5c387f1cb748865fd11`
- Runtime Hook Authority: `ABSENT`
- S9 Integration Authority: `NOT AUTHORIZED`

## 1. Delta 요약

현재 Runtime과 `Game/**`는 이 작업에서 읽거나 변경하지 않았다. 따라서 아래
항목은 기존 구현의 class, event, payload 또는 timing을 설명하는 것이 아니다.
잠긴 Gameplay Fact를 Presentation이 소비하기 위해 향후 검토해야 할 **semantic
hook 후보**만 정리한다.

모든 hook 이름, payload, band, rate, duration과 연결 package는
`PROPOSED / REVIEW REQUIRED`다. Presentation은 Gameplay state를 mutate하지
않고, 검증된 Gameplay/Physical/Domain Fact 이후에만 실행해야 한다.

## 2. Future semantic hook 후보

| Hook candidate | 최소 의미 | VFX consumer | 상태/책임 경계 |
|---|---|---|---|
| `EligibilityPresentationChanged` | Binder/Chair의 현재 획득 가능 여부와 rejection reason | acquisition brackets | `PROPOSED / REVIEW REQUIRED`; target selection을 변경하지 않음 |
| `AcquisitionCommittedPresentation` | 실제 ownership commit 완료 | acquire confirm | `PROPOSED / REVIEW REQUIRED`; pickup을 실행하지 않음 |
| `ThrowChargeBandChanged` | charge의 normalized visual band | charge ticks | `PROPOSED / REVIEW REQUIRED`; charge 계산·1.2초 tuning을 소유하지 않음 |
| `ThrowPhysicalReleaseConfirmed` | held object가 실제 release 됨 | release snap | `PROPOSED / REVIEW REQUIRED`; Rigidbody release 이후 |
| `PhysicalContactPresentationResolved` | contact 위치·normal·material role·impulse band | paper/dust/wheel/contact cue | `PROPOSED / REVIEW REQUIRED`; damage/break/causal 판정 금지 |
| `AllyReadyPresentationChanged` | READY 느낌표 표시 가능 state | READY cue | `PROPOSED / REVIEW REQUIRED`; Ally decision을 만들지 않음 |
| `SupplyRequestPresentationResolved` | Binder/Chair supply accepted 또는 rejected와 reason | Q commit/reject cue | `PROPOSED / REVIEW REQUIRED`; Q arbitration 이후 |
| `BinderCatchPresentationResolved` | catch success 또는 miss 확정 | catch cue | `PROPOSED / REVIEW REQUIRED`; ownership 전환 이후 |
| `ChairFeedPhysicalReleaseConfirmed` | Ally hand release와 floor support 확정 | grounded feed cue | `PROPOSED / REVIEW REQUIRED`; Chair trajectory를 만들지 않음 |
| `WheelFloorContactPresentationSample` | 유효 wheel-floor contact와 진행 방향 | wheel puffs/skid | `PROPOSED / REVIEW REQUIRED`; bounded rate limit 필요 |
| `TeamBadgePresentationAwarded` | PASS/BRACE/BREAK 중 하나의 새 badge Fact | badge stamp | `PROPOSED / REVIEW REQUIRED`; duplicate award 억제는 Gameplay 소유 |
| `TeamReadyPresentationChanged` | 서로 다른 badge 두 개로 TEAM READY 확정 | paired bracket cue | `PROPOSED / REVIEW REQUIRED`; resource/spend state 금지 |
| `FinisherPresentationPhaseChanged` | HANDOVER CRASH의 검증된 phase 경계 | paired/contact cues | `PROPOSED / REVIEW REQUIRED`; sequence와 camera를 구동하지 않음 |
| `DrawerCausalContactPresentationResolved` | 귀속 가능한 Runner/Chair→Drawer contact Fact | Drawer impact cue | `PROPOSED / REVIEW REQUIRED`; Drawer 이동·causal lineage 판정 금지 |
| `DrawerRouteStatePresentationChanged` | route가 실제 `OPEN`으로 변함 | route chevron | `PROPOSED / REVIEW REQUIRED`; state 선행 표시 금지 |
| `RunnerPressurePresentationChanged` | telegraph/commit/contact/stagger/grab-readability state | Runner cues | `PROPOSED / REVIEW REQUIRED`; AI/attack token/damage 변경 금지 |
| `ResetBarrierPresentationChanged` | Reset start와 authored restore complete | reset clear/complete cue | `PROPOSED / REVIEW REQUIRED`; exclusive barrier 내부 mutation 금지 |
| `ExitPresentationChanged` | Exit가 실제 active/inactive로 변함 | fixture mask/direction cue | `PROPOSED / REVIEW REQUIRED`; objective completion 판정 금지 |
| `UiActionFeedbackResolved` | action confirmation/rejection와 readable reason | UI snap/X cue | `PROPOSED / REVIEW REQUIRED`; 입력 소비·arbitration 변경 금지 |

후속 Runtime 검토 시 실제 code symbol을 새로 만들기 전에 현재 implementation의
기존 signal을 재사용할 수 있는지 먼저 감사해야 한다. 이 문서는 새 event bus,
manager, package 또는 architecture를 요구하지 않는다.

## 3. 제안 payload 최소화

다음 정보만 후보로 검토한다. 모두 `PROPOSED / REVIEW REQUIRED`다.

- stable presentation cue ID
- source/target runtime identity의 read-only reference 또는 presentation handle
- world contact position과 contact normal
- material role: `PAPER`, `WHEEL_FLOOR`, `OFFICE_HARD_SURFACE`, `UI_ONLY`
- semantic intensity band: `LOW`, `MEDIUM`, `HIGH`; raw force나 damage 수치 노출 금지
- result: `CONFIRMED`, `REJECTED`, `MISSED`, `RESET`
- rejection reason의 localized presentation key
- reset generation 또는 equivalent stale-cue guard

VFX payload에 damage, break health, hidden inventory, new causal reward, new object
state 또는 mutable Gameplay authority를 추가하지 않는다.

## 4. Cue lifecycle 후보

| 단계 | 제안 동작 | 경계 |
|---|---|---|
| Request | Gameplay/Physical Fact가 presentation request를 발행 | `PROPOSED / REVIEW REQUIRED` |
| Validate | cue ID, current reset generation, visible target와 occlusion 확인 | `PROPOSED / REVIEW REQUIRED` |
| Acquire | 사전 할당된 presentation instance를 확보 | `PROPOSED / REVIEW REQUIRED`; runtime allocation 정책 미확정 |
| Play | attach-space와 contact normal에 맞춰 짧게 재생 | `PROPOSED / REVIEW REQUIRED` |
| Release | duration 종료, state invalidation 또는 Reset start에서 반환 | `PROPOSED / REVIEW REQUIRED` |

Reset start에서는 charge, eligibility, wheel-loop, READY와 pending set-piece cue를
즉시 무효화하고, restore complete 뒤 현재 authoritative state로 다시 표현해야
한다. 정확한 barrier 연결은 Runtime Relay가 필요하다.

## 5. Performance class 후보

아래 수치는 Source 제작과 Preview 비교를 위한 상한 후보일 뿐이며 WebGL
실측 전 승인되지 않는다.

| Class | 용도 | Source target | 상태 |
|---|---|---|---|
| `P0_UI` | reticle, badge, confirm/reject, READY 보조 | particle 없음; 단일 atlas draw 후보; 지속 state는 event 반복 금지 | `PROPOSED / REVIEW REQUIRED` |
| `P0_MICRO` | release snap, catch, 작은 tag contact | emitter 1개 이하, 동시 visible quad 8개 이하, 0.35초 이하 후보 | `PROPOSED / REVIEW REQUIRED` |
| `P1_CONTACT` | Binder/Chair/Drawer hard contact와 wheel burst | emitter 2개 이하, visible quad 16개 이하, 0.60초 이하 후보 | `PROPOSED / REVIEW REQUIRED` |
| `P2_SETPIECE` | HANDOVER CRASH의 국소 contact relay | 국소 emitter 3개 이하, visible quad 32개 이하, 마지막 contact 후 1.0초 이하 후보 | `PROPOSED / REVIEW REQUIRED` |

모든 visual element의 Gameplay collision과 causal-carrier 역할은 `0`이다. active
effect 중 runtime fracture, dynamic gameplay debris와 신규 Instantiate/Destroy
정책을 이 문서로 승인하지 않는다.

## 6. Texture·Flipbook Source 후보

| Candidate | 제안 포맷 | 용도 | 상태 |
|---|---|---|---|
| `T_VFX_Showcase_IndustrialCueShapes_Atlas_v001.png` | RGBA atlas, alpha blend | bracket/tick/stamp/chevron | `MISSING / REVIEW REQUIRED` |
| `T_VFX_Showcase_PaperBits_Atlas_v001.png` | RGBA atlas, alpha blend | Binder contact paper | `MISSING / REVIEW REQUIRED` |
| `T_VFX_Showcase_ContactDust_4x4_v001.png` | 4×4 flipbook, alpha blend | office hard-surface dust | `MISSING / REVIEW REQUIRED` |
| `T_VFX_Showcase_WheelContact_4x4_v001.png` | 4×4 flipbook, alpha blend | floor-supported wheel contact | `MISSING / REVIEW REQUIRED` |
| `T_VFX_Showcase_ImpactMarks_Atlas_v001.png` | RGBA atlas | brief non-persistent contact accent | `MISSING / REVIEW REQUIRED` |
| `T_VFX_Showcase_ExitLightMasks_v001.png` | grayscale/RGBA mask | Exit fixture/sign activation | `MISSING / REVIEW REQUIRED` |
| `T_VFX_Showcase_UIConfirmReject_Atlas_v001.png` | RGBA atlas | UI feedback | `MISSING / REVIEW REQUIRED` |

정확한 resolution, compression, mipmap, color space, blend mode와 import setting은
S9 이전 Technical Review에서 정한다. Additive-only neon, custom Shader Graph,
VFX Graph와 screen-space distortion을 전제로 제작하지 않는다.

## 7. Occlusion과 screen coverage 후보

- contact 핵심 silhouette의 약 `8%` 이상을 가리는 효과는 revision 후보다.
- 중앙 reticle 주변 필수 aiming 영역은 charge tick 외 particle-free로 둔다.
- world cue는 solid occluder 뒤에서 표시하지 않는다.
- near camera `0.5 m` 안쪽 visual element는 fade/cull 후보로 둔다.
- long smoke tail, volumetric fog, lens dirt와 full-screen color flash는 `0`이다.
- 위 수치와 거리도 `PROPOSED / REVIEW REQUIRED`이며 gameplay-camera Preview와
  1366×768/1920×1080 Web test 없이는 승인하지 않는다.

## 8. License와 provenance delta

현재 실제 Source가 없으므로 creator, exact license, source URL, source hash와
attribution evidence는 모두 `MISSING`이다. 외부 open-source 또는 상업 이용
가능 Source는 후속 Relay에서 다음을 모두 만족할 때만 후보가 된다.

1. 원 저작자와 canonical source URL 식별
2. 다운로드 시점의 exact license text 보존
3. commercial use, derivative work, redistribution와 attribution 확인
4. 원본 파일 SHA-256 기록
5. raw source를 Git에 넣을 수 있는지 별도 확인
6. 제출 build와 marketing capture에 사용할 수 있는지 확인
7. Human Owner Technical/rights review

권리가 불명확하면 `BLOCKED — PROVENANCE OR LICENSE INCOMPLETE`로 중단한다.

## 9. 필요한 후속 권한

1. `SOURCE/PREVIEW CREATION`: 위 후보 Source, Texture, Flipbook, License,
   Preview의 exact allowlist를 가진 새 one-shot Relay
2. `HUMAN OWNER REVIEW`: grayscale, gameplay-camera, contact/occlusion Preview
3. `S9 RUNTIME INTEGRATION`: 승인된 source만 Unity로 import하고 actual hooks,
   pooling, renderer, import settings와 Web performance를 검증하는 별도 Relay

이 세 단계는 서로 권한을 상속하지 않는다. 현재 Relay는 1단계의 문서 준비만
완료하며 실제 Source 생성도 승인하지 않는다.
