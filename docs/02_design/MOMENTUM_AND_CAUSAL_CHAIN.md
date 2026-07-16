# Momentum and Causal Chain

## 1. 목표

“멋있게 싸워라”를 점수 문구가 아니라 자원 보상으로 강제한다. 직접 타격보다 후속 충돌, 환경 변화, 동료 릴레이가 더 큰 Momentum을 준다.

## 2. Momentum Baseline

- 현재 게이지: 0~100
- 최대 Banked Pip: 3
- 게이지 100에서 Pip 1로 변환
- 일반 Heavy: 2 Pip
- Structural Feature: 3 Pip
- 피격 시 현재 게이지 일부 손실, Pip 유지

초기 보상 예시:

| 사건 | 보상 시작값 |
|---|---:|
| 직접 Light 적중 | 3 |
| 공격 차단 | 5 |
| Medium 직접 적중 | 7 |
| 2-hop Chain | 12 |
| 3-hop Chain | 20 |
| 환경 통로 변화 | 25 |
| Perfect Catch 후 1초 내 적중 | +8 |
| 동료 1명이 포함된 Chain | +5 |

수치는 Graybox 튜닝 대상이며 구조는 고정한다.

## 3. Throw Lineage Data

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

1. 플레이어 투척·리디렉션에서 Chain 생성
2. 물체가 적을 이동시키면 적에게 계보 전달
3. 적이 가구를 움직이면 가구에 전달
4. 가구가 다른 적·설비를 건드리면 깊이 증가
5. 일정 시간 충돌이 없으면 Chain 종료
6. 같은 대상의 빠른 반복 충돌은 중복 억제

## 5. UI 피드백

- 깊이 1은 별도 선 없음
- 깊이 2 이상에서 짧은 Chain Link
- 효과음 음정 또는 층 증가
- Momentum 게이지 증가
- 특별 조건에서만 회사식 문구 표시

예: `REPLY ALL`, `ASSET REALLOCATION`, `UNSCHEDULED RESTRUCTURING`.

## 6. 디버그 요구

- Chain ID 표시 토글
- 깊이와 TTL 표시
- 참가 Entity 목록
- 보상 이벤트 로그
- 순환 전파 감지
- 한 프레임 이벤트 폭주 제한
