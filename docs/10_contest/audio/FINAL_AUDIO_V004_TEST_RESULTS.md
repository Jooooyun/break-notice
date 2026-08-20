# FINAL AUDIO v004 — test results

Unity version: `6000.3.20f1`.

## Authoring

- First v007 authoring: PASS — 40 clips, one listener, 36 total Scene AudioSources.
- Second idempotent v007 authoring: PASS — identical serialized source counts.
- CHR00 locomotion/Chair-whoosh event rebuild: PASS.
- ENY01 Standard Run foot-contact event rebuild: PASS.

## Automated tests

- Final Audio EditMode: `7/7 PASS` (`FinalAudioV004-EditMode-v3.xml`).
- Final Audio PlayMode: `7/7 PASS` (`FinalAudioV004-PlayMode-v6.xml`).
- Related EditMode regression: `41/41 PASS`
  (`FinalAudioV004-Regression-EditMode-v2.xml`).
- Related PlayMode regression bundle: `49/51 PASS`. The two red tests are
  pre-existing timing-sensitive non-audio assertions: MediumExtending must be
  observable long enough to accumulate movement, and the first queued player
  combo contact must always be Jab at a boundary target placement. Final Audio
  focused tests and their exact event paths remain green.
- Windows Development build: `PASS`, `0 warnings`, `0 errors`,
  `1,099,352,645 bytes`, GUID `2133e3466fdb4096bb1e1e4af1c78519`.
- Windows player boot smoke: engine, assemblies, Input System and PhysX loaded;
  no runtime exception was emitted before the intentionally terminated
  headless process.
- WebGL Development build: `BLOCKED` by the pre-existing
  `BrowserPointerCapture.cs:54` `WebGLInput` CS0103 error; Final Audio code is not
  implicated.

## Evidence files

- `Game/TestResults/FinalAudioV004-Authoring.log`
- `Game/TestResults/FinalAudioV004-Authoring-v2.log`
- `Game/TestResults/FinalAudioV004-Rebuild-CHR00.log`
- `Game/TestResults/FinalAudioV004-Rebuild-ENY01.log`
- `Game/TestResults/FinalAudioV004-EditMode-v3.log`
- `Game/TestResults/FinalAudioV004-EditMode-v3.xml`
- `Game/TestResults/FinalAudioV004-PlayMode-v6.log`
- `Game/TestResults/FinalAudioV004-PlayMode-v6.xml`
- `Game/TestResults/FinalAudioV004-Regression-EditMode-v2.log`
- `Game/TestResults/FinalAudioV004-Regression-EditMode-v2.xml`
- `Game/TestResults/FinalAudioV004-Regression-PlayMode-v2.log`
- `Game/TestResults/FinalAudioV004-Regression-PlayMode-v2.xml`
- `Game/TestResults/FinalAudioV004-WebGL-Build.log`
- `Game/TestResults/FinalAudioV004-Windows-Build.log`
- `Game/TestResults/FinalAudioV004-Windows-Smoke.log`
