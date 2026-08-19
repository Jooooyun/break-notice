using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    public readonly struct PrototypeThrowSolution
    {
        public PrototypeThrowSolution(
            Vector3 origin,
            Vector3 initialVelocity,
            Vector3 gravity)
        {
            Origin = origin;
            InitialVelocity = initialVelocity;
            Gravity = gravity;
        }

        public Vector3 Origin { get; }

        public Vector3 InitialVelocity { get; }

        public Vector3 Gravity { get; }

        public bool IsValid =>
            IsFinite(Origin) &&
            IsFinite(InitialVelocity) &&
            IsFinite(Gravity) &&
            InitialVelocity.sqrMagnitude > 0.0001f;

        public Vector3 PositionAt(float time)
        {
            float clampedTime = Mathf.Max(0f, time);
            return Origin + InitialVelocity * clampedTime +
                0.5f * Gravity * clampedTime * clampedTime;
        }

        private static bool IsFinite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
                !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
                !float.IsNaN(value.z) && !float.IsInfinity(value.z);
        }
    }

    public static class PrototypeThrowBallistics
    {
        public const float UpwardArcBiasSpeed = 3.2f;

        public static PrototypeThrowSolution Solve(
            Vector3 origin,
            Vector3 aimDirection,
            float totalSpeed,
            Vector3 fallbackForward,
            Vector3 gravity)
        {
            Vector3 direction = aimDirection.sqrMagnitude > 0.0001f
                ? aimDirection.normalized
                : fallbackForward.sqrMagnitude > 0.0001f
                    ? fallbackForward.normalized
                    : Vector3.forward;
            Vector3 liftedDirection = direction * Mathf.Max(0.1f, totalSpeed) +
                Vector3.up * UpwardArcBiasSpeed;
            if (liftedDirection.sqrMagnitude <= 0.0001f)
            {
                liftedDirection = Vector3.up;
            }

            Vector3 initialVelocity = liftedDirection.normalized *
                Mathf.Max(0.1f, totalSpeed);
            return new PrototypeThrowSolution(origin, initialVelocity, gravity);
        }
    }
}
