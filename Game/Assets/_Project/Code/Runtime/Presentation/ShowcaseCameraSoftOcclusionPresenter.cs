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

        private sealed class SoftPanelCache
        {
            public SoftPanelCache(
                int panelId,
                Renderer[] renderers)
            {
                PanelId = panelId;
                Renderers = renderers;
                ForceRenderingOffBeforeHide = new bool[renderers.Length];
            }

            public int PanelId { get; }
            public Renderer[] Renderers { get; }
            public bool[] ForceRenderingOffBeforeHide { get; }
            public int LastSeenGeneration { get; set; }
            public bool IsHidden { get; set; }
        }

        private readonly RaycastHit[] hits = new RaycastHit[HitCapacity];
        private List<SoftPanelCache> panelCaches = new();
        private Dictionary<int, SoftPanelCache> colliderCaches = new();
        private int softLayerMask;
        private int resolveGeneration;

        public int ActiveOccluderCount { get; private set; }
        public int CachedPanelCount => panelCaches.Count;
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
        /// Rebuilds the panel-to-renderer and collider-to-panel snapshots. The
        /// completed snapshots replace the previous cache together, so Resolve
        /// can remain traversal- and allocation-free between topology changes.
        /// Call this after adding or removing renderers/colliders at runtime.
        /// </summary>
        public void RefreshCache()
        {
            var panelsHiddenBeforeRefresh = new HashSet<int>();
            for (int index = 0; index < panelCaches.Count; index++)
            {
                SoftPanelCache existingCache = panelCaches[index];
                if (existingCache.IsHidden)
                {
                    panelsHiddenBeforeRefresh.Add(existingCache.PanelId);
                }
            }

            ShowcaseCameraSoftOccluderRoot[] roots =
                Object.FindObjectsByType<ShowcaseCameraSoftOccluderRoot>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.InstanceID);
            var rebuiltPanels = new List<SoftPanelCache>();
            var rebuiltColliders = new Dictionary<int, SoftPanelCache>();
            var panelsByTransform = new Dictionary<int, SoftPanelCache>();

            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                ShowcaseCameraSoftOccluderRoot root = roots[rootIndex];
                if (root == null)
                {
                    continue;
                }

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

                    int panelId = collider.transform.GetInstanceID();
                    if (!panelsByTransform.TryGetValue(
                            panelId,
                            out SoftPanelCache panelCache))
                    {
                        Renderer[] renderers = CollectPanelRenderers(
                            collider.transform,
                            root);
                        panelCache = new SoftPanelCache(panelId, renderers);
                        panelsByTransform.Add(panelId, panelCache);
                        rebuiltPanels.Add(panelCache);
                    }

                    rebuiltColliders[collider.GetInstanceID()] = panelCache;
                }
            }

            // The new snapshot is complete before the active snapshot changes.
            // Restore presentation state through the old cache, then swap both
            // collections as one logical operation.
            RestoreAllHiddenPanels();
            panelCaches = rebuiltPanels;
            colliderCaches = rebuiltColliders;
            resolveGeneration = 0;
            int rehiddenPanelCount = 0;
            for (int index = 0; index < panelCaches.Count; index++)
            {
                SoftPanelCache panelCache = panelCaches[index];
                if (panelsHiddenBeforeRefresh.Contains(panelCache.PanelId))
                {
                    HidePanel(panelCache);
                    rehiddenPanelCount++;
                }
            }
            ActiveOccluderCount = rehiddenPanelCount;
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
            int activePanelCount = 0;
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
                            out SoftPanelCache panelCache))
                    {
                        // Topology changes are explicit cache boundaries. Do
                        // not turn an unexpected collider into a per-frame
                        // hierarchy scan; its owner must request a cache rebuild.
                        continue;
                    }

                    if (panelCache.LastSeenGeneration == generation)
                    {
                        continue;
                    }

                    panelCache.LastSeenGeneration = generation;
                    activePanelCount++;
                }
            }

            for (int index = 0; index < panelCaches.Count; index++)
            {
                SoftPanelCache panelCache = panelCaches[index];
                if (panelCache.LastSeenGeneration == generation)
                {
                    HidePanel(panelCache);
                }
                else
                {
                    RestorePanel(panelCache);
                }
            }

            ActiveOccluderCount = activePanelCount;
        }

        public void ResetVisibility()
        {
            RestoreAllHiddenPanels();
            ActiveOccluderCount = 0;
        }

        private static Renderer[] CollectPanelRenderers(
            Transform panelTransform,
            ShowcaseCameraSoftOccluderRoot root)
        {
            Renderer[] candidates =
                panelTransform.GetComponentsInChildren<Renderer>(true);
            var ownedRenderers = new List<Renderer>(candidates.Length);
            for (int index = 0; index < candidates.Length; index++)
            {
                Renderer renderer = candidates[index];
                if (renderer != null &&
                    renderer.GetComponentInParent<
                        ShowcaseCameraSoftOccluderRoot>() == root &&
                    FindNearestColliderTransform(renderer.transform, root) ==
                        panelTransform)
                {
                    ownedRenderers.Add(renderer);
                }
            }

            return ownedRenderers.ToArray();
        }

        private static Transform FindNearestColliderTransform(
            Transform rendererTransform,
            ShowcaseCameraSoftOccluderRoot root)
        {
            Transform current = rendererTransform;
            Transform rootTransform = root.transform;
            while (current != null)
            {
                if (current.GetComponent<Collider>() != null)
                {
                    return current;
                }
                if (current == rootTransform)
                {
                    break;
                }
                current = current.parent;
            }

            return null;
        }

        private static void HidePanel(SoftPanelCache panelCache)
        {
            Renderer[] renderers = panelCache.Renderers;
            bool[] originalStates = panelCache.ForceRenderingOffBeforeHide;
            if (!panelCache.IsHidden)
            {
                for (int index = 0; index < renderers.Length; index++)
                {
                    Renderer renderer = renderers[index];
                    if (renderer != null)
                    {
                        originalStates[index] = renderer.forceRenderingOff;
                    }
                }
                panelCache.IsHidden = true;
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

        private static void RestorePanel(SoftPanelCache panelCache)
        {
            if (!panelCache.IsHidden)
            {
                return;
            }

            Renderer[] renderers = panelCache.Renderers;
            bool[] originalStates = panelCache.ForceRenderingOffBeforeHide;
            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer != null)
                {
                    renderer.forceRenderingOff = originalStates[index];
                }
            }
            panelCache.IsHidden = false;
        }

        private void RestoreAllHiddenPanels()
        {
            for (int index = 0; index < panelCaches.Count; index++)
            {
                RestorePanel(panelCaches[index]);
            }
        }

        private int NextGeneration()
        {
            if (resolveGeneration == int.MaxValue)
            {
                for (int index = 0; index < panelCaches.Count; index++)
                {
                    panelCaches[index].LastSeenGeneration = 0;
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
