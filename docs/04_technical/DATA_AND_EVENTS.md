# Data and Event Contracts

## Stable IDs

모든 데이터 자산은 표시 이름과 별도로 안정적인 ID를 가진다.

- `OBJ-*`
- `ENY-*`
- `ALLY-*`
- `UPG-*`
- `ENC-*`
- `SFX-*`
- `VFX-*`

## Core Event Shapes

### ImpactEvent

- SourceEntityId
- TargetEntityId
- ContactPoint
- ContactNormal
- RelativeVelocity
- ImpulseMagnitude
- WeightClass
- MaterialType
- ChainId
- ChainDepth
- Timestamp

### PassEvent

- AllyId
- ObjectId
- DeliveryMode
- OriginPosition
- IntendedCatchPosition
- ReleaseTime

### CatchEvent

- ObjectId
- Result: Normal / Perfect / Miss
- TimingOffset
- WasQueuedThrow

### BreakEvent

- BreakableId
- PreviousState
- NewState
- ChainId
- SpawnedLargePieceIds
- OpenedRouteId

### MomentumEvent

- ChainId
- Reason
- BaseValue
- Modifiers
- FinalValue

## Save Data

Prototype에는 영구 저장이 필수가 아니다. Vertical Slice 저장은 옵션·진행 설정과 Run Seed 정도로 제한한다. 물리 오브젝트 전체 상태를 저장하지 않는다.
