# Non-Functional Requirements

> **PROPOSED G1 BASELINE — DECISIONS INCORPORATED, PENDING FINAL OWNER WORDING AND BASELINE APPROVAL**
>
> Document Decision Source: `DEC-G1-015`, with Owner normalization
>
> 아래 문구, 우선순위 표기와 Gate 배치는 최종 Owner wording 및
> G1 Baseline 승인 전까지 Proposed/Draft다. 정확한 수치와 구현
> 방식은 승인하지 않는다.

## Classification Rules

- Priority에는 `MUST`, `SHOULD`, `COULD`, `WON'T`만 사용한다.
- `DEFERRED`는 Priority가 아니라 적용 단계 또는 권한 상태다.
- 적용 단계는 해당 NFR이 처음 요구되는 범위를 나타낸다.
- Owning Gate는 요구사항을 최종 승인하는 Gate가 아니라, 해당
  검증·예산·설계 결정을 소유할 후보 Gate다.
- 모든 행은 Priority, Authority와 Owning Gate를 별도 열로 관리한다.
- `NFR-R-004`는 영속 저장 범위가 승인되기 전 `DEFERRED` Authority다.

## Performance

| ID | Applicability | Priority | Authority | Owning Gate | Qualitative Baseline Intent | Deferred Exact Value or Implementation Note | Validation Method | Decision Source |
|---|---|---|---|---|---|---|---|---|
| NFR-P-001 | VERTICAL SLICE | MUST | DRAFT | G6 | Vertical Slice는 문서화된 대상 환경에서 안정적이고 재현 가능한 성능을 제공해야 한다. | 대상 사양, 해상도와 FPS는 G3 Draft 후 G6 기준으로 검증 | G6 target-build profiling | DEC-G1-015 |
| NFR-P-002 | PROTOTYPE CORE | MUST | DRAFT | G3 | 동시에 활성화되는 동적 물리 오브젝트는 프로파일링과 안전 복구가 가능한 제한된 예산 안에 있어야 한다. | 정확한 활성 오브젝트 수와 component 선택은 G3 Technical Budget | G3 budget review and stress profile | DEC-G1-015 |
| NFR-P-003 | PROTOTYPE CORE | MUST | DRAFT | G3 | 큰 물리 파편은 원인과 결과를 읽을 수 있고 성능을 통제할 수 있는 제한된 예산을 가져야 한다. | 사건·방별 큰 파편 수는 G3 Technical Budget | G3 destruction-budget review | DEC-G1-015 |
| NFR-P-004 | PROTOTYPE CORE | MUST | DRAFT | G3 | 작은 종이·유리·먼지 반응은 핵심 물리와 Gameplay readability를 지배하지 않아야 한다. | Particle, VFX 또는 다른 표현 방식과 예산은 G3 Technical Draft | G3 material-response budget review | DEC-G1-015 |
| NFR-P-005 | VERTICAL SLICE | SHOULD | DRAFT | G5/G6 | 활성 적 수는 승인된 encounter 역할, 가독성과 성능 범위 안에서 제한되어야 한다. | 정확한 적 수는 G5 Scope에서 결정하고 G6에서 검증 | G5 content review and G6 performance profile | DEC-G1-015 normalization |
| NFR-P-006 | VERTICAL SLICE | MUST | DRAFT | G3/G6 | 범위에 포함된 구조 파괴는 허용 불가능한 Frame Spike를 만들지 않아야 한다. | Pooling, staged activation 등은 G3 구현 후보일 뿐 요구 방식이 아님 | G3 spike-budget review and G6 target-build profile | DEC-G1-015 normalization |

## Usability and Readability

| ID | Applicability | Priority | Authority | Owning Gate | Qualitative Baseline Intent | Deferred Exact Value or Implementation Note | Validation Method | Decision Source |
|---|---|---|---|---|---|---|---|---|
| NFR-U-001 | PROTOTYPE CORE | MUST | DRAFT | G4 | 플레이어, 동료, 적과 사용 가능 물체는 Gameplay 거리에서 빠르고 일관되게 구분되어야 한다. | 인지 시간과 성공률은 G4 Graybox Criterion | G4 observational readability playtest | DEC-G1-015 |
| NFR-U-002 | PROTOTYPE CORE | MUST | DRAFT | G4 | 공급 경로와 물체 진행 방향은 UI나 색상 표시에만 의존하지 않고 자세, 운동과 공간 관계로 읽혀야 한다. | 정량 가독성 기준은 G4 Graybox Criterion | G4 causal-direction playtest | DEC-G1-015 |
| NFR-U-003 | PROTOTYPE CORE | MUST | DRAFT | G4 | Medium 물체 사용 중에도 카메라는 목표, 경로와 충돌 표면의 판단을 방해하지 않아야 한다. | occlusion 허용치와 카메라 값은 G2 Draft | G4 camera-readability playtest | DEC-G1-015 |
| NFR-U-004 | VERTICAL SLICE | SHOULD | DRAFT | G6 | 카메라 흔들림, 모션 블러와 FOV 변화는 합리적인 접근성 제어를 제공해야 한다. | 옵션 구성과 조정 범위는 G2 Draft | G6 accessibility review | DEC-G1-015 |
| NFR-U-005 | PROTOTYPE CORE | MUST | DRAFT | G2 | 팀, 물체 무게와 상호작용 가능 여부를 색상만으로 구분하지 않아야 한다. | 실루엣, 명도, 재질과 공간 분리 기준은 G2 Draft | G2 visual-rule review followed by G4 playtest | DEC-G1-015 |

## Maintainability

| ID | Applicability | Priority | Authority | Owning Gate | Qualitative Baseline Intent | Deferred Exact Value or Implementation Note | Validation Method | Decision Source |
|---|---|---|---|---|---|---|---|---|
| NFR-M-001 | PROTOTYPE CORE | MUST | DRAFT | G3 | 시스템 책임과 상태·이벤트 계약은 결합 위험을 검토할 수 있도록 명시적으로 분리되어야 한다. | 인터페이스, event shape와 component 경계는 G3 Technical Draft | G3 architecture review | DEC-G1-015 |
| NFR-M-002 | PROTOTYPE CORE | SHOULD | DRAFT | G3 | 조정 가능한 정의 데이터는 런타임 상태와 분리되어 반복 수정 가능해야 한다. | 구체적인 데이터 asset 기술은 G3 Technical Draft | G3 data-boundary review | DEC-G1-015 |
| NFR-M-003 | PROTOTYPE CORE | MUST | DRAFT | G3 | 공유 정의 데이터에는 현재 체력, Momentum 또는 encounter 상태 같은 런타임 상태를 저장하지 않아야 한다. | 런타임 상태 보관 구조는 G3 Technical Draft | G3 state-ownership review | DEC-G1-015 |
| NFR-M-004 | PROTOTYPE CORE | MUST | DRAFT | G1 | 모든 기능·비기능 요구사항은 정확한 근거 경로와 검증 의도에 추적 가능해야 한다. | 정량값 없음 | G1 RTM completeness and link audit | DEC-G1-015, DEC-G1-018 |
| NFR-M-005 | PROTOTYPE CORE | MUST | DRAFT | G1 | 신규 패키지는 ADR 또는 Change Request 승인 없이 추가하지 않아야 한다. | 정확한 패키지와 버전은 G3에서 별도 승인 | G1 change-control audit | DEC-G1-015 |

## Reliability

| ID | Applicability | Priority | Authority | Owning Gate | Qualitative Baseline Intent | Deferred Exact Value or Implementation Note | Validation Method | Decision Source |
|---|---|---|---|---|---|---|---|---|
| NFR-R-001 | PROTOTYPE CORE | MUST | DRAFT | G4 | 정상 플레이 조건의 빠른 물체는 벽과 유효 대상의 충돌을 건너뛰지 않아야 한다. | 속도 범위와 collision 구현은 G3 Draft | G4 representative trajectory test | DEC-G1-015 |
| NFR-R-002 | PROTOTYPE CORE | MUST | DRAFT | G3 | 비정상 위치·속도·반복 반응 상태는 탐지, 격리 또는 안전 복구할 수 있어야 한다. | 임계값과 복구 방식은 G3 Technical Draft | G3 fault-injection and recovery review | DEC-G1-015 |
| NFR-R-003 | PROTOTYPE CORE | MUST | DRAFT | G4 | Reset은 causal attribution, 자원과 encounter 오브젝트 상태를 기준 상태로 복원해야 한다. | 복원 목록은 G2 Draft, 허용 시간은 G4 Criterion | AT-FLOW-001 and G4 reset repetition test | DEC-G1-015, DEC-G1-017 |
| NFR-R-004 | VERTICAL SLICE | SHOULD | DEFERRED | G5/G6 | 영속 저장 기능이 승인 범위에 들어올 경우 물리 오류가 저장 데이터의 무결성을 훼손하지 않아야 한다. | 저장 범위, schema와 복구 방식은 G5 결정 후 Draft; 편입 시 MUST 승격을 Owner가 재검토 | G5 save-scope review and conditional G6 integrity test | DEC-G1-015 normalization |

## Development Productivity

| ID | Applicability | Priority | Authority | Owning Gate | Qualitative Baseline Intent | Deferred Exact Value or Implementation Note | Validation Method | Decision Source |
|---|---|---|---|---|---|---|---|---|
| NFR-D-001 | PROTOTYPE CORE | MUST | DRAFT | G1 | 한 Codex 작업은 단일 목적과 명시적으로 제한된 파일 범위를 가져야 한다. | 정량값 없음 | task scope and diff audit | DEC-G1-015 |
| NFR-D-002 | PROTOTYPE CORE | MUST | DRAFT | G1 | 구현 전에 요구사항 ID, 검증 의도와 적용 Gate가 존재해야 한다. | 구체 테스트 구현은 G3 이후 | G1 readiness and RTM audit | DEC-G1-015 |
| NFR-D-003 | PROTOTYPE CORE | SHOULD | DRAFT | G3 | 1인 개발자가 핵심 상태와 causal result를 추적할 수 있는 관측성을 제공해야 한다. | 로그, overlay와 debug tool 목록은 G3 Technical Draft | G3 observability review | DEC-G1-015 |
