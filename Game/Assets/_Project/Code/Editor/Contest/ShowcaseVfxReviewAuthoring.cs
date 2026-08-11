using System;
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
    /// Authors an isolated, local-review-only presentation scene for the eight
    /// Showcase VFX source candidates. Nothing created here is connected to a
    /// Gameplay Scene, runtime hook, shared Prefab, or build setting.
    /// </summary>
    public static class ShowcaseVfxReviewAuthoring
    {
        public const string ReviewRootPath =
            "Assets/_Project/Art/VFX/Showcase";

        public const string ReviewScenePath =
            "Assets/_Project/Scenes/90_Review/LOCAL_REVIEW_ONLY_ShowcaseVFX.unity";

        public const string ParticleShaderName =
            "Particles/Standard Unlit";

        private const string BackdropShaderName =
            "Unlit/Color";

        private const string SceneRootName =
            "LOCAL_REVIEW_ONLY_ShowcaseVFX_ROOT";

        private static readonly ReviewAssetDefinition[] Definitions =
        {
            new ReviewAssetDefinition(
                "STATION_01_INTERACTION_TARGET",
                "01  INTERACTION TARGET",
                "Assets/_Project/Art/VFX/Showcase/Textures/T_VFX_Showcase_InteractionSoftRing_v001.png",
                "M_VFX_Showcase_InteractionSoftRing_v001",
                "PF_VFX_Showcase_InteractionSoftRing_LOCAL_REVIEW_ONLY_v001",
                false,
                512,
                1.35f,
                1.75f,
                1.85f,
                0.82f,
                1.12f),
            new ReviewAssetDefinition(
                "STATION_02_PICKUP_CATCH_CONFIRM",
                "02  PICKUP / CATCH VALID",
                "Assets/_Project/Art/VFX/Showcase/Textures/T_VFX_Showcase_PickupCatchConfirm_v001.png",
                "M_VFX_Showcase_PickupCatchConfirm_v001",
                "PF_VFX_Showcase_PickupCatchConfirm_LOCAL_REVIEW_ONLY_v001",
                false,
                512,
                0.72f,
                1.28f,
                1.35f,
                0.72f,
                1.05f),
            new ReviewAssetDefinition(
                "STATION_03_INTERACTION_REJECTED",
                "03  INTERACTION REJECTED",
                "Assets/_Project/Art/VFX/Showcase/Textures/T_VFX_Showcase_InteractionRejected_v001.png",
                "M_VFX_Showcase_InteractionRejected_v001",
                "PF_VFX_Showcase_InteractionRejected_LOCAL_REVIEW_ONLY_v001",
                false,
                512,
                0.78f,
                1.36f,
                1.35f,
                0.84f,
                1.14f),
            new ReviewAssetDefinition(
                "STATION_04_LIGHT_CONTACT_DUST",
                "04  LIGHT CONTACT DUST  /  4x4",
                "Assets/_Project/Art/VFX/Showcase/Flipbooks/T_VFX_Showcase_LightContactDust_4x4_v001.png",
                "M_VFX_Showcase_LightContactDust_4x4_v001",
                "PF_VFX_Showcase_LightContactDust_4x4_LOCAL_REVIEW_ONLY_v001",
                true,
                2048,
                0.72f,
                1.24f,
                1.75f,
                0.92f,
                1.06f),
            new ReviewAssetDefinition(
                "STATION_05_CHAIR_WHEEL_CONTACT",
                "05  CHAIR WHEEL CONTACT  /  4x4",
                "Assets/_Project/Art/VFX/Showcase/Flipbooks/T_VFX_Showcase_ChairWheelContact_4x4_v001.png",
                "M_VFX_Showcase_ChairWheelContact_4x4_v001",
                "PF_VFX_Showcase_ChairWheelContact_4x4_LOCAL_REVIEW_ONLY_v001",
                true,
                2048,
                0.96f,
                1.52f,
                2.20f,
                0.92f,
                1.06f),
            new ReviewAssetDefinition(
                "STATION_06_RESTRAINED_PAPER_BURST",
                "06  RESTRAINED PAPER BURST  /  4x4",
                "Assets/_Project/Art/VFX/Showcase/Flipbooks/T_VFX_Showcase_RestrainedPaperBurst_4x4_v001.png",
                "M_VFX_Showcase_RestrainedPaperBurst_4x4_v001",
                "PF_VFX_Showcase_RestrainedPaperBurst_4x4_LOCAL_REVIEW_ONLY_v001",
                true,
                2048,
                1.12f,
                1.72f,
                2.05f,
                0.94f,
                1.04f),
            new ReviewAssetDefinition(
                "STATION_07_ALLY_READY_SUPPORT",
                "07  ALLY READY SUPPORT",
                "Assets/_Project/Art/VFX/Showcase/Textures/T_VFX_Showcase_AllyReadySupport_v001.png",
                "M_VFX_Showcase_AllyReadySupport_v001",
                "PF_VFX_Showcase_AllyReadySupport_LOCAL_REVIEW_ONLY_v001",
                false,
                512,
                0.96f,
                1.48f,
                1.30f,
                0.80f,
                1.10f),
            new ReviewAssetDefinition(
                "STATION_08_EXIT_ACTIVATION",
                "08  EXIT ACTIVATION",
                "Assets/_Project/Art/VFX/Showcase/Textures/T_VFX_Showcase_ExitActivation_v001.png",
                "M_VFX_Showcase_ExitActivation_v001",
                "PF_VFX_Showcase_ExitActivation_LOCAL_REVIEW_ONLY_v001",
                false,
                512,
                1.36f,
                1.82f,
                1.90f,
                0.84f,
                1.12f)
        };

        [MenuItem("BREAK NOTICE/Contest/Prepare Isolated Showcase VFX Review")]
        public static void PrepareReviewSetup()
        {
            EnsureAssetFolder(ReviewRootPath);
            EnsureAssetFolder($"{ReviewRootPath}/Textures");
            EnsureAssetFolder($"{ReviewRootPath}/Flipbooks");
            EnsureAssetFolder($"{ReviewRootPath}/Materials");
            EnsureAssetFolder($"{ReviewRootPath}/Prefabs");
            EnsureAssetFolder("Assets/_Project/Scenes/90_Review");

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            foreach (ReviewAssetDefinition definition in Definitions)
            {
                ConfigureTextureImporter(definition);
            }

            Shader particleShader = Shader.Find(ParticleShaderName);
            if (particleShader == null)
            {
                throw new InvalidOperationException(
                    $"Required built-in URP shader is unavailable: {ParticleShaderName}");
            }

            Material[] materials = Definitions
                .Select(definition => CreateParticleMaterial(definition, particleShader))
                .ToArray();

            for (int index = 0; index < Definitions.Length; index++)
            {
                CreateReviewPrefab(Definitions[index], materials[index]);
            }

            Material backdropMaterial = CreateBackdropMaterial();
            CreateReviewScene(backdropMaterial);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            Debug.Log(
                $"Isolated Showcase VFX review prepared: imports={Definitions.Length}, " +
                $"stations={Definitions.Length}/{Definitions.Length}, scene={ReviewScenePath}, " +
                "gameplayIntegration=0.");
        }

        public static void CaptureReviewFrameForValidation()
        {
            const string outputArgument = "-showcaseVfxReviewCapturePath";
            string outputPath = ReadCommandLineValue(outputArgument);
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                throw new ArgumentException(
                    $"A PNG output path is required through {outputArgument}.");
            }

            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            Camera camera = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Camera>(true))
                .Single();

            foreach (ParticleSystem particleSystem in scene.GetRootGameObjects()
                         .SelectMany(root => root.GetComponentsInChildren<ParticleSystem>(true)))
            {
                particleSystem.Simulate(0.45f, true, true, false);
            }

            const int width = 1920;
            const int height = 1080;
            RenderTexture renderTexture = new RenderTexture(
                width,
                height,
                24,
                RenderTextureFormat.ARGB32)
            {
                name = "LOCAL_REVIEW_ONLY_ShowcaseVFX_ValidationCapture",
                antiAliasing = 1
            };
            Texture2D capture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            RenderTexture previousActive = RenderTexture.active;

            try
            {
                renderTexture.Create();
                camera.targetTexture = renderTexture;
                camera.Render();

                RenderTexture.active = renderTexture;
                capture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, false);
                capture.Apply(false, false);

                string fullPath = Path.GetFullPath(outputPath);
                string directory = Path.GetDirectoryName(fullPath);
                if (string.IsNullOrEmpty(directory))
                {
                    throw new InvalidOperationException(
                        $"Capture output directory is invalid: {fullPath}");
                }

                Directory.CreateDirectory(directory);
                File.WriteAllBytes(fullPath, capture.EncodeToPNG());
                Debug.Log($"Showcase VFX review validation capture written: {fullPath}");
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previousActive;
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(capture);
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        private static void ConfigureTextureImporter(ReviewAssetDefinition definition)
        {
            TextureImporter importer =
                AssetImporter.GetAtPath(definition.TexturePath) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    $"Required review texture is missing or not importable: {definition.TexturePath}");
            }

            importer.textureType = TextureImporterType.Default;
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.isReadable = false;
            importer.mipmapEnabled = definition.IsFlipbook;
            importer.borderMipmap = definition.IsFlipbook;
            importer.streamingMipmaps = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.anisoLevel = 1;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.crunchedCompression = false;
            importer.maxTextureSize = definition.MaxTextureSize;
            importer.SaveAndReimport();
        }

        private static Material CreateParticleMaterial(
            ReviewAssetDefinition definition,
            Shader shader)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(definition.TexturePath);
            if (texture == null)
            {
                throw new InvalidOperationException(
                    $"Imported review texture is unavailable: {definition.TexturePath}");
            }

            string materialPath = definition.MaterialPath;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(shader)
                {
                    name = definition.MaterialName
                };
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
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
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

        private static void CreateReviewPrefab(
            ReviewAssetDefinition definition,
            Material material)
        {
            GameObject temporaryRoot = new GameObject(definition.PrefabName);
            try
            {
                ParticleSystem particleSystem = temporaryRoot.AddComponent<ParticleSystem>();
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                ParticleSystem.MainModule main = particleSystem.main;
                main.duration = definition.CycleDuration;
                main.loop = true;
                main.prewarm = false;
                main.playOnAwake = true;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                main.simulationSpeed = 1f;
                main.startLifetime = definition.Lifetime;
                main.startSpeed = 0f;
                main.startSize3D = false;
                main.startSize = definition.StartSize;
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
                emission.SetBursts(new[] { new ParticleSystem.Burst(0.05f, 1) });

                ParticleSystem.ShapeModule shape = particleSystem.shape;
                shape.enabled = false;

                ConfigureLifetimeCurves(particleSystem, definition);
                ConfigureTextureSheetAnimation(particleSystem, definition.IsFlipbook);

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

                ParticleSystemRenderer renderer =
                    temporaryRoot.GetComponent<ParticleSystemRenderer>();
                renderer.sharedMaterial = material;
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                renderer.alignment = ParticleSystemRenderSpace.View;
                renderer.sortMode = ParticleSystemSortMode.Distance;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                renderer.minParticleSize = 0f;
                renderer.maxParticleSize = 1f;
                renderer.enableGPUInstancing = false;

                if (PrefabUtility.SaveAsPrefabAsset(temporaryRoot, definition.PrefabPath) == null)
                {
                    throw new InvalidOperationException(
                        $"Could not save isolated review Prefab: {definition.PrefabPath}");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(temporaryRoot);
            }
        }

        private static void ConfigureLifetimeCurves(
            ParticleSystem particleSystem,
            ReviewAssetDefinition definition)
        {
            Gradient colorGradient = new Gradient();
            colorGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                definition.IsFlipbook
                    ? new[]
                    {
                        new GradientAlphaKey(1f, 0f),
                        new GradientAlphaKey(1f, 0.78f),
                        new GradientAlphaKey(0f, 1f)
                    }
                    : new[]
                    {
                        new GradientAlphaKey(0f, 0f),
                        new GradientAlphaKey(1f, 0.12f),
                        new GradientAlphaKey(0.86f, 0.72f),
                        new GradientAlphaKey(0f, 1f)
                    });

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime =
                particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(colorGradient);

            AnimationCurve sizeCurve = new AnimationCurve(
                new Keyframe(0f, definition.StartScaleMultiplier),
                new Keyframe(0.72f, 1f),
                new Keyframe(1f, definition.EndScaleMultiplier));
            sizeCurve.preWrapMode = WrapMode.ClampForever;
            sizeCurve.postWrapMode = WrapMode.ClampForever;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime =
                particleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.separateAxes = false;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        }

        private static void ConfigureTextureSheetAnimation(
            ParticleSystem particleSystem,
            bool isFlipbook)
        {
            ParticleSystem.TextureSheetAnimationModule sheet =
                particleSystem.textureSheetAnimation;
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
                AnimationCurve.Linear(0f, 0f, 1f, 1f));
            sheet.startFrame = new ParticleSystem.MinMaxCurve(0f);
            sheet.cycleCount = 1;
        }

        private static Material CreateBackdropMaterial()
        {
            const string path =
                "Assets/_Project/Art/VFX/Showcase/Materials/M_LOCAL_REVIEW_ONLY_ShowcaseVFX_Backdrop.mat";
            Shader shader = Shader.Find(BackdropShaderName);
            if (shader == null)
            {
                throw new InvalidOperationException(
                    $"Required built-in URP shader is unavailable: {BackdropShaderName}");
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader)
                {
                    name = "M_LOCAL_REVIEW_ONLY_ShowcaseVFX_Backdrop"
                };
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            Color backdrop = new Color(0.075f, 0.09f, 0.105f, 1f);
            SetColorIfPresent(material, "_Color", backdrop);
            SetFloatIfPresent(material, "_ZWrite", 1f);
            material.SetOverrideTag("RenderType", "Opaque");
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Geometry;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateReviewScene(Material backdropMaterial)
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            GameObject root = new GameObject(SceneRootName);
            SceneManager.MoveGameObjectToScene(root, scene);

            CreateReviewCamera(root.transform);
            CreateLabel(
                root.transform,
                "REVIEW_HEADER",
                "LOCAL REVIEW ONLY  /  SHOWCASE VFX  /  8 STATIONS",
                new Vector3(0f, 4.25f, -0.38f),
                0.038f,
                new Color(0.78f, 0.83f, 0.86f, 1f));

            float[] columns = { -5.25f, -1.75f, 1.75f, 5.25f };
            float[] rows = { 2.25f, -1.05f };

            for (int index = 0; index < Definitions.Length; index++)
            {
                ReviewAssetDefinition definition = Definitions[index];
                GameObject station = new GameObject(definition.StationName);
                station.transform.SetParent(root.transform, false);
                station.transform.localPosition = new Vector3(
                    columns[index % columns.Length],
                    rows[index / columns.Length],
                    0f);

                CreateStationBackdrop(station.transform, backdropMaterial);
                CreateLabel(
                    station.transform,
                    "LABEL",
                    definition.DisplayName,
                    new Vector3(0f, -0.93f, -0.36f),
                    0.026f,
                    new Color(0.70f, 0.76f, 0.79f, 1f));

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    definition.PrefabPath);
                if (prefab == null)
                {
                    throw new InvalidOperationException(
                        $"Required isolated review Prefab is unavailable: {definition.PrefabPath}");
                }

                GameObject instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
                if (instance == null)
                {
                    throw new InvalidOperationException(
                        $"Could not instantiate isolated review Prefab: {definition.PrefabPath}");
                }

                instance.name = "VFX_REVIEW_INSTANCE";
                instance.transform.SetParent(station.transform, false);
                instance.transform.localPosition = new Vector3(0f, 0.22f, -0.30f);
            }

            RenderSettings.fog = false;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.20f, 0.22f, 0.24f, 1f);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ReviewScenePath))
            {
                throw new InvalidOperationException(
                    $"Could not save isolated review Scene: {ReviewScenePath}");
            }
        }

        private static void CreateReviewCamera(Transform parent)
        {
            GameObject cameraObject = new GameObject("LOCAL_REVIEW_ONLY_CAMERA");
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.position = new Vector3(0f, 0.60f, -14.5f);
            cameraObject.transform.rotation = Quaternion.LookRotation(
                new Vector3(0f, 0.60f, 0f) - cameraObject.transform.position,
                Vector3.up);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.018f, 0.022f, 0.027f, 1f);
            camera.fieldOfView = 42f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.allowHDR = true;
            camera.allowMSAA = true;
            camera.depth = 0f;
            cameraObject.AddComponent<AudioListener>();
        }

        private static void CreateStationBackdrop(
            Transform parent,
            Material backdropMaterial)
        {
            GameObject backdrop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backdrop.name = "REVIEW_BACKDROP";
            backdrop.transform.SetParent(parent, false);
            backdrop.transform.localPosition = new Vector3(0f, 0f, 0.30f);
            backdrop.transform.localScale = new Vector3(3.25f, 2.55f, 0.08f);

            Collider collider = backdrop.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            MeshRenderer renderer = backdrop.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = backdropMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
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

        private static void SetTextureIfPresent(
            Material material,
            string propertyName,
            Texture texture)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetTexture(propertyName, texture);
            }
        }

        private static void SetColorIfPresent(
            Material material,
            string propertyName,
            Color color)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, color);
            }
        }

        private static void SetFloatIfPresent(
            Material material,
            string propertyName,
            float value)
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
                if (string.Equals(
                        arguments[index],
                        argumentName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return arguments[index + 1];
                }
            }

            return null;
        }

        private readonly struct ReviewAssetDefinition
        {
            public ReviewAssetDefinition(
                string stationName,
                string displayName,
                string texturePath,
                string materialName,
                string prefabName,
                bool isFlipbook,
                int maxTextureSize,
                float lifetime,
                float cycleDuration,
                float startSize,
                float startScaleMultiplier,
                float endScaleMultiplier)
            {
                StationName = stationName;
                DisplayName = displayName;
                TexturePath = texturePath;
                MaterialName = materialName;
                PrefabName = prefabName;
                IsFlipbook = isFlipbook;
                MaxTextureSize = maxTextureSize;
                Lifetime = lifetime;
                CycleDuration = cycleDuration;
                StartSize = startSize;
                StartScaleMultiplier = startScaleMultiplier;
                EndScaleMultiplier = endScaleMultiplier;
            }

            public string StationName { get; }
            public string DisplayName { get; }
            public string TexturePath { get; }
            public string MaterialName { get; }
            public string PrefabName { get; }
            public bool IsFlipbook { get; }
            public int MaxTextureSize { get; }
            public float Lifetime { get; }
            public float CycleDuration { get; }
            public float StartSize { get; }
            public float StartScaleMultiplier { get; }
            public float EndScaleMultiplier { get; }

            public string MaterialPath =>
                $"{ReviewRootPath}/Materials/{MaterialName}.mat";

            public string PrefabPath =>
                $"{ReviewRootPath}/Prefabs/{PrefabName}.prefab";
        }
    }
}
