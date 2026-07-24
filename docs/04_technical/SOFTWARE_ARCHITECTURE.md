# Software Architecture

> **OWNER-APPROVED G3 DIRECTION — IMPLEMENTATION NAMES REMAIN PROVISIONAL**
>
> Authority: `docs/04_technical/G3_TECHNICAL_BASELINE.md` —
> `## Architecture and Dependency Direction`

## Dependency Rule

Composition root가 dependency를 명시적으로 구성한다. Gameplay과
Simulation은 UI, Camera, VFX 또는 Audio 구현을 참조하지 않는다.
Presentation adapter는 domain fact를 소비할 뿐 gameplay state를
변경하지 않는다.

금지:

- generic service locator 또는 dependency-injection framework;
- global mutable static registry;
- primary global event bus;
- raw collision callback의 cross-system mutation;
- Presentation-to-Gameplay backflow;
- optional system의 Prototype dependency graph 침투.

## Responsibility Domains

### Composition and Encounter Bootstrap

encounter dependency, lifecycle root와 authored baseline을 조립한다.
구체 class 이름은 미정이다.

### Logical Input Adapter

platform input을 G2 logical intent로 변환한다. arbitration이나 state
mutation을 소유하지 않는다.

### Player Locomotion and Camera Adapter

승인된 locomotion과 gameplay camera adapter만 소유한다. Evade와
Vault는 active Prototype responsibility가 아니다.

### Interaction and Ownership

G2 context/state contract에 따라 request를 검증하고 Carry/Throw의
singular ownership을 commit한다. fixed Catch/Strap/Scoop priority와
settled Quick/Charged mode를 가정하지 않는다.

### Physical Object Lifecycle

activation, physical release, carrier transfer와 stable physical
continuity를 소유한다.

### Ally Supply and Reservation

room-local physical pool, singular reservation, pre-release abort와
physical release를 소유한다. generic ally inventory, first-wave Relay
Position과 Suppressed를 소유하지 않는다.

### Cargo Strap

valid Anchor/eligible Medium 검사, attach, bounded redirect와 explicit
release를 소유한다. automatic Safety Release는 승인되지 않았다.

### Impact Normalization

raw collision을 normalized domain fact로 바꾼다. Momentum, UI 또는
unrelated state를 직접 변경하지 않는다.

### Causal Attribution and Room State

Origin, carrier, Result uniqueness와 one-room state flag를 소유한다.
Momentum은 later integration이다.

### Reset and Fault Recovery

exclusive Reset barrier, authored baseline restore, fault quarantine와
audited Scene-reload fallback을 소유한다.

### Presentation Adapters

domain fact를 camera/VFX/audio/UI로 변환한다. domain 결과를 만들지
않는다.

### Test and Observability

EditMode/PlayMode/Manual owner mapping과 required trace/counter를
소유한다. 테스트 구현이나 실행은 아직 없다.

## Single-Owner Rule

Carry, Strap, Relay reservation, physical lifecycle, Causal Origin,
room-state flag와 Reset state는 각각 하나의 authoritative owner만
가진다. repeated delivery가 가능한 boundary는 idempotent handling과
duplicate suppression을 명시한다.

## Later Systems

Perfect Catch, Momentum/Pip, HeavyAction, Relay Position과 Policy는
later Vertical Slice owner가 있다. Breakable, Structural, Upgrade,
Incident Report, LGT_01과 FIN_01은 first-wave architecture에서 제외한다.
