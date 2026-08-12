# Showcase VFX Complete Production/Review Pack v003

Asset ID: `VFX-SHOWCASE-COMPLETE-PACK-003`

기준 SHA: `a86490a69014aebc81ce05661c1ad91e6bd831b8`

브랜치: `codex/showcase-vfx-complete-pack`

상태: `REVIEW / OWNER APPROVAL PENDING`

Gameplay/Runtime Integration Eligibility: `NONE`

이 패키지는 특정 회사나 게임의 자산·고유 연출을 복제하지 않는다. “캡콤급” 요청은 고급 상업 액션 게임 수준의 즉시성, 물리적 인과, 재질 구분, 계층화된 타이밍을 목표 품질로 해석했다.

## 1. Human Owner 결정 보존

- 기존 8종 중 1–7번은 `OLD v001 / OWNER SELECTED`를 유지한다.
- 8번 `EXIT / ROUTE ACTIVATION`만 `NEW v002 / OWNER SELECTED`를 유지한다.
- v003 추가 제작물 48종은 전부 `REVIEW / OWNER APPROVAL PENDING`이다.
- 기존 v001/v002 commit과 파일은 삭제·재작성하지 않았다.
- Gameplay Scene, gameplay runtime hook, pooling, damage/breakage/fracture 통합은 0이다.

## 2. 실제 제작 수량

- 전체 VFX effect: `48`
  - 잠긴 Showcase gameplay cue: `36`
  - 타격·재질 audition: `12`
- effect당 독립 layer: `3`
- 전체 Unity-ready layer PNG: `144`
  - 512×512 RGBA static: `69`
  - 2048×2048 RGBA 4×4 flipbook: `75`
- flipbook animation frame: `1,200` (`75 × 16`)
- Unity review material: `144`
- Unity review prefab: `48`
- 소스/런타임 후보 총 byte: `14,176,680`
- 후보 SHA-256 고유성: `144/144`

물리 effect는 모두 다음 3단 구조다.

1. `PrimaryTransient`: 접점과 normal에 붙는 날카로운 시작
2. `DirectionalBody`: 속도·재질 방향을 따르는 본체
3. `Settle`: 짧은 낙하·침전·잔향

상태 effect는 `Primary / Support / Settle`의 3 layer로 나누어 색상 외에도 형태와 움직임으로 읽히게 했다.

## 3. 잠긴 Gameplay cue 36종

### Acquisition / Throw

- Binder acquisition available
- Chair acquisition available
- Acquisition confirmed
- Throw charge stage 1
- Throw charge stage 2
- Throw charge full
- Throw physical release
- Binder physical contact
- Chair physical contact

### Ally / Supply / Catch

- Ally READY support
- Q binder supply accepted
- Q chair supply accepted
- Binder catch
- Chair floor-supported feed
- Chair wheel/floor contact
- Supply rejected
- Supply path rejected

### Team / Handover / Object

- PASS badge support
- BRACE badge support
- BREAK badge support
- TEAM READY
- HANDOVER CRASH commit
- Handover chair skid
- Handover chair-to-runner contact
- Handover runner-to-drawer contact
- Drawer causal impact
- Drawer route-state open
- EXIT/route activation

### Runner / System / UI

- Runner pressure telegraph
- Runner pressure contact
- Runner stagger/tag readability
- Runner grab pressure
- Reset start
- Reset complete
- UI confirmation
- UI rejection

## 4. 타격·재질 audition 12종

- Punch light / cloth contact
- Punch medium / cloth contact
- Punch strong / nonlethal contact
- Punch glancing contact
- Punch / forearm brace block
- Shoulder / body check
- Hand-to-desk miss contact
- Painted metal drawer contact
- Chair frame / plastic contact
- Binder / paper-edge contact
- Office object / floor drop
- Wall / partition contact

Punch 계열은 강도 차이를 단순 크기 배율로만 만들지 않았다. 접점 압축 폭, 주 방향, cloth fold fan, 잔류 입자의 무게와 비대칭을 각각 다르게 제작했다. 폭발·불꽃·피·화면 flash·원형 방사 spam은 사용하지 않았다.

## 5. Source와 provenance

외부 다운로드나 third-party asset은 사용하지 않았다.

Image generation 원본:

- `Source/VFX_Showcase_CompleteClothImpactBank_ImageGen_v003.png`
- `Source/VFX_Showcase_CompleteOfficeContactBank_ImageGen_v003.png`

Straight-alpha 파생 source:

- `Source/VFX_Showcase_CompleteClothImpactBank_AlphaExtract_v003.png`
- `Source/VFX_Showcase_CompleteOfficeContactBank_AlphaExtract_v003.png`

프로젝트 내부에서 재사용한 v002 원본/straight-alpha bank:

- `Source/VFX_Showcase_PremiumDustBank_ImageGen_v002.png`
- `Source/VFX_Showcase_PremiumDustBank_AlphaExtract_v002.png`
- `Source/VFX_Showcase_PremiumRubberBank_ImageGen_v002.png`
- `Source/VFX_Showcase_PremiumRubberBank_AlphaExtract_v002.png`
- `Source/VFX_Showcase_PremiumPaperBank_ImageGen_v002.png`
- `Source/VFX_Showcase_PremiumPaperBank_AlphaExtract_v002.png`

재현 가능한 deterministic 제작 source:

- `Source/build_showcase_vfx_complete_v003.py`

분류:

- AI 원본: `ORIGINAL_AI_GENERATED_SOURCE`
- alpha 파생 원본: `ORIGINAL_AI_GENERATED_DERIVATIVE_SOURCE`
- 최종 후보: `ORIGINAL_DETERMINISTIC_SOURCE` 또는 `ORIGINAL_AI_ASSISTED_DETERMINISTIC_DERIVATIVE`
- 외부 source/license: `NONE`
- 권리 근거: `PROJECT ORIGINAL; NO THIRD-PARTY SOURCE; HUMAN OWNER AUTHORIZED CREATION`

모든 binary의 SHA-256, byte, dimensions, alpha 수치, frame grid/order, source 분류와 rights basis는 다음 UTF-8 CSV에 기록한다.

- `Licenses/VFX_Showcase_Complete_Provenance_Ledger_v003.csv`

Ledger row: `188` (source, 144 candidates, previews, reproducible builder 포함)

## 6. Unity review 구성

Unity Hub project:

`C:\Users\jhbjm\Desktop\break-notice-wt-vfx-complete-pack\Game`

Scene:

`Assets/_Project/Scenes/90_Review/LOCAL_REVIEW_ONLY_ShowcaseVFX.unity`

Menu:

`BREAK NOTICE > Contest > Prepare Isolated Showcase VFX Review`

Play를 누르면 별도 클릭 없이 다음이 작동한다.

- Owner-selected core 1페이지
- v003 cue/impact 대형 audition 12페이지
- gameplay-camera impact context 1페이지
- 합계 `14`페이지 자동 순회, 페이지당 `6초`
- 첫 OLD 선택 페이지 원샷 `1.2초` review 재발사
- 각 v003 station의 왼쪽 대형 composite와 오른쪽 solo layer 1/2/3 동시 비교
- composite `48/48`, solo `144/144`
- gameplay-camera context effect `8/8`

Review 전용 bootstrap은 `ContestReview` component 하나뿐이며 gameplay assembly/state/input/hook 의존성이 없다.

## 7. Human Owner preview

전체 소스 미리보기:

- `Preview/Complete_v003/VFX_Showcase_Complete_AllCueContactSheet_v003.png`
- `Preview/Complete_v003/VFX_Showcase_Complete_ImpactLabContactSheet_v003.png`
- `Preview/Complete_v003/VFX_Showcase_Complete_GrayscaleAlphaAudit_v003.png`

Unity 페이지:

- `Preview/Complete_v003/VFX_Showcase_Complete_UnityPage_00_v003.png` — Owner-selected OLD 7 + NEW EXIT
- `Preview/Complete_v003/VFX_Showcase_Complete_UnityPage_01_v003.png` ~ `UnityPage_12` — 48종 대형 composite/solo audition
- `Preview/Complete_v003/VFX_Showcase_Complete_UnityPage_13_v003.png` — gameplay-camera impact context
- 각 페이지와 동일 이름의 `_Grayscale.png` — 색상 제거 가독성
- `Preview/Complete_v003/VFX_Showcase_Complete_ActualPlay_Page00_v003.png` — 실제 Play 증거
- `Preview/Complete_v003/VFX_Showcase_Complete_FixedSceneView_v003.png` — 정면/upright Scene View 증거

Owner 확인 항목은 effect별 `Keep / Revise / Cut`, alpha edge, 실제 scale, loop cadence, 방향성, gameplay readability다.

## 8. Alpha / overdraw risk

- 모든 후보는 straight alpha RGBA다.
- transparent pixel RGB는 정리했고 alpha fringe/black halo 방지 검사를 포함한다.
- static/각 flipbook cell의 투명 margin은 최소 `24px`다.
- magenta, neon cyan, chroma green spill은 후보 전체에서 0을 요구한다.
- 물리 effect는 동시에 정확히 3 layer다. fullscreen quad, bloom stack, screen flash는 없다.
- 높은 상대 overdraw 후보는 cloth strong contact의 DirectionalBody, chair skid의 rubber body, paper burst의 page fan이다. 모두 짧은 lifetime과 작은 world-space 범위로 제한했다.
- 최종 camera/target hardware profiling과 pooling/rate limit은 후속 Runtime integration Relay가 필요하다.

## 9. 기술 검증 상태

- Unity Editor `6000.3.20f1` compile: `PASS`
- production/review replacement: `48/48`
- Scene page: `14/14`
- owner-selected Play active/live: `8/8`
- complete composite Play: `48/48`
- solo layer Play: `144/144`
- gameplay-camera context Play: `8/8`
- missing texture/material/shader: `0`
- magenta/missing shader: `0`
- source/runtime candidate byte identity: `144/144`
- PNG signature/dimension/RGBA/alpha/margin: `144/144`
- flipbook grid/frame order/unique frame: `75/75`, `1,200/1,200`
- duplicate SHA/GUID/path: `0`
- orientation/front/up/positive determinant: `PASS`
- camera frustum pages: `14/14`
- focused EditMode: `136/136 PASS`
- full EditMode: `201/201 PASS`
- full PlayMode: `59/59 PASS`
- actual Play contract: `selected 8/8, complete 48/48, solo 144/144, gameplay context 8/8`
- Unity console hard error: `0`
- Gameplay integration: `0`
- Web build/browser: `NOT RUN / OUT OF SCOPE`

시각 품질은 자동 승인하지 않는다. 모든 v003 결과는 Human Owner 판정 전까지 `REVIEW / OWNER APPROVAL PENDING`이다.

## 10. 남은 Runtime integration

- 실제 gameplay event/hook 연결
- target/contact normal/velocity binding
- gameplay camera 거리별 최종 scale/lifetime 조정
- pooling, simultaneous rate limit, Reset cancellation
- 실제 target occlusion과 hardware overdraw profiling
- Gameplay Scene/Prefab 연결

이 항목들은 이번 source/review package에 포함되지 않았다.

No new production asset blocks this work package. Graybox fallback remains active.
