# Unity Project Standard

## 예정 구조

```text
Game/Assets/_Project/
├─ Art/
│  ├─ Characters/
│  ├─ Environment/
│  ├─ Props/
│  ├─ Materials/
│  ├─ VFX/
│  └─ UI/
├─ Audio/
├─ Code/
│  ├─ Runtime/
│  │  ├─ Core/
│  │  ├─ Player/
│  │  ├─ Allies/
│  │  ├─ Enemies/
│  │  ├─ Physics/
│  │  ├─ Progression/
│  │  └─ Presentation/
│  ├─ Editor/
│  └─ Tests/
│     ├─ EditMode/
│     └─ PlayMode/
├─ Data/
├─ Prefabs/
├─ Scenes/
│  ├─ 00_Bootstrap/
│  ├─ 10_Prototype/
│  └─ 20_VerticalSlice/
└─ Settings/
```

## Naming

- C# type: PascalCase
- private field: `_camelCase`
- local/parameter: camelCase
- ScriptableObject asset: `Type_Name_Variant`
- Prefab: `PF_Category_Name`
- Scene: `SCN_XX_Name`
- Material: `MAT_Name`
- VFX: `VFX_Name`
- Audio: `SFX_Category_Name`

## Assembly Definitions

구현 시작 시 Runtime, Editor, Tests를 asmdef로 분리한다. 시스템별 과도한 asmdef 분할은 피한다.

## Scene Rule

- Bootstrap은 서비스와 설정만
- Prototype Scene은 한 방과 Reset
- 버티컬 슬라이스 방은 Additive 또는 명시적 전환 전략을 기술 설계 후 확정

## Package Rule

패키지 추가 전:

1. 필요 기능
2. 대안
3. 유지보수 상태
4. 빌드 영향
5. 라이선스
6. 제거 계획

을 ADR에 기록한다.
