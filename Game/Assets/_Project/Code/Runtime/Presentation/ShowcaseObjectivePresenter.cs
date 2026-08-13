using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Presentation
{
    [DefaultExecutionOrder(220)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseObjectivePresenter : MonoBehaviour
    {
        [SerializeField]
        private ShowcaseObjectiveController objectiveController;

        private GUIStyle statusStyle;
        private GUIStyle cardStyle;
        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;

        public ShowcaseObjectiveController ObjectiveController => objectiveController;

        public void Configure(ShowcaseObjectiveController objective)
        {
            objectiveController = objective;
        }

        private void OnGUI()
        {
            if (objectiveController == null)
            {
                return;
            }

            EnsureStyles();
            if (objectiveController.RunComplete)
            {
                DrawResultCard();
                return;
            }

            string status = BuildObjectiveText(objectiveController);
            GUI.Label(
                new Rect(12f, 154f, Mathf.Min(Screen.width - 24f, 760f), 38f),
                status,
                statusStyle);
        }

        public static string BuildObjectiveText(ShowcaseObjectiveController objective)
        {
            if (objective == null)
            {
                return string.Empty;
            }

            if (objective.FailureTriggered)
            {
                return "RUN FAILED — [BACKSPACE] RESET";
            }

            if (objective.DrawerSolved)
            {
                return "DRAWER PARKED — EXIT OPEN — RUN TO THE GREEN EXIT";
            }

            return "OBJECTIVE: DRIVE THE DRAWER INTO THE MARKED PARKING BAY";
        }

        private void DrawResultCard()
        {
            const float width = 520f;
            const float height = 430f;
            float left = (Screen.width - width) * 0.5f;
            float top = (Screen.height - height) * 0.5f;
            GUI.Box(new Rect(left, top, width, height), GUIContent.none, cardStyle);
            GUI.Label(new Rect(left + 24f, top + 18f, width - 48f, 44f),
                "OPERATIONS FLOOR CLEAR", titleStyle);

            float y = top + 78f;
            DrawField(left, ref y, "CLEAR TIME",
                $"{objectiveController.ClearTimeSeconds:0.0} SEC");
            DrawField(left, ref y, "DRAWER SOLUTION",
                objectiveController.DrawerSolutionLabel);
            DrawField(left, ref y, "TEAM TAGS",
                $"{objectiveController.TeamTagCount} / 3");
            DrawField(left, ref y, "PHYSICAL SOLUTIONS DISCOVERED",
                $"{objectiveController.PhysicalSolutionsDiscovered} / 4");
            DrawField(left, ref y, "LONGEST CAUSAL CHAIN",
                objectiveController.LongestCausalChain.ToString());
            DrawField(left, ref y, "ALTERNATE ROUTES USED",
                objectiveController.AlternateRoutesUsed.ToString());

            if (GUI.Button(new Rect(left + 24f, top + height - 66f, 282f, 40f),
                    "REPLAY — NEW PRESSURE VARIANT", buttonStyle))
            {
                objectiveController.RequestReplay();
            }
            GUI.enabled = false;
            GUI.Button(new Rect(left + 318f, top + height - 66f, 178f, 40f),
                "RETURN TO TITLE", buttonStyle);
            GUI.enabled = true;
        }

        private void DrawField(float left, ref float y, string name, string value)
        {
            GUI.Label(new Rect(left + 30f, y, 300f, 30f), name, labelStyle);
            GUI.Label(new Rect(left + 300f, y, 190f, 30f), value, labelStyle);
            y += 42f;
        }

        private void EnsureStyles()
        {
            statusStyle ??= new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.85f, 1f, 0.32f, 1f) }
            };
            cardStyle ??= new GUIStyle(GUI.skin.box)
            {
                normal = { background = Texture2D.grayTexture }
            };
            titleStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 25,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            buttonStyle ??= new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
        }
    }
}
