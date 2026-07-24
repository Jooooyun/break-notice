# Milestones and Gates

| Gate | 통과 조건 | 실패 시 |
|---|---|---|
| G0 Charter | 목표·제약·권한 승인 | 프로젝트 재정의 |
| G1 Requirements | MUST 요구사항과 검증 완비 | 요구사항 보완 |
| G2 Design | 시스템·아트·범위 모순 없음 | 설계 수정 |
| G3 Technical | 책임·데이터·테스트·예산 매핑 | 기술 설계 수정 |
| G4 Graybox | 5분 재미 지표 통과 | 수치·카메라·배치 조정, 필요 시 CR |
| G5 Freeze | 실제 제작 범위와 에셋 목록 확정 | 본 제작 금지 |
| G6 Slice | 기능·성능·인수 기준 통과 | 범위 축소 또는 수정 |
| G7 Decision | 시장성·재플레이·유지 가능성 증거 | 확장/축소/중단 결정 |

## 현재 위치

- G1 Requirements: `PASSED`
- G2 Design: `PASSED / OWNER-APPROVED BASELINE`
- G3 Technical: `PASSED / OWNER-APPROVED BASELINE`
- G3 Technical Baseline: `OWNER-APPROVED / PASSED`
- G3 Technical Decisions:
  `OWNER-APPROVED / BASELINE APPLIED`
- G3 Causal Result-Family Alignment:
  `OWNER-APPROVED / CORRECTED`
- G3 Technical Final Audit:
  `REVISION REQUIRED / HISTORICAL BASELINE AUDIT`
- G3 Residual Active Status and Package-Authority Correction:
  `OWNER-APPROVED / COMPLETE`
- G3 Technical Final Audit Remediation:
  `COMPLETE / VERIFIED BY RE-AUDIT`
- G3 Technical Baseline Re-Audit:
  `READY FOR G3 GATE DECISION — CONSUMED BY G3 PASSAGE`
- G3 Gate Passage Source:
  `docs/05_production/G3_TECHNICAL_BASELINE_REAUDIT.md` at
  `9e1487e85be5a54ba0824c209deabcf738e1abf0`
- G4 Graybox: `NOT PASSED / NOT STARTED / NOT AUTHORIZED`
- G5 Freeze: `NOT PASSED`
- G3 Technical checklist: `6 / 6`
- Design Freeze: `30 / 32`
- Destruction Budget and Implementation Rules:
  `OWNER-APPROVED / LOCKED`
- Office Core Kit:
  `OWNER-APPROVED / LOCKED`
- CHR_00 Player Turnaround Brief:
  `OWNER-APPROVED / READY`
- CHR_00 Player Turnaround Image:
  `NOT CREATED / IMAGE GENERATION NOT AUTHORIZED`
- Production Visual Lock sequence: `COMPLETE`
- Full Design Lock: `INCOMPLETE`
- Graybox: `NOT STARTED`
- Phase label: `PRE-PRODUCTION / DESIGN FREEZE COMPLETION` — Owner-approved transition
- Phase label review: `COMPLETED / OWNER-APPROVED`
- Implementation Gate: `CLOSED`

G1의 정확한 요구사항·NFR·행동 검증 의도와 범위 경계, 여덟 G2
계약과 교정된 authority/trace 기준선은 Owner-approved이며 G1과 G2
Gate는 `PASSED`다. G3의 책임·데이터·이벤트·검증·예산·Reset·
test-harness 방향과 exact Editor/package documentation lock은
Owner-approved 기준선으로 적용되었고 Technical checklist는 `6 / 6`다.
역사적 Final Audit의 두 잔여 권한 오류는 교정됐고 Re-Audit의
`READY FOR G3 GATE DECISION` 판정과 Owner 승인을 근거로 G3 Gate는
`PASSED`다. Re-Audit 문서와 역사적 Final Audit은 변경하지 않는다.
Prototype의 모든 파괴 category cap `0`과 Later-Slice의 bounded
room·event·fragment·small-material hard cap, authored-state·pooling·
atomic-admission·causal-carrier·Reset·fault 규칙은 Owner-approved로
LOCK되었다. Structural Feature는 `VERTICAL SLICE / SHOULD / PARTIAL`
및 별도 G5 승인 조건을 유지한다. Office Core Kit은 정확한
Prototype-required family `16`개와 Later-Slice extension candidate
`8`개, interaction·variant·reuse 및 Prototype destruction `0`
경계로 `OWNER-APPROVED / LOCKED`되었다. 실제 model, texture, material,
layout과 Unity asset creation은 `NOT STARTED / NOT AUTHORIZED`이며
LGT_01은 Records Archive-local `COULD`, FIN_01은 `OUT / COULD`를
유지한다. 이 Lock은 G5를 통과시키지 않는다. Design Freeze는
`30 / 32`이며 남은 항목은 `CHR_00 Player Turnaround`과 `첫 구현
티켓 세분화`의 정확히 두 개다. Turnaround Production Brief와 binary
review contract는 `OWNER-APPROVED / READY`지만 미래 후보는
`NOT CREATED`, 이미지 생성은 `NOT AUTHORIZED`이고 checkbox는
`UNCHECKED`다. 현재 다음 허가 경계는
`OWNER EXPLICIT IMAGE-GENERATION REQUEST REQUIRED — CHR_00 PLAYER TURNAROUND`이며
새 numbered task는 승인되지 않았다.
G4와 G5는 `NOT PASSED`, Graybox는 `NOT STARTED`이고 구현 권한이 없다.
승인된 여덟 Production Visual의
Canon은 유지되지만 Full Design Lock은 미완료다. FIN_01은 PR-F-022
`OUT / COULD`이며 승인된 Vertical Slice와 최종 제품 범위 밖이다.
정확한 조명·피니셔 기능, destruction hard cap을 제외한 tuning,
implementation class와 pass threshold는 Draft 또는 미승인 상태이며
Implementation Gate는 `CLOSED`다.
