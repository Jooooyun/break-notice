# Product Requirements

> **PROPOSED G1 BASELINE — DECISIONS INCORPORATED, PENDING FINAL OWNER WORDING AND BASELINE APPROVAL**
>
> Document Decision Source: `DEC-G1-001` through `DEC-G1-019`
>
> Owner가 승인한 것은 결정 방향과 이 문서 개정 권한이다. 아래
> 정확한 문구와 G1 Baseline은 최종 Owner 승인 전까지 `DRAFT`이며,
> G1 통과, Phase 변경, Graybox 또는 구현을 승인하지 않는다.

## Authority and Scope Model

적용 단계, 우선순위, 권한 상태와 추적 완성도를 서로 다른 축으로
관리한다.

| 축 | 허용 값 | 의미 |
|---|---|---|
| 적용 단계 | `PROTOTYPE CORE`, `VERTICAL SLICE`, `P2`, `OUT` | 기능을 처음 요구하거나 검토하는 범위 |
| 우선순위 | `MUST`, `SHOULD`, `COULD`, `WON'T` | 해당 적용 단계 안에서의 중요도 |
| 권한 상태 | `DRAFT`, `DEFERRED`, `REJECTED`, `SUPERSEDED` | 문구와 범위의 의사결정 상태 |
| 추적 상태 | `UNMAPPED`, `PARTIAL`, `MAPPED`, `VERIFIED` | 요구사항과 근거·검증의 연결 성숙도 |

`DEFERRED`는 Priority가 아니다. Priority 열에는 `MUST`, `SHOULD`,
`COULD`, `WON'T`만 사용한다.

모든 요구사항 본문은 관찰 가능한 결과를 설명한다. 정확한 입력,
거리, 시간, 힘, 비용, 수량과 구현 기술은 G2 이후의 Draft,
Technical Budget 또는 Validation Criterion으로 분리한다.

## Proposed Scope Summary

| 범위 | Proposed Requirement IDs | Decision Source |
|---|---|---|
| Prototype Core MUST | PR-F-001~009, PR-F-012~013, PR-F-016~017 | DEC-G1-002 |
| Vertical Slice MUST | PR-F-010~011, PR-F-014, PR-F-018, PR-F-023 | DEC-G1-004~007, DEC-G1-009 |
| Vertical Slice SHOULD | PR-F-015, PR-F-019, PR-F-024 | DEC-G1-006, DEC-G1-008, DEC-G1-010 |
| P2 / Deferred | PR-F-020 | DEC-G1-011 |
| Local COULD | PR-F-021 | DEC-G1-012 |
| Outside approved product and Vertical Slice scope / COULD | PR-F-022 | DEC-G1-013 |

Prototype Core의 PR-F-008은 실제 Prototype에 등장하는 Light와
Medium의 역할 구분을 검증한다. Heavy는 별도 질량 계층 후보로
식별 가능해야 하지만, 실제 Heavy 사용, Pip 소비와 공간 변화는
Prototype Core에 포함하지 않는다.

## Functional Requirements

| ID | Applicability | Priority | Authority | Proposed Requirement | Validation Reference | Decision Source |
|---|---|---|---|---|---|---|
| PR-F-001 | PROTOTYPE CORE | MUST | DRAFT | 플레이어는 가까운 유효 물체를 이동 흐름을 장시간 중단하지 않고 직접 획득할 수 있어야 한다. | AT-SCOOP-001 | DEC-G1-002 |
| PR-F-002 | PROTOTYPE CORE | MUST | DRAFT | 전달·투척·리디렉션된 물체는 순간이동하거나 공중 정지하지 않고 장애물의 영향을 받는 연속된 물리 경로를 가져야 한다. | AT-PHY-001 | DEC-G1-002, DEC-G1-017 |
| PR-F-003 | PROTOTYPE CORE | MUST | DRAFT | 승인된 한 명의 활성 동료는 장면 안의 실제 물체를 물리적으로 공급해야 한다. Light는 던지고 Medium은 바닥 지지를 유지한 채 굴리거나 밀 수 있다. | AT-ALLY-001 | DEC-G1-002 |
| PR-F-004 | PROTOTYPE CORE | MUST | DRAFT | 플레이어는 유효한 전달 물체를 안정적인 Normal Catch로 받을 수 있어야 한다. | AT-CATCH-001 | DEC-G1-002, DEC-G1-004 |
| PR-F-005 | PROTOTYPE CORE | MUST | DRAFT | 플레이어는 물체가 도착하기 전에 투척 의도를 예약하여 Catch에서 재투척으로 흐름을 이어갈 수 있어야 한다. | AT-THROW-003 | DEC-G1-002 |
| PR-F-006 | PROTOTYPE CORE | MUST | DRAFT | 별도 산업용 화물 스트랩은 유효한 하중 지지 금속 Anchor에 연결된 바닥 지지 물체의 진행 방향을 플레이어의 기계적 행동으로 바꿀 수 있어야 한다. | AT-STRAP-001 | DEC-G1-002 |
| PR-F-007 | PROTOTYPE CORE | MUST | DRAFT | 붉은 넥타이는 복장 전용이어야 하며 물체 연결, 당김, 공격 또는 이동에 사용되지 않아야 한다. | AT-ART-001 | DEC-G1-002 |
| PR-F-008 | PROTOTYPE CORE | MUST | DRAFT | Prototype에 실제 등장하는 Light와 Medium은 역할과 피드백으로 구분되어야 한다. Heavy는 별도 질량 계층 후보로 식별 가능해야 하지만 Prototype Core에서 실제 사용, 자원 소비 또는 공간 변화를 요구하지 않는다. | AT-WEIGHT-001 | DEC-G1-002, DEC-G1-006 clarification |
| PR-F-009 | PROTOTYPE CORE | MUST | DRAFT | 최초 플레이어 행동에서 파생된 후속 물리 결과는 특정 장면 순서에 의존하지 않고 동일한 causal origin으로 귀속될 수 있어야 한다. | AT-CHAIN-001 | DEC-G1-002, DEC-G1-017 |
| PR-F-010 | VERTICAL SLICE | MUST | DRAFT | 직접 적중보다 더 긴 후속 물리 결과 또는 동료 릴레이가 포함된 결과는 더 큰 Momentum 보상으로 구분되어야 한다. | AT-MOM-001 | DEC-G1-005 |
| PR-F-011 | VERTICAL SLICE | MUST | DRAFT | 승인된 자원 조건을 충족하면 Heavy 물체를 사용할 수 있고, 충족하지 못하면 사용할 수 없다는 결과가 명확해야 한다. | AT-HEAVY-001 | DEC-G1-006 |
| PR-F-012 | PROTOTYPE CORE | MUST | DRAFT | 의미 있는 충돌은 적 자세·위치, 가구 위치, 파손 상태, 시야 또는 통로 중 하나 이상의 읽을 수 있는 결과를 만들어야 한다. | AT-IMPACT-001 | DEC-G1-002 |
| PR-F-013 | PROTOTYPE CORE | MUST | DRAFT | 일반 전투에서 활성 지원 동료는 한 명을 초과하지 않아야 한다. | AT-SCOPE-001 | DEC-G1-002 |
| PR-F-014 | VERTICAL SLICE | MUST | DRAFT | 동료는 자유 추적 파티가 아니라 승인 후보 Relay Position 사이에서 제한적으로 지원 위치를 바꿔야 한다. Prototype Core에서는 고정 지원 위치를 사용할 수 있다. | AT-ALLY-002 | DEC-G1-007 |
| PR-F-015 | VERTICAL SLICE | SHOULD | DRAFT | 동료는 사망·영구 이탈하는 호위 대상이 아니라, 압박에 따라 지원이 일시 중단되고 복구 가능한 Suppressed 상태를 가질 수 있어야 한다. | AT-ALLY-003 | DEC-G1-008 |
| PR-F-016 | PROTOTYPE CORE | MUST | DRAFT | 전투 또는 핵심 물리 상호작용 후에는 시작 상태와 구분되는 의미 있는 국소 room-state change가 남아야 한다. | AT-ROOM-001 | DEC-G1-002 |
| PR-F-017 | PROTOTYPE CORE | MUST | DRAFT | 플레이어는 같은 검증 상황을 다시 시험할 수 있도록 encounter를 기준 상태로 즉시 Reset할 수 있어야 한다. | AT-FLOW-001 | DEC-G1-002, DEC-G1-017 |
| PR-F-018 | VERTICAL SLICE | MUST | DRAFT | 방 완료 후 서로 다른 행동 변화를 주는 복수의 Policy Violation 후보 중 하나를 선택할 수 있어야 한다. 정확한 제시 수와 전체 pool 수는 별도 Draft다. | AT-ROGUE-001 | DEC-G1-009 |
| PR-F-019 | VERTICAL SLICE | SHOULD | DRAFT | 선택된 대표 물체의 제한된 subset은 무게 계층 외의 행동 차이를 드러내는 데이터 기반 특성을 가질 수 있어야 한다. | AT-PROP-001 | DEC-G1-010 |
| PR-F-020 | P2 | SHOULD | DEFERRED | 승인된 후속 범위에서 중요한 causal event를 Incident Report로 재구성할 수 있어야 한다. 현재 G1, Prototype Core 또는 Vertical Slice의 차단 조건이 아니다. | DORMANT — 승격 전 활성 AT 없음 | DEC-G1-011 |
| PR-F-021 | VERTICAL SLICE | COULD | DRAFT | Records Archive는 지역 한정 after-hours 조명 변주를 제공할 수 있다. 이는 Global Lighting 또는 Color Grading 기준이 아니다. | LGT_01 Visual Review | DEC-G1-012 |
| PR-F-022 | OUT | COULD | DRAFT | 별도 제품 범위 승인이 있을 경우에만 희귀 scripted finisher에서 익명의 Finisher-only 보조 직원이 참여할 수 있다. 이는 CHR_02, 지속 Party 또는 일반 전투의 두 번째 Active Ally를 승인하지 않는다. | AT-FIN-001 — DORMANT / NON-BLOCKING | DEC-G1-013 |
| PR-F-023 | VERTICAL SLICE | MUST | DRAFT | 플레이어는 Normal Catch와 구분되는 선택적 Perfect Catch 결과를 수행할 수 있어야 한다. 정확한 판정 창, Contact Freeze와 Momentum 값은 별도 Draft다. | AT-CATCH-002 | DEC-G1-004 |
| PR-F-024 | VERTICAL SLICE | SHOULD | DRAFT | 승인된 조건을 충족하면 제한된 Structural Feature가 국소 시야, 통로 또는 공간 상태를 의미 있게 바꿀 수 있어야 한다. | AT-STRUCT-001 | DEC-G1-006 |

## Numerical and Implementation Boundary

Decision Source: `DEC-G1-016`

| Exact-Value Category | Later Owning Gate | Proposed Treatment |
|---|---|---|
| interaction range | G2 | Draft starting value |
| catch timing | G2 | Draft starting value |
| Contact Freeze | G2 | Draft tuning profile |
| Chain depth | G2 | Draft rule and tuning profile |
| Momentum and Pip values | G2 | Draft resource profile |
| Heavy cost | G2 | Draft resource profile |
| room-state-change count | G4 | Graybox success criterion |
| Policy Violation presentation count | G2 | Draft interaction profile |
| total upgrade-pool count | G5 | Production scope decision |
| enemy count | G5 | Production scope decision; G6 performance validation |
| dynamic physics and debris budgets | G3 | Technical budget |
| performance targets | G3/G6 | Technical budget followed by Slice criterion |
| playtest targets | G4/G6 | Graybox or Slice success criterion |

모든 수치와 threshold는 위 Gate에서 별도 제안·승인될 때까지
`DRAFT`다. 입력 매핑, animation, AI, physics, camera와 Unity 구현도
이 G1 Proposed Baseline의 승인이 아니다.

## Gate and Phase Boundary

Decision Source: `DEC-G1-019`

- 이 문서는 G1 통과 기록이 아니라 `PROPOSED / DRAFT`다.
- Owner가 최종 G1 Baseline을 별도로 승인한 뒤에만 Phase label을
  검토할 수 있다.
- G1 승인 자체는 Phase를 자동 변경하지 않는다.
- G2 Design, Graybox, Full Design Lock과 Unity 구현에는 각각 별도
  Gate와 Owner 승인이 필요하다.
- 현재 Phase는 `PRE-PRODUCTION / PRODUCTION VISUAL LOCK`이며
  Implementation Gate는 `CLOSED`다.

## Excluded Requirements

| ID | Applicability | Priority | Authority | Excluded Scope | Rationale | Decision Source |
|---|---|---|---|---|---|---|
| PR-X-001 | OUT | WON'T | DRAFT | 네트워크 Co-op | 물리 동기화와 제작 위험 | DEC-G1-014 |
| PR-X-002 | OUT | WON'T | DRAFT | 모든 벽의 임의 파괴 | 성능·레벨 설계·QA 비용 | DEC-G1-014 |
| PR-X-003 | OUT | WON'T | DRAFT | 완전 절차 생성 방 | 의도된 물리 기회와 충돌 | DEC-G1-014 |
| PR-X-004 | OUT | WON'T | DRAFT | 총기 중심 전투 | 사무용품 전투 정체성 훼손 | DEC-G1-014 |
| PR-X-005 | OUT | WON'T | DRAFT | 염력·마법·초능력 | 승인된 물리 연결 정체성과 충돌 | DEC-G1-014 |
| PR-X-006 | OUT | WON'T | DRAFT | 동료 영구 사망·호위 실패 | 통쾌함을 호위 스트레스로 전환 | DEC-G1-014 |
| PR-X-007 | OUT | WON'T | DRAFT | 모바일 동시 출시 | 입력·성능·QA 범위 초과 | DEC-G1-014 |

모든 `WON'T / OUT` 항목은 재검토 전에 승인된 Change Request가
필요하다.
