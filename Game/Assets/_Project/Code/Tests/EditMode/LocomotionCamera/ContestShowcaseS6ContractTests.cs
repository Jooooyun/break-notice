using System.IO;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestShowcaseS6ContractTests
    {
        private GameObject pressureObject;
        private GameObject ownerObject;
        private GameObject runnerOneObject;
        private GameObject runnerTwoObject;

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(runnerTwoObject);
            Object.DestroyImmediate(runnerOneObject);
            Object.DestroyImmediate(ownerObject);
            Object.DestroyImmediate(pressureObject);
        }

        [Test]
        public void AttackToken_IsSingularAndTransfersOnlyAfterRelease()
        {
            ShowcaseRunnerPressureController pressure = CreatePressure();
            ShowcaseRunnerAgent first = CreateRunner("RUNNER 1", ref runnerOneObject, pressure);
            ShowcaseRunnerAgent second = CreateRunner("RUNNER 2", ref runnerTwoObject, pressure);

            Assert.That(pressure.TryAcquireAttackToken(first), Is.True);
            Assert.That(pressure.TryAcquireAttackToken(second), Is.False);
            Assert.That(pressure.ActiveAttackTokenCount, Is.EqualTo(1));
            Assert.That(pressure.AttackTokenOwner, Is.SameAs(first));
            pressure.ReleaseAttackToken(first);
            Assert.That(pressure.TryAcquireAttackToken(second), Is.False,
                "A released token must observe the authored team attack gap.");
            pressure.ResetToRunBaseline();
            Assert.That(pressure.TryAcquireAttackToken(second), Is.True,
                "Authored Reset clears the team attack gap deterministically.");
            Assert.That(pressure.AttackTokenOwner, Is.SameAs(second));
        }

        [Test]
        public void RunnerStagger_ExposesCombatTagAndTagExtendsToMediumStagger()
        {
            ShowcaseRunnerPressureController pressure = CreatePressure();
            ShowcaseRunnerAgent runner = CreateRunner("RUNNER 1", ref runnerOneObject, pressure);

            runner.ReceivePhysicalStagger(false);
            Assert.That(runner.State, Is.EqualTo(ShowcaseRunnerState.OffBalance));
            Assert.That(runner.CanReceiveCombatTag, Is.True);
            runner.ReceiveCombatTag();
            Assert.That(runner.State, Is.EqualTo(ShowcaseRunnerState.Staggered));
        }

        [Test]
        public void S6Source_ContainsRequiredPressureAndNoNewEnemyArchetype()
        {
            string source = File.ReadAllText(
                    "Assets/_Project/Code/Runtime/Player/ShowcaseRunnerPressureController.cs") +
                File.ReadAllText(
                    "Assets/_Project/Code/Runtime/Player/ShowcaseRunnerAgent.cs");
            StringAssert.Contains("ShowcaseRunnerAttackKind.BodyCheck", source);
            StringAssert.Contains("ShowcaseRunnerAttackKind.Shove", source);
            StringAssert.Contains("TryCommitShortGrab", source);
            StringAssert.Contains("TryRescuePlayer", source);
            StringAssert.Contains("TryCombatTag", source);
            StringAssert.Contains("AttackTokenOwner", source);
            StringAssert.DoesNotContain("Boss", source);
            StringAssert.DoesNotContain("NavMesh", source);
        }

        private ShowcaseRunnerPressureController CreatePressure()
        {
            pressureObject = new GameObject("S6Pressure");
            ownerObject = new GameObject("S6Owner");
            PrototypeInteractionStateOwner owner =
                ownerObject.AddComponent<PrototypeInteractionStateOwner>();
            ShowcaseRunnerPressureController pressure =
                pressureObject.AddComponent<ShowcaseRunnerPressureController>();
            pressure.Configure(owner, ownerObject.transform, null, null, null, null, null, null);
            return pressure;
        }

        private static ShowcaseRunnerAgent CreateRunner(
            string id,
            ref GameObject runnerObject,
            ShowcaseRunnerPressureController pressure)
        {
            runnerObject = new GameObject(id);
            runnerObject.AddComponent<CapsuleCollider>();
            Rigidbody body = runnerObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            ShowcaseRunnerAgent runner = runnerObject.AddComponent<ShowcaseRunnerAgent>();
            runner.Configure(id, pressure, body, pressure.transform);
            return runner;
        }
    }
}
