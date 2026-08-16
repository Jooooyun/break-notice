using System.Collections.Generic;
using BreakNotice.Gameplay.Interaction;
using Unity.Cinemachine;
using Unity.Profiling;
using UnityEngine;

namespace BreakNotice.Presentation
{
    public enum ShowcaseCameraConstraintStatus
    {
        Uninitialized,
        SolvedOpenSpace,
        SolvedGameplayOverrideReduced,
        SolvedStaticCompression,
        SolvedActorCompression,
        DynamicActorNoSolution,
        PreviousPoseRevalidated,
        NoValidPose
    }

    public enum ShowcaseCameraNoValidPoseReason
    {
        None,
        StaticWorld,
        NearPlane,
        PlayerEnvelope,
        CarryEnvelope,
        DynamicActor,
        MultipleConstraints
    }

    public enum ShowcaseCameraSolutionFamily
    {
        Requested,
        BoomCompressed,
        GameplayReduced,
        ShoulderReduced,
        EmergencyCentered
    }

    internal readonly struct ShowcaseCameraConstraintFrame
    {
        public ShowcaseCameraConstraintFrame(
            Transform playerRoot,
            CharacterController playerController,
            Transform stableAnchor,
            PrototypeInteractionTarget carriedTarget,
            float coreTargetHeight,
            float coreDistance,
            float coreShoulderOffset,
            float coreShoulderHeight,
            float coreVerticalArmLength,
            Vector4 gameplayOffsets,
            float yaw,
            float pitch,
            float cameraRadius,
            float clearanceMargin,
            float collisionReturnDuration,
            ShowcaseContextualCornerFrameSnapshot cornerSnapshot)
        {
            PlayerRoot = playerRoot;
            PlayerController = playerController;
            StableAnchor = stableAnchor;
            CarriedTarget = carriedTarget;
            CoreTargetHeight = coreTargetHeight;
            CoreDistance = coreDistance;
            CoreShoulderOffset = coreShoulderOffset;
            CoreShoulderHeight = coreShoulderHeight;
            CoreVerticalArmLength = coreVerticalArmLength;
            GameplayOffsets = gameplayOffsets;
            Yaw = yaw;
            Pitch = pitch;
            CameraRadius = cameraRadius;
            ClearanceMargin = clearanceMargin;
            CollisionReturnDuration = collisionReturnDuration;
            CornerSnapshot = cornerSnapshot;
        }

        public Transform PlayerRoot { get; }
        public CharacterController PlayerController { get; }
        public Transform StableAnchor { get; }
        public PrototypeInteractionTarget CarriedTarget { get; }
        public float CoreTargetHeight { get; }
        public float CoreDistance { get; }
        public float CoreShoulderOffset { get; }
        public float CoreShoulderHeight { get; }
        public float CoreVerticalArmLength { get; }
        public Vector4 GameplayOffsets { get; }
        public float Yaw { get; }
        public float Pitch { get; }
        public float CameraRadius { get; }
        public float ClearanceMargin { get; }
        public float CollisionReturnDuration { get; }
        public ShowcaseContextualCornerFrameSnapshot CornerSnapshot { get; }
        public bool IsValid => PlayerRoot != null && StableAnchor != null;
        public bool HasGameplayOverride => GameplayOffsets.sqrMagnitude > 0.000001f;
    }

    /// <summary>
    /// The sole spatial constraint authority for the showcase camera.  The
    /// ThirdPersonFollow component produces an unconstrained rig; this extension
    /// resolves that CameraState before the Brain writes the output Camera.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShowcaseCameraConstraintSolver : CinemachineExtension
    {
        private const int QueryCapacity = 96;
        private const float ClearHoldDuration = 0.1f;
        private const float SubjectSkin = 0.08f;
        private const float PlayerEnvelopeRadiusHeightFactor = 0.43f;
        private const float MinimumBoomDistance = 0.2f;
        private const int BoomSearchSteps = 48;
        private const int ScalarBracketSteps = 16;
        private const int ScalarSearchIterations = 10;
        private const float ScalarTolerance = 0.002f;
        private const float BoomTolerance = 0.005f;
        private const float CornerSightlineProbeDistance = 8f;
        private const float CornerSightlineProbeRadius = 0.02f;
        private const float RecoveryGameplayEpsilon = 0.01f;
        private const float RecoveryShoulderEpsilon = 0.01f;
        private const float RecoveryBoomEpsilon = 0.02f;
        private const float BranchSwitchCooldown = 0.15f;
        private const float MaterialGameplayGain = 0.05f;
        private const float MaterialShoulderGain = 0.05f;
        private const float MaterialBoomGain = 0.10f;
        private const float ContactNormalQuantization = 0.05f;
        private const float ContactPlaneQuantization = 0.02f;
        private const int MaximumPhysicsQueriesPerFrame = 24;
        private const int MaximumPhysicsQueriesPerCandidate = 12;
        // Ordinary solving remains bounded by MaximumPhysicsQueriesPerFrame.
        // This reserve is reachable only after ordinary solving has failed and
        // exists so fail-closed validation cannot be defeated by the budget
        // exhaustion that triggered it.
        private const int FailClosedSafetyQueryReserve = 40;
        private const float ConstrainedRefinementInterval = 0.05f;
        private const float CornerProbeInterval = 0.04f;
        private const float StaticReleaseEvidenceDuration = 0.18f;
        private const float StaticReleaseSpatialMargin = 0.075f;
        private const float SubjectReleaseEvidenceDuration = 0.10f;

        private static readonly ProfilerMarker FinalizeMarker =
            new("BreakNotice.CameraConstraint.Finalize");
        private static readonly ProfilerMarker FastSafetyMarker =
            new("BreakNotice.CameraConstraint.FastSafety");
        private static readonly ProfilerMarker FullSolveMarker =
            new("BreakNotice.CameraConstraint.FullSolve");
        private static readonly ProfilerMarker CornerProbeMarker =
            new("BreakNotice.CameraConstraint.CornerProbe");

        [SerializeField]
        private LayerMask hardWorldLayers;

        [SerializeField]
        private LayerMask actorProxyLayers;

        private int hardWorldQueryMask;

        private ShowcaseCameraConstraintFrame frameInput;
        private bool hasFrameInput;
        private SpatialTuple requestedTuple;
        private SpatialTuple safeTargetTuple;
        private SpatialTuple resolvedTuple;
        private SpatialTuple observedRecoveryTarget;
        private SpatialTuple recoverySourceTuple;
        private ShowcaseCameraSolutionFamily activeSolutionFamily;
        private ShowcaseCameraSolutionFamily observedRecoveryFamily;
        private GeometrySignature activeGeometrySignature;
        private GeometrySignature observedRecoveryGeometrySignature;
        private bool hasResolvedTuple;
        private bool constraintRecoveryRequired;
        private bool recoveryActive;
        private float recoveryElapsed;
        private float staticReleaseClearTimer;
        private float branchSwitchCooldownRemaining;
        private bool hardConstraintActiveThisFrame;
        private int lastEvaluationFrame = -1;
        private int evaluationsThisFrame;
        private int physicsQueriesThisFrame;
        private int failClosedSafetyQueriesThisFrame;
        private int fullSolvesThisFrame;
        private int tupleCommitsThisFrame;
        private int cornerCandidateEvaluationsThisFrame;
        private int queryBudgetExceededCount;
        private int fastPathFrameCount;
        private bool queryBudgetExceededThisFrame;
        private bool failClosedSafetyPass;
        private bool fastSafetySucceededThisFrame;
        private bool collectGeometrySignatureThisPass = true;
        private float cornerProbeElapsed;
        private ShowcaseContextualCornerMode nextCornerProbeMode =
            ShowcaseContextualCornerMode.RightDefault;
        private ShowcaseContextualCornerCandidate cachedRightDefaultCandidate;
        private ShowcaseContextualCornerCandidate cachedRightLeanCandidate;
        private ShowcaseContextualCornerCandidate cachedTemporaryLeftCandidate;
        private float cachedSubjectForwardSightlineClearance =
            CornerSightlineProbeDistance;
        private Collider currentHardBlocker;
        private Transform currentActorProxy;
        private Pose lastSolvedPose;
        private bool hasLastSolvedPose;
        private readonly RaycastHit[] castHits = new RaycastHit[QueryCapacity];
        private readonly Collider[] overlapHits = new Collider[QueryCapacity];
        private readonly List<HardContact> hardContacts = new(QueryCapacity);
        private readonly List<GeometryPlaneKey> geometryPlanes = new(QueryCapacity);
        private readonly List<ActorProxy> actorProxies = new();
        private readonly HashSet<int> hardWorldColliderIds = new();
        private PrototypeInteractionTarget cachedCarriedTarget;
        private Collider[] cachedCarryColliders = System.Array.Empty<Collider>();
        private readonly ShowcaseContextualCornerAimResolver cornerResolver = new();
        private readonly ShowcaseCameraContactLatch staticContactLatch = new();
        private readonly ShowcaseCameraContactLatch subjectContactLatch = new();
        private float cornerShoulderIntentFactor =
            ShowcaseContextualCornerAimResolver.RightDefaultShoulderFactor;
        private bool contactLatchInitialized;
        private bool subjectLatchInitialized;
        private bool emergencyShoulderLatched;
        private float emergencyHorizontalShoulderFactor = 1f;
        private int emergencyShoulderEpisodeId;
        private Collider latchedHardBlocker;
        private Transform latchedSubjectActorProxy;
        private float requestedBoomDistance;
        private Vector3 finalStableAnchorLocalCameraOffset;
        private int activeSoftOccluderCount;
        private ShowcaseCameraSoftOcclusionPresenter softOcclusionPresenter;
        private int lastContactAdvanceFrame = -1;
        private int lastSubjectAdvanceFrame = -1;
        private int cornerContactEpisodeId;
        private bool cornerContactDecisionLatched;
        private float cornerContactShoulderFactor =
            ShowcaseContextualCornerAimResolver.RightDefaultShoulderFactor;

        public ShowcaseCameraConstraintStatus Status { get; private set; }
            = ShowcaseCameraConstraintStatus.Uninitialized;

        public ShowcaseCameraNoValidPoseReason NoValidPoseReason { get; private set; }
            = ShowcaseCameraNoValidPoseReason.None;

        public float ResolvedGameplayWeight => hasResolvedTuple
            ? resolvedTuple.GameplayWeight
            : 1f;

        public float ResolvedShoulderScale => hasResolvedTuple
            ? resolvedTuple.ShoulderScale
            : 1f;

        public float ResolvedBoomDistance => hasResolvedTuple
            ? resolvedTuple.BoomDistance
            : 0f;

        public float SafeBoomFraction { get; private set; } = 1f;

        public Collider CurrentHardBlocker => currentHardBlocker != null
            ? currentHardBlocker
            : null;

        public Transform CurrentActorProxy => currentActorProxy != null
            ? currentActorProxy
            : null;

        public Pose LastSolvedPose => lastSolvedPose;

        public bool HasLastSolvedPose => hasLastSolvedPose;

        public int LastEvaluationFrame => lastEvaluationFrame;

        public int EvaluationsThisFrame => evaluationsThisFrame;

        public int PostBrainTransformWriteCount => 0;

        public bool SubjectConstraintApplied { get; private set; }

        public bool OutputPoseUsedAsProfileSource => false;

        public bool HardConstraintActiveThisFrame => hardConstraintActiveThisFrame;

        public float GameplayRecoveryTimer => staticReleaseClearTimer;

        public float ShoulderRecoveryTimer => staticReleaseClearTimer;

        public float BoomRecoveryTimer => staticReleaseClearTimer;

        public ShowcaseContextualCornerMode ContextualCornerMode =>
            cornerResolver.CurrentMode;

        public float ContextualCornerBlendProgress =>
            cornerResolver.CurrentBlendProgress;

        public float ContextualCornerShoulderFactor =>
            cornerResolver.CurrentShoulderFactor;

        public int ContextualCornerActiveBlockerId =>
            cornerResolver.ActiveBlockerId;

        public int ContextualCornerTransitionCount =>
            cornerResolver.TransitionCount;

        public ShowcaseCameraSolutionFamily ActiveSolutionFamily =>
            activeSolutionFamily;

        public ulong ActiveGeometrySignature => activeGeometrySignature.Value;

        public int QuerySaturationCount { get; private set; }

        public int PhysicsQueriesThisFrame => physicsQueriesThisFrame;

        public int FailClosedSafetyQueriesThisFrame =>
            failClosedSafetyQueriesThisFrame;

        public int PhysicsQueryBudgetPerFrame => MaximumPhysicsQueriesPerFrame;

        public int FullSolvesThisFrame => fullSolvesThisFrame;

        public int TupleCommitsThisFrame => tupleCommitsThisFrame;

        public int CornerCandidateEvaluationsThisFrame =>
            cornerCandidateEvaluationsThisFrame;

        public int QueryBudgetExceededCount => queryBudgetExceededCount;

        public bool QueryBudgetExceededThisFrame => queryBudgetExceededThisFrame;

        public bool FastSafetySucceededThisFrame => fastSafetySucceededThisFrame;

        public int FastPathFrameCount => fastPathFrameCount;

        public ShowcaseStaticCameraContactPhase StaticContactPhase =>
            staticContactLatch.Phase;

        public int ContactEpisodeId => staticContactLatch.ContactEpisodeId;

        public int RestoreStartCount => staticContactLatch.RestoreStartCount;

        public float RequestedBoomDistance => requestedBoomDistance;

        public bool EmergencyShoulderLatched => emergencyShoulderLatched;

        public Collider ActiveHardBlocker => latchedHardBlocker != null
            ? latchedHardBlocker
            : null;

        public int ActiveSoftOccluderCount => activeSoftOccluderCount;

        public Vector3 FinalStableAnchorLocalCameraOffset =>
            finalStableAnchorLocalCameraOffset;

        public ShowcaseStaticCameraContactPhase SubjectContactPhase =>
            subjectContactLatch.Phase;

        public int SubjectContactEpisodeId => subjectContactLatch.ContactEpisodeId;

        public int SubjectRestoreStartCount => subjectContactLatch.RestoreStartCount;

        public float SubjectResolvedBoomDistance => subjectContactLatch.CurrentBoom;

        protected override void Awake()
        {
            base.Awake();
            ResolveLayerMasks();
            softOcclusionPresenter =
                GetComponent<ShowcaseCameraSoftOcclusionPresenter>();
            if (softOcclusionPresenter == null)
            {
                softOcclusionPresenter = gameObject.AddComponent<
                    ShowcaseCameraSoftOcclusionPresenter>();
            }
            RefreshHardWorldColliders();
            RefreshActorProxies();
            cachedRightDefaultCandidate = CreateUnavailableCornerCandidate(
                ShowcaseContextualCornerMode.RightDefault);
            cachedRightLeanCandidate = CreateUnavailableCornerCandidate(
                ShowcaseContextualCornerMode.RightLean);
            cachedTemporaryLeftCandidate = CreateUnavailableCornerCandidate(
                ShowcaseContextualCornerMode.TemporaryLeft);
        }

        private void OnValidate()
        {
            ResolveLayerMasks();
        }

        internal void SetFrameInput(ShowcaseCameraConstraintFrame value)
        {
            frameInput = value;
            hasFrameInput = value.IsValid;
            if (cachedCarriedTarget != value.CarriedTarget)
            {
                cachedCarriedTarget = value.CarriedTarget;
                cachedCarryColliders = cachedCarriedTarget != null
                    ? cachedCarriedTarget.GetComponentsInChildren<Collider>(true)
                    : System.Array.Empty<Collider>();
            }
        }

        public void ResetSolverState()
        {
            requestedTuple = default;
            safeTargetTuple = default;
            resolvedTuple = default;
            observedRecoveryTarget = default;
            recoverySourceTuple = default;
            activeSolutionFamily = ShowcaseCameraSolutionFamily.Requested;
            observedRecoveryFamily = ShowcaseCameraSolutionFamily.Requested;
            activeGeometrySignature = default;
            observedRecoveryGeometrySignature = default;
            hasResolvedTuple = false;
            constraintRecoveryRequired = false;
            recoveryActive = false;
            recoveryElapsed = 0f;
            staticReleaseClearTimer = 0f;
            branchSwitchCooldownRemaining = 0f;
            hardConstraintActiveThisFrame = false;
            currentHardBlocker = null;
            currentActorProxy = null;
            hasLastSolvedPose = false;
            SubjectConstraintApplied = false;
            QuerySaturationCount = 0;
            physicsQueriesThisFrame = 0;
            failClosedSafetyQueriesThisFrame = 0;
            fullSolvesThisFrame = 0;
            tupleCommitsThisFrame = 0;
            cornerCandidateEvaluationsThisFrame = 0;
            queryBudgetExceededCount = 0;
            fastPathFrameCount = 0;
            queryBudgetExceededThisFrame = false;
            failClosedSafetyPass = false;
            fastSafetySucceededThisFrame = false;
            collectGeometrySignatureThisPass = true;
            cornerProbeElapsed = CornerProbeInterval;
            nextCornerProbeMode = ShowcaseContextualCornerMode.RightDefault;
            cachedRightDefaultCandidate = CreateUnavailableCornerCandidate(
                ShowcaseContextualCornerMode.RightDefault);
            cachedRightLeanCandidate = CreateUnavailableCornerCandidate(
                ShowcaseContextualCornerMode.RightLean);
            cachedTemporaryLeftCandidate = CreateUnavailableCornerCandidate(
                ShowcaseContextualCornerMode.TemporaryLeft);
            cachedSubjectForwardSightlineClearance =
                CornerSightlineProbeDistance;
            cachedCarriedTarget = frameInput.CarriedTarget;
            cachedCarryColliders = cachedCarriedTarget != null
                ? cachedCarriedTarget.GetComponentsInChildren<Collider>(true)
                : System.Array.Empty<Collider>();
            cornerShoulderIntentFactor =
                ShowcaseContextualCornerAimResolver.RightDefaultShoulderFactor;
            cornerResolver.Reset(frameInput.CornerSnapshot.ResetRevision);
            requestedBoomDistance = Mathf.Max(
                MinimumBoomDistance,
                ComposeProfile(1f).Distance);
            staticContactLatch.Reset(requestedBoomDistance);
            contactLatchInitialized = hasFrameInput;
            subjectContactLatch.Reset(requestedBoomDistance);
            subjectLatchInitialized = hasFrameInput;
            emergencyShoulderLatched = false;
            emergencyHorizontalShoulderFactor = 1f;
            emergencyShoulderEpisodeId = 0;
            latchedHardBlocker = null;
            latchedSubjectActorProxy = null;
            finalStableAnchorLocalCameraOffset = Vector3.zero;
            activeSoftOccluderCount = 0;
            softOcclusionPresenter?.ResetVisibility();
            lastContactAdvanceFrame = -1;
            lastSubjectAdvanceFrame = -1;
            cornerContactEpisodeId = 0;
            cornerContactDecisionLatched = false;
            cornerContactShoulderFactor =
                ShowcaseContextualCornerAimResolver.RightDefaultShoulderFactor;
            Status = ShowcaseCameraConstraintStatus.Uninitialized;
            NoValidPoseReason = ShowcaseCameraNoValidPoseReason.None;
            RefreshHardWorldColliders();
            RefreshActorProxies();
        }

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage,
            ref CameraState state,
            float deltaTime)
        {
            if (stage != CinemachineCore.Stage.Finalize ||
                !hasFrameInput ||
                !frameInput.IsValid)
            {
                return;
            }

            if (lastEvaluationFrame != Time.frameCount)
            {
                lastEvaluationFrame = Time.frameCount;
                evaluationsThisFrame = 0;
                physicsQueriesThisFrame = 0;
                failClosedSafetyQueriesThisFrame = 0;
                fullSolvesThisFrame = 0;
                tupleCommitsThisFrame = 0;
                cornerCandidateEvaluationsThisFrame = 0;
                queryBudgetExceededThisFrame = false;
                failClosedSafetyPass = false;
                fastSafetySucceededThisFrame = false;
            }
            evaluationsThisFrame++;

            Quaternion orientation = state.GetFinalOrientation();
            using (FinalizeMarker.Auto())
            {
                Solve(ref state, orientation, state.Lens, deltaTime);
            }
        }

        private void Solve(
            ref CameraState state,
            Quaternion cameraOrientation,
            LensSettings lens,
            float deltaTime)
        {
            if (evaluationsThisFrame > 1 && hasLastSolvedPose)
            {
                ApplySolvedPosition(
                    ref state,
                    lastSolvedPose.position,
                    cameraOrientation,
                    Status);
                return;
            }

            bool forceImmediate = deltaTime < 0f;
            float safeDeltaTime = Mathf.Max(0f, deltaTime);
            bool advanceContactState = lastContactAdvanceFrame != Time.frameCount;
            bool advanceSubjectState = lastSubjectAdvanceFrame != Time.frameCount;
            SubjectConstraintApplied = false;
            NoValidPoseReason = ShowcaseCameraNoValidPoseReason.None;
            currentHardBlocker = null;
            currentActorProxy = null;
            hardConstraintActiveThisFrame = false;
            activeSoftOccluderCount = 0;

            ResolveContextualCornerIntent(cameraOrientation, lens, safeDeltaTime);
            requestedTuple = BuildRequestedTuple();
            requestedBoomDistance = requestedTuple.BoomDistance;
            if (!contactLatchInitialized || forceImmediate)
            {
                staticContactLatch.Reset(requestedBoomDistance);
                contactLatchInitialized = true;
                emergencyShoulderLatched = false;
                emergencyHorizontalShoulderFactor = 1f;
                emergencyShoulderEpisodeId = 0;
                cornerContactEpisodeId = 0;
                cornerContactDecisionLatched = false;
                latchedHardBlocker = null;
            }
            if (!subjectLatchInitialized || forceImmediate)
            {
                subjectContactLatch.Reset(requestedBoomDistance);
                subjectLatchInitialized = true;
                latchedSubjectActorProxy = null;
            }

            float contactShoulderFactor = emergencyShoulderLatched &&
                    staticContactLatch.Phase !=
                        ShowcaseStaticCameraContactPhase.Clear
                ? emergencyHorizontalShoulderFactor
                : 1f;
            SpatialTuple fullRequestedTuple = new SpatialTuple(
                1f,
                1f,
                requestedBoomDistance);
            bool requestedStaticSafe;
            CameraCandidate requestedStaticCandidate;
            ConstraintFailure requestedStaticFailure;
            using (FastSafetyMarker.Auto())
            {
                requestedStaticSafe = EvaluateStaticTuple(
                    fullRequestedTuple,
                    cameraOrientation,
                    lens,
                    out requestedStaticCandidate,
                    out requestedStaticFailure);
            }
            fastSafetySucceededThisFrame = requestedStaticSafe;

            bool requestedBlockedForLatch = !requestedStaticSafe;
            float safeStaticBoom = staticContactLatch.CurrentBoom;
            SpatialTuple validatedStaticTuple = fullRequestedTuple;
            CameraCandidate validatedStaticCandidate = requestedStaticCandidate;
            ConstraintFailure limitingFailure = requestedStaticFailure;

            if (!requestedStaticSafe)
            {
                fullSolvesThisFrame = 1;
                bool foundStaticSolution = TrySolveMaximumStaticBoom(
                    contactShoulderFactor,
                    cameraOrientation,
                    lens,
                    out validatedStaticTuple,
                    out validatedStaticCandidate,
                    out ConstraintFailure boomFailure);
                limitingFailure = ConstraintFailure.Prefer(
                    limitingFailure,
                    boomFailure);

                if (!foundStaticSolution && !emergencyShoulderLatched)
                {
                    foundStaticSolution = TryFindMaximumStaticHorizontalShoulder(
                        cameraOrientation,
                        lens,
                        out validatedStaticTuple,
                        out validatedStaticCandidate,
                        out ConstraintFailure shoulderFailure);
                    limitingFailure = ConstraintFailure.Prefer(
                        limitingFailure,
                        shoulderFailure);
                    if (foundStaticSolution)
                    {
                        emergencyShoulderLatched = true;
                        emergencyHorizontalShoulderFactor =
                            validatedStaticTuple.ShoulderScale;
                        contactShoulderFactor =
                            emergencyHorizontalShoulderFactor;
                    }
                }

                if (!foundStaticSolution)
                {
                    ApplyFailClosedPreviousPose(
                        ref state,
                        cameraOrientation,
                        lens,
                        limitingFailure);
                    return;
                }

                safeStaticBoom = validatedStaticTuple.BoomDistance;
                Collider observedBlocker = requestedStaticFailure.Blocker ??
                    limitingFailure.Blocker;
                if (observedBlocker != null)
                {
                    latchedHardBlocker = observedBlocker;
                }
            }
            else if (staticContactLatch.Phase !=
                     ShowcaseStaticCameraContactPhase.Clear)
            {
                bool clearWithMargin = IsRequestedStaticPoseClearWithMargin(
                    cameraOrientation,
                    lens,
                    out Collider marginBlocker);
                requestedBlockedForLatch = !clearWithMargin;
                if (marginBlocker != null)
                {
                    latchedHardBlocker = marginBlocker;
                }
            }

            int previousEpisode = staticContactLatch.ContactEpisodeId;
            if (advanceContactState)
            {
                staticContactLatch.Advance(
                    requestedBlockedForLatch,
                    safeStaticBoom,
                    requestedBoomDistance,
                    safeDeltaTime,
                    StaticReleaseEvidenceDuration,
                    Mathf.Max(0.01f, frameInput.CollisionReturnDuration));
                lastContactAdvanceFrame = Time.frameCount;
            }

            if (staticContactLatch.ContactEpisodeId != previousEpisode)
            {
                cornerContactEpisodeId = staticContactLatch.ContactEpisodeId;
                cornerContactDecisionLatched = false;
                cornerContactShoulderFactor = cornerShoulderIntentFactor;
                if (emergencyShoulderLatched)
                {
                    emergencyShoulderEpisodeId =
                        staticContactLatch.ContactEpisodeId;
                }
            }

            if (staticContactLatch.Phase ==
                ShowcaseStaticCameraContactPhase.Clear)
            {
                emergencyShoulderLatched = false;
                emergencyHorizontalShoulderFactor = 1f;
                emergencyShoulderEpisodeId = 0;
                cornerContactEpisodeId = 0;
                cornerContactDecisionLatched = false;
                latchedHardBlocker = null;
            }

            float resolvedShoulderFactor = 1f;
            if (emergencyShoulderLatched)
            {
                resolvedShoulderFactor = staticContactLatch.Phase ==
                        ShowcaseStaticCameraContactPhase.Restoring
                    ? Mathf.Lerp(
                        emergencyHorizontalShoulderFactor,
                        1f,
                        staticContactLatch.RestoreProgress)
                    : emergencyHorizontalShoulderFactor;
            }

            SpatialTuple staticResolvedTuple = new SpatialTuple(
                1f,
                resolvedShoulderFactor,
                staticContactLatch.CurrentBoom);
            ConstraintFailure finalFailure;
            bool requestedSubjectSafe = IsSubjectTupleSafe(
                staticResolvedTuple,
                out finalFailure);
            float safeSubjectBoom = staticResolvedTuple.BoomDistance;
            if (!requestedSubjectSafe)
            {
                if (!TrySolveMaximumSubjectSafeBoom(
                    staticResolvedTuple.BoomDistance,
                    resolvedShoulderFactor,
                    out SpatialTuple subjectSafeTuple,
                    out _,
                    out ConstraintFailure subjectFailure))
                {
                    finalFailure = ConstraintFailure.Prefer(
                        finalFailure,
                        subjectFailure);
                    ApplyFailClosedPreviousPose(
                        ref state,
                        cameraOrientation,
                        lens,
                        finalFailure);
                    return;
                }

                safeSubjectBoom = subjectSafeTuple.BoomDistance;
                finalFailure = ConstraintFailure.Prefer(finalFailure, subjectFailure);
                if (finalFailure.ActorRoot != null)
                {
                    latchedSubjectActorProxy = finalFailure.ActorRoot;
                }
            }

            if (advanceSubjectState)
            {
                subjectContactLatch.Advance(
                    !requestedSubjectSafe,
                    safeSubjectBoom,
                    staticResolvedTuple.BoomDistance,
                    safeDeltaTime,
                    SubjectReleaseEvidenceDuration,
                    Mathf.Max(0.01f, frameInput.CollisionReturnDuration));
                lastSubjectAdvanceFrame = Time.frameCount;
            }

            if (subjectContactLatch.Phase ==
                ShowcaseStaticCameraContactPhase.Clear)
            {
                latchedSubjectActorProxy = null;
            }

            SpatialTuple outputTuple = new SpatialTuple(
                1f,
                resolvedShoulderFactor,
                Mathf.Min(
                    staticResolvedTuple.BoomDistance,
                    subjectContactLatch.CurrentBoom));
            CameraCandidate finalCandidate = BuildAnalyticCandidate(outputTuple);
            bool finalSafe = IsSubjectTupleSafe(outputTuple, out ConstraintFailure outputFailure);
            if (!finalSafe)
            {
                if (!TrySolveMaximumSubjectSafeBoom(
                        outputTuple.BoomDistance,
                        resolvedShoulderFactor,
                        out SpatialTuple emergencySubjectTuple,
                        out finalCandidate,
                        out ConstraintFailure emergencySubjectFailure))
                {
                    finalFailure = ConstraintFailure.Prefer(
                        finalFailure,
                        ConstraintFailure.Prefer(
                            outputFailure,
                            emergencySubjectFailure));
                    ApplyFailClosedPreviousPose(
                        ref state,
                        cameraOrientation,
                        lens,
                        finalFailure);
                    return;
                }

                outputTuple = emergencySubjectTuple;
                if (advanceSubjectState)
                {
                    subjectContactLatch.Advance(
                        true,
                        emergencySubjectTuple.BoomDistance,
                        staticResolvedTuple.BoomDistance,
                        0f,
                        SubjectReleaseEvidenceDuration,
                        Mathf.Max(0.01f, frameInput.CollisionReturnDuration));
                }
                finalFailure = ConstraintFailure.Prefer(
                    finalFailure,
                    ConstraintFailure.Prefer(outputFailure, emergencySubjectFailure));
                if (finalFailure.ActorRoot != null)
                {
                    latchedSubjectActorProxy = finalFailure.ActorRoot;
                }
            }

            bool subjectCompressionActive = subjectContactLatch.Phase !=
                    ShowcaseStaticCameraContactPhase.Clear ||
                outputTuple.BoomDistance <
                    staticResolvedTuple.BoomDistance - BoomTolerance;
            SubjectConstraintApplied = subjectCompressionActive;

            SpatialTuple previousResolved = resolvedTuple;
            resolvedTuple = outputTuple;
            safeTargetTuple = validatedStaticTuple;
            hasResolvedTuple = true;
            constraintRecoveryRequired = staticContactLatch.Phase !=
                ShowcaseStaticCameraContactPhase.Clear;
            hardConstraintActiveThisFrame = constraintRecoveryRequired;
            currentHardBlocker = hardConstraintActiveThisFrame
                ? latchedHardBlocker
                : null;
            currentActorProxy = subjectCompressionActive
                ? latchedSubjectActorProxy
                : null;
            activeGeometrySignature = limitingFailure.GeometrySignature;
            activeSolutionFamily = resolvedShoulderFactor < 1f - ScalarTolerance
                ? ShowcaseCameraSolutionFamily.EmergencyCentered
                : staticContactLatch.CurrentBoom <
                    requestedBoomDistance - BoomTolerance
                    ? ShowcaseCameraSolutionFamily.BoomCompressed
                    : ShowcaseCameraSolutionFamily.Requested;
            RecordTupleCommit(previousResolved, resolvedTuple);
            fastPathFrameCount += activeSolutionFamily ==
                ShowcaseCameraSolutionFamily.Requested ? 1 : 0;

            SafeBoomFraction = requestedBoomDistance <= 0.0001f
                ? 1f
                : Mathf.Clamp01(
                    resolvedTuple.BoomDistance / requestedBoomDistance);
            NoValidPoseReason = ShowcaseCameraNoValidPoseReason.None;
            ShowcaseCameraConstraintStatus targetStatus = subjectCompressionActive
                ? ShowcaseCameraConstraintStatus.SolvedActorCompression
                : hardConstraintActiveThisFrame
                    ? ShowcaseCameraConstraintStatus.SolvedStaticCompression
                    : ShowcaseCameraConstraintStatus.SolvedOpenSpace;
            ApplySolvedPosition(
                ref state,
                finalCandidate.Position,
                cameraOrientation,
                targetStatus);
        }

        private bool TrySolveMaximumSubjectSafeBoom(
            float maximumBoom,
            float horizontalShoulderFactor,
            out SpatialTuple solved,
            out CameraCandidate candidate,
            out ConstraintFailure failure)
        {
            maximumBoom = Mathf.Clamp(
                maximumBoom,
                MinimumBoomDistance,
                requestedBoomDistance);
            float range = Mathf.Max(0f, maximumBoom - MinimumBoomDistance);
            failure = default;
            for (int bracket = 0; bracket <= BoomSearchSteps; bracket++)
            {
                float distance = maximumBoom -
                    range * bracket / Mathf.Max(1, BoomSearchSteps);
                SpatialTuple probe = new SpatialTuple(
                    1f,
                    horizontalShoulderFactor,
                    distance);
                if (!IsSubjectTupleSafe(probe, out ConstraintFailure subjectFailure))
                {
                    failure = ConstraintFailure.Prefer(failure, subjectFailure);
                    continue;
                }

                solved = probe;
                candidate = BuildAnalyticCandidate(probe);
                return true;
            }

            solved = default;
            candidate = default;
            return false;
        }

        private CameraCandidate BuildAnalyticCandidate(SpatialTuple tuple)
        {
            CameraProfileValues profile = ComposeProfile(tuple.GameplayWeight);
            GetRigPoints(
                tuple.GameplayWeight,
                tuple.ShoulderScale,
                out Vector3 root,
                out Vector3 shoulder,
                out Vector3 hand,
                out Vector3 boomDirection);
            float distance = Mathf.Clamp(
                tuple.BoomDistance,
                MinimumBoomDistance,
                Mathf.Max(MinimumBoomDistance, profile.Distance));
            return new CameraCandidate
            {
                Root = root,
                Shoulder = shoulder,
                Hand = hand,
                Position = hand + boomDirection * distance,
                Distance = distance,
                ShoulderScale = tuple.ShoulderScale,
                GameplayWeight = tuple.GameplayWeight,
                Blocker = null,
                WasBoomCompressed = distance < profile.Distance - BoomTolerance
            };
        }

        private bool IsSubjectTupleSafe(
            SpatialTuple tuple,
            out ConstraintFailure failure)
        {
            GetRigPoints(
                1f,
                tuple.ShoulderScale,
                out _,
                out _,
                out Vector3 hand,
                out Vector3 boomDirection);
            Vector3 position = hand + boomDirection * tuple.BoomDistance;
            if (!IsOutsidePlayerEnvelope(position))
            {
                failure = new ConstraintFailure(
                    ShowcaseCameraNoValidPoseReason.PlayerEnvelope,
                    null,
                    null);
                return false;
            }
            if (!IsOutsideCarryEnvelope(position))
            {
                failure = new ConstraintFailure(
                    ShowcaseCameraNoValidPoseReason.CarryEnvelope,
                    null,
                    null);
                return false;
            }
            if (TryFindBlockingActor(position, out ActorProxy actor, out _))
            {
                failure = new ConstraintFailure(
                    ShowcaseCameraNoValidPoseReason.DynamicActor,
                    null,
                    actor.Root);
                return false;
            }

            failure = default;
            return true;
        }

        private void ApplyFailClosedPreviousPose(
            ref CameraState state,
            Quaternion orientation,
            LensSettings lens,
            ConstraintFailure failure)
        {
            ShowcaseCameraNoValidPoseReason originalReason = failure.Reason ==
                    ShowcaseCameraNoValidPoseReason.None
                ? ShowcaseCameraNoValidPoseReason.MultipleConstraints
                : failure.Reason;
            Collider originalBlocker = failure.Blocker ?? latchedHardBlocker;
            Transform originalActor = failure.ActorRoot ?? latchedSubjectActorProxy;
            bool budgetWasExceeded = queryBudgetExceededThisFrame ||
                failure.QuerySaturated;
            bool staticFailure = originalBlocker != null ||
                failure.QuerySaturated ||
                originalReason == ShowcaseCameraNoValidPoseReason.StaticWorld ||
                originalReason == ShowcaseCameraNoValidPoseReason.NearPlane ||
                originalReason == ShowcaseCameraNoValidPoseReason.MultipleConstraints;

            // The ordinary solver may have consumed its entire per-frame query
            // budget.  Safety validation must still execute: otherwise the
            // extension silently leaves Cinemachine's unconstrained position in
            // CameraState.  The emergency reserve is separately observable and
            // can never be consumed by the normal solve.
            bool queryBudgetExceededDuringFailClosed = false;
            failClosedSafetyPass = true;
            queryBudgetExceededThisFrame = false;
            try
            {
                if (TryUseRevalidatedPreviousPose(
                        orientation,
                        lens,
                        out Vector3 previous))
                {
                    ApplyFailClosedDiagnostics(
                        originalReason,
                        originalBlocker,
                        originalActor,
                        staticFailure,
                        failure.GeometrySignature);
                    ApplySolvedPosition(
                        ref state,
                        previous,
                        orientation,
                        ShowcaseCameraConstraintStatus.PreviousPoseRevalidated);
                    return;
                }

                float preferredShoulder = hasResolvedTuple
                    ? resolvedTuple.ShoulderScale
                    : emergencyShoulderLatched
                        ? emergencyHorizontalShoulderFactor
                        : 1f;
                if (TrySolveMaximumSafeBoom(
                        1f,
                        preferredShoulder,
                        orientation,
                        lens,
                        out SpatialTuple safeTuple,
                        out CameraCandidate safeCandidate,
                        out ConstraintFailure fallbackFailure))
                {
                    CommitFailClosedFallback(
                        ref state,
                        orientation,
                        safeTuple,
                        safeCandidate,
                        ConstraintFailure.Prefer(failure, fallbackFailure),
                        originalReason,
                        originalBlocker,
                        originalActor,
                        staticFailure,
                        fullySafe: true);
                    return;
                }

                ConstraintFailure combinedFailure = ConstraintFailure.Prefer(
                    failure,
                    fallbackFailure);

                if (!staticFailure)
                {
                    float subjectSearchMaximum = Mathf.Min(
                        requestedBoomDistance,
                        staticContactLatch.CurrentBoom);
                    bool foundSubjectSafeTuple = TrySolveMaximumSubjectSafeBoom(
                            subjectSearchMaximum,
                            preferredShoulder,
                            out SpatialTuple subjectSafeTuple,
                            out _,
                            out ConstraintFailure subjectSearchFailure);
                    ConstraintFailure subjectStaticFailure = default;
                    if (foundSubjectSafeTuple && EvaluateStaticTuple(
                            subjectSafeTuple,
                            orientation,
                            lens,
                            out CameraCandidate subjectSafeCandidate,
                            out subjectStaticFailure))
                    {
                        CommitFailClosedFallback(
                            ref state,
                            orientation,
                            subjectSafeTuple,
                            subjectSafeCandidate,
                            ConstraintFailure.Prefer(
                                combinedFailure,
                                subjectSearchFailure),
                            originalReason,
                            originalBlocker,
                            originalActor,
                            staticFailure: false,
                            fullySafe: true);
                        return;
                    }

                    combinedFailure = ConstraintFailure.Prefer(
                        combinedFailure,
                        subjectSearchFailure);
                    combinedFailure = ConstraintFailure.Prefer(
                        combinedFailure,
                        subjectStaticFailure);
                    SpatialTuple subjectNoSolutionTuple = new SpatialTuple(
                        1f,
                        preferredShoulder,
                        Mathf.Min(
                            requestedBoomDistance,
                            staticContactLatch.CurrentBoom));
                    if (EvaluateStaticTuple(
                            subjectNoSolutionTuple,
                            orientation,
                            lens,
                            out CameraCandidate subjectNoSolutionCandidate,
                            out ConstraintFailure newlyObservedStaticFailure))
                    {
                        // There is no subject-safe boom at the current shoulder,
                        // but this pose is still proven non-penetrating.  Keep
                        // the horizontal shoulder unchanged and report the
                        // subject no-solution instead of inventing a static-wall
                        // emergency shoulder transition.
                        CommitFailClosedFallback(
                            ref state,
                            orientation,
                            subjectNoSolutionTuple,
                            subjectNoSolutionCandidate,
                            combinedFailure,
                            originalReason,
                            originalBlocker,
                            originalActor,
                            staticFailure: false,
                            fullySafe: false);
                        return;
                    }

                    combinedFailure = ConstraintFailure.Prefer(
                        combinedFailure,
                        newlyObservedStaticFailure);
                    staticFailure = true;
                }

                // If the current horizontal shoulder has no solution, a
                // centered solver candidate is the final static-world fallback.
                // It is still built from StableAnchor/profile authority; the raw
                // Cinemachine pose is never adopted.
                if (TrySolveMaximumStaticBoom(
                        0f,
                        orientation,
                        lens,
                        out SpatialTuple staticTuple,
                        out CameraCandidate staticCandidate,
                        out ConstraintFailure staticFallbackFailure))
                {
                    combinedFailure = ConstraintFailure.Prefer(
                        combinedFailure,
                        staticFallbackFailure);
                    bool subjectSafe = IsSubjectTupleSafe(
                        staticTuple,
                        out ConstraintFailure subjectFailure);
                    combinedFailure = ConstraintFailure.Prefer(
                        combinedFailure,
                        subjectFailure);

                    CommitFailClosedFallback(
                        ref state,
                        orientation,
                        staticTuple,
                        staticCandidate,
                        combinedFailure,
                        subjectSafe ? originalReason : subjectFailure.Reason,
                        originalBlocker,
                        subjectSafe ? originalActor : subjectFailure.ActorRoot,
                        staticFailure: true,
                        fullySafe: subjectSafe);
                    return;
                }

                combinedFailure = ConstraintFailure.Prefer(
                    combinedFailure,
                    staticFallbackFailure);

                // A physically impossible layout (or exhausted emergency
                // reserve) has no provably valid output.  Still overwrite the
                // CameraState with a deterministic StableAnchor-derived pose so
                // an unconstrained Cinemachine position can never escape.  The
                // NoValidPose diagnostics deliberately do not claim safety.
                SpatialTuple deterministicTuple = new SpatialTuple(
                    1f,
                    0f,
                    MinimumBoomDistance);
                CameraCandidate deterministicCandidate =
                    BuildAnalyticCandidate(deterministicTuple);
                CommitFailClosedFallback(
                    ref state,
                    orientation,
                    deterministicTuple,
                    deterministicCandidate,
                    combinedFailure,
                    combinedFailure.Reason == ShowcaseCameraNoValidPoseReason.None
                        ? ShowcaseCameraNoValidPoseReason.MultipleConstraints
                        : combinedFailure.Reason,
                    combinedFailure.Blocker ?? originalBlocker,
                    combinedFailure.ActorRoot ?? originalActor,
                    staticFailure: true,
                    fullySafe: false);
            }
            finally
            {
                queryBudgetExceededDuringFailClosed = queryBudgetExceededThisFrame;
                failClosedSafetyPass = false;
                queryBudgetExceededThisFrame = budgetWasExceeded ||
                    queryBudgetExceededDuringFailClosed;
            }
        }

        private void CommitFailClosedFallback(
            ref CameraState state,
            Quaternion orientation,
            SpatialTuple tuple,
            CameraCandidate candidate,
            ConstraintFailure failure,
            ShowcaseCameraNoValidPoseReason diagnosticReason,
            Collider diagnosticBlocker,
            Transform diagnosticActor,
            bool staticFailure,
            bool fullySafe)
        {
            SpatialTuple previousResolved = resolvedTuple;
            resolvedTuple = tuple;
            safeTargetTuple = tuple;
            hasResolvedTuple = true;
            constraintRecoveryRequired = staticFailure ||
                tuple.BoomDistance < requestedBoomDistance - BoomTolerance ||
                tuple.ShoulderScale < 1f - ScalarTolerance;
            activeSolutionFamily = tuple.ShoulderScale < 1f - ScalarTolerance
                ? ShowcaseCameraSolutionFamily.EmergencyCentered
                : tuple.BoomDistance < requestedBoomDistance - BoomTolerance
                    ? ShowcaseCameraSolutionFamily.BoomCompressed
                    : ShowcaseCameraSolutionFamily.Requested;
            activeGeometrySignature = failure.GeometrySignature;
            SafeBoomFraction = requestedBoomDistance <= 0.0001f
                ? 1f
                : Mathf.Clamp01(tuple.BoomDistance / requestedBoomDistance);
            RecordTupleCommit(previousResolved, tuple);

            if (staticFailure)
            {
                int previousEpisode = staticContactLatch.ContactEpisodeId;
                staticContactLatch.Advance(
                    true,
                    tuple.BoomDistance,
                    requestedBoomDistance,
                    0f,
                    StaticReleaseEvidenceDuration,
                    Mathf.Max(0.01f, frameInput.CollisionReturnDuration));
                lastContactAdvanceFrame = Time.frameCount;
                contactLatchInitialized = true;
                if (staticContactLatch.ContactEpisodeId != previousEpisode)
                {
                    cornerContactEpisodeId = staticContactLatch.ContactEpisodeId;
                    cornerContactDecisionLatched = false;
                    cornerContactShoulderFactor = cornerShoulderIntentFactor;
                }
                if (tuple.ShoulderScale < 1f - ScalarTolerance)
                {
                    emergencyShoulderLatched = true;
                    emergencyHorizontalShoulderFactor = tuple.ShoulderScale;
                    emergencyShoulderEpisodeId =
                        staticContactLatch.ContactEpisodeId;
                }
            }

            ApplyFailClosedDiagnostics(
                diagnosticReason,
                diagnosticBlocker,
                diagnosticActor,
                staticFailure,
                failure.GeometrySignature);
            SubjectConstraintApplied = !fullySafe &&
                (diagnosticReason == ShowcaseCameraNoValidPoseReason.PlayerEnvelope ||
                 diagnosticReason == ShowcaseCameraNoValidPoseReason.CarryEnvelope ||
                 diagnosticReason == ShowcaseCameraNoValidPoseReason.DynamicActor ||
                 diagnosticActor != null);

            ShowcaseCameraConstraintStatus fallbackStatus;
            if (!fullySafe)
            {
                fallbackStatus = diagnosticReason ==
                        ShowcaseCameraNoValidPoseReason.DynamicActor
                    ? ShowcaseCameraConstraintStatus.DynamicActorNoSolution
                    : ShowcaseCameraConstraintStatus.NoValidPose;
            }
            else if (staticFailure)
            {
                fallbackStatus = ShowcaseCameraConstraintStatus.SolvedStaticCompression;
            }
            else
            {
                fallbackStatus = ShowcaseCameraConstraintStatus.SolvedActorCompression;
            }

            ApplySolvedPosition(
                ref state,
                candidate.Position,
                orientation,
                fallbackStatus);
        }

        private void ApplyFailClosedDiagnostics(
            ShowcaseCameraNoValidPoseReason reason,
            Collider blocker,
            Transform actor,
            bool staticFailure,
            GeometrySignature geometrySignature)
        {
            NoValidPoseReason = reason == ShowcaseCameraNoValidPoseReason.None
                ? ShowcaseCameraNoValidPoseReason.MultipleConstraints
                : reason;
            currentHardBlocker = blocker;
            if (blocker != null)
            {
                latchedHardBlocker = blocker;
            }
            currentActorProxy = actor;
            if (actor != null)
            {
                latchedSubjectActorProxy = actor;
            }
            hardConstraintActiveThisFrame = staticFailure;
            activeGeometrySignature = geometrySignature;
        }

        private void ApplySolvedPosition(
            ref CameraState state,
            Vector3 solvedPosition,
            Quaternion orientation,
            ShowcaseCameraConstraintStatus status)
        {
            Vector3 original = state.GetFinalPosition();
            state.PositionCorrection += solvedPosition - original;
            lastSolvedPose = new Pose(solvedPosition, orientation);
            hasLastSolvedPose = true;
            finalStableAnchorLocalCameraOffset = frameInput.StableAnchor != null
                ? frameInput.StableAnchor.InverseTransformPoint(solvedPosition)
                : Vector3.zero;
            if (softOcclusionPresenter != null && frameInput.StableAnchor != null)
            {
                softOcclusionPresenter.Resolve(
                    frameInput.StableAnchor.position,
                    solvedPosition);
                activeSoftOccluderCount =
                    softOcclusionPresenter.ActiveOccluderCount;
            }
            Status = status;
        }

        private bool TryRunFullSolve(
            SpatialTuple requested,
            Quaternion orientation,
            LensSettings lens,
            out SpatialTuple solved,
            out CameraCandidate candidate,
            out ConstraintFailure failure)
        {
            if (fullSolvesThisFrame >= 1)
            {
                solved = default;
                candidate = default;
                failure = new ConstraintFailure(
                    ShowcaseCameraNoValidPoseReason.MultipleConstraints,
                    null,
                    null,
                    default,
                    true);
                return false;
            }

            fullSolvesThisFrame++;
            using (FullSolveMarker.Auto())
            {
                if (SolveSafeTuple(
                    requested,
                    orientation,
                    lens,
                    out solved,
                    out candidate,
                    out failure))
                {
                    return true;
                }

                bool hasEmergency = TrySolveMaximumSafeBoom(
                    0f,
                    0f,
                    orientation,
                    lens,
                    out SpatialTuple emergencyTuple,
                    out CameraCandidate emergencyCandidate,
                    out ConstraintFailure emergencyFailure);
                if (hasEmergency)
                {
                    solved = emergencyTuple;
                    candidate = emergencyCandidate;
                    failure = ConstraintFailure.Prefer(
                        failure,
                        emergencyFailure);
                    return true;
                }
                return false;
            }
        }

        private void RecordTupleCommit(SpatialTuple previous, SpatialTuple next)
        {
            if (!hasResolvedTuple || !SpatialTuple.Approximately(previous, next))
            {
                tupleCommitsThisFrame++;
            }
        }

        private int BuildSemanticStateHash(LensSettings lens)
        {
            ShowcaseContextualCornerFrameSnapshot snapshot =
                frameInput.CornerSnapshot;
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + snapshot.ResetRevision;
                hash = hash * 31 + (snapshot.IsAim ? 1 : 0);
                hash = hash * 31 +
                    (snapshot.IsDirectCarryThrowWindup ? 1 : 0);
                hash = hash * 31 + (int)snapshot.CarryMode;
                hash = hash * 31 + (int)snapshot.ThrowPhase;
                hash = hash * 31 + snapshot.CarryTargetRuntimeId;
                hash = hash * 31 +
                    (frameInput.CarriedTarget != null
                        ? frameInput.CarriedTarget.GetInstanceID()
                        : 0);
                hash = hash * 31 + Mathf.RoundToInt(lens.NearClipPlane * 1000f);
                return hash;
            }
        }

        private void ResolveContextualCornerIntent(
            Quaternion orientation,
            LensSettings lens,
            float deltaTime)
        {
            if (staticContactLatch.Phase ==
                    ShowcaseStaticCameraContactPhase.Contact &&
                cornerContactEpisodeId == staticContactLatch.ContactEpisodeId &&
                cornerContactEpisodeId != 0 &&
                cornerContactDecisionLatched)
            {
                // After one non-default side decision completes, the contact
                // episode owns it until full clear. Collider seams and plane
                // changes may update diagnostics, but cannot restart arbitration.
                cornerShoulderIntentFactor = cornerContactShoulderFactor;
                return;
            }

            cornerProbeElapsed += Mathf.Max(0f, deltaTime);
            ShowcaseContextualCornerFrameSnapshot snapshot =
                BuildCurrentCornerSnapshot(orientation, lens);

            if (!snapshot.IsEligible)
            {
                nextCornerProbeMode = ShowcaseContextualCornerMode.RightDefault;
                cachedRightDefaultCandidate = CreateUnavailableCornerCandidate(
                    ShowcaseContextualCornerMode.RightDefault);
                cachedRightLeanCandidate = CreateUnavailableCornerCandidate(
                    ShowcaseContextualCornerMode.RightLean);
                cachedTemporaryLeftCandidate = CreateUnavailableCornerCandidate(
                    ShowcaseContextualCornerMode.TemporaryLeft);
                cachedSubjectForwardSightlineClearance =
                    CornerSightlineProbeDistance;
                cornerProbeElapsed = 0f;
            }
            else if (cornerProbeElapsed >= CornerProbeInterval &&
                     !queryBudgetExceededThisFrame)
            {
                cornerProbeElapsed = 0f;
                using (CornerProbeMarker.Auto())
                {
                    ProbeNextContextualCornerCandidate(orientation, lens);
                }
                snapshot = BuildCurrentCornerSnapshot(orientation, lens);
            }

            ShowcaseContextualCornerSelection selection = cornerResolver.Evaluate(
                snapshot,
                cachedRightDefaultCandidate,
                cachedRightLeanCandidate,
                cachedTemporaryLeftCandidate);
            cornerShoulderIntentFactor = selection.ShoulderIntentFactor;
            if (staticContactLatch.Phase ==
                    ShowcaseStaticCameraContactPhase.Contact &&
                cornerContactEpisodeId == staticContactLatch.ContactEpisodeId &&
                cornerContactEpisodeId != 0 &&
                !cornerResolver.IsTransitioning &&
                selection.Mode != ShowcaseContextualCornerMode.RightDefault)
            {
                cornerContactDecisionLatched = true;
                cornerContactShoulderFactor = selection.ShoulderIntentFactor;
            }
        }

        private void ProbeNextContextualCornerCandidate(
            Quaternion orientation,
            LensSettings lens)
        {
            cachedSubjectForwardSightlineClearance =
                MeasureSubjectForwardSightlineClearance(orientation);
            if (queryBudgetExceededThisFrame)
            {
                return;
            }

            ShowcaseContextualCornerMode probedMode = nextCornerProbeMode;
            ShowcaseContextualCornerCandidate candidate =
                EvaluateContextualCornerCandidate(
                    probedMode,
                    GetCornerShoulderFactor(probedMode),
                    orientation,
                    lens);
            cornerCandidateEvaluationsThisFrame++;

            switch (probedMode)
            {
                case ShowcaseContextualCornerMode.RightLean:
                    cachedRightLeanCandidate = candidate;
                    nextCornerProbeMode = candidate.IsSafeIntent
                        ? ShowcaseContextualCornerMode.RightDefault
                        : ShowcaseContextualCornerMode.TemporaryLeft;
                    break;
                case ShowcaseContextualCornerMode.TemporaryLeft:
                    cachedTemporaryLeftCandidate = candidate;
                    nextCornerProbeMode = cornerResolver.CurrentMode ==
                            ShowcaseContextualCornerMode.TemporaryLeft
                        ? ShowcaseContextualCornerMode.RightLean
                        : ShowcaseContextualCornerMode.RightDefault;
                    break;
                default:
                    cachedRightDefaultCandidate = candidate;
                    nextCornerProbeMode = candidate.HasStaticWorldConstraint
                        ? ShowcaseContextualCornerMode.RightLean
                        : ShowcaseContextualCornerMode.RightDefault;
                    break;
            }
        }

        private ShowcaseContextualCornerFrameSnapshot BuildCurrentCornerSnapshot(
            Quaternion orientation,
            LensSettings lens)
        {
            ShowcaseContextualCornerFrameSnapshot source = frameInput.CornerSnapshot;
            bool restoringStaticBoom = staticContactLatch.Phase ==
                ShowcaseStaticCameraContactPhase.Restoring;
            return new ShowcaseContextualCornerFrameSnapshot(
                source.FrameId,
                source.DeltaTime,
                source.ResetRevision,
                source.LogicalYaw,
                source.LogicalPitch,
                source.IsAim && !restoringStaticBoom,
                source.IsDirectCarryThrowWindup && !restoringStaticBoom,
                source.CarryMode,
                source.ThrowPhase,
                source.CarryTargetRuntimeId,
                source.PlayerRootPose,
                source.OrbitTargetPose,
                source.BaseTargetHeight,
                source.BaseDistance,
                source.BaseShoulderOffset,
                source.BaseShoulderHeight,
                source.BaseVerticalArmLength,
                lens.FieldOfView,
                lens.Aspect > 0f ? lens.Aspect : source.Aspect,
                lens.NearClipPlane,
                cornerResolver.CurrentMode,
                cornerResolver.CurrentBlendProgress,
                source.ActiveBlockerId,
                cachedSubjectForwardSightlineClearance);
        }

        private ShowcaseContextualCornerCandidate EvaluateContextualCornerCandidate(
            ShowcaseContextualCornerMode mode,
            float shoulderIntentFactor,
            Quaternion orientation,
            LensSettings lens)
        {
            float savedFactor = cornerShoulderIntentFactor;
            ShowcaseCameraNoValidPoseReason savedReason = NoValidPoseReason;
            bool savedSubjectConstraint = SubjectConstraintApplied;
            cornerShoulderIntentFactor = Mathf.Clamp(shoulderIntentFactor, -1f, 1f);

            SpatialTuple requested = BuildRequestedTuple();
            bool solved = EvaluateTuple(
                requested,
                orientation,
                lens,
                out CameraCandidate candidate,
                out ConstraintFailure failure);
            float requestedBoom = requested.BoomDistance;
            float centerClearance = solved
                ? MeasureCenterSightlineClearance(candidate.Position, orientation, lens)
                : 0f;
            bool staticWorldLimited = failure.Blocker != null ||
                failure.Reason == ShowcaseCameraNoValidPoseReason.StaticWorld ||
                failure.Reason == ShowcaseCameraNoValidPoseReason.NearPlane;
            int blockerId = failure.Blocker != null
                ? failure.Blocker.GetInstanceID()
                : 0;
            bool nearPlaneSafe = solved &&
                failure.Reason != ShowcaseCameraNoValidPoseReason.NearPlane;
            bool subjectSafe = solved &&
                failure.Reason != ShowcaseCameraNoValidPoseReason.PlayerEnvelope &&
                failure.Reason != ShowcaseCameraNoValidPoseReason.CarryEnvelope &&
                failure.Reason != ShowcaseCameraNoValidPoseReason.DynamicActor;

            cornerShoulderIntentFactor = savedFactor;
            NoValidPoseReason = savedReason;
            SubjectConstraintApplied = savedSubjectConstraint;
            return new ShowcaseContextualCornerCandidate(
                mode,
                shoulderIntentFactor,
                solved,
                solved,
                staticWorldLimited,
                nearPlaneSafe,
                subjectSafe,
                requestedBoom,
                solved ? requestedBoom : 0f,
                solved ? 1f : 0f,
                centerClearance,
                blockerId,
                failure.GeometrySignature.Value);
        }

        private static ShowcaseContextualCornerCandidate
            CreateUnavailableCornerCandidate(ShowcaseContextualCornerMode mode)
        {
            return new ShowcaseContextualCornerCandidate(
                mode,
                GetCornerShoulderFactor(mode),
                false,
                false,
                false,
                false,
                false,
                0f,
                0f,
                0f,
                0f,
                0,
                0UL);
        }

        private static float GetCornerShoulderFactor(
            ShowcaseContextualCornerMode mode)
        {
            return mode switch
            {
                ShowcaseContextualCornerMode.RightLean =>
                    ShowcaseContextualCornerAimResolver.RightLeanShoulderFactor,
                ShowcaseContextualCornerMode.TemporaryLeft =>
                    ShowcaseContextualCornerAimResolver.TemporaryLeftShoulderFactor,
                _ => ShowcaseContextualCornerAimResolver.RightDefaultShoulderFactor
            };
        }

        private float MeasureSubjectForwardSightlineClearance(Quaternion orientation)
        {
            Vector3 forward = orientation * Vector3.forward;
            float subjectClearance = 0.5f;
            if (frameInput.PlayerController != null)
            {
                subjectClearance = Mathf.Max(
                    subjectClearance,
                    frameInput.PlayerController.radius +
                    frameInput.PlayerController.skinWidth +
                    frameInput.ClearanceMargin);
            }
            Vector3 origin = frameInput.CornerSnapshot.PlayerRootPose.position +
                Vector3.up * frameInput.CoreTargetHeight +
                forward * subjectClearance;
            Vector3 end = origin + forward * CornerSightlineProbeDistance;
            return TryFindHardBlocker(
                    origin,
                    end,
                    CornerSightlineProbeRadius,
                    out RaycastHit hit,
                    out _)
                ? hit.distance
                : CornerSightlineProbeDistance;
        }

        private float MeasureCenterSightlineClearance(
            Vector3 cameraPosition,
            Quaternion orientation,
            LensSettings lens)
        {
            Vector3 forward = orientation * Vector3.forward;
            Vector3 origin = cameraPosition +
                forward * Mathf.Max(0.01f, lens.NearClipPlane);
            Vector3 end = origin + forward * CornerSightlineProbeDistance;
            return TryFindHardBlocker(
                    origin,
                    end,
                    CornerSightlineProbeRadius,
                    out RaycastHit hit,
                    out _)
                ? hit.distance
                : CornerSightlineProbeDistance;
        }

        private SpatialTuple BuildRequestedTuple()
        {
            const float requestedGameplayWeight = 1f;
            CameraProfileValues profile = ComposeProfile(requestedGameplayWeight);
            return new SpatialTuple(
                requestedGameplayWeight,
                1f,
                Mathf.Max(MinimumBoomDistance, profile.Distance));
        }

        private bool SolveSafeTuple(
            SpatialTuple requested,
            Quaternion orientation,
            LensSettings lens,
            out SpatialTuple solved,
            out CameraCandidate candidate,
            out ConstraintFailure limitingFailure)
        {
            if (!frameInput.HasGameplayOverride)
            {
                if (TrySolveMaximumSafeBoom(
                        requested.GameplayWeight,
                        requested.ShoulderScale,
                        orientation,
                        lens,
                        out solved,
                        out candidate,
                        out limitingFailure))
                {
                    return true;
                }

                ConstraintFailure requestedFailure = limitingFailure;
                if (TryFindMaximumFeasibleShoulderScale(
                        requested.GameplayWeight,
                        orientation,
                        lens,
                        out solved,
                        out candidate,
                        out limitingFailure))
                {
                    return true;
                }

                limitingFailure = ConstraintFailure.Prefer(
                    requestedFailure,
                    limitingFailure);
                return false;
            }

            if (TryFindMaximumFeasibleGameplayWeight(
                    requested.GameplayWeight,
                    1f,
                    orientation,
                    lens,
                    out solved,
                    out candidate,
                    out limitingFailure))
            {
                return true;
            }

            ConstraintFailure fullShoulderFailure = limitingFailure;
            if (!TryFindMaximumFeasibleShoulderScale(
                    0f,
                    orientation,
                    lens,
                    out SpatialTuple shoulderTuple,
                    out CameraCandidate shoulderCandidate,
                    out ConstraintFailure shoulderFailure))
            {
                solved = default;
                candidate = default;
                limitingFailure = ConstraintFailure.Prefer(
                    fullShoulderFailure,
                    shoulderFailure);
                return false;
            }

            if (TryFindMaximumFeasibleGameplayWeight(
                    requested.GameplayWeight,
                    shoulderTuple.ShoulderScale,
                    orientation,
                    lens,
                    out solved,
                    out candidate,
                    out limitingFailure))
            {
                return true;
            }

            solved = shoulderTuple;
            candidate = shoulderCandidate;
            limitingFailure = shoulderFailure;
            return true;
        }

        private bool TryFindMaximumFeasibleGameplayWeight(
            float maximumGameplayWeight,
            float shoulderScale,
            Quaternion orientation,
            LensSettings lens,
            out SpatialTuple solved,
            out CameraCandidate candidate,
            out ConstraintFailure limitingFailure)
        {
            maximumGameplayWeight = Mathf.Clamp01(maximumGameplayWeight);
            float previousProbe = maximumGameplayWeight;
            limitingFailure = default;
            int gameplayBracketCount = maximumGameplayWeight <= ScalarTolerance
                ? 0
                : ScalarBracketSteps;
            for (int bracket = 0; bracket <= gameplayBracketCount; bracket++)
            {
                float probe = bracket == 0
                    ? maximumGameplayWeight
                    : Mathf.Lerp(maximumGameplayWeight, 0f,
                        bracket / (float)gameplayBracketCount);
                if (!TrySolveMaximumSafeBoom(
                        probe,
                        shoulderScale,
                        orientation,
                        lens,
                        out SpatialTuple probeTuple,
                        out CameraCandidate probeCandidate,
                        out ConstraintFailure probeFailure))
                {
                    limitingFailure = ConstraintFailure.Prefer(
                        limitingFailure,
                        probeFailure);
                    if (queryBudgetExceededThisFrame)
                    {
                        break;
                    }
                    previousProbe = probe;
                    continue;
                }

                float low = probe;
                float high = previousProbe;
                solved = probeTuple;
                candidate = probeCandidate;
                for (int iteration = 0;
                     iteration < ScalarSearchIterations && high - low > ScalarTolerance;
                     iteration++)
                {
                    float middle = (low + high) * 0.5f;
                    if (TrySolveMaximumSafeBoom(
                            middle,
                            shoulderScale,
                            orientation,
                            lens,
                            out SpatialTuple middleTuple,
                            out CameraCandidate middleCandidate,
                            out ConstraintFailure middleFailure))
                    {
                        low = middle;
                        solved = middleTuple;
                        candidate = middleCandidate;
                    }
                    else
                    {
                        high = middle;
                        limitingFailure = ConstraintFailure.Prefer(
                            limitingFailure,
                            middleFailure);
                        if (queryBudgetExceededThisFrame)
                        {
                            break;
                        }
                    }
                }
                return true;
            }

            solved = default;
            candidate = default;
            return false;
        }

        private bool TryFindMaximumFeasibleShoulderScale(
            float gameplayWeight,
            Quaternion orientation,
            LensSettings lens,
            out SpatialTuple solved,
            out CameraCandidate candidate,
            out ConstraintFailure limitingFailure)
        {
            limitingFailure = default;
            if (!TrySolveMaximumSafeBoom(
                    gameplayWeight,
                    0f,
                    orientation,
                    lens,
                    out solved,
                    out candidate,
                    out ConstraintFailure centeredFailure))
            {
                limitingFailure = centeredFailure;
                solved = default;
                candidate = default;
                return false;
            }

            float low = 0f;
            float high = 1f;
            for (int iteration = 0;
                 iteration < ScalarSearchIterations &&
                 high - low > ScalarTolerance &&
                 !queryBudgetExceededThisFrame;
                 iteration++)
            {
                float middle = (low + high) * 0.5f;
                if (TrySolveMaximumSafeBoom(
                        gameplayWeight,
                        middle,
                        orientation,
                        lens,
                        out SpatialTuple middleTuple,
                        out CameraCandidate middleCandidate,
                        out ConstraintFailure middleFailure))
                {
                    low = middle;
                    solved = middleTuple;
                    candidate = middleCandidate;
                }
                else
                {
                    high = middle;
                    limitingFailure = ConstraintFailure.Prefer(
                        limitingFailure,
                        middleFailure);
                }
            }
            return true;
        }

        private bool TrySolveMaximumSafeBoom(
            float gameplayWeight,
            float shoulderScale,
            Quaternion orientation,
            LensSettings lens,
            out SpatialTuple solved,
            out CameraCandidate candidate,
            out ConstraintFailure limitingFailure)
        {
            int candidateQueryStart = TotalPhysicsQueriesThisFrame;
            if (!HasCandidateQueryCapacity(candidateQueryStart, 4))
            {
                solved = default;
                candidate = default;
                limitingFailure = new ConstraintFailure(
                    ShowcaseCameraNoValidPoseReason.MultipleConstraints,
                    null,
                    null,
                    default);
                return false;
            }
            if (!IsPivotChainSafe(
                    Mathf.Clamp01(gameplayWeight),
                    Mathf.Clamp01(shoulderScale),
                    out Collider pivotBlocker,
                    out GeometrySignature pivotGeometry))
            {
                solved = default;
                candidate = default;
                limitingFailure = new ConstraintFailure(
                    ShowcaseCameraNoValidPoseReason.StaticWorld,
                    pivotBlocker,
                    null,
                    pivotGeometry);
                return false;
            }

            CameraProfileValues profile = ComposeProfile(gameplayWeight);
            float requestedDistance = Mathf.Max(MinimumBoomDistance, profile.Distance);
            SpatialTuple requested = new SpatialTuple(
                gameplayWeight,
                shoulderScale,
                requestedDistance);
            if (!HasCandidateQueryCapacity(candidateQueryStart, 3))
            {
                solved = default;
                candidate = default;
                limitingFailure = new ConstraintFailure(
                    ShowcaseCameraNoValidPoseReason.MultipleConstraints,
                    null,
                    null,
                    default);
                return false;
            }
            if (EvaluateTupleInternal(
                    requested,
                    orientation,
                    lens,
                    validatePivotChain: false,
                    validateSubjectConstraints: true,
                    out candidate,
                    out limitingFailure))
            {
                solved = requested;
                return true;
            }

            float searchMaximum = requestedDistance;
            if (limitingFailure.Blocker != null &&
                HasCandidateQueryCapacity(candidateQueryStart, 1))
            {
                GetRigPoints(
                    gameplayWeight,
                    shoulderScale,
                    out _,
                    out _,
                    out Vector3 hand,
                    out Vector3 boomDirection);
                if (TryFindHardBlocker(
                        hand,
                        hand + boomDirection * requestedDistance,
                        Mathf.Max(0.01f, frameInput.CameraRadius),
                        HardQueryRole.HandToCamera,
                        out RaycastHit hit,
                        out _,
                        out _))
                {
                    searchMaximum = Mathf.Clamp(
                        hit.distance - Mathf.Max(0.001f, frameInput.ClearanceMargin),
                        MinimumBoomDistance,
                        requestedDistance);
                }
            }

            if (searchMaximum < requestedDistance - BoomTolerance &&
                HasCandidateQueryCapacity(candidateQueryStart, 3))
            {
                SpatialTuple blockerBoundary = new SpatialTuple(
                    gameplayWeight,
                    shoulderScale,
                    searchMaximum);
                if (EvaluateTupleInternal(
                        blockerBoundary,
                        orientation,
                        lens,
                        validatePivotChain: false,
                        validateSubjectConstraints: true,
                        out candidate,
                        out ConstraintFailure boundaryFailure))
                {
                    solved = blockerBoundary;
                    return true;
                }
                limitingFailure = ConstraintFailure.Prefer(
                    limitingFailure,
                    boundaryFailure);
            }

            float previousDistance = searchMaximum;
            float range = Mathf.Max(0f, searchMaximum - MinimumBoomDistance);
            for (int bracket = 1; bracket <= BoomSearchSteps; bracket++)
            {
                if (queryBudgetExceededThisFrame ||
                    !HasCandidateQueryCapacity(candidateQueryStart, 3))
                {
                    break;
                }
                float distance = searchMaximum -
                    range * bracket / BoomSearchSteps;
                SpatialTuple probe = new SpatialTuple(
                    gameplayWeight,
                    shoulderScale,
                    distance);
                if (!EvaluateTupleInternal(
                        probe,
                        orientation,
                        lens,
                        validatePivotChain: false,
                        validateSubjectConstraints: true,
                        out CameraCandidate probeCandidate,
                        out ConstraintFailure probeFailure))
                {
                    limitingFailure = ConstraintFailure.Prefer(
                        limitingFailure,
                        probeFailure);
                    previousDistance = distance;
                    continue;
                }

                float low = distance;
                float high = previousDistance;
                solved = probe;
                candidate = probeCandidate;
                for (int iteration = 0;
                     iteration < ScalarSearchIterations && high - low > BoomTolerance;
                     iteration++)
                {
                    if (!HasCandidateQueryCapacity(candidateQueryStart, 3))
                    {
                        break;
                    }
                    float middle = (low + high) * 0.5f;
                    SpatialTuple middleTuple = new SpatialTuple(
                        gameplayWeight,
                        shoulderScale,
                        middle);
                    if (EvaluateTupleInternal(
                            middleTuple,
                            orientation,
                            lens,
                            validatePivotChain: false,
                            validateSubjectConstraints: true,
                            out CameraCandidate middleCandidate,
                            out ConstraintFailure middleFailure))
                    {
                        low = middle;
                        solved = middleTuple;
                        candidate = middleCandidate;
                    }
                    else
                    {
                        high = middle;
                        limitingFailure = ConstraintFailure.Prefer(
                            limitingFailure,
                            middleFailure);
                        if (queryBudgetExceededThisFrame)
                        {
                            break;
                        }
                    }
                }
                return true;
            }

            solved = default;
            candidate = default;
            return false;
        }

        private bool TrySolveMaximumStaticBoom(
            float horizontalShoulderFactor,
            Quaternion orientation,
            LensSettings lens,
            out SpatialTuple solved,
            out CameraCandidate candidate,
            out ConstraintFailure limitingFailure)
        {
            int candidateQueryStart = TotalPhysicsQueriesThisFrame;
            horizontalShoulderFactor = Mathf.Clamp01(horizontalShoulderFactor);
            if (!HasCandidateQueryCapacity(candidateQueryStart, 4))
            {
                solved = default;
                candidate = default;
                limitingFailure = new ConstraintFailure(
                    ShowcaseCameraNoValidPoseReason.MultipleConstraints,
                    null,
                    null,
                    GeometrySignature.BudgetExceeded,
                    true);
                return false;
            }
            if (!IsPivotChainSafe(
                    1f,
                    horizontalShoulderFactor,
                    out Collider pivotBlocker,
                    out GeometrySignature pivotGeometry))
            {
                solved = default;
                candidate = default;
                limitingFailure = new ConstraintFailure(
                    ShowcaseCameraNoValidPoseReason.StaticWorld,
                    pivotBlocker,
                    null,
                    pivotGeometry,
                    queryBudgetExceededThisFrame);
                return false;
            }

            float requestedDistance = Mathf.Max(
                MinimumBoomDistance,
                ComposeProfile(1f).Distance);
            SpatialTuple requested = new SpatialTuple(
                1f,
                horizontalShoulderFactor,
                requestedDistance);
            if (EvaluateTupleInternal(
                    requested,
                    orientation,
                    lens,
                    validatePivotChain: false,
                    validateSubjectConstraints: false,
                    out candidate,
                    out limitingFailure))
            {
                solved = requested;
                return true;
            }

            GetRigPoints(
                1f,
                horizontalShoulderFactor,
                out _,
                out _,
                out Vector3 hand,
                out Vector3 boomDirection);
            float searchMaximum = requestedDistance;
            if (HasCandidateQueryCapacity(candidateQueryStart, 1) &&
                TryFindHardBlocker(
                    hand,
                    hand + boomDirection * requestedDistance,
                    Mathf.Max(0.01f, frameInput.CameraRadius),
                    HardQueryRole.HandToCamera,
                    out RaycastHit hit,
                    out Collider boomBlocker,
                    out GeometrySignature boomGeometry))
            {
                if (boomBlocker == null)
                {
                    solved = default;
                    candidate = default;
                    limitingFailure = new ConstraintFailure(
                        ShowcaseCameraNoValidPoseReason.MultipleConstraints,
                        null,
                        null,
                        GeometrySignature.BudgetExceeded,
                        true);
                    return false;
                }
                else
                {
                    limitingFailure = ConstraintFailure.Prefer(
                        limitingFailure,
                        new ConstraintFailure(
                            ShowcaseCameraNoValidPoseReason.StaticWorld,
                            boomBlocker,
                            null,
                            boomGeometry));
                }
                searchMaximum = Mathf.Clamp(
                    hit.distance - Mathf.Max(0.001f, frameInput.ClearanceMargin),
                    MinimumBoomDistance,
                    requestedDistance);
            }

            float previousDistance = searchMaximum;
            float range = Mathf.Max(0f, searchMaximum - MinimumBoomDistance);
            int remainingProbeBudget = Mathf.Max(
                1,
                (MaximumPhysicsQueriesPerCandidate -
                    (TotalPhysicsQueriesThisFrame - candidateQueryStart)) / 3);
            int bracketCount = Mathf.Min(BoomSearchSteps, remainingProbeBudget);
            for (int bracket = 0; bracket <= bracketCount; bracket++)
            {
                if (queryBudgetExceededThisFrame ||
                    !HasCandidateQueryCapacity(candidateQueryStart, 3))
                {
                    break;
                }
                float distance = bracket == 0
                    ? searchMaximum
                    : searchMaximum - range * bracket / Mathf.Max(1, bracketCount);
                SpatialTuple probe = new SpatialTuple(
                    1f,
                    horizontalShoulderFactor,
                    distance);
                if (!EvaluateTupleInternal(
                        probe,
                        orientation,
                        lens,
                        validatePivotChain: false,
                        validateSubjectConstraints: false,
                        out CameraCandidate probeCandidate,
                        out ConstraintFailure probeFailure))
                {
                    limitingFailure = ConstraintFailure.Prefer(
                        limitingFailure,
                        probeFailure);
                    previousDistance = distance;
                    continue;
                }

                solved = probe;
                candidate = probeCandidate;
                float low = distance;
                float high = previousDistance;
                while (high - low > BoomTolerance &&
                       HasCandidateQueryCapacity(candidateQueryStart, 3))
                {
                    float middle = (low + high) * 0.5f;
                    SpatialTuple middleTuple = new SpatialTuple(
                        1f,
                        horizontalShoulderFactor,
                        middle);
                    if (EvaluateTupleInternal(
                            middleTuple,
                            orientation,
                            lens,
                            validatePivotChain: false,
                            validateSubjectConstraints: false,
                            out CameraCandidate middleCandidate,
                            out ConstraintFailure middleFailure))
                    {
                        low = middle;
                        solved = middleTuple;
                        candidate = middleCandidate;
                    }
                    else
                    {
                        high = middle;
                        limitingFailure = ConstraintFailure.Prefer(
                            limitingFailure,
                            middleFailure);
                    }
                }
                return true;
            }

            solved = default;
            candidate = default;
            return false;
        }

        private bool TryFindMaximumStaticHorizontalShoulder(
            Quaternion orientation,
            LensSettings lens,
            out SpatialTuple solved,
            out CameraCandidate candidate,
            out ConstraintFailure limitingFailure)
        {
            if (!TrySolveMaximumStaticBoom(
                    0f,
                    orientation,
                    lens,
                    out solved,
                    out candidate,
                    out limitingFailure))
            {
                return false;
            }

            float low = 0f;
            float high = 1f;
            for (int iteration = 0;
                 iteration < ScalarSearchIterations &&
                 high - low > ScalarTolerance &&
                 !queryBudgetExceededThisFrame;
                 iteration++)
            {
                float middle = (low + high) * 0.5f;
                if (TrySolveMaximumStaticBoom(
                        middle,
                        orientation,
                        lens,
                        out SpatialTuple middleTuple,
                        out CameraCandidate middleCandidate,
                        out ConstraintFailure middleFailure))
                {
                    low = middle;
                    solved = middleTuple;
                    candidate = middleCandidate;
                }
                else
                {
                    high = middle;
                    limitingFailure = ConstraintFailure.Prefer(
                        limitingFailure,
                        middleFailure);
                }
            }
            return true;
        }

        private bool HasCandidateQueryCapacity(
            int candidateQueryStart,
            int estimatedQueryCount)
        {
            int frameLimit = failClosedSafetyPass
                ? MaximumPhysicsQueriesPerFrame + FailClosedSafetyQueryReserve
                : MaximumPhysicsQueriesPerFrame;
            int candidateLimit = Mathf.Min(
                frameLimit,
                candidateQueryStart + MaximumPhysicsQueriesPerCandidate);
            return TotalPhysicsQueriesThisFrame +
                    Mathf.Max(0, estimatedQueryCount) <=
                candidateLimit;
        }

        private bool HasFrameQueryCapacity(int estimatedQueryCount)
        {
            int frameLimit = failClosedSafetyPass
                ? MaximumPhysicsQueriesPerFrame + FailClosedSafetyQueryReserve
                : MaximumPhysicsQueriesPerFrame;
            return TotalPhysicsQueriesThisFrame +
                    Mathf.Max(0, estimatedQueryCount) <=
                frameLimit;
        }

        private int TotalPhysicsQueriesThisFrame =>
            physicsQueriesThisFrame + failClosedSafetyQueriesThisFrame;

        private bool EvaluateTuple(
            SpatialTuple tuple,
            Quaternion orientation,
            LensSettings lens,
            out CameraCandidate candidate,
            out ConstraintFailure failure)
        {
            return EvaluateTupleInternal(
                tuple,
                orientation,
                lens,
                validatePivotChain: true,
                validateSubjectConstraints: true,
                out candidate,
                out failure);
        }

        private bool EvaluateFastSafetyTuple(
            SpatialTuple tuple,
            Quaternion orientation,
            LensSettings lens,
            out CameraCandidate candidate,
            out ConstraintFailure failure)
        {
            float gameplayWeight = Mathf.Clamp01(tuple.GameplayWeight);
            float shoulderScale = Mathf.Clamp01(tuple.ShoulderScale);
            if (!IsPivotChainSweepSafe(
                    gameplayWeight,
                    shoulderScale,
                    out Collider pivotBlocker,
                    out GeometrySignature pivotGeometry))
            {
                candidate = default;
                failure = new ConstraintFailure(
                    ShowcaseCameraNoValidPoseReason.StaticWorld,
                    pivotBlocker,
                    null,
                    pivotGeometry);
                return false;
            }

            return EvaluateTupleInternal(
                tuple,
                orientation,
                lens,
                validatePivotChain: false,
                validateSubjectConstraints: true,
                out candidate,
                out failure);
        }

        private bool EvaluateStaticTuple(
            SpatialTuple tuple,
            Quaternion orientation,
            LensSettings lens,
            out CameraCandidate candidate,
            out ConstraintFailure failure)
        {
            return EvaluateTupleInternal(
                tuple,
                orientation,
                lens,
                validatePivotChain: true,
                validateSubjectConstraints: false,
                out candidate,
                out failure);
        }

        private bool EvaluateTupleInternal(
            SpatialTuple tuple,
            Quaternion orientation,
            LensSettings lens,
            bool validatePivotChain,
            bool validateSubjectConstraints,
            out CameraCandidate candidate,
            out ConstraintFailure failure)
        {
            candidate = default;
            failure = default;
            float gameplayWeight = Mathf.Clamp01(tuple.GameplayWeight);
            float shoulderScale = Mathf.Clamp01(tuple.ShoulderScale);
            if (validatePivotChain && !IsPivotChainSafe(
                    gameplayWeight,
                    shoulderScale,
                    out Collider pivotBlocker,
                    out GeometrySignature pivotGeometry))
            {
                failure = new ConstraintFailure(
                    ShowcaseCameraNoValidPoseReason.StaticWorld,
                    pivotBlocker,
                    null,
                    pivotGeometry);
                return false;
            }

            CameraProfileValues profile = ComposeProfile(gameplayWeight);
            float distance = Mathf.Clamp(
                tuple.BoomDistance,
                MinimumBoomDistance,
                Mathf.Max(MinimumBoomDistance, profile.Distance));
            Quaternion targetRotation = Quaternion.Euler(frameInput.Pitch, frameInput.Yaw, 0f);
            Quaternion heading = Quaternion.Euler(0f, frameInput.Yaw, 0f);
            Vector3 root = frameInput.StableAnchor.position;
            Vector3 shoulder = root + heading * new Vector3(
                profile.ShoulderOffset * shoulderScale,
                profile.ShoulderHeight,
                0f);
            Vector3 hand = shoulder + targetRotation *
                new Vector3(0f, profile.VerticalArmLength, 0f);
            Vector3 boomDirection = -(targetRotation * Vector3.forward).normalized;
            Vector3 position = hand + boomDirection * distance;
            if (TryFindHardBlocker(
                    hand,
                    position,
                    Mathf.Max(0.01f, frameInput.CameraRadius),
                    HardQueryRole.HandToCamera,
                    out _,
                    out Collider boomBlocker,
                    out GeometrySignature boomGeometry))
            {
                failure = new ConstraintFailure(
                    ShowcaseCameraNoValidPoseReason.StaticWorld,
                    boomBlocker,
                    null,
                    boomGeometry);
                return false;
            }

            if (!IsCandidateStaticPositionValid(
                    position,
                    orientation,
                    lens,
                    out GeometrySignature candidateGeometry))
            {
                TryFindHardOverlap(
                    position,
                    Mathf.Max(0.01f, frameInput.CameraRadius),
                    out Collider positionBlocker);
                failure = new ConstraintFailure(
                    NoValidPoseReason,
                    positionBlocker,
                    null,
                    candidateGeometry);
                return false;
            }

            if (validateSubjectConstraints && !IsOutsidePlayerEnvelope(position))
            {
                SubjectConstraintApplied = true;
                failure = new ConstraintFailure(
                    ShowcaseCameraNoValidPoseReason.PlayerEnvelope,
                    null,
                    null);
                return false;
            }

            if (validateSubjectConstraints && !IsOutsideCarryEnvelope(position))
            {
                failure = new ConstraintFailure(
                    ShowcaseCameraNoValidPoseReason.CarryEnvelope,
                    null,
                    null);
                return false;
            }

            if (validateSubjectConstraints &&
                TryFindBlockingActor(position, out ActorProxy actor, out _))
            {
                failure = new ConstraintFailure(
                    ShowcaseCameraNoValidPoseReason.DynamicActor,
                    null,
                    actor.Root);
                return false;
            }

            candidate = new CameraCandidate
            {
                Root = root,
                Shoulder = shoulder,
                Hand = hand,
                Position = position,
                Distance = distance,
                ShoulderScale = shoulderScale,
                GameplayWeight = gameplayWeight,
                Blocker = null,
                WasBoomCompressed = distance < profile.Distance - BoomTolerance
            };
            NoValidPoseReason = ShowcaseCameraNoValidPoseReason.None;
            return true;
        }

        private void GetRigPoints(
            float gameplayWeight,
            float shoulderScale,
            out Vector3 root,
            out Vector3 shoulder,
            out Vector3 hand,
            out Vector3 boomDirection)
        {
            CameraProfileValues profile = ComposeProfile(gameplayWeight);
            Quaternion targetRotation = Quaternion.Euler(frameInput.Pitch, frameInput.Yaw, 0f);
            Quaternion heading = Quaternion.Euler(0f, frameInput.Yaw, 0f);
            root = frameInput.StableAnchor.position;
            shoulder = root + heading * new Vector3(
                profile.ShoulderOffset * shoulderScale,
                profile.ShoulderHeight,
                0f);
            hand = shoulder + targetRotation *
                new Vector3(0f, profile.VerticalArmLength, 0f);
            boomDirection = -(targetRotation * Vector3.forward).normalized;
        }

        private void AdvanceResolvedTuple(
            SpatialTuple target,
            bool targetIsConstraintLimited,
            ShowcaseCameraSolutionFamily targetFamily,
            GeometrySignature targetGeometrySignature,
            Quaternion orientation,
            LensSettings lens,
            float deltaTime,
            bool forceImmediate)
        {
            branchSwitchCooldownRemaining = Mathf.Max(
                0f,
                branchSwitchCooldownRemaining - deltaTime);
            if (!hasResolvedTuple || forceImmediate)
            {
                resolvedTuple = target;
                observedRecoveryTarget = target;
                activeSolutionFamily = targetFamily;
                observedRecoveryFamily = targetFamily;
                activeGeometrySignature = targetGeometrySignature;
                observedRecoveryGeometrySignature = targetGeometrySignature;
                hasResolvedTuple = true;
                constraintRecoveryRequired = targetIsConstraintLimited;
                recoveryActive = false;
                recoveryElapsed = 0f;
                staticReleaseClearTimer = 0f;
                return;
            }

            if (!constraintRecoveryRequired && !targetIsConstraintLimited)
            {
                // Core/Aim/Carry profile blending is already owned by
                // ThirdPersonCameraRig.  Do not reinterpret an ordinary profile
                // transition as collision recovery.
                resolvedTuple = target;
                observedRecoveryTarget = target;
                activeSolutionFamily = targetFamily;
                observedRecoveryFamily = targetFamily;
                activeGeometrySignature = targetGeometrySignature;
                observedRecoveryGeometrySignature = targetGeometrySignature;
                recoveryActive = false;
                recoveryElapsed = 0f;
                staticReleaseClearTimer = 0f;
                return;
            }

            if (!EvaluateTuple(
                    resolvedTuple,
                    orientation,
                    lens,
                    out _,
                    out _))
            {
                SpatialTuple immediateTarget = target;
                ShowcaseCameraSolutionFamily immediateFamily = targetFamily;
                if (TryFindNearestSafeTuple(
                        resolvedTuple,
                        target,
                        targetFamily,
                        orientation,
                        lens,
                        out SpatialTuple nearestSafe,
                        out ShowcaseCameraSolutionFamily nearestFamily))
                {
                    immediateTarget = nearestSafe;
                    immediateFamily = nearestFamily;
                }

                bool familyChanged = immediateFamily != activeSolutionFamily;
                resolvedTuple = immediateTarget;
                observedRecoveryTarget = immediateTarget;
                activeSolutionFamily = immediateFamily;
                observedRecoveryFamily = immediateFamily;
                activeGeometrySignature = targetGeometrySignature;
                observedRecoveryGeometrySignature = targetGeometrySignature;
                constraintRecoveryRequired = true;
                recoveryActive = false;
                recoveryElapsed = 0f;
                staticReleaseClearTimer = 0f;
                if (familyChanged)
                {
                    branchSwitchCooldownRemaining = BranchSwitchCooldown;
                }
                return;
            }

            if (RecoveryEquivalent(
                    resolvedTuple,
                    activeSolutionFamily,
                    activeGeometrySignature,
                    target,
                    targetFamily,
                    targetGeometrySignature))
            {
                resolvedTuple = target;
                observedRecoveryTarget = target;
                activeSolutionFamily = targetFamily;
                observedRecoveryFamily = targetFamily;
                activeGeometrySignature = targetGeometrySignature;
                observedRecoveryGeometrySignature = targetGeometrySignature;
                constraintRecoveryRequired = targetIsConstraintLimited;
                recoveryActive = false;
                recoveryElapsed = 0f;
                staticReleaseClearTimer = 0f;
                return;
            }

            if (!RecoveryEquivalent(
                    observedRecoveryTarget,
                    observedRecoveryFamily,
                    observedRecoveryGeometrySignature,
                    target,
                    targetFamily,
                    targetGeometrySignature))
            {
                observedRecoveryTarget = target;
                observedRecoveryFamily = targetFamily;
                observedRecoveryGeometrySignature = targetGeometrySignature;
                if (targetIsConstraintLimited)
                {
                    constraintRecoveryRequired = true;
                }
                recoveryActive = false;
                recoveryElapsed = 0f;
                staticReleaseClearTimer = 0f;
                return;
            }

            // A safe current branch remains authoritative until another branch
            // has a material, persistent advantage.  A more restrictive target
            // is applied only if the current branch actually becomes unsafe.
            bool returningToRequestedPose =
                targetFamily == ShowcaseCameraSolutionFamily.Requested &&
                !targetIsConstraintLimited;
            if (!returningToRequestedPose &&
                targetFamily != activeSolutionFamily &&
                branchSwitchCooldownRemaining > 0f)
            {
                return;
            }
            if (!returningToRequestedPose &&
                !IsMaterialImprovement(target, resolvedTuple))
            {
                return;
            }

            if (!recoveryActive)
            {
                staticReleaseClearTimer += deltaTime;
                if (staticReleaseClearTimer < ClearHoldDuration)
                {
                    return;
                }
                recoverySourceTuple = resolvedTuple;
                recoveryElapsed = 0f;
                recoveryActive = true;
            }

            recoveryElapsed += deltaTime;
            float duration = Mathf.Max(0.01f, frameInput.CollisionReturnDuration);
            float progress = Mathf.Clamp01(recoveryElapsed / duration);
            SpatialTuple next = SpatialTuple.Lerp(
                recoverySourceTuple,
                observedRecoveryTarget,
                progress);
            if (!EvaluateTuple(next, orientation, lens, out _, out _))
            {
                recoveryActive = false;
                recoveryElapsed = 0f;
                staticReleaseClearTimer = 0f;
                return;
            }

            resolvedTuple = progress >= 1f ? observedRecoveryTarget : next;
            if (progress >= 1f)
            {
                bool familyChanged = activeSolutionFamily != observedRecoveryFamily;
                activeSolutionFamily = observedRecoveryFamily;
                activeGeometrySignature = observedRecoveryGeometrySignature;
                constraintRecoveryRequired = targetIsConstraintLimited;
                recoveryActive = false;
                recoveryElapsed = 0f;
                staticReleaseClearTimer = 0f;
                if (familyChanged)
                {
                    branchSwitchCooldownRemaining = BranchSwitchCooldown;
                }
            }
        }

        private bool TryFindNearestSafeTuple(
            SpatialTuple current,
            SpatialTuple target,
            ShowcaseCameraSolutionFamily targetFamily,
            Quaternion orientation,
            LensSettings lens,
            out SpatialTuple nearest,
            out ShowcaseCameraSolutionFamily nearestFamily)
        {
            nearest = target;
            nearestFamily = targetFamily;
            float nearestDistance = TupleDistanceSquared(current, target);
            bool found = EvaluateTuple(target, orientation, lens, out _, out _);

            if (TrySolveMaximumSafeBoom(
                    current.GameplayWeight,
                    current.ShoulderScale,
                    orientation,
                    lens,
                    out SpatialTuple sameFamilyTuple,
                    out _,
                    out _))
            {
                ShowcaseCameraSolutionFamily sameFamily =
                    ClassifySolutionFamily(sameFamilyTuple, requestedTuple);
                ConsiderNearestSafeTuple(
                    current,
                    sameFamilyTuple,
                    sameFamily,
                    ref found,
                    ref nearest,
                    ref nearestFamily,
                    ref nearestDistance);
            }

            const int nearestSearchSteps = 8;
            for (int step = 1; step <= nearestSearchSteps; step++)
            {
                if (queryBudgetExceededThisFrame)
                {
                    break;
                }
                float amount = step / (float)nearestSearchSteps;
                float gameplayWeight = Mathf.Lerp(
                    current.GameplayWeight,
                    target.GameplayWeight,
                    amount);
                float shoulderScale = Mathf.Lerp(
                    current.ShoulderScale,
                    target.ShoulderScale,
                    amount);
                if (!TrySolveMaximumSafeBoom(
                        gameplayWeight,
                        shoulderScale,
                        orientation,
                        lens,
                        out SpatialTuple candidate,
                        out _,
                        out _))
                {
                    continue;
                }

                ShowcaseCameraSolutionFamily family =
                    ClassifySolutionFamily(candidate, requestedTuple);
                ConsiderNearestSafeTuple(
                    current,
                    candidate,
                    family,
                    ref found,
                    ref nearest,
                    ref nearestFamily,
                    ref nearestDistance);
            }
            return found;
        }

        private void ConsiderNearestSafeTuple(
            SpatialTuple current,
            SpatialTuple candidate,
            ShowcaseCameraSolutionFamily family,
            ref bool found,
            ref SpatialTuple nearest,
            ref ShowcaseCameraSolutionFamily nearestFamily,
            ref float nearestDistance)
        {
            float distance = TupleDistanceSquared(current, candidate);
            bool candidateKeepsFamily = family == activeSolutionFamily;
            bool nearestKeepsFamily = nearestFamily == activeSolutionFamily;
            if (!found ||
                (candidateKeepsFamily && !nearestKeepsFamily) ||
                (candidateKeepsFamily == nearestKeepsFamily &&
                 distance < nearestDistance - 0.000001f))
            {
                found = true;
                nearest = candidate;
                nearestFamily = family;
                nearestDistance = distance;
            }
        }

        private static float TupleDistanceSquared(
            SpatialTuple left,
            SpatialTuple right)
        {
            float gameplay = left.GameplayWeight - right.GameplayWeight;
            float shoulder = left.ShoulderScale - right.ShoulderScale;
            float boom = (left.BoomDistance - right.BoomDistance) * 0.5f;
            return gameplay * gameplay + shoulder * shoulder + boom * boom;
        }

        private static bool RecoveryEquivalent(
            SpatialTuple left,
            ShowcaseCameraSolutionFamily leftFamily,
            GeometrySignature leftGeometry,
            SpatialTuple right,
            ShowcaseCameraSolutionFamily rightFamily,
            GeometrySignature rightGeometry)
        {
            if (leftFamily != rightFamily ||
                (!leftGeometry.IsEmpty &&
                 !rightGeometry.IsEmpty &&
                 leftGeometry != rightGeometry))
            {
                return false;
            }

            return Mathf.Abs(left.GameplayWeight - right.GameplayWeight) <=
                    RecoveryGameplayEpsilon &&
                Mathf.Abs(left.ShoulderScale - right.ShoulderScale) <=
                    RecoveryShoulderEpsilon &&
                Mathf.Abs(left.BoomDistance - right.BoomDistance) <=
                    RecoveryBoomEpsilon;
        }

        private static bool IsMaterialImprovement(
            SpatialTuple candidate,
            SpatialTuple current)
        {
            return candidate.GameplayWeight - current.GameplayWeight >=
                    MaterialGameplayGain ||
                candidate.ShoulderScale - current.ShoulderScale >=
                    MaterialShoulderGain ||
                candidate.BoomDistance - current.BoomDistance >=
                    MaterialBoomGain;
        }

        private static ShowcaseCameraSolutionFamily ClassifySolutionFamily(
            SpatialTuple tuple,
            SpatialTuple requested)
        {
            if (SpatialTuple.Approximately(tuple, requested))
            {
                return ShowcaseCameraSolutionFamily.Requested;
            }
            if (tuple.ShoulderScale <= ScalarTolerance)
            {
                return ShowcaseCameraSolutionFamily.EmergencyCentered;
            }
            if (tuple.ShoulderScale < requested.ShoulderScale - ScalarTolerance)
            {
                return ShowcaseCameraSolutionFamily.ShoulderReduced;
            }
            if (tuple.GameplayWeight < requested.GameplayWeight - ScalarTolerance)
            {
                return ShowcaseCameraSolutionFamily.GameplayReduced;
            }
            return ShowcaseCameraSolutionFamily.BoomCompressed;
        }

        private bool IsPivotChainSafe(
            float gameplayWeight,
            float shoulderScale,
            out Collider blocker)
        {
            return IsPivotChainSafe(
                gameplayWeight,
                shoulderScale,
                out blocker,
                out _);
        }

        private bool IsPivotChainSweepSafe(
            float gameplayWeight,
            float shoulderScale,
            out Collider blocker,
            out GeometrySignature geometrySignature)
        {
            GetRigPoints(
                gameplayWeight,
                shoulderScale,
                out Vector3 root,
                out Vector3 shoulder,
                out Vector3 hand,
                out _);
            float pivotRadius = Mathf.Max(
                0.04f,
                frameInput.CameraRadius * 0.42f);
            blocker = null;
            geometrySignature = default;
            bool blocked = false;
            blocked |= AccumulateHardBlocker(
                root,
                shoulder,
                pivotRadius,
                HardQueryRole.RootToShoulder,
                ref blocker,
                ref geometrySignature);
            blocked |= AccumulateHardBlocker(
                shoulder,
                hand,
                pivotRadius,
                HardQueryRole.ShoulderToHand,
                ref blocker,
                ref geometrySignature);
            return !blocked;
        }

        private bool IsPivotChainSafe(
            float gameplayWeight,
            float shoulderScale,
            out Collider blocker,
            out GeometrySignature geometrySignature)
        {
            CameraProfileValues profile = ComposeProfile(gameplayWeight);
            Quaternion targetRotation = Quaternion.Euler(frameInput.Pitch, frameInput.Yaw, 0f);
            Quaternion heading = Quaternion.Euler(0f, frameInput.Yaw, 0f);
            Vector3 root = frameInput.StableAnchor.position;
            Vector3 shoulder = root + heading * new Vector3(
                profile.ShoulderOffset * shoulderScale,
                profile.ShoulderHeight,
                0f);
            Vector3 hand = shoulder + targetRotation *
                new Vector3(0f, profile.VerticalArmLength, 0f);
            float pivotRadius = Mathf.Max(0.04f, frameInput.CameraRadius * 0.42f);
            blocker = null;
            geometrySignature = default;
            bool blocked = false;
            blocked |= AccumulateHardBlocker(
                root,
                shoulder,
                pivotRadius,
                HardQueryRole.RootToShoulder,
                ref blocker,
                ref geometrySignature);
            blocked |= AccumulateHardOverlap(
                shoulder,
                pivotRadius,
                HardQueryRole.ShoulderVolume,
                ref blocker,
                ref geometrySignature);
            blocked |= AccumulateHardBlocker(
                shoulder,
                hand,
                pivotRadius,
                HardQueryRole.ShoulderToHand,
                ref blocker,
                ref geometrySignature);
            blocked |= AccumulateHardOverlap(
                hand,
                pivotRadius,
                HardQueryRole.HandVolume,
                ref blocker,
                ref geometrySignature);
            return !blocked;
        }

        private bool TryFindRequestedStaticBlocker(
            SpatialTuple tuple,
            out Collider blocker)
        {
            return TryFindRequestedStaticBlocker(tuple, out blocker, out _);
        }

        private bool TryFindRequestedStaticBlocker(
            SpatialTuple tuple,
            out Collider blocker,
            out GeometrySignature geometrySignature)
        {
            blocker = null;
            float gameplayWeight = Mathf.Clamp01(tuple.GameplayWeight);
            float shoulderScale = Mathf.Clamp01(tuple.ShoulderScale);
            CameraProfileValues profile = ComposeProfile(gameplayWeight);
            Quaternion targetRotation = Quaternion.Euler(frameInput.Pitch, frameInput.Yaw, 0f);
            Quaternion heading = Quaternion.Euler(0f, frameInput.Yaw, 0f);
            Vector3 root = frameInput.StableAnchor.position;
            Vector3 shoulder = root + heading * new Vector3(
                profile.ShoulderOffset * shoulderScale,
                profile.ShoulderHeight,
                0f);
            Vector3 hand = shoulder + targetRotation *
                new Vector3(0f, profile.VerticalArmLength, 0f);
            Vector3 boomDirection = -(targetRotation * Vector3.forward).normalized;
            Vector3 camera = hand + boomDirection * Mathf.Max(
                MinimumBoomDistance,
                tuple.BoomDistance);
            float pivotRadius = Mathf.Max(0.04f, frameInput.CameraRadius * 0.42f);
            float cameraRadius = Mathf.Max(0.01f, frameInput.CameraRadius);

            blocker = null;
            geometrySignature = default;
            bool blocked = false;
            blocked |= AccumulateHardBlocker(
                root,
                shoulder,
                pivotRadius,
                HardQueryRole.RootToShoulder,
                ref blocker,
                ref geometrySignature);
            blocked |= AccumulateHardOverlap(
                shoulder,
                pivotRadius,
                HardQueryRole.ShoulderVolume,
                ref blocker,
                ref geometrySignature);
            blocked |= AccumulateHardBlocker(
                shoulder,
                hand,
                pivotRadius,
                HardQueryRole.ShoulderToHand,
                ref blocker,
                ref geometrySignature);
            blocked |= AccumulateHardOverlap(
                hand,
                pivotRadius,
                HardQueryRole.HandVolume,
                ref blocker,
                ref geometrySignature);
            blocked |= AccumulateHardBlocker(
                hand,
                camera,
                cameraRadius,
                HardQueryRole.HandToCamera,
                ref blocker,
                ref geometrySignature);
            blocked |= AccumulateHardOverlap(
                camera,
                cameraRadius,
                HardQueryRole.CameraBody,
                ref blocker,
                ref geometrySignature);
            return blocked;
        }

        private bool AccumulateHardBlocker(
            Vector3 start,
            Vector3 end,
            float radius,
            HardQueryRole queryRole,
            ref Collider blocker,
            ref GeometrySignature geometrySignature)
        {
            bool blocked = TryFindHardBlocker(
                start,
                end,
                radius,
                queryRole,
                out _,
                out Collider found,
                out GeometrySignature foundGeometry);
            if (blocked)
            {
                blocker = ChooseDiagnosticBlocker(blocker, found);
                geometrySignature = GeometrySignature.Combine(
                    geometrySignature,
                    foundGeometry);
            }
            return blocked;
        }

        private bool AccumulateHardOverlap(
            Vector3 position,
            float radius,
            HardQueryRole queryRole,
            ref Collider blocker,
            ref GeometrySignature geometrySignature)
        {
            bool blocked = TryFindHardOverlap(
                position,
                radius,
                queryRole,
                out Collider found,
                out GeometrySignature foundGeometry);
            if (blocked)
            {
                blocker = ChooseDiagnosticBlocker(blocker, found);
                geometrySignature = GeometrySignature.Combine(
                    geometrySignature,
                    foundGeometry);
            }
            return blocked;
        }

        private static Collider ChooseDiagnosticBlocker(
            Collider current,
            Collider candidate)
        {
            if (current == null) return candidate;
            if (candidate == null) return current;
            return candidate.GetInstanceID() < current.GetInstanceID()
                ? candidate
                : current;
        }

        private CameraProfileValues ComposeProfile(float gameplayWeight)
        {
            Vector4 offsets = frameInput.GameplayOffsets * Mathf.Clamp01(gameplayWeight);
            return new CameraProfileValues(
                frameInput.CoreTargetHeight + offsets.x,
                frameInput.CoreDistance + offsets.y,
                (frameInput.CoreShoulderOffset + offsets.z) *
                    cornerShoulderIntentFactor,
                frameInput.CoreShoulderHeight,
                frameInput.CoreVerticalArmLength + offsets.w);
        }

        private bool IsCandidatePositionValid(
            Vector3 position,
            Quaternion orientation,
            LensSettings lens)
        {
            return IsCandidatePositionValid(
                position,
                orientation,
                lens,
                out _);
        }

        private bool IsCandidatePositionValid(
            Vector3 position,
            Quaternion orientation,
            LensSettings lens,
            out GeometrySignature geometrySignature)
        {
            if (!IsCandidateStaticPositionValid(
                    position,
                    orientation,
                    lens,
                    out geometrySignature))
            {
                return false;
            }
            if (!IsOutsidePlayerEnvelope(position))
            {
                SubjectConstraintApplied = true;
                NoValidPoseReason = ShowcaseCameraNoValidPoseReason.PlayerEnvelope;
                return false;
            }
            if (!IsOutsideCarryEnvelope(position))
            {
                NoValidPoseReason = ShowcaseCameraNoValidPoseReason.CarryEnvelope;
                return false;
            }
            return true;
        }

        private bool IsCandidateStaticPositionValid(
            Vector3 position,
            Quaternion orientation,
            LensSettings lens,
            out GeometrySignature geometrySignature)
        {
            geometrySignature = default;
            if (TryFindHardOverlap(
                    position,
                    Mathf.Max(0.01f, frameInput.CameraRadius),
                    HardQueryRole.CameraBody,
                    out _,
                    out GeometrySignature bodyGeometry))
            {
                geometrySignature = bodyGeometry;
                NoValidPoseReason = ShowcaseCameraNoValidPoseReason.StaticWorld;
                return false;
            }
            if (!IsNearPlaneSafe(
                    position,
                    orientation,
                    lens,
                    out GeometrySignature nearPlaneGeometry))
            {
                geometrySignature = nearPlaneGeometry;
                NoValidPoseReason = ShowcaseCameraNoValidPoseReason.NearPlane;
                return false;
            }
            return true;
        }

        private bool IsNearPlaneSafe(
            Vector3 cameraPosition,
            Quaternion orientation,
            LensSettings lens)
        {
            return IsNearPlaneSafe(
                cameraPosition,
                orientation,
                lens,
                out _);
        }

        private bool IsNearPlaneSafe(
            Vector3 cameraPosition,
            Quaternion orientation,
            LensSettings lens,
            out GeometrySignature geometrySignature)
        {
            float near = Mathf.Max(0.01f, lens.NearClipPlane);
            float aspect = lens.Aspect > 0f ? lens.Aspect : 16f / 9f;
            float halfHeight = near * Mathf.Tan(
                Mathf.Clamp(lens.FieldOfView, 1f, 179f) * 0.5f * Mathf.Deg2Rad);
            float halfWidth = halfHeight * aspect;
            float thickness = Mathf.Max(0.01f, frameInput.ClearanceMargin * 0.5f);
            Vector3 center = cameraPosition + orientation * Vector3.forward * (near * 0.5f);
            Vector3 halfExtents = new Vector3(
                halfWidth + thickness,
                halfHeight + thickness,
                near * 0.5f + thickness);
            return !TryFindHardBoxOverlap(
                center,
                halfExtents,
                orientation,
                HardQueryRole.NearPlane,
                out _,
                out geometrySignature);
        }

        private bool IsRequestedStaticPoseClearWithMargin(
            Quaternion orientation,
            LensSettings lens,
            out Collider blocker)
        {
            GetRigPoints(
                1f,
                1f,
                out Vector3 root,
                out Vector3 shoulder,
                out Vector3 hand,
                out Vector3 boomDirection);
            Vector3 camera = hand + boomDirection * requestedBoomDistance;
            float pivotRadius = Mathf.Max(
                0.04f,
                frameInput.CameraRadius * 0.42f) + StaticReleaseSpatialMargin;
            float cameraRadius = Mathf.Max(
                0.01f,
                frameInput.CameraRadius) + StaticReleaseSpatialMargin;

            if (TryFindHardBlocker(
                    root,
                    shoulder,
                    pivotRadius,
                    out _,
                    out blocker) ||
                TryFindHardOverlap(shoulder, pivotRadius, out blocker) ||
                TryFindHardBlocker(
                    shoulder,
                    hand,
                    pivotRadius,
                    out _,
                    out blocker) ||
                TryFindHardOverlap(hand, pivotRadius, out blocker) ||
                TryFindHardBlocker(
                    hand,
                    camera,
                    cameraRadius,
                    out _,
                    out blocker) ||
                TryFindHardOverlap(camera, cameraRadius, out blocker) ||
                !IsNearPlaneSafeWithMargin(
                    camera,
                    orientation,
                    lens,
                    StaticReleaseSpatialMargin,
                    out blocker))
            {
                return false;
            }

            return !queryBudgetExceededThisFrame;
        }

        private bool IsNearPlaneSafeWithMargin(
            Vector3 cameraPosition,
            Quaternion orientation,
            LensSettings lens,
            float margin,
            out Collider blocker)
        {
            float near = Mathf.Max(0.01f, lens.NearClipPlane);
            float aspect = lens.Aspect > 0f ? lens.Aspect : 16f / 9f;
            float halfHeight = near * Mathf.Tan(
                Mathf.Clamp(lens.FieldOfView, 1f, 179f) *
                0.5f * Mathf.Deg2Rad);
            float halfWidth = halfHeight * aspect;
            float thickness = Mathf.Max(
                0.01f,
                frameInput.ClearanceMargin * 0.5f);
            float safeMargin = Mathf.Max(0f, margin);
            Vector3 center = cameraPosition +
                orientation * Vector3.forward * (near * 0.5f);
            Vector3 halfExtents = new Vector3(
                halfWidth + thickness + safeMargin,
                halfHeight + thickness + safeMargin,
                near * 0.5f + thickness + safeMargin);
            return !TryFindHardBoxOverlap(
                center,
                halfExtents,
                orientation,
                out blocker);
        }

        private bool IsOutsidePlayerEnvelope(Vector3 position)
        {
            CharacterController controller = frameInput.PlayerController;
            if (controller == null)
            {
                Vector3 center = frameInput.CornerSnapshot.PlayerRootPose.position +
                    Vector3.up;
                return Vector3.Distance(position, center) >=
                    Mathf.Max(1.1f, frameInput.CameraRadius + SubjectSkin + 0.3f);
            }

            GetCharacterControllerCapsule(
                controller,
                out Vector3 bottom,
                out Vector3 top,
                out float radius);
            float height = Vector3.Distance(bottom, top) + radius * 2f;
            float stableSubjectRadius = Mathf.Max(
                radius,
                height * PlayerEnvelopeRadiusHeightFactor);
            Vector3 closest = ClosestPointOnSegment(position, bottom, top);
            return Vector3.Distance(position, closest) >=
                stableSubjectRadius +
                Mathf.Max(0.01f, frameInput.CameraRadius) +
                SubjectSkin;
        }

        private bool IsOutsideCarryEnvelope(Vector3 position)
        {
            PrototypeInteractionTarget target = frameInput.CarriedTarget;
            if (target == null)
            {
                return true;
            }
            float required = Mathf.Max(0.01f, frameInput.CameraRadius) + SubjectSkin;
            foreach (Collider collider in cachedCarryColliders)
            {
                if (collider == null || !collider.enabled || collider.isTrigger)
                {
                    continue;
                }
                Vector3 closest = collider.ClosestPoint(position);
                if (Vector3.Distance(position, closest) < required)
                {
                    return false;
                }
            }
            return true;
        }

        private bool TryFindBlockingActor(
            Vector3 cameraPosition,
            out ActorProxy blocking,
            out float deepestPenetration)
        {
            blocking = default;
            deepestPenetration = 0f;
            float cameraRadius = Mathf.Max(0.01f, frameInput.CameraRadius);
            int bestId = int.MaxValue;
            foreach (ActorProxy proxy in actorProxies)
            {
                if (!proxy.IsValid ||
                    proxy.Root == frameInput.PlayerRoot ||
                    (frameInput.CarriedTarget != null &&
                     (proxy.Root == frameInput.CarriedTarget.transform ||
                      proxy.Root.IsChildOf(frameInput.CarriedTarget.transform))))
                {
                    continue;
                }
                proxy.GetCapsule(out Vector3 bottom, out Vector3 top, out float radius);
                Vector3 closest = ClosestPointOnSegment(cameraPosition, bottom, top);
                float penetration = radius + cameraRadius -
                    Vector3.Distance(cameraPosition, closest);
                if (penetration <= 0f)
                {
                    continue;
                }
                int id = proxy.Root.GetInstanceID();
                if (penetration > deepestPenetration + 0.0001f ||
                    (Mathf.Abs(penetration - deepestPenetration) <= 0.0001f &&
                     id < bestId))
                {
                    blocking = proxy;
                    deepestPenetration = penetration;
                    bestId = id;
                }
            }
            return blocking.IsValid;
        }

        private bool TryUseRevalidatedPreviousPose(
            Quaternion orientation,
            LensSettings lens,
            out Vector3 previous)
        {
            previous = hasLastSolvedPose ? lastSolvedPose.position : Vector3.zero;
            return hasLastSolvedPose &&
                IsFinite(previous) &&
                IsCandidatePositionValid(previous, orientation, lens) &&
                !TryFindBlockingActor(previous, out _, out _);
        }

        private bool TryUseStableEmergencyPoseWithoutWorldQueries(
            out Vector3 previous)
        {
            previous = hasLastSolvedPose ? lastSolvedPose.position : Vector3.zero;
            return hasLastSolvedPose &&
                IsOutsidePlayerEnvelope(previous) &&
                IsOutsideCarryEnvelope(previous);
        }

        private bool TryFindHardBlocker(
            Vector3 start,
            Vector3 end,
            float radius,
            out RaycastHit nearestHit,
            out Collider blocker)
        {
            return TryFindHardBlocker(
                start,
                end,
                radius,
                HardQueryRole.GenericCast,
                out nearestHit,
                out blocker,
                out _);
        }

        private bool TryFindHardBlocker(
            Vector3 start,
            Vector3 end,
            float radius,
            HardQueryRole queryRole,
            out RaycastHit nearestHit,
            out Collider blocker,
            out GeometrySignature geometrySignature)
        {
            nearestHit = default;
            blocker = null;
            geometrySignature = default;
            Vector3 delta = end - start;
            float distance = delta.magnitude;
            if (distance <= 0.0001f)
            {
                return TryFindHardOverlap(
                    start,
                    radius,
                    queryRole,
                    out blocker,
                    out geometrySignature);
            }

            if (!TryConsumePhysicsQuery())
            {
                geometrySignature = GeometrySignature.BudgetExceeded;
                return true;
            }

            int hitCount = Physics.SphereCastNonAlloc(
                start,
                Mathf.Max(0.001f, radius),
                delta / distance,
                castHits,
                distance,
                hardWorldQueryMask,
                QueryTriggerInteraction.Ignore);
            bool saturated = hitCount >= castHits.Length;
            if (saturated)
            {
                QuerySaturationCount++;
            }

            hardContacts.Clear();
            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = castHits[index];
                if (!IsHardWorldCollider(hit.collider))
                {
                    continue;
                }

                Vector3 normal = hit.normal.sqrMagnitude > 0.000001f
                    ? hit.normal.normalized
                    : -(delta / distance);
                hardContacts.Add(HardContact.FromCast(
                    queryRole,
                    hit,
                    normal));
            }

            SortHardContacts(hardContacts);
            geometrySignature = collectGeometrySignatureThisPass
                ? BuildGeometrySignature(hardContacts)
                : activeGeometrySignature;
            if (hardContacts.Count > 0)
            {
                HardContact limiting = hardContacts[0];
                nearestHit = limiting.CastHit;
                blocker = limiting.Collider;
                return true;
            }

            // A full NonAlloc buffer is an indeterminate query.  Treat it as
            // unsafe instead of trusting an arbitrary partial result set.
            return saturated;
        }

        private bool TryFindHardOverlap(
            Vector3 position,
            float radius,
            out Collider blocker)
        {
            return TryFindHardOverlap(
                position,
                radius,
                HardQueryRole.GenericOverlap,
                out blocker,
                out _);
        }

        private bool TryFindHardOverlap(
            Vector3 position,
            float radius,
            HardQueryRole queryRole,
            out Collider blocker,
            out GeometrySignature geometrySignature)
        {
            blocker = null;
            geometrySignature = default;
            float safeRadius = Mathf.Max(0.001f, radius);
            if (!TryConsumePhysicsQuery())
            {
                geometrySignature = GeometrySignature.BudgetExceeded;
                return true;
            }
            int count = Physics.OverlapSphereNonAlloc(
                position,
                safeRadius,
                overlapHits,
                hardWorldQueryMask,
                QueryTriggerInteraction.Ignore);
            bool saturated = count >= overlapHits.Length;
            if (saturated)
            {
                QuerySaturationCount++;
            }

            hardContacts.Clear();
            for (int index = 0; index < count; index++)
            {
                Collider collider = overlapHits[index];
                if (IsHardWorldCollider(collider))
                {
                    hardContacts.Add(BuildOverlapContact(
                        queryRole,
                        collider,
                        position,
                        safeRadius));
                }
            }

            SortHardContacts(hardContacts);
            geometrySignature = collectGeometrySignatureThisPass
                ? BuildGeometrySignature(hardContacts)
                : activeGeometrySignature;
            if (hardContacts.Count > 0)
            {
                blocker = hardContacts[0].Collider;
                return true;
            }
            return saturated;
        }

        private bool TryFindHardBoxOverlap(
            Vector3 center,
            Vector3 halfExtents,
            Quaternion orientation,
            out Collider blocker)
        {
            return TryFindHardBoxOverlap(
                center,
                halfExtents,
                orientation,
                HardQueryRole.NearPlane,
                out blocker,
                out _);
        }

        private bool TryFindHardBoxOverlap(
            Vector3 center,
            Vector3 halfExtents,
            Quaternion orientation,
            HardQueryRole queryRole,
            out Collider blocker,
            out GeometrySignature geometrySignature)
        {
            blocker = null;
            geometrySignature = default;
            if (!TryConsumePhysicsQuery())
            {
                geometrySignature = GeometrySignature.BudgetExceeded;
                return true;
            }
            int count = Physics.OverlapBoxNonAlloc(
                center,
                halfExtents,
                overlapHits,
                orientation,
                hardWorldQueryMask,
                QueryTriggerInteraction.Ignore);
            bool saturated = count >= overlapHits.Length;
            if (saturated)
            {
                QuerySaturationCount++;
            }

            hardContacts.Clear();
            for (int index = 0; index < count; index++)
            {
                Collider collider = overlapHits[index];
                if (IsHardWorldCollider(collider))
                {
                    hardContacts.Add(BuildOverlapContact(
                        queryRole,
                        collider,
                        center,
                        halfExtents.magnitude));
                }
            }

            SortHardContacts(hardContacts);
            geometrySignature = collectGeometrySignatureThisPass
                ? BuildGeometrySignature(hardContacts)
                : activeGeometrySignature;
            if (hardContacts.Count > 0)
            {
                blocker = hardContacts[0].Collider;
                return true;
            }
            return saturated;
        }

        private bool TryConsumePhysicsQuery()
        {
            if (physicsQueriesThisFrame < MaximumPhysicsQueriesPerFrame)
            {
                physicsQueriesThisFrame++;
                return true;
            }

            if (failClosedSafetyPass &&
                failClosedSafetyQueriesThisFrame < FailClosedSafetyQueryReserve)
            {
                failClosedSafetyQueriesThisFrame++;
                return true;
            }

            if (!queryBudgetExceededThisFrame)
            {
                queryBudgetExceededThisFrame = true;
                queryBudgetExceededCount++;
            }
            return false;
        }

        private static HardContact BuildOverlapContact(
            HardQueryRole queryRole,
            Collider collider,
            Vector3 queryCenter,
            float queryRadius)
        {
            Vector3 closest = collider.ClosestPoint(queryCenter);
            Vector3 separation = queryCenter - closest;
            Vector3 normal;
            Vector3 point;
            float surfaceDistance;
            if (separation.sqrMagnitude > 0.000001f)
            {
                surfaceDistance = separation.magnitude;
                normal = separation / surfaceDistance;
                point = closest;
            }
            else
            {
                ResolveNearestBoundsPlane(
                    collider.bounds,
                    queryCenter,
                    out normal,
                    out point,
                    out surfaceDistance);
            }

            return HardContact.FromOverlap(
                queryRole,
                collider,
                normal,
                point,
                Mathf.Max(0f, queryRadius - surfaceDistance));
        }

        private static void ResolveNearestBoundsPlane(
            Bounds bounds,
            Vector3 point,
            out Vector3 normal,
            out Vector3 surfacePoint,
            out float distance)
        {
            int nearest = 0;
            float nearestDistance = Mathf.Abs(point.x - bounds.min.x);
            ConsiderBoundsPlane(1, Mathf.Abs(bounds.max.x - point.x),
                ref nearest, ref nearestDistance);
            ConsiderBoundsPlane(2, Mathf.Abs(point.y - bounds.min.y),
                ref nearest, ref nearestDistance);
            ConsiderBoundsPlane(3, Mathf.Abs(bounds.max.y - point.y),
                ref nearest, ref nearestDistance);
            ConsiderBoundsPlane(4, Mathf.Abs(point.z - bounds.min.z),
                ref nearest, ref nearestDistance);
            ConsiderBoundsPlane(5, Mathf.Abs(bounds.max.z - point.z),
                ref nearest, ref nearestDistance);

            normal = nearest switch
            {
                0 => Vector3.left,
                1 => Vector3.right,
                2 => Vector3.down,
                3 => Vector3.up,
                4 => Vector3.back,
                _ => Vector3.forward
            };
            surfacePoint = point;
            switch (nearest)
            {
                case 0: surfacePoint.x = bounds.min.x; break;
                case 1: surfacePoint.x = bounds.max.x; break;
                case 2: surfacePoint.y = bounds.min.y; break;
                case 3: surfacePoint.y = bounds.max.y; break;
                case 4: surfacePoint.z = bounds.min.z; break;
                default: surfacePoint.z = bounds.max.z; break;
            }
            distance = nearestDistance;
        }

        private static void ConsiderBoundsPlane(
            int candidateIndex,
            float candidateDistance,
            ref int nearestIndex,
            ref float nearestDistance)
        {
            if (candidateDistance < nearestDistance)
            {
                nearestIndex = candidateIndex;
                nearestDistance = candidateDistance;
            }
        }

        private static void SortHardContacts(List<HardContact> contacts)
        {
            for (int index = 1; index < contacts.Count; index++)
            {
                HardContact value = contacts[index];
                int insertion = index - 1;
                while (insertion >= 0 &&
                       HardContact.Compare(value, contacts[insertion]) < 0)
                {
                    contacts[insertion + 1] = contacts[insertion];
                    insertion--;
                }
                contacts[insertion + 1] = value;
            }
        }

        private GeometrySignature BuildGeometrySignature(
            List<HardContact> contacts)
        {
            geometryPlanes.Clear();
            foreach (HardContact contact in contacts)
            {
                bool duplicate = false;
                foreach (GeometryPlaneKey existing in geometryPlanes)
                {
                    if (existing == contact.PlaneKey)
                    {
                        duplicate = true;
                        break;
                    }
                }
                if (!duplicate)
                {
                    geometryPlanes.Add(contact.PlaneKey);
                }
            }
            for (int index = 1; index < geometryPlanes.Count; index++)
            {
                GeometryPlaneKey value = geometryPlanes[index];
                int insertion = index - 1;
                while (insertion >= 0 &&
                       GeometryPlaneKey.Compare(value, geometryPlanes[insertion]) < 0)
                {
                    geometryPlanes[insertion + 1] = geometryPlanes[insertion];
                    insertion--;
                }
                geometryPlanes[insertion + 1] = value;
            }

            GeometrySignature signature = default;
            foreach (GeometryPlaneKey plane in geometryPlanes)
            {
                signature = GeometrySignature.Combine(signature, plane);
            }
            return signature;
        }

        private bool IsHardWorldCollider(Collider collider)
        {
            if (collider == null || !collider.enabled || collider.isTrigger)
            {
                return false;
            }
            Transform colliderTransform = collider.transform;
            if (IsInHierarchy(colliderTransform, frameInput.PlayerRoot) ||
                (frameInput.CarriedTarget != null &&
                 IsInHierarchy(colliderTransform, frameInput.CarriedTarget.transform)) ||
                collider.GetComponentInParent<ShowcaseCameraSoftOccluderRoot>() != null ||
                collider.GetComponentInParent<PrototypeInteractionTarget>() != null ||
                collider.GetComponentInParent<ShowcaseRunnerAgent>() != null ||
                collider.GetComponentInParent<ShowcaseMobileAllyController>() != null)
            {
                return false;
            }

            int layerBit = 1 << collider.gameObject.layer;
            return (hardWorldLayers.value & layerBit) != 0 ||
                hardWorldColliderIds.Contains(collider.GetInstanceID());
        }

        private void RefreshHardWorldColliders()
        {
            hardWorldColliderIds.Clear();
            hardWorldQueryMask = hardWorldLayers.value;
            ShowcaseCameraWorldBlockerRoot[] roots =
                Object.FindObjectsByType<ShowcaseCameraWorldBlockerRoot>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.InstanceID);
            foreach (ShowcaseCameraWorldBlockerRoot root in roots)
            {
                if (root == null)
                {
                    continue;
                }

                Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
                foreach (Collider collider in colliders)
                {
                    if (collider == null || collider.isTrigger ||
                        collider.GetComponentInParent<
                            ShowcaseCameraSoftOccluderRoot>() != null)
                    {
                        continue;
                    }

                    int layerBit = 1 << collider.gameObject.layer;
                    if ((hardWorldLayers.value & layerBit) != 0)
                    {
                        hardWorldColliderIds.Add(collider.GetInstanceID());
                    }
                }
            }
        }

        private void RefreshActorProxies()
        {
            actorProxies.Clear();
            ShowcaseRunnerAgent[] runners = Object.FindObjectsByType<ShowcaseRunnerAgent>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.InstanceID);
            foreach (ShowcaseRunnerAgent runner in runners)
            {
                AddActorProxy(runner.transform);
            }
            ShowcaseMobileAllyController[] allies =
                Object.FindObjectsByType<ShowcaseMobileAllyController>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.InstanceID);
            foreach (ShowcaseMobileAllyController ally in allies)
            {
                AddActorProxy(ally.transform);
            }
            for (int index = 1; index < actorProxies.Count; index++)
            {
                ActorProxy value = actorProxies[index];
                int insertAt = index - 1;
                while (insertAt >= 0 &&
                       actorProxies[insertAt].Root.GetInstanceID() >
                       value.Root.GetInstanceID())
                {
                    actorProxies[insertAt + 1] = actorProxies[insertAt];
                    insertAt--;
                }
                actorProxies[insertAt + 1] = value;
            }
        }

        private void AddActorProxy(Transform root)
        {
            if (root == null)
            {
                return;
            }
            foreach (ActorProxy existing in actorProxies)
            {
                if (existing.Root == root)
                {
                    return;
                }
            }
            CharacterController controller = root.GetComponent<CharacterController>();
            CapsuleCollider capsule = root.GetComponent<CapsuleCollider>();
            if (controller == null && capsule == null)
            {
                controller = root.GetComponentInChildren<CharacterController>(true);
                capsule = root.GetComponentInChildren<CapsuleCollider>(true);
            }
            if (controller != null || capsule != null)
            {
                actorProxies.Add(new ActorProxy(root, controller, capsule));
            }
        }

        private void ResolveLayerMasks()
        {
            int staticLayer = LayerMask.NameToLayer("CameraStaticWorld");
            int kinematicLayer = LayerMask.NameToLayer("CameraKinematicWorld");
            int actorLayer = LayerMask.NameToLayer("CameraActorProxy");
            int hardMask = 0;
            if (staticLayer >= 0)
            {
                hardMask |= 1 << staticLayer;
            }
            if (kinematicLayer >= 0)
            {
                hardMask |= 1 << kinematicLayer;
            }
            hardWorldLayers = hardMask;
            hardWorldQueryMask = hardMask;
            actorProxyLayers = actorLayer >= 0 ? 1 << actorLayer : 0;
        }

        private static bool IsInHierarchy(Transform candidate, Transform root)
        {
            return candidate != null && root != null &&
                (candidate == root || candidate.IsChildOf(root));
        }

        private static Vector3 ClosestPointOnSegment(
            Vector3 point,
            Vector3 start,
            Vector3 end)
        {
            Vector3 segment = end - start;
            float lengthSquared = segment.sqrMagnitude;
            if (lengthSquared <= 0.0001f)
            {
                return start;
            }
            float amount = Mathf.Clamp01(Vector3.Dot(point - start, segment) / lengthSquared);
            return start + segment * amount;
        }

        private static bool IsFinite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
                !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
                !float.IsNaN(value.z) && !float.IsInfinity(value.z);
        }

        private static void GetCharacterControllerCapsule(
            CharacterController controller,
            out Vector3 bottom,
            out Vector3 top,
            out float radius)
        {
            Vector3 scale = controller.transform.lossyScale;
            float verticalScale = Mathf.Abs(scale.y);
            float radialScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            radius = controller.radius * radialScale;
            float height = Mathf.Max(radius * 2f, controller.height * verticalScale);
            Vector3 center = controller.transform.TransformPoint(controller.center);
            float halfSegment = Mathf.Max(0f, height * 0.5f - radius);
            bottom = center - Vector3.up * halfSegment;
            top = center + Vector3.up * halfSegment;
        }

        private enum HardQueryRole
        {
            GenericCast,
            GenericOverlap,
            RootToShoulder,
            ShoulderVolume,
            ShoulderToHand,
            HandVolume,
            HandToCamera,
            CameraBody,
            NearPlane,
            Sightline
        }

        private readonly struct GeometryPlaneKey
        {
            public GeometryPlaneKey(
                HardQueryRole queryRole,
                Vector3 normal,
                Vector3 point)
            {
                Vector3 safeNormal = normal.sqrMagnitude > 0.000001f
                    ? normal.normalized
                    : Vector3.up;
                QueryRole = (int)queryRole;
                NormalX = Mathf.RoundToInt(
                    safeNormal.x / ContactNormalQuantization);
                NormalY = Mathf.RoundToInt(
                    safeNormal.y / ContactNormalQuantization);
                NormalZ = Mathf.RoundToInt(
                    safeNormal.z / ContactNormalQuantization);
                PlaneOffset = Mathf.RoundToInt(
                    Vector3.Dot(safeNormal, point) /
                    ContactPlaneQuantization);
            }

            public int QueryRole { get; }
            public int NormalX { get; }
            public int NormalY { get; }
            public int NormalZ { get; }
            public int PlaneOffset { get; }

            public static int Compare(
                GeometryPlaneKey left,
                GeometryPlaneKey right)
            {
                int result = left.QueryRole.CompareTo(right.QueryRole);
                if (result != 0) return result;
                result = left.NormalX.CompareTo(right.NormalX);
                if (result != 0) return result;
                result = left.NormalY.CompareTo(right.NormalY);
                if (result != 0) return result;
                result = left.NormalZ.CompareTo(right.NormalZ);
                if (result != 0) return result;
                return left.PlaneOffset.CompareTo(right.PlaneOffset);
            }

            public static bool operator ==(
                GeometryPlaneKey left,
                GeometryPlaneKey right)
            {
                return Compare(left, right) == 0;
            }

            public static bool operator !=(
                GeometryPlaneKey left,
                GeometryPlaneKey right)
            {
                return !(left == right);
            }

            public override bool Equals(object obj)
            {
                return obj is GeometryPlaneKey other && this == other;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = QueryRole;
                    hash = hash * 397 ^ NormalX;
                    hash = hash * 397 ^ NormalY;
                    hash = hash * 397 ^ NormalZ;
                    return hash * 397 ^ PlaneOffset;
                }
            }
        }

        private readonly struct GeometrySignature
        {
            private const ulong OffsetBasis = 1469598103934665603UL;
            private const ulong Prime = 1099511628211UL;

            public static GeometrySignature BudgetExceeded =>
                new(0xB0D6E7E5UL);

            public GeometrySignature(ulong value)
            {
                Value = value;
            }

            public ulong Value { get; }
            public bool IsEmpty => Value == 0UL;

            public static GeometrySignature Combine(
                GeometrySignature source,
                GeometryPlaneKey plane)
            {
                ulong value = source.IsEmpty ? OffsetBasis : source.Value;
                value = Mix(value, plane.QueryRole);
                value = Mix(value, plane.NormalX);
                value = Mix(value, plane.NormalY);
                value = Mix(value, plane.NormalZ);
                value = Mix(value, plane.PlaneOffset);
                return new GeometrySignature(value);
            }

            public static GeometrySignature Combine(
                GeometrySignature left,
                GeometrySignature right)
            {
                if (right.IsEmpty) return left;
                if (left.IsEmpty) return right;
                ulong value = left.Value;
                value ^= right.Value;
                value *= Prime;
                return new GeometrySignature(value);
            }

            private static ulong Mix(ulong value, int component)
            {
                unchecked
                {
                    value ^= (uint)component;
                    return value * Prime;
                }
            }

            public static bool operator ==(
                GeometrySignature left,
                GeometrySignature right)
            {
                return left.Value == right.Value;
            }

            public static bool operator !=(
                GeometrySignature left,
                GeometrySignature right)
            {
                return !(left == right);
            }

            public override bool Equals(object obj)
            {
                return obj is GeometrySignature other && this == other;
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }
        }

        private readonly struct HardContact
        {
            private HardContact(
                HardQueryRole queryRole,
                Collider collider,
                RaycastHit castHit,
                Vector3 normal,
                Vector3 point,
                float castDistance,
                float penetrationDepth,
                bool isPenetrating)
            {
                Collider = collider;
                CastHit = castHit;
                CastDistance = Mathf.Max(0f, castDistance);
                PenetrationDepth = Mathf.Max(0f, penetrationDepth);
                IsPenetrating = isPenetrating;
                PlaneKey = new GeometryPlaneKey(queryRole, normal, point);
                ColliderId = collider != null ? collider.GetInstanceID() : int.MaxValue;
            }

            public Collider Collider { get; }
            public RaycastHit CastHit { get; }
            public float CastDistance { get; }
            public float PenetrationDepth { get; }
            public bool IsPenetrating { get; }
            public GeometryPlaneKey PlaneKey { get; }
            public int ColliderId { get; }

            public static HardContact FromCast(
                HardQueryRole queryRole,
                RaycastHit hit,
                Vector3 normal)
            {
                return new HardContact(
                    queryRole,
                    hit.collider,
                    hit,
                    normal,
                    hit.point,
                    hit.distance,
                    0f,
                    false);
            }

            public static HardContact FromOverlap(
                HardQueryRole queryRole,
                Collider collider,
                Vector3 normal,
                Vector3 point,
                float penetrationDepth)
            {
                return new HardContact(
                    queryRole,
                    collider,
                    default,
                    normal,
                    point,
                    0f,
                    penetrationDepth,
                    true);
            }

            public static int Compare(HardContact left, HardContact right)
            {
                int role = GeometryPlaneKey.Compare(left.PlaneKey, right.PlaneKey);
                if (left.IsPenetrating != right.IsPenetrating)
                {
                    return left.IsPenetrating ? -1 : 1;
                }
                int penetration = right.PenetrationDepth.CompareTo(
                    left.PenetrationDepth);
                if (penetration != 0) return penetration;
                int distance = left.CastDistance.CompareTo(right.CastDistance);
                if (distance != 0) return distance;
                if (role != 0) return role;
                return left.ColliderId.CompareTo(right.ColliderId);
            }
        }

        private readonly struct CameraProfileValues
        {
            public CameraProfileValues(
                float targetHeight,
                float distance,
                float shoulderOffset,
                float shoulderHeight,
                float verticalArmLength)
            {
                TargetHeight = targetHeight;
                Distance = distance;
                ShoulderOffset = shoulderOffset;
                ShoulderHeight = shoulderHeight;
                VerticalArmLength = verticalArmLength;
            }

            public float TargetHeight { get; }
            public float Distance { get; }
            public float ShoulderOffset { get; }
            public float ShoulderHeight { get; }
            public float VerticalArmLength { get; }
        }

        private readonly struct SpatialTuple
        {
            public SpatialTuple(
                float gameplayWeight,
                float shoulderScale,
                float boomDistance)
            {
                GameplayWeight = Mathf.Clamp01(gameplayWeight);
                ShoulderScale = Mathf.Clamp01(shoulderScale);
                BoomDistance = Mathf.Max(0f, boomDistance);
            }

            public float GameplayWeight { get; }
            public float ShoulderScale { get; }
            public float BoomDistance { get; }

            public static SpatialTuple Lerp(
                SpatialTuple from,
                SpatialTuple to,
                float amount)
            {
                float clamped = Mathf.Clamp01(amount);
                return new SpatialTuple(
                    Mathf.Lerp(from.GameplayWeight, to.GameplayWeight, clamped),
                    Mathf.Lerp(from.ShoulderScale, to.ShoulderScale, clamped),
                    Mathf.Lerp(from.BoomDistance, to.BoomDistance, clamped));
            }

            public static bool Approximately(SpatialTuple left, SpatialTuple right)
            {
                return Mathf.Abs(left.GameplayWeight - right.GameplayWeight) <=
                        ScalarTolerance &&
                    Mathf.Abs(left.ShoulderScale - right.ShoulderScale) <=
                        ScalarTolerance &&
                    Mathf.Abs(left.BoomDistance - right.BoomDistance) <= BoomTolerance;
            }
        }

        private readonly struct ConstraintFailure
        {
            public ConstraintFailure(
                ShowcaseCameraNoValidPoseReason reason,
                Collider blocker,
                Transform actorRoot,
                GeometrySignature geometrySignature = default,
                bool querySaturated = false)
            {
                Reason = reason;
                Blocker = blocker;
                ActorRoot = actorRoot;
                GeometrySignature = geometrySignature;
                QuerySaturated = querySaturated;
            }

            public ShowcaseCameraNoValidPoseReason Reason { get; }
            public Collider Blocker { get; }
            public Transform ActorRoot { get; }
            public GeometrySignature GeometrySignature { get; }
            public bool QuerySaturated { get; }

            public static ConstraintFailure Prefer(
                ConstraintFailure first,
                ConstraintFailure second)
            {
                if (first.Reason == ShowcaseCameraNoValidPoseReason.None)
                {
                    return second;
                }
                if (first.Blocker == null && second.Blocker != null)
                {
                    return second;
                }
                if (first.ActorRoot == null && second.ActorRoot != null)
                {
                    return second;
                }
                if (first.GeometrySignature.IsEmpty &&
                    !second.GeometrySignature.IsEmpty)
                {
                    return second;
                }
                if (!first.QuerySaturated && second.QuerySaturated)
                {
                    return second;
                }
                return first;
            }
        }

        private struct CameraCandidate
        {
            public Vector3 Root;
            public Vector3 Shoulder;
            public Vector3 Hand;
            public Vector3 Position;
            public float Distance;
            public float ShoulderScale;
            public float GameplayWeight;
            public Collider Blocker;
            public bool WasBoomCompressed;
        }

        private readonly struct ActorProxy
        {
            public ActorProxy(
                Transform root,
                CharacterController controller,
                CapsuleCollider capsule)
            {
                Root = root;
                Controller = controller;
                Capsule = capsule;
            }

            public Transform Root { get; }
            private CharacterController Controller { get; }
            private CapsuleCollider Capsule { get; }
            public bool IsValid => Root != null && (Controller != null || Capsule != null);

            public void GetCapsule(
                out Vector3 bottom,
                out Vector3 top,
                out float radius)
            {
                if (Controller != null)
                {
                    GetCharacterControllerCapsule(Controller, out bottom, out top, out radius);
                    return;
                }

                Vector3 scale = Capsule.transform.lossyScale;
                float verticalScale = Mathf.Abs(scale.y);
                float radialScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
                radius = Capsule.radius * radialScale;
                float height = Mathf.Max(radius * 2f, Capsule.height * verticalScale);
                Vector3 center = Capsule.transform.TransformPoint(Capsule.center);
                float halfSegment = Mathf.Max(0f, height * 0.5f - radius);
                bottom = center - Vector3.up * halfSegment;
                top = center + Vector3.up * halfSegment;
            }
        }
    }
}
