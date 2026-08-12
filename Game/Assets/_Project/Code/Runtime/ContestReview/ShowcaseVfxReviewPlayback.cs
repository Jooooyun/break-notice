using UnityEngine;

namespace BreakNotice.ContestReview
{
    /// <summary>
    /// Review-only playback bootstrap for the isolated Showcase VFX scene.
    /// It has no gameplay assembly, state, input, or hook dependency.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10000)]
    public sealed class ShowcaseVfxReviewPlayback : MonoBehaviour
    {
        public int LastStartedSystemCount { get; private set; }

        private void Start()
        {
            RestartAllNow();
        }

        public int RestartAllNow()
        {
            ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>(false);
            foreach (ParticleSystem system in systems)
            {
                system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                system.Play(false);
            }

            LastStartedSystemCount = systems.Length;
            return LastStartedSystemCount;
        }
    }
}
