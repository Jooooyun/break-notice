# Technical Design

## 1. 기술 기준

- Engine: Unity 6
- Rendering: URP
- Physics: Built-in 3D PhysX / GameObject
- Language: C#
- Input: Unity Input System
- Camera: Cinemachine
- AI Navigation: Unity AI Navigation/NavMesh, 제한된 Relay Position 이동
- Data: ScriptableObject definitions + 런타임 인스턴스 상태
- Testing: Unity Test Framework, EditMode/PlayMode
- Target: Windows PC, 1080p/60 FPS

정확한 Unity Editor 패치 버전과 패키지 버전은 Unity 프로젝트 Bootstrap 시 ADR로 잠근다.

## 2. Architecture Layers

```text
Presentation
- Camera, Animation, VFX, Audio, UI

Gameplay
- Player Interaction, Throw, Strap, Ally Relay, Enemy Stability

Simulation
- Physics Objects, Impacts, Breakables, Causal Chain

Progression
- Momentum, Upgrades, Encounter Flow, Run State

Infrastructure
- Input, Save, Pooling, Logging, Debug Tools
```

## 3. 핵심 시스템

- `GameFlowController`
- `EncounterDirector`
- `PlayerMotor`
- `PlayerInteractionController`
- `CarryController`
- `ThrowController`
- `CargoStrapController`
- `AllyRelayController`
- `CatchController`
- `PhysicsObject`
- `ImpactResolver`
- `StabilityController`
- `BreakableObject`
- `CausalChainService`
- `MomentumService`
- `FeedbackDirector`
- `ObjectPoolService`

각 클래스명은 구현 전 상세 설계에서 조정 가능하지만 책임 분리는 유지한다.

## 4. 물리 원칙

- 모든 물체를 상시 Dynamic으로 두지 않음
- 빠른 투척물에만 CCD 선택 적용
- 복잡한 MeshCollider 대신 Compound primitive collider
- 물리 이벤트에서 직접 모든 게임 시스템을 호출하지 않고 `ImpactEvent`로 정규화
- 파괴는 사전 제작 프리팹 교체
- 작은 파편은 VFX
- 속도·각속도 안전 상한

## 5. 데이터 원칙

ScriptableObject 정의 예:

- ObjectDefinition
- EnemyDefinition
- AllyDefinition
- UpgradeDefinition
- ImpactProfile
- BreakableDefinition
- EncounterDefinition

런타임 상태는 MonoBehaviour/POCO 인스턴스가 관리한다.

## 6. 디버깅

- 현재 Target과 Interaction Priority 표시
- Rigidbody sleep/awake 시각화
- Chain ID/depth/TTL 표시
- Momentum 보상 로그
- Relay Position과 패스 궤적 표시
- 파괴 예산 카운터
- Reset 누수 검사

## 7. 구현 순서

1. 물리 오브젝트와 기본 투척
2. Stability와 ImpactResolver
3. Causal Chain
4. Momentum
5. 동료 Relay와 Catch
6. Cargo Strap Redirect
7. Breakable/Structural
8. 피드백과 카메라
9. 업그레이드
10. Incident Report
