# Glossary

> **PROPOSED G1 BASELINE — DECISIONS INCORPORATED, PENDING FINAL OWNER WORDING AND BASELINE APPROVAL**
>
> Decision Source: `DEC-G1-001`, `DEC-G1-018`
>
> 아래 요구사항 관리 용어는 Owner가 최종 문구와 G1 Baseline을
> 승인하기 전까지 `DRAFT`다. 기존에 별도 승인된 Visual Canon의
> `LOCKED` 상태를 변경하지 않는다.

| 용어 | 정의 |
|---|---|
| Applicability | 요구사항이 처음 적용되거나 검토되는 범위. Priority 및 Authority와 독립된 축 |
| PROTOTYPE CORE | 첫 Graybox에서 핵심 인과 Gameplay를 한 방 규모로 검증하는 적용 범위 |
| VERTICAL SLICE | Prototype Core 이후 제품 품질과 제작 경로를 검증하는 제한된 Slice 적용 범위 |
| P2 | 현재 Prototype Core와 Vertical Slice를 막지 않는 후속 적용 단계 |
| OUT | 현재 승인된 제품 및 Vertical Slice 적용 범위 밖 |
| Priority | 해당 Applicability 안에서의 중요도. `MUST`, `SHOULD`, `COULD`, `WON'T`만 사용 |
| MUST | 해당 적용 범위의 목적 달성에 필수 |
| SHOULD | 중요하지만 범위·위험 결정에 따라 축소 가능한 후보 |
| COULD | 별도 Owner 범위 승인 전에는 비차단 선택 후보 |
| WON'T | 현재 승인 기준선에서는 구현하지 않으며 재검토에 Change Request가 필요한 항목 |
| Authority | 문구와 범위 결정의 권한 상태. `DRAFT`, `DEFERRED`, `REJECTED`, `SUPERSEDED` 사용 |
| DRAFT | 검토 중이며 아직 최종 Owner Baseline 승인을 받지 않은 상태 |
| DEFERRED | 후속 단계나 별도 범위 결정 전까지 활성 기준선에서 보류된 상태. Priority 값이 아님 |
| REJECTED | Owner 결정으로 채택되지 않은 상태 |
| SUPERSEDED | 더 높은 권한 또는 후속 승인 기록으로 대체된 상태 |
| Trace Status | 요구사항과 근거·설계·기술·검증 연결의 성숙도. Authority와 독립된 축 |
| UNMAPPED | 요구사항에 필요한 추적 연결이 아직 없음 |
| PARTIAL | 일부 연결은 있으나 후속 Gate 또는 자료가 필요 |
| MAPPED | 현재 의도한 근거와 검증 경로가 연결됨. 승인이나 검증 완료를 의미하지 않음 |
| VERIFIED | 승인된 기준에 따라 연결과 결과가 실제로 확인됨 |
| Decision Source | Proposed 문구나 분류 변경을 발생시킨 Owner 결정 ID |
| Proposed G1 Baseline | Owner 결정 방향을 반영했지만 정확한 문구와 G1 통과가 아직 승인되지 않은 요구사항 후보 |
| Scoop | 플레이어 근처의 물체를 직접 빠르게 집는 행동 |
| Cargo Strap | 허리 릴에서 나오는 검정·황색 산업용 화물 스트랩 |
| Snag / Redirect | 승인된 GEAR_01 연결 체인을 하중 지지 금속 Anchor에 연결해 바닥 지지 물체의 진행 방향을 바꾸는 후보 행동 |
| Call & Catch | 동료에게 물체 전달을 요청하고 실제 궤적으로 받아 사용하는 행동 |
| Normal Catch | 넓은 판정으로 자동 보조되는 기본 캐치 |
| Perfect Catch | Normal Catch와 구분되는 선택적 timing 결과. 판정 창, Contact Freeze와 Momentum 값은 Draft |
| Queued Throw | 물체가 도착하기 전 투척을 준비해 캐치 후 멈추지 않고 던지는 기능 |
| Relay Position | 동료가 자유 추적 대신 사용하는 안전한 고정 지원 위치 |
| Suppressed | 적 압박으로 동료의 패스가 일시 정지된 상태 |
| Light | 공격 차단·정밀 세팅용 소형 물체 |
| Medium | 적과 가구를 이동시켜 연쇄를 만드는 주력 물체 |
| Heavy | Light·Medium과 구분되는 질량 계층 후보. 사용 조건·비용·공간 변화는 별도 Draft |
| Momentum | 후속 충돌과 팀 릴레이로 얻는 피니셔 자원 |
| Momentum Pip | Momentum을 저장하는 후보 단위. 정확한 최대치와 비용은 Draft |
| Causal Chain | 최초 플레이어 행동에서 파생된 충돌 계보 |
| Throw Lineage | 물체와 적 사이에 전달되는 Causal Chain 런타임 정보 |
| Stability | 적의 균형·자세 유지 능력 |
| Contact Freeze | 충돌 순간의 짧은 시각·애니메이션 정지 |
| Impulse Release | Contact Freeze 직후 위치와 물체가 강하게 이동하는 단계 |
| Structural Feature | 파괴되면 길·시야·통로를 바꾸는 방별 구조 요소 |
| Incident Report | 가장 인상적인 연쇄를 짧게 재생하고 결과를 회사 문서처럼 요약하는 기능 |
| Canon | 실제 구현과 문서에서 반드시 유지해야 하는 승인 정보 |
| Non-Canonical | 참고는 가능하지만 구현 요구사항이 아닌 이미지의 우연한 요소 |
