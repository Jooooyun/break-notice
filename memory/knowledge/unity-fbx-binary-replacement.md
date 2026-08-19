# Unity FBX binary replacement — verified procedure

## Failure observed on 2026-08-17

- A new 144-sample `Picking Up.fbx` binary replaced the old `CHR00_Pickup_Low_Small.fbx` while the project Unity Editor was open.
- A separately generated correct `.meta` was copied onto disk, but the open Editor still held the old importer state. When that Editor exited, it serialized the stale state back over the correct file.
- Stale state: take `Scene`, clip fileID `-623603649343465320`, frames `1–37`, events `0.33333334/1.0`.
- Controller state: new clip fileID `-203655887218126122`.
- Result: state entry succeeded, but the bound clip had no matching `ShowcaseCarryContact`/`ShowcaseActionComplete`; the actor remained in the low 37-frame pose and `SHOWCASE ANIMATION EVENT TIMEOUT: Acquire` fired after six seconds.

## Verified safe procedure

1. Do not copy a generated FBX `.meta` or controller over a project while that Unity Editor can later save stale importer state.
2. Close the project Editor, or run the rebuild inside that same Editor through `BREAK NOTICE/Contest/Rebuild CHR00 Player Animation Controller`.
3. For a binary replacement whose default take is missing or differs, clear custom clips and pass once through Generic / No Avatar, then rebuild as Humanoid / Create From This Model / source Avatar null.
4. Verify source take, clip fileID, configured frame range, AnimationEvents and controller motion fileID as one atomic contract.
5. Run the exact natural runtime path; an importer-only or copied-project PASS does not prove the currently open Editor has consumed the same metadata.

## Correct Picking Up contract

- take `mixamo.com`
- clip fileID `-203655887218126122`
- Unity frames `0–143`
- `ShowcaseCarryContact` at `0.25174826`
- `ShowcaseActionComplete` at `0.91608393`
- controller `PickupLowSmallAction` references the same GUID/fileID

Evidence: actual worktree rebuild PASS, actual-worktree focused importer 1/1 PASS, actual v007 Small Pickup→Carry→Charged Throw 1/1 PASS, and zero `Rig Error`/C# diagnostics on 2026-08-17.
