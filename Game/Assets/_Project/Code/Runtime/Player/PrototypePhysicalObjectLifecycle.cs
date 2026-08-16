using System.Collections.Generic;
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
        private const int CollisionRestoreClearFixedSteps = 2;
        private const float GripWorldBlockReleaseDelay = 0.05f;

        [SerializeField]
        private PrototypeInteractionStateOwner interactionOwner;

        [SerializeField]
        private Transform carryAnchor;

        [SerializeField]
        private ShowcaseBinderGripSocket lightBinderGripSocket;

        [SerializeField]
        private bool requireBoneTrackedLightGrip;

        [SerializeField, Min(0.02f)]
        private float maximumGripSeparation = 0.18f;

        [SerializeField, Min(0.1f)]
        private float acquisitionSpeed = 7f;

        [SerializeField, Min(0.1f)]
        private float carryRotationSpeed = 540f;

        [SerializeField, Min(0.1f)]
        private float lightThrowSpeed = 12.5f;

        [SerializeField, Min(0.1f)]
        private float mediumThrowSpeed = 8f;

        [SerializeField, Range(1f, 3f)]
        private float maximumThrowSpeedMultiplier = 1.6f;

        private CharacterController playerController;
        private PrototypeInteractionTarget carriedTarget;
        private Rigidbody carriedBody;
        private Collider carriedCollider;
        private BoxCollider carriedBoxCollider;
        private int carriedOriginalLayer;
        private bool carriedCameraCollisionIgnored;
        private CarryCollisionLease activeCarryCollisionLease;
        private readonly List<CarryCollisionLease> pendingCollisionRestores = new();
        private uint nextCarryCollisionToken;
        private bool gripWorldBlockDetectedThisStep;
        private Vector3 gripWorldBlockNormalThisStep;
        private float gripWorldBlockClearTimer;
        private uint nextThrowOriginSequence;
        private readonly RaycastHit[] carrySweepHits = new RaycastHit[CarryCollisionCapacity];
        private readonly Collider[] carryOverlapHits = new Collider[CarryCollisionCapacity];

        public PrototypeInteractionTarget CarriedTarget => carriedTarget;

        public PlayerThrowOrigin LastThrowOrigin { get; private set; }

        public uint ThrowOriginCount => nextThrowOriginSequence;

        public float AcquisitionPathLength { get; private set; }

        public float LastReleaseSpeed { get; private set; }

        public float LastThrowCharge01 { get; private set; }

        public float LastThrowSpeedMultiplier { get; private set; } = 1f;

        public float LastReleaseKineticEnergy { get; private set; }

        public float MaximumThrowSpeedMultiplier => maximumThrowSpeedMultiplier;

        public float GripSeparation { get; private set; }

        public bool GripWorldBlocked { get; private set; }

        public Vector3 GripWorldBlockNormal { get; private set; }

        public bool GripSelfOverlap { get; private set; }

        public bool GripPoseReached =>
            !float.IsInfinity(GripSeparation) &&
            GripSeparation <= maximumGripSeparation;

        // Compatibility mirror.  It now means a confirmed world obstruction,
        // never animation lag or player/carried-object self overlap.
        public bool IsGripBlocked => GripWorldBlocked;

        private void OnEnable()
        {
            EnsureCollisionLeaseIgnored(activeCarryCollisionLease);
            foreach (CarryCollisionLease pending in pendingCollisionRestores)
            {
                EnsureCollisionLeaseIgnored(pending);
            }
        }

        public void Configure(
            PrototypeInteractionStateOwner owner,
            Transform authoredCarryAnchor,
            float authoredMaximumThrowSpeedMultiplier = 1.6f)
        {
            interactionOwner = owner;
            carryAnchor = authoredCarryAnchor;
            maximumThrowSpeedMultiplier = Mathf.Clamp(
                authoredMaximumThrowSpeedMultiplier,
                1f,
                3f);
            playerController = GetComponent<CharacterController>();
        }

        public void ConfigureLightBinderGrip(
            ShowcaseBinderGripSocket gripSocket,
            float authoredMaximumGripSeparation = 0.18f)
        {
            lightBinderGripSocket = gripSocket;
            requireBoneTrackedLightGrip = gripSocket != null;
            maximumGripSeparation = Mathf.Max(0.02f, authoredMaximumGripSeparation);
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

            if (target.WeightClass == PrototypeWeightClass.Light &&
                requireBoneTrackedLightGrip &&
                (lightBinderGripSocket == null ||
                 !lightBinderGripSocket.TryGetWorldPose(out _, out _)))
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
            activeCarryCollisionLease = AcquireCarryCollisionLease(target, body);
            EnsureCollisionLeaseIgnored(activeCarryCollisionLease);
            SetCarriedCameraCollisionIgnored(true);
            ResetGripDiagnostics();
            if (target.WeightClass == PrototypeWeightClass.Light &&
                requireBoneTrackedLightGrip)
            {
                FollowLightGrip(true);
            }
            return true;
        }

        public bool TryThrow(Vector3 requestedDirection, float charge01 = 0f)
        {
            Vector3 direction = requestedDirection.sqrMagnitude > 0.0001f
                ? requestedDirection.normalized
                : transform.forward;
            return TryThrowResolved(direction, charge01);
        }

        public bool TryThrowAt(Vector3 frozenAimPoint, float charge01 = 0f)
        {
            if (carriedBody == null)
            {
                return false;
            }

            return TryThrowResolved(
                ResolveAimDirection(carriedBody.worldCenterOfMass, frozenAimPoint),
                charge01);
        }

        private bool TryThrowResolved(Vector3 requestedDirection, float charge01)
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

            SetCarriedCameraCollisionIgnored(false);
            ApplyPhysicalThrow(target, body, requestedDirection, charge01);
            QueueActiveCollisionLeaseForRestore(body);

            carriedTarget = null;
            carriedBody = null;
            carriedCollider = null;
            carriedBoxCollider = null;
            ResetGripDiagnostics();
            interactionOwner.CompleteThrow(target);
            return true;
        }

        public bool TryThrowCommittedRelayTarget(
            PrototypeInteractionTarget target,
            Vector3 requestedDirection,
            float charge01 = 0f)
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

            ApplyPhysicalThrow(target, body, requestedDirection, charge01);
            return interactionOwner.CompleteThrow(target);
        }

        public bool TryThrowCommittedRelayTargetAt(
            PrototypeInteractionTarget target,
            Vector3 frozenAimPoint,
            float charge01 = 0f)
        {
            Rigidbody body = target != null ? target.GetComponent<Rigidbody>() : null;
            if (body == null)
            {
                return false;
            }
            return TryThrowCommittedRelayTarget(
                target,
                ResolveAimDirection(body.worldCenterOfMass, frozenAimPoint),
                charge01);
        }

        public float GetThrowSpeed(PrototypeWeightClass weightClass)
        {
            return weightClass == PrototypeWeightClass.Light
                ? lightThrowSpeed
                : mediumThrowSpeed;
        }

        public float GetThrowSpeed(PrototypeWeightClass weightClass, float charge01)
        {
            return GetThrowSpeed(weightClass) * GetThrowSpeedMultiplier(charge01);
        }

        public float GetThrowSpeedMultiplier(float charge01)
        {
            return Mathf.Lerp(
                1f,
                maximumThrowSpeedMultiplier,
                Mathf.Clamp01(charge01));
        }

        private void ApplyPhysicalThrow(
            PrototypeInteractionTarget target,
            Rigidbody body,
            Vector3 requestedDirection,
            float charge01)
        {
            Vector3 direction = requestedDirection.sqrMagnitude > 0.0001f
                ? requestedDirection.normalized
                : transform.forward;
            float clampedCharge = Mathf.Clamp01(charge01);
            float speedMultiplier = GetThrowSpeedMultiplier(clampedCharge);
            float throwSpeed = GetThrowSpeed(target.WeightClass) * speedMultiplier;
            Vector3 releaseVelocity = direction * throwSpeed;

            body.isKinematic = false;
            body.useGravity = true;
            body.linearVelocity = releaseVelocity;
            Vector3 lateralAxis = Vector3.Cross(Vector3.up, direction).normalized;
            if (lateralAxis.sqrMagnitude <= 0.0001f)
            {
                lateralAxis = transform.right;
            }
            body.angularVelocity = lateralAxis *
                (target.WeightClass == PrototypeWeightClass.Light ? 4f : 0.5f);
            body.WakeUp();

            nextThrowOriginSequence++;
            LastThrowOrigin = new PlayerThrowOrigin(
                nextThrowOriginSequence,
                target.ContentDefinitionId,
                target.RuntimeInstanceKey,
                target.WeightClass);
            LastReleaseSpeed = releaseVelocity.magnitude;
            LastThrowCharge01 = clampedCharge;
            LastThrowSpeedMultiplier = speedMultiplier;
            LastReleaseKineticEnergy = 0.5f * body.mass * releaseVelocity.sqrMagnitude;
        }

        public void DetachForReset()
        {
            SetCarriedCameraCollisionIgnored(false);
            QueueActiveCollisionLeaseForRestore(carriedBody);
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
            LastThrowCharge01 = 0f;
            LastThrowSpeedMultiplier = 1f;
            LastReleaseKineticEnergy = 0f;
            ResetGripDiagnostics();
        }

        public void ClearThrowOriginForReset()
        {
            LastThrowOrigin = default;
            nextThrowOriginSequence = 0;
        }

        private void FixedUpdate()
        {
            TryRestorePendingReleasedPlayerCollisions();
            if (carriedTarget == null || carriedBody == null)
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

            EnsureCollisionLeaseIgnored(activeCarryCollisionLease);

            if (carriedTarget.WeightClass == PrototypeWeightClass.Light &&
                requireBoneTrackedLightGrip)
            {
                return;
            }
            if (carryAnchor == null)
            {
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

            BeginGripWorldProbe();
            Quaternion currentRotation = carriedBody.rotation;
            nextPosition = ResolveCarryTranslation(before, nextPosition, currentRotation);
            if (WouldCarryPosePenetrate(nextPosition, currentRotation))
            {
                nextPosition = ResolveCarryPenetration(before, nextPosition, currentRotation);
            }

            nextRotation = ResolveCarryRotation(nextPosition, currentRotation, nextRotation);

            carriedBody.MovePosition(nextPosition);
            carriedBody.MoveRotation(nextRotation);
            AcquisitionPathLength += Vector3.Distance(before, nextPosition);
            UpdateGripDiagnostics(destination, nextPosition, Time.fixedDeltaTime);
        }

        private Vector3 ResolveAimDirection(Vector3 releasePosition, Vector3 aimPoint)
        {
            Vector3 direction = aimPoint - releasePosition;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = transform.forward;
            }
            direction.Normalize();

            float pitch = Mathf.Asin(Mathf.Clamp(direction.y, -1f, 1f)) *
                Mathf.Rad2Deg;
            float clampedPitch = Mathf.Clamp(pitch, -12f, 22f);
            Vector3 planar = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planar.sqrMagnitude <= 0.0001f)
            {
                planar = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            }
            planar.Normalize();
            float radians = clampedPitch * Mathf.Deg2Rad;
            return (planar * Mathf.Cos(radians) + Vector3.up * Mathf.Sin(radians)).normalized;
        }

        private void TryRestorePendingReleasedPlayerCollisions()
        {
            for (int index = pendingCollisionRestores.Count - 1; index >= 0; index--)
            {
                CarryCollisionLease lease = pendingCollisionRestores[index];
                if (lease == null)
                {
                    pendingCollisionRestores.RemoveAt(index);
                    continue;
                }

                EnsureCollisionLeaseIgnored(lease);
                bool movingTowardPlayer = IsReleasedBodyMovingTowardPlayer(lease);
                if (IsCollisionLeaseClear(lease) && !movingTowardPlayer)
                {
                    lease.ClearFixedSteps++;
                }
                else
                {
                    lease.ClearFixedSteps = 0;
                }

                if (lease.ClearFixedSteps < CollisionRestoreClearFixedSteps)
                {
                    continue;
                }

                RestoreCollisionLease(lease);
                pendingCollisionRestores.RemoveAt(index);
            }
        }

        private void LateUpdate()
        {
            if (carriedTarget == null || carriedBody == null ||
                carriedTarget.WeightClass != PrototypeWeightClass.Light ||
                !requireBoneTrackedLightGrip)
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

            EnsureCollisionLeaseIgnored(activeCarryCollisionLease);
            FollowLightGrip(false);
        }

        private void FollowLightGrip(bool snapAtContact)
        {
            BeginGripWorldProbe();
            if (lightBinderGripSocket == null || carriedBody == null ||
                !lightBinderGripSocket.TryGetWorldPose(
                    out Vector3 destination,
                    out Quaternion requestedRotation))
            {
                GripSeparation = float.PositiveInfinity;
                GripWorldBlocked = false;
                GripWorldBlockNormal = Vector3.zero;
                gripWorldBlockClearTimer = 0f;
                GripSelfOverlap = ComputeActiveCarrySelfOverlap();
                return;
            }

            Vector3 before = carriedBody.position;
            Quaternion currentRotation = carriedBody.rotation;
            Vector3 nextPosition = ResolveCarryTranslation(
                before,
                destination,
                currentRotation);
            if (WouldCarryPosePenetrate(nextPosition, currentRotation))
            {
                nextPosition = ResolveCarryPenetration(
                    before,
                    nextPosition,
                    currentRotation);
            }

            Quaternion nextRotation = ResolveCarryRotation(
                nextPosition,
                currentRotation,
                requestedRotation);
            carriedBody.position = nextPosition;
            carriedBody.rotation = nextRotation;
            AcquisitionPathLength += Vector3.Distance(before, nextPosition);
            UpdateGripDiagnostics(destination, nextPosition, Time.deltaTime);

            if (snapAtContact && GripSeparation <= CarryCollisionSkin)
            {
                GripSeparation = 0f;
            }
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
                    RecordGripWorldBlock(hit.normal);
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
                        out Vector3 separationDirection,
                        out float distance) &&
                    distance > CarryPenetrationTolerance)
                {
                    RecordGripWorldBlock(separationDirection);
                    return true;
                }
            }

            return false;
        }

        private bool IsCarryBlockingCollider(Collider other)
        {
            if (other == null ||
                IsInHierarchy(other.transform, transform) ||
                (carriedTarget != null &&
                 IsInHierarchy(other.transform, carriedTarget.transform)) ||
                other.attachedRigidbody == carriedBody ||
                other.GetComponentInParent<ShowcaseRunnerAgent>() != null ||
                other.GetComponentInParent<ShowcaseMobileAllyController>() != null)
            {
                return false;
            }

            if (carriedCollider == null)
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

        private CarryCollisionLease AcquireCarryCollisionLease(
            PrototypeInteractionTarget target,
            Rigidbody body)
        {
            for (int index = pendingCollisionRestores.Count - 1; index >= 0; index--)
            {
                CarryCollisionLease pending = pendingCollisionRestores[index];
                if (pending == null || pending.Target != target)
                {
                    continue;
                }

                pendingCollisionRestores.RemoveAt(index);
                pending.Body = body;
                pending.ClearFixedSteps = 0;
                pending.HasLastDistance = false;
                return pending;
            }

            if (playerController == null)
            {
                playerController = GetComponent<CharacterController>();
            }

            Collider[] playerColliders = GetComponentsInChildren<Collider>(true);
            Collider[] targetColliders = target.GetComponentsInChildren<Collider>(true);
            List<CollisionPair> pairs = new();
            foreach (Collider playerCollider in playerColliders)
            {
                if (playerCollider == null)
                {
                    continue;
                }
                foreach (Collider targetCollider in targetColliders)
                {
                    if (targetCollider == null || playerCollider == targetCollider)
                    {
                        continue;
                    }
                    pairs.Add(new CollisionPair(playerCollider, targetCollider));
                }
            }

            nextCarryCollisionToken++;
            return new CarryCollisionLease(
                nextCarryCollisionToken,
                target,
                body,
                playerColliders,
                targetColliders,
                pairs);
        }

        private static void EnsureCollisionLeaseIgnored(CarryCollisionLease lease)
        {
            if (lease == null)
            {
                return;
            }

            foreach (CollisionPair pair in lease.Pairs)
            {
                if (!pair.IsValid || Physics.GetIgnoreCollision(pair.Player, pair.Target))
                {
                    continue;
                }
                Physics.IgnoreCollision(pair.Player, pair.Target, true);
            }
        }

        private void QueueActiveCollisionLeaseForRestore(Rigidbody releasedBody)
        {
            if (activeCarryCollisionLease == null)
            {
                return;
            }

            activeCarryCollisionLease.Body = releasedBody;
            activeCarryCollisionLease.ClearFixedSteps = 0;
            activeCarryCollisionLease.HasLastDistance = false;
            EnsureCollisionLeaseIgnored(activeCarryCollisionLease);
            pendingCollisionRestores.Add(activeCarryCollisionLease);
            activeCarryCollisionLease = null;
        }

        private static void RestoreCollisionLease(CarryCollisionLease lease)
        {
            if (lease == null)
            {
                return;
            }
            foreach (CollisionPair pair in lease.Pairs)
            {
                if (pair.IsValid)
                {
                    Physics.IgnoreCollision(pair.Player, pair.Target, false);
                }
            }
        }

        private bool IsCollisionLeaseClear(CarryCollisionLease lease)
        {
            foreach (CollisionPair pair in lease.Pairs)
            {
                if (!pair.IsValid || !pair.Player.enabled || !pair.Target.enabled ||
                    !pair.Player.gameObject.activeInHierarchy ||
                    !pair.Target.gameObject.activeInHierarchy)
                {
                    continue;
                }
                if (Physics.ComputePenetration(
                        pair.Player,
                        pair.Player.transform.position,
                        pair.Player.transform.rotation,
                        pair.Target,
                        pair.Target.transform.position,
                        pair.Target.transform.rotation,
                        out _,
                        out float distance) &&
                    distance > CarryPenetrationTolerance)
                {
                    return false;
                }
            }

            if (playerController == null || !playerController.enabled)
            {
                return true;
            }

            GetCharacterControllerCapsule(
                playerController,
                out Vector3 bottom,
                out Vector3 top,
                out float radius);
            float clearance = Mathf.Max(playerController.skinWidth, CarryCollisionSkin) + 0.01f;
            int overlapCount = Physics.OverlapCapsuleNonAlloc(
                bottom,
                top,
                radius + clearance,
                carryOverlapHits,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore);
            for (int index = 0; index < overlapCount; index++)
            {
                if (lease.ContainsTargetCollider(carryOverlapHits[index]))
                {
                    return false;
                }
            }
            return overlapCount < CarryCollisionCapacity;
        }

        private bool IsReleasedBodyMovingTowardPlayer(CarryCollisionLease lease)
        {
            if (lease.Body == null || playerController == null)
            {
                return false;
            }

            Vector3 bodyPosition = lease.Body.worldCenterOfMass;
            Vector3 playerPosition = playerController.bounds.center;
            bodyPosition.y = 0f;
            playerPosition.y = 0f;
            float distance = Vector3.Distance(bodyPosition, playerPosition);
            bool movingToward = lease.HasLastDistance &&
                distance < lease.LastPlayerDistance - CarryPenetrationTolerance;
            lease.LastPlayerDistance = distance;
            lease.HasLastDistance = true;
            return movingToward;
        }

        private void BeginGripWorldProbe()
        {
            gripWorldBlockDetectedThisStep = false;
            gripWorldBlockNormalThisStep = Vector3.zero;
        }

        private void RecordGripWorldBlock(Vector3 normal)
        {
            Vector3 normalized = normal.sqrMagnitude > 0.0001f
                ? normal.normalized
                : Vector3.zero;
            if (!gripWorldBlockDetectedThisStep ||
                Mathf.Abs(normalized.y) < Mathf.Abs(gripWorldBlockNormalThisStep.y))
            {
                gripWorldBlockNormalThisStep = normalized;
            }
            gripWorldBlockDetectedThisStep = true;
        }

        private void UpdateGripDiagnostics(
            Vector3 desiredGripPosition,
            Vector3 actualPosition,
            float deltaTime)
        {
            GripSeparation = Vector3.Distance(actualPosition, desiredGripPosition);
            GripSelfOverlap = ComputeActiveCarrySelfOverlap();
            bool confirmedWorldBlock = gripWorldBlockDetectedThisStep &&
                GripSeparation > maximumGripSeparation;
            if (confirmedWorldBlock)
            {
                GripWorldBlocked = true;
                GripWorldBlockNormal = gripWorldBlockNormalThisStep;
                gripWorldBlockClearTimer = 0f;
                return;
            }

            if (!GripWorldBlocked)
            {
                GripWorldBlockNormal = Vector3.zero;
                gripWorldBlockClearTimer = 0f;
                return;
            }

            bool pathClearAndPoseReached = !gripWorldBlockDetectedThisStep &&
                GripSeparation <= maximumGripSeparation;
            gripWorldBlockClearTimer = pathClearAndPoseReached
                ? gripWorldBlockClearTimer + Mathf.Max(0f, deltaTime)
                : 0f;
            if (gripWorldBlockClearTimer >= GripWorldBlockReleaseDelay)
            {
                GripWorldBlocked = false;
                GripWorldBlockNormal = Vector3.zero;
                gripWorldBlockClearTimer = 0f;
            }
        }

        private bool ComputeActiveCarrySelfOverlap()
        {
            CarryCollisionLease lease = activeCarryCollisionLease;
            if (lease == null)
            {
                return false;
            }
            foreach (CollisionPair pair in lease.Pairs)
            {
                if (!pair.IsValid || !pair.Player.enabled || !pair.Target.enabled)
                {
                    continue;
                }
                if (Physics.ComputePenetration(
                        pair.Player,
                        pair.Player.transform.position,
                        pair.Player.transform.rotation,
                        pair.Target,
                        pair.Target.transform.position,
                        pair.Target.transform.rotation,
                        out _,
                        out float distance) &&
                    distance > CarryPenetrationTolerance)
                {
                    return true;
                }
            }
            return false;
        }

        private void ResetGripDiagnostics()
        {
            GripSeparation = 0f;
            GripWorldBlocked = false;
            GripWorldBlockNormal = Vector3.zero;
            GripSelfOverlap = false;
            gripWorldBlockDetectedThisStep = false;
            gripWorldBlockNormalThisStep = Vector3.zero;
            gripWorldBlockClearTimer = 0f;
        }

        private static bool IsInHierarchy(Transform candidate, Transform root)
        {
            return candidate != null && root != null &&
                (candidate == root || candidate.IsChildOf(root));
        }

        private static void GetCharacterControllerCapsule(
            CharacterController controller,
            out Vector3 bottom,
            out Vector3 top,
            out float radius)
        {
            Vector3 scale = controller.transform.lossyScale;
            float verticalScale = Mathf.Abs(scale.y);
            float radialScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            radius = controller.radius * radialScale;
            float height = Mathf.Max(radius * 2f, controller.height * verticalScale);
            Vector3 center = controller.transform.TransformPoint(controller.center);
            float halfSegment = Mathf.Max(0f, height * 0.5f - radius);
            bottom = center - Vector3.up * halfSegment;
            top = center + Vector3.up * halfSegment;
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

        private readonly struct CollisionPair
        {
            public CollisionPair(Collider player, Collider target)
            {
                Player = player;
                Target = target;
            }

            public Collider Player { get; }
            public Collider Target { get; }
            public bool IsValid => Player != null && Target != null;
        }

        private sealed class CarryCollisionLease
        {
            public CarryCollisionLease(
                uint token,
                PrototypeInteractionTarget target,
                Rigidbody body,
                Collider[] playerColliders,
                Collider[] targetColliders,
                List<CollisionPair> pairs)
            {
                Token = token;
                Target = target;
                Body = body;
                PlayerColliders = playerColliders;
                TargetColliders = targetColliders;
                Pairs = pairs;
            }

            public uint Token { get; }
            public PrototypeInteractionTarget Target { get; }
            public Rigidbody Body { get; set; }
            public Collider[] PlayerColliders { get; }
            public Collider[] TargetColliders { get; }
            public List<CollisionPair> Pairs { get; }
            public int ClearFixedSteps { get; set; }
            public float LastPlayerDistance { get; set; }
            public bool HasLastDistance { get; set; }

            public bool ContainsTargetCollider(Collider value)
            {
                if (value == null)
                {
                    return false;
                }
                foreach (Collider targetCollider in TargetColliders)
                {
                    if (targetCollider == value)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
