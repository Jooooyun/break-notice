# ADR-0015 — G3 Technical Baseline Decisions

- Status: Accepted
- Date: 2026-07-25
- Decision Source: delegated Owner approval of `DEC-G3-001` through
  `DEC-G3-013`
- Scope: documentation-level technical baseline only

## Decision

Owner가 `APPROVE ALL RECOMMENDED G3 DEFAULTS`로 다음 13개 결정을
모두 Option A로 승인했다.

| Decision | Accepted Option A |
|---|---|
| DEC-G3-001 | Unity Editor와 approved package를 exact version으로 함께 lock |
| DEC-G3-002 | minimal Prototype package baseline; AI Navigation later |
| DEC-G3-003 | approved one-room Prototype behavior만 first wave에 포함 |
| DEC-G3-004 | explicit composition, single owner, request/result와 one-way presentation dependency |
| DEC-G3-005 | stable definition reference, transient runtime identity와 runtime state 분리 |
| DEC-G3-006 | ordered lifecycle, idempotency, duplicate/reentrancy rule와 exclusive Reset barrier |
| DEC-G3-007 | content/runtime/Origin/Result identity 분리, Origin merge 금지 |
| DEC-G3-008 | bounded dynamics, selective CCD, normalized collision과 observable recovery |
| DEC-G3-009 | simplified Strap visual/logical constraint, bounded redirect와 explicit release |
| DEC-G3-010 | Prototype/Slice budget 및 later measurement ownership 분리 |
| DEC-G3-011 | snapshot/restore primary, audited Scene reload fallback, persistent save 분리 |
| DEC-G3-012 | EditMode/PlayMode/Manual plan과 ownership/lineage/reset/fault/budget observability |
| DEC-G3-013 | bounded 10-wave sequence; G3 exit와 final review 전 실행 금지 |

## Exact Editor and Package Documentation Lock

| Identifier | Exact lock | Classification |
|---|---|---|
| Unity Editor | `6000.3.20f1` | Editor Core |
| `com.unity.render-pipelines.universal` | `17.3.0` | Editor-matched Core Package |
| `com.unity.inputsystem` | `1.20.0` | Separately installed Released Package |
| `com.unity.cinemachine` | `3.1.7` | Separately installed Released Package |
| `com.unity.test-framework` | `1.6.0` | Editor-matched Core Package |

AI Navigation은 Prototype package baseline 밖이며 approved Relay Position
implementation wave까지 `DEFERRED`다. floating package version은
허용하지 않으며 transitive dependency는 실제 generated package lock이
나중에 기록한다.

## Technical Baseline

세부 기술 기준선은
`docs/04_technical/G3_TECHNICAL_BASELINE.md`를 따른다.

승인된 방향은 다음을 포함한다.

- Prototype와 later/optional technical scope의 분리;
- explicit architecture와 one-way dependency;
- definition data, runtime state와 causal identity의 분리;
- ordered request/fact/result/presentation flow와 Reset barrier;
- bounded physics activation, normalized impact와 observable recovery;
- simplified Cargo Strap constraint와 explicit release;
- Prototype/later-Slice measurement ownership 분리;
- snapshot/restore Reset과 audited Scene-reload fallback;
- EditMode, PlayMode, Manual harness plan과 observability;
- G3 passage와 final review 뒤에만 가능한 bounded sequence.

## Exclusions

Prototype baseline은 Addressables, DOTS/Entities, Netcode, AI Navigation,
third-party rope/behavior-tree/save framework, generic service locator,
generic dependency injection, generic global event bus, persistent
telemetry와 runtime analytics를 포함하지 않는다.

Perfect Catch, Momentum/Pip, HeavyAction, Relay Position과 Policy
Violation은 later Vertical Slice owner를 가진다. Suppressed, Behavior
Tags, Structural Feature, breakables, LGT_01, FIN_01, Incident Report,
persistent save, CHR_02, extra active allies, additional enemy archetypes와
multi-room production은 first-wave scope 밖이다.

## Gate Boundary

이 ADR은 G3 Technical documentation baseline을 승인한다. G3는 Final
Audit 전까지 `NOT PASSED`다. Unity Bootstrap, package installation,
code, test, Scene, Prefab, build, Graybox와 구현은 승인되지 않았고
Implementation Gate는 `CLOSED`다.
