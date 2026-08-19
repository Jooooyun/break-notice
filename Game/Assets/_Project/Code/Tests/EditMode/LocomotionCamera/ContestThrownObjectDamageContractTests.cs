using System.Collections.Generic;
using System.Reflection;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestThrownObjectDamageContractTests
    {
        private readonly List<GameObject> createdObjects = new();

        [TearDown]
        public void TearDown()
        {
            for (int index = createdObjects.Count - 1; index >= 0; index--)
            {
                Object.DestroyImmediate(createdObjects[index]);
            }
            createdObjects.Clear();
        }

        [TestCase(PrototypeWeightClass.Light, 20)]
        [TestCase(PrototypeWeightClass.Medium, 30)]
        public void PlayerThrownObject_UsesHealthApprovalAndDeduplicatesOneThrow(
            PrototypeWeightClass weightClass,
            int expectedDamage)
        {
            GameObject player = CreateObject("PLAYER");
            PrototypeInteractionStateOwner owner =
                player.AddComponent<PrototypeInteractionStateOwner>();
            ShowcaseRunnerPressureController pressure =
                player.AddComponent<ShowcaseRunnerPressureController>();
            pressure.Configure(
                owner,
                player.transform,
                null,
                null,
                null,
                null,
                null,
                null);

            GameObject runnerObject = CreateObject("RUNNER");
            runnerObject.transform.position = Vector3.forward * 2f;
            runnerObject.AddComponent<CapsuleCollider>();
            Rigidbody runnerBody = runnerObject.AddComponent<Rigidbody>();
            runnerBody.isKinematic = true;
            ShowcaseCombatHealth health =
                runnerObject.AddComponent<ShowcaseCombatHealth>();
            health.Configure(ShowcaseCombatTeam.Enemy, 100, 0f);
            ShowcaseRunnerAgent runner =
                runnerObject.AddComponent<ShowcaseRunnerAgent>();
            runner.Configure(
                "RUNNER",
                pressure,
                runnerBody,
                player.transform,
                authoredHealth: health);

            GameObject sourceObject = CreateObject("THROWN_OBJECT");
            PrototypeInteractionTarget source =
                sourceObject.AddComponent<PrototypeInteractionTarget>();
            source.Configure(
                "THROWN-DEF",
                "THROWN-INSTANCE",
                "Thrown Object",
                weightClass,
                weightClass == PrototypeWeightClass.Light
                    ? PrototypeInteractionRole.DirectBinder
                    : PrototypeInteractionRole.DirectChair,
                true,
                false,
                false,
                false);
            CommitPlayerThrow(source, 1u);

            int contactCount = 0;
            bool lastAccepted = false;
            pressure.CombatContactResolved += result =>
            {
                contactCount++;
                lastAccepted = result.Accepted;
            };

            pressure.RegisterPhysicalImpact(runner, source, 6f);

            Assert.That(health.CurrentHealth, Is.EqualTo(100 - expectedDamage));
            Assert.That(health.AcceptedDamageCount, Is.EqualTo(1));
            Assert.That(pressure.PhysicalImpactCount, Is.EqualTo(1));
            Assert.That(contactCount, Is.EqualTo(1));
            Assert.That(lastAccepted, Is.True);

            pressure.RegisterPhysicalImpact(runner, source, 6f);

            Assert.That(health.CurrentHealth, Is.EqualTo(100 - expectedDamage));
            Assert.That(health.AcceptedDamageCount, Is.EqualTo(1));
            Assert.That(pressure.PhysicalImpactCount, Is.EqualTo(1));
            Assert.That(contactCount, Is.EqualTo(2));
            Assert.That(lastAccepted, Is.False);
        }

        private static void CommitPlayerThrow(
            PrototypeInteractionTarget target,
            uint sequence)
        {
            MethodInfo commit = typeof(PrototypeInteractionTarget).GetMethod(
                "TryCommitSuccessfulPlayerThrow",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(commit, Is.Not.Null);
            Assert.That(commit.Invoke(target, new object[] { sequence }), Is.True);
        }

        private GameObject CreateObject(string name)
        {
            var created = new GameObject(name);
            createdObjects.Add(created);
            return created;
        }
    }
}
