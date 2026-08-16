# CHR00 애니메이션별 Unity 적용 가이드

`updatedAnimation/` 안의 37개 fbx 각각을 Unity에서 어떻게 적용해야 하는지 정리. 전부 스킨+Armature+애니메이션 통짜 포함 fbx이므로, Import 시 Animation Type은 `DELIVERY_NOTES.md`에 적힌 대로 Generic/Legacy 권장.

---

## 1. 로코모션 (Base Layer, Blend Tree / State Machine)

| 클립 | 적용 방식 |
|---|---|
| CHR00_Idle | 기본 대기 상태(Entry state), 루프 |
| CHR00_Idle_Variant_Stretch | Idle 상태에서 랜덤/일정 시간마다 전환되는 배리언트, 루프 or 원샷 후 Idle 복귀 |
| CHR00_Walk_F | 전진 걷기 Blend Tree 축, 루프, 인플레이스(제자리) — 이동은 스크립트 root motion 아닌 CharacterController로 처리 |
| CHR00_Run_F | 전진 달리기 Blend Tree 축, 루프, 인플레이스 |
| CHR00_Walk_Stop | Walk_F → Idle 전환 시 원샷 재생 |
| CHR00_Run_Stop | Run_F → Idle 전환 시 원샷 재생 |
| CHR00_Strafe_L / CHR00_Strafe_R | 좌우 옆걸음 Blend Tree 축, 루프. `AIM_STRAFE_LAYER_GUIDE.md` 참고해서 Aim Layer랑 조합 가능 |
| CHR00_Strafe_L_Short_ALT / CHR00_Strafe_R_Short_ALT | 위 두 개의 짧은 대안 테이크 — 짧은 사이드스텝 입력(탭)에 쓸 것, 홀드 입력엔 기본 Strafe_L/R |
| CHR00_Turn_L_90 / CHR00_Turn_R_90 | 90도 회전 원샷, 정지 상태에서 방향 전환 입력 시 |
| CHR00_Turn_L_Loop_ALT / CHR00_Turn_R_Loop_ALT | 위 회전의 루프 대안 — 연속 회전(스틱 홀드) 상황에 |
| CHR00_Turn_180_Running / CHR00_Turn_180_Walking | 각각 Run/Walk 중 180도 급회전 입력 시 원샷, 끝나면 같은 로코모션으로 자연 복귀 |
| CHR00_Jump | 점프 입력 시 원샷 트리거. 21프레임(2.7배 가속) |

## 2. 피격/기상 (원샷, Damage 시스템에서 트리거)

| 클립 | 적용 방식 |
|---|---|
| CHR00_Melee_Hit_Body | 피격 리액션 원샷 — 공격 아님, 데미지 받을 때 전용 |
| CHR00_Knockdown_F / CHR00_Knockdown_B | 넉백 데미지 방향에 따라 원샷, 끝나면 눕은 자세로 홀드 → Getup으로 연결 |
| CHR00_Getup_F | Knockdown_F 이후 자동 트리거 원샷, 70프레임(4.96배 가속) |
| CHR00_Getup_B | Knockdown_B 이후 자동 트리거 원샷 |

## 3. 콤보/근접 공격

| 클립 | 적용 방식 |
|---|---|
| CHR00_Melee_JabCross_raw | Mixamo 원본 통짜(60프레임, Jab+Cross 미분리). **Unity Model Importer의 Clips 리스트**에서 프레임 구간 잘라 Jab/Cross 두 개 State로 분리해서 쓸 것 — Blender 작업 불필요. Cross는 Jab의 cancel window(끝나기 직전)에서 다음 입력 있을 때만 전환, 없으면 Jab 자체가 Idle로 복귀. Damage timing marker는 아직 안 찍혀있음 — Animation Event로 Unity 쪽에서 직접 추가해야 함 |

## 4. 조준 (레이어 합성)

| 클립 | 적용 방식 |
|---|---|
| CHR00_Aim_Idle | 정지 포즈 placeholder. 단독 사용 안 함 — `AIM_STRAFE_LAYER_GUIDE.md`대로 Aim Layer(상체 Avatar Mask)에 얹어서 하체 로코모션과 조합. 방향 반응은 Multi-Aim Constraint로 추가 처리 |
| PREVIEW_Strafe_plus_Aim_inplace | **직접 쓰지 말 것** — 참고용. `AIM_STRAFE_LAYER_GUIDE.md`의 Layer 조합 방식으로 대체 |

## 5. 캐치 (원샷, 방향/크기별 트리거)

| 클립 | 적용 방식 |
|---|---|
| CHR00_Catch_Small_Center | 정면에서 오는 Small 오브젝트 캐치 원샷 |
| CHR00_Catch_Small_Diving_L / CHR00_Catch_Small_Diving_R | 좌/우로 벗어난 Small 오브젝트, 다이빙 캐치 원샷 |
| CHR00_Catch_Medium / CHR00_Catch_Large | **품질 이슈 있음**(손목 등 관절 뒤틀림) — 일단 연결은 가능하나 비주얼 확인 후 재작업 예정. 임시로 넣어도 되지만 QA 때 플래그 필요 |

## 6. 던지기 (원샷)

| 클립 | 적용 방식 |
|---|---|
| CHR00_Throw_Small | Small 오브젝트 던지기 원샷, release 시점 Animation Event로 실제 투사체 스폰 |
| CHR00_Throw_Medium / CHR00_Throw_Large | **품질 이슈 있음**(관절 뒤틀림 + 전환 구간 부자연스러움) — Catch_Medium/Large와 동일하게 임시 연결만, 재작업 대기 |
| CHR00_Throw_TwoHand_base_REF | **직접 게임에 쓰지 말 것** — 향후 양손 던지기 계열 제작할 때 참고용 베이스 포즈 소스 |

## 7. 물건 줍기 (원샷)

| 클립 | 적용 방식 |
|---|---|
| CHR00_Pickup_Low_Small | 바닥의 Small 오브젝트 획득 원샷, 인플레이스(이번에 이동 드리프트 제거함) |
| CHR00_Pickup_Low_Medium | 바닥의 Medium 오브젝트 획득 원샷, 인플레이스 |

## 8. 미확정 / 보류

| 클립 | 적용 방식 |
|---|---|
| CHR00_Strap_Tension_Loop_candidate | **아직 정식 승인 안 됨** — Animator에 연결하지 말고 대기. 승인되면 별도 안내 |

---

## 아직 없는 것 (이 배포에 포함 안 됨)
- 상체 전용 Carry 루프, Carry_Medium_Idle/Walk, Pickup_Low 계열 데미지/접촉 마커, ENY01_Grab_Hold/Release, Grapple 계열 — 전부 Blender 쪽 미제작/미완료 상태.
