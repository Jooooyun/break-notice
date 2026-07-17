# Gameplay Camera Specification

## 승인 이미지

![PGT_01](../../references/approved/gameplay/PGT_01_Gameplay_Camera_v002_FINAL.png)

`PGT_01_Gameplay_Camera_v002_FINAL`을 PGT_01 Gameplay Camera Production Visual Canon으로 승인한다.

승인 범위는 이미지와 아래 Canonical Elements에 한정한다. 정확한 카메라 수치, 동작, 방·적·소품·조명 또는 기술 구현은 승인하지 않는다.

## Canonical Elements

- 실제 3인칭 Gameplay View
- 약간 높은 Over-the-Shoulder 방향
- 플레이어의 상대적 화면 비중
- 플레이어 전신 또는 거의 전신 가시성
- 바닥과 물체 진행 공간의 가독성
- Gameplay distance에서의 플레이어·동료·적 구분
- 사용 가능 물체의 가독성
- 복수 이동 경로의 가독성

CHR_00, GEAR_01과 CHR_01은 각 Visual Spec과 ADR에 기록된 Canonical Elements만 따른다.

## Non-Canonical Elements

- 정확한 카메라 거리, 높이와 FOV
- damping, collision, occlusion과 camera state transition
- Camera Shake, FOV punch, motion blur와 camera assist
- Cinemachine 구성과 Unity 구현 방식
- 이미지의 정확한 방 구조와 치수
- 적 외형, 유니폼, 장비, 인원수와 배치
- 소품 모델, 배치, 수량과 Light/Medium/Heavy 분포
- 동료의 정확한 위치와 Relay Position
- 정확한 조명, 노출, 유리 구조와 collision surface 배치
- 얼굴·머리·주름·마모·미세 비대칭과 렌더링 아티팩트

## Candidate Camera Starting Values — Draft

다음 값은 Graybox 검증을 위한 `CANDIDATE` 시작값이며 PGT_01 Visual Canon이나 구현 수치가 아니다.

- 플레이어 뒤 거리: 약 4.5m
- 플레이어 머리 기준 높이: 약 +1.2~1.5m
- FOV: 약 68°

## Draft Functional Behavior — Not Approved by the Visual Lock

### 상태별 변화

| 상태 | Draft 후보 변화 |
|---|---|
| 기본 이동 | 넓은 시야와 부드러운 추적 |
| Aim | 어깨 쪽으로 제한적 접근, 목표 가림 방지 |
| Carry Medium | 약간 뒤로 이동, 물체 실루엣 보정 |
| Carry Heavy | 더 멀어지고 높아짐, 공간과 발 위치 표시 |
| Perfect Catch | 짧은 미세 kick, 자동 회전 금지 |
| Heavy Impact | 짧은 FOV punch와 방향성 kick |
| Ragdoll/Chain | 카메라가 결과를 따라 자동으로 과회전하지 않음 |

### Collision and Occlusion

- 벽 충돌 시 플레이어와 카메라 사이를 축소
- 물체가 화면 중앙과 조준점을 장시간 가리지 않도록 carry offset 사용
- 투명 벽 처리 또는 카메라 충돌 우선순위 정의
- 동료 패스 경로를 카메라가 고의로 가리지 않음

### Accessibility

- Camera Shake 강도
- Impact FOV 효과 On/Off 또는 강도
- Motion Blur On/Off
- Aim Sensitivity
- Horizontal/Vertical Invert
- Auto Camera Assist 강도

위 기능의 정확한 값, 동작과 구현은 상세 설계와 Graybox 검증 전까지 `DRAFT`다.

## Visual Exclusions

- 상시 극단적 로우앵글
- 더치 앵글
- 적에게 자동 잠금되어 주변 물리 기회를 숨기는 구도
- 화면 결과를 읽을 수 없을 정도의 흔들림
- 캐릭터 뒤통수와 물체가 화면 대부분을 차지하는 초근접 OTS
- cinematic depth of field로 gameplay readability를 훼손하는 구도
