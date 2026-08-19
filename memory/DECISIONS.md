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
