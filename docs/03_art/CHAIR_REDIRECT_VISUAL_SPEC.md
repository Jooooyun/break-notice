# Chair Redirect Core Loop Visual Specification

## 승인 이미지

![PGT_02](../../references/approved/gameplay/PGT_02_Chair_Redirect_Core_Loop_v010_FINAL.png)

`PGT_02_Chair_Redirect_Core_Loop_v010_FINAL`을 PGT_02 Chair Redirect Core Loop Production Visual Canon으로 승인한다.

승인 범위는 이미지와 아래 Canonical Elements에 한정한다. 정확한 게임플레이 동작, 수치, 물리, 애니메이션 또는 기술 구현은 승인하지 않는다.

## Canonical Elements

- 승인된 CHR_01 동료가 바닥 지지 의자를 물리적으로 공급하고 완전히 놓는다.
- 플레이어가 GEAR_01의 연속된 물리 연결을 통해 최종 방향 전환의 주체가 된다.
- `허리 릴 → 검정·황색 스트랩 → 은색 카라비너 → 교체식 금속 클램프 → 하중 지지 의자 금속 Anchor` 연결이 보인다.
- 의자는 바닥 접촉을 유지한다.
- 스트랩 장력선과 의자의 새 진행 방향이 구분된다.
- 플레이어의 체중, 스탠스, 골반·상체 회전과 브레이스 자세로 리디렉션이 읽힌다.
- 장면은 목표와 충돌하기 전에 끝난다.

## Continuity Constraints

- CHR_00, GEAR_01, CHR_01과 PGT_01은 각 Visual Spec과 ADR에 기록된 Canonical Elements만 따른다.
- 암적색 넥타이는 복장 전용이며 릴, 스트랩, 카라비너, 클램프, 의자 또는 다른 물체와 연결되지 않는다.
- 동료는 의자를 물리적으로 공급하고 완전히 놓으며 원격으로 제어하지 않는다.
- 플레이어는 GEAR_01을 통해 최종 리디렉션을 수행하며 염력, 부유, 에너지 케이블 또는 그래플링을 사용하지 않는다.
- 의자 Anchor는 캐스터, 휠 축, 플라스틱 하우징 또는 취약 돌출부가 아닌 하중 지지 금속 구조다.

## Non-Canonical Elements

- 정확한 입력, 거리, 리디렉션 각도와 의자 속도
- 힘, 장력, 마찰, 타이밍과 물리값
- 자동 연결·해제와 정확한 제어 규칙
- 정확한 애니메이션, AI, Rigidbody, Joint, IK와 충돌 해석
- 정확한 카메라 수치, Cinemachine 구성과 Unity 구현
- 이미지의 정확한 방 구조와 조명
- 목표 위치, 외형, 정체성과 정확한 encounter 구성
- 의자 모델과 캐스터 각도
- 플레이어·동료·목표 위치와 세부 포즈
- 렌더링 아티팩트와 우연한 미세 비대칭

## PGT_03 Boundary

PGT_02는 목표와 충돌하기 전 상태에서 끝난다. 충돌, Knockback, ragdoll, 파괴, Chain Reaction, Momentum 효과, 열린 경로와 Aftermath는 `PGT_03 — Chain Reaction Aftermath`의 미승인 범위다.

## Implementation Boundary

이 Visual Lock은 상세 Chair Redirect 기능 설계나 Unity 구현을 승인하지 않는다. Implementation Gate는 `CLOSED`다.
