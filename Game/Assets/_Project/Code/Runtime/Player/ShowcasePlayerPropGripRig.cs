using BreakNotice.Gameplay.Combat;
using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    public enum ShowcasePlayerPropGripPhase
    {
        None,
        AcquisitionPending,
        AcquisitionAttached,
        Carry,
        Throw
    }

    public enum ShowcasePlayerPropGripProfile
    {
        None,
        CatchLightCenter,
        CatchLightLeft,
        CatchLightRight,
        CatchMedium,
        PickupLowSmall,
        PickupLowMedium,
        ThrowLight,
        ThrowMedium
    }

    /// <summary>
    /// Presentation-only provider for player prop poses and medium-prop hand IK.
    /// Gameplay ownership, contact acceptance, and physical release remain with
    /// their existing runtime owners; this component only answers explicit calls.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShowcasePlayerPropGripRig : MonoBehaviour
    {
        // Keep the chair body outside CHR00's capsule. Hands target the near
        // backrest face below, so the prop center itself does not need to enter
        // the player's reach envelope.
        // Authored anchor is player-local (0, 1.05, 1.15). These offsets keep
        // the chair's near face beyond CHR00's 0.45 m capsule while
        // placing the backrest grip inside the target avatar's arm reach.
        private const float MediumCarryAnchorRetraction = 0.33f;
        private const float MediumCarryDownOffset = 1.10f;
        private const float MediumGripNearFaceFactor = 1f;
        private const float MediumGripPalmClearance = 0.070f;
        private const float AuthoredMediumLateralGrip = 0.12f;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private Transform actorRoot;

        [SerializeField]
        private Transform carryAnchor;

        [SerializeField]
        private ShowcaseBinderGripSocket stableLightSocket;

        [SerializeField]
        private Vector3 lightActionLocalOffset = new Vector3(0f, -0.13f, 0.03f);

        [SerializeField]
        private Vector3 lightActionLocalRotationEuler = new Vector3(0f, 90f, 0f);

        [SerializeField, Min(0.01f)]
        private float mediumLateralGrip = AuthoredMediumLateralGrip;

        [SerializeField, Range(0.1f, 1f)]
        private float mediumBackrestHeight = 0.78f;

        private Transform leftHand;
        private Transform rightHand;
        private Transform leftLowerArm;
        private Transform rightLowerArm;
        private Pose acquisitionContactPose;
        private Pose throwSourcePose;
        private ShowcasePlayerPropGripProfile carryProfileBeforeThrow;
        private bool hasAcquisitionContactPose;
        private bool hasThrowSourcePose;
        private float acquisitionStabilizeNormalized;
        private float acquisitionCompleteNormalized;
        private float throwReleaseNormalized;
        private Pose mediumDesiredPoseForFrame;
        private int mediumDesiredPoseFrame = -1;
        private uint mediumDesiredPoseSequence;
        private ShowcasePlayerPropGripPhase mediumDesiredPosePhase;
        private PrototypeInteractionTarget mediumDesiredPoseTarget;
        private bool poseIsAuthoritative;

        public ShowcasePlayerPropGripPhase Phase { get; private set; }

        public ShowcasePlayerPropGripProfile Profile { get; private set; }

        public PrototypeInteractionTarget Target { get; private set; }

        public uint SequenceId { get; private set; }

        public float PreAttachPositionError { get; private set; }

        public float CurrentGripPositionError { get; private set; }

        public float CurrentGripRotationError { get; private set; }

        public float ReleasePoseError { get; private set; }

        public float MediumIkWeight { get; private set; }

        public int LastAppliedPoseFrameId { get; private set; } = -1;

        public float MediumLeftHandGripPositionError { get; private set; }

        public float MediumRightHandGripPositionError { get; private set; }

        public bool IsRuntimeAvailable => HasHumanoidRig;

        /// <summary>
        /// True only while this presentation rig has a valid pose for the active
        /// target. It does not grant gameplay or physics ownership.
        /// </summary>
        public bool IsAuthoritative => poseIsAuthoritative && HasHumanoidRig;

        private bool HasHumanoidRig =>
            isActiveAndEnabled &&
            animator != null &&
            animator.isActiveAndEnabled &&
            animator.isHuman &&
            leftHand != null &&
            rightHand != null &&
            leftLowerArm != null &&
            rightLowerArm != null;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
            if (actorRoot == null)
            {
                actorRoot = transform;
            }
            ResolveBones();
        }

        private void OnDisable()
        {
            ResetRuntimeState();
        }

        public void Configure(
            Animator authoredAnimator,
            Transform authoredActorRoot,
            Transform authoredCarryAnchor,
            ShowcaseBinderGripSocket authoredStableLightSocket)
        {
            animator = authoredAnimator != null ? authoredAnimator : GetComponent<Animator>();
            actorRoot = authoredActorRoot != null ? authoredActorRoot : transform;
            carryAnchor = authoredCarryAnchor;
            stableLightSocket = authoredStableLightSocket;
            mediumLateralGrip = AuthoredMediumLateralGrip;
            ResolveBones();
            ResetRuntimeState();
        }

        public bool PrepareAcquisition(
            PrototypeInteractionTarget target,
            ShowcasePlayerAnimationCue cue,
            PrototypeInteractionOutcome outcome)
        {
            if (!HasHumanoidRig || target == null ||
                (outcome != PrototypeInteractionOutcome.Scoop &&
                 outcome != PrototypeInteractionOutcome.Catch) ||
                !TryResolveAcquisitionProfile(target, cue, out ShowcasePlayerPropGripProfile profile) ||
                !ShowcaseChr00AnimationTiming.TryGetAcquisitionTiming(
                    cue,
                    out _,
                    out float stabilizeNormalized,
                    out float completeNormalized))
            {
                return false;
            }

            AdvanceSequence();
            Target = target;
            Profile = profile;
            Phase = ShowcasePlayerPropGripPhase.AcquisitionPending;
            acquisitionStabilizeNormalized = stabilizeNormalized;
            acquisitionCompleteNormalized = completeNormalized;
            throwReleaseNormalized = 0f;
            hasAcquisitionContactPose = false;
            hasThrowSourcePose = false;
            carryProfileBeforeThrow = ShowcasePlayerPropGripProfile.None;
            PreAttachPositionError = 0f;
            CurrentGripPositionError = 0f;
            CurrentGripRotationError = 0f;
            ReleasePoseError = 0f;
            MediumIkWeight = 0f;
            // Contact remains the exact authority boundary. Pending acquisition
            // selects a profile but must not claim an object pose yet.
            poseIsAuthoritative = false;
            return true;
        }

        public bool TryCommitAcquisitionContact(
            PrototypeInteractionTarget target,
            out Pose desired)
        {
            desired = default;
            if (Phase != ShowcasePlayerPropGripPhase.AcquisitionPending ||
                !IsMatchingTarget(target) ||
                !TryGetActionObjectPose(Profile, target, out desired))
            {
                return false;
            }

            acquisitionContactPose = desired;
            hasAcquisitionContactPose = true;
            Phase = ShowcasePlayerPropGripPhase.AcquisitionAttached;
            PreAttachPositionError = Vector3.Distance(target.transform.position, desired.position);
            MediumIkWeight = IsMediumProfile(Profile)
                ? EvaluateAcquisitionBlendWeight()
                : 0f;
            // A desired contact pose is not evidence that physics applied it.
            // Lifecycle confirms the actual pose after collision resolution.
            poseIsAuthoritative = false;
            return true;
        }

        public bool CompleteAcquisition(PrototypeInteractionTarget target)
        {
            if (Phase != ShowcasePlayerPropGripPhase.AcquisitionAttached ||
                !IsMatchingTarget(target))
            {
                return false;
            }

            ShowcasePlayerPropGripProfile completedProfile = Profile;
            Phase = ShowcasePlayerPropGripPhase.Carry;
            Profile = completedProfile == ShowcasePlayerPropGripProfile.PickupLowSmall
                ? ShowcasePlayerPropGripProfile.PickupLowSmall
                : target.WeightClass == PrototypeWeightClass.Medium
                    ? ShowcasePlayerPropGripProfile.CatchMedium
                    : ShowcasePlayerPropGripProfile.CatchLightCenter;
            hasAcquisitionContactPose = false;
            MediumIkWeight = target.WeightClass == PrototypeWeightClass.Medium ? 1f : 0f;
            poseIsAuthoritative = TryGetStableCarryPose(target, out _);
            return true;
        }

        public bool BeginThrow(
            PrototypeInteractionTarget target,
            ShowcasePlayerAnimationCue cue,
            Vector3 sourcePosition,
            Quaternion sourceRotation)
        {
            if (!HasHumanoidRig || target == null ||
                !TryResolveThrowProfile(target, cue, out ShowcasePlayerPropGripProfile profile) ||
                !ShowcaseChr00AnimationTiming.TryGetThrowTiming(
                    cue,
                    out float releaseNormalized,
                    out _) ||
                !IsFinite(sourcePosition) ||
                !TryNormalize(sourceRotation, out Quaternion normalizedSourceRotation))
            {
                return false;
            }

            ShowcasePlayerPropGripProfile sourceCarryProfile = Phase == ShowcasePlayerPropGripPhase.Carry
                ? Profile
                : ShowcasePlayerPropGripProfile.None;
            AdvanceSequence();
            Target = target;
            carryProfileBeforeThrow = sourceCarryProfile;
            Profile = profile;
            Phase = ShowcasePlayerPropGripPhase.Throw;
            acquisitionStabilizeNormalized = 0f;
            acquisitionCompleteNormalized = 0f;
            throwReleaseNormalized = releaseNormalized;
            throwSourcePose = new Pose(sourcePosition, normalizedSourceRotation);
            hasThrowSourcePose = true;
            hasAcquisitionContactPose = false;
            ReleasePoseError = 0f;
            MediumIkWeight = IsMediumProfile(profile)
                ? 1f - EvaluateThrowBlendWeight()
                : 0f;
            poseIsAuthoritative = true;
            return true;
        }

        public bool CancelAction(PrototypeInteractionTarget target, bool remainsCarried)
        {
            if (!IsMatchingTarget(target))
            {
                return false;
            }

            if (!remainsCarried)
            {
                ClearActiveState();
                return true;
            }

            Phase = ShowcasePlayerPropGripPhase.Carry;
            Profile = carryProfileBeforeThrow != ShowcasePlayerPropGripProfile.None
                ? carryProfileBeforeThrow
                : target.WeightClass == PrototypeWeightClass.Medium
                    ? ShowcasePlayerPropGripProfile.CatchMedium
                    : ShowcasePlayerPropGripProfile.CatchLightCenter;
            hasAcquisitionContactPose = false;
            hasThrowSourcePose = false;
            carryProfileBeforeThrow = ShowcasePlayerPropGripProfile.None;
            MediumIkWeight = target.WeightClass == PrototypeWeightClass.Medium ? 1f : 0f;
            poseIsAuthoritative = TryGetStableCarryPose(target, out _);
            return true;
        }

        public bool CompleteRelease(PrototypeInteractionTarget target)
        {
            if (Phase != ShowcasePlayerPropGripPhase.Throw || !IsMatchingTarget(target))
            {
                return false;
            }

            ClearActiveState();
            return true;
        }

        public void ResetRuntimeState()
        {
            Phase = ShowcasePlayerPropGripPhase.None;
            Profile = ShowcasePlayerPropGripProfile.None;
            Target = null;
            SequenceId = 0;
            PreAttachPositionError = 0f;
            CurrentGripPositionError = 0f;
            CurrentGripRotationError = 0f;
            ReleasePoseError = 0f;
            MediumIkWeight = 0f;
            LastAppliedPoseFrameId = -1;
            MediumLeftHandGripPositionError = 0f;
            MediumRightHandGripPositionError = 0f;
            poseIsAuthoritative = false;
            hasAcquisitionContactPose = false;
            hasThrowSourcePose = false;
            carryProfileBeforeThrow = ShowcasePlayerPropGripProfile.None;
            acquisitionContactPose = default;
            throwSourcePose = default;
            acquisitionStabilizeNormalized = 0f;
            acquisitionCompleteNormalized = 0f;
            throwReleaseNormalized = 0f;
            InvalidateMediumDesiredPoseCache();
        }

        public bool TryGetDesiredObjectPose(
            PrototypeInteractionTarget target,
            out Pose pose)
        {
            pose = default;
            if (!HasHumanoidRig || !IsMatchingTarget(target))
            {
                poseIsAuthoritative = false;
                return false;
            }

            bool resolved;
            if (IsMediumProfile(Profile) &&
                TryGetCachedMediumDesiredPose(target, out pose))
            {
                resolved = true;
            }
            else
            {
                resolved = TryResolveDesiredObjectPose(target, out pose);
            }

            if (!resolved || !IsFinite(pose))
            {
                poseIsAuthoritative = false;
                return false;
            }

            poseIsAuthoritative = true;
            return true;
        }

        public bool ConfirmAppliedObjectPose(
            PrototypeInteractionTarget target,
            Pose desiredPose,
            Pose actualPose)
        {
            if (!HasHumanoidRig ||
                Phase == ShowcasePlayerPropGripPhase.None ||
                Phase == ShowcasePlayerPropGripPhase.AcquisitionPending ||
                !IsMatchingTarget(target) ||
                !IsFinite(desiredPose) ||
                !IsFinite(actualPose))
            {
                poseIsAuthoritative = false;
                return false;
            }

            CurrentGripPositionError = Vector3.Distance(
                actualPose.position,
                desiredPose.position);
            CurrentGripRotationError = Quaternion.Angle(
                actualPose.rotation,
                desiredPose.rotation);
            LastAppliedPoseFrameId = Time.frameCount;
            if (IsMediumProfile(Profile))
            {
                if (TryGetMediumGripPoints(
                        target,
                        actualPose,
                        out Vector3 leftGrip,
                        out Vector3 rightGrip,
                        out _))
                {
                    MediumLeftHandGripPositionError = Vector3.Distance(
                        leftHand.position,
                        leftGrip);
                    MediumRightHandGripPositionError = Vector3.Distance(
                        rightHand.position,
                        rightGrip);
                }
                else
                {
                    MediumLeftHandGripPositionError = float.PositiveInfinity;
                    MediumRightHandGripPositionError = float.PositiveInfinity;
                }
            }
            else
            {
                MediumLeftHandGripPositionError = 0f;
                MediumRightHandGripPositionError = 0f;
            }
            poseIsAuthoritative = true;
            return true;
        }

        public bool TryGetReleasePose(
            PrototypeInteractionTarget target,
            out Pose pose)
        {
            pose = default;
            if (!HasHumanoidRig ||
                Phase != ShowcasePlayerPropGripPhase.Throw ||
                !IsMatchingTarget(target) ||
                !TryGetActionObjectPose(Profile, target, out pose) ||
                !IsFinite(pose))
            {
                return false;
            }

            // Do not reuse a cached throw target here. Animation release events
            // consume the exact hand-frame pose evaluated in this call.
            poseIsAuthoritative = true;
            return true;
        }

        public bool ConfirmAppliedReleasePose(
            PrototypeInteractionTarget target,
            Pose actualPose)
        {
            if (!HasHumanoidRig ||
                Phase != ShowcasePlayerPropGripPhase.Throw ||
                !IsMatchingTarget(target) ||
                !IsFinite(actualPose) ||
                !TryGetActionObjectPose(Profile, target, out Pose desiredPose) ||
                !IsFinite(desiredPose))
            {
                return false;
            }

            if (!ConfirmAppliedObjectPose(target, desiredPose, actualPose))
            {
                return false;
            }

            ReleasePoseError = CurrentGripPositionError;
            return true;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!HasHumanoidRig)
            {
                MediumIkWeight = 0f;
                return;
            }

            float weight = EvaluateMediumIkWeight();
            MediumIkWeight = weight;
            if (weight <= 0f || Target == null ||
                !TryResolveDesiredObjectPose(Target, out Pose desiredObjectPose) ||
                !IsFinite(desiredObjectPose) ||
                !TryGetMediumGripPoints(
                    Target,
                    desiredObjectPose,
                    out Vector3 left,
                    out Vector3 right,
                    out Quaternion rotation))
            {
                InvalidateMediumDesiredPoseCache();
                SetMediumHandIk(0f, default, default, Quaternion.identity);
                return;
            }

            CacheMediumDesiredPose(Target, desiredObjectPose);
            SetMediumHandIk(weight, left, right, rotation);
        }

        private bool TryResolveDesiredObjectPose(
            PrototypeInteractionTarget target,
            out Pose pose)
        {
            pose = default;
            switch (Phase)
            {
                case ShowcasePlayerPropGripPhase.AcquisitionAttached:
                    return TryGetAcquisitionAttachedPose(target, out pose);
                case ShowcasePlayerPropGripPhase.Carry:
                    bool resolved = TryGetStableCarryPose(target, out pose);
                    MediumIkWeight = target.WeightClass == PrototypeWeightClass.Medium ? 1f : 0f;
                    return resolved;
                case ShowcasePlayerPropGripPhase.Throw:
                    return TryGetThrowPose(target, out pose);
                default:
                    return false;
            }
        }

        private bool TryGetAcquisitionAttachedPose(
            PrototypeInteractionTarget target,
            out Pose pose)
        {
            pose = default;
            if (!hasAcquisitionContactPose ||
                !TryGetStableCarryPose(target, out Pose stablePose))
            {
                return false;
            }

            Pose actionPose = TryGetActionObjectPose(Profile, target, out Pose currentActionPose)
                ? currentActionPose
                : acquisitionContactPose;
            float blend = Mathf.InverseLerp(
                acquisitionStabilizeNormalized,
                acquisitionCompleteNormalized,
                GetActionNormalizedTime());
            pose = LerpPose(actionPose, stablePose, blend);
            MediumIkWeight = IsMediumProfile(Profile) ? blend : 0f;
            return IsFinite(pose);
        }

        private bool TryGetThrowPose(
            PrototypeInteractionTarget target,
            out Pose pose)
        {
            pose = default;
            if (!hasThrowSourcePose ||
                !TryGetActionObjectPose(Profile, target, out Pose actionPose))
            {
                return false;
            }

            float blend = EvaluateThrowBlendWeight();
            pose = LerpPose(throwSourcePose, actionPose, blend);
            MediumIkWeight = IsMediumProfile(Profile) ? 1f - blend : 0f;
            return IsFinite(pose);
        }

        private bool TryGetActionObjectPose(
            ShowcasePlayerPropGripProfile profile,
            PrototypeInteractionTarget target,
            out Pose pose)
        {
            pose = default;
            switch (profile)
            {
                case ShowcasePlayerPropGripProfile.CatchLightCenter:
                    return TryGetLightActionPose(
                        (leftHand.position + rightHand.position) * 0.5f,
                        rightHand,
                        rightLowerArm,
                        out pose);
                case ShowcasePlayerPropGripProfile.CatchLightLeft:
                    return TryGetLightActionPose(
                        (leftHand.position + rightHand.position) * 0.5f,
                        leftHand,
                        leftLowerArm,
                        out pose);
                case ShowcasePlayerPropGripProfile.CatchLightRight:
                    return TryGetLightActionPose(
                        (leftHand.position + rightHand.position) * 0.5f,
                        rightHand,
                        rightLowerArm,
                        out pose);
                case ShowcasePlayerPropGripProfile.PickupLowSmall:
                    return TryGetLightActionPose(
                        rightHand.position,
                        rightHand,
                        rightLowerArm,
                        out pose);
                case ShowcasePlayerPropGripProfile.ThrowLight:
                    return TryGetLightActionPose(
                        rightHand.position,
                        rightHand,
                        rightLowerArm,
                        out pose);
                case ShowcasePlayerPropGripProfile.PickupLowMedium:
                    return TryGetMediumPickupPose(target, out pose);
                case ShowcasePlayerPropGripProfile.CatchMedium:
                case ShowcasePlayerPropGripProfile.ThrowMedium:
                    return TryGetMediumActionPose(target, out pose);
                default:
                    return false;
            }
        }

        private bool TryGetLightActionPose(
            Vector3 origin,
            Transform hand,
            Transform lowerArm,
            out Pose pose)
        {
            pose = default;
            if (!TryGetForearmFrame(hand, lowerArm, out Quaternion frame))
            {
                return false;
            }

            Quaternion rotation = frame * Quaternion.Euler(lightActionLocalRotationEuler);
            pose = new Pose(origin + frame * lightActionLocalOffset, rotation);
            return IsFinite(pose);
        }

        private bool TryGetMediumActionPose(
            PrototypeInteractionTarget target,
            out Pose pose)
        {
            pose = default;
            if (target == null ||
                !target.TryGetComponent(out BoxCollider chairCollider) ||
                chairCollider.transform != target.transform ||
                !TryGetBilateralHandFrame(out Quaternion handFrame))
            {
                return false;
            }

            Vector3 handMidpoint = (leftHand.position + rightHand.position) * 0.5f;
            Vector3 localGripMidpoint = GetMediumLocalGripMidpoint(chairCollider);
            Vector3 scaledLocalGrip = Vector3.Scale(
                localGripMidpoint,
                Abs(target.transform.lossyScale));
            Vector3 rootPosition = handMidpoint - handFrame * scaledLocalGrip;
            pose = new Pose(rootPosition, handFrame);
            return IsFinite(pose);
        }

        private bool TryGetMediumPickupPose(
            PrototypeInteractionTarget target,
            out Pose pose)
        {
            pose = default;
            if (target == null ||
                !target.TryGetComponent(out BoxCollider chairCollider) ||
                chairCollider.transform != target.transform ||
                !TryGetForearmFrame(rightHand, rightLowerArm, out Quaternion handFrame))
            {
                return false;
            }

            Vector3 localGripMidpoint = GetMediumLocalGripMidpoint(chairCollider);
            Vector3 scaledLocalGrip = Vector3.Scale(
                localGripMidpoint,
                Abs(target.transform.lossyScale));
            pose = new Pose(
                rightHand.position - handFrame * scaledLocalGrip,
                handFrame);
            return IsFinite(pose);
        }

        private bool TryGetStableCarryPose(
            PrototypeInteractionTarget target,
            out Pose pose)
        {
            pose = default;
            if (target == null)
            {
                return false;
            }

            if (target.WeightClass == PrototypeWeightClass.Light)
            {
                if (Profile == ShowcasePlayerPropGripProfile.PickupLowSmall)
                {
                    return TryGetActionObjectPose(Profile, target, out pose);
                }

                if (stableLightSocket == null ||
                    !stableLightSocket.TryGetWorldPose(
                        out Vector3 lightPosition,
                        out Quaternion lightRotation) ||
                    !TryNormalize(lightRotation, out Quaternion normalizedLightRotation))
                {
                    return false;
                }

                pose = new Pose(lightPosition, normalizedLightRotation);
                return IsFinite(pose);
            }

            if (target.WeightClass != PrototypeWeightClass.Medium ||
                carryAnchor == null || actorRoot == null)
            {
                return false;
            }

            Vector3 position = carryAnchor.position -
                actorRoot.forward * MediumCarryAnchorRetraction +
                Vector3.down * MediumCarryDownOffset;
            if (!TryNormalize(actorRoot.rotation, out Quaternion rotation))
            {
                return false;
            }

            pose = new Pose(position, rotation);
            return IsFinite(pose);
        }

        private bool TryGetForearmFrame(
            Transform hand,
            Transform lowerArm,
            out Quaternion frame)
        {
            frame = Quaternion.identity;
            if (hand == null || lowerArm == null)
            {
                return false;
            }

            Vector3 up = hand.position - lowerArm.position;
            if (up.sqrMagnitude <= 0.000001f)
            {
                up = hand.up;
            }
            if (up.sqrMagnitude <= 0.000001f)
            {
                return false;
            }
            up.Normalize();

            Vector3 referenceForward = actorRoot != null ? actorRoot.forward : transform.forward;
            Vector3 forward = Vector3.ProjectOnPlane(referenceForward, up);
            if (forward.sqrMagnitude <= 0.000001f)
            {
                forward = Vector3.ProjectOnPlane(hand.forward, up);
            }
            if (forward.sqrMagnitude <= 0.000001f)
            {
                forward = Vector3.Cross(up, Vector3.right);
            }
            if (forward.sqrMagnitude <= 0.000001f)
            {
                return false;
            }
            forward.Normalize();

            frame = Quaternion.LookRotation(forward, up);
            return IsFinite(frame);
        }

        private bool TryGetBilateralHandFrame(out Quaternion frame)
        {
            frame = Quaternion.identity;
            Vector3 right = rightHand.position - leftHand.position;
            if (right.sqrMagnitude <= 0.000001f)
            {
                return false;
            }
            right.Normalize();

            Vector3 forearmUp =
                (leftHand.position - leftLowerArm.position) +
                (rightHand.position - rightLowerArm.position);
            Vector3 fallbackUp = actorRoot != null ? actorRoot.up : transform.up;
            Vector3 up = Vector3.ProjectOnPlane(forearmUp, right);
            if (up.sqrMagnitude <= 0.000001f)
            {
                up = Vector3.ProjectOnPlane(fallbackUp, right);
            }
            if (up.sqrMagnitude <= 0.000001f)
            {
                return false;
            }
            up.Normalize();
            if (Vector3.Dot(up, fallbackUp) < 0f)
            {
                up = -up;
            }

            Vector3 forward = Vector3.Cross(right, up);
            if (forward.sqrMagnitude <= 0.000001f)
            {
                return false;
            }
            forward.Normalize();
            Vector3 referenceForward = actorRoot != null ? actorRoot.forward : transform.forward;
            if (Vector3.Dot(forward, referenceForward) < 0f)
            {
                forward = -forward;
                up = -up;
            }

            frame = Quaternion.LookRotation(forward, up);
            return IsFinite(frame);
        }

        private bool TryGetMediumGripPoints(
            PrototypeInteractionTarget target,
            Pose objectPose,
            out Vector3 left,
            out Vector3 right,
            out Quaternion rotation)
        {
            left = default;
            right = default;
            rotation = Quaternion.identity;
            if (target == null ||
                !target.TryGetComponent(out BoxCollider chairCollider) ||
                chairCollider.transform != target.transform)
            {
                return false;
            }

            Vector3 scaledLocalGrip = Vector3.Scale(
                GetMediumLocalGripMidpoint(chairCollider),
                Abs(target.transform.lossyScale));
            Vector3 midpoint = objectPose.position + objectPose.rotation * scaledLocalGrip;
            Vector3 side = objectPose.rotation * Vector3.right;
            left = midpoint - side * mediumLateralGrip;
            right = midpoint + side * mediumLateralGrip;
            rotation = Quaternion.LookRotation(
                -(objectPose.rotation * Vector3.forward),
                objectPose.rotation * Vector3.up);
            return IsFinite(left) && IsFinite(right) && IsFinite(rotation);
        }

        private void CacheMediumDesiredPose(
            PrototypeInteractionTarget target,
            Pose pose)
        {
            mediumDesiredPoseForFrame = pose;
            mediumDesiredPoseFrame = Time.frameCount;
            mediumDesiredPoseSequence = SequenceId;
            mediumDesiredPosePhase = Phase;
            mediumDesiredPoseTarget = target;
        }

        private bool TryGetCachedMediumDesiredPose(
            PrototypeInteractionTarget target,
            out Pose pose)
        {
            pose = mediumDesiredPoseForFrame;
            return mediumDesiredPoseFrame == Time.frameCount &&
                mediumDesiredPoseSequence == SequenceId &&
                mediumDesiredPosePhase == Phase &&
                mediumDesiredPoseTarget == target &&
                IsFinite(pose);
        }

        private void InvalidateMediumDesiredPoseCache()
        {
            mediumDesiredPoseForFrame = default;
            mediumDesiredPoseFrame = -1;
            mediumDesiredPoseSequence = 0;
            mediumDesiredPosePhase = ShowcasePlayerPropGripPhase.None;
            mediumDesiredPoseTarget = null;
        }

        private Vector3 GetMediumLocalGripMidpoint(BoxCollider chairCollider)
        {
            return chairCollider.center +
                Vector3.up * (chairCollider.size.y * 0.5f * mediumBackrestHeight) +
                Vector3.back *
                    ((chairCollider.size.z * 0.5f * MediumGripNearFaceFactor) +
                     MediumGripPalmClearance);
        }

        private float EvaluateMediumIkWeight()
        {
            if (!IsMediumProfile(Profile) || Target == null)
            {
                return 0f;
            }

            switch (Phase)
            {
                case ShowcasePlayerPropGripPhase.AcquisitionAttached:
                    return EvaluateAcquisitionBlendWeight();
                case ShowcasePlayerPropGripPhase.Carry:
                    return 1f;
                case ShowcasePlayerPropGripPhase.Throw:
                    return 1f - EvaluateThrowBlendWeight();
                default:
                    return 0f;
            }
        }

        private float EvaluateAcquisitionBlendWeight()
        {
            return Mathf.InverseLerp(
                acquisitionStabilizeNormalized,
                acquisitionCompleteNormalized,
                GetActionNormalizedTime());
        }

        private float EvaluateThrowBlendWeight()
        {
            return Mathf.InverseLerp(
                0f,
                throwReleaseNormalized,
                GetActionNormalizedTime());
        }

        private float GetActionNormalizedTime()
        {
            if (animator == null || animator.layerCount <= 0)
            {
                return 0f;
            }

            AnimatorStateInfo info = animator.IsInTransition(0)
                ? animator.GetNextAnimatorStateInfo(0)
                : animator.GetCurrentAnimatorStateInfo(0);
            return Mathf.Clamp01(info.normalizedTime);
        }

        private void SetMediumHandIk(
            float weight,
            Vector3 left,
            Vector3 right,
            Quaternion rotation)
        {
            weight = Mathf.Clamp01(weight);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, weight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, weight);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, weight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, weight);
            if (weight <= 0f)
            {
                return;
            }

            animator.SetIKPosition(AvatarIKGoal.LeftHand, left);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, rotation);
            animator.SetIKPosition(AvatarIKGoal.RightHand, right);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rotation);
        }

        private void ResolveBones()
        {
            leftHand = null;
            rightHand = null;
            leftLowerArm = null;
            rightLowerArm = null;
            if (animator == null || !animator.isHuman)
            {
                poseIsAuthoritative = false;
                return;
            }

            leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            if (!HasHumanoidRig)
            {
                poseIsAuthoritative = false;
            }
        }

        private bool TryResolveAcquisitionProfile(
            PrototypeInteractionTarget target,
            ShowcasePlayerAnimationCue cue,
            out ShowcasePlayerPropGripProfile profile)
        {
            profile = ShowcasePlayerPropGripProfile.None;
            if (target.WeightClass == PrototypeWeightClass.Medium)
            {
                if (cue == ShowcasePlayerAnimationCue.PickupLowMedium)
                {
                    profile = ShowcasePlayerPropGripProfile.PickupLowMedium;
                    return true;
                }
                if (cue != ShowcasePlayerAnimationCue.CatchMedium)
                {
                    return false;
                }
                profile = ShowcasePlayerPropGripProfile.CatchMedium;
                return true;
            }

            if (target.WeightClass != PrototypeWeightClass.Light)
            {
                return false;
            }

            switch (cue)
            {
                case ShowcasePlayerAnimationCue.PickupLowSmall:
                    profile = ShowcasePlayerPropGripProfile.PickupLowSmall;
                    return true;
                case ShowcasePlayerAnimationCue.CatchSmallCenter:
                    profile = ShowcasePlayerPropGripProfile.CatchLightCenter;
                    return true;
                case ShowcasePlayerAnimationCue.CatchSmallLeft:
                    profile = ShowcasePlayerPropGripProfile.CatchLightLeft;
                    return true;
                case ShowcasePlayerAnimationCue.CatchSmallRight:
                    profile = ShowcasePlayerPropGripProfile.CatchLightRight;
                    return true;
                default:
                    return false;
            }
        }

        private bool TryResolveThrowProfile(
            PrototypeInteractionTarget target,
            ShowcasePlayerAnimationCue cue,
            out ShowcasePlayerPropGripProfile profile)
        {
            profile = ShowcasePlayerPropGripProfile.None;
            if (target.WeightClass == PrototypeWeightClass.Light &&
                cue == ShowcasePlayerAnimationCue.ThrowSmall)
            {
                profile = ShowcasePlayerPropGripProfile.ThrowLight;
                return true;
            }

            if (target.WeightClass == PrototypeWeightClass.Medium &&
                cue == ShowcasePlayerAnimationCue.ThrowMedium)
            {
                profile = ShowcasePlayerPropGripProfile.ThrowMedium;
                return true;
            }

            return false;
        }

        private bool IsMatchingTarget(PrototypeInteractionTarget target)
        {
            return target != null && Target == target;
        }

        private void AdvanceSequence()
        {
            SequenceId++;
            if (SequenceId == 0)
            {
                SequenceId = 1;
            }
        }

        private void ClearActiveState()
        {
            Phase = ShowcasePlayerPropGripPhase.None;
            Profile = ShowcasePlayerPropGripProfile.None;
            Target = null;
            MediumIkWeight = 0f;
            poseIsAuthoritative = false;
            hasAcquisitionContactPose = false;
            hasThrowSourcePose = false;
            carryProfileBeforeThrow = ShowcasePlayerPropGripProfile.None;
            acquisitionContactPose = default;
            throwSourcePose = default;
            acquisitionStabilizeNormalized = 0f;
            acquisitionCompleteNormalized = 0f;
            throwReleaseNormalized = 0f;
            InvalidateMediumDesiredPoseCache();
        }

        private static bool IsMediumProfile(ShowcasePlayerPropGripProfile profile)
        {
            return profile == ShowcasePlayerPropGripProfile.PickupLowMedium ||
                profile == ShowcasePlayerPropGripProfile.CatchMedium ||
                profile == ShowcasePlayerPropGripProfile.ThrowMedium;
        }

        private static Pose LerpPose(Pose from, Pose to, float t)
        {
            t = Mathf.Clamp01(t);
            return new Pose(
                Vector3.Lerp(from.position, to.position, t),
                Quaternion.Slerp(from.rotation, to.rotation, t));
        }

        private static Vector3 Abs(Vector3 value)
        {
            return new Vector3(
                Mathf.Abs(value.x),
                Mathf.Abs(value.y),
                Mathf.Abs(value.z));
        }

        private static bool IsFinite(Pose pose)
        {
            return IsFinite(pose.position) && IsFinite(pose.rotation);
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(Quaternion value)
        {
            return IsFinite(value.x) && IsFinite(value.y) &&
                IsFinite(value.z) && IsFinite(value.w) &&
                value.x * value.x + value.y * value.y +
                value.z * value.z + value.w * value.w > 0.000001f;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool TryNormalize(Quaternion value, out Quaternion normalized)
        {
            normalized = Quaternion.identity;
            if (!IsFinite(value))
            {
                return false;
            }

            float magnitude = Mathf.Sqrt(
                value.x * value.x + value.y * value.y +
                value.z * value.z + value.w * value.w);
            if (magnitude <= 0.000001f)
            {
                return false;
            }

            float inverse = 1f / magnitude;
            normalized = new Quaternion(
                value.x * inverse,
                value.y * inverse,
                value.z * inverse,
                value.w * inverse);
            return true;
        }
    }
}
