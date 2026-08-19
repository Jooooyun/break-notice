using System.Collections;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestContextualWeaponSelectionPlayModeTests
    {
        [UnityTest]
        public IEnumerator Carry_SelectFistsDropsWithoutThrowOrSilentFallback()
        {
            GameObject player = new("ContextualCarryPlayer");
            GameObject targetObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                player.AddComponent<CharacterController>();
                PrototypeInteractionStateOwner owner =
                    player.AddComponent<PrototypeInteractionStateOwner>();
                PrototypePhysicalObjectLifecycle lifecycle =
                    player.AddComponent<PrototypePhysicalObjectLifecycle>();
                PrototypeInteractionController interaction =
                    player.AddComponent<PrototypeInteractionController>();
                Transform anchor = new GameObject("CarryAnchor").transform;
                anchor.SetParent(player.transform, false);
                anchor.localPosition = new Vector3(0f, 1f, 1.1f);
                lifecycle.Configure(owner, anchor);

                Rigidbody body = targetObject.AddComponent<Rigidbody>();
                body.useGravity = false;
                PrototypeInteractionTarget target =
                    targetObject.AddComponent<PrototypeInteractionTarget>();
                target.Configure(
                    "OBJ-P-L-001",
                    "CONTEXTUAL-LIGHT-01",
                    "Contextual Light",
                    PrototypeWeightClass.Light,
                    PrototypeInteractionRole.DirectBinder,
                    true,
                    false,
                    false,
                    false);
                interaction.Configure(
                    null,
                    owner,
                    null,
                    new[] { target },
                    2.8f,
                    false,
                    lifecycle);

                Assert.That(owner.TryCommit(PrototypeInteractionDecision.Accept(
                    PrototypeInteractionOutcome.Scoop, target)), Is.True);
                Assert.That(owner.TryCompleteAcquisition(target), Is.True);
                Assert.That(lifecycle.BeginCarry(target), Is.True);
                Assert.That(owner.WeaponSelection,
                    Is.EqualTo(PrototypeWeaponSelection.ObjectWeapon));

                Assert.That(interaction.ProcessWeaponSelection(true, true), Is.False);
                Assert.That(owner.CarriedTarget, Is.SameAs(target));
                Assert.That(interaction.ProcessWeaponSelection(true, false), Is.True);

                Assert.That(owner.WeaponSelection,
                    Is.EqualTo(PrototypeWeaponSelection.Fists));
                Assert.That(owner.HandState,
                    Is.EqualTo(PrototypeHandOwnershipState.Free));
                Assert.That(owner.CarriedTarget, Is.Null);
                Assert.That(lifecycle.CarriedTarget, Is.Null);
                Assert.That(body.isKinematic, Is.False);
                Assert.That(body.useGravity, Is.True);
                Assert.That(body.linearVelocity, Is.EqualTo(Vector3.zero));
                Assert.That(lifecycle.ThrowOriginCount, Is.Zero);
                Assert.That(target.CommittedPlayerThrowUses, Is.Zero);
                Assert.That(interaction.ProcessAttackPressed(), Is.False);
                Assert.That(interaction.HasThrowCharge, Is.False);
                Assert.That(interaction.ProcessWeaponSelection(false, true), Is.False);
                Assert.That(owner.WeaponSelection,
                    Is.EqualTo(PrototypeWeaponSelection.Fists));

                body.position = player.transform.position + Vector3.forward * 4f;
                Physics.SyncTransforms();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
            }
            finally
            {
                Object.Destroy(player);
                Object.Destroy(targetObject);
            }

            yield return null;
        }
    }
}
