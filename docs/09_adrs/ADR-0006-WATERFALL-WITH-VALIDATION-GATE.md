# ADR-0006: Waterfall with One Explicit Graybox Validation Gate

- Status: Accepted
- Date: 2026-07-16

## Decision

요구사항→설계→기술설계→Graybox 검증→최종 동결→본 제작 순서를 사용한다.

## Rationale

Codex 기반 구현에서 설계 흔들림을 막아야 하지만, 게임의 재미는 문서만으로 증명할 수 없다.

## Constraint

Graybox에서 수치·카메라·배치·타이밍은 조정 가능하다. 핵심 정체성 변경은 Change Request와 ADR 대체가 필요하다.
