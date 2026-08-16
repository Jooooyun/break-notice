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
    public sealed class ShowcaseAudioImpactLadderReviewContractTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/90_Review/LOCAL_REVIEW_ONLY_ShowcaseAudioImpactLadder_v004.unity";
        private const string RootName = "LOCAL_REVIEW_ONLY_SHOWCASE_AUDIO_IMPACT_LADDER_V004";
        private const string AudioFolder =
            "Assets/_Project/Audio/Review/Showcase/Rebuild_v004";

        private static readonly string[] CueIds =
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

        private static readonly string[] StemNames =
        {
            "pre",
            "signature",
            "emitter",
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
        public void Scene_IsLocalReviewOnly_BuildExcluded_AndHasNoRuntimePhysicsOrAnimation()
        {
            Assert.That(
                EditorBuildSettings.scenes.Select(item => item.path),
                Does.Not.Contain(ScenePath));
            Assert.That(root.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<Rigidbody>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<Animator>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<Animation>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<Camera>(true), Has.Length.EqualTo(1));
            Assert.That(root.GetComponentsInChildren<AudioListener>(true), Has.Length.EqualTo(1));
        }

        [Test]
        public void PlaybackGroups_AreExactElevenCueLadder_WithFiveAlignedStemsEach()
        {
            Transform playbackRoot = root.transform.Find("PlaybackGroups");
            Assert.That(playbackRoot, Is.Not.Null);
            Assert.That(playbackRoot.childCount, Is.EqualTo(11));
            Assert.That(CueIds.Count(id => id.StartsWith("T1_HAND_", StringComparison.Ordinal)), Is.EqualTo(3));
            Assert.That(CueIds.Count(id => id.StartsWith("T2_HOOK_", StringComparison.Ordinal)), Is.EqualTo(3));
            Assert.That(CueIds.Count(id => id.StartsWith("T2_BINDER_", StringComparison.Ordinal)), Is.EqualTo(3));
            Assert.That(CueIds.Count(id => id.StartsWith("T3_CHAIR_", StringComparison.Ordinal)), Is.EqualTo(2));

            foreach (string cueId in CueIds)
            {
                Transform group = playbackRoot.Find(cueId);
                Assert.That(group, Is.Not.Null, cueId);
                Assert.That(group.gameObject.activeSelf, Is.False, cueId);
                Assert.That(group.GetComponentsInChildren<AudioSource>(true), Has.Length.EqualTo(5));
                CollectionAssert.AreEqual(StemNames, group.Cast<Transform>().Select(child => child.name));
            }
        }

        [Test]
        public void EveryStem_Is48kStereoPcm_Decompressed_Aligned_AndUnmixed()
        {
            Transform playbackRoot = root.transform.Find("PlaybackGroups");
            AudioSource[] sources = playbackRoot.GetComponentsInChildren<AudioSource>(true);
            Assert.That(sources, Has.Length.EqualTo(55));
            foreach (AudioSource source in sources)
            {
                Assert.That(source.clip, Is.Not.Null, source.name);
                Assert.That(source.clip.frequency, Is.EqualTo(48000), source.clip.name);
                Assert.That(source.clip.channels, Is.EqualTo(2), source.clip.name);
                Assert.That(source.clip.length, Is.EqualTo(1.35f).Within(0.002f), source.clip.name);
                Assert.That(source.clip.samples, Is.EqualTo(64800), source.clip.name);
                Assert.That(source.outputAudioMixerGroup, Is.Null, source.clip.name);
                Assert.That(source.playOnAwake, Is.False, source.clip.name);
                Assert.That(source.loop, Is.False, source.clip.name);
                Assert.That(source.volume, Is.EqualTo(1f), source.clip.name);
                Assert.That(source.pitch, Is.EqualTo(1f), source.clip.name);
                Assert.That(source.dopplerLevel, Is.Zero, source.clip.name);

                string path = AssetDatabase.GetAssetPath(source.clip);
                Assert.That(path, Does.StartWith(AudioFolder + "/Stems/"));
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                Assert.That(importer, Is.Not.Null, path);
                Assert.That(importer.forceToMono, Is.False, path);
                Assert.That(importer.defaultSampleSettings.preloadAudioData, Is.True, path);
                Assert.That(importer.defaultSampleSettings.loadType, Is.EqualTo(AudioClipLoadType.DecompressOnLoad), path);
                Assert.That(importer.defaultSampleSettings.compressionFormat, Is.EqualTo(AudioCompressionFormat.PCM), path);
            }
        }

        [Test]
        public void Signature_Is2D_AndMaterialStemsArePositionedWithoutMixer()
        {
            Transform playbackRoot = root.transform.Find("PlaybackGroups");
            foreach (Transform group in playbackRoot)
            {
                Assert.That(group.Find("signature").GetComponent<AudioSource>().spatialBlend, Is.Zero);
                Assert.That(group.Find("pre").GetComponent<AudioSource>().spatialBlend, Is.EqualTo(0.15f));
                Assert.That(group.Find("emitter").GetComponent<AudioSource>().spatialBlend, Is.EqualTo(0.85f));
                Assert.That(group.Find("runner").GetComponent<AudioSource>().spatialBlend, Is.EqualTo(0.85f));
                Assert.That(group.Find("floor_tail").GetComponent<AudioSource>().spatialBlend, Is.EqualTo(0.80f));
                Assert.That(
                    group.GetComponentsInChildren<AudioSource>(true).All(item => item.outputAudioMixerGroup == null),
                    Is.True,
                    group.name);
            }
        }

        [Test]
        public void SceneDependencies_StayInsideV004ReviewAssets()
        {
            string[] dependencies = AssetDatabase.GetDependencies(ScenePath, true);
            Assert.That(dependencies.Any(path => path.Contains("Rebuild_v003", StringComparison.OrdinalIgnoreCase)), Is.False);
            Assert.That(dependencies.Any(path => path.Contains("AudioMixer", StringComparison.OrdinalIgnoreCase)), Is.False);
            Assert.That(dependencies, Does.Not.Contain(
                "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v007.unity"));
            foreach (string dependency in dependencies.Where(path => path.StartsWith("Assets/_Project/")))
            {
                Assert.That(
                    dependency == ScenePath || dependency.StartsWith(AudioFolder + "/"),
                    Is.True,
                    dependency);
            }
        }

        [Test]
        public void Manifest_DeclaresExactCueStemAndContactContract()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string fullPath = Path.Combine(
                projectRoot,
                (AudioFolder + "/REVIEW_MANIFEST.json").Replace('/', Path.DirectorySeparatorChar));
            Assert.That(File.Exists(fullPath), Is.True);
            ReviewManifest manifest = JsonUtility.FromJson<ReviewManifest>(File.ReadAllText(fullPath));
            Assert.That(manifest, Is.Not.Null);
            Assert.That(manifest.status, Is.EqualTo("REVIEW_CANDIDATE_NOT_FINAL_NOT_RUNTIME"));
            Assert.That(manifest.sample_rate, Is.EqualTo(48000));
            Assert.That(manifest.channels, Is.EqualTo(2));
            Assert.That(manifest.frames, Is.EqualTo(64800));
            Assert.That(manifest.contact_seconds, Is.EqualTo(0.2f).Within(0.0001f));
            CollectionAssert.AreEqual(StemNames, manifest.stems);
            CollectionAssert.AreEqual(CueIds, manifest.cues.Select(item => item.cue_id));
            foreach (CueManifest cue in manifest.cues)
            {
                Assert.That(cue.stems, Has.Length.EqualTo(5), cue.cue_id);
                CollectionAssert.AreEqual(StemNames, cue.stems.Select(item => item.name), cue.cue_id);
            }
        }

        [Test]
        public void ReviewAudio_CountsAndSurfacesAreExact()
        {
            string[] stemPaths = FindAudioPaths(AudioFolder + "/Stems");
            string[] compositePaths = FindAudioPaths(AudioFolder + "/Composites");
            string[] derivedPaths = FindAudioPaths(AudioFolder + "/Derived");
            string[] referencePaths = FindAudioPaths(AudioFolder + "/ReferenceOnly");
            Assert.That(stemPaths, Has.Length.EqualTo(55));
            Assert.That(compositePaths, Has.Length.EqualTo(11));
            Assert.That(derivedPaths, Has.Length.EqualTo(4));
            Assert.That(referencePaths, Has.Length.EqualTo(2));
            CollectionAssert.AreEquivalent(
                CueIds.Select(id => $"{AudioFolder}/Composites/{id}_REVIEW.wav"),
                compositePaths);
        }

        [Test]
        public void ProductionStemsAndComposites_AreAllByteDistinct()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string[] paths = FindAudioPaths(AudioFolder + "/Stems")
                .Concat(FindAudioPaths(AudioFolder + "/Composites"))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            Assert.That(paths, Has.Length.EqualTo(66));
            string[] hashes = paths.Select(path => ComputeSha256(File.ReadAllBytes(Path.Combine(
                projectRoot,
                path.Replace('/', Path.DirectorySeparatorChar))))).ToArray();
            Assert.That(hashes.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(66));
        }

        private static string[] FindAudioPaths(string folder)
        {
            return AssetDatabase.FindAssets("t:AudioClip", new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
        }

        private static string ComputeSha256(byte[] bytes)
        {
            using SHA256 sha256 = SHA256.Create();
            return string.Concat(sha256.ComputeHash(bytes).Select(value => value.ToString("x2")));
        }

        [Serializable]
        private sealed class ReviewManifest
        {
            public string status;
            public int sample_rate;
            public int channels;
            public int frames;
            public float contact_seconds;
            public string[] stems;
            public CueManifest[] cues;
        }

        [Serializable]
        private sealed class CueManifest
        {
            public string cue_id;
            public StemManifest[] stems;
        }

        [Serializable]
        private sealed class StemManifest
        {
            public string name;
        }
    }
}

