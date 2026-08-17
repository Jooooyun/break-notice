# AI 전용 — 캐릭터 애니메이션 적용 지침 v002

v001(`docs/10_contest/handoffs/AI_ANIMATION_APPLY_GUIDE.md`)의 후속. CHR01/ENY01 다리가
안쪽으로 꺾이는 리그 문제 제보에 대한 검증 + 확정 클립 실제 생산까지 마친 결과를 반영함.
v001과 이 문서가 다르면 **이 문서(v002)가 최신 기준**.

---

## 0. 이번에 밝혀진 것

- **CHR01/ENY01의 정적 리그(rest pose) 자체는 정상.** CHR00(고친 후) vs 원본 main_character
  vs female_helper vs enemy_normal, 다리 본 축 비교함(허벅지-정강이 꼬임각 전부 0.6~1.5도
  범위, 이상 없음). "본이 근본적으로 망가졌다"는 증거 없음.
- 실제 재발 위험은 리그 데이터가 아니라 **워크플로우**에 있었음:
  1. `ArtSource/CHR01_Ally`, `ENY01_ComplianceRunner`의 Animations 폴더 fbx가 전부
     mesh 미포함(애니메이션 전용) — CHR00이 실제로 겪었던 "Copy From Other Avatar"
     Rig Error 패턴과 같은 함정
  2. mocap을 기존 타겟에 world-space bake 없이 naive 적용하면 Automatic Bone
     Orientation 불일치로 관절이 뒤틀릴 수 있음(v001 §자잘한 버그 참고)
- CHR01 로코모션 소스 실제 비교 검증함(female_helper 자체 walking.fbx 리타겟 vs
  CHR00_Walk_F.fbx 리타겟) — **둘 다 quaternion flip 0회, 회전량도 비슷**. 리그
  자체엔 문제 없음이 재확인됨. 최종적으로는 v001과 동일하게 CHR00 파일 재사용 방식 채택
  (이미 검증된 소스라 일관성 있고, 신규 소스 관리 부담도 없음).
- **따라서 이번 파이프라인은 v001의 "mesh+bone+anim 한 파일에 결합"(§9) 원칙을
  CHR01/ENY01에도 그대로 적용해서 확정 클립을 전부 재생산함.** 아래 §1이 최종 산출물.

---

**Unity 6000.3.20f1 배치 모드로 실제 검증 완료**: 116개(CHR01 20 + ENY01 Normal 48 +
ENY01 Elite 48) 전부 import → `animationType = Human` → `avatarSetup =
CreateFromThisModel`로 자동 세팅 후 Avatar.isValid/isHuman 체크 — **116/116 성공, 실패 0건**.
산출물은 `Game/Assets/_Project/Art/Animation/{CHR01, ENY01/Normal, ENY01/Elite}/`에
이미 들어있음(각 fbx에 대응 .meta 포함, git에 커밋됨). **단, 이건 import 성공 +
Avatar 유효성만 자동 확인한 것** — 실제 재생했을 때 팔다리 꺾임 없이 자연스러운지는
Owner가 Editor에서 직접 눈으로 확인 필요(§2 참고).

## 1. 확정 산출물 — mesh+Armature+애니메이션 한 파일에 결합, Copy From Other Avatar 금지

파일당 독립적으로 Humanoid Avatar 생성 가능(v001 §9와 동일 원칙). Unity에서 각 fbx를
"Create Avatar From This Model"로 처리할 것 — 절대 다른 파일 Avatar를 Copy해서 쓰지 말 것.

**export 시 텍스처 임베드 필수** — 이번에 실제로 겪은 버그: `bpy.ops.export_scene.fbx()`에
`path_mode='COPY', embed_textures=True`를 안 주면, 재질 연결은 export되지만 텍스처
이미지 자체는 이 PC의 절대경로(`*.fbm/` 폴더)만 참조하고 fbx 안엔 안 들어감 — 파일을
다른 데로 옮기면 텍스처 깨짐. 반드시 두 옵션 켤 것:
```python
bpy.ops.export_scene.fbx(
    filepath=out_path, use_selection=True,
    object_types={'ARMATURE', 'MESH'}, add_leaf_bones=False,
    bake_anim=True, bake_anim_use_all_actions=False,
    bake_anim_use_nla_strips=False, bake_anim_step=1.0,
    mesh_smooth_type='FACE',
    path_mode='COPY', embed_textures=True,   # 이거 빠지면 텍스처 안 박힘
)
```

### CHR01_Ally (총 20개, `female_helper.fbx` 베이스)

CHR00 파일(`updatedAnimation/CHR00_*.fbx`, 이미 완성·검증된 소스)을 female_helper
Armature에 world-space retarget bake한 것 + Supply_Binder 2종(자체 소스를 같은 방식으로
mesh 결합만 다시 함):

- CHR01_Idle, CHR01_Idle_Variant_Stretch
- CHR01_Walk_F, CHR01_Run_F, CHR01_Walk_Stop, CHR01_Run_Stop
- CHR01_Strafe_L, CHR01_Strafe_R
- CHR01_Turn_L_90, CHR01_Turn_R_90, CHR01_Turn_180_Running, CHR01_Turn_180_Walking
- CHR01_Jump
- CHR01_Knockdown_F, CHR01_Knockdown_B, CHR01_Getup_F, CHR01_Getup_B
- CHR01_Melee_Hit_Body
- CHR01_Supply_Binder_Windup(21프레임 고정 홀드), CHR01_Supply_Binder_Release(21~68,
  release+회복) — 재생 순서: Windup 홀드 → 트리거 시 Release

**CHR01에 절대 적용하지 말 것**(v001과 동일): Aim, Catch, 풀 Throw, Strap 메인 조작.
CHR01은 이동+던지기 조합 없이 정지 상태 Supply만 수행.

**미확정, 이번 산출물에 없음** (Owner 결정 대기 — v001 이후 추가로 확인된 사항):
- 로코모션 자체를 "고정 지원형" 캐릭터에 쓸지는 CHR01 A/B 시나리오 결정에 달림
  (`CHR01_Ally/DELIVERY.md` 참고). 결정 전까지는 만들어만 두고 Runtime 미연결 권장.
- Carabiner Assist(§6, Variant A/B/C, ~20클립), Supply_Binder_Abort — **소스 자체가
  없음, 이번 세션에서 새로 서베이함**: ArtSource/0810/updatedAnimation 어디에도
  Carabiner 관련 애니메이션 파일 0개. GEAR01_CargoStrap도 Export/Source 폴더가 전부
  비어있음(프롭 에셋 자체도 아직 없음). Variant 선택 전까지 착수 불가능한 상태.
  (참고: CHR00 자체 화물 스트랩 조작 애니메이션은 `updatedAnimation` "화물 스트랩/카라비너
  텐션" 세트로 이미 존재하지만, 이건 플레이어 본인이 스트랩 쓰는 동작이지 CHR01의
  Carabiner Assist[동료가 도와주는 동작]와는 다른 기능이라 재사용 불가.)

### ENY01_ComplianceRunner — normal / elite (각 48개, 같은 리스트 각자 리그로 생산)

0810 배치 로코모션/Strafe/Injured/Hit + ArtSource 승인분을 각 캐릭터 자기 자신의
`enemy_normal_RIGGED_textured.fbx` / `enemy_elite_RIGGED_textured.fbx`에 결합 export:

- 로코모션 10: Idle/Walking/Walking_Backwards/Standard_Run/Run_Backwards/Jump/
  Turn_Left/Turning_Right_45_Degrees/Left_Turn_90/Right_Turn_90
- Strafe 4: Left_Strafe/Right_Strafe/Left_Strafe_Walking/Right_Strafe_Walking
- Injured 20종 전부(idle/walk/run × 방향×회전 조합, 세트로 묶어 사용 권장)
- Hit_Reaction(0810) + approved 5종(Hit_Head/Hit_Body/Hit_Rib/Hit_Stomach/Hit_BigHeadHit)
- Melee_Candidate_Boxing/Boxing_2/Kicking/Kicking_2 — **후보 상태 유지**(root motion
  미확인이었던 걸 이번에 in-place 처리는 해뒀지만, 톤/최종 채택 여부는 Unity에서
  재생해보고 재판단 필요, 파일명에 `_candidate` 유지 안 함 주의 — 대신 이 문서에
  "후보"로 명시하니 Runtime 연결 전 확인할 것)
- Mutant_Punch/Mutant_Swiping/Mutant_Walking — **Producer 승인됨(2026-08-13)**, 그대로 적용
- One_Hand_Club_Combo — 맨손 스윙 컨셉 조건 하에 포함(무기 프롭 없이 사용할 것)

**이번 산출물에서 제외함(판단 근거)**:
- 총기 계열 6종(rifle aiming idle/rifle run/rifle jump/firing rifle/reloading/
  toss grenade) — 게임 캐논에 총기 없음, Owner 확인 요청 없이 바로 제외 처리
  (재수배 필요하면 그때 다시 소싱)
- Strafe 중복 후보 4종(strafe/strafe (2)/strafe left/strafe right) — 이름 규칙이 다른
  구버전 잔재로 판단, 이미 정상 좌우쌍(위 Strafe 4종)이 있어 중복. CHR00의 Strafe_L/R을
  ENY로 끌어오는 것도 검토했으나 불필요 — ENY 자체 세트가 이미 충분하고 자기 체형에
  맞는 리깅이라 그대로 감
- Stabbing, Jump Attack — 살상 연출/과장 연출 리스크(§8.4 저촉 소지, 기존
  CHR00 Dying1/Dying2가 "Weapon/military/lethal content" 사유로 ARCHIVE 처리된 것과
  같은 기준 적용). 필요해지면 Owner 재확인 후 별도 소싱

---

## 2. 검증 절차 — Humanoid로 연결할 것 (v001 §3 확장)

**반드시 Humanoid 방식으로 연결할 것. Generic 쓰지 말 것.** 이유: CHR00/CHR01/ENY01 전부
Mixamo 표준 스켈레톤(`mixamorig:` 본 이름 체계) 기반이라 Humanoid로 잡으면 (1) 파일마다
자기 자신의 Avatar를 자동 생성할 수 있고 (2) 나중에 다른 Mixamo 기반 캐릭터끼리 애니메이션
재사용/리타겟이 자유로움. Generic으로 잡으면 이 프로젝트가 지금까지 겪은 "Copy From Other
Avatar Rig Error" 문제를 원천적으로 피할 방법이 없어짐.

**단계별(Inspector 기준):**

1. fbx를 Unity 프로젝트 Assets 안에 넣음(드래그 or 파일 복사)
2. Project 창에서 그 fbx 선택 → Inspector 상단 탭 중 **Rig** 탭 클릭
3. **Animation Type** 드롭다운 → `Humanoid` 선택 (기본값이 `Generic`으로 잡혀있을 수 있음,
   반드시 바꿀 것)
4. **Avatar Definition** 드롭다운 → **`Create From This Model`** 선택
   — **`Copy From Other Avatar`는 절대 선택하지 말 것.** 이게 이번 프로젝트가 실제로
   겪었던 버그의 원인(파일마다 본 계층이 미세하게 달라서 다른 파일 Avatar 억지로
   갖다 붙이면 팔다리가 이상하게 꺾임). 각 fbx는 자기 메쉬+본이 이미 다 들어있으니
   무조건 자기 자신에서 새로 만들면 됨.
5. 하단 **Apply** 버튼 클릭 → Unity가 자동으로 본 이름(`mixamorig:Hips`,
   `mixamorig:LeftUpLeg` 등)을 Humanoid 표준 골격 슬롯에 매핑 시도함
6. **꼭 확인**: Rig 탭에 있는 **Configure...** 버튼 클릭 → Avatar 설정 화면 열림.
   화면에 초록색 사람 실루엣 나오는데, **본 매핑 실패한 부위는 빨간색/회색으로 표시됨**.
   전체 초록색이면 정상, 하나라도 빨갛게 뜨면 그 fbx 본 이름이 표준이랑 안 맞는다는
   뜻이니 Blender에서 이름 다시 확인해야 함(이번 CHR01/ENY01은 전부 Mixamo 표준이라
   정상적으로 자동 매핑됨 — 이미 확인됨, §0 참고)
7. 다 확인했으면 Configure 화면 나가서(우측 상단 Done 또는 다른 애셋 클릭) 저장
8. 씬에 캐릭터 배치 → **Animator** 컴포넌트 확인(Avatar 필드에 방금 만든 Avatar 자동으로
   들어가 있어야 함) → 재생해서 스켈레톤 리타겟 정상인지(팔다리 꺾임/뒤틀림 없는지),
   제자리에서 도는지, 인접 애니메이션과 자연스럽게 이어지는지 확인
9. 문제 있으면 v001 §3-4 체크리스트(Mesh 포함 export 실수, All Actions 옵션,
   Edit/Pose Mode 혼동, Quaternion 반대경로 회전)부터 의심

**여러 파일 한 번에 처리할 때**: fbx 여러 개를 Project 창에서 다중 선택(Shift/Ctrl+클릭)한
채로 Rig 탭 열면 설정을 한 번에 일괄 적용 가능 — Animation Type=Humanoid,
Avatar Definition=Create From This Model로 맞추고 Apply 누르면 선택한 파일 전부에 적용됨.
116개를 하나씩 할 필요 없음.

## 3. Git 커밋 규칙

v001과 동일: 목적 하나당 커밋 하나, `git add -A` 금지 정확한 경로만 stage, PR은
Owner 승인 없이 생성 금지.

---

## 4. Rodin → Mixamo 소싱 참고 (신규)

Rodin에서 LOD 폴더 받으면 `lod.fbx`(텍스처 없음) 대신 **`lod_basic_pbr.fbx`**
(diffuse/normal/roughness/metallic 임베드됨) 쓸 것. `lod_basic_shaded.fbx`는 라이팅이
구워진 텍스처라 쓰지 말 것. emissive는 어느 변형에도 안 박혀있어서 `texture_emissive.png`로
따로 받아 Mixamo 리깅 후 Blender에서 수동으로 재연결해야 기존 5텍스처 스펙(diffuse/
normal/metallic/roughness/emissive) 맞음. Mixamo 업로드 전에 fbx 안의 Camera/Cube/Light
잡동사니, LOD1~4는 지우고 LOD0 메쉬만 남길 것(리깅 계산엔 텍스처 무관, 지오메트리만 씀).
