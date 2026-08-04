# Unity Toolchain Intent and Provisioning Evidence

> **OWNER-APPROVED TOOLCHAIN INTENT AND VERIFIED POINT-IN-TIME EVIDENCE —
> NOT THE LIVE PROJECT-STATE BOARD**
>
> Date: `2026-07-25`
>
> Authority: Human Owner direct instruction superseding the earlier
> environment-provisioning form of `TASK-0065`.

## Authority and Scope

이 문서는 향후 별도 승인된 Unity Bootstrap이 사용해야 할 정확한
Editor와 direct Package 의도, 그리고 TASK-0071-R3에서 별도 승인된
Editor 복구·검증의 point-in-time evidence를 기록한다. 설치 검증
증거는 Package materialization, BREAK NOTICE Unity project, 테스트,
구현, G4, Graybox, Relay 발급 또는 Gate 개방을 수행하거나 승인하지
않는다.

Pre-G4 Full Design Lock의 frozen-source 내용은 변경하지 않는다.
그 문서와 과거 감사 기록의 다음 작업 문구는 작성 당시의
point-in-time evidence다. 현재 Phase, Gate, Relay, provisioning,
execution readiness와 다음 권한 경계의 유일한 가변 권위는
[`docs/00_project/DESIGN_STATUS.md`](../00_project/DESIGN_STATUS.md)다.

## Exact Intended Future Toolchain

| Toolchain item | Exact locked intent |
|---|---|
| Unity Editor | `6000.3.20f1` |
| Unity Editor changeset | `c9ba695d4f07` |
| Universal Render Pipeline | `17.3.0` |
| Input System | `1.20.0` |
| Cinemachine | `3.1.7` |
| Unity Test Framework | `1.6.0` |
| AI Navigation | `DEFERRED / NOT IN PROTOTYPE DIRECT-PACKAGE BASELINE` |

위 표는 `OWNER-APPROVED / LOCKED INTENT`다. floating version과 추가
direct Package를 승인하지 않는다. AI Navigation은 Prototype direct
Package baseline 밖이며 계속 `DEFERRED`다.

## TASK-0071-R3 Current Verified Evidence

### Authority and historical stop preservation

- Repository baseline: `b82bb382fc0439847d6497ee2fadd9c55781a9fa`
- Owner Version Decision: `A — RETAIN UNITY 6000.3.20f1 / c9ba695d4f07`
- TASK-0071: `ELEVATION STOP / HISTORICAL`
- TASK-0071-R1: `PARTIAL INSTALLATION / UNITY.EXE ABSENT / HISTORICAL`
- TASK-0071-R2: `UNSIGNED UNINSTALLER TRUST CHECK STOP / HISTORICAL`
- TASK-0071-R3:
  `PARTIAL ROOT QUARANTINED / FRESH INSTALL VERIFIED / COMPLETE`

### Partial root identity and quarantine

| Evidence | Verified result |
|---|---|
| Partial source root | `C:\Program Files\Unity\Hub\Editor\6000.3.20f1` |
| Required `Unity.exe` before recovery | `ABSENT` |
| Source/descendant reparse points | `0 / 0` |
| `Editor\Unity.com` SHA-256 | `cd26e404b85345ea8eda0476ea35602e23f6000c1d865fb44593cdc865c09afc` |
| `Editor\Uninstall.exe` SHA-256 | `21cfe061255c73f9b0d6757e311a5f595ba7dc4d780a925d86c0f661265a81fa` |
| Unsigned Uninstaller status | `NOT SIGNED / NOT EXECUTED` |
| Partial file count | `31,086` |
| Partial directory count | `3,679` |
| Partial total bytes | `6,111,841,704` |
| Exact quarantine root | `C:\Program Files\Unity\Hub\Editor\6000.3.20f1.partial-task0071-r1` |
| Quarantine result | `RENAMED EXACTLY / IDENTITY REVERIFIED / REMOVED AFTER FULL SUCCESS` |

Source와 quarantine 내부 Unity binary는 실행하지 않았다. wildcard,
Registry edit, reparse point, hard-link substitution, 다른 Editor 변경
또는 보안 설정 변경 없이 exact root만 격리했고, 두 batch startup과
후속 검증이 모두 끝난 뒤 exact quarantine root만 제거했다.

### Signed installer and fresh installation

| Evidence | Verified result |
|---|---|
| Installer path before cleanup | `C:\Users\jhbjm\AppData\Local\Temp\break-notice-task0071\UnitySetup64-6000.3.20f1.exe` |
| Installer bytes | `4,173,270,160` |
| Installer SHA-256 | `0e55b2b4af53a40f4204899eecc54f0020dc06e7a2de4810a1c65792ffd0dbc9` |
| Installer FileVersion | `6000.3.20.0` |
| Installer Authenticode | `Valid / Unity Technologies` |
| Installer use | `ONE INTERACTIVE RUN / EXIT CODE 0` |
| Install selection | `EDITOR CORE ONLY / NO OPTIONAL MODULE` |
| Fresh target | `C:\Program Files\Unity\Hub\Editor\6000.3.20f1` |
| Stable target observations | `49,756 FILES / 5,005 DIRECTORIES / 8,319,482,917 BYTES` twice identically |
| Installer cleanup | `REMOVED AFTER SUCCESS` |

새 download, 보안 경고 우회, Unity Hub launch·upgrade, 다른 Editor 제거,
PATH 변경 또는 optional module 설치는 수행하지 않았다.

### Required Unity.exe, registration and security

| Evidence | Verified result |
|---|---|
| Required binary | `C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe` |
| File status | `REGULAR FILE / NOT REPARSE / ONE HARDLINK PATH` |
| File bytes | `282,536` |
| FileVersion | `6000.3.20.13220457` |
| ProductVersion | `6000.3.20f1_c9ba695d4f07` |
| Unity.exe SHA-256 | `f025d925990cbef012bba6600c8463ee0c94cbab90ee10c4c2f93dd69f85120e` |
| Authenticode | `Valid / Unity Technologies SF` |
| Version/changeset | `6000.3.20f1 / c9ba695d4f07 / VERIFIED` |
| Registration | `PASS / ONE NON-CONFLICTING EXACT-VERSION ENTRY / FRESH TARGET / NO QUARANTINE REFERENCE` |
| Security audit | `PASS / READ-ONLY DEFENDER AND EVENT CHECK / NO BLOCK, REMOVAL OR QUARANTINE EVIDENCE` |

Fresh `Uninstall.exe`도 `NotSigned`였으며 실행하지 않았다. Registry 또는
Defender 설정은 수정하지 않았고 exclusion 추가, file restore 또는
보안 기능 비활성화도 수행하지 않았다. 기존 Editor `2021.3.45f1`,
`2022.3.62f1`, `2022.3.74f1`, `6000.3.2f1`의 Unity.exe SHA-256은
복구 전후 동일했고, fresh binary는 `6000.3.2f1`과 서로 달랐다.

### Two independent external batch startups

Disposable project와 log는 저장소 밖
`C:\Users\jhbjm\AppData\Local\Temp\break-notice-task0071-r3` 아래에서만
만들었고 성공 증거 hash를 캡처한 뒤 전부 제거했다.

| Run | Invocation role | Exit evidence | Exact version/changeset | Log SHA-256 |
|---|---|---|---|---|
| 1 | `-createProject` | `0 / LOG-CONFIRMED SUCCESSFUL BATCH EXIT` | `6000.3.20f1 / c9ba695d4f07` | `9c4361e5aa1e2ec969191c20515289418baa725618686309d90fbf44dea5c058` |
| 2 | `-projectPath` reopen | `0 / PROCESS EXIT` | `6000.3.20f1 / c9ba695d4f07` | `aaa89fd24ee77e57d88aec7f0e7dfc337d8e95aa2c9daaa1397f1426ca661197` |

두 Run 모두 exact fresh Unity.exe를 사용했고 successful batch exit,
License Error `0`, Activation Error `0`, Credential Prompt `0`, Fatal Error
`0`, Crash `0`, 같은 project의 동시 Editor `0`을 확인했다. 두 log의
초기 licensing access-token validation/update warning은 non-blocking이었고
이후 local entitlement resolution과 정상 종료가 확인됐다. 허용되는
결론은 오직
`EXISTING LOCAL LICENSE ACCEPTED FOR TWO POINT-IN-TIME BATCH STARTUPS`다.
Account, Email, Serial, License ID, Credential 또는 Machine Token은
수집·기록하지 않았으며 영구 Account·Subscription·License 유효성을
주장하지 않는다.

### Cleanup, repository protection and execution boundary

- Quarantine: `REMOVED AFTER SUCCESS`
- Disposable project, batch logs and R3 temp root: `REMOVED AFTER HASH CAPTURE`
- Signed Installer: `REMOVED AFTER SUCCESS`
- Fresh exact Editor, Unity Hub, existing local License state and every
  other Editor: `PRESERVED`
- Repository HEAD/origin before success-only synchronization:
  `b82bb382fc0439847d6497ee2fadd9c55781a9fa / 0 AHEAD / 0 BEHIND`
- Tracked/staged/conflict: `0 / 0 / 0`
- Existing untracked snapshot and Game placeholder digest: `UNCHANGED`
- BREAK NOTICE ProjectVersion, Package manifest/lock, `.meta`, C#, asmdef,
  Scene, Prefab, Test, Build and image change: `ABSENT`
- Package Materialization: `NOT STARTED / NOT AUTHORIZED`
- Package Resolution: `NOT VERIFIED`
- IMP-0001: `7 / 10 / EXECUTION BLOCKED`
- Unsatisfied preconditions: `7 / 8 / 9`
- Active Relay / execution SHA / Gate: `ABSENT / ABSENT / CLOSED`
- G4 / Graybox: `NOT STARTED / NOT AUTHORIZED` / `NOT STARTED`
- Next boundary: `OWNER IMP-0001 EXECUTION AUTHORIZATION RELAY REQUIRED`

## Intent versus Execution Evidence

**TASK-0065R SNAPSHOT / HISTORICAL — NOT LIVE STATUS**

| Evidence category | TASK-0065R snapshot |
|---|---|
| Intended Editor version | LOCKED |
| Intended Editor changeset | LOCKED |
| Intended Package versions | LOCKED |
| Editor installed | NOT VERIFIED / OWNER-DEFERRED |
| Unity Hub modified | NO / NOT AUTHORIZED |
| License verified | NO / NOT AUTHORIZED |
| Batch mode verified | NO / NOT AUTHORIZED |
| Disposable project verified | NO / NOT AUTHORIZED |
| ProjectVersion materialized | NO |
| Package manifest materialized | NO |
| Package lock materialized | NO |
| Package resolution verified | NO |
| BREAK NOTICE Unity project opened | NO |
| Tests executed | NO / NOT AUTHORIZED |
| Implementation-ready | NO |

`LOCKED INTENT`는 installed, resolved, licensed, tested, verified 또는
implementation-ready 증거가 아니다. 이 문서는 어떤 실행 준비
상태도 주장하지 않는다.

## Historical TASK-0064 Stop Result

- Task: `TASK-0064`
- Result: `STOP CONDITION CORRECTLY APPLIED`
- Historical cause: exact Editor `6000.3.20f1` was not installed at the
  time of the check
- Repository changes: `0`
- Commit: `NONE`
- Push: `NONE`
- Relay: `EAR-IMP-0001-001 / EXPIRED / UNUSED`
- Gate: `CLOSED / NEVER ACTIVATED`

위 머신 관찰은 TASK-0064 당시의 역사적 증거일 뿐이다. TASK-0065R은
현재 Editor 설치 목록, Unity Hub 또는 라이선스 상태를 다시 검사하지
않았다.

## Withdrawn TASK-0065 Provisioning Authority

이전의
`TASK-0065 — Provision and Verify Unity Editor 6000.3.20f1 Execution Environment`
권위는 Human Owner에 의해 실행 전에
`WITHDRAWN / SUPERSEDED BEFORE EXECUTION`됐다.

설치 파일 다운로드, Editor·Hub 설치나 변경, 라이선스 확인,
batch mode, 임시 프로젝트, system-level provisioning과 IMP-0001
실행은 해당 권위로 수행되지 않았다. 현재 승인 상태는 DESIGN_STATUS를
따른다.

## TASK-0065R HISTORICAL PRECONDITION SNAPSHOT

| # | Mandatory precondition | TASK-0065R snapshot |
|---:|---|---|
| 1 | Turnaround generated | SATISFIED |
| 2 | Binary Review PASS | SATISFIED |
| 3 | Owner-approved and registered | SATISFIED |
| 4 | Design Freeze `32 / 32` | SATISFIED |
| 5 | Pre-G4 Full Design Lock | SATISFIED |
| 6 | Final Re-Audit readiness result | SATISFIED |
| 7 | Active Execution Authorization Relay | NOT SATISFIED — `EAR-IMP-0001-001 EXPIRED / UNUSED` |
| 8 | Limited Gate exact open state | NOT SATISFIED — `GATE CLOSED` |
| 9 | Active Relay-supplied execution SHA | NOT SATISFIED — `NO ACTIVE RELAY OR SHA` |
| 10 | Exact Editor installed and verified | NOT SATISFIED — `OWNER-DEFERRED / NOT VERIFIED` |

TASK-0065R가 기록한 point-in-time progress는 `6 / 10`이다. 이는
현재 precondition 충족 상태를 독립적으로 주장하지 않는다.

## Historical Execution Relay and Gate Evidence

다음은 TASK-0064 및 TASK-0065R 당시의 immutable point-in-time
evidence이며 live authority가 아니다.

- `EAR-IMP-0001-001`: `EXPIRED / UNUSED`
- Active Execution Authorization Relay: `ABSENT`
- New Relay authorization: `NOT AUTHORIZED`
- Concrete execution SHA: `ABSENT`
- Implementation Gate: `CLOSED`
- IMP-0001: `EXECUTION BLOCKED / TOOLCHAIN PROVISIONING OWNER-DEFERRED`
- G4: `NOT PASSED / NOT STARTED / NOT AUTHORIZED`
- G5: `NOT PASSED / REQUIRES G4 EVIDENCE`
- Graybox: `NOT STARTED`

Standing Owner Delegation, GPT Pro의 PASS/APPROVE, Pre-G4 Full Design
Lock, 과거 Relay 또는 저장소 상태만으로 provisioning, 새 Relay,
Gate 개방이나 구현 권한을 추론할 수 없다.

현재 provisioning과 execution readiness는
[`docs/00_project/DESIGN_STATUS.md`](../00_project/DESIGN_STATUS.md)와
미래의 명시적 bounded Execution Authorization Relay가 각각 정한
범위에서만 판단한다. 미래 Relay는 모든 precondition을 독립적으로
재계산해야 한다.

## Current Claim Boundary after TASK-0071-R3

현재 exact Unity Editor와 changeset, fresh Unity.exe, 두 외부 disposable
project startup 및 existing local license acceptance의 point-in-time
증거는 주장할 수 있다. 다음은 계속 주장해서는 안 된다.

- 영구 Account, Subscription 또는 License 유효성;
- ProjectVersion, Package manifest 또는 Package lock의 materialization;
- Package resolution;
- BREAK NOTICE Unity project가 열렸거나 생성됐다는 주장;
- 테스트 실행 또는 implementation-ready 상태;
- IMP-0001 실행, G4·Graybox 시작;
- 활성 Relay, concrete execution SHA 또는 열린 Gate.

## Change-Control Boundary

정확한 intended Editor, changeset 또는 Package 버전을 바꾸거나 AI
Navigation을 Prototype direct Package baseline에 추가하려면
`docs/00_project/CHANGE_CONTROL.md`에 따른 영향 분석과 Human Owner
승인이 먼저 필요하다. 실행 결과는 이 intent 기록에 소급해
추론하지 않고 별도 증거로 기록한다.

## Current Status Source

현재 provisioning, Gate, Relay, IMP-0001 readiness와 다음 권한
경계는
[`docs/00_project/DESIGN_STATUS.md`](../00_project/DESIGN_STATUS.md)만
유지한다. 이 intent/historical evidence 문서는 독립적인 현재 상태나
다음 작업을 지정하지 않는다.
