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
        {
            Attacker = attacker;
            Victim = victim;
            Accepted = accepted;
            WorldPoint = worldPoint;
        }

        public Transform Attacker { get; }
        public Transform Victim { get; }
        public bool Accepted { get; }
        public Vector3 WorldPoint { get; }
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

    [DefaultExecutionOrder(-30)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseRunnerPressureController : MonoBehaviour
    {
        private const int VisibilityHitCapacity = 32;
        private const float PlayerMeleeFacingDot = 0.7f;
        private const float TeamAttackGapSeconds = 0.4f;

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

        [SerializeField, Min(1f)]
        private float rescueRange = 7f;

        [SerializeField, Min(1f)]
        private float combatTagRange = 7f;

        [SerializeField, Min(0.5f)]
        private float playerMeleeRange = 0.9f;

        [SerializeField, Min(0.1f)]
        private float playerMeleeCooldown = 0.45f;

        private readonly RaycastHit[] visibilityHits =
            new RaycastHit[VisibilityHitCapacity];
        private ShowcaseRunnerAgent attackTokenOwner;
        private ShowcaseRunnerAgent grabbingRunner;
        private int observedResetCount;
        private float nextPlayerMeleeTime;
        private bool externallySuspended;
        private float nextTeamAttackTime;

        [SerializeField]
        private ShowcasePlayerCombatController playerCombat;

        public ShowcaseRunnerAgent[] Runners => runners;

        public ShowcaseRunnerAgent AttackTokenOwner => attackTokenOwner;

        public int ActiveAttackTokenCount => attackTokenOwner != null ? 1 : 0;

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

        public float RescueRange => rescueRange;

        public float CombatTagRange => combatTagRange;

        public float PlayerMeleeRange => playerMeleeRange;

        public bool IsExternallySuspended => externallySuspended;
        public Transform PlayerTransform => playerTransform;
        public ShowcaseCombatHealth PlayerHealth => playerCombat?.Health;

        public event Action<ShowcaseCombatContactResult> CombatContactResolved;
        public event Action BaselineReset;

        private void Update()
        {
            if (resetController != null && resetController.ResetCount != observedResetCount)
            {
                observedResetCount = resetController.ResetCount;
                ResetToRunBaseline();
            }
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
            nextTeamAttackTime = 0f;
            playerCombat = player != null
                ? player.GetComponent<ShowcasePlayerCombatController>()
                : null;
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
                attackTokenOwner = null;
            }
        }

        public bool TryAcquireAttackToken(ShowcaseRunnerAgent runner)
        {
            if (!CanSimulatePressure() || runner == null ||
                Time.time < nextTeamAttackTime ||
                (attackTokenOwner != null && attackTokenOwner != runner))
            {
                return false;
            }

            attackTokenOwner = runner;
            return true;
        }

        public void ReleaseAttackToken(ShowcaseRunnerAgent runner)
        {
            if (attackTokenOwner == runner)
            {
                attackTokenOwner = null;
                nextTeamAttackTime = Time.time + TeamAttackGapSeconds;
            }
        }

        public bool TryCommitGrab(ShowcaseRunnerAgent runner)
        {
            if (!CanSimulatePressure() || runner == null ||
                attackTokenOwner != runner || grabbingRunner != null ||
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
                interactionOwner.StrapState != PrototypeStrapOwnershipState.Stored ||
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
            if (runner == null || source == null || impactSpeed < 1.5f)
            {
                return;
            }

            if (source.WeightClass == PrototypeWeightClass.Light)
            {
                runner.ReceivePhysicalStagger(false);
                PhysicalImpactCount++;
            }
            else if (source.WeightClass == PrototypeWeightClass.Medium)
            {
                runner.ReceivePhysicalStagger(true);
                PhysicalImpactCount++;
            }
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
            int damage)
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
                target.transform.position + Vector3.up));
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
            attackTokenOwner = null;
            grabbingRunner = null;
            PlayerHitCount = 0;
            GrabCount = 0;
            RescueCount = 0;
            CombatTagCount = 0;
            PlayerMeleeCount = 0;
            PhysicalImpactCount = 0;
            nextPlayerMeleeTime = 0f;
            nextTeamAttackTime = 0f;
            externallySuspended = false;
            foreach (ShowcaseRunnerAgent runner in runners)
            {
                runner?.ResetToRunBaseline();
            }
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

        private ShowcaseRunnerAgent ResolveCombatTagTarget()
        {
            if (!CanSimulatePressure() || allyTransform == null)
            {
                return null;
            }

            ShowcaseRunnerAgent best = null;
            float bestDistance = float.PositiveInfinity;
            for (int index = 0; index < runners.Length; index++)
            {
                ShowcaseRunnerAgent runner = runners[index];
                if (runner == null || !runner.CanReceiveCombatTag)
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
            for (int index = 0; index < runners.Length; index++)
            {
                ShowcaseRunnerAgent runner = runners[index];
                if (runner == null || runner.IsDead ||
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

        private static float PlanarDistance(Vector3 first, Vector3 second)
        {
            return Vector3.ProjectOnPlane(first - second, Vector3.up).magnitude;
        }
    }
}
