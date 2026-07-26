# G2 Policy Violation Selection and Run-Application Contract

> **OWNER-APPROVED G2 POLICY VIOLATION SELECTION AND RUN-APPLICATION CONTRACT**
>
> Decision Source: `TASK-0044 delegated Owner decision`
>
> 이 문서는 Policy Violation의 선택 기회, 후보 자격, 단일 선택과
> 현재 run 적용 수명주기를 논리 설계 수준에서 승인한다. 실제 후보
> 효과, 최종 후보 pool, UI, 저장, 기술 자료구조와 Unity 구현은
> 승인하지 않는다.

## Authority and Scope Boundary

- 이 계약은 `PR-F-018`과 `AT-ROGUE-001`에 필요한 관찰 가능한
  selection/run lifecycle을 정의한다.
- G1 Requirement wording, Applicability, Priority, Authority,
  Validation Reference 또는 Decision Source를 변경하지 않는다.
- 기존 Logical Input, Interaction State, Weight-Class, Momentum/Pip,
  Causal Chain, Ally Relay/Suppressed와 Prototype Content 계약을
  변경하지 않는다.
- 아래 term과 lifecycle state는 design description이며 runtime enum,
  class, component, field, event, serialized asset 또는 save schema를
  승인하지 않는다.
- 이 계약은 하나의 제한된 Owner-approved logical contract이며
  구현 또는 Gate 권한을 만들지 않는다.
- 현재 전역 Phase, Gate, Lock, Graybox, Relay와 다음 권한 경계는
  [`docs/00_project/DESIGN_STATUS.md`](../00_project/DESIGN_STATUS.md)를
  따른다.

## Applicability

- Policy Violation selection은 `PROTOTYPE CORE`에서 비활성이다.
- Policy Violation selection은 `VERTICAL SLICE`에서 활성이고
  `PR-F-018`의 필수 행동 결과다.
- 선택 결과는 현재 run에만 적용하며 permanent meta progression이
  아니다.
- Policy Violation은 Prototype acceptance condition이 아니다.
- 현재 G2 기준선은 run마다 정확히 하나의 Selection Opportunity만
  지원한다.
- 두 번째 기회를 추가하려면 별도 Owner G2 결정이 필요하다.

## Core Terms

| Term | Owner-approved logical meaning |
|---|---|
| `Policy Violation` | 이미 승인된 gameplay 행동 또는 자원 결정 하나를 바꾸는 임시 current-run rule exception |
| `Selection Opportunity` | 승인된 두 후보를 제공하는 run당 한 번의 encounter 사이 선택 지점 |
| `Candidate` | Offer에 들어갈 자격을 갖춘 완전히 정의된 observable rule change 하나 |
| `Offer` | 승인된 Selection Opportunity에서 제공되는 정확히 두 개의 서로 다른 eligible Candidate |
| `Selected Policy` | 플레이어가 Offer에서 고른 단 하나의 Candidate |
| `Active Policy` | 다음 combat encounter 진입부터 효력이 생긴 Selected Policy |

이 term은 runtime type, data record, UI object 또는 asset 이름을
승인하지 않는다.

## Logical Lifecycle States

현재 logical lifecycle은 다음 네 design state만 사용한다.

| State | Observable meaning |
|---|---|
| `NONE` | 현재 run에 선택된 Policy Violation이 없다. |
| `OFFERED` | 승인된 Selection Opportunity에서 정확히 두 개의 valid Candidate가 제공된다. |
| `SELECTED_PENDING` | Candidate 하나를 선택했지만 완료된 encounter에는 소급 적용하지 않는다. |
| `ACTIVE` | 선택한 Policy가 다음 combat encounter 진입부터 현재 run의 남은 기간에 적용된다. |

이 이름은 state machine, enum, save field 또는 구현 자료형을
승인하지 않는다.

## Selection Opportunity

- 현재 G2 기준선에는 run당 정확히 한 번의 Selection Opportunity가
  있다.
- 기회는 한 개의 eligible combat encounter가 완료된 뒤 발생한다.
- 기회는 뒤따르는 combat encounter에 진입하기 전에 발생한다.
- active run에 다른 combat encounter가 남아 있을 때만 발생한다.
- exact room index, room identity, encounter order와 final room count는
  G5 production-scope Draft 결정으로 남는다.
- Prototype Core에는 Selection Opportunity가 없다.

## Offer Construction

현재 G2 시작 profile의 Offer는 정확히 `2 candidates`다.

1. 두 Candidate 모두 Candidate Eligibility를 만족해야 한다.
2. 두 Candidate 모두 Offer 전에 observable rule change가 완전히
   정의되어야 한다.
3. 두 Candidate는 gameplay 결과와 결정에서 관찰 가능하게 달라야 한다.
4. 같은 Candidate가 두 slot을 차지할 수 없다.
5. 표현만 다르고 gameplay outcome이 동등한 두 Candidate는 valid
   Offer가 아니다.
6. placeholder, locked item 또는 future-feature advertisement를
   Offer에 넣지 않는다.
7. 승인된 Vertical Slice에서 비활성 또는 범위 밖인 기능에 의존하는
   Candidate를 Offer에 넣지 않는다.

Offer count `2`는 Owner-approved G2 starting value다. 변경에는 근거와
동기화된 G2 revision이 필요하다. 최종 Candidate pool 크기는 G5
production-scope 결정으로 남는다.

## Candidate Eligibility

Candidate는 아래 조건을 모두 만족할 때만 eligible하다.

### Observable Rule Change

Candidate definition은 최소한 다음을 명시해야 한다.

- 승인된 baseline behavior;
- changed behavior;
- activation trigger;
- affected scope;
- current-run duration;
- exclusions;
- observable validation case.

정확한 before/after 차이가 없는 Candidate는 Offer할 수 없다.

### Existing-System Boundary

Candidate는 이미 승인된 active behavior 또는 resource relationship
하나만 바꿀 수 있다. 다음을 새로 만들 수 없다.

- logical input;
- player action;
- ally command;
- second active ally;
- actor;
- enemy archetype;
- Weight Class;
- object category;
- resource;
- Causal Result family;
- room;
- progression layer.

### Active-Scope Dependency

mandatory Vertical Slice validation에 쓰는 Candidate는 Slice에서도
유지되는 Prototype Core behavior 또는 같은 범위의 active
`VERTICAL SLICE / MUST` behavior에만 의존할 수 있다.

mandatory Candidate는 다음에 의존할 수 없다.

- `Suppressed`가 `SHOULD`인 동안의 Suppressed;
- PR-F-019가 `SHOULD`인 동안의 Behavior Tags;
- PR-F-024가 `SHOULD`인 동안의 Structural Feature;
- Incident Report;
- LGT_01;
- FIN_01;
- `Sprint`;
- `Evade`;
- `Vault`;
- `Shove`;
- CHR_02;
- 두 번째 일반 전투 ally;
- 다른 `OUT`, `COULD`, `P2`, `DEFERRED` 또는 inactive feature.

### Product-Identity Boundary

Candidate는 다음을 도입할 수 없다.

- telekinesis 또는 remote object control;
- necktie functionality;
- firearm-centered combat;
- free-follow multi-ally play;
- room-wide destruction;
- permanent stat grinding.

### Behavior-Change Boundary

- Candidate는 gameplay decision 또는 rule을 바꿔야 한다.
- purely cosmetic Candidate는 invalid하다.
- observable rule change 없이 damage, health, defense 또는 percentage
  stat만 올리는 Candidate는 현재 승인 기준선에서 invalid하다.

## Candidate Definition Record

향후 G5에서 승인할 Candidate는 최소한 다음을 문서화해야 한다.

- content-reference ID;
- display name;
- target approved behavior;
- baseline rule;
- modified rule;
- activation trigger;
- current-run duration;
- incompatible features;
- validation intent;
- exact tuning의 later owning Gate.

이는 design-content record다. C# class, ScriptableObject, JSON schema,
database table 또는 Unity asset을 승인하지 않는다.

## Candidate Distinction

두 Candidate는 하나를 선택했을 때 gameplay decision 또는 behavior가
실질적으로 달라질 때만 distinct하다.

valid distinction은 다음에서 올 수 있다.

- 서로 다른 approved system을 변경;
- 서로 다른 trigger를 변경;
- 서로 다른 action availability를 변경;
- 서로 다른 resource decision을 변경;
- 서로 다른 causal 또는 relay responsibility를 변경.

다음만으로는 distinction이 충분하지 않다.

- 다른 이름;
- 다른 icon;
- 다른 color;
- 같은 효과의 미세한 수치 변형;
- outcome이 같은 corporate flavor text.

## Single Selection

승인된 Selection Opportunity에서는 다음을 모두 지킨다.

- 정확히 한 Candidate를 선택한다.
- 두 번째 Candidate를 선택할 수 없다.
- 두 Candidate를 결합할 수 없다.
- 둘을 모두 보유할 수 없다.
- 선택하지 않은 Candidate는 해당 기회에서 폐기한다.

현재 G2 기준선은 다음을 지원하지 않는다.

- skip;
- reroll;
- refresh;
- candidate replacement;
- candidate locking;
- later selection을 위한 보관;
- confirmation 뒤 취소;
- multiple-policy selection.

exact confirmation input, menu navigation과 presentation은 `DRAFT`다.

## Application Timing

- 선택은 eligible encounter가 완료된 뒤 발생한다.
- 선택한 Policy는 완료된 encounter를 바꾸지 않는다.
- 이미 정산된 Momentum 또는 Causal Result를 소급 변경하지 않는다.
- 이미 완료된 room state를 소급 변경하지 않는다.
- confirmation 뒤 `SELECTED_PENDING`이 된다.
- 다음 combat encounter 진입에서 `ACTIVE`가 된다.

Selection event 자체는 다음을 만들지 않는다.

- Momentum;
- Pip;
- Causal Origin;
- Meaningful Result;
- object restoration;
- room change;
- encounter Reset.

## Active-Policy Ownership

현재 기준선은 run마다 정확히 `one Active Policy Violation`만 허용한다.

- stacking 없음;
- replacement 없음;
- upgrade 없음;
- duplicate application 없음;
- duration refresh 없음;
- second selection event 없음;
- cross-run carryover 없음.

multiple selection 또는 stacking을 지원하려면 별도 Owner G2 결정이
필요하다.

## Room-Transition Persistence

`ACTIVE`가 된 Policy는 다음 규칙을 따른다.

- 같은 run의 후속 room transition을 지나 유지한다.
- 같은 run의 이후 모든 eligible encounter에 적용한다.
- room이 종료되었다는 이유만으로 clear하지 않는다.
- room 사이에서 reroll하지 않는다.
- 스스로 room content count 또는 actor count를 변경하지 않는다.

후속 room의 exact 수와 identity는 G5 Draft다.

## ResetEncounter Boundary

`ResetEncounter`는 현재 encounter-entry baseline을 복원한다.

Policy가 encounter 진입 때 이미 `ACTIVE`였다면 Reset은 다음을
모두 보존한다.

- 같은 Selected Policy;
- `ACTIVE` 상태;
- 그 encounter의 entry baseline과 동일한 Policy 조건.

Reset은 다음을 하지 않는다.

- Candidate 재제시;
- Candidate reroll;
- Policy clear;
- effect duplication;
- selection-menu undo.

selection은 encounter 사이에서 발생한다. snapshot technology와 restore
order는 G3 Draft다.

## Run End and New Run

run 종료 시 다음을 모두 수행한다.

- Selected Policy를 clear한다.
- 그 effect를 더 이상 적용하지 않는다.
- permanent account-wide benefit을 만들지 않는다.
- 이 계약으로 meta currency를 지급하지 않는다.

새 run은 lifecycle state `NONE`으로 시작한다. cross-run save, resume와
persistence는 별도 승인 전까지 `DEFERRED`다.

## Failure and Safe Rejection

다음 상황에서는 selection을 resolve할 수 없다.

- eligible하고 distinct한 Candidate가 두 개보다 적음;
- Candidate 하나의 behavior definition이 불완전함;
- 두 Candidate effect가 동등함;
- Candidate가 inactive 또는 unauthorized feature에 의존함;
- selection confirmation이 모호함;
- 둘 이상의 Candidate가 선택됨.

invalid resolution에서는 다음을 모두 지킨다.

- Candidate를 `ACTIVE`로 만들지 않는다.
- default Candidate를 조용히 선택하지 않는다.
- random fallback을 사용하지 않는다.
- gameplay stat을 변경하지 않는다.
- run progression state를 손상시키지 않는다.

exact error presentation은 `DRAFT`다.

## Current Candidate-Content Boundary

이 계약은 selection과 current-run application lifecycle만 승인한다.
final Candidate pool 또는 production-ready effect를 승인하지 않는다.

다음 기존 항목은 개별 G5 review 전까지
`SUPERSEDED / HISTORICAL DRAFT CANDIDATES`다.

- Delegated Authority
- Flexible Working
- Team Building Exercise
- Redirection Policy
- Asset Tagging
- Rolling Forecast
- Domino Policy
- Shared Liability
- Return Mail
- Capital Expense
- Open Floor Plan
- Paid Leave

위 이름과 과거 효과는 active G2 또는 production authority가 아니다.
과거 exact pool size `12`는 승인되지 않았다. 과거 exact offer size
`3`은 이 계약의 G2 starting offer size `2`로 대체된다.

## Candidate-Content Promotion Rule

Policy Violation Candidate가 G5 production scope에 들어가려면 다음을
모두 충족해야 한다.

1. exact observable effect definition;
2. approved active system만 변경한다는 확인;
3. 모든 현재 G2 contract와의 conflict review;
4. Acceptance-Test mapping;
5. solo-development feasibility review;
6. explicit G5 scope approval.

과거 Draft 표에 있다는 이유만으로 Candidate를 구현하지 않는다.

## Feedback Boundary

향후 presentation은 플레이어가 다음을 구분할 수 있게 해야 한다.

- 두 Candidate;
- 현재 focus된 Candidate;
- 선택한 Candidate;
- Policy가 active가 되는 시점;
- 선택한 Policy만 적용된다는 사실;
- current-run only라는 사실.

이 계약은 menu/card layout, icon, clip, typography, color, input binding,
animation, sound, VFX, confirmation timing 또는 localization을 승인하지
않는다. color만을 유일한 구분 수단으로 사용할 수 없다.

## Technical and Production Deferrals

다음은 계속 `DRAFT` 또는 `DEFERRED`다.

- final Candidate names와 effects;
- final pool size;
- rarity, random seed와 weighting;
- offer-generation algorithm과 duplicate-filter implementation;
- UI, presentation과 accessibility details;
- save/resume와 persistence data;
- analytics와 telemetry;
- event schema와 runtime state;
- ScriptableObject 또는 다른 data architecture;
- Unity implementation;
- exact room index와 final Vertical Slice room count;
- final balance.

이 계약은 code, scene, prefab, package, build, test implementation,
Graybox 또는 Implementation Gate 개방 권한을 만들지 않는다.
