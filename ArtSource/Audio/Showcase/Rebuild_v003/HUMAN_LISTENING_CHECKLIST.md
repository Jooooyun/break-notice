# Human Owner blind listening checklist

Before recording the first decision, use only `Renders/Blind/Videos/BLIND_X/Y/Z_AUDITION_2VAR_REVIEW.mp4` and the matching WAV files in `Renders/Blind`.

Do not open `Renders/PRIVATE_BLIND_MAPPING.json`, `Renders/Candidates`, `Renders/Stems`, the build script, or manifest/QA files whose hashes can reveal candidate identity.

## Pass 1 — laptop speakers

Use the same system volume for X, Y, and Z. Play each `BLIND_*_AUDITION_2VAR_REVIEW.wav` once, then repeat in a different order.

For each label, record `PASS`, `BORDERLINE`, or `FAIL`:

- the hit reads at the exact visible contact, not before or late
- it feels like the Showcase's T3 peak, not an ordinary punch
- the opening transient survives small speakers without painful sharpness
- Chair metal/plastic is distinguishable from Runner body/cloth
- floor/caster/skid follows the hit instead of becoming one boom
- variation 1 and 2 belong to the same family but do not sound duplicated

## Pass 2 — headphones

- no fake bone break, gore, magic, or oversized trailer boom
- no phase pull, smeared stereo center, harsh click, or flabby sub tail
- material order remains `Chair → Runner → floor/caster`
- the tail ends cleanly and leaves space for commands/status cues
- twenty manual repeats do not become tiring or obviously machine-gunned

## Decision

Record exactly one:

- `PASS X`
- `PASS Y`
- `PASS Z`
- `REJECT ALL`

Optional revision note: identify the layer (`pre`, `signature`, `chair`, `runner`, or `floor_tail`) and the problem (`too early`, `too loud`, `too sharp`, `too dull`, `too long`, `material unclear`, or `repetitive`).

This decision approves only the North-Star sonic direction. It does not approve Runtime integration, AudioMixer, BGM, other P0 families, or `FINAL` source status.
