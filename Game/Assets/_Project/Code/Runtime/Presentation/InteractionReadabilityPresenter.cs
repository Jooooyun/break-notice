using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Presentation
{
    [DisallowMultipleComponent]
    public sealed class InteractionReadabilityPresenter : MonoBehaviour
    {
        [SerializeField]
        private PrototypeInteractionController interactionController;

        private GUIStyle promptStyle;
        private GUIStyle crosshairStyle;

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
                return "RESTORING AUTHORED BASELINE";
            }

            if (owner.GlobalState == PrototypeGlobalInteractionState.FailurePending)
            {
                return "INTERACTIONS BLOCKED — FAILURE PENDING";
            }

            if (owner.HandState == PrototypeHandOwnershipState.Carrying && owner.CarriedTarget != null)
            {
                return $"CARRYING {owner.CarriedTarget.DisplayName} [{owner.CarriedTarget.WeightClass.ToString().ToUpperInvariant()}]";
            }

            PrototypeInteractionTarget target = interactionController.CurrentTarget;
            if (target == null)
            {
                return "MOVE CLOSER AND FACE AN OFFICE OBJECT";
            }

            if (target.WeightClass == PrototypeWeightClass.Heavy)
            {
                return $"{target.DisplayName} [HEAVY] — RECOGNITION ONLY";
            }

            if (target.Role == PrototypeInteractionRole.Drawer)
            {
                return $"{target.DisplayName} [MEDIUM] — IMPACT TARGET / DIRECT PICKUP BLOCKED";
            }

            if (!target.DirectAcquisitionEligible)
            {
                return $"{target.DisplayName} [{target.WeightClass.ToString().ToUpperInvariant()}] — ALLY SUPPLY POOL";
            }

            return $"[E] PICK UP {target.DisplayName} [{target.WeightClass.ToString().ToUpperInvariant()}]";
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
            GUI.Label(
                new Rect((Screen.width * 0.5f) - 12f, (Screen.height * 0.5f) - 16f, 24f, 32f),
                "+",
                crosshairStyle);
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
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.82f, 0.15f, 0.9f) }
            };
        }
    }
}
