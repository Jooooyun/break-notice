using System;
using System.Collections.Generic;
using BreakNotice.Gameplay.Encounter;
using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
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
        Recovering
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
        private float playerMeleeRange = 2.1f;

        [SerializeField, Min(0.1f)]
        private float playerMeleeCooldown = 0.45f;

        private readonly RaycastHit[] visibilityHits =
            new RaycastHit[VisibilityHitCapacity];
        private ShowcaseRunnerAgent attackTokenOwner;
        private ShowcaseRunnerAgent grabbingRunner;
        private int observedResetCount;
        private float nextPlayerMeleeTime;

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
            playerMeleeRange = Mathf.Max(0.5f, authoredMeleeRange);
            observedResetCount = resetController != null ? resetController.ResetCount : 0;
            nextPlayerMeleeTime = 0f;
        }

        public bool CanSimulatePressure()
        {
            return interactionOwner != null &&
                interactionOwner.GlobalState == PrototypeGlobalInteractionState.Playable;
        }

        public bool TryAcquireAttackToken(ShowcaseRunnerAgent runner)
        {
            if (!CanSimulatePressure() || runner == null ||
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
            playerLocomotion?.SetExternalMovementBlocked(true);
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
            if (!CanSimulatePressure() || PlayerGrabbed || Time.time < nextPlayerMeleeTime ||
                interactionOwner.HandState != PrototypeHandOwnershipState.Free ||
                interactionOwner.StrapState != PrototypeStrapOwnershipState.Stored ||
                interactionOwner.RelayState != PrototypeRelayReservationState.None)
            {
                return false;
            }

            ShowcaseRunnerAgent target = ResolvePlayerMeleeTarget();
            if (target == null)
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

        public void ResetToRunBaseline()
        {
            playerLocomotion?.SetExternalMovementBlocked(false);
            attackTokenOwner = null;
            grabbingRunner = null;
            PlayerHitCount = 0;
            GrabCount = 0;
            RescueCount = 0;
            CombatTagCount = 0;
            PlayerMeleeCount = 0;
            PhysicalImpactCount = 0;
            nextPlayerMeleeTime = 0f;
            foreach (ShowcaseRunnerAgent runner in runners)
            {
                runner?.ResetToRunBaseline();
            }
        }

        private void ReleaseGrab(bool rescued)
        {
            ShowcaseRunnerAgent previous = grabbingRunner;
            grabbingRunner = null;
            playerLocomotion?.SetExternalMovementBlocked(false);
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
                if (runner == null || runner.IsIncapacitated)
                {
                    continue;
                }

                Vector3 offset = Vector3.ProjectOnPlane(
                    runner.transform.position - playerTransform.position,
                    Vector3.up);
                float distance = offset.magnitude;
                if (distance > playerMeleeRange || offset.sqrMagnitude <= 0.001f ||
                    Vector3.Dot(forward, offset.normalized) < 0.35f ||
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
