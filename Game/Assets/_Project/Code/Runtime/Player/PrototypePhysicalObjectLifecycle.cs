using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    [DefaultExecutionOrder(-90)]
    [DisallowMultipleComponent]
    public sealed class PrototypePhysicalObjectLifecycle : MonoBehaviour
    {
        private const float CarryCollisionSkin = 0.02f;
        private const float CarryPenetrationTolerance = 0.001f;
        private const float CarryRotationCollisionStep = 2f;
        private const int CarryCollisionCapacity = 32;
        private const int CarryCollisionResolveIterations = 8;

        [SerializeField]
        private PrototypeInteractionStateOwner interactionOwner;

        [SerializeField]
        private Transform carryAnchor;

        [SerializeField, Min(0.1f)]
        private float acquisitionSpeed = 7f;

        [SerializeField, Min(0.1f)]
        private float carryRotationSpeed = 540f;

        [SerializeField, Min(0.1f)]
        private float lightThrowSpeed = 10f;

        [SerializeField, Min(0.1f)]
        private float mediumThrowSpeed = 6.5f;

        [SerializeField]
        private float upwardThrowSpeed = 0.8f;

        private CharacterController playerController;
        private PrototypeInteractionTarget carriedTarget;
        private Rigidbody carriedBody;
        private Collider carriedCollider;
        private BoxCollider carriedBoxCollider;
        private bool playerCollisionIgnored;
        private int carriedOriginalLayer;
        private bool carriedCameraCollisionIgnored;
        private uint nextThrowOriginSequence;
        private readonly RaycastHit[] carrySweepHits = new RaycastHit[CarryCollisionCapacity];
        private readonly Collider[] carryOverlapHits = new Collider[CarryCollisionCapacity];

        public PrototypeInteractionTarget CarriedTarget => carriedTarget;

        public PlayerThrowOrigin LastThrowOrigin { get; private set; }

        public uint ThrowOriginCount => nextThrowOriginSequence;

        public float AcquisitionPathLength { get; private set; }

        public float LastReleaseSpeed { get; private set; }

        public void Configure(
            PrototypeInteractionStateOwner owner,
            Transform authoredCarryAnchor)
        {
            interactionOwner = owner;
            carryAnchor = authoredCarryAnchor;
            playerController = GetComponent<CharacterController>();
        }

        public bool BeginCarry(PrototypeInteractionTarget target)
        {
            if (target == null ||
                interactionOwner == null ||
                interactionOwner.HandState != PrototypeHandOwnershipState.Carrying ||
                interactionOwner.CarriedTarget != target ||
                carriedTarget != null ||
                (!target.IsScoopEligible && !target.CatchEligible))
            {
                return false;
            }

            Rigidbody body = target.GetComponent<Rigidbody>();
            Collider targetCollider = target.GetComponent<Collider>();
            if (body == null ||
                targetCollider is not BoxCollider targetBoxCollider ||
                targetBoxCollider.transform != body.transform)
            {
                return false;
            }

            carriedTarget = target;
            carriedBody = body;
            carriedCollider = targetCollider;
            carriedBoxCollider = targetBoxCollider;
            AcquisitionPathLength = 0f;

            carriedBody.linearVelocity = Vector3.zero;
            carriedBody.angularVelocity = Vector3.zero;
            carriedBody.useGravity = false;
            carriedBody.isKinematic = true;
            SetPlayerCollisionIgnored(true);
            SetCarriedCameraCollisionIgnored(true);
            return true;
        }

        public bool TryThrow(Vector3 requestedDirection)
        {
            if (carriedTarget == null || carriedBody == null || interactionOwner == null)
            {
                return false;
            }

            PrototypeInteractionTarget target = carriedTarget;
            Rigidbody body = carriedBody;
            if (!interactionOwner.TryCommitThrow(out PrototypeInteractionTarget committedTarget) ||
                committedTarget != target)
            {
                return false;
            }

            SetPlayerCollisionIgnored(false);
            SetCarriedCameraCollisionIgnored(false);
            ApplyPhysicalThrow(target, body, requestedDirection);

            carriedTarget = null;
            carriedBody = null;
            carriedCollider = null;
            carriedBoxCollider = null;
            interactionOwner.CompleteThrow(target);
            return true;
        }

        public bool TryThrowCommittedRelayTarget(
            PrototypeInteractionTarget target,
            Vector3 requestedDirection)
        {
            if (target == null ||
                interactionOwner == null ||
                interactionOwner.BlocksNewInteraction ||
                interactionOwner.HandState != PrototypeHandOwnershipState.ThrowCommitted ||
                interactionOwner.ThrowCommittedTarget != target ||
                !target.CatchEligible)
            {
                return false;
            }

            Rigidbody body = target.GetComponent<Rigidbody>();
            Collider targetCollider = target.GetComponent<Collider>();
            if (body == null ||
                targetCollider is not BoxCollider ||
                targetCollider.transform != body.transform)
            {
                return false;
            }

            ApplyPhysicalThrow(target, body, requestedDirection);
            return interactionOwner.CompleteThrow(target);
        }

        public float GetThrowSpeed(PrototypeWeightClass weightClass)
        {
            return weightClass == PrototypeWeightClass.Light
                ? lightThrowSpeed
                : mediumThrowSpeed;
        }

        private void ApplyPhysicalThrow(
            PrototypeInteractionTarget target,
            Rigidbody body,
            Vector3 requestedDirection)
        {
            Vector3 direction = requestedDirection.sqrMagnitude > 0.0001f
                ? requestedDirection.normalized
                : transform.forward;
            float throwSpeed = GetThrowSpeed(target.WeightClass);
            Vector3 releaseVelocity = direction * throwSpeed + Vector3.up * upwardThrowSpeed;

            body.isKinematic = false;
            body.useGravity = true;
            body.linearVelocity = releaseVelocity;
            body.angularVelocity = Vector3.up *
                (target.WeightClass == PrototypeWeightClass.Light ? 4f : 2f);
            body.WakeUp();

            nextThrowOriginSequence++;
            LastThrowOrigin = new PlayerThrowOrigin(
                nextThrowOriginSequence,
                target.ContentDefinitionId,
                target.RuntimeInstanceKey,
                target.WeightClass);
            LastReleaseSpeed = releaseVelocity.magnitude;
        }

        public void DetachForReset()
        {
            SetPlayerCollisionIgnored(false);
            SetCarriedCameraCollisionIgnored(false);
            if (carriedBody != null)
            {
                carriedBody.isKinematic = false;
                carriedBody.useGravity = true;
                carriedBody.linearVelocity = Vector3.zero;
                carriedBody.angularVelocity = Vector3.zero;
            }

            carriedTarget = null;
            carriedBody = null;
            carriedCollider = null;
            carriedBoxCollider = null;
            AcquisitionPathLength = 0f;
            LastReleaseSpeed = 0f;
        }

        public void ClearThrowOriginForReset()
        {
            LastThrowOrigin = default;
            nextThrowOriginSequence = 0;
        }

        private void FixedUpdate()
        {
            if (carriedTarget == null || carriedBody == null || carryAnchor == null)
            {
                return;
            }

            if (interactionOwner == null ||
                interactionOwner.HandState != PrototypeHandOwnershipState.Carrying ||
                interactionOwner.CarriedTarget != carriedTarget)
            {
                DetachForReset();
                return;
            }

            Vector3 weightOffset = carriedTarget.WeightClass == PrototypeWeightClass.Medium
                ? transform.forward * 0.45f + Vector3.down * 0.25f
                : Vector3.zero;
            Vector3 destination = carryAnchor.position + weightOffset;
            Vector3 before = carriedBody.position;
            Vector3 nextPosition = Vector3.MoveTowards(
                before,
                destination,
                acquisitionSpeed * Time.fixedDeltaTime);
            Quaternion nextRotation = Quaternion.RotateTowards(
                carriedBody.rotation,
                transform.rotation,
                carryRotationSpeed * Time.fixedDeltaTime);

            Quaternion currentRotation = carriedBody.rotation;
            nextPosition = ResolveCarryTranslation(before, nextPosition, currentRotation);
            if (WouldCarryPosePenetrate(nextPosition, currentRotation))
            {
                nextPosition = ResolveCarryPenetration(before, nextPosition, currentRotation);
            }

            nextRotation = ResolveCarryRotation(nextPosition, currentRotation, nextRotation);
            TryRestorePlayerCollision(nextPosition, nextRotation);

            carriedBody.MovePosition(nextPosition);
            carriedBody.MoveRotation(nextRotation);
            AcquisitionPathLength += Vector3.Distance(before, nextPosition);
        }

        private Vector3 ResolveCarryTranslation(
            Vector3 currentPosition,
            Vector3 requestedPosition,
            Quaternion currentRotation)
        {
            if (carriedBoxCollider == null || carriedBoxCollider.transform != carriedBody.transform)
            {
                return requestedPosition;
            }

            Vector3 displacement = requestedPosition - currentPosition;
            float requestedDistance = displacement.magnitude;
            if (requestedDistance <= Mathf.Epsilon)
            {
                return currentPosition;
            }

            Vector3 direction = displacement / requestedDistance;
            float permittedDistance = GetPermittedCarryDistance(
                currentPosition,
                direction,
                requestedDistance,
                currentRotation,
                out Vector3 blockingNormal);
            Vector3 resolvedPosition = currentPosition + direction * permittedDistance;
            if (permittedDistance >= requestedDistance - Mathf.Epsilon)
            {
                return resolvedPosition;
            }

            Vector3 remainingDisplacement = requestedPosition - resolvedPosition;
            Vector3 slideDisplacement = Vector3.ProjectOnPlane(
                remainingDisplacement,
                blockingNormal);
            float slideDistance = slideDisplacement.magnitude;
            if (slideDistance <= Mathf.Epsilon)
            {
                return resolvedPosition;
            }

            Vector3 slideDirection = slideDisplacement / slideDistance;
            float permittedSlideDistance = GetPermittedCarryDistance(
                resolvedPosition,
                slideDirection,
                slideDistance,
                currentRotation,
                out _);
            return resolvedPosition + slideDirection * permittedSlideDistance;
        }

        private float GetPermittedCarryDistance(
            Vector3 currentPosition,
            Vector3 direction,
            float requestedDistance,
            Quaternion currentRotation,
            out Vector3 blockingNormal)
        {
            Vector3 halfExtents = GetCarryBoxHalfExtents();
            Vector3 queryHalfExtents = new Vector3(
                Mathf.Max(0.001f, halfExtents.x - CarryPenetrationTolerance),
                Mathf.Max(0.001f, halfExtents.y - CarryPenetrationTolerance),
                Mathf.Max(0.001f, halfExtents.z - CarryPenetrationTolerance));
            Vector3 center = GetCarryBoxCenter(currentPosition, currentRotation);
            int hitCount = Physics.BoxCastNonAlloc(
                center,
                queryHalfExtents,
                direction,
                carrySweepHits,
                currentRotation,
                requestedDistance + CarryCollisionSkin,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore);

            blockingNormal = -direction;
            if (hitCount >= CarryCollisionCapacity)
            {
                return 0f;
            }

            float permittedDistance = requestedDistance;
            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = carrySweepHits[index];
                if (!IsCarryBlockingCollider(hit.collider))
                {
                    continue;
                }

                // BoxCast reports an authored support contact as a zero-distance
                // initial overlap whose normal points opposite the whole sweep.
                // Ignore only a surface at or below the carried box while the
                // requested step lifts away from it. Wall and lateral contacts
                // continue through the normal blocking/slide path below.
                if (hit.distance <= CarryCollisionSkin + CarryPenetrationTolerance &&
                    IsLiftingAwayFromSupportingSurface(
                        hit.collider,
                        currentPosition,
                        currentRotation,
                        direction))
                {
                    continue;
                }

                if (Vector3.Dot(direction, hit.normal) >= -CarryPenetrationTolerance)
                {
                    continue;
                }

                float candidateDistance = Mathf.Max(
                    0f,
                    hit.distance - CarryCollisionSkin - CarryPenetrationTolerance);
                if (candidateDistance < permittedDistance)
                {
                    permittedDistance = candidateDistance;
                    blockingNormal = hit.normal;
                }
            }

            return permittedDistance;
        }

        private bool IsLiftingAwayFromSupportingSurface(
            Collider other,
            Vector3 currentPosition,
            Quaternion currentRotation,
            Vector3 direction)
        {
            if (direction.y <= CarryPenetrationTolerance)
            {
                return false;
            }

            Vector3 halfExtents = GetCarryBoxHalfExtents();
            Vector3 worldRight = currentRotation * Vector3.right;
            Vector3 worldUp = currentRotation * Vector3.up;
            Vector3 worldForward = currentRotation * Vector3.forward;
            float verticalHalfExtent =
                Mathf.Abs(worldRight.y) * halfExtents.x +
                Mathf.Abs(worldUp.y) * halfExtents.y +
                Mathf.Abs(worldForward.y) * halfExtents.z;
            float carriedBottom =
                GetCarryBoxCenter(currentPosition, currentRotation).y -
                verticalHalfExtent;
            return other.bounds.max.y <=
                carriedBottom + CarryCollisionSkin + CarryPenetrationTolerance;
        }

        private Quaternion ResolveCarryRotation(
            Vector3 position,
            Quaternion currentRotation,
            Quaternion requestedRotation)
        {
            float requestedAngle = Quaternion.Angle(currentRotation, requestedRotation);
            if (requestedAngle <= Mathf.Epsilon)
            {
                return currentRotation;
            }

            int stepCount = Mathf.Max(
                1,
                Mathf.CeilToInt(requestedAngle / CarryRotationCollisionStep));
            Quaternion acceptedRotation = currentRotation;
            for (int step = 1; step <= stepCount; step++)
            {
                Quaternion candidate = Quaternion.Slerp(
                    currentRotation,
                    requestedRotation,
                    step / (float)stepCount);
                if (WouldCarryPosePenetrate(position, candidate))
                {
                    break;
                }

                acceptedRotation = candidate;
            }

            return acceptedRotation;
        }

        private Vector3 ResolveCarryPenetration(
            Vector3 safePosition,
            Vector3 requestedPosition,
            Quaternion requestedRotation)
        {
            if (WouldCarryPosePenetrate(safePosition, requestedRotation))
            {
                return safePosition;
            }

            Vector3 accepted = safePosition;
            Vector3 rejected = requestedPosition;
            for (int iteration = 0; iteration < CarryCollisionResolveIterations; iteration++)
            {
                Vector3 candidate = Vector3.Lerp(accepted, rejected, 0.5f);
                if (WouldCarryPosePenetrate(candidate, requestedRotation))
                {
                    rejected = candidate;
                }
                else
                {
                    accepted = candidate;
                }
            }

            return accepted;
        }

        private bool WouldCarryPosePenetrate(Vector3 position, Quaternion rotation)
        {
            if (carriedBoxCollider == null || carriedBoxCollider.transform != carriedBody.transform)
            {
                return false;
            }

            int overlapCount = Physics.OverlapBoxNonAlloc(
                GetCarryBoxCenter(position, rotation),
                GetCarryBoxHalfExtents(),
                carryOverlapHits,
                rotation,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore);
            if (overlapCount >= CarryCollisionCapacity)
            {
                return true;
            }

            for (int index = 0; index < overlapCount; index++)
            {
                Collider other = carryOverlapHits[index];
                if (!IsCarryBlockingCollider(other))
                {
                    continue;
                }

                if (Physics.ComputePenetration(
                        carriedBoxCollider,
                        position,
                        rotation,
                        other,
                        other.transform.position,
                        other.transform.rotation,
                        out _,
                        out float distance) &&
                    distance > CarryPenetrationTolerance)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsCarryBlockingCollider(Collider other)
        {
            bool belongsToPlayer = other != null &&
                (other == playerController || other.transform.IsChildOf(transform));
            if (other == null ||
                other == carriedCollider ||
                other.attachedRigidbody == carriedBody ||
                (belongsToPlayer && playerCollisionIgnored))
            {
                return false;
            }

            if (Physics.GetIgnoreLayerCollision(
                    carriedCollider.gameObject.layer,
                    other.gameObject.layer))
            {
                return false;
            }

            return !Physics.GetIgnoreCollision(carriedCollider, other);
        }

        private void TryRestorePlayerCollision(Vector3 position, Quaternion rotation)
        {
            if (!playerCollisionIgnored ||
                playerController == null ||
                carriedBoxCollider == null)
            {
                return;
            }

            bool overlapsPlayer = Physics.ComputePenetration(
                carriedBoxCollider,
                position,
                rotation,
                playerController,
                playerController.transform.position,
                playerController.transform.rotation,
                out _,
                out float distance) &&
                distance > CarryPenetrationTolerance;
            if (!overlapsPlayer)
            {
                SetPlayerCollisionIgnored(false);
            }
        }

        private Vector3 GetCarryBoxCenter(Vector3 position, Quaternion rotation)
        {
            Vector3 scaledCenter = Vector3.Scale(
                carriedBoxCollider.center,
                carriedBoxCollider.transform.lossyScale);
            return position + rotation * scaledCenter;
        }

        private Vector3 GetCarryBoxHalfExtents()
        {
            Vector3 scale = carriedBoxCollider.transform.lossyScale;
            scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            return Vector3.Scale(carriedBoxCollider.size * 0.5f, scale);
        }

        private void SetPlayerCollisionIgnored(bool ignored)
        {
            if (playerController == null)
            {
                playerController = GetComponent<CharacterController>();
            }

            if (playerController == null || carriedCollider == null || playerCollisionIgnored == ignored)
            {
                return;
            }

            Physics.IgnoreCollision(playerController, carriedCollider, ignored);
            playerCollisionIgnored = ignored;
        }

        private void SetCarriedCameraCollisionIgnored(bool ignored)
        {
            if (ignored)
            {
                if (carriedTarget == null || carriedCameraCollisionIgnored)
                {
                    return;
                }

                carriedOriginalLayer = carriedTarget.gameObject.layer;
                carriedTarget.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                carriedCameraCollisionIgnored = true;
                return;
            }

            if (carriedCameraCollisionIgnored && carriedTarget != null)
            {
                carriedTarget.gameObject.layer = carriedOriginalLayer;
            }

            carriedCameraCollisionIgnored = false;
        }
    }
}
