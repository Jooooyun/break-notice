using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Encounter;
using BreakNotice.Gameplay.Input;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestChr00ObjectAnimationSyncPlayModeTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v007.unity";
        private static readonly Vector3 OpenRoutePlayerPlanarPosition =
            new Vector3(0f, 0f, -12f);
        private const float DirectAcquisitionDistance = 1.25f;

        private Scene loadedScene;
        private GameObject testView;
        private ShowcasePlayerCombatController combat;
        private PrototypeInteractionController interaction;
        private PrototypeInteractionStateOwner owner;
        private PrototypePhysicalObjectLifecycle lifecycle;
        private ShowcaseCharacterAnimationDriver driver;
        private ShowcasePlayerPropGripRig gripRig;
        private ShowcaseAnimationEventReceiver receiver;
        private InteractionReadabilityPresenter interactionPresenter;
        private InputSystemLogicalInputAdapter input;
        private AuthoredEncounterResetController reset;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (testView != null)
            {
                UnityEngine.Object.Destroy(testView);
                testView = null;
            }
            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(loadedScene);
                if (unload != null)
                {
                    yield return unload;
                }
            }
            loadedScene = default;
        }

        [UnityTest]
        public IEnumerator LightPickupCarryAndChargedThrow_UseNaturalEventFrames()
        {
            yield return LoadScene();
            PrototypeInteractionTarget binder = Target(
                "BINDER-DIRECT-01",
                "OBJ-P-L-001",
                PrototypeInteractionRole.DirectBinder);

            yield return AcquireNaturally(binder);
            yield return AssertLightCarryEnvelope(binder);
            yield return ThrowNaturally(binder);
        }

        [UnityTest]
        public IEnumerator MediumPickupCarryAndChargedThrow_UseNaturalEventFrames()
        {
            yield return LoadScene();
            PrototypeInteractionTarget chair = Target(
                "CHAIR-DIRECT-01",
                "OBJ-P-M-001",
                PrototypeInteractionRole.DirectChair);

            yield return AcquireNaturally(chair);
            AssertMediumSubjectEnvelope(chair);
            int acquisitionAppliedFrame = gripRig.LastAppliedPoseFrameId;
            yield return CollectAndAssertMediumCarryDiagnostics(
                chair,
                acquisitionAppliedFrame,
                1f);
            Assert.That(gripRig.IsAuthoritative, Is.True);
            Assert.That(gripRig.MediumIkWeight, Is.EqualTo(1f).Within(0.01f));
            AssertCarryPose(chair);
            AssertMediumHandGrip();
            AssertMediumSubjectEnvelope(chair);
            yield return AssertMediumCarryTracksHandsDuringMovement(chair);
            yield return ThrowNaturally(chair);
        }

        [UnityTest]
        public IEnumerator ResetAfterNaturalThrow_ClearsGripSnapshotAndOwnership()
        {
            yield return LoadScene();
            PrototypeInteractionTarget binder = Target(
                "BINDER-DIRECT-01",
                "OBJ-P-L-001",
                PrototypeInteractionRole.DirectBinder);

            yield return AcquireNaturally(binder);
            yield return ThrowNaturally(binder);
            Assert.That(lifecycle.LastPhysicalReleaseSnapshot.IsValid, Is.True);

            Assert.That(reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
            yield return null;
            yield return null;

            Assert.That(gripRig.Phase, Is.EqualTo(ShowcasePlayerPropGripPhase.None));
            Assert.That(gripRig.Target, Is.Null);
            Assert.That(gripRig.SequenceId, Is.Zero);
            Assert.That(gripRig.IsAuthoritative, Is.False);
            Assert.That(lifecycle.CarriedTarget, Is.Null);
            Assert.That(lifecycle.LastPhysicalReleaseSnapshot.IsValid, Is.False);
            Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
            Assert.That(owner.CarriedTarget, Is.Null);
        }

        [UnityTest]
        public IEnumerator AcquisitionContactBlockedByWorld_RollsBackPhysicalCarry()
        {
            yield return LoadScene();
            PrototypeInteractionTarget binder = Target(
                "BINDER-DIRECT-01",
                "OBJ-P-L-001",
                PrototypeInteractionRole.DirectBinder);
            PositionForDirectAcquisition(binder);
            Rigidbody body = binder.GetComponent<Rigidbody>();
            Collider targetCollider = binder.GetComponent<Collider>();
            CharacterController playerCollider = combat.GetComponent<CharacterController>();
            int originalLayer = binder.gameObject.layer;
            Vector3 originalPosition = body.position;
            Quaternion originalRotation = body.rotation;
            int contactBefore = receiver.CarryContactInvocationCount;

            Assert.That(interaction.RefreshTarget(false), Is.SameAs(binder));
            yield return null;
            PrototypeInteractionDecision decision = interaction.ProcessInteractPrimary(false);
            Assert.That(decision.IsAccepted, Is.True, decision.Rejection.ToString());
            Assert.That(owner.HandState,
                Is.EqualTo(PrototypeHandOwnershipState.AcquisitionCommitted));

            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "ACQUISITION_CONTACT_BLOCKER";
            SceneManager.MoveGameObjectToScene(wall, loadedScene);
            Vector3 handSide = combat.transform.position + Vector3.up * 1.1f +
                combat.transform.forward * 0.35f;
            wall.transform.position = Vector3.Lerp(
                binder.GetComponent<Collider>().bounds.center,
                handSide,
                0.5f);
            wall.transform.rotation = combat.transform.rotation;
            wall.transform.localScale = new Vector3(3f, 3f, 0.12f);
            Physics.SyncTransforms();

            yield return WaitUntilOrFail(
                () => receiver.CarryContactInvocationCount == contactBefore + 1,
                3f,
                "Blocked acquisition carry-contact AnimationEvent did not fire.");

            Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
            Assert.That(owner.CarriedTarget, Is.Null);
            Assert.That(lifecycle.CarriedTarget, Is.Null);
            Assert.That(gripRig.Phase, Is.EqualTo(ShowcasePlayerPropGripPhase.None));
            Assert.That(gripRig.IsAuthoritative, Is.False);
            Assert.That(body.isKinematic, Is.False);
            Assert.That(body.useGravity, Is.True);
            Assert.That(binder.gameObject.layer, Is.EqualTo(originalLayer));
            Assert.That(Vector3.Distance(body.position, originalPosition),
                Is.LessThanOrEqualTo(0.03f));
            Assert.That(Quaternion.Angle(body.rotation, originalRotation),
                Is.LessThanOrEqualTo(1f));
            Assert.That(Physics.GetIgnoreCollision(playerCollider, targetCollider), Is.False);

            combat.ResetToRunBaseline();
            UnityEngine.Object.Destroy(wall);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayerOverlapAtRelease_IsHandledByCollisionLease()
        {
            yield return LoadScene();
            PrototypeInteractionTarget binder = Target(
                "BINDER-DIRECT-01",
                "OBJ-P-L-001",
                PrototypeInteractionRole.DirectBinder);
            yield return AcquireNaturally(binder);

            CharacterController playerCollider = combat.GetComponent<CharacterController>();
            BoxCollider binderCollider = binder.GetComponent<BoxCollider>();
            Assert.That(playerCollider, Is.Not.Null);
            Assert.That(binderCollider, Is.Not.Null);
            playerCollider.height = 4f;
            playerCollider.radius = 2f;
            Physics.SyncTransforms();
            Assert.That(Physics.ComputePenetration(
                playerCollider,
                playerCollider.transform.position,
                playerCollider.transform.rotation,
                binderCollider,
                binderCollider.transform.position,
                binderCollider.transform.rotation,
                out _,
                out float overlapDistance) && overlapDistance > 0.001f,
                Is.True,
                "The fixture must exercise a leased player/prop overlap.");

            yield return ThrowNaturally(binder);
        }

        [UnityTest]
        public IEnumerator Disable_ClearsGripAuthorityAndPhysicalLeaseState()
        {
            yield return LoadScene();
            PrototypeInteractionTarget binder = Target(
                "BINDER-DIRECT-01",
                "OBJ-P-L-001",
                PrototypeInteractionRole.DirectBinder);
            int originalLayer = binder.gameObject.layer;
            yield return AcquireNaturally(binder);
            Rigidbody body = binder.GetComponent<Rigidbody>();
            Collider targetCollider = binder.GetComponent<Collider>();
            CharacterController playerCollider = combat.GetComponent<CharacterController>();

            driver.Animator.enabled = false;
            Assert.That(gripRig.IsAuthoritative, Is.False);
            yield return WaitUntilOrFail(
                () =>
                    gripRig.Phase == ShowcasePlayerPropGripPhase.None &&
                    lifecycle.CarriedTarget == null &&
                    owner.HandState == PrototypeHandOwnershipState.Free,
                1f,
                "Animator disable did not complete carry cleanup.");
            AssertDisabledCarryCleanup(
                binder,
                body,
                targetCollider,
                playerCollider,
                originalLayer);

            driver.Animator.enabled = true;
            yield return null;
            yield return AcquireNaturally(binder);

            gripRig.enabled = false;
            Assert.That(gripRig.IsAuthoritative, Is.False);
            yield return WaitUntilOrFail(
                () =>
                    gripRig.Phase == ShowcasePlayerPropGripPhase.None &&
                    lifecycle.CarriedTarget == null &&
                    owner.HandState == PrototypeHandOwnershipState.Free,
                1f,
                "Grip rig disable did not complete carry cleanup.");
            AssertDisabledCarryCleanup(
                binder,
                body,
                targetCollider,
                playerCollider,
                originalLayer);

            gripRig.enabled = true;
            yield return null;
            yield return AcquireNaturally(binder);

            Assert.That(Physics.GetIgnoreCollision(playerCollider, targetCollider), Is.True);
            lifecycle.enabled = false;
            AssertDisabledCarryCleanup(
                binder,
                body,
                targetCollider,
                playerCollider,
                originalLayer);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MediumCarryLifecycleDisable_CleansSynchronouslyAndRemainsSafe()
        {
            yield return LoadScene();
            PrototypeInteractionTarget chair = Target(
                "CHAIR-DIRECT-01",
                "OBJ-P-M-001",
                PrototypeInteractionRole.DirectChair);
            int originalLayer = chair.gameObject.layer;
            yield return AcquireNaturally(chair);

            Rigidbody body = chair.GetComponent<Rigidbody>();
            BoxCollider chairCollider = chair.GetComponent<BoxCollider>();
            CharacterController playerCollider = combat.GetComponent<CharacterController>();
            Assert.That(body, Is.Not.Null);
            Assert.That(chairCollider, Is.Not.Null);
            Assert.That(playerCollider, Is.Not.Null);
            AssertMediumSubjectEnvelope(chair);
            Assert.That(Physics.GetIgnoreCollision(playerCollider, chairCollider), Is.True);

            lifecycle.enabled = false;
            Physics.SyncTransforms();

            AssertDisabledCarryCleanup(
                chair,
                body,
                chairCollider,
                playerCollider,
                originalLayer);
            AssertMediumSubjectEnvelope(chair);
            AssertBodyStopped(body, "Synchronous lifecycle-disable cleanup");

            Vector3 cleanupPosition = body.position;
            yield return new WaitForFixedUpdate();
            yield return null;
            Physics.SyncTransforms();

            AssertMediumSubjectEnvelope(chair);
            Assert.That(Physics.GetIgnoreCollision(playerCollider, chairCollider), Is.False);
            AssertPassiveBodyDidNotExplode(
                body,
                cleanupPosition,
                "Lifecycle-disable cleanup next fixed step");
        }

        [UnityTest]
        public IEnumerator MediumCarryLifecycleDisable_WithNoSafePose_QuarantinesSynchronously()
        {
            yield return LoadScene();
            PrototypeInteractionTarget chair = Target(
                "CHAIR-DIRECT-01",
                "OBJ-P-M-001",
                PrototypeInteractionRole.DirectChair);
            yield return AcquireNaturally(chair);

            Rigidbody body = chair.GetComponent<Rigidbody>();
            BoxCollider chairCollider = chair.GetComponent<BoxCollider>();
            CharacterController playerCollider = combat.GetComponent<CharacterController>();
            Assert.That(body, Is.Not.Null);
            Assert.That(chairCollider, Is.Not.Null);
            Assert.That(playerCollider, Is.Not.Null);
            Assert.That(Physics.GetIgnoreCollision(playerCollider, chairCollider), Is.True);

            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "NO_SAFE_LEASE_RECOVERY_VOLUME";
            SceneManager.MoveGameObjectToScene(blocker, loadedScene);
            blocker.transform.SetPositionAndRotation(
                combat.transform.position + Vector3.up * 1.5f,
                Quaternion.identity);
            blocker.transform.localScale = Vector3.one * 12f;
            Physics.SyncTransforms();
            Assert.That(Physics.ComputePenetration(
                chairCollider,
                chairCollider.transform.position,
                chairCollider.transform.rotation,
                blocker.GetComponent<BoxCollider>(),
                blocker.transform.position,
                blocker.transform.rotation,
                out _,
                out float worldPenetration) && worldPenetration > 0.001f,
                Is.True,
                "The fixture must block the current, fallback, and bounded search poses.");

            LogAssert.Expect(
                LogType.Error,
                "PLAYER PROP COLLISION LEASE QUARANTINED: CHAIR-DIRECT-01. " +
                "No player-clear, world-safe pose was available during non-deferred cleanup.");
            lifecycle.enabled = false;

            Assert.That(chair.gameObject.activeSelf, Is.False);
            Assert.That(body.isKinematic, Is.True);
            Assert.That(body.useGravity, Is.False);
            AssertBodyStopped(body, "No-safe-pose quarantine");
            Assert.That(Physics.GetIgnoreCollision(playerCollider, chairCollider), Is.False);
            Assert.That(lifecycle.CarriedTarget, Is.Null);
            Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
            Assert.That(owner.CarriedTarget, Is.Null);
            Assert.That(gripRig.Phase, Is.EqualTo(ShowcasePlayerPropGripPhase.None));
            Assert.That(gripRig.Target, Is.Null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator LifecycleDisable_AfterCarriedBodyDestroyed_QuarantinesWithoutException()
        {
            yield return LoadScene();
            PrototypeInteractionTarget binder = Target(
                "BINDER-DIRECT-01",
                "OBJ-P-L-001",
                PrototypeInteractionRole.DirectBinder);
            yield return AcquireNaturally(binder);

            Rigidbody body = binder.GetComponent<Rigidbody>();
            BoxCollider binderCollider = binder.GetComponent<BoxCollider>();
            CharacterController playerCollider = combat.GetComponent<CharacterController>();
            Assert.That(body, Is.Not.Null);
            Assert.That(binderCollider, Is.Not.Null);
            Assert.That(playerCollider, Is.Not.Null);
            Assert.That(Physics.GetIgnoreCollision(playerCollider, binderCollider), Is.True);

            UnityEngine.Object.DestroyImmediate(body);
            Assert.That(binder.GetComponent<Rigidbody>(), Is.Null);
            LogAssert.Expect(
                LogType.Error,
                "PLAYER PROP COLLISION LEASE QUARANTINED: BINDER-DIRECT-01. " +
                "The Rigidbody was unavailable during synchronous cleanup.");
            lifecycle.enabled = false;

            Assert.That(binder.gameObject.activeSelf, Is.False);
            Assert.That(Physics.GetIgnoreCollision(playerCollider, binderCollider), Is.False);
            Assert.That(lifecycle.CarriedTarget, Is.Null);
            Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
            Assert.That(owner.CarriedTarget, Is.Null);
            Assert.That(gripRig.Phase, Is.EqualTo(ShowcasePlayerPropGripPhase.None));
            Assert.That(gripRig.Target, Is.Null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MediumDirectThrowWindupAnimatorDisable_CancelsWithoutPenetration()
        {
            yield return LoadScene();
            PrototypeInteractionTarget chair = Target(
                "CHAIR-DIRECT-01",
                "OBJ-P-M-001",
                PrototypeInteractionRole.DirectChair);
            int originalLayer = chair.gameObject.layer;
            yield return AcquireNaturally(chair);

            Rigidbody body = chair.GetComponent<Rigidbody>();
            BoxCollider chairCollider = chair.GetComponent<BoxCollider>();
            CharacterController playerCollider = combat.GetComponent<CharacterController>();
            Assert.That(body, Is.Not.Null);
            Assert.That(chairCollider, Is.Not.Null);
            Assert.That(playerCollider, Is.Not.Null);
            AssertMediumSubjectEnvelope(chair);
            Assert.That(Physics.GetIgnoreCollision(playerCollider, chairCollider), Is.True);

            yield return BeginDirectThrowWindup(chair);
            Assert.That(combat.IsActionLocked, Is.True);
            Assert.That(gripRig.Phase, Is.EqualTo(ShowcasePlayerPropGripPhase.Throw));

            LogAssert.Expect(
                LogType.Error,
                "SHOWCASE PLAYER PROP RUNTIME UNAVAILABLE: DirectThrow. " +
                "The pending action and mobility lock are being cancelled.");
            driver.Animator.enabled = false;

            yield return WaitUntilOrFail(
                () =>
                    !combat.IsActionLocked &&
                    combat.AnimationCue == ShowcasePlayerAnimationCue.None &&
                    owner.HandState == PrototypeHandOwnershipState.Free &&
                    owner.CarriedTarget == null &&
                    owner.ThrowCommittedTarget == null &&
                    lifecycle.CarriedTarget == null &&
                    gripRig.Phase == ShowcasePlayerPropGripPhase.None &&
                    gripRig.Target == null &&
                    !gripRig.IsAuthoritative,
                1f,
                "Animator disable during Medium direct throw windup did not clear " +
                "combat, ownership, lifecycle, and grip state.");
            Physics.SyncTransforms();

            AssertDisabledCarryCleanup(
                chair,
                body,
                chairCollider,
                playerCollider,
                originalLayer);
            Assert.That(combat.IsActionLocked, Is.False);
            Assert.That(combat.AnimationCue, Is.EqualTo(ShowcasePlayerAnimationCue.None));
            Assert.That(owner.ThrowCommittedTarget, Is.Null);
            AssertMediumSubjectEnvelope(chair);
            AssertPassiveBodyDidNotExplode(
                body,
                body.position,
                "Animator-disable direct-windup cleanup");

            Vector3 cleanupPosition = body.position;
            yield return new WaitForFixedUpdate();
            yield return null;
            Physics.SyncTransforms();

            AssertMediumSubjectEnvelope(chair);
            Assert.That(Physics.GetIgnoreCollision(playerCollider, chairCollider), Is.False);
            AssertPassiveBodyDidNotExplode(
                body,
                cleanupPosition,
                "Animator-disable direct-windup next fixed step");
        }

        private void AssertDisabledCarryCleanup(
            PrototypeInteractionTarget target,
            Rigidbody body,
            Collider targetCollider,
            CharacterController playerCollider,
            int originalLayer)
        {
            Assert.That(gripRig.IsAuthoritative, Is.False);
            Assert.That(gripRig.Phase, Is.EqualTo(ShowcasePlayerPropGripPhase.None));
            Assert.That(gripRig.Target, Is.Null);
            Assert.That(lifecycle.CarriedTarget, Is.Null);
            Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
            Assert.That(owner.CarriedTarget, Is.Null);
            Assert.That(body.isKinematic, Is.False);
            Assert.That(body.useGravity, Is.True);
            Assert.That(target.gameObject.layer, Is.EqualTo(originalLayer));
            Assert.That(Physics.GetIgnoreCollision(playerCollider, targetCollider), Is.False,
                $"Player collision lease remained ignored for {target.name}. " +
                $"bodyWorld={body.position:F5} " +
                $"bodyLocal={combat.transform.InverseTransformPoint(body.position):F5} " +
                $"playerWorld={combat.transform.position:F5}.");
        }

        private IEnumerator LoadScene()
        {
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
            Assert.That(load, Is.Not.Null);
            yield return load;
            loadedScene = SceneManager.GetSceneByPath(ScenePath);
            Assert.That(loadedScene.IsValid() && loadedScene.isLoaded, Is.True);
            yield return null;

            combat = SceneComponents<ShowcasePlayerCombatController>().Single();
            interaction = combat.GetComponent<PrototypeInteractionController>();
            owner = combat.GetComponent<PrototypeInteractionStateOwner>();
            lifecycle = combat.GetComponent<PrototypePhysicalObjectLifecycle>();
            input = combat.GetComponent<InputSystemLogicalInputAdapter>();
            driver = combat.GetComponent<ShowcaseCharacterAnimationDriver>();
            gripRig = driver.Animator.GetComponent<ShowcasePlayerPropGripRig>();
            receiver = driver.Animator.GetComponent<ShowcaseAnimationEventReceiver>();
            interactionPresenter = SceneComponents<InteractionReadabilityPresenter>().Single();
            reset = SceneComponents<AuthoredEncounterResetController>().Single();

            Assert.That(interaction, Is.Not.Null);
            Assert.That(owner, Is.Not.Null);
            Assert.That(lifecycle, Is.Not.Null);
            Assert.That(input, Is.Not.Null);
            Assert.That(gripRig, Is.Not.Null);
            Assert.That(receiver, Is.Not.Null);
            Assert.That(interactionPresenter, Is.Not.Null);
            input.enabled = false;
            foreach (ShowcaseRunnerAgent runner in SceneComponents<ShowcaseRunnerAgent>())
            {
                runner.enabled = false;
            }
        }

        private IEnumerator AcquireNaturally(PrototypeInteractionTarget target)
        {
            PositionForDirectAcquisition(target);
            uint gripSequenceBefore = gripRig.SequenceId;
            int contactBefore = receiver.CarryContactInvocationCount;
            int completionBefore = receiver.ActionCompleteInvocationCount;
            ShowcasePlayerAnimationCue expectedCue =
                target.WeightClass == PrototypeWeightClass.Medium
                    ? ShowcasePlayerAnimationCue.PickupLowMedium
                    : ShowcasePlayerAnimationCue.PickupLowSmall;
            string expectedState = target.WeightClass == PrototypeWeightClass.Medium
                ? "PickupLowMediumAction"
                : "PickupLowSmallAction";

            Assert.That(interaction.RefreshTarget(false), Is.SameAs(target));
            yield return null;
            float acquisitionStartedAt = Time.time;
            PrototypeInteractionDecision decision = interaction.ProcessInteractPrimary(false);
            Assert.That(decision.IsAccepted, Is.True, decision.Rejection.ToString());
            Assert.That(decision.Outcome, Is.EqualTo(PrototypeInteractionOutcome.Scoop));
            Assert.That(combat.AnimationCue, Is.EqualTo(expectedCue));
            Assert.That(owner.HandState,
                Is.EqualTo(PrototypeHandOwnershipState.AcquisitionCommitted));
            Assert.That(lifecycle.CarriedTarget, Is.Null);
            Assert.That(gripRig.Phase,
                Is.EqualTo(ShowcasePlayerPropGripPhase.AcquisitionPending));
            Assert.That(gripRig.SequenceId, Is.EqualTo(gripSequenceBefore + 1));
            Assert.That(gripRig.Target, Is.SameAs(target));

            yield return WaitUntilOrFail(
                () => driver.CurrentBaseStateName == expectedState,
                1f,
                $"{expectedState} did not become the active CHR00 state.");

            yield return new WaitForSeconds(0.1f);
            Assert.That(receiver.CarryContactInvocationCount, Is.EqualTo(contactBefore));
            Assert.That(lifecycle.CarriedTarget, Is.Null);
            yield return WaitUntilOrFail(
                () => receiver.CarryContactInvocationCount == contactBefore + 1,
                3f,
                "Natural carry-contact AnimationEvent did not fire.");

            Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Carrying));
            Assert.That(owner.CarriedTarget, Is.SameAs(target));
            Assert.That(lifecycle.CarriedTarget, Is.SameAs(target));
            Assert.That(gripRig.Target, Is.SameAs(target));
            Assert.That(gripRig.Phase,
                Is.EqualTo(ShowcasePlayerPropGripPhase.AcquisitionAttached));
            float contactBudget = target.WeightClass == PrototypeWeightClass.Medium
                ? 0.65f
                : 0.58f;
            Assert.That(Time.time - acquisitionStartedAt,
                Is.LessThanOrEqualTo(contactBudget),
                $"{target.WeightClass} pickup contact remained too slow.");
            yield return null;
            AssertCarryPose(target);

            yield return WaitUntilOrFail(
                () => !combat.IsActionLocked,
                6f,
                "Natural acquisition completion event did not release the action.");
            Assert.That(receiver.CarryContactInvocationCount, Is.EqualTo(contactBefore + 1));
            Assert.That(receiver.ActionCompleteInvocationCount,
                Is.EqualTo(completionBefore + 1));
            float completionBudget = target.WeightClass == PrototypeWeightClass.Medium
                ? 1.75f
                : 1.55f;
            Assert.That(Time.time - acquisitionStartedAt,
                Is.LessThanOrEqualTo(completionBudget),
                $"{target.WeightClass} pickup kept the player locked too long.");
            Assert.That(gripRig.Phase, Is.EqualTo(ShowcasePlayerPropGripPhase.Carry));
            yield return null;
            AssertCarryPose(target);
        }

        private IEnumerator AssertLightCarryEnvelope(PrototypeInteractionTarget target)
        {
            CharacterController movement = combat.GetComponent<CharacterController>();
            Assert.That(movement, Is.Not.Null);
            input.enabled = false;
            for (int frame = 0; frame < 60; frame++)
            {
                bool aiming = frame >= 40;
                SetInputSnapshot(input, new LogicalInputSnapshot(
                    Vector2.zero,
                    Vector2.zero,
                    aiming,
                    false,
                    false,
                    false,
                    false,
                    false));
                if (frame >= 20 && frame < 40)
                {
                    movement.Move(combat.transform.forward * 0.01f);
                }
                yield return null;
                AssertCarryPose(target);
            }
            SetInputSnapshot(input, default);
        }

        private IEnumerator ThrowNaturally(PrototypeInteractionTarget target)
        {
            int releaseBefore = receiver.ThrowReleaseInvocationCount;
            int completionBefore = receiver.ActionCompleteInvocationCount;
            string expectedState = target.WeightClass == PrototypeWeightClass.Medium
                ? "ThrowMediumAction"
                : "ThrowLightAction";
            SetInputSnapshot(input, new LogicalInputSnapshot(
                Vector2.zero,
                Vector2.zero,
                false,
                false,
                false,
                false,
                false,
                false,
                throwHeld: true));
            Assert.That(interaction.BeginThrowCharge(), Is.True);
            yield return new WaitForSecondsRealtime(0.12f);
            float charge01 = interaction.ThrowCharge01;
            Assert.That(charge01, Is.GreaterThan(0.05f));
            Assert.That(interaction.TryGetThrowPreview(
                out PrototypeThrowSolution previewSolution), Is.True);
            Assert.That(previewSolution.InitialVelocity.y, Is.GreaterThan(0f));
            var previewPoints = new Vector3[26];
            Assert.That(interaction.TryBuildThrowTrajectoryPreview(
                previewPoints,
                out int previewPointCount), Is.True);
            Assert.That(previewPointCount, Is.GreaterThanOrEqualTo(4));
            Assert.That(previewPoints[1].y, Is.GreaterThan(previewPoints[0].y));
            Assert.That(interactionPresenter.IsThrowTrajectoryVisible, Is.True);
            Assert.That(interactionPresenter.ThrowTrajectoryPointCount,
                Is.GreaterThanOrEqualTo(4));
            bool releaseAccepted = interaction.ReleaseThrowCharge();
            SetInputSnapshot(input, default);
            Assert.That(releaseAccepted, Is.True);
            Assert.That(interaction.ThrowPresentation.Phase,
                Is.EqualTo(PrototypeThrowPresentationPhase.ReleaseAccepted));
            Assert.That(interaction.ThrowPresentation.Target, Is.SameAs(target));
            Assert.That(owner.CarriedTarget, Is.SameAs(target));
            Assert.That(lifecycle.CarriedTarget, Is.SameAs(target));
            Assert.That(gripRig.Phase, Is.EqualTo(ShowcasePlayerPropGripPhase.Throw));

            yield return WaitUntilOrFail(
                () => driver.CurrentBaseStateName == expectedState,
                1f,
                $"{expectedState} did not become the active CHR00 state.");

            yield return new WaitForSeconds(0.1f);
            Assert.That(receiver.ThrowReleaseInvocationCount, Is.EqualTo(releaseBefore));
            Assert.That(lifecycle.CarriedTarget, Is.SameAs(target));
            yield return WaitUntilOrFail(
                () => receiver.ThrowReleaseInvocationCount == releaseBefore + 1,
                3f,
                "Natural throw-release AnimationEvent did not fire.");

            PhysicalReleaseSnapshot snapshot = lifecycle.LastPhysicalReleaseSnapshot;
            Assert.That(snapshot.IsValid, Is.True);
            Assert.That(snapshot.Target, Is.SameAs(target));
            Assert.That(snapshot.FrameId,
                Is.InRange(Time.frameCount - 1, Time.frameCount));
            Assert.That(IsFinite(snapshot.Position), Is.True);
            Assert.That(IsFinite(snapshot.Rotation), Is.True);
            Assert.That(gripRig.ReleasePoseError, Is.LessThanOrEqualTo(0.03f));
            Assert.That(gripRig.CurrentGripRotationError, Is.LessThanOrEqualTo(5f));
            Assert.That(snapshot.LinearVelocity.magnitude,
                Is.EqualTo(lifecycle.GetThrowSpeed(target.WeightClass, charge01))
                    .Within(0.02f));
            Assert.That(snapshot.LinearVelocity.y, Is.GreaterThan(0f),
                "The committed physical throw must begin with the visible upward arc.");
            Assert.That(lifecycle.CarriedTarget, Is.Null);
            Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
            Assert.That(gripRig.Phase, Is.EqualTo(ShowcasePlayerPropGripPhase.None));
            Assert.That(receiver.ThrowReleaseInvocationCount, Is.EqualTo(releaseBefore + 1));
            yield return null;
            Assert.That(interactionPresenter.IsThrowTrajectoryVisible, Is.False);
            yield return WaitUntilOrFail(
                () => !combat.IsActionLocked,
                3f,
                "Natural throw completion event did not release the action.");
            Assert.That(receiver.ActionCompleteInvocationCount,
                Is.EqualTo(completionBefore + 1));
        }

        private IEnumerator BeginDirectThrowWindup(PrototypeInteractionTarget target)
        {
            SetInputSnapshot(input, new LogicalInputSnapshot(
                Vector2.zero,
                Vector2.zero,
                false,
                false,
                false,
                false,
                false,
                false,
                throwHeld: true));
            Assert.That(interaction.BeginThrowCharge(), Is.True);
            yield return new WaitForSecondsRealtime(0.12f);
            Assert.That(interaction.ThrowCharge01, Is.GreaterThan(0.05f));
            bool releaseAccepted = interaction.ReleaseThrowCharge();
            SetInputSnapshot(input, default);

            Assert.That(releaseAccepted, Is.True);
            Assert.That(interaction.ThrowPresentation.Phase,
                Is.EqualTo(PrototypeThrowPresentationPhase.ReleaseAccepted));
            Assert.That(interaction.ThrowPresentation.Target, Is.SameAs(target));
            Assert.That(owner.CarriedTarget, Is.SameAs(target));
            Assert.That(lifecycle.CarriedTarget, Is.SameAs(target));
            Assert.That(gripRig.Phase, Is.EqualTo(ShowcasePlayerPropGripPhase.Throw));
        }

        private void PositionForDirectAcquisition(PrototypeInteractionTarget target)
        {
            Rigidbody body = target.GetComponent<Rigidbody>();
            Collider collider = target.GetComponent<Collider>();
            CharacterController movement = combat.GetComponent<CharacterController>();
            Assert.That(body, Is.Not.Null);
            Assert.That(collider, Is.Not.Null);
            Assert.That(movement, Is.Not.Null);

            bool movementWasEnabled = movement.enabled;
            movement.enabled = false;
            combat.transform.SetPositionAndRotation(
                new Vector3(
                    OpenRoutePlayerPlanarPosition.x,
                    combat.transform.position.y,
                    OpenRoutePlayerPlanarPosition.z),
                Quaternion.identity);
            movement.enabled = movementWasEnabled;

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            Vector3 targetPlanarPosition = combat.transform.position +
                combat.transform.forward * DirectAcquisitionDistance;
            body.position = new Vector3(
                targetPlanarPosition.x,
                combat.transform.position.y + 2f,
                targetPlanarPosition.z);
            body.rotation = combat.transform.rotation;
            target.transform.SetPositionAndRotation(body.position, body.rotation);
            Physics.SyncTransforms();

            float rootToBottom = target.transform.position.y - collider.bounds.min.y;
            float supportHeight = FindOpenRouteSupportHeight(target, targetPlanarPosition);
            body.position = new Vector3(
                targetPlanarPosition.x,
                supportHeight + rootToBottom + 0.005f,
                targetPlanarPosition.z);
            target.transform.SetPositionAndRotation(body.position, body.rotation);
            Physics.SyncTransforms();
            AssertOpenRouteIsClear(target);

            testView = new GameObject("CHR00 Object Sync Test View");
            SceneManager.MoveGameObjectToScene(testView, loadedScene);
            testView.transform.position = combat.transform.position + Vector3.up * 1.15f;
            testView.transform.rotation = Quaternion.LookRotation(
                collider.bounds.center - testView.transform.position);
            interaction.Configure(
                input,
                owner,
                testView.transform,
                new[] { target },
                2.8f,
                false,
                lifecycle,
                interaction.AllyRelayController,
                authoredFullThrowChargeDuration: interaction.FullThrowChargeDuration);
            Physics.SyncTransforms();
        }

        private float FindOpenRouteSupportHeight(
            PrototypeInteractionTarget target,
            Vector3 targetPlanarPosition)
        {
            RaycastHit[] hits = Physics.RaycastAll(
                new Vector3(targetPlanarPosition.x, 4f, targetPlanarPosition.z),
                Vector3.down,
                8f,
                ~0,
                QueryTriggerInteraction.Ignore);
            RaycastHit support = hits
                .Where(hit =>
                    hit.collider != null &&
                    hit.normal.y >= 0.95f &&
                    !IsInHierarchy(hit.collider, combat.transform) &&
                    !IsInHierarchy(hit.collider, target.transform))
                .OrderByDescending(hit => hit.point.y)
                .FirstOrDefault();

            Assert.That(support.collider, Is.Not.Null,
                "The authored Z0-to-Z1 open route must retain a physical floor support.");
            Assert.That(support.point.y, Is.EqualTo(0f).Within(0.02f),
                $"Unexpected support '{support.collider.name}' in the authored open route.");
            return support.point.y;
        }

        private void AssertOpenRouteIsClear(PrototypeInteractionTarget target)
        {
            Vector3 queryCenter = combat.transform.position + combat.transform.forward;
            Collider[] blockers = Physics.OverlapBox(
                    queryCenter,
                    new Vector3(1.25f, 0.85f, 2f),
                    combat.transform.rotation,
                    ~0,
                    QueryTriggerInteraction.Ignore)
                .Where(candidate =>
                    candidate != null &&
                    candidate.enabled &&
                    !IsInHierarchy(candidate, combat.transform) &&
                    !IsInHierarchy(candidate, target.transform))
                .ToArray();

            Assert.That(blockers, Is.Empty,
                "The direct-acquisition fixture must use the real unobstructed Z0-to-Z1 " +
                $"route. Blocking colliders: {string.Join(", ", blockers.Select(BlockerPath))}");
        }

        private static bool IsInHierarchy(Collider candidate, Transform root)
        {
            return candidate.transform == root || candidate.transform.IsChildOf(root);
        }

        private static string BlockerPath(Collider collider)
        {
            return $"{collider.transform.name} [{collider.GetType().Name}]";
        }

        private void AssertCarryPose(PrototypeInteractionTarget target)
        {
            Assert.That(gripRig.IsAuthoritative, Is.True);
            Assert.That(gripRig.Target, Is.SameAs(target));
            Assert.That(lifecycle.CarriedTarget, Is.SameAs(target));
            Assert.That(gripRig.LastAppliedPoseFrameId,
                Is.InRange(Time.frameCount - 1, Time.frameCount));
            Assert.That(gripRig.CurrentGripPositionError, Is.LessThanOrEqualTo(0.03f));
            Assert.That(gripRig.CurrentGripRotationError, Is.LessThanOrEqualTo(5f));
            Assert.That(lifecycle.GripSeparation, Is.LessThanOrEqualTo(0.03f));
            Assert.That(lifecycle.GripWorldBlocked, Is.False);
        }

        private void AssertMediumHandGrip()
        {
            Assert.That(gripRig.MediumLeftHandGripPositionError,
                Is.LessThanOrEqualTo(0.03f));
            Assert.That(gripRig.MediumRightHandGripPositionError,
                Is.LessThanOrEqualTo(0.03f));
        }

        private IEnumerator CollectAndAssertMediumCarryDiagnostics(
            PrototypeInteractionTarget target,
            int initialAppliedFrame,
            float timeoutSeconds)
        {
            var snapshots = new List<string>(3);
            int lastRecordedFrame = initialAppliedFrame;
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (snapshots.Count < 3 && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
                int appliedFrame = gripRig.LastAppliedPoseFrameId;
                if (gripRig.Phase != ShowcasePlayerPropGripPhase.Carry ||
                    appliedFrame <= lastRecordedFrame)
                {
                    continue;
                }

                lastRecordedFrame = appliedFrame;
                snapshots.Add(FormatMediumCarryDiagnostic(target, appliedFrame));
            }

            string trace = string.Join(Environment.NewLine, snapshots);
            Debug.Log("CHR00 medium carry finalized-pose trace:" + Environment.NewLine + trace);
            Assert.That(snapshots, Has.Count.EqualTo(3),
                "Medium carry did not produce three distinct finalized applied frames." +
                Environment.NewLine + trace);
            Assert.That(gripRig.MediumLeftHandGripPositionError,
                Is.LessThanOrEqualTo(0.03f), trace);
            Assert.That(gripRig.MediumRightHandGripPositionError,
                Is.LessThanOrEqualTo(0.03f), trace);
        }

        private string FormatMediumCarryDiagnostic(
            PrototypeInteractionTarget target,
            int appliedFrame)
        {
            Transform leftHand = driver.Animator.GetBoneTransform(HumanBodyBones.LeftHand);
            Transform rightHand = driver.Animator.GetBoneTransform(HumanBodyBones.RightHand);
            Transform leftUpperArm = driver.Animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            Transform rightUpperArm = driver.Animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            Transform stableAnchor = combat.transform.Find("ContestCarryAnchor");
            BoxCollider chair = target.GetComponent<BoxCollider>();
            bool hasDesiredPose = gripRig.TryGetDesiredObjectPose(target, out Pose desiredPose);

            Vector3 desiredLeftGrip = Vector3.positiveInfinity;
            Vector3 desiredRightGrip = Vector3.positiveInfinity;
            if (hasDesiredPose && chair != null)
            {
                Vector3 localGrip = chair.center +
                    Vector3.up * (chair.size.y * 0.5f * 0.78f) +
                    Vector3.back * ((chair.size.z * 0.5f) + 0.070f);
                Vector3 scale = target.transform.lossyScale;
                Vector3 scaledLocalGrip = Vector3.Scale(
                    localGrip,
                    new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
                Vector3 midpoint = desiredPose.position +
                    desiredPose.rotation * scaledLocalGrip;
                Vector3 side = desiredPose.rotation * Vector3.right;
                desiredLeftGrip = midpoint - side * 0.12f;
                desiredRightGrip = midpoint + side * 0.12f;
            }

            string desired = hasDesiredPose
                ? $"pos={desiredPose.position:F5} rot={desiredPose.rotation.eulerAngles:F3} " +
                  $"playerLocal={combat.transform.InverseTransformPoint(desiredPose.position):F5}"
                : "<unavailable>";
            string anchor = stableAnchor != null
                ? $"world={stableAnchor.position:F5} local={stableAnchor.localPosition:F5}"
                : "<missing>";
            return
                $"appliedFrame={appliedFrame} phase={gripRig.Phase} " +
                $"leftError={gripRig.MediumLeftHandGripPositionError:F6} " +
                $"rightError={gripRig.MediumRightHandGripPositionError:F6} " +
                $"ikWeight={gripRig.MediumIkWeight:F4} " +
                $"leftHand={leftHand?.position.ToString("F5") ?? "<missing>"} " +
                $"rightHand={rightHand?.position.ToString("F5") ?? "<missing>"} " +
                $"leftUpperArm={leftUpperArm?.position.ToString("F5") ?? "<missing>"} " +
                $"rightUpperArm={rightUpperArm?.position.ToString("F5") ?? "<missing>"} " +
                $"desiredLeftGrip={desiredLeftGrip:F5} " +
                $"desiredRightGrip={desiredRightGrip:F5} " +
                $"chairRoot={target.transform.position:F5} " +
                $"playerRoot={combat.transform.position:F5} " +
                $"stableAnchor[{anchor}] desired[{desired}]";
        }

        private IEnumerator AssertMediumCarryTracksHandsDuringMovement(
            PrototypeInteractionTarget target)
        {
            input.enabled = false;
            for (int frame = 0; frame < 8; frame++)
            {
                SetInputSnapshot(input, new LogicalInputSnapshot(
                    Vector2.up,
                    Vector2.zero,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false));
                yield return null;
                SetInputSnapshot(input, default);
                // The zero-input frame lets the previous locomotion Update be
                // consumed by the lifecycle's same-frame LateUpdate before the
                // coroutine observes the final applied prop pose.
                yield return null;
                AssertCarryPose(target);
                AssertMediumHandGrip();
                AssertMediumSubjectEnvelope(target);
            }
            SetInputSnapshot(input, default);
        }

        private void AssertMediumSubjectEnvelope(PrototypeInteractionTarget target)
        {
            CharacterController playerCollider = combat.GetComponent<CharacterController>();
            BoxCollider chairCollider = target.GetComponent<BoxCollider>();
            Assert.That(playerCollider, Is.Not.Null);
            Assert.That(chairCollider, Is.Not.Null);

            bool penetrates = Physics.ComputePenetration(
                playerCollider,
                playerCollider.transform.position,
                playerCollider.transform.rotation,
                chairCollider,
                chairCollider.transform.position,
                chairCollider.transform.rotation,
                out _,
                out float penetrationDepth) && penetrationDepth > 0.001f;
            Assert.That(penetrates, Is.False,
                $"Settled Medium carry entered CHR00's physical envelope by " +
                $"{penetrationDepth:F6}m. frame={Time.frameCount} " +
                $"applied={gripRig.LastAppliedPoseFrameId} " +
                $"chairLocal={combat.transform.InverseTransformPoint(target.transform.position):F5} " +
                $"chairWorld={target.transform.position:F5} " +
                $"playerWorld={combat.transform.position:F5}.");
        }

        private static void AssertBodyStopped(Rigidbody body, string context)
        {
            Assert.That(IsFinite(body.linearVelocity), Is.True, context);
            Assert.That(IsFinite(body.angularVelocity), Is.True, context);
            Assert.That(body.linearVelocity.magnitude,
                Is.LessThanOrEqualTo(0.001f),
                $"{context} left linear velocity {body.linearVelocity:F6}.");
            Assert.That(body.angularVelocity.magnitude,
                Is.LessThanOrEqualTo(0.001f),
                $"{context} left angular velocity {body.angularVelocity:F6}.");
        }

        private static void AssertPassiveBodyDidNotExplode(
            Rigidbody body,
            Vector3 cleanupPosition,
            string context)
        {
            Assert.That(IsFinite(body.position), Is.True, context);
            Assert.That(IsFinite(body.linearVelocity), Is.True, context);
            Assert.That(IsFinite(body.angularVelocity), Is.True, context);

            float passiveLinearSpeedLimit =
                Physics.gravity.magnitude * Time.fixedDeltaTime + 0.05f;
            float passiveTranslationLimit =
                passiveLinearSpeedLimit * Time.fixedDeltaTime + 0.01f;
            Assert.That(body.linearVelocity.magnitude,
                Is.LessThanOrEqualTo(passiveLinearSpeedLimit),
                $"{context} produced linear velocity {body.linearVelocity:F6}.");
            Assert.That(body.angularVelocity.magnitude,
                Is.LessThanOrEqualTo(0.05f),
                $"{context} produced angular velocity {body.angularVelocity:F6}.");
            Assert.That(Vector3.Distance(body.position, cleanupPosition),
                Is.LessThanOrEqualTo(passiveTranslationLimit),
                $"{context} displaced the Medium prop from {cleanupPosition:F6} " +
                $"to {body.position:F6}.");
        }

        private PrototypeInteractionTarget Target(
            string runtimeInstanceKey,
            string contentDefinitionId,
            PrototypeInteractionRole role)
        {
            PrototypeInteractionTarget[] matches = SceneComponents<PrototypeInteractionTarget>()
                .Where(target => string.Equals(
                    target.RuntimeInstanceKey,
                    runtimeInstanceKey,
                    StringComparison.Ordinal))
                .ToArray();
            Assert.That(matches, Has.Length.EqualTo(1), runtimeInstanceKey);
            PrototypeInteractionTarget target = matches[0];
            Assert.That(target.ContentDefinitionId, Is.EqualTo(contentDefinitionId));
            Assert.That(target.Role, Is.EqualTo(role));
            return target;
        }

        private static void SetInputSnapshot(
            InputSystemLogicalInputAdapter adapter,
            LogicalInputSnapshot value)
        {
            PropertyInfo snapshot = typeof(InputSystemLogicalInputAdapter).GetProperty(
                nameof(InputSystemLogicalInputAdapter.Snapshot),
                BindingFlags.Instance | BindingFlags.Public);
            MethodInfo setter = snapshot?.GetSetMethod(true);
            Assert.That(setter, Is.Not.Null);
            setter.Invoke(adapter, new object[] { value });
        }

        private static IEnumerator WaitUntilOrFail(
            Func<bool> predicate,
            float timeoutSeconds,
            string message)
        {
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!predicate() && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }
            Assert.That(predicate(), Is.True, message);
        }

        private static bool IsFinite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
                   !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
                   !float.IsNaN(value.z) && !float.IsInfinity(value.z);
        }

        private static bool IsFinite(Quaternion value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
                   !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
                   !float.IsNaN(value.z) && !float.IsInfinity(value.z) &&
                   !float.IsNaN(value.w) && !float.IsInfinity(value.w);
        }

        private T[] SceneComponents<T>() where T : Component
        {
            return loadedScene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }
    }
}
