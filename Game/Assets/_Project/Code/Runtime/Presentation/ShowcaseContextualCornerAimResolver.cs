using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Presentation
{
    public enum ShowcaseContextualCornerMode
    {
        RightDefault,
        RightLean,
        TemporaryLeft
    }

    public readonly struct ShowcaseContextualCornerFrameSnapshot
    {
        public ShowcaseContextualCornerFrameSnapshot(
            int frameId,
            float deltaTime,
            int resetRevision,
            float logicalYaw,
            float logicalPitch,
            bool isAim,
            bool isDirectCarryThrowWindup,
            CameraCarryMode carryMode,
            PrototypeThrowPresentationPhase throwPhase,
            int carryTargetRuntimeId,
            Pose playerRootPose,
            Pose orbitTargetPose,
            float baseTargetHeight,
            float baseDistance,
            float baseShoulderOffset,
            float baseShoulderHeight,
            float baseVerticalArmLength,
            float verticalFieldOfView,
            float aspect,
            float nearClipPlane,
            ShowcaseContextualCornerMode currentMode,
            float currentBlendProgress,
            int activeBlockerId,
            float subjectForwardSightlineClearance)
        {
            FrameId = frameId;
            DeltaTime = Mathf.Max(0f, deltaTime);
            ResetRevision = resetRevision;
            LogicalYaw = logicalYaw;
            LogicalPitch = logicalPitch;
            IsAim = isAim;
            IsDirectCarryThrowWindup = isDirectCarryThrowWindup;
            CarryMode = carryMode;
            ThrowPhase = throwPhase;
            CarryTargetRuntimeId = carryTargetRuntimeId;
            PlayerRootPose = playerRootPose;
            OrbitTargetPose = orbitTargetPose;
            BaseTargetHeight = baseTargetHeight;
            BaseDistance = baseDistance;
            BaseShoulderOffset = baseShoulderOffset;
            BaseShoulderHeight = baseShoulderHeight;
            BaseVerticalArmLength = baseVerticalArmLength;
            VerticalFieldOfView = verticalFieldOfView;
            Aspect = aspect;
            NearClipPlane = nearClipPlane;
            CurrentMode = currentMode;
            CurrentBlendProgress = Mathf.Clamp01(currentBlendProgress);
            ActiveBlockerId = activeBlockerId;
            SubjectForwardSightlineClearance = Mathf.Max(
                0f,
                subjectForwardSightlineClearance);
        }

        public int FrameId { get; }
        public float DeltaTime { get; }
        public int ResetRevision { get; }
        public float LogicalYaw { get; }
        public float LogicalPitch { get; }
        public bool IsAim { get; }
        public bool IsDirectCarryThrowWindup { get; }
        public CameraCarryMode CarryMode { get; }
        public PrototypeThrowPresentationPhase ThrowPhase { get; }
        public int CarryTargetRuntimeId { get; }
        public Pose PlayerRootPose { get; }
        public Pose OrbitTargetPose { get; }
        public float BaseTargetHeight { get; }
        public float BaseDistance { get; }
        public float BaseShoulderOffset { get; }
        public float BaseShoulderHeight { get; }
        public float BaseVerticalArmLength { get; }
        public float VerticalFieldOfView { get; }
        public float Aspect { get; }
        public float NearClipPlane { get; }
        public ShowcaseContextualCornerMode CurrentMode { get; }
        public float CurrentBlendProgress { get; }
        public int ActiveBlockerId { get; }
        public float SubjectForwardSightlineClearance { get; }

        public bool IsEligible => IsAim || IsDirectCarryThrowWindup;

        public bool ForwardSightlineAlive =>
            SubjectForwardSightlineClearance >=
            ShowcaseContextualCornerAimResolver.MinimumForwardSightlineClearance;
    }

    public readonly struct ShowcaseContextualCornerCandidate
    {
        public ShowcaseContextualCornerCandidate(
            ShowcaseContextualCornerMode mode,
            float shoulderIntentFactor,
            bool isSolved,
            bool isIntentFullyPreserved,
            bool hasStaticWorldConstraint,
            bool isNearPlaneSafe,
            bool isSubjectSafe,
            float requestedBoomDistance,
            float safeBoomDistance,
            float atomicPreservation01,
            float centerSightlineClearance,
            int blockerId,
            ulong geometrySignature = 0UL)
        {
            Mode = mode;
            ShoulderIntentFactor = Mathf.Clamp(shoulderIntentFactor, -1f, 1f);
            IsSolved = isSolved;
            IsIntentFullyPreserved = isIntentFullyPreserved;
            HasStaticWorldConstraint = hasStaticWorldConstraint;
            IsNearPlaneSafe = isNearPlaneSafe;
            IsSubjectSafe = isSubjectSafe;
            RequestedBoomDistance = Mathf.Max(0f, requestedBoomDistance);
            SafeBoomDistance = Mathf.Max(0f, safeBoomDistance);
            AtomicPreservation01 = Mathf.Clamp01(atomicPreservation01);
            CenterSightlineClearance = Mathf.Max(0f, centerSightlineClearance);
            BlockerId = blockerId;
            GeometrySignature = geometrySignature;
        }

        public ShowcaseContextualCornerMode Mode { get; }
        public float ShoulderIntentFactor { get; }
        public bool IsSolved { get; }
        public bool IsIntentFullyPreserved { get; }
        public bool HasStaticWorldConstraint { get; }
        public bool IsNearPlaneSafe { get; }
        public bool IsSubjectSafe { get; }
        public float RequestedBoomDistance { get; }
        public float SafeBoomDistance { get; }
        public float AtomicPreservation01 { get; }
        public float CenterSightlineClearance { get; }
        public int BlockerId { get; }
        public ulong GeometrySignature { get; }

        public float EffectiveClearance => !IsSolved
            ? 0f
            : Mathf.Min(
                CenterSightlineClearance,
                RequestedBoomDistance * AtomicPreservation01);

        public bool IsSafeIntent => IsSolved &&
            IsIntentFullyPreserved &&
            IsNearPlaneSafe &&
            IsSubjectSafe;

        public bool IsHardSafe => IsSolved && IsNearPlaneSafe && IsSubjectSafe;
    }

    public readonly struct ShowcaseContextualCornerSelection
    {
        public ShowcaseContextualCornerSelection(
            ShowcaseContextualCornerMode mode,
            float shoulderIntentFactor,
            float blendProgress,
            int blockerId,
            int transitionCount)
        {
            Mode = mode;
            ShoulderIntentFactor = Mathf.Clamp(shoulderIntentFactor, -1f, 1f);
            BlendProgress = Mathf.Clamp01(blendProgress);
            BlockerId = blockerId;
            TransitionCount = transitionCount;
        }

        public ShowcaseContextualCornerMode Mode { get; }
        public float ShoulderIntentFactor { get; }
        public float BlendProgress { get; }
        public int BlockerId { get; }
        public int TransitionCount { get; }
    }

    /// <summary>
    /// Side-effect-free camera intent state machine.  It consumes already
    /// evaluated candidates and never performs physics, changes gameplay
    /// heading, or writes a Camera/Cinemachine state.
    /// </summary>
    public sealed class ShowcaseContextualCornerAimResolver
    {
        public const float RightDefaultShoulderFactor = 1f;
        public const float RightLeanShoulderFactor = 0.35f;
        public const float TemporaryLeftShoulderFactor = -1f;
        public const float EnterEvidenceDuration = 0.10f;
        public const float EnterBlendDuration = 0.12f;
        public const float ExitEvidenceDuration = 0.15f;
        public const float ReturnBlendDuration = 0.18f;
        public const float SideChangeCooldown = 0.20f;
        public const float ImprovementDistance = 0.75f;
        public const float ImprovementRatio = 0.15f;
        public const float OppositeImprovementDistance = 1.25f;
        public const float OppositeImprovementRatio = 0.25f;
        public const float MaximumAngularCorrection = 6f;
        public const float MinimumForwardSightlineClearance = 1.25f;
        public const float RetainLeanDistance = 0.30f;
        public const float RetainLeanRatio = 0.075f;
        public const float ExitLeanDistance = 0.20f;
        public const float ExitLeanRatio = 0.05f;
        public const float ReturnMinimumClearance = 0.05f;
        public const float RetainOppositeDistance = 0.50f;
        public const float RetainOppositeRatio = 0.10f;
        public const float ExitOppositeDistance = 0.30f;
        public const float ExitOppositeRatio = 0.075f;

        private ShowcaseContextualCornerMode committedMode =
            ShowcaseContextualCornerMode.RightDefault;
        private ShowcaseContextualCornerMode transitionSourceMode =
            ShowcaseContextualCornerMode.RightDefault;
        private ShowcaseContextualCornerMode transitionTargetMode =
            ShowcaseContextualCornerMode.RightDefault;
        private ShowcaseContextualCornerMode pendingMode =
            ShowcaseContextualCornerMode.RightDefault;
        private ulong pendingGeometrySignature;
        private ulong activeGeometrySignature;
        private ulong transitionTargetGeometrySignature;
        private float currentShoulderFactor = RightDefaultShoulderFactor;
        private float blendSourceFactor = RightDefaultShoulderFactor;
        private float blendTargetFactor = RightDefaultShoulderFactor;
        private float blendElapsed;
        private float blendDuration;
        private float evidenceElapsed;
        private float sideChangeCooldownRemaining;
        private bool isTransitioning;
        private int observedResetRevision = int.MinValue;
        private int lastEvaluatedFrame = -1;
        private int activeBlockerId;
        private int transitionCount;

        public ShowcaseContextualCornerMode CurrentMode => committedMode;

        public ShowcaseContextualCornerMode TransitionSourceMode =>
            transitionSourceMode;

        public ShowcaseContextualCornerMode TransitionTargetMode =>
            transitionTargetMode;

        public bool IsTransitioning => isTransitioning;

        public ulong ActiveGeometrySignature => activeGeometrySignature;

        public float CurrentShoulderFactor => currentShoulderFactor;

        public float CurrentBlendProgress => blendDuration <= 0f
            ? 1f
            : Mathf.Clamp01(blendElapsed / blendDuration);

        public int ActiveBlockerId => activeBlockerId;

        public int TransitionCount => transitionCount;

        public ShowcaseContextualCornerSelection Evaluate(
            ShowcaseContextualCornerFrameSnapshot frame,
            ShowcaseContextualCornerCandidate rightDefault,
            ShowcaseContextualCornerCandidate rightLean,
            ShowcaseContextualCornerCandidate temporaryLeft)
        {
            if (frame.ResetRevision != observedResetRevision)
            {
                Reset(frame.ResetRevision);
            }

            if (lastEvaluatedFrame == frame.FrameId)
            {
                return BuildSelection();
            }
            lastEvaluatedFrame = frame.FrameId;

            float deltaTime = Mathf.Max(0f, frame.DeltaTime);
            AdvanceBlend(deltaTime);
            sideChangeCooldownRemaining = Mathf.Max(
                0f,
                sideChangeCooldownRemaining - deltaTime);

            if (!frame.IsEligible)
            {
                evidenceElapsed = 0f;
                pendingMode = ShowcaseContextualCornerMode.RightDefault;
                pendingGeometrySignature = 0UL;
                activeBlockerId = 0;
                if (currentShoulderFactor < RightDefaultShoulderFactor - 0.0001f &&
                    (!isTransitioning ||
                     transitionTargetMode != ShowcaseContextualCornerMode.RightDefault))
                {
                    BeginModeTransition(
                        ShowcaseContextualCornerMode.RightDefault,
                        ReturnBlendDuration,
                        0UL);
                }
                return BuildSelection();
            }

            // Geometry may change while a shoulder blend is running, but the
            // target is immutable until that blend completes.  The B solver
            // continues to enforce non-penetration during the transition.
            if (isTransitioning)
            {
                return BuildSelection();
            }

            ShowcaseContextualCornerMode desiredMode = ResolveDesiredMode(
                frame,
                rightDefault,
                rightLean,
                temporaryLeft);
            int desiredBlockerId = desiredMode == ShowcaseContextualCornerMode.RightDefault
                ? 0
                : rightDefault.BlockerId;
            ulong desiredGeometrySignature = desiredMode ==
                    ShowcaseContextualCornerMode.RightDefault
                ? 0UL
                : rightDefault.GeometrySignature;

            if (desiredMode == committedMode)
            {
                pendingMode = desiredMode;
                pendingGeometrySignature = desiredGeometrySignature;
                evidenceElapsed = 0f;
                activeBlockerId = desiredBlockerId;
                if (desiredGeometrySignature != 0UL)
                {
                    activeGeometrySignature = desiredGeometrySignature;
                }
                return BuildSelection();
            }

            bool materialGeometryChange =
                pendingGeometrySignature != 0UL &&
                desiredGeometrySignature != 0UL &&
                pendingGeometrySignature != desiredGeometrySignature;
            if (desiredMode != pendingMode || materialGeometryChange)
            {
                pendingMode = desiredMode;
                pendingGeometrySignature = desiredGeometrySignature;
                evidenceElapsed = 0f;
            }
            evidenceElapsed += deltaTime;

            float requiredEvidence = desiredMode ==
                    ShowcaseContextualCornerMode.RightDefault
                ? ExitEvidenceDuration
                : EnterEvidenceDuration;
            bool sideChangeBlocked =
                committedMode != ShowcaseContextualCornerMode.RightDefault &&
                desiredMode != ShowcaseContextualCornerMode.RightDefault &&
                sideChangeCooldownRemaining > 0f;
            if (!sideChangeBlocked && evidenceElapsed >= requiredEvidence)
            {
                BeginModeTransition(
                    desiredMode,
                    desiredMode == ShowcaseContextualCornerMode.RightDefault
                        ? ReturnBlendDuration
                        : EnterBlendDuration,
                    desiredGeometrySignature);
                activeBlockerId = desiredBlockerId;
                evidenceElapsed = 0f;
            }

            return BuildSelection();
        }

        public void Reset(int resetRevision, int frameId = -1)
        {
            observedResetRevision = resetRevision;
            lastEvaluatedFrame = frameId;
            committedMode = ShowcaseContextualCornerMode.RightDefault;
            transitionSourceMode = ShowcaseContextualCornerMode.RightDefault;
            transitionTargetMode = ShowcaseContextualCornerMode.RightDefault;
            pendingMode = ShowcaseContextualCornerMode.RightDefault;
            pendingGeometrySignature = 0UL;
            activeGeometrySignature = 0UL;
            transitionTargetGeometrySignature = 0UL;
            currentShoulderFactor = RightDefaultShoulderFactor;
            blendSourceFactor = RightDefaultShoulderFactor;
            blendTargetFactor = RightDefaultShoulderFactor;
            blendElapsed = 0f;
            blendDuration = 0f;
            evidenceElapsed = 0f;
            sideChangeCooldownRemaining = 0f;
            isTransitioning = false;
            activeBlockerId = 0;
            transitionCount = 0;
        }

        private ShowcaseContextualCornerMode ResolveDesiredMode(
            ShowcaseContextualCornerFrameSnapshot frame,
            ShowcaseContextualCornerCandidate rightDefault,
            ShowcaseContextualCornerCandidate rightLean,
            ShowcaseContextualCornerCandidate temporaryLeft)
        {
            if (!frame.ForwardSightlineAlive ||
                !rightDefault.HasStaticWorldConstraint)
            {
                return ShowcaseContextualCornerMode.RightDefault;
            }

            if (committedMode == ShowcaseContextualCornerMode.RightLean)
            {
                if (rightLean.IsSafeIntent &&
                    (!rightDefault.IsHardSafe ||
                     HasAdvantage(
                         rightLean,
                         rightDefault,
                         RetainLeanDistance,
                         RetainLeanRatio)))
                {
                    return ShowcaseContextualCornerMode.RightLean;
                }

                if (!rightLean.IsSafeIntent &&
                    temporaryLeft.IsSafeIntent &&
                    HasAdvantage(
                        temporaryLeft,
                        rightLean,
                        OppositeImprovementDistance,
                        OppositeImprovementRatio))
                {
                    return ShowcaseContextualCornerMode.TemporaryLeft;
                }

                if (rightDefault.IsHardSafe &&
                    rightDefault.EffectiveClearance >= ReturnMinimumClearance &&
                    !HasAdvantage(
                        rightLean,
                        rightDefault,
                        ExitLeanDistance,
                        ExitLeanRatio))
                {
                    return ShowcaseContextualCornerMode.RightDefault;
                }
                return ShowcaseContextualCornerMode.RightLean;
            }

            if (committedMode == ShowcaseContextualCornerMode.TemporaryLeft)
            {
                ShowcaseContextualCornerCandidate returnCandidate =
                    rightLean.IsSafeIntent ? rightLean : rightDefault;
                if (temporaryLeft.IsSafeIntent &&
                    (!returnCandidate.IsHardSafe ||
                     HasAdvantage(
                         temporaryLeft,
                         returnCandidate,
                         RetainOppositeDistance,
                         RetainOppositeRatio)))
                {
                    return ShowcaseContextualCornerMode.TemporaryLeft;
                }

                if (returnCandidate.IsHardSafe &&
                    !HasAdvantage(
                        temporaryLeft,
                        returnCandidate,
                        ExitOppositeDistance,
                        ExitOppositeRatio))
                {
                    return rightLean.IsSafeIntent
                        ? ShowcaseContextualCornerMode.RightLean
                        : ShowcaseContextualCornerMode.RightDefault;
                }
                return ShowcaseContextualCornerMode.TemporaryLeft;
            }

            // Lean has strict entry priority.  The opposite shoulder is only a
            // bounded fallback when the right-side lean itself is unavailable.
            if (rightLean.IsSafeIntent &&
                HasAdvantage(
                    rightLean,
                    rightDefault,
                    ImprovementDistance,
                    ImprovementRatio))
            {
                return ShowcaseContextualCornerMode.RightLean;
            }

            if (!rightLean.IsSafeIntent &&
                temporaryLeft.IsSafeIntent &&
                HasAdvantage(
                    temporaryLeft,
                    rightLean,
                    OppositeImprovementDistance,
                    OppositeImprovementRatio))
            {
                return ShowcaseContextualCornerMode.TemporaryLeft;
            }
            return ShowcaseContextualCornerMode.RightDefault;
        }

        private static bool HasAdvantage(
            ShowcaseContextualCornerCandidate candidate,
            ShowcaseContextualCornerCandidate baseline,
            float minimumDistance,
            float minimumRatio)
        {
            if (!candidate.IsHardSafe)
            {
                return false;
            }

            float baselineClearance = Mathf.Max(0f, baseline.EffectiveClearance);
            float candidateClearance = Mathf.Max(0f, candidate.EffectiveClearance);
            float gain = candidateClearance - baselineClearance;
            if (gain >= minimumDistance)
            {
                return true;
            }
            if (candidateClearance <= baselineClearance)
            {
                return false;
            }
            return baselineClearance <= 0.0001f ||
                gain / baselineClearance >= minimumRatio;
        }

        private void BeginModeTransition(
            ShowcaseContextualCornerMode mode,
            float duration,
            ulong geometrySignature)
        {
            pendingMode = mode;
            pendingGeometrySignature = geometrySignature;
            transitionSourceMode = committedMode;
            transitionTargetMode = mode;
            transitionTargetGeometrySignature = geometrySignature;
            blendSourceFactor = currentShoulderFactor;
            blendTargetFactor = GetTargetFactor(mode);
            blendElapsed = 0f;
            blendDuration = Mathf.Max(0.01f, duration);
            isTransitioning = true;
            transitionCount++;
        }

        private void AdvanceBlend(float deltaTime)
        {
            if (!isTransitioning)
            {
                currentShoulderFactor = blendTargetFactor;
                return;
            }

            blendElapsed = Mathf.Min(blendDuration, blendElapsed + deltaTime);
            float normalized = Mathf.Clamp01(blendElapsed / blendDuration);
            float curved = normalized * normalized * (3f - 2f * normalized);
            currentShoulderFactor = Mathf.Lerp(
                blendSourceFactor,
                blendTargetFactor,
                curved);
            if (blendElapsed >= blendDuration)
            {
                currentShoulderFactor = blendTargetFactor;
                committedMode = transitionTargetMode;
                transitionSourceMode = committedMode;
                activeGeometrySignature = transitionTargetGeometrySignature;
                isTransitioning = false;
                sideChangeCooldownRemaining = SideChangeCooldown;
            }
        }

        private ShowcaseContextualCornerSelection BuildSelection()
        {
            return new ShowcaseContextualCornerSelection(
                committedMode,
                currentShoulderFactor,
                CurrentBlendProgress,
                activeBlockerId,
                transitionCount);
        }

        private static float GetTargetFactor(ShowcaseContextualCornerMode mode)
        {
            switch (mode)
            {
                case ShowcaseContextualCornerMode.RightLean:
                    return RightLeanShoulderFactor;
                case ShowcaseContextualCornerMode.TemporaryLeft:
                    return TemporaryLeftShoulderFactor;
                default:
                    return RightDefaultShoulderFactor;
            }
        }
    }
}
