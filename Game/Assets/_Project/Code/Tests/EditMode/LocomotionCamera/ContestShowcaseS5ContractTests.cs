using System.IO;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestShowcaseS5ContractTests
    {
        private GameObject ownerObject;
        private GameObject targetObject;

        [TearDown]
        public void TearDown()
        {
            if (targetObject != null)
            {
                Object.DestroyImmediate(targetObject);
            }
            if (ownerObject != null)
            {
                Object.DestroyImmediate(ownerObject);
            }
        }

        [Test]
        public void StrapOwner_TransitionsAttachedTensionedStoredWithOneExactTarget()
        {
            ownerObject = new GameObject("S5Owner");
            PrototypeInteractionStateOwner owner =
                ownerObject.AddComponent<PrototypeInteractionStateOwner>();
            PrototypeInteractionTarget chair = CreateChair();
            PrototypeInteractionDecision attach = PrototypeInteractionDecision.Accept(
                PrototypeInteractionOutcome.AttachStrap,
                chair);

            Assert.That(owner.TryCommit(attach), Is.True);
            Assert.That(owner.StrapState, Is.EqualTo(PrototypeStrapOwnershipState.Attached));
            Assert.That(owner.StrapTarget, Is.SameAs(chair));
            Assert.That(owner.TryBeginStrapTension(chair), Is.True);
            Assert.That(owner.StrapState, Is.EqualTo(PrototypeStrapOwnershipState.Tensioned));
            Assert.That(owner.TryBeginStrapTension(chair), Is.False);
            Assert.That(owner.TryReleaseStrap(), Is.True);
            Assert.That(owner.StrapState, Is.EqualTo(PrototypeStrapOwnershipState.Stored));
            Assert.That(owner.StrapTarget, Is.Null);
            Assert.That(owner.TryReleaseStrap(), Is.False);
        }

        [Test]
        public void S5Runtime_UsesBoundedGroundedRedirectAndNoRopeSimulation()
        {
            string source = File.ReadAllText(
                "Assets/_Project/Code/Runtime/Player/ShowcaseCargoStrapController.cs");

            StringAssert.Contains("IsFloorSupported", source);
            StringAssert.Contains("maximumStrapLength", source);
            StringAssert.Contains("TryExplicitRelease", source);
            StringAssert.Contains("RigidbodyConstraints.FreezeRotationX", source);
            StringAssert.Contains("RigidbodyConstraints.FreezeRotationZ", source);
            StringAssert.DoesNotContain("Joint", source);
            StringAssert.DoesNotContain("Spring", source);
            StringAssert.DoesNotContain("Grapple", source);
        }

        private PrototypeInteractionTarget CreateChair()
        {
            targetObject = new GameObject("S5Chair");
            PrototypeInteractionTarget target =
                targetObject.AddComponent<PrototypeInteractionTarget>();
            target.Configure(
                "OBJ-P-M-001",
                "S5-CHAIR-01",
                "Wheeled Office Chair",
                PrototypeWeightClass.Medium,
                PrototypeInteractionRole.DirectChair,
                true,
                false,
                true,
                true);
            return target;
        }
    }
}
