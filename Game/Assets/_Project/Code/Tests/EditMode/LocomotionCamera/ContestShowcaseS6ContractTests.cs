using System.IO;
using System.Linq;
using System.Reflection;
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
        public void RunnerAttackSelector_UsesTwoOrThreePunchesPerKickAndEliteTechniques()
        {
            ShowcaseRunnerPressureController pressure = CreatePressure();
            ShowcaseRunnerAgent normal = CreateRunner(
                "NORMAL",
                ref runnerOneObject,
                pressure);
            ShowcaseRunnerAgent elite = CreateRunner(
                "ELITE",
                ref runnerTwoObject,
                pressure,
                true);

            ShowcaseRunnerAnimationCue[] normalCues = CommitAttacks(normal, 64);
            AssertPunchKickCadence(normalCues, false);

            ShowcaseRunnerAnimationCue[] eliteCues = CommitAttacks(elite, 64);
            AssertPunchKickCadence(eliteCues, true);
            Assert.That(eliteCues,
                Does.Contain(ShowcaseRunnerAnimationCue.EliteMutantPunch));
            Assert.That(eliteCues,
                Does.Contain(ShowcaseRunnerAnimationCue.EliteMutantSwiping));
            elite.ResetToRunBaseline();
            Assert.That(CommitAttacks(elite, 64), Is.EqualTo(eliteCues),
                "Authored Reset must restart the elite selector deterministically.");
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
            StringAssert.Contains("attackRandomState", source);
            StringAssert.Contains("punchesUntilKick", source);
            StringAssert.Contains("ResolvePunchCue", source);
            StringAssert.Contains("ResolveKickCue", source);
            StringAssert.Contains("ShowcaseRunnerAnimationCue.EliteMutantSwiping", source);
            StringAssert.Contains("ShowcaseRunnerAnimationCue.EliteMutantPunch", source);
            StringAssert.DoesNotContain("return ShowcaseRunnerAnimationCue.EliteJumpAttack", source);
            StringAssert.DoesNotContain("EliteJumpLunge", source);
            StringAssert.DoesNotContain("lungeWindowOpen", source);
            StringAssert.DoesNotContain("lungeTravelDistance", source);
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
            ShowcaseRunnerPressureController pressure,
            bool elite = false)
        {
            runnerObject = new GameObject(id);
            runnerObject.AddComponent<CapsuleCollider>();
            Rigidbody body = runnerObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            ShowcaseRunnerAgent runner = runnerObject.AddComponent<ShowcaseRunnerAgent>();
            runner.Configure(
                id,
                pressure,
                body,
                pressure.transform,
                authoredElite: elite);
            return runner;
        }

        private static ShowcaseRunnerAnimationCue[] CommitAttacks(
            ShowcaseRunnerAgent runner,
            int count)
        {
            MethodInfo commit = typeof(ShowcaseRunnerAgent).GetMethod(
                "CommitNextAttack",
                BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo commitPrepared = typeof(ShowcaseRunnerAgent).GetMethod(
                "CommitPreparedAttack",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(commit, Is.Not.Null);
            Assert.That(commitPrepared, Is.Not.Null);

            ShowcaseRunnerAnimationCue[] cues = new ShowcaseRunnerAnimationCue[count];
            for (int index = 0; index < cues.Length; index++)
            {
                commit.Invoke(runner, null);
                cues[index] = runner.AnimationCue;
                commitPrepared.Invoke(runner, null);
                runner.CompleteAnimationAction();
            }
            return cues;
        }

        private static void AssertPunchKickCadence(
            ShowcaseRunnerAnimationCue[] cues,
            bool elite)
        {
            int punches = 0;
            int kicks = 0;
            foreach (ShowcaseRunnerAnimationCue cue in cues)
            {
                bool kick = cue == ShowcaseRunnerAnimationCue.Kicking ||
                    cue == ShowcaseRunnerAnimationCue.Kicking2 ||
                    cue == ShowcaseRunnerAnimationCue.Boxing2;
                if (kick)
                {
                    Assert.That(punches, Is.InRange(2, 3),
                        "Every kick must follow exactly two or three punch actions.");
                    punches = 0;
                    kicks++;
                    continue;
                }

                if (elite)
                {
                    Assert.That(new[]
                    {
                        ShowcaseRunnerAnimationCue.Boxing,
                        ShowcaseRunnerAnimationCue.EliteMutantPunch,
                        ShowcaseRunnerAnimationCue.EliteMutantSwiping
                    }, Does.Contain(cue));
                }
                else
                {
                    Assert.That(cue,
                        Is.EqualTo(ShowcaseRunnerAnimationCue.Boxing));
                }
                punches++;
                Assert.That(punches, Is.LessThanOrEqualTo(3));
            }

            Assert.That(kicks, Is.GreaterThanOrEqualTo(cues.Length / 4 - 1));
            Assert.That(kicks, Is.LessThanOrEqualTo(cues.Length / 3 + 1));
        }
    }
}
