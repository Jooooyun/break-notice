using System;
using BreakNotice.Gameplay.Input;
using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class PrototypeInteractionController : MonoBehaviour
    {
        private const int VisibilityHitCapacity = 32;
        private const float ReticleRayMaximumDistance = 100f;

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

        [SerializeField]
        private bool strapContextEnabled;

        [SerializeField]
        private PrototypePhysicalObjectLifecycle physicalObjectLifecycle;

        [SerializeField]
        private PrototypeAllyRelayController allyRelayController;

        private readonly PrototypeInteractionArbitrator arbitrator =
            new PrototypeInteractionArbitrator();
        private readonly RaycastHit[] visibilityHits = new RaycastHit[VisibilityHitCapacity];
#if UNITY_WEBGL && !UNITY_EDITOR
        private bool pointerWasLocked;
#endif

        public PrototypeInteractionStateOwner InteractionOwner => interactionOwner;

        public PrototypeInteractionTarget CurrentTarget { get; private set; }

        public PrototypeInteractionTarget CurrentRelayCandidate { get; private set; }

        public PrototypeInteractionDecision LastDecision { get; private set; }

        public float InteractionRange => interactionRange;

        public float CurrentTargetDistance { get; private set; } = float.PositiveInfinity;

        public uint InteractionAttemptSequence { get; private set; }

        public uint AllyCallAttemptSequence { get; private set; }

        public bool LastAllyCallAccepted { get; private set; }

        public PrototypeAllyRelayRejection LastAllyCallRejection { get; private set; }

        public PrototypeInteractionTarget LastAllyCallTarget { get; private set; }

        public PrototypeAllyRelayController AllyRelayController => allyRelayController;

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
            if (logicalInput == null || interactionOwner == null)
            {
                return;
            }

            bool isAiming = logicalInput.Snapshot.Aim;
            RefreshTarget(isAiming);
            if (logicalInput.Snapshot.ResetEncounter)
            {
                // Reset owns this entire logical frame even though the authored
                // restore barrier completes earlier in LateUpdate.
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

            if (logicalInput.Snapshot.CallAlly)
            {
                ProcessCallAlly();
            }

            if (logicalInput.Snapshot.InteractPrimary)
            {
                ProcessInteractPrimary(isAiming && pointerActionAvailable);
            }

            if (logicalInput.Snapshot.Throw && pointerActionAvailable)
            {
                ProcessThrow();
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
            PrototypeAllyRelayController relayController = null)
        {
            logicalInput = input;
            interactionOwner = owner;
            viewTransform = view;
            targets = authoredTargets ?? Array.Empty<PrototypeInteractionTarget>();
            interactionRange = Mathf.Max(0.5f, range);
            strapContextEnabled = enableStrapContext;
            physicalObjectLifecycle = physicalLifecycle;
            allyRelayController = relayController;
        }

        public PrototypeInteractionTarget RefreshTarget(bool isAiming)
        {
            // The centre reticle ray is the exact selector in both normal and Aim.
            // isAiming remains part of the public contract because it controls the
            // explicit Strap context outside this resolver.
            _ = isAiming;
            Vector3 interactionOrigin = transform.position + Vector3.up;
            if (!TryResolveReticleHit(
                    interactionOrigin,
                    out PrototypeInteractionTarget reticleTarget,
                    out float interactionDistance))
            {
                return ClearCurrentTarget();
            }

            CurrentTarget = reticleTarget != null && interactionDistance <= interactionRange
                ? reticleTarget
                : null;
            CurrentTargetDistance = CurrentTarget != null
                ? interactionDistance
                : float.PositiveInfinity;
            CurrentRelayCandidate =
                allyRelayController != null &&
                allyRelayController.CanRequestSupply(reticleTarget, out _)
                    ? reticleTarget
                    : null;
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
                        direction);
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
            return physicalObjectLifecycle.TryThrow(direction);
        }

        public bool ProcessCallAlly()
        {
            RefreshTarget(false);
            PrototypeInteractionTarget candidate = CurrentRelayCandidate;
            PrototypeAllyRelayRejection rejection =
                PrototypeAllyRelayRejection.AllyUnavailable;
            bool accepted = allyRelayController != null &&
                allyRelayController.TryRequestSupply(
                    candidate,
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

        private PrototypeInteractionTarget ClearCurrentTarget()
        {
            CurrentTarget = null;
            CurrentRelayCandidate = null;
            CurrentTargetDistance = float.PositiveInfinity;
            return null;
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
