# Enemy Elite (ENY01 후보) — Animation Usage Guide

베이스: `enemy_elite_RIGGED_textured.fbx` (리깅 완료, 65 bone, mesh 1개(LOD0), 텍스처 5장 임베드 완료 — diffuse/normal/metallic/roughness/emissive). `enemy_elite_UPLOAD_notex.fbx`는 텍스처 입히기 전 중간 산물이라 최종 딜리버리에서 제외함.
검증: 45개 클립 전부 fcurve 회전 점프 재검사 clean. 거의 전부 root motion 있음(in-place idle류 2개만 예외).

## 상태별 매핑

### Locomotion — 기본(비부상)

| 파일 | 프레임 | 용도 |
|---|---|---|
| idle.fbx | 251 | 기본 대기 |
| walking.fbx / walking backwards.fbx | 42 / 42 | 전진/후진 걷기 |
| standard run.fbx / run backwards.fbx | 23 / 17 | 전진/후진 달리기 |
| jump.fbx | 66 | 점프 |
| turn left.fbx / turning right 45 degrees.fbx / left turn 90.fbx / right turn 90.fbx | 28/31/28/29 | 제자리 회전(각도별) |

### Strafe — 옆이동 (8종, 중복 검토 필요)

| 파일 | 프레임 | 포즈 확인 | 계열 |
|---|---|---|---|
| left strafe.fbx | 21 | (카메라 밖, 큰 횡이동) | 빠른 좌측 strafe |
| right strafe.fbx | 21 | (카메라 밖) | 빠른 우측 strafe — left strafe와 이동량 수치 거의 동일(정상적인 좌우 대칭 쌍) |
| left strafe walking.fbx | 32 | 상체 곧게 선 걷기형 strafe | 걷기 속도 좌측 strafe |
| right strafe walking.fbx | 32 | 상체 곧게 선 걷기형 strafe | 걷기 속도 우측 strafe — left와 대칭 쌍 |
| strafe.fbx | 21 | (카메라 밖) | 이름만 있는 범용 strafe |
| strafe (2).fbx | 17 | 크라우치 전투 자세 | 전투 자세 strafe |
| strafe left.fbx | 32 | 크라우치 방어 자세, 옆으로 조준 | 전투 자세 좌측 strafe |
| strafe right.fbx | 44 | — | 전투 자세 우측 strafe |

**정리**: `left/right strafe`와 `left/right strafe walking`은 속도(달리기/걷기) 티어가 다른 정상적인 좌우 쌍 — 4개 다 유지 추천. 반면 `strafe`, `strafe (2)`, `strafe left`, `strafe right` 4개는 이름 규칙이 다르고 위 4종과 목적이 겹침 — 중복 후보 (아래 참고).

### Combat / Weapon

| 파일 | 프레임 | 포즈 확인 | 용도 |
|---|---|---|---|
| rifle aiming idle.fbx | 90 | — | 조준 대기 |
| rifle run.fbx | 22 | — | 소총 든 채 달리기 |
| rifle jump.fbx | 19 | — | 소총 든 채 점프 |
| firing rifle.fbx | 9 | — | 발사 |
| reloading.fbx | 100 | 손 위로 드는 동작 | 재장전 |
| toss grenade.fbx | 91 | 던지는 자세 확인됨 | 수류탄 투척 |
| hit reaction.fbx | 14 | 움찔하며 팔 드는 반응 확인됨 | 피격 리액션 |

**주의**: 게임 캐논(화물 스트랩만, 총기 없음)과 안 맞을 수 있음 — main 때와 같은 이슈. 적 전용이면 문제없지만, 만약 적도 총기 없는 컨셉이면 이 7개 전부 재검토 필요.

### Injured — 부상 상태 세트 (17종)

| 파일 | 프레임 | 용도 |
|---|---|---|
| injured idle.fbx / injured hurting idle.fbx / injured stumble idle.fbx / injured wave idle.fbx | 277/182/150/147 | 부상 대기 4종(정도·제스처 다름) |
| injured walk.fbx / injured walk backwards.fbx | 50 / 51 | 부상 상태 걷기 전/후진 |
| injured walk left turn.fbx / injured walk right turn.fbx | 61 / 56 | 부상 걷기 회전 |
| injured run.fbx / injured run backwards.fbx | 20 / 19 | 부상 달리기 전/후진 |
| injured run left turn.fbx / injured run right turn.fbx | 19 / 20 | 부상 달리기 회전 |
| injured run backwards left turn.fbx / injured run backwards right turn.fbx | 16 / 17 | 부상 후진 달리기 회전 |
| injured run jump.fbx / injured standing jump.fbx | 23 / 58 | 부상 점프(이동중/제자리) |
| injured backwards turn left.fbx / injured backwards turn right.fbx | 23 / 22 | 부상 후진 회전 |
| injured turn left.fbx / injured turn right.fbx | 28 / 22 | 부상 제자리 회전 |

부상 세트는 서로 조합(idle/walk/run × 방향×회전)이 체계적이라 그대로 다 쓰기 적합함 — 압박 상태(pressure)나 스태거(stagger) 표현할 때 이 세트 통째로 사용 권장.

## 중복 후보 (삭제 안 함, 판단 요청)

1. **strafe / strafe (2) / strafe left / strafe right (4개)**: `left strafe`, `right strafe`, `left strafe walking`, `right strafe walking` 4종과 목적 겹침. 저 4종만 남기고 이 4개는 정리 후보.
2. injured idle 4종은 겹치는 것 같지만 실제로 정도/제스처가 다 달라(단순 대기/아파하는 대기/비틀거림/손 흔들기) 중복 아닐 가능성 높음 — 유지 추천.

## 기술적 확인 필요

- **Root Motion**: 거의 전부 있음(2개 idle류만 예외). in-place 베이크 필요.
- Rig: 65 bone (female_helper와 동일 스펙, main은 41 bone — 불일치 재확인 필요).
- **텍스처: 완료**. diffuse/normal/metallic/roughness/emissive 5장 임베드, `enemy_elite_RIGGED_textured.fbx`에 반영됨.
- enemy_normal도 리깅 완료됨. 위 클립들 그대로 리타겟팅해서 재사용함(`character_enemy_normal/` 폴더, 별도 가이드 참고).
