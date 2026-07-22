# ADR-0014: FIN_01 Conference Table Team Finisher Production Visual Lock v003

- Status: Accepted
- Date: 2026-07-22

## Decision

`references/approved/gameplay/FIN_01_Conference_Table_Team_Finisher_v003_FINAL.png`를 FIN_01 Conference Table Team Finisher Production Visual Canon으로 승인한다.

FIN_01은 PR-F-022 `COULD` 기능의 Visual-only conditional direction이다. 이 결정은 FIN_01을 Vertical Slice, 최종 제품, 구현 우선순위 또는 제작 약속에 편입하지 않는다.

## Canon

- 승인된 CHR_00 한 명, 승인된 CHR_01 한 명과 익명의 Finisher-only 보조 직원 한 명
- 세 인물이 하나의 Heavy 회의 테이블에 직접 접촉해 공동으로 미는 관계
- 중앙 선행 위치에서 최종 방향과 타이밍을 주도하는 CHR_00
- 플레이어를 지원하되 대체하지 않는 CHR_01과 익명 보조 직원
- 양손 접촉, 체중 이동, 발 접지와 상체 기울기로 읽히는 공동 추진
- 현실적인 하중 보조 상태의 CHR_00 외부 전완 브레이스
- 통합 캐스터 구조로 바닥 지지를 유지하는 테이블
- 임시 목표와 충돌하기 전에 끝나는 장면
- Stored/Inactive GEAR_01과 완전히 분리된 붉은 넥타이

세부 Canonical Elements는 `docs/03_art/CONFERENCE_TABLE_TEAM_FINISHER_VISUAL_SPEC.md`를 따른다.

## Ally-Scope Boundary

익명의 보조 직원은 이 Finisher 시각 구성 안에서만 존재하며 CHR_02, 일반 전투 Active Ally, 지속 Party Member 또는 승인된 Character Identity가 아니다. 일반 전투의 one-active-ally 제한은 유지된다.

## Non-Canon

- 익명 보조 직원의 얼굴, 복장, 직업, 이름과 신원
- 정확한 테이블 모델, 크기, 질량, 재질, base 구조와 캐스터 수
- 정확한 목표 수와 외형, 카메라·방·배우 위치와 자세
- Trigger, 빈도, Momentum/Pip 비용, 입력, 속도, 힘, 마찰과 타이밍
- Choreography, Animation, AI, IK, Rigidbody와 Caster Physics
- Cinemachine과 Unity 구현
- Impact, Knockback, Ragdoll, 피해, 유리 파손, 잔해와 Aftermath

## Consequences

- FIN_01의 문서화된 Visual Canon은 `LOCKED`다.
- FIN_01 기능과 구현은 `COULD`, `DRAFT` 또는 미승인 상태를 유지한다.
- 현재 Production Visual Lock sequence는 완료되었다.
- Full Design Lock은 미완료다.
- 다음 Production Visual 대상은 승인되지 않았다.
- Implementation Gate는 `CLOSED`로 유지한다.
