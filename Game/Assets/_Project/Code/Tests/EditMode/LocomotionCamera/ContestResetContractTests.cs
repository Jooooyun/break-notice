using System.Collections.Generic;
using System.IO;
using BreakNotice.Gameplay.Encounter;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestResetContractTests
    {
        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject item in createdObjects)
            {
                Object.DestroyImmediate(item);
            }

            createdObjects.Clear();
        }

        [Test]
        public void InteractionOwner_ResetRestoresEveryOwnershipDomain()
        {
            PrototypeInteractionStateOwner owner = CreateOwner();
            PrototypeInteractionTarget binder = CreateTarget(
                "BINDER-DIRECT-01",
                PrototypeWeightClass.Light,
                PrototypeInteractionRole.DirectBinder,
                true,
                false,
                false);
            var arbitrator = new PrototypeInteractionArbitrator();
            Assert.That(owner.TryCommit(
                arbitrator.Decide(owner.CreateContext(false, binder))), Is.True);

            Assert.That(owner.TryBeginReset(), Is.True);
            Assert.That(owner.RestoreInteractionBaseline(), Is.True);

            Assert.That(owner.GlobalState, Is.EqualTo(PrototypeGlobalInteractionState.Playable));
            Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
            Assert.That(owner.StrapState, Is.EqualTo(PrototypeStrapOwnershipState.Stored));
            Assert.That(owner.RelayState, Is.EqualTo(PrototypeRelayReservationState.None));
            Assert.That(owner.CarriedTarget, Is.Null);
            Assert.That(owner.ThrowCommittedTarget, Is.Null);
            Assert.That(owner.StrapTarget, Is.Null);
            Assert.That(owner.RelayReservedTarget, Is.Null);
            Assert.That(owner.QueuedThrowArmed, Is.False);
        }

        [Test]
        public void FailurePending_UsesSameResetBarrierWithoutAutomaticStrapRelease()
        {
            PrototypeInteractionStateOwner owner = CreateOwner();
            Assert.That(owner.TryBeginFailurePending(), Is.True);
            Assert.That(owner.TryBeginReset(), Is.True);
            Assert.That(owner.GlobalState, Is.EqualTo(PrototypeGlobalInteractionState.ResetPending));
            Assert.That(owner.RestoreInteractionBaseline(), Is.True);
            Assert.That(owner.StrapState, Is.EqualTo(PrototypeStrapOwnershipState.Stored));
        }

        [Test]
        public void ResetImplementation_UsesAuthoredSnapshotWithoutSceneReloadOrObjectCreation()
        {
            string source = File.ReadAllText(
                "Assets/_Project/Code/Runtime/Player/AuthoredEncounterResetController.cs");

            StringAssert.Contains("CaptureBaseline", source);
            StringAssert.Contains("RequestReset", source);
            StringAssert.DoesNotContain("SceneManager.LoadScene", source);
            StringAssert.DoesNotContain("Instantiate(", source);
            StringAssert.DoesNotContain("Destroy(", source);
        }

        [Test]
        public void ManualAndAutomaticResetCausesRemainDistinct()
        {
            Assert.That(
                EncounterResetCause.ManualInput,
                Is.Not.EqualTo(EncounterResetCause.AutomaticFailureRestart));
            Assert.That(System.Enum.GetValues(typeof(EncounterResetCause)).Length, Is.EqualTo(2));
        }

        private PrototypeInteractionStateOwner CreateOwner()
        {
            GameObject item = new GameObject("Owner");
            createdObjects.Add(item);
            return item.AddComponent<PrototypeInteractionStateOwner>();
        }

        private PrototypeInteractionTarget CreateTarget(
            string key,
            PrototypeWeightClass weightClass,
            PrototypeInteractionRole role,
            bool direct,
            bool strap,
            bool anchor)
        {
            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            createdObjects.Add(item);
            PrototypeInteractionTarget target = item.AddComponent<PrototypeInteractionTarget>();
            target.Configure(
                "TEST",
                key,
                role.ToString(),
                weightClass,
                role,
                direct,
                false,
                strap,
                anchor);
            return target;
        }
    }
}
