using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    public readonly struct ShowcaseOrientedBoxGeometry
    {
        private const float MinimumAxisLength = 0.000001f;

        private ShowcaseOrientedBoxGeometry(
            Vector3 center,
            Vector3 right,
            Vector3 up,
            Vector3 forward,
            Vector3 halfExtents)
        {
            Center = center;
            Right = right;
            Up = up;
            Forward = forward;
            HalfExtents = halfExtents;
        }

        public Vector3 Center { get; }
        public Vector3 Right { get; }
        public Vector3 Up { get; }
        public Vector3 Forward { get; }
        public Vector3 HalfExtents { get; }

        public static bool TryCreate(
            BoxCollider collider,
            out ShowcaseOrientedBoxGeometry geometry)
        {
            geometry = default;
            if (collider == null || !collider.enabled)
            {
                return false;
            }

            Transform colliderTransform = collider.transform;
            Vector3 lossyScale = colliderTransform.lossyScale;
            Vector3 halfExtents = new Vector3(
                Mathf.Abs(collider.size.x * lossyScale.x) * 0.5f,
                Mathf.Abs(collider.size.y * lossyScale.y) * 0.5f,
                Mathf.Abs(collider.size.z * lossyScale.z) * 0.5f);
            Vector3 right = colliderTransform.TransformDirection(
                Vector3.right * AxisSign(lossyScale.x));
            Vector3 up = colliderTransform.TransformDirection(
                Vector3.up * AxisSign(lossyScale.y));
            Vector3 forward = colliderTransform.TransformDirection(
                Vector3.forward * AxisSign(lossyScale.z));
            Vector3 center = colliderTransform.TransformPoint(collider.center);
            if (!IsFinite(center) || !IsFinite(halfExtents) ||
                halfExtents.x <= MinimumAxisLength ||
                halfExtents.y <= MinimumAxisLength ||
                halfExtents.z <= MinimumAxisLength ||
                !TryNormalize(ref right) ||
                !TryNormalize(ref up) ||
                !TryNormalize(ref forward))
            {
                return false;
            }

            geometry = new ShowcaseOrientedBoxGeometry(
                center,
                right,
                up,
                forward,
                halfExtents);
            return true;
        }

        public float GetPlanarSupportRadius(Vector3 worldDirection)
        {
            Vector3 direction = Vector3.ProjectOnPlane(worldDirection, Vector3.up);
            if (!TryNormalize(ref direction))
            {
                return 0f;
            }

            return Mathf.Abs(Vector3.Dot(direction, Right)) * HalfExtents.x +
                   Mathf.Abs(Vector3.Dot(direction, Forward)) * HalfExtents.z;
        }

        private static bool TryNormalize(ref Vector3 value)
        {
            float sqrMagnitude = value.sqrMagnitude;
            if (!IsFinite(sqrMagnitude) ||
                sqrMagnitude <= MinimumAxisLength * MinimumAxisLength)
            {
                return false;
            }

            value /= Mathf.Sqrt(sqrMagnitude);
            return IsFinite(value);
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) &&
                   IsFinite(value.y) &&
                   IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static float AxisSign(float value)
        {
            return value < 0f ? -1f : 1f;
        }
    }
}
