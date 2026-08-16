# Rebuild v003 source ledger — REVIEW ONLY

Retrieved: `2026-08-16`

License target: `CC0 1.0`

Baseline: `b15d971375cfb3a0be7d978f9311b58702e9d431`

This ledger covers only newly acquired source bytes used by the `Chair → Runner` T3 North-Star review. No source, render, layer, or script from v001/v002 is present in the processing graph.

## Rights and quality boundary

- Kenney and OpenGameArt files were downloaded directly from their official asset pages.
- Freesound individual pages mark each listed sound `Creative Commons 0`. The retained bytes are Freesound's public `-hq.mp3` preview derivatives, not the login-gated original lossless downloads.
- The Freesound previews are permitted only for this isolated `REVIEW` comparison. Before any `FINAL` or Runtime proposal, replace every selected Freesound preview with its original lossless download and rerun the blind review, unless the Human Owner explicitly accepts preview-derived sources.
- CC0 removes the listed copyright restrictions but does not independently prove that an uploader owned every underlying right. Page snapshots and Freesound terms are preserved so this residual risk is visible.

## New recording sources

| Role | Official asset page | Retrieved bytes | Direct retrieved URL | SHA-256 |
|---|---|---|---|---|
| Office-chair caster, roll and rattle | [Freesound 317850](https://freesound.org/people/benniknop/sounds/317850/) | `freesound-317850-office-chair-roll-rattle-hq-preview.mp3` | `https://cdn.freesound.org/previews/317/317850_2821704-hq.mp3` | `36403919eaa0a0be18c47b188d7cada2a1aa48ec375034581dfa20fde4575cb2` |
| Steel-chair movement and impact | [Freesound 494620](https://freesound.org/people/Anakronizm/sounds/494620/) | `freesound-494620-steel-chair-hq-preview.mp3` | `https://cdn.freesound.org/previews/494/494620_10532031-hq.mp3` | `72cac01e96da03a0b50d406e527f3800b422a6d18763e43d1b0e643e22ee3d6d` |
| Plastic/metal object on concrete core | [Freesound 104051](https://freesound.org/people/RutgerMuller/sounds/104051/) | `freesound-104051-plastic-metal-concrete-impact-hq-preview.mp3` | `https://cdn.freesound.org/previews/104/104051_179538-hq.mp3` | `e6cf13f53e47b99fe09e224ef8ceff61f8fd49c78263f65ff223c136d9b47184` |
| Plastic chair scrape on concrete | [Freesound 708509](https://freesound.org/people/Group39/sounds/708509/) | `freesound-708509-plastic-chair-concrete-scrape-hq-preview.mp3` | `https://cdn.freesound.org/previews/708/708509_15367714-hq.mp3` | `9dc3cedd1d62ae4f9cca8064681a107f353fe825d64c3d6390486b869141067d` |
| Body/coat contact core | [Freesound 235451](https://freesound.org/people/jcdecha/sounds/235451/) | `freesound-235451-body-hit-coat-wall-hq-preview.mp3` | `https://cdn.freesound.org/previews/235/235451_3843719-hq.mp3` | `41d655cf3c0cdb7b05ece9db2bc82cfefd2d2a4e543c520a7092e8228fff6ab7` |
| Clothed-body contact | [Freesound 629976](https://freesound.org/people/Flem0527/sounds/629976/) | `freesound-629976-clothed-body-hit-hq-preview.mp3` | `https://cdn.freesound.org/previews/629/629976_12715066-hq.mp3` | `0cf61888090d94bdfee37d499ea5370d739773a729c42ce5bd1e287911d0b56f` |
| Fast chair/floor slide | [Freesound 51132](https://freesound.org/people/RutgerMuller/sounds/51132/) | `freesound-51132-chair-sliding-quickly-hq-preview.mp3` | `https://cdn.freesound.org/previews/51/51132_179538-hq.mp3` | `e55ebc39361f5f0ad87f57f98b3cf699f761da1e6230cde05a759d3588a8a61d` |

## CC0 support packs

### Kenney — Impact Sounds 1.0

- Official page: [https://kenney.nl/assets/impact-sounds](https://kenney.nl/assets/impact-sounds)
- Page states: `130×` files, `Creative Commons CC0`
- Direct archive: `https://kenney.nl/media/pages/assets/impact-sounds/87b4ddecda-1677589768/kenney_impact-sounds.zip`
- Archive SHA-256: `029d734af1582474edf3a694d1b0cebc97c1c152f2f39fa34d4c2bafc5de77f8`
- Used members and hashes are recorded in `Renders/Evidence/BUILD_MANIFEST_REVIEW.json`: two concrete contacts, two metal-heavy impacts, two punch-heavy contacts, and two soft-heavy contacts.
- The archive's `License.txt` is retained beside its audio and states CC0.

### OpenGameArt — Metal Impact Sounds

- Official page: [https://opengameart.org/content/metal-impact-sounds](https://opengameart.org/content/metal-impact-sounds)
- Page states: author `BMacZero`, license `CC0`
- Used direct files:
  - `clink1_0.wav` — `0d439cd2b1f36f501b94c39661e39f91d160affe25c79f77dff7a2cd0ed7eadd`
  - `clink2.wav` — `ecbf5a221f2aff32376b8584eea875d0b7d9963d22f6c16e237f5e7b63f51dc2`
  - `clink3.wav` — `dd07516e51e9092efc36d13668de7d29d096955a437894cedeb5064933e2650f`
  - `thud2.wav` — `5fb5c1b37a95cb6716d0bb529a0b9fe1ae9f3e2228b8f30c5c3dcbb1b570a3af`
  - `thud3.wav` — `3aa636679ffdc9602d27655ca59a7c7342ae137b68e54c61aad0fb3c414506ca`

## Preserved verification evidence

| Snapshot | SHA-256 | Bytes |
|---|---|---:|
| `creativecommons-cc0-1.0.html` | `4ceb8ae6835f2f5263caa0e39c9e1adca9469686c267475049b33521dabbe339` | 30476 |
| `freesound-104051-plastic-metal-concrete-impact.html` | `eaa9328ef4fa9710cdcf5344570f55bb4e9a3adeab06f5286c4ea787d468c3b0` | 39973 |
| `freesound-235451-body-hit-coat-wall.html` | `2a1afb69bcbf2742bb75d7275a4df731d0a7085d8ac10b1a1b99142f2a7478d6` | 35447 |
| `freesound-317850-office-chair-roll-rattle.html` | `d98d17f59a5cb5a8b5a162cce076a683b543f0818bcb8974d18175ccae560a88` | 41880 |
| `freesound-494620-steel-chair.html` | `f4bae4a54c234b3c7b4404aa73f2266a6526515260c7d1bf9c6cdddbf541c0b9` | 35822 |
| `freesound-51132-chair-sliding-quickly.html` | `47acd108efcc72104a9fae703121f0e1c714bb860d27db551438deb0026ec245` | 44192 |
| `freesound-629976-clothed-body-hit.html` | `5166700c3cb894d70333c7e21ee0579629079543cdd58ba4deacafc5dee69fb2` | 37087 |
| `freesound-708509-plastic-chair-concrete-scrape.html` | `b8c023a9d1587ae7f899f659d69ec7f5601d607f462ac673ea61dbb8ec407b9b` | 36579 |
| `freesound-terms-of-use.html` | `9ae526bc4648e22c9dea66afbaaddccfead796950787737e545656ffa376876a` | 34577 |
| `kenney-impact-sounds.html` | `1ccb473dbbc6b194427a97c6a3a02df7d18db4dad4fb12afb6550b334465c9d5` | 12931 |
| `opengameart-metal-impact-sounds.html` | `92b2125790fab35d64ac3af42438b7b2f3180d99ae2273fb8592a73323fde687` | 26510 |

Exact source member hashes and decode metadata are recorded in `Renders/Evidence/BUILD_MANIFEST_REVIEW.json`.

## Explicit exclusions

- no LaSonothèque source
- no v001/v002 approved reference reel
- no old composite, export, source master, build script, QA script, or review scene
- no famous game/film sample
- no music, voice, generated external audio, or non-CC0 source
