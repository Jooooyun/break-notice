using System.Text;
using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Presentation
{
    [DefaultExecutionOrder(205)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseRunnerPressurePresenter : MonoBehaviour
    {
        [SerializeField]
        private ShowcaseRunnerPressureController pressureController;

        private GUIStyle statusStyle;

        public ShowcaseRunnerPressureController PressureController => pressureController;

        public void Configure(ShowcaseRunnerPressureController pressure)
        {
            pressureController = pressure;
        }

        public string BuildStatusText()
        {
            if (pressureController == null)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            ShowcaseRunnerAgent[] authoredRunners = pressureController.Runners;
            for (int index = 0; index < authoredRunners.Length; index++)
            {
                ShowcaseRunnerAgent runner = authoredRunners[index];
                if (runner == null)
                {
                    continue;
                }
                if (builder.Length > 0)
                {
                    builder.Append("  |  ");
                }
                builder.Append(runner.RunnerId);
                builder.Append(": ");
                builder.Append(runner.State.ToString().ToUpperInvariant());
            }
            string melee = pressureController.HasPlayerMeleeTarget
                ? "  |  [LMB] SHOVE"
                : string.Empty;
            string token = pressureController.AttackTokenOwner != null
                ? $"  |  ATTACK TOKEN: {pressureController.AttackTokenOwner.RunnerId}"
                : string.Empty;
            return builder.ToString() + token + melee;
        }

        private void OnGUI()
        {
            string status = BuildStatusText();
            if (status.Length == 0)
            {
                return;
            }

            statusStyle ??= new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            statusStyle.normal.textColor = new Color(1f, 0.55f, 0.22f, 1f);
            GUI.Label(new Rect(12f, 106f, Screen.width - 24f, 34f), status, statusStyle);
        }
    }
}
