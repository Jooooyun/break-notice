using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Presentation
{
    [DefaultExecutionOrder(220)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseTeamContextPresenter : MonoBehaviour
    {
        [SerializeField]
        private ShowcaseTeamContextController teamContext;

        private GUIStyle commandStyle;
        private GUIStyle readinessStyle;
        private GUIStyle feedbackStyle;
        private uint observedAttemptSequence;
        private string activeFeedback = string.Empty;
        private float feedbackExpiresAt;

        public ShowcaseTeamContextController TeamContext => teamContext;

        public void Configure(ShowcaseTeamContextController controller)
        {
            teamContext = controller;
        }

        public string BuildQPromptText()
        {
            if (teamContext == null)
            {
                return string.Empty;
            }

            ShowcaseQContext context = teamContext.ResolveCurrentQContext();
            if (!teamContext.IsContextAvailable(context) &&
                context == ShowcaseQContext.SendSupply)
            {
                return "ALLY SUPPLY BLOCKED";
            }

            return context switch
            {
                ShowcaseQContext.BreakGrab => "[Q] BREAK GRAB",
                ShowcaseQContext.TeamFinish => "[Q] TEAM FINISH",
                ShowcaseQContext.AssistAttach => "[Q] ASSIST ATTACH",
                ShowcaseQContext.CombatTag => "[Q] TAG SHOVE / SET HIM UP",
                ShowcaseQContext.SendSupply => BuildSupplyPrompt(),
                ShowcaseQContext.ToggleFollow =>
                    teamContext.MobileAllyController != null &&
                    teamContext.MobileAllyController.IsFollowCommanded
                        ? "[Q] HOLD POSITION"
                        : "[Q] FOLLOW",
                ShowcaseQContext.Blocked => "TEAM COMMANDS BLOCKED",
                _ => string.Empty
            };
        }

        public string BuildReadinessText()
        {
            if (teamContext == null)
            {
                return string.Empty;
            }

            string readiness =
                $"PASS [{GetBadgeMark(teamContext.HasPass)}]  " +
                $"BRACE [{GetBadgeMark(teamContext.HasBrace)}]  " +
                $"BREAK [{GetBadgeMark(teamContext.HasBreak)}]";
            return teamContext.IsTeamReady
                ? readiness + "  |  TEAM READY"
                : readiness;
        }

        public string BuildLatestFeedbackText()
        {
            if (teamContext == null || teamContext.QAttemptSequence == 0)
            {
                return string.Empty;
            }

            if (!teamContext.LastQAccepted)
            {
                return teamContext.LastQContext switch
                {
                    ShowcaseQContext.Blocked =>
                        "TEAM COMMAND BLOCKED: RESET OR FAILURE PENDING",
                    ShowcaseQContext.SendSupply =>
                        "TEAM COMMAND BLOCKED: SUPPLY NOT READY",
                    _ => "TEAM COMMAND REJECTED: CONTEXT CHANGED"
                };
            }

            return teamContext.LastQContext switch
            {
                ShowcaseQContext.SendSupply => "TEAM COMMAND ACCEPTED: SEND SUPPLY",
                ShowcaseQContext.ToggleFollow => "TEAM COMMAND ACCEPTED: FOLLOW STATE CHANGED",
                ShowcaseQContext.BreakGrab => "TEAM COMMAND ACCEPTED: BREAK GRAB",
                ShowcaseQContext.TeamFinish => "TEAM COMMAND ACCEPTED: TEAM FINISH",
                ShowcaseQContext.AssistAttach => "TEAM COMMAND ACCEPTED: ASSIST ATTACH",
                ShowcaseQContext.CombatTag => "TEAM COMMAND ACCEPTED: COMBAT TAG",
                _ => "TEAM COMMAND ACCEPTED"
            };
        }

        private void LateUpdate()
        {
            if (teamContext == null ||
                teamContext.QAttemptSequence == observedAttemptSequence)
            {
                return;
            }

            observedAttemptSequence = teamContext.QAttemptSequence;
            activeFeedback = BuildLatestFeedbackText();
            feedbackExpiresAt = Time.unscaledTime + 0.9f;
        }

        private void OnGUI()
        {
            if (teamContext == null)
            {
                return;
            }

            EnsureStyles();
            string command = BuildQPromptText();
            if (command.Length > 0)
            {
                GUI.Label(
                    new Rect((Screen.width * 0.5f) - 250f, 88f, 500f, 34f),
                    command,
                    commandStyle);
            }

            string readiness = BuildReadinessText();
            if (readiness.Length > 0)
            {
                GUI.Label(
                    new Rect(12f, 52f, Mathf.Min(520f, Screen.width - 24f), 32f),
                    readiness,
                    readinessStyle);
            }

            if (activeFeedback.Length > 0 && Time.unscaledTime <= feedbackExpiresAt)
            {
                GUI.Label(
                    new Rect((Screen.width * 0.5f) - 260f, 126f, 520f, 30f),
                    activeFeedback,
                    feedbackStyle);
            }
        }

        private string BuildSupplyPrompt()
        {
            PrototypeInteractionTarget target =
                teamContext.AllyRelayController?.PreparedTarget;
            return target?.Role == PrototypeInteractionRole.AllyChair
                ? "[Q] SEND CHAIR"
                : "[Q] SEND BINDER";
        }

        private static string GetBadgeMark(bool earned)
        {
            return earned ? "X" : " ";
        }

        private void EnsureStyles()
        {
            if (commandStyle != null)
            {
                return;
            }

            commandStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.45f, 0.85f, 1f, 1f) }
            };
            readinessStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.84f, 0.25f, 1f) }
            };
            feedbackStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.45f, 1f, 0.55f, 1f) }
            };
        }
    }
}
