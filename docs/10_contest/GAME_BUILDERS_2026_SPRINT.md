# Game Builders Seoul 2026 Contest Sprint

- Project: `BREAK NOTICE`
- Public Working Title: `BREAK NOTICE: ZERO DEFECT`
- Track: `OpenAI Game Builders Seoul 2026 / Track 1`
- Status: `CONTEST-0001 C0-C4 ACCEPTED FOUNDATION PRESERVED / CONTEST-0002 S0 AUTHORITY REGISTRATION COMPLETE / S1 NOT AUTHORIZED / C5 NOT AUTHORIZED`
- Owner: `조윤성`
- Stable Ticket: `CONTEST-0001`
- Showcase Ticket: `CONTEST-0002`
- Stable Branch: `contest/game-builders-2026 @ 15c915c4c7552113941149e84ba5cc1fea9c63f1`
- Showcase Branch: `contest/game-builders-2026-showcase`

## Goal

포트폴리오 대표작과 Track 1 제출에 사용할 수 있는 안정적인 5–7분
Operations Floor Showcase를 완성한다. 검증된 단일 방 `CONTEST-0001`은
C0–C4 회귀 기준선으로 보존하고, 새 방향은 별도 `CONTEST-0002` lineage와
Scene에서만 단계별로 구현한다. 전체 게임을 만들지 않으며 실제 playable
link와 core originality를 content volume보다 우선한다.

## Project History

- 2024: DYC 초기 개발 착수 후 중단
- 2026-07-01: 조윤성이 단독 전면 refactor·redesign 시작
- 2026-08-04: contest period 시작
- 현재: 추가 사람이 실제 contest 산출물을 만들기 전까지 Codex를 활용한
  1인 Human project

## Baselines and Evidence

- Pre-challenge: `b82bb382fc0439847d6497ee2fadd9c55781a9fa`
- Pre-challenge tag: `gb2026-prechallenge`
- Sprint start: `28b7f70c4b6299159afe5b99ea7b98d1e5399352`
- Sprint tag: `gb2026-sprint-start`
- Remote contest/art publication: `COMPLETE / VERIFIED / 2026-08-06`
- Final tag: `gb2026-submission` — submission freeze에서만 생성
- Contest diff:
  `git log gb2026-prechallenge..gb2026-submission` 및
  `git diff --stat gb2026-prechallenge..gb2026-submission`

기존 public history를 amend/rebase/force-push하거나 기존 tag를 이동하지 않는다.

## Accepted Foundation and Showcase Scope

`CONTEST-0001`의 single-room C0–C4 이동·카메라·Interaction·Carry·Throw·
Reset·물리 공급 및 post-acceptance tuning은 accepted regression
foundation으로 보존한다. 기존 Scene과 티켓을 새 방향에 맞춰 소급 수정하지
않는다.

`CONTEST-0002 / PRT-RM-002` design authority:

- target session `5–7분`, 즉시 replay 동기
- 별도 authored Showcase room `1`, 48 m × 34 m, flat figure-eight
- central Cubicle Island, west collaboration loop, east physics lane,
  north Drawer/Finisher gate와 짧은 Exit run
- CHR_00 `1`, mobile CHR_01 `1`, 동일 Compliance Runner `2`
- Binder `5`, Chair `3`, Drawer `1`, Copier recognition-only `1`
- Ally roles: Follow, Fetch, Supply, Brace, Rescue, Combat Tag, Finisher Partner
- Q priority: Break Grab → Team Finish → Assist Attach → Tag → Supply → Regroup
- PASS / BRACE / BREAK 중 서로 다른 2개로 TEAM READY
- Drawer solutions: Direct Chair, Strap Redirect, Runner Causal Collision,
  optional `FIN-P-001 HANDOVER CRASH`

이 scope는 공모전 Showcase 전용이며 full-game Canon 또는 구현 완료로 자동
고정하지 않는다.

## Forbidden Scope

두 번째 Showcase room, full campaign, procedural generation, persistent save,
hidden inventory, 두 번째 active ally, 추가 enemy archetype, boss, firearm,
magic, telekinesis, grapple, Momentum/Pip/HeavyAction, Policy Violation,
upgrade/meta progression, structural/breakable/glass destruction, runtime
fracture, unrestricted ragdoll, full rope/towing, multiplayer/account/
leaderboard/analytics, paid API core dependency, Hive SDK, Addressables,
DOTS/Netcode, third-party behavior/rope/save package, long cutscene/dialogue와
desktop-browser 안정화 전 mobile optimization을 구현하지 않는다.

## Team and Contribution Policy

- 공식 팀은 최대 3명: 조윤성 + Designer A + Designer B
- 실제 산출물을 만들지 않은 사람은 기여자로 기록하지 않는다.
- 실제 기여는 날짜, 파일, 역할, 권리와 사용 허용을 ledger에 기록한다.
- 운영진 서면 허용 또는 공식 팀 재구성 없이 네 번째 사람의 contest-period
  신규 산출물을 제출작에 포함하지 않는다.
- Designer A/B는 각 art branch에서 one-purpose commit과 normal push만
  사용한다. PR·merge와 `main`/contest integration branch 직접 push는 별도
  Human Owner 승인 전 금지한다.

## Branch Policy

- Accepted regression integration: `contest/game-builders-2026`
- Showcase integration: `contest/game-builders-2026-showcase`
- Character/Animation: `art/characters-animation`
- Environment/UI/VFX: `art/environment-ui-vfx`
- stable contest와 main은 S0에서 수정하지 않는다.
- S0는 art branch를 switch/fast-forward/merge하지 않는다.
- 현재 Designer A/B 전달은 source candidate이며 Runtime approval이 아니다.
- PR/merge는 별도 Human Owner 승인 전 금지한다.
- 어떤 contest branch도 자동으로 `main`에 병합하지 않는다.

## Showcase Work Packages

| Package | Target | Current State |
|---|---|---|
| S0 | CR-0004, ADR-0024, CONTEST-0002, exact directives, locks/matrix/handoffs | `COMPLETE / DOCUMENTATION ONLY` |
| S1 | separate 48 m × 34 m figure-eight graybox | `NOT AUTHORIZED` |
| S2 | mobile Ally Follow anchors, catch-up, blocked/regroup | `NOT AUTHORIZED` |
| S3 | mobile Fetch, Binder Toss, Chair Feed and Reset adaptation | `NOT AUTHORIZED` |
| S4 | Q context, Rescue, Combat Tag, PASS/BRACE/BREAK, TEAM READY | `NOT AUTHORIZED` |
| S5 | bounded Cargo Strap and Ally attach/tension assist | `NOT AUTHORIZED` |
| S6 | two Runner pressure, short grab, stagger and one attack token | `NOT AUTHORIZED` |
| S7 | Drawer four-solution hooks, Parking Bay, EXIT and result card | `NOT AUTHORIZED` |
| S8 | HANDOVER CRASH paired sequence and physical Drawer result | `NOT AUTHORIZED` |
| S9 | technically accepted Art/VFX/Audio/UI and Web qualification | `NOT AUTHORIZED` |

각 package는 새 baseline-bound one-shot Human Owner Relay가 필요하다. 일정이
밀리면 art complexity를 줄이며 scope를 추가하지 않는다.

## Current Implementation Progress — 2026-08-11

- C0: `COMPLETE / HUMAN OWNER PASS`
- C1: `COMPLETE / commit 278d062a6b486a176a7cb9144b75dfb058567ead`
- C2: `COMPLETE / commit 083b44e86c76977bd855bf0c2c09baf0001b9750`
- C3: `IMPLEMENTATION AND AUTOMATED ACCEPTANCE COMPLETE / containing commit`
- C4: `IMPLEMENTATION COMPLETE / initial commit 30376d475ca0e07e9026965a9f11c6cfec241ba5 / Human Owner acceptance revision e4ee3b714552fb8bf8ba90216ce858a988f0f006 / HUMAN MANUAL ACCEPTANCE PASS 2026-08-11`
- Post-Acceptance Tuning: `CHARGED THROW, PICKUP ASSIST AND ALLY SUPPLY VARIATION / commits e5be4c476ab9cc8f2e3ffacc17b124db60b15f12 and f1fd9b15f407be9003eff622231e338358376bcc / HUMAN CHECKLIST PASS`
- Preserved Validation: `Unity compile PASS / EditMode 65/65 / PlayMode 59/59 / total 124/124 / Web GUID 3cc7416d44c44067bc51c6474c46351d / warning-error 0/0 / HTTP 19/19`
- S0: `CR-0004 / ADR-0024 / CONTEST-0002 AUTHORITY REGISTRATION COMPLETE / NO UNITY TEST OR BUILD`
- Boundary: `S1 NOT AUTHORIZED / HISTORICAL C5 NOT AUTHORIZED / RUNTIME ART NOT AUTHORIZED / MAIN PRODUCT GATE CLOSED`

## Definition of Complete

- Chrome/Edge anonymous desktop browser에서 public URL이 안정적으로 실행
- Direct Chair, Strap Redirect, Runner Causal Collision과 optional Team
  Finisher의 네 solution hook을 각 승인된 package 범위에서 검증
- 5–7분 평균 흐름과 alternate route/replay 의도를 Human playtest로 검증
- manual reset `10`, failure restart `5`, full clear `3`
- blocking browser/Unity console error `0`
- source SHA, tests, build hash/size, browser evidence와 licenses 기록
- thumbnail, controls, 200자 이내 소개와 실행 지침 준비
- final release tag와 backup deployable build 생성

## Feature and Submission Freeze

- Feature Freeze: `2026-08-24 이전`
- 이후에는 blocker, usability, performance, presentation, license와
  submission fix만 허용한다.
- Submission Freeze에서 source SHA, build hash, URL, browser results,
  contributor/license ledger를 고정하고 `gb2026-submission`을 생성한다.
