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

        private GUIStyle statusStyle;

        public ShowcaseMobileAllyController MobileAlly => mobileAlly;

        public void Configure(ShowcaseMobileAllyController controller)
        {
            mobileAlly = controller;
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
                ShowcaseMobileAllyState.Blocked => "ALLY BLOCKED | [Q] REGROUP",
                ShowcaseMobileAllyState.Regrouping => "ALLY REGROUPING",
                ShowcaseMobileAllyState.Fetching => "ALLY FETCHING OBJECT",
                ShowcaseMobileAllyState.ReturningWithSupply => "ALLY RETURNING WITH SUPPLY",
                ShowcaseMobileAllyState.FastCatchingUp => "ALLY FAST CATCH-UP | [Q] REGROUP",
                ShowcaseMobileAllyState.CatchingUp => "ALLY CATCH-UP | [Q] REGROUP",
                _ => "ALLY FOLLOWING | [Q] REGROUP"
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
