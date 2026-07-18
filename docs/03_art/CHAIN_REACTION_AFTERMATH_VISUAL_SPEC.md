# Chain Reaction Aftermath Visual Specification

## 승인 이미지

![PGT_03](../../references/approved/gameplay/PGT_03_Chain_Reaction_Aftermath_v003_FINAL.png)

`PGT_03_Chain_Reaction_Aftermath_v003_FINAL`을 PGT_03 Chain Reaction Aftermath Production Visual Canon으로 승인한다.

승인 범위는 이미지와 아래 Canonical Elements에 한정한다. 정확한 Chain 동작, 파괴 예산 수치, 물리, VFX 또는 기술 구현은 승인하지 않는다.

## Canonical Elements

- PGT_02의 리디렉션 의자가 시작 원인의 증거로 남는다.
- 하나의 임시 목표가 비치명적으로 이동·균형 상실 후 회복 중이다.
- 국소적인 유리 파손 구역이 형성된다.
- 이동식 롤링 수납 자산과 복사기급 사무 자산이 연쇄적으로 이동 또는 전도된다.
- 파괴 결과로 새로운 시야선과 변경된 국소 room state가 읽힌다.
- 큰 물리 조각은 제한적이고 읽을 수 있다.
- 작은 유리·종이·먼지·토너 반응은 절제된다.
- GEAR_01 스트랩은 의자에 연결된 채 느슨하며 더 이상 의자를 제어하는 것으로 보이지 않는다.
- 파괴는 국소적이고 통제되며 실제 3인칭 gameplay view에서 읽힌다.

## Continuity Constraints

- CHR_00, GEAR_01, CHR_01, PGT_01과 PGT_02는 각 Visual Spec과 ADR에 기록된 Canonical Elements만 따른다.
- 리디렉션 의자는 PGT_02에서 문서화된 시작 원인의 증거로 남는다.
- 목표 반응은 비치명적 이동·균형 상실과 회복 범위에 머문다.
- GEAR_01 연결은 유지되지만 스트랩은 중력에 따라 느슨하게 처지고 의자를 계속 제어하지 않는다.
- 암적색 넥타이는 복장 전용이며 장비나 물체와 연결되지 않는다.
- 파괴와 재질 반응은 플레이 공간과 인과를 가리지 않는 국소 범위로 제한한다.
- HUD, Chain UI, 화염 폭발, 염력, 부유 물체, 초자연 효과, 군중과 room-wide destruction을 사용하지 않는다.

## Visibility-Line Boundary

깨진 유리 구역으로 새로운 visibility line과 변경된 국소 room state가 읽히는 것은 Canon이다.

완전히 통과 가능한 경로의 존재, 정확한 형태, 폭, 높이, 충돌 표면과 이동 규칙은 승인하지 않는다.

## Non-Canonical Elements

- 정확한 Chain 순서와 hop 수
- 정확한 적·물체·큰 파편·작은 파편·Particle 수
- 피해, Stability, Knockback, Ragdoll, Momentum과 보상
- 정확한 방 구조, 소품 모델·배치와 통로 치수
- 목표, 의자, 롤링 수납 자산과 복사기의 정확한 위치·외형·자세
- 유리 파손 형태와 정확한 가시선·경로 형상
- 스트랩 해제 시점과 자동 해제 규칙
- 정확한 애니메이션, 물리값, VFX와 파손 임계값
- Rigidbody, Breakable, Joint, IK, Cinemachine과 Unity 구현
- 정확한 조명, 노출, 재질 마모, 파편 배치와 렌더링 아티팩트

## LGT_01 and FIN_01 Boundary

PGT_03는 Night Archive Lighting, emergency-light Canon 또는 Conference Table Team Finisher를 승인하지 않는다. 다음 Production Visual 대상은 `LGT_01 — Night Archive Lighting`이며 `FIN_01`도 계속 미승인이다.

## Implementation Boundary

이 Visual Lock은 상세 Chain·파괴 기능, destruction budget 수치, VFX 설계 또는 Unity 구현을 승인하지 않는다. Implementation Gate는 `CLOSED`다.
