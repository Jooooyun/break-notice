using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Presentation
{
    [DefaultExecutionOrder(210)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseMobileAllyPresenter : MonoBehaviour
    {
        [SerializeField]
        private ShowcaseMobileAllyController mobileAlly;

        [SerializeField]
        private ShowcaseTeamContextController teamContext;

        private GUIStyle statusStyle;

        public ShowcaseMobileAllyController MobileAlly => mobileAlly;

        public ShowcaseTeamContextController TeamContext => teamContext;

        public void Configure(
            ShowcaseMobileAllyController controller,
            ShowcaseTeamContextController teamController = null)
        {
            mobileAlly = controller;
            teamContext = teamController;
        }

        public string BuildStatusText()
        {
            if (mobileAlly == null)
            {
                return string.Empty;
            }

            if (mobileAlly.State == ShowcaseMobileAllyState.Blocked &&
                mobileAlly.HasTaskMovement)
            {
                return "ALLY TASK BLOCKED | CLEAR PATH OR RESET";
            }

            return mobileAlly.State switch
            {
                ShowcaseMobileAllyState.HoldingPosition =>
                    teamContext != null
                        ? "ALLY HOLDING POSITION | [G] REGROUP"
                        : "ALLY HOLDING POSITION | [Q] FOLLOW | [G] REGROUP",
                ShowcaseMobileAllyState.Blocked => "ALLY BLOCKED | [G] REGROUP",
                ShowcaseMobileAllyState.Regrouping => teamContext != null
                    ? "ALLY REGROUPING"
                    : "ALLY REGROUPING | [Q] HOLD",
                ShowcaseMobileAllyState.Fetching => "ALLY FETCHING OBJECT",
                ShowcaseMobileAllyState.ReturningWithSupply => "ALLY RETURNING WITH SUPPLY",
                ShowcaseMobileAllyState.FastCatchingUp =>
                    teamContext != null
                        ? "ALLY FAST CATCH-UP | [G] REGROUP"
                        : "ALLY FAST CATCH-UP | [Q] HOLD | [G] REGROUP",
                ShowcaseMobileAllyState.CatchingUp =>
                    teamContext != null
                        ? "ALLY CATCH-UP | [G] REGROUP"
                        : "ALLY CATCH-UP | [Q] HOLD | [G] REGROUP",
                _ => teamContext != null
                    ? "ALLY FOLLOWING | [G] REGROUP"
                    : "ALLY FOLLOWING | [Q] HOLD | [G] REGROUP"
            };
        }

        private void OnGUI()
        {
            string text = BuildStatusText();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            statusStyle ??= new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = Color.white,
                    background = Texture2D.grayTexture
                }
            };

            float width = Mathf.Min(360f, Screen.width - 24f);
            Rect rect = new Rect(Screen.width - width - 12f, 12f, width, 34f);
            Color previous = GUI.color;
            GUI.color = mobileAlly.State == ShowcaseMobileAllyState.Blocked
                ? new Color(0.95f, 0.45f, 0.25f, 0.95f)
                : new Color(0.3f, 0.75f, 1f, 0.9f);
            GUI.Box(rect, text, statusStyle);
            GUI.color = previous;
        }
    }
}
