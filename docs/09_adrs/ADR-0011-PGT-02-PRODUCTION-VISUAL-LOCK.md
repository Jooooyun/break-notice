# ADR-0011: PGT_02 Chair Redirect Core Loop Production Visual Lock v010

- Status: Accepted
- Date: 2026-07-17

## Decision

`references/approved/gameplay/PGT_02_Chair_Redirect_Core_Loop_v010_FINAL.png`를 PGT_02 Chair Redirect Core Loop Production Visual Canon으로 승인한다.

## Canon

- 승인된 동료가 바닥 지지 의자를 물리적으로 공급하고 완전히 놓는다.
- 플레이어가 GEAR_01의 연속된 물리 연결을 통해 최종 방향 전환의 주체가 된다.
- `허리 릴 → 검정·황색 스트랩 → 은색 카라비너 → 교체식 금속 클램프 → 하중 지지 의자 금속 Anchor` 연결이 보인다.
- 의자는 바닥 접촉을 유지한다.
- 스트랩 장력선과 의자의 새 진행 방향이 구분된다.
- 플레이어의 체중, 스탠스, 골반·상체 회전과 브레이스 자세로 리디렉션이 읽힌다.
- 장면은 목표와 충돌하기 전에 끝난다.

세부 Canonical Elements는 `docs/03_art/CHAIR_REDIRECT_VISUAL_SPEC.md`를 따른다.

## Non-Canon

- 정확한 입력, 거리, 각도, 속도, 힘, 장력, 마찰과 타이밍
- 자동 연결·해제, 애니메이션, AI와 물리값
- Rigidbody, Joint, IK, Cinemachine과 Unity 구현
- 정확한 방 구조, 조명, 목표 위치와 외형
- 의자 모델, 캐스터 각도, 배우 위치와 세부 포즈

## Consequences

- PGT_02의 문서화된 Visual Canon은 `LOCKED`이며 상세 기능 동작은 `DRAFT`다.
- 다음 Production Visual 대상은 `PGT_03 — Chain Reaction Aftermath`다.
- PGT_03는 PGT_02의 문서화된 Canonical Elements만 continuity constraint로 사용할 수 있다.
- PGT_02 이미지의 정확한 방·조명·목표·의자·캐스터·배우 위치와 세부 포즈는 PGT_03 Canon으로 자동 승격되지 않는다.
- PGT_03의 충돌, Knockback, 파괴, Chain Reaction과 Aftermath는 이 ADR에서 승인하지 않는다.
- PGT_03 이미지 생성 또는 gameplay behavior는 별도 preflight와 owner 승인이 필요하다.
- Implementation Gate는 `CLOSED`로 유지한다.
