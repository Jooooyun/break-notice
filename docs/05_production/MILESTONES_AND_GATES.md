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
- G3 Technical: `NOT PASSED / CURRENT BASELINE WORK`
- G3 Technical Decisions:
  `OWNER-APPROVED / BASELINE APPLIED`
- G3 Causal Result-Family Alignment:
  `OWNER-APPROVED / CORRECTED`
- G3 Technical Final Audit:
  `REVISION REQUIRED / HISTORICAL BASELINE AUDIT`
- G3 Residual Active Status and Package-Authority Correction:
  `OWNER-APPROVED / COMPLETE`
- G3 Technical Final Audit Remediation:
  `CORRECTIONS APPLIED / VERIFIED BY RE-AUDIT`
- G3 Technical Baseline Re-Audit:
  `READY FOR G3 GATE DECISION`
- G3 Technical checklist: `6 / 6`
- Design Freeze: `28 / 32`
- Production Visual Lock sequence: `COMPLETE`
- Full Design Lock: `INCOMPLETE`
- Graybox: `NOT STARTED`
- Phase label: `PRE-PRODUCTION / G3 TECHNICAL BASELINE` — Owner-approved transition
- Phase label review: `COMPLETED / OWNER-APPROVED`
- Implementation Gate: `CLOSED`

G1의 정확한 요구사항·NFR·행동 검증 의도와 범위 경계, 여덟 G2
계약과 교정된 authority/trace 기준선은 Owner-approved이며 G1과 G2
Gate는 `PASSED`다. G3의 책임·데이터·이벤트·검증·예산·Reset·
test-harness 방향과 exact Editor/package documentation lock은
Owner-approved 기준선으로 적용되었고 Technical checklist는 `6 / 6`다.
현재 다음 Gate 작업은
`TASK-0054 — Promote G3 Technical Baseline and Record G3 Passage`다.
역사적 Final Audit의 두 잔여 권한 오류는 교정됐고 Re-Audit은
`READY FOR G3 GATE DECISION`이지만 별도 Gate 결정 전까지 G3는
`NOT PASSED`이며 G4, Graybox와 구현 권한이
없다. 승인된 여덟 Production Visual의
Canon은 유지되지만 Full Design Lock은 미완료다. FIN_01은 PR-F-022
`OUT / COULD`이며 승인된 Vertical Slice와 최종 제품 범위 밖이다.
정확한 조명·파괴·피니셔 기능, tuning, implementation class와
pass threshold는 Draft 또는 미승인 상태다.
