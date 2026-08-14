using System;
using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Presentation
{
    [DefaultExecutionOrder(300)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseHudPresenter : MonoBehaviour
    {
        private const int HealthSegmentCount = 7;
        private const int ChargeSegmentCount = 12;
        private const int ChargeTextureSize = 128;
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        private static readonly Color Graphite = new Color(0.075f, 0.082f, 0.085f, 0.84f);
        private static readonly Color EmptyGraphite = new Color(0.19f, 0.20f, 0.20f, 0.72f);
        private static readonly Color AgedIvory = new Color(0.86f, 0.83f, 0.73f, 0.96f);
        private static readonly Color BlueGray = new Color(0.43f, 0.54f, 0.60f, 0.96f);
        private static readonly Color MutedRust = new Color(0.62f, 0.25f, 0.17f, 0.98f);
        private static readonly Color ChargeRed = new Color(0.56f, 0.10f, 0.075f, 0.97f);
        private static readonly Color ChargePeakRed = new Color(0.74f, 0.16f, 0.09f, 0.99f);
        private static readonly Color DamageTrail = new Color(0.82f, 0.73f, 0.57f, 0.82f);

        [SerializeField]
        private PrototypeInteractionController interactionController;

        [SerializeField]
        private Camera gameplayCamera;

        [SerializeField, Min(0.01f)]
        private float damageTrailDelay = 0.08f;

        [SerializeField, Min(0.05f)]
        private float damageTrailDuration = 0.32f;

        [SerializeField, Min(0.05f)]
        private float recoveryDuration = 0.22f;

        private ShowcaseWorldHealthAnchor[] worldHealthAnchors =
            Array.Empty<ShowcaseWorldHealthAnchor>();
        private Texture2D solidTexture;
        private Texture2D[] chargeTextures;
        private GUIStyle conditionStyle;
        private float playerHealthTarget = 1f;
        private float playerHealthVisual = 1f;
        private float playerHealthTrail = 1f;
        private float damageTrailDelayRemaining;
        private bool previousChargeVisible;
        private float lastCharge01;
        private float chargeReleaseEchoRemaining;
        private float anchorRefreshRemaining;

        public static ShowcaseHudPresenter Active { get; private set; }

        public float PlayerHealthNormalized => playerHealthTarget;

        public float PlayerHealthVisualNormalized => playerHealthVisual;

        public float PlayerHealthTrailNormalized => playerHealthTrail;

        public PrototypeInteractionController InteractionController => interactionController;

        public void Configure(
            PrototypeInteractionController controller,
            Camera camera = null)
        {
            interactionController = controller;
            gameplayCamera = camera;
        }

        public void SetPlayerHealthNormalized(float value)
        {
            float clamped = Mathf.Clamp01(value);
            if (clamped < playerHealthVisual)
            {
                playerHealthTrail = Mathf.Max(playerHealthTrail, playerHealthVisual);
                playerHealthVisual = clamped;
                damageTrailDelayRemaining = damageTrailDelay;
            }

            playerHealthTarget = clamped;
        }

        public void SetPlayerHealth(float current, float maximum)
        {
            SetPlayerHealthNormalized(maximum > 0f ? current / maximum : 0f);
        }

        public void RefreshWorldHealthAnchors()
        {
            worldHealthAnchors = FindObjectsByType<ShowcaseWorldHealthAnchor>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
        }

        public static int CalculateChargeSegmentCount(float normalizedCharge)
        {
            return Mathf.Clamp(
                Mathf.CeilToInt(Mathf.Clamp01(normalizedCharge) * ChargeSegmentCount),
                0,
                ChargeSegmentCount);
        }

        public static int CalculateHealthSegmentCount(float normalizedHealth)
        {
            return Mathf.Clamp(
                Mathf.CeilToInt(Mathf.Clamp01(normalizedHealth) * HealthSegmentCount),
                0,
                HealthSegmentCount);
        }

        public void TickPresentation(float unscaledDeltaTime)
        {
            float deltaTime = Mathf.Max(0f, unscaledDeltaTime);
            if (playerHealthTarget > playerHealthVisual)
            {
                playerHealthVisual = Mathf.MoveTowards(
                    playerHealthVisual,
                    playerHealthTarget,
                    deltaTime / recoveryDuration);
                playerHealthTrail = playerHealthVisual;
            }
            else if (damageTrailDelayRemaining > 0f)
            {
                damageTrailDelayRemaining = Mathf.Max(
                    0f,
                    damageTrailDelayRemaining - deltaTime);
            }
            else if (playerHealthTrail > playerHealthVisual)
            {
                playerHealthTrail = Mathf.MoveTowards(
                    playerHealthTrail,
                    playerHealthVisual,
                    deltaTime / damageTrailDuration);
            }

            chargeReleaseEchoRemaining = Mathf.Max(
                0f,
                chargeReleaseEchoRemaining - deltaTime);
        }

        private void Awake()
        {
            ResolveReferences();
            EnsureTextures();
            RefreshWorldHealthAnchors();
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
            DestroyTexture(solidTexture);
            if (chargeTextures == null)
            {
                return;
            }

            foreach (Texture2D texture in chargeTextures)
            {
                DestroyTexture(texture);
            }
        }

        private void Update()
        {
            TickPresentation(Time.unscaledDeltaTime);

            bool chargeVisible = interactionController != null &&
                                 interactionController.HasThrowCharge;
            if (chargeVisible)
            {
                lastCharge01 = interactionController.ThrowCharge01;
            }
            else if (previousChargeVisible)
            {
                chargeReleaseEchoRemaining = 0.12f;
            }

            previousChargeVisible = chargeVisible;
            anchorRefreshRemaining -= Time.unscaledDeltaTime;
            if (anchorRefreshRemaining <= 0f)
            {
                anchorRefreshRemaining = 0.5f;
                RefreshWorldHealthAnchors();
            }
        }

        private void OnGUI()
        {
            EnsureTextures();
            EnsureStyles();
            ResolveReferences();

            float scale = Mathf.Max(
                0.45f,
                Mathf.Min(Screen.width / ReferenceWidth, Screen.height / ReferenceHeight));
            DrawPlayerHealth(scale);
            DrawCharge(scale);
            DrawWorldHealth(scale);
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
                playerHealthVisual,
                playerHealthTrail,
                HealthSegmentCount,
                7f * scale,
                true);
        }

        private void DrawCharge(float scale)
        {
            bool active = interactionController != null && interactionController.HasThrowCharge;
            if (!active && chargeReleaseEchoRemaining <= 0f)
            {
                return;
            }

            float charge01 = active ? interactionController.ThrowCharge01 : lastCharge01;
            int stage = CalculateChargeSegmentCount(charge01);
            float echo01 = active ? 0f : 1f - (chargeReleaseEchoRemaining / 0.12f);
            float pulse = active && charge01 >= 0.999f
                ? 1f + (Mathf.Sin(Time.unscaledTime * 13f) * 0.025f)
                : 1f;
            float size = 92f * scale * pulse * (1f + (echo01 * 0.16f));
            Rect rect = new Rect(
                (Screen.width - size) * 0.5f,
                (Screen.height - size) * 0.5f,
                size,
                size);

            Color previous = GUI.color;
            GUI.color = active
                ? Color.white
                : new Color(1f, 1f, 1f, Mathf.Clamp01(1f - echo01));
            GUI.DrawTexture(rect, chargeTextures[stage], ScaleMode.StretchToFill, true);
            GUI.color = previous;
        }

        private void DrawWorldHealth(float scale)
        {
            Camera camera = ResolveCamera();
            if (camera == null)
            {
                return;
            }

            foreach (ShowcaseWorldHealthAnchor anchor in worldHealthAnchors)
            {
                if (anchor == null || !anchor.Visible)
                {
                    continue;
                }

                Vector3 screenPoint = camera.WorldToScreenPoint(anchor.transform.position);
                if (screenPoint.z <= 0f ||
                    screenPoint.x < 0f || screenPoint.x > Screen.width ||
                    screenPoint.y < 0f || screenPoint.y > Screen.height)
                {
                    continue;
                }

                float width = 96f * scale;
                DrawSegmentedBar(
                    new Rect(
                        screenPoint.x - (width * 0.5f),
                        Screen.height - screenPoint.y,
                        width,
                        Mathf.Max(3f, 5f * scale)),
                    anchor.NormalizedHealth,
                    anchor.NormalizedHealth,
                    HealthSegmentCount,
                    2f * scale,
                    false);
            }
        }

        private void DrawSegmentedBar(
            Rect rect,
            float current,
            float trail,
            int segmentCount,
            float gap,
            bool playerPalette)
        {
            float totalGap = gap * (segmentCount - 1);
            float segmentWidth = (rect.width - totalGap) / segmentCount;
            float currentScaled = Mathf.Clamp01(current) * segmentCount;
            float trailScaled = Mathf.Clamp01(trail) * segmentCount;

            for (int index = 0; index < segmentCount; index++)
            {
                Rect segment = new Rect(
                    rect.x + ((segmentWidth + gap) * index),
                    rect.y,
                    segmentWidth,
                    rect.height);
                DrawSolidRect(segment, EmptyGraphite);
                DrawPartialSegment(segment, trailScaled - index, DamageTrail);

                float localFill = Mathf.Clamp01(currentScaled - index);
                Color fill = playerPalette
                    ? GetPlayerHealthColor(current, index, segmentCount)
                    : GetEnemyHealthColor(current);
                DrawPartialSegment(segment, localFill, fill);
            }
        }

        private void DrawPartialSegment(Rect segment, float fill01, Color color)
        {
            float clamped = Mathf.Clamp01(fill01);
            if (clamped <= 0f)
            {
                return;
            }

            Rect fill = segment;
            fill.width *= clamped;
            DrawSolidRect(fill, color);
        }

        private void DrawSolidRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, solidTexture, ScaleMode.StretchToFill, true);
            GUI.color = previous;
        }

        private static Color GetPlayerHealthColor(
            float normalizedHealth,
            int index,
            int segmentCount)
        {
            if (normalizedHealth <= 0.28f)
            {
                return MutedRust;
            }

            float t = segmentCount <= 1 ? 0f : index / (float)(segmentCount - 1);
            return Color.Lerp(AgedIvory, BlueGray, Mathf.Clamp01(t * 1.35f));
        }

        private static Color GetEnemyHealthColor(float normalizedHealth)
        {
            return normalizedHealth <= 0.28f
                ? MutedRust
                : new Color(0.78f, 0.76f, 0.67f, 0.94f);
        }

        private void ResolveReferences()
        {
            if (interactionController == null)
            {
                interactionController = FindFirstObjectByType<PrototypeInteractionController>();
            }

            ResolveCamera();
        }

        private Camera ResolveCamera()
        {
            if (gameplayCamera != null)
            {
                return gameplayCamera;
            }

            gameplayCamera = interactionController != null
                ? interactionController.ViewCamera
                : null;
            if (gameplayCamera == null)
            {
                gameplayCamera = Camera.main;
            }

            return gameplayCamera;
        }

        private void EnsureStyles()
        {
            if (conditionStyle != null)
            {
                return;
            }

            conditionStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.90f, 0.88f, 0.80f, 0.96f) }
            };
        }

        private void EnsureTextures()
        {
            if (solidTexture == null)
            {
                solidTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
                {
                    name = "ShowcaseHudSolid",
                    hideFlags = HideFlags.HideAndDontSave,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
                solidTexture.SetPixel(0, 0, Color.white);
                solidTexture.Apply(false, true);
            }

            if (chargeTextures != null && chargeTextures.Length == ChargeSegmentCount + 1)
            {
                return;
            }

            chargeTextures = new Texture2D[ChargeSegmentCount + 1];
            for (int stage = 0; stage <= ChargeSegmentCount; stage++)
            {
                chargeTextures[stage] = BuildChargeTexture(stage);
            }
        }

        private static Texture2D BuildChargeTexture(int activeSegments)
        {
            var texture = new Texture2D(
                ChargeTextureSize,
                ChargeTextureSize,
                TextureFormat.RGBA32,
                false)
            {
                name = $"ShowcaseCharge_{activeSegments:00}",
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[ChargeTextureSize * ChargeTextureSize];
            Vector2 center = new Vector2(
                (ChargeTextureSize - 1) * 0.5f,
                (ChargeTextureSize - 1) * 0.5f);

            for (int y = 0; y < ChargeTextureSize; y++)
            {
                for (int x = 0; x < ChargeTextureSize; x++)
                {
                    Vector2 delta = new Vector2(x, y) - center;
                    float radius = delta.magnitude;
                    Color color = Color.clear;
                    if (radius >= 42f && radius <= 47f)
                    {
                        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
                        int segment = GetChargeSegment(angle);
                        if (segment >= 0)
                        {
                            bool active = segment < activeSegments;
                            color = active
                                ? Color.Lerp(
                                    ChargeRed,
                                    ChargePeakRed,
                                    segment / (float)(ChargeSegmentCount - 1))
                                : new Color(Graphite.r, Graphite.g, Graphite.b, 0.42f);
                            float edge = Mathf.Min(radius - 42f, 47f - radius);
                            color.a *= Mathf.Clamp01(edge / 1.15f);
                        }
                    }
                    else if (radius <= 2.1f)
                    {
                        color = new Color(ChargeRed.r, ChargeRed.g, ChargeRed.b, 0.78f);
                    }

                    pixels[(y * ChargeTextureSize) + x] = color;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private static int GetChargeSegment(float angle)
        {
            float normalized;
            int sideOffset;
            if (angle >= -58f && angle <= 58f)
            {
                normalized = Mathf.InverseLerp(-58f, 58f, angle);
                sideOffset = 1;
            }
            else if (angle >= 122f && angle <= 180f)
            {
                normalized = Mathf.InverseLerp(122f, 238f, angle);
                sideOffset = 0;
            }
            else if (angle >= -180f && angle <= -122f)
            {
                normalized = Mathf.InverseLerp(122f, 238f, angle + 360f);
                sideOffset = 0;
            }
            else
            {
                return -1;
            }

            float sixSegments = normalized * 6f;
            int local = Mathf.Clamp(Mathf.FloorToInt(sixSegments), 0, 5);
            float withinSegment = sixSegments - Mathf.Floor(sixSegments);
            if (withinSegment < 0.13f || withinSegment > 0.87f)
            {
                return -1;
            }

            return (local * 2) + sideOffset;
        }

        private static void DestroyTexture(Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(texture);
            }
            else
            {
                DestroyImmediate(texture);
            }
        }
    }
}
