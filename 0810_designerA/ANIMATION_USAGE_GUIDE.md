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

### Idle — 대기 (5종, 실제 포즈 다 다름)

| 파일 | 프레임 | 포즈 특징 | 추천 용도 |
|---|---|---|---|
| idle.fbx | 120 | 기본 이완 자세 | **Default Idle** |
| idle (2).fbx | 300 | 주먹 살짝 든 경계 자세 | **Combat-Ready Idle** |
| idle (3).fbx | 98 | 고개 숙인 피곤/방심 자세 | 랜덤 Idle Break 후보 |
| idle (4).fbx | 77 | 낮은 방어 자세(스쿼트성) | 압박/피격 직후 Idle 후보 |
| idle (5).fbx | 68 | 주먹 든 얕은 경계 자세 | idle(2)와 목적 겹침 |

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
| stand to cover (2).fbx | 38 | 서있다가 엄폐 진입 (변형) |
| cover to stand.fbx | 46 | 엄폐에서 일어서기 |
| cover to stand (2).fbx | 34 | 엄폐에서 일어서기 (변형) |
| crouched sneaking left.fbx | 39 | 크라우치 좌측 이동 |
| crouched sneaking right.fbx | 33 | 크라우치 우측 이동 |
| left cover sneak.fbx | 35 | 엄폐물 붙은 채 좌측 이동 |
| right cover sneak.fbx | 34 | 엄폐물 붙은 채 우측 이동 |

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
| Dying.fbx | 133 | 사망 모션 A |
| Dying (1).fbx | 141 | 사망 모션 B |
| Stable Sword Outward Slash.fbx | 62 | 검 베기 |
| Rifle Block.fbx | 85 | 총기 막기 자세 |

**주의**: 게임 설계 문서상 플레이어 무기 체계는 화물 스트랩뿐, 검/총 언급 없음. 이 2개는 지금 캐논과 안 맞음 — 적/동료용으로 재해석하거나 드롭 검토.

## 중복/축소 후보 (삭제 안 함, 판단만 요청)

1. **idle 5종 → 2~3종으로 축소 가능**: idle.fbx(기본) + idle(2).fbx(전투 대기)면 상태머신 커버 가능. idle(5)는 idle(2)와 포즈 목적 겹침. idle(3)/idle(4)는 랜덤 배리에이션 용도로 남길지 판단 필요.
2. **stand to cover / stand to cover (2)**: 둘 다 동일 목적(서기→엄폐). 하나만 채택 추천.
3. **cover to stand / cover to stand (2)**: 둘 다 동일 목적(엄폐→서기). 하나만 채택 추천.
4. **crouched sneaking left/right vs left/right cover sneak**: 목적 유사(크라우치 이동)하지만 엄폐물 밀착 여부가 다름 — 실제 다른 상황(오픈필드 잠입 vs 엄폐 벽 이동)이라 중복 아닐 수 있음, 기획 확인 필요.
5. **Dying / Dying (1)**: 사망 변주 2개, 의도적 다양성일 수 있음(반복감 감소) — 스펙상 예산 여유 있으면 유지 추천.

## 기술적 확인 필요 사항 (Unity 통합 전)

- **Root Motion 여부 미확인**: falling to roll / Rope Swinging 렌더 시 캐릭터가 카메라 밖으로 크게 이동함 — 클립에 루트 모션(위치 이동)이 포함된 것으로 보임. 프로젝트 스펙(`no root translation in locomotion/supply clips unless explicitly documented`)과 맞는지 확인 필요. Mixamo 재다운로드 시 In Place 옵션 확인 권장.
- Rig: Mixamo Standard Skeleton, 41 bone (2 Chain Fingers). 이후 CHR01/ENY01과 리그 컨벤션 통일 여부 확인 필요.
