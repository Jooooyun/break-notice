# Product Requirements

## 우선순위

- **MUST**: 프로토타입 또는 버티컬 슬라이스의 정체성을 위해 필수
- **SHOULD**: 핵심을 강화하지만 일정 위험 시 축소 가능
- **COULD**: 본 제작 승인 후 고려
- **WON'T**: 현재 제품 범위에서 제외

## Functional Requirements

| ID | 우선순위 | 요구사항 | 검증 |
|---|---|---|---|
| PR-F-001 | MUST | 플레이어는 근거리 물체를 이동을 거의 멈추지 않고 직접 집을 수 있어야 한다. | AT-SCOOP-001 |
| PR-F-002 | MUST | 물체는 실제 공간 경로를 가지며 순간이동하거나 공중 정지하지 않아야 한다. | AT-PHY-001 |
| PR-F-003 | MUST | 동료는 방 안의 실제 물체를 Light는 던지고 Medium은 굴리거나 밀어야 한다. | AT-ALLY-001 |
| PR-F-004 | MUST | 플레이어는 Normal Catch와 Perfect Catch로 동료·적의 투척물을 받을 수 있어야 한다. | AT-CATCH-001/002 |
| PR-F-005 | MUST | 플레이어는 캐치 전에 투척을 예약하여 한 동작으로 다시 던질 수 있어야 한다. | AT-THROW-003 |
| PR-F-006 | MUST | 별도 산업용 화물 스트랩은 연결 가능한 물체의 진행 방향을 짧은 거리에서 바꿀 수 있어야 한다. | AT-STRAP-001 |
| PR-F-007 | MUST | 붉은 넥타이는 어떤 물체에도 연결되지 않아야 한다. | AT-ART-001 |
| PR-F-008 | MUST | Light·Medium·Heavy는 전투 역할과 피드백이 구분되어야 한다. | AT-WEIGHT-001 |
| PR-F-009 | MUST | 최초 플레이어 행동에서 파생된 후속 충돌을 Causal Chain으로 추적해야 한다. | AT-CHAIN-001 |
| PR-F-010 | MUST | 긴 후속 충돌과 동료 릴레이는 Momentum을 더 많이 지급해야 한다. | AT-MOM-001 |
| PR-F-011 | MUST | Momentum Pip을 소비해 Heavy 물체 또는 Structural Feature를 사용해야 한다. | AT-HEAVY-001 |
| PR-F-012 | MUST | 충돌은 적 위치, Stability, 가구 위치, 파손 상태, 통로 중 최소 하나를 변화시켜야 한다. | AT-IMPACT-001 |
| PR-F-013 | MUST | 일반 전투의 활성 동료는 한 명이어야 한다. | AT-SCOPE-001 |
| PR-F-014 | MUST | 동료는 자유 추적 파티가 아니라 Relay Position에서 지원해야 한다. | AT-ALLY-002 |
| PR-F-015 | MUST | 동료는 사망 호위 대상이 아니라 Suppressed 상태를 가져야 한다. | AT-ALLY-003 |
| PR-F-016 | MUST | 방 클리어 후 전투 전과 다른 공간 상태가 남아야 한다. | AT-ROOM-001 |
| PR-F-017 | MUST | 플레이어는 즉시 Reset하여 같은 방을 다시 시험할 수 있어야 한다. | AT-FLOW-001 |
| PR-F-018 | SHOULD | 방 클리어 후 3개의 Policy Violation 중 하나를 선택한다. | AT-ROGUE-001 |
| PR-F-019 | SHOULD | 물체는 무게 외 행동 태그를 하나 이상 가질 수 있다. | AT-PROP-001 |
| PR-F-020 | SHOULD | 가장 강한 연쇄를 Incident Report로 재생할 수 있다. | AT-INC-001 |
| PR-F-021 | COULD | 야간 기록실 등 조명 변주를 제공한다. | Visual review |
| PR-F-022 | COULD | 희귀 합동 테이블 피니셔에서 동료 2명이 참여한다. | AT-FIN-001 |

## Excluded Requirements

| ID | 범위 제외 항목 | 이유 |
|---|---|---|
| PR-X-001 | 네트워크 Co-op | 물리 동기화와 제작 위험 |
| PR-X-002 | 모든 벽의 임의 파괴 | 성능·레벨 설계·QA 비용 |
| PR-X-003 | 완전 절차 생성 방 | 의도된 물리 기회와 충돌 |
| PR-X-004 | 총기 중심 전투 | 사무용품 전투 정체성 훼손 |
| PR-X-005 | 염력·마법·초능력 | 주인공 설정과 핵심 훅 충돌 |
| PR-X-006 | 동료 영구 사망·호위 실패 | 통쾌함을 호위 스트레스로 전환 |
| PR-X-007 | 모바일 동시 출시 | 입력·성능·QA 범위 초과 |
