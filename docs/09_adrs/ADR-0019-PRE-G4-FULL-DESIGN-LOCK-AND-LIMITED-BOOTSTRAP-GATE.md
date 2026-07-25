# ADR-0019 — Pre-G4 Full Design Lock and Limited Bootstrap Gate

- Status: Accepted
- Date: 2026-07-25
- Decision Source: `TASK-0061 Owner-delegated correction after Final Pre-Implementation Review`
- Scope: authority and sequencing semantics only

## Context

`docs/05_production/FINAL_PRE_IMPLEMENTATION_REVIEW.md`는 Design Freeze
Checklist `32 / 32`의 증거를 확인했지만 다음 두 권위 결함 때문에
`REVISION REQUIRED`를 기록했다.

1. IMP-0001이 실행 SHA를 티켓 안에 삽입하도록 요구하면서 실행 시
   HEAD가 그 SHA와 같아야 한다고 요구해 자기참조식 기준선이 됐다.
2. G4 이전 문서 동결, G4 Graybox, G5 Production Freeze와 제한된
   IMP-0001 Bootstrap Gate의 관계가 정의되지 않았다.

이 ADR은 두 결함의 의미와 순서만 교정한다. Full Design Lock을
최종화하거나 Gate를 열거나 실행 SHA를 선정하지 않는다.

## Decision

### Three Separate Freeze Concepts

1. **Design Freeze Checklist `32 / 32`**
   - 필수 문서 증거가 모두 존재한다는 documentary evidence
     prerequisite다.
   - 그 자체로 Full Design Lock, G4, G5, G6 또는 구현을 승인하지
     않는다.
2. **PRE-G4 FULL DESIGN LOCK**
   - G1/G2/G3 passage, Design Freeze `32 / 32`, 승인 Visual
     Canon·Production Support, 파괴·Office Core Kit Lock, 승인된
     IMP-0001 decomposition과 성공한 Final Re-Audit을 하나의
     Owner-approved frozen documentation baseline으로 고정하는
     pre-Graybox lock이다.
   - empty repository Bootstrap과 이후 별도 승인되는 G4의 안정적
     문서 기반일 뿐 gameplay, 재미, 성능, G4, G5 또는 G6 passage를
     주장하지 않는다.
   - 성공한 Re-Audit 뒤 별도 Owner task가 최종화해야 한다.
3. **G5 PRODUCTION FREEZE**
   - G4 Graybox evidence를 반영해 실제 제작 범위, asset list, 일정과
     risk를 승인하는 post-G4 Gate다.
   - G4보다 먼저 통과할 수 없으며 Pre-G4 Full Design Lock과 동일하지
     않다.

현재 Pre-G4 Full Design Lock semantics는
`OWNER-APPROVED / DEFINED`지만 Lock 자체는
`INCOMPLETE / RE-AUDIT REQUIRED`다.

### Required Sequence

정확한 순서는 다음과 같다.

1. Pre-G4 documentary baseline과 Full Design Lock
2. narrowly authorized infrastructure preparation
3. separately authorized G4 Graybox
4. G4 evidence와 Gate decision
5. G5 Production Freeze
6. later Vertical Slice production

empty Bootstrap은 G4를 시작하지 않는다. 제한 Gate는 G4 또는 G5를
통과시키지 않는다.

### Limited Implementation Gate States

- `CLOSED`: 어떤 implementation ticket도 실행할 수 없다.
- `OPEN — IMP-0001 ONLY / EMPTY BOOTSTRAP`: 정확히 IMP-0001의 빈 Unity
  repository Bootstrap 한 번만 허용하는 ticket-, baseline-, scope-
  specific one-shot state다.

제한 Gate가 열리더라도 다음은 금지된다.

- gameplay, C#, Test, Scene, Prefab, Graybox 또는 production asset;
- 다른 implementation ticket;
- G4 또는 G5의 시작·통과;
- 다른 task로의 상속, 추론 또는 범위 확장.

TASK-0061은 이 상태를 정의만 하며 현재 Gate는 `CLOSED`다.

### One-Shot Expiration

`OPEN — IMP-0001 ONLY / EMPTY BOOTSTRAP` 권한은 다음 중 하나가
발생하면 자동으로 무효가 된다.

- 승인된 IMP-0001 완료;
- IMP-0001 stop condition 발생;
- local HEAD와 `origin/main`의 승인 기준선 drift;
- 작업 전 tracked 또는 staged 변경;
- 다른 ticket 또는 다른 scope 시도;
- 승인되지 않은 Package 필요 또는 발견;
- 허용 범위 확대;
- Owner 또는 상위 권위의 철회.

만료·소비·철회된 권한은 재사용하거나 다른 작업으로 상속할 수 없다.
새 실행에는 새 Execution Authorization Relay가 필요하다.

### Execution Authorization Relay

IMP-0001의 실행 기준선은 티켓에 저장하지 않는다. 별도의 bounded
Owner-approved `Execution Authorization Relay`가 다음을 모두 제공해야
한다.

- 정확한 ticket ID `IMP-0001`;
- 이미 `origin/main`에 push된 완전한 40자 immutable commit SHA;
- branch `main`;
- local HEAD와 `origin/main`의 SHA 일치 및 clean tracked/staged 상태;
- Gate 상태 `OPEN — IMP-0001 ONLY / EMPTY BOOTSTRAP`;
- 정확한 allowed/forbidden files;
- 정확한 operations와 validations;
- commit/push 권한;
- one-shot 및 expiration 조건.

Relay는 다음이 모두 끝난 뒤에만 별도 task가 발급할 수 있다.

1. TASK-0061 교정 commit이 push됨;
2. Final Pre-Implementation Re-Audit 결과가
   `READY FOR LIMITED IMPLEMENTATION GATE DECISION — IMP-0001 ONLY`;
3. 별도 task가 Pre-G4 Full Design Lock을 최종화함;
4. 그 task가 별도의 실행 Relay 발급을 명시적으로 승인함.

### Valid and Invalid Baseline Binding

유효한 실행 기준선은 이미 `origin/main`에 존재하는 완전한 40자
commit SHA이며, 실행 직전 local HEAD와 `origin/main`이 모두 그 SHA와
같고 ahead/behind가 `0 / 0`, tracked/staged 상태가 clean이어야 한다.
기준선 선택 뒤 티켓을 수정하지 않는다.

다음은 무효다.

- `current main`, `latest main` 또는 다른 가변 표현;
- short SHA;
- 과거 task에서 상속한 SHA;
- 새 권위 commit 전에 티켓이나 문서에 미리 삽입한 SHA;
- 권위 commit이 push되기 전에 공급된 SHA.

## Current Boundary

- Phase: `PRE-PRODUCTION / DESIGN FREEZE COMPLETION`
- G1/G2/G3: `PASSED`
- Design Freeze: `32 / 32`
- Historical Final Pre-Implementation Review: `REVISION REQUIRED`
- Final Review Remediation:
  `TASK-0061 CORRECTIONS APPLIED / RE-AUDIT REQUIRED`
- Pre-G4 Full Design Lock Semantics: `OWNER-APPROVED / DEFINED`
- Pre-G4 Full Design Lock: `INCOMPLETE / RE-AUDIT REQUIRED`
- Limited Implementation Gate Semantics: `OWNER-APPROVED / DEFINED`
- Limited Implementation Gate Current State: `CLOSED`
- G4: `NOT PASSED / NOT STARTED / NOT AUTHORIZED`
- G5: `NOT PASSED / REQUIRES G4 EVIDENCE`
- Graybox: `NOT STARTED`
- IMP-0001:
  `EXECUTION BLOCKED / CORRECTION APPLIED / RE-AUDIT REQUIRED`
- Concrete IMP-0001 execution SHA:
  `ABSENT / SUPPLIED ONLY BY FUTURE EXECUTION RELAY`

## Consequences

- IMP-0001의 baseline binding은 non-self-referential하게 정의된다.
- Pre-G4 Full Design Lock과 G5 Production Freeze가 분리된다.
- 빈 Bootstrap과 G4 Graybox가 별도 승인 단계로 분리된다.
- Historical Final Review의 `REVISION REQUIRED` 증거는 변경하지 않는다.
- 다음 단일 작업은
  `TASK-0062 — Re-Audit Final Pre-Implementation Readiness after Gate and Baseline Corrections`다.

이 ADR은 Full Design Lock passage, Gate opening, Execution
Authorization Relay, concrete SHA, Unity, Package materialization, code,
test, Scene, Prefab, Build, Graybox, G4 또는 G5 passage 권한을 만들지
않는다.
