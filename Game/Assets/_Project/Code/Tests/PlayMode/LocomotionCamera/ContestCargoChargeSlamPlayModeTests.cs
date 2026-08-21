using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Input;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestCargoChargeSlamPlayModeTests
    {
        private const string S7ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v007.unity";
        private static int cleanupSequence;

        private sealed class MediumFixture
        {
            public ShowcaseCargoStrapController Cargo;
            public PrototypeInteractionController Interaction;
            public PrototypeInteractionStateOwner Owner;
            public InputSystemLogicalInputAdapter Input;
            public ThirdPersonLocomotion Locomotion;
            public ShowcasePlayerCombatController Combat;
            public ShowcaseRunnerPressureController Pressure;
            public ShowcaseCharacterAnimationDriver PlayerAnimation;
            public PrototypeInteractionTarget Chair;
            public Rigidbody ChairBody;
            public ShowcaseRunnerAgent Normal;
            public ShowcaseRunnerAgent Elite;
            public float NormalY;
            public float EliteY;
        }

        [UnityTearDown]
        public IEnumerator TearDownLoadedShowcaseScene()
        {
            Scene cleanup = SceneManager.CreateScene(
                $"P0_02_TEST_CLEANUP_{++cleanupSequence}");
            SceneManager.SetActiveScene(cleanup);
            for (int index = SceneManager.sceneCount - 1; index >= 0; index--)
            {
                Scene loaded = SceneManager.GetSceneAt(index);
                if (loaded == cleanup || !loaded.isLoaded)
                {
                    continue;
                }

                AsyncOperation unload = SceneManager.UnloadSceneAsync(loaded);
                if (unload != null)
                {
                    yield return unload;
                }
            }
        }

        [UnityTest]
        public IEnumerator StationaryValidationRosterPreservesOriginalsAndDoesNotApproach()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S7ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            Assert.That(pressure.Runners.Length, Is.EqualTo(2),
                "The original active combat roster must remain exactly two.");
            Assert.That(pressure.TargetableRunners.Length, Is.EqualTo(2),
                "Batch validation must remove the four manual-only runners " +
                "before legacy Scene-wide component queries execute.");
            Assert.That(SceneComponents<ShowcaseRunnerAgent>().Count(runner =>
                    runner != null && runner.RunnerId.StartsWith(
                        "P0_03_STATIONARY_",
                        StringComparison.Ordinal)),
                Is.Zero);

            foreach (ShowcaseRunnerAgent original in pressure.Runners)
            {
                original.enabled = false;
            }

            ShowcaseRunnerAgent template = pressure.Runners
                .First(runner => !runner.IsElite);
            GameObject cloneObject = UnityEngine.Object.Instantiate(
                template.gameObject);
            cloneObject.name = "P0_03_STATIONARY_RUNTIME_TEST";
            ShowcaseRunnerAgent stationary =
                cloneObject.GetComponent<ShowcaseRunnerAgent>();
            Rigidbody body = cloneObject.GetComponent<Rigidbody>();
            ShowcaseCombatHealth health =
                cloneObject.GetComponent<ShowcaseCombatHealth>();
            Assert.That(stationary, Is.Not.Null);
            Assert.That(body, Is.Not.Null);
            Assert.That(health, Is.Not.Null);
            Vector3 start = pressure.PlayerTransform.position +
                pressure.PlayerTransform.forward * 4f;
            start.y = template.transform.position.y;
            cloneObject.transform.position = start;
            body.position = start;
            body.isKinematic = true;
            body.useGravity = false;
            stationary.Configure(
                "P0_03_STATIONARY_RUNTIME_TEST",
                pressure,
                body,
                pressure.PlayerTransform,
                authoredHealth: health,
                authoredElite: false);
            stationary.ConfigureAutonomousMovementSuppressed(true);
            Physics.SyncTransforms();

            for (int step = 0; step < 30; step++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(stationary.SuppressesAutonomousMovement, Is.True);
            Assert.That(Vector3.Distance(stationary.transform.position, start),
                Is.LessThan(0.01f),
                "A stationary validation Runner moved without being hit.");
            Assert.That(stationary.AttackCommitCount, Is.Zero);
            Assert.That(stationary.CommandedPlanarSpeed,
                Is.EqualTo(0f).Within(0.001f));
            UnityEngine.Object.Destroy(cloneObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ChargeAllowsReducedMovementAndCancelCreatesNoAttack()
        {
            MediumFixture fixture = null;
            yield return LoadMediumFixture(value => fixture = value);
            PositionOpenSweepTarget(fixture, 3.6f, -0.6f);

            int healthBefore = fixture.Normal.Health.CurrentHealth;
            Collider normalCollider = fixture.Normal.GetComponent<Collider>();
            Vector3 aimPoint = normalCollider.bounds.center;
            Vector3 commandedVelocity = fixture.Owner.transform.right * 2.5f;
            SetPrivateField(
                fixture.Normal,
                "commandedPlanarVelocity",
                commandedVelocity);
            Vector3 playerStart = fixture.Owner.transform.position;
            float playerStartY = playerStart.y;

            Assert.That(fixture.Cargo.TryBeginCharge(aimPoint), Is.True,
                fixture.Cargo.LastMediumAttackRejection);
            Assert.That(fixture.Cargo.IsCharging, Is.True);
            Assert.That(fixture.Cargo.CurrentAttackResolution.IsAccepted, Is.True);
            ShowcaseCargoAttackType resolvedAttack =
                fixture.Cargo.CurrentAttackResolution.AttackType;
            Assert.That(
                resolvedAttack == ShowcaseCargoAttackType.SweepLeft ||
                resolvedAttack == ShowcaseCargoAttackType.SweepRight,
                Is.True,
                "Normal enemy body aim must remain in the lateral Sweep " +
                "family rather than being misclassified as Slam.");
            Assert.That(fixture.Cargo.ChargeResolvedTarget,
                Is.SameAs(fixture.Normal));
            Vector3 leadOffset = fixture.Cargo.ChargeResolvedTargetPoint -
                normalCollider.bounds.center;
            Assert.That(
                Vector3.Dot(leadOffset, commandedVelocity.normalized),
                Is.GreaterThan(0.25f),
                "Moving runner target must be led along its estimated velocity.");
            Assert.That(fixture.Cargo.BlocksPlayerMobility, Is.False);
            Assert.That(fixture.Locomotion.MovementSpeedScale,
                Is.EqualTo(0.75f).Within(0.001f));
            Assert.That(fixture.Locomotion.IsSprintExternallyBlocked, Is.True);
            Assert.That(fixture.Locomotion.IsJumpExternallyBlocked, Is.True);

            SetSnapshot(fixture.Input, new LogicalInputSnapshot(
                Vector2.up,
                Vector2.zero,
                true,
                false,
                false,
                false,
                false,
                false,
                sprint: true,
                jump: true));
            for (int index = 0; index < 8; index++)
            {
                yield return new WaitForFixedUpdate();
            }

            Vector3 planarTravel = Vector3.ProjectOnPlane(
                fixture.Owner.transform.position - playerStart,
                Vector3.up);
            Assert.That(planarTravel.magnitude, Is.GreaterThan(0.04f));
            Assert.That(Mathf.Abs(
                    fixture.Owner.transform.position.y - playerStartY),
                Is.LessThan(0.15f));
            Assert.That(fixture.Locomotion.IsSprinting, Is.False);

            float fullChargeDeadline = Time.time + 1.2f;
            while (fixture.Cargo.Charge01 < 0.98f &&
                   Time.time < fullChargeDeadline)
            {
                yield return null;
            }
            Assert.That(fixture.Cargo.Charge01, Is.GreaterThanOrEqualTo(0.98f));
            Assert.That(fixture.Locomotion.MovementSpeedScale,
                Is.EqualTo(0.5f).Within(0.03f),
                "Full charge must slow movement from 75% toward 50%.");

            Assert.That(fixture.Cargo.TryCancelCharge(), Is.True);
            SetSnapshot(fixture.Input, default);
            yield return null;

            Assert.That(fixture.Cargo.Phase,
                Is.EqualTo(ShowcaseCargoStrapPhase.Tensioned));
            Assert.That(fixture.Cargo.ChargeCancelCount, Is.EqualTo(1));
            Assert.That(fixture.Cargo.AttackReleaseCount, Is.EqualTo(0));
            Assert.That(fixture.Cargo.AttackImpulseCount, Is.EqualTo(0));
            Assert.That(fixture.Normal.Health.CurrentHealth, Is.EqualTo(healthBefore));
            Assert.That(fixture.Locomotion.MovementSpeedScale,
                Is.EqualTo(1f).Within(0.001f));
            Assert.That(fixture.Locomotion.IsSprintExternallyBlocked, Is.False);
            Assert.That(fixture.Locomotion.IsJumpExternallyBlocked, Is.False);
        }

        [UnityTest]
        public IEnumerator NaturalLmbChargeReleaseStartsAnimationImpulseAndMovesChair()
        {
            MediumFixture fixture = null;
            yield return LoadMediumFixture(value => fixture = value);
            PositionOpenSweepTarget(fixture, 3.6f, 0.7f);

            fixture.PlayerAnimation.enabled = true;
            yield return null;
            Vector3 aimPoint = fixture.Normal.GetComponent<Collider>().bounds.center;
            Assert.That(fixture.Interaction.ProcessAttackPressed(), Is.True,
                fixture.Cargo.LastMediumAttackRejection);
            Assert.That(fixture.Cargo.IsCharging, Is.True);

            float chargeDeadline = Time.time + 0.5f;
            while (fixture.Cargo.Charge01 < 0.45f && Time.time < chargeDeadline)
            {
                yield return null;
            }
            Assert.That(fixture.Cargo.TryReleaseCharge(aimPoint), Is.True,
                fixture.Cargo.LastMediumAttackRejection);
            Vector3 start = fixture.ChairBody.worldCenterOfMass;
            float deadline = Time.time + 1.5f;
            while (fixture.Cargo.AttackImpulseCount == 0 && Time.time < deadline)
            {
                yield return null;
            }
            Assert.That(fixture.Cargo.AttackImpulseCount, Is.EqualTo(1),
                "The natural Humanoid Strap action must fire its authored " +
                "Chair impulse event after LMB Charge release.");

            for (int index = 0; index < 8; index++)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(Vector3.Distance(
                    fixture.ChairBody.worldCenterOfMass,
                    start),
                Is.GreaterThan(0.25f),
                $"Chair did not travel; phase={fixture.Cargo.Phase}, " +
                $"termination={fixture.Cargo.MediumSweepTerminationReason}, " +
                $"obstacle={fixture.Cargo.MediumSweepTerminationObstacle?.name}, " +
                $"energy={fixture.Cargo.RemainingImpactEnergy01:0.00}");
        }

        [UnityTest]
        public IEnumerator FirstContinuousLmbHoldStartsChargeImmediatelyBeforeRearReady()
        {
            MediumFixture fixture = null;
            yield return LoadMediumFixture(
                value => fixture = value,
                waitForRearReady: false);

            Assert.That(fixture.Cargo.Phase,
                Is.EqualTo(ShowcaseCargoStrapPhase.Tensioned));
            SetPrivateField(fixture.Cargo, "mediumRearReady", false);
            SetPrivateField(fixture.Cargo, "mediumRearStableSteps", 0);
            fixture.ChairBody.position +=
                fixture.Owner.transform.right * 0.9f;
            fixture.ChairBody.transform.position = fixture.ChairBody.position;
            Physics.SyncTransforms();
            Assert.That(fixture.Cargo.IsMediumRearReady, Is.False);

            SetSnapshot(fixture.Input, new LogicalInputSnapshot(
                Vector2.zero,
                Vector2.zero,
                false,
                false,
                true,
                false,
                false,
                false,
                throwHeld: true));
            yield return null;

            Assert.That(fixture.Cargo.IsCharging, Is.True,
                $"The first LMB hold did not start Charge immediately; " +
                $"rejection={fixture.Cargo.LastMediumAttackRejection}");
            Assert.That(fixture.Cargo.IsMediumRearReady, Is.False,
                "Charge must not wait for the last rear-alignment steps.");
            float chargeAfterFirstPress = fixture.Cargo.Charge01;

            // No second press edge is supplied. The same continuous hold must
            // keep accumulating Charge while the Chair finishes rear alignment.
            SetSnapshot(fixture.Input, new LogicalInputSnapshot(
                Vector2.zero,
                Vector2.zero,
                false,
                false,
                false,
                false,
                false,
                false,
                throwHeld: true));
            float chargeDeadline = Time.time + 0.3f;
            while (fixture.Cargo.Charge01 <= chargeAfterFirstPress + 0.08f &&
                   Time.time < chargeDeadline)
            {
                yield return null;
            }

            Assert.That(fixture.Cargo.IsCharging, Is.True);
            Assert.That(fixture.Cargo.Charge01,
                Is.GreaterThan(chargeAfterFirstPress + 0.08f),
                "Charge stopped unless a second LMB press was generated.");
            Assert.That(fixture.Cargo.AttackReleaseCount, Is.Zero);

            SetSnapshot(fixture.Input, default);
            Assert.That(fixture.Cargo.TryCancelCharge(), Is.True);
        }

        [UnityTest]
        public IEnumerator NaturalSlamReleaseRaisesChairAndCommitsOneContact()
        {
            MediumFixture fixture = null;
            yield return LoadMediumFixture(value => fixture = value);

            Vector3 player = fixture.Owner.transform.position;
            Vector3 impactPoint = player +
                (fixture.Owner.transform.forward * 1.35f);
            impactPoint.y = player.y - 0.9f;
            fixture.Normal.transform.position = new Vector3(
                impactPoint.x,
                fixture.NormalY,
                impactPoint.z);
            fixture.Elite.transform.position = fixture.Owner.transform.position +
                new Vector3(12f, 0f, 12f);
            fixture.Elite.transform.position = new Vector3(
                fixture.Elite.transform.position.x,
                fixture.EliteY,
                fixture.Elite.transform.position.z);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            fixture.PlayerAnimation.enabled = true;
            yield return null;
            Assert.That(fixture.Cargo.TryBeginCharge(impactPoint), Is.True,
                fixture.Cargo.LastMediumAttackRejection);
            Assert.That(fixture.Cargo.CurrentAttackResolution.AttackType,
                Is.EqualTo(ShowcaseCargoAttackType.OverheadSlam));
            float chargeDeadline = Time.time + 1.2f;
            while (fixture.Cargo.Charge01 < 0.92f && Time.time < chargeDeadline)
            {
                yield return null;
            }
            Assert.That(fixture.Cargo.TryReleaseCharge(impactPoint), Is.True,
                fixture.Cargo.LastMediumAttackRejection);

            int healthBefore = fixture.Normal.Health.CurrentHealth;
            Vector3 start = fixture.ChairBody.worldCenterOfMass;
            float maximumY = start.y;
            float deadline = Time.time + 2f;
            while (fixture.Cargo.SlamImpactCount == 0 && Time.time < deadline)
            {
                maximumY = Mathf.Max(
                    maximumY,
                    fixture.ChairBody.worldCenterOfMass.y);
                yield return new WaitForFixedUpdate();
            }

            Assert.That(fixture.Cargo.AttackImpulseCount, Is.EqualTo(1),
                "Natural Slam animation did not fire its Chair impulse.");
            Assert.That(maximumY - start.y, Is.GreaterThan(0.5f),
                "The Chair never reached an overhead Slam apex.");
            Assert.That(fixture.Cargo.SlamImpactCount, Is.EqualTo(1),
                $"Natural Slam did not authorize contact; " +
                $"phase={fixture.Cargo.Phase}, " +
                $"downward={GetPrivateField<float>(fixture.Cargo, "slamDownwardTravel"):0.00}");
            Assert.That(fixture.Normal.Health.CurrentHealth,
                Is.LessThan(healthBefore));
        }

        [UnityTest]
        public IEnumerator TapReleaseCommitsOneQuickAttackWithFastAcceleration()
        {
            MediumFixture fixture = null;
            yield return LoadMediumFixture(value => fixture = value);
            PositionOpenSweepTarget(fixture, 3.6f, 0.7f);

            Vector3 aimPoint = fixture.Normal.GetComponent<Collider>().bounds.center;
            Assert.That(fixture.Cargo.TryBeginCharge(aimPoint), Is.True,
                fixture.Cargo.LastMediumAttackRejection);
            Assert.That(fixture.Cargo.CurrentAttackResolution.AttackType,
                Is.EqualTo(ShowcaseCargoAttackType.SweepRight),
                "Open body aim must not be misclassified as Slam.");
            float quickDeadline = Time.time + 0.25f;
            while (fixture.Cargo.Charge01 < 0.08f && Time.time < quickDeadline)
            {
                yield return null;
            }

            Assert.That(fixture.Cargo.TryReleaseCharge(aimPoint), Is.True,
                fixture.Cargo.LastMediumAttackRejection);
            uint attackRevision = fixture.Cargo.AnimationCueRevision;
            Assert.That(fixture.Cargo.ActiveAttackChargeProfile.Tier,
                Is.EqualTo(ShowcaseCargoChargeTier.Quick));
            Assert.That(fixture.Cargo.ActiveAttackChargeProfile.DamageMultiplier,
                Is.EqualTo(0.7f).Within(0.001f));
            Assert.That(fixture.Cargo.AttackReleaseCount, Is.EqualTo(1));
            Assert.That(fixture.Cargo.IsAttackActive, Is.True);
            Assert.That(fixture.Cargo.TryReleaseCharge(aimPoint), Is.False);

            yield return new WaitForSeconds(0.12f);
            Assert.That(
                fixture.Cargo.NotifyAnimationStateEntered(attackRevision),
                Is.True);
            Assert.That(
                fixture.Cargo.CommitAnimationStrapAttackImpulse(attackRevision),
                Is.True);
            Assert.That(fixture.Cargo.AttackImpulseCount, Is.EqualTo(1));
            Assert.That(fixture.Cargo.LastReleaseToObjectAccelerationSeconds,
                Is.InRange(0.1f, 0.2f));
            TestContext.WriteLine(
                $"Quick release-to-acceleration: " +
                $"{fixture.Cargo.LastReleaseToObjectAccelerationSeconds:F3}s");

            Assert.That(
                fixture.Cargo.CompleteAnimationStrapAction(attackRevision),
                Is.True);
            Assert.That(fixture.Cargo.AttackReleaseCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator SlamTargetKeepsChairSupportAboveRecordedFloor()
        {
            MediumFixture fixture = null;
            yield return LoadMediumFixture(value => fixture = value);

            Vector3 impactPoint = fixture.Owner.transform.position +
                (fixture.Owner.transform.forward * 1.35f);
            impactPoint.y = fixture.Owner.transform.position.y - 0.9f;
            Vector3 resolvedCenter = InvokePrivate<Vector3>(
                fixture.Cargo,
                "ResolveSlamTargetPosition",
                impactPoint);
            float supportOffset = GetPrivateField<float>(
                fixture.Cargo,
                "mediumGroundSupportOffsetY");
            float recordedCenterY = GetPrivateField<float>(
                fixture.Cargo,
                "mediumGroundCenterOfMassY");
            float recordedFloorY = recordedCenterY - supportOffset;
            float resolvedBottomY = resolvedCenter.y - supportOffset;

            Assert.That(supportOffset, Is.GreaterThan(0.01f));
            Assert.That(resolvedBottomY,
                Is.GreaterThanOrEqualTo(recordedFloorY + 0.02f),
                "Slam target must preserve COM-to-bottom support and a floor " +
                "separation instead of driving the Chair through the floor.");
        }

        [UnityTest]
        public IEnumerator FreeAimWithoutEnemyStillReleasesAndAcceleratesChair()
        {
            MediumFixture fixture = null;
            yield return LoadMediumFixture(value => fixture = value);

            int normalHealthBefore = fixture.Normal.Health.CurrentHealth;
            int eliteHealthBefore = fixture.Elite.Health.CurrentHealth;
            Vector3 aimPoint = fixture.Owner.transform.position +
                (fixture.Owner.transform.forward * 3.6f) +
                (fixture.Owner.transform.right * 0.7f) +
                (Vector3.up * 1.1f);

            Assert.That(fixture.Cargo.TryBeginCharge(aimPoint), Is.True,
                fixture.Cargo.LastMediumAttackRejection);
            Assert.That(fixture.Cargo.ChargeResolvedTarget, Is.Null);
            Assert.That(fixture.Cargo.CurrentAttackResolution.IsAccepted, Is.True);
            Assert.That(fixture.Cargo.CurrentAttackResolution.AttackType,
                Is.EqualTo(ShowcaseCargoAttackType.SweepRight));

            float quickDeadline = Time.time + 0.25f;
            while (fixture.Cargo.Charge01 < 0.08f && Time.time < quickDeadline)
            {
                yield return null;
            }

            Assert.That(fixture.Cargo.TryReleaseCharge(aimPoint), Is.True,
                fixture.Cargo.LastMediumAttackRejection);
            Assert.That(fixture.Cargo.MediumSwingTarget, Is.Null);
            Assert.That(fixture.Cargo.ActiveAttackChargeProfile.Tier,
                Is.EqualTo(ShowcaseCargoChargeTier.Quick));
            Assert.That(fixture.Cargo.AttackReleaseCount, Is.EqualTo(1));
            uint attackRevision = fixture.Cargo.AnimationCueRevision;

            yield return new WaitForSeconds(0.12f);
            Assert.That(
                fixture.Cargo.NotifyAnimationStateEntered(attackRevision),
                Is.True);
            Assert.That(
                fixture.Cargo.CommitAnimationStrapAttackImpulse(attackRevision),
                Is.True);
            Assert.That(
                fixture.Cargo.ReportAnimationProgress(attackRevision, 0.75f),
                Is.True);
            Vector3 chairStart = fixture.ChairBody.position;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.That(fixture.Cargo.MediumSwingStepCount, Is.GreaterThan(0));
            Assert.That(
                Vector3.Distance(fixture.ChairBody.position, chairStart),
                Is.GreaterThan(0.02f),
                "Targetless free aim must still launch the Chair along its sweep.");
            Assert.That(fixture.Normal.Health.CurrentHealth,
                Is.EqualTo(normalHealthBefore));
            Assert.That(fixture.Elite.Health.CurrentHealth,
                Is.EqualTo(eliteHealthBefore));
            Assert.That(fixture.Cargo.AttackReleaseCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator QuickSweepUsesShortTravelBudgetAndConsumesOneChairUse()
        {
            MediumFixture fixture = null;
            yield return LoadMediumFixture(value => fixture = value);
            PositionOpenSweepTarget(fixture, 3.6f, 0.7f);

            Vector3 aimPoint = fixture.Normal.GetComponent<Collider>().bounds.center;
            int usesBefore = fixture.Chair.CommittedPlayerUses;
            Assert.That(fixture.Cargo.TryBeginCharge(aimPoint), Is.True,
                fixture.Cargo.LastMediumAttackRejection);
            float quickDeadline = Time.time + 0.2f;
            while (fixture.Cargo.Charge01 < 0.06f &&
                   Time.time < quickDeadline)
            {
                yield return null;
            }
            Assert.That(fixture.Cargo.TryReleaseCharge(aimPoint), Is.True,
                fixture.Cargo.LastMediumAttackRejection);
            Assert.That(fixture.Cargo.ActiveAttackChargeProfile.Tier,
                Is.EqualTo(ShowcaseCargoChargeTier.Quick));
            uint revision = fixture.Cargo.AnimationCueRevision;
            yield return new WaitForSeconds(0.12f);
            Assert.That(fixture.Cargo.NotifyAnimationStateEntered(revision),
                Is.True);
            Assert.That(
                fixture.Cargo.CommitAnimationStrapAttackImpulse(revision),
                Is.True);

            ShowcaseRunnerAgent victim = CreateStationaryRunnerClone(
                fixture.Normal,
                "P0_03_QUICK_TRAVEL_VICTIM",
                fixture.Pressure,
                fixture.Owner.transform,
                200);
            MoveRunnerColliderCenter(victim, new Vector3(0f, 12f, 18f));
            Vector3 start = victim.transform.position;
            Assert.That(fixture.Pressure.TryRegisterCargoSweepImpact(
                    fixture.Cargo,
                    victim,
                    fixture.Chair,
                    12f,
                    Vector3.right * 12f,
                    victim.GetComponent<Collider>().bounds.center),
                Is.True);
            Assert.That(fixture.Chair.CommittedPlayerUses,
                Is.EqualTo(usesBefore + 1),
                "The first accepted swept Enemy contact must consume exactly " +
                "one Chair use even without a physical OnCollision callback.");

            fixture.ChairBody.constraints = RigidbodyConstraints.FreezeAll;
            for (int step = 0; step < 18; step++)
            {
                yield return new WaitForFixedUpdate();
            }
            float distance = Vector3.ProjectOnPlane(
                victim.transform.position - start,
                Vector3.up).magnitude;
            Assert.That(distance, Is.InRange(0.2f, 0.85f),
                $"Quick victim travelled {distance:0.00} m; a tap must not " +
                "reuse the Power/Full launch budget.");

            int committedUses = fixture.Chair.CommittedPlayerUses;
            fixture.Pressure.RegisterPhysicalImpact(
                victim,
                fixture.Chair,
                12f,
                Vector3.right * 12f,
                victim.GetComponent<Collider>().bounds.center);
            Assert.That(fixture.Chair.CommittedPlayerUses,
                Is.EqualTo(committedUses),
                "Swept and physical callbacks must share one use authority.");

            UnityEngine.Object.Destroy(victim.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FullSweptBoxHitsThreePathRunnersOnceAndCapsBeforeFourth()
        {
            MediumFixture fixture = null;
            yield return LoadMediumFixture(value => fixture = value);
            Vector3 aimPoint = fixture.Owner.transform.position +
                (fixture.Owner.transform.forward * 4f) +
                (fixture.Owner.transform.right * 0.8f) +
                (Vector3.up * 1.1f);

            Assert.That(fixture.Cargo.TryBeginCharge(aimPoint), Is.True,
                fixture.Cargo.LastMediumAttackRejection);
            Assert.That(fixture.Cargo.ChargeResolvedTarget, Is.Null,
                "This contract must not depend on the planning target.");
            float fullDeadline = Time.time + 1.2f;
            while (fixture.Cargo.Charge01 < 0.92f &&
                   Time.time < fullDeadline)
            {
                yield return null;
            }
            Assert.That(fixture.Cargo.TryReleaseCharge(aimPoint), Is.True,
                fixture.Cargo.LastMediumAttackRejection);
            uint revision = fixture.Cargo.AnimationCueRevision;
            yield return new WaitForSeconds(0.12f);
            Assert.That(fixture.Cargo.NotifyAnimationStateEntered(revision),
                Is.True);
            Assert.That(
                fixture.Cargo.CommitAnimationStrapAttackImpulse(revision),
                Is.True);

            ShowcaseRunnerAgent third = CreateDisabledRunnerClone(
                fixture.Normal,
                "P0_03_SWEEP_THIRD");
            ShowcaseRunnerAgent fourth = CreateDisabledRunnerClone(
                fixture.Normal,
                "P0_03_SWEEP_FOURTH");
            var orderedRunners = new[]
            {
                fixture.Normal,
                fixture.Elite,
                third,
                fourth
            };
            Collider chairCollider = fixture.Chair.GetComponent<Collider>();
            Assert.That(chairCollider, Is.Not.Null);
            Vector3 direction = Vector3.right;
            Vector3 startCenter = fixture.Owner.transform.position +
                (Vector3.up * 5f) - (direction * 2f);
            MoveRigidbodyColliderCenter(
                fixture.ChairBody,
                chairCollider,
                startCenter);
            for (int index = 0; index < orderedRunners.Length; index++)
            {
                ShowcaseRunnerAgent runner = orderedRunners[index];
                runner.enabled = false;
                Rigidbody runnerBody = runner.GetComponent<Rigidbody>();
                runnerBody.isKinematic = true;
                runnerBody.useGravity = false;
                MoveRunnerColliderCenter(
                    runner,
                    startCenter + direction * (1.5f + index * 1.6f));
            }

            GameObject duplicateChild = new GameObject(
                "P0_03_DUPLICATE_CHILD_COLLIDER");
            duplicateChild.transform.SetParent(
                fixture.Normal.transform,
                false);
            BoxCollider duplicateCollider =
                duplicateChild.AddComponent<BoxCollider>();
            duplicateCollider.size = new Vector3(0.7f, 1.4f, 0.7f);
            Physics.SyncTransforms();
            InvokePrivateVoid(fixture.Cargo, "CaptureMediumSweepPose");

            int usesBefore = fixture.Chair.CommittedPlayerUses;
            int[] healthBefore = orderedRunners
                .Select(runner => runner.Health.CurrentHealth)
                .ToArray();
            int acceptedContactEvents = 0;
            fixture.Pressure.CombatContactResolved += result =>
            {
                if (result.Accepted &&
                    result.Attacker == fixture.Chair.transform)
                {
                    acceptedContactEvents++;
                }
            };

            MoveRigidbodyColliderCenter(
                fixture.ChairBody,
                chairCollider,
                startCenter + direction * 7.2f);
            fixture.ChairBody.linearVelocity = direction * 14f;
            Physics.SyncTransforms();
            bool terminated = InvokePrivate<bool>(
                fixture.Cargo,
                "ResolveMediumSweepContacts");

            ShowcaseCargoChargeProfile profile =
                fixture.Cargo.ActiveAttackChargeProfile;
            int directDamage = Mathf.Max(
                1,
                Mathf.RoundToInt(25f * profile.DamageMultiplier));
            int secondDamage = Mathf.Max(
                1,
                Mathf.RoundToInt(directDamage * 0.75f));
            int thirdDamage = Mathf.Max(
                1,
                Mathf.RoundToInt(directDamage * 0.525f));
            Assert.That(terminated, Is.True);
            Assert.That(fixture.Cargo.MediumSweepHitCount, Is.EqualTo(3));
            Assert.That(fixture.Cargo.MediumSweepMaximumTargets, Is.EqualTo(3));
            Assert.That(fixture.Cargo.PrimarySweptVictim,
                Is.SameAs(fixture.Normal));
            Assert.That(fixture.Cargo.MediumSweepTerminationReason,
                Is.EqualTo(
                    ShowcaseCargoSweepTerminationReason.MaximumTargetsReached));
            Assert.That(fixture.Normal.Health.CurrentHealth,
                Is.EqualTo(healthBefore[0] - directDamage));
            Assert.That(fixture.Elite.Health.CurrentHealth,
                Is.EqualTo(healthBefore[1] - secondDamage));
            Assert.That(third.Health.CurrentHealth,
                Is.EqualTo(healthBefore[2] - thirdDamage));
            Assert.That(fourth.Health.CurrentHealth,
                Is.EqualTo(healthBefore[3]));
            Assert.That(acceptedContactEvents, Is.EqualTo(3),
                "Child colliders and the physical callback share one authority.");
            Assert.That(fixture.Chair.CommittedPlayerUses,
                Is.EqualTo(usesBefore + 1),
                "Three victims in one Full Sweep still consume one Chair use.");
            Assert.That(fixture.Normal.IsCargoSweepCarryActive, Is.True);
            Assert.That(fixture.Normal.CargoSweepCarrySequence,
                Is.EqualTo(fixture.Cargo.ActiveMediumSwingSequence));
            Assert.That(fixture.Cargo.RemainingImpactEnergy01,
                Is.LessThanOrEqualTo(
                    ShowcaseCargoSweepEnergyModel.ContinueThreshold01));

            int duplicateHealth = fixture.Normal.Health.CurrentHealth;
            int impactCount = fixture.Pressure.PhysicalImpactCount;
            fixture.Pressure.RegisterPhysicalImpact(
                fixture.Normal,
                fixture.Chair,
                14f,
                direction * 14f,
                fixture.Normal.GetComponent<Collider>().bounds.center);
            Assert.That(fixture.Normal.Health.CurrentHealth,
                Is.EqualTo(duplicateHealth));
            Assert.That(fixture.Pressure.PhysicalImpactCount,
                Is.EqualTo(impactCount));
            Assert.That(acceptedContactEvents, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator FullSweepLaunchesThreeVictimsWithStrictOrdinalFalloff()
        {
            MediumFixture fixture = null;
            yield return LoadMediumFixture(value => fixture = value);
            Vector3 aimPoint = fixture.Owner.transform.position +
                (fixture.Owner.transform.forward * 4f) +
                (fixture.Owner.transform.right * 0.8f) +
                (Vector3.up * 1.1f);

            Assert.That(fixture.Cargo.TryBeginCharge(aimPoint), Is.True,
                fixture.Cargo.LastMediumAttackRejection);
            float fullDeadline = Time.time + 1.2f;
            while (fixture.Cargo.Charge01 < 0.98f &&
                   Time.time < fullDeadline)
            {
                yield return null;
            }
            Assert.That(fixture.Cargo.TryReleaseCharge(aimPoint), Is.True,
                fixture.Cargo.LastMediumAttackRejection);
            uint revision = fixture.Cargo.AnimationCueRevision;
            yield return new WaitForSeconds(0.12f);
            Assert.That(fixture.Cargo.NotifyAnimationStateEntered(revision),
                Is.True);
            Assert.That(
                fixture.Cargo.CommitAnimationStrapAttackImpulse(revision),
                Is.True);

            ShowcaseRunnerAgent first = CreateStationaryRunnerClone(
                fixture.Normal,
                "P0_03_FALLOFF_FIRST",
                fixture.Pressure,
                fixture.Owner.transform,
                200);
            ShowcaseRunnerAgent second = CreateStationaryRunnerClone(
                fixture.Normal,
                "P0_03_FALLOFF_SECOND",
                fixture.Pressure,
                fixture.Owner.transform,
                200);
            ShowcaseRunnerAgent third = CreateStationaryRunnerClone(
                fixture.Normal,
                "P0_03_FALLOFF_THIRD_LETHAL",
                fixture.Pressure,
                fixture.Owner.transform,
                10);
            MoveRunnerColliderCenter(
                first,
                new Vector3(-18f, 12f, -18f));
            MoveRunnerColliderCenter(
                second,
                new Vector3(0f, 12f, 18f));
            MoveRunnerColliderCenter(
                third,
                new Vector3(18f, 12f, -18f));
            Vector3 firstStart = first.transform.position;
            Vector3 secondStart = second.transform.position;
            Vector3 thirdStart = third.transform.position;
            Vector3 impactVelocity = Vector3.right * 14f;

            Assert.That(fixture.Pressure.TryRegisterCargoSweepImpact(
                    fixture.Cargo,
                    first,
                    fixture.Chair,
                    14f,
                    impactVelocity,
                    first.GetComponent<Collider>().bounds.center),
                Is.True);
            Assert.That(fixture.Pressure.TryRegisterCargoSweepImpact(
                    fixture.Cargo,
                    second,
                    fixture.Chair,
                    14f,
                    impactVelocity,
                    second.GetComponent<Collider>().bounds.center),
                Is.True);
            Assert.That(fixture.Pressure.TryRegisterCargoSweepImpact(
                    fixture.Cargo,
                    third,
                    fixture.Chair,
                    14f,
                    impactVelocity,
                    third.GetComponent<Collider>().bounds.center),
                Is.True);

            Assert.That(first.IsCargoSweepCarryActive, Is.True);
            Assert.That(second.IsCargoSweepReactionProtected, Is.True);
            Assert.That(third.IsDead, Is.True,
                "The third target intentionally verifies lethal Sweep motion.");
            Assert.That(third.IsDeathMomentumActive, Is.True,
                "A lethal second/third Sweep hit must still launch the victim.");
            Assert.That(fixture.Cargo.MediumSweepHitCount, Is.EqualTo(3));

            fixture.ChairBody.constraints = RigidbodyConstraints.FreezeAll;
            for (int step = 0; step < 24; step++)
            {
                yield return new WaitForFixedUpdate();
            }

            float firstDistance = Vector3.ProjectOnPlane(
                first.transform.position - firstStart,
                Vector3.up).magnitude;
            float secondDistance = Vector3.ProjectOnPlane(
                second.transform.position - secondStart,
                Vector3.up).magnitude;
            float thirdDistance = Vector3.ProjectOnPlane(
                third.transform.position - thirdStart,
                Vector3.up).magnitude;
            Assert.That(firstDistance,
                Is.GreaterThan(secondDistance + 0.2f),
                $"first={firstDistance:0.00}, second={secondDistance:0.00}");
            Assert.That(secondDistance,
                Is.GreaterThan(thirdDistance + 0.2f),
                $"second={secondDistance:0.00}, third={thirdDistance:0.00}");
            Assert.That(thirdDistance, Is.GreaterThan(0.8f),
                "Every accepted Full Sweep victim must visibly move, even " +
                "when the attenuated third hit is lethal.");

            UnityEngine.Object.Destroy(first.gameObject);
            UnityEngine.Object.Destroy(second.gameObject);
            UnityEngine.Object.Destroy(third.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PrimaryCarryEnemyCollisionTransfersHitAndSweepContinues()
        {
            MediumFixture fixture = null;
            yield return LoadMediumFixture(value => fixture = value);
            fixture.Normal.ConfigureAutonomousMovementSuppressed(true);
            fixture.Elite.ConfigureAutonomousMovementSuppressed(true);
            fixture.Normal.enabled = true;
            fixture.Elite.enabled = true;
            Vector3 aimPoint = fixture.Owner.transform.position +
                (fixture.Owner.transform.forward * 4f) +
                (fixture.Owner.transform.right * 0.8f) +
                (Vector3.up * 1.1f);

            Assert.That(fixture.Cargo.TryBeginCharge(aimPoint), Is.True,
                fixture.Cargo.LastMediumAttackRejection);
            float fullDeadline = Time.time + 1.2f;
            while (fixture.Cargo.Charge01 < 0.92f &&
                   Time.time < fullDeadline)
            {
                yield return null;
            }
            Assert.That(fixture.Cargo.TryReleaseCharge(aimPoint), Is.True,
                fixture.Cargo.LastMediumAttackRejection);
            uint revision = fixture.Cargo.AnimationCueRevision;
            yield return new WaitForSeconds(0.12f);
            Assert.That(fixture.Cargo.NotifyAnimationStateEntered(revision),
                Is.True);
            Assert.That(
                fixture.Cargo.CommitAnimationStrapAttackImpulse(revision),
                Is.True);

            Collider chairCollider = fixture.Chair.GetComponent<Collider>();
            Vector3 direction = Vector3.right;
            Vector3 startCenter = fixture.Owner.transform.position +
                (Vector3.up * 5f) - (direction * 2f);
            MoveRigidbodyColliderCenter(
                fixture.ChairBody,
                chairCollider,
                startCenter);
            MoveRunnerColliderCenter(
                fixture.Normal,
                startCenter + direction * 1.5f);
            MoveRunnerColliderCenter(
                fixture.Elite,
                startCenter + direction * 8f);
            Physics.SyncTransforms();
            InvokePrivateVoid(fixture.Cargo, "CaptureMediumSweepPose");
            MoveRigidbodyColliderCenter(
                fixture.ChairBody,
                chairCollider,
                startCenter + direction * 2.5f);
            fixture.ChairBody.linearVelocity = direction * 14f;
            Physics.SyncTransforms();
            InvokePrivate<bool>(
                fixture.Cargo,
                "ResolveMediumSweepContacts");

            Assert.That(fixture.Cargo.MediumSweepHitCount, Is.EqualTo(1));
            Assert.That(fixture.Cargo.PrimarySweptVictim,
                Is.SameAs(fixture.Normal));
            ShowcaseObjectImpactEvaluation primaryImpact =
                fixture.Normal.LastObjectImpactEvaluation;
            Assert.That(primaryImpact.IsValid, Is.True);
            MoveRunnerColliderCenter(
                fixture.Elite,
                fixture.Normal.GetComponent<Collider>().bounds.center +
                    direction * 1.1f);
            Physics.SyncTransforms();
            int eliteHealthBefore = fixture.Elite.Health.CurrentHealth;
            int enemyCollisionEvents = 0;
            fixture.Pressure.EnemyCollisionResolved += _ =>
                enemyCollisionEvents++;

            bool collisionAccepted = fixture.Pressure.RegisterSecondaryImpact(
                fixture.Normal,
                fixture.Elite.GetComponent<Collider>(),
                primaryImpact);
            int eliteHealthAfter = fixture.Elite.Health.CurrentHealth;

            Assert.That(collisionAccepted, Is.True);
            Assert.That(enemyCollisionEvents, Is.EqualTo(1));
            Assert.That(fixture.Pressure.EnemyCollisionCount, Is.EqualTo(1));
            Assert.That(fixture.Pressure.LastSecondaryImpactResult.Kind,
                Is.EqualTo(ShowcaseSecondaryImpactKind.Enemy));
            Assert.That(eliteHealthAfter, Is.LessThan(eliteHealthBefore));
            Assert.That(fixture.Normal.State,
                Is.EqualTo(ShowcaseRunnerState.Staggered));
            Assert.That(fixture.Elite.State,
                Is.EqualTo(ShowcaseRunnerState.Staggered));
            Assert.That(fixture.Cargo.MediumSweepHitCount, Is.EqualTo(2));
            Assert.That(fixture.Cargo.IsMediumSweepTerminated, Is.False,
                "Enemy-to-Enemy contact must not cancel the Chair's remaining " +
                "Full Sweep path.");
            Assert.That(fixture.Normal.IsCargoSweepCarryActive, Is.True);
            Assert.That(fixture.Elite.IsCargoSweepReactionProtected, Is.True,
                "The transferred second ordinal must keep its launch even when " +
                "the hit-reaction animation enters late.");

            ShowcaseRunnerAgent third = CreateStationaryRunnerClone(
                fixture.Normal,
                "P0_03_ENEMY_COLLISION_THIRD",
                fixture.Pressure,
                fixture.Owner.transform,
                200);
            MoveRunnerColliderCenter(third, new Vector3(18f, 12f, -18f));
            Vector3 normalStart = fixture.Normal.transform.position;
            Vector3 eliteStart = fixture.Elite.transform.position;
            Vector3 thirdStart = third.transform.position;
            Assert.That(fixture.Pressure.TryRegisterCargoSweepImpact(
                    fixture.Cargo,
                    third,
                    fixture.Chair,
                    14f,
                    direction * 14f,
                    third.GetComponent<Collider>().bounds.center),
                Is.True,
                "The third path victim must remain authorizable after the " +
                "first victim collides with the second.");
            Assert.That(fixture.Cargo.MediumSweepHitCount, Is.EqualTo(3));
            Assert.That(fixture.Cargo.IsMediumSweepTerminated, Is.True);
            Assert.That(fixture.Cargo.MediumSweepTerminationReason,
                Is.EqualTo(
                    ShowcaseCargoSweepTerminationReason.MaximumTargetsReached));

            fixture.ChairBody.constraints = RigidbodyConstraints.FreezeAll;
            for (int step = 0; step < 24; step++)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(Vector3.ProjectOnPlane(
                    fixture.Normal.transform.position - normalStart,
                    Vector3.up).magnitude,
                Is.GreaterThan(0.2f));
            Assert.That(Vector3.ProjectOnPlane(
                    fixture.Elite.transform.position - eliteStart,
                    Vector3.up).magnitude,
                Is.GreaterThan(0.2f));
            Assert.That(Vector3.ProjectOnPlane(
                    third.transform.position - thirdStart,
                    Vector3.up).magnitude,
                Is.GreaterThan(0.2f));

            bool duplicateAccepted = fixture.Pressure.RegisterSecondaryImpact(
                fixture.Normal,
                fixture.Elite.GetComponent<Collider>(),
                primaryImpact);
            Assert.That(duplicateAccepted, Is.False);
            Assert.That(fixture.Elite.Health.CurrentHealth,
                Is.EqualTo(eliteHealthAfter));
            Assert.That(enemyCollisionEvents, Is.EqualTo(1));

            UnityEngine.Object.Destroy(third.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SweptBoxStopsAtStaticWallBeforeRunnerBehindIt()
        {
            MediumFixture fixture = null;
            yield return LoadMediumFixture(value => fixture = value);
            Vector3 aimPoint = fixture.Owner.transform.position +
                (fixture.Owner.transform.forward * 4f) +
                (fixture.Owner.transform.right * 0.8f) +
                (Vector3.up * 1.1f);
            Assert.That(fixture.Cargo.TryBeginCharge(aimPoint), Is.True,
                fixture.Cargo.LastMediumAttackRejection);
            Assert.That(fixture.Cargo.TryReleaseCharge(aimPoint), Is.True,
                fixture.Cargo.LastMediumAttackRejection);
            uint revision = fixture.Cargo.AnimationCueRevision;
            yield return new WaitForSeconds(0.12f);
            Assert.That(fixture.Cargo.NotifyAnimationStateEntered(revision),
                Is.True);
            Assert.That(
                fixture.Cargo.CommitAnimationStrapAttackImpulse(revision),
                Is.True);

            Collider chairCollider = fixture.Chair.GetComponent<Collider>();
            Vector3 direction = Vector3.right;
            Vector3 startCenter = fixture.Owner.transform.position +
                (Vector3.up * 5f) - (direction * 2f);
            MoveRigidbodyColliderCenter(
                fixture.ChairBody,
                chairCollider,
                startCenter);
            MoveRunnerColliderCenter(
                fixture.Normal,
                startCenter + direction * 4.5f);
            fixture.Elite.transform.position += Vector3.forward * 20f;
            GameObject wallObject = new GameObject("P0_03_SWEEP_WALL");
            wallObject.transform.position =
                startCenter + direction * 2.5f;
            BoxCollider wallCollider = wallObject.AddComponent<BoxCollider>();
            wallCollider.size = new Vector3(0.3f, 3f, 3f);
            Physics.SyncTransforms();
            InvokePrivateVoid(fixture.Cargo, "CaptureMediumSweepPose");

            int healthBefore = fixture.Normal.Health.CurrentHealth;
            MoveRigidbodyColliderCenter(
                fixture.ChairBody,
                chairCollider,
                startCenter + direction * 6f);
            fixture.ChairBody.linearVelocity = direction * 14f;
            Physics.SyncTransforms();
            bool terminated = InvokePrivate<bool>(
                fixture.Cargo,
                "ResolveMediumSweepContacts");

            Assert.That(terminated, Is.True);
            Assert.That(fixture.Cargo.IsMediumSweepTerminated, Is.True);
            Assert.That(fixture.Cargo.MediumSweepTerminationReason,
                Is.EqualTo(ShowcaseCargoSweepTerminationReason.WorldObstacle));
            Assert.That(fixture.Cargo.MediumSweepTerminationObstacle,
                Is.SameAs(wallCollider));
            Assert.That(fixture.Cargo.MediumSweepHitCount, Is.Zero);
            Assert.That(fixture.Normal.Health.CurrentHealth,
                Is.EqualTo(healthBefore));
            Assert.That(
                Vector3.ProjectOnPlane(
                    fixture.ChairBody.linearVelocity,
                    Vector3.up).magnitude,
                Is.LessThan(2f));

            yield return null;
            Assert.That(fixture.Cargo.Phase,
                Is.EqualTo(ShowcaseCargoStrapPhase.MediumRecovering));
        }

        [UnityTest]
        public IEnumerator CloseRunnerUsesNormalMeleeComboWhileChairStaysTensioned()
        {
            MediumFixture fixture = null;
            yield return LoadMediumFixture(value => fixture = value);
            PositionCloseRangeTarget(fixture, 0.72f);

            int healthBefore = fixture.Normal.Health.CurrentHealth;
            Assert.That(fixture.Interaction.WeaponSelection,
                Is.EqualTo(PrototypeWeaponSelection.ObjectWeapon));
            Assert.That(fixture.Cargo.Phase,
                Is.EqualTo(ShowcaseCargoStrapPhase.Tensioned));

            Assert.That(fixture.Interaction.ProcessAttackPressed(), Is.True,
                "A runner inside melee range must own LMB before Chair Charge.");
            Assert.That(fixture.Cargo.IsCharging, Is.False);
            Assert.That(fixture.Cargo.AttackReleaseCount, Is.Zero);
            Assert.That(fixture.Combat.IsMeleeActionActive, Is.True);
            Assert.That(fixture.Combat.IsStrappedMeleeActive, Is.True);
            Assert.That(fixture.Combat.IsCloseRangeDisengageActive, Is.False);
            Assert.That(fixture.Combat.ActiveMeleeTarget,
                Is.SameAs(fixture.Normal));
            Assert.That(fixture.Combat.ActiveMeleeStep, Is.EqualTo(1),
                "Close-range Object Weapon input must enter the same Jab-first " +
                "combo used while unarmed.");
            Assert.That(fixture.Combat.CloseRangeDisengageCount, Is.Zero);
            SetSnapshot(fixture.Input, new LogicalInputSnapshot(
                Vector2.zero,
                Vector2.zero,
                false,
                false,
                true,
                false,
                false,
                false));
            InvokePrivateVoid(fixture.Interaction, "Update");
            SetSnapshot(fixture.Input, default);
            Assert.That(fixture.Combat.HasBufferedMelee, Is.True);

            fixture.Combat.NotifyAnimationAttackWindowOpened();
            MethodInfo commitBoneHit = typeof(ShowcasePlayerCombatController)
                .GetMethod(
                    "TryCommitBoneHit",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(commitBoneHit, Is.Not.Null);
            Collider runnerCollider = fixture.Normal.GetComponent<Collider>();
            object committed = commitBoneHit.Invoke(
                fixture.Combat,
                new object[]
                {
                    fixture.Normal,
                    fixture.Combat.ActiveAttackSequence,
                    runnerCollider.bounds.center
                });
            Assert.That(committed, Is.EqualTo(true));

            Assert.That(fixture.Normal.Health.CurrentHealth,
                Is.EqualTo(healthBefore - 15));
            Assert.That(fixture.Normal.RequestedReactionDistance,
                Is.LessThan(0.3f),
                "The old one-off disengage shove must not replace normal melee " +
                "hit reaction movement.");
            Assert.That(fixture.Combat.CloseRangeDisengageHitCount,
                Is.Zero);
            Assert.That(fixture.Cargo.Phase,
                Is.EqualTo(ShowcaseCargoStrapPhase.Tensioned));
            Assert.That(fixture.Interaction.WeaponSelection,
                Is.EqualTo(PrototypeWeaponSelection.ObjectWeapon),
                "The Chair must remain tensioned and selected after the punch.");
        }

        [UnityTest]
        public IEnumerator FullSlamUsesActualContactForDirectOuterAndRecoveryTiming()
        {
            MediumFixture fixture = null;
            yield return LoadMediumFixture(value => fixture = value);

            Vector3 player = fixture.Owner.transform.position;
            Vector3 impactPoint = player +
                (fixture.Owner.transform.forward * 1.35f);
            impactPoint.y = player.y - 0.9f;
            fixture.Normal.transform.position = new Vector3(
                impactPoint.x,
                fixture.NormalY,
                impactPoint.z);
            fixture.Elite.transform.position = new Vector3(
                impactPoint.x,
                fixture.EliteY,
                impactPoint.z) + (fixture.Owner.transform.right * 1.4f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            Assert.That(fixture.Cargo.TryBeginCharge(impactPoint), Is.True,
                fixture.Cargo.LastMediumAttackRejection);
            Assert.That(fixture.Cargo.CurrentAttackResolution.AttackType,
                Is.EqualTo(ShowcaseCargoAttackType.OverheadSlam));
            Assert.That(fixture.Cargo.CurrentAttackResolution.Reason,
                Is.EqualTo(
                    ShowcaseCargoAttackResolutionReason.ExplicitDownwardSlam));

            float fullDeadline = Time.time + 1.2f;
            while (fixture.Cargo.Charge01 < 0.92f && Time.time < fullDeadline)
            {
                yield return null;
            }
            Assert.That(fixture.Cargo.Charge01, Is.GreaterThanOrEqualTo(0.9f));
            Assert.That(fixture.Cargo.TryReleaseCharge(impactPoint), Is.True,
                fixture.Cargo.LastMediumAttackRejection);
            ShowcaseCargoChargeProfile profile =
                fixture.Cargo.ActiveAttackChargeProfile;
            uint attackRevision = fixture.Cargo.AnimationCueRevision;
            Assert.That(fixture.Cargo.ActiveAttackType,
                Is.EqualTo(ShowcaseCargoAttackType.OverheadSlam));
            Assert.That(fixture.Cargo.AnimationCue,
                Is.EqualTo(ShowcaseCargoStrapAnimationCue.Slam));
            Assert.That(profile.Tier, Is.EqualTo(ShowcaseCargoChargeTier.Full));
            Assert.That(profile.AllowsKnockdown, Is.True);

            yield return new WaitForSeconds(0.12f);
            Assert.That(
                fixture.Cargo.NotifyAnimationStateEntered(attackRevision),
                Is.True);
            Assert.That(
                fixture.Cargo.CommitAnimationStrapAttackImpulse(attackRevision),
                Is.True);
            Assert.That(fixture.Cargo.LastReleaseToObjectAccelerationSeconds,
                Is.InRange(0.1f, 0.2f));
            TestContext.WriteLine(
                $"Full Slam release-to-acceleration: " +
                $"{fixture.Cargo.LastReleaseToObjectAccelerationSeconds:F3}s");

            RigidbodyConstraints attackConstraints = fixture.ChairBody.constraints;
            fixture.ChairBody.constraints = RigidbodyConstraints.FreezeAll;
            float contactDeadline = Time.time + 0.43f;
            while (Time.time < contactDeadline)
            {
                yield return null;
            }

            fixture.ChairBody.constraints = attackConstraints;
            Animator normalAnimator =
                fixture.Normal.GetComponentInChildren<Animator>(true);
            Assert.That(normalAnimator, Is.Not.Null);
            Assert.That(normalAnimator.isHuman, Is.True);
            Transform head = normalAnimator.GetBoneTransform(HumanBodyBones.Head);
            Assert.That(head, Is.Not.Null);
            Collider chairCollider = fixture.Chair.GetComponent<Collider>();
            Assert.That(chairCollider, Is.Not.Null);
            const float headSurfaceGap = 0.24f;
            Vector3 desiredChairCenter = head.position +
                (Vector3.up *
                 (chairCollider.bounds.extents.y + headSurfaceGap));
            MoveRigidbodyColliderCenter(
                fixture.ChairBody,
                chairCollider,
                desiredChairCenter);
            ShowcaseBoneCombatRig normalRig =
                fixture.Normal.GetComponent<ShowcaseBoneCombatRig>();
            Assert.That(normalRig, Is.Not.Null);
            Vector3 stationaryColliderCenter = chairCollider.bounds.center;
            Assert.That(normalRig.TryResolveExternalColliderContact(
                    chairCollider,
                    stationaryColliderCenter,
                    out _),
                Is.False,
                "The ordinary combat hurtbox must stay unchanged for non-Slam " +
                "contacts.");
            fixture.ChairBody.linearVelocity = Vector3.down * 12f;
            SetPrivateField(fixture.Cargo, "slamDescending", true);
            SetPrivateField(fixture.Cargo, "slamDownwardTravel", 0.8f);
            SetPrivateField(
                fixture.Cargo,
                "slamPreviousColliderCenter",
                stationaryColliderCenter);
            SetPrivateField(
                fixture.Cargo,
                "slamColliderSweepInitialized",
                true);

            int normalHealthBefore = fixture.Normal.Health.CurrentHealth;
            int eliteHealthBefore = fixture.Elite.Health.CurrentHealth;
            int impactCountBefore = fixture.Pressure.PhysicalImpactCount;
            bool logicalHurtboxContact = InvokePrivate<bool>(
                fixture.Cargo,
                "TryResolveSlamHurtboxContact");
            Assert.That(logicalHurtboxContact, Is.True,
                "Descending Chair must resolve the runner Head hurtbox even " +
                "without a physical head collider.");

            int directDamage = Mathf.Max(
                1,
                Mathf.RoundToInt(25f * profile.DamageMultiplier));
            int outerDamage = Mathf.Max(
                1,
                Mathf.RoundToInt(directDamage * 0.35f));
            Assert.That(fixture.Cargo.SlamImpactCount, Is.EqualTo(1));
            Assert.That(fixture.Pressure.PhysicalImpactCount,
                Is.EqualTo(impactCountBefore + 2));
            Assert.That(fixture.Normal.Health.CurrentHealth,
                Is.EqualTo(normalHealthBefore - directDamage));
            Assert.That(fixture.Elite.Health.CurrentHealth,
                Is.EqualTo(eliteHealthBefore - outerDamage));
            Assert.That(fixture.Normal.State,
                Is.EqualTo(ShowcaseRunnerState.KnockedDown));
            Assert.That(fixture.Elite.State,
                Is.EqualTo(ShowcaseRunnerState.Staggered));
            Assert.That(fixture.Cargo.LastReleaseToContactSeconds,
                Is.InRange(0.5f, 0.9f));
            TestContext.WriteLine(
                $"Full Slam release-to-contact: " +
                $"{fixture.Cargo.LastReleaseToContactSeconds:F3}s");

            bool duplicateLogicalContact = InvokePrivate<bool>(
                fixture.Cargo,
                "TryResolveSlamHurtboxContact");
            Assert.That(duplicateLogicalContact, Is.False);
            Assert.That(fixture.Cargo.SlamImpactCount, Is.EqualTo(1));
            Assert.That(fixture.Normal.Health.CurrentHealth,
                Is.EqualTo(normalHealthBefore - directDamage));
            Assert.That(fixture.Elite.Health.CurrentHealth,
                Is.EqualTo(eliteHealthBefore - outerDamage));

            fixture.ChairBody.constraints = RigidbodyConstraints.FreezeAll;
            Assert.That(
                fixture.Cargo.CompleteAnimationStrapAction(attackRevision),
                Is.True);
            float recoveryDeadline = Time.time + 0.65f;
            while (fixture.Cargo.LastImpactToControlRecoverySeconds < 0f &&
                   Time.time < recoveryDeadline)
            {
                yield return null;
            }
            Assert.That(fixture.Cargo.LastImpactToControlRecoverySeconds,
                Is.InRange(0.3f, 0.55f));
            TestContext.WriteLine(
                $"Full Slam impact-to-control-recovery: " +
                $"{fixture.Cargo.LastImpactToControlRecoverySeconds:F3}s");
        }

        private static IEnumerator LoadMediumFixture(
            Action<MediumFixture> completed,
            bool waitForRearReady = true)
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S7ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;

            ShowcaseCargoStrapController cargo =
                SceneComponents<ShowcaseCargoStrapController>().Single();
            PrototypeInteractionController interaction =
                SceneComponents<PrototypeInteractionController>().Single();
            PrototypeInteractionStateOwner owner =
                SceneComponents<PrototypeInteractionStateOwner>().Single();
            InputSystemLogicalInputAdapter input =
                SceneComponents<InputSystemLogicalInputAdapter>().Single();
            ThirdPersonLocomotion locomotion =
                owner.GetComponent<ThirdPersonLocomotion>();
            ShowcasePlayerCombatController combat =
                owner.GetComponent<ShowcasePlayerCombatController>();
            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            PrototypeInteractionTarget chair =
                SceneComponents<PrototypeInteractionTarget>()
                    .First(target => target.Role ==
                        PrototypeInteractionRole.DirectChair);
            ShowcaseRunnerAgent normal = pressure.Runners
                .Single(runner => !runner.IsElite);
            ShowcaseRunnerAgent elite = pressure.Runners
                .Single(runner => runner.IsElite);
            float normalY = normal.transform.position.y;
            float eliteY = elite.transform.position.y;

            ShowcaseCharacterAnimationDriver playerAnimation =
                SceneComponents<ShowcaseCharacterAnimationDriver>()
                    .FirstOrDefault(driver => driver.Role ==
                        ShowcaseCharacterAnimationRole.Player);
            if (playerAnimation != null)
            {
                playerAnimation.enabled = false;
            }
            input.enabled = false;
            SetSnapshot(input, default);
            normal.enabled = false;
            elite.enabled = false;

            owner.transform.position = chair.transform.position +
                new Vector3(0f, 0.9f, -2f);
            normal.transform.position = owner.transform.position +
                new Vector3(12f, 0f, 12f);
            normal.transform.position = new Vector3(
                normal.transform.position.x,
                normalY,
                normal.transform.position.z);
            elite.transform.position = owner.transform.position +
                new Vector3(-12f, 0f, 12f);
            elite.transform.position = new Vector3(
                elite.transform.position.x,
                eliteY,
                elite.transform.position.z);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            Assert.That(cargo.TryBeginDirectAttach(chair), Is.True);
            CompleteAttachSequence(cargo);
            if (waitForRearReady)
            {
                float rearDeadline = Time.time + 2.5f;
                while (!cargo.IsMediumRearReady && Time.time < rearDeadline)
                {
                    yield return new WaitForFixedUpdate();
                }
                Assert.That(cargo.IsMediumRearReady, Is.True);
                Assert.That(cargo.IsMediumFloorSupported, Is.True);
            }

            completed(new MediumFixture
            {
                Cargo = cargo,
                Interaction = interaction,
                Owner = owner,
                Input = input,
                Locomotion = locomotion,
                Combat = combat,
                Pressure = pressure,
                PlayerAnimation = playerAnimation,
                Chair = chair,
                ChairBody = chair.GetComponent<Rigidbody>(),
                Normal = normal,
                Elite = elite,
                NormalY = normalY,
                EliteY = eliteY
            });
        }

        private static void PositionCloseRangeTarget(
            MediumFixture fixture,
            float forwardDistance)
        {
            Vector3 targetPosition = fixture.Owner.transform.position +
                fixture.Owner.transform.forward * forwardDistance;
            fixture.Normal.transform.position = new Vector3(
                targetPosition.x,
                fixture.NormalY,
                targetPosition.z);
            fixture.Elite.transform.position = fixture.Owner.transform.position +
                new Vector3(-12f, 0f, 12f);
            fixture.Elite.transform.position = new Vector3(
                fixture.Elite.transform.position.x,
                fixture.EliteY,
                fixture.Elite.transform.position.z);
            Physics.SyncTransforms();
        }

        private static void PositionOpenSweepTarget(
            MediumFixture fixture,
            float forwardDistance,
            float lateralDistance)
        {
            Vector3 targetPosition = fixture.Owner.transform.position +
                (fixture.Owner.transform.forward * forwardDistance) +
                (fixture.Owner.transform.right * lateralDistance);
            fixture.Normal.transform.position = new Vector3(
                targetPosition.x,
                fixture.NormalY,
                targetPosition.z);
            fixture.Elite.transform.position = fixture.Owner.transform.position +
                new Vector3(12f, 0f, 12f);
            fixture.Elite.transform.position = new Vector3(
                fixture.Elite.transform.position.x,
                fixture.EliteY,
                fixture.Elite.transform.position.z);
            Physics.SyncTransforms();
        }

        private static ShowcaseRunnerAgent CreateDisabledRunnerClone(
            ShowcaseRunnerAgent source,
            string cloneName)
        {
            GameObject cloneObject = UnityEngine.Object.Instantiate(
                source.gameObject);
            cloneObject.name = cloneName;
            ShowcaseRunnerAgent clone =
                cloneObject.GetComponent<ShowcaseRunnerAgent>();
            Assert.That(clone, Is.Not.Null);
            clone.enabled = false;
            Rigidbody body = clone.GetComponent<Rigidbody>();
            Assert.That(body, Is.Not.Null);
            body.isKinematic = true;
            body.useGravity = false;
            return clone;
        }

        private static ShowcaseRunnerAgent CreateStationaryRunnerClone(
            ShowcaseRunnerAgent source,
            string cloneName,
            ShowcaseRunnerPressureController pressure,
            Transform player,
            int maximumHealth)
        {
            GameObject cloneObject = UnityEngine.Object.Instantiate(
                source.gameObject);
            cloneObject.name = cloneName;
            ShowcaseRunnerAgent clone =
                cloneObject.GetComponent<ShowcaseRunnerAgent>();
            ShowcaseCombatHealth health =
                cloneObject.GetComponent<ShowcaseCombatHealth>();
            Rigidbody body = cloneObject.GetComponent<Rigidbody>();
            Assert.That(clone, Is.Not.Null);
            Assert.That(health, Is.Not.Null);
            Assert.That(body, Is.Not.Null);
            body.isKinematic = true;
            body.useGravity = false;
            health.Configure(
                ShowcaseCombatTeam.Enemy,
                Mathf.Max(1, maximumHealth),
                0f);
            clone.Configure(
                cloneName,
                pressure,
                body,
                player,
                authoredHealth: health,
                authoredElite: false);
            clone.ConfigureAutonomousMovementSuppressed(true);
            clone.ConfigureBatchModeDisabled(false);
            clone.enabled = true;
            return clone;
        }

        private static void MoveRunnerColliderCenter(
            ShowcaseRunnerAgent runner,
            Vector3 desiredCenter)
        {
            Collider collider = runner.GetComponent<Collider>();
            Assert.That(collider, Is.Not.Null);
            runner.transform.position += desiredCenter - collider.bounds.center;
            Rigidbody body = runner.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.position = runner.transform.position;
            }
            Physics.SyncTransforms();
        }

        private static void MoveRigidbodyColliderCenter(
            Rigidbody body,
            Collider collider,
            Vector3 desiredCenter)
        {
            Assert.That(body, Is.Not.Null);
            Assert.That(collider, Is.Not.Null);
            body.position += desiredCenter - collider.bounds.center;
            body.transform.position = body.position;
            Physics.SyncTransforms();
        }

        private static void CompleteAttachSequence(ShowcaseCargoStrapController cargo)
        {
            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Prepare));
            uint prepareRevision = cargo.AnimationCueRevision;
            Assert.That(cargo.NotifyAnimationStateEntered(prepareRevision), Is.True);
            Assert.That(cargo.CompleteAnimationStrapAction(prepareRevision), Is.True);

            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Throw));
            uint throwRevision = cargo.AnimationCueRevision;
            Assert.That(cargo.NotifyAnimationStateEntered(throwRevision), Is.True);
            Assert.That(cargo.CommitAnimationStrapThrowRelease(throwRevision), Is.True);
            Assert.That(cargo.CommitAnimationStrapAttachContact(throwRevision), Is.True);
            Assert.That(cargo.CompleteAnimationStrapAction(throwRevision), Is.True);

            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Pull));
            uint pullRevision = cargo.AnimationCueRevision;
            Assert.That(cargo.NotifyAnimationStateEntered(pullRevision), Is.True);
            Assert.That(cargo.CommitAnimationStrapPullComplete(pullRevision), Is.True);
            Assert.That(cargo.Phase,
                Is.EqualTo(ShowcaseCargoStrapPhase.Tensioned));
        }

        private static T[] SceneComponents<T>() where T : Component
        {
            Scene scene = SceneManager.GetActiveScene();
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }

        private static void SetSnapshot(
            InputSystemLogicalInputAdapter input,
            LogicalInputSnapshot snapshot)
        {
            PropertyInfo property = typeof(InputSystemLogicalInputAdapter)
                .GetProperty(nameof(InputSystemLogicalInputAdapter.Snapshot));
            MethodInfo setter = property?.GetSetMethod(true);
            Assert.That(setter, Is.Not.Null);
            setter.Invoke(input, new object[] { snapshot });
        }

        private static void SetPrivateField<T>(
            object target,
            string fieldName,
            T value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }

        private static T GetPrivateField<T>(
            object target,
            string fieldName)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            object value = field.GetValue(target);
            Assert.That(value, Is.InstanceOf<T>());
            return (T)value;
        }

        private static T InvokePrivate<T>(
            object target,
            string methodName,
            params object[] arguments)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName);
            object result = method.Invoke(target, arguments);
            Assert.That(result, Is.InstanceOf<T>());
            return (T)result;
        }

        private static void InvokePrivateVoid(
            object target,
            string methodName,
            params object[] arguments)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName);
            method.Invoke(target, arguments);
        }
    }
}
