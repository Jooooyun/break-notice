# Codex Prompt Library

## A. 설계 검토

```text
Read AGENTS.md and the listed design documents. Do not write code.
Find contradictions, undefined states, untestable requirements, and scope expansion.
Return findings ordered by severity with exact document paths and proposed wording.
```

## B. 구현 계획

```text
Read TASK-XXXX and all linked Canon documents. Do not edit files yet.
Produce a file-by-file implementation plan, test plan, dependency risks, and any conflict with locked decisions.
```

## C. 제한 구현

```text
Implement only TASK-XXXX. Modify only Allowed Files. Do not add packages or refactor unrelated code.
Run required tests and report changed files, test results, assumptions, and remaining risks.
```

## D. 리뷰

```text
Review the current diff against TASK-XXXX, AGENTS.md, linked requirements, performance budget, and acceptance tests.
Do not modify code. Report violations, missing tests, hidden coupling, reset leaks, and scope creep.
```

## E. 문서 동기화

```text
Compare implementation behavior with linked Canon documents and acceptance tests.
Do not change gameplay. Update only documentation that is stale, preserving locked decisions and traceability IDs.
```
