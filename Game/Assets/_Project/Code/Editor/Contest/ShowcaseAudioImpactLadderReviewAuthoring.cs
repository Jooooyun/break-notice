using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace BreakNotice.Contest.Editor
{
    public static class ShowcaseAudioImpactLadderReviewAuthoring
    {
        public const string SceneFolder = "Assets/_Project/Scenes/90_Review";
        public const string ScenePath =
            SceneFolder + "/LOCAL_REVIEW_ONLY_ShowcaseAudioImpactLadder_v004.unity";
        public const string AudioFolder =
            "Assets/_Project/Audio/Review/Showcase/Rebuild_v004";
        public const string RootName = "LOCAL_REVIEW_ONLY_SHOWCASE_AUDIO_IMPACT_LADDER_V004";
        public const string StageName = "ImpactStage";
        public const float ContactSeconds = 0.2f;
        public const float ReviewDurationSeconds = 1.35f;

        public static readonly string[] CueIds =
        {
            "T1_HAND_RUNNER_v01",
            "T1_HAND_RUNNER_v02",
            "T1_HAND_RUNNER_v03",
            "T2_HOOK_RUNNER_v01",
            "T2_HOOK_RUNNER_v02",
            "T2_HOOK_RUNNER_v03",
            "T2_BINDER_RUNNER_v01",
            "T2_BINDER_RUNNER_v02",
            "T2_BINDER_RUNNER_v03",
            "T3_CHAIR_RUNNER_RC_v01",
            "T3_CHAIR_RUNNER_RC_v02"
        };

        public static readonly string[] StemNames =
        {
            "pre",
            "signature",
            "emitter",
            "runner",
            "floor_tail"
        };

        [MenuItem("BREAK NOTICE/Review/Audio Impact Ladder v004/Build LOCAL REVIEW Scene")]
        public static void BuildReviewSetup()
        {
            if (!Application.isBatchMode &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            RequireReviewFiles();
            EnsureFolder(SceneFolder);
            EnsureFolder(AudioFolder);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ConfigureAudioImporters();

            Material floorMaterial = CreateMaterial(
                AudioFolder + "/SceneAssets/ReviewFloor.mat",
                new Color(0.055f, 0.070f, 0.095f, 1f),
                0.05f,
                0.20f);
            Material wallMaterial = CreateMaterial(
                AudioFolder + "/SceneAssets/ReviewWall.mat",
                new Color(0.11f, 0.14f, 0.19f, 1f),
                0.04f,
                0.18f);
            Material chairMaterial = CreateMaterial(
                AudioFolder + "/SceneAssets/ReviewChair.mat",
                new Color(0.90f, 0.60f, 0.08f, 1f),
                0.58f,
                0.34f);
            Material chairDarkMaterial = CreateMaterial(
                AudioFolder + "/SceneAssets/ReviewChairDark.mat",
                new Color(0.045f, 0.055f, 0.070f, 1f),
                0.38f,
                0.26f);
            Material runnerMaterial = CreateMaterial(
                AudioFolder + "/SceneAssets/ReviewRunner.mat",
                new Color(0.18f, 0.48f, 0.74f, 1f),
                0.0f,
                0.28f);
            Material accentMaterial = CreateMaterial(
                AudioFolder + "/SceneAssets/ReviewAccent.mat",
                new Color(1.0f, 0.19f, 0.13f, 1f),
                0.0f,
                0.22f);

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            GameObject rootObject = new GameObject(RootName);
            SceneManager.MoveGameObjectToScene(rootObject, scene);
            Transform root = rootObject.transform;

            BuildEnvironment(root, floorMaterial, wallMaterial, accentMaterial);
            Camera reviewCamera = BuildCamera(root);
            BuildLighting(root);
            BuildStage(root, chairMaterial, chairDarkMaterial, runnerMaterial);
            BuildPlaybackGroups(root);
            BuildLabels(root);
            reviewCamera.transform.LookAt(new Vector3(-0.55f, 1.15f, 0f));

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"Could not save {ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log(
                "Built LOCAL_REVIEW_ONLY Audio Impact Ladder v004: " +
                "11 cue groups, 55 aligned stems, one primitive review stage, " +
                "no animation asset, physics, gameplay scene, mixer, or Runtime hook.");
        }

        [MenuItem("BREAK NOTICE/Review/Audio Impact Ladder v004/Open Playback Window")]
        public static void OpenPlaybackWindow()
        {
            ShowcaseAudioImpactLadderReviewWindow.Open();
        }

        [MenuItem("BREAK NOTICE/Review/Audio Impact Ladder v004/Capture T3 Common Video Frames")]
        public static void CaptureCommonVideoFrames()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                BuildReviewSetup();
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject root = scene.GetRootGameObjects().Single(item => item.name == RootName);
            Camera camera = root.GetComponentsInChildren<Camera>(true).Single();
            Transform stage = root.transform.Find(StageName);
            Transform chair = stage.Find("Chair");
            Transform runner = stage.Find("Runner");

            string captureFolder = Path.Combine(
                Path.GetTempPath(),
                "break-notice-audio-impact-ladder-v004-frames-" +
                DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(captureFolder);

            const int width = 960;
            const int height = 540;
            const int framesPerSecond = 30;
            const float gapSeconds = 0.65f;
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
                    int variation;
                    float localTime;
                    if (globalTime < ReviewDurationSeconds)
                    {
                        variation = 1;
                        localTime = globalTime;
                    }
                    else if (globalTime < ReviewDurationSeconds + gapSeconds)
                    {
                        variation = 0;
                        localTime = 0f;
                    }
                    else
                    {
                        variation = 2;
                        localTime = globalTime - ReviewDurationSeconds - gapSeconds;
                    }

                    SampleStage(chair, runner, localTime, variation);
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
                SampleStage(chair, runner, 0f, 1);
            }

            Debug.Log(
                $"FRAME_CAPTURE_DIR={captureFolder}\n" +
                $"FRAME_CAPTURE_COUNT={frameCount}\n" +
                $"FRAME_CAPTURE_FPS={framesPerSecond}");
        }

        private static void RequireReviewFiles()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            foreach (string cueId in CueIds)
            {
                RequireAssetFile(projectRoot, CompositeAssetPath(cueId));
                foreach (string stemName in StemNames)
                {
                    RequireAssetFile(projectRoot, StemAssetPath(cueId, stemName));
                }
            }

            foreach (string name in new[]
            {
                "IMPACT_LADDER_ACTUAL_DYNAMIC_REVIEW.wav",
                "T1_HAND_12_REPEAT_STRESS_REVIEW.wav",
                "TIER_10_TRIAL_BLIND_REVIEW.wav",
                "MATERIAL_10_TRIAL_BLIND_REVIEW.wav"
            })
            {
                RequireAssetFile(projectRoot, AudioFolder + "/Derived/" + name);
            }

            RequireAssetFile(projectRoot, AudioFolder + "/ReferenceOnly/T3_X_PAIR_BLIND_REVIEW.wav");
            RequireAssetFile(projectRoot, AudioFolder + "/ReferenceOnly/T3_Y_PAIR_BLIND_REVIEW.wav");
            RequireAssetFile(projectRoot, AudioFolder + "/REVIEW_MANIFEST.json");
        }

        private static void RequireAssetFile(string projectRoot, string assetPath)
        {
            string fullPath = Path.Combine(
                projectRoot,
                assetPath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("Missing generated v004 review asset.", fullPath);
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

        private static void BuildEnvironment(
            Transform root,
            Material floorMaterial,
            Material wallMaterial,
            Material accentMaterial)
        {
            Transform environment = new GameObject("Environment").transform;
            environment.SetParent(root, false);
            CreatePrimitive("Floor", PrimitiveType.Cube, environment,
                new Vector3(0f, -0.12f, 0f), new Vector3(12f, 0.20f, 7f), floorMaterial);
            CreatePrimitive("BackWall", PrimitiveType.Cube, environment,
                new Vector3(0f, 2.8f, 3.0f), new Vector3(12f, 5.8f, 0.20f), wallMaterial);
            CreatePrimitive("ContactDatum", PrimitiveType.Cube, environment,
                new Vector3(-0.18f, 0.012f, 0f), new Vector3(0.035f, 0.025f, 3.8f), accentMaterial);
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
            camera.backgroundColor = new Color(0.022f, 0.030f, 0.048f, 1f);
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

        private static void BuildStage(
            Transform root,
            Material chairMaterial,
            Material chairDarkMaterial,
            Material runnerMaterial)
        {
            Transform stage = new GameObject(StageName).transform;
            stage.SetParent(root, false);
            Transform chair = new GameObject("Chair").transform;
            chair.SetParent(stage, false);
            CreatePrimitive("Seat", PrimitiveType.Cube, chair,
                new Vector3(0f, 0.82f, 0f), new Vector3(1.10f, 0.18f, 0.90f), chairMaterial);
            CreatePrimitive("Back", PrimitiveType.Cube, chair,
                new Vector3(0f, 1.38f, 0.38f), new Vector3(1.10f, 0.92f, 0.13f), chairMaterial);
            CreatePrimitive("Stem", PrimitiveType.Cylinder, chair,
                new Vector3(0f, 0.42f, 0f), new Vector3(0.12f, 0.32f, 0.12f), chairDarkMaterial);
            for (int index = 0; index < 5; index++)
            {
                float angle = index * Mathf.PI * 2f / 5f;
                Transform caster = CreatePrimitive(
                    $"Caster_{index + 1:00}", PrimitiveType.Cylinder, chair,
                    new Vector3(Mathf.Cos(angle) * 0.48f, 0.12f, Mathf.Sin(angle) * 0.35f),
                    new Vector3(0.12f, 0.08f, 0.12f), chairDarkMaterial);
                caster.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }

            Transform runner = new GameObject("Runner").transform;
            runner.SetParent(stage, false);
            CreatePrimitive("Body", PrimitiveType.Capsule, runner,
                new Vector3(0f, 1.10f, 0f), new Vector3(0.78f, 1.06f, 0.78f), runnerMaterial);
            CreatePrimitive("Head", PrimitiveType.Sphere, runner,
                new Vector3(0f, 2.28f, 0f), Vector3.one * 0.56f, runnerMaterial);
            SampleStage(chair, runner, 0f, 1);
        }

        private static void BuildPlaybackGroups(Transform root)
        {
            Transform playbackRoot = new GameObject("PlaybackGroups").transform;
            playbackRoot.SetParent(root, false);
            var positions = new Dictionary<string, Vector3>
            {
                { "pre", new Vector3(-0.65f, 1.0f, -0.10f) },
                { "signature", new Vector3(0f, 1.1f, -0.4f) },
                { "emitter", new Vector3(-0.35f, 0.95f, 0f) },
                { "runner", new Vector3(0.35f, 1.25f, 0f) },
                { "floor_tail", new Vector3(0.15f, 0.12f, 0.15f) }
            };
            var spatialBlend = new Dictionary<string, float>
            {
                { "pre", 0.15f },
                { "signature", 0f },
                { "emitter", 0.85f },
                { "runner", 0.85f },
                { "floor_tail", 0.80f }
            };

            foreach (string cueId in CueIds)
            {
                Transform group = new GameObject(cueId).transform;
                group.SetParent(playbackRoot, false);
                foreach (string stemName in StemNames)
                {
                    GameObject sourceObject = new GameObject(stemName);
                    sourceObject.transform.SetParent(group, false);
                    sourceObject.transform.localPosition = positions[stemName];
                    AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(
                        StemAssetPath(cueId, stemName));
                    if (clip == null)
                    {
                        throw new InvalidOperationException(
                            $"Could not import {cueId} / {stemName}.");
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

        private static void BuildLabels(Transform root)
        {
            CreateText("Title", root,
                "LOCAL REVIEW ONLY — ARCADE EDGE IMPACT LADDER v004",
                new Vector3(-5.2f, 3.75f, 2.82f), 0.29f, Color.white, TextAnchor.UpperLeft);
            CreateText("Timing", root,
                "CONTACT = FILE TIME 0.200 s   ·   T1 < T2 < T3 ACTUAL DYNAMICS",
                new Vector3(-5.2f, 3.33f, 2.82f), 0.20f,
                new Color(1.0f, 0.77f, 0.25f, 1f), TextAnchor.UpperLeft);
            CreateText("Boundary", root,
                "EDITOR REVIEW ONLY · NO RUNTIME / MIXER / GAMEPLAY HOOK",
                new Vector3(-5.2f, 3.02f, 2.82f), 0.17f,
                new Color(0.64f, 0.75f, 0.90f, 1f), TextAnchor.UpperLeft);
        }

        private static void SampleStage(
            Transform chair,
            Transform runner,
            float time,
            int variation)
        {
            float variationScale = variation == 2 ? 1.06f : 1f;
            if (variation == 0)
            {
                chair.localPosition = new Vector3(-3.2f, 0f, 0f);
                chair.localRotation = Quaternion.identity;
                runner.localPosition = new Vector3(0.40f, 0f, 0f);
                runner.localRotation = Quaternion.identity;
                return;
            }

            if (time <= ContactSeconds)
            {
                float normalized = Mathf.Clamp01(time / ContactSeconds);
                float eased = 1f - Mathf.Pow(1f - normalized, 2.2f);
                chair.localPosition = new Vector3(Mathf.Lerp(-3.2f, -0.18f, eased), 0f, 0f);
                chair.localRotation = Quaternion.Euler(0f, normalized * (variation == 2 ? -7f : 5f), 0f);
                runner.localPosition = new Vector3(0.40f, 0f, 0f);
                runner.localRotation = Quaternion.identity;
                return;
            }

            float post = time - ContactSeconds;
            chair.localPosition = new Vector3(-0.18f + Mathf.Min(post / 0.24f, 1f) * 0.42f, 0f, 0f);
            chair.localRotation = Quaternion.Euler(0f, (variation == 2 ? -7f : 5f) + post * 18f, 0f);
            float reaction = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(post / 0.32f));
            runner.localPosition = new Vector3(0.40f + reaction * 0.48f * variationScale, 0f, 0f);
            runner.localRotation = Quaternion.Euler(0f, 0f, -reaction * 10f * variationScale);
        }

        private static Material CreateMaterial(
            string path,
            Color color,
            float metallic,
            float smoothness)
        {
            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
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
            instance.GetComponent<Renderer>().sharedMaterial = material;
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

        private static string CompositeAssetPath(string cueId)
        {
            return AudioFolder + $"/Composites/{cueId}_REVIEW.wav";
        }

        private static string StemAssetPath(string cueId, string stemName)
        {
            return AudioFolder + $"/Stems/{cueId}/{cueId}_{stemName}_REVIEW.wav";
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

    public sealed class ShowcaseAudioImpactLadderReviewWindow : EditorWindow
    {
        private Vector2 scroll;

        public static void Open()
        {
            var window = GetWindow<ShowcaseAudioImpactLadderReviewWindow>();
            window.titleContent = new GUIContent("Impact Ladder v004");
            window.minSize = new Vector2(520f, 540f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Arcade Edge Impact Ladder v004", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Laptop speakers first. Fixed OS volume. This is an Editor-only review surface; " +
                "it does not attach audio to gameplay.",
                MessageType.Info);
            if (GUILayout.Button("STOP PREVIEW", GUILayout.Height(28f)))
            {
                StopPreview();
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawSection("Required review reels", new Dictionary<string, string>
            {
                { "Actual-dynamic ladder", "Derived/IMPACT_LADDER_ACTUAL_DYNAMIC_REVIEW.wav" },
                { "Tier 10-trial blind", "Derived/TIER_10_TRIAL_BLIND_REVIEW.wav" },
                { "Material 10-trial blind", "Derived/MATERIAL_10_TRIAL_BLIND_REVIEW.wav" },
                { "T1 12-repeat stress", "Derived/T1_HAND_12_REPEAT_STRESS_REVIEW.wav" }
            });
            DrawSection("T3 blind reference comparison", new Dictionary<string, string>
            {
                { "BLIND X — two variations", "ReferenceOnly/T3_X_PAIR_BLIND_REVIEW.wav" },
                { "BLIND Y — two variations", "ReferenceOnly/T3_Y_PAIR_BLIND_REVIEW.wav" }
            });

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Individual composites", EditorStyles.boldLabel);
            foreach (string cueId in ShowcaseAudioImpactLadderReviewAuthoring.CueIds)
            {
                if (GUILayout.Button(cueId, GUILayout.Height(24f)))
                {
                    Play(ShowcaseAudioImpactLadderReviewAuthoring.AudioFolder +
                        $"/Composites/{cueId}_REVIEW.wav");
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.HelpBox(
                "Do not open Private answer keys until answers are written. Human PASS LADDER " +
                "is still required; technical PASS is not final approval.",
                MessageType.Warning);
        }

        private static void DrawSection(string title, Dictionary<string, string> entries)
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            foreach (KeyValuePair<string, string> entry in entries)
            {
                if (GUILayout.Button(entry.Key, GUILayout.Height(30f)))
                {
                    Play(ShowcaseAudioImpactLadderReviewAuthoring.AudioFolder + "/" + entry.Value);
                }
            }
        }

        private static void Play(string assetPath)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (clip == null)
            {
                throw new InvalidOperationException($"Could not load review clip: {assetPath}");
            }

            StopPreview();
            Type audioUtil = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtil.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .First(item => item.Name == "PlayPreviewClip" &&
                    item.GetParameters().Length > 0 &&
                    item.GetParameters()[0].ParameterType == typeof(AudioClip));
            ParameterInfo[] parameters = method.GetParameters();
            object[] arguments = parameters.Length switch
            {
                1 => new object[] { clip },
                2 => new object[] { clip, 0 },
                _ => new object[] { clip, 0, false }
            };
            method.Invoke(null, arguments);
        }

        private static void StopPreview()
        {
            Type audioUtil = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtil.GetMethod(
                "StopAllPreviewClips",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            method?.Invoke(null, null);
        }
    }
}

