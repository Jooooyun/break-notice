# Design Status Board

현재 단계: `PRE-PRODUCTION / G2 DESIGN BASELINE`

G1 Requirements: `PASSED`

G2 Design: `NOT PASSED / CURRENT BASELINE WORK`

Production Visual Lock sequence: `COMPLETE`

Full Design Lock: `INCOMPLETE`

Graybox: `NOT STARTED`

Phase label review: `COMPLETED / OWNER-APPROVED`

Implementation Gate: `CLOSED`

| 영역 | 상태 | 다음 산출물 | Gate |
|---|---|---|---|
| G1 Requirements Baseline | OWNER-APPROVED / PASSED | G2 Design baseline | G1 PASSED |
| 제품 비전 | OWNER-APPROVED G1 BASELINE | 상세 설계 정렬 | G1 PASSED |
| Logical Input Action Contract | OWNER-APPROVED / G2 PARTIAL | Interaction State and Conflict Contract와 함께 유지 | G2 |
| Interaction State and Conflict Contract | OWNER-APPROVED / G2 PARTIAL | Light/Medium/Heavy data range contract | G2 |
| 핵심 전투 동사 | REQUIREMENTS OWNER-APPROVED / DESIGN DRAFT | 상세 게임플레이 설계 | G2 |
| 동료 릴레이 | VISUAL LOCKED / FUNCTIONAL DRAFT | 상세 게임플레이 설계 | G2 |
| 화물 스트랩 | VISUAL LOCKED / FUNCTIONAL DRAFT | 상세 게임플레이 설계 | G2 |
| CHR_00 Player Identity | LOCKED | 없음 | G2 |
| CHR_01 First Ally Identity | LOCKED | 없음 | G2 |
| 카메라 | VISUAL LOCKED / FUNCTIONAL DRAFT | 상세 카메라 설계 | G2 |
| Chair Redirect Core Loop | VISUAL LOCKED / FUNCTIONAL DRAFT | 상세 Chair Redirect 설계 | G2 |
| PGT_03 Chain Reaction Aftermath | VISUAL LOCKED / FUNCTIONAL DRAFT | 상세 Chain·파괴 결과 설계 | G2 |
| LGT_01 Night Archive Lighting | VISUAL LOCKED / TECHNICAL DRAFT | 정확한 조명·렌더링 설계 | G2 |
| FIN_01 Conference Table Team Finisher | VISUAL LOCKED / FEATURE COULD / FUNCTIONAL DRAFT | 제품 범위 결정 전 구현 금지 | G2 |
| 파괴 예산 | HIGH-LEVEL BASELINE | 정확한 파편·Particle·VFX·성능 예산 | G2 |
| 적 1~3종 | DRAFT | 실루엣 시트 | G2 |
| 기술 아키텍처 | DRAFT | Prototype 상세 설계 | G3 |
| Graybox | NOT STARTED | Design Readiness Gate 통과 후 | G4 |
| 본 제작 | PENDING | Implementation Gate 개방 후 | G5 |

## Production Visual Lock Sequence

1. `GEAR_01 — Cargo Strapping System` — `LOCKED`
2. `CHR_01 — First Ally Identity` — `LOCKED`
3. `PGT_01 — Gameplay Camera` — `LOCKED`
4. `PGT_02 — Chair Redirect Core Loop` — `LOCKED`
5. `PGT_03 — Chain Reaction Aftermath` — `LOCKED`
6. `LGT_01 — Night Archive Lighting` — `LOCKED`
7. `FIN_01 — Conference Table Team Finisher` — `LOCKED`

현재 Production Visual Lock sequence는 완료되었다. 다음 Production Visual 대상은 승인되지 않았다.

FIN_01은 PR-F-022 `COULD` 기능의 Visual-only conditional direction이며 승인된 Vertical Slice 또는 최종 제품 범위에 포함되지 않는다. Full Design Lock은 여전히 미완료이고 Implementation Gate는 `CLOSED`다.

`CHR_00 Player Turnaround`은 향후 3D 모델링용 `PENDING` Production Support 산출물이며 현재 Production Visual Lock 순서를 차단하지 않는다.

G1 요구사항·NFR의 정확한 문구와 disposition, 기술 중립적 검증
의도와 Applicability/Priority 경계는 Owner-approved이며 G1
Requirements Gate는 `PASSED`다. 상세 게임플레이 설계, G2
Light/Medium/Heavy 데이터 범위, Acceptance Test의 정확한 수치·임계값, 파괴 예산,
카메라 기능, 조명·렌더링 구현, FIN_01 제품 편입, 기술 아키텍처,
구현 계획과 Vertical Slice의 정확한 콘텐츠는 `DRAFT`, `COULD`,
`DEFERRED` 또는 미승인이다. 승인된 여덟 Visual의 문서화된
Canonical Elements만 `LOCKED`이며 현재 Phase label은 Owner가
승인한 `PRE-PRODUCTION / G2 DESIGN BASELINE`이다.
