# Risk Register

| ID | 위험 | 가능성 | 영향 | 대응 | Trigger |
|---|---|---:|---:|---|---|
| R-001 | 물리 불안정·터널링 | 높음 | 높음 | 단순 Collider, 선택 CCD, 속도 상한, 테스트 | 물체가 1% 이상 관통 |
| R-002 | 던지기 반복 노동 | 중 | 매우 높음 | Catch, Redirect, 적 방해, 물체 태그 | 5분 내 행동 다양성 부족 |
| R-003 | 동료 AI 비용 | 높음 | 높음 | 활성 1명, Relay Position, 제한 상태 | 경로·끼임 버그 반복 |
| R-004 | 파괴 성능 | 높음 | 높음 | 사전 파손, 큰 조각 예산, VFX 분리 | 60 FPS 하락 |
| R-005 | 아트 목표 과대 | 높음 | 높음 | 디자인 Canon/렌더 Canon 분리, 모듈 키트 | 캐릭터 1종 제작 지연 |
| R-006 | Codex 구조 드리프트 | 높음 | 높음 | AGENTS, 티켓, ADR, 리뷰 | 중복 시스템·전역 상태 |
| R-007 | 설계 과잉 | 중 | 높음 | Gate와 Prototype 최소 범위 | 구현 전 문서 무한 확장 |
| R-008 | 카메라 멀미·가림 | 중 | 높음 | 높은 OTS, 옵션, 무게별 거리 | 목표 상실 10% 초과 |
| R-009 | Chain 보상 snowball | 중 | 중 | Pip cap, Heavy 비용, 보상 곡선 | 숙련자가 방 즉시 삭제 |
| R-010 | 초보가 Heavy 미사용 | 중 | 높음 | 직접 행동도 게이지 제공 | 첫 5분 Heavy 경험 실패 |
| R-011 | 이미지가 요구사항 오염 | 높음 | 중 | Visual Index, Canon/Non-canon | 임의 총기·장비 구현 요청 |
| R-012 | Scope creep | 높음 | 매우 높음 | Won't list, CR process | 신규 핵심 시스템 요청 |

## Active Risk Dispositions

### R-007 — 설계 과잉

- Status: `ACTIVE / TRIGGERED`
- Evidence: `Design Freeze and Pre-G4 Full Design Lock were completed
  while Game remains a placeholder and G4 has not started; FGD-001 was
  registered through Owner-approved C3 Change Control without
  implementation`
- Immediate response: `G4 EVIDENCE-FIRST DOCUMENTATION BRAKE`
- Owner: `DESIGN AUTHORITY`
- Owning Gate: `PRE-G4 / G4`
- Closure condition: `Human Owner-authorized G4 evidence exists and the
  Human Owner explicitly lifts or revises the brake`
- Live-state authority:
  `docs/00_project/DESIGN_STATUS.md`

Current response evidence:

- FGD-001 is registered as the single Owner-approved full-game target
  through CR-0001 and ADR-0021;
- no additional speculative Story, Campaign, Map, Level or exact
  Vertical Slice document is permitted before G4-A evidence;
- the next growth step must be separately authorized Unity/toolchain/G4
  evidence, not a v006-style review-document expansion;
- R-007 remains `ACTIVE / TRIGGERED`; no closure is claimed.

이 disposition은 새 설계, 구현, Unity, Graybox, Gate passage 또는
Execution Authorization Relay를 승인하지 않는다.
