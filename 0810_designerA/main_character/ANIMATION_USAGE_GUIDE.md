# 0810 Designer A — Animation Usage Guide

Source: Mixamo mocap, `main_norig_tex.fbx` 베이스 리그(41 bone, Standard Humanoid) 기준.
검증 방법: 각 클립을 베이스 리그에 적용해 mid-frame 포즈를 렌더해서 실제 확인함 (파일명만으로 판단 안 함).
전부 fcurve 회전 점프(관절 튐) 재검사 완료, clean.

## 상태별 매핑

### Locomotion — 기본 이동

| 파일 | 프레임 | 용도 |
|---|---|---|
| walking.fbx | 32 | 걷기 |
| running.fbx | 22 | 달리기 |
| run to stop.fbx | 28 | 달리다 정지 (감속 블렌드) |
| left turn.fbx | 32 | 제자리 좌회전 |
| right turn.fbx | 32 | 제자리 우회전 |

### Idle — 대기 (2종으로 축소됨)

| 파일 | 프레임 | 포즈 특징 | 용도 |
|---|---|---|---|
| idle.fbx | 120 | 기본 이완 자세 | **Default Idle** |
| idle (2).fbx | 300 | 주먹 살짝 든 경계 자세 | **Combat-Ready Idle** |

idle(3)/(4)/(5) 삭제됨(2026-08-10 정리). idle(5)는 idle(2)와 목적 겹쳐서 제거, idle(3)/(4)는 별도 상태로 안 쓰기로 함.

### Traversal — 점프/낙하

| 파일 | 프레임 | 용도 |
|---|---|---|
| jumping up.fbx | 8 | 점프 시작 |
| falling idle.fbx | 21 | 공중 체공 상태 |
| falling to roll.fbx | 55 | 낙하 후 구르기 착지 |
| hard landing.fbx | 60 | 강한 착지(구르기 없이) |

### Cover System — 엄폐

| 파일 | 프레임 | 용도 |
|---|---|---|
| stand to cover.fbx | 29 | 서있다가 엄폐 진입 |
| cover to stand.fbx | 46 | 엄폐에서 일어서기 |
| crouched sneaking left.fbx | 39 | 크라우치 좌측 이동 |
| crouched sneaking right.fbx | 33 | 크라우치 우측 이동 |
| left cover sneak.fbx | 35 | 엄폐물 붙은 채 좌측 이동 |
| right cover sneak.fbx | 34 | 엄폐물 붙은 채 우측 이동 |

`stand to cover (2)`, `cover to stand (2)` 변형본은 삭제됨(2026-08-10 정리, 목적 중복).

### Core Mechanic — 잡고 당기기 (Cargo Strap 후보)

| 파일 | 프레임 | 용도 |
|---|---|---|
| Pulling A Rope.fbx | 443 | 당기는 상체 heave 동작, Strap Tension 참고용 |
| Rope Swinging.fbx | 134 | 매달려 스윙, Strap 이동 참고용 |

**주의**: 게임 캐논은 로프가 아니라 화물 스트랩+카라비너(GEAR01)임. 이 두 클립은 팔/상체 동작 참고 자료로만 쓰고, 손에 쥔 오브젝트·이펙트는 스트랩으로 교체 필요.

### Combat

| 파일 | 프레임 | 용도 |
|---|---|---|
| Hook Punch.fbx | 66 | 근접 훅 펀치 |
| Fist Fight A.fbx | 141 | 맨손 격투 콤보 |
| Reaction.fbx | 56 | 피격 리액션 |
| Dodging.fbx | 68 | 회피 |
| Dying1.fbx | 133 | 사망 모션 A |
| Dying2.fbx | 141 | 사망 모션 B |
| Stable Sword Outward Slash.fbx | 62 | 검 베기 |
| Rifle Block.fbx | 85 | 총기 막기 자세 |

**주의**: 게임 설계 문서상 플레이어 무기 체계는 화물 스트랩뿐, 검/총 언급 없음. 이 2개는 지금 캐논과 안 맞음 — 적/동료용으로 재해석하거나 드롭 검토.

## 중복/축소 처리 결과 (2026-08-10)

1. ~~idle 5종 → 2종으로 축소~~ **완료**: idle(3)/(4)/(5) 삭제, idle.fbx + idle(2).fbx만 유지.
2. ~~stand to cover 중복~~ **완료**: `stand to cover (2).fbx` 삭제, `stand to cover.fbx`만 유지.
3. ~~cover to stand 중복~~ **완료**: `cover to stand (2).fbx` 삭제, `cover to stand.fbx`만 유지.
4. **crouched sneaking left/right vs left/right cover sneak**: 유지. 목적 유사(크라우치 이동)하지만 엄폐물 밀착 여부가 다름 — 실제 다른 상황(오픈필드 잠입 vs 엄폐 벽 이동)이라 중복 아닐 수 있음, 기획 확인 필요.
5. **Dying1 / Dying2**: 유지 + 구분 가능하게 파일명 정리(`Dying.fbx`→`Dying1.fbx`, `Dying (1).fbx`→`Dying2.fbx`). 사망 변주 2개, 의도적 다양성으로 유지.

## 기술적 확인 필요 사항 (Unity 통합 전)

- **Root Motion 처리 완료 (2026-08-10)**: 26개 클립 전부 hip 본 world 좌표 기준 수평 이동(x,y)을 프레임별로 상쇄해서 in-place로 재굽고 원본 교체함. 수직(z, 점프 높이/착지 낙차/스윙 진폭)은 그대로 보존. 변환 전/후 world-space 이동량 검증 완료 — x,y 전부 0.0000, z는 각 클립 특성에 맞게 유지됨(예: walking z=0.08 자연스러운 상하 바운스, Rope Swinging z=1.14 스윙 진폭, Dying1/2 z=0.85~0.90 쓰러지는 낙차). 렌더로 포즈 무결성도 재확인함(walking, Dying1, Rope Swinging, falling to roll).
- 프로젝트 스펙(`no root translation in locomotion/supply clips unless explicitly documented`) 충족.
- Rig: Mixamo Standard Skeleton, 41 bone (2 Chain Fingers). 이후 CHR01/ENY01과 리그 컨벤션 통일 여부 확인 필요.
