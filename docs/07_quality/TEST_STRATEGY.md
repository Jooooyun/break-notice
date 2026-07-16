# Test Strategy

## Test Pyramid

### EditMode

- Momentum 계산
- Chain 전파·TTL·중복 억제
- 데이터 정의 검증
- 장력·허용 각도 계산
- 업그레이드 규칙

### PlayMode

- Scoop/Carry/Throw 상태
- Catch/Perfect Catch/Queued Throw
- 동료 패스 실제 경로
- Rigidbody 충돌과 Stability
- 파손 프리팹 전환
- Reset 상태 복구

### Manual/Playtest

- 타격감
- 카메라 가독성
- 물체 선택 의도
- 동료 시스템 이해
- 재플레이 행동

## Regression Focus

- Reset 후 이벤트 중복
- Chain 무한 순환
- Catch 우선순위 오작동
- Strap 장애물 관통
- 빠른 물체 터널링
- 파손 후 NavMesh/통로 불일치
- Momentum 중복 지급

## Build Gate

MUST 테스트 실패가 있으면 배포 빌드를 만들지 않는다.
