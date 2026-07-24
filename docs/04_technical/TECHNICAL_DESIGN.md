# Technical Design

> **MIXED AUTHORITY — ALIGNED TO OWNER-APPROVED G3 TECHNICAL BASELINE**
>
> `docs/04_technical/G3_TECHNICAL_BASELINE.md`와 `ADR-0015`가 G3
> 기술 정책의 권위다. 이 문서의 class 이름과 구현 세부는
> provisional이며 Unity Bootstrap이나 구현을 승인하지 않는다.

## 1. Exact Technical Documentation Lock

| Item | Exact lock |
|---|---|
| Unity Editor | `6000.3.20f1` |
| URP | `17.3.0` |
| Input System | `1.20.0` |
| Cinemachine | `3.1.7` |
| Unity Test Framework | `1.6.0` |

- Physics direction: built-in 3D PhysX / GameObject.
- Baseline platform: Windows PC.
- AI Navigation: Prototype에서 제외, Relay Position wave까지 Deferred.
- hardware, resolution, FPS와 physics tuning은 later evidence다.

## 2. Prototype-Only First Wave

first wave는 one-room Prototype의 logical input, locomotion/camera,
interaction ownership, Scoop/Carry/Throw/Queued Throw, fixed Ally의
physical Light·floor-supported Medium supply, explicit Cargo Strap,
normalized impact, depth-limited causal attribution, room-state change,
Reset와 observability만 소유한다.

Perfect Catch, Momentum/Pip, HeavyAction, Relay Position과 Policy는
later Vertical Slice owner를 가진다. Suppressed, Behavior Tags,
Structural, breakables, LGT_01, FIN_01, Incident Report와 persistent
save는 first wave 밖이다.

## 3. Architecture Responsibility Domains

- Composition and Encounter Bootstrap
- Logical Input Adapter
- Player Locomotion and Camera Adapter
- Interaction and Ownership
- Physical Object Lifecycle
- Ally Supply and Reservation
- Cargo Strap
- Impact Normalization
- Causal Attribution and Room State
- Reset and Fault Recovery
- Presentation Adapters
- Test and Observability

각 mutable state는 owner 하나만 가진다. explicit construction과
one-way Presentation adapter를 사용한다. service locator, global
mutable registry와 primary global event bus는 사용하지 않는다.

## 4. Provisional Implementation Names

다음은 책임을 설명하기 위한 후보 이름이며 Canon이 아니다.

- Encounter composition root
- Logical input adapter
- Locomotion adapter
- Interaction/Carry/Throw owner
- Physical object lifecycle owner
- Ally supply/reservation owner
- Cargo Strap owner
- Impact normalizer
- Causal/room-state owner
- Reset/fault owner
- Presentation adapter
- Harness/observability adapter

Evade, Vault, fixed Catch/Strap/Scoop priority, settled Quick/Charged
mode, generic ally inventory, automatic Strap release와 first-wave
Momentum/Breakable/Upgrade/Incident system은 active assumption이 아니다.

## 4A. Physics Policy

- bounded Dynamic activation;
- measured trajectory risk에 따른 selective CCD;
- primitive/simple compound collider;
- raw collision을 normalized domain fact로 변환;
- released object의 physical continuity;
- observable quarantine 또는 baseline restore;
- 정확한 안전 상한은 later tuning.

Prototype breakable, Structural Feature와 uncontrolled ragdoll은 비활성이다.

## 5. 데이터 원칙

stable content reference, transient runtime entity, encounter generation,
Causal Origin과 Result uniqueness identity를 분리한다. shared definition에
runtime state를 저장하지 않는다. Carry, Strap, Relay reservation과
encounter state에는 각각 single owner가 있다.

## 6. 디버깅

- active intent/context와 interaction state;
- object ownership과 physical state;
- Strap state/Anchor validity;
- Ally state/reservation/relay phase;
- Origin/parent/carrier/depth/Result family;
- duplicate/cycle rejection reason;
- room-state flag와 Reset diff;
- subscription/fault/active-body/collision/frame-spike counter.

## 7. Bounded Post-G3 Sequence

G3 passage와 final pre-implementation review 이후에만 다음 순서를
ticket으로 분해한다.

1. Unity and repository Bootstrap
2. definition/runtime identity and state foundation
3. locomotion, camera and logical input adapter
4. interaction arbitration and ownership
5. acquisition, Carry, Throw and Queued Throw
6. Reset, diff and fallback
7. fixed Ally supply and Catch
8. Cargo Strap attach, redirect and release
9. impact, causal attribution and room-state change
10. feedback, observability, fault recovery and regression

이 순서는 현재 실행 권한이 아니다.
