using System;
using BreakNotice.Gameplay;
using System.Collections.Generic;
using BreakNotice.Gameplay.Input;
using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    public enum ShowcaseCargoStrapPhase
    {
        Stored,
        Prepare,
        Rolling,
        Throw,
        Pull,
        Tensioned,
        SmallSpinning,
        AttackLeft,
        AttackRight,
        MediumExtending,
        MediumRecovering,
        Releasing
    }

    public enum ShowcaseCargoStrapAnimationCue
    {
        None,
        Prepare,
        Rolling,
        Throw,
        Pull,
        AttackLeft,
        AttackRight
    }

    public readonly struct ShowcaseCargoStrapPresentationSnapshot
    {
        public ShowcaseCargoStrapPresentationSnapshot(
            ShowcaseCargoStrapPhase phase,
            float phaseProgress01,
            PrototypeInteractionTarget activeTarget,
            Vector3 reelWorldPosition,
            Vector3 anchorWorldPosition,
            Vector3 releaseStartWorldPosition,
            Vector3 releaseEndWorldPosition,
            bool isClampReleased,
            bool isTensioned,
            bool isWarning,
            PrototypeWeaponSelection weaponSelection,
            float effectiveTetherLength = 0f,
            float tension01 = -1f,
            bool isSmallSpinObstructed = false)
        {
            Phase = phase;
            PhaseProgress01 = Mathf.Clamp01(phaseProgress01);
            ActiveTarget = activeTarget;
            ReelWorldPosition = reelWorldPosition;
            AnchorWorldPosition = anchorWorldPosition;
            ReleaseStartWorldPosition = releaseStartWorldPosition;
            ReleaseEndWorldPosition = releaseEndWorldPosition;
            IsClampReleased = isClampReleased;
            IsTensioned = isTensioned;
            IsWarning = isWarning;
            WeaponSelection = weaponSelection;
            EffectiveTetherLength = Mathf.Max(0f, effectiveTetherLength);
            HasDynamicTension = tension01 >= 0f;
            Tension01 = Mathf.Clamp01(tension01);
            IsSmallSpinObstructed = isSmallSpinObstructed;
        }

        public ShowcaseCargoStrapPhase Phase { get; }
        public float PhaseProgress01 { get; }
        public PrototypeInteractionTarget ActiveTarget { get; }
        public Vector3 ReelWorldPosition { get; }
        public Vector3 AnchorWorldPosition { get; }
        public Vector3 ReleaseStartWorldPosition { get; }
        public Vector3 ReleaseEndWorldPosition { get; }
        public bool IsClampReleased { get; }
        public bool IsTensioned { get; }
        public bool IsWarning { get; }
        public PrototypeWeaponSelection WeaponSelection { get; }
        public float EffectiveTetherLength { get; }
        public bool HasDynamicTension { get; }
        public float Tension01 { get; }
        public bool IsSmallSpinObstructed { get; }
    }

    [DefaultExecutionOrder(-40)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseCargoStrapController : MonoBehaviour
    {
        public static readonly Vector3 AuthoredCarabinerModelLocalPosition =
            new Vector3(0.13f, 0.94f, 0.14f);

        private const int VisibilityHitCapacity = 32;
        private const int SmallOrbitOverlapCapacity = 32;
        private const float AttackBufferStart01 = 0.65f;
        private const float ReleaseDuration = 0.2f;
        private const float AnimationEventTimeout = 4f;
        private const float AttackCollisionGraceSeconds = 0.4f;
        private const float SmallSpinCycleSeconds = 12f / 30f;

        [SerializeField]
        private PrototypeInteractionStateOwner interactionOwner;

        [SerializeField]
        private InputSystemLogicalInputAdapter logicalInput;

        [SerializeField]
        private Transform playerTransform;

        [SerializeField]
        private Collider playerCollider;

        [SerializeField]
        private Transform allyTransform;

        [SerializeField]
        private PrototypeInteractionTarget[] authoredTargets =
            Array.Empty<PrototypeInteractionTarget>();

        [SerializeField]
        private LineRenderer strapLine;

        [SerializeField, Min(0.5f)]
        private float assistAttachRange = 3.5f;

        [SerializeField, Min(0.5f)]
        private float maximumAllyAssistRange = 7f;

        [SerializeField, Min(0.1f)]
        private float redirectSpeed = 4.5f;

        [SerializeField, Min(1f)]
        private float maximumStrapLength = 7f;

        [SerializeField, Min(0.01f)]
        private float warningDistance = 6.2f;

        [SerializeField, Min(0.1f)]
        private float smallSwingSpeed = 7.2f;

        [SerializeField, Min(0f)]
        private float smallSwingLift = 1.4f;

        [SerializeField, Min(0.1f)]
        private float smallSpinRadius = 0.65f;

        [SerializeField, Min(0.1f)]
        private float smallSpinTrackingResponse = 14f;

        [SerializeField, Min(1f)]
        private float smallSpinMaximumAcceleration = 80f;

        [SerializeField, Min(0.1f)]
        private float smallSpinMaximumSpeed = 13f;

        [SerializeField, Min(0f)]
        private float smallSpinHeadTopOffset = 0.2f;

        [SerializeField, Min(0f)]
        private float smallSpinHeadRadius = 0.18f;

        [SerializeField, Min(0.01f)]
        private float smallSpinBodyClearance = 0.15f;

        [SerializeField, Min(0f)]
        private float smallSpinTrackingClearanceMargin = 0.04f;

        [SerializeField, Min(0.01f)]
        private float smallSpinObstaclePadding = 0.02f;

        [SerializeField, Min(0.01f)]
        private float smallSpinCenterDampingSeconds = 0.08f;

        [SerializeField, Min(0.5f)]
        private float mediumRearMinimumDistance = 1.35f;

        [SerializeField, Min(0.5f)]
        private float mediumRearMaximumDistance = 2.6f;

        [SerializeField, Min(0.1f)]
        private float mediumRearClearance = 0.35f;

        [SerializeField, Min(0.1f)]
        private float mediumRearTrackingResponse = 8f;

        [SerializeField, Min(1f)]
        private float mediumRearMaximumAcceleration = 35f;

        [SerializeField, Min(0.1f)]
        private float mediumRearMaximumSpeed = 5.5f;

        [SerializeField, Min(0.1f)]
        private float mediumSwingMinimumRange = 1.25f;

        [SerializeField, Min(0.1f)]
        private float mediumSwingMaximumRange = 5.8f;

        [SerializeField, Min(0.1f)]
        private float mediumSwingTrajectoryAllowance = 0.55f;

        [SerializeField, Min(0.1f)]
        private float mediumSwingArcHeight = 0.65f;

        [SerializeField, Min(0.1f)]
        private float mediumSwingTrackingResponse = 24f;

        [SerializeField, Min(1f)]
        private float mediumSwingMaximumAcceleration = 150f;

        [SerializeField, Min(0.1f)]
        private float mediumSwingMaximumSpeed = 22f;

        [SerializeField, Min(0.1f)]
        private float mediumSwingMaximumTrackingError = 3.5f;

        [SerializeField, Min(0.1f)]
        private float mediumTetherExtensionSpeed = 10f;

        [SerializeField, Min(0f)]
        private float mediumTargetCorrectionDistance = 0.6f;

        [SerializeField, Min(0.1f)]
        private float mediumTargetCorrectionSpeed = 2f;

        [SerializeField, Min(0.01f)]
        private float mediumRearReadyTolerance = 0.3f;

        [SerializeField, Min(0.1f)]
        private float mediumRecoveryTrackingResponse = 12f;

        [SerializeField, Min(1f)]
        private float mediumRecoveryMaximumAcceleration = 60f;

        [SerializeField, Min(0.1f)]
        private float mediumRecoveryMaximumSpeed = 8f;

        [SerializeField, Min(0.1f)]
        private float mediumRecoveryTimeout = 2.25f;

        [SerializeField, Min(0.05f)]
        private float mediumExtensionDuration = 0.65f;

        [SerializeField, Range(0f, 1f)]
        private float pullMotionStart01 = 0.22f;

        [SerializeField, Range(0f, 1f)]
        private float pullMotionEnd01 = 0.58f;

        [SerializeField, Min(0.1f)]
        private float smallPullTargetRadius = 0.72f;

        [SerializeField, Min(0.1f)]
        private float pullTrackingResponse = 18f;

        [SerializeField, Min(0.1f)]
        private float pullMaximumSpeed = 10f;

        [SerializeField, Min(1f)]
        private float pullMaximumAcceleration = 80f;

        [SerializeField, Min(0.1f)]
        private float validCollisionSpeed = 2f;

        [SerializeField, Min(1f)]
        private float anchorFacingSpeedDegrees = 420f;

        private readonly RaycastHit[] visibilityHits =
            new RaycastHit[VisibilityHitCapacity];
        private readonly Collider[] smallOrbitOverlapHits =
            new Collider[SmallOrbitOverlapCapacity];
        private PrototypeInteractionTarget activeTarget;
        private Rigidbody activeBody;
        private Collider activeCollider;
        private RigidbodyConstraints originalConstraints;
        private CollisionDetectionMode originalCollisionDetection;
        private RigidbodyInterpolation originalInterpolation;
        private bool originalWasKinematic;
        private bool originalUsedGravity;
        private bool playerCollisionIgnored;
        private ThirdPersonLocomotion locomotion;
        private PrototypePhysicalObjectLifecycle physicalLifecycle;
        private ShowcaseRunnerPressureController runnerPressure;
        private Animator humanoidAnimator;
        private Transform rightHandTransform;
        private Transform headTransform;
        private Transform hipsTransform;
        private Vector3 carabinerHipsLocalPosition;
        private PrototypeInteractionTarget pendingTarget;
        private bool automaticSequence;
        private bool pendingWasAssisted;
        private bool interactReleaseQueued;
        private bool releaseQueued;
        private bool bufferedAttack;
        private bool bufferedAttackLeft;
        private Vector3 bufferedMediumAimWorldPoint;
        private bool lastAttackWasLeft;
        private bool attackImpulseCommitted;
        private bool attackUsePending;
        private bool clampReleased;
        private bool animationStateEntered;
        private bool smallThrowLaunchPending;
        private uint nextCargoStrapUseSequence;
        private uint activeCargoStrapUseSequence;
        private uint completedSmallThrowRevision;
        private float attackCollisionExpiresAt;
        private float phaseStartedAt;
        private float phaseProgress01;
        private float releaseStartedAt;
        private Vector3 releaseStartWorldPosition;
        private Vector3 releaseEndWorldPosition;
        private Vector3 smallThrowAimWorldPoint;
        private float smallSpinAngleRadians;
        private float smallSpinLastSafeAngleRadians;
        private float smallSpinDynamicRadius;
        private float smallSpinVerticalClearance;
        private bool smallSpinCenterInitialized;
        private bool smallSpinHasSafeAngle;
        private bool smallSpinObstructed;
        private Vector3 smallSpinCenterWorld;
        private Vector3 smallSpinCenterVelocity;
        private Vector3 smallSpinPreviousCenterWorld;
        private readonly HashSet<int> mediumSwingHitRunnerIds = new();
        private bool mediumSwingPlanActive;
        private bool mediumSwingMotionStarted;
        private bool mediumSwingAttackLeft;
        private bool mediumSwingImpactCommitted;
        private uint nextMediumSwingSequence;
        private uint activeMediumSwingSequence;
        private ShowcaseRunnerAgent mediumSwingTarget;
        private Collider mediumSwingTargetCollider;
        private Vector3 mediumSwingSnapshotTargetPoint;
        private Vector3 mediumSwingCorrectedTargetPoint;
        private Vector3 mediumSwingForwardSnapshot;
        private Vector3 mediumSwingRightSnapshot;
        private float mediumSwingRearRadius;
        private float mediumSwingStartAngleRadians;
        private float mediumSwingTargetAngleRadians;
        private float mediumSwingDesiredTetherLength;
        private float mediumSwingEffectiveTetherLength;
        private int mediumSwingTrackingFailureSteps;
        private float mediumRecoveryStartedAt;
        private float mediumExtensionStartedAt;
        private float mediumGroundCenterOfMassY;
        private float mediumRearDistance;
        private bool mediumRearReady;
        private int mediumRearStableSteps;
        private bool pullMotionInitialized;
        private Vector3 pullStartWorldPosition;
        private float pullStartPlanarRadius;
        private float pullStartHeight;
        private float pullTargetPlanarRadius;
        private float pullTargetHeight;

        public ShowcaseCargoStrapPhase Phase { get; private set; } =
            ShowcaseCargoStrapPhase.Stored;

        public ShowcaseCargoStrapAnimationCue AnimationCue { get; private set; }

        public uint AnimationCueRevision { get; private set; }

        public event Action<PrototypeInteractionTarget> AssistedAttachCompleted;

        public PrototypeInteractionTarget ActiveTarget => activeTarget != null
            ? activeTarget
            : pendingTarget;

        public PrototypeInteractionTarget LastRedirectedTarget { get; private set; }

        public PrototypeInteractionTarget AssistCandidate { get; private set; }

        public bool IsAttached => interactionOwner != null &&
            interactionOwner.StrapState != PrototypeStrapOwnershipState.Stored &&
            activeTarget != null;

        public bool IsTensioned => interactionOwner != null &&
            interactionOwner.StrapState == PrototypeStrapOwnershipState.Tensioned &&
            activeTarget != null;

        public bool IsActionLocked => Phase == ShowcaseCargoStrapPhase.Prepare ||
            Phase == ShowcaseCargoStrapPhase.Rolling ||
            Phase == ShowcaseCargoStrapPhase.Throw ||
            Phase == ShowcaseCargoStrapPhase.Pull ||
            Phase == ShowcaseCargoStrapPhase.SmallSpinning ||
            Phase == ShowcaseCargoStrapPhase.AttackLeft ||
            Phase == ShowcaseCargoStrapPhase.AttackRight ||
            Phase == ShowcaseCargoStrapPhase.MediumExtending ||
            Phase == ShowcaseCargoStrapPhase.MediumRecovering;

        public bool IsAttackActive => Phase == ShowcaseCargoStrapPhase.AttackLeft ||
            Phase == ShowcaseCargoStrapPhase.AttackRight;

        public bool IsSmallSpinning => Phase == ShowcaseCargoStrapPhase.SmallSpinning;

        public bool IsMediumSwinging => mediumSwingPlanActive &&
            IsAttackActive && mediumSwingMotionStarted;

        public bool IsMediumRecovering =>
            Phase == ShowcaseCargoStrapPhase.MediumRecovering;

        public bool IsMediumAttackMobilityPhase =>
            activeTarget != null &&
            activeTarget.WeightClass == PrototypeWeightClass.Medium &&
            mediumSwingPlanActive &&
            (Phase == ShowcaseCargoStrapPhase.AttackLeft ||
             Phase == ShowcaseCargoStrapPhase.AttackRight ||
             Phase == ShowcaseCargoStrapPhase.MediumExtending ||
             Phase == ShowcaseCargoStrapPhase.MediumRecovering);

        public bool IsMediumRearReady => mediumRearReady;

        public bool IsMediumFloorSupported => activeCollider != null &&
            IsFloorSupported(activeCollider);

        public bool BlocksPlayerMobility => IsActionLocked &&
            !IsSmallSpinning &&
            !IsMediumAttackMobilityPhase;

        public Transform ResolvedRightHandTransform => rightHandTransform;

        public Vector3 SmallSpinCenterWorld => smallSpinCenterInitialized
            ? smallSpinCenterWorld
            : ResolveSmallSpinTargetCenter();

        public float SmallSpinRadius => smallSpinDynamicRadius > 0f
            ? smallSpinDynamicRadius
            : ResolveSmallSpinRadius();

        public float SmallSpinVerticalClearance => smallSpinVerticalClearance;

        public bool IsSmallSpinObstructed => smallSpinObstructed;


        public ShowcaseRunnerAgent MediumSwingTarget => mediumSwingTarget;

        public Vector3 MediumSwingSnapshotTargetPoint =>
            mediumSwingSnapshotTargetPoint;

        public Vector3 MediumSwingCorrectedTargetPoint =>
            mediumSwingCorrectedTargetPoint;

        public float MediumEffectiveTetherLength =>
            mediumSwingEffectiveTetherLength;

        public float MediumDesiredTetherLength =>
            mediumSwingDesiredTetherLength;

        public uint ActiveMediumSwingSequence => activeMediumSwingSequence;

        public int MediumSwingStepCount { get; private set; }

        public int MediumRecoveryStepCount { get; private set; }

        public float PullDistanceReduced { get; private set; }

        public bool AttackBufferWindowOpen => IsAttackActive &&
            phaseProgress01 >= AttackBufferStart01;

        public bool IsStrapWeaponSelected => IsTensioned &&
            interactionOwner != null && interactionOwner.WeaponSelection ==
                PrototypeWeaponSelection.ObjectWeapon;

        public float PhaseProgress01 => Phase == ShowcaseCargoStrapPhase.Releasing
            ? Mathf.Clamp01((Time.time - releaseStartedAt) / ReleaseDuration)
            : phaseProgress01;

        public float CurrentStrapLength => activeTarget == null || playerTransform == null
            ? 0f
            : Vector3.Distance(GetReelPosition(), GetAnchorPosition());

        public bool IsWarning => IsAttached && CurrentStrapLength >= warningDistance;

        public int AssistedAttachCount { get; private set; }

        public int DirectAttachCount { get; private set; }

        public int ReleaseCount { get; private set; }

        public int RedirectStepCount { get; private set; }

        public int AttackImpulseCount { get; private set; }

        public int ValidStrapCollisionCount { get; private set; }

        public int DepletionCount { get; private set; }

        public int BufferedAttackCount { get; private set; }

        public int RejectedAnimationEventCount { get; private set; }

        public int AnchorFacingStepCount { get; private set; }

        public float LastAnchorFacingErrorDegrees { get; private set; }

        public int FaultCount { get; private set; }

        public string LastFault { get; private set; } = string.Empty;

        public string LastMediumAttackRejection { get; private set; } =
            string.Empty;

        public float AssistAttachRange => assistAttachRange;

        public float MaximumAllyAssistRange => maximumAllyAssistRange;

        public float RedirectSpeed => redirectSpeed;

        public float MaximumStrapLength => maximumStrapLength;

        public float MediumMaximumAttackRange => Mathf.Min(
            mediumSwingMaximumRange,
            maximumStrapLength - mediumSwingTrajectoryAllowance);

        public float WarningDistance => warningDistance;

        public ShowcaseCargoStrapPresentationSnapshot PresentationSnapshot =>
            new ShowcaseCargoStrapPresentationSnapshot(
                Phase,
                PhaseProgress01,
                ActiveTarget,
                GetReelPosition(),
                GetAnchorPosition(),
                releaseStartWorldPosition,
                releaseEndWorldPosition,
                clampReleased,
                IsTensioned,
                IsWarning,
                interactionOwner != null
                    ? interactionOwner.WeaponSelection
                    : PrototypeWeaponSelection.Fists,
                ResolvePresentationEffectiveTetherLength(),
                ResolvePresentationTension(),
                smallSpinObstructed);

        private float ResolvePresentationEffectiveTetherLength()
        {
            if (ActiveTarget?.WeightClass != PrototypeWeightClass.Medium)
            {
                return 0f;
            }
            return mediumSwingPlanActive &&
                   mediumSwingEffectiveTetherLength > 0f
                ? mediumSwingEffectiveTetherLength
                : Mathf.Max(mediumRearDistance, CurrentStrapLength);
        }

        private float ResolvePresentationTension()
        {
            if (ActiveTarget?.WeightClass != PrototypeWeightClass.Medium)
            {
                return -1f;
            }
            return Phase switch
            {
                ShowcaseCargoStrapPhase.Pull =>
                    Mathf.Clamp01(phaseProgress01),
                ShowcaseCargoStrapPhase.AttackLeft => 1f,
                ShowcaseCargoStrapPhase.AttackRight => 1f,
                ShowcaseCargoStrapPhase.MediumExtending => 1f,
                ShowcaseCargoStrapPhase.MediumRecovering => 0.65f,
                ShowcaseCargoStrapPhase.Releasing => 0.35f,
                _ => 0.55f
            };
        }

        private void Awake()
        {
            if (interactionOwner == null)
            {
                interactionOwner = GetComponent<PrototypeInteractionStateOwner>();
            }
            if (logicalInput == null)
            {
                logicalInput = GetComponent<InputSystemLogicalInputAdapter>();
            }
            if (playerTransform == null)
            {
                playerTransform = transform;
            }
            if (playerCollider == null)
            {
                playerCollider = GetComponent<Collider>();
            }
            locomotion = GetComponent<ThirdPersonLocomotion>();
            physicalLifecycle = GetComponent<PrototypePhysicalObjectLifecycle>();
            ResolveRunnerPressureController();
            ResolveRightHandTransform();
            ResolveCarabinerTransform();
        }

        private void Update()
        {
            if (Phase == ShowcaseCargoStrapPhase.Stored && pendingTarget == null)
            {
                SynchronizeLogicalAttachment();
            }
            SynchronizeSmallSpinWithWeaponSelection();
            ObserveAttackCollision();
            ObserveMediumSwingState();
            ObserveBufferedMediumAttack();
            ObserveAttachInputRelease();
            if (Phase == ShowcaseCargoStrapPhase.Releasing)
            {
                UpdateRelease();
            }
            else
            {
                CheckAnimationTimeout();
            }
            SynchronizeLocomotionPolicy();
            if (IsActionLocked &&
                Phase != ShowcaseCargoStrapPhase.SmallSpinning &&
                !(mediumSwingPlanActive && mediumSwingMotionStarted))
            {
                FaceSequenceTarget();
            }
            RefreshAssistCandidate();
        }

        private void FixedUpdate()
        {
            SynchronizeLogicalAttachment();
            if (!IsAttached || activeBody == null || playerTransform == null)
            {
                return;
            }

            if (Phase == ShowcaseCargoStrapPhase.Pull)
            {
                UpdatePullBody();
                return;
            }

            if ((Phase == ShowcaseCargoStrapPhase.SmallSpinning ||
                 (Phase == ShowcaseCargoStrapPhase.Throw && smallThrowLaunchPending)) &&
                activeTarget.WeightClass == PrototypeWeightClass.Light)
            {
                UpdateSmallSpinBody();
                return;
            }

            if (activeTarget.WeightClass == PrototypeWeightClass.Medium)
            {
                if (mediumSwingPlanActive &&
                    Phase != ShowcaseCargoStrapPhase.MediumRecovering &&
                    !IsMediumSwingTargetWithinRange())
                {
                    LastMediumAttackRejection =
                        "MEDIUM_TARGET_MOVED_OUT_OF_RANGE";
                    BeginMediumRecovery();
                }
                if ((IsAttackActive ||
                     Phase == ShowcaseCargoStrapPhase.MediumExtending) &&
                    mediumSwingMotionStarted)
                {
                    UpdateMediumSwingBody();
                }
                else if (Phase == ShowcaseCargoStrapPhase.MediumRecovering)
                {
                    UpdateMediumRecoveryBody();
                }
                else if (Phase == ShowcaseCargoStrapPhase.Tensioned ||
                         IsAttackActive)
                {
                    UpdateMediumRearFollower();
                }
                ApplyMaximumStrapConstraint(false);
                return;
            }
            ApplyMaximumStrapConstraint(true);
        }

        private void ApplyMaximumStrapConstraint(bool allowPositionCorrection)
        {
            Vector3 fromReel = GetAnchorPosition() - GetReelPosition();
            float length = fromReel.magnitude;
            float constraintLength = !allowPositionCorrection &&
                mediumSwingPlanActive &&
                mediumSwingEffectiveTetherLength > 0f
                    ? Mathf.Min(
                        maximumStrapLength,
                        mediumSwingEffectiveTetherLength + 0.2f)
                    : maximumStrapLength;
            if (length <= 0.001f || length < constraintLength)
            {
                return;
            }

            Vector3 outward = fromReel / length;
            float overrun = length - constraintLength;
            if (allowPositionCorrection && overrun > 0f)
            {
                // Preserve the accepted Small behaviour. Medium never enters
                // this position-correction path; its tether is velocity-only.
                activeBody.MovePosition(activeBody.position - (outward * overrun));
            }
            float outwardSpeed = Vector3.Dot(activeBody.linearVelocity, outward);
            if (outwardSpeed > 0f)
            {
                float maximumVelocityChange = allowPositionCorrection
                    ? outwardSpeed
                    : mediumRearMaximumAcceleration * Time.fixedDeltaTime;
                activeBody.linearVelocity -= outward * Mathf.Min(
                    outwardSpeed,
                    maximumVelocityChange);
            }
            if (!allowPositionCorrection && overrun > 0f)
            {
                activeBody.linearVelocity += -outward * Mathf.Min(
                    mediumRearMaximumAcceleration * Time.fixedDeltaTime,
                    overrun * mediumRearTrackingResponse);
            }
        }

        private void OnDisable()
        {
            ClearLocomotionPolicy();
            interactionOwner?.TryReleaseStrap();
            ClearPhysicalAttachment();
            pendingTarget = null;
            Phase = ShowcaseCargoStrapPhase.Stored;
            AnimationCue = ShowcaseCargoStrapAnimationCue.None;
        }

        public void Configure(
            PrototypeInteractionStateOwner owner,
            InputSystemLogicalInputAdapter input,
            Transform player,
            Collider authoredPlayerCollider,
            Transform ally,
            PrototypeInteractionTarget[] targets,
            LineRenderer line,
            float authoredAssistRange = 3.5f,
            float authoredMaximumAllyRange = 7f,
            float authoredRedirectSpeed = 4.5f,
            float authoredMaximumLength = 7f,
            float authoredWarningDistance = 6.2f)
        {
            interactionOwner = owner;
            logicalInput = input;
            playerTransform = player;
            playerCollider = authoredPlayerCollider;
            allyTransform = ally;
            authoredTargets = targets ?? Array.Empty<PrototypeInteractionTarget>();
            strapLine = line;
            assistAttachRange = Mathf.Max(0.5f, authoredAssistRange);
            maximumAllyAssistRange = Mathf.Max(0.5f, authoredMaximumAllyRange);
            redirectSpeed = Mathf.Max(0.1f, authoredRedirectSpeed);
            maximumStrapLength = Mathf.Max(1f, authoredMaximumLength);
            warningDistance = Mathf.Clamp(
                authoredWarningDistance,
                0.5f,
                maximumStrapLength);
            locomotion = player != null
                ? player.GetComponent<ThirdPersonLocomotion>()
                : GetComponent<ThirdPersonLocomotion>();
            physicalLifecycle = player != null
                ? player.GetComponent<PrototypePhysicalObjectLifecycle>()
                : GetComponent<PrototypePhysicalObjectLifecycle>();
            ResolveRunnerPressureController();
            ResolveRightHandTransform();
            ResolveCarabinerTransform();
            SynchronizeLogicalAttachment();
            RefreshAssistCandidate();
        }

        public bool CanAssistAttach()
        {
            RefreshAssistCandidate();
            return AssistCandidate != null;
        }

        public bool CanDirectAttach(PrototypeInteractionTarget target)
        {
            if (Phase != ShowcaseCargoStrapPhase.Stored ||
                pendingTarget != null || interactionOwner == null ||
                interactionOwner.GlobalState != PrototypeGlobalInteractionState.Playable ||
                interactionOwner.HandState != PrototypeHandOwnershipState.Free ||
                interactionOwner.StrapState != PrototypeStrapOwnershipState.Stored ||
                interactionOwner.RelayState != PrototypeRelayReservationState.None ||
                !IsEligibleStrapTarget(target) ||
                !TryGetPhysicalTarget(target, out _, out Collider targetCollider))
            {
                return false;
            }

            return IsFloorSupported(targetCollider);
        }

        public bool TryBeginDirectAttach(PrototypeInteractionTarget target)
        {
            return CanDirectAttach(target) &&
                BeginAttachSequence(target, false, false);
        }

        public bool TryAssistAttach(out PrototypeInteractionTarget target)
        {
            RefreshAssistCandidate();
            target = AssistCandidate;
            if (target == null || interactionOwner == null)
            {
                return false;
            }

            return BeginAttachSequence(target, true, true);
        }

        public bool TryExplicitRelease()
        {
            if (Phase == ShowcaseCargoStrapPhase.Stored ||
                Phase == ShowcaseCargoStrapPhase.Releasing)
            {
                return false;
            }

            if (IsAttackActive)
            {
                releaseQueued = true;
                return true;
            }

            if (activeTarget == null)
            {
                CancelPendingSequence();
                return true;
            }

            return BeginRelease(false);
        }

        public void CancelForPlayerInterruption()
        {
            if (Phase == ShowcaseCargoStrapPhase.Stored ||
                Phase == ShowcaseCargoStrapPhase.Releasing)
            {
                return;
            }

            if (activeTarget != null)
            {
                BeginRelease(false);
            }
            else
            {
                CancelPendingSequence();
            }
        }

        public bool NotifyDirectAttachment(PrototypeInteractionTarget target)
        {
            if (target == null || interactionOwner == null ||
                Phase != ShowcaseCargoStrapPhase.Stored)
            {
                return false;
            }

            // Legacy callers used to commit StateOwner first and then notify
            // Cargo, which bypassed every animation event. Roll that logical
            // fact back and enter the same deferred transaction as live input.
            if (interactionOwner.StrapTarget == target &&
                interactionOwner.StrapState != PrototypeStrapOwnershipState.Stored)
            {
                interactionOwner.TryReleaseStrap();
            }
            return TryBeginDirectAttach(target);
        }

        public bool TryBeginAttack(Vector3 aimWorldPoint)
        {
            LastMediumAttackRejection = string.Empty;
            if (Phase == ShowcaseCargoStrapPhase.SmallSpinning)
            {
                if (!IsStrapWeaponSelected || activeTarget == null ||
                    activeTarget.WeightClass != PrototypeWeightClass.Light ||
                    !activeTarget.gameObject.activeInHierarchy)
                {
                    return false;
                }

                StartSmallThrow(aimWorldPoint);
                return true;
            }

            if (IsAttackActive)
            {
                if (mediumSwingPlanActive && AttackBufferWindowOpen &&
                    !bufferedAttack)
                {
                    bufferedAttack = true;
                    bufferedMediumAimWorldPoint = aimWorldPoint;
                    BufferedAttackCount++;
                    return true;
                }
                return false;
            }

            if (Phase != ShowcaseCargoStrapPhase.Tensioned ||
                !IsStrapWeaponSelected || activeTarget == null ||
                !activeTarget.gameObject.activeInHierarchy)
            {
                LastMediumAttackRejection = "MEDIUM_ATTACK_STATE_NOT_READY";
                return false;
            }

            if (activeTarget.WeightClass != PrototypeWeightClass.Medium ||
                !mediumRearReady ||
                !TryCreateMediumSwingPlan(aimWorldPoint, out bool attackLeft))
            {
                if (string.IsNullOrEmpty(LastMediumAttackRejection))
                {
                    LastMediumAttackRejection = !mediumRearReady
                        ? "MEDIUM_REAR_NOT_READY"
                        : "MEDIUM_TARGET_PLAN_REJECTED";
                }
                return false;
            }

            StartAttack(attackLeft);
            return true;
        }

        private bool TryCreateMediumSwingPlan(
            Vector3 aimWorldPoint,
            out bool attackLeft)
        {
            attackLeft = false;
            ResolveRunnerPressureController();
            float maximumTargetRange = MediumMaximumAttackRange;
            if (runnerPressure == null || maximumTargetRange <= 0f ||
                !runnerPressure.TryResolveCargoStrapTarget(
                    aimWorldPoint,
                    maximumTargetRange,
                    out ShowcaseRunnerAgent target,
                    out Vector3 targetPoint,
                    out _))
            {
                LastMediumAttackRejection = "MEDIUM_TARGET_NOT_RESOLVED";
                return false;
            }

            Collider targetCollider = target.GetComponent<Collider>();
            if (targetCollider == null || !targetCollider.enabled)
            {
                LastMediumAttackRejection = "MEDIUM_TARGET_COLLIDER_MISSING";
                return false;
            }

            Vector3 reel = GetReelPosition();
            Vector3 planarTargetOffset = Vector3.ProjectOnPlane(
                targetPoint - reel,
                Vector3.up);
            float targetDistance = planarTargetOffset.magnitude;
            if (targetDistance > maximumTargetRange + 0.001f)
            {
                LastMediumAttackRejection = "MEDIUM_TARGET_OUT_OF_RANGE";
                return false;
            }

            attackLeft = ResolveAttackLeft(targetPoint);
            mediumSwingPlanActive = true;
            mediumSwingMotionStarted = false;
            mediumSwingAttackLeft = attackLeft;
            mediumSwingImpactCommitted = false;
            mediumSwingTarget = target;
            mediumSwingTargetCollider = targetCollider;
            mediumSwingSnapshotTargetPoint = targetPoint;
            mediumSwingCorrectedTargetPoint = targetPoint;
            mediumSwingForwardSnapshot = Vector3.ProjectOnPlane(
                playerTransform.forward,
                Vector3.up).normalized;
            mediumSwingRightSnapshot = Vector3.ProjectOnPlane(
                playerTransform.right,
                Vector3.up).normalized;
            mediumSwingRearRadius = Mathf.Max(
                mediumRearMinimumDistance,
                Vector3.ProjectOnPlane(
                    activeBody.worldCenterOfMass - reel,
                    Vector3.up).magnitude);
            mediumSwingStartAngleRadians = attackLeft
                ? -Mathf.PI
                : Mathf.PI;
            Vector3 targetDirection = planarTargetOffset.sqrMagnitude > 0.0001f
                ? planarTargetOffset.normalized
                : mediumSwingForwardSnapshot;
            mediumSwingTargetAngleRadians = Mathf.Atan2(
                Vector3.Dot(targetDirection, mediumSwingRightSnapshot),
                Vector3.Dot(targetDirection, mediumSwingForwardSnapshot));
            mediumSwingDesiredTetherLength = Mathf.Clamp(
                targetDistance + mediumSwingTrajectoryAllowance,
                Mathf.Max(mediumSwingMinimumRange, mediumSwingRearRadius),
                maximumStrapLength - 0.05f);
            mediumSwingEffectiveTetherLength = Mathf.Clamp(
                CurrentStrapLength,
                mediumSwingMinimumRange,
                mediumSwingDesiredTetherLength);
            mediumSwingTrackingFailureSteps = 0;
            activeMediumSwingSequence = ++nextMediumSwingSequence;
            mediumSwingHitRunnerIds.Clear();
            mediumRearReady = false;
            mediumRearStableSteps = 0;
            return true;
        }

        public bool NotifyAnimationStateEntered(uint revision)
        {
            if (!IsCurrentRevision(revision))
            {
                RejectedAnimationEventCount++;
                return false;
            }

            animationStateEntered = true;
            return true;
        }

        public bool ReportAnimationProgress(uint revision, float normalized01)
        {
            if (!IsCurrentRevision(revision))
            {
                return false;
            }

            phaseProgress01 = Mathf.Clamp01(normalized01);
            return true;
        }

        public bool CommitAnimationStrapThrowRelease(uint revision)
        {
            if (!IsQualifiedAnimationEvent(
                    revision,
                    ShowcaseCargoStrapAnimationCue.Throw))
            {
                return false;
            }

            if (smallThrowLaunchPending)
            {
                return CommitSmallThrowLaunch(revision);
            }

            clampReleased = true;
            return true;
        }

        public bool CommitAnimationStrapAttachContact(uint revision)
        {
            if (revision != 0 && revision == completedSmallThrowRevision)
            {
                return true;
            }

            if (!IsQualifiedAnimationEvent(
                    revision,
                    ShowcaseCargoStrapAnimationCue.Throw) ||
                pendingTarget == null || activeTarget != null ||
                !CanCommitPendingAttachment())
            {
                return false;
            }

            PrototypeInteractionDecision decision =
                PrototypeInteractionDecision.Accept(
                    PrototypeInteractionOutcome.AttachStrap,
                    pendingTarget);
            if (!interactionOwner.TryCommit(decision) ||
                !BeginPhysicalAttachment(pendingTarget))
            {
                interactionOwner.TryReleaseStrap();
                RegisterFault("STRAP_ATTACH_CONTACT_COMMIT_FAILED");
                CancelPendingSequence();
                return false;
            }

            clampReleased = true;
            if (pendingWasAssisted)
            {
                AssistedAttachCount++;
            }
            else
            {
                DirectAttachCount++;
            }
            return true;
        }

        public bool CommitAnimationStrapPullComplete(uint revision)
        {
            if (!IsQualifiedAnimationEvent(
                    revision,
                    ShowcaseCargoStrapAnimationCue.Pull))
            {
                return false;
            }

            return CompletePull();
        }

        public bool CommitAnimationStrapAttackImpulse(uint revision)
        {
            ShowcaseCargoStrapAnimationCue expected =
                Phase == ShowcaseCargoStrapPhase.AttackLeft
                    ? ShowcaseCargoStrapAnimationCue.AttackLeft
                    : ShowcaseCargoStrapAnimationCue.AttackRight;
            if (!IsAttackActive || attackImpulseCommitted ||
                !IsQualifiedAnimationEvent(revision, expected) ||
                activeBody == null || activeTarget == null)
            {
                return false;
            }

            attackImpulseCommitted = true;
            if (attackUsePending && activeCargoStrapUseSequence != 0)
            {
                activeTarget.CancelCargoStrapCollisionObservation(
                    activeCargoStrapUseSequence);
            }
            attackUsePending = true;
            attackCollisionExpiresAt = Time.time + AttackCollisionGraceSeconds;
            activeCargoStrapUseSequence = ++nextCargoStrapUseSequence;
            activeTarget.BeginCargoStrapCollisionObservation(
                activeCargoStrapUseSequence,
                playerTransform,
                validCollisionSpeed);
            ApplyAttackImpulse(Phase == ShowcaseCargoStrapPhase.AttackLeft);
            AttackImpulseCount++;
            RedirectStepCount++;
            LastRedirectedTarget = activeTarget;
            return true;
        }

        public bool CompleteAnimationStrapAction(uint revision)
        {
            if (revision != 0 && revision == completedSmallThrowRevision)
            {
                return true;
            }

            ShowcaseCargoStrapAnimationCue expectedCue = Phase switch
            {
                ShowcaseCargoStrapPhase.Prepare =>
                    ShowcaseCargoStrapAnimationCue.Prepare,
                ShowcaseCargoStrapPhase.Rolling =>
                    ShowcaseCargoStrapAnimationCue.Rolling,
                ShowcaseCargoStrapPhase.Throw =>
                    ShowcaseCargoStrapAnimationCue.Throw,
                ShowcaseCargoStrapPhase.Pull =>
                    ShowcaseCargoStrapAnimationCue.Pull,
                ShowcaseCargoStrapPhase.SmallSpinning =>
                    ShowcaseCargoStrapAnimationCue.Rolling,
                ShowcaseCargoStrapPhase.AttackLeft =>
                    ShowcaseCargoStrapAnimationCue.AttackLeft,
                ShowcaseCargoStrapPhase.AttackRight =>
                    ShowcaseCargoStrapAnimationCue.AttackRight,
                _ => ShowcaseCargoStrapAnimationCue.None
            };
            if (expectedCue == ShowcaseCargoStrapAnimationCue.None ||
                !IsQualifiedAnimationEvent(revision, expectedCue))
            {
                return false;
            }

            phaseProgress01 = 1f;
            switch (Phase)
            {
                case ShowcaseCargoStrapPhase.Prepare:
                    if (automaticSequence || interactReleaseQueued ||
                        logicalInput == null ||
                        !logicalInput.Snapshot.InteractPrimaryHeld)
                    {
                        StartThrow();
                    }
                    else
                    {
                        EnterPhase(
                            ShowcaseCargoStrapPhase.Rolling,
                            ShowcaseCargoStrapAnimationCue.Rolling);
                    }
                    return true;

                case ShowcaseCargoStrapPhase.Rolling:
                    if (interactReleaseQueued || logicalInput == null ||
                        !logicalInput.Snapshot.InteractPrimaryHeld)
                    {
                        StartThrow();
                    }
                    else
                    {
                        phaseProgress01 = 0f;
                    }
                    return true;

                case ShowcaseCargoStrapPhase.Throw:
                    if (activeTarget == null || interactionOwner == null ||
                        interactionOwner.StrapTarget != activeTarget)
                    {
                        RegisterFault("STRAP_ATTACH_CONTACT_EVENT_MISSING");
                        CancelPendingSequence();
                        return false;
                    }
                    EnterPhase(
                        ShowcaseCargoStrapPhase.Pull,
                        ShowcaseCargoStrapAnimationCue.Pull);
                    return true;

                case ShowcaseCargoStrapPhase.Pull:
                    return CompletePull();

                case ShowcaseCargoStrapPhase.SmallSpinning:
                    phaseProgress01 = 0f;
                    return true;

                case ShowcaseCargoStrapPhase.AttackLeft:
                case ShowcaseCargoStrapPhase.AttackRight:
                    CompleteAttack();
                    return true;

                default:
                    return false;
            }
        }

        private bool BeginAttachSequence(
            PrototypeInteractionTarget target,
            bool autoSequence,
            bool assisted)
        {
            if (target == null || !target.IsCargoStrapEligible ||
                Phase != ShowcaseCargoStrapPhase.Stored || pendingTarget != null)
            {
                return false;
            }

            pendingTarget = target;
            automaticSequence = autoSequence;
            pendingWasAssisted = assisted;
            interactReleaseQueued = autoSequence;
            releaseQueued = false;
            bufferedAttack = false;
            bufferedMediumAimWorldPoint = Vector3.zero;
            clampReleased = false;
            EnterPhase(
                ShowcaseCargoStrapPhase.Prepare,
                ShowcaseCargoStrapAnimationCue.Prepare);
            return true;
        }

        private void StartThrow()
        {
            interactReleaseQueued = true;
            smallThrowLaunchPending = false;
            EnterPhase(
                ShowcaseCargoStrapPhase.Throw,
                ShowcaseCargoStrapAnimationCue.Throw);
        }

        private bool CompletePull()
        {
            if (activeTarget == null || interactionOwner == null ||
                interactionOwner.StrapTarget != activeTarget ||
                (!IsTensioned &&
                 !interactionOwner.TryBeginStrapTension(activeTarget)))
            {
                RegisterFault("STRAP_PULL_COMMIT_FAILED");
                BeginRelease(false);
                return false;
            }

            bool completeAssistedAttach = pendingWasAssisted;
            PrototypeInteractionTarget completedTarget = activeTarget;
            interactionOwner.TrySelectWeapon(PrototypeWeaponSelection.ObjectWeapon);
            EnterTensioned();
            if (completeAssistedAttach)
            {
                AssistedAttachCompleted?.Invoke(completedTarget);
            }
            return true;
        }

        private void EnterTensioned()
        {
            automaticSequence = false;
            pendingWasAssisted = false;
            interactReleaseQueued = false;
            releaseQueued = false;
            bool preserveMediumBuffer =
                Phase == ShowcaseCargoStrapPhase.MediumRecovering &&
                bufferedAttack;
            if (!preserveMediumBuffer)
            {
                bufferedAttack = false;
                bufferedMediumAimWorldPoint = Vector3.zero;
            }
            if (activeTarget != null &&
                activeTarget.WeightClass == PrototypeWeightClass.Medium &&
                activeBody != null)
            {
                mediumRearDistance = ResolveMediumRearDistance();
                mediumRearReady = false;
                mediumRearStableSteps = 0;
            }
            if (activeTarget != null &&
                activeTarget.WeightClass == PrototypeWeightClass.Light &&
                IsStrapWeaponSelected)
            {
                EnterSmallSpinning();
            }
            else
            {
                EnterPhase(
                    ShowcaseCargoStrapPhase.Tensioned,
                    ShowcaseCargoStrapAnimationCue.None);
            }
        }

        private void EnterSmallSpinning()
        {
            ResolveRightHandTransform();
            smallSpinDynamicRadius = ResolveSmallSpinRadius();
            smallSpinCenterWorld = ResolveSmallSpinTargetCenter();
            smallSpinPreviousCenterWorld = smallSpinCenterWorld;
            smallSpinCenterVelocity = Vector3.zero;
            smallSpinCenterInitialized = true;
            smallSpinObstructed = false;
            smallSpinHasSafeAngle = false;
            Vector3 center = smallSpinCenterWorld;
            Vector3 offset = activeBody != null
                ? Vector3.ProjectOnPlane(activeBody.worldCenterOfMass - center, Vector3.up)
                : Vector3.zero;
            Vector3 playerRight = playerTransform != null
                ? playerTransform.right
                : Vector3.right;
            Vector3 playerForward = playerTransform != null
                ? playerTransform.forward
                : Vector3.forward;
            if (offset.sqrMagnitude <= 0.0001f)
            {
                offset = playerRight;
            }
            smallSpinAngleRadians = Mathf.Atan2(
                Vector3.Dot(offset.normalized, playerForward),
                Vector3.Dot(offset.normalized, playerRight));
            smallSpinLastSafeAngleRadians = smallSpinAngleRadians;
            smallThrowLaunchPending = false;
            EnterPhase(
                ShowcaseCargoStrapPhase.SmallSpinning,
                ShowcaseCargoStrapAnimationCue.Rolling);
        }

        private void StartSmallThrow(Vector3 aimWorldPoint)
        {
            smallThrowAimWorldPoint = aimWorldPoint;
            smallThrowLaunchPending = true;
            EnterPhase(
                ShowcaseCargoStrapPhase.Throw,
                ShowcaseCargoStrapAnimationCue.Throw);
        }

        private bool CommitSmallThrowLaunch(uint revision)
        {
            PrototypeInteractionTarget launchedTarget = activeTarget;
            if (launchedTarget == null || activeBody == null ||
                launchedTarget.WeightClass != PrototypeWeightClass.Light ||
                interactionOwner == null)
            {
                RegisterFault("SMALL_STRAP_THROW_TARGET_MISSING");
                CancelPendingSequence();
                return false;
            }

            physicalLifecycle ??= GetComponent<PrototypePhysicalObjectLifecycle>();
            if (physicalLifecycle == null ||
                !physicalLifecycle.TryCreateThrowSolution(
                    launchedTarget,
                    smallThrowAimWorldPoint,
                    0f,
                    out _))
            {
                RegisterFault("SMALL_STRAP_THROW_SOLUTION_FAILED");
                BeginRelease(false);
                return false;
            }

            if (interactionOwner.StrapState !=
                    PrototypeStrapOwnershipState.ReleaseRequested &&
                !interactionOwner.TryBeginStrapRelease(launchedTarget))
            {
                RegisterFault("SMALL_STRAP_THROW_RELEASE_BEGIN_FAILED");
                BeginRelease(false);
                return false;
            }
            if (!interactionOwner.TryCompleteStrapRelease(launchedTarget))
            {
                interactionOwner.TryReleaseStrap();
                if (interactionOwner.StrapState != PrototypeStrapOwnershipState.Stored)
                {
                    RegisterFault("SMALL_STRAP_THROW_RELEASE_COMMIT_FAILED");
                    BeginRelease(false);
                    return false;
                }
            }

            clampReleased = true;
            completedSmallThrowRevision = revision;
            smallThrowLaunchPending = false;
            pendingTarget = null;
            ClearPhysicalAttachment();

            bool launched = physicalLifecycle.TryLaunchReleasedStrapTargetAt(
                launchedTarget,
                smallThrowAimWorldPoint,
                0f);
            if (!launched)
            {
                RegisterFault("SMALL_STRAP_THROW_PHYSICAL_LAUNCH_FAILED");
            }
            else if (launchedTarget.IsPlayerThrowExhausted)
            {
                DepletionCount++;
            }

            interactionOwner.TrySelectWeapon(PrototypeWeaponSelection.Fists);
            EnterPhase(
                ShowcaseCargoStrapPhase.Stored,
                ShowcaseCargoStrapAnimationCue.None);
            return launched;
        }

        private void StartAttack(bool attackLeft)
        {
            lastAttackWasLeft = attackLeft;
            attackImpulseCommitted = false;
            bufferedAttack = false;
            bufferedMediumAimWorldPoint = Vector3.zero;
            releaseQueued = false;
            EnterPhase(
                attackLeft
                    ? ShowcaseCargoStrapPhase.AttackLeft
                    : ShowcaseCargoStrapPhase.AttackRight,
                attackLeft
                    ? ShowcaseCargoStrapAnimationCue.AttackLeft
                    : ShowcaseCargoStrapAnimationCue.AttackRight);
        }

        private void CompleteAttack()
        {
            bool startBuffered = bufferedAttack;
            bool nextLeft = bufferedAttackLeft;
            bool mediumAttack = mediumSwingPlanActive &&
                activeTarget != null &&
                activeTarget.WeightClass == PrototypeWeightClass.Medium;
            if (!mediumAttack)
            {
                bufferedAttack = false;
            }
            if (releaseQueued)
            {
                BeginRelease(false);
            }
            else if (mediumAttack)
            {
                mediumExtensionStartedAt = Time.time;
                EnterPhase(
                    ShowcaseCargoStrapPhase.MediumExtending,
                    ShowcaseCargoStrapAnimationCue.None);
            }
            else if (startBuffered)
            {
                StartAttack(nextLeft);
            }
            else
            {
                EnterPhase(
                    ShowcaseCargoStrapPhase.Tensioned,
                    ShowcaseCargoStrapAnimationCue.None);
            }
        }

        private bool BeginRelease(bool depleted)
        {
            if (activeTarget == null || interactionOwner == null)
            {
                CancelPendingSequence();
                return false;
            }

            if (interactionOwner.StrapState !=
                    PrototypeStrapOwnershipState.ReleaseRequested &&
                !interactionOwner.TryBeginStrapRelease(activeTarget))
            {
                return false;
            }

            releaseStartWorldPosition = GetAnchorPosition();
            releaseEndWorldPosition = GetReelPosition();
            releaseStartedAt = Time.time;
            ClearMediumSwingPlan(false);
            releaseQueued = false;
            bufferedAttack = false;
            bufferedMediumAimWorldPoint = Vector3.zero;
            attackUsePending = false;
            smallThrowLaunchPending = false;
            activeTarget.CancelCargoStrapCollisionObservation(
                activeCargoStrapUseSequence);
            activeCargoStrapUseSequence = 0;
            DampRedirectVelocity();
            ReleaseCount++;
            if (depleted)
            {
                interactionOwner.TrySelectWeapon(PrototypeWeaponSelection.Fists);
            }
            EnterPhase(
                ShowcaseCargoStrapPhase.Releasing,
                ShowcaseCargoStrapAnimationCue.None);
            return true;
        }

        private void UpdateRelease()
        {
            phaseProgress01 = Mathf.Clamp01(
                (Time.time - releaseStartedAt) / ReleaseDuration);
            if (phaseProgress01 < 1f)
            {
                return;
            }

            PrototypeInteractionTarget releasedTarget = activeTarget;
            if (releasedTarget != null && interactionOwner != null &&
                !interactionOwner.TryCompleteStrapRelease(releasedTarget))
            {
                interactionOwner.TryReleaseStrap();
            }
            ClearPhysicalAttachment();
            pendingTarget = null;
            clampReleased = false;
            EnterPhase(
                ShowcaseCargoStrapPhase.Stored,
                ShowcaseCargoStrapAnimationCue.None);
        }

        private void EnterPhase(
            ShowcaseCargoStrapPhase phase,
            ShowcaseCargoStrapAnimationCue cue)
        {
            if (phase != ShowcaseCargoStrapPhase.Pull)
            {
                pullMotionInitialized = false;
            }
            if (phase != ShowcaseCargoStrapPhase.SmallSpinning &&
                !(phase == ShowcaseCargoStrapPhase.Throw &&
                  smallThrowLaunchPending))
            {
                ResetSmallSpinRuntime();
            }
            Phase = phase;
            phaseStartedAt = Time.time;
            phaseProgress01 = phase == ShowcaseCargoStrapPhase.Tensioned ? 1f : 0f;
            animationStateEntered = cue == ShowcaseCargoStrapAnimationCue.None;
            AnimationCue = cue;
            AnimationCueRevision++;
            if (phase == ShowcaseCargoStrapPhase.Pull)
            {
                InitializePullMotion();
            }
            SynchronizeLocomotionPolicy();
        }

        private void SynchronizeLocomotionPolicy()
        {
            if (locomotion == null)
            {
                return;
            }

            locomotion.SetMobilityLock(
                PlayerMobilityLockReason.PlayerAction,
                BlocksPlayerMobility);
            bool mediumMovementPhase = IsMediumAttackMobilityPhase;
            locomotion.SetJumpLock(
                PlayerMobilityLockReason.PlayerAction,
                mediumMovementPhase);
            Vector3 facingDirection = mediumMovementPhase && playerTransform != null
                ? mediumSwingSnapshotTargetPoint - playerTransform.position
                : Vector3.zero;
            locomotion.SetFacingOverride(
                PlayerMobilityLockReason.PlayerAction,
                facingDirection,
                mediumMovementPhase);
        }

        private void ClearLocomotionPolicy()
        {
            if (locomotion == null)
            {
                return;
            }
            locomotion.SetMobilityLock(PlayerMobilityLockReason.PlayerAction, false);
            locomotion.SetJumpLock(PlayerMobilityLockReason.PlayerAction, false);
            locomotion.SetFacingOverride(
                PlayerMobilityLockReason.PlayerAction,
                Vector3.zero,
                false);
        }

        private bool IsCurrentRevision(uint revision)
        {
            return revision != 0 && revision == AnimationCueRevision;
        }

        private bool IsQualifiedAnimationEvent(
            uint revision,
            ShowcaseCargoStrapAnimationCue expectedCue)
        {
            if (IsCurrentRevision(revision) && AnimationCue == expectedCue &&
                animationStateEntered)
            {
                return true;
            }

            RejectedAnimationEventCount++;
            return false;
        }

        private void ObserveAttachInputRelease()
        {
            if (automaticSequence || logicalInput == null ||
                (Phase != ShowcaseCargoStrapPhase.Prepare &&
                 Phase != ShowcaseCargoStrapPhase.Rolling))
            {
                return;
            }

            if (logicalInput.Snapshot.InteractPrimaryReleased ||
                !logicalInput.Snapshot.InteractPrimaryHeld)
            {
                interactReleaseQueued = true;
                if (Phase == ShowcaseCargoStrapPhase.Rolling)
                {
                    StartThrow();
                }
            }
        }

        private void SynchronizeSmallSpinWithWeaponSelection()
        {
            if (!IsAttached || activeTarget == null ||
                activeTarget.WeightClass != PrototypeWeightClass.Light)
            {
                return;
            }

            if (Phase == ShowcaseCargoStrapPhase.SmallSpinning &&
                !IsStrapWeaponSelected)
            {
                DampRedirectVelocity();
                EnterPhase(
                    ShowcaseCargoStrapPhase.Tensioned,
                    ShowcaseCargoStrapAnimationCue.None);
            }
            else if (Phase == ShowcaseCargoStrapPhase.Tensioned &&
                     IsStrapWeaponSelected)
            {
                EnterSmallSpinning();
            }
        }

        private void UpdateSmallSpinBody()
        {
            if (activeBody == null || activeCollider == null ||
                playerTransform == null ||
                (Phase == ShowcaseCargoStrapPhase.SmallSpinning &&
                 !animationStateEntered))
            {
                return;
            }

            float deltaTime = Mathf.Max(Time.fixedDeltaTime, 0.0001f);
            UpdateSmallSpinCenter(deltaTime);
            smallSpinDynamicRadius = ResolveSmallSpinRadius();
            float authoredAngularSpeed = (Mathf.PI * 2f) / SmallSpinCycleSeconds;
            float maximumRelativeSpeed = Mathf.Max(
                1f,
                smallSpinMaximumSpeed - smallSpinCenterVelocity.magnitude);
            float accelerationLimitedAngularSpeed = Mathf.Sqrt(
                (smallSpinMaximumAcceleration * 0.75f) /
                Mathf.Max(0.1f, smallSpinDynamicRadius));
            float angularSpeed = Mathf.Min(
                authoredAngularSpeed,
                maximumRelativeSpeed / Mathf.Max(0.1f, smallSpinDynamicRadius),
                accelerationLimitedAngularSpeed);
            float headTop = ResolveSmallHeadWorldPosition().y +
                smallSpinHeadTopOffset;
            smallSpinVerticalClearance =
                (activeBody.worldCenterOfMass.y - activeCollider.bounds.extents.y) -
                headTop;
            bool overheadReady = smallSpinVerticalClearance >=
                smallSpinBodyClearance - 0.02f;
            float candidateAngle = overheadReady
                ? Mathf.Repeat(
                    smallSpinAngleRadians + (angularSpeed * deltaTime),
                    Mathf.PI * 2f)
                : smallSpinAngleRadians;
            Vector3 candidatePosition = ResolveSmallOrbitPosition(
                smallSpinCenterWorld,
                candidateAngle,
                smallSpinDynamicRadius);

            bool candidateClear = IsSmallOrbitPoseClear(
                activeBody.worldCenterOfMass,
                candidatePosition);
            Vector3 targetPosition;
            float activeAngle;
            if (candidateClear)
            {
                smallSpinAngleRadians = candidateAngle;
                smallSpinLastSafeAngleRadians = candidateAngle;
                smallSpinHasSafeAngle = true;
                smallSpinObstructed = false;
                activeAngle = candidateAngle;
                targetPosition = candidatePosition;
            }
            else
            {
                smallSpinObstructed = true;
                activeAngle = smallSpinHasSafeAngle
                    ? smallSpinLastSafeAngleRadians
                    : smallSpinAngleRadians;
                Vector3 lastSafePosition = ResolveSmallOrbitPosition(
                    smallSpinCenterWorld,
                    activeAngle,
                    smallSpinDynamicRadius);
                targetPosition = smallSpinHasSafeAngle &&
                    IsSmallOrbitPoseClear(
                        activeBody.worldCenterOfMass,
                        lastSafePosition)
                    ? lastSafePosition
                    : ResolveSmallSpinSafeHoldPosition();
                if (!IsSmallOrbitPoseClear(
                        activeBody.worldCenterOfMass,
                        targetPosition))
                {
                    targetPosition = activeBody.worldCenterOfMass;
                }
            }

            float cosine = Mathf.Cos(activeAngle);
            float sine = Mathf.Sin(activeAngle);
            Vector3 tangent = (-playerTransform.right * sine) +
                (playerTransform.forward * cosine);
            Vector3 orbitVelocity = smallSpinObstructed || !overheadReady
                ? Vector3.zero
                : tangent * angularSpeed * smallSpinDynamicRadius;
            Vector3 desiredVelocity = smallSpinCenterVelocity +
                orbitVelocity +
                ((targetPosition - activeBody.worldCenterOfMass) *
                 smallSpinTrackingResponse);
            float maximumDrivenSpeed = smallSpinMaximumSpeed +
                smallSpinCenterVelocity.magnitude;
            Vector3 desiredPlanarVelocity = Vector3.ClampMagnitude(
                Vector3.ProjectOnPlane(desiredVelocity, Vector3.up),
                maximumDrivenSpeed);
            float desiredVerticalVelocity = Mathf.Clamp(
                desiredVelocity.y,
                -maximumDrivenSpeed,
                maximumDrivenSpeed);
            float clearanceError = smallSpinBodyClearance -
                smallSpinVerticalClearance;
            if (clearanceError > 0f)
            {
                desiredVerticalVelocity = Mathf.Max(
                    desiredVerticalVelocity,
                    smallSpinCenterVelocity.y +
                        (clearanceError * smallSpinTrackingResponse * 2f));
            }
            desiredVelocity = desiredPlanarVelocity +
                (Vector3.up * desiredVerticalVelocity);

            Vector3 requestedVelocityChange = desiredVelocity -
                activeBody.linearVelocity;
            float maximumVelocityChange =
                smallSpinMaximumAcceleration * deltaTime;
            Vector3 planarVelocityChange = Vector3.ClampMagnitude(
                Vector3.ProjectOnPlane(requestedVelocityChange, Vector3.up),
                maximumVelocityChange);
            float verticalVelocityChange = Mathf.Clamp(
                requestedVelocityChange.y,
                -maximumVelocityChange,
                maximumVelocityChange);
            activeBody.linearVelocity += planarVelocityChange +
                (Vector3.up * verticalVelocityChange);
            if (smallSpinObstructed)
            {
                // The sweep predicts a solid before the next pose. Remove all
                // relative swing/lift immediately so the following physics step
                // cannot spend the remaining momentum penetrating that solid.
                // Player planar feed-forward remains, so a paused tether still
                // follows the actor without a Transform correction.
                activeBody.linearVelocity =
                    Vector3.ProjectOnPlane(smallSpinCenterVelocity, Vector3.up) +
                    (Vector3.up * Mathf.Min(0f, desiredVerticalVelocity));
            }
            activeBody.angularVelocity = smallSpinObstructed
                ? Vector3.MoveTowards(
                    activeBody.angularVelocity,
                    Vector3.zero,
                    smallSpinMaximumAcceleration * deltaTime)
                : Vector3.up * angularSpeed;
            activeBody.WakeUp();

            smallSpinVerticalClearance =
                (activeBody.worldCenterOfMass.y - activeCollider.bounds.extents.y) -
                headTop;
        }

        private void UpdateSmallSpinCenter(float deltaTime)
        {
            Vector3 requiredCenter = ResolveSmallSpinTargetCenter();
            if (!smallSpinCenterInitialized)
            {
                smallSpinCenterWorld = requiredCenter;
                smallSpinPreviousCenterWorld = requiredCenter;
                smallSpinCenterVelocity = Vector3.zero;
                smallSpinCenterInitialized = true;
            }
            else
            {
                float response = 1f - Mathf.Exp(
                    -deltaTime / Mathf.Max(0.01f, smallSpinCenterDampingSeconds));
                Vector3 nextCenter = Vector3.Lerp(
                    smallSpinCenterWorld,
                    requiredCenter,
                    response);
                nextCenter.y = Mathf.Max(nextCenter.y, requiredCenter.y);
                smallSpinPreviousCenterWorld = smallSpinCenterWorld;
                smallSpinCenterWorld = nextCenter;
                smallSpinCenterVelocity =
                    (smallSpinCenterWorld - smallSpinPreviousCenterWorld) /
                    deltaTime;
            }

        }

        private Vector3 ResolveSmallSpinTargetCenter()
        {
            Vector3 head = ResolveSmallHeadWorldPosition();
            float objectHalfHeight = activeCollider != null
                ? activeCollider.bounds.extents.y
                : 0.16f;
            return new Vector3(
                head.x,
                head.y + smallSpinHeadTopOffset + objectHalfHeight +
                    smallSpinBodyClearance + smallSpinTrackingClearanceMargin,
                head.z);
        }

        private Vector3 ResolveSmallHeadWorldPosition()
        {
            if (humanoidAnimator == null || headTransform == null)
            {
                ResolveRightHandTransform();
            }
            if (headTransform != null)
            {
                return headTransform.position;
            }
            return playerTransform != null
                ? playerTransform.position + (Vector3.up * 1.55f)
                : transform.position + (Vector3.up * 1.55f);
        }

        private float ResolveSmallSpinRadius()
        {
            if (activeCollider == null)
            {
                return smallSpinRadius;
            }
            Vector3 extents = activeCollider.bounds.extents;
            float objectRadius = Mathf.Sqrt(
                (extents.x * extents.x) + (extents.z * extents.z));
            float playerRadius = playerCollider != null
                ? Mathf.Max(
                    playerCollider.bounds.extents.x,
                    playerCollider.bounds.extents.z)
                : smallSpinHeadRadius;
            return Mathf.Max(
                smallSpinRadius,
                playerRadius + objectRadius + smallSpinBodyClearance +
                    smallSpinTrackingClearanceMargin);
        }

        private Vector3 ResolveSmallOrbitPosition(
            Vector3 center,
            float angleRadians,
            float radius)
        {
            float cosine = Mathf.Cos(angleRadians);
            float sine = Mathf.Sin(angleRadians);
            Vector3 radial = (playerTransform.right * cosine) +
                (playerTransform.forward * sine);
            return center + (radial * radius);
        }

        private Vector3 ResolveSmallSpinSafeHoldPosition()
        {
            Vector3 grip = playerTransform != null
                ? playerTransform.position + (Vector3.up * 1.25f)
                : transform.position + (Vector3.up * 1.25f);
            if (TryResolveRightHandGripWorldPosition(
                    humanoidAnimator,
                    out Vector3 resolvedGrip))
            {
                grip = resolvedGrip;
            }
            Vector3 right = playerTransform != null
                ? Vector3.ProjectOnPlane(playerTransform.right, Vector3.up).normalized
                : Vector3.right;
            Vector3 extents = activeCollider != null
                ? activeCollider.bounds.extents
                : new Vector3(0.15f, 0.16f, 0.05f);
            float objectRadius = Mathf.Sqrt(
                (extents.x * extents.x) + (extents.z * extents.z));
            return grip +
                (right * (objectRadius + smallSpinBodyClearance)) +
                (Vector3.up * (extents.y * 0.25f));
        }

        private bool IsSmallOrbitPoseClear(Vector3 origin, Vector3 targetPosition)
        {
            if (activeCollider == null)
            {
                return false;
            }

            Vector3 extents = activeCollider.bounds.extents +
                (Vector3.one * smallSpinObstaclePadding);
            Vector3 colliderCenterOffset = activeCollider.bounds.center -
                activeBody.worldCenterOfMass;
            Vector3 castOrigin = origin + colliderCenterOffset;
            Vector3 castTarget = targetPosition + colliderCenterOffset;
            Vector3 displacement = castTarget - castOrigin;
            float distance = displacement.magnitude;
            Vector3 direction = distance > 0.0001f
                ? displacement / distance
                : Vector3.zero;
            if (distance > 0.0001f)
            {
                int hitCount = Physics.BoxCastNonAlloc(
                    castOrigin,
                    extents,
                    direction,
                    visibilityHits,
                    Quaternion.identity,
                    distance,
                    ~0,
                    QueryTriggerInteraction.Ignore);
                if (hitCount >= visibilityHits.Length)
                {
                    return false;
                }
                for (int index = 0; index < hitCount; index++)
                {
                    RaycastHit hit = visibilityHits[index];
                    if (IsSmallOrbitBlockingCollider(hit.collider) &&
                        !IsDepartingSmallOrbitSupport(
                            hit.collider,
                            origin,
                            targetPosition,
                            hit.distance) &&
                        !(hit.distance <= smallSpinObstaclePadding &&
                          Vector3.Dot(hit.normal, direction) > 0f))
                    {
                        return false;
                    }
                }
            }

            int overlapCount = Physics.OverlapBoxNonAlloc(
                castTarget,
                extents,
                smallOrbitOverlapHits,
                Quaternion.identity,
                ~0,
                QueryTriggerInteraction.Ignore);
            if (overlapCount >= smallOrbitOverlapHits.Length)
            {
                return false;
            }
            for (int index = 0; index < overlapCount; index++)
            {
                if (IsSmallOrbitBlockingCollider(smallOrbitOverlapHits[index]))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsDepartingSmallOrbitSupport(
            Collider candidate,
            Vector3 origin,
            Vector3 targetPosition,
            float hitDistance)
        {
            if (candidate == null || activeCollider == null ||
                targetPosition.y <= origin.y + 0.01f ||
                hitDistance > 0.08f)
            {
                return false;
            }
            return candidate.bounds.max.y <=
                activeCollider.bounds.min.y + 0.08f;
        }

        private bool IsSmallOrbitBlockingCollider(Collider candidate)
        {
            if (candidate == null || candidate == activeCollider ||
                candidate.isTrigger || BelongsToPlayer(candidate))
            {
                return false;
            }
            if (activeTarget != null &&
                (candidate.transform == activeTarget.transform ||
                 candidate.transform.IsChildOf(activeTarget.transform)))
            {
                return false;
            }
            return true;
        }

        private void ResetSmallSpinRuntime()
        {
            smallSpinCenterInitialized = false;
            smallSpinHasSafeAngle = false;
            smallSpinObstructed = false;
            smallSpinDynamicRadius = 0f;
            smallSpinVerticalClearance = 0f;
            smallSpinCenterWorld = Vector3.zero;
            smallSpinCenterVelocity = Vector3.zero;
            smallSpinPreviousCenterWorld = Vector3.zero;
        }

        private void ResolveRightHandTransform()
        {
            Transform searchRoot = playerTransform != null
                ? playerTransform
                : transform;
            humanoidAnimator = ResolveHumanoidAnimator(searchRoot);
            rightHandTransform = humanoidAnimator != null
                ? humanoidAnimator.GetBoneTransform(HumanBodyBones.RightHand)
                : null;
            headTransform = humanoidAnimator != null
                ? humanoidAnimator.GetBoneTransform(HumanBodyBones.Head)
                : null;
        }

        private void ResolveCarabinerTransform()
        {
            Transform searchRoot = playerTransform != null
                ? playerTransform
                : transform;
            Animator animator = ResolveHumanoidAnimator(searchRoot);
            humanoidAnimator = animator;
            hipsTransform = animator != null
                ? animator.GetBoneTransform(HumanBodyBones.Hips)
                : null;
            if (hipsTransform != null && animator != null)
            {
                Vector3 authoredWorld = animator.transform.TransformPoint(
                    AuthoredCarabinerModelLocalPosition);
                carabinerHipsLocalPosition = hipsTransform.InverseTransformPoint(
                    authoredWorld);
            }
        }

        public static bool TryResolveRightHandGripWorldPosition(
            Animator animator,
            out Vector3 gripWorldPosition)
        {
            gripWorldPosition = default;
            if (animator == null || !animator.isHuman)
            {
                return false;
            }

            Transform hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            if (hand == null)
            {
                return false;
            }

            Vector3 knuckleCenter = Vector3.zero;
            int knuckleCount = 0;
            AddBonePosition(animator, HumanBodyBones.RightIndexProximal,
                ref knuckleCenter, ref knuckleCount);
            AddBonePosition(animator, HumanBodyBones.RightMiddleProximal,
                ref knuckleCenter, ref knuckleCount);
            AddBonePosition(animator, HumanBodyBones.RightRingProximal,
                ref knuckleCenter, ref knuckleCount);
            AddBonePosition(animator, HumanBodyBones.RightLittleProximal,
                ref knuckleCenter, ref knuckleCount);
            if (knuckleCount == 0)
            {
                gripWorldPosition = hand.position;
                return true;
            }

            knuckleCenter /= knuckleCount;
            gripWorldPosition = Vector3.Lerp(
                hand.position,
                knuckleCenter,
                0.58f);
            return true;
        }

        private static void AddBonePosition(
            Animator animator,
            HumanBodyBones bone,
            ref Vector3 sum,
            ref int count)
        {
            Transform transform = animator.GetBoneTransform(bone);
            if (transform == null)
            {
                return;
            }

            sum += transform.position;
            count++;
        }

        private static Animator ResolveHumanoidAnimator(Transform searchRoot)
        {
            if (searchRoot == null)
            {
                return null;
            }

            Animator[] candidates = searchRoot.GetComponentsInChildren<Animator>(true);
            foreach (Animator candidate in candidates)
            {
                if (candidate != null && candidate.isHuman)
                {
                    return candidate;
                }
            }

            return null;
        }

        private void ResolveRunnerPressureController()
        {
            if (runnerPressure != null)
            {
                return;
            }
            PrototypeInteractionController interaction =
                GetComponent<PrototypeInteractionController>();
            runnerPressure = interaction != null
                ? interaction.ShowcaseRunnerPressureController
                : null;
            if (runnerPressure == null)
            {
                GameObject[] sceneRoots = gameObject.scene.GetRootGameObjects();
                foreach (GameObject root in sceneRoots)
                {
                    ShowcaseRunnerPressureController candidate =
                        root.GetComponentInChildren<
                            ShowcaseRunnerPressureController>(true);
                    if (candidate != null)
                    {
                        runnerPressure = candidate;
                        break;
                    }
                }
            }
        }

        private void ApplyAttackImpulse(bool attackLeft)
        {
            float side = attackLeft ? -1f : 1f;
            Vector3 direction = Vector3.ProjectOnPlane(
                (playerTransform.right * side) + (playerTransform.forward * 0.35f),
                Vector3.up).normalized;
            if (activeTarget.WeightClass == PrototypeWeightClass.Light)
            {
                activeBody.linearVelocity =
                    (direction * smallSwingSpeed) + (Vector3.up * smallSwingLift);
                activeBody.angularVelocity = Vector3.up * (attackLeft ? -5f : 5f);
            }
            else
            {
                mediumSwingMotionStarted = mediumSwingPlanActive;
            }
            activeBody.WakeUp();
        }

        private void InitializePullMotion()
        {
            pullMotionInitialized = activeBody != null && activeTarget != null;
            PullDistanceReduced = 0f;
            if (!pullMotionInitialized)
            {
                return;
            }

            Vector3 center = GetReelPosition();
            pullStartWorldPosition = activeBody.worldCenterOfMass;
            Vector3 planarOffset = Vector3.ProjectOnPlane(
                activeBody.worldCenterOfMass - center,
                Vector3.up);
            pullStartPlanarRadius = planarOffset.magnitude;
            pullStartHeight = activeBody.worldCenterOfMass.y;
            pullTargetPlanarRadius = activeTarget.WeightClass ==
                PrototypeWeightClass.Light
                    ? smallPullTargetRadius
                    : ResolveMediumRearDistance();
            pullTargetPlanarRadius = Mathf.Min(
                pullStartPlanarRadius,
                pullTargetPlanarRadius);
            pullTargetHeight = activeTarget.WeightClass ==
                PrototypeWeightClass.Light
                    ? ResolveSmallSpinSafeHoldPosition().y
                    : mediumGroundCenterOfMassY;
        }

        private void UpdatePullBody()
        {
            if (!pullMotionInitialized || activeBody == null ||
                activeTarget == null)
            {
                return;
            }

            float normalized = Mathf.InverseLerp(
                pullMotionStart01,
                Mathf.Max(pullMotionStart01 + 0.01f, pullMotionEnd01),
                phaseProgress01);
            float pull01 = Mathf.SmoothStep(0f, 1f, normalized);
            Vector3 center = GetReelPosition();
            Vector3 actualOffset = Vector3.ProjectOnPlane(
                activeBody.worldCenterOfMass - center,
                Vector3.up);
            Vector3 desiredPosition;
            if (activeTarget.WeightClass == PrototypeWeightClass.Medium)
            {
                desiredPosition = Vector3.Lerp(
                    pullStartWorldPosition,
                    GetMediumRearReadyPoint(),
                    pull01);
                desiredPosition.y = Mathf.Lerp(
                    pullStartHeight,
                    pullTargetHeight,
                    pull01);
            }
            else
            {
                Vector3 radial = actualOffset.sqrMagnitude > 0.0001f
                    ? actualOffset.normalized
                    : (playerTransform != null
                        ? playerTransform.forward
                        : Vector3.forward);
                float desiredRadius = Mathf.Lerp(
                    pullStartPlanarRadius,
                    pullTargetPlanarRadius,
                    pull01);
                float desiredHeight = Mathf.Lerp(
                    pullStartHeight,
                    pullTargetHeight,
                    pull01);
                desiredPosition = center + (radial * desiredRadius);
                desiredPosition.y = desiredHeight;
            }
            Vector3 positionError = desiredPosition -
                activeBody.worldCenterOfMass;
            Vector3 desiredVelocity = Vector3.ClampMagnitude(
                positionError * pullTrackingResponse,
                pullMaximumSpeed);
            Vector3 velocityChange = desiredVelocity -
                activeBody.linearVelocity;
            velocityChange = Vector3.ClampMagnitude(
                velocityChange,
                pullMaximumAcceleration * Time.fixedDeltaTime);
            activeBody.linearVelocity += velocityChange;
            activeBody.angularVelocity *= 0.75f;
            activeBody.WakeUp();
            PullDistanceReduced = Mathf.Max(
                PullDistanceReduced,
                activeTarget.WeightClass == PrototypeWeightClass.Medium
                    ? Vector3.Distance(
                        pullStartWorldPosition,
                        activeBody.worldCenterOfMass)
                    : pullStartPlanarRadius - actualOffset.magnitude);
        }

        private float ResolveMediumRearDistance()
        {
            if (activeCollider == null || playerCollider == null)
            {
                return mediumRearMinimumDistance;
            }

            Bounds playerBounds = playerCollider.bounds;
            Bounds objectBounds = activeCollider.bounds;
            float playerRadius = Mathf.Max(
                playerBounds.extents.x,
                playerBounds.extents.z);
            float objectRadius = Mathf.Max(
                objectBounds.extents.x,
                objectBounds.extents.z);
            return Mathf.Clamp(
                playerRadius + objectRadius + mediumRearClearance,
                mediumRearMinimumDistance,
                mediumRearMaximumDistance);
        }

        private Vector3 GetMediumRearReadyPoint()
        {
            Vector3 forward = playerTransform != null
                ? Vector3.ProjectOnPlane(playerTransform.forward, Vector3.up)
                : Vector3.forward;
            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward;
            }
            forward.Normalize();
            Vector3 point = GetReelPosition() -
                (forward * Mathf.Max(
                    mediumRearMinimumDistance,
                    mediumRearDistance));
            point.y = mediumGroundCenterOfMassY;
            return point;
        }

        private void UpdateMediumRearFollower()
        {
            if (activeBody == null || activeTarget == null ||
                activeTarget.WeightClass != PrototypeWeightClass.Medium)
            {
                return;
            }

            Vector3 targetPoint = GetMediumRearReadyPoint();
            Vector3 planarError = Vector3.ProjectOnPlane(
                targetPoint - activeBody.worldCenterOfMass,
                Vector3.up);
            Vector3 feedForward = ResolvePlayerPlanarVelocity();
            Vector3 desiredPlanar = feedForward +
                (planarError * mediumRearTrackingResponse);
            if (planarError.magnitude <= mediumRearReadyTolerance)
            {
                desiredPlanar = feedForward;
            }
            ApplyMediumVelocityServo(
                desiredPlanar + (Vector3.up * activeBody.linearVelocity.y),
                mediumRearMaximumSpeed,
                mediumRearMaximumAcceleration);
            activeBody.angularVelocity = Vector3.up *
                Mathf.MoveTowards(activeBody.angularVelocity.y, 0f, 6f *
                    Time.fixedDeltaTime);

            Vector3 relativePlanarVelocity = Vector3.ProjectOnPlane(
                activeBody.linearVelocity - feedForward,
                Vector3.up);
            if (planarError.magnitude <= mediumRearReadyTolerance &&
                relativePlanarVelocity.magnitude <= 1.25f &&
                Mathf.Abs(activeBody.linearVelocity.y) <= 0.5f &&
                IsFloorSupported(activeCollider))
            {
                mediumRearStableSteps++;
            }
            else
            {
                mediumRearStableSteps = 0;
            }
            mediumRearReady = mediumRearStableSteps >= 3;
        }

        private void UpdateMediumSwingBody()
        {
            if (!mediumSwingPlanActive || !mediumSwingMotionStarted ||
                activeBody == null || playerTransform == null)
            {
                return;
            }

            float startNormalized = mediumSwingAttackLeft ? 0.4f : 8f / 22f;
            float rawProgress = Phase == ShowcaseCargoStrapPhase.MediumExtending
                ? 1f
                : Mathf.InverseLerp(startNormalized, 0.95f, phaseProgress01);
            float swing01 = Mathf.SmoothStep(0f, 1f, rawProgress);
            UpdateMediumTargetCorrection(swing01);
            mediumSwingEffectiveTetherLength = Mathf.MoveTowards(
                mediumSwingEffectiveTetherLength,
                mediumSwingDesiredTetherLength,
                mediumTetherExtensionSpeed * Time.fixedDeltaTime);

            float angle = Mathf.Lerp(
                mediumSwingStartAngleRadians,
                mediumSwingTargetAngleRadians,
                swing01);
            Vector3 radial =
                (mediumSwingForwardSnapshot * Mathf.Cos(angle)) +
                (mediumSwingRightSnapshot * Mathf.Sin(angle));
            float extension01 = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.InverseLerp(0.5f, 1f, swing01));
            float radius = Mathf.Lerp(
                mediumSwingRearRadius,
                mediumSwingEffectiveTetherLength,
                extension01);
            Vector3 desiredPoint = GetReelPosition() + (radial * radius);
            desiredPoint.y = mediumGroundCenterOfMassY +
                (Mathf.Sin(Mathf.PI * swing01) * mediumSwingArcHeight);
            float targetBlend = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.InverseLerp(0.65f, 1f, swing01));
            desiredPoint = Vector3.Lerp(
                desiredPoint,
                mediumSwingCorrectedTargetPoint,
                targetBlend);

            Vector3 error = desiredPoint - activeBody.worldCenterOfMass;
            Vector3 feedForward = ResolvePlayerPlanarVelocity();
            float closingSpeed = error.sqrMagnitude > 0.0001f
                ? Vector3.Dot(
                    activeBody.linearVelocity - feedForward,
                    error.normalized)
                : 0f;
            if (swing01 >= 0.35f &&
                error.magnitude > mediumSwingMaximumTrackingError &&
                closingSpeed <= 0.5f)
            {
                mediumSwingTrackingFailureSteps++;
            }
            else
            {
                mediumSwingTrackingFailureSteps = 0;
            }
            if (mediumSwingTrackingFailureSteps >= 8)
            {
                BeginMediumRecovery();
                return;
            }
            Vector3 desiredVelocity = Vector3.ClampMagnitude(
                error * mediumSwingTrackingResponse,
                mediumSwingMaximumSpeed) + feedForward;
            ApplyMediumVelocityServo(
                desiredVelocity,
                mediumSwingMaximumSpeed,
                mediumSwingMaximumAcceleration);
            activeBody.angularVelocity = Vector3.up *
                (mediumSwingAttackLeft ? -3f : 3f);
            activeBody.WakeUp();
            MediumSwingStepCount++;
        }

        private void UpdateMediumTargetCorrection(float swing01)
        {
            if (mediumSwingTargetCollider == null || swing01 >= 0.55f)
            {
                return;
            }

            Vector3 currentPoint = mediumSwingTargetCollider.bounds.center;
            Vector3 boundedPoint = mediumSwingSnapshotTargetPoint +
                Vector3.ClampMagnitude(
                    currentPoint - mediumSwingSnapshotTargetPoint,
                    mediumTargetCorrectionDistance);
            mediumSwingCorrectedTargetPoint = Vector3.MoveTowards(
                mediumSwingCorrectedTargetPoint,
                boundedPoint,
                mediumTargetCorrectionSpeed * Time.fixedDeltaTime);
        }

        private void ObserveMediumSwingState()
        {
            if (!mediumSwingPlanActive)
            {
                return;
            }
            if (Phase == ShowcaseCargoStrapPhase.MediumExtending &&
                (mediumSwingImpactCommitted ||
                 Time.time - mediumExtensionStartedAt >=
                    mediumExtensionDuration))
            {
                BeginMediumRecovery();
            }
        }

        private void ObserveBufferedMediumAttack()
        {
            if (!bufferedAttack ||
                Phase != ShowcaseCargoStrapPhase.Tensioned ||
                !mediumRearReady || !IsStrapWeaponSelected ||
                activeTarget == null ||
                activeTarget.WeightClass != PrototypeWeightClass.Medium)
            {
                return;
            }

            Vector3 aimPoint = bufferedMediumAimWorldPoint;
            bufferedAttack = false;
            bufferedMediumAimWorldPoint = Vector3.zero;
            if (TryCreateMediumSwingPlan(aimPoint, out bool attackLeft))
            {
                StartAttack(attackLeft);
            }
        }

        private void BeginMediumRecovery()
        {
            if (!mediumSwingPlanActive ||
                Phase == ShowcaseCargoStrapPhase.MediumRecovering)
            {
                return;
            }

            CancelActiveAttackObservation();
            mediumSwingMotionStarted = false;
            mediumRecoveryStartedAt = Time.time;
            mediumRearReady = false;
            mediumRearStableSteps = 0;
            DampRedirectVelocity();
            EnterPhase(
                ShowcaseCargoStrapPhase.MediumRecovering,
                ShowcaseCargoStrapAnimationCue.None);
        }

        private void UpdateMediumRecoveryBody()
        {
            if (!mediumSwingPlanActive || activeBody == null)
            {
                return;
            }

            Vector3 error = GetMediumRearReadyPoint() -
                activeBody.worldCenterOfMass;
            Vector3 feedForward = ResolvePlayerPlanarVelocity();
            Vector3 desiredVelocity = feedForward + Vector3.ClampMagnitude(
                error * mediumRecoveryTrackingResponse,
                mediumRecoveryMaximumSpeed);
            ApplyMediumVelocityServo(
                desiredVelocity,
                mediumRecoveryMaximumSpeed,
                mediumRecoveryMaximumAcceleration);
            activeBody.angularVelocity *= 0.7f;
            activeBody.WakeUp();
            MediumRecoveryStepCount++;

            if (error.magnitude <= mediumRearReadyTolerance &&
                (activeBody.linearVelocity - feedForward).magnitude <= 1.25f &&
                Mathf.Abs(activeBody.linearVelocity.y) <= 0.5f &&
                IsFloorSupported(activeCollider))
            {
                mediumRearStableSteps++;
            }
            else
            {
                mediumRearStableSteps = 0;
            }
            if (mediumRearStableSteps >= 3 ||
                Time.time - mediumRecoveryStartedAt >= mediumRecoveryTimeout)
            {
                ClearMediumSwingPlan(false);
                EnterPhase(
                    ShowcaseCargoStrapPhase.Tensioned,
                    ShowcaseCargoStrapAnimationCue.None);
            }
        }

        private void ApplyMediumVelocityServo(
            Vector3 desiredVelocity,
            float maximumSpeed,
            float maximumAcceleration)
        {
            desiredVelocity = Vector3.ClampMagnitude(
                desiredVelocity,
                maximumSpeed);
            Vector3 velocityChange = desiredVelocity - activeBody.linearVelocity;
            velocityChange = Vector3.ClampMagnitude(
                velocityChange,
                maximumAcceleration * Time.fixedDeltaTime);
            activeBody.linearVelocity += velocityChange;
        }

        private Vector3 ResolvePlayerPlanarVelocity()
        {
            if (playerTransform == null)
            {
                return Vector3.zero;
            }
            CharacterController controller =
                playerTransform.GetComponent<CharacterController>();
            return controller != null
                ? Vector3.ProjectOnPlane(controller.velocity, Vector3.up)
                : Vector3.zero;
        }

        private bool IsMediumSwingTargetWithinRange()
        {
            if (!mediumSwingPlanActive || mediumSwingTarget == null ||
                mediumSwingTargetCollider == null ||
                !mediumSwingTarget.gameObject.activeInHierarchy ||
                !mediumSwingTargetCollider.enabled)
            {
                return false;
            }

            Vector3 reelToTarget =
                mediumSwingTargetCollider.bounds.center - GetReelPosition();
            float planarDistance = Vector3.ProjectOnPlane(
                reelToTarget,
                Vector3.up).magnitude;
            float requiredLength = reelToTarget.magnitude +
                mediumSwingTrajectoryAllowance;
            return planarDistance <= MediumMaximumAttackRange + 0.001f &&
                requiredLength <= maximumStrapLength + 0.001f;
        }

        private void CancelActiveAttackObservation()
        {
            if (attackUsePending && activeTarget != null &&
                activeCargoStrapUseSequence != 0)
            {
                activeTarget.CancelCargoStrapCollisionObservation(
                    activeCargoStrapUseSequence);
            }
            attackUsePending = false;
            activeCargoStrapUseSequence = 0;
        }

        private void ClearMediumSwingPlan(bool dampVelocity)
        {
            bool wasActive = mediumSwingPlanActive;
            CancelActiveAttackObservation();
            mediumSwingPlanActive = false;
            mediumSwingMotionStarted = false;
            mediumSwingAttackLeft = false;
            mediumSwingImpactCommitted = false;
            activeMediumSwingSequence = 0;
            mediumSwingTarget = null;
            mediumSwingTargetCollider = null;
            mediumSwingSnapshotTargetPoint = Vector3.zero;
            mediumSwingCorrectedTargetPoint = Vector3.zero;
            mediumSwingForwardSnapshot = Vector3.zero;
            mediumSwingRightSnapshot = Vector3.zero;
            mediumSwingRearRadius = 0f;
            mediumSwingStartAngleRadians = 0f;
            mediumSwingTargetAngleRadians = 0f;
            mediumSwingDesiredTetherLength = 0f;
            mediumSwingEffectiveTetherLength = 0f;
            mediumSwingTrackingFailureSteps = 0;
            mediumSwingHitRunnerIds.Clear();
            if (wasActive && dampVelocity)
            {
                DampRedirectVelocity();
            }
        }

        public bool TryAuthorizeMediumSwingImpact(
            PrototypeInteractionTarget source,
            ShowcaseRunnerAgent runner,
            float impactSpeed,
            out uint swingSequence,
            out string sourceId)
        {
            swingSequence = 0;
            sourceId = string.Empty;
            if (!mediumSwingPlanActive || !mediumSwingMotionStarted ||
                source == null || source != activeTarget ||
                source.WeightClass != PrototypeWeightClass.Medium ||
                runner == null || runner != mediumSwingTarget ||
                impactSpeed < validCollisionSpeed ||
                activeMediumSwingSequence == 0 ||
                mediumSwingHitRunnerIds.Contains(runner.GetInstanceID()) ||
                interactionOwner == null ||
                interactionOwner.GlobalState !=
                    PrototypeGlobalInteractionState.Playable ||
                interactionOwner.StrapTarget != source ||
                !IsStrapWeaponSelected ||
                (!IsAttackActive &&
                 Phase != ShowcaseCargoStrapPhase.MediumExtending))
            {
                return false;
            }
            if (!IsMediumSwingTargetWithinRange())
            {
                return false;
            }

            swingSequence = activeMediumSwingSequence;
            sourceId = $"CHR00_STRAP:{source.RuntimeInstanceKey}";
            return true;
        }

        public void ConfirmMediumSwingImpact(
            ShowcaseRunnerAgent runner,
            uint swingSequence)
        {
            if (runner == null || swingSequence == 0 ||
                swingSequence != activeMediumSwingSequence)
            {
                return;
            }
            mediumSwingHitRunnerIds.Add(runner.GetInstanceID());
            mediumSwingImpactCommitted = true;
        }

        private void ObserveAttackCollision()
        {
            if (!attackUsePending || activeTarget == null)
            {
                return;
            }
            if (Time.time > attackCollisionExpiresAt &&
                !mediumSwingPlanActive)
            {
                activeTarget.CancelCargoStrapCollisionObservation(
                    activeCargoStrapUseSequence);
                attackUsePending = false;
                activeCargoStrapUseSequence = 0;
                return;
            }
            if (!activeTarget.TryConsumeCargoStrapCollision(
                    activeCargoStrapUseSequence,
                    out Collider other,
                    out float observedSpeed))
            {
                return;
            }

            if (other == null ||
                observedSpeed < validCollisionSpeed ||
                BelongsToPlayer(other) ||
                other.transform == activeTarget.transform ||
                other.transform.IsChildOf(activeTarget.transform))
            {
                return;
            }

            attackUsePending = false;
            if (!activeTarget.TryCommitSuccessfulCargoStrapCollision(
                    activeCargoStrapUseSequence))
            {
                RegisterFault("STRAP_DURABILITY_COMMIT_FAILED");
                activeCargoStrapUseSequence = 0;
                return;
            }
            activeCargoStrapUseSequence = 0;

            ValidStrapCollisionCount++;
            if (activeTarget.IsPlayerThrowExhausted)
            {
                DepletionCount++;
                BeginRelease(true);
            }
        }

        private void DampRedirectVelocity()
        {
            if (activeBody == null)
            {
                return;
            }
            Vector3 planar = Vector3.ProjectOnPlane(activeBody.linearVelocity, Vector3.up);
            activeBody.linearVelocity = (planar * 0.2f) +
                (Vector3.up * Mathf.Min(activeBody.linearVelocity.y, 0f));
        }

        private bool CanCommitPendingAttachment()
        {
            if (interactionOwner == null || pendingTarget == null ||
                !TryGetPhysicalTarget(
                    pendingTarget,
                    out _,
                    out Collider pendingCollider))
            {
                return false;
            }

            return
                interactionOwner.GlobalState == PrototypeGlobalInteractionState.Playable &&
                interactionOwner.HandState == PrototypeHandOwnershipState.Free &&
                interactionOwner.StrapState == PrototypeStrapOwnershipState.Stored &&
                pendingTarget.IsCargoStrapEligible &&
                pendingTarget.gameObject.activeInHierarchy &&
                IsFloorSupported(pendingCollider) &&
                HasClearPlayerAttachPath(pendingTarget, pendingCollider) &&
                Vector3.Distance(GetReelPosition(), GetAnchorPosition()) <=
                    maximumStrapLength;
        }

        private bool HasClearPlayerAttachPath(
            PrototypeInteractionTarget target,
            Collider targetCollider)
        {
            Vector3 start = GetReelPosition();
            Vector3 end = GetAnchorPosition();
            Vector3 displacement = end - start;
            float distance = displacement.magnitude;
            if (distance <= 0.05f)
            {
                return true;
            }

            int hitCount = Physics.RaycastNonAlloc(
                start,
                displacement / distance,
                visibilityHits,
                distance + 0.05f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            if (hitCount >= VisibilityHitCapacity)
            {
                return false;
            }

            float nearestDistance = float.PositiveInfinity;
            Collider nearest = null;
            for (int index = 0; index < hitCount; index++)
            {
                Collider candidate = visibilityHits[index].collider;
                if (candidate == null || BelongsToPlayer(candidate) ||
                    candidate.transform == allyTransform ||
                    (allyTransform != null &&
                     candidate.transform.IsChildOf(allyTransform)))
                {
                    continue;
                }

                if (visibilityHits[index].distance < nearestDistance)
                {
                    nearestDistance = visibilityHits[index].distance;
                    nearest = candidate;
                }
            }

            return nearest != null &&
                (nearest == targetCollider ||
                 nearest.transform == target.transform ||
                 nearest.transform.IsChildOf(target.transform));
        }

        private void CheckAnimationTimeout()
        {
            if (!IsActionLocked || Phase == ShowcaseCargoStrapPhase.Rolling ||
                Phase == ShowcaseCargoStrapPhase.SmallSpinning ||
                Time.time - phaseStartedAt <= AnimationEventTimeout)
            {
                return;
            }

            RegisterFault(animationStateEntered
                ? $"STRAP_ANIMATION_EVENT_TIMEOUT:{Phase}"
                : $"STRAP_ANIMATION_STATE_TIMEOUT:{Phase}");
            if (activeTarget != null)
            {
                BeginRelease(false);
            }
            else
            {
                CancelPendingSequence();
            }
        }

        private void CancelPendingSequence()
        {
            locomotion?.SetMobilityLock(PlayerMobilityLockReason.PlayerAction, false);
            interactionOwner?.TryReleaseStrap();
            ClearPhysicalAttachment();
            pendingTarget = null;
            automaticSequence = false;
            pendingWasAssisted = false;
            interactReleaseQueued = false;
            releaseQueued = false;
            bufferedAttack = false;
            bufferedMediumAimWorldPoint = Vector3.zero;
            attackUsePending = false;
            smallThrowLaunchPending = false;
            activeCargoStrapUseSequence = 0;
            clampReleased = false;
            EnterPhase(
                ShowcaseCargoStrapPhase.Stored,
                ShowcaseCargoStrapAnimationCue.None);
        }

        private bool ResolveAttackLeft(Vector3 aimWorldPoint)
        {
            if (playerTransform == null)
            {
                return !lastAttackWasLeft;
            }
            Vector3 toAim = Vector3.ProjectOnPlane(
                aimWorldPoint - playerTransform.position,
                Vector3.up);
            float lateral = Vector3.Dot(toAim, playerTransform.right);
            return Mathf.Abs(lateral) > 0.25f
                ? lateral < 0f
                : !lastAttackWasLeft;
        }

        private void FaceSequenceTarget()
        {
            if (playerTransform == null || ActiveTarget == null)
            {
                return;
            }

            Vector3 facingPoint = smallThrowLaunchPending
                ? smallThrowAimWorldPoint
                : mediumSwingPlanActive
                    ? mediumSwingCorrectedTargetPoint
                    : GetAnchorPosition();
            Vector3 direction = Vector3.ProjectOnPlane(
                facingPoint - playerTransform.position,
                Vector3.up);
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Quaternion desired = Quaternion.LookRotation(direction.normalized, Vector3.up);
            LastAnchorFacingErrorDegrees = Quaternion.Angle(
                playerTransform.rotation,
                desired);
            playerTransform.rotation = Quaternion.RotateTowards(
                playerTransform.rotation,
                desired,
                anchorFacingSpeedDegrees * Time.deltaTime);
            AnchorFacingStepCount++;
        }

        public bool WasRedirected(PrototypeInteractionTarget target)
        {
            return target != null && LastRedirectedTarget == target;
        }

        public void ResetToRunBaseline()
        {
            ClearLocomotionPolicy();
            ClearPhysicalAttachment();
            pendingTarget = null;
            AssistCandidate = null;
            automaticSequence = false;
            pendingWasAssisted = false;
            interactReleaseQueued = false;
            releaseQueued = false;
            bufferedAttack = false;
            bufferedMediumAimWorldPoint = Vector3.zero;
            attackImpulseCommitted = false;
            attackUsePending = false;
            clampReleased = false;
            smallThrowLaunchPending = false;
            completedSmallThrowRevision = 0;
            smallThrowAimWorldPoint = Vector3.zero;
            smallSpinAngleRadians = 0f;
            pullMotionInitialized = false;
            pullStartWorldPosition = Vector3.zero;
            pullStartPlanarRadius = 0f;
            pullStartHeight = 0f;
            pullTargetPlanarRadius = 0f;
            pullTargetHeight = 0f;
            PullDistanceReduced = 0f;
            phaseProgress01 = 0f;
            releaseStartWorldPosition = Vector3.zero;
            releaseEndWorldPosition = Vector3.zero;
            Phase = ShowcaseCargoStrapPhase.Stored;
            AnimationCue = ShowcaseCargoStrapAnimationCue.None;
            AnimationCueRevision++;
            AssistedAttachCount = 0;
            DirectAttachCount = 0;
            ReleaseCount = 0;
            RedirectStepCount = 0;
            AttackImpulseCount = 0;
            ValidStrapCollisionCount = 0;
            DepletionCount = 0;
            BufferedAttackCount = 0;
            RejectedAnimationEventCount = 0;
            AnchorFacingStepCount = 0;
            MediumSwingStepCount = 0;
            MediumRecoveryStepCount = 0;
            LastAnchorFacingErrorDegrees = 0f;
            LastRedirectedTarget = null;
            FaultCount = 0;
            LastFault = string.Empty;
            nextCargoStrapUseSequence = 0;
            activeCargoStrapUseSequence = 0;
            nextMediumSwingSequence = 0;
            activeMediumSwingSequence = 0;
            RefreshAssistCandidate();
        }

        private void SynchronizeLogicalAttachment()
        {
            if (interactionOwner == null)
            {
                ClearPhysicalAttachment();
                return;
            }

            if (interactionOwner.StrapState == PrototypeStrapOwnershipState.Stored ||
                interactionOwner.StrapTarget == null)
            {
                ClearPhysicalAttachment();
                return;
            }

            if (Phase == ShowcaseCargoStrapPhase.Stored && pendingTarget == null &&
                activeTarget == null)
            {
                RegisterFault("EXTERNAL_STRAP_COMMIT_REJECTED");
                interactionOwner.TryReleaseStrap();
                ClearPhysicalAttachment();
                return;
            }

            if (activeTarget != interactionOwner.StrapTarget)
            {
                if (!BeginPhysicalAttachment(interactionOwner.StrapTarget))
                {
                    interactionOwner.TryReleaseStrap();
                }
            }
        }

        private bool BeginPhysicalAttachment(PrototypeInteractionTarget target)
        {
            if (target == activeTarget && activeBody != null && activeCollider != null)
            {
                return true;
            }

            ClearPhysicalAttachment();
            Rigidbody body = target != null ? target.GetComponent<Rigidbody>() : null;
            Collider targetCollider = target != null ? target.GetComponent<Collider>() : null;
            if (!IsEligibleStrapTarget(target) || body == null || targetCollider == null)
            {
                RegisterFault("INVALID_STRAP_PHYSICAL_TARGET");
                return false;
            }

            activeTarget = target;
            activeBody = body;
            activeCollider = targetCollider;
            originalConstraints = body.constraints;
            originalCollisionDetection = body.collisionDetectionMode;
            originalInterpolation = body.interpolation;
            originalWasKinematic = body.isKinematic;
            originalUsedGravity = body.useGravity;
            body.isKinematic = false;
            body.useGravity = true;
            body.constraints = target.WeightClass == PrototypeWeightClass.Medium
                ? originalConstraints |
                  RigidbodyConstraints.FreezeRotationX |
                  RigidbodyConstraints.FreezeRotationZ
                : originalConstraints;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            if (target.WeightClass == PrototypeWeightClass.Medium)
            {
                mediumGroundCenterOfMassY = body.worldCenterOfMass.y;
                mediumRearDistance = ResolveMediumRearDistance();
                mediumRearReady = false;
                mediumRearStableSteps = 0;
            }
            if (playerCollider != null)
            {
                Physics.IgnoreCollision(playerCollider, activeCollider, true);
                playerCollisionIgnored = true;
            }
            return true;
        }

        private void ClearPhysicalAttachment()
        {
            ClearLocomotionPolicy();
            ResetSmallSpinRuntime();
            ClearMediumSwingPlan(false);
            pullMotionInitialized = false;
            if (activeTarget != null)
            {
                activeTarget.CancelCargoStrapCollisionObservation(
                    activeCargoStrapUseSequence);
            }
            activeCargoStrapUseSequence = 0;
            RestorePlayerCollision();
            if (activeBody != null)
            {
                activeBody.constraints = originalConstraints;
                activeBody.collisionDetectionMode = originalCollisionDetection;
                activeBody.interpolation = originalInterpolation;
                activeBody.isKinematic = originalWasKinematic;
                activeBody.useGravity = originalUsedGravity;
            }

            activeTarget = null;
            activeBody = null;
            activeCollider = null;
            mediumGroundCenterOfMassY = 0f;
            mediumRearDistance = 0f;
            mediumRearReady = false;
            mediumRearStableSteps = 0;
        }

        private void RestorePlayerCollision()
        {
            if (playerCollisionIgnored && playerCollider != null && activeCollider != null)
            {
                Physics.IgnoreCollision(playerCollider, activeCollider, false);
            }
            playerCollisionIgnored = false;
        }

        private void RefreshAssistCandidate()
        {
            AssistCandidate = null;
            if (Phase != ShowcaseCargoStrapPhase.Stored || pendingTarget != null ||
                interactionOwner == null ||
                interactionOwner.GlobalState != PrototypeGlobalInteractionState.Playable ||
                interactionOwner.HandState != PrototypeHandOwnershipState.Free ||
                interactionOwner.StrapState != PrototypeStrapOwnershipState.Stored ||
                interactionOwner.RelayState != PrototypeRelayReservationState.None ||
                playerTransform == null ||
                allyTransform == null ||
                Vector3.Distance(playerTransform.position, allyTransform.position) >
                    maximumAllyAssistRange)
            {
                return;
            }

            float bestDistance = float.PositiveInfinity;
            string bestKey = null;
            foreach (PrototypeInteractionTarget target in authoredTargets)
            {
                if (!IsEligibleStrapTarget(target) ||
                    !TryGetPhysicalTarget(target, out _, out Collider targetCollider))
                {
                    continue;
                }

                float distance = Vector3.Distance(
                    playerTransform.position + Vector3.up,
                    targetCollider.ClosestPoint(playerTransform.position + Vector3.up));
                if (distance > assistAttachRange ||
                    !IsFloorSupported(targetCollider) ||
                    !HasClearAssistPath(target, targetCollider))
                {
                    continue;
                }

                string key = target.RuntimeInstanceKey ?? string.Empty;
                if (distance < bestDistance - 0.0001f ||
                    (Mathf.Abs(distance - bestDistance) <= 0.0001f &&
                     (bestKey == null || string.CompareOrdinal(key, bestKey) < 0)))
                {
                    bestDistance = distance;
                    bestKey = key;
                    AssistCandidate = target;
                }
            }
        }

        private bool IsFloorSupported(Collider targetCollider)
        {
            Vector3 origin = new Vector3(
                targetCollider.bounds.center.x,
                targetCollider.bounds.min.y + 0.08f,
                targetCollider.bounds.center.z);
            int hitCount = Physics.RaycastNonAlloc(
                origin,
                Vector3.down,
                visibilityHits,
                0.35f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            for (int index = 0; index < hitCount; index++)
            {
                Collider candidate = visibilityHits[index].collider;
                if (candidate != null &&
                    candidate != targetCollider &&
                    !BelongsToPlayer(candidate) &&
                    !candidate.transform.IsChildOf(targetCollider.transform))
                {
                    return true;
                }
            }
            return false;
        }

        private bool HasClearAssistPath(
            PrototypeInteractionTarget target,
            Collider targetCollider)
        {
            Vector3 start = allyTransform.position + Vector3.up;
            Vector3 end = targetCollider.bounds.center;
            Vector3 displacement = end - start;
            float distance = displacement.magnitude;
            if (distance <= 0.05f)
            {
                return true;
            }

            int hitCount = Physics.RaycastNonAlloc(
                start,
                displacement / distance,
                visibilityHits,
                distance + 0.05f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            if (hitCount >= VisibilityHitCapacity)
            {
                return false;
            }

            float nearestDistance = float.PositiveInfinity;
            Collider nearest = null;
            for (int index = 0; index < hitCount; index++)
            {
                Collider candidate = visibilityHits[index].collider;
                if (candidate == null ||
                    BelongsToPlayer(candidate) ||
                    candidate.transform == allyTransform ||
                    candidate.transform.IsChildOf(allyTransform))
                {
                    continue;
                }

                if (visibilityHits[index].distance < nearestDistance)
                {
                    nearestDistance = visibilityHits[index].distance;
                    nearest = candidate;
                }
            }

            return nearest != null &&
                (nearest == targetCollider ||
                 nearest.transform == target.transform ||
                 nearest.transform.IsChildOf(target.transform));
        }

        private bool BelongsToPlayer(Collider candidate)
        {
            return candidate != null && playerTransform != null &&
                (candidate.transform == playerTransform ||
                 candidate.transform.IsChildOf(playerTransform));
        }

        private static bool IsEligibleStrapTarget(PrototypeInteractionTarget target)
        {
            return target != null && target.IsCargoStrapEligible;
        }

        private static bool TryGetPhysicalTarget(
            PrototypeInteractionTarget target,
            out Rigidbody body,
            out Collider targetCollider)
        {
            body = target != null ? target.GetComponent<Rigidbody>() : null;
            targetCollider = target != null ? target.GetComponent<Collider>() : null;
            return body != null && targetCollider != null;
        }

        private Vector3 GetReelPosition()
        {
            if (hipsTransform == null)
            {
                ResolveCarabinerTransform();
            }
            if (hipsTransform != null)
            {
                return hipsTransform.TransformPoint(carabinerHipsLocalPosition);
            }
            return playerTransform != null
                ? playerTransform.position
                : transform.position;
        }

        private Vector3 GetAnchorPosition()
        {
            PrototypeInteractionTarget target = ActiveTarget;
            if (target == null)
            {
                return GetReelPosition();
            }
            if (target.StrapClampSocket != null)
            {
                return target.StrapClampSocket.position;
            }
            Collider targetCollider = activeCollider != null
                ? activeCollider
                : target.GetComponent<Collider>();
            return targetCollider != null
                ? targetCollider.bounds.center
                : target.transform.position;
        }

        private void SetLineVisible(bool visible)
        {
            // Presentation owns every renderer write. Kept as a no-op so old
            // serialized scenes and binary-compatible Configure calls remain valid.
        }

        private void RegisterFault(string reason)
        {
            FaultCount++;
            LastFault = reason ?? string.Empty;
        }
    }
}
