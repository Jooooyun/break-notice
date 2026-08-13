using System;
using System.Collections.Generic;
using BreakNotice.Gameplay.Input;
using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    [DefaultExecutionOrder(-40)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseCargoStrapController : MonoBehaviour
    {
        private const int VisibilityHitCapacity = 32;

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

        private readonly RaycastHit[] visibilityHits =
            new RaycastHit[VisibilityHitCapacity];
        private PrototypeInteractionTarget activeTarget;
        private Rigidbody activeBody;
        private Collider activeCollider;
        private RigidbodyConstraints originalConstraints;
        private CollisionDetectionMode originalCollisionDetection;
        private RigidbodyInterpolation originalInterpolation;
        private bool originalWasKinematic;
        private bool originalUsedGravity;
        private bool playerCollisionIgnored;

        public PrototypeInteractionTarget ActiveTarget => activeTarget;

        public PrototypeInteractionTarget AssistCandidate { get; private set; }

        public bool IsAttached => interactionOwner != null &&
            interactionOwner.StrapState != PrototypeStrapOwnershipState.Stored &&
            activeTarget != null;

        public bool IsTensioned => interactionOwner != null &&
            interactionOwner.StrapState == PrototypeStrapOwnershipState.Tensioned &&
            activeTarget != null;

        public float CurrentStrapLength => activeTarget == null || playerTransform == null
            ? 0f
            : Vector3.Distance(GetReelPosition(), GetAnchorPosition());

        public bool IsWarning => IsAttached && CurrentStrapLength >= warningDistance;

        public int AssistedAttachCount { get; private set; }

        public int DirectAttachCount { get; private set; }

        public int ReleaseCount { get; private set; }

        public int RedirectStepCount { get; private set; }

        public int FaultCount { get; private set; }

        public string LastFault { get; private set; } = string.Empty;

        public float AssistAttachRange => assistAttachRange;

        public float MaximumAllyAssistRange => maximumAllyAssistRange;

        public float RedirectSpeed => redirectSpeed;

        public float MaximumStrapLength => maximumStrapLength;

        public float WarningDistance => warningDistance;

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
            if (strapLine == null)
            {
                strapLine = GetComponent<LineRenderer>();
            }

            SetLineVisible(false);
        }

        private void Update()
        {
            SynchronizeLogicalAttachment();
            RefreshAssistCandidate();
        }

        private void FixedUpdate()
        {
            SynchronizeLogicalAttachment();
            if (!IsAttached ||
                interactionOwner.BlocksNewInteraction ||
                logicalInput == null ||
                activeBody == null)
            {
                return;
            }

            Vector2 moveInput = logicalInput.Snapshot.Move;
            if (moveInput.sqrMagnitude <= 0.01f)
            {
                return;
            }

            if (interactionOwner.StrapState == PrototypeStrapOwnershipState.Attached)
            {
                interactionOwner.TryBeginStrapTension(activeTarget);
            }

            Vector3 forward = Vector3.ProjectOnPlane(playerTransform.forward, Vector3.up)
                .normalized;
            Vector3 right = Vector3.ProjectOnPlane(playerTransform.right, Vector3.up)
                .normalized;
            Vector3 direction = (forward * moveInput.y) + (right * moveInput.x);
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }
            direction.Normalize();

            Vector3 fromPlayer = Vector3.ProjectOnPlane(
                activeBody.worldCenterOfMass - playerTransform.position,
                Vector3.up);
            if (fromPlayer.sqrMagnitude > 0.001f &&
                CurrentStrapLength >= maximumStrapLength &&
                Vector3.Dot(direction, fromPlayer.normalized) > 0f)
            {
                direction = Vector3.ProjectOnPlane(direction, fromPlayer.normalized);
                if (direction.sqrMagnitude <= 0.0001f)
                {
                    activeBody.linearVelocity = new Vector3(
                        0f,
                        Mathf.Min(activeBody.linearVelocity.y, 0f),
                        0f);
                    return;
                }
                direction.Normalize();
            }

            Vector3 velocity = direction * redirectSpeed;
            velocity.y = Mathf.Min(activeBody.linearVelocity.y, 0f);
            activeBody.linearVelocity = velocity;
            activeBody.angularVelocity = Vector3.zero;
            RedirectStepCount++;
        }

        private void LateUpdate()
        {
            if (!IsAttached || strapLine == null)
            {
                SetLineVisible(false);
                return;
            }

            SetLineVisible(true);
            strapLine.SetPosition(0, GetReelPosition());
            strapLine.SetPosition(1, GetAnchorPosition());
            Color color = IsWarning
                ? new Color(1f, 0.3f, 0.08f, 1f)
                : new Color(1f, 0.78f, 0.08f, 1f);
            strapLine.startColor = color;
            strapLine.endColor = color;
        }

        private void OnDisable()
        {
            ClearPhysicalAttachment();
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
            SetLineVisible(false);
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
            if (interactionOwner == null ||
                interactionOwner.GlobalState != PrototypeGlobalInteractionState.Playable ||
                interactionOwner.HandState != PrototypeHandOwnershipState.Free ||
                interactionOwner.StrapState != PrototypeStrapOwnershipState.Stored ||
                interactionOwner.RelayState != PrototypeRelayReservationState.None ||
                !IsEligibleChair(target) ||
                !TryGetPhysicalTarget(target, out _, out Collider targetCollider))
            {
                return false;
            }

            return IsFloorSupported(targetCollider);
        }

        public bool TryAssistAttach(out PrototypeInteractionTarget target)
        {
            RefreshAssistCandidate();
            target = AssistCandidate;
            if (target == null || interactionOwner == null)
            {
                return false;
            }

            PrototypeInteractionDecision decision =
                PrototypeInteractionDecision.Accept(
                    PrototypeInteractionOutcome.AttachStrap,
                    target);
            if (!interactionOwner.TryCommit(decision))
            {
                return false;
            }

            if (!BeginPhysicalAttachment(target))
            {
                interactionOwner.TryReleaseStrap();
                target = null;
                return false;
            }

            AssistedAttachCount++;
            RefreshAssistCandidate();
            return true;
        }

        public bool TryExplicitRelease()
        {
            if (interactionOwner == null || !interactionOwner.TryReleaseStrap())
            {
                return false;
            }

            ReleaseCount++;
            ClearPhysicalAttachment();
            RefreshAssistCandidate();
            return true;
        }

        public bool NotifyDirectAttachment(PrototypeInteractionTarget target)
        {
            if (target == null || interactionOwner?.StrapTarget != target)
            {
                return false;
            }

            if (!BeginPhysicalAttachment(target))
            {
                interactionOwner.TryReleaseStrap();
                return false;
            }

            DirectAttachCount++;
            return true;
        }

        public void ResetToRunBaseline()
        {
            ClearPhysicalAttachment();
            AssistCandidate = null;
            AssistedAttachCount = 0;
            DirectAttachCount = 0;
            ReleaseCount = 0;
            RedirectStepCount = 0;
            FaultCount = 0;
            LastFault = string.Empty;
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
            if (!IsEligibleChair(target) || body == null || targetCollider == null)
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
            body.constraints = originalConstraints |
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationZ;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            if (playerCollider != null)
            {
                Physics.IgnoreCollision(playerCollider, activeCollider, true);
                playerCollisionIgnored = true;
            }
            SetLineVisible(true);
            return true;
        }

        private void ClearPhysicalAttachment()
        {
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
            SetLineVisible(false);
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
            if (interactionOwner == null ||
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
                if (!IsEligibleChair(target) ||
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

        private static bool IsEligibleChair(PrototypeInteractionTarget target)
        {
            return target != null &&
                target.WeightClass == PrototypeWeightClass.Medium &&
                target.StrapEligible &&
                target.HasLoadBearingMetalAnchor &&
                (target.Role == PrototypeInteractionRole.DirectChair ||
                 target.Role == PrototypeInteractionRole.AllyChair);
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
            return playerTransform.position +
                (playerTransform.right * 0.28f) +
                (Vector3.up * 0.9f);
        }

        private Vector3 GetAnchorPosition()
        {
            return activeCollider != null
                ? activeCollider.bounds.center
                : activeTarget.transform.position;
        }

        private void SetLineVisible(bool visible)
        {
            if (strapLine == null)
            {
                return;
            }

            strapLine.enabled = visible;
            strapLine.positionCount = 2;
        }

        private void RegisterFault(string reason)
        {
            FaultCount++;
            LastFault = reason ?? string.Empty;
        }
    }
}
