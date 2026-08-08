using System.Collections.Generic;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestInteractionArbitrationTests
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
        public void NoTarget_IsSafeRejectionWithoutOwnershipMutation()
        {
            PrototypeInteractionStateOwner owner = CreateOwner();
            var arbitrator = new PrototypeInteractionArbitrator();

            PrototypeInteractionDecision decision = arbitrator.Decide(
                owner.CreateContext(false, null));

            Assert.That(decision.IsAccepted, Is.False);
            Assert.That(decision.Rejection, Is.EqualTo(PrototypeInteractionRejection.NoValidTarget));
            Assert.That(owner.TryCommit(decision), Is.False);
            AssertBaseline(owner);
        }

        [Test]
        public void InvalidExplicitStrapContext_DoesNotFallBackToScoop()
        {
            PrototypeInteractionTarget binder = CreateTarget(
                "BINDER-DIRECT-01",
                PrototypeWeightClass.Light,
                PrototypeInteractionRole.DirectBinder,
                true,
                false,
                false,
                false);
            PrototypeInteractionStateOwner owner = CreateOwner();
            var arbitrator = new PrototypeInteractionArbitrator();

            PrototypeInteractionDecision decision = arbitrator.Decide(
                owner.CreateContext(true, binder));

            Assert.That(decision.IsAccepted, Is.False);
            Assert.That(
                decision.Rejection,
                Is.EqualTo(PrototypeInteractionRejection.InvalidExplicitStrapContext));
            AssertBaseline(owner);
        }

        [Test]
        public void ActiveRelay_UsesStrapOrCatchOnlyAndNeverUnrelatedScoop()
        {
            PrototypeInteractionTarget incomingBinder = CreateTarget(
                "BINDER-ALLY-01",
                PrototypeWeightClass.Light,
                PrototypeInteractionRole.AllyBinder,
                false,
                true,
                false,
                false);
            PrototypeInteractionTarget directBinder = CreateTarget(
                "BINDER-DIRECT-01",
                PrototypeWeightClass.Light,
                PrototypeInteractionRole.DirectBinder,
                true,
                false,
                false,
                false);
            PrototypeInteractionStateOwner owner = CreateOwner();
            var arbitrator = new PrototypeInteractionArbitrator();
            Assert.That(owner.TryReserveRelay(incomingBinder), Is.True);

            PrototypeInteractionDecision catchDecision = arbitrator.Decide(
                owner.CreateContext(false, incomingBinder));
            PrototypeInteractionDecision unrelatedDecision = arbitrator.Decide(
                owner.CreateContext(false, directBinder));

            Assert.That(catchDecision.Outcome, Is.EqualTo(PrototypeInteractionOutcome.Catch));
            Assert.That(unrelatedDecision.IsAccepted, Is.False);
            Assert.That(
                unrelatedDecision.Rejection,
                Is.EqualTo(PrototypeInteractionRejection.InvalidRelayContext));
        }

        [Test]
        public void PrototypeEligibility_DistinguishesBinderChairDrawerAndCopier()
        {
            PrototypeInteractionTarget binder = CreateTarget(
                "BINDER-DIRECT-01", PrototypeWeightClass.Light,
                PrototypeInteractionRole.DirectBinder, true, false, false, false);
            PrototypeInteractionTarget chair = CreateTarget(
                "CHAIR-DIRECT-01", PrototypeWeightClass.Medium,
                PrototypeInteractionRole.DirectChair, true, false, true, true);
            PrototypeInteractionTarget drawer = CreateTarget(
                "DRAWER-01", PrototypeWeightClass.Medium,
                PrototypeInteractionRole.Drawer, false, false, false, false);
            PrototypeInteractionTarget copier = CreateTarget(
                "COPIER-01", PrototypeWeightClass.Heavy,
                PrototypeInteractionRole.Copier, false, false, false, false);
            PrototypeInteractionStateOwner owner = CreateOwner();
            var arbitrator = new PrototypeInteractionArbitrator();

            Assert.That(arbitrator.Decide(owner.CreateContext(false, binder)).Outcome,
                Is.EqualTo(PrototypeInteractionOutcome.Scoop));
            Assert.That(arbitrator.Decide(owner.CreateContext(false, chair)).Outcome,
                Is.EqualTo(PrototypeInteractionOutcome.Scoop));
            Assert.That(arbitrator.Decide(owner.CreateContext(false, drawer)).Rejection,
                Is.EqualTo(PrototypeInteractionRejection.DirectAcquisitionProhibited));
            Assert.That(arbitrator.Decide(owner.CreateContext(false, copier)).Rejection,
                Is.EqualTo(PrototypeInteractionRejection.HeavyRecognitionOnly));
        }

        [Test]
        public void Owner_EnforcesSingularCarryStrapAndRelayOwnership()
        {
            PrototypeInteractionTarget binder = CreateTarget(
                "BINDER-DIRECT-01", PrototypeWeightClass.Light,
                PrototypeInteractionRole.DirectBinder, true, false, false, false);
            PrototypeInteractionTarget secondBinder = CreateTarget(
                "BINDER-DIRECT-02", PrototypeWeightClass.Light,
                PrototypeInteractionRole.DirectBinder, true, false, false, false);
            PrototypeInteractionTarget chair = CreateTarget(
                "CHAIR-ALLY-01", PrototypeWeightClass.Medium,
                PrototypeInteractionRole.AllyChair, false, false, true, true);
            PrototypeInteractionStateOwner owner = CreateOwner();
            var arbitrator = new PrototypeInteractionArbitrator();

            PrototypeInteractionDecision scoop = arbitrator.Decide(
                owner.CreateContext(false, binder));
            Assert.That(owner.TryCommit(scoop), Is.True);
            Assert.That(owner.TryCommit(PrototypeInteractionDecision.Accept(
                PrototypeInteractionOutcome.Scoop, secondBinder)), Is.False);
            Assert.That(owner.TryReserveRelay(chair), Is.False);
            Assert.That(owner.CarriedTarget, Is.SameAs(binder));
            Assert.That(owner.StrapTarget, Is.Null);
            Assert.That(owner.RelayReservedTarget, Is.Null);
        }

        [Test]
        public void FailureAndResetPending_BlockEveryNewInteraction()
        {
            PrototypeInteractionTarget binder = CreateTarget(
                "BINDER-DIRECT-01", PrototypeWeightClass.Light,
                PrototypeInteractionRole.DirectBinder, true, false, false, false);
            var arbitrator = new PrototypeInteractionArbitrator();

            PrototypeInteractionStateOwner failureOwner = CreateOwner();
            Assert.That(failureOwner.TryBeginFailurePending(), Is.True);
            PrototypeInteractionDecision failureDecision = arbitrator.Decide(
                failureOwner.CreateContext(false, binder));
            Assert.That(failureDecision.Rejection,
                Is.EqualTo(PrototypeInteractionRejection.GlobalStateBlocked));

            PrototypeInteractionStateOwner resetOwner = CreateOwner();
            Assert.That(resetOwner.TryBeginReset(), Is.True);
            PrototypeInteractionDecision resetDecision = arbitrator.Decide(
                resetOwner.CreateContext(false, binder));
            Assert.That(resetDecision.Rejection,
                Is.EqualTo(PrototypeInteractionRejection.GlobalStateBlocked));
            Assert.That(resetOwner.TryReserveRelay(binder), Is.False);
        }

        private PrototypeInteractionStateOwner CreateOwner()
        {
            GameObject item = new GameObject("InteractionOwner");
            createdObjects.Add(item);
            return item.AddComponent<PrototypeInteractionStateOwner>();
        }

        private PrototypeInteractionTarget CreateTarget(
            string instanceKey,
            PrototypeWeightClass weightClass,
            PrototypeInteractionRole role,
            bool direct,
            bool catchEligible,
            bool strapEligible,
            bool validAnchor)
        {
            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            createdObjects.Add(item);
            PrototypeInteractionTarget target = item.AddComponent<PrototypeInteractionTarget>();
            target.Configure(
                "TEST-DEFINITION",
                instanceKey,
                role.ToString(),
                weightClass,
                role,
                direct,
                catchEligible,
                strapEligible,
                validAnchor);
            return target;
        }

        private static void AssertBaseline(PrototypeInteractionStateOwner owner)
        {
            Assert.That(owner.GlobalState, Is.EqualTo(PrototypeGlobalInteractionState.Playable));
            Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
            Assert.That(owner.StrapState, Is.EqualTo(PrototypeStrapOwnershipState.Stored));
            Assert.That(owner.RelayState, Is.EqualTo(PrototypeRelayReservationState.None));
            Assert.That(owner.CarriedTarget, Is.Null);
            Assert.That(owner.StrapTarget, Is.Null);
            Assert.That(owner.RelayReservedTarget, Is.Null);
        }
    }
}
