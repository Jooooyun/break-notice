# AI 전용 — 캐릭터 애니메이션 적용 지침

이 문서는 Designer A가 로컬에서 소싱/가공한 애니메이션을 git commit → pull 이후,
작업을 이어받는 AI(Codex)가 Unity에 적용하기 위한 절차서다. 실제 Runtime
Integration 권한 범위는 상위 문서(추가업무.txt, DESIGNER_A_CODEX_WORK_ORDER.md)를
따른다 — 이 문서는 그 권한 안에서의 실무 절차만 다룬다.

---

## 0. 선행 확인

- 이 파일과 같이 커밋된 애니메이션 폴더 구조(실제 git 경로):
  ```
  ArtSource/Characters/
  ├─ ANIMATION_COVERAGE_MATRIX_v001.csv
  ├─ CHR00_Player/
  │   ├─ DELIVERY.md
  │   └─ Animations/                      (로코모션 + Catch/Throw/Aim 등, 31개)
  ├─ CHR01_Ally/
  │   ├─ DELIVERY.md
  │   └─ Animations/                      (Supply_Binder_Windup/Release 2개)
  └─ ENY01_ComplianceRunner/
      ├─ DELIVERY.md
      └─ Animations/
          ├─ (기존 로코모션/Hit, normal·elite 공용)
          ├─ approved/                    (Elite Hit 5종, 바로 사용 가능)
          ├─ candidates/                  (근접공격 후보 4종, in-place 미확인)
          └─ pending_producer_review/     (Elite 6종, 미승인 — NOTICE.txt 참고)
  ```
  이 문서 자체는 `docs/10_contest/handoffs/AI_ANIMATION_APPLY_GUIDE.md`에 위치.
- 파일명이 `_candidate`로 끝나는 것은 **아직 in-place 처리/최종 확정 안 된 후보**다.
  그대로 Runtime에 박지 말고, 먼저 root motion 유무 확인 후 필요시 in-place 변환할 것
  (기준: Hips 본의 수평 이동량이 프레임 간 유의미하게 변하면 root motion 있음).

---

## 1. 캐릭터별 적용 지침

### CHR00 (Player)
- `Idle / Walk_F / Run_F / Strafe_L·R / Turn_L·R_90 / Turn_180 / Walk_Stop / Run_Stop /
  Jump / Knockdown_F·B / Getup_F·B / Melee_Hit_Body / Idle_Variant_Stretch` — 그대로 적용
- `Aim_Idle / Catch_Small·Medium·Large / Throw_Small·Medium·Large / Strap_Tension_Loop_candidate`
  — CHR00 전용, 다른 캐릭터에 재사용하지 말 것
- `PREVIEW_Strafe_plus_Aim_inplace.fbx` — **Runtime 클립 아님, 참고용**. Additive
  레이어가 실제로 어떻게 보이는지 확인하려고 만든 예시 합성본. Animator에 그대로
  연결하지 말 것 — 실제 적용은 Strafe_L/R(로코모션) + Aim_Idle(Additive 레이어)을
  Unity에서 따로 결합해서 처리
- `CHR00_Throw_TwoHand_base_REF.fbx` — **Runtime 클립 아님, 참고용**. Throw_Medium/
  Large를 만든 원본 소스 백업(수정 이력 추적용). 그대로 쓰지 말고 이미 완성된
  Throw_Medium/Large를 사용할 것

### CHR01 (Ally)
- 로코모션은 CHR00 파일을 **그대로 복사**해서 적용 가능(65본 그룹, 본 이름 서브셋 확인됨)
- **Supply_Binder 완료됨, 그대로 적용**:
  - `CHR01_Supply_Binder_Windup.fbx` — 단일 고정 프레임(진짜 정지, 루프 걸어도 안전), 팔 들어올린 준비 자세
  - `CHR01_Supply_Binder_Release.fbx` — 릴리즈+회복, in-place 확인 완료, 끝에서 자연스럽게 팔 내려옴
  - 재생 순서: Windup(홀드, 트리거 대기) → 트리거 시 Release 재생
- `Supply_candidate_FrisbeeThrow.fbx` / `Supply_candidate_OverhandThrow.fbx` /
  `Supply_candidate_ThrowIn_TwoHand.fbx` — **미사용 잉여 후보**. Supply_Binder는
  `Throw` 후보로 이미 완성했음. 이 셋은 root motion 처리도 안 된 원본 그대로라
  Runtime에 바로 쓰면 안 됨(자리 이탈함). 다른 용도 필요할 때만 별도로 in-place 처리할 것.
- **CHR01에는 절대 적용하지 말 것**: Aim, Catch, 풀 Throw(windup+release 전체),
  Strap 메인 조작 — CHR01은 이동+던지기 조합 없이 정지 상태 Supply만 수행

### ENY01_ComplianceRunner (normal 인스턴스)
- 로코모션은 기존 0810 배치 그대로 재사용(이미 완비)
- `Animations/candidates/`(Boxing/Kicking 4종) 검토 후 적용 — root motion 미확인,
  in-place 처리 먼저 할 것. 부족하면 v003 §8 리스트 기준으로 Jab/Straight/Hook/
  Overhand/Forearm Check/Two-hand Shove 추가 소싱

### ENY01_ComplianceRunner (elite 인스턴스)
- 로코모션·Hit는 normal과 동일 스켈레톤이라 그대로 공유(`Animations/approved/` 5종)
- 근접 공격은 §2 참고 — 조건부 적용, 새로 만들지 말고 우선 normal 근접공격을
  속도/텔레그래프만 조정해서 재활용하는 방향으로 진행(DELIVERY.md §결정요청 참고,
  Owner 확답 오면 그대로 실행)

---

## 2. Elite 처리 — AI가 분석 끝내고 결정지점만 올릴 것

**하지 말 것**: "여기 애매한 게 있으니 확인해달라"처럼 raw 상태로 떠넘기는 것.
**할 것**: AI가 각 클립을 실제로 Unity에 넣어 확인하고, 왜 문제인지/왜 괜찮은지
구체적으로 판단한 다음, Owner에게는 "이런 이유로 이렇게 판단했고, A/B 중 뭘
택할지"만 묻는다. 아래는 이번 세션에서 이미 그 분석을 마친 결과이므로 그대로
따르고, 새로 추가되는 클립도 이 형식을 지킬 것.

`ENY01_ComplianceRunner/Animations/approved/` 5종(Hit 리액션)은 바로 적용.

`ENY01_ComplianceRunner/Animations/pending_producer_review/` 6종 — **2026-08-13
Producer 승인으로 Unity 로드 가능 상태로 전환됨.** DELIVERY.md에 개별 톤 판정
근거는 이미 적어놨으니 참고하되, "폴더를 불러와서 확인한다"까지는 승인된 것이고
**클립 하나하나의 최종 채택 여부까지 자동 승인된 건 아님** — Unity에서 실제로
재생해보고 톤 맞는지 다시 판단할 것. 특히 `Mutant Punch/Swiping/Walking`(정체성
문제), `Stabbing`(살상 연출 위험), `Jump Attack`(§8.4 위반 소지)은 확인 후에도
여전히 리스크가 남아있다는 점을 Owner에게 재확인 없이 임의로 최종 승인 처리하지
말 것.

---

## 3. 적용 및 검증 절차 (기존 작업 방식 그대로)

1. Unity 프로젝트에 해당 캐릭터의 fbx import
2. 기존 Animator Controller / Avatar 설정에 맞춰 클립 연결
3. **실제로 재생시켜서 확인**:
   - 스켈레톤 리타겟 정상인지(팔다리 꺾임/뒤틀림 없는지)
   - 제자리에서 도는지(원치 않는 이동 없는지, root motion 의도한 경우 제외)
   - 시작/끝 자세가 인접 애니메이션(Idle 등)과 자연스럽게 이어지는지
4. 문제 있으면 Blender에서 원인 파악 후 재작업 — 이번 세션에서 자주 나온 실수들:
   - Export 시 Object Types에 Mesh 포함시켜서 용량 폭증
   - Export Animation 옵션에서 "All Actions" 켜져서 무관한 액션까지 같이 export됨
   - Pose Mode 아닌 Edit Mode에서 편집
   - Quaternion 보간 시 최단경로 아닌 반대방향으로 돌아서 관절 꼬임
   이런 증상 보이면 원인부터 저 목록에서 먼저 의심할 것

---

## 4. Git 커밋 규칙 (기존 프로젝트 규칙 그대로 적용)

- 커밋은 목적 하나당 하나(one-purpose commit)
- `git add .` / `git add -A` 금지, 정확한 파일 경로만 stage
- PR/merge는 Human Owner 승인 없이 생성 금지
- 커밋 예시: `art: apply CHR00/CHR01/ENY01 sourced animations v001`

---

## 5. 완료 후 문서 갱신

- `ArtSource/Characters/ANIMATION_COVERAGE_MATRIX_v001.csv` — **작성 완료**(168행).
  기존 154개 파일 자동 분류 + 이번 세션 신규 산출물 14행 포함. `Required Gameplay Clip`
  컬럼까지 v003 정식 이름으로 매핑 완료. Frame 번호/Root Motion 세부값은 아직
  TBD로 비어있음 — 실제 적용하면서 하나씩 채울 것.
- `CHR00_Player/DELIVERY.md`, `CHR01_Ally/DELIVERY.md`, `ENY01_ComplianceRunner/DELIVERY.md`
  — **작성 완료**. 완료분/미완료분 구분 기록됨, `[결정 요청]` 항목은 Owner 확답
  오면 그 내용대로 갱신할 것(AI가 임의로 항목 지우지 말 것).

---

## 6. 세션 중 발견된 자잘한 버그 (참고용)

- Blender에서 "새 액션 만들어서 특정 프레임 값 복사해넣기" 방식으로 static pose
  파일 만들면 export 시 값이 사라지는 경우 발생함(원인 불명, 재현은 됨). 대신
  **기존에 이미 정상 작동하는 액션을 `scene.frame_start`/`frame_end`로 범위만
  트림해서 export**하는 방식이 안전함 — 이번 세션에서 CHR01_Supply_Binder_Windup
  만들 때 이 문제로 두 번 재작업함.
