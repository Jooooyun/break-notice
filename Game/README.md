# Unity Project Bootstrap

이 폴더는 `TASK-0072 / EAR-IMP-0001-002`에서 exact-version Empty Unity
Bootstrap으로 materialize됐다. 실행 기준선은
`9234bbed61a284b590f9b73ef44c8d0139e374e1`, Editor는 Unity
`6000.3.20f1` / changeset `c9ba695d4f07`이다.

## Resolved Direct Package Baseline

| Identifier | Resolved version |
|---|---|
| `com.unity.render-pipelines.universal` | `17.3.0` |
| `com.unity.inputsystem` | `1.20.0` |
| `com.unity.cinemachine` | `3.1.7` |
| `com.unity.test-framework` | `1.6.0` |

`Packages/manifest.json`에는 위 네 direct dependency만 존재하며,
`Packages/packages-lock.json`은 exact Editor로 두 번 연속 열어 동일한
내용과 SHA-256을 유지했다. AI Navigation은 direct/transitive package
baseline에 없고 Prototype에서 계속 `DEFERRED`다.

Unity가 빈 URP 프로젝트 초기화 중 생성한
`Assets/DefaultVolumeProfile.asset`과
`Assets/UniversalRenderPipelineGlobalSettings.asset` 및 각 `.meta`는
Human Owner가 bootstrap infrastructure로 명시적으로 허용했다. 이들은
gameplay 또는 external production asset이 아니다.

## Runtime Identity Foundation

`TASK-0074 / EAR-IMP-0002-001`에서 engine-independent
`BreakNotice.Core` Runtime Identity Foundation을 구현했다. Runtime
assembly는 `UnityEngine` 또는 `UnityEditor`를 참조하지 않으며 다음 여섯
타입만 공개한다.

- `ContentId`
- `RuntimeEntityId`
- `EncounterGenerationId`
- `RuntimeIdentity`
- `RuntimeIdentityAllocator`
- `RuntimeEntityRegistry`

정확히 `17`개의 EditMode Test를 서로 독립적인 Unity Editor 프로세스에서
두 번 실행했고 두 실행 모두 `17 / 17 PASS`였다. PlayMode Test는 없다.

## Locomotion, Camera and Logical Input Graybox Foundation

`TASK-0075 / EAR-IMP-0003-001`에서 `BreakNotice.Gameplay`의 정확히 여덟
Prototype logical action과 Input System adapter, grounded third-person
locomotion, `BreakNotice.Presentation`의 normal/Aim camera rig, 두 Graybox
Prefab과 primitive-only `G4A_LocomotionCameraGraybox` Scene을 구현했다.

정확히 `6`개의 EditMode Test와 `4`개의 PlayMode Test를 독립 cycle 두
번 실행해 각 cycle `10 / 10 PASS`를 확인했다. exact Unity Editor의 수동
smoke에서도 Move, Look, Aim, wide/narrow traversal, camera occlusion
recovery, actor/object readability와 금지 action 무효를 확인했고 종료 후
Console log/warning/error는 `0 / 0 / 0`이었다.

## Scope Boundary

현재 구현은 Runtime Identity Foundation과 TASK-0075의 locomotion,
camera, logical-input Graybox foundation에 한정된다. Interaction, Carry,
Throw, Cargo Strap, ally/enemy behavior, combat, production/external asset,
Build와 formal G4 evidence는 없다. 기존 Empty Bootstrap, exact package
lock과 URP Bootstrap asset은 보존됐다. IMP-0003은 `COMPLETE / VERIFIED`,
G4는 `NOT PASSED`, formal evidence는 `NOT COLLECTED`, Implementation Gate는
`CLOSED`, active Relay는 `ABSENT`다. 다음 작업에는 별도의 Design
Authority 검토와 ticket-specific one-shot Relay가 필요하다.
