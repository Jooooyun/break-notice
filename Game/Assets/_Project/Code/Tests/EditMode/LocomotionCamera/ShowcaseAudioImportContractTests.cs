using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ShowcaseAudioImportContractTests
    {
        private const string ReviewAudioRoot =
            "Assets/_Project/Audio/Review/Showcase";

        private const string ReviewScenePath =
            "Assets/_Project/Scenes/90_Review/LOCAL_REVIEW_ONLY_ShowcaseAudio.unity";

        private const string ReviewRootName =
            "LOCAL_REVIEW_ONLY_ShowcaseAudio";

        private static readonly ClipExpectation[] Expected =
        {
            new ClipExpectation("UI", "AUD_UI_Confirm_01_v001.wav", 1, false, false),
            new ClipExpectation("UI", "AUD_UI_Reject_01_v001.wav", 1, false, false),
            new ClipExpectation("Player", "AUD_PLY_Binder_Acquire_01_v001.wav", 1, true, false),
            new ClipExpectation("Player", "AUD_PLY_Chair_Acquire_01_v001.wav", 1, true, false),
            new ClipExpectation("Player", "AUD_PLY_Binder_ThrowRelease_01_v001.wav", 1, true, false),
            new ClipExpectation("Binder", "AUD_BND_Impact_OfficeSurface_01_v001.wav", 1, true, false),
            new ClipExpectation("Binder", "AUD_BND_Impact_OfficeSurface_02_v001.wav", 1, true, false),
            new ClipExpectation("Binder", "AUD_BND_Impact_OfficeSurface_03_v001.wav", 1, true, false),
            new ClipExpectation("Chair", "AUD_CHR_Impact_StaticOffice_01_v001.wav", 1, true, false),
            new ClipExpectation("Chair", "AUD_CHR_Impact_StaticOffice_02_v001.wav", 1, true, false),
            new ClipExpectation("Chair", "AUD_CHR_Impact_StaticOffice_03_v001.wav", 1, true, false),
            new ClipExpectation("Chair", "AUD_CHR_CasterRoll_Loop_v001.wav", 1, true, true),
            new ClipExpectation("Ally", "AUD_ALY_Ready_01_v001.wav", 1, true, false),
            new ClipExpectation("Ally", "AUD_ALY_BinderSupply_Release_01_v001.wav", 1, true, false),
            new ClipExpectation("Binder", "AUD_BND_Catch_Secure_01_v001.wav", 1, true, false),
            new ClipExpectation("Player", "AUD_PLY_ThrowCharge_Release_v001.wav", 1, true, false),
            new ClipExpectation("Reset", "AUD_RST_Reset_Accept_v001.wav", 1, false, false),
            new ClipExpectation("Player", "AUD_PLY_Footstep_OfficeFloor_01_v001.wav", 1, true, false),
            new ClipExpectation("Player", "AUD_PLY_Footstep_OfficeFloor_02_v001.wav", 1, true, false),
            new ClipExpectation("Player", "AUD_PLY_Footstep_OfficeFloor_03_v001.wav", 1, true, false),
            new ClipExpectation("Player", "AUD_PLY_Footstep_OfficeFloor_04_v001.wav", 1, true, false),
            new ClipExpectation("Ambience", "AUD_AMB_OperationsFloor_RoomTone_Loop_v001.wav", 2, false, true),
            new ClipExpectation("Exit", "AUD_EXT_Exit_Activated_v001.wav", 2, false, false)
        };

        [Test]
        public void ReviewFolder_ContainsExactly23UniqueAudioClipPathsAndGuids()
        {
            string[] guids = AssetDatabase.FindAssets(
                "t:AudioClip",
                new[] { ReviewAudioRoot });
            string[] paths = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();
            string[] expectedPaths = Expected
                .Select(expectation => expectation.AssetPath)
                .ToArray();

            Assert.That(guids.Length, Is.EqualTo(23));
            Assert.That(guids.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(23));
            Assert.That(paths.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(23));
            Assert.That(paths, Is.EquivalentTo(expectedPaths));
            Assert.That(paths.All(path => !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path))), Is.True);
        }

        [Test]
        public void ReviewWavFiles_AreByteIdenticalToArtSourceExports()
        {
            string gameRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string repositoryRoot = Path.GetFullPath(Path.Combine(gameRoot, ".."));

            foreach (ClipExpectation expectation in Expected)
            {
                string sourcePath = Path.Combine(
                    repositoryRoot,
                    "ArtSource",
                    "Audio",
                    "Showcase",
                    "Export",
                    expectation.FileName);
                string reviewPath = Path.Combine(
                    gameRoot,
                    expectation.AssetPath.Replace('/', Path.DirectorySeparatorChar));

                Assert.That(File.Exists(sourcePath), Is.True, $"Missing source: {sourcePath}");
                Assert.That(File.Exists(reviewPath), Is.True, $"Missing review copy: {reviewPath}");
                CollectionAssert.AreEqual(
                    File.ReadAllBytes(sourcePath),
                    File.ReadAllBytes(reviewPath),
                    $"Review WAV changed bytes: {expectation.FileName}");
            }
        }

        [Test]
        public void ImportSettings_AreReviewOnlyPcmPreloadedAndPreserveChannels()
        {
            foreach (ClipExpectation expectation in Expected)
            {
                AudioImporter importer =
                    AssetImporter.GetAtPath(expectation.AssetPath) as AudioImporter;
                Assert.That(importer, Is.Not.Null, expectation.AssetPath);
                Assert.That(importer.forceToMono, Is.False, expectation.AssetPath);
                Assert.That(importer.loadInBackground, Is.False, expectation.AssetPath);
                Assert.That(importer.ambisonic, Is.False, expectation.AssetPath);

                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                Assert.That(settings.preloadAudioData, Is.True, expectation.AssetPath);
                Assert.That(
                    settings.loadType,
                    Is.EqualTo(AudioClipLoadType.DecompressOnLoad),
                    expectation.AssetPath);
                Assert.That(
                    settings.compressionFormat,
                    Is.EqualTo(AudioCompressionFormat.PCM),
                    expectation.AssetPath);
                Assert.That(
                    settings.sampleRateSetting,
                    Is.EqualTo(AudioSampleRateSetting.PreserveSampleRate),
                    expectation.AssetPath);
            }
        }

        [Test]
        public void EveryClip_DecodesAt48KhzWithExpectedChannelsAndNonzeroSamples()
        {
            foreach (ClipExpectation expectation in Expected)
            {
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(expectation.AssetPath);
                Assert.That(clip, Is.Not.Null, expectation.AssetPath);
                Assert.That(clip.frequency, Is.EqualTo(48000), expectation.AssetPath);
                Assert.That(clip.channels, Is.EqualTo(expectation.Channels), expectation.AssetPath);
                Assert.That(clip.samples, Is.GreaterThan(0), expectation.AssetPath);
                Assert.That(clip.length, Is.GreaterThan(0f), expectation.AssetPath);
                Assert.That(clip.preloadAudioData, Is.True, expectation.AssetPath);
                Assert.That(clip.LoadAudioData(), Is.True, expectation.AssetPath);

                var samples = new float[clip.samples * clip.channels];
                Assert.That(clip.GetData(samples, 0), Is.True, expectation.AssetPath);
                Assert.That(
                    samples.Any(sample => Mathf.Abs(sample) > 0.000001f),
                    Is.True,
                    $"Imported clip is silent: {expectation.AssetPath}");
            }
        }

        [Test]
        public void ReviewScene_HasCategoryStationsTwoLoopCandidatesAndNoGameplayConnection()
        {
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Single);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);

            GameObject root = scene.GetRootGameObjects()
                .Single(item => string.Equals(item.name, ReviewRootName, StringComparison.Ordinal));
            AudioSource[] sources = root.GetComponentsInChildren<AudioSource>(true);
            AudioListener[] listeners = root.GetComponentsInChildren<AudioListener>(true);
            string[] expectedCategories = Expected
                .Select(expectation => expectation.Category)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            Assert.That(sources.Length, Is.EqualTo(23));
            Assert.That(listeners.Length, Is.EqualTo(1));
            Assert.That(
                root.transform.Cast<Transform>().Count(child => child.name.StartsWith("Category_", StringComparison.Ordinal)),
                Is.EqualTo(expectedCategories.Length));
            Assert.That(
                sources.Select(source => AssetDatabase.GetAssetPath(source.clip)),
                Is.EquivalentTo(Expected.Select(expectation => expectation.AssetPath)));
            Assert.That(sources.Select(source => source.clip).Distinct().Count(), Is.EqualTo(23));
            Assert.That(sources.Count(source => source.loop), Is.EqualTo(2));

            foreach (AudioSource source in sources)
            {
                string path = AssetDatabase.GetAssetPath(source.clip);
                ClipExpectation expectation = Expected.Single(item => item.AssetPath == path);
                Assert.That(source.playOnAwake, Is.False, path);
                Assert.That(source.loop, Is.EqualTo(expectation.LoopIntent), path);
                Assert.That(source.outputAudioMixerGroup, Is.Null, path);
                Assert.That(
                    source.spatialBlend,
                    Is.EqualTo(expectation.ReviewSpatialized ? 1f : 0f).Within(0.0001f),
                    path);
            }

            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                Assert.That(
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject),
                    Is.Zero,
                    transform.name);
            }

            Assert.That(
                EditorBuildSettings.scenes.Any(setting =>
                    string.Equals(setting.path, ReviewScenePath, StringComparison.OrdinalIgnoreCase)),
                Is.False,
                "LOCAL_REVIEW_ONLY scene must not be added to Gameplay Build Settings.");
        }

        private readonly struct ClipExpectation
        {
            public ClipExpectation(
                string category,
                string fileName,
                int channels,
                bool reviewSpatialized,
                bool loopIntent)
            {
                Category = category;
                FileName = fileName;
                Channels = channels;
                ReviewSpatialized = reviewSpatialized;
                LoopIntent = loopIntent;
            }

            public string Category { get; }
            public string FileName { get; }
            public int Channels { get; }
            public bool ReviewSpatialized { get; }
            public bool LoopIntent { get; }
            public string AssetPath => $"{ReviewAudioRoot}/{FileName}";
        }
    }
}
