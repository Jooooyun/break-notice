# Acceptance Tests

> **OWNER-APPROVED G1 BEHAVIORAL BASELINE — G2/G3/G4/G6 DETAILS REMAIN DRAFT**
>
> Document Decision Source: `DEC-G1-017`
>
> 기술 중립적 행동 결과는 G1 기준으로 Owner-approved다. G2 입력·상태
> 계약, G3 구현 기술·예산, G4/G6 성공 임계값은 Draft이며 어떤
> gameplay test도 실행 또는 통과되지 않았다.

## Acceptance-Test Policy

Decision Source: `DEC-G1-017`

- G1 test wording은 관찰 가능한 behavioral outcome만 요구한다.
- 논리 입력 이름과 상태 전이는 G2 Draft다.
- Rigidbody, IK, Cinemachine, Pooling 등 기술 선택은 G3 Draft다.
- 거리, 시간, 힘, 비용과 수량은 별도 tuning 또는 Gate criterion이다.
- 특정 물체·적·유리의 고정 순서는 causal requirement가 아니다.
- `AT-FIN-001`은 PR-F-022가 `COULD / OUT`인 동안
  `DORMANT / NON-BLOCKING`이다.
- PR-F-020은 `P2 / DEFERRED`인 동안 활성 Acceptance Test가 없다.

## Test Decision Sources

| Test or Policy | Decision Source |
|---|---|
| AT-SCOOP-001, AT-ALLY-001, AT-THROW-003, AT-STRAP-001, AT-ART-001, AT-IMPACT-001, AT-SCOPE-001, AT-ROOM-001 | DEC-G1-002, DEC-G1-017 |
| AT-PHY-001, AT-CHAIN-001, AT-FLOW-001 | DEC-G1-002, DEC-G1-017 |
| AT-CATCH-001, AT-CATCH-002 | DEC-G1-004, DEC-G1-017 |
| AT-WEIGHT-001 | DEC-G1-002, DEC-G1-006 clarification, DEC-G1-017 |
| AT-MOM-001 | DEC-G1-005, DEC-G1-017 |
| AT-HEAVY-001, AT-STRUCT-001 | DEC-G1-006, DEC-G1-017 |
| AT-ALLY-002 | DEC-G1-007, DEC-G1-017 |
| AT-ALLY-003 | DEC-G1-008, DEC-G1-017 |
| AT-ROGUE-001 | DEC-G1-009, DEC-G1-017 |
| AT-PROP-001 | DEC-G1-010, DEC-G1-017 |
| PR-F-020 deferred test gap | DEC-G1-011, DEC-G1-017 |
| AT-FIN-001 dormant policy | DEC-G1-013, DEC-G1-017 |

## Prototype Core Behavioral Tests

### AT-SCOOP-001 — 근거리 직접 집기

Requirement: `PR-F-001`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / G2+ DETAILS DRAFT`

**Given** 플레이어가 이동 중이고 가까운 곳에 유효 물체가 있다.
**When** 플레이어가 직접 획득 의도를 실행한다.
**Then** 이동 흐름을 장시간 중단하지 않고 물체를 획득한다.
**And** 물체는 먼 위치에서 손으로 순간이동하지 않는다.

정확한 interaction range와 입력 매핑은 G2 Draft다.

### AT-PHY-001 — 연속 물리 경로

Requirement: `PR-F-002`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / G2+ DETAILS DRAFT`

**Given** 유효 물체가 전달, 투척 또는 리디렉션된다.
**When** 물체가 출발점에서 해제된다.
**Then** 물체는 목적 결과까지 연속된 공간 경로를 가진다.
**And** 적, 가구와 충돌 표면이 그 경로를 가로막거나 바꿀 수 있다.
**And** 물체는 공중 정지하거나 순간이동하지 않는다.

구체 physics component와 collision mode는 G3 Draft다.

### AT-ALLY-001 — 동료의 실제 물체 공급

Requirement: `PR-F-003`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / G2+ DETAILS DRAFT`

**Given** 승인된 활성 동료 주변에 유효한 실제 물체가 있다.
**When** 플레이어가 해당 물체 계층의 공급을 요청한다.
**Then** 동료는 Light를 물리적으로 던지거나 Medium을 바닥으로 밀어 공급한다.
**And** 물체는 허공에서 생성되거나 원격 이동하지 않는다.

### AT-CATCH-001 — Normal Catch

Requirement: `PR-F-004`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / G2+ DETAILS DRAFT`

**Given** 요청된 물체가 유효한 Catch 관계로 플레이어에게 접근한다.
**When** 플레이어가 Normal Catch 조건을 충족한다.
**Then** 물체를 안정적으로 획득하고 다음 Carry 또는 Queued Throw 상태로 이어갈 수 있다.

Catch 범위와 timing은 G2 Draft다.

### AT-THROW-003 — Queued Throw

Requirement: `PR-F-005`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / G2+ DETAILS DRAFT`

**Given** 전달 물체가 도착하기 전에 플레이어가 투척 의도를 예약했다.
**When** Catch가 성공한다.
**Then** 별도 정지·재획득 없이 Catch에서 재투척으로 흐름이 이어진다.

정확한 buffer와 animation timing은 G2/G3 Draft다.

### AT-STRAP-001 — 화물 스트랩 리디렉션

Requirement: `PR-F-006`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / G2+ DETAILS DRAFT`

**Given** 바닥 지지 이동 물체에 유효한 하중 지지 금속 Anchor가 있다.
**When** 플레이어가 승인된 GEAR_01 연결 체인을 사용해 기계적으로 방향을 바꾼다.
**Then** `허리 릴 → 검정·황색 스트랩 → 은색 카라비너 → 교체식 금속 클램프 → 하중 지지 금속 Anchor`가 연속된 물리 연결로 유지된다.
**And** 물체는 바닥 접촉과 물리적 운동을 유지하면서 새 진행 방향을 보인다.
**And** 넥타이는 연결에 사용되지 않는다.
**And** 캐스터, 휠 축, 플라스틱 하우징 또는 취약 돌출부를 Anchor로 사용하지 않는다.
**And** 물체는 공중 정지, 원격 towing 또는 telekinesis로 보이지 않는다.

거리, 각도, 장력, release와 입력은 G2/G3 Draft다.

### AT-ART-001 — 넥타이 기능 분리

Requirement: `PR-F-007`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / G2+ DETAILS DRAFT`

승인된 캐릭터, 장비, animation, VFX와 Gameplay에서 붉은 넥타이는
물체 연결, 당김, 공격 또는 이동에 사용되지 않는다.

### AT-WEIGHT-001 — Prototype 무게 역할

Requirement: `PR-F-008`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / G2+ DETAILS DRAFT`

**Given** Prototype Core의 대표 Light와 Medium 물체가 있다.
**When** 동일한 encounter에서 사용한다.
**Then** Light의 빠른 setup 역할과 Medium의 위치·causal result 역할이 피드백과 결과로 구분된다.
**And** Heavy 후보는 별도 질량 계층으로 식별 가능하다.
**But** Heavy 사용, Pip 소비와 공간 변화는 Prototype Core 통과 조건이 아니다.

### AT-CHAIN-001 — Sequence-Agnostic Causal Attribution

Requirement: `PR-F-009`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / G2+ DETAILS DRAFT`

**Given** 하나의 플레이어 행동이 하나 이상의 후속 물리 결과를 유발한다.
**When** 대상과 환경이 서로 다른 순서 또는 조합으로 반응한다.
**Then** 유효한 후속 결과는 동일한 causal origin에 귀속될 수 있다.
**And** 동일한 결과는 중복 귀속되지 않는다.

정확한 Chain depth, TTL과 reward는 G2/G3 Draft다.

### AT-IMPACT-001 — 의미 있는 충돌 결과

Requirement: `PR-F-012`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / G2+ DETAILS DRAFT`

의미 있는 충돌은 적 자세·위치, 가구 위치, 파손 상태, 시야 또는
통로 중 하나 이상의 읽을 수 있는 결과를 만든다.

### AT-SCOPE-001 — 일반 전투 활성 동료

Requirement: `PR-F-013`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / G2+ DETAILS DRAFT`

일반 전투 중 활성 지원 동료는 한 명을 초과하지 않는다. FIN_01의
익명 보조 직원은 이 일반 전투 범위에 포함되지 않는다.

### AT-ROOM-001 — 의미 있는 국소 Room-State Change

Requirement: `PR-F-016`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / G2+ DETAILS DRAFT`

**Given** encounter의 시작 상태가 기록되어 있다.
**When** 핵심 물리 상호작용 또는 전투가 종료된다.
**Then** 시작 상태와 구분되는 의미 있는 국소 공간 변화가 남는다.

정확한 변화 개수와 통로 치수는 G4 Graybox Criterion이다.

### AT-FLOW-001 — Reset

Requirement: `PR-F-017`, `NFR-R-003`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / G2+ DETAILS DRAFT`

**When** Reset을 실행한다.
**Then** 현재 범위에서 활성화된 물체, 적, 동료, causal attribution,
자원, 파손과 이벤트 상태가 기준 상태로 돌아간다.
**And** 같은 검증 상황을 다시 실행할 수 있다.

복원 목록과 허용 시간은 G2/G4에서 정한다.

## Vertical Slice Behavioral Tests

### AT-CATCH-002 — Perfect Catch

Requirement: `PR-F-023`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / G2+ DETAILS DRAFT`

**Given** 물체가 선택적 Perfect Catch 조건으로 접근한다.
**When** 플레이어가 해당 조건을 충족한다.
**Then** 결과는 Normal Catch와 명확히 구분된다.

정확한 timing window, Contact Freeze와 Momentum 보너스는 별도
G2 Draft이며 이 G1 test outcome이 아니다.

### AT-MOM-001 — Momentum 보상 차등

Requirement: `PR-F-010`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / G2+ DETAILS DRAFT`

직접 적중과 비교해 더 긴 후속 결과 또는 동료 릴레이가 포함된
유효 결과는 더 큰 Momentum 보상으로 구분된다.

정확한 Chain depth와 보상값은 G2 Draft다.

### AT-HEAVY-001 — Heavy Object Access

Requirement: `PR-F-011`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / G2+ DETAILS DRAFT`

**Given** Heavy 물체와 승인 후보 자원 조건이 있다.
**When** 자원 조건이 충족되지 않는다.
**Then** Heavy 사용이 차단되고 원인을 읽을 수 있다.
**When** 자원 조건이 충족된다.
**Then** Heavy 사용이 허용되고 의미 있는 물리 결과가 발생한다.

정확한 Pip 비용, 질량, 힘과 물리값은 G2/G3 Draft다.

### AT-ALLY-002 — Relay Position

Requirement: `PR-F-014`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / G2+ DETAILS DRAFT`

**Given** 동료가 지원 위치를 바꿀 필요가 있다.
**When** 위치 전환이 발생한다.
**Then** 동료는 승인 후보 Relay Position 사이에서 이동한다.
**And** 플레이어를 자유 추적하는 지속 파티가 되지 않는다.

정확한 Position 수와 배치는 G2 Draft다.

### AT-ALLY-003 — Suppressed

Requirement: `PR-F-015`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / CONDITIONAL / G2+ DETAILS DRAFT`

**Given** 동료 지원이 적 압박으로 안전하지 않다.
**When** Suppressed 조건이 충족된다.
**Then** 지원은 일시 중단되지만 동료는 사망하거나 영구 이탈하지 않는다.
**When** 복구 조건이 충족된다.
**Then** 지원이 재개될 수 있다.

조건, timing과 AI는 G2/G3 Draft다.

### AT-ROGUE-001 — 행동 변경 선택

Requirement: `PR-F-018`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / G2+ DETAILS DRAFT`

**Given** 방 완료 보상 단계에 진입한다.
**Then** 서로 다른 행동 변화를 주는 복수 후보가 제시된다.
**When** 플레이어가 하나를 선택한다.
**Then** 선택한 행동 변화만 현재 run에 적용된다.

화면 제시 수와 전체 upgrade pool 수는 각각 G2 Draft와 G5 Scope다.

### AT-PROP-001 — 대표 행동 태그

Requirement: `PR-F-019`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / CONDITIONAL / G2+ DETAILS DRAFT`

선택된 대표 물체는 동일 무게 계층의 일반 물체와 구분되는
비무게 행동 특성을 물리 결과로 보여준다.

태그 종류와 적용 물체 수는 G2/G5 Draft다.

### AT-STRUCT-001 — Structural Feature

Requirement: `PR-F-024`
Status: `OWNER-APPROVED G1 BEHAVIORAL INTENT / CONDITIONAL / G2+ DETAILS DRAFT`

**Given** 승인 후보 Structural Feature와 유효 사용 조건이 있다.
**When** 해당 feature가 사용된다.
**Then** 국소 시야, 통로 또는 공간 상태 중 하나 이상이 의미 있게 바뀐다.

Feature 수, 파손 방식, 비용과 물리 구현은 G2/G3/G5 Draft다.

## Dormant Tests

### AT-FIN-001 — 특수 합동 피니셔

Requirement: `PR-F-022`
Status: `DORMANT / NON-BLOCKING`

PR-F-022는 `COULD`이며 승인된 Vertical Slice와 최종 제품 범위
밖이다. 별도 범위 승인 전에는 G1, G4 또는 G6 통과 조건으로
사용하지 않는다.

시각 방향은 FIN_01 Visual Canon만 따르며 Trigger, 비용, 물리,
animation, AI와 Unity 구현은 승인되지 않았다.

## Deferred Test Gap

PR-F-020은 `P2 / DEFERRED`다. `AT-INC-001`은 현재 생성하지 않는다.
PR-F-020이 Owner 결정으로 활성 범위에 승격될 경우 별도 승인
작업에서 behavioral outcome과 검증 Gate를 먼저 정의한다.
