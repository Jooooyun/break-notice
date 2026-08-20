# FINAL AUDIO v004 — known missing

## Source inventory

No requested runtime mapping is missing from `FINAL_AUDIO_v004_LIGHT_ENEMY.zip`.
The archive contains exactly the 40 approved runtime WAVs and no audition reel or documentation payload.

## Deliberately absent substitutions

- There is no Enemy Heavy Hurt-specific clip in v004. Runtime does not pitch-shift, clone, or disguise another asset as a new heavy voice; enemy nonlethal voice remains within the approved Enemy Light Hurt A/B/D pool.
- There is no separate landing clip in the v004 mapping. Footstep contact audio is not reused as a fabricated landing sound.
- No derived Strap composite was created. Runtime schedules the approved Z01 and Z02 originals directly.

All absent roles are null-safe or outside the approved v004 mapping.

## Build limitation outside Final Audio

The WebGL Development build is blocked by the pre-existing
`Assets/_Project/Code/Runtime/Contest/BrowserPointerCapture.cs:54` reference to
`WebGLInput`, which is unresolved under the current Unity 6000.3.20f1 WebGL
player compilation (`CS0103`). Final Audio does not modify that file. The same
v007 content builds successfully for Windows Development with zero warnings and
zero errors.
