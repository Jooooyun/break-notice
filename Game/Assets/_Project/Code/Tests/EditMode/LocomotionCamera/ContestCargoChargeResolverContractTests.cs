using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestCargoChargeResolverContractTests
    {
        [Test]
        public void ChargeCurve_UsesLockedQuickPowerAndFullBands()
        {
            ShowcaseCargoChargeProfile quick =
                ShowcaseCargoChargeModel.Evaluate(0.349f);
            ShowcaseCargoChargeProfile power =
                ShowcaseCargoChargeModel.Evaluate(0.35f);
            ShowcaseCargoChargeProfile fullEntry =
                ShowcaseCargoChargeModel.Evaluate(0.75f);
            ShowcaseCargoChargeProfile full =
                ShowcaseCargoChargeModel.Evaluate(1f);

            Assert.That(quick.Tier, Is.EqualTo(ShowcaseCargoChargeTier.Quick));
            Assert.That(quick.DamageMultiplier, Is.EqualTo(0.7f).Within(0.001f));
            Assert.That(quick.KnockbackMultiplier, Is.EqualTo(0.6f).Within(0.001f));
            Assert.That(quick.AllowsKnockdown, Is.False);
            Assert.That(power.Tier, Is.EqualTo(ShowcaseCargoChargeTier.Power));
            Assert.That(power.DamageMultiplier, Is.EqualTo(1f).Within(0.001f));
            Assert.That(power.KnockbackMultiplier, Is.EqualTo(1f).Within(0.001f));
            Assert.That(power.AllowsKnockdown, Is.False);
            Assert.That(fullEntry.Tier, Is.EqualTo(ShowcaseCargoChargeTier.Full));
            Assert.That(fullEntry.DamageMultiplier,
                Is.EqualTo(1.35f).Within(0.001f));
            Assert.That(fullEntry.KnockbackMultiplier,
                Is.EqualTo(1.5f).Within(0.001f));
            Assert.That(fullEntry.AllowsKnockdown, Is.False);
            Assert.That(full.DamageMultiplier, Is.EqualTo(1.5f).Within(0.001f));
            Assert.That(full.KnockbackMultiplier, Is.EqualTo(1.8f).Within(0.001f));
            Assert.That(full.AllowsKnockdown, Is.True);
        }

        [Test]
        public void ChargeCurve_QuickKeepsStartupAdvantageAndFullScalesFeedback()
        {
            ShowcaseCargoChargeProfile quick =
                ShowcaseCargoChargeModel.Evaluate(0.1f);
            ShowcaseCargoChargeProfile power =
                ShowcaseCargoChargeModel.Evaluate(0.5f);
            ShowcaseCargoChargeProfile full =
                ShowcaseCargoChargeModel.Evaluate(1f);

            Assert.That(quick.AnimationSpeedMultiplier,
                Is.GreaterThan(power.AnimationSpeedMultiplier));
            Assert.That(power.AnimationSpeedMultiplier,
                Is.GreaterThan(full.AnimationSpeedMultiplier));
            Assert.That(quick.FeedbackSeverity,
                Is.EqualTo(ShowcaseObjectImpactSeverity.Light));
            Assert.That(power.FeedbackSeverity,
                Is.EqualTo(ShowcaseObjectImpactSeverity.Medium));
            Assert.That(full.FeedbackSeverity,
                Is.EqualTo(ShowcaseObjectImpactSeverity.Heavy));
            Assert.That(full.CameraImpulseMultiplier,
                Is.GreaterThan(power.CameraImpulseMultiplier));
            Assert.That(full.HitStopMultiplier,
                Is.GreaterThan(power.HitStopMultiplier));
            Assert.That(full.ImpactEnergyBudgetMultiplier,
                Is.GreaterThan(power.ImpactEnergyBudgetMultiplier));
            Assert.That(quick.ObjectSpeedMultiplier,
                Is.LessThan(power.ObjectSpeedMultiplier));
            Assert.That(power.ObjectSpeedMultiplier,
                Is.LessThan(full.ObjectSpeedMultiplier));
        }

        [Test]
        public void SweepCharge_QuickPowerFullUseDistinctReachAndTravelBudgets()
        {
            ShowcaseCargoChargeProfile quick =
                ShowcaseCargoChargeModel.Evaluate(0.05f);
            ShowcaseCargoChargeProfile power =
                ShowcaseCargoChargeModel.Evaluate(0.55f);
            ShowcaseCargoChargeProfile full =
                ShowcaseCargoChargeModel.Evaluate(1f);
            ShowcaseCargoSweepHitProfile first =
                ShowcaseCargoSweepEnergyModel.ResolveHitProfile(1);

            float quickReach =
                ShowcaseCargoSweepEnergyModel.ResolveSweepReachScale(quick);
            float powerReach =
                ShowcaseCargoSweepEnergyModel.ResolveSweepReachScale(power);
            float fullReach =
                ShowcaseCargoSweepEnergyModel.ResolveSweepReachScale(full);
            float quickTravel =
                ShowcaseCargoSweepEnergyModel.ResolveTargetTravelDistance(
                    quick,
                    first,
                    2.8f);
            float powerTravel =
                ShowcaseCargoSweepEnergyModel.ResolveTargetTravelDistance(
                    power,
                    first,
                    2.8f);
            float fullTravel =
                ShowcaseCargoSweepEnergyModel.ResolveTargetTravelDistance(
                    full,
                    first,
                    2.8f);

            Assert.That(quickReach, Is.LessThan(powerReach));
            Assert.That(powerReach, Is.LessThan(fullReach));
            Assert.That(quickTravel, Is.LessThan(powerTravel));
            Assert.That(powerTravel, Is.LessThan(fullTravel));
            Assert.That(quickTravel, Is.LessThanOrEqualTo(0.75f));
            Assert.That(fullTravel, Is.GreaterThanOrEqualTo(2.4f));
        }

        [Test]
        public void SweepEnergy_ChargeTierLocksOneTwoAndThreeTargetBudgets()
        {
            ShowcaseCargoSweepEnergyState lowQuick =
                ShowcaseCargoSweepEnergyModel.ResolveInitialState(
                    ShowcaseCargoChargeModel.Evaluate(0.1f));
            ShowcaseCargoSweepEnergyState highQuick =
                ShowcaseCargoSweepEnergyModel.ResolveInitialState(
                    ShowcaseCargoChargeModel.Evaluate(0.34f));
            ShowcaseCargoSweepEnergyState power =
                ShowcaseCargoSweepEnergyModel.ResolveInitialState(
                    ShowcaseCargoChargeModel.Evaluate(0.55f));
            ShowcaseCargoSweepEnergyState full =
                ShowcaseCargoSweepEnergyModel.ResolveInitialState(
                    ShowcaseCargoChargeModel.Evaluate(1f));

            Assert.That(lowQuick.MaximumTargets, Is.EqualTo(1));
            Assert.That(highQuick.MaximumTargets, Is.EqualTo(2));
            Assert.That(power.MaximumTargets, Is.EqualTo(2));
            Assert.That(full.MaximumTargets, Is.EqualTo(3));
            Assert.That(lowQuick.InitialEnergy01,
                Is.LessThan(highQuick.InitialEnergy01));
            Assert.That(highQuick.InitialEnergy01,
                Is.LessThan(power.InitialEnergy01));
            Assert.That(power.InitialEnergy01,
                Is.LessThan(full.InitialEnergy01));
            Assert.That(
                ShowcaseCargoSweepEnergyModel.AbsoluteMaximumTargets,
                Is.EqualTo(3));
        }

        [Test]
        public void SweepEnergy_HitOrdinalUsesLockedDamageAndDecayCurve()
        {
            ShowcaseCargoSweepHitProfile first =
                ShowcaseCargoSweepEnergyModel.ResolveHitProfile(1);
            ShowcaseCargoSweepHitProfile second =
                ShowcaseCargoSweepEnergyModel.ResolveHitProfile(2);
            ShowcaseCargoSweepHitProfile third =
                ShowcaseCargoSweepEnergyModel.ResolveHitProfile(3);

            Assert.That(first.DamageScale, Is.EqualTo(1f).Within(0.001f));
            Assert.That(second.DamageScale,
                Is.EqualTo(0.75f).Within(0.001f));
            Assert.That(third.DamageScale,
                Is.EqualTo(0.525f).Within(0.001f));
            Assert.That(first.EnergyCost01,
                Is.EqualTo(0.25f).Within(0.001f));
            Assert.That(second.EnergyCost01,
                Is.EqualTo(0.3f).Within(0.001f));
            Assert.That(third.EnergyCost01,
                Is.EqualTo(0.45f).Within(0.001f));
            Assert.That(first.KnockbackScale,
                Is.GreaterThan(second.KnockbackScale));
            Assert.That(second.KnockbackScale,
                Is.GreaterThan(third.KnockbackScale));
            Assert.That(
                first.EnergyCost01 + second.EnergyCost01,
                Is.GreaterThan(0.5f));
            Assert.That(
                ShowcaseCargoSweepEnergyModel.ResolvePrimaryCarryDuration(
                    ShowcaseCargoChargeModel.Evaluate(1f)),
                Is.InRange(0.15f, 0.35f));
        }

        [Test]
        public void FullSweep_TravelDistanceStrictlyDecaysAcrossThreeVictims()
        {
            ShowcaseCargoChargeProfile full =
                ShowcaseCargoChargeModel.Evaluate(1f);
            ShowcaseCargoSweepHitProfile firstProfile =
                ShowcaseCargoSweepEnergyModel.ResolveHitProfile(1);
            ShowcaseCargoSweepHitProfile secondProfile =
                ShowcaseCargoSweepEnergyModel.ResolveHitProfile(2);
            ShowcaseCargoSweepHitProfile thirdProfile =
                ShowcaseCargoSweepEnergyModel.ResolveHitProfile(3);

            float firstDistance =
                ShowcaseCargoSweepEnergyModel.ResolveTargetTravelDistance(
                    full,
                    firstProfile,
                    2.8f);
            float secondDistance =
                ShowcaseCargoSweepEnergyModel.ResolveTargetTravelDistance(
                    full,
                    secondProfile,
                    2.8f);
            float thirdDistance =
                ShowcaseCargoSweepEnergyModel.ResolveTargetTravelDistance(
                    full,
                    thirdProfile,
                    2.8f);
            float secondDuration =
                ShowcaseCargoSweepEnergyModel.ResolveTargetTravelDuration(
                    full,
                    secondProfile,
                    0.32f);
            float thirdDuration =
                ShowcaseCargoSweepEnergyModel.ResolveTargetTravelDuration(
                    full,
                    thirdProfile,
                    0.32f);

            Assert.That(firstDistance, Is.GreaterThan(secondDistance));
            Assert.That(secondDistance, Is.GreaterThan(thirdDistance));
            Assert.That(secondDistance, Is.GreaterThan(1.5f));
            Assert.That(thirdDistance, Is.GreaterThan(1f));
            Assert.That(secondDuration, Is.GreaterThan(thirdDuration));
        }

        [Test]
        public void Resolver_ExplicitFloorAimUsesOverheadSlamBeforeSweep()
        {
            ShowcaseCargoAttackResolution resolution =
                ShowcaseCargoAttackResolver.Resolve(CreateInput(
                    targetPosition: new Vector3(0.4f, 0f, 1.5f),
                    hasPrimaryTarget: true,
                    leftSweepClear: true,
                    rightSweepClear: true,
                    overheadPathClear: true,
                    floorAim: true,
                    downwardAim: true,
                    nearbyEnemyDistance: 0.2f));

            Assert.That(resolution.AttackType,
                Is.EqualTo(ShowcaseCargoAttackType.OverheadSlam));
            Assert.That(resolution.Reason,
                Is.EqualTo(
                    ShowcaseCargoAttackResolutionReason.ExplicitDownwardSlam));
            Assert.That(resolution.IsFallback, Is.False);
        }

        [TestCase(-1f, ShowcaseCargoAttackType.SweepLeft)]
        [TestCase(1f, ShowcaseCargoAttackType.SweepRight)]
        public void Resolver_OpenPreferredSideSelectsDeterministicSweep(
            float targetX,
            ShowcaseCargoAttackType expected)
        {
            ShowcaseCargoAttackResolution resolution =
                ShowcaseCargoAttackResolver.Resolve(CreateInput(
                    targetPosition: new Vector3(targetX, 1f, 4f),
                    hasPrimaryTarget: true,
                    leftSweepClear: true,
                    rightSweepClear: true,
                    overheadPathClear: true,
                    nearbyEnemyDistance: 4f));

            Assert.That(resolution.AttackType, Is.EqualTo(expected));
            Assert.That(resolution.Reason,
                Is.EqualTo(ShowcaseCargoAttackResolutionReason.PreferredSweep));
        }

        [Test]
        public void Resolver_FreeAimWithoutEnemyStillSelectsOpenSweep()
        {
            ShowcaseCargoAttackResolution resolution =
                ShowcaseCargoAttackResolver.Resolve(CreateInput(
                    targetPosition: new Vector3(0.8f, 1f, 4f),
                    hasPrimaryTarget: false,
                    leftSweepClear: true,
                    rightSweepClear: true,
                    overheadPathClear: true,
                    nearbyEnemyDistance: float.PositiveInfinity));

            Assert.That(resolution.AttackType,
                Is.EqualTo(ShowcaseCargoAttackType.SweepRight));
            Assert.That(resolution.Reason,
                Is.EqualTo(ShowcaseCargoAttackResolutionReason.PreferredSweep));
            Assert.That(resolution.IsAccepted, Is.True);
        }

        [Test]
        public void Resolver_FreeFloorAimWithoutEnemyStillSelectsSlam()
        {
            ShowcaseCargoAttackResolution resolution =
                ShowcaseCargoAttackResolver.Resolve(CreateInput(
                    targetPosition: new Vector3(0f, 0f, 2f),
                    hasPrimaryTarget: false,
                    leftSweepClear: true,
                    rightSweepClear: true,
                    overheadPathClear: true,
                    floorAim: true,
                    downwardAim: true,
                    nearbyEnemyDistance: float.PositiveInfinity));

            Assert.That(resolution.AttackType,
                Is.EqualTo(ShowcaseCargoAttackType.OverheadSlam));
            Assert.That(resolution.Reason,
                Is.EqualTo(
                    ShowcaseCargoAttackResolutionReason.ExplicitDownwardSlam));
            Assert.That(resolution.IsAccepted, Is.True);
        }

        [Test]
        public void Resolver_BlockedPreferredSweepUsesOpenAlternateBeforeSlam()
        {
            ShowcaseCargoAttackResolution resolution =
                ShowcaseCargoAttackResolver.Resolve(CreateInput(
                    targetPosition: new Vector3(-1f, 1f, 2f),
                    hasPrimaryTarget: true,
                    leftSweepClear: false,
                    rightSweepClear: true,
                    overheadPathClear: true,
                    nearbyEnemyDistance: 1f));

            Assert.That(resolution.AttackType,
                Is.EqualTo(ShowcaseCargoAttackType.SweepRight));
            Assert.That(resolution.Reason,
                Is.EqualTo(ShowcaseCargoAttackResolutionReason.AlternateSweep));
            Assert.That(resolution.IsFallback, Is.True);
        }

        [TestCase(-1f)]
        [TestCase(1f)]
        public void Resolver_NarrowCorridorForcesSlamRegardlessOfTargetSide(
            float targetX)
        {
            ShowcaseCargoAttackResolution resolution =
                ShowcaseCargoAttackResolver.Resolve(CreateInput(
                    targetPosition: new Vector3(targetX, 1f, 2f),
                    hasPrimaryTarget: true,
                    leftSweepClear: false,
                    rightSweepClear: false,
                    overheadPathClear: true,
                    nearbyEnemyDistance: 1f));

            Assert.That(resolution.AttackType,
                Is.EqualTo(ShowcaseCargoAttackType.OverheadSlam));
            Assert.That(resolution.Reason,
                Is.EqualTo(ShowcaseCargoAttackResolutionReason.
                    SweepBlockedSlamFallback));
            Assert.That(resolution.IsFallback, Is.True);
        }

        [Test]
        public void Resolver_LowAimWithoutSteepDownwardIntentRemainsSweep()
        {
            ShowcaseCargoAttackResolution resolution =
                ShowcaseCargoAttackResolver.Resolve(CreateInput(
                    targetPosition: new Vector3(0.7f, 0.3f, 2f),
                    hasPrimaryTarget: true,
                    leftSweepClear: true,
                    rightSweepClear: true,
                    overheadPathClear: true,
                    floorAim: true,
                    downwardAim: false,
                    nearbyEnemyDistance: 0.5f));

            Assert.That(resolution.AttackType,
                Is.EqualTo(ShowcaseCargoAttackType.SweepRight));
            Assert.That(resolution.Reason,
                Is.EqualTo(ShowcaseCargoAttackResolutionReason.PreferredSweep));
        }

        [Test]
        public void Resolver_AllBlockedRejectsInsteadOfRandomlySwitching()
        {
            ShowcaseCargoAttackResolution resolution =
                ShowcaseCargoAttackResolver.Resolve(CreateInput(
                    targetPosition: new Vector3(0f, 1f, 2f),
                    hasPrimaryTarget: true,
                    leftSweepClear: false,
                    rightSweepClear: false,
                    overheadPathClear: false,
                    nearbyEnemyDistance: 1f));

            Assert.That(resolution.AttackType,
                Is.EqualTo(ShowcaseCargoAttackType.Rejected));
            Assert.That(resolution.Reason,
                Is.EqualTo(
                    ShowcaseCargoAttackResolutionReason.AllPathsBlocked));
            Assert.That(resolution.IsAccepted, Is.False);
        }

        private static ShowcaseCargoAttackResolverInput CreateInput(
            Vector3 targetPosition,
            bool hasPrimaryTarget,
            bool leftSweepClear,
            bool rightSweepClear,
            bool overheadPathClear,
            bool floorAim = false,
            bool downwardAim = false,
            float nearbyEnemyDistance = 1f)
        {
            return new ShowcaseCargoAttackResolverInput(
                targetPosition,
                targetPosition,
                Vector3.zero,
                Vector3.forward,
                Vector3.right,
                hasPrimaryTarget,
                leftSweepClear,
                rightSweepClear,
                overheadPathClear,
                floorAim,
                downwardAim,
                nearbyEnemyDistance,
                2.4f,
                true);
        }
    }
}
