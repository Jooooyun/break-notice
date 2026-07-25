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
- G5 Freeze:
  `NOT PASSED / FINAL PRE-IMPLEMENTATION REVIEW REVISION REQUIRED`
- G3 Technical checklist: `6 / 6`
- Design Freeze: `32 / 32`
- Design Freeze Completion:
  `COMPLETE / FINAL PRE-IMPLEMENTATION REVIEW COMPLETE — REVISION REQUIRED`
- Final Pre-Implementation Review:
  `REVISION REQUIRED / docs/05_production/FINAL_PRE_IMPLEMENTATION_REVIEW.md`
- Destruction Budget and Implementation Rules:
  `OWNER-APPROVED / LOCKED`
- Office Core Kit:
  `OWNER-APPROVED / LOCKED`
- CHR_00 Player Turnaround Brief:
  `OWNER-APPROVED / CONSUMED BY APPROVED TURNAROUND`
- CHR_00 Player Turnaround Production Support:
  `OWNER-APPROVED / REGISTERED / LOCKED PRODUCTION SUPPORT`
- First Implementation Ticket:
  `IMP-0001 / OWNER-APPROVED DECOMPOSITION / EXECUTION BLOCKED`
- Production Visual Lock sequence: `COMPLETE`
- Primary Production Visual Count: `8`
- Approved Production Support Count: `1`
- Full Design Lock:
  `INCOMPLETE / FINAL PRE-IMPLEMENTATION REVIEW REVISION REQUIRED`
- Graybox: `NOT STARTED`
- Phase label: `PRE-PRODUCTION / DESIGN FREEZE COMPLETION` — Owner-approved transition
- Phase label review: `COMPLETED / OWNER-APPROVED`
- Implementation Gate: `CLOSED`
- Current Next Task:
  `TASK-0061 — Correct IMP-0001 Baseline Binding and Limited Bootstrap Gate Sequencing`

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
유지한다. 이 Lock은 G5를 통과시키지 않는다. 첫 구현 티켓은
`docs/06_codex/IMP-0001-UNITY-REPOSITORY-BOOTSTRAP.md`로 세분화됐지만
concrete execution baseline SHA가 없고 실행은 차단되어 있다. Design
Freeze는 `32 / 32`와 `COMPLETE / FINAL PRE-IMPLEMENTATION REVIEW
COMPLETE — REVISION REQUIRED`다. Turnaround Production Brief의 binary review contract를
후보가 `PASS`했고 Human Owner가 `APPROVE`했으며 byte-identical
승인본과 ADR-0018이 `LOCKED PRODUCTION SUPPORT`로 등록되었다.
Production Visual은 계속 여덟 개이고 Production Support는 한 개다.
Final Pre-Implementation Review는 `REVISION REQUIRED`로 완료되었고
IMP-0001 baseline binding과 pre-G4 Full Design Lock·limited Gate
sequencing을 blocker로 기록했다. 현재 다음 단일 작업은
`TASK-0061 — Correct IMP-0001 Baseline Binding and Limited Bootstrap Gate
Sequencing`이다. G4는 `NOT PASSED / NOT STARTED / NOT AUTHORIZED`,
G5는 `NOT PASSED / FINAL PRE-IMPLEMENTATION REVIEW REVISION REQUIRED`,
Graybox는 `NOT STARTED`이고 구현 권한이 없다.
승인된 여덟 Production Visual의
Canon은 유지되지만 Full Design Lock은 `INCOMPLETE / FINAL
PRE-IMPLEMENTATION REVIEW REVISION REQUIRED`다. FIN_01은 PR-F-022
`OUT / COULD`이며 승인된 Vertical Slice와 최종 제품 범위 밖이다.
정확한 조명·피니셔 기능, destruction hard cap을 제외한 tuning,
implementation class와 pass threshold는 Draft 또는 미승인 상태이며
Implementation Gate는 `CLOSED`다.
