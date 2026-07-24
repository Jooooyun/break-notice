# Momentum and Causal Chain

> **MIXED AUTHORITY — G2 MOMENTUM/PIP PROFILE APPROVED; CHAIN DESIGN DRAFT**
>
> Momentum/Pip의 current authority는
> [`MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md`](MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md)를
> 따른다. 이 문서의 Chain sections는 후속 Owner 결정 전까지
> `DRAFT`이며 G3 runtime structure나 Unity 구현을 승인하지 않는다.

### Owner-Approved Through TASK-0039

- Momentum/Pip resource model;
- starting reward table and precedence;
- Pip threshold `100` and maximum capacity `3`;
- HeavyAction starting cost `2 Banked Pips`;
- resource persistence and Reset behavior;
- no passive decay and no ordinary-damage resource loss;
- `100 Momentum` per-origin reward cap;
- Heavy/Structural anti-self-funding boundary.

### Still Draft

- Chain creation, propagation and termination;
- depth, hop count and TTL;
- duplicate suppression and exact result uniqueness;
- runtime lineage fields and data structures;
- UI, VFX and audio;
- animation, physics and implementation.

## 1. 목표

“멋있게 싸워라”를 점수 문구가 아니라 자원 보상으로 강제한다. 직접 타격보다 후속 충돌, 환경 변화, 동료 릴레이가 더 큰 Momentum을 준다.

## 2. Momentum/Pip Starting Profile

Owner-approved 시작 profile은 다음 계약의 정확한 heading을 따른다.

- [`## Starting Values`](MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md#starting-values)
- [`## Momentum-to-Pip Conversion`](MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md#momentum-to-pip-conversion)
- [`## Base Reward Categories`](MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md#base-reward-categories)
- [`## Reward Precedence`](MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md#reward-precedence)
- [`## Ally Relay Modifier`](MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md#ally-relay-modifier)
- [`## Perfect Catch Modifier`](MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md#perfect-catch-modifier)
- [`## Per-Origin Reward Cap`](MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md#per-origin-reward-cap)
- [`## HeavyAction Cost and Spend`](MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md#heavyaction-cost-and-spend)
- [`## Resource Persistence and Reset`](MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md#resource-persistence-and-reset)

이전의 direct reward `3`, `5`, `7`, `12`, `20`, Perfect Catch `+8`,
Structural Feature `3 Pip`, damage-based gauge loss와 “구조는 고정”이라는
문구는 모두 `SUPERSEDED HISTORICAL DRAFT`다. 현재 active authority가
아니며 후속 Chain 또는 tuning 결정의 근거로 사용하지 않는다.

현재 시작값의 조정은 G4/G6 evidence와 명시적 revision이 필요하다.
Structural Feature cost는 승인되지 않았고 HeavyAction-origin result는
Momentum을 만들지 않는다.

## 3. Throw Lineage Data

> `DRAFT — NOT APPROVED BY TASK-0039`

각 원인에는 최소 다음 런타임 정보가 있다.

- Chain ID
- Source Actor
- Origin Object
- Current Depth
- Last Impact Time
- Expiration Time
- Participating Entity IDs
- Tags: AllyPass, PerfectCatch, Redirect, Heavy, Structural

## 4. 전파 규칙

> `DRAFT — NOT APPROVED BY TASK-0039`

1. 플레이어 투척·리디렉션에서 Chain 생성
2. 물체가 적을 이동시키면 적에게 계보 전달
3. 적이 가구를 움직이면 가구에 전달
4. 가구가 다른 적·설비를 건드리면 깊이 증가
5. 일정 시간 충돌이 없으면 Chain 종료
6. 같은 대상의 빠른 반복 충돌은 중복 억제

## 5. UI 피드백

> `DRAFT — NOT APPROVED BY TASK-0039`

- 깊이 1은 별도 선 없음
- 깊이 2 이상에서 짧은 Chain Link
- 효과음 음정 또는 층 증가
- Momentum 게이지 증가
- 특별 조건에서만 회사식 문구 표시

예: `REPLY ALL`, `ASSET REALLOCATION`, `UNSCHEDULED RESTRUCTURING`.

## 6. 디버그 요구

> `DRAFT — NOT APPROVED BY TASK-0039`

- Chain ID 표시 토글
- 깊이와 TTL 표시
- 참가 Entity 목록
- 보상 이벤트 로그
- 순환 전파 감지
- 한 프레임 이벤트 폭주 제한
