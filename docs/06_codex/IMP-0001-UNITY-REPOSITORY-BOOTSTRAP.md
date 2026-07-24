# IMP-0001 — Unity Repository Bootstrap and Exact Package-Lock Materialization

Status: `OWNER-APPROVED TICKET DECOMPOSITION / EXECUTION BLOCKED`

Review Readiness:
`READY FOR FINAL REVIEW / FINAL PRE-IMPLEMENTATION REVIEW NOT STARTED`

`REQUIRED EXECUTION BASELINE SHA: TO BE PROVIDED BY THE FINAL PRE-IMPLEMENTATION REVIEW`

## Authority and Execution Boundary

이 문서는 미래의 첫 구현 티켓을 세분화한 승인 문서다. 현재 활성
구현 작업이 아니며 Unity 프로젝트 생성, Package 설치, 코드 작성,
Graybox, Implementation Gate 개방 또는 Git 쓰기 권한을 부여하지
않는다.

현재 TASK-0058 기준선은 미래 구현 기준선이 아니다. 위
`REQUIRED EXECUTION BASELINE SHA`에 구체 SHA가 없으면 IMP-0001은
즉시 중지해야 한다. 구체 SHA가 제공된 뒤에도 local HEAD 또는
`origin/main`이 해당 SHA와 다르면 중지해야 한다.

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
5. Full Design Lock이 별도로 검토됐다.
6. Design Authority가
   `FINAL PRE-IMPLEMENTATION REVIEW REQUIRED`를 발행했다.
7. Final Review 결과가 IMP-0001 실행을 명시적으로 승인했다.
8. Implementation Gate가 `CLOSED`에서 `OPEN`으로 명시적으로 변경됐다.
9. 구체적인 미래 execution baseline SHA가 이 티켓에 삽입됐다.
10. 정확한 Unity Editor가 설치됐고 독립적으로 검증됐다.

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

1. Final Review가 제공한 baseline SHA와 clean repository state를
   검증한다.
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
Given the final-review baseline and an installed Unity Editor 6000.3.20f1
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

- concrete execution baseline present
- local HEAD equals required future SHA
- `origin/main` equals required future SHA
- branch is `main`
- tracked 및 staged state clean
- Implementation Gate is `OPEN`
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
| Commit | `ALLOWED ONLY AFTER FINAL REVIEW, OPEN GATE AND ALL VALIDATIONS` |
| Push | `ALLOWED ONLY AFTER THE SAME CONDITIONS` |

Future commit message:

`chore: materialize Unity project and exact package lock`

TASK-0058은 위 Git 쓰기 권한을 활성화하지 않는다.

## Future Stop Conditions

다음 조건 중 하나라도 발생하면 즉시 중지하고 임의로 우회하지 않는다.

- final baseline SHA가 없다.
- Implementation Gate가 `OPEN`이 아니다.
- Turnaround가 승인·등록되지 않았다.
- Design Freeze가 `32 / 32`가 아니다.
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
2. Final-Review Authorization Verification
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
SHA는 없고 Unity project도 없다. Final Pre-Implementation Review는
시작되지 않았으며 G4와 G5는 `NOT PASSED`, Graybox는 `NOT STARTED`,
Implementation Gate는 `CLOSED`다.
