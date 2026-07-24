# Vertical Slice Scope

> **OWNER-APPROVED G1 APPLICABILITY AND PRIORITY BASELINE**
>
> Document Decision Source: `DEC-G1-001`, `DEC-G1-002`,
> `DEC-G1-004` through `DEC-G1-013`, `DEC-G1-016`, `DEC-G1-019`
>
> Prototype Core, Vertical Slice, SHOULD, Deferred/COULD와 OUT/WON'T의
> Applicability 및 Priority 경계는 G1 기준으로 Owner-approved다.
> 정확한 콘텐츠 수, 플레이 시간, 성능값과 playtest threshold는
> 승인되지 않았으며 G2 통과, Graybox 시작과 제작 약속을 의미하지 않는다.

## 목표

Prototype Core에서 입증한 direct acquisition, physical relay,
Queued Throw, cargo-strap redirect, causal attribution, meaningful
impact, local room-state change와 Reset을 확장하여 실제 제품의
핵심 재미, 시각 방향과 주요 기술 위험을 한정된 Slice에서 검증한다.

## Scope Model

- `PROTOTYPE CORE MUST`: 첫 Graybox에서 먼저 입증
- `VERTICAL SLICE MUST`: Prototype Core 입증 후 Slice에 필수 후보
- `SHOULD`: 일정·기술 위험에 따라 축소 가능한 강화 범위
- `COULD`: 별도 편입 결정 없이는 blocking scope가 아님
- `P2`: 현재 Slice blocking scope 밖
- `OUT / WON'T`: 현재 승인 대상 제품 범위 밖

## Character and Encounter Boundary

- 승인된 CHR_00 플레이어 한 명
- 일반 전투에서 승인된 CHR_01 활성 동료 한 명
- 명확한 역할과 읽을 수 있는 provisional enemy set
- 일반 전투의 persistent multi-ally party 없음
- FIN_01 익명 보조 직원은 Slice actor 또는 CHR_02가 아님

적 역할, 종류, 수, Elite 편입과 actor 배치는 G5 Scope와 G6
Performance Validation 전까지 Draft다.

## Environment Boundary

Slice는 다음 gameplay 기능을 검증할 수 있는 제한된 모듈형
corporate environment 후보를 사용한다.

- direct acquisition과 physical relay;
- Medium 이동 경로와 collision surface;
- causal result와 local room-state change;
- Heavy-object 후보;
- 조건부 Structural Feature 후보;
- 복수의 읽을 수 있는 movement route.

Open Operations Office, Copy and Meeting Hub와 Records Archive는
현재 설계 후보다. 정확한 방 수, 순서, 구조, 통로 치수와 콘텐츠
배치는 G5 전까지 Draft다.

LGT_01은 Records Archive 지역에 한정된 `COULD` 조명 변주이며
Global Lighting 또는 Color Grading 기준이 아니다.

## Object Boundary

- Light와 Medium은 Prototype Core에서 역할 차이를 먼저 검증
- Heavy-object 사용은 `VERTICAL SLICE MUST` 후보
- Structural Feature 사용은 `SHOULD`
- 실제 물체 모델, 종류, 질량, 수와 파괴 예산은 G3/G5 Draft

Prototype Core는 Heavy 후보를 별도 질량 계층으로 읽을 수 있게
유지할 수 있지만, Heavy 사용, Pip 소비와 공간 변화는 요구하지 않는다.

## Prototype Core

Decision Source: `DEC-G1-002`, with the Owner-approved Prototype
Weight-Scope clarification.

첫 one-room Graybox에서 다음 결과만 우선 검증한다.

- PR-F-001 direct near-object acquisition;
- PR-F-002 continuous physical trajectory;
- PR-F-003 one approved active ally physically supplying objects;
- PR-F-004 Normal Catch;
- PR-F-005 Queued Throw;
- PR-F-006 cargo-strap redirect;
- PR-F-007 necktie and gear separation;
- PR-F-008 실제 등장하는 Light/Medium role readability;
- PR-F-009 minimum causal attribution;
- PR-F-012 meaningful impact;
- PR-F-013 one active ally in normal combat;
- PR-F-016 meaningful local room-state change;
- PR-F-017 immediate Reset.

Heavy는 별도 질량 계층 후보로 식별 가능할 수 있으나, 첫 Prototype
Core는 Momentum, Heavy 사용이나 Pip 소비, Structural Feature,
Relay Position 이동, Suppressed, Policy Violation, Incident Report,
Night Archive 조명 또는 Team Finisher를 요구하지 않는다.

## Later Graybox Increments

Decision Source: `DEC-G1-004` through `DEC-G1-007`.

Prototype Core 인과와 반복 흐름이 검증된 뒤 다음을 순차 후보로
추가한다.

- PR-F-023 Perfect Catch;
- PR-F-010 Momentum;
- PR-F-011 Heavy-object access and use;
- PR-F-014 Relay Position movement.

이는 Graybox 시작이나 해당 기능 구현을 현재 허가하지 않는다.

## Vertical Slice Candidate

Decision Source: `DEC-G1-004` through `DEC-G1-007`, `DEC-G1-009`.

다음은 Owner-approved G1 `VERTICAL SLICE / MUST` 범위이다.

- PR-F-023 Perfect Catch;
- PR-F-010 Momentum reward differentiation;
- PR-F-011 Heavy-object use;
- PR-F-014 Relay Position support;
- PR-F-018 서로 다른 행동 변화를 주는 Policy Violation 후보 중
  하나의 선택.

Prototype Core에서 이미 검증한 `MUST` 결과도 Slice에서 회귀 없이
유지해야 한다.

## SHOULD

Decision Source: `DEC-G1-006`, `DEC-G1-008`, `DEC-G1-010`.

- PR-F-024 제한된 Structural Feature use;
- PR-F-015 Suppressed;
- PR-F-019 representative subset의 non-weight Behavior Tags.

## Deferred / COULD

Decision Source: `DEC-G1-011` through `DEC-G1-013`.

- PR-F-020 Incident Report: `P2 / SHOULD / DEFERRED`; G1, Prototype
  Core와 Vertical Slice의 비차단 범위;
- PR-F-021 Night Archive local lighting variant: `VERTICAL SLICE /
  COULD`; Records Archive 지역 후보이며 Global Lighting이 아님;
- PR-F-022 Team Finisher: `OUT / COULD`; 승인된 Vertical Slice와
  최종 제품 범위 밖이며 CHR_02나 지속 두-동료 Party를 만들지 않음.

## Exact Values Deferred

Decision Source: `DEC-G1-016`

다음 값은 이 G1 Scope에서 승인하지 않는다.

- 플레이 시간과 room 수;
- enemy 종류와 수;
- Light, Medium, Heavy와 Structural asset 수;
- Relay Position 수와 배치;
- interaction range와 Catch timing;
- Momentum, Pip와 Heavy 비용;
- Chain depth;
- room-state-change 수;
- Policy Violation 제시 수와 전체 pool 수;
- Rigidbody, debris와 destruction budget;
- target PC, 해상도, FPS와 playtest threshold.

정확한 값은 각각 G2 Draft, G3 Technical Budget, G4 Graybox
Criterion, G5 Scope Freeze 또는 G6 Slice Acceptance에서 결정한다.

## Excluded

- Co-op
- 영구 다수 동료 파티
- 총기 중심 전투
- 염력·마법·초능력
- 모든 벽의 자유 파괴
- 완전 절차 생성 방
- 모바일 동시 출시
- FIN_01의 자동 제품·Slice 편입
- 복잡한 메타 성장, 신규 게임 모드와 요청되지 않은 플랫폼 확대

## Completion Boundary

Decision Source: `DEC-G1-019`

Vertical Slice 완료 후보 조건은 다음과 같다.

- Owner-approved `MUST` 요구사항의 활성 Acceptance Test 통과;
- G3에서 승인된 기술 예산 안의 재현 가능한 성능;
- G4에서 승인된 core-hook, 가독성, causal understanding과 replay 기준;
- G5에서 승인된 actor, environment, object와 feature 범위 준수;
- G6에서 승인된 성능·접근성·외부 검증 기준 통과.

요구사항과 기술 중립적 Acceptance Test 의도는 G1
Owner-approved다. 정확한 수치, G2 논리 계약, G3 구현과 G4/G5/G6
기준은 Draft이며 이 항목들은 G2, G4, G5 또는 G6 통과 선언이 아니다.
