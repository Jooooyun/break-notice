# FINAL AUDIO v004 Unity integration

Goal: audit `FINAL_AUDIO_v004_LIGHT_ENEMY.zip` and integrate every approved runtime WAV into the existing BREAK NOTICE gameplay facts with no manual Inspector work.

Done means: approved WAVs are imported unchanged; Binder, Chair, Strap, footsteps, Small spin, player/enemy voices are connected through one typed architecture; transient/loop/voice policies and dedupe are enforced; v007 and relevant prefabs/config are serialized; compile/tests/smoke/build evidence and owner-facing reports exist; no commit or push is made.

## Terrain map

- [observed] Worktree/branch/HEAD match the owner directive and the tree started clean.
- [observed] The project already contains accepted-contact unarmed audio and must be extended rather than replaced.
- [confirmed] ZIP SHA-256 `13D624CCDF0406C928DB37A1F8790F9CB427628A956071BD553561D41AFC83DA`; exactly 40 intact PCM WAVs, all 48 kHz/24-bit, no basename/SHA duplicates, no requested mapping missing.
- [confirmed] ZIP contains runtime WAVs only: audition reel and HTML/CSV/TXT/MD/Audition/QA material are absent.
- [observed] Existing audio is one accepted-contact unarmed-impact module with eight pooled 3D sources and one OutputCamera listener; there is no mixer, profile, footstep, voice, or loop service to duplicate.
- [observed] Authoritative seams are acquisition commit, physical drop/throw commit, accepted object combat contact, target collision entry, strap attach/tension state commits, DamageAccepted, and new explicit animation foot contacts.
- [observed] v007 is the only final-combat Scene authority. Binder/Chair/Runners are scene-authored, not gameplay prefabs; shared Player/Camera prefabs must remain untouched.
- [named-unfilled] Unity import, compile, test, smoke, and build evidence.

## Skeleton v1

1. Source authority
   - [filled] exact worktree, branch, HEAD, and clean start state
   - [filled] ZIP integrity and approved runtime inventory
   - [filled] excluded audition/document inventory
   - [filled] byte identity and source format
   - [filled] Unity import settings
2. Architecture
   - [filled] reuse boundary for current unarmed combat audio
   - [filled] typed profile and transient source pool
   - [filled] object-bound roll/spin loops
   - [filled] actor-bound voice and footstep state
3. Gameplay facts
   - [filled] Binder pickup/place/throw/catch/impact routing
   - [filled] Chair pickup/place/throw/whoosh/roll/skid/impact/stop
   - [filled] Strap extend sequence/lock/tension
   - [filled] grounded foot contact for player/enemies
   - [filled] Small spin transition and fade
   - [filled] hurt/heavy/KO voice priority and cooldown
4. Serialization
   - [filled] AudioClip import and profile asset
   - [filled] prefab/scene ownership decision: v007-only, no shared prefab mutation
   - [filled] scene/source wiring
   - [filled] idempotent Editor authoring
5. Evidence and handoff
   - [filled] static validation
   - [filled] EditMode and PlayMode tests
   - [filled] scene smoke and Windows build
   - [filled] mapping, known-missing, results, and integration reports

Single next leaf: Human Owner review of the diff and the recorded WebGL blocker.

## Known gaps

- WebGL player build remains blocked by the pre-existing `WebGLInput` compile error in `BrowserPointerCapture.cs`.
- Two broader non-audio PlayMode assertions remain timing-sensitive; details are in the test report.

## Done check

- No approved WAV modified.
- No audition reel or documents under runtime Assets.
- Manual Inspector work remaining: zero.
- Missing audio is reported and left null-safe, never substituted.
- No commit or push.
