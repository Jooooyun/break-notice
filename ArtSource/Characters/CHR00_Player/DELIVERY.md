# CHR00 — Player Delivery

Asset ID: CHR00
Asset Name: Player
Creator: AutumnCrab
GitHub ID: AutumnCrab
Contribution Date: 2026-08-12

Work Classification:
- Mixed (AI Generated model + third-party rig/animation retarget)

Exact License:
- Model (Rodin): Hyper3D Rodin 유료 구독 플랜 사용(0.5 credit) — 상업적 사용 라이선스 포함, 생성물 소유권 사용자 귀속
- Rig/Animation (Mixamo): Adobe Mixamo 무료 라이선스 — 상업/비상업 무제한 사용, 로열티 없음
- 제약: Mixamo raw FBX를 독립 애셋팩으로 재판매/재배포 금지. 게임 내장 출시는 허용 범위.
Written Permission: 별도 서면 허가 불필요 — Rodin 유료 플랜 상업 라이선스 + Mixamo 무료 상업 라이선스로 커버됨
Reference Used: Rodin(모델 생성), Mixamo(auto-rig + 애니메이션 retarget — 검색어: Idle/Walking/Strafe/Turn/Knocked Down/Stand Up/Goalkeeper Catch/Throw/Frisbee Throw/Throw In 등), GPT(Sora) 생성 참조 이미지(작업자A 제작)

Source Files: 로컬 Blender 작업 파일(미커밋, Downloads 스크래치 폴더)
Export Files: ArtSource/Characters/CHR00_Player/Animations/ 아래 전체(목록은 Animation Files 참고)
Texture Files: N/A(애니메이션 전용 딜리버리, mesh 미포함 — Without Skin 방식)
Animation Files:
  로코모션: CHR00_Idle, CHR00_Idle_Variant_Stretch, CHR00_Walk_F, CHR00_Run_F,
  CHR00_Strafe_L(+Short_ALT), CHR00_Strafe_R(+Short_ALT), CHR00_Turn_L_90(+Loop_ALT),
  CHR00_Turn_R_90(+Loop_ALT), CHR00_Turn_180_Running, CHR00_Turn_180_Walking,
  CHR00_Walk_Stop, CHR00_Run_Stop, CHR00_Jump
  전투/리액션: CHR00_Knockdown_F, CHR00_Knockdown_B, CHR00_Getup_F, CHR00_Getup_B,
  CHR00_Melee_Hit_Body
  핵심 메커니즘: CHR00_Aim_Idle, CHR00_Catch_Small_Center, CHR00_Catch_Small_Diving_L,
  CHR00_Catch_Small_Diving_R, CHR00_Catch_Medium, CHR00_Catch_Large, CHR00_Throw_Small,
  CHR00_Throw_Medium, CHR00_Throw_Large, CHR00_Strap_Tension_Loop_candidate(미확정 후보)
Audio Files: N/A
UI Files: N/A
VFX Files: N/A

Polygon Count: N/A(mesh 미포함 딜리버리)
Material Slots: N/A
Texture Resolution: N/A
Scale: 1,1,1
Pivot: feet-ground center
Forward Axis: +Z
Rig Type: Humanoid (Mixamo auto-rig, 41본)
Animation Type: Full Body / 일부 Upper Body(Aim_Idle), 전부 in-place(root motion baked out)
Loop Clips: Idle·Strafe·Turn류만 Yes, 나머지(Knockdown/Catch/Throw 등)는 No(1회성)

Known Issues:
  - CHR00_Idle: 팔 벌리는 제스처 자세임 — 오진단했다가 Human Owner 확인 결과 의도된 디자인으로 확정됨, 버그 아님
  - CHR00_Aim_Idle: 전용 소스 없어서 Throw 애니메이션 windup 프레임을 정지시킨 임시 대체물. 추후 전용 Aim 애니메이션으로 교체 권장
  - CHR00_Strap_Tension_Loop_candidate: Pulling A Rope 소스 재활용한 미확정 후보, Preview/승인 안 거침
  - _Short_ALT / _Loop_ALT 접미사 파일들은 대안 테이크, 코어 딜리버리 아니고 참고용으로 같이 넣어둠
  - CHR00_Throw_Medium/Large: 어깨너비/종료자세 관련 여러 차례 재수정 거침, 최종본은 렌더로 시작-종료 자세 일치 검증 완료
Requested Integration: Unity Animator import, retarget 확인 완료(작업자A 측 Codex 연동 테스트)
Blocking Feature: Strap 시스템(Deploy/Carabiner/Attach/Redirect/Release) 전체 미착수 — Runtime Strap 연동은 이 딜리버리에 안 포함됨
Suggested Runtime Path: 통합 담당(Human Owner/Codex)이 결정
Preview Files: 세션 중 Blender headless 렌더로 다수 검증(파일 미보관, 텍스트 로그로만 확인)

Status: WORK IN PROGRESS (Strap 계열 제외 로코모션/Catch/Throw는 완료)
