using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestUnarmedCombatAudioContractTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v007.unity";
        private const string AudioRootName = "SHOWCASE_UNARMED_COMBAT_AUDIO";
        private const string AudioFolder = "Assets/_Project/Audio/Impact/Unarmed";
        private const int VoiceCount = 8;
        private readonly struct SelectedClip
        {
            public SelectedClip(
                string path,
                string sha256,
                int samples,
                ShowcaseUnarmedCombatAudioTake take,
                int leadSamples)
            {
                Path = path;
                Sha256 = sha256;
                Samples = samples;
                Take = take;
                LeadSamples = leadSamples;
            }

            public string Path { get; }
            public string Sha256 { get; }
            public int Samples { get; }
            public ShowcaseUnarmedCombatAudioTake Take { get; }
            public int LeadSamples { get; }
        }

        private static readonly SelectedClip[] SelectedClips =
        {
            new(
                AudioFolder + "/BN_BODY_IMPACT_06_FULL.wav",
                "0c63b004096846543c85adfd950ddfec8903904ec57a24f72f7fb62dff4e1889",
                40838,
                ShowcaseUnarmedCombatAudioTake.P01DeepBody,
                ShowcaseUnarmedCombatAudioTiming.P01LeadSamples),
            new(
                AudioFolder + "/BN_FACE_PUNCH_02_FULL.wav",
                "3d2c5577cddf4f1c51b2348cc50ce0ed03ca176cbca763aa2adf1503b218fd27",
                37765,
                ShowcaseUnarmedCombatAudioTake.P03Cross,
                ShowcaseUnarmedCombatAudioTiming.P03LeadSamples),
            new(
                AudioFolder + "/BN_FACE_PUNCH_03_FULL.wav",
                "99ba380822f0d9b45b539c0217434ba59cff7aa595a916c5b52834d3ac188fa7",
                39173,
                ShowcaseUnarmedCombatAudioTake.P04AlternateCross,
                ShowcaseUnarmedCombatAudioTiming.P04LeadSamples),
            new(
                AudioFolder + "/BN_FACE_PUNCH_01_FULL.wav",
                "5ea948fbbe47bc9df71558a17262e3b1c349499eca57efa235bd46f4cbe88f80",
                32540,
                ShowcaseUnarmedCombatAudioTake.P08Jab,
                ShowcaseUnarmedCombatAudioTiming.P08LeadSamples),
            new(
                AudioFolder + "/BN_FACE_PUNCH_04_FULL.wav",
                "6e10a5c9f79fd0eebdafbf10fc307e267377ef42cde16a6e3f521d67d518a526",
                49516,
                ShowcaseUnarmedCombatAudioTake.P09HeavyHook,
                ShowcaseUnarmedCombatAudioTiming.P09LeadSamples),
            new(
                AudioFolder + "/BN_BODY_IMPACT_02_FULL.wav",
                "df976cb1b0b39f13498f9b165c2800b847e0193291780de9beda3f6103dd08d7",
                38514,
                ShowcaseUnarmedCombatAudioTake.P10FastBody,
                ShowcaseUnarmedCombatAudioTiming.P10LeadSamples)
        };

        [Test]
        public void SelectedFullWavs_PreserveBytesAndPcmImportContract()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            Assert.That(projectRoot, Is.Not.Null);
            foreach (SelectedClip selected in SelectedClips)
            {
                string absolutePath = System.IO.Path.Combine(
                    projectRoot,
                    selected.Path.Replace('/', System.IO.Path.DirectorySeparatorChar));
                Assert.That(File.Exists(absolutePath), Is.True, selected.Path);
                using SHA256 sha = SHA256.Create();
                using FileStream stream = File.OpenRead(absolutePath);
                string hash = BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", string.Empty)
                    .ToLowerInvariant();
                Assert.That(hash, Is.EqualTo(selected.Sha256), selected.Path);

                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(selected.Path);
                Assert.That(clip, Is.Not.Null, selected.Path);
                Assert.That(clip.frequency, Is.EqualTo(ShowcaseUnarmedCombatAudioTiming.SampleRate),
                    selected.Path);
                Assert.That(clip.channels, Is.EqualTo(2), selected.Path);
                Assert.That(clip.samples, Is.EqualTo(selected.Samples), selected.Path);

                AudioImporter importer = AssetImporter.GetAtPath(selected.Path) as AudioImporter;
                Assert.That(importer, Is.Not.Null, selected.Path);
                Assert.That(importer.forceToMono, Is.False, selected.Path);
                Assert.That(importer.ambisonic, Is.False, selected.Path);
                Assert.That(importer.loadInBackground, Is.False, selected.Path);
                string importerYaml = File.ReadAllText(absolutePath + ".meta");
                StringAssert.Contains("normalize: 0", importerYaml, selected.Path);
                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                Assert.That(settings.preloadAudioData, Is.True, selected.Path);
                Assert.That(settings.loadType, Is.EqualTo(AudioClipLoadType.DecompressOnLoad),
                    selected.Path);
                Assert.That(settings.compressionFormat,
                    Is.EqualTo(AudioCompressionFormat.PCM), selected.Path);
                Assert.That(settings.sampleRateSetting,
                    Is.EqualTo(AudioSampleRateSetting.PreserveSampleRate), selected.Path);
            }
        }

        [Test]
        public void TimingAuthority_MapsOneImpactSamplePerAcceptedUnarmedContact()
        {
            AssertPlayerPlan(
                ShowcasePlayerAnimationCue.MeleeJab,
                ShowcaseUnarmedCombatAudioTake.P08Jab,
                ShowcaseUnarmedCombatAudioTiming.P08LeadSamples);
            AssertPlayerPlan(
                ShowcasePlayerAnimationCue.MeleeCross,
                ShowcaseUnarmedCombatAudioTake.P03Cross,
                ShowcaseUnarmedCombatAudioTiming.P03LeadSamples);
            AssertPlayerPlan(
                ShowcasePlayerAnimationCue.MeleeHook,
                ShowcaseUnarmedCombatAudioTake.P09HeavyHook,
                ShowcaseUnarmedCombatAudioTiming.P09LeadSamples);

            AssertRunnerPlan(
                ShowcaseRunnerAnimationCue.Boxing,
                ShowcaseUnarmedCombatAudioTake.P03Cross,
                ShowcaseUnarmedCombatAudioTake.P03Cross,
                ShowcaseUnarmedCombatAudioTiming.P03LeadSamples);
            AssertRunnerPlan(
                ShowcaseRunnerAnimationCue.Boxing,
                ShowcaseUnarmedCombatAudioTake.P04AlternateCross,
                ShowcaseUnarmedCombatAudioTake.P04AlternateCross,
                ShowcaseUnarmedCombatAudioTiming.P04LeadSamples);
            AssertRunnerPlan(
                ShowcaseRunnerAnimationCue.Boxing2,
                ShowcaseUnarmedCombatAudioTake.P03Cross,
                ShowcaseUnarmedCombatAudioTake.P01DeepBody,
                ShowcaseUnarmedCombatAudioTiming.P01LeadSamples);
            AssertRunnerPlan(
                ShowcaseRunnerAnimationCue.Kicking,
                ShowcaseUnarmedCombatAudioTake.P03Cross,
                ShowcaseUnarmedCombatAudioTake.P01DeepBody,
                ShowcaseUnarmedCombatAudioTiming.P01LeadSamples);
            AssertRunnerPlan(
                ShowcaseRunnerAnimationCue.Kicking2,
                ShowcaseUnarmedCombatAudioTake.P03Cross,
                ShowcaseUnarmedCombatAudioTake.P10FastBody,
                ShowcaseUnarmedCombatAudioTiming.P10LeadSamples);
            AssertRunnerPlan(
                ShowcaseRunnerAnimationCue.EliteMutantPunch,
                ShowcaseUnarmedCombatAudioTake.P04AlternateCross,
                ShowcaseUnarmedCombatAudioTake.P10FastBody,
                ShowcaseUnarmedCombatAudioTiming.P10LeadSamples);
            AssertRunnerPlan(
                ShowcaseRunnerAnimationCue.EliteMutantSwiping,
                ShowcaseUnarmedCombatAudioTake.P04AlternateCross,
                ShowcaseUnarmedCombatAudioTake.P01DeepBody,
                ShowcaseUnarmedCombatAudioTiming.P01LeadSamples);

            Assert.That(ShowcaseUnarmedCombatAudioController.ConfirmedImpactVolume,
                Is.EqualTo(0.4f));

            Assert.That(ShowcaseUnarmedCombatAudioTiming.TryResolvePlayer(
                ShowcasePlayerAnimationCue.HitBody,
                out _), Is.False,
                "A victim reaction must not layer a second full take over its attacker.");
            Assert.That(ShowcaseUnarmedCombatAudioTiming.TryResolveRunner(
                ShowcaseRunnerAnimationCue.HitBody,
                ShowcaseUnarmedCombatAudioTake.P03Cross,
                out _), Is.False,
                "A victim reaction must not layer a second full take over its attacker.");
        }

        [Test]
        public void S7Scene_HasOneBoundedSpatialUnarmedAudioDirector()
        {
            Scene scene = EditorSceneManager.OpenScene(
                ScenePath,
                OpenSceneMode.Single);
            ShowcaseUnarmedCombatAudioController director = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<
                    ShowcaseUnarmedCombatAudioController>(true))
                .Single();

            Assert.That(director.gameObject.name,
                Is.EqualTo(AudioRootName));
            Assert.That(director.PlayerCombat, Is.Not.Null);
            Assert.That(director.PressureController, Is.Not.Null);
            Assert.That(director.Runners.Length, Is.EqualTo(2));
            Assert.That(director.AuthoredVoices.Length,
                Is.EqualTo(VoiceCount));
            AudioListener listener = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<AudioListener>(true))
                .Single();
            Assert.That(listener.enabled, Is.True);
            Assert.That(listener.GetComponent<Camera>(), Is.Not.Null);
            Assert.That(listener.gameObject.name, Is.EqualTo("OutputCamera"));
            CollectionAssert.AreEquivalent(
                SelectedClips.Select(item =>
                    System.IO.Path.GetFileNameWithoutExtension(item.Path)),
                new[]
                {
                    director.P01DeepBody.name,
                    director.P03Cross.name,
                    director.P04AlternateCross.name,
                    director.P08Jab.name,
                    director.P09HeavyHook.name,
                    director.P10FastBody.name
                });

            foreach (AudioSource voice in director.AuthoredVoices)
            {
                Assert.That(voice, Is.Not.Null);
                Assert.That(voice.playOnAwake, Is.False);
                Assert.That(voice.loop, Is.False);
                Assert.That(voice.volume,
                    Is.EqualTo(ShowcaseUnarmedCombatAudioController.
                        ConfirmedImpactVolume));
                Assert.That(voice.pitch, Is.EqualTo(1f));
                Assert.That(voice.spatialBlend, Is.EqualTo(1f));
                Assert.That(voice.dopplerLevel, Is.EqualTo(0f));
                Assert.That(voice.bypassEffects, Is.True);
                Assert.That(voice.bypassReverbZones, Is.True);
            }
        }

        private static void AssertPlayerPlan(
            ShowcasePlayerAnimationCue cue,
            ShowcaseUnarmedCombatAudioTake expectedTake,
            int expectedLeadSamples)
        {
            Assert.That(ShowcaseUnarmedCombatAudioTiming.TryResolvePlayer(
                cue,
                out ShowcaseUnarmedCombatAudioPlan plan), Is.True);
            Assert.That(plan.Take, Is.EqualTo(expectedTake));
            Assert.That(plan.LeadSamples, Is.EqualTo(expectedLeadSamples));
        }

        private static void AssertRunnerPlan(
            ShowcaseRunnerAnimationCue cue,
            ShowcaseUnarmedCombatAudioTake selectedFaceTake,
            ShowcaseUnarmedCombatAudioTake expectedTake,
            int expectedLeadSamples)
        {
            Assert.That(ShowcaseUnarmedCombatAudioTiming.TryResolveRunner(
                cue,
                selectedFaceTake,
                out ShowcaseUnarmedCombatAudioPlan plan), Is.True);
            Assert.That(plan.Take, Is.EqualTo(expectedTake));
            Assert.That(plan.LeadSamples, Is.EqualTo(expectedLeadSamples));
        }
    }
}
