using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Encounter;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestChr01SupplyAnimationSyncPlayModeTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v007.unity";
        private const float BinderPositionTolerance = 0.01f;
        private const float BinderRotationTolerance = 1f;
        private const float HandPositionTolerance = 0.03f;
        private const float RawHandResidualTolerance = 0.05f;
        private const float GripStabilityTolerance = 0.002f;
        private const float PenetrationTolerance = 0.001f;
        private const float GroundTolerance = 0.01f;
        private static readonly Vector3 OpenRouteAllyPlanarPosition =
            new Vector3(0f, 0f, -12f);
        private static readonly Vector3 OpenRouteChairPlanarPosition =
            new Vector3(0f, 0f, -9f);

        private Scene loadedScene;
        private PrototypeAllyRelayController relay;
        private ShowcaseCharacterAnimationDriver driver;
        private ShowcaseAnimationEventReceiver receiver;
        private ShowcaseBinderGripSocket binderSocket;
        private ShowcaseChairGripRig chairGrip;
        private CharacterController allyController;
        private AuthoredEncounterResetController reset;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
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
        public IEnumerator BinderFetchReadyAndRelease_CommitsAtOneOwnedTransitionBoundary()
        {
            yield return LoadScene();
            PrototypeInteractionTarget binder = Target(
                "BINDER-ALLY-01",
                PrototypeInteractionRole.AllyBinder);
            Rigidbody body = binder.GetComponent<Rigidbody>();
            Assert.That(body, Is.Not.Null);

            int contactBefore = relay.BinderWindupReadyContactCount;
            Vector3 preContactPosition = body.position;
            Quaternion preContactRotation = body.rotation;
            Assert.That(relay.TryRequestFetch(
                binder,
                out PrototypeAllyRelayRejection fetchRejection),
                Is.True,
                fetchRejection.ToString());

            yield return WaitUntilOrFail(
                () => relay.IsAwaitingBinderWindupReadyContact,
                20f,
                "Actual v007 CHR01 did not reach the Binder windup transaction.");
            uint sequence = relay.BinderWindupReadySequence;
            Assert.That(sequence, Is.Not.Zero);
            Assert.That(relay.BinderWindupReadyTarget, Is.SameAs(binder));
            Assert.That(relay.BinderWindupReadyContactCount, Is.EqualTo(contactBefore));
            preContactPosition = body.position;
            preContactRotation = body.rotation;

            int windupHash = Animator.StringToHash("Base Layer.BinderWindupReady");
            yield return WaitUntilOrFail(
                () => driver.Animator.IsInTransition(0) &&
                      driver.Animator.GetNextAnimatorStateInfo(0).fullPathHash == windupHash,
                1f,
                "Binder windup cross-fade was not observable before contact.");
            Assert.That(relay.BinderWindupReadyContactCount, Is.EqualTo(contactBefore));
            Assert.That(relay.IsBinderWindupReadyContactCommitted, Is.False);
            Assert.That(Vector3.Distance(body.position, preContactPosition),
                Is.LessThanOrEqualTo(0.001f),
                "The Binder moved before the windup transition completed.");
            Assert.That(Quaternion.Angle(body.rotation, preContactRotation),
                Is.LessThanOrEqualTo(0.1f),
                "The Binder rotated before the windup transition completed.");

            yield return WaitUntilOrFail(
                () => relay.BinderWindupReadyContactCount == contactBefore + 1,
                2f,
                "Binder ready contact was not committed after the transition.");
            Assert.That(driver.Animator.IsInTransition(0), Is.False);
            Assert.That(driver.Animator.GetCurrentAnimatorStateInfo(0).fullPathHash,
                Is.EqualTo(windupHash));
            Assert.That(relay.IsBinderWindupReadyContactCommitted, Is.True);
            Assert.That(relay.PreparedTarget, Is.SameAs(binder));
            AssertBinderAtSocket(body);

            for (int frame = 0; frame < 5; frame++)
            {
                yield return null;
                Assert.That(relay.BinderWindupReadyContactCount,
                    Is.EqualTo(contactBefore + 1),
                    "Binder ready contact must be exactly once per transaction.");
                AssertBinderAtSocket(body);
            }

            yield return WaitUntilOrFail(
                () => relay.IsReadyForCall,
                3f,
                "Prepared Binder did not enter the Ready state.");
            int eventBefore = receiver.AllySupplyReleaseInvocationCount;
            int releaseBefore = relay.ReleaseCount;
            Assert.That(relay.TryRequestReadySupply(
                out PrototypeInteractionTarget selected,
                out PrototypeAllyRelayRejection releaseRejection),
                Is.True,
                releaseRejection.ToString());
            Assert.That(selected, Is.SameAs(binder));
            Assert.That(relay.RelayPhase, Is.EqualTo(PrototypeRelayPhase.PreRelease));

            yield return WaitUntilOrFail(
                () => receiver.AllySupplyReleaseInvocationCount == eventBefore + 1,
                3f,
                "Natural CHR01 Binder release AnimationEvent did not fire.");
            Assert.That(relay.ReleaseCount, Is.EqualTo(releaseBefore + 1));
            Assert.That(relay.RelayPhase, Is.EqualTo(PrototypeRelayPhase.Released));
            for (int frame = 0; frame < 5; frame++)
            {
                yield return null;
                Assert.That(receiver.AllySupplyReleaseInvocationCount,
                    Is.EqualTo(eventBefore + 1));
                Assert.That(relay.ReleaseCount, Is.EqualTo(releaseBefore + 1));
            }

            Assert.That(reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
            yield return null;
            yield return new WaitForFixedUpdate();
            Assert.That(relay.BinderWindupReadyContactCount, Is.Zero);
            Assert.That(relay.IsBinderWindupReadyContactCommitted, Is.False);
            Assert.That(relay.IsAwaitingBinderWindupReadyContact, Is.False);
            Assert.That(relay.BinderWindupReadyTarget, Is.Null);
            Assert.That(relay.BinderWindupReadySequence, Is.Zero);
        }

        [UnityTest]
        public IEnumerator ChairFetchReadyAndFeed_UsesMonotonicPostIkGripWithoutMovingChair()
        {
            yield return LoadScene();
            PrototypeInteractionTarget chair = Target(
                "CHAIR-ALLY-01",
                PrototypeInteractionRole.AllyChair);
            Rigidbody body = chair.GetComponent<Rigidbody>();
            BoxCollider chairCollider = chair.GetComponent<BoxCollider>();
            Assert.That(body, Is.Not.Null);
            Assert.That(chairCollider, Is.Not.Null);
            PositionForOpenChairFetch(chair, body, chairCollider);

            int blendCountBefore = chairGrip.BlendStartCount;
            Assert.That(relay.TryRequestFetch(
                chair,
                out PrototypeAllyRelayRejection fetchRejection),
                Is.True,
                fetchRejection.ToString());

            yield return WaitForChairPhysicalAcquisition(chair, 20f);
            yield return WaitUntilOrFail(
                () => chairGrip.LastAppliedFrame >= 0 &&
                      chairGrip.Phase == ShowcaseChairGripPhase.BlendIn,
                2f,
                () => "Actual v007 CHR01 acquired the Chair but did not publish " +
                    "the BlendIn IK frame. " + DescribeChairState());
            Assert.That(chairGrip.BlendStartCount, Is.EqualTo(blendCountBefore + 1));
            List<float> blendInWeights = new List<float>();
            int observedFrame = -1;
            float previousWeight = -1f;
            float blendInDeadline = Time.realtimeSinceStartup + 2f;
            while (chairGrip.Phase != ShowcaseChairGripPhase.Holding &&
                   Time.realtimeSinceStartup < blendInDeadline)
            {
                yield return null;
                if (chairGrip.LastAppliedFrame < 0 ||
                    chairGrip.LastAppliedFrame == observedFrame)
                {
                    continue;
                }

                observedFrame = chairGrip.LastAppliedFrame;
                Assert.That(chairGrip.CurrentWeight + 0.0001f,
                    Is.GreaterThanOrEqualTo(previousWeight),
                    "Chair grip BlendIn must be monotonic.");
                Assert.That(chairGrip.BlendStartCount,
                    Is.EqualTo(blendCountBefore + 1),
                    "Chair grip BlendIn restarted.");
                previousWeight = chairGrip.CurrentWeight;
                blendInWeights.Add(previousWeight);
            }
            Assert.That(chairGrip.Phase, Is.EqualTo(ShowcaseChairGripPhase.Holding));
            Assert.That(blendInWeights.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(chairGrip.CurrentWeight, Is.EqualTo(1f).Within(0.01f));
            Assert.That(chairGrip.TargetCollider, Is.SameAs(chairCollider));
            yield return TraceChairHoldingFrames(chairCollider, true);
            Assert.That(chairGrip.LeftHandError,
                Is.LessThanOrEqualTo(RawHandResidualTolerance));
            Assert.That(chairGrip.RightHandError,
                Is.LessThanOrEqualTo(RawHandResidualTolerance));
            Assert.That(chairGrip.HasRenderedGripContact, Is.True);
            Assert.That(chairGrip.LeftRenderedGripContactError,
                Is.LessThanOrEqualTo(HandPositionTolerance));
            Assert.That(chairGrip.RightRenderedGripContactError,
                Is.LessThanOrEqualTo(HandPositionTolerance));

            yield return WaitUntilOrFail(
                () => relay.IsReadyForCall,
                3f,
                "Prepared Chair did not enter the Ready state.");
            yield return WaitForStableBodyPose(body, 1f);
            Vector3 heldPosition = body.position;
            Quaternion heldRotation = body.rotation;
            int appliedFrame = chairGrip.LastAppliedFrame;
            for (int sample = 0; sample < 3; sample++)
            {
                int previousAppliedFrame = appliedFrame;
                yield return WaitUntilOrFail(
                    () => chairGrip.LastAppliedFrame != previousAppliedFrame,
                    1f,
                    "Chair grip did not publish a new post-IK diagnostic frame.");
                appliedFrame = chairGrip.LastAppliedFrame;
                Assert.That(Vector3.Distance(body.position, heldPosition),
                    Is.LessThanOrEqualTo(0.001f),
                    "The presentation rig must not move the physical Chair.");
                Assert.That(Quaternion.Angle(body.rotation, heldRotation),
                    Is.LessThanOrEqualTo(0.1f),
                    "The presentation rig must not rotate the physical Chair.");
                Assert.That(chairGrip.LeftHandError,
                    Is.LessThanOrEqualTo(RawHandResidualTolerance));
                Assert.That(chairGrip.RightHandError,
                    Is.LessThanOrEqualTo(RawHandResidualTolerance));
                Assert.That(chairGrip.HasRenderedGripContact, Is.True);
                Assert.That(chairGrip.LeftRenderedGripContactError,
                    Is.LessThanOrEqualTo(HandPositionTolerance));
                Assert.That(chairGrip.RightRenderedGripContactError,
                    Is.LessThanOrEqualTo(HandPositionTolerance));
            }

            AssertActorDoesNotPenetrateChair(chairCollider);
            AssertChairIsGroundSupported(chairCollider);

            Assert.That(relay.TryRequestReadySupply(
                out PrototypeInteractionTarget selected,
                out PrototypeAllyRelayRejection releaseRejection),
                Is.True,
                releaseRejection.ToString());
            Assert.That(selected, Is.SameAs(chair));
            yield return WaitUntilOrFail(
                () => chairGrip.Phase == ShowcaseChairGripPhase.BlendOut,
                3f,
                "Chair feed did not begin a grip BlendOut.");
            Assert.That(chairGrip.BlendStartCount, Is.EqualTo(blendCountBefore + 2));
            Vector3 latchedLeft = chairGrip.LeftGripTarget;
            Vector3 latchedRight = chairGrip.RightGripTarget;
            Assert.That(chairGrip.HasRenderedGripContact, Is.True);
            Vector3 latchedRenderedLeft = chairGrip.LeftRenderedGripContact;
            Vector3 latchedRenderedRight = chairGrip.RightRenderedGripContact;
            float previousBlendOutWeight = chairGrip.CurrentWeight;
            int blendOutSamples = 0;
            observedFrame = chairGrip.LastAppliedFrame;
            float blendOutDeadline = Time.realtimeSinceStartup + 2f;
            while (chairGrip.Phase != ShowcaseChairGripPhase.None &&
                   Time.realtimeSinceStartup < blendOutDeadline)
            {
                yield return null;
                if (chairGrip.LastAppliedFrame < 0 ||
                    chairGrip.LastAppliedFrame == observedFrame)
                {
                    continue;
                }

                observedFrame = chairGrip.LastAppliedFrame;
                Assert.That(chairGrip.CurrentWeight,
                    Is.LessThanOrEqualTo(previousBlendOutWeight + 0.0001f),
                    "Chair grip BlendOut must be monotonic.");
                Assert.That(chairGrip.BlendStartCount,
                    Is.EqualTo(blendCountBefore + 2),
                    "Chair grip BlendOut restarted.");
                Assert.That(Vector3.Distance(chairGrip.LeftGripTarget, latchedLeft),
                    Is.LessThanOrEqualTo(0.001f),
                    "BlendOut must retain its event-frame left-hand target.");
                Assert.That(Vector3.Distance(chairGrip.RightGripTarget, latchedRight),
                    Is.LessThanOrEqualTo(0.001f),
                    "BlendOut must retain its event-frame right-hand target.");
                Assert.That(chairGrip.HasRenderedGripContact, Is.True,
                    "BlendOut must retain the rendered contact latch.");
                Assert.That(Vector3.Distance(
                        chairGrip.LeftRenderedGripContact,
                        latchedRenderedLeft),
                    Is.LessThanOrEqualTo(0.001f),
                    "BlendOut must retain its event-frame left rendered contact.");
                Assert.That(Vector3.Distance(
                        chairGrip.RightRenderedGripContact,
                        latchedRenderedRight),
                    Is.LessThanOrEqualTo(0.001f),
                    "BlendOut must retain its event-frame right rendered contact.");
                previousBlendOutWeight = chairGrip.CurrentWeight;
                blendOutSamples++;
            }
            Assert.That(blendOutSamples, Is.GreaterThanOrEqualTo(2));
            Assert.That(chairGrip.Phase, Is.EqualTo(ShowcaseChairGripPhase.None));
            Assert.That(chairGrip.CurrentWeight, Is.Zero.Within(0.0001f));
            Assert.That(chairGrip.TargetCollider, Is.Null);

            yield return WaitUntilOrFail(
                () => relay.RelayPhase == PrototypeRelayPhase.None &&
                      relay.ActiveTarget == null,
                10f,
                "Actual v007 Chair feed did not finish its physical route.");
        }

        [UnityTest]
        public IEnumerator ChairGrip_ResetAndAnimatorDisableClearHoldingState()
        {
            yield return LoadScene();
            PrototypeInteractionTarget chair = Target(
                "CHAIR-ALLY-01",
                PrototypeInteractionRole.AllyChair);
            Rigidbody body = chair.GetComponent<Rigidbody>();
            BoxCollider chairCollider = chair.GetComponent<BoxCollider>();
            Assert.That(body, Is.Not.Null);
            Assert.That(chairCollider, Is.Not.Null);
            PositionForOpenChairFetch(chair, body, chairCollider);
            Assert.That(relay.TryRequestFetch(
                chair,
                out PrototypeAllyRelayRejection rejection),
                Is.True,
                rejection.ToString());
            yield return WaitForChairPhysicalAcquisition(chair, 20f);
            yield return WaitUntilOrFail(
                () => chairGrip.Phase == ShowcaseChairGripPhase.Holding,
                2f,
                () => "Actual v007 Chair was acquired but the grip did not reach " +
                    "Holding before reset. " + DescribeChairState());
            yield return TraceChairHoldingFrames(chairCollider, false);

            Assert.That(reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
            AssertGripCleared();
            yield return null;
            AssertGripCleared();
            yield return new WaitForFixedUpdate();
            AssertGripCleared();
            Assert.That(relay.PreparedTarget, Is.Null);
            Assert.That(relay.ActiveTarget, Is.Null);

            PositionForOpenChairFetch(chair, body, chairCollider);
            Assert.That(relay.TryRequestFetch(
                chair,
                out PrototypeAllyRelayRejection secondRejection),
                Is.True,
                secondRejection.ToString());
            yield return WaitForChairPhysicalAcquisition(chair, 20f);
            yield return WaitUntilOrFail(
                () => chairGrip.Phase == ShowcaseChairGripPhase.Holding,
                2f,
                () => "Actual v007 Chair was reacquired but the grip did not reach " +
                    "Holding before Animator disable. " + DescribeChairState());

            driver.Animator.enabled = false;
            yield return null;
            AssertGripCleared();
            yield return new WaitForFixedUpdate();
            yield return null;
            AssertGripCleared();
        }

        [TestCase(0f)]
        [TestCase(45f)]
        [TestCase(90f)]
        public void OrientedBoxSupportRadius_IsCorrectAtAuthoredChairYaws(float yaw)
        {
            GameObject source = new GameObject("CHAIR_OBB_SUPPORT_TEST");
            try
            {
                BoxCollider box = source.AddComponent<BoxCollider>();
                box.size = new Vector3(0.729f, 0.998f, 0.697f);
                source.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

                Assert.That(ShowcaseOrientedBoxGeometry.TryCreate(box, out var geometry),
                    Is.True);
                float radians = yaw * Mathf.Deg2Rad;
                float expected = Mathf.Abs(Mathf.Cos(radians)) * box.size.x * 0.5f +
                    Mathf.Abs(Mathf.Sin(radians)) * box.size.z * 0.5f;
                Assert.That(geometry.GetPlanarSupportRadius(Vector3.right),
                    Is.EqualTo(expected).Within(0.0001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(source);
            }
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

            foreach (ShowcaseRunnerAgent runner in SceneComponents<ShowcaseRunnerAgent>())
            {
                runner.enabled = false;
                Rigidbody runnerBody = runner.GetComponent<Rigidbody>();
                if (runnerBody != null)
                {
                    runnerBody.linearVelocity = Vector3.zero;
                    runnerBody.angularVelocity = Vector3.zero;
                }
            }

            relay = SceneComponents<PrototypeAllyRelayController>().Single();
            driver = SceneComponents<ShowcaseCharacterAnimationDriver>()
                .Single(candidate => candidate.Role == ShowcaseCharacterAnimationRole.Ally);
            receiver = driver.Animator.GetComponent<ShowcaseAnimationEventReceiver>();
            binderSocket = driver.GetComponentInParent<ShowcaseBinderGripSocket>();
            chairGrip = driver.Animator.GetComponent<ShowcaseChairGripRig>();
            allyController = relay.GetComponent<CharacterController>();
            reset = SceneComponents<AuthoredEncounterResetController>().Single();

            Assert.That(receiver, Is.Not.Null);
            Assert.That(binderSocket, Is.Not.Null);
            Assert.That(chairGrip, Is.Not.Null);
            Assert.That(chairGrip.IsAvailable, Is.True);
            Assert.That(allyController, Is.Not.Null);
            yield return null;
        }

        private PrototypeInteractionTarget Target(
            string runtimeKey,
            PrototypeInteractionRole role)
        {
            PrototypeInteractionTarget target = SceneComponents<PrototypeInteractionTarget>()
                .Single(candidate =>
                    candidate.RuntimeInstanceKey == runtimeKey &&
                    candidate.Role == role);
            Assert.That(relay.SupplyTargets, Does.Contain(target));
            return target;
        }

        private void AssertBinderAtSocket(Rigidbody body)
        {
            Assert.That(binderSocket.TryGetWorldPose(
                out Vector3 socketPosition,
                out Quaternion socketRotation), Is.True);
            Assert.That(Vector3.Distance(body.position, socketPosition),
                Is.LessThanOrEqualTo(BinderPositionTolerance));
            Assert.That(Quaternion.Angle(body.rotation, socketRotation),
                Is.LessThanOrEqualTo(BinderRotationTolerance));
        }

        private void PositionForOpenChairFetch(
            PrototypeInteractionTarget chair,
            Rigidbody body,
            BoxCollider chairCollider)
        {
            Assert.That(relay.TryEvaluateFetchTarget(
                    chair,
                    out float baselineRouteCost),
                Is.True,
                "The authored far CHAIR-ALLY-01 route preflight must ignore only " +
                "the exact fetch target at its endpoint.");
            Assert.That(float.IsFinite(baselineRouteCost), Is.True);

            bool controllerWasEnabled = allyController.enabled;
            allyController.enabled = false;
            relay.transform.SetPositionAndRotation(
                new Vector3(
                    OpenRouteAllyPlanarPosition.x,
                    relay.transform.position.y,
                    OpenRouteAllyPlanarPosition.z),
                Quaternion.identity);
            allyController.enabled = controllerWasEnabled;

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = new Vector3(
                OpenRouteChairPlanarPosition.x,
                relay.transform.position.y + 2f,
                OpenRouteChairPlanarPosition.z);
            body.rotation = Quaternion.identity;
            chair.transform.SetPositionAndRotation(body.position, body.rotation);
            Physics.SyncTransforms();

            float rootToBottom = chair.transform.position.y - chairCollider.bounds.min.y;
            float supportHeight = FindOpenRouteSupportHeight(chair);
            body.position = new Vector3(
                OpenRouteChairPlanarPosition.x,
                supportHeight + rootToBottom + 0.005f,
                OpenRouteChairPlanarPosition.z);
            chair.transform.SetPositionAndRotation(body.position, body.rotation);
            Physics.SyncTransforms();

            Assert.That(relay.TryEvaluateFetchTarget(chair, out float routeCost),
                Is.True,
                "The isolated v007 CHR01/Chair fixture must retain the real " +
                "unobstructed Z0-to-Z1 fetch route.");
            Assert.That(float.IsFinite(routeCost), Is.True);
            Assert.That(routeCost, Is.GreaterThan(0f),
                "The isolated Chair must require an actual CHR01 fetch movement.");
        }

        private float FindOpenRouteSupportHeight(PrototypeInteractionTarget chair)
        {
            RaycastHit support = Physics.RaycastAll(
                    new Vector3(
                        OpenRouteChairPlanarPosition.x,
                        4f,
                        OpenRouteChairPlanarPosition.z),
                    Vector3.down,
                    8f,
                    ~0,
                    QueryTriggerInteraction.Ignore)
                .Where(hit =>
                    hit.collider != null &&
                    hit.normal.y >= 0.95f &&
                    !hit.collider.transform.IsChildOf(relay.transform) &&
                    hit.collider.transform != relay.transform &&
                    !hit.collider.transform.IsChildOf(chair.transform) &&
                    hit.collider.transform != chair.transform)
                .OrderByDescending(hit => hit.point.y)
                .FirstOrDefault();

            Assert.That(support.collider, Is.Not.Null,
                "The authored Z0-to-Z1 open route must retain physical floor support.");
            Assert.That(support.point.y, Is.EqualTo(0f).Within(0.02f),
                $"Unexpected support '{support.collider.name}' in the open route.");
            return support.point.y;
        }

        private void AssertActorDoesNotPenetrateChair(BoxCollider chairCollider)
        {
            bool overlaps = Physics.ComputePenetration(
                allyController,
                allyController.transform.position,
                allyController.transform.rotation,
                chairCollider,
                chairCollider.transform.position,
                chairCollider.transform.rotation,
                out _,
                out float penetration);
            Assert.That(overlaps ? penetration : 0f,
                Is.LessThanOrEqualTo(PenetrationTolerance),
                "The actual CHR01 CharacterController penetrated the prepared Chair.");
        }

        private void AssertChairIsGroundSupported(BoxCollider chairCollider)
        {
            Assert.That(ShowcaseOrientedBoxGeometry.TryCreate(
                chairCollider,
                out var geometry), Is.True);
            float verticalRadius =
                Mathf.Abs(Vector3.Dot(Vector3.up, geometry.Right)) * geometry.HalfExtents.x +
                Mathf.Abs(Vector3.Dot(Vector3.up, geometry.Up)) * geometry.HalfExtents.y +
                Mathf.Abs(Vector3.Dot(Vector3.up, geometry.Forward)) * geometry.HalfExtents.z;
            float bottom = geometry.Center.y - verticalRadius;
            RaycastHit ground = Physics.RaycastAll(
                    geometry.Center + Vector3.up * 0.05f,
                    Vector3.down,
                    5f,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore)
                .Where(hit =>
                    hit.collider != null &&
                    hit.collider != chairCollider &&
                    hit.collider.attachedRigidbody != chairCollider.attachedRigidbody &&
                    !hit.collider.transform.IsChildOf(relay.transform))
                .OrderBy(hit => hit.distance)
                .FirstOrDefault();
            Assert.That(ground.collider, Is.Not.Null,
                "No physical ground was found below the prepared Chair.");
            Assert.That(Mathf.Abs(bottom - ground.point.y),
                Is.LessThanOrEqualTo(GroundTolerance),
                "The prepared Chair must remain OBB-supported by the floor.");
        }

        private void AssertGripCleared()
        {
            Assert.That(chairGrip.IsActive, Is.False);
            Assert.That(chairGrip.Phase, Is.EqualTo(ShowcaseChairGripPhase.None));
            Assert.That(chairGrip.CurrentWeight, Is.Zero.Within(0.0001f));
            Assert.That(chairGrip.TargetCollider, Is.Null);
            Assert.That(chairGrip.LeftHandError, Is.Zero.Within(0.0001f));
            Assert.That(chairGrip.RightHandError, Is.Zero.Within(0.0001f));
            Assert.That(chairGrip.HasRenderedGripContact, Is.False);
            Assert.That(chairGrip.LeftRenderedGripContact, Is.EqualTo(Vector3.zero));
            Assert.That(chairGrip.RightRenderedGripContact, Is.EqualTo(Vector3.zero));
            Assert.That(chairGrip.LeftRenderedGripContactError,
                Is.Zero.Within(0.0001f));
            Assert.That(chairGrip.RightRenderedGripContactError,
                Is.Zero.Within(0.0001f));
            Assert.That(chairGrip.LastAppliedFrame, Is.EqualTo(-1));
            Assert.That(chairGrip.BlendStartCount, Is.Zero);
        }

        private IEnumerator WaitForChairPhysicalAcquisition(
            PrototypeInteractionTarget expected,
            float timeoutSeconds)
        {
            yield return WaitUntilOrFail(
                () => relay.IsChairGripActive ||
                      relay.AllyState == PrototypeAllySupportState.Blocked,
                timeoutSeconds,
                () => "Actual v007 CHR01 did not complete the physical Chair " +
                    "acquisition. " + DescribeChairState());
            Assert.That(relay.IsChairGripActive, Is.True, DescribeChairState());
            Assert.That(relay.PreparedTarget, Is.SameAs(expected), DescribeChairState());
            Assert.That(chairGrip.IsAvailable, Is.True, DescribeChairState());
            Assert.That(chairGrip.IsActive, Is.True, DescribeChairState());
        }

        private IEnumerator TraceChairHoldingFrames(
            BoxCollider chairCollider,
            bool assertHandTolerance)
        {
            Transform leftHand = driver.Animator.GetBoneTransform(HumanBodyBones.LeftHand);
            Transform rightHand = driver.Animator.GetBoneTransform(HumanBodyBones.RightHand);
            Transform leftUpperArm =
                driver.Animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            Transform leftLowerArm =
                driver.Animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            Transform rightUpperArm =
                driver.Animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            Transform rightLowerArm =
                driver.Animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            Assert.That(leftHand, Is.Not.Null);
            Assert.That(rightHand, Is.Not.Null);
            Assert.That(leftUpperArm, Is.Not.Null);
            Assert.That(leftLowerArm, Is.Not.Null);
            Assert.That(rightUpperArm, Is.Not.Null);
            Assert.That(rightLowerArm, Is.Not.Null);

            yield return WaitUntilOrFail(
                () => chairGrip.Phase == ShowcaseChairGripPhase.Holding &&
                      chairGrip.HasRenderedGripContact,
                1f,
                "The Holding IK pass did not publish its rendered contact latch.");

            int observedFrame = -1;
            var leftErrors = new List<float>();
            var rightErrors = new List<float>();
            var leftRenderedErrors = new List<float>();
            var rightRenderedErrors = new List<float>();
            var localLeftGoals = new List<Vector3>();
            var localRightGoals = new List<Vector3>();
            var localLeftContacts = new List<Vector3>();
            var localRightContacts = new List<Vector3>();
            float deadline = Time.realtimeSinceStartup + 1f;
            while (leftErrors.Count < 3 && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
                if (chairGrip.LastAppliedFrame < 0 ||
                    chairGrip.LastAppliedFrame == observedFrame ||
                    chairGrip.Phase != ShowcaseChairGripPhase.Holding)
                {
                    continue;
                }

                observedFrame = chairGrip.LastAppliedFrame;
                Vector3 leftTarget = chairGrip.LeftGripTarget;
                Vector3 rightTarget = chairGrip.RightGripTarget;
                Assert.That(chairGrip.HasRenderedGripContact, Is.True);
                Vector3 leftContact = chairGrip.LeftRenderedGripContact;
                Vector3 rightContact = chairGrip.RightRenderedGripContact;
                float leftDelta = Vector3.Distance(leftHand.position, leftTarget);
                float rightDelta = Vector3.Distance(rightHand.position, rightTarget);
                float leftRenderedDelta =
                    Vector3.Distance(leftHand.position, leftContact);
                float rightRenderedDelta =
                    Vector3.Distance(rightHand.position, rightContact);
                float leftUpperSegment =
                    Vector3.Distance(leftUpperArm.position, leftLowerArm.position);
                float leftLowerSegment =
                    Vector3.Distance(leftLowerArm.position, leftHand.position);
                float leftChainLength = leftUpperSegment + leftLowerSegment;
                float leftTargetDistance =
                    Vector3.Distance(leftUpperArm.position, leftTarget);
                float leftActualReach =
                    Vector3.Distance(leftUpperArm.position, leftHand.position);
                float rightUpperSegment =
                    Vector3.Distance(rightUpperArm.position, rightLowerArm.position);
                float rightLowerSegment =
                    Vector3.Distance(rightLowerArm.position, rightHand.position);
                float rightChainLength = rightUpperSegment + rightLowerSegment;
                float rightTargetDistance =
                    Vector3.Distance(rightUpperArm.position, rightTarget);
                float rightActualReach =
                    Vector3.Distance(rightUpperArm.position, rightHand.position);
                leftErrors.Add(leftDelta);
                rightErrors.Add(rightDelta);
                leftRenderedErrors.Add(leftRenderedDelta);
                rightRenderedErrors.Add(rightRenderedDelta);
                localLeftGoals.Add(chairCollider.transform.InverseTransformPoint(leftTarget));
                localRightGoals.Add(chairCollider.transform.InverseTransformPoint(rightTarget));
                localLeftContacts.Add(
                    chairCollider.transform.InverseTransformPoint(leftContact));
                localRightContacts.Add(
                    chairCollider.transform.InverseTransformPoint(rightContact));

                Assert.That(ShowcaseOrientedBoxGeometry.TryCreate(
                    chairCollider,
                    out var geometry), Is.True);
                Debug.Log(
                    $"CHR01 CHAIR HOLDING TRACE {leftErrors.Count}/3: " +
                    $"Frame={observedFrame}, Phase={chairGrip.Phase}, " +
                    $"Weight={chairGrip.CurrentWeight:F6}, " +
                    $"LeftTarget={leftTarget.ToString("F6")}, " +
                    $"LeftHand={leftHand.position.ToString("F6")}, " +
                    $"LeftDelta={leftDelta:F9}, LeftDiagnostic={chairGrip.LeftHandError:F9}, " +
                    $"RightTarget={rightTarget.ToString("F6")}, " +
                    $"RightHand={rightHand.position.ToString("F6")}, " +
                    $"RightDelta={rightDelta:F9}, RightDiagnostic={chairGrip.RightHandError:F9}, " +
                    $"LeftRenderedContact={leftContact.ToString("F6")}, " +
                    $"LeftRenderedDelta={leftRenderedDelta:F9}, " +
                    $"RightRenderedContact={rightContact.ToString("F6")}, " +
                    $"RightRenderedDelta={rightRenderedDelta:F9}, " +
                    $"ChairPosition={chairCollider.transform.position.ToString("F6")}, " +
                    $"ChairRotation={chairCollider.transform.rotation.eulerAngles.ToString("F6")}, " +
                    $"ObbCenter={geometry.Center.ToString("F6")}, " +
                    $"AllyLocalChair={relay.transform.InverseTransformPoint(chairCollider.transform.position).ToString("F6")}, " +
                    $"AllyLocalLeftTarget={relay.transform.InverseTransformPoint(leftTarget).ToString("F6")}, " +
                    $"AllyLocalRightTarget={relay.transform.InverseTransformPoint(rightTarget).ToString("F6")}, " +
                    $"LeftUpperArm={leftUpperArm.position.ToString("F6")}, " +
                    $"LeftLowerArm={leftLowerArm.position.ToString("F6")}, " +
                    $"LeftSegments={leftUpperSegment:F9}+{leftLowerSegment:F9}={leftChainLength:F9}, " +
                    $"LeftUpperToTarget={leftTargetDistance:F9}, " +
                    $"LeftTargetExcess={leftTargetDistance - leftChainLength:F9}, " +
                    $"LeftActualReach={leftActualReach:F9}, " +
                    $"RightUpperArm={rightUpperArm.position.ToString("F6")}, " +
                    $"RightLowerArm={rightLowerArm.position.ToString("F6")}, " +
                    $"RightSegments={rightUpperSegment:F9}+{rightLowerSegment:F9}={rightChainLength:F9}, " +
                    $"RightUpperToTarget={rightTargetDistance:F9}, " +
                    $"RightTargetExcess={rightTargetDistance - rightChainLength:F9}, " +
                    $"RightActualReach={rightActualReach:F9}." );
            }

            Assert.That(leftErrors.Count, Is.EqualTo(3),
                "Three distinct post-IK Holding frames were not published.");
            if (!assertHandTolerance)
            {
                yield break;
            }

            for (int index = 0; index < leftErrors.Count; index++)
            {
                Assert.That(leftErrors[index],
                    Is.LessThanOrEqualTo(RawHandResidualTolerance),
                    $"Left raw IK residual exceeded its diagnostic envelope on " +
                    $"Holding sample {index + 1}.");
                Assert.That(rightErrors[index],
                    Is.LessThanOrEqualTo(RawHandResidualTolerance),
                    $"Right raw IK residual exceeded its diagnostic envelope on " +
                    $"Holding sample {index + 1}.");
                Assert.That(leftRenderedErrors[index],
                    Is.LessThanOrEqualTo(HandPositionTolerance),
                    $"Left rendered contact drifted on Holding sample {index + 1}.");
                Assert.That(rightRenderedErrors[index],
                    Is.LessThanOrEqualTo(HandPositionTolerance),
                    $"Right rendered contact drifted on Holding sample {index + 1}.");
                if (index == 0)
                {
                    continue;
                }

                Assert.That(Vector3.Distance(
                        localLeftGoals[index - 1],
                        localLeftGoals[index]),
                    Is.LessThanOrEqualTo(GripStabilityTolerance),
                    "The analytic left IK goal fed back from rendered contact.");
                Assert.That(Vector3.Distance(
                        localRightGoals[index - 1],
                        localRightGoals[index]),
                    Is.LessThanOrEqualTo(GripStabilityTolerance),
                    "The analytic right IK goal fed back from rendered contact.");
                Assert.That(Vector3.Distance(
                        localLeftContacts[index - 1],
                        localLeftContacts[index]),
                    Is.LessThanOrEqualTo(GripStabilityTolerance),
                    "The rendered left contact was not Chair-local stable.");
                Assert.That(Vector3.Distance(
                        localRightContacts[index - 1],
                        localRightContacts[index]),
                    Is.LessThanOrEqualTo(GripStabilityTolerance),
                    "The rendered right contact was not Chair-local stable.");
            }
        }

        private string DescribeChairState()
        {
            string prepared = relay.PreparedTarget != null
                ? relay.PreparedTarget.RuntimeInstanceKey
                : "null";
            string active = relay.ActiveTarget != null
                ? relay.ActiveTarget.RuntimeInstanceKey
                : "null";
            string rigTarget = chairGrip.TargetCollider != null
                ? chairGrip.TargetCollider.name
                : "null";
            return $"AllyState={relay.AllyState}, RelayPhase={relay.RelayPhase}, " +
                $"Prepared={prepared}, Active={active}, " +
                $"RigAvailable={chairGrip.IsAvailable}, RigActive={chairGrip.IsActive}, " +
                $"RigPhase={chairGrip.Phase}, RigTarget={rigTarget}, " +
                $"LastAppliedFrame={chairGrip.LastAppliedFrame}, " +
                $"AnimatorEnabled={driver.Animator.enabled}, " +
                $"AnimatorInitialized={driver.Animator.isInitialized}, " +
                $"AnimatorCulling={driver.Animator.cullingMode}.";
        }

        private static IEnumerator WaitForStableBodyPose(Rigidbody body, float timeoutSeconds)
        {
            Vector3 previousPosition = body.position;
            Quaternion previousRotation = body.rotation;
            int stableFrames = 0;
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (stableFrames < 4 && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
                bool stable = Vector3.Distance(body.position, previousPosition) <= 0.0001f &&
                    Quaternion.Angle(body.rotation, previousRotation) <= 0.01f;
                stableFrames = stable ? stableFrames + 1 : 0;
                previousPosition = body.position;
                previousRotation = body.rotation;
            }
            Assert.That(stableFrames, Is.GreaterThanOrEqualTo(4),
                "Prepared Chair pose did not settle before presentation-only verification.");
        }

        private static IEnumerator WaitUntilOrFail(
            Func<bool> predicate,
            float timeoutSeconds,
            string message)
        {
            return WaitUntilOrFail(predicate, timeoutSeconds, () => message);
        }

        private static IEnumerator WaitUntilOrFail(
            Func<bool> predicate,
            float timeoutSeconds,
            Func<string> message)
        {
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!predicate() && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }
            Assert.That(predicate(), Is.True, message());
        }

        private T[] SceneComponents<T>() where T : Component
        {
            return loadedScene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }
    }
}
