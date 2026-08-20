using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BreakNotice.Gameplay;
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
    public sealed class ContestFinalAudioV004ContractTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/20_Showcase/" +
            "Contest_OperationsFloor_Showcase_v007.unity";
        private const string AudioRoot = "Assets/_Project/Audio/Final_v004";
        private const string ProfilePath =
            AudioRoot + "/Config/FinalGameplayAudioProfile.asset";
        private const string FinalRootName = "SHOWCASE_FINAL_GAMEPLAY_AUDIO";
        private const string AuthoringTypeName =
            "BreakNotice.Contest.Editor.ContestFinalAudioV004Authoring";
        private const int RuntimeClipCount = 40;
        private const int ExpectedSceneAudioSourceCount = 36;

        private static readonly string[] ExpectedRuntimePaths =
        {
            Foley("BN_FOL_BINDER_PICKUP_03"),
            Foley("BN_FOL_BINDER_RELEASE_02"),
            Foley("BN_FOL_BINDER_THROW_01"),
            Foley("BN_FOL_CHAIR_PICKUP_03"),
            Foley("BN_FOL_CHAIR_RELEASE_02"),
            Foley("BN_FOL_CHAIR_THROW_02"),
            Foley("BN_FOL_CHAIR_SWING_WHOOSH_MEDIUM_B"),
            Foley("BN_FOL_FOOTSTEP_OFFICE_HARD_01"),
            Foley("BN_FOL_FOOTSTEP_OFFICE_HARD_02"),
            Foley("BN_FOL_FOOTSTEP_OFFICE_HARD_03"),
            Foley("BN_FOL_FOOTSTEP_OFFICE_HARD_04"),
            Foley("BN_FOL_FOOTSTEP_OFFICE_HARD_05"),
            Foley("BN_FOL_FOOTSTEP_OFFICE_HARD_06"),
            Foley("BN_FOL_FOOTSTEP_OFFICE_HARD_07"),
            Foley("BN_FOL_FOOTSTEP_OFFICE_HARD_08"),
            Impact("BN_SFX_BINDER_CATCH_A"),
            Impact("BN_SFX_BINDER_DIRECT_HIT_A"),
            Impact("BN_SFX_BINDER_FLOOR_DROP_B"),
            Impact("BN_SFX_BINDER_WALL_HIT_B"),
            Impact("BN_SFX_CHAIR_ENEMY_MAIN_IMPACT_B"),
            Impact("BN_SFX_CHAIR_STOP_A"),
            Impact("BN_SFX_CHAIR_STOP_B"),
            Impact("BN_SFX_CHAIR_WORLD_IMPACT_A"),
            Motion("BN_WP01_CHAIR_ROLL_LOOP_02"),
            Motion("BN_WP01_CHAIR_SKID_01"),
            Motion("BN_FOL_SMALL_OBJECT_SPIN_LOOP_A"),
            Strap("BN_FOL_STRAP_ACTUAL_ZIPPER_Z01_OPEN_FULL"),
            Strap("BN_FOL_STRAP_ACTUAL_ZIPPER_Z02_CLOSE_FULL"),
            Strap("BN_FOL_STRAP_LOCK_02"),
            Strap("BN_FOL_STRAP_TENSION_02"),
            Voice("BN_VOX_PLAYER_LIGHT_HURT_A"),
            Voice("BN_VOX_PLAYER_LIGHT_HURT_B"),
            Voice("BN_VOX_PLAYER_HEAVY_HURT_B"),
            Voice("BN_VOX_PLAYER_KO_DEATH_A"),
            Voice("BN_VOX_PLAYER_KO_DEATH_B"),
            Voice("BN_VOX_ENEMY_LIGHT_HURT_A"),
            Voice("BN_VOX_ENEMY_LIGHT_HURT_B"),
            Voice("BN_VOX_ENEMY_LIGHT_HURT_D"),
            Voice("BN_VOX_ENEMY_KO_DEATH_A"),
            Voice("BN_VOX_ENEMY_KO_DEATH_B")
        };

        [Test]
        public void Profile_MapsExactlyFortyDistinctApprovedRuntimeClips()
        {
            FinalGameplayAudioProfile profile = AssetDatabase.LoadAssetAtPath<
                FinalGameplayAudioProfile>(ProfilePath);
            Assert.That(profile, Is.Not.Null, ProfilePath);

            AssertClip(profile.BinderPickupClip, Foley("BN_FOL_BINDER_PICKUP_03"));
            AssertClip(profile.BinderThrowClip, Foley("BN_FOL_BINDER_RELEASE_02"));
            AssertClip(profile.BinderPlaceDownClip, Foley("BN_FOL_BINDER_THROW_01"));
            AssertClip(profile.BinderDirectHitClip,
                Impact("BN_SFX_BINDER_DIRECT_HIT_A"));
            AssertClip(profile.BinderWallHitClip,
                Impact("BN_SFX_BINDER_WALL_HIT_B"));
            AssertClip(profile.BinderFloorDropClip,
                Impact("BN_SFX_BINDER_FLOOR_DROP_B"));
            AssertClip(profile.BinderCatchClip, Impact("BN_SFX_BINDER_CATCH_A"));

            AssertClip(profile.ChairPickupClip, Foley("BN_FOL_CHAIR_PICKUP_03"));
            AssertClip(profile.ChairReleaseClip, Foley("BN_FOL_CHAIR_RELEASE_02"));
            AssertClip(profile.ChairThrowClip, Foley("BN_FOL_CHAIR_THROW_02"));
            AssertClip(profile.ChairSwingWhooshClip,
                Foley("BN_FOL_CHAIR_SWING_WHOOSH_MEDIUM_B"));
            AssertClip(profile.ChairRollLoopClip,
                Motion("BN_WP01_CHAIR_ROLL_LOOP_02"));
            AssertClip(profile.ChairSkidClip, Motion("BN_WP01_CHAIR_SKID_01"));
            AssertClip(profile.ChairEnemyImpactClip,
                Impact("BN_SFX_CHAIR_ENEMY_MAIN_IMPACT_B"));
            AssertClip(profile.ChairWorldImpactClip,
                Impact("BN_SFX_CHAIR_WORLD_IMPACT_A"));
            AssertPool(profile.ChairStopPool,
                Impact("BN_SFX_CHAIR_STOP_A"),
                Impact("BN_SFX_CHAIR_STOP_B"));

            AssertClip(profile.StrapExtendPart1Clip,
                Strap("BN_FOL_STRAP_ACTUAL_ZIPPER_Z01_OPEN_FULL"));
            AssertClip(profile.StrapExtendPart2Clip,
                Strap("BN_FOL_STRAP_ACTUAL_ZIPPER_Z02_CLOSE_FULL"));
            AssertClip(profile.StrapLockClip, Strap("BN_FOL_STRAP_LOCK_02"));
            AssertClip(profile.StrapTensionClip,
                Strap("BN_FOL_STRAP_TENSION_02"));
            AssertPool(profile.FootstepPool, Enumerable.Range(1, 8)
                .Select(index => Foley(
                    $"BN_FOL_FOOTSTEP_OFFICE_HARD_{index:00}"))
                .ToArray());
            AssertClip(profile.SmallObjectSpinLoopClip,
                Motion("BN_FOL_SMALL_OBJECT_SPIN_LOOP_A"));

            AssertPool(profile.PlayerLightHurtPool,
                Voice("BN_VOX_PLAYER_LIGHT_HURT_A"),
                Voice("BN_VOX_PLAYER_LIGHT_HURT_B"));
            AssertClip(profile.PlayerHeavyHurtClip,
                Voice("BN_VOX_PLAYER_HEAVY_HURT_B"));
            AssertPool(profile.PlayerKoPool,
                Voice("BN_VOX_PLAYER_KO_DEATH_A"),
                Voice("BN_VOX_PLAYER_KO_DEATH_B"));
            AssertPool(profile.EnemyLightHurtPool,
                Voice("BN_VOX_ENEMY_LIGHT_HURT_A"),
                Voice("BN_VOX_ENEMY_LIGHT_HURT_B"),
                Voice("BN_VOX_ENEMY_LIGHT_HURT_D"));
            AssertPool(profile.EnemyKoPool,
                Voice("BN_VOX_ENEMY_KO_DEATH_A"),
                Voice("BN_VOX_ENEMY_KO_DEATH_B"));

            AudioClip[] clips = profile.EnumerateRequiredClips().ToArray();
            Assert.That(clips, Has.Length.EqualTo(RuntimeClipCount));
            Assert.That(clips.All(clip => clip != null), Is.True);
            Assert.That(clips.Distinct().Count(), Is.EqualTo(RuntimeClipCount));
            CollectionAssert.AreEquivalent(
                ExpectedRuntimePaths,
                clips.Select(AssetDatabase.GetAssetPath));
        }

        [Test]
        public void RuntimeFolder_HasExactlyFortyClipsNoReelAndPcmImportContract()
        {
            string[] actualPaths = AssetDatabase.FindAssets(
                    "t:AudioClip",
                    new[] { AudioRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".wav",
                    StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();

            Assert.That(ExpectedRuntimePaths.Distinct(StringComparer.Ordinal).Count(),
                Is.EqualTo(RuntimeClipCount));
            Assert.That(actualPaths, Has.Length.EqualTo(RuntimeClipCount));
            CollectionAssert.AreEquivalent(ExpectedRuntimePaths, actualPaths);
            Assert.That(actualPaths.Any(path => path.IndexOf(
                    "BN_ENEMY_LIGHT_VOICE_REEL",
                    StringComparison.OrdinalIgnoreCase) >= 0),
                Is.False);

            foreach (string path in actualPaths)
            {
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                Assert.That(clip, Is.Not.Null, path);
                Assert.That(clip.frequency, Is.EqualTo(48000), path);

                AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
                Assert.That(importer, Is.Not.Null, path);
                Assert.That(importer.forceToMono, Is.True, path);
                Assert.That(importer.ambisonic, Is.False, path);
                Assert.That(importer.loadInBackground, Is.False, path);
                var serialized = new SerializedObject(importer);
                SerializedProperty normalize = serialized.FindProperty("m_Normalize");
                if (normalize != null)
                {
                    Assert.That(normalize.boolValue, Is.False, path);
                }
                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                Assert.That(settings.preloadAudioData, Is.True, path);
                Assert.That(settings.loadType,
                    Is.EqualTo(AudioClipLoadType.DecompressOnLoad), path);
                Assert.That(settings.compressionFormat,
                    Is.EqualTo(AudioCompressionFormat.PCM), path);
                Assert.That(settings.sampleRateSetting,
                    Is.EqualTo(AudioSampleRateSetting.PreserveSampleRate), path);
            }
        }

        [Test]
        public void NonRepeatingSelector_IsExhaustiveAndNeverRepeatsValidPrevious()
        {
            uint emptyState = 7u;
            Assert.That(ShowcaseFinalGameplayAudioController.
                SelectNonRepeatingIndex(0, -1, ref emptyState), Is.EqualTo(-1));

            for (int count = 1; count <= 8; count++)
            {
                for (int previous = -1; previous < count; previous++)
                {
                    var reached = new HashSet<int>();
                    for (uint seed = 0; seed < 4096; seed++)
                    {
                        uint state = seed;
                        int selected = ShowcaseFinalGameplayAudioController.
                            SelectNonRepeatingIndex(count, previous, ref state);
                        Assert.That(selected, Is.InRange(0, count - 1),
                            $"count={count}, previous={previous}, seed={seed}");
                        if (count > 1 && previous >= 0)
                        {
                            Assert.That(selected, Is.Not.EqualTo(previous),
                                $"count={count}, previous={previous}, seed={seed}");
                        }
                        reached.Add(selected);
                    }

                    int expectedReachable = previous >= 0 && count > 1
                        ? count - 1
                        : count;
                    Assert.That(reached.Count, Is.EqualTo(expectedReachable),
                        $"selector coverage count={count}, previous={previous}");
                }
            }
        }

        [Test]
        public void LocomotionAndChairWhooshEvents_UseOwnedNormalizedTiming()
        {
            const string playerRoot =
                "Assets/_Project/Art/Animation/CHR00/DesignerA/Approved";
            AssertImporterEvent(
                playerRoot + "/CHR00_Walk_F.fbx",
                "ShowcaseLeftFootstep",
                ShowcaseChr00AnimationTiming.WalkLeftFootstep);
            AssertImporterEvent(
                playerRoot + "/CHR00_Walk_F.fbx",
                "ShowcaseRightFootstep",
                ShowcaseChr00AnimationTiming.WalkRightFootstep);
            AssertImporterEvent(
                playerRoot + "/CHR00_Run_F.fbx",
                "ShowcaseLeftFootstep",
                ShowcaseChr00AnimationTiming.RunLeftFootstep);
            AssertImporterEvent(
                playerRoot + "/CHR00_Run_F.fbx",
                "ShowcaseRightFootstep",
                ShowcaseChr00AnimationTiming.RunRightFootstep);
            AssertImporterEvent(
                playerRoot + "/CHR00_Throw_Medium.fbx",
                "ShowcaseChairSwingWhoosh",
                ShowcaseChr00AnimationTiming.ThrowMediumSwingWhoosh);

            foreach (string path in new[]
            {
                "Assets/_Project/Art/Animation/ENY01/Normal/" +
                    "ENY01_Normal_Standard_Run.fbx",
                "Assets/_Project/Art/Animation/ENY01/Elite/" +
                    "ENY01_Elite_Standard_Run.fbx"
            })
            {
                AssertImporterEvent(
                    path,
                    "ShowcaseLeftFootstep",
                    ShowcaseNonPlayerAnimationTiming.EnemyRunLeftFootstep);
                AssertImporterEvent(
                    path,
                    "ShowcaseRightFootstep",
                    ShowcaseNonPlayerAnimationTiming.EnemyRunRightFootstep);
            }
        }

        [Test]
        public void GameplaySources_ExposeAndCommitAllFinalAudioEventHooks()
        {
            AssertEventContract(
                typeof(PrototypeInteractionTarget),
                "CollisionEntered",
                typeof(Action<PrototypeInteractionCollisionSnapshot>),
                "PrototypeInteractionTarget.cs",
                "CollisionEntered?.Invoke(new PrototypeInteractionCollisionSnapshot(");
            AssertEventContract(
                typeof(PrototypePhysicalObjectLifecycle),
                "CarriedTargetDropped",
                typeof(Action<PrototypeInteractionTarget>),
                "PrototypePhysicalObjectLifecycle.cs",
                "CarriedTargetDropped?.Invoke(target);");
            AssertEventContract(
                typeof(PrototypePhysicalObjectLifecycle),
                "PlayerThrowCommitted",
                typeof(Action<PhysicalReleaseSnapshot>),
                "PrototypePhysicalObjectLifecycle.cs",
                "PlayerThrowCommitted?.Invoke(LastPhysicalReleaseSnapshot);");
            AssertEventContract(
                typeof(PrototypeAllyRelayController),
                "SupplyReleased",
                typeof(Action<PrototypeInteractionTarget>),
                "PrototypeAllyRelayController.cs",
                "SupplyReleased?.Invoke(activeTarget);");
            AssertEventContract(
                typeof(ShowcasePlayerCombatController),
                "AcquisitionCommitted",
                typeof(Action<PrototypeInteractionTarget,
                    PrototypeInteractionOutcome>),
                "ShowcasePlayerCombatController.cs",
                "AcquisitionCommitted?.Invoke(");
            AssertEventContract(
                typeof(ShowcasePlayerCombatController),
                "ChairSwingWhooshCommitted",
                typeof(Action<PrototypeInteractionTarget>),
                "ShowcasePlayerCombatController.cs",
                "ChairSwingWhooshCommitted?.Invoke(pendingTarget);");
            AssertEventContract(
                typeof(ShowcasePlayerCombatController),
                "QueuedRelayCatchCommitted",
                typeof(Action<PrototypeInteractionTarget>),
                "ShowcasePlayerCombatController.cs",
                "QueuedRelayCatchCommitted?.Invoke(target);");
            AssertEventContract(
                typeof(ShowcasePlayerCombatController),
                "QueuedRelayCatchCancelled",
                typeof(Action<PrototypeInteractionTarget>),
                "ShowcasePlayerCombatController.cs",
                "QueuedRelayCatchCancelled?.Invoke(pendingTarget);");
            AssertEventContract(
                typeof(ShowcaseAnimationEventReceiver),
                "FootstepContact",
                typeof(Action<ShowcaseAnimationEventReceiver, int>),
                "ShowcaseAnimationEventReceiver.cs",
                "FootstepContact?.Invoke(this, foot);");
            AssertEventContract(
                typeof(ShowcaseCargoStrapController),
                "StrapExtendStarted",
                typeof(Action<PrototypeInteractionTarget>),
                "ShowcaseCargoStrapController.cs",
                "StrapExtendStarted?.Invoke(target);");
            AssertEventContract(
                typeof(ShowcaseCargoStrapController),
                "StrapLockCommitted",
                typeof(Action<PrototypeInteractionTarget>),
                "ShowcaseCargoStrapController.cs",
                "StrapLockCommitted?.Invoke(activeTarget);");
            AssertEventContract(
                typeof(ShowcaseCargoStrapController),
                "StrapTensionCommitted",
                typeof(Action<PrototypeInteractionTarget>),
                "ShowcaseCargoStrapController.cs",
                "StrapTensionCommitted?.Invoke(completedTarget);");
        }

        [Test]
        public void S7Scene_PassesFinalAudioAuthoringValidation()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Assert.DoesNotThrow(() => InvokeAuthoring(
                "ValidateCurrentIntegration",
                new[] { typeof(Scene) },
                scene));
        }

        [Test]
        public void ApplyToSceneTwice_PreservesFinalAudioSourceAndComponentCounts()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            try
            {
                InvokeAuthoring("ApplyToScene", new[] { typeof(Scene) }, scene);
                int[] first = CaptureWiringCounts(scene);

                InvokeAuthoring("ApplyToScene", new[] { typeof(Scene) }, scene);
                int[] second = CaptureWiringCounts(scene);

                CollectionAssert.AreEqual(first, second);
                Assert.That(second[0], Is.EqualTo(1), "Final root count");
                Assert.That(second[1], Is.EqualTo(1), "Final controller count");
                Assert.That(second[2], Is.EqualTo(ExpectedSceneAudioSourceCount),
                    "Scene AudioSource count");
                Assert.That(second[3], Is.EqualTo(1), "AudioListener count");
                Assert.That(second[4], Is.EqualTo(14), "Final root source count");
                Assert.That(second[5], Is.EqualTo(1), "Unarmed controller count");
                Assert.That(second[6], Is.EqualTo(4), "Animation receiver count");
                Assert.That(second[7], Is.EqualTo(10), "Interaction target count");
                Assert.DoesNotThrow(() => InvokeAuthoring(
                    "ValidateCurrentIntegration",
                    new[] { typeof(Scene) },
                    scene));
            }
            finally
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }

        private static void AssertClip(AudioClip clip, string expectedPath)
        {
            Assert.That(clip, Is.Not.Null, expectedPath);
            Assert.That(AssetDatabase.GetAssetPath(clip), Is.EqualTo(expectedPath));
        }

        private static void AssertPool(
            IReadOnlyList<AudioClip> actual,
            params string[] expectedPaths)
        {
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.Count, Is.EqualTo(expectedPaths.Length));
            for (int index = 0; index < expectedPaths.Length; index++)
            {
                AssertClip(actual[index], expectedPaths[index]);
            }
        }

        private static void AssertImporterEvent(
            string assetPath,
            string functionName,
            float expectedNormalizedTime)
        {
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            Assert.That(importer, Is.Not.Null, assetPath);
            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0)
            {
                clips = importer.defaultClipAnimations;
            }
            AnimationEvent[] matches = clips
                .SelectMany(clip => clip.events ?? Array.Empty<AnimationEvent>())
                .Where(animationEvent =>
                    animationEvent.functionName == functionName)
                .ToArray();
            Assert.That(matches, Has.Length.EqualTo(1),
                $"{assetPath} / {functionName}");
            Assert.That(matches[0].time,
                Is.EqualTo(expectedNormalizedTime).Within(0.0001f),
                $"{assetPath} / {functionName}");
        }

        private static void AssertEventContract(
            Type owner,
            string eventName,
            Type expectedHandler,
            string sourceFile,
            string invocation)
        {
            EventInfo eventInfo = owner.GetEvent(eventName);
            Assert.That(eventInfo, Is.Not.Null, $"{owner.Name}.{eventName}");
            Assert.That(eventInfo.EventHandlerType, Is.EqualTo(expectedHandler));

            string sourcePath = Path.Combine(
                Application.dataPath,
                "_Project",
                "Code",
                "Runtime",
                "Player",
                sourceFile);
            Assert.That(File.Exists(sourcePath), Is.True, sourcePath);
            StringAssert.Contains(invocation, File.ReadAllText(sourcePath));
        }

        private static object InvokeAuthoring(
            string methodName,
            Type[] parameterTypes,
            params object[] arguments)
        {
            Type authoring = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(AuthoringTypeName, false))
                .FirstOrDefault(type => type != null);
            Assert.That(authoring, Is.Not.Null, AuthoringTypeName);
            MethodInfo method = authoring.GetMethod(
                methodName,
                BindingFlags.Public | BindingFlags.Static,
                null,
                parameterTypes,
                null);
            Assert.That(method, Is.Not.Null,
                $"{AuthoringTypeName}.{methodName}");
            try
            {
                return method.Invoke(null, arguments);
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException ?? exception;
            }
        }

        private static int[] CaptureWiringCounts(Scene scene)
        {
            ShowcaseFinalGameplayAudioController controller = SceneComponents<
                ShowcaseFinalGameplayAudioController>(scene).Single();
            return new[]
            {
                scene.GetRootGameObjects().Count(root => root.name == FinalRootName),
                SceneComponents<ShowcaseFinalGameplayAudioController>(scene).Length,
                SceneComponents<AudioSource>(scene).Length,
                SceneComponents<AudioListener>(scene).Length,
                controller.GetComponentsInChildren<AudioSource>(true).Length,
                SceneComponents<ShowcaseUnarmedCombatAudioController>(scene).Length,
                SceneComponents<ShowcaseAnimationEventReceiver>(scene).Length,
                SceneComponents<PrototypeInteractionTarget>(scene).Length
            };
        }

        private static T[] SceneComponents<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }

        private static string Foley(string name) =>
            $"{AudioRoot}/Foley/{name}.wav";

        private static string Impact(string name) =>
            $"{AudioRoot}/Impacts/{name}.wav";

        private static string Motion(string name) =>
            $"{AudioRoot}/Motion/{name}.wav";

        private static string Strap(string name) =>
            $"{AudioRoot}/Strap/{name}.wav";

        private static string Voice(string name) =>
            $"{AudioRoot}/Voices/{name}.wav";
    }
}
