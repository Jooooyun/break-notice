# ADR-0010: PGT_01 Gameplay Camera Production Visual Lock v002

- Status: Accepted
- Date: 2026-07-17

## Decision

`references/approved/gameplay/PGT_01_Gameplay_Camera_v002_FINAL.png`를 PGT_01 Gameplay Camera Production Visual Canon으로 승인한다.

## Canon

- 실제 3인칭 Gameplay View
- 약간 높은 Over-the-Shoulder 방향
- 플레이어의 상대적 화면 비중
- 전신 또는 거의 전신 가시성
- 바닥과 물체 진행 공간의 가독성
- 플레이어·동료·적 구분
- 사용 가능 물체와 복수 이동 경로의 가독성

세부 Canonical Elements는 `docs/03_art/CAMERA_SPEC.md`를 따른다.

## Non-Canon

- 정확한 카메라 거리, 높이, FOV, damping, collision과 occlusion
- 카메라 상태 전환, Camera Shake, Cinemachine과 Unity 구현
- 정확한 방 구조, 적 외형과 인원수, 소품 모델·배치·수량
- 동료 위치, Relay Position, 조명과 collision surface
- 이미지의 얼굴·머리·주름·마모·미세 비대칭과 렌더링 아티팩트

## Consequences

- PGT_01은 `LOCKED` Production Visual이다.
- 다음 Production Visual 대상은 `PGT_02 — Chair Redirect Core Loop`다.
- PGT_02는 PGT_01의 Canonical Elements만 continuity constraint로 사용한다.
- PGT_01 이미지의 정확한 방·적·소품·동료 위치·조명은 PGT_02 Canon으로 자동 승격되지 않는다.
- PGT_02 이미지 생성 또는 gameplay behavior는 별도 preflight와 owner 승인이 필요하다.
- Implementation Gate는 `CLOSED`로 유지한다.
