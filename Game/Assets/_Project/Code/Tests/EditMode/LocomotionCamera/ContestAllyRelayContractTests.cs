using System.Collections.Generic;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestAllyRelayContractTests
    {
        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject item in createdObjects)
            {
                if (item != null)
                {
                    Object.DestroyImmediate(item);
                }
            }

            createdObjects.Clear();
        }

        [Test]
        public void ValidExactAllyPoolTarget_CommitsOnePhysicalReservation()
        {
            RelayFixture fixture = CreateFixture();

            Assert.That(
                fixture.Relay.TryRequestSupply(fixture.BinderOne, out var rejection),
                Is.True);
            Assert.That(rejection, Is.EqualTo(PrototypeAllyRelayRejection.None));
            Assert.That(fixture.Owner.RelayState, Is.EqualTo(PrototypeRelayReservationState.Active));
            Assert.That(fixture.Owner.RelayReservedTarget, Is.SameAs(fixture.BinderOne));
            Assert.That(fixture.Relay.ActiveTarget, Is.SameAs(fixture.BinderOne));
            Assert.That(fixture.Relay.AllyState, Is.EqualTo(PrototypeAllySupportState.SupplyCommitted));
            Assert.That(fixture.Relay.RelayPhase, Is.EqualTo(PrototypeRelayPhase.PreRelease));
        }

        [Test]
        public void ZeroCandidate_IsSafeRejectionWithoutMutation()
        {
            RelayFixture fixture = CreateFixture();

            Assert.That(fixture.Relay.TryRequestSupply(null, out var rejection), Is.False);
            Assert.That(rejection, Is.EqualTo(PrototypeAllyRelayRejection.NoValidSupply));
            AssertRelayBaseline(fixture);
        }

        [TestCase(PrototypeInteractionRole.DirectBinder)]
        [TestCase(PrototypeInteractionRole.Drawer)]
        [TestCase(PrototypeInteractionRole.Copier)]
        public void DirectDrawerAndCopier_AreNeverRelayFallbacks(PrototypeInteractionRole role)
        {
            RelayFixture fixture = CreateFixture();
            PrototypeInteractionTarget invalid = CreateTarget(
                role.ToString(),
                role == PrototypeInteractionRole.Copier
                    ? PrototypeWeightClass.Heavy
                    : PrototypeWeightClass.Light,
                role,
                role == PrototypeInteractionRole.DirectBinder,
                false,
                false,
                false,
                true);

            Assert.That(fixture.Relay.TryRequestSupply(invalid, out var rejection), Is.False);
            Assert.That(rejection, Is.EqualTo(PrototypeAllyRelayRejection.NoValidSupply));
            AssertRelayBaseline(fixture);
        }

        [Test]
        public void Carrying_BlocksCallAllyWithoutDroppingCarry()
        {
            RelayFixture fixture = CreateFixture();
            PrototypeInteractionTarget direct = CreateTarget(
                "DIRECT",
                PrototypeWeightClass.Light,
                PrototypeInteractionRole.DirectBinder,
                true,
                false,
                false,
                false,
                true);
            Assert.That(fixture.Owner.TryCommit(
                PrototypeInteractionDecision.Accept(PrototypeInteractionOutcome.Scoop, direct)), Is.True);
            Assert.That(fixture.Owner.TryCompleteAcquisition(direct), Is.True);

            Assert.That(
                fixture.Relay.TryRequestSupply(fixture.BinderOne, out var rejection),
                Is.False);
            Assert.That(rejection, Is.EqualTo(PrototypeAllyRelayRejection.HandOwnershipBlocked));
            Assert.That(fixture.Owner.CarriedTarget, Is.SameAs(direct));
            Assert.That(fixture.Owner.RelayReservedTarget, Is.Null);
        }

        [Test]
        public void ActiveStrap_BlocksCallAlly()
        {
            RelayFixture fixture = CreateFixture();
            PrototypeInteractionTarget chair = CreateTarget(
                "STRAP-TARGET",
                PrototypeWeightClass.Medium,
                PrototypeInteractionRole.DirectChair,
                false,
                false,
                true,
                true,
                true);
            Assert.That(fixture.Owner.TryCommit(
                PrototypeInteractionDecision.Accept(PrototypeInteractionOutcome.AttachStrap, chair)), Is.True);

            Assert.That(
                fixture.Relay.TryRequestSupply(fixture.BinderOne, out var rejection),
                Is.False);
            Assert.That(rejection, Is.EqualTo(PrototypeAllyRelayRejection.StrapOwnershipBlocked));
            Assert.That(fixture.Owner.StrapTarget, Is.SameAs(chair));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ResetOrFailurePending_BlocksCallAlly(bool reset)
        {
            RelayFixture fixture = CreateFixture();
            Assert.That(reset
                ? fixture.Owner.TryBeginReset()
                : fixture.Owner.TryBeginFailurePending(), Is.True);

            Assert.That(
                fixture.Relay.TryRequestSupply(fixture.BinderOne, out var rejection),
                Is.False);
            Assert.That(rejection, Is.EqualTo(PrototypeAllyRelayRejection.GlobalStateBlocked));
            Assert.That(fixture.Owner.RelayReservedTarget, Is.Null);
        }

        [Test]
        public void RepeatedCallAlly_DoesNotCreateSecondReservation()
        {
            RelayFixture fixture = CreateFixture();
            Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderOne, out _), Is.True);

            Assert.That(
                fixture.Relay.TryRequestSupply(fixture.BinderTwo, out var rejection),
                Is.False);
            Assert.That(rejection, Is.EqualTo(PrototypeAllyRelayRejection.RelayAlreadyActive));
            Assert.That(fixture.Owner.RelayReservedTarget, Is.SameAs(fixture.BinderOne));
            Assert.That(fixture.Relay.ActiveTarget, Is.SameAs(fixture.BinderOne));
        }

        [Test]
        public void CatchArbitration_RequiresReservedInstanceIdentity()
        {
            RelayFixture fixture = CreateFixture();
            var arbitrator = new PrototypeInteractionArbitrator();
            Assert.That(fixture.Owner.TryReserveRelay(fixture.BinderOne), Is.True);

            PrototypeInteractionDecision wrong = arbitrator.Decide(
                fixture.Owner.CreateContext(false, fixture.BinderTwo));

            Assert.That(wrong.IsAccepted, Is.False);
            Assert.That(wrong.Rejection, Is.EqualTo(PrototypeInteractionRejection.InvalidRelayContext));
            Assert.That(fixture.Owner.RelayReservedTarget, Is.SameAs(fixture.BinderOne));
        }

        [Test]
        public void BinderRelay_ArmsQueuedThrowExactlyOnce()
        {
            RelayFixture fixture = CreateFixture();
            Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderOne, out _), Is.True);

            Assert.That(fixture.Relay.TryArmQueuedThrow(), Is.True);
            Assert.That(fixture.Relay.TryArmQueuedThrow(), Is.False);
            Assert.That(fixture.Owner.QueuedThrowArmed, Is.True);
            Assert.That(fixture.Owner.RelayReservedTarget, Is.SameAs(fixture.BinderOne));
        }

        [Test]
        public void BinderSupplySpeedVariation_HasSafeResettableBounds()
        {
            RelayFixture fixture = CreateFixture();

            Assert.That(fixture.Relay.MinimumBinderSpeedMultiplier, Is.EqualTo(0.9f).Within(0.001f));
            Assert.That(fixture.Relay.MaximumBinderSpeedMultiplier, Is.EqualTo(1.1f).Within(0.001f));
            Assert.That(fixture.Relay.LastBinderSpeedMultiplier, Is.EqualTo(1f).Within(0.001f));
            Assert.That(fixture.Relay.BinderSpeedVariationSequence, Is.Zero);
        }

        [Test]
        public void ChairRelay_CannotArmQueuedThrow()
        {
            RelayFixture fixture = CreateFixture();
            Assert.That(fixture.Relay.TryRequestSupply(fixture.Chair, out _), Is.True);

            Assert.That(fixture.Relay.TryArmQueuedThrow(), Is.False);
            Assert.That(fixture.Owner.QueuedThrowArmed, Is.False);
        }

        [Test]
        public void MissInvalidationAndReset_ClearQueuedThrow()
        {
            RelayFixture fixture = CreateFixture();
            Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderOne, out _), Is.True);
            Assert.That(fixture.Relay.TryArmQueuedThrow(), Is.True);

            fixture.Relay.ResolveCatchFailure(fixture.BinderOne);
            Assert.That(fixture.Owner.QueuedThrowArmed, Is.False);
            Assert.That(fixture.Owner.RelayState, Is.EqualTo(PrototypeRelayReservationState.None));

            Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderTwo, out _), Is.True);
            Assert.That(fixture.Relay.TryArmQueuedThrow(), Is.True);
            Assert.That(fixture.Owner.TryBeginReset(), Is.True);
            fixture.Relay.ResetToAuthoredBaseline();
            Assert.That(fixture.Owner.RestoreInteractionBaseline(), Is.True);
            Assert.That(fixture.Owner.QueuedThrowArmed, Is.False);
            AssertRelayBaseline(fixture);
        }

        [Test]
        public void NormalCatch_CommitKeepsExactRelayReservationUntilContactCompletion()
        {
            RelayFixture fixture = CreateFixture();
            var arbitrator = new PrototypeInteractionArbitrator();
            Assert.That(fixture.Owner.TryReserveRelay(fixture.BinderOne), Is.True);

            PrototypeInteractionDecision catchDecision = arbitrator.Decide(
                fixture.Owner.CreateContext(false, fixture.BinderOne));

            Assert.That(fixture.Owner.TryCommit(catchDecision), Is.True);
            Assert.That(fixture.Owner.HandState,
                Is.EqualTo(PrototypeHandOwnershipState.AcquisitionCommitted));
            Assert.That(fixture.Owner.AcquisitionCommittedTarget,
                Is.SameAs(fixture.BinderOne));
            Assert.That(fixture.Owner.RelayState,
                Is.EqualTo(PrototypeRelayReservationState.CatchCommitted));
            Assert.That(fixture.Owner.RelayReservedTarget,
                Is.SameAs(fixture.BinderOne));
            Assert.That(fixture.Owner.TryCompleteAcquisition(fixture.BinderOne), Is.True);
            Assert.That(fixture.Owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Carrying));
            Assert.That(fixture.Owner.CarriedTarget, Is.SameAs(fixture.BinderOne));
            Assert.That(fixture.Owner.RelayState,
                Is.EqualTo(PrototypeRelayReservationState.CatchCommitted));
            Assert.That(
                fixture.Owner.TryCompleteRelayCatchReservation(fixture.BinderOne),
                Is.True);
            Assert.That(fixture.Owner.RelayState, Is.EqualTo(PrototypeRelayReservationState.None));
            Assert.That(fixture.Owner.RelayReservedTarget, Is.Null);
            Assert.That(
                fixture.Owner.TryCompleteRelayCatchReservation(fixture.BinderOne),
                Is.False,
                "The animation contact transaction must settle exactly once.");
        }

        [Test]
        public void QueuedCatch_CommitConsumesQueueButKeepsExactRelayReservation()
        {
            RelayFixture fixture = CreateFixture();
            var arbitrator = new PrototypeInteractionArbitrator();
            Assert.That(fixture.Owner.TryReserveRelay(fixture.BinderOne), Is.True);
            Assert.That(fixture.Owner.TryArmQueuedThrow(), Is.True);

            PrototypeInteractionDecision catchDecision = arbitrator.Decide(
                fixture.Owner.CreateContext(false, fixture.BinderOne));

            Assert.That(fixture.Owner.TryCommit(catchDecision), Is.True);
            Assert.That(fixture.Owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.ThrowCommitted));
            Assert.That(fixture.Owner.ThrowCommittedTarget, Is.SameAs(fixture.BinderOne));
            Assert.That(fixture.Owner.CarriedTarget, Is.Null);
            Assert.That(fixture.Owner.QueuedThrowArmed, Is.False);
            Assert.That(fixture.Owner.RelayState,
                Is.EqualTo(PrototypeRelayReservationState.CatchCommitted));
            Assert.That(fixture.Owner.RelayReservedTarget,
                Is.SameAs(fixture.BinderOne));
            Assert.That(
                fixture.Owner.TryCompleteRelayCatchReservation(fixture.BinderOne),
                Is.True);
            Assert.That(fixture.Owner.RelayState,
                Is.EqualTo(PrototypeRelayReservationState.None));
        }

        [Test]
        public void CatchReservationCompletionAndCancellation_AreTargetCheckedAndIdempotent()
        {
            RelayFixture fixture = CreateFixture();
            var arbitrator = new PrototypeInteractionArbitrator();
            Assert.That(fixture.Owner.TryReserveRelay(fixture.BinderOne), Is.True);
            PrototypeInteractionDecision catchDecision = arbitrator.Decide(
                fixture.Owner.CreateContext(false, fixture.BinderOne));
            Assert.That(fixture.Owner.TryCommit(catchDecision), Is.True);

            Assert.That(
                fixture.Owner.TryCompleteRelayCatchReservation(fixture.BinderTwo),
                Is.False);
            Assert.That(
                fixture.Owner.TryCancelRelayReservation(fixture.BinderTwo),
                Is.False);
            Assert.That(fixture.Owner.RelayState,
                Is.EqualTo(PrototypeRelayReservationState.CatchCommitted));
            Assert.That(
                fixture.Owner.TryCancelRelayReservation(fixture.BinderOne),
                Is.True);
            Assert.That(
                fixture.Owner.TryCancelRelayReservation(fixture.BinderOne),
                Is.False);
            Assert.That(fixture.Owner.RelayState,
                Is.EqualTo(PrototypeRelayReservationState.None));
            Assert.That(fixture.Owner.RelayReservedTarget, Is.Null);
        }

        [Test]
        public void AuthoredReset_RestoresAllyRelayReservationAndQueueBaseline()
        {
            RelayFixture fixture = CreateFixture();
            Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderOne, out _), Is.True);
            Assert.That(fixture.Relay.TryArmQueuedThrow(), Is.True);
            Assert.That(fixture.Owner.TryBeginReset(), Is.True);

            fixture.Relay.ResetToAuthoredBaseline();
            Assert.That(fixture.Owner.RestoreInteractionBaseline(), Is.True);

            AssertRelayBaseline(fixture);
            Assert.That(fixture.Owner.QueuedThrowArmed, Is.False);
        }

        [Test]
        public void PresentationRead_DoesNotMutateGameplayState()
        {
            RelayFixture fixture = CreateFixture();
            Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderOne, out _), Is.True);
            PrototypeInteractionController controller =
                fixture.Player.AddComponent<PrototypeInteractionController>();
            controller.Configure(
                null,
                fixture.Owner,
                fixture.View.transform,
                fixture.AllTargets,
                2.8f,
                false,
                fixture.Lifecycle,
                fixture.Relay);
            InteractionReadabilityPresenter presenter =
                fixture.Player.AddComponent<InteractionReadabilityPresenter>();
            presenter.Configure(controller);
            PrototypeInteractionTarget reservedBefore = fixture.Owner.RelayReservedTarget;
            PrototypeRelayPhase phaseBefore = fixture.Relay.RelayPhase;

            string text = presenter.BuildStatusText();

            Assert.That(text, Does.Contain("SUPPLY COMMITTED"));
            Assert.That(fixture.Owner.RelayReservedTarget, Is.SameAs(reservedBefore));
            Assert.That(fixture.Relay.RelayPhase, Is.EqualTo(phaseBefore));
            Assert.That(fixture.Owner.RelayState, Is.EqualTo(PrototypeRelayReservationState.Active));
        }

        private RelayFixture CreateFixture()
        {
            GameObject player = CreateObject("Player");
            player.transform.position = Vector3.zero;
            player.AddComponent<CharacterController>();
            PrototypeInteractionStateOwner owner =
                player.AddComponent<PrototypeInteractionStateOwner>();
            PrototypePhysicalObjectLifecycle lifecycle =
                player.AddComponent<PrototypePhysicalObjectLifecycle>();
            GameObject anchor = CreateObject("CarryAnchor");
            anchor.transform.SetParent(player.transform, false);
            anchor.transform.localPosition = new Vector3(0f, 1f, 1f);
            lifecycle.Configure(owner, anchor.transform);

            GameObject ally = CreateObject("Ally");
            ally.transform.position = new Vector3(-2f, 0f, 3f);
            GameObject view = CreateObject("View");
            view.transform.position = new Vector3(0f, 1f, -1f);
            view.transform.rotation = Quaternion.identity;
            PrototypeInteractionTarget binderOne = CreateTarget(
                "BINDER-ALLY-01", PrototypeWeightClass.Light,
                PrototypeInteractionRole.AllyBinder, false, true, false, false, true);
            PrototypeInteractionTarget binderTwo = CreateTarget(
                "BINDER-ALLY-02", PrototypeWeightClass.Light,
                PrototypeInteractionRole.AllyBinder, false, true, false, false, true);
            PrototypeInteractionTarget chair = CreateTarget(
                "CHAIR-ALLY-01", PrototypeWeightClass.Medium,
                PrototypeInteractionRole.AllyChair, false, false, true, true, true);
            binderOne.transform.position = new Vector3(0f, 1f, 2f);
            binderTwo.transform.position = new Vector3(1f, 1f, 2f);
            chair.transform.position = new Vector3(-1f, 1f, 2f);
            Physics.SyncTransforms();

            PrototypeAllyRelayController relay =
                ally.AddComponent<PrototypeAllyRelayController>();
            PrototypeInteractionTarget[] allTargets = { binderOne, binderTwo, chair };
            relay.Configure(
                owner,
                lifecycle,
                ally.transform,
                player.transform,
                allTargets,
                0.02f,
                6f,
                3f,
                2.8f,
                0.2f,
                0.5f,
                0.1f,
                0.5f);
            return new RelayFixture(
                player,
                ally,
                view,
                owner,
                lifecycle,
                relay,
                binderOne,
                binderTwo,
                chair,
                allTargets);
        }

        private PrototypeInteractionTarget CreateTarget(
            string key,
            PrototypeWeightClass weightClass,
            PrototypeInteractionRole role,
            bool direct,
            bool catchEligible,
            bool strapEligible,
            bool anchor,
            bool addBody)
        {
            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            createdObjects.Add(item);
            if (addBody)
            {
                Rigidbody body = item.AddComponent<Rigidbody>();
                body.useGravity = false;
            }

            PrototypeInteractionTarget target =
                item.AddComponent<PrototypeInteractionTarget>();
            target.Configure(
                "TEST-DEFINITION",
                key,
                role.ToString(),
                weightClass,
                role,
                direct,
                catchEligible,
                strapEligible,
                anchor);
            return target;
        }

        private GameObject CreateObject(string name)
        {
            GameObject item = new GameObject(name);
            createdObjects.Add(item);
            return item;
        }

        private static void AssertRelayBaseline(RelayFixture fixture)
        {
            Assert.That(fixture.Relay.HasBaselineState, Is.True);
            Assert.That(fixture.Owner.RelayState, Is.EqualTo(PrototypeRelayReservationState.None));
            Assert.That(fixture.Owner.RelayReservedTarget, Is.Null);
            Assert.That(fixture.Relay.ActiveTarget, Is.Null);
            Assert.That(fixture.Relay.AllyState, Is.EqualTo(PrototypeAllySupportState.Ready));
        }

        private sealed class RelayFixture
        {
            public RelayFixture(
                GameObject player,
                GameObject ally,
                GameObject view,
                PrototypeInteractionStateOwner owner,
                PrototypePhysicalObjectLifecycle lifecycle,
                PrototypeAllyRelayController relay,
                PrototypeInteractionTarget binderOne,
                PrototypeInteractionTarget binderTwo,
                PrototypeInteractionTarget chair,
                PrototypeInteractionTarget[] allTargets)
            {
                Player = player;
                Ally = ally;
                View = view;
                Owner = owner;
                Lifecycle = lifecycle;
                Relay = relay;
                BinderOne = binderOne;
                BinderTwo = binderTwo;
                Chair = chair;
                AllTargets = allTargets;
            }

            public GameObject Player { get; }
            public GameObject Ally { get; }
            public GameObject View { get; }
            public PrototypeInteractionStateOwner Owner { get; }
            public PrototypePhysicalObjectLifecycle Lifecycle { get; }
            public PrototypeAllyRelayController Relay { get; }
            public PrototypeInteractionTarget BinderOne { get; }
            public PrototypeInteractionTarget BinderTwo { get; }
            public PrototypeInteractionTarget Chair { get; }
            public PrototypeInteractionTarget[] AllTargets { get; }
        }
    }
}
