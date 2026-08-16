using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Input;
using BreakNotice.Gameplay.Interaction;
using Unity.Cinemachine;
using UnityEngine;

namespace BreakNotice.Presentation
{
    public enum CameraCoreMode
    {
        Normal,
        Sprint,
        Aim
    }

    public enum FinalCameraProjectionMode
    {
        Perspective,
        Orthographic
    }

    public enum CameraCarryMode
    {
        None,
        Light,
        Medium
    }

    public readonly struct FinalCameraSnapshot
    {
        public FinalCameraSnapshot(
            Pose cameraPose,
            Vector3 actualCameraTransformPosition,
            Ray centerViewRay,
            float verticalFieldOfView,
            float aspect,
            float nearClipPlane,
            float farClipPlane,
            FinalCameraProjectionMode projectionMode,
            int frameId,
            bool isValid)
        {
            CameraPose = cameraPose;
            ActualCameraTransformPosition = actualCameraTransformPosition;
            CenterViewRay = centerViewRay;
            VerticalFieldOfView = verticalFieldOfView;
            Aspect = aspect;
            NearClipPlane = nearClipPlane;
            FarClipPlane = farClipPlane;
            ProjectionMode = projectionMode;
            FrameId = frameId;
            IsValid = isValid;
        }

        public Pose CameraPose { get; }

        public Vector3 ActualCameraTransformPosition { get; }

        public Ray CenterViewRay { get; }

        public float VerticalFieldOfView { get; }

        public float Aspect { get; }

        public float NearClipPlane { get; }

        public float FarClipPlane { get; }

        public FinalCameraProjectionMode ProjectionMode { get; }

        public int FrameId { get; }

        public bool IsValid { get; }

        public static FinalCameraSnapshot Invalid(int frameId)
        {
            return new FinalCameraSnapshot(
                new Pose(Vector3.zero, Quaternion.identity),
                Vector3.zero,
                new Ray(Vector3.zero, Vector3.forward),
                0f,
                0f,
                0f,
                0f,
                FinalCameraProjectionMode.Perspective,
                frameId,
                false);
        }
    }

    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public sealed class ThirdPersonCameraRig : MonoBehaviour
    {
        private const float MinimumTransitionDuration = 0.01f;

        [SerializeField]
        private Transform playerTarget;

        [SerializeField]
        private InputSystemLogicalInputAdapter logicalInput;

        [SerializeField]
        private Transform orbitTarget;

        [SerializeField]
        private Camera outputCamera;

        [SerializeField]
        private CinemachineCamera virtualCamera;

        [SerializeField]
        private CinemachineThirdPersonFollow thirdPersonFollow;

        [SerializeField]
        private ShowcaseCameraConstraintSolver constraintSolver;

        [SerializeField]
        private ShowcasePlayerCombatController playerCombat;

        [SerializeField]
        private ThirdPersonLocomotion locomotion;

        [SerializeField]
        private PrototypeInteractionStateOwner interactionOwner;

        [SerializeField]
        private PrototypeInteractionController interactionController;

        [SerializeField]
        private PrototypePhysicalObjectLifecycle physicalObjectLifecycle;

        [Header("Stable Root Anchor")]
        [SerializeField]
        private float targetHeight = 0.16f;

        [SerializeField]
        private float sprintTargetHeight = 0.16f;

        [SerializeField]
        private float aimTargetHeight = 0.25f;

        [SerializeField]
        private float neutralPitch = 3f;

        [Header("Immediate Look Input")]
        [SerializeField]
        private float lookSensitivity = 0.25f;

        [SerializeField, Range(0.1f, 1f)]
        private float aimLookSpeedMultiplier = 0.78f;

        [SerializeField]
        private float minimumPitch = -40f;

        [SerializeField]
        private float maximumPitch = 55f;

        [Header("Leon Core - Normal")]
        [SerializeField]
        private float normalDistance = 2.55f;

        [SerializeField]
        private float normalShoulderOffset = 0.85f;

        [SerializeField]
        private float normalShoulderHeight;

        [SerializeField]
        private float normalVerticalArmLength = 0.1f;

        [SerializeField]
        private float normalFieldOfView = 50f;

        [SerializeField]
        private Vector3 normalDamping = new Vector3(0.06f, 0.1f, 0.08f);

        [Header("Leon Core - Sprint")]
        [SerializeField]
        private float sprintDistance = 2.7f;

        [SerializeField]
        private float sprintShoulderOffset = 0.85f;

        [SerializeField]
        private float sprintShoulderHeight;

        [SerializeField]
        private float sprintVerticalArmLength = 0.1f;

        [SerializeField]
        private float sprintFieldOfView = 50f;

        [SerializeField]
        private Vector3 sprintDamping = new Vector3(0.08f, 0.12f, 0.1f);

        [Header("Leon Core - Aim")]
        [SerializeField]
        private float aimDistance = 1.75f;

        [SerializeField]
        private float aimShoulderOffset = 0.75f;

        [SerializeField]
        private float aimShoulderHeight;

        [SerializeField]
        private float aimVerticalArmLength = 0.08f;

        [SerializeField]
        private float aimFieldOfView = 48f;

        [SerializeField]
        private Vector3 aimDamping = new Vector3(0.01f, 0.03f, 0.02f);

        [Header("Break Notice - Carry Light Override")]
        [SerializeField]
        private float carryLightTargetHeightOffset = 0.04f;

        [SerializeField]
        private float carryLightDistanceOffset = 0.15f;

        [SerializeField]
        private float carryLightShoulderOffset = 0.12f;

        [SerializeField]
        private float carryLightVerticalArmOffset = 0.02f;

        [Header("Break Notice - Carry Medium Override")]
        [SerializeField]
        private float carryMediumTargetHeightOffset = 0.12f;

        [SerializeField]
        private float carryMediumDistanceOffset = 0.45f;

        [SerializeField]
        private float carryMediumShoulderOffset = 0.32f;

        [SerializeField]
        private float carryMediumVerticalArmOffset = 0.06f;

        [Header("Break Notice - Throw Windup Override")]
        [SerializeField]
        private float throwWindupTargetHeightOffset = 0.05f;

        [SerializeField]
        private float throwWindupDistanceOffset = 0.18f;

        [SerializeField]
        private float throwWindupShoulderOffset = 0.12f;

        [SerializeField]
        private float throwWindupVerticalArmOffset = 0.03f;

        [Header("Core Transitions")]
        [SerializeField, Min(MinimumTransitionDuration)]
        private float aimTransitionDuration = 0.18f;

        [SerializeField, Min(MinimumTransitionDuration)]
        private float sprintTransitionDuration = 0.2f;

        [SerializeField, Min(MinimumTransitionDuration)]
        private float carryTransitionDuration = 0.2f;

        [SerializeField, Min(MinimumTransitionDuration)]
        private float throwWindupTransitionDuration = 0.12f;

        [Header("Collision And Lens")]
        [SerializeField]
        private float cameraCollisionRadius = 0.25f;

        [SerializeField, Range(0.2f, 0.3f)]
        private float collisionReturnDamping = 0.25f;

        [SerializeField, Min(0.01f)]
        private float collisionClearanceMargin = 0.05f;

        [SerializeField]
        private float nearClipPlane = 0.1f;

        [SerializeField]
        private float farClipPlane = 500f;

        [SerializeField, Min(0.5f)]
        private float forcedTeleportResetDistance = 1f;

        private float yaw;
        private float pitch;
        private float aimBlend;
        private float sprintBlend;
        private CameraCoreMode requestedMode;
        private CameraCarryMode requestedCarryMode;
        private bool requestedThrowWindup;
        private CameraPresentationProfile evaluatedProfile;
        private CameraPresentationProfile transitionSource;
        private CameraPresentationProfile transitionTarget;
        private Vector4 evaluatedGameplayOffset;
        private Vector4 transitionSourceGameplayOffset;
        private Vector4 transitionTargetGameplayOffset;
        private float transitionSourceAimBlend;
        private float transitionTargetAimBlend;
        private float transitionSourceSprintBlend;
        private float transitionTargetSprintBlend;
        private float transitionSourceCarryBlend;
        private float transitionTargetCarryBlend;
        private float transitionSourceThrowWindupBlend;
        private float transitionTargetThrowWindupBlend;
        private float transitionElapsed;
        private float transitionDuration;
        private bool profileInitialized;
        private bool resetWasRequested;
        private bool systemGateActive;
        private bool hasLastPlayerPosition;
        private int cameraResetRevision;
        private Vector3 lastPlayerPosition;
        private FinalCameraSnapshot finalSnapshot;
        private CameraPresentationProfile activePresentationProfile;
        private CharacterController playerController;
        private bool subjectSafetyCorrectionApplied;
#if UNITY_WEBGL && !UNITY_EDITOR
        private bool pointerWasLocked;
#endif

        public Camera OutputCamera => outputCamera;

        public Transform StableAnchor => orbitTarget;

        public Transform GameplayMovementReference => orbitTarget;

        public float Yaw => yaw;

        public float Pitch => pitch;

        public bool IsAiming { get; private set; }

        public CameraCoreMode CoreMode => requestedMode;

        public CameraCarryMode CarryMode => requestedCarryMode;

        public bool IsThrowWindup => requestedThrowWindup;

        public float AimBlend => aimBlend;

        public float SprintBlend => sprintBlend;

        public float CarryBlend { get; private set; }

        public float ThrowWindupBlend { get; private set; }

        public float GameplayOverrideSpatialWeight => constraintSolver != null
            ? constraintSolver.ResolvedGameplayWeight
            : 1f;

        public Collider GameplayOverrideBlockingCollider =>
            constraintSolver != null ? constraintSolver.CurrentHardBlocker : null;

        public Collider CurrentBlockingCollider => constraintSolver != null
            ? constraintSolver.CurrentHardBlocker
            : null;

        public Transform CurrentActorProxy => constraintSolver != null
            ? constraintSolver.CurrentActorProxy
            : null;

        public bool SubjectSafetyCorrectionApplied => subjectSafetyCorrectionApplied;

        public ShowcaseCameraConstraintStatus ConstraintStatus => constraintSolver != null
            ? constraintSolver.Status
            : ShowcaseCameraConstraintStatus.Uninitialized;

        public ShowcaseCameraNoValidPoseReason NoValidPoseReason => constraintSolver != null
            ? constraintSolver.NoValidPoseReason
            : ShowcaseCameraNoValidPoseReason.None;

        public float SolvedBoomDistance => constraintSolver != null
            ? constraintSolver.ResolvedBoomDistance
            : CurrentDistance;

        public float SolvedShoulderScale => constraintSolver != null
            ? constraintSolver.ResolvedShoulderScale
            : 1f;

        public float SafeBoomFraction => constraintSolver != null
            ? constraintSolver.SafeBoomFraction
            : 1f;

        public bool HardConstraintActiveThisFrame => constraintSolver != null &&
            constraintSolver.HardConstraintActiveThisFrame;

        public int ConstraintEvaluationsThisFrame => constraintSolver != null
            ? constraintSolver.EvaluationsThisFrame
            : 0;

        public int PostBrainTransformWriteCount => constraintSolver != null
            ? constraintSolver.PostBrainTransformWriteCount
            : 0;

        public ShowcaseContextualCornerMode ContextualCornerMode =>
            constraintSolver != null
                ? constraintSolver.ContextualCornerMode
                : ShowcaseContextualCornerMode.RightDefault;

        public float ContextualCornerBlendProgress =>
            constraintSolver != null
                ? constraintSolver.ContextualCornerBlendProgress
                : 1f;

        public float ContextualCornerShoulderFactor =>
            constraintSolver != null
                ? constraintSolver.ContextualCornerShoulderFactor
                : ShowcaseContextualCornerAimResolver.RightDefaultShoulderFactor;

        public int ContextualCornerActiveBlockerId => constraintSolver != null
            ? constraintSolver.ContextualCornerActiveBlockerId
            : 0;

        public int ContextualCornerTransitionCount => constraintSolver != null
            ? constraintSolver.ContextualCornerTransitionCount
            : 0;

        public bool IsTransitioning => transitionElapsed < transitionDuration;

        public float CurrentDistance => thirdPersonFollow != null
            ? thirdPersonFollow.CameraDistance
            : 0f;

        public Vector3 CurrentShoulderOffset => thirdPersonFollow != null
            ? thirdPersonFollow.ShoulderOffset
            : Vector3.zero;

        public float CurrentVerticalArmLength => thirdPersonFollow != null
            ? thirdPersonFollow.VerticalArmLength
            : 0f;

        public float CurrentFieldOfView => virtualCamera != null
            ? virtualCamera.Lens.FieldOfView
            : 0f;

        public FinalCameraSnapshot CurrentSnapshot =>
            finalSnapshot.IsValid && finalSnapshot.FrameId == Time.frameCount
                ? finalSnapshot
                : FinalCameraSnapshot.Invalid(Time.frameCount);

        private void Awake()
        {
            ResolveDependencies();
            EnsureConstraintSolver();
            InitializeProfile(CameraCoreMode.Normal);
            ApplyPipelineConfiguration();
        }

        private void OnEnable()
        {
            CinemachineCore.CameraUpdatedEvent.AddListener(OnCinemachineCameraUpdated);
            ResolveDependencies();
            EnsureConstraintSolver();
            if (!profileInitialized)
            {
                InitializeProfile(CameraCoreMode.Normal);
            }
            ApplyPipelineConfiguration();
        }

        private void OnDisable()
        {
            CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCinemachineCameraUpdated);
            InvalidateSnapshot();
        }

        private void Update()
        {
            if (playerTarget == null || orbitTarget == null)
            {
                systemGateActive = true;
                InvalidateSnapshot();
                return;
            }

            bool resetRequested =
                logicalInput != null && logicalInput.Snapshot.ResetEncounter;
            systemGateActive = resetRequested;
            if (resetRequested && !resetWasRequested)
            {
                HardResetCameraState();
            }
            resetWasRequested = resetRequested;

            Vector2 look = logicalInput != null ? logicalInput.Snapshot.Look : Vector2.zero;
#if UNITY_WEBGL && !UNITY_EDITOR
            bool pointerCaptured =
                logicalInput != null && logicalInput.IsWebPointerGameplayActive;
            bool pointerJustCaptured = pointerCaptured && !pointerWasLocked;
            pointerWasLocked = pointerCaptured;
            if (logicalInput != null && logicalInput.IsWebPointerFallbackActive)
            {
                look = logicalInput.WebPointerFallbackLookDelta;
            }

            if (!pointerCaptured || pointerJustCaptured)
            {
                look = Vector2.zero;
            }
#endif
            IsAiming = !systemGateActive &&
                logicalInput != null &&
                logicalInput.Snapshot.Aim &&
                (playerCombat == null || playerCombat.AllowsAim);
#if UNITY_WEBGL && !UNITY_EDITOR
            IsAiming &= pointerCaptured && !pointerJustCaptured;
#endif
            float lookMultiplier = IsAiming ? aimLookSpeedMultiplier : 1f;
            yaw += look.x * lookSensitivity * lookMultiplier;
            pitch = Mathf.Clamp(
                pitch - look.y * lookSensitivity * lookMultiplier,
                minimumPitch,
                maximumPitch);
            ApplyGameplayHeading();
        }

        private void LateUpdate()
        {
            if (playerTarget == null || orbitTarget == null)
            {
                return;
            }

            if (hasLastPlayerPosition &&
                Vector3.Distance(playerTarget.position, lastPlayerPosition) >=
                forcedTeleportResetDistance)
            {
                HardResetCameraState();
            }

            CameraCoreMode desiredMode = ResolveDesiredMode();
            ResolveDesiredGameplayOverride(
                out CameraCarryMode desiredCarryMode,
                out bool desiredThrowWindup);
            if (desiredMode != requestedMode ||
                desiredCarryMode != requestedCarryMode ||
                desiredThrowWindup != requestedThrowWindup)
            {
                BeginTransition(desiredMode, desiredCarryMode, desiredThrowWindup);
            }

            AdvanceTransition(Time.unscaledDeltaTime);
            activePresentationProfile = evaluatedProfile.WithOffsets(
                evaluatedGameplayOffset.x,
                evaluatedGameplayOffset.y,
                evaluatedGameplayOffset.z,
                evaluatedGameplayOffset.w);
            ApplyStableAnchorPose(activePresentationProfile);
            ApplyPresentation(activePresentationProfile);
            UpdateConstraintFrameInput();

            lastPlayerPosition = playerTarget.position;
            hasLastPlayerPosition = true;
        }

        public void Configure(Transform target, InputSystemLogicalInputAdapter input)
        {
            bool targetChanged = playerTarget != target;
            playerTarget = target;
            logicalInput = input;
            if (targetChanged)
            {
                playerCombat = null;
                locomotion = null;
                interactionOwner = null;
                interactionController = null;
                physicalObjectLifecycle = null;
                playerController = null;
            }
            ResolveDependencies();
            EnsureConstraintSolver();
            HardResetCameraState();
            ApplyPipelineConfiguration();
        }

        public bool TryGetFinalCameraSnapshot(out FinalCameraSnapshot snapshot)
        {
            snapshot = CurrentSnapshot;
            return snapshot.IsValid;
        }

        public void HardResetCameraState()
        {
            unchecked
            {
                cameraResetRevision++;
            }
            if (playerTarget != null)
            {
                yaw = playerTarget.eulerAngles.y;
            }
            pitch = Mathf.Clamp(neutralPitch, minimumPitch, maximumPitch);
            InitializeProfile(CameraCoreMode.Normal);
            subjectSafetyCorrectionApplied = false;
            activePresentationProfile = evaluatedProfile;
            ApplyStableAnchorPose(activePresentationProfile);
            ApplyPresentation(activePresentationProfile);
            EnsureConstraintSolver();
            constraintSolver?.ResetSolverState();
            UpdateConstraintFrameInput();
            hasLastPlayerPosition = playerTarget != null;
            if (hasLastPlayerPosition)
            {
                lastPlayerPosition = playerTarget.position;
            }

            if (virtualCamera != null)
            {
                virtualCamera.PreviousStateIsValid = false;
            }
            if (outputCamera != null &&
                outputCamera.TryGetComponent(out CinemachineBrain brain))
            {
                brain.ResetState();
            }
            InvalidateSnapshot();
        }

        private CameraCoreMode ResolveDesiredMode()
        {
            if (IsAiming)
            {
                return CameraCoreMode.Aim;
            }
            return locomotion != null && locomotion.IsSprinting
                ? CameraCoreMode.Sprint
                : CameraCoreMode.Normal;
        }

        private void ResolveDesiredGameplayOverride(
            out CameraCarryMode carryMode,
            out bool throwWindup)
        {
            carryMode = CameraCarryMode.None;
            throwWindup = false;
            if (systemGateActive || physicalObjectLifecycle == null)
            {
                return;
            }

            PrototypeInteractionTarget target =
                physicalObjectLifecycle.CarriedTarget;

            if (target == null)
            {
                return;
            }

            carryMode = target.WeightClass == PrototypeWeightClass.Light
                ? CameraCarryMode.Light
                : target.WeightClass == PrototypeWeightClass.Medium
                    ? CameraCarryMode.Medium
                    : CameraCarryMode.None;
            if (carryMode == CameraCarryMode.None)
            {
                return;
            }

            PrototypeThrowPresentationSnapshot presentation =
                interactionController != null
                    ? interactionController.ThrowPresentation
                    : default;
            throwWindup = presentation.IsValid &&
                presentation.Target == target &&
                (presentation.Phase ==
                    PrototypeThrowPresentationPhase.ChargingDirectCarry ||
                 presentation.Phase ==
                    PrototypeThrowPresentationPhase.ReleaseAccepted);
        }

        private void BeginTransition(
            CameraCoreMode desiredMode,
            CameraCarryMode desiredCarryMode,
            bool desiredThrowWindup)
        {
            transitionSource = evaluatedProfile;
            transitionTarget = GetCoreProfile(desiredMode);
            transitionSourceGameplayOffset = evaluatedGameplayOffset;
            evaluatedGameplayOffset = transitionSourceGameplayOffset;
            transitionTargetGameplayOffset = GetGameplayOffsets(
                desiredCarryMode,
                desiredThrowWindup,
                1f,
                1f);
            transitionSourceAimBlend = aimBlend;
            transitionTargetAimBlend = desiredMode == CameraCoreMode.Aim ? 1f : 0f;
            transitionSourceSprintBlend = sprintBlend;
            transitionTargetSprintBlend = desiredMode == CameraCoreMode.Sprint ? 1f : 0f;
            transitionSourceCarryBlend = CarryBlend;
            transitionTargetCarryBlend = desiredCarryMode == CameraCarryMode.None ? 0f : 1f;
            transitionSourceThrowWindupBlend = ThrowWindupBlend;
            transitionTargetThrowWindupBlend = desiredThrowWindup ? 1f : 0f;
            transitionElapsed = 0f;
            transitionDuration = ResolveTransitionDuration(
                requestedMode,
                desiredMode,
                requestedCarryMode,
                desiredCarryMode,
                requestedThrowWindup,
                desiredThrowWindup);
            requestedMode = desiredMode;
            requestedCarryMode = desiredCarryMode;
            requestedThrowWindup = desiredThrowWindup;
        }

        private void AdvanceTransition(float unscaledDeltaTime)
        {
            if (transitionDuration <= 0f || transitionElapsed >= transitionDuration)
            {
                evaluatedProfile = transitionTarget;
                evaluatedGameplayOffset = transitionTargetGameplayOffset;
                aimBlend = transitionTargetAimBlend;
                sprintBlend = transitionTargetSprintBlend;
                CarryBlend = transitionTargetCarryBlend;
                ThrowWindupBlend = transitionTargetThrowWindupBlend;
                transitionElapsed = transitionDuration;
                return;
            }

            transitionElapsed = Mathf.Min(
                transitionDuration,
                transitionElapsed + Mathf.Max(0f, unscaledDeltaTime));
            float normalized = Mathf.Clamp01(transitionElapsed / transitionDuration);
            float curved = normalized * normalized * (3f - 2f * normalized);
            evaluatedProfile = CameraPresentationProfile.Lerp(
                transitionSource,
                transitionTarget,
                curved);
            evaluatedGameplayOffset = Vector4.Lerp(
                transitionSourceGameplayOffset,
                transitionTargetGameplayOffset,
                curved);
            aimBlend = Mathf.Lerp(
                transitionSourceAimBlend,
                transitionTargetAimBlend,
                curved);
            sprintBlend = Mathf.Lerp(
                transitionSourceSprintBlend,
                transitionTargetSprintBlend,
                curved);
            CarryBlend = Mathf.Lerp(
                transitionSourceCarryBlend,
                transitionTargetCarryBlend,
                curved);
            ThrowWindupBlend = Mathf.Lerp(
                transitionSourceThrowWindupBlend,
                transitionTargetThrowWindupBlend,
                curved);
        }

        private float ResolveTransitionDuration(
            CameraCoreMode from,
            CameraCoreMode to,
            CameraCarryMode fromCarry,
            CameraCarryMode toCarry,
            bool fromThrowWindup,
            bool toThrowWindup)
        {
            if (fromThrowWindup != toThrowWindup)
            {
                return Mathf.Max(
                    MinimumTransitionDuration,
                    throwWindupTransitionDuration);
            }
            if (fromCarry != toCarry)
            {
                return Mathf.Max(MinimumTransitionDuration, carryTransitionDuration);
            }
            if (from == CameraCoreMode.Aim || to == CameraCoreMode.Aim)
            {
                return Mathf.Max(MinimumTransitionDuration, aimTransitionDuration);
            }
            return Mathf.Max(MinimumTransitionDuration, sprintTransitionDuration);
        }

        private void InitializeProfile(CameraCoreMode mode)
        {
            requestedMode = mode;
            requestedCarryMode = CameraCarryMode.None;
            requestedThrowWindup = false;
            evaluatedProfile = GetCoreProfile(mode);
            transitionSource = evaluatedProfile;
            transitionTarget = evaluatedProfile;
            evaluatedGameplayOffset = Vector4.zero;
            transitionSourceGameplayOffset = Vector4.zero;
            transitionTargetGameplayOffset = Vector4.zero;
            transitionElapsed = 0f;
            transitionDuration = 0f;
            aimBlend = mode == CameraCoreMode.Aim ? 1f : 0f;
            sprintBlend = mode == CameraCoreMode.Sprint ? 1f : 0f;
            transitionSourceAimBlend = aimBlend;
            transitionTargetAimBlend = aimBlend;
            transitionSourceSprintBlend = sprintBlend;
            transitionTargetSprintBlend = sprintBlend;
            CarryBlend = 0f;
            ThrowWindupBlend = 0f;
            transitionSourceCarryBlend = 0f;
            transitionTargetCarryBlend = 0f;
            transitionSourceThrowWindupBlend = 0f;
            transitionTargetThrowWindupBlend = 0f;
            activePresentationProfile = evaluatedProfile;
            profileInitialized = true;
        }

        private CameraPresentationProfile GetCoreProfile(CameraCoreMode mode)
        {
            switch (mode)
            {
                case CameraCoreMode.Sprint:
                    return new CameraPresentationProfile(
                        sprintTargetHeight,
                        sprintDistance,
                        sprintShoulderOffset,
                        sprintShoulderHeight,
                        sprintVerticalArmLength,
                        sprintFieldOfView,
                        sprintDamping);
                case CameraCoreMode.Aim:
                    return new CameraPresentationProfile(
                        aimTargetHeight,
                        aimDistance,
                        aimShoulderOffset,
                        aimShoulderHeight,
                        aimVerticalArmLength,
                        aimFieldOfView,
                        aimDamping);
                default:
                    return new CameraPresentationProfile(
                        targetHeight,
                        normalDistance,
                        normalShoulderOffset,
                        normalShoulderHeight,
                        normalVerticalArmLength,
                        normalFieldOfView,
                        normalDamping);
            }
        }

        private CameraPresentationProfile GetComposedProfile(
            CameraCoreMode mode,
            CameraCarryMode carryMode,
            bool throwWindup)
        {
            CameraPresentationProfile profile = GetCoreProfile(mode);
            if (carryMode == CameraCarryMode.Light)
            {
                profile = profile.WithOffsets(
                    carryLightTargetHeightOffset,
                    carryLightDistanceOffset,
                    carryLightShoulderOffset,
                    carryLightVerticalArmOffset);
            }
            else if (carryMode == CameraCarryMode.Medium)
            {
                profile = profile.WithOffsets(
                    carryMediumTargetHeightOffset,
                    carryMediumDistanceOffset,
                    carryMediumShoulderOffset,
                    carryMediumVerticalArmOffset);
            }

            if (throwWindup)
            {
                profile = profile.WithOffsets(
                    throwWindupTargetHeightOffset,
                    throwWindupDistanceOffset,
                    throwWindupShoulderOffset,
                    throwWindupVerticalArmOffset);
            }
            return profile;
        }

        private Vector4 GetGameplayOffsets(
            CameraCarryMode carryMode,
            bool throwWindup,
            float carryWeight,
            float throwWeight)
        {
            Vector4 offsets = Vector4.zero;
            float boundedCarryWeight = Mathf.Clamp01(carryWeight);
            if (carryMode == CameraCarryMode.Light)
            {
                offsets += new Vector4(
                    carryLightTargetHeightOffset,
                    carryLightDistanceOffset,
                    carryLightShoulderOffset,
                    carryLightVerticalArmOffset) * boundedCarryWeight;
            }
            else if (carryMode == CameraCarryMode.Medium)
            {
                offsets += new Vector4(
                    carryMediumTargetHeightOffset,
                    carryMediumDistanceOffset,
                    carryMediumShoulderOffset,
                    carryMediumVerticalArmOffset) * boundedCarryWeight;
            }

            if (throwWindup)
            {
                offsets += new Vector4(
                    throwWindupTargetHeightOffset,
                    throwWindupDistanceOffset,
                    throwWindupShoulderOffset,
                    throwWindupVerticalArmOffset) * Mathf.Clamp01(throwWeight);
            }
            return offsets;
        }

        private void ResolveDependencies()
        {
            if (playerTarget == null)
            {
                return;
            }
            if (playerCombat == null)
            {
                playerCombat = playerTarget.GetComponent<ShowcasePlayerCombatController>();
            }
            if (locomotion == null)
            {
                locomotion = playerTarget.GetComponent<ThirdPersonLocomotion>();
            }
            if (locomotion != null && orbitTarget != null)
            {
                // Gameplay movement follows the stable yaw/pitch authority, never
                // a collision-compressed output Camera transform.
                locomotion.ConfigureCamera(orbitTarget);
            }
            if (interactionOwner == null)
            {
                interactionOwner =
                    playerTarget.GetComponent<PrototypeInteractionStateOwner>();
            }
            if (interactionController == null)
            {
                interactionController =
                    playerTarget.GetComponent<PrototypeInteractionController>();
            }
            if (physicalObjectLifecycle == null)
            {
                physicalObjectLifecycle =
                    playerTarget.GetComponent<PrototypePhysicalObjectLifecycle>();
            }
            if (playerController == null)
            {
                playerController = playerTarget.GetComponent<CharacterController>();
            }
        }

        private void EnsureConstraintSolver()
        {
            if (virtualCamera == null)
            {
                return;
            }
            if (constraintSolver == null)
            {
                constraintSolver =
                    virtualCamera.GetComponent<ShowcaseCameraConstraintSolver>();
            }
            if (constraintSolver == null && Application.isPlaying)
            {
                constraintSolver =
                    virtualCamera.gameObject.AddComponent<ShowcaseCameraConstraintSolver>();
            }
        }

        private void UpdateConstraintFrameInput()
        {
            if (constraintSolver == null)
            {
                return;
            }
            PrototypeInteractionTarget carriedTarget =
                physicalObjectLifecycle != null
                    ? physicalObjectLifecycle.CarriedTarget
                    : null;
            PrototypeThrowPresentationSnapshot throwPresentation =
                interactionController != null
                    ? interactionController.ThrowPresentation
                    : default;
            bool directCarryThrowWindup = requestedThrowWindup &&
                throwPresentation.IsValid &&
                throwPresentation.Target == carriedTarget &&
                (throwPresentation.Phase ==
                    PrototypeThrowPresentationPhase.ChargingDirectCarry ||
                 throwPresentation.Phase ==
                    PrototypeThrowPresentationPhase.ReleaseAccepted);
            LensSettings lens = virtualCamera != null
                ? virtualCamera.Lens
                : default;
            float aspect = outputCamera != null && outputCamera.aspect > 0f
                ? outputCamera.aspect
                : 16f / 9f;
            int blockerId = constraintSolver.CurrentHardBlocker != null
                ? constraintSolver.CurrentHardBlocker.GetInstanceID()
                : 0;
            ShowcaseContextualCornerFrameSnapshot cornerSnapshot =
                new ShowcaseContextualCornerFrameSnapshot(
                    Time.frameCount,
                    Time.unscaledDeltaTime,
                    cameraResetRevision,
                    yaw,
                    pitch,
                    requestedMode == CameraCoreMode.Aim,
                    directCarryThrowWindup,
                    requestedCarryMode,
                    throwPresentation.Phase,
                    carriedTarget != null ? carriedTarget.GetInstanceID() : 0,
                    playerTarget != null
                        ? new Pose(playerTarget.position, playerTarget.rotation)
                        : new Pose(Vector3.zero, Quaternion.identity),
                    orbitTarget != null
                        ? new Pose(orbitTarget.position, orbitTarget.rotation)
                        : new Pose(Vector3.zero, Quaternion.identity),
                    evaluatedProfile.TargetHeight,
                    evaluatedProfile.Distance,
                    evaluatedProfile.ShoulderOffset,
                    evaluatedProfile.ShoulderHeight,
                    evaluatedProfile.VerticalArmLength,
                    lens.FieldOfView,
                    aspect,
                    lens.NearClipPlane,
                    constraintSolver.ContextualCornerMode,
                    constraintSolver.ContextualCornerBlendProgress,
                    blockerId,
                    0f);
            constraintSolver.SetFrameInput(new ShowcaseCameraConstraintFrame(
                playerTarget,
                playerController,
                orbitTarget,
                carriedTarget,
                evaluatedProfile.TargetHeight,
                evaluatedProfile.Distance,
                evaluatedProfile.ShoulderOffset,
                evaluatedProfile.ShoulderHeight,
                evaluatedProfile.VerticalArmLength,
                evaluatedGameplayOffset,
                yaw,
                pitch,
                cameraCollisionRadius,
                collisionClearanceMargin,
                collisionReturnDamping,
                cornerSnapshot));
        }

        private void ApplyPipelineConfiguration()
        {
            if (virtualCamera != null && orbitTarget != null)
            {
                virtualCamera.Follow = orbitTarget;
                virtualCamera.LookAt = null;
                LensSettings lens = virtualCamera.Lens;
                lens.ModeOverride = LensSettings.OverrideModes.Perspective;
                lens.NearClipPlane = Mathf.Max(0.01f, nearClipPlane);
                lens.FarClipPlane = Mathf.Max(lens.NearClipPlane + 1f, farClipPlane);
                lens.Dutch = 0f;
                lens.PhysicalProperties.LensShift = Vector2.zero;
                virtualCamera.Lens = lens;
            }

            if (outputCamera != null)
            {
                outputCamera.orthographic = false;
                outputCamera.usePhysicalProperties = false;
                outputCamera.lensShift = Vector2.zero;
            }

            if (thirdPersonFollow != null)
            {
                CinemachineThirdPersonFollow.ObstacleSettings obstacleSettings =
                    thirdPersonFollow.AvoidObstacles;
                obstacleSettings.Enabled = false;
                obstacleSettings.CollisionFilter = 0;
                obstacleSettings.IgnoreTag = string.Empty;
                obstacleSettings.CameraRadius = cameraCollisionRadius;
                obstacleSettings.DampingIntoCollision = 0f;
                obstacleSettings.DampingFromCollision = 0f;
                thirdPersonFollow.AvoidObstacles = obstacleSettings;
                thirdPersonFollow.CameraSide = 1f;
            }

            EnsureConstraintSolver();
            ApplyStableAnchorPose(activePresentationProfile);
            ApplyPresentation(activePresentationProfile);
            UpdateConstraintFrameInput();
        }

        private void ApplyStableAnchorPose(CameraPresentationProfile profile)
        {
            if (playerTarget == null || orbitTarget == null)
            {
                return;
            }
            orbitTarget.position =
                playerTarget.position + Vector3.up * profile.TargetHeight;
            orbitTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private void ApplyGameplayHeading()
        {
            if (orbitTarget != null)
            {
                orbitTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);
            }
        }

        private void ApplyPresentation(CameraPresentationProfile profile)
        {
            if (thirdPersonFollow != null)
            {
                thirdPersonFollow.CameraDistance = profile.Distance;
                thirdPersonFollow.ShoulderOffset = new Vector3(
                    profile.ShoulderOffset,
                    profile.ShoulderHeight,
                    0f);
                thirdPersonFollow.VerticalArmLength = profile.VerticalArmLength;
                thirdPersonFollow.CameraSide = 1f;
                thirdPersonFollow.Damping = profile.Damping;
            }

            if (virtualCamera != null)
            {
                LensSettings lens = virtualCamera.Lens;
                lens.FieldOfView = Mathf.Clamp(profile.FieldOfView, 38f, 55f);
                lens.ModeOverride = LensSettings.OverrideModes.Perspective;
                lens.NearClipPlane = Mathf.Max(0.01f, nearClipPlane);
                lens.FarClipPlane = Mathf.Max(lens.NearClipPlane + 1f, farClipPlane);
                lens.Dutch = 0f;
                lens.PhysicalProperties.LensShift = Vector2.zero;
                virtualCamera.Lens = lens;
            }
        }

        private void OnCinemachineCameraUpdated(CinemachineBrain brain)
        {
            if (brain == null || outputCamera == null || brain.OutputCamera != outputCamera)
            {
                return;
            }

            if (systemGateActive ||
                !brain.isActiveAndEnabled ||
                !outputCamera.isActiveAndEnabled ||
                !outputCamera.gameObject.activeInHierarchy)
            {
                InvalidateSnapshot();
                return;
            }

            Transform cameraTransform = outputCamera.transform;
            subjectSafetyCorrectionApplied = constraintSolver != null &&
                constraintSolver.SubjectConstraintApplied;
            Ray centerRay = outputCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            float fieldOfView = outputCamera.fieldOfView;
            float aspect = outputCamera.aspect;
            float near = outputCamera.nearClipPlane;
            float far = outputCamera.farClipPlane;
            bool finite = IsFinite(cameraTransform.position) &&
                IsFinite(cameraTransform.rotation) &&
                IsFinite(centerRay.origin) &&
                IsFinite(centerRay.direction) &&
                IsFinite(fieldOfView) &&
                IsFinite(aspect) &&
                IsFinite(near) &&
                IsFinite(far);
            if (!finite || aspect <= 0f || near <= 0f || far <= near)
            {
                InvalidateSnapshot();
                return;
            }

            finalSnapshot = new FinalCameraSnapshot(
                new Pose(cameraTransform.position, cameraTransform.rotation),
                cameraTransform.position,
                centerRay,
                fieldOfView,
                aspect,
                near,
                far,
                outputCamera.orthographic
                    ? FinalCameraProjectionMode.Orthographic
                    : FinalCameraProjectionMode.Perspective,
                Time.frameCount,
                true);
        }

        private void InvalidateSnapshot()
        {
            finalSnapshot = FinalCameraSnapshot.Invalid(Time.frameCount);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(Quaternion value)
        {
            return IsFinite(value.x) &&
                IsFinite(value.y) &&
                IsFinite(value.z) &&
                IsFinite(value.w);
        }

        private readonly struct CameraPresentationProfile
        {
            public CameraPresentationProfile(
                float targetHeight,
                float distance,
                float shoulderOffset,
                float shoulderHeight,
                float verticalArmLength,
                float fieldOfView,
                Vector3 damping)
            {
                TargetHeight = targetHeight;
                Distance = distance;
                ShoulderOffset = shoulderOffset;
                ShoulderHeight = shoulderHeight;
                VerticalArmLength = verticalArmLength;
                FieldOfView = fieldOfView;
                Damping = damping;
            }

            public float TargetHeight { get; }

            public float Distance { get; }

            public float ShoulderOffset { get; }

            public float ShoulderHeight { get; }

            public float VerticalArmLength { get; }

            public float FieldOfView { get; }

            public Vector3 Damping { get; }

            public static CameraPresentationProfile Lerp(
                CameraPresentationProfile from,
                CameraPresentationProfile to,
                float amount)
            {
                float t = Mathf.Clamp01(amount);
                return new CameraPresentationProfile(
                    Mathf.Lerp(from.TargetHeight, to.TargetHeight, t),
                    Mathf.Lerp(from.Distance, to.Distance, t),
                    Mathf.Lerp(from.ShoulderOffset, to.ShoulderOffset, t),
                    Mathf.Lerp(from.ShoulderHeight, to.ShoulderHeight, t),
                    Mathf.Lerp(from.VerticalArmLength, to.VerticalArmLength, t),
                    Mathf.Lerp(from.FieldOfView, to.FieldOfView, t),
                    Vector3.Lerp(from.Damping, to.Damping, t));
            }

            public CameraPresentationProfile WithOffsets(
                float targetHeightOffset,
                float distanceOffset,
                float shoulderOffset,
                float verticalArmOffset)
            {
                return new CameraPresentationProfile(
                    TargetHeight + targetHeightOffset,
                    Distance + distanceOffset,
                    ShoulderOffset + shoulderOffset,
                    ShoulderHeight,
                    VerticalArmLength + verticalArmOffset,
                    FieldOfView,
                    Damping);
            }
        }
    }
}
