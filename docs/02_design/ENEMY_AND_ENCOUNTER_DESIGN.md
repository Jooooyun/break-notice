# Enemy and Encounter Design

## 1. 적 설계 원칙

- 적은 HP 자루가 아니라 물리 위치와 패스 경로를 바꾸는 존재다.
- 정면 투척 하나로 모든 적을 해결하지 못하게 한다.
- 공격은 명확한 준비 동작과 취소 가능 창을 가진다.
- 총기 중심 압박은 사용하지 않는다.

## 2. Prototype Enemy

### Compliance Runner

- 근접 접근
- 낮은 Stability
- 명확한 한두 가지 공격
- Medium에 잘 이동
- 다른 적·가구와 충돌하기 좋은 체형

Prototype: 3명.

## 3. Vertical Slice Enemy

### Partition Guard

- 이동식 파티션·화이트보드 방패
- 정면 Light/Medium을 방어
- Rolling 다리 공격, 측면, 환경 충돌에 약함

### Asset Mover 또는 Interceptor

둘 중 하나만 버티컬 슬라이스에 채택한다.

- Asset Mover: 플레이어 탄약 위치를 바꿈
- Interceptor: 동료 Light 패스를 가로막음

## 4. Encounter Rules

모든 전투방은 최소 포함:

- 즉시 사용 가능한 Light 2개 이상
- 주력 Medium 2개 이상
- Momentum 이후 Heavy 1개
- 충돌 표면 2개
- Structural Feature 1개
- 동료 Relay Position 2개
- 방해받을 수 있는 패스 경로
- 전투 후 열리는 경로 또는 시야

## 5. Vertical Slice Rooms

### Room 1 — Open Operations Office

- Runner 3
- Facilities Mover 1
- 의자·모니터·파티션·유리
- 목표: 기본 패스와 Chain

### Room 2 — Copy and Meeting Hub

- Runner 3, Partition Guard 1
- 복사기·화이트보드·유리·회의실 테이블
- 목표: 시야 변화, 토너, Heavy

### Room 3 — Records Archive

- Runner 3~4, Guard 1, Elite 후보 1
- 이동식 기록 선반·카트·캐비닛
- 목표: 좁은 통로, Momentum 관리, 야간 조명 변주

## 6. 방 클리어 후 검증

- 시작과 다른 오브젝트 배치
- 열린 길 또는 사라진 엄폐
- 사용된 동료 재고
- 남은 재사용 탄약
- 최소 한 번의 2-hop 이상 Chain 기회
