# ADR-0023 — Game Builders 2026 Contest-Bounded Product Branch

- Status: `Accepted`
- Date: `2026-08-06`
- Decision Source: `Human Owner Direct Authorization`
- Change Control: `CR-0003`
- Ticket: `CONTEST-0001`

## Context

장기 제품 `main`의 G4-A locomotion·camera·logical-input foundation은
검증됐지만 formal G4 evidence와 G5는 통과하지 않았다. Game Builders
2026 제출에는 빠르게 검증 가능한 브라우저 core loop가 필요하며, 장기 제품
기준선과 공모전 기간 변경을 명확히 분리해야 한다.

## Decision

exact baseline `28b7f70c4b6299159afe5b99ea7b98d1e5399352`에서
`contest/game-builders-2026`을 만들고 다음 bounded Gate를 연다.

`OPEN — CONTEST-0001 ONLY / GAME BUILDERS 2026 SINGLE-ROOM BROWSER PROTOTYPE`

CONTEST-0001은 잠긴 C0~C9 범위 안에서 여러 one-purpose commit을 허용한다.
`main`과 공모전 branch는 자동 병합하지 않으며, 공모전 종료 뒤 재사용 가능한
결과는 Human Owner 검토 후 별도로 선택한다.

## Visual Reference Decision

기존 승인 Visual Canon과 Production Support는 runtime asset이 아니라
디자인·모델링·리뷰 기준이다. 공모전 제작자는 tracked
`references/approved/` 경로와 해당 Visual Spec/ADR을 사용한다.
untracked REVIEW 원본은 byte-identical 승인본이 이미 있는 경우에도 원본
증거로 보존하고 stage하지 않는다. 승인되지 않은 이전 revision은 historical
exploration으로 유지한다.

## Consequences

- G4/G5 passage를 주장하지 않는다.
- Contest room과 encounter는 full-game Canon이 아니다.
- 기존 package lock과 G4-A scene을 보존한다.
- 별도 Contest Scene과 Web build pipeline을 허용한다.
- production asset은 정확한 provenance, license, delivery와 Owner 승인이
  있어야 통합할 수 있다.
- `gb2026-prechallenge`, `gb2026-sprint-start`, 이후
  `gb2026-submission`이 contest-period evidence를 고정한다.

## Non-Authorization Boundary

이 ADR은 full campaign, 두 번째 room, 새 actor/object definition,
금지 mechanic, 승인 없는 dependency/service, contest branch의 자동 main
병합, 기존 history 재작성 또는 승인되지 않은 자산 사용을 허용하지 않는다.
