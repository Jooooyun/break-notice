# Decision Log

| ID | 날짜 | 결정 | 상태 | 근거 |
|---|---|---|---|---|
| DL-001 | 2026-07-16 | 2D가 아닌 3D TPS로 제작 | LOCKED | 동료 패스, 높이·각도·거리, 공간 파괴가 핵심 |
| DL-002 | 2026-07-16 | Unity 6 + URP + GameObject PhysX | LOCKED | 기존 경험 활용, 빠른 반복, 제한적 물리 파괴에 적합 |
| DL-003 | 2026-07-16 | 원거리 염력 Grab 제거 | LOCKED | 평범한 회사원 판타지와 충돌 |
| DL-004 | 2026-07-16 | 직접 집기 + 별도 화물 스트랩 + 동료 패스 | LOCKED | 템포와 설정 설득력을 동시에 유지 |
| DL-005 | 2026-07-16 | 붉은 넥타이는 복장 전용 | LOCKED | 스트랩과 시각·기능 혼동 방지 |
| DL-006 | 2026-07-16 | 일반 전투 활성 동료 1명 | LOCKED | AI·애니메이션·가독성·성능 범위 통제 |
| DL-007 | 2026-07-16 | Grounded Corporate Action Realism | LOCKED | 애니풍 배제, 현실적 인물과 과장된 결과의 대비 |
| DL-008 | 2026-07-16 | 폭포수 + Graybox Validation Gate | LOCKED | 설계 흔들림을 막되 재미를 문서만으로 가정하지 않음 |
| DL-009 | 2026-07-16 | `CHR_00_Player_Identity_Lock_v002_FINAL` 승인 | LOCKED | 주인공 실루엣과 산업 장비 정체성 확정 |
| DL-010 | 2026-07-16 | 완전 절차 생성·Co-op·완전 파괴 제외 | LOCKED | 1인 개발 리스크 제한 |
| DL-011 | 2026-07-17 | `GEAR_01_Cargo_Strapping_System_v002_FINAL` 승인 | LOCKED | 별도 산업용 스트랩의 장비 구성, 연결 체인, 여섯 시각 상태 확정 |
| DL-012 | 2026-07-17 | `CHR_01_First_Ally_Identity_v001_FINAL` 승인 | LOCKED | 첫 동료의 시설·물류 직원 정체성과 실용 작업복 실루엣 확정 |
| DL-013 | 2026-07-17 | `PGT_01_Gameplay_Camera_v002_FINAL` 승인 | LOCKED | 실제 3인칭 gameplay framing과 전장 가독성 기준 확정 |
| DL-014 | 2026-07-17 | `PGT_02_Chair_Redirect_Core_Loop_v010_FINAL` 승인 | LOCKED | 동료 공급·완전 해제에서 플레이어 기계식 리디렉션으로 이어지는 충돌 전 시각 인과 확정 |
| DL-015 | 2026-07-18 | `PGT_03_Chain_Reaction_Aftermath_v003_FINAL` 승인 | LOCKED | 국소 Chain Aftermath, 새로운 visibility line, 통제된 파괴와 느슨한 post-impact 스트랩 상태 확정 |
| DL-016 | 2026-07-18 | `LGT_01_Night_Archive_Lighting_v001_FINAL` 승인 | LOCKED | Records Archive 지역의 after-hours 조명 변주와 gameplay readability 방향 확정 |
| DL-017 | 2026-07-22 | `FIN_01_Conference_Table_Team_Finisher_v003_FINAL` 승인 | LOCKED | 3인 Heavy 테이블 공동 추진과 CHR_00 주도권의 Visual-only conditional direction 확정; PR-F-022는 COULD 유지 |
| DL-018 | 2026-07-24 | G1 요구사항·NFR·기술 중립적 행동 검증 의도·적용 범위의 정확한 문구와 disposition 승인 | OWNER-APPROVED | Owner의 TASK-0033 G1 Baseline 승인; 구현·검증 완료와 G2 통과는 별도 |
| DL-019 | 2026-07-24 | 한 문장 제품 정의, Target Player, Windows PC 기준 플랫폼과 정성적 5분 재미 의도 승인 | OWNER-APPROVED | 정확한 시간·비율·표본·횟수·하드웨어·해상도·성능값은 G4/G5/G6 Draft 유지 |
| DL-020 | 2026-07-24 | 동기화 및 전체 검증 성공 후 G1 Requirements Gate 통과 기록 | PASSED | 현재 Phase label 유지, G2 미통과, Full Design Lock 미완료, Graybox 미시작, Implementation Gate CLOSED |
| DL-021 | 2026-07-24 | Phase label을 `PRE-PRODUCTION / PRODUCTION VISUAL LOCK`에서 `PRE-PRODUCTION / G2 DESIGN BASELINE`으로 변경 | OWNER-APPROVED | G1 PASSED 유지; G2 미통과, Full Design Lock 미완료, Graybox 미시작, Implementation Gate CLOSED |
| DL-022 | 2026-07-24 | Owner-approved G2 Logical Input Action Contract | OWNER-APPROVED | 9개 logical action의 이름과 intent 의미만 승인; physical binding, timing, arbitration, state conflict와 구현은 Draft 유지 |
| DL-023 | 2026-07-24 | Owner-approved G2 Interaction State, Arbitration, and Conflict Contract | OWNER-APPROVED | Scoop/Catch/Throw/CallAlly/Cargo Strap/Reset conflict 승인; exact timing·target ranking·weight eligibility·animation·physics·구현은 Draft 유지 |
| DL-024 | 2026-07-24 | Owner-approved G2 Weight-Class Functional Data Range Contract | OWNER-APPROVED | Light/Medium/Heavy functional envelope과 eligibility 승인; kilogram·force·speed·damage·timing·Momentum/Pip·animation·physics·구현은 Draft 유지 |
| DL-025 | 2026-07-24 | Owner-approved G2 Momentum/Pip Starting-Value and Resource Lifecycle Contract | OWNER-APPROVED | threshold·capacity·reward table·modifiers·cap·persistence와 HeavyAction 시작 비용 승인; Chain propagation·final tuning·animation·physics·구현은 Draft 유지 |
