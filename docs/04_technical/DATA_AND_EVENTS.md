# Data and Event Contracts

> **OWNER-APPROVED G3 SEMANTIC BASELINE — NAMES AND PAYLOADS PROVISIONAL**
>
> Authority: `docs/04_technical/G3_TECHNICAL_BASELINE.md` —
> `## Definition Data and Runtime State Ownership` and
> `## Ordered Request, Fact, Result, and Presentation Flow`

## Identity Categories

- stable content-reference identity;
- transient runtime-entity identity;
- encounter 또는 Reset generation identity;
- Causal Origin identity;
- Result uniqueness identity.

표시 이름과 stable content reference는 분리한다. content ID는 runtime
instance ID가 아니고, runtime instance ID는 Origin ID가 아니며,
Origin ID는 Result key가 아니다. GUID 형식, integer width와
serialization은 미정이다.

## Definition and Runtime State

shared definition data에는 runtime state를 저장하지 않는다. Carry,
Strap, Relay reservation, physical lifecycle, Origin/Result와 encounter
room state는 각각 single owner를 가진다. Persistent save는 Prototype
runtime physics를 소유하지 않는다.

## Ordered Semantic Topology

`Request` → `Validation` → `Ownership Commit` → `Physical Action or Physical Fact`
→ `Normalized Domain Fact` → `Domain Result` → `Presentation`

Reset은 다른 lifecycle 처리를 막는 exclusive barrier다.

## Semantic Event Families

### Request and Validation

- interaction request/rejection;
- Strap release request;
- Reset requested;
- later Policy와 Momentum request.

### Ownership and Physical Action

- ownership commit/release;
- Throw commit/physical release;
- Queued Throw armed/cleared;
- Relay reservation, pre-release abort/physical release;
- Strap attach/redirect/release completed.

### Normalized Facts and Domain Results

- normalized impact;
- Causal Origin creation;
- carrier transfer;
- meaningful Result record;
- Origin termination;
- room-state change;
- fault detection/recovery;
- Reset started/completed.

### Presentation

Presentation은 normalized fact와 domain result를 소비한다. gameplay
state를 되돌려 변경하지 않는다.

## Delivery Properties

각 family에는 explicit owner와 deterministic ordering이 필요하다.
repeated delivery 가능성이 있으면 idempotency와 duplicate suppression을
적용한다. reentrancy boundary를 두며 raw collision은 unrelated system을
직접 변경하지 않는다.

## Causal Result Contract

Origin은 initiating carrier, attributed carrier, parent 하나, depth와
provenance를 가진다. Result family는 `TARGET`, `OBJECT`, `ROOM_STATE`,
`STRUCTURAL`이며 같은 Origin의 compound target/result-family가
unique하다. branch는 Origin을 공유할 수 있지만 merge하지 않는다.
ambiguous multi-Origin result는 reward가 없고 terminated Origin은
reopen되지 않는다.

구체 event name, payload field, collection type와 transport는 미정이다.

## Save Data Boundary

Prototype은 persistent gameplay save와 full Rigidbody-world persistence를
사용하지 않는다. later save가 승인될 때만 별도 schema version과
serialization을 정의한다.
