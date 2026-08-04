# IMP-0002 — Runtime Identity and Definition/State Separation Foundation

Status: `OWNER-APPROVED TICKET DECOMPOSITION / EXECUTION BLOCKED`

Future Gate:
`OPEN — IMP-0002 ONLY / RUNTIME IDENTITY AND STATE FOUNDATION`

Future Relay: `REQUIRED / NOT ISSUED`

Future Execution SHA: `SUPPLIED ONLY BY A SEPARATE FUTURE RELAY`

## Authority and Current Boundary

이 문서는 future IMP-0002 execution을 위한 static ticket decomposition이다.
TASK-0073은 ticket과 future Gate instance를 정의할 뿐 Gate를 열거나
Execution Authorization Relay를 발급하지 않는다. 현재 Implementation
Gate는 `CLOSED`, active Relay는 `ABSENT`, IMP-0002 execution은
`NOT STARTED / NOT AUTHORIZED`다.

ADR-0019는 완료된 IMP-0001의 역사적·의미론적 권위다. post-Bootstrap
per-ticket Gate 권위는 CR-0002와 ADR-0022를 따른다. 미래 실행은 별도의
Owner-approved Relay가 exact ticket ID, 이미 push된 immutable SHA,
branch `main`, clean equality, exact scope, validation과 one-shot
expiration을 제공할 때만 가능하다.

## Future Objective

stable content-reference identity, transient runtime-entity identity,
encounter-generation identity와 runtime registry ownership을 분리하는
가장 작은 engine-independent C# foundation을 구현한다.

Gameplay는 구현하지 않는다.

## Exact Future Runtime Assembly

Future assembly: `BreakNotice.Core`

Required asmdef behavior:

- `noEngineReferences: true`
- UnityEngine dependency 없음
- UnityEditor dependency 없음
- external Package dependency 없음
- static global registry 없음
- service locator 없음
- event bus 없음

## Exact Future Runtime Types

IMP-0002의 future execution은 아래 정확히 여섯 Runtime type만 만들 수
있다.

### 1. `ContentId`

- `readonly struct`
- 하나의 non-empty string을 감싼다.
- null, empty와 whitespace-only 값을 거부한다.
- ordinal equality를 사용한다.
- display name을 포함하지 않는다.
- runtime state를 포함하지 않는다.
- implicit conversion을 제공하지 않는다.

### 2. `RuntimeEntityId`

- `readonly struct`
- non-zero `ulong`을 감싼다.
- value equality를 사용한다.
- zero는 invalid다.

### 3. `EncounterGenerationId`

- `readonly struct`
- non-zero `uint`를 감싼다.
- value equality를 사용한다.
- zero는 invalid다.

### 4. `RuntimeIdentity`

정확히 다음 세 값만 포함한다.

- `ContentId`
- `RuntimeEntityId`
- `EncounterGenerationId`

IMP-0002에는 Origin ID 또는 Result key를 추가하지 않는다.

### 5. `RuntimeIdentityAllocator`

- sealed, instance-owned class다.
- construction 시 고정된 `EncounterGenerationId`를 가진다.
- `RuntimeEntityId`를 `1`부터 monotonically 할당한다.
- random ID를 사용하지 않는다.
- GUID를 생성하지 않는다.
- static mutable counter가 없다.
- overflow 전에 throw한다.
- construction 뒤 generation을 바꿀 수 없다.

### 6. `RuntimeEntityRegistry`

- sealed, instance-owned class다.
- 고정된 `EncounterGenerationId`를 가진다.
- `RuntimeIdentity`를 등록한다.
- zero ID를 거부한다.
- duplicate `RuntimeEntityId`를 거부한다.
- mismatched `EncounterGenerationId`를 거부한다.
- `TryGet`을 지원한다.
- exact requested runtime ID만 remove한다.
- complete clear를 지원한다.
- read-only count를 노출한다.
- Unity object를 포함하지 않는다.
- global static state를 포함하지 않는다.

## Explicitly Deferred from IMP-0002

다음을 구현하지 않는다.

- Causal Origin ID
- Result key
- interaction state
- Carry
- Throw
- Reset execution
- player input
- locomotion
- camera
- MonoBehaviour
- ScriptableObject asset
- Scene
- Prefab
- gameplay object
- save schema
- serialization
- network ID
- editor tooling
- logging overlay

## Exact Future File Scope

### Runtime

- `Game/Assets/_Project/Code/Runtime/Core/BreakNotice.Core.asmdef`
- `Game/Assets/_Project/Code/Runtime/Core/Identity/ContentId.cs`
- `Game/Assets/_Project/Code/Runtime/Core/Identity/RuntimeEntityId.cs`
- `Game/Assets/_Project/Code/Runtime/Core/Identity/EncounterGenerationId.cs`
- `Game/Assets/_Project/Code/Runtime/Core/Identity/RuntimeIdentity.cs`
- `Game/Assets/_Project/Code/Runtime/Core/Identity/RuntimeIdentityAllocator.cs`
- `Game/Assets/_Project/Code/Runtime/Core/Identity/RuntimeEntityRegistry.cs`

### EditMode Tests

- `Game/Assets/_Project/Code/Tests/EditMode/BreakNotice.Core.EditModeTests.asmdef`
- `Game/Assets/_Project/Code/Tests/EditMode/RuntimeIdentityTests.cs`
- `Game/Assets/_Project/Code/Tests/EditMode/RuntimeEntityRegistryTests.cs`

### Generated Metadata

Unity가 생성한 아래 metadata만 허용한다.

- 위 exact Runtime file에 각각 대응하는 `.meta`
- 위 exact EditMode Test file에 각각 대응하는 `.meta`
- 새 `Game/Assets/_Project/Code/Runtime/Core/Identity` directory에 대응하는
  `Identity.meta`

그 밖의 generated metadata는 IMP-0002 scope가 아니다.

### Future Status Synchronization

- `Game/README.md`
- `CHANGELOG.md`
- `MANIFEST.json`
- `docs/00_project/DESIGN_STATUS.md`

위 목록 밖의 future file은 IMP-0002에서 허용하지 않는다.

## Exact Future EditMode Test Contract

Future execution은 아래 정확히 열여덟 behavior를 EditMode에서 검증해야
한다.

1. ContentId rejects null.
2. ContentId rejects empty.
3. ContentId rejects whitespace-only.
4. ContentId uses ordinal equality.
5. RuntimeEntityId rejects zero.
6. EncounterGenerationId rejects zero.
7. Allocator starts at RuntimeEntityId `1`.
8. Allocator produces monotonically increasing unique IDs.
9. Allocator preserves its fixed generation.
10. Registry accepts a matching-generation identity.
11. Registry rejects duplicate RuntimeEntityId.
12. Registry rejects mismatched generation.
13. Registry `TryGet` returns the exact registered identity.
14. Registry removes only the exact requested runtime ID.
15. Registry clear returns count to zero.
16. Runtime assembly contains no mutable static field.
17. Runtime assembly has no UnityEngine or UnityEditor reference.
18. Tests run twice without order dependence or retained static state.

No PlayMode Test is required by IMP-0002.

## Future Validation and Stop Boundary

Future Relay은 위 exact file scope와 열여덟 EditMode obligations를
독립적으로 반복해야 한다. 다음 중 하나라도 발생하면 commit 없이
중지하고 Relay와 Gate를 만료한다.

- baseline, branch 또는 local/remote clean equality 불일치
- Gate가 exact future Gate와 다름
- ticket 밖 type 또는 file 필요
- UnityEngine, UnityEditor 또는 external Package dependency 필요
- gameplay, Scene, Prefab, PlayMode Test 또는 external asset 필요
- Origin ID, Result key, service locator, event bus 또는 global mutable
  state 필요
- validation 실패 또는 scope expansion

## Current State After Ticket Lock

- IMP-0001: `COMPLETE`
- TASK-0072 Completion Review: `APPROVE WITH NON-BLOCKING NOTES`
- IMP-0001 Disposition: `APPROVE / COMPLETE`
- Post-IMP-0001 Design Authority Review:
  `APPROVE`
- CR-0002: `APPROVED / APPLIED`
- ADR-0022: `ACCEPTED`
- IMP-0002:
  `OWNER-APPROVED TICKET DECOMPOSITION / EXECUTION BLOCKED`
- G4-A Foundation Entry: `OWNER-APPROVED / RELAY PENDING`
- G4-A Execution: `NOT STARTED / NOT AUTHORIZED`
- G4: `NOT PASSED / NOT STARTED / NOT AUTHORIZED`
- Graybox: `NOT STARTED`
- Implementation Gate: `CLOSED`
- Active Relay: `ABSENT`
- External AI Asset: `NOT AUTHORIZED`
- Next Authorization Boundary:
  `OWNER IMP-0002 EXECUTION AUTHORIZATION RELAY REQUIRED`
