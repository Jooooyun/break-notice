using BreakNotice.Gameplay.Input;
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
        private float rotationResponse = 12f;

        [SerializeField]
        private float gravity = -25f;

        private CharacterController controller;
        private float verticalVelocity;

        public float MoveSpeed => moveSpeed;

        public float RotationResponse => rotationResponse;

        public float Gravity => gravity;

        public float VerticalVelocity => verticalVelocity;

        public bool IsGrounded => controller != null && controller.isGrounded;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (logicalInput == null)
            {
                logicalInput = GetComponent<InputSystemLogicalInputAdapter>();
            }
        }

        private void Update()
        {
            Vector2 moveInput = logicalInput.Snapshot.Move;
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

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            Vector3 velocity = desiredDirection * moveSpeed + Vector3.up * verticalVelocity;
            controller.Move(velocity * Time.deltaTime);

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }
        }

        public void ConfigureCamera(Transform value)
        {
            cameraTransform = value;
        }

        public void ResetMotionState()
        {
            verticalVelocity = -2f;
        }
    }
}
