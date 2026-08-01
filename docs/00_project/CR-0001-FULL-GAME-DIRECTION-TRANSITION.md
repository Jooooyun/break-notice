# CR-0001: Full-Game Direction Transition

- 요청자: `Human Owner`
- 날짜: `2026-08-01`
- 변경 등급: `C3`
- 변경 이유: 제한된 물리 액션 기준선을 하나의 authored full-game target으로 전환하고, 제작 확장 전에 G4-A evidence gate로 Core Loop를 검증하기 위함.
- 현재 Canon: `3D third-person physics-action Roguelite with run-oriented product framing`
- 제안 변경: `Single-campaign corporate physics-action adventure using the full FGD-001 ZERO DEFECT/HANDOVER Story, Campaign Map, Levels 0–6, Finale, Ending, and G4-A evidence gate`
- 플레이 경험 영향: 플레이어는 3.5–5시간 목표의 단일 authored campaign에서 ZERO DEFECT/HANDOVER 사건, 3막 구조, Official Plan과 As-Built Map, Levels 0–6, 구조 인원 약 15명, 중앙 아트리움 Finale와 insolvency Ending을 경험한다. 기존 물리 획득·릴레이·투척·Cargo Strap·인과 가독성은 핵심 행동으로 유지된다.
- 요구사항·RTM 영향: PR-F-018은 `P2 / COULD / DEFERRED` replay-layer 후보로 이동하고 기존 logical contract를 보존한다. 비살상 Player Failure와 encounter restart, 상위 진행 보존을 PR-F-025 및 AT-FAIL-001로 추가하고 RTM을 갱신한다. As-Built는 post-G4 requirement decomposition 전까지 FGD-001이 full-game target으로 소유한다.
- G2 적용성 영향: 승인된 G2 action, interaction arbitration, Weight Class, Momentum/Pip, Causal Chain, Ally Relay, Prototype 수량과 zero-destruction 값은 변경하지 않는다. Primary campaign에 즉시 필요하지 않은 run-oriented 적용성만 후속 Gate로 미룬다.
- Progression·Policy Violation 영향: Roguelite와 Policy Violation은 `DEFERRED REPLAY LAYER` 후보로 재분류한다. Policy의 선택·run 적용 logical contract는 증거로 보존하되 primary campaign, G4-A 또는 구현 범위가 아니다.
- Story·Campaign·Map·Level 권위 영향: FGD-001이 ZERO DEFECT/HANDOVER Story, 3막, Campaign Map, Levels 0–6, Hybrid Vertical Loop, As-Built Map, Finale와 Ending의 유일한 Owner-approved full-game target이다. 별도 Story·Map·Level 문서를 생성하지 않는다.
- Player Failure 영향: 죽음 대신 비살상 restraint와 현재 encounter entry baseline restart를 사용하며, encounter 이전에 완료된 Story·rescue·map·shortcut 진행은 유지한다. exact 수치·연출·복구 timing은 Draft다.
- 아트·애니메이션 영향: 기존 Visual Canon과 Production Support를 변경하지 않는다. FGD-001의 full-game target은 미래 production scope input이며 model, texture, material, rig, animation, UI 또는 신규 Visual 제작을 승인하지 않는다.
- 기술 영향: Unity 6000.3.20f1 documentation lock과 G3 technical baseline을 유지한다. campaign state, As-Built, save/persistence와 failure restore는 후속 decomposition이 필요하며 이번 변경은 schema, code, package, Scene, Prefab 또는 runtime 구현을 승인하지 않는다.
- 테스트 영향: G4-A는 FGD-001 §19의 한 room Core Loop evidence brief를 사용한다. PR-F-025는 AT-FAIL-001로 추적하고 AT-ROGUE-001은 dormant로 전환한다. G4-A 실행·passage는 별도 승인 전까지 시작하지 않는다.
- 일정·범위 영향: full-game target은 장기 3.5–5시간 플레이 목표이지 일정 추정이 아니다. Levels 0–6 제작과 campaign integration은 G4-A PASS 전까지 차단되고, 실제 일정은 G4-A production-speed evidence와 이후 G4-B/G5 evidence 뒤 산정한다.
- 되돌리기 계획: `G4-A failure does not silently revert to Roguelite.` 실패는 production expansion을 차단하고 Core Loop redesign을 요구한다. 승인된 product direction을 변경하려면 새로운 C3 Change Request와 Human Owner 결정이 필요하다.
- 결정: `APPROVED`
