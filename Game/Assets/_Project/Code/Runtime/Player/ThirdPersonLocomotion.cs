using BreakNotice.Gameplay.Input;
using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Gameplay
{
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
        private float moveSpeed = 4.5f;

        [SerializeField]
        private float sprintSpeed = 6.5f;

        [SerializeField]
        private float rotationResponse = 12f;

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
        private float verticalVelocity;
        private float coyoteTimeRemaining;
        private float jumpBufferTimeRemaining;
        private bool jumpConsumed;
        private CollisionFlags lastCollisionFlags;
        private bool externalMovementBlocked;
        private Vector3 externalPlanarVelocity;

        public float MoveSpeed => moveSpeed;

        public float SprintSpeed => sprintSpeed;

        public float RotationResponse => rotationResponse;

        public float Gravity => gravity;

        public float JumpHeight => jumpHeight;

        public float CoyoteTime => coyoteTime;

        public float JumpBufferTime => jumpBufferTime;

        public float VerticalVelocity => verticalVelocity;

        public bool IsGrounded => controller != null &&
            (controller.isGrounded ||
             (lastCollisionFlags & CollisionFlags.Below) != 0);

        public bool IsSprinting { get; private set; }

        public float CurrentHorizontalSpeed { get; private set; }

        public bool IsMovementExternallyBlocked => externalMovementBlocked;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (logicalInput == null)
            {
                logicalInput = GetComponent<InputSystemLogicalInputAdapter>();
            }

            interactionOwner = GetComponent<PrototypeInteractionStateOwner>();
        }

        private void Update()
        {
            Vector2 moveInput = logicalInput.Snapshot.Move;
            bool mobilityBlocked = logicalInput.Snapshot.ResetEncounter ||
                externalMovementBlocked ||
                (interactionOwner != null && interactionOwner.BlocksNewInteraction);
            bool mediumCarry = IsCarryingMedium();
            bool hasMoveInput = moveInput.sqrMagnitude > 0.0001f;
            IsSprinting = !mobilityBlocked &&
                !mediumCarry &&
                !logicalInput.Snapshot.Aim &&
                logicalInput.Snapshot.Sprint &&
                hasMoveInput;
            CurrentHorizontalSpeed = IsSprinting ? sprintSpeed : moveSpeed;

            if (logicalInput.Snapshot.Jump && !mobilityBlocked && !mediumCarry)
            {
                jumpBufferTimeRemaining = jumpBufferTime;
            }
            else
            {
                jumpBufferTimeRemaining = Mathf.Max(
                    0f,
                    jumpBufferTimeRemaining - Time.deltaTime);
            }

            if (mobilityBlocked || mediumCarry)
            {
                jumpBufferTimeRemaining = 0f;
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

            if (desiredDirection.sqrMagnitude > 0.0001f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(
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
                !mobilityBlocked &&
                !mediumCarry)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpBufferTimeRemaining = 0f;
                coyoteTimeRemaining = 0f;
                jumpConsumed = true;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            Vector3 velocity = desiredDirection * CurrentHorizontalSpeed +
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

        public void SetExternalMovementBlocked(bool blocked)
        {
            externalMovementBlocked = blocked;
            if (blocked)
            {
                IsSprinting = false;
                jumpBufferTimeRemaining = 0f;
            }
        }

        public void AddExternalPlanarImpulse(Vector3 velocity)
        {
            externalPlanarVelocity += Vector3.ProjectOnPlane(velocity, Vector3.up);
        }

        public void ResetMotionState()
        {
            verticalVelocity = -2f;
            coyoteTimeRemaining = 0f;
            jumpBufferTimeRemaining = 0f;
            jumpConsumed = false;
            IsSprinting = false;
            CurrentHorizontalSpeed = moveSpeed;
            externalMovementBlocked = false;
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
