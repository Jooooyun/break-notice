using BreakNotice.Gameplay.Input;
using Unity.Cinemachine;
using UnityEngine;

namespace BreakNotice.Presentation
{
    [DisallowMultipleComponent]
    public sealed class ThirdPersonCameraRig : MonoBehaviour
    {
        [SerializeField]
        private Transform playerTarget;

        [SerializeField]
        private InputSystemLogicalInputAdapter logicalInput;

        [SerializeField]
        private Transform orbitTarget;

        [SerializeField]
        private Camera outputCamera;

        [SerializeField]
        private CinemachineCamera virtualCamera;

        [SerializeField]
        private CinemachineThirdPersonFollow thirdPersonFollow;

        [SerializeField]
        private float targetHeight = 1.45f;

        [SerializeField]
        private float lookSensitivity = 1.5f;

        [SerializeField]
        private float minimumPitch = -30f;

        [SerializeField]
        private float maximumPitch = 60f;

        [SerializeField]
        private float normalDistance = 4.5f;

        [SerializeField]
        private float aimDistance = 3.1f;

        [SerializeField]
        private float normalShoulderOffset = 0.55f;

        [SerializeField]
        private float aimShoulderOffset = 0.78f;

        [SerializeField]
        private float shoulderHeight = 0.15f;

        [SerializeField]
        private float normalFieldOfView = 68f;

        [SerializeField]
        private float aimFieldOfView = 55f;

        [SerializeField, Min(0.01f)]
        private float aimTransitionDuration = 0.2f;

        [SerializeField]
        private float cameraCollisionRadius = 0.25f;

        private float yaw;
        private float pitch = 12f;
        private float aimBlend;

        public Camera OutputCamera => outputCamera;

        public float Yaw => yaw;

        public float Pitch => pitch;

        public bool IsAiming { get; private set; }

        public float AimBlend => aimBlend;

        public float CurrentDistance => thirdPersonFollow != null
            ? thirdPersonFollow.CameraDistance
            : 0f;

        private void Awake()
        {
            ApplyPipelineConfiguration();
        }

        private void OnEnable()
        {
            ApplyPipelineConfiguration();
        }

        private void LateUpdate()
        {
            if (playerTarget == null || orbitTarget == null)
            {
                return;
            }

            Vector2 look = logicalInput != null ? logicalInput.Snapshot.Look : Vector2.zero;
            yaw += look.x * lookSensitivity;
            pitch = Mathf.Clamp(pitch - look.y * lookSensitivity, minimumPitch, maximumPitch);

            orbitTarget.position = playerTarget.position + Vector3.up * targetHeight;
            orbitTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);

            IsAiming = logicalInput != null && logicalInput.Snapshot.Aim;
            float transitionDuration = Mathf.Max(aimTransitionDuration, 0.01f);
            float targetAimBlend = IsAiming ? 1f : 0f;
            aimBlend = Mathf.MoveTowards(
                aimBlend,
                targetAimBlend,
                Time.unscaledDeltaTime / transitionDuration);
            ApplyPresentation(aimBlend);
        }

        public void Configure(Transform target, InputSystemLogicalInputAdapter input)
        {
            playerTarget = target;
            logicalInput = input;
            if (playerTarget != null)
            {
                yaw = playerTarget.eulerAngles.y;
            }

            ApplyPipelineConfiguration();
        }

        private void ApplyPipelineConfiguration()
        {
            if (virtualCamera != null && orbitTarget != null)
            {
                virtualCamera.Follow = orbitTarget;
            }

            if (thirdPersonFollow != null)
            {
                CinemachineThirdPersonFollow.ObstacleSettings obstacleSettings =
                    thirdPersonFollow.AvoidObstacles;
                obstacleSettings.Enabled = true;
                obstacleSettings.CollisionFilter = ~0;
                obstacleSettings.IgnoreTag = string.Empty;
                obstacleSettings.CameraRadius = cameraCollisionRadius;
                obstacleSettings.DampingIntoCollision = 0f;
                obstacleSettings.DampingFromCollision = 0.5f;
                thirdPersonFollow.AvoidObstacles = obstacleSettings;
            }

            aimBlend = IsAiming ? 1f : 0f;
            ApplyPresentation(aimBlend);
        }

        private void ApplyPresentation(float blend)
        {
            float clampedBlend = Mathf.Clamp01(blend);

            if (thirdPersonFollow != null)
            {
                thirdPersonFollow.CameraDistance = Mathf.Lerp(
                    normalDistance,
                    aimDistance,
                    clampedBlend);
                thirdPersonFollow.ShoulderOffset = new Vector3(
                    Mathf.Lerp(normalShoulderOffset, aimShoulderOffset, clampedBlend),
                    shoulderHeight,
                    0f);
            }

            if (virtualCamera != null)
            {
                LensSettings lens = virtualCamera.Lens;
                lens.FieldOfView = Mathf.Lerp(
                    normalFieldOfView,
                    aimFieldOfView,
                    clampedBlend);
                virtualCamera.Lens = lens;
            }
        }
    }
}
