using System.Text;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Presentation
{
    public readonly struct ShowcaseOffScreenAttackWarning
    {
        public ShowcaseOffScreenAttackWarning(
            ShowcaseRunnerAgent runner,
            Vector2 viewportEdge,
            Vector2 direction)
        {
            Runner = runner;
            ViewportEdge = viewportEdge;
            Direction = direction;
        }

        public ShowcaseRunnerAgent Runner { get; }
        public Vector2 ViewportEdge { get; }
        public Vector2 Direction { get; }
    }

    [DefaultExecutionOrder(205)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseRunnerPressurePresenter : MonoBehaviour
    {
        [SerializeField]
        private ShowcaseRunnerPressureController pressureController;

        [SerializeField, Range(1, 3)]
        private int maximumOffScreenWarnings = 3;

        private GUIStyle statusStyle;
        private GUIStyle markerStyle;
        private GUIStyle worldMarkerStyle;
        private GUIStyle offScreenWarningStyle;
        private float contactMarkerUntil;
        private bool contactAccepted;
        private Vector3 contactMarkerWorldPoint;
        private ShowcaseBoneCombatRig playerCombatRig;

        public ShowcaseRunnerPressureController PressureController => pressureController;
        public bool IsContactMarkerVisible => Time.unscaledTime < contactMarkerUntil;
        public Vector3 LastContactMarkerWorldPoint => contactMarkerWorldPoint;
        public int LastOffScreenWarningCount { get; private set; }

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
            ShowcaseAttackBudgetSnapshot budget =
                pressureController.CurrentAttackBudget;
            string attackBudget =
                $"  |  ATTACK BUDGET N " +
                $"{budget.ActiveNormalAttackers}/{budget.NormalCapacity}  " +
                $"E {budget.ActiveEliteAttackers}/{budget.EliteCapacity}  " +
                $"T {budget.ActiveAttackers}/{budget.TotalCapacity}";
            return health + builder + attackBudget + melee;
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
            DrawOffScreenAttackWarnings();
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

        public bool TryGetOffScreenAttackWarning(
            Camera camera,
            ShowcaseRunnerAgent runner,
            out ShowcaseOffScreenAttackWarning warning)
        {
            if (camera == null || runner == null || runner.IsDead ||
                !runner.IsAttackWindup || pressureController == null ||
                !pressureController.CanSimulatePressure())
            {
                warning = default;
                return false;
            }

            Vector3 viewport = camera.WorldToViewportPoint(
                runner.transform.position + Vector3.up * 1.45f);
            bool onScreen = viewport.z > 0f &&
                viewport.x >= 0f && viewport.x <= 1f &&
                viewport.y >= 0f && viewport.y <= 1f;
            if (onScreen)
            {
                warning = default;
                return false;
            }

            Vector2 direction = new Vector2(
                viewport.x - 0.5f,
                viewport.y - 0.5f);
            if (viewport.z <= 0f)
            {
                direction = -direction;
            }
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector2.up;
            }
            direction.Normalize();

            const float margin = 0.08f;
            const float halfExtent = 0.5f - margin;
            float horizontalScale = Mathf.Abs(direction.x) > 0.0001f
                ? halfExtent / Mathf.Abs(direction.x)
                : float.PositiveInfinity;
            float verticalScale = Mathf.Abs(direction.y) > 0.0001f
                ? halfExtent / Mathf.Abs(direction.y)
                : float.PositiveInfinity;
            float edgeScale = Mathf.Min(horizontalScale, verticalScale);
            Vector2 edge = new Vector2(0.5f, 0.5f) +
                direction * edgeScale;
            edge.x = Mathf.Clamp(edge.x, margin, 1f - margin);
            edge.y = Mathf.Clamp(edge.y, margin, 1f - margin);
            warning = new ShowcaseOffScreenAttackWarning(
                runner,
                edge,
                direction);
            return true;
        }

        private void DrawOffScreenAttackWarnings()
        {
            LastOffScreenWarningCount = 0;
            Camera camera = Camera.main;
            if (camera == null || pressureController == null ||
                !pressureController.CanSimulatePressure())
            {
                return;
            }

            offScreenWarningStyle ??= new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            float pulse = 0.72f +
                0.28f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 10f));
            offScreenWarningStyle.normal.textColor =
                new Color(1f, 0.72f, 0.16f, pulse);

            int warningLimit = Mathf.Clamp(maximumOffScreenWarnings, 1, 3);
            foreach (ShowcaseRunnerAgent runner in pressureController.Runners)
            {
                if (LastOffScreenWarningCount >= warningLimit)
                {
                    break;
                }
                if (!TryGetOffScreenAttackWarning(
                        camera,
                        runner,
                        out ShowcaseOffScreenAttackWarning warning))
                {
                    continue;
                }

                Vector2 screen = new Vector2(
                    warning.ViewportEdge.x * Screen.width,
                    (1f - warning.ViewportEdge.y) * Screen.height);
                GUI.Label(
                    new Rect(screen.x - 34f, screen.y - 22f, 68f, 44f),
                    ResolveWarningArrow(warning.Direction) + " !",
                    offScreenWarningStyle);
                LastOffScreenWarningCount++;
            }
        }

        private static string ResolveWarningArrow(Vector2 direction)
        {
            if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
            {
                return direction.x >= 0f ? "▶" : "◀";
            }
            return direction.y >= 0f ? "▲" : "▼";
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
            float hitStopSeconds = result.HitStopSeconds;
            if (hitStopSeconds <= 0f)
            {
                return;
            }
            ShowcaseCharacterAnimationDriver attackerDriver =
                result.Attacker?.GetComponent<ShowcaseCharacterAnimationDriver>();
            if (attackerDriver == null && result.HasObjectImpact &&
                pressureController != null &&
                pressureController.PlayerTransform != null)
            {
                attackerDriver = pressureController.PlayerTransform
                    .GetComponent<ShowcaseCharacterAnimationDriver>();
            }
            attackerDriver?.ApplyLocalHitStop(hitStopSeconds);
            result.Victim?.GetComponent<ShowcaseCharacterAnimationDriver>()
                ?.ApplyLocalHitStop(hitStopSeconds);
        }

        private void HandleBaselineReset()
        {
            contactMarkerUntil = 0f;
            contactAccepted = false;
            contactMarkerWorldPoint = Vector3.zero;
            LastOffScreenWarningCount = 0;
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
