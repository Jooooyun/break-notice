# Unity Project Placeholder

이 폴더는 향후 Unity 프로젝트가 생성될 예정 위치다. 현재 Unity
프로젝트는 존재하지 않으며 generated Unity file,
`ProjectSettings/ProjectVersion.txt`, package manifest와 package lock도
존재하지 않는다.

## Approved Documentation Locks

| Identifier | Current approved documentation lock |
|---|---|
| Unity Editor | `6000.3.20f1` |
| `com.unity.render-pipelines.universal` | `17.3.0` |
| `com.unity.inputsystem` | `1.20.0` |
| `com.unity.cinemachine` | `3.1.7` |
| `com.unity.test-framework` | `1.6.0` |

Unity Editor는 Editor Core다. URP와 Test Framework는 Editor-matched
Core Package, Input System과 Cinemachine은 separately installed
Released Package다. 위 값은 설치 결과가 아닌 문서 기준 lock이며
floating version은 허용하지 않는다. 추가 package는 승인되지 않았다.

AI Navigation은 Prototype Package Baseline 밖이며 `DEFERRED`다. 별도
승인된 Relay Position implementation wave에서만 다시 검토할 수 있다.

Bootstrap 실행, Unity project 생성, package 설치, `Assets/_Project`
적용, asmdef·test 구조 또는 Prototype Scene 생성은 현재 승인되지
않았다. 향후 Bootstrap에는 별도의 명시적 승인 Task가 필요하다.
