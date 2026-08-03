# G4-A Detailed Evidence Protocol

Status: `OWNER-APPROVED / CORRECTED / READY FOR FUTURE EXECUTION AUTHORIZATION`

Decision baseline: `a90fe71abc50eea9d06898672ca73c4b48bcff34`

Protocol decision: `G4-A DETAILED EVIDENCE PROTOCOL OWNER-APPROVED / EXECUTION NOT AUTHORIZED`

Evidence: `NOT COLLECTED`

Execution: `NOT STARTED / NOT AUTHORIZED`

## TASK-0070-R1 correction history

- TASK-0070 initial registration commit:
  `1be067ec154fea60646113214332ebd64fc66fd6`
- post-push exact conformance result:
  `REVISION REQUIRED / HISTORICAL`
- TASK-0070-R1: `EXACT CONTRACT CORRECTION APPLIED`
- evidence: `NOT COLLECTED`
- execution: `NOT STARTED / NOT AUTHORIZED`

이 문서는 FGD-001의 Owner-approved G4-A 방향과 이진 판정 규칙을
미래 Build Qualification, 참가자 세션, 관찰, 증거 기록과 Owner Gate
판정 입력 절차로 표준화한다. 프로토콜 승인은 Unity provisioning,
Build, 참가자 실행, 증거 수집, Graybox 시작, G4 passage,
Implementation Gate 개방 또는 Execution Authorization Relay 발급을
승인하지 않는다.

## 1. Authority and preserved boundaries

실질 권위 우선순위는 다음과 같다.

1. `docs/02_design/FGD_001_DESTROY_YOUR_COMPANY_ZERO_DEFECT_FULL_GAME_BASELINE.md`
2. `docs/00_project/CR-0001-FULL-GAME-DIRECTION-TRANSITION.md`
3. `docs/09_adrs/ADR-0021-FULL-GAME-DIRECTION-ZERO-DEFECT.md`
4. 승인된 G1/G2/G3 계약, Prototype Content Lock과 Acceptance Tests
5. 이 실행 프로토콜

이 문서는 FGD-001, CR-0001, ADR-0021, G1/G2/G3 passage, Prototype
content, 승인 Visual Canon, Pre-G4 Full Design Lock과 intended toolchain을
변경하지 않는다. 정확한 Unity Editor lock은 계속 `6000.3.20f1`이며
toolchain provisioning은 `OWNER-DEFERRED`다. G4는 `NOT PASSED / NOT
STARTED / NOT AUTHORIZED`, G5는 `NOT PASSED / REQUIRES G4 EVIDENCE`,
Graybox는 `NOT STARTED`, Implementation Gate는 `CLOSED`, active Relay는
`ABSENT`다.

## 2. Evidence roles

### 2.1 Design Authority

- 이 프로토콜과 향후 변경을 승인한다.
- 미래 G4-A Evidence Report를 검토한다.
- 증거 disposition을 근거로 별도의 G4-A Gate 결정을 내린다.
- 프로토콜이나 보고서가 스스로 Gate를 통과시킬 수 없다.

### 2.2 Facilitator / Observer

- 각 scored round에는 정확히 한 명의 primary Facilitator/Observer가
  있어야 한다. Human Owner 또는 위임된 tester가 맡을 수 있다.
- 정확한 표준 안내문을 읽고, 시도 시작·종료와 관찰 결과를 기록한다.
- mechanic, useful object, 정답 경로 또는 판정 기준을 설명하거나
  coaching하지 않는다.
- Facilitator/Observer는 scored participant가 아니다.

### 2.3 Technical Operator

- 별도의 technical operator는 승인된 미래 실행 범위 안에서 Build 실행,
  capture와 log 보조만 할 수 있다.
- 참가자에게 mechanic 또는 해결법을 설명하거나 coaching할 수 없다.

### 2.4 Scored Participants

- Binary Round마다 서로 다른 참가자 정확히 `3`명이다.
- 각 참가자는 해당 Round의 시도 `2`회를 독립적으로 수행한다.
- 프로젝트의 개발·설계 역할을 맡았거나 정확한 room/build에 노출된 사람,
  FGD-001 또는 이 프로토콜을 읽은 사람은 제외한다.
- 기본적인 3D third-person action 조작 경험은 허용한다.
- 한 Round의 참가자는 동일한 input scheme을 사용한다.
- Human Owner와 repository 기여자는 scored participant가 아니다.

## 3. Non-scoring Build Qualification

참가자 세션 전에 아래 항목을 모두 확인한다. 하나라도 실패하면 참가자
세션을 시작하지 않는다. Qualification은 Q1–Q5를 채점하지 않으며 그
자체로 Gate 증거가 아니다.

### 3.1 Locked content and scope check

- authored Core Room 정확히 `1`
- CHR_00 `1`, CHR_01 `1`, Compliance Runner `2`
- File Binder `3`, Wheeled Office Chair `2`, Rolling Drawer Cabinet `1`
- recognition-only Copier-Class Asset `1`
- fixed authored ally support position `1`
- Prototype destruction category와 event `0`
- HeavyAction, Perfect Catch, Momentum/Pip, Structural, Breakable,
  Suppressed, Policy Violation, LGT_01과 FIN_01을 포함한 excluded content 없음

### 3.2 Active action check

다음 core action `8`개가 활성이고 observable해야 한다.

1. `Move`
2. `Look`
3. `Aim`
4. `InteractPrimary`
5. `Throw`
6. `CallAlly`
7. `StrapRelease`
8. `ResetEncounter`

`HeavyAction`은 사용할 수 없어야 한다.

### 3.3 Lifecycle and stability check

- manual Reset이 authored encounter-entry baseline을 복원한다.
- 비살상 failure restart를 연속 `3`회 수행해 매번 baseline을 복원한다.
- Manual Reset 뒤와 연속 `3`회의 validated nonlethal failure restart 각각의
  뒤에, 승인된 baseline-diff instrumentation으로 복원된 runtime state를
  authored encounter-entry baseline과 비교한다. 참가자 세션을 허용하기
  전에 모든 unexpected difference count는 정확히 `0`이어야 한다.
- baseline-diff는 최소한 다음을 모두 포함한다.
  - actor presence와 authored start state
  - object presence와 authored start state
  - direct supply pool과 Ally supply pool
  - Drawer blockage와 side-lane state
  - Copier recognition-only state
  - Carry ownership
  - Relay와 reservation
  - Queued Throw
  - Cargo Strap
  - Throw와 Heavy commitment
  - Causal Origin과 Result state
  - room-state flags
  - event subscription 또는 handler count
- Reset 또는 failure restart 뒤 duplicate subscription이 없다.
- interaction, active object, Relay, Strap, queue와 causal ownership 누수가 없다.
- required event/log capture가 동작한다.
- 평가를 막는 crash가 없다.

이 baseline-diff 비교는 Build Qualification evidence이며 Q1–Q5 scoring이
아니다.

## 4. Round types and allowed sequence

### 4.1 Optional Exploratory Round

- Binary Round 1 전에만 선택적으로 한 번 수행할 수 있다.
- 정확히 `2`개의 non-scoring session으로 구성한다.
- 내부 참여자를 사용할 수 있으나 scored participant로 재사용할 수 없다.
- Q1–Q5, Gate result 또는 PASS evidence를 만들지 않는다.
- exploratory 결과에 따른 pre-binary parameter revision은 최대 `1`회다.
- 새 mechanic, content, enemy role, Story 또는 system을 추가할 수 없다.
- 다음 round는 반드시 Binary Round 1이며 두 번째 exploratory round는 없다.

### 4.2 Binary Round 1

- 서로 다른 scored participant `3`명
- 참가자당 시도 `2`회
- 총 scored attempt `6`회
- Round 전체에서 동일한 immutable Build와 content를 사용한다.

### 4.3 Binary Retest

- Binary Round 1 결과가 아래의 one-retune branch에 해당할 때만 수행한다.
- bounded Retune 정확히 `1`회 뒤, 새 참가자 `3`명이 시도 `2`회씩 수행한다.
- 이전 scored participant를 재사용하지 않는다.
- Retest는 최대 `1`회이며 Build와 content는 Retest 전체에서 immutable하다.

### 4.4 Q2-only Layout Recheck

- 전체 PASS rule을 충족했지만 `Q2 = NO`인 경우에만 수행한다.
- Core Room layout-only revision 정확히 `1`회 뒤 새 참가자 `3`명이
  시도 `2`회씩 수행한다.
- `Q2`만 다시 채점한다.
- Q1, Q3 또는 Q5의 명백한 regression이 관찰되면 recheck를 무효화하고
  Owner-approved Change Control로 회부한다.
- Binary Retest와 Q2-only Layout Recheck는 서로 배타적이다.
- Binary Retest가 사용된 뒤 PASS rule을 충족하더라도 `Q2 = NO`이면 추가
  Q2 revision은 없고 G4-B는 차단된다.

## 5. Session and attempt contract

각 scored participant는 정해진 순서의 Variant에서 시도 `2`회를 수행한다.
각 시도는 다음 중 가장 먼저 발생한 시점에 종료한다.

1. Drawer objective를 통해 side route를 열고 exit에 도달함
2. 비살상 failure가 발생함
3. 참가자가 `ResetEncounter`를 사용함
4. 시도 시작 뒤 `5:00` 경과

참가자당 scored play time은 최대 `10:00`이다. 첫 시도 후 authored
baseline을 완전히 복원하고 두 번째 Variant를 로드한다. 이때 안내자는
다음 문장만 말한다.

> The room has reset. Try again.

시도 사이에 debrief, mechanic 설명 또는 hint를 제공하지 않는다. 두 번째
시도 뒤에만 질문과 debrief를 수행하며 세 번째 시도는 없다.

## 6. Fixed Variant order

무작위 배정은 사용하지 않는다.

| Round | Participant | Attempt 1 | Attempt 2 |
|---|---|---|---|
| Binary Round 1 | P01 | Variant A | Variant B |
| Binary Round 1 | P02 | Variant B | Variant C |
| Binary Round 1 | P03 | Variant C | Variant A |
| Retest 또는 Q2-only Recheck | P04 | Variant B | Variant A |
| Retest 또는 Q2-only Recheck | P05 | Variant C | Variant B |
| Retest 또는 Q2-only Recheck | P06 | Variant A | Variant C |

## 7. Standardized introduction and control card

모든 scored participant에게 첫 시도 직전에 다음 문구를 그대로 읽는다.

> Open the marked side route and reach the exit. Use the controls on this
> card. You may reset at any time. I cannot explain the intended solution
> while you play.

control card는 binding 정보만 표시한다: Move, Look, Aim, Interact, Throw,
Call Ally, Strap Release, Reset. useful object, lane, weight, ally supply,
Catch, queued throw, Strap, Anchor, Drawer, causal chain 또는 scoring을
설명해서는 안 된다.

## 8. Coaching and intervention boundary

허용되는 개입은 다음뿐이다.

- objective 문구를 요청 시 한 번 그대로 반복
- hardware 또는 input malfunction 교정
- 안전을 위한 pause

다음 개입은 모두 명시적으로 금지한다.

- useful object의 이름을 말함
- `CallAlly` 사용을 제안함
- Throw를 제안함
- Strap을 제안함
- Anchor를 가리킴
- lane을 가리킴
- Catch를 설명함
- Queued Throw를 설명함
- Drawer 해결법을 설명함
- `warning about an approaching enemy is prohibited`
- causal result를 설명함
- `suggesting Reset is prohibited`
- mechanic을 시범 보임

위 금지 개입 중 하나라도 발생하면 해당 participant session은 무효이며
새 eligible participant로 대체한다.

## 9. Intentional-use and approach definitions

### 9.1 Intentional Use

prompt 없이 유효한 context에서 수행됐으며 objective 진행 또는 현재
pressure 대응이라는 인식 가능한 목적이 있는 행동이다.

### 9.2 Valid Relay

참가자가 자발적으로 `CallAlly`를 사용하고 CHR_01이 실제 물리 carrier를
release하며 teleport 또는 숨겨진 직접 전달이 발생하지 않은 경우다.

### 9.3 Valid Throw

Direct 또는 Queued release가 실제 trajectory를 만들고 참가자의 인식
가능한 목적을 가진 경우다.

### 9.4 Valid Strap

유효한 load-bearing Anchor에 연결된 floor-supported Medium을 플레이어가
redirect해 observable object 또는 room-state change를 만든 경우다.

### 9.5 Distinct Physical Approach

다음 중 하나를 만족하며 materially different causal sequence를 만든다.

- primary interaction family가 다름
- lane과 initiating object의 조합이 다름

같은 object의 angle·timing만 바꾼 반복, Reset 뒤 동일 해결의 반복은
distinct approach가 아니다.

## 10. Q1–Q5 scoring

### Q1

> Under pressure, does the player voluntarily use the core Relay, Throw, and Strap actions?

참가자 `YES`: 두 시도를 합쳐 coaching 없이 Valid Relay, Valid Throw 또는
Valid Strap을 하나 이상 자발적으로 사용했다.

Round `YES`: 참가자 `3`명 중 `2`명 이상이 `YES`다.

### Q2

> Can the same Drawer objective be solved through at least two valid physical approaches?

Round `YES`: 참가자 `2`명 이상이 성공한 Drawer route solution을 만들고,
그 성공들 가운데 위 정의에 따른 distinct physical approach가 `2`개
이상 존재한다.

### Q3

> Is Player–Ally–Object causality readable without explanation?

두 번째 시도 뒤 참가자에게 다음 세 질문을 순서대로 묻는다.

1. `What action changed or opened the route?`
2. `What did your ally do, and what did you do?`
3. `What physical sequence caused the final result?`

참가자 `YES`: initiating action, 관련 ally 또는 carrier, resulting object
또는 room-state를 식별하고 결과를 telekinesis나 숨겨진 script가 아닌
물리적 sequence로 설명한다.

Round `YES`: 참가자 `3`명 중 `2`명 이상이 `YES`다.

### Q4

> After failure or Reset, do the layout variants create a reason to try another approach?

참가자 `YES`: 두 번째 시도 시작 `60`초 안의 첫 committed plan이 첫
시도와 비교해 lane, initiating object 또는 primary interaction family를
materially 바꾸며, 그 변경이 prompt 없이 발생한다.

Round `YES`: 참가자 `3`명 중 `2`명 이상이 `YES`다.

### Q5

> Can the camera simultaneously keep the player, active object, enemy pressure, and exit objective readable?

camera critical event는 다음 중 하나다.

1. active decision 중 player, active object, immediate pressure 또는 route
   objective 중 하나가 연속 `2`초를 초과해 완전히 보이지 않음
2. essential information을 camera가 가려 참가자가 멈추거나, blind action을
   하거나, plan을 포기함
3. collision, snap 또는 rotation으로 안전 pause, 불편 호소 또는 플레이
   중단이 발생함
4. Reset 또는 Redirect 뒤 정상 control 상태에서 `3`초 안에 active object
   또는 route를 다시 찾지 못함

두 번째 시도 뒤 정확히 다음 질문을 묻는다.

> Did the camera ever stop you from seeing something you needed to make a decision?

참가자 `YES`: 두 시도에 camera critical event가 없고 위 질문에 camera가
결정을 막지 않았다고 답한다.

Round `YES`: 참가자 `3`명 중 `2`명 이상이 `YES`이고, discomfort 또는
safety 이유로 끝난 session이 없다.

## 11. Binary decision rule

### Evidence supports a future PASS decision

```text
4–5 YES
AND Q1 = YES
AND Q3 = YES
AND Q5 = YES
```

기록 disposition은 `EVIDENCE SUPPORTS G4-A PASS DECISION`이다. 이는
G4-A PASS 자체가 아니며 Design Authority의 별도 Owner Gate 결정이
필요하다.

### Q2-only branch

PASS rule을 충족했지만 `Q2 = NO`이면
`Q2-ONLY RECHECK REQUIRED`다. Section 4.4의 단 한 번의
layout-only recheck 외에 scope를 확장하지 않는다.

### One bounded Retune and Binary Retest

total YES가 `2–3`이거나 mandatory question인 Q1, Q3, Q5 중 하나라도
`NO`이면 `ONE RETUNE / BINARY RETEST REQUIRED`다. Section
4.3의 한 번의 Retest만 허용한다.

### Core redesign

total YES가 `0–1`이거나 허용된 Binary Retest도 PASS rule을 충족하지
못하면 `CORE LOOP REDESIGN REQUIRED`다. G4-B와 Story/Campaign/Map/Level
production integration은 차단된다.

## 12. Allowed Retune surface

한 번의 bounded Retune 또는 Q2-only revision은 다음 Draft surface에만
한정한다.

- existing actor starting positions
- existing object starting positions
- existing Variant lane spacing
- Draft camera distance
- Draft camera damping
- Draft camera collision
- Draft camera occlusion
- Draft Runner pressure timing
- Draft Runner pressure spacing
- Draft Runner movement
- Draft Throw response
- Draft Catch response
- Draft Strap response
- objective-marker visibility
- control-label readability
- collision tolerance required for approved actions
- interaction tolerance required for approved actions

새 mechanic, content type, enemy role, Story scene, system, asset family,
destruction 또는 HeavyAction을 추가할 수 없다. Retest 또는 Recheck가
시작된 뒤 해당 Round의 Build와 content는 immutable하다.

## 13. Session evidence artifacts

각 scored session은 다음 `16`개 필드를 가져야 한다.

1. anonymous participant ID
2. eligibility result와 broad experience bracket
3. immutable Build ID와 repository SHA
4. 실제 사용 toolchain 식별자
5. Variant order
6. attempt start/end timestamp
7. attempt end reason
8. intentional-use action과 timestamp
9. Relay, Throw, Strap validity record
10. failure와 Reset record
11. approach signature
12. Q3 causal answer 원문 또는 정확한 전사
13. camera critical event record
14. participant Q1, Q3, Q4, Q5 result
15. facilitator/technical intervention record
16. invalid flag와 reason

`Each scored session must include at least one of exactly:`

- `a full game-window screen recording`
- `an independent second-observer record`

generic screen recording, observer attestation 또는 observer note라는 약한
대체 문구는 사용할 수 없다. 얼굴 recording은 요구하지 않는다.
personally identifying information은 anonymous ID와 broad experience
bracket 외에는 수집하지 않는다.

## 14. Round evidence package

각 Binary Round package는 다음을 모두 포함한다.

1. immutable build hash
2. repository source SHA
3. protocol version
4. participant eligibility table
5. exactly six attempt records
6. Variant assignment table
7. event and Reset logs
8. approach-signature table
9. Q1–Q5 aggregation table
10. Critical Camera Event table
11. invalid-session replacement records
12. production-speed ledger
13. scope-conformance checklist
14. exactly one recommended Evidence Disposition

필수 항목이 빠지면 Round는 무효다.

## 15. Production-speed evidence

G4-A package는 다음 실제값을 별도 ledger에 기록한다.

1. active implementation hours
2. rework hours
3. debugging hours
4. tool or asset wait time
5. external dependency time
6. calendar elapsed time
7. actual weekly development hours available
8. reusable systems or assets created
9. discarded work
10. build and evidence-capture time

한 개 sample을 Levels 0–6 전체 일정으로 직접 배수 추정하지 않는다.
이 ledger는 G5/C3 계획 입력일 뿐 G4-A gameplay score가 아니다.

## 16. Evidence invalidation

### 16.1 Invalid session

다음 중 하나면 session을 무효화하고 새 참가자로 대체한다.

- participant가 scored room 또는 build에 prior exposure가 있었음
- Facilitator가 mechanic을 coaching함
- Round 중 build가 변경됨
- 잘못된 Variant order를 사용함
- required evidence가 누락됨
- excluded content가 활성화됨
- locked actor count가 변경됨
- locked object count가 변경됨
- unauthorized mechanic 또는 system이 활성화됨
- blocking crash가 정상 완료를 막음
- 참가자 사이 input scheme이 변경됨

기존의 eligibility 실패 또는 participant 재사용, attempt time cap 위반,
attempt 사이 baseline restore 실패도 session을 무효화한다. 무효 session은
새 eligible participant로 대체한다.

### 16.2 Invalid round

다음 중 하나면 Round 전체를 무효화하고 Gate 결정을 내리지 않는다.

- immutable build가 변경됨
- scored session을 둘 이상 대체할 수 없음
- scope가 승인된 FGD-001 G4-A content와 다름
- required log 또는 recording을 재구성할 수 없음
- source evidence 없이 Q1–Q5 result를 편집함

기존의 Build Qualification 실패, participant 또는 attempt 수 오류,
unresolved critical camera safety event도 Round를 무효화한다.

Round disposition은 `EVIDENCE ROUND INVALID`다.

## 17. Future Evidence Report contract

미래 실행이 별도 승인된 뒤 다음 경로를 사용한다.

`docs/05_production/G4A_EVIDENCE_REPORT.md`

이 작업에서는 해당 파일을 만들지 않는다. 미래 보고서는 정확히 다음
section을 포함해야 한다.

1. Authority and Execution Baseline
2. Toolchain and Build Identity
3. Build Qualification Result
4. Exploratory Round Disposition
5. Participant Eligibility
6. Session and Variant Inventory
7. Scope-Conformance Review
8. Q1 Evidence
9. Q2 Evidence
10. Q3 Evidence
11. Q4 Evidence
12. Q5 Evidence
13. Failure and Reset Evidence
14. Camera Critical Events
15. Invalid or Replaced Sessions
16. Production-Speed Evidence
17. Binary Decision Calculation
18. Retune or Q2-Only Branch
19. Blocking Issues
20. Non-Blocking Notes
21. Evidence Disposition
22. Owner Gate Decision Required

허용 disposition은 다음 다섯 개뿐이다.

1. `EVIDENCE SUPPORTS G4-A PASS DECISION`
2. `Q2-ONLY RECHECK REQUIRED`
3. `ONE RETUNE / BINARY RETEST REQUIRED`
4. `CORE LOOP REDESIGN REQUIRED`
5. `EVIDENCE ROUND INVALID`

Evidence Report는 스스로 G4-A를 PASS하거나 G4-B, G5, Graybox,
Implementation Gate 또는 Relay를 승인할 수 없다.

## 18. Protocol Change Control

다음 값을 바꾸려면 evidence collection 전에 Human Owner-approved Change
Control이 필요하다.

- scored participant count
- participant eligibility
- attempt count
- attempt time cap
- Variant order
- exact standardized introduction script
- Coaching and intervention boundary
- Q1–Q5 aggregation thresholds
- Mandatory Questions
- PASS rule
- Retune count
- Q2-only revision count
- evidence-artifact requirements

`C0`은 spelling, formatting 또는 valid-link correction처럼 권위를 바꾸지
않는 교정만 허용한다. 위 값을 조용히 편집하거나 실행 중 재해석할 수 없다.

## 19. Next authorization boundary

`OWNER EXACT UNITY EDITOR VERSION AND TOOLCHAIN PROVISIONING DECISION REQUIRED`

활성화된 numbered implementation task는 없다. 이 경계 전에는 Unity,
package, Build, participant session, evidence collection, Graybox 또는
implementation을 실행하지 않는다.
