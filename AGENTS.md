# AGENTS.md — BREAK NOTICE Preproduction Operating Rules

이 파일은 BREAK NOTICE 저장소 전체에 적용되는 최상위 Codex 작업 규칙이다.

Codex는 모든 작업을 시작하기 전에 이 파일을 읽어야 한다.

---

# 1. Current Project Phase

현재 단계:

`PRE-PRODUCTION / DESIGN FREEZE COMPLETION`

현재 프로젝트는 전체 Design Lock 상태가 아니다.

현재 Gate 상태:

- G1 Requirements: `PASSED`
- G2 Design: `PASSED / OWNER-APPROVED BASELINE`
- G3 Technical: `PASSED / OWNER-APPROVED BASELINE`
- G3 Technical Baseline: `OWNER-APPROVED / PASSED`
- G3 Technical Decisions:
  `OWNER-APPROVED / BASELINE APPLIED`
- G3 Causal Result-Family Alignment:
  `OWNER-APPROVED / CORRECTED`
- G3 Technical Final Audit:
  `REVISION REQUIRED / HISTORICAL BASELINE AUDIT`
- G3 Residual Active Status and Package-Authority Correction:
  `OWNER-APPROVED / COMPLETE`
- G3 Technical Final Audit Remediation:
  `COMPLETE / VERIFIED BY RE-AUDIT`
- G3 Technical Baseline Re-Audit:
  `READY FOR G3 GATE DECISION — CONSUMED BY G3 PASSAGE`
- G3 Gate Passage Source:
  `docs/05_production/G3_TECHNICAL_BASELINE_REAUDIT.md` at
  `9e1487e85be5a54ba0824c209deabcf738e1abf0`
- G4 Graybox: `NOT PASSED / NOT STARTED / NOT AUTHORIZED`
- G5 Freeze: `NOT PASSED`
- G3 Technical Checklist: `6 / 6`
- Design Freeze: `29 / 32`
- Destruction Budget and Implementation Rules:
  `OWNER-APPROVED / LOCKED`
- Production Visual Lock sequence: `COMPLETE`
- Full Design Lock: `INCOMPLETE`
- Graybox: `NOT STARTED`
- Implementation Gate: `CLOSED`
- Phase label review: `COMPLETED / OWNER-APPROVED`

G1 요구사항의 정확한 문구, disposition, Applicability, Priority와
기술 중립적 검증 의도는 Owner-approved다. 이는 구현 또는 검증
완료를 뜻하지 않는다.

G2 logical input action inventory와 각 action의 intent 의미, 상호작용
state ownership, action availability, cross-context arbitration,
safe rejection, Reset priority, Light/Medium/Heavy functional envelope과
class-based Catch·Strap·Carry·Throw·Relay eligibility는
Owner-approved다. Heavy active use는 Vertical Slice의
`HeavyAction`으로만 승인된다.

Momentum/Pip은 Vertical Slice에서만 활성화하며 다음 G2 시작 profile은
Owner-approved다.

- threshold `100 Momentum`
- 최대 `3 Banked Pips`
- direct Light `5`, direct Medium `10`, unique downstream result `15`,
  meaningful local room-state change `25`
- Ally Relay modifier `+5`와 Perfect Catch modifier `+5`
- origin당 reward cap `100 Momentum`
- `HeavyAction` 시작 비용 `2 Banked Pips`
- 같은 run의 room transition persistence와 encounter-entry Reset baseline
- passive decay 없음
- ordinary-damage resource loss 없음
- HeavyAction-origin self-funding 없음

다음 G2 Causal Chain logical contract도 Owner-approved다.

- eligible player-owned Origin과 non-rewarding HeavyAction Origin 경계
- Direct Result는 Depth `0`, Downstream Result는 Depth `1`~`4`
- 최대 propagation depth `4`
- 물리적 Transfer, Branch 허용과 Origin·Branch merge 금지
- 네 가지 Meaningful Result family와 Origin별 result uniqueness
- duplicate suppression, cycle prevention과 multiple-origin 비병합
- hard·natural termination, post-termination 비재개와 Reset 우선순위
- PGT_03의 정확한 순서가 아닌 sequence-agnostic causal attribution

다음 G2 Ally Relay, Relay Position and Suppressed logical contract도
Owner-approved다.

- 일반 전투 active ally는 승인된 CHR_01 한 명
- Prototype Core는 한 개의 고정 authored support position, 실제
  Light와 이동 가능한 Medium 공급, Relay Position 전환과 Suppressed
  비활성
- Vertical Slice는 유한한 authored Relay Position 전환을 사용할 수
  있으며 Suppressed는 조건부 `SHOULD`
- `READY`, `SUPPLY_COMMITTED`, `RELOCATING`, `SUPPRESSED` design state
- 한 개의 physical supply candidate, reservation과 하나의
  `ACTIVE_RELAY`
- `PRE-RELEASE` safe abort와 `RELEASED` 이후 실제 물리 해소
- Light는 airborne Catch-oriented, 이동 가능한 Medium은
  floor-supported Strap-oriented, Heavy는 normal relay 제외
- player의 최종 Throw·Redirect·HeavyAction·resource agency 보존
- Ally Relay modifier는 release와 player ownership 이후의 유효한
  Meaningful Result에만 origin당 `+5`

다음 G2 Prototype Enemy, Room, and Object Content Lock도
Owner-approved다.

- `PRT-RM-001 — Open Operations Prototype Room` 정확히 한 개
- CHR_00 한 명, CHR_01 한 명, 동일한 `ENY-P-001 — Compliance Runner`
  두 명으로 총 네 actor
- `PRT-RP-001 — Fixed Ally Support Position` 한 개
- 네 object definition과 일곱 instance:
  - `OBJ-P-L-001` File Binder 세 개
  - `OBJ-P-M-001` Wheeled Office Chair 두 개
  - `OBJ-P-M-002` Rolling Drawer Cabinet 한 개
  - `OBJ-P-H-001` Copier-Class Asset 한 개
- ally supply pool은 Binder 두 개와 Chair 한 개
- direct-acquisition pool은 Binder 한 개와 Chair 한 개
- Drawer는 attributed collision으로 side lane을 열고 Reset까지
  room-state change를 유지
- Copier는 Heavy recognition-only이며 Prototype active use 없음
- Perfect Catch, Momentum/Pip, HeavyAction, Structural, breakage,
  Suppressed, LGT_01과 FIN_01은 Prototype에서 비활성 또는 제외
- exact content authority는
  `docs/02_design/PROTOTYPE_CONTENT_LOCK.md`를 따름

다음 G2 Policy Violation Selection and Run-Application logical
contract도 Owner-approved다.

- Policy Violation은 `VERTICAL SLICE ONLY`이며 Prototype에서 비활성
- run당 정확히 한 Selection Opportunity
- 정확히 두 개의 distinct eligible Candidate를 Offer
- 정확히 한 Candidate만 선택
- run당 정확히 한 Active Policy
- stacking, replacement, skip과 reroll 없음
- 선택 결과는 다음 combat encounter 진입부터 적용
- 같은 run의 room transition과 `ResetEncounter`에서 Active Policy 보존
- run 종료에서 clear하고 새 run은 Policy 없이 시작
- actual Candidate effect와 final pool은 G5 Draft
- exact authority는
  `docs/02_design/POLICY_VIOLATION_SELECTION_RUN_APPLICATION_CONTRACT.md`를
  따름

`docs/02_design/MASTER_GDD.md`는
`MIXED-AUTHORITY CONSOLIDATION DOCUMENT`다. 연결된 Owner-approved
G1 기준선과 G2 계약이 우선하며, 연결되지 않은 정확한 mechanics,
수치, 콘텐츠와 제작 가정은 Draft 또는 historical
non-authoritative material이다.

PR-F-019 Behavior Tags와 PR-F-024 Structural Feature는 각각
`VERTICAL SLICE / SHOULD`인 optional scope이며 exact behavior,
activation과 production content가 미승인인 동안 RTM Trace Status는
`PARTIAL`이다. NFR-U-001~005의 Visual Spec과 Art Direction은
readability의 Design/Canon evidence이며 technical implementation
source가 아니다. 관련 technical owner는 Owner-approved
`docs/04_technical/G3_TECHNICAL_BASELINE.md`에 매핑된다.

여덟 G2 계약과 교정된 authority/trace 기준선은
`G2 Design Baseline Re-Audit`의 `READY FOR G2 GATE DECISION` 판정과
Owner 승인에 따라 G2 Design Baseline으로 승인되었고 G2 Gate는
`PASSED`다. 기존 `G2 Design Baseline Final Audit` 판정은 기준선
`a0cf9b15d120b0842e2ddcfff1be697863581533`에 대한 역사적
`REVISION REQUIRED`로 유지된다. Re-Audit은 G2 passage에 소비된
권한 근거다. Owner-approved G3 Technical Baseline의 역사적 Final
Audit은 `REVISION REQUIRED`이며, 두 활성 권한 오류의 교정은
`OWNER-APPROVED / COMPLETE`다. 교정 기준선 Re-Audit의
`READY FOR G3 GATE DECISION` 판정은 Owner-approved G3 passage의
근거로 소비되었고 G3 Gate는 `PASSED`다. G3 passage는 구현 또는
테스트 완료를 뜻하지 않으며 RTM의 `PARTIAL` 행은 정당한
later-Gate deferral로 유지된다. 어떠한 RTM 행도 `VERIFIED`로
승격되지 않았다.

다음 G3 documentation-level technical baseline은 Owner-approved다.

- Unity Editor `6000.3.20f1`
- URP `17.3.0`
- Input System `1.20.0`
- Cinemachine `3.1.7`
- Unity Test Framework `1.6.0`
- AI Navigation은 Prototype package baseline에서 `DEFERRED`
- explicit composition, single mutable-state owner와 one-way
  Presentation dependency
- definition/runtime/Origin/Result identity 분리
- ordered Request→Validation→Ownership→Physical Fact→Domain
  Fact→Result→Presentation flow와 exclusive Reset barrier
- bounded Dynamic activation, selective CCD, normalized impact와
  observable fault recovery
- simplified Cargo Strap constraint와 explicit `StrapRelease`;
  automatic Safety Release와 full rope simulation 금지
- Prototype/later-Slice budget owner 분리
- authored snapshot/restore primary와 audited Scene-reload fallback
- EditMode/PlayMode/Manual harness ownership과 observability
- G3 passage와 final pre-implementation review 뒤에만 가능한 bounded
  10-wave sequence

G3 Causal Result Family는 Owner-approved G2 계약의 정확한 네 family인
`ACTOR_STATE_CHANGE`, `OBJECT_STATE_CHANGE`, `BREAK_STATE_CHANGE`,
`ROOM_STATE_CHANGE`로 교정되었다. Structural Feature는 별도의 optional
design role이며 Result Family가 아니다.

정확한 권위는 `docs/04_technical/G3_TECHNICAL_BASELINE.md`와
`docs/09_adrs/ADR-0015-G3-TECHNICAL-BASELINE.md`를 따른다. 이는
실제 Unity project, package resolution, code, test, Scene, Prefab,
build 또는 Graybox를 승인하지 않는다.

다음 파괴 예산과 구현 경계는 Owner-approved Design Freeze
baseline이다.

- Prototype Core의 Breakable, Structural Feature, Break Event,
  large physical fragment, causal-carrier fragment, small dynamic
  debris, destruction-specific particle와 runtime fracture cap은 모두 `0`
- Later-Slice room당 ordinary Breakable `2`, 별도 G5 승인이 있을 때만
  허용되는 conditional Structural Feature `1`, ordinary Break Event
  `2`, Structural Event `1`, 총 break transition `3`
- ordinary/Structural event당 large fragment `4`/`8`, room pool `16`,
  awake `12`, event당 causal-carrier `2`, recursive/player-interactive
  fragment와 small dynamic debris `0`
- fixed step당 새 Break Event `1`
- ordinary/Structural event당 small material response `32`/`64`,
  event당 Primary Impact emitter `1`, Material Burst `1`,
  Persistent Aftermath marker `1`, room당 marker `3`,
  동시 live small element `96`
- 모든 small material element의 gameplay collision과 causal-carrier
  역할 `0`
- pre-authored intact→broken state, preallocated pool, active event 중
  `Instantiate`/`Destroy` 금지, atomic admission, partial break 금지,
  `DESTRUCTION_BUDGET_REJECTED` 관측
- runtime fracture·shattering·recursive destruction·runtime NavMesh
  rebuild·arbitrary wall destruction·room collapse 금지
- 완전한 Reset restore와 observable quarantine·baseline·fallback

정확한 권위는
`docs/04_technical/DESTRUCTION_BUDGET_AND_IMPLEMENTATION_RULES.md`와
`docs/09_adrs/ADR-0016-BOUNDED-DESTRUCTION-BUDGET.md`를 따른다.
Structural Feature는 계속 `VERTICAL SLICE / SHOULD / PARTIAL`이며 이
baseline은 G4·G5 passage, FPS·hardware·frame-time 기준, Unity 구현,
Graybox 또는 Implementation Gate 개방을 승인하지 않는다.

exact timing, range, buffer, target ranking, physical binding, movement
constraint와 구현은 계속 `DRAFT`다.

다음 항목은 아직 `DRAFT`다.

- 상세 게임플레이 설계
- Light/Medium/Heavy의 exact physical mass·dimension·speed·force·
  damage와 object-specific assignment
- Chain runtime lineage field·ID·event·storage, exact natural-termination
  threshold, maximum simultaneously active origins와 simultaneous-origin
  settlement algorithm
- Perfect Catch timing, Contact Freeze, Behavior Tag details, final
  Momentum/Pip balance와 break value
- Chain·Momentum/Pip HUD, VFX, audio, animation, physics와 implementation
- exact Prototype room geometry, actor·object position, final enemy·
  object·environment art와 physical values
- Vertical Slice Relay Position count, 모든 exact position·support zone·
  route, ally supply ranking·trajectory·timing, Suppressed threshold·
  duration·recovery, feedback, AI와 navigation
- Policy Violation의 actual Candidate names·effects, final pool,
  rarity·weighting·randomization, final room index, UI·presentation,
  save·telemetry와 technical implementation
- 논리 입력의 timing·buffer, physical binding, exact movement
  constraint, animation, physics와 Unity 구현
- Acceptance Test의 정확한 입력·수치·성공 임계값
- 파괴 hard cap을 제외한 밸런스·성능 tuning과 FPS·hardware·
  frame-time 기준
- exact implementation class, payload, schema와 tuning
- Unity Bootstrap 및 구현 ticket
- Vertical Slice의 정확한 콘텐츠와 구현 범위

Implementation Gate는 `CLOSED`다.

현재 Phase label은 Owner 승인에 따라
`PRE-PRODUCTION / DESIGN FREEZE COMPLETION`이다. G3 Technical은
`PASSED / OWNER-APPROVED BASELINE`이며 기술 책임·데이터·이벤트·
검증·예산·Reset·test-harness 기준은 Owner-approved baseline으로
승격되었다. 역사적 Final Audit은 `REVISION REQUIRED / HISTORICAL
BASELINE AUDIT`, 잔여 활성 상태·패키지 권한 교정은
`OWNER-APPROVED / COMPLETE`, remediation은
`COMPLETE / VERIFIED BY RE-AUDIT`다. Re-Audit의
`READY FOR G3 GATE DECISION` 판정은 G3 passage에 소비되었지만
Re-Audit 문서 자체는 변경하지 않는다. Technical checklist는
`6 / 6`, Design Freeze는 `29 / 32`다. 파괴 예산과 구현 규칙은
`OWNER-APPROVED / LOCKED`지만 현재 `Game/**`에는 placeholder만 있고
Unity project는 생성되지 않았다. G4와 G5는 `NOT PASSED`이며 다음
단일 작업은 `PENDING DESIGN AUTHORITY REVIEW AFTER TASK-0055`다.
남은 Design Freeze 항목은 `CHR_00 Player Turnaround`,
`Office Core Kit 목록`, `첫 구현 티켓 세분화`의 정확히 세 개다.
새 numbered task는 승인되지 않았다. remaining Design Freeze와
final pre-implementation review 전에는 Graybox 또는 Unity 구현을
시작하지 않으며 Implementation Gate는 `CLOSED`다.

---

# 2. Current Approved Visual Canon

현재 승인되고 LOCK된 Production Visual은 다음 여덟 개다.

- `references/approved/characters/CHR_00_Player_Identity_Lock_v002_FINAL.png`
- `references/approved/equipment/GEAR_01_Cargo_Strapping_System_v002_FINAL.png`
- `references/approved/characters/CHR_01_First_Ally_Identity_v001_FINAL.png`
- `references/approved/gameplay/PGT_01_Gameplay_Camera_v002_FINAL.png`
- `references/approved/gameplay/PGT_02_Chair_Redirect_Core_Loop_v010_FINAL.png`
- `references/approved/gameplay/PGT_03_Chain_Reaction_Aftermath_v003_FINAL.png`
- `references/approved/lighting/LGT_01_Night_Archive_Lighting_v001_FINAL.png`
- `references/approved/gameplay/FIN_01_Conference_Table_Team_Finisher_v003_FINAL.png`

관련 승인 문서:

- `docs/03_art/PLAYER_VISUAL_SPEC.md`
- `docs/03_art/CARGO_STRAP_SPEC.md`
- `docs/03_art/ALLY_VISUAL_SPEC.md`
- `docs/03_art/CAMERA_SPEC.md`
- `docs/03_art/CHAIR_REDIRECT_VISUAL_SPEC.md`
- `docs/03_art/CHAIN_REACTION_AFTERMATH_VISUAL_SPEC.md`
- `docs/03_art/NIGHT_ARCHIVE_LIGHTING_VISUAL_SPEC.md`
- `docs/03_art/CONFERENCE_TABLE_TEAM_FINISHER_VISUAL_SPEC.md`
- `docs/09_adrs/ADR-0007-PLAYER-VISUAL-LOCK.md`
- `docs/09_adrs/ADR-0008-GEAR-01-PRODUCTION-VISUAL-LOCK.md`
- `docs/09_adrs/ADR-0009-CHR-01-PRODUCTION-VISUAL-LOCK.md`
- `docs/09_adrs/ADR-0010-PGT-01-PRODUCTION-VISUAL-LOCK.md`
- `docs/09_adrs/ADR-0011-PGT-02-PRODUCTION-VISUAL-LOCK.md`
- `docs/09_adrs/ADR-0012-PGT-03-PRODUCTION-VISUAL-LOCK.md`
- `docs/09_adrs/ADR-0013-LGT-01-PRODUCTION-VISUAL-LOCK.md`
- `docs/09_adrs/ADR-0014-FIN-01-PRODUCTION-VISUAL-LOCK.md`

승인된 CHR_00의 다음 요소는 임의로 변경하지 않는다.

- 현실적인 성인 남성 비율
- 지치고 집중된 회사원 인상
- 구겨진 흰색 셔츠
- 걷어 올린 소매
- 짧고 느슨한 암적색 넥타이
- 사원증
- 작업용 바지
- 작업 장갑
- 안전화
- 산업용 하중 분산 하네스
- 한쪽 팔의 외부 장착형 하중 보조 브레이스
- 허리 장착형 수축식 화물 스트랩 릴
- 검정·황색 화물 스트랩
- 금속 카라비너

이미지의 렌더링 품질, 미세 볼트 위치, 얼굴 모공, 흠집 위치는 Canon이 아니다.

승인된 GEAR_01의 다음 요소는 임의로 변경하지 않는다.

- 허리 장착형 수축 릴
- 검정·황색 직조 화물 스트랩
- 은색 금속 카라비너
- 보강 아이가 있는 교체식 자산 고정 클램프
- `릴 → 스트랩 → 카라비너 → 클램프 → 하중 지지 금속 Anchor` 연결 체인
- 캐스터·휠 축·플라스틱 하우징·취약 돌출부가 아닌 하중 지지 금속 Anchor
- Stored, Attached, Tensioned Redirect, Safety Release, Brace Lock, Warning Behavior의 여섯 시각 상태
- 바닥 지지 상태를 유지하는 의자 리디렉션
- 체결 위치가 읽히는 브레이스 잠금 레버
- 제한된 황색 기계식 경고 표시
- 넥타이와 장비의 완전한 분리

정확한 스트랩 거리, 각도, 힘, 타이밍, 입력, 장력 임계값, 내부 작동, 기술 구현, 미세 부품 위치는 GEAR_01 Canon이 아니다.

승인된 CHR_01의 다음 요소는 임의로 변경하지 않는다.

- 평범하고 현실적인 시설·물류 직원 정체성
- 회청색 계열의 실용 작업복
- 회사 사원증
- 작업 장갑
- 안전화
- 소형 유지보수 파우치
- 현실적인 현장 근무자 체형
- 절제되고 진지한 표정
- CHR_00의 넥타이·브레이스·하네스·허리 릴·화물 스트랩을 복제하지 않는 실루엣
- 보안·군사·전술·슈퍼히어로 장비와 구분되는 일반 직원 실루엣

정확한 얼굴 세부, 성별 표현, 민족성, 나이, 머리 모양, 이름, 배경 이야기, 피부·머리카락 렌더 품질, 주름·마모 위치, 미세 비대칭은 CHR_01 Canon이 아니다. 패스 재고·거리·타이밍, Relay Position, Suppressed, Heavy 지원, AI, 애니메이션 수와 Unity 구현도 승인되지 않았다.

승인된 PGT_01의 다음 요소는 임의로 변경하지 않는다.

- 실제 3인칭 Gameplay View
- 약간 높은 Over-the-Shoulder 방향
- 플레이어의 상대적 화면 비중
- 전신 또는 거의 전신 가시성
- 바닥과 물체 진행 공간의 가독성
- 플레이어·동료·적 구분
- 사용 가능 물체와 복수 이동 경로의 가독성

정확한 카메라 거리, 높이, FOV, damping, collision, occlusion, 카메라 상태 전환, Camera Shake, Cinemachine과 Unity 구현은 PGT_01 Canon이 아니다. 이미지의 정확한 방 구조, 적 외형과 인원수, 소품 모델·배치·수량, 동료 위치, 조명과 collision surface도 incidental Non-Canon detail이다.

승인된 PGT_02의 다음 요소는 임의로 변경하지 않는다.

- 승인된 동료가 바닥 지지 의자를 물리적으로 공급하고 완전히 놓는 인과
- 플레이어가 GEAR_01의 연속된 물리 연결을 통해 최종 방향 전환의 주체가 되는 인과
- `허리 릴 → 검정·황색 스트랩 → 은색 카라비너 → 교체식 금속 클램프 → 하중 지지 의자 금속 Anchor` 연결
- 의자의 바닥 접촉 유지
- 스트랩 장력선과 의자의 새 진행 방향 구분
- 플레이어의 체중, 스탠스, 골반·상체 회전과 브레이스 자세로 읽히는 리디렉션
- 목표와 충돌하기 전에 끝나는 장면

정확한 입력, 거리, 각도, 속도, 힘, 장력, 마찰, 타이밍, 자동 연결·해제, 애니메이션, AI, 물리값, Rigidbody, Joint, IK, Cinemachine과 Unity 구현은 PGT_02 Canon이 아니다. 이미지의 정확한 방 구조, 조명, 목표 위치와 외형, 의자 모델, 캐스터 각도, 배우 위치와 세부 포즈도 incidental Non-Canon detail이다.

승인된 PGT_03의 다음 요소는 임의로 변경하지 않는다.

- PGT_02의 리디렉션 의자가 시작 원인의 증거로 남는 인과
- 하나의 임시 목표가 비치명적으로 이동·균형 상실 후 회복 중인 상태
- 국소적인 유리 파손 구역
- 이동식 롤링 수납 자산과 복사기급 사무 자산의 연쇄 이동 또는 전도
- 파괴 결과로 읽히는 새로운 시야선과 변경된 국소 room state
- 제한적이고 읽을 수 있는 큰 물리 조각과 절제된 작은 유리·종이·먼지·토너 반응
- 의자에 연결된 채 느슨하고 더 이상 제어하지 않는 GEAR_01 스트랩
- 실제 3인칭 gameplay view에서 읽히는 국소적이고 통제된 파괴

정확한 Chain 순서와 hop 수, 적·물체·파편·Particle 수, 피해, Stability, Knockback, Ragdoll, Momentum, 보상, 정확한 방 구조와 소품 배치, 통로 치수, 스트랩 해제 시점, 애니메이션, 물리값, VFX, Rigidbody, Breakable, Cinemachine과 Unity 구현은 PGT_03 Canon이 아니다. 깨진 유리 구역은 새로운 visibility line으로 승인되지만 완전히 통과 가능한 경로의 정확한 형태와 치수는 Canon이 아니다.

승인된 LGT_01의 다음 요소는 임의로 변경하지 않는다.

- Records Archive에 한정된 after-hours 조명 변주
- 현실적인 차가운 중성 형광등 Practical을 Primary Source로 사용하는 관계
- 선반과 통로 암부의 절제된 Cool Neutral-to-Green-Blue Ambient
- Black Crush 없이 유지되는 Shadow Detail
- 거칠고 제한된 바닥·금속·유리·플라스틱 PBR Highlight
- 플레이어, 동료, 임시 적, Stored GEAR_01, 사용 가능 물체와 복수 통로의 Gameplay Readability
- 전경, 선반 Aisle과 후경 사이의 공간 깊이
- 공포나 초자연 표현이 아닌 진지한 After-hours Corporate Tension

LGT_01은 Records Archive 지역 조명 변주이며 게임 전체의 Global Lighting 또는 Color Grading 기준이 아니다. 정확한 색온도, RGB, Lux, Exposure, Contrast, 조명 기구 수와 위치, Fixture Failure, Flicker, Emergency Gameplay, Shadow, Reflection, LUT, Tone Mapping, Baked/Realtime/Mixed Lighting, GI, Probe, Volumetric, Post-processing, URP 설정과 Unity 구현은 LGT_01 Canon이 아니다. 이미지의 정확한 방 구조, 선반 수, Aisle 폭, 적·소품·카트 수와 위치, Actor 배치와 Level Layout도 incidental Non-Canon detail이다.

승인된 FIN_01의 다음 요소는 임의로 변경하지 않는다.

- 승인된 CHR_00 플레이어 한 명, 승인된 CHR_01 동료 한 명과 익명의 Finisher-only 보조 직원 한 명
- 세 사람이 하나의 Heavy 회의 테이블에 직접 접촉해 공동으로 미는 관계
- 중앙 선행 위치에서 최종 방향과 타이밍을 주도하는 CHR_00
- 플레이어를 지원하되 대체하지 않는 CHR_01과 익명 보조 직원
- 세 인물의 양손 접촉, 체중 이동, 발 접지와 상체 기울기로 읽히는 공동 추진
- 현실적인 하중 보조 상태로 읽히는 CHR_00의 외부 전완 브레이스
- 통합 캐스터 구조로 바닥 지지를 유지하는 테이블
- 임시 목표와 충돌하기 전에 끝나는 장면
- Stored/Inactive 상태이며 테이블에 연결되지 않는 GEAR_01
- 복장 전용이며 테이블·장비·인물과 연결되지 않는 붉은 넥타이

FIN_01은 PR-F-022 `COULD` 기능의 Visual-only conditional direction이다. 익명의 보조 직원은 CHR_02, 일반 전투 Active Ally, 지속 Party Member 또는 승인된 Character Identity가 아니다. 정확한 인물 신원, 테이블·캐스터·목표·카메라·방·배우 배치와 자세는 incidental Non-Canon이다. Trigger, 빈도, Momentum/Pip 비용, 입력, 물리, 안무, 애니메이션, AI, IK, Rigidbody, Caster Physics, Cinemachine과 Unity 구현은 승인되지 않았다. FIN_01 Visual Lock은 Vertical Slice 또는 최종 제품 편입을 승인하지 않는다.

---

# 3. Production Visual Lock Progress

다음 순서로 Production Visual Lock을 진행한다.

1. `GEAR_01 — Cargo Strapping System` — `LOCKED`
2. `CHR_01 — First Ally Identity` — `LOCKED`
3. `PGT_01 — Gameplay Camera` — `LOCKED`
4. `PGT_02 — Chair Redirect Core Loop` — `LOCKED`
5. `PGT_03 — Chain Reaction Aftermath` — `LOCKED`
6. `LGT_01 — Night Archive Lighting` — `LOCKED`
7. `FIN_01 — Conference Table Team Finisher` — `LOCKED`

현재 Production Visual Lock sequence는 완료되었다. 다음 Production Visual 대상은 승인되지 않았다.

이는 Full Design Lock 완료, FIN_01의 제품 범위 편입 또는 Implementation Gate 개방을 뜻하지 않는다.

각 결과물은 사용자에게 명시적으로 승인받기 전까지 `REVIEW` 상태다.

Codex는 스스로 결과물을 `APPROVED`, `FINAL`, `LOCKED`로 선언하지 않는다.

---

# 4. Approved High-Level Constraints

다음은 현재 프리프로덕션 작업에서 유지할 상위 제약이다.

- 1인 개발
- 폭포수 기반 개발
- 명시적 단계별 승인 게이트
- Unity `6000.3.20f1` documentation lock; project 미생성
- 3D Third-Person 예정
- Grounded Corporate Action Realism
- Grounded world, absurd physics
- 마법과 염력 없음
- 초능력 Grab 없음
- 붉은 넥타이는 일반 복장과 시각적 상징일 뿐임
- 넥타이를 로프, 케이블, 무기, 연결 장치로 사용하지 않음
- 물체 제어에는 별도의 산업용 화물 스트랩 사용
- 일반 전투의 활성 동료는 한 명
- 파괴는 통제되고 1인 개발 가능한 범위로 제한
- 캐릭터는 상황을 진지하게 받아들임
- 코미디는 과장된 얼굴이 아니라 물리적 결과에서 발생

이 제약들은 세부 게임 설계가 모두 완료되었다는 뜻이 아니다.

---

# 5. Instruction Authority

작업 중 정보가 충돌하면 다음 순서로 판단한다.

1. 현재 사용자가 명시적으로 내린 작업 지시
2. 저장소 루트 `AGENTS.md`
3. 사용자가 승인한 ADR
4. 현재 Phase 및 Status 문서
5. 승인된 Visual Lock 문서
6. 상세 설계 및 기술 문서
7. 탐색 이미지
8. 과거 문서와 Archive

현재 작업 지시가 승인된 상위 결정과 충돌하면 임의로 실행하지 않는다.

충돌 내용을 보고하고 사용자 판단을 기다린다.

탐색 이미지의 우연한 디테일은 게임 요구사항이 아니다.

승인 이미지도 해당 Visual Lock 문서에 기록된 Canonical Element만 권한을 가진다.

---

# 6. Duplicate Documentation Warning

현재 저장소에는 서로 겹치는 문서 구조가 존재할 수 있다.

예:

- 대문자 기반 문서 폴더
- 소문자 기반 문서 폴더
- 유사한 요구사항 문서
- 유사한 인수 테스트 문서
- 유사한 Codex 프로토콜 문서

Codex는 별도 정리 작업 없이 다음 행동을 하지 않는다.

- 어느 문서가 Canon인지 임의 결정
- 중복 파일 삭제
- 문서 이동
- 내용 병합
- 요구사항 ID 변경
- 폴더 이름 통일
- Archive 처리

중복이나 충돌을 발견하면 보고만 하고, 명시적인 정리 작업이 주어질 때까지 수정하지 않는다.

---

# 7. One Task, One Outcome

하나의 Task는 하나의 명확한 결과만 만든다.

허용되는 예:

- 저장소 상태 감사
- GEAR_01 시안 검토
- 승인된 이미지 등록
- 특정 문서 한 개 정리
- 특정 시스템 하나 설계
- 특정 기능 하나 구현

금지되는 예:

- 설계 확정과 Unity 구현을 동시에 진행
- 이미지 생성과 저장소 전체 정리를 동시에 진행
- 요청받지 않은 리팩터링
- 관련 없는 파일 정리
- 새로운 기능 추가
- 프로젝트 전체를 알아서 완성

---

# 8. Before Editing

파일을 수정하기 전에 반드시 다음을 먼저 보고한다.

- 이해한 작업 목표
- 읽은 지침과 Canon 문서
- 변경 예정 파일
- 변경하지 않을 파일
- 발견한 모호함
- 범위 밖 항목
- 사용자 승인이 필요한 사항

모호함이 결과에 영향을 줄 경우 임의로 추측하지 않는다.

---

# 9. Repository Modification Rules

명시적으로 허용되지 않은 파일은 수정하지 않는다.

명시적인 요청 없이는 다음을 하지 않는다.

- 파일 생성
- 파일 삭제
- 파일 이동
- 파일 이름 변경
- 디렉터리 구조 변경
- 외부 의존성 설치
- Unity 패키지 설치
- 브랜치 생성
- Commit
- Push
- Pull Request 생성
- Pull Request Merge

자동 Merge는 금지한다.

사용자가 허용한 경우에도 변경사항과 Diff를 먼저 보고한다.

---

# 10. Visual Workflow Rules

비주얼 작업 상태는 다음을 사용한다.

- `EXPLORATION`
- `REVIEW`
- `REVISION_REQUIRED`
- `LOCKED`
- `ARCHIVED`

새로 생성된 시안의 기본 상태는 `REVIEW`다.

사용자만 다음 판정을 내릴 수 있다.

- `APPROVE`
- `REVISION REQUIRED`
- `REJECT`

사용자가 명시적으로 `APPROVE`하기 전에는:

- FINAL 파일명 사용 금지
- LOCK 문서 등록 금지
- ADR 승인 기록 금지
- 다음 단계의 Canon으로 사용 금지

이미지 생성은 사용자가 명시적으로 요청한 경우에만 수행한다.

---

# 11. Current Implementation Restrictions

Implementation Gate가 닫혀 있으므로 다음은 금지한다.

- Unity 프로젝트 초기화 또는 변경
- C# 게임플레이 코드 작성
- 패키지 설치
- Input System 설정
- Cinemachine 설정
- Rigidbody 시스템 구현
- 적 AI 구현
- 씬 제작
- Prefab 제작
- 빌드 파이프라인 구성
- 최적화 작업
- 테스트 코드 구현

현재 허용 범위는 명시적으로 요청된 프리프로덕션 작업뿐이다.

- 저장소 상태 확인
- 비주얼 시안 관리
- 승인 기록
- 문서 검토
- 설계 조율
- 범위 정의
- 변경 관리

---

# 12. Scope Protection

Codex는 다음을 임의로 추가하지 않는다.

- Co-op
- 총기
- 마법
- 염력
- 슈퍼히어로 장비
- 군사 SF 장비
- 다수 동료 동행 시스템
- 완전 절차적 파괴
- 모든 벽 자유 파괴
- 오픈월드
- 온라인 기능
- 추가 플랫폼
- 보스
- 메타 성장
- 새로운 캐릭터
- 새로운 게임 모드

좋아 보인다는 이유만으로 범위를 확장하지 않는다.

---

# 13. Completion Report

모든 작업 완료 보고에는 다음을 포함한다.

1. 작업 요약
2. 읽은 파일
3. 변경한 파일
4. 변경하지 않은 범위
5. 승인된 결정과의 일치 여부
6. 수행한 검사
7. 남은 모호함
8. 알려진 한계
9. 다음 권장 작업 한 개
10. 사용자 승인이 필요한 사항

파일을 수정하지 않은 작업에서는 반드시 다음을 명시한다.

`No files, branches, commits, pushes, or pull requests were created or modified.`

---

# 14. Final Operating Principle

설계되지 않은 것을 구현하지 않는다.

승인되지 않은 것을 LOCK하지 않는다.

요청받지 않은 범위를 확장하지 않는다.

문서 충돌을 조용히 해결하지 않는다.

사용자가 승인하기 전에는 다음 단계로 넘어가지 않는다.

**The specification is law.  
The image is evidence, not authority.  
Owner approval opens the next gate.**
