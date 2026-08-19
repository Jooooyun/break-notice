using System.Text;
using BreakNotice.Gameplay.Combat;
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
        private GUIStyle markerStyle;
        private GUIStyle worldMarkerStyle;
        private float contactMarkerUntil;
        private bool contactAccepted;
        private Vector3 contactMarkerWorldPoint;
        private ShowcaseBoneCombatRig playerCombatRig;

        public ShowcaseRunnerPressureController PressureController => pressureController;
        public bool IsContactMarkerVisible => Time.unscaledTime < contactMarkerUntil;
        public Vector3 LastContactMarkerWorldPoint => contactMarkerWorldPoint;

        public void Configure(ShowcaseRunnerPressureController pressure)
        {
            Unsubscribe();
            pressureController = pressure;
            playerCombatRig = pressureController != null &&
                pressureController.PlayerTransform != null
                    ? pressureController.PlayerTransform.GetComponent<ShowcaseBoneCombatRig>()
                    : null;
            Subscribe();
        }

        private void OnEnable() => Subscribe();
        private void OnDisable() => Unsubscribe();

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
            ShowcaseCombatHealth playerHealth = pressureController.PlayerHealth;
            string health = playerHealth != null && ShowcaseHealthVfxPresenter.Active == null
                ? $"PLAYER HP {playerHealth.CurrentHealth}/{playerHealth.MaximumHealth}  |  "
                : string.Empty;
            string melee = pressureController.HasPlayerMeleeTarget
                ? "  |  [LMB] MELEE"
                : string.Empty;
            string token = pressureController.AttackTokenOwner != null
                ? $"  |  ATTACK TOKEN: {pressureController.AttackTokenOwner.RunnerId}"
                : string.Empty;
            return health + builder + token + melee;
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

            DrawContactMarker();
            DrawPlayerMeleeMarker();
            DrawWorldContactMarker();
            DrawRunnerMarkers();
        }

        public bool TryGetPlayerStrikeMarker(
            out Vector3 worldCenter,
            out float radius)
        {
            if (playerCombatRig == null && pressureController != null &&
                pressureController.PlayerTransform != null)
            {
                playerCombatRig = pressureController.PlayerTransform
                    .GetComponent<ShowcaseBoneCombatRig>();
            }
            if (playerCombatRig != null &&
                playerCombatRig.TryGetPrimaryActiveStrikeVolume(
                    out worldCenter,
                    out radius))
            {
                return true;
            }

            worldCenter = default;
            radius = 0f;
            return false;
        }

        private void DrawContactMarker()
        {
            if (ShowcaseCombatHitVfxPresenter.Active != null ||
                Time.unscaledTime >= contactMarkerUntil)
            {
                return;
            }
            markerStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 30,
                fontStyle = FontStyle.Bold
            };
            markerStyle.normal.textColor = contactAccepted
                ? Color.white
                : new Color(0.55f, 0.55f, 0.55f, 1f);
            GUI.Label(
                new Rect(Screen.width * 0.5f - 25f, Screen.height * 0.5f - 25f, 50f, 50f),
                contactAccepted ? "×" : "·",
                markerStyle);
        }

        private void DrawRunnerMarkers()
        {
            Camera camera = Camera.main;
            if (camera == null || pressureController == null)
            {
                return;
            }
            worldMarkerStyle ??= new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
            foreach (ShowcaseRunnerAgent runner in pressureController.Runners)
            {
                if (runner == null || runner.IsDead)
                {
                    continue;
                }
                Vector3 screen = camera.WorldToScreenPoint(
                    runner.transform.position + Vector3.up * 2.25f);
                if (screen.z <= 0f)
                {
                    continue;
                }
                bool telegraph = runner.State == ShowcaseRunnerState.Telegraph;
                bool active = runner.IsAttackWindowOpen;
                ShowcaseCombatHealth health = runner.Health;
                bool showHealth = ShowcaseHealthVfxPresenter.Active == null &&
                    health != null &&
                    (health.CurrentHealth < health.MaximumHealth ||
                     Vector3.Distance(
                         pressureController.PlayerTransform.position,
                         runner.transform.position) <= 3f);
                if (!telegraph && !active && !showHealth)
                {
                    continue;
                }
                worldMarkerStyle.normal.textColor = active
                    ? Color.red
                    : telegraph
                        ? Color.yellow
                        : Color.white;
                string state = active ? "ACTIVE" : telegraph ? "!" : string.Empty;
                string hp = showHealth
                    ? $"  HP {health.CurrentHealth}/{health.MaximumHealth}"
                    : string.Empty;
                GUI.Label(
                    new Rect(screen.x - 70f, Screen.height - screen.y - 18f, 140f, 32f),
                    state + hp,
                    worldMarkerStyle);
            }
        }

        private void DrawPlayerMeleeMarker()
        {
            Camera camera = Camera.main;
            if (camera == null ||
                !TryGetPlayerStrikeMarker(out Vector3 center, out _))
            {
                return;
            }

            Vector3 screen = camera.WorldToScreenPoint(center);
            if (screen.z <= 0f)
            {
                return;
            }

            EnsureWorldMarkerStyle();
            worldMarkerStyle.normal.textColor = new Color(1f, 0.58f, 0.08f, 1f);
            GUI.Label(
                new Rect(screen.x - 45f, Screen.height - screen.y - 18f, 90f, 36f),
                "◎ STRIKE",
                worldMarkerStyle);
        }

        private void DrawWorldContactMarker()
        {
            if (ShowcaseCombatHitVfxPresenter.Active != null ||
                !contactAccepted || !IsContactMarkerVisible)
            {
                return;
            }

            Camera camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            Vector3 screen = camera.WorldToScreenPoint(contactMarkerWorldPoint);
            if (screen.z <= 0f)
            {
                return;
            }

            EnsureWorldMarkerStyle();
            worldMarkerStyle.normal.textColor = Color.white;
            GUI.Label(
                new Rect(screen.x - 42f, Screen.height - screen.y - 22f, 84f, 44f),
                "× HIT",
                worldMarkerStyle);
        }

        private void HandleCombatContact(ShowcaseCombatContactResult result)
        {
            contactAccepted = result.Accepted;
            contactMarkerWorldPoint = result.WorldPoint;
            contactMarkerUntil = Time.unscaledTime + 0.20f;
            if (!result.Accepted)
            {
                return;
            }
            result.Attacker?.GetComponent<ShowcaseCharacterAnimationDriver>()
                ?.ApplyLocalHitStop(0.05f);
            result.Victim?.GetComponent<ShowcaseCharacterAnimationDriver>()
                ?.ApplyLocalHitStop(0.05f);
        }

        private void HandleBaselineReset()
        {
            contactMarkerUntil = 0f;
            contactAccepted = false;
            contactMarkerWorldPoint = Vector3.zero;
        }

        private void EnsureWorldMarkerStyle()
        {
            worldMarkerStyle ??= new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
        }

        private void Subscribe()
        {
            if (pressureController == null)
            {
                return;
            }
            pressureController.CombatContactResolved -= HandleCombatContact;
            pressureController.BaselineReset -= HandleBaselineReset;
            pressureController.CombatContactResolved += HandleCombatContact;
            pressureController.BaselineReset += HandleBaselineReset;
        }

        private void Unsubscribe()
        {
            if (pressureController == null)
            {
                return;
            }
            pressureController.CombatContactResolved -= HandleCombatContact;
            pressureController.BaselineReset -= HandleBaselineReset;
        }
    }
}
