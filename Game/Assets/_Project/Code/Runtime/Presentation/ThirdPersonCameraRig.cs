using BreakNotice.Gameplay.Input;
using Unity.Cinemachine;
using UnityEngine;

namespace BreakNotice.Presentation
{
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public sealed class ThirdPersonCameraRig : MonoBehaviour
    {
        private const string CameraObstacleIgnoreTag = "Player";

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
        private float targetHeight = 0.95f;

        [SerializeField]
        private float aimTargetHeight = 1.1f;

        [SerializeField]
        private float lookSensitivity = 0.25f;

        [SerializeField, Range(0.1f, 1f)]
        private float aimLookSpeedMultiplier = 0.78f;

        [SerializeField]
        private float minimumPitch = -12f;

        [SerializeField]
        private float maximumPitch = 35f;

        [SerializeField]
        private float normalDistance = 2.5f;

        [SerializeField]
        private float aimDistance = 1.85f;

        [SerializeField]
        private float normalShoulderOffset = 0.9f;

        [SerializeField]
        private float aimShoulderOffset = 0.78f;

        [SerializeField]
        private float normalShoulderHeight;

        [SerializeField]
        private float aimShoulderHeight = 0.04f;

        [SerializeField]
        private float normalVerticalArmLength = 0.1f;

        [SerializeField]
        private float aimVerticalArmLength = 0.08f;

        [SerializeField]
        private float normalFieldOfView = 52f;

        [SerializeField]
        private float aimFieldOfView = 45f;

        [SerializeField]
        private Vector3 normalDamping = new Vector3(0.08f, 0.15f, 0.12f);

        [SerializeField]
        private Vector3 aimDamping = new Vector3(0.05f, 0.08f, 0.08f);

        [SerializeField, Min(0.01f)]
        private float aimTransitionDuration = 0.2f;

        [SerializeField]
        private float cameraCollisionRadius = 0.25f;

        private float yaw;
        private float pitch = 3f;
        private float aimBlend;
#if UNITY_WEBGL && !UNITY_EDITOR
        private bool pointerWasLocked;
#endif

        public Camera OutputCamera => outputCamera;

        public float Yaw => yaw;

        public float Pitch => pitch;

        public bool IsAiming { get; private set; }

        public float AimBlend => aimBlend;

        public float CurrentDistance => thirdPersonFollow != null
            ? thirdPersonFollow.CameraDistance
            : 0f;

        public Vector3 CurrentShoulderOffset => thirdPersonFollow != null
            ? thirdPersonFollow.ShoulderOffset
            : Vector3.zero;

        public float CurrentVerticalArmLength => thirdPersonFollow != null
            ? thirdPersonFollow.VerticalArmLength
            : 0f;

        public float CurrentFieldOfView => virtualCamera != null
            ? virtualCamera.Lens.FieldOfView
            : 0f;

        private void Awake()
        {
            EnsurePlayerCollisionIgnored();
            ApplyPipelineConfiguration();
        }

        private void OnEnable()
        {
            EnsurePlayerCollisionIgnored();
            ApplyPipelineConfiguration();
        }

        private void LateUpdate()
        {
            if (playerTarget == null || orbitTarget == null)
            {
                return;
            }

            Vector2 look = logicalInput != null ? logicalInput.Snapshot.Look : Vector2.zero;
#if UNITY_WEBGL && !UNITY_EDITOR
            bool pointerCaptured =
                logicalInput != null && logicalInput.IsWebPointerGameplayActive;
            bool pointerJustCaptured = pointerCaptured && !pointerWasLocked;
            pointerWasLocked = pointerCaptured;
            if (logicalInput != null && logicalInput.IsWebPointerFallbackActive)
            {
                look = logicalInput.WebPointerFallbackLookDelta;
            }

            if (!pointerCaptured || pointerJustCaptured)
            {
                look = Vector2.zero;
            }
#endif
            IsAiming = logicalInput != null && logicalInput.Snapshot.Aim;
#if UNITY_WEBGL && !UNITY_EDITOR
            IsAiming &= pointerCaptured && !pointerJustCaptured;
#endif
            float transitionDuration = Mathf.Max(aimTransitionDuration, 0.01f);
            aimBlend = Mathf.MoveTowards(
                aimBlend,
                IsAiming ? 1f : 0f,
                Time.unscaledDeltaTime / transitionDuration);
            float presentationBlend = Mathf.SmoothStep(0f, 1f, aimBlend);
            float lookMultiplier = Mathf.Lerp(1f, aimLookSpeedMultiplier, presentationBlend);

            yaw += look.x * lookSensitivity * lookMultiplier;
            pitch = Mathf.Clamp(
                pitch - look.y * lookSensitivity * lookMultiplier,
                minimumPitch,
                maximumPitch);

            orbitTarget.position = playerTarget.position + Vector3.up * Mathf.Lerp(
                targetHeight,
                aimTargetHeight,
                presentationBlend);
            orbitTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);

            ApplyPresentation(presentationBlend);
        }

        public void Configure(Transform target, InputSystemLogicalInputAdapter input)
        {
            playerTarget = target;
            logicalInput = input;
            if (playerTarget != null)
            {
                yaw = playerTarget.eulerAngles.y;
            }

            EnsurePlayerCollisionIgnored();
            ApplyPipelineConfiguration();
        }

        private void EnsurePlayerCollisionIgnored()
        {
            if (playerTarget != null && playerTarget.CompareTag("Untagged"))
            {
                playerTarget.gameObject.tag = CameraObstacleIgnoreTag;
            }
        }

        private void ApplyPipelineConfiguration()
        {
            if (virtualCamera != null && orbitTarget != null)
            {
                virtualCamera.Follow = orbitTarget;
                virtualCamera.LookAt = null;
            }

            if (thirdPersonFollow != null)
            {
                CinemachineThirdPersonFollow.ObstacleSettings obstacleSettings =
                    thirdPersonFollow.AvoidObstacles;
                obstacleSettings.Enabled = true;
                obstacleSettings.CollisionFilter = Physics.DefaultRaycastLayers;
                obstacleSettings.IgnoreTag = CameraObstacleIgnoreTag;
                obstacleSettings.CameraRadius = cameraCollisionRadius;
                obstacleSettings.DampingIntoCollision = 0f;
                obstacleSettings.DampingFromCollision = 0.5f;
                thirdPersonFollow.AvoidObstacles = obstacleSettings;
            }

            ApplyPresentation(Mathf.SmoothStep(0f, 1f, aimBlend));
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
                    Mathf.Lerp(normalShoulderHeight, aimShoulderHeight, clampedBlend),
                    0f);
                thirdPersonFollow.VerticalArmLength = Mathf.Lerp(
                    normalVerticalArmLength,
                    aimVerticalArmLength,
                    clampedBlend);
                thirdPersonFollow.CameraSide = 1f;
                thirdPersonFollow.Damping = Vector3.Lerp(
                    normalDamping,
                    aimDamping,
                    clampedBlend);
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
