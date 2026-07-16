# ADR-0008: GEAR_01 Production Visual Lock v002

- Status: Accepted
- Date: 2026-07-17

## Decision

`references/approved/equipment/GEAR_01_Cargo_Strapping_System_v002_FINAL.png`를 GEAR_01 Cargo Strapping System Production Visual Canon으로 승인한다.

## Canon

- 허리 수축 릴, 검정·황색 직조 스트랩, 은색 카라비너, 교체식 금속 자산 클램프
- `릴 → 스트랩 → 카라비너 → 클램프 → 하중 지지 금속 Anchor` 연결 체인
- 캐스터·휠 축·플라스틱 하우징·취약 돌출부를 사용하지 않는 금속 Anchor
- Stored, Attached, Tensioned Redirect, Safety Release, Brace Lock, Warning Behavior의 여섯 시각 상태
- 바닥 지지 의자, 체결 위치가 읽히는 브레이스 잠금 레버, 제한된 황색 경고 표시
- 암적색 넥타이와 전체 장비의 완전한 분리

세부 Canonical Elements는 `docs/03_art/CARGO_STRAP_SPEC.md`를 따른다.

## Non-Canon

정확한 거리, 각도, 힘, 타이밍, 입력, 장력 임계값, 내부 작동 원리, 리깅·물리·Unity 구현, 미세 볼트·흠집·패널 배치는 승인하지 않는다.

## Consequences

- GEAR_01은 `LOCKED` Production Visual이다.
- 다음 Production Visual 대상은 `CHR_01 — First Ally Identity`다.
- Implementation Gate는 `CLOSED`로 유지한다.
