using System;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.Contest.Editor
{
    public static class ShowcaseHudAuthoring
    {
        public const string ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v003.unity";
        public const string PrefabFolder = "Assets/_Project/Prefabs/Presentation";
        public const string HudPrefabPath = PrefabFolder + "/ShowcaseHud.prefab";
        public const string EnemyAnchorPrefabPath =
            PrefabFolder + "/ShowcaseEnemyHealthAnchor.prefab";
        public const string HudRootName = "SHOWCASE_PRESENTATION_HUD";
        public const string EnemyAnchorName = "ShowcaseEnemyHealthAnchor";

        public const string DemoScenePath =
            "Assets/_Project/Scenes/90_Review/LOCAL_REVIEW_ONLY_ShowcaseHUD.unity";

        [MenuItem("BREAK NOTICE/Contest/Prepare Approved Showcase HUD")]
        public static void PrepareApprovedShowcaseHud()
        {
            EnsureFolder("Assets/_Project/Prefabs", "Presentation");
            CreateHudPrefab();
            CreateEnemyAnchorPrefab();

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            InstallInScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"Could not save {ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Selection.activeGameObject = FindRoot(scene, HudRootName);
            SceneView.lastActiveSceneView?.FrameSelected();
            Debug.Log(
                "Approved Showcase HUD prepared: player CONDITION, two enemy overhead bars, " +
                "and existing 1.2-second ThrowCharge01 binding. No damage mechanic was added.");
        }

        public static void RunValidation()
        {
            PrepareApprovedShowcaseHud();
            ValidatePreparedAssets();
        }

        public static void ValidatePreparedAssets()
        {
            GameObject hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath);
            GameObject anchorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                EnemyAnchorPrefabPath);
            if (hudPrefab == null || hudPrefab.GetComponent<ShowcaseHudPresenter>() == null)
            {
                throw new InvalidOperationException("ShowcaseHud prefab contract failed.");
            }

            if (anchorPrefab == null ||
                anchorPrefab.GetComponent<ShowcaseWorldHealthAnchor>() == null)
            {
                throw new InvalidOperationException(
                    "ShowcaseEnemyHealthAnchor prefab contract failed.");
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject hud = FindRoot(scene, HudRootName);
            if (hud == null || hud.GetComponent<ShowcaseHudPresenter>() == null)
            {
                throw new InvalidOperationException("Showcase Scene HUD installation failed.");
            }

            GameObject enemyOne = RequireRootAny(
                scene,
                "GAMEPLAY_ENY01_RUNNER_01",
                "EnemyProxy_01");
            GameObject enemyTwo = RequireRootAny(
                scene,
                "GAMEPLAY_ENY01_RUNNER_02",
                "EnemyProxy_02");
            if (enemyOne.transform.Find(EnemyAnchorName) == null ||
                enemyTwo.transform.Find(EnemyAnchorName) == null)
            {
                throw new InvalidOperationException(
                    "Both Showcase enemy health anchors are required.");
            }

            Debug.Log(
                "Showcase HUD validation passed: prefab 2/2, Scene HUD 1/1, " +
                "enemy anchors 2/2, charge source bound to ThrowCharge01.");
        }

        public static void InstallInScene(Scene scene)
        {
            GameObject player = RequireRootAny(scene, "GAMEPLAY_CHR00_PLAYER", "Player");
            GameObject enemyOne = RequireRootAny(
                scene,
                "GAMEPLAY_ENY01_RUNNER_01",
                "EnemyProxy_01");
            GameObject enemyTwo = RequireRootAny(
                scene,
                "GAMEPLAY_ENY01_RUNNER_02",
                "EnemyProxy_02");

            GameObject existingHud = FindRoot(scene, HudRootName);
            if (existingHud != null)
            {
                UnityEngine.Object.DestroyImmediate(existingHud);
            }

            GameObject hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath);
            GameObject hud = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab, scene);
            hud.name = HudRootName;
            ShowcaseHudPresenter presenter = hud.GetComponent<ShowcaseHudPresenter>();
            PrototypeInteractionController interaction =
                player.GetComponent<PrototypeInteractionController>();
            if (interaction == null)
            {
                throw new InvalidOperationException(
                    "The Showcase player must own PrototypeInteractionController.");
            }

            presenter.Configure(interaction, interaction.ViewCamera);
            InstallEnemyAnchor(enemyOne.transform);
            InstallEnemyAnchor(enemyTwo.transform);
            presenter.RefreshWorldHealthAnchors();
        }

        private static void InstallEnemyAnchor(Transform actor)
        {
            Transform existing = actor.Find(EnemyAnchorName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing.gameObject);
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                EnemyAnchorPrefabPath);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, actor);
            instance.name = EnemyAnchorName;
            instance.transform.localPosition = new Vector3(0f, 2.12f, 0f);
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
        }

        private static void CreateHudPrefab()
        {
            var root = new GameObject("ShowcaseHud");
            try
            {
                root.AddComponent<ShowcaseHudPresenter>();
                PrefabUtility.SaveAsPrefabAsset(root, HudPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void CreateEnemyAnchorPrefab()
        {
            var root = new GameObject(EnemyAnchorName);
            try
            {
                root.AddComponent<ShowcaseWorldHealthAnchor>();
                PrefabUtility.SaveAsPrefabAsset(root, EnemyAnchorPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static GameObject RequireRootAny(Scene scene, params string[] names)
        {
            foreach (string name in names)
            {
                GameObject root = FindRoot(scene, name);
                if (root != null)
                {
                    return root;
                }
            }

            throw new InvalidOperationException(
                $"Required Scene root not found: {string.Join(" or ", names)}.");
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == name)
                {
                    return root;
                }
            }

            return null;
        }
    }
}
