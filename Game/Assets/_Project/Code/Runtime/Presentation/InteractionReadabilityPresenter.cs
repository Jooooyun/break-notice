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

        [SerializeField]
        private PrototypeInteractionController interactionController;

        private GUIStyle promptStyle;
        private GUIStyle crosshairStyle;
        private GUIStyle targetStyle;
        private GUIStyle feedbackStyle;
        private uint observedAttemptSequence;
        private uint observedAllyCallSequence;
        private string activeFeedback = string.Empty;
        private float feedbackExpiresAt;

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

            if (owner.HandState == PrototypeHandOwnershipState.Carrying && owner.CarriedTarget != null)
            {
                return $"CARRYING {owner.CarriedTarget.DisplayName} " +
                       $"[{GetWeightLabel(owner.CarriedTarget)}] | LMB THROW";
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
                    return "QUEUED THROW ARMED | BINDER INCOMING | [E] CATCH";
                }

                return relay.CanCatch(interactionController.CurrentTarget)
                    ? "BINDER INCOMING | [E] CATCH"
                    : "BINDER INCOMING | CENTER RETICLE AND MOVE INTO CATCH RANGE";
            }

            if (relay != null && relay.IsChairFeedActive)
            {
                return "CHAIR GROUNDED FEED | CATCH BLOCKED | QUEUED THROW BLOCKED";
            }

            if (interactionController.CurrentRelayCandidate != null)
            {
                return $"ALLY READY | CALL ALLY AVAILABLE: " +
                       $"{interactionController.CurrentRelayCandidate.DisplayName} " +
                       "| [Q] SUPPLY";
            }

            PrototypeInteractionTarget target = interactionController.CurrentTarget;
            if (target == null)
            {
                return $"CENTER RETICLE ON AN OBJECT | RANGE <= " +
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
                return $"TARGET: {identity} | {distance} | ALLY SUPPLY / DIRECT PICKUP BLOCKED";
            }

            return $"TARGET LOCKED: {identity} | {distance} | [E] PICK UP";
        }

        public string BuildReticleText()
        {
            PrototypeAllyRelayController relay = interactionController?.AllyRelayController;
            if (relay != null &&
                relay.IsBinderIncoming &&
                relay.CanCatch(interactionController.CurrentTarget))
            {
                return "[E]";
            }

            if (interactionController?.CurrentRelayCandidate != null)
            {
                return "[Q]";
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

            return target.IsScoopEligible
                ? "[E]"
                : "[!]";
        }

        public string BuildReticleLabel()
        {
            PrototypeAllyRelayController relay = interactionController?.AllyRelayController;
            if (relay != null &&
                relay.IsBinderIncoming &&
                relay.CanCatch(interactionController.CurrentTarget))
            {
                return interactionController.InteractionOwner.QueuedThrowArmed
                    ? "CATCH + QUEUED THROW"
                    : "CATCH INCOMING BINDER";
            }

            if (interactionController?.CurrentRelayCandidate != null)
            {
                return $"CALL ALLY FOR " +
                       interactionController.CurrentRelayCandidate.DisplayName;
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

            if (target.IsScoopEligible)
            {
                return $"PICK UP {target.DisplayName}";
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
                _ => "CALL BLOCKED: NO VALID SUPPLY"
            };
        }

        private void LateUpdate()
        {
            if (interactionController == null)
            {
                return;
            }

            if (interactionController.AllyCallAttemptSequence != observedAllyCallSequence)
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

        private void OnGUI()
        {
            string status = BuildStatusText();
            if (status.Length == 0)
            {
                return;
            }

            EnsureStyles();
            GUI.Label(
                new Rect(0f, Screen.height - 78f, Screen.width, 42f),
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
        }
    }
}
