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
