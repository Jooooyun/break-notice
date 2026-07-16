# Physics and Destruction Implementation

## Rigidbody Activation

```text
Static/Kinematic/Sleeping
→ 플레이어 접근 또는 영향 예정
→ Dynamic 활성화
→ 일정 시간 저속 유지
→ Sleep 또는 단순 상태로 복귀
```

## Collision Detection Baseline

| 물체 | 시작 모드 |
|---|---|
| 느린 Light | Discrete 또는 Continuous Speculative |
| 빠른 Light | Continuous Dynamic |
| 의자·모니터 | Continuous Speculative, 필요 시 Dynamic |
| 복사기·캐비닛 | Discrete |
| 정적 환경 | Static Collider |

실제 값은 Prototype 프로파일링 후 고정한다.

## Collider Policy

- 책상: 3~5 Box Collider
- 의자: Seat, Back, Base
- 복사기: 1~3 Box Collider
- 파편: Box 또는 단순 Convex
- 비정적 Non-Convex MeshCollider 금지

## Breakable Flow

1. `ImpactResolver`가 누적 또는 단발 임계값 판정
2. 원본 렌더러·콜라이더 비활성화
3. 풀에서 파손 프리팹 활성화
4. 큰 조각에 원본 속도·impact 분배
5. 작은 조각 VFX 실행
6. Lineage와 Chain Depth 전달
7. 열린 통로·NavMesh obstacle 상태 갱신

## Strap Simulation

완전한 Rope Simulation을 사용하지 않는다.

- 시각 스트랩: 단순 세그먼트/렌더러
- 게임 판정: 최대 거리, 장력, 각도, 물체 속도
- 플레이어 회전과 이동에서 보정 impulse 계산
- 과도한 장력에서 자동 Release

목표는 물리적 설득력이지 범용 줄 시뮬레이션이 아니다.

## Ragdoll

- 적 기본 제어는 애니메이션
- 강한 충돌에서 제한적 physical animation 또는 짧은 ragdoll
- 회복 가능한 상태와 전투 불능 상태 분리
- 각속도, 관절, 바닥 관통 안전장치
