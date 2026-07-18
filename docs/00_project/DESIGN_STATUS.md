# Design Status Board

현재 단계: `PRE-PRODUCTION / PRODUCTION VISUAL LOCK`

Implementation Gate: `CLOSED`

| 영역 | 상태 | 다음 산출물 | Gate |
|---|---|---|---|
| 제품 비전 | HIGH-LEVEL BASELINE | 상세 설계 정렬 | G1 |
| 핵심 전투 동사 | HIGH-LEVEL BASELINE | 상세 게임플레이 설계 | G2 |
| 동료 릴레이 | VISUAL LOCKED / FUNCTIONAL DRAFT | 상세 게임플레이 설계 | G2 |
| 화물 스트랩 | VISUAL LOCKED / FUNCTIONAL DRAFT | 상세 게임플레이 설계 | G2 |
| CHR_00 Player Identity | LOCKED | 없음 | G2 |
| CHR_01 First Ally Identity | LOCKED | 없음 | G2 |
| 카메라 | VISUAL LOCKED / FUNCTIONAL DRAFT | 상세 카메라 설계 | G2 |
| Chair Redirect Core Loop | VISUAL LOCKED / FUNCTIONAL DRAFT | 상세 Chair Redirect 설계 | G2 |
| PGT_03 Chain Reaction Aftermath | VISUAL LOCKED / FUNCTIONAL DRAFT | 상세 Chain·파괴 결과 설계 | G2 |
| 파괴 예산 | HIGH-LEVEL BASELINE | 정확한 파편·Particle·VFX·성능 예산 | G2 |
| 적 1~3종 | DRAFT | 실루엣 시트 | G2 |
| 기술 아키텍처 | DRAFT | Prototype 상세 설계 | G3 |
| Graybox | PENDING | Design Readiness Gate 통과 후 | G4 |
| 본 제작 | PENDING | Implementation Gate 개방 후 | G5 |

## Production Visual Lock Sequence

1. `GEAR_01 — Cargo Strapping System` — `LOCKED`
2. `CHR_01 — First Ally Identity` — `LOCKED`
3. `PGT_01 — Gameplay Camera` — `LOCKED`
4. `PGT_02 — Chair Redirect Core Loop` — `LOCKED`
5. `PGT_03 — Chain Reaction Aftermath` — `LOCKED`
6. `LGT_01 — Night Archive Lighting` — `NEXT`
7. `FIN_01 — Conference Table Team Finisher`

즉시 다음 대상은 `LGT_01 — Night Archive Lighting`이다.

`CHR_00 Player Turnaround`은 향후 3D 모델링용 `PENDING` Production Support 산출물이며 현재 Production Visual Lock 순서를 차단하지 않는다.

상세 게임플레이 설계, 요구사항, 인수 테스트, 밸런스 수치, 정확한 파괴 예산, 카메라 기능, 기술 아키텍처, 구현 계획, Vertical Slice 세부 범위는 모두 `DRAFT`다. CHR_00 Player Identity, GEAR_01 Cargo Strapping System, CHR_01 First Ally Identity, PGT_01 Gameplay Camera, PGT_02 Chair Redirect Core Loop와 PGT_03 Chain Reaction Aftermath의 문서화된 Visual Canon만 `LOCKED`이며 그 외 Production Visual은 사용자 승인 전까지 `REVIEW` 또는 `PENDING`이다.
