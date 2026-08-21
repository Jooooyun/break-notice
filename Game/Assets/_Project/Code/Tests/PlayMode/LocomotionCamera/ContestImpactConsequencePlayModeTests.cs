using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestImpactConsequencePlayModeTests
    {
        private sealed class Fixture
        {
            public PrototypeInteractionStateOwner Owner;
            public ShowcaseRunnerPressureController Pressure;
            public ShowcaseRunnerAgent[] Runners;
        }

        private readonly List<GameObject> createdObjects = new();
        private uint nextThrowSequence = 1000;

        [UnityTearDown]
        public IEnumerator TearDown()
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
        public IEnumerator BinderFlinch_AndChairMedium_UseDistinctControlledDistances()
        {
            Fixture fixture = CreateFixture(2);
            ShowcaseRunnerAgent binderVictim = fixture.Runners[0];
            ShowcaseRunnerAgent chairVictim = fixture.Runners[1];
            Vector3 binderStart = binderVictim.transform.position;
            Vector3 chairStart = chairVictim.transform.position;
            PrototypeInteractionTarget binder = CreateThrownObject(
                PrototypeWeightClass.Light,
                PrototypeInteractionRole.DirectBinder,
                1f,
                Vector3.right * 6f,
                0f,
                ShowcaseObjectImpactAttackOriginType.HandThrow);
            PrototypeInteractionTarget chair = CreateThrownObject(
                PrototypeWeightClass.Medium,
                PrototypeInteractionRole.DirectChair,
                8f,
                Vector3.right * 8f,
                0f,
                ShowcaseObjectImpactAttackOriginType.HandThrow);

            RegisterImpact(fixture, binderVictim, binder, Vector3.right * 6f);
            ShowcaseObjectImpactEvaluation binderEvaluation =
                fixture.Pressure.LastObjectImpactEvaluation;
            AcknowledgeCurrentAnimation(binderVictim);
            RegisterImpact(fixture, chairVictim, chair, Vector3.right * 8f);
            ShowcaseObjectImpactEvaluation chairEvaluation =
                fixture.Pressure.LastObjectImpactEvaluation;
            AcknowledgeCurrentAnimation(chairVictim);

            Assert.That(binderEvaluation.Severity,
                Is.EqualTo(ShowcaseObjectImpactSeverity.Light));
            Assert.That(binderVictim.State,
                Is.EqualTo(ShowcaseRunnerState.OffBalance));
            Assert.That(chairEvaluation.Severity,
                Is.EqualTo(ShowcaseObjectImpactSeverity.Medium));
            Assert.That(chairVictim.State,
                Is.EqualTo(ShowcaseRunnerState.Staggered));

            yield return WaitForReactionToFinish(binderVictim);
            yield return WaitForReactionToFinish(chairVictim);

            float binderDistance = PlanarDistance(
                binderVictim.transform.position,
                binderStart);
            float chairDistance = PlanarDistance(
                chairVictim.transform.position,
                chairStart);
            Assert.That(binderDistance, Is.InRange(0.15f, 0.36f));
            Assert.That(chairDistance, Is.InRange(0.4f, 1.61f));
            Assert.That(chairDistance, Is.GreaterThan(binderDistance));
            Assert.That(binderVictim.State,
                Is.Not.EqualTo(ShowcaseRunnerState.KnockedDown));
        }

        [UnityTest]
        public IEnumerator HeavyChair_FollowsObjectVelocity_AndEntersKnockdown()
        {
            Fixture fixture = CreateFixture(1);
            ShowcaseRunnerAgent runner = fixture.Runners[0];
            Vector3 start = runner.transform.position;
            Vector3 velocity = new Vector3(12.8f, 0.5f, 0f);
            PrototypeInteractionTarget chair = CreateThrownObject(
                PrototypeWeightClass.Medium,
                PrototypeInteractionRole.DirectChair,
                8f,
                velocity,
                1f,
                ShowcaseObjectImpactAttackOriginType.HandThrow);

            RegisterImpact(fixture, runner, chair, velocity);
            ShowcaseObjectImpactEvaluation evaluation =
                fixture.Pressure.LastObjectImpactEvaluation;
            AcknowledgeCurrentAnimation(runner);

            Assert.That(evaluation.Severity,
                Is.EqualTo(ShowcaseObjectImpactSeverity.Heavy));
            Assert.That(evaluation.Context.AttackOriginType,
                Is.EqualTo(ShowcaseObjectImpactAttackOriginType.HandThrow));
            Assert.That(evaluation.Context.ThrowCharge01,
                Is.EqualTo(1f).Within(0.001f));
            Assert.That(evaluation.Context.HasLastReleaseKineticEnergy, Is.True);
            Assert.That(evaluation.Context.LastReleaseKineticEnergy,
                Is.GreaterThan(0f));
            Assert.That(runner.State,
                Is.EqualTo(ShowcaseRunnerState.KnockedDown));
            Assert.That(Vector3.Dot(
                    evaluation.Context.PrimaryImpactDirection,
                    Vector3.right),
                Is.GreaterThan(0.999f));
            yield return WaitForReactionToFinish(runner);

            Vector3 displacement = Vector3.ProjectOnPlane(
                runner.transform.position - start,
                Vector3.up);
            Assert.That(displacement.magnitude, Is.InRange(1.6f, 2.81f));
            Assert.That(Vector3.Dot(displacement.normalized, Vector3.right),
                Is.GreaterThan(0.99f));
        }

        [UnityTest]
        public IEnumerator HeavyChair_WallSweepStopsBeforePenetration_AndEmitsSecondaryImpact()
        {
            Fixture fixture = CreateFixture(1);
            ShowcaseRunnerAgent runner = fixture.Runners[0];
            Collider runnerCollider = runner.GetComponent<Collider>();
            GameObject wallObject = CreateObject("IMPACT_TEST_WALL");
            wallObject.transform.SetPositionAndRotation(
                runner.transform.position + Vector3.right * 1.35f,
                Quaternion.identity);
            wallObject.transform.localScale = new Vector3(0.2f, 3f, 3f);
            Collider wallCollider = wallObject.AddComponent<BoxCollider>();
            PrototypeInteractionTarget chair = CreateThrownObject(
                PrototypeWeightClass.Medium,
                PrototypeInteractionRole.DirectChair,
                8f,
                Vector3.right * 12.8f,
                1f,
                ShowcaseObjectImpactAttackOriginType.HandThrow);
            Physics.SyncTransforms();

            RegisterImpact(
                fixture,
                runner,
                chair,
                Vector3.right * 12.8f);
            AcknowledgeCurrentAnimation(runner);
            yield return WaitForReactionToFinish(runner);
            Physics.SyncTransforms();

            Assert.That(runnerCollider.bounds.max.x,
                Is.LessThanOrEqualTo(wallCollider.bounds.min.x + 0.025f));
            Assert.That(fixture.Pressure.SecondaryImpactCount, Is.EqualTo(1));
            Assert.That(fixture.Pressure.LastSecondaryImpactResult.Kind,
                Is.EqualTo(ShowcaseSecondaryImpactKind.WorldSolid));
            Assert.That(fixture.Pressure.LastSecondaryImpactResult.Obstacle,
                Is.SameAs(wallCollider));
            Assert.That(runner.SecondaryStunCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator EnemySecondaryCollision_StaggersBoth_DamagesSecondary_AndDeduplicatesPair()
        {
            Fixture fixture = CreateFixture(2, 1.35f);
            ShowcaseRunnerAgent primary = fixture.Runners[0];
            ShowcaseRunnerAgent secondary = fixture.Runners[1];
            int secondaryHealthBefore = secondary.Health.CurrentHealth;
            PrototypeInteractionTarget chair = CreateThrownObject(
                PrototypeWeightClass.Medium,
                PrototypeInteractionRole.DirectChair,
                8f,
                Vector3.right * 12.8f,
                1f,
                ShowcaseObjectImpactAttackOriginType.HandThrow);
            Physics.SyncTransforms();

            RegisterImpact(
                fixture,
                primary,
                chair,
                Vector3.right * 12.8f);
            ShowcaseObjectImpactEvaluation evaluation =
                fixture.Pressure.LastObjectImpactEvaluation;
            AcknowledgeCurrentAnimation(primary);

            float deadline = Time.realtimeSinceStartup + 1f;
            while (fixture.Pressure.SecondaryImpactCount == 0 &&
                   Time.realtimeSinceStartup < deadline)
            {
                yield return new WaitForFixedUpdate();
                AcknowledgeCurrentAnimation(primary);
                AcknowledgeCurrentAnimation(secondary);
            }

            Assert.That(fixture.Pressure.SecondaryImpactCount, Is.EqualTo(1));
            Assert.That(fixture.Pressure.LastSecondaryImpactResult.Kind,
                Is.EqualTo(ShowcaseSecondaryImpactKind.Enemy));
            Assert.That(secondary.Health.CurrentHealth,
                Is.LessThan(secondaryHealthBefore));
            Assert.That(primary.SecondaryStunCount, Is.EqualTo(1));
            Assert.That(secondary.SecondaryStunCount, Is.EqualTo(1));
            Assert.That(secondary.State,
                Is.EqualTo(ShowcaseRunnerState.Staggered));

            bool duplicateAccepted = fixture.Pressure.RegisterSecondaryImpact(
                primary,
                secondary.GetComponent<Collider>(),
                evaluation);
            Assert.That(duplicateAccepted, Is.False);
            Assert.That(fixture.Pressure.SecondaryImpactCount, Is.EqualTo(1));
            Assert.That(
                fixture.Pressure.RejectedSecondaryImpactDuplicateCount,
                Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator KillingHeavyChair_PreservesControlledDeathMomentumBeforeSettlement()
        {
            Fixture fixture = CreateFixture(1, runnerSpacing: 4f, runnerHealth: 30);
            ShowcaseRunnerAgent runner = fixture.Runners[0];
            Collider runnerCollider = runner.GetComponent<Collider>();
            Vector3 start = runner.transform.position;
            PrototypeInteractionTarget chair = CreateThrownObject(
                PrototypeWeightClass.Medium,
                PrototypeInteractionRole.DirectChair,
                8f,
                Vector3.right * 12.8f,
                1f,
                ShowcaseObjectImpactAttackOriginType.HandThrow);

            RegisterImpact(
                fixture,
                runner,
                chair,
                Vector3.right * 12.8f);

            Assert.That(runner.IsDead, Is.True);
            Assert.That(runner.State, Is.EqualTo(ShowcaseRunnerState.Dead));
            Assert.That(runner.IsDeathMomentumActive, Is.True);
            Assert.That(runner.IsReactionDisplacementActive, Is.True);
            Assert.That(runner.CurrentReactionVelocity.magnitude,
                Is.GreaterThan(0f));
            Assert.That(runnerCollider.enabled, Is.True,
                "Heavy killing blow must keep collision active during momentum.");

            yield return WaitForReactionToFinish(runner);

            Assert.That(PlanarDistance(runner.transform.position, start),
                Is.InRange(1.6f, 2.81f));
            Assert.That(runner.IsDeathMomentumActive, Is.False);
            Assert.That(runner.CurrentReactionVelocity, Is.EqualTo(Vector3.zero));
            Assert.That(runnerCollider.enabled, Is.False);
        }

        [UnityTest]
        public IEnumerator OneThrowSequence_PlaysDamageVfxAndFeedbackExactlyOnce()
        {
            Fixture fixture = CreateFixture(1);
            ShowcaseRunnerAgent runner = fixture.Runners[0];
            ShowcaseCombatHitVfxPresenter vfx = CreateVfxPresenter(fixture.Pressure);
            ShowcaseHitFeedbackController feedback = CreateFeedbackController(
                fixture.Pressure);
            PrototypeInteractionTarget chair = CreateThrownObject(
                PrototypeWeightClass.Medium,
                PrototypeInteractionRole.DirectChair,
                8f,
                Vector3.right * 8f,
                0f,
                ShowcaseObjectImpactAttackOriginType.HandThrow);
            int healthBefore = runner.Health.CurrentHealth;

            RegisterImpact(fixture, runner, chair, Vector3.right * 8f);
            AcknowledgeCurrentAnimation(runner);
            RegisterImpact(fixture, runner, chair, Vector3.right * 8f);

            Assert.That(runner.Health.AcceptedDamageCount, Is.EqualTo(1));
            Assert.That(runner.Health.CurrentHealth, Is.LessThan(healthBefore));
            Assert.That(fixture.Pressure.PhysicalImpactCount, Is.EqualTo(1));
            Assert.That(vfx.PlayedImpactCount, Is.EqualTo(1));
            Assert.That(vfx.LastTier, Is.EqualTo(ShowcaseHitVfxTier.Medium));
            Assert.That(feedback.PlayCount, Is.EqualTo(1));
            Assert.That(feedback.LastLevel, Is.EqualTo(ShowcaseHitVfxTier.Medium));
            Assert.That(runner.Health.RejectedDamageCount, Is.EqualTo(1));
            yield return null;
        }

        private Fixture CreateFixture(
            int runnerCount,
            float runnerSpacing = 4f,
            int runnerHealth = 100)
        {
            GameObject player = CreateObject("IMPACT_TEST_PLAYER");
            player.transform.position = new Vector3(-50f, 1f, -50f);
            PrototypeInteractionStateOwner owner =
                player.AddComponent<PrototypeInteractionStateOwner>();
            ShowcaseRunnerPressureController pressure =
                player.AddComponent<ShowcaseRunnerPressureController>();
            var runners = new ShowcaseRunnerAgent[runnerCount];
            for (int index = 0; index < runnerCount; index++)
            {
                GameObject runnerObject = CreateObject($"IMPACT_TEST_RUNNER_{index}");
                runnerObject.transform.position =
                    new Vector3(index * runnerSpacing, 1f, 0f);
                CapsuleCollider collider =
                    runnerObject.AddComponent<CapsuleCollider>();
                collider.height = 2f;
                collider.radius = 0.5f;
                Rigidbody body = runnerObject.AddComponent<Rigidbody>();
                body.isKinematic = true;
                body.useGravity = false;
                body.constraints = RigidbodyConstraints.FreezeRotation;
                ShowcaseCombatHealth health =
                    runnerObject.AddComponent<ShowcaseCombatHealth>();
                health.Configure(
                    ShowcaseCombatTeam.Enemy,
                    runnerHealth,
                    0f);
                ShowcaseRunnerAgent runner =
                    runnerObject.AddComponent<ShowcaseRunnerAgent>();
                runner.Configure(
                    $"IMPACT_TEST_RUNNER_{index}",
                    pressure,
                    body,
                    player.transform,
                    authoredHealth: health);
                runners[index] = runner;
            }
            pressure.Configure(
                owner,
                player.transform,
                null,
                null,
                null,
                null,
                null,
                runners);
            Physics.SyncTransforms();
            return new Fixture
            {
                Owner = owner,
                Pressure = pressure,
                Runners = runners
            };
        }

        private PrototypeInteractionTarget CreateThrownObject(
            PrototypeWeightClass weightClass,
            PrototypeInteractionRole role,
            float mass,
            Vector3 velocity,
            float charge01,
            ShowcaseObjectImpactAttackOriginType origin)
        {
            GameObject sourceObject = CreateObject(
                $"IMPACT_SOURCE_{weightClass}_{nextThrowSequence}");
            sourceObject.transform.position = new Vector3(30f, 10f, 30f);
            sourceObject.AddComponent<BoxCollider>();
            Rigidbody body = sourceObject.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.mass = mass;
            body.linearVelocity = velocity;
            PrototypeInteractionTarget target =
                sourceObject.AddComponent<PrototypeInteractionTarget>();
            target.Configure(
                $"IMPACT-{weightClass}-DEF",
                $"IMPACT-{weightClass}-{nextThrowSequence}",
                $"Impact {weightClass}",
                weightClass,
                role,
                true,
                false,
                false,
                false);

            uint sequence = ++nextThrowSequence;
            MethodInfo commit = typeof(PrototypeInteractionTarget).GetMethod(
                "TryCommitSuccessfulPlayerThrow",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(commit, Is.Not.Null);
            Assert.That(commit.Invoke(target, new object[] { sequence }), Is.True);
            var release = new PhysicalReleaseSnapshot(
                Time.frameCount,
                sequence,
                target,
                target.transform.position,
                target.transform.rotation,
                velocity,
                Vector3.zero,
                charge01,
                origin);
            MethodInfo record = typeof(PrototypeInteractionTarget).GetMethod(
                "RecordPhysicalReleaseImpact",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(record, Is.Not.Null);
            record.Invoke(target, new object[]
            {
                release,
                0.5f * mass * velocity.sqrMagnitude
            });
            return target;
        }

        private static void RegisterImpact(
            Fixture fixture,
            ShowcaseRunnerAgent runner,
            PrototypeInteractionTarget source,
            Vector3 velocity)
        {
            fixture.Pressure.RegisterPhysicalImpact(
                runner,
                source,
                velocity.magnitude,
                velocity,
                runner.transform.position + Vector3.up);
        }

        private ShowcaseCombatHitVfxPresenter CreateVfxPresenter(
            ShowcaseRunnerPressureController pressure)
        {
            GameObject[] light = CreateParticleVariants("LIGHT");
            GameObject[] medium = CreateParticleVariants("MEDIUM");
            GameObject[] heavy = CreateParticleVariants("HEAVY");
            GameObject root = CreateObject("IMPACT_TEST_VFX_PRESENTER");
            root.SetActive(false);
            ShowcaseCombatHitVfxPresenter presenter =
                root.AddComponent<ShowcaseCombatHitVfxPresenter>();
            presenter.Configure(pressure, light, medium, heavy);
            root.SetActive(true);
            return presenter;
        }

        private ShowcaseHitFeedbackController CreateFeedbackController(
            ShowcaseRunnerPressureController pressure)
        {
            GameObject root = CreateObject("IMPACT_TEST_FEEDBACK");
            root.SetActive(false);
            ShowcaseHitFeedbackController feedback =
                root.AddComponent<ShowcaseHitFeedbackController>();
            feedback.Configure(pressure, null, null);
            root.SetActive(true);
            return feedback;
        }

        private GameObject[] CreateParticleVariants(string prefix)
        {
            var variants = new GameObject[3];
            for (int index = 0; index < variants.Length; index++)
            {
                GameObject variant = CreateObject(
                    $"IMPACT_TEST_{prefix}_VFX_{index}");
                variant.SetActive(false);
                ParticleSystem system = variant.AddComponent<ParticleSystem>();
                ParticleSystem.MainModule main = system.main;
                main.playOnAwake = false;
                main.loop = false;
                variants[index] = variant;
            }
            return variants;
        }

        private static void AcknowledgeCurrentAnimation(
            ShowcaseRunnerAgent runner)
        {
            if (runner != null && runner.AnimationCueRevision != 0)
            {
                runner.NotifyAnimationStateEntered(runner.AnimationCueRevision);
            }
        }

        private static IEnumerator WaitForReactionToFinish(
            ShowcaseRunnerAgent runner)
        {
            float deadline = Time.realtimeSinceStartup + 1.25f;
            while (runner.IsReactionDisplacementActive &&
                   Time.realtimeSinceStartup < deadline)
            {
                yield return new WaitForFixedUpdate();
                AcknowledgeCurrentAnimation(runner);
            }
            Assert.That(runner.IsReactionDisplacementActive, Is.False,
                "Controlled impact displacement did not settle within its bounded window.");
        }

        private GameObject CreateObject(string name)
        {
            var created = new GameObject(name);
            createdObjects.Add(created);
            return created;
        }

        private static float PlanarDistance(Vector3 first, Vector3 second)
        {
            return Vector3.ProjectOnPlane(first - second, Vector3.up).magnitude;
        }
    }
}
