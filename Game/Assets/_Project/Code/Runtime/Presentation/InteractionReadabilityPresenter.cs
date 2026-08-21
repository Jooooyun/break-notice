using System.Globalization;
using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Presentation
{
    [DefaultExecutionOrder(200)]
    [DisallowMultipleComponent]
    public sealed class InteractionReadabilityPresenter : MonoBehaviour
    {
        private const float FeedbackDuration = 0.9f;
        private const int ThrowTrajectoryCapacity = 26;

        [SerializeField]
        private PrototypeInteractionController interactionController;

        private GUIStyle promptStyle;
        private GUIStyle crosshairStyle;
        private GUIStyle targetStyle;
        private GUIStyle feedbackStyle;
        private GUIStyle allyIndicatorStyle;
        private uint observedAttemptSequence;
        private uint observedAllyCallSequence;
        private uint observedAllyTaskSequence;
        private string activeFeedback = string.Empty;
        private float feedbackExpiresAt;
        private readonly Vector3[] throwTrajectoryPoints =
            new Vector3[ThrowTrajectoryCapacity];
        private LineRenderer throwTrajectoryLine;
        private Material throwTrajectoryMaterial;

        public bool IsThrowTrajectoryVisible =>
            throwTrajectoryLine != null && throwTrajectoryLine.enabled;

        public int ThrowTrajectoryPointCount => throwTrajectoryLine != null
            ? throwTrajectoryLine.positionCount
            : 0;

        public void Configure(PrototypeInteractionController controller)
        {
            interactionController = controller;
        }

        public string BuildStatusText()
        {
            if (interactionController == null || interactionController.InteractionOwner == null)
            {
                return string.Empty;
            }

            PrototypeInteractionStateOwner owner = interactionController.InteractionOwner;
            if (owner.GlobalState == PrototypeGlobalInteractionState.ResetPending)
            {
                return "RESET | RESTORING AUTHORED BASELINE";
            }

            if (owner.GlobalState == PrototypeGlobalInteractionState.FailurePending)
            {
                return "INTERACTIONS BLOCKED | FAILURE PENDING";
            }

            ShowcaseCargoStrapController cargoStrap =
                interactionController.ShowcaseCargoStrapController;
            if (cargoStrap != null &&
                cargoStrap.Phase != ShowcaseCargoStrapPhase.Stored)
            {
                string targetName = cargoStrap.ActiveTarget != null
                    ? cargoStrap.ActiveTarget.DisplayName
                    : "OBJECT";
                if (cargoStrap.Phase == ShowcaseCargoStrapPhase.Prepare)
                {
                    return $"GEAR01 PREPARE: {targetName} | HOLD [E], RELEASE TO THROW";
                }
                if (cargoStrap.Phase == ShowcaseCargoStrapPhase.Rolling)
                {
                    return $"GEAR01 READY: {targetName} | RELEASE [E] TO ATTACH";
                }
                if (cargoStrap.Phase == ShowcaseCargoStrapPhase.Throw)
                {
                    return cargoStrap.IsTensioned &&
                           cargoStrap.ActiveTarget?.WeightClass ==
                               PrototypeWeightClass.Light
                        ? $"GEAR01 SMALL THROW RELEASE: {targetName}"
                        : $"GEAR01 CLAMP IN FLIGHT: {targetName}";
                }
                if (cargoStrap.Phase == ShowcaseCargoStrapPhase.Pull)
                {
                    return $"GEAR01 APPLYING TENSION: {targetName}";
                }
                if (cargoStrap.IsCharging)
                {
                    ShowcaseCargoChargeProfile profile =
                        cargoStrap.ActiveAttackChargeProfile;
                    ShowcaseCargoAttackResolution resolution =
                        cargoStrap.CurrentAttackResolution;
                    string fallback = resolution.IsFallback
                        ? " | FALLBACK"
                        : string.Empty;
                    return $"GEAR01 CHARGE {BuildChargeBar(profile.Charge01)} " +
                           $"{Mathf.RoundToInt(profile.Charge01 * 100f)}% " +
                           $"{profile.Tier.ToString().ToUpperInvariant()} | " +
                           $"{BuildAttackTypeLabel(resolution.AttackType)}" +
                           $"{fallback} | RELEASE LMB | [R] CANCEL";
                }
                if (cargoStrap.Phase == ShowcaseCargoStrapPhase.Releasing)
                {
                    return "GEAR01 SAFETY RELEASE | CONTROLLED RETRACTION";
                }
                if (cargoStrap.Phase == ShowcaseCargoStrapPhase.SmallSpinning)
                {
                    return $"GEAR01 SMALL SPINNING: {targetName} | " +
                           "[1] FISTS | [2] SPIN | LMB THROW | [R] RELEASE";
                }

                string state = cargoStrap.Phase == ShowcaseCargoStrapPhase.Slam ||
                               (cargoStrap.Phase ==
                                    ShowcaseCargoStrapPhase.MediumExtending &&
                                cargoStrap.ActiveAttackType ==
                                    ShowcaseCargoAttackType.OverheadSlam)
                    ? "OVERHEAD SLAM"
                    : cargoStrap.IsMediumSwinging
                        ? "MEDIUM TETHER SWING"
                    : cargoStrap.Phase == ShowcaseCargoStrapPhase.MediumExtending
                        ? "MEDIUM EXTEND"
                        : cargoStrap.IsMediumRecovering
                            ? "MEDIUM RECOVER"
                            : cargoStrap.IsMediumRearReady
                                ? "REAR READY"
                                : "REAR FOLLOW";
                string warning = cargoStrap.IsWarning ? " | LIMIT WARNING" : string.Empty;
                string weapon = cargoStrap.IsStrapWeaponSelected
                    ? "[2] STRAP OBJECT | HOLD LMB CHARGE"
                    : "[1] FISTS | [2] SELECT STRAP OBJECT";
                return $"GEAR01 {state}: {targetName} | " +
                       $"{cargoStrap.CurrentStrapLength:0.0}/{cargoStrap.MaximumStrapLength:0.0} M" +
                       $"{warning} | {weapon} | [R] RELEASE";
            }

            if (owner.HandState == PrototypeHandOwnershipState.Carrying && owner.CarriedTarget != null)
            {
                string throwPrompt = interactionController.HasThrowCharge
                    ? $"THROW CHARGE {GetThrowChargePercent()}% | RELEASE LMB"
                    : "HOLD LMB TO CHARGE THROW";
                return $"CARRYING {owner.CarriedTarget.DisplayName} " +
                       $"[{GetWeightLabel(owner.CarriedTarget)}] | {throwPrompt}";
            }

            PrototypeAllyRelayController relay = interactionController.AllyRelayController;
            if (relay != null && relay.RelayPhase == PrototypeRelayPhase.PreRelease)
            {
                return $"SUPPLY COMMITTED: {relay.ActiveTarget?.DisplayName ?? "OBJECT"} " +
                       "| PHYSICAL RELEASE PENDING";
            }

            if (relay != null && relay.IsBinderIncoming)
            {
                if (owner.QueuedThrowArmed)
                {
                    string charge = interactionController.IsQueuedThrowCharge
                        ? $" {GetThrowChargePercent()}%"
                        : string.Empty;
                    string state = interactionController.IsThrowCharging
                        ? "CHARGING"
                        : "ARMED";
                    return $"QUEUED THROW {state}{charge} | BINDER INCOMING | [E] CATCH";
                }

                return relay.CanCatch(interactionController.CurrentTarget)
                    ? "BINDER INCOMING | [E] CATCH"
                    : "BINDER INCOMING | CENTER RETICLE AND MOVE INTO CATCH RANGE";
            }

            if (relay != null && relay.IsChairFeedActive)
            {
                return "CHAIR GROUNDED FEED | SAFE PLAYER CLEARANCE | CATCH BLOCKED";
            }

            if (relay != null && relay.IsReadyForCall)
            {
                return $"ALLY READY: {relay.PreparedTarget?.DisplayName ?? "OBJECT"} " +
                       (interactionController.ShowcaseTeamContextController != null
                           ? "| AWAITING TEAM COMMAND"
                           : "| [Q] SUPPLY");
            }

            if (relay != null && relay.IsSupplyLaneBlocked)
            {
                return $"ALLY SUPPLY BLOCKED: {relay.PreparedTarget?.DisplayName ?? "OBJECT"} " +
                       "| PHYSICAL LANE OBSTRUCTED";
            }

            if (relay != null && relay.IsSupplyPreparationBlocked)
            {
                return $"ALLY PATH BLOCKED: {relay.PreparedTarget?.DisplayName ?? "OBJECT"} " +
                       "| CANNOT REACH SUPPLY";
            }

            if (relay != null &&
                relay.AllyState == PrototypeAllySupportState.ReturningWithSupply)
            {
                return $"ALLY RETURNING WITH: " +
                       (relay.PreparedTarget?.DisplayName ?? "OBJECT");
            }

            if (relay != null &&
                relay.AllyState == PrototypeAllySupportState.MovingToSupply)
            {
                return $"ALLY MOVING TO SUPPLY: " +
                       (relay.PreparedTarget?.DisplayName ?? "OBJECT");
            }

            if (relay != null &&
                relay.AllyState == PrototypeAllySupportState.AcquiringSupply)
            {
                return $"ALLY ACQUIRING: " +
                       (relay.PreparedTarget?.DisplayName ?? "OBJECT");
            }

            if (relay != null && relay.AllyState == PrototypeAllySupportState.Depleted)
            {
                return "ALLY SUPPLY DEPLETED | RESET TO RESTORE AUTHORED POOL";
            }

            PrototypeInteractionTarget target = interactionController.CurrentTarget;
            if (target == null)
            {
                if (relay != null && relay.IsAwaitingFetchTask)
                {
                    string scouted = relay.SuggestedFetchTarget != null
                        ? $"ALLY SCOUTED: {relay.SuggestedFetchTarget.DisplayName} | "
                        : string.Empty;
                    return scouted +
                           "[F] FETCH NEAREST REACHABLE BINDER OR CHAIR | " +
                           BuildAllyMovementPrompt();
                }

                return $"CENTER RETICLE OR ALIGN A FLOOR PICKUP BELOW IT | RANGE <= " +
                       $"{interactionController.InteractionRange.ToString("0.0", CultureInfo.InvariantCulture)} M " +
                       "| CLEAR LINE OF SIGHT";
            }

            string identity = $"{target.DisplayName} [{GetWeightLabel(target)}]";
            string distance = GetCurrentDistanceText();
            if (target.WeightClass == PrototypeWeightClass.Heavy)
            {
                return $"TARGET: {identity} | {distance} | RECOGNITION ONLY";
            }

            if (target.Role == PrototypeInteractionRole.Drawer)
            {
                return $"TARGET: {identity} | {distance} | IMPACT TARGET / DIRECT PICKUP BLOCKED";
            }

            if (!target.DirectAcquisitionEligible)
            {
                if (relay != null && relay.RequiresExplicitFetch &&
                    (target.Role == PrototypeInteractionRole.AllyBinder ||
                     target.Role == PrototypeInteractionRole.AllyChair))
                {
                    return $"ALLY SUPPLY OBJECT: {identity} | {distance} | [F] AUTO FETCH";
                }

                return $"TARGET: {identity} | {distance} | ALLY SUPPLY / DIRECT PICKUP BLOCKED";
            }

            string selection = interactionController.CurrentTargetUsesPickupAssist
                ? "PICKUP ASSIST"
                : "TARGET LOCKED";
            string strapPrompt = target.IsCargoStrapEligible
                ? " | RMB+[E] STRAP"
                : string.Empty;
            return $"{selection}: {identity} | {distance} | [E] PICK UP{strapPrompt}";
        }

        public string BuildReticleText()
        {
            ShowcaseCargoStrapController cargo =
                interactionController?.ShowcaseCargoStrapController;
            if (cargo != null && cargo.IsCharging)
            {
                return cargo.CurrentAttackResolution.AttackType switch
                {
                    ShowcaseCargoAttackType.SweepLeft => "<",
                    ShowcaseCargoAttackType.SweepRight => ">",
                    ShowcaseCargoAttackType.OverheadSlam => "V",
                    _ => "[!]"
                };
            }

            PrototypeAllyRelayController relay = interactionController?.AllyRelayController;
            if (relay != null &&
                relay.IsBinderIncoming &&
                relay.CanCatch(interactionController.CurrentTarget))
            {
                return "[E]";
            }

            if (relay != null && relay.IsAwaitingFetchTask &&
                interactionController?.InteractionOwner?.GlobalState ==
                    PrototypeGlobalInteractionState.Playable)
            {
                return "[F]";
            }

            PrototypeInteractionTarget target = interactionController?.CurrentTarget;
            if (target == null ||
                interactionController.InteractionOwner == null ||
                interactionController.InteractionOwner.GlobalState !=
                    PrototypeGlobalInteractionState.Playable ||
                interactionController.InteractionOwner.HandState !=
                    PrototypeHandOwnershipState.Free)
            {
                return "+";
            }

            return target.IsScoopEligible || target.IsCargoStrapEligible
                ? "[E]"
                : "[!]";
        }

        public string BuildAllyIndicatorText()
        {
            return interactionController?.AllyRelayController?.IsReadyForCall == true
                ? "!"
                : string.Empty;
        }

        public string BuildReticleLabel()
        {
            ShowcaseCargoStrapController cargo =
                interactionController?.ShowcaseCargoStrapController;
            if (cargo != null && cargo.IsCharging)
            {
                ShowcaseCargoAttackResolution resolution =
                    cargo.CurrentAttackResolution;
                string label = BuildAttackTypeLabel(resolution.AttackType);
                return resolution.IsFallback
                    ? $"{label} - PREDICTED FALLBACK"
                    : label;
            }

            PrototypeAllyRelayController relay = interactionController?.AllyRelayController;
            if (relay != null &&
                relay.IsBinderIncoming &&
                relay.CanCatch(interactionController.CurrentTarget))
            {
                return interactionController.InteractionOwner.QueuedThrowArmed
                    ? "CATCH + QUEUED THROW"
                    : "CATCH INCOMING BINDER";
            }

            if (relay != null && relay.IsAwaitingFetchTask &&
                interactionController?.InteractionOwner?.GlobalState ==
                    PrototypeGlobalInteractionState.Playable)
            {
                return "AUTO FETCH NEAREST REACHABLE OBJECT";
            }

            PrototypeInteractionTarget target = interactionController?.CurrentTarget;
            if (target == null ||
                interactionController.InteractionOwner == null ||
                interactionController.InteractionOwner.GlobalState !=
                    PrototypeGlobalInteractionState.Playable ||
                interactionController.InteractionOwner.HandState !=
                    PrototypeHandOwnershipState.Free)
            {
                return string.Empty;
            }

            if (target.IsCargoStrapEligible)
            {
                return target.IsScoopEligible
                    ? $"PICK UP OR AIM+STRAP {target.DisplayName}"
                    : $"AIM+STRAP {target.DisplayName}";
            }

            if (target.IsScoopEligible)
            {
                return interactionController.CurrentTargetUsesPickupAssist
                    ? $"PICKUP ASSIST: {target.DisplayName}"
                    : $"PICK UP {target.DisplayName}";
            }

            if (target.WeightClass == PrototypeWeightClass.Heavy)
            {
                return "HEAVY - RECOGNITION ONLY";
            }

            return "DIRECT PICKUP BLOCKED";
        }

        public string BuildLatestAttemptFeedbackText()
        {
            if (interactionController == null || interactionController.InteractionAttemptSequence == 0)
            {
                return string.Empty;
            }

            PrototypeInteractionDecision decision = interactionController.LastDecision;
            string targetName = decision.Target != null
                ? decision.Target.DisplayName
                : "OBJECT";
            if (decision.IsAccepted)
            {
                return decision.Outcome == PrototypeInteractionOutcome.Scoop ||
                       decision.Outcome == PrototypeInteractionOutcome.Catch
                    ? $"PICKUP CONFIRMED: {targetName}"
                    : $"INTERACTION CONFIRMED: {targetName}";
            }

            return decision.Rejection switch
            {
                PrototypeInteractionRejection.NoValidTarget =>
                    "NO VALID TARGET: CENTER RETICLE, MOVE WITHIN RANGE, KEEP LINE OF SIGHT",
                PrototypeInteractionRejection.GlobalStateBlocked =>
                    "INTERACTION BLOCKED: RESET OR FAILURE PENDING",
                PrototypeInteractionRejection.HandOwnershipBlocked =>
                    "INTERACTION BLOCKED: HANDS OCCUPIED",
                PrototypeInteractionRejection.StrapOwnershipBlocked =>
                    "INTERACTION BLOCKED: STRAP OCCUPIED",
                PrototypeInteractionRejection.InvalidExplicitStrapContext =>
                    "INTERACTION BLOCKED: INVALID STRAP TARGET",
                PrototypeInteractionRejection.InvalidRelayContext =>
                    "INTERACTION BLOCKED: INVALID RELAY TARGET",
                PrototypeInteractionRejection.DirectAcquisitionProhibited =>
                    $"DIRECT PICKUP BLOCKED: {targetName}",
                PrototypeInteractionRejection.HeavyRecognitionOnly =>
                    $"HEAVY RECOGNITION ONLY: {targetName}",
                _ => "INTERACTION BLOCKED: OWNERSHIP CONFLICT"
            };
        }

        public string BuildLatestAllyCallFeedbackText()
        {
            if (interactionController == null ||
                interactionController.AllyCallAttemptSequence == 0)
            {
                return string.Empty;
            }

            if (interactionController.LastAllyCallAccepted)
            {
                return $"SUPPLY COMMITTED: " +
                       (interactionController.LastAllyCallTarget?.DisplayName ?? "OBJECT");
            }

            return interactionController.LastAllyCallRejection switch
            {
                PrototypeAllyRelayRejection.GlobalStateBlocked =>
                    "CALL BLOCKED: RESET OR FAILURE PENDING",
                PrototypeAllyRelayRejection.HandOwnershipBlocked =>
                    "CALL BLOCKED: HANDS OCCUPIED",
                PrototypeAllyRelayRejection.StrapOwnershipBlocked =>
                    "CALL BLOCKED: STRAP OCCUPIED",
                PrototypeAllyRelayRejection.RelayAlreadyActive =>
                    "CALL BLOCKED: SUPPLY ALREADY COMMITTED",
                PrototypeAllyRelayRejection.SupplyAlreadyConsumed =>
                    "CALL BLOCKED: SUPPLY ALREADY USED",
                PrototypeAllyRelayRejection.InvalidPhysicalLane =>
                    "CALL BLOCKED: PHYSICAL LANE OBSTRUCTED",
                PrototypeAllyRelayRejection.InvalidPhysicalTarget =>
                    "CALL BLOCKED: INVALID PHYSICAL SUPPLY",
                PrototypeAllyRelayRejection.AllyUnavailable =>
                    "CALL BLOCKED: ALLY NOT READY",
                PrototypeAllyRelayRejection.NoValidSupply =>
                    "CALL BLOCKED: SUPPLY DEPLETED",
                _ => "CALL BLOCKED: NO VALID SUPPLY"
            };
        }

        public string BuildLatestAllyTaskFeedbackText()
        {
            if (interactionController == null ||
                interactionController.AllyTaskAttemptSequence == 0)
            {
                return string.Empty;
            }

            if (interactionController.LastAllyTaskAccepted)
            {
                return $"FETCH ORDER ACCEPTED: " +
                       (interactionController.LastAllyTaskTarget?.DisplayName ?? "OBJECT");
            }

            return interactionController.LastAllyTaskRejection switch
            {
                PrototypeAllyRelayRejection.GlobalStateBlocked =>
                    "FETCH BLOCKED: RESET OR FAILURE PENDING",
                PrototypeAllyRelayRejection.RelayAlreadyActive =>
                    "FETCH BLOCKED: ALLY TASK ALREADY ACTIVE",
                PrototypeAllyRelayRejection.SupplyAlreadyConsumed =>
                    "FETCH BLOCKED: OBJECT ALREADY USED",
                PrototypeAllyRelayRejection.InvalidPhysicalTarget =>
                    "FETCH BLOCKED: INVALID PHYSICAL OBJECT",
                PrototypeAllyRelayRejection.InvalidPhysicalLane =>
                    "NO REACHABLE FETCH TARGET INSIDE THE AUTHORED ALLY ROUTE",
                PrototypeAllyRelayRejection.AllyUnavailable =>
                    "FETCH BLOCKED: ALLY UNAVAILABLE",
                PrototypeAllyRelayRejection.NoValidSupply =>
                    "NO AVAILABLE ALLY BINDER OR CHAIR | RESET TO RESTORE USED OBJECTS",
                _ => "FETCH BLOCKED: NO VALID SUPPLY"
            };
        }

        private string BuildAllyMovementPrompt()
        {
            if (interactionController?.ShowcaseTeamContextController != null)
            {
                return "[G] REGROUP";
            }

            ShowcaseMobileAllyController mobile =
                interactionController?.ShowcaseMobileAllyController;
            return mobile != null && mobile.IsFollowCommanded
                ? "[Q] HOLD | [G] REGROUP"
                : "[Q] FOLLOW | [G] REGROUP";
        }

        private static string BuildChargeBar(float charge01)
        {
            const int segmentCount = 10;
            int filled = Mathf.Clamp(
                Mathf.RoundToInt(Mathf.Clamp01(charge01) * segmentCount),
                0,
                segmentCount);
            return $"[{new string('#', filled)}" +
                   $"{new string('-', segmentCount - filled)}]";
        }

        private static string BuildAttackTypeLabel(
            ShowcaseCargoAttackType attackType)
        {
            return attackType switch
            {
                ShowcaseCargoAttackType.SweepLeft => "SWEEP LEFT",
                ShowcaseCargoAttackType.SweepRight => "SWEEP RIGHT",
                ShowcaseCargoAttackType.OverheadSlam => "OVERHEAD SLAM",
                ShowcaseCargoAttackType.Rejected => "ATTACK PATH BLOCKED",
                _ => "NO VALID ATTACK"
            };
        }

        private void LateUpdate()
        {
            if (interactionController == null)
            {
                HideThrowTrajectory();
                return;
            }

            UpdateThrowTrajectory();

            if (interactionController.AllyTaskAttemptSequence != observedAllyTaskSequence)
            {
                observedAllyTaskSequence = interactionController.AllyTaskAttemptSequence;
                activeFeedback = BuildLatestAllyTaskFeedbackText();
                feedbackExpiresAt = Time.unscaledTime + FeedbackDuration;
            }
            else if (interactionController.AllyCallAttemptSequence != observedAllyCallSequence)
            {
                observedAllyCallSequence = interactionController.AllyCallAttemptSequence;
                activeFeedback = BuildLatestAllyCallFeedbackText();
                feedbackExpiresAt = Time.unscaledTime + FeedbackDuration;
            }
            else if (interactionController.InteractionAttemptSequence != observedAttemptSequence)
            {
                observedAttemptSequence = interactionController.InteractionAttemptSequence;
                activeFeedback = BuildLatestAttemptFeedbackText();
                feedbackExpiresAt = Time.unscaledTime + FeedbackDuration;
            }
        }

        private void OnDisable()
        {
            HideThrowTrajectory();
        }

        private void OnDestroy()
        {
            if (throwTrajectoryMaterial != null)
            {
                Destroy(throwTrajectoryMaterial);
                throwTrajectoryMaterial = null;
            }
        }

        private void UpdateThrowTrajectory()
        {
            if (!Application.isPlaying)
            {
                HideThrowTrajectory();
                return;
            }

            ShowcaseCargoStrapController cargo =
                interactionController.ShowcaseCargoStrapController;
            int pointCount = 0;
            bool cargoPreview = cargo != null &&
                cargo.TryBuildAttackTrajectoryPreview(
                    throwTrajectoryPoints,
                    out pointCount);
            if (!cargoPreview &&
                !interactionController.TryBuildThrowTrajectoryPreview(
                    throwTrajectoryPoints,
                    out pointCount))
            {
                HideThrowTrajectory();
                return;
            }

            EnsureThrowTrajectoryLine();
            Color trajectoryColor;
            if (cargoPreview)
            {
                ShowcaseCargoChargeProfile charge =
                    cargo.ActiveAttackChargeProfile;
                trajectoryColor = charge.Tier switch
                {
                    ShowcaseCargoChargeTier.Quick =>
                        new Color(0.28f, 0.82f, 1f, 0.92f),
                    ShowcaseCargoChargeTier.Power =>
                        new Color(1f, 0.76f, 0.2f, 0.96f),
                    _ => new Color(1f, 0.28f, 0.08f, 1f)
                };
            }
            else
            {
                PrototypeThrowPresentationSnapshot presentation =
                    interactionController.ThrowPresentation;
                trajectoryColor = Color.Lerp(
                    new Color(0.1f, 0.8f, 1f, 0.92f),
                    new Color(1f, 0.62f, 0.08f, 1f),
                    presentation.Charge01);
            }
            throwTrajectoryLine.startColor = trajectoryColor;
            throwTrajectoryLine.endColor = new Color(
                trajectoryColor.r,
                trajectoryColor.g,
                trajectoryColor.b,
                0.24f);
            throwTrajectoryLine.positionCount = pointCount;
            for (int index = 0; index < pointCount; index++)
            {
                throwTrajectoryLine.SetPosition(index, throwTrajectoryPoints[index]);
            }
            throwTrajectoryLine.enabled = true;
        }

        private void EnsureThrowTrajectoryLine()
        {
            if (throwTrajectoryLine != null)
            {
                return;
            }

            GameObject lineObject = new("CHR00_THROW_TRAJECTORY");
            lineObject.transform.SetParent(transform, false);
            throwTrajectoryLine = lineObject.AddComponent<LineRenderer>();
            throwTrajectoryLine.useWorldSpace = true;
            throwTrajectoryLine.loop = false;
            throwTrajectoryLine.startWidth = 0.045f;
            throwTrajectoryLine.endWidth = 0.018f;
            throwTrajectoryLine.numCapVertices = 2;
            throwTrajectoryLine.numCornerVertices = 2;
            throwTrajectoryLine.receiveShadows = false;
            throwTrajectoryLine.enabled = false;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }
            if (shader != null)
            {
                throwTrajectoryMaterial = new Material(shader)
                {
                    name = "CHR00_THROW_TRAJECTORY_RUNTIME_MATERIAL"
                };
                throwTrajectoryLine.sharedMaterial = throwTrajectoryMaterial;
            }
        }

        private void HideThrowTrajectory()
        {
            if (throwTrajectoryLine == null)
            {
                return;
            }
            throwTrajectoryLine.enabled = false;
            throwTrajectoryLine.positionCount = 0;
        }

        private void OnGUI()
        {
            string status = BuildStatusText();
            if (status.Length == 0)
            {
                return;
            }

            EnsureStyles();
            DrawAllyReadyIndicator();
            bool healthVfxActive = ShowcaseHealthVfxPresenter.Active != null;
            GUI.Label(
                healthVfxActive
                    ? new Rect(
                        Screen.width * 0.35f,
                        Screen.height - 78f,
                        Screen.width * 0.63f,
                        42f)
                    : new Rect(0f, Screen.height - 78f, Screen.width, 42f),
                status,
                promptStyle);

            string reticle = BuildReticleText();
            crosshairStyle.normal.textColor = GetReticleColor(reticle);
            GUI.Label(
                new Rect((Screen.width * 0.5f) - 28f, (Screen.height * 0.5f) - 18f, 56f, 36f),
                reticle,
                crosshairStyle);

            string targetLabel = BuildReticleLabel();
            if (targetLabel.Length > 0)
            {
                GUI.Label(
                    new Rect((Screen.width * 0.5f) - 190f, (Screen.height * 0.5f) + 20f, 380f, 30f),
                    targetLabel,
                    targetStyle);
            }

            if (activeFeedback.Length > 0 && Time.unscaledTime <= feedbackExpiresAt)
            {
                GUI.Label(
                    new Rect((Screen.width * 0.5f) - 280f, Screen.height - 122f, 560f, 34f),
                    activeFeedback,
                    feedbackStyle);
            }
        }

        private string GetCurrentDistanceText()
        {
            float distance = interactionController.CurrentTargetDistance;
            return float.IsInfinity(distance)
                ? "OUT OF RANGE"
                : $"{distance.ToString("0.0", CultureInfo.InvariantCulture)} M";
        }

        private int GetThrowChargePercent()
        {
            return Mathf.RoundToInt(interactionController.ThrowCharge01 * 100f);
        }

        private static string GetWeightLabel(PrototypeInteractionTarget target)
        {
            return target.WeightClass.ToString().ToUpperInvariant();
        }

        private static Color GetReticleColor(string reticle)
        {
            return reticle switch
            {
                "[E]" => new Color(0.35f, 1f, 0.45f, 0.95f),
                "[Q]" => new Color(0.3f, 0.75f, 1f, 0.95f),
                "[!]" => new Color(1f, 0.65f, 0.1f, 0.95f),
                _ => new Color(1f, 1f, 1f, 0.8f)
            };
        }

        private void EnsureStyles()
        {
            if (promptStyle != null)
            {
                return;
            }

            promptStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            crosshairStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };

            targetStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            feedbackStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.35f, 1f, 0.45f, 1f) }
            };

            allyIndicatorStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.82f, 0.12f, 1f) }
            };
        }

        private void DrawAllyReadyIndicator()
        {
            PrototypeAllyRelayController relay = interactionController?.AllyRelayController;
            Camera viewCamera = interactionController?.ViewCamera;
            string indicator = BuildAllyIndicatorText();
            if (relay == null || indicator.Length == 0 || viewCamera == null)
            {
                return;
            }

            Vector3 screenPoint = viewCamera.WorldToScreenPoint(relay.IndicatorWorldPosition);
            if (screenPoint.z <= 0f)
            {
                return;
            }

            float guiY = Screen.height - screenPoint.y;
            GUI.Label(
                new Rect(screenPoint.x - 24f, guiY - 24f, 48f, 48f),
                indicator,
                allyIndicatorStyle);
        }
    }
}
