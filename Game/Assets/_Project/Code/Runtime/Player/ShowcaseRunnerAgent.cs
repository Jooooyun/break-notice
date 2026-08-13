using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
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

        public string RunnerId => runnerId;

        public ShowcaseRunnerState State { get; private set; } = ShowcaseRunnerState.Patrol;

        public ShowcaseRunnerAttackKind LastAttackKind { get; private set; }

        public bool CanReceiveCombatTag =>
            State == ShowcaseRunnerState.OffBalance ||
            State == ShowcaseRunnerState.Staggered;

        public bool IsIncapacitated => CanReceiveCombatTag ||
            State == ShowcaseRunnerState.GrabCommitted;

        public float StateElapsed => stateElapsed;

        public Vector3 BaselinePosition => baselinePosition;

        public int AttackCommitCount { get; private set; }

        private void Awake()
        {
            body ??= GetComponent<Rigidbody>();
            CaptureBaseline();
        }

        private void FixedUpdate()
        {
            if (coordinator == null || playerTransform == null || body == null ||
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
                case ShowcaseRunnerState.Telegraph:
                    FacePlayer(deltaTime);
                    if (stateElapsed >= 0.45f)
                    {
                        CommitNextAttack();
                    }
                    return;
                case ShowcaseRunnerState.BodyCheck:
                    AdvanceTowardPlayer(bodyCheckSpeed, deltaTime);
                    if (!hitApplied && DistanceToPlayer() <= 1.25f)
                    {
                        hitApplied = true;
                        coordinator.ApplyPlayerPressure(this, 4.2f);
                    }
                    if (stateElapsed >= 0.42f)
                    {
                        CompleteAttack();
                    }
                    return;
                case ShowcaseRunnerState.Shove:
                    FacePlayer(deltaTime);
                    if (!hitApplied && stateElapsed >= 0.14f && DistanceToPlayer() <= 1.7f)
                    {
                        hitApplied = true;
                        coordinator.ApplyPlayerPressure(this, 3.2f);
                    }
                    if (stateElapsed >= 0.34f)
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
            float authoredBodyCheckSpeed = 6.5f)
        {
            runnerId = string.IsNullOrWhiteSpace(authoredRunnerId)
                ? name
                : authoredRunnerId;
            coordinator = pressure;
            body = authoredBody != null ? authoredBody : GetComponent<Rigidbody>();
            playerTransform = player;
            approachSpeed = Mathf.Max(0.1f, authoredApproachSpeed);
            bodyCheckSpeed = Mathf.Max(0.1f, authoredBodyCheckSpeed);
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
            InterruptAttack();
            EnterState(medium
                ? ShowcaseRunnerState.Staggered
                : ShowcaseRunnerState.OffBalance);
        }

        public void ReceivePlayerShove(Vector3 playerForward)
        {
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
            AttackCommitCount = 0;
        }

        private void OnCollisionEnter(Collision collision)
        {
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
            switch (LastAttackKind)
            {
                case ShowcaseRunnerAttackKind.BodyCheck:
                    AttackCommitCount++;
                    EnterState(ShowcaseRunnerState.BodyCheck);
                    break;
                case ShowcaseRunnerAttackKind.Shove:
                    AttackCommitCount++;
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

        private void CaptureBaseline()
        {
            baselinePosition = transform.position;
            baselineRotation = transform.rotation;
        }
    }
}
