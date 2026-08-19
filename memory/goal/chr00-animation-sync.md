# Goal — CHR00 updated animation synchronization

## Goal

Connect the approved/reachable CHR00 updated-animation delivery to runtime gameplay and make every animation-owned physical/domain transition occur on the matching visual frame.

## Definition of done

Both low-pickup clips are naturally reachable; every reachable CHR00 action uses an independently imported Humanoid source and frame-derived contact/release/window/completion markers; focused Unity tests pass; a short human visual smoke finds no visible ownership or physics desynchronization.

## Mobilization

| Branch | Needs | Held | Gap → first move |
|---|---|---|---|
| Import/controller | 37 exact assets, independent avatars, state mapping | Current 37-FBX worktree + authoring | Run focused importer/controller gate after edits |
| Pickup | Small/Medium cues, states, contact and completion frames | Existing acquisition transaction and GripRig | Measure both clips, then bind exact cues/events |
| Catch/throw | Visual contact/release and completion frames | Existing event-owned physical commits | Re-measure delivered clips; replace inherited normalized times |
| Combat/reaction | Attack windows and visual completion | Bone overlap authority and action epochs | Re-measure raw Jab/Cross and reaction clips |
| Verification | Deterministic natural event tests + visual smoke | Existing EditMode/PlayMode fixtures | Add only CHR00-focused coverage |

## Terrain map

- confirmed — Updated FBXs must be Humanoid/Create From This Model; Copy From Other Avatar is forbidden by D-002.
- observed — Runtime state changes are code-driven CrossFade calls; Animator Trigger parameters are absent.
- observed — Physical acquisition and throw already have single AnimationEvent commit seams.
- confirmed — Small pickup uses the supplied stationary `Picking Up.fbx` (D-006); Medium retains its own source.
- unknown — Exact source frames for all physical/action markers (Q-001).
- excluded — Heavy/Large activation and reference/candidate/turn/ALT integration are outside this goal.

## Skeleton v1

1. Source truth
   - [filled] exact 37-FBX inventory and import policy
   - [filled] per-clip source frame/take evidence for connected physical/combat actions
2. Runtime mapping
   - [filled] current controller and cue map
   - [filled] Pickup Small/Medium cues and states
3. Synchronization
   - [filled] acquisition contact/completion
   - [filled] throw release/completion
   - [filled] melee window/completion
   - [filled] hit/knockdown/getup completion timing; source seam remains Q-003
   - [filled] locomotion/jump/stop state-boundary audit; Jump policy remains Q-002
4. Verification
   - [filled] focused EditMode source/event contract
   - [filled] focused PlayMode natural-event behavior
   - [named-unfilled] human visual smoke

## Single next leaf

Run the short human visual smoke for Pickup Small/Medium, Jump, and both Knockdown→Getup seams, then decide Q-002 and Q-003 without changing timing blindly.

## Done-check

- [x] Pickup Small and Medium naturally enter dedicated states.
- [x] Every reachable CHR00 physical/domain event is tied to a measured frame, except the still-open Jump impulse policy.
- [x] No CHR01/ENY01/camera/input/scene/package drift.
- [x] Focused EditMode and PlayMode pass with no compiler errors.
- [ ] Human visual smoke accepted.

## Known gaps

- Q-002 Jump anticipation/physics authority.
- Q-003 Knockdown→Getup source-pose discontinuity.
- Q-004 resolved by D-006 replacement with `Picking Up.fbx`.
