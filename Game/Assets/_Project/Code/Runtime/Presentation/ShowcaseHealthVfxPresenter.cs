using System;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Presentation
{
    [DefaultExecutionOrder(300)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseHealthVfxPresenter : MonoBehaviour
    {
        private const int SegmentCount = 7;
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;
        private const float HeadTopOffset = 0.20f;

        private static readonly Color EmptyGraphite =
            new(0.19f, 0.20f, 0.20f, 0.72f);
        private static readonly Color PlayerHealthRed =
            new(0.72f, 0.20f, 0.15f, 0.98f);
        private static readonly Color PlayerHealthDeepRed =
            new(0.52f, 0.10f, 0.08f, 0.98f);
        private static readonly Color EnemyHealthRed =
            new(0.62f, 0.15f, 0.11f, 0.96f);
        private static readonly Color CriticalHealthRed =
            new(0.82f, 0.08f, 0.05f, 0.99f);
        private static readonly Color DamageTrail =
            new(0.82f, 0.73f, 0.57f, 0.82f);

        [Serializable]
        private sealed class HealthVisual
        {
            public ShowcaseCombatHealth Health;
            public ShowcaseRunnerAgent Runner;
            public float Target = 1f;
            public float Current = 1f;
            public float Trail = 1f;
            public float TrailDelay;
        }

        [SerializeField]
        private ShowcaseRunnerPressureController pressureController;

        [SerializeField]
        private Camera gameplayCamera;

        [SerializeField, Min(0.01f)]
        private float damageTrailDelay = 0.08f;

        [SerializeField, Min(0.05f)]
        private float damageTrailDuration = 0.32f;

        [SerializeField, Min(0.05f)]
        private float recoveryDuration = 0.22f;

        private HealthVisual playerVisual;
        private HealthVisual[] runnerVisuals = Array.Empty<HealthVisual>();
        private Texture2D solidTexture;
        private GUIStyle conditionStyle;

        public static ShowcaseHealthVfxPresenter Active { get; private set; }

        public float PlayerHealthNormalized => playerVisual?.Target ?? 0f;
        public float PlayerHealthTrailNormalized => playerVisual?.Trail ?? 0f;

        public void Configure(
            ShowcaseRunnerPressureController pressure,
            Camera camera)
        {
            pressureController = pressure;
            gameplayCamera = camera;
            BuildHealthSources();
        }

        public static int CalculateHealthSegmentCount(float normalizedHealth)
        {
            return Mathf.Clamp(
                Mathf.CeilToInt(Mathf.Clamp01(normalizedHealth) * SegmentCount),
                0,
                SegmentCount);
        }

        private void Awake()
        {
            ResolveReferences();
            BuildHealthSources();
            EnsureTexture();
        }

        private void OnEnable()
        {
            Active = this;
        }

        private void OnDisable()
        {
            if (Active == this)
            {
                Active = null;
            }
        }

        private void OnDestroy()
        {
            if (solidTexture == null)
            {
                return;
            }
            if (Application.isPlaying)
            {
                Destroy(solidTexture);
            }
            else
            {
                DestroyImmediate(solidTexture);
            }
        }

        private void Update()
        {
            ResolveReferences();
            if (playerVisual == null ||
                playerVisual.Health != pressureController?.PlayerHealth)
            {
                BuildHealthSources();
            }

            float deltaTime = Mathf.Max(0f, Time.unscaledDeltaTime);
            TickVisual(playerVisual, deltaTime);
            foreach (HealthVisual visual in runnerVisuals)
            {
                TickVisual(visual, deltaTime);
            }
        }

        private void OnGUI()
        {
            ResolveReferences();
            EnsureTexture();
            EnsureStyle();
            if (playerVisual == null)
            {
                return;
            }

            float scale = Mathf.Max(
                0.45f,
                Mathf.Min(
                    Screen.width / ReferenceWidth,
                    Screen.height / ReferenceHeight));
            DrawPlayerHealth(scale);
            DrawRunnerHealth(scale);
        }

        private void BuildHealthSources()
        {
            if (pressureController == null)
            {
                playerVisual = null;
                runnerVisuals = Array.Empty<HealthVisual>();
                return;
            }

            playerVisual = CreateVisual(pressureController.PlayerHealth, null);
            ShowcaseRunnerAgent[] runners = pressureController.Runners ??
                Array.Empty<ShowcaseRunnerAgent>();
            runnerVisuals = new HealthVisual[runners.Length];
            for (int index = 0; index < runners.Length; index++)
            {
                ShowcaseRunnerAgent runner = runners[index];
                runnerVisuals[index] = CreateVisual(runner?.Health, runner);
            }
        }

        private static HealthVisual CreateVisual(
            ShowcaseCombatHealth health,
            ShowcaseRunnerAgent runner)
        {
            float normalized = health != null ? health.NormalizedHealth : 0f;
            return new HealthVisual
            {
                Health = health,
                Runner = runner,
                Target = normalized,
                Current = normalized,
                Trail = normalized
            };
        }

        private void TickVisual(HealthVisual visual, float deltaTime)
        {
            if (visual?.Health == null)
            {
                return;
            }

            float normalized = visual.Health.NormalizedHealth;
            if (normalized < visual.Current)
            {
                visual.Trail = Mathf.Max(visual.Trail, visual.Current);
                visual.Current = normalized;
                visual.TrailDelay = damageTrailDelay;
            }
            visual.Target = normalized;

            if (visual.Target > visual.Current)
            {
                visual.Current = Mathf.MoveTowards(
                    visual.Current,
                    visual.Target,
                    deltaTime / recoveryDuration);
                visual.Trail = visual.Current;
            }
            else if (visual.TrailDelay > 0f)
            {
                visual.TrailDelay = Mathf.Max(0f, visual.TrailDelay - deltaTime);
            }
            else if (visual.Trail > visual.Current)
            {
                visual.Trail = Mathf.MoveTowards(
                    visual.Trail,
                    visual.Current,
                    deltaTime / damageTrailDuration);
            }
        }

        private void DrawPlayerHealth(float scale)
        {
            float left = 68f * scale;
            float labelTop = Screen.height - (112f * scale);
            conditionStyle.fontSize = Mathf.Max(15, Mathf.RoundToInt(27f * scale));
            GUI.Label(
                new Rect(left, labelTop, 260f * scale, 34f * scale),
                "CONDITION",
                conditionStyle);
            DrawSegmentedBar(
                new Rect(
                    left,
                    Screen.height - (60f * scale),
                    570f * scale,
                    8f * scale),
                playerVisual.Current,
                playerVisual.Trail,
                7f * scale,
                true);
        }

        private void DrawRunnerHealth(float scale)
        {
            Camera camera = ResolveCamera();
            if (camera == null)
            {
                return;
            }

            foreach (HealthVisual visual in runnerVisuals)
            {
                if (visual?.Runner == null || visual.Health == null ||
                    visual.Health.IsDead)
                {
                    continue;
                }

                Vector3 screenPoint = camera.WorldToScreenPoint(
                    ResolveRunnerHeadTop(visual.Runner));
                if (screenPoint.z <= 0f ||
                    screenPoint.x < 0f || screenPoint.x > Screen.width ||
                    screenPoint.y < 0f || screenPoint.y > Screen.height)
                {
                    continue;
                }

                float width = 96f * scale;
                float height = Mathf.Max(3f, 5f * scale);
                DrawSegmentedBar(
                    new Rect(
                        screenPoint.x - (width * 0.5f),
                        Screen.height - screenPoint.y - height - (4f * scale),
                        width,
                        height),
                    visual.Current,
                    visual.Trail,
                    2f * scale,
                    false);
            }
        }

        private static Vector3 ResolveRunnerHeadTop(ShowcaseRunnerAgent runner)
        {
            Animator[] animators = runner.GetComponentsInChildren<Animator>(true);
            foreach (Animator animator in animators)
            {
                if (!animator.isHuman)
                {
                    continue;
                }
                Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
                if (head != null)
                {
                    return head.position + Vector3.up * HeadTopOffset;
                }
            }

            Collider collider = runner.GetComponent<Collider>();
            return collider != null
                ? new Vector3(
                    collider.bounds.center.x,
                    collider.bounds.max.y + 0.06f,
                    collider.bounds.center.z)
                : runner.transform.position + Vector3.up * 1.9f;
        }

        private void DrawSegmentedBar(
            Rect rect,
            float current,
            float trail,
            float gap,
            bool playerPalette)
        {
            float segmentWidth =
                (rect.width - (gap * (SegmentCount - 1))) / SegmentCount;
            float currentScaled = Mathf.Clamp01(current) * SegmentCount;
            float trailScaled = Mathf.Clamp01(trail) * SegmentCount;
            for (int index = 0; index < SegmentCount; index++)
            {
                Rect segment = new(
                    rect.x + ((segmentWidth + gap) * index),
                    rect.y,
                    segmentWidth,
                    rect.height);
                DrawSolidRect(segment, EmptyGraphite);
                DrawPartialSegment(segment, trailScaled - index, DamageTrail);
                Color fill = playerPalette
                    ? GetPlayerHealthColor(current, index)
                    : GetEnemyHealthColor(current);
                DrawPartialSegment(segment, currentScaled - index, fill);
            }
        }

        private void DrawPartialSegment(Rect segment, float fill01, Color color)
        {
            float clamped = Mathf.Clamp01(fill01);
            if (clamped <= 0f)
            {
                return;
            }
            segment.width *= clamped;
            DrawSolidRect(segment, color);
        }

        private void DrawSolidRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, solidTexture, ScaleMode.StretchToFill, true);
            GUI.color = previous;
        }

        private static Color GetPlayerHealthColor(float health, int index)
        {
            if (health <= 0.28f)
            {
                return CriticalHealthRed;
            }
            return Color.Lerp(
                PlayerHealthRed,
                PlayerHealthDeepRed,
                Mathf.Clamp01((index / (float)(SegmentCount - 1)) * 1.15f));
        }

        private static Color GetEnemyHealthColor(float health)
        {
            return health <= 0.28f ? CriticalHealthRed : EnemyHealthRed;
        }

        private void ResolveReferences()
        {
            if (pressureController == null)
            {
                pressureController = FindFirstObjectByType<
                    ShowcaseRunnerPressureController>();
            }
            ResolveCamera();
        }

        private Camera ResolveCamera()
        {
            if (gameplayCamera == null)
            {
                gameplayCamera = Camera.main;
            }
            return gameplayCamera;
        }

        private void EnsureTexture()
        {
            if (solidTexture != null)
            {
                return;
            }
            solidTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                name = "ShowcaseHealthVfxSolid",
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            solidTexture.SetPixel(0, 0, Color.white);
            solidTexture.Apply(false, true);
        }

        private void EnsureStyle()
        {
            conditionStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = new Color(0.90f, 0.88f, 0.80f, 0.96f)
                }
            };
        }
    }
}
