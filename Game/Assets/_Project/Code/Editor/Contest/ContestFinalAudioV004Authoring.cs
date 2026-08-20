using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.Contest.Editor
{
    public static class ContestFinalAudioV004Authoring
    {
        public const string FinalAudioRootName = "SHOWCASE_FINAL_GAMEPLAY_AUDIO";
        public const string AudioRoot = "Assets/_Project/Audio/Final_v004";
        public const string ProfilePath =
            AudioRoot + "/Config/FinalGameplayAudioProfile.asset";
        public const int TransientSourceCount = 12;
        public const int StrapSourceCount = 2;
        public const int ExpectedRunnerCount = 2;
        public const int ExpectedTargetCount = 10;
        public const int ExpectedChairCount = 3;
        public const int ExpectedLightTargetCount = 5;
        public const int ExpectedSceneAudioSourceCount = 36;
        public const string DevelopmentBuildPath =
            "Builds/WebGL/FinalAudioV004Development";
        public const string WindowsDevelopmentBuildPath =
            "Builds/Windows/FinalAudioV004Development/BreakNoticeFinalAudioV004.exe";

        private const string BinderPickupPath =
            AudioRoot + "/Foley/BN_FOL_BINDER_PICKUP_03.wav";
        private const string BinderThrowPath =
            AudioRoot + "/Foley/BN_FOL_BINDER_RELEASE_02.wav";
        private const string BinderPlaceDownPath =
            AudioRoot + "/Foley/BN_FOL_BINDER_THROW_01.wav";
        private const string ChairPickupPath =
            AudioRoot + "/Foley/BN_FOL_CHAIR_PICKUP_03.wav";
        private const string ChairReleasePath =
            AudioRoot + "/Foley/BN_FOL_CHAIR_RELEASE_02.wav";
        private const string ChairThrowPath =
            AudioRoot + "/Foley/BN_FOL_CHAIR_THROW_02.wav";
        private const string ChairSwingWhooshPath =
            AudioRoot + "/Foley/BN_FOL_CHAIR_SWING_WHOOSH_MEDIUM_B.wav";

        private const string BinderCatchPath =
            AudioRoot + "/Impacts/BN_SFX_BINDER_CATCH_A.wav";
        private const string BinderDirectHitPath =
            AudioRoot + "/Impacts/BN_SFX_BINDER_DIRECT_HIT_A.wav";
        private const string BinderFloorDropPath =
            AudioRoot + "/Impacts/BN_SFX_BINDER_FLOOR_DROP_B.wav";
        private const string BinderWallHitPath =
            AudioRoot + "/Impacts/BN_SFX_BINDER_WALL_HIT_B.wav";
        private const string ChairEnemyImpactPath =
            AudioRoot + "/Impacts/BN_SFX_CHAIR_ENEMY_MAIN_IMPACT_B.wav";
        private const string ChairStopAPath =
            AudioRoot + "/Impacts/BN_SFX_CHAIR_STOP_A.wav";
        private const string ChairStopBPath =
            AudioRoot + "/Impacts/BN_SFX_CHAIR_STOP_B.wav";
        private const string ChairWorldImpactPath =
            AudioRoot + "/Impacts/BN_SFX_CHAIR_WORLD_IMPACT_A.wav";

        private const string ChairRollLoopPath =
            AudioRoot + "/Motion/BN_WP01_CHAIR_ROLL_LOOP_02.wav";
        private const string ChairSkidPath =
            AudioRoot + "/Motion/BN_WP01_CHAIR_SKID_01.wav";
        private const string SmallSpinLoopPath =
            AudioRoot + "/Motion/BN_FOL_SMALL_OBJECT_SPIN_LOOP_A.wav";

        private const string StrapExtendPart1Path =
            AudioRoot + "/Strap/BN_FOL_STRAP_ACTUAL_ZIPPER_Z01_OPEN_FULL.wav";
        private const string StrapExtendPart2Path =
            AudioRoot + "/Strap/BN_FOL_STRAP_ACTUAL_ZIPPER_Z02_CLOSE_FULL.wav";
        private const string StrapLockPath =
            AudioRoot + "/Strap/BN_FOL_STRAP_LOCK_02.wav";
        private const string StrapTensionPath =
            AudioRoot + "/Strap/BN_FOL_STRAP_TENSION_02.wav";

        private const string PlayerHeavyHurtPath =
            AudioRoot + "/Voices/BN_VOX_PLAYER_HEAVY_HURT_B.wav";

        private const string PlayerFootstepSourceName =
            "FINAL_AUDIO_PLAYER_FOOTSTEP";
        private const string PlayerVoiceSourceName =
            "FINAL_AUDIO_PLAYER_VOICE";
        private const string EnemyFootstepSourceName =
            "FINAL_AUDIO_ENEMY_FOOTSTEP";
        private const string EnemyVoiceSourceName =
            "FINAL_AUDIO_ENEMY_VOICE";
        private const string ChairRollSourceName =
            "FINAL_AUDIO_CHAIR_ROLL";
        private const string SmallSpinSourceName =
            "FINAL_AUDIO_SMALL_SPIN";

        private static readonly string[] FootstepPaths = Enumerable.Range(1, 8)
            .Select(index =>
                $"{AudioRoot}/Foley/BN_FOL_FOOTSTEP_OFFICE_HARD_{index:00}.wav")
            .ToArray();

        private static readonly string[] PlayerLightHurtPaths =
        {
            AudioRoot + "/Voices/BN_VOX_PLAYER_LIGHT_HURT_A.wav",
            AudioRoot + "/Voices/BN_VOX_PLAYER_LIGHT_HURT_B.wav"
        };

        private static readonly string[] PlayerKoPaths =
        {
            AudioRoot + "/Voices/BN_VOX_PLAYER_KO_DEATH_A.wav",
            AudioRoot + "/Voices/BN_VOX_PLAYER_KO_DEATH_B.wav"
        };

        private static readonly string[] EnemyLightHurtPaths =
        {
            AudioRoot + "/Voices/BN_VOX_ENEMY_LIGHT_HURT_A.wav",
            AudioRoot + "/Voices/BN_VOX_ENEMY_LIGHT_HURT_B.wav",
            AudioRoot + "/Voices/BN_VOX_ENEMY_LIGHT_HURT_D.wav"
        };

        private static readonly string[] EnemyKoPaths =
        {
            AudioRoot + "/Voices/BN_VOX_ENEMY_KO_DEATH_A.wav",
            AudioRoot + "/Voices/BN_VOX_ENEMY_KO_DEATH_B.wav"
        };

        private static readonly string[] RuntimeAudioPaths =
        {
            BinderPickupPath,
            BinderThrowPath,
            BinderPlaceDownPath,
            ChairPickupPath,
            ChairReleasePath,
            ChairThrowPath,
            ChairSwingWhooshPath,
            BinderCatchPath,
            BinderDirectHitPath,
            BinderFloorDropPath,
            BinderWallHitPath,
            ChairEnemyImpactPath,
            ChairStopAPath,
            ChairStopBPath,
            ChairWorldImpactPath,
            ChairRollLoopPath,
            ChairSkidPath,
            SmallSpinLoopPath,
            StrapExtendPart1Path,
            StrapExtendPart2Path,
            StrapLockPath,
            StrapTensionPath,
            PlayerHeavyHurtPath,
            AudioRoot + "/Voices/BN_VOX_PLAYER_LIGHT_HURT_A.wav",
            AudioRoot + "/Voices/BN_VOX_PLAYER_LIGHT_HURT_B.wav",
            AudioRoot + "/Voices/BN_VOX_PLAYER_KO_DEATH_A.wav",
            AudioRoot + "/Voices/BN_VOX_PLAYER_KO_DEATH_B.wav",
            AudioRoot + "/Voices/BN_VOX_ENEMY_LIGHT_HURT_A.wav",
            AudioRoot + "/Voices/BN_VOX_ENEMY_LIGHT_HURT_B.wav",
            AudioRoot + "/Voices/BN_VOX_ENEMY_LIGHT_HURT_D.wav",
            AudioRoot + "/Voices/BN_VOX_ENEMY_KO_DEATH_A.wav",
            AudioRoot + "/Voices/BN_VOX_ENEMY_KO_DEATH_B.wav",
            AudioRoot + "/Foley/BN_FOL_FOOTSTEP_OFFICE_HARD_01.wav",
            AudioRoot + "/Foley/BN_FOL_FOOTSTEP_OFFICE_HARD_02.wav",
            AudioRoot + "/Foley/BN_FOL_FOOTSTEP_OFFICE_HARD_03.wav",
            AudioRoot + "/Foley/BN_FOL_FOOTSTEP_OFFICE_HARD_04.wav",
            AudioRoot + "/Foley/BN_FOL_FOOTSTEP_OFFICE_HARD_05.wav",
            AudioRoot + "/Foley/BN_FOL_FOOTSTEP_OFFICE_HARD_06.wav",
            AudioRoot + "/Foley/BN_FOL_FOOTSTEP_OFFICE_HARD_07.wav",
            AudioRoot + "/Foley/BN_FOL_FOOTSTEP_OFFICE_HARD_08.wav"
        };

        [MenuItem("BREAK NOTICE/Contest/Prepare Final Audio v004 To S7")]
        public static void Prepare()
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(
                ContestShowcaseSceneAuthoring.ShowcaseS7ScenePath);
            if (sceneAsset == null)
            {
                throw new InvalidOperationException(
                    $"Missing playable S7 Scene: " +
                    ContestShowcaseSceneAuthoring.ShowcaseS7ScenePath);
            }

            ConfigureRuntimeAudioImporters();
            CreateOrUpdateProfile();
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
                    "Could not save the S7 Final Audio v004 wiring.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ValidateCurrentIntegration(scene);
            Debug.Log(
                "Final Audio v004 prepared: 40 approved runtime WAVs, typed " +
                "profile, 12 transient voices, scheduled strap pair, actor " +
                "voice/footsteps and object-bound roll/spin loops.");
        }

        [MenuItem("BREAK NOTICE/Contest/Build Final Audio v004 S7 Web Development")]
        public static void BuildWebDevelopment()
        {
            Prepare();
            if (!BuildPipeline.IsBuildTargetSupported(
                    BuildTargetGroup.WebGL,
                    BuildTarget.WebGL))
            {
                throw new InvalidOperationException(
                    "WebGL Build Support is unavailable for this Unity installation.");
            }

            string projectRoot = Path.GetFullPath(Path.Combine(
                Application.dataPath,
                ".."));
            string buildRoot = Path.GetFullPath(Path.Combine(
                projectRoot,
                DevelopmentBuildPath));
            string safePrefix = projectRoot.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!buildRoot.StartsWith(
                    safePrefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Unsafe Final Audio build output path: {buildRoot}");
            }

            if (Directory.Exists(buildRoot))
            {
                Directory.Delete(buildRoot, true);
            }
            Directory.CreateDirectory(buildRoot);

            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.runInBackground = true;
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ContestShowcaseSceneAuthoring.ShowcaseS7ScenePath },
                locationPathName = buildRoot,
                target = BuildTarget.WebGL,
                options = BuildOptions.Development
            });
            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Final Audio v004 S7 build failed: {summary.result}; " +
                    $"warnings={summary.totalWarnings}; errors={summary.totalErrors}.");
            }

            File.WriteAllText(
                Path.Combine(buildRoot, "FinalAudioV004BuildMetadata.json"),
                "{\n" +
                $"  \"unityVersion\": \"{Application.unityVersion}\",\n" +
                $"  \"scene\": \"{ContestShowcaseSceneAuthoring.ShowcaseS7ScenePath}\",\n" +
                $"  \"buildGuid\": \"{summary.guid}\",\n" +
                $"  \"result\": \"{summary.result}\",\n" +
                $"  \"totalBytes\": {summary.totalSize},\n" +
                $"  \"totalWarnings\": {summary.totalWarnings},\n" +
                $"  \"totalErrors\": {summary.totalErrors}\n" +
                "}\n");
            Debug.Log(
                $"Final Audio v004 S7 Web Development build succeeded: " +
                $"{buildRoot}; bytes={summary.totalSize}; guid={summary.guid}.");
        }

        [MenuItem("BREAK NOTICE/Contest/Build Final Audio v004 S7 Windows Development")]
        public static void BuildWindowsDevelopment()
        {
            Prepare();
            if (!BuildPipeline.IsBuildTargetSupported(
                    BuildTargetGroup.Standalone,
                    BuildTarget.StandaloneWindows64))
            {
                throw new InvalidOperationException(
                    "Windows Build Support is unavailable for this Unity installation.");
            }

            string projectRoot = Path.GetFullPath(Path.Combine(
                Application.dataPath,
                ".."));
            string executablePath = Path.GetFullPath(Path.Combine(
                projectRoot,
                WindowsDevelopmentBuildPath));
            string safePrefix = projectRoot.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!executablePath.StartsWith(
                    safePrefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Unsafe Final Audio Windows output path: {executablePath}");
            }
            string outputFolder = Path.GetDirectoryName(executablePath);
            if (string.IsNullOrEmpty(outputFolder))
            {
                throw new InvalidOperationException(
                    $"Invalid Final Audio Windows output: {executablePath}");
            }
            if (Directory.Exists(outputFolder))
            {
                Directory.Delete(outputFolder, true);
            }
            Directory.CreateDirectory(outputFolder);

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ContestShowcaseSceneAuthoring.ShowcaseS7ScenePath },
                locationPathName = executablePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Final Audio v004 Windows build failed: {summary.result}; " +
                    $"warnings={summary.totalWarnings}; errors={summary.totalErrors}.");
            }
            File.WriteAllText(
                Path.Combine(outputFolder, "FinalAudioV004BuildMetadata.json"),
                "{\n" +
                $"  \"unityVersion\": \"{Application.unityVersion}\",\n" +
                $"  \"scene\": \"{ContestShowcaseSceneAuthoring.ShowcaseS7ScenePath}\",\n" +
                $"  \"buildGuid\": \"{summary.guid}\",\n" +
                $"  \"result\": \"{summary.result}\",\n" +
                $"  \"totalBytes\": {summary.totalSize},\n" +
                $"  \"totalWarnings\": {summary.totalWarnings},\n" +
                $"  \"totalErrors\": {summary.totalErrors}\n" +
                "}\n");
            Debug.Log(
                $"Final Audio v004 S7 Windows Development build succeeded: " +
                $"{executablePath}; bytes={summary.totalSize}; guid={summary.guid}.");
        }

        public static void ApplyToScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded ||
                scene.path != ContestShowcaseSceneAuthoring.ShowcaseS7ScenePath)
            {
                throw new ArgumentException(
                    "Final Audio v004 can only be applied to the loaded S7 Scene.",
                    nameof(scene));
            }

            ContestUnarmedCombatAudioAuthoring.ApplyToScene(scene);
            FinalGameplayAudioProfile profile = CreateOrUpdateProfile();

            ShowcasePlayerCombatController playerCombat =
                SingleSceneComponent<ShowcasePlayerCombatController>(scene);
            ShowcaseCombatHealth playerHealth = playerCombat.Health;
            ThirdPersonLocomotion locomotion =
                playerCombat.GetComponent<ThirdPersonLocomotion>();
            PrototypePhysicalObjectLifecycle physicalLifecycle =
                playerCombat.GetComponent<PrototypePhysicalObjectLifecycle>();
            PrototypeAllyRelayController allyRelay =
                SingleSceneComponent<PrototypeAllyRelayController>(scene);
            ShowcaseCargoStrapController cargoStrap =
                playerCombat.GetComponent<ShowcaseCargoStrapController>();
            ShowcaseRunnerPressureController pressure =
                SingleSceneComponent<ShowcaseRunnerPressureController>(scene);
            ShowcaseRunnerAgent[] runners = pressure.Runners
                .Where(runner => runner != null)
                .OrderBy(runner => runner.RunnerId, StringComparer.Ordinal)
                .ToArray();
            PrototypeInteractionTarget[] targets = SceneComponents<
                    PrototypeInteractionTarget>(scene)
                .OrderBy(target => target.RuntimeInstanceKey, StringComparer.Ordinal)
                .ToArray();
            PrototypeInteractionTarget[] chairs = targets
                .Where(IsChair)
                .ToArray();
            PrototypeInteractionTarget[] lights = targets
                .Where(target => target.WeightClass == PrototypeWeightClass.Light)
                .ToArray();
            ShowcaseAnimationEventReceiver[] receivers = ResolveReceivers(
                playerCombat,
                runners);

            RequireSceneContract(
                playerHealth,
                locomotion,
                physicalLifecycle,
                cargoStrap,
                runners,
                targets,
                chairs,
                lights,
                receivers);

            DestroyFinalAudioRoots(scene);
            var root = new GameObject(FinalAudioRootName);
            SceneManager.MoveGameObjectToScene(root, scene);
            ShowcaseFinalGameplayAudioController controller =
                root.AddComponent<ShowcaseFinalGameplayAudioController>();

            AudioSource[] transientSources = Enumerable.Range(
                    1,
                    TransientSourceCount)
                .Select(index => CreateSource(
                    root.transform,
                    $"FINAL_TRANSIENT_SOURCE_{index:00}",
                    false,
                    null,
                    80,
                    2f,
                    26f))
                .ToArray();
            AudioSource strapPart1 = CreateSource(
                root.transform,
                "FINAL_STRAP_EXTEND_PART_01",
                false,
                profile.StrapExtendPart1Clip,
                72,
                2f,
                24f);
            AudioSource strapPart2 = CreateSource(
                root.transform,
                "FINAL_STRAP_EXTEND_PART_02",
                false,
                profile.StrapExtendPart2Clip,
                72,
                2f,
                24f);

            AudioSource playerFootstep = RecreateOwnedSource(
                playerCombat.transform,
                PlayerFootstepSourceName,
                false,
                null,
                96,
                1.5f,
                18f);
            AudioSource playerVoice = RecreateOwnedSource(
                playerCombat.transform,
                PlayerVoiceSourceName,
                false,
                null,
                40,
                2f,
                22f);

            var enemyFootsteps = new AudioSource[runners.Length];
            var enemyVoices = new AudioSource[runners.Length];
            for (int index = 0; index < runners.Length; index++)
            {
                enemyFootsteps[index] = RecreateOwnedSource(
                    runners[index].transform,
                    EnemyFootstepSourceName,
                    false,
                    null,
                    104,
                    1.5f,
                    20f);
                enemyVoices[index] = RecreateOwnedSource(
                    runners[index].transform,
                    EnemyVoiceSourceName,
                    false,
                    null,
                    48,
                    2f,
                    24f);
            }

            AudioSource[] chairRollSources = chairs
                .Select(chair => RecreateOwnedSource(
                    chair.transform,
                    ChairRollSourceName,
                    true,
                    profile.ChairRollLoopClip,
                    112,
                    2f,
                    24f))
                .ToArray();
            AudioSource[] smallSpinSources = lights
                .Select(light => RecreateOwnedSource(
                    light.transform,
                    SmallSpinSourceName,
                    true,
                    profile.SmallObjectSpinLoopClip,
                    112,
                    2f,
                    24f))
                .ToArray();

            controller.Configure(
                profile,
                playerCombat,
                playerHealth,
                locomotion,
                physicalLifecycle,
                allyRelay,
                cargoStrap,
                pressure,
                runners,
                targets,
                receivers,
                transientSources,
                strapPart1,
                strapPart2,
                playerFootstep,
                enemyFootsteps,
                playerVoice,
                enemyVoices,
                chairs,
                chairRollSources,
                lights,
                smallSpinSources);

            EditorUtility.SetDirty(root);
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("BREAK NOTICE/Contest/Validate Final Audio v004 Integration")]
        public static void ValidateCurrentIntegration()
        {
            Scene scene = SceneManager.GetSceneByPath(
                ContestShowcaseSceneAuthoring.ShowcaseS7ScenePath);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(
                    ContestShowcaseSceneAuthoring.ShowcaseS7ScenePath,
                    OpenSceneMode.Single);
            }
            ValidateCurrentIntegration(scene);
        }

        public static void ValidateCurrentIntegration(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded ||
                scene.path != ContestShowcaseSceneAuthoring.ShowcaseS7ScenePath)
            {
                throw new InvalidOperationException(
                    "The loaded validation target is not the S7 gameplay Scene.");
            }

            foreach (string path in RuntimeAudioPaths)
            {
                RequireClip(path);
                ValidateImporter(path);
            }
            if (RuntimeAudioPaths.Length != 40 ||
                RuntimeAudioPaths.Distinct(StringComparer.Ordinal).Count() != 40)
            {
                throw new InvalidOperationException(
                    "Final Audio v004 must contain exactly 40 distinct runtime WAV paths.");
            }
            if (AssetDatabase.FindAssets(
                    "BN_ENEMY_LIGHT_VOICE_REEL t:AudioClip",
                    new[] { AudioRoot }).Length != 0)
            {
                throw new InvalidOperationException(
                    "The audition-only enemy voice reel entered Runtime Audio.");
            }

            FinalGameplayAudioProfile profile = AssetDatabase.LoadAssetAtPath<
                FinalGameplayAudioProfile>(ProfilePath);
            if (profile == null)
            {
                throw new InvalidOperationException(
                    $"Missing Final Audio v004 profile: {ProfilePath}");
            }
            AudioClip[] requiredClips = profile.EnumerateRequiredClips().ToArray();
            if (requiredClips.Length != 40 || requiredClips.Any(clip => clip == null) ||
                requiredClips.Distinct().Count() != 40)
            {
                throw new InvalidOperationException(
                    "The Final Audio profile must reference 40 distinct non-null clips.");
            }

            ShowcaseFinalGameplayAudioController controller =
                SceneComponents<ShowcaseFinalGameplayAudioController>(scene).Single();
            if (controller.gameObject.name != FinalAudioRootName ||
                controller.transform.parent != null || controller.Profile != profile)
            {
                throw new InvalidOperationException(
                    "The Final Audio controller root or profile reference is invalid.");
            }
            if (controller.TransientSources.Length != TransientSourceCount ||
                controller.Runners.Length != ExpectedRunnerCount ||
                controller.Targets.Length != ExpectedTargetCount ||
                controller.Receivers.Length != ExpectedRunnerCount + 1 ||
                controller.EnemyFootstepSources.Length != ExpectedRunnerCount ||
                controller.EnemyVoiceSources.Length != ExpectedRunnerCount ||
                controller.ChairTargets.Length != ExpectedChairCount ||
                controller.ChairRollSources.Length != ExpectedChairCount ||
                controller.LightTargets.Length != ExpectedLightTargetCount ||
                controller.SmallSpinSources.Length != ExpectedLightTargetCount)
            {
                throw new InvalidOperationException(
                    "Final Audio serialized reference counts do not match S7 authority.");
            }

            ValidateTransientSources(controller.TransientSources);
            ValidateSource(controller.StrapPart1Source, false, profile.StrapExtendPart1Clip);
            ValidateSource(controller.StrapPart2Source, false, profile.StrapExtendPart2Clip);
            ValidateSource(controller.PlayerFootstepSource, false, null);
            ValidateSource(controller.PlayerVoiceSource, false, null);
            foreach (AudioSource source in controller.EnemyFootstepSources)
            {
                ValidateSource(source, false, null);
            }
            foreach (AudioSource source in controller.EnemyVoiceSources)
            {
                ValidateSource(source, false, null);
            }
            foreach (AudioSource source in controller.ChairRollSources)
            {
                ValidateSource(source, true, profile.ChairRollLoopClip);
            }
            foreach (AudioSource source in controller.SmallSpinSources)
            {
                ValidateSource(source, true, profile.SmallObjectSpinLoopClip);
            }

            Transform finalRoot = controller.transform;
            if (finalRoot.childCount != TransientSourceCount + StrapSourceCount ||
                finalRoot.GetComponentsInChildren<AudioSource>(true).Length !=
                    TransientSourceCount + StrapSourceCount ||
                controller.TransientSources.Any(source =>
                    source.transform.parent != finalRoot) ||
                controller.StrapPart1Source.transform.parent != finalRoot ||
                controller.StrapPart2Source.transform.parent != finalRoot)
            {
                throw new InvalidOperationException(
                    "The Final Audio root does not own exactly 14 direct sources.");
            }
            ValidateDirectChild(
                controller.PlayerFootstepSource,
                controller.PlayerCombat.transform);
            ValidateDirectChild(
                controller.PlayerVoiceSource,
                controller.PlayerCombat.transform);
            for (int index = 0; index < controller.Runners.Length; index++)
            {
                ValidateDirectChild(
                    controller.EnemyFootstepSources[index],
                    controller.Runners[index].transform);
                ValidateDirectChild(
                    controller.EnemyVoiceSources[index],
                    controller.Runners[index].transform);
            }
            for (int index = 0; index < controller.ChairTargets.Length; index++)
            {
                ValidateDirectChild(
                    controller.ChairRollSources[index],
                    controller.ChairTargets[index].transform);
            }
            for (int index = 0; index < controller.LightTargets.Length; index++)
            {
                ValidateDirectChild(
                    controller.SmallSpinSources[index],
                    controller.LightTargets[index].transform);
            }

            AudioListener[] listeners = SceneComponents<AudioListener>(scene);
            if (listeners.Length != 1 || !listeners[0].enabled ||
                listeners[0].GetComponent<Camera>() == null ||
                listeners[0].gameObject.name != "OutputCamera")
            {
                throw new InvalidOperationException(
                    "S7 must retain exactly one enabled OutputCamera AudioListener.");
            }
            if (SceneComponents<AudioSource>(scene).Length !=
                ExpectedSceneAudioSourceCount)
            {
                throw new InvalidOperationException(
                    $"S7 must contain exactly {ExpectedSceneAudioSourceCount} " +
                    "authored AudioSources after Final Audio integration.");
            }

            Debug.Log(
                "Final Audio v004 validation passed: 40 clips, one listener, " +
                "12 transient sources, two scheduled strap sources, actor-local " +
                "voice/footstep sources, three chair loops and five small loops.");
        }

        private static FinalGameplayAudioProfile CreateOrUpdateProfile()
        {
            EnsureAssetFolder(AudioRoot + "/Config");
            FinalGameplayAudioProfile profile = AssetDatabase.LoadAssetAtPath<
                FinalGameplayAudioProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<FinalGameplayAudioProfile>();
                profile.name = "FinalGameplayAudioProfile";
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            var serialized = new SerializedObject(profile);
            SetClip(serialized, "binderPickupClip", BinderPickupPath);
            SetClip(serialized, "binderThrowClip", BinderThrowPath);
            SetClip(serialized, "binderPlaceDownClip", BinderPlaceDownPath);
            SetClip(serialized, "binderDirectHitClip", BinderDirectHitPath);
            SetClip(serialized, "binderWallHitClip", BinderWallHitPath);
            SetClip(serialized, "binderFloorDropClip", BinderFloorDropPath);
            SetClip(serialized, "binderCatchClip", BinderCatchPath);

            SetClip(serialized, "chairPickupClip", ChairPickupPath);
            SetClip(serialized, "chairReleaseClip", ChairReleasePath);
            SetClip(serialized, "chairThrowClip", ChairThrowPath);
            SetClip(serialized, "chairSwingWhooshClip", ChairSwingWhooshPath);
            SetClip(serialized, "chairRollLoopClip", ChairRollLoopPath);
            SetClip(serialized, "chairSkidClip", ChairSkidPath);
            SetClip(serialized, "chairEnemyImpactClip", ChairEnemyImpactPath);
            SetClip(serialized, "chairWorldImpactClip", ChairWorldImpactPath);
            SetClipArray(
                serialized,
                "chairStopPool",
                new[] { ChairStopAPath, ChairStopBPath });

            SetClip(serialized, "strapExtendPart1Clip", StrapExtendPart1Path);
            SetClip(serialized, "strapExtendPart2Clip", StrapExtendPart2Path);
            SetClip(serialized, "strapLockClip", StrapLockPath);
            SetClip(serialized, "strapTensionClip", StrapTensionPath);
            SetClipArray(serialized, "footstepPool", FootstepPaths);
            SetClip(serialized, "smallObjectSpinLoopClip", SmallSpinLoopPath);

            SetClipArray(serialized, "playerLightHurtPool", PlayerLightHurtPaths);
            SetClip(serialized, "playerHeavyHurtClip", PlayerHeavyHurtPath);
            SetClipArray(serialized, "playerKoPool", PlayerKoPaths);
            SetClipArray(serialized, "enemyLightHurtPool", EnemyLightHurtPaths);
            SetClipArray(serialized, "enemyKoPool", EnemyKoPaths);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void ConfigureRuntimeAudioImporters()
        {
            if (RuntimeAudioPaths.Length != 40 ||
                RuntimeAudioPaths.Distinct(StringComparer.Ordinal).Count() != 40)
            {
                throw new InvalidOperationException(
                    "Final Audio v004 path inventory is not exactly 40 unique WAVs.");
            }

            foreach (string path in RuntimeAudioPaths)
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null)
                {
                    throw new InvalidOperationException(
                        $"Missing Final Audio v004 importer: {path}");
                }

                importer.forceToMono = true;
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

        private static AudioSource CreateSource(
            Transform parent,
            string name,
            bool loop,
            AudioClip clip,
            int priority,
            float minDistance,
            float maxDistance)
        {
            var sourceObject = new GameObject(name);
            sourceObject.transform.SetParent(parent, false);
            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.clip = clip;
            source.volume = loop ? 0f : 1f;
            source.pitch = 1f;
            source.priority = Mathf.Clamp(priority, 0, 256);
            source.spatialBlend = 1f;
            source.dopplerLevel = 0f;
            source.spread = 0f;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.bypassEffects = true;
            source.bypassReverbZones = true;
            EditorUtility.SetDirty(sourceObject);
            EditorUtility.SetDirty(source);
            return source;
        }

        private static AudioSource RecreateOwnedSource(
            Transform parent,
            string name,
            bool loop,
            AudioClip clip,
            int priority,
            float minDistance,
            float maxDistance)
        {
            for (int index = parent.childCount - 1; index >= 0; index--)
            {
                Transform child = parent.GetChild(index);
                if (child != null &&
                    string.Equals(child.name, name, StringComparison.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }
            return CreateSource(
                parent,
                name,
                loop,
                clip,
                priority,
                minDistance,
                maxDistance);
        }

        private static void DestroyFinalAudioRoots(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects()
                         .Where(candidate => string.Equals(
                             candidate.name,
                             FinalAudioRootName,
                             StringComparison.Ordinal))
                         .ToArray())
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static ShowcaseAnimationEventReceiver[] ResolveReceivers(
            ShowcasePlayerCombatController player,
            ShowcaseRunnerAgent[] runners)
        {
            var receivers = new List<ShowcaseAnimationEventReceiver>(
                ExpectedRunnerCount + 1);
            receivers.Add(RequireActorReceiver(player.transform));
            receivers.AddRange(runners.Select(runner =>
                RequireActorReceiver(runner.transform)));
            return receivers.ToArray();
        }

        private static ShowcaseAnimationEventReceiver RequireActorReceiver(
            Transform actor)
        {
            ShowcaseCharacterAnimationDriver driver =
                actor.GetComponent<ShowcaseCharacterAnimationDriver>();
            ShowcaseAnimationEventReceiver receiver = driver != null &&
                driver.Animator != null
                    ? driver.Animator.GetComponent<ShowcaseAnimationEventReceiver>()
                    : null;
            if (receiver == null)
            {
                throw new InvalidOperationException(
                    $"Missing runtime AnimationEvent receiver for actor {actor.name}.");
            }
            return receiver;
        }

        private static bool IsChair(PrototypeInteractionTarget target)
        {
            return target != null &&
                (target.Role == PrototypeInteractionRole.DirectChair ||
                 target.Role == PrototypeInteractionRole.AllyChair);
        }

        private static void RequireSceneContract(
            ShowcaseCombatHealth playerHealth,
            ThirdPersonLocomotion locomotion,
            PrototypePhysicalObjectLifecycle physicalLifecycle,
            ShowcaseCargoStrapController cargoStrap,
            ShowcaseRunnerAgent[] runners,
            PrototypeInteractionTarget[] targets,
            PrototypeInteractionTarget[] chairs,
            PrototypeInteractionTarget[] lights,
            ShowcaseAnimationEventReceiver[] receivers)
        {
            if (playerHealth == null || locomotion == null ||
                physicalLifecycle == null || cargoStrap == null)
            {
                throw new InvalidOperationException(
                    "S7 player audio dependencies are incomplete.");
            }
            if (runners.Length != ExpectedRunnerCount ||
                targets.Length != ExpectedTargetCount ||
                chairs.Length != ExpectedChairCount ||
                lights.Length != ExpectedLightTargetCount ||
                receivers.Length != ExpectedRunnerCount + 1)
            {
                throw new InvalidOperationException(
                    $"Unexpected S7 audio authority counts: runners={runners.Length}, " +
                    $"targets={targets.Length}, chairs={chairs.Length}, " +
                    $"lights={lights.Length}, receivers={receivers.Length}.");
            }
        }

        private static T SingleSceneComponent<T>(Scene scene)
            where T : Component
        {
            return SceneComponents<T>(scene).Single();
        }

        private static T[] SceneComponents<T>(Scene scene)
            where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }

        private static void SetClip(
            SerializedObject serialized,
            string propertyName,
            string path)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"Missing Final Audio profile property: {propertyName}");
            }
            property.objectReferenceValue = RequireClip(path);
        }

        private static void SetClipArray(
            SerializedObject serialized,
            string propertyName,
            IReadOnlyList<string> paths)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null || !property.isArray)
            {
                throw new InvalidOperationException(
                    $"Missing Final Audio profile array: {propertyName}");
            }
            property.arraySize = paths.Count;
            for (int index = 0; index < paths.Count; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue =
                    RequireClip(paths[index]);
            }
        }

        private static AudioClip RequireClip(string path)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null)
            {
                throw new InvalidOperationException(
                    $"Missing approved Final Audio v004 clip: {path}");
            }
            return clip;
        }

        private static void ValidateImporter(string path)
        {
            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null || !importer.forceToMono || importer.ambisonic ||
                importer.loadInBackground)
            {
                throw new InvalidOperationException(
                    $"Invalid Final Audio v004 importer flags: {path}");
            }
            var serialized = new SerializedObject(importer);
            SerializedProperty normalize = serialized.FindProperty("m_Normalize");
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            if ((normalize != null && normalize.boolValue) ||
                !settings.preloadAudioData ||
                settings.loadType != AudioClipLoadType.DecompressOnLoad ||
                settings.compressionFormat != AudioCompressionFormat.PCM ||
                settings.sampleRateSetting !=
                    AudioSampleRateSetting.PreserveSampleRate)
            {
                throw new InvalidOperationException(
                    $"Invalid Final Audio v004 PCM import contract: {path}");
            }
        }

        private static void EnsureAssetFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }
            int slash = path.LastIndexOf('/');
            if (slash <= 0)
            {
                throw new InvalidOperationException(
                    $"Invalid asset folder path: {path}");
            }
            string parent = path.Substring(0, slash);
            string name = path.Substring(slash + 1);
            EnsureAssetFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void ValidateTransientSources(AudioSource[] sources)
        {
            if (sources.Length != TransientSourceCount ||
                sources.Distinct().Count() != TransientSourceCount)
            {
                throw new InvalidOperationException(
                    "The Final Audio transient pool is not exactly 12 unique sources.");
            }
            foreach (AudioSource source in sources)
            {
                ValidateSource(source, false, null);
            }
        }

        private static void ValidateSource(
            AudioSource source,
            bool expectedLoop,
            AudioClip expectedClip)
        {
            if (source == null || source.playOnAwake ||
                source.loop != expectedLoop || source.clip != expectedClip ||
                source.spatialBlend < 0.999f || source.dopplerLevel != 0f)
            {
                throw new InvalidOperationException(
                    $"Invalid Final Audio source: " +
                    (source != null ? source.name : "<null>"));
            }
        }

        private static void ValidateDirectChild(
            AudioSource source,
            Transform expectedParent)
        {
            if (source == null || expectedParent == null ||
                source.transform.parent != expectedParent)
            {
                throw new InvalidOperationException(
                    "A Final Audio actor/object source is not a direct child of " +
                    "its serialized authority.");
            }
        }
    }
}
