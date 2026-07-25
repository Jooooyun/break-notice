# Pre-G4 Full Design Lock

Status: `OWNER-APPROVED / LOCKED`

Date: `2026-07-25`

Frozen Design-Source Baseline SHA: `f638be24c6895a6969083626fa3f94228b533ec6`

## Authority and Decision Source

이 기록은 다음 두 결정을 하나의 Pre-G4 권위 전환으로 소비한다.

1. `TASK-0062` Final Pre-Implementation Re-Audit의
   `48 PASS / 0 FAIL` 및
   `READY FOR LIMITED IMPLEMENTATION GATE DECISION — IMP-0001 ONLY`;
2. `TASK-0063`의 Owner-delegated 결정:
   `PRE-G4 FULL DESIGN LOCK finalized; IMP-0001 Relay issuance authorized
   but not issued.`

ADR-0019가 정의한 세 Freeze 개념과 제한 Gate 의미를 유지한다. 이
기록과 ADR-0020은 감사된 설계 소스를 변경하지 않고 그 집합을
Owner-approved Lock으로 최종화한다.

## Frozen Design-Source Baseline

동결된 설계 소스 기준선은 정확히 다음 40자 immutable commit이다.

`f638be24c6895a6969083626fa3f94228b533ec6`

이 SHA는 TASK-0061 교정 뒤 TASK-0062 Re-Audit 기록을 포함해
`origin/main`에 push된 감사 기준선이다. 이후 Lock 기록용 commit은
이 설계 소스 SHA를 소급 변경하지 않는다. 미래 IMP-0001 Execution
Authorization Relay의 실행 SHA는 이 문서에 저장하지 않는다.

## Included G1/G2/G3 Authority

동결 범위에는 다음 승인 권위가 포함된다.

- G1 Requirements `PASSED`: 정확한 requirement/NFR 문구,
  Applicability, Priority, Authority, disposition과 기술 중립적 검증
  의도;
- G2 Design `PASSED / OWNER-APPROVED BASELINE`: 여덟 논리 계약의
  action intent, state arbitration, Weight-Class envelope,
  Momentum/Pip starting profile, causal propagation/termination,
  Ally Relay/Suppressed, Prototype content와 Policy Violation
  selection/run-application 경계;
- G3 Technical `PASSED / OWNER-APPROVED BASELINE`: 책임·dependency,
  mutable-state ownership, ordered event topology, identity 분리,
  Reset, observability, physics activation, Cargo Strap technical
  boundary, test-harness ownership과 exact Editor/Package
  documentation lock;
- Unity Editor `6000.3.20f1`, URP `17.3.0`, Input System `1.20.0`,
  Cinemachine `3.1.7`, Unity Test Framework `1.6.0`;
- AI Navigation:
  `DEFERRED / NOT IN PROTOTYPE PACKAGE BASELINE`.

G1/G2/G3 passage는 구현, test execution, runtime behavior 또는
Acceptance Test 통과를 뜻하지 않는다.

## Design Freeze Evidence

`docs/05_production/DESIGN_FREEZE_CHECKLIST.md`는 정확히
`32 / 32`, unchecked `0`이다. 이 체크리스트는 documentary evidence
prerequisite이며 스스로 G4, G5 또는 구현 권한을 만들지 않는다.

역사적 Final Pre-Implementation Review는 `44 PASS / 4 FAIL`과
`REVISION REQUIRED`로 보존한다. TASK-0061은 네 실패의 두 원인인
Gate sequencing과 self-referential baseline binding을 교정했다.
TASK-0062 Re-Audit은 네 실패를 모두 closure하고 정확히
`48 PASS / 0 FAIL`을 기록했다. 이 성공 결과가 본 Lock 결정의 직접
감사 근거다.

## Visual Canon and Production Support

다음 여덟 Production Visual은 기존 Visual Spec과 Accepted ADR의
Canonical Elements만 `LOCKED`다.

1. CHR_00 Player Identity;
2. GEAR_01 Cargo Strapping System;
3. CHR_01 First Ally Identity;
4. PGT_01 Gameplay Camera;
5. PGT_02 Chair Redirect Core Loop;
6. PGT_03 Chain Reaction Aftermath;
7. LGT_01 Night Archive Lighting;
8. FIN_01 Conference Table Team Finisher.

CHR_00 Player Turnaround v001은 별도 한 개의
`LOCKED PRODUCTION SUPPORT`다. 주 CHR_00 Identity 권위에 종속되며
새 Identity, gameplay, topology, rig, material, Unity asset 또는 구현
권한을 만들지 않는다. LGT_01은 Records Archive-local `COULD`,
FIN_01은 `OUT / COULD`이며 제품·Slice 범위에 자동 편입되지 않는다.

## Prototype Content Boundary

첫 gameplay 구현 파동의 설계 범위는 다음으로 고정된다.

- room `1`;
- CHR_00 `1`, CHR_01 `1`, 동일 Compliance Runner `2`로 actor `4`;
- 고정 Prototype support position `1`;
- object definition `4`, object instance `7`;
- File Binder `3`, Wheeled Office Chair `2`, Rolling Drawer Cabinet
  `1`, Copier-Class Asset `1`;
- gameplay-interactive definition `3`, recognition-only definition `1`;
- Prototype destruction category cap 전부 `0`.

Perfect Catch, Momentum/Pip, HeavyAction, Policy Violation, Relay Position
transition, Suppressed, LGT_01과 FIN_01은 Prototype 첫 파동 밖이다.

## Destruction and Office Core Kit Locks

ADR-0016과
`DESTRUCTION_BUDGET_AND_IMPLEMENTATION_RULES.md`의 Prototype
zero-destruction profile, Later-Slice hard scope caps, pre-authored state,
preallocated pooling, atomic admission, causal-carrier, Reset와 fault
경계를 포함한다. 이는 measured performance 또는 G4/G6 evidence가
아니다. Structural Feature는 별도 G5 승인이 필요한
`VERTICAL SLICE / SHOULD / PARTIAL`이다.

ADR-0017과 `OFFICE_CORE_KIT.md`의 정확한 `16`개
Prototype-required family, `8`개 Later-Slice candidate, reuse와
variant cap을 포함한다. 실제 model, texture, material, layout, Prefab,
Unity asset, Breakable identity와 Structural inclusion은 포함하지
않는다.

## Approved Implementation-Ticket Boundary

`IMP-0001 — Unity Repository Bootstrap and Exact Package-Lock
Materialization`의 ticket decomposition은 Owner-approved다. 유일한
미래 결과는 `Game/` 아래의 otherwise empty Unity repository
Bootstrap, exact direct Package lock materialization과 clean Editor
startup 검증이다.

이 Lock은 IMP-0001의 열 가지 mandatory precondition 중 1~6만
충족한다. 7~10인 별도 Relay, exact limited Gate, Relay-supplied
immutable execution SHA와 설치된 exact Editor 증거는 미충족이다.
따라서 IMP-0001은 계속 `EXECUTION BLOCKED`다.

## Explicit Deferred and Later-Gate Evidence

다음은 본 Lock의 통과 주장에 포함되지 않는다.

- exact physical mass, dimension, speed, force, damage와 timing;
- final balance, input binding, buffer, animation, AI와 navigation;
- runtime class, payload, schema, tuning과 production architecture
  materialization;
- gameplay test, Acceptance Test와 playtest 실행 결과;
- FPS, hardware, frame-time과 performance passage;
- final Breakable/Structural identity와 G5 production asset list;
- exact Slice room, enemy, object, upgrade, VFX, audio와 UI content;
- 3D model, texture, material, rig, animation과 production asset;
- Unity project, Package resolution, code, Scene, Prefab, Test, Build,
  Graybox와 runtime implementation.

위 증거는 해당 G4, G5 또는 G6 Gate와 별도 승인 task가 소유한다.

## Change-Control Boundary

동결 기준선의 requirement, G2 contract, G3 technical decision, Visual
Canonical Element, Prototype content, destruction cap, Office Core Kit
scope 또는 IMP-0001 ticket boundary를 바꾸려면
`docs/00_project/CHANGE_CONTROL.md`의 등급·영향 분석·Owner 승인과
필요한 ADR 교체를 먼저 수행한다. 후속 구현 또는 이미지의 incidental
detail은 이 Lock을 조용히 변경할 수 없다.

## G4 and G5 Non-Passage

- G4 Graybox:
  `NOT PASSED / NOT STARTED / NOT AUTHORIZED`;
- G5 Production Freeze:
  `NOT PASSED / REQUIRES G4 EVIDENCE`;
- Graybox: `NOT STARTED`.

Pre-G4 Full Design Lock은 pre-Graybox frozen documentation baseline이며
재미, runtime physics, performance, G4 또는 G5 passage를 주장하지
않는다. G5는 G4 evidence와 explicit G4 decision 뒤에만 가능하다.

## Limited Gate and Relay-Issuance Boundary

- Pre-G4 Full Design Lock: `OWNER-APPROVED / LOCKED`;
- IMP-0001 Execution Authorization Relay issuance:
  `OWNER-AUTHORIZED / RELAY NOT ISSUED`;
- Implementation Gate current state: `CLOSED`;
- Concrete IMP-0001 execution SHA:
  `ABSENT / FUTURE RELAY REQUIRED`.

`OPEN — IMP-0001 ONLY / EMPTY BOOTSTRAP`은 여전히 future one-shot
state다. 이 문서는 Gate를 열거나 Relay를 발급하지 않는다. 미래 Relay
발급은 별도의 `TASK-0064`가 이 Lock 최종 commit의 이미 push된 완전한
40자 SHA를 기준으로 수행해야 한다. 이 문서에는 미래 SHA를 예측하거나
삽입하지 않는다.

## Current Repository Implementation State

현재 `Game/**`는 승인된 placeholder 구조와 `Game/README.md`만 가진다.
`ProjectVersion.txt`, Package manifest, Package lock, C#, asmdef, Scene,
Prefab, authored Test, CI와 Build configuration은 없다. Unity Editor를
실행하지 않았고 Package를 materialize하지 않았다.

Repository implementation state는 `EMPTY`, Graybox는 `NOT STARTED`,
IMP-0001은 `EXECUTION BLOCKED`, Implementation Gate는 `CLOSED`다.

## Next Authorization Boundary

정확히 하나의 다음 bounded task는 다음과 같다.

`TASK-0064 — IMP-0001 Execution Authorization Relay — Open the One-Shot Limited Gate and Materialize the Empty Unity Bootstrap`

TASK-0064는 본 Lock 최종 pushed SHA를 concrete baseline으로 사용하고
ADR-0019와 IMP-0001이 요구하는 ticket, branch, clean equality,
allowed/forbidden files, operations, validations, commit/push와
one-shot expiration을 모두 명시해야 한다. 그 Relay가 실제로 발급되기
전에는 IMP-0001, Unity, Package materialization 또는 Gate 개방을
실행하지 않는다.
