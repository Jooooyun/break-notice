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
        ThrowSmall,
        ThrowMedium,
        HitBody,
        KnockdownBack,
        KnockdownFront,
        GetupBack,
        GetupFront,
        Death1,
        Death2
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

        private static readonly float[] MeleeDurations = { 0f, 1.00f, 1.05f, 1.15f };
        private static readonly int[] MeleeDamage = { 0, 15, 20, 35 };
        private const float AnimatorStateEntryTimeout = 0.15f;
        private const float MaximumMeleeApproachDistance = 0.25f;
        private const float MeleeContactStandoff = 0.67f;
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
        private int activeMeleeStep;
        private float lastMeleeCompletedAt = float.NegativeInfinity;
        private uint nextAttackSequence;
        private uint activeAttackSequence;
        private bool getupUsesBackVariant;
        private float actionRequestedAt;
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

        public ShowcaseCombatHealth Health => health;
        public ShowcasePlayerAnimationCue AnimationCue { get; private set; }
        public uint AnimationCueRevision { get; private set; }
        public bool IsDead => health != null && health.IsDead;
        public bool IsActionLocked => activeAction != ActiveAction.None;
        public bool BlocksInteraction => IsDead || IsActionLocked;
        public bool AllowsAim => !IsDead && activeAction == ActiveAction.None;
        public bool CanBufferMelee => activeAction == ActiveAction.Melee &&
            activeMeleeStep < 3 && !meleeQueued;
        public int ActiveMeleeStep => activeMeleeStep;
        public bool HasBufferedMelee => meleeQueued;
        public uint ActiveAttackSequence => activeAttackSequence;
        public bool IsMeleeActionActive => activeAction == ActiveAction.Melee;
        public ShowcaseRunnerAgent ActiveMeleeTarget => meleeTarget;
        public float MeleeApproachDistance => meleeApproachDistance;

        private void OnEnable()
        {
            SubscribeToReset();
        }

        private void OnDisable()
        {
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

            if (!animationStateAcknowledged)
            {
                if (activeAction != ActiveAction.Death &&
                    Time.time - actionRequestedAt >= AnimatorStateEntryTimeout)
                {
                    ActiveAction timedOut = activeAction;
                    Debug.LogError(
                        $"SHOWCASE ANIMATOR STATE ENTRY TIMEOUT: {timedOut}. " +
                        "Gameplay fact was not committed; ownership is being restored.",
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
            SubscribeToReset();
            ResetToRunBaseline();
        }

        public void ConfigureBoneCombatRig(ShowcaseBoneCombatRig rig)
        {
            boneCombatRig = rig;
        }

        public bool TryRequestMelee(ShowcaseRunnerAgent target)
        {
            if (IsDead || interactionOwner == null || logicalInput == null ||
                logicalInput.Snapshot.Aim ||
                interactionOwner.HandState != PrototypeHandOwnershipState.Free ||
                interactionOwner.StrapState != PrototypeStrapOwnershipState.Stored ||
                interactionOwner.RelayState != PrototypeRelayReservationState.None)
            {
                return false;
            }

            if (activeAction == ActiveAction.Melee)
            {
                if (!meleeQueued && activeMeleeStep < 3)
                {
                    meleeQueued = true;
                    return true;
                }
                return false;
            }

            if (activeAction != ActiveAction.None)
            {
                return false;
            }

            StartMelee(1, target);
            return true;
        }

        public bool TryBeginAcquisition(
            PrototypeInteractionTarget target,
            PrototypeInteractionOutcome outcome)
        {
            if (target == null || IsDead || activeAction != ActiveAction.None ||
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

            if (target.WeightClass == PrototypeWeightClass.Medium)
            {
                SetCue(ShowcasePlayerAnimationCue.CatchMedium);
            }
            else if (outcome == PrototypeInteractionOutcome.Scoop)
            {
                // A dedicated low pickup source is still pending. Do not misuse the
                // directional airborne catch clips for a grounded scoop fallback.
                SetCue(ShowcasePlayerAnimationCue.CatchSmallCenter);
            }
            else
            {
                float lateral = transform.InverseTransformPoint(target.transform.position).x;
                if (Mathf.Abs(lateral) > 0.28f)
                {
                    actionDuration = 1.25f;
                }
                SetCue(Mathf.Abs(lateral) <= 0.28f
                    ? ShowcasePlayerAnimationCue.CatchSmallCenter
                    : lateral < 0f
                        ? ShowcasePlayerAnimationCue.CatchSmallLeft
                        : ShowcasePlayerAnimationCue.CatchSmallRight);
            }
            return true;
        }

        public bool TryBeginDirectThrow(
            PrototypeInteractionTarget target,
            Vector3 direction,
            float charge01)
        {
            if (target == null || IsDead || activeAction != ActiveAction.None ||
                physicalLifecycle == null || physicalLifecycle.CarriedTarget != target)
            {
                return false;
            }

            pendingTarget = target;
            pendingThrowDirection = direction;
            pendingThrowUsesAimPoint = false;
            pendingThrowCharge = Mathf.Clamp01(charge01);
            activeAction = ActiveAction.DirectThrow;
            actionStartedAt = Time.time;
            actionDuration = target.WeightClass == PrototypeWeightClass.Light ? 1.25f : 1.35f;
            actionEventCommitted = false;
            meleeBufferWindowOpen = false;
            locomotion?.SetMobilityLock(PlayerMobilityLockReason.PlayerAction, true);
            SetCue(target.WeightClass == PrototypeWeightClass.Light
                ? ShowcasePlayerAnimationCue.ThrowSmall
                : ShowcasePlayerAnimationCue.ThrowMedium);
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
                interactionOwner.HandState != PrototypeHandOwnershipState.ThrowCommitted ||
                interactionOwner.ThrowCommittedTarget != target)
            {
                return false;
            }

            pendingTarget = target;
            pendingThrowDirection = direction;
            pendingThrowUsesAimPoint = false;
            pendingThrowCharge = Mathf.Clamp01(charge01);
            activeAction = ActiveAction.QueuedRelayThrow;
            actionStartedAt = Time.time;
            actionDuration = target.WeightClass == PrototypeWeightClass.Light ? 1.25f : 1.35f;
            actionEventCommitted = false;
            meleeBufferWindowOpen = false;
            locomotion?.SetMobilityLock(PlayerMobilityLockReason.PlayerAction, true);
            SetCue(target.WeightClass == PrototypeWeightClass.Light
                ? ShowcasePlayerAnimationCue.ThrowSmall
                : ShowcasePlayerAnimationCue.ThrowMedium);
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
                activeMeleeStep < 3;
        }

        public void QueueReactionDisplacement(Vector3 direction, float distance, float duration)
        {
            pendingReactionDirection = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
            pendingReactionDistance = Mathf.Max(0f, distance);
            pendingReactionDuration = Mathf.Max(0.01f, duration);
            reactionDistanceApplied = 0f;
            reactionElapsed = 0f;
            reactionDisplacementPending = pendingReactionDirection.sqrMagnitude > 0.001f &&
                pendingReactionDistance > 0f;
            reactionDisplacementActive = false;
        }

        internal bool TryCommitBoneHit(
            ShowcaseRunnerAgent actualVictim,
            uint expectedAttackSequence)
        {
            if (activeAction != ActiveAction.Melee || actualVictim == null ||
                actualVictim.IsDead || !actionEventCommitted ||
                expectedAttackSequence == 0 ||
                expectedAttackSequence != activeAttackSequence)
            {
                return false;
            }

            return runnerPressure != null &&
                runnerPressure.CommitPlayerMeleeHitFromBoneContact(
                    actualVictim,
                    activeMeleeStep,
                    activeAttackSequence,
                    MeleeDamage[activeMeleeStep]);
        }

        public void CommitAnimationCarryContact()
        {
            if (activeAction != ActiveAction.Acquire || actionEventCommitted ||
                pendingTarget == null)
            {
                return;
            }
            bool ownershipCompleted = interactionOwner != null &&
                interactionOwner.TryCompleteAcquisition(pendingTarget);
            bool carried = ownershipCompleted && physicalLifecycle != null &&
                physicalLifecycle.BeginCarry(pendingTarget);
            actionEventCommitted = carried;
            if (pendingAcquisitionOutcome == PrototypeInteractionOutcome.Catch)
            {
                if (carried)
                {
                    allyRelay?.CompleteCatch(pendingTarget, false);
                }
                else
                {
                    allyRelay?.ResolveCatchFailure(pendingTarget);
                }
            }

            if (!carried)
            {
                interactionOwner?.CancelAcquisition(pendingTarget);
                interactionOwner?.CancelCarry(pendingTarget);
            }
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
                    allyRelay?.CompleteCatch(pendingTarget, true);
                }
                else
                {
                    interactionOwner?.CompleteThrow(pendingTarget);
                    allyRelay?.ResolveCatchFailure(pendingTarget);
                }
            }
            else
            {
                return;
            }

            actionEventCommitted = true;
        }

        public void CompleteAnimationAction()
        {
            if (activeAction == ActiveAction.None || activeAction == ActiveAction.Death)
            {
                return;
            }

            if (RequiresGameplayCommit(activeAction) && !actionEventCommitted)
            {
                Debug.LogError(
                    $"SHOWCASE ANIMATION COMPLETED WITHOUT GAMEPLAY EVENT: {activeAction}.",
                    this);
                CancelTimedOutAction(activeAction);
                return;
            }

            if (activeAction == ActiveAction.Melee && meleeQueued && activeMeleeStep < 3)
            {
                int nextStep = activeMeleeStep + 1;
                ShowcaseRunnerAgent nextTarget = meleeTarget;
                if (nextTarget == null || nextTarget.IsDead)
                {
                    nextTarget = runnerPressure != null
                        ? runnerPressure.ResolvePlayerMeleeTargetForCombo()
                        : null;
                }
                meleeQueued = false;
                StartMelee(nextStep, nextTarget ?? meleeTarget);
                return;
            }

            if (activeAction == ActiveAction.Knockdown)
            {
                activeAction = ActiveAction.Getup;
                actionStartedAt = Time.time;
                actionDuration = 1.15f;
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
            if (health == null || !health.TryApplyDamage(request))
            {
                return false;
            }

            CancelPendingGameplayAction();
            ClearReactionDisplacement();
            if (health.IsDead)
            {
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

            if (request.Reaction == ShowcaseHitReaction.Knockdown)
            {
                Vector3 direction = Vector3.ProjectOnPlane(
                    request.ImpactDirection,
                    Vector3.up).normalized;
                getupUsesBackVariant = Vector3.Dot(transform.forward, direction) < 0f;
                activeAction = ActiveAction.Knockdown;
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
                actionStartedAt = Time.time;
                actionDuration = 0.68f;
                locomotion?.SetMobilityLock(PlayerMobilityLockReason.HitReaction, true);
                SetCue(ShowcasePlayerAnimationCue.HitBody);
            }
            return true;
        }

        public void ResetToRunBaseline()
        {
            health?.ResetToBaseline();
            activeAction = ActiveAction.None;
            meleeTarget = null;
            meleeApproachDistance = 0f;
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
            meleeBufferWindowOpen = false;
            activeMeleeStep = 0;
            lastMeleeCompletedAt = float.NegativeInfinity;
            nextAttackSequence = 0;
            activeAttackSequence = 0;
            getupUsesBackVariant = false;
            actionRequestedAt = 0f;
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
            activeMeleeStep = Mathf.Clamp(step, 1, 3);
            meleeTarget = target;
            meleeApproachDistance = 0f;
            activeAction = ActiveAction.Melee;
            actionStartedAt = Time.time;
            actionDuration = MeleeDurations[activeMeleeStep];
            actionEventCommitted = false;
            meleeBufferWindowOpen = false;
            activeAttackSequence = ++nextAttackSequence;
            locomotion?.SetMobilityLock(PlayerMobilityLockReason.PlayerAction, true);
            SetCue(activeMeleeStep switch
            {
                1 => ShowcasePlayerAnimationCue.MeleeJab,
                2 => ShowcasePlayerAnimationCue.MeleeCross,
                _ => ShowcasePlayerAnimationCue.MeleeHook
            });
        }

        private void FaceMeleeTarget(float deltaTime)
        {
            if (meleeTarget == null || deltaTime <= 0f)
            {
                return;
            }

            Vector3 direction = Vector3.ProjectOnPlane(
                meleeTarget.transform.position - transform.position,
                Vector3.up);
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(direction.normalized, Vector3.up),
                360f * deltaTime);
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
            boneCombatRig?.CloseAttackWindow();
            activeAction = ActiveAction.None;
            if (completed == ActiveAction.Melee)
            {
                lastMeleeCompletedAt = Time.time;
                activeMeleeStep = 0;
                meleeQueued = false;
            }
            ReleaseMobilityLockFor(completed);
            meleeTarget = null;
            meleeApproachDistance = 0f;
            pendingTarget = null;
            pendingAcquisitionOutcome = PrototypeInteractionOutcome.None;
            pendingThrowDirection = Vector3.zero;
            pendingThrowAimPoint = Vector3.zero;
            pendingThrowUsesAimPoint = false;
            pendingThrowCharge = 0f;
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
                interactionOwner?.CancelAcquisition(pendingTarget);
                interactionOwner?.CancelCarry(pendingTarget);
                if (pendingAcquisitionOutcome == PrototypeInteractionOutcome.Catch)
                {
                    allyRelay?.ResolveCatchFailure(pendingTarget);
                }
            }
            else if (activeAction == ActiveAction.QueuedRelayThrow && pendingTarget != null)
            {
                interactionOwner?.CompleteThrow(pendingTarget);
                allyRelay?.ResolveCatchFailure(pendingTarget);
            }
            activeAction = ActiveAction.None;
            pendingTarget = null;
            pendingThrowDirection = Vector3.zero;
            pendingThrowAimPoint = Vector3.zero;
            pendingThrowUsesAimPoint = false;
            pendingThrowCharge = 0f;
            meleeTarget = null;
            meleeApproachDistance = 0f;
            meleeQueued = false;
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

        private void ReleaseMobilityLockFor(ActiveAction action)
        {
            switch (action)
            {
                case ActiveAction.Melee:
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
                PlayerMobilityLockReason.Death
            })
            {
                locomotion?.SetMobilityLock(reason, false);
            }
        }

        private void SetCue(ShowcasePlayerAnimationCue cue)
        {
            AnimationCue = cue;
            AnimationCueRevision++;
            awaitedAnimationCueRevision = AnimationCueRevision;
            actionRequestedAt = Time.time;
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
