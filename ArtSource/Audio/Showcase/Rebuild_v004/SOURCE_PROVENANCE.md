# Rebuild v004 lossless source provenance

Status: `VERIFIED SOURCE INPUT / OUTPUTS REMAIN REVIEW_CANDIDATE`

All production inputs are local WAV files under `Sources/Lossless`. The builder
checks the exact SHA-256 before decoding and rejects MP3, OGG, Vorbis, and paths
outside that directory. No prior BREAK NOTICE source, stem, or rendered audio
byte is accepted by the production builder.

## Direct LaSonothèque recordings

Joseph SARDIN's sound pages identify these as real indoor mono recordings,
48 kHz / 24-bit, released as CC0/public-domain-equivalent audio. The local
files are the site's direct BWF/WAV downloads.

| Local file | Official page | Direct WAV | SHA-256 |
|---|---|---|---|
| `Punch_10_2465.wav` | https://lasonotheque.org/coup-de-poing-10-s2465.html | https://lasonotheque.org/UPLOAD/bwf-en/2465.wav | `049ce0074a7f779b5fbf9aad0389fcf7b7a84ca8551d12206c80a0de8a814d2f` |
| `Punch_11_2466.wav` | https://lasonotheque.org/coup-de-poing-11-s2466.html | https://lasonotheque.org/UPLOAD/bwf-en/2466.wav | `8b7da28fbb95c4a0097619e9aa752e8748a88dd97a630b0d5a241c2febb67e6d` |
| `Punch_12_2467.wav` | https://lasonotheque.org/coup-de-poing-12-s2467.html | https://lasonotheque.org/UPLOAD/bwf-en/2467.wav | `ad8facc4c662883a3b2b5e68b3ca43a0798cb32c7ac2a2f26353f07d35de5480` |

Punch #10-#12 were selected because the earlier v002 source tree contains
Punch #4-#9 but not these files. A Git-blob audit against the complete v002
tree found zero byte-identical matches for every v004 source.

## OpenGameArt CC0 lossless archives

Only WAV members were retained. The downloaded archives were decoded locally;
the lossy-only cardboard candidate (`10 Impact/Shield Blocks`) was rejected.

| Source pack | Official page | Official archive | Archive SHA-256 | Retained WAV members |
|---|---|---|---|---|
| Owlish Media Sound Effects | https://opengameart.org/content/sound-effects-pack | https://opengameart.org/sites/default/files/Owlish%20Media%20Sound%20Effects.zip | `57d7059fe2c0ea5f47c7554ace22a538ea22ab40bfc0906b71aa259afffd3d18` | hit, slap, shoulder/cloth, fruit/body, blanket cloth, paper crumple and page flip |
| Book Flip Sounds | https://opengameart.org/content/book-flip-sounds | https://opengameart.org/sites/default/files/BookFlip_SFX.zip | `aa59a4f7a1f0aa1d1cbcb9561cf1120f2cc66a07ff49b7cc7dc8810ccb9646fe` | BookFlip 2, 9, 12 |
| 68 Workshop Sounds | https://opengameart.org/content/68-workshop-sounds | https://opengameart.org/sites/default/files/workshop.7z | `79d7801041b5fbc8e181d9aaaa8dafb94b68bb51f325017e82fb37448e2fb1d3` | plastic hammer, jingle, metal drop, thuds, scrape |
| 27 Metal Audio Samples | https://opengameart.org/content/27-metal-audio-samples-sfx | https://opengameart.org/sites/default/files/27-Metal-Audio-Samples-blacklodgegames.com_.zip | `479e85403898c2ff2d1e2d6a130b8cfb4ae00fbd8cbcaf440917450f996f5232` | dull collisions 06/08/11 and wobble 08 |
| Scrapes | https://opengameart.org/content/scrapes | https://opengameart.org/sites/default/files/scrapes.zip | `9598070ddd77ed99e922df1d4472a249624adcd945794d1b6eba88ac77f6a014` | scrape 1 and 2 WAV |

### Exact retained archive members

- Owlish Media: `OwlishMedia/cloth_blanket_movement_4.wav` ← `Cloth, Rustle/320142__owlstorm__blanket-movement-4.wav`; `cloth_blanket_movement_6.wav` ← `Cloth, Rustle/320144__owlstorm__blanket-movement-6.wav`; `impact_fruit1.wav` ← `Impacts/fruit1.wav`; `impact_hit.wav` ← `Impacts/hit.wav`; `impact_shouldergrab.wav` ← `Impacts/shouldergrab.wav`; `impact_slap2.wav` ← `Impacts/slap2.wav`; `paper_crumples_1.wav` ← `Paper/crumples1.wav`; `paper_sketchbook_flips_1.wav` ← `Paper/paper-sketchbook-page-flips-1.wav`.
- Book Flip: `BookFlip/BookFlip_2.wav` ← `BookFlip2.wav`; `BookFlip_9.wav` ← `BookFlip9.wav`; `BookFlip_12.wav` ← `BookFlip12.wav`.
- Workshop: `Workshop/hammering_on_plastic.wav` ← `workshop/workshop - hammering on plastic.wav`; `jingle_3.wav` ← `workshop/workshop - jingle3.wav`; `metal_drop_2.wav` ← `workshop/workshop - metal drop 2.wav`; `scrape_5.wav` ← `workshop/workshop - scrape5.wav`; `thuds.wav` ← `workshop/workshop - thuds.wav`.
- Metal27: `Metal27/dull_metal_collision_06.wav` ← `27-Metal-Audio-Samples-blacklodgegames.com/dull_metal_collision_06_44k_32bit_stereo.wav`; `dull_metal_collision_08.wav` ← `27-Metal-Audio-Samples-blacklodgegames.com/dull_metal_collision_08_44k_32bit_stereo.wav`; `dull_metal_collision_11.wav` ← `27-Metal-Audio-Samples-blacklodgegames.com/dull_metal_collision_11_44k_32bit_stereo.wav`; `wobble_08.wav` ← `27-Metal-Audio-Samples-blacklodgegames.com/wobble_08_44k_32bit_stereo.wav`.
- Scrapes: `Scrapes/scrape_1.wav` ← `scrape-1.wav`; `scrape_2.wav` ← `scrape-2.wav`.

OpenGameArt labels every listed pack CC0. The descriptions for the Owlish pack
state that the uploader recorded the effects over several years; other pages
identify their uploaders but do not independently prove chain of title. That is
the residual provenance limit, so this pack remains a review candidate until
the Human Owner accepts it.

## Rejected candidates

- Freesound preview MP3 files: lossy preview bytes, not source masters.
- Freesound original downloads: login/API gated in this work session.
- `10 Impact/Shield Blocks`: its advertised archive contains OGG only.
- Rubberduck breaking/falling packs: OGG only.
- Earlier LaSonothèque Punch #4-#9 and Body Fall #1-#4: already present in the
  v002 source tree and therefore forbidden by the clean-rebuild rule.
