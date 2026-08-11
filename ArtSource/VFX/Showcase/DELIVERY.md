# Showcase VFX Production Source Delivery

Asset ID: `VFX-SHOWCASE-SOURCE-PACK-001`

Asset Name: `BREAK NOTICE Showcase P0/P1 Production Source Pack v001`

Creator: `OpenAI Codex for the BREAK NOTICE Human Owner`

Contribution Date: `2026-08-12 KST`

Branch / Starting Baseline:
`codex/showcase-vfx / 578f2282bcace5f25e2d1603e66ee256ac8ac347`

Current Status:
`DELIVERED / PENDING HUMAN OWNER TECHNICAL REVIEW / NOT OWNER APPROVED / NOT RUNTIME INTEGRATED`

## 1. Scope and rights classification

- Original deterministic vector/raster work: `YES`
- Original AI-generated natural-material source: `YES — THREE SOURCE IMAGES`
- Purchased or downloaded third-party content: `NO`
- Open-license third-party content: `NO EXTERNAL SOURCE USED`
- Reference-image input to generation: `NONE`
- Copyrighted logo, printed text or brand mark: `NONE`
- Exact rights basis:
  `PROJECT ORIGINAL; NO THIRD-PARTY LICENSE APPLIES; HUMAN OWNER AUTHORIZED CREATION`
- Commercial use:
  `AUTHORIZED FOR BREAK NOTICE PROJECT AND CONTEST MATERIALS BY HUMAN OWNER`
- Redistribution:
  `AUTHORIZED WITH PROJECT DELIVERY; NO EXTERNAL SOURCE RESTRICTION`
- Derivative work: `AUTHORIZED`
- Final rights/technical approval: `HUMAN OWNER REVIEW REQUIRED`

The approved Production Visuals and tracked art/camera documents were used only as
directional Canon. No pixels, logos, proprietary third-party source files or other
lane assets were copied into this pack.

Machine-readable per-file provenance, SHA-256, byte count, dimensions and alpha
metrics are recorded in:

`ArtSource/VFX/Showcase/Licenses/VFX_Showcase_Provenance_Ledger_v001.csv`

## 2. Unity-ready delivery candidates

These files contain usable visual content. They are source-lane candidates only;
no Unity `.meta`, Material, Prefab, Particle System, VFX Graph, Shader Graph or
Runtime hook was created.

| Inspection index | File | Role | Format | SHA-256 |
|---:|---|---|---|---|
| 1 | `Textures/T_VFX_Showcase_InteractionSoftRing_v001.png` | interaction target soft ring/highlight | 512×512 RGBA | `3cdefac76ca1fcde0d35d58b602e9c6ea6a4f5abaad43fff49b0b0c5add91841` |
| 2 | `Textures/T_VFX_Showcase_PickupCatchConfirm_v001.png` | valid pickup/catch confirmation | 512×512 RGBA | `01c9410152bf89e0ba35c0897d879ba6144bb80a307ef349ccb3d504f7b7b0df` |
| 3 | `Textures/T_VFX_Showcase_InteractionRejected_v001.png` | invalid/rejected interaction pulse | 512×512 RGBA | `79a350a04630a07f06d9af12c52eb7202cd091615a9452577208e6ee709ca02a` |
| 4 | `Flipbooks/T_VFX_Showcase_LightContactDust_4x4_v001.png` | Light object contact dust | 2048×2048 RGBA, 4×4 | `db85987ec72d905fdd75dc6ea20a8598f0075493b4e2c71fde1c744beda70f33` |
| 5 | `Flipbooks/T_VFX_Showcase_ChairWheelContact_4x4_v001.png` | Medium Chair wheel/contact dust with paired grounded strokes | 2048×2048 RGBA, 4×4 | `4e528117f430367a1f3b69df330145584f5b440c54e80e63c69696ac538d0970` |
| 6 | `Flipbooks/T_VFX_Showcase_RestrainedPaperBurst_4x4_v001.png` | restrained Binder paper burst | 2048×2048 RGBA, 4×4 | `07e97069f6c25bc077971ffda8dd34add53b795522039e459df655d1e120af76` |
| 7 | `Textures/T_VFX_Showcase_AllyReadySupport_v001.png` | pulse/support frame around the existing Ally READY exclamation | 512×512 RGBA | `73f389e9d608274eda96a9a7f497163e5e0c9b7c7675759647840ca982e93d27` |
| 8 | `Textures/T_VFX_Showcase_ExitActivation_v001.png` | Exit activation/readability pulse | 512×512 RGBA | `14e9c3daeb0704e64092e23614cb92a40328b95945c39930bd702c4c8ecdc398` |

All three flipbooks use row-major order from frame `0` at upper-left to frame
`15` at lower-right. Every frame is exactly 512×512, non-empty, unique and has
at least 20 transparent pixels of per-cell outer margin.

## 3. Editable and reproducible sources

Deterministic vector sources:

- `Source/VFX_Showcase_InteractionSoftRing_v001.svg`
- `Source/VFX_Showcase_PickupCatchConfirm_v001.svg`
- `Source/VFX_Showcase_InteractionRejected_v001.svg`
- `Source/VFX_Showcase_AllyReadySupport_v001.svg`
- `Source/VFX_Showcase_ExitActivation_v001.svg`

Natural-material generation sources:

- `Source/VFX_Showcase_LightContactDust_ImageGen_v001.png`
- `Source/VFX_Showcase_ChairWheelDust_ImageGen_v001.png`
- `Source/VFX_Showcase_PaperElements_ImageGen_v001.png`

The three ImageGen files are preserved 1254×1254 RGB chroma-key intermediates for
reproducibility. They are not Unity-ready textures. The delivered flipbooks use
locally recovered straight transparent alpha, neutralized key spill and clean
transparent borders.

Reproducible build source:

`Source/build_showcase_vfx_pack_v001.py`

Required workspace libraries: Python 3.13, Pillow 12.1.1, NumPy 2.4.4 and SciPy
1.17.1. Rebuild/validate from repository root with:

```powershell
python ArtSource/VFX/Showcase/Source/build_showcase_vfx_pack_v001.py
```

The script writes only within `ArtSource/VFX/Showcase/{Source,Textures,Flipbooks,Preview,Licenses}`.

## 4. Image generation provenance and exact prompts

Generation mode: `OpenAI built-in image generation through the imagegen skill`.
Each request was a new, independent generation with no reference image and no
third-party source input. The selected generated originals remain unchanged in
the Codex generated-image directory; the byte-identical selected copies are
preserved under `Source/`.

### IG-VFX-SHW-001 — Light contact dust

```text
Use case: production-ready game VFX source sprite.
Asset type: isolated natural-material source image for a restrained office-floor contact dust effect.
Primary request: create one compact, low-profile puff of fine dry gray-beige dust caused by a light file binder touching a dusty office floor. The motion must feel small, physical, and close to the ground, with several soft particulate lobes and a few tiny grains, suitable as source material for a short 4x4 flipbook.
Scene/backdrop: perfectly flat, uniform solid chroma-key green #00FF00 background filling the entire canvas; no horizon, floor plane, gradient, texture, shadow, or ambient environment.
Style/medium: realistic natural dust sprite source, clean game-production element, restrained corporate-office material response; not concept art and not an illustration panel.
Composition: one centered dust puff occupying about 45% of canvas width and no more than 24% of canvas height; generous empty green margin on every edge; asymmetric horizontal direction with the contact origin near lower center; subject fully visible and uncropped.
Color/lighting: dust only in muted warm gray, cool gray, and very subtle beige; matte, soft, internally shaded enough to retain form after alpha extraction; absolutely no green within the dust subject.
Constraints: no fire, explosion, smoke column, fog bank, sparks, embers, glowing energy, rocks, glass, paper, object, text, letters, numbers, logo, watermark, border, frame, cast shadow, or magenta. No photoreal scene. Keep the effect sparse and gameplay-readable.
```

Selected source SHA-256:
`5d3a2704941467028bd8912b43e4f255b8e0fa2b6c97277feddf427b19524436`

### IG-VFX-SHW-002 — Chair wheel/contact dust

```text
Use case: production-ready game VFX source sprite.
Asset type: isolated natural-material source image for a grounded office-chair wheel contact effect.
Primary request: create one thin, lateral trail of fine gray-brown office-floor dust caused by wheeled office-chair casters rolling and skidding while the chair remains fully floor-supported. Include two visually separated low puffs and a faint narrow dust streak between them, with a few tiny grains; suitable as source material for a short 4x4 flipbook.
Scene/backdrop: perfectly flat, uniform solid chroma-key green #00FF00 background filling the entire canvas; no horizon, floor plane, gradient, texture, shadow, or ambient environment.
Style/medium: realistic natural dust sprite source, restrained physical office-material response, clean game-production element; not concept art.
Composition: centered horizontally in the lower-middle, occupying about 62% of canvas width and no more than 16% of canvas height; strongly horizontal left-to-right flow, generous empty green margin on every edge, fully visible and uncropped.
Color/lighting: muted cool gray, neutral gray-brown, and tiny beige dust variation only; matte and soft with enough internal value variation for alpha extraction; absolutely no green in the subject.
Constraints: no chair, wheel, caster, floor, object, fire, explosion, smoke column, fog bank, sparks, embers, glowing energy, rocks, paper, glass, text, letters, numbers, logo, watermark, frame, cast shadow, or magenta. No upward blast. Keep it sparse, low, directional, and gameplay-readable.
```

Selected source SHA-256:
`fe7722f615a7e9ab8528072c5baef59e95c34b497e21989ac90392a7f2ba5d22`

### IG-VFX-SHW-003 — Restrained paper elements

```text
Use case: production-ready game VFX source sprite bank.
Asset type: isolated natural office-paper elements for a restrained paper-contact burst flipbook.
Primary request: create a sparse collection of 14 separate small office-paper scraps and thin paper slivers, as if a file binder made a modest physical contact. Shapes should include a few irregular torn rectangles, narrow strips, folded corner silhouettes, and edge-on slivers. Every piece must be clearly separated from every other piece so it can be masked independently.
Scene/backdrop: perfectly flat, uniform solid chroma-key green #00FF00 background filling the entire canvas; no horizon, floor plane, gradient, texture, shadow, or environment.
Style/medium: realistic but simplified game VFX source elements, restrained office-material response, matte paper with subtle thickness and gentle folds; not a scene and not concept art.
Composition: elements distributed within the central 64% of the canvas, generous empty green margin on all edges, no overlap between pieces, no crop, no large sheet dominating the image. Preserve clear negative space.
Color/lighting: paper only in off-white, cool light gray, very pale recycled beige, and a few muted blue-gray pieces; low contrast internal fold shading; absolutely no green within subjects.
Constraints: absolutely no writing, printed lines, typography, letters, numbers, logos, watermarks, stamps, brand marks, folders, binder covers, clips, staples, fire, explosion, dust cloud, smoke, sparks, glow, magenta, cast shadow, border, or frame. Keep the set sparse and physically plausible, not a confetti celebration.
```

Selected source SHA-256:
`16853debb2a3d1315a8feba79e36b68af33e3c0cae567730bcb062ac01ab573a`

## 5. Human Owner inspection previews

- `Preview/VFX_Showcase_ProductionSourcePack_ContactSheet_v001.png`
  - 2048×1152 RGBA
  - inspection order is the `1`–`8` order in the delivery table
  - SHA-256: `5526bd2f9375426dcf79f8427317d81a90804c18651d6a42e49185fdb27d59b2`
- `Preview/VFX_Showcase_GrayscaleAlphaReadability_v001.png`
  - 2048×1920 RGBA
  - each numbered asset is shown as a grayscale composite followed by its alpha mask
  - SHA-256: `7071f42a433084cd8abf3c5341f74da339458e558add65e9c928f6a924bd28a1`

Both previews are review composites only and are not Runtime textures.

## 6. Validation result

- PNG signature: `PASS — 13/13`
- Unity-ready candidate dimensions: `PASS — 5×512², 3×2048²`
- Unity-ready color mode: `PASS — RGBA 8-bit`
- Alpha coverage and partial-alpha pixels: `PASS — 8/8`
- Outer transparent margin: `PASS — minimum 24 px per delivery image`
- Flipbook grid: `PASS — 4×4 / 512² cells / row-major`
- Flipbook cell content: `PASS — 48/48 non-empty and partially transparent`
- Flipbook frame uniqueness: `PASS — 16/16 unique for each atlas`
- Magenta/missing-texture color signature: `PASS — 0 pixels in Runtime candidates`
- Baked text, logo or watermark in Runtime candidates: `PASS — none`
- Visual inspection: `PASS — all source, delivery and preview images inspected`
- Binary count / total bytes: `13 / 5,084,775 bytes`
- Runtime integration, Unity tests, build and browser smoke: `NOT RUN / OUT OF SCOPE`

## 7. Runtime integration still missing

Requested Integration: `NONE IN THIS PASS`

The following remain `PROPOSED / REVIEW REQUIRED` and require a separate S9
Runtime Art Integration relay:

- Unity import settings, compression, mipmaps, color space and wrap/filter mode
- alpha blend/material choice and renderer ownership
- authoritative existing signal audit and exact presentation hook binding
- object-local/world/contact-normal attach space and scale
- duration, rate limiting, pooling, occlusion and Reset cancellation
- READY exclamation composition, Exit fixture placement and UI pairing
- WebGL alpha-overdraw/performance verification, Unity tests and browser smoke

No Gameplay state, damage, breakage, fracture, causal carrier, physics, input or
Scene behavior is contained in these files.

No new production asset blocks this work package. Graybox fallback remains active.
