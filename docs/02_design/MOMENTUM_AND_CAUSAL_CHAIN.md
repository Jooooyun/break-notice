# Momentum and Causal Chain

> **MIXED AUTHORITY — G2 MOMENTUM/PIP PROFILE AND CAUSAL CHAIN LOGICAL
> CONTRACT APPROVED; TECHNICAL AND PRESENTATION DESIGN DRAFT**
>
> Momentum/Pip의 current authority는
> [`MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md`](MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md)를
> 따르고, Causal Chain의 logical authority는
> [`CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md)를
> 따른다. 이 문서는 두 계약의 축약본이며 runtime structure,
> presentation 또는 Unity 구현을 승인하지 않는다.

### Owner-Approved Through TASK-0040

- Momentum/Pip resource model;
- starting reward table and precedence;
- Pip threshold `100` and maximum capacity `3`;
- HeavyAction starting cost `2 Banked Pips`;
- resource persistence and Reset behavior;
- no passive decay and no ordinary-damage resource loss;
- `100 Momentum` per-origin reward cap;
- Heavy/Structural anti-self-funding boundary;
- eligible Causal Origin and fixed provenance;
- Direct Depth `0`, Downstream Depth `1` through `4`;
- maximum propagation depth `4`;
- physical Transfer, Branch, result uniqueness, duplicate and cycle
  prevention;
- multiple-origin non-merge, termination and Reset boundaries.

### Still Draft

- runtime lineage fields, IDs, events and data structures;
- exact natural-termination threshold, grace window and settlement order;
- simultaneous-origin ownership algorithm;
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

## 3. Causal Origin and Result Classification

Owner-approved 전체 기준은 다음 계약 heading을 따른다.

- [`## Eligible Origin Creation`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md#eligible-origin-creation)
- [`## Origin Provenance`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md#origin-provenance)
- [`## Direct and Downstream Classification`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md#direct-and-downstream-classification)
- [`## Meaningful Result Families`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md#meaningful-result-families)
- [`## Result Uniqueness and Duplicate Suppression`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md#result-uniqueness-and-duplicate-suppression)

Direct Result는 Initiating Carrier가 만드는 Depth `0` 결과이고,
Downstream Result는 Transfer를 받은 Attributed Carrier가 만드는
Depth `1`~`4` 결과다. 같은 Origin의 같은 target/result-family
조합은 한 번만 인정한다.

## 4. Propagation and Termination

Owner-approved 전체 기준은 다음 계약 heading을 따른다.

- [`## Propagation Depth`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md#propagation-depth)
- [`## Propagation Eligibility`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md#propagation-eligibility)
- [`## Carrier Assignment and Cycle Prevention`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md#carrier-assignment-and-cycle-prevention)
- [`## Multiple-Origin Conflict`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md#multiple-origin-conflict)
- [`## Branch Behavior`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md#branch-behavior)
- [`## Termination Conditions`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md#termination-conditions)
- [`## Reset Boundary`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md#reset-boundary)

최대 전파 깊이는 `4`다. Branch는 같은 Origin의 cap, modifier,
duplicate와 termination을 공유하며 서로 merge하지 않는다. completed
Reset은 모든 활성 Origin을 종료하고, 종료된 Origin은 다시 열리지
않는다.

### SUPERSEDED TECHNICAL DRAFT CANDIDATES

다음 역사적 runtime field 후보는 Owner-approved logical contract가
아니며 G3에서 재검토하기 전 구현 근거로 사용하지 않는다.

- Chain ID
- Source Actor
- Origin Object
- Current Depth
- Last Impact Time
- Expiration Time
- Participating Entity IDs
- Tags: AllyPass, PerfectCatch, Redirect, Heavy, Structural

TTL, collision callback, timeout, event ordering, duplicate key와
lineage storage도 계속 Draft다.

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
