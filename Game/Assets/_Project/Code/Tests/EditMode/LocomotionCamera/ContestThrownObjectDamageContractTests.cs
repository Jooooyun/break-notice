using System.Collections.Generic;
using System.Reflection;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
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

        [TestCase(PrototypeWeightClass.Light, 17)]
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
            sourceObject.AddComponent<BoxCollider>();
            Rigidbody sourceBody = sourceObject.AddComponent<Rigidbody>();
            sourceBody.useGravity = false;
            sourceBody.mass = weightClass == PrototypeWeightClass.Light ? 1f : 8f;
            sourceBody.linearVelocity = Vector3.forward * 6f;
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
            Assert.That(pressure.LastObjectImpactEvaluation.IsValid, Is.True);
            Assert.That(pressure.LastObjectImpactEvaluation.Severity,
                Is.EqualTo(weightClass == PrototypeWeightClass.Light
                    ? ShowcaseObjectImpactSeverity.Light
                    : ShowcaseObjectImpactSeverity.Medium));
            Assert.That(pressure.LastObjectImpactEvaluation.Context.ObjectMass,
                Is.EqualTo(sourceBody.mass).Within(0.001f));
            Assert.That(Vector3.Distance(
                    pressure.LastObjectImpactEvaluation.Context.
                        PrimaryImpactDirection,
                    Vector3.forward),
                Is.LessThan(0.001f));

            pressure.RegisterPhysicalImpact(runner, source, 6f);

            Assert.That(health.CurrentHealth, Is.EqualTo(100 - expectedDamage));
            Assert.That(health.AcceptedDamageCount, Is.EqualTo(1));
            Assert.That(pressure.PhysicalImpactCount, Is.EqualTo(1));
            Assert.That(contactCount, Is.EqualTo(2));
            Assert.That(lastAccepted, Is.False);
        }

        [Test]
        public void ImpactTuning_DifferentiatesBinderMediumAndHeavyChairBands()
        {
            var tuning = new ShowcaseObjectImpactTuning();
            PrototypeInteractionTarget binder = CreateTarget(
                "BINDER",
                PrototypeWeightClass.Light,
                PrototypeInteractionRole.DirectBinder);
            PrototypeInteractionTarget chair = CreateTarget(
                "CHAIR",
                PrototypeWeightClass.Medium,
                PrototypeInteractionRole.DirectChair);

            ShowcaseObjectImpactEvaluation binderImpact = tuning.Evaluate(
                CreateContext(
                    binder,
                    1f,
                    6f,
                    0f,
                    1u),
                20);
            ShowcaseObjectImpactEvaluation chairMediumImpact = tuning.Evaluate(
                CreateContext(
                    chair,
                    8f,
                    8f,
                    0f,
                    2u),
                30);
            ShowcaseObjectImpactEvaluation chairHeavyImpact = tuning.Evaluate(
                CreateContext(
                    chair,
                    8f,
                    12.8f,
                    1f,
                    3u),
                30);

            Assert.That(binderImpact.Severity,
                Is.EqualTo(ShowcaseObjectImpactSeverity.Light));
            Assert.That(binderImpact.CausesKnockdown, Is.False);
            Assert.That(binderImpact.DisplacementDistance,
                Is.InRange(0.15f, 0.35f));
            Assert.That(chairMediumImpact.Severity,
                Is.EqualTo(ShowcaseObjectImpactSeverity.Medium));
            Assert.That(chairMediumImpact.DisplacementDistance,
                Is.InRange(0.4f, 1.6f));
            Assert.That(chairMediumImpact.DisplacementDistance,
                Is.GreaterThan(binderImpact.DisplacementDistance));
            Assert.That(chairHeavyImpact.Severity,
                Is.EqualTo(ShowcaseObjectImpactSeverity.Heavy));
            Assert.That(chairHeavyImpact.CausesKnockdown, Is.True);
            Assert.That(chairHeavyImpact.DisplacementDistance,
                Is.InRange(1.6f, 2.8f));
            Assert.That(chairHeavyImpact.DamageMultiplier,
                Is.InRange(1.35f, 1.5f));
            Assert.That(binderImpact.HitStopSeconds, Is.InRange(0f, 0.02f));
            Assert.That(chairMediumImpact.HitStopSeconds,
                Is.InRange(0.025f, 0.045f));
            Assert.That(chairHeavyImpact.HitStopSeconds,
                Is.InRange(0.04f, 0.07f));

            Assert.That(ShowcaseCombatHitVfxPresenter.ResolveTier(
                    new ShowcaseCombatContactResult(
                        binder.transform,
                        null,
                        true,
                        Vector3.zero,
                        binderImpact)),
                Is.EqualTo(ShowcaseHitVfxTier.Light));
            Assert.That(ShowcaseCombatHitVfxPresenter.ResolveTier(
                    new ShowcaseCombatContactResult(
                        chair.transform,
                        null,
                        true,
                        Vector3.zero,
                        chairMediumImpact)),
                Is.EqualTo(ShowcaseHitVfxTier.Medium));
            Assert.That(ShowcaseCombatHitVfxPresenter.ResolveTier(
                    new ShowcaseCombatContactResult(
                        chair.transform,
                        null,
                        true,
                        Vector3.zero,
                        chairHeavyImpact)),
                Is.EqualTo(ShowcaseHitVfxTier.Heavy));
        }

        private ShowcaseObjectImpactContext CreateContext(
            PrototypeInteractionTarget target,
            float mass,
            float speed,
            float charge01,
            uint sequence)
        {
            return new ShowcaseObjectImpactContext(
                target,
                target.WeightClass,
                speed,
                Vector3.forward * speed,
                mass,
                charge01,
                true,
                0.5f * mass * speed * speed,
                ShowcaseObjectImpactAttackOriginType.HandThrow,
                Vector3.forward,
                Vector3.zero,
                sequence);
        }

        private PrototypeInteractionTarget CreateTarget(
            string name,
            PrototypeWeightClass weightClass,
            PrototypeInteractionRole role)
        {
            GameObject sourceObject = CreateObject(name);
            PrototypeInteractionTarget target =
                sourceObject.AddComponent<PrototypeInteractionTarget>();
            target.Configure(
                $"{name}-DEF",
                $"{name}-INSTANCE",
                name,
                weightClass,
                role,
                true,
                false,
                false,
                false);
            return target;
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
