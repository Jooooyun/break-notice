# Performance Budget

## Target

- 1920×1080
- 60 FPS 목표
- Windows PC
- 정확한 최소/권장 사양은 Vertical Slice 프로파일링 후 확정

## Runtime Budget Baseline

| 항목 | 일반 목표 | 순간 최대 |
|---|---:|---:|
| 활성 적 | 3~5 | 6 |
| 활성 동료 | 1 | 특수 연출 2 |
| Awake Dynamic Rigidbody | 20~30 | 40 |
| 실제 큰 파편 | 8~15/사건 | 25/방 |
| Structural Feature | 1/방 | 2 |
| 동시 Chain 추적 | 8 | 16 |
| 실제 물리 종이·작은 유리 | 0 | 0 |

## CPU 위험

- Collision callback 폭주
- 복잡한 Convex MeshCollider
- 동료 자유 경로 탐색
- 다중 ragdoll
- 한 프레임 파편 대량 활성화

## GPU 위험

- 투명 유리 중첩
- 실시간 반사
- 과도한 먼지·종이 overdraw
- 다수 동적 그림자
- 영화급 머리카락·피부 셰이더

## Gate

예산을 초과하는 기능은 최적화 약속만으로 승인하지 않는다. 재현 가능한 프로파일링 결과와 대안이 필요하다.
