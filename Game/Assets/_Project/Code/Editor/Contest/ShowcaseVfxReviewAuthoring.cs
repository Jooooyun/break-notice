using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BreakNotice.ContestReview;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace BreakNotice.Contest.Editor
{
    /// <summary>
    /// Authors the isolated premium Showcase VFX review scene. The generated
    /// assets remain local-review-only and do not reference gameplay state,
    /// runtime hooks, shared prefabs, build settings, packages, or custom shaders.
    /// </summary>
    [InitializeOnLoad]
    public static class ShowcaseVfxReviewAuthoring
    {
        public const string ReviewRootPath =
            "Assets/_Project/Art/VFX/Showcase";

        public const string PremiumRootPath =
            ReviewRootPath + "/Premium_v002";

        public const string CompleteRootPath =
            ReviewRootPath + "/Complete_v003";

        public const string ReviewScenePath =
            "Assets/_Project/Scenes/90_Review/LOCAL_REVIEW_ONLY_ShowcaseVFX.unity";

        public const string ParticleShaderName =
            "Particles/Standard Unlit";

        public const string ReviewMenuPath =
            "BREAK NOTICE/Contest/Prepare Isolated Showcase VFX Review";

        private const string BackdropShaderName = "Unlit/Color";
        private const string SceneRootName = "LOCAL_REVIEW_ONLY_ShowcaseVFX_ROOT";
        private const string OldNewZoneName = "ZONE_01_OLD_VS_NEW";
        private const string LayerZoneName = "ZONE_02_SOLO_LAYER_AND_COMPOSITE";
        private const string VariationZoneName = "ZONE_03_PHYSICAL_VARIATION_AUDITION";
        private const string GameplayZoneName = "ZONE_04_GAMEPLAY_CAMERA_EQUIVALENT";
        private const string CompletePagePrefix = "COMPLETE_PAGE_";
        private const string CompleteCaptureDirectoryArgument =
            "-showcaseVfxCompleteCaptureDirectory";
        private const string FixedSceneViewCaptureArgument =
            "-showcaseVfxFixedSceneViewCapture";
        private const string ActualPlayCaptureArgument =
            "-showcaseVfxActualPlayCapture";
        private const string ActualPlayCaptureCompleteKey =
            "BreakNotice.ShowcaseVfxReview.ActualPlayCaptureComplete";
        private const string ActualPlayCaptureExitCodeKey =
            "BreakNotice.ShowcaseVfxReview.ActualPlayCaptureExitCode";
        private const float ReviewCameraFieldOfView = 40f;
        private const float ReviewCameraNearClip = 0.1f;
        private const float ReviewCameraFarClip = 80f;
        private const float ReviewSceneViewSize = 6.15f;
        private const float ActualPlayCaptureDelay = 0.36f;

        private static readonly Vector3 ReviewCameraPosition =
            new Vector3(400f, 0.65f, -19.5f);

        private static readonly Vector3 ReviewCameraTarget =
            new Vector3(400f, 0.55f, 0f);

        private static readonly Quaternion ReviewCameraRotation =
            Quaternion.LookRotation(
                ReviewCameraTarget - ReviewCameraPosition,
                Vector3.up);

        private static double actualPlayCaptureEnteredAt;
        private static bool actualPlayCaptureScheduled;

        static ShowcaseVfxReviewAuthoring()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;

            if (!string.IsNullOrWhiteSpace(ReadCommandLineValue(ActualPlayCaptureArgument)))
            {
                if (EditorApplication.isPlaying)
                {
                    ScheduleActualPlayCapture();
                }
                else if (SessionState.GetBool(ActualPlayCaptureCompleteKey, false))
                {
                    EditorApplication.delayCall += ExitAfterActualPlayCapture;
                }
            }
        }

        private static readonly VariationDefinition[] PhysicalVariations =
        {
            new VariationDefinition("A", 0f, 1.00f, 0.000f, 0.00f, 0.00f),
            new VariationDefinition("B", 14f, 0.82f, 0.035f, 0.12f, -0.06f),
            new VariationDefinition("C", -12f, 1.20f, 0.060f, -0.10f, 0.09f)
        };

        private static readonly EffectDefinition[] Effects =
        {
            new EffectDefinition(
                "STATION_01_INTERACTION_TARGET_READABILITY",
                "01  INTERACTION TARGET READABILITY",
                "InteractionTarget",
                false,
                1.55f,
                1.60f,
                "Assets/_Project/Art/VFX/Showcase/Prefabs/PF_VFX_Showcase_InteractionSoftRing_LOCAL_REVIEW_ONLY_v001.prefab",
                new[]
                {
                    TextureLayer("EdgeTrace", "T_VFX_Showcase_InteractionTarget_EdgeTrace_v002.png", 0.92f, 0.00f, 1.72f, 0f),
                    TextureLayer("ContactBracket", "T_VFX_Showcase_InteractionTarget_ContactBracket_v002.png", 0.76f, 0.06f, 1.60f, 0f),
                    TextureLayer("MaterialWink", "T_VFX_Showcase_InteractionTarget_MaterialWink_v002.png", 0.54f, 0.13f, 1.68f, 0f)
                }),
            new EffectDefinition(
                "STATION_02_CATCH_CONFIRM",
                "02  CATCH CONFIRM",
                "CatchConfirm",
                true,
                1.26f,
                1.05f,
                "Assets/_Project/Art/VFX/Showcase/Prefabs/PF_VFX_Showcase_PickupCatchConfirm_LOCAL_REVIEW_ONLY_v001.prefab",
                new[]
                {
                    FlipbookLayer("PrimaryCompression", "T_VFX_Showcase_CatchConfirm_PrimaryCompression_4x4_v002.png", 0.68f, 0.00f, 1.22f, -2f),
                    FlipbookLayer("PaperSnap", "T_VFX_Showcase_CatchConfirm_PaperSnap_4x4_v002.png", 0.76f, 0.025f, 1.12f, 0f),
                    FlipbookLayer("DirectionalSettle", "T_VFX_Showcase_CatchConfirm_DirectionalSettle_4x4_v002.png", 0.82f, 0.08f, 1.18f, 1f)
                }),
            new EffectDefinition(
                "STATION_03_INVALID_REJECTED_ACTION",
                "03  INVALID / REJECTED ACTION",
                "InvalidRejected",
                false,
                1.34f,
                1.48f,
                "Assets/_Project/Art/VFX/Showcase/Prefabs/PF_VFX_Showcase_InteractionRejected_LOCAL_REVIEW_ONLY_v001.prefab",
                new[]
                {
                    TextureLayer("ReticleInterruption", "T_VFX_Showcase_InvalidRejected_ReticleInterruption_v002.png", 0.54f, 0.00f, 1.38f, 0f),
                    TextureLayer("TargetDropout", "T_VFX_Showcase_InvalidRejected_TargetDropout_v002.png", 0.78f, 0.035f, 1.48f, 0f),
                    TextureLayer("ReverseEcho", "T_VFX_Showcase_InvalidRejected_ReverseEcho_v002.png", 0.62f, 0.07f, 1.45f, 0f)
                }),
            new EffectDefinition(
                "STATION_04_ALLY_READY_SUPPORT",
                "04  ALLY READY SUPPORT",
                "AllyReady",
                false,
                1.52f,
                1.46f,
                "Assets/_Project/Art/VFX/Showcase/Prefabs/PF_VFX_Showcase_AllyReadySupport_LOCAL_REVIEW_ONLY_v001.prefab",
                new[]
                {
                    TextureLayer("GazeTick", "T_VFX_Showcase_AllyReady_GazeTick_v002.png", 0.66f, 0.00f, 1.38f, 0f),
                    TextureLayer("PostureCue", "T_VFX_Showcase_AllyReady_PostureCue_v002.png", 0.90f, 0.055f, 1.48f, 0f),
                    TextureLayer("FootContact", "T_VFX_Showcase_AllyReady_FootContact_v002.png", 0.74f, 0.11f, 1.52f, 0f)
                }),
            new EffectDefinition(
                "STATION_05_LIGHT_OFFICE_CONTACT_BINDER",
                "05  LIGHT OFFICE CONTACT / BINDER IMPACT",
                "BinderImpact",
                true,
                1.44f,
                1.30f,
                "Assets/_Project/Art/VFX/Showcase/Prefabs/PF_VFX_Showcase_LightContactDust_4x4_LOCAL_REVIEW_ONLY_v001.prefab",
                new[]
                {
                    FlipbookLayer("PrimaryTransient", "T_VFX_Showcase_BinderImpact_PrimaryTransient_4x4_v002.png", 0.72f, 0.00f, 1.34f, -3f),
                    FlipbookLayer("DirectionalDust", "T_VFX_Showcase_BinderImpact_DirectionalDust_4x4_v002.png", 0.88f, 0.025f, 1.46f, -1f),
                    FlipbookLayer("FiberSettle", "T_VFX_Showcase_BinderImpact_FiberSettle_4x4_v002.png", 0.96f, 0.08f, 1.36f, 2f)
                }),
            new EffectDefinition(
                "STATION_06_CHAIR_WHEEL_CONTACT_SKID",
                "06  CHAIR WHEEL CONTACT / SKID",
                "ChairSkid",
                true,
                1.62f,
                1.58f,
                "Assets/_Project/Art/VFX/Showcase/Prefabs/PF_VFX_Showcase_ChairWheelContact_4x4_LOCAL_REVIEW_ONLY_v001.prefab",
                new[]
                {
                    FlipbookLayer("CasterBite", "T_VFX_Showcase_ChairSkid_CasterBite_4x4_v002.png", 0.82f, 0.00f, 1.64f, -1f),
                    FlipbookLayer("RubberBody", "T_VFX_Showcase_ChairSkid_RubberBody_4x4_v002.png", 0.98f, 0.035f, 1.92f, 0f),
                    FlipbookLayer("ContactSettle", "T_VFX_Showcase_ChairSkid_ContactSettle_4x4_v002.png", 1.04f, 0.10f, 1.76f, 1f)
                }),
            new EffectDefinition(
                "STATION_07_PAPER_MATERIAL_BURST",
                "07  PAPER MATERIAL BURST",
                "PaperBurst",
                true,
                1.72f,
                1.48f,
                "Assets/_Project/Art/VFX/Showcase/Prefabs/PF_VFX_Showcase_RestrainedPaperBurst_4x4_LOCAL_REVIEW_ONLY_v001.prefab",
                new[]
                {
                    FlipbookLayer("PrimarySnap", "T_VFX_Showcase_PaperBurst_PrimarySnap_4x4_v002.png", 0.82f, 0.00f, 1.48f, -4f),
                    FlipbookLayer("PageFan", "T_VFX_Showcase_PaperBurst_PageFan_4x4_v002.png", 1.10f, 0.035f, 1.62f, 0f),
                    FlipbookLayer("FlutterSettle", "T_VFX_Showcase_PaperBurst_FlutterSettle_4x4_v002.png", 1.28f, 0.12f, 1.58f, 3f)
                }),
            new EffectDefinition(
                "STATION_08_EXIT_ROUTE_ACTIVATION",
                "08  EXIT / ROUTE ACTIVATION",
                "ExitRoute",
                false,
                1.76f,
                1.64f,
                "Assets/_Project/Art/VFX/Showcase/Prefabs/PF_VFX_Showcase_ExitActivation_LOCAL_REVIEW_ONLY_v001.prefab",
                new[]
                {
                    TextureLayer("PracticalLamp", "T_VFX_Showcase_ExitRoute_PracticalLamp_v002.png", 0.80f, 0.00f, 1.58f, 0f),
                    TextureLayer("SignHardware", "T_VFX_Showcase_ExitRoute_SignHardware_v002.png", 1.06f, 0.08f, 1.66f, 0f),
                    TextureLayer("RouteEdge", "T_VFX_Showcase_ExitRoute_RouteEdge_v002.png", 1.18f, 0.16f, 1.68f, 0f)
                })
        };

        private static readonly CompleteEffectDefinition[] CompleteEffects =
        {
            CE("VFX-SHW-ACQ-001", "BinderEligible", "BINDER ACQUISITION AVAILABLE", false, 1.00f, 0f),
            CE("VFX-SHW-ACQ-002", "ChairEligible", "CHAIR ACQUISITION AVAILABLE", false, 1.00f, 0f),
            CE("VFX-SHW-ACQ-003", "AcquisitionConfirmed", "ACQUISITION CONFIRMED", false, 1.00f, 0f),
            CE("VFX-SHW-THR-001", "ThrowChargeStage1", "THROW CHARGE / STAGE 1", false, 0.96f, 0f),
            CE("VFX-SHW-THR-002", "ThrowChargeStage2", "THROW CHARGE / STAGE 2", false, 1.00f, 0f),
            CE("VFX-SHW-THR-003", "ThrowChargeFull", "THROW CHARGE / FULL", false, 1.05f, 0f),
            CE("VFX-SHW-THR-004", "ThrowPhysicalRelease", "THROW PHYSICAL RELEASE", true, 0.92f, 8f),
            CE("VFX-SHW-THR-005", "BinderPhysicalContact", "BINDER PHYSICAL CONTACT", true, 1.04f, 12f),
            CE("VFX-SHW-THR-006", "ChairPhysicalContact", "CHAIR PHYSICAL CONTACT", true, 1.18f, 4f),
            CE("VFX-SHW-ALY-001", "AllyReady", "ALLY READY SUPPORT", false, 1.00f, 0f),
            CE("VFX-SHW-ALY-002", "QBinderSupplyAccepted", "Q BINDER SUPPLY ACCEPTED", true, 0.92f, 15f),
            CE("VFX-SHW-ALY-003", "QChairSupplyAccepted", "Q CHAIR SUPPLY ACCEPTED", true, 1.05f, 2f),
            CE("VFX-SHW-ALY-004", "BinderCatch", "BINDER CATCH", true, 0.98f, -4f),
            CE("VFX-SHW-ALY-005", "ChairFeedRelease", "CHAIR FLOOR-SUPPORTED FEED", true, 1.08f, 0f),
            CE("VFX-SHW-ALY-006", "ChairWheelFloorContact", "CHAIR WHEEL / FLOOR CONTACT", true, 1.12f, -2f),
            CE("VFX-SHW-ALY-007", "SupplyRejected", "SUPPLY REJECTED", false, 1.00f, 0f),
            CE("VFX-SHW-ALY-008", "SupplyPathRejected", "SUPPLY PATH REJECTED", false, 1.00f, 0f),
            CE("VFX-SHW-TEAM-001", "PassBadge", "PASS BADGE", false, 1.00f, 0f),
            CE("VFX-SHW-TEAM-002", "BraceBadge", "BRACE BADGE", false, 1.00f, 0f),
            CE("VFX-SHW-TEAM-003", "BreakBadge", "BREAK BADGE", false, 1.00f, 0f),
            CE("VFX-SHW-TEAM-004", "TeamReady", "TEAM READY", false, 1.05f, 0f),
            CE("VFX-SHW-FIN-001", "HandoverCommit", "HANDOVER CRASH / COMMIT", false, 1.00f, 0f),
            CE("VFX-SHW-FIN-002", "HandoverChairSkid", "HANDOVER / CHAIR SKID", true, 1.20f, 1f),
            CE("VFX-SHW-FIN-003", "HandoverChairRunnerContact", "HANDOVER / CHAIR TO RUNNER", true, 1.18f, 7f),
            CE("VFX-SHW-FIN-004", "HandoverRunnerDrawerContact", "HANDOVER / RUNNER TO DRAWER", true, 1.20f, 5f),
            CE("VFX-SHW-OBJ-001", "DrawerCausalImpact", "DRAWER CAUSAL IMPACT", true, 1.24f, 3f),
            CE("VFX-SHW-OBJ-002", "DrawerRouteOpen", "DRAWER ROUTE STATE OPEN", false, 1.05f, 0f),
            CE("VFX-SHW-OBJ-003", "ExitActivation", "EXIT / ROUTE ACTIVATION", false, 1.08f, 0f),
            CE("VFX-SHW-RUN-001", "RunnerPressureTelegraph", "RUNNER PRESSURE TELEGRAPH", false, 1.04f, 0f),
            CE("VFX-SHW-RUN-002", "RunnerPressureContact", "RUNNER PRESSURE CONTACT", true, 1.08f, 4f),
            CE("VFX-SHW-RUN-003", "RunnerStaggerOpportunity", "RUNNER STAGGER / TAG READABILITY", false, 1.00f, 0f),
            CE("VFX-SHW-RUN-004", "RunnerGrabPressure", "RUNNER GRAB PRESSURE", false, 1.03f, 0f),
            CE("VFX-SHW-SYS-001", "ResetStart", "RESET START", false, 1.00f, 0f),
            CE("VFX-SHW-SYS-002", "ResetComplete", "RESET COMPLETE", false, 1.00f, 0f),
            CE("VFX-SHW-SYS-003", "UiConfirmation", "UI CONFIRMATION", false, 0.94f, 0f),
            CE("VFX-SHW-SYS-004", "UiRejection", "UI REJECTION", false, 0.94f, 0f),
            CE("VFX-LAB-HIT-001", "PunchLightCloth", "PUNCH LIGHT / CLOTH CONTACT", true, 0.96f, 10f),
            CE("VFX-LAB-HIT-002", "PunchMediumCloth", "PUNCH MEDIUM / CLOTH CONTACT", true, 1.10f, 4f),
            CE("VFX-LAB-HIT-003", "PunchStrongNonlethal", "PUNCH STRONG / NONLETHAL CONTACT", true, 1.28f, -2f),
            CE("VFX-LAB-HIT-004", "PunchGlancing", "PUNCH GLANCING CONTACT", true, 0.98f, 24f),
            CE("VFX-LAB-HIT-005", "PunchBraceBlock", "PUNCH / FOREARM BRACE BLOCK", true, 1.12f, -16f),
            CE("VFX-LAB-HIT-006", "ShoulderBodyCheck", "SHOULDER / BODY CHECK", true, 1.22f, 2f),
            CE("VFX-LAB-MAT-001", "HandDeskMiss", "HAND TO DESK / MISS CONTACT", true, 1.02f, -8f),
            CE("VFX-LAB-MAT-002", "MetalDrawerContact", "PAINTED METAL DRAWER CONTACT", true, 1.16f, 6f),
            CE("VFX-LAB-MAT-003", "ChairFramePlasticContact", "CHAIR FRAME / PLASTIC CONTACT", true, 1.10f, 0f),
            CE("VFX-LAB-MAT-004", "BinderPaperEdgeContact", "BINDER / PAPER EDGE CONTACT", true, 1.02f, 18f),
            CE("VFX-LAB-MAT-005", "ObjectFloorDrop", "OFFICE OBJECT / FLOOR DROP", true, 1.24f, 72f),
            CE("VFX-LAB-MAT-006", "WallPartitionContact", "WALL / PARTITION CONTACT", true, 1.12f, -6f),
        };

        [MenuItem(ReviewMenuPath)]
        public static void PrepareReviewSetup()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            EnsureAssetFolder(CompleteRootPath);
            EnsureAssetFolder(CompleteRootPath + "/Materials");
            EnsureAssetFolder(CompleteRootPath + "/Prefabs");
            EnsureAssetFolder("Assets/_Project/Scenes/90_Review");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            foreach (CompleteLayerDefinition layer in CompleteEffects.SelectMany(effect => effect.Layers))
            {
                ConfigureCompleteTextureImporter(layer);
            }

            Shader particleShader = Shader.Find(ParticleShaderName);
            if (particleShader == null)
            {
                throw new InvalidOperationException(
                    $"Required built-in particle shader is unavailable: {ParticleShaderName}");
            }

            var completeMaterials = new Dictionary<string, Material>(StringComparer.Ordinal);
            foreach (CompleteLayerDefinition layer in CompleteEffects.SelectMany(effect => effect.Layers))
            {
                completeMaterials.Add(layer.MaterialName, CreateCompleteParticleMaterial(layer, particleShader));
            }
            foreach (CompleteEffectDefinition effect in CompleteEffects)
            {
                CreateCompletePrefab(effect, completeMaterials);
            }

            Material backdrop = LoadExistingPremiumReviewMaterial(
                "M_LOCAL_REVIEW_ONLY_ShowcaseVFX_PremiumBackdrop_v002");
            Material panel = LoadExistingPremiumReviewMaterial(
                "M_LOCAL_REVIEW_ONLY_ShowcaseVFX_PremiumPanel_v002");
            Material floor = LoadExistingPremiumReviewMaterial(
                "M_LOCAL_REVIEW_ONLY_ShowcaseVFX_PremiumFloor_v002");
            Material prop = LoadExistingPremiumReviewMaterial(
                "M_LOCAL_REVIEW_ONLY_ShowcaseVFX_PremiumProp_v002");
            Material binder = LoadExistingPremiumReviewMaterial(
                "M_LOCAL_REVIEW_ONLY_ShowcaseVFX_PremiumBinder_v002");
            Material ally = LoadExistingPremiumReviewMaterial(
                "M_LOCAL_REVIEW_ONLY_ShowcaseVFX_PremiumAlly_v002");
            Material exit = LoadExistingPremiumReviewMaterial(
                "M_LOCAL_REVIEW_ONLY_ShowcaseVFX_PremiumExit_v002");

            GameObject reviewRoot = CreateReviewScene(
                backdrop,
                panel,
                floor,
                prop,
                binder,
                ally,
                exit);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            FrameSceneViewForReview(reviewRoot, true);
            Debug.Log(
                "Showcase VFX complete review prepared: selectedCore=8/8, " +
                "lockedCueEffects=36/36, impactMaterialAuditions=12/12, " +
                "completeLayerImports=144, completeCompositeStations=48/48, pages=14, " +
                "soloLayerAuditions=144/144, gameplayContextEffects=8/8, " +
                "sceneViewFrontFramed=1, automaticPlayTour=1, gameplayIntegration=0, " +
                "status=REVIEW / OWNER APPROVAL PENDING.");
        }

        public static void CaptureFixedReviewSceneViewFrame()
        {
            string outputPath = RequireOutputPath(FixedSceneViewCaptureArgument);
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            GameObject root = FindReviewRoot(scene);
            SceneView sceneView = FrameSceneViewForReview(root, false);

            SimulateAllParticles(root, ActualPlayCaptureDelay);
            Camera sceneViewCamera = sceneView.camera;
            if (sceneViewCamera == null ||
                Vector3.Dot(sceneViewCamera.transform.forward, Vector3.forward) < 0.99f ||
                Vector3.Dot(sceneViewCamera.transform.up, Vector3.up) < 0.99f)
            {
                throw new InvalidOperationException(
                    "Scene View front/upright framing contract failed.");
            }

            Camera reviewCamera = root.GetComponentsInChildren<Camera>(true).Single();
            ConfigureReviewCamera(reviewCamera);
            GameObject captureCameraObject = new GameObject(
                "LOCAL_REVIEW_ONLY_FIXED_SCENE_CAPTURE_CAMERA");
            Camera captureCamera = captureCameraObject.AddComponent<Camera>();
            captureCamera.CopyFrom(reviewCamera);
            captureCamera.transform.SetPositionAndRotation(
                reviewCamera.transform.position,
                reviewCamera.transform.rotation);
            captureCamera.enabled = false;
            captureCamera.aspect = 16f / 9f;
            captureCamera.ResetProjectionMatrix();
            Texture2D capture = RenderCamera(captureCamera, 1920, 1080);
            try
            {
                File.WriteAllBytes(outputPath, capture.EncodeToPNG());
            }
            finally
            {
                DestroyReviewObject(capture);
                DestroyReviewObject(captureCameraObject);
            }

            Debug.Log(
                $"Complete Showcase VFX fixed Scene View capture written: {outputPath}; " +
                "orientation=FRONT_UPRIGHT; stations=8/8.");
        }

        public static void CaptureFixedReviewGameFrameFromActualPlay()
        {
            string outputPath = RequireOutputPath(ActualPlayCaptureArgument);
            SessionState.SetBool(ActualPlayCaptureCompleteKey, false);
            SessionState.SetInt(ActualPlayCaptureExitCodeKey, 1);

            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            GameObject root = FindReviewRoot(scene);
            Camera camera = root.GetComponentsInChildren<Camera>(true).Single();
            ConfigureReviewCamera(camera);
            camera.aspect = 16f / 9f;
            EditorApplication.isPlaying = true;
            Debug.Log(
                $"Premium Showcase VFX actual Play capture armed: {outputPath}");
        }

        public static void CapturePremiumReviewFrames()
        {
            const string argument = "-showcaseVfxPremiumCaptureDirectory";
            string outputDirectory = ReadCommandLineValue(argument);
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                throw new ArgumentException(
                    $"An output directory is required through {argument}.");
            }

            string fullOutputDirectory = Path.GetFullPath(outputDirectory);
            Directory.CreateDirectory(fullOutputDirectory);

            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            GameObject root = scene.GetRootGameObjects().Single(item => item.name == SceneRootName);
            Camera camera = root.GetComponentsInChildren<Camera>(true).Single();

            CaptureDefinition[] captures =
            {
                new CaptureDefinition(
                    "VFX_Showcase_Premium_OldVsNew_v002.png",
                    new Vector3(0f, 0.65f, -19.5f),
                    new Vector3(0f, 0.55f, 0f),
                    40f,
                    0.42f),
                new CaptureDefinition(
                    "VFX_Showcase_Premium_LayerBreakdown_v002.png",
                    new Vector3(100f, 0f, -24.5f),
                    new Vector3(100f, 0f, 0f),
                    40f,
                    0.44f),
                new CaptureDefinition(
                    "VFX_Showcase_Premium_PhysicalVariations_v002.png",
                    new Vector3(200f, 0.45f, -18.5f),
                    new Vector3(200f, 0.25f, 0f),
                    41f,
                    0.40f),
                new CaptureDefinition(
                    "VFX_Showcase_Premium_GameplayCamera_v002.png",
                    new Vector3(300f, 4.65f, -8.6f),
                    new Vector3(300f, 1.25f, 4.8f),
                    48f,
                    0.46f)
            };

            Texture2D gameplayCapture = null;
            foreach (CaptureDefinition capture in captures)
            {
                camera.transform.position = capture.CameraPosition;
                camera.transform.rotation = Quaternion.LookRotation(
                    capture.CameraTarget - capture.CameraPosition,
                    Vector3.up);
                camera.fieldOfView = capture.FieldOfView;

                SimulateAllParticles(root, capture.SimulationTime);
                Texture2D result = RenderCamera(camera, 2560, 1440);
                string outputPath = Path.Combine(fullOutputDirectory, capture.FileName);
                File.WriteAllBytes(outputPath, result.EncodeToPNG());
                Debug.Log($"Premium Showcase VFX review capture written: {outputPath}");

                if (capture.FileName.Contains("GameplayCamera", StringComparison.Ordinal))
                {
                    gameplayCapture = result;
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(result);
                }
            }

            if (gameplayCapture == null)
            {
                throw new InvalidOperationException("Gameplay-equivalent capture was not produced.");
            }

            Texture2D grayscale = CreateGrayscale(gameplayCapture);
            string grayscalePath = Path.Combine(
                fullOutputDirectory,
                "VFX_Showcase_Premium_GrayscaleReadability_v002.png");
            File.WriteAllBytes(grayscalePath, grayscale.EncodeToPNG());
            Debug.Log($"Premium Showcase VFX grayscale capture written: {grayscalePath}");
            UnityEngine.Object.DestroyImmediate(grayscale);
            UnityEngine.Object.DestroyImmediate(gameplayCapture);
        }

        public static void CaptureCompleteReviewPages()
        {
            string outputDirectory = ReadCommandLineValue(CompleteCaptureDirectoryArgument);
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                throw new ArgumentException(
                    $"An output directory is required through {CompleteCaptureDirectoryArgument}.");
            }
            string fullOutputDirectory = Path.GetFullPath(outputDirectory);
            Directory.CreateDirectory(fullOutputDirectory);

            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            GameObject root = FindReviewRoot(scene);
            Camera camera = root.GetComponentsInChildren<Camera>(true).Single();
            Transform[] pages = root.transform.Cast<Transform>()
                .Where(child => child.name.StartsWith(CompletePagePrefix, StringComparison.Ordinal))
                .OrderBy(child => child.name, StringComparer.Ordinal)
                .ToArray();
            if (pages.Length != 14)
            {
                throw new InvalidOperationException(
                    $"Complete capture requires 14 pages; found {pages.Length}.");
            }

            for (int index = 0; index < pages.Length; index++)
            {
                Transform anchor = pages[index].Find("CAMERA_ANCHOR");
                Transform target = pages[index].Find("CAMERA_TARGET");
                camera.transform.SetPositionAndRotation(
                    anchor.position,
                    Quaternion.LookRotation(target.position - anchor.position, Vector3.up));
                camera.fieldOfView = pages[index].name.Contains(
                    "GAMEPLAY_CAMERA",
                    StringComparison.Ordinal) ? 48f : ReviewCameraFieldOfView;
                camera.nearClipPlane = ReviewCameraNearClip;
                camera.farClipPlane = ReviewCameraFarClip;
                SimulateAllParticles(root, 0.28f + index * 0.005f);
                Texture2D capture = RenderCamera(camera, 1920, 1080);
                Texture2D grayscale = CreateGrayscale(capture);
                string stem = $"VFX_Showcase_Complete_UnityPage_{index:00}_v003";
                string colorPath = Path.Combine(fullOutputDirectory, stem + ".png");
                string grayscalePath = Path.Combine(
                    fullOutputDirectory,
                    stem + "_Grayscale.png");
                File.WriteAllBytes(colorPath, capture.EncodeToPNG());
                File.WriteAllBytes(grayscalePath, grayscale.EncodeToPNG());
                DestroyReviewObject(capture);
                DestroyReviewObject(grayscale);
                Debug.Log(
                    $"Complete Showcase VFX page capture written: {colorPath}; " +
                    $"page={index + 1}/14; orientation=FRONT_UPRIGHT.");
            }
        }

        private static LayerDefinition TextureLayer(
            string name,
            string fileName,
            float lifetime,
            float delay,
            float startSize,
            float rotation)
        {
            return new LayerDefinition(
                name,
                $"{ReviewRootPath}/Textures/Premium_v002/{fileName}",
                false,
                lifetime,
                delay,
                startSize,
                rotation);
        }

        private static LayerDefinition FlipbookLayer(
            string name,
            string fileName,
            float lifetime,
            float delay,
            float startSize,
            float rotation)
        {
            return new LayerDefinition(
                name,
                $"{ReviewRootPath}/Flipbooks/Premium_v002/{fileName}",
                true,
                lifetime,
                delay,
                startSize,
                rotation);
        }

        private static CompleteEffectDefinition CE(
            string cueId,
            string slug,
            string displayName,
            bool isPhysical,
            float reviewScale,
            float rotation)
        {
            string folder = isPhysical ? "Flipbooks" : "Textures";
            string[] layerNames = isPhysical
                ? new[] { "PrimaryTransient", "DirectionalBody", "Settle" }
                : new[] { "Primary", "Support", "Settle" };
            var layers = new CompleteLayerDefinition[3];
            for (int index = 0; index < layers.Length; index++)
            {
                string flipbookSuffix = isPhysical ? "_4x4" : string.Empty;
                string texturePath =
                    $"{ReviewRootPath}/{folder}/Complete_v003/" +
                    $"T_VFX_Showcase_{slug}_{layerNames[index]}{flipbookSuffix}_v003.png";
                float lifetime = isPhysical
                    ? new[] { 0.38f, 0.66f, 0.92f }[index]
                    : new[] { 0.72f, 0.90f, 1.08f }[index];
                float delay = isPhysical
                    ? new[] { 0.000f, 0.022f, 0.082f }[index]
                    : new[] { 0.000f, 0.055f, 0.115f }[index];
                float size = reviewScale * (isPhysical
                    ? new[] { 1.44f, 1.56f, 1.46f }[index]
                    : new[] { 1.38f, 1.46f, 1.40f }[index]);
                layers[index] = new CompleteLayerDefinition(
                    slug,
                    layerNames[index],
                    texturePath,
                    isPhysical,
                    lifetime,
                    delay,
                    size,
                    rotation + (index - 1) * 1.5f);
            }
            return new CompleteEffectDefinition(
                cueId,
                slug,
                displayName,
                isPhysical,
                isPhysical ? 1.34f : 1.58f,
                reviewScale,
                layers);
        }

        private static void ConfigureCompleteTextureImporter(CompleteLayerDefinition layer)
        {
            TextureImporter importer = AssetImporter.GetAtPath(layer.TexturePath) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    $"Required complete review texture is missing: {layer.TexturePath}");
            }
            importer.textureType = TextureImporterType.Default;
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.isReadable = false;
            importer.mipmapEnabled = layer.IsFlipbook;
            importer.borderMipmap = layer.IsFlipbook;
            importer.streamingMipmaps = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.anisoLevel = 1;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.crunchedCompression = false;
            importer.maxTextureSize = layer.IsFlipbook ? 2048 : 512;
            importer.SaveAndReimport();
        }

        private static Material CreateCompleteParticleMaterial(
            CompleteLayerDefinition layer,
            Shader shader)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(layer.TexturePath);
            if (texture == null)
            {
                throw new InvalidOperationException(
                    $"Imported complete review texture is unavailable: {layer.TexturePath}");
            }
            Material material = AssetDatabase.LoadAssetAtPath<Material>(layer.MaterialPath);
            if (material == null)
            {
                material = new Material(shader) { name = layer.MaterialName };
                AssetDatabase.CreateAsset(material, layer.MaterialPath);
            }
            else
            {
                material.shader = shader;
            }
            SetTextureIfPresent(material, "_MainTex", texture);
            SetColorIfPresent(material, "_Color", Color.white);
            SetFloatIfPresent(material, "_Mode", 2f);
            SetFloatIfPresent(material, "_ColorMode", 0f);
            SetFloatIfPresent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
            SetFloatIfPresent(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            SetFloatIfPresent(material, "_ZWrite", 0f);
            SetFloatIfPresent(material, "_Cull", (float)CullMode.Off);
            SetFloatIfPresent(material, "_AlphaClip", 0f);
            SetFloatIfPresent(material, "_SoftParticlesEnabled", 0f);
            SetFloatIfPresent(material, "_CameraFadingEnabled", 0f);
            SetFloatIfPresent(material, "_FlipbookMode", 0f);
            SetFloatIfPresent(material, "_LightingEnabled", 0f);
            SetFloatIfPresent(material, "_EmissionEnabled", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.DisableKeyword("_ALPHAMODULATE_ON");
            material.DisableKeyword("_SOFTPARTICLES_ON");
            material.DisableKeyword("_FADING_ON");
            material.renderQueue = (int)RenderQueue.Transparent;
            material.enableInstancing = false;
            material.doubleSidedGI = false;
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            material.SetShaderPassEnabled("ShadowCaster", false);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateCompletePrefab(
            CompleteEffectDefinition effect,
            IReadOnlyDictionary<string, Material> materials)
        {
            GameObject root = new GameObject(effect.PrefabName);
            try
            {
                for (int index = 0; index < effect.Layers.Length; index++)
                {
                    CompleteLayerDefinition layer = effect.Layers[index];
                    GameObject layerObject = new GameObject($"LAYER_{index + 1:00}_{layer.LayerName}");
                    layerObject.transform.SetParent(root.transform, false);
                    layerObject.transform.localPosition = new Vector3(0f, 0f, -0.012f * index);
                    layerObject.transform.localRotation = Quaternion.Euler(0f, 0f, layer.Rotation);
                    ParticleSystem system = layerObject.AddComponent<ParticleSystem>();
                    ConfigureCompleteParticleSystem(system, effect, layer, index, materials[layer.MaterialName]);
                }
                if (PrefabUtility.SaveAsPrefabAsset(root, effect.PrefabPath) == null)
                {
                    throw new InvalidOperationException(
                        $"Could not save complete review prefab: {effect.PrefabPath}");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void ConfigureCompleteParticleSystem(
            ParticleSystem particleSystem,
            CompleteEffectDefinition effect,
            CompleteLayerDefinition layer,
            int layerIndex,
            Material material)
        {
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = particleSystem.main;
            main.duration = effect.CycleDuration;
            main.loop = true;
            main.prewarm = false;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.simulationSpeed = 1f;
            main.startLifetime = layer.Lifetime;
            main.startSpeed = 0f;
            main.startSize = layer.StartSize;
            main.startRotation = 0f;
            main.startColor = Color.white;
            main.gravityModifier = 0f;
            main.maxParticles = 1;
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.rateOverDistance = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(layer.Delay, 1) });
            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.enabled = false;
            ConfigureLifetimeCurves(particleSystem, layer.IsFlipbook, layerIndex);
            ConfigureTextureSheetAnimation(particleSystem, layer.IsFlipbook);
            ParticleSystem.NoiseModule noise = particleSystem.noise;
            noise.enabled = false;
            ParticleSystem.CollisionModule collision = particleSystem.collision;
            collision.enabled = false;
            ParticleSystem.TriggerModule trigger = particleSystem.trigger;
            trigger.enabled = false;
            ParticleSystem.TrailModule trails = particleSystem.trails;
            trails.enabled = false;
            ParticleSystem.LightsModule lights = particleSystem.lights;
            lights.enabled = false;
            ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.sortMode = ParticleSystemSortMode.Distance;
            renderer.sortingOrder = 20 + layerIndex;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            renderer.minParticleSize = 0f;
            renderer.maxParticleSize = 1f;
            renderer.enableGPUInstancing = false;
        }

        private static void ConfigureTextureImporter(LayerDefinition layer)
        {
            TextureImporter importer = AssetImporter.GetAtPath(layer.TexturePath) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    $"Required premium review texture is missing: {layer.TexturePath}");
            }

            importer.textureType = TextureImporterType.Default;
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.isReadable = false;
            importer.mipmapEnabled = layer.IsFlipbook;
            importer.borderMipmap = layer.IsFlipbook;
            importer.streamingMipmaps = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.anisoLevel = 1;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.crunchedCompression = false;
            importer.maxTextureSize = layer.IsFlipbook ? 2048 : 512;
            importer.SaveAndReimport();
        }

        private static Material CreateParticleMaterial(
            LayerDefinition layer,
            Shader shader)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(layer.TexturePath);
            if (texture == null)
            {
                throw new InvalidOperationException(
                    $"Imported premium review texture is unavailable: {layer.TexturePath}");
            }

            string materialPath = layer.MaterialPath;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(shader) { name = layer.MaterialName };
                AssetDatabase.CreateAsset(material, materialPath);
            }
            else
            {
                material.shader = shader;
            }

            SetTextureIfPresent(material, "_MainTex", texture);
            SetColorIfPresent(material, "_Color", Color.white);
            SetFloatIfPresent(material, "_Mode", 2f);
            SetFloatIfPresent(material, "_ColorMode", 0f);
            SetFloatIfPresent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
            SetFloatIfPresent(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            SetFloatIfPresent(material, "_ZWrite", 0f);
            SetFloatIfPresent(material, "_Cull", (float)CullMode.Off);
            SetFloatIfPresent(material, "_AlphaClip", 0f);
            SetFloatIfPresent(material, "_SoftParticlesEnabled", 0f);
            SetFloatIfPresent(material, "_CameraFadingEnabled", 0f);
            SetFloatIfPresent(material, "_FlipbookMode", 0f);
            SetFloatIfPresent(material, "_LightingEnabled", 0f);
            SetFloatIfPresent(material, "_EmissionEnabled", 0f);

            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.DisableKeyword("_ALPHAMODULATE_ON");
            material.DisableKeyword("_SOFTPARTICLES_ON");
            material.DisableKeyword("_FADING_ON");
            material.renderQueue = (int)RenderQueue.Transparent;
            material.enableInstancing = false;
            material.doubleSidedGI = false;
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            material.SetShaderPassEnabled("ShadowCaster", false);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateCompositePrefab(
            EffectDefinition effect,
            VariationDefinition variation,
            IReadOnlyDictionary<string, Material> materials)
        {
            GameObject temporaryRoot = new GameObject(effect.PrefabName(variation));
            try
            {
                for (int index = 0; index < effect.Layers.Length; index++)
                {
                    LayerDefinition layer = effect.Layers[index];
                    GameObject layerObject = new GameObject($"LAYER_{index + 1:00}_{layer.Name}");
                    layerObject.transform.SetParent(temporaryRoot.transform, false);
                    float signedLayer = index - 1f;
                    layerObject.transform.localPosition = new Vector3(
                        variation.LayerOffsetX * signedLayer,
                        variation.LayerOffsetY * signedLayer,
                        -0.012f * index);
                    layerObject.transform.localRotation = Quaternion.Euler(
                        0f,
                        0f,
                        variation.Rotation + layer.Rotation + signedLayer * variation.Rotation * 0.12f);
                    layerObject.transform.localScale = Vector3.one *
                        (variation.Scale * (1f + signedLayer * 0.035f));

                    ParticleSystem particleSystem = layerObject.AddComponent<ParticleSystem>();
                    ConfigureParticleSystem(
                        particleSystem,
                        effect,
                        layer,
                        variation,
                        index,
                        materials[layer.Name]);
                }

                string prefabPath = effect.PrefabPath(variation);
                if (PrefabUtility.SaveAsPrefabAsset(temporaryRoot, prefabPath) == null)
                {
                    throw new InvalidOperationException(
                        $"Could not save premium review prefab: {prefabPath}");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(temporaryRoot);
            }
        }

        private static void ConfigureParticleSystem(
            ParticleSystem particleSystem,
            EffectDefinition effect,
            LayerDefinition layer,
            VariationDefinition variation,
            int layerIndex,
            Material material)
        {
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = particleSystem.main;
            main.duration = effect.CycleDuration;
            main.loop = true;
            main.prewarm = false;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.simulationSpeed = 1f;
            main.startLifetime = layer.Lifetime;
            main.startSpeed = 0f;
            main.startSize3D = false;
            main.startSize = layer.StartSize;
            main.startRotation = 0f;
            main.startColor = Color.white;
            main.gravityModifier = 0f;
            main.maxParticles = 1;
            main.stopAction = ParticleSystemStopAction.None;
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.rateOverDistance = 0f;
            float burstTime = layer.Delay + variation.DelayBias * layerIndex;
            emission.SetBursts(new[] { new ParticleSystem.Burst(burstTime, 1) });

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.enabled = false;
            ConfigureLifetimeCurves(particleSystem, layer.IsFlipbook, layerIndex);
            ConfigureTextureSheetAnimation(particleSystem, layer.IsFlipbook);

            ParticleSystem.NoiseModule noise = particleSystem.noise;
            noise.enabled = false;
            ParticleSystem.CollisionModule collision = particleSystem.collision;
            collision.enabled = false;
            ParticleSystem.TriggerModule trigger = particleSystem.trigger;
            trigger.enabled = false;
            ParticleSystem.TrailModule trails = particleSystem.trails;
            trails.enabled = false;
            ParticleSystem.LightsModule lights = particleSystem.lights;
            lights.enabled = false;

            ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.sortMode = ParticleSystemSortMode.Distance;
            renderer.sortingOrder = layerIndex;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            renderer.minParticleSize = 0f;
            renderer.maxParticleSize = 1f;
            renderer.enableGPUInstancing = false;
        }

        private static void ConfigureLifetimeCurves(
            ParticleSystem particleSystem,
            bool isFlipbook,
            int layerIndex)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                isFlipbook
                    ? new[]
                    {
                        new GradientAlphaKey(layerIndex == 0 ? 1f : 0.82f, 0f),
                        new GradientAlphaKey(1f, 0.12f),
                        new GradientAlphaKey(0.88f, 0.72f),
                        new GradientAlphaKey(0f, 1f)
                    }
                    : new[]
                    {
                        new GradientAlphaKey(0f, 0f),
                        new GradientAlphaKey(1f, 0.10f + 0.03f * layerIndex),
                        new GradientAlphaKey(0.88f, 0.66f),
                        new GradientAlphaKey(0f, 1f)
                    });

            ParticleSystem.ColorOverLifetimeModule color = particleSystem.colorOverLifetime;
            color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(gradient);

            float startScale = isFlipbook ? 0.88f : 0.94f - layerIndex * 0.02f;
            float endScale = isFlipbook ? 1.04f + layerIndex * 0.035f : 1.02f + layerIndex * 0.025f;
            AnimationCurve curve = new AnimationCurve(
                new Keyframe(0f, startScale),
                new Keyframe(0.34f, 1f),
                new Keyframe(1f, endScale));
            curve.preWrapMode = WrapMode.ClampForever;
            curve.postWrapMode = WrapMode.ClampForever;
            ParticleSystem.SizeOverLifetimeModule size = particleSystem.sizeOverLifetime;
            size.enabled = true;
            size.separateAxes = false;
            size.size = new ParticleSystem.MinMaxCurve(1f, curve);
        }

        private static void ConfigureTextureSheetAnimation(
            ParticleSystem particleSystem,
            bool isFlipbook)
        {
            ParticleSystem.TextureSheetAnimationModule sheet = particleSystem.textureSheetAnimation;
            sheet.enabled = isFlipbook;
            if (!isFlipbook)
            {
                return;
            }

            sheet.mode = ParticleSystemAnimationMode.Grid;
            sheet.numTilesX = 4;
            sheet.numTilesY = 4;
            sheet.animation = ParticleSystemAnimationType.WholeSheet;
            sheet.frameOverTime = new ParticleSystem.MinMaxCurve(
                1f,
                AnimationCurve.Linear(0f, 0f, 1f, 0.999f));
            sheet.startFrame = new ParticleSystem.MinMaxCurve(0f);
            sheet.cycleCount = 1;
        }

        private static Material LoadExistingPremiumReviewMaterial(string name)
        {
            string path = $"{PremiumRootPath}/Materials/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null || material.shader == null)
            {
                throw new InvalidOperationException(
                    $"Required existing premium review material is unavailable: {path}");
            }
            return material;
        }

        private static Material CreateSolidMaterial(string name, Color color)
        {
            string path = $"{PremiumRootPath}/Materials/{name}.mat";
            Shader shader = Shader.Find(BackdropShaderName);
            if (shader == null)
            {
                throw new InvalidOperationException(
                    $"Required built-in review shader is unavailable: {BackdropShaderName}");
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            SetColorIfPresent(material, "_Color", color);
            SetFloatIfPresent(material, "_ZWrite", 1f);
            material.SetOverrideTag("RenderType", "Opaque");
            material.renderQueue = (int)RenderQueue.Geometry;
            material.SetShaderPassEnabled("ShadowCaster", false);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject CreateReviewScene(
            Material backdrop,
            Material panel,
            Material floor,
            Material prop,
            Material binder,
            Material ally,
            Material exit)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject(SceneRootName);
            SceneManager.MoveGameObjectToScene(root, scene);
            root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            root.transform.localScale = Vector3.one;
            root.AddComponent<ShowcaseVfxReviewPlayback>();
            CreateReviewCamera(root.transform);

            CreateOldNewZone(scene, root.transform, Vector3.zero, backdrop, panel);
            CreateLayerZone(scene, root.transform, new Vector3(100f, 0f, 0f), backdrop, panel);
            CreateVariationZone(scene, root.transform, new Vector3(200f, 0f, 0f), backdrop, panel);
            CreateGameplayZone(
                scene,
                root.transform,
                new Vector3(300f, 0f, 0f),
                floor,
                prop,
                binder,
                ally,
                exit);
            CreateCompleteGalleryPages(
                scene,
                root.transform,
                backdrop,
                panel,
                floor,
                prop,
                binder,
                ally,
                exit);

            RenderSettings.fog = false;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.22f, 0.23f, 0.23f, 1f);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ReviewScenePath))
            {
                throw new InvalidOperationException(
                    $"Could not save isolated premium review scene: {ReviewScenePath}");
            }

            return root;
        }

        private static void CreateOldNewZone(
            Scene scene,
            Transform parent,
            Vector3 position,
            Material backdrop,
            Material panel)
        {
            Transform zone = CreateZone(parent, OldNewZoneName, position);
            CreatePrimitive(
                zone,
                "ZONE_BACKDROP",
                PrimitiveType.Cube,
                new Vector3(0f, 0.5f, 0.9f),
                new Vector3(21f, 9.8f, 0.12f),
                backdrop);
            CreateLabel(
                zone,
                "HEADER",
                "SHOWCASE VFX PREMIUM v002  /  OLD vs NEW  /  REVIEW - OWNER APPROVAL PENDING",
                new Vector3(0f, 4.82f, -0.25f),
                0.035f,
                new Color(0.78f, 0.82f, 0.82f, 1f));

            float[] columns = { -7.5f, -2.5f, 2.5f, 7.5f };
            float[] rows = { 2.55f, -1.55f };
            for (int index = 0; index < Effects.Length; index++)
            {
                EffectDefinition effect = Effects[index];
                GameObject station = new GameObject(effect.StationName);
                station.transform.SetParent(zone, false);
                station.transform.localPosition = new Vector3(
                    columns[index % 4],
                    rows[index / 4],
                    0f);
                CreatePrimitive(
                    station.transform,
                    "REVIEW_BACKDROP",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0f, 0.35f),
                    new Vector3(4.55f, 3.65f, 0.08f),
                    panel);
                CreateLabel(
                    station.transform,
                    "LABEL",
                    effect.DisplayName,
                    new Vector3(0f, 1.48f, -0.30f),
                    0.018f,
                    new Color(0.68f, 0.73f, 0.73f, 1f));
                CreateLabel(
                    station.transform,
                    "OLD_LABEL",
                    "OLD v001 / VISUALLY REJECTED",
                    new Vector3(-1.05f, -1.42f, -0.30f),
                    0.012f,
                    new Color(0.52f, 0.48f, 0.46f, 1f));
                CreateLabel(
                    station.transform,
                    "NEW_LABEL",
                    "PREMIUM v002 / REVIEW",
                    new Vector3(1.05f, -1.42f, -0.30f),
                    0.012f,
                    new Color(0.62f, 0.70f, 0.66f, 1f));

                GameObject oldInstance = InstantiatePrefab(
                    effect.OldPrefabPath,
                    scene,
                    station.transform,
                    "OLD_V001");
                oldInstance.transform.localPosition = new Vector3(-1.05f, 0f, -0.20f);
                oldInstance.transform.localScale = Vector3.one * 0.82f;

                GameObject newInstance = InstantiatePrefab(
                    effect.PrimaryPrefabPath,
                    scene,
                    station.transform,
                    "NEW_PREMIUM_V002_COMPOSITE");
                newInstance.transform.localPosition = new Vector3(1.05f, 0f, -0.24f);
                newInstance.transform.localScale = Vector3.one * 1.08f;
            }
        }

        private static void CreateLayerZone(
            Scene scene,
            Transform parent,
            Vector3 position,
            Material backdrop,
            Material panel)
        {
            Transform zone = CreateZone(parent, LayerZoneName, position);
            CreatePrimitive(
                zone,
                "ZONE_BACKDROP",
                PrimitiveType.Cube,
                new Vector3(0f, 0f, 0.9f),
                new Vector3(26f, 12.5f, 0.12f),
                backdrop);
            CreateLabel(
                zone,
                "HEADER",
                "SOLO LAYER 1 / 2 / 3  +  COMPOSITE  /  REVIEW",
                new Vector3(0f, 5.82f, -0.25f),
                0.038f,
                new Color(0.78f, 0.82f, 0.82f, 1f));

            float[] columns = { -6.15f, 6.15f };
            float[] rows = { 4.15f, 1.35f, -1.45f, -4.25f };
            string[] viewLabels = { "SOLO L1", "SOLO L2", "SOLO L3", "COMPOSITE" };
            float[] viewPositions = { -4.20f, -1.40f, 1.40f, 4.20f };

            for (int index = 0; index < Effects.Length; index++)
            {
                EffectDefinition effect = Effects[index];
                GameObject station = new GameObject("LAYER_" + effect.StationName);
                station.transform.SetParent(zone, false);
                station.transform.localPosition = new Vector3(
                    columns[index % 2],
                    rows[index / 2],
                    0f);
                CreatePrimitive(
                    station.transform,
                    "REVIEW_BACKDROP",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0f, 0.35f),
                    new Vector3(11.8f, 2.45f, 0.08f),
                    panel);
                CreateLabel(
                    station.transform,
                    "LABEL",
                    effect.DisplayName,
                    new Vector3(0f, 0.91f, -0.30f),
                    0.017f,
                    new Color(0.67f, 0.72f, 0.72f, 1f));

                for (int view = 0; view < 4; view++)
                {
                    GameObject instance = InstantiatePrefab(
                        effect.PrimaryPrefabPath,
                        scene,
                        station.transform,
                        view == 3 ? "COMPOSITE" : $"SOLO_LAYER_{view + 1:00}");
                    instance.transform.localPosition = new Vector3(viewPositions[view], 0f, -0.20f);
                    instance.transform.localScale = Vector3.one * 1.02f;
                    if (view < 3)
                    {
                        SetOnlyLayerActive(instance, view);
                    }
                    CreateLabel(
                        station.transform,
                        "VIEW_LABEL_" + view,
                        viewLabels[view],
                        new Vector3(viewPositions[view], -0.91f, -0.30f),
                        0.013f,
                        new Color(0.55f, 0.61f, 0.60f, 1f));
                }
            }
        }

        private static void CreateVariationZone(
            Scene scene,
            Transform parent,
            Vector3 position,
            Material backdrop,
            Material panel)
        {
            Transform zone = CreateZone(parent, VariationZoneName, position);
            CreatePrimitive(
                zone,
                "ZONE_BACKDROP",
                PrimitiveType.Cube,
                new Vector3(0f, 0.25f, 0.9f),
                new Vector3(21f, 10f, 0.12f),
                backdrop);
            CreateLabel(
                zone,
                "HEADER",
                "PHYSICAL EFFECT DETERMINISTIC VARIATION A / B / C  /  REVIEW",
                new Vector3(0f, 4.72f, -0.25f),
                0.037f,
                new Color(0.78f, 0.82f, 0.82f, 1f));

            EffectDefinition[] physical = Effects.Where(effect => effect.IsPhysical).ToArray();
            float[] columns = { -5.0f, 5.0f };
            float[] rows = { 2.35f, -2.05f };
            float[] viewPositions = { -2.85f, 0f, 2.85f };
            for (int index = 0; index < physical.Length; index++)
            {
                EffectDefinition effect = physical[index];
                GameObject station = new GameObject("VARIATION_" + effect.StationName);
                station.transform.SetParent(zone, false);
                station.transform.localPosition = new Vector3(
                    columns[index % 2],
                    rows[index / 2],
                    0f);
                CreatePrimitive(
                    station.transform,
                    "REVIEW_BACKDROP",
                    PrimitiveType.Cube,
                    new Vector3(0f, 0f, 0.35f),
                    new Vector3(9.5f, 3.75f, 0.08f),
                    panel);
                CreateLabel(
                    station.transform,
                    "LABEL",
                    effect.DisplayName,
                    new Vector3(0f, 1.48f, -0.30f),
                    0.020f,
                    new Color(0.68f, 0.73f, 0.73f, 1f));
                for (int variationIndex = 0; variationIndex < PhysicalVariations.Length; variationIndex++)
                {
                    VariationDefinition variation = PhysicalVariations[variationIndex];
                    GameObject instance = InstantiatePrefab(
                        effect.PrefabPath(variation),
                        scene,
                        station.transform,
                        "VARIATION_" + variation.Suffix);
                    instance.transform.localPosition = new Vector3(
                        viewPositions[variationIndex],
                        0f,
                        -0.20f);
                    instance.transform.localScale = Vector3.one * 1.26f;
                    CreateLabel(
                        station.transform,
                        "VAR_LABEL_" + variation.Suffix,
                        "VAR " + variation.Suffix,
                        new Vector3(viewPositions[variationIndex], -1.43f, -0.30f),
                        0.016f,
                        new Color(0.58f, 0.64f, 0.62f, 1f));
                }
            }
        }

        private static void CreateGameplayZone(
            Scene scene,
            Transform parent,
            Vector3 position,
            Material floor,
            Material prop,
            Material binder,
            Material ally,
            Material exit)
        {
            Transform zone = CreateZone(parent, GameplayZoneName, position);
            CreatePrimitive(zone, "OFFICE_FLOOR", PrimitiveType.Cube,
                new Vector3(0f, -0.18f, 4f), new Vector3(18f, 0.24f, 14f), floor);
            CreatePrimitive(zone, "BACK_WALL", PrimitiveType.Cube,
                new Vector3(0f, 3.2f, 10.8f), new Vector3(18f, 6.5f, 0.24f), prop);
            CreatePrimitive(zone, "LOW_PARTITION_LEFT", PrimitiveType.Cube,
                new Vector3(-5.6f, 1.05f, 4.0f), new Vector3(0.25f, 2.1f, 5.8f), prop);

            GameObject player = CreatePrimitive(zone, "PLAYER_PROXY", PrimitiveType.Capsule,
                new Vector3(-1.1f, 1.0f, 0.7f), new Vector3(0.72f, 1f, 0.72f), prop);
            player.transform.localRotation = Quaternion.Euler(0f, 20f, 0f);
            CreatePrimitive(player.transform, "HAND_PROXY", PrimitiveType.Cube,
                new Vector3(0.55f, 0.35f, 0.15f), new Vector3(0.42f, 0.18f, 0.22f), prop);

            CreatePrimitive(zone, "ALLY_PROXY", PrimitiveType.Capsule,
                new Vector3(4.8f, 1.0f, 5.9f), new Vector3(0.68f, 1f, 0.68f), ally);

            CreatePrimitive(zone, "BINDER_TARGET", PrimitiveType.Cube,
                new Vector3(-4.4f, 0.28f, 2.4f), new Vector3(0.75f, 0.17f, 1.05f), binder);
            CreatePrimitive(zone, "BINDER_IMPACT_PROXY", PrimitiveType.Cube,
                new Vector3(-2.5f, 0.26f, 6.8f), new Vector3(0.85f, 0.18f, 1.12f), binder);
            CreatePrimitive(zone, "PAPER_STACK_PROXY", PrimitiveType.Cube,
                new Vector3(0.6f, 0.18f, 7.9f), new Vector3(1.15f, 0.22f, 0.82f), binder);

            GameObject chair = new GameObject("CHAIR_PROXY");
            chair.transform.SetParent(zone, false);
            chair.transform.localPosition = new Vector3(2.45f, 0f, 4.15f);
            CreatePrimitive(chair.transform, "SEAT", PrimitiveType.Cube,
                new Vector3(0f, 0.75f, 0f), new Vector3(1.35f, 0.24f, 1.25f), prop);
            CreatePrimitive(chair.transform, "BACK", PrimitiveType.Cube,
                new Vector3(0f, 1.45f, 0.48f), new Vector3(1.25f, 1.28f, 0.20f), prop);
            CreatePrimitive(chair.transform, "BASE", PrimitiveType.Cube,
                new Vector3(0f, 0.26f, 0f), new Vector3(1.55f, 0.12f, 0.22f), prop);
            for (int wheel = 0; wheel < 4; wheel++)
            {
                float x = wheel < 2 ? -0.62f : 0.62f;
                float z = wheel % 2 == 0 ? -0.42f : 0.42f;
                GameObject caster = CreatePrimitive(chair.transform, "CASTER_" + wheel,
                    PrimitiveType.Cylinder, new Vector3(x, 0.12f, z),
                    new Vector3(0.16f, 0.08f, 0.16f), prop);
                caster.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }

            Transform doorway = new GameObject("EXIT_HARDWARE_PROXY").transform;
            doorway.SetParent(zone, false);
            doorway.localPosition = new Vector3(5.5f, 0f, 9.9f);
            CreatePrimitive(doorway, "LEFT_JAMB", PrimitiveType.Cube,
                new Vector3(-1.2f, 1.7f, 0f), new Vector3(0.18f, 3.4f, 0.24f), exit);
            CreatePrimitive(doorway, "RIGHT_JAMB", PrimitiveType.Cube,
                new Vector3(1.2f, 1.7f, 0f), new Vector3(0.18f, 3.4f, 0.24f), exit);
            CreatePrimitive(doorway, "HEADER", PrimitiveType.Cube,
                new Vector3(0f, 3.35f, 0f), new Vector3(2.55f, 0.18f, 0.24f), exit);

            PlaceGameplayEffect(scene, zone, Effects[0], new Vector3(-4.4f, 0.88f, 2.4f), 1.18f, 0f);
            PlaceGameplayEffect(scene, zone, Effects[1], new Vector3(-0.2f, 1.62f, 1.1f), 1.20f, 0f);
            PlaceGameplayEffect(scene, zone, Effects[2], new Vector3(-4.2f, 1.55f, 3.0f), 1.04f, -8f);
            PlaceGameplayEffect(scene, zone, Effects[3], new Vector3(4.8f, 2.45f, 5.9f), 1.22f, 0f);
            PlaceGameplayEffect(scene, zone, Effects[4], new Vector3(-2.1f, 0.48f, 6.8f), 1.28f, 0f);
            PlaceGameplayEffect(scene, zone, Effects[5], new Vector3(2.35f, 0.36f, 4.4f), 1.48f, 0f);
            PlaceGameplayEffect(scene, zone, Effects[6], new Vector3(0.9f, 0.82f, 7.9f), 1.30f, -5f);
            PlaceGameplayEffect(scene, zone, Effects[7], new Vector3(5.5f, 2.05f, 9.6f), 1.54f, 0f);
        }

        private static void CreateCompleteGalleryPages(
            Scene scene,
            Transform parent,
            Material backdrop,
            Material panel,
            Material floor,
            Material prop,
            Material binder,
            Material ally,
            Material exit)
        {
            CreateOwnerSelectedCorePage(
                scene,
                parent,
                new Vector3(400f, 0f, 0f),
                backdrop,
                panel);

            const int effectsPerPage = 4;
            int pageCount = Mathf.CeilToInt(CompleteEffects.Length / (float)effectsPerPage);
            for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
            {
                CompleteEffectDefinition[] pageEffects = CompleteEffects
                    .Skip(pageIndex * effectsPerPage)
                    .Take(effectsPerPage)
                    .ToArray();
                CreateCompleteEffectPage(
                    scene,
                    parent,
                    new Vector3(440f + pageIndex * 40f, 0f, 0f),
                    pageIndex + 1,
                    pageCount,
                    pageEffects,
                    backdrop,
                    panel);
            }

            CreateCompleteGameplayContextPage(
                scene,
                parent,
                new Vector3(920f, 0f, 0f),
                backdrop,
                floor,
                prop,
                binder,
                ally,
                exit);
        }

        private static void CreateOwnerSelectedCorePage(
            Scene scene,
            Transform parent,
            Vector3 position,
            Material backdrop,
            Material panel)
        {
            Transform page = CreateCompletePageShell(
                parent,
                CompletePagePrefix + "00_OWNER_SELECTED_CORE",
                position,
                "OWNER-SELECTED CORE  /  OLD v001 x7 + NEW EXIT v002  /  REVIEW",
                backdrop);
            float[] columns = { -7.5f, -2.5f, 2.5f, 7.5f };
            float[] rows = { 2.55f, -1.55f };
            for (int index = 0; index < Effects.Length; index++)
            {
                EffectDefinition effect = Effects[index];
                GameObject station = CreateGalleryStation(
                    page,
                    $"SELECTED_STATION_{index + 1:00}_{effect.EffectName}",
                    columns[index % 4],
                    rows[index / 4],
                    effect.DisplayName,
                    index == 7 ? "NEW v002 / OWNER SELECTED" : "OLD v001 / OWNER SELECTED",
                    panel);
                string path = index == 7 ? effect.PrimaryPrefabPath : effect.OldPrefabPath;
                GameObject instance = InstantiatePrefab(
                    path,
                    scene,
                    station.transform,
                    "OWNER_SELECTED_COMPOSITE");
                instance.transform.localPosition = new Vector3(0f, -0.05f, -0.22f);
                instance.transform.localScale = Vector3.one * (index == 7 ? 1.38f : 1.62f);
            }
        }

        private static void CreateCompleteEffectPage(
            Scene scene,
            Transform parent,
            Vector3 position,
            int pageIndex,
            int pageCount,
            IReadOnlyList<CompleteEffectDefinition> effects,
            Material backdrop,
            Material panel)
        {
            string first = effects[0].CueId;
            string last = effects[effects.Count - 1].CueId;
            Transform page = CreateCompletePageShell(
                parent,
                $"{CompletePagePrefix}{pageIndex:00}_{effects[0].Slug}",
                position,
                $"COMPLETE v003  /  PAGE {pageIndex}/{pageCount}  /  {first} - {last}  /  REVIEW",
                backdrop);
            float[] columns = { -5f, 5f };
            float[] rows = { 2.55f, -1.55f };
            for (int index = 0; index < effects.Count; index++)
            {
                CompleteEffectDefinition effect = effects[index];
                GameObject station = CreateGalleryStation(
                    page,
                    $"CUE_STATION_{effect.CueId}_{effect.Slug}",
                    columns[index % 2],
                    rows[index / 2],
                    effect.DisplayName,
                    effect.CueId + " / COMPOSITE LEFT + SOLO P-B-S RIGHT",
                    panel,
                    true);
                GameObject instance = InstantiatePrefab(
                    effect.PrefabPath,
                    scene,
                    station.transform,
                    "COMPLETE_V003_COMPOSITE");
                instance.transform.localPosition = new Vector3(-1.62f, -0.02f, -0.24f);
                instance.transform.localScale = Vector3.one *
                    (effect.IsPhysical ? 2.35f : 1.82f);
                CreateCompleteSoloLayers(scene, station.transform, effect);
            }
        }

        private static void CreateCompleteSoloLayers(
            Scene scene,
            Transform station,
            CompleteEffectDefinition effect)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(effect.PrefabPath);
            if (prefab == null || prefab.transform.childCount != 3)
            {
                throw new InvalidOperationException(
                    $"Complete solo-layer source prefab is invalid: {effect.PrefabPath}");
            }

            float[] rows = { 0.82f, 0f, -0.82f };
            for (int layerIndex = 0; layerIndex < 3; layerIndex++)
            {
                Transform source = prefab.transform.GetChild(layerIndex);
                GameObject solo = UnityEngine.Object.Instantiate(source.gameObject);
                solo.name = $"SOLO_LAYER_{layerIndex + 1:00}_{effect.Layers[layerIndex].LayerName}";
                SceneManager.MoveGameObjectToScene(solo, scene);
                solo.transform.SetParent(station, false);
                solo.transform.localPosition = new Vector3(2.35f, rows[layerIndex], -0.26f);
                solo.transform.localRotation = source.localRotation;
                solo.transform.localScale = Vector3.one *
                    (effect.IsPhysical ? 0.98f : 0.72f);
                solo.SetActive(true);
            }
        }

        private static void CreateCompleteGameplayContextPage(
            Scene scene,
            Transform parent,
            Vector3 position,
            Material backdrop,
            Material floor,
            Material prop,
            Material binder,
            Material ally,
            Material exit)
        {
            Transform page = CreateCompletePageShell(
                parent,
                CompletePagePrefix + "13_GAMEPLAY_CAMERA_IMPACT_CONTEXT",
                position,
                "COMPLETE v003 / GAMEPLAY-CAMERA IMPACT CONTEXT / REVIEW",
                backdrop);
            Transform pageBackdrop = page.Find("PAGE_BACKDROP");
            pageBackdrop.localPosition = new Vector3(0f, 2.4f, 12f);
            pageBackdrop.localScale = new Vector3(18f, 7f, 0.12f);
            Transform cameraAnchor = page.Find("CAMERA_ANCHOR");
            Transform cameraTarget = page.Find("CAMERA_TARGET");
            cameraAnchor.localPosition = new Vector3(0f, 4.65f, -8.6f);
            cameraTarget.localPosition = new Vector3(0f, 1.25f, 4.8f);

            CreatePrimitive(page, "OFFICE_FLOOR", PrimitiveType.Cube,
                new Vector3(0f, -0.18f, 5.4f), new Vector3(17f, 0.22f, 14f), floor);
            CreatePrimitive(page, "LEFT_PARTITION", PrimitiveType.Cube,
                new Vector3(-7.8f, 1.6f, 5.8f), new Vector3(0.18f, 3.2f, 11f), prop);
            CreatePrimitive(page, "RIGHT_PARTITION", PrimitiveType.Cube,
                new Vector3(7.8f, 1.6f, 5.8f), new Vector3(0.18f, 3.2f, 11f), prop);
            CreatePrimitive(page, "PLAYER_PROXY", PrimitiveType.Capsule,
                new Vector3(-2.8f, 1.0f, 0.7f), new Vector3(0.72f, 1.0f, 0.72f), prop);
            CreatePrimitive(page, "ALLY_PROXY", PrimitiveType.Capsule,
                new Vector3(3.5f, 1.0f, 5.5f), new Vector3(0.68f, 1.0f, 0.68f), ally);
            CreatePrimitive(page, "RUNNER_PROXY_A", PrimitiveType.Capsule,
                new Vector3(-0.6f, 1.0f, 2.6f), new Vector3(0.68f, 1.0f, 0.68f), prop);
            CreatePrimitive(page, "RUNNER_PROXY_B", PrimitiveType.Capsule,
                new Vector3(1.35f, 1.0f, 4.1f), new Vector3(0.68f, 1.0f, 0.68f), prop);
            CreatePrimitive(page, "RUNNER_PROXY_C", PrimitiveType.Capsule,
                new Vector3(-1.2f, 1.0f, 6.2f), new Vector3(0.68f, 1.0f, 0.68f), prop);
            CreatePrimitive(page, "DESK_PROXY", PrimitiveType.Cube,
                new Vector3(-4.4f, 0.65f, 5.2f), new Vector3(2.3f, 1.3f, 1.1f), prop);
            CreatePrimitive(page, "BINDER_PROXY", PrimitiveType.Cube,
                new Vector3(-3.75f, 1.38f, 4.95f), new Vector3(0.72f, 0.12f, 0.52f), binder);
            CreatePrimitive(page, "DRAWER_PROXY", PrimitiveType.Cube,
                new Vector3(3.55f, 0.72f, 7.35f), new Vector3(1.35f, 1.44f, 1.25f), prop);
            Transform chair = new GameObject("CHAIR_PROXY").transform;
            chair.SetParent(page, false);
            chair.localPosition = new Vector3(2.2f, 0f, 3.6f);
            CreatePrimitive(chair, "SEAT", PrimitiveType.Cube,
                new Vector3(0f, 0.76f, 0f), new Vector3(1.15f, 0.16f, 1.05f), prop);
            CreatePrimitive(chair, "BACK", PrimitiveType.Cube,
                new Vector3(0f, 1.38f, 0.46f), new Vector3(1.12f, 1.18f, 0.14f), prop);
            CreatePrimitive(chair, "CASTER_LEFT", PrimitiveType.Cylinder,
                new Vector3(-0.42f, 0.12f, -0.32f), new Vector3(0.16f, 0.08f, 0.16f), prop)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            CreatePrimitive(chair, "CASTER_RIGHT", PrimitiveType.Cylinder,
                new Vector3(0.42f, 0.12f, -0.32f), new Vector3(0.16f, 0.08f, 0.16f), prop)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Transform doorway = new GameObject("EXIT_HARDWARE_PROXY").transform;
            doorway.SetParent(page, false);
            doorway.localPosition = new Vector3(5.35f, 0f, 10.6f);
            CreatePrimitive(doorway, "LEFT_JAMB", PrimitiveType.Cube,
                new Vector3(-1.15f, 1.7f, 0f), new Vector3(0.18f, 3.4f, 0.24f), exit);
            CreatePrimitive(doorway, "RIGHT_JAMB", PrimitiveType.Cube,
                new Vector3(1.15f, 1.7f, 0f), new Vector3(0.18f, 3.4f, 0.24f), exit);
            CreatePrimitive(doorway, "HEADER", PrimitiveType.Cube,
                new Vector3(0f, 3.35f, 0f), new Vector3(2.48f, 0.18f, 0.24f), exit);

            PlaceCompleteGameplayEffect(scene, page, 1, "PunchLightCloth",
                new Vector3(-0.58f, 1.38f, 2.35f), 1.10f, 8f);
            PlaceCompleteGameplayEffect(scene, page, 2, "PunchMediumCloth",
                new Vector3(1.28f, 1.42f, 3.78f), 1.18f, 3f);
            PlaceCompleteGameplayEffect(scene, page, 3, "PunchStrongNonlethal",
                new Vector3(-1.12f, 1.48f, 5.92f), 1.30f, -4f);
            PlaceCompleteGameplayEffect(scene, page, 4, "PunchBraceBlock",
                new Vector3(0.18f, 1.62f, 6.75f), 1.18f, -14f);
            PlaceCompleteGameplayEffect(scene, page, 5, "BinderPhysicalContact",
                new Vector3(-3.72f, 1.34f, 4.70f), 1.08f, 16f);
            PlaceCompleteGameplayEffect(scene, page, 6, "ChairWheelFloorContact",
                new Vector3(2.18f, 0.24f, 3.25f), 1.20f, -2f);
            PlaceCompleteGameplayEffect(scene, page, 7, "DrawerCausalImpact",
                new Vector3(3.32f, 1.02f, 6.95f), 1.24f, 5f);
            PlaceCompleteGameplayEffect(scene, page, 8, "ExitActivation",
                new Vector3(5.35f, 2.18f, 10.35f), 1.36f, 0f);
        }

        private static void PlaceCompleteGameplayEffect(
            Scene scene,
            Transform parent,
            int index,
            string slug,
            Vector3 localPosition,
            float scale,
            float rotation)
        {
            CompleteEffectDefinition effect = CompleteEffects.Single(item => item.Slug == slug);
            GameObject instance = InstantiatePrefab(
                effect.PrefabPath,
                scene,
                parent,
                $"GAMEPLAY_EFFECT_{index:00}_{slug}");
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            instance.transform.localScale = Vector3.one * scale *
                (effect.IsPhysical ? 1.65f : 1.15f);
        }

        private static Transform CreateCompletePageShell(
            Transform parent,
            string name,
            Vector3 position,
            string header,
            Material backdrop)
        {
            Transform page = CreateZone(parent, name, position);
            CreatePrimitive(
                page,
                "PAGE_BACKDROP",
                PrimitiveType.Cube,
                new Vector3(0f, 0.5f, 0.9f),
                new Vector3(21f, 9.8f, 0.12f),
                backdrop);
            CreateLabel(
                page,
                "HEADER",
                header,
                new Vector3(0f, 4.82f, -0.25f),
                0.033f,
                new Color(0.80f, 0.84f, 0.83f, 1f));
            CreateLabel(
                page,
                "FOOTER",
                "AUTO TOUR IN PLAY MODE  /  PRIMARY TRANSIENT > DIRECTIONAL BODY > SETTLE  /  GAMEPLAY HOOK 0",
                new Vector3(0f, -4.64f, -0.25f),
                0.019f,
                new Color(0.49f, 0.57f, 0.55f, 1f));
            Transform cameraAnchor = new GameObject("CAMERA_ANCHOR").transform;
            cameraAnchor.SetParent(page, false);
            cameraAnchor.localPosition = new Vector3(0f, 0.65f, -19.5f);
            cameraAnchor.localRotation = Quaternion.identity;
            Transform cameraTarget = new GameObject("CAMERA_TARGET").transform;
            cameraTarget.SetParent(page, false);
            cameraTarget.localPosition = new Vector3(0f, 0.55f, 0f);
            cameraTarget.localRotation = Quaternion.identity;
            return page;
        }

        private static GameObject CreateGalleryStation(
            Transform page,
            string name,
            float x,
            float y,
            string displayName,
            string status,
            Material panel,
            bool wide = false)
        {
            GameObject station = new GameObject(name);
            station.transform.SetParent(page, false);
            station.transform.localPosition = new Vector3(x, y, 0f);
            CreatePrimitive(
                station.transform,
                "REVIEW_BACKDROP",
                PrimitiveType.Cube,
                new Vector3(0f, 0f, 0.35f),
                new Vector3(wide ? 9.35f : 4.55f, 3.65f, 0.08f),
                panel);
            CreateLabel(
                station.transform,
                "LABEL",
                displayName,
                new Vector3(0f, 1.48f, -0.30f),
                0.016f,
                new Color(0.70f, 0.75f, 0.74f, 1f));
            CreateLabel(
                station.transform,
                "STATUS",
                status,
                new Vector3(0f, -1.42f, -0.30f),
                0.012f,
                new Color(0.56f, 0.64f, 0.61f, 1f));
            return station;
        }

        private static void PlaceGameplayEffect(
            Scene scene,
            Transform parent,
            EffectDefinition effect,
            Vector3 localPosition,
            float scale,
            float rotation)
        {
            GameObject instance = InstantiatePrefab(
                effect.PrimaryPrefabPath,
                scene,
                parent,
                "GAMEPLAY_EQUIVALENT_" + effect.EffectName);
            instance.transform.localPosition = localPosition;
            instance.transform.localScale = Vector3.one * scale;
            instance.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
        }

        private static Transform CreateZone(Transform parent, string name, Vector3 position)
        {
            GameObject zone = new GameObject(name);
            zone.transform.SetParent(parent, false);
            zone.transform.localPosition = position;
            zone.transform.localRotation = Quaternion.identity;
            zone.transform.localScale = Vector3.one;
            return zone.transform;
        }

        private static GameObject InstantiatePrefab(
            string prefabPath,
            Scene scene,
            Transform parent,
            string instanceName)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException(
                    $"Required isolated review prefab is unavailable: {prefabPath}");
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException(
                    $"Could not instantiate isolated review prefab: {prefabPath}");
            }
            instance.name = instanceName;
            instance.transform.SetParent(parent, false);
            return instance;
        }

        private static void SetOnlyLayerActive(GameObject instance, int activeLayer)
        {
            for (int index = 0; index < instance.transform.childCount; index++)
            {
                instance.transform.GetChild(index).gameObject.SetActive(index == activeLayer);
            }
        }

        private static GameObject CreatePrimitive(
            Transform parent,
            string name,
            PrimitiveType type,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            GameObject result = GameObject.CreatePrimitive(type);
            result.name = name;
            result.transform.SetParent(parent, false);
            result.transform.localPosition = localPosition;
            result.transform.localScale = localScale;
            Collider collider = result.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
            MeshRenderer renderer = result.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            return result;
        }

        private static void CreateReviewCamera(Transform parent)
        {
            GameObject cameraObject = new GameObject("LOCAL_REVIEW_ONLY_CAMERA");
            cameraObject.transform.SetParent(parent, false);
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            ConfigureReviewCamera(camera);
        }

        private static void ConfigureReviewCamera(Camera camera)
        {
            camera.transform.SetPositionAndRotation(
                ReviewCameraPosition,
                ReviewCameraRotation);
            camera.transform.localScale = Vector3.one;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.016f, 0.019f, 0.020f, 1f);
            camera.fieldOfView = ReviewCameraFieldOfView;
            camera.nearClipPlane = ReviewCameraNearClip;
            camera.farClipPlane = ReviewCameraFarClip;
            camera.allowHDR = true;
            camera.allowMSAA = true;
            camera.depth = 0f;
        }

        private static void CreateLabel(
            Transform parent,
            string objectName,
            string text,
            Vector3 localPosition,
            float characterSize,
            Color color)
        {
            GameObject labelObject = new GameObject(objectName);
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = localPosition;
            labelObject.transform.localRotation = Quaternion.identity;
            labelObject.transform.localScale = Vector3.one;
            TextMesh label = labelObject.AddComponent<TextMesh>();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.font = font;
            label.text = text;
            label.fontSize = 64;
            label.characterSize = characterSize;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = color;
            MeshRenderer renderer = labelObject.GetComponent<MeshRenderer>();
            if (font != null)
            {
                renderer.sharedMaterial = font.material;
            }
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        }

        private static void SimulateAllParticles(GameObject root, float time)
        {
            foreach (ParticleSystem particleSystem in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particleSystem.Simulate(time, true, true, false);
            }
        }

        private static Texture2D RenderCamera(Camera camera, int width, int height)
        {
            RenderTexture renderTexture = new RenderTexture(
                width,
                height,
                24,
                RenderTextureFormat.ARGB32)
            {
                name = "LOCAL_REVIEW_ONLY_ShowcaseVFX_PremiumCapture",
                antiAliasing = 1
            };
            Texture2D capture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            RenderTexture previous = RenderTexture.active;
            Rect previousRect = camera.rect;
            float previousAspect = camera.aspect;
            try
            {
                renderTexture.Create();
                camera.targetTexture = renderTexture;
                camera.rect = new Rect(0f, 0f, 1f, 1f);
                camera.aspect = width / (float)height;
                camera.ResetProjectionMatrix();
                camera.Render();
                RenderTexture.active = renderTexture;
                capture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, false);
                capture.Apply(false, false);
                return capture;
            }
            finally
            {
                camera.targetTexture = null;
                camera.rect = previousRect;
                camera.aspect = previousAspect;
                camera.ResetProjectionMatrix();
                RenderTexture.active = previous;
                renderTexture.Release();
                DestroyReviewObject(renderTexture);
            }
        }

        private static GameObject FindReviewRoot(Scene scene)
        {
            GameObject root = scene.GetRootGameObjects()
                .SingleOrDefault(item => item.name == SceneRootName);
            if (root == null)
            {
                throw new InvalidOperationException(
                    $"Review root is unavailable in scene: {scene.path}");
            }

            return root;
        }

        private static string RequireOutputPath(string argumentName)
        {
            string outputPath = ReadCommandLineValue(argumentName);
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                throw new ArgumentException(
                    $"An output path is required through {argumentName}.");
            }

            string fullPath = Path.GetFullPath(outputPath);
            string directory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new InvalidOperationException(
                    $"Capture output has no parent directory: {fullPath}");
            }

            Directory.CreateDirectory(directory);
            return fullPath;
        }

        private static SceneView FrameSceneViewForReview(GameObject root, bool focus)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            SceneView view = SceneView.lastActiveSceneView ?? EditorWindow.GetWindow<SceneView>();
            if (view == null)
            {
                throw new InvalidOperationException("Scene View is unavailable for review framing.");
            }

            Selection.activeGameObject = root;
            view.in2DMode = false;
            view.orthographic = false;
            view.sceneLighting = false;
            view.LookAtDirect(
                ReviewCameraTarget,
                ReviewCameraRotation,
                ReviewSceneViewSize);
            view.Repaint();
            if (focus && !Application.isBatchMode)
            {
                view.Focus();
            }

            return view;
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (string.IsNullOrWhiteSpace(ReadCommandLineValue(ActualPlayCaptureArgument)))
            {
                return;
            }

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                ScheduleActualPlayCapture();
            }
            else if (
                state == PlayModeStateChange.EnteredEditMode &&
                SessionState.GetBool(ActualPlayCaptureCompleteKey, false))
            {
                EditorApplication.delayCall += ExitAfterActualPlayCapture;
            }
        }

        private static void ScheduleActualPlayCapture()
        {
            if (actualPlayCaptureScheduled)
            {
                return;
            }

            actualPlayCaptureScheduled = true;
            actualPlayCaptureEnteredAt = Time.unscaledTimeAsDouble;
            EditorApplication.update -= ActualPlayCaptureTick;
            EditorApplication.update += ActualPlayCaptureTick;
        }

        private static void ActualPlayCaptureTick()
        {
            if (!EditorApplication.isPlaying ||
                Time.unscaledTimeAsDouble - actualPlayCaptureEnteredAt <
                ActualPlayCaptureDelay)
            {
                return;
            }

            EditorApplication.update -= ActualPlayCaptureTick;
            actualPlayCaptureScheduled = false;
            int exitCode = 1;
            try
            {
                CaptureAndValidateActualPlayFrame();
                exitCode = 0;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                SessionState.SetInt(ActualPlayCaptureExitCodeKey, exitCode);
                SessionState.SetBool(ActualPlayCaptureCompleteKey, true);
                EditorApplication.isPlaying = false;
            }
        }

        private static void CaptureAndValidateActualPlayFrame()
        {
            string outputPath = RequireOutputPath(ActualPlayCaptureArgument);
            Scene scene = SceneManager.GetActiveScene();
            GameObject root = FindReviewRoot(scene);
            Camera camera = root.GetComponentsInChildren<Camera>(true).Single();
            ConfigureReviewCamera(camera);
            camera.aspect = 16f / 9f;
            camera.ResetProjectionMatrix();

            Transform[] pages = root.transform.Cast<Transform>()
                .Where(child => child.name.StartsWith(CompletePagePrefix, StringComparison.Ordinal))
                .OrderBy(child => child.name, StringComparer.Ordinal)
                .ToArray();
            if (pages.Length != 14)
            {
                throw new InvalidOperationException(
                    $"Complete review page count is not 14: {pages.Length}");
            }

            Plane[] frustum = GeometryUtility.CalculateFrustumPlanes(camera);
            Transform[] selectedStations = pages[0].Cast<Transform>()
                .Where(child => child.name.StartsWith("SELECTED_STATION_", StringComparison.Ordinal))
                .ToArray();
            int visibleSelected = selectedStations.Count(station =>
            {
                Renderer backdrop = station.GetComponentsInChildren<Renderer>(true)
                    .Single(item => item.gameObject.name == "REVIEW_BACKDROP");
                return GeometryUtility.TestPlanesAABB(frustum, backdrop.bounds);
            });
            int playingSelected = selectedStations.Count(station =>
            {
                ParticleSystem[] systems = station.GetComponentsInChildren<ParticleSystem>(false);
                return systems.Length > 0 && systems.All(system => system.isPlaying);
            });
            int liveSelected = selectedStations.Count(station =>
                station.GetComponentsInChildren<ParticleSystem>(false)
                    .Any(system => system.particleCount > 0));

            Transform[] completeComposites = pages
                .SelectMany(page => page.GetComponentsInChildren<Transform>(true))
                .Where(item => item.name == "COMPLETE_V003_COMPOSITE")
                .ToArray();
            Transform[] completeSoloLayers = pages
                .SelectMany(page => page.GetComponentsInChildren<Transform>(true))
                .Where(item => item.name.StartsWith("SOLO_LAYER_", StringComparison.Ordinal))
                .ToArray();
            int playingStations = 0;
            int liveStations = 0;
            int activeSystems = 0;
            int systemsWithParticles = 0;
            foreach (Transform composite in completeComposites)
            {
                ParticleSystem[] systems = composite.GetComponentsInChildren<ParticleSystem>(false);
                if (systems.Length != 3)
                {
                    throw new InvalidOperationException(
                        $"Complete composite layer count is not 3: {composite.parent.name}={systems.Length}");
                }
                bool allPlaying = true;
                bool anyLive = false;
                foreach (ParticleSystem system in systems)
                {
                    bool playing = system.gameObject.activeInHierarchy && system.isPlaying;
                    allPlaying &= playing;
                    activeSystems += playing ? 1 : 0;
                    bool hasParticles = system.particleCount > 0;
                    anyLive |= hasParticles;
                    systemsWithParticles += hasParticles ? 1 : 0;
                }
                playingStations += allPlaying ? 1 : 0;
                liveStations += anyLive ? 1 : 0;
            }

            int playingSoloSystems = completeSoloLayers.Count(layer =>
            {
                ParticleSystem[] systems = layer.GetComponentsInChildren<ParticleSystem>(false);
                return systems.Length == 1 && systems[0].isPlaying;
            });
            Transform[] gameplayEffects = pages[13].Cast<Transform>()
                .Where(item => item.name.StartsWith("GAMEPLAY_EFFECT_", StringComparison.Ordinal))
                .ToArray();
            int playingGameplayEffects = gameplayEffects.Count(effect =>
            {
                ParticleSystem[] systems = effect.GetComponentsInChildren<ParticleSystem>(false);
                return systems.Length == 3 && systems.All(system => system.isPlaying);
            });

            if (selectedStations.Length != 8 || visibleSelected != 8 ||
                playingSelected != 8 || liveSelected != 8 ||
                completeComposites.Length != 48 || playingStations != 48 ||
                completeSoloLayers.Length != 144 || playingSoloSystems != 144 ||
                gameplayEffects.Length != 8 || playingGameplayEffects != 8 ||
                liveStations != 48 || activeSystems != 144 || systemsWithParticles < 48)
            {
                throw new InvalidOperationException(
                    "Complete actual Play review contract failed: " +
                    $"pages={pages.Length}/14, selected={selectedStations.Length}/8, " +
                    $"visibleSelected={visibleSelected}/8, complete={completeComposites.Length}/48, " +
                    $"playingSelected={playingSelected}/8, liveSelected={liveSelected}/8, " +
                    $"playing={playingStations}/48, live={liveStations}/48, " +
                    $"soloPlaying={playingSoloSystems}/144, " +
                    $"gameplayPlaying={playingGameplayEffects}/8, " +
                    $"activeSystems={activeSystems}/144, " +
                    $"systemsWithParticles={systemsWithParticles}/144 (instantaneous minimum 48).");
            }

            Debug.Log(
                "Complete Showcase VFX actual Play camera contract: " +
                $"position={camera.transform.position}; fov={camera.fieldOfView}; " +
                $"aspect={camera.aspect}; rect={camera.rect}; " +
                $"rootScale={root.transform.lossyScale}; pageScale={pages[0].lossyScale}; " +
                $"screen={Screen.width}x{Screen.height}.");

            GameObject captureCameraObject = new GameObject(
                "LOCAL_REVIEW_ONLY_ACTUAL_PLAY_CAPTURE_CAMERA");
            Camera captureCamera = captureCameraObject.AddComponent<Camera>();
            captureCamera.CopyFrom(camera);
            captureCamera.transform.SetPositionAndRotation(
                camera.transform.position,
                camera.transform.rotation);
            captureCamera.enabled = false;
            captureCamera.aspect = 16f / 9f;
            captureCamera.ResetProjectionMatrix();
            Texture2D capture = RenderCamera(captureCamera, 1920, 1080);
            try
            {
                File.WriteAllBytes(outputPath, capture.EncodeToPNG());
            }
            finally
            {
                DestroyReviewObject(capture);
                DestroyReviewObject(captureCameraObject);
            }

            Debug.Log(
                "Complete Showcase VFX actual Play capture PASS: " +
                $"{outputPath}; pages=14/14; selectedVisible=8/8; " +
                $"selectedPlaying={playingSelected}/8; selectedLive={liveSelected}/8; " +
                $"completeStations=48/48; playing=48/48; live=48/48; " +
                $"soloPlaying={playingSoloSystems}/144; gameplayPlaying={playingGameplayEffects}/8; " +
                $"activeSystems=144/144; systemsWithParticles={systemsWithParticles}/144; " +
                "gameplayIntegration=0.");
        }

        private static void ExitAfterActualPlayCapture()
        {
            EditorApplication.delayCall -= ExitAfterActualPlayCapture;
            int exitCode = SessionState.GetInt(ActualPlayCaptureExitCodeKey, 1);
            EditorApplication.Exit(exitCode);
        }

        private static void DestroyReviewObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(target);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static Texture2D CreateGrayscale(Texture2D source)
        {
            Texture2D result = new Texture2D(
                source.width,
                source.height,
                TextureFormat.RGBA32,
                false);
            Color32[] pixels = source.GetPixels32();
            for (int index = 0; index < pixels.Length; index++)
            {
                Color32 pixel = pixels[index];
                byte luminance = (byte)Mathf.Clamp(
                    Mathf.RoundToInt(pixel.r * 0.2126f + pixel.g * 0.7152f + pixel.b * 0.0722f),
                    0,
                    255);
                pixels[index] = new Color32(luminance, luminance, luminance, pixel.a);
            }
            result.SetPixels32(pixels);
            result.Apply(false, false);
            return result;
        }

        private static void EnsureAssetFolder(string assetPath)
        {
            string[] segments = assetPath.Split('/');
            if (segments.Length == 0 || !string.Equals(segments[0], "Assets", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Invalid Unity asset folder path: {assetPath}");
            }
            string current = segments[0];
            for (int index = 1; index < segments.Length; index++)
            {
                string next = $"{current}/{segments[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[index]);
                }
                current = next;
            }
        }

        private static void SetTextureIfPresent(Material material, string propertyName, Texture texture)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetTexture(propertyName, texture);
            }
        }

        private static void SetColorIfPresent(Material material, string propertyName, Color color)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, color);
            }
        }

        private static void SetFloatIfPresent(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static string ReadCommandLineValue(string argumentName)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int index = 0; index < arguments.Length - 1; index++)
            {
                if (string.Equals(arguments[index], argumentName, StringComparison.OrdinalIgnoreCase))
                {
                    return arguments[index + 1];
                }
            }
            return null;
        }

        private sealed class LayerDefinition
        {
            public LayerDefinition(
                string name,
                string texturePath,
                bool isFlipbook,
                float lifetime,
                float delay,
                float startSize,
                float rotation)
            {
                Name = name;
                TexturePath = texturePath;
                IsFlipbook = isFlipbook;
                Lifetime = lifetime;
                Delay = delay;
                StartSize = startSize;
                Rotation = rotation;
            }

            public string Name { get; }
            public string TexturePath { get; }
            public bool IsFlipbook { get; }
            public float Lifetime { get; }
            public float Delay { get; }
            public float StartSize { get; }
            public float Rotation { get; }
            public string MaterialName => "M_VFX_Showcase_Premium_" + Name + "_v002";
            public string MaterialPath => PremiumRootPath + "/Materials/" + MaterialName + ".mat";
        }

        private sealed class EffectDefinition
        {
            public EffectDefinition(
                string stationName,
                string displayName,
                string effectName,
                bool isPhysical,
                float cycleDuration,
                float reviewScale,
                string oldPrefabPath,
                LayerDefinition[] layers)
            {
                StationName = stationName;
                DisplayName = displayName;
                EffectName = effectName;
                IsPhysical = isPhysical;
                CycleDuration = cycleDuration;
                ReviewScale = reviewScale;
                OldPrefabPath = oldPrefabPath;
                Layers = layers;
            }

            public string StationName { get; }
            public string DisplayName { get; }
            public string EffectName { get; }
            public bool IsPhysical { get; }
            public float CycleDuration { get; }
            public float ReviewScale { get; }
            public string OldPrefabPath { get; }
            public LayerDefinition[] Layers { get; }

            public string PrefabName(VariationDefinition variation)
            {
                string suffix = IsPhysical ? "_VAR_" + variation.Suffix : string.Empty;
                return $"PF_VFX_Showcase_{EffectName}_PREMIUM_REVIEW_ONLY{suffix}_v002";
            }

            public string PrefabPath(VariationDefinition variation)
            {
                return PremiumRootPath + "/Prefabs/" + PrefabName(variation) + ".prefab";
            }

            public string PrimaryPrefabPath => PrefabPath(
                IsPhysical ? PhysicalVariations[0] : VariationDefinition.Static);
        }

        private sealed class CompleteLayerDefinition
        {
            public CompleteLayerDefinition(
                string effectSlug,
                string layerName,
                string texturePath,
                bool isFlipbook,
                float lifetime,
                float delay,
                float startSize,
                float rotation)
            {
                EffectSlug = effectSlug;
                LayerName = layerName;
                TexturePath = texturePath;
                IsFlipbook = isFlipbook;
                Lifetime = lifetime;
                Delay = delay;
                StartSize = startSize;
                Rotation = rotation;
            }

            public string EffectSlug { get; }
            public string LayerName { get; }
            public string TexturePath { get; }
            public bool IsFlipbook { get; }
            public float Lifetime { get; }
            public float Delay { get; }
            public float StartSize { get; }
            public float Rotation { get; }
            public string MaterialName =>
                $"M_VFX_Showcase_Complete_{EffectSlug}_{LayerName}_v003";
            public string MaterialPath =>
                CompleteRootPath + "/Materials/" + MaterialName + ".mat";
        }

        private sealed class CompleteEffectDefinition
        {
            public CompleteEffectDefinition(
                string cueId,
                string slug,
                string displayName,
                bool isPhysical,
                float cycleDuration,
                float reviewScale,
                CompleteLayerDefinition[] layers)
            {
                CueId = cueId;
                Slug = slug;
                DisplayName = displayName;
                IsPhysical = isPhysical;
                CycleDuration = cycleDuration;
                ReviewScale = reviewScale;
                Layers = layers;
            }

            public string CueId { get; }
            public string Slug { get; }
            public string DisplayName { get; }
            public bool IsPhysical { get; }
            public float CycleDuration { get; }
            public float ReviewScale { get; }
            public CompleteLayerDefinition[] Layers { get; }
            public string PrefabName =>
                $"PF_VFX_Showcase_{Slug}_COMPLETE_REVIEW_ONLY_v003";
            public string PrefabPath =>
                CompleteRootPath + "/Prefabs/" + PrefabName + ".prefab";
        }

        private readonly struct VariationDefinition
        {
            public static readonly VariationDefinition Static =
                new VariationDefinition("STATIC", 0f, 1f, 0f, 0f, 0f);

            public VariationDefinition(
                string suffix,
                float rotation,
                float scale,
                float delayBias,
                float layerOffsetX,
                float layerOffsetY)
            {
                Suffix = suffix;
                Rotation = rotation;
                Scale = scale;
                DelayBias = delayBias;
                LayerOffsetX = layerOffsetX;
                LayerOffsetY = layerOffsetY;
            }

            public string Suffix { get; }
            public float Rotation { get; }
            public float Scale { get; }
            public float DelayBias { get; }
            public float LayerOffsetX { get; }
            public float LayerOffsetY { get; }
        }

        private readonly struct CaptureDefinition
        {
            public CaptureDefinition(
                string fileName,
                Vector3 cameraPosition,
                Vector3 cameraTarget,
                float fieldOfView,
                float simulationTime)
            {
                FileName = fileName;
                CameraPosition = cameraPosition;
                CameraTarget = cameraTarget;
                FieldOfView = fieldOfView;
                SimulationTime = simulationTime;
            }

            public string FileName { get; }
            public Vector3 CameraPosition { get; }
            public Vector3 CameraTarget { get; }
            public float FieldOfView { get; }
            public float SimulationTime { get; }
        }
    }
}
