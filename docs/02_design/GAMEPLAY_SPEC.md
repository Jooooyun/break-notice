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

이 mapping은 state availability, interaction priority, target ranking,
buffer, cancel 또는 timing을 결정하지 않는다.

### Deferred Candidates

`Sprint`, `Evade`, `Vault`, `Shove`는 모두
`DRAFT CANDIDATE — NOT ACTIVE IN THE G2 LOGICAL INPUT BASELINE`이다.
Owner-approved G1 Prototype Core에 필수적이지 않고 locomotion,
combat, animation, physics 또는 state-conflict 범위를 추가하므로
각각 별도 G2 scope decision이 필요하다. 기존 아래 설계 논의는
역사적 Draft로 유지하며 활성 입력 계약으로 사용하지 않는다.

아래 2~9절의 priority, flow, state conflict, feedback, timing과 value는
별도 Owner 결정 전까지 `DRAFT`다. 이 작업은 `Player Interaction
Priority`를 해결하지 않는다.

## 2. Player Interaction Priority

하나의 입력이 여러 대상과 겹칠 때 아래 순서를 따른다.

1. 도착 중인 Catch 대상
2. 명시적으로 조준된 Strap Anchor
3. 손이 닿는 Scoop 물체
4. 문맥 환경 상호작용

모호한 자동 선택으로 플레이어 의도를 바꾸지 않는다. 현재 선택 대상은 실루엣·작은 기계 표시로 피드백한다.

## 3. Scoop Flow

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
- 놓친 Medium은 바닥을 구르며 환경과 충돌할 수 있다.

## 5. Throw

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

```text
유효 Anchor 조준
→ Attach
→ 물체 운동 상태 유지
→ 플레이어 이동·회전에 따라 장력 계산
→ 허용 각도 내 진행 방향 보정
→ Release 또는 자동 안전 해제
```

- 물체가 정지한 상태에서 먼 거리로 끌어오는 기능이 아니다.
- Heavy는 Anchor 해제·보조 용도 외 리디렉션 불가.
- 과도한 장력, 장애물, 최대 거리에서 자동 해제한다.

## 7. Shove

- 적과 거리 벌리기
- 적을 충돌 선상에 배치
- 흔들리는 가구 전도
- 진행 중 물체 방향의 미세 보정
- 직접 주력 공격이 아님

## 8. State Conflicts

| 현재 상태 | 금지/제한 |
|---|---|
| Carry Light | Sprint 가능, Evade 가능 |
| Carry Medium | Sprint 제한, Evade 제한적 |
| Carry Heavy | Sprint 불가, 회전 제한, 피격 위험 |
| Strap Attached | 최대 거리 제한, 일부 회피 제한 |
| Catch Window | Scoop보다 Catch 우선 |
| Knockback | 입력 버퍼 최소화, 회복 후 명확히 복귀 |

## 9. Feedback Requirements

모든 행동은 최소 두 개 채널로 피드백한다.

- 시각: 자세, 물체 위치, 장비 표시, 환경 변화
- 청각: 재질, 모터, 스트랩 장력, 충돌
- 카메라: 제한된 kick, FOV, 진동
- UI: 최소한의 Momentum, Catch 타이밍, Ally 상태
