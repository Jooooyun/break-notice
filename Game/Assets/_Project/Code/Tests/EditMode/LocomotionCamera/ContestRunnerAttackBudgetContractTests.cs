using System.Collections.Generic;
using System.Reflection;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestRunnerAttackBudgetContractTests
    {
        private readonly List<GameObject> createdObjects = new();
        private ShowcaseRunnerPressureController pressure;
        private PrototypeInteractionStateOwner owner;

        [SetUp]
        public void SetUp()
        {
            GameObject ownerObject = CreateObject("ATTACK_BUDGET_PLAYER");
            owner = ownerObject.AddComponent<PrototypeInteractionStateOwner>();
            pressure = CreateObject("ATTACK_BUDGET_OWNER")
                .AddComponent<ShowcaseRunnerPressureController>();
            pressure.Configure(
                owner,
                ownerObject.transform,
                null,
                null,
                null,
                null,
                null,
                null);
        }

        [TearDown]
        public void TearDown()
        {
            for (int index = createdObjects.Count - 1; index >= 0; index--)
            {
                Object.DestroyImmediate(createdObjects[index]);
            }
            createdObjects.Clear();
        }

        [Test]
        public void BudgetFormula_UsesOneTwoThreeNormalSlotsByEnemyCount()
        {
            Assert.That(pressure.ResolveBaseNormalAttackBudget(1), Is.EqualTo(1));
            Assert.That(pressure.ResolveBaseNormalAttackBudget(2), Is.EqualTo(1));
            Assert.That(pressure.ResolveBaseNormalAttackBudget(3), Is.EqualTo(2));
            Assert.That(pressure.ResolveBaseNormalAttackBudget(4), Is.EqualTo(2));
            Assert.That(pressure.ResolveBaseNormalAttackBudget(5), Is.EqualTo(3));
            Assert.That(pressure.ResolveBaseNormalAttackBudget(9), Is.EqualTo(3));
        }

        [Test]
        public void DifficultyRamp_OpensOneThenTwoThenNormalTwoPlusElite()
        {
            ShowcaseRunnerAgent[] runners = CreateThreeNormalOneElite();

            ShowcaseAttackBudgetSnapshot opening =
                pressure.EvaluateAttackBudget(0f);
            ShowcaseAttackBudgetSnapshot middle =
                pressure.EvaluateAttackBudget(13f);
            ShowcaseAttackBudgetSnapshot full =
                pressure.EvaluateAttackBudget(25f);

            Assert.That(opening.EnemyCount, Is.EqualTo(runners.Length));
            Assert.That(opening.TotalCapacity, Is.EqualTo(1));
            Assert.That(opening.NormalCapacity, Is.EqualTo(1));
            Assert.That(middle.TotalCapacity, Is.EqualTo(2));
            Assert.That(middle.NormalCapacity, Is.EqualTo(2));
            Assert.That(full.TotalCapacity, Is.EqualTo(3));
            Assert.That(full.NormalCapacity, Is.EqualTo(2));
            Assert.That(full.EliteCapacity, Is.EqualTo(1));
            Assert.That(full.ElitePressureActive, Is.True);
        }

        [Test]
        public void Slots_StartGapAndActorCooldown_BlockBudgetViolations()
        {
            ShowcaseRunnerAgent[] runners = CreateThreeNormalOneElite();
            ShowcaseRunnerAgent normalOne = runners[0];
            ShowcaseRunnerAgent normalTwo = runners[1];
            ShowcaseRunnerAgent normalThree = runners[2];
            ShowcaseRunnerAgent elite = runners[3];
            float start = pressure.PressureStartedAt + 25f;

            Assert.That(
                pressure.TryAcquireAttackToken(normalOne, start),
                Is.True);
            Assert.That(
                pressure.TryAcquireAttackToken(normalTwo, start + 0.14f),
                Is.False,
                "The global 0.15-0.30 second start gap must reject a near-simultaneous start.");
            Assert.That(
                pressure.TryAcquireAttackToken(normalTwo, start + 0.31f),
                Is.True);
            Assert.That(
                pressure.TryAcquireAttackToken(normalThree, start + 0.62f),
                Is.False,
                "Four total enemies permit only two simultaneous Normal attackers.");
            Assert.That(
                pressure.TryAcquireAttackToken(elite, start + 0.62f),
                Is.True,
                "The Elite must use its separate slot after the same global start gap.");
            Assert.That(pressure.ActiveNormalAttackCount, Is.EqualTo(2));
            Assert.That(pressure.ActiveEliteAttackCount, Is.EqualTo(1));
            Assert.That(pressure.ActiveAttackTokenCount, Is.EqualTo(3));

            pressure.ReleaseAttackToken(normalOne, start + 0.7f);
            Assert.That(
                pressure.TryAcquireAttackToken(normalOne, start + 1.0f),
                Is.False,
                "A released actor must observe its deterministic 0.8-1.4 second cooldown.");
            Assert.That(
                pressure.TryAcquireAttackToken(normalOne, start + 2.2f),
                Is.True);
        }

        [Test]
        public void TelegraphVariation_IsDeterministicAndInsideFairnessRanges()
        {
            ShowcaseRunnerAgent[] runners = CreateThreeNormalOneElite();
            float normalFirst = pressure.ResolveTelegraphDuration(runners[0], 7);
            float normalRepeat = pressure.ResolveTelegraphDuration(runners[0], 7);
            float elite = pressure.ResolveTelegraphDuration(runners[3], 7);

            Assert.That(normalFirst, Is.EqualTo(normalRepeat).Within(0.000001f));
            Assert.That(normalFirst, Is.InRange(0.20f, 0.35f));
            Assert.That(elite, Is.InRange(0.35f, 0.55f));
        }

        [Test]
        public void PressureRing_GivesDifferentPhysicalTargetsAndResetCleansSlots()
        {
            ShowcaseRunnerAgent[] runners = CreateThreeNormalOneElite();
            Assert.That(pressure.TryResolvePressurePosition(
                runners[0], out Vector3 first, out float firstSpeed), Is.True);
            Assert.That(pressure.TryResolvePressurePosition(
                runners[1], out Vector3 second, out float secondSpeed), Is.True);
            Assert.That(Vector3.Distance(first, second), Is.GreaterThan(0.2f));
            Assert.That(firstSpeed, Is.InRange(0.5f, 1.2f));
            Assert.That(secondSpeed, Is.InRange(0.5f, 1.2f));

            float start = pressure.PressureStartedAt + 25f;
            Assert.That(pressure.TryAcquireAttackToken(runners[0], start), Is.True);
            Assert.That(pressure.TryAcquireAttackToken(runners[3], start + 0.31f), Is.True);
            pressure.ResetToRunBaseline();

            Assert.That(pressure.ActiveAttackTokenCount, Is.Zero);
            Assert.That(pressure.ActiveNormalAttackCount, Is.Zero);
            Assert.That(pressure.ActiveEliteAttackCount, Is.Zero);
            Assert.That(pressure.AttackTokenOwner, Is.Null);
            Assert.That(
                pressure.TryAcquireAttackToken(
                    runners[1],
                    pressure.PressureStartedAt),
                Is.True,
                "Reset must clear global and per-actor cadence ownership.");
        }

        [Test]
        public void OffScreenWarning_ExistsOnlyDuringLiveTelegraph()
        {
            ShowcaseRunnerAgent runner = CreateRunner(
                "OFFSCREEN NORMAL",
                false,
                new Vector3(0f, 0f, -3f));
            pressure.Configure(
                owner,
                owner.transform,
                null,
                null,
                null,
                null,
                null,
                new[] { runner });

            Camera camera = CreateObject("OFFSCREEN CAMERA").AddComponent<Camera>();
            camera.transform.SetPositionAndRotation(
                Vector3.zero,
                Quaternion.identity);
            ShowcaseRunnerPressurePresenter presenter =
                CreateObject("OFFSCREEN PRESENTER")
                    .AddComponent<ShowcaseRunnerPressurePresenter>();
            presenter.Configure(pressure);

            MethodInfo commit = typeof(ShowcaseRunnerAgent).GetMethod(
                "CommitNextAttack",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(commit, Is.Not.Null);
            commit.Invoke(runner, null);
            Assert.That(runner.IsAttackWindup, Is.True);
            Assert.That(presenter.TryGetOffScreenAttackWarning(
                camera,
                runner,
                out ShowcaseOffScreenAttackWarning warning), Is.True);
            Assert.That(warning.Runner, Is.SameAs(runner));
            Assert.That(warning.ViewportEdge.x, Is.InRange(0.08f, 0.92f));
            Assert.That(warning.ViewportEdge.y, Is.InRange(0.08f, 0.92f));

            runner.ReceivePhysicalStagger(false);
            Assert.That(runner.IsAttackWindup, Is.False);
            Assert.That(presenter.TryGetOffScreenAttackWarning(
                camera,
                runner,
                out _), Is.False,
                "A cancelled telegraph must remove its edge warning immediately.");
        }

        private ShowcaseRunnerAgent[] CreateThreeNormalOneElite()
        {
            ShowcaseRunnerAgent[] runners =
            {
                CreateRunner("NORMAL 1", false, new Vector3(-1f, 0f, 1f)),
                CreateRunner("NORMAL 2", false, new Vector3(1f, 0f, 1f)),
                CreateRunner("NORMAL 3", false, new Vector3(-1f, 0f, -1f)),
                CreateRunner("ELITE 1", true, new Vector3(1f, 0f, -1f))
            };
            pressure.Configure(
                owner,
                owner.transform,
                null,
                null,
                null,
                null,
                null,
                runners);
            return runners;
        }

        private ShowcaseRunnerAgent CreateRunner(
            string id,
            bool elite,
            Vector3 position)
        {
            GameObject runnerObject = CreateObject(id);
            runnerObject.transform.position = position;
            runnerObject.AddComponent<CapsuleCollider>();
            Rigidbody body = runnerObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            ShowcaseRunnerAgent runner =
                runnerObject.AddComponent<ShowcaseRunnerAgent>();
            runner.Configure(
                id,
                pressure,
                body,
                owner.transform,
                authoredElite: elite);
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
