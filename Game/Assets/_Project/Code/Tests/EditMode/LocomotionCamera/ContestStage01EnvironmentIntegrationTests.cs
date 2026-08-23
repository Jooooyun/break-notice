using System;
using System.Linq;
using BreakNotice.Contest;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestStage01EnvironmentIntegrationTests
    {
        private const string ExpectedSourceCommit =
            "223d48de8be514bd8235c76c9fe32d1270927675";

        private const string ExpectedEnvironmentSceneGuid =
            "f1cc13ff499a5c34b8bf36287ba94871";

        private const string ExpectedPreviewCharacterGuid =
            "cac49e26122a33b4788046ebd9c0fa02";

        private const string ExpectedEnvironmentFloorGuid =
            "5f67da4a36b05f74eb3f37b6b2412bf6";

        private const string EnvironmentFloorAssetPath =
            "Packages/com.breaknotice.environment.officecore/Architecture/floor.fbx";

        private const string EnvironmentSampleMaterialPath =
            "Packages/com.breaknotice.props.gameplay/PRP_FileBinder/Materials/" +
            "PRP_binder_B_v001.mat";

        [TearDown]
        public void RestoreEmptyEditModeScene()
        {
            EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
        }

        [Test]
        public void Integration_IsPinnedToLatestEnvironmentCommit_AndAllAssetsResolve()
        {
            Assert.That(
                ShowcaseStage01Bootstrap.IntegrationSourceCommit,
                Is.EqualTo(ExpectedSourceCommit));

            Assert.That(
                AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    ShowcaseStage01Bootstrap.BootstrapScenePath),
                Is.Not.Null,
                ShowcaseStage01Bootstrap.BootstrapScenePath);
            Assert.That(
                AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    ShowcaseStage01Bootstrap.GameplayScenePath),
                Is.Not.Null,
                ShowcaseStage01Bootstrap.GameplayScenePath);
            Assert.That(
                AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    ShowcaseStage01Bootstrap.EnvironmentScenePath),
                Is.Not.Null,
                ShowcaseStage01Bootstrap.EnvironmentScenePath);
            Assert.That(
                AssetDatabase.AssetPathToGUID(
                    ShowcaseStage01Bootstrap.EnvironmentScenePath),
                Is.EqualTo(ExpectedEnvironmentSceneGuid));
            Assert.That(
                AssetDatabase.AssetPathToGUID(
                    ShowcaseStage01Bootstrap.EnvironmentPreviewCharacterAssetPath),
                Is.EqualTo(ExpectedPreviewCharacterGuid));
            Assert.That(
                AssetDatabase.AssetPathToGUID(EnvironmentFloorAssetPath),
                Is.EqualTo(ExpectedEnvironmentFloorGuid));
            Assert.That(
                AssetDatabase.LoadAssetAtPath<Material>(
                    EnvironmentSampleMaterialPath),
                Is.Not.Null,
                EnvironmentSampleMaterialPath);
        }

        [Test]
        public void BuildSettings_StartWithBootstrap_ThenGameplayAndVisualMap()
        {
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            Assert.That(buildScenes, Is.Not.Empty);
            Assert.That(buildScenes[0].enabled, Is.True);
            Assert.That(
                buildScenes[0].path,
                Is.EqualTo(ShowcaseStage01Bootstrap.BootstrapScenePath));

            Assert.That(
                ShowcaseStage01Bootstrap.CreateBuildScenePaths(),
                Is.EqualTo(new[]
                {
                    ShowcaseStage01Bootstrap.BootstrapScenePath,
                    ShowcaseStage01Bootstrap.GameplayScenePath,
                    ShowcaseStage01Bootstrap.EnvironmentScenePath
                }));

            foreach (string requiredPath in
                     ShowcaseStage01Bootstrap.CreateBuildScenePaths())
            {
                EditorBuildSettingsScene match = buildScenes.SingleOrDefault(scene =>
                    string.Equals(
                        scene.path,
                        requiredPath,
                        StringComparison.OrdinalIgnoreCase));
                Assert.That(match, Is.Not.Null, requiredPath);
                Assert.That(match.enabled, Is.True, requiredPath);
            }
        }

        [Test]
        public void LegacySuppression_RemovesEveryGrayboxObjectExceptSupportFloor()
        {
            var legacyRoot = new GameObject(
                ShowcaseStage01Bootstrap.LegacyEnvironmentRootName);

            var collisionGroup = new GameObject("01_STATIC_COLLISION");
            collisionGroup.transform.SetParent(legacyRoot.transform, false);
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = ShowcaseStage01Bootstrap.LegacyFloorColliderName;
            floor.transform.SetParent(collisionGroup.transform, false);
            Renderer floorRenderer = floor.GetComponent<Renderer>();
            Collider floorCollider = floor.GetComponent<Collider>();

            GameObject legacyWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            legacyWall.name = "PROXY_Wall_North";
            legacyWall.transform.SetParent(collisionGroup.transform, false);
            Renderer wallRenderer = legacyWall.GetComponent<Renderer>();
            Collider wallCollider = legacyWall.GetComponent<Collider>();

            GameObject decorative = GameObject.CreatePrimitive(PrimitiveType.Cube);
            decorative.name = "DENSITY_PROXY_DECORATIVE";
            decorative.transform.SetParent(legacyRoot.transform, false);
            Renderer decorativeRenderer = decorative.GetComponent<Renderer>();
            Collider decorativeCollider = decorative.GetComponent<Collider>();
            Rigidbody decorativeBody = decorative.AddComponent<Rigidbody>();

            var lightObject = new GameObject("LEGACY_LIGHT");
            lightObject.transform.SetParent(legacyRoot.transform, false);
            Light light = lightObject.AddComponent<Light>();

            var audioObject = new GameObject("LEGACY_AUDIO");
            audioObject.transform.SetParent(legacyRoot.transform, false);
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();

            var canvasObject = new GameObject("LEGACY_CANVAS");
            canvasObject.transform.SetParent(legacyRoot.transform, false);
            Canvas canvas = canvasObject.AddComponent<Canvas>();

            int changed = ShowcaseStage01Bootstrap.SuppressLegacyEnvironment(
                legacyRoot,
                out int preservedColliders);

            Assert.That(changed, Is.GreaterThanOrEqualTo(6));
            Assert.That(preservedColliders, Is.EqualTo(1));
            Assert.That(floorRenderer.enabled, Is.False);
            Assert.That(floorCollider.enabled, Is.True);
            Assert.That(wallRenderer.enabled, Is.False);
            Assert.That(wallCollider.enabled, Is.False);
            Assert.That(decorativeRenderer.enabled, Is.False);
            Assert.That(decorativeCollider.enabled, Is.False);
            Assert.That(decorativeBody.isKinematic, Is.True);
            Assert.That(decorativeBody.useGravity, Is.False);
            Assert.That(decorativeBody.detectCollisions, Is.False);
            Assert.That(light.enabled, Is.False);
            Assert.That(audioSource.enabled, Is.False);
            Assert.That(canvas.enabled, Is.False);
        }

        [Test]
        public void Stage01Recenter_MapsAuthoredFloorOriginToGameplayOrigin()
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = ShowcaseStage01Bootstrap.EnvironmentFloorObjectName;
            floor.transform.position = new Vector3(-7.39f, 0f, -40.955f);
            floor.transform.localScale = new Vector3(60f, 0.2f, 64f);
            SceneManager.MoveGameObjectToScene(floor, scene);

            var marker = new GameObject("VISUAL_ALIGNMENT_MARKER");
            marker.transform.position = new Vector3(5f, 2f, -15f);
            SceneManager.MoveGameObjectToScene(marker, scene);

            Vector3 offset = ShowcaseStage01Bootstrap.RecenterStageEnvironment(scene);

            Assert.That(offset.x, Is.EqualTo(7.39f).Within(0.001f));
            Assert.That(offset.y, Is.EqualTo(0f).Within(0.001f));
            Assert.That(offset.z, Is.EqualTo(40.955f).Within(0.001f));
            Assert.That(floor.transform.position.x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(floor.transform.position.y, Is.EqualTo(0f).Within(0.001f));
            Assert.That(floor.transform.position.z, Is.EqualTo(0f).Within(0.001f));
            Assert.That(marker.transform.position.x, Is.EqualTo(12.39f).Within(0.001f));
            Assert.That(marker.transform.position.y, Is.EqualTo(2f).Within(0.001f));
            Assert.That(marker.transform.position.z, Is.EqualTo(25.955f).Within(0.001f));
        }

        [Test]
        public void BuiltInMaterialConversion_PreservesPinnedSourceAndSurfaceData()
        {
            Assert.That(
                GraphicsSettings.currentRenderPipeline,
                Is.Null,
                "The proven v007 gameplay project remains on the Built-in pipeline.");

            Material source = AssetDatabase.LoadAssetAtPath<Material>(
                EnvironmentSampleMaterialPath);
            Assert.That(source, Is.Not.Null);
            Assert.That(source.shader, Is.Not.Null);
            Assert.That(
                source.shader.name,
                Does.StartWith("Universal Render Pipeline/"));

            string originalShaderName = source.shader.name;
            Material converted =
                ShowcaseStage01Bootstrap.CreateBuiltInCompatibleMaterial(source);
            try
            {
                Assert.That(converted.shader.name, Is.EqualTo("Standard"));
                Assert.That(converted.shader.isSupported, Is.True);
                Assert.That(converted.enableInstancing, Is.True,
                    "Repeated Stage01 meshes must use GPU instancing after " +
                    "Built-in material conversion.");
                Assert.That(source.shader.name, Is.EqualTo(originalShaderName));
                Assert.That(
                    converted.GetTexture("_MainTex"),
                    Is.EqualTo(source.GetTexture("_BaseMap")));

                Color expectedColor = source.GetColor("_BaseColor");
                Color actualColor = converted.GetColor("_Color");
                Assert.That(actualColor.r, Is.EqualTo(expectedColor.r).Within(0.0001f));
                Assert.That(actualColor.g, Is.EqualTo(expectedColor.g).Within(0.0001f));
                Assert.That(actualColor.b, Is.EqualTo(expectedColor.b).Within(0.0001f));
                Assert.That(actualColor.a, Is.EqualTo(expectedColor.a).Within(0.0001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(converted);
            }
        }

        [Test]
        public void LegacySupportFloor_ExpandsAcrossFullStageBoundsAndKeepsTopHeight()
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            GameObject support = GameObject.CreatePrimitive(PrimitiveType.Cube);
            support.name = ShowcaseStage01Bootstrap.LegacyFloorColliderName;
            support.transform.position = new Vector3(0f, -0.25f, 0f);
            support.transform.localScale = new Vector3(10f, 0.5f, 8f);
            support.GetComponent<Renderer>().enabled = false;
            SceneManager.MoveGameObjectToScene(support, scene);
            BoxCollider supportCollider = support.GetComponent<BoxCollider>();

            GameObject westVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            westVisual.name = "STAGE_WEST_EDGE";
            westVisual.transform.position = new Vector3(-38f, 1f, -27f);
            westVisual.transform.localScale = new Vector3(6f, 2f, 4f);
            SceneManager.MoveGameObjectToScene(westVisual, scene);

            GameObject eastVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eastVisual.name = "STAGE_EAST_EDGE";
            eastVisual.transform.position = new Vector3(44f, 1f, 35f);
            eastVisual.transform.localScale = new Vector3(8f, 2f, 6f);
            SceneManager.MoveGameObjectToScene(eastVisual, scene);

            Physics.SyncTransforms();
            Bounds initialBounds = supportCollider.bounds;
            Bounds westBounds = westVisual.GetComponent<Renderer>().bounds;
            Bounds eastBounds = eastVisual.GetComponent<Renderer>().bounds;

            Bounds expanded = ShowcaseStage01Bootstrap.ExpandLegacyFloorToCoverStage(
                supportCollider,
                scene,
                ShowcaseStage01Bootstrap.LegacyFloorCoverageMargin);

            float margin = ShowcaseStage01Bootstrap.LegacyFloorCoverageMargin;
            Assert.That(expanded.max.y, Is.EqualTo(initialBounds.max.y).Within(0.001f));
            Assert.That(expanded.size.y, Is.EqualTo(initialBounds.size.y).Within(0.001f));
            Assert.That(expanded.min.x, Is.LessThanOrEqualTo(westBounds.min.x - margin + 0.05f));
            Assert.That(expanded.min.z, Is.LessThanOrEqualTo(westBounds.min.z - margin + 0.05f));
            Assert.That(expanded.max.x, Is.GreaterThanOrEqualTo(eastBounds.max.x + margin - 0.05f));
            Assert.That(expanded.max.z, Is.GreaterThanOrEqualTo(eastBounds.max.z + margin - 0.05f));
            Assert.That(expanded.size.x, Is.GreaterThan(initialBounds.size.x));
            Assert.That(expanded.size.z, Is.GreaterThan(initialBounds.size.z));
            Assert.That(supportCollider.enabled, Is.True);
        }

        [Test]
        public void StagePreparation_KeepsVisualsAndAuthorsFinalMapCollision()
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "ENV_Wall_6m_TEST";
            SceneManager.MoveGameObjectToScene(visual, scene);
            Renderer visualRenderer = visual.GetComponent<Renderer>();
            Collider visualCollider = visual.GetComponent<Collider>();
            Rigidbody visualBody = visual.AddComponent<Rigidbody>();

            var previewActor = new GameObject("FINAL_MAP_PREVIEW_ACTOR");
            SceneManager.MoveGameObjectToScene(previewActor, scene);
            Animator previewAnimator = previewActor.AddComponent<Animator>();
            GameObject previewVisual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            previewVisual.transform.SetParent(previewActor.transform, false);
            Renderer previewRenderer = previewVisual.GetComponent<Renderer>();
            Collider previewCollider = previewVisual.GetComponent<Collider>();

            int enabled = ShowcaseStage01Bootstrap.PrepareStageEnvironment(
                scene,
                out int disabledColliders,
                out int generatedColliders,
                out int convertedMaterials);

            Assert.That(enabled, Is.EqualTo(1));
            Assert.That(disabledColliders, Is.GreaterThanOrEqualTo(2));
            Assert.That(generatedColliders, Is.EqualTo(1));
            Assert.That(convertedMaterials, Is.EqualTo(0));
            Assert.That(visualRenderer.enabled, Is.True);
            Assert.That(visualCollider.enabled, Is.False);
            Assert.That(visual.GetComponent<MeshCollider>(), Is.Not.Null);
            Assert.That(visual.GetComponent<MeshCollider>().enabled, Is.True);
            Assert.That(visualBody.isKinematic, Is.True);
            Assert.That(visualBody.useGravity, Is.False);
            Assert.That(visualBody.detectCollisions, Is.True);
            Assert.That(previewAnimator.enabled, Is.False);
            Assert.That(previewRenderer.enabled, Is.False);
            Assert.That(previewCollider.enabled, Is.False);
        }
    }
}
