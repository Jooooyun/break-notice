# CR-0003 — Game Builders 2026 Contest Sprint

- Requester: `Human Owner — 조윤성`
- Date: `2026-08-06`
- Change Class: `C3 / CONTEST-BOUNDED PRODUCT BRANCH`
- Status: `OWNER-APPROVED / APPLIED`
- Execution Ticket: `CONTEST-0001`
- Execution Relay: `EAR-CONTEST-0001-001`
- Exact Baseline: `28b7f70c4b6299159afe5b99ea7b98d1e5399352`
- Exact Branch: `contest/game-builders-2026`
- Gate:
  `OPEN — CONTEST-0001 ONLY / GAME BUILDERS 2026 SINGLE-ROOM BROWSER PROTOTYPE`

## 요청

공모전 전 장기 제품 기준선과 기존 G4-A evidence scene을 보존하면서,
`BREAK NOTICE: ZERO DEFECT`를 OpenAI Game Builders Seoul 2026 Track 1에
제출 가능한 단일 방 브라우저 프로토타입으로 만든다.

## 승인 범위

- 공모전 기준 태그와 전용 통합·아트 브랜치
- 공모전 최소 문서, 기여자·라이선스·빌드·아트 기록
- 기존 승인 Visual Canon과 Production Support의 제작 기준 연결
- exact Unity `6000.3.20f1` Web Build Support와 결정적 로컬 자동화
- 기존 locomotion·camera·logical input을 보존한 별도 Contest Scene
- CONTEST-0001의 C0~C9 work package
- 공모전 범위 안의 one-purpose commit과 일반 push

CONTEST-0001은 여러 work package와 commit을 허용하지만, 아래 잠긴
공모전 범위를 벗어나는 권한으로 상속하거나 재사용할 수 없다.

## 공모전 범위

- authored room `1`
- CHR_00 `1`, CHR_01 fixed ally `1`, 동일 Compliance Runner `2`
- Binder `3`, Chair `2`, Drawer `1`, Copier recognition-only `1`
- Move, Look, Aim, InteractPrimary, Throw, CallAlly, StrapRelease,
  ResetEncounter의 정확한 여덟 logical action
- Drawer를 이동해 Side Route를 열고 EXIT에 도달하는 objective `1`
- Direct/Chained Physical Throw와 Ally Chair + Cargo Strap Redirect의
  물리적으로 구분되는 두 solution family
- browser build, test, playtest, presentation과 submission evidence

정확한 Contest room layout과 encounter sequence는 Prototype
Representation이며 Full-game Canon을 자동 변경하지 않는다.

## 금지 범위

두 번째 room, 새 ally/enemy archetype, boss, firearm, magic,
telekinesis, grapple, Momentum/Pip/HeavyAction, Policy Violation,
upgrade/meta progression, structural/breakable/glass destruction,
runtime fracture, unrestricted ragdoll, full rope/towing, multiplayer,
account/leaderboard/analytics, paid API gameplay dependency, 승인 없는
package/service, mobile-first optimization과 full campaign은 금지한다.

## 기존 기준선 보호

- `main`은 장기 BREAK NOTICE 제품 기준선으로 유지한다.
- contest branch를 자동으로 `main`에 병합하지 않는다.
- 기존 공개 history, tag와 approved reference를 재작성하지 않는다.
- `G4A_LocomotionCameraGraybox`는 immutable task evidence로 보존한다.
- 공모전 기간 작업은 pre-challenge/submission tag diff로 분리한다.

## 기존 untracked Human Owner 작업

2026-08-06 Human Owner가 식별한 기존 untracked 작업은 삭제·덮어쓰기·
이동하지 않는다.

- `.claude/settings.local.json`: local configuration / protected
- `docs/05_production/PROJECT_AND_STORY_INTEGRATION_REVIEW_v001.md`:
  draft advisory / non-canon / protected
- `docs/05_production/ZERO_DEFECT_REVIEW_AND_STORY_v002.md`:
  draft advisory / non-canon / protected
- `references/exploration/*_REVIEW.png` 23개: 원본 보존, 승인 기록과
  byte identity에 따라 분류, 선택적 승인본 경로만 제작 기준으로 사용

## 종료와 Rollback

Relay는 최종 제출, `2026-08-26` 종료, Owner 철회, branch/baseline
identity 상실, 승인되지 않은 package/service 또는 scope expansion 필요,
데이터·라이선스·기여자·보안·개인정보 위험에서 만료한다.

Rollback은 contest branch를 중단하는 것이며 `main`, 기존 tag, 승인 Visual
Canon 또는 G4-A evidence scene을 변경하지 않는다.
