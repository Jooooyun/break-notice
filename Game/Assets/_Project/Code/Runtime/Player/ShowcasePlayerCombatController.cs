using System;
using BreakNotice.Gameplay.Encounter;
using BreakNotice.Gameplay.Input;
using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Gameplay.Combat
{
    public enum ShowcasePlayerAnimationCue
    {
        None,
        MeleeJab,
        MeleeCross,
        MeleeHook,
        CatchSmallCenter,
        CatchSmallLeft,
        CatchSmallRight,
        CatchMedium,
        PickupLowSmall,
        PickupLowMedium,
        ThrowSmall,
        ThrowMedium,
        HitBody,
        KnockdownBack,
        KnockdownFront,
        GetupBack,
        GetupFront,
        Death1,
        Death2,
        MeleeJabReturn
    }

    [DefaultExecutionOrder(-35)]
    [DisallowMultipleComponent]
    public sealed class ShowcasePlayerCombatController : MonoBehaviour
    {
        private enum ActiveAction
        {
            None,
            Melee,
            Acquire,
            DirectThrow,
            QueuedRelayThrow,
            HitReaction,
            Knockdown,
            Getup,
            Death
        }

        private enum ExternalReactionAllowance
        {
            None,
            Full,
            KnockdownCapped
        }

        private static readonly float[] MeleeDurations = { 0f, 1.00f, 1.05f, 1.15f };
        private static readonly int[] MeleeDamage = { 0, 15, 20, 35 };
        private const float AnimatorStateEntryTimeout = 0.15f;
        private const int MinimumAnimatorStateEntryFrames = 2;
        private const float MaximumMeleeApproachDistance = 0.36f;
        private const float MeleeContactStandoff = 0.84f;
        private const float MeleeApproachSpeed = 5f;

        [SerializeField]
        private ShowcaseCombatHealth health;

        [SerializeField]
        private PrototypeInteractionStateOwner interactionOwner;

        [SerializeField]
        private InputSystemLogicalInputAdapter logicalInput;

        [SerializeField]
        private ThirdPersonLocomotion locomotion;

        [SerializeField]
        private PrototypePhysicalObjectLifecycle physicalLifecycle;

        [SerializeField]
        private PrototypeAllyRelayController allyRelay;

        [SerializeField]
        private ShowcaseRunnerPressureController runnerPressure;

        [SerializeField]
        private AuthoredEncounterResetController resetController;

        [SerializeField]
        private ShowcaseObjectiveController objectiveController;

        [SerializeField]
        private ShowcaseBoneCombatRig boneCombatRig;

        [SerializeField]
        private ShowcaseCargoStrapController cargoStrap;

        [SerializeField]
        private PrototypeInteractionController interactionController;

        [Header("Player Hit Protection")]
        [SerializeField, Range(0.05f, 0.20f)]
        private float damageImmunitySeconds = 0.12f;

        [SerializeField, Range(0.25f, 0.40f)]
        private float hitReactionImmunitySeconds = 0.32f;

        [SerializeField, Range(0.20f, 0.60f)]
        private float getupProtectionSeconds = 0.40f;

        [SerializeField, Range(0f, 0.30f)]
        private float knockdownAdditionalLaunchCap = 0.18f;

        private ActiveAction activeAction;
        private ShowcaseRunnerAgent meleeTarget;
        private PrototypeInteractionTarget pendingTarget;
        private PrototypeInteractionOutcome pendingAcquisitionOutcome;
        private Vector3 pendingThrowDirection;
        private Vector3 pendingThrowAimPoint;
        private bool pendingThrowUsesAimPoint;
        private float pendingThrowCharge;
        private float actionStartedAt;
        private float actionDuration;
        private bool actionEventCommitted;
        private bool meleeQueued;
        private bool meleeHookQueued;
        private bool meleeJabReturnActive;
        private int activeMeleeStep;
        private float lastMeleeCompletedAt = float.NegativeInfinity;
        private uint nextAttackSequence;
        private uint activeAttackSequence;
        private bool getupUsesBackVariant;
        private float actionRequestedAt;
        private int actionRequestedFrame = -1;
        private bool animationStateAcknowledged;
        private uint awaitedAnimationCueRevision;
        private bool meleeBufferWindowOpen;
        private Vector3 pendingReactionDirection;
        private float pendingReactionDistance;
        private float pendingReactionDuration;
        private float reactionDistanceApplied;
        private float reactionElapsed;
        private bool reactionDisplacementPending;
        private bool reactionDisplacementActive;
        private float meleeApproachDistance;
        private Vector3 meleeFacingDirection;
        private bool chairSwingWhooshCommitted;
        private bool closeRangeDisengageActive;
        private bool strappedMeleeActive;
        private int lastAcceptedDamageFrame = -1;
        private float staggerProtectionUntil = float.NegativeInfinity;
        private float getupProtectionUntil = float.NegativeInfinity;
        private float knockdownAdditionalLaunchRemaining;
        private ExternalReactionAllowance externalReactionAllowance;

        public ShowcaseCombatHealth Health => health;
        public ShowcasePlayerAnimationCue AnimationCue { get; private set; }
        public uint AnimationCueRevision { get; private set; }
        public bool IsDead => health != null && health.IsDead;
        public bool IsActionLocked => activeAction != ActiveAction.None;
        public bool BlocksInteraction => IsDead || IsActionLocked;
        public bool AllowsAim => !IsDead && activeAction == ActiveAction.None;
        public bool CanBufferMelee => CanAcceptMeleeFollowUpInput;
        public int ActiveMeleeStep => activeMeleeStep;
        public bool HasBufferedMelee => meleeQueued || meleeHookQueued;
        public uint ActiveAttackSequence => activeAttackSequence;
        public bool IsMeleeActionActive => activeAction == ActiveAction.Melee;
        public ShowcaseRunnerAgent ActiveMeleeTarget => meleeTarget;
        public float MeleeApproachDistance => meleeApproachDistance;
        public bool IsCloseRangeDisengageActive =>
            closeRangeDisengageActive && IsMeleeActionActive;
        public bool IsStrappedMeleeActive =>
            strappedMeleeActive && IsMeleeActionActive;
        public int CloseRangeDisengageCount { get; private set; }
        public int CloseRangeDisengageHitCount { get; private set; }
        public int SameFrameDamageDedupCount { get; private set; }
        public int ProtectedStaggerCount { get; private set; }
        public int SuppressedAdditionalLaunchCount { get; private set; }
        public float AdditionalKnockdownLaunchDistance { get; private set; }
        public float PendingReactionDistance => pendingReactionDistance;
        public float DamageImmunitySeconds => damageImmunitySeconds;
        public float HitReactionImmunitySeconds => hitReactionImmunitySeconds;
        public float GetupProtectionSeconds => getupProtectionSeconds;
        public float KnockdownAdditionalLaunchCap => knockdownAdditionalLaunchCap;
        public bool IsKnockdownOrGetup =>
            activeAction == ActiveAction.Knockdown ||
            activeAction == ActiveAction.Getup;

        public event Action<PrototypeInteractionTarget, PrototypeInteractionOutcome>
            AcquisitionCommitted;

        public event Action<PrototypeInteractionTarget> ChairSwingWhooshCommitted;

        public event Action<PrototypeInteractionTarget> QueuedRelayCatchCommitted;

        public event Action<PrototypeInteractionTarget> QueuedRelayCatchCancelled;

        private void OnEnable()
        {
            cargoStrap ??= GetComponent<ShowcaseCargoStrapController>();
            interactionController ??= GetComponent<PrototypeInteractionController>();
            ApplyDamageImmunityToHealth();
            SubscribeToReset();
        }

        private void OnDisable()
        {
            CancelPendingGameplayAction();
            AnimationCue = ShowcasePlayerAnimationCue.None;
            AnimationCueRevision++;
            ClearOwnedMobilityLocks();

            if (resetController != null)
            {
                resetController.ResetPerformed -= HandleResetPerformed;
            }
        }

        private void Update()
        {
            if (IsDead)
            {
                if (logicalInput != null && logicalInput.Snapshot.Restart)
                {
                    resetController?.RequestReset(EncounterResetCause.AutomaticFailureRestart);
                }
                return;
            }

            if (activeAction == ActiveAction.None)
            {
                return;
            }

            if (RequiresPlayerPropRuntime(activeAction) &&
                (physicalLifecycle == null ||
                 !physicalLifecycle.IsPlayerPropRuntimeAvailable))
            {
                Debug.LogError(
                    $"SHOWCASE PLAYER PROP RUNTIME UNAVAILABLE: {activeAction}. " +
                    "The pending action and mobility lock are being cancelled.",
                    this);
                CancelPendingGameplayAction();
                SetCue(ShowcasePlayerAnimationCue.None);
                return;
            }

            if (!animationStateAcknowledged)
            {
                if (activeAction != ActiveAction.Death &&
                    Time.time - actionRequestedAt >= AnimatorStateEntryTimeout &&
                    Time.frameCount - actionRequestedFrame >=
                        MinimumAnimatorStateEntryFrames)
                {
                    ActiveAction timedOut = activeAction;
                    string detail = timedOut == ActiveAction.HitReaction
                        ? "Accepted damage remains committed; hit-reaction " +
                          "presentation was not acknowledged and mobility " +
                          "ownership is being restored."
                        : "Gameplay fact was not committed; ownership is being restored.";
                    Debug.LogError(
                        $"SHOWCASE ANIMATOR STATE ENTRY TIMEOUT: {timedOut}. {detail}",
                        this);
                    CancelTimedOutAction(timedOut);
                }
                return;
            }

            float elapsed = Time.time - actionStartedAt;
            if (activeAction == ActiveAction.Melee &&
                boneCombatRig != null && !boneCombatRig.IsAttackWindowOpen)
            {
                AdvanceMeleeApproach(Time.deltaTime);
                FaceMeleeTarget(Time.deltaTime);
            }
            AdvanceReactionDisplacement(Time.deltaTime);
            // This is quarantine only, never a gameplay-commit fallback. Several
            // approved getup/throw sources are longer than the prototype's old
            // wall-clock action estimates, so the guard must outlive the clip.
            float timeout = Mathf.Max(actionDuration + 2f, 6f);
            if (activeAction != ActiveAction.Death && elapsed >= timeout)
            {
                ActiveAction timedOut = activeAction;
                Debug.LogError(
                    $"SHOWCASE ANIMATION EVENT TIMEOUT: {timedOut}. " +
                    "Gameplay fact was not forced; ownership is being restored.",
                    this);
                CancelTimedOutAction(timedOut);
            }
        }

        public void Configure(
            ShowcaseCombatHealth authoredHealth,
            PrototypeInteractionStateOwner owner,
            InputSystemLogicalInputAdapter input,
            ThirdPersonLocomotion playerLocomotion,
            PrototypePhysicalObjectLifecycle lifecycle,
            PrototypeAllyRelayController relay,
            ShowcaseRunnerPressureController pressure,
            AuthoredEncounterResetController reset,
            ShowcaseObjectiveController objective)
        {
            if (resetController != null)
            {
                resetController.ResetPerformed -= HandleResetPerformed;
            }

            health = authoredHealth;
            interactionOwner = owner;
            logicalInput = input;
            locomotion = playerLocomotion;
            physicalLifecycle = lifecycle;
            allyRelay = relay;
            runnerPressure = pressure;
            resetController = reset;
            objectiveController = objective;
            boneCombatRig ??= GetComponent<ShowcaseBoneCombatRig>();
            cargoStrap ??= GetComponent<ShowcaseCargoStrapController>();
            interactionController ??= GetComponent<PrototypeInteractionController>();
            ApplyDamageImmunityToHealth();
            SubscribeToReset();
            ResetToRunBaseline();
        }

        public void ConfigureHitProtection(
            float authoredDamageImmunitySeconds,
            float authoredHitReactionImmunitySeconds,
            float authoredGetupProtectionSeconds,
            float authoredKnockdownAdditionalLaunchCap)
        {
            damageImmunitySeconds = Mathf.Clamp(
                authoredDamageImmunitySeconds,
                0.05f,
                0.20f);
            hitReactionImmunitySeconds = Mathf.Clamp(
                authoredHitReactionImmunitySeconds,
                0.25f,
                0.40f);
            getupProtectionSeconds = Mathf.Clamp(
                authoredGetupProtectionSeconds,
                0.20f,
                0.60f);
            knockdownAdditionalLaunchCap = Mathf.Clamp(
                authoredKnockdownAdditionalLaunchCap,
                0f,
                0.30f);
            ApplyDamageImmunityToHealth();
        }

        public void ConfigureBoneCombatRig(ShowcaseBoneCombatRig rig)
        {
            boneCombatRig = rig;
        }

        public bool TryRequestMelee(ShowcaseRunnerAgent target)
        {
            return TryRequestMeleeInternal(target, false);
        }

        public bool TryRequestStrappedMelee(ShowcaseRunnerAgent target)
        {
            return TryRequestMeleeInternal(target, true);
        }

        private bool TryRequestMeleeInternal(
            ShowcaseRunnerAgent target,
            bool useStrappedObjectWeapon)
        {
            if (IsDead || interactionOwner == null || logicalInput == null ||
                logicalInput.Snapshot.Aim ||
                interactionOwner.HandState != PrototypeHandOwnershipState.Free ||
                interactionOwner.RelayState != PrototypeRelayReservationState.None ||
                (useStrappedObjectWeapon
                    ? !CanUseStrappedMelee()
                    : interactionOwner.WeaponSelection !=
                        PrototypeWeaponSelection.Fists))
            {
                return false;
            }

            if (activeAction == ActiveAction.Melee)
            {
                if (strappedMeleeActive != useStrappedObjectWeapon)
                {
                    return false;
                }
                if (CanAcceptMeleeFollowUpInput)
                {
                    if (activeMeleeStep == 1 && meleeQueued)
                    {
                        meleeHookQueued = true;
                    }
                    else
                    {
                        meleeQueued = true;
                    }
                    return true;
                }
                return false;
            }

            if (activeAction != ActiveAction.None)
            {
                return false;
            }

            closeRangeDisengageActive = false;
            strappedMeleeActive = useStrappedObjectWeapon;
            StartMelee(1, target);
            return true;
        }

        private bool CanUseStrappedMelee()
        {
            return interactionOwner != null &&
                interactionOwner.WeaponSelection ==
                    PrototypeWeaponSelection.ObjectWeapon &&
                cargoStrap != null && cargoStrap.IsTensioned &&
                !cargoStrap.IsActionLocked &&
                cargoStrap.ActiveTarget != null &&
                cargoStrap.ActiveTarget.WeightClass ==
                    PrototypeWeightClass.Medium &&
                interactionOwner.ObjectWeaponTarget ==
                    cargoStrap.ActiveTarget;
        }

        public bool TryRequestCloseRangeDisengage(ShowcaseRunnerAgent target)
        {
            return TryRequestStrappedMelee(target);
        }

        private bool CanAcceptMeleeFollowUpInput =>
            activeAction == ActiveAction.Melee &&
            !meleeJabReturnActive &&
            activeMeleeStep < 3 &&
            (activeMeleeStep == 1
                ? !meleeHookQueued
                : !meleeQueued && meleeBufferWindowOpen);

        public bool TryBeginAcquisition(
            PrototypeInteractionTarget target,
            PrototypeInteractionOutcome outcome)
        {
            if (target == null || IsDead || activeAction != ActiveAction.None ||
                target.WeightClass == PrototypeWeightClass.Heavy ||
                (outcome != PrototypeInteractionOutcome.Scoop &&
                 outcome != PrototypeInteractionOutcome.Catch))
            {
                return false;
            }

            pendingTarget = target;
            pendingAcquisitionOutcome = outcome;
            activeAction = ActiveAction.Acquire;
            actionStartedAt = Time.time;
            actionDuration = target.WeightClass == PrototypeWeightClass.Light ? 0.82f : 1.05f;
            actionEventCommitted = false;
            meleeBufferWindowOpen = false;
            locomotion?.SetMobilityLock(PlayerMobilityLockReason.PlayerAction, true);

            ShowcasePlayerAnimationCue acquisitionCue;
            if (outcome == PrototypeInteractionOutcome.Scoop)
            {
                acquisitionCue = target.WeightClass == PrototypeWeightClass.Medium
                    ? ShowcasePlayerAnimationCue.PickupLowMedium
                    : ShowcasePlayerAnimationCue.PickupLowSmall;
            }
            else if (target.WeightClass == PrototypeWeightClass.Medium)
            {
                acquisitionCue = ShowcasePlayerAnimationCue.CatchMedium;
            }
            else
            {
                float lateral = transform.InverseTransformPoint(target.transform.position).x;
                if (Mathf.Abs(lateral) > 0.28f)
                {
                    actionDuration = 1.25f;
                }
                acquisitionCue = Mathf.Abs(lateral) <= 0.28f
                    ? ShowcasePlayerAnimationCue.CatchSmallCenter
                    : lateral < 0f
                        ? ShowcasePlayerAnimationCue.CatchSmallLeft
                        : ShowcasePlayerAnimationCue.CatchSmallRight;
            }

            SetCue(acquisitionCue);
            if (physicalLifecycle == null ||
                !physicalLifecycle.PrepareAnimationAcquisition(
                    target,
                    acquisitionCue,
                    outcome))
            {
                Debug.LogError(
                    $"SHOWCASE PROP ACQUISITION PREPARE FAILED: {target.RuntimeInstanceKey} / " +
                    $"{acquisitionCue}.",
                    this);
                CancelPendingGameplayAction();
                return false;
            }
            return true;
        }

        public bool TryBeginDirectThrow(
            PrototypeInteractionTarget target,
            Vector3 direction,
            float charge01)
        {
            if (target == null || IsDead || activeAction != ActiveAction.None ||
                interactionOwner == null ||
                interactionOwner.WeaponSelection !=
                    PrototypeWeaponSelection.ObjectWeapon ||
                target.WeightClass == PrototypeWeightClass.Heavy ||
                physicalLifecycle == null || physicalLifecycle.CarriedTarget != target)
            {
                return false;
            }

            pendingTarget = target;
            pendingThrowDirection = direction;
            pendingThrowUsesAimPoint = false;
            pendingThrowCharge = Mathf.Clamp01(charge01);
            chairSwingWhooshCommitted = false;
            activeAction = ActiveAction.DirectThrow;
            actionStartedAt = Time.time;
            actionDuration = target.WeightClass == PrototypeWeightClass.Light ? 1.25f : 1.35f;
            actionEventCommitted = false;
            meleeBufferWindowOpen = false;
            locomotion?.SetMobilityLock(PlayerMobilityLockReason.PlayerAction, true);
            ShowcasePlayerAnimationCue throwCue =
                target.WeightClass == PrototypeWeightClass.Light
                ? ShowcasePlayerAnimationCue.ThrowSmall
                : ShowcasePlayerAnimationCue.ThrowMedium;
            SetCue(throwCue);
            if (!physicalLifecycle.BeginAnimationThrow(target, throwCue))
            {
                Debug.LogError(
                    $"SHOWCASE PROP THROW PREPARE FAILED: {target.RuntimeInstanceKey} / " +
                    $"{throwCue}.",
                    this);
                CancelPendingGameplayAction();
                return false;
            }
            return true;
        }

        public bool TryBeginDirectThrowAt(
            PrototypeInteractionTarget target,
            Vector3 frozenAimPoint,
            float charge01)
        {
            if (!TryBeginDirectThrow(target, transform.forward, charge01))
            {
                return false;
            }
            pendingThrowAimPoint = frozenAimPoint;
            pendingThrowUsesAimPoint = true;
            FaceThrowAimPoint();
            return true;
        }

        public bool TryBeginQueuedRelayThrow(
            PrototypeInteractionTarget target,
            Vector3 direction,
            float charge01)
        {
            if (target == null || IsDead || activeAction != ActiveAction.None ||
                interactionOwner == null ||
                interactionOwner.WeaponSelection !=
                    PrototypeWeaponSelection.ObjectWeapon ||
                target.WeightClass == PrototypeWeightClass.Heavy ||
                interactionOwner.HandState != PrototypeHandOwnershipState.ThrowCommitted ||
                interactionOwner.ThrowCommittedTarget != target)
            {
                return false;
            }

            pendingTarget = target;
            pendingThrowDirection = direction;
            pendingThrowUsesAimPoint = false;
            pendingThrowCharge = Mathf.Clamp01(charge01);
            chairSwingWhooshCommitted = false;
            activeAction = ActiveAction.QueuedRelayThrow;
            actionStartedAt = Time.time;
            actionDuration = target.WeightClass == PrototypeWeightClass.Light ? 1.25f : 1.35f;
            actionEventCommitted = false;
            meleeBufferWindowOpen = false;
            locomotion?.SetMobilityLock(PlayerMobilityLockReason.PlayerAction, true);
            ShowcasePlayerAnimationCue throwCue =
                target.WeightClass == PrototypeWeightClass.Light
                ? ShowcasePlayerAnimationCue.ThrowSmall
                : ShowcasePlayerAnimationCue.ThrowMedium;
            SetCue(throwCue);
            if (physicalLifecycle == null ||
                !physicalLifecycle.BeginAnimationThrow(target, throwCue))
            {
                Debug.LogError(
                    $"SHOWCASE QUEUED PROP THROW PREPARE FAILED: " +
                    $"{target.RuntimeInstanceKey} / {throwCue}.",
                    this);
                CancelPendingGameplayAction();
                return false;
            }
            QueuedRelayCatchCommitted?.Invoke(target);
            return true;
        }

        public bool TryBeginQueuedRelayThrowAt(
            PrototypeInteractionTarget target,
            Vector3 frozenAimPoint,
            float charge01)
        {
            if (!TryBeginQueuedRelayThrow(target, transform.forward, charge01))
            {
                return false;
            }
            pendingThrowAimPoint = frozenAimPoint;
            pendingThrowUsesAimPoint = true;
            FaceThrowAimPoint();
            return true;
        }

        public void CommitAnimationCombatHit()
        {
            // Retained for old serialized events only. A timed event is not a hit.
            // See NotifyAnimationAttackWindowOpened and TryCommitBoneHit.
        }

        public void NotifyAnimationAttackWindowOpened()
        {
            if (activeAction == ActiveAction.Melee)
            {
                // The attack window may open before ordinary Update steps have
                // consumed the full bounded contact approach. Finish only the
                // remaining authored approach through CharacterController now;
                // the total travel cap remains MaximumMeleeApproachDistance.
                AdvanceMeleeApproach(
                    MaximumMeleeApproachDistance /
                    Mathf.Max(0.01f, MeleeApproachSpeed));
                FaceMeleeTarget(0f);
                actionEventCommitted = true;
            }
        }

        public void NotifyAnimationStateEntered(uint cueRevision)
        {
            if (activeAction == ActiveAction.None || animationStateAcknowledged ||
                cueRevision == 0 || cueRevision != awaitedAnimationCueRevision)
            {
                return;
            }

            animationStateAcknowledged = true;
            actionStartedAt = Time.time;
            if (reactionDisplacementPending)
            {
                reactionDisplacementPending = false;
                reactionDisplacementActive = true;
                reactionElapsed = 0f;
                reactionDistanceApplied = 0f;
            }
        }

        public void SetComboBufferOpen(bool isOpen)
        {
            meleeBufferWindowOpen = isOpen &&
                activeAction == ActiveAction.Melee &&
                !meleeJabReturnActive &&
                activeMeleeStep < 3;
        }

        public void QueueReactionDisplacement(Vector3 direction, float distance, float duration)
        {
            if (externalReactionAllowance == ExternalReactionAllowance.None)
            {
                return;
            }

            float resolvedDistance = Mathf.Max(0f, distance);
            if (externalReactionAllowance ==
                ExternalReactionAllowance.KnockdownCapped)
            {
                if (reactionDisplacementPending || reactionDisplacementActive)
                {
                    SuppressedAdditionalLaunchCount++;
                    externalReactionAllowance = ExternalReactionAllowance.None;
                    return;
                }

                resolvedDistance = Mathf.Min(
                    resolvedDistance,
                    knockdownAdditionalLaunchRemaining);
                knockdownAdditionalLaunchRemaining = Mathf.Max(
                    0f,
                    knockdownAdditionalLaunchRemaining - resolvedDistance);
                AdditionalKnockdownLaunchDistance += resolvedDistance;
            }
            externalReactionAllowance = ExternalReactionAllowance.None;

            pendingReactionDirection = Vector3.ProjectOnPlane(
                direction,
                Vector3.up).normalized;
            pendingReactionDistance = resolvedDistance;
            pendingReactionDuration = Mathf.Max(0.01f, duration);
            reactionDistanceApplied = 0f;
            reactionElapsed = 0f;
            reactionDisplacementPending = pendingReactionDirection.sqrMagnitude > 0.001f &&
                pendingReactionDistance > 0f;
            reactionDisplacementActive = false;
        }

        internal bool TryCommitBoneHit(
            ShowcaseRunnerAgent actualVictim,
            uint expectedAttackSequence,
            Vector3 worldPoint)
        {
            if (activeAction != ActiveAction.Melee || actualVictim == null ||
                actualVictim.IsDead || !actionEventCommitted ||
                expectedAttackSequence == 0 ||
                expectedAttackSequence != activeAttackSequence)
            {
                return false;
            }

            bool accepted = runnerPressure != null &&
                runnerPressure.CommitPlayerMeleeHitFromBoneContact(
                    actualVictim,
                    activeMeleeStep,
                    activeAttackSequence,
                    MeleeDamage[activeMeleeStep],
                    worldPoint);
            return accepted;
        }

        public void CommitAnimationCarryContact()
        {
            TryCommitAnimationCarryContact();
        }

        private bool TryCommitAnimationCarryContact()
        {
            if (activeAction != ActiveAction.Acquire || actionEventCommitted ||
                pendingTarget == null)
            {
                return actionEventCommitted;
            }
            bool ownershipCompleted = interactionOwner != null &&
                interactionOwner.TryCompleteAcquisition(pendingTarget);
            bool carried = ownershipCompleted && physicalLifecycle != null &&
                physicalLifecycle.BeginCarry(pendingTarget);
            bool catchFailed = false;
            if (pendingAcquisitionOutcome == PrototypeInteractionOutcome.Catch)
            {
                bool relayCompleted = carried && allyRelay != null &&
                    allyRelay.CompleteCatch(pendingTarget, false);
                if (!relayCompleted)
                {
                    carried = false;
                    catchFailed = true;
                }
            }

            if (!carried)
            {
                interactionOwner?.CancelAcquisition(pendingTarget);
                interactionOwner?.CancelCarry(pendingTarget);
                physicalLifecycle?.CancelPlayerPropAction(pendingTarget, false);
                if (catchFailed)
                {
                    allyRelay?.ResolveCatchFailure(pendingTarget);
                }
            }
            actionEventCommitted = carried;
            if (carried)
            {
                AcquisitionCommitted?.Invoke(
                    pendingTarget,
                    pendingAcquisitionOutcome);
            }
            return carried;
        }

        public bool CommitAnimationChairSwingWhoosh()
        {
            if (chairSwingWhooshCommitted || pendingTarget == null ||
                (activeAction != ActiveAction.DirectThrow &&
                 activeAction != ActiveAction.QueuedRelayThrow) ||
                (pendingTarget.Role != PrototypeInteractionRole.DirectChair &&
                 pendingTarget.Role != PrototypeInteractionRole.AllyChair))
            {
                return false;
            }

            chairSwingWhooshCommitted = true;
            ChairSwingWhooshCommitted?.Invoke(pendingTarget);
            return true;
        }

        public void CommitAnimationThrowRelease()
        {
            if (actionEventCommitted || pendingTarget == null)
            {
                return;
            }

            bool released;
            if (activeAction == ActiveAction.DirectThrow)
            {
                released = physicalLifecycle != null &&
                    (pendingThrowUsesAimPoint
                        ? physicalLifecycle.TryThrowAt(
                            pendingThrowAimPoint,
                            pendingThrowCharge)
                        : physicalLifecycle.TryThrow(
                            pendingThrowDirection,
                            pendingThrowCharge));
            }
            else if (activeAction == ActiveAction.QueuedRelayThrow)
            {
                released = physicalLifecycle != null &&
                    (pendingThrowUsesAimPoint
                        ? physicalLifecycle.TryThrowCommittedRelayTargetAt(
                            pendingTarget,
                            pendingThrowAimPoint,
                            pendingThrowCharge)
                        : physicalLifecycle.TryThrowCommittedRelayTarget(
                            pendingTarget,
                            pendingThrowDirection,
                            pendingThrowCharge));
                if (released)
                {
                    bool relayCompleted = allyRelay != null &&
                        allyRelay.CompleteCatch(pendingTarget, true);
                    if (!relayCompleted)
                    {
                        Debug.LogError(
                            $"SHOWCASE QUEUED THROW PHYSICALLY RELEASED BUT RELAY " +
                            $"FINALIZATION FAILED: {pendingTarget.RuntimeInstanceKey}.",
                            this);
                        interactionOwner?.TryCancelRelayReservation(pendingTarget);
                        allyRelay?.ResolveCatchFailure(pendingTarget);
                    }
                }
            }
            else
            {
                return;
            }

            actionEventCommitted = released;
            if (!released)
            {
                Debug.LogError(
                    $"SHOWCASE ANIMATION THROW RELEASE FAILED: {activeAction} / " +
                    $"{pendingTarget.RuntimeInstanceKey}. Gameplay release was not committed.",
                    this);
                CancelPendingGameplayAction();
            }
        }

        public void CompleteAnimationAction()
        {
            if (activeAction == ActiveAction.None || activeAction == ActiveAction.Death)
            {
                return;
            }

            if (activeAction == ActiveAction.Melee)
            {
                // AnimationEvent ordering can place ActionComplete before the
                // closing-window event in the same rendered frame. Capture the
                // final authored strike segment now, then resolve it before the
                // combo advances so Jab can never be re-labelled as Cross.
                boneCombatRig?.CaptureAndResolvePendingContacts();
            }

            // A severe frame hitch can cross both authored pickup events in one
            // Animator evaluation and deliver ActionComplete without the earlier
            // CarryContact callback. ActionComplete is still proof that the exact
            // acquisition clip reached its terminal authored frame, so perform one
            // bounded late contact settlement before treating the transaction as
            // corrupt. Throw and combat actions deliberately keep their stricter
            // event-only authority.
            if (activeAction == ActiveAction.Acquire &&
                !actionEventCommitted)
            {
                TryCommitAnimationCarryContact();
            }

            if (RequiresGameplayCommit(activeAction) && !actionEventCommitted)
            {
                Debug.LogError(
                    $"SHOWCASE ANIMATION COMPLETED WITHOUT GAMEPLAY EVENT: {activeAction}.",
                    this);
                CancelTimedOutAction(activeAction);
                return;
            }

            if (activeAction == ActiveAction.Melee && meleeJabReturnActive)
            {
                meleeJabReturnActive = false;
                CompleteActiveAction();
                return;
            }

            if (activeAction == ActiveAction.Melee && meleeQueued && activeMeleeStep < 3)
            {
                int completedStep = activeMeleeStep;
                bool carryRapidFollowUp = completedStep == 1 && meleeHookQueued;
                ShowcaseRunnerAgent nextTarget = meleeTarget;
                if (nextTarget == null || nextTarget.IsDead)
                {
                    nextTarget = ResolveReticleMeleeTarget();
                }
                bool finisherReady = ShouldUseMeleeFinisher(nextTarget);
                int nextStep = finisherReady
                    ? 3
                    : completedStep == 1
                        ? 2
                        : 1;
                meleeQueued = false;
                meleeHookQueued = false;
                StartMelee(nextStep, nextTarget ?? meleeTarget);
                if (carryRapidFollowUp && nextStep == 2)
                {
                    meleeQueued = true;
                }
                return;
            }

            if (activeAction == ActiveAction.Melee && activeMeleeStep == 1)
            {
                BeginMeleeJabReturn();
                return;
            }

            if (activeAction == ActiveAction.Knockdown)
            {
                activeAction = ActiveAction.Getup;
                actionStartedAt = Time.time;
                actionDuration = 1.15f;
                getupProtectionUntil = Mathf.Max(
                    getupProtectionUntil,
                    Time.time + actionDuration +
                    Mathf.Clamp(getupProtectionSeconds, 0.20f, 0.60f));
                actionEventCommitted = false;
                SetCue(getupUsesBackVariant
                    ? ShowcasePlayerAnimationCue.GetupBack
                    : ShowcasePlayerAnimationCue.GetupFront);
                return;
            }

            CompleteActiveAction();
        }

        public bool ReceiveDamage(ShowcaseDamageRequest request)
        {
            return ReceiveDamage(request, Time.time, Time.frameCount);
        }

        public bool ReceiveDamage(
            ShowcaseDamageRequest request,
            float timestamp,
            int frameCount)
        {
            if (health == null || health.IsDead)
            {
                return false;
            }

            if (frameCount == lastAcceptedDamageFrame)
            {
                SameFrameDamageDedupCount++;
                return false;
            }
            if (!health.TryApplyDamage(request, timestamp))
            {
                return false;
            }

            lastAcceptedDamageFrame = frameCount;
            externalReactionAllowance = ExternalReactionAllowance.None;

            if (cargoStrap != null &&
                (cargoStrap.IsActionLocked || cargoStrap.IsAttached))
            {
                cargoStrap.CancelForPlayerInterruption();
            }

            if (health.IsDead)
            {
                // Lethal damage always outranks stagger/get-up protection and
                // cannot be overwritten by a later contact in the same frame.
                CancelPendingGameplayAction();
                ClearReactionDisplacement();
                activeAction = ActiveAction.Death;
                ClearOwnedMobilityLocks();
                locomotion?.ClearExternalPlanarImpulse();
                locomotion?.SetMobilityLock(PlayerMobilityLockReason.Death, true);
                interactionOwner?.TryBeginFailurePending();
                objectiveController?.NotifyPlayerDeath();
                SetCue(health.AcceptedDamageCount % 2 == 0
                    ? ShowcasePlayerAnimationCue.Death2
                    : ShowcasePlayerAnimationCue.Death1);
                return true;
            }

            bool knockdownActive = activeAction == ActiveAction.Knockdown;
            bool getupActive = activeAction == ActiveAction.Getup;
            bool staggerProtected = knockdownActive || getupActive ||
                timestamp < staggerProtectionUntil ||
                timestamp < getupProtectionUntil;
            if (staggerProtected)
            {
                ProtectedStaggerCount++;
                externalReactionAllowance = knockdownActive
                    ? ExternalReactionAllowance.KnockdownCapped
                    : ExternalReactionAllowance.None;
                return true;
            }

            CancelPendingGameplayAction();
            ClearReactionDisplacement();
            staggerProtectionUntil = timestamp +
                Mathf.Clamp(hitReactionImmunitySeconds, 0.25f, 0.40f);
            if (request.Reaction == ShowcaseHitReaction.Knockdown)
            {
                Vector3 direction = Vector3.ProjectOnPlane(
                    request.ImpactDirection,
                    Vector3.up).normalized;
                getupUsesBackVariant = Vector3.Dot(transform.forward, direction) < 0f;
                activeAction = ActiveAction.Knockdown;
                knockdownAdditionalLaunchRemaining = Mathf.Clamp(
                    knockdownAdditionalLaunchCap,
                    0f,
                    0.30f);
                externalReactionAllowance = ExternalReactionAllowance.Full;
                actionStartedAt = Time.time;
                actionDuration = 1.20f;
                locomotion?.SetMobilityLock(PlayerMobilityLockReason.Knockdown, true);
                SetCue(getupUsesBackVariant
                    ? ShowcasePlayerAnimationCue.KnockdownBack
                    : ShowcasePlayerAnimationCue.KnockdownFront);
            }
            else
            {
                activeAction = ActiveAction.HitReaction;
                knockdownAdditionalLaunchRemaining = 0f;
                externalReactionAllowance = ExternalReactionAllowance.Full;
                actionStartedAt = Time.time;
                actionDuration = 0.68f;
                locomotion?.SetMobilityLock(PlayerMobilityLockReason.HitReaction, true);
                SetCue(ShowcasePlayerAnimationCue.HitBody);
            }
            return true;
        }

        public void ResetToRunBaseline()
        {
            physicalLifecycle?.ResetPlayerPropAction();
            health?.ResetToBaseline();
            activeAction = ActiveAction.None;
            meleeTarget = null;
            meleeApproachDistance = 0f;
            meleeFacingDirection = Vector3.zero;
            pendingTarget = null;
            pendingAcquisitionOutcome = PrototypeInteractionOutcome.None;
            pendingThrowDirection = Vector3.zero;
            pendingThrowAimPoint = Vector3.zero;
            pendingThrowUsesAimPoint = false;
            pendingThrowCharge = 0f;
            actionStartedAt = 0f;
            actionDuration = 0f;
            actionEventCommitted = false;
            meleeQueued = false;
            meleeHookQueued = false;
            meleeJabReturnActive = false;
            meleeBufferWindowOpen = false;
            activeMeleeStep = 0;
            chairSwingWhooshCommitted = false;
            closeRangeDisengageActive = false;
            strappedMeleeActive = false;
            lastMeleeCompletedAt = float.NegativeInfinity;
            nextAttackSequence = 0;
            activeAttackSequence = 0;
            CloseRangeDisengageCount = 0;
            CloseRangeDisengageHitCount = 0;
            lastAcceptedDamageFrame = -1;
            staggerProtectionUntil = float.NegativeInfinity;
            getupProtectionUntil = float.NegativeInfinity;
            knockdownAdditionalLaunchRemaining = 0f;
            externalReactionAllowance = ExternalReactionAllowance.None;
            SameFrameDamageDedupCount = 0;
            ProtectedStaggerCount = 0;
            SuppressedAdditionalLaunchCount = 0;
            AdditionalKnockdownLaunchDistance = 0f;
            getupUsesBackVariant = false;
            actionRequestedAt = 0f;
            actionRequestedFrame = -1;
            animationStateAcknowledged = false;
            awaitedAnimationCueRevision = 0;
            ClearReactionDisplacement();
            AnimationCue = ShowcasePlayerAnimationCue.None;
            AnimationCueRevision++;
            boneCombatRig?.ResetRuntimeState();
            ClearOwnedMobilityLocks();
        }

        private void StartMelee(int step, ShowcaseRunnerAgent target)
        {
            boneCombatRig?.CloseAttackWindow();
            meleeJabReturnActive = false;
            activeMeleeStep = Mathf.Clamp(step, 1, 3);
            if (activeMeleeStep == 1)
            {
                meleeQueued = false;
                meleeHookQueued = false;
            }
            meleeFacingDirection = ResolveMeleeFacingDirection(target);
            ApplyMeleeFacing(meleeFacingDirection, true);
            if (target == null && runnerPressure != null)
            {
                target = ResolveReticleMeleeTarget();
                if (target != null)
                {
                    meleeFacingDirection = ResolveMeleeFacingDirection(target);
                    ApplyMeleeFacing(meleeFacingDirection, true);
                }
            }
            meleeTarget = target;
            meleeApproachDistance = 0f;
            activeAction = ActiveAction.Melee;
            actionStartedAt = Time.time;
            actionDuration = MeleeDurations[activeMeleeStep];
            actionEventCommitted = false;
            meleeBufferWindowOpen = false;
            activeAttackSequence = ++nextAttackSequence;
            // Keep planar locomotion available. The authored full-body strike
            // stays on the Base layer; a legs-only locomotion layer is blended
            // only while the player is actually moving.
            locomotion?.SetJumpLock(PlayerMobilityLockReason.MeleeAction, true);
            UpdateMeleeFacingOverride();
            SetCue(activeMeleeStep switch
            {
                1 => ShowcasePlayerAnimationCue.MeleeJab,
                2 => ShowcasePlayerAnimationCue.MeleeCross,
                _ => ShowcasePlayerAnimationCue.MeleeHook
            });
        }

        private void BeginMeleeJabReturn()
        {
            boneCombatRig?.CloseAttackWindow();
            meleeJabReturnActive = true;
            meleeBufferWindowOpen = false;
            actionStartedAt = Time.time;
            actionDuration = MeleeDurations[1];
            SetCue(ShowcasePlayerAnimationCue.MeleeJabReturn);
        }

        private void FaceMeleeTarget(float deltaTime)
        {
            Vector3 direction = meleeTarget != null
                ? Vector3.ProjectOnPlane(
                    meleeTarget.transform.position - transform.position,
                    Vector3.up)
                : meleeFacingDirection;
            if (direction.sqrMagnitude <= 0.001f)
            {
                locomotion?.SetFacingOverride(
                    PlayerMobilityLockReason.MeleeAction,
                    Vector3.zero,
                    false);
                return;
            }

            meleeFacingDirection = direction.normalized;
            ApplyMeleeFacing(meleeFacingDirection, false);

            if (deltaTime <= 0f)
            {
                return;
            }

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(direction.normalized, Vector3.up),
                360f * deltaTime);
        }

        private Vector3 ResolveMeleeFacingDirection(ShowcaseRunnerAgent target)
        {
            Vector3 direction = target != null
                ? Vector3.ProjectOnPlane(
                    target.transform.position - transform.position,
                    Vector3.up)
                : Vector3.zero;
            if (direction.sqrMagnitude <= 0.001f)
            {
                interactionController ??= GetComponent<PrototypeInteractionController>();
                if (interactionController != null)
                {
                    direction = Vector3.ProjectOnPlane(
                        interactionController.ResolveReticleAimPoint() -
                            transform.position,
                        Vector3.up);
                }
            }
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            }
            return direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : Vector3.forward;
        }

        private ShowcaseRunnerAgent ResolveReticleMeleeTarget()
        {
            if (runnerPressure == null)
            {
                return null;
            }

            interactionController ??=
                GetComponent<PrototypeInteractionController>();
            return interactionController != null &&
                   interactionController.TryGetReticleRay(out Ray reticleRay)
                ? runnerPressure.ResolvePlayerMeleeTargetForReticle(reticleRay)
                : runnerPressure.ResolvePlayerMeleeTargetForCombo();
        }

        private void ApplyMeleeFacing(Vector3 direction, bool snapImmediately)
        {
            Vector3 planar = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planar.sqrMagnitude <= 0.001f)
            {
                return;
            }

            planar.Normalize();
            locomotion?.SetFacingOverride(
                PlayerMobilityLockReason.MeleeAction,
                planar,
                true);
            if (snapImmediately)
            {
                transform.rotation = Quaternion.LookRotation(planar, Vector3.up);
            }
        }

        private void UpdateMeleeFacingOverride()
        {
            FaceMeleeTarget(0f);
        }

        private static bool ShouldUseMeleeFinisher(ShowcaseRunnerAgent target)
        {
            return target != null && !target.IsDead && target.Health != null &&
                target.Health.CurrentHealth <= MeleeDamage[3];
        }

        private void AdvanceMeleeApproach(float deltaTime)
        {
            if (meleeTarget == null || locomotion == null || deltaTime <= 0f ||
                meleeApproachDistance >= MaximumMeleeApproachDistance)
            {
                return;
            }

            CharacterController controller = locomotion.GetComponent<CharacterController>();
            if (controller == null)
            {
                return;
            }

            Vector3 offset = Vector3.ProjectOnPlane(
                meleeTarget.transform.position - transform.position,
                Vector3.up);
            float distance = offset.magnitude;
            float needed = distance - MeleeContactStandoff;
            if (needed <= 0.001f || offset.sqrMagnitude <= 0.001f)
            {
                return;
            }

            float step = Mathf.Min(
                needed,
                MaximumMeleeApproachDistance - meleeApproachDistance,
                MeleeApproachSpeed * deltaTime);
            Vector3 before = transform.position;
            controller.Move(offset.normalized * step);
            meleeApproachDistance += Vector3.ProjectOnPlane(
                transform.position - before,
                Vector3.up).magnitude;
        }

        private void FaceThrowAimPoint()
        {
            Vector3 direction = Vector3.ProjectOnPlane(
                pendingThrowAimPoint - transform.position,
                Vector3.up);
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up);
            }
        }

        private void CompleteActiveAction()
        {
            ActiveAction completed = activeAction;
            PrototypeInteractionTarget completedTarget = pendingTarget;
            if (completed == ActiveAction.Getup)
            {
                getupProtectionUntil = Mathf.Max(
                    getupProtectionUntil,
                    Time.time + Mathf.Clamp(
                        getupProtectionSeconds,
                        0.20f,
                        0.60f));
            }
            if (completed == ActiveAction.Acquire && completedTarget != null &&
                physicalLifecycle != null &&
                !physicalLifecycle.CompleteAnimationAcquisition(completedTarget))
            {
                Debug.LogError(
                    $"SHOWCASE PROP ACQUISITION COMPLETION FAILED: " +
                    $"{completedTarget.RuntimeInstanceKey}. Falling back to stable carry.",
                    this);
                physicalLifecycle.CancelPlayerPropAction(completedTarget, true);
            }
            boneCombatRig?.CloseAttackWindow();
            activeAction = ActiveAction.None;
            if (completed == ActiveAction.Melee)
            {
                lastMeleeCompletedAt = Time.time;
                activeMeleeStep = 0;
                meleeQueued = false;
                meleeHookQueued = false;
                meleeJabReturnActive = false;
                closeRangeDisengageActive = false;
                strappedMeleeActive = false;
            }
            ReleaseMobilityLockFor(completed);
            meleeTarget = null;
            meleeApproachDistance = 0f;
            meleeFacingDirection = Vector3.zero;
            pendingTarget = null;
            pendingAcquisitionOutcome = PrototypeInteractionOutcome.None;
            pendingThrowDirection = Vector3.zero;
            pendingThrowAimPoint = Vector3.zero;
            pendingThrowUsesAimPoint = false;
            pendingThrowCharge = 0f;
            chairSwingWhooshCommitted = false;
            actionEventCommitted = false;
            animationStateAcknowledged = false;
            awaitedAnimationCueRevision = 0;
            meleeBufferWindowOpen = false;
            if (completed != ActiveAction.HitReaction &&
                completed != ActiveAction.Knockdown &&
                completed != ActiveAction.Getup)
            {
                ClearReactionDisplacement();
            }
        }

        private void CancelPendingGameplayAction()
        {
            ActiveAction cancelled = activeAction;
            boneCombatRig?.CloseAttackWindow();
            if (activeAction == ActiveAction.Acquire && pendingTarget != null)
            {
                bool acquisitionCancelled =
                    interactionOwner?.CancelAcquisition(pendingTarget) == true;
                bool carryCancelled =
                    interactionOwner?.CancelCarry(pendingTarget) == true;
                bool remainsCarried = !acquisitionCancelled && !carryCancelled &&
                    interactionOwner != null &&
                    interactionOwner.HandState == PrototypeHandOwnershipState.Carrying &&
                    interactionOwner.CarriedTarget == pendingTarget;
                physicalLifecycle?.CancelPlayerPropAction(
                    pendingTarget,
                    remainsCarried);
                if (pendingAcquisitionOutcome == PrototypeInteractionOutcome.Catch)
                {
                    allyRelay?.ResolveCatchFailure(pendingTarget);
                }
            }
            else if (activeAction == ActiveAction.DirectThrow && pendingTarget != null)
            {
                bool remainsCarried = interactionOwner != null &&
                    interactionOwner.HandState == PrototypeHandOwnershipState.Carrying &&
                    interactionOwner.CarriedTarget == pendingTarget &&
                    physicalLifecycle != null &&
                    physicalLifecycle.CarriedTarget == pendingTarget;
                physicalLifecycle?.CancelPlayerPropAction(
                    pendingTarget,
                    remainsCarried);
            }
            else if (activeAction == ActiveAction.QueuedRelayThrow && pendingTarget != null)
            {
                QueuedRelayCatchCancelled?.Invoke(pendingTarget);
                interactionOwner?.CompleteThrow(pendingTarget);
                allyRelay?.ResolveCatchFailure(pendingTarget);
                physicalLifecycle?.CancelPlayerPropAction(pendingTarget, false);
            }
            activeAction = ActiveAction.None;
            pendingTarget = null;
            pendingThrowDirection = Vector3.zero;
            pendingThrowAimPoint = Vector3.zero;
            pendingThrowUsesAimPoint = false;
            pendingThrowCharge = 0f;
            meleeTarget = null;
            meleeApproachDistance = 0f;
            meleeFacingDirection = Vector3.zero;
            meleeQueued = false;
            meleeHookQueued = false;
            meleeJabReturnActive = false;
            closeRangeDisengageActive = false;
            strappedMeleeActive = false;
            meleeBufferWindowOpen = false;
            actionEventCommitted = false;
            animationStateAcknowledged = false;
            awaitedAnimationCueRevision = 0;
            ClearReactionDisplacement();
            ReleaseMobilityLockFor(cancelled);
        }

        private void CancelTimedOutAction(ActiveAction timedOut)
        {
            CancelPendingGameplayAction();
            if (timedOut == ActiveAction.HitReaction ||
                timedOut == ActiveAction.Knockdown ||
                timedOut == ActiveAction.Getup)
            {
                ReleaseMobilityLockFor(timedOut);
            }
        }

        private static bool RequiresGameplayCommit(ActiveAction action)
        {
            return action == ActiveAction.Melee ||
                action == ActiveAction.Acquire ||
                action == ActiveAction.DirectThrow ||
                action == ActiveAction.QueuedRelayThrow;
        }

        private static bool RequiresPlayerPropRuntime(ActiveAction action)
        {
            return action == ActiveAction.Acquire ||
                action == ActiveAction.DirectThrow ||
                action == ActiveAction.QueuedRelayThrow;
        }

        private void ReleaseMobilityLockFor(ActiveAction action)
        {
            switch (action)
            {
                case ActiveAction.Melee:
                    locomotion?.SetJumpLock(
                        PlayerMobilityLockReason.MeleeAction,
                        false);
                    locomotion?.SetFacingOverride(
                        PlayerMobilityLockReason.MeleeAction,
                        Vector3.zero,
                        false);
                    break;
                case ActiveAction.Acquire:
                case ActiveAction.DirectThrow:
                case ActiveAction.QueuedRelayThrow:
                    locomotion?.SetMobilityLock(
                        PlayerMobilityLockReason.PlayerAction,
                        false);
                    break;
                case ActiveAction.HitReaction:
                    locomotion?.SetMobilityLock(
                        PlayerMobilityLockReason.HitReaction,
                        false);
                    break;
                case ActiveAction.Knockdown:
                case ActiveAction.Getup:
                    locomotion?.SetMobilityLock(
                        PlayerMobilityLockReason.Knockdown,
                        false);
                    break;
                case ActiveAction.Death:
                    locomotion?.SetMobilityLock(PlayerMobilityLockReason.Death, false);
                    break;
            }
        }

        private void ClearOwnedMobilityLocks()
        {
            foreach (PlayerMobilityLockReason reason in new[]
            {
                PlayerMobilityLockReason.PlayerAction,
                PlayerMobilityLockReason.HitReaction,
                PlayerMobilityLockReason.Knockdown,
                PlayerMobilityLockReason.Death,
                PlayerMobilityLockReason.MeleeAction
            })
            {
                locomotion?.SetMobilityLock(reason, false);
            }
            locomotion?.SetJumpLock(PlayerMobilityLockReason.PlayerAction, false);
            locomotion?.SetJumpLock(PlayerMobilityLockReason.MeleeAction, false);
            locomotion?.SetFacingOverride(
                PlayerMobilityLockReason.PlayerAction,
                Vector3.zero,
                false);
            locomotion?.SetFacingOverride(
                PlayerMobilityLockReason.MeleeAction,
                Vector3.zero,
                false);
        }

        private void SetCue(ShowcasePlayerAnimationCue cue)
        {
            AnimationCue = cue;
            AnimationCueRevision++;
            awaitedAnimationCueRevision = AnimationCueRevision;
            actionRequestedAt = Time.time;
            actionRequestedFrame = Time.frameCount;
            animationStateAcknowledged = false;
        }

        private void AdvanceReactionDisplacement(float deltaTime)
        {
            if (!reactionDisplacementActive || locomotion == null)
            {
                return;
            }

            CharacterController controller = locomotion.GetComponent<CharacterController>();
            if (controller == null)
            {
                ClearReactionDisplacement();
                return;
            }

            reactionElapsed = Mathf.Min(pendingReactionDuration, reactionElapsed + deltaTime);
            float targetDistance = pendingReactionDistance *
                (reactionElapsed / pendingReactionDuration);
            float stepDistance = Mathf.Max(0f, targetDistance - reactionDistanceApplied);
            if (stepDistance > 0f)
            {
                CollisionFlags flags = controller.Move(
                    pendingReactionDirection * stepDistance);
                reactionDistanceApplied += stepDistance;
                if ((flags & CollisionFlags.Sides) != 0)
                {
                    reactionDisplacementActive = false;
                }
            }

            if (reactionElapsed >= pendingReactionDuration)
            {
                reactionDisplacementActive = false;
            }
        }

        private void ClearReactionDisplacement()
        {
            pendingReactionDirection = Vector3.zero;
            pendingReactionDistance = 0f;
            pendingReactionDuration = 0f;
            reactionDistanceApplied = 0f;
            reactionElapsed = 0f;
            reactionDisplacementPending = false;
            reactionDisplacementActive = false;
            externalReactionAllowance = ExternalReactionAllowance.None;
        }

        private void ApplyDamageImmunityToHealth()
        {
            health?.SetInvulnerabilitySeconds(Mathf.Clamp(
                damageImmunitySeconds,
                0.05f,
                0.20f));
        }

        private void SubscribeToReset()
        {
            if (resetController == null)
            {
                return;
            }
            resetController.ResetPerformed -= HandleResetPerformed;
            resetController.ResetPerformed += HandleResetPerformed;
        }

        private void HandleResetPerformed(EncounterResetCause cause)
        {
            ResetToRunBaseline();
        }
    }
}
