using System;
using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    public enum ShowcaseObjectImpactAttackOriginType
    {
        HandThrow,
        CargoStrapMediumSwing,
        CargoStrapOverheadSlam,
        LightStrapThrow
    }

    public enum ShowcaseObjectImpactSeverity
    {
        Light,
        Medium,
        Heavy
    }

    public enum ShowcaseSecondaryImpactKind
    {
        WorldSolid,
        Enemy
    }

    public readonly struct ShowcaseObjectImpactContext
    {
        public ShowcaseObjectImpactContext(
            PrototypeInteractionTarget sourceObject,
            PrototypeWeightClass weightClass,
            float relativeImpactSpeed,
            Vector3 objectLinearVelocity,
            float objectMass,
            float throwCharge01,
            bool hasLastReleaseKineticEnergy,
            float lastReleaseKineticEnergy,
            ShowcaseObjectImpactAttackOriginType attackOriginType,
            Vector3 primaryImpactDirection,
            Vector3 worldPoint,
            uint attackSequence,
            float impactEnergyBudgetMultiplier = 1f)
        {
            SourceObject = sourceObject;
            WeightClass = weightClass;
            RelativeImpactSpeed = Mathf.Max(0f, relativeImpactSpeed);
            ObjectLinearVelocity = IsFinite(objectLinearVelocity)
                ? objectLinearVelocity
                : Vector3.zero;
            ObjectMass = Mathf.Max(0.01f, objectMass);
            ThrowCharge01 = Mathf.Clamp01(throwCharge01);
            HasLastReleaseKineticEnergy =
                hasLastReleaseKineticEnergy &&
                IsFinite(lastReleaseKineticEnergy) &&
                lastReleaseKineticEnergy >= 0f;
            LastReleaseKineticEnergy = HasLastReleaseKineticEnergy
                ? lastReleaseKineticEnergy
                : 0f;
            AttackOriginType = attackOriginType;
            Vector3 planarDirection = Vector3.ProjectOnPlane(
                primaryImpactDirection,
                Vector3.up);
            PrimaryImpactDirection = planarDirection.sqrMagnitude > 0.001f
                ? planarDirection.normalized
                : Vector3.forward;
            WorldPoint = IsFinite(worldPoint)
                ? worldPoint
                : sourceObject != null
                    ? sourceObject.transform.position
                    : Vector3.zero;
            AttackSequence = attackSequence;
            ImpactEnergyBudgetMultiplier = Mathf.Max(
                0f,
                impactEnergyBudgetMultiplier);
        }

        public PrototypeInteractionTarget SourceObject { get; }

        public PrototypeWeightClass WeightClass { get; }

        public float RelativeImpactSpeed { get; }

        public Vector3 ObjectLinearVelocity { get; }

        public float ObjectMass { get; }

        public float ThrowCharge01 { get; }

        public bool HasLastReleaseKineticEnergy { get; }

        public float LastReleaseKineticEnergy { get; }

        public ShowcaseObjectImpactAttackOriginType AttackOriginType { get; }

        public Vector3 PrimaryImpactDirection { get; }

        public Vector3 WorldPoint { get; }

        public uint AttackSequence { get; }

        public float ImpactEnergyBudgetMultiplier { get; }

        public bool IsValid =>
            SourceObject != null &&
            AttackSequence != 0 &&
            RelativeImpactSpeed > 0f &&
            ObjectMass > 0f;

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) &&
                   IsFinite(value.y) &&
                   IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }

    public readonly struct ShowcaseObjectImpactEvaluation
    {
        public ShowcaseObjectImpactEvaluation(
            ShowcaseObjectImpactContext context,
            ShowcaseObjectImpactSeverity severity,
            float impactScore,
            int baseDamage,
            float damageMultiplier,
            int damage,
            float displacementDistance,
            float displacementDuration,
            float hitStopSeconds,
            float cameraImpulseMultiplier = 1f,
            bool? causesKnockdown = null)
        {
            Context = context;
            Severity = severity;
            ImpactScore = Mathf.Max(0f, impactScore);
            BaseDamage = Mathf.Max(0, baseDamage);
            DamageMultiplier = Mathf.Max(0f, damageMultiplier);
            Damage = Mathf.Max(0, damage);
            DisplacementDistance = Mathf.Max(0f, displacementDistance);
            DisplacementDuration = Mathf.Max(0.01f, displacementDuration);
            HitStopSeconds = Mathf.Max(0f, hitStopSeconds);
            CameraImpulseMultiplier = Mathf.Max(0f, cameraImpulseMultiplier);
            CausesKnockdown = causesKnockdown ??
                severity == ShowcaseObjectImpactSeverity.Heavy;
        }

        public ShowcaseObjectImpactContext Context { get; }

        public ShowcaseObjectImpactSeverity Severity { get; }

        public float ImpactScore { get; }

        public int BaseDamage { get; }

        public float DamageMultiplier { get; }

        public int Damage { get; }

        public float DisplacementDistance { get; }

        public float DisplacementDuration { get; }

        public float HitStopSeconds { get; }

        public float CameraImpulseMultiplier { get; }

        public bool CausesKnockdown { get; }

        public bool IsValid => Context.IsValid && Damage > 0;
    }

    [Serializable]
    public sealed class ShowcaseObjectImpactTuning
    {
        [SerializeField, Min(0.1f)]
        private float minimumAcceptedImpactSpeed = 1.5f;

        [SerializeField, Min(0.1f)]
        private float referenceMass = 4f;

        [SerializeField, Min(0.1f)]
        private float referenceSpeed = 5f;

        [SerializeField, Min(0.01f)]
        private float minimumMassFactor = 0.5f;

        [SerializeField, Min(0.01f)]
        private float maximumMassFactor = 2f;

        [SerializeField, Min(0.01f)]
        private float minimumRelativeSpeedFactor = 0.25f;

        [SerializeField, Min(0.01f)]
        private float maximumRelativeSpeedFactor = 3f;

        [SerializeField, Min(0.01f)]
        private float minimumChargeFactor = 0.75f;

        [SerializeField, Min(0.01f)]
        private float maximumChargeFactor = 1.5f;

        [SerializeField, Min(0.01f)]
        private float mediumScoreThreshold = 0.45f;

        [SerializeField, Min(0.02f)]
        private float heavyScoreThreshold = 5f;

        [SerializeField, Min(0.03f)]
        private float fullHeavyScore = 9f;

        [SerializeField, Range(0.1f, 2f)]
        private float lightDamageMultiplier = 0.85f;

        [SerializeField, Range(0.1f, 2f)]
        private float mediumDamageMultiplier = 1f;

        [SerializeField, Range(0.1f, 3f)]
        private float heavyDamageMultiplier = 1.45f;

        [SerializeField, Min(0f)]
        private float lightMinimumDisplacement = 0.15f;

        [SerializeField, Min(0f)]
        private float lightMaximumDisplacement = 0.35f;

        [SerializeField, Min(0f)]
        private float mediumMinimumDisplacement = 0.4f;

        [SerializeField, Min(0f)]
        private float mediumMaximumDisplacement = 1.6f;

        [SerializeField, Min(0f)]
        private float heavyMinimumDisplacement = 1.6f;

        [SerializeField, Min(0f)]
        private float heavyMaximumDisplacement = 2.8f;

        [SerializeField, Min(0.01f)]
        private float lightMinimumDuration = 0.16f;

        [SerializeField, Min(0.01f)]
        private float lightMaximumDuration = 0.22f;

        [SerializeField, Min(0.01f)]
        private float mediumMinimumDuration = 0.22f;

        [SerializeField, Min(0.01f)]
        private float mediumMaximumDuration = 0.30f;

        [SerializeField, Min(0.01f)]
        private float heavyMinimumDuration = 0.30f;

        [SerializeField, Min(0.01f)]
        private float heavyMaximumDuration = 0.38f;

        [SerializeField, Min(0f)]
        private float lightMinimumHitStop = 0.006f;

        [SerializeField, Min(0f)]
        private float lightMaximumHitStop = 0.018f;

        [SerializeField, Min(0f)]
        private float mediumMinimumHitStop = 0.028f;

        [SerializeField, Min(0f)]
        private float mediumMaximumHitStop = 0.043f;

        [SerializeField, Min(0f)]
        private float heavyMinimumHitStop = 0.05f;

        [SerializeField, Min(0f)]
        private float heavyMaximumHitStop = 0.065f;

        public float MinimumAcceptedImpactSpeed =>
            Mathf.Max(0.1f, minimumAcceptedImpactSpeed);

        public float ReferenceMass => Mathf.Max(0.1f, referenceMass);

        public float ReferenceSpeed => Mathf.Max(0.1f, referenceSpeed);

        public float MediumScoreThreshold => Mathf.Max(0.01f, mediumScoreThreshold);

        public float HeavyScoreThreshold => Mathf.Max(
            MediumScoreThreshold + 0.01f,
            heavyScoreThreshold);

        public float FullHeavyScore => Mathf.Max(
            HeavyScoreThreshold + 0.01f,
            fullHeavyScore);

        public ShowcaseObjectImpactEvaluation Evaluate(
            ShowcaseObjectImpactContext context,
            int baseDamage)
        {
            if (!context.IsValid || baseDamage <= 0)
            {
                return default;
            }

            float massFactor = Mathf.Clamp(
                context.ObjectMass / ReferenceMass,
                Mathf.Max(0.01f, minimumMassFactor),
                Mathf.Max(minimumMassFactor, maximumMassFactor));
            float relativeSpeedFactor = Mathf.Clamp(
                context.RelativeImpactSpeed / ReferenceSpeed,
                Mathf.Max(0.01f, minimumRelativeSpeedFactor),
                Mathf.Max(minimumRelativeSpeedFactor, maximumRelativeSpeedFactor));
            float chargeFactor = Mathf.Lerp(
                Mathf.Max(0.01f, minimumChargeFactor),
                Mathf.Max(minimumChargeFactor, maximumChargeFactor),
                context.ThrowCharge01);
            float impactScore = massFactor * relativeSpeedFactor * chargeFactor;

            ShowcaseObjectImpactSeverity severity = ResolveSeverity(
                context.WeightClass,
                impactScore);
            float severity01 = ResolveSeverityProgress(severity, impactScore, context);
            float damageMultiplier = ResolveDamageMultiplier(severity);
            int damage = Mathf.Max(
                1,
                Mathf.RoundToInt(Mathf.Max(1, baseDamage) * damageMultiplier));
            float displacement = ResolveDisplacement(severity, severity01);
            float duration = ResolveDuration(severity, severity01);
            float hitStop = ResolveHitStop(severity, severity01);

            return new ShowcaseObjectImpactEvaluation(
                context,
                severity,
                impactScore,
                baseDamage,
                damageMultiplier,
                damage,
                displacement,
                duration,
                hitStop);
        }

        private ShowcaseObjectImpactSeverity ResolveSeverity(
            PrototypeWeightClass weightClass,
            float impactScore)
        {
            if (weightClass == PrototypeWeightClass.Light)
            {
                // Binder and other Light objects are deliberately capped at a
                // readable flinch. High speed may strengthen the flinch but may
                // never silently turn a Light prop into a chair knockdown.
                return ShowcaseObjectImpactSeverity.Light;
            }

            if (impactScore >= HeavyScoreThreshold)
            {
                return ShowcaseObjectImpactSeverity.Heavy;
            }

            // A qualified Medium-object collision always starts at the lower
            // Medium band. This keeps a weak chair distinct from a Binder while
            // still allowing its score to scale continuously toward Heavy.
            return ShowcaseObjectImpactSeverity.Medium;
        }

        private float ResolveSeverityProgress(
            ShowcaseObjectImpactSeverity severity,
            float impactScore,
            ShowcaseObjectImpactContext context)
        {
            return severity switch
            {
                ShowcaseObjectImpactSeverity.Light => Mathf.InverseLerp(
                    MinimumAcceptedImpactSpeed,
                    ReferenceSpeed * 1.6f,
                    context.RelativeImpactSpeed),
                ShowcaseObjectImpactSeverity.Medium => Mathf.InverseLerp(
                    MediumScoreThreshold,
                    HeavyScoreThreshold,
                    impactScore),
                ShowcaseObjectImpactSeverity.Heavy => Mathf.InverseLerp(
                    HeavyScoreThreshold,
                    FullHeavyScore,
                    impactScore),
                _ => 0f
            };
        }

        private float ResolveDamageMultiplier(ShowcaseObjectImpactSeverity severity)
        {
            return severity switch
            {
                ShowcaseObjectImpactSeverity.Light =>
                    Mathf.Max(0.1f, lightDamageMultiplier),
                ShowcaseObjectImpactSeverity.Medium =>
                    Mathf.Max(0.1f, mediumDamageMultiplier),
                ShowcaseObjectImpactSeverity.Heavy =>
                    Mathf.Max(0.1f, heavyDamageMultiplier),
                _ => 1f
            };
        }

        private float ResolveDisplacement(
            ShowcaseObjectImpactSeverity severity,
            float progress01)
        {
            return severity switch
            {
                ShowcaseObjectImpactSeverity.Light => Mathf.Lerp(
                    Mathf.Max(0f, lightMinimumDisplacement),
                    Mathf.Max(lightMinimumDisplacement, lightMaximumDisplacement),
                    progress01),
                ShowcaseObjectImpactSeverity.Medium => Mathf.Lerp(
                    Mathf.Max(0f, mediumMinimumDisplacement),
                    Mathf.Max(mediumMinimumDisplacement, mediumMaximumDisplacement),
                    progress01),
                ShowcaseObjectImpactSeverity.Heavy => Mathf.Lerp(
                    Mathf.Max(0f, heavyMinimumDisplacement),
                    Mathf.Max(heavyMinimumDisplacement, heavyMaximumDisplacement),
                    progress01),
                _ => 0f
            };
        }

        private float ResolveDuration(
            ShowcaseObjectImpactSeverity severity,
            float progress01)
        {
            return severity switch
            {
                ShowcaseObjectImpactSeverity.Light => Mathf.Lerp(
                    Mathf.Max(0.01f, lightMinimumDuration),
                    Mathf.Max(lightMinimumDuration, lightMaximumDuration),
                    progress01),
                ShowcaseObjectImpactSeverity.Medium => Mathf.Lerp(
                    Mathf.Max(0.01f, mediumMinimumDuration),
                    Mathf.Max(mediumMinimumDuration, mediumMaximumDuration),
                    progress01),
                ShowcaseObjectImpactSeverity.Heavy => Mathf.Lerp(
                    Mathf.Max(0.01f, heavyMinimumDuration),
                    Mathf.Max(heavyMinimumDuration, heavyMaximumDuration),
                    progress01),
                _ => 0.2f
            };
        }

        private float ResolveHitStop(
            ShowcaseObjectImpactSeverity severity,
            float progress01)
        {
            return severity switch
            {
                ShowcaseObjectImpactSeverity.Light => Mathf.Lerp(
                    Mathf.Max(0f, lightMinimumHitStop),
                    Mathf.Max(lightMinimumHitStop, lightMaximumHitStop),
                    progress01),
                ShowcaseObjectImpactSeverity.Medium => Mathf.Lerp(
                    Mathf.Max(0f, mediumMinimumHitStop),
                    Mathf.Max(mediumMinimumHitStop, mediumMaximumHitStop),
                    progress01),
                ShowcaseObjectImpactSeverity.Heavy => Mathf.Lerp(
                    Mathf.Max(0f, heavyMinimumHitStop),
                    Mathf.Max(heavyMinimumHitStop, heavyMaximumHitStop),
                    progress01),
                _ => 0f
            };
        }
    }
}
