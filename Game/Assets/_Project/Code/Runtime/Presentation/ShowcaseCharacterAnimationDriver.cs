using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Input;
using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Presentation
{
    public enum ShowcaseCharacterAnimationRole
    {
        Player = 0,
        Ally = 1,
        EnemyNormal = 2,
        EnemyElite = 3
    }

    [DisallowMultipleComponent]
    public sealed class ShowcaseCharacterAnimationDriver : MonoBehaviour
    {
        private static readonly int SpeedParameter = Animator.StringToHash("Speed");
        private static readonly int LocomotionPlaybackParameter =
            Animator.StringToHash("LocomotionPlayback");
        private static readonly int AimParameter = Animator.StringToHash("Aim");
        private static readonly int SupplyReadyParameter = Animator.StringToHash("SupplyReady");
        private static readonly int CatchLightTrigger = Animator.StringToHash("CatchLight");
        private static readonly int CatchMediumTrigger = Animator.StringToHash("CatchMedium");
        private static readonly int ThrowLightTrigger = Animator.StringToHash("ThrowLight");
        private static readonly int ThrowMediumTrigger = Animator.StringToHash("ThrowMedium");
        private static readonly int SupplyReleaseTrigger = Animator.StringToHash("SupplyRelease");

        [SerializeField]
        private ShowcaseCharacterAnimationRole role;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private CharacterController movementController;

        [SerializeField]
        private InputSystemLogicalInputAdapter logicalInput;

        [SerializeField]
        private PrototypePhysicalObjectLifecycle physicalLifecycle;

        [SerializeField]
        private PrototypeAllyRelayController allyRelay;

        [SerializeField, Min(0.1f)]
        private float walkReferenceSpeed = 2.8f;

        [SerializeField, Range(0.25f, 1f)]
        private float minimumPlaybackRate = 0.85f;

        [SerializeField, Range(1f, 2f)]
        private float maximumPlaybackRate = 1.15f;

        [SerializeField, Range(0.02f, 0.25f)]
        private float maximumVisualGroundingCorrection = 0.12f;

        [SerializeField, Min(0.5f)]
        private float maximumGroundDistanceFromActor = 1.25f;

        private PrototypeInteractionTarget observedCarriedTarget;
        private PrototypeWeightClass observedCarriedWeight = PrototypeWeightClass.Light;
        private uint observedThrowOriginCount;
        private int observedAllyReleaseCount;
        private readonly RaycastHit[] groundingHits = new RaycastHit[16];
        private Renderer[] animatedRenderers;

        public ShowcaseCharacterAnimationRole Role => role;

        public Animator Animator => animator;

        public CharacterController MovementController => movementController;

        public bool IsRuntimeConfigured => animator != null && animator.runtimeAnimatorController != null;

        public float LastPlanarSpeed { get; private set; }

        public float LastPlaybackRate { get; private set; } = 1f;

        public float LastVisualGroundingCorrection { get; private set; }

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }
            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.stabilizeFeet = false;
                CacheAnimatedRenderers();
            }

            if (movementController == null)
            {
                movementController = GetComponent<CharacterController>();
            }

            if (role == ShowcaseCharacterAnimationRole.Player)
            {
                logicalInput ??= GetComponent<InputSystemLogicalInputAdapter>();
                physicalLifecycle ??= GetComponent<PrototypePhysicalObjectLifecycle>();
            }
            else if (role == ShowcaseCharacterAnimationRole.Ally)
            {
                allyRelay ??= GetComponent<PrototypeAllyRelayController>();
            }

            CaptureCurrentGameplayState();
        }

        private void Update()
        {
            if (!IsRuntimeConfigured)
            {
                return;
            }

            Vector3 velocity = movementController != null
                ? movementController.velocity
                : Vector3.zero;
            float planarSpeed = new Vector2(velocity.x, velocity.z).magnitude;
            float normalizedSpeed = Mathf.Clamp01(planarSpeed / 4.5f);
            LastPlanarSpeed = planarSpeed;
            LastPlaybackRate = CalculateLocomotionPlaybackRate(planarSpeed);
            animator.SetFloat(SpeedParameter, normalizedSpeed);
            animator.SetFloat(LocomotionPlaybackParameter, LastPlaybackRate);

            switch (role)
            {
                case ShowcaseCharacterAnimationRole.Player:
                    UpdatePlayerAnimation();
                    break;
                case ShowcaseCharacterAnimationRole.Ally:
                    UpdateAllyAnimation();
                    break;
            }
        }

        private void LateUpdate()
        {
            ApplyVisualGroundingCorrection();
        }

        public void Configure(
            ShowcaseCharacterAnimationRole animationRole,
            Animator authoredAnimator,
            CharacterController authoredMovementController = null,
            InputSystemLogicalInputAdapter authoredLogicalInput = null,
            PrototypePhysicalObjectLifecycle authoredPhysicalLifecycle = null,
            PrototypeAllyRelayController authoredAllyRelay = null)
        {
            role = animationRole;
            animator = authoredAnimator;
            movementController = authoredMovementController;
            logicalInput = authoredLogicalInput;
            physicalLifecycle = authoredPhysicalLifecycle;
            allyRelay = authoredAllyRelay;
            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                animator.stabilizeFeet = false;
                CacheAnimatedRenderers();
            }

            CaptureCurrentGameplayState();
        }

        public float CalculateLocomotionPlaybackRate(float planarSpeed)
        {
            float clampedSpeed = Mathf.Max(0f, planarSpeed);
            if (clampedSpeed <= 0.05f)
            {
                return 1f;
            }

            return Mathf.Clamp(
                clampedSpeed / Mathf.Max(0.1f, walkReferenceSpeed),
                minimumPlaybackRate,
                maximumPlaybackRate);
        }

        public bool ApplyVisualGroundingCorrection()
        {
            LastVisualGroundingCorrection = 0f;
            if (animator == null)
            {
                return false;
            }

            if (animatedRenderers == null || animatedRenderers.Length == 0)
            {
                CacheAnimatedRenderers();
            }
            if (animatedRenderers == null || animatedRenderers.Length == 0)
            {
                return false;
            }

            int hitCount = Physics.RaycastNonAlloc(
                transform.position + Vector3.up * 2f,
                Vector3.down,
                groundingHits,
                8f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            float nearestDistance = float.PositiveInfinity;
            float groundY = 0f;
            bool foundGround = false;
            for (int index = 0; index < hitCount; index++)
            {
                Collider collider = groundingHits[index].collider;
                if (collider == null ||
                    collider.transform == transform ||
                    collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (groundingHits[index].distance < nearestDistance)
                {
                    nearestDistance = groundingHits[index].distance;
                    groundY = groundingHits[index].point.y;
                    foundGround = true;
                }
            }

            if (!foundGround ||
                transform.position.y - groundY > maximumGroundDistanceFromActor)
            {
                return false;
            }

            float visualBottomY = float.PositiveInfinity;
            foreach (Renderer renderer in animatedRenderers)
            {
                if (renderer != null && renderer.enabled)
                {
                    visualBottomY = Mathf.Min(visualBottomY, renderer.bounds.min.y);
                }
            }
            if (float.IsPositiveInfinity(visualBottomY))
            {
                return false;
            }

            float correction = groundY - visualBottomY;
            if (Mathf.Abs(correction) > maximumVisualGroundingCorrection)
            {
                return false;
            }

            animator.transform.position += Vector3.up * correction;
            LastVisualGroundingCorrection = correction;
            return true;
        }

        private void CacheAnimatedRenderers()
        {
            animatedRenderers = animator != null
                ? animator.GetComponentsInChildren<Renderer>(true)
                : null;
        }

        private void UpdatePlayerAnimation()
        {
            bool aiming = logicalInput != null && logicalInput.Snapshot.Aim;
            animator.SetBool(AimParameter, aiming);

            PrototypeInteractionTarget carried = physicalLifecycle?.CarriedTarget;
            if (carried != null && observedCarriedTarget == null)
            {
                observedCarriedWeight = carried.WeightClass;
                animator.SetTrigger(
                    carried.WeightClass == PrototypeWeightClass.Light
                        ? CatchLightTrigger
                        : CatchMediumTrigger);
            }

            if (physicalLifecycle != null &&
                physicalLifecycle.ThrowOriginCount != observedThrowOriginCount)
            {
                animator.SetTrigger(
                    observedCarriedWeight == PrototypeWeightClass.Light
                        ? ThrowLightTrigger
                        : ThrowMediumTrigger);
                observedThrowOriginCount = physicalLifecycle.ThrowOriginCount;
            }

            if (carried != null)
            {
                observedCarriedWeight = carried.WeightClass;
            }

            observedCarriedTarget = carried;
        }

        private void UpdateAllyAnimation()
        {
            bool supplyReady = allyRelay?.PreparedTarget != null &&
                (allyRelay.AllyState == PrototypeAllySupportState.Ready ||
                 allyRelay.AllyState == PrototypeAllySupportState.Blocked);
            animator.SetBool(SupplyReadyParameter, supplyReady);

            if (allyRelay != null && allyRelay.ReleaseCount != observedAllyReleaseCount)
            {
                animator.SetTrigger(SupplyReleaseTrigger);
                observedAllyReleaseCount = allyRelay.ReleaseCount;
            }
        }

        private void CaptureCurrentGameplayState()
        {
            observedCarriedTarget = physicalLifecycle?.CarriedTarget;
            if (observedCarriedTarget != null)
            {
                observedCarriedWeight = observedCarriedTarget.WeightClass;
            }

            observedThrowOriginCount = physicalLifecycle?.ThrowOriginCount ?? 0;
            observedAllyReleaseCount = allyRelay?.ReleaseCount ?? 0;
        }
    }
}
