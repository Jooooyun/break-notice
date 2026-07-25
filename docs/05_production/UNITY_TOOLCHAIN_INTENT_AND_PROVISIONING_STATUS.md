# Unity Toolchain Intent and Provisioning Status

> **OWNER-APPROVED INTENDED TOOLCHAIN / PROVISIONING DEFERRED**
>
> Date: `2026-07-25`
>
> Authority: Human Owner direct instruction superseding the earlier
> environment-provisioning form of `TASK-0065`.

## Authority and Scope

이 문서는 향후 별도 승인된 Unity Bootstrap이 사용해야 할 정확한
Editor와 direct Package 의도를 기록한다. 이 기록은 문서 권위만
갱신하며 설치, Unity Hub 변경, 라이선스 확인, Editor 실행, Package
materialization, 테스트, 구현 또는 Gate 개방을 수행하거나 승인하지
않는다.

Pre-G4 Full Design Lock의 frozen-source 내용은 변경하지 않는다.
그 문서와 과거 감사 기록의 다음 작업 문구는 작성 당시의
point-in-time evidence이며, 현재 운영 권위는 이 문서와 최신 Human
Owner 직접 지시를 따른다.

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

## Intent versus Execution Evidence

| Evidence category | Current status |
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
실행은 수행되지 않았고 현재도 승인되지 않았다.

## IMP-0001 Precondition Status

| # | Mandatory precondition | Persistent status |
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

Persistent progress is `6 / 10`. 만료된 TASK-0064 Relay는 조건 7, 8
또는 9를 지속적으로 충족하지 않는다.

## Execution Relay and Gate Boundary

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

## Prohibited Current Claims

현재 다음을 주장해서는 안 된다.

- Unity Editor 또는 정확한 changeset이 설치·검증됐다는 주장;
- Unity Hub 또는 라이선스가 확인됐다는 주장;
- batch mode나 임시 프로젝트가 성공했다는 주장;
- ProjectVersion, Package manifest 또는 Package lock이 생성됐다는 주장;
- Package가 resolve됐다는 주장;
- BREAK NOTICE Unity project가 열렸다는 주장;
- 테스트가 실행됐거나 구현 준비가 완료됐다는 주장;
- 활성 Relay, concrete execution SHA 또는 열린 Gate가 있다는 주장.

## Future Explicit Owner Authorization Required

Unity 환경 provisioning은 향후 Human Owner가 직접 명시적으로
지시하기 전까지 `OWNER-DEFERRED`다. 그 지시가 있더라도 실제 실행은
새 bounded task, 현재 기준선, 정확한 범위, 검증, Git 권한과 필요한
Gate·Relay 조건을 별도로 충족해야 한다.

## Change-Control Boundary

정확한 intended Editor, changeset 또는 Package 버전을 바꾸거나 AI
Navigation을 Prototype direct Package baseline에 추가하려면
`docs/00_project/CHANGE_CONTROL.md`에 따른 영향 분석과 Human Owner
승인이 먼저 필요하다. 실행 결과는 이 intent 기록에 소급해
추론하지 않고 별도 증거로 기록한다.

## Current Next Authorization Boundary

`OWNER EXPLICIT UNITY TOOLCHAIN PROVISIONING INSTRUCTION REQUIRED`
