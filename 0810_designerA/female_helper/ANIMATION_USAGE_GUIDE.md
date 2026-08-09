# Female Helper (CHR01 Ally 후보) — Animation Usage Guide

베이스: `female_helper.fbx` (리깅+텍스처 완료, 65 bone Standard Skeleton, mesh 5개(LOD0~4) 유지).
검증: 33개 클립 전부 fcurve 회전 점프 재검사 clean. Root motion 대부분 존재 확인(아래 표에 반영), main 때처럼 in-place 베이크 필요.

## 상태별 매핑

### Locomotion — 공용 세트 (main과 동일 라이브러리에서 받음)

| 파일 | 프레임 | 용도 |
|---|---|---|
| walking.fbx | 32 | 걷기 |
| running.fbx | 22 | 달리기 |
| run to stop.fbx | 25 | 달리다 정지 |
| left turn.fbx / right turn.fbx | 32 / 30 | 제자리 회전 |
| jumping up.fbx | 8 | 점프 |
| falling idle.fbx / falling to roll.fbx / hard landing.fbx | 22 / 45 / 61 | 낙하~착지 |
| idle.fbx, idle (2)~(5).fbx | 116~298 | 대기 5종 (main과 동일 이슈 — 아래 중복 항목 참고) |
| stand to cover.fbx / stand to cover (2).fbx | 35 / 38 | 엄폐 진입 |
| cover to stand.fbx / cover to stand (2).fbx | 46 / 35 | 엄폐 해제 |
| crouched sneaking left/right.fbx | 39 / 34 | 크라우치 이동 |
| left/right cover sneak.fbx | 35 / 34 | 엄폐 밀착 이동 |

### Female Helper 전용 (female_helper@ 접두사)

| 파일 | 프레임 | 포즈 확인 | 용도 |
|---|---|---|---|
| female_helper@Pull Heavy Object Start.fbx | 42 | 손 뻗어 잡으러 가는 자세 | 당기기 시작 |
| female_helper@Pull Heavy Object.fbx | 34 | 뒤로 기대며 당기는 루프 | 당기기 지속(루프) — **Cargo Strap 코어 메카닉에 제일 적합** |
| female_helper@Pull Heavy Object Stop.fbx | 41 | 몸 기울인 채 놓는 동작 | 당기기 종료 |
| female_helper@Pulling A Rope.fbx | 443 | (main과 동일 계열) | 로프형 당기기 참고용, 위 3종이 더 적합 |
| female_helper@Sumo High Pull.fbx | 121 | 서서 손 목 근처로 당김 | 무거운 물체 들어올리기 |
| female_helper@Sumo High Pull (1).fbx | 79 | 숙여서 당기는 자세 | 위와 다른 리프팅 변형(중량물 초반 자세) |
| female_helper@Change Direction.fbx | 66 | (렌더 확인 안 됨, root motion 큼) | 이동 중 방향 전환 |
| female_helper@Dismissing Gesture.fbx | 68 | 팔 옆으로 뻗는 손짓 | 대화/거절 제스처 (컷신·상호작용용) |
| female_helper@Jog In Circle.fbx | 179 | (렌더 확인 안 됨, 원형 이동이라 카메라 밖으로 나감) | 제자리 대기 순찰 루프 |
| female_helper@Running.fbx | 39 | — | `running.fbx`와 별개 클립(프레임 다름), 동료 전용 달리기로 사용 가능 |
| female_helper@Stand To Cover.fbx | 35 | — | `stand to cover.fbx`와 **완전 중복**(아래 참고) |

## 중복 후보 (삭제 안 함, 판단 요청)

1. **`stand to cover.fbx` ↔ `female_helper@Stand To Cover.fbx`**: world-space 이동량 수치 완전히 동일(x=0.163 y=1.300 z=0.162) — 같은 클립이 이름만 다르게 두 번 들어옴. 하나 삭제 추천.
2. **idle 5종**: main과 동일한 문제. idle.fbx(기본) + idle(2).fbx(경계) 2개로 축소 추천, 나머지는 판단 필요.
3. **Pull Heavy Object 3종(Start/loop/Stop) vs Pulling A Rope**: Pull Heavy Object 세트가 시작-루프-종료 구조 갖춘 완성형이라 Cargo Strap 메카닉엔 이쪽이 더 적합함. Pulling A Rope는 참고용으로만 두고 실사용은 Pull Heavy Object 권장.
4. **Sumo High Pull / Sumo High Pull (1)**: 이름 비슷하지만 포즈 다름(하나는 선 자세, 하나는 숙인 자세) — 리프팅 동작의 다른 단계로 보임, 중복 아닐 가능성 큼.

## 기술적 확인 필요

- **Root Motion**: main과 동일하게 거의 모든 클립에 root motion 있음. Unity 통합 전 in-place 베이크 필요(main에 썼던 스크립트 그대로 재사용 가능).
- Rig: 65 bone Standard Skeleton (main은 41 bone, 2 Chain Fingers였음) — **본 개수 다름**. CHR00(main)과 CHR01(helper) 리그 컨벤션 통일 필요한지 확인.
- female_helper@ 접두사 클립들은 act_bones=52 (armbones=65 중 일부만 애니메이트, 손가락 등 정지) — 정상, 문제 아님.
