# Ally Relay System

상태: `MIXED AUTHORITY — OWNER-APPROVED CONTRACT SUMMARY WITH
HISTORICAL DRAFT DETAIL`

## 1. Authority

동료 릴레이, Relay Position과 Suppressed의 현재 Owner-approved G2
권한은
[`ALLY_RELAY_SUPPRESSED_CONTRACT.md`](ALLY_RELAY_SUPPRESSED_CONTRACT.md)를
따른다.

이 문서의 과거 세부안은 계약과 충돌할 경우 권한이 없다. exact AI,
pathfinding, animation, physics, timing, count와 Unity 구현은 Draft다.

## 2. Owner-Approved Summary

- 일반 전투 active ally는 승인된 CHR_01 한 명뿐이다.
- Prototype Core는 한 개의 고정 authored support position에서 실제
  Light와 이동 가능한 Medium을 공급한다.
- Vertical Slice는 유한한 authored Relay Positions와 그 사이의
  물리적 전환을 사용할 수 있다.
- Light는 airborne Catch-oriented physical supply다.
- 이동 가능한 Medium은 floor-supported Strap-oriented supply다.
- Heavy는 normal `CallAlly` relay에 포함되지 않는다.
- Suppressed는 Prototype Core에서 비활성이며 Vertical Slice
  `SHOULD`인 조건부, 일시적, 회복 가능한 support state다.
- 동료는 플레이어의 최종 Throw, Redirect, HeavyAction, resource
  소비 또는 적 처치를 대신하지 않는다.

## 3. Active Ally and Identity Boundary

`Facilities Mover`는 승인된 이름, 직무 또는 Character Identity가
아니다. 활성 동료의 시각적 정체성은 CHR_01 Canon을 따른다.

FIN_01의 익명 Finisher-only 보조 직원은 CHR_02, 일반 전투 active
ally 또는 지속 party member가 아니다. FIN_01은 계속 `OUT / COULD`다.

## 4. Relay Position Summary

Prototype Core의 position은 encounter 동안 고정된다. Relay Position
전환은 Vertical Slice에만 적용하며 authored position 사이에서만
물리적으로 일어난다. free-follow와 teleport는 없다.

정확한 position 수, placement, support zone, route, navigation과
전환 시간은 Draft다.

## 5. Physical Supply Summary

`CallAlly`는 현재 gameplay context가 정확히 한 개의 유효한 물체와
readable supply lane을 만들 때만 수락된다. 후보는 실제 방에 존재하고,
도달 가능하며, 예약되지 않았고, 승인된 Weight delivery envelope에
속해야 한다.

spawn, teleport, remote pull, hidden inventory와 duplication은 없다.
한 relay는 한 물체만 예약하며 release 또는 safe abort에서 reservation을
해소한다.

## 6. Relay Lifecycle Summary

동료 지원 상태는 `READY`, `SUPPLY_COMMITTED`, `RELOCATING`,
`SUPPRESSED`로 구분한다. `ACTIVE_RELAY`는 release 전 `PRE-RELEASE`와
release 후 `RELEASED` 논리 phase를 가진다.

release 전 물체·경로·position 무효화, Suppressed, Reset 또는 lifecycle
종료는 safe abort를 만들 수 있다. release 뒤 물체는 삭제되거나
원격 제어되지 않고 실제 물리 경로를 계속 따른다.

## 7. Suppressed

Suppressed는 active hostile pressure가 current support position을
contested 또는 unsafe하게 만들 때만 진입한다. 새 `CallAlly`,
reservation과 relay 시작을 차단하지만 이미 `RELEASED`인 물체,
유효한 provenance 또는 player-owned follow-through를 지우지 않는다.

hostile pressure가 해소되고 유효한 authored position에 있으며
unresolved pre-release relay가 없으면 `READY`로 회복할 수 있다.

## 8. Historical Draft — Non-Authoritative

다음 과거 문구와 후보는 Owner-approved 계약이 아니며 이 문서에서
활성 규칙으로 사용하지 않는다.

- 방마다 Relay Position `2~3개`라는 고정 수;
- `Facilities Mover`라는 Identity 또는 직무;
- exact 유효 범위, 후보 표시 수, cooldown과 recovery;
- 동료의 자동 Heavy 고정부 해제 또는 방향 정렬 지원;
- 파괴된 가구가 자동으로 새 Light/Medium 공급물을 생성한다는 규칙;
- 재고 자동 회수;
- 고정 AI 우선순위;
- NavMesh, behavior tree, animation, IK, Rigidbody, physics와 Unity
  구현.

Prototype의 exact 적, 방, 물체 목록과 공급 inventory는 후속
`Prototype Enemy, Room, Object Content Lock`에서 결정한다.
