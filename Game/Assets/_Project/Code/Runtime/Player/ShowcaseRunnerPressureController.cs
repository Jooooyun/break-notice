using System;
using System.Collections.Generic;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Encounter;
using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    public readonly struct ShowcaseCombatContactResult
    {
        public ShowcaseCombatContactResult(
            Transform attacker,
            Transform victim,
            bool accepted,
            Vector3 worldPoint)
            : this(
                attacker,
                victim,
                accepted,
                worldPoint,
                default,
                0.05f)
        {
        }

        public ShowcaseCombatContactResult(
            Transform attacker,
            Transform victim,
            bool accepted,
            Vector3 worldPoint,
            ShowcaseObjectImpactEvaluation objectImpact)
            : this(
                attacker,
                victim,
                accepted,
                worldPoint,
                objectImpact,
                objectImpact.IsValid ? objectImpact.HitStopSeconds : 0.05f)
        {
        }

        private ShowcaseCombatContactResult(
            Transform attacker,
            Transform victim,
            bool accepted,
            Vector3 worldPoint,
            ShowcaseObjectImpactEvaluation objectImpact,
            float hitStopSeconds)
        {
            Attacker = attacker;
            Victim = victim;
            Accepted = accepted;
            WorldPoint = worldPoint;
            ObjectImpact = objectImpact;
            HitStopSeconds = Mathf.Max(0f, hitStopSeconds);
            CameraImpulseMultiplier = objectImpact.IsValid
                ? objectImpact.CameraImpulseMultiplier
                : 1f;
        }

        public Transform Attacker { get; }
        public Transform Victim { get; }
        public bool Accepted { get; }
        public Vector3 WorldPoint { get; }
        public ShowcaseObjectImpactEvaluation ObjectImpact { get; }
        public bool HasObjectImpact => ObjectImpact.IsValid;
        public ShowcaseObjectImpactSeverity ImpactSeverity =>
            HasObjectImpact
                ? ObjectImpact.Severity
                : ShowcaseObjectImpactSeverity.Medium;
        public float HitStopSeconds { get; }
        public float CameraImpulseMultiplier { get; }
    }

    public readonly struct ShowcaseSecondaryImpactResult
    {
        public ShowcaseSecondaryImpactResult(
            ShowcaseSecondaryImpactKind kind,
            ShowcaseRunnerAgent primaryRunner,
            ShowcaseRunnerAgent secondaryRunner,
            Collider obstacle,
            ShowcaseObjectImpactEvaluation objectImpact,
            int secondaryDamage,
            bool secondaryDamageAccepted)
        {
            Kind = kind;
            PrimaryRunner = primaryRunner;
            SecondaryRunner = secondaryRunner;
            Obstacle = obstacle;
            ObjectImpact = objectImpact;
            SecondaryDamage = Mathf.Max(0, secondaryDamage);
            SecondaryDamageAccepted = secondaryDamageAccepted;
        }

        public ShowcaseSecondaryImpactKind Kind { get; }
        public ShowcaseRunnerAgent PrimaryRunner { get; }
        public ShowcaseRunnerAgent SecondaryRunner { get; }
        public Collider Obstacle { get; }
        public ShowcaseObjectImpactEvaluation ObjectImpact { get; }
        public int SecondaryDamage { get; }
        public bool SecondaryDamageAccepted { get; }
    }

    public enum ShowcaseRunnerState
    {
        Patrol,
        Approach,
        Telegraph,
        BodyCheck,
        Shove,
        GrabCommitted,
        OffBalance,
        Staggered,
        Recovering,
        KnockedDown,
        GettingUp,
        Dead
    }

    public enum ShowcaseRunnerAttackKind
    {
        BodyCheck,
        Shove,
        ShortGrab
    }

    public enum ShowcaseElitePressurePriority
    {
        Default,
        PlayerCharging,
        PlayerStationary,
        PlayerFocusingAnotherEnemy
    }

    public readonly struct ShowcaseAttackBudgetSnapshot
    {
        public ShowcaseAttackBudgetSnapshot(
            int enemyCount,
            int normalCapacity,
            int eliteCapacity,
            int totalCapacity,
            int activeNormalAttackers,
            int activeEliteAttackers,
            bool elitePressureActive)
        {
            EnemyCount = Mathf.Max(0, enemyCount);
            NormalCapacity = Mathf.Max(0, normalCapacity);
            EliteCapacity = Mathf.Max(0, eliteCapacity);
            TotalCapacity = Mathf.Max(1, totalCapacity);
            ActiveNormalAttackers = Mathf.Max(0, activeNormalAttackers);
            ActiveEliteAttackers = Mathf.Max(0, activeEliteAttackers);
            ElitePressureActive = elitePressureActive;
        }

        public int EnemyCount { get; }
        public int NormalCapacity { get; }
        public int EliteCapacity { get; }
        public int TotalCapacity { get; }
        public int ActiveNormalAttackers { get; }
        public int ActiveEliteAttackers { get; }
        public int ActiveAttackers =>
            ActiveNormalAttackers + ActiveEliteAttackers;
        public bool ElitePressureActive { get; }
    }

    [DefaultExecutionOrder(-30)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseRunnerPressureController : MonoBehaviour
    {
        private const int VisibilityHitCapacity = 32;
        private const float PlayerMeleeFacingDot = 0.7f;
        private const int LightThrowBaseDamage = 20;
        private const int MediumThrowBaseDamage = 30;
        private const int MediumSwingBaseDamage = 25;
        private const float CargoTargetAimPadding = 0.35f;

        private readonly struct SecondaryImpactPairKey :
            IEquatable<SecondaryImpactPairKey>
        {
            public SecondaryImpactPairKey(
                PrototypeInteractionTarget source,
                uint attackSequence,
                ShowcaseRunnerAgent first,
                ShowcaseRunnerAgent second)
            {
                SourceInstanceId = source != null ? source.GetInstanceID() : 0;
                AttackSequence = attackSequence;
                int firstId = first != null ? first.GetInstanceID() : 0;
                int secondId = second != null ? second.GetInstanceID() : 0;
                FirstRunnerId = Mathf.Min(firstId, secondId);
                SecondRunnerId = Mathf.Max(firstId, secondId);
            }

            private int SourceInstanceId { get; }
            private uint AttackSequence { get; }
            private int FirstRunnerId { get; }
            private int SecondRunnerId { get; }

            public bool Equals(SecondaryImpactPairKey other)
            {
                return SourceInstanceId == other.SourceInstanceId &&
                       AttackSequence == other.AttackSequence &&
                       FirstRunnerId == other.FirstRunnerId &&
                       SecondRunnerId == other.SecondRunnerId;
            }

            public override bool Equals(object obj)
            {
                return obj is SecondaryImpactPairKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(
                    SourceInstanceId,
                    AttackSequence,
                    FirstRunnerId,
                    SecondRunnerId);
            }
        }

        [SerializeField]
        private PrototypeInteractionStateOwner interactionOwner;

        [SerializeField]
        private Transform playerTransform;

        [SerializeField]
        private ThirdPersonLocomotion playerLocomotion;

        [SerializeField]
        private Transform allyTransform;

        [SerializeField]
        private ShowcaseMobileAllyController mobileAllyController;

        [SerializeField]
        private ShowcaseTeamContextController teamContextController;

        [SerializeField]
        private AuthoredEncounterResetController resetController;

        [SerializeField]
        private ShowcaseRunnerAgent[] runners = Array.Empty<ShowcaseRunnerAgent>();

        [SerializeField]
        private ShowcaseRunnerAgent[] validationRunners =
            Array.Empty<ShowcaseRunnerAgent>();

        [Header("Attack Budget")]
        [SerializeField, Min(1)]
        private int normalSingleSlotEnemyMaximum = 2;

        [SerializeField, Min(2)]
        private int normalDoubleSlotEnemyMaximum = 4;

        [SerializeField, Range(1, 3)]
        private int maximumNormalAttackers = 3;

        [SerializeField, Range(0, 1)]
        private int eliteAttackSlots = 1;

        [SerializeField, Range(1, 3)]
        private int maximumSimultaneousHardContacts = 3;

        [Header("Attack Start Cadence")]
        [SerializeField, Range(0.15f, 0.30f)]
        private float globalAttackStartGapMinimum = 0.18f;

        [SerializeField, Range(0.15f, 0.30f)]
        private float globalAttackStartGapMaximum = 0.26f;

        [SerializeField, Range(0.8f, 1.4f)]
        private float actorAttackCooldownMinimum = 0.85f;

        [SerializeField, Range(0.8f, 1.4f)]
        private float actorAttackCooldownMaximum = 1.25f;

        [SerializeField, Range(0.20f, 0.35f)]
        private float normalTelegraphMinimum = 0.22f;

        [SerializeField, Range(0.20f, 0.35f)]
        private float normalTelegraphMaximum = 0.32f;

        [SerializeField, Range(0.35f, 0.55f)]
        private float eliteTelegraphMinimum = 0.40f;

        [SerializeField, Range(0.35f, 0.55f)]
        private float eliteTelegraphMaximum = 0.50f;

        [Header("Pressure Ramp")]
        [SerializeField, Range(10f, 15f)]
        private float openingSingleAttackerSeconds = 12f;

        [SerializeField, Min(12f)]
        private float fullPressureSeconds = 24f;

        [Header("Pressure Ring")]
        [SerializeField, Min(0.9f)]
        private float normalPressureRingRadius = 1.22f;

        [SerializeField, Min(0.9f)]
        private float elitePressureRingRadius = 1.08f;

        [SerializeField, Range(5f, 30f)]
        private float pressureOrbitDegreesPerSecond = 16f;

        [SerializeField, Range(0.5f, 1.2f)]
        private float pressureMovementSpeedScale = 0.85f;

        [SerializeField, Min(0.5f)]
        private float stationaryPrioritySeconds = 2f;

        [SerializeField, Min(1f)]
        private float rescueRange = 7f;

        [SerializeField, Min(1f)]
        private float combatTagRange = 7f;

        [SerializeField, Min(0.5f)]
        private float playerMeleeRange = 0.9f;

        [SerializeField, Min(0.1f)]
        private float playerMeleeCooldown = 0.45f;

        [SerializeField]
        private ShowcaseObjectImpactTuning objectImpactTuning = new();

        [SerializeField, Range(0f, 1f)]
        private float secondaryEnemyDamageMultiplier = 0.25f;

        [SerializeField, Min(0f)]
        private float secondaryEnemyDisplacementDistance = 0.22f;

        [SerializeField, Min(0.01f)]
        private float secondaryEnemyDisplacementDuration = 0.18f;

        [SerializeField, Min(0.01f)]
        private float secondaryStunSeconds = 0.35f;

        private readonly RaycastHit[] visibilityHits =
            new RaycastHit[VisibilityHitCapacity];
        private readonly HashSet<ShowcaseRunnerAgent> normalAttackers = new();
        private readonly HashSet<ShowcaseRunnerAgent> eliteAttackers = new();
        private readonly Dictionary<int, float> nextActorAttackStartTimes = new();
        private ShowcaseRunnerAgent grabbingRunner;
        private int observedResetCount;
        private float nextPlayerMeleeTime;
        private bool externallySuspended;
        private float nextGlobalAttackStartTime;
        private float pressureStartedAt;
        private Vector3 lastObservedPlayerPosition;
        private float playerStationarySince;
        private bool playerPositionObserved;
        private ShowcaseElitePressurePriority elitePressurePriority;
        private readonly HashSet<SecondaryImpactPairKey>
            acceptedSecondaryImpactPairs = new();
        private ShowcaseRunnerAgent[] targetableRunners =
            Array.Empty<ShowcaseRunnerAgent>();

        [SerializeField]
        private ShowcasePlayerCombatController playerCombat;

        [SerializeField]
        private ShowcaseCargoStrapController cargoStrap;

        public ShowcaseRunnerAgent[] Runners => runners;

        public ShowcaseRunnerAgent[] ValidationRunners => validationRunners;

        public ShowcaseRunnerAgent[] TargetableRunners => targetableRunners;

        public ShowcaseRunnerAgent AttackTokenOwner =>
            ResolveFirstActiveAttacker();

        public int ActiveAttackTokenCount =>
            normalAttackers.Count + eliteAttackers.Count;

        public int ActiveNormalAttackCount => normalAttackers.Count;

        public int ActiveEliteAttackCount => eliteAttackers.Count;

        public float PressureStartedAt => pressureStartedAt;

        public float PressureElapsedSeconds =>
            Mathf.Max(0f, Time.time - pressureStartedAt);

        public ShowcaseAttackBudgetSnapshot CurrentAttackBudget =>
            EvaluateAttackBudget(PressureElapsedSeconds);

        public ShowcaseElitePressurePriority ElitePressurePriority =>
            elitePressurePriority;

        public Vector2 GlobalAttackStartGapRange => new(
            Mathf.Min(globalAttackStartGapMinimum, globalAttackStartGapMaximum),
            Mathf.Max(globalAttackStartGapMinimum, globalAttackStartGapMaximum));

        public Vector2 ActorAttackCooldownRange => new(
            Mathf.Min(actorAttackCooldownMinimum, actorAttackCooldownMaximum),
            Mathf.Max(actorAttackCooldownMinimum, actorAttackCooldownMaximum));

        public Vector2 NormalTelegraphRange => new(
            Mathf.Min(normalTelegraphMinimum, normalTelegraphMaximum),
            Mathf.Max(normalTelegraphMinimum, normalTelegraphMaximum));

        public Vector2 EliteTelegraphRange => new(
            Mathf.Min(eliteTelegraphMinimum, eliteTelegraphMaximum),
            Mathf.Max(eliteTelegraphMinimum, eliteTelegraphMaximum));

        public bool PlayerGrabbed => grabbingRunner != null;

        public ShowcaseRunnerAgent GrabbingRunner => grabbingRunner;

        public bool HasCombatTagTarget => ResolveCombatTagTarget() != null;

        public bool HasPlayerMeleeTarget => ResolvePlayerMeleeTarget() != null;

        public int PlayerHitCount { get; private set; }

        public int GrabCount { get; private set; }

        public int RescueCount { get; private set; }

        public int CombatTagCount { get; private set; }

        public int PlayerMeleeCount { get; private set; }

        public int PhysicalImpactCount { get; private set; }

        public int SecondaryImpactCount { get; private set; }

        public int EnemyCollisionCount { get; private set; }

        public int RejectedSecondaryImpactDuplicateCount { get; private set; }

        public ShowcaseObjectImpactEvaluation LastObjectImpactEvaluation
        {
            get;
            private set;
        }

        public ShowcaseSecondaryImpactResult LastSecondaryImpactResult
        {
            get;
            private set;
        }

        public ShowcaseObjectImpactTuning ObjectImpactTuning =>
            objectImpactTuning ??= new ShowcaseObjectImpactTuning();

        public float RescueRange => rescueRange;

        public float CombatTagRange => combatTagRange;

        public float PlayerMeleeRange => playerMeleeRange;

        public bool IsExternallySuspended => externallySuspended;
        public Transform PlayerTransform => playerTransform;
        public ShowcaseCombatHealth PlayerHealth => playerCombat?.Health;

        public event Action<ShowcaseCombatContactResult> CombatContactResolved;
        public event Action<ShowcaseSecondaryImpactResult> SecondaryImpactResolved;
        public event Action<ShowcaseSecondaryImpactResult> EnemyCollisionResolved;
        public event Action BaselineReset;

        private void Awake()
        {
            RebuildTargetableRunners();
            InitializePressureRuntime(Time.time);
            ShowcaseRunnerValidationRosterSpawner.EnsureForManualPlay(this);
        }

        private void Update()
        {
            if (resetController != null && resetController.ResetCount != observedResetCount)
            {
                observedResetCount = resetController.ResetCount;
                ResetToRunBaseline();
            }

            CleanupAttackBudget();
            UpdatePlayerPressureContext();
        }

        public void Configure(
            PrototypeInteractionStateOwner owner,
            Transform player,
            ThirdPersonLocomotion locomotion,
            Transform ally,
            ShowcaseMobileAllyController mobileAlly,
            ShowcaseTeamContextController teamContext,
            AuthoredEncounterResetController reset,
            ShowcaseRunnerAgent[] authoredRunners,
            float authoredRescueRange = 7f,
            float authoredCombatTagRange = 7f,
            float authoredMeleeRange = 2.1f)
        {
            interactionOwner = owner;
            playerTransform = player;
            playerLocomotion = locomotion;
            allyTransform = ally;
            mobileAllyController = mobileAlly;
            teamContextController = teamContext;
            resetController = reset;
            if (authoredRunners == null)
            {
                runners = Array.Empty<ShowcaseRunnerAgent>();
            }
            else
            {
                var validRunners = new List<ShowcaseRunnerAgent>(authoredRunners.Length);
                for (int index = 0; index < authoredRunners.Length; index++)
                {
                    if (authoredRunners[index] != null)
                    {
                        validRunners.Add(authoredRunners[index]);
                    }
                }
                runners = validRunners.ToArray();
            }
            rescueRange = Mathf.Max(1f, authoredRescueRange);
            combatTagRange = Mathf.Max(1f, authoredCombatTagRange);
            playerMeleeRange = Mathf.Min(0.9f, Mathf.Max(0.5f, authoredMeleeRange));
            observedResetCount = resetController != null ? resetController.ResetCount : 0;
            nextPlayerMeleeTime = 0f;
            playerCombat = player != null
                ? player.GetComponent<ShowcasePlayerCombatController>()
                : null;
            cargoStrap = player != null
                ? player.GetComponent<ShowcaseCargoStrapController>()
                : null;
            RebuildTargetableRunners();
            InitializePressureRuntime(Time.time);
        }

        public void ConfigureStationaryValidationRunners(
            ShowcaseRunnerAgent[] authoredValidationRunners)
        {
            validationRunners = FilterDistinctRunners(
                authoredValidationRunners,
                runners);
            RebuildTargetableRunners();
        }

        public void ConfigureRuntimeRunnerRoster(
            ShowcaseRunnerAgent[] runtimeRunners)
        {
            runners = FilterDistinctRunners(runtimeRunners, null);
            RebuildTargetableRunners();
            InitializePressureRuntime(Time.time);
        }

        internal void NotifyBatchDisabledValidationRunner(
            ShowcaseRunnerAgent disabledRunner)
        {
            if (disabledRunner == null || validationRunners == null)
            {
                return;
            }

            for (int index = 0; index < validationRunners.Length; index++)
            {
                if (validationRunners[index] == disabledRunner)
                {
                    validationRunners[index] = null;
                }
            }
            RebuildTargetableRunners();
        }

        private void RebuildTargetableRunners()
        {
            var combined = new List<ShowcaseRunnerAgent>(
                (runners?.Length ?? 0) +
                (validationRunners?.Length ?? 0));
            var seen = new HashSet<int>();
            AppendDistinctRunners(runners, combined, seen);
            AppendDistinctRunners(validationRunners, combined, seen);
            targetableRunners = combined.ToArray();
        }

        private static ShowcaseRunnerAgent[] FilterDistinctRunners(
            ShowcaseRunnerAgent[] candidates,
            ShowcaseRunnerAgent[] exclusions)
        {
            var excludedIds = new HashSet<int>();
            if (exclusions != null)
            {
                for (int index = 0; index < exclusions.Length; index++)
                {
                    if (exclusions[index] != null)
                    {
                        excludedIds.Add(exclusions[index].GetInstanceID());
                    }
                }
            }

            var result = new List<ShowcaseRunnerAgent>(
                candidates?.Length ?? 0);
            var seen = new HashSet<int>();
            if (candidates != null)
            {
                for (int index = 0; index < candidates.Length; index++)
                {
                    ShowcaseRunnerAgent candidate = candidates[index];
                    if (candidate == null)
                    {
                        continue;
                    }
                    int instanceId = candidate.GetInstanceID();
                    if (excludedIds.Contains(instanceId) ||
                        !seen.Add(instanceId))
                    {
                        continue;
                    }
                    result.Add(candidate);
                }
            }
            return result.ToArray();
        }

        private static void AppendDistinctRunners(
            ShowcaseRunnerAgent[] source,
            List<ShowcaseRunnerAgent> destination,
            HashSet<int> seen)
        {
            if (source == null)
            {
                return;
            }
            for (int index = 0; index < source.Length; index++)
            {
                ShowcaseRunnerAgent runner = source[index];
                if (runner != null && seen.Add(runner.GetInstanceID()))
                {
                    destination.Add(runner);
                }
            }
        }

        public void ConfigurePlayerCombat(ShowcasePlayerCombatController combat)
        {
            playerCombat = combat;
        }

        public bool CanSimulatePressure()
        {
            return !externallySuspended && interactionOwner != null &&
                interactionOwner.GlobalState == PrototypeGlobalInteractionState.Playable;
        }

        public void SetExternalSuspended(bool suspended)
        {
            externallySuspended = suspended;
            if (suspended)
            {
                ClearAttackBudget(false);
            }
        }

        public bool TryAcquireAttackToken(ShowcaseRunnerAgent runner)
        {
            return TryAcquireAttackToken(runner, Time.time);
        }

        public bool TryAcquireAttackToken(
            ShowcaseRunnerAgent runner,
            float timestamp)
        {
            CleanupAttackBudget();
            if (IsRunnerActiveAttacker(runner))
            {
                return true;
            }
            if (!CanSimulatePressure() || runner == null || runner.IsDead ||
                !runner.gameObject.activeInHierarchy ||
                runner.SuppressesAutonomousMovement ||
                timestamp < nextGlobalAttackStartTime)
            {
                return false;
            }

            int runnerId = runner.GetInstanceID();
            if (nextActorAttackStartTimes.TryGetValue(
                    runnerId,
                    out float nextActorStart) &&
                timestamp < nextActorStart)
            {
                return false;
            }

            ShowcaseAttackBudgetSnapshot budget = EvaluateAttackBudget(
                Mathf.Max(0f, timestamp - pressureStartedAt));
            if (budget.ActiveAttackers >= budget.TotalCapacity)
            {
                return false;
            }

            HashSet<ShowcaseRunnerAgent> slot = runner.IsElite
                ? eliteAttackers
                : normalAttackers;
            int slotCapacity = runner.IsElite
                ? budget.EliteCapacity
                : budget.NormalCapacity;
            if (slotCapacity <= 0 || slot.Count >= slotCapacity)
            {
                return false;
            }

            slot.Add(runner);
            nextGlobalAttackStartTime = timestamp +
                ResolveGlobalAttackStartGap(runner);
            return true;
        }

        public void ReleaseAttackToken(ShowcaseRunnerAgent runner)
        {
            ReleaseAttackToken(runner, Time.time);
        }

        public void ReleaseAttackToken(
            ShowcaseRunnerAgent runner,
            float timestamp)
        {
            if (runner == null)
            {
                return;
            }

            bool removed = normalAttackers.Remove(runner) |
                eliteAttackers.Remove(runner);
            if (removed)
            {
                nextActorAttackStartTimes[runner.GetInstanceID()] = timestamp +
                    ResolveActorAttackCooldown(runner);
            }
        }

        public bool IsRunnerActiveAttacker(ShowcaseRunnerAgent runner)
        {
            return runner != null &&
                (normalAttackers.Contains(runner) ||
                 eliteAttackers.Contains(runner));
        }

        public bool CanRunnerAutonomouslyAttack(ShowcaseRunnerAgent runner)
        {
            if (runner == null || runner.IsDead ||
                runner.SuppressesAutonomousMovement)
            {
                return false;
            }
            if (!runner.IsElite || PressureElapsedSeconds >= fullPressureSeconds)
            {
                return true;
            }

            // An Elite may still carry an isolated test/fallback encounter, but
            // it does not steal the opening slot while a Normal threat is live.
            return !HasLivingEnabledNormalRunner();
        }

        public bool ShouldMaintainPressureRing(ShowcaseRunnerAgent runner)
        {
            if (!CanSimulatePressure() || runner == null ||
                IsRunnerActiveAttacker(runner))
            {
                return false;
            }
            if (!CanRunnerAutonomouslyAttack(runner) ||
                Time.time < nextGlobalAttackStartTime)
            {
                return true;
            }

            if (nextActorAttackStartTimes.TryGetValue(
                    runner.GetInstanceID(),
                    out float nextActorStart) &&
                Time.time < nextActorStart)
            {
                return true;
            }

            ShowcaseAttackBudgetSnapshot budget = CurrentAttackBudget;
            if (budget.ActiveAttackers >= budget.TotalCapacity)
            {
                return true;
            }
            return runner.IsElite
                ? budget.ActiveEliteAttackers >= budget.EliteCapacity
                : budget.ActiveNormalAttackers >= budget.NormalCapacity;
        }

        public bool TryCommitGrab(ShowcaseRunnerAgent runner)
        {
            if (!CanSimulatePressure() || runner == null ||
                !IsRunnerActiveAttacker(runner) || grabbingRunner != null ||
                playerTransform == null ||
                PlanarDistance(runner.transform.position, playerTransform.position) > 1.55f)
            {
                return false;
            }

            grabbingRunner = runner;
            GrabCount++;
            playerLocomotion?.SetMobilityLock(PlayerMobilityLockReason.Grab, true);
            return true;
        }

        public void ResolveTimedGrab(ShowcaseRunnerAgent runner)
        {
            if (grabbingRunner != runner)
            {
                return;
            }

            ReleaseGrab(false);
            ApplyPlayerPressure(runner, 2.8f);
        }

        public bool TryRescuePlayer()
        {
            if (!PlayerGrabbed || allyTransform == null || playerTransform == null ||
                mobileAllyController == null || mobileAllyController.HasTaskMovement ||
                PlanarDistance(allyTransform.position, playerTransform.position) > rescueRange ||
                !HasClearLane(allyTransform.position + Vector3.up, playerTransform.position + Vector3.up))
            {
                return false;
            }

            ShowcaseRunnerAgent rescuedFrom = grabbingRunner;
            ReleaseGrab(true);
            rescuedFrom.ReceiveRescueStagger();
            RescueCount++;
            return true;
        }

        public bool TryCombatTag()
        {
            ShowcaseRunnerAgent target = ResolveCombatTagTarget();
            if (target == null)
            {
                return false;
            }

            target.ReceiveCombatTag();
            CombatTagCount++;
            return true;
        }

        public bool TryPlayerShove()
        {
            if (!CanSimulatePressure() || PlayerGrabbed)
            {
                return false;
            }

            ShowcaseRunnerAgent target = ResolvePlayerMeleeTarget();
            if (playerCombat != null)
            {
                return playerCombat.TryRequestMelee(target);
            }
            if (Time.time < nextPlayerMeleeTime || target == null ||
                interactionOwner.HandState != PrototypeHandOwnershipState.Free ||
                interactionOwner.RelayState != PrototypeRelayReservationState.None)
            {
                return false;
            }

            target.ReceivePlayerShove(playerTransform.forward);
            nextPlayerMeleeTime = Time.time + playerMeleeCooldown;
            PlayerMeleeCount++;
            return true;
        }

        public void RegisterPhysicalImpact(
            ShowcaseRunnerAgent runner,
            PrototypeInteractionTarget source,
            float impactSpeed)
        {
            Rigidbody sourceBody = source != null
                ? source.GetComponent<Rigidbody>()
                : null;
            RegisterPhysicalImpact(
                runner,
                source,
                impactSpeed,
                sourceBody != null ? sourceBody.linearVelocity : Vector3.zero,
                ResolveObjectImpactPoint(source, runner));
        }

        public void RegisterPhysicalImpact(
            ShowcaseRunnerAgent runner,
            PrototypeInteractionTarget source,
            float impactSpeed,
            Vector3 objectLinearVelocity,
            Vector3 worldPoint)
        {
            if (runner == null || source == null ||
                impactSpeed < ObjectImpactTuning.MinimumAcceptedImpactSpeed)
            {
                return;
            }

            if (source.WeightClass == PrototypeWeightClass.Medium)
            {
                ShowcaseCargoStrapController cargo = ResolveCargoStrap();
                bool cargoOwnsSource = cargo != null &&
                    cargo.ActiveTarget == source;
                bool activeCargoSwing = cargoOwnsSource &&
                    (cargo.IsMediumSwinging ||
                     cargo.Phase == ShowcaseCargoStrapPhase.MediumExtending);
                if (activeCargoSwing)
                {
                    TryRegisterCargoSweepImpact(
                        cargo,
                        runner,
                        source,
                        impactSpeed,
                        objectLinearVelocity,
                        worldPoint);
                    return;
                }
                if (cargoOwnsSource)
                {
                    // Rear follow, Pull and Recover contacts are controlled
                    // tether motion, not attacks. Do not synthesize a stagger.
                    return;
                }
            }

            if (TryCommitPlayerThrownObjectImpact(
                    runner,
                    source,
                    impactSpeed,
                    objectLinearVelocity,
                    worldPoint))
            {
                return;
            }

            if (source.WeightClass == PrototypeWeightClass.Light)
            {
                runner.ReceivePhysicalStagger(false);
                PhysicalImpactCount++;
                CombatContactResolved?.Invoke(new ShowcaseCombatContactResult(
                    source.transform,
                    runner.transform,
                    true,
                    worldPoint));
            }
            else if (source.WeightClass == PrototypeWeightClass.Medium)
            {
                runner.ReceivePhysicalStagger(true);
                PhysicalImpactCount++;
                CombatContactResolved?.Invoke(new ShowcaseCombatContactResult(
                    source.transform,
                    runner.transform,
                    true,
                    worldPoint));
            }
        }

        public bool TryRegisterCargoSweepImpact(
            ShowcaseCargoStrapController cargo,
            ShowcaseRunnerAgent runner,
            PrototypeInteractionTarget source,
            float impactSpeed,
            Vector3 objectLinearVelocity,
            Vector3 worldPoint)
        {
            if (cargo == null || runner == null || source == null ||
                cargo.ActiveTarget != source || !CanSimulatePressure() ||
                !cargo.TryAuthorizeMediumSwingImpact(
                    source,
                    runner,
                    impactSpeed,
                    false,
                    out ShowcaseCargoSweepHitAuthorization authorization))
            {
                return false;
            }

            Vector3 resolvedVelocity = ResolveCargoSweepImpactVelocity(
                runner,
                objectLinearVelocity,
                authorization.HitProfile.HitOrdinal);
            ShowcaseCargoChargeProfile chargeProfile =
                cargo.ActiveAttackChargeProfile;
            ShowcaseObjectImpactEvaluation evaluation =
                CreateObjectImpactEvaluation(
                    source,
                    runner,
                    impactSpeed,
                    resolvedVelocity,
                    worldPoint,
                    authorization.SwingSequence,
                    ShowcaseObjectImpactAttackOriginType.CargoStrapMediumSwing,
                    MediumSwingBaseDamage,
                    chargeProfile);
            float travelDistance =
                ShowcaseCargoSweepEnergyModel.ResolveTargetTravelDistance(
                    chargeProfile,
                    authorization.HitProfile,
                    evaluation.DisplacementDistance);
            float travelDuration =
                ShowcaseCargoSweepEnergyModel.ResolveTargetTravelDuration(
                    chargeProfile,
                    authorization.HitProfile,
                    evaluation.DisplacementDuration);
            ShowcaseObjectImpactSeverity scaledSeverity =
                authorization.HitProfile.HitOrdinal == 1
                    ? evaluation.Severity
                    : ShowcaseObjectImpactSeverity.Medium;
            evaluation = ScaleCargoImpactEvaluation(
                evaluation,
                authorization.HitProfile.DamageScale,
                1f,
                authorization.HitProfile.FeedbackScale,
                scaledSeverity,
                false);
            evaluation = OverrideCargoSweepMotion(
                evaluation,
                travelDistance,
                travelDuration);

            bool accepted = runner.ReceivePlayerObjectImpact(
                authorization.SourceId,
                evaluation);
            if (accepted)
            {
                cargo.ConfirmMediumSwingImpact(
                    runner,
                    authorization.SwingSequence);
                PhysicalImpactCount++;
                LastObjectImpactEvaluation = evaluation;
                if (authorization.IsPrimaryVictim && !runner.IsDead)
                {
                    Collider sourceCollider = source.GetComponent<Collider>();
                    float carryDuration =
                        ShowcaseCargoSweepEnergyModel.
                            ResolvePrimaryCarryDuration(chargeProfile);
                    float controlledCarrySpeed = travelDistance /
                        Mathf.Max(0.01f, carryDuration);
                    runner.BeginCargoSweepCarry(
                        sourceCollider,
                        evaluation.Context.PrimaryImpactDirection,
                        controlledCarrySpeed,
                        carryDuration,
                        0.06f,
                        evaluation);
                }
            }
            else
            {
                cargo.RejectMediumSwingImpact(
                    runner,
                    authorization.SwingSequence);
            }

            CombatContactResolved?.Invoke(new ShowcaseCombatContactResult(
                source.transform,
                runner.transform,
                accepted,
                evaluation.Context.WorldPoint,
                evaluation));
            return accepted;
        }

        private static ShowcaseObjectImpactEvaluation OverrideCargoSweepMotion(
            ShowcaseObjectImpactEvaluation evaluation,
            float displacementDistance,
            float displacementDuration)
        {
            if (!evaluation.IsValid)
            {
                return evaluation;
            }

            return new ShowcaseObjectImpactEvaluation(
                evaluation.Context,
                evaluation.Severity,
                evaluation.ImpactScore,
                evaluation.BaseDamage,
                evaluation.DamageMultiplier,
                evaluation.Damage,
                Mathf.Max(0f, displacementDistance),
                Mathf.Max(0.01f, displacementDuration),
                evaluation.HitStopSeconds,
                evaluation.CameraImpulseMultiplier,
                evaluation.CausesKnockdown);
        }

        private Vector3 ResolveCargoSweepImpactVelocity(
            ShowcaseRunnerAgent runner,
            Vector3 objectLinearVelocity,
            int hitOrdinal)
        {
            Vector3 planarVelocity = Vector3.ProjectOnPlane(
                objectLinearVelocity,
                Vector3.up);
            float speed = planarVelocity.magnitude;
            Vector3 pathDirection = speed > 0.001f
                ? planarVelocity / speed
                : playerTransform != null
                    ? playerTransform.forward
                    : Vector3.forward;
            if (hitOrdinal <= 1 || playerTransform == null)
            {
                return pathDirection * Mathf.Max(0.1f, speed);
            }

            Vector3 outward = Vector3.ProjectOnPlane(
                runner.transform.position - playerTransform.position,
                Vector3.up).normalized;
            if (outward.sqrMagnitude <= 0.001f)
            {
                outward = pathDirection;
            }
            Vector3 resolvedDirection =
                (pathDirection * 0.55f) + (outward * 0.85f);
            if (resolvedDirection.sqrMagnitude <= 0.001f)
            {
                resolvedDirection = pathDirection;
            }
            return resolvedDirection.normalized * Mathf.Max(0.1f, speed);
        }

        public int ResolveCargoSlamImpact(
            ShowcaseCargoStrapController cargo,
            PrototypeInteractionTarget source,
            Vector3 worldPoint,
            Vector3 objectLinearVelocity,
            float impactSpeed,
            uint attackSequence,
            ShowcaseCargoChargeProfile chargeProfile,
            float directRadius,
            float outerRadius,
            ShowcaseRunnerAgent directContactRunner = null)
        {
            if (cargo == null || source == null || cargo.ActiveTarget != source ||
                cargo.ActiveAttackType !=
                    ShowcaseCargoAttackType.OverheadSlam ||
                attackSequence == 0 ||
                attackSequence != cargo.ActiveMediumSwingSequence ||
                impactSpeed < ObjectImpactTuning.MinimumAcceptedImpactSpeed ||
                !CanSimulatePressure())
            {
                return 0;
            }

            float resolvedDirectRadius = Mathf.Clamp(directRadius, 0.5f, 0.7f);
            float resolvedOuterRadius = Mathf.Clamp(
                outerRadius,
                Mathf.Max(1.5f, resolvedDirectRadius + 0.1f),
                2f);
            int acceptedCount = 0;
            for (int index = 0; index < targetableRunners.Length; index++)
            {
                ShowcaseRunnerAgent runner = targetableRunners[index];
                if (runner == null || runner.IsDead ||
                    !runner.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Collider runnerCollider = runner.GetComponent<Collider>();
                bool hasEnabledCollider = runnerCollider != null &&
                    runnerCollider.enabled;
                Vector3 runnerCenter = hasEnabledCollider
                    ? runnerCollider.bounds.center
                    : runner.transform.position + Vector3.up;
                Vector3 closestPoint = hasEnabledCollider
                    ? runnerCollider.ClosestPoint(worldPoint)
                    : runnerCenter;
                float planarDistance = Vector3.ProjectOnPlane(
                    closestPoint - worldPoint,
                    Vector3.up).magnitude;
                bool direct = runner == directContactRunner ||
                    planarDistance <= resolvedDirectRadius;
                if (!direct && planarDistance > resolvedOuterRadius)
                {
                    continue;
                }

                Vector3 radialDirection = Vector3.ProjectOnPlane(
                    runnerCenter - worldPoint,
                    Vector3.up);
                if (radialDirection.sqrMagnitude <= 0.001f)
                {
                    radialDirection = playerTransform != null
                        ? playerTransform.forward
                        : Vector3.forward;
                }
                radialDirection.Normalize();
                Vector3 resolvedVelocity = radialDirection * Mathf.Max(
                    impactSpeed,
                    Vector3.ProjectOnPlane(
                        objectLinearVelocity,
                        Vector3.up).magnitude);
                ShowcaseObjectImpactEvaluation evaluation =
                    CreateObjectImpactEvaluation(
                        source,
                        runner,
                        impactSpeed,
                        resolvedVelocity,
                        worldPoint,
                        attackSequence,
                        ShowcaseObjectImpactAttackOriginType.
                            CargoStrapOverheadSlam,
                        MediumSwingBaseDamage,
                        chargeProfile);
                if (!direct)
                {
                    evaluation = ScaleCargoImpactEvaluation(
                        evaluation,
                        0.35f,
                        0.45f,
                        0.65f,
                        ShowcaseObjectImpactSeverity.Medium,
                        false);
                }

                string sourceId = ResolveImpactSourceId(evaluation.Context);
                if (!direct)
                {
                    sourceId = $"{sourceId}:OUTER:{runner.RunnerId}";
                }
                bool accepted = runner.ReceivePlayerObjectImpact(
                    sourceId,
                    evaluation);
                CombatContactResolved?.Invoke(new ShowcaseCombatContactResult(
                    source.transform,
                    runner.transform,
                    accepted,
                    worldPoint,
                    evaluation));
                if (!accepted)
                {
                    continue;
                }

                acceptedCount++;
                PhysicalImpactCount++;
                LastObjectImpactEvaluation = evaluation;
            }
            return acceptedCount;
        }

        private bool TryCommitPlayerThrownObjectImpact(
            ShowcaseRunnerAgent runner,
            PrototypeInteractionTarget source,
            float impactSpeed,
            Vector3 objectLinearVelocity,
            Vector3 worldPoint)
        {
            ShowcaseCargoStrapController cargo = ResolveCargoStrap();
            if (cargo != null && cargo.ActiveTarget == source)
            {
                return false;
            }

            uint throwSequence = source.LastCommittedPlayerThrowSequence;
            int baseDamage = source.WeightClass switch
            {
                PrototypeWeightClass.Light => LightThrowBaseDamage,
                PrototypeWeightClass.Medium => MediumThrowBaseDamage,
                _ => 0
            };
            if (throwSequence == 0 || baseDamage <= 0 || !CanSimulatePressure())
            {
                return false;
            }

            ShowcaseObjectImpactEvaluation evaluation =
                CreateObjectImpactEvaluation(
                    source,
                    runner,
                    impactSpeed,
                    objectLinearVelocity,
                    worldPoint,
                    throwSequence,
                    ShowcaseObjectImpactAttackOriginType.HandThrow,
                    baseDamage);
            bool accepted = runner.ReceivePlayerObjectImpact(
                ResolveImpactSourceId(evaluation.Context),
                evaluation);
            CombatContactResolved?.Invoke(new ShowcaseCombatContactResult(
                source.transform,
                runner.transform,
                accepted,
                evaluation.Context.WorldPoint,
                evaluation));
            if (accepted)
            {
                PhysicalImpactCount++;
                LastObjectImpactEvaluation = evaluation;
            }
            return true;
        }

        public bool RegisterSecondaryImpact(
            ShowcaseRunnerAgent primaryRunner,
            Collider obstacle,
            ShowcaseObjectImpactEvaluation objectImpact)
        {
            if (primaryRunner == null || obstacle == null || !objectImpact.IsValid)
            {
                return false;
            }

            ShowcaseRunnerAgent secondaryRunner =
                obstacle.GetComponentInParent<ShowcaseRunnerAgent>();
            if (secondaryRunner == primaryRunner)
            {
                return false;
            }

            ShowcaseCargoStrapController cargo = ResolveCargoStrap();
            bool activeCargoSweepCollision =
                objectImpact.Context.AttackOriginType ==
                    ShowcaseObjectImpactAttackOriginType.
                        CargoStrapMediumSwing &&
                cargo != null &&
                cargo.ActiveTarget == objectImpact.Context.SourceObject &&
                cargo.ActiveMediumSwingSequence ==
                    objectImpact.Context.AttackSequence;
            bool cargoSweepPrimaryCollision =
                activeCargoSweepCollision &&
                cargo.PrimarySweptVictim == primaryRunner;

            if (secondaryRunner == null)
            {
                primaryRunner.ApplySecondaryImpactStun(secondaryStunSeconds);
                var worldResult = new ShowcaseSecondaryImpactResult(
                    ShowcaseSecondaryImpactKind.WorldSolid,
                    primaryRunner,
                    null,
                    obstacle,
                    objectImpact,
                    0,
                    false);
                LastSecondaryImpactResult = worldResult;
                SecondaryImpactCount++;
                SecondaryImpactResolved?.Invoke(worldResult);
                if (cargoSweepPrimaryCollision)
                {
                    cargo.NotifyPrimarySweepVictimBlocked(
                        primaryRunner,
                        objectImpact.Context.AttackSequence,
                        obstacle);
                }
                return true;
            }

            if (secondaryRunner.IsDead)
            {
                return false;
            }

            var pairKey = new SecondaryImpactPairKey(
                objectImpact.Context.SourceObject,
                objectImpact.Context.AttackSequence,
                primaryRunner,
                secondaryRunner);
            if (!acceptedSecondaryImpactPairs.Add(pairKey))
            {
                RejectedSecondaryImpactDuplicateCount++;
                return false;
            }

            if (activeCargoSweepCollision)
            {
                return RegisterCargoSweepEnemyCollision(
                    cargo,
                    primaryRunner,
                    secondaryRunner,
                    obstacle,
                    objectImpact);
            }

            primaryRunner.ApplySecondaryImpactStun(secondaryStunSeconds);
            int secondaryDamage = Mathf.Max(
                1,
                Mathf.RoundToInt(
                    objectImpact.Damage *
                    Mathf.Clamp01(secondaryEnemyDamageMultiplier)));
            string sourceId =
                $"{ResolveImpactSourceId(objectImpact.Context)}:" +
                $"SECONDARY:{primaryRunner.RunnerId}:{secondaryRunner.RunnerId}";
            bool secondaryAccepted = secondaryRunner.ReceiveSecondaryObjectImpact(
                sourceId,
                objectImpact.Context.AttackSequence,
                secondaryDamage,
                objectImpact.Context.PrimaryImpactDirection,
                secondaryEnemyDisplacementDistance,
                secondaryEnemyDisplacementDuration,
                secondaryStunSeconds);
            var enemyResult = new ShowcaseSecondaryImpactResult(
                ShowcaseSecondaryImpactKind.Enemy,
                primaryRunner,
                secondaryRunner,
                obstacle,
                objectImpact,
                secondaryDamage,
                secondaryAccepted);
            LastSecondaryImpactResult = enemyResult;
            SecondaryImpactCount++;
            EnemyCollisionCount++;
            SecondaryImpactResolved?.Invoke(enemyResult);
            EnemyCollisionResolved?.Invoke(enemyResult);
            return true;
        }

        private bool RegisterCargoSweepEnemyCollision(
            ShowcaseCargoStrapController cargo,
            ShowcaseRunnerAgent primaryRunner,
            ShowcaseRunnerAgent secondaryRunner,
            Collider obstacle,
            ShowcaseObjectImpactEvaluation primaryImpact)
        {
            primaryRunner.ApplySecondaryImpactStun(secondaryStunSeconds);
            bool damageAccepted = false;
            int secondaryDamage = 0;
            ShowcaseObjectImpactEvaluation secondaryEvaluation = default;
            float impactSpeed = Mathf.Max(
                ObjectImpactTuning.MinimumAcceptedImpactSpeed,
                primaryImpact.Context.RelativeImpactSpeed * 0.5f);
            if (cargo.TryAuthorizeMediumSwingImpact(
                    primaryImpact.Context.SourceObject,
                    secondaryRunner,
                    impactSpeed,
                    true,
                    out ShowcaseCargoSweepHitAuthorization authorization))
            {
                secondaryEvaluation = CreateCargoEnemyCollisionEvaluation(
                    cargo,
                    primaryRunner,
                    secondaryRunner,
                    obstacle,
                    primaryImpact,
                    authorization);
                secondaryDamage = secondaryEvaluation.Damage;
                damageAccepted = secondaryRunner.ReceivePlayerObjectImpact(
                    authorization.SourceId,
                    secondaryEvaluation);
                if (damageAccepted)
                {
                    cargo.ConfirmMediumSwingImpact(
                        secondaryRunner,
                        authorization.SwingSequence);
                    PhysicalImpactCount++;
                    LastObjectImpactEvaluation = secondaryEvaluation;
                }
                else
                {
                    cargo.RejectMediumSwingImpact(
                        secondaryRunner,
                        authorization.SwingSequence);
                }

                CombatContactResolved?.Invoke(
                    new ShowcaseCombatContactResult(
                        primaryImpact.Context.SourceObject.transform,
                        secondaryRunner.transform,
                        damageAccepted,
                        secondaryEvaluation.Context.WorldPoint,
                        secondaryEvaluation));
            }

            var result = new ShowcaseSecondaryImpactResult(
                ShowcaseSecondaryImpactKind.Enemy,
                primaryRunner,
                secondaryRunner,
                obstacle,
                damageAccepted ? secondaryEvaluation : primaryImpact,
                secondaryDamage,
                damageAccepted);
            LastSecondaryImpactResult = result;
            SecondaryImpactCount++;
            EnemyCollisionCount++;
            SecondaryImpactResolved?.Invoke(result);
            EnemyCollisionResolved?.Invoke(result);
            return true;
        }

        private ShowcaseObjectImpactEvaluation
            CreateCargoEnemyCollisionEvaluation(
                ShowcaseCargoStrapController cargo,
                ShowcaseRunnerAgent primaryRunner,
                ShowcaseRunnerAgent secondaryRunner,
                Collider obstacle,
                ShowcaseObjectImpactEvaluation primaryImpact,
                ShowcaseCargoSweepHitAuthorization authorization)
        {
            Vector3 direction = Vector3.ProjectOnPlane(
                secondaryRunner.transform.position -
                    primaryRunner.transform.position,
                Vector3.up).normalized;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = primaryImpact.Context.PrimaryImpactDirection;
            }
            float speed = Mathf.Max(
                ObjectImpactTuning.MinimumAcceptedImpactSpeed,
                primaryImpact.Context.RelativeImpactSpeed * 0.5f);
            Vector3 worldPoint = obstacle != null
                ? obstacle.ClosestPoint(primaryRunner.transform.position)
                : secondaryRunner.transform.position + Vector3.up;
            ShowcaseCargoChargeProfile chargeProfile =
                cargo.ActiveAttackChargeProfile;
            ShowcaseObjectImpactEvaluation evaluation =
                CreateObjectImpactEvaluation(
                    primaryImpact.Context.SourceObject,
                    secondaryRunner,
                    speed,
                    direction * speed,
                    worldPoint,
                    authorization.SwingSequence,
                    ShowcaseObjectImpactAttackOriginType.CargoStrapMediumSwing,
                    MediumSwingBaseDamage,
                    chargeProfile);
            float travelDistance =
                ShowcaseCargoSweepEnergyModel.ResolveTargetTravelDistance(
                    chargeProfile,
                    authorization.HitProfile,
                    evaluation.DisplacementDistance);
            float travelDuration =
                ShowcaseCargoSweepEnergyModel.ResolveTargetTravelDuration(
                    chargeProfile,
                    authorization.HitProfile,
                    evaluation.DisplacementDuration);
            ShowcaseObjectImpactSeverity severity =
                authorization.HitProfile.HitOrdinal == 1
                    ? evaluation.Severity
                    : ShowcaseObjectImpactSeverity.Medium;
            evaluation = ScaleCargoImpactEvaluation(
                evaluation,
                authorization.HitProfile.DamageScale,
                1f,
                authorization.HitProfile.FeedbackScale,
                severity,
                false);
            return OverrideCargoSweepMotion(
                evaluation,
                travelDistance,
                travelDuration);
        }

        private ShowcaseObjectImpactEvaluation CreateObjectImpactEvaluation(
            PrototypeInteractionTarget source,
            ShowcaseRunnerAgent runner,
            float impactSpeed,
            Vector3 objectLinearVelocity,
            Vector3 worldPoint,
            uint attackSequence,
            ShowcaseObjectImpactAttackOriginType fallbackOrigin,
            int baseDamage,
            ShowcaseCargoChargeProfile? cargoChargeProfile = null)
        {
            Rigidbody sourceBody = source != null
                ? source.GetComponent<Rigidbody>()
                : null;
            float mass = sourceBody != null
                ? Mathf.Max(0.01f, sourceBody.mass)
                : source != null && source.WeightClass == PrototypeWeightClass.Light
                    ? 1f
                    : ObjectImpactTuning.ReferenceMass;
            float charge01 = 0f;
            bool hasKineticEnergy = false;
            float kineticEnergy = 0f;
            ShowcaseObjectImpactAttackOriginType origin = fallbackOrigin;
            float impactEnergyBudgetMultiplier = 1f;
            Vector3 resolvedLinearVelocity = IsFinite(objectLinearVelocity)
                ? objectLinearVelocity
                : Vector3.zero;

            if (cargoChargeProfile.HasValue)
            {
                ShowcaseCargoChargeProfile profile = cargoChargeProfile.Value;
                charge01 = profile.Charge01;
                impactEnergyBudgetMultiplier =
                    profile.ImpactEnergyBudgetMultiplier;
            }
            else if (fallbackOrigin !=
                         ShowcaseObjectImpactAttackOriginType.
                             CargoStrapMediumSwing &&
                     fallbackOrigin !=
                         ShowcaseObjectImpactAttackOriginType.
                             CargoStrapOverheadSlam &&
                source != null &&
                source.TryGetPhysicalReleaseImpact(
                    attackSequence,
                    out PhysicalReleaseSnapshot release,
                    out kineticEnergy))
            {
                origin = release.AttackOriginType;
                charge01 = release.Charge01;
                hasKineticEnergy = kineticEnergy >= 0f;
                if (resolvedLinearVelocity.sqrMagnitude <= 0.001f &&
                    IsFinite(release.LinearVelocity))
                {
                    resolvedLinearVelocity = release.LinearVelocity;
                }
            }

            Vector3 primaryDirection = Vector3.ProjectOnPlane(
                resolvedLinearVelocity,
                Vector3.up);
            if (primaryDirection.sqrMagnitude <= 0.001f)
            {
                primaryDirection = ResolveObjectImpactDirection(source, runner);
            }
            Vector3 resolvedPoint = IsFinite(worldPoint)
                ? worldPoint
                : ResolveObjectImpactPoint(source, runner);
            var context = new ShowcaseObjectImpactContext(
                source,
                source != null ? source.WeightClass : default,
                impactSpeed,
                resolvedLinearVelocity,
                mass,
                charge01,
                hasKineticEnergy,
                kineticEnergy,
                origin,
                primaryDirection,
                resolvedPoint,
                attackSequence,
                impactEnergyBudgetMultiplier);
            ShowcaseObjectImpactEvaluation evaluation =
                ObjectImpactTuning.Evaluate(context, baseDamage);
            return cargoChargeProfile.HasValue
                ? ApplyCargoChargeProfile(
                    evaluation,
                    baseDamage,
                    cargoChargeProfile.Value)
                : evaluation;
        }

        private static ShowcaseObjectImpactEvaluation ApplyCargoChargeProfile(
            ShowcaseObjectImpactEvaluation evaluation,
            int baseDamage,
            ShowcaseCargoChargeProfile profile)
        {
            if (!evaluation.Context.IsValid)
            {
                return default;
            }

            int damage = Mathf.Max(
                1,
                Mathf.RoundToInt(baseDamage * profile.DamageMultiplier));
            return new ShowcaseObjectImpactEvaluation(
                evaluation.Context,
                profile.FeedbackSeverity,
                evaluation.ImpactScore *
                    profile.ImpactEnergyBudgetMultiplier,
                baseDamage,
                profile.DamageMultiplier,
                damage,
                evaluation.DisplacementDistance *
                    profile.KnockbackMultiplier,
                evaluation.DisplacementDuration,
                evaluation.HitStopSeconds * profile.HitStopMultiplier,
                profile.CameraImpulseMultiplier,
                profile.AllowsKnockdown);
        }

        private static ShowcaseObjectImpactEvaluation ScaleCargoImpactEvaluation(
            ShowcaseObjectImpactEvaluation evaluation,
            float damageScale,
            float knockbackScale,
            float feedbackScale,
            ShowcaseObjectImpactSeverity severity,
            bool causesKnockdown)
        {
            if (!evaluation.IsValid)
            {
                return evaluation;
            }

            float resolvedDamageScale = Mathf.Max(0f, damageScale);
            return new ShowcaseObjectImpactEvaluation(
                evaluation.Context,
                severity,
                evaluation.ImpactScore * Mathf.Max(0f, feedbackScale),
                evaluation.BaseDamage,
                evaluation.DamageMultiplier * resolvedDamageScale,
                Mathf.Max(
                    1,
                    Mathf.RoundToInt(evaluation.Damage * resolvedDamageScale)),
                evaluation.DisplacementDistance *
                    Mathf.Max(0f, knockbackScale),
                evaluation.DisplacementDuration,
                evaluation.HitStopSeconds * Mathf.Max(0f, feedbackScale),
                evaluation.CameraImpulseMultiplier *
                    Mathf.Max(0f, feedbackScale),
                causesKnockdown);
        }

        private static string ResolveImpactSourceId(
            ShowcaseObjectImpactContext context)
        {
            string runtimeKey = context.SourceObject != null
                ? context.SourceObject.RuntimeInstanceKey
                : "UNKNOWN";
            return context.AttackOriginType switch
            {
                ShowcaseObjectImpactAttackOriginType.CargoStrapMediumSwing =>
                    $"CHR00_STRAP:{runtimeKey}",
                ShowcaseObjectImpactAttackOriginType.CargoStrapOverheadSlam =>
                    $"CHR00_STRAP_SLAM:{runtimeKey}",
                ShowcaseObjectImpactAttackOriginType.LightStrapThrow =>
                    $"CHR00_LIGHT_STRAP_THROW:{runtimeKey}",
                _ => $"CHR00_THROW:{runtimeKey}"
            };
        }

        public bool TryResolveCargoStrapTarget(
            Vector3 aimPoint,
            float maxRange,
            out ShowcaseRunnerAgent runner,
            out Vector3 targetPoint,
            out bool beyondRange)
        {
            runner = null;
            targetPoint = default;
            beyondRange = false;
            if (!CanSimulatePressure() || playerTransform == null ||
                maxRange <= 0f)
            {
                return false;
            }

            Vector3 origin = playerTransform.position + Vector3.up;
            Vector3 aimDirection = aimPoint - origin;
            if (aimDirection.sqrMagnitude <= 0.001f)
            {
                aimDirection = playerTransform.forward;
            }
            aimDirection.Normalize();

            ShowcaseRunnerAgent best = null;
            Vector3 bestCenter = default;
            float bestScore = float.PositiveInfinity;
            float bestRange = float.PositiveInfinity;
            for (int index = 0; index < targetableRunners.Length; index++)
            {
                ShowcaseRunnerAgent candidate = targetableRunners[index];
                if (candidate == null || candidate.IsDead ||
                    !candidate.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Collider targetCollider = candidate.GetComponent<Collider>();
                if (targetCollider == null || !targetCollider.enabled)
                {
                    continue;
                }

                Vector3 center = targetCollider.bounds.center;
                Vector3 toCenter = center - origin;
                float rayDistance = Vector3.Dot(toCenter, aimDirection);
                if (rayDistance <= 0f || !HasClearLane(origin, center))
                {
                    continue;
                }

                Vector3 nearestRayPoint = origin + (aimDirection * rayDistance);
                float rayMissDistance = Vector3.Distance(center, nearestRayPoint);
                Vector3 extents = targetCollider.bounds.extents;
                float aimAllowance = Mathf.Max(extents.x, extents.z) +
                    CargoTargetAimPadding;
                if (rayMissDistance > aimAllowance)
                {
                    continue;
                }

                float planarRange = PlanarDistance(center, playerTransform.position);
                float score = rayMissDistance / Mathf.Max(0.05f, aimAllowance);
                bool betterScore = score < bestScore - 0.0001f;
                bool sameScore = Mathf.Abs(score - bestScore) <= 0.0001f;
                bool betterRange = sameScore && planarRange < bestRange - 0.0001f;
                bool deterministicTie = sameScore &&
                    Mathf.Abs(planarRange - bestRange) <= 0.0001f &&
                    (best == null || string.CompareOrdinal(
                        candidate.RunnerId,
                        best.RunnerId) < 0);
                if (!betterScore && !betterRange && !deterministicTie)
                {
                    continue;
                }

                best = candidate;
                bestCenter = center;
                bestScore = score;
                bestRange = planarRange;
            }

            if (best == null)
            {
                return false;
            }
            if (bestRange > maxRange)
            {
                beyondRange = true;
                targetPoint = bestCenter;
                return false;
            }

            runner = best;
            targetPoint = bestCenter;
            return true;
        }

        public void ApplyPlayerPressure(ShowcaseRunnerAgent runner, float impulseSpeed)
        {
            if (runner == null || playerTransform == null)
            {
                return;
            }

            Vector3 direction = Vector3.ProjectOnPlane(
                playerTransform.position - runner.transform.position,
                Vector3.up).normalized;
            playerLocomotion?.AddExternalPlanarImpulse(direction * Mathf.Max(0f, impulseSpeed));
            PlayerHitCount++;
        }

        public void ApplyPlayerAttack(
            ShowcaseRunnerAgent runner,
            int damage,
            bool knockdown,
            float impulseSpeed,
            uint attackSequence)
        {
            if (runner == null || playerTransform == null)
            {
                return;
            }

            Vector3 direction = Vector3.ProjectOnPlane(
                playerTransform.position - runner.transform.position,
                Vector3.up).normalized;
            bool accepted = playerCombat != null && playerCombat.ReceiveDamage(
                new ShowcaseDamageRequest(
                    ShowcaseCombatTeam.Enemy,
                    runner.RunnerId,
                    attackSequence,
                    damage,
                    knockdown ? ShowcaseHitReaction.Knockdown : ShowcaseHitReaction.Body,
                    direction));
            if (!accepted && playerCombat != null)
            {
                return;
            }

            playerCombat?.QueueReactionDisplacement(
                direction,
                knockdown ? 0.65f : 0.3f,
                knockdown ? 0.24f : 0.2f);
            PlayerHitCount++;
        }

        public bool ApplyPlayerAttackFromBoneContact(
            ShowcaseRunnerAgent runner,
            ShowcasePlayerCombatController actualVictim,
            int damage,
            bool knockdown,
            float impulseSpeed,
            uint attackSequence)
        {
            if (runner == null || actualVictim == null ||
                actualVictim != playerCombat || playerTransform == null)
            {
                return false;
            }

            Vector3 direction = Vector3.ProjectOnPlane(
                playerTransform.position - runner.transform.position,
                Vector3.up).normalized;
            bool accepted = actualVictim.ReceiveDamage(
                new ShowcaseDamageRequest(
                    ShowcaseCombatTeam.Enemy,
                    runner.RunnerId,
                    attackSequence,
                    damage,
                    knockdown ? ShowcaseHitReaction.Knockdown : ShowcaseHitReaction.Body,
                    direction));
            CombatContactResolved?.Invoke(new ShowcaseCombatContactResult(
                runner.transform,
                playerTransform,
                accepted,
                playerTransform.position + Vector3.up));
            if (!accepted)
            {
                return false;
            }

            actualVictim.QueueReactionDisplacement(
                direction,
                knockdown ? 0.65f : 0.3f,
                knockdown ? 0.24f : 0.2f);
            PlayerHitCount++;
            return true;
        }

        public bool CommitPlayerMeleeHit(
            ShowcaseRunnerAgent target,
            int comboStep,
            uint attackSequence,
            int damage)
        {
            if (target == null || target.IsDead || playerTransform == null ||
                target != ResolvePlayerMeleeTarget())
            {
                return false;
            }

            bool accepted = target.ReceivePlayerMeleeHit(
                comboStep,
                attackSequence,
                damage,
                playerTransform.position);
            CombatContactResolved?.Invoke(new ShowcaseCombatContactResult(
                playerTransform,
                target.transform,
                accepted,
                target.transform.position + Vector3.up));
            if (accepted)
            {
                PlayerMeleeCount++;
            }
            return accepted;
        }

        public bool CommitPlayerMeleeHitFromBoneContact(
            ShowcaseRunnerAgent target,
            int comboStep,
            uint attackSequence,
            int damage,
            Vector3 worldPoint)
        {
            if (!IsValidMeleeContactTarget(target))
            {
                return false;
            }

            bool accepted = target.ReceivePlayerMeleeHit(
                comboStep,
                attackSequence,
                damage,
                playerTransform.position);
            CombatContactResolved?.Invoke(new ShowcaseCombatContactResult(
                playerTransform,
                target.transform,
                accepted,
                worldPoint));
            if (accepted)
            {
                PlayerMeleeCount++;
            }
            return accepted;
        }

        public ShowcaseRunnerAgent ResolvePlayerMeleeTargetForCombo()
        {
            return ResolvePlayerMeleeTarget();
        }

        public void ResetToRunBaseline()
        {
            playerLocomotion?.SetMobilityLock(PlayerMobilityLockReason.Grab, false);
            ClearAttackBudget(true);
            grabbingRunner = null;
            PlayerHitCount = 0;
            GrabCount = 0;
            RescueCount = 0;
            CombatTagCount = 0;
            PlayerMeleeCount = 0;
            PhysicalImpactCount = 0;
            SecondaryImpactCount = 0;
            EnemyCollisionCount = 0;
            RejectedSecondaryImpactDuplicateCount = 0;
            LastObjectImpactEvaluation = default;
            LastSecondaryImpactResult = default;
            acceptedSecondaryImpactPairs.Clear();
            nextPlayerMeleeTime = 0f;
            externallySuspended = false;
            foreach (ShowcaseRunnerAgent runner in targetableRunners)
            {
                runner?.ResetToRunBaseline();
            }
            InitializePressureRuntime(Time.time);
            BaselineReset?.Invoke();
        }

        private void ReleaseGrab(bool rescued)
        {
            ShowcaseRunnerAgent previous = grabbingRunner;
            grabbingRunner = null;
            playerLocomotion?.SetMobilityLock(PlayerMobilityLockReason.Grab, false);
            ReleaseAttackToken(previous);
            if (!rescued)
            {
                previous?.FinishGrabWithoutRescue();
            }
        }

        public int ResolveBaseNormalAttackBudget(int enemyCount)
        {
            int singleMaximum = Mathf.Max(1, normalSingleSlotEnemyMaximum);
            int doubleMaximum = Mathf.Max(
                singleMaximum + 1,
                normalDoubleSlotEnemyMaximum);
            int resolvedEnemyCount = Mathf.Max(1, enemyCount);
            if (resolvedEnemyCount <= singleMaximum)
            {
                return 1;
            }
            if (resolvedEnemyCount <= doubleMaximum)
            {
                return Mathf.Min(2, Mathf.Max(1, maximumNormalAttackers));
            }
            return Mathf.Clamp(maximumNormalAttackers, 1, 3);
        }

        public ShowcaseAttackBudgetSnapshot EvaluateAttackBudget(
            float elapsedSeconds)
        {
            int enemyCount = 0;
            int normalCount = 0;
            int eliteCount = 0;
            if (runners != null)
            {
                for (int index = 0; index < runners.Length; index++)
                {
                    ShowcaseRunnerAgent runner = runners[index];
                    if (!IsLiveAutonomousRunner(runner))
                    {
                        continue;
                    }

                    enemyCount++;
                    if (runner.IsElite)
                    {
                        eliteCount++;
                    }
                    else
                    {
                        normalCount++;
                    }
                }
            }

            // Synthetic contract tests and isolated attack auditions may bind a
            // runner after Configure. Keep one typed slot available in that case.
            bool unregisteredAudition = enemyCount == 0;
            int totalMaximum = Mathf.Clamp(
                maximumSimultaneousHardContacts,
                1,
                3);
            float openingEnd = Mathf.Clamp(
                openingSingleAttackerSeconds,
                10f,
                15f);
            float fullPressureStart = Mathf.Max(openingEnd, fullPressureSeconds);
            float elapsed = Mathf.Max(0f, elapsedSeconds);
            bool fullPressure = elapsed >= fullPressureStart;
            int totalCapacity = elapsed < openingEnd
                ? 1
                : fullPressure
                    ? totalMaximum
                    : Mathf.Min(2, totalMaximum);

            int normalCapacity = normalCount > 0 || unregisteredAudition
                ? Mathf.Min(
                    ResolveBaseNormalAttackBudget(enemyCount),
                    totalCapacity)
                : 0;
            if (elapsed < openingEnd)
            {
                normalCapacity = Mathf.Min(normalCapacity, 1);
            }
            else if (!fullPressure)
            {
                normalCapacity = Mathf.Min(normalCapacity, 2);
            }

            int eliteCapacity = eliteCount > 0 || unregisteredAudition
                ? Mathf.Min(Mathf.Clamp(eliteAttackSlots, 0, 1), totalCapacity)
                : 0;
            if (fullPressure && eliteCapacity > 0 && normalCount > 0)
            {
                // Keep the Elite slot genuinely separate instead of letting the
                // third Normal consume the total hard-contact ceiling first.
                normalCapacity = Mathf.Min(
                    normalCapacity,
                    Mathf.Max(1, totalCapacity - eliteCapacity));
            }

            return new ShowcaseAttackBudgetSnapshot(
                enemyCount,
                normalCapacity,
                eliteCapacity,
                totalCapacity,
                normalAttackers.Count,
                eliteAttackers.Count,
                fullPressure);
        }

        public float ResolveGlobalAttackStartGap(ShowcaseRunnerAgent runner)
        {
            Vector2 range = GlobalAttackStartGapRange;
            return Mathf.Lerp(
                range.x,
                range.y,
                ResolveDeterministic01(
                    runner,
                    0xA511E9B3u,
                    (uint)Mathf.Max(1, runner != null
                        ? runner.AttackCommitCount + 1
                        : 1)));
        }

        public float ResolveActorAttackCooldown(ShowcaseRunnerAgent runner)
        {
            Vector2 range = ActorAttackCooldownRange;
            float cooldown = Mathf.Lerp(
                range.x,
                range.y,
                ResolveDeterministic01(runner, 0x63D83595u, 0u));
            if (runner != null && runner.IsElite &&
                elitePressurePriority != ShowcaseElitePressurePriority.Default)
            {
                cooldown = Mathf.Max(range.x, cooldown * 0.9f);
            }
            return cooldown;
        }

        public float ResolveTelegraphDuration(
            ShowcaseRunnerAgent runner,
            uint attackSequence)
        {
            Vector2 range = runner != null && runner.IsElite
                ? EliteTelegraphRange
                : NormalTelegraphRange;
            return Mathf.Lerp(
                range.x,
                range.y,
                ResolveDeterministic01(
                    runner,
                    runner != null && runner.IsElite
                        ? 0xC2B2AE35u
                        : 0x27D4EB2Fu,
                    attackSequence));
        }

        public bool TryResolvePressurePosition(
            ShowcaseRunnerAgent runner,
            out Vector3 worldPosition,
            out float movementSpeedScale)
        {
            if (runner == null || playerTransform == null || runner.IsDead ||
                IsRunnerActiveAttacker(runner))
            {
                worldPosition = default;
                movementSpeedScale = 0f;
                return false;
            }

            int ordinal = ResolveRunnerPressureOrdinal(runner);
            uint stableHash = StableRunnerHash(runner, 0x9E3779B9u);
            float side = (stableHash & 1u) == 0u ? 1f : -1f;
            int lane = Mathf.Abs(ordinal) % 3;
            bool charging = ResolveCargoStrap()?.IsCharging == true;
            float baseAngle;
            if (runner.IsElite)
            {
                baseAngle = elitePressurePriority switch
                {
                    ShowcaseElitePressurePriority.PlayerCharging => 38f,
                    ShowcaseElitePressurePriority.PlayerStationary => 48f,
                    ShowcaseElitePressurePriority.PlayerFocusingAnotherEnemy => 68f,
                    _ => 56f
                };
            }
            else
            {
                baseAngle = charging
                    ? 78f + lane * 14f
                    : 60f + lane * 20f;
            }

            float phase = ResolveDeterministic01(
                runner,
                0x165667B1u,
                0u) * Mathf.PI * 2f;
            float orbit = Mathf.Sin(
                PressureElapsedSeconds *
                    pressureOrbitDegreesPerSecond * Mathf.Deg2Rad +
                phase) * 16f;
            float angle = side * (baseAngle + orbit);

            Vector3 forward = Vector3.ProjectOnPlane(
                playerTransform.forward,
                Vector3.up).normalized;
            if (forward.sqrMagnitude <= 0.001f)
            {
                forward = Vector3.forward;
            }
            Vector3 radial = Quaternion.AngleAxis(angle, Vector3.up) * forward;
            float ringRadius = runner.IsElite
                ? elitePressureRingRadius + lane * 0.08f
                : normalPressureRingRadius + lane * 0.13f;
            worldPosition = playerTransform.position +
                radial.normalized * Mathf.Max(0.9f, ringRadius);
            worldPosition.y = runner.transform.position.y;
            movementSpeedScale = Mathf.Clamp(
                pressureMovementSpeedScale *
                (runner.IsElite &&
                 elitePressurePriority != ShowcaseElitePressurePriority.Default
                    ? 1.08f
                    : 1f),
                0.5f,
                1.2f);
            return true;
        }

        private void InitializePressureRuntime(float timestamp)
        {
            ClearAttackBudget(true);
            pressureStartedAt = timestamp;
            nextGlobalAttackStartTime = float.NegativeInfinity;
            playerStationarySince = timestamp;
            playerPositionObserved = playerTransform != null;
            lastObservedPlayerPosition = playerTransform != null
                ? playerTransform.position
                : Vector3.zero;
            elitePressurePriority = ShowcaseElitePressurePriority.Default;
        }

        private void ClearAttackBudget(bool clearActorCooldowns)
        {
            normalAttackers.Clear();
            eliteAttackers.Clear();
            nextGlobalAttackStartTime = float.NegativeInfinity;
            if (clearActorCooldowns)
            {
                nextActorAttackStartTimes.Clear();
            }
        }

        private void CleanupAttackBudget()
        {
            normalAttackers.RemoveWhere(runner =>
                !IsLiveAutonomousRunner(runner));
            eliteAttackers.RemoveWhere(runner =>
                !IsLiveAutonomousRunner(runner));
        }

        private void UpdatePlayerPressureContext()
        {
            if (playerTransform == null)
            {
                playerPositionObserved = false;
                elitePressurePriority = ShowcaseElitePressurePriority.Default;
                return;
            }

            Vector3 currentPosition = playerTransform.position;
            if (!playerPositionObserved)
            {
                playerPositionObserved = true;
                lastObservedPlayerPosition = currentPosition;
                playerStationarySince = Time.time;
            }
            else if (PlanarDistance(
                         currentPosition,
                         lastObservedPlayerPosition) >= 0.025f)
            {
                playerStationarySince = Time.time;
                lastObservedPlayerPosition = currentPosition;
            }

            ShowcaseCargoStrapController cargo = ResolveCargoStrap();
            if (cargo != null && cargo.IsCharging)
            {
                elitePressurePriority =
                    ShowcaseElitePressurePriority.PlayerCharging;
            }
            else if (Time.time - playerStationarySince >=
                     Mathf.Max(0.5f, stationaryPrioritySeconds))
            {
                elitePressurePriority =
                    ShowcaseElitePressurePriority.PlayerStationary;
            }
            else if (IsPlayerFocusingAnotherEnemy())
            {
                elitePressurePriority =
                    ShowcaseElitePressurePriority.PlayerFocusingAnotherEnemy;
            }
            else
            {
                elitePressurePriority = ShowcaseElitePressurePriority.Default;
            }
        }

        private bool IsPlayerFocusingAnotherEnemy()
        {
            Vector3 forward = Vector3.ProjectOnPlane(
                playerTransform.forward,
                Vector3.up).normalized;
            if (forward.sqrMagnitude <= 0.001f || runners == null)
            {
                return false;
            }

            for (int index = 0; index < runners.Length; index++)
            {
                ShowcaseRunnerAgent runner = runners[index];
                if (!IsLiveAutonomousRunner(runner) || runner.IsElite)
                {
                    continue;
                }

                Vector3 offset = Vector3.ProjectOnPlane(
                    runner.transform.position - playerTransform.position,
                    Vector3.up);
                if (offset.sqrMagnitude <= 36f &&
                    offset.sqrMagnitude > 0.001f &&
                    Vector3.Dot(forward, offset.normalized) >= 0.72f)
                {
                    return true;
                }
            }
            return false;
        }

        private bool HasLivingEnabledNormalRunner()
        {
            if (runners == null)
            {
                return false;
            }
            for (int index = 0; index < runners.Length; index++)
            {
                ShowcaseRunnerAgent runner = runners[index];
                if (IsLiveAutonomousRunner(runner) && !runner.IsElite)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsLiveAutonomousRunner(ShowcaseRunnerAgent runner)
        {
            return runner != null && runner.enabled &&
                runner.gameObject.activeInHierarchy && !runner.IsDead &&
                !runner.SuppressesAutonomousMovement;
        }

        private ShowcaseRunnerAgent ResolveFirstActiveAttacker()
        {
            if (runners != null)
            {
                for (int index = 0; index < runners.Length; index++)
                {
                    ShowcaseRunnerAgent runner = runners[index];
                    if (IsRunnerActiveAttacker(runner))
                    {
                        return runner;
                    }
                }
            }
            foreach (ShowcaseRunnerAgent runner in normalAttackers)
            {
                if (runner != null)
                {
                    return runner;
                }
            }
            foreach (ShowcaseRunnerAgent runner in eliteAttackers)
            {
                if (runner != null)
                {
                    return runner;
                }
            }
            return null;
        }

        private int ResolveRunnerPressureOrdinal(ShowcaseRunnerAgent runner)
        {
            if (runners != null)
            {
                for (int index = 0; index < runners.Length; index++)
                {
                    if (runners[index] == runner)
                    {
                        return index;
                    }
                }
            }
            return (int)(StableRunnerHash(runner, 0x85EBCA6Bu) % 6u);
        }

        private static float ResolveDeterministic01(
            ShowcaseRunnerAgent runner,
            uint salt,
            uint sequence)
        {
            uint value = StableRunnerHash(runner, salt) ^
                (sequence * 0x9E3779B9u);
            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;
            return (value & 0x00FFFFFFu) / 16777215f;
        }

        private static uint StableRunnerHash(
            ShowcaseRunnerAgent runner,
            uint salt)
        {
            uint hash = 2166136261u ^ salt;
            string value = runner != null &&
                !string.IsNullOrWhiteSpace(runner.RunnerId)
                    ? runner.RunnerId
                    : "RUNNER";
            for (int index = 0; index < value.Length; index++)
            {
                hash ^= value[index];
                hash *= 16777619u;
            }
            return hash != 0u ? hash : 0xA341316Cu;
        }

        private ShowcaseRunnerAgent ResolveCombatTagTarget()
        {
            if (!CanSimulatePressure() || allyTransform == null)
            {
                return null;
            }

            ShowcaseRunnerAgent best = null;
            float bestDistance = float.PositiveInfinity;
            for (int index = 0; index < targetableRunners.Length; index++)
            {
                ShowcaseRunnerAgent runner = targetableRunners[index];
                if (runner == null || !runner.gameObject.activeInHierarchy ||
                    !runner.CanReceiveCombatTag)
                {
                    continue;
                }

                float distance = PlanarDistance(
                    allyTransform.position,
                    runner.transform.position);
                if (distance > combatTagRange || !HasClearLane(
                        allyTransform.position + Vector3.up,
                        runner.transform.position + Vector3.up))
                {
                    continue;
                }

                if (distance < bestDistance - 0.0001f ||
                    (Mathf.Abs(distance - bestDistance) <= 0.0001f &&
                     (best == null || string.CompareOrdinal(runner.RunnerId, best.RunnerId) < 0)))
                {
                    best = runner;
                    bestDistance = distance;
                }
            }
            return best;
        }

        private ShowcaseRunnerAgent ResolvePlayerMeleeTarget()
        {
            if (!CanSimulatePressure() || playerTransform == null)
            {
                return null;
            }

            Vector3 forward = Vector3.ProjectOnPlane(playerTransform.forward, Vector3.up).normalized;
            ShowcaseRunnerAgent best = null;
            float bestDistance = float.PositiveInfinity;
            for (int index = 0; index < targetableRunners.Length; index++)
            {
                ShowcaseRunnerAgent runner = targetableRunners[index];
                if (runner == null || !runner.gameObject.activeInHierarchy ||
                    runner.IsDead ||
                    runner.State == ShowcaseRunnerState.GrabCommitted)
                {
                    continue;
                }

                Vector3 offset = Vector3.ProjectOnPlane(
                    runner.transform.position - playerTransform.position,
                    Vector3.up);
                float distance = offset.magnitude;
                if (distance > playerMeleeRange || offset.sqrMagnitude <= 0.001f ||
                    Vector3.Dot(forward, offset.normalized) < PlayerMeleeFacingDot ||
                    !HasClearLane(
                        playerTransform.position + Vector3.up,
                        runner.transform.position + Vector3.up))
                {
                    continue;
                }

                if (distance < bestDistance - 0.0001f ||
                    (Mathf.Abs(distance - bestDistance) <= 0.0001f &&
                     (best == null || string.CompareOrdinal(runner.RunnerId, best.RunnerId) < 0)))
                {
                    best = runner;
                    bestDistance = distance;
                }
            }
            return best;
        }

        private bool IsValidMeleeContactTarget(ShowcaseRunnerAgent target)
        {
            if (target == null || target.IsDead || playerTransform == null ||
                !CanSimulatePressure() || playerCombat == null ||
                target != playerCombat.ActiveMeleeTarget)
            {
                return false;
            }

            Vector3 offset = Vector3.ProjectOnPlane(
                target.transform.position - playerTransform.position,
                Vector3.up);
            Vector3 forward = Vector3.ProjectOnPlane(
                playerTransform.forward,
                Vector3.up).normalized;
            return offset.sqrMagnitude > 0.001f &&
                Vector3.Dot(forward, offset.normalized) >= PlayerMeleeFacingDot &&
                HasClearLane(
                    playerTransform.position + Vector3.up,
                    target.transform.position + Vector3.up);
        }

        private ShowcaseCargoStrapController ResolveCargoStrap()
        {
            if (cargoStrap == null && playerTransform != null)
            {
                cargoStrap = playerTransform.GetComponent<ShowcaseCargoStrapController>();
            }
            return cargoStrap;
        }

        private static Vector3 ResolveObjectImpactDirection(
            PrototypeInteractionTarget source,
            ShowcaseRunnerAgent runner)
        {
            Rigidbody sourceBody = source != null
                ? source.GetComponent<Rigidbody>()
                : null;
            Vector3 direction = sourceBody != null
                ? Vector3.ProjectOnPlane(sourceBody.linearVelocity, Vector3.up)
                : Vector3.zero;
            if (direction.sqrMagnitude <= 0.001f && source != null && runner != null)
            {
                direction = Vector3.ProjectOnPlane(
                    runner.transform.position - source.transform.position,
                    Vector3.up);
            }
            return direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : Vector3.forward;
        }

        private static Vector3 ResolveObjectImpactPoint(
            PrototypeInteractionTarget source,
            ShowcaseRunnerAgent runner)
        {
            Collider sourceCollider = source != null
                ? source.GetComponent<Collider>()
                : null;
            Collider runnerCollider = runner != null
                ? runner.GetComponent<Collider>()
                : null;
            if (sourceCollider != null && runnerCollider != null)
            {
                return runnerCollider.ClosestPoint(sourceCollider.bounds.center);
            }
            return runner != null
                ? runner.transform.position + Vector3.up
                : Vector3.zero;
        }

        private bool HasClearLane(Vector3 start, Vector3 end)
        {
            Vector3 displacement = end - start;
            float distance = displacement.magnitude;
            if (distance <= 0.05f)
            {
                return true;
            }

            int count = Physics.RaycastNonAlloc(
                start,
                displacement / distance,
                visibilityHits,
                distance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            for (int index = 0; index < count; index++)
            {
                Transform hit = visibilityHits[index].collider?.transform;
                bool belongsToRunner = false;
                for (int runnerIndex = 0; runnerIndex < runners.Length; runnerIndex++)
                {
                    if (runners[runnerIndex] != null &&
                        BelongsTo(hit, runners[runnerIndex].transform))
                    {
                        belongsToRunner = true;
                        break;
                    }
                }
                if (hit == null || BelongsTo(hit, playerTransform) ||
                    BelongsTo(hit, allyTransform) || belongsToRunner)
                {
                    continue;
                }
                return false;
            }
            return count < VisibilityHitCapacity;
        }

        private static bool BelongsTo(Transform candidate, Transform root)
        {
            return candidate != null && root != null &&
                (candidate == root || candidate.IsChildOf(root));
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) &&
                   IsFinite(value.y) &&
                   IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static float PlanarDistance(Vector3 first, Vector3 second)
        {
            return Vector3.ProjectOnPlane(first - second, Vector3.up).magnitude;
        }
    }
}
