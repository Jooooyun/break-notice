using System;
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

                // A solo audition instance contains three inactive sibling layers.
                // Unity can skip the first scheduled burst for the one active child
                // when that prefab hierarchy enters Play Mode. Emit only that solo
                // layer explicitly; composite timing and production candidates stay
                // untouched.
                Transform parent = system.transform.parent;
                if (parent != null &&
                    parent.name.StartsWith("SOLO_0", StringComparison.Ordinal))
                {
                    system.Emit(1);
                }
            }

            LastStartedSystemCount = systems.Length;
            return LastStartedSystemCount;
        }
    }
}
