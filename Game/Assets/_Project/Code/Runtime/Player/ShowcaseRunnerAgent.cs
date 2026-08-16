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
        private const float PlayerStandoffDistance = 0.81f;
        private const float TelegraphSettleSeconds = 0.12f;
        private const float EliteJumpLungeSpeed = 4.25f;
        private const float EliteJumpMaximumTravel = 1.85f;
        private const float PostHitCooldown = 0.65f;
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
        private ShowcaseBoneCombatRig boneCombatRig;

        [SerializeField, Min(0.1f)]
        private float approachSpeed = 2.7f;

        [SerializeField, Min(0.1f)]
        private float bodyCheckSpeed = 6.5f;

        [SerializeField, Min(0.5f)]
        private float activationRange = 11f;

        [SerializeField, Min(0.5f)]
        private float attackRange = 1.25f;

        [SerializeField, Min(0.1f)]
        private float attackCooldown = 1.25f;

        private Vector3 baselinePosition;
        private Quaternion baselineRotation;
        private float stateElapsed;
        private float cooldownRemaining;
        private int attackSequence;
        private bool hitApplied;
        private uint activeAttackSequence;
        private int attackAnimationVariant;
        private Collider runnerCollider;
        private bool getupUsesBackVariant;
        private const float AnimatorStateEntryTimeout = 0.15f;
        private float animationRequestedAt;
        private bool animationStateAcknowledged;
        private uint awaitedAnimationCueRevision;
        private bool lungeWindowOpen;
        private Vector3 pendingReactionDirection;
        private float pendingReactionDistance;
        private float pendingReactionDuration;
        private float reactionDistanceApplied;
        private float reactionElapsed;
        private bool reactionDisplacementPending;
        private bool reactionDisplacementActive;
        private float lungeTravelDistance;

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

        public ShowcaseCombatHealth Health => health;

        public ShowcaseRunnerAnimationCue AnimationCue { get; private set; }

        public uint AnimationCueRevision { get; private set; }

        public uint ActiveAttackSequence => activeAttackSequence;

        public float StateElapsed => stateElapsed;
        public bool CanOpenAttackWindow =>
            !IsDead && (State == ShowcaseRunnerState.BodyCheck ||
                        State == ShowcaseRunnerState.Shove);

        public Vector3 BaselinePosition => baselinePosition;

        public int AttackCommitCount { get; private set; }
        public float CommandedPlanarSpeed { get; private set; }
        public bool IsAttackWindowOpen => boneCombatRig != null &&
            boneCombatRig.IsAttackWindowOpen;

        private void Awake()
        {
            body ??= GetComponent<Rigidbody>();
            runnerCollider = GetComponent<Collider>();
            boneCombatRig ??= GetComponent<ShowcaseBoneCombatRig>();
            ConfigurePlayerCollisionPair();
            CaptureBaseline();
        }

        private void FixedUpdate()
        {
            CommandedPlanarSpeed = 0f;
            if (coordinator == null || playerTransform == null || body == null || IsDead ||
                !coordinator.CanSimulatePressure())
            {
                return;
            }

            float deltaTime = Time.fixedDeltaTime;
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
            AdvanceReactionDisplacement(deltaTime);
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
                    if (stateElapsed >= TelegraphSettleSeconds)
                    {
                        CommitNextAttack();
                    }
                    return;
                case ShowcaseRunnerState.BodyCheck:
                    if (!IsAttackWindowOpen)
                    {
                        FacePlayer(deltaTime);
                    }
                    if (lungeWindowOpen &&
                        AnimationCue == ShowcaseRunnerAnimationCue.EliteJumpAttack &&
                        lungeTravelDistance < EliteJumpMaximumTravel)
                    {
                        lungeTravelDistance += AdvanceTowardPlayer(
                            EliteJumpLungeSpeed,
                            deltaTime);
                    }
                    if (stateElapsed >= 4f)
                    {
                        Debug.LogError(
                            "RUNNER ATTACK ANIMATION EVENT TIMEOUT; DAMAGE WAS NOT FORCED.",
                            this);
                        CompleteAttack();
                    }
                    return;
                case ShowcaseRunnerState.Shove:
                    if (!IsAttackWindowOpen)
                    {
                        FacePlayer(deltaTime);
                    }
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

            float distance = DistanceToPlayer();
            if (distance > activationRange)
            {
                EnterState(ShowcaseRunnerState.Patrol);
                return;
            }

            if (distance > attackRange)
            {
                EnterState(ShowcaseRunnerState.Approach, false);
                AdvanceTowardPlayer(approachSpeed, deltaTime);
                return;
            }

            if (cooldownRemaining <= 0f && coordinator.TryAcquireAttackToken(this))
            {
                EnterState(ShowcaseRunnerState.Telegraph);
            }
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
            bodyCheckSpeed = Mathf.Max(0.1f, authoredBodyCheckSpeed);
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
            attackAnimationVariant = 0;
            getupUsesBackVariant = false;
            animationRequestedAt = 0f;
            animationStateAcknowledged = false;
            awaitedAnimationCueRevision = 0;
            lungeWindowOpen = false;
            lungeTravelDistance = 0f;
            ClearReactionDisplacement();
            AttackCommitCount = 0;
            CommandedPlanarSpeed = 0f;
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
            bool accepted = health.TryApplyDamage(new ShowcaseDamageRequest(
                ShowcaseCombatTeam.Player,
                "CHR00",
                authoredAttackSequence,
                damage,
                reaction,
                impactDirection));
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
                    QueueReactionDisplacement(impactDirection, 0.1f, 0.18f);
                    EnterState(ShowcaseRunnerState.OffBalance);
                    break;
                case ShowcaseHitReaction.Body:
                    SetAnimationCue(ShowcaseRunnerAnimationCue.HitBody);
                    QueueReactionDisplacement(impactDirection, 0.15f, 0.2f);
                    EnterState(ShowcaseRunnerState.Staggered);
                    break;
                case ShowcaseHitReaction.Rib:
                    SetAnimationCue(ShowcaseRunnerAnimationCue.HitRib);
                    QueueReactionDisplacement(impactDirection, 0.15f, 0.2f);
                    EnterState(ShowcaseRunnerState.Staggered);
                    break;
                case ShowcaseHitReaction.Stomach:
                    SetAnimationCue(ShowcaseRunnerAnimationCue.HitStomach);
                    QueueReactionDisplacement(impactDirection, 0.15f, 0.2f);
                    EnterState(ShowcaseRunnerState.Staggered);
                    break;
                default:
                    getupUsesBackVariant = Vector3.Dot(transform.forward, impactDirection) < 0f;
                    SetAnimationCue(getupUsesBackVariant
                        ? ShowcaseRunnerAnimationCue.KnockdownBack
                        : ShowcaseRunnerAnimationCue.KnockdownFront);
                    QueueReactionDisplacement(impactDirection, 0.55f, 0.24f);
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
            lungeWindowOpen = isOpen &&
                State == ShowcaseRunnerState.BodyCheck &&
                AnimationCue == ShowcaseRunnerAnimationCue.EliteJumpAttack;
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
                AnimationCue == ShowcaseRunnerAnimationCue.EliteMutantPunch ||
                AnimationCue == ShowcaseRunnerAnimationCue.EliteMutantSwiping ||
                AnimationCue == ShowcaseRunnerAnimationCue.EliteJumpAttack;
            bool accepted = coordinator.ApplyPlayerAttackFromBoneContact(
                this,
                actualVictim,
                heavy ? 30 : 20,
                heavy,
                heavy ? 4.2f : 3.2f,
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
                coordinator?.RegisterPhysicalImpact(
                    this,
                    source,
                    collision.relativeVelocity.magnitude);
            }
        }

        private void CommitNextAttack()
        {
            boneCombatRig?.CloseAttackWindow();
            // ShortGrab remains disabled until a visible approved clip exists.
            LastAttackKind = (ShowcaseRunnerAttackKind)(attackSequence % 2);
            attackSequence++;
            hitApplied = false;
            activeAttackSequence++;
            lungeTravelDistance = 0f;
            switch (LastAttackKind)
            {
                case ShowcaseRunnerAttackKind.BodyCheck:
                    AttackCommitCount++;
                    SetAnimationCue(ResolveBodyCheckAnimation());
                    EnterState(ShowcaseRunnerState.BodyCheck);
                    break;
                case ShowcaseRunnerAttackKind.Shove:
                    AttackCommitCount++;
                    SetAnimationCue(ResolveShoveAnimation());
                    EnterState(ShowcaseRunnerState.Shove);
                    break;
                default:
                    if (!TryCommitShortGrab())
                    {
                        CompleteAttack();
                    }
                    break;
            }
        }

        private void CompleteAttack()
        {
            boneCombatRig?.CloseAttackWindow();
            coordinator?.ReleaseAttackToken(this);
            cooldownRemaining = attackCooldown;
            EnterState(ShowcaseRunnerState.Recovering);
        }

        private void InterruptAttack()
        {
            boneCombatRig?.CloseAttackWindow();
            coordinator?.ReleaseAttackToken(this);
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
            float distance = Mathf.Min(
                speed * deltaTime,
                Mathf.Max(0f, planarDistance - PlayerStandoffDistance));
            if (distance <= 0.0001f)
            {
                CommandedPlanarSpeed = 0f;
                return 0f;
            }
            if (body.SweepTest(direction, out RaycastHit hit, distance + 0.08f,
                    QueryTriggerInteraction.Ignore) &&
                hit.transform != playerTransform &&
                !hit.transform.IsChildOf(playerTransform))
            {
                CommandedPlanarSpeed = 0f;
                return 0f;
            }

            body.MovePosition(body.position + direction * distance);
            body.MoveRotation(Quaternion.RotateTowards(
                body.rotation,
                Quaternion.LookRotation(direction, Vector3.up),
                540f * deltaTime));
            return distance;
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
                body.MoveRotation(Quaternion.RotateTowards(
                    body.rotation,
                    Quaternion.LookRotation(direction.normalized, Vector3.up),
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

        private void QueueReactionDisplacement(Vector3 direction, float distance, float duration)
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
                bool blocked = body.SweepTest(
                    pendingReactionDirection,
                    out RaycastHit hit,
                    stepDistance + 0.02f,
                    QueryTriggerInteraction.Ignore) &&
                    hit.transform != playerTransform &&
                    (playerTransform == null || !hit.transform.IsChildOf(playerTransform));
                if (blocked)
                {
                    reactionDisplacementActive = false;
                    return;
                }

                body.MovePosition(body.position + pendingReactionDirection * stepDistance);
                reactionDistanceApplied += stepDistance;
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

        private void CancelAnimationBoundState()
        {
            lungeWindowOpen = false;
            ClearReactionDisplacement();
            awaitedAnimationCueRevision = 0;
            animationStateAcknowledged = false;
            switch (State)
            {
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

        private ShowcaseRunnerAnimationCue ResolveBodyCheckAnimation()
        {
            attackAnimationVariant++;
            if (elite)
            {
                return attackAnimationVariant % 2 == 0
                    ? ShowcaseRunnerAnimationCue.EliteMutantSwiping
                    : ShowcaseRunnerAnimationCue.EliteJumpAttack;
            }
            return attackAnimationVariant % 2 == 0
                ? ShowcaseRunnerAnimationCue.Kicking2
                : ShowcaseRunnerAnimationCue.Kicking;
        }

        private ShowcaseRunnerAnimationCue ResolveShoveAnimation()
        {
            attackAnimationVariant++;
            if (elite && attackAnimationVariant % 2 != 0)
            {
                return ShowcaseRunnerAnimationCue.EliteMutantPunch;
            }
            return attackAnimationVariant % 2 == 0
                ? ShowcaseRunnerAnimationCue.Boxing2
                : ShowcaseRunnerAnimationCue.Boxing;
        }

        private void EnterDeath()
        {
            lungeWindowOpen = false;
            ClearReactionDisplacement();
            boneCombatRig?.CloseAttackWindow();
            InterruptAttack();
            SetAnimationCue(health.AcceptedDamageCount % 2 == 0
                ? ShowcaseRunnerAnimationCue.Death2
                : ShowcaseRunnerAnimationCue.Death1);
            EnterState(ShowcaseRunnerState.Dead);
            if (runnerCollider != null)
            {
                runnerCollider.enabled = false;
            }
            if (body != null)
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
