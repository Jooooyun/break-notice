using System;
using System.Linq;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.Contest.Editor
{
    public static class ContestCombatVfxAuthoring
    {
        public const string RootName = "SHOWCASE_COMBAT_VFX";

        private const string PrefabFolder =
            "Assets/_Project/Art/VFX/Showcase/HitProduction_v005/Prefabs";

        [MenuItem("BREAK NOTICE/Contest/Apply Combat And Health VFX To S7")]
        public static void PrepareS7CombatVfx()
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(
                ContestShowcaseSceneAuthoring.ShowcaseS7ScenePath);
            if (sceneAsset == null)
            {
                throw new InvalidOperationException(
                    "Missing playable S7 Scene: " +
                    ContestShowcaseSceneAuthoring.ShowcaseS7ScenePath);
            }

            Scene scene = EditorSceneManager.OpenScene(
                ContestShowcaseSceneAuthoring.ShowcaseS7ScenePath,
                OpenSceneMode.Single);
            ApplyToScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(
                    scene,
                    ContestShowcaseSceneAuthoring.ShowcaseS7ScenePath))
            {
                throw new InvalidOperationException(
                    "Could not save the S7 combat VFX wiring.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log(
                "S7 combat VFX prepared: accepted-contact Light/Medium/Heavy " +
                "A/B/C impacts and actual player/Runner health displays.");
        }

        public static void ApplyToScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new ArgumentException(
                    "A valid loaded Scene is required.",
                    nameof(scene));
            }

            GameObject oldRoot = scene.GetRootGameObjects().SingleOrDefault(root =>
                string.Equals(root.name, RootName, StringComparison.Ordinal));
            if (oldRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(oldRoot);
            }

            ShowcaseRunnerPressureController pressure = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<
                    ShowcaseRunnerPressureController>(true))
                .Single();
            Camera outputCamera = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Camera>(true))
                .Single(camera => string.Equals(
                    camera.gameObject.name,
                    "OutputCamera",
                    StringComparison.Ordinal));
            EnableCharacterHitEmission(pressure);
            CinemachineCamera virtualCamera = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<
                    CinemachineCamera>(true))
                .Single();

            var root = new GameObject(RootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            ShowcaseCombatHitVfxPresenter hitVfx =
                root.AddComponent<ShowcaseCombatHitVfxPresenter>();
            hitVfx.Configure(
                pressure,
                LoadTier("Light"),
                LoadTier("Medium"),
                LoadTier("Heavy"));

            ShowcaseHealthVfxPresenter healthVfx =
                root.AddComponent<ShowcaseHealthVfxPresenter>();
            healthVfx.Configure(pressure, outputCamera);

            foreach (CinemachineImpulseListener staleListener in
                     virtualCamera.GetComponents<CinemachineImpulseListener>())
            {
                UnityEngine.Object.DestroyImmediate(staleListener);
            }

            ShowcaseHitRotationImpulseExtension cameraImpulse =
                virtualCamera.GetComponent<ShowcaseHitRotationImpulseExtension>() ??
                virtualCamera.gameObject.AddComponent<
                    ShowcaseHitRotationImpulseExtension>();
            ShowcaseCameraConstraintSolver constraintSolver =
                virtualCamera.GetComponent<ShowcaseCameraConstraintSolver>();
            if (constraintSolver == null)
            {
                throw new InvalidOperationException(
                    "The S7 Cinemachine camera is missing its constraint solver.");
            }
            constraintSolver.ConfigureHitFeedback(cameraImpulse);
            ShowcaseHitFeedbackController hitFeedback =
                root.AddComponent<ShowcaseHitFeedbackController>();
            hitFeedback.Configure(
                pressure,
                outputCamera,
                cameraImpulse);

            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(hitVfx);
            EditorUtility.SetDirty(healthVfx);
            EditorUtility.SetDirty(hitFeedback);
            EditorUtility.SetDirty(cameraImpulse);
            EditorUtility.SetDirty(constraintSolver);
            EditorUtility.SetDirty(virtualCamera.gameObject);
        }

        private static GameObject[] LoadTier(string tier)
        {
            string[] variants = { "A", "B", "C" };
            return variants.Select(variant => RequirePrefab(
                $"{PrefabFolder}/PF_VFX_Showcase_Hit{tier}_{variant}_" +
                "REVIEW_ONLY_v005.prefab")).ToArray();
        }

        private static GameObject RequirePrefab(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                throw new InvalidOperationException(
                    $"Missing selected showcase hit VFX prefab: {path}.");
            }
            return prefab;
        }

        private static void EnableCharacterHitEmission(
            ShowcaseRunnerPressureController pressure)
        {
            Transform[] roots = new[] { pressure.PlayerTransform }
                .Concat(pressure.Runners
                    .Where(runner => runner != null)
                    .Select(runner => runner.transform))
                .Where(root => root != null)
                .ToArray();
            foreach (Material material in roots
                         .SelectMany(root => root.GetComponentsInChildren<Renderer>(true))
                         .SelectMany(renderer => renderer.sharedMaterials)
                         .Where(material => material != null)
                         .Distinct())
            {
                DesignerACharacterIntegrationAuthoring
                    .EnsureCharacterHitEmission(material);
            }
        }
    }
}
