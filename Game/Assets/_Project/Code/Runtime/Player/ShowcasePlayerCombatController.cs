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

        private ActiveAction activeAction;
        private ShowcaseRunnerAgent meleeTarget;
        private PrototypeInteractionTarget pendingTarget;
        private PrototypeInteractionOutcome pendingAcquisitionOutcome;
        private Vector3 pendingThrowDirection;
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

        public ShowcaseCombatHealth Health => health;
        public ShowcasePlayerAnimationCue AnimationCue { get; private set; }
        public uint AnimationCueRevision { get; private set; }
        public bool IsDead => health != null && health.IsDead;
        public bool IsActionLocked => activeAction != ActiveAction.None;
        public bool BlocksInteraction => IsDead || IsActionLocked;
        public bool CanBufferMelee => activeAction == ActiveAction.Melee &&
            activeMeleeStep < 3 && !meleeQueued;
        public int ActiveMeleeStep => activeMeleeStep;
        public bool HasBufferedMelee => meleeQueued;
        public uint ActiveAttackSequence => activeAttackSequence;

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

            float elapsed = Time.time - actionStartedAt;
            float normalized = actionDuration > 0f ? elapsed / actionDuration : 1f;

            // Imported events are authoritative. These late fallbacks keep a missing or
            // stripped event from permanently locking gameplay in a broken build.
            if (!actionEventCommitted && normalized >= 0.68f)
            {
                switch (activeAction)
                {
                    case ActiveAction.Melee:
                        CommitAnimationCombatHit();
                        break;
                    case ActiveAction.Acquire:
                        CommitAnimationCarryContact();
                        break;
                    case ActiveAction.DirectThrow:
                    case ActiveAction.QueuedRelayThrow:
                        CommitAnimationThrowRelease();
                        break;
                }
            }

            if (normalized < 1f)
            {
                return;
            }

            if (activeAction == ActiveAction.Melee && meleeQueued && activeMeleeStep < 3)
            {
                int nextStep = activeMeleeStep + 1;
                ShowcaseRunnerAgent nextTarget = runnerPressure != null
                    ? runnerPressure.ResolvePlayerMeleeTargetForCombo()
                    : meleeTarget;
                meleeQueued = false;
                StartMelee(nextStep, nextTarget ?? meleeTarget);
                return;
            }

            if (activeAction == ActiveAction.Knockdown)
            {
                activeAction = ActiveAction.Getup;
                actionStartedAt = Time.time;
                actionDuration = 1.15f;
                SetCue(getupUsesBackVariant
                    ? ShowcasePlayerAnimationCue.GetupBack
                    : ShowcasePlayerAnimationCue.GetupFront);
                return;
            }

            CompleteActiveAction();
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
            SubscribeToReset();
            ResetToRunBaseline();
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
                float normalized = Mathf.Clamp01(
                    (Time.time - actionStartedAt) / Mathf.Max(0.01f, actionDuration));
                if (!meleeQueued && activeMeleeStep < 3 &&
                    normalized >= 0.35f && normalized <= 0.70f)
                {
                    meleeQueued = true;
                    return true;
                }
                return false;
            }

            if (activeAction != ActiveAction.None || target == null)
            {
                return false;
            }

            int step = Time.time - lastMeleeCompletedAt <= 0.65f
                ? Mathf.Clamp(activeMeleeStep + 1, 1, 3)
                : 1;
            StartMelee(step, target);
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
            locomotion?.SetExternalMovementBlocked(true);

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
            pendingThrowCharge = Mathf.Clamp01(charge01);
            activeAction = ActiveAction.DirectThrow;
            actionStartedAt = Time.time;
            actionDuration = target.WeightClass == PrototypeWeightClass.Light ? 1.25f : 1.35f;
            actionEventCommitted = false;
            locomotion?.SetExternalMovementBlocked(true);
            SetCue(target.WeightClass == PrototypeWeightClass.Light
                ? ShowcasePlayerAnimationCue.ThrowSmall
                : ShowcasePlayerAnimationCue.ThrowMedium);
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
            pendingThrowCharge = Mathf.Clamp01(charge01);
            activeAction = ActiveAction.QueuedRelayThrow;
            actionStartedAt = Time.time;
            actionDuration = target.WeightClass == PrototypeWeightClass.Light ? 1.25f : 1.35f;
            actionEventCommitted = false;
            locomotion?.SetExternalMovementBlocked(true);
            SetCue(target.WeightClass == PrototypeWeightClass.Light
                ? ShowcasePlayerAnimationCue.ThrowSmall
                : ShowcasePlayerAnimationCue.ThrowMedium);
            return true;
        }

        public void CommitAnimationCombatHit()
        {
            if (activeAction != ActiveAction.Melee || actionEventCommitted)
            {
                return;
            }
            actionEventCommitted = true;
            runnerPressure?.CommitPlayerMeleeHit(
                meleeTarget,
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
            actionEventCommitted = true;
            bool carried = physicalLifecycle != null &&
                physicalLifecycle.BeginCarry(pendingTarget);
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
                    physicalLifecycle.TryThrow(pendingThrowDirection, pendingThrowCharge);
            }
            else if (activeAction == ActiveAction.QueuedRelayThrow)
            {
                released = physicalLifecycle != null &&
                    physicalLifecycle.TryThrowCommittedRelayTarget(
                        pendingTarget,
                        pendingThrowDirection,
                        pendingThrowCharge);
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

        public bool ReceiveDamage(ShowcaseDamageRequest request)
        {
            if (health == null || !health.TryApplyDamage(request))
            {
                return false;
            }

            CancelPendingGameplayAction();
            if (health.IsDead)
            {
                activeAction = ActiveAction.Death;
                locomotion?.SetExternalMovementBlocked(true);
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
                locomotion?.SetExternalMovementBlocked(true);
                SetCue(getupUsesBackVariant
                    ? ShowcasePlayerAnimationCue.KnockdownBack
                    : ShowcasePlayerAnimationCue.KnockdownFront);
            }
            else
            {
                activeAction = ActiveAction.HitReaction;
                actionStartedAt = Time.time;
                actionDuration = 0.68f;
                locomotion?.SetExternalMovementBlocked(true);
                SetCue(ShowcasePlayerAnimationCue.HitBody);
            }
            return true;
        }

        public void ResetToRunBaseline()
        {
            health?.ResetToBaseline();
            activeAction = ActiveAction.None;
            meleeTarget = null;
            pendingTarget = null;
            pendingAcquisitionOutcome = PrototypeInteractionOutcome.None;
            pendingThrowDirection = Vector3.zero;
            pendingThrowCharge = 0f;
            actionStartedAt = 0f;
            actionDuration = 0f;
            actionEventCommitted = false;
            meleeQueued = false;
            activeMeleeStep = 0;
            lastMeleeCompletedAt = float.NegativeInfinity;
            nextAttackSequence = 0;
            activeAttackSequence = 0;
            getupUsesBackVariant = false;
            AnimationCue = ShowcasePlayerAnimationCue.None;
            AnimationCueRevision++;
            locomotion?.SetExternalMovementBlocked(false);
        }

        private void StartMelee(int step, ShowcaseRunnerAgent target)
        {
            activeMeleeStep = Mathf.Clamp(step, 1, 3);
            meleeTarget = target;
            activeAction = ActiveAction.Melee;
            actionStartedAt = Time.time;
            actionDuration = MeleeDurations[activeMeleeStep];
            actionEventCommitted = false;
            activeAttackSequence = ++nextAttackSequence;
            locomotion?.SetExternalMovementBlocked(true);
            SetCue(activeMeleeStep switch
            {
                1 => ShowcasePlayerAnimationCue.MeleeJab,
                2 => ShowcasePlayerAnimationCue.MeleeCross,
                _ => ShowcasePlayerAnimationCue.MeleeHook
            });
        }

        private void CompleteActiveAction()
        {
            ActiveAction completed = activeAction;
            activeAction = ActiveAction.None;
            if (completed == ActiveAction.Melee)
            {
                lastMeleeCompletedAt = Time.time;
            }
            if (completed != ActiveAction.Death)
            {
                locomotion?.SetExternalMovementBlocked(false);
            }
            meleeTarget = null;
            pendingTarget = null;
            pendingAcquisitionOutcome = PrototypeInteractionOutcome.None;
            pendingThrowDirection = Vector3.zero;
            pendingThrowCharge = 0f;
            actionEventCommitted = false;
        }

        private void CancelPendingGameplayAction()
        {
            if (activeAction == ActiveAction.Acquire && pendingTarget != null)
            {
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
            meleeTarget = null;
            meleeQueued = false;
            actionEventCommitted = false;
        }

        private void SetCue(ShowcasePlayerAnimationCue cue)
        {
            AnimationCue = cue;
            AnimationCueRevision++;
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
