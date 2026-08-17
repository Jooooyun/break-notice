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

## 2. 검증 절차 (v001 §3과 동일, 재확인만)

1. Unity 프로젝트에 fbx import
2. **"Create Avatar From This Model"로 새 Avatar 생성** — 절대 다른 파일 Avatar Copy 금지
3. 실제 재생: 스켈레톤 리타겟 정상인지(팔다리 꺾임/뒤틀림 없는지), 제자리에서 도는지,
   인접 애니메이션과 자연스럽게 이어지는지
4. 문제 있으면 v001 §3-4 체크리스트(Mesh 포함 export 실수, All Actions 옵션,
   Edit/Pose Mode 혼동, Quaternion 반대경로 회전)부터 의심

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

---

## 5. Unity에서 텍스처 흰색/회색으로만 나올 때 (신규, 실제로 겪음)

`path_mode='COPY', embed_textures=True`로 export해도(§1 참고) **Unity에서 열면 캐릭터가
텍스처 없이 밋밋한 흰색으로 나올 수 있음**. 원인: Blender FBX exporter가 diffuse/normal
같은 일부 채널은 텍스처를 바이트로 fbx 안에 넣긴 하지만, FBX 레거시 머티리얼 포맷
자체엔 Metallic/Roughness 같은 PBR 슬롯이 없어서 머티리얼 속성-텍스처 연결 자체가
씹힘 — Unity 쪽에서 보면 셰이더가 `Standard`(URP 프로젝트인데 legacy shader)로 들어오고
텍스처 슬롯 전부 NULL, 심지어 `embedded_texture_count=0`(Unity가 텍스처 서브에셋 자체를
못 찾음)로 나옴. **Blender에서 packed=true로 확인되는 것과 Unity가 실제로 그 텍스처를
읽어오는 것은 별개 문제** — Blender 쪽 검증만으로는 이 버그 못 잡음, Unity에서 직접
열어봐야 걸림.

**해결 절차**(CHR01/ENY01에 이미 적용 완료, `Game/Assets/_Project/Art/Materials/Fixed/`
참고):
1. Blender에서 캐릭터 베이스 fbx(`female_helper.fbx` 등) 열어서 `bpy.data.images`의
   각 이미지를 `img.save(filepath=...)`로 PNG 파일로 직접 추출(embedded 상태론 못 씀,
   실제 파일로 뽑아야 함)
2. 추출한 PNG를 Unity `Assets/.../Textures/{캐릭터}/`에 넣기
3. `Shader.Find("Universal Render Pipeline/Lit")`로 머티리얼 새로 만들고 `_BaseMap`
   (diffuse), `_BumpMap`(normal, TextureImporterType.NormalMap으로 import 설정 바꿔야 함)
   수동으로 연결. Metallic/Roughness는 텍스처로 안 넣고 `_Metallic`/`_Smoothness` 슬라이더
   값으로 근사(정밀 PBR 필요하면 나중에 채널 합성해서 텍스처로 교체)
4. 캐릭터별 fbx 전부에 대해 `ModelImporter.AddRemap(new SourceAssetIdentifier(typeof(Material),
   "원래머티리얼이름"), 새머티리얼)` 호출 후 `SaveAndReimport()` — Blender 재export 없이
   Unity 쪽에서 머티리얼만 갈아끼우는 방식, 훨씬 빠름
