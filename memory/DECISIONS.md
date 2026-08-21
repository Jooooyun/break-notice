# DECISIONS — append-only ledger

Rules: only user-confirmed decisions are recorded. Changed decisions get a new entry that supersedes the old entry.

---

## D-001 · Adopt durable project memory — 2026-08-17 (user, prior session)

Record animation integration mistakes, failure causes, fixes, and reusable procedures so they are not rediscovered.

## D-002 · Keep the textured CHR00 model and use independent Humanoid avatars — 2026-08-17 (user)

Retain the current textured CHR00 model. Import every updated animation FBX as Humanoid with Create From This Model; never Copy From Other Avatar.

## D-003 · Replace CHR00 animation sources with the updated 37-FBX delivery — 2026-08-17 (user)

Use Designer A's `updatedAnimation` delivery as the CHR00 animation source. Existing connected clips are replaced; reference, preview, and unapproved candidate clips remain unbound unless separately approved.

## D-004 · Connect both low-pickup clips and synchronize all CHR00 gameplay commits to animation — 2026-08-17 (user)

Connect `CHR00_Pickup_Low_Small` and `CHR00_Pickup_Low_Medium`. For CHR00, object contact/release, combat windows, physics, ownership, and action completion must match the actual delivered animation rather than inherited arbitrary normalized times.

→ superseded by D-005 (2026-08-17)

## D-005 · Reuse Pickup Low Medium for Small floor pickup — 2026-08-17 (user-confirmed)

`PickupLowSmallAction` must reuse `CHR00_Pickup_Low_Medium` with the same animation-owned synchronization. Keep `CHR00_Pickup_Low_Small` in the repository as an unbound source asset because its 5.61 m forward travel is not a valid in-place pickup. Remove the now-redundant Small-specific verification logic; the Human Owner will perform the visual check.

→ superseded by D-006 (2026-08-17)

## D-006 · Use the supplied Picking Up FBX for Pickup Low Small — 2026-08-17 (user-confirmed)

Replace `CHR00_Pickup_Low_Small` with the user-supplied `Picking Up.fbx`. Keep it as an independent Humanoid/Create From This Model source, bind it to `PickupLowSmallAction`, and synchronize contact/stabilization/completion to its own 144-sample motion. `CHR00_Pickup_Low_Medium` remains unchanged.

## D-007 · Integrate FINAL AUDIO v004 through gameplay facts — 2026-08-20 (user, FINAL AUDIO v004 direct task)

Import the 40 approved runtime WAVs unchanged and connect them without manual Inspector work. Preserve the special Binder semantics: `BN_FOL_BINDER_RELEASE_02.wav` is Binder Throw and `BN_FOL_BINDER_THROW_01.wav` is Binder Place Down. Play Strap Extend as DSP-scheduled Z01 OPEN FULL followed by Z02 CLOSE FULL. Use one-shots for discrete facts, object-bound loops for Chair Roll and Small Spin, actor-bound voice sources, per-actor random state, no immediate pool repeat, collision/physics-step deduplication, voice cooldown, and `KO > Heavy Hurt > Light Hurt` priority. Audio must follow successful gameplay/animation/physics facts rather than input attempts; missing audio must remain null-safe and may not be disguised with another clip. The current unarmed accepted-contact audio remains authoritative for its six takes and Final v004 extends rather than replaces it. No commit or push is allowed before Human Owner review.

## D-008 · Build P0-01 as an impact-consequence slice inside the existing combat transaction — 2026-08-21 (user, direct task)

For Binder and Chair enemy impacts, preserve the existing collision sequence, health approval, runner reaction displacement, contact presentation, and cargo-strap authorization paths. Add one typed impact context carrying source, weight, relative speed, linear velocity, mass, charge, release kinetic-energy availability, attack origin, direction, point, and sequence. Resolve Light/Medium/Heavy through serialized mass × relative-speed × charge tuning; scale existing base damage, controlled swept displacement, knockdown, secondary world/enemy collisions, death momentum, VFX/camera tier, and bounded unscaled local hit stop from that accepted evaluation. Do not introduce ragdoll, Charge input, Slam, Multi-hit Sweep, enemy attack-budget changes, Final Map integration, asset recreation, or a parallel combat system. No commit or push is allowed before Human Owner review.

## D-009 · Extend the existing Cargo Strap transaction with player-owned Charge and Overhead Slam — 2026-08-21 (user, direct task)

For a tensioned Medium Cargo Strap object, use the existing logical Throw action as `LMB Press/Hold = Charging` and `LMB Release = resolve and attack`; use the existing explicit Strap Release action `[R]/RB` as Charge Cancel while Charging, without creating a new Input System or a second Strap Controller. Preserve 75% movement, camera-facing aim, target changes, and cancel judgment during Charge. Use Quick `<0.35`, Power `0.35–<0.75`, and Full `>=0.75` profiles; Full may knock down only after the `0.90` threshold and auto-fires after remaining full for `0.65 s`. Resolve explicit downward/floor aim to Slam first, otherwise choose the open target-side Sweep, use visible Slam fallback when that Sweep is blocked and Overhead is clear, use the alternate Sweep only when Overhead is unavailable, and reject when all paths are blocked. No approved Slam clip exists, so reuse the approved right-side Strap upper-body action only as a documented procedural fallback while the attached Chair performs the actual upward/downward Rigidbody path. Slam authorization must come from the Chair's real descending travel and collision point; Direct uses `0.65 m`, Outer uses `1.75 m` radial stagger. Keep the legacy immediate `TryBeginAttack` API as deterministic Power Left/Right behavior for established tests/callers, while live LMB gameplay uses the new Charge Resolver. Do not add P0-03 Sweep multi-hit, enemy attack-budget changes, map integration, a state-machine rewrite, commit, or push before Human Owner review.

→ partially superseded by D-010 (2026-08-21)

## D-010 · Make Charge target-optional, slow Full Charge to 50%, lead moving enemies, and accept logical Slam hurtboxes — 2026-08-21 (user, direct bug report)

Keep the existing automatic target-side attack behavior whenever a valid enemy is resolved, but do not require an enemy to start, hold, resolve, preview, or release a Medium Cargo Strap Charge. When no enemy is available, clamp the camera aim to the valid Strap range and execute the same path-aware Sweep/Slam transaction against that free-aim point. Charge movement remains `75%` through Quick/Power and eases toward approximately `50%` at Full. A resolved moving enemy must be led from its estimated planar command/physics/reaction velocity with a bounded correction, while the existing early-flight correction remains active. Overhead Slam still requires actual descending Chair travel and actual Chair geometry contact, but a targeted runner's existing Humanoid Head/Torso/Pelvis/limb hurtbox spheres count as contact even when no physical head collider exists; physical collision remains valid, Direct/Outer semantics remain unchanged, and the attack may commit only once per sequence.

↪ resolver-priority and Slam-contact details partially superseded by D-011 (2026-08-21)

## D-011 · Make Overhead Slam deliberate, floor-safe, and Head-contact tolerant only for Cargo Slam — 2026-08-21 (user, direct bug report)

Normal enemy head/body aim must remain in the lateral Sweep family. Explicit Slam requires both a near-floor aim point and a steep downward direction. If the preferred Sweep side is blocked but the opposite side is open, use the open alternate Sweep; use Overhead Slam fallback only when both lateral paths are unavailable and the overhead path is clear. The Slam trajectory may not drive the Chair below its floor support: preserve the attached Chair's actual center-of-mass-to-bottom offset, keep a small positive floor separation, and do not add post-trajectory downward overshoot. Expand the logical Humanoid Head contact radius only for the Cargo Slam transaction; ordinary player/enemy strike hurtboxes and other external-collider contacts remain unchanged.

## D-012 · Force Slam in true narrow corridors and preempt Chair Charge with close-range disengage — 2026-08-21 (user, direct follow-up)

When both lateral Chair Sweep paths are unavailable, the resolver must choose Overhead Slam whenever the overhead path is safe, regardless of target side or centered-target alternation; if overhead is also blocked, reject rather than clip through geometry. While a Medium Chair remains tensioned and selected, an enemy already inside the existing front-facing melee range owns LMB before Charge: use the approved CHR00 Jab as a one-hit contextual disengage, apply its normal accepted melee damage plus a bounded shove to create space, and preserve the tensioned Chair and Object Weapon selection. No unapproved CHR00 kick asset is introduced.

## D-013 · Authorize Horizontal Chair Sweep damage from the Chair's swept volume, not its planning target — 2026-08-21 (user, direct task)

Keep `mediumSwingTarget` only for aim, lead, and trajectory correction. During a Horizontal Sweep, inspect the attached Chair collider's previous-to-current box volume each FixedUpdate and route both swept contacts and `OnCollisionEnter` through one Cargo sequence authority. A sequence may damage each runner once and no more than three runners total. Use deterministic ordinal damage scales `100% / 75% / 52.5%`, impact-energy costs `25% / 30% / 45%`, Charge-tier target budgets of Quick `1–2`, Power `2`, and Full `3`, and terminate or sharply damp on exhausted energy, target cap, fixed/heavy world obstruction, or a blocked primary victim. The first accepted swept victim receives a bounded non-parented `0.18–0.32 s` Chair-path carry with collision and separation preserved; later victims receive attenuated outward displacement and stagger. Primary-victim collision with another runner emits the typed enemy-collision hook, applies low deduplicated damage/stagger to the secondary runner, and terminates the carry. Overhead Slam keeps its existing Direct/Outer radial transaction. No commit or push is allowed before Human Owner review.

## D-014 · Repair floor-supported Chair attacks and add a separate stationary validation roster — 2026-08-21 (user, direct bug report)

The operations-floor support collider must never terminate a Horizontal Chair Sweep merely because a BoxCast begins flush with the floor and returns a lateral or inverted normal. Floor classification uses the Chair's recorded support height and collider bounds first; true fixed/heavy obstacles still terminate the Sweep. Preserve the original two autonomous combat runners unchanged in the primary `Runners` roster and add four separate P0-03 validation runners under `P0_03_STATIONARY_CHAIR_VALIDATION`: three right-Sweep targets and one isolated Slam target. Validation runners suppress autonomous approach, facing, telegraph, and attack, while remaining damageable and allowing authored hit, stagger, knockdown, and Chair-carry displacement. They are isolated from batch-mode regression tests. During Medium Charge, Sweep, extension, and recovery, movement and Sprint remain available, but Jump stays blocked and target-facing remains owned until the Medium transaction returns to Tensioned. No commit or push is allowed before Human Owner review.

## D-015 · Make the first LMB hold authoritative and preserve ordinal Full-Sweep launch — 2026-08-21 (user, direct bug report)

A continuous LMB hold on a selected tensioned Medium Chair must not be discarded when the initial press occurs before `mediumRearReady` or during browser pointer capture. The held state retries the existing Charge transaction as soon as the Chair becomes rear-ready, without requiring a second press and without bypassing the close-range Jab priority. During a Full Horizontal Sweep, every accepted path victim up to the existing hard cap of three must receive visible controlled motion: the first victim keeps the existing Chair-path carry, the second travels less, and the third travels less again. Ordinal travel uses deterministic `100% / 72% / 48%` distance falloff with bounded durations; normal animation-entry timing may not cancel Cargo Sweep movement, a lethal second or third hit must preserve its remaining Sweep momentum, and normal recovery may not truncate an unblocked primary carry. Wall, Heavy obstacle, and primary-victim blocking still terminate movement. No commit or push is allowed before Human Owner review.

→ first-hold start timing superseded by D-016 (2026-08-21)

## D-016 · Start Medium Charge on the first valid held frame before final rear alignment — 2026-08-21 (user, direct clarification)

For a selected, tensioned Medium Chair, the first valid LMB press/hold begins `Charging` immediately; it does not wait for `mediumRearReady` and does not consume the click while the Chair is completing its final rear-alignment steps. The existing rear follower continues operating during `Charging`, so attack release still uses the corrected Chair pose. Held-input retry remains as protection for Web pointer capture or an action-state transition, and the close-range contextual Jab still owns LMB before Charge. D-015's Full-Sweep ordinal movement, lethal-hit momentum, obstacle termination, and no-commit/no-push rules remain unchanged.

↪ close-range ownership and Enemy-collision termination superseded by D-017 (2026-08-22)

## D-017 · Give Charge meaningful range, consume one use per accepted Sweep, continue through Enemy collisions, and use the normal melee combo at close range — 2026-08-22 (user, direct bug report)

Quick, Power, and Full Medium-Chair attacks must differ in Chair reach, object speed, and victim travel; a tap may not reuse Power/Full launch distance. The first accepted Sweep or Slam victim consumes exactly one Cargo-Strap use for the attack sequence even when damage came from the non-alloc swept volume rather than `OnCollisionEnter`; multiple victims and duplicate callbacks do not consume extra uses, and final-use depletion is deferred until the attached attack releases safely. Enemy-to-Enemy contact during an active Horizontal Sweep transfers the next ordinal hit and emits the existing causal event but does not terminate the remaining Chair path; Cargo victims preserve their controlled movement through one another, while walls and Heavy world obstacles remain hard stops. A close Runner no longer triggers the special one-Jab disengage shove: LMB uses the same Jab→Cross→Hook input, damage, buffer, and finisher logic as normal unarmed combat while the Chair stays tensioned and Object Weapon remains selected. Chair Skid one-shot playback is removed; the quieter Roll loop and Stop/impact sounds remain. No commit or push is allowed before Human Owner review.

## D-018 · Replace the singular Runner attack token with a ramped Normal/Elite pressure budget — 2026-08-22 (user, direct task)

Runner pressure uses concurrent typed slots instead of one `attackTokenOwner`: 1–2 enemies permit one Normal attacker, 3–4 permit two, and 5+ permit at most three, while Elite owns one separate slot and all hard contacts remain under a total cap of three. Encounter pressure ramps from one attacker for the first `10–15 s`, to two in the middle, to `Normal 1–2 + Elite 1`; starts use a deterministic `0.15–0.30 s` global gap, each actor uses a deterministic `0.8–1.4 s` cooldown, Normal telegraphs stay in `0.20–0.35 s`, and Elite telegraphs stay in `0.35–0.55 s`. Runners denied a slot physically maintain distinct moving flank/ring angles instead of freezing or teleporting. Elite prioritizes player Charge, prolonged stationarity, and focus on another enemy without making Full Charge impossible. Player Damage immunity is short and separate from `0.25–0.40 s` stagger protection, same-frame damage deduplication, bounded Knockdown relaunch, Get-up protection, and lethal-state priority. Only off-screen enemies currently telegraphing receive an edge warning, and cancellation/reset removes it. Preserve existing Cargo/Chair behavior, approved enemy types/animations, Final Map isolation, and the no-commit/no-push rule.

The budgeted Telegraph is a true pre-animation interval: the selected attack cue is prepared but its authored strike clip does not begin until Telegraph ends, preventing natural hit events from expiring during a longer Elite warning. Attack-window close also samples and resolves the final swept-bone segment before clearing the sequence, so a contact on the closing animation frame cannot be lost or re-labelled as the next combo step.

## D-019 · Add a temporary non-batch 3 Normal + 1 Elite validation roster to v007 — 2026-08-22 (user, direct task)

Until the designer map arrives, manual Play of `Contest_OperationsFloor_Showcase_v007` dynamically expands the authored `Normal 1 + Elite 1` pressure roster to `Normal 3 + Elite 1` by cloning the existing approved Normal runner twice. The four active runners begin in distinct left-front, right-front, left-rear and right-rear lanes at approximately `6.5–10.5 m` from CHR00, face the player, and own resettable runtime baselines. The older P0-03 stationary Chair-validation roster is hidden for this pressure-validation session so it does not visually pollute the encounter. Batch/EditMode authority remains the original two authored runners; no Scene YAML, Final Map, prefab, enemy type or designer-map assumption is added. Runtime audio must resolve the live Pressure roster so cloned runners retain footsteps, hurt/death voice and accepted-contact melee audio. No commit or push is allowed before Human Owner review.

Closing-frame combat settlement is ordered as `final Humanoid LateUpdate sweep → pending contact resolution → attack-window close → combo/action advance`. This preserves the correct Jab damage/sequence under full-suite frame timing instead of allowing Cross to become the first accepted hit.
