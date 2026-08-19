# CHR00 pickup candidates

## Picking Up is the accepted stationary Small pickup source — verified 2026-08-17

- Decision: D-006 supersedes the temporary D-005 Medium reuse.
- Source: `C:/Users/jhbjm/Documents/카카오톡 받은 파일/Picking Up.fbx`, SHA-256 `3D9F5942A127F3F8AE39A1698315D502FA795805D9F340256FBB72451A4C2FB3`.
- Compatibility: 41 Mixamo bones and parent hierarchy match CHR00; import is Humanoid / Create From This Model with no copied source Avatar.
- Motion: 144 samples at 30 fps, effectively in-place (about 4.44 cm net lateral and 0.22 cm net forward Hips displacement), right-hand one-handed floor pickup.
- Unity representation: source take `mixamo.com`, frames `0–143` (the DCC/source report numbers the same samples `1–144`).
- Synchronization: contact source frame 37 (`0.25174826` normalized), stabilization frame 106 (`0.73426574`), completion frame 132 (`0.91608393`).
- Evidence: focused importer contract 1/1 PASS and actual v007 Small Pickup→Carry→Charged Throw PlayMode 1/1 PASS on 2026-08-17.
- Limit: automated tests verify state, ownership, event and physical continuity; final visual quality still requires the Human Owner smoke.

## Superseded finding: the Designer A delivery's original Pickup Low Small was moving — verified 2026-08-17

- Claim: `CHR00_Pickup_Low_Small` is not an in-place pickup. Its evaluated Hips travels about 5.614 m horizontally over frames 2–38.
- Claim: `CHR00_Pickup_Low_Medium` is effectively stationary by comparison: about 4.4 cm net lateral and 0.2 cm net forward Hips displacement over frames 2–145.
- Claim: `CHR00_Catch_Small_Center` is also stationary horizontally, but its authored action is a two-hand forward catch rather than a floor pickup.
- Sources: direct Blender 5.1 evaluation of the three delivered FBXs; exact CHR00 37-file asset inventory under `Game/Assets/_Project/Art/Animation/CHR00/DesignerA/Approved`.
- Sample: 3 relevant delivered FBXs, every source frame evaluated.
- Limits: no Unity visual smoke was used for this comparison. Reusing the Medium motion for a Small object is a temporary semantic compromise and still needs a brief visual check.
- Historical verdict: do not use the original delivered `Pickup_Low_Small` for an in-place interaction. This is no longer the active source after D-006 replaced that binary with the verified `Picking Up.fbx`.
