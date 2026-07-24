# Gameplay Specification

## 1. Logical Input Actions

논리 입력 의도의 Owner-approved 기준선은
[`LOGICAL_INPUT_CONTRACT.md`](LOGICAL_INPUT_CONTRACT.md)를 따른다.
이 절은 계약의 축약본이며 실제 키보드·게임패드 바인딩은 승인하지
않는다.

| Action | Applicability | 승인된 논리 의도 |
|---|---|---|
| `Move` | `PROTOTYPE CORE` | 원하는 평면 이동 방향 |
| `Look` | `PROTOTYPE CORE` | 원하는 camera 및 facing-orientation 변화 |
| `Aim` | `PROTOTYPE CORE` | targeting 또는 directional-focus 요청 |
| `InteractPrimary` | `PROTOTYPE CORE` | 현재 유효한 primary physical interaction 요청 |
| `Throw` | `PROTOTYPE CORE` | direct throw 또는 pre-arrival Queued Throw 의도 |
| `CallAlly` | `PROTOTYPE CORE` | 한 명의 활성 동료에게 physical object supply 요청 |
| `StrapRelease` | `PROTOTYPE CORE` | 활성 GEAR_01 연결의 명시적 종료 요청 |
| `ResetEncounter` | `PROTOTYPE CORE` | validation encounter 기준 상태 복원 요청 |
| `HeavyAction` | `VERTICAL SLICE` | 후속 승인 조건 아래 Heavy access/use 요청 |

### Context and Mode Summary

- Scoop, Normal Catch, Perfect Catch attempt와 Strap Attach는
  `InteractPrimary`를 사용한다.
- Direct Throw와 Queued Throw는 `Throw`를 사용한다.
- Quick, Charged와 Queued는 별도 logical input ID가 아니라 단일
  `Throw` intent의 후보 해석이다.
- Explicit Strap Release는 `StrapRelease`, ally supply request는
  `CallAlly`, Heavy request는 `HeavyAction`, encounter reset은
  `ResetEncounter`를 사용한다.

state availability, cross-context arbitration와 conflict는
[`INTERACTION_STATE_CONFLICT_CONTRACT.md`](INTERACTION_STATE_CONFLICT_CONTRACT.md)를
따른다. 동일 context의 target ranking, buffer, cancel과 timing은
계속 Draft다.

### Weight-Class Functional Boundary

class-based interaction envelope는
[`WEIGHT_CLASS_DATA_RANGE_CONTRACT.md`](WEIGHT_CLASS_DATA_RANGE_CONTRACT.md)를
따른다.

- Scoop: valid Light/Medium only; not Heavy.
- Catch: `CATCH_ELIGIBLE` Light relay only.
- Direct Throw: carried Light/valid Medium.
- Queued Throw: Catch-eligible Light relay only.
- Cargo Strap Redirect: mobile, floor-supported, valid-anchor Medium only.
- Heavy: `HeavyAction` and `VERTICAL SLICE ONLY`; no normal
  Scoop/Carry/Throw/Relay/Strap.

exact object assignment, mass, size, timing, range, Quick/Charged
threshold, force, movement restriction, animation과 physics는 Draft다.

### Momentum/Pip Starting Profile

Owner-approved G2 시작 profile은
[`MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md`](MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md)를
따른다.

- Momentum/Pip은 Prototype Core에서 비활성이며 Vertical Slice에서
  활성화한다.
- `HeavyAction`의 승인된 시작 비용은 `2 Banked Pips`다.
- Perfect Catch는 Catch 자체가 아니라 이후 player-owned action이
  유효한 physical result를 만들 때 origin당 한 번 `+5 Momentum`
  modifier를 적용할 수 있다.
- exact Chain classification·propagation·termination, Perfect Catch
  timing, HUD, feedback와 implementation은 계속 Draft다.

### Deferred Candidates

`Sprint`, `Evade`, `Vault`, `Shove`는 모두
`DRAFT CANDIDATE — NOT ACTIVE IN THE G2 LOGICAL INPUT BASELINE`이다.
Owner-approved G1 Prototype Core에 필수적이지 않고 locomotion,
combat, animation, physics 또는 state-conflict 범위를 추가하므로
각각 별도 G2 scope decision이 필요하다. 기존 아래 설계 논의는
역사적 Draft로 유지하며 활성 입력 계약으로 사용하지 않는다.

아래 2절과 8절은 Owner-approved Interaction State and Conflict
Contract의 축약본이다. 3~7절과 9절의 상세 flow, feedback, timing과
value는 별도 Owner 결정 전까지 `DRAFT`다. 따라서 6절의 자동
안전 해제 문구도 승인된 rule이 아닌 역사적 Draft 후보다.

## 2. Player Interaction Priority

Owner-approved cross-context arbitration은
[`INTERACTION_STATE_CONFLICT_CONTRACT.md`](INTERACTION_STATE_CONFLICT_CONTRACT.md)
`## InteractPrimary Arbitration`을 따른다.

- `ACTIVE_RELAY`에서는 명시적으로 조준한 유효
  `STRAP_ELIGIBLE` Anchor가 먼저 Strap Attach context를 만들고,
  그렇지 않으면 유효한 `CATCH_ELIGIBLE` Catch context만 평가한다.
- `ACTIVE_RELAY` 중 unrelated Scoop 또는 다른 context fallback은
  없다.
- relay가 없으면 명시적 Strap context를 먼저 평가하고, 그 context가
  없을 때만 Scoop candidate를 평가한다.
- 유효하지 않은 명시적 Strap context는 Scoop으로 fallback하지
  않는다.

동일 context 안의 target ranking과 정확한 feedback은 Draft다.

## 3. Scoop Flow

Owner-approved class envelope은 valid Light와 Medium의 Scoop만 허용하며
Heavy는 normal Scoop을 사용하지 않는다. 유효한 개별 object와
interaction range는 Draft다.

```text
Idle/Move
→ 유효 물체 감지
→ 입력
→ 손 위치·물체 자세 보정
→ Carry State
→ Aim 또는 Throw
```

실패 조건:

- 다른 물체가 경로를 완전히 막음
- 무게 등급이 현재 허용 범위 초과
- 물체가 이미 다른 시스템에 잠김

실패 시 이동을 멈추지 않고 짧은 피드백만 제공한다.

## 4. Catch Flow

```text
Pass Requested
→ Ally Wind-up
→ Object Released
→ Catch Window Open
→ Normal Catch 또는 Perfect Catch
→ Carry State / Queued Throw
```

- Normal Catch는 넓고 안정적이다.
- Perfect Catch는 짧고 선택적 보너스다.
- 놓친 Light는 바닥 물체가 되어 계속 사용 가능하다.
- Catch는 `CATCH_ELIGIBLE` Light relay에만 적용한다. Medium의 바닥
  roll/push는 Catch가 아닌 Strap-oriented physical relay다.

Normal/Perfect window, 보조 판정, Contact Freeze와 feedback은 Draft다.

## 5. Throw

Direct Throw는 carried Light와 valid Medium에 적용한다. Queued Throw는
Catch-eligible Light relay에만 적용한다. 아래 Quick/Charged 해석,
impulse, timing, 자세와 movement restriction은 역사적 `DRAFT`
후보다.

### Quick Throw

- 입력 짧게
- 빠른 발동
- 낮은 impulse
- Light 공격 차단, 근거리 대응

### Charged Throw

- 입력 유지
- 무게별 자세와 이동 제한
- 높은 impulse와 정확한 목표

### Queued Throw

- 패스 도착 전 Throw 유지
- 캐치와 투척 애니메이션을 연결
- Perfect Catch 성공 시 안정화와 보너스 적용

## 6. Cargo Strap Redirect

Owner-approved class envelope에서 normal Cargo Strap Redirect는
mobile, floor-supported, valid load-bearing metal Anchor를 가진
eligible Medium에만 적용한다. Light와 Heavy는 Strap eligible이
아니다.

```text
유효 Anchor 조준
→ Attach
→ 물체 운동 상태 유지
→ 플레이어 이동·회전에 따라 장력 계산
→ 허용 각도 내 진행 방향 보정
→ Explicit Release
```

- 물체가 정지한 상태에서 먼 거리로 끌어오는 기능이 아니다.
- Heavy에는 normal Strap Redirect를 사용하지 않는다.
- 과도한 장력, 장애물 또는 거리 조건에 따른 자동 Safety Release는
  승인된 rule이 아닌 역사적 `DRAFT` 후보다.

Heavy의 유일하게 승인된 player-action entry point는 Vertical Slice의
`HeavyAction`이다. 시작 비용은 `2 Banked Pips`이며 resource lifecycle은
Momentum/Pip Contract를 따른다. final cost tuning과 execution은
Draft다.

## 7. Shove

> `Shove`는 비활성 `DRAFT CANDIDATE`이며 현재 G2 logical-action
> baseline이나 state-conflict contract에 포함되지 않는다.

- 적과 거리 벌리기
- 적을 충돌 선상에 배치
- 흔들리는 가구 전도
- 진행 중 물체 방향의 미세 보정
- 직접 주력 공격이 아님

## 8. State Conflicts

Owner-approved 상태 소유권과 conflict의 전체 기준은
[`INTERACTION_STATE_CONFLICT_CONTRACT.md`](INTERACTION_STATE_CONFLICT_CONTRACT.md)를
따른다.

- Carry, active Cargo Strap, Throw commitment와 Heavy commitment는
  서로 배타적이다.
- `ACTIVE_RELAY` 중 unrelated Scoop과 추가 `CallAlly`를 차단한다.
- active Cargo Strap 중 `InteractPrimary`, `Throw`, `CallAlly`와
  `HeavyAction`을 차단하고 명시적 `StrapRelease`를 허용한다.
- Queued Throw는 eligible active relay에서 하나만 arm하며 Catch
  이외의 relay 해소, Strap Attach 또는 Reset에서 clear한다.
- `ResetEncounter`는 모든 상호작용 소유권보다 우선한다.
- invalid context는 unrelated action으로 fallback하지 않는다.

Carry/Strap/Throw/Heavy 중의 정확한 movement restriction, 외부
Knockback 처리, buffer와 cancel은 Draft다.

## 9. Feedback Requirements

모든 행동은 최소 두 개 채널로 피드백한다.

- 시각: 자세, 물체 위치, 장비 표시, 환경 변화
- 청각: 재질, 모터, 스트랩 장력, 충돌
- 카메라: 제한된 kick, FOV, 진동
- UI: 최소한의 Momentum, Catch 타이밍, Ally 상태
