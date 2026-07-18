# ADR-0012: PGT_03 Chain Reaction Aftermath Production Visual Lock v003

- Status: Accepted
- Date: 2026-07-18

## Decision

`references/approved/gameplay/PGT_03_Chain_Reaction_Aftermath_v003_FINAL.png`를 PGT_03 Chain Reaction Aftermath Production Visual Canon으로 승인한다.

## Canon

- PGT_02의 리디렉션 의자가 시작 원인의 증거로 남는다.
- 하나의 임시 목표가 비치명적으로 이동·균형 상실 후 회복 중이다.
- 국소적인 유리 파손 구역이 형성된다.
- 이동식 롤링 수납 자산과 복사기급 사무 자산이 연쇄적으로 이동 또는 전도된다.
- 파괴 결과로 새로운 시야선과 변경된 국소 room state가 읽힌다.
- 큰 물리 조각은 제한적이고 읽을 수 있으며 작은 유리·종이·먼지·토너 반응은 절제된다.
- GEAR_01 스트랩은 의자에 연결된 채 느슨하고 더 이상 의자를 제어하지 않는다.
- 파괴는 국소적이고 통제되며 실제 3인칭 gameplay view에서 읽힌다.

세부 Canonical Elements는 `docs/03_art/CHAIN_REACTION_AFTERMATH_VISUAL_SPEC.md`를 따른다.

## Visibility-Line Boundary

깨진 유리 구역은 새로운 visibility line과 변경된 국소 room state를 만든다. 완전히 통과 가능한 경로의 정확한 형태와 치수는 승인하지 않는다.

## Non-Canon

- 정확한 Chain 순서와 hop 수
- 적·물체·파편·Particle 수
- 피해, Stability, Knockback, Ragdoll, Momentum과 보상
- 정확한 방 구조, 소품 배치와 통로 치수
- 스트랩 해제 시점과 자동 해제 규칙
- 애니메이션, 물리값, VFX와 파손 임계값
- Rigidbody, Breakable, Joint, IK, Cinemachine과 Unity 구현
- 정확한 조명, 자산 모델, 배우 위치와 세부 포즈

## Consequences

- PGT_03의 문서화된 Visual Canon은 `LOCKED`이며 정확한 Chain·파괴 예산·기능 동작과 구현은 `DRAFT`다.
- 다음 Production Visual 대상은 `LGT_01 — Night Archive Lighting`이다.
- LGT_01과 FIN_01은 이 ADR에서 승인하지 않는다.
- PGT_03 이미지의 정확한 방·조명·목표·소품·파편·가시선 형상은 후속 Canon으로 자동 승격되지 않는다.
- Implementation Gate는 `CLOSED`로 유지한다.
