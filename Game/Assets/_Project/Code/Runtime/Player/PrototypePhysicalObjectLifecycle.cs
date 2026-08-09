using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    [DefaultExecutionOrder(-90)]
    [DisallowMultipleComponent]
    public sealed class PrototypePhysicalObjectLifecycle : MonoBehaviour
    {
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
        private bool playerCollisionIgnored;
        private int carriedOriginalLayer;
        private bool carriedCameraCollisionIgnored;
        private uint nextThrowOriginSequence;

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
                !target.IsScoopEligible)
            {
                return false;
            }

            Rigidbody body = target.GetComponent<Rigidbody>();
            if (body == null)
            {
                return false;
            }

            carriedTarget = target;
            carriedBody = body;
            carriedCollider = target.GetComponent<Collider>();
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

            Vector3 direction = requestedDirection.sqrMagnitude > 0.0001f
                ? requestedDirection.normalized
                : transform.forward;
            float throwSpeed = GetThrowSpeed(target.WeightClass);
            Vector3 releaseVelocity = direction * throwSpeed + Vector3.up * upwardThrowSpeed;

            SetPlayerCollisionIgnored(false);
            SetCarriedCameraCollisionIgnored(false);
            body.isKinematic = false;
            body.useGravity = true;
            body.linearVelocity = releaseVelocity;
            body.angularVelocity = Vector3.up *
                (target.WeightClass == PrototypeWeightClass.Light ? 4f : 2f);

            nextThrowOriginSequence++;
            LastThrowOrigin = new PlayerThrowOrigin(
                nextThrowOriginSequence,
                target.ContentDefinitionId,
                target.RuntimeInstanceKey,
                target.WeightClass);
            LastReleaseSpeed = releaseVelocity.magnitude;

            carriedTarget = null;
            carriedBody = null;
            carriedCollider = null;
            interactionOwner.CompleteThrow(target);
            return true;
        }

        public float GetThrowSpeed(PrototypeWeightClass weightClass)
        {
            return weightClass == PrototypeWeightClass.Light
                ? lightThrowSpeed
                : mediumThrowSpeed;
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

            carriedBody.MovePosition(nextPosition);
            carriedBody.MoveRotation(nextRotation);
            AcquisitionPathLength += Vector3.Distance(before, nextPosition);
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
