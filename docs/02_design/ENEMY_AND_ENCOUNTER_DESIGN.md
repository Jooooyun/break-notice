# Enemy and Encounter Design

> **MIXED AUTHORITY**
>
> `PRT-RM-001`과 `ENY-P-001`의 Prototype 적용은
> [`PROTOTYPE_CONTENT_LOCK.md`](PROTOTYPE_CONTENT_LOCK.md)의
> `OWNER-APPROVED G2 PROTOTYPE CONTENT LOCK`을 따른다. Vertical Slice
> enemy, room과 encounter 확장안은 계속 `DRAFT`다.

## 1. 적 설계 원칙

- 적은 HP 자루가 아니라 물리 위치와 패스 경로를 바꾸는 존재다.
- 정면 투척 하나로 모든 적을 해결하지 못하게 한다.
- 공격은 명확한 준비 동작과 취소 가능 창을 가진다.
- 총기 중심 압박은 사용하지 않는다.

## 2. Prototype Enemy

### ENY-P-001 — Compliance Runner

- Prototype functional archetype은 정확히 한 종류다.
- instance는 정확히 두 명이며 둘 다 같은 archetype을 사용한다.
- 평범한 corporate compliance/security pressure로 읽힌다.
- 물리적으로 이동 가능한 causal target이다.
- 읽을 수 있는 close-range pressure pattern 한 가지만 사용한다.
- nonlethal presentation을 유지한다.
- Suppressed 기능은 비활성이다.

정확한 얼굴, uniform, final identity, Stability, stats, attack timing,
reaction, ragdoll, AI, navigation과 Unity 구현은 `DRAFT`다.

역사적 문구 `Prototype: 3명`은
`SUPERSEDED PROTOTYPE DRAFT`다. 현재 수량은 두 명이다.

Prototype에는 Guard, Asset Mover, Interceptor, Elite, boss, ranged
enemy, firearm, shield, suppression specialist 또는 두 번째 enemy
archetype을 넣지 않는다.

### Prototype Room Boundary

- `PRT-RM-001 — Open Operations Prototype Room` 정확히 한 개;
- CHR_00 한 명, CHR_01 한 명, Compliance Runner 두 명;
- 총 인간 actor 네 명;
- multiple room, room transition과 procedural layout 없음.

상세 content는
[`PROTOTYPE_CONTENT_LOCK.md`](PROTOTYPE_CONTENT_LOCK.md)를 따른다.

## 3. Vertical Slice Enemy

이 절 전체는 `DRAFT CANDIDATES`다.

### Partition Guard

- 이동식 파티션·화이트보드 방패
- 정면 Light/Medium을 방어
- Rolling 다리 공격, 측면, 환경 충돌에 약함

### Asset Mover 또는 Interceptor

둘 중 하나만 버티컬 슬라이스에 채택한다.

- Asset Mover: 플레이어 탄약 위치를 바꿈
- Interceptor: 동료 Light 패스를 가로막음

위 이름, identity, 수량과 포함 여부는 `DRAFT`이며 Prototype content에
포함되지 않는다.

## 4. Encounter Rules — Vertical Slice Draft

다음 역사적 목록은 Vertical Slice room design candidate이며
`PRT-RM-001`의 필수 구성으로 사용하지 않는다.

- 즉시 사용 가능한 Light 2개 이상
- 주력 Medium 2개 이상
- Momentum 이후 Heavy 1개
- 충돌 표면 2개
- Structural Feature 1개
- 동료 Relay Position 2개
- 방해받을 수 있는 패스 경로
- 전투 후 열리는 경로 또는 시야

## 5. Vertical Slice Rooms — Draft Candidates

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

위 세 방, actor 수, object 구성과 목표는 승인된 Prototype room과
분리된 `DRAFT` 후보다. `PRT-RM-001`은 이 중 어느 방의 final 또는
Vertical Slice version도 아니다.

## 6. 방 클리어 후 검증 — Later Draft

- 시작과 다른 오브젝트 배치
- 열린 길 또는 사라진 엄폐
- 사용된 동료 재고
- 남은 재사용 탄약
- 최소 한 번의 2-hop 이상 Chain 기회

Prototype에서는 drawer의 side-lane clearance와 Reset까지 유지되는
국소 room-state change만 content-level 필수 결과로 승인한다. 정확한
검증 수치와 final encounter-clear 조건은 계속 `DRAFT`다.
