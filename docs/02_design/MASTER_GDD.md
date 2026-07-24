# BREAK NOTICE — Master Game Design Document

> **MIXED-AUTHORITY CONSOLIDATION DOCUMENT**
>
> 이 문서는 제품 비전, Owner-approved G1 기준선, Owner-approved G2
> 계약, 역사적 설계 후보를 한곳에서 설명한다. 연결된 Owner-approved
> 요구사항·G2 계약·Accepted ADR·Visual Canon 문서가 각 항목의
> 권한을 소유하며 이 문서는 그 권한을 덮어쓰지 않는다. 연결되지 않은
> 정확한 mechanics, 수치, 콘텐츠, 제작 가정은 `DRAFT`,
> `SUPERSEDED` 또는 `HISTORICAL / NON-AUTHORITATIVE`다. 이 문서는
> 기술 명세나 구현 권한이 아니다.

## 1. 한 문장 정의

BREAK NOTICE는 해고 통보를 받은 사내 자산관리 직원이 산업용 화물
장비와 동료의 물리적 릴레이로 사무용품을 직접 받아 던지고 방향을
바꾸며, 읽을 수 있는 연쇄 충돌로 사무실의 국소 상태를 재구성하는
3인칭 물리 액션 게임이다.

## 2. 대표 장면

아래 장면은 Physical Relay와 읽을 수 있는 국소 인과를 설명하는
`ILLUSTRATIVE / NON-BINDING EXAMPLE`이다. 고정 gameplay sequence,
actor·object 수, 충돌 순서, room layout, force, destruction 또는
결과를 승인하지 않는다.

```text
시설 동료가 의자를 바닥으로 굴린다.
→ 플레이어가 허리 릴의 화물 스트랩을 의자 프레임에 연결한다.
→ 몸 전체로 회전해 진행 방향을 바꾼다.
→ 의자가 보안요원의 다리를 쓸어 넘어뜨린다.
→ 보안요원이 유리 칸막이를 깨뜨린다.
→ 프레임이 이동식 서랍장을 밀어 복사기에 충돌시킨다.
→ 복사기가 기울며 종이와 토너를 뿜고 새로운 통로가 열린다.
```

**No magic. Just teamwork, momentum, and office violence.**

PGT_02와 PGT_03는 각각 충돌 전 리디렉션 인과와 국소 aftermath의
Visual Canon evidence다. 두 이미지의 구도나 사건 순서는 gameplay
choreography 또는 구현 script가 아니다. Prototype의 정확한 콘텐츠와
affordance는
[`PROTOTYPE_CONTENT_LOCK.md`](PROTOTYPE_CONTENT_LOCK.md)가 소유한다.

## 3. 플레이어 판타지

주인공은 군인, 초능력자, 암살자가 아니다. 회사에서 가구·복사기·서버 장비를 옮기던 자산관리 직원이다. 전투 능력은 다음에서 나온다.

- 작업 경험과 물체 취급 기술
- 외부 장착형 하중 보조 브레이스
- 허리 수축식 화물 스트랩
- 실제 방 안의 사무용품
- 동료의 패스와 환경 조작
- 후속 충돌을 읽고 만드는 판단

## 4. 톤

- 세계와 인물은 현실적이다.
- 캐릭터는 상황을 진지하게 받아들인다.
- 코미디는 평범한 물체가 거대한 결과를 만드는 데서 나온다.
- 잔혹한 피와 절단보다 보호 장비 파손, 자세 붕괴, 에어백 전개 등 비살상 표현을 우선한다.

## 5. 핵심 Pillars

### 5.1 Physical Relay

동료는 공격력 버프가 아니라 실제 공간에서 물체를 전달한다. 패스는 가로막히고, 바닥을 구르고, 가구에 부딪히며, 플레이어가 받아야 한다.

### 5.2 Controlled Chaos

결과는 화려하지만 원인은 추적 가능해야 한다. 플레이어는 다음 시도에서 더 긴 Chain을 의도적으로 만든다.

### 5.3 Weight Has Meaning

Light, Medium, Heavy는 피해 숫자가 아니라 전투 역할과 움직임이 다르다.

### 5.4 Room Remembers

전투 종료 시 방의 통로, 가구, 시야, 설비가 시작 상태와 달라야 한다.

### 5.5 Fun Before Content

5분 Graybox가 반복해서 재미있다는 증거 없이 캐릭터 생산, 방 확장, 메타 성장, 보스를 진행하지 않는다.

## 6. 배경

대형 기업이 구조조정 당일 `Business Continuity Lockdown`을 발동한다. 자동 보안 시스템은 해고 대상 직원을 “미승인 자산”으로 분류하고 건물을 봉쇄한다. 주인공과 여러 부서 직원은 최상층 통제실로 올라가 봉쇄와 해고 처리를 직접 끝내려 한다.

## 7. 플레이 세션 구조

Policy Violation의 현재 Owner-approved G2 authority는
[`POLICY_VIOLATION_SELECTION_RUN_APPLICATION_CONTRACT.md`](POLICY_VIOLATION_SELECTION_RUN_APPLICATION_CONTRACT.md)를
따른다.

```text
Run 시작
→ 방 선택
→ 전투방 진입
→ 물체·동료·적·환경 기회 확인
→ 패스와 연쇄 충돌로 방 클리어
→ 다른 전투방이 남아 있으면 run당 한 번의 Policy Violation 기회
→ 서로 다른 eligible Candidate 2개 중 정확히 1개 선택
→ 선택 결과는 다음 전투방 진입부터 적용
→ 같은 run의 후속 방에서 한 Active Policy 유지
→ Run 종료 시 Policy clear
```

Policy Violation은 Prototype Core에서 비활성이고 Vertical Slice에서만
활성이다. Selection Opportunity는 run당 정확히 한 번이며, 한 eligible
encounter 완료 뒤 다른 combat encounter가 남아 있을 때 다음 encounter
진입 전에 발생한다. exact room index와 final room count는 G5 Draft다.

역사적 `10~15분`, `3개 전투방`, exact Elite와 finisher sequence는
`NON-AUTHORITATIVE G5/G6 DRAFT PRODUCTION CANDIDATES`다. FIN_01은
계속 `OUT / COULD`이며 이 session flow로 Vertical Slice에 편입되지
않는다.

## 8. 전투 루프

아래 순환은 승인된 동작들의 관계를 설명하는 예시이며 고정 입력 순서,
필수 choreography 또는 하나의 attempt 순서가 아니다.

```text
SCOUT     사용할 물체, 동료 위치, 충돌 표면 확인
→ 직접 물체 획득 또는 동료의 실제 물체 공급
→ Normal Catch·Queued Throw 또는 Medium feed·redirect
→ 읽을 수 있는 물리 결과와 인과 귀속
→ 국소 room-state change 확인
→ 필요할 때 Reset하고 다른 물체·각도·순서 재시도
```

권한별 활성 범위는 다음과 같다.

- `PROTOTYPE CORE`: direct acquisition, physical ally relay, Normal
  Catch, Queued Throw, Medium feed와 Cargo Strap redirect, causal
  result, meaningful local room-state change, Reset.
- `VERTICAL SLICE ONLY`: Perfect Catch, Momentum/Pip, HeavyAction,
  Relay Position, Policy Violation.
- `VERTICAL SLICE / SHOULD`: Suppressed, representative Behavior Tags,
  Structural Feature.
- `VERTICAL SLICE / COULD`: LGT_01 Records Archive-local variant.
- `OUT / COULD`: FIN_01.
- `P2 / DEFERRED`: Incident Report.

정확한 action/state/class/resource/chain/ally/content authority는
각 Owner-approved G2 계약을 따른다:
[`LOGICAL_INPUT_CONTRACT.md`](LOGICAL_INPUT_CONTRACT.md),
[`INTERACTION_STATE_CONFLICT_CONTRACT.md`](INTERACTION_STATE_CONFLICT_CONTRACT.md),
[`WEIGHT_CLASS_DATA_RANGE_CONTRACT.md`](WEIGHT_CLASS_DATA_RANGE_CONTRACT.md),
[`MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md`](MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md),
[`CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md),
[`ALLY_RELAY_SUPPRESSED_CONTRACT.md`](ALLY_RELAY_SUPPRESSED_CONTRACT.md),
[`PROTOTYPE_CONTENT_LOCK.md`](PROTOTYPE_CONTENT_LOCK.md),
[`POLICY_VIOLATION_SELECTION_RUN_APPLICATION_CONTRACT.md`](POLICY_VIOLATION_SELECTION_RUN_APPLICATION_CONTRACT.md).

## 9. 기본 논리 액션

현재 활성 logical action은 정확히 아홉 개다.

| Applicability | Active logical action |
|---|---|
| `PROTOTYPE CORE` | `Move`, `Look`, `Aim`, `InteractPrimary`, `Throw`, `CallAlly`, `StrapRelease`, `ResetEncounter` |
| `VERTICAL SLICE ONLY` | `HeavyAction` |

- Scoop, Normal Catch, Perfect Catch attempt와 Strap Attach는
  `InteractPrimary`의 context result다.
- Direct Throw와 Queued Throw는 같은 `Throw` action을 사용한다.
- Quick Throw와 Charged Throw는 미결정 action mode 후보다.
- `Sprint`, `Evade`, `Vault`, `Shove`는 비활성
  `DRAFT CANDIDATE`이며 현재 논리 입력 기준선에 포함되지 않는다.

정확한 intent, availability와 conflict는
[`LOGICAL_INPUT_CONTRACT.md`](LOGICAL_INPUT_CONTRACT.md)와
[`INTERACTION_STATE_CONFLICT_CONTRACT.md`](INTERACTION_STATE_CONFLICT_CONTRACT.md)를
따른다. physical binding, timing, buffer, target ranking, locomotion,
animation과 구현은 Draft다.

## 10. Scoop

현재 Scoop 권한은 다음 네 계약에 있다.

- [`LOGICAL_INPUT_CONTRACT.md`](LOGICAL_INPUT_CONTRACT.md)
- [`INTERACTION_STATE_CONFLICT_CONTRACT.md`](INTERACTION_STATE_CONFLICT_CONTRACT.md)
- [`WEIGHT_CLASS_DATA_RANGE_CONTRACT.md`](WEIGHT_CLASS_DATA_RANGE_CONTRACT.md)
- [`PROTOTYPE_CONTENT_LOCK.md`](PROTOTYPE_CONTENT_LOCK.md)

Scoop은 유효한 가까운 Light 또는 Medium의 직접 획득 결과다. Heavy는
normal Scoop 대상이 아니며 Prototype의 Copier-Class Asset은
recognition-only다. 물체를 순간이동시키거나 hidden inventory로 바꾸지
않는다.

역사적 `1.5~2m`, one-hand handling, 자세·회전 자동 보정과 정확한
획득 자세는 `HISTORICAL / NON-AUTHORITATIVE G2-G4 DRAFT`다.

## 11. Cargo Strap

시각·물리 연결 Canon은
[`CARGO_STRAP_SPEC.md`](../03_art/CARGO_STRAP_SPEC.md), 기능 범위는
[`INTERACTION_STATE_CONFLICT_CONTRACT.md`](INTERACTION_STATE_CONFLICT_CONTRACT.md),
[`WEIGHT_CLASS_DATA_RANGE_CONTRACT.md`](WEIGHT_CLASS_DATA_RANGE_CONTRACT.md),
[`ALLY_RELAY_SUPPRESSED_CONTRACT.md`](ALLY_RELAY_SUPPRESSED_CONTRACT.md)를
따른다.

- 대상은 바닥 지지를 유지하고 실제로 이동 가능한 eligible
  `MEDIUM`이다.
- Anchor는 하중 지지 금속 구조여야 한다.
- `허리 릴 → 검정·황색 스트랩 → 은색 카라비너 → 교체식 금속
  클램프 → 하중 지지 금속 Anchor`의 물리 연결을 유지한다.
- 붉은 넥타이는 장비와 완전히 분리한다.
- 캐스터, 휠 축, 플라스틱 하우징, 취약 돌출부에 연결하지 않는다.
- Heavy towing, grapple, 공중 부유, 장애물 관통, 원격 제어와
  telekinesis를 허용하지 않는다.

적을 당긴다는 역사적 문구는
`SUPERSEDED / UNAPPROVED HISTORICAL DRAFT`이며 active authority가
아니다. Automatic Safety Release의 조건과 동작도 `DRAFT`다.

## 12. Catch and Throw

Catch와 Throw의 권한은
[`LOGICAL_INPUT_CONTRACT.md`](LOGICAL_INPUT_CONTRACT.md),
[`INTERACTION_STATE_CONFLICT_CONTRACT.md`](INTERACTION_STATE_CONFLICT_CONTRACT.md),
[`WEIGHT_CLASS_DATA_RANGE_CONTRACT.md`](WEIGHT_CLASS_DATA_RANGE_CONTRACT.md),
[`ALLY_RELAY_SUPPRESSED_CONTRACT.md`](ALLY_RELAY_SUPPRESSED_CONTRACT.md),
[`MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md`](MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md)를
따른다.

### Normal Catch

Normal Catch는 `CATCH_ELIGIBLE` Light relay의 Prototype baseline
Catch outcome이다. 성공하면 Carry 또는 Queued Throw로 이어질 수
있다.

### Perfect Catch

Perfect Catch는 `VERTICAL SLICE ONLY`이며 Normal Catch와 구분되는
선택적 결과다. Catch 자체가 아니라 이후 player-owned action이
유효한 Meaningful Result를 만들면 현재 승인 계약의 origin당
`+5 Momentum` modifier를 적용할 수 있다.

### Queued Throw

Queued Throw는 별도 logical action이 아니라 같은 `Throw` intent의
pre-arrival mode이며 Catch 성공 뒤 정지·재획득 없이 투척 결과로
전환할 수 있다.

Normal Catch의 넓은 assistance, facing, exact timing, Perfect Catch
window, Contact Freeze, hold/release 해석, Quick/Charged 구분,
animation과 feedback은 모두 `DRAFT`다.

## 13. 동료

현재 권한은
[`ALLY_RELAY_SUPPRESSED_CONTRACT.md`](ALLY_RELAY_SUPPRESSED_CONTRACT.md)를
따른다.

- 일반 전투 active ally는 승인된 CHR_01 한 명이다.
- 동료는 실제 방의 유효하고 예약되지 않은 물체만 공급하며 player의
  최종 행동 주체를 대체하지 않는다.
- Prototype Core에서는 한 개의 고정 authored support position을
  사용하고 Relay Position 전환을 활성화하지 않는다.
- Vertical Slice에서는 유한한 authored Relay Positions 사이의 물리적
  전환을 사용할 수 있다. free-follow와 teleport는 없다.
- Light는 airborne Catch-oriented physical supply다.
- 이동 가능한 Medium은 floor-supported Strap-oriented supply다.
- Heavy는 normal `CallAlly` relay에 포함되지 않으며 자동 고정부
  해제·정렬 지원도 승인되지 않았다.
- Suppressed는 Prototype Core에서 비활성이며 Vertical Slice
  `SHOULD`인 조건부, 일시적, 회복 가능한 support state다.
- 동료는 적을 임의로 공격·처치하거나 GEAR_01을 조작하거나 resource를
  소비하지 않는다.

`Facilities Mover`는 승인된 이름·직무·Identity가 아니다. 정확한
Relay Position 수와 배치, supply inventory, candidate ranking,
trajectory, route, suppression threshold·duration, AI, animation,
physics와 Unity 구현은 Draft다.

## 14. 적

Prototype exact content authority는
[`PROTOTYPE_CONTENT_LOCK.md`](PROTOTYPE_CONTENT_LOCK.md)를 따른다.

### ENY-P-001 — Compliance Runner

- Prototype의 유일한 enemy archetype이며 정확히 두 instance다.
- ordinary corporate compliance/security pressure로 읽힌다.
- 물리적으로 이동 가능한 causal target이다.
- 읽을 수 있는 close-range pressure pattern 한 가지만 가진다.
- nonlethal presentation을 유지한다.

정확한 identity, Stability, stats, timing, AI, navigation, reaction과
Unity 구현은 `DRAFT`다.

### Vertical Slice Enemy — Draft Candidates

Partition Guard, Asset Mover, Interceptor, Elite와 다른 enemy
archetype은 Vertical Slice/later `DRAFT` 후보다. Prototype에는 포함하지
않고, final 이름·identity·수량·기능을 승인하지 않는다.

## 15. 물체 무게

Prototype은 정확히 네 object definition과 일곱 instance를 사용한다.
상세 boundary는
[`PROTOTYPE_CONTENT_LOCK.md`](PROTOTYPE_CONTENT_LOCK.md)를 따른다.

### Light — Setup and Precision

`OBJ-P-L-001` File Binder 세 개다. 한 개는 direct-acquisition pool,
두 개는 ally supply pool에 있으며 direct/relay throw와 causal
initiator 역할을 맡는다.

### Medium — Chain Maker

- `OBJ-P-M-001` Wheeled Office Chair 두 개: direct handling과
  floor-supported GEAR_01 redirect;
- `OBJ-P-M-002` Rolling Drawer Cabinet 한 개: attributed collision과
  side-lane clearance만 사용.

### Heavy — Space Breaker

`OBJ-P-H-001` Copier-Class Asset 한 개는 static, floor-supported
recognition-only landmark다. Prototype에서 HeavyAction, Momentum,
movement, breakage 또는 room-state result를 사용하지 않는다.

## 16. 행동 태그

Prototype content는 Behavior Tag를 요구하지 않는다. 다음 역사적 tag
목록은 Vertical Slice/later object differentiation을 위한 `DRAFT`
candidate이며 Prototype 기능이나 구현을 승인하지 않는다.

- Rolling
- Slab
- Rebound
- Fragile
- Powered
- Anchored
- Crush
- Tethered

exact tag type, object assignment, physical result, data structure와
Unity 구현은 별도 후속 결정 전까지 `DRAFT`다.

## 17. Momentum

Owner-approved G2 시작 profile은
[`MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md`](MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md)를
따른다.

- Momentum/Pip은 `VERTICAL SLICE ONLY`이며 Prototype Core에서는
  비활성이다.
- run은 Current Momentum `0`, Banked Pip `0`으로 시작한다.
- threshold `100 Momentum`에서 Pip으로 변환하며 최대
  `3 Banked Pips`를 저장한다.
- `HeavyAction` 시작 비용은 `2 Banked Pips`다.
- passive decay와 ordinary-damage resource loss는 없다.
- HeavyAction-origin result는 Momentum을 만들지 않는다.
- Structural Feature의 Pip cost는 승인되지 않았다.
- reward table, modifier, cap과 lifecycle의 정확한 시작값은 위
  계약에 있으며 G4/G6 evidence-based tuning 대상이다.

이 문서의 다른 Momentum·Chain·Heavy·Structural 수치 또는 보상 예시는
위 계약과 별도로 Owner-approved되지 않은 역사적 Draft다. Chain의
logical 생성·전파·종료는
[`CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md)를
따르며 runtime lineage, exact termination threshold, presentation과
final balance는 계속 Draft다.

## 18. Causal Chain

Owner-approved logical rule은
[`CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md)를
따른다.

- eligible player-owned 행동이 Causal Origin과 Initiating Carrier를
  Depth `0`에서 만든다.
- Initiating Carrier의 결과는 Direct, 물리적 Transfer를 받은
  Attributed Carrier의 결과는 Depth `1`~`4` Downstream이다.
- 최대 전파 깊이는 `4`이며 Depth `4` 뒤의 물리 simulation은 계속될 수
  있지만 Depth `5` 귀속 또는 reward를 만들지 않는다.
- Branch는 허용하지만 Branch와 Origin은 서로 merge하지 않는다.
- 같은 Origin의 같은 target/result-family 반복, resting contact,
  jitter와 cycle은 새 Result나 reward를 만들지 않는다.
- completed Reset과 lifecycle 종료는 Origin을 종료하며, 종료된 Origin은
  다시 열리지 않는다.
- Chain은 sequence-agnostic이다. PGT_03의 정확한 의자·적·유리·카트·
  복사기 순서는 gameplay rule이 아니다.

player-facing Chain Link, 문구, HUD, VFX, audio, exact natural-termination
threshold, runtime lineage와 Unity 구현은 Draft다.

## 19. 타격감

역사적 Light `20~35ms`, Medium `55~80ms`, Heavy `90~130ms`
Contact Freeze 범위는
`SUPERSEDED / HISTORICAL TUNING — NOT ACTIVE G2 AUTHORITY`다.
Contact Freeze 자체의 정확한 값과 적용 조건은 Draft다.

### Impulse Release

읽을 수 있는 물리적 변위가 spectacle보다 우선한다. 정확한 freeze,
impulse, force, damage, camera와 animation은 Draft다.

### Sound Layers

다음은 presentation 분리의 `DRAFT CANDIDATE`다.

1. 비행음
2. 물체 재질 접촉음
3. 보호 장비·신체 둔탁음
4. 벽·가구 후속 충돌음

## 20. 파괴

Prototype Content Lock은 breakage, glass, debris, destruction VFX와
Structural Feature를 모두 제외한다. Prototype의 meaningful
room-state change는 Rolling Drawer Cabinet의 비파괴 side-lane
clearance로 검증한다.

PR-F-024 Structural Feature는 `VERTICAL SLICE / SHOULD`이며 현재
activation, cost, feature content와 물리 동작이 승인되지 않았다.
Dynamic Prop, pre-fractured Breakable Furniture와 authored Structural
Feature라는 역사적 분류는 later design/production
`DRAFT CANDIDATE`다.

breakable 수, debris·particle 수, Rigidbody budget, fracture,
Pooling과 staged activation은 G3/G5 Draft다. PGT_03는 읽을 수 있는
국소 aftermath의 Visual Canon이며 정확한 content, sequence 또는
destruction budget이 아니다. unrestricted room-wide destruction은
금지한다.

## 21. Roguelite 보상

Policy Violation의 선택·current-run 적용 authority는
[`POLICY_VIOLATION_SELECTION_RUN_APPLICATION_CONTRACT.md`](POLICY_VIOLATION_SELECTION_RUN_APPLICATION_CONTRACT.md)를
따른다. 현재 승인 범위는 run당 한 번의 기회, 두 Candidate, 단일 선택,
다음 encounter 적용, one Active Policy, Reset 보존과 run-end clear다.

기존 Candidate 이름과 효과 예시는
[`ROGUELITE_PROGRESSION.md`](ROGUELITE_PROGRESSION.md)의
`SUPERSEDED / HISTORICAL DRAFT CANDIDATES`이며 active G2 또는
production authority가 아니다. final Candidate pool, exact effects,
UI, save와 구현은 Draft 또는 Deferred다.

## 22. 버티컬 슬라이스 방

Open Operations Office, Copy and Meeting Hub, Records Archive와 그
역사적 순서·enemy·object·objective는
`NON-AUTHORITATIVE G5/G6 DRAFT PRODUCTION CANDIDATES`다. 정확한 방
수, 순서, layout, enemy·object 수와 Elite 편입은 승인되지 않았다.

Records Archive 후보는 LGT_01을 자동 활성화하지 않는다. LGT_01은
Records Archive-local `COULD`이고 Global Lighting 기준이 아니다.
FIN_01은 계속 `OUT / COULD`이며 Vertical Slice 또는 최종 제품 범위에
편입되지 않는다.

현재 Prototype은
[`PROTOTYPE_CONTENT_LOCK.md`](PROTOTYPE_CONTENT_LOCK.md)의
`PRT-RM-001` 한 room만 사용한다.

## 23. 프로토타입 최소 범위

현재 exact authority는
[`PROTOTYPE_CONTENT_LOCK.md`](PROTOTYPE_CONTENT_LOCK.md) 하나다.
Prototype은 한 room, CHR_00 한 명, CHR_01 한 명, 동일한 Compliance
Runner 두 명, 네 object definition과 일곱 instance, 일곱 validation
affordance를 사용한다. Heavy Copier-Class Asset은 recognition-only이며
breakage, Momentum/Pip과 active Heavy use는 없다.

### Superseded Historical Prototype List

아래 과거 목록은 더 이상 독립적인 authority가 아니다. 현재 잠금과
일치하는 항목도 위 Content Lock과 관련 G2 계약에서만 권한을 얻는다.
충돌하는 수량·기능은
`SUPERSEDED PROTOTYPE DRAFT — NOT ACTIVE AUTHORITY`다.

- Graybox 방 1개 — 현재 authority는 `PRT-RM-001` 한 room
- 플레이어 1명 — 현재 authority는 CHR_00 한 명
- Relay 동료 1명 — 현재 authority는 CHR_01 한 명
- 기본 적 3명 — `SUPERSEDED`; 현재 Compliance Runner 두 명
- Light 2종 — `SUPERSEDED`; 현재 Binder 한 definition, 세 instance
- Medium 2종 — `SUPERSEDED`; 현재 Chair와 Drawer 두 definition
- Heavy 1종 — `SUPERSEDED`인 active-use 표현; 현재 Copier 한
  definition은 recognition-only
- 유리벽 1개 — `SUPERSEDED`; Prototype에 glass/breakage 없음
- Scoop, 패스, 캐치, 즉시 투척 — 현재 authority는 각 G2 계약과
  Content Lock affordance
- 의자 굴리기와 스트랩 리디렉션 — 현재 authority는 grounded Chair
  feed와 player-owned redirect affordance
- 기본 Stability, Momentum, 1-hop 이상 Chain — `SUPERSEDED`;
  Prototype Momentum은 비활성이고 causal opportunity는 Depth `1`
- 즉시 Reset — 현재 authority는 Reset 계약과 Content Lock

## 24. 절대 하지 않을 것

- Co-op
- 자유 절차 생성
- 모든 벽 완전 파괴
- 총기 중심 전투
- 동료 자유 추적 파티
- 작은 파편 전체 물리
- 재미 검증 전 보스·메타·스토리 캠페인 확대
- 포토리얼 AAA 아트 목표
- 넥타이를 케이블로 사용
