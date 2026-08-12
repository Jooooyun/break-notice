using System;
using System.Linq;
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
        private const float ParticlePreRollSeconds = 0.06f;

        [SerializeField, Min(2f)]
        private float pageDurationSeconds = 6f;

        [SerializeField, Min(0.4f)]
        private float ownerSelectedReplayIntervalSeconds = 1.2f;

        private Camera reviewCamera;
        private Transform[] pages = Array.Empty<Transform>();
        private float nextPageTime;
        private float nextReplayTime;

        public int LastStartedSystemCount { get; private set; }
        public int LastPageStartedSystemCount { get; private set; }
        public int PageCount => pages.Length;
        public int CurrentPageIndex { get; private set; }

        private void Start()
        {
            reviewCamera = GetComponentInChildren<Camera>(true);
            pages = transform.Cast<Transform>()
                .Where(child => child.name.StartsWith("COMPLETE_PAGE_", StringComparison.Ordinal))
                .OrderBy(child => child.name, StringComparer.Ordinal)
                .ToArray();
            if (reviewCamera == null || pages.Length == 0)
            {
                Debug.LogError("Showcase VFX complete review camera/pages are unavailable.", this);
                enabled = false;
                return;
            }
            RestartAllNow();
            CurrentPageIndex = 0;
            Transform firstPage = pages[CurrentPageIndex];
            ApplyPageCamera(firstPage);
            LastPageStartedSystemCount =
                firstPage.GetComponentsInChildren<ParticleSystem>(false).Length;
            nextPageTime = Time.unscaledTime + pageDurationSeconds;
            nextReplayTime = Time.unscaledTime + ownerSelectedReplayIntervalSeconds;
        }

        private void Update()
        {
            if (pages.Length == 0)
            {
                return;
            }

            if (Time.unscaledTime >= nextPageTime)
            {
                ShowPageNow((CurrentPageIndex + 1) % pages.Length);
                return;
            }

            if (CurrentPageIndex == 0 && Time.unscaledTime >= nextReplayTime)
            {
                ReplayCurrentPageNow();
            }
        }

        public int RestartAllNow()
        {
            ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>(false);
            foreach (ParticleSystem system in systems)
            {
                system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                system.Simulate(ParticlePreRollSeconds, false, true, false);
                system.Play(false);
            }

            LastStartedSystemCount = systems.Length;
            return LastStartedSystemCount;
        }

        public int ShowPageNow(int index)
        {
            if (pages.Length == 0)
            {
                return 0;
            }
            CurrentPageIndex = ((index % pages.Length) + pages.Length) % pages.Length;
            Transform page = pages[CurrentPageIndex];
            ApplyPageCamera(page);
            RestartPageSystems(page);
            nextPageTime = Time.unscaledTime + pageDurationSeconds;
            nextReplayTime = CurrentPageIndex == 0
                ? Time.unscaledTime + ownerSelectedReplayIntervalSeconds
                : float.PositiveInfinity;
            return LastPageStartedSystemCount;
        }

        private void ApplyPageCamera(Transform page)
        {
            Transform anchor = page.Find("CAMERA_ANCHOR");
            Transform target = page.Find("CAMERA_TARGET");
            if (anchor == null || target == null)
            {
                throw new InvalidOperationException(
                    $"Review page camera contract is incomplete: {page.name}");
            }
            reviewCamera.transform.SetPositionAndRotation(
                anchor.position,
                Quaternion.LookRotation(target.position - anchor.position, Vector3.up));
            reviewCamera.transform.localScale = Vector3.one;
            reviewCamera.fieldOfView = page.name.Contains(
                "GAMEPLAY_CAMERA",
                StringComparison.Ordinal) ? 48f : 40f;
        }

        public int ReplayCurrentPageNow()
        {
            if (pages.Length == 0)
            {
                return 0;
            }

            RestartPageSystems(pages[CurrentPageIndex]);
            nextReplayTime = CurrentPageIndex == 0
                ? Time.unscaledTime + ownerSelectedReplayIntervalSeconds
                : float.PositiveInfinity;
            return LastPageStartedSystemCount;
        }

        private void RestartPageSystems(Transform page)
        {
            ParticleSystem[] pageSystems = page.GetComponentsInChildren<ParticleSystem>(false);
            foreach (ParticleSystem system in pageSystems)
            {
                system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                system.Simulate(ParticlePreRollSeconds, false, true, false);
                system.Play(false);
            }
            LastPageStartedSystemCount = pageSystems.Length;
        }
    }
}
