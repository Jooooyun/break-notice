using System.Collections;
using System.Linq;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestInteractionPlayModeTests
    {
        [UnityTest]
        public IEnumerator InteractPrimary_CommitsOneCarryOwnerWithoutTeleportingTarget()
        {
            GameObject player = new GameObject("Player");
            GameObject targetObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                player.transform.position = Vector3.zero;
                player.transform.rotation = Quaternion.identity;
                targetObject.transform.position = new Vector3(0f, 1f, 1.5f);

                PrototypeInteractionStateOwner owner =
                    player.AddComponent<PrototypeInteractionStateOwner>();
                PrototypeInteractionController controller =
                    player.AddComponent<PrototypeInteractionController>();
                PrototypeInteractionTarget target =
                    targetObject.AddComponent<PrototypeInteractionTarget>();
                target.Configure(
                    "OBJ-P-L-001",
                    "BINDER-DIRECT-TEST",
                    "FILE BINDER",
                    PrototypeWeightClass.Light,
                    PrototypeInteractionRole.DirectBinder,
                    true,
                    false,
                    false,
                    false);
                controller.Configure(null, owner, null, new[] { target }, 2.8f, false);
                InteractionReadabilityPresenter presenter =
                    player.AddComponent<InteractionReadabilityPresenter>();
                presenter.Configure(controller);

                Vector3 before = target.transform.position;
                Physics.SyncTransforms();
                Assert.That(controller.RefreshTarget(false), Is.SameAs(target));
                Assert.That(presenter.BuildReticleText(), Is.EqualTo("[E]"));
                Assert.That(presenter.BuildStatusText(), Does.Contain("[E] PICK UP"));
                PrototypeInteractionDecision decision = controller.ProcessInteractPrimary(false);

                Assert.That(
                    decision.Outcome,
                    Is.EqualTo(PrototypeInteractionOutcome.Scoop),
                    $"rejection={decision.Rejection}; current={controller.CurrentTarget?.RuntimeInstanceKey ?? "<none>"}; " +
                    $"eligible={target.IsScoopEligible}; range={controller.InteractionRange}");
                Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Carrying));
                Assert.That(owner.CarriedTarget, Is.SameAs(target));
                Assert.That(controller.InteractionAttemptSequence, Is.EqualTo(1));
                Assert.That(
                    presenter.BuildLatestAttemptFeedbackText(),
                    Is.EqualTo("PICKUP CONFIRMED: FILE BINDER"));
                Assert.That(target.transform.position, Is.EqualTo(before));
                yield return null;
                Assert.That(target.transform.position, Is.EqualTo(before));
            }
            finally
            {
                Object.Destroy(player);
                Object.Destroy(targetObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator Targeting_NonAimUsesCameraCenterBeforeCloserOffAxisObject()
        {
            GameObject player = new GameObject("Player");
            GameObject view = new GameObject("View");
            GameObject centeredObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject closerOffAxisObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                player.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                player.AddComponent<CharacterController>();
                view.transform.position = new Vector3(0f, 1f, -2f);
                view.transform.rotation = Quaternion.identity;
                centeredObject.transform.position = new Vector3(0f, 1f, 2f);
                closerOffAxisObject.transform.position = new Vector3(0.7f, 1f, 0.9f);

                PrototypeInteractionStateOwner owner =
                    player.AddComponent<PrototypeInteractionStateOwner>();
                PrototypeInteractionController controller =
                    player.AddComponent<PrototypeInteractionController>();
                PrototypeInteractionTarget centered = CreateTarget(
                    centeredObject,
                    "BINDER-CENTERED",
                    "CENTERED BINDER",
                    PrototypeWeightClass.Light,
                    PrototypeInteractionRole.DirectBinder,
                    true);
                PrototypeInteractionTarget closerOffAxis = CreateTarget(
                    closerOffAxisObject,
                    "BINDER-CLOSER",
                    "CLOSER BINDER",
                    PrototypeWeightClass.Light,
                    PrototypeInteractionRole.DirectBinder,
                    true);
                controller.Configure(
                    null,
                    owner,
                    view.transform,
                    new[] { closerOffAxis, centered },
                    2.8f,
                    false);

                Physics.SyncTransforms();
                Assert.That(controller.RefreshTarget(false), Is.SameAs(centered));
                Assert.That(controller.CurrentTargetDistance, Is.LessThan(2.8f));
                controller.Configure(
                    null,
                    owner,
                    view.transform,
                    new[] { closerOffAxis },
                    2.8f,
                    false);
                Assert.That(
                    controller.RefreshTarget(false),
                    Is.Null,
                    "An off-reticle object must not inherit the old wide-cone selection behavior.");
            }
            finally
            {
                Object.Destroy(player);
                Object.Destroy(view);
                Object.Destroy(centeredObject);
                Object.Destroy(closerOffAxisObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator Targeting_WorldOcclusionRejectsWithoutOwnershipMutation()
        {
            GameObject player = new GameObject("Player");
            GameObject view = new GameObject("View");
            GameObject targetObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                view.transform.position = new Vector3(0f, 1f, 0f);
                view.transform.rotation = Quaternion.identity;
                targetObject.transform.position = new Vector3(0f, 1f, 2f);
                wall.transform.position = new Vector3(0f, 1f, 1f);
                wall.transform.localScale = new Vector3(1.5f, 2f, 0.2f);

                PrototypeInteractionStateOwner owner =
                    player.AddComponent<PrototypeInteractionStateOwner>();
                PrototypeInteractionController controller =
                    player.AddComponent<PrototypeInteractionController>();
                PrototypeInteractionTarget target = CreateTarget(
                    targetObject,
                    "BINDER-OCCLUDED",
                    "OCCLUDED BINDER",
                    PrototypeWeightClass.Light,
                    PrototypeInteractionRole.DirectBinder,
                    true);
                controller.Configure(
                    null,
                    owner,
                    view.transform,
                    new[] { target },
                    2.8f,
                    false);

                Physics.SyncTransforms();
                Assert.That(controller.RefreshTarget(false), Is.Null);
                PrototypeInteractionDecision decision = controller.ProcessInteractPrimary(false);
                Assert.That(decision.Rejection, Is.EqualTo(PrototypeInteractionRejection.NoValidTarget));
                Assert.That(controller.InteractionAttemptSequence, Is.EqualTo(1));
                Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
                Assert.That(owner.CarriedTarget, Is.Null);
            }
            finally
            {
                Object.Destroy(player);
                Object.Destroy(view);
                Object.Destroy(targetObject);
                Object.Destroy(wall);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator Targeting_EqualGeometryUsesRuntimeKeyIndependentOfAuthoredOrder()
        {
            GameObject player = new GameObject("Player");
            GameObject view = new GameObject("View");
            GameObject targetAObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject targetBObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                view.transform.position = new Vector3(0f, 1f, 0f);
                view.transform.rotation = Quaternion.identity;
                targetAObject.transform.position = new Vector3(0f, 1f, 1.5f);
                targetBObject.transform.position = targetAObject.transform.position;

                PrototypeInteractionStateOwner owner =
                    player.AddComponent<PrototypeInteractionStateOwner>();
                PrototypeInteractionController controller =
                    player.AddComponent<PrototypeInteractionController>();
                PrototypeInteractionTarget targetA = CreateTarget(
                    targetAObject,
                    "A-STABLE-KEY",
                    "BINDER A",
                    PrototypeWeightClass.Light,
                    PrototypeInteractionRole.DirectBinder,
                    true);
                PrototypeInteractionTarget targetB = CreateTarget(
                    targetBObject,
                    "B-STABLE-KEY",
                    "BINDER B",
                    PrototypeWeightClass.Light,
                    PrototypeInteractionRole.DirectBinder,
                    true);
                controller.Configure(
                    null,
                    owner,
                    view.transform,
                    new[] { targetB, targetA },
                    2.8f,
                    false);

                Physics.SyncTransforms();
                Assert.That(controller.RefreshTarget(false), Is.SameAs(targetA));
            }
            finally
            {
                Object.Destroy(player);
                Object.Destroy(view);
                Object.Destroy(targetAObject);
                Object.Destroy(targetBObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator Targeting_UsesColliderClosestPointForRangeAndReportsDistance()
        {
            GameObject player = new GameObject("Player");
            GameObject view = new GameObject("View");
            GameObject targetObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                view.transform.position = new Vector3(0f, 1f, 0f);
                view.transform.rotation = Quaternion.identity;
                targetObject.transform.position = new Vector3(0f, 1f, 3.25f);

                PrototypeInteractionStateOwner owner =
                    player.AddComponent<PrototypeInteractionStateOwner>();
                PrototypeInteractionController controller =
                    player.AddComponent<PrototypeInteractionController>();
                PrototypeInteractionTarget target = CreateTarget(
                    targetObject,
                    "BINDER-RANGE-EDGE",
                    "EDGE BINDER",
                    PrototypeWeightClass.Light,
                    PrototypeInteractionRole.DirectBinder,
                    true);
                controller.Configure(
                    null,
                    owner,
                    view.transform,
                    new[] { target },
                    2.8f,
                    false);

                Physics.SyncTransforms();
                Assert.That(controller.RefreshTarget(false), Is.SameAs(target));
                Assert.That(controller.CurrentTargetDistance, Is.EqualTo(2.75f).Within(0.01f));
            }
            finally
            {
                Object.Destroy(player);
                Object.Destroy(view);
                Object.Destroy(targetObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator Readability_HeavyTargetShowsBlockedReticleAndExactReason()
        {
            GameObject player = new GameObject("Player");
            GameObject view = new GameObject("View");
            GameObject targetObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                view.transform.position = new Vector3(0f, 1f, 0f);
                view.transform.rotation = Quaternion.identity;
                targetObject.transform.position = new Vector3(0f, 1f, 1.5f);

                PrototypeInteractionStateOwner owner =
                    player.AddComponent<PrototypeInteractionStateOwner>();
                PrototypeInteractionController controller =
                    player.AddComponent<PrototypeInteractionController>();
                InteractionReadabilityPresenter presenter =
                    player.AddComponent<InteractionReadabilityPresenter>();
                PrototypeInteractionTarget target = CreateTarget(
                    targetObject,
                    "COPIER-READABILITY",
                    "COPIER",
                    PrototypeWeightClass.Heavy,
                    PrototypeInteractionRole.Copier,
                    false);
                controller.Configure(
                    null,
                    owner,
                    view.transform,
                    new[] { target },
                    2.8f,
                    false);
                presenter.Configure(controller);

                Physics.SyncTransforms();
                Assert.That(controller.RefreshTarget(false), Is.SameAs(target));
                Assert.That(presenter.BuildReticleText(), Is.EqualTo("[!]"));
                Assert.That(presenter.BuildReticleLabel(), Is.EqualTo("HEAVY - RECOGNITION ONLY"));
                Assert.That(presenter.BuildStatusText(), Does.Contain("RECOGNITION ONLY"));
                Assert.That(presenter.BuildStatusText(), Does.Not.Contain("[E] PICK UP"));
            }
            finally
            {
                Object.Destroy(player);
                Object.Destroy(view);
                Object.Destroy(targetObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ContestScene_HasOneOwnerExactTargetClassesAndOneWayPresenter()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "Contest_SingleRoomGraybox",
                LoadSceneMode.Additive);
            yield return load;

            Scene contestScene = SceneManager.GetSceneByName("Contest_SingleRoomGraybox");
            Assert.That(contestScene.IsValid() && contestScene.isLoaded, Is.True);

            PrototypeInteractionStateOwner[] owners =
                Object.FindObjectsByType<PrototypeInteractionStateOwner>(FindObjectsSortMode.None);
            PrototypeInteractionTarget[] targets =
                Object.FindObjectsByType<PrototypeInteractionTarget>(FindObjectsSortMode.None);
            InteractionReadabilityPresenter[] presenters =
                Object.FindObjectsByType<InteractionReadabilityPresenter>(FindObjectsSortMode.None);
            PrototypeInteractionController[] controllers =
                Object.FindObjectsByType<PrototypeInteractionController>(FindObjectsSortMode.None);

            Assert.That(owners, Has.Length.EqualTo(1));
            Assert.That(targets, Has.Length.EqualTo(7));
            Assert.That(presenters, Has.Length.EqualTo(1));
            Assert.That(controllers, Has.Length.EqualTo(1));
            Assert.That(targets.Count(target => target.WeightClass == PrototypeWeightClass.Light), Is.EqualTo(3));
            Assert.That(targets.Count(target => target.WeightClass == PrototypeWeightClass.Medium), Is.EqualTo(3));
            Assert.That(targets.Count(target => target.WeightClass == PrototypeWeightClass.Heavy), Is.EqualTo(1));
            Assert.That(targets.Single(target => target.Role == PrototypeInteractionRole.Drawer).DirectAcquisitionEligible, Is.False);
            Assert.That(targets.Single(target => target.Role == PrototypeInteractionRole.Copier).DirectAcquisitionEligible, Is.False);
            Assert.That(owners[0].GlobalState, Is.EqualTo(PrototypeGlobalInteractionState.Playable));
            Assert.That(presenters[0].BuildStatusText(), Is.Not.Null);

            AsyncOperation unload = SceneManager.UnloadSceneAsync(contestScene);
            yield return unload;
        }

        private static PrototypeInteractionTarget CreateTarget(
            GameObject targetObject,
            string instanceKey,
            string displayName,
            PrototypeWeightClass weightClass,
            PrototypeInteractionRole role,
            bool directAcquisitionEligible)
        {
            PrototypeInteractionTarget target =
                targetObject.AddComponent<PrototypeInteractionTarget>();
            target.Configure(
                "TEST-DEFINITION",
                instanceKey,
                displayName,
                weightClass,
                role,
                directAcquisitionEligible,
                false,
                false,
                false);
            return target;
        }
    }
}
