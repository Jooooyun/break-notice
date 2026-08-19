using System;
using System.Linq;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.Contest.Editor
{
    public static class ContestUnarmedCombatAudioAuthoring
    {
        public const string AudioRootName = "SHOWCASE_UNARMED_COMBAT_AUDIO";
        public const int VoiceCount = 8;
        public const string AudioFolder = "Assets/_Project/Audio/Impact/Unarmed";
        public const string P01Path = AudioFolder + "/BN_BODY_IMPACT_06_FULL.wav";
        public const string P03Path = AudioFolder + "/BN_FACE_PUNCH_02_FULL.wav";
        public const string P04Path = AudioFolder + "/BN_FACE_PUNCH_03_FULL.wav";
        public const string P08Path = AudioFolder + "/BN_FACE_PUNCH_01_FULL.wav";
        public const string P09Path = AudioFolder + "/BN_FACE_PUNCH_04_FULL.wav";
        public const string P10Path = AudioFolder + "/BN_BODY_IMPACT_02_FULL.wav";

        private static readonly string[] SelectedAudioPaths =
        {
            P01Path,
            P03Path,
            P04Path,
            P08Path,
            P09Path,
            P10Path
        };

        [MenuItem("BREAK NOTICE/Contest/Apply Unarmed Combat Audio To S7")]
        public static void PrepareS7UnarmedCombatAudio()
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(
                ContestShowcaseSceneAuthoring.ShowcaseS7ScenePath);
            if (sceneAsset == null)
            {
                throw new InvalidOperationException(
                    $"Missing playable S7 Scene: " +
                    ContestShowcaseSceneAuthoring.ShowcaseS7ScenePath);
            }

            ConfigureSelectedAudioImporters();
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
                    "Could not save the S7 unarmed-combat audio wiring.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log(
                "S7 unarmed combat audio prepared: P01/P03/P04/P08/P09/P10 " +
                "full WAVs, accepted-contact impact sync, player and Runner " +
                "melee only.");
        }

        public static void ConfigureSelectedAudioImporters()
        {
            foreach (string path in SelectedAudioPaths)
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null)
                {
                    throw new InvalidOperationException(
                        $"Missing selected unarmed AudioImporter: {path}.");
                }

                importer.forceToMono = false;
                importer.ambisonic = false;
                importer.loadInBackground = false;
                var serializedImporter = new SerializedObject(importer);
                SerializedProperty normalize =
                    serializedImporter.FindProperty("m_Normalize");
                if (normalize != null)
                {
                    normalize.boolValue = false;
                    serializedImporter.ApplyModifiedPropertiesWithoutUndo();
                }
                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.PCM;
                settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
                settings.preloadAudioData = true;
                importer.defaultSampleSettings = settings;
                if (AssetDatabase.WriteImportSettingsIfDirty(path))
                {
                    AssetDatabase.ImportAsset(
                        path,
                        ImportAssetOptions.ForceSynchronousImport);
                }
            }
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
                string.Equals(root.name, AudioRootName, StringComparison.Ordinal));
            if (oldRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(oldRoot);
            }

            ShowcasePlayerCombatController player = scene.GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<ShowcasePlayerCombatController>(true))
                .Single();
            ShowcaseRunnerPressureController pressure = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<
                    ShowcaseRunnerPressureController>(true))
                .Single();
            ShowcaseRunnerAgent[] runners = pressure.Runners
                .Where(runner => runner != null)
                .ToArray();
            if (runners.Length != 2)
            {
                throw new InvalidOperationException(
                    $"S7 unarmed audio requires exactly two authored Runners, got " +
                    $"{runners.Length}.");
            }

            Camera outputCamera = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Camera>(true))
                .Single(camera => string.Equals(
                    camera.gameObject.name,
                    "OutputCamera",
                    StringComparison.Ordinal));
            foreach (AudioListener oldListener in scene.GetRootGameObjects()
                         .SelectMany(root =>
                             root.GetComponentsInChildren<AudioListener>(true)))
            {
                if (oldListener.gameObject != outputCamera.gameObject)
                {
                    UnityEngine.Object.DestroyImmediate(oldListener);
                }
            }
            AudioListener listener = outputCamera.GetComponent<AudioListener>();
            if (listener == null)
            {
                listener = outputCamera.gameObject.AddComponent<AudioListener>();
            }
            listener.enabled = true;
            EditorUtility.SetDirty(listener);

            var root = new GameObject(AudioRootName);
            SceneManager.MoveGameObjectToScene(root, scene);
            ShowcaseUnarmedCombatAudioController controller =
                root.AddComponent<ShowcaseUnarmedCombatAudioController>();
            var voices = new AudioSource[VoiceCount];
            for (int index = 0; index < voices.Length; index++)
            {
                var voiceObject = new GameObject($"UNARMED_AUDIO_VOICE_{index + 1:00}");
                voiceObject.transform.SetParent(root.transform, false);
                AudioSource source = voiceObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.volume = ShowcaseUnarmedCombatAudioController.
                    ConfirmedImpactVolume;
                source.pitch = 1f;
                source.priority = 64;
                source.spatialBlend = 1f;
                source.dopplerLevel = 0f;
                source.spread = 0f;
                source.minDistance = 2f;
                source.maxDistance = 24f;
                source.rolloffMode = AudioRolloffMode.Logarithmic;
                source.bypassEffects = true;
                source.bypassReverbZones = true;
                voices[index] = source;
                EditorUtility.SetDirty(voiceObject);
                EditorUtility.SetDirty(source);
            }

            controller.Configure(
                player,
                pressure,
                runners,
                RequireClip(P01Path),
                RequireClip(P03Path),
                RequireClip(P04Path),
                RequireClip(P08Path),
                RequireClip(P09Path),
                RequireClip(P10Path),
                voices);
            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(outputCamera.gameObject);
        }

        private static AudioClip RequireClip(string path)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null)
            {
                throw new InvalidOperationException(
                    $"Missing selected unarmed full WAV: {path}.");
            }
            return clip;
        }
    }
}
