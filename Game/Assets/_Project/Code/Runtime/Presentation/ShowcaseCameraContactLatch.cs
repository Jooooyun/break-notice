using UnityEngine;

namespace BreakNotice.Presentation
{
    public enum ShowcaseStaticCameraContactPhase
    {
        Clear,
        Contact,
        Restoring
    }

    /// <summary>
    /// Deterministic, geometry-identity-independent boom recovery authority.
    /// A contact episode only permits additional inward compression.  Once the
    /// full requested pose stays clear long enough, exactly one monotonic
    /// restoration returns the boom to its requested distance.
    /// </summary>
    public sealed class ShowcaseCameraContactLatch
    {
        private float clearEvidence;
        private float restoreElapsed;
        private float restoreStartBoom;
        private float restoreTargetBoom;

        public ShowcaseStaticCameraContactPhase Phase { get; private set; } =
            ShowcaseStaticCameraContactPhase.Clear;

        public int ContactEpisodeId { get; private set; }

        public int RestoreStartCount { get; private set; }

        public float CurrentBoom { get; private set; }

        public float ClearEvidence => clearEvidence;

        public float RestoreProgress => Phase !=
                ShowcaseStaticCameraContactPhase.Restoring ||
            restoreTargetBoom <= restoreStartBoom + 0.0001f
                ? Phase == ShowcaseStaticCameraContactPhase.Clear ? 1f : 0f
                : Mathf.Clamp01(
                    (CurrentBoom - restoreStartBoom) /
                    (restoreTargetBoom - restoreStartBoom));

        public void Reset(float requestedBoom)
        {
            Phase = ShowcaseStaticCameraContactPhase.Clear;
            ContactEpisodeId = 0;
            RestoreStartCount = 0;
            CurrentBoom = Mathf.Max(0f, requestedBoom);
            clearEvidence = 0f;
            restoreElapsed = 0f;
            restoreStartBoom = CurrentBoom;
            restoreTargetBoom = CurrentBoom;
        }

        public float Advance(
            bool requestedPoseBlocked,
            float safeBoom,
            float requestedBoom,
            float deltaTime,
            float releaseEvidenceDuration,
            float returnDuration,
            bool forceImmediate = false)
        {
            requestedBoom = Mathf.Max(0f, requestedBoom);
            safeBoom = Mathf.Clamp(safeBoom, 0f, requestedBoom);
            deltaTime = Mathf.Max(0f, deltaTime);
            releaseEvidenceDuration = Mathf.Max(0f, releaseEvidenceDuration);
            returnDuration = Mathf.Max(0.0001f, returnDuration);

            if (forceImmediate)
            {
                Reset(requestedBoom);
                return CurrentBoom;
            }

            if (CurrentBoom > requestedBoom)
            {
                CurrentBoom = requestedBoom;
            }

            if (requestedPoseBlocked)
            {
                if (Phase == ShowcaseStaticCameraContactPhase.Clear)
                {
                    ContactEpisodeId++;
                }

                Phase = ShowcaseStaticCameraContactPhase.Contact;
                clearEvidence = 0f;
                restoreElapsed = 0f;
                CurrentBoom = Mathf.Min(CurrentBoom, safeBoom);
                return CurrentBoom;
            }

            if (Phase == ShowcaseStaticCameraContactPhase.Clear)
            {
                CurrentBoom = requestedBoom;
                clearEvidence = 0f;
                return CurrentBoom;
            }

            if (Phase == ShowcaseStaticCameraContactPhase.Contact)
            {
                clearEvidence += deltaTime;
                if (clearEvidence + 0.000001f < releaseEvidenceDuration)
                {
                    return CurrentBoom;
                }

                Phase = ShowcaseStaticCameraContactPhase.Restoring;
                restoreStartBoom = CurrentBoom;
                restoreTargetBoom = requestedBoom;
                restoreElapsed = 0f;
                RestoreStartCount++;
                return CurrentBoom;
            }

            restoreTargetBoom = requestedBoom;
            restoreElapsed = Mathf.Min(returnDuration, restoreElapsed + deltaTime);
            float progress = Mathf.Clamp01(restoreElapsed / returnDuration);
            float next = Mathf.Lerp(restoreStartBoom, restoreTargetBoom, progress);
            CurrentBoom = Mathf.Clamp(
                Mathf.Max(CurrentBoom, next),
                0f,
                restoreTargetBoom);
            if (progress >= 1f - 0.000001f)
            {
                CurrentBoom = restoreTargetBoom;
                Phase = ShowcaseStaticCameraContactPhase.Clear;
                clearEvidence = 0f;
                restoreElapsed = 0f;
            }
            return CurrentBoom;
        }
    }
}
