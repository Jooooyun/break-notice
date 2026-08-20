using System;
using System.Collections.Generic;
using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    [DefaultExecutionOrder(-60)]
    [DisallowMultipleComponent]
    public sealed class PrototypeAllyRelayController : MonoBehaviour
    {
        private const int LaneHitCapacity = 32;
        private const float MovementHeight = 0.75f;
        private const float MovementArrivalTolerance = 0.025f;
        private const float AcquisitionArrivalTolerance = 0.02f;
        private const float FetchAssessmentInterval = 0.25f;

        [SerializeField]
        private PrototypeInteractionStateOwner interactionOwner;

        [SerializeField]
        private PrototypePhysicalObjectLifecycle physicalObjectLifecycle;

        [SerializeField]
        private ShowcaseMobileAllyController mobileAllyController;

        [SerializeField]
        private Transform allyTransform;

        [SerializeField]
        private Transform playerTransform;

        [SerializeField]
        private PrototypeInteractionTarget[] supplyTargets =
            Array.Empty<PrototypeInteractionTarget>();

        [SerializeField, Min(0f)]
        private float preReleaseDelay = 0.2f;

        [SerializeField]
        private bool requiresAnimationRelease;

        [SerializeField, Min(0.1f)]
        private float binderHorizontalSpeed = 7.5f;

        [SerializeField, Range(0.5f, 1f)]
        private float minimumBinderSpeedMultiplier = 0.9f;

        [SerializeField, Range(1f, 1.5f)]
        private float maximumBinderSpeedMultiplier = 1.1f;

        [SerializeField, Min(0.1f)]
        private float chairFeedSpeed = 4f;

        [SerializeField, Min(0.5f)]
        private float catchRange = 2.8f;

        [SerializeField, Min(0.05f)]
        private float settleSpeed = 0.35f;

        [SerializeField, Min(0.05f)]
        private float settleAngularSpeed = 0.75f;

        [SerializeField, Min(0.05f)]
        private float settleDuration = 0.35f;

        [SerializeField, Min(0.25f)]
        private float maximumRelayDuration = 4f;

        [SerializeField, Min(0.1f)]
        private float allyMoveSpeed = 2.5f;

        [SerializeField, Min(1f)]
        private float allyTurnSpeed = 540f;

        [SerializeField, Min(0.1f)]
        private float supplyAcquisitionSpeed = 3.5f;

        [SerializeField, Min(0.25f)]
        private float supplyApproachDistance = 0.85f;

        [SerializeField]
        private bool requiresExplicitFetch;

        [SerializeField]
        private Vector3 binderHoldOffset = new Vector3(0f, 1.05f, 0.6f);

        [SerializeField]
        private ShowcaseBinderGripSocket binderGripSocket;

        [SerializeField]
        private ShowcaseChairGripRig chairGripRig;

        [SerializeField]
        private bool requiresChairGripRig;

        [SerializeField, Min(0.1f)]
        private float chairPlayerClearance = 0.45f;

        [SerializeField, Min(0.5f)]
        private float indicatorHeight = 2.1f;

        private readonly HashSet<PrototypeInteractionTarget> consumedTargets =
            new HashSet<PrototypeInteractionTarget>();
        private readonly RaycastHit[] laneHits = new RaycastHit[LaneHitCapacity];
        private readonly RaycastHit[] movementHits = new RaycastHit[LaneHitCapacity];
        private PrototypeInteractionTarget activeTarget;
        private Rigidbody activeBody;
        private PrototypeInteractionTarget preparedTarget;
        private Rigidbody preparedBody;
        private Collider preparedCollider;
        private bool preparedTargetAcquired;
        private bool preparedLaneOpen;
        private bool preparedBodyStateCaptured;
        private bool preparedWasKinematic;
        private bool preparedUsedGravity;
        private RigidbodyConstraints preparedConstraints;
        private Vector3 preparedVelocity;
        private Vector3 preparedAngularVelocity;
        private float phaseElapsed;
        private float settledElapsed;
        private bool preReleaseWasKinematic;
        private bool preReleaseUsedGravity;
        private RigidbodyConstraints preReleaseConstraints;
        private Vector3 preReleaseVelocity;
        private Vector3 preReleaseAngularVelocity;
        private Vector3 chairFeedDestination;
        private Vector3 chairFeedDirection;
        private bool chairFeedDestinationValid;
        private uint binderSpeedVariationSequence;
        private float preparedGroundHeight;
        private float fetchAssessmentElapsed;
        private PrototypeInteractionTarget suggestedFetchTarget;
        private bool binderWindupReadyRequested;
        private bool binderWindupReadyContactCommitted;
        private float binderWindupReadyElapsed;
        private uint nextBinderWindupReadySequence;
        private uint activeBinderWindupReadySequence;

        public PrototypeAllySupportState AllyState { get; private set; } =
            PrototypeAllySupportState.Ready;

        public PrototypeRelayPhase RelayPhase { get; private set; } =
            PrototypeRelayPhase.None;

        public PrototypeInteractionTarget ActiveTarget => activeTarget;

        public PrototypeInteractionTarget PreparedTarget => preparedTarget;

        public IReadOnlyList<PrototypeInteractionTarget> SupplyTargets => supplyTargets;

        public PrototypeInteractionTarget SuggestedFetchTarget => suggestedFetchTarget;

        public bool RequiresExplicitFetch => requiresExplicitFetch;

        public bool RequiresAnimationRelease => requiresAnimationRelease;

        public bool IsAwaitingFetchTask =>
            requiresExplicitFetch &&
            RelayPhase == PrototypeRelayPhase.None &&
            preparedTarget == null &&
            HasAvailableFetchTarget();

        public bool IsReturningWithSupply =>
            preparedTargetAcquired &&
            mobileAllyController != null &&
            mobileAllyController.IsReturnTask;

        public int ConsumedTargetCount => consumedTargets.Count;

        public bool IsFetchTargetAvailable(PrototypeInteractionTarget target)
        {
            return requiresExplicitFetch &&
                   IsKnownSupplyTarget(target) &&
                   !consumedTargets.Contains(target) &&
                   TryGetPhysicalTarget(target, out _, out _);
        }

        public bool TryEvaluateFetchTarget(
            PrototypeInteractionTarget target,
            out float routeCost)
        {
            routeCost = float.PositiveInfinity;
            return IsFetchTargetAvailable(target) &&
                   mobileAllyController != null &&
                   mobileAllyController.TryEvaluateFetchRoute(
                       target.transform,
                       supplyApproachDistance,
                       out routeCost);
        }

        public PrototypeInteractionTarget ResolveBestFetchTarget()
        {
            if (!requiresExplicitFetch || mobileAllyController == null ||
                interactionOwner == null ||
                interactionOwner.GlobalState != PrototypeGlobalInteractionState.Playable ||
                RelayPhase != PrototypeRelayPhase.None || activeTarget != null ||
                preparedTarget != null || mobileAllyController.HasTaskMovement)
            {
                return null;
            }

            PrototypeInteractionTarget best = null;
            float bestRouteCost = float.PositiveInfinity;
            string bestKey = null;
            for (int index = 0; index < supplyTargets.Length; index++)
            {
                PrototypeInteractionTarget candidate = supplyTargets[index];
                if (!TryEvaluateFetchTarget(candidate, out float routeCost))
                {
                    continue;
                }

                string key = candidate.RuntimeInstanceKey ?? string.Empty;
                bool lowerCost = routeCost < bestRouteCost - 0.0001f;
                bool sameCost = Mathf.Abs(routeCost - bestRouteCost) <= 0.0001f;
                bool keyWins = sameCost &&
                    (best == null || string.CompareOrdinal(key, bestKey) < 0);
                if (!lowerCost && !keyWins)
                {
                    continue;
                }

                best = candidate;
                bestRouteCost = routeCost;
                bestKey = key;
            }

            return best;
        }

        public bool IsReadyForCall =>
            preparedTargetAcquired &&
            preparedTarget != null &&
            preparedLaneOpen &&
            AllyState == PrototypeAllySupportState.Ready &&
            interactionOwner != null &&
            interactionOwner.GlobalState == PrototypeGlobalInteractionState.Playable &&
            interactionOwner.HandState == PrototypeHandOwnershipState.Free &&
            interactionOwner.StrapState == PrototypeStrapOwnershipState.Stored &&
            interactionOwner.RelayState == PrototypeRelayReservationState.None;

        public bool IsSupplyPreparationBlocked =>
            AllyState == PrototypeAllySupportState.Blocked && !preparedTargetAcquired;

        public bool IsSupplyLaneBlocked =>
            AllyState == PrototypeAllySupportState.Blocked && preparedTargetAcquired;

        public Vector3 IndicatorWorldPosition =>
            (allyTransform != null ? allyTransform.position : transform.position) +
            Vector3.up * indicatorHeight;

        public PrototypeAllyRelayRejection LastRequestRejection { get; private set; }

        public int ReleaseCount { get; private set; }

        public bool IsAwaitingAnimationRelease =>
            RelayPhase == PrototypeRelayPhase.PreRelease && activeTarget != null;

        public bool IsAwaitingBinderWindupReadyContact =>
            AllyState == PrototypeAllySupportState.AcquiringSupply &&
            preparedTarget != null &&
            preparedTarget.Role == PrototypeInteractionRole.AllyBinder &&
            binderWindupReadyRequested &&
            !binderWindupReadyContactCommitted &&
            activeBinderWindupReadySequence != 0;

        public bool IsBinderWindupReadyHeld =>
            binderWindupReadyContactCommitted &&
            preparedTargetAcquired &&
            preparedTarget != null &&
            preparedTarget.Role == PrototypeInteractionRole.AllyBinder &&
            (AllyState == PrototypeAllySupportState.Ready ||
             AllyState == PrototypeAllySupportState.Blocked ||
             AllyState == PrototypeAllySupportState.AcquiringSupply);

        public PrototypeInteractionTarget BinderWindupReadyTarget =>
            binderWindupReadyRequested ? preparedTarget : null;

        public uint BinderWindupReadySequence =>
            binderWindupReadyRequested
                ? activeBinderWindupReadySequence
                : 0;

        public bool IsBinderWindupReadyContactCommitted =>
            binderWindupReadyContactCommitted;

        public int BinderWindupReadyContactCount { get; private set; }

        public int CatchCount { get; private set; }

        public event Action<PrototypeInteractionTarget> CatchCompleted;

        public event Action<PrototypeInteractionTarget> SupplyReleased;

        public int QueuedThrowCount { get; private set; }

        public int PhysicalResolutionCount { get; private set; }

        public Vector3 LastReleasePosition { get; private set; }

        public Vector3 LastLaunchVelocity { get; private set; }

        public float LastBinderSpeedMultiplier { get; private set; } = 1f;

        public float LastBinderHorizontalSpeed { get; private set; }

        public uint BinderSpeedVariationSequence => binderSpeedVariationSequence;

        public float PreReleaseDelay => preReleaseDelay;

        public float BinderHorizontalSpeed => binderHorizontalSpeed;

        public float MinimumBinderSpeedMultiplier => minimumBinderSpeedMultiplier;

        public float MaximumBinderSpeedMultiplier => maximumBinderSpeedMultiplier;

        public float ChairFeedSpeed => chairFeedSpeed;

        public float CatchRange => catchRange;

        public float SettleSpeed => settleSpeed;

        public float SettleDuration => settleDuration;

        public float MaximumRelayDuration => maximumRelayDuration;

        public float AllyMoveSpeed => allyMoveSpeed;

        public float SupplyAcquisitionSpeed => supplyAcquisitionSpeed;

        public float PreparationPathLength { get; private set; }

        public float AllyTravelPathLength { get; private set; }

        public bool HasBaselineState =>
            RelayPhase == PrototypeRelayPhase.None &&
            activeTarget == null &&
            preparedTarget == null &&
            consumedTargets.Count == 0 &&
            (mobileAllyController == null || !mobileAllyController.HasTaskMovement) &&
            (interactionOwner == null ||
             interactionOwner.RelayState == PrototypeRelayReservationState.None);

        public bool IsBinderIncoming =>
            RelayPhase == PrototypeRelayPhase.Released &&
            activeTarget != null &&
            activeTarget.Role == PrototypeInteractionRole.AllyBinder;

        public bool IsChairFeedActive =>
            RelayPhase == PrototypeRelayPhase.Released &&
            activeTarget != null &&
            activeTarget.Role == PrototypeInteractionRole.AllyChair;

        public bool IsChairGripActive => chairGripRig != null && chairGripRig.IsActive;

        public void Configure(
            PrototypeInteractionStateOwner owner,
            PrototypePhysicalObjectLifecycle lifecycle,
            Transform authoredAlly,
            Transform player,
            PrototypeInteractionTarget[] authoredSupplyTargets,
            float authoredPreReleaseDelay = 0.2f,
            float authoredBinderHorizontalSpeed = 7.5f,
            float authoredChairFeedSpeed = 4f,
            float authoredCatchRange = 2.8f,
            float authoredSettleSpeed = 0.35f,
            float authoredSettleAngularSpeed = 0.75f,
            float authoredSettleDuration = 0.35f,
            float authoredMaximumRelayDuration = 4f,
            float authoredAllyMoveSpeed = 2.5f,
            float authoredAllyTurnSpeed = 540f,
            float authoredSupplyAcquisitionSpeed = 3.5f,
            float authoredSupplyApproachDistance = 0.85f,
            float authoredChairPlayerClearance = 0.45f,
            float authoredMinimumBinderSpeedMultiplier = 0.9f,
            float authoredMaximumBinderSpeedMultiplier = 1.1f)
        {
            interactionOwner = owner;
            physicalObjectLifecycle = lifecycle;
            allyTransform = authoredAlly;
            playerTransform = player;
            supplyTargets = authoredSupplyTargets ?? Array.Empty<PrototypeInteractionTarget>();
            preReleaseDelay = Mathf.Max(0f, authoredPreReleaseDelay);
            binderHorizontalSpeed = Mathf.Max(0.1f, authoredBinderHorizontalSpeed);
            chairFeedSpeed = Mathf.Max(0.1f, authoredChairFeedSpeed);
            catchRange = Mathf.Max(0.5f, authoredCatchRange);
            settleSpeed = Mathf.Max(0.05f, authoredSettleSpeed);
            settleAngularSpeed = Mathf.Max(0.05f, authoredSettleAngularSpeed);
            settleDuration = Mathf.Max(0.05f, authoredSettleDuration);
            maximumRelayDuration = Mathf.Max(0.25f, authoredMaximumRelayDuration);
            allyMoveSpeed = Mathf.Max(0.1f, authoredAllyMoveSpeed);
            allyTurnSpeed = Mathf.Max(1f, authoredAllyTurnSpeed);
            supplyAcquisitionSpeed = Mathf.Max(0.1f, authoredSupplyAcquisitionSpeed);
            supplyApproachDistance = Mathf.Max(0.25f, authoredSupplyApproachDistance);
            chairPlayerClearance = Mathf.Max(0.1f, authoredChairPlayerClearance);
            minimumBinderSpeedMultiplier = Mathf.Clamp(
                authoredMinimumBinderSpeedMultiplier,
                0.5f,
                1f);
            maximumBinderSpeedMultiplier = Mathf.Clamp(
                authoredMaximumBinderSpeedMultiplier,
                1f,
                1.5f);
        }

        public void ConfigureShowcaseFetch(
            ShowcaseMobileAllyController mobileAlly,
            PrototypeInteractionTarget[] authoredSupplyTargets,
            bool explicitFetchRequired)
        {
            mobileAllyController = mobileAlly;
            supplyTargets = authoredSupplyTargets ?? Array.Empty<PrototypeInteractionTarget>();
            requiresExplicitFetch = explicitFetchRequired;
            suggestedFetchTarget = null;
            fetchAssessmentElapsed = FetchAssessmentInterval;
            ClearBinderWindupReadyTransaction();
            ClearPreparedTarget(true);
            ClearActiveRelayState();
            AllyState = PrototypeAllySupportState.Ready;
        }

        public void ConfigureAnimationSync(bool required)
        {
            requiresAnimationRelease = required;
        }

        public void ConfigureBinderGrip(ShowcaseBinderGripSocket gripSocket)
        {
            binderGripSocket = gripSocket;
        }

        public void ConfigureChairGrip(
            ShowcaseChairGripRig gripRig,
            bool required = true)
        {
            chairGripRig = gripRig;
            requiresChairGripRig = required;
        }

        public bool TryRequestFetch(
            PrototypeInteractionTarget target,
            out PrototypeAllyRelayRejection rejection)
        {
            if (!requiresExplicitFetch || mobileAllyController == null)
            {
                rejection = PrototypeAllyRelayRejection.AllyUnavailable;
                LastRequestRejection = rejection;
                return false;
            }

            if (interactionOwner == null ||
                interactionOwner.GlobalState != PrototypeGlobalInteractionState.Playable)
            {
                rejection = PrototypeAllyRelayRejection.GlobalStateBlocked;
                LastRequestRejection = rejection;
                return false;
            }

            if (RelayPhase != PrototypeRelayPhase.None || activeTarget != null ||
                preparedTarget != null || mobileAllyController.HasTaskMovement)
            {
                rejection = PrototypeAllyRelayRejection.RelayAlreadyActive;
                LastRequestRejection = rejection;
                return false;
            }

            if (!IsKnownSupplyTarget(target))
            {
                rejection = PrototypeAllyRelayRejection.NoValidSupply;
                LastRequestRejection = rejection;
                return false;
            }

            if (consumedTargets.Contains(target))
            {
                rejection = PrototypeAllyRelayRejection.SupplyAlreadyConsumed;
                LastRequestRejection = rejection;
                return false;
            }

            if (!TryGetPhysicalTarget(target, out Rigidbody body, out Collider targetCollider))
            {
                rejection = PrototypeAllyRelayRejection.InvalidPhysicalTarget;
                LastRequestRejection = rejection;
                return false;
            }

            if (!mobileAllyController.TryBeginFetchMovement(
                    target.transform,
                    supplyApproachDistance))
            {
                rejection = PrototypeAllyRelayRejection.AllyUnavailable;
                LastRequestRejection = rejection;
                return false;
            }

            preparedTarget = target;
            preparedBody = body;
            preparedCollider = targetCollider;
            preparedTargetAcquired = false;
            preparedLaneOpen = false;
            preparedBodyStateCaptured = false;
            preparedGroundHeight = body.position.y;
            suggestedFetchTarget = null;
            fetchAssessmentElapsed = 0f;
            AllyState = PrototypeAllySupportState.MovingToSupply;
            LastRequestRejection = PrototypeAllyRelayRejection.None;
            rejection = PrototypeAllyRelayRejection.None;
            return true;
        }

        public bool TryRequestAutomaticFetch(
            out PrototypeInteractionTarget target,
            out PrototypeAllyRelayRejection rejection)
        {
            target = null;
            if (!requiresExplicitFetch || mobileAllyController == null)
            {
                rejection = PrototypeAllyRelayRejection.AllyUnavailable;
                LastRequestRejection = rejection;
                return false;
            }

            if (interactionOwner == null ||
                interactionOwner.GlobalState != PrototypeGlobalInteractionState.Playable)
            {
                rejection = PrototypeAllyRelayRejection.GlobalStateBlocked;
                LastRequestRejection = rejection;
                return false;
            }

            if (RelayPhase != PrototypeRelayPhase.None || activeTarget != null ||
                preparedTarget != null || mobileAllyController.HasTaskMovement)
            {
                target = preparedTarget ?? activeTarget;
                rejection = PrototypeAllyRelayRejection.RelayAlreadyActive;
                LastRequestRejection = rejection;
                return false;
            }

            target = ResolveBestFetchTarget();
            if (target == null)
            {
                rejection = HasAvailableFetchTarget()
                    ? PrototypeAllyRelayRejection.InvalidPhysicalLane
                    : PrototypeAllyRelayRejection.NoValidSupply;
                LastRequestRejection = rejection;
                return false;
            }

            return TryRequestFetch(target, out rejection);
        }

        public bool TryRequestReadySupply(
            out PrototypeInteractionTarget target,
            out PrototypeAllyRelayRejection rejection)
        {
            target = preparedTarget;
            if (target == null)
            {
                rejection = AllyState == PrototypeAllySupportState.Depleted
                    ? PrototypeAllyRelayRejection.NoValidSupply
                    : PrototypeAllyRelayRejection.AllyUnavailable;
                LastRequestRejection = rejection;
                return false;
            }

            if (!preparedTargetAcquired)
            {
                rejection = IsSupplyPreparationBlocked
                    ? PrototypeAllyRelayRejection.InvalidPhysicalLane
                    : PrototypeAllyRelayRejection.AllyUnavailable;
                LastRequestRejection = rejection;
                return false;
            }

            if (AllyState == PrototypeAllySupportState.Blocked || !preparedLaneOpen)
            {
                rejection = PrototypeAllyRelayRejection.InvalidPhysicalLane;
                LastRequestRejection = rejection;
                return false;
            }

            if (AllyState != PrototypeAllySupportState.Ready)
            {
                rejection = PrototypeAllyRelayRejection.AllyUnavailable;
                LastRequestRejection = rejection;
                return false;
            }

            return TryRequestSupply(target, out rejection);
        }

        public bool CanRequestSupply(
            PrototypeInteractionTarget target,
            out PrototypeAllyRelayRejection rejection)
        {
            if (interactionOwner == null ||
                interactionOwner.GlobalState != PrototypeGlobalInteractionState.Playable)
            {
                rejection = PrototypeAllyRelayRejection.GlobalStateBlocked;
                return false;
            }

            if (interactionOwner.HandState != PrototypeHandOwnershipState.Free)
            {
                rejection = PrototypeAllyRelayRejection.HandOwnershipBlocked;
                return false;
            }

            if (interactionOwner.StrapState != PrototypeStrapOwnershipState.Stored)
            {
                rejection = PrototypeAllyRelayRejection.StrapOwnershipBlocked;
                return false;
            }

            if (interactionOwner.RelayState != PrototypeRelayReservationState.None ||
                RelayPhase != PrototypeRelayPhase.None)
            {
                rejection = PrototypeAllyRelayRejection.RelayAlreadyActive;
                return false;
            }

            if (interactionOwner.QueuedThrowArmed)
            {
                rejection = PrototypeAllyRelayRejection.QueueAlreadyArmed;
                return false;
            }

            if (!IsKnownSupplyTarget(target))
            {
                rejection = PrototypeAllyRelayRejection.NoValidSupply;
                return false;
            }

            if (consumedTargets.Contains(target))
            {
                rejection = PrototypeAllyRelayRejection.SupplyAlreadyConsumed;
                return false;
            }

            if (AllyState != PrototypeAllySupportState.Ready)
            {
                rejection = PrototypeAllyRelayRejection.AllyUnavailable;
                return false;
            }

            if (!TryGetPhysicalTarget(target, out _, out _))
            {
                rejection = PrototypeAllyRelayRejection.InvalidPhysicalTarget;
                return false;
            }

            if (!HasValidPhysicalLane(target))
            {
                rejection = PrototypeAllyRelayRejection.InvalidPhysicalLane;
                return false;
            }

            rejection = PrototypeAllyRelayRejection.None;
            return true;
        }

        public bool TryRequestSupply(
            PrototypeInteractionTarget target,
            out PrototypeAllyRelayRejection rejection)
        {
            if (!CanRequestSupply(target, out rejection) ||
                !TryGetPhysicalTarget(target, out Rigidbody body, out _))
            {
                LastRequestRejection = rejection;
                return false;
            }

            if (!interactionOwner.TryReserveRelay(target))
            {
                rejection = PrototypeAllyRelayRejection.RelayAlreadyActive;
                LastRequestRejection = rejection;
                return false;
            }

            activeTarget = target;
            activeBody = body;
            phaseElapsed = 0f;
            settledElapsed = 0f;
            preReleaseWasKinematic = body.isKinematic;
            preReleaseUsedGravity = body.useGravity;
            preReleaseConstraints = body.constraints;
            preReleaseVelocity = body.linearVelocity;
            preReleaseAngularVelocity = body.angularVelocity;
            if (!body.isKinematic)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
            body.useGravity = false;
            body.isKinematic = true;
            AllyState = PrototypeAllySupportState.SupplyCommitted;
            RelayPhase = PrototypeRelayPhase.PreRelease;
            LastRequestRejection = PrototypeAllyRelayRejection.None;
            if (target == preparedTarget)
            {
                ClearPreparedTarget(false);
            }
            return true;
        }

        public bool TryArmQueuedThrow()
        {
            return RelayPhase != PrototypeRelayPhase.None &&
                   activeTarget != null &&
                   activeTarget.CatchEligible &&
                   interactionOwner != null &&
                   interactionOwner.TryArmQueuedThrow();
        }

        public bool CanCatch(PrototypeInteractionTarget target)
        {
            if (target == null ||
                target != activeTarget ||
                RelayPhase != PrototypeRelayPhase.Released ||
                !target.CatchEligible ||
                interactionOwner == null ||
                interactionOwner.RelayState != PrototypeRelayReservationState.Active ||
                interactionOwner.RelayReservedTarget != target ||
                playerTransform == null)
            {
                return false;
            }

            Collider targetCollider = target.GetComponent<Collider>();
            if (targetCollider == null)
            {
                return false;
            }

            Vector3 interactionOrigin = playerTransform.position + Vector3.up;
            Vector3 closestPoint = targetCollider.ClosestPoint(interactionOrigin);
            return Vector3.Distance(interactionOrigin, closestPoint) <= catchRange &&
                   HasClearSegment(
                       interactionOrigin,
                       closestPoint,
                       target,
                       activeBody,
                       targetCollider);
        }

        public bool CompleteCatch(PrototypeInteractionTarget target, bool queuedThrow)
        {
            if (target == null || target != activeTarget ||
                RelayPhase != PrototypeRelayPhase.Released ||
                interactionOwner == null ||
                !interactionOwner.TryCompleteRelayCatchReservation(target))
            {
                return false;
            }

            CatchCount++;
            if (queuedThrow)
            {
                QueuedThrowCount++;
            }

            ClearActiveRelayState();
            CatchCompleted?.Invoke(target);
            return true;
        }

        public void ResolveCatchFailure(PrototypeInteractionTarget target)
        {
            if (target != null && target == activeTarget)
            {
                interactionOwner?.TryCancelRelayReservation(target);
                ClearActiveRelayState();
            }
        }

        public void ResetToAuthoredBaseline()
        {
            interactionOwner?.ClearRelayReservation();
            mobileAllyController?.ClearTaskMovement();
            ClearPreparedTarget(true);
            consumedTargets.Clear();
            ClearActiveRelayState();
            chairGripRig?.ResetGrip();
            LastRequestRejection = PrototypeAllyRelayRejection.None;
            LastReleasePosition = Vector3.zero;
            LastLaunchVelocity = Vector3.zero;
            LastBinderSpeedMultiplier = 1f;
            LastBinderHorizontalSpeed = 0f;
            binderSpeedVariationSequence = 0;
            BinderWindupReadyContactCount = 0;
            PreparationPathLength = 0f;
            AllyTravelPathLength = 0f;
            preparedGroundHeight = 0f;
            suggestedFetchTarget = null;
            fetchAssessmentElapsed = FetchAssessmentInterval;
        }

        public bool HasValidPhysicalLane(PrototypeInteractionTarget target)
        {
            if (!TryGetPhysicalTarget(target, out Rigidbody body, out Collider targetCollider) ||
                playerTransform == null)
            {
                return false;
            }

            Vector3 start = targetCollider.bounds.center;
            Vector3 destination;
            if (target.Role == PrototypeInteractionRole.AllyBinder)
            {
                destination = GetBinderDestination();
            }
            else
            {
                if (!TryGetChairFeedDestination(body.position, targetCollider, out Vector3 bodyDestination))
                {
                    return false;
                }

                destination = bodyDestination + (targetCollider.bounds.center - body.position);
            }
            Vector3 displacement = destination - start;
            float distance = displacement.magnitude;
            if (distance <= 0.05f)
            {
                return true;
            }

            return HasClearSegment(start, destination, target, body, targetCollider);
        }

        private void FixedUpdate()
        {
            if (interactionOwner == null || interactionOwner.BlocksNewInteraction)
            {
                return;
            }

            if (RelayPhase == PrototypeRelayPhase.None || activeTarget == null)
            {
                AdvanceSupplyPreparation(Time.fixedDeltaTime);
                return;
            }

            if (RelayPhase == PrototypeRelayPhase.PreRelease)
            {
                MaintainActiveBinderAtGrip();
                phaseElapsed += Time.fixedDeltaTime;
                bool waitsForAnimation = requiresAnimationRelease &&
                    activeTarget.Role == PrototypeInteractionRole.AllyBinder;
                if (!waitsForAnimation && phaseElapsed >= preReleaseDelay)
                {
                    ReleaseActiveTarget();
                }
                else if (waitsForAnimation &&
                         phaseElapsed >= Mathf.Max(2f, preReleaseDelay + 1f))
                {
                    Debug.LogError(
                        "ALLY SUPPLY ANIMATION RELEASE EVENT TIMEOUT; " +
                        "the reserved object was not launched.",
                        this);
                    AbortPreRelease();
                }

                return;
            }

            PrototypeRelayReservationState reservationState =
                interactionOwner.RelayState;
            bool reservationValid =
                reservationState == PrototypeRelayReservationState.Active ||
                reservationState == PrototypeRelayReservationState.CatchCommitted;
            if (!reservationValid ||
                interactionOwner.RelayReservedTarget != activeTarget)
            {
                ClearActiveRelayState();
                return;
            }

            if (reservationState == PrototypeRelayReservationState.CatchCommitted)
            {
                // The player has accepted this exact airborne object and the
                // animation contact event now owns settlement. Do not let the
                // ordinary relay timeout erase the transaction in between.
                return;
            }

            phaseElapsed += Time.fixedDeltaTime;
            if (activeBody == null)
            {
                ResolvePhysicalPassage();
                return;
            }

            if (activeTarget.Role == PrototypeInteractionRole.AllyChair &&
                AdvanceChairFeed())
            {
                return;
            }

            bool settled = activeBody.IsSleeping() ||
                           (activeBody.linearVelocity.magnitude <= settleSpeed &&
                            activeBody.angularVelocity.magnitude <= settleAngularSpeed);
            settledElapsed = settled ? settledElapsed + Time.fixedDeltaTime : 0f;
            if (settledElapsed >= settleDuration || phaseElapsed >= maximumRelayDuration)
            {
                ResolvePhysicalPassage();
            }
        }

        private void LateUpdate()
        {
            if (RelayPhase == PrototypeRelayPhase.PreRelease)
            {
                MaintainActiveBinderAtGrip();
            }
            else if (preparedTargetAcquired &&
                     preparedTarget != null &&
                     preparedTarget.Role == PrototypeInteractionRole.AllyBinder)
            {
                MaintainPreparedBinder(0f);
            }
        }

        private void ReleaseActiveTarget()
        {
            if (activeTarget == null || activeBody == null ||
                interactionOwner == null ||
                interactionOwner.RelayReservedTarget != activeTarget)
            {
                AbortPreRelease();
                return;
            }

            activeBody.isKinematic = false;
            if (activeTarget.Role == PrototypeInteractionRole.AllyBinder)
            {
                activeBody.constraints = preReleaseConstraints;
                activeBody.useGravity = true;
                LastBinderSpeedMultiplier = GetNextBinderSpeedMultiplier();
                activeBody.linearVelocity = CalculateBinderLaunchVelocity(
                    activeBody.worldCenterOfMass,
                    binderHorizontalSpeed * LastBinderSpeedMultiplier);
                LastBinderHorizontalSpeed = Vector3.ProjectOnPlane(
                    activeBody.linearVelocity,
                    Vector3.up).magnitude;
                activeBody.angularVelocity = Vector3.up * 2f;
                chairFeedDestinationValid = false;
            }
            else
            {
                Collider targetCollider = activeTarget.GetComponent<Collider>();
                if (targetCollider == null ||
                    !TryGetChairFeedDestination(
                        activeBody.position,
                        targetCollider,
                        out chairFeedDestination))
                {
                    AbortPreRelease();
                    return;
                }

                chairFeedDirection = Vector3.ProjectOnPlane(
                    chairFeedDestination - activeBody.position,
                    Vector3.up).normalized;
                chairFeedDestinationValid = chairFeedDirection.sqrMagnitude > 0.0001f;
                if (!chairFeedDestinationValid)
                {
                    AbortPreRelease();
                    return;
                }

                activeBody.constraints = preReleaseConstraints |
                    RigidbodyConstraints.FreezePositionY |
                    RigidbodyConstraints.FreezeRotationX |
                    RigidbodyConstraints.FreezeRotationZ;
                activeBody.useGravity = true;
                activeBody.linearVelocity = chairFeedDirection * chairFeedSpeed;
                activeBody.angularVelocity = Vector3.zero;
            }
            activeBody.WakeUp();
            if (activeTarget.Role == PrototypeInteractionRole.AllyChair)
            {
                chairGripRig?.EndGrip();
            }

            consumedTargets.Add(activeTarget);
            RelayPhase = PrototypeRelayPhase.Released;
            phaseElapsed = 0f;
            settledElapsed = 0f;
            LastReleasePosition = activeBody.position;
            LastLaunchVelocity = activeBody.linearVelocity;
            ReleaseCount++;
            SupplyReleased?.Invoke(activeTarget);
        }

        public bool CommitAnimationSupplyRelease()
        {
            if (!IsAwaitingAnimationRelease || activeTarget == null ||
                activeTarget.Role != PrototypeInteractionRole.AllyBinder)
            {
                return false;
            }

            ReleaseActiveTarget();
            return RelayPhase == PrototypeRelayPhase.Released;
        }

        public bool CommitBinderWindupReadyContact(
            PrototypeInteractionTarget expectedTarget,
            uint expectedSequence)
        {
            if (!IsAwaitingBinderWindupReadyContact ||
                expectedTarget == null ||
                expectedTarget != preparedTarget ||
                expectedSequence == 0 ||
                expectedSequence != activeBinderWindupReadySequence ||
                preparedBody == null ||
                binderGripSocket == null ||
                !binderGripSocket.TryGetWorldPose(
                    out Vector3 position,
                    out Quaternion rotation))
            {
                return false;
            }

            Vector3 before = preparedBody.position;
            preparedBody.position = position;
            preparedBody.rotation = rotation;
            PreparationPathLength += Vector3.Distance(before, position);
            preparedTargetAcquired = true;
            binderWindupReadyContactCommitted = true;
            BinderWindupReadyContactCount++;
            CompletePreparedAcquisition(0f);
            return true;
        }

        private Vector3 CalculateBinderLaunchVelocity(
            Vector3 start,
            float requestedHorizontalSpeed)
        {
            Vector3 destination = GetBinderDestination();
            Vector3 displacement = destination - start;
            Vector3 horizontal = Vector3.ProjectOnPlane(displacement, Vector3.up);
            float flightTime = Mathf.Clamp(
                horizontal.magnitude / Mathf.Max(0.1f, requestedHorizontalSpeed),
                0.35f,
                1.1f);
            Vector3 horizontalVelocity = horizontal / flightTime;
            float verticalVelocity =
                (displacement.y - 0.5f * Physics.gravity.y * flightTime * flightTime) /
                flightTime;
            return horizontalVelocity + Vector3.up * verticalVelocity;
        }

        private float GetNextBinderSpeedMultiplier()
        {
            binderSpeedVariationSequence++;
            // A resettable low-discrepancy sequence varies every authored release
            // without mutating Unity's global random state or making tests flaky.
            float unit = Mathf.Repeat(
                binderSpeedVariationSequence * 0.61803398875f + 0.137f,
                1f);
            return Mathf.Lerp(
                minimumBinderSpeedMultiplier,
                maximumBinderSpeedMultiplier,
                unit);
        }

        private Vector3 CalculateChairFeedVelocity(Vector3 start)
        {
            Vector3 direction = Vector3.ProjectOnPlane(chairFeedDestination - start, Vector3.up);
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = chairFeedDirection;
            }

            return direction.normalized * chairFeedSpeed;
        }

        private Vector3 GetBinderDestination()
        {
            if (playerTransform == null)
            {
                return transform.position + transform.forward * 2f + Vector3.up;
            }

            return playerTransform.position +
                   playerTransform.forward * 0.75f +
                   Vector3.up * 1.05f;
        }

        private void ResolvePhysicalPassage()
        {
            FinalizeChairFeedBody();
            interactionOwner?.ClearRelayReservation();
            PhysicalResolutionCount++;
            ClearActiveRelayState();
        }

        private void AbortPreRelease()
        {
            if (activeBody != null)
            {
                activeBody.isKinematic = preReleaseWasKinematic;
                activeBody.useGravity = preReleaseUsedGravity;
                activeBody.constraints = preReleaseConstraints;
                if (!activeBody.isKinematic)
                {
                    activeBody.linearVelocity = preReleaseVelocity;
                    activeBody.angularVelocity = preReleaseAngularVelocity;
                }
            }

            interactionOwner?.ClearRelayReservation();
            ClearActiveRelayState();
        }

        private void ClearActiveRelayState()
        {
            chairGripRig?.EndGrip();
            activeTarget = null;
            activeBody = null;
            phaseElapsed = 0f;
            settledElapsed = 0f;
            AllyState = PrototypeAllySupportState.Ready;
            RelayPhase = PrototypeRelayPhase.None;
            chairFeedDestinationValid = false;
            chairFeedDirection = Vector3.zero;
            chairFeedDestination = Vector3.zero;
        }

        private void AdvanceSupplyPreparation(float deltaTime)
        {
            if (preparedTarget == null)
            {
                if (requiresExplicitFetch)
                {
                    fetchAssessmentElapsed += deltaTime;
                    if (fetchAssessmentElapsed >= FetchAssessmentInterval)
                    {
                        suggestedFetchTarget = ResolveBestFetchTarget();
                        fetchAssessmentElapsed = 0f;
                    }

                    AllyState = suggestedFetchTarget != null
                        ? PrototypeAllySupportState.Ready
                        : HasAvailableFetchTarget()
                            ? PrototypeAllySupportState.Blocked
                            : PrototypeAllySupportState.Depleted;
                    return;
                }

                preparedTarget = SelectNextSupplyTarget();
                preparedTargetAcquired = false;
                preparedLaneOpen = false;
                preparedBodyStateCaptured = false;
                if (preparedTarget == null)
                {
                    AllyState = PrototypeAllySupportState.Depleted;
                    return;
                }

                if (!TryGetPhysicalTarget(preparedTarget, out preparedBody, out preparedCollider))
                {
                    AllyState = PrototypeAllySupportState.Blocked;
                    return;
                }

                preparedGroundHeight = preparedBody.position.y;
                AllyState = PrototypeAllySupportState.MovingToSupply;
            }

            if (!preparedTargetAcquired)
            {
                if (AllyState == PrototypeAllySupportState.AcquiringSupply)
                {
                    AdvanceSupplyAcquisition(deltaTime);
                }
                else
                {
                    AdvanceAllyToSupply(deltaTime);
                }

                return;
            }

            FacePlayer(deltaTime);
            MaintainPreparedSupply(deltaTime);
            preparedLaneOpen = HasValidPhysicalLane(preparedTarget);
            AllyState = preparedLaneOpen
                ? PrototypeAllySupportState.Ready
                : PrototypeAllySupportState.Blocked;
        }

        private void AdvanceAllyToSupply(float deltaTime)
        {
            if (requiresExplicitFetch && mobileAllyController != null)
            {
                if (!mobileAllyController.IsFetching(preparedTarget?.transform))
                {
                    AllyState = PrototypeAllySupportState.Blocked;
                    return;
                }

                if (mobileAllyController.TaskBlocked)
                {
                    AllyState = PrototypeAllySupportState.Blocked;
                    return;
                }

                if (mobileAllyController.TaskReached)
                {
                    AllyState = PrototypeAllySupportState.AcquiringSupply;
                }
                else
                {
                    AllyState = PrototypeAllySupportState.MovingToSupply;
                }
                return;
            }

            if (allyTransform == null || preparedTarget == null)
            {
                AllyState = PrototypeAllySupportState.Blocked;
                return;
            }

            Vector3 current = allyTransform.position;
            Vector3 toTarget = Vector3.ProjectOnPlane(
                preparedTarget.transform.position - current,
                Vector3.up);
            float distance = toTarget.magnitude;
            float remaining = distance - supplyApproachDistance;
            if (remaining <= MovementArrivalTolerance)
            {
                AllyState = PrototypeAllySupportState.AcquiringSupply;
                return;
            }

            Vector3 direction = toTarget / distance;
            float stepDistance = Mathf.Min(allyMoveSpeed * deltaTime, remaining);
            Vector3 next = current + direction * stepDistance;
            if (!HasClearAllyMovementStep(current, next, preparedTarget))
            {
                AllyState = PrototypeAllySupportState.Blocked;
                return;
            }

            allyTransform.position = next;
            allyTransform.rotation = Quaternion.RotateTowards(
                allyTransform.rotation,
                Quaternion.LookRotation(direction, Vector3.up),
                allyTurnSpeed * deltaTime);
            AllyTravelPathLength += Vector3.Distance(current, next);
            AllyState = PrototypeAllySupportState.MovingToSupply;
        }

        private void AdvanceSupplyAcquisition(float deltaTime)
        {
            if (preparedTarget == null || preparedBody == null || preparedCollider == null)
            {
                AllyState = PrototypeAllySupportState.Blocked;
                return;
            }

            CapturePreparedBodyState();
            if (!preparedBody.isKinematic)
            {
                preparedBody.linearVelocity = Vector3.zero;
                preparedBody.angularVelocity = Vector3.zero;
            }
            preparedBody.useGravity = false;
            preparedBody.isKinematic = true;

            bool chair = preparedTarget.Role == PrototypeInteractionRole.AllyChair;
            if (chair && requiresChairGripRig &&
                (chairGripRig == null || !chairGripRig.IsAvailable ||
                 preparedCollider is not BoxCollider))
            {
                RestorePreparedBodyAfterFailedAcquisition();
                AllyState = PrototypeAllySupportState.Blocked;
                return;
            }
            if (!chair && requiresAnimationRelease)
            {
                if (binderGripSocket == null ||
                    !binderGripSocket.TryGetWorldPose(out _, out _))
                {
                    RestorePreparedBodyAfterFailedAcquisition();
                    AllyState = PrototypeAllySupportState.Blocked;
                    return;
                }

                if (!binderWindupReadyRequested)
                {
                    binderWindupReadyRequested = true;
                    binderWindupReadyContactCommitted = false;
                    binderWindupReadyElapsed = 0f;
                    activeBinderWindupReadySequence =
                        NextBinderWindupReadySequence();
                    return;
                }

                binderWindupReadyElapsed += deltaTime;
                if (!binderWindupReadyContactCommitted &&
                    binderWindupReadyElapsed >= 0.15f)
                {
                    Debug.LogError(
                        "ALLY BINDER WINDUP READY CONTACT TIMEOUT; " +
                        "the exact supply target was not attached.",
                        this);
                    RestorePreparedBodyAfterFailedAcquisition();
                    AllyState = PrototypeAllySupportState.Blocked;
                }
                return;
            }

            Vector3 destination = chair
                ? allyTransform.TransformPoint(new Vector3(0f, 0f, supplyApproachDistance))
                : allyTransform.TransformPoint(binderHoldOffset);
            if (chair)
            {
                destination.y = preparedGroundHeight;
            }
            Quaternion destinationRotation = chair
                ? Quaternion.Euler(0f, allyTransform.eulerAngles.y, 0f)
                : allyTransform.rotation;
            // Reaching the target is the authored acquisition contact. Attach in
            // that same simulation step so the actor never walks away while the
            // supply object visually trails behind it.
            Vector3 before = preparedBody.position;
            preparedBody.position = destination;
            preparedBody.rotation = destinationRotation;
            if (chair && preparedCollider is BoxCollider chairBox &&
                chairGripRig != null && chairGripRig.IsAvailable &&
                !chairGripRig.BeginGrip(chairBox))
            {
                RestorePreparedBodyAfterFailedAcquisition();
                AllyState = PrototypeAllySupportState.Blocked;
                return;
            }
            PreparationPathLength += Vector3.Distance(before, destination);
            preparedTargetAcquired = true;
            CompletePreparedAcquisition(deltaTime);
        }

        private void CompletePreparedAcquisition(float deltaTime)
        {
            mobileAllyController?.ClearTaskMovement();
            FacePlayer(deltaTime);
            MaintainPreparedSupply(deltaTime);
            preparedLaneOpen = HasValidPhysicalLane(preparedTarget);
            AllyState = preparedLaneOpen
                ? PrototypeAllySupportState.Ready
                : PrototypeAllySupportState.Blocked;
        }

        private void MaintainPreparedSupply(float deltaTime)
        {
            MaintainPreparedBinder(deltaTime);
            MaintainPreparedChair(deltaTime);
        }

        private void MaintainPreparedBinder(float deltaTime)
        {
            if (preparedTarget == null ||
                preparedTarget.Role != PrototypeInteractionRole.AllyBinder ||
                preparedBody == null ||
                allyTransform == null)
            {
                return;
            }

            if (requiresAnimationRelease)
            {
                if (binderGripSocket != null &&
                    binderGripSocket.TryGetWorldPose(
                        out Vector3 gripPosition,
                        out Quaternion gripRotation))
                {
                    preparedBody.position = gripPosition;
                    preparedBody.rotation = gripRotation;
                }
                return;
            }

            Vector3 destination = allyTransform.TransformPoint(binderHoldOffset);
            preparedBody.position = destination;
            preparedBody.rotation = allyTransform.rotation;
        }

        private void MaintainActiveBinderAtGrip()
        {
            if (activeTarget == null ||
                activeTarget.Role != PrototypeInteractionRole.AllyBinder ||
                activeBody == null ||
                binderGripSocket == null ||
                !binderGripSocket.TryGetWorldPose(
                    out Vector3 position,
                    out Quaternion rotation))
            {
                return;
            }

            activeBody.position = position;
            activeBody.rotation = rotation;
        }

        private void MaintainPreparedChair(float deltaTime)
        {
            if (preparedTarget == null ||
                preparedTarget.Role != PrototypeInteractionRole.AllyChair ||
                preparedBody == null ||
                allyTransform == null)
            {
                return;
            }

            Vector3 destination = allyTransform.TransformPoint(
                new Vector3(0f, 0f, supplyApproachDistance));
            destination.y = preparedGroundHeight;
            preparedBody.position = destination;
            Quaternion yaw = Quaternion.Euler(0f, allyTransform.eulerAngles.y, 0f);
            preparedBody.rotation = yaw;
            if (chairGripRig != null && chairGripRig.IsAvailable &&
                !chairGripRig.IsActive && preparedCollider is BoxCollider chairBox)
            {
                chairGripRig.BeginGrip(chairBox);
            }
        }

        private void FacePlayer(float deltaTime)
        {
            if (allyTransform == null || playerTransform == null)
            {
                return;
            }

            Vector3 direction = Vector3.ProjectOnPlane(
                playerTransform.position - allyTransform.position,
                Vector3.up);
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            allyTransform.rotation = Quaternion.RotateTowards(
                allyTransform.rotation,
                Quaternion.LookRotation(direction.normalized, Vector3.up),
                allyTurnSpeed * deltaTime);
        }

        private PrototypeInteractionTarget SelectNextSupplyTarget()
        {
            PrototypeInteractionTarget best = null;
            for (int index = 0; index < supplyTargets.Length; index++)
            {
                PrototypeInteractionTarget candidate = supplyTargets[index];
                if (!IsKnownSupplyTarget(candidate) ||
                    consumedTargets.Contains(candidate) ||
                    !TryGetPhysicalTarget(candidate, out _, out _))
                {
                    continue;
                }

                if (best == null || string.CompareOrdinal(
                        candidate.RuntimeInstanceKey,
                        best.RuntimeInstanceKey) < 0)
                {
                    best = candidate;
                }
            }

            return best;
        }

        private bool HasAvailableFetchTarget()
        {
            for (int index = 0; index < supplyTargets.Length; index++)
            {
                if (IsFetchTargetAvailable(supplyTargets[index]))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasClearAllyMovementStep(
            Vector3 current,
            Vector3 next,
            PrototypeInteractionTarget destinationTarget)
        {
            Vector3 start = current + Vector3.up * MovementHeight;
            Vector3 end = next + Vector3.up * MovementHeight;
            Vector3 displacement = end - start;
            float distance = displacement.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                return true;
            }

            int hitCount = Physics.RaycastNonAlloc(
                start,
                displacement / distance,
                movementHits,
                distance + 0.05f,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore);
            if (hitCount >= LaneHitCapacity)
            {
                return false;
            }

            for (int index = 0; index < hitCount; index++)
            {
                Collider hit = movementHits[index].collider;
                if (hit == null ||
                    BelongsTo(hit.transform, allyTransform) ||
                    (destinationTarget != null &&
                     (hit.transform == destinationTarget.transform ||
                      hit.transform.IsChildOf(destinationTarget.transform))))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private bool HasClearSegment(
            Vector3 start,
            Vector3 destination,
            PrototypeInteractionTarget target,
            Rigidbody body,
            Collider targetCollider)
        {
            Vector3 displacement = destination - start;
            float distance = displacement.magnitude;
            if (distance <= 0.05f)
            {
                return true;
            }

            int hitCount = Physics.RaycastNonAlloc(
                start,
                displacement / distance,
                laneHits,
                distance,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore);
            if (hitCount >= LaneHitCapacity)
            {
                return false;
            }

            for (int index = 0; index < hitCount; index++)
            {
                Collider hitCollider = laneHits[index].collider;
                if (hitCollider == null ||
                    hitCollider == targetCollider ||
                    hitCollider.attachedRigidbody == body ||
                    BelongsTo(hitCollider.transform, playerTransform) ||
                    BelongsTo(hitCollider.transform, allyTransform))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private bool TryGetChairFeedDestination(
            Vector3 bodyPosition,
            Collider chairCollider,
            out Vector3 destination)
        {
            destination = bodyPosition;
            if (playerTransform == null ||
                chairCollider is not BoxCollider chairBox ||
                !ShowcaseOrientedBoxGeometry.TryCreate(
                    chairBox,
                    out ShowcaseOrientedBoxGeometry chairGeometry))
            {
                return false;
            }

            Collider playerCollider = playerTransform.GetComponent<Collider>();
            Vector3 playerCenter = playerCollider != null
                ? playerCollider.bounds.center
                : playerTransform.position;
            Vector3 horizontal = Vector3.ProjectOnPlane(
                playerCenter - chairGeometry.Center,
                Vector3.up);
            float distance = horizontal.magnitude;
            if (distance <= 0.0001f)
            {
                return false;
            }

            Vector3 direction = horizontal / distance;
            float chairExtent = chairGeometry.GetPlanarSupportRadius(direction);
            float playerExtent = 0.5f;
            CharacterController playerController =
                playerTransform.GetComponent<CharacterController>();
            if (playerController != null)
            {
                Vector3 playerScale = playerController.transform.lossyScale;
                playerExtent = playerController.radius * Mathf.Max(
                    Mathf.Abs(playerScale.x),
                    Mathf.Abs(playerScale.z));
            }
            else if (playerCollider != null)
            {
                Bounds playerBounds = playerCollider.bounds;
                playerExtent =
                    Mathf.Abs(direction.x) * playerBounds.extents.x +
                    Mathf.Abs(direction.z) * playerBounds.extents.z;
            }

            float safeDistance = chairExtent + playerExtent + chairPlayerClearance;
            float travelDistance = distance - safeDistance;
            if (travelDistance <= 0.1f)
            {
                return false;
            }

            Vector3 destinationCenter =
                chairGeometry.Center + direction * travelDistance;
            destination = bodyPosition +
                (destinationCenter - chairGeometry.Center);
            destination.y = bodyPosition.y;
            return true;
        }

        private bool AdvanceChairFeed()
        {
            if (!chairFeedDestinationValid || activeBody == null)
            {
                return false;
            }

            Vector3 remaining = Vector3.ProjectOnPlane(
                chairFeedDestination - activeBody.position,
                Vector3.up);
            float forwardRemaining = Vector3.Dot(remaining, chairFeedDirection);
            float stopDistance = Mathf.Max(
                0.04f,
                chairFeedSpeed * Time.fixedDeltaTime * 1.5f);
            if (forwardRemaining > stopDistance)
            {
                return false;
            }

            activeBody.linearVelocity = Vector3.zero;
            activeBody.angularVelocity = Vector3.zero;
            ResolvePhysicalPassage();
            return true;
        }

        private void FinalizeChairFeedBody()
        {
            if (activeTarget == null ||
                activeTarget.Role != PrototypeInteractionRole.AllyChair ||
                activeBody == null)
            {
                return;
            }

            activeBody.linearVelocity = Vector3.zero;
            activeBody.angularVelocity = Vector3.zero;
            activeBody.constraints = preReleaseConstraints |
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationZ;
            activeBody.useGravity = true;
            activeBody.WakeUp();
        }

        private void CapturePreparedBodyState()
        {
            if (preparedBodyStateCaptured || preparedBody == null)
            {
                return;
            }

            preparedWasKinematic = preparedBody.isKinematic;
            preparedUsedGravity = preparedBody.useGravity;
            preparedConstraints = preparedBody.constraints;
            preparedVelocity = preparedBody.linearVelocity;
            preparedAngularVelocity = preparedBody.angularVelocity;
            preparedBodyStateCaptured = true;
        }

        private void RestorePreparedBodyAfterFailedAcquisition()
        {
            chairGripRig?.EndGrip();
            if (preparedBodyStateCaptured && preparedBody != null)
            {
                preparedBody.isKinematic = preparedWasKinematic;
                preparedBody.useGravity = preparedUsedGravity;
                preparedBody.constraints = preparedConstraints;
                if (!preparedBody.isKinematic)
                {
                    preparedBody.linearVelocity = preparedVelocity;
                    preparedBody.angularVelocity = preparedAngularVelocity;
                }
            }
            ClearBinderWindupReadyTransaction();
        }

        private void ClearBinderWindupReadyTransaction()
        {
            binderWindupReadyRequested = false;
            binderWindupReadyContactCommitted = false;
            binderWindupReadyElapsed = 0f;
            activeBinderWindupReadySequence = 0;
        }

        private uint NextBinderWindupReadySequence()
        {
            nextBinderWindupReadySequence++;
            if (nextBinderWindupReadySequence == 0)
            {
                nextBinderWindupReadySequence++;
            }
            return nextBinderWindupReadySequence;
        }

        private void ClearPreparedTarget(bool restoreBody)
        {
            mobileAllyController?.ClearTaskMovement();
            if (restoreBody && preparedBodyStateCaptured && preparedBody != null)
            {
                preparedBody.isKinematic = preparedWasKinematic;
                preparedBody.useGravity = preparedUsedGravity;
                preparedBody.constraints = preparedConstraints;
                if (!preparedBody.isKinematic)
                {
                    preparedBody.linearVelocity = preparedVelocity;
                    preparedBody.angularVelocity = preparedAngularVelocity;
                }
            }
            if (restoreBody || activeTarget == null ||
                activeTarget.Role != PrototypeInteractionRole.AllyChair)
            {
                chairGripRig?.EndGrip();
            }

            preparedTarget = null;
            preparedBody = null;
            preparedCollider = null;
            preparedTargetAcquired = false;
            preparedLaneOpen = false;
            preparedBodyStateCaptured = false;
            preparedGroundHeight = 0f;
            ClearBinderWindupReadyTransaction();
        }

        private bool IsKnownSupplyTarget(PrototypeInteractionTarget target)
        {
            if (target == null)
            {
                return false;
            }

            bool exactAuthoredTarget = false;
            for (int index = 0; index < supplyTargets.Length; index++)
            {
                if (supplyTargets[index] == target)
                {
                    exactAuthoredTarget = true;
                    break;
                }
            }

            if (!exactAuthoredTarget)
            {
                return false;
            }

            return target.Role switch
            {
                PrototypeInteractionRole.AllyBinder =>
                    target.WeightClass == PrototypeWeightClass.Light &&
                    target.CatchEligible,
                PrototypeInteractionRole.AllyChair =>
                    target.WeightClass == PrototypeWeightClass.Medium &&
                    !target.CatchEligible &&
                    target.StrapEligible,
                _ => false
            };
        }

        private static bool TryGetPhysicalTarget(
            PrototypeInteractionTarget target,
            out Rigidbody body,
            out Collider targetCollider)
        {
            body = target != null ? target.GetComponent<Rigidbody>() : null;
            targetCollider = target != null ? target.GetComponent<Collider>() : null;
            return body != null &&
                   targetCollider is BoxCollider &&
                   targetCollider.transform == body.transform &&
                   target.gameObject.activeInHierarchy;
        }

        private static bool BelongsTo(Transform candidate, Transform root)
        {
            return candidate != null && root != null &&
                   (candidate == root || candidate.IsChildOf(root));
        }
    }
}
