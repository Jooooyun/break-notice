using System.Collections.Generic;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestCarryThrowContractTests
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
        public void CarryCommitAndThrowCommit_HaveOneOrderedOwner()
        {
            PrototypeInteractionStateOwner owner = CreatePlayer().owner;
            PrototypeInteractionTarget binder = CreateTarget(
                PrototypeWeightClass.Light,
                PrototypeInteractionRole.DirectBinder,
                true,
                true);
            var arbitrator = new PrototypeInteractionArbitrator();

            Assert.That(owner.TryCommit(arbitrator.Decide(owner.CreateContext(false, binder))), Is.True);
            Assert.That(owner.HandState,
                Is.EqualTo(PrototypeHandOwnershipState.AcquisitionCommitted));
            Assert.That(owner.AcquisitionCommittedTarget, Is.SameAs(binder));
            Assert.That(owner.TryCompleteAcquisition(binder), Is.True);
            Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Carrying));
            Assert.That(owner.TryCommitThrow(out PrototypeInteractionTarget committed), Is.True);
            Assert.That(committed, Is.SameAs(binder));
            Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.ThrowCommitted));
            Assert.That(owner.CompleteThrow(binder), Is.True);
            Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
            Assert.That(owner.CarriedTarget, Is.Null);
            Assert.That(owner.ThrowCommittedTarget, Is.Null);
        }

        [Test]
        public void PhysicalLifecycle_RequiresVisibleEligibleRigidbodyTarget()
        {
            (PrototypeInteractionStateOwner owner, PrototypePhysicalObjectLifecycle lifecycle) player =
                CreatePlayer();
            PrototypeInteractionTarget binder = CreateTarget(
                PrototypeWeightClass.Light,
                PrototypeInteractionRole.DirectBinder,
                true,
                false);
            var arbitrator = new PrototypeInteractionArbitrator();

            Assert.That(player.owner.TryCommit(
                arbitrator.Decide(player.owner.CreateContext(false, binder))), Is.True);
            Assert.That(player.lifecycle.BeginCarry(binder), Is.False);
            Assert.That(binder.gameObject.activeSelf, Is.True);
            Assert.That(binder.GetComponent<Renderer>().enabled, Is.True);
        }

        [Test]
        public void ThrowProfile_DistinguishesLightAndMediumWithoutHeavyPath()
        {
            PrototypePhysicalObjectLifecycle lifecycle = CreatePlayer().lifecycle;

            Assert.That(
                lifecycle.GetThrowSpeed(PrototypeWeightClass.Light),
                Is.EqualTo(12.5f).Within(0.001f));
            Assert.That(
                lifecycle.GetThrowSpeed(PrototypeWeightClass.Medium),
                Is.EqualTo(8f).Within(0.001f));
            Assert.That(
                lifecycle.GetThrowSpeed(PrototypeWeightClass.Light),
                Is.GreaterThan(lifecycle.GetThrowSpeed(PrototypeWeightClass.Medium)));
            Assert.That(
                lifecycle.GetThrowSpeed(PrototypeWeightClass.Heavy),
                Is.EqualTo(lifecycle.GetThrowSpeed(PrototypeWeightClass.Medium)));
        }

        [Test]
        public void ThrowUseBudget_IsWeightDerivedWithoutOpeningHeavyAcquisition()
        {
            PrototypeInteractionTarget light = CreateTarget(
                PrototypeWeightClass.Light,
                PrototypeInteractionRole.DirectBinder,
                true,
                true);
            PrototypeInteractionTarget medium = CreateTarget(
                PrototypeWeightClass.Medium,
                PrototypeInteractionRole.DirectChair,
                true,
                true);
            PrototypeInteractionTarget heavy = CreateTarget(
                PrototypeWeightClass.Heavy,
                PrototypeInteractionRole.Copier,
                true,
                false);
            var arbitrator = new PrototypeInteractionArbitrator();
            PrototypeInteractionStateOwner owner = CreatePlayer().owner;

            Assert.That(light.MaximumPlayerThrowUses, Is.EqualTo(1));
            Assert.That(medium.MaximumPlayerThrowUses, Is.EqualTo(2));
            Assert.That(heavy.MaximumPlayerThrowUses, Is.EqualTo(3));
            Assert.That(light.CommittedPlayerThrowUses, Is.Zero);
            Assert.That(light.RemainingPlayerThrowUses, Is.EqualTo(1));
            Assert.That(light.IsPlayerThrowExhausted, Is.False);
            Assert.That(light.IsThrowDepletionPending, Is.False);
            Assert.That(light.LastCommittedPlayerThrowSequence, Is.Zero);
            Assert.That(heavy.IsScoopEligible, Is.False);
            Assert.That(
                arbitrator.Decide(owner.CreateContext(false, heavy)).Rejection,
                Is.EqualTo(PrototypeInteractionRejection.HeavyRecognitionOnly));
        }

        [Test]
        public void SuccessfulPhysicalThrow_CommitsExactlyOneUse()
        {
            (PrototypeInteractionStateOwner owner, PrototypePhysicalObjectLifecycle lifecycle) player =
                CreatePlayer();
            PrototypeInteractionTarget binder = CreateTarget(
                PrototypeWeightClass.Light,
                PrototypeInteractionRole.DirectBinder,
                true,
                true);
            var arbitrator = new PrototypeInteractionArbitrator();

            Assert.That(player.owner.TryCommit(
                arbitrator.Decide(player.owner.CreateContext(false, binder))), Is.True);
            Assert.That(player.owner.TryCompleteAcquisition(binder), Is.True);
            Assert.That(player.lifecycle.BeginCarry(binder), Is.True);
            Assert.That(player.lifecycle.TryThrow(Vector3.forward), Is.True);

            Assert.That(binder.CommittedPlayerThrowUses, Is.EqualTo(1));
            Assert.That(binder.RemainingPlayerThrowUses, Is.Zero);
            Assert.That(binder.IsPlayerThrowExhausted, Is.True);
            Assert.That(binder.IsThrowDepletionPending, Is.True);
            Assert.That(binder.LastCommittedPlayerThrowSequence, Is.EqualTo(1));
            Assert.That(binder.DirectAcquisitionEligible, Is.False);
            Assert.That(player.lifecycle.ThrowOriginCount, Is.EqualTo(1));
        }

        [Test]
        public void ChargedThrowProfile_PreservesTapAndBoundsMaximumSpeedAndEnergyMultiplier()
        {
            PrototypePhysicalObjectLifecycle lifecycle = CreatePlayer().lifecycle;

            Assert.That(lifecycle.MaximumThrowSpeedMultiplier, Is.EqualTo(1.6f).Within(0.001f));
            Assert.That(lifecycle.GetThrowSpeedMultiplier(0f), Is.EqualTo(1f).Within(0.001f));
            Assert.That(lifecycle.GetThrowSpeedMultiplier(0.5f), Is.EqualTo(1.3f).Within(0.001f));
            Assert.That(lifecycle.GetThrowSpeedMultiplier(1f), Is.EqualTo(1.6f).Within(0.001f));
            Assert.That(
                lifecycle.GetThrowSpeed(PrototypeWeightClass.Light, 0f),
                Is.EqualTo(12.5f).Within(0.001f));
            Assert.That(
                lifecycle.GetThrowSpeed(PrototypeWeightClass.Light, 1f),
                Is.EqualTo(20f).Within(0.001f));
            Assert.That(
                lifecycle.GetThrowSpeed(PrototypeWeightClass.Medium, 1f),
                Is.EqualTo(12.8f).Within(0.001f));
            Assert.That(
                lifecycle.GetThrowSpeedMultiplier(1f) * lifecycle.GetThrowSpeedMultiplier(1f),
                Is.EqualTo(2.56f).Within(0.001f));
        }

        [Test]
        public void BallisticThrow_AddsVisibleArcWithoutChangingAuthoredTotalSpeed()
        {
            PrototypeThrowSolution light = PrototypeThrowBallistics.Solve(
                Vector3.up,
                Vector3.forward,
                12.5f,
                Vector3.forward,
                Physics.gravity);
            PrototypeThrowSolution medium = PrototypeThrowBallistics.Solve(
                Vector3.up,
                Vector3.forward,
                8f,
                Vector3.forward,
                Physics.gravity);
            PrototypeThrowSolution charged = PrototypeThrowBallistics.Solve(
                Vector3.up,
                Vector3.forward,
                20f,
                Vector3.forward,
                Physics.gravity);

            Assert.That(light.IsValid, Is.True);
            Assert.That(light.InitialVelocity.magnitude,
                Is.EqualTo(12.5f).Within(0.001f));
            Assert.That(medium.InitialVelocity.magnitude,
                Is.EqualTo(8f).Within(0.001f));
            Assert.That(charged.InitialVelocity.magnitude,
                Is.EqualTo(20f).Within(0.001f));
            Assert.That(light.InitialVelocity.y, Is.GreaterThan(0f));
            Assert.That(medium.InitialVelocity.y, Is.GreaterThan(0f));
            Assert.That(
                medium.InitialVelocity.y / medium.InitialVelocity.z,
                Is.GreaterThan(light.InitialVelocity.y / light.InitialVelocity.z));

            float apexTime = -light.InitialVelocity.y / Physics.gravity.y;
            Assert.That(apexTime, Is.GreaterThan(0f));
            Assert.That(light.PositionAt(apexTime).y,
                Is.GreaterThan(light.Origin.y + 0.2f));
            Assert.That(light.PositionAt(apexTime * 2.2f).y,
                Is.LessThan(light.PositionAt(apexTime).y));
        }

        [Test]
        public void PlayerThrowOrigin_SeparatesDefinitionInstanceAndSequence()
        {
            var origin = new PlayerThrowOrigin(
                4,
                "OBJ-P-L-001",
                "BINDER-DIRECT-01",
                PrototypeWeightClass.Light);

            Assert.That(origin.IsPlayerOwned, Is.True);
            Assert.That(origin.Sequence, Is.EqualTo(4));
            Assert.That(origin.ContentDefinitionId, Is.EqualTo("OBJ-P-L-001"));
            Assert.That(origin.RuntimeInstanceKey, Is.EqualTo("BINDER-DIRECT-01"));
            Assert.That(origin.ContentDefinitionId, Is.Not.EqualTo(origin.RuntimeInstanceKey));
        }

        private (PrototypeInteractionStateOwner owner, PrototypePhysicalObjectLifecycle lifecycle)
            CreatePlayer()
        {
            GameObject player = new GameObject("Player");
            createdObjects.Add(player);
            PrototypeInteractionStateOwner owner =
                player.AddComponent<PrototypeInteractionStateOwner>();
            PrototypePhysicalObjectLifecycle lifecycle =
                player.AddComponent<PrototypePhysicalObjectLifecycle>();
            GameObject anchor = new GameObject("CarryAnchor");
            createdObjects.Add(anchor);
            anchor.transform.SetParent(player.transform, false);
            lifecycle.Configure(owner, anchor.transform);
            return (owner, lifecycle);
        }

        private PrototypeInteractionTarget CreateTarget(
            PrototypeWeightClass weightClass,
            PrototypeInteractionRole role,
            bool direct,
            bool addRigidbody)
        {
            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            createdObjects.Add(item);
            if (addRigidbody)
            {
                item.AddComponent<Rigidbody>();
            }

            PrototypeInteractionTarget target = item.AddComponent<PrototypeInteractionTarget>();
            target.Configure(
                "TEST-DEFINITION",
                role.ToString(),
                role.ToString(),
                weightClass,
                role,
                direct,
                false,
                false,
                false);
            return target;
        }
    }
}
