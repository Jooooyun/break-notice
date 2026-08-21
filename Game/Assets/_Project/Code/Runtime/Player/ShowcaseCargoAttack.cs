using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    public enum ShowcaseCargoChargeTier
    {
        Quick,
        Power,
        Full
    }

    public enum ShowcaseCargoAttackType
    {
        None,
        SweepLeft,
        SweepRight,
        OverheadSlam,
        Rejected
    }

    public enum ShowcaseCargoAttackResolutionReason
    {
        None,
        ExplicitDownwardSlam,
        PreferredSweep,
        AlternateSweep,
        SweepBlockedSlamFallback,
        NoTarget,
        AllPathsBlocked
    }

    public enum ShowcaseCargoSweepTerminationReason
    {
        None,
        EnergyDepleted,
        MaximumTargetsReached,
        WorldObstacle,
        PrimaryVictimBlocked
    }

    public readonly struct ShowcaseCargoSweepEnergyState
    {
        public ShowcaseCargoSweepEnergyState(
            float initialEnergy01,
            int maximumTargets)
        {
            InitialEnergy01 = Mathf.Clamp01(initialEnergy01);
            MaximumTargets = Mathf.Clamp(
                maximumTargets,
                1,
                ShowcaseCargoSweepEnergyModel.AbsoluteMaximumTargets);
        }

        public float InitialEnergy01 { get; }
        public int MaximumTargets { get; }
    }

    public readonly struct ShowcaseCargoSweepHitProfile
    {
        public ShowcaseCargoSweepHitProfile(
            int hitOrdinal,
            float damageScale,
            float knockbackScale,
            float feedbackScale,
            float energyCost01)
        {
            HitOrdinal = Mathf.Clamp(
                hitOrdinal,
                1,
                ShowcaseCargoSweepEnergyModel.AbsoluteMaximumTargets);
            DamageScale = Mathf.Max(0f, damageScale);
            KnockbackScale = Mathf.Max(0f, knockbackScale);
            FeedbackScale = Mathf.Max(0f, feedbackScale);
            EnergyCost01 = Mathf.Clamp01(energyCost01);
        }

        public int HitOrdinal { get; }
        public float DamageScale { get; }
        public float KnockbackScale { get; }
        public float FeedbackScale { get; }
        public float EnergyCost01 { get; }
    }

    public readonly struct ShowcaseCargoSweepHitAuthorization
    {
        public ShowcaseCargoSweepHitAuthorization(
            uint swingSequence,
            string sourceId,
            ShowcaseCargoSweepHitProfile hitProfile,
            bool isPrimaryVictim,
            bool isCausalEnemyCollision)
        {
            SwingSequence = swingSequence;
            SourceId = sourceId ?? string.Empty;
            HitProfile = hitProfile;
            IsPrimaryVictim = isPrimaryVictim;
            IsCausalEnemyCollision = isCausalEnemyCollision;
        }

        public uint SwingSequence { get; }
        public string SourceId { get; }
        public ShowcaseCargoSweepHitProfile HitProfile { get; }
        public bool IsPrimaryVictim { get; }
        public bool IsCausalEnemyCollision { get; }
        public bool IsValid =>
            SwingSequence != 0 &&
            !string.IsNullOrWhiteSpace(SourceId) &&
            HitProfile.HitOrdinal > 0;
    }

    public readonly struct ShowcaseCargoChargeProfile
    {
        public ShowcaseCargoChargeProfile(
            float charge01,
            ShowcaseCargoChargeTier tier,
            float damageMultiplier,
            float knockbackMultiplier,
            float objectSpeedMultiplier,
            float animationSpeedMultiplier,
            float cameraImpulseMultiplier,
            float hitStopMultiplier,
            float impactEnergyBudgetMultiplier,
            ShowcaseObjectImpactSeverity feedbackSeverity,
            float knockdownThreshold01)
        {
            Charge01 = Mathf.Clamp01(charge01);
            Tier = tier;
            DamageMultiplier = Mathf.Max(0f, damageMultiplier);
            KnockbackMultiplier = Mathf.Max(0f, knockbackMultiplier);
            ObjectSpeedMultiplier = Mathf.Max(0.1f, objectSpeedMultiplier);
            AnimationSpeedMultiplier = Mathf.Max(0.1f, animationSpeedMultiplier);
            CameraImpulseMultiplier = Mathf.Max(0f, cameraImpulseMultiplier);
            HitStopMultiplier = Mathf.Max(0f, hitStopMultiplier);
            ImpactEnergyBudgetMultiplier = Mathf.Max(
                0f,
                impactEnergyBudgetMultiplier);
            FeedbackSeverity = feedbackSeverity;
            KnockdownThreshold01 = Mathf.Clamp01(knockdownThreshold01);
        }

        public float Charge01 { get; }
        public ShowcaseCargoChargeTier Tier { get; }
        public float DamageMultiplier { get; }
        public float KnockbackMultiplier { get; }
        public float ObjectSpeedMultiplier { get; }
        public float AnimationSpeedMultiplier { get; }
        public float CameraImpulseMultiplier { get; }
        public float HitStopMultiplier { get; }
        public float ImpactEnergyBudgetMultiplier { get; }
        public ShowcaseObjectImpactSeverity FeedbackSeverity { get; }
        public float KnockdownThreshold01 { get; }
        public bool AllowsKnockdown =>
            Tier == ShowcaseCargoChargeTier.Full &&
            Charge01 >= KnockdownThreshold01;
    }

    public readonly struct ShowcaseCargoAttackResolverInput
    {
        public ShowcaseCargoAttackResolverInput(
            Vector3 aimPoint,
            Vector3 targetPosition,
            Vector3 playerPosition,
            Vector3 playerForward,
            Vector3 playerRight,
            bool hasPrimaryTarget,
            bool leftSweepClear,
            bool rightSweepClear,
            bool overheadPathClear,
            bool floorAim,
            bool downwardAim,
            float nearbyEnemyDistance,
            float slamSelectionRange,
            bool preferLeftWhenCentered)
        {
            AimPoint = aimPoint;
            TargetPosition = targetPosition;
            PlayerPosition = playerPosition;
            PlayerForward = playerForward;
            PlayerRight = playerRight;
            HasPrimaryTarget = hasPrimaryTarget;
            LeftSweepClear = leftSweepClear;
            RightSweepClear = rightSweepClear;
            OverheadPathClear = overheadPathClear;
            FloorAim = floorAim;
            DownwardAim = downwardAim;
            NearbyEnemyDistance = Mathf.Max(0f, nearbyEnemyDistance);
            SlamSelectionRange = Mathf.Max(0.1f, slamSelectionRange);
            PreferLeftWhenCentered = preferLeftWhenCentered;
        }

        public Vector3 AimPoint { get; }
        public Vector3 TargetPosition { get; }
        public Vector3 PlayerPosition { get; }
        public Vector3 PlayerForward { get; }
        public Vector3 PlayerRight { get; }
        public bool HasPrimaryTarget { get; }
        public bool LeftSweepClear { get; }
        public bool RightSweepClear { get; }
        public bool OverheadPathClear { get; }
        public bool FloorAim { get; }
        public bool DownwardAim { get; }
        public float NearbyEnemyDistance { get; }
        public float SlamSelectionRange { get; }
        public bool PreferLeftWhenCentered { get; }
    }

    public readonly struct ShowcaseCargoAttackResolution
    {
        public ShowcaseCargoAttackResolution(
            ShowcaseCargoAttackType attackType,
            ShowcaseCargoAttackResolutionReason reason)
        {
            AttackType = attackType;
            Reason = reason;
        }

        public ShowcaseCargoAttackType AttackType { get; }
        public ShowcaseCargoAttackResolutionReason Reason { get; }
        public bool IsAccepted =>
            AttackType != ShowcaseCargoAttackType.None &&
            AttackType != ShowcaseCargoAttackType.Rejected;
        public bool IsFallback =>
            Reason == ShowcaseCargoAttackResolutionReason.AlternateSweep ||
            Reason ==
                ShowcaseCargoAttackResolutionReason.SweepBlockedSlamFallback;
    }

    public static class ShowcaseCargoChargeModel
    {
        public const float QuickUpperThreshold01 = 0.35f;
        public const float FullLowerThreshold01 = 0.75f;
        public const float FullKnockdownThreshold01 = 0.9f;

        public static ShowcaseCargoChargeProfile Evaluate(float charge01)
        {
            float charge = Mathf.Clamp01(charge01);
            if (charge < QuickUpperThreshold01)
            {
                float quick01 = Mathf.InverseLerp(
                    0f,
                    QuickUpperThreshold01,
                    charge);
                return new ShowcaseCargoChargeProfile(
                    charge,
                    ShowcaseCargoChargeTier.Quick,
                    0.7f,
                    0.6f,
                    Mathf.Lerp(0.48f, 0.68f, quick01),
                    1.75f,
                    0.65f,
                    0.65f,
                    0.7f,
                    ShowcaseObjectImpactSeverity.Light,
                    FullKnockdownThreshold01);
            }

            if (charge < FullLowerThreshold01)
            {
                float power01 = Mathf.InverseLerp(
                    QuickUpperThreshold01,
                    FullLowerThreshold01,
                    charge);
                return new ShowcaseCargoChargeProfile(
                    charge,
                    ShowcaseCargoChargeTier.Power,
                    1f,
                    1f,
                    Mathf.Lerp(0.78f, 1f, power01),
                    1.55f,
                    1f,
                    1f,
                    1f,
                    ShowcaseObjectImpactSeverity.Medium,
                    FullKnockdownThreshold01);
            }

            float full01 = Mathf.InverseLerp(
                FullLowerThreshold01,
                1f,
                charge);
            return new ShowcaseCargoChargeProfile(
                charge,
                ShowcaseCargoChargeTier.Full,
                Mathf.Lerp(1.35f, 1.5f, full01),
                Mathf.Lerp(1.5f, 1.8f, full01),
                Mathf.Lerp(1.15f, 1.3f, full01),
                Mathf.Lerp(1.45f, 1.35f, full01),
                Mathf.Lerp(1.35f, 1.5f, full01),
                Mathf.Lerp(1.25f, 1.45f, full01),
                Mathf.Lerp(1.5f, 1.8f, full01),
                ShowcaseObjectImpactSeverity.Heavy,
                FullKnockdownThreshold01);
        }
    }

    public static class ShowcaseCargoSweepEnergyModel
    {
        public const int AbsoluteMaximumTargets = 3;
        public const float ContinueThreshold01 = 0.2f;

        public static ShowcaseCargoSweepEnergyState ResolveInitialState(
            ShowcaseCargoChargeProfile chargeProfile)
        {
            switch (chargeProfile.Tier)
            {
                case ShowcaseCargoChargeTier.Quick:
                {
                    float quick01 = Mathf.InverseLerp(
                        0f,
                        ShowcaseCargoChargeModel.QuickUpperThreshold01,
                        chargeProfile.Charge01);
                    float energy = Mathf.Lerp(0.43f, 0.63f, quick01);
                    int maximumTargets = energy >= 0.52f ? 2 : 1;
                    return new ShowcaseCargoSweepEnergyState(
                        energy,
                        maximumTargets);
                }

                case ShowcaseCargoChargeTier.Power:
                {
                    float power01 = Mathf.InverseLerp(
                        ShowcaseCargoChargeModel.QuickUpperThreshold01,
                        ShowcaseCargoChargeModel.FullLowerThreshold01,
                        chargeProfile.Charge01);
                    return new ShowcaseCargoSweepEnergyState(
                        Mathf.Lerp(0.72f, 0.88f, power01),
                        2);
                }

                default:
                {
                    float full01 = Mathf.InverseLerp(
                        ShowcaseCargoChargeModel.FullLowerThreshold01,
                        1f,
                        chargeProfile.Charge01);
                    return new ShowcaseCargoSweepEnergyState(
                        Mathf.Lerp(0.92f, 1f, full01),
                        AbsoluteMaximumTargets);
                }
            }
        }

        public static ShowcaseCargoSweepHitProfile ResolveHitProfile(
            int hitOrdinal)
        {
            return hitOrdinal switch
            {
                1 => new ShowcaseCargoSweepHitProfile(
                    1,
                    1f,
                    1f,
                    1f,
                    0.25f),
                2 => new ShowcaseCargoSweepHitProfile(
                    2,
                    0.75f,
                    0.75f,
                    0.82f,
                    0.3f),
                _ => new ShowcaseCargoSweepHitProfile(
                    3,
                    0.525f,
                    0.55f,
                    0.65f,
                    0.45f)
            };
        }

        public static float ResolvePrimaryCarryDuration(
            ShowcaseCargoChargeProfile chargeProfile)
        {
            return chargeProfile.Tier switch
            {
                ShowcaseCargoChargeTier.Quick => 0.16f,
                ShowcaseCargoChargeTier.Power => 0.23f,
                _ => 0.32f
            };
        }

        public static float ResolveSweepReachScale(
            ShowcaseCargoChargeProfile chargeProfile)
        {
            switch (chargeProfile.Tier)
            {
                case ShowcaseCargoChargeTier.Quick:
                {
                    float quick01 = Mathf.InverseLerp(
                        0f,
                        ShowcaseCargoChargeModel.QuickUpperThreshold01,
                        chargeProfile.Charge01);
                    return Mathf.Lerp(0.32f, 0.55f, quick01);
                }

                case ShowcaseCargoChargeTier.Power:
                {
                    float power01 = Mathf.InverseLerp(
                        ShowcaseCargoChargeModel.QuickUpperThreshold01,
                        ShowcaseCargoChargeModel.FullLowerThreshold01,
                        chargeProfile.Charge01);
                    return Mathf.Lerp(0.67f, 0.88f, power01);
                }

                default:
                {
                    float full01 = Mathf.InverseLerp(
                        ShowcaseCargoChargeModel.FullLowerThreshold01,
                        1f,
                        chargeProfile.Charge01);
                    return Mathf.Lerp(0.95f, 1f, full01);
                }
            }
        }

        public static float ResolveTargetTravelDistance(
            ShowcaseCargoChargeProfile chargeProfile,
            ShowcaseCargoSweepHitProfile hitProfile,
            float evaluatedDistance)
        {
            float minimumBaseDistance;
            float maximumBaseDistance;
            switch (chargeProfile.Tier)
            {
                case ShowcaseCargoChargeTier.Quick:
                {
                    float quick01 = Mathf.InverseLerp(
                        0f,
                        ShowcaseCargoChargeModel.QuickUpperThreshold01,
                        chargeProfile.Charge01);
                    minimumBaseDistance = Mathf.Lerp(0.3f, 0.6f, quick01);
                    maximumBaseDistance = 0.75f;
                    break;
                }

                case ShowcaseCargoChargeTier.Power:
                {
                    float power01 = Mathf.InverseLerp(
                        ShowcaseCargoChargeModel.QuickUpperThreshold01,
                        ShowcaseCargoChargeModel.FullLowerThreshold01,
                        chargeProfile.Charge01);
                    minimumBaseDistance = Mathf.Lerp(0.85f, 1.35f, power01);
                    maximumBaseDistance = 1.8f;
                    break;
                }

                default:
                {
                    float full01 = Mathf.InverseLerp(
                        ShowcaseCargoChargeModel.FullLowerThreshold01,
                        1f,
                        chargeProfile.Charge01);
                    minimumBaseDistance = Mathf.Lerp(2.4f, 2.8f, full01);
                    maximumBaseDistance = 3.4f;
                    break;
                }
            }

            float ordinalScale = hitProfile.HitOrdinal switch
            {
                1 => 1f,
                2 => 0.72f,
                _ => 0.48f
            };
            float baseDistance = Mathf.Clamp(
                Mathf.Max(0f, evaluatedDistance),
                minimumBaseDistance,
                Mathf.Max(minimumBaseDistance, maximumBaseDistance));
            return baseDistance * ordinalScale;
        }

        public static float ResolveTargetTravelDuration(
            ShowcaseCargoChargeProfile chargeProfile,
            ShowcaseCargoSweepHitProfile hitProfile,
            float evaluatedDuration)
        {
            float minimumDuration;
            float maximumDuration;
            switch (chargeProfile.Tier)
            {
                case ShowcaseCargoChargeTier.Quick:
                    minimumDuration = 0.18f;
                    maximumDuration = 0.22f;
                    break;
                case ShowcaseCargoChargeTier.Power:
                    minimumDuration = 0.22f;
                    maximumDuration = 0.28f;
                    break;
                default:
                    minimumDuration = 0.28f;
                    maximumDuration = 0.34f;
                    break;
            }

            float ordinalScale = hitProfile.HitOrdinal switch
            {
                1 => 1f,
                2 => 0.9f,
                _ => 0.8f
            };
            return Mathf.Clamp(
                    Mathf.Max(0.01f, evaluatedDuration),
                    minimumDuration,
                    maximumDuration) *
                ordinalScale;
        }

        public static float ResolveMotionScale(
            float remainingEnergy01,
            float initialEnergy01)
        {
            float fraction = initialEnergy01 > 0.001f
                ? Mathf.Clamp01(remainingEnergy01 / initialEnergy01)
                : 0f;
            return Mathf.Lerp(0.35f, 1f, fraction);
        }
    }

    public static class ShowcaseCargoAttackResolver
    {
        private const float CenteredLateralDeadZone = 0.12f;

        public static ShowcaseCargoAttackResolution Resolve(
            ShowcaseCargoAttackResolverInput input)
        {
            // Slam is a deliberate command, not the default answer to any
            // low-ish target. Requiring both a floor hit and a steep downward
            // direction keeps normal torso/head aim in the sweep family.
            bool explicitSlam = input.FloorAim && input.DownwardAim;
            if (explicitSlam && input.OverheadPathClear)
            {
                return new ShowcaseCargoAttackResolution(
                    ShowcaseCargoAttackType.OverheadSlam,
                    ShowcaseCargoAttackResolutionReason.ExplicitDownwardSlam);
            }

            // A corridor that blocks both lateral Chair paths has no honest
            // Sweep answer. Resolve Slam before target-side preference so a
            // centered or rapidly changing target cannot make the choice feel
            // arbitrary. If the overhead path is also blocked, reject rather
            // than tunnelling through level geometry.
            bool bothSweepPathsBlocked =
                !input.LeftSweepClear && !input.RightSweepClear;
            if (bothSweepPathsBlocked)
            {
                return input.OverheadPathClear
                    ? new ShowcaseCargoAttackResolution(
                        ShowcaseCargoAttackType.OverheadSlam,
                        ShowcaseCargoAttackResolutionReason.
                            SweepBlockedSlamFallback)
                    : new ShowcaseCargoAttackResolution(
                        ShowcaseCargoAttackType.Rejected,
                        ShowcaseCargoAttackResolutionReason.AllPathsBlocked);
            }

            Vector3 toTarget = Vector3.ProjectOnPlane(
                input.TargetPosition - input.PlayerPosition,
                Vector3.up);
            float lateral = Vector3.Dot(toTarget, input.PlayerRight);
            bool preferLeft = Mathf.Abs(lateral) <= CenteredLateralDeadZone
                ? input.PreferLeftWhenCentered
                : lateral < 0f;
            bool preferredClear = preferLeft
                ? input.LeftSweepClear
                : input.RightSweepClear;
            bool alternateClear = preferLeft
                ? input.RightSweepClear
                : input.LeftSweepClear;

            if (preferredClear)
            {
                return new ShowcaseCargoAttackResolution(
                    preferLeft
                        ? ShowcaseCargoAttackType.SweepLeft
                        : ShowcaseCargoAttackType.SweepRight,
                    ShowcaseCargoAttackResolutionReason.PreferredSweep);
            }

            // A blocked preferred side must not silently turn an otherwise
            // valid lateral attack into Slam. Give the player the open sweep
            // first; Slam is the visible last resort only when both sides are
            // unavailable.
            if (alternateClear)
            {
                return new ShowcaseCargoAttackResolution(
                    preferLeft
                        ? ShowcaseCargoAttackType.SweepRight
                        : ShowcaseCargoAttackType.SweepLeft,
                    ShowcaseCargoAttackResolutionReason.AlternateSweep);
            }

            return new ShowcaseCargoAttackResolution(
                ShowcaseCargoAttackType.Rejected,
                ShowcaseCargoAttackResolutionReason.AllPathsBlocked);
        }
    }
}
