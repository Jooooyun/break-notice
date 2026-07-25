# IMP-0001 — Unity Repository Bootstrap and Exact Package-Lock Materialization

Status: `OWNER-APPROVED TICKET DECOMPOSITION / EXECUTION BLOCKED / PRE-G4 FULL DESIGN LOCK SATISFIED / TOOLCHAIN PROVISIONING OWNER-DEFERRED`

Review Readiness:
`FINAL PRE-IMPLEMENTATION RE-AUDIT PASSED / PRE-G4 FULL DESIGN LOCK OWNER-APPROVED / TOOLCHAIN PROVISIONING DEFERRED / NEW EXECUTION RELAY NOT AUTHORIZED`

`REQUIRED EXECUTION BASELINE SHA: SUPPLIED BY A SEPARATE OWNER-APPROVED EXECUTION AUTHORIZATION RELAY — NOT STORED IN THIS TICKET`

## Authority and Execution Boundary

이 문서는 미래의 첫 구현 티켓을 세분화한 승인 문서다. 현재 활성
구현 작업이 아니며 Unity 프로젝트 생성, Package 설치, 코드 작성,
Graybox, Implementation Gate 개방 또는 Git 쓰기 권한을 부여하지
않는다.

현재 도구 의도와 provisioning 상태의 권위 기록은
`docs/05_production/UNITY_TOOLCHAIN_INTENT_AND_PROVISIONING_STATUS.md`다.
정확한 Editor·Package 버전은 `LOCKED INTENT`지만 설치, Unity Hub,
라이선스, batch mode, Package resolution과 구현 준비는 검증되지
않았고 Human Owner가 provisioning을 연기했다.

이 티켓은 구체 execution SHA를 저장하거나 실행 직전에 수정하지
않는다. 미래 실행 기준선은 별도의 Owner-approved Execution
Authorization Relay가 이미 `origin/main`에 push된 완전한 40자
immutable SHA로 공급해야 한다. 실행 직전 local HEAD와 `origin/main`이
모두 그 SHA와 같고 ahead/behind가 `0 / 0`, tracked/staged 상태가
clean이어야 한다. 기준선 drift가 발생하면 기존 Relay는 무효이며 새
Relay가 필요하다.

이 티켓의 단일 결과는 다음과 같다.

`Materialize an otherwise empty Unity project under Game/ using the Owner-approved Editor and direct Package locks, verify clean package resolution and Editor startup, and create no gameplay, scene, prefab, test or production asset.`

이는 하나의 infrastructure 결과이며 gameplay를 구성하지 않는다.

## Requirement and Authority Mapping

- `NFR-M-005` — Package 변경에는 명시적 승인이 필요하다.
- `NFR-D-001` — 하나의 bounded task만 수행한다.
- `NFR-D-002` — 구현 전에 validation intent를 정의한다.
- `docs/04_technical/G3_TECHNICAL_BASELINE.md` —
  `## Exact Editor and Package Lock`
- `docs/04_technical/UNITY_PROJECT_STANDARD.md` —
  `## Exact Documentation Lock`
- `docs/04_technical/G3_TECHNICAL_BASELINE.md` —
  `## Prototype and Later-Scope Separation`
- `docs/09_adrs/ADR-0015-G3-TECHNICAL-BASELINE.md`

새 Product Requirement 또는 Functional Requirement를 만들지 않는다.

## Read First

- `AGENTS.md`
- `README.md`
- `MANIFEST.json`
- `docs/00_project/DESIGN_STATUS.md`
- `docs/00_project/CHANGE_CONTROL.md`
- `docs/04_technical/G3_TECHNICAL_BASELINE.md`
- `docs/04_technical/UNITY_PROJECT_STANDARD.md`
- `docs/09_adrs/ADR-0015-G3-TECHNICAL-BASELINE.md`
- `Game/README.md`
- `.gitignore`
- `docs/06_codex/CODEX_EXECUTION_PROTOCOL.md`
- `docs/05_production/DESIGN_FREEZE_CHECKLIST.md`
- `docs/05_production/MILESTONES_AND_GATES.md`
- `docs/07_quality/TEST_STRATEGY.md`

## Mandatory Future Execution Preconditions

다음 열 가지 조건이 모두 참이기 전에는 IMP-0001을 실행할 수 없다.
어느 한 조건도 다른 조건의 충족을 암시하지 않는다.
`No one condition may imply another.`

1. CHR_00 Turnaround 후보가 별도의 명시적 Owner 요청 아래 생성됐다.
2. Turnaround가 binary Review Contract를 통과했다.
3. Owner가 Turnaround를 명시적으로 승인하고 등록했다.
4. Design Freeze가 정확히 `32 / 32`다.
5. `PRE-G4 FULL DESIGN LOCK`이 명시적으로
   `OWNER-APPROVED / LOCKED`다.
6. Final Pre-Implementation Re-Audit 결과가 정확히
   `READY FOR LIMITED IMPLEMENTATION GATE DECISION — IMP-0001 ONLY`다.
7. 별도의 Owner-approved Execution Authorization Relay가 IMP-0001을
   명시적으로 승인한다.
8. Relay가 Gate를 정확히
   `OPEN — IMP-0001 ONLY / EMPTY BOOTSTRAP`으로 설정한다.
9. Relay가 구체 immutable SHA를 공급하고 clean local
   HEAD/`origin/main`/`main` 일치를 요구한다.
10. Unity Editor `6000.3.20f1`이 설치됐고 독립적으로 검증됐다.

### Current Precondition Evidence

| # | Mandatory precondition | Current evidence |
|---:|---|---|
| 1 | Owner-authorized Turnaround generation | SATISFIED |
| 2 | Binary Review Contract PASS | SATISFIED |
| 3 | Owner approval and registration | SATISFIED |
| 4 | Design Freeze `32 / 32` | SATISFIED |
| 5 | Pre-G4 Full Design Lock `OWNER-APPROVED / LOCKED` | SATISFIED |
| 6 | Re-Audit exact limited-Gate readiness result | SATISFIED |
| 7 | Separate Owner-approved Execution Authorization Relay | NOT SATISFIED / EAR-IMP-0001-001 EXPIRED UNUSED / NO ACTIVE RELAY |
| 8 | Gate `OPEN — IMP-0001 ONLY / EMPTY BOOTSTRAP` | NOT SATISFIED / IMPLEMENTATION GATE CLOSED |
| 9 | Relay-supplied immutable SHA and clean equality | NOT SATISFIED / NO ACTIVE RELAY-SUPPLIED SHA |
| 10 | Installed and independently verified Editor `6000.3.20f1` | NOT SATISFIED / OWNER-DEFERRED / INSTALLATION AND VERIFICATION NOT AUTHORIZED |

Current progress: `6 / 10`.

Pre-G4 Full Design Lock은 조건 1~6을 충족하지만 7~10을 암시하지
않는다. TASK-0064에서 발급됐던 `EAR-IMP-0001-001`은 exact Editor
부재 Stop Condition 뒤 `EXPIRED / UNUSED`가 됐고 Gate는 열리지
않았다. concrete execution SHA와 active Relay는 없으며 새 Relay와
provisioning은 Human Owner의 새 직접 지시 전까지 승인되지 않는다.
따라서 실행은 계속 차단된다.

## Execution Authorization Relay

Execution Authorization Relay는 이 티켓과 별개의 bounded
Owner-approved instruction이다. Relay는 다음을 모두 포함해야 한다.

- ticket ID `IMP-0001`;
- 이미 `origin/main`에 push된 완전한 40자 immutable commit SHA;
- branch `main`;
- local HEAD와 `origin/main`의 SHA 일치, ahead/behind `0 / 0`,
  clean tracked/staged 상태;
- Gate 상태 `OPEN — IMP-0001 ONLY / EMPTY BOOTSTRAP`;
- 정확한 allowed/forbidden files;
- 정확한 operations와 validations;
- commit/push 권한;
- one-shot 및 expiration 조건.

`current main`, `latest main`, short SHA, 과거 task에서 상속한 SHA,
권위 commit 전에 티켓에 삽입한 SHA, 또는 권위 commit이 push되기 전에
공급된 SHA는 무효다. 기준선 선택을 위해 이 티켓을 수정하지 않는다.

TASK-0061 교정 commit push, Final Pre-Implementation Re-Audit의 정확한
`READY FOR LIMITED IMPLEMENTATION GATE DECISION — IMP-0001 ONLY`와
TASK-0063 Pre-G4 Full Design Lock 최종화는 완료됐다. 이후 TASK-0064의
`EAR-IMP-0001-001`은 활성화 전에 Stop Condition을 만나
`EXPIRED / UNUSED`가 됐다. 현재 active Relay와 Relay-supplied SHA는
없으며 새 Relay는 승인되지 않았다. 향후 Human Owner가 provisioning을
직접 다시 지시한 뒤에도 새 bounded Relay가 위 모든 필드를 다시
제공해야 한다.

## Limited Implementation Gate Boundary

미래 Gate 상태는 정확히 다음 둘 중 하나다.

- `CLOSED`: 어떤 implementation ticket도 실행할 수 없다.
- `OPEN — IMP-0001 ONLY / EMPTY BOOTSTRAP`: 이 티켓의 아래 empty
  Bootstrap 범위만 한 번 실행할 수 있다.

제한 Gate는 gameplay, C#, Test, Scene, Prefab, Graybox, 다른 ticket,
G4/G5 passage 또는 상속 가능한 구현 권한을 만들지 않는다. 다음 중
하나가 발생하면 자동 만료된다.

- IMP-0001 완료 또는 stop condition;
- local HEAD 또는 `origin/main` drift;
- 작업 전 tracked/staged 변경;
- 다른 ticket·scope 시도;
- 승인되지 않은 Package;
- 범위 확대;
- 권위 철회.

만료·소비된 Gate와 Relay는 재사용할 수 없으며 새 실행에는 새 Relay가
필요하다.

## Exact Future Editor and Package Lock

floating version은 허용하지 않는다.

| Identifier | Exact future lock |
|---|---|
| Unity Editor | `6000.3.20f1` |
| `com.unity.render-pipelines.universal` | `17.3.0` |
| `com.unity.inputsystem` | `1.20.0` |
| `com.unity.cinemachine` | `3.1.7` |
| `com.unity.test-framework` | `1.6.0` |

규칙:

- AI Navigation은 absent 및 `DEFERRED`를 유지한다.
- 추가 direct non-module Package를 허용하지 않는다.
- Unity built-in `com.unity.modules.*` entry는 허용한다.
- generated transitive dependency는 실제
  `Game/Packages/packages-lock.json`에서만 허용한다.
- transitive version을 수동으로 발명하지 않는다.
- Addressables, DOTS, Entities, Netcode와 이전에 금지된 framework는
  계속 제외한다.

## Future Allowed Files

별도 승인된 미래 실행은 다음 경로만 변경할 수 있다.

- `Game/ProjectSettings/**`
- `Game/Packages/manifest.json`
- `Game/Packages/packages-lock.json`
- `Game/Assets/**/*.meta`
- `Game/Assets/_Project/**` 아래의 기존 `.gitkeep`
- `Game/README.md`
- `CHANGELOG.md`
- `MANIFEST.json`
- `docs/00_project/DESIGN_STATUS.md`

승인된 placeholder 구조를 보존하는 데 필요한 Unity-generated directory
metadata는 만들 수 있다. non-meta gameplay asset은 만들 수 없다.

## Future Forbidden Files and Content

다음 경로 또는 콘텐츠는 금지한다.

- root `Packages/**`
- root `ProjectSettings/**`
- `docs/01_requirements/**`
- `docs/02_design/**`
- 승인된 모든 Visual Spec과 ADR
- `references/**`
- `.github/**`
- C# 파일과 asmdef
- Unity Scene과 Prefab
- Test와 test assembly
- Input Action asset
- URP pipeline asset, Renderer asset와 Volume Profile
- Material, Texture, Model, Animation과 Audio
- UI, gameplay Data와 ScriptableObject
- build configuration과 CI
- Graybox content

Sample Scene과 Template Sample Content는 commit 결과에 남아서는 안 된다.

## Future Exact Operations

별도 승인된 미래 실행자는 다음을 정확히 수행한다.

1. Execution Authorization Relay가 제공한 baseline SHA, 정확한 제한
   Gate 상태와 clean repository state를 검증한다.
2. Unity Editor `6000.3.20f1`을 검증한다.
3. 다른 Editor revision으로 대체하지 않고 중지한다.
4. Unity project root를 `Game/`에 materialize한다.
5. 기존 `Game/Assets/_Project/**` placeholder 구조를 보존한다.
6. `Game/ProjectSettings/ProjectVersion.txt`를 생성한다.
7. 정확한 direct Package manifest를 생성한다.
8. Unity가 실제 Package lock을 생성하게 한다.
9. 승인되지 않은 direct non-module Package를 모두 제거한다.
10. AI Navigation이 없음을 확인한다.
11. Sample Scene과 모든 Template Sample Content를 제거한다.
12. C#, asmdef, Scene, Prefab 또는 Test를 만들지 않는다.
13. non-interactive batch mode로 프로젝트를 열고 정상 종료한다.
14. Editor log에서 compile, Package-resolution과 missing-dependency
    error를 검사한다.
15. generated cache directory가 tracked되지 않았음을 확인한다.
16. 허용된 repository status file만 갱신한다.
17. 모든 미래 acceptance criterion이 PASS인 뒤에만 commit하고 push한다.

## Future Acceptance Criteria

### Exact Editor

```gherkin
Given the Execution Authorization Relay baseline and an installed Unity Editor 6000.3.20f1
When the Game project is materialized
Then Game/ProjectSettings/ProjectVersion.txt identifies 6000.3.20f1
And no substitute Editor revision is used
```

### Exact Direct Package Set

```gherkin
Given the Owner-approved Package documentation lock
When Game/Packages/manifest.json is inspected
Then every approved direct non-module Package uses its exact version
And no additional direct non-module Package is present
And AI Navigation is absent
```

### Actual Package Lock

```gherkin
Given a successful Unity Package resolution
When Game/Packages/packages-lock.json is inspected
Then the four approved direct Package locks in the five-item Editor-and-Package lock set resolve to the approved versions
And all other entries are generated dependencies rather than invented manual additions
```

### Empty Bootstrap Scope

```gherkin
Given the materialized Game project
When tracked project content is inventoried
Then no C# file exists
And no asmdef exists
And no Scene exists
And no Prefab exists
And no Test exists
And no authored gameplay or production asset exists
```

### Clean Editor Startup

```gherkin
Given the materialized project and resolved Packages
When Unity 6000.3.20f1 opens the project in batch mode
Then the process exits successfully
And no compilation error is reported
And no missing direct Package is reported
```

## Future Required Validations

각 항목을 정확히 `PASS` 또는 `FAIL`로 보고한다. 하나라도 FAIL이면
IMP-0001을 중지한다.

- separate Execution Authorization Relay present
- Relay ticket ID equals `IMP-0001`
- concrete full-length immutable execution baseline present in the Relay
- local HEAD equals required future SHA
- `origin/main` equals required future SHA
- branch is `main`
- tracked 및 staged state clean
- Implementation Gate is exactly `OPEN — IMP-0001 ONLY / EMPTY BOOTSTRAP`
- exact Editor verified
- project root is `Game/`
- ProjectVersion exists
- Package manifest exists
- Package lock exists
- exact direct Package versions
- no floating Package version
- AI Navigation absent
- no unauthorized direct Package
- no root Package or ProjectSettings folder
- no C#
- no asmdef
- no Scene
- no Prefab
- no Test
- no Input Action asset
- no URP configuration asset
- no sample content
- no tracked Library, Temp, Logs, Obj 또는 UserSettings
- batch-mode startup succeeds
- compilation error count equals zero
- Package-resolution error count equals zero
- only future Allowed Files changed
- Manifest parses and hashes validate
- `git diff --check` passes

## Future Performance Boundary

- empty Bootstrap에는 FPS 또는 frame-time requirement를 적용하지 않는다.
- Prototype performance claim을 만들지 않는다.
- Editor startup, Package resolution과 tracked-file hygiene만 측정한다.
- performance profiling은 나중의 별도 승인 티켓에서 시작한다.

## Future Git Permissions

| Operation | Permission |
|---|---|
| Branch creation | `FORBIDDEN` |
| Branch switching | `FORBIDDEN` |
| Pull request | `FORBIDDEN` |
| Merge | `FORBIDDEN` |
| Rebase | `FORBIDDEN` |
| Force push | `FORBIDDEN` |
| Commit | `ALLOWED ONLY AFTER PRE-G4 FULL DESIGN LOCK, SUCCESSFUL RE-AUDIT, VALID EXECUTION RELAY, EXACT LIMITED GATE AND ALL VALIDATIONS` |
| Push | `ALLOWED ONLY AFTER THE SAME CONDITIONS` |

Future commit message:

`chore: materialize Unity project and exact package lock`

TASK-0061과 TASK-0063은 위 Git 쓰기 권한을 활성화하지 않는다.

## Future Stop Conditions

다음 조건 중 하나라도 발생하면 즉시 중지하고 임의로 우회하지 않는다.

- 별도의 Execution Authorization Relay가 없다.
- Relay ticket ID가 `IMP-0001`과 다르다.
- Relay에 완전한 40자 immutable baseline SHA가 없다.
- Relay SHA가 이미 `origin/main`에 push된 commit이 아니다.
- Implementation Gate가 정확히
  `OPEN — IMP-0001 ONLY / EMPTY BOOTSTRAP`이 아니다.
- Relay 또는 제한 Gate가 만료·소비·철회됐다.
- local HEAD 또는 `origin/main`이 Relay SHA에서 drift했다.
- 다른 ticket 또는 scope가 함께 요청됐다.
- Turnaround가 승인·등록되지 않았다.
- Design Freeze가 `32 / 32`가 아니다.
- Pre-G4 Full Design Lock이 `OWNER-APPROVED / LOCKED`가 아니다.
- Final Pre-Implementation Re-Audit 결과가 정확한 limited-Gate
  readiness 판정이 아니다.
- 정확한 Editor를 사용할 수 없다.
- 다른 Editor version이 필요하다.
- direct Package version이 승인 lock과 충돌한다.
- 다른 direct Package가 필요해 보인다.
- Unity가 gameplay, Scene, Prefab, C#, Test 또는 build file을 요구한다.
- 승인 Canon 또는 G2/G3 baseline을 변경해야 한다.
- Future Allowed Files 밖의 파일을 변경해야 한다.
- repository에 선행 tracked 또는 staged 변경이 있다.
- push 전에 `origin/main`이 변경된다.
- validation 하나라도 실패한다.

## Future Completion Report Contract

완료 보고에는 정확히 다음 section을 사용한다.

1. Task Summary
2. Execution Authorization Relay Verification
3. Execution Baseline Verification
4. Exact Editor Verification
5. Files Read
6. Files Created
7. Files Modified
8. Scope Intentionally Not Changed
9. Unity Project Root
10. ProjectVersion Result
11. Package Manifest Result
12. Package Lock Result
13. Direct Package Allowlist Review
14. AI Navigation Exclusion
15. Sample-Content Removal
16. Empty Bootstrap Inventory
17. Batch-Mode Startup Result
18. Compilation and Package-Resolution Log Review
19. Generated-Cache Hygiene
20. Manifest Validation
21. Full Validation Results
22. Commit Result
23. Push Result
24. Final Pushed SHA
25. Blocking Issues
26. Non-Blocking Notes
27. Confirmation that No Gameplay Was Implemented
28. Confirmation that No Scene, Prefab, Code or Test Was Created
29. Confirmation that G4 Was Not Passed by the Ticket
30. Confirmation that Graybox Was Not Started

## Current Boundary Confirmation

IMP-0001 세분화는 완료됐지만 실행은 차단되어 있다. concrete execution
SHA는 이 티켓에 없다. Final Pre-Implementation Review는
`REVISION REQUIRED`로 역사 보존되고 TASK-0061 authority correction은
TASK-0062 Re-Audit의 `48 / 48 PASS`로 검증됐다. TASK-0063은 Pre-G4
Full Design Lock을 `OWNER-APPROVED / LOCKED`로 최종화했다. 역사적
TASK-0064는 exact Editor 부재에 Stop Condition을 올바르게 적용했고
repository change, commit과 push는 없었다. `EAR-IMP-0001-001`은
`EXPIRED / UNUSED`, active Relay와 concrete execution SHA는
`ABSENT`다. 현재 mandatory precondition은 `6 / 10`이고 toolchain
provisioning은 `OWNER-DEFERRED`다. G4는
`NOT PASSED / NOT STARTED / NOT AUTHORIZED`, G5는
`NOT PASSED / REQUIRES G4 EVIDENCE`, Graybox는 `NOT STARTED`,
Implementation Gate는 `CLOSED`다. Unity project는 없다. 현재 다음
권한 경계는
`OWNER EXPLICIT UNITY TOOLCHAIN PROVISIONING INSTRUCTION REQUIRED`다.
