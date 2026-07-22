# Conference Table Team Finisher Visual Specification

## 승인 이미지

![FIN_01](../../references/approved/gameplay/FIN_01_Conference_Table_Team_Finisher_v003_FINAL.png)

`FIN_01_Conference_Table_Team_Finisher_v003_FINAL`을 FIN_01 Conference Table Team Finisher Production Visual Canon으로 승인한다.

승인 범위는 이미지와 아래 Canonical Elements에 한정한다. FIN_01은 PR-F-022 `COULD` 기능의 Visual-only conditional direction이며 Vertical Slice 또는 최종 제품 범위 편입을 승인하지 않는다.

## Canonical Elements

- 승인된 CHR_00 플레이어 한 명
- 승인된 CHR_01 동료 한 명
- 익명의 Finisher-only 보조 직원 한 명
- 세 사람이 하나의 Heavy 회의 테이블에 직접 접촉해 공동으로 미는 관계
- 중앙 선행 위치에서 최종 방향과 타이밍을 주도하는 CHR_00
- 플레이어를 지원하되 대체하지 않는 CHR_01과 익명 보조 직원
- 세 인물의 양손 접촉, 체중 이동, 발 접지와 상체 기울기로 읽히는 공동 추진
- 현실적인 하중 보조 상태로 읽히는 CHR_00의 외부 전완 브레이스
- 통합 캐스터 구조로 바닥 지지를 유지하는 Heavy 회의 테이블
- 임시 목표와 충돌하기 전에 끝나는 장면
- Stored/Inactive 상태이며 테이블에 연결되지 않는 GEAR_01
- 복장 전용이며 테이블·장비·인물과 연결되지 않는 붉은 넥타이

## Character and Ally-Scope Boundary

- 익명의 보조 직원은 이 Finisher 시각 구성 안에서만 존재한다.
- 익명의 보조 직원은 CHR_02, 일반 전투 Active Ally, 지속 Party Member 또는 승인된 Character Identity가 아니다.
- 익명의 보조 직원은 CHR_00 또는 CHR_01을 복제하지 않으며 플레이어 전용 장비를 사용하지 않는다.
- 일반 전투의 one-active-ally 제한은 유지된다.
- 정확한 얼굴, 복장, 직업, 이름, 성별 표현, 민족성, 연령과 배경은 incidental Non-Canon이다.

## Physical and Continuity Constraints

- 모든 추진 인물은 동일한 테이블에 직접 물리 접촉한다.
- CHR_00은 최종 방향과 타이밍의 시각적 주체이며 두 지원 인물은 이를 대체하지 않는다.
- 테이블은 통합 캐스터 구조로 바닥 지지를 유지한다.
- 장면은 목표 접촉, Knockback, Ragdoll, 피해, 유리 파손, 잔해 또는 Aftermath 전에 끝난다.
- GEAR_01은 Stored/Inactive 상태이며 화물 스트랩·카라비너·클램프가 테이블에 연결되지 않는다.
- 붉은 넥타이는 복장 전용이며 장비·테이블·인물과 연결되지 않는다.
- 염력, 원격 제어, 에너지 케이블, 부유 테이블, 슈퍼히어로 힘, 총기와 전술 장비를 사용하지 않는다.

## Non-Canonical Elements

- 익명 보조 직원의 정확한 얼굴, 복장, 직업, 이름과 신원
- 정확한 테이블 모델, 크기, 질량, 재질, 통합 base 구조와 캐스터 수
- 정확한 목표 수, 외형, 장비, 위치와 간격
- 정확한 카메라 거리, 높이, FOV와 전환
- 정확한 방 구조, 조명, 가구, Collision Surface와 배우 배치
- 정확한 손·발 위치, 체중 이동, 상체 기울기와 세부 자세
- Trigger, 사용 빈도와 Momentum/Pip 비용
- 입력, 속도, 힘, 질량, 마찰과 타이밍
- 정확한 Choreography, Animation 수, AI와 IK
- Rigidbody, Caster Physics, Collision, Cinemachine과 Unity 구현
- 이미지의 주름·마모·미세 비대칭과 렌더링 아티팩트

## Product and Implementation Boundary

- FIN_01은 PR-F-022 `COULD` 상태를 유지한다.
- 이 Visual Lock은 FIN_01을 Vertical Slice, 최종 제품, 구현 우선순위 또는 제작 약속에 편입하지 않는다.
- AT-FIN-001은 구현 허가가 아니다.
- CHR_02 또는 지속 다중 동료 Party는 승인되지 않았다.
- Full Design Lock은 미완료이며 Implementation Gate는 `CLOSED`다.
