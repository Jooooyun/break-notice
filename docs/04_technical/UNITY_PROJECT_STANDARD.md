# Unity Project Standard

> **OWNER-APPROVED G3 DOCUMENTATION LOCK — NO UNITY PROJECT EXISTS**
>
> Authority: `docs/04_technical/G3_TECHNICAL_BASELINE.md` —
> `## Exact Editor and Package Lock`
>
> 현재 `Game/**`에는 보존용 placeholder만 있으며 generated Unity
> project, package manifest/lock, `ProjectVersion.txt`, Scene, Prefab,
> C#, asmdef와 Test가 없다. Bootstrap은 승인되지 않았다.

## Exact Documentation Lock

| Identifier | Exact lock |
|---|---|
| Unity Editor | `6000.3.20f1` |
| `com.unity.render-pipelines.universal` | `17.3.0` |
| `com.unity.inputsystem` | `1.20.0` |
| `com.unity.cinemachine` | `3.1.7` |
| `com.unity.test-framework` | `1.6.0` |

floating version은 금지한다. URP와 Test Framework는 Editor-matched
Core Package, Input System과 Cinemachine은 separately installed
Released Package다. AI Navigation은 Prototype baseline 밖이다.
transitive dependency는 실제 generated package lock에서만 기록한다.

## Planned Structure, Not Existing State

```text
Game/Assets/_Project/
├─ Art/
├─ Audio/
├─ Code/
│  ├─ Runtime/
│  ├─ Editor/
│  └─ Tests/
├─ Data/
├─ Prefabs/
├─ Scenes/
└─ Settings/
```

이는 승인된 파일 생성 목록이 아니다. G3 passage, remaining Design
Freeze work와 final pre-implementation review 뒤의 별도 ticket만
구체 folder와 file을 만들 수 있다.

## Prototype Module Boundary

first wave에는 identity/state foundation, input/locomotion/camera,
interaction/ownership, physical object lifecycle, fixed Ally supply,
Cargo Strap, impact/causal room state, Reset/fault와 observability만
포함한다.

Progression, Momentum, HeavyAction, Policy, Relay Position, Breakable,
Structural, Upgrade, Incident Report, persistent save, LGT_01과 FIN_01은
Prototype module이 아니다.

## Naming Candidates

- C# type: PascalCase
- private field: `_camelCase`
- local/parameter: camelCase
- data asset: `Type_Name_Variant`
- Prefab: `PF_Category_Name`
- Scene: `SCN_XX_Name`

이 naming은 future implementation convention 후보이며 file creation
authority가 아니다.

## Assembly and Scene Boundary

Runtime/Editor/Test assembly 분리와 Bootstrap/Prototype Scene 구조는
future ticket에서 exact dependency를 검토한다. 과도한 asmdef 분할과
미승인 Vertical Slice module의 선행 생성을 피한다.

## Package Rule

approved minimal lock 밖의 package를 추가하려면 Change Request와
다음을 기록한다.

1. 필요한 기능;
2. 대안;
3. 유지보수 상태;
4. build 영향;
5. license;
6. 제거 계획.

Addressables, DOTS/Entities, Netcode, third-party rope/behavior-tree/save,
generic service locator/DI/event bus, telemetry와 analytics는 현재
승인되지 않았다.
