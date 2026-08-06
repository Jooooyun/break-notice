# Game Builders Seoul 2026 Contest Sprint

- Project: `BREAK NOTICE`
- Public Working Title: `BREAK NOTICE: ZERO DEFECT`
- Track: `OpenAI Game Builders Seoul 2026 / Track 1`
- Status: `ACTIVE / C0 AIM-SMOOTHED BUILD OWNER RETEST PENDING / C1 NOT AUTHORIZED`
- Owner: `조윤성`
- Ticket: `CONTEST-0001`
- Branch: `contest/game-builders-2026`

## Goal

포트폴리오 대표작과 Track 1 제출에 사용할 수 있는 안정적인 단일 방
desktop-browser physics action prototype을 완성한다. 전체 게임을 만들지
않으며, 실제 playable link와 core originality를 content volume보다 우선한다.

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

## Exact Contest Scope

- first blind clear `3–5분`, failure 포함 session 약 `7분` 이내
- authored room `1`, primary objective `1`, 즉시 retry
- CHR_00 `1`, fixed CHR_01 `1`, 동일 Compliance Runner `2`
- Binder `3`, Chair `2`, Drawer `1`, Copier recognition-only `1`
- logical actions: Move, Look, Aim, InteractPrimary, Throw, CallAlly,
  StrapRelease, ResetEncounter
- Drawer를 물리적으로 이동해 Side Route를 열고 EXIT 도달
- solution A: direct/chained Binder 또는 Chair throw
- solution B: ally-fed grounded Chair + bounded Cargo Strap redirect

Contest room layout과 encounter sequence는 Prototype Representation이며
full-game Canon으로 자동 고정하지 않는다.

## Forbidden Scope

두 번째 room, full campaign, procedural generation, persistent save,
inventory, 추가 ally/enemy archetype, boss, firearm, magic, telekinesis,
grapple, Momentum/Pip/HeavyAction, Policy Violation, upgrade/meta progression,
structural/breakable/glass destruction, runtime fracture, unrestricted ragdoll,
full rope/towing, multiplayer/account/leaderboard/analytics, paid API core
dependency, Hive SDK, Addressables, DOTS/Netcode, third-party behavior/rope/save
package, cutscene/long dialogue와 desktop-browser 안정화 전 mobile
optimization을 구현하지 않는다.

## Team and Contribution Policy

- 공식 팀은 최대 3명: 조윤성 + Designer A + Designer B
- 실제 산출물을 만들지 않은 사람은 기여자로 기록하지 않는다.
- 실제 기여는 날짜, 파일, 역할, 권리와 사용 허용을 ledger에 기록한다.
- 운영진 서면 허용 또는 공식 팀 재구성 없이 네 번째 사람의 contest-period
  신규 산출물을 제출작에 포함하지 않는다.
- Designer A/B는 각 art branch와 PR을 사용하며 `main` 또는 contest
  integration branch에 직접 push하지 않는다.

## Branch Policy

- Integration: `contest/game-builders-2026`
- Character/Animation: `art/characters-animation`
- Environment/UI/VFX: `art/environment-ui-vfx`
- Art branches는 최신 검증 contest commit에서 생성한다.
- Art PR target은 contest integration branch다.
- contest branch를 자동으로 `main`에 병합하지 않는다.

## Work Package Schedule

| Date | Target |
|---|---|
| 2026-08-06 | C0 branch, authority, art pipeline, Web build/browser qualification |
| 2026-08-07~09 | C1 interaction, C2 acquisition/carry/throw |
| 2026-08-10 | C3 reset/baseline diff |
| 2026-08-11~12 | C4 ally relay/catch/queued throw |
| 2026-08-13~14 | C5 Cargo Strap |
| 2026-08-15~16 | C6 Drawer/route/EXIT/win |
| 2026-08-17 | C7 Runner/failure/restart |
| 2026-08-18 | Feature complete target |
| 2026-08-19~22 | C8 art/UI/VFX/audio, optimization, internal playtest |
| 2026-08-23 | External playtest and blocker closure |
| 2026-08-24 | Release candidate and Feature Freeze |
| 2026-08-25 | Final submission and URL verification |
| 2026-08-26 | Emergency recovery only |

일정이 밀리면 art complexity를 줄이며 scope를 추가하지 않는다.

## Definition of Complete

- Chrome/Edge anonymous desktop browser에서 public URL이 안정적으로 실행
- 두 solution family로 full clear 각각 검증
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
