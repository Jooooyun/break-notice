# ENY01 — Compliance Runner Delivery

Asset ID: ENY01
Asset Name: Compliance Runner (normal / elite 두 인스턴스가 동일 스켈레톤 공유)
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
Reference Used: Rodin(모델 생성), Mixamo(auto-rig + 애니메이션 retarget — 검색어: Boxing/Kicking/Big Hit To Head/Head Hit/Hit To Body/Rib Hit/Stomach Hit)

Source Files: 로컬 Blender 작업 파일(미커밋)
Export Files: ArtSource/Characters/ENY01_ComplianceRunner/Animations/ 아래
Texture Files: N/A(애니메이션 전용, mesh 미포함)
Animation Files:
  로코모션(normal/elite 공용): 기존 0810 배치에 이미 완비(Walk F/B, Strafe, Turn,
  Run, Injured 세트 포함) — 재사용, 신규 제작 없음
  Hit Reaction: 기존 hit reaction.fbx(0810 배치) + eliteAnim/approved 5종
  (Big Hit To Head, Head Hit, Hit To Body, Rib Hit, Stomach Hit) — 바로 사용 가능
  근접 공격 후보(미확정): Melee_candidate_Boxing, Melee_candidate_Boxing_2,
  Melee_candidate_Kicking, Melee_candidate_Kicking_2 — root motion 미처리,
  톤 검토 안 됨(§8.4 "평범한 회사 보안" 기준 부합 여부 확인 필요)
  **[승인됨— 2026-08-13 Producer 확인] `Animations/pending_producer_review/` 6종,
  Unity에 로드해서 검토 진행 가능.** 개별 톤 판정(참고용, 여전히 유효):
  - `Mutant Punch/Swiping/Walking` 3종: Mixamo "Mutant"(괴물/좀비용) 모션 팩,
    "평범한 회사 사람" 정체성과 충돌 — 실사용 여부는 Unity 확인 후 재판단 권장
  - `One Hand Club Combo`: 무기 프롭 없이 맨손 스윙으로 쓰면 조건부 적합
  - `Stabbing`: 칼 찌르기로 보여 처형/살상 연출 위험 — Unity 확인 후 재판단 권장
  - `Jump Attack`: §8.4 "elite-only 과장 연출" 금지 항목에 근접 — Unity 확인 후 재판단 권장
  Producer 승인은 "폴더를 Runtime/Unity로 불러와서 실제로 확인해본다"는 의미이며,
  개별 클립의 최종 Canon 채택 여부까지 자동 승인된 것은 아님. Unity에서 확인한 뒤
  각 클립 채택/폐기를 다시 확정할 것.
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
Animation Type: Full Body, in-place(root motion 미확인분 있음 — 아래 참고)
Loop Clips: Idle/로코모션류 Yes, Hit/공격류 No

Known Issues:
  - Melee_candidate_* 4종은 root motion 유무 미확인 — Runtime 적용 전 반드시 체크 필요
  - Jab/Straight/Hook/Overhand/Forearm Check/Two-hand Shove(v003 §8.1 권장) 및
    Grab/Control(§8.3, 비살상 제지) 추가 소싱 안 됨 — 근접 공격 커버리지 부족
  - **[결정 요청] Elite 근접 공격 방향.** 위 6종이 거의 다 부적합 판정 나면서 Elite
    전용 공격이 사실상 비어있는 상태입니다. 대안 두 가지를 검토했습니다:
    (A) Normal과 완전히 동일한 근접 공격 재사용 → 제작 부담 0이지만 "둔탁하고 강력함"
    차별화가 안 됨, Elite로서 존재 의미가 옅어짐
    (B) 같은 동작을 더 느리고 크게(예: 스윙 속도 절반, 텔레그래프 프레임 길게) 재활용
    → 원래 계획했던 방향, 새 소스 필요 없이 기존 Normal 클립을 프레임/타이밍만
    조정해서 만들 수 있어 제작 부담이 크지 않음
    **저는 (B) 방향을 권장합니다** — 새로 만들 것 없이 기존 Normal 애니메이션의
    재생 속도/텔레그래프만 조정하면 되고, Elite 차별화 목적에도 맞습니다.
    이견 없으시면 (B)로 진행하겠습니다.
Requested Integration: Unity Animator import 미검증
Blocking Feature: 근접 공격 세트 전체(Normal/Elite 둘 다 최종 확정 안 됨)
Suggested Runtime Path: 통합 담당(Human Owner/Codex)이 결정
Preview Files: 세션 중 Blender headless 렌더로 Hit 5종만 검증(파일 미보관)

Status: WORK IN PROGRESS (로코모션·Hit는 완료, 근접 공격은 후보 단계, Elite 방향 미확정)
