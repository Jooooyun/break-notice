# CHR00 애니메이션 전체 목록

`updatedAnimation/`에 있는 41개 fbx 전부 — 각각 스킨 메쉬+Armature+애니메이션이 한 파일에 통째로 들어있음(완전 독립적, 다른 파일이랑 Avatar 맞출 필요 없음). Humanoid로 임포트해도 정상 동작 확인됨(각 fbx가 자기 자신에서 Avatar를 새로 만드는 방식이라 Copy From Other Avatar류 에러 없음).

---

## 로코모션

CHR00_Idle - 기본 대기 자세, 팔 벌리는 제스처(의도된 디자인). 아무 입력 없을 때 기본 상태로 사용.

CHR00_Idle_Variant_Stretch - Idle 배리언트, 기지개 켜는 동작. Idle 중 랜덤/주기적으로 섞어서 심심하지 않게 쓰는 용도.

CHR00_Walk_F - 전진 걷기, 인플레이스. 걷기 입력 중 Blend Tree 축으로 사용.

CHR00_Run_F - 전진 달리기, 인플레이스. 달리기 입력 중 Blend Tree 축으로 사용.

CHR00_Walk_Stop - 걷다가 멈추는 동작. Walk_F에서 입력 끊길 때 재생.

CHR00_Run_Stop - 뛰다가 멈추는 동작. Run_F에서 입력 끊길 때 재생.

CHR00_Strafe_L / CHR00_Strafe_R - 좌/우 옆걸음, 인플레이스. 좌우 스트레이프 입력 중 사용, Aim Layer랑 합성 가능(상체 조준 + 하체 스트레이프).

CHR00_Strafe_L_Short_ALT / CHR00_Strafe_R_Short_ALT - 위 두 개의 짧은 버전. 짧게 탭 입력 시 사용, 길게 홀드하면 기본 Strafe_L/R.

CHR00_Turn_L_90 / CHR00_Turn_R_90 - 제자리에서 90도 회전. 방향 전환 입력 시 사용.

CHR00_Turn_L_Loop_ALT / CHR00_Turn_R_Loop_ALT - 위 회전의 루프 버전. 스틱 계속 홀드해서 연속 회전할 때 사용.

CHR00_Turn_180_Running / CHR00_Turn_180_Walking - 달리기/걷기 중 180도 급회전. 이동 중 반대 방향 입력 시 사용, 끝나면 같은 로코모션으로 복귀.

CHR00_Jump - 점프 동작(21프레임, 빠르게 조정됨). 점프 입력 시 사용.

---

## 피격 / 기상

CHR00_Melee_Hit_Body - 피격 리액션(공격 아님). 플레이어가 데미지 받을 때 사용.

CHR00_Knockdown_F / CHR00_Knockdown_B - 앞/뒤 넉백. 데미지 방향에 따라 사용, 끝나면 눕은 채 대기 → Getup으로 연결.

CHR00_Getup_F - 앞으로 넘어진 데미지 후 기상(70프레임, 빠르게 조정됨). Knockdown_F 후 자동 재생.

CHR00_Getup_B - 뒤로 넘어진 데미지 후 기상. Knockdown_B 후 자동 재생.

---

## 근접 콤보

CHR00_Melee_JabCross_raw - 원본 통짜 60프레임(Jab+Cross 미분리). Unity Model Importer의 Clips 기능으로 프레임 구간 잘라서 Jab 1타/Cross 2타로 분리해서 사용. Cross는 Jab 끝나기 직전(cancel window)에 다음 입력 있을 때만 전환. 데미지 타이밍 마커 아직 없음, Unity에서 Animation Event로 추가 필요.

---

## 조준

CHR00_Aim_Idle - 조준 정지 포즈(placeholder). 단독으로 안 쓰고, 상체만 Avatar Mask로 분리한 Aim Layer에 얹어서 하체 로코모션(Strafe/Walk 등)이랑 합성해서 사용. 방향 반응은 Unity Animation Rigging의 Multi-Aim Constraint로 처리.

PREVIEW_Strafe_plus_Aim_inplace - 참고용 클립, 게임에 직접 쓰지 말 것. 위 Aim_Idle + Layer 합성 방식으로 대체.

---

## 캐치

CHR00_Catch_Small_Center - 정면에서 오는 작은 물체 캐치. 정면 캐치 입력 시 사용.

CHR00_Catch_Small_Diving_L / CHR00_Catch_Small_Diving_R - 좌/우로 벗어난 작은 물체 다이빙 캐치. 좌우로 빗나간 물체 캐치 시 사용.

CHR00_Catch_Medium - 중간 크기 물체 캐치. IK로 팔 간격 재조정한 상태(작업 반영됨), 실제 렌더/플레이 확인은 아직 안 됨 — Unity 적용 후 눈으로 한 번 확인 권장.

---

## 던지기

CHR00_Throw_Small - 작은 물체 던지기. Release 시점에 Animation Event로 투사체 스폰 처리.

CHR00_Throw_Medium - 중간 크기 물체 던지기(77프레임). 회전 급전환 구간 일부 수정됨, 완전 검증은 안 됨 — Unity 적용 후 확인 권장.

CHR00_Throw_TwoHand_base_REF - 참고용 베이스 포즈, 직접 쓰지 말 것. 향후 양손 던지기 계열 제작 시 참고 소스.

---

## 줍기

CHR00_Pickup_Low_Small - 바닥의 작은 물체 획득, 인플레이스. 바닥 물체 획득 입력 시 사용.

CHR00_Pickup_Low_Medium - 바닥의 중간 물체 획득, 인플레이스. 바닥 물체 획득 입력 시 사용.

---

## 화물 스트랩 / 카라비너 텐션

Cargo Strap 메커닉(의자·카트 등에 스트랩 걸어서 고정/견인) 전용 시퀀스. 클립끼리 포즈가 이어지도록 만들어짐: Prepare → Rolling(대기 루프) → Throw → Pull 순서로 자연스럽게 연결됨.

CHR00_Strap_Prepare - 스트랩을 꺼내서 앵커에 걸 준비하는 동작(66프레임). 카라비너/스트랩 인터랙션 시작 시 사용.

CHR00_Strap_Rolling - 스트랩 감아 정리하는 반복 루프(13프레임, 루프). Prepare 끝나고 앵커에 걸기 전 대기 상태에 사용.

CHR00_Strap_Throw - 스트랩을 앵커 쪽으로 던져서 거는 동작(14프레임). Rolling 대기 상태에서 실제 액션(건다) 입력 시 사용.

CHR00_Strap_Pull - 스트랩을 당겨서 장력(텐션)을 거는 동작(35프레임). CARGO_STRAP_SPEC.md의 "Tensioned Redirect" 상태 진입 시 사용. Throw 끝난 후 이어짐. hip 위치가 완전 인플레이스는 아님(6cm 정도 미세 이동), 필요하면 추후 보정 가능.

CHR00_Strap_Attack_L / CHR00_Strap_Attack_R - 스트랩에 힘을 주는 동작 배리언트, 좌/우로 스윙 궤적이 다름(각각 20/22프레임, 루프). Pull 이후 대기 자세에서 시작/종료. 정확히 어느 상황에서 L이 쓰이고 R이 쓰이는지는 디자인 쪽 확인 필요 — 포즈상으로는 텐션 조정/반복 당김 동작으로 보임.

CHR00_Strap_Tension_Loop_candidate - 스트랩 당긴 채 유지하는 루프(195프레임), 미확정 후보. 정식 승인 전까지 Runtime 연결하지 말 것.

---

## 제작 안 됨 (Large 계열 포함)

CHR00_Catch_Large, CHR00_Throw_Large - 프로듀서 결정으로 스코프 아웃, 이 배포에 없음.

상체 전용 Carry 루프, Carry_Medium_Idle/Walk, Pickup 데미지/접촉 마커, ENY01_Grab_Hold/Release, Grapple 계열 - 전부 미제작.
