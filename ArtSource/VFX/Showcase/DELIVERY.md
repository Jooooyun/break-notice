# Showcase VFX Premium Revision v002 Delivery

Asset ID: `VFX-SHOWCASE-PREMIUM-REVISION-002`

기준선: `de866ef3bae2af137f65e349caa06cd17360065e`

브랜치: `codex/showcase-vfx-premium-revision`

상태: `REVIEW / OWNER APPROVAL PENDING`

Gameplay/Runtime Integration Eligibility: `NONE`

## 1. Human Owner 판정 상태

- 기존 VFX 8종(v001): `REVISION_REQUIRED / OWNER NOT APPROVED`
- 기존 Unity Review package: `TECHNICALLY FUNCTIONAL, VISUALLY REJECTED`
- Premium replacement v002: `REVIEW / OWNER APPROVAL PENDING`
- v002는 기술 검수 후보이며 `OWNER APPROVED`, `FINAL`, `LOCKED`가 아니다.
- 기존 commit/history와 v001 파일은 비교 증거로 보존했다.

## 2. Premium replacement 8/8 및 레이어 구성

각 효과는 세 개의 독립 레이어를 Built-in-compatible ParticleSystem에서 합성한다.
물리 효과는 짧은 primary transient, 방향성 body, settle/residue 순서를 유지한다.

| Station | Effect | Layer 1 | Layer 2 | Layer 3 | 상태 |
|---:|---|---|---|---|---|
| 1 | Interaction Target Readability | EdgeTrace | ContactBracket | MaterialWink | REVIEW |
| 2 | Catch Confirm | PrimaryCompression | PaperSnap | DirectionalSettle | REVIEW |
| 3 | Invalid/Rejected Action | ReticleInterruption | TargetDropout | ReverseEcho | REVIEW |
| 4 | Ally READY | GazeTick | PostureCue | FootContact | REVIEW |
| 5 | Light Office Contact / Binder Impact | PrimaryTransient | DirectionalDust | FiberSettle | REVIEW |
| 6 | Chair Wheel Contact / Skid | CasterBite | RubberBody | ContactSettle | REVIEW |
| 7 | Paper Material Burst | PrimarySnap | PageFan | FlutterSettle | REVIEW |
| 8 | EXIT / Route Activation | PracticalLamp | SignHardware | RouteEdge | REVIEW |

상태 효과 12개는 각각 `512x512 RGBA` single texture다. 물리 효과 12개는 각각
`2048x2048 RGBA`, `4x4`, `512x512/cell` flipbook이다. 총 Runtime candidate는
24개이며 4x4 animation frame은 합계 192개다.

Unity frame order는 `UNITY_WHOLE_SHEET_LOWER_LEFT_ROW_MAJOR`로 고정했다.
각 flipbook은 16/16 frame이 서로 다르고 모든 cell이 비어 있지 않다.

## 3. 물리 효과 deterministic variation

Catch, Binder impact, Chair skid, Paper burst 각각에 다음 세 변주를 제공한다.

- Variation A: 기준 방향, scale `1.00`, layer delay `0.000s`
- Variation B: 방향 `+14°`, scale `0.82`, layer delay `0.035s`, layer offset 적용
- Variation C: 방향 `-12°`, scale `1.20`, layer delay `0.060s`, 반대 layer offset 적용

변주는 같은 stock flipbook을 단순 반복한 것이 아니라 회전, 크기, layer 시차와
레이어 위치 관계를 결정적으로 바꾼 review audition이다. Gameplay randomization이나
Runtime hook은 포함하지 않는다.

## 4. Source와 제작 방식

Self-created originals만 사용했다. 외부 다운로드, 제3자 asset, 로고, 상표, 타사
게임 source, 저작권 보호 source는 사용하지 않았다.

Image generation source:

- `Source/VFX_Showcase_PremiumDustBank_ImageGen_v002.png`
- `Source/VFX_Showcase_PremiumRubberBank_ImageGen_v002.png`
- `Source/VFX_Showcase_PremiumPaperBank_ImageGen_v002.png`

Straight-alpha element bank:

- `Source/VFX_Showcase_PremiumDustBank_AlphaExtract_v002.png`
- `Source/VFX_Showcase_PremiumRubberBank_AlphaExtract_v002.png`
- `Source/VFX_Showcase_PremiumPaperBank_AlphaExtract_v002.png`

Reproducible deterministic build source:

- `Source/build_showcase_vfx_premium_v002.py`

Alpha extraction은 imagegen skill의 chroma-key workflow를 사용했고 `--auto-key border`,
`--soft-matte`, `--transparent-threshold 12`, `--opaque-threshold 220`, `--despill`을
적용했다. 출력은 straight alpha이며 투명 pixel의 RGB 오염과 green spill을 제거했다.
CLI/model downgrade는 발생하지 않았다.

### ImageGen prompt record

세 요청 모두 reference image 없이 OpenAI built-in image generation으로 새로 생성했다.

`IG-VFX-SHW-PREMIUM-002-A`

```text
Create a production game-VFX source bank on a perfectly flat uniform chroma-key green #00FF00 background. Arrange sixteen clearly separated, isolated, restrained office-contact dust and paper-fiber elements in a clean 4 by 4 specimen grid with generous green space between every element and around all canvas edges. Include low directional dust wisps, thin compression streaks, tiny warm-gray particulate clusters, short floor-hugging settle trails, and subtle paper-fiber accents. Use realistic muted warm gray, cool gray and pale beige material values with soft translucent edges and internal texture. Each element must have a distinct asymmetrical silhouette and a clear physical direction, suitable for constructing layered binder-contact and catch-confirm flipbooks. No floor, horizon, cast shadow, text, icons, logos, watermark, fire, explosion, smoke column, opaque cartoon puff, spark shower, neon, cyan, magenta, glow core, radial burst, celebration confetti, object, hand, binder, chair, crop or overlap.
```

`IG-VFX-SHW-PREMIUM-002-B`

```text
Create a production game-VFX source bank on a perfectly flat uniform chroma-key green #00FF00 background. Arrange sixteen clearly separated, isolated chair-caster and rubber-skid contact elements in a clean 4 by 4 specimen grid with generous green space between every element and around all canvas edges. Include paired low caster bites, narrow directional rubber scuffs, fine gray-brown floor dust ribbons, compressed contact smears, and short settling grains. Keep every shape extremely low to the ground, lateral, restrained, asymmetrical and physically caused by wheeled office-chair contact. Use muted charcoal, neutral gray-brown and tiny cold-metal highlights only, with soft translucent edges and material texture. No chair, wheel, floor plane, horizon, cast shadow, text, icons, logo, watermark, fire, explosion, smoke cloud, opaque cartoon puff, sparks, neon, cyan, magenta, glow core, radial burst, crop or overlap.
```

`IG-VFX-SHW-PREMIUM-002-C`

```text
Create a production game-VFX source bank on a perfectly flat uniform chroma-key green #00FF00 background. Arrange twenty clearly separated, isolated office-paper elements in a clean 5 by 4 specimen grid with generous green space between every element and around all canvas edges. Include irregular torn paper fragments, folded corners, thin edge-on slivers, small page fans, curled strips and asymmetric flutter shapes. Use off-white, recycled beige, cool light gray and a few muted blue-gray paper tones with subtle fiber edges, thickness and fold shading. Every piece must be physically plausible and individually maskable for layered catch, binder-impact and paper-burst flipbooks. No writing, printed lines, letters, numbers, logos, watermarks, folder cover, binder, clip, staple, hand, dust cloud, fire, explosion, sparks, neon, cyan, magenta, glow, celebration confetti, cast shadow, crop, overlap or stock icon shape.
```

## 5. Provenance와 권리

원산지 분류:

- AI source 3개: `ORIGINAL_AI_GENERATED_SOURCE`
- alpha element bank 3개: `ORIGINAL_AI_GENERATED_DERIVATIVE_SOURCE`
- 24개 Runtime candidate: `ORIGINAL_DETERMINISTIC_SOURCE` 또는
  `ORIGINAL_AI_ASSISTED_DETERMINISTIC_DERIVATIVE`
- review preview 8개: `DETERMINISTIC_REVIEW_PREVIEW`
- 제3자 source/license: `NONE`

상업 이용 및 저장소 재배포 근거:
`PROJECT ORIGINAL; NO THIRD-PARTY SOURCE; HUMAN OWNER AUTHORIZED CREATION`.

39개 source/candidate/preview/build 항목의 SHA-256, byte, dimensions, alpha metrics,
frame order, rights basis와 review 상태는 다음 UTF-8 CSV에 기록했다.

- `Licenses/VFX_Showcase_Premium_Provenance_Ledger_v002.csv`

Runtime Unity 영역의 24개 PNG는 ArtSource 후보와 `24/24 byte-identical`이며
focused EditMode test가 동일 SHA-256을 검증한다.

## 6. Unity review delivery

Unity project:
`C:\Users\jhbjm\Desktop\break-notice-wt-vfx-premium-revision\Game`

Scene:
`Assets/_Project/Scenes/90_Review/LOCAL_REVIEW_ONLY_ShowcaseVFX.unity`

Menu:
`BREAK NOTICE > Contest > Prepare Isolated Showcase VFX Review`

Unity review 구성:

- Runtime PNG import: 24
- Built-in `Particles/Standard Unlit` 기반 Material: 24 effect layer + 7 review environment
- Composite Prefab: 16 (status 4 + physical A/B/C 12)
- Composite station: 8/8
- Solo layer view: 24/24
- Physical variation audition: 12/12
- Old vs new comparison: 8/8
- Gameplay-camera-equivalent zone: 8/8
- Gameplay Scene/Prefab/runtime hook 연결: 0

## 7. Human Owner inspection preview

- `Preview/Premium_v002/VFX_Showcase_Premium_SourceContactSheet_v002.png`
- `Preview/Premium_v002/VFX_Showcase_Premium_LayerBreakdown_Source_v002.png`
- `Preview/Premium_v002/VFX_Showcase_Premium_GrayscaleAlphaAudit_Source_v002.png`
- `Preview/Premium_v002/VFX_Showcase_Premium_OldVsNew_v002.png`
- `Preview/Premium_v002/VFX_Showcase_Premium_LayerBreakdown_v002.png`
- `Preview/Premium_v002/VFX_Showcase_Premium_PhysicalVariations_v002.png`
- `Preview/Premium_v002/VFX_Showcase_Premium_GameplayCamera_v002.png`
- `Preview/Premium_v002/VFX_Showcase_Premium_GrayscaleReadability_v002.png`

Source contact sheet와 다섯 Unity capture를 모두 육안 검수했다. 확인 항목은 8/8
가시성, 합성/solo 차이, A/B/C 변주, straight-alpha edge, 검은 halo, magenta,
green spill, 화면 점유, grayscale silhouette다.

## 8. Alpha/overdraw risk note

- 모든 후보에 partial alpha가 있어 hard-alpha silhouette를 피했다.
- magenta, neon cyan, green spill pixel은 후보 24개 전체에서 각각 0이다.
- 투명 여백 최소값은 static/flipbook cell 모두 32px 이상이다.
- 투명 pixel의 pure-black RGB를 제거해 straight-alpha mip edge의 black halo 위험을 줄였다.
- 한 composite의 동시 layer는 정확히 3개다. 큰 fullscreen quad, screen flash와 bloom
  stack은 없다.
- 가장 높은 overdraw 후보는 Binder `DirectionalDust`, Chair `RubberBody`, Paper
  `PageFan`이다. 좁고 짧은 lifetime으로 제한했지만 최종 Runtime camera/target
  hardware profiling은 아직 수행하지 않았다.
- Chair skid는 현실적인 낮은 명도를 지켜 grayscale에서 가장 약하게 보인다.
  Catch transient도 camera distance에서 작게 느껴질 수 있으므로 두 항목은 Human Owner가
  Keep/Revise/Cut로 직접 판정해야 한다.

## 9. Validation

- Unity Editor `6000.3.20f1` compile: `PASS`
- Premium replacement: `8/8`
- Runtime PNG ArtSource→Unity byte identity: `24/24 PASS`
- PNG signature/dimensions/RGBA/alpha coverage: `24/24 PASS`
- Flipbook grid/frame order/non-empty/unique: `12/12 PASS`, `192/192 frames`
- Importer contract: `24/24 PASS`
- Material and shader reference contract: `PASS`
- Missing texture/material/shader: `0`
- Magenta: `0`
- Alpha fringe/black halo signature: `0`
- Duplicate GUID/path: `0`
- Focused EditMode: `127/127 PASS`
- Full EditMode: `192/192 PASS`
- Full PlayMode: `59/59 PASS`
- Review capture hard console error: `0`
- Gameplay integration: `0`
- Web build/browser: `NOT RUN / FORBIDDEN`

시각 품질은 자동 PASS로 판정하지 않는다. Human Owner가 각 station을 실제 camera에서
Keep / Revise / Cut 판정하기 전까지 모든 v002 결과는
`REVIEW / OWNER APPROVAL PENDING`이다.

## 10. Runtime integration remaining

다음 항목은 이번 revision에 포함하지 않았고 새 Human Owner Relay가 필요하다.

- Gameplay event/hook 연결
- Runtime pooling과 rate limiting
- 실제 target/normal/velocity data binding
- 실제 카메라 거리별 scale과 lifetime 최종 조정
- occlusion, Reset cancellation과 WebGL overdraw profiling
- Gameplay Scene/Prefab 변경

No new production asset blocks this work package. Graybox fallback remains active.
