using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ShowcaseAudioImpactReviewContractTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/90_Review/LOCAL_REVIEW_ONLY_ShowcaseAudioImpact_v003.unity";
        private const string RootName = "LOCAL_REVIEW_ONLY_SHOWCASE_AUDIO_IMPACT_V003";
        private const string AudioFolder =
            "Assets/_Project/Audio/Review/Showcase/Rebuild_v003";
        private const string MotionClipPath =
            AudioFolder + "/LOCAL_REVIEW_ONLY_NorthStarMotion_v003.anim";
        private static readonly string[] BlindLabels = { "X", "Y", "Z" };
        private static readonly string[] StemNames =
        {
            "pre",
            "signature",
            "chair",
            "runner",
            "floor_tail"
        };

        private Scene scene;
        private GameObject root;

        [SetUp]
        public void SetUp()
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath), Is.Not.Null);
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);
            root = scene.GetRootGameObjects().Single(item => item.name == RootName);
        }

        [TearDown]
        public void TearDown()
        {
            if (scene.IsValid() && scene.isLoaded)
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Scene_IsLocalReviewOnly_AndExcludedFromBuildSettings()
        {
            Assert.That(
                EditorBuildSettings.scenes.Select(item => item.path),
                Does.Not.Contain(ScenePath));
            Assert.That(root.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<Camera>(true), Has.Length.EqualTo(1));
            Assert.That(root.GetComponentsInChildren<AudioListener>(true), Has.Length.EqualTo(1));
            Assert.That(root.GetComponentsInChildren<Collider>(true), Is.Empty);
        }

        [Test]
        public void PlaybackGroups_AreExactlyThreeBlindDirections_TwoVariations_FiveStems()
        {
            Transform playbackRoot = root.transform.Find("PlaybackGroups");
            Assert.That(playbackRoot, Is.Not.Null);
            Assert.That(playbackRoot.childCount, Is.EqualTo(6));
            foreach (string blindLabel in BlindLabels)
            {
                for (int variation = 1; variation <= 2; variation++)
                {
                    Transform group = playbackRoot.Find($"BLIND_{blindLabel}_v{variation:00}");
                    Assert.That(group, Is.Not.Null);
                    Assert.That(group.gameObject.activeSelf, Is.False);
                    Assert.That(group.GetComponentsInChildren<AudioSource>(true), Has.Length.EqualTo(5));
                    CollectionAssert.AreEquivalent(
                        StemNames,
                        group.Cast<Transform>().Select(child => child.name));
                }
            }
        }

        [Test]
        public void EveryStem_Is48kStereoPcm_Decompressed_Aligned_AndUnmixed()
        {
            Transform playbackRoot = root.transform.Find("PlaybackGroups");
            foreach (AudioSource audioSource in playbackRoot.GetComponentsInChildren<AudioSource>(true))
            {
                Assert.That(audioSource.clip, Is.Not.Null, audioSource.name);
                Assert.That(audioSource.clip.frequency, Is.EqualTo(48000), audioSource.clip.name);
                Assert.That(audioSource.clip.channels, Is.EqualTo(2), audioSource.clip.name);
                Assert.That(audioSource.clip.length, Is.EqualTo(1.35f).Within(0.002f), audioSource.clip.name);
                Assert.That(audioSource.clip.samples, Is.EqualTo(64800), audioSource.clip.name);
                Assert.That(audioSource.outputAudioMixerGroup, Is.Null, audioSource.clip.name);
                Assert.That(audioSource.playOnAwake, Is.False, audioSource.clip.name);
                Assert.That(audioSource.loop, Is.False, audioSource.clip.name);
                Assert.That(audioSource.volume, Is.EqualTo(1f), audioSource.clip.name);
                Assert.That(audioSource.pitch, Is.EqualTo(1f), audioSource.clip.name);
                Assert.That(audioSource.dopplerLevel, Is.Zero, audioSource.clip.name);

                string path = AssetDatabase.GetAssetPath(audioSource.clip);
                Assert.That(path, Does.StartWith(AudioFolder + "/Stems/"));
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                Assert.That(importer, Is.Not.Null, path);
                Assert.That(importer.forceToMono, Is.False, path);
                Assert.That(importer.defaultSampleSettings.preloadAudioData, Is.True, path);
                Assert.That(
                    importer.defaultSampleSettings.loadType,
                    Is.EqualTo(AudioClipLoadType.DecompressOnLoad),
                    path);
                Assert.That(
                    importer.defaultSampleSettings.compressionFormat,
                    Is.EqualTo(AudioCompressionFormat.PCM),
                    path);
            }
        }

        [Test]
        public void SignatureIs2D_AndMaterialStemsArePositionedWithoutMixer()
        {
            Transform playbackRoot = root.transform.Find("PlaybackGroups");
            foreach (Transform group in playbackRoot)
            {
                Assert.That(group.Find("signature").GetComponent<AudioSource>().spatialBlend, Is.Zero);
                Assert.That(group.Find("pre").GetComponent<AudioSource>().spatialBlend, Is.EqualTo(0.15f));
                Assert.That(group.Find("chair").GetComponent<AudioSource>().spatialBlend, Is.EqualTo(0.85f));
                Assert.That(group.Find("runner").GetComponent<AudioSource>().spatialBlend, Is.EqualTo(0.85f));
                Assert.That(group.Find("floor_tail").GetComponent<AudioSource>().spatialBlend, Is.EqualTo(0.80f));
            }
        }

        [Test]
        public void MotionClip_HasOnePointThreeFiveSecondReviewWindow_NoAnimationEvents()
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(MotionClipPath);
            Assert.That(clip, Is.Not.Null);
            Assert.That(clip.legacy, Is.True);
            Assert.That(clip.length, Is.EqualTo(1.35f).Within(0.001f));
            Assert.That(AnimationUtility.GetAnimationEvents(clip), Is.Empty);
            Assert.That(root.transform.Find("NorthStarStage").GetComponent<Animation>(), Is.Not.Null);
        }

        [Test]
        public void SceneDependencies_StayInsideReviewAssets_AndUseNoGameplayScene()
        {
            string[] dependencies = AssetDatabase.GetDependencies(ScenePath, true);
            Assert.That(dependencies, Does.Not.Contain(
                "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v007.unity"));
            Assert.That(dependencies.Any(path => path.Contains("AudioMixer", StringComparison.OrdinalIgnoreCase)), Is.False);
            foreach (string dependency in dependencies.Where(path => path.StartsWith("Assets/_Project/")))
            {
                Assert.That(
                    dependency == ScenePath || dependency.StartsWith(AudioFolder + "/"),
                    Is.True,
                    dependency);
            }
        }

        [Test]
        public void BlindStemFiles_AreByteDistinctAcrossDirectionsAndVariations()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string[] paths = AssetDatabase.FindAssets("t:AudioClip", new[] { AudioFolder + "/Stems" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            Assert.That(paths, Has.Length.EqualTo(30));
            string[] hashes = paths.Select(path =>
                    ComputeSha256(File.ReadAllBytes(Path.Combine(
                        projectRoot,
                        path.Replace('/', Path.DirectorySeparatorChar)))))
                .ToArray();
            Assert.That(hashes.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(30));
        }

        private static string ComputeSha256(byte[] bytes)
        {
            using SHA256 sha256 = SHA256.Create();
            return string.Concat(
                sha256.ComputeHash(bytes).Select(value => value.ToString("x2")));
        }
    }
}
