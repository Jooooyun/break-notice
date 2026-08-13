using System;
using BreakNotice.Gameplay.Encounter;
using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    public enum ShowcaseDrawerSolution
    {
        None,
        DirectChairThrow,
        CargoStrapRedirect,
        RunnerCausalCollision,
        TeamFinisher
    }

    [Flags]
    public enum ShowcaseDrawerSolutionMask
    {
        None = 0,
        DirectChairThrow = 1 << 0,
        CargoStrapRedirect = 1 << 1,
        RunnerCausalCollision = 1 << 2,
        TeamFinisher = 1 << 3
    }

    [DefaultExecutionOrder(25)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseObjectiveController : MonoBehaviour
    {
        [SerializeField]
        private PrototypeInteractionStateOwner interactionOwner;

        [SerializeField]
        private ThirdPersonLocomotion playerLocomotion;

        [SerializeField]
        private Transform playerTransform;

        [SerializeField]
        private AuthoredEncounterResetController resetController;

        [SerializeField]
        private ShowcaseTeamContextController teamContextController;

        [SerializeField]
        private ShowcaseRunnerPressureController runnerPressureController;

        [SerializeField]
        private PrototypeInteractionTarget drawerTarget;

        [SerializeField]
        private Rigidbody drawerBody;

        [SerializeField]
        private Transform parkingBayCenter;

        [SerializeField]
        private Transform exitCenter;

        [SerializeField]
        private Renderer exitSignRenderer;

        [SerializeField, Min(0.5f)]
        private float parkingHalfExtent = 1.65f;

        [SerializeField, Min(0.5f)]
        private float exitHalfExtent = 1.15f;

        [SerializeField, Min(0.1f)]
        private float minimumDrawerImpactSpeed = 2.2f;

        [SerializeField, Min(1)]
        private int maximumPlayerHits = 5;

        private MaterialPropertyBlock signPropertyBlock;
        private int observedResetCount;
        private float runStartTime;
        private ShowcaseDrawerSolution pendingSolution;
        private AuthoredEncounterResetController subscribedResetController;

        public bool DrawerSolved { get; private set; }

        public bool ExitOpen => DrawerSolved;

        public bool RunComplete { get; private set; }

        public bool FailureTriggered { get; private set; }

        public ShowcaseDrawerSolution DrawerSolution { get; private set; }

        public ShowcaseDrawerSolutionMask DiscoveredSolutions { get; private set; }

        public int DrawerImpactCount { get; private set; }

        public float ClearTimeSeconds { get; private set; }

        public int PhysicalSolutionsDiscovered => CountSolutions(DiscoveredSolutions);

        public int TeamTagCount => teamContextController != null
            ? teamContextController.BadgeCount
            : 0;

        public int LongestCausalChain { get; private set; }

        public int AlternateRoutesUsed => RunComplete ? 1 : 0;

        public int MaximumPlayerHits => maximumPlayerHits;

        public float MinimumDrawerImpactSpeed => minimumDrawerImpactSpeed;

        public PrototypeInteractionTarget DrawerTarget => drawerTarget;

        public Rigidbody DrawerBody => drawerBody;

        public Transform ParkingBayCenter => parkingBayCenter;

        public Transform ExitCenter => exitCenter;

        public string DrawerSolutionLabel => GetSolutionLabel(DrawerSolution);

        private void OnEnable()
        {
            RefreshResetSubscription();
        }

        private void OnDisable()
        {
            RemoveResetSubscription();
        }

        private void Start()
        {
            RefreshResetSubscription();
            observedResetCount = resetController != null ? resetController.ResetCount : 0;
            runStartTime = Time.time;
            SetExitSignOpen(false);
        }

        private void Update()
        {
            if (resetController != null && resetController.ResetCount != observedResetCount)
            {
                observedResetCount = resetController.ResetCount;
                ResetToRunBaseline();
            }

            EvaluateObjectiveState();
        }

        public void Configure(
            PrototypeInteractionStateOwner owner,
            ThirdPersonLocomotion locomotion,
            Transform player,
            AuthoredEncounterResetController reset,
            ShowcaseTeamContextController teamContext,
            ShowcaseRunnerPressureController runnerPressure,
            PrototypeInteractionTarget authoredDrawerTarget,
            Rigidbody authoredDrawerBody,
            Transform authoredParkingBayCenter,
            Transform authoredExitCenter,
            Renderer authoredExitSignRenderer,
            float authoredParkingHalfExtent = 1.65f,
            float authoredExitHalfExtent = 1.15f,
            float authoredMinimumImpactSpeed = 2.2f,
            int authoredMaximumPlayerHits = 5)
        {
            interactionOwner = owner;
            playerLocomotion = locomotion;
            playerTransform = player;
            resetController = reset;
            teamContextController = teamContext;
            runnerPressureController = runnerPressure;
            drawerTarget = authoredDrawerTarget;
            drawerBody = authoredDrawerBody;
            parkingBayCenter = authoredParkingBayCenter;
            exitCenter = authoredExitCenter;
            exitSignRenderer = authoredExitSignRenderer;
            parkingHalfExtent = Mathf.Max(0.5f, authoredParkingHalfExtent);
            exitHalfExtent = Mathf.Max(0.5f, authoredExitHalfExtent);
            minimumDrawerImpactSpeed = Mathf.Max(0.1f, authoredMinimumImpactSpeed);
            maximumPlayerHits = Mathf.Max(1, authoredMaximumPlayerHits);
            observedResetCount = resetController != null ? resetController.ResetCount : 0;
            runStartTime = Time.time;
            RefreshResetSubscription();
            SetExitSignOpen(false);
        }

        public bool TryRegisterDrawerImpact(
            ShowcaseDrawerSolution solution,
            Vector3 impactDirection,
            float impactSpeed)
        {
            if (solution == ShowcaseDrawerSolution.None ||
                DrawerSolved || RunComplete || FailureTriggered ||
                interactionOwner == null || interactionOwner.BlocksNewInteraction ||
                drawerBody == null || parkingBayCenter == null ||
                impactSpeed < minimumDrawerImpactSpeed ||
                (solution == ShowcaseDrawerSolution.TeamFinisher &&
                 (teamContextController == null || !teamContextController.IsTeamReady)))
            {
                return false;
            }

            Vector3 towardParking = Vector3.ProjectOnPlane(
                parkingBayCenter.position - drawerBody.position,
                Vector3.up);
            if (towardParking.sqrMagnitude <= 0.001f)
            {
                return false;
            }

            Vector3 authoredImpact = Vector3.ProjectOnPlane(impactDirection, Vector3.up);
            if (authoredImpact.sqrMagnitude > 0.001f &&
                Vector3.Dot(authoredImpact.normalized, towardParking.normalized) < -0.25f)
            {
                return false;
            }

            pendingSolution = solution;
            DiscoveredSolutions |= ToMask(solution);
            DrawerImpactCount++;
            LongestCausalChain = Mathf.Max(LongestCausalChain, GetCausalDepth(solution));

            drawerBody.isKinematic = false;
            drawerBody.WakeUp();
            float assistedSpeed = Mathf.Clamp(impactSpeed * 0.72f, 3.8f, 7.2f);
            Vector3 existing = drawerBody.linearVelocity;
            drawerBody.linearVelocity =
                towardParking.normalized * assistedSpeed +
                Vector3.up * Mathf.Min(existing.y, 0f);
            drawerBody.angularVelocity = Vector3.zero;
            return true;
        }

        public bool NotifyTeamFinisherDrawerImpact(Vector3 direction, float impactSpeed)
        {
            return TryRegisterDrawerImpact(
                ShowcaseDrawerSolution.TeamFinisher,
                direction,
                impactSpeed);
        }

        public void EvaluateObjectiveState()
        {
            if (FailureTriggered || RunComplete)
            {
                return;
            }

            if (TryTriggerFailureIfThresholdReached())
            {
                return;
            }

            if (!DrawerSolved && pendingSolution != ShowcaseDrawerSolution.None &&
                IsInsidePlanarZone(drawerTarget != null ? drawerTarget.transform : null,
                    parkingBayCenter, parkingHalfExtent))
            {
                CompleteDrawerObjective();
            }

            if (DrawerSolved)
            {
                TryCompleteExit();
            }
        }

        public bool TryTriggerFailureIfThresholdReached()
        {
            if (FailureTriggered || RunComplete || runnerPressureController == null ||
                runnerPressureController.PlayerHitCount < maximumPlayerHits ||
                interactionOwner == null || !interactionOwner.TryBeginFailurePending())
            {
                return false;
            }

            FailureTriggered = true;
            SuspendGameplay(true);
            return true;
        }

        public bool TryCompleteExit()
        {
            if (!DrawerSolved || RunComplete || FailureTriggered ||
                !IsInsidePlanarZone(playerTransform, exitCenter, exitHalfExtent))
            {
                return false;
            }

            RunComplete = true;
            ClearTimeSeconds = Mathf.Max(0f, Time.time - runStartTime);
            SuspendGameplay(true);
            return true;
        }

        public bool RequestReplay()
        {
            return resetController != null &&
                resetController.RequestReset(EncounterResetCause.ManualInput);
        }

        public void ResetToRunBaseline()
        {
            DrawerSolved = false;
            RunComplete = false;
            FailureTriggered = false;
            DrawerSolution = ShowcaseDrawerSolution.None;
            pendingSolution = ShowcaseDrawerSolution.None;
            DiscoveredSolutions = ShowcaseDrawerSolutionMask.None;
            DrawerImpactCount = 0;
            ClearTimeSeconds = 0f;
            LongestCausalChain = 0;
            runStartTime = Time.time;
            SuspendGameplay(false);
            SetExitSignOpen(false);
        }

        private void CompleteDrawerObjective()
        {
            DrawerSolved = true;
            DrawerSolution = pendingSolution;
            if (drawerBody != null)
            {
                drawerBody.linearVelocity = Vector3.zero;
                drawerBody.angularVelocity = Vector3.zero;
                drawerBody.isKinematic = true;
            }
            SetExitSignOpen(true);
        }

        private void HandleResetPerformed(EncounterResetCause cause)
        {
            observedResetCount = resetController != null ? resetController.ResetCount : 0;
            ResetToRunBaseline();
        }

        private void RefreshResetSubscription()
        {
            if (!isActiveAndEnabled || subscribedResetController == resetController)
            {
                return;
            }

            RemoveResetSubscription();
            subscribedResetController = resetController;
            if (subscribedResetController != null)
            {
                subscribedResetController.ResetPerformed += HandleResetPerformed;
            }
        }

        private void RemoveResetSubscription()
        {
            if (subscribedResetController != null)
            {
                subscribedResetController.ResetPerformed -= HandleResetPerformed;
                subscribedResetController = null;
            }
        }

        private void SuspendGameplay(bool suspended)
        {
            runnerPressureController?.SetExternalSuspended(suspended);
            playerLocomotion?.SetExternalMovementBlocked(suspended);
        }

        private void SetExitSignOpen(bool open)
        {
            if (exitSignRenderer == null)
            {
                return;
            }

            Color color = open
                ? new Color(0.25f, 1f, 0.38f, 1f)
                : new Color(1f, 0.28f, 0.12f, 1f);
            signPropertyBlock ??= new MaterialPropertyBlock();
            signPropertyBlock.Clear();
            signPropertyBlock.SetColor("_BaseColor", color);
            signPropertyBlock.SetColor("_Color", color);
            exitSignRenderer.SetPropertyBlock(signPropertyBlock);
        }

        private static bool IsInsidePlanarZone(
            Transform candidate,
            Transform center,
            float halfExtent)
        {
            if (candidate == null || center == null)
            {
                return false;
            }

            Vector3 delta = candidate.position - center.position;
            return Mathf.Abs(delta.x) <= halfExtent &&
                Mathf.Abs(delta.z) <= halfExtent;
        }

        private static ShowcaseDrawerSolutionMask ToMask(ShowcaseDrawerSolution solution)
        {
            return solution switch
            {
                ShowcaseDrawerSolution.DirectChairThrow =>
                    ShowcaseDrawerSolutionMask.DirectChairThrow,
                ShowcaseDrawerSolution.CargoStrapRedirect =>
                    ShowcaseDrawerSolutionMask.CargoStrapRedirect,
                ShowcaseDrawerSolution.RunnerCausalCollision =>
                    ShowcaseDrawerSolutionMask.RunnerCausalCollision,
                ShowcaseDrawerSolution.TeamFinisher =>
                    ShowcaseDrawerSolutionMask.TeamFinisher,
                _ => ShowcaseDrawerSolutionMask.None
            };
        }

        private static int CountSolutions(ShowcaseDrawerSolutionMask mask)
        {
            int count = 0;
            if ((mask & ShowcaseDrawerSolutionMask.DirectChairThrow) != 0) count++;
            if ((mask & ShowcaseDrawerSolutionMask.CargoStrapRedirect) != 0) count++;
            if ((mask & ShowcaseDrawerSolutionMask.RunnerCausalCollision) != 0) count++;
            if ((mask & ShowcaseDrawerSolutionMask.TeamFinisher) != 0) count++;
            return count;
        }

        private static int GetCausalDepth(ShowcaseDrawerSolution solution)
        {
            return solution switch
            {
                ShowcaseDrawerSolution.DirectChairThrow => 1,
                ShowcaseDrawerSolution.CargoStrapRedirect => 2,
                ShowcaseDrawerSolution.RunnerCausalCollision => 2,
                ShowcaseDrawerSolution.TeamFinisher => 3,
                _ => 0
            };
        }

        public static string GetSolutionLabel(ShowcaseDrawerSolution solution)
        {
            return solution switch
            {
                ShowcaseDrawerSolution.DirectChairThrow => "DIRECT CHAIR THROW",
                ShowcaseDrawerSolution.CargoStrapRedirect => "CARGO STRAP REDIRECT",
                ShowcaseDrawerSolution.RunnerCausalCollision => "RUNNER CAUSAL COLLISION",
                ShowcaseDrawerSolution.TeamFinisher => "TEAM FINISHER",
                _ => "NOT SOLVED"
            };
        }
    }
}
