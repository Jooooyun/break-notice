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
    public static class ShowcaseAudioImpactReviewAuthoring
    {
        public const string SceneFolder = "Assets/_Project/Scenes/90_Review";
        public const string ScenePath =
            SceneFolder + "/LOCAL_REVIEW_ONLY_ShowcaseAudioImpact_v003.unity";
        public const string AudioFolder =
            "Assets/_Project/Audio/Review/Showcase/Rebuild_v003";
        public const string MotionClipPath =
            AudioFolder + "/LOCAL_REVIEW_ONLY_NorthStarMotion_v003.anim";
        public const string RootName = "LOCAL_REVIEW_ONLY_SHOWCASE_AUDIO_IMPACT_V003";
        public const string StageName = "NorthStarStage";
        public const string MotionClipName = "LOCAL_REVIEW_ONLY_NorthStarMotion_v003";
        public const float ContactSeconds = 0.2f;
        public const float ReviewDurationSeconds = 1.35f;

        private const string ArmedBlindKey = "BreakNotice.AudioImpactV003.ArmedBlind";
        private const string ArmedVariationKey = "BreakNotice.AudioImpactV003.ArmedVariation";

        private static readonly string[] BlindLabels = { "X", "Y", "Z" };
        private static readonly string[] StemNames =
        {
            "pre",
            "signature",
            "chair",
            "runner",
            "floor_tail"
        };

        [MenuItem("BREAK NOTICE/Review/Audio Impact v003/Build LOCAL REVIEW Scene")]
        public static void BuildReviewSetup()
        {
            if (!Application.isBatchMode &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            RequireAudioFiles();
            EnsureFolder(SceneFolder);
            EnsureFolder(AudioFolder);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ConfigureAudioImporters();

            Material floorMaterial = CreateMaterial(
                AudioFolder + "/ReviewFloor.mat",
                new Color(0.075f, 0.090f, 0.115f, 1f),
                0.05f,
                0.20f);
            Material wallMaterial = CreateMaterial(
                AudioFolder + "/ReviewWall.mat",
                new Color(0.12f, 0.15f, 0.20f, 1f),
                0.05f,
                0.18f);
            Material chairMaterial = CreateMaterial(
                AudioFolder + "/ReviewChair.mat",
                new Color(0.82f, 0.57f, 0.10f, 1f),
                0.65f,
                0.38f);
            Material chairDarkMaterial = CreateMaterial(
                AudioFolder + "/ReviewChairDark.mat",
                new Color(0.055f, 0.065f, 0.075f, 1f),
                0.35f,
                0.28f);
            Material runnerMaterial = CreateMaterial(
                AudioFolder + "/ReviewRunner.mat",
                new Color(0.20f, 0.48f, 0.72f, 1f),
                0.0f,
                0.30f);
            Material accentMaterial = CreateMaterial(
                AudioFolder + "/ReviewAccent.mat",
                new Color(1.0f, 0.20f, 0.16f, 1f),
                0.0f,
                0.22f);
            AnimationClip motionClip = CreateMotionClip();

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            GameObject rootObject = new GameObject(RootName);
            SceneManager.MoveGameObjectToScene(rootObject, scene);
            Transform root = rootObject.transform;

            BuildEnvironment(root, floorMaterial, wallMaterial, accentMaterial);
            Camera reviewCamera = BuildCamera(root);
            BuildLighting(root);
            Transform stage = BuildStage(
                root,
                chairMaterial,
                chairDarkMaterial,
                runnerMaterial,
                motionClip);
            BuildPlaybackGroups(root);
            BuildLabels(root, accentMaterial);

            reviewCamera.transform.LookAt(new Vector3(0f, 1.15f, 0f));
            motionClip.SampleAnimation(stage.gameObject, 0f);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"Could not save {ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log(
                "Built LOCAL_REVIEW_ONLY Showcase Audio Impact v003: " +
                "one primitive Chair->Runner stage, X/Y/Z x two variations, five aligned stems, " +
                "no gameplay scene or Runtime audio hook.");
        }

        [MenuItem("BREAK NOTICE/Review/Audio Impact v003/Open Blind Playback Window")]
        public static void OpenPlaybackWindow()
        {
            ShowcaseAudioImpactReviewWindow.Open();
        }

        [MenuItem("BREAK NOTICE/Review/Audio Impact v003/Capture Common Video Frames")]
        public static void CaptureCommonVideoFrames()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                BuildReviewSetup();
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject root = scene.GetRootGameObjects()
                .Single(item => item.name == RootName);
            Camera camera = root.GetComponentsInChildren<Camera>(true).Single();
            Transform stage = root.transform.Find(StageName);
            Animation animation = stage.GetComponent<Animation>();
            AnimationClip clip = animation.GetClip(MotionClipName);
            if (clip == null)
            {
                throw new InvalidOperationException("Review motion clip is missing.");
            }

            string captureFolder = Path.Combine(
                Path.GetTempPath(),
                "break-notice-audio-impact-v003-frames-" +
                DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(captureFolder);

            const int width = 960;
            const int height = 540;
            const int framesPerSecond = 30;
            const float gapSeconds = 0.8f;
            float auditionSeconds = ReviewDurationSeconds * 2f + gapSeconds;
            int frameCount = Mathf.CeilToInt(auditionSeconds * framesPerSecond);

            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = camera.targetTexture;
            try
            {
                camera.targetTexture = renderTexture;
                for (int frame = 0; frame < frameCount; frame++)
                {
                    float globalTime = frame / (float)framesPerSecond;
                    float localTime;
                    if (globalTime < ReviewDurationSeconds)
                    {
                        localTime = globalTime;
                    }
                    else if (globalTime < ReviewDurationSeconds + gapSeconds)
                    {
                        localTime = 0f;
                    }
                    else
                    {
                        localTime = globalTime - ReviewDurationSeconds - gapSeconds;
                    }

                    clip.SampleAnimation(stage.gameObject, Mathf.Clamp(localTime, 0f, ReviewDurationSeconds));
                    camera.Render();
                    RenderTexture.active = renderTexture;
                    texture.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
                    texture.Apply(false, false);
                    File.WriteAllBytes(
                        Path.Combine(captureFolder, $"frame_{frame:D4}.png"),
                        texture.EncodeToPNG());
                }
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(renderTexture);
                clip.SampleAnimation(stage.gameObject, 0f);
            }

            Debug.Log(
                $"FRAME_CAPTURE_DIR={captureFolder}\n" +
                $"FRAME_CAPTURE_COUNT={frameCount}\n" +
                $"FRAME_CAPTURE_FPS={framesPerSecond}");
        }

        public static void ArmAndEnterPlayMode(string blindLabel, int variation)
        {
            if (!BlindLabels.Contains(blindLabel))
            {
                throw new ArgumentOutOfRangeException(nameof(blindLabel));
            }

            if (variation < 1 || variation > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(variation));
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                BuildReviewSetup();
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SessionState.SetString(ArmedBlindKey, blindLabel);
            SessionState.SetInt(ArmedVariationKey, variation);
            EditorApplication.isPlaying = true;
        }

        [InitializeOnLoadMethod]
        private static void RegisterPlayModeHandler()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode)
            {
                return;
            }

            string blindLabel = SessionState.GetString(ArmedBlindKey, string.Empty);
            int variation = SessionState.GetInt(ArmedVariationKey, 0);
            if (string.IsNullOrEmpty(blindLabel) || variation < 1 || variation > 2)
            {
                return;
            }

            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogError("Armed audio review scene is not loaded.");
                return;
            }

            GameObject root = scene.GetRootGameObjects()
                .Single(item => item.name == RootName);
            Transform playbackRoot = root.transform.Find("PlaybackGroups");
            foreach (Transform child in playbackRoot)
            {
                child.gameObject.SetActive(false);
            }

            Transform selected = playbackRoot.Find($"BLIND_{blindLabel}_v{variation:00}");
            if (selected == null)
            {
                Debug.LogError($"Missing armed group {blindLabel} v{variation:00}.");
                return;
            }

            selected.gameObject.SetActive(true);
            Animation animation = root.transform.Find(StageName).GetComponent<Animation>();
            animation.Rewind(MotionClipName);
            animation.Play(MotionClipName);
            foreach (AudioSource audioSource in selected.GetComponentsInChildren<AudioSource>(true))
            {
                audioSource.Play();
            }

            Debug.Log($"PLAYING_BLIND_REVIEW={blindLabel}_v{variation:00}");
        }

        private static void RequireAudioFiles()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            foreach (string blindLabel in BlindLabels)
            {
                for (int variation = 1; variation <= 2; variation++)
                {
                    foreach (string stemName in StemNames)
                    {
                        string assetPath = StemAssetPath(blindLabel, variation, stemName);
                        string fullPath = Path.Combine(
                            projectRoot,
                            assetPath.Replace('/', Path.DirectorySeparatorChar));
                        if (!File.Exists(fullPath))
                        {
                            throw new FileNotFoundException(
                                "Missing generated REVIEW stem.",
                                fullPath);
                        }
                    }
                }
            }
        }

        private static void ConfigureAudioImporters()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { AudioFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null)
                {
                    continue;
                }

                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.PCM;
                settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
                settings.preloadAudioData = true;
                importer.defaultSampleSettings = settings;
                importer.forceToMono = false;
                importer.loadInBackground = false;
                importer.ambisonic = false;
                importer.SaveAndReimport();
            }
        }

        private static Transform BuildEnvironment(
            Transform root,
            Material floorMaterial,
            Material wallMaterial,
            Material accentMaterial)
        {
            Transform environment = new GameObject("Environment").transform;
            environment.SetParent(root, false);
            CreatePrimitive(
                "Floor",
                PrimitiveType.Cube,
                environment,
                new Vector3(0f, -0.12f, 0f),
                new Vector3(12f, 0.20f, 7f),
                floorMaterial);
            CreatePrimitive(
                "BackWall",
                PrimitiveType.Cube,
                environment,
                new Vector3(0f, 2.8f, 3.0f),
                new Vector3(12f, 5.8f, 0.20f),
                wallMaterial);
            CreatePrimitive(
                "ContactDatum",
                PrimitiveType.Cube,
                environment,
                new Vector3(-0.05f, 0.012f, 0f),
                new Vector3(0.035f, 0.025f, 3.8f),
                accentMaterial);
            return environment;
        }

        private static Camera BuildCamera(Transform root)
        {
            GameObject cameraObject = new GameObject("ReviewCamera");
            cameraObject.transform.SetParent(root, false);
            cameraObject.transform.position = new Vector3(0f, 3.65f, -8.4f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 42f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 50f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.035f, 0.055f, 1f);
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        private static void BuildLighting(Transform root)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.32f, 0.37f, 0.46f, 1f);
            GameObject keyObject = new GameObject("KeyLight");
            keyObject.transform.SetParent(root, false);
            keyObject.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
            Light key = keyObject.AddComponent<Light>();
            key.type = LightType.Directional;
            key.intensity = 1.55f;
            key.color = new Color(1.0f, 0.90f, 0.76f, 1f);

            GameObject fillObject = new GameObject("FillLight");
            fillObject.transform.SetParent(root, false);
            fillObject.transform.position = new Vector3(2.6f, 2.8f, -2.0f);
            Light fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Point;
            fill.range = 8f;
            fill.intensity = 2.2f;
            fill.color = new Color(0.35f, 0.57f, 1.0f, 1f);
        }

        private static Transform BuildStage(
            Transform root,
            Material chairMaterial,
            Material chairDarkMaterial,
            Material runnerMaterial,
            AnimationClip motionClip)
        {
            Transform stage = new GameObject(StageName).transform;
            stage.SetParent(root, false);

            Transform chair = new GameObject("Chair").transform;
            chair.SetParent(stage, false);
            chair.localPosition = new Vector3(-3.2f, 0f, 0f);
            CreatePrimitive(
                "Seat",
                PrimitiveType.Cube,
                chair,
                new Vector3(0f, 0.82f, 0f),
                new Vector3(1.10f, 0.18f, 0.90f),
                chairMaterial);
            CreatePrimitive(
                "Back",
                PrimitiveType.Cube,
                chair,
                new Vector3(0f, 1.38f, 0.38f),
                new Vector3(1.10f, 0.92f, 0.13f),
                chairMaterial);
            CreatePrimitive(
                "Stem",
                PrimitiveType.Cylinder,
                chair,
                new Vector3(0f, 0.42f, 0f),
                new Vector3(0.12f, 0.32f, 0.12f),
                chairDarkMaterial);
            for (int index = 0; index < 5; index++)
            {
                float angle = index * Mathf.PI * 2f / 5f;
                Vector3 wheelPosition = new Vector3(
                    Mathf.Cos(angle) * 0.48f,
                    0.12f,
                    Mathf.Sin(angle) * 0.35f);
                Transform wheel = CreatePrimitive(
                    $"Caster_{index + 1:00}",
                    PrimitiveType.Cylinder,
                    chair,
                    wheelPosition,
                    new Vector3(0.12f, 0.08f, 0.12f),
                    chairDarkMaterial);
                wheel.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }

            Transform runner = new GameObject("Runner").transform;
            runner.SetParent(stage, false);
            runner.localPosition = new Vector3(0.40f, 0f, 0f);
            CreatePrimitive(
                "Body",
                PrimitiveType.Capsule,
                runner,
                new Vector3(0f, 1.10f, 0f),
                new Vector3(0.78f, 1.06f, 0.78f),
                runnerMaterial);
            CreatePrimitive(
                "Head",
                PrimitiveType.Sphere,
                runner,
                new Vector3(0f, 2.28f, 0f),
                Vector3.one * 0.56f,
                runnerMaterial);

            Animation animation = stage.gameObject.AddComponent<Animation>();
            animation.playAutomatically = false;
            animation.AddClip(motionClip, MotionClipName);
            animation.clip = motionClip;
            return stage;
        }

        private static void BuildPlaybackGroups(Transform root)
        {
            Transform playbackRoot = new GameObject("PlaybackGroups").transform;
            playbackRoot.SetParent(root, false);
            var positions = new Dictionary<string, Vector3>
            {
                { "pre", new Vector3(-0.65f, 1.0f, -0.10f) },
                { "signature", new Vector3(0f, 1.1f, -0.4f) },
                { "chair", new Vector3(-0.35f, 0.95f, 0f) },
                { "runner", new Vector3(0.35f, 1.25f, 0f) },
                { "floor_tail", new Vector3(0.15f, 0.12f, 0.15f) }
            };
            var spatialBlend = new Dictionary<string, float>
            {
                { "pre", 0.15f },
                { "signature", 0f },
                { "chair", 0.85f },
                { "runner", 0.85f },
                { "floor_tail", 0.80f }
            };

            foreach (string blindLabel in BlindLabels)
            {
                for (int variation = 1; variation <= 2; variation++)
                {
                    Transform group = new GameObject($"BLIND_{blindLabel}_v{variation:00}").transform;
                    group.SetParent(playbackRoot, false);
                    foreach (string stemName in StemNames)
                    {
                        GameObject sourceObject = new GameObject(stemName);
                        sourceObject.transform.SetParent(group, false);
                        sourceObject.transform.localPosition = positions[stemName];
                        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(
                            StemAssetPath(blindLabel, variation, stemName));
                        if (clip == null)
                        {
                            throw new InvalidOperationException(
                                $"Could not import {blindLabel} v{variation:00} {stemName}.");
                        }

                        AudioSource audioSource = sourceObject.AddComponent<AudioSource>();
                        audioSource.clip = clip;
                        audioSource.playOnAwake = false;
                        audioSource.loop = false;
                        audioSource.volume = 1f;
                        audioSource.pitch = 1f;
                        audioSource.spatialBlend = spatialBlend[stemName];
                        audioSource.dopplerLevel = 0f;
                        audioSource.rolloffMode = AudioRolloffMode.Linear;
                        audioSource.minDistance = 1f;
                        audioSource.maxDistance = 18f;
                    }

                    group.gameObject.SetActive(false);
                }
            }
        }

        private static void BuildLabels(Transform root, Material accentMaterial)
        {
            CreateText(
                "Title",
                root,
                "LOCAL REVIEW ONLY — CHAIR → RUNNER T3",
                new Vector3(-4.8f, 3.75f, 2.82f),
                0.30f,
                Color.white,
                TextAnchor.UpperLeft);
            CreateText(
                "Timing",
                root,
                "SAME VISUAL / CAMERA / CONTACT FOR X · Y · Z\nCONTACT = FILE TIME 0.200 s",
                new Vector3(-4.8f, 3.25f, 2.82f),
                0.18f,
                new Color(0.72f, 0.80f, 0.92f, 1f),
                TextAnchor.UpperLeft);
            CreateText(
                "Authority",
                root,
                "REVIEW — NOT FINAL — NO RUNTIME INTEGRATION",
                new Vector3(-4.8f, 0.60f, 2.82f),
                0.20f,
                new Color(1f, 0.38f, 0.30f, 1f),
                TextAnchor.LowerLeft);
        }

        private static AnimationClip CreateMotionClip()
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(MotionClipPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                clip.name = MotionClipName;
                clip.legacy = true;
                AssetDatabase.CreateAsset(clip, MotionClipPath);
            }

            foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
            {
                AnimationUtility.SetEditorCurve(clip, binding, null);
            }

            SetCurve(
                clip,
                "Chair",
                "m_LocalPosition.x",
                Key(0f, -3.20f),
                Key(0.10f, -2.30f),
                Key(ContactSeconds, -0.40f),
                Key(0.28f, -0.72f),
                Key(0.48f, -0.62f),
                Key(ReviewDurationSeconds, -0.62f));
            SetCurve(
                clip,
                "Chair",
                "localEulerAnglesRaw.z",
                Key(0f, 0f),
                Key(ContactSeconds, -4f),
                Key(0.29f, 7f),
                Key(0.55f, 2f),
                Key(ReviewDurationSeconds, 2f));
            SetCurve(
                clip,
                "Runner",
                "m_LocalPosition.x",
                Key(0f, 0.40f),
                Key(ContactSeconds, 0.40f),
                Key(0.30f, 0.78f),
                Key(0.58f, 1.48f),
                Key(0.92f, 1.58f),
                Key(ReviewDurationSeconds, 1.58f));
            SetCurve(
                clip,
                "Runner",
                "m_LocalPosition.y",
                Key(0f, 0f),
                Key(ContactSeconds, 0f),
                Key(0.34f, 0.16f),
                Key(0.68f, 0.04f),
                Key(ReviewDurationSeconds, 0.04f));
            SetCurve(
                clip,
                "Runner",
                "localEulerAnglesRaw.z",
                Key(0f, 0f),
                Key(ContactSeconds, 0f),
                Key(0.33f, -18f),
                Key(0.62f, -31f),
                Key(0.95f, -24f),
                Key(ReviewDurationSeconds, -24f));
            clip.wrapMode = WrapMode.ClampForever;
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static void SetCurve(
            AnimationClip clip,
            string relativePath,
            string propertyName,
            params Keyframe[] keys)
        {
            var curve = new AnimationCurve(keys);
            for (int index = 0; index < curve.length; index++)
            {
                AnimationUtility.SetKeyLeftTangentMode(
                    curve,
                    index,
                    AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(
                    curve,
                    index,
                    AnimationUtility.TangentMode.ClampedAuto);
            }

            AnimationUtility.SetEditorCurve(
                clip,
                EditorCurveBinding.FloatCurve(relativePath, typeof(Transform), propertyName),
                curve);
        }

        private static Keyframe Key(float time, float value)
        {
            return new Keyframe(time, value);
        }

        private static Material CreateMaterial(
            string path,
            Color color,
            float metallic,
            float smoothness)
        {
            Shader shader = GraphicsSettings.currentRenderPipeline == null
                ? Shader.Find("Standard")
                : Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                throw new InvalidOperationException("No review material shader is available.");
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else
            {
                material.color = color;
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Transform CreatePrimitive(
            string name,
            PrimitiveType primitiveType,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            GameObject instance = GameObject.CreatePrimitive(primitiveType);
            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localPosition;
            instance.transform.localScale = localScale;
            Renderer renderer = instance.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            foreach (Collider collider in instance.GetComponents<Collider>())
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            return instance.transform;
        }

        private static TextMesh CreateText(
            string name,
            Transform parent,
            string text,
            Vector3 localPosition,
            float characterSize,
            Color color,
            TextAnchor anchor)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            textObject.transform.localPosition = localPosition;
            TextMesh textMesh = textObject.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.fontSize = 64;
            textMesh.characterSize = characterSize / textMesh.fontSize;
            textMesh.color = color;
            textMesh.anchor = anchor;
            textMesh.alignment = TextAlignment.Left;
            return textMesh;
        }

        private static string StemAssetPath(string blindLabel, int variation, string stemName)
        {
            return AudioFolder +
                $"/Stems/{blindLabel}/BLIND_{blindLabel}_v{variation:00}_{stemName}_REVIEW.wav";
        }

        private static void EnsureFolder(string assetFolder)
        {
            string normalized = assetFolder.Replace('\\', '/').TrimEnd('/');
            string[] parts = normalized.Split('/');
            if (parts.Length == 0 || parts[0] != "Assets")
            {
                throw new ArgumentException($"Not an Assets path: {assetFolder}");
            }

            string current = "Assets";
            for (int index = 1; index < parts.Length; index++)
            {
                string next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }

                current = next;
            }
        }
    }

    public sealed class ShowcaseAudioImpactReviewWindow : EditorWindow
    {
        public static void Open()
        {
            var window = GetWindow<ShowcaseAudioImpactReviewWindow>();
            window.titleContent = new GUIContent("Audio Impact v003");
            window.minSize = new Vector2(430f, 270f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField(
                "Chair → Runner T3 — BLIND REVIEW",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Use laptop speakers first. X/Y/Z share the same Scene, camera, motion, " +
                "0.200 s contact, and loudness target. This window does not reveal A/B/C.",
                MessageType.Info);
            EditorGUILayout.Space(6f);
            foreach (string blindLabel in new[] { "X", "Y", "Z" })
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"BLIND {blindLabel}", GUILayout.Width(95f));
                if (GUILayout.Button("Play variation 1", GUILayout.Height(34f)))
                {
                    ShowcaseAudioImpactReviewAuthoring.ArmAndEnterPlayMode(blindLabel, 1);
                }

                if (GUILayout.Button("Play variation 2", GUILayout.Height(34f)))
                {
                    ShowcaseAudioImpactReviewAuthoring.ArmAndEnterPlayMode(blindLabel, 2);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(
                "Stop Play Mode before selecting another candidate. Do not open the private mapping " +
                "until you record PASS X / PASS Y / PASS Z / REJECT ALL.",
                MessageType.Warning);
        }
    }
}
