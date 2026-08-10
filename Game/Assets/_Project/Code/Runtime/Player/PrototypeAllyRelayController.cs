using System;
using System.Collections.Generic;
using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    [DefaultExecutionOrder(-80)]
    [DisallowMultipleComponent]
    public sealed class PrototypeAllyRelayController : MonoBehaviour
    {
        private const int LaneHitCapacity = 32;

        [SerializeField]
        private PrototypeInteractionStateOwner interactionOwner;

        [SerializeField]
        private PrototypePhysicalObjectLifecycle physicalObjectLifecycle;

        [SerializeField]
        private Transform allyTransform;

        [SerializeField]
        private Transform playerTransform;

        [SerializeField]
        private PrototypeInteractionTarget[] supplyTargets =
            Array.Empty<PrototypeInteractionTarget>();

        [SerializeField, Min(0f)]
        private float preReleaseDelay = 0.2f;

        [SerializeField, Min(0.1f)]
        private float binderHorizontalSpeed = 7.5f;

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

        private readonly HashSet<PrototypeInteractionTarget> consumedTargets =
            new HashSet<PrototypeInteractionTarget>();
        private readonly RaycastHit[] laneHits = new RaycastHit[LaneHitCapacity];
        private PrototypeInteractionTarget activeTarget;
        private Rigidbody activeBody;
        private float phaseElapsed;
        private float settledElapsed;
        private bool preReleaseWasKinematic;
        private bool preReleaseUsedGravity;
        private Vector3 preReleaseVelocity;
        private Vector3 preReleaseAngularVelocity;

        public PrototypeAllySupportState AllyState { get; private set; } =
            PrototypeAllySupportState.Ready;

        public PrototypeRelayPhase RelayPhase { get; private set; } =
            PrototypeRelayPhase.None;

        public PrototypeInteractionTarget ActiveTarget => activeTarget;

        public PrototypeAllyRelayRejection LastRequestRejection { get; private set; }

        public int ReleaseCount { get; private set; }

        public int CatchCount { get; private set; }

        public int QueuedThrowCount { get; private set; }

        public int PhysicalResolutionCount { get; private set; }

        public Vector3 LastReleasePosition { get; private set; }

        public Vector3 LastLaunchVelocity { get; private set; }

        public float PreReleaseDelay => preReleaseDelay;

        public float BinderHorizontalSpeed => binderHorizontalSpeed;

        public float ChairFeedSpeed => chairFeedSpeed;

        public float CatchRange => catchRange;

        public float SettleSpeed => settleSpeed;

        public float SettleDuration => settleDuration;

        public float MaximumRelayDuration => maximumRelayDuration;

        public bool HasBaselineState =>
            AllyState == PrototypeAllySupportState.Ready &&
            RelayPhase == PrototypeRelayPhase.None &&
            activeTarget == null &&
            consumedTargets.Count == 0;

        public bool IsBinderIncoming =>
            RelayPhase == PrototypeRelayPhase.Released &&
            activeTarget != null &&
            activeTarget.Role == PrototypeInteractionRole.AllyBinder;

        public bool IsChairFeedActive =>
            RelayPhase == PrototypeRelayPhase.Released &&
            activeTarget != null &&
            activeTarget.Role == PrototypeInteractionRole.AllyChair;

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
            float authoredMaximumRelayDuration = 4f)
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

            if (AllyState != PrototypeAllySupportState.Ready)
            {
                rejection = PrototypeAllyRelayRejection.AllyUnavailable;
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
            preReleaseVelocity = body.linearVelocity;
            preReleaseAngularVelocity = body.angularVelocity;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.useGravity = false;
            body.isKinematic = true;
            AllyState = PrototypeAllySupportState.SupplyCommitted;
            RelayPhase = PrototypeRelayPhase.PreRelease;
            LastRequestRejection = PrototypeAllyRelayRejection.None;
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
            return Vector3.Distance(
                interactionOrigin,
                targetCollider.ClosestPoint(interactionOrigin)) <= catchRange;
        }

        public bool CompleteCatch(PrototypeInteractionTarget target, bool queuedThrow)
        {
            if (target == null || target != activeTarget ||
                RelayPhase != PrototypeRelayPhase.Released)
            {
                return false;
            }

            CatchCount++;
            if (queuedThrow)
            {
                QueuedThrowCount++;
            }

            ClearActiveRelayState();
            return true;
        }

        public void ResolveCatchFailure(PrototypeInteractionTarget target)
        {
            if (target != null && target == activeTarget)
            {
                interactionOwner?.ClearRelayReservation();
                ClearActiveRelayState();
            }
        }

        public void ResetToAuthoredBaseline()
        {
            interactionOwner?.ClearRelayReservation();
            consumedTargets.Clear();
            ClearActiveRelayState();
            LastRequestRejection = PrototypeAllyRelayRejection.None;
            LastReleasePosition = Vector3.zero;
            LastLaunchVelocity = Vector3.zero;
        }

        public bool HasValidPhysicalLane(PrototypeInteractionTarget target)
        {
            if (!TryGetPhysicalTarget(target, out Rigidbody body, out Collider targetCollider) ||
                playerTransform == null)
            {
                return false;
            }

            Vector3 start = targetCollider.bounds.center;
            Vector3 destination = target.Role == PrototypeInteractionRole.AllyBinder
                ? GetBinderDestination()
                : new Vector3(
                    playerTransform.position.x,
                    start.y,
                    playerTransform.position.z);
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

        private void FixedUpdate()
        {
            if (RelayPhase == PrototypeRelayPhase.None || activeTarget == null)
            {
                return;
            }

            if (interactionOwner == null || interactionOwner.BlocksNewInteraction)
            {
                return;
            }

            if (RelayPhase == PrototypeRelayPhase.PreRelease)
            {
                phaseElapsed += Time.fixedDeltaTime;
                if (phaseElapsed >= preReleaseDelay)
                {
                    ReleaseActiveTarget();
                }

                return;
            }

            if (interactionOwner.RelayState != PrototypeRelayReservationState.Active ||
                interactionOwner.RelayReservedTarget != activeTarget)
            {
                ClearActiveRelayState();
                return;
            }

            phaseElapsed += Time.fixedDeltaTime;
            if (activeBody == null)
            {
                ResolvePhysicalPassage();
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
            activeBody.useGravity = true;
            activeBody.linearVelocity = activeTarget.Role == PrototypeInteractionRole.AllyBinder
                ? CalculateBinderLaunchVelocity(activeBody.worldCenterOfMass)
                : CalculateChairFeedVelocity(activeBody.worldCenterOfMass);
            activeBody.angularVelocity = activeTarget.Role == PrototypeInteractionRole.AllyBinder
                ? Vector3.up * 2f
                : Vector3.zero;
            activeBody.WakeUp();

            consumedTargets.Add(activeTarget);
            RelayPhase = PrototypeRelayPhase.Released;
            phaseElapsed = 0f;
            settledElapsed = 0f;
            LastReleasePosition = activeBody.position;
            LastLaunchVelocity = activeBody.linearVelocity;
            ReleaseCount++;
        }

        private Vector3 CalculateBinderLaunchVelocity(Vector3 start)
        {
            Vector3 destination = GetBinderDestination();
            Vector3 displacement = destination - start;
            Vector3 horizontal = Vector3.ProjectOnPlane(displacement, Vector3.up);
            float flightTime = Mathf.Clamp(
                horizontal.magnitude / binderHorizontalSpeed,
                0.35f,
                1.1f);
            Vector3 horizontalVelocity = horizontal / flightTime;
            float verticalVelocity =
                (displacement.y - 0.5f * Physics.gravity.y * flightTime * flightTime) /
                flightTime;
            return horizontalVelocity + Vector3.up * verticalVelocity;
        }

        private Vector3 CalculateChairFeedVelocity(Vector3 start)
        {
            Vector3 destination = playerTransform != null
                ? playerTransform.position
                : start + transform.forward;
            Vector3 direction = Vector3.ProjectOnPlane(destination - start, Vector3.up);
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
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
            activeTarget = null;
            activeBody = null;
            phaseElapsed = 0f;
            settledElapsed = 0f;
            AllyState = PrototypeAllySupportState.Ready;
            RelayPhase = PrototypeRelayPhase.None;
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
                    target.CatchEligible &&
                    !target.StrapEligible,
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
