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
        private const string BinderWindupReadyStateName = "BinderWindupReady";
        private const string BinderWindupReadyStatePath =
            "Base Layer." + BinderWindupReadyStateName;
        private static readonly int BinderWindupReadyStateHash =
            Animator.StringToHash(BinderWindupReadyStatePath);

        private const float MinimumPlaybackRate = 0.75f;
        private const float MaximumPlaybackRate = 1.5f;
        private const float LocomotionBlendSeconds = 0.12f;
        private const float LocomotionToActionBlendSeconds = 0.08f;
        private const float BinderWindupReadyBlendSeconds = 0.08f;
        private const float ComboBlendSeconds = 0.06f;
        private const float HitReactionBlendSeconds = 0.06f;
        private const float KnockdownBlendSeconds = 0.04f;
        private const float AimDirectionAxisSwitchMargin = 0.15f;
        private const float AimUpperBodyBlendSeconds = 0.10f;

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
        private uint observedRunnerCueRevision;
        private PrototypeRelayPhase observedRelayPhase;
        private Vector3 previousWorldPosition;
        private bool observedGrounded = true;
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
        private float localHitStopUntil;
        private bool localHitStopActive;
        private int aimUpperBodyLayerIndex = -1;
        private bool hasAimUpperBodyPose;

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

            if (pendingActionStateHash == 0 ||
                !IsAnimatorInState(pendingActionStateHash))
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
            if (playerCombat != null &&
                playerCombat.AnimationCueRevision != observedPlayerCueRevision)
            {
                observedPlayerCueRevision = playerCombat.AnimationCueRevision;
                if (TryResolvePlayerCueState(playerCombat.AnimationCue, out string state))
                {
                    SetAimUpperBodyActive(false, true);
                    PlayerLocomotionDirection = ShowcasePlayerLocomotionDirection.None;
                    PlayState(
                        state,
                        ResolvePlayerActionBlend(playerCombat.AnimationCue),
                        true);
                    QueueActionStateAcknowledgement(
                        state,
                        playerCombat.AnimationCueRevision);
                    decorativePlaybackActive = false;
                    jumpPlaybackActive = false;
                    actionCueStartedThisFrame = true;
                }
            }

            if (actionCueStartedThisFrame ||
                (playerCombat != null && playerCombat.IsActionLocked))
            {
                SetAimUpperBodyActive(false, true);
                PlayerLocomotionDirection = ShowcasePlayerLocomotionDirection.None;
                // An action owns the full body and mobility until completion.
                // Discard the pre-action locomotion observation so unlock cannot
                // synthesize a decorative stop that never happened on screen.
                observedMoving = false;
                observedSprinting = false;
                return;
            }

            bool grounded = locomotion == null || locomotion.IsGrounded;
            if (observedGrounded && !grounded && locomotion != null &&
                locomotion.VerticalVelocity > 0f)
            {
                SetAimUpperBodyActive(false, true);
                PlayerLocomotionDirection = ShowcasePlayerLocomotionDirection.None;
                PlayState("JumpAction", LocomotionToActionBlendSeconds, true);
                jumpPlaybackActive = true;
                decorativePlaybackActive = false;
            }
            observedGrounded = grounded;

            if (jumpPlaybackActive)
            {
                if (!grounded)
                {
                    return;
                }
                jumpPlaybackActive = false;
            }

            bool aiming = logicalInput != null && logicalInput.Snapshot.Aim &&
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

            bool inactive = !moving && !aiming &&
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
                return "AimIdle";
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
                }
            }

            if (runnerAgent.State == ShowcaseRunnerState.BodyCheck ||
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
                runnerAgent.CommandedPlanarSpeed > 0.05f ? "Walk" : "Idle",
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

        private float ResolvePlayerActionBlend(ShowcasePlayerAnimationCue cue)
        {
            return cue switch
            {
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
            return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.98f;
        }

        private void QueueActionStateAcknowledgement(string stateName, uint cueRevision)
        {
            pendingActionStateHash = Animator.StringToHash("Base Layer." + stateName);
            pendingActionCueRevision = cueRevision;
        }

        private bool IsAnimatorInState(int stateHash)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).fullPathHash == stateHash)
            {
                return true;
            }

            return animator.IsInTransition(0) &&
                animator.GetNextAnimatorStateInfo(0).fullPathHash == stateHash;
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

        private static bool TryResolvePlayerCueState(
            ShowcasePlayerAnimationCue cue,
            out string state)
        {
            state = cue switch
            {
                ShowcasePlayerAnimationCue.MeleeJab => "MeleeJabAction",
                ShowcasePlayerAnimationCue.MeleeCross => "MeleeCrossAction",
                ShowcasePlayerAnimationCue.MeleeHook => "MeleeHookAction",
                ShowcasePlayerAnimationCue.CatchSmallCenter => "CatchLightAction",
                ShowcasePlayerAnimationCue.CatchSmallLeft => "CatchSmallLeftAction",
                ShowcasePlayerAnimationCue.CatchSmallRight => "CatchSmallRightAction",
                ShowcasePlayerAnimationCue.CatchMedium => "CatchMediumAction",
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

        private static bool TryResolveRunnerCueState(
            ShowcaseRunnerAnimationCue cue,
            out string state)
        {
            state = cue switch
            {
                ShowcaseRunnerAnimationCue.Boxing => "AttackBoxingAction",
                ShowcaseRunnerAnimationCue.Boxing2 => "AttackBoxing2Action",
                ShowcaseRunnerAnimationCue.Kicking => "AttackKickingAction",
                ShowcaseRunnerAnimationCue.Kicking2 => "AttackKicking2Action",
                ShowcaseRunnerAnimationCue.EliteMutantPunch => "EliteMutantPunchAction",
                ShowcaseRunnerAnimationCue.EliteMutantSwiping => "EliteMutantSwipingAction",
                ShowcaseRunnerAnimationCue.EliteJumpAttack => "EliteJumpAttackAction",
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
            if (animator == null)
            {
                return;
            }
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
        }

        private void CaptureCurrentGameplayState()
        {
            observedRelayPhase = allyRelay?.RelayPhase ?? PrototypeRelayPhase.None;
            observedPlayerCueRevision = playerCombat?.AnimationCueRevision ?? 0;
            observedRunnerCueRevision = runnerAgent?.AnimationCueRevision ?? 0;
            observedGrounded = locomotion == null || locomotion.IsGrounded;
            observedSprinting = locomotion != null && locomotion.IsSprinting;
            observedMoving = false;
            PlayerLocomotionDirection = ShowcasePlayerLocomotionDirection.None;
            SetAimUpperBodyActive(false, true);
            currentStateHash = 0;
            CurrentBaseStateName = string.Empty;
            pendingActionStateHash = 0;
            pendingActionCueRevision = 0;
            allyBinderWindupReadyPlaybackActive = false;
            ClearPendingAllyBinderWindupReadyContact();
        }
    }
}
