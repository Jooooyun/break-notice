using System.Collections.Generic;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestContextualWeaponSelectionContractTests
    {
        private readonly List<GameObject> createdObjects = new();

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
        public void OwnershipTransitions_AutoSelectCarriedOrRelayObjectAndFallBackToFists()
        {
            PrototypeInteractionStateOwner owner = CreateOwner();
            PrototypeInteractionTarget binder = CreateTarget(
                "BINDER-01",
                PrototypeWeightClass.Light,
                PrototypeInteractionRole.DirectBinder,
                direct: true,
                catchEligible: false,
                strapEligible: false,
                anchor: false);
            PrototypeInteractionTarget incoming = CreateTarget(
                "ALLY-BINDER-01",
                PrototypeWeightClass.Light,
                PrototypeInteractionRole.AllyBinder,
                direct: false,
                catchEligible: true,
                strapEligible: false,
                anchor: false);

            Assert.That(owner.WeaponSelection,
                Is.EqualTo(PrototypeWeaponSelection.Fists));
            Assert.That(owner.ObjectWeaponTarget, Is.Null);
            Assert.That(owner.TrySelectWeapon(
                PrototypeWeaponSelection.ObjectWeapon), Is.False);

            Assert.That(owner.TryCommit(PrototypeInteractionDecision.Accept(
                PrototypeInteractionOutcome.Scoop, binder)), Is.True);
            Assert.That(owner.WeaponSelection,
                Is.EqualTo(PrototypeWeaponSelection.ObjectWeapon));
            Assert.That(owner.TryCompleteAcquisition(binder), Is.True);
            Assert.That(owner.ObjectWeaponTarget, Is.SameAs(binder));
            Assert.That(owner.CancelCarry(binder), Is.True);
            Assert.That(owner.WeaponSelection,
                Is.EqualTo(PrototypeWeaponSelection.Fists));

            Assert.That(owner.TryReserveRelay(incoming), Is.True);
            Assert.That(owner.WeaponSelection,
                Is.EqualTo(PrototypeWeaponSelection.ObjectWeapon));
            Assert.That(owner.ObjectWeaponTarget, Is.SameAs(incoming));
            Assert.That(owner.TrySelectWeapon(
                PrototypeWeaponSelection.Fists), Is.False);
            owner.ClearRelayReservation();
            Assert.That(owner.WeaponSelection,
                Is.EqualTo(PrototypeWeaponSelection.Fists));
        }

        [Test]
        public void Selection_SimultaneousRequestIsNoOpAndResetRestoresFists()
        {
            PrototypeInteractionStateOwner owner = CreateOwner();
            PrototypeInteractionController controller =
                owner.gameObject.AddComponent<PrototypeInteractionController>();
            controller.Configure(null, owner, null, null, 2.8f, false);

            Assert.That(controller.ProcessWeaponSelection(true, true), Is.False);
            Assert.That(owner.WeaponSelection,
                Is.EqualTo(PrototypeWeaponSelection.Fists));
            Assert.That(controller.ProcessWeaponSelection(false, true), Is.False);

            PrototypeInteractionTarget binder = CreateTarget(
                "BINDER-RESET",
                PrototypeWeightClass.Light,
                PrototypeInteractionRole.DirectBinder,
                direct: true,
                catchEligible: false,
                strapEligible: false,
                anchor: false);
            Assert.That(owner.TryCommit(PrototypeInteractionDecision.Accept(
                PrototypeInteractionOutcome.Scoop, binder)), Is.True);
            Assert.That(owner.TryCompleteAcquisition(binder), Is.True);
            Assert.That(owner.WeaponSelection,
                Is.EqualTo(PrototypeWeaponSelection.ObjectWeapon));
            Assert.That(owner.TryBeginReset(), Is.True);
            Assert.That(owner.RestoreInteractionBaseline(), Is.True);
            Assert.That(owner.WeaponSelection,
                Is.EqualTo(PrototypeWeaponSelection.Fists));
            Assert.That(owner.ObjectWeaponTarget, Is.Null);
        }

        [Test]
        public void TensionedStrapTarget_CanBeSelectedAsObjectWeapon()
        {
            PrototypeInteractionStateOwner owner = CreateOwner();
            PrototypeInteractionTarget chair = CreateTarget(
                "CHAIR-STRAP",
                PrototypeWeightClass.Medium,
                PrototypeInteractionRole.DirectChair,
                direct: true,
                catchEligible: false,
                strapEligible: true,
                anchor: true);

            Assert.That(owner.TryCommit(PrototypeInteractionDecision.Accept(
                PrototypeInteractionOutcome.AttachStrap, chair)), Is.True);
            Assert.That(owner.TryBeginStrapTension(chair), Is.True);
            Assert.That(owner.WeaponSelection,
                Is.EqualTo(PrototypeWeaponSelection.Fists));
            Assert.That(owner.ObjectWeaponTarget, Is.SameAs(chair));
            Assert.That(owner.TrySelectWeapon(
                PrototypeWeaponSelection.ObjectWeapon), Is.True);
            Assert.That(owner.WeaponSelection,
                Is.EqualTo(PrototypeWeaponSelection.ObjectWeapon));
        }

        [Test]
        public void Heavy_RemainsUnavailableToPickupRelayAndObjectSelection()
        {
            PrototypeInteractionStateOwner owner = CreateOwner();
            PrototypeInteractionTarget heavy = CreateTarget(
                "HEAVY-01",
                PrototypeWeightClass.Heavy,
                PrototypeInteractionRole.Copier,
                direct: true,
                catchEligible: true,
                strapEligible: true,
                anchor: true);

            Assert.That(owner.TryCommit(PrototypeInteractionDecision.Accept(
                PrototypeInteractionOutcome.Scoop, heavy)), Is.False);
            Assert.That(owner.TryReserveRelay(heavy), Is.False);
            Assert.That(owner.TrySelectWeapon(
                PrototypeWeaponSelection.ObjectWeapon), Is.False);
            Assert.That(owner.WeaponSelection,
                Is.EqualTo(PrototypeWeaponSelection.Fists));
            Assert.That(owner.ObjectWeaponTarget, Is.Null);
        }

        private PrototypeInteractionStateOwner CreateOwner()
        {
            GameObject item = new("ContextualWeaponOwner");
            createdObjects.Add(item);
            return item.AddComponent<PrototypeInteractionStateOwner>();
        }

        private PrototypeInteractionTarget CreateTarget(
            string key,
            PrototypeWeightClass weight,
            PrototypeInteractionRole role,
            bool direct,
            bool catchEligible,
            bool strapEligible,
            bool anchor)
        {
            GameObject item = new(key);
            createdObjects.Add(item);
            PrototypeInteractionTarget target =
                item.AddComponent<PrototypeInteractionTarget>();
            target.Configure(
                $"DEF-{key}",
                key,
                key,
                weight,
                role,
                direct,
                catchEligible,
                strapEligible,
                anchor);
            return target;
        }
    }
}
