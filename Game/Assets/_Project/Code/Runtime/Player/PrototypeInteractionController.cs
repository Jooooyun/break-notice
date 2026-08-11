using System;
using BreakNotice.Gameplay.Input;
using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class PrototypeInteractionController : MonoBehaviour
    {
        private enum ThrowChargeMode
        {
            None,
            DirectCarry,
            QueuedRelay
        }

        private const int VisibilityHitCapacity = 32;
        private const float ReticleRayMaximumDistance = 100f;
        private const float CatchAssistMinimumForwardDot = 0.65f;

        [SerializeField]
        private InputSystemLogicalInputAdapter logicalInput;

        [SerializeField]
        private PrototypeInteractionStateOwner interactionOwner;

        [SerializeField]
        private Transform viewTransform;

        [SerializeField]
        private PrototypeInteractionTarget[] targets = Array.Empty<PrototypeInteractionTarget>();

        [SerializeField, Min(0.5f)]
        private float interactionRange = 2.8f;

        [SerializeField, Range(1f, 20f)]
        private float pickupAssistHorizontalAngle = 8f;

        [SerializeField, Range(0f, 30f)]
        private float pickupAssistUpwardAngle = 8f;

        [SerializeField, Range(0f, 80f)]
        private float pickupAssistDownwardAngle = 65f;

        [SerializeField]
        private bool strapContextEnabled;

        [SerializeField]
        private PrototypePhysicalObjectLifecycle physicalObjectLifecycle;

        [SerializeField]
        private PrototypeAllyRelayController allyRelayController;

        [SerializeField]
        private ShowcaseMobileAllyController showcaseMobileAllyController;

        [SerializeField, Min(0.1f)]
        private float fullThrowChargeDuration = 1.2f;

        private readonly PrototypeInteractionArbitrator arbitrator =
            new PrototypeInteractionArbitrator();
        private readonly RaycastHit[] visibilityHits = new RaycastHit[VisibilityHitCapacity];
        private ThrowChargeMode throwChargeMode;
        private bool throwChargeHeld;
        private float throwChargeStartedAt;
        private float frozenThrowCharge01;
#if UNITY_WEBGL && !UNITY_EDITOR
        private bool pointerWasLocked;
#endif

        public PrototypeInteractionStateOwner InteractionOwner => interactionOwner;

        public PrototypeInteractionTarget CurrentTarget { get; private set; }

        public PrototypeInteractionDecision LastDecision { get; private set; }

        public float InteractionRange => interactionRange;

        public float CurrentTargetDistance { get; private set; } = float.PositiveInfinity;

        public bool CurrentTargetUsesPickupAssist { get; private set; }

        public float PickupAssistHorizontalAngle => pickupAssistHorizontalAngle;

        public float PickupAssistUpwardAngle => pickupAssistUpwardAngle;

        public float PickupAssistDownwardAngle => pickupAssistDownwardAngle;

        public uint InteractionAttemptSequence { get; private set; }

        public uint AllyCallAttemptSequence { get; private set; }

        public bool LastAllyCallAccepted { get; private set; }

        public PrototypeAllyRelayRejection LastAllyCallRejection { get; private set; }

        public PrototypeInteractionTarget LastAllyCallTarget { get; private set; }

        public PrototypeAllyRelayController AllyRelayController => allyRelayController;

        public ShowcaseMobileAllyController ShowcaseMobileAllyController =>
            showcaseMobileAllyController;

        public Transform ViewTransform => viewTransform;

        public bool HasThrowCharge => throwChargeMode != ThrowChargeMode.None;

        public bool IsThrowCharging => HasThrowCharge && throwChargeHeld;

        public bool IsQueuedThrowCharge => throwChargeMode == ThrowChargeMode.QueuedRelay;

        public float FullThrowChargeDuration => fullThrowChargeDuration;

        public float ThrowCharge01 => !HasThrowCharge
            ? 0f
            : throwChargeHeld
                ? CalculateThrowCharge01(Time.unscaledTime - throwChargeStartedAt)
                : frozenThrowCharge01;

        public Camera ViewCamera => viewTransform != null
            ? viewTransform.GetComponent<Camera>()
            : null;

        private void Awake()
        {
            if (logicalInput == null)
            {
                logicalInput = GetComponent<InputSystemLogicalInputAdapter>();
            }

            if (interactionOwner == null)
            {
                interactionOwner = GetComponent<PrototypeInteractionStateOwner>();
            }
        }

        private void LateUpdate()
        {
            if (interactionOwner == null)
            {
                return;
            }

            ValidateThrowChargeOwnership();
            if (logicalInput == null)
            {
                return;
            }

            bool isAiming = logicalInput.Snapshot.Aim;
            RefreshTarget(isAiming);
            if (logicalInput.Snapshot.ResetEncounter)
            {
                // Reset owns this entire logical frame even though the authored
                // restore barrier completes earlier in LateUpdate.
                ClearThrowCharge();
                return;
            }

            bool pointerActionAvailable = true;
#if UNITY_WEBGL && !UNITY_EDITOR
            // The first trusted click after an Escape/focus loss is reserved for
            // browser capture. It must not also throw a carried object or commit
            // another gameplay interaction in the same frame.
            bool pointerCaptured = logicalInput.IsWebPointerGameplayActive;
            bool pointerJustCaptured = pointerCaptured && !pointerWasLocked;
            pointerWasLocked = pointerCaptured;
            pointerActionAvailable = pointerCaptured && !pointerJustCaptured;
#endif

            if (!pointerActionAvailable && HasThrowCharge)
            {
                SuspendOrCancelThrowCharge();
            }

            if (logicalInput.Snapshot.Throw && pointerActionAvailable)
            {
                BeginThrowCharge();
            }

            if (logicalInput.Snapshot.CallAlly)
            {
                if (showcaseMobileAllyController != null &&
                    showcaseMobileAllyController.OwnsRegroupFallback)
                {
                    showcaseMobileAllyController.TryRequestRegroup();
                }
                else
                {
                    ProcessCallAlly();
                }
            }

            if (logicalInput.Snapshot.InteractPrimary)
            {
                ProcessInteractPrimary(isAiming && pointerActionAvailable);
            }

            if (logicalInput.Snapshot.ThrowReleased && pointerActionAvailable)
            {
                ReleaseThrowCharge();
            }
            else if (throwChargeHeld && !logicalInput.Snapshot.ThrowHeld)
            {
                SuspendOrCancelThrowCharge();
            }
        }

        public void Configure(
            InputSystemLogicalInputAdapter input,
            PrototypeInteractionStateOwner owner,
            Transform view,
            PrototypeInteractionTarget[] authoredTargets,
            float range,
            bool enableStrapContext,
            PrototypePhysicalObjectLifecycle physicalLifecycle = null,
            PrototypeAllyRelayController relayController = null,
            float authoredPickupAssistHorizontalAngle = 8f,
            float authoredPickupAssistUpwardAngle = 8f,
            float authoredPickupAssistDownwardAngle = 65f,
            float authoredFullThrowChargeDuration = 1.2f)
        {
            logicalInput = input;
            interactionOwner = owner;
            viewTransform = view;
            targets = authoredTargets ?? Array.Empty<PrototypeInteractionTarget>();
            interactionRange = Mathf.Max(0.5f, range);
            strapContextEnabled = enableStrapContext;
            physicalObjectLifecycle = physicalLifecycle;
            allyRelayController = relayController;
            pickupAssistHorizontalAngle = Mathf.Clamp(
                authoredPickupAssistHorizontalAngle,
                1f,
                20f);
            pickupAssistUpwardAngle = Mathf.Clamp(
                authoredPickupAssistUpwardAngle,
                0f,
                30f);
            pickupAssistDownwardAngle = Mathf.Clamp(
                authoredPickupAssistDownwardAngle,
                0f,
                80f);
            fullThrowChargeDuration = Mathf.Max(0.1f, authoredFullThrowChargeDuration);
        }

        public void ConfigureShowcaseMobileAlly(ShowcaseMobileAllyController controller)
        {
            showcaseMobileAllyController = controller;
        }

        public PrototypeInteractionTarget RefreshTarget(bool isAiming)
        {
            // The centre reticle ray remains the first-priority selector. When it
            // misses, a narrow horizontal strip can assist only eligible floor
            // pickups below the reticle so the bounded camera pitch does not make
            // a nearby authored object unreachable.
            _ = isAiming;
            Vector3 interactionOrigin = transform.position + Vector3.up;
            if (!TryResolveReticleHit(
                    interactionOrigin,
                    out PrototypeInteractionTarget reticleTarget,
                    out float interactionDistance))
            {
                ClearCurrentTarget();
                TryApplyIncomingCatchAssist(interactionOrigin);
                return CurrentTarget;
            }

            CurrentTarget = reticleTarget != null && interactionDistance <= interactionRange
                ? reticleTarget
                : null;
            CurrentTargetDistance = CurrentTarget != null
                ? interactionDistance
                : float.PositiveInfinity;
            CurrentTargetUsesPickupAssist = false;
            if (CurrentTarget == null)
            {
                TryResolveManualPickupAssist(interactionOrigin);
            }

            TryApplyIncomingCatchAssist(interactionOrigin);
            return CurrentTarget;
        }

        public PrototypeInteractionDecision ProcessInteractPrimary(bool isAiming)
        {
            if (interactionOwner == null)
            {
                return RecordDecision(PrototypeInteractionDecision.Reject(
                    PrototypeInteractionRejection.OwnershipConflict,
                    CurrentTarget));
            }

            RefreshTarget(isAiming);
            bool explicitStrapContext =
                strapContextEnabled &&
                isAiming &&
                CurrentTarget != null &&
                CurrentTarget.HasLoadBearingMetalAnchor;

            if (interactionOwner.RelayState == PrototypeRelayReservationState.Active &&
                (allyRelayController == null || !allyRelayController.CanCatch(CurrentTarget)))
            {
                return RecordDecision(PrototypeInteractionDecision.Reject(
                    PrototypeInteractionRejection.InvalidRelayContext,
                    CurrentTarget));
            }

            PrototypeInteractionContext context = interactionOwner.CreateContext(
                explicitStrapContext,
                CurrentTarget);
            PrototypeInteractionDecision decision = arbitrator.Decide(context);
            bool queuedCatch = decision.Outcome == PrototypeInteractionOutcome.Catch &&
                               interactionOwner.QueuedThrowArmed;

            if (decision.IsAccepted && !interactionOwner.TryCommit(decision))
            {
                decision = PrototypeInteractionDecision.Reject(
                    PrototypeInteractionRejection.OwnershipConflict,
                    CurrentTarget);
            }

            else if (decision.IsAccepted &&
                     decision.Outcome == PrototypeInteractionOutcome.Catch &&
                     queuedCatch)
            {
                Vector3 direction = viewTransform != null
                    ? viewTransform.forward
                    : transform.forward;
                bool physicalThrowSucceeded = physicalObjectLifecycle != null &&
                    physicalObjectLifecycle.TryThrowCommittedRelayTarget(
                        decision.Target,
                        direction,
                        IsQueuedThrowCharge ? ThrowCharge01 : 0f);
                if (!physicalThrowSucceeded)
                {
                    interactionOwner.CompleteThrow(decision.Target);
                    allyRelayController?.ResolveCatchFailure(decision.Target);
                    decision = PrototypeInteractionDecision.Reject(
                        PrototypeInteractionRejection.OwnershipConflict,
                        CurrentTarget);
                }
                else
                {
                    allyRelayController?.CompleteCatch(decision.Target, true);
                }

                ClearThrowCharge();
            }

            else if (decision.IsAccepted &&
                     decision.Outcome == PrototypeInteractionOutcome.Catch &&
                     physicalObjectLifecycle == null)
            {
                interactionOwner.CancelCarry(decision.Target);
                allyRelayController?.ResolveCatchFailure(decision.Target);
                decision = PrototypeInteractionDecision.Reject(
                    PrototypeInteractionRejection.OwnershipConflict,
                    CurrentTarget);
            }

            else if (decision.IsAccepted &&
                     (decision.Outcome == PrototypeInteractionOutcome.Scoop ||
                      decision.Outcome == PrototypeInteractionOutcome.Catch) &&
                     physicalObjectLifecycle != null)
            {
                if (!physicalObjectLifecycle.BeginCarry(decision.Target))
                {
                    interactionOwner.CancelCarry(decision.Target);
                    if (decision.Outcome == PrototypeInteractionOutcome.Catch)
                    {
                        allyRelayController?.ResolveCatchFailure(decision.Target);
                    }

                    decision = PrototypeInteractionDecision.Reject(
                        PrototypeInteractionRejection.OwnershipConflict,
                        CurrentTarget);
                }
                else if (decision.Outcome == PrototypeInteractionOutcome.Catch)
                {
                    allyRelayController?.CompleteCatch(decision.Target, false);
                }
            }

            return RecordDecision(decision);
        }

        public bool ProcessThrow()
        {
            return ProcessThrow(0f);
        }

        public bool ProcessThrow(float charge01)
        {
            if (interactionOwner == null ||
                interactionOwner.BlocksNewInteraction)
            {
                return false;
            }

            if (interactionOwner.HandState == PrototypeHandOwnershipState.Free &&
                interactionOwner.RelayState == PrototypeRelayReservationState.Active)
            {
                return allyRelayController != null &&
                       allyRelayController.TryArmQueuedThrow();
            }

            if (physicalObjectLifecycle == null)
            {
                return false;
            }

            Vector3 direction = viewTransform != null
                ? viewTransform.forward
                : transform.forward;
            bool released = physicalObjectLifecycle.TryThrow(direction, charge01);
            if (released)
            {
                ClearThrowCharge();
            }

            return released;
        }

        public bool BeginThrowCharge()
        {
            ValidateThrowChargeOwnership();
            if (interactionOwner == null ||
                interactionOwner.BlocksNewInteraction ||
                HasThrowCharge)
            {
                return false;
            }

            ThrowChargeMode mode;
            if (interactionOwner.HandState == PrototypeHandOwnershipState.Carrying &&
                interactionOwner.CarriedTarget != null &&
                physicalObjectLifecycle != null &&
                physicalObjectLifecycle.CarriedTarget == interactionOwner.CarriedTarget)
            {
                mode = ThrowChargeMode.DirectCarry;
            }
            else if (interactionOwner.HandState == PrototypeHandOwnershipState.Free &&
                     interactionOwner.RelayState == PrototypeRelayReservationState.Active &&
                     allyRelayController != null &&
                     (interactionOwner.QueuedThrowArmed || allyRelayController.TryArmQueuedThrow()))
            {
                mode = ThrowChargeMode.QueuedRelay;
            }
            else
            {
                return false;
            }

            throwChargeMode = mode;
            throwChargeHeld = true;
            throwChargeStartedAt = Time.unscaledTime;
            frozenThrowCharge01 = 0f;
            return true;
        }

        public bool ReleaseThrowCharge()
        {
            ValidateThrowChargeOwnership();
            if (!HasThrowCharge)
            {
                return false;
            }

            float charge01 = ThrowCharge01;
            if (throwChargeMode == ThrowChargeMode.QueuedRelay)
            {
                frozenThrowCharge01 = charge01;
                throwChargeHeld = false;
                return interactionOwner != null && interactionOwner.QueuedThrowArmed;
            }

            ClearThrowCharge();
            return ProcessThrow(charge01);
        }

        public float CalculateThrowCharge01(float heldDuration)
        {
            return Mathf.Clamp01(Mathf.Max(0f, heldDuration) / fullThrowChargeDuration);
        }

        public bool ProcessCallAlly()
        {
            PrototypeInteractionTarget candidate = allyRelayController?.PreparedTarget;
            PrototypeAllyRelayRejection rejection =
                PrototypeAllyRelayRejection.AllyUnavailable;
            bool accepted = allyRelayController != null &&
                allyRelayController.TryRequestReadySupply(
                    out candidate,
                    out rejection);

            AllyCallAttemptSequence++;
            LastAllyCallAccepted = accepted;
            LastAllyCallRejection = accepted
                ? PrototypeAllyRelayRejection.None
                : rejection;
            LastAllyCallTarget = candidate;
            return accepted;
        }

        private PrototypeInteractionDecision RecordDecision(
            PrototypeInteractionDecision decision)
        {
            LastDecision = decision;
            InteractionAttemptSequence++;
            return LastDecision;
        }

        private void ValidateThrowChargeOwnership()
        {
            if (!HasThrowCharge || interactionOwner == null)
            {
                return;
            }

            bool valid = !interactionOwner.BlocksNewInteraction &&
                (throwChargeMode switch
                {
                    ThrowChargeMode.DirectCarry =>
                        interactionOwner.HandState == PrototypeHandOwnershipState.Carrying &&
                        interactionOwner.CarriedTarget != null &&
                        physicalObjectLifecycle != null &&
                        physicalObjectLifecycle.CarriedTarget == interactionOwner.CarriedTarget,
                    ThrowChargeMode.QueuedRelay =>
                        interactionOwner.HandState == PrototypeHandOwnershipState.Free &&
                        interactionOwner.RelayState == PrototypeRelayReservationState.Active &&
                        interactionOwner.QueuedThrowArmed &&
                        allyRelayController != null &&
                        allyRelayController.ActiveTarget == interactionOwner.RelayReservedTarget,
                    _ => false
                });
            if (!valid)
            {
                ClearThrowCharge();
            }
        }

        private void SuspendOrCancelThrowCharge()
        {
            if (!HasThrowCharge)
            {
                return;
            }

            if (throwChargeMode == ThrowChargeMode.QueuedRelay)
            {
                frozenThrowCharge01 = ThrowCharge01;
                throwChargeHeld = false;
                return;
            }

            ClearThrowCharge();
        }

        private void ClearThrowCharge()
        {
            throwChargeMode = ThrowChargeMode.None;
            throwChargeHeld = false;
            throwChargeStartedAt = 0f;
            frozenThrowCharge01 = 0f;
        }

        private bool TryResolveReticleHit(
            Vector3 interactionOrigin,
            out PrototypeInteractionTarget nearestTarget,
            out float nearestInteractionDistance)
        {
            Vector3 viewOrigin = viewTransform != null
                ? viewTransform.position
                : interactionOrigin;
            Vector3 forward = viewTransform != null
                ? viewTransform.forward
                : transform.forward;
            int hitCount = Physics.RaycastNonAlloc(
                viewOrigin,
                forward.normalized,
                visibilityHits,
                ReticleRayMaximumDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            if (hitCount >= VisibilityHitCapacity)
            {
                nearestTarget = null;
                nearestInteractionDistance = float.PositiveInfinity;
                return false;
            }

            nearestTarget = null;
            float nearestHitDistance = float.PositiveInfinity;
            nearestInteractionDistance = float.PositiveInfinity;
            string nearestKey = null;
            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = visibilityHits[index];
                if (hit.collider == null || BelongsToPlayer(hit.collider))
                {
                    continue;
                }

                PrototypeInteractionTarget hitTarget = FindTargetForCollider(hit.collider);
                float interactionDistance = hitTarget != null
                    ? Vector3.Distance(
                        interactionOrigin,
                        hit.collider.ClosestPoint(interactionOrigin))
                    : float.PositiveInfinity;
                string key = hitTarget != null
                    ? hitTarget.RuntimeInstanceKey ?? string.Empty
                    : null;
                bool isCloser = hit.distance < nearestHitDistance - 0.0001f;
                bool isSameDistance =
                    Mathf.Abs(hit.distance - nearestHitDistance) <= 0.0001f;
                bool isTargetTieWinner =
                    isSameDistance &&
                    hitTarget != null &&
                    nearestTarget != null &&
                    string.CompareOrdinal(key, nearestKey) < 0;
                bool blockerWinsTie =
                    isSameDistance && hitTarget == null && nearestTarget != null;
                if (isCloser || isTargetTieWinner || blockerWinsTie)
                {
                    nearestHitDistance = hit.distance;
                    nearestTarget = hitTarget;
                    nearestInteractionDistance = hitTarget != null
                        ? interactionDistance
                        : float.PositiveInfinity;
                    nearestKey = key;
                }
            }

            return true;
        }

        private bool TryResolveManualPickupAssist(Vector3 interactionOrigin)
        {
            Vector3 viewOrigin = viewTransform != null
                ? viewTransform.position
                : interactionOrigin;
            Vector3 forward = viewTransform != null
                ? viewTransform.forward.normalized
                : transform.forward.normalized;
            Vector3 right = viewTransform != null
                ? viewTransform.right.normalized
                : transform.right.normalized;
            Vector3 up = viewTransform != null
                ? viewTransform.up.normalized
                : transform.up.normalized;

            PrototypeInteractionTarget bestTarget = null;
            float bestHorizontalAngle = float.PositiveInfinity;
            float bestVerticalDistance = float.PositiveInfinity;
            float bestInteractionDistance = float.PositiveInfinity;
            string bestKey = null;

            foreach (PrototypeInteractionTarget target in targets)
            {
                if (target == null ||
                    !target.gameObject.activeInHierarchy ||
                    target == interactionOwner?.CarriedTarget ||
                    !target.IsScoopEligible)
                {
                    continue;
                }

                Collider targetCollider = target.GetComponent<Collider>();
                if (targetCollider == null)
                {
                    continue;
                }

                float interactionDistance = Vector3.Distance(
                    interactionOrigin,
                    targetCollider.ClosestPoint(interactionOrigin));
                if (interactionDistance > interactionRange)
                {
                    continue;
                }

                Vector3 targetPoint = targetCollider.bounds.center;
                Vector3 toTarget = targetPoint - viewOrigin;
                float forwardDistance = Vector3.Dot(toTarget, forward);
                if (forwardDistance <= 0.01f)
                {
                    continue;
                }

                float lateralDistance = Vector3.Dot(toTarget, right);
                float verticalDistance = Vector3.Dot(toTarget, up);
                float horizontalAngle = Mathf.Abs(
                    Mathf.Atan2(lateralDistance, forwardDistance) * Mathf.Rad2Deg);
                float verticalAngle = Mathf.Atan2(
                    verticalDistance,
                    Mathf.Sqrt(
                        (forwardDistance * forwardDistance) +
                        (lateralDistance * lateralDistance))) * Mathf.Rad2Deg;
                if (horizontalAngle > pickupAssistHorizontalAngle ||
                    verticalAngle > pickupAssistUpwardAngle ||
                    verticalAngle < -pickupAssistDownwardAngle ||
                    !HasDirectVisibility(viewOrigin, targetPoint, target))
                {
                    continue;
                }

                float verticalDistanceFromReticle = Mathf.Abs(verticalAngle);
                string key = target.RuntimeInstanceKey ?? string.Empty;
                bool betterHorizontal =
                    horizontalAngle < bestHorizontalAngle - 0.001f;
                bool sameHorizontal =
                    Mathf.Abs(horizontalAngle - bestHorizontalAngle) <= 0.001f;
                bool betterVertical =
                    sameHorizontal &&
                    verticalDistanceFromReticle < bestVerticalDistance - 0.001f;
                bool sameVertical =
                    sameHorizontal &&
                    Mathf.Abs(verticalDistanceFromReticle - bestVerticalDistance) <= 0.001f;
                bool betterDistance =
                    sameVertical &&
                    interactionDistance < bestInteractionDistance - 0.0001f;
                bool sameDistance =
                    sameVertical &&
                    Mathf.Abs(interactionDistance - bestInteractionDistance) <= 0.0001f;
                bool keyWins =
                    sameDistance &&
                    (bestTarget == null || string.CompareOrdinal(key, bestKey) < 0);
                if (!betterHorizontal && !betterVertical && !betterDistance && !keyWins)
                {
                    continue;
                }

                bestTarget = target;
                bestHorizontalAngle = horizontalAngle;
                bestVerticalDistance = verticalDistanceFromReticle;
                bestInteractionDistance = interactionDistance;
                bestKey = key;
            }

            if (bestTarget == null)
            {
                return false;
            }

            CurrentTarget = bestTarget;
            CurrentTargetDistance = bestInteractionDistance;
            CurrentTargetUsesPickupAssist = true;
            return true;
        }

        private PrototypeInteractionTarget ClearCurrentTarget()
        {
            CurrentTarget = null;
            CurrentTargetDistance = float.PositiveInfinity;
            CurrentTargetUsesPickupAssist = false;
            return null;
        }

        private bool TryApplyIncomingCatchAssist(Vector3 interactionOrigin)
        {
            PrototypeInteractionTarget catchTarget = allyRelayController?.ActiveTarget;
            if (catchTarget == null ||
                !allyRelayController.IsBinderIncoming ||
                !allyRelayController.CanCatch(catchTarget))
            {
                return false;
            }

            Collider targetCollider = catchTarget.GetComponent<Collider>();
            if (targetCollider == null)
            {
                return false;
            }

            Vector3 viewOrigin = viewTransform != null
                ? viewTransform.position
                : interactionOrigin;
            Vector3 targetPoint = targetCollider.bounds.center;
            Vector3 toTarget = targetPoint - viewOrigin;
            if (toTarget.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            Vector3 viewForward = viewTransform != null
                ? viewTransform.forward
                : transform.forward;
            if (Vector3.Dot(viewForward.normalized, toTarget.normalized) <
                CatchAssistMinimumForwardDot ||
                !HasDirectVisibility(viewOrigin, targetPoint, catchTarget))
            {
                return false;
            }

            CurrentTarget = catchTarget;
            CurrentTargetDistance = Vector3.Distance(
                interactionOrigin,
                targetCollider.ClosestPoint(interactionOrigin));
            CurrentTargetUsesPickupAssist = false;
            return true;
        }

        private bool HasDirectVisibility(
            Vector3 origin,
            Vector3 destination,
            PrototypeInteractionTarget target)
        {
            Vector3 displacement = destination - origin;
            float distance = displacement.magnitude;
            if (distance <= 0.01f)
            {
                return true;
            }

            int hitCount = Physics.RaycastNonAlloc(
                origin,
                displacement / distance,
                visibilityHits,
                distance + 0.01f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            if (hitCount >= VisibilityHitCapacity)
            {
                return false;
            }

            float nearestDistance = float.PositiveInfinity;
            Collider nearestCollider = null;
            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = visibilityHits[index];
                if (hit.collider == null || BelongsToPlayer(hit.collider))
                {
                    continue;
                }

                if (hit.distance < nearestDistance)
                {
                    nearestDistance = hit.distance;
                    nearestCollider = hit.collider;
                }
            }

            return nearestCollider != null &&
                   (nearestCollider.transform == target.transform ||
                    nearestCollider.transform.IsChildOf(target.transform));
        }

        private bool BelongsToPlayer(Collider candidate)
        {
            return candidate.transform == transform || candidate.transform.IsChildOf(transform);
        }

        private PrototypeInteractionTarget FindTargetForCollider(Collider candidate)
        {
            foreach (PrototypeInteractionTarget target in targets)
            {
                if (target == null ||
                    !target.gameObject.activeInHierarchy ||
                    target == interactionOwner?.CarriedTarget)
                {
                    continue;
                }

                if (candidate.transform == target.transform ||
                    candidate.transform.IsChildOf(target.transform))
                {
                    return target;
                }
            }

            return null;
        }
    }
}
