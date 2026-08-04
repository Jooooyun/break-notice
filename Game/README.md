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

## Scope Boundary

현재 프로젝트에는 gameplay C#, asmdef, Test, Scene, Prefab, sample
content, imported external asset, Build 또는 Graybox가 없다. 기존
`Assets/_Project` 폴더 골격과 `.gitkeep`은 보존됐다. G4는
`NOT PASSED / NOT STARTED / NOT AUTHORIZED`, Graybox는 `NOT STARTED`,
Implementation Gate는 `CLOSED`다. 이후 작업은 별도의 bounded 권한이
필요하다.
