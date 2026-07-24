# G3 Technical Baseline Preflight Audit and Decision Package

> **DESIGN AUTHORITY PREFLIGHT — READY FOR OWNER G3 TECHNICAL DECISIONS**

감사 기준선: `3bf04d7a86601f07ccbd65f635b1f9a05aaaa5df`

이 문서는 현재 Technical Draft와 비어 있는 Unity 예정 구조를
Owner-approved G1/G2 기준선에 대조한 G3 사전 감사다. G1과 G2는
`PASSED`지만 G3는 `NOT PASSED`다. 이 감사로 승인된 기술 결정은 없고,
정확한 Unity Editor 또는 패키지 버전도 선택하지 않았다.

이 문서는 Unity, package, C#, scene, prefab, test, build, Graybox 또는
구현 작업을 허가하지 않는다. Design Freeze는 `22 / 32`, G3 Technical
checklist는 `0 / 6`, Full Design Lock은 `INCOMPLETE`, Graybox는
`NOT STARTED`, Implementation Gate는 `CLOSED`다.

## Baseline and Repository Inventory

| 항목 | 확인 결과 |
|---|---|
| Branch | `main` |
| Local HEAD / `origin/main` | `3bf04d7a86601f07ccbd65f635b1f9a05aaaa5df` / 동일 |
| Ahead / Behind | 작업 전 `0 / 0` |
| Tracked / Staged | 작업 전 `CLEAN / CLEAN` |
| Current Phase | `PRE-PRODUCTION / G3 TECHNICAL BASELINE` |
| G1 Requirements | `PASSED` |
| G2 Design | `PASSED / OWNER-APPROVED BASELINE` |
| G2 Gameplay | `7 / 7` |
| G3 Technical | `NOT PASSED / CURRENT BASELINE WORK` |
| Design Freeze / G3 Technical | `22 / 32` / `0 / 6` |
| Full Design Lock / Graybox | `INCOMPLETE` / `NOT STARTED` |
| Implementation Gate | `CLOSED` |

기존 `references/exploration/*_REVIEW.png` 22개는 사전 존재 untracked
Owner 자료로 확인했으며 읽기·수정·이동·삭제·stage하지 않았다.

### Repository Technical Inventory

| 검사 대상 | 현재 저장소 상태 | 감사 판단 |
|---|---|---|
| `Game/**` tracked inventory | `Game/README.md`와 `.gitkeep` 25개뿐 | Unity 프로젝트가 아닌 빈 예정 구조 |
| `Game/ProjectSettings/ProjectVersion.txt` | 없음 | exact Editor lock 미존재 |
| `Game/Packages/manifest.json` | 없음 | package selection 미존재 |
| `Game/Packages/packages-lock.json` | 없음 | package version lock 미존재 |
| C# (`*.cs`) | 0 | 구현 없음 |
| Assembly Definition (`*.asmdef`) | 0 | 구현 구조 없음 |
| Scene (`*.unity`) | 0 | Bootstrap/Prototype scene 없음 |
| Prefab (`*.prefab`) | 0 | prefab 없음 |
| Test configuration / test code | 0 | harness와 test 구현 없음 |
| CI / build configuration | `.github`에는 issue/PR template만 존재 | CI, workflow와 build pipeline 없음 |
| Root `Packages/**`, `ProjectSettings/**` | 없음 | 중복 Unity root 없음 |

누락 항목은 이 감사에서 만들지 않았다. `Game/README.md`의 Bootstrap
목록은 과거 후보이며 G3 승인 또는 구현 권한이 아니다.

### Current Technical Assumption Inventory

| 영역 | 현재 Draft 가정 |
|---|---|
| Engine / rendering | Unity 6, URP, GameObject 기반 Built-in 3D PhysX, C# |
| Input / camera | Unity Input System, Cinemachine |
| AI / navigation | Unity AI Navigation/NavMesh와 제한된 Relay Position 이동 |
| Data | ScriptableObject definition + runtime instance state |
| Testing | Unity Test Framework, EditMode/PlayMode |
| Target | Windows PC; 1080p/60 FPS 후보 |
| Architecture | Presentation, Gameplay, Simulation, Progression, Infrastructure |
| Events | Impact, Pass, Catch, Break, Momentum 후보 payload |
| Physics | bounded activation, selective CCD, primitive collider, normalized impact |
| Reset / save | Prototype save 없음; snapshot 또는 regeneration; Slice save 후보 |
| Debug | interaction, Rigidbody, Chain, Momentum, Relay, budget, Reset 관찰 후보 |
| Order | Physics→Stability→Chain→Momentum→Relay→Strap→Breakable 순의 기존 Draft |

## Authority and Technical-Draft Classification

여섯 Technical 문서는 전부 `DRAFT`다. 반복 기재는 승인 근거가 아니다.
아래 각 행은 정확히 하나의 통제 분류만 사용한다.

| Source / statement | Classification | 근거와 처분 |
|---|---|---|
| `TECHNICAL_DESIGN.md` — Unity 6, URP, GameObject PhysX, C# 방향 | `ALIGNED G3 CANDIDATE` | 승인된 상위 방향과 일치하지만 exact revision과 project settings는 미승인 |
| 동일 — Editor patch와 package version은 Bootstrap에서 고정 | `OUTDATED / CONFLICTING` | 구현 Bootstrap 전에 G3에서 먼저 고정해야 함 |
| 동일 — Input System, Cinemachine | `MISSING G3 DECISION` | Prototype package 최소선과 버전을 결정해야 함 |
| 동일 — AI Navigation | `MISSING G3 DECISION` | Prototype ally에는 불필요하며 later Slice Relay Position 후보로만 package timing을 정해야 함 |
| 동일 — ScriptableObject definition/runtime state 분리 | `ALIGNED G3 CANDIDATE` | 방향은 유효하나 concrete schema는 미승인 |
| 동일 — Unity Test Framework | `MISSING G3 DECISION` | harness layer와 package lock이 필요 |
| 동일 — Windows PC | `ALIGNED G3 CANDIDATE` | G1 baseline platform과 일치 |
| 동일 — 1080p/60 FPS | `MISSING G3 DECISION` | G3 budget proposal과 G6 target-build acceptance 전 측정 후보이며 G1/G2 승인값이 아님 |
| 동일 — 5-layer architecture | `ALIGNED G3 CANDIDATE` | dependency direction을 먼저 승인해야 함 |
| 동일 — 열여섯 candidate class | `PREMATURE IMPLEMENTATION DETAIL` | 책임은 감사 가능하나 이름·수·경계는 provisional |
| 동일 — bounded dynamics, selective CCD, primitive collider | `ALIGNED G3 CANDIDATE` | exact mode와 threshold는 후속 profiling |
| 동일 — prefab-swap breakage와 small-piece VFX | `LEGITIMATE G5 PRODUCTION DEFERRAL` | Prototype에는 required breakable이 없음 |
| 동일 — velocity/angular safety cap | `MISSING G3 DECISION` | policy는 필요하지만 값은 G4 |
| 동일 — debug displays/counters | `ALIGNED G3 CANDIDATE` | causal observability와 fault recovery에 필요 |
| 동일 — Stability/Momentum/Breakable/Upgrade/Incident를 초기 순서에 포함 | `OUTDATED / CONFLICTING` | approved Prototype scope와 불일치 |
| `SOFTWARE_ARCHITECTURE.md` — simulation이 presentation을 참조하지 않음 | `ALIGNED G3 CANDIDATE` | 단방향 dependency의 유효한 출발점 |
| 동일 — `PlayerMotor`가 Evade/Vault 소유 | `OUTDATED / CONFLICTING` | 두 action은 inactive Draft |
| 동일 — 고정 `Catch > Strap Anchor > Scoop` | `OUTDATED / CONFLICTING` | G2 arbitration/context contract와 불일치 |
| 동일 — Quick/Charged/Queued Throw | `OUTDATED / CONFLICTING` | Quick/Charged는 승인된 active mode가 아님 |
| 동일 — Strap automatic safety release | `OUTDATED / CONFLICTING` | explicit `StrapRelease`만 승인, automatic rule은 미승인 |
| 동일 — Ally generic inventory | `OUTDATED / CONFLICTING` | room-local physical pool와 reservation contract를 누락 |
| 동일 — Impact normalization과 Causal attribution 분리 | `ALIGNED G3 CANDIDATE` | raw collision의 cross-system mutation을 차단 |
| 동일 — Momentum/Breakable component가 Prototype 책임 목록에 함께 존재 | `PREMATURE IMPLEMENTATION DETAIL` | later/optional system으로 격리 필요 |
| 동일 — anti-pattern 목록 | `ALIGNED G3 CANDIDATE` | static global state, monolith, physics-order dependence를 금지하는 방향 |
| `DATA_AND_EVENTS.md` — stable content IDs | `ALIGNED G3 CANDIDATE` | content ID와 runtime instance ID 분리가 추가로 필요 |
| 동일 — ID namespace가 G2 `PRT-*` reference를 포괄하지 않음 | `MISSING G3 DECISION` | 기존 승인 ID를 보존하는 mapping policy 필요 |
| 동일 — 다섯 event payload가 완결 contract처럼 보임 | `OUTDATED / CONFLICTING` | ownership, Reset, relay lifecycle, Origin/result와 Policy event가 누락 |
| 동일 — `ChainId`, `ChainDepth`만으로 causal identity 표현 | `OUTDATED / CONFLICTING` | Origin, parent/carrier, result uniqueness와 no-merge 경계를 충족하지 못함 |
| 동일 — Prototype persistent save 없음 | `ALIGNED G3 CANDIDATE` | G1/NFR deferred boundary와 일치 |
| `PHYSICS_DESTRUCTION_IMPLEMENTATION.md` — activation/sleep lifecycle | `ALIGNED G3 CANDIDATE` | exact transition 조건은 G4 |
| 동일 — object별 collision mode | `LEGITIMATE G4 TUNING DEFERRAL` | profile 결과 없이 고정 불가 |
| 동일 — exact collider counts | `LEGITIMATE G4 TUNING DEFERRAL` | geometry/profile 후보이며 Canon이 아님 |
| 동일 — Prototype 흐름처럼 서술된 Breakable Flow | `OUTDATED / CONFLICTING` | Prototype breakable/Structural requirement가 없음 |
| 동일 — full rope simulation 배제 | `ALIGNED G3 CANDIDATE` | bounded Cargo Strap 요구에 맞음 |
| 동일 — 과도 장력 automatic release | `OUTDATED / CONFLICTING` | Owner 승인 없는 behavior |
| 동일 — 제한 ragdoll 후보 | `LEGITIMATE G4 TUNING DEFERRAL` | actor reaction은 필요하나 mode·duration·threshold 미승인 |
| `PERFORMANCE_BUDGET.md` — Windows PC target | `ALIGNED G3 CANDIDATE` | baseline platform과 일치 |
| 동일 — resolution/FPS와 runtime 숫자 | `MISSING G3 DECISION` | Prototype/Slice budget 분리와 측정법이 먼저 필요 |
| 동일 — 3~5 enemy 등 content-like budget | `OUTDATED / CONFLICTING` | Prototype exact enemy 2와 혼재하며 content count는 runtime budget이 아님 |
| 동일 — breakable/Structural/fragment budget | `LEGITIMATE G5 PRODUCTION DEFERRAL` | Prototype required scope에 없음 |
| 동일 — CPU/GPU risk 목록 | `ALIGNED G3 CANDIDATE` | profiling counter와 spike policy로 연결 필요 |
| `SAVE_AND_VERSIONING.md` — Prototype save 없음 | `ALIGNED G3 CANDIDATE` | persistent gameplay save와 Reset을 분리 |
| 동일 — snapshot 또는 regeneration | `MISSING G3 DECISION` | authoritative reset path와 fallback/ordering 미정 |
| 동일 — Slice settings/run save 후보 | `LEGITIMATE G5 PRODUCTION DEFERRAL` | persistent-save scope는 conditional/deferred |
| 동일 — full Rigidbody-world persistence 금지 | `ALIGNED G3 CANDIDATE` | bounded solo scope와 일치 |
| 동일 — independent save schema version | `ALIGNED G3 CANDIDATE` | save가 승인될 경우에만 적용 |
| `Game`에 ProjectVersion/package lock이 없음 | `G3 BLOCKER` | G3 passage와 Bootstrap 전에 exact lock 필요; 현재 감사 자체는 가능 |

### Six Technical Checklist Items

| Checklist item | 현재 감사 결과 |
|---|---|
| Unity 정확한 버전과 패키지 잠금 | 선택 기준과 결정 질문 준비; 아직 `0` |
| 아키텍처 검토 | stale 책임과 권장 dependency map 준비; 아직 `0` |
| Event Contract 검토 | missing lifecycle/order/idempotency 결정 준비; 아직 `0` |
| 성능 예산 검토 | Prototype/Slice/측정 Gate 분리 결정 준비; 아직 `0` |
| Reset 전략 검토 | hybrid 후보와 restore scope 결정 준비; 아직 `0` |
| Test harness 계획 | layer/observability 결정 준비; 아직 `0` |

어느 checkbox도 이 감사에서 체크하지 않았다.

### Eight G2 Contract Technical Boundaries

| Owner-approved G2 contract | Audited G3 boundary |
|---|---|
| `LOGICAL_INPUT_CONTRACT.md` | device adapter, context routing와 request observability가 필요; binding과 timing은 미승인 |
| `INTERACTION_STATE_CONFLICT_CONTRACT.md` | single state owner, arbitration order와 safe-rejection result가 필요; storage/API는 미승인 |
| `WEIGHT_CLASS_DATA_RANGE_CONTRACT.md` | definition eligibility와 runtime classification owner가 필요; mass/force/speed는 G4 Draft |
| `MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md` | later resource/event owner와 one-time conversion이 필요; Prototype에는 system 자체가 없음 |
| `CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md` | Origin/carrier/result identity, ordering, duplicate/cycle/termination mapping이 필요 |
| `ALLY_RELAY_SUPPRESSED_CONTRACT.md` | room-local pool, reservation/abort/release owner가 필요; navigation과 Suppressed는 later |
| `PROTOTYPE_CONTENT_LOCK.md` | exact instance identity, authored Reset baseline와 measurement scenario가 필요 |
| `POLICY_VIOLATION_SELECTION_RUN_APPLICATION_CONTRACT.md` | later run-state/event owner가 필요; Prototype 제외, actual Candidate content는 G5 Draft |

여덟 계약의 wording, 수치, Applicability와 Authority는 변경하지 않았다.
이 표는 승인된 behavior를 G3 decision question에 연결할 뿐이다.

## Unity Editor and Package Preflight

| Candidate | Prototype Core | Later / optional | 감사 결론 |
|---|---|---|---|
| Unity 6 exact revision | Bootstrap 전 필수 lock | 동일 revision 정책 필요 | 현 저장소 근거로 exact patch 선택 불가 |
| URP | 상위 방향상 Prototype renderer 후보 | LGT_01은 promoted될 때 별도 설정 | exact package/settings 미승인 |
| GameObject + Built-in 3D PhysX | bounded physical Prototype 후보 | Slice 확장 가능 | DOTS/Entities 필요 근거 없음 |
| C# | Unity runtime 후보 | 동일 | language/version 세부는 Editor lock에 종속 |
| Input System | active logical action adapter 후보 | later action map 확장 | exact version/binding 미승인 |
| Cinemachine | third-person camera adapter 후보 | later camera state 확장 | exact version/rig/settings 미승인 |
| AI Navigation | 고정 ally position 때문에 불필요 | Relay Position 전환 시 후보 | Prototype package set에서 제외 권고 |
| ScriptableObject | package가 아닌 definition-data 후보 | Slice definition 확장 | schema나 runtime state 저장은 미승인 |
| Unity Test Framework | EditMode/PlayMode harness 후보 | regression 확장 | exact version/fixture 미승인 |

현재 요구사항은 Addressables, DOTS/Entities, Netcode, third-party rope,
third-party behavior tree, third-party save framework 또는 generic
service-locator framework를 정당화하지 않는다. 모두
`OUT / NOT APPLICABLE`이며 별도 Change Control 없이는 package baseline에
포함하지 않는다.

## Prototype and Vertical Slice Technical Scope

### Prototype Required

- one-room Bootstrap/encounter lifecycle와 deterministic Reset;
- Move/Look/Aim 및 승인된 Prototype logical action adapter;
- player locomotion과 PGT_01 가독성 방향의 third-person camera;
- context/state arbitration과 safe rejection;
- stable definition reference, transient runtime instance와 unique ownership;
- direct Light/Medium acquisition, Carry, Throw와 Queued Throw;
- CHR_01 한 명의 fixed-position room-local physical supply,
  reservation, release와 Normal Catch;
- floor-supported eligible Medium의 explicit Cargo Strap attach/redirect/release;
- normalized impact, Causal Origin/carrier/result attribution,
  duplicate/cycle suppression과 depth-1 validation;
- non-Structural local room-state change;
- feedback adapter와 required observability/fault counters.

### Vertical Slice Required Later

- Perfect Catch;
- Momentum/Pip와 Heavy spend lifecycle;
- HeavyAction;
- Relay Position transition와 그때 필요한 navigation 후보;
- Policy Violation selection/pending/active/clear lifecycle;
- Prototype regression을 유지한 additional authored Slice content.

### Optional / Conditional

| Feature | Disposition |
|---|---|
| Suppressed | `VERTICAL SLICE / SHOULD`; Prototype 제외 |
| Behavior Tags | `VERTICAL SLICE / SHOULD / PARTIAL`; 별도 promotion 필요 |
| Structural Feature | `VERTICAL SLICE / SHOULD / PARTIAL`; Heavy와 분리, Prototype 제외 |
| LGT_01 | Records Archive-local `COULD`; promoted될 때만 기술 scope |
| FIN_01 | `OUT / COULD`; visual-only conditional, 구현 scope 아님 |
| Incident Report | `P2 / DEFERRED`; active test와 기술 owner 없음 |
| Breakables | Prototype requirement 없음; Structural/production 결정 뒤 후보 |
| Persistent save | `DEFERRED / conditional`; Prototype Reset과 분리 |

first Prototype implementation wave는 이 later/optional system을
future-proofing 명목으로 포함하지 않는다.

## Architecture and Responsibility Audit

### Proposed Prototype Responsibility Map

| Responsibility | Boundary |
|---|---|
| Composition/configuration | approved definitions와 runtime owner를 연결; service locator 금지 |
| Player motion/camera | movement와 view만 소유; interaction/result를 소유하지 않음 |
| Logical input/context | device input을 G2 action request로 변환; gameplay state를 직접 변경하지 않음 |
| Interaction arbitration | current state/context/eligibility로 한 request를 resolve |
| Ownership registry | Carry, Strap, relay reservation의 단일 owner와 transition 기록 |
| Physical object handling | acquisition, carry, release와 throw commitment/continuity |
| Ally physical supply | fixed support, room-local pool, reservation, abort, release |
| Cargo Strap | valid Anchor, eligible Medium, active constraint/redirect, explicit release |
| Impact normalization | raw collision을 bounded simulation fact로 변환 |
| Causal attribution | Origin, carrier, depth, result uniqueness와 termination |
| Room-state | approved local non-Structural change의 authoritative flag/result |
| Reset orchestration | exclusive barrier, restore ordering, callback/subscription cleanup |
| Presentation adapter | gameplay result를 camera/VFX/audio/UI로 변환; 역방향 mutation 금지 |
| Observability | state, lineage, faults, budgets와 Reset diff를 읽기 전용으로 노출 |

### Later Vertical Slice Responsibility Map

Perfect Catch, Momentum/Pip, HeavyAction, Relay Position/navigation, Policy
Violation을 각각 기존 Prototype boundary에 추가한다. Suppressed,
Behavior Tags, Structural, LGT_01은 promotion된 경우만 추가하며
FIN_01과 Incident Report는 포함하지 않는다.

권장 dependency direction은
`Bootstrap/Adapters → Application Orchestration → Domain State/Rules →
Simulation Facts`이고 Presentation은 result를 관찰하기만 한다. Unity
component, raw collision, UI, camera, audio와 VFX가 다른 domain state를
직접 변경하지 못하게 한다. 작은 프로젝트에 필요한 경계만 두고
범용 DI/service locator/event framework는 만들지 않는다.

첫 Prototype wave에서 만들지 않을 책임은 Stability gameplay,
Breakable/Structural, Momentum/Pip, HeavyAction, Policy, Relay Position
transition, Suppressed, Behavior Tag, persistent save, LGT_01, FIN_01과
Incident Report다.

`GameFlowController`, `EncounterDirector`, `PlayerMotor`,
`PlayerInteractionController`, `CarryController`, `ThrowController`,
`CargoStrapController`, `AllyRelayController`, `CatchController`,
`PhysicsObject`, `ImpactResolver`, `StabilityController`,
`BreakableObject`, `CausalChainService`, `MomentumService`,
`FeedbackDirector`, `ObjectPoolService`는 모두 provisional candidate
name이다. 이 감사는 class name을 승인하지 않는다.

## Definition Data and Runtime State Audit

| Data/state | 권장 소유 방향 | 미결정 경계 |
|---|---|---|
| Stable content reference | 승인된 `PRT-*`, `OBJ-*`, `ENY-*`, `ALLY-*`, `ENC-*`를 변경 없이 참조 | namespace/schema/asset type |
| Runtime entity identity | spawn/restore마다 고유하고 inspectable한 transient identity | concrete format |
| Definition data | shared, read-only-at-runtime 또는 explicit tunable profile | ScriptableObject schema |
| Runtime entity state | 단일 live instance owner | component/POCO layout |
| Encounter state | room lifecycle와 Reset orchestrator | serialization form |
| Carry ownership | 한 object와 한 carrier의 authoritative relation | storage/API |
| Strap ownership | player, object, Anchor와 active phase의 authoritative relation | constraint representation |
| Relay reservation | room-local pool item의 available/reserved/released relation | collection type |
| Causal identity | Origin/carrier/result를 runtime identity에 연결 | concrete key/struct |
| Momentum/Pip | later run/encounter resource owner | Prototype에는 존재하지 않음 |
| Policy state | later run owner: offered/pending/active/cleared | save boundary와 candidate schema |
| Reset baseline | authored encounter baseline + runtime restore plan | snapshot representation |

Shared definition에는 current owner, current Momentum, current velocity,
reservation, active Policy 같은 runtime state를 저장하지 않는다.
Prototype state는 persistent save와 분리한다.

## Event Topology and Ordering Audit

현재 다섯 payload는 참고 후보일 뿐 approved G2 lifecycle을 완전히
표현하지 못한다. final event name과 payload field는 이 감사에서
승인하지 않는다.

| Required lifecycle fact | Proposed technical owner | Ordering/idempotency rule |
|---|---|---|
| Interaction request/resolution | input adapter / interaction arbiter | request ID당 resolution 1회 |
| Carry acquire/release | ownership authority | ownership commit 뒤 physics transition |
| Throw commit/release | object handling | commit과 physical release를 구분 |
| Queued Throw arm/clear | interaction state owner | Catch result와 Reset에서 deterministic clear |
| Relay request/reservation/abort/release | ally relay owner | reservation 선행, pre-release abort만 reclaim |
| Strap attach/tension/explicit/completed release | Strap owner | valid Anchor와 ownership commit 후 active |
| Origin creation/carrier transfer/result | causal owner | normalized physical fact 뒤 ordered mutation |
| Origin termination | causal owner | termination 뒤 reward/propagation 금지 |
| Room-state change | room-state owner | unique result accepted 뒤 적용 |
| Reset request/start/complete | Reset orchestrator | exclusive barrier; start 후 gameplay request 거부 |
| Fault detected/recovered | fault/recovery boundary | repeated report idempotent, recovery auditable |
| Policy selected/pending/active/cleared | later run owner | next-encounter activation과 run-end clear |
| Momentum conversion/Heavy spend | later resource owner | accepted unique result/spend commit 뒤 한 번 |

Raw collision callback은 unrelated gameplay, UI, camera, audio, VFX,
Momentum 또는 Policy를 직접 변경하지 않는다. Presentation callback도
gameplay state를 역으로 변경하지 않는다. dispatcher reentrancy,
duplicate subscription과 physics callback order에 기대지 않는 ordered
application policy가 G3 결정으로 필요하다.

## Causal Identity and Result-Key Audit

| Concern | Required distinction |
|---|---|
| Content identity | authored definition/reference ID |
| Runtime identity | 현재 encounter의 object/actor instance |
| Origin identity | player-owned initiating action/result lineage |
| Initiating carrier | Origin을 만든 initial physical carrier |
| Attributed carrier | 해당 hop/result를 전달한 runtime carrier |
| Parent/depth | branch 관계와 approved maximum depth 추적 |
| Provenance | Direct/Downstream와 Ally/Perfect modifier evidence |
| Result family | meaningful result category |
| Uniqueness | 같은 Origin에서 target runtime identity + result family 중복 억제 |
| Branch sharing | 한 Origin의 branch는 같은 Origin을 공유 |
| No merge | 서로 다른 Origin을 병합하지 않음 |
| Ambiguous multi-Origin | implicit merge 대신 deterministic reject/isolated resolution |
| Termination/Reset | terminal 상태 후 전파 금지; Reset에서 runtime lineage clear |

Runtime object reference만 사용하면 Reset과 observation이 불안정하고,
stable content ID만 사용하면 동일 definition instance들을 구분하지
못한다. stable content reference, transient runtime identity와 compound
result identity를 분리하는 방향이 필요하지만 concrete struct, GUID,
payload와 collection은 미승인이다.

## Physics, Collision and Recovery Audit

- Static/Kinematic/Sleeping에서 bounded Dynamic으로 전환하고 안정 후
  sleep/deactivation하는 방향은 usable candidate다.
- selective CCD와 primitive/compound collider는 usable candidate지만
  object별 mode, count, mass, velocity, force와 threshold는 G4 profiling
  전까지 Draft다.
- ownership transition은 physics operation보다 먼저 commit되고,
  release 이후 실제 경로와 collision continuity가 관찰되어야 한다.
- collision은 normalized fact를 거쳐 causal/room-state logic으로
  전달한다.
- tunneling, NaN/invalid transform, extreme velocity, stuck ownership,
  orphan reservation, duplicate callback과 reset-diff를 detect할 후보가
  필요하다.
- recovery는 object disappearance나 silent teleport가 아니라
  bounded stop/quarantine, safe authored restore 또는 encounter reload로
  기록 가능해야 한다.
- actor reaction은 non-lethal, bounded, recoverable 방향만 사용하고
  uncontrolled ragdoll은 금지한다.
- Prototype에는 breakable, Structural Feature, unrestricted destruction,
  floating, hidden inventory 또는 remote object control을 넣지 않는다.

## Cargo Strap Technical Audit

최소 책임은 approved equipment chain의 연속성, load-bearing metal
Anchor validity, floor-supported eligible Medium, player-owned redirect,
interaction exclusion과 explicit `StrapRelease`다.

| Candidate | 감사 판단 |
|---|---|
| constrained redirect impulse | bounded candidate; exact equation/value 미승인 |
| authored pivot/redirect response | bounded candidate; physical continuity 검증 필요 |
| simplified segmented visual + logical constraint | 권장 방향; full rope 없이 connection readability 지원 |
| general-purpose full rope simulation | 요구 근거 없음; 첫 Prototype에서 배제 |

caster, wheel axis, plastic housing와 fragile protrusion은 Anchor가 아니고,
Heavy towing, grapple, telekinesis, necktie connection은 금지한다.
Strap active 중 Catch/Carry/Throw를 허용하지 않는다. automatic Safety
Release는 Visual state 이름으로 존재해도 gameplay behavior가 승인되지
않았으므로 포함하지 않는다. final solver, distance, angle, tension,
force와 release threshold는 미승인이다.

## Performance and Destruction-Budget Audit

Prototype content facts인 human actor `4`, enemy `2`, interactive object
instance `7`, active ally `1`, required breakable `0`은 runtime budget
승인값이 아니다.

| Budget domain | Prototype G3 decision | Later ownership |
|---|---|---|
| active/visible actors | exact locked content를 실행할 measurement category | Slice content는 G5, target build는 G6 |
| awake dynamic bodies | state별 counter와 spike observation | exact ceiling은 G3 proposal 후 G4 profile |
| collision callbacks | normalized accepted/rejected/fault counter | exact threshold는 G4/G6 |
| live Causal Origins/results | lifecycle/duplicate counter | exact ceiling은 G3/G4 |
| large physical fragments | Prototype required count 0 | Structural/breakable promotion 뒤 G5/G6 |
| small VFX responses | readability와 overdraw counter 후보 | exact budget은 G4/G6 |
| camera/VFX cost | instrumentation category | target threshold는 G4/G6 |
| Reset | duration, allocations, stale-state diff, spike | pass threshold는 G4/G6 |
| target environment | Windows PC | hardware/resolution/FPS exact values는 G3/G6 Draft |

기존 `3~5` enemy, awake body, fragment, Structural과 Chain count 표를
Prototype budget으로 승인하지 않는다. 먼저 Prototype/Slice profile을
분리하고 measurement method, representative scenario와 later acceptance
owner를 결정해야 한다.

## Reset, Save and Versioning Audit

| Candidate | 장점 | 위험 | 감사 판단 |
|---|---|---|---|
| complete scene reload | 단순하고 강한 cleanup | 느린 iteration, bootstrap/service 중복 위험 | fallback 후보 |
| authored baseline snapshot/restore | 빠르고 비교 가능한 Reset | 누락 field/subscription 위험 | primary 후보 |
| deterministic reconstruction | 명확한 factory path | 전체 재생성 경계와 identity 재할당 부담 | 부분 후보 |
| hybrid snapshot restore + scene-reload fallback | 빠른 정상 경로와 안전 fallback | 두 경로의 동등성 검증 필요 | 권장 방향 |

restore scope는 CHR_00, CHR_01, Runner 두 명, exact object pool,
transform/physical state, Carry/Strap, relay reservation/lifecycle, ally,
Origin/Result, room-state, fault/subscription state를 포함해야 한다.
Momentum/Pip와 Policy는 later active scope에서 각 계약대로 복원한다.

Prototype persistent gameplay save는 필수가 아니고 Reset은 account/save
load가 아니다. full Rigidbody-world persistence는 금지한다. settings,
run data와 save schema는 persistent-save scope가 승인될 때만 다룬다.

## Test Harness and Observability Audit

### Proposed Harness Layers

| Layer | Candidate ownership |
|---|---|
| EditMode | logical state/arbitration, ID/state separation, Chain uniqueness/termination, later Momentum arithmetic와 Policy lifecycle, technology-neutral Reset plan |
| PlayMode | Scoop/Carry/Throw continuity, Light/Medium physical relay, Strap, collision, depth-1 causal result, room-state, repeated Reset, fault recovery, camera/route instrumentation |
| Manual / Playtest | physical readability, player agency, camera readability, experimentation, non-telekinetic interpretation |

### Required Observability Candidates

- active logical action/context와 interaction state;
- selected candidate와 safe-rejection reason;
- Carry owner;
- Strap phase, Anchor validity와 explicit release;
- ally state, reservation와 relay phase;
- Origin, parent, carrier, depth, provenance와 Result family;
- duplicate/cycle suppression reason;
- later resource values;
- room-state flags;
- Reset baseline diff와 subscription count;
- physics fault와 recovery counters;
- active-body, collision, causal, fragment/VFX와 frame-spike counters.

현재 test harness, fixture, debug overlay와 CI는 없다. 이 감사는 만들지
않으며 layer ownership과 minimum observability만 Owner 결정으로
준비한다.

## RTM and Acceptance-Test Technical Ownership

### Functional Requirements

| ID | G2 behavior owner | Proposed G3 owner / test layer | Missing decision or later dependency | RTM Technical Source |
|---|---|---|---|---|
| PR-F-001 | Input/Interaction/Weight | arbitration + object ownership / Edit+Play | acquisition query/ownership commit; range G4 | Draft source는 방향만 제공, G3 pending |
| PR-F-002 | Physical Relay | physical object lifecycle / Play | release/collision continuity; force G4 | G3 pending |
| PR-F-003 | Ally/Content Lock | relay reservation/release / Edit+Play | pool/reservation/order; AI timing G4 | G3 pending |
| PR-F-004 | Interaction/Weight/Ally | Catch resolution / Edit+Play | result ordering; timing G4 | G3 pending |
| PR-F-005 | Input/Interaction | Queued Throw state / Edit+Play | arm/clear/release ordering; timing G4 | G3 pending |
| PR-F-006 | Interaction/Weight/GEAR | Strap owner/constraint / Edit+Play | solver/Anchor/release; tuning G4 | G3 pending |
| PR-F-007 | ADR/Visual Canon | equipment-connection validation / Play+Manual | enforcement representation | `G3 PENDING` 정직 |
| PR-F-008 | Weight/Content Lock | definition/runtime eligibility / Edit+Play | data schema/profile; values G4 | `G3 PENDING` 정직 |
| PR-F-009 | Chain Contract | causal identity/result service / Edit+Play | compound key/order/termination | Draft class/field는 stale, G3 pending |
| PR-F-010 | Momentum/Chain/Ally | later resource owner / Edit+Play | not Prototype; tuning G4/G6 | later G3 owner 필요 |
| PR-F-011 | Input/Weight/Momentum | later HeavyAction owner / Edit+Play | not Prototype; execution G3/G4 | later G3 owner 필요 |
| PR-F-012 | Weight/Chain | impact normalization/result / Edit+Play | result mapping; threshold G4 | G3 pending |
| PR-F-013 | ADR/Ally/Content Lock | ally lifecycle owner / Edit+Play | one-instance enforcement | `G3 PENDING` 정직 |
| PR-F-014 | Ally Contract | later relay-position/navigation / Edit+Play | route/navigation; content G5 | later G3 owner 필요 |
| PR-F-015 | Ally Contract | conditional suppression owner / Edit+Play | SHOULD inclusion; G3/G5 | `PARTIAL` 정직 |
| PR-F-016 | Chain/Content Lock | room-state owner / Edit+Play | unique change/result mapping; G4 | G3 pending |
| PR-F-017 | all lifecycle contracts | Reset orchestrator / Edit+Play | strategy/order/fallback; threshold G4 | G3 pending |
| PR-F-018 | Policy Contract | later run-policy owner / Edit+Play | not Prototype; candidate content G5 | G3 mapping pending |
| PR-F-019 | G1 intent, Draft taxonomy | conditional behavior-tag owner / Edit+Play | separate promotion/G5 | `PARTIAL` 정직 |
| PR-F-020 | G1 deferred boundary | 없음 | Change Control 전 technology/test 없음 | `PARTIAL` 정직 |
| PR-F-021 | LGT_01 Canon/scope | conditional rendering owner / Manual+G6 | COULD promotion, values/settings G3/G5/G6 | `PARTIAL` 정직 |
| PR-F-022 | FIN_01 visual-only | 없음 | OUT/COULD; dormant AT-FIN만 유지 | `PARTIAL` 정직 |
| PR-F-023 | Interaction/Momentum | later Perfect Catch owner / Edit+Play | not Prototype; timing/presentation G4/G6 | later G3 owner 필요 |
| PR-F-024 | G1 intent | conditional Structural owner / Play | separate promotion/G3/G5 | `PARTIAL` 정직 |

모든 Prototype MUST와 Vertical Slice MUST에는 G2 behavior owner와
candidate G3 owner가 있다. 현재 RTM의 Technical Draft path는 승인된
implementation mapping이 아니며 stale candidate class/heading을
구현 성숙도로 해석하면 안 된다. optional `PARTIAL`은 유지하는 것이
정직하다.

### RTM Technical-Source Honesty Findings

현재 RTM의 repository path/heading reference 301개는 모두 존재하고
깨진 heading은 0개다. 그러나 syntactic validity는 G3 maturity가
아니다.

- PR-F-001의 `### PlayerInteractionController`는 obsolete fixed
  Catch/Strap/Scoop order를 포함한다.
- PR-F-003, PR-F-014와 PR-F-015의 `### AllyRelayController`는 approved
  room-local pool/reservation 대신 generic inventory를 가정한다.
- PR-F-004와 PR-F-023의 `### CatchEvent`, PR-F-009의
  `### ImpactEvent`는 ordering, ownership와 causal identity를 충분히
  표현하지 않는다.
- PR-F-005의 `### ThrowController`는 unresolved Quick/Charged mode를
  settled responsibility처럼 포함한다.
- PR-F-006의 `## Strap Simulation`은 unapproved automatic release를
  포함한다.
- PR-F-010과 PR-F-011의 `### MomentumService`는 approved later
  lifecycle의 event/order/spend ownership을 닫지 않는다.
- PR-F-016의 `## Breakable Flow`는 non-Structural Prototype
  room-state change의 기술 owner로 부적절하며 Prototype breakable을
  암시한다.
- PR-F-017의 `## Prototype`은 snapshot-versus-regeneration을 결정하지
  않았다.
- PR-F-018의 `## 5. 데이터 원칙`은 Policy selected/pending/active/clear
  lifecycle owner를 제공하지 않는다.

따라서 위 source는 `MAPPED` 행에 존재하더라도 G3 기술 완결 증거가
아니다. G3 결정 적용 시 RTM Technical Source와 필요한 Trace Status를
별도 동기화해야 한다. 반대로 `G3 PENDING`과 optional `PARTIAL` 행은
현재 미성숙도를 정직하게 드러낸다. 이 감사에서는 RTM을 변경하지
않았다.

### Non-Functional Requirements

| NFR | Proposed G3 ownership / later Gate | 감사 |
|---|---|---|
| NFR-P-001 | build/profile owner / G6 | target build environment와 exact criteria 미정 |
| NFR-P-002 | physics budget/observability / G3-G4 | bounded active-body policy 필요 |
| NFR-P-003 | fragment budget / G3-G5 | Prototype required fragment 0, later scope |
| NFR-P-004 | VFX budget / G3-G4-G6 | small response는 physics object가 아님 |
| NFR-P-005 | production content / G5-G6 | enemy count는 기술 budget과 분리 |
| NFR-P-006 | spike/fault instrumentation / G3-G6 | measurement scenario와 threshold 미정 |
| NFR-U-001 | presentation/readability / G4 | Visual source는 기술 구현이 아님 |
| NFR-U-002 | path/direction instrumentation / G4 | manual/PlayMode ownership 필요 |
| NFR-U-003 | camera/occlusion adapter / G3-G4 | exact camera technique 미정 |
| NFR-U-004 | conditional accessibility / G6 | optional Slice scope |
| NFR-U-005 | presentation separation / G4 | Canon/Art는 policy, implementation은 G3 pending |
| NFR-M-001 | architecture/dependency / G3 | DEC-G3-004 필요 |
| NFR-M-002 | definition/runtime separation / G3 | DEC-G3-005 필요 |
| NFR-M-003 | unique runtime owner / G3 | DEC-G3-005/006 필요 |
| NFR-M-004 | trace governance / G1-G3 | G3 적용 후 RTM source 재동기화 필요 |
| NFR-M-005 | package Change Control / G1-G3 | DEC-G3-001/002와 exact locks 필요 |
| NFR-R-001 | collision continuity / G3-G4 | PlayMode harness와 tuning 필요 |
| NFR-R-002 | fault/recovery / G3 | detection, quarantine, fallback 필요 |
| NFR-R-003 | Reset owner / G3-G4 | strategy와 repeated-reset threshold 필요 |
| NFR-R-004 | persistent save / G5-G6 | `DEFERRED`, Prototype owner 없음 |
| NFR-D-001 | task/file-scope policy / G1 | 현재 운영 규칙과 정렬 |
| NFR-D-002 | trace/Gate policy / G1-G3 | owner와 validation layer 필요 |
| NFR-D-003 | observability / G3 | DEC-G3-012 필요 |

23개 모두 감사했다. NFR-U의 Visual/Art source는 Design/Policy이며
Technical Source가 아니다. G3 결정 후에도 실제 구현·profile evidence가
없으면 `PARTIAL`을 유지해야 한다.

### Acceptance Tests

| Layer proposal | Active tests |
|---|---|
| EditMode + PlayMode | AT-SCOOP-001, AT-PHY-001, AT-ALLY-001, AT-CATCH-001, AT-THROW-003, AT-STRAP-001, AT-WEIGHT-001, AT-CHAIN-001, AT-IMPACT-001, AT-SCOPE-001, AT-ROOM-001, AT-FLOW-001 |
| Manual/Visual + PlayMode evidence | AT-ART-001 |
| Later Slice EditMode + PlayMode | AT-CATCH-002, AT-MOM-001, AT-HEAVY-001, AT-ALLY-002, AT-ALLY-003, AT-ROGUE-001, AT-PROP-001, AT-STRUCT-001 |
| Dormant | AT-FIN-001 — `DORMANT / NON-BLOCKING`, harness activation 금지 |
| Deferred gap | PR-F-020 — active `AT-INC-001` 없음, 그대로 유지 |

모든 active test에 proposed layer가 있으나 test code, fixture, input,
timing, threshold와 execution evidence는 없다.

## Implementation Sequence and Solo Feasibility

G3 passage와 별도 최종 pre-implementation review 뒤에만 다음 bounded
sequence를 사용할 수 있다.

1. exact Editor/package lock과 repository/Unity Bootstrap;
2. stable definition reference, runtime identity와 state ownership foundation;
3. player locomotion, camera와 logical input adapter;
4. interaction arbitration와 ownership transitions;
5. direct physical acquisition, Carry, Throw와 Queued Throw;
6. authored baseline Reset, repeated-reset diff와 fallback;
7. fixed-position ally room-local supply/reservation/Catch;
8. valid-Anchor Cargo Strap attach/redirect/explicit release;
9. collision normalization, Causal Origin/result와 room-state change;
10. feedback adapter, observability, fault recovery와 Prototype regression.

Momentum/Pip, HeavyAction, Policy, Relay Position transition, Suppressed,
Behavior Tags, Structural, breakables, LGT_01, FIN_01, Incident Report와
persistent save는 이 sequence 뒤 별도 later-scope wave다.

| Risk | Solo assessment |
|---|---|
| Architecture/event | explicit small boundaries는 감당 가능; generic framework는 불필요 |
| Physics/Strap | 가장 높은 Prototype integration risk; bounded response와 observability 필요 |
| Ally AI | fixed position/pool로 Prototype 위험 제한; navigation은 later |
| Reset | 초기부터 설계하지 않으면 누적 비용 큼; sixth wave 이전 확보 |
| Test/debug | 최소 harness와 counters는 재작업을 줄임; CI는 아직 불필요 |
| Content | exact one-room/4 actors/7 objects로 제한되어 수용 가능 |

## G3 Decision Package

> **DESIGN AUTHORITY RECOMMENDATION — NOT OWNER APPROVAL**

### DEC-G3-001 — Unity Editor Lock Policy

| Field | Decision package |
|---|---|
| Question | Unity Bootstrap 전에 exact Unity 6 Editor revision을 어떤 정책으로 고정할 것인가? |
| Option A | G3에서 지원·재현 근거를 확인한 exact Unity 6 revision 하나를 고정하고 `ProjectVersion.txt`를 Bootstrap의 첫 결과로 기록한다. |
| Option B | Unity 6 major/minor만 지정하고 작업자별 patch를 허용한다. |
| Option C | Bootstrap 후 사용된 revision을 사후 기록한다. |
| Recommendation | **A** |
| Rationale | 재현성, package compatibility와 solo rollback을 확보하며 floating revision을 막는다. 이 감사에서는 exact revision을 선택하지 않는다. |
| Solo-development impact | 사전 확인 비용은 작고 환경 차이·재import·package drift 위험을 크게 줄인다. |
| Affected requirements/contracts/files | NFR-M-005, Windows PC boundary; `TECHNICAL_DESIGN.md`, `UNITY_PROJECT_STANDARD.md`, 향후 `ProjectVersion.txt` |
| Affected checklist item | Unity 정확한 버전과 패키지 잠금 |
| Owner response | `DEC-G3-001: ____` |

### DEC-G3-002 — Minimal Package Baseline

| Field | Decision package |
|---|---|
| Question | 첫 Prototype에 허용할 package baseline과 later deferral을 어떻게 나눌 것인가? |
| Option A | Unity 6/URP/GameObject PhysX/C# 위에 Input System, Cinemachine, Unity Test Framework만 Prototype 후보로 두고 exact compatible version을 함께 pin한다. AI Navigation은 Relay Position wave까지 defer하고 나머지 package는 제외한다. |
| Option B | Option A에 AI Navigation을 Prototype부터 포함한다. |
| Option C | Addressables, DOTS, rope, behavior tree, save/service framework까지 future-proofing package로 포함한다. |
| Recommendation | **A** |
| Rationale | Prototype requirement를 충족하는 최소 후보이며 fixed ally position에는 navigation package가 필요 없다. |
| Solo-development impact | upgrade surface, integration과 debugging 부담을 최소화한다. |
| Affected requirements/contracts/files | PR-F-001~017의 Prototype subset, NFR-M-005; `TECHNICAL_DESIGN.md`, `UNITY_PROJECT_STANDARD.md`, 향후 package manifest/lock |
| Affected checklist item | Unity 정확한 버전과 패키지 잠금 |
| Owner response | `DEC-G3-002: ____` |

### DEC-G3-003 — Prototype versus Slice Technical Scope

| Field | Decision package |
|---|---|
| Question | first implementation wave를 approved one-room Prototype behavior만으로 제한할 것인가? |
| Option A | Prototype Required 목록만 구현하고 Perfect/Momentum/Heavy/Relay Position/Policy와 optional systems는 later wave로 격리한다. |
| Option B | Vertical Slice MUST의 state skeleton도 first wave에 포함한다. |
| Option C | SHOULD/COULD까지 extensibility scaffold를 먼저 만든다. |
| Recommendation | **A** |
| Rationale | 현재 Prototype은 later system 없이 모든 MUST를 검증할 수 있다. |
| Solo-development impact | 초기 integration graph와 regression surface를 가장 작게 유지한다. |
| Affected requirements/contracts/files | PR-F-001~024, Prototype Content Lock, Vertical Slice Scope; 여섯 Technical Draft |
| Affected checklist item | 아키텍처 검토 |
| Owner response | `DEC-G3-003: ____` |

### DEC-G3-004 — Architecture and Dependency Direction

| Field | Decision package |
|---|---|
| Question | Prototype 책임과 dependency를 어떤 최소 구조로 고정할 것인가? |
| Option A | explicit composition root, single state owners, request/result boundary와 presentation one-way adapter를 사용하고 class name은 provisional로 둔다. |
| Option B | 현재 candidate component/class 목록을 그대로 baseline으로 승인한다. |
| Option C | generic service locator/event/DI framework를 먼저 구축한다. |
| Recommendation | **A** |
| Rationale | G2 ownership을 지키면서 stale Evade/Vault/priority/mode 책임과 presentation backflow를 제거한다. |
| Solo-development impact | 필요한 분리만 유지하고 framework 유지비를 피한다. |
| Affected requirements/contracts/files | NFR-M-001/M-003, Interaction/Ally/Chain contracts; `TECHNICAL_DESIGN.md`, `SOFTWARE_ARCHITECTURE.md` |
| Affected checklist item | 아키텍처 검토 |
| Owner response | `DEC-G3-004: ____` |

### DEC-G3-005 — Definition Data and Runtime State

| Field | Decision package |
|---|---|
| Question | definition, runtime instance와 encounter state ownership을 어떤 원칙으로 분리할 것인가? |
| Option A | stable content reference + transient runtime identity를 분리하고 shared definition에는 runtime state를 금지하며 Carry/Strap/relay/encounter에 단일 owner를 둔다. |
| Option B | Unity object reference와 component state만으로 모든 identity를 표현한다. |
| Option C | shared ScriptableObject에 definition과 current runtime state를 함께 둔다. |
| Recommendation | **A** |
| Rationale | Reset, duplicate suppression, inspection과 future save boundary를 동시에 보호한다. concrete schema는 후속 설계다. |
| Solo-development impact | 작은 명시적 state model로 hidden mutation과 reset leak를 줄인다. |
| Affected requirements/contracts/files | PR-F-001~018, NFR-M-002/M-003, 모든 G2 state contract; `DATA_AND_EVENTS.md`, `SAVE_AND_VERSIONING.md` |
| Affected checklist item | 아키텍처 검토 |
| Owner response | `DEC-G3-005: ____` |

### DEC-G3-006 — Event Topology and Ordering

| Field | Decision package |
|---|---|
| Question | physics와 gameplay lifecycle을 어떤 ordered request/result policy로 연결할 것인가? |
| Option A | request→validate→ownership commit→physical action→normalized fact→domain result→presentation 순서를 사용하고 Reset을 exclusive barrier로 두며 duplicate/idempotency/reentrancy 규칙을 명시한다. |
| Option B | current five event payload만 유지하고 callback order에 맡긴다. |
| Option C | 모든 state change를 전역 event bus에 publish한다. |
| Recommendation | **A** |
| Rationale | G2 lifecycle 누락을 채우고 raw collision/presentation의 cross-system mutation을 막는다. final event 이름과 payload는 후속이다. |
| Solo-development impact | 재현 가능한 debugging과 Reset을 얻고 global event complexity를 피한다. |
| Affected requirements/contracts/files | Interaction, Ally, Chain, Momentum, Policy contracts; `DATA_AND_EVENTS.md`, `SOFTWARE_ARCHITECTURE.md` |
| Affected checklist item | Event Contract 검토 |
| Owner response | `DEC-G3-006: ____` |

### DEC-G3-007 — Causal Identity and Result Keys

| Field | Decision package |
|---|---|
| Question | Origin, carrier, branch와 unique Result를 어떤 identity 분리 원칙으로 표현할 것인가? |
| Option A | stable content reference, transient runtime identity, Origin identity를 분리하고 같은 Origin의 compound target/result uniqueness를 사용하며 Origin merge를 금지한다. |
| Option B | runtime object reference와 ChainDepth만 사용한다. |
| Option C | content ID 하나를 instance, Origin과 Result identity로 재사용한다. |
| Recommendation | **A** |
| Rationale | Reset, duplicate/cycle suppression, branch 공유와 multiple-Origin 격리를 모두 표현한다. concrete key format은 미승인이다. |
| Solo-development impact | lineage bug를 초기에 관찰 가능하게 하며 과도한 graph framework는 필요 없다. |
| Affected requirements/contracts/files | PR-F-009/010/012/016, Chain/Momentum contracts; `DATA_AND_EVENTS.md`, `TECHNICAL_DESIGN.md` |
| Affected checklist item | Event Contract 검토 |
| Owner response | `DEC-G3-007: ____` |

### DEC-G3-008 — Physics, Collision and Fault Recovery

| Field | Decision package |
|---|---|
| Question | Prototype physics activation, collision normalization과 fault recovery의 기술 정책을 어떻게 제한할 것인가? |
| Option A | bounded dynamic activation, selective CCD, primitive/compound collider, normalized impact와 observable quarantine/restore policy를 사용하고 exact values는 G4로 둔다. |
| Option B | 모든 interactive object를 상시 Dynamic/CCD로 둔다. |
| Option C | fault object를 즉시 숨기거나 teleport해 계속 진행한다. |
| Recommendation | **A** |
| Rationale | physical continuity와 안정성을 함께 보호하며 Prototype breakable/uncontrolled ragdoll을 배제한다. |
| Solo-development impact | profiling과 recovery 부담을 제한하면서 high-risk physics를 측정 가능하게 한다. |
| Affected requirements/contracts/files | PR-F-002/006/008/009/012/016/017, NFR-P/R; `PHYSICS_DESTRUCTION_IMPLEMENTATION.md`, `PERFORMANCE_BUDGET.md` |
| Affected checklist item | 성능 예산 검토 |
| Owner response | `DEC-G3-008: ____` |

### DEC-G3-009 — Cargo Strap Technical Envelope

| Field | Decision package |
|---|---|
| Question | Cargo Strap의 first Prototype solver boundary를 어떤 방향으로 제한할 것인가? |
| Option A | simplified segmented visual + logical constraint와 bounded redirect response를 사용하며 valid Anchor, eligible floor-supported Medium, interaction exclusion과 explicit release만 소유한다. |
| Option B | authored pivot response만 사용하고 물리 constraint를 쓰지 않는다. |
| Option C | general-purpose rope simulation과 automatic Safety Release를 구현한다. |
| Recommendation | **A** |
| Rationale | physical equipment chain을 읽히게 하면서 full rope와 미승인 automatic behavior를 피한다. exact solver/equation은 후속이다. |
| Solo-development impact | 핵심 차별점의 risk를 bounded integration 하나로 제한한다. |
| Affected requirements/contracts/files | PR-F-006/007, Interaction/Weight contracts, GEAR_01 Canon; `PHYSICS_DESTRUCTION_IMPLEMENTATION.md`, `SOFTWARE_ARCHITECTURE.md` |
| Affected checklist item | 아키텍처 검토 |
| Owner response | `DEC-G3-009: ____` |

### DEC-G3-010 — Performance and Destruction Budget Policy

| Field | Decision package |
|---|---|
| Question | Prototype와 Slice budget 및 later measurement criteria를 어떻게 분리할 것인가? |
| Option A | locked Prototype scenario용 category/measurement method를 먼저 정하고 exact ceilings은 G3 후속 profile proposal과 G4/G6 evidence로 잠그며 Slice/production budget을 별도 표로 둔다. |
| Option B | 현재 일반 수치 표를 Prototype과 Slice 공통 승인값으로 사용한다. |
| Option C | budget 없이 최적화가 필요할 때 대응한다. |
| Recommendation | **A** |
| Rationale | content count와 runtime ceiling을 분리하고 zero-breakable Prototype을 정확히 반영한다. |
| Solo-development impact | 조기 과최적화 없이 spike와 scale risk를 재현 가능하게 만든다. |
| Affected requirements/contracts/files | NFR-P-001~006, Prototype Content Lock; `PERFORMANCE_BUDGET.md`, `PHYSICS_DESTRUCTION_IMPLEMENTATION.md` |
| Affected checklist item | 성능 예산 검토 |
| Owner response | `DEC-G3-010: ____` |

### DEC-G3-011 — Reset, Save and Versioning

| Field | Decision package |
|---|---|
| Question | Prototype Reset의 authoritative 정상 경로와 안전 fallback을 무엇으로 둘 것인가? |
| Option A | authored baseline snapshot/restore를 primary로, complete scene reload를 audited fallback으로 두고 repeated diff/subscription 검사를 요구한다. |
| Option B | 모든 Reset을 complete scene reload로만 처리한다. |
| Option C | deterministic regeneration과 future persistent-save framework를 먼저 구축한다. |
| Recommendation | **A** |
| Rationale | 빠른 반복과 확실한 복구를 함께 제공하며 persistent save와 Reset을 분리한다. concrete serialization은 후속이다. |
| Solo-development impact | restore 누락 검증 부담은 있지만 장기 iteration과 fault recovery가 단순해진다. |
| Affected requirements/contracts/files | PR-F-017, NFR-R-002/R-003/R-004, 모든 lifecycle contract; `SAVE_AND_VERSIONING.md` |
| Affected checklist item | Reset 전략 검토 |
| Owner response | `DEC-G3-011: ____` |

### DEC-G3-012 — Test Harness and Observability

| Field | Decision package |
|---|---|
| Question | first Prototype부터 어떤 harness layer와 observability를 필수 기술 baseline으로 둘 것인가? |
| Option A | EditMode logical tests, PlayMode physical/lifecycle tests, Manual readability를 분리하고 ownership/lineage/reset/fault/budget counters를 필수 관찰 대상으로 둔다. |
| Option B | PlayMode end-to-end test만 만든다. |
| Option C | Graybox 이후 수동 검증만 시작한다. |
| Recommendation | **A** |
| Rationale | physics와 Reset의 비결정성을 조기에 격리하고 Acceptance intent의 기술 owner를 제공한다. |
| Solo-development impact | 작은 초기 투자로 반복 regression과 원인 추적 비용을 줄인다. CI/overlay 구현은 아직 하지 않는다. |
| Affected requirements/contracts/files | 모든 active AT, NFR-D-003/R; `TEST_STRATEGY.md`, `ACCEPTANCE_TESTS.md`, 여섯 Technical Draft |
| Affected checklist item | Test harness 계획 |
| Owner response | `DEC-G3-012: ____` |

### DEC-G3-013 — Implementation Sequence and G3 Exit

| Field | Decision package |
|---|---|
| Question | G3 passage 후에도 별도 final pre-implementation review를 거쳐 bounded Prototype 순서만 실행할 것인가? |
| Option A | 본 문서의 10-wave Prototype sequence를 승인 방향으로 사용하고 23-item G3 Exit Checklist와 final pre-implementation review 전에는 Bootstrap/implementation을 금지한다. |
| Option B | current Technical Draft의 10-step sequence를 사용한다. |
| Option C | Prototype과 Vertical Slice system을 병렬 구현한다. |
| Recommendation | **A** |
| Rationale | Reset과 observability를 조기에 확보하고 later/optional system의 조기 침투를 막는다. |
| Solo-development impact | 가장 짧은 critical path와 명확한 stop/review 지점을 제공한다. |
| Affected requirements/contracts/files | 모든 Prototype MUST, Prototype Content Lock; `TECHNICAL_DESIGN.md`, `UNITY_PROJECT_STANDARD.md`, production Gate docs |
| Affected checklist item | 아키텍처 검토; 첫 구현 티켓 세분화는 G3 후 별도 |
| Owner response | `DEC-G3-013: ____` |

## Design Authority Recommended Defaults

다음은 하나의 coherent default package이며 Owner 승인이 아니다.

1. exact Unity 6 revision과 모든 approved package version을 Bootstrap
   전에 함께 pin한다.
2. Prototype 후보 package는 URP, Input System, Cinemachine, Unity Test
   Framework로 제한하고 AI Navigation은 later Relay Position까지
   defer한다.
3. approved one-room Prototype behavior만 first wave에 포함한다.
4. explicit composition, single owner, request/result와 one-way
   presentation dependency를 사용한다.
5. stable definition reference, transient runtime identity와 runtime state를
   분리한다.
6. ordered lifecycle, idempotency와 Reset barrier를 승인한다.
7. Origin/carrier/result identity를 분리하고 Origin merge를 금지한다.
8. bounded dynamics, normalized collision와 observable recovery를 사용한다.
9. Cargo Strap은 simplified visual/logical constraint + bounded response,
   explicit release로 제한한다.
10. Prototype/Slice budget과 G4/G6 측정을 분리한다.
11. snapshot/restore primary + scene-reload fallback의 hybrid Reset 방향을
    사용하고 persistent save를 분리한다.
12. EditMode/PlayMode/Manual layer와 causal/reset/fault observability를
    first wave부터 소유한다.
13. G3 exit와 final pre-implementation review 뒤에만 bounded 10-wave
    sequence를 ticket으로 분해한다.

## Owner Response Form

```text
DEC-G3-001:
DEC-G3-002:
DEC-G3-003:
DEC-G3-004:
DEC-G3-005:
DEC-G3-006:
DEC-G3-007:
DEC-G3-008:
DEC-G3-009:
DEC-G3-010:
DEC-G3-011:
DEC-G3-012:
DEC-G3-013:

APPROVE ALL RECOMMENDED G3 DEFAULTS
```

`APPROVE ALL RECOMMENDED G3 DEFAULTS`는 위 13개 결정의 방향만
승인한다. 이 문구는 G3를 통과시키거나, exact Editor/package version을
자동 선택하거나, Unity Bootstrap, code, Graybox 또는 구현을 허가하지
않는다.

## Proposed G3 Exit Checklist

- [ ] exact Unity Editor version locked
- [ ] exact package versions locked
- [ ] Prototype and Vertical Slice technical scope separated
- [ ] architecture responsibilities approved
- [ ] dependency direction approved
- [ ] definition/runtime state ownership approved
- [ ] core event contracts approved
- [ ] Causal identity and duplicate rules mapped
- [ ] physics and safety policy approved
- [ ] Cargo Strap technical envelope approved
- [ ] Prototype and Slice performance budgets separated
- [ ] Reset strategy approved
- [ ] save and versioning boundary approved
- [ ] test harness approved
- [ ] observability plan approved
- [ ] all Prototype MUSTs have technical owners
- [ ] all active tests have harness ownership
- [ ] RTM Technical Sources are honest
- [ ] no optional feature enters Prototype implementation
- [ ] no Unity or code work occurs before final pre-implementation review
- [ ] G3 remains NOT PASSED in this audit
- [ ] Graybox remains NOT STARTED
- [ ] Implementation Gate remains CLOSED

이 checklist는 제안이며 어느 항목도 이번 감사에서 통과 또는 체크하지
않았다.

## Blocking Issues

없음. 현재 모순과 누락은 13개의 bounded Owner decision으로 답할 수
있고, 필요한 G1/G2/Canon 근거는 존재한다. 이 판단은 G3 passage가
아니다.

## Non-Blocking Notes

- 현재 Technical Draft의 class, event payload, package와 budget은
  후보이며 Owner-approved 기술 baseline이 아니다.
- `Game/README.md`의 Bootstrap 순서는 exact lock과 G3 passage보다 먼저
  실행할 수 없다.
- `PRODUCT_REQUIREMENTS.md`나 RTM의 Draft Technical Source는 기술
  성숙도 또는 구현 완료를 의미하지 않는다.
- exact mass, force, speed, timing, collider, CCD, resolution, FPS,
  hardware와 pass threshold는 G4/G6 또는 별도 G3 budget proposal까지
  Draft다.
- Art/Production Support 3개와 첫 implementation-ticket decomposition은
  이 감사 범위 밖이며 Design Freeze는 `22 / 32`다.

## Overall Result

`READY FOR OWNER G3 TECHNICAL DECISIONS`

모든 기술 결정 질문이 bounded하고, 기존 Draft의 충돌과 repository
implementation 부재가 명시되었으며, Owner가 하나의 compact form과
coherent default package로 응답할 수 있다. 기술 선택, G3 passage와
구현 권한은 생성되지 않았다.

## Exactly One Recommended Next Task

`TASK-0049 — Apply Owner-Approved G3 Technical Baseline Decisions`

TASK-0049는 Owner가 모든 `DEC-G3-001`~`DEC-G3-013`에 답하거나
`APPROVE ALL RECOMMENDED G3 DEFAULTS`를 명시하기 전까지 차단된다.
Unity Bootstrap, Graybox, code, Art Support 또는 Implementation Gate
개방을 다음 작업으로 권고하지 않는다.

## Gate and Implementation Boundary

- Current Phase: `PRE-PRODUCTION / G3 TECHNICAL BASELINE`
- G1 Requirements: `PASSED`
- G2 Design: `PASSED / OWNER-APPROVED BASELINE`
- G2 Gameplay: `7 / 7`
- G3 Technical: `NOT PASSED / CURRENT BASELINE WORK`
- G3 Technical checklist: `0 / 6`
- Design Freeze: `22 / 32`
- Production Visual Lock sequence: `COMPLETE`
- Full Design Lock: `INCOMPLETE`
- Graybox: `NOT STARTED`
- Implementation Gate: `CLOSED`
- LGT_01: Records Archive-local `COULD`
- FIN_01: `OUT / COULD`

No technical decision, exact version, package installation, Unity project,
code, test, scene, prefab, build, Graybox or implementation is authorized
by this audit.
