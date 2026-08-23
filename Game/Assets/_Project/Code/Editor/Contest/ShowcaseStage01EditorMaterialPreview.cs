using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace BreakNotice.Contest.Editor
{
    /// <summary>
    /// Non-destructive Scene-view bridge for the pinned Stage01 art package.
    ///
    /// Stage01 was authored with URP/Lit materials while the proven v007 project
    /// still uses the Built-in render pipeline. In Play Mode the bootstrap already
    /// creates temporary Standard-material equivalents. This editor bridge applies
    /// the same conversion only to loaded Stage01 renderer instances so the map is
    /// readable in Scene View without switching the whole project to URP or editing
    /// the external package material assets.
    ///
    /// Original shared-material arrays are restored before Play Mode, assembly
    /// reload, scene close, editor quit, and before saving the Stage01 scene.
    /// </summary>
    [InitializeOnLoad]
    public static class ShowcaseStage01EditorMaterialPreview
    {
        private sealed class RendererRecord
        {
            public Renderer Renderer;
            public Material[] OriginalMaterials;
        }

        private static readonly Dictionary<int, RendererRecord> RendererRecords =
            new Dictionary<int, RendererRecord>();

        private static readonly Dictionary<Material, Material> PreviewMaterials =
            new Dictionary<Material, Material>();

        private static bool previewEnabled = true;
        private static bool refreshQueued;

        static ShowcaseStage01EditorMaterialPreview()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            EditorSceneManager.sceneOpened += HandleSceneOpened;
            EditorSceneManager.sceneClosing += HandleSceneClosing;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
            EditorApplication.projectChanged += QueueRefresh;
            EditorApplication.quitting += RestoreAll;
            AssemblyReloadEvents.beforeAssemblyReload += RestoreAll;
            QueueRefresh();
        }

        [MenuItem("BREAK NOTICE/Stage01/Refresh Built-in Material Preview")]
        public static void RefreshLoadedStageScenes()
        {
            refreshQueued = false;
            previewEnabled = true;

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (GraphicsSettings.currentRenderPipeline != null)
            {
                RestoreAll();
                return;
            }

            int changedRenderers = 0;
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (!IsStage01Scene(scene))
                {
                    continue;
                }

                changedRenderers += ApplyPreviewToScene(scene);
            }

            if (changedRenderers > 0)
            {
                Debug.Log(
                    "BREAK NOTICE Stage01 editor material preview active. " +
                    $"renderersConverted={changedRenderers}; " +
                    $"uniqueMaterials={PreviewMaterials.Count}. " +
                    "This is an in-memory preview only; source package materials remain unchanged.");
            }

            SceneView.RepaintAll();
        }

        [MenuItem("BREAK NOTICE/Stage01/Disable Built-in Material Preview")]
        public static void DisablePreview()
        {
            previewEnabled = false;
            RestoreAll();
            SceneView.RepaintAll();
        }

        /// <summary>
        /// Batch-only focused validation entry used to prove that the directly
        /// opened Stage01 scene can be previewed in the Built-in project without
        /// unsupported URP materials remaining on its renderers.
        /// </summary>
        public static void ValidatePreviewFromCommandLine()
        {
            try
            {
                Scene scene = EditorSceneManager.OpenScene(
                    ShowcaseStage01Bootstrap.EnvironmentScenePath,
                    OpenSceneMode.Single);

                int changedRenderers = ApplyPreviewToScene(scene);
                int urpMaterialsRemaining = 0;
                int unsupportedMaterials = 0;
                Renderer[] renderers = GetSceneRenderers(scene);
                for (int rendererIndex = 0;
                     rendererIndex < renderers.Length;
                     rendererIndex++)
                {
                    Renderer renderer = renderers[rendererIndex];
                    if (renderer == null)
                    {
                        continue;
                    }

                    Material[] materials = renderer.sharedMaterials;
                    for (int materialIndex = 0;
                         materialIndex < materials.Length;
                         materialIndex++)
                    {
                        Material material = materials[materialIndex];
                        if (material == null || material.shader == null)
                        {
                            continue;
                        }

                        if (material.shader.name.StartsWith(
                                "Universal Render Pipeline/",
                                StringComparison.Ordinal))
                        {
                            urpMaterialsRemaining++;
                        }

                        if (!material.shader.isSupported)
                        {
                            unsupportedMaterials++;
                        }
                    }
                }

                if (GraphicsSettings.currentRenderPipeline == null &&
                    (changedRenderers <= 0 ||
                     urpMaterialsRemaining != 0 ||
                     unsupportedMaterials != 0))
                {
                    throw new InvalidOperationException(
                        "Stage01 Built-in editor preview validation failed: " +
                        $"changedRenderers={changedRenderers}, " +
                        $"urpMaterialsRemaining={urpMaterialsRemaining}, " +
                        $"unsupportedMaterials={unsupportedMaterials}.");
                }

                Debug.Log(
                    "BREAK NOTICE Stage01 editor material preview validation passed. " +
                    $"changedRenderers={changedRenderers}; " +
                    $"uniqueMaterials={PreviewMaterials.Count}; " +
                    $"urpMaterialsRemaining={urpMaterialsRemaining}; " +
                    $"unsupportedMaterials={unsupportedMaterials}.");
                RestoreAll();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                RestoreAll();
                EditorApplication.Exit(3);
            }
        }

        internal static bool RestoreForSave(string[] paths)
        {
            if (paths == null || paths.Length == 0 || RendererRecords.Count == 0)
            {
                return false;
            }

            bool savingStage = false;
            for (int index = 0; index < paths.Length; index++)
            {
                string path = paths[index];
                if (string.Equals(
                        path,
                        ShowcaseStage01Bootstrap.EnvironmentScenePath,
                        StringComparison.OrdinalIgnoreCase))
                {
                    savingStage = true;
                    break;
                }
            }

            if (!savingStage)
            {
                return false;
            }

            RestoreAll();
            QueueRefresh();
            return true;
        }

        private static void HandleSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (IsStage01Scene(scene))
            {
                QueueRefresh();
            }
        }

        private static void HandleSceneClosing(Scene scene, bool removingScene)
        {
            if (IsStage01Scene(scene))
            {
                RestoreScene(scene);
            }
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                RestoreAll();
                return;
            }

            if (state == PlayModeStateChange.EnteredEditMode && previewEnabled)
            {
                QueueRefresh();
            }
        }

        private static void QueueRefresh()
        {
            if (Application.isBatchMode || !previewEnabled || refreshQueued)
            {
                return;
            }

            refreshQueued = true;
            EditorApplication.delayCall += RefreshLoadedStageScenes;
        }

        private static int ApplyPreviewToScene(Scene scene)
        {
            if (!scene.IsValid() ||
                !scene.isLoaded ||
                GraphicsSettings.currentRenderPipeline != null)
            {
                return 0;
            }

            int changedRenderers = 0;
            Renderer[] renderers = GetSceneRenderers(scene);
            for (int rendererIndex = 0;
                 rendererIndex < renderers.Length;
                 rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                if (renderer == null)
                {
                    continue;
                }

                int instanceId = renderer.GetInstanceID();
                if (RendererRecords.ContainsKey(instanceId))
                {
                    continue;
                }

                Material[] originalMaterials = renderer.sharedMaterials;
                Material[] previewMaterials =
                    (Material[])originalMaterials.Clone();
                bool changed = false;
                for (int materialIndex = 0;
                     materialIndex < previewMaterials.Length;
                     materialIndex++)
                {
                    Material source = previewMaterials[materialIndex];
                    if (!IsUrpMaterial(source))
                    {
                        continue;
                    }

                    if (!PreviewMaterials.TryGetValue(
                            source,
                            out Material converted) ||
                        converted == null)
                    {
                        converted =
                            ShowcaseStage01Bootstrap.CreateBuiltInCompatibleMaterial(
                                source);
                        converted.name = source.name + "_Stage01EditorPreview";
                        converted.hideFlags = HideFlags.HideAndDontSave;
                        PreviewMaterials[source] = converted;
                    }

                    previewMaterials[materialIndex] = converted;
                    changed = true;
                }

                if (!changed)
                {
                    continue;
                }

                RendererRecords[instanceId] = new RendererRecord
                {
                    Renderer = renderer,
                    OriginalMaterials = originalMaterials
                };
                renderer.sharedMaterials = previewMaterials;
                changedRenderers++;
            }

            return changedRenderers;
        }

        private static void RestoreScene(Scene scene)
        {
            if (!scene.IsValid())
            {
                return;
            }

            var removeIds = new List<int>();
            foreach (KeyValuePair<int, RendererRecord> pair in RendererRecords)
            {
                RendererRecord record = pair.Value;
                Renderer renderer = record.Renderer;
                if (renderer == null || renderer.gameObject.scene != scene)
                {
                    continue;
                }

                renderer.sharedMaterials = record.OriginalMaterials;
                removeIds.Add(pair.Key);
            }

            for (int index = 0; index < removeIds.Count; index++)
            {
                RendererRecords.Remove(removeIds[index]);
            }

            if (RendererRecords.Count == 0)
            {
                DestroyPreviewMaterials();
            }
        }

        private static void RestoreAll()
        {
            foreach (RendererRecord record in RendererRecords.Values)
            {
                if (record.Renderer != null)
                {
                    record.Renderer.sharedMaterials = record.OriginalMaterials;
                }
            }

            RendererRecords.Clear();
            DestroyPreviewMaterials();
        }

        private static void DestroyPreviewMaterials()
        {
            foreach (Material material in PreviewMaterials.Values)
            {
                if (material != null)
                {
                    UnityEngine.Object.DestroyImmediate(material);
                }
            }

            PreviewMaterials.Clear();
        }

        private static bool IsStage01Scene(Scene scene)
        {
            return scene.IsValid() &&
                scene.isLoaded &&
                (string.Equals(
                     scene.path,
                     ShowcaseStage01Bootstrap.EnvironmentScenePath,
                     StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(
                     scene.name,
                     ShowcaseStage01Bootstrap.EnvironmentSceneName,
                     StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsUrpMaterial(Material material)
        {
            return material != null &&
                material.shader != null &&
                material.shader.name.StartsWith(
                    "Universal Render Pipeline/",
                    StringComparison.Ordinal);
        }

        private static Renderer[] GetSceneRenderers(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return Array.Empty<Renderer>();
            }

            GameObject[] roots = scene.GetRootGameObjects();
            int total = 0;
            var groups = new Renderer[roots.Length][];
            for (int index = 0; index < roots.Length; index++)
            {
                Renderer[] renderers =
                    roots[index].GetComponentsInChildren<Renderer>(true);
                groups[index] = renderers;
                total += renderers.Length;
            }

            var result = new Renderer[total];
            int writeIndex = 0;
            for (int index = 0; index < groups.Length; index++)
            {
                Renderer[] renderers = groups[index];
                Array.Copy(renderers, 0, result, writeIndex, renderers.Length);
                writeIndex += renderers.Length;
            }

            return result;
        }
    }

    internal sealed class ShowcaseStage01PreviewSaveGuard : AssetModificationProcessor
    {
        private static string[] OnWillSaveAssets(string[] paths)
        {
            ShowcaseStage01EditorMaterialPreview.RestoreForSave(paths);
            return paths;
        }
    }
}
