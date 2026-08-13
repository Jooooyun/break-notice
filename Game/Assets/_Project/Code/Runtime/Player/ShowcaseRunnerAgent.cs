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

        [SerializeField, Min(0.1f)]
        private float approachSpeed = 2.7f;

        [SerializeField, Min(0.1f)]
        private float bodyCheckSpeed = 6.5f;

        [SerializeField, Min(0.5f)]
        private float activationRange = 11f;

        [SerializeField, Min(0.5f)]
        private float attackRange = 1.8f;

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

        public Vector3 BaselinePosition => baselinePosition;

        public int AttackCommitCount { get; private set; }

        private void Awake()
        {
            body ??= GetComponent<Rigidbody>();
            runnerCollider = GetComponent<Collider>();
            CaptureBaseline();
        }

        private void FixedUpdate()
        {
            if (coordinator == null || playerTransform == null || body == null || IsDead ||
                !coordinator.CanSimulatePressure())
            {
                return;
            }

            float deltaTime = Time.fixedDeltaTime;
            stateElapsed += deltaTime;
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - deltaTime);
            switch (State)
            {
                case ShowcaseRunnerState.OffBalance:
                    if (stateElapsed >= 0.8f)
                    {
                        EnterState(ShowcaseRunnerState.Recovering);
                    }
                    return;
                case ShowcaseRunnerState.Staggered:
                    if (stateElapsed >= 1.55f)
                    {
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
                    if (stateElapsed >= 1.35f)
                    {
                        SetAnimationCue(LastAttackKind == ShowcaseRunnerAttackKind.BodyCheck
                            ? ShowcaseRunnerAnimationCue.GetupBack
                            : ShowcaseRunnerAnimationCue.GetupFront);
                        EnterState(ShowcaseRunnerState.Recovering);
                    }
                    return;
                case ShowcaseRunnerState.Dead:
                    return;
                case ShowcaseRunnerState.Telegraph:
                    FacePlayer(deltaTime);
                    if (stateElapsed >= 0.45f)
                    {
                        CommitNextAttack();
                    }
                    return;
                case ShowcaseRunnerState.BodyCheck:
                    AdvanceTowardPlayer(bodyCheckSpeed, deltaTime);
                    if (!hitApplied && stateElapsed >= 0.82f && DistanceToPlayer() <= 1.45f)
                    {
                        CommitAnimationAttackHit();
                    }
                    if (stateElapsed >= 1.15f)
                    {
                        CompleteAttack();
                    }
                    return;
                case ShowcaseRunnerState.Shove:
                    FacePlayer(deltaTime);
                    if (!hitApplied && stateElapsed >= 0.62f && DistanceToPlayer() <= 1.7f)
                    {
                        CommitAnimationAttackHit();
                    }
                    if (stateElapsed >= 1.0f)
                    {
                        CompleteAttack();
                    }
                    return;
                case ShowcaseRunnerState.GrabCommitted:
                    FacePlayer(deltaTime);
                    if (stateElapsed >= 1.1f)
                    {
                        coordinator.ResolveTimedGrab(this);
                    }
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
            CaptureBaseline();
            ResetToRunBaseline();
        }

        public bool TryCommitShortGrab()
        {
            if (coordinator == null || !coordinator.TryCommitGrab(this))
            {
                return false;
            }

            LastAttackKind = ShowcaseRunnerAttackKind.ShortGrab;
            AttackCommitCount++;
            EnterState(ShowcaseRunnerState.GrabCommitted);
            return true;
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
            MoveBy(direction * 0.7f);
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
            AttackCommitCount = 0;
            AnimationCue = ShowcaseRunnerAnimationCue.None;
            AnimationCueRevision++;
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
            if (health.IsDead)
            {
                EnterDeath();
                return true;
            }

            switch (reaction)
            {
                case ShowcaseHitReaction.Light:
                    SetAnimationCue(ShowcaseRunnerAnimationCue.HitLight);
                    EnterState(ShowcaseRunnerState.OffBalance);
                    break;
                case ShowcaseHitReaction.Body:
                    SetAnimationCue(ShowcaseRunnerAnimationCue.HitBody);
                    EnterState(ShowcaseRunnerState.Staggered);
                    break;
                case ShowcaseHitReaction.Rib:
                    SetAnimationCue(ShowcaseRunnerAnimationCue.HitRib);
                    EnterState(ShowcaseRunnerState.Staggered);
                    break;
                case ShowcaseHitReaction.Stomach:
                    SetAnimationCue(ShowcaseRunnerAnimationCue.HitStomach);
                    EnterState(ShowcaseRunnerState.Staggered);
                    break;
                default:
                    SetAnimationCue(ShowcaseRunnerAnimationCue.HitBigHead);
                    MoveBy(impactDirection * 0.85f);
                    EnterState(ShowcaseRunnerState.KnockedDown);
                    break;
            }
            return true;
        }

        public void CommitAnimationAttackHit()
        {
            if (hitApplied || IsDead || coordinator == null)
            {
                return;
            }

            float allowedRange = State == ShowcaseRunnerState.BodyCheck ? 1.55f : 1.75f;
            if (DistanceToPlayer() > allowedRange)
            {
                return;
            }

            hitApplied = true;
            bool heavy = AnimationCue == ShowcaseRunnerAnimationCue.Kicking ||
                AnimationCue == ShowcaseRunnerAnimationCue.Kicking2 ||
                AnimationCue == ShowcaseRunnerAnimationCue.EliteMutantPunch ||
                AnimationCue == ShowcaseRunnerAnimationCue.EliteMutantSwiping ||
                AnimationCue == ShowcaseRunnerAnimationCue.EliteJumpAttack;
            coordinator.ApplyPlayerAttack(
                this,
                heavy ? 30 : 20,
                heavy,
                heavy ? 4.2f : 3.2f,
                activeAttackSequence);
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
            LastAttackKind = (ShowcaseRunnerAttackKind)(attackSequence % 3);
            attackSequence++;
            hitApplied = false;
            activeAttackSequence++;
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
            coordinator?.ReleaseAttackToken(this);
            cooldownRemaining = attackCooldown;
            EnterState(ShowcaseRunnerState.Recovering);
        }

        private void InterruptAttack()
        {
            coordinator?.ReleaseAttackToken(this);
        }

        private void AdvanceTowardPlayer(float speed, float deltaTime)
        {
            Vector3 direction = Vector3.ProjectOnPlane(
                playerTransform.position - body.position,
                Vector3.up);
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            direction.Normalize();
            float distance = speed * deltaTime;
            if (body.SweepTest(direction, out RaycastHit hit, distance + 0.08f,
                    QueryTriggerInteraction.Ignore) &&
                hit.transform != playerTransform &&
                !hit.transform.IsChildOf(playerTransform))
            {
                return;
            }

            body.MovePosition(body.position + direction * distance);
            body.MoveRotation(Quaternion.RotateTowards(
                body.rotation,
                Quaternion.LookRotation(direction, Vector3.up),
                540f * deltaTime));
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
        }

        private void CaptureBaseline()
        {
            baselinePosition = transform.position;
            baselineRotation = transform.rotation;
        }
    }
}
