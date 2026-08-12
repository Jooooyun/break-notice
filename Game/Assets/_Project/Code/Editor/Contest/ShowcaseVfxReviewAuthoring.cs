using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public static class ShowcaseVfxReviewAuthoring
    {
        public const string ReviewRootPath =
            "Assets/_Project/Art/VFX/Showcase";

        public const string PremiumRootPath =
            ReviewRootPath + "/Premium_v002";

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

        [MenuItem(ReviewMenuPath)]
        public static void PrepareReviewSetup()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            EnsureAssetFolder(PremiumRootPath);
            EnsureAssetFolder(PremiumRootPath + "/Materials");
            EnsureAssetFolder(PremiumRootPath + "/Prefabs");
            EnsureAssetFolder("Assets/_Project/Scenes/90_Review");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            foreach (LayerDefinition layer in Effects.SelectMany(effect => effect.Layers))
            {
                ConfigureTextureImporter(layer);
            }

            Shader particleShader = Shader.Find(ParticleShaderName);
            if (particleShader == null)
            {
                throw new InvalidOperationException(
                    $"Required built-in particle shader is unavailable: {ParticleShaderName}");
            }

            var materials = new Dictionary<string, Material>(StringComparer.Ordinal);
            foreach (LayerDefinition layer in Effects.SelectMany(effect => effect.Layers))
            {
                materials.Add(layer.Name, CreateParticleMaterial(layer, particleShader));
            }

            foreach (EffectDefinition effect in Effects)
            {
                if (effect.IsPhysical)
                {
                    foreach (VariationDefinition variation in PhysicalVariations)
                    {
                        CreateCompositePrefab(effect, variation, materials);
                    }
                }
                else
                {
                    CreateCompositePrefab(effect, VariationDefinition.Static, materials);
                }
            }

            Material backdrop = CreateSolidMaterial(
                "M_LOCAL_REVIEW_ONLY_ShowcaseVFX_PremiumBackdrop_v002",
                new Color(0.055f, 0.064f, 0.068f, 1f));
            Material panel = CreateSolidMaterial(
                "M_LOCAL_REVIEW_ONLY_ShowcaseVFX_PremiumPanel_v002",
                new Color(0.082f, 0.094f, 0.097f, 1f));
            Material floor = CreateSolidMaterial(
                "M_LOCAL_REVIEW_ONLY_ShowcaseVFX_PremiumFloor_v002",
                new Color(0.16f, 0.17f, 0.17f, 1f));
            Material prop = CreateSolidMaterial(
                "M_LOCAL_REVIEW_ONLY_ShowcaseVFX_PremiumProp_v002",
                new Color(0.32f, 0.34f, 0.34f, 1f));
            Material binder = CreateSolidMaterial(
                "M_LOCAL_REVIEW_ONLY_ShowcaseVFX_PremiumBinder_v002",
                new Color(0.31f, 0.37f, 0.43f, 1f));
            Material ally = CreateSolidMaterial(
                "M_LOCAL_REVIEW_ONLY_ShowcaseVFX_PremiumAlly_v002",
                new Color(0.31f, 0.40f, 0.45f, 1f));
            Material exit = CreateSolidMaterial(
                "M_LOCAL_REVIEW_ONLY_ShowcaseVFX_PremiumExit_v002",
                new Color(0.28f, 0.38f, 0.32f, 1f));

            CreateReviewScene(backdrop, panel, floor, prop, binder, ally, exit);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log(
                "Premium Showcase VFX review prepared: replacementEffects=8/8, " +
                "layerImports=24, compositeStations=8/8, physicalVariations=12, " +
                "gameplayIntegration=0, status=REVIEW / OWNER APPROVAL PENDING.");
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
            main.cullingMode = ParticleSystemCullingMode.Automatic;

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

        private static void CreateReviewScene(
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

            RenderSettings.fog = false;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.22f, 0.23f, 0.23f, 1f);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ReviewScenePath))
            {
                throw new InvalidOperationException(
                    $"Could not save isolated premium review scene: {ReviewScenePath}");
            }
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
            cameraObject.transform.position = new Vector3(0f, 0.65f, -19.5f);
            cameraObject.transform.rotation = Quaternion.LookRotation(
                new Vector3(0f, 0.55f, 0f) - cameraObject.transform.position,
                Vector3.up);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.016f, 0.019f, 0.020f, 1f);
            camera.fieldOfView = 40f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 70f;
            camera.allowHDR = true;
            camera.allowMSAA = true;
            camera.depth = 0f;
            cameraObject.AddComponent<AudioListener>();
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
            try
            {
                renderTexture.Create();
                camera.targetTexture = renderTexture;
                camera.Render();
                RenderTexture.active = renderTexture;
                capture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, false);
                capture.Apply(false, false);
                return capture;
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
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
