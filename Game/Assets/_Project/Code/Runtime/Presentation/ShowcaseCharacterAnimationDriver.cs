using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Combat;
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

    public enum ShowcasePlayerLocomotionDirection
    {
        None = 0,
        Forward = 1,
        Backward = 2,
        Left = 3,
        Right = 4
    }

    [DefaultExecutionOrder(120)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseCharacterAnimationDriver : MonoBehaviour
    {
        private static readonly int SpeedParameter = Animator.StringToHash("Speed");
        private static readonly int LocomotionPlaybackParameter =
            Animator.StringToHash("LocomotionPlayback");

        private const string AimUpperBodyLayerName = "Aim Upper Body";
        private const string AimUpperBodyStatePath =
            AimUpperBodyLayerName + ".AimUpperBodyPose";
        private const string StrapUpperBodyLayerName = "Strap Upper Body";
        private const string StrapRollingUpperBodyStateName =
            "StrapRollingUpperBodyPose";
        private const string StrapRollingUpperBodyStatePath =
            StrapUpperBodyLayerName + "." + StrapRollingUpperBodyStateName;
        private const string StrapTensionUpperBodyStateName =
            "StrapTensionUpperBodyPose";
        private const string StrapTensionUpperBodyStatePath =
            StrapUpperBodyLayerName + "." + StrapTensionUpperBodyStateName;
        private const string StrapAttackLeftUpperBodyStateName =
            "StrapAttackLeftUpperBodyAction";
        private const string StrapAttackLeftUpperBodyStatePath =
            StrapUpperBodyLayerName + "." + StrapAttackLeftUpperBodyStateName;
        private const string StrapAttackRightUpperBodyStateName =
            "StrapAttackRightUpperBodyAction";
        private const string StrapAttackRightUpperBodyStatePath =
            StrapUpperBodyLayerName + "." + StrapAttackRightUpperBodyStateName;
        private const string MeleeMovingLegsLayerName = "Melee Moving Legs";
        private const string MeleeLegsIdleStateName = "MeleeLegsIdle";
        private const string MeleeLegsForwardStateName = "MeleeLegsForward";
        private const string MeleeLegsBackwardStateName = "MeleeLegsBackward";
        private const string MeleeLegsLeftStateName = "MeleeLegsLeft";
        private const string MeleeLegsRightStateName = "MeleeLegsRight";
        private const string MeleeLegsRunStateName = "MeleeLegsRun";
        private const string BinderWindupReadyStateName = "BinderWindupReady";
        private const string BinderWindupReadyStatePath =
            "Base Layer." + BinderWindupReadyStateName;
        private static readonly int BinderWindupReadyStateHash =
            Animator.StringToHash(BinderWindupReadyStatePath);

        private const float MinimumPlaybackRate = 0.75f;
        private const float MaximumPlaybackRate = 1.5f;
        private const float LocomotionBlendSeconds = 0.12f;
        private const float LocomotionToActionBlendSeconds = 0.08f;
        private const float EnemyActionBlendSeconds = 0.12f;
        private const float BinderWindupReadyBlendSeconds = 0.08f;
        private const float ComboBlendSeconds = 0.06f;
        private const float HitReactionBlendSeconds = 0.06f;
        private const float KnockdownBlendSeconds = 0.04f;
        private const float CargoStrapActionBlendSeconds = 0.08f;
        private const float AimDirectionAxisSwitchMargin = 0.15f;
        private const float AimUpperBodyBlendSeconds = 0.10f;
        private const float JumpTakeoffBlendNormalizedDuration = 0.02f;

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
        private ShowcaseCargoStrapController cargoStrap;

        [SerializeField]
        private PrototypeAllyRelayController allyRelay;

        [SerializeField]
        private ShowcaseMobileAllyController mobileAlly;

        [SerializeField]
        private ShowcasePlayerCombatController playerCombat;

        [SerializeField]
        private ShowcaseRunnerAgent runnerAgent;

        [SerializeField]
        private ThirdPersonLocomotion locomotion;

        [SerializeField, Min(0.1f)]
        private float walkReferenceSpeed = 2.8f;

        private uint observedPlayerCueRevision;
        private uint observedCargoStrapCueRevision;
        private uint observedRunnerCueRevision;
        private uint observedJumpTakeoffRevision;
        private PrototypeRelayPhase observedRelayPhase;
        private Vector3 previousWorldPosition;
        private bool observedSprinting;
        private bool observedMoving;
        private bool decorativePlaybackActive;
        private bool jumpPlaybackActive;
        private bool allyReleasePlaybackActive;
        private bool allyBinderWindupReadyPlaybackActive;
        private PrototypeInteractionTarget pendingAllyBinderWindupReadyTarget;
        private uint pendingAllyBinderWindupReadySequence;
        private bool pendingAllyBinderWindupReadyCommitAttempted;
        private float inactiveDuration;
        private bool idleVariantPlayed;
        private int currentStateHash;
        private int pendingActionStateHash;
        private uint pendingActionCueRevision;
        private int pendingActionLayerIndex;
        private int activeCargoStrapStateHash;
        private uint activeCargoStrapCueRevision;
        private int pendingCargoStrapStateHash;
        private uint pendingCargoStrapCueRevision;
        private int activeCargoStrapLayerIndex;
        private ShowcaseAnimationEventReceiver animationEventReceiver;
        private float localHitStopUntil;
        private bool localHitStopActive;
        private int aimUpperBodyLayerIndex = -1;
        private bool hasAimUpperBodyPose;
        private int strapUpperBodyLayerIndex = -1;
        private bool hasStrapRollingUpperBodyPose;
        private bool hasStrapAttackUpperBodyPoses;
        private int meleeMovingLegsLayerIndex = -1;
        private bool hasMeleeMovingLegsPoses;

        public ShowcaseCharacterAnimationRole Role => role;
        public Animator Animator => animator;
        public CharacterController MovementController => movementController;
        public bool IsRuntimeConfigured =>
            animator != null && animator.runtimeAnimatorController != null;
        public float LastPlanarSpeed { get; private set; }
        public float LastPlaybackRate { get; private set; } = 1f;
        public int CurrentStateHash => currentStateHash;
        public ShowcasePlayerLocomotionDirection PlayerLocomotionDirection
        {
            get;
            private set;
        }
        public string CurrentBaseStateName { get; private set; } = string.Empty;
        public bool HasAimUpperBodyLayer => hasAimUpperBodyPose;
        public float AimUpperBodyLayerWeight { get; private set; }
        public bool HasStrapRollingUpperBodyLayer => hasStrapRollingUpperBodyPose;
        public float StrapRollingUpperBodyLayerWeight { get; private set; }
        public bool HasMeleeMovingLegsLayer => hasMeleeMovingLegsPoses;
        public float MeleeMovingLegsLayerWeight { get; private set; }

        private void Awake()
        {
            animator ??= GetComponentInChildren<Animator>(true);
            movementController ??= GetComponent<CharacterController>();
            ResolveRoleDependencies();
            ConfigureAnimatorRuntime();
            CaptureCurrentGameplayState();
            previousWorldPosition = transform.position;
        }

        private void Update()
        {
            if (!IsRuntimeConfigured)
            {
                return;
            }

            if (localHitStopActive)
            {
                if (Time.unscaledTime < localHitStopUntil)
                {
                    animator.speed = 0f;
                    return;
                }
                animator.speed = 1f;
                localHitStopActive = false;
            }

            LastPlanarSpeed = ResolvePlanarSpeed();
            LastPlaybackRate = CalculateLocomotionPlaybackRate(
                LastPlanarSpeed,
                ResolveNaturalReferenceSpeed());
            animator.SetFloat(
                SpeedParameter,
                Mathf.Clamp01(LastPlanarSpeed / 4.5f));
            animator.SetFloat(LocomotionPlaybackParameter, LastPlaybackRate);
            animator.speed = ResolveCargoActionPlaybackSpeed();

            switch (role)
            {
                case ShowcaseCharacterAnimationRole.Player:
                    UpdatePlayerAnimation();
                    break;
                case ShowcaseCharacterAnimationRole.Ally:
                    UpdateAllyAnimation();
                    break;
                case ShowcaseCharacterAnimationRole.EnemyNormal:
                case ShowcaseCharacterAnimationRole.EnemyElite:
                    UpdateRunnerAnimation();
                    break;
            }

            previousWorldPosition = transform.position;
        }

        public void ApplyLocalHitStop(float duration)
        {
            if (!IsRuntimeConfigured || duration <= 0f)
            {
                return;
            }
            localHitStopUntil = Mathf.Max(
                localHitStopUntil,
                Time.unscaledTime + duration);
            localHitStopActive = true;
            animator.speed = 0f;
        }

        private void LateUpdate()
        {
            if (animator == null)
            {
                return;
            }

            if (role == ShowcaseCharacterAnimationRole.Ally)
            {
                TryCommitAllyBinderWindupReadyContact();
                return;
            }

            if (role == ShowcaseCharacterAnimationRole.Player)
            {
                UpdateCargoStrapAnimationFeedback();
            }

            if (pendingActionStateHash == 0 ||
                !IsAnimatorInState(
                    pendingActionStateHash,
                    pendingActionLayerIndex))
            {
                return;
            }

            if (role == ShowcaseCharacterAnimationRole.Player)
            {
                playerCombat?.NotifyAnimationStateEntered(pendingActionCueRevision);
            }
            else if (role == ShowcaseCharacterAnimationRole.EnemyNormal ||
                     role == ShowcaseCharacterAnimationRole.EnemyElite)
            {
                runnerAgent?.NotifyAnimationStateEntered(pendingActionCueRevision);
            }
            pendingActionStateHash = 0;
            pendingActionCueRevision = 0;
            pendingActionLayerIndex = 0;
        }

        public void Configure(
            ShowcaseCharacterAnimationRole animationRole,
            Animator authoredAnimator,
            CharacterController authoredMovementController = null,
            InputSystemLogicalInputAdapter authoredLogicalInput = null,
            PrototypePhysicalObjectLifecycle authoredPhysicalLifecycle = null,
            PrototypeAllyRelayController authoredAllyRelay = null,
            ShowcaseCargoStrapController authoredCargoStrap = null)
        {
            role = animationRole;
            animator = authoredAnimator;
            movementController = authoredMovementController;
            logicalInput = authoredLogicalInput;
            physicalLifecycle = authoredPhysicalLifecycle;
            allyRelay = authoredAllyRelay;
            cargoStrap = authoredCargoStrap;
            ResolveRoleDependencies();
            ConfigureAnimatorRuntime();
            CaptureCurrentGameplayState();
            previousWorldPosition = transform.position;
        }

        public void ConfigureCombat(
            ShowcasePlayerCombatController authoredPlayerCombat,
            ShowcaseRunnerAgent authoredRunnerAgent,
            ThirdPersonLocomotion authoredLocomotion)
        {
            playerCombat = authoredPlayerCombat;
            runnerAgent = authoredRunnerAgent;
            locomotion = authoredLocomotion;
            observedPlayerCueRevision = playerCombat?.AnimationCueRevision ?? 0;
            observedRunnerCueRevision = runnerAgent?.AnimationCueRevision ?? 0;
            observedJumpTakeoffRevision = locomotion?.JumpTakeoffRevision ?? 0;
        }

        public float CalculateLocomotionPlaybackRate(float planarSpeed)
        {
            return CalculateLocomotionPlaybackRate(
                planarSpeed,
                ResolveNaturalReferenceSpeed());
        }

        public float CalculateLocomotionPlaybackRate(
            float planarSpeed,
            float naturalReferenceSpeed)
        {
            float clampedSpeed = Mathf.Max(0f, planarSpeed);
            if (clampedSpeed <= 0.05f)
            {
                return 1f;
            }

            return Mathf.Clamp(
                clampedSpeed / Mathf.Max(0.1f, naturalReferenceSpeed),
                MinimumPlaybackRate,
                MaximumPlaybackRate);
        }

        private void UpdatePlayerAnimation()
        {
            bool actionCueStartedThisFrame = false;
            bool meleeFullBodyAction = playerCombat != null &&
                playerCombat.IsMeleeActionActive;
            if (!meleeFullBodyAction)
            {
                SetMeleeMovingLegsActive(false, false);
            }
            bool mediumAttackMobility = cargoStrap != null &&
                cargoStrap.IsMediumAttackMobilityPhase &&
                hasStrapRollingUpperBodyPose &&
                hasStrapAttackUpperBodyPoses;
            bool playerReactionCuePending = playerCombat != null &&
                playerCombat.AnimationCueRevision != observedPlayerCueRevision &&
                IsPlayerReactionCue(playerCombat.AnimationCue);
            if (!playerReactionCuePending && cargoStrap != null &&
                cargoStrap.AnimationCueRevision != observedCargoStrapCueRevision)
            {
                observedCargoStrapCueRevision = cargoStrap.AnimationCueRevision;
                if (TryResolveCargoStrapCueState(
                        cargoStrap.AnimationCue,
                        out string strapState))
                {
                    SetAimUpperBodyActive(false, true);
                    SetMeleeMovingLegsActive(false, true);
                    bool smallRollingUpperBody = hasStrapRollingUpperBodyPose &&
                        cargoStrap.IsSmallSpinning &&
                        cargoStrap.AnimationCue ==
                            ShowcaseCargoStrapAnimationCue.Rolling;
                    bool mediumAttackUpperBody = mediumAttackMobility &&
                        (cargoStrap.AnimationCue ==
                             ShowcaseCargoStrapAnimationCue.AttackLeft ||
                         cargoStrap.AnimationCue ==
                             ShowcaseCargoStrapAnimationCue.AttackRight ||
                         cargoStrap.AnimationCue ==
                             ShowcaseCargoStrapAnimationCue.Slam);
                    bool useStrapUpperBody = smallRollingUpperBody ||
                        mediumAttackUpperBody;
                    if (useStrapUpperBody)
                    {
                        string upperBodyState = mediumAttackUpperBody
                            ? cargoStrap.AnimationCue ==
                              ShowcaseCargoStrapAnimationCue.AttackLeft
                                ? StrapAttackLeftUpperBodyStateName
                                : StrapAttackRightUpperBodyStateName
                            : StrapRollingUpperBodyStateName;
                        PlayStrapUpperBodyState(
                            upperBodyState,
                            true);
                        QueueCargoStrapStateAcknowledgement(
                            upperBodyState,
                            cargoStrap.AnimationCueRevision,
                            strapUpperBodyLayerIndex,
                            StrapUpperBodyLayerName);
                    }
                    else
                    {
                        SetStrapRollingUpperBodyActive(false, true);
                        PlayerLocomotionDirection =
                            ShowcasePlayerLocomotionDirection.None;
                        PlayState(
                            strapState,
                            CargoStrapActionBlendSeconds,
                            true);
                        QueueCargoStrapStateAcknowledgement(
                            strapState,
                            cargoStrap.AnimationCueRevision);
                        actionCueStartedThisFrame = true;
                    }
                    decorativePlaybackActive = false;
                    jumpPlaybackActive = false;
                }
                else
                {
                    SetStrapRollingUpperBodyActive(false, true);
                    ClearCargoStrapAnimationFeedback();
                }
            }

            if (!actionCueStartedThisFrame && playerCombat != null &&
                playerCombat.AnimationCueRevision != observedPlayerCueRevision)
            {
                observedPlayerCueRevision = playerCombat.AnimationCueRevision;
                if (TryResolvePlayerCueState(playerCombat.AnimationCue, out string state))
                {
                    SetAimUpperBodyActive(false, true);
                    SetStrapRollingUpperBodyActive(false, true);
                    if (!IsPlayerMeleeCue(playerCombat.AnimationCue))
                    {
                        SetMeleeMovingLegsActive(false, true);
                    }
                    PlayerLocomotionDirection =
                        ShowcasePlayerLocomotionDirection.None;
                    PlayState(
                        state,
                        ResolvePlayerActionBlend(playerCombat.AnimationCue),
                        true);
                    QueueActionStateAcknowledgement(
                        state,
                        playerCombat.AnimationCueRevision);
                    actionCueStartedThisFrame = true;
                    decorativePlaybackActive = false;
                    jumpPlaybackActive = false;
                }
            }

            if (meleeFullBodyAction)
            {
                // Preserve the original authored full-body strike (hips, torso,
                // shoulder and arm) on Base. Only moving legs are overridden;
                // standing punches therefore remain byte-for-byte faithful to
                // the previously approved presentation.
                SetAimUpperBodyActive(false, true);
                SetStrapRollingUpperBodyActive(false, true);
                UpdateMeleeMovingLegsLocomotion();
                PlayerLocomotionDirection = ShowcasePlayerLocomotionDirection.None;
                observedMoving = LastPlanarSpeed > 0.05f;
                observedSprinting = locomotion != null && locomotion.IsSprinting;
                return;
            }

            if (actionCueStartedThisFrame ||
                (cargoStrap != null && cargoStrap.IsActionLocked &&
                 !cargoStrap.IsSmallSpinning &&
                 !mediumAttackMobility) ||
                (playerCombat != null && playerCombat.IsActionLocked))
            {
                SetAimUpperBodyActive(false, true);
                SetStrapRollingUpperBodyActive(false, true);
                SetMeleeMovingLegsActive(false, true);
                PlayerLocomotionDirection = ShowcasePlayerLocomotionDirection.None;
                // An action owns the full body and mobility until completion.
                // Discard the pre-action locomotion observation so unlock cannot
                // synthesize a decorative stop that never happened on screen.
                observedMoving = false;
                observedSprinting = false;
                return;
            }

            bool smallSpinning = cargoStrap != null && cargoStrap.IsSmallSpinning &&
                hasStrapRollingUpperBodyPose;
            bool tensionedStrap = cargoStrap != null &&
                cargoStrap.IsStrapWeaponSelected &&
                cargoStrap.Phase == ShowcaseCargoStrapPhase.Tensioned &&
                hasStrapRollingUpperBodyPose;
            bool strapLocomotionOverlay = smallSpinning || tensionedStrap ||
                mediumAttackMobility;
            if (strapLocomotionOverlay)
            {
                SetAimUpperBodyActive(false, true);
                string upperBodyState = mediumAttackMobility && cargoStrap.Phase ==
                    ShowcaseCargoStrapPhase.AttackLeft
                        ? StrapAttackLeftUpperBodyStateName
                        : mediumAttackMobility &&
                          (cargoStrap.Phase ==
                               ShowcaseCargoStrapPhase.AttackRight ||
                           cargoStrap.Phase == ShowcaseCargoStrapPhase.Slam)
                            ? StrapAttackRightUpperBodyStateName
                            : tensionedStrap || mediumAttackMobility
                                ? StrapTensionUpperBodyStateName
                                : StrapRollingUpperBodyStateName;
                PlayStrapUpperBodyState(upperBodyState, false);
            }
            else
            {
                SetStrapRollingUpperBodyActive(false, false);
            }

            bool jumpTakeoffStartedThisFrame = false;
            if (locomotion != null &&
                locomotion.JumpTakeoffRevision != observedJumpTakeoffRevision)
            {
                observedJumpTakeoffRevision = locomotion.JumpTakeoffRevision;
                if (locomotion.VerticalVelocity > 0f)
                {
                    SetAimUpperBodyActive(false, true);
                    PlayerLocomotionDirection = ShowcasePlayerLocomotionDirection.None;
                    PlayJumpTakeoffState();
                    jumpPlaybackActive = true;
                    jumpTakeoffStartedThisFrame = true;
                    decorativePlaybackActive = false;
                }
            }

            bool grounded = locomotion == null || locomotion.IsGrounded;

            if (jumpPlaybackActive)
            {
                if (jumpTakeoffStartedThisFrame || !grounded)
                {
                    return;
                }
                jumpPlaybackActive = false;
            }

            bool aiming = !strapLocomotionOverlay && logicalInput != null &&
                logicalInput.Snapshot.Aim &&
                (playerCombat == null || playerCombat.AllowsAim);
            bool sprinting = locomotion != null && locomotion.IsSprinting;
            bool moving = LastPlanarSpeed > 0.05f;

            bool stopPlaybackStartedThisFrame = observedMoving && !moving && !aiming;
            if (stopPlaybackStartedThisFrame)
            {
                PlayState(
                    observedSprinting ? "RunStopAction" : "WalkStopAction",
                    LocomotionToActionBlendSeconds,
                    true);
                decorativePlaybackActive = true;
            }
            observedSprinting = sprinting;
            observedMoving = moving;

            if (stopPlaybackStartedThisFrame)
            {
                // Stop clips own the full body.  Clear the Aim overlay before
                // returning, otherwise the decorative early-return path would
                // freeze a partially blended upper-body pose for the whole clip.
                SetAimUpperBodyActive(false, true);
                PlayerLocomotionDirection = ShowcasePlayerLocomotionDirection.None;
                return;
            }

            if (decorativePlaybackActive)
            {
                if (moving || aiming)
                {
                    decorativePlaybackActive = false;
                }
                else if (!CurrentStateFinished())
                {
                    return;
                }
                else
                {
                    decorativePlaybackActive = false;
                }
            }

            bool inactive = !strapLocomotionOverlay && !moving && !aiming &&
                physicalLifecycle?.CarriedTarget == null;
            inactiveDuration = inactive ? inactiveDuration + Time.deltaTime : 0f;
            if (!inactive)
            {
                idleVariantPlayed = false;
            }
            else if (!idleVariantPlayed && inactiveDuration >= 8f)
            {
                SetAimUpperBodyActive(false, false);
                PlayerLocomotionDirection = ShowcasePlayerLocomotionDirection.None;
                PlayState("IdleVariantAction", LocomotionToActionBlendSeconds, true);
                idleVariantPlayed = true;
                decorativePlaybackActive = true;
                return;
            }

            string locomotionState;
            if (aiming)
            {
                SetAimUpperBodyActive(true, false);
                locomotionState = ResolveAimLocomotionState(moving);
                animator.SetFloat(
                    LocomotionPlaybackParameter,
                    PlayerLocomotionDirection ==
                        ShowcasePlayerLocomotionDirection.Backward
                        ? -LastPlaybackRate
                        : LastPlaybackRate);
            }
            else
            {
                SetAimUpperBodyActive(false, false);
                PlayerLocomotionDirection = ShowcasePlayerLocomotionDirection.None;
                animator.SetFloat(LocomotionPlaybackParameter, LastPlaybackRate);
                locomotionState = sprinting && moving
                    ? "Run"
                    : moving
                        ? "Walk"
                        : "Idle";
            }
            PlayState(locomotionState, LocomotionBlendSeconds, false);
        }

        private string ResolveAimLocomotionState(bool moving)
        {
            if (!moving)
            {
                PlayerLocomotionDirection = ShowcasePlayerLocomotionDirection.None;
                // The full-body AimIdle source shifts and twists the authored
                // pelvis. Keep the stable Idle lower body and apply the existing
                // masked Aim pose only to the upper body.
                return hasAimUpperBodyPose ? "Idle" : "AimIdle";
            }

            // The directional base clips provide the legs while the masked
            // Aim Upper Body layer preserves the authored aiming pose. If the
            // optional layer is absent, retain the safe full-body AimIdle pose
            // instead of allowing an un-aimed walk underneath gameplay aim.
            if (!hasAimUpperBodyPose)
            {
                PlayerLocomotionDirection = ShowcasePlayerLocomotionDirection.None;
                return "AimIdle";
            }

            Vector3 worldVelocity = ResolveWorldPlanarVelocity();
            Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);
            Vector2 localDirection = new(localVelocity.x, localVelocity.z);
            if (localDirection.sqrMagnitude <= 0.0025f)
            {
                PlayerLocomotionDirection = ShowcasePlayerLocomotionDirection.None;
                return "AimIdle";
            }

            localDirection.Normalize();
            float lateral = Mathf.Abs(localDirection.x);
            float longitudinal = Mathf.Abs(localDirection.y);
            bool useLateral = ResolveAimLateralAxis(lateral, longitudinal);

            PlayerLocomotionDirection = useLateral
                ? localDirection.x < 0f
                    ? ShowcasePlayerLocomotionDirection.Left
                    : ShowcasePlayerLocomotionDirection.Right
                : localDirection.y < 0f
                    ? ShowcasePlayerLocomotionDirection.Backward
                    : ShowcasePlayerLocomotionDirection.Forward;

            string requestedState = PlayerLocomotionDirection switch
            {
                ShowcasePlayerLocomotionDirection.Forward => "AimForward",
                ShowcasePlayerLocomotionDirection.Backward => "AimBackward",
                ShowcasePlayerLocomotionDirection.Left => "AimStrafeLeft",
                ShowcasePlayerLocomotionDirection.Right => "AimStrafeRight",
                _ => "AimIdle"
            };

            if (HasBaseState(requestedState))
            {
                return requestedState;
            }

            PlayerLocomotionDirection = ShowcasePlayerLocomotionDirection.None;
            return "AimIdle";
        }

        private bool ResolveAimLateralAxis(float lateral, float longitudinal)
        {
            bool wasLateral =
                PlayerLocomotionDirection == ShowcasePlayerLocomotionDirection.Left ||
                PlayerLocomotionDirection == ShowcasePlayerLocomotionDirection.Right;
            bool wasLongitudinal =
                PlayerLocomotionDirection == ShowcasePlayerLocomotionDirection.Forward ||
                PlayerLocomotionDirection == ShowcasePlayerLocomotionDirection.Backward;

            if (wasLateral)
            {
                return longitudinal <= lateral + AimDirectionAxisSwitchMargin;
            }
            if (wasLongitudinal)
            {
                return lateral > longitudinal + AimDirectionAxisSwitchMargin;
            }
            return lateral > longitudinal;
        }

        private void SetAimUpperBodyActive(bool active, bool immediate)
        {
            float targetWeight = active && hasAimUpperBodyPose ? 1f : 0f;
            if (immediate || AimUpperBodyBlendSeconds <= 0f)
            {
                AimUpperBodyLayerWeight = targetWeight;
            }
            else
            {
                AimUpperBodyLayerWeight = Mathf.MoveTowards(
                    AimUpperBodyLayerWeight,
                    targetWeight,
                    Time.deltaTime / AimUpperBodyBlendSeconds);
            }

            if (aimUpperBodyLayerIndex >= 0 && animator != null)
            {
                animator.SetLayerWeight(
                    aimUpperBodyLayerIndex,
                    AimUpperBodyLayerWeight);
            }
        }

        private void PlayStrapUpperBodyState(string stateName, bool restart)
        {
            SetStrapRollingUpperBodyActive(true, true);
            int stateHash = Animator.StringToHash(
                StrapUpperBodyLayerName + "." + stateName);
            if (animator == null || strapUpperBodyLayerIndex < 0 ||
                (!restart && IsAnimatorInState(
                    stateHash,
                    strapUpperBodyLayerIndex)))
            {
                return;
            }

            animator.CrossFadeInFixedTime(
                stateHash,
                CargoStrapActionBlendSeconds,
                strapUpperBodyLayerIndex,
                0f);
        }

        private void SetStrapRollingUpperBodyActive(bool active, bool immediate)
        {
            float targetWeight = active && hasStrapRollingUpperBodyPose ? 1f : 0f;
            if (immediate || AimUpperBodyBlendSeconds <= 0f)
            {
                StrapRollingUpperBodyLayerWeight = targetWeight;
            }
            else
            {
                StrapRollingUpperBodyLayerWeight = Mathf.MoveTowards(
                    StrapRollingUpperBodyLayerWeight,
                    targetWeight,
                    Time.deltaTime / AimUpperBodyBlendSeconds);
            }

            if (strapUpperBodyLayerIndex >= 0 && animator != null)
            {
                animator.SetLayerWeight(
                    strapUpperBodyLayerIndex,
                    StrapRollingUpperBodyLayerWeight);
            }
        }

        private void UpdateMeleeMovingLegsLocomotion()
        {
            bool moving = LastPlanarSpeed > 0.05f;
            if (!moving || !hasMeleeMovingLegsPoses)
            {
                SetMeleeMovingLegsActive(false, false);
                return;
            }

            string stateName;
            float playbackRate = LastPlaybackRate;
            if (locomotion != null && locomotion.IsSprinting)
            {
                stateName = MeleeLegsRunStateName;
            }
            else
            {
                Vector3 localVelocity = transform.InverseTransformDirection(
                    ResolveWorldPlanarVelocity());
                bool lateral = Mathf.Abs(localVelocity.x) >
                    Mathf.Abs(localVelocity.z);
                stateName = lateral
                    ? localVelocity.x < 0f
                        ? MeleeLegsLeftStateName
                        : MeleeLegsRightStateName
                    : localVelocity.z < 0f
                        ? MeleeLegsBackwardStateName
                        : MeleeLegsForwardStateName;
                if (stateName == MeleeLegsBackwardStateName)
                {
                    playbackRate = -playbackRate;
                }
            }
            animator.SetFloat(LocomotionPlaybackParameter, playbackRate);
            int stateHash = Animator.StringToHash(
                MeleeMovingLegsLayerName + "." + stateName);
            if (animator != null && meleeMovingLegsLayerIndex >= 0 &&
                !IsAnimatorInState(stateHash, meleeMovingLegsLayerIndex))
            {
                animator.CrossFadeInFixedTime(
                    stateHash,
                    LocomotionBlendSeconds,
                    meleeMovingLegsLayerIndex,
                    0f);
            }
            SetMeleeMovingLegsActive(true, false);
        }

        private void SetMeleeMovingLegsActive(bool active, bool immediate)
        {
            float targetWeight = active && hasMeleeMovingLegsPoses ? 1f : 0f;
            if (immediate || ComboBlendSeconds <= 0f)
            {
                MeleeMovingLegsLayerWeight = targetWeight;
            }
            else
            {
                MeleeMovingLegsLayerWeight = Mathf.MoveTowards(
                    MeleeMovingLegsLayerWeight,
                    targetWeight,
                    Time.deltaTime / ComboBlendSeconds);
            }

            if (meleeMovingLegsLayerIndex >= 0 && animator != null)
            {
                animator.SetLayerWeight(
                    meleeMovingLegsLayerIndex,
                    MeleeMovingLegsLayerWeight);
            }
        }

        private bool HasBaseState(string stateName)
        {
            return animator != null && animator.HasState(
                0,
                Animator.StringToHash("Base Layer." + stateName));
        }

        private void UpdateAllyAnimation()
        {
            PrototypeRelayPhase relayPhase =
                allyRelay?.RelayPhase ?? PrototypeRelayPhase.None;
            if (relayPhase == PrototypeRelayPhase.PreRelease &&
                observedRelayPhase != PrototypeRelayPhase.PreRelease)
            {
                bool binderRelease = allyRelay.ActiveTarget != null &&
                    allyRelay.ActiveTarget.Role == PrototypeInteractionRole.AllyBinder;
                if (binderRelease)
                {
                    ClearPendingAllyBinderWindupReadyContact();
                    allyBinderWindupReadyPlaybackActive = false;
                    PlayState(
                        "SupplyReleaseAction",
                        LocomotionToActionBlendSeconds,
                        true);
                }
                allyReleasePlaybackActive = binderRelease;
                observedRelayPhase = relayPhase;
                // Animator evaluates the requested cross-fade after Update.
                // Reading normalized time now would inspect the previous Idle
                // state and cancel the release before its first frame.
                return;
            }
            observedRelayPhase = relayPhase;

            if (allyReleasePlaybackActive)
            {
                if (!CurrentStateFinished())
                {
                    return;
                }
                allyReleasePlaybackActive = false;
                PlayState("Idle", LocomotionBlendSeconds, false);
                return;
            }

            if (allyRelay != null &&
                allyRelay.IsAwaitingBinderWindupReadyContact)
            {
                PrototypeInteractionTarget requestedTarget =
                    allyRelay.BinderWindupReadyTarget;
                uint requestedSequence = allyRelay.BinderWindupReadySequence;
                bool requestChanged =
                    requestedTarget != pendingAllyBinderWindupReadyTarget ||
                    requestedSequence != pendingAllyBinderWindupReadySequence;
                if (!allyBinderWindupReadyPlaybackActive || requestChanged)
                {
                    PlayState(
                        BinderWindupReadyStateName,
                        BinderWindupReadyBlendSeconds,
                        true);
                    pendingAllyBinderWindupReadyTarget = requestedTarget;
                    pendingAllyBinderWindupReadySequence = requestedSequence;
                    pendingAllyBinderWindupReadyCommitAttempted = false;
                    allyBinderWindupReadyPlaybackActive = true;
                }
                return;
            }

            if (allyRelay != null && allyRelay.IsBinderWindupReadyHeld)
            {
                ClearPendingAllyBinderWindupReadyContact();
                allyBinderWindupReadyPlaybackActive = true;
                PlayState(
                    BinderWindupReadyStateName,
                    BinderWindupReadyBlendSeconds,
                    false);
                return;
            }

            ClearPendingAllyBinderWindupReadyContact();
            allyBinderWindupReadyPlaybackActive = false;

            bool running = mobileAlly != null &&
                LastPlanarSpeed > mobileAlly.NormalSpeed + 0.1f;
            string state = LastPlanarSpeed <= 0.05f
                ? "Idle"
                : running
                    ? "Run"
                    : "Walk";
            // Non-Binder Ready and ordinary movement use locomotion. A prepared
            // Binder is handled above and keeps the authored Windup/Ready pose.
            PlayState(state, LocomotionBlendSeconds, false);
        }

        private void UpdateRunnerAnimation()
        {
            if (runnerAgent == null)
            {
                PlayState("Idle", LocomotionBlendSeconds, false);
                return;
            }

            if (runnerAgent.IsAttackWindup)
            {
                // Telegraph time is gameplay-owned. Do not start the authored
                // strike clip until the budgeted wind-up has completed, or its
                // natural contact event can pass before the attack state opens.
                PlayState("Idle", EnemyActionBlendSeconds, false);
                return;
            }

            bool actionCueStartedThisFrame = false;
            if (runnerAgent.AnimationCueRevision != observedRunnerCueRevision)
            {
                observedRunnerCueRevision = runnerAgent.AnimationCueRevision;
                if (TryResolveRunnerCueState(runnerAgent.AnimationCue, out string state))
                {
                    PlayState(
                        state,
                        ResolveRunnerActionBlend(runnerAgent.AnimationCue),
                        true);
                    QueueActionStateAcknowledgement(
                        state,
                        runnerAgent.AnimationCueRevision);
                    actionCueStartedThisFrame = true;
                }
            }

            if (actionCueStartedThisFrame ||
                runnerAgent.IsAttackWindup ||
                runnerAgent.IsAttackRecovery ||
                runnerAgent.State == ShowcaseRunnerState.BodyCheck ||
                runnerAgent.State == ShowcaseRunnerState.Shove ||
                runnerAgent.State == ShowcaseRunnerState.OffBalance ||
                runnerAgent.State == ShowcaseRunnerState.Staggered ||
                runnerAgent.State == ShowcaseRunnerState.KnockedDown ||
                runnerAgent.State == ShowcaseRunnerState.GettingUp ||
                runnerAgent.State == ShowcaseRunnerState.Dead)
            {
                return;
            }

            PlayState(
                runnerAgent.CommandedPlanarSpeed > 0.05f ? "Run" : "Idle",
                LocomotionBlendSeconds,
                false);
        }

        private void PlayState(string stateName, float blendSeconds, bool restart)
        {
            int stateHash = Animator.StringToHash("Base Layer." + stateName);
            if (!animator.HasState(0, stateHash))
            {
                Debug.LogError($"SHOWCASE RUNTIME ANIMATION STATE MISSING: {stateName}", this);
                return;
            }
            if (!restart && currentStateHash == stateHash)
            {
                CurrentBaseStateName = stateName;
                return;
            }

            animator.CrossFadeInFixedTime(stateHash, blendSeconds, 0, 0f);
            currentStateHash = stateHash;
            CurrentBaseStateName = stateName;
        }

        private void PlayJumpTakeoffState()
        {
            const string stateName = "JumpAction";
            int stateHash = Animator.StringToHash("Base Layer." + stateName);
            if (!animator.HasState(0, stateHash))
            {
                Debug.LogError($"SHOWCASE RUNTIME ANIMATION STATE MISSING: {stateName}", this);
                return;
            }

            // Physics has already committed takeoff this frame. Enter the source
            // clip at its authored takeoff frame instead of replaying the nine
            // grounded anticipation frames while the controller is airborne.
            animator.CrossFade(
                stateHash,
                JumpTakeoffBlendNormalizedDuration,
                0,
                ShowcaseChr00AnimationTiming.JumpImpulse);
            currentStateHash = stateHash;
            CurrentBaseStateName = stateName;
        }

        private float ResolvePlayerActionBlend(ShowcasePlayerAnimationCue cue)
        {
            return cue switch
            {
                ShowcasePlayerAnimationCue.MeleeJabReturn or
                ShowcasePlayerAnimationCue.MeleeCross or
                ShowcasePlayerAnimationCue.MeleeHook => ComboBlendSeconds,
                ShowcasePlayerAnimationCue.HitBody => HitReactionBlendSeconds,
                ShowcasePlayerAnimationCue.KnockdownBack or
                ShowcasePlayerAnimationCue.KnockdownFront or
                ShowcasePlayerAnimationCue.Death1 or
                ShowcasePlayerAnimationCue.Death2 => KnockdownBlendSeconds,
                _ => LocomotionToActionBlendSeconds
            };
        }

        private float ResolveRunnerActionBlend(ShowcaseRunnerAnimationCue cue)
        {
            return cue switch
            {
                ShowcaseRunnerAnimationCue.Boxing or
                ShowcaseRunnerAnimationCue.Boxing2 or
                ShowcaseRunnerAnimationCue.Kicking or
                ShowcaseRunnerAnimationCue.Kicking2 or
                ShowcaseRunnerAnimationCue.EliteMutantPunch or
                ShowcaseRunnerAnimationCue.EliteMutantSwiping =>
                    EnemyActionBlendSeconds,
                ShowcaseRunnerAnimationCue.HitLight or
                ShowcaseRunnerAnimationCue.HitBody or
                ShowcaseRunnerAnimationCue.HitRib or
                ShowcaseRunnerAnimationCue.HitStomach or
                ShowcaseRunnerAnimationCue.HitBigHead => HitReactionBlendSeconds,
                ShowcaseRunnerAnimationCue.KnockdownBack or
                ShowcaseRunnerAnimationCue.KnockdownFront or
                ShowcaseRunnerAnimationCue.Death1 or
                ShowcaseRunnerAnimationCue.Death2 => KnockdownBlendSeconds,
                _ => LocomotionToActionBlendSeconds
            };
        }

        private bool CurrentStateFinished()
        {
            if (animator.IsInTransition(0))
            {
                return false;
            }
            float finishNormalized = role == ShowcaseCharacterAnimationRole.Player
                ? CurrentBaseStateName switch
                {
                    "WalkStopAction" => ShowcaseChr00AnimationTiming.WalkStopComplete,
                    "RunStopAction" => ShowcaseChr00AnimationTiming.RunStopComplete,
                    "IdleVariantAction" => ShowcaseChr00AnimationTiming.IdleVariantComplete,
                    _ => 0.98f
                }
                : 0.98f;
            return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >=
                finishNormalized;
        }

        private void QueueActionStateAcknowledgement(
            string stateName,
            uint cueRevision,
            int layerIndex = 0,
            string layerName = "Base Layer")
        {
            pendingActionStateHash = Animator.StringToHash(layerName + "." + stateName);
            pendingActionCueRevision = cueRevision;
            pendingActionLayerIndex = layerIndex;
        }

        private void QueueCargoStrapStateAcknowledgement(
            string stateName,
            uint cueRevision,
            int layerIndex = 0,
            string layerName = "Base Layer")
        {
            int stateHash = Animator.StringToHash(layerName + "." + stateName);
            activeCargoStrapStateHash = stateHash;
            activeCargoStrapCueRevision = cueRevision;
            activeCargoStrapLayerIndex = layerIndex;
            pendingCargoStrapStateHash = stateHash;
            pendingCargoStrapCueRevision = cueRevision;
            animationEventReceiver ??=
                animator.GetComponent<ShowcaseAnimationEventReceiver>();
            animationEventReceiver?.SetCargoStrapAnimationRevision(cueRevision);
        }

        private void UpdateCargoStrapAnimationFeedback()
        {
            if (cargoStrap == null || activeCargoStrapStateHash == 0 ||
                cargoStrap.AnimationCueRevision != activeCargoStrapCueRevision ||
                !IsAnimatorInState(
                    activeCargoStrapStateHash,
                    activeCargoStrapLayerIndex))
            {
                return;
            }

            animationEventReceiver ??=
                animator.GetComponent<ShowcaseAnimationEventReceiver>();
            animationEventReceiver?.SetCargoStrapAnimationRevision(
                activeCargoStrapCueRevision);

            if (pendingCargoStrapStateHash == activeCargoStrapStateHash &&
                pendingCargoStrapCueRevision == activeCargoStrapCueRevision)
            {
                cargoStrap.NotifyAnimationStateEntered(
                    pendingCargoStrapCueRevision);
                pendingCargoStrapStateHash = 0;
                pendingCargoStrapCueRevision = 0;
            }

            AnimatorStateInfo stateInfo =
                animator.GetCurrentAnimatorStateInfo(activeCargoStrapLayerIndex);
            if (stateInfo.fullPathHash != activeCargoStrapStateHash &&
                animator.IsInTransition(activeCargoStrapLayerIndex))
            {
                stateInfo = animator.GetNextAnimatorStateInfo(
                    activeCargoStrapLayerIndex);
            }
            cargoStrap.ReportAnimationProgress(
                activeCargoStrapCueRevision,
                Mathf.Clamp01(stateInfo.normalizedTime));
        }

        private void ClearCargoStrapAnimationFeedback()
        {
            activeCargoStrapStateHash = 0;
            activeCargoStrapCueRevision = 0;
            pendingCargoStrapStateHash = 0;
            pendingCargoStrapCueRevision = 0;
            activeCargoStrapLayerIndex = 0;
        }

        private bool IsAnimatorInState(int stateHash)
        {
            return IsAnimatorInState(stateHash, 0);
        }

        private bool IsAnimatorInState(int stateHash, int layerIndex)
        {
            if (animator.GetCurrentAnimatorStateInfo(layerIndex).fullPathHash == stateHash)
            {
                return true;
            }

            return animator.IsInTransition(layerIndex) &&
                animator.GetNextAnimatorStateInfo(layerIndex).fullPathHash == stateHash;
        }

        private void TryCommitAllyBinderWindupReadyContact()
        {
            if (pendingAllyBinderWindupReadyCommitAttempted ||
                pendingAllyBinderWindupReadyTarget == null ||
                pendingAllyBinderWindupReadySequence == 0 ||
                allyRelay == null)
            {
                return;
            }

            if (allyRelay.BinderWindupReadyTarget !=
                    pendingAllyBinderWindupReadyTarget ||
                allyRelay.BinderWindupReadySequence !=
                    pendingAllyBinderWindupReadySequence)
            {
                ClearPendingAllyBinderWindupReadyContact();
                return;
            }

            if (animator.IsInTransition(0) ||
                animator.GetCurrentAnimatorStateInfo(0).fullPathHash !=
                    BinderWindupReadyStateHash)
            {
                return;
            }

            pendingAllyBinderWindupReadyCommitAttempted =
                allyRelay.CommitBinderWindupReadyContact(
                pendingAllyBinderWindupReadyTarget,
                pendingAllyBinderWindupReadySequence);
        }

        private void ClearPendingAllyBinderWindupReadyContact()
        {
            pendingAllyBinderWindupReadyTarget = null;
            pendingAllyBinderWindupReadySequence = 0;
            pendingAllyBinderWindupReadyCommitAttempted = false;
        }

        private float ResolvePlanarSpeed()
        {
            if (role == ShowcaseCharacterAnimationRole.Ally && mobileAlly != null)
            {
                return Mathf.Max(0f, mobileAlly.ActualPlanarSpeed);
            }

            if (runnerAgent != null)
            {
                return Mathf.Max(0f, runnerAgent.CommandedPlanarSpeed);
            }

            Vector3 velocity = ResolveWorldPlanarVelocity();
            return new Vector2(velocity.x, velocity.z).magnitude;
        }

        private Vector3 ResolveWorldPlanarVelocity()
        {
            Vector3 velocity = movementController != null
                ? movementController.velocity
                : (transform.position - previousWorldPosition) /
                  Mathf.Max(0.0001f, Time.deltaTime);
            velocity.y = 0f;
            return velocity;
        }

        private float ResolveNaturalReferenceSpeed()
        {
            switch (role)
            {
                case ShowcaseCharacterAnimationRole.Player:
                    if (locomotion != null)
                    {
                        return locomotion.IsSprinting
                            ? locomotion.SprintSpeed
                            : locomotion.MoveSpeed;
                    }
                    return walkReferenceSpeed;
                case ShowcaseCharacterAnimationRole.Ally:
                    if (mobileAlly == null)
                    {
                        return walkReferenceSpeed;
                    }
                    return LastPlanarSpeed > mobileAlly.NormalSpeed + 0.1f
                        ? mobileAlly.CatchUpSpeed
                        : mobileAlly.NormalSpeed;
                default:
                    return 2.7f;
            }
        }

        private float ResolveCargoActionPlaybackSpeed()
        {
            if (role != ShowcaseCharacterAnimationRole.Player ||
                cargoStrap == null || !cargoStrap.IsAttackActive ||
                cargoStrap.ActiveTarget == null ||
                cargoStrap.ActiveTarget.WeightClass !=
                    PrototypeWeightClass.Medium)
            {
                return 1f;
            }

            return Mathf.Clamp(
                cargoStrap.ActiveAttackChargeProfile.AnimationSpeedMultiplier,
                1f,
                1.8f);
        }

        private static bool TryResolveCargoStrapCueState(
            ShowcaseCargoStrapAnimationCue cue,
            out string state)
        {
            state = cue switch
            {
                ShowcaseCargoStrapAnimationCue.Prepare => "StrapPrepareAction",
                ShowcaseCargoStrapAnimationCue.Rolling => "StrapRollingAction",
                ShowcaseCargoStrapAnimationCue.Throw => "StrapThrowAction",
                ShowcaseCargoStrapAnimationCue.Pull => "StrapPullAction",
                ShowcaseCargoStrapAnimationCue.AttackLeft => "StrapAttackLeftAction",
                ShowcaseCargoStrapAnimationCue.AttackRight => "StrapAttackRightAction",
                // No approved overhead clip exists. Slam deliberately reuses
                // the approved right-side upper-body action while the attached
                // object performs the authored procedural vertical path.
                ShowcaseCargoStrapAnimationCue.Slam => "StrapAttackRightAction",
                _ => string.Empty
            };
            return !string.IsNullOrEmpty(state);
        }

        private static bool TryResolvePlayerCueState(
            ShowcasePlayerAnimationCue cue,
            out string state)
        {
            state = cue switch
            {
                ShowcasePlayerAnimationCue.MeleeJab => "MeleeJabAction",
                ShowcasePlayerAnimationCue.MeleeJabReturn => "MeleeJabReturnAction",
                ShowcasePlayerAnimationCue.MeleeCross => "MeleeCrossAction",
                ShowcasePlayerAnimationCue.MeleeHook => "MeleeHookAction",
                ShowcasePlayerAnimationCue.CatchSmallCenter => "CatchLightAction",
                ShowcasePlayerAnimationCue.CatchSmallLeft => "CatchSmallLeftAction",
                ShowcasePlayerAnimationCue.CatchSmallRight => "CatchSmallRightAction",
                ShowcasePlayerAnimationCue.CatchMedium => "CatchMediumAction",
                ShowcasePlayerAnimationCue.PickupLowSmall => "PickupLowSmallAction",
                ShowcasePlayerAnimationCue.PickupLowMedium => "PickupLowMediumAction",
                ShowcasePlayerAnimationCue.ThrowSmall => "ThrowLightAction",
                ShowcasePlayerAnimationCue.ThrowMedium => "ThrowMediumAction",
                ShowcasePlayerAnimationCue.HitBody => "HitBodyAction",
                ShowcasePlayerAnimationCue.KnockdownBack => "KnockdownBackAction",
                ShowcasePlayerAnimationCue.KnockdownFront => "KnockdownFrontAction",
                ShowcasePlayerAnimationCue.GetupBack => "GetupBackAction",
                ShowcasePlayerAnimationCue.GetupFront => "GetupFrontAction",
                ShowcasePlayerAnimationCue.Death1 => "Death1Action",
                ShowcasePlayerAnimationCue.Death2 => "Death2Action",
                _ => string.Empty
            };
            return !string.IsNullOrEmpty(state);
        }

        private static bool IsPlayerMeleeCue(ShowcasePlayerAnimationCue cue)
        {
            return cue == ShowcasePlayerAnimationCue.MeleeJab ||
                cue == ShowcasePlayerAnimationCue.MeleeJabReturn ||
                cue == ShowcasePlayerAnimationCue.MeleeCross ||
                cue == ShowcasePlayerAnimationCue.MeleeHook;
        }

        private static bool IsPlayerReactionCue(ShowcasePlayerAnimationCue cue)
        {
            return cue == ShowcasePlayerAnimationCue.HitBody ||
                cue == ShowcasePlayerAnimationCue.KnockdownBack ||
                cue == ShowcasePlayerAnimationCue.KnockdownFront ||
                cue == ShowcasePlayerAnimationCue.GetupBack ||
                cue == ShowcasePlayerAnimationCue.GetupFront ||
                cue == ShowcasePlayerAnimationCue.Death1 ||
                cue == ShowcasePlayerAnimationCue.Death2;
        }

        private static bool TryResolveRunnerCueState(
            ShowcaseRunnerAnimationCue cue,
            out string state)
        {
            state = cue switch
            {
                ShowcaseRunnerAnimationCue.Boxing => "AttackBoxingAction",
                ShowcaseRunnerAnimationCue.Boxing2 => "AttackKneeStrikeAction",
                ShowcaseRunnerAnimationCue.Kicking => "AttackKickingAction",
                ShowcaseRunnerAnimationCue.Kicking2 => "AttackKicking2Action",
                ShowcaseRunnerAnimationCue.EliteMutantPunch =>
                    "EliteMutantPunchAction",
                ShowcaseRunnerAnimationCue.EliteMutantSwiping =>
                    "EliteMutantSwipingAction",
                ShowcaseRunnerAnimationCue.HitLight => "HitLightAction",
                ShowcaseRunnerAnimationCue.HitBody => "HitBodyAction",
                ShowcaseRunnerAnimationCue.HitRib => "HitRibAction",
                ShowcaseRunnerAnimationCue.HitStomach => "HitStomachAction",
                ShowcaseRunnerAnimationCue.HitBigHead => "HitBigHeadAction",
                ShowcaseRunnerAnimationCue.KnockdownBack => "KnockdownBackAction",
                ShowcaseRunnerAnimationCue.KnockdownFront => "KnockdownFrontAction",
                ShowcaseRunnerAnimationCue.GetupBack => "GetupBackAction",
                ShowcaseRunnerAnimationCue.GetupFront => "GetupFrontAction",
                ShowcaseRunnerAnimationCue.Death1 => "Death1Action",
                ShowcaseRunnerAnimationCue.Death2 => "Death2Action",
                _ => string.Empty
            };
            return !string.IsNullOrEmpty(state);
        }

        private void ResolveRoleDependencies()
        {
            if (role == ShowcaseCharacterAnimationRole.Player)
            {
                logicalInput ??= GetComponent<InputSystemLogicalInputAdapter>();
                physicalLifecycle ??= GetComponent<PrototypePhysicalObjectLifecycle>();
                cargoStrap ??= GetComponent<ShowcaseCargoStrapController>();
                playerCombat ??= GetComponent<ShowcasePlayerCombatController>();
                locomotion ??= GetComponent<ThirdPersonLocomotion>();
            }
            else if (role == ShowcaseCharacterAnimationRole.Ally)
            {
                allyRelay ??= GetComponent<PrototypeAllyRelayController>();
                mobileAlly ??= GetComponent<ShowcaseMobileAllyController>();
            }
            else
            {
                runnerAgent ??= GetComponent<ShowcaseRunnerAgent>();
            }
        }

        private void ConfigureAnimatorRuntime()
        {
            aimUpperBodyLayerIndex = -1;
            hasAimUpperBodyPose = false;
            AimUpperBodyLayerWeight = 0f;
            strapUpperBodyLayerIndex = -1;
            hasStrapRollingUpperBodyPose = false;
            hasStrapAttackUpperBodyPoses = false;
            StrapRollingUpperBodyLayerWeight = 0f;
            meleeMovingLegsLayerIndex = -1;
            hasMeleeMovingLegsPoses = false;
            MeleeMovingLegsLayerWeight = 0f;
            if (animator == null)
            {
                animationEventReceiver = null;
                return;
            }
            animationEventReceiver =
                animator.GetComponent<ShowcaseAnimationEventReceiver>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.stabilizeFeet = false;

            aimUpperBodyLayerIndex = role == ShowcaseCharacterAnimationRole.Player
                ? animator.GetLayerIndex(AimUpperBodyLayerName)
                : -1;
            hasAimUpperBodyPose = aimUpperBodyLayerIndex >= 0 &&
                animator.HasState(
                    aimUpperBodyLayerIndex,
                    Animator.StringToHash(AimUpperBodyStatePath));
            if (aimUpperBodyLayerIndex >= 0)
            {
                animator.SetLayerWeight(aimUpperBodyLayerIndex, 0f);
            }
            strapUpperBodyLayerIndex = role == ShowcaseCharacterAnimationRole.Player
                ? animator.GetLayerIndex(StrapUpperBodyLayerName)
                : -1;
            hasStrapRollingUpperBodyPose = strapUpperBodyLayerIndex >= 0 &&
                animator.HasState(
                    strapUpperBodyLayerIndex,
                    Animator.StringToHash(StrapRollingUpperBodyStatePath)) &&
                animator.HasState(
                    strapUpperBodyLayerIndex,
                    Animator.StringToHash(StrapTensionUpperBodyStatePath));
            hasStrapAttackUpperBodyPoses = strapUpperBodyLayerIndex >= 0 &&
                animator.HasState(
                    strapUpperBodyLayerIndex,
                    Animator.StringToHash(StrapAttackLeftUpperBodyStatePath)) &&
                animator.HasState(
                    strapUpperBodyLayerIndex,
                    Animator.StringToHash(StrapAttackRightUpperBodyStatePath));
            if (strapUpperBodyLayerIndex >= 0)
            {
                animator.SetLayerWeight(strapUpperBodyLayerIndex, 0f);
            }
            meleeMovingLegsLayerIndex = role == ShowcaseCharacterAnimationRole.Player
                ? animator.GetLayerIndex(MeleeMovingLegsLayerName)
                : -1;
            hasMeleeMovingLegsPoses = meleeMovingLegsLayerIndex >= 0 &&
                animator.HasState(
                    meleeMovingLegsLayerIndex,
                    Animator.StringToHash(
                        MeleeMovingLegsLayerName + "." +
                        MeleeLegsIdleStateName)) &&
                animator.HasState(
                    meleeMovingLegsLayerIndex,
                    Animator.StringToHash(
                        MeleeMovingLegsLayerName + "." +
                        MeleeLegsForwardStateName)) &&
                animator.HasState(
                    meleeMovingLegsLayerIndex,
                    Animator.StringToHash(
                        MeleeMovingLegsLayerName + "." +
                        MeleeLegsBackwardStateName)) &&
                animator.HasState(
                    meleeMovingLegsLayerIndex,
                    Animator.StringToHash(
                        MeleeMovingLegsLayerName + "." +
                        MeleeLegsLeftStateName)) &&
                animator.HasState(
                    meleeMovingLegsLayerIndex,
                    Animator.StringToHash(
                        MeleeMovingLegsLayerName + "." +
                        MeleeLegsRightStateName)) &&
                animator.HasState(
                    meleeMovingLegsLayerIndex,
                    Animator.StringToHash(
                        MeleeMovingLegsLayerName + "." +
                        MeleeLegsRunStateName));
            if (meleeMovingLegsLayerIndex >= 0)
            {
                animator.SetLayerWeight(meleeMovingLegsLayerIndex, 0f);
            }
        }

        private void CaptureCurrentGameplayState()
        {
            observedRelayPhase = allyRelay?.RelayPhase ?? PrototypeRelayPhase.None;
            observedPlayerCueRevision = playerCombat?.AnimationCueRevision ?? 0;
            observedCargoStrapCueRevision =
                cargoStrap?.AnimationCueRevision ?? 0;
            observedRunnerCueRevision = runnerAgent?.AnimationCueRevision ?? 0;
            observedJumpTakeoffRevision = locomotion?.JumpTakeoffRevision ?? 0;
            observedSprinting = locomotion != null && locomotion.IsSprinting;
            observedMoving = false;
            PlayerLocomotionDirection = ShowcasePlayerLocomotionDirection.None;
            SetAimUpperBodyActive(false, true);
            SetStrapRollingUpperBodyActive(false, true);
            SetMeleeMovingLegsActive(false, true);
            currentStateHash = 0;
            CurrentBaseStateName = string.Empty;
            pendingActionStateHash = 0;
            pendingActionCueRevision = 0;
            pendingActionLayerIndex = 0;
            ClearCargoStrapAnimationFeedback();
            allyBinderWindupReadyPlaybackActive = false;
            ClearPendingAllyBinderWindupReadyContact();
        }
    }
}
