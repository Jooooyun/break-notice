# VFX and Destruction Style Guide

## Status

`MIXED AUTHORITY`

`PGT_03_Chain_Reaction_Aftermath_v003_FINAL`의 문서화된 Canonical
Elements는 Visual Lock이다. 정확한 물리 파편·small-response 상한과
표현 정책은 Owner-approved
`docs/04_technical/DESTRUCTION_BUDGET_AND_IMPLEMENTATION_RULES.md`와
ADR-0016을 따른다. PGT_03의 exact layout, Chain 순서와 구현은
`CHAIN_REACTION_AFTERMATH_VISUAL_SPEC.md` 및 ADR-0012의 Non-Canon
경계를 유지한다.

## 예산

아래 수치는 Owner-approved hard scope cap이다.

### Owner-Approved

- 파괴는 국소적이고 읽을 수 있어야 한다.
- ordinary Break Event의 Large Physical Fragment는 최대 `4`,
  conditional Structural Event는 최대 `8`이다.
- room당 pooled Large Fragment는 최대 `16`, 동시에 awake 상태는
  최대 `12`다.
- small dynamic Rigidbody debris는 `0`이다.
- ordinary/conditional Structural event의 Small Material Response는
  각각 최대 `32`/`64`, room 동시 live cap은 `96`이다.
- Primary Impact emitter는 event당 `1`개다.
- Material Burst emitter는 event당 `1`개다.
- persistent Aftermath marker는 event당 `1`, room당 동시 최대 `3`이다.
- response는 절제하며 gameplay route와 causal readability를 가리지
  않는다.
- 화면을 가리는 연기, 초자연 energy, fireball 또는 일반 폭발 표현을
  사용하지 않는다.
- fragment와 emitter는 pre-authored/pool 정책을 따르며 runtime mesh
  fracture를 사용하지 않는다.

이 수치는 Design Freeze scope cap이며 exact Particle System, VFX Graph,
shader, lifetime, force 또는 성능 통과 기준을 승인하지 않는다.

## Superseded Historical Candidate

- 파괴 사건당 실제 큰 파편 `8–15`개 후보는 `SUPERSEDED`다.
- 이 문서나 과거 prompt의 다른 conflicting count도 active authority가
  아니다.

## Material-Art Guidance

### Primary Impact

최초 충돌점의 짧고 작은 방향성 효과를 사용한다. 일반 폭발 모양은
금지한다.

### Material Burst

- 유리: 읽을 수 있는 authored 큰 조각과 VFX-only small glass
- 목재·복합재: authored 큰 조각과 VFX-only dust/splinter
- 금속: 기본적으로 deformation, 짧은 spark와 sound
- 종이·먼지·토너: 넓고 느린 VFX-only response
- 전자제품: 화면 flash, toner, spark와 제한된 authored 큰 부품

### Chain Link

후속 chain presentation이 필요하면 neon line보다 밝기와 방향 중심으로
표현한다. exact hop trigger와 시간은 Draft다.

### Aftermath

떨어지는 종이, toner 잔류, authored local visibility/route state와
이동한 가구로 결과를 읽게 한다. exact asset identity와 route는 G5
Draft다.

## 무게별 VFX

| 등급 | VFX 목표 |
|---|---|
| Light | 얇고 날카로운 작은 파편 |
| Medium | 방향성 먼지, 종이, 큰 물체 이동 |
| Heavy | 짧은 화면 정지, 큰 덩어리, 구조 상태 변화 |

위 표는 material-art guidance다. Weight Class만으로 Breakable,
fragment count, Structural inclusion 또는 exact response를 승인하지
않는다.

## 금지

만화 별, 속도선, 형광 외곽선, energy wave, 과도한 slow motion,
fireball, screen-covering smoke, runtime fracture, 작은 동적 debris,
recursive destruction과 room-wide collapse.
