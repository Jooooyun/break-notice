using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Encounter;
using BreakNotice.Gameplay.Input;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.Contest.Editor
{
    public static class DesignerACharacterIntegrationAuthoring
    {
        private readonly struct RuntimeStateContract
        {
            public RuntimeStateContract(
                string name,
                string assetPath,
                float playbackSpeed = 1f,
                bool locomotionDriven = false,
                string clipName = null,
                float cycleOffset = 0f)
            {
                Name = name;
                AssetPath = assetPath;
                PlaybackSpeed = playbackSpeed;
                LocomotionDriven = locomotionDriven;
                ClipName = clipName;
                CycleOffset = cycleOffset;
            }

            public string Name { get; }
            public string AssetPath { get; }
            public float PlaybackSpeed { get; }
            public bool LocomotionDriven { get; }
            public string ClipName { get; }
            public float CycleOffset { get; }
        }

        public const string RuntimeVisualName = "RUNTIME_DESIGNER_A_VISUAL";
        public const string ControllerRoot =
            "Assets/_Project/Art/Animation/DesignerAControllers";

        private const string PlayerAimUpperBodyMaskPath =
            ControllerRoot + "/CHR00_AimUpperBody.mask";
        private const string PlayerAimUpperBodyLayerName = "Aim Upper Body";
        private const string PlayerAimUpperBodyStateName = "AimUpperBodyPose";
        private const string PlayerStrapUpperBodyLayerName = "Strap Upper Body";
        private const string PlayerStrapRollingUpperBodyStateName =
            "StrapRollingUpperBodyPose";
        private const string PlayerStrapTensionUpperBodyStateName =
            "StrapTensionUpperBodyPose";
        private const string PlayerStrapAttackLeftUpperBodyStateName =
            "StrapAttackLeftUpperBodyAction";
        private const string PlayerStrapAttackRightUpperBodyStateName =
            "StrapAttackRightUpperBodyAction";
        private const string PlayerMeleeMovingLegsMaskPath =
            ControllerRoot + "/CHR00_MeleeMovingLegs.mask";
        private const string PlayerMeleeMovingLegsLayerName = "Melee Moving Legs";
        private const string PlayerMeleeLegsIdleStateName = "MeleeLegsIdle";
        private const string PlayerMeleeLegsForwardStateName = "MeleeLegsForward";
        private const string PlayerMeleeLegsBackwardStateName = "MeleeLegsBackward";
        private const string PlayerMeleeLegsLeftStateName = "MeleeLegsLeft";
        private const string PlayerMeleeLegsRightStateName = "MeleeLegsRight";
        private const string PlayerMeleeLegsRunStateName = "MeleeLegsRun";

        private const string CharacterRoot = "Assets/_Project/Art/Characters";
        private const string AnimationRoot = "Assets/_Project/Art/Animation";
        private const string TextureRoot =
            "Assets/_Project/Art/Textures/Characters/DesignerA";
        private const string MaterialRoot =
            "Assets/_Project/Art/Materials/Characters/DesignerA";
        private const string CharacterHitEmissionMaskPath =
            MaterialRoot + "/T_CharacterHitEmissionBlack.asset";

        private const string Chr00Model =
            CharacterRoot + "/CHR00/DesignerA/CHR00_Player.fbx";
        private const string Chr01Model =
            CharacterRoot + "/CHR01/DesignerA/CHR01_Ally.fbx";
        private const string EnyNormalModel =
            CharacterRoot + "/ENY01/DesignerA/ENY01_Normal.fbx";
        private const string EnyEliteModel =
            CharacterRoot + "/ENY01/DesignerA/ENY01_Elite.fbx";

        private const string Chr00AnimationRoot =
            AnimationRoot + "/CHR00/DesignerA";
        private const string Chr01AnimationRoot =
            AnimationRoot + "/CHR01";
        private const string EnyNormalAnimationRoot =
            AnimationRoot + "/ENY01/Normal";
        private const string EnyEliteAnimationRoot =
            AnimationRoot + "/ENY01/Elite";

        private const string Chr01SupplyReleaseRuntimeClipName =
            "CHR01_Supply_Binder_Release_Runtime";
        private const string Chr01SupplyWindupRuntimeClipName =
            "CHR01_Supply_Binder_Windup_Runtime";

        private static readonly string[] Chr01LatestClipNames =
        {
            "CHR01_Getup_B",
            "CHR01_Getup_F",
            "CHR01_Idle",
            "CHR01_Idle_Variant_Stretch",
            "CHR01_Jump",
            "CHR01_Knockdown_B",
            "CHR01_Knockdown_F",
            "CHR01_Melee_Hit_Body",
            "CHR01_Run_F",
            "CHR01_Run_Stop",
            "CHR01_Strafe_L",
            "CHR01_Strafe_R",
            "CHR01_Supply_Binder_Release",
            "CHR01_Supply_Binder_Windup",
            "CHR01_Turn_180_Running",
            "CHR01_Turn_180_Walking",
            "CHR01_Turn_L_90",
            "CHR01_Turn_R_90",
            "CHR01_Walk_F",
            "CHR01_Walk_Stop"
        };

        private static readonly HashSet<string> Chr01LoopingClipNames =
            new(StringComparer.Ordinal)
            {
                "CHR01_Idle",
                "CHR01_Walk_F",
                "CHR01_Run_F",
                "CHR01_Strafe_L",
                "CHR01_Strafe_R",
                "CHR01_Supply_Binder_Windup"
            };

        private static readonly string[] EnyLatestClipSuffixes =
        {
            "Hit_BigHeadHit",
            "Hit_Body",
            "Hit_Head",
            "Hit_Reaction",
            "Hit_Rib",
            "Hit_Stomach",
            "Idle",
            "Injured_Backwards_Turn_Left",
            "Injured_Backwards_Turn_Right",
            "Injured_Hurting_Idle",
            "Injured_Idle",
            "Injured_Run_Backwards",
            "Injured_Run_Backwards_Left_Turn",
            "Injured_Run_Backwards_Right_Turn",
            "Injured_Run_Jump",
            "Injured_Run_Left_Turn",
            "Injured_Run_Right_Turn",
            "Injured_Run",
            "Injured_Standing_Jump",
            "Injured_Stumble_Idle",
            "Injured_Turn_Left",
            "Injured_Turn_Right",
            "Injured_Walk_Backwards",
            "Injured_Walk_Left_Turn",
            "Injured_Walk_Right_Turn",
            "Injured_Walk",
            "Injured_Wave_Idle",
            "Jump",
            "Left_Strafe",
            "Left_Strafe_Walking",
            "Left_Turn_90",
            "Lod_Mutant_Punch",
            "Lod_Mutant_Swiping",
            "Lod_Mutant_Walking",
            "Lod_One_Hand_Club_Combo",
            "Melee_Candidate_Boxing",
            "Melee_Candidate_Boxing_2",
            "Melee_Candidate_Kicking",
            "Melee_Candidate_Kicking_2",
            "Right_Strafe",
            "Right_Strafe_Walking",
            "Right_Turn_90",
            "Run_Backwards",
            "Standard_Run",
            "Turn_Left",
            "Turning_Right_45_Degrees",
            "Walking",
            "Walking_Backwards"
        };

        private static readonly HashSet<string> EnyLatestLoopingClipSuffixes =
            new(StringComparer.Ordinal)
            {
                "Idle",
                "Walking",
                "Walking_Backwards",
                "Standard_Run",
                "Run_Backwards",
                "Left_Strafe",
                "Right_Strafe",
                "Left_Strafe_Walking",
                "Right_Strafe_Walking",
                "Lod_Mutant_Walking",
                "Injured_Hurting_Idle",
                "Injured_Idle",
                "Injured_Stumble_Idle",
                "Injured_Wave_Idle",
                "Injured_Walk",
                "Injured_Walk_Backwards",
                "Injured_Run",
                "Injured_Run_Backwards"
            };

        private static readonly CharacterDefinition[] Characters =
        {
            new CharacterDefinition(
                "CHR00",
                ShowcaseCharacterAnimationRole.Player,
                Chr00Model,
                "GAMEPLAY_CHR00_PLAYER",
                "Player",
                ControllerRoot + "/CHR00_Player.controller"),
            new CharacterDefinition(
                "CHR01",
                ShowcaseCharacterAnimationRole.Ally,
                Chr01Model,
                "GAMEPLAY_CHR01_MOBILE_ALLY",
                "AllyProxy",
                ControllerRoot + "/CHR01_Ally.controller"),
            new CharacterDefinition(
                "ENY01_NORMAL",
                ShowcaseCharacterAnimationRole.EnemyNormal,
                EnyNormalModel,
                "GAMEPLAY_ENY01_RUNNER_01",
                "EnemyProxy_01",
                ControllerRoot + "/ENY01_Normal.controller"),
            new CharacterDefinition(
                "ENY01_ELITE",
                ShowcaseCharacterAnimationRole.EnemyElite,
                EnyEliteModel,
                "GAMEPLAY_ENY01_RUNNER_02",
                "EnemyProxy_02",
                ControllerRoot + "/ENY01_Elite.controller")
        };

        [MenuItem("BREAK NOTICE/Contest/Integrate Approved Designer A Characters")]
        public static void IntegrateApprovedCharacters()
        {
            ConfigureImports();
            CreateControllers();

            Scene scene = EditorSceneManager.OpenScene(
                ContestShowcaseSceneAuthoring.ShowcaseS3ScenePath,
                OpenSceneMode.Single);
            foreach (CharacterDefinition definition in Characters)
            {
                IntegrateCharacter(scene, definition);
            }

            ValidateScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(
                scene,
                ContestShowcaseSceneAuthoring.ShowcaseS3ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log(
                "DESIGNER A RUNTIME INTEGRATION PASS: " +
                "CHR00 + CHR01 + ENY01 Normal + ENY01 Elite, " +
                "approved animation catalogs and live gameplay drivers connected.");
        }

        [MenuItem("BREAK NOTICE/Contest/Rebuild CHR00 Player Animation Controller")]
        public static void RebuildPlayerAnimationController()
        {
            EnsureFolder(ControllerRoot);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ConfigureAnimationFolder(Chr00AnimationRoot + "/Approved");
            ConfigurePlayerMeleeSubclips();
            ConfigurePlayerStrapSubclips();
            ConfigureCombatAnimationEvents();
            CreatePlayerController();
            AssetDatabase.SaveAssets();
            Debug.Log(
                "CHR00 UPDATED ANIMATION SET PASS: independent Humanoid imports, " +
                "Jab/Cross subclips, owned events and player controller rebuilt; " +
                "no Scene was opened or modified.");
        }

        [MenuItem("BREAK NOTICE/Contest/Rebuild ENY01 Enemy Animation Controllers")]
        public static void RebuildEnemyAnimationControllers()
        {
            EnsureFolder(ControllerRoot);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ConfigureLatestEnemyAnimations();
            CreateEnemyController(false);
            CreateEnemyController(true);
            AssetDatabase.SaveAssets();
            Debug.Log(
                "ENY01 LATEST ANIMATION SET PASS: Normal and Elite each imported " +
                "their own exact 48 Humanoid sources; 17/19-state controllers rebuilt; " +
                "no CHR00, CHR01, or Scene authoring was invoked.");
        }

        [MenuItem("BREAK NOTICE/Contest/Rebuild CHR01 Ally Animation Controller")]
        public static void RebuildAllyAnimationController()
        {
            EnsureFolder(ControllerRoot);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ConfigureLatestChr01Animations();
            CreateAllyController();
            AssetDatabase.SaveAssets();
            Debug.Log(
                "CHR01 LATEST ANIMATION SET PASS: exact top-level 20 independent " +
                "Humanoid sources imported, five runtime states rebuilt, Supply " +
                "release synchronized; no CHR00, ENY01, or Scene authoring invoked.");
        }

        [MenuItem("BREAK NOTICE/Contest/Reset CHR00 Strap Avatars From Own FBX")]
        public static void ResetPlayerStrapAvatarsFromOwnFbx()
        {
            string approvedRoot = Chr00AnimationRoot + "/Approved";
            string[] paths = AssetDatabase.FindAssets("t:Model", new[] { approvedRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => IsApprovedPlayerStrapClip(path))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            if (paths.Length != 6)
            {
                throw new InvalidOperationException(
                    $"Expected exactly six approved CHR00 Strap FBX files, found {paths.Length}.");
            }

            foreach (string path in paths)
            {
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null)
                {
                    throw new InvalidOperationException(
                        $"Strap animation importer is missing: {path}");
                }

                importer.clipAnimations = Array.Empty<ModelImporterClipAnimation>();
                importer.animationType = ModelImporterAnimationType.Generic;
                importer.avatarSetup = ModelImporterAvatarSetup.NoAvatar;
                importer.sourceAvatar = null;
                importer.SaveAndReimport();
            }

            RebuildPlayerAnimationController();
        }

        public static void ValidateCurrentIntegration()
        {
            Scene scene = EditorSceneManager.OpenScene(
                ContestShowcaseSceneAuthoring.ShowcaseS3ScenePath,
                OpenSceneMode.Single);
            ValidateScene(scene);
            Debug.Log("DESIGNER A RUNTIME INTEGRATION VALIDATION PASS.");
        }

        [MenuItem("BREAK NOTICE/Contest/Integrate Character Combat And Health")]
        public static void IntegrateShowcaseCombatAndHealth()
        {
            CreateCombatControllers();

            Scene scene = EditorSceneManager.OpenScene(
                ContestShowcaseSceneAuthoring.ShowcaseS7ScenePath,
                OpenSceneMode.Single);

            GameObject player = FindRootAny(
                scene,
                Characters[0].PrimaryRoot,
                Characters[0].LegacyRoot);
            ShowcaseRunnerPressureController pressure = scene.GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<ShowcaseRunnerPressureController>(true))
                .Single();
            AuthoredEncounterResetController reset = scene.GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<AuthoredEncounterResetController>(true))
                .Single();
            ShowcaseObjectiveController objective = scene.GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<ShowcaseObjectiveController>(true))
                .Single();

            ShowcaseCombatHealth playerHealth =
                player.GetComponent<ShowcaseCombatHealth>() ??
                player.AddComponent<ShowcaseCombatHealth>();
            GameObject ally = FindRootAny(
                scene,
                Characters[1].PrimaryRoot,
                Characters[1].LegacyRoot);
            PrototypeAllyRelayController allyRelay =
                ally.GetComponent<PrototypeAllyRelayController>();
            if (allyRelay == null)
            {
                throw new InvalidOperationException(
                    "The authored CHR01 relay owner is required for animation sync.");
            }
            allyRelay.ConfigureAnimationSync(true);
            playerHealth.Configure(ShowcaseCombatTeam.Player, 100, 0.6f);
            ShowcasePlayerCombatController playerCombat =
                player.GetComponent<ShowcasePlayerCombatController>() ??
                player.AddComponent<ShowcasePlayerCombatController>();
            playerCombat.Configure(
                playerHealth,
                player.GetComponent<PrototypeInteractionStateOwner>(),
                player.GetComponent<InputSystemLogicalInputAdapter>(),
                player.GetComponent<ThirdPersonLocomotion>(),
                player.GetComponent<PrototypePhysicalObjectLifecycle>(),
                allyRelay,
                pressure,
                reset,
                objective);
            player.GetComponent<PrototypeInteractionController>()
                .ConfigureShowcaseCombat(playerCombat);
            pressure.ConfigurePlayerCombat(playerCombat);
            objective.ConfigurePlayerHealth(playerHealth);

            ShowcaseCharacterAnimationDriver playerDriver =
                player.GetComponent<ShowcaseCharacterAnimationDriver>();
            ShowcaseBinderGripSocket playerGrip =
                player.GetComponent<ShowcaseBinderGripSocket>() ??
                player.AddComponent<ShowcaseBinderGripSocket>();
            playerGrip.Configure(
                playerDriver.Animator,
                player.transform,
                new Vector3(0f, -0.12f, 0.04f),
                new Vector3(0f, 90f, 0f));
            Transform playerCarryAnchor = player.transform.Find("ContestCarryAnchor");
            if (playerCarryAnchor == null)
            {
                throw new InvalidOperationException(
                    "The authored CHR00 ContestCarryAnchor is required for prop grip sync.");
            }
            ShowcasePlayerPropGripRig playerPropGrip =
                playerDriver.Animator.GetComponent<ShowcasePlayerPropGripRig>() ??
                playerDriver.Animator.gameObject.AddComponent<ShowcasePlayerPropGripRig>();
            playerPropGrip.Configure(
                playerDriver.Animator,
                player.transform,
                playerCarryAnchor,
                playerGrip);
            PrototypePhysicalObjectLifecycle playerLifecycle =
                player.GetComponent<PrototypePhysicalObjectLifecycle>();
            playerLifecycle.ConfigureLightBinderGrip(playerGrip, 0.18f);
            playerLifecycle.ConfigurePlayerPropGrip(playerPropGrip);
            ShowcaseBoneCombatRig playerBoneRig =
                player.GetComponent<ShowcaseBoneCombatRig>() ??
                player.AddComponent<ShowcaseBoneCombatRig>();
            playerBoneRig.Configure(
                playerDriver.Animator,
                ShowcaseCombatTeam.Player,
                playerCombat,
                null);
            playerCombat.ConfigureBoneCombatRig(playerBoneRig);
            playerDriver.ConfigureCombat(
                playerCombat,
                null,
                player.GetComponent<ThirdPersonLocomotion>());
            ConfigureAnimationReceiver(
                playerDriver.Animator,
                playerCombat,
                null,
                null,
                player.GetComponent<ShowcaseCargoStrapController>());

            ShowcaseCharacterAnimationDriver allyDriver =
                ally.GetComponent<ShowcaseCharacterAnimationDriver>();
            ShowcaseBinderGripSocket allyGrip =
                ally.GetComponent<ShowcaseBinderGripSocket>() ??
                ally.AddComponent<ShowcaseBinderGripSocket>();
            allyGrip.Configure(
                allyDriver.Animator,
                ally.transform,
                new Vector3(0f, -0.11f, 0.035f),
                new Vector3(0f, 90f, 0f));
            allyRelay.ConfigureBinderGrip(allyGrip);
            ShowcaseChairGripRig chairGrip =
                allyDriver.Animator.GetComponent<ShowcaseChairGripRig>() ??
                allyDriver.Animator.gameObject.AddComponent<ShowcaseChairGripRig>();
            chairGrip.Configure(allyDriver.Animator);
            allyRelay.ConfigureChairGrip(chairGrip);
            ShowcaseBoneCombatRig allyBoneRig =
                ally.GetComponent<ShowcaseBoneCombatRig>() ??
                ally.AddComponent<ShowcaseBoneCombatRig>();
            allyBoneRig.Configure(
                allyDriver.Animator,
                ShowcaseCombatTeam.Ally,
                null,
                null);
            ConfigureAnimationReceiver(
                allyDriver.Animator,
                null,
                null,
                allyRelay);
            EditorUtility.SetDirty(allyRelay);

            for (int index = 0; index < 2; index++)
            {
                CharacterDefinition definition = Characters[index + 2];
                GameObject runnerObject = FindRootAny(
                    scene,
                    definition.PrimaryRoot,
                    definition.LegacyRoot);
                ShowcaseRunnerAgent runner = runnerObject.GetComponent<ShowcaseRunnerAgent>();
                ShowcaseCombatHealth runnerHealth =
                    runnerObject.GetComponent<ShowcaseCombatHealth>() ??
                    runnerObject.AddComponent<ShowcaseCombatHealth>();
                bool elite = index == 1;
                runnerHealth.Configure(
                    ShowcaseCombatTeam.Enemy,
                    elite ? 180 : 100,
                    0.12f);
                runner.Configure(
                    $"RUNNER {index + 1}",
                    pressure,
                    runnerObject.GetComponent<Rigidbody>(),
                    player.transform,
                    2.7f,
                    6.5f,
                    runnerHealth,
                    elite);
                ShowcaseCharacterAnimationDriver driver =
                    runnerObject.GetComponent<ShowcaseCharacterAnimationDriver>();
                ShowcaseBoneCombatRig runnerBoneRig =
                    runnerObject.GetComponent<ShowcaseBoneCombatRig>() ??
                    runnerObject.AddComponent<ShowcaseBoneCombatRig>();
                runnerBoneRig.Configure(
                    driver.Animator,
                    ShowcaseCombatTeam.Enemy,
                    null,
                    runner);
                runner.ConfigureBoneCombatRig(runnerBoneRig);
                driver.ConfigureCombat(null, runner, null);
                ConfigureAnimationReceiver(driver.Animator, null, runner, null);
                EditorUtility.SetDirty(runnerObject);
                EditorUtility.SetDirty(runner);
                EditorUtility.SetDirty(runnerHealth);
                EditorUtility.SetDirty(driver);
                EditorUtility.SetDirty(runnerBoneRig);
            }

            ValidateScene(scene);
            EditorUtility.SetDirty(player);
            EditorUtility.SetDirty(playerHealth);
            EditorUtility.SetDirty(playerCombat);
            EditorUtility.SetDirty(playerDriver);
            EditorUtility.SetDirty(playerBoneRig);
            EditorUtility.SetDirty(playerGrip);
            EditorUtility.SetDirty(playerPropGrip);
            EditorUtility.SetDirty(playerLifecycle);
            EditorUtility.SetDirty(allyBoneRig);
            EditorUtility.SetDirty(allyGrip);
            EditorUtility.SetDirty(pressure);
            EditorUtility.SetDirty(objective);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(
                    scene,
                    ContestShowcaseSceneAuthoring.ShowcaseS7ScenePath))
            {
                throw new InvalidOperationException(
                    "Could not save the character combat Showcase Scene.");
            }
            AssetDatabase.SaveAssets();
            Debug.Log(
                "SHOWCASE CHARACTER COMBAT + HEALTH INTEGRATION PASS: CHR00 combo, " +
                "animation-event carry/throw release, Player 100 HP, Normal 100 HP, " +
                "Elite 180 HP and deterministic failure/reset contracts connected.");
        }

        private static void ConfigureAnimationReceiver(
            Animator animator,
            ShowcasePlayerCombatController playerCombat,
            ShowcaseRunnerAgent runner,
            PrototypeAllyRelayController allyRelay,
            ShowcaseCargoStrapController cargoStrap = null)
        {
            if (animator == null)
            {
                throw new InvalidOperationException("Runtime Animator is required.");
            }
            ShowcaseAnimationEventReceiver receiver =
                animator.GetComponent<ShowcaseAnimationEventReceiver>() ??
                animator.gameObject.AddComponent<ShowcaseAnimationEventReceiver>();
            receiver.Configure(playerCombat, runner, allyRelay, cargoStrap);
            EditorUtility.SetDirty(receiver);
        }

        [MenuItem("BREAK NOTICE/Contest/Repair Designer A Runtime Locomotion")]
        public static void RepairRuntimeFacingAndLocomotion()
        {
            CreateControllers();
            Scene scene = EditorSceneManager.OpenScene(
                ContestShowcaseSceneAuthoring.ShowcaseS3ScenePath,
                OpenSceneMode.Single);
            foreach (CharacterDefinition definition in Characters)
            {
                GameObject actor = FindRootAny(
                    scene,
                    definition.PrimaryRoot,
                    definition.LegacyRoot);
                Transform visual = actor.transform.Find(RuntimeVisualName);
                if (visual == null || visual.childCount != 1)
                {
                    throw new InvalidOperationException(
                        $"Designer A runtime visual hierarchy is invalid: {definition.Key}");
                }

                Transform model = visual.GetChild(0);
                model.localRotation = Quaternion.identity;
                NormalizeVisual(actor.transform, model.gameObject, definition.Role);
                Animator animator = model.GetComponent<Animator>() ??
                    model.GetComponentInChildren<Animator>(true);
                if (animator == null)
                {
                    throw new InvalidOperationException(
                        $"Designer A runtime animator is missing: {definition.Key}");
                }
                animator.applyRootMotion = false;
                animator.stabilizeFeet = false;
                EditorUtility.SetDirty(model);
                EditorUtility.SetDirty(animator);
            }

            ValidateScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(
                scene,
                ContestShowcaseSceneAuthoring.ShowcaseS3ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log(
                "DESIGNER A RUNTIME LOCOMOTION REPAIR PASS: +Z forward, " +
                "state-specific Idle-Walk-Run cadence, immutable visual-root baseline and " +
                "physical actor grounding applied; Foot IK remains inactive.");
        }

        private static void ConfigureImports()
        {
            EnsureFolder(ControllerRoot);
            EnsureFolder(TextureRoot);
            EnsureFolder(MaterialRoot);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            foreach (CharacterDefinition definition in Characters)
            {
                ConfigureModelImporter(definition.ModelPath);
            }

            LoadAvatar(Chr00Model);
            LoadAvatar(Chr01Model);
            LoadAvatar(EnyNormalModel);
            LoadAvatar(EnyEliteModel);
            ConfigureAnimationFolder(Chr00AnimationRoot);
            ConfigureLatestChr01Animations();
            ConfigureLatestEnemyAnimations();

            foreach (CharacterDefinition definition in Characters)
            {
                ExtractAndConfigureTextures(definition);
            }
        }

        private static void ConfigureModelImporter(string assetPath)
        {
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    $"Designer A model importer is missing: {assetPath}");
            }

            importer.importAnimation = true;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.globalScale = 1f;
            importer.importCameras = false;
            importer.importLights = false;
            importer.materialImportMode =
                ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
            importer.materialName =
                ModelImporterMaterialName.BasedOnModelNameAndMaterialName;
            importer.materialSearch = ModelImporterMaterialSearch.Local;
            importer.SaveAndReimport();
        }

        private static void ConfigureLatestChr01Animations()
        {
            string[] paths = AssetDatabase.FindAssets(
                    "t:Model",
                    new[] { Chr01AnimationRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path =>
                    path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        Path.GetDirectoryName(path)?.Replace('\\', '/'),
                        Chr01AnimationRoot,
                        StringComparison.Ordinal))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            string[] actualNames = paths
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();
            if (paths.Length != Chr01LatestClipNames.Length ||
                !actualNames.ToHashSet(StringComparer.Ordinal).SetEquals(
                    Chr01LatestClipNames))
            {
                throw new InvalidOperationException(
                    "CHR01 latest delivery must contain exactly the approved " +
                    $"top-level 20 FBX files; found {paths.Length}.");
            }

            foreach (string path in paths)
            {
                ModelImporter importer =
                    AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null)
                {
                    throw new InvalidOperationException(
                        $"CHR01 latest animation importer is missing: {path}");
                }

                string clipName = Path.GetFileNameWithoutExtension(path);
                bool release = string.Equals(
                    clipName,
                    "CHR01_Supply_Binder_Release",
                    StringComparison.Ordinal);
                bool loop = Chr01LoopingClipNames.Contains(clipName);
                ModelImporterClipAnimation source =
                    importer.defaultClipAnimations?.SingleOrDefault();
                if (source == null)
                {
                    throw new InvalidOperationException(
                        $"CHR01 latest source must have one default take: {path}");
                }

                ModelImporterClipAnimation[] existing = importer.clipAnimations;
                ModelImporterClipAnimation[] desiredClips;
                if (release)
                {
                    ModelImporterClipAnimation windup = CreateRuntimeSubclip(
                        Chr01SupplyWindupRuntimeClipName,
                        source,
                        21f,
                        22f,
                        existing,
                        true);
                    windup.events = Array.Empty<AnimationEvent>();
                    ModelImporterClipAnimation releaseClip = CreateRuntimeSubclip(
                        Chr01SupplyReleaseRuntimeClipName,
                        source,
                        21f,
                        44f,
                        existing);
                    releaseClip.events = new[]
                    {
                        new AnimationEvent
                        {
                            functionName = "ShowcaseAllySupplyRelease",
                            // Unity source frames 21..44; Blender reports 22..45.
                            // The authored open-hand release is source frame 28.
                            time = ShowcaseNonPlayerAnimationTiming.AllySupplyRelease
                        }
                    };
                    desiredClips = new[] { windup, releaseClip };
                }
                else
                {
                    ModelImporterClipAnimation desired = CreateRuntimeSubclip(
                        clipName,
                        source,
                        source.firstFrame,
                        source.lastFrame,
                        existing,
                        loop);
                    desired.events = Array.Empty<AnimationEvent>();
                    desiredClips = new[] { desired };
                }

                bool importerChanged =
                    !importer.importAnimation ||
                    importer.animationType != ModelImporterAnimationType.Human ||
                    importer.avatarSetup !=
                        ModelImporterAvatarSetup.CreateFromThisModel ||
                    importer.sourceAvatar != null ||
                    importer.importCameras ||
                    importer.importLights ||
                    importer.materialImportMode != ModelImporterMaterialImportMode.None;
                importer.importAnimation = true;
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.sourceAvatar = null;
                importer.importCameras = false;
                importer.importLights = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;

                HumanDescription humanDescription = importer.humanDescription;
                if (humanDescription.hasTranslationDoF)
                {
                    humanDescription.hasTranslationDoF = false;
                    importer.humanDescription = humanDescription;
                    importerChanged = true;
                }

                bool clipChanged = existing == null ||
                    existing.Length != desiredClips.Length;
                if (!clipChanged)
                {
                    for (int index = 0; index < desiredClips.Length; index++)
                    {
                        ModelImporterClipAnimation actual = existing[index];
                        ModelImporterClipAnimation desired = desiredClips[index];
                        if (actual.name != desired.name ||
                            actual.takeName != desired.takeName ||
                            !Mathf.Approximately(actual.firstFrame, desired.firstFrame) ||
                            !Mathf.Approximately(actual.lastFrame, desired.lastFrame) ||
                            actual.loopTime != desired.loopTime ||
                            actual.loopPose != desired.loopPose ||
                            !actual.lockRootRotation ||
                            actual.keepOriginalOrientation ||
                            !actual.lockRootHeightY ||
                            actual.keepOriginalPositionY ||
                            !actual.lockRootPositionXZ ||
                            actual.keepOriginalPositionXZ ||
                            !actual.heightFromFeet ||
                            !AnimationEventsEqual(actual.events, desired.events))
                        {
                            clipChanged = true;
                            break;
                        }
                    }
                }
                if (importerChanged || clipChanged)
                {
                    importer.clipAnimations = desiredClips;
                    importer.SaveAndReimport();
                }

                LoadAvatar(path);
            }
        }

        private static void ConfigureLatestEnemyAnimations()
        {
            ConfigureLatestEnemyAnimations(false);
            ConfigureLatestEnemyAnimations(true);
        }

        private static void ConfigureLatestEnemyAnimations(bool elite)
        {
            string variant = elite ? "Elite" : "Normal";
            string root = elite ? EnyEliteAnimationRoot : EnyNormalAnimationRoot;
            string filePrefix = $"ENY01_{variant}_";
            string[] paths = AssetDatabase.FindAssets("t:Model", new[] { root })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path =>
                    path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        Path.GetDirectoryName(path)?.Replace('\\', '/'),
                        root,
                        StringComparison.Ordinal))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            string[] expectedNames = EnyLatestClipSuffixes
                .Select(suffix => filePrefix + suffix)
                .ToArray();
            string[] actualNames = paths
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();
            if (paths.Length != EnyLatestClipSuffixes.Length ||
                !actualNames.ToHashSet(StringComparer.Ordinal).SetEquals(expectedNames))
            {
                throw new InvalidOperationException(
                    $"ENY01 {variant} latest delivery must contain exactly the " +
                    $"approved flat 48 FBX files; found {paths.Length}.");
            }

            foreach (string path in paths)
            {
                ModelImporter importer =
                    AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null)
                {
                    throw new InvalidOperationException(
                        $"ENY01 {variant} animation importer is missing: {path}");
                }

                string sourceName = Path.GetFileNameWithoutExtension(path);
                string suffix = sourceName.Substring(filePrefix.Length);
                ModelImporterClipAnimation source =
                    importer.defaultClipAnimations?.SingleOrDefault();
                if (source == null)
                {
                    throw new InvalidOperationException(
                        $"ENY01 {variant} source must have one default take: {path}");
                }

                ModelImporterClipAnimation[] existing = importer.clipAnimations;
                bool loop = EnyLatestLoopingClipSuffixes.Contains(suffix);
                ModelImporterClipAnimation desired = CreateRuntimeSubclip(
                    sourceName,
                    source,
                    source.firstFrame,
                    source.lastFrame,
                    existing,
                    loop);
                desired.events = CreateLatestEnemyAnimationEvents(elite, suffix);

                bool importerChanged =
                    !importer.importAnimation ||
                    importer.animationType != ModelImporterAnimationType.Human ||
                    importer.avatarSetup !=
                        ModelImporterAvatarSetup.CreateFromThisModel ||
                    importer.sourceAvatar != null ||
                    importer.importCameras ||
                    importer.importLights ||
                    importer.materialImportMode != ModelImporterMaterialImportMode.None ||
                    !Mathf.Approximately(importer.globalScale, 1f);
                importer.importAnimation = true;
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.sourceAvatar = null;
                importer.importCameras = false;
                importer.importLights = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.globalScale = 1f;

                HumanDescription humanDescription = importer.humanDescription;
                if (humanDescription.hasTranslationDoF)
                {
                    humanDescription.hasTranslationDoF = false;
                    importer.humanDescription = humanDescription;
                    importerChanged = true;
                }

                bool clipChanged = existing == null || existing.Length != 1 ||
                    existing[0].name != desired.name ||
                    existing[0].takeName != desired.takeName ||
                    !Mathf.Approximately(existing[0].firstFrame, desired.firstFrame) ||
                    !Mathf.Approximately(existing[0].lastFrame, desired.lastFrame) ||
                    existing[0].loopTime != desired.loopTime ||
                    existing[0].loopPose != desired.loopPose ||
                    !existing[0].lockRootRotation ||
                    existing[0].keepOriginalOrientation ||
                    !existing[0].lockRootHeightY ||
                    existing[0].keepOriginalPositionY ||
                    !existing[0].lockRootPositionXZ ||
                    existing[0].keepOriginalPositionXZ ||
                    !existing[0].heightFromFeet ||
                    !AnimationEventsEqual(existing[0].events, desired.events);
                if (importerChanged || clipChanged)
                {
                    importer.clipAnimations = new[] { desired };
                    importer.SaveAndReimport();
                }

                LoadAvatar(path);
            }
        }

        private static AnimationEvent[] CreateLatestEnemyAnimationEvents(
            bool elite,
            string suffix)
        {
            if (suffix == "Standard_Run")
            {
                return new[]
                {
                    new AnimationEvent
                    {
                        functionName = "ShowcaseLeftFootstep",
                        time = ShowcaseNonPlayerAnimationTiming.EnemyRunLeftFootstep
                    },
                    new AnimationEvent
                    {
                        functionName = "ShowcaseRightFootstep",
                        time = ShowcaseNonPlayerAnimationTiming.EnemyRunRightFootstep
                    }
                };
            }

            (float open, float close)? attack = suffix switch
            {
                "Melee_Candidate_Boxing" => (
                    ShowcaseNonPlayerAnimationTiming.BoxingWindowOpen,
                    ShowcaseNonPlayerAnimationTiming.BoxingWindowClose),
                "Melee_Candidate_Boxing_2" => (
                    ShowcaseNonPlayerAnimationTiming.Boxing2WindowOpen,
                    ShowcaseNonPlayerAnimationTiming.Boxing2WindowClose),
                "Melee_Candidate_Kicking" => (
                    ShowcaseNonPlayerAnimationTiming.KickingWindowOpen,
                    ShowcaseNonPlayerAnimationTiming.KickingWindowClose),
                "Melee_Candidate_Kicking_2" => (
                    ShowcaseNonPlayerAnimationTiming.Kicking2WindowOpen,
                    ShowcaseNonPlayerAnimationTiming.Kicking2WindowClose),
                "Lod_Mutant_Punch" when elite => (
                    ShowcaseNonPlayerAnimationTiming.MutantPunchWindowOpen,
                    ShowcaseNonPlayerAnimationTiming.MutantPunchWindowClose),
                "Lod_Mutant_Swiping" when elite => (
                    ShowcaseNonPlayerAnimationTiming.MutantSwipeWindowOpen,
                    ShowcaseNonPlayerAnimationTiming.MutantSwipeWindowClose),
                _ => null
            };
            if (attack.HasValue)
            {
                return new[]
                {
                    new AnimationEvent
                    {
                        functionName = "ShowcaseAttackWindowOpen",
                        time = attack.Value.open
                    },
                    new AnimationEvent
                    {
                        functionName = "ShowcaseAttackWindowClose",
                        time = attack.Value.close
                    },
                    new AnimationEvent
                    {
                        functionName = "ShowcaseActionComplete",
                        time = ShowcaseNonPlayerAnimationTiming.EnemyActionComplete
                    }
                };
            }

            bool connectedHit = suffix == "Hit_Head" ||
                suffix == "Hit_Body" ||
                suffix == "Hit_Rib" ||
                suffix == "Hit_Stomach" ||
                suffix == "Hit_BigHeadHit";
            return connectedHit
                ? new[]
                {
                    new AnimationEvent
                    {
                        functionName = "ShowcaseActionComplete",
                        time = ShowcaseNonPlayerAnimationTiming.EnemyHitComplete
                    }
                }
                : Array.Empty<AnimationEvent>();
        }

        private static void ConfigureAnimationFolder(string folder)
        {
            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { folder });
            string[] paths = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            if (paths.Length == 0)
            {
                throw new InvalidOperationException(
                    $"No Designer A animation FBX files were imported from {folder}.");
            }

            foreach (string path in paths)
            {
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null)
                {
                    throw new InvalidOperationException(
                        $"Animation importer is missing: {path}");
                }

                ModelImporterClipAnimation[] importedDefaults =
                    importer.defaultClipAnimations;
                ModelImporterClipAnimation[] importedConfigured =
                    importer.clipAnimations;
                bool sourceTakeChanged = importedConfigured != null &&
                    importedConfigured.Length > 0 &&
                    (importedDefaults == null ||
                     importedDefaults.Length == 0 ||
                     importedConfigured.Any(clip => !importedDefaults.Any(source =>
                         source.takeName == clip.takeName)));
                if (sourceTakeChanged)
                {
                    // A binary replacement can keep the old .meta GUID while its
                    // source root/take changes. Clear the stale HumanDescription
                    // through Generic once before rebuilding an independent Avatar.
                    importer.clipAnimations = Array.Empty<ModelImporterClipAnimation>();
                    importer.animationType = ModelImporterAnimationType.Generic;
                    importer.avatarSetup = ModelImporterAvatarSetup.NoAvatar;
                    importer.sourceAvatar = null;
                    importer.SaveAndReimport();
                    importer = AssetImporter.GetAtPath(path) as ModelImporter;
                    if (importer == null)
                    {
                        throw new InvalidOperationException(
                            $"Animation importer disappeared after source reset: {path}");
                    }
                }

                bool changed =
                    sourceTakeChanged ||
                    !importer.importAnimation ||
                    importer.animationType != ModelImporterAnimationType.Human ||
                    importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel ||
                    importer.sourceAvatar != null ||
                    importer.importCameras ||
                    importer.importLights ||
                    importer.materialImportMode != ModelImporterMaterialImportMode.None;
                importer.importAnimation = true;
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.sourceAvatar = null;
                importer.importCameras = false;
                importer.importLights = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;

                // The approved strap clips deliberately animate a small set of
                // non-root translations. Unity otherwise discards them and
                // reports an Avatar bone-length mismatch on every import.
                // Translation DoF keeps each source-owned Humanoid Avatar
                // independent; Copy From Other Avatar remains prohibited.
                HumanDescription humanDescription = importer.humanDescription;
                bool useTranslationDof = IsApprovedPlayerStrapClip(path);
                if (humanDescription.hasTranslationDoF != useTranslationDof)
                {
                    humanDescription.hasTranslationDoF = useTranslationDof;
                    importer.humanDescription = humanDescription;
                    changed = true;
                }

                ModelImporterClipAnimation[] defaults = importer.defaultClipAnimations;
                if (defaults == null || defaults.Length == 0)
                {
                    throw new InvalidOperationException(
                        $"Animation source has no default take: {path}");
                }

                ModelImporterClipAnimation[] clips = importer.clipAnimations;
                bool configuredTakeMissing = clips == null || clips.Length == 0 ||
                    clips.Any(clip => !defaults.Any(source =>
                        source.takeName == clip.takeName));
                if (sourceTakeChanged || configuredTakeMissing)
                {
                    clips = defaults;
                    string clipName = Path.GetFileNameWithoutExtension(path);
                    foreach (ModelImporterClipAnimation clip in clips)
                    {
                        clip.name = clipName;
                    }
                    changed = true;
                }
                foreach (ModelImporterClipAnimation clip in clips)
                {
                    bool loop = IsLoopingClip(path);
                    changed |= clip.loopTime != loop ||
                               clip.loopPose != loop ||
                               !clip.lockRootRotation ||
                               clip.keepOriginalOrientation ||
                               !clip.lockRootHeightY ||
                               clip.keepOriginalPositionY ||
                               !clip.lockRootPositionXZ ||
                               clip.keepOriginalPositionXZ ||
                               !clip.heightFromFeet;
                    clip.loopTime = loop;
                    clip.loopPose = loop;
                    clip.lockRootRotation = true;
                    clip.keepOriginalOrientation = false;
                    clip.lockRootHeightY = true;
                    clip.keepOriginalPositionY = false;
                    clip.lockRootPositionXZ = true;
                    clip.keepOriginalPositionXZ = false;
                    clip.heightFromFeet = true;
                }

                if (changed)
                {
                    importer.clipAnimations = clips;
                    importer.SaveAndReimport();
                }
            }
        }

        private static bool IsLoopingClip(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            if (name.Contains("preview") ||
                name.Contains("_ref") ||
                name.Contains("short_alt") ||
                (name.StartsWith("chr00_turn_", StringComparison.Ordinal) &&
                 !name.Contains("loop_alt")))
            {
                return false;
            }
            return name.Contains("idle") ||
                   name.Contains("walking") ||
                   name.Contains("walk_f") ||
                   name.Contains("running") ||
                   name.Contains("run_f") ||
                   name.Contains("standard run") ||
                   name.Contains("strafe") ||
                   name.Contains("turn") ||
                   name.Contains("strap_rolling") ||
                   name.Contains("tension_loop") ||
                   name.Contains("windup");
        }

        private static bool IsApprovedPlayerStrapClip(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            return path.IndexOf(
                       "/CHR00/DesignerA/Approved/",
                       StringComparison.OrdinalIgnoreCase) >= 0 &&
                   name.StartsWith("CHR00_Strap_", StringComparison.Ordinal) &&
                   name.IndexOf("candidate", StringComparison.OrdinalIgnoreCase) < 0;
        }

        private static Avatar LoadAvatar(string modelPath)
        {
            Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(modelPath)
                .OfType<Avatar>()
                .FirstOrDefault(candidate => candidate.isValid && candidate.isHuman);
            if (avatar == null)
            {
                throw new InvalidOperationException(
                    $"A valid Humanoid avatar was not generated: {modelPath}");
            }

            return avatar;
        }

        private static void ExtractAndConfigureTextures(CharacterDefinition definition)
        {
            string textureFolder = TextureRoot + "/" + definition.Key;
            EnsureFolder(textureFolder);
            ModelImporter importer = AssetImporter.GetAtPath(definition.ModelPath) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    $"Model importer disappeared: {definition.ModelPath}");
            }

            importer.ExtractTextures(textureFolder);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            NormalizeExtractedTexturePath(definition, textureFolder);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            Texture2D[] textures = LoadTextures(textureFolder);
            if (textures.Length == 0)
            {
                throw new InvalidOperationException(
                    $"No embedded textures were extracted for {definition.Key}.");
            }

            foreach (Texture2D texture in textures)
            {
                string texturePath = AssetDatabase.GetAssetPath(texture);
                TextureImporter textureImporter =
                    AssetImporter.GetAtPath(texturePath) as TextureImporter;
                if (textureImporter == null)
                {
                    continue;
                }

                string lower = texture.name.ToLowerInvariant();
                textureImporter.textureType = lower.Contains("normal")
                    ? TextureImporterType.NormalMap
                    : TextureImporterType.Default;
                textureImporter.sRGBTexture =
                    !lower.Contains("normal") &&
                    !lower.Contains("metal") &&
                    !lower.Contains("rough");
                textureImporter.isReadable = lower.Contains("metal") || lower.Contains("rough");
                textureImporter.maxTextureSize = 4096;
                textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;
                textureImporter.SaveAndReimport();
            }

            GetOrCreateCharacterMaterial(definition, textureFolder);
        }

        private static void NormalizeExtractedTexturePath(
            CharacterDefinition definition,
            string textureFolder)
        {
            if (definition.Key != "CHR00")
            {
                return;
            }

            string extensionless = textureFolder + "/texture_0";
            string normalized = textureFolder + "/texture_diffuse.png";
            if (AssetImporter.GetAtPath(normalized) is TextureImporter)
            {
                if (AssetDatabase.LoadMainAssetAtPath(extensionless) != null)
                {
                    AssetDatabase.DeleteAsset(extensionless);
                }

                return;
            }

            string extensionlessAbsolute = Path.GetFullPath(extensionless);
            if (!File.Exists(extensionlessAbsolute))
            {
                throw new InvalidOperationException(
                    "The CHR00 embedded diffuse texture was not extracted.");
            }

            AssetDatabase.DeleteAsset(normalized);
            File.Copy(extensionlessAbsolute, Path.GetFullPath(normalized), true);
            AssetDatabase.DeleteAsset(extensionless);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset(
                normalized,
                ImportAssetOptions.ForceSynchronousImport |
                ImportAssetOptions.ForceUpdate);
        }

        private static Material GetOrCreateCharacterMaterial(
            CharacterDefinition definition,
            string textureFolder = null)
        {
            string materialPath = MaterialRoot + "/" + definition.Key + ".mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (existing != null && string.IsNullOrEmpty(textureFolder))
            {
                EnsureCharacterHitEmission(existing);
                return existing;
            }

            textureFolder ??= TextureRoot + "/" + definition.Key;
            Texture2D[] textures = LoadTextures(textureFolder);
            Texture2D diffuse = FindTexture(textures, "diffuse", "texture_0");
            Texture2D normal = FindTexture(textures, "normal");
            Texture2D metallic = FindTexture(textures, "metallic");
            Texture2D roughness = FindTexture(textures, "roughness");
            Texture2D emission = FindTexture(textures, "emissive", "emission");
            if (diffuse == null)
            {
                diffuse = textures.FirstOrDefault(texture =>
                    texture.name.IndexOf("normal", StringComparison.OrdinalIgnoreCase) < 0 &&
                    texture.name.IndexOf("metal", StringComparison.OrdinalIgnoreCase) < 0 &&
                    texture.name.IndexOf("rough", StringComparison.OrdinalIgnoreCase) < 0);
            }

            Shader shader = Shader.Find("Standard");
            if (shader == null || diffuse == null)
            {
                throw new InvalidOperationException(
                    $"Could not build the runtime material for {definition.Key}.");
            }

            Material material = existing;
            if (material == null)
            {
                material = new Material(shader) { name = definition.Key + "_DesignerA" };
                AssetDatabase.CreateAsset(material, materialPath);
            }
            else
            {
                material.shader = shader;
            }

            material.SetColor("_Color", Color.white);
            material.SetTexture("_MainTex", diffuse);
            material.SetFloat("_Glossiness", 0.35f);
            material.SetFloat("_Metallic", 0f);
            if (normal != null)
            {
                material.SetTexture("_BumpMap", normal);
                material.EnableKeyword("_NORMALMAP");
            }

            Texture2D packed = GetOrCreatePackedMetallic(
                definition.Key,
                metallic,
                roughness);
            if (packed != null)
            {
                material.SetTexture("_MetallicGlossMap", packed);
                material.SetFloat("_Metallic", 1f);
                material.EnableKeyword("_METALLICGLOSSMAP");
            }

            if (emission != null)
            {
                material.SetTexture("_EmissionMap", emission);
            }

            EnsureCharacterHitEmission(material);
            return material;
        }

        internal static void EnsureCharacterHitEmission(Material material)
        {
            if (material == null)
            {
                return;
            }

            // StandardShaderGUI removes _EMISSION again when the authored
            // emission is black. Keep a non-black authored color behind an
            // explicit black mask so the variant survives import/build while
            // the baseline appearance remains unchanged. Runtime feedback
            // overrides the map with white for the short visible pulse.
            Texture2D blackMask = GetOrCreateCharacterHitEmissionMask();
            if (material.HasProperty("_EmissionMap"))
            {
                material.SetTexture("_EmissionMap", blackMask);
            }
            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor(
                    "_EmissionColor",
                    new Color(0.12f, 0.12f, 0.12f, 1f));
            }
            material.globalIlluminationFlags =
                MaterialGlobalIlluminationFlags.RealtimeEmissive;
            material.EnableKeyword("_EMISSION");
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
        }

        private static Texture2D GetOrCreateCharacterHitEmissionMask()
        {
            Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(
                CharacterHitEmissionMaskPath);
            if (existing != null)
            {
                return existing;
            }

            var texture = new Texture2D(
                1,
                1,
                TextureFormat.RGBA32,
                false,
                true)
            {
                name = "T_CharacterHitEmissionBlack",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                anisoLevel = 0
            };
            texture.SetPixel(0, 0, Color.black);
            texture.Apply(false, false);
            AssetDatabase.CreateAsset(texture, CharacterHitEmissionMaskPath);
            return texture;
        }

        private static Texture2D[] LoadTextures(string textureFolder)
        {
            var textures = AssetDatabase.FindAssets(
                    "t:Texture2D",
                    new[] { textureFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Texture2D>)
                .Where(texture => texture != null)
                .ToList();
            Texture2D normalizedChr00 = AssetDatabase.LoadAssetAtPath<Texture2D>(
                textureFolder + "/texture_diffuse.png");
            if (normalizedChr00 != null && !textures.Contains(normalizedChr00))
            {
                textures.Add(normalizedChr00);
            }

            return textures.ToArray();
        }

        private static Texture2D FindTexture(Texture2D[] textures, params string[] tokens)
        {
            foreach (string token in tokens)
            {
                Texture2D match = textures.FirstOrDefault(texture =>
                    texture.name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static Texture2D GetOrCreatePackedMetallic(
            string key,
            Texture2D metallic,
            Texture2D roughness)
        {
            if (metallic == null || roughness == null ||
                metallic.width != roughness.width || metallic.height != roughness.height)
            {
                return null;
            }

            string legacyPath = TextureRoot + "/" + key + "_MetallicSmoothness.asset";
            string path = TextureRoot + "/" + key + "_MetallicSmoothness.png";
            var packed = new Texture2D(
                metallic.width,
                metallic.height,
                TextureFormat.RGBA32,
                false,
                true)
            {
                name = key + "_MetallicSmoothness"
            };

            Color32[] metal = metallic.GetPixels32();
            Color32[] rough = roughness.GetPixels32();
            var output = new Color32[metal.Length];
            for (int index = 0; index < output.Length; index++)
            {
                byte smoothness = (byte)(255 - rough[index].r);
                output[index] = new Color32(metal[index].r, 0, 0, smoothness);
            }

            packed.SetPixels32(output);
            packed.Apply(false, false);
            byte[] png = packed.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(packed);
            if (png == null || png.Length == 0)
            {
                throw new InvalidOperationException(
                    $"Could not encode the metallic-smoothness texture for {key}.");
            }

            if (AssetDatabase.LoadMainAssetAtPath(legacyPath) != null)
            {
                AssetDatabase.DeleteAsset(legacyPath);
            }

            File.WriteAllBytes(Path.GetFullPath(path), png);
            AssetDatabase.ImportAsset(
                path,
                ImportAssetOptions.ForceSynchronousImport |
                ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    $"Could not import the metallic-smoothness texture for {key}.");
            }

            importer.sRGBTexture = false;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.isReadable = false;
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void CreateControllers()
        {
            EnsureFolder(ControllerRoot);
            ConfigurePlayerMeleeSubclips();
            ConfigurePlayerStrapSubclips();
            ConfigureCombatAnimationEvents();
            CreatePlayerController();
            CreateAllyController();
            CreateEnemyController(false);
            CreateEnemyController(true);
            AssetDatabase.SaveAssets();
        }

        private static void CreateCombatControllers()
        {
            EnsureFolder(ControllerRoot);
            ConfigureLatestEnemyAnimations();
            ConfigurePlayerMeleeSubclips();
            ConfigurePlayerStrapSubclips();
            ConfigureCombatAnimationEvents();
            CreatePlayerController();
            CreateAllyController();
            CreateEnemyController(false);
            CreateEnemyController(true);
            AssetDatabase.SaveAssets();
        }

        private static bool ControllerContainsState(string path, string stateName)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            return controller != null && controller.layers
                .SelectMany(layer => layer.stateMachine.states)
                .Any(child => child.state.name == stateName);
        }

        private static void EnsureAllyRunState()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                Characters[1].ControllerPath);
            if (controller == null)
            {
                CreateAllyController();
                return;
            }

            if (!controller.parameters.Any(parameter =>
                    parameter.name == "Sprint" &&
                    parameter.type == AnimatorControllerParameterType.Bool))
            {
                controller.AddParameter("Sprint", AnimatorControllerParameterType.Bool);
            }

            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState run = machine.states
                .Select(child => child.state)
                .SingleOrDefault(state => state.name == "Run");
            if (run != null)
            {
                return;
            }

            AnimatorState walk = machine.states
                .Select(child => child.state)
                .Single(state => state.name == "Walk");
            run = AddState(machine, "Run", LoadClip(
                Chr01AnimationRoot + "/CHR01_Run_F.fbx"));
            run.speedParameterActive = true;
            run.speedParameter = "LocomotionPlayback";
            AddBoolTransition(walk, run, "Sprint", true);
            AddBoolTransition(run, walk, "Sprint", false);
            EditorUtility.SetDirty(controller);
        }

        private static void ConfigureCombatAnimationEvents()
        {
            (string path, string clipName, float openTime, float closeTime)[] attacks =
            {
                (Chr00AnimationRoot + "/Approved/CHR00_Melee_JabCross_raw.fbx",
                    "CHR00_Melee_Jab_Runtime",
                    ShowcaseChr00AnimationTiming.MeleeJabWindowOpen,
                    ShowcaseChr00AnimationTiming.MeleeJabWindowClose),
                (Chr00AnimationRoot + "/Approved/CHR00_Melee_JabCross_raw.fbx",
                    "CHR00_Melee_Cross_Runtime",
                    ShowcaseChr00AnimationTiming.MeleeCrossWindowOpen,
                    ShowcaseChr00AnimationTiming.MeleeCrossWindowClose),
                (Chr00AnimationRoot + "/Legacy/Hook Punch.fbx", null, 0.38f, 0.58f)
            };
            foreach ((string path, string clipName, float openTime, float closeTime) in attacks)
            {
                RemoveAnimationEvent(path, clipName, "ShowcaseCombatHit");
                ConfigureAnimationEvent(
                    path,
                    "ShowcaseAttackWindowOpen",
                    openTime,
                    clipName);
                ConfigureAnimationEvent(
                    path,
                    "ShowcaseAttackWindowClose",
                    closeTime,
                    clipName);
                float completeTime = clipName == "CHR00_Melee_Jab_Runtime"
                    ? ShowcaseChr00AnimationTiming.MeleeJabComplete
                    : clipName == "CHR00_Melee_Cross_Runtime"
                        ? ShowcaseChr00AnimationTiming.MeleeCrossComplete
                        : 0.88f;
                ConfigureAnimationEvent(
                    path,
                    "ShowcaseActionComplete",
                    completeTime,
                    clipName);
            }

            ConfigureAnimationEvent(
                Chr00AnimationRoot + "/Approved/CHR00_Melee_JabCross_raw.fbx",
                "ShowcaseActionComplete",
                ShowcaseChr00AnimationTiming.MeleeJabReturnComplete,
                "CHR00_Melee_Jab_Return_Runtime");

            foreach ((string path, string clipName, float openTime, float closeTime) in new[]
            {
                (Chr00AnimationRoot + "/Approved/CHR00_Melee_JabCross_raw.fbx",
                    "CHR00_Melee_Jab_Runtime",
                    ShowcaseChr00AnimationTiming.MeleeJabBufferOpen,
                    ShowcaseChr00AnimationTiming.MeleeJabBufferClose),
                (Chr00AnimationRoot + "/Approved/CHR00_Melee_JabCross_raw.fbx",
                    "CHR00_Melee_Cross_Runtime",
                    ShowcaseChr00AnimationTiming.MeleeCrossBufferOpen,
                    ShowcaseChr00AnimationTiming.MeleeCrossBufferClose)
            })
            {
                ConfigureAnimationEvent(path, "ShowcaseComboBufferOpen", openTime, clipName);
                ConfigureAnimationEvent(path, "ShowcaseComboBufferClose", closeTime, clipName);
            }

            foreach ((string path, float contact, float complete) in new[]
            {
                (Chr00AnimationRoot + "/Approved/CHR00_Pickup_Low_Small.fbx",
                    ShowcaseChr00AnimationTiming.PickupLowSmallContact,
                    ShowcaseChr00AnimationTiming.PickupLowSmallComplete),
                (Chr00AnimationRoot + "/Approved/CHR00_Pickup_Low_Medium.fbx",
                    ShowcaseChr00AnimationTiming.PickupLowMediumContact,
                    ShowcaseChr00AnimationTiming.PickupLowMediumComplete),
                (Chr00AnimationRoot + "/Approved/CHR00_Catch_Small_Center.fbx",
                    ShowcaseChr00AnimationTiming.CatchSmallCenterContact,
                    ShowcaseChr00AnimationTiming.CatchSmallCenterComplete),
                (Chr00AnimationRoot + "/Approved/CHR00_Catch_Small_Diving_L.fbx",
                    ShowcaseChr00AnimationTiming.CatchSmallDivingContact,
                    ShowcaseChr00AnimationTiming.CatchSmallDivingComplete),
                (Chr00AnimationRoot + "/Approved/CHR00_Catch_Small_Diving_R.fbx",
                    ShowcaseChr00AnimationTiming.CatchSmallDivingContact,
                    ShowcaseChr00AnimationTiming.CatchSmallDivingComplete),
                (Chr00AnimationRoot + "/Approved/CHR00_Catch_Medium.fbx",
                    ShowcaseChr00AnimationTiming.CatchMediumContact,
                    ShowcaseChr00AnimationTiming.CatchMediumComplete)
            })
            {
                ConfigureAnimationEvent(path, "ShowcaseCarryContact", contact);
                ConfigureAnimationEvent(path, "ShowcaseActionComplete", complete);
            }

            foreach ((string path, float releaseTime, float completeTime) in new[]
            {
                (Chr00AnimationRoot + "/Approved/CHR00_Throw_Small.fbx",
                    ShowcaseChr00AnimationTiming.ThrowSmallRelease,
                    ShowcaseChr00AnimationTiming.ThrowSmallComplete),
                (Chr00AnimationRoot + "/Approved/CHR00_Throw_Medium.fbx",
                    ShowcaseChr00AnimationTiming.ThrowMediumRelease,
                    ShowcaseChr00AnimationTiming.ThrowMediumComplete)
            })
            {
                ConfigureAnimationEvent(path, "ShowcaseThrowRelease", releaseTime);
                ConfigureAnimationEvent(path, "ShowcaseActionComplete", completeTime);
            }

            ConfigureAnimationEvent(
                Chr00AnimationRoot + "/Approved/CHR00_Throw_Medium.fbx",
                "ShowcaseChairSwingWhoosh",
                ShowcaseChr00AnimationTiming.ThrowMediumSwingWhoosh);

            foreach ((string path, float completeTime) in new[]
            {
                (Chr00AnimationRoot + "/Approved/CHR00_Melee_Hit_Body.fbx",
                    ShowcaseChr00AnimationTiming.HitBodyComplete),
                (Chr00AnimationRoot + "/Approved/CHR00_Knockdown_B.fbx",
                    ShowcaseChr00AnimationTiming.KnockdownBackComplete),
                (Chr00AnimationRoot + "/Approved/CHR00_Knockdown_F.fbx",
                    ShowcaseChr00AnimationTiming.KnockdownFrontComplete),
                (Chr00AnimationRoot + "/Approved/CHR00_Getup_B.fbx",
                    ShowcaseChr00AnimationTiming.GetupBackComplete),
                (Chr00AnimationRoot + "/Approved/CHR00_Getup_F.fbx",
                    ShowcaseChr00AnimationTiming.GetupFrontComplete)
            })
            {
                ConfigureAnimationEvent(path, "ShowcaseActionComplete", completeTime);
            }

            string strapRoot = Chr00AnimationRoot + "/Approved/CHR00_Strap_";
            ConfigureAnimationEvent(
                strapRoot + "Prepare.fbx",
                "ShowcaseStrapActionComplete",
                0.98f);

            ConfigureAnimationEvent(
                strapRoot + "Throw.fbx",
                "ShowcaseStrapThrowRelease",
                13f / 14f);
            ConfigureAnimationEvent(
                strapRoot + "Throw.fbx",
                "ShowcaseStrapAttachContact",
                0.985f);
            ConfigureAnimationEvent(
                strapRoot + "Throw.fbx",
                "ShowcaseStrapActionComplete",
                0.995f);

            string strapPull = strapRoot + "Pull.fbx";
            RemoveAnimationEvent(
                strapPull,
                "CHR00_Strap_Pull",
                "ShowcaseStrapActionComplete");
            ConfigureAnimationEvent(
                strapPull,
                "ShowcaseStrapPullComplete",
                0.98f,
                "CHR00_Strap_Pull");

            foreach ((string side, string clipName, float impulseTime) in new[]
            {
                ("Attack_L.fbx", "CHR00_Strap_Attack_L", 8f / 20f),
                ("Attack_R.fbx", "CHR00_Strap_Attack_R", 8f / 22f)
            })
            {
                ConfigureAnimationEvent(
                    strapRoot + side,
                    "ShowcaseStrapAttackImpulse",
                    impulseTime,
                    clipName);
                ConfigureAnimationEvent(
                    strapRoot + side,
                    "ShowcaseStrapActionComplete",
                    0.95f,
                    clipName);
            }

            ConfigureAnimationEvent(
                Chr00AnimationRoot + "/Approved/CHR00_Walk_F.fbx",
                "ShowcaseLeftFootstep",
                ShowcaseChr00AnimationTiming.WalkLeftFootstep);
            ConfigureAnimationEvent(
                Chr00AnimationRoot + "/Approved/CHR00_Walk_F.fbx",
                "ShowcaseRightFootstep",
                ShowcaseChr00AnimationTiming.WalkRightFootstep);
            ConfigureAnimationEvent(
                Chr00AnimationRoot + "/Approved/CHR00_Run_F.fbx",
                "ShowcaseLeftFootstep",
                ShowcaseChr00AnimationTiming.RunLeftFootstep);
            ConfigureAnimationEvent(
                Chr00AnimationRoot + "/Approved/CHR00_Run_F.fbx",
                "ShowcaseRightFootstep",
                ShowcaseChr00AnimationTiming.RunRightFootstep);
        }

        private static void ConfigurePlayerMeleeSubclips()
        {
            string path =
                Chr00AnimationRoot + "/Approved/CHR00_Melee_JabCross_raw.fbx";
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    $"CHR00 melee source importer is missing: {path}");
            }

            ModelImporterClipAnimation[] existing = importer.clipAnimations;
            ModelImporterClipAnimation source = importer.defaultClipAnimations
                .FirstOrDefault();
            if (source == null)
            {
                throw new InvalidOperationException(
                    $"CHR00 melee source has no default take: {path}");
            }

            ModelImporterClipAnimation jab = CreateRuntimeSubclip(
                "CHR00_Melee_Jab_Runtime",
                source,
                1f,
                14f,
                existing);
            ModelImporterClipAnimation jabReturn = CreateRuntimeSubclip(
                "CHR00_Melee_Jab_Return_Runtime",
                source,
                1f,
                14f,
                existing);
            ModelImporterClipAnimation cross = CreateRuntimeSubclip(
                "CHR00_Melee_Cross_Runtime",
                source,
                14f,
                60f,
                existing);
            ModelImporterClipAnimation[] desired = { jab, jabReturn, cross };

            bool same = existing != null && existing.Length == desired.Length;
            if (same)
            {
                for (int index = 0; index < desired.Length; index++)
                {
                    same &= existing[index].name == desired[index].name &&
                            Mathf.Approximately(existing[index].firstFrame,
                                desired[index].firstFrame) &&
                            Mathf.Approximately(existing[index].lastFrame,
                                desired[index].lastFrame);
                }
            }
            if (!same)
            {
                importer.clipAnimations = desired;
                importer.SaveAndReimport();
            }

            ConfigurePlayerStrapAttackClip(
                Chr00AnimationRoot + "/Approved/CHR00_Strap_Attack_L.fbx",
                "CHR00_Strap_Attack_L");
            ConfigurePlayerStrapAttackClip(
                Chr00AnimationRoot + "/Approved/CHR00_Strap_Attack_R.fbx",
                "CHR00_Strap_Attack_R");
        }

        private static void ConfigurePlayerStrapAttackClip(
            string path,
            string attackClipName)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    $"CHR00 strap attack source importer is missing: {path}");
            }

            ModelImporterClipAnimation[] existing = importer.clipAnimations;
            ModelImporterClipAnimation source = existing?
                .FirstOrDefault(candidate => candidate.name == attackClipName) ??
                importer.defaultClipAnimations.FirstOrDefault();
            if (source == null)
            {
                throw new InvalidOperationException(
                    $"CHR00 strap attack source has no take: {path}");
            }

            ModelImporterClipAnimation attack = CreateRuntimeSubclip(
                attackClipName,
                source,
                source.firstFrame,
                source.lastFrame,
                existing);
            ModelImporterClipAnimation[] desired = { attack };
            bool same = existing != null && existing.Length == desired.Length;
            if (same)
            {
                for (int index = 0; index < desired.Length; index++)
                {
                    same &= existing[index].name == desired[index].name &&
                            Mathf.Approximately(existing[index].firstFrame,
                                desired[index].firstFrame) &&
                            Mathf.Approximately(existing[index].lastFrame,
                                desired[index].lastFrame) &&
                            existing[index].loopTime == desired[index].loopTime;
                }
            }
            if (!same)
            {
                importer.clipAnimations = desired;
                importer.SaveAndReimport();
            }
        }

        private static void ConfigurePlayerStrapSubclips()
        {
            string path =
                Chr00AnimationRoot + "/Approved/CHR00_Strap_Pull.fbx";
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    $"CHR00 strap pull source importer is missing: {path}");
            }

            ModelImporterClipAnimation[] existing = importer.clipAnimations;
            ModelImporterClipAnimation source = importer.defaultClipAnimations
                .FirstOrDefault();
            if (source == null)
            {
                throw new InvalidOperationException(
                    $"CHR00 strap pull source has no default take: {path}");
            }

            ModelImporterClipAnimation pull = CreateRuntimeSubclip(
                "CHR00_Strap_Pull",
                source,
                source.firstFrame,
                source.lastFrame,
                existing);
            ModelImporterClipAnimation hold = CreateRuntimeSubclip(
                "CHR00_Strap_Tension_Hold_Runtime",
                source,
                source.lastFrame - 1f,
                source.lastFrame,
                existing,
                true);
            ModelImporterClipAnimation[] desired = { pull, hold };

            bool same = existing != null && existing.Length == desired.Length;
            if (same)
            {
                for (int index = 0; index < desired.Length; index++)
                {
                    same &= existing[index].name == desired[index].name &&
                            Mathf.Approximately(existing[index].firstFrame,
                                desired[index].firstFrame) &&
                            Mathf.Approximately(existing[index].lastFrame,
                                desired[index].lastFrame) &&
                            existing[index].loopTime == desired[index].loopTime;
                }
            }
            if (!same)
            {
                importer.clipAnimations = desired;
                importer.SaveAndReimport();
            }
        }

        private static ModelImporterClipAnimation CreateRuntimeSubclip(
            string name,
            ModelImporterClipAnimation source,
            float firstFrame,
            float lastFrame,
            ModelImporterClipAnimation[] existing,
            bool loop = false)
        {
            AnimationEvent[] preserved = existing?
                .FirstOrDefault(candidate => candidate.name == name)?.events ??
                Array.Empty<AnimationEvent>();
            return new ModelImporterClipAnimation
            {
                name = name,
                takeName = source.takeName,
                firstFrame = Mathf.Max(source.firstFrame, firstFrame),
                lastFrame = Mathf.Min(source.lastFrame, lastFrame),
                loopTime = loop,
                loopPose = loop,
                lockRootRotation = true,
                keepOriginalOrientation = false,
                lockRootHeightY = true,
                keepOriginalPositionY = false,
                lockRootPositionXZ = true,
                keepOriginalPositionXZ = false,
                heightFromFeet = true,
                events = preserved
            };
        }

        private static void ConfigureAnimationEvent(
            string assetPath,
            string functionName,
            float normalizedTime,
            string clipName = null)
        {
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    $"Combat animation importer is missing: {assetPath}");
            }

            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0)
            {
                clips = importer.defaultClipAnimations;
            }
            if (clips == null || clips.Length == 0)
            {
                throw new InvalidOperationException(
                    $"Owned animation events require a clip: {assetPath}");
            }
            int clipIndex = string.IsNullOrEmpty(clipName)
                ? clips.Length == 1 ? 0 : -1
                : Array.FindIndex(clips, candidate => candidate.name == clipName);
            if (clipIndex < 0)
            {
                throw new InvalidOperationException(
                    $"Owned animation event clip is ambiguous or missing: " +
                    $"{assetPath} / {clipName ?? "<default>"}");
            }
            AnimationClip runtimeClip = LoadClip(assetPath, clips[clipIndex].name);
            float sourceDuration = Mathf.Max(
                0f,
                clips[clipIndex].lastFrame - clips[clipIndex].firstFrame) /
                Mathf.Max(1f, runtimeClip.frameRate);
            float normalizedEventTime = Mathf.Clamp01(normalizedTime);
            float runtimeEventTime = sourceDuration * normalizedEventTime;
            AnimationEvent[] preserved =
                (clips[clipIndex].events ?? Array.Empty<AnimationEvent>())
                .Where(candidate => candidate.functionName != functionName)
                .ToArray();
            var desired = new AnimationEvent
            {
                functionName = functionName,
                // ModelImporterClipAnimation stores event time normalized (0..1).
                // AnimationUtility exposes the imported runtime event in seconds.
                time = normalizedEventTime
            };
            AnimationEvent[] reconciled = preserved
                .Append(desired)
                .OrderBy(candidate => candidate.time)
                .ThenBy(candidate => candidate.functionName, StringComparer.Ordinal)
                .ToArray();

            if (AnimationEventsEqual(clips[clipIndex].events, reconciled))
            {
                ValidateOwnedAnimationEvent(
                    assetPath,
                    clips[clipIndex].name,
                    functionName,
                    runtimeEventTime);
                return;
            }

            clips[clipIndex].events = reconciled;
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
            ValidateOwnedAnimationEvent(
                assetPath,
                clips[clipIndex].name,
                functionName,
                runtimeEventTime);
        }

        private static void RemoveAnimationEvent(
            string assetPath,
            string clipName,
            string functionName)
        {
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null)
            {
                return;
            }
            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0)
            {
                clips = importer.defaultClipAnimations;
            }
            bool changed = false;
            for (int index = 0; index < clips.Length; index++)
            {
                if (!string.IsNullOrEmpty(clipName) && clips[index].name != clipName)
                {
                    continue;
                }
                AnimationEvent[] events = clips[index].events ?? Array.Empty<AnimationEvent>();
                AnimationEvent[] filtered = events
                    .Where(candidate => candidate.functionName != functionName)
                    .ToArray();
                if (filtered.Length != events.Length)
                {
                    clips[index].events = filtered;
                    changed = true;
                }
            }
            if (changed)
            {
                importer.clipAnimations = clips;
                importer.SaveAndReimport();
            }
        }

        private static bool AnimationEventsEqual(
            AnimationEvent[] left,
            AnimationEvent[] right)
        {
            left ??= Array.Empty<AnimationEvent>();
            right ??= Array.Empty<AnimationEvent>();
            if (left.Length != right.Length)
            {
                return false;
            }

            for (int index = 0; index < left.Length; index++)
            {
                AnimationEvent a = left[index];
                AnimationEvent b = right[index];
                if (a.functionName != b.functionName ||
                    !Mathf.Approximately(a.time, b.time) ||
                    a.stringParameter != b.stringParameter ||
                    !Mathf.Approximately(a.floatParameter, b.floatParameter) ||
                    a.intParameter != b.intParameter ||
                    a.objectReferenceParameter != b.objectReferenceParameter ||
                    a.messageOptions != b.messageOptions)
                {
                    return false;
                }
            }

            return true;
        }

        private static void ValidateOwnedAnimationEvent(
            string assetPath,
            string clipName,
            string functionName,
            float expectedTime)
        {
            AnimationClip clip = LoadClip(assetPath, clipName);
            AnimationEvent[] matches = AnimationUtility.GetAnimationEvents(clip)
                .Where(candidate => candidate.functionName == functionName)
                .ToArray();
            float tolerance = Mathf.Max(0.001f, 1f / Mathf.Max(1f, clip.frameRate));
            if (matches.Length != 1 ||
                Mathf.Abs(matches[0].time - expectedTime) > tolerance ||
                matches[0].time > clip.length + tolerance)
            {
                throw new InvalidOperationException(
                    $"Owned animation event validation failed: {assetPath} / {functionName}");
            }
        }

        private static void CreatePlayerController()
        {
            RuntimeStateContract[] states =
            {
                new("Idle", Chr00AnimationRoot + "/Approved/CHR00_Idle.fbx", 1f, true),
                new("Walk", Chr00AnimationRoot + "/Approved/CHR00_Walk_F.fbx", 1f, true),
                new("Run", Chr00AnimationRoot + "/Approved/CHR00_Run_F.fbx", 1f, true),
                new("AimIdle", Chr00AnimationRoot + "/Approved/CHR00_Aim_Idle.fbx"),
                new("AimForward", Chr00AnimationRoot + "/Approved/CHR00_Walk_F.fbx", 1f, true),
                new("AimBackward", Chr00AnimationRoot + "/Approved/CHR00_Walk_F.fbx", 1f, true, null, 1f),
                new("AimStrafeLeft", Chr00AnimationRoot + "/Approved/CHR00_Strafe_L.fbx", 1f, true),
                new("AimStrafeRight", Chr00AnimationRoot + "/Approved/CHR00_Strafe_R.fbx", 1f, true),
                new("WalkStopAction", Chr00AnimationRoot + "/Approved/CHR00_Walk_Stop.fbx"),
                new("RunStopAction", Chr00AnimationRoot + "/Approved/CHR00_Run_Stop.fbx"),
                new("JumpAction", Chr00AnimationRoot + "/Approved/CHR00_Jump.fbx"),
                new("IdleVariantAction", Chr00AnimationRoot + "/Approved/CHR00_Idle_Variant_Stretch.fbx"),
                new("CatchLightAction", Chr00AnimationRoot + "/Approved/CHR00_Catch_Small_Center.fbx", 1.5f),
                new("CatchSmallLeftAction", Chr00AnimationRoot + "/Approved/CHR00_Catch_Small_Diving_L.fbx", 2.2f),
                new("CatchSmallRightAction", Chr00AnimationRoot + "/Approved/CHR00_Catch_Small_Diving_R.fbx", 2.2f),
                new("CatchMediumAction", Chr00AnimationRoot + "/Approved/CHR00_Catch_Medium.fbx", 1.2f),
                new("PickupLowSmallAction", Chr00AnimationRoot + "/Approved/CHR00_Pickup_Low_Small.fbx", ShowcaseChr00AnimationTiming.PickupLowSmallStateSpeed),
                new("PickupLowMediumAction", Chr00AnimationRoot + "/Approved/CHR00_Pickup_Low_Medium.fbx", ShowcaseChr00AnimationTiming.PickupLowMediumStateSpeed),
                new("ThrowLightAction", Chr00AnimationRoot + "/Approved/CHR00_Throw_Small.fbx", 1.6f),
                new("ThrowMediumAction", Chr00AnimationRoot + "/Approved/CHR00_Throw_Medium.fbx", 1.5f),
                new("StrapPrepareAction", Chr00AnimationRoot + "/Approved/CHR00_Strap_Prepare.fbx", 1.6f),
                new("StrapRollingAction", Chr00AnimationRoot + "/Approved/CHR00_Strap_Rolling.fbx"),
                new("StrapThrowAction", Chr00AnimationRoot + "/Approved/CHR00_Strap_Throw.fbx", 1.4f),
                new("StrapPullAction", Chr00AnimationRoot + "/Approved/CHR00_Strap_Pull.fbx", 1.4f, false, "CHR00_Strap_Pull"),
                new("StrapTensionHold", Chr00AnimationRoot + "/Approved/CHR00_Strap_Pull.fbx", 1f, false, "CHR00_Strap_Tension_Hold_Runtime"),
                new("StrapAttackLeftAction", Chr00AnimationRoot + "/Approved/CHR00_Strap_Attack_L.fbx", 0.65f, false, "CHR00_Strap_Attack_L"),
                new("StrapAttackRightAction", Chr00AnimationRoot + "/Approved/CHR00_Strap_Attack_R.fbx", 0.65f, false, "CHR00_Strap_Attack_R"),
                new("MeleeJabAction", Chr00AnimationRoot + "/Approved/CHR00_Melee_JabCross_raw.fbx", 1.25f, false, "CHR00_Melee_Jab_Runtime"),
                new("MeleeJabReturnAction", Chr00AnimationRoot + "/Approved/CHR00_Melee_JabCross_raw.fbx", -1.25f, false, "CHR00_Melee_Jab_Return_Runtime", 1f),
                new("MeleeCrossAction", Chr00AnimationRoot + "/Approved/CHR00_Melee_JabCross_raw.fbx", 1.25f, false, "CHR00_Melee_Cross_Runtime"),
                new("MeleeHookAction", Chr00AnimationRoot + "/Legacy/Hook Punch.fbx", ShowcaseChr00AnimationTiming.MeleeHookStateSpeed),
                new("HitBodyAction", Chr00AnimationRoot + "/Approved/CHR00_Melee_Hit_Body.fbx"),
                new("KnockdownBackAction", Chr00AnimationRoot + "/Approved/CHR00_Knockdown_B.fbx"),
                new("KnockdownFrontAction", Chr00AnimationRoot + "/Approved/CHR00_Knockdown_F.fbx"),
                new("GetupBackAction", Chr00AnimationRoot + "/Approved/CHR00_Getup_B.fbx"),
                new("GetupFrontAction", Chr00AnimationRoot + "/Approved/CHR00_Getup_F.fbx"),
                new("Death1Action", Chr00AnimationRoot + "/Legacy/Dying1.fbx"),
                new("Death2Action", Chr00AnimationRoot + "/Legacy/Dying2.fbx")
            };
            AvatarMask upperBodyMask = EnsurePlayerAimUpperBodyMask();
            AvatarMask meleeMovingLegsMask = EnsurePlayerMeleeMovingLegsMask();
            EnsureAnimatorLayerIkPass(Characters[0].ControllerPath);
            if (PlayerControllerMatchesRuntimeContract(
                    Characters[0].ControllerPath,
                    states,
                    upperBodyMask,
                    meleeMovingLegsMask))
            {
                return;
            }

            AnimatorController controller = CreateController(Characters[0].ControllerPath);
            RemoveAdditionalControllerLayers(controller);
            AddCoreParameters(controller, true, false);
            BuildRuntimeStates(controller.layers[0].stateMachine, states);
            EnsureAnimatorLayerIkPass(Characters[0].ControllerPath);
            AddPlayerAimUpperBodyLayer(controller, upperBodyMask);
            AddPlayerStrapUpperBodyLayer(controller, upperBodyMask);
            AddPlayerMeleeMovingLegsLayer(controller, meleeMovingLegsMask);
            EditorUtility.SetDirty(controller);
        }

        private static void CreateAllyController()
        {
            ConfigureLatestChr01Animations();
            RuntimeStateContract[] states =
            {
                new("Idle", Chr01AnimationRoot + "/CHR01_Idle.fbx", 1f, true),
                new("Walk", Chr01AnimationRoot + "/CHR01_Walk_F.fbx", 1f, true),
                new("Run", Chr01AnimationRoot + "/CHR01_Run_F.fbx", 1f, true),
                new(
                    "BinderWindupReady",
                    Chr01AnimationRoot + "/CHR01_Supply_Binder_Release.fbx",
                    1f,
                    false,
                    Chr01SupplyWindupRuntimeClipName),
                new(
                    "SupplyReleaseAction",
                    Chr01AnimationRoot + "/CHR01_Supply_Binder_Release.fbx",
                    ShowcaseNonPlayerAnimationTiming.AllySupplyReleaseStateSpeed,
                    false,
                    Chr01SupplyReleaseRuntimeClipName)
            };
            if (ControllerMatchesRuntimeContract(Characters[1].ControllerPath, states))
            {
                EnsureAnimatorLayerIkPass(Characters[1].ControllerPath);
                return;
            }

            AnimatorController controller = CreateController(Characters[1].ControllerPath);
            AddCoreParameters(controller, false, true);
            BuildRuntimeStates(controller.layers[0].stateMachine, states);
            EnsureAnimatorLayerIkPass(Characters[1].ControllerPath);
        }

        private static void CreateEnemyController(bool elite)
        {
            CharacterDefinition definition = Characters[elite ? 3 : 2];
            var states = new List<RuntimeStateContract>
            {
                new("Idle", EnemyClipPath(elite, "Idle"), 1f, true),
                new("Run", EnemyClipPath(elite, "Standard_Run"), 1f, true),
                new("HitLightAction", EnemyClipPath(elite, "Hit_Head")),
                new("HitBodyAction", EnemyClipPath(elite, "Hit_Body")),
                new("HitRibAction", EnemyClipPath(elite, "Hit_Rib")),
                new("HitStomachAction", EnemyClipPath(elite, "Hit_Stomach")),
                new("HitBigHeadAction", EnemyClipPath(elite, "Hit_BigHeadHit")),
                new(
                    "AttackBoxingAction",
                    EnemyClipPath(elite, "Melee_Candidate_Boxing"),
                    ShowcaseNonPlayerAnimationTiming.BoxingStateSpeed),
                new(
                    "AttackKneeStrikeAction",
                    EnemyClipPath(elite, "Melee_Candidate_Boxing_2"),
                    ShowcaseNonPlayerAnimationTiming.Boxing2StateSpeed),
                new(
                    "AttackKickingAction",
                    EnemyClipPath(elite, "Melee_Candidate_Kicking"),
                    ShowcaseNonPlayerAnimationTiming.KickingStateSpeed),
                new(
                    "AttackKicking2Action",
                    EnemyClipPath(elite, "Melee_Candidate_Kicking_2"),
                    ShowcaseNonPlayerAnimationTiming.Kicking2StateSpeed),
                new("KnockdownBackAction", Chr00AnimationRoot + "/Approved/CHR00_Knockdown_B.fbx"),
                new("KnockdownFrontAction", Chr00AnimationRoot + "/Approved/CHR00_Knockdown_F.fbx"),
                new("GetupBackAction", Chr00AnimationRoot + "/Approved/CHR00_Getup_B.fbx"),
                new("GetupFrontAction", Chr00AnimationRoot + "/Approved/CHR00_Getup_F.fbx"),
                new("Death1Action", Chr00AnimationRoot + "/Approved/CHR00_Knockdown_F.fbx"),
                new("Death2Action", Chr00AnimationRoot + "/Approved/CHR00_Knockdown_B.fbx")
            };
            if (elite)
            {
                states.Add(new RuntimeStateContract(
                    "EliteMutantPunchAction",
                    EnemyClipPath(true, "Lod_Mutant_Punch"),
                    ShowcaseNonPlayerAnimationTiming.MutantPunchStateSpeed));
                states.Add(new RuntimeStateContract(
                    "EliteMutantSwipingAction",
                    EnemyClipPath(true, "Lod_Mutant_Swiping"),
                    ShowcaseNonPlayerAnimationTiming.MutantSwipeStateSpeed));
            }
            if (ControllerMatchesRuntimeContract(definition.ControllerPath, states))
            {
                return;
            }

            AnimatorController controller = CreateController(definition.ControllerPath);
            AddCoreParameters(controller, false, false);
            BuildRuntimeStates(controller.layers[0].stateMachine, states);
        }

        private static string EnemyClipPath(bool elite, string suffix)
        {
            string variant = elite ? "Elite" : "Normal";
            string root = elite ? EnyEliteAnimationRoot : EnyNormalAnimationRoot;
            return $"{root}/ENY01_{variant}_{suffix}.fbx";
        }

        private static void BuildRuntimeStates(
            AnimatorStateMachine machine,
            IEnumerable<RuntimeStateContract> contracts)
        {
            AnimatorState idle = null;
            foreach (RuntimeStateContract contract in contracts)
            {
                AnimatorState state = AddRuntimeState(
                    machine,
                    contract.Name,
                    LoadContractClip(contract),
                    contract.PlaybackSpeed,
                    contract.LocomotionDriven,
                    contract.CycleOffset);
                if (contract.Name == "Idle")
                {
                    idle = state;
                }
            }
            machine.defaultState = idle ?? throw new InvalidOperationException(
                "Runtime Animator contract must contain an Idle state.");
        }

        private static void EnsureAnimatorLayerIkPass(string controllerPath)
        {
            AnimatorController controller =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null || controller.layers.Length == 0 ||
                controller.layers[0].iKPass)
            {
                return;
            }

            AnimatorControllerLayer[] layers = controller.layers;
            layers[0].iKPass = true;
            controller.layers = layers;
            EditorUtility.SetDirty(controller);
        }

        private static AvatarMask EnsurePlayerAimUpperBodyMask()
        {
            AvatarMask mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(
                PlayerAimUpperBodyMaskPath);
            bool created = mask == null;
            if (created)
            {
                mask = new AvatarMask
                {
                    name = Path.GetFileNameWithoutExtension(PlayerAimUpperBodyMaskPath)
                };
                AssetDatabase.CreateAsset(mask, PlayerAimUpperBodyMaskPath);
            }

            bool changed = created;
            for (int index = 0; index < (int)AvatarMaskBodyPart.LastBodyPart; index++)
            {
                AvatarMaskBodyPart bodyPart = (AvatarMaskBodyPart)index;
                bool expected = IsPlayerAimUpperBodyPart(bodyPart);
                if (mask.GetHumanoidBodyPartActive(bodyPart) == expected)
                {
                    continue;
                }

                mask.SetHumanoidBodyPartActive(bodyPart, expected);
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(mask);
            }
            return mask;
        }

        private static bool IsPlayerAimUpperBodyPart(AvatarMaskBodyPart bodyPart)
        {
            switch (bodyPart)
            {
                case AvatarMaskBodyPart.Body:
                case AvatarMaskBodyPart.Head:
                case AvatarMaskBodyPart.LeftArm:
                case AvatarMaskBodyPart.RightArm:
                case AvatarMaskBodyPart.LeftFingers:
                case AvatarMaskBodyPart.RightFingers:
                case AvatarMaskBodyPart.LeftHandIK:
                case AvatarMaskBodyPart.RightHandIK:
                    return true;
                default:
                    return false;
            }
        }

        private static AvatarMask EnsurePlayerMeleeMovingLegsMask()
        {
            AvatarMask mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(
                PlayerMeleeMovingLegsMaskPath);
            bool created = mask == null;
            if (created)
            {
                mask = new AvatarMask
                {
                    name = Path.GetFileNameWithoutExtension(
                        PlayerMeleeMovingLegsMaskPath)
                };
                AssetDatabase.CreateAsset(mask, PlayerMeleeMovingLegsMaskPath);
            }

            bool changed = created;
            for (int index = 0;
                 index < (int)AvatarMaskBodyPart.LastBodyPart;
                 index++)
            {
                AvatarMaskBodyPart bodyPart = (AvatarMaskBodyPart)index;
                bool expected = bodyPart == AvatarMaskBodyPart.LeftLeg ||
                    bodyPart == AvatarMaskBodyPart.RightLeg;
                if (mask.GetHumanoidBodyPartActive(bodyPart) == expected)
                {
                    continue;
                }
                mask.SetHumanoidBodyPartActive(bodyPart, expected);
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(mask);
            }
            return mask;
        }

        private static void AddPlayerAimUpperBodyLayer(
            AnimatorController controller,
            AvatarMask upperBodyMask)
        {
            controller.AddLayer(PlayerAimUpperBodyLayerName);
            AnimatorControllerLayer[] layers = controller.layers;
            int layerIndex = layers.Length - 1;
            AnimatorControllerLayer layer = layers[layerIndex];
            layer.avatarMask = upperBodyMask;
            layer.blendingMode = AnimatorLayerBlendingMode.Override;
            layer.defaultWeight = 0f;
            layer.iKPass = false;
            layer.syncedLayerIndex = -1;
            layer.syncedLayerAffectsTiming = false;
            layers[layerIndex] = layer;
            controller.layers = layers;

            AnimatorStateMachine stateMachine = layer.stateMachine;
            AnimatorState state = AddRuntimeState(
                stateMachine,
                PlayerAimUpperBodyStateName,
                LoadClip(Chr00AnimationRoot + "/Approved/CHR00_Aim_Idle.fbx"));
            stateMachine.defaultState = state;
        }

        private static void AddPlayerStrapUpperBodyLayer(
            AnimatorController controller,
            AvatarMask upperBodyMask)
        {
            controller.AddLayer(PlayerStrapUpperBodyLayerName);
            AnimatorControllerLayer[] layers = controller.layers;
            int layerIndex = layers.Length - 1;
            AnimatorControllerLayer layer = layers[layerIndex];
            layer.avatarMask = upperBodyMask;
            layer.blendingMode = AnimatorLayerBlendingMode.Override;
            layer.defaultWeight = 0f;
            layer.iKPass = false;
            layer.syncedLayerIndex = -1;
            layer.syncedLayerAffectsTiming = false;
            layers[layerIndex] = layer;
            controller.layers = layers;

            AnimatorStateMachine stateMachine = layer.stateMachine;
            AnimatorState rolling = AddRuntimeState(
                stateMachine,
                PlayerStrapRollingUpperBodyStateName,
                LoadClip(Chr00AnimationRoot + "/Approved/CHR00_Strap_Rolling.fbx"));
            AddRuntimeState(
                stateMachine,
                PlayerStrapTensionUpperBodyStateName,
                LoadClip(
                    Chr00AnimationRoot + "/Approved/CHR00_Strap_Pull.fbx",
                    "CHR00_Strap_Tension_Hold_Runtime"));
            AddRuntimeState(
                stateMachine,
                PlayerStrapAttackLeftUpperBodyStateName,
                LoadClip(
                    Chr00AnimationRoot + "/Approved/CHR00_Strap_Attack_L.fbx",
                    "CHR00_Strap_Attack_L"),
                0.65f);
            AddRuntimeState(
                stateMachine,
                PlayerStrapAttackRightUpperBodyStateName,
                LoadClip(
                    Chr00AnimationRoot + "/Approved/CHR00_Strap_Attack_R.fbx",
                    "CHR00_Strap_Attack_R"),
                0.65f);
            stateMachine.defaultState = rolling;
        }

        private static void AddPlayerMeleeMovingLegsLayer(
            AnimatorController controller,
            AvatarMask movingLegsMask)
        {
            controller.AddLayer(PlayerMeleeMovingLegsLayerName);
            AnimatorControllerLayer[] layers = controller.layers;
            int layerIndex = layers.Length - 1;
            AnimatorControllerLayer layer = layers[layerIndex];
            layer.avatarMask = movingLegsMask;
            layer.blendingMode = AnimatorLayerBlendingMode.Override;
            layer.defaultWeight = 0f;
            layer.iKPass = false;
            layer.syncedLayerIndex = -1;
            layer.syncedLayerAffectsTiming = false;
            layers[layerIndex] = layer;
            controller.layers = layers;

            AnimatorStateMachine stateMachine = layer.stateMachine;
            AnimatorState idle = AddRuntimeState(
                stateMachine,
                PlayerMeleeLegsIdleStateName,
                LoadClip(Chr00AnimationRoot + "/Approved/CHR00_Idle.fbx"),
                1f,
                true);
            AddRuntimeState(
                stateMachine,
                PlayerMeleeLegsForwardStateName,
                LoadClip(Chr00AnimationRoot + "/Approved/CHR00_Walk_F.fbx"),
                1f,
                true);
            AddRuntimeState(
                stateMachine,
                PlayerMeleeLegsBackwardStateName,
                LoadClip(Chr00AnimationRoot + "/Approved/CHR00_Walk_F.fbx"),
                1f,
                true,
                1f);
            AddRuntimeState(
                stateMachine,
                PlayerMeleeLegsLeftStateName,
                LoadClip(Chr00AnimationRoot + "/Approved/CHR00_Strafe_L.fbx"),
                1f,
                true);
            AddRuntimeState(
                stateMachine,
                PlayerMeleeLegsRightStateName,
                LoadClip(Chr00AnimationRoot + "/Approved/CHR00_Strafe_R.fbx"),
                1f,
                true);
            AddRuntimeState(
                stateMachine,
                PlayerMeleeLegsRunStateName,
                LoadClip(Chr00AnimationRoot + "/Approved/CHR00_Run_F.fbx"),
                1f,
                true);
            stateMachine.defaultState = idle;
        }

        private static void RemoveAdditionalControllerLayers(
            AnimatorController controller)
        {
            while (controller.layers.Length > 1)
            {
                controller.RemoveLayer(controller.layers.Length - 1);
            }
        }

        private static bool PlayerControllerMatchesRuntimeContract(
            string path,
            IEnumerable<RuntimeStateContract> authoredContracts,
            AvatarMask upperBodyMask,
            AvatarMask meleeMovingLegsMask)
        {
            RuntimeStateContract[] contracts = authoredContracts.ToArray();
            if (contracts.Any(contract =>
                    IsForbiddenPlayerRuntimeClip(contract.AssetPath)))
            {
                throw new InvalidOperationException(
                    "CHR00 runtime controller cannot reference PREVIEW, Turn, ALT, " +
                    "REF, or candidate clips.");
            }

            AnimatorController controller =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null || controller.layers.Length != 4 ||
                !controller.layers[0].iKPass ||
                !ControllerHasRuntimeParameters(controller) ||
                !RuntimeStateMachineMatchesContract(
                    controller.layers[0].stateMachine,
                    contracts))
            {
                return false;
            }

            AnimatorControllerLayer upperLayer = controller.layers[1];
            AnimatorStateMachine upperMachine = upperLayer.stateMachine;
            if (upperLayer.name != PlayerAimUpperBodyLayerName ||
                upperLayer.avatarMask != upperBodyMask ||
                upperLayer.blendingMode != AnimatorLayerBlendingMode.Override ||
                Mathf.Abs(upperLayer.defaultWeight) > 0.0001f ||
                upperLayer.iKPass || upperLayer.syncedLayerIndex != -1 ||
                upperLayer.syncedLayerAffectsTiming ||
                upperMachine == null || upperMachine.stateMachines.Length != 0 ||
                upperMachine.anyStateTransitions.Length != 0 ||
                upperMachine.states.Length != 1 ||
                upperMachine.defaultState == null ||
                upperMachine.defaultState.name != PlayerAimUpperBodyStateName)
            {
                return false;
            }

            AnimatorState upperState = upperMachine.states[0].state;
            if (upperState.name != PlayerAimUpperBodyStateName ||
                upperState.motion != LoadClip(
                    Chr00AnimationRoot + "/Approved/CHR00_Aim_Idle.fbx") ||
                Mathf.Abs(upperState.speed - 1f) > 0.0001f ||
                Mathf.Abs(upperState.cycleOffset) > 0.0001f ||
                upperState.speedParameterActive || upperState.iKOnFeet ||
                !upperState.writeDefaultValues || upperState.transitions.Length != 0)
            {
                return false;
            }

            AnimatorControllerLayer strapLayer = controller.layers[2];
            AnimatorStateMachine strapMachine = strapLayer.stateMachine;
            if (strapLayer.name != PlayerStrapUpperBodyLayerName ||
                strapLayer.avatarMask != upperBodyMask ||
                strapLayer.blendingMode != AnimatorLayerBlendingMode.Override ||
                Mathf.Abs(strapLayer.defaultWeight) > 0.0001f ||
                strapLayer.iKPass || strapLayer.syncedLayerIndex != -1 ||
                strapLayer.syncedLayerAffectsTiming || strapMachine == null ||
                strapMachine.stateMachines.Length != 0 ||
                strapMachine.anyStateTransitions.Length != 0 ||
                strapMachine.states.Length != 4 ||
                strapMachine.defaultState == null ||
                strapMachine.defaultState.name !=
                    PlayerStrapRollingUpperBodyStateName)
            {
                return false;
            }

            Dictionary<string, AnimatorState> strapStates = strapMachine.states
                .Select(child => child.state)
                .ToDictionary(state => state.name, StringComparer.Ordinal);
            bool strapLayerMatches = RuntimeUpperBodyStateMatches(
                       strapStates,
                       PlayerStrapRollingUpperBodyStateName,
                       LoadClip(Chr00AnimationRoot +
                           "/Approved/CHR00_Strap_Rolling.fbx")) &&
                   RuntimeUpperBodyStateMatches(
                       strapStates,
                       PlayerStrapTensionUpperBodyStateName,
                       LoadClip(
                           Chr00AnimationRoot + "/Approved/CHR00_Strap_Pull.fbx",
                           "CHR00_Strap_Tension_Hold_Runtime")) &&
                   RuntimeUpperBodyStateMatches(
                       strapStates,
                       PlayerStrapAttackLeftUpperBodyStateName,
                       LoadClip(
                           Chr00AnimationRoot +
                               "/Approved/CHR00_Strap_Attack_L.fbx",
                           "CHR00_Strap_Attack_L"),
                       0.65f) &&
                   RuntimeUpperBodyStateMatches(
                       strapStates,
                       PlayerStrapAttackRightUpperBodyStateName,
                        LoadClip(
                            Chr00AnimationRoot +
                                "/Approved/CHR00_Strap_Attack_R.fbx",
                            "CHR00_Strap_Attack_R"),
                        0.65f);
            if (!strapLayerMatches)
            {
                return false;
            }

            AnimatorControllerLayer meleeLayer = controller.layers[3];
            AnimatorStateMachine meleeMachine = meleeLayer.stateMachine;
            if (meleeLayer.name != PlayerMeleeMovingLegsLayerName ||
                meleeLayer.avatarMask != meleeMovingLegsMask ||
                meleeLayer.blendingMode != AnimatorLayerBlendingMode.Override ||
                Mathf.Abs(meleeLayer.defaultWeight) > 0.0001f ||
                meleeLayer.iKPass || meleeLayer.syncedLayerIndex != -1 ||
                meleeLayer.syncedLayerAffectsTiming || meleeMachine == null ||
                meleeMachine.stateMachines.Length != 0 ||
                meleeMachine.anyStateTransitions.Length != 0 ||
                meleeMachine.states.Length != 6 ||
                meleeMachine.defaultState == null ||
                meleeMachine.defaultState.name != PlayerMeleeLegsIdleStateName)
            {
                return false;
            }

            Dictionary<string, AnimatorState> meleeStates = meleeMachine.states
                .Select(child => child.state)
                .ToDictionary(state => state.name, StringComparer.Ordinal);
            return RuntimeUpperBodyStateMatches(
                       meleeStates,
                       PlayerMeleeLegsIdleStateName,
                       LoadClip(Chr00AnimationRoot + "/Approved/CHR00_Idle.fbx"),
                       1f,
                       0f,
                       true) &&
                   RuntimeUpperBodyStateMatches(
                       meleeStates,
                       PlayerMeleeLegsForwardStateName,
                       LoadClip(Chr00AnimationRoot + "/Approved/CHR00_Walk_F.fbx"),
                       1f,
                       0f,
                       true) &&
                   RuntimeUpperBodyStateMatches(
                       meleeStates,
                       PlayerMeleeLegsBackwardStateName,
                       LoadClip(Chr00AnimationRoot + "/Approved/CHR00_Walk_F.fbx"),
                       1f,
                       1f,
                       true) &&
                   RuntimeUpperBodyStateMatches(
                       meleeStates,
                       PlayerMeleeLegsLeftStateName,
                       LoadClip(Chr00AnimationRoot + "/Approved/CHR00_Strafe_L.fbx"),
                       1f,
                       0f,
                       true) &&
                   RuntimeUpperBodyStateMatches(
                       meleeStates,
                       PlayerMeleeLegsRightStateName,
                       LoadClip(Chr00AnimationRoot + "/Approved/CHR00_Strafe_R.fbx"),
                       1f,
                       0f,
                       true) &&
                   RuntimeUpperBodyStateMatches(
                       meleeStates,
                       PlayerMeleeLegsRunStateName,
                       LoadClip(Chr00AnimationRoot + "/Approved/CHR00_Run_F.fbx"),
                       1f,
                       0f,
                       true);
        }

        private static bool RuntimeUpperBodyStateMatches(
            IReadOnlyDictionary<string, AnimatorState> states,
            string stateName,
            AnimationClip motion,
            float playbackSpeed = 1f,
            float cycleOffset = 0f,
            bool locomotionDriven = false)
        {
            return states.TryGetValue(stateName, out AnimatorState state) &&
                   state.motion == motion &&
                   Mathf.Abs(state.speed - playbackSpeed) <= 0.0001f &&
                   Mathf.Abs(state.cycleOffset - cycleOffset) <= 0.0001f &&
                   state.speedParameterActive == locomotionDriven &&
                   (!locomotionDriven ||
                    state.speedParameter == "LocomotionPlayback") &&
                   !state.iKOnFeet &&
                   state.writeDefaultValues && state.transitions.Length == 0;
        }

        private static bool IsForbiddenPlayerRuntimeClip(string assetPath)
        {
            string clipName = Path.GetFileNameWithoutExtension(assetPath);
            return clipName.IndexOf("PREVIEW", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   clipName.IndexOf("Turn", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   clipName.IndexOf("ALT", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   clipName.IndexOf("_REF", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   clipName.IndexOf("candidate", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool ControllerMatchesRuntimeContract(
            string path,
            IEnumerable<RuntimeStateContract> authoredContracts)
        {
            AnimatorController controller =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            RuntimeStateContract[] contracts = authoredContracts.ToArray();
            if (controller == null || controller.layers.Length != 1 ||
                !ControllerHasRuntimeParameters(controller))
            {
                return false;
            }

            return RuntimeStateMachineMatchesContract(
                controller.layers[0].stateMachine,
                contracts);
        }

        private static bool ControllerHasRuntimeParameters(
            AnimatorController controller)
        {
            return controller.parameters.Length == 2 &&
                   controller.parameters.All(parameter =>
                       parameter.type == AnimatorControllerParameterType.Float &&
                       (parameter.name == "Speed" ||
                        parameter.name == "LocomotionPlayback"));
        }

        private static bool RuntimeStateMachineMatchesContract(
            AnimatorStateMachine machine,
            RuntimeStateContract[] contracts)
        {
            ChildAnimatorState[] children = machine.states;
            if (machine.defaultState == null || machine.defaultState.name != "Idle" ||
                children.Length != contracts.Length ||
                machine.stateMachines.Length != 0 ||
                machine.anyStateTransitions.Length != 0)
            {
                return false;
            }

            foreach (RuntimeStateContract contract in contracts)
            {
                AnimatorState state = children
                    .Select(child => child.state)
                    .SingleOrDefault(candidate => candidate.name == contract.Name);
                if (state == null || state.motion != LoadContractClip(contract) ||
                    Mathf.Abs(state.speed - ResolveRuntimePlaybackSpeed(
                        contract.PlaybackSpeed)) >
                    0.0001f ||
                    Mathf.Abs(state.cycleOffset - contract.CycleOffset) > 0.0001f ||
                    state.iKOnFeet || !state.writeDefaultValues ||
                    state.speedParameterActive != contract.LocomotionDriven ||
                    (contract.LocomotionDriven &&
                     state.speedParameter != "LocomotionPlayback") ||
                    state.transitions.Length != 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static AnimationClip LoadContractClip(RuntimeStateContract contract)
        {
            return string.IsNullOrEmpty(contract.ClipName)
                ? LoadClip(contract.AssetPath)
                : LoadClip(contract.AssetPath, contract.ClipName);
        }

        private static AnimatorController CreateController(string path)
        {
            AnimatorController existing =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (existing != null)
            {
                existing.parameters = Array.Empty<AnimatorControllerParameter>();
                AnimatorStateMachine machine = existing.layers[0].stateMachine;
                foreach (ChildAnimatorState child in machine.states.ToArray())
                {
                    machine.RemoveState(child.state);
                }
                foreach (ChildAnimatorStateMachine child in machine.stateMachines.ToArray())
                {
                    machine.RemoveStateMachine(child.stateMachine);
                }
                foreach (AnimatorStateTransition transition in
                         machine.anyStateTransitions.ToArray())
                {
                    machine.RemoveAnyStateTransition(transition);
                }
                return existing;
            }

            AnimatorController controller =
                AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.name = Path.GetFileNameWithoutExtension(path);
            return controller;
        }

        private static void AddCoreParameters(
            AnimatorController controller,
            bool player,
            bool ally)
        {
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter(
                "LocomotionPlayback",
                AnimatorControllerParameterType.Float);
        }

        private static AnimatorState AddState(
            AnimatorStateMachine machine,
            string name,
            Motion motion)
        {
            AnimatorState state = machine.AddState(name);
            state.motion = motion;
            state.writeDefaultValues = true;
            return state;
        }

        private static AnimatorState AddRuntimeState(
            AnimatorStateMachine machine,
            string name,
            AnimationClip clip,
            float playbackSpeed = 1f,
            bool locomotionDriven = false,
            float cycleOffset = 0f)
        {
            AnimatorState state = AddState(machine, name, clip);
            state.speed = ResolveRuntimePlaybackSpeed(playbackSpeed);
            state.cycleOffset = cycleOffset;
            state.iKOnFeet = false;
            if (locomotionDriven)
            {
                state.speedParameterActive = true;
                state.speedParameter = "LocomotionPlayback";
            }
            return state;
        }

        private static float ResolveRuntimePlaybackSpeed(float playbackSpeed)
        {
            if (Mathf.Abs(playbackSpeed) >= 0.05f)
            {
                return playbackSpeed;
            }

            return playbackSpeed < 0f ? -0.05f : 0.05f;
        }

        private static void ConfigureLocomotion(
            AnimatorStateMachine machine,
            AnimatorState idle,
            AnimatorState walk)
        {
            machine.defaultState = idle;
            RemoveSpeedTransitions(idle);
            RemoveSpeedTransitions(walk);
            AddFloatTransition(idle, walk, AnimatorConditionMode.Greater, 0.05f);
            AddFloatTransition(walk, idle, AnimatorConditionMode.Less, 0.05f);
            ConfigureLocomotionQuality(idle, walk);
        }

        private static void ConfigureLocomotionQuality(
            AnimatorState idle,
            AnimatorState walk)
        {
            idle.iKOnFeet = false;
            walk.iKOnFeet = false;
            walk.speedParameterActive = true;
            walk.speedParameter = "LocomotionPlayback";

            foreach (AnimatorStateTransition transition in new[] { idle, walk }
                         .SelectMany(state => state.transitions)
                         .Where(candidate => candidate.conditions.Any(condition =>
                             condition.parameter == "Speed")))
            {
                transition.hasExitTime = false;
                transition.duration = 0.08f;
            }
        }

        private static void RemovePrimaryRunState(AnimatorStateMachine machine)
        {
            AnimatorState run = machine.states
                .Select(child => child.state)
                .SingleOrDefault(state => state.name == "Run");
            if (run == null)
            {
                return;
            }

            foreach (AnimatorState state in machine.states.Select(child => child.state))
            {
                foreach (AnimatorStateTransition transition in state.transitions
                             .Where(candidate => candidate.destinationState == run)
                             .ToArray())
                {
                    state.RemoveTransition(transition);
                }
            }

            foreach (AnimatorStateTransition transition in machine.anyStateTransitions
                         .Where(candidate => candidate.destinationState == run)
                         .ToArray())
            {
                machine.RemoveAnyStateTransition(transition);
            }

            machine.RemoveState(run);
        }

        private static void RemoveSpeedTransitions(AnimatorState state)
        {
            foreach (AnimatorStateTransition transition in state.transitions
                         .Where(candidate => candidate.conditions.Any(condition =>
                             condition.parameter == "Speed"))
                         .ToArray())
            {
                state.RemoveTransition(transition);
            }
        }

        private static void AddFloatTransition(
            AnimatorState source,
            AnimatorState destination,
            AnimatorConditionMode mode,
            float threshold)
        {
            AnimatorStateTransition transition = source.AddTransition(destination);
            transition.hasExitTime = false;
            transition.duration = 0.08f;
            transition.AddCondition(mode, threshold, "Speed");
        }

        private static void AddBoolTransition(
            AnimatorState source,
            AnimatorState destination,
            string parameter,
            bool expected)
        {
            AnimatorStateTransition transition = source.AddTransition(destination);
            transition.hasExitTime = false;
            transition.duration = 0.08f;
            transition.AddCondition(
                expected ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                0f,
                parameter);
        }

        private static void EnsureLocomotionParameter(AnimatorController controller)
        {
            if (!controller.parameters.Any(parameter =>
                    parameter.name == "LocomotionPlayback" &&
                    parameter.type == AnimatorControllerParameterType.Float))
            {
                controller.AddParameter(
                    "LocomotionPlayback",
                    AnimatorControllerParameterType.Float);
            }
        }

        private static void AddBoolAnyState(
            AnimatorStateMachine machine,
            AnimatorState destination,
            string parameter)
        {
            AnimatorStateTransition transition = machine.AddAnyStateTransition(destination);
            transition.hasExitTime = false;
            transition.duration = 0.1f;
            transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.If, 0f, parameter);
        }

        private static void AddBoolExit(
            AnimatorState source,
            AnimatorState destination,
            string parameter)
        {
            AnimatorStateTransition transition = source.AddTransition(destination);
            transition.hasExitTime = false;
            transition.duration = 0.1f;
            transition.AddCondition(AnimatorConditionMode.IfNot, 0f, parameter);
        }

        private static AnimatorState AddOneShot(
            AnimatorStateMachine machine,
            AnimatorState idle,
            string trigger,
            AnimationClip clip,
            bool returnToIdle = true,
            float playbackSpeed = 1f)
        {
            AnimatorState state = AddState(machine, trigger + "Action", clip);
            state.speed = Mathf.Max(0.05f, playbackSpeed);
            AnimatorStateTransition enter = machine.AddAnyStateTransition(state);
            enter.hasExitTime = false;
            enter.duration = 0.08f;
            enter.canTransitionToSelf = false;
            enter.AddCondition(AnimatorConditionMode.If, 0f, trigger);
            if (returnToIdle)
            {
                AnimatorStateTransition exit = state.AddTransition(idle);
                exit.hasExitTime = true;
                exit.exitTime = 0.9f;
                exit.duration = 0.1f;
            }
            return state;
        }

        private static void AddCatalogStates(
            AnimatorStateMachine machine,
            IEnumerable<string> folders,
            string prefix)
        {
            var existingNames = new HashSet<string>(
                machine.states.Select(child => child.state.name),
                StringComparer.Ordinal);
            int index = 0;
            foreach (string path in folders.SelectMany(GetAnimationPaths)
                         .OrderBy(path => path, StringComparer.Ordinal))
            {
                string relative = path.Substring(AnimationRoot.Length + 1);
                string stateName = prefix + "__" +
                    Path.ChangeExtension(relative, null)
                        .Replace('/', '_')
                        .Replace('\\', '_');
                if (!existingNames.Add(stateName))
                {
                    continue;
                }

                AnimatorState state = machine.AddState(
                    stateName,
                    new Vector3(520f + (index % 4) * 220f, (index / 4) * 42f, 0f));
                state.motion = LoadClip(path);
                state.writeDefaultValues = true;
                index++;
            }
        }

        private static string[] GetAnimationPaths(string folder)
        {
            return AssetDatabase.FindAssets("t:Model", new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
        }

        private static AnimationClip LoadClip(string assetPath)
        {
            AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<AnimationClip>()
                .FirstOrDefault(candidate => !candidate.name.StartsWith(
                    "__preview__",
                    StringComparison.Ordinal));
            if (clip == null)
            {
                throw new InvalidOperationException(
                    $"Animation clip could not be loaded: {assetPath}");
            }

            return clip;
        }

        private static AnimationClip LoadClip(string assetPath, string clipName)
        {
            AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<AnimationClip>()
                .FirstOrDefault(candidate => candidate.name == clipName);
            if (clip == null)
            {
                throw new InvalidOperationException(
                    $"Animation clip could not be loaded: {assetPath} / {clipName}");
            }
            return clip;
        }

        private static void IntegrateCharacter(
            Scene scene,
            CharacterDefinition definition)
        {
            GameObject actor = FindRootAny(scene, definition.PrimaryRoot, definition.LegacyRoot);
            Transform previous = actor.transform.Find(RuntimeVisualName);
            if (previous != null)
            {
                UnityEngine.Object.DestroyImmediate(previous.gameObject);
            }

            foreach (Renderer renderer in actor.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = false;
            }

            var visualRoot = new GameObject(RuntimeVisualName);
            visualRoot.transform.SetParent(actor.transform, false);
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
                definition.ModelPath);
            GameObject instance = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException(
                    $"Could not instantiate {definition.ModelPath}.");
            }

            instance.name = definition.Key + "_MODEL";
            instance.transform.SetParent(visualRoot.transform, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            NormalizeVisual(actor.transform, instance, definition.Role);

            Material material = GetOrCreateCharacterMaterial(definition);
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                int slots = Math.Max(1, renderer.sharedMaterials.Length);
                renderer.sharedMaterials = Enumerable.Repeat(material, slots).ToArray();
                renderer.enabled = true;
            }

            Animator animator = instance.GetComponent<Animator>() ??
                instance.GetComponentInChildren<Animator>(true) ??
                instance.AddComponent<Animator>();
            animator.runtimeAnimatorController =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                    definition.ControllerPath);
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            animator.stabilizeFeet = false;

            ShowcaseCharacterAnimationDriver driver =
                actor.GetComponent<ShowcaseCharacterAnimationDriver>() ??
                actor.AddComponent<ShowcaseCharacterAnimationDriver>();
            driver.Configure(
                definition.Role,
                animator,
                actor.GetComponent<CharacterController>(),
                actor.GetComponent<InputSystemLogicalInputAdapter>(),
                actor.GetComponent<PrototypePhysicalObjectLifecycle>(),
                actor.GetComponent<PrototypeAllyRelayController>());
            ShowcaseBoneCombatRig boneRig =
                actor.GetComponent<ShowcaseBoneCombatRig>() ??
                actor.AddComponent<ShowcaseBoneCombatRig>();
            ShowcaseCombatTeam team = definition.Role switch
            {
                ShowcaseCharacterAnimationRole.Player => ShowcaseCombatTeam.Player,
                ShowcaseCharacterAnimationRole.Ally => ShowcaseCombatTeam.Ally,
                _ => ShowcaseCombatTeam.Enemy
            };
            boneRig.Configure(
                animator,
                team,
                actor.GetComponent<ShowcasePlayerCombatController>(),
                actor.GetComponent<ShowcaseRunnerAgent>());

            EditorUtility.SetDirty(actor);
            EditorUtility.SetDirty(instance);
            EditorUtility.SetDirty(animator);
            EditorUtility.SetDirty(driver);
            EditorUtility.SetDirty(boneRig);
        }

        private static void NormalizeVisual(
            Transform actor,
            GameObject instance,
            ShowcaseCharacterAnimationRole role)
        {
            Bounds bounds = CalculateBounds(instance);
            if (bounds.size.y <= 0.001f)
            {
                throw new InvalidOperationException(
                    $"No visible model bounds were found for {instance.name}.");
            }

            instance.transform.localScale *= 1.8f / bounds.size.y;
            bounds = CalculateBounds(instance);
            float supportY = CalculateSupportPlaneY(actor);
            instance.transform.position += new Vector3(
                actor.position.x - bounds.center.x,
                supportY - bounds.min.y,
                actor.position.z - bounds.center.z);

            if (role == ShowcaseCharacterAnimationRole.EnemyNormal ||
                role == ShowcaseCharacterAnimationRole.EnemyElite)
            {
                // Enemy collision and attack reach are owned by the actor root.
                // Mesh-bounds centering put the Normal skeleton 12 cm behind that
                // root because its bind silhouette is asymmetric. Keep grounding,
                // but align the FBX/Humanoid root on XZ for both variants.
                Vector3 localPosition = instance.transform.localPosition;
                localPosition.x = 0f;
                localPosition.z = 0f;
                instance.transform.localPosition = localPosition;
            }
        }

        private static float CalculateSupportPlaneY(Transform actor)
        {
            Physics.SyncTransforms();
            RaycastHit groundHit = Physics.RaycastAll(
                    actor.position + Vector3.up * 2f,
                    Vector3.down,
                    8f,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore)
                .Where(hit => !hit.collider.transform.IsChildOf(actor))
                .OrderBy(hit => hit.distance)
                .FirstOrDefault();
            if (groundHit.collider != null)
            {
                return groundHit.point.y;
            }

            Collider[] supportColliders = actor.GetComponents<Collider>()
                .Where(collider => collider.enabled && !collider.isTrigger)
                .ToArray();
            if (supportColliders.Length == 0)
            {
                return actor.position.y;
            }

            return supportColliders.Min(collider => collider.bounds.min.y);
        }

        private static Bounds CalculateBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(root.transform.position, Vector3.zero);
            }

            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return bounds;
        }

        private static void ValidateScene(Scene scene)
        {
            ShowcaseCharacterAnimationDriver[] drivers = scene.GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<ShowcaseCharacterAnimationDriver>(true))
                .ToArray();
            if (drivers.Length != 4 ||
                drivers.Select(driver => driver.Role).Distinct().Count() != 4)
            {
                throw new InvalidOperationException(
                    "Exactly four unique Designer A character animation roles are required.");
            }

            foreach (ShowcaseCharacterAnimationDriver driver in drivers)
            {
                if (!driver.IsRuntimeConfigured ||
                    driver.transform.Find(RuntimeVisualName) == null ||
                    driver.Animator.avatar == null ||
                    !driver.Animator.avatar.isHuman ||
                    driver.Animator.GetComponentsInChildren<Renderer>(true).Length == 0)
                {
                    throw new InvalidOperationException(
                        $"Designer A runtime visual is incomplete: {driver.Role}");
                }
            }

            AnimatorController normal = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                Characters[2].ControllerPath);
            AnimatorController elite = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                Characters[3].ControllerPath);
            string[] forbiddenEnemyStates =
            {
                "Walk",
                "AttackBoxing2Action",
                "EliteJumpAttackAction",
                "EliteMutantWalkingAction",
                "EliteClubComboAction",
                "EliteStabbingAction"
            };
            if (!ControllerHasState(normal, "Run") ||
                !ControllerHasState(elite, "Run") ||
                ControllerHasState(normal, "EliteMutantPunchAction") ||
                ControllerHasState(normal, "EliteMutantSwipingAction") ||
                !ControllerHasState(elite, "EliteMutantPunchAction") ||
                !ControllerHasState(elite, "EliteMutantSwipingAction") ||
                forbiddenEnemyStates.Any(name => ControllerHasState(normal, name) ||
                                                  ControllerHasState(elite, name)))
            {
                throw new InvalidOperationException(
                    "ENY01 runtime eligibility mismatch: Elite Punch/Swipe must be " +
                    "Elite-only while weapon, prop and review sources stay unreferenced.");
            }

            ShowcaseBoneCombatRig[] combatRigs = scene.GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<ShowcaseBoneCombatRig>(true))
                .ToArray();
            if (combatRigs.Length != 4 ||
                combatRigs.Any(rig => rig.Animator == null ||
                                      !rig.Animator.isHuman))
            {
                throw new InvalidOperationException(
                    "Exactly four humanoid bone combat rigs are required.");
            }
        }

        private static bool ControllerHasState(
            AnimatorController controller,
            string stateName)
        {
            return controller != null && controller.layers
                .SelectMany(layer => layer.stateMachine.states)
                .Any(child => child.state.name == stateName);
        }

        private static int CountStatesWithPrefix(
            AnimatorController controller,
            string prefix)
        {
            return controller.layers
                .SelectMany(layer => layer.stateMachine.states)
                .Count(child => child.state.name.StartsWith(prefix, StringComparison.Ordinal));
        }

        private static GameObject FindRootAny(
            Scene scene,
            string primary,
            string legacy)
        {
            GameObject root = scene.GetRootGameObjects().FirstOrDefault(candidate =>
                candidate.name == primary || candidate.name == legacy);
            if (root == null)
            {
                throw new InvalidOperationException(
                    $"Required actor root was not found: {primary} / {legacy}");
            }

            return root;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            int separator = path.LastIndexOf('/');
            if (separator <= 0)
            {
                throw new InvalidOperationException($"Invalid asset folder: {path}");
            }

            string parent = path.Substring(0, separator);
            string leaf = path.Substring(separator + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        private readonly struct CharacterDefinition
        {
            public CharacterDefinition(
                string key,
                ShowcaseCharacterAnimationRole role,
                string modelPath,
                string primaryRoot,
                string legacyRoot,
                string controllerPath)
            {
                Key = key;
                Role = role;
                ModelPath = modelPath;
                PrimaryRoot = primaryRoot;
                LegacyRoot = legacyRoot;
                ControllerPath = controllerPath;
            }

            public string Key { get; }
            public ShowcaseCharacterAnimationRole Role { get; }
            public string ModelPath { get; }
            public string PrimaryRoot { get; }
            public string LegacyRoot { get; }
            public string ControllerPath { get; }
        }
    }
}
