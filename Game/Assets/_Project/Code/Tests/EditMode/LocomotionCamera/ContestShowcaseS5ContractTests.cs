using System.Collections.Generic;
using System.IO;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestShowcaseS5ContractTests
    {
        private readonly List<GameObject> createdObjects = new();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject createdObject in createdObjects)
            {
                if (createdObject != null)
                {
                    Object.DestroyImmediate(createdObject);
                }
            }
            createdObjects.Clear();
        }

        [Test]
        public void StrapController_DefersOwnershipUntilThrowContactAndTensionsAfterPull()
        {
            GameObject ownerObject = CreateObject("S5Owner");
            CapsuleCollider playerCollider =
                ownerObject.AddComponent<CapsuleCollider>();
            PrototypeInteractionStateOwner owner =
                ownerObject.AddComponent<PrototypeInteractionStateOwner>();
            ShowcaseCargoStrapController cargo =
                ownerObject.AddComponent<ShowcaseCargoStrapController>();
            PrototypeInteractionTarget chair = CreateChair();
            CreateFloor();
            cargo.Configure(
                owner,
                null,
                ownerObject.transform,
                playerCollider,
                null,
                new[] { chair },
                null);
            Physics.SyncTransforms();

            Assert.That(cargo.TryBeginDirectAttach(chair), Is.True);
            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Prepare));
            Assert.That(owner.StrapState, Is.EqualTo(PrototypeStrapOwnershipState.Stored));
            Assert.That(owner.StrapTarget, Is.Null);

            uint prepareRevision = cargo.AnimationCueRevision;
            Assert.That(cargo.NotifyAnimationStateEntered(prepareRevision), Is.True);
            Assert.That(cargo.CompleteAnimationStrapAction(prepareRevision), Is.True);
            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Throw));
            Assert.That(owner.StrapState, Is.EqualTo(PrototypeStrapOwnershipState.Stored));

            uint throwRevision = cargo.AnimationCueRevision;
            Assert.That(cargo.NotifyAnimationStateEntered(throwRevision), Is.True);
            Assert.That(cargo.CommitAnimationStrapThrowRelease(throwRevision), Is.True);
            Assert.That(cargo.CommitAnimationStrapAttachContact(throwRevision), Is.True);
            Assert.That(owner.StrapState, Is.EqualTo(PrototypeStrapOwnershipState.Attached));
            Assert.That(owner.StrapTarget, Is.SameAs(chair));
            Assert.That(owner.WeaponSelection, Is.EqualTo(PrototypeWeaponSelection.Fists));
            Assert.That(cargo.CompleteAnimationStrapAction(throwRevision), Is.True);
            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Pull));

            uint pullRevision = cargo.AnimationCueRevision;
            Assert.That(cargo.NotifyAnimationStateEntered(pullRevision), Is.True);
            Assert.That(cargo.CommitAnimationStrapPullComplete(pullRevision), Is.True);
            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Tensioned));
            Assert.That(owner.StrapState, Is.EqualTo(PrototypeStrapOwnershipState.Tensioned));
            Assert.That(owner.WeaponSelection,
                Is.EqualTo(PrototypeWeaponSelection.ObjectWeapon));
            Assert.That(owner.ObjectWeaponTarget, Is.SameAs(chair));

            Assert.That(cargo.TryExplicitRelease(), Is.True);
            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Releasing));
            Assert.That(owner.StrapState,
                Is.EqualTo(PrototypeStrapOwnershipState.ReleaseRequested));
            Assert.That(owner.StrapTarget, Is.SameAs(chair));
        }

        [Test]
        public void SmallRequiresExplicitClampAndStrapHitsShareLightMediumUseBudget()
        {
            PrototypeInteractionTarget binder = CreateTarget(
                "S5Binder",
                PrototypeWeightClass.Light,
                PrototypeInteractionRole.DirectBinder,
                hasValidAnchor: false);
            Assert.That(binder.IsCargoStrapEligible, Is.False);

            Transform clampSocket = CreateObject("S5BinderClamp").transform;
            clampSocket.SetParent(binder.transform, false);
            binder.ConfigureStrapClampSocket(clampSocket);

            PrototypeInteractionTarget chair = CreateTarget(
                "S5Medium",
                PrototypeWeightClass.Medium,
                PrototypeInteractionRole.DirectChair,
                hasValidAnchor: true);
            Assert.That(binder.IsCargoStrapEligible, Is.True);
            Assert.That(binder.MaximumPlayerUses, Is.EqualTo(1));
            Assert.That(chair.MaximumPlayerUses, Is.EqualTo(2));

            string source = File.ReadAllText(
                "Assets/_Project/Code/Runtime/Player/PrototypeInteractionTarget.cs");
            StringAssert.Contains("TryCommitSuccessfulPlayerThrow", source);
            StringAssert.Contains("TryCommitSuccessfulCargoStrapCollision", source);
            StringAssert.Contains("return CommitSharedPlayerUse(false)", source);
            StringAssert.Contains("return CommitSharedPlayerUse(true)", source);
        }

        [Test]
        public void S5Runtime_UsesBoundedGroundedRedirectAndNoRopeSimulation()
        {
            string source = File.ReadAllText(
                "Assets/_Project/Code/Runtime/Player/ShowcaseCargoStrapController.cs");

            StringAssert.Contains("IsFloorSupported", source);
            StringAssert.Contains("maximumStrapLength", source);
            StringAssert.Contains("TryExplicitRelease", source);
            StringAssert.Contains("IsMediumAttackMobilityPhase", source);
            StringAssert.Contains("SetJumpLock", source);
            StringAssert.Contains("SetFacingOverride", source);
            StringAssert.Contains("Physics.BoxCastNonAlloc", source);
            StringAssert.Contains("Physics.OverlapBoxNonAlloc", source);
            StringAssert.Contains("smallSpinBodyClearance", source);
            StringAssert.Contains("ReleaseDuration = 0.2f", source);
            StringAssert.Contains("RigidbodyConstraints.FreezeRotationX", source);
            StringAssert.Contains("RigidbodyConstraints.FreezeRotationZ", source);
            StringAssert.DoesNotContain("Joint", source);
            StringAssert.DoesNotContain("Spring", source);
            StringAssert.DoesNotContain("Grapple", source);
        }

        private PrototypeInteractionTarget CreateChair()
        {
            PrototypeInteractionTarget target = CreateTarget(
                "S5Chair",
                PrototypeWeightClass.Medium,
                PrototypeInteractionRole.DirectChair,
                hasValidAnchor: true);
            target.transform.position = new Vector3(0f, 0.25f, 1f);
            target.gameObject.AddComponent<BoxCollider>().size =
                new Vector3(0.5f, 0.5f, 0.5f);
            Rigidbody body = target.gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            return target;
        }

        private PrototypeInteractionTarget CreateTarget(
            string name,
            PrototypeWeightClass weight,
            PrototypeInteractionRole role,
            bool hasValidAnchor)
        {
            GameObject targetObject = CreateObject(name);
            PrototypeInteractionTarget target =
                targetObject.AddComponent<PrototypeInteractionTarget>();
            target.Configure(
                $"DEF-{name}",
                $"INSTANCE-{name}",
                name,
                weight,
                role,
                true,
                false,
                true,
                hasValidAnchor);
            return target;
        }

        private void CreateFloor()
        {
            GameObject floor = CreateObject("S5Floor");
            floor.transform.position = new Vector3(0f, -0.1f, 0f);
            BoxCollider collider = floor.AddComponent<BoxCollider>();
            collider.size = new Vector3(10f, 0.2f, 10f);
        }

        private GameObject CreateObject(string name)
        {
            GameObject createdObject = new(name);
            createdObjects.Add(createdObject);
            return createdObject;
        }
    }
}
