# CHR01 — Fixed Ally Delivery

Asset ID: CHR01
Asset Name: Fixed Ally
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
Reference Used: Rodin(모델 생성), Mixamo(auto-rig + 애니메이션 retarget — 검색어: Throw/Frisbee Throw/Goalkeeper Overhand Throw/Throw In), GPT(Sora) 생성 참조 이미지(작업자A 제작)

Source Files: 로컬 Blender 작업 파일(미커밋)
Export Files: ArtSource/Characters/CHR01_Ally/Animations/CHR01_Supply_Binder_Windup.fbx, CHR01_Supply_Binder_Release.fbx
Texture Files: N/A(애니메이션 전용, mesh 미포함)
Animation Files:
  완료: CHR01_Supply_Binder_Windup(단일 고정 프레임, 팔 들어올린 대기 자세),
  CHR01_Supply_Binder_Release(릴리즈+회복, in-place 확인 완료, 20~67프레임 트림)
  미완료: CHR01_Supply_Binder_Abort — 소스 없음
  로코모션: CHR00 파일 재사용 예정(65본 서브셋 확인됨)이나 아직 실제 복사/적용 안 함
  §6 Carabiner Assist(Variant A/B/C, 약 20개 클립) — 전량 미착수
  §6.5 추가 리액션(Duck/Flinch/Step_Back 등) — 전량 미착수
Audio Files: N/A
UI Files: N/A
VFX Files: N/A

Polygon Count: N/A
Material Slots: N/A
Texture Resolution: N/A
Scale: 1,1,1
Pivot: feet-ground center
Forward Axis: +Z
Rig Type: Humanoid (Mixamo auto-rig, 65본)
Animation Type: Full Body, in-place (root motion baked out)
Loop Clips: Supply_Binder_Windup만 Yes(정지 홀드), 나머지 No

Known Issues:
  - Mixamo 다운로드 기본 root motion 포함 → Blender 스크립트로 hip XY 고정, Z만 보존해 in-place 변환
  - Supply_Binder_Windup 제작 중 "새 액션 생성 후 export" 방식에서 값이 소실되는
    버그 발견 — "기존 액션 프레임 범위만 트림" 방식으로 우회 해결함(원인 미상)
  - **[결정 요청] CHR01 이동 가능 여부.** 저장소 문서상 이름은 "Fixed Ally"라 고정형처럼
    읽히지만, 실제로는 Walk_F/B, Strafe_L/R, Turn류 로코모션 파일이 이미 CHR00과 같은
    스켈레톤으로 재사용 가능한 상태로 준비돼 있습니다. 검토 결과 두 시나리오 중 하나입니다:
    (A) 이름 그대로 고정 위치 지원형 → 이 로코모션 파일들은 전부 스코프 아웃, 작업량 감소
    (B) 이름과 무관하게 실제로는 플레이어를 따라다니며 지원 → 로코모션 그대로 사용, 계획대로 진행
    로코모션 파일이 이미 손쉽게 재사용 가능한 상태라는 점을 감안하면 (B) 쪽에 무게를
    두고 있으나 확정은 아닙니다. **A/B 중 선택만 알려주시면 그에 맞춰 바로 진행합니다.**
  - **[결정 요청] §6 Carabiner Assist 방식.** 세 변형(A: 동료가 Chair만 고정 잡아줌 /
    B: 동료가 직접 카라비너를 걸고 잠금 / C: 동료가 연결하면 플레이어가 바로 장력을
    이어받음)이 문서에 정의돼 있고, 상호 배타적이라 하나(또는 조합) 선택이 필요합니다.
    난이도/제작 부담 기준으로는 A(단순 고정)가 제일 적고, C(협동 인계)가 제일 많습니다.
    아직 셋 다 미착수 상태라 **어느 변형으로 갈지(또는 여러 개 다 만들지) 결정만
    주시면 그 변형부터 소싱 시작하겠습니다.**
Requested Integration: Unity Animator import 아직 테스트 안 함(CHR01 실제 캐릭터로는 미검증)
Blocking Feature: Chair Feed(§5.3), Carabiner Assist(§6) 전체
Suggested Runtime Path: 통합 담당(Human Owner/Codex)이 결정
Preview Files: 세션 중 Blender headless 렌더로 Supply_Binder만 검증(파일 미보관)

Status: WORK IN PROGRESS (Supply_Binder 일부만 완료, 전체 스펙 대비 약 20~30% 수준)
