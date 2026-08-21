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
        Charging,
        SmallSpinning,
        AttackLeft,
        AttackRight,
        Slam,
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
        AttackRight,
        Slam
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
            bool isSmallSpinObstructed = false,
            float charge01 = -1f,
            ShowcaseCargoAttackType resolvedAttackType =
                ShowcaseCargoAttackType.None,
            ShowcaseCargoAttackResolutionReason resolutionReason =
                ShowcaseCargoAttackResolutionReason.None)
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
            HasCharge = charge01 >= 0f;
            Charge01 = Mathf.Clamp01(charge01);
            ResolvedAttackType = resolvedAttackType;
            ResolutionReason = resolutionReason;
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
        public bool HasCharge { get; }
        public float Charge01 { get; }
        public ShowcaseCargoAttackType ResolvedAttackType { get; }
        public ShowcaseCargoAttackResolutionReason ResolutionReason { get; }
    }

    [DefaultExecutionOrder(-40)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseCargoStrapController : MonoBehaviour
    {
        public static readonly Vector3 AuthoredCarabinerModelLocalPosition =
            new Vector3(0.13f, 0.94f, 0.14f);

        private const int VisibilityHitCapacity = 32;
        private const int SmallOrbitOverlapCapacity = 32;
        private const int MediumSweepCastCapacity = 48;
        private const int MediumSweepOverlapCapacity = 48;
        private const float AttackBufferStart01 = 0.65f;
        private const float ReleaseDuration = 0.2f;
        private const float AnimationEventTimeout = 4f;
        private const float AttackCollisionGraceSeconds = 0.4f;
        private const float SmallSpinCycleSeconds = 12f / 30f;
        private const float LegacyPowerCharge01 = 0.5f;
        private const float ChargeMovementScale = 0.75f;
        private const float FullChargeMovementScale = 0.5f;
        private const float FullChargeAutoFireDelay = 0.65f;
        private const float MediumControlRecoveryDelay = 0.4f;
        private const int AttackPreviewMinimumCapacity = 3;
        private const float SlamFloorSeparation = 0.025f;
        private const float ExplicitSlamMinimumDownwardDrop = 0.65f;
        private const float ExplicitSlamMinimumDownwardDot = 0.35f;
        private const float ExplicitSlamFloorTolerance = 0.35f;
        private const float MediumSweepCastSkin = 0.015f;
        private const float MediumSweepFloorTolerance = 0.08f;
        private const float MediumSweepMinimumTravel = 0.002f;

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

        [SerializeField, Min(0f)]
        private float mediumTargetLeadSeconds = 0.42f;

        [SerializeField, Min(0f)]
        private float mediumTargetLeadMaximumDistance = 1.4f;

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

        [SerializeField, Min(0.1f)]
        private float mediumChargeDuration = 1f;

        [SerializeField, Min(0.1f)]
        private float slamSelectionRange = 2.4f;

        [SerializeField, Min(0.1f)]
        private float slamDirectRadius = 0.65f;

        [SerializeField, Min(0.2f)]
        private float slamOuterRadius = 1.75f;

        [SerializeField, Min(0.1f)]
        private float slamApexHeight = 2.2f;

        [SerializeField, Min(0.1f)]
        private float slamTrackingResponse = 28f;

        [SerializeField, Min(1f)]
        private float slamMaximumAcceleration = 180f;

        [SerializeField, Min(0.1f)]
        private float slamMaximumSpeed = 24f;

        [SerializeField, Min(0.1f)]
        private float slamMinimumDownwardTravel = 0.45f;

        [SerializeField, Min(0f)]
        private float attackClearancePadding = 0.08f;

        private readonly RaycastHit[] visibilityHits =
            new RaycastHit[VisibilityHitCapacity];
        private readonly Collider[] smallOrbitOverlapHits =
            new Collider[SmallOrbitOverlapCapacity];
        private readonly RaycastHit[] mediumSweepCastHits =
            new RaycastHit[MediumSweepCastCapacity];
        private readonly Collider[] mediumSweepOverlapHits =
            new Collider[MediumSweepOverlapCapacity];
        private readonly int[] mediumSweepOverlapOrder =
            new int[MediumSweepOverlapCapacity];
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
        private bool cargoStrapUseCommitted;
        private bool depletedReleaseQueued;
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
        private readonly Dictionary<int, ShowcaseCargoSweepHitAuthorization>
            mediumSwingPendingHits = new();
        private bool mediumSwingPlanActive;
        private bool mediumSwingMotionStarted;
        private bool mediumSwingAttackLeft;
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
        private Vector3 mediumSweepPreviousCenter;
        private Quaternion mediumSweepPreviousRotation;
        private Vector3 mediumSweepPreviousHalfExtents;
        private bool mediumSweepPoseInitialized;
        private ShowcaseRunnerAgent primarySweptVictim;
        private float mediumSweepInitialEnergy01;
        private float mediumSweepReachScale = 1f;
        private float remainingImpactEnergy01;
        private int hitOrdinal;
        private int mediumSweepMaximumTargets;
        private bool swingTerminated;
        private ShowcaseCargoSweepTerminationReason mediumSweepTerminationReason;
        private Collider mediumSweepTerminationObstacle;
        private Vector3 mediumSweepLastPathDirection;
        private float mediumRecoveryStartedAt;
        private float mediumExtensionStartedAt;
        private float mediumGroundCenterOfMassY;
        private float mediumGroundSupportOffsetY;
        private float mediumRearDistance;
        private bool mediumRearReady;
        private int mediumRearStableSteps;
        private bool pullMotionInitialized;
        private Vector3 pullStartWorldPosition;
        private float pullStartPlanarRadius;
        private float pullStartHeight;
        private float pullTargetPlanarRadius;
        private float pullTargetHeight;
        private float chargeStartedAt;
        private float fullChargeReachedAt = -1f;
        private Vector3 chargeAimWorldPoint;
        private ShowcaseRunnerAgent chargeResolvedTarget;
        private Collider chargeResolvedTargetCollider;
        private Vector3 chargeResolvedTargetPoint;
        private ShowcaseCargoAttackResolution chargeResolution;
        private ShowcaseCargoChargeProfile activeChargeProfile;
        private ShowcaseCargoAttackType activeAttackType;
        private ShowcaseCargoAttackResolution lastAttackResolution;
        private float attackReleasedAt = -1f;
        private float attackAccelerationAt = -1f;
        private float attackImpactAt = -1f;
        private float controlRecoveryAt = -1f;
        private bool mediumControlRecovered = true;
        private bool slamMotionStarted;
        private bool slamDescending;
        private bool slamImpactResolved;
        private float slamMotionStartedAt;
        private float slamPreviousCenterY;
        private float slamDownwardTravel;
        private Vector3 slamStartWorldPosition;
        private Vector3 slamApexWorldPosition;
        private Vector3 slamTargetWorldPosition;
        private Vector3 slamPreviousColliderCenter;
        private bool slamColliderSweepInitialized;

        public ShowcaseCargoStrapPhase Phase { get; private set; } =
            ShowcaseCargoStrapPhase.Stored;

        public ShowcaseCargoStrapAnimationCue AnimationCue { get; private set; }

        public uint AnimationCueRevision { get; private set; }

        public event Action<PrototypeInteractionTarget> AssistedAttachCompleted;

        public event Action<PrototypeInteractionTarget> StrapExtendStarted;

        public event Action<PrototypeInteractionTarget> StrapLockCommitted;

        public event Action<PrototypeInteractionTarget> StrapTensionCommitted;

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
            Phase == ShowcaseCargoStrapPhase.Charging ||
            Phase == ShowcaseCargoStrapPhase.SmallSpinning ||
            Phase == ShowcaseCargoStrapPhase.AttackLeft ||
            Phase == ShowcaseCargoStrapPhase.AttackRight ||
            Phase == ShowcaseCargoStrapPhase.Slam ||
            Phase == ShowcaseCargoStrapPhase.MediumExtending ||
            Phase == ShowcaseCargoStrapPhase.MediumRecovering;

        public bool IsAttackActive => Phase == ShowcaseCargoStrapPhase.AttackLeft ||
            Phase == ShowcaseCargoStrapPhase.AttackRight ||
            Phase == ShowcaseCargoStrapPhase.Slam;

        public bool IsCharging => Phase == ShowcaseCargoStrapPhase.Charging;

        public bool IsSweepAttackActive =>
            Phase == ShowcaseCargoStrapPhase.AttackLeft ||
            Phase == ShowcaseCargoStrapPhase.AttackRight;

        public bool IsSmallSpinning => Phase == ShowcaseCargoStrapPhase.SmallSpinning;

        public bool IsMediumSwinging => mediumSwingPlanActive &&
            IsSweepAttackActive && mediumSwingMotionStarted;

        public bool IsMediumRecovering =>
            Phase == ShowcaseCargoStrapPhase.MediumRecovering;

        public bool IsMediumAttackMobilityPhase =>
            activeTarget != null &&
            activeTarget.WeightClass == PrototypeWeightClass.Medium &&
            (Phase == ShowcaseCargoStrapPhase.Charging ||
             (mediumSwingPlanActive &&
              (Phase == ShowcaseCargoStrapPhase.AttackLeft ||
               Phase == ShowcaseCargoStrapPhase.AttackRight ||
               Phase == ShowcaseCargoStrapPhase.Slam ||
               Phase == ShowcaseCargoStrapPhase.MediumExtending ||
               Phase == ShowcaseCargoStrapPhase.MediumRecovering)));

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

        public ShowcaseRunnerAgent PrimarySweptVictim => primarySweptVictim;

        public float RemainingImpactEnergy01 => remainingImpactEnergy01;

        public int MediumSweepHitCount => hitOrdinal;

        public int MediumSweepMaximumTargets => mediumSweepMaximumTargets;

        public bool IsMediumSweepTerminated => swingTerminated;

        public ShowcaseCargoSweepTerminationReason MediumSweepTerminationReason =>
            mediumSweepTerminationReason;

        public Collider MediumSweepTerminationObstacle =>
            mediumSweepTerminationObstacle;

        public Vector3 MediumSweepLastPathDirection =>
            mediumSweepLastPathDirection;

        public int MediumSwingStepCount { get; private set; }

        public int MediumRecoveryStepCount { get; private set; }

        public float PullDistanceReduced { get; private set; }

        public bool AttackBufferWindowOpen => IsSweepAttackActive &&
            phaseProgress01 >= AttackBufferStart01;

        public float Charge01 => IsCharging
            ? Mathf.Clamp01(
                (Time.time - chargeStartedAt) /
                Mathf.Max(0.1f, mediumChargeDuration))
            : activeChargeProfile.Charge01;

        public ShowcaseCargoChargeProfile ActiveAttackChargeProfile =>
            IsCharging
                ? ShowcaseCargoChargeModel.Evaluate(Charge01)
                : activeChargeProfile;

        public ShowcaseCargoAttackType ActiveAttackType => activeAttackType;

        public ShowcaseCargoAttackResolution CurrentAttackResolution =>
            IsCharging ? chargeResolution : lastAttackResolution;

        public ShowcaseRunnerAgent ChargeResolvedTarget =>
            chargeResolvedTarget;

        public Vector3 ChargeResolvedTargetPoint =>
            chargeResolvedTargetPoint;

        public float SlamDirectRadius => Mathf.Max(0.1f, slamDirectRadius);

        public float SlamOuterRadius => Mathf.Max(
            SlamDirectRadius + 0.1f,
            slamOuterRadius);

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

        public int ChargeCancelCount { get; private set; }

        public int AttackReleaseCount { get; private set; }

        public int SlamImpactCount { get; private set; }

        public float LastReleaseToObjectAccelerationSeconds { get; private set; } =
            -1f;

        public float LastReleaseToContactSeconds { get; private set; } = -1f;

        public float LastImpactToControlRecoverySeconds { get; private set; } = -1f;

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
                smallSpinObstructed,
                IsCharging ? Charge01 : -1f,
                CurrentAttackResolution.AttackType,
                CurrentAttackResolution.Reason);

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
                ShowcaseCargoStrapPhase.Charging =>
                    Mathf.Lerp(0.7f, 1f, Charge01),
                ShowcaseCargoStrapPhase.AttackLeft => 1f,
                ShowcaseCargoStrapPhase.AttackRight => 1f,
                ShowcaseCargoStrapPhase.Slam => 1f,
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
            UpdateChargeState();
            ObserveAttackCollision();
            ObserveMediumSwingState();
            ObserveBufferedMediumAttack();
            UpdateMediumControlRecovery();
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
                    (!mediumSwingMotionStarted ||
                     activeAttackType ==
                        ShowcaseCargoAttackType.OverheadSlam) &&
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
                    UpdateMediumAttackBody();
                }
                else if (Phase == ShowcaseCargoStrapPhase.MediumRecovering)
                {
                    UpdateMediumRecoveryBody();
                }
                else if (Phase == ShowcaseCargoStrapPhase.Tensioned ||
                         Phase == ShowcaseCargoStrapPhase.Charging ||
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
            ResetChargeRuntime();
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
            if (IsCharging)
            {
                return TryCancelCharge();
            }

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
            if (IsCharging)
            {
                TryCancelCharge();
                return;
            }

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

            if (IsSweepAttackActive)
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

            if (IsAttackActive || IsCharging)
            {
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
                !mediumRearReady)
            {
                LastMediumAttackRejection = !mediumRearReady
                    ? "MEDIUM_REAR_NOT_READY"
                    : "MEDIUM_TARGET_PLAN_REJECTED";
                return false;
            }

            ShowcaseCargoChargeProfile powerProfile =
                ShowcaseCargoChargeModel.Evaluate(LegacyPowerCharge01);
            if (!TryResolveLegacyMediumAttack(
                    aimWorldPoint,
                    out ShowcaseCargoAttackResolution resolution,
                    out ShowcaseRunnerAgent target,
                    out Collider targetCollider,
                    out Vector3 targetPoint) ||
                !TryCreateMediumAttackPlan(
                    resolution,
                    powerProfile,
                    target,
                    targetCollider,
                    targetPoint,
                    true))
            {
                if (string.IsNullOrEmpty(LastMediumAttackRejection))
                {
                    LastMediumAttackRejection = ResolveAttackRejectionText(
                        resolution);
                }
                return false;
            }

            CommitAttackRelease(powerProfile, resolution);
            StartResolvedAttack(resolution.AttackType);
            return true;
        }

        private bool TryResolveLegacyMediumAttack(
            Vector3 aimWorldPoint,
            out ShowcaseCargoAttackResolution resolution,
            out ShowcaseRunnerAgent target,
            out Collider targetCollider,
            out Vector3 targetPoint)
        {
            resolution = default;
            target = null;
            targetCollider = null;
            targetPoint = aimWorldPoint;
            ResolveRunnerPressureController();
            float maximumTargetRange = MediumMaximumAttackRange;
            if (runnerPressure == null || maximumTargetRange <= 0f ||
                !runnerPressure.TryResolveCargoStrapTarget(
                    aimWorldPoint,
                    maximumTargetRange,
                    out target,
                    out targetPoint,
                    out _))
            {
                LastMediumAttackRejection = "MEDIUM_TARGET_NOT_RESOLVED";
                return false;
            }

            targetCollider = target.GetComponent<Collider>();
            if (targetCollider == null || !targetCollider.enabled)
            {
                LastMediumAttackRejection = "MEDIUM_TARGET_COLLIDER_MISSING";
                return false;
            }

            bool attackLeft = ResolveAttackLeft(targetPoint);
            resolution = new ShowcaseCargoAttackResolution(
                attackLeft
                    ? ShowcaseCargoAttackType.SweepLeft
                    : ShowcaseCargoAttackType.SweepRight,
                ShowcaseCargoAttackResolutionReason.PreferredSweep);
            return true;
        }

        public bool TryBeginCharge(Vector3 aimWorldPoint)
        {
            LastMediumAttackRejection = string.Empty;
            if (Phase == ShowcaseCargoStrapPhase.SmallSpinning)
            {
                return TryBeginAttack(aimWorldPoint);
            }

            if (IsCharging)
            {
                return TryUpdateChargeAim(aimWorldPoint);
            }

            if (Phase != ShowcaseCargoStrapPhase.Tensioned ||
                !IsStrapWeaponSelected || activeTarget == null ||
                !activeTarget.gameObject.activeInHierarchy ||
                activeTarget.WeightClass != PrototypeWeightClass.Medium)
            {
                LastMediumAttackRejection =
                    "MEDIUM_CHARGE_STATE_NOT_READY";
                return false;
            }

            chargeStartedAt = Time.time;
            fullChargeReachedAt = -1f;
            chargeAimWorldPoint = aimWorldPoint;
            activeChargeProfile = ShowcaseCargoChargeModel.Evaluate(0f);
            activeAttackType = ShowcaseCargoAttackType.None;
            mediumControlRecovered = false;
            chargeResolution = default;
            RefreshChargeResolution(aimWorldPoint);
            EnterPhase(
                ShowcaseCargoStrapPhase.Charging,
                ShowcaseCargoStrapAnimationCue.None);
            return true;
        }

        public bool TryUpdateChargeAim(Vector3 aimWorldPoint)
        {
            if (!IsCharging)
            {
                return false;
            }

            chargeAimWorldPoint = aimWorldPoint;
            RefreshChargeResolution(aimWorldPoint);
            return true;
        }

        public bool TryReleaseCharge(Vector3 aimWorldPoint)
        {
            if (!IsCharging)
            {
                return false;
            }

            chargeAimWorldPoint = aimWorldPoint;
            RefreshChargeResolution(aimWorldPoint);
            ShowcaseCargoChargeProfile profile =
                ShowcaseCargoChargeModel.Evaluate(Charge01);
            ShowcaseCargoAttackResolution resolution = chargeResolution;
            if (!resolution.IsAccepted ||
                !TryCreateMediumAttackPlan(
                    resolution,
                    profile,
                    chargeResolvedTarget,
                    chargeResolvedTargetCollider,
                    chargeResolvedTargetPoint))
            {
                LastMediumAttackRejection = ResolveAttackRejectionText(
                    resolution);
                CancelCharge(false);
                return false;
            }

            CommitAttackRelease(profile, resolution);
            StartResolvedAttack(resolution.AttackType);
            return true;
        }

        public bool TryCancelCharge()
        {
            return CancelCharge(true);
        }

        private bool CancelCharge(bool countCancel)
        {
            if (!IsCharging)
            {
                return false;
            }

            if (countCancel)
            {
                ChargeCancelCount++;
            }
            ResetChargeRuntime();
            DampRedirectVelocity();
            EnterPhase(
                ShowcaseCargoStrapPhase.Tensioned,
                ShowcaseCargoStrapAnimationCue.None);
            return true;
        }

        private void CommitAttackRelease(
            ShowcaseCargoChargeProfile profile,
            ShowcaseCargoAttackResolution resolution)
        {
            activeChargeProfile = profile;
            activeAttackType = resolution.AttackType;
            lastAttackResolution = resolution;
            attackReleasedAt = Time.time;
            attackAccelerationAt = -1f;
            attackImpactAt = -1f;
            controlRecoveryAt = -1f;
            mediumControlRecovered = false;
            LastReleaseToObjectAccelerationSeconds = -1f;
            LastReleaseToContactSeconds = -1f;
            LastImpactToControlRecoverySeconds = -1f;
            AttackReleaseCount++;
            ResetChargeRuntime(false);
        }

        private bool TryCreateMediumAttackPlan(
            ShowcaseCargoAttackResolution resolution,
            ShowcaseCargoChargeProfile profile,
            ShowcaseRunnerAgent target,
            Collider targetCollider,
            Vector3 targetPoint,
            bool preserveLegacyReach = false)
        {
            bool hasTrackedTarget = target != null;
            bool hasValidTrackedCollider = hasTrackedTarget &&
                targetCollider != null && targetCollider.enabled;
            if (!resolution.IsAccepted || activeBody == null ||
                playerTransform == null ||
                (hasTrackedTarget && !hasValidTrackedCollider) ||
                (!hasTrackedTarget && targetCollider != null))
            {
                return false;
            }

            float maximumTargetRange = MediumMaximumAttackRange;
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

            bool slamAttack = resolution.AttackType ==
                ShowcaseCargoAttackType.OverheadSlam;
            Vector3 plannedTargetPoint = targetPoint;
            float plannedTargetDistance = targetDistance;
            mediumSweepReachScale = preserveLegacyReach
                ? 1f
                : ShowcaseCargoSweepEnergyModel.ResolveSweepReachScale(profile);
            if (!slamAttack && targetDistance > 0.001f)
            {
                float minimumReach = Mathf.Min(
                    targetDistance,
                    mediumSwingMinimumRange);
                plannedTargetDistance = Mathf.Lerp(
                    minimumReach,
                    targetDistance,
                    mediumSweepReachScale);
                Vector3 verticalOffset = Vector3.Project(
                    targetPoint - reel,
                    Vector3.up);
                plannedTargetPoint = reel +
                    (planarTargetOffset.normalized * plannedTargetDistance) +
                    verticalOffset;
            }

            bool attackLeft = resolution.AttackType ==
                ShowcaseCargoAttackType.SweepLeft;
            mediumSwingPlanActive = true;
            mediumSwingMotionStarted = false;
            mediumSwingAttackLeft = attackLeft;
            mediumSwingTarget = target;
            mediumSwingTargetCollider = targetCollider;
            mediumSwingSnapshotTargetPoint = plannedTargetPoint;
            mediumSwingCorrectedTargetPoint = plannedTargetPoint;
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
                plannedTargetDistance + mediumSwingTrajectoryAllowance,
                Mathf.Max(mediumSwingMinimumRange, mediumSwingRearRadius),
                maximumStrapLength - 0.05f);
            mediumSwingEffectiveTetherLength = Mathf.Clamp(
                CurrentStrapLength,
                mediumSwingMinimumRange,
                mediumSwingDesiredTetherLength);
            mediumSwingTrackingFailureSteps = 0;
            activeMediumSwingSequence = ++nextMediumSwingSequence;
            mediumSwingHitRunnerIds.Clear();
            mediumSwingPendingHits.Clear();
            mediumRearReady = false;
            mediumRearStableSteps = 0;
            activeChargeProfile = profile;
            activeAttackType = resolution.AttackType;
            lastAttackResolution = resolution;
            InitializeMediumSweepRuntime(profile, resolution.AttackType);
            InitializeSlamPlan(plannedTargetPoint);
            return true;
        }

        private void InitializeMediumSweepRuntime(
            ShowcaseCargoChargeProfile profile,
            ShowcaseCargoAttackType attackType)
        {
            mediumSweepPreviousCenter = Vector3.zero;
            mediumSweepPreviousRotation = Quaternion.identity;
            mediumSweepPreviousHalfExtents = Vector3.zero;
            mediumSweepPoseInitialized = false;
            primarySweptVictim = null;
            mediumSweepInitialEnergy01 = 0f;
            mediumSweepReachScale = 1f;
            remainingImpactEnergy01 = 0f;
            hitOrdinal = 0;
            mediumSweepMaximumTargets = 0;
            swingTerminated = false;
            mediumSweepTerminationReason =
                ShowcaseCargoSweepTerminationReason.None;
            mediumSweepTerminationObstacle = null;
            mediumSweepLastPathDirection = Vector3.zero;
            if (attackType == ShowcaseCargoAttackType.OverheadSlam)
            {
                return;
            }

            ShowcaseCargoSweepEnergyState energyState =
                ShowcaseCargoSweepEnergyModel.ResolveInitialState(profile);
            mediumSweepInitialEnergy01 = energyState.InitialEnergy01;
            remainingImpactEnergy01 = energyState.InitialEnergy01;
            mediumSweepMaximumTargets = energyState.MaximumTargets;
        }

        public bool TryBuildAttackTrajectoryPreview(
            Vector3[] points,
            out int pointCount)
        {
            pointCount = 0;
            if (!IsCharging || points == null ||
                points.Length < AttackPreviewMinimumCapacity ||
                !chargeResolution.IsAccepted || activeBody == null ||
                playerTransform == null)
            {
                return false;
            }

            int requestedCount = Mathf.Clamp(points.Length, 3, 18);
            if (chargeResolution.AttackType ==
                ShowcaseCargoAttackType.OverheadSlam)
            {
                Vector3 start = activeBody.worldCenterOfMass;
                Vector3 end = ResolveSlamTargetPosition(
                    chargeResolvedTargetPoint);
                Vector3 apex = ResolveSlamApex(start, end);
                for (int index = 0; index < requestedCount; index++)
                {
                    float t = index / (float)(requestedCount - 1);
                    float inverse = 1f - t;
                    points[index] =
                        (inverse * inverse * start) +
                        (2f * inverse * t * apex) +
                        (t * t * end);
                }
                pointCount = requestedCount;
                return true;
            }

            bool attackLeft = chargeResolution.AttackType ==
                ShowcaseCargoAttackType.SweepLeft;
            for (int index = 0; index < requestedCount; index++)
            {
                float t = index / (float)(requestedCount - 1);
                points[index] = ResolvePredictedSweepPoint(
                    attackLeft,
                    chargeResolvedTargetPoint,
                    t);
            }
            pointCount = requestedCount;
            return true;
        }

        private void UpdateChargeState()
        {
            if (!IsCharging)
            {
                return;
            }

            phaseProgress01 = Charge01;
            activeChargeProfile = ShowcaseCargoChargeModel.Evaluate(Charge01);
            RefreshChargeResolution(chargeAimWorldPoint);
            SynchronizeLocomotionPolicy();
            if (Charge01 < 1f)
            {
                fullChargeReachedAt = -1f;
                return;
            }

            if (fullChargeReachedAt < 0f)
            {
                fullChargeReachedAt = Time.time;
                return;
            }

            if (Time.time - fullChargeReachedAt >= FullChargeAutoFireDelay)
            {
                TryReleaseCharge(chargeAimWorldPoint);
            }
        }

        private void UpdateMediumControlRecovery()
        {
            if (mediumControlRecovered || attackImpactAt < 0f ||
                controlRecoveryAt < 0f || Time.time < controlRecoveryAt)
            {
                return;
            }

            mediumControlRecovered = true;
            LastImpactToControlRecoverySeconds = Mathf.Max(
                0f,
                Time.time - attackImpactAt);
            SynchronizeLocomotionPolicy();
        }

        private void MarkAttackImpact()
        {
            if (attackImpactAt >= 0f)
            {
                return;
            }

            attackImpactAt = Time.time;
            LastReleaseToContactSeconds = attackReleasedAt >= 0f
                ? Mathf.Max(0f, attackImpactAt - attackReleasedAt)
                : -1f;
            controlRecoveryAt = attackImpactAt + MediumControlRecoveryDelay;
            mediumControlRecovered = false;
        }

        private void ResetChargeRuntime(bool clearActiveProfile = true)
        {
            chargeStartedAt = 0f;
            fullChargeReachedAt = -1f;
            chargeAimWorldPoint = Vector3.zero;
            chargeResolvedTarget = null;
            chargeResolvedTargetCollider = null;
            chargeResolvedTargetPoint = Vector3.zero;
            chargeResolution = default;
            if (clearActiveProfile)
            {
                activeChargeProfile = ShowcaseCargoChargeModel.Evaluate(0f);
                activeAttackType = ShowcaseCargoAttackType.None;
            }
        }

        private void RefreshChargeResolution(Vector3 aimWorldPoint)
        {
            ResolveRunnerPressureController();
            chargeResolvedTarget = null;
            chargeResolvedTargetCollider = null;
            chargeResolvedTargetPoint = aimWorldPoint;

            ShowcaseCargoChargeProfile chargeProfile =
                ShowcaseCargoChargeModel.Evaluate(Charge01);
            Vector3 freeAimPoint = ResolveFreeAimAttackPoint(aimWorldPoint);

            ShowcaseRunnerAgent primaryTarget = null;
            Vector3 primaryTargetPoint = aimWorldPoint;
            bool hasPrimaryTarget = runnerPressure != null &&
                runnerPressure.TryResolveCargoStrapTarget(
                    aimWorldPoint,
                    MediumMaximumAttackRange,
                    out primaryTarget,
                    out primaryTargetPoint,
                    out _);
            Collider primaryCollider = hasPrimaryTarget && primaryTarget != null
                ? primaryTarget.GetComponent<Collider>()
                : null;
            if (primaryCollider == null || !primaryCollider.enabled)
            {
                hasPrimaryTarget = false;
                primaryTarget = null;
                primaryCollider = null;
            }
            else
            {
                primaryTargetPoint = ResolvePredictedRunnerTargetPoint(
                    primaryTarget,
                    primaryCollider.bounds.center,
                    chargeProfile,
                    false);
            }

            TryResolveNearestSlamTarget(
                aimWorldPoint,
                out ShowcaseRunnerAgent nearbyTarget,
                out Collider nearbyCollider,
                out Vector3 nearbyPoint,
                out float nearbyDistance);
            bool hasNearbySlamTarget = nearbyTarget != null &&
                nearbyCollider != null && nearbyCollider.enabled &&
                nearbyDistance <= slamSelectionRange;
            if (hasNearbySlamTarget)
            {
                nearbyPoint = ResolvePredictedRunnerTargetPoint(
                    nearbyTarget,
                    nearbyCollider.bounds.center,
                    chargeProfile,
                    true);
            }

            Vector3 sweepPoint = hasPrimaryTarget
                ? primaryTargetPoint
                : freeAimPoint;
            Vector3 slamPoint = hasNearbySlamTarget
                ? nearbyPoint
                : freeAimPoint;

            bool floorAim = IsFloorAim(aimWorldPoint);
            bool downwardAim = IsDeliberateDownwardAim(
                aimWorldPoint,
                floorAim);
            bool leftClear = IsPredictedSweepPathClear(true, sweepPoint);
            bool rightClear = IsPredictedSweepPathClear(false, sweepPoint);
            bool overheadClear = IsOverheadPathClear(slamPoint);
            chargeResolution = ShowcaseCargoAttackResolver.Resolve(
                new ShowcaseCargoAttackResolverInput(
                    aimWorldPoint,
                    sweepPoint,
                    playerTransform != null
                        ? playerTransform.position
                        : transform.position,
                    playerTransform != null
                        ? playerTransform.forward
                        : transform.forward,
                    playerTransform != null
                        ? playerTransform.right
                        : transform.right,
                    hasPrimaryTarget,
                    leftClear,
                    rightClear,
                    overheadClear,
                    floorAim,
                    downwardAim,
                    nearbyDistance,
                    slamSelectionRange,
                    !lastAttackWasLeft));

            bool resolvedSlam = chargeResolution.AttackType ==
                ShowcaseCargoAttackType.OverheadSlam;
            if (resolvedSlam)
            {
                chargeResolvedTarget = hasNearbySlamTarget
                    ? nearbyTarget
                    : null;
                chargeResolvedTargetCollider = hasNearbySlamTarget
                    ? nearbyCollider
                    : null;
                chargeResolvedTargetPoint = slamPoint;
            }
            else
            {
                chargeResolvedTarget = hasPrimaryTarget
                    ? primaryTarget
                    : null;
                chargeResolvedTargetCollider = hasPrimaryTarget
                    ? primaryCollider
                    : null;
                chargeResolvedTargetPoint = sweepPoint;
            }
        }

        private void TryResolveNearestSlamTarget(
            Vector3 aimWorldPoint,
            out ShowcaseRunnerAgent target,
            out Collider targetCollider,
            out Vector3 targetPoint,
            out float distance)
        {
            target = null;
            targetCollider = null;
            targetPoint = aimWorldPoint;
            distance = float.PositiveInfinity;
            if (runnerPressure == null)
            {
                return;
            }

            foreach (ShowcaseRunnerAgent candidate in
                runnerPressure.TargetableRunners)
            {
                if (candidate == null || candidate.IsDead ||
                    !candidate.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Collider candidateCollider = candidate.GetComponent<Collider>();
                if (candidateCollider == null || !candidateCollider.enabled)
                {
                    continue;
                }

                Vector3 reelClosest = candidateCollider.ClosestPoint(
                    GetReelPosition());
                float reelDistance = Vector3.ProjectOnPlane(
                    reelClosest - GetReelPosition(),
                    Vector3.up).magnitude;
                if (reelDistance > MediumMaximumAttackRange + 0.001f)
                {
                    continue;
                }

                Vector3 closest = candidateCollider.ClosestPoint(aimWorldPoint);
                float candidateDistance = Vector3.ProjectOnPlane(
                    closest - aimWorldPoint,
                    Vector3.up).magnitude;
                if (candidateDistance >= distance)
                {
                    continue;
                }

                target = candidate;
                targetCollider = candidateCollider;
                targetPoint = candidateCollider.bounds.center;
                distance = candidateDistance;
            }
        }

        private Vector3 ResolveFreeAimAttackPoint(Vector3 aimWorldPoint)
        {
            Vector3 reel = GetReelPosition();
            Vector3 planarOffset = Vector3.ProjectOnPlane(
                aimWorldPoint - reel,
                Vector3.up);
            Vector3 fallbackForward = playerTransform != null
                ? Vector3.ProjectOnPlane(playerTransform.forward, Vector3.up)
                : Vector3.forward;
            if (fallbackForward.sqrMagnitude <= 0.0001f)
            {
                fallbackForward = Vector3.forward;
            }
            fallbackForward.Normalize();

            Vector3 direction = planarOffset.sqrMagnitude > 0.0001f
                ? planarOffset.normalized
                : fallbackForward;
            float maximumDistance = Mathf.Max(
                0.1f,
                MediumMaximumAttackRange);
            float minimumDistance = Mathf.Min(
                maximumDistance,
                Mathf.Max(0.1f, mediumSwingMinimumRange));
            float distance = Mathf.Clamp(
                planarOffset.magnitude,
                minimumDistance,
                maximumDistance);
            Vector3 result = reel + direction * distance;
            result.y = mediumGroundCenterOfMassY;
            return result;
        }

        private Vector3 ResolvePredictedRunnerTargetPoint(
            ShowcaseRunnerAgent runner,
            Vector3 currentPoint,
            ShowcaseCargoChargeProfile profile,
            bool overheadSlam,
            float leadScale = 1f)
        {
            if (runner == null)
            {
                return ClampAttackTargetPoint(currentPoint);
            }

            float startupSeconds = 0.16f /
                Mathf.Max(0.1f, profile.AnimationSpeedMultiplier);
            float travelSeconds = Mathf.Max(0f, mediumTargetLeadSeconds) /
                Mathf.Max(0.1f, profile.ObjectSpeedMultiplier);
            if (overheadSlam)
            {
                travelSeconds += 0.08f;
            }
            Vector3 predictedOffset =
                Vector3.ProjectOnPlane(
                    runner.EstimatedPlanarVelocity,
                    Vector3.up) *
                (startupSeconds + travelSeconds) *
                Mathf.Clamp01(leadScale);
            predictedOffset = Vector3.ClampMagnitude(
                predictedOffset,
                Mathf.Max(0f, mediumTargetLeadMaximumDistance));
            return ClampAttackTargetPoint(currentPoint + predictedOffset);
        }

        private Vector3 ClampAttackTargetPoint(Vector3 targetPoint)
        {
            Vector3 reel = GetReelPosition();
            Vector3 planarOffset = Vector3.ProjectOnPlane(
                targetPoint - reel,
                Vector3.up);
            float maximumDistance = Mathf.Max(
                0.1f,
                MediumMaximumAttackRange);
            if (planarOffset.magnitude <= maximumDistance ||
                planarOffset.sqrMagnitude <= 0.0001f)
            {
                return targetPoint;
            }

            Vector3 result = reel +
                planarOffset.normalized * maximumDistance;
            result.y = targetPoint.y;
            return result;
        }

        private bool IsFloorAim(Vector3 aimWorldPoint)
        {
            if (playerTransform == null)
            {
                return false;
            }

            Vector3 planar = Vector3.ProjectOnPlane(
                aimWorldPoint - playerTransform.position,
                Vector3.up);
            float floorHeight = mediumGroundCenterOfMassY -
                ResolveMediumSupportOffsetY();
            return aimWorldPoint.y <=
                       floorHeight + ExplicitSlamFloorTolerance &&
                planar.magnitude <= slamSelectionRange + 0.75f;
        }

        private bool IsDeliberateDownwardAim(
            Vector3 aimWorldPoint,
            bool floorAim)
        {
            if (!floorAim)
            {
                return false;
            }

            Vector3 toAim = aimWorldPoint - GetReelPosition();
            float distance = toAim.magnitude;
            if (distance <= 0.001f ||
                -toAim.y < ExplicitSlamMinimumDownwardDrop)
            {
                return false;
            }

            return Vector3.Dot(toAim / distance, Vector3.down) >=
                ExplicitSlamMinimumDownwardDot;
        }

        private bool IsPredictedSweepPathClear(
            bool attackLeft,
            Vector3 targetPoint)
        {
            if (activeBody == null || activeCollider == null ||
                playerTransform == null)
            {
                return false;
            }

            Vector3 previous = activeBody.worldCenterOfMass;
            const int segmentCount = 8;
            for (int index = 1; index <= segmentCount; index++)
            {
                float t = index / (float)segmentCount;
                Vector3 current = ResolvePredictedSweepPoint(
                    attackLeft,
                    targetPoint,
                    t);
                if (!IsAttackSegmentClear(previous, current, true))
                {
                    return false;
                }
                previous = current;
            }
            return true;
        }

        private Vector3 ResolvePredictedSweepPoint(
            bool attackLeft,
            Vector3 targetPoint,
            float normalized01)
        {
            float t = Mathf.Clamp01(normalized01);
            Vector3 forward = Vector3.ProjectOnPlane(
                playerTransform.forward,
                Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(
                playerTransform.right,
                Vector3.up).normalized;
            Vector3 reel = GetReelPosition();
            Vector3 targetOffset = Vector3.ProjectOnPlane(
                targetPoint - reel,
                Vector3.up);
            Vector3 targetDirection = targetOffset.sqrMagnitude > 0.0001f
                ? targetOffset.normalized
                : forward;
            float targetAngle = Mathf.Atan2(
                Vector3.Dot(targetDirection, right),
                Vector3.Dot(targetDirection, forward));
            float startAngle = attackLeft ? -Mathf.PI : Mathf.PI;
            float angle = Mathf.Lerp(startAngle, targetAngle, t);
            float rearRadius = Mathf.Max(
                mediumRearMinimumDistance,
                Vector3.ProjectOnPlane(
                    activeBody.worldCenterOfMass - reel,
                    Vector3.up).magnitude);
            float targetRadius = Mathf.Clamp(
                targetOffset.magnitude + mediumSwingTrajectoryAllowance,
                Mathf.Max(mediumSwingMinimumRange, rearRadius),
                maximumStrapLength - 0.05f);
            float extension01 = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.InverseLerp(0.5f, 1f, t));
            float radius = Mathf.Lerp(rearRadius, targetRadius, extension01);
            Vector3 radial =
                (forward * Mathf.Cos(angle)) +
                (right * Mathf.Sin(angle));
            Vector3 point = reel + (radial * radius);
            point.y = mediumGroundCenterOfMassY +
                (Mathf.Sin(Mathf.PI * t) * mediumSwingArcHeight);
            float targetBlend = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.InverseLerp(0.65f, 1f, t));
            return Vector3.Lerp(point, targetPoint, targetBlend);
        }

        private bool IsOverheadPathClear(Vector3 targetPoint)
        {
            if (activeBody == null || activeCollider == null)
            {
                return false;
            }

            Vector3 start = activeBody.worldCenterOfMass;
            Vector3 end = ResolveSlamTargetPosition(targetPoint);
            Vector3 apex = ResolveSlamApex(start, end);
            return IsAttackSegmentClear(start, apex, false) &&
                IsAttackSegmentClear(apex, end, true);
        }

        private bool IsAttackSegmentClear(
            Vector3 start,
            Vector3 end,
            bool allowFloor)
        {
            Vector3 displacement = end - start;
            float distance = displacement.magnitude;
            if (distance <= 0.01f)
            {
                return true;
            }

            Vector3 colliderExtents = activeCollider.bounds.extents;
            float floorSupportInset = Mathf.Clamp(
                colliderExtents.y * 0.2f,
                0.08f,
                0.16f);
            Vector3 halfExtents = new Vector3(
                colliderExtents.x + attackClearancePadding,
                Mathf.Max(0.05f, colliderExtents.y - floorSupportInset),
                colliderExtents.z + attackClearancePadding);
            int hitCount = Physics.BoxCastNonAlloc(
                start,
                halfExtents,
                displacement / distance,
                visibilityHits,
                activeBody.rotation,
                distance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            if (hitCount >= VisibilityHitCapacity)
            {
                return false;
            }

            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = visibilityHits[index];
                Collider candidate = hit.collider;
                if (candidate == null || candidate.isTrigger ||
                    BelongsToPlayer(candidate) ||
                    candidate == activeCollider ||
                    candidate.transform == activeTarget.transform ||
                    candidate.transform.IsChildOf(activeTarget.transform) ||
                    candidate.GetComponentInParent<ShowcaseRunnerAgent>() != null)
                {
                    continue;
                }
                if (allowFloor && hit.normal.y >= 0.55f)
                {
                    continue;
                }
                return false;
            }
            return true;
        }

        private Vector3 ResolveSlamTargetPosition(Vector3 targetPoint)
        {
            Vector3 result = targetPoint;
            float supportOffsetY = ResolveMediumSupportOffsetY();
            float fallbackFloorHeight = mediumGroundCenterOfMassY -
                supportOffsetY;
            result.y = fallbackFloorHeight + supportOffsetY +
                SlamFloorSeparation;
            if (activeCollider == null)
            {
                return result;
            }

            Vector3 origin = targetPoint + (Vector3.up * 2.5f);
            int hitCount = Physics.RaycastNonAlloc(
                origin,
                Vector3.down,
                visibilityHits,
                6f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            float nearest = float.PositiveInfinity;
            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = visibilityHits[index];
                Collider candidate = hit.collider;
                if (candidate == null || candidate.isTrigger ||
                    BelongsToPlayer(candidate) || candidate == activeCollider ||
                    candidate.GetComponentInParent<ShowcaseRunnerAgent>() != null ||
                    hit.normal.y < 0.55f || hit.distance >= nearest)
                {
                    continue;
                }
                nearest = hit.distance;
                result.y = hit.point.y + supportOffsetY +
                    SlamFloorSeparation;
            }
            return result;
        }

        private float ResolveMediumSupportOffsetY()
        {
            if (mediumGroundSupportOffsetY > 0.001f)
            {
                return mediumGroundSupportOffsetY;
            }

            if (activeBody != null && activeCollider != null)
            {
                return Mathf.Max(
                    0.01f,
                    activeBody.worldCenterOfMass.y -
                    activeCollider.bounds.min.y);
            }

            return 0.01f;
        }

        private Vector3 ResolveSlamApex(Vector3 start, Vector3 end)
        {
            Vector3 apex = Vector3.Lerp(start, end, 0.5f);
            apex.y = Mathf.Max(start.y, end.y) + slamApexHeight;
            return apex;
        }

        private void InitializeSlamPlan(Vector3 targetPoint)
        {
            slamMotionStarted = false;
            slamDescending = false;
            slamImpactResolved = false;
            slamMotionStartedAt = 0f;
            slamDownwardTravel = 0f;
            slamStartWorldPosition = Vector3.zero;
            slamApexWorldPosition = Vector3.zero;
            slamTargetWorldPosition = Vector3.zero;
            slamPreviousColliderCenter = Vector3.zero;
            slamColliderSweepInitialized = false;
            if (activeAttackType != ShowcaseCargoAttackType.OverheadSlam ||
                activeBody == null)
            {
                return;
            }

            slamStartWorldPosition = activeBody.worldCenterOfMass;
            slamTargetWorldPosition = ResolveSlamTargetPosition(targetPoint);
            slamApexWorldPosition = ResolveSlamApex(
                slamStartWorldPosition,
                slamTargetWorldPosition);
            slamPreviousCenterY = slamStartWorldPosition.y;
            if (activeCollider != null)
            {
                slamPreviousColliderCenter = activeCollider.bounds.center;
                slamColliderSweepInitialized = true;
            }
        }

        private static string ResolveAttackRejectionText(
            ShowcaseCargoAttackResolution resolution)
        {
            return resolution.Reason switch
            {
                ShowcaseCargoAttackResolutionReason.NoTarget =>
                    "MEDIUM_TARGET_NOT_RESOLVED",
                ShowcaseCargoAttackResolutionReason.AllPathsBlocked =>
                    "MEDIUM_ALL_ATTACK_PATHS_BLOCKED",
                _ => "MEDIUM_TARGET_PLAN_REJECTED"
            };
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
            StrapLockCommitted?.Invoke(activeTarget);
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
            ShowcaseCargoStrapAnimationCue expected = Phase switch
            {
                ShowcaseCargoStrapPhase.AttackLeft =>
                    ShowcaseCargoStrapAnimationCue.AttackLeft,
                ShowcaseCargoStrapPhase.AttackRight =>
                    ShowcaseCargoStrapAnimationCue.AttackRight,
                ShowcaseCargoStrapPhase.Slam =>
                    ShowcaseCargoStrapAnimationCue.Slam,
                _ => ShowcaseCargoStrapAnimationCue.None
            };
            if (!IsAttackActive || attackImpulseCommitted ||
                expected == ShowcaseCargoStrapAnimationCue.None ||
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
            attackAccelerationAt = Time.time;
            LastReleaseToObjectAccelerationSeconds = attackReleasedAt >= 0f
                ? Mathf.Max(0f, attackAccelerationAt - attackReleasedAt)
                : -1f;
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
                ShowcaseCargoStrapPhase.Slam =>
                    ShowcaseCargoStrapAnimationCue.Slam,
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
                case ShowcaseCargoStrapPhase.Slam:
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
            StrapExtendStarted?.Invoke(target);
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
            bool commitsTension = !IsTensioned;
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
            if (commitsTension)
            {
                StrapTensionCommitted?.Invoke(completedTarget);
            }
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
            StartResolvedAttack(
                attackLeft
                    ? ShowcaseCargoAttackType.SweepLeft
                    : ShowcaseCargoAttackType.SweepRight);
        }

        private void StartResolvedAttack(ShowcaseCargoAttackType attackType)
        {
            bool attackLeft = attackType == ShowcaseCargoAttackType.SweepLeft;
            if (attackType == ShowcaseCargoAttackType.SweepLeft ||
                attackType == ShowcaseCargoAttackType.SweepRight)
            {
                lastAttackWasLeft = attackLeft;
            }
            activeAttackType = attackType;
            attackImpulseCommitted = false;
            cargoStrapUseCommitted = false;
            depletedReleaseQueued = false;
            bufferedAttack = false;
            bufferedMediumAimWorldPoint = Vector3.zero;
            releaseQueued = false;
            if (attackType == ShowcaseCargoAttackType.OverheadSlam)
            {
                EnterPhase(
                    ShowcaseCargoStrapPhase.Slam,
                    ShowcaseCargoStrapAnimationCue.Slam);
                return;
            }

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
            if (depletedReleaseQueued)
            {
                BeginRelease(true);
            }
            else if (releaseQueued)
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
            ResetChargeRuntime();
            ClearMediumSwingPlan(false);
            releaseQueued = false;
            depletedReleaseQueued = false;
            bufferedAttack = false;
            bufferedMediumAimWorldPoint = Vector3.zero;
            attackUsePending = false;
            cargoStrapUseCommitted = false;
            depletedReleaseQueued = false;
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
            releasedTarget?.FinalizeDeferredCargoStrapDepletion();
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
            bool chargeActive = IsCharging;
            bool directedControl = chargeActive || mediumMovementPhase;
            locomotion.SetMovementSpeedScale(
                PlayerMobilityLockReason.PlayerAction,
                ResolveChargeMovementScale(),
                chargeActive);
            locomotion.SetSprintLock(
                PlayerMobilityLockReason.PlayerAction,
                chargeActive);
            locomotion.SetJumpLock(
                PlayerMobilityLockReason.PlayerAction,
                chargeActive || mediumMovementPhase);
            Vector3 facingPoint = chargeActive
                ? chargeResolvedTargetPoint
                : mediumSwingSnapshotTargetPoint;
            Vector3 facingDirection = directedControl && playerTransform != null
                ? facingPoint - playerTransform.position
                : Vector3.zero;
            locomotion.SetFacingOverride(
                PlayerMobilityLockReason.PlayerAction,
                facingDirection,
                directedControl && facingDirection.sqrMagnitude > 0.0001f);
        }

        private float ResolveChargeMovementScale()
        {
            if (!IsCharging)
            {
                return ChargeMovementScale;
            }

            float fullChargeProgress = Mathf.InverseLerp(
                ShowcaseCargoChargeModel.FullLowerThreshold01,
                1f,
                Charge01);
            return Mathf.Lerp(
                ChargeMovementScale,
                FullChargeMovementScale,
                Mathf.SmoothStep(0f, 1f, fullChargeProgress));
        }

        private void ClearLocomotionPolicy()
        {
            if (locomotion == null)
            {
                return;
            }
            locomotion.SetMobilityLock(PlayerMobilityLockReason.PlayerAction, false);
            locomotion.SetJumpLock(PlayerMobilityLockReason.PlayerAction, false);
            locomotion.SetMovementSpeedScale(
                PlayerMobilityLockReason.PlayerAction,
                1f,
                false);
            locomotion.SetSprintLock(
                PlayerMobilityLockReason.PlayerAction,
                false);
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
                if (activeAttackType == ShowcaseCargoAttackType.OverheadSlam &&
                    mediumSwingMotionStarted)
                {
                    slamMotionStarted = true;
                    slamMotionStartedAt = Time.time;
                    slamDescending = false;
                    slamImpactResolved = false;
                    slamDownwardTravel = 0f;
                    slamPreviousCenterY = activeBody.worldCenterOfMass.y;
                    if (activeCollider != null)
                    {
                        slamPreviousColliderCenter =
                            activeCollider.bounds.center;
                        slamColliderSweepInitialized = true;
                    }
                    Vector3 initialDirection =
                        slamApexWorldPosition - activeBody.worldCenterOfMass;
                    if (initialDirection.sqrMagnitude > 0.001f)
                    {
                        activeBody.linearVelocity = initialDirection.normalized *
                            (6.5f * activeChargeProfile.ObjectSpeedMultiplier);
                    }
                    activeBody.angularVelocity = Vector3.zero;
                }
                else if (mediumSwingMotionStarted)
                {
                    CaptureMediumSweepPose();
                }
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

        private void UpdateMediumAttackBody()
        {
            if (activeAttackType == ShowcaseCargoAttackType.OverheadSlam)
            {
                UpdateSlamBody();
                return;
            }

            UpdateMediumSwingBody();
        }

        private void UpdateSlamBody()
        {
            if (!mediumSwingPlanActive || !mediumSwingMotionStarted ||
                !slamMotionStarted || activeBody == null)
            {
                return;
            }

            float speedMultiplier = Mathf.Max(
                0.1f,
                activeChargeProfile.ObjectSpeedMultiplier);
            float riseDuration = 0.22f / speedMultiplier;
            float fallDuration = 0.38f / speedMultiplier;
            float elapsed = Mathf.Max(0f, Time.time - slamMotionStartedAt);
            if (mediumSwingTarget != null &&
                mediumSwingTargetCollider != null && elapsed < riseDuration)
            {
                float totalFlightDuration = riseDuration + fallDuration;
                float remainingLeadScale = totalFlightDuration > 0.001f
                    ? Mathf.Clamp01(
                        (totalFlightDuration - elapsed) /
                        totalFlightDuration)
                    : 0f;
                Vector3 currentPoint = ResolvePredictedRunnerTargetPoint(
                    mediumSwingTarget,
                    mediumSwingTargetCollider.bounds.center,
                    activeChargeProfile,
                    true,
                    remainingLeadScale);
                Vector3 boundedPoint = mediumSwingSnapshotTargetPoint +
                    Vector3.ClampMagnitude(
                        currentPoint - mediumSwingSnapshotTargetPoint,
                        mediumTargetCorrectionDistance);
                mediumSwingCorrectedTargetPoint = Vector3.MoveTowards(
                    mediumSwingCorrectedTargetPoint,
                    boundedPoint,
                    mediumTargetCorrectionSpeed * Time.fixedDeltaTime);
                slamTargetWorldPosition = Vector3.MoveTowards(
                    slamTargetWorldPosition,
                    ResolveSlamTargetPosition(
                        mediumSwingCorrectedTargetPoint),
                    mediumTargetCorrectionSpeed * Time.fixedDeltaTime);
                slamApexWorldPosition = ResolveSlamApex(
                    slamStartWorldPosition,
                    slamTargetWorldPosition);
            }
            Vector3 desiredPoint;
            if (elapsed < riseDuration)
            {
                float rise01 = Mathf.SmoothStep(
                    0f,
                    1f,
                    elapsed / Mathf.Max(0.01f, riseDuration));
                desiredPoint = Vector3.Lerp(
                    slamStartWorldPosition,
                    slamApexWorldPosition,
                    rise01);
                slamDescending = false;
            }
            else
            {
                float fall01 = Mathf.Clamp01(
                    (elapsed - riseDuration) /
                    Mathf.Max(0.01f, fallDuration));
                float acceleratedFall = Mathf.Pow(fall01, 1.35f);
                desiredPoint = Vector3.Lerp(
                    slamApexWorldPosition,
                    slamTargetWorldPosition,
                    acceleratedFall);
                if (fall01 >= 1f)
                {
                    desiredPoint = slamTargetWorldPosition;
                }
                desiredPoint.y = Mathf.Max(
                    desiredPoint.y,
                    slamTargetWorldPosition.y);
                slamDescending = true;
            }

            if (slamDescending)
            {
                float currentY = activeBody.worldCenterOfMass.y;
                slamDownwardTravel += Mathf.Max(
                    0f,
                    slamPreviousCenterY - currentY);
                slamPreviousCenterY = currentY;
            }
            else
            {
                slamPreviousCenterY = activeBody.worldCenterOfMass.y;
            }

            Vector3 feedForward = ResolvePlayerPlanarVelocity();
            Vector3 error = desiredPoint - activeBody.worldCenterOfMass;
            Vector3 desiredVelocity = Vector3.ClampMagnitude(
                error * slamTrackingResponse,
                slamMaximumSpeed * speedMultiplier) + feedForward;
            ApplyMediumVelocityServo(
                desiredVelocity,
                slamMaximumSpeed * speedMultiplier,
                slamMaximumAcceleration * speedMultiplier);
            activeBody.angularVelocity = Vector3.MoveTowards(
                activeBody.angularVelocity,
                Vector3.zero,
                14f * Time.fixedDeltaTime);
            activeBody.WakeUp();
            TryResolveSlamHurtboxContact();
            MediumSwingStepCount++;
        }

        private void UpdateMediumSwingBody()
        {
            if (!mediumSwingPlanActive || !mediumSwingMotionStarted ||
                activeBody == null || playerTransform == null)
            {
                return;
            }
            if (swingTerminated || ResolveMediumSweepContacts())
            {
                BeginMediumRecovery();
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
                mediumSwingMaximumSpeed *
                    activeChargeProfile.ObjectSpeedMultiplier) + feedForward;
            float energyMotionScale =
                ShowcaseCargoSweepEnergyModel.ResolveMotionScale(
                    remainingImpactEnergy01,
                    mediumSweepInitialEnergy01);
            ApplyMediumVelocityServo(
                desiredVelocity,
                mediumSwingMaximumSpeed *
                    activeChargeProfile.ObjectSpeedMultiplier *
                    energyMotionScale,
                mediumSwingMaximumAcceleration *
                    activeChargeProfile.ObjectSpeedMultiplier *
                    energyMotionScale);
            activeBody.angularVelocity = Vector3.up *
                (mediumSwingAttackLeft ? -3f : 3f);
            activeBody.WakeUp();
            MediumSwingStepCount++;
        }

        private void CaptureMediumSweepPose()
        {
            mediumSweepPoseInitialized = TryGetMediumSweepBoxPose(
                out mediumSweepPreviousCenter,
                out mediumSweepPreviousRotation,
                out mediumSweepPreviousHalfExtents);
        }

        private bool ResolveMediumSweepContacts()
        {
            if (activeAttackType == ShowcaseCargoAttackType.OverheadSlam ||
                !mediumSwingPlanActive || !mediumSwingMotionStarted ||
                activeBody == null || activeCollider == null ||
                activeMediumSwingSequence == 0)
            {
                return false;
            }

            if (!TryGetMediumSweepBoxPose(
                    out Vector3 currentCenter,
                    out Quaternion currentRotation,
                    out Vector3 currentHalfExtents))
            {
                return false;
            }
            if (!mediumSweepPoseInitialized)
            {
                mediumSweepPreviousCenter = currentCenter;
                mediumSweepPreviousRotation = currentRotation;
                mediumSweepPreviousHalfExtents = currentHalfExtents;
                mediumSweepPoseInitialized = true;
                return false;
            }

            Vector3 displacement = currentCenter - mediumSweepPreviousCenter;
            float travelDistance = displacement.magnitude;
            Vector3 pathDirection = travelDistance >= MediumSweepMinimumTravel
                ? displacement / travelDistance
                : Vector3.ProjectOnPlane(
                    activeBody.linearVelocity,
                    Vector3.up).normalized;
            if (pathDirection.sqrMagnitude <= 0.001f)
            {
                pathDirection = mediumSweepLastPathDirection.sqrMagnitude > 0.001f
                    ? mediumSweepLastPathDirection
                    : mediumSwingForwardSnapshot;
            }
            pathDirection = Vector3.ProjectOnPlane(
                pathDirection,
                Vector3.up).normalized;
            if (pathDirection.sqrMagnitude <= 0.001f)
            {
                pathDirection = playerTransform != null
                    ? Vector3.ProjectOnPlane(
                        playerTransform.forward,
                        Vector3.up).normalized
                    : Vector3.forward;
            }
            mediumSweepLastPathDirection = pathDirection;

            float sweptSpeed = travelDistance > 0f
                ? travelDistance / Mathf.Max(0.001f, Time.fixedDeltaTime)
                : 0f;
            float impactSpeed = Mathf.Max(
                validCollisionSpeed,
                sweptSpeed,
                Vector3.ProjectOnPlane(
                    activeBody.linearVelocity,
                    Vector3.up).magnitude);

            if (travelDistance >= MediumSweepMinimumTravel)
            {
                Vector3 castHalfExtents = Vector3.Max(
                    mediumSweepPreviousHalfExtents,
                    currentHalfExtents);
                int hitCount = Physics.BoxCastNonAlloc(
                    mediumSweepPreviousCenter,
                    castHalfExtents,
                    displacement / travelDistance,
                    mediumSweepCastHits,
                    mediumSweepPreviousRotation,
                    travelDistance + MediumSweepCastSkin,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore);
                SortMediumSweepCastHits(hitCount);
                for (int index = 0; index < hitCount && !swingTerminated; index++)
                {
                    RaycastHit hit = mediumSweepCastHits[index];
                    ProcessMediumSweepCollider(
                        hit.collider,
                        hit.point,
                        hit.normal,
                        pathDirection,
                        impactSpeed);
                }
            }

            if (!swingTerminated)
            {
                int overlapCount = Physics.OverlapBoxNonAlloc(
                    currentCenter,
                    currentHalfExtents +
                        (Vector3.one * MediumSweepCastSkin),
                    mediumSweepOverlapHits,
                    currentRotation,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore);
                SortMediumSweepOverlapHits(
                    overlapCount,
                    mediumSweepPreviousCenter,
                    pathDirection);
                for (int orderIndex = 0;
                     orderIndex < overlapCount && !swingTerminated;
                     orderIndex++)
                {
                    Collider candidate = mediumSweepOverlapHits[
                        mediumSweepOverlapOrder[orderIndex]];
                    Vector3 point = candidate != null
                        ? candidate.ClosestPoint(currentCenter)
                        : currentCenter;
                    ProcessMediumSweepCollider(
                        candidate,
                        point,
                        Vector3.zero,
                        pathDirection,
                        impactSpeed);
                }
            }

            mediumSweepPreviousCenter = currentCenter;
            mediumSweepPreviousRotation = currentRotation;
            mediumSweepPreviousHalfExtents = currentHalfExtents;
            return swingTerminated;
        }

        private bool TryGetMediumSweepBoxPose(
            out Vector3 center,
            out Quaternion rotation,
            out Vector3 halfExtents)
        {
            if (activeCollider == null || !activeCollider.enabled)
            {
                center = Vector3.zero;
                rotation = Quaternion.identity;
                halfExtents = Vector3.zero;
                return false;
            }

            if (activeCollider is BoxCollider boxCollider)
            {
                Transform boxTransform = boxCollider.transform;
                Vector3 scale = boxTransform.lossyScale;
                scale = new Vector3(
                    Mathf.Abs(scale.x),
                    Mathf.Abs(scale.y),
                    Mathf.Abs(scale.z));
                center = boxTransform.TransformPoint(boxCollider.center);
                rotation = boxTransform.rotation;
                halfExtents = Vector3.Scale(
                    boxCollider.size * 0.5f,
                    scale);
            }
            else
            {
                Bounds bounds = activeCollider.bounds;
                center = bounds.center;
                rotation = Quaternion.identity;
                halfExtents = bounds.extents;
            }

            halfExtents = new Vector3(
                Mathf.Max(0.01f, halfExtents.x),
                Mathf.Max(0.01f, halfExtents.y),
                Mathf.Max(0.01f, halfExtents.z));
            return true;
        }

        private void SortMediumSweepCastHits(int hitCount)
        {
            int count = Mathf.Min(hitCount, mediumSweepCastHits.Length);
            for (int index = 1; index < count; index++)
            {
                RaycastHit value = mediumSweepCastHits[index];
                int insertion = index - 1;
                while (insertion >= 0 &&
                       mediumSweepCastHits[insertion].distance > value.distance)
                {
                    mediumSweepCastHits[insertion + 1] =
                        mediumSweepCastHits[insertion];
                    insertion--;
                }
                mediumSweepCastHits[insertion + 1] = value;
            }
        }

        private void SortMediumSweepOverlapHits(
            int hitCount,
            Vector3 pathOrigin,
            Vector3 pathDirection)
        {
            int count = Mathf.Min(hitCount, mediumSweepOverlapOrder.Length);
            for (int index = 0; index < count; index++)
            {
                mediumSweepOverlapOrder[index] = index;
            }
            for (int index = 1; index < count; index++)
            {
                int value = mediumSweepOverlapOrder[index];
                float valueDistance = ResolveMediumSweepPathDistance(
                    mediumSweepOverlapHits[value],
                    pathOrigin,
                    pathDirection);
                int insertion = index - 1;
                while (insertion >= 0 &&
                       ResolveMediumSweepPathDistance(
                           mediumSweepOverlapHits[
                               mediumSweepOverlapOrder[insertion]],
                           pathOrigin,
                           pathDirection) > valueDistance)
                {
                    mediumSweepOverlapOrder[insertion + 1] =
                        mediumSweepOverlapOrder[insertion];
                    insertion--;
                }
                mediumSweepOverlapOrder[insertion + 1] = value;
            }
        }

        private static float ResolveMediumSweepPathDistance(
            Collider candidate,
            Vector3 pathOrigin,
            Vector3 pathDirection)
        {
            return candidate == null
                ? float.PositiveInfinity
                : Mathf.Max(
                    0f,
                    Vector3.Dot(
                        candidate.bounds.center - pathOrigin,
                        pathDirection));
        }

        private void ProcessMediumSweepCollider(
            Collider candidate,
            Vector3 worldPoint,
            Vector3 contactNormal,
            Vector3 pathDirection,
            float impactSpeed)
        {
            if (candidate == null || !candidate.enabled ||
                candidate.isTrigger || IsMediumSweepSelfCollider(candidate) ||
                BelongsToPlayer(candidate) || BelongsToAlly(candidate))
            {
                return;
            }

            ShowcaseRunnerAgent runner =
                candidate.GetComponentInParent<ShowcaseRunnerAgent>();
            if (runner != null)
            {
                int runnerId = runner.GetInstanceID();
                if (runner.IsDead ||
                    mediumSwingHitRunnerIds.Contains(runnerId) ||
                    mediumSwingPendingHits.ContainsKey(runnerId))
                {
                    return;
                }

                ResolveRunnerPressureController();
                if (runnerPressure == null)
                {
                    return;
                }
                Vector3 resolvedPoint = candidate.ClosestPoint(worldPoint);
                Vector3 velocity = pathDirection * Mathf.Max(
                    validCollisionSpeed,
                    impactSpeed);
                runnerPressure.TryRegisterCargoSweepImpact(
                    this,
                    runner,
                    activeTarget,
                    impactSpeed,
                    velocity,
                    resolvedPoint);
                return;
            }

            if (IsBlockingMediumSweepObstacle(candidate, contactNormal))
            {
                TerminateMediumSweep(
                    ShowcaseCargoSweepTerminationReason.WorldObstacle,
                    candidate);
            }
        }

        private bool IsMediumSweepSelfCollider(Collider candidate)
        {
            if (candidate == null || activeTarget == null)
            {
                return false;
            }
            Transform candidateTransform = candidate.transform;
            Transform sourceTransform = activeTarget.transform;
            return candidateTransform == sourceTransform ||
                   candidateTransform.IsChildOf(sourceTransform) ||
                   sourceTransform.IsChildOf(candidateTransform);
        }

        private bool BelongsToAlly(Collider candidate)
        {
            return candidate != null && allyTransform != null &&
                (candidate.transform == allyTransform ||
                 candidate.transform.IsChildOf(allyTransform));
        }

        private bool IsBlockingMediumSweepObstacle(
            Collider candidate,
            Vector3 contactNormal)
        {
            if (candidate == null || activeCollider == null ||
                IsMediumSweepFloorContact(candidate, contactNormal))
            {
                return false;
            }

            PrototypeInteractionTarget target =
                candidate.GetComponentInParent<PrototypeInteractionTarget>();
            if (target != null)
            {
                if (target == activeTarget)
                {
                    return false;
                }
                if (target.WeightClass == PrototypeWeightClass.Heavy ||
                    target.Role == PrototypeInteractionRole.Drawer)
                {
                    return true;
                }

                Rigidbody targetBody = candidate.attachedRigidbody;
                if (target.WeightClass == PrototypeWeightClass.Medium)
                {
                    return targetBody == null || targetBody.isKinematic ||
                        activeBody == null ||
                        targetBody.mass >= activeBody.mass * 1.1f;
                }
                return false;
            }

            Rigidbody obstacleBody = candidate.attachedRigidbody;
            if (obstacleBody == null)
            {
                return true;
            }
            if (obstacleBody == activeBody)
            {
                return false;
            }
            return obstacleBody.isKinematic || activeBody == null ||
                obstacleBody.mass >= Mathf.Max(12f, activeBody.mass * 1.25f);
        }

        private bool IsMediumSweepFloorContact(
            Collider candidate,
            Vector3 contactNormal)
        {
            Bounds chairBounds = activeCollider.bounds;
            Bounds candidateBounds = candidate.bounds;
            float recordedSupportY = mediumGroundCenterOfMassY -
                Mathf.Max(0.01f, mediumGroundSupportOffsetY);
            float supportBandY = Mathf.Max(
                chairBounds.min.y,
                recordedSupportY) + MediumSweepFloorTolerance;
            bool belowChair = candidateBounds.max.y <= supportBandY;
            bool upwardSurface = contactNormal.sqrMagnitude > 0.001f &&
                Vector3.Dot(contactNormal.normalized, Vector3.up) >= 0.7f;
            bool broadSupportSurface =
                candidateBounds.center.y < chairBounds.center.y &&
                candidateBounds.size.x >= chairBounds.size.x * 1.25f &&
                candidateBounds.size.z >= chairBounds.size.z * 1.25f;

            // A BoxCast that begins flush with the floor can return a lateral
            // or inverted normal even though the collider is only supporting
            // the Chair from below. Bounds own that classification; contact
            // normal is only a secondary allowance for ramps and seams.
            return belowChair || (upwardSurface && broadSupportSurface);
        }

        private void TerminateMediumSweep(
            ShowcaseCargoSweepTerminationReason reason,
            Collider obstacle = null)
        {
            if (swingTerminated ||
                activeAttackType == ShowcaseCargoAttackType.OverheadSlam)
            {
                return;
            }

            swingTerminated = true;
            mediumSweepTerminationReason = reason;
            mediumSweepTerminationObstacle = obstacle;
            if (reason ==
                    ShowcaseCargoSweepTerminationReason.WorldObstacle ||
                reason ==
                    ShowcaseCargoSweepTerminationReason.PrimaryVictimBlocked)
            {
                primarySweptVictim?.CancelCargoSweepCarry(
                    activeMediumSwingSequence);
            }
            if (activeBody != null)
            {
                Vector3 planar = Vector3.ProjectOnPlane(
                    activeBody.linearVelocity,
                    Vector3.up);
                float retention = reason ==
                        ShowcaseCargoSweepTerminationReason.WorldObstacle ||
                    reason ==
                        ShowcaseCargoSweepTerminationReason.PrimaryVictimBlocked
                        ? 0.08f
                        : 0.22f;
                activeBody.linearVelocity = (planar * retention) +
                    (Vector3.up * Mathf.Min(0f, activeBody.linearVelocity.y));
                activeBody.angularVelocity *= 0.2f;
            }
        }

        private void UpdateMediumTargetCorrection(float swing01)
        {
            if (mediumSwingTarget == null ||
                mediumSwingTargetCollider == null || swing01 >= 0.55f)
            {
                return;
            }

            Vector3 currentPoint = ResolvePredictedRunnerTargetPoint(
                mediumSwingTarget,
                mediumSwingTargetCollider.bounds.center,
                activeChargeProfile,
                false,
                1f - swing01);
            float correctionDistance = mediumTargetCorrectionDistance *
                mediumSweepReachScale;
            Vector3 boundedPoint = mediumSwingSnapshotTargetPoint +
                Vector3.ClampMagnitude(
                    currentPoint - mediumSwingSnapshotTargetPoint,
                    correctionDistance);
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
            if (swingTerminated &&
                activeAttackType != ShowcaseCargoAttackType.OverheadSlam &&
                Phase != ShowcaseCargoStrapPhase.MediumRecovering)
            {
                BeginMediumRecovery();
                return;
            }
            if (Phase == ShowcaseCargoStrapPhase.MediumExtending &&
                Time.time - mediumExtensionStartedAt >=
                    mediumExtensionDuration)
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
            TryBeginAttack(aimPoint);
        }

        private void BeginMediumRecovery()
        {
            if (!mediumSwingPlanActive ||
                Phase == ShowcaseCargoStrapPhase.MediumRecovering)
            {
                return;
            }

            if (depletedReleaseQueued)
            {
                BeginRelease(true);
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

            if (depletedReleaseQueued)
            {
                BeginRelease(true);
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
                bool preservePrimaryCarry = primarySweptVictim != null &&
                    primarySweptVictim.IsCargoSweepCarryActive &&
                    mediumSweepTerminationReason !=
                        ShowcaseCargoSweepTerminationReason.WorldObstacle &&
                    mediumSweepTerminationReason !=
                        ShowcaseCargoSweepTerminationReason.
                            PrimaryVictimBlocked;
                ClearMediumSwingPlan(
                    false,
                    preservePendingControlRecovery: true,
                    preservePrimaryCarry: preservePrimaryCarry);
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
            if (!mediumSwingPlanActive)
            {
                return false;
            }

            Vector3 targetPoint = mediumSwingSnapshotTargetPoint;
            if (mediumSwingTarget != null)
            {
                if (mediumSwingTargetCollider == null ||
                    !mediumSwingTarget.gameObject.activeInHierarchy ||
                    !mediumSwingTargetCollider.enabled)
                {
                    return false;
                }
                targetPoint = mediumSwingTargetCollider.bounds.center;
            }

            Vector3 reelToTarget = targetPoint - GetReelPosition();
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

        private void ClearMediumSwingPlan(
            bool dampVelocity,
            bool preservePendingControlRecovery = false,
            bool preservePrimaryCarry = false)
        {
            bool wasActive = mediumSwingPlanActive;
            bool keepControlRecoveryPending =
                preservePendingControlRecovery &&
                attackImpactAt >= 0f &&
                !mediumControlRecovered &&
                controlRecoveryAt >= 0f;
            CancelActiveAttackObservation();
            if (!preservePrimaryCarry)
            {
                primarySweptVictim?.CancelCargoSweepCarry(
                    activeMediumSwingSequence);
            }
            mediumSwingPlanActive = false;
            mediumSwingMotionStarted = false;
            mediumSwingAttackLeft = false;
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
            mediumSwingPendingHits.Clear();
            mediumSweepPreviousCenter = Vector3.zero;
            mediumSweepPreviousRotation = Quaternion.identity;
            mediumSweepPreviousHalfExtents = Vector3.zero;
            mediumSweepPoseInitialized = false;
            primarySweptVictim = null;
            mediumSweepInitialEnergy01 = 0f;
            mediumSweepReachScale = 1f;
            remainingImpactEnergy01 = 0f;
            hitOrdinal = 0;
            mediumSweepMaximumTargets = 0;
            swingTerminated = false;
            mediumSweepTerminationReason =
                ShowcaseCargoSweepTerminationReason.None;
            mediumSweepTerminationObstacle = null;
            mediumSweepLastPathDirection = Vector3.zero;
            activeAttackType = ShowcaseCargoAttackType.None;
            activeChargeProfile = ShowcaseCargoChargeModel.Evaluate(0f);
            slamMotionStarted = false;
            slamDescending = false;
            slamImpactResolved = false;
            slamMotionStartedAt = 0f;
            slamPreviousCenterY = 0f;
            slamDownwardTravel = 0f;
            slamStartWorldPosition = Vector3.zero;
            slamApexWorldPosition = Vector3.zero;
            slamTargetWorldPosition = Vector3.zero;
            slamPreviousColliderCenter = Vector3.zero;
            slamColliderSweepInitialized = false;
            if (!keepControlRecoveryPending)
            {
                mediumControlRecovered = true;
                controlRecoveryAt = -1f;
            }
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
            bool accepted = TryAuthorizeMediumSwingImpact(
                source,
                runner,
                impactSpeed,
                false,
                out ShowcaseCargoSweepHitAuthorization authorization);
            swingSequence = accepted ? authorization.SwingSequence : 0;
            sourceId = accepted ? authorization.SourceId : string.Empty;
            return accepted;
        }

        public bool TryAuthorizeMediumSwingImpact(
            PrototypeInteractionTarget source,
            ShowcaseRunnerAgent runner,
            float impactSpeed,
            bool isCausalEnemyCollision,
            out ShowcaseCargoSweepHitAuthorization authorization)
        {
            authorization = default;
            int runnerId = runner != null ? runner.GetInstanceID() : 0;
            if (!mediumSwingPlanActive || !mediumSwingMotionStarted ||
                swingTerminated ||
                activeAttackType == ShowcaseCargoAttackType.OverheadSlam ||
                source == null || source != activeTarget ||
                source.WeightClass != PrototypeWeightClass.Medium ||
                runner == null || runner.IsDead ||
                impactSpeed < validCollisionSpeed ||
                activeMediumSwingSequence == 0 ||
                mediumSweepMaximumTargets <= 0 ||
                hitOrdinal >= mediumSweepMaximumTargets ||
                hitOrdinal >=
                    ShowcaseCargoSweepEnergyModel.AbsoluteMaximumTargets ||
                remainingImpactEnergy01 <
                    ShowcaseCargoSweepEnergyModel.ContinueThreshold01 ||
                mediumSwingHitRunnerIds.Contains(runnerId) ||
                mediumSwingPendingHits.ContainsKey(runnerId) ||
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

            ShowcaseCargoSweepHitProfile hitProfile =
                ShowcaseCargoSweepEnergyModel.ResolveHitProfile(
                    hitOrdinal + 1);
            authorization = new ShowcaseCargoSweepHitAuthorization(
                activeMediumSwingSequence,
                $"CHR00_STRAP:{source.RuntimeInstanceKey}:SWEEP",
                hitProfile,
                primarySweptVictim == null,
                isCausalEnemyCollision);
            mediumSwingPendingHits.Add(runnerId, authorization);
            return true;
        }

        public void ConfirmMediumSwingImpact(
            ShowcaseRunnerAgent runner,
            uint swingSequence)
        {
            if (runner == null || swingSequence == 0 ||
                swingSequence != activeMediumSwingSequence ||
                !mediumSwingPendingHits.TryGetValue(
                    runner.GetInstanceID(),
                    out ShowcaseCargoSweepHitAuthorization authorization) ||
                authorization.SwingSequence != swingSequence)
            {
                return;
            }

            int runnerId = runner.GetInstanceID();
            mediumSwingPendingHits.Remove(runnerId);
            if (!mediumSwingHitRunnerIds.Add(runnerId))
            {
                return;
            }

            hitOrdinal = authorization.HitProfile.HitOrdinal;
            if (authorization.IsPrimaryVictim && primarySweptVictim == null)
            {
                primarySweptVictim = runner;
            }
            CommitAcceptedCargoStrapUse();
            remainingImpactEnergy01 = Mathf.Max(
                0f,
                remainingImpactEnergy01 -
                    authorization.HitProfile.EnergyCost01);
            MarkAttackImpact();

            if (activeBody != null)
            {
                float energyFraction = mediumSweepInitialEnergy01 > 0.001f
                    ? Mathf.Clamp01(
                        remainingImpactEnergy01 /
                        mediumSweepInitialEnergy01)
                    : 0f;
                float velocityRetention = Mathf.Lerp(
                    0.48f,
                    0.78f,
                    energyFraction);
                Vector3 planar = Vector3.ProjectOnPlane(
                    activeBody.linearVelocity,
                    Vector3.up);
                activeBody.linearVelocity = (planar * velocityRetention) +
                    Vector3.Project(activeBody.linearVelocity, Vector3.up);
                activeBody.angularVelocity *= Mathf.Lerp(
                    0.55f,
                    0.82f,
                    energyFraction);
            }

            if (hitOrdinal >= mediumSweepMaximumTargets ||
                hitOrdinal >=
                    ShowcaseCargoSweepEnergyModel.AbsoluteMaximumTargets)
            {
                TerminateMediumSweep(
                    ShowcaseCargoSweepTerminationReason.MaximumTargetsReached);
            }
            else if (remainingImpactEnergy01 <
                     ShowcaseCargoSweepEnergyModel.ContinueThreshold01)
            {
                TerminateMediumSweep(
                    ShowcaseCargoSweepTerminationReason.EnergyDepleted);
            }
        }

        private bool CommitAcceptedCargoStrapUse()
        {
            if (cargoStrapUseCommitted)
            {
                return true;
            }
            if (activeTarget == null || activeCargoStrapUseSequence == 0)
            {
                RegisterFault("STRAP_USE_SEQUENCE_MISSING");
                return false;
            }

            uint useSequence = activeCargoStrapUseSequence;
            bool committed = activeTarget.
                TryCommitSuccessfulCargoStrapImpact(useSequence);
            if (!committed &&
                activeTarget.LastCommittedCargoStrapSequence != useSequence)
            {
                RegisterFault("STRAP_DURABILITY_COMMIT_FAILED");
                return false;
            }

            cargoStrapUseCommitted = true;
            attackUsePending = false;
            activeTarget.CancelCargoStrapCollisionObservation(useSequence);
            activeCargoStrapUseSequence = 0;
            ValidStrapCollisionCount++;
            if (activeTarget.IsPlayerThrowExhausted)
            {
                DepletionCount++;
                depletedReleaseQueued = true;
            }
            return true;
        }

        public void RejectMediumSwingImpact(
            ShowcaseRunnerAgent runner,
            uint swingSequence)
        {
            if (runner == null || swingSequence == 0 ||
                swingSequence != activeMediumSwingSequence)
            {
                return;
            }
            mediumSwingPendingHits.Remove(runner.GetInstanceID());
        }

        public bool NotifyPrimarySweepVictimBlocked(
            ShowcaseRunnerAgent runner,
            uint swingSequence,
            Collider obstacle)
        {
            if (runner == null || runner != primarySweptVictim ||
                swingSequence == 0 ||
                swingSequence != activeMediumSwingSequence ||
                activeAttackType == ShowcaseCargoAttackType.OverheadSlam ||
                !mediumSwingPlanActive)
            {
                return false;
            }

            TerminateMediumSweep(
                ShowcaseCargoSweepTerminationReason.PrimaryVictimBlocked,
                obstacle);
            return true;
        }

        private bool TryResolveSlamHurtboxContact()
        {
            if (activeCollider == null)
            {
                return false;
            }

            Vector3 currentColliderCenter = activeCollider.bounds.center;
            Vector3 previousColliderCenter = slamColliderSweepInitialized
                ? slamPreviousColliderCenter
                : currentColliderCenter;
            slamPreviousColliderCenter = currentColliderCenter;
            slamColliderSweepInitialized = true;
            if (activeAttackType != ShowcaseCargoAttackType.OverheadSlam ||
                !mediumSwingPlanActive || !mediumSwingMotionStarted ||
                !slamMotionStarted || !slamDescending || slamImpactResolved ||
                mediumSwingTarget == null ||
                activeBody == null || activeMediumSwingSequence == 0 ||
                slamDownwardTravel < slamMinimumDownwardTravel ||
                activeBody.linearVelocity.y > -0.35f ||
                !mediumSwingTarget.TryResolveCargoSlamContact(
                    activeCollider,
                    previousColliderCenter,
                    out Vector3 contactPoint))
            {
                return false;
            }

            CommitSlamImpact(
                contactPoint,
                Mathf.Max(
                    validCollisionSpeed,
                    activeBody.linearVelocity.magnitude),
                mediumSwingTarget);
            return true;
        }

        private void CommitSlamImpact(
            Vector3 worldPoint,
            float impactSpeed,
            ShowcaseRunnerAgent directContactRunner)
        {
            if (slamImpactResolved || activeBody == null || activeTarget == null)
            {
                return;
            }

            slamImpactResolved = true;
            SlamImpactCount++;
            MarkAttackImpact();
            ResolveRunnerPressureController();
            int acceptedCount = runnerPressure?.ResolveCargoSlamImpact(
                this,
                activeTarget,
                worldPoint,
                activeBody.linearVelocity,
                impactSpeed,
                activeMediumSwingSequence,
                activeChargeProfile,
                SlamDirectRadius,
                SlamOuterRadius,
                directContactRunner) ?? 0;
            if (acceptedCount > 0)
            {
                CommitAcceptedCargoStrapUse();
            }
        }

        private void HandleActiveTargetCollision(
            PrototypeInteractionCollisionSnapshot snapshot)
        {
            if (activeAttackType != ShowcaseCargoAttackType.OverheadSlam ||
                !mediumSwingPlanActive || !mediumSwingMotionStarted ||
                !slamMotionStarted || !slamDescending || slamImpactResolved ||
                snapshot.Target == null || snapshot.Target != activeTarget ||
                activeMediumSwingSequence == 0 ||
                slamDownwardTravel < slamMinimumDownwardTravel ||
                snapshot.RelativeSpeed < validCollisionSpeed * 0.75f ||
                activeBody == null || activeBody.linearVelocity.y > -0.35f ||
                snapshot.OtherCollider == null ||
                BelongsToPlayer(snapshot.OtherCollider))
            {
                return;
            }

            ShowcaseRunnerAgent directContactRunner =
                snapshot.OtherCollider.GetComponentInParent<
                    ShowcaseRunnerAgent>();
            CommitSlamImpact(
                snapshot.Point,
                snapshot.RelativeSpeed,
                directContactRunner);
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
            if (!activeTarget.TryCommitSuccessfulCargoStrapImpact(
                    activeCargoStrapUseSequence))
            {
                RegisterFault("STRAP_DURABILITY_COMMIT_FAILED");
                activeCargoStrapUseSequence = 0;
                return;
            }
            cargoStrapUseCommitted = true;
            activeCargoStrapUseSequence = 0;

            ValidStrapCollisionCount++;
            if (activeTarget.IsPlayerThrowExhausted)
            {
                DepletionCount++;
                depletedReleaseQueued = true;
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
                Phase == ShowcaseCargoStrapPhase.Charging ||
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
            ResetChargeRuntime();
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
                : IsCharging
                    ? chargeResolvedTargetPoint
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
            ResetChargeRuntime();
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
            cargoStrapUseCommitted = false;
            depletedReleaseQueued = false;
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
            ChargeCancelCount = 0;
            AttackReleaseCount = 0;
            SlamImpactCount = 0;
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
            lastAttackResolution = default;
            attackReleasedAt = -1f;
            attackAccelerationAt = -1f;
            attackImpactAt = -1f;
            controlRecoveryAt = -1f;
            mediumControlRecovered = true;
            LastReleaseToObjectAccelerationSeconds = -1f;
            LastReleaseToContactSeconds = -1f;
            LastImpactToControlRecoverySeconds = -1f;
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
            activeTarget.CollisionEntered += HandleActiveTargetCollision;
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
                mediumGroundSupportOffsetY = Mathf.Max(
                    0.01f,
                    body.worldCenterOfMass.y - targetCollider.bounds.min.y);
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
            ResetChargeRuntime();
            ResetSmallSpinRuntime();
            ClearMediumSwingPlan(false);
            pullMotionInitialized = false;
            if (activeTarget != null)
            {
                activeTarget.CollisionEntered -= HandleActiveTargetCollision;
                activeTarget.CancelCargoStrapCollisionObservation(
                    activeCargoStrapUseSequence);
            }
            activeCargoStrapUseSequence = 0;
            cargoStrapUseCommitted = false;
            depletedReleaseQueued = false;
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
            mediumGroundSupportOffsetY = 0f;
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
