using System.Collections.Generic;
using UnityEngine;

namespace BreakNotice.Presentation
{
    /// <summary>
    /// Presentation-only cutaway for interior camera occluders. It never
    /// changes GameObject activity, layers, or collider state; gameplay
    /// collision therefore remains continuous while the obstructing renderer
    /// is omitted from the current camera view.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShowcaseCameraSoftOcclusionPresenter : MonoBehaviour
    {
        private const int HitCapacity = 64;
        private const float ProbeRadius = 0.04f;

        private sealed class SoftRootCache
        {
            public SoftRootCache(
                ShowcaseCameraSoftOccluderRoot root,
                Renderer[] renderers)
            {
                Root = root;
                Renderers = renderers;
                ForceRenderingOffBeforeHide = new bool[renderers.Length];
            }

            public ShowcaseCameraSoftOccluderRoot Root { get; }
            public Renderer[] Renderers { get; }
            public bool[] ForceRenderingOffBeforeHide { get; }
            public int LastSeenGeneration { get; set; }
            public bool IsHidden { get; set; }
        }

        private readonly RaycastHit[] hits = new RaycastHit[HitCapacity];
        private List<SoftRootCache> rootCaches = new();
        private Dictionary<int, SoftRootCache> colliderCaches = new();
        private int softLayerMask;
        private int resolveGeneration;

        public int ActiveOccluderCount { get; private set; }
        public int CachedRootCount => rootCaches.Count;
        public int CachedColliderCount => colliderCaches.Count;
        public int CacheRefreshCount { get; private set; }

        private void Awake()
        {
            ResolveLayerMask();
            RefreshCache();
        }

        private void OnValidate()
        {
            ResolveLayerMask();
        }

        private void OnDisable()
        {
            ResetVisibility();
        }

        /// <summary>
        /// Rebuilds the root-to-renderer and collider-to-root snapshots. The
        /// completed snapshots replace the previous cache together, so Resolve
        /// can remain traversal- and allocation-free between topology changes.
        /// Call this after adding or removing renderers/colliders at runtime.
        /// </summary>
        public void RefreshCache()
        {
            var rootsHiddenBeforeRefresh = new HashSet<int>();
            for (int index = 0; index < rootCaches.Count; index++)
            {
                SoftRootCache existingCache = rootCaches[index];
                if (existingCache.IsHidden && existingCache.Root != null)
                {
                    rootsHiddenBeforeRefresh.Add(
                        existingCache.Root.GetInstanceID());
                }
            }

            ShowcaseCameraSoftOccluderRoot[] roots =
                Object.FindObjectsByType<ShowcaseCameraSoftOccluderRoot>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.InstanceID);
            var rebuiltRoots = new List<SoftRootCache>(roots.Length);
            var rebuiltColliders = new Dictionary<int, SoftRootCache>();

            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                ShowcaseCameraSoftOccluderRoot root = roots[rootIndex];
                if (root == null)
                {
                    continue;
                }

                Renderer[] renderers = CollectOwnedRenderers(root);
                var rootCache = new SoftRootCache(root, renderers);
                rebuiltRoots.Add(rootCache);

                Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
                for (int colliderIndex = 0;
                     colliderIndex < colliders.Length;
                     colliderIndex++)
                {
                    Collider collider = colliders[colliderIndex];
                    if (collider == null ||
                        collider.GetComponentInParent<
                            ShowcaseCameraSoftOccluderRoot>() != root)
                    {
                        continue;
                    }

                    rebuiltColliders[collider.GetInstanceID()] = rootCache;
                }
            }

            // The new snapshot is complete before the active snapshot changes.
            // Restore presentation state through the old cache, then swap both
            // collections as one logical operation.
            RestoreAllHiddenRoots();
            rootCaches = rebuiltRoots;
            colliderCaches = rebuiltColliders;
            resolveGeneration = 0;
            int rehiddenRootCount = 0;
            for (int index = 0; index < rootCaches.Count; index++)
            {
                SoftRootCache rootCache = rootCaches[index];
                if (rootCache.Root != null &&
                    rootsHiddenBeforeRefresh.Contains(
                        rootCache.Root.GetInstanceID()))
                {
                    HideRoot(rootCache);
                    rehiddenRootCount++;
                }
            }
            ActiveOccluderCount = rehiddenRootCount;
            CacheRefreshCount++;
        }

        public void Resolve(Vector3 subjectPosition, Vector3 cameraPosition)
        {
            if (softLayerMask == 0)
            {
                ResetVisibility();
                return;
            }

            int generation = NextGeneration();
            int activeRootCount = 0;
            Vector3 delta = cameraPosition - subjectPosition;
            float distance = delta.magnitude;
            if (distance > 0.0001f)
            {
                int count = Physics.SphereCastNonAlloc(
                    subjectPosition,
                    ProbeRadius,
                    delta / distance,
                    hits,
                    distance,
                    softLayerMask,
                    QueryTriggerInteraction.Ignore);
                int safeCount = Mathf.Min(count, hits.Length);
                for (int index = 0; index < safeCount; index++)
                {
                    Collider collider = hits[index].collider;
                    if (collider == null)
                    {
                        continue;
                    }

                    if (!colliderCaches.TryGetValue(
                            collider.GetInstanceID(),
                            out SoftRootCache rootCache))
                    {
                        // Topology changes are explicit cache boundaries. Do
                        // not turn an unexpected collider into a per-frame
                        // hierarchy scan; its owner must request a cache rebuild.
                        continue;
                    }

                    if (rootCache.LastSeenGeneration == generation)
                    {
                        continue;
                    }

                    rootCache.LastSeenGeneration = generation;
                    activeRootCount++;
                }
            }

            for (int index = 0; index < rootCaches.Count; index++)
            {
                SoftRootCache rootCache = rootCaches[index];
                if (rootCache.LastSeenGeneration == generation)
                {
                    HideRoot(rootCache);
                }
                else
                {
                    RestoreRoot(rootCache);
                }
            }

            ActiveOccluderCount = activeRootCount;
        }

        public void ResetVisibility()
        {
            RestoreAllHiddenRoots();
            ActiveOccluderCount = 0;
        }

        private static Renderer[] CollectOwnedRenderers(
            ShowcaseCameraSoftOccluderRoot root)
        {
            Renderer[] candidates = root.GetComponentsInChildren<Renderer>(true);
            var ownedRenderers = new List<Renderer>(candidates.Length);
            for (int index = 0; index < candidates.Length; index++)
            {
                Renderer renderer = candidates[index];
                if (renderer != null &&
                    renderer.GetComponentInParent<
                        ShowcaseCameraSoftOccluderRoot>() == root)
                {
                    ownedRenderers.Add(renderer);
                }
            }

            return ownedRenderers.ToArray();
        }

        private static void HideRoot(SoftRootCache rootCache)
        {
            Renderer[] renderers = rootCache.Renderers;
            bool[] originalStates = rootCache.ForceRenderingOffBeforeHide;
            if (!rootCache.IsHidden)
            {
                for (int index = 0; index < renderers.Length; index++)
                {
                    Renderer renderer = renderers[index];
                    if (renderer != null)
                    {
                        originalStates[index] = renderer.forceRenderingOff;
                    }
                }
                rootCache.IsHidden = true;
            }

            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer != null)
                {
                    renderer.forceRenderingOff = true;
                }
            }
        }

        private static void RestoreRoot(SoftRootCache rootCache)
        {
            if (!rootCache.IsHidden)
            {
                return;
            }

            Renderer[] renderers = rootCache.Renderers;
            bool[] originalStates = rootCache.ForceRenderingOffBeforeHide;
            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer != null)
                {
                    renderer.forceRenderingOff = originalStates[index];
                }
            }
            rootCache.IsHidden = false;
        }

        private void RestoreAllHiddenRoots()
        {
            for (int index = 0; index < rootCaches.Count; index++)
            {
                RestoreRoot(rootCaches[index]);
            }
        }

        private int NextGeneration()
        {
            if (resolveGeneration == int.MaxValue)
            {
                for (int index = 0; index < rootCaches.Count; index++)
                {
                    rootCaches[index].LastSeenGeneration = 0;
                }
                resolveGeneration = 0;
            }

            resolveGeneration++;
            return resolveGeneration;
        }

        private void ResolveLayerMask()
        {
            int layer = LayerMask.NameToLayer("CameraSoftOccluder");
            softLayerMask = layer >= 0 ? 1 << layer : 0;
        }
    }
}
