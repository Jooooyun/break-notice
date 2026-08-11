# BREAK NOTICE VFX Merge Handoff v001

- Status: `REVIEW READY / MERGE NOT AUTHORIZED / PR NOT AUTHORIZED`
- Work Package: `SHOWCASE-VFX-SOURCE-001`
- Relay: `EAR-CONTEST-0002-VFX-SOURCE-001`
- Source Branch: `codex/showcase-vfx`
- Immutable Base: `f51315aea91dd00293a4f5c387f1cb748865fd11`
- Intended Authority Lineage: `CONTEST-0002 / PRT-RM-002`
- Runtime Art: `0`
- Texture / Flipbook / Binary: `0`

## 1. Handoff 목적

이 문서는 향후 Human Owner가 branch 내용을 검토할 때 사용하는
documentation-only handoff다. 파일명에 `MERGE`가 포함되어 있어도 현재 Relay는
merge, cherry-pick, rebase, PR 또는 stable Showcase branch 변경을 승인하지
않는다.

## 2. 전달 내용

| Path | 역할 |
|---|---|
| `docs/10_contest/workstreams/vfx/SHOWCASE_VFX_DIRECTION_v001.md` | 물리적 VFX 방향, 형태 언어, accessibility와 후보 파일명 |
| `docs/10_contest/workstreams/vfx/SHOWCASE_VFX_CUE_MATRIX_v001.csv` | 잠긴 Gameplay Event별 cue mapping |
| `docs/10_contest/workstreams/vfx/SHOWCASE_VFX_TECHNICAL_DELTA_v001.md` | 확정되지 않은 semantic hook·performance·source delta |
| `docs/10_contest/workstreams/vfx/SHOWCASE_VFX_DELIVERY_CHECKLIST_v001.md` | Source/Preview/License와 미래 Runtime 검수 checklist |
| `docs/10_contest/workstreams/vfx/VFX_MERGE_HANDOFF_v001.md` | 이 branch의 검토·후속 권한 경계 |
| `ArtSource/VFX/Showcase/DELIVERY.md` | 비어 있는 Showcase VFX source delivery manifest |
| `ArtSource/VFX/Showcase/Source/.gitkeep` | Source placeholder |
| `ArtSource/VFX/Showcase/Licenses/.gitkeep` | License evidence placeholder |
| `ArtSource/VFX/Showcase/Preview/.gitkeep` | Preview placeholder |

## 3. Human Owner 결정 보존

- `CONTEST-0001` C0–C4 accepted foundation과 기존 Scene은 변경하지 않았다.
- `PRT-RM-002`의 mobile Ally, Q priority, PASS/BRACE/BREAK, TEAM READY,
  Drawer solution과 HANDOVER CRASH 정의를 바꾸지 않았다.
- VFX는 물리 결과를 보조하며 Gameplay/causal/objective state를 만들지 않는다.
- destruction, runtime fracture, gameplay debris, 새 damage/breakage가 없다.
- S1, C5, S9와 Main Product Gate는 닫힌 상태다.

## 4. 검토 시 확인할 항목

- cue ID가 중복되지 않는가
- 필수 15개 CSV 열이 exact header로 존재하는가
- 모든 hook과 timing 미확정값이 `PROPOSED / REVIEW REQUIRED`인가
- 모든 실제 Source, License, Texture, Flipbook과 Preview가 `MISSING`인가
- physical contact와 route/Exit Fact의 순서가 Gameplay Lock과 일치하는가
- color 외 shape/motion/text accessibility backup이 있는가
- magic, neon, explosion, fracture, screen-wide scope가 유입되지 않았는가
- exact allowlist 밖 변경이 없는가

## 5. 현재 누락과 risk

| 항목 | 상태 | 영향 |
|---|---|---|
| Editable VFX Source | `MISSING` | 이번 documentation package를 차단하지 않음 |
| Texture / Flipbook | `MISSING` | 실제 VFX Preview와 S9 통합은 시작 불가 |
| Preview | `MISSING` | visual quality·grayscale·occlusion review 불가 |
| Creator / License evidence | `MISSING` | 외부 Source 선택 또는 delivery 승인 불가 |
| Actual Runtime hooks | `PROPOSED / REVIEW REQUIRED` | Runtime integration 불가 |
| Duration / budgets | `PROPOSED / REVIEW REQUIRED` | WebGL 실측 전 승인 불가 |

## 6. Commit과 validation 기록 경계

실제 commit SHA와 push 결과는 commit 완료 뒤 최종 `WORK PACKAGE REPORT`에서
보고한다. 이 handoff를 commit SHA로 사후 수정해 두 번째 commit을 만들지 않는다.

필수 validation:

- exact allowlist diff
- tracked/staged/conflict hygiene
- CSV parsing과 header 검사
- cue ID duplicate `0`
- UTF-8 decode
- forbidden scope term/context audit
- `git diff --check`
- shared authority와 `Game/**` diff `0`

Unity Test, Web build, browser smoke와 console 검사는 이 source-documentation
Relay에서 실행하지 않는다.

## 7. 후속 작업

1. Human Owner가 이 direction/matrix를 `APPROVE`, `APPROVE WITH
   NON-BLOCKING NOTES` 또는 `REVISION REQUIRED`로 검토한다.
2. 승인 시 새 source-only Relay가 exact 후보 Source/Texture/Flipbook/Preview/
   License 파일 allowlist를 부여한다.
3. Source/Preview와 rights review 통과 후 별도 S9 Relay가 Runtime import와 Web
   qualification을 승인할 수 있다.

각 단계는 one-shot이며 이 branch 또는 문서에서 권한을 상속하지 않는다.

No new production asset blocks this work package. Graybox fallback remains active.
