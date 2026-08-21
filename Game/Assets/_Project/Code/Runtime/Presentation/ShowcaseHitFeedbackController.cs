using System;
using System.Collections.Generic;
using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Presentation
{
    [DefaultExecutionOrder(10000)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseHitFeedbackController : MonoBehaviour
    {
        [Serializable]
        public sealed class FeedbackProfile
        {
            [Min(0.01f)] public float Duration;
            [Min(0f)] public float PositionAmplitude;
            [Min(0f)] public float RotationAmplitude;
            [Range(0f, 1f)] public float VictimGlowIntensity;
            [Min(0.01f)] public float VictimGlowDuration;

            public FeedbackProfile(
                float duration,
                float positionAmplitude,
                float rotationAmplitude,
                float victimGlowIntensity,
                float victimGlowDuration)
            {
                Duration = duration;
                PositionAmplitude = positionAmplitude;
                RotationAmplitude = rotationAmplitude;
                VictimGlowIntensity = victimGlowIntensity;
                VictimGlowDuration = victimGlowDuration;
            }
        }

        private sealed class GlowRendererState
        {
            public Renderer Renderer;
            public int MaterialIndex;
            public Color BaselineEmission;
            public MaterialPropertyBlock Original;
            public MaterialPropertyBlock Working;
        }

        private sealed class VictimGlowState
        {
            public Transform Root;
            public GlowRendererState[] Renderers;
            public bool Active;
            public float StartedAt;
            public float Duration;
            public float Intensity;
            public float PeakHoldDuration;
        }

        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColorId =
            Shader.PropertyToID("_EmissionColor");
        private static readonly int EmissionMapId = Shader.PropertyToID("_EmissionMap");
        private const float MaximumEmissionPower = 4.5f;
        private const float MaximumPeakHoldDuration = 0.08f;

        [SerializeField]
        private ShowcaseRunnerPressureController pressureController;

        [SerializeField]
        private Camera outputCamera;

        [SerializeField]
        private ShowcaseHitRotationImpulseExtension cameraImpulse;

        [SerializeField]
        private FeedbackProfile lightProfile =
            new(0.075f, 0.055f, 0.32f, 0.55f, 0.16f);

        [SerializeField]
        private FeedbackProfile mediumProfile =
            new(0.10f, 0.085f, 0.60f, 0.75f, 0.18f);

        [SerializeField]
        private FeedbackProfile heavyProfile =
            new(0.135f, 0.125f, 0.90f, 1f, 0.20f);

        [SerializeField, ColorUsage(false, true)]
        private Color victimGlowColor = new(1f, 0.72f, 0.42f, 1f);

        private float shakeEndsAt;
        private uint feedbackSequence;
        private VictimGlowState[] victimGlowStates =
            Array.Empty<VictimGlowState>();

        public static ShowcaseHitFeedbackController Active { get; private set; }

        public int PlayCount { get; private set; }
        public ShowcaseHitVfxTier LastLevel { get; private set; }
        public Vector3 LastHitPoint { get; private set; }
        public Vector3 LastHitDirection { get; private set; }
        public float LastImpulseDuration { get; private set; }
        public bool IsShakeActive => Time.unscaledTime < shakeEndsAt ||
            (cameraImpulse != null && cameraImpulse.IsImpulseActive);
        public int GlowPlaybackCount { get; private set; }
        public int ActiveVictimGlowCount
        {
            get
            {
                int count = 0;
                foreach (VictimGlowState state in victimGlowStates)
                {
                    count += state.Active ? 1 : 0;
                }
                return count;
            }
        }
        public FeedbackProfile LightProfile => lightProfile;
        public FeedbackProfile MediumProfile => mediumProfile;
        public FeedbackProfile HeavyProfile => heavyProfile;

        public void Configure(
            ShowcaseRunnerPressureController pressure,
            Camera camera,
            ShowcaseHitRotationImpulseExtension impulse)
        {
            Unsubscribe();
            pressureController = pressure;
            outputCamera = camera;
            cameraImpulse = impulse;
            BuildVictimGlowStates();
            Subscribe();
        }

        public void Play(
            ShowcaseHitVfxTier level,
            Vector3 hitPoint,
            Vector3 hitDirection,
            Transform victim = null,
            float cameraImpulseMultiplier = 1f)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            FeedbackProfile profile = ResolveProfile(level);
            Vector3 localDirection = ResolveCameraLocalDirection(hitDirection);
            float impulseMultiplier = Mathf.Clamp(
                cameraImpulseMultiplier,
                0f,
                2f);
            float impulseDuration = profile.Duration * Mathf.Lerp(
                0.85f,
                1.15f,
                impulseMultiplier * 0.5f);

            feedbackSequence++;
            cameraImpulse?.Play(
                impulseDuration,
                profile.PositionAmplitude * impulseMultiplier,
                profile.RotationAmplitude * impulseMultiplier,
                localDirection,
                feedbackSequence);
            shakeEndsAt = Mathf.Max(
                shakeEndsAt,
                Time.unscaledTime + impulseDuration);
            StartVictimGlow(
                victim,
                profile.VictimGlowIntensity,
                profile.VictimGlowDuration);

            PlayCount++;
            LastLevel = level;
            LastHitPoint = hitPoint;
            LastHitDirection = hitDirection;
            LastImpulseDuration = impulseDuration;
        }

        public bool IsVictimGlowActive(Transform victim)
        {
            VictimGlowState state = ResolveVictimGlowState(victim);
            return state != null && state.Active;
        }

        private void OnEnable()
        {
            Active = this;
            BuildVictimGlowStates();
            Subscribe();
        }

        private void OnDisable()
        {
            if (Active == this)
            {
                Active = null;
            }
            Unsubscribe();
            ResetFeedback();
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            foreach (VictimGlowState state in victimGlowStates)
            {
                if (!state.Active)
                {
                    continue;
                }
                float normalized = Mathf.Clamp01(
                    (now - state.StartedAt - state.PeakHoldDuration) /
                    Mathf.Max(0.01f, state.Duration - state.PeakHoldDuration));
                if (now - state.StartedAt <= state.PeakHoldDuration)
                {
                    ApplyVictimGlow(state, state.Intensity);
                    continue;
                }
                if (normalized >= 1f)
                {
                    RestoreVictimGlow(state);
                    continue;
                }
                float remaining = 1f - normalized;
                ApplyVictimGlow(state, state.Intensity * remaining * remaining);
            }
        }

        private void HandleCombatContact(ShowcaseCombatContactResult result)
        {
            if (!result.Accepted || result.Attacker == null)
            {
                return;
            }

            Vector3 direction = result.Victim != null
                ? result.Victim.position - result.Attacker.position
                : result.Attacker.forward;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = result.Attacker.forward;
            }
            Play(
                ShowcaseCombatHitVfxPresenter.ResolveTier(result),
                result.WorldPoint,
                direction.normalized,
                result.Victim,
                result.CameraImpulseMultiplier);
        }

        private FeedbackProfile ResolveProfile(ShowcaseHitVfxTier level)
        {
            return level switch
            {
                ShowcaseHitVfxTier.Light => lightProfile,
                ShowcaseHitVfxTier.Medium => mediumProfile,
                ShowcaseHitVfxTier.Heavy => heavyProfile,
                _ => mediumProfile
            };
        }

        private Vector3 ResolveCameraLocalDirection(Vector3 worldDirection)
        {
            Vector3 normalized = worldDirection.sqrMagnitude > 0.001f
                ? worldDirection.normalized
                : Vector3.forward;
            return outputCamera != null
                ? outputCamera.transform.InverseTransformDirection(normalized).normalized
                : normalized;
        }

        private void BuildVictimGlowStates()
        {
            RestoreAllVictimGlows();
            if (pressureController == null)
            {
                victimGlowStates = Array.Empty<VictimGlowState>();
                return;
            }

            var roots = new List<Transform>(3);
            if (pressureController.PlayerTransform != null)
            {
                roots.Add(pressureController.PlayerTransform);
            }
            foreach (ShowcaseRunnerAgent runner in pressureController.Runners)
            {
                if (runner != null)
                {
                    roots.Add(runner.transform);
                }
            }

            victimGlowStates = new VictimGlowState[roots.Count];
            for (int index = 0; index < roots.Count; index++)
            {
                Transform visualRoot = roots[index].Find(
                    "RUNTIME_DESIGNER_A_VISUAL");
                Renderer[] renderers =
                    visualRoot != null
                        ? visualRoot.GetComponentsInChildren<Renderer>(true)
                        : Array.Empty<Renderer>();
                var states = new List<GlowRendererState>(renderers.Length);
                foreach (Renderer renderer in renderers)
                {
                    Material[] materials = renderer.sharedMaterials;
                    for (int materialIndex = 0;
                         materialIndex < materials.Length;
                         materialIndex++)
                    {
                        Material material = materials[materialIndex];
                        Color baselineEmission = material != null &&
                            material.HasProperty(EmissionColorId)
                                ? material.GetColor(EmissionColorId)
                                : Color.black;
                        states.Add(new GlowRendererState
                        {
                            Renderer = renderer,
                            MaterialIndex = materialIndex,
                            BaselineEmission = baselineEmission,
                            Original = new MaterialPropertyBlock(),
                            Working = new MaterialPropertyBlock()
                        });
                    }
                }
                victimGlowStates[index] = new VictimGlowState
                {
                    Root = roots[index],
                    Renderers = states.ToArray()
                };
            }
        }

        private void StartVictimGlow(
            Transform victim,
            float intensity,
            float duration)
        {
            VictimGlowState state = ResolveVictimGlowState(victim);
            if (state == null || state.Renderers.Length == 0)
            {
                return;
            }

            if (!state.Active)
            {
                foreach (GlowRendererState rendererState in state.Renderers)
                {
                    if (rendererState.Renderer == null)
                    {
                        continue;
                    }
                    rendererState.Original.Clear();
                    rendererState.Working.Clear();
                    rendererState.Renderer.GetPropertyBlock(
                        rendererState.Original,
                        rendererState.MaterialIndex);
                    rendererState.Renderer.GetPropertyBlock(
                        rendererState.Working,
                        rendererState.MaterialIndex);
                }
            }

            state.Active = true;
            state.StartedAt = Time.unscaledTime;
            state.Duration = Mathf.Max(0.01f, duration);
            state.Intensity = Mathf.Clamp01(intensity);
            state.PeakHoldDuration = Mathf.Min(
                MaximumPeakHoldDuration,
                state.Duration * 0.4f);
            ApplyVictimGlow(state, state.Intensity);
            GlowPlaybackCount++;
        }

        private VictimGlowState ResolveVictimGlowState(Transform victim)
        {
            if (victim == null)
            {
                return null;
            }
            foreach (VictimGlowState state in victimGlowStates)
            {
                if (state.Root == victim || victim.IsChildOf(state.Root) ||
                    state.Root.IsChildOf(victim))
                {
                    return state;
                }
            }
            return null;
        }

        private void ApplyVictimGlow(VictimGlowState state, float amount)
        {
            float t = Mathf.Clamp01(amount);
            Color tint = Color.Lerp(
                Color.white,
                new Color(1.10f, 1.03f, 0.92f, 1f),
                t);
            Color emission = victimGlowColor * (MaximumEmissionPower * t);
            emission.a = 1f;
            foreach (GlowRendererState rendererState in state.Renderers)
            {
                if (rendererState.Renderer == null)
                {
                    continue;
                }
                rendererState.Working.SetColor(ColorId, tint);
                rendererState.Working.SetColor(BaseColorId, tint);
                rendererState.Working.SetTexture(
                    EmissionMapId,
                    Texture2D.whiteTexture);
                rendererState.Working.SetColor(
                    EmissionColorId,
                    rendererState.BaselineEmission + emission);
                rendererState.Renderer.SetPropertyBlock(
                    rendererState.Working,
                    rendererState.MaterialIndex);
            }
        }

        private static void RestoreVictimGlow(VictimGlowState state)
        {
            foreach (GlowRendererState rendererState in state.Renderers)
            {
                if (rendererState.Renderer != null)
                {
                    rendererState.Renderer.SetPropertyBlock(
                        rendererState.Original,
                        rendererState.MaterialIndex);
                }
            }
            state.Active = false;
        }

        private void RestoreAllVictimGlows()
        {
            foreach (VictimGlowState state in victimGlowStates)
            {
                if (state.Active)
                {
                    RestoreVictimGlow(state);
                }
            }
        }

        private void HandleBaselineReset()
        {
            ResetFeedback();
            PlayCount = 0;
            feedbackSequence = 0;
            GlowPlaybackCount = 0;
            LastHitPoint = Vector3.zero;
            LastHitDirection = Vector3.zero;
            LastImpulseDuration = 0f;
        }

        private void ResetFeedback()
        {
            shakeEndsAt = 0f;
            cameraImpulse?.ResetFeedback();
            RestoreAllVictimGlows();
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
