using System.Collections;
using System.Collections.Generic;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestRunnerPressureBudgetPlayModeTests
    {
        private readonly List<GameObject> createdObjects = new();

        [UnityTearDown]
        public IEnumerator TearDownObjects()
        {
            for (int index = createdObjects.Count - 1; index >= 0; index--)
            {
                if (createdObjects[index] != null)
                {
                    Object.Destroy(createdObjects[index]);
                }
            }
            createdObjects.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator DeniedRunner_PhysicallyFlanksInsteadOfWaitingStill()
        {
            GameObject player = CreateObject("PRESSURE_PLAYER");
            PrototypeInteractionStateOwner owner =
                player.AddComponent<PrototypeInteractionStateOwner>();
            ShowcaseRunnerPressureController pressure =
                CreateObject("PRESSURE_BUDGET")
                    .AddComponent<ShowcaseRunnerPressureController>();
            ShowcaseRunnerAgent attacker = CreateRunner(
                "ATTACKER",
                pressure,
                player.transform,
                Vector3.forward * 0.81f);
            ShowcaseRunnerAgent flanker = CreateRunner(
                "FLANKER",
                pressure,
                player.transform,
                Vector3.back * 0.81f);
            pressure.Configure(
                owner,
                player.transform,
                null,
                null,
                null,
                null,
                null,
                new[] { attacker, flanker });

            Assert.That(pressure.TryAcquireAttackToken(attacker), Is.True);
            Vector3 start = flanker.transform.position;
            for (int index = 0; index < 8; index++)
            {
                yield return new WaitForFixedUpdate();
            }

            float planarTravel = Vector3.ProjectOnPlane(
                flanker.transform.position - start,
                Vector3.up).magnitude;
            Assert.That(pressure.ActiveAttackTokenCount, Is.EqualTo(1));
            Assert.That(flanker.IsMaintainingPressureRing, Is.True);
            Assert.That(flanker.PressureTravelDistance, Is.GreaterThan(0.02f));
            Assert.That(planarTravel, Is.GreaterThan(0.02f));
            Assert.That(Vector3.Distance(
                flanker.LastPressureTarget,
                player.transform.position), Is.GreaterThanOrEqualTo(0.9f),
                "The denied runner must keep a pressure ring outside player overlap.");
        }

        private ShowcaseRunnerAgent CreateRunner(
            string id,
            ShowcaseRunnerPressureController pressure,
            Transform player,
            Vector3 position)
        {
            GameObject runnerObject = CreateObject(id);
            runnerObject.transform.position = position;
            CapsuleCollider collider = runnerObject.AddComponent<CapsuleCollider>();
            collider.radius = 0.32f;
            collider.height = 1.8f;
            Rigidbody body = runnerObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            ShowcaseRunnerAgent runner =
                runnerObject.AddComponent<ShowcaseRunnerAgent>();
            runner.Configure(id, pressure, body, player);
            return runner;
        }

        private GameObject CreateObject(string name)
        {
            var created = new GameObject(name);
            createdObjects.Add(created);
            return created;
        }
    }
}
