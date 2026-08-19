using System;
using BreakNotice.Gameplay.Input;
using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Gameplay
{
    [Flags]
    public enum PlayerMobilityLockReason
    {
        None = 0,
        PlayerAction = 1 << 0,
        HitReaction = 1 << 1,
        Knockdown = 1 << 2,
        Grab = 1 << 3,
        Objective = 1 << 4,
        Death = 1 << 5,
        MeleeAction = 1 << 6
    }

    [DefaultExecutionOrder(110)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(InputSystemLogicalInputAdapter))]
    public sealed class ThirdPersonLocomotion : MonoBehaviour
    {
        [SerializeField]
        private InputSystemLogicalInputAdapter logicalInput;

        [SerializeField]
        private Transform cameraTransform;

        [SerializeField]
        private float moveSpeed = 3.2f;

        [SerializeField]
        private float sprintSpeed = 5.5f;

        [SerializeField]
        private float rotationResponse = 12f;

        [SerializeField]
        private float aimTurnSpeedDegrees = 720f;

        [SerializeField]
        private float gravity = -25f;

        [SerializeField]
        private float jumpHeight = 1.15f;

        [SerializeField]
        private float coyoteTime = 0.1f;

        [SerializeField]
        private float jumpBufferTime = 0.1f;

        private CharacterController controller;
        private PrototypeInteractionStateOwner interactionOwner;
        private PrototypeInteractionController interactionController;
        private PrototypePhysicalObjectLifecycle physicalLifecycle;
        private float verticalVelocity;
        private float coyoteTimeRemaining;
        private float jumpBufferTimeRemaining;
        private bool jumpConsumed;
        private CollisionFlags lastCollisionFlags;
        private PlayerMobilityLockReason mobilityLocks;
        private PlayerMobilityLockReason jumpLocks;
        private PlayerMobilityLockReason facingOverrideLocks;
        private Vector3 facingOverrideWorldDirection;
        private Vector3 externalPlanarVelocity;

        public float MoveSpeed => moveSpeed;

        public float SprintSpeed => sprintSpeed;

        public float RotationResponse => rotationResponse;

        public float Gravity => gravity;

        public float JumpHeight => jumpHeight;

        public float CoyoteTime => coyoteTime;

        public float JumpBufferTime => jumpBufferTime;

        public float VerticalVelocity => verticalVelocity;

        /// <summary>
        /// Advances exactly once when a buffered jump is accepted and its
        /// physical takeoff impulse is committed. Presentation observes this
        /// revision instead of inferring takeoff one frame later from grounding.
        /// </summary>
        public uint JumpTakeoffRevision { get; private set; }

        public bool IsGrounded => controller != null &&
            (controller.isGrounded ||
             (lastCollisionFlags & CollisionFlags.Below) != 0);

        public bool IsSprinting { get; private set; }

        public float CurrentHorizontalSpeed { get; private set; }

        public PlayerMobilityLockReason MobilityLocks => mobilityLocks;

        public PlayerMobilityLockReason JumpLocks => jumpLocks;

        public bool IsMovementExternallyBlocked =>
            mobilityLocks != PlayerMobilityLockReason.None;

        public bool IsJumpExternallyBlocked =>
            jumpLocks != PlayerMobilityLockReason.None;

        public bool HasFacingOverride =>
            facingOverrideLocks != PlayerMobilityLockReason.None &&
            facingOverrideWorldDirection.sqrMagnitude > 0.0001f;

        public Vector3 AimFacingWorldDirection { get; private set; }

        public Transform MovementReference => cameraTransform;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (logicalInput == null)
            {
                logicalInput = GetComponent<InputSystemLogicalInputAdapter>();
            }

            interactionOwner = GetComponent<PrototypeInteractionStateOwner>();
            interactionController = GetComponent<PrototypeInteractionController>();
            physicalLifecycle = GetComponent<PrototypePhysicalObjectLifecycle>();
        }

        private void Update()
        {
            Vector2 moveInput = logicalInput.Snapshot.Move;
            bool mobilityBlocked = logicalInput.Snapshot.ResetEncounter ||
                IsMovementExternallyBlocked ||
                (interactionOwner != null && interactionOwner.BlocksNewInteraction);
            bool jumpBlocked = mobilityBlocked || IsJumpExternallyBlocked;
            bool mediumCarry = IsCarryingMedium();
            bool hasMoveInput = moveInput.sqrMagnitude > 0.0001f;
            IsSprinting = !mobilityBlocked &&
                !mediumCarry &&
                !logicalInput.Snapshot.Aim &&
                logicalInput.Snapshot.Sprint &&
                hasMoveInput;
            CurrentHorizontalSpeed = IsSprinting ? sprintSpeed : moveSpeed;

            if (logicalInput.Snapshot.Jump && !jumpBlocked && !mediumCarry)
            {
                jumpBufferTimeRemaining = jumpBufferTime;
            }
            else
            {
                jumpBufferTimeRemaining = Mathf.Max(
                    0f,
                    jumpBufferTimeRemaining - Time.deltaTime);
            }

            if (jumpBlocked || mediumCarry)
            {
                jumpBufferTimeRemaining = 0f;
            }
            if (mobilityBlocked || mediumCarry)
            {
                IsSprinting = false;
                CurrentHorizontalSpeed = moveSpeed;
            }

            Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;

            forward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
            right = Vector3.ProjectOnPlane(right, Vector3.up).normalized;

            Vector3 desiredDirection = forward * moveInput.y + right * moveInput.x;
            if (desiredDirection.sqrMagnitude > 1f)
            {
                desiredDirection.Normalize();
            }
            if (physicalLifecycle != null && physicalLifecycle.GripWorldBlocked)
            {
                Vector3 blockNormal = Vector3.ProjectOnPlane(
                    physicalLifecycle.GripWorldBlockNormal,
                    Vector3.up);
                if (blockNormal.sqrMagnitude > 0.0001f)
                {
                    blockNormal.Normalize();
                    float inward = Vector3.Dot(desiredDirection, blockNormal);
                    if (inward < 0f)
                    {
                        desiredDirection -= inward * blockNormal;
                    }
                }
            }

            bool aiming = logicalInput.Snapshot.Aim && !mobilityBlocked;
            Vector3 aimFacingDirection = forward;
            if (aiming && interactionController != null)
            {
                Vector3 toReticle = Vector3.ProjectOnPlane(
                    interactionController.ResolveReticleAimPoint() - transform.position,
                    Vector3.up);
                if (toReticle.sqrMagnitude > 0.0001f)
                {
                    aimFacingDirection = toReticle.normalized;
                }
            }
            AimFacingWorldDirection = aiming ? aimFacingDirection : Vector3.zero;
            bool facingOverridden = !mobilityBlocked && HasFacingOverride;
            Vector3 facingDirection = facingOverridden
                ? facingOverrideWorldDirection
                : aiming
                    ? aimFacingDirection
                    : desiredDirection;
            if (!mobilityBlocked && facingDirection.sqrMagnitude > 0.0001f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(facingDirection, Vector3.up);
                transform.rotation = aiming || facingOverridden
                    ? Quaternion.RotateTowards(
                        transform.rotation,
                        desiredRotation,
                        Mathf.Max(0f, aimTurnSpeedDegrees) * Time.deltaTime)
                    : Quaternion.Slerp(
                        transform.rotation,
                        desiredRotation,
                        1f - Mathf.Exp(-rotationResponse * Time.deltaTime));
            }

            bool groundedBeforeMove = IsGrounded;
            if (groundedBeforeMove && verticalVelocity <= 0f)
            {
                verticalVelocity = -2f;
                coyoteTimeRemaining = coyoteTime;
                jumpConsumed = false;
            }
            else
            {
                coyoteTimeRemaining = Mathf.Max(0f, coyoteTimeRemaining - Time.deltaTime);
            }

            if (!jumpConsumed &&
                jumpBufferTimeRemaining > 0f &&
                coyoteTimeRemaining > 0f &&
                !jumpBlocked &&
                !mediumCarry)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                JumpTakeoffRevision++;
                jumpBufferTimeRemaining = 0f;
                coyoteTimeRemaining = 0f;
                jumpConsumed = true;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            Vector3 controlledPlanarVelocity = mobilityBlocked
                ? Vector3.zero
                : desiredDirection * CurrentHorizontalSpeed;
            Vector3 velocity = controlledPlanarVelocity +
                externalPlanarVelocity +
                Vector3.up * verticalVelocity;
            CollisionFlags collisionFlags = controller.Move(velocity * Time.deltaTime);
            lastCollisionFlags = collisionFlags;

            if ((collisionFlags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
            {
                verticalVelocity = 0f;
            }

            if (IsGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
                coyoteTimeRemaining = coyoteTime;
                jumpConsumed = false;
            }

            externalPlanarVelocity = Vector3.MoveTowards(
                externalPlanarVelocity,
                Vector3.zero,
                14f * Time.deltaTime);
        }

        public void ConfigureCamera(Transform value)
        {
            cameraTransform = value;
        }

        public void ConfigureInteractionOwner(PrototypeInteractionStateOwner value)
        {
            interactionOwner = value;
        }

        public void SetMobilityLock(PlayerMobilityLockReason reason, bool locked)
        {
            if (reason == PlayerMobilityLockReason.None)
            {
                return;
            }

            if (locked)
            {
                mobilityLocks |= reason;
            }
            else
            {
                mobilityLocks &= ~reason;
            }

            if (IsMovementExternallyBlocked)
            {
                IsSprinting = false;
                jumpBufferTimeRemaining = 0f;
            }
        }

        public bool HasMobilityLock(PlayerMobilityLockReason reason)
        {
            return reason != PlayerMobilityLockReason.None &&
                (mobilityLocks & reason) == reason;
        }

        public void SetJumpLock(PlayerMobilityLockReason reason, bool locked)
        {
            if (reason == PlayerMobilityLockReason.None)
            {
                return;
            }

            if (locked)
            {
                jumpLocks |= reason;
                jumpBufferTimeRemaining = 0f;
            }
            else
            {
                jumpLocks &= ~reason;
            }
        }

        public bool HasJumpLock(PlayerMobilityLockReason reason)
        {
            return reason != PlayerMobilityLockReason.None &&
                (jumpLocks & reason) == reason;
        }

        public void SetFacingOverride(
            PlayerMobilityLockReason reason,
            Vector3 worldDirection,
            bool active)
        {
            if (reason == PlayerMobilityLockReason.None)
            {
                return;
            }

            if (active)
            {
                Vector3 planar = Vector3.ProjectOnPlane(worldDirection, Vector3.up);
                if (planar.sqrMagnitude <= 0.0001f)
                {
                    return;
                }
                facingOverrideLocks |= reason;
                facingOverrideWorldDirection = planar.normalized;
            }
            else
            {
                facingOverrideLocks &= ~reason;
                if (facingOverrideLocks == PlayerMobilityLockReason.None)
                {
                    facingOverrideWorldDirection = Vector3.zero;
                }
            }
        }

        public void AddExternalPlanarImpulse(Vector3 velocity)
        {
            externalPlanarVelocity += Vector3.ProjectOnPlane(velocity, Vector3.up);
        }

        public void ClearExternalPlanarImpulse()
        {
            externalPlanarVelocity = Vector3.zero;
        }

        public void ResetMotionState()
        {
            verticalVelocity = -2f;
            coyoteTimeRemaining = 0f;
            jumpBufferTimeRemaining = 0f;
            jumpConsumed = false;
            JumpTakeoffRevision = 0;
            IsSprinting = false;
            CurrentHorizontalSpeed = moveSpeed;
            mobilityLocks = PlayerMobilityLockReason.None;
            jumpLocks = PlayerMobilityLockReason.None;
            facingOverrideLocks = PlayerMobilityLockReason.None;
            facingOverrideWorldDirection = Vector3.zero;
            AimFacingWorldDirection = Vector3.zero;
            externalPlanarVelocity = Vector3.zero;
        }

        private bool IsCarryingMedium()
        {
            return interactionOwner != null &&
                interactionOwner.HandState == PrototypeHandOwnershipState.Carrying &&
                interactionOwner.CarriedTarget != null &&
                interactionOwner.CarriedTarget.WeightClass == PrototypeWeightClass.Medium;
        }
    }
}
