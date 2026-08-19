using Unity.Cinemachine;
using UnityEngine;

namespace BreakNotice.Presentation
{
    [DefaultExecutionOrder(15000)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseHitRotationImpulseExtension : CinemachineExtension
    {
        private float startedAt;
        private float duration;
        private float positionAmplitude;
        private float amplitude;
        private Vector3 cameraLocalDirection;
        private int directionSign = 1;
        private bool active;
        private bool pendingFirstApplication;

        public bool IsImpulseActive => active &&
            (pendingFirstApplication || Time.unscaledTime - startedAt < duration);

        public Vector3 LastAppliedEuler { get; private set; }
        public float PeakAppliedEulerMagnitude { get; private set; }
        public Vector3 LastAppliedPosition { get; private set; }
        public float PeakAppliedPositionMagnitude { get; private set; }

        public void Play(
            float impulseDuration,
            float positionalAmplitude,
            float rotationAmplitude,
            Vector3 localDirection,
            uint sequence)
        {
            if (!IsImpulseActive)
            {
                amplitude = 0f;
                positionAmplitude = 0f;
            }
            float current = EvaluateCurrentAmplitude();
            float requested = Mathf.Max(0f, rotationAmplitude);
            if (requested >= current)
            {
                cameraLocalDirection = localDirection.sqrMagnitude > 0.001f
                    ? localDirection.normalized
                    : Vector3.forward;
            }

            amplitude = Mathf.Max(current, requested);
            positionAmplitude = Mathf.Max(positionAmplitude, positionalAmplitude);
            duration = Mathf.Max(0.01f, impulseDuration);
            startedAt = Time.unscaledTime;
            directionSign = (sequence & 1u) == 0u ? 1 : -1;
            active = amplitude > 0f;
            pendingFirstApplication = active;
        }

        public void ResetFeedback()
        {
            active = false;
            pendingFirstApplication = false;
            amplitude = 0f;
            positionAmplitude = 0f;
            duration = 0f;
            LastAppliedEuler = Vector3.zero;
            LastAppliedPosition = Vector3.zero;
        }

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage,
            ref CameraState state,
            float deltaTime)
        {
            // ShowcaseCameraConstraintSolver owns the deterministic tail call.
            // Applying here would make extension registration order observable.
        }

        public void ApplyAfterConstraint(ref CameraState state)
        {
            if (!active)
            {
                return;
            }

            float elapsed = Time.unscaledTime - startedAt;
            bool firstApplication = pendingFirstApplication;
            pendingFirstApplication = false;
            if (!firstApplication && elapsed >= duration)
            {
                ResetFeedback();
                return;
            }

            float normalized = firstApplication
                ? 0f
                : Mathf.Clamp01(elapsed / duration);
            float decay = 1f - normalized;
            decay *= decay;
            float kick = Mathf.Cos(normalized * Mathf.PI * 2.35f) * decay;
            float horizontal = Mathf.Clamp(cameraLocalDirection.x, -1f, 1f);
            float vertical = Mathf.Clamp(cameraLocalDirection.y, -1f, 1f);
            Vector3 localPositionDirection = new Vector3(
                -horizontal * 0.72f,
                Mathf.Clamp(0.30f - (vertical * 0.22f), -0.25f, 0.5f),
                0.28f).normalized;
            LastAppliedPosition = state.GetFinalOrientation() *
                (localPositionDirection * positionAmplitude * kick);
            PeakAppliedPositionMagnitude = Mathf.Max(
                PeakAppliedPositionMagnitude,
                LastAppliedPosition.magnitude);
            LastAppliedEuler = new Vector3(
                ((-vertical * 0.35f) + (directionSign * 0.75f)) * amplitude * kick,
                ((horizontal * 0.90f) + (directionSign * 0.25f)) * amplitude * kick,
                ((-horizontal * 0.35f) + (directionSign * 0.20f)) *
                    amplitude * kick);
            PeakAppliedEulerMagnitude = Mathf.Max(
                PeakAppliedEulerMagnitude,
                LastAppliedEuler.magnitude);
            state.PositionCorrection += LastAppliedPosition;
            state.OrientationCorrection *= Quaternion.Euler(LastAppliedEuler);
        }

        private float EvaluateCurrentAmplitude()
        {
            if (!active || duration <= 0f)
            {
                return 0f;
            }
            float normalized = Mathf.Clamp01(
                (Time.unscaledTime - startedAt) / duration);
            float remaining = 1f - normalized;
            return amplitude * remaining * remaining;
        }

        private void OnDisable()
        {
            ResetFeedback();
        }
    }
}
