using System;
using BreakNotice.Gameplay.Input;
using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public sealed class PrototypeInteractionController : MonoBehaviour
    {
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

        [SerializeField, Range(-1f, 1f)]
        private float minimumForwardDot = 0.25f;

        [SerializeField]
        private bool strapContextEnabled;

        private readonly PrototypeInteractionArbitrator arbitrator =
            new PrototypeInteractionArbitrator();

        public PrototypeInteractionStateOwner InteractionOwner => interactionOwner;

        public PrototypeInteractionTarget CurrentTarget { get; private set; }

        public PrototypeInteractionDecision LastDecision { get; private set; }

        public float InteractionRange => interactionRange;

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

        private void Update()
        {
            if (logicalInput == null || interactionOwner == null)
            {
                return;
            }

            bool isAiming = logicalInput.Snapshot.Aim;
            RefreshTarget(isAiming);

            if (logicalInput.Snapshot.InteractPrimary)
            {
                ProcessInteractPrimary(isAiming);
            }
        }

        public void Configure(
            InputSystemLogicalInputAdapter input,
            PrototypeInteractionStateOwner owner,
            Transform view,
            PrototypeInteractionTarget[] authoredTargets,
            float range,
            float forwardDot,
            bool enableStrapContext)
        {
            logicalInput = input;
            interactionOwner = owner;
            viewTransform = view;
            targets = authoredTargets ?? Array.Empty<PrototypeInteractionTarget>();
            interactionRange = Mathf.Max(0.5f, range);
            minimumForwardDot = Mathf.Clamp(forwardDot, -1f, 1f);
            strapContextEnabled = enableStrapContext;
        }

        public PrototypeInteractionTarget RefreshTarget(bool isAiming)
        {
            Vector3 origin = transform.position + Vector3.up;
            Vector3 forward = isAiming && viewTransform != null
                ? viewTransform.forward
                : transform.forward;

            PrototypeInteractionTarget bestTarget = null;
            float bestScore = float.PositiveInfinity;
            string bestKey = null;

            foreach (PrototypeInteractionTarget target in targets)
            {
                if (target == null || !target.gameObject.activeInHierarchy || target == interactionOwner?.CarriedTarget)
                {
                    continue;
                }

                // Authored target pivots are the deterministic selection points. This avoids
                // depending on Physics transform synchronization before the first simulation step.
                Vector3 targetPoint = target.transform.position;
                Vector3 offset = targetPoint - origin;
                float distance = offset.magnitude;
                if (distance > interactionRange || distance <= 0.001f)
                {
                    continue;
                }

                float forwardDot = Vector3.Dot(forward.normalized, offset / distance);
                if (forwardDot < minimumForwardDot)
                {
                    continue;
                }

                float score = distance + ((1f - forwardDot) * 0.75f);
                string key = target.RuntimeInstanceKey ?? string.Empty;
                if (score < bestScore - 0.0001f ||
                    (Mathf.Abs(score - bestScore) <= 0.0001f &&
                     string.CompareOrdinal(key, bestKey) < 0))
                {
                    bestTarget = target;
                    bestScore = score;
                    bestKey = key;
                }
            }

            CurrentTarget = bestTarget;
            return CurrentTarget;
        }

        public PrototypeInteractionDecision ProcessInteractPrimary(bool isAiming)
        {
            if (interactionOwner == null)
            {
                LastDecision = PrototypeInteractionDecision.Reject(
                    PrototypeInteractionRejection.OwnershipConflict,
                    CurrentTarget);
                return LastDecision;
            }

            RefreshTarget(isAiming);
            bool explicitStrapContext =
                strapContextEnabled &&
                isAiming &&
                CurrentTarget != null &&
                CurrentTarget.HasLoadBearingMetalAnchor;

            PrototypeInteractionContext context = interactionOwner.CreateContext(
                explicitStrapContext,
                CurrentTarget);
            LastDecision = arbitrator.Decide(context);

            if (LastDecision.IsAccepted && !interactionOwner.TryCommit(LastDecision))
            {
                LastDecision = PrototypeInteractionDecision.Reject(
                    PrototypeInteractionRejection.OwnershipConflict,
                    CurrentTarget);
            }

            return LastDecision;
        }
    }
}
