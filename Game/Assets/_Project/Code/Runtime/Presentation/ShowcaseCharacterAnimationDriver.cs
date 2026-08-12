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

        [SerializeField]
        private float runThreshold = 0.65f;

        private PrototypeInteractionTarget observedCarriedTarget;
        private PrototypeWeightClass observedCarriedWeight = PrototypeWeightClass.Light;
        private uint observedThrowOriginCount;
        private int observedAllyReleaseCount;

        public ShowcaseCharacterAnimationRole Role => role;

        public Animator Animator => animator;

        public CharacterController MovementController => movementController;

        public bool IsRuntimeConfigured => animator != null && animator.runtimeAnimatorController != null;

        public float RunThreshold => runThreshold;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
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
            animator.SetFloat(SpeedParameter, normalizedSpeed, 0.08f, Time.deltaTime);

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

        public void Configure(
            ShowcaseCharacterAnimationRole animationRole,
            Animator authoredAnimator,
            CharacterController authoredMovementController = null,
            InputSystemLogicalInputAdapter authoredLogicalInput = null,
            PrototypePhysicalObjectLifecycle authoredPhysicalLifecycle = null,
            PrototypeAllyRelayController authoredAllyRelay = null,
            float authoredRunThreshold = 0.65f)
        {
            role = animationRole;
            animator = authoredAnimator;
            movementController = authoredMovementController;
            logicalInput = authoredLogicalInput;
            physicalLifecycle = authoredPhysicalLifecycle;
            allyRelay = authoredAllyRelay;
            runThreshold = Mathf.Clamp01(authoredRunThreshold);
            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            }

            CaptureCurrentGameplayState();
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
