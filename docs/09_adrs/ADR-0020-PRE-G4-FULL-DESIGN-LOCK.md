# ADR-0020 — Pre-G4 Full Design Lock Finalization

- Status: Accepted
- Date: 2026-07-25
- Decision Source:
  `TASK-0062 successful Final Pre-Implementation Re-Audit` and
  `TASK-0063 Owner-delegated decision`
- Frozen Design-Source Baseline SHA:
  `f638be24c6895a6969083626fa3f94228b533ec6`
- Scope: Pre-G4 documentation Lock finalization and future IMP-0001 Relay
  issuance authorization only

## Context

ADR-0019는 Design Freeze Checklist, Pre-G4 Full Design Lock과 post-G4
G5 Production Freeze를 분리하고, self-referential ticket SHA 대신
별도 Execution Authorization Relay를 정의했다.

TASK-0062 Re-Audit은 TASK-0061 교정 뒤 네 역사적 실패를 모두
closure하고 `48 PASS / 0 FAIL` 및
`READY FOR LIMITED IMPLEMENTATION GATE DECISION — IMP-0001 ONLY`를
기록했다. Design Freeze는 `32 / 32`이고 감사된 설계 소스 기준선은
`f638be24c6895a6969083626fa3f94228b533ec6`이다.

## Decision

`docs/05_production/PRE_G4_FULL_DESIGN_LOCK.md`를 Pre-G4 frozen
documentation baseline의 권위 기록으로 승인한다.

- Pre-G4 Full Design Lock:
  `OWNER-APPROVED / LOCKED`;
- Frozen Design-Source Baseline SHA:
  `f638be24c6895a6969083626fa3f94228b533ec6`;
- IMP-0001 Execution Authorization Relay issuance:
  `OWNER-AUTHORIZED / RELAY NOT ISSUED`;
- Implementation Gate: `CLOSED`;
- concrete IMP-0001 execution SHA:
  `ABSENT / FUTURE RELAY REQUIRED`.

이 결정은 G1/G2/G3 passage, Design Freeze `32 / 32`, 승인 Visual
Canon과 Production Support, Prototype content, destruction/Office Core
Kit Lock, IMP-0001 decomposition과 성공한 TASK-0062 Re-Audit을 하나의
pre-Graybox documentation baseline으로 고정한다.

## Relay Boundary

본 결정은 Relay 발급 권한만 승인한다. Relay 자체를 발급하거나 Gate를
열지 않는다. 미래의 별도
`TASK-0064 — IMP-0001 Execution Authorization Relay — Open the One-Shot Limited Gate and Materialize the Empty Unity Bootstrap`
만 이 ADR을 소비할 수 있다.

TASK-0064 Relay는 TASK-0063 최종 commit이 `origin/main`에 push된 뒤 그
완전한 40자 immutable SHA를 execution baseline으로 명시해야 한다.
이 ADR은 미래 SHA를 저장하거나 예측하지 않는다.

## Non-Decision

이 ADR은 다음을 승인하지 않는다.

- `OPEN — IMP-0001 ONLY / EMPTY BOOTSTRAP`의 현재 적용;
- Execution Authorization Relay 발급;
- Unity Editor 실행 또는 설치;
- Package manifest/lock materialization;
- C#, asmdef, Scene, Prefab, Test, Build 또는 production asset;
- gameplay, Graybox, G4 passage, G5 passage 또는 G6 passage;
- deferred tuning, performance와 Acceptance Test evidence;
- FIN_01 제품 편입 또는 LGT_01 전역화.

## Consequences

- Pre-G4 Full Design Lock은 최종화된다.
- Design Freeze는 `32 / 32 / COMPLETE / DOCUMENTARY PREREQUISITE`로
  유지된다.
- G4는 `NOT PASSED / NOT STARTED / NOT AUTHORIZED`다.
- G5는 `NOT PASSED / REQUIRES G4 EVIDENCE`다.
- Graybox는 `NOT STARTED`다.
- Implementation Gate는 `CLOSED`다.
- IMP-0001은 열 가지 precondition 중 `6 / 10`만 충족해
  `EXECUTION BLOCKED`다.
- 다음 단일 작업은 TASK-0064 Relay issuance다.
