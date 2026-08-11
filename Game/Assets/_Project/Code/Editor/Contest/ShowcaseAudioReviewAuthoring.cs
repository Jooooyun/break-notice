using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.Contest.Editor
{
    public static class ShowcaseAudioReviewAuthoring
    {
        public const string ReviewAudioRoot =
            "Assets/_Project/Audio/Review/Showcase";

        public const string ReviewScenePath =
            "Assets/_Project/Scenes/90_Review/LOCAL_REVIEW_ONLY_ShowcaseAudio.unity";

        public const string ReviewRootName =
            "LOCAL_REVIEW_ONLY_ShowcaseAudio";

        private const string ReviewSceneFolder =
            "Assets/_Project/Scenes/90_Review";

        private static readonly ClipDefinition[] ClipDefinitions =
        {
            new ClipDefinition(1, "UI", "AUD_UI_Confirm_01_v001.wav", 1, false, false),
            new ClipDefinition(2, "UI", "AUD_UI_Reject_01_v001.wav", 1, false, false),
            new ClipDefinition(3, "Player", "AUD_PLY_Binder_Acquire_01_v001.wav", 1, true, false),
            new ClipDefinition(4, "Player", "AUD_PLY_Chair_Acquire_01_v001.wav", 1, true, false),
            new ClipDefinition(5, "Player", "AUD_PLY_Binder_ThrowRelease_01_v001.wav", 1, true, false),
            new ClipDefinition(6, "Binder", "AUD_BND_Impact_OfficeSurface_01_v001.wav", 1, true, false),
            new ClipDefinition(7, "Binder", "AUD_BND_Impact_OfficeSurface_02_v001.wav", 1, true, false),
            new ClipDefinition(8, "Binder", "AUD_BND_Impact_OfficeSurface_03_v001.wav", 1, true, false),
            new ClipDefinition(9, "Chair", "AUD_CHR_Impact_StaticOffice_01_v001.wav", 1, true, false),
            new ClipDefinition(10, "Chair", "AUD_CHR_Impact_StaticOffice_02_v001.wav", 1, true, false),
            new ClipDefinition(11, "Chair", "AUD_CHR_Impact_StaticOffice_03_v001.wav", 1, true, false),
            new ClipDefinition(12, "Chair", "AUD_CHR_CasterRoll_Loop_v001.wav", 1, true, true),
            new ClipDefinition(13, "Ally", "AUD_ALY_Ready_01_v001.wav", 1, true, false),
            new ClipDefinition(14, "Ally", "AUD_ALY_BinderSupply_Release_01_v001.wav", 1, true, false),
            new ClipDefinition(15, "Binder", "AUD_BND_Catch_Secure_01_v001.wav", 1, true, false),
            new ClipDefinition(16, "Player", "AUD_PLY_ThrowCharge_Release_v001.wav", 1, true, false),
            new ClipDefinition(17, "Reset", "AUD_RST_Reset_Accept_v001.wav", 1, false, false),
            new ClipDefinition(18, "Player", "AUD_PLY_Footstep_OfficeFloor_01_v001.wav", 1, true, false),
            new ClipDefinition(19, "Player", "AUD_PLY_Footstep_OfficeFloor_02_v001.wav", 1, true, false),
            new ClipDefinition(20, "Player", "AUD_PLY_Footstep_OfficeFloor_03_v001.wav", 1, true, false),
            new ClipDefinition(21, "Player", "AUD_PLY_Footstep_OfficeFloor_04_v001.wav", 1, true, false),
            new ClipDefinition(22, "Ambience", "AUD_AMB_OperationsFloor_RoomTone_Loop_v001.wav", 2, false, true),
            new ClipDefinition(23, "Exit", "AUD_EXT_Exit_Activated_v001.wav", 2, false, false)
        };

        public static IReadOnlyList<ClipDefinition> Clips => ClipDefinitions;

        [MenuItem("BREAK NOTICE/Audio Review/Build Isolated Showcase Review Setup")]
        public static void BuildReviewSetup()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            RequireExactAudioAssets();
            ConfigureImportSettings();
            ShowcaseAudioAuditionWindow.ValidatePreviewApi();
            CreateReviewScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log(
                $"Showcase audio review setup prepared: {ClipDefinitions.Length} clips, " +
                $"scene={ReviewScenePath}. LOCAL REVIEW ONLY; gameplay integration remains zero.");
        }

        [MenuItem("BREAK NOTICE/Audio Review/Open Isolated Showcase Review Scene")]
        public static void OpenReviewScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ReviewScenePath) == null)
            {
                BuildReviewSetup();
                return;
            }

            EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
        }

        public static AudioClip LoadClip(ClipDefinition definition)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(definition.AssetPath);
        }

        private static void RequireExactAudioAssets()
        {
            string[] expectedPaths = ClipDefinitions
                .Select(definition => definition.AssetPath)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            string[] actualPaths = AssetDatabase
                .FindAssets("t:AudioClip", new[] { ReviewAudioRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();

            if (!expectedPaths.SequenceEqual(actualPaths, StringComparer.Ordinal))
            {
                throw new InvalidOperationException(
                    "The isolated Showcase review folder must contain exactly the 23 authorized WAV files. " +
                    $"Expected={expectedPaths.Length}, actual={actualPaths.Length}.");
            }
        }

        private static void ConfigureImportSettings()
        {
            foreach (ClipDefinition definition in ClipDefinitions)
            {
                AudioImporter importer = AssetImporter.GetAtPath(definition.AssetPath) as AudioImporter;
                if (importer == null)
                {
                    throw new InvalidOperationException(
                        $"AudioImporter was not available for {definition.AssetPath}.");
                }

                importer.forceToMono = false;
                importer.loadInBackground = false;
                importer.ambisonic = false;

                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.PCM;
                settings.quality = 1f;
                settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
                settings.preloadAudioData = true;
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
            }
        }

        private static void CreateReviewScene()
        {
            if (!AssetDatabase.IsValidFolder(ReviewSceneFolder))
            {
                string guid = AssetDatabase.CreateFolder(
                    "Assets/_Project/Scenes",
                    "90_Review");
                if (string.IsNullOrEmpty(guid))
                {
                    throw new InvalidOperationException(
                        $"Could not create the review scene folder: {ReviewSceneFolder}.");
                }
            }

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            GameObject root = new GameObject(ReviewRootName);

            GameObject listenerObject = new GameObject("ReviewListener");
            listenerObject.transform.SetParent(root.transform, false);
            listenerObject.AddComponent<AudioListener>();

            string[] categories = ClipDefinitions
                .Select(definition => definition.Category)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            for (int categoryIndex = 0; categoryIndex < categories.Length; categoryIndex++)
            {
                string category = categories[categoryIndex];
                GameObject categoryObject = new GameObject($"Category_{category}");
                categoryObject.transform.SetParent(root.transform, false);
                categoryObject.transform.localPosition = new Vector3(categoryIndex * 4f, 0f, 0f);

                ClipDefinition[] categoryClips = ClipDefinitions
                    .Where(definition => string.Equals(
                        definition.Category,
                        category,
                        StringComparison.Ordinal))
                    .ToArray();

                for (int clipIndex = 0; clipIndex < categoryClips.Length; clipIndex++)
                {
                    ClipDefinition definition = categoryClips[clipIndex];
                    AudioClip clip = LoadClip(definition);
                    if (clip == null)
                    {
                        throw new InvalidOperationException(
                            $"Review clip could not be loaded: {definition.AssetPath}.");
                    }

                    string stationName =
                        $"{definition.Order:D2}_{System.IO.Path.GetFileNameWithoutExtension(definition.FileName)}";
                    GameObject station = new GameObject(stationName);
                    station.transform.SetParent(categoryObject.transform, false);
                    station.transform.localPosition = new Vector3(0f, 0f, clipIndex * 2.5f);

                    AudioSource source = station.AddComponent<AudioSource>();
                    source.clip = clip;
                    source.playOnAwake = false;
                    source.loop = definition.LoopIntent;
                    source.volume = 1f;
                    source.pitch = 1f;
                    source.dopplerLevel = 0f;
                    source.spatialBlend = definition.ReviewSpatialized ? 1f : 0f;
                    source.rolloffMode = AudioRolloffMode.Logarithmic;
                    source.minDistance = 1f;
                    source.maxDistance = 25f;
                    source.outputAudioMixerGroup = null;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ReviewScenePath))
            {
                throw new InvalidOperationException(
                    $"Could not save isolated audio review scene: {ReviewScenePath}.");
            }
        }

        public readonly struct ClipDefinition
        {
            public ClipDefinition(
                int order,
                string category,
                string fileName,
                int expectedChannels,
                bool reviewSpatialized,
                bool loopIntent)
            {
                Order = order;
                Category = category;
                FileName = fileName;
                ExpectedChannels = expectedChannels;
                ReviewSpatialized = reviewSpatialized;
                LoopIntent = loopIntent;
            }

            public int Order { get; }
            public string Category { get; }
            public string FileName { get; }
            public int ExpectedChannels { get; }
            public bool ReviewSpatialized { get; }
            public bool LoopIntent { get; }
            public string AssetPath => $"{ReviewAudioRoot}/{FileName}";
        }
    }
}
