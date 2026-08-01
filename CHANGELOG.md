# Changelog

## Unreleased

### Changed

- TASK-0068에서 active Prototype의 무조건적 `defeat flow` 제외를 permanent death·Life/Continue·full Campaign Game Over·production save/checkpoint·failure alternate ending 제외로 교정하고 minimum nonlethal encounter failure/restart를 명시적으로 포함함; PR-F-025와 AT-FAIL-001의 active G4-A 범위를 비살상 presentation·current authored encounter-entry restart·반복 가능한 clean baseline으로 한정하고 `FAILURE_PENDING` 논리 상태, external encounter-pressure request, shared exclusive Reset barrier, G3 owner와 harness를 정렬하되 Campaign progress preservation은 FGD-001 later target으로 유지함. G1/G2/G3 passage와 Pre-G4 Lock을 보존하고 `TARGETED C3 RE-AUDIT REQUIRED BEFORE G4-A IMPLEMENTATION`, G4-A detailed protocol `PENDING`, G4/Graybox `NOT STARTED / NOT AUTHORIZED`, toolchain provisioning `OWNER-DEFERRED`, Gate `CLOSED`, active Relay `ABSENT`, 다음 단일 작업 `TASK-0069 — Targeted C3 Full-Game and G4-A Authority Re-Audit`를 기록했으며 FGD-001·CR-0001·ADR-0021·Story·Campaign·Map·Level·Visual·Unity·Package·code·test·Graybox는 변경하지 않음
- Human Owner-approved C3 Change Request CR-0001과 Accepted ADR-0021을 통해 byte-identical FGD-001을 `OWNER-APPROVED / LOCKED FULL-GAME TARGET`으로 등록하고 Working Title `DESTROY YOUR COMPANY: ZERO DEFECT`, ZERO DEFECT/HANDOVER 세계선, 단일 authored campaign, 3막·Levels 0–6·Hybrid Vertical Loop, Official Plan/As-Built pillar, 약 15명 구조 목표, traditional boss 0, 중앙 Atrium Finale와 insolvency Ending을 동기화함; PR-F-018/Policy는 Deferred Replay Layer로 이동하고 PR-F-025/AT-FAIL-001을 추가했으며 G4-A brief는 승인하되 `NOT STARTED / NOT AUTHORIZED`, R-007과 추가 speculative design brake는 ACTIVE, G4/G5 미통과, Graybox 미시작, toolchain provisioning Owner-deferred, Gate CLOSED, active Relay ABSENT와 다음 경계 `OWNER EXPLICIT UNITY TOOLCHAIN PROVISIONING INSTRUCTION REQUIRED`를 유지하고 Unity·Game·Package·코드·Scene·Prefab·Test·Build·이미지·Graybox 작업은 수행하지 않음
- TASK-0066은 기준선·범위 불일치 때문에 편집 전에 올바르게 중단됐고, 대체 TASK-0066-R1에서 mutable correction 범위를 여덟 문서에서 열다섯 문서로 확장해 `DESIGN_STATUS.md`를 유일한 mutable live-state authority로 중앙화함; MANIFEST·AGENTS·README의 파생 역할, Audit·Accepted ADR·Pre-G4 Lock·Visual safety guard의 immutable 또는 artifact-local 분류를 명시하고 R-007을 `ACTIVE / TRIGGERED`로 전환해 G4 evidence-first documentation brake를 활성화하되 Canon 또는 구현 권한은 변경하지 않음
- Human Owner의 직접 지시로 intended Unity toolchain을 Editor `6000.3.20f1` / changeset `c9ba695d4f07`, URP `17.3.0`, Input System `1.20.0`, Cinemachine `3.1.7`, Unity Test Framework `1.6.0`, AI Navigation `DEFERRED`로 문서 잠금하고 설치·Unity Hub 변경·라이선스 검증·batch mode·임시 프로젝트·Package materialization/resolution·머신 provisioning을 별도 직접 지시까지 연기함; 역사적 TASK-0064는 exact Editor 부재 Stop Condition을 적용해 설치·라이선스 확인·batch run·repository change 없이 종료됐고 `EAR-IMP-0001-001`은 `EXPIRED / UNUSED`, active Relay와 concrete execution SHA는 부재, IMP-0001은 `6 / 10 / EXECUTION BLOCKED`, Implementation Gate는 `CLOSED`로 유지하며 현재 다음 권한 경계를 `OWNER EXPLICIT UNITY TOOLCHAIN PROVISIONING INSTRUCTION REQUIRED`로 기록함
- TASK-0062의 `48 PASS / 0 FAIL` Re-Audit과 TASK-0063 Owner-delegated 결정을 근거로 감사된 설계 소스 기준선 `f638be24c6895a6969083626fa3f94228b533ec6`을 `OWNER-APPROVED / LOCKED` Pre-G4 Full Design Lock으로 최종화하고 PRE_G4_FULL_DESIGN_LOCK 기록과 ADR-0020을 추가함; IMP-0001의 future Execution Authorization Relay issuance는 `OWNER-AUTHORIZED / RELAY NOT ISSUED`, mandatory precondition은 `6 / 10`, execution은 차단, Gate는 `CLOSED`, concrete execution SHA는 부재, G4/G5는 미통과, Graybox는 미시작으로 유지하며 다음 단일 작업을 `TASK-0064 — IMP-0001 Execution Authorization Relay — Open the One-Shot Limited Gate and Materialize the Empty Unity Bootstrap`으로 지정하되 이번 작업에서 Relay 발급·Gate 개방·Unity·Package·code·Scene·Prefab·Test·Build·image·Graybox 실행은 하지 않음
- TASK-0061 교정 범위를 기준선 `9b72bce20c4bc96a66280b9fd69f7127e7a0554e`에서 독립 재감사해 역사적 Final Review의 네 실패를 모두 closure하고 `48 PASS / 0 FAIL` 및 `READY FOR LIMITED IMPLEMENTATION GATE DECISION — IMP-0001 ONLY`를 기록함; Design Freeze는 `32/32`, 역사적 Final Review는 `REVISION REQUIRED`, Full Design Lock은 `INCOMPLETE / READY FOR OWNER DECISION`, G4/G5는 미통과, Graybox는 미시작, Implementation Gate는 `CLOSED`, IMP-0001은 실행 차단, Relay와 concrete execution SHA는 부재한 상태를 유지하며 다음 단일 작업을 `TASK-0063 — Finalize Pre-G4 Full Design Lock and Authorize IMP-0001 Execution Relay Issuance`로 지정하고 Unity·Package·code·Scene·Prefab·Test·Build·image·Graybox 실행은 하지 않음
- TASK-0061에서 ADR-0019를 추가하고 IMP-0001의 자기참조식 in-ticket execution SHA를 별도 Owner-approved Execution Authorization Relay 방식으로 교정했으며, Design Freeze `32/32`를 documentary prerequisite로 유지한 채 Pre-G4 Full Design Lock과 post-G4 G5 Production Freeze를 분리하고 `OPEN — IMP-0001 ONLY / EMPTY BOOTSTRAP` one-shot Gate의 범위·만료·비상속 조건을 정의함; 역사적 Final Review는 `REVISION REQUIRED`, remediation은 `TASK-0061 CORRECTIONS APPLIED / RE-AUDIT REQUIRED`, Full Design Lock은 `INCOMPLETE / RE-AUDIT REQUIRED`, G4/G5는 미통과, Graybox는 미시작, Implementation Gate는 `CLOSED`, IMP-0001은 실행 차단, concrete SHA는 future Relay에서만 공급되며 다음 단일 작업은 `TASK-0062 — Re-Audit Final Pre-Implementation Readiness after Gate and Baseline Corrections`로 지정하고 Unity·Package·code·Scene·Prefab·Test·Build·Graybox 실행은 하지 않음
- 기준선 `a6398513bdc017529c600c8aa5bb5d7ea6e3e5cb`의 G1/G2/G3 passage, Design Freeze `32/32`, 아홉 승인 비주얼·지원 파일, Prototype·파괴·Office Core Kit 범위, Gate 순서, IMP-0001과 저장소 hygiene를 repository-wide Final Pre-Implementation Review로 감사해 `44 PASS / 4 FAIL` 및 `REVISION REQUIRED`를 기록함; IMP-0001의 in-ticket execution SHA가 self-referential이고 pre-G4 Full Design Lock 및 IMP-0001-only limited Implementation Gate 의미가 명시되지 않은 두 authority blocker를 확인하여 다음 단일 작업을 `TASK-0061 — Correct IMP-0001 Baseline Binding and Limited Bootstrap Gate Sequencing`으로 지정하되 G4/G5는 미통과, Full Design Lock은 미완료, Graybox는 미시작, IMP-0001은 실행 차단, Implementation Gate는 `CLOSED`로 유지하고 Unity·Package·code·Scene·Prefab·Test·Build·Graybox 권한을 만들지 않음
- CHR_00 Player Turnaround v001이 binary review contract를 `PASS`하고 Human Owner의 명시적 `APPROVE`를 받아 byte-identical `LOCKED PRODUCTION SUPPORT`로 등록되었으며, ADR-0018과 Player·Production 문서에 기존 Identity 권위 우선, 다섯 뷰 제작 가독성, Non-Canon 및 비구현 경계를 기록함; Production Visual은 계속 `8`, Production Support는 `1`, Design Freeze는 `32/32`와 `COMPLETE / FINAL PRE-IMPLEMENTATION REVIEW REQUIRED`, G5와 Full Design Lock은 아직 미완료, Graybox는 미시작, IMP-0001 실행은 차단, Implementation Gate는 `CLOSED`로 유지하고 bounded GPT Pro `PASS`/`APPROVE`에 대한 Human Owner standing delegation의 무-Gate-우회 경계를 동기화하되 Unity·Graybox·구현 권한은 만들지 않음
- 첫 구현 티켓 `IMP-0001 — Unity Repository Bootstrap and Exact Package-Lock Materialization`을 문서로 세분화해 future execution precondition, concrete baseline 경계, exact Editor·Package lock, 허용·금지 경로, operations, Given/When/Then, validation, performance, Git permission, stop condition과 completion contract를 고정하되 실행은 `OWNER-APPROVED TICKET DECOMPOSITION / EXECUTION BLOCKED`로 유지함; Unity·Package·code·asset은 생성하지 않고 Design Freeze를 `31/32`로 갱신했으며 유일한 미완료 항목은 CHR_00 Turnaround, 다음 경계는 `OWNER EXPLICIT IMAGE-GENERATION REQUEST REQUIRED — CHR_00 PLAYER TURNAROUND`로 보존
- CHR_00 Player Turnaround의 Owner-approved Production Brief와 binary review contract를 추가해 sole Identity reference, 정확한 다섯 전신 뷰, neutral modeling pose, 신체·의복·하네스·브레이스·릴·보관 스트랩·카라비너 continuity, 후면·측면 readability, 최대 세 detail callout, 3D simplification 및 Canon/Non-Canon 경계를 고정함; 미래 후보 경로만 예약하고 이미지는 생성·승인하지 않았으며 Turnaround checkbox는 미완료, Design Freeze는 `30/32`, G4/G5는 `NOT PASSED`, Graybox·Unity·구현은 미승인, 다음 경계는 `OWNER EXPLICIT IMAGE-GENERATION REQUEST REQUIRED — CHR_00 PLAYER TURNAROUND`로 기록
- Office Core Kit을 정확한 Prototype-required family `16`개(architecture/fixed fixture `7`, furniture/gameplay `5`, decorative/dressing `4`)와 Later-Slice extension candidate `8`개로 잠그고, gameplay-interactive definition `3`, recognition-only definition `1`, fixed/decorative family `12`, exact Prototype instance `7`, decorative non-interaction, geometry·finish variant cap과 department reuse policy, Prototype destruction `0`을 ADR-0017에 기록함; Design Freeze는 `30/32`로 갱신하되 G4/G5는 `NOT PASSED`, 실제 model·texture·material·Unity asset creation은 `NOT STARTED / NOT AUTHORIZED`, Graybox는 `NOT STARTED`, Implementation Gate는 `CLOSED`로 유지하며 다음 산출물은 `PENDING DESIGN AUTHORITY REVIEW AFTER TASK-0056`로 표시
- Prototype의 모든 파괴 category cap을 `0`으로 잠그고 Later-Slice의 room·event·fragment·small-material hard cap, small dynamic debris `0`, pre-authored state 전환, preallocated pooling, atomic break admission, causal-carrier 제한, Reset·fault observability와 runtime fracture·recursive destruction 금지를 Owner-approved baseline 및 ADR-0016으로 기록함; Design Freeze를 `29/32`로 갱신하되 Structural Feature는 `VERTICAL SLICE / SHOULD / PARTIAL`, G4/G5는 `NOT PASSED`, Full Design Lock은 `INCOMPLETE`, Graybox는 `NOT STARTED`, Unity·구현 권한은 없고 Implementation Gate는 `CLOSED`로 유지하며 다음 작업은 `PENDING DESIGN AUTHORITY REVIEW AFTER TASK-0055`로 표시
- 성공한 G3 Re-Audit의 `35/35 PASS`, blocker `2/2` closure와 Owner 승인을 근거로 G3 Technical Baseline 및 G3 Gate를 `OWNER-APPROVED / PASSED`로 승격하고 현재 Phase를 `PRE-PRODUCTION / DESIGN FREEZE COMPLETION`으로 전환함; 역사적 Final Audit은 `REVISION REQUIRED`로, Re-Audit 문서는 `READY FOR G3 GATE DECISION`으로 변경 없이 보존하되 그 readiness는 G3 passage에 소비되었고, G4/G5는 `NOT PASSED`, Design Freeze는 `28/32`, Full Design Lock은 `INCOMPLETE`, Graybox는 `NOT STARTED`, Implementation Gate는 `CLOSED`로 유지하며 다음 단일 작업을 `TASK-0055`로 지정
- corrected G3 baseline을 repository-wide Re-Audit해 역사적 Final Audit의 blocker `2/2` closure, exact Editor/package lock, six Technical checklist evidence, G1/G2-to-G3 owner·harness coverage, RTM path/heading honesty, Prototype/later scope와 empty Unity implementation state를 `35/35 PASS`로 확인하고 `READY FOR G3 GATE DECISION`을 기록하되, G3는 `NOT PASSED`, Full Design Lock은 `INCOMPLETE`, Graybox는 `NOT STARTED`, Implementation Gate는 `CLOSED`로 유지하며 다음 단일 작업을 `TASK-0054`로 지정
- G3 Final Audit의 두 잔여 활성 권한 오류를 교정해 AI Navigation을 Prototype Bootstrap package 권한에서 제거하고 exact Editor/package documentation lock은 유지했으며, Product Requirements의 active Phase를 `PRE-PRODUCTION / G3 TECHNICAL BASELINE`으로 동기화함; Final Audit은 역사적 `REVISION REQUIRED`, remediation은 `CORRECTIONS APPLIED / RE-AUDIT REQUIRED`, 다음 작업은 `TASK-0053`으로 기록하되 G3 passage·Unity·package·code·Graybox 권한은 만들지 않음
- G3 Technical Baseline과 Result-Family correction을 최종 감사해 technical owner·harness·RTM·scope·exact version lock은 정렬됐지만 `Game/README.md`의 AI Navigation package 권위와 `PRODUCT_REQUIREMENTS.md`의 active Phase가 현재 기준선과 충돌함을 확인하여 `REVISION REQUIRED`로 기록하되, G3 passage·Unity·package·code·Graybox·구현 권한은 만들지 않음
- G3 Causal Result Family taxonomy를 Owner-approved G2 Chain 계약의 `ACTOR_STATE_CHANGE`, `OBJECT_STATE_CHANGE`, `BREAK_STATE_CHANGE`, `ROOM_STATE_CHANGE`로 교정해 `BREAK_STATE_CHANGE`를 복구하고 무단 `STRUCTURAL` Result Family를 제거하되 설계·보상·범위·Gate·구현 상태는 변경하지 않으며 G3 Final Audit을 계속 대기
- Owner-approved `DEC-G3-001`~`DEC-G3-013`을 G3 Technical Baseline과 ADR-0015로 적용하고 Unity `6000.3.20f1`, URP `17.3.0`, Input System `1.20.0`, Cinemachine `3.1.7`, Test Framework `1.6.0` documentation lock, Prototype/later scope, architecture·state·event·causal·physics·Strap·budget·Reset·harness 책임을 동기화해 Technical checklist를 `6/6`, Design Freeze를 `28/32`로 갱신하되 G3는 `NOT PASSED`, Graybox는 `NOT STARTED`, Implementation Gate는 `CLOSED`로 유지하고 Unity 또는 구현 파일은 만들지 않음
- G3 Technical Draft 여섯 개와 빈 Unity 예정 구조를 Owner-approved G1/G2 기준선에 대조해 `READY FOR OWNER G3 TECHNICAL DECISIONS`로 기록하고 13개 bounded 기술 결정을 준비하되, 기술 선택·G3 passage·Unity·Graybox·구현 권한은 만들지 않음
- 성공한 G2 Re-Audit과 위임된 Owner 승인을 근거로 교정 기준선을 Owner-approved G2 Design Baseline으로 승격하고 G2 Gate를 `PASSED`로 기록한 뒤 현재 Phase를 `PRE-PRODUCTION / G3 TECHNICAL BASELINE`으로 전환하되, G3는 `NOT PASSED / CURRENT BASELINE WORK`, Full Design Lock은 `INCOMPLETE`, Graybox는 `NOT STARTED`, Implementation Gate는 `CLOSED`로 유지하며 Unity 또는 구현 권한을 만들지 않음
- 교정된 G2 기준선을 재감사해 `READY FOR G2 GATE DECISION`으로 기록하되 G2 passage, G3 entry, Graybox 또는 구현 권한은 만들지 않고 기존 Final Audit의 역사적 `REVISION REQUIRED`와 Implementation Gate `CLOSED`를 유지
- G2 stale-authority와 RTM source/trace honesty를 교정해 Master GDD를 계약 우선의 mixed-authority 문서로 분류하고, PR-F-019·PR-F-024를 `PARTIAL`로 조정하며 NFR-U-001~005의 Visual/Art evidence를 Technical Source에서 분리함; 기존 Final Audit은 역사적 `REVISION REQUIRED`로 유지하고 다음 산출물을 G2 Design Baseline Re-Audit로 전환
- G2 Policy Violation Selection and Run-Application Contract를 승인 상태로 등록해 run당 한 선택 기회, 두 Candidate, 단일 current-run Active Policy와 next-encounter·Reset·run-end lifecycle을 정의하고, 역사적 12-Candidate/3-choice 목록은 superseded Draft로 격하하되 final Candidate content·UI·save·implementation은 Draft로 유지하며 G2는 통과시키지 않음
- 기준선 `a0cf9b15d120b0842e2ddcfff1be697863581533`의 G2 Design Baseline Final Audit을 기록하고, PR-F-018 G2 계약 부재·stale Draft authority leak·RTM source/trace 과대 분류로 `REVISION REQUIRED`를 판정하되 G2 미통과와 Implementation Gate `CLOSED`를 유지
- G2 Prototype Content Lock을 승인 상태로 등록해 `PRT-RM-001` 한 room, CHR_00·CHR_01·Compliance Runner 두 명, `PRT-RP-001`, 네 object definition·일곱 instance와 두 physical pool·일곱 validation affordance를 고정하고 final art·수치·AI·animation·physics·Unity 구현은 Draft로 유지
- G2 Ally Relay, Relay Position과 Suppressed 계약을 승인 상태로 등록해 일반 전투 CHR_01 한 명, Prototype 고정 support position, 실제 Light/Medium 공급, reservation·release lifecycle, Vertical Slice position 전환과 조건부 SHOULD Suppressed를 정의하고 exact content·position·ranking·timing·AI·navigation·animation·physics·Unity 구현은 Draft로 유지
- G2 Causal Chain의 Origin 생성, Direct/Downstream 분류, 최대 Depth `4`, Branch·중복·순환·다중 Origin 경계와 종료·Reset 규칙을 승인 상태로 등록하고 runtime lineage·settlement threshold·표현·Unity 구현은 Draft로 유지
- G2 Momentum/Pip 시작 profile의 reward table·modifier·origin cap·resource lifecycle·HeavyAction 비용을 승인 상태로 등록하고, Chain propagation·final tuning·UI·physics·구현은 Draft로 유지
- G2 Light/Medium/Heavy functional envelope과 Catch/Strap/Carry/Throw/Relay eligibility를 승인 상태로 등록하고, Heavy는 Vertical Slice의 HeavyAction-only로 유지하며 수치·물리·구현값은 Draft로 보존
- G2 Interaction State, Arbitration, and Conflict Contract를 승인 상태로 등록해 Scoop/Catch/Throw/CallAlly/Cargo Strap/Heavy/Reset의 소유권·가용성·충돌·안전 거부를 정의하고, exact timing·binding·weight assignment·ranking·physics·구현은 Draft로 유지
- G2 Logical Input Action Contract를 승인 상태로 등록해 9개 active action과 4개 deferred candidate를 분리하고, state·binding·timing·implementation은 Draft로 유지
- Post-G1 Phase label을 `PRE-PRODUCTION / G2 DESIGN BASELINE`으로 동기화하고 G2를 현재 baseline work로 표시하되, G2 미통과와 Implementation Gate `CLOSED`를 유지
- 커밋 `b701a321d03e9c74ad2e3ec9bb8c4ee4fa921c30`의 정확한 G1 요구사항·NFR·기술 중립적 검증 의도·범위 문구를 Owner-approved Baseline으로 승격
- 제품 정의, Target Player, Windows PC 기준 플랫폼과 정성적 5분 재미 의도를 동기화하고, 전체 검증 후 G1 Requirements Gate를 `PASSED`로 기록
- 현재 Phase label 유지, G2 미통과, Full Design Lock 미완료, Graybox 미시작과 Implementation Gate `CLOSED`를 명시
- `FIN_01_Conference_Table_Team_Finisher_v003_FINAL`을 승인된 Production Visual로 등록
- FIN_01의 3인 Heavy 테이블 공동 추진 Visual Canon과 비승인 기능·제품 범위·구현 경계를 ADR-0014 및 Conference Table Team Finisher Visual 명세에 기록
- 현재 Production Visual Lock sequence 완료를 기록하되 Full Design Lock 미완료와 Implementation Gate `CLOSED`를 유지
- `LGT_01_Night_Archive_Lighting_v001_FINAL`을 승인된 Production Visual로 등록
- LGT_01의 Records Archive 지역 조명 변주 Canon과 비승인 조명 수치·전역 조명·렌더링 구현 범위를 ADR-0013 및 Night Archive Lighting Visual 명세에 기록
- 다음 Production Visual 대상을 `FIN_01 — Conference Table Team Finisher`로 전환
- `PGT_03_Chain_Reaction_Aftermath_v003_FINAL`을 승인된 Production Visual로 등록
- PGT_03의 국소 Chain Aftermath, 새로운 visibility line, 느슨한 post-impact 스트랩과 비승인 파괴·구현 범위를 ADR-0012 및 Chain Reaction Aftermath Visual 명세에 기록
- 다음 Production Visual 대상을 `LGT_01 — Night Archive Lighting`으로 전환
- `PGT_02_Chair_Redirect_Core_Loop_v010_FINAL`을 승인된 Production Visual로 등록
- PGT_02의 동료 공급·완전 해제, 플레이어 최종 리디렉션, 연속 GEAR_01 연결과 충돌 전 경계를 ADR-0011 및 Chair Redirect Visual 명세에 기록
- 다음 Production Visual 대상을 `PGT_03 — Chain Reaction Aftermath`로 전환
- `PGT_01_Gameplay_Camera_v002_FINAL`을 승인된 Production Visual로 등록
- PGT_01의 gameplay framing Canon과 비승인 카메라·환경·적·구현 범위를 ADR-0010 및 Camera 명세에 기록
- 다음 Production Visual 대상을 `PGT_02 — Chair Redirect Core Loop`로 전환
- `CHR_01_First_Ally_Identity_v001_FINAL`을 승인된 Production Visual로 등록
- CHR_01의 시설·물류 직원 Visual Canon과 비승인 신원·게임플레이 범위를 ADR-0009 및 Ally Visual 명세에 기록
- 다음 Production Visual 대상을 `PGT_01 — Gameplay Camera`로 전환
- `GEAR_01_Cargo_Strapping_System_v002_FINAL`을 승인된 Production Visual로 등록
- GEAR_01의 시각 Canon과 비승인 기능 범위를 ADR-0008 및 Cargo Strap 명세에 기록
- 다음 Production Visual 대상을 `CHR_01 — First Ally Identity`로 전환
- DOC-001에 따라 Canon 경로, Production Visual Lock 순서, 현재 Draft 상태 용어를 실제 저장소 상태와 정렬
- `CHR_00 Player Turnaround`을 비차단 Production Support 산출물로 분류하고 당시 `GEAR_01`을 다음 대상으로 지정
- Implementation Gate `CLOSED` 유지

## 0.1.0 — 2026-07-16

### Added

- 설계 우선 저장소 초기화
- Canon 요구사항·GDD·아트·기술·생산·품질 문서
- Codex 실행 프로토콜과 작업 템플릿
- 10종 컨셉 탐색 이미지
- 주인공 승인 이미지 `CHR_00_Player_Identity_Lock_v002_FINAL`
- Unity 프로젝트 예정 폴더 구조

### High-Level Baseline

- 3D TPS, Unity 6, URP, GameObject PhysX
- 직접 집기 + 별도 화물 스트랩 + 동료 패스
- 넥타이와 스트랩의 기능 분리
- 일반 전투 활성 동료 1명
- Grounded Corporate Action Realism
