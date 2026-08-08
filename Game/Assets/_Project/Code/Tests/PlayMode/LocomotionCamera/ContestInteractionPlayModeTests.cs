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
                controller.Configure(null, owner, null, new[] { target }, 2.8f, 0.25f, false);

                Vector3 before = target.transform.position;
                PrototypeInteractionDecision decision = controller.ProcessInteractPrimary(false);

                Assert.That(
                    decision.Outcome,
                    Is.EqualTo(PrototypeInteractionOutcome.Scoop),
                    $"rejection={decision.Rejection}; current={controller.CurrentTarget?.RuntimeInstanceKey ?? "<none>"}; " +
                    $"eligible={target.IsScoopEligible}; range={controller.InteractionRange}");
                Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Carrying));
                Assert.That(owner.CarriedTarget, Is.SameAs(target));
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

            Assert.That(owners, Has.Length.EqualTo(1));
            Assert.That(targets, Has.Length.EqualTo(7));
            Assert.That(presenters, Has.Length.EqualTo(1));
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
    }
}
