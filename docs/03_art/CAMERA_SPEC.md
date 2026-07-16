# Gameplay Camera Specification

## 목표

물체를 던지는 손맛과 주변 전장의 읽기 쉬움을 동시에 유지한다. 영화 포스터용 로우앵글과 실제 게임 카메라를 분리한다.

## 기본 카메라 Baseline

- 형식: 약간 높은 3인칭 OTS
- 플레이어 뒤 거리 시작값: 약 4.5m
- 플레이어 머리 기준 높이: 약 +1.2~1.5m
- FOV 시작값: 약 68°
- 주인공 전신 또는 거의 전신 표시
- 바닥과 물체 궤적을 충분히 표시
- 기본 전투에서는 얕은 Depth of Field를 사용하지 않음

수치는 `PGT_01_Gameplay_Camera_Lock` 이미지와 Graybox에서 최종 조정한다.

## 상태별 변화

| 상태 | 변화 |
|---|---|
| 기본 이동 | 넓은 시야와 부드러운 추적 |
| Aim | 어깨 쪽으로 제한적 접근, 목표 가림 방지 |
| Carry Medium | 약간 뒤로 이동, 물체 실루엣 보정 |
| Carry Heavy | 더 멀어지고 높아짐, 공간과 발 위치 표시 |
| Perfect Catch | 짧은 미세 kick, 자동 회전 금지 |
| Heavy Impact | 짧은 FOV punch와 방향성 kick |
| Ragdoll/Chain | 카메라가 결과를 따라 자동으로 과회전하지 않음 |

## Collision and Occlusion

- 벽 충돌 시 플레이어와 카메라 사이를 축소
- 물체가 화면 중앙과 조준점을 장시간 가리지 않도록 carry offset 사용
- 투명 벽 처리 또는 카메라 충돌 우선순위를 명시
- 동료 패스 경로를 카메라가 고의로 가리지 않음

## Accessibility

- Camera Shake 0~100%
- Impact FOV 효과 On/Off 또는 강도
- Motion Blur On/Off
- Aim Sensitivity
- Horizontal/Vertical Invert
- Auto Camera Assist 강도

## 금지

- 상시 극단적 로우앵글
- 더치 앵글
- 적에게 자동 잠금되어 주변 물리 기회를 숨김
- Heavy 충돌 때 화면 전체를 읽을 수 없을 정도의 흔들림
- 캐릭터 뒤통수와 물체가 화면 대부분을 차지하는 초근접 OTS
