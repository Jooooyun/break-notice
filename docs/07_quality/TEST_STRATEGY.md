# Test Strategy

> **OWNER-APPROVED G1 VALIDATION POLICY**
>
> Document Decision Source: `DEC-G1-017`
>
> 다섯 검증 Layer와 활성화 정책은 G1 기준으로 Owner-approved다.
> G3 harness 책임과 Unity Test Framework `1.6.0` 문서 lock은
> Owner-approved다. 테스트 구현·실행, fixture, 정확한 수치와 Build
> Gate 통과는 승인하지 않는다.

## Validation Layers

Decision Source: `DEC-G1-017`

| Layer | Owning Gate | Content |
|---|---|---|
| 1. Technology-neutral behavioral outcome | G1 | 구현 기술과 수치에 독립적인 관찰 결과 |
| 2. Logical input and state contract | G2 | 입력 의도, 상태 전이, 우선순위와 실패 조건 |
| 3. Implementation and technical budget | G3 | component 후보, data/event 계약, physics 접근과 test harness |
| 4. Draft tuning profile | G2/G3 Draft | 거리, timing, force, 비용과 초기 profile. Baseline requirement나 통과 기준이 아님 |
| 5. Success and acceptance threshold | G4/G6 | Graybox의 core-hook·가독성 기준과 Slice의 기능·성능·접근성 기준 |

정확한 거리, timing, force, count와 threshold는 G1 behavioral
wording에 포함하지 않는다.

G5는 위 다섯 검증 Layer와 별도로 실제 actor, asset, room과 feature
수의 Production Scope를 소유한다. G5 수량 결정도 G1 행동 문구가
아니다.

## Test Activation Policy

- `PROTOTYPE CORE MUST`: G4 Graybox 진입 전 활성 검증 대상으로 준비
- `VERTICAL SLICE MUST`: Prototype Core 검증 후 단계적으로 활성화
- `SHOULD`: 범위 편입 시 활성화
- `COULD`: 별도 Owner 범위 승인 전 non-blocking
- `P2 / DEFERRED`: 승격 전 test gap을 blocker로 취급하지 않음
- `WON'T / OUT`: 구현·검증 범위에서 제외

`AT-FIN-001`은 PR-F-022가 `COULD / OUT`인 동안
`DORMANT / NON-BLOCKING`이다. PR-F-020에는 승격 전
`AT-INC-001`을 만들지 않는다.

## Owner-Approved G3 Harness Plan

Decision Source: `ADR-0015`, `DEC-G3-012`

Harness의 semantic owner와 observability plan은 G3 기준으로
Owner-approved다. Unity Test Framework의 documentation lock은
`1.6.0`이지만 test assembly, API, fixture, overlay, CI와 실행 결과는
아직 없다.

| Activation group | Harness ownership | Acceptance-Test mapping |
|---|---|---|
| Prototype logical support | EditMode | state transition, arbitration, safe rejection, identity separation, causal depth/uniqueness/branch/termination, Reset-plan comparison |
| Prototype physical/lifecycle | PlayMode | AT-SCOOP-001, AT-PHY-001, AT-ALLY-001, AT-CATCH-001, AT-THROW-003, AT-STRAP-001, AT-WEIGHT-001, AT-CHAIN-001, AT-IMPACT-001, AT-SCOPE-001, AT-ROOM-001, AT-FLOW-001 |
| Prototype readability | Manual / Playtest | AT-ART-001과 위 Prototype test의 physical readability, agency, camera/route readability, non-telekinetic interpretation |
| Later Vertical Slice | Later EditMode / PlayMode / Manual | AT-CATCH-002, AT-MOM-001, AT-HEAVY-001, AT-ALLY-002, AT-ROGUE-001 |
| Conditional SHOULD | Later only if promoted | AT-ALLY-003, AT-PROP-001, AT-STRUCT-001 |
| Dormant | No active harness | AT-FIN-001 — `DORMANT / NON-BLOCKING` |
| Deferred gap | No test before promotion | PR-F-020; no active AT-INC-001 |

Required observability:

- active logical intent/context와 interaction state;
- ownership;
- Strap state와 Anchor validity;
- Ally state, reservation과 relay phase;
- Origin, parent, carrier, depth와 Result family;
- duplicate/cycle rejection reason;
- room-state flag;
- Reset diff와 subscription count;
- fault, active-body, collision과 frame-spike counter.

Reset은 harness에서도 exclusive lifecycle barrier다. repeated delivery
가능 boundary는 idempotency와 duplicate suppression을 검증한다.

## Candidate Test Pyramid

### EditMode Candidate

- causal attribution 전파·종료·중복 억제
- Momentum 계산과 Heavy 자원 규칙
- 데이터 정의와 런타임 상태 분리
- Cargo Strap의 승인된 논리 제약
- upgrade 선택 규칙

Unity Test Framework `1.6.0`은 exact documentation lock이다. 구체 API,
fixture와 test implementation은 Draft다.

### PlayMode Candidate

- Scoop, Carry, Throw와 Reset의 상태 결과
- Normal Catch, Perfect Catch와 Queued Throw
- 동료 공급 물체의 연속 경로
- 충돌 누락과 비정상 물리 상태 복구
- 국소 room-state change
- 조건부 Breakable 또는 Structural 결과

구체 physics component와 prefab 전환 방식은 G3 Draft다.

### Manual and Playtest Candidate

- 타격감과 causal readability
- 카메라와 경로 가독성
- Light/Medium 역할 구분
- 동료 공급과 플레이어 agency 이해
- 자발적 실험과 Reset 행동

정량 success threshold는 G4 또는 G6에서 정한다.

## Regression Focus

- Reset 후 이벤트 또는 causal attribution 중복
- 무한 causal propagation
- Catch 우선순위 오작동
- Strap 장애물 관통 또는 원격 towing
- 빠른 물체 충돌 누락
- 파손 후 통로·collision 상태 불일치
- Momentum 중복 지급

## Build Gate

Owner가 승인한 활성 `MUST` 요구사항의 blocking test가 실패하면
해당 Gate를 통과할 수 없다.

Acceptance Tests의 기술 중립적 행동 의도는 G1 Owner-approved이고
logical contract는 G2 Owner-approved이며 harness ownership은 G3
Owner-approved다. 어떤 테스트도 구현·실행·통과되지 않았고 G4/G6
임계값과 배포 빌드 권한은 별도 Draft와 Gate에 남는다.
