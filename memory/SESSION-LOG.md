# SESSION LOG — append, dated

---

## 2026-08-17

- Initialized durable memory for the CHR00 updated-animation integration (D-001).
- Confirmed the textured existing model remains and updated FBXs use independent Humanoid avatars (D-002).
- Updated 37-FBX delivery is present; current controller references 22 unique delivery FBXs and leaves 15 unbound (observed, pending narrow Unity gate).
- User required both low-pickup clips and complete CHR00 animation/gameplay synchronization (D-004).
- Replaced the CHR00 delivery with all 37 updated FBXs and rebuilt each importer as Humanoid / Create From This Model / no source Avatar.
- Connected Pickup Low Small/Medium as dedicated runtime cues and states. Small contact is frame 13/37; Medium contact is frame 37/144.
- Centralized frame-derived acquisition, throw, melee, reaction, stop, and idle-variant boundaries in `ShowcaseChr00AnimationTiming`.
- Corrected Jab to LeftHand and Cross to RightHand, made combo input honor the authored buffer window, and added the 1-hit reverse Jab return state.
- Corrected Catch/Throw physical commits: Center/Medium catch frame 12, Dive frame 48, Small throw frame 48, Medium throw frame 49; action completion now follows measured clip endpoints/settles.
- Removed pickup carry snaps: Small remains left-hand carried until Throw windup blends to the right-hand throw; Medium blends the object and bilateral IK together over its stabilization interval.
- Narrow Unity evidence passed: importer 1/1, event contracts 2/2, pickup/carry/throw 2/2, natural combo 1/1, Jab return 1/1, knockdown/getup 1/1; compiler diagnostics 0.
- Known source/visual decisions remain open: Jump anticipation versus immediate physics, Knockdown→Getup pose discontinuity, and Pickup Small's large authored Hips travel.
- Verified the stationary Small-pickup alternatives and recorded them in `memory/knowledge/chr00-pickup-candidates.md`: Medium is the only true low-pickup fallback; Center is stationary but catch-like.
- D-006 superseded the Medium fallback: replaced `CHR00_Pickup_Low_Small.fbx` with the user-supplied stationary `Picking Up.fbx` (SHA-256 `3D9F5942...C2FB3`).
- Rebuilt the stale `Scene/1–37` importer metadata through a one-time Generic reset, then restored independent Humanoid/Create From This Model import. Unity exposes the 144 samples as take `mixamo.com`, frames `0–143`.
- Bound Small pickup to its actual right-hand motion and frame-derived contact/stabilization/completion at source frames 37/106/132. Medium pickup was not changed.
- Narrow replacement evidence passed: updated-source importer contract 1/1 and actual v007 Small Pickup→Carry→Charged Throw PlayMode 1/1; compiler errors/warnings 0.
- Human smoke exposed a false handoff: the open Unity Editor overwrote the verified Small `.meta` on shutdown with its cached legacy `Scene/1–37` importer state while the controller retained the new clip fileID. This caused a low-pose freeze and `SHOWCASE ANIMATION EVENT TIMEOUT: Acquire`.
- Rebuilt the actual worktree with Unity closed. The atomic contract is now take `mixamo.com`, fileID `-203655887218126122`, frames `0–143`, events `.25174826/.91608393`, matching the controller.
- Post-fix evidence on the actual worktree: importer 1/1 PASS and natural v007 Small Pickup→Carry→Charged Throw 1/1 PASS. The safe binary-replacement procedure is recorded in `memory/knowledge/unity-fbx-binary-replacement.md`.
