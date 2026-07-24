# Roguelite Progression

> **MIXED AUTHORITY — OWNER-APPROVED SELECTION LIFECYCLE /
> SUPERSEDED HISTORICAL CANDIDATE IDEAS / G5 CONTENT DRAFT**
>
> 현재 권한은
> [`POLICY_VIOLATION_SELECTION_RUN_APPLICATION_CONTRACT.md`](POLICY_VIOLATION_SELECTION_RUN_APPLICATION_CONTRACT.md)를
> 따른다. 아래 역사적 Candidate 이름과 효과는 승인된 콘텐츠가 아니다.

## Authority Split

### Owner-Approved G2 Selection Lifecycle

- Policy Violation은 `VERTICAL SLICE ONLY`다.
- run당 Selection Opportunity는 정확히 한 번이다.
- 기회는 eligible encounter 완료 뒤, 다른 combat encounter가 남아
  있을 때 다음 encounter 진입 전에 발생한다.
- G2 starting Offer는 정확히 두 개의 distinct eligible Candidate다.
- 플레이어는 정확히 하나만 선택한다.
- 선택한 Policy는 다음 encounter 진입부터 `ACTIVE`가 된다.
- run마다 Active Policy는 정확히 하나다.
- stacking, replacement, skip과 reroll은 없다.
- Active Policy는 같은 run의 room transition과 Reset에서 유지된다.
- run 종료에서 clear되고 새 run은 Policy 없이 시작한다.
- final Candidate pool과 exact Candidate content는 G5 Draft다.

### Remaining G5 Content Draft

- final Candidate names와 effects;
- final Candidate pool size;
- rarity, weighting과 randomization;
- exact room index와 final room count;
- presentation, UI, icon과 clip;
- balance와 production inclusion.

## Superseded Historical Candidate Ideas — Not Approved

아래 12개 행은 아이디어 보존 목적의
`SUPERSEDED / HISTORICAL DRAFT CANDIDATE`다. ID, 이름, 효과 또는
exact pool count는 active G2 authority나 G5 production scope가 아니다.

| Historical ID | Historical name | Historical effect idea | Authority |
|---|---|---|---|
| UPG-001 | Delegated Authority | 동료에게 받은 물체의 첫 충돌 impulse 증가 | SUPERSEDED / NOT APPROVED |
| UPG-002 | Flexible Working | 회피 도중 패스 캐치 가능 | SUPERSEDED / NOT APPROVED |
| UPG-003 | Team Building Exercise | 동료 Medium 적중 시 즉시 Light 추가 패스 | SUPERSEDED / NOT APPROVED |
| UPG-004 | Redirection Policy | 스트랩 리디렉션 허용 각도 확대 | SUPERSEDED / NOT APPROVED |
| UPG-005 | Asset Tagging | 연결한 물체의 Anchor 안정성 증가 | SUPERSEDED / NOT APPROVED |
| UPG-006 | Rolling Forecast | Rolling 물체 수평 속도 유지 | SUPERSEDED / NOT APPROVED |
| UPG-007 | Domino Policy | 가구 relay Momentum 증가 | SUPERSEDED / NOT APPROVED |
| UPG-008 | Shared Liability | 적-적 충돌 Stability 피해 증가 | SUPERSEDED / NOT APPROVED |
| UPG-009 | Return Mail | 정밀 Light가 플레이어 방향으로 되튐 | SUPERSEDED / NOT APPROVED |
| UPG-010 | Capital Expense | Heavy 구조 파괴 시 Momentum 일부 환급 | SUPERSEDED / NOT APPROVED |
| UPG-011 | Open Floor Plan | Structural 파괴 시 짧은 주변 충격 | SUPERSEDED / NOT APPROVED |
| UPG-012 | Paid Leave | 긴 Chain에서 소량 회복 | SUPERSEDED / NOT APPROVED |

일부 역사적 효과는 `Evade`, Structural Feature, Suppressed 또는 다른
비활성·조건부 기능에 의존하므로 현재 mandatory Candidate가 될 수 없다.

## Removed from Active Authority

다음 역사적 규칙은 active authority가 아니다.

- exact `12`-Candidate pool;
- exact `3`-choice Offer;
- 최소 두 category 보장;
- icon 또는 clip presentation 요구;
- `Evade`, Structural Feature, Suppressed 또는 다른 inactive system에
  의존하는 Candidate effect.

현재 Owner-approved Offer count는 `2`다. final pool count, actual
Candidate effect, selection UI와 presentation은 후속 Gate 전까지
`DRAFT`다.

## Product and Implementation Boundary

- permanent stat grinding과 cross-run benefit을 만들지 않는다.
- 새 logical input, player action, ally command, actor, resource 또는
  progression layer를 Candidate로 추가하지 않는다.
- 과거 표의 존재만으로 Candidate를 구현하지 않는다.
- save, analytics, event schema, data architecture, code와 Unity
  구현은 G3/G5 이후 별도 승인 전까지 Draft 또는 Deferred다.
