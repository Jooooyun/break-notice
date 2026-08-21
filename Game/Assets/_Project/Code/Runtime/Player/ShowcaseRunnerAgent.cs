using System.Collections.Generic;
using BreakNotice.Gameplay.Combat;
using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    public enum ShowcaseRunnerAnimationCue
    {
        None,
        Boxing,
        Boxing2,
        Kicking,
        Kicking2,
        EliteMutantPunch,
        EliteMutantSwiping,
        EliteJumpAttack,
        HitLight,
        HitBody,
        HitRib,
        HitStomach,
        HitBigHead,
        KnockdownBack,
        KnockdownFront,
        GetupBack,
        GetupFront,
        Death1,
        Death2
    }

    [DefaultExecutionOrder(-20)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public sealed class ShowcaseRunnerAgent : MonoBehaviour
    {
        // The delivered attacks are authored in place. The runner must finish
        // its approach at actual limb-contact distance instead of entering the
        // telegraph at the older broad acquisition radius.
        private const float PlayerStandoffDistance = 0.81f;
        private const float AttackArrivalTolerance = 0.01f;
        private const float FallbackTelegraphSeconds = 0.25f;
        private const float PostHitCooldown = 0.65f;
        // Cargo Slam approaches from above with a broad Chair silhouette, but
        // the Humanoid Head binding is intentionally small for fist combat.
        // Expand only this transaction so ordinary strike hurtboxes stay exact.
        private const float CargoSlamHeadHurtboxExpansion = 0.18f;
        // The Elite right hook travels across the actor's right side instead of
        // straight down its forward axis. A braced stance turns that authored
        // arc into the target without moving either root through the other.
        private const float EliteMutantPunchFacingOffsetDegrees = 38f;
        [SerializeField]
        private string runnerId = "RUNNER";

        [SerializeField]
        private ShowcaseRunnerPressureController coordinator;

        [SerializeField]
        private Rigidbody body;

        [SerializeField]
        private Transform playerTransform;

        [SerializeField]
        private ShowcaseCombatHealth health;

        [SerializeField]
        private bool elite;

        [SerializeField]
        private bool suppressAutonomousMovement;

        [SerializeField]
        private bool disableInBatchMode;

        [SerializeField]
        private ShowcaseBoneCombatRig boneCombatRig;

        [SerializeField, Min(0.1f)]
        private float approachSpeed = 2.7f;

        [SerializeField, Min(0.5f)]
        private float activationRange = 11f;

        [SerializeField, Min(0.5f)]
        private float attackRange = 1.25f;

        [SerializeField, Min(0.1f)]
        private float attackCooldown = 0.4f;

        private Vector3 baselinePosition;
        private Quaternion baselineRotation;
        private float stateElapsed;
        private float cooldownRemaining;
        private int attackSequence;
        private bool hitApplied;
        private uint activeAttackSequence;
        private uint attackRandomState;
        private int punchesUntilKick;
        private bool hasPreparedAttack;
        private bool attackTokenReleasedAtWindowClose;
        private Collider runnerCollider;
        private bool getupUsesBackVariant;
        private const float AnimatorStateEntryTimeout = 0.15f;
        private float animationRequestedAt;
        private bool animationStateAcknowledged;
        private uint awaitedAnimationCueRevision;
        private Vector3 pendingReactionDirection;
        private float pendingReactionDistance;
        private float pendingReactionDuration;
        private float reactionDistanceApplied;
        private float reactionElapsed;
        private bool reactionDisplacementPending;
        private bool reactionDisplacementActive;
        private ShowcaseObjectImpactEvaluation activeObjectImpact;
        private Vector3 currentReactionVelocity;
        private Vector3 commandedPlanarVelocity;
        private bool deathMomentumActive;
        private float secondaryStunUntil;
        private bool cargoSweepCarryActive;
        private uint cargoSweepCarrySequence;
        private Collider cargoSweepCarrySourceCollider;
        private Vector3 cargoSweepCarryDirection;
        private float cargoSweepCarrySpeed;
        private float cargoSweepCarryDuration;
        private float cargoSweepCarryElapsed;
        private float cargoSweepCarrySeparation;
        private float cargoSweepCarryMaximumDistance;
        private float cargoSweepCarryDistanceApplied;
        private Collider lastCargoSweepCarryBlock;
        private bool cargoSweepReactionProtected;
        private readonly HashSet<int> cargoSweepIgnoredRunnerIds = new();
        private float activeTelegraphDuration = FallbackTelegraphSeconds;
        private Vector3 lastPressureTarget;
        private float pressureTravelDistance;
        private bool maintainingPressureRing;
        private ShowcaseRunnerAnimationCue preparedAttackCue;

        public string RunnerId => runnerId;

        public ShowcaseRunnerState State { get; private set; } = ShowcaseRunnerState.Patrol;

        public ShowcaseRunnerAttackKind LastAttackKind { get; private set; }

        public bool CanReceiveCombatTag =>
            State == ShowcaseRunnerState.OffBalance ||
            State == ShowcaseRunnerState.Staggered;

        public bool IsIncapacitated => CanReceiveCombatTag ||
            State == ShowcaseRunnerState.GrabCommitted ||
            State == ShowcaseRunnerState.KnockedDown ||
            State == ShowcaseRunnerState.Dead;

        public bool IsDead => health != null && health.IsDead;

        public bool IsElite => elite;

        public bool SuppressesAutonomousMovement =>
            suppressAutonomousMovement;

        public bool DisablesInBatchMode => disableInBatchMode;

        public ShowcaseCombatHealth Health => health;

        public ShowcaseRunnerAnimationCue AnimationCue { get; private set; }

        public ShowcaseRunnerAnimationCue PreparedAttackCue =>
            hasPreparedAttack
                ? preparedAttackCue
                : ShowcaseRunnerAnimationCue.None;

        public uint AnimationCueRevision { get; private set; }

        public uint ActiveAttackSequence => activeAttackSequence;

        public float StateElapsed => stateElapsed;

        public float ActiveTelegraphDuration => activeTelegraphDuration;

        public Vector3 LastPressureTarget => lastPressureTarget;

        public float PressureTravelDistance => pressureTravelDistance;

        public bool IsMaintainingPressureRing => maintainingPressureRing;

        public float AttackStandoffDistance =>
            Mathf.Min(attackRange, PlayerStandoffDistance);
        public bool IsAttackWindup =>
            State == ShowcaseRunnerState.Telegraph && hasPreparedAttack;
        public bool IsAttackRecovery =>
            State == ShowcaseRunnerState.Recovering &&
            attackTokenReleasedAtWindowClose;
        public bool CanOpenAttackWindow =>
            !IsDead && (State == ShowcaseRunnerState.BodyCheck ||
                        State == ShowcaseRunnerState.Shove);

        public Vector3 BaselinePosition => baselinePosition;

        public int AttackCommitCount { get; private set; }
        public float CommandedPlanarSpeed { get; private set; }
        public bool IsAttackWindowOpen => boneCombatRig != null &&
            boneCombatRig.IsAttackWindowOpen;

        public ShowcaseObjectImpactEvaluation LastObjectImpactEvaluation
        {
            get;
            private set;
        }

        public ShowcaseObjectImpactSeverity LastImpactSeverity =>
            LastObjectImpactEvaluation.IsValid
                ? LastObjectImpactEvaluation.Severity
                : ShowcaseObjectImpactSeverity.Light;

        public float RequestedReactionDistance => pendingReactionDistance;

        public float ReactionDistanceApplied => reactionDistanceApplied;

        public Vector3 CurrentReactionVelocity => currentReactionVelocity;

        public Vector3 EstimatedPlanarVelocity
        {
            get
            {
                Vector3 reactionVelocity = reactionDisplacementActive
                    ? Vector3.ProjectOnPlane(
                        currentReactionVelocity,
                        Vector3.up)
                    : Vector3.zero;
                if (reactionVelocity.sqrMagnitude > 0.001f)
                {
                    return reactionVelocity;
                }

                Vector3 actualVelocity = body != null
                    ? Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up)
                    : Vector3.zero;
                return commandedPlanarVelocity.sqrMagnitude >=
                        actualVelocity.sqrMagnitude
                    ? commandedPlanarVelocity
                    : actualVelocity;
            }
        }

        public bool IsReactionDisplacementActive =>
            reactionDisplacementPending || reactionDisplacementActive ||
            cargoSweepCarryActive;

        public bool IsCargoSweepCarryActive => cargoSweepCarryActive;

        public uint CargoSweepCarrySequence => cargoSweepCarrySequence;

        public Collider LastCargoSweepCarryBlock => lastCargoSweepCarryBlock;

        public int CargoSweepCarryBlockCount { get; private set; }

        public bool IsCargoSweepReactionProtected =>
            cargoSweepReactionProtected;

        public bool IsDeathMomentumActive => deathMomentumActive;

        public int SecondaryStunCount { get; private set; }

        private void Awake()
        {
            body ??= GetComponent<Rigidbody>();
            runnerCollider = GetComponent<Collider>();
            boneCombatRig ??= GetComponent<ShowcaseBoneCombatRig>();
            ConfigurePlayerCollisionPair();
            CaptureBaseline();
            ResetAttackCadence();
            if (disableInBatchMode && Application.isBatchMode)
            {
                coordinator?.NotifyBatchDisabledValidationRunner(this);
                UnityEngine.Object.DestroyImmediate(gameObject);
                return;
            }
        }

        private void FixedUpdate()
        {
            CommandedPlanarSpeed = 0f;
            commandedPlanarVelocity = Vector3.zero;
            maintainingPressureRing = false;
            if (body == null)
            {
                return;
            }

            float deltaTime = Time.fixedDeltaTime;
            if (cargoSweepCarryActive)
            {
                AdvanceCargoSweepCarry(deltaTime);
            }
            else
            {
                AdvanceReactionDisplacement(deltaTime);
            }
            if (IsDead)
            {
                return;
            }
            if (coordinator == null || playerTransform == null ||
                !coordinator.CanSimulatePressure())
            {
                return;
            }
            if (!animationStateAcknowledged && awaitedAnimationCueRevision != 0)
            {
                if (State != ShowcaseRunnerState.Dead &&
                    Time.time - animationRequestedAt >= AnimatorStateEntryTimeout)
                {
                    Debug.LogError(
                        $"RUNNER ANIMATOR STATE ENTRY TIMEOUT: {State}. " +
                        "Damage or displacement was not forced.",
                        this);
                    CancelAnimationBoundState();
                }
                return;
            }

            stateElapsed += deltaTime;
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - deltaTime);
            switch (State)
            {
                case ShowcaseRunnerState.OffBalance:
                    if (stateElapsed >= 3f)
                    {
                        Debug.LogError("RUNNER HIT ANIMATION COMPLETION EVENT TIMEOUT.", this);
                        EnterState(ShowcaseRunnerState.Recovering);
                    }
                    return;
                case ShowcaseRunnerState.Staggered:
                    if (stateElapsed >= 3f)
                    {
                        Debug.LogError("RUNNER STAGGER ANIMATION COMPLETION EVENT TIMEOUT.", this);
                        EnterState(ShowcaseRunnerState.Recovering);
                    }
                    return;
                case ShowcaseRunnerState.Recovering:
                    if (attackTokenReleasedAtWindowClose)
                    {
                        float recoveryDistance = DistanceToPlayer();
                        if (recoveryDistance > activationRange)
                        {
                            EnterState(ShowcaseRunnerState.Patrol);
                            return;
                        }
                        if (recoveryDistance >
                            AttackStandoffDistance + AttackArrivalTolerance)
                        {
                            EnterState(ShowcaseRunnerState.Approach);
                            if (ShouldUsePressureRing(recoveryDistance))
                            {
                                AdvancePressureRing(approachSpeed, deltaTime);
                            }
                            else
                            {
                                AdvanceTowardPlayer(approachSpeed, deltaTime);
                            }
                            return;
                        }
                        if (cooldownRemaining <= 0f &&
                            coordinator.CanRunnerAutonomouslyAttack(this) &&
                            coordinator.TryAcquireAttackToken(this))
                        {
                            CommitNextAttack();
                            return;
                        }
                        AdvancePressureRing(approachSpeed, deltaTime);
                        return;
                    }
                    if (stateElapsed >= 0.45f)
                    {
                        EnterState(ShowcaseRunnerState.Patrol);
                    }
                    return;
                case ShowcaseRunnerState.KnockedDown:
                    if (stateElapsed >= 4f)
                    {
                        Debug.LogError("RUNNER KNOCKDOWN COMPLETION EVENT TIMEOUT.", this);
                        EnterState(ShowcaseRunnerState.Recovering);
                    }
                    return;
                case ShowcaseRunnerState.GettingUp:
                    if (stateElapsed >= 4f)
                    {
                        Debug.LogError("RUNNER GETUP COMPLETION EVENT TIMEOUT.", this);
                        EnterState(ShowcaseRunnerState.Recovering);
                    }
                    return;
                case ShowcaseRunnerState.Dead:
                    return;
                case ShowcaseRunnerState.Telegraph:
                    FacePlayer(deltaTime);
                    if (stateElapsed >= activeTelegraphDuration)
                    {
                        CommitPreparedAttack();
                    }
                    return;
                case ShowcaseRunnerState.BodyCheck:
                    if (stateElapsed >= 4f)
                    {
                        Debug.LogError(
                            "RUNNER ATTACK ANIMATION EVENT TIMEOUT; DAMAGE WAS NOT FORCED.",
                            this);
                        CompleteAttack();
                    }
                    return;
                case ShowcaseRunnerState.Shove:
                    if (stateElapsed >= 4f)
                    {
                        Debug.LogError(
                            "RUNNER ATTACK ANIMATION EVENT TIMEOUT; DAMAGE WAS NOT FORCED.",
                            this);
                        CompleteAttack();
                    }
                    return;
                case ShowcaseRunnerState.GrabCommitted:
                    return;
            }

            if (suppressAutonomousMovement)
            {
                // Validation runners remain fully damageable and keep authored
                // hit/carry displacement, but never approach, rotate toward,
                // telegraph or attack the player on their own.
                return;
            }

            float distance = DistanceToPlayer();
            if (distance > activationRange)
            {
                EnterState(ShowcaseRunnerState.Patrol);
                return;
            }

            if (distance > AttackStandoffDistance + AttackArrivalTolerance)
            {
                EnterState(ShowcaseRunnerState.Approach, false);
                if (ShouldUsePressureRing(distance))
                {
                    AdvancePressureRing(approachSpeed, deltaTime);
                }
                else
                {
                    AdvanceTowardPlayer(approachSpeed, deltaTime);
                }
                return;
            }

            if (cooldownRemaining <= 0f &&
                coordinator.CanRunnerAutonomouslyAttack(this) &&
                coordinator.TryAcquireAttackToken(this))
            {
                CommitNextAttack();
                return;
            }
            AdvancePressureRing(approachSpeed, deltaTime);
        }

        public void Configure(
            string authoredRunnerId,
            ShowcaseRunnerPressureController pressure,
            Rigidbody authoredBody,
            Transform player,
            float authoredApproachSpeed = 2.7f,
            float authoredBodyCheckSpeed = 6.5f,
            ShowcaseCombatHealth authoredHealth = null,
            bool authoredElite = false)
        {
            runnerId = string.IsNullOrWhiteSpace(authoredRunnerId)
                ? name
                : authoredRunnerId;
            coordinator = pressure;
            body = authoredBody != null ? authoredBody : GetComponent<Rigidbody>();
            playerTransform = player;
            approachSpeed = Mathf.Max(0.1f, authoredApproachSpeed);
            health = authoredHealth;
            elite = authoredElite;
            runnerCollider = GetComponent<Collider>();
            boneCombatRig ??= GetComponent<ShowcaseBoneCombatRig>();
            attackRange = 1.25f;
            ConfigurePlayerCollisionPair();
            CaptureBaseline();
            ResetToRunBaseline();
        }

        public void ConfigureBoneCombatRig(ShowcaseBoneCombatRig rig)
        {
            boneCombatRig = rig;
        }

        public void ConfigureAutonomousMovementSuppressed(bool suppressed)
        {
            suppressAutonomousMovement = suppressed;
            if (!suppressed)
            {
                return;
            }

            InterruptAttack();
            cooldownRemaining = Mathf.Max(cooldownRemaining, 0.3f);
            if (!IsDead && State != ShowcaseRunnerState.OffBalance &&
                State != ShowcaseRunnerState.Staggered &&
                State != ShowcaseRunnerState.KnockedDown &&
                State != ShowcaseRunnerState.GettingUp)
            {
                EnterState(ShowcaseRunnerState.Patrol);
            }
            CommandedPlanarSpeed = 0f;
            commandedPlanarVelocity = Vector3.zero;
        }

        public void ConfigureBatchModeDisabled(bool disabled)
        {
            disableInBatchMode = disabled;
        }

        public bool TryResolveCargoSlamContact(
            Collider cargoCollider,
            out Vector3 worldPoint)
        {
            Vector3 currentCenter = cargoCollider != null
                ? cargoCollider.bounds.center
                : Vector3.zero;
            return TryResolveCargoSlamContact(
                cargoCollider,
                currentCenter,
                out worldPoint);
        }

        public bool TryResolveCargoSlamContact(
            Collider cargoCollider,
            Vector3 previousColliderCenter,
            out Vector3 worldPoint)
        {
            if (IsDead || boneCombatRig == null)
            {
                worldPoint = default;
                return false;
            }

            return boneCombatRig.TryResolveExternalColliderContact(
                cargoCollider,
                previousColliderCenter,
                CargoSlamHeadHurtboxExpansion,
                out worldPoint);
        }

        public bool TryCommitShortGrab()
        {
            // No approved ENY01 grab/hold/release source exists yet. Keeping this
            // disabled prevents an invisible gameplay grab from preceding animation.
            return false;
        }

        public void ReceivePhysicalStagger(bool medium)
        {
            if (IsDead)
            {
                return;
            }
            InterruptAttack();
            SetAnimationCue(medium
                ? ShowcaseRunnerAnimationCue.HitBody
                : ShowcaseRunnerAnimationCue.HitLight);
            EnterState(medium
                ? ShowcaseRunnerState.Staggered
                : ShowcaseRunnerState.OffBalance);
        }

        public void ReceivePlayerShove(Vector3 playerForward)
        {
            if (IsDead)
            {
                return;
            }
            InterruptAttack();
            Vector3 direction = Vector3.ProjectOnPlane(playerForward, Vector3.up).normalized;
            SetAnimationCue(ShowcaseRunnerAnimationCue.HitBody);
            QueueReactionDisplacement(direction, 0.3f, 0.2f);
            EnterState(ShowcaseRunnerState.OffBalance);
        }

        public void ReceiveCombatTag()
        {
            if (!CanReceiveCombatTag)
            {
                return;
            }

            InterruptAttack();
            EnterState(ShowcaseRunnerState.Staggered);
        }

        public void ReceiveRescueStagger()
        {
            if (IsDead)
            {
                return;
            }
            InterruptAttack();
            EnterState(ShowcaseRunnerState.Staggered);
        }

        public void FinishGrabWithoutRescue()
        {
            if (State == ShowcaseRunnerState.GrabCommitted)
            {
                EnterState(ShowcaseRunnerState.Recovering);
                cooldownRemaining = attackCooldown;
            }
        }

        public void ConfigureTemporaryValidationBaseline(
            Vector3 worldPosition,
            Quaternion worldRotation)
        {
            coordinator?.ReleaseAttackToken(this);
            if (body != null)
            {
                body.position = worldPosition;
                body.rotation = worldRotation;
                if (!body.isKinematic)
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
            }
            transform.SetPositionAndRotation(worldPosition, worldRotation);
            CaptureBaseline();
            ResetToRunBaseline();
        }

        public void ResetToRunBaseline()
        {
            coordinator?.ReleaseAttackToken(this);
            if (body != null)
            {
                body.position = baselinePosition;
                body.rotation = baselineRotation;
                if (!body.isKinematic)
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
            }
            else
            {
                transform.SetPositionAndRotation(baselinePosition, baselineRotation);
            }

            State = ShowcaseRunnerState.Patrol;
            LastAttackKind = ShowcaseRunnerAttackKind.BodyCheck;
            stateElapsed = 0f;
            cooldownRemaining = 0.3f;
            attackSequence = 0;
            hitApplied = false;
            activeAttackSequence = 0;
            ResetAttackCadence();
            hasPreparedAttack = false;
            preparedAttackCue = ShowcaseRunnerAnimationCue.None;
            attackTokenReleasedAtWindowClose = false;
            activeTelegraphDuration = FallbackTelegraphSeconds;
            lastPressureTarget = baselinePosition;
            pressureTravelDistance = 0f;
            maintainingPressureRing = false;
            getupUsesBackVariant = false;
            animationRequestedAt = 0f;
            animationStateAcknowledged = false;
            awaitedAnimationCueRevision = 0;
            ClearReactionDisplacement();
            LastObjectImpactEvaluation = default;
            secondaryStunUntil = 0f;
            SecondaryStunCount = 0;
            CargoSweepCarryBlockCount = 0;
            lastCargoSweepCarryBlock = null;
            AttackCommitCount = 0;
            CommandedPlanarSpeed = 0f;
            commandedPlanarVelocity = Vector3.zero;
            AnimationCue = ShowcaseRunnerAnimationCue.None;
            AnimationCueRevision++;
            boneCombatRig?.ResetRuntimeState();
            health?.ResetToBaseline();
            if (runnerCollider != null)
            {
                runnerCollider.enabled = true;
            }
        }

        public bool ReceivePlayerMeleeHit(
            int comboStep,
            uint authoredAttackSequence,
            int damage,
            Vector3 playerPosition)
        {
            if (health == null || IsDead)
            {
                return false;
            }

            Vector3 impactDirection = Vector3.ProjectOnPlane(
                transform.position - playerPosition,
                Vector3.up).normalized;
            ShowcaseHitReaction reaction = comboStep switch
            {
                1 => ShowcaseHitReaction.Light,
                2 => (authoredAttackSequence % 3) switch
                {
                    0 => ShowcaseHitReaction.Body,
                    1 => ShowcaseHitReaction.Rib,
                    _ => ShowcaseHitReaction.Stomach
                },
                _ => ShowcaseHitReaction.BigHead
            };
            return ReceivePlayerHit(
                "CHR00",
                authoredAttackSequence,
                damage,
                reaction,
                impactDirection);
        }

        public bool ReceivePlayerObjectHit(
            string sourceId,
            uint authoredAttackSequence,
            int damage,
            Vector3 impactDirection)
        {
            if (string.IsNullOrWhiteSpace(sourceId) ||
                authoredAttackSequence == 0)
            {
                return false;
            }

            return ReceivePlayerHit(
                sourceId,
                authoredAttackSequence,
                damage,
                ShowcaseHitReaction.Body,
                impactDirection);
        }

        public bool ReceivePlayerObjectImpact(
            string sourceId,
            ShowcaseObjectImpactEvaluation evaluation)
        {
            if (health == null || IsDead || string.IsNullOrWhiteSpace(sourceId) ||
                !evaluation.IsValid)
            {
                return false;
            }

            Vector3 planarImpactDirection = Vector3.ProjectOnPlane(
                evaluation.Context.PrimaryImpactDirection,
                Vector3.up).normalized;
            if (planarImpactDirection.sqrMagnitude <= 0.001f)
            {
                planarImpactDirection = transform.forward;
            }
            ShowcaseHitReaction reaction = evaluation.CausesKnockdown
                ? ShowcaseHitReaction.Knockdown
                : evaluation.Severity == ShowcaseObjectImpactSeverity.Light
                    ? ShowcaseHitReaction.Light
                    : ShowcaseHitReaction.Body;
            bool accepted = health.TryApplyDamage(new ShowcaseDamageRequest(
                ShowcaseCombatTeam.Player,
                sourceId,
                evaluation.Context.AttackSequence,
                evaluation.Damage,
                reaction,
                planarImpactDirection));
            if (!accepted)
            {
                return false;
            }

            LastObjectImpactEvaluation = evaluation;
            InterruptAttack();
            cooldownRemaining = Mathf.Max(cooldownRemaining, PostHitCooldown);
            if (health.IsDead)
            {
                EnterDeath(evaluation);
                return true;
            }

            if (evaluation.CausesKnockdown)
            {
                getupUsesBackVariant = Vector3.Dot(
                    transform.forward,
                    planarImpactDirection) < 0f;
                SetAnimationCue(getupUsesBackVariant
                    ? ShowcaseRunnerAnimationCue.KnockdownBack
                    : ShowcaseRunnerAnimationCue.KnockdownFront);
                EnterState(ShowcaseRunnerState.KnockedDown);
            }
            else if (evaluation.Severity == ShowcaseObjectImpactSeverity.Light)
            {
                SetAnimationCue(ShowcaseRunnerAnimationCue.HitLight);
                EnterState(ShowcaseRunnerState.OffBalance);
            }
            else
            {
                SetAnimationCue(ShowcaseRunnerAnimationCue.HitBody);
                EnterState(ShowcaseRunnerState.Staggered);
            }
            QueueReactionDisplacement(
                planarImpactDirection,
                evaluation.DisplacementDistance,
                evaluation.DisplacementDuration,
                evaluation,
                true,
                false);
            return true;
        }

        public bool BeginCargoSweepCarry(
            Collider sourceCollider,
            Vector3 pathDirection,
            float pathSpeed,
            float duration,
            float separation,
            ShowcaseObjectImpactEvaluation evaluation)
        {
            Vector3 planarDirection = Vector3.ProjectOnPlane(
                pathDirection,
                Vector3.up).normalized;
            if (IsDead || body == null || runnerCollider == null ||
                sourceCollider == null || !sourceCollider.enabled ||
                planarDirection.sqrMagnitude <= 0.001f ||
                !evaluation.IsValid ||
                evaluation.Context.AttackOriginType !=
                    ShowcaseObjectImpactAttackOriginType.CargoStrapMediumSwing)
            {
                return false;
            }

            ClearReactionDisplacement();
            cargoSweepIgnoredRunnerIds.Clear();
            cargoSweepCarryActive = true;
            cargoSweepReactionProtected = true;
            cargoSweepCarrySequence = evaluation.Context.AttackSequence;
            cargoSweepCarrySourceCollider = sourceCollider;
            cargoSweepCarryDirection = planarDirection;
            cargoSweepCarryDuration = Mathf.Clamp(duration, 0.15f, 0.35f);
            cargoSweepCarryElapsed = 0f;
            cargoSweepCarrySeparation = Mathf.Max(0.02f, separation);
            cargoSweepCarryMaximumDistance = Mathf.Max(
                0.05f,
                evaluation.DisplacementDistance);
            cargoSweepCarryDistanceApplied = 0f;
            cargoSweepCarrySpeed = cargoSweepCarryMaximumDistance /
                cargoSweepCarryDuration;
            lastCargoSweepCarryBlock = null;
            pendingReactionDirection = planarDirection;
            activeObjectImpact = evaluation;
            currentReactionVelocity =
                cargoSweepCarryDirection * cargoSweepCarrySpeed;
            ResolveInitialCargoSweepSeparation();
            return true;
        }

        public void CancelCargoSweepCarry(uint swingSequence)
        {
            if (!cargoSweepCarryActive ||
                (swingSequence != 0 &&
                 swingSequence != cargoSweepCarrySequence))
            {
                return;
            }
            CompleteCargoSweepCarry();
        }

        public bool ReceiveSecondaryObjectImpact(
            string sourceId,
            uint authoredAttackSequence,
            int damage,
            Vector3 impactDirection,
            float displacementDistance,
            float displacementDuration,
            float stunSeconds)
        {
            if (health == null || IsDead || string.IsNullOrWhiteSpace(sourceId) ||
                authoredAttackSequence == 0 || damage <= 0)
            {
                return false;
            }

            Vector3 planarDirection = Vector3.ProjectOnPlane(
                impactDirection,
                Vector3.up).normalized;
            if (planarDirection.sqrMagnitude <= 0.001f)
            {
                planarDirection = transform.forward;
            }
            bool accepted = health.TryApplyDamage(new ShowcaseDamageRequest(
                ShowcaseCombatTeam.Player,
                sourceId,
                authoredAttackSequence,
                damage,
                ShowcaseHitReaction.Body,
                planarDirection));
            if (!accepted)
            {
                return false;
            }

            InterruptAttack();
            cooldownRemaining = Mathf.Max(
                cooldownRemaining,
                Mathf.Max(PostHitCooldown, stunSeconds));
            secondaryStunUntil = Mathf.Max(
                secondaryStunUntil,
                Time.time + Mathf.Max(0f, stunSeconds));
            SecondaryStunCount++;
            if (health.IsDead)
            {
                EnterDeath();
                return true;
            }

            SetAnimationCue(ShowcaseRunnerAnimationCue.HitBody);
            EnterState(ShowcaseRunnerState.Staggered);
            QueueReactionDisplacement(
                planarDirection,
                displacementDistance,
                displacementDuration,
                default,
                true,
                false);
            return true;
        }

        public void ApplySecondaryImpactStun(float stunSeconds)
        {
            if (IsDead)
            {
                return;
            }

            InterruptAttack();
            cooldownRemaining = Mathf.Max(
                cooldownRemaining,
                Mathf.Max(PostHitCooldown, stunSeconds));
            secondaryStunUntil = Mathf.Max(
                secondaryStunUntil,
                Time.time + Mathf.Max(0f, stunSeconds));
            SecondaryStunCount++;
            if (State == ShowcaseRunnerState.KnockedDown ||
                State == ShowcaseRunnerState.GettingUp)
            {
                return;
            }

            if (State != ShowcaseRunnerState.Staggered)
            {
                SetAnimationCue(ShowcaseRunnerAnimationCue.HitBody);
            }
            EnterState(ShowcaseRunnerState.Staggered);
        }

        private bool ReceivePlayerHit(
            string sourceId,
            uint authoredAttackSequence,
            int damage,
            ShowcaseHitReaction reaction,
            Vector3 impactDirection)
        {
            if (health == null || IsDead)
            {
                return false;
            }

            Vector3 planarImpactDirection = Vector3.ProjectOnPlane(
                impactDirection,
                Vector3.up).normalized;
            if (planarImpactDirection.sqrMagnitude <= 0.001f)
            {
                planarImpactDirection = transform.forward;
            }

            bool accepted = health.TryApplyDamage(new ShowcaseDamageRequest(
                ShowcaseCombatTeam.Player,
                sourceId,
                authoredAttackSequence,
                damage,
                reaction,
                planarImpactDirection));
            if (!accepted)
            {
                return false;
            }

            InterruptAttack();
            cooldownRemaining = Mathf.Max(cooldownRemaining, PostHitCooldown);
            if (health.IsDead)
            {
                EnterDeath();
                return true;
            }

            switch (reaction)
            {
                case ShowcaseHitReaction.Light:
                    SetAnimationCue(ShowcaseRunnerAnimationCue.HitLight);
                    QueueReactionDisplacement(planarImpactDirection, 0.1f, 0.18f);
                    EnterState(ShowcaseRunnerState.OffBalance);
                    break;
                case ShowcaseHitReaction.Body:
                    SetAnimationCue(ShowcaseRunnerAnimationCue.HitBody);
                    QueueReactionDisplacement(planarImpactDirection, 0.15f, 0.2f);
                    EnterState(ShowcaseRunnerState.Staggered);
                    break;
                case ShowcaseHitReaction.Rib:
                    SetAnimationCue(ShowcaseRunnerAnimationCue.HitRib);
                    QueueReactionDisplacement(planarImpactDirection, 0.15f, 0.2f);
                    EnterState(ShowcaseRunnerState.Staggered);
                    break;
                case ShowcaseHitReaction.Stomach:
                    SetAnimationCue(ShowcaseRunnerAnimationCue.HitStomach);
                    QueueReactionDisplacement(planarImpactDirection, 0.15f, 0.2f);
                    EnterState(ShowcaseRunnerState.Staggered);
                    break;
                default:
                    getupUsesBackVariant = Vector3.Dot(
                        transform.forward,
                        planarImpactDirection) < 0f;
                    SetAnimationCue(getupUsesBackVariant
                        ? ShowcaseRunnerAnimationCue.KnockdownBack
                        : ShowcaseRunnerAnimationCue.KnockdownFront);
                    QueueReactionDisplacement(
                        planarImpactDirection,
                        0.55f,
                        0.24f);
                    EnterState(ShowcaseRunnerState.KnockedDown);
                    break;
            }
            return true;
        }

        public void CommitAnimationAttackHit()
        {
            // Retained for old serialized events only. Damage is committed only by
            // TryCommitBoneHit after a bone-tracked overlap.
        }

        public void NotifyAnimationStateEntered(uint cueRevision)
        {
            if (cueRevision == 0 || cueRevision != awaitedAnimationCueRevision ||
                animationStateAcknowledged)
            {
                return;
            }

            animationStateAcknowledged = true;
            stateElapsed = 0f;
            if (reactionDisplacementPending)
            {
                reactionDisplacementPending = false;
                reactionDisplacementActive = true;
                reactionElapsed = 0f;
                reactionDistanceApplied = 0f;
            }
        }

        public void SetAnimationLungeWindow(bool isOpen)
        {
            // Legacy AnimationEvent compatibility only. Elite Jump Attack is no
            // longer reachable and no runner movement is owned by this event.
        }

        internal bool TryCommitBoneHit(
            ShowcasePlayerCombatController actualVictim,
            uint expectedAttackSequence)
        {
            if (hitApplied || actualVictim == null || actualVictim.IsDead ||
                coordinator == null || !CanOpenAttackWindow ||
                expectedAttackSequence == 0 ||
                expectedAttackSequence != activeAttackSequence)
            {
                return false;
            }

            bool heavy = AnimationCue == ShowcaseRunnerAnimationCue.Kicking ||
                AnimationCue == ShowcaseRunnerAnimationCue.Kicking2 ||
                AnimationCue == ShowcaseRunnerAnimationCue.Boxing2;
            bool eliteTechnique =
                AnimationCue == ShowcaseRunnerAnimationCue.EliteMutantPunch ||
                AnimationCue == ShowcaseRunnerAnimationCue.EliteMutantSwiping;
            bool accepted = coordinator.ApplyPlayerAttackFromBoneContact(
                this,
                actualVictim,
                heavy ? 30 : eliteTechnique ? 25 : 20,
                heavy,
                heavy ? 4.2f : eliteTechnique ? 3.8f : 3.2f,
                activeAttackSequence);
            if (accepted)
            {
                hitApplied = true;
            }
            return accepted;
        }

        public void CompleteAnimationAction()
        {
            switch (State)
            {
                case ShowcaseRunnerState.Telegraph:
                case ShowcaseRunnerState.BodyCheck:
                case ShowcaseRunnerState.Shove:
                    CompleteAttack();
                    break;
                case ShowcaseRunnerState.OffBalance:
                case ShowcaseRunnerState.Staggered:
                    EnterState(ShowcaseRunnerState.Recovering);
                    break;
                case ShowcaseRunnerState.KnockedDown:
                    SetAnimationCue(getupUsesBackVariant
                        ? ShowcaseRunnerAnimationCue.GetupBack
                        : ShowcaseRunnerAnimationCue.GetupFront);
                    EnterState(ShowcaseRunnerState.GettingUp);
                    break;
                case ShowcaseRunnerState.GettingUp:
                    EnterState(ShowcaseRunnerState.Recovering);
                    break;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (IsDead)
            {
                return;
            }
            PrototypeInteractionTarget source =
                collision.collider.GetComponentInParent<PrototypeInteractionTarget>();
            if (source != null)
            {
                Rigidbody sourceBody = source.GetComponent<Rigidbody>();
                Vector3 worldPoint = collision.contactCount > 0
                    ? collision.GetContact(0).point
                    : transform.position + Vector3.up;
                coordinator?.RegisterPhysicalImpact(
                    this,
                    source,
                    collision.relativeVelocity.magnitude,
                    sourceBody != null
                        ? sourceBody.linearVelocity
                        : Vector3.zero,
                    worldPoint);
            }
        }

        private void CommitNextAttack()
        {
            if (hasPreparedAttack)
            {
                return;
            }

            boneCombatRig?.CloseAttackWindow();
            ShowcaseRunnerAnimationCue cue = ResolveNextAttackAnimation();
            LastAttackKind = IsKickCue(cue)
                ? ShowcaseRunnerAttackKind.BodyCheck
                : ShowcaseRunnerAttackKind.Shove;
            preparedAttackCue = cue;
            attackSequence++;
            hitApplied = false;
            activeAttackSequence++;
            activeTelegraphDuration = coordinator != null
                ? coordinator.ResolveTelegraphDuration(
                    this,
                    activeAttackSequence)
                : FallbackTelegraphSeconds;
            attackTokenReleasedAtWindowClose = false;
            AttackCommitCount++;

            hasPreparedAttack = true;
            EnterState(ShowcaseRunnerState.Telegraph);
        }

        private void CommitPreparedAttack()
        {
            if (!hasPreparedAttack)
            {
                CompleteAttack();
                return;
            }

            ShowcaseRunnerAnimationCue cue = preparedAttackCue;
            hasPreparedAttack = false;
            preparedAttackCue = ShowcaseRunnerAnimationCue.None;
            EnterState(LastAttackKind == ShowcaseRunnerAttackKind.BodyCheck
                ? ShowcaseRunnerState.BodyCheck
                : ShowcaseRunnerState.Shove);
            SetAnimationCue(cue);
        }

        public void NotifyAttackWindowClosed(uint closingAttackSequence)
        {
            if (attackTokenReleasedAtWindowClose ||
                closingAttackSequence == 0 ||
                closingAttackSequence != activeAttackSequence ||
                !CanOpenAttackWindow)
            {
                return;
            }

            attackTokenReleasedAtWindowClose = true;
            cooldownRemaining = Mathf.Max(cooldownRemaining, attackCooldown);
            coordinator?.ReleaseAttackToken(this);
        }

        private void CompleteAttack()
        {
            boneCombatRig?.CloseAttackWindow();
            hasPreparedAttack = false;
            preparedAttackCue = ShowcaseRunnerAnimationCue.None;
            coordinator?.ReleaseAttackToken(this);
            if (!attackTokenReleasedAtWindowClose)
            {
                cooldownRemaining = Mathf.Max(cooldownRemaining, attackCooldown);
            }
            EnterState(ShowcaseRunnerState.Recovering);
        }

        private void InterruptAttack()
        {
            boneCombatRig?.CloseAttackWindow();
            hasPreparedAttack = false;
            preparedAttackCue = ShowcaseRunnerAnimationCue.None;
            coordinator?.ReleaseAttackToken(this);
            attackTokenReleasedAtWindowClose = false;
        }

        private float AdvanceTowardPlayer(float speed, float deltaTime)
        {
            Vector3 direction = Vector3.ProjectOnPlane(
                playerTransform.position - body.position,
                Vector3.up);
            if (direction.sqrMagnitude <= 0.001f)
            {
                return 0f;
            }

            float planarDistance = direction.magnitude;
            direction.Normalize();
            CommandedPlanarSpeed = Mathf.Max(0f, speed);
            commandedPlanarVelocity = direction * CommandedPlanarSpeed;
            float distance = Mathf.Min(
                speed * deltaTime,
                Mathf.Max(0f, planarDistance - PlayerStandoffDistance));
            if (distance <= 0.0001f)
            {
                CommandedPlanarSpeed = 0f;
                commandedPlanarVelocity = Vector3.zero;
                return 0f;
            }
            if (body.SweepTest(direction, out RaycastHit hit, distance + 0.08f,
                    QueryTriggerInteraction.Ignore) &&
                hit.transform != playerTransform &&
                !hit.transform.IsChildOf(playerTransform))
            {
                CommandedPlanarSpeed = 0f;
                commandedPlanarVelocity = Vector3.zero;
                return 0f;
            }

            body.MovePosition(body.position + direction * distance);
            body.MoveRotation(Quaternion.RotateTowards(
                body.rotation,
                Quaternion.LookRotation(direction, Vector3.up),
                540f * deltaTime));
            return distance;
        }

        private bool ShouldUsePressureRing(float distanceToPlayer)
        {
            return distanceToPlayer <= 2.35f && coordinator != null &&
                coordinator.ShouldMaintainPressureRing(this);
        }

        private float AdvancePressureRing(float speed, float deltaTime)
        {
            if (coordinator == null ||
                !coordinator.TryResolvePressurePosition(
                    this,
                    out Vector3 pressureTarget,
                    out float speedScale))
            {
                return 0f;
            }

            maintainingPressureRing = true;
            lastPressureTarget = pressureTarget;
            float distance = AdvanceTowardPoint(
                pressureTarget,
                Mathf.Max(0.1f, speed * speedScale),
                deltaTime,
                0.06f);
            pressureTravelDistance += distance;
            return distance;
        }

        private float AdvanceTowardPoint(
            Vector3 worldPoint,
            float speed,
            float deltaTime,
            float stopDistance)
        {
            Vector3 direction = Vector3.ProjectOnPlane(
                worldPoint - body.position,
                Vector3.up);
            if (direction.sqrMagnitude <= 0.001f)
            {
                return 0f;
            }

            float planarDistance = direction.magnitude;
            direction.Normalize();
            float distance = Mathf.Min(
                Mathf.Max(0f, speed) * deltaTime,
                Mathf.Max(0f, planarDistance - Mathf.Max(0f, stopDistance)));
            if (distance <= 0.0001f)
            {
                return 0f;
            }

            Vector3 moveDirection = direction;
            float moveDistance = distance;
            if (IsPlanarMoveBlocked(moveDirection, moveDistance))
            {
                Vector3 tangent = Vector3.Cross(Vector3.up, direction).normalized;
                if ((CreateAttackRandomSeed() & 1u) != 0u)
                {
                    tangent = -tangent;
                }
                moveDistance *= 0.72f;
                if (IsPlanarMoveBlocked(tangent, moveDistance))
                {
                    tangent = -tangent;
                }
                if (IsPlanarMoveBlocked(tangent, moveDistance))
                {
                    CommandedPlanarSpeed = 0f;
                    commandedPlanarVelocity = Vector3.zero;
                    return 0f;
                }
                moveDirection = tangent;
            }

            CommandedPlanarSpeed = Mathf.Max(0f, speed);
            commandedPlanarVelocity = moveDirection * CommandedPlanarSpeed;
            body.MovePosition(body.position + moveDirection * moveDistance);
            body.MoveRotation(Quaternion.RotateTowards(
                body.rotation,
                Quaternion.LookRotation(moveDirection, Vector3.up),
                540f * deltaTime));
            return moveDistance;
        }

        private bool IsPlanarMoveBlocked(Vector3 direction, float distance)
        {
            return body.SweepTest(
                       direction,
                       out RaycastHit hit,
                       distance + 0.08f,
                       QueryTriggerInteraction.Ignore) &&
                   hit.transform != playerTransform &&
                   (playerTransform == null ||
                    !hit.transform.IsChildOf(playerTransform));
        }

        private void ConfigurePlayerCollisionPair()
        {
            if (runnerCollider == null || playerTransform == null)
            {
                return;
            }

            CharacterController playerController =
                playerTransform.GetComponent<CharacterController>();
            if (playerController != null)
            {
                // Damage is owned by the animated bone volumes. Root colliders
                // remain solid against walls/props but never depenetrate the player.
                Physics.IgnoreCollision(runnerCollider, playerController, true);
            }
        }

        private void FacePlayer(float deltaTime)
        {
            Vector3 direction = Vector3.ProjectOnPlane(
                playerTransform.position - body.position,
                Vector3.up);
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up);
                if (AnimationCue == ShowcaseRunnerAnimationCue.EliteMutantPunch ||
                    preparedAttackCue ==
                        ShowcaseRunnerAnimationCue.EliteMutantPunch)
                {
                    targetRotation *= Quaternion.Euler(
                        0f,
                        EliteMutantPunchFacingOffsetDegrees,
                        0f);
                }
                body.MoveRotation(Quaternion.RotateTowards(
                    body.rotation,
                    targetRotation,
                    540f * deltaTime));
            }
        }

        private void MoveBy(Vector3 offset)
        {
            if (body != null)
            {
                body.position += Vector3.ProjectOnPlane(offset, Vector3.up);
            }
        }

        private void QueueReactionDisplacement(
            Vector3 direction,
            float distance,
            float duration)
        {
            QueueReactionDisplacement(
                direction,
                distance,
                duration,
                default,
                false,
                false);
        }

        private void QueueReactionDisplacement(
            Vector3 direction,
            float distance,
            float duration,
            ShowcaseObjectImpactEvaluation objectImpact,
            bool activateImmediately,
            bool preserveDeathMomentum)
        {
            pendingReactionDirection = Vector3.ProjectOnPlane(
                direction,
                Vector3.up).normalized;
            pendingReactionDistance = Mathf.Max(0f, distance);
            pendingReactionDuration = Mathf.Max(0.01f, duration);
            reactionDistanceApplied = 0f;
            reactionElapsed = 0f;
            activeObjectImpact = objectImpact;
            cargoSweepIgnoredRunnerIds.Clear();
            bool hasDisplacement =
                pendingReactionDirection.sqrMagnitude > 0.001f &&
                pendingReactionDistance > 0f;
            cargoSweepReactionProtected = hasDisplacement &&
                objectImpact.IsValid &&
                objectImpact.Context.AttackOriginType ==
                    ShowcaseObjectImpactAttackOriginType.CargoStrapMediumSwing;
            reactionDisplacementPending = hasDisplacement && !activateImmediately;
            reactionDisplacementActive = hasDisplacement && activateImmediately;
            deathMomentumActive = hasDisplacement && preserveDeathMomentum;
            currentReactionVelocity = hasDisplacement
                ? pendingReactionDirection *
                  (pendingReactionDistance / pendingReactionDuration)
                : Vector3.zero;
        }

        private void ResolveInitialCargoSweepSeparation()
        {
            if (!cargoSweepCarryActive || body == null ||
                runnerCollider == null ||
                cargoSweepCarrySourceCollider == null)
            {
                return;
            }

            Bounds runnerBounds = runnerCollider.bounds;
            Bounds sourceBounds = cargoSweepCarrySourceCollider.bounds;
            float requiredCenterGap =
                ProjectBoundsRadius(
                    runnerBounds.extents,
                    cargoSweepCarryDirection) +
                ProjectBoundsRadius(
                    sourceBounds.extents,
                    cargoSweepCarryDirection) +
                cargoSweepCarrySeparation;
            float currentCenterGap = Vector3.Dot(
                runnerBounds.center - sourceBounds.center,
                cargoSweepCarryDirection);
            float correction = Mathf.Max(
                0f,
                requiredCenterGap - currentCenterGap);

            if (Physics.ComputePenetration(
                    runnerCollider,
                    runnerCollider.transform.position,
                    runnerCollider.transform.rotation,
                    cargoSweepCarrySourceCollider,
                    cargoSweepCarrySourceCollider.transform.position,
                    cargoSweepCarrySourceCollider.transform.rotation,
                    out Vector3 separationDirection,
                    out float separationDistance) &&
                Vector3.Dot(
                    separationDirection,
                    cargoSweepCarryDirection) > -0.2f)
            {
                correction = Mathf.Max(
                    correction,
                    separationDistance + cargoSweepCarrySeparation);
            }

            correction = Mathf.Min(
                correction,
                Mathf.Min(
                    0.35f,
                    cargoSweepCarryMaximumDistance * 0.5f));
            if (correction <= 0.001f ||
                TryResolveReactionBlock(correction, out _))
            {
                return;
            }

            body.MovePosition(
                body.position + cargoSweepCarryDirection * correction);
            reactionDistanceApplied += correction;
            cargoSweepCarryDistanceApplied += correction;
        }

        private void AdvanceCargoSweepCarry(float deltaTime)
        {
            if (!cargoSweepCarryActive || body == null)
            {
                return;
            }
            if (IsDead || cargoSweepCarrySourceCollider == null ||
                !cargoSweepCarrySourceCollider.enabled ||
                cargoSweepCarryElapsed >= cargoSweepCarryDuration ||
                cargoSweepCarryDistanceApplied >=
                    cargoSweepCarryMaximumDistance - 0.001f)
            {
                CompleteCargoSweepCarry();
                return;
            }

            Rigidbody sourceBody =
                cargoSweepCarrySourceCollider.attachedRigidbody;
            Vector3 sourceVelocity = sourceBody != null
                ? Vector3.ProjectOnPlane(
                    sourceBody.linearVelocity,
                    Vector3.up)
                : Vector3.zero;
            if (sourceVelocity.sqrMagnitude > 0.25f)
            {
                cargoSweepCarryDirection = Vector3.Slerp(
                    cargoSweepCarryDirection,
                    sourceVelocity.normalized,
                    0.35f).normalized;
            }
            pendingReactionDirection = cargoSweepCarryDirection;

            Bounds runnerBounds = runnerCollider.bounds;
            Bounds sourceBounds = cargoSweepCarrySourceCollider.bounds;
            float requiredCenterGap =
                ProjectBoundsRadius(
                    runnerBounds.extents,
                    cargoSweepCarryDirection) +
                ProjectBoundsRadius(
                    sourceBounds.extents,
                    cargoSweepCarryDirection) +
                cargoSweepCarrySeparation;
            float currentCenterGap = Vector3.Dot(
                runnerBounds.center - sourceBounds.center,
                cargoSweepCarryDirection);
            float gapCorrection = Mathf.Clamp(
                requiredCenterGap - currentCenterGap,
                0f,
                cargoSweepCarrySpeed * deltaTime * 0.5f);
            float stepDistance =
                (cargoSweepCarrySpeed * deltaTime) + gapCorrection;
            stepDistance = Mathf.Min(
                stepDistance,
                Mathf.Max(
                    0f,
                    cargoSweepCarryMaximumDistance -
                    cargoSweepCarryDistanceApplied));

            if (stepDistance > 0f &&
                TryResolveReactionBlock(
                    stepDistance,
                    out RaycastHit blockingHit))
            {
                if (TryContinueCargoSweepThroughRunner(
                        blockingHit.collider,
                        activeObjectImpact))
                {
                    cargoSweepCarryElapsed = Mathf.Min(
                        cargoSweepCarryDuration,
                        cargoSweepCarryElapsed + deltaTime);
                    currentReactionVelocity =
                        cargoSweepCarryDirection * cargoSweepCarrySpeed;
                    return;
                }

                float permittedDistance = Mathf.Clamp(
                    blockingHit.distance - 0.02f,
                    0f,
                    stepDistance);
                if (permittedDistance > 0f)
                {
                    body.MovePosition(
                        body.position +
                        cargoSweepCarryDirection * permittedDistance);
                    reactionDistanceApplied += permittedDistance;
                    cargoSweepCarryDistanceApplied += permittedDistance;
                }

                ShowcaseObjectImpactEvaluation collisionImpact =
                    activeObjectImpact;
                Collider obstacle = blockingHit.collider;
                lastCargoSweepCarryBlock = obstacle;
                CargoSweepCarryBlockCount++;
                CompleteCargoSweepCarry(preserveLastBlock: true);
                if (collisionImpact.IsValid && obstacle != null)
                {
                    coordinator?.RegisterSecondaryImpact(
                        this,
                        obstacle,
                        collisionImpact);
                }
                return;
            }

            if (stepDistance > 0f)
            {
                body.MovePosition(
                    body.position +
                    cargoSweepCarryDirection * stepDistance);
                reactionDistanceApplied += stepDistance;
                cargoSweepCarryDistanceApplied += stepDistance;
            }
            cargoSweepCarryElapsed = Mathf.Min(
                cargoSweepCarryDuration,
                cargoSweepCarryElapsed + deltaTime);
            currentReactionVelocity =
                cargoSweepCarryDirection * cargoSweepCarrySpeed;
            if (cargoSweepCarryElapsed >= cargoSweepCarryDuration)
            {
                CompleteCargoSweepCarry();
            }
        }

        private static float ProjectBoundsRadius(
            Vector3 extents,
            Vector3 direction)
        {
            Vector3 absoluteDirection = new Vector3(
                Mathf.Abs(direction.x),
                Mathf.Abs(direction.y),
                Mathf.Abs(direction.z));
            return Vector3.Dot(extents, absoluteDirection);
        }

        private void CompleteCargoSweepCarry(bool preserveLastBlock = false)
        {
            cargoSweepCarryActive = false;
            cargoSweepCarrySequence = 0;
            cargoSweepCarrySourceCollider = null;
            cargoSweepCarryDirection = Vector3.zero;
            cargoSweepCarrySpeed = 0f;
            cargoSweepCarryDuration = 0f;
            cargoSweepCarryElapsed = 0f;
            cargoSweepCarrySeparation = 0f;
            cargoSweepCarryMaximumDistance = 0f;
            cargoSweepCarryDistanceApplied = 0f;
            cargoSweepReactionProtected = false;
            cargoSweepIgnoredRunnerIds.Clear();
            currentReactionVelocity = Vector3.zero;
            activeObjectImpact = default;
            if (!preserveLastBlock)
            {
                lastCargoSweepCarryBlock = null;
            }
        }

        private void AdvanceReactionDisplacement(float deltaTime)
        {
            if (!reactionDisplacementActive || body == null)
            {
                return;
            }

            reactionElapsed = Mathf.Min(pendingReactionDuration, reactionElapsed + deltaTime);
            float targetDistance = pendingReactionDistance *
                (reactionElapsed / pendingReactionDuration);
            float stepDistance = Mathf.Max(0f, targetDistance - reactionDistanceApplied);
            if (stepDistance > 0f)
            {
                if (TryResolveReactionBlock(
                        stepDistance,
                        out RaycastHit blockingHit))
                {
                    if (TryContinueCargoSweepThroughRunner(
                            blockingHit.collider,
                            activeObjectImpact))
                    {
                        return;
                    }

                    float permittedDistance = Mathf.Clamp(
                        blockingHit.distance - 0.02f,
                        0f,
                        stepDistance);
                    if (permittedDistance > 0f)
                    {
                        body.MovePosition(
                            body.position +
                            pendingReactionDirection * permittedDistance);
                        reactionDistanceApplied += permittedDistance;
                    }

                    ShowcaseObjectImpactEvaluation collisionImpact =
                        activeObjectImpact;
                    Collider obstacle = blockingHit.collider;
                    CompleteReactionDisplacement();
                    if (collisionImpact.IsValid && obstacle != null)
                    {
                        coordinator?.RegisterSecondaryImpact(
                            this,
                            obstacle,
                            collisionImpact);
                    }
                    return;
                }

                body.MovePosition(body.position + pendingReactionDirection * stepDistance);
                reactionDistanceApplied += stepDistance;
            }

            if (reactionElapsed >= pendingReactionDuration)
            {
                CompleteReactionDisplacement();
            }
        }

        private bool TryContinueCargoSweepThroughRunner(
            Collider obstacle,
            ShowcaseObjectImpactEvaluation collisionImpact)
        {
            if (!cargoSweepReactionProtected || obstacle == null ||
                !collisionImpact.IsValid || coordinator == null)
            {
                return false;
            }

            ShowcaseRunnerAgent otherRunner =
                obstacle.GetComponentInParent<ShowcaseRunnerAgent>();
            if (otherRunner == null || otherRunner == this)
            {
                return false;
            }

            int otherId = otherRunner.GetInstanceID();
            if (cargoSweepIgnoredRunnerIds.Add(otherId) &&
                !otherRunner.IsDead)
            {
                coordinator.RegisterSecondaryImpact(
                    this,
                    obstacle,
                    collisionImpact);
            }
            return true;
        }

        private bool TryResolveReactionBlock(
            float stepDistance,
            out RaycastHit blockingHit)
        {
            blockingHit = default;
            RaycastHit[] hits = body.SweepTestAll(
                pendingReactionDirection,
                stepDistance + 0.02f,
                QueryTriggerInteraction.Ignore);
            float nearestDistance = float.PositiveInfinity;
            foreach (RaycastHit hit in hits)
            {
                if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) >= 0.75f ||
                    ShouldIgnoreReactionHit(hit.collider) ||
                    hit.distance >= nearestDistance)
                {
                    continue;
                }

                nearestDistance = hit.distance;
                blockingHit = hit;
            }
            return blockingHit.collider != null;
        }

        private bool ShouldIgnoreReactionHit(Collider candidate)
        {
            if (candidate == null || !candidate.enabled)
            {
                return true;
            }

            Transform candidateTransform = candidate.transform;
            if (candidateTransform == transform ||
                candidateTransform.IsChildOf(transform) ||
                transform.IsChildOf(candidateTransform))
            {
                return true;
            }
            if (playerTransform != null &&
                (candidateTransform == playerTransform ||
                 candidateTransform.IsChildOf(playerTransform)))
            {
                return true;
            }

            ShowcaseRunnerAgent candidateRunner =
                candidate.GetComponentInParent<ShowcaseRunnerAgent>();
            if (cargoSweepReactionProtected && candidateRunner != null &&
                cargoSweepIgnoredRunnerIds.Contains(
                    candidateRunner.GetInstanceID()))
            {
                return true;
            }

            PrototypeInteractionTarget source =
                activeObjectImpact.IsValid
                    ? activeObjectImpact.Context.SourceObject
                    : null;
            return source != null &&
                   (candidateTransform == source.transform ||
                    candidateTransform.IsChildOf(source.transform) ||
                    source.transform.IsChildOf(candidateTransform));
        }

        private void CompleteReactionDisplacement()
        {
            reactionDisplacementPending = false;
            reactionDisplacementActive = false;
            cargoSweepReactionProtected = false;
            cargoSweepIgnoredRunnerIds.Clear();
            currentReactionVelocity = Vector3.zero;
            activeObjectImpact = default;
            if (!deathMomentumActive)
            {
                return;
            }

            deathMomentumActive = false;
            CompleteDeathSettlement();
        }

        private void ClearReactionDisplacement()
        {
            CompleteCargoSweepCarry();
            pendingReactionDirection = Vector3.zero;
            pendingReactionDistance = 0f;
            pendingReactionDuration = 0f;
            reactionDistanceApplied = 0f;
            reactionElapsed = 0f;
            reactionDisplacementPending = false;
            reactionDisplacementActive = false;
            cargoSweepReactionProtected = false;
            cargoSweepIgnoredRunnerIds.Clear();
            activeObjectImpact = default;
            currentReactionVelocity = Vector3.zero;
            deathMomentumActive = false;
        }

        private void CancelAnimationBoundState()
        {
            if (!cargoSweepCarryActive && !cargoSweepReactionProtected)
            {
                ClearReactionDisplacement();
            }
            awaitedAnimationCueRevision = 0;
            animationStateAcknowledged = false;
            switch (State)
            {
                case ShowcaseRunnerState.Telegraph:
                case ShowcaseRunnerState.BodyCheck:
                case ShowcaseRunnerState.Shove:
                    CompleteAttack();
                    break;
                case ShowcaseRunnerState.OffBalance:
                case ShowcaseRunnerState.Staggered:
                case ShowcaseRunnerState.KnockedDown:
                case ShowcaseRunnerState.GettingUp:
                    EnterState(ShowcaseRunnerState.Recovering);
                    break;
            }
        }

        private float DistanceToPlayer()
        {
            return playerTransform == null
                ? float.PositiveInfinity
                : Vector3.ProjectOnPlane(
                    playerTransform.position - transform.position,
                    Vector3.up).magnitude;
        }

        private void EnterState(ShowcaseRunnerState state, bool resetElapsed = true)
        {
            if (State == state && !resetElapsed)
            {
                return;
            }
            State = state;
            if (resetElapsed)
            {
                stateElapsed = 0f;
            }
        }

        private ShowcaseRunnerAnimationCue ResolveNextAttackAnimation()
        {
            if (punchesUntilKick > 0)
            {
                punchesUntilKick--;
                return ResolvePunchCue(elite, NextAttackRandom(8));
            }

            ShowcaseRunnerAnimationCue kick = ResolveKickCue(NextAttackRandom(3));
            punchesUntilKick = 2 + NextAttackRandom(2);
            return kick;
        }

        private static ShowcaseRunnerAnimationCue ResolvePunchCue(
            bool eliteRunner,
            int slot)
        {
            if (!eliteRunner)
            {
                return ShowcaseRunnerAnimationCue.Boxing;
            }

            int normalized = Mathf.Abs(slot) % 8;
            return normalized switch
            {
                < 4 => ShowcaseRunnerAnimationCue.Boxing,
                < 6 => ShowcaseRunnerAnimationCue.EliteMutantPunch,
                _ => ShowcaseRunnerAnimationCue.EliteMutantSwiping
            };
        }

        private static ShowcaseRunnerAnimationCue ResolveKickCue(int slot)
        {
            return (Mathf.Abs(slot) % 3) switch
            {
                0 => ShowcaseRunnerAnimationCue.Kicking,
                1 => ShowcaseRunnerAnimationCue.Kicking2,
                _ => ShowcaseRunnerAnimationCue.Boxing2
            };
        }

        private static bool IsKickCue(ShowcaseRunnerAnimationCue cue)
        {
            return cue == ShowcaseRunnerAnimationCue.Kicking ||
                cue == ShowcaseRunnerAnimationCue.Kicking2 ||
                cue == ShowcaseRunnerAnimationCue.Boxing2;
        }

        private uint CreateAttackRandomSeed()
        {
            uint hash = 2166136261u;
            string value = string.IsNullOrEmpty(runnerId) ? name : runnerId;
            for (int index = 0; index < value.Length; index++)
            {
                hash ^= value[index];
                hash *= 16777619u;
            }
            hash ^= elite ? 0x9E3779B9u : 0x85EBCA6Bu;
            return hash != 0u ? hash : 0xA341316Cu;
        }

        private void ResetAttackCadence()
        {
            attackRandomState = CreateAttackRandomSeed();
            punchesUntilKick = 2 + NextAttackRandom(2);
        }

        private int NextAttackRandom(int maxExclusive)
        {
            if (maxExclusive <= 1)
            {
                return 0;
            }
            uint state = attackRandomState != 0u
                ? attackRandomState
                : 0xA341316Cu;
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            attackRandomState = state;
            return (int)(state % (uint)maxExclusive);
        }

        private void EnterDeath()
        {
            EnterDeath(default);
        }

        private void EnterDeath(ShowcaseObjectImpactEvaluation killingImpact)
        {
            ClearReactionDisplacement();
            boneCombatRig?.CloseAttackWindow();
            InterruptAttack();
            SetAnimationCue(health.AcceptedDamageCount % 2 == 0
                ? ShowcaseRunnerAnimationCue.Death2
                : ShowcaseRunnerAnimationCue.Death1);
            EnterState(ShowcaseRunnerState.Dead);
            bool preserveChairMomentum =
                killingImpact.IsValid &&
                killingImpact.Context.WeightClass == PrototypeWeightClass.Medium &&
                killingImpact.DisplacementDistance > 0f &&
                (killingImpact.Severity ==
                     ShowcaseObjectImpactSeverity.Heavy ||
                 killingImpact.Context.AttackOriginType ==
                     ShowcaseObjectImpactAttackOriginType.
                         CargoStrapMediumSwing);
            if (preserveChairMomentum)
            {
                QueueReactionDisplacement(
                    killingImpact.Context.PrimaryImpactDirection,
                    killingImpact.DisplacementDistance,
                    killingImpact.DisplacementDuration,
                    killingImpact,
                    true,
                    true);
                return;
            }

            CompleteDeathSettlement();
        }

        private void CompleteDeathSettlement()
        {
            if (runnerCollider != null)
            {
                runnerCollider.enabled = false;
            }
            if (body != null && !body.isKinematic)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }

        private void SetAnimationCue(ShowcaseRunnerAnimationCue cue)
        {
            AnimationCue = cue;
            AnimationCueRevision++;
            awaitedAnimationCueRevision = AnimationCueRevision;
            animationRequestedAt = Time.time;
            animationStateAcknowledged = false;
        }

        private void CaptureBaseline()
        {
            baselinePosition = transform.position;
            baselineRotation = transform.rotation;
        }
    }
}
