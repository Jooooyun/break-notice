# Codex Execution Protocol

## 1. 목적

Codex를 무제한 자동 코더가 아니라 **승인된 명세를 구현하는 제한된 작업자**로 사용한다.

## 2. 작업 단위

한 작업은 다음을 넘지 않는 것을 원칙으로 한다.

- 하나의 사용자 관찰 가능 행동
- 하나의 주요 시스템 또는 수직 Slice
- 변경 파일 3~8개 수준
- 독립 검증 가능한 인수 조건

## 3. 필수 입력

```text
Task ID
Goal
Requirement IDs
Allowed files
Forbidden changes
Preconditions
Implementation notes
Acceptance criteria
Required tests
Deliverable report
```

## 4. 표준 프롬프트 구조

```markdown
You are implementing task TASK-XXXX in the BREAK NOTICE repository.

Read first:
- AGENTS.md
- [관련 Canon 문서]

Goal:
[한 문장]

Requirement IDs:
[IDs]

Allowed files:
[목록]

Do not change:
[목록]

Acceptance criteria:
[Given/When/Then]

Required tests:
[목록]

Before editing, report any conflict with Canon. Do not invent missing behavior.
After editing, report changed files, tests, assumptions, and remaining risks.
```

## 5. 단계

1. **Read**: Canon과 티켓 확인
2. **Plan**: 파일 단위 계획
3. **Implement**: 허용 파일만 수정
4. **Test**: 자동·수동 검증
5. **Review**: Diff, 로그, 성능
6. **Report**: 변경·테스트·리스크

## 6. 실패 처리

- 요구사항 모호: 구현 중지, 질문 또는 Change Request
- 테스트 실패: 원인과 재현 절차 보고
- 설계 충돌: 코드 우회 금지
- 범위 초과 필요: 별도 작업 제안만 하고 현재 작업에는 포함하지 않음

## 7. G4-A Bounded Per-Ticket Gate and Relay

post-Bootstrap implementation은 CR-0002와 ADR-0022의 fail-closed 모델을
따른다.

- Default Gate: `CLOSED`
- Future open-state format:
  `OPEN — <APPROVED TICKET ID> ONLY / <EXACT APPROVED SCOPE>`
- 모든 open state는 ticket-, baseline-, branch-, scope-specific,
  one-shot, non-inheritable, non-reusable다.
- 각 ticket은 exact allowed/forbidden file, operation, acceptance
  criteria와 automated validation을 자체 소유한다.
- ticket은 execution SHA를 저장하지 않는다.
- 별도 Owner-approved Execution Authorization Relay만 이미
  `origin/main`에 push된 full immutable SHA, exact branch, clean
  local/remote equality, exact Gate state, operations, validations,
  commit/push 권한과 expiration을 공급할 수 있다.
- completion, Stop Condition, baseline drift, dirty pre-state, 다른
  ticket/scope, unauthorized dependency, scope expansion 또는 revocation은
  Gate와 Relay를 자동 만료한다.
- 이전 ticket의 Gate, Relay, SHA 또는 권한은 후속 ticket에 상속하거나
  재사용할 수 없다.

ADR-0019의 `OPEN — IMP-0001 ONLY / EMPTY BOOTSTRAP`은 완료된 IMP-0001의
역사적 semantic authority다. 첫 post-Bootstrap future instance는
`OPEN — IMP-0002 ONLY / RUNTIME IDENTITY AND STATE FOUNDATION`이지만
현재는 정의만 됐고 열리지 않았다. 현재 Gate는 `CLOSED`, active Relay는
`ABSENT`, IMP-0002 Relay는 `REQUIRED / NOT ISSUED`다.

Gate format이나 ticket decomposition은 다른 wave, gameplay, Scene,
Prefab, Graybox, external asset, G4 passage 또는 G5 passage를 자동으로
승인하지 않는다.

## 8. 코드 리뷰 질문

- 요구사항 밖 기능을 추가했는가?
- 물체가 순간이동하거나 전역 검색에 의존하는가?
- 물리 callback과 표현 계층이 결합됐는가?
- Reset에서 상태 누수가 있는가?
- 신규 dependency가 생겼는가?
- 테스트가 실제 실패를 잡는가?
- 성능 예산을 측정했는가?
