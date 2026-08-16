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

        private const string CharacterRoot = "Assets/_Project/Art/Characters";
        private const string AnimationRoot = "Assets/_Project/Art/Animation";
        private const string TextureRoot =
            "Assets/_Project/Art/Textures/Characters/DesignerA";
        private const string MaterialRoot =
            "Assets/_Project/Art/Materials/Characters/DesignerA";

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
            AnimationRoot + "/CHR01/DesignerA";
        private const string EnyAnimationRoot =
            AnimationRoot + "/ENY01/DesignerA";

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
            CreatePlayerController();
            AssetDatabase.SaveAssets();
            Debug.Log(
                "CHR00 PLAYER ANIMATION CONTROLLER PASS: controller-only authoring complete; " +
                "the current Scene was not opened or modified.");
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
            ConfigureAnimationReceiver(playerDriver.Animator, playerCombat, null, null);

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
                    elite ? 0.12f : 0.2f);
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
            PrototypeAllyRelayController allyRelay)
        {
            if (animator == null)
            {
                throw new InvalidOperationException("Runtime Animator is required.");
            }
            ShowcaseAnimationEventReceiver receiver =
                animator.GetComponent<ShowcaseAnimationEventReceiver>() ??
                animator.gameObject.AddComponent<ShowcaseAnimationEventReceiver>();
            receiver.Configure(playerCombat, runner, allyRelay);
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
                NormalizeVisual(actor.transform, model.gameObject);
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
            ConfigureAnimationFolder(Chr01AnimationRoot);
            ConfigureAnimationFolder(EnyAnimationRoot);

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

                bool changed =
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

                ModelImporterClipAnimation[] clips = importer.clipAnimations;
                if (clips == null || clips.Length == 0)
                {
                    clips = importer.defaultClipAnimations;
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
            return name.Contains("idle") ||
                   name.Contains("walking") ||
                   name.Contains("walk_f") ||
                   name.Contains("running") ||
                   name.Contains("run_f") ||
                   name.Contains("standard run") ||
                   name.Contains("strafe") ||
                   name.Contains("turn") ||
                   name.Contains("tension_loop") ||
                   name.Contains("windup");
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
                material.SetColor("_EmissionColor", Color.white);
                material.EnableKeyword("_EMISSION");
            }

            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
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
            ConfigurePlayerMeleeSubclips();
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
                Chr01AnimationRoot + "/SharedLocomotion/CHR00_Run_F.fbx"));
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
                (Chr00AnimationRoot + "/Legacy/Fist Fight A.fbx",
                    "CHR00_Melee_Jab_Runtime", 0.42f, 0.82f),
                (Chr00AnimationRoot + "/Legacy/Fist Fight A.fbx",
                    "CHR00_Melee_Cross_Runtime", 0.42f, 0.68f),
                (Chr00AnimationRoot + "/Legacy/Hook Punch.fbx", null, 0.38f, 0.58f),
                (EnyAnimationRoot + "/MeleeCandidates/Melee_candidate_Boxing.fbx", null, 0.36f, 0.56f),
                (EnyAnimationRoot + "/MeleeCandidates/Melee_candidate_Boxing_2.fbx", null, 0.36f, 0.58f),
                (EnyAnimationRoot + "/MeleeCandidates/Melee_candidate_Kicking.fbx", null, 0.48f, 0.68f),
                (EnyAnimationRoot + "/MeleeCandidates/Melee_candidate_Kicking_2.fbx", null, 0.34f, 0.52f),
                (EnyAnimationRoot + "/EliteOnly/lod@Mutant Punch.fbx", null, 0.34f, 0.54f),
                (EnyAnimationRoot + "/EliteOnly/lod@Mutant Swiping.fbx", null, 0.42f, 0.64f),
                (EnyAnimationRoot + "/EliteOnly/lod@Jump Attack.fbx", null, 0.40f, 0.60f)
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
                ConfigureAnimationEvent(
                    path,
                    "ShowcaseActionComplete",
                    0.88f,
                    clipName);
            }

            foreach ((string path, string clipName, float openTime, float closeTime) in new[]
            {
                (Chr00AnimationRoot + "/Legacy/Fist Fight A.fbx",
                    "CHR00_Melee_Jab_Runtime", 0.52f, 0.78f),
                (Chr00AnimationRoot + "/Legacy/Fist Fight A.fbx",
                    "CHR00_Melee_Cross_Runtime", 0.50f, 0.76f)
            })
            {
                ConfigureAnimationEvent(path, "ShowcaseComboBufferOpen", openTime, clipName);
                ConfigureAnimationEvent(path, "ShowcaseComboBufferClose", closeTime, clipName);
            }

            string eliteJump = EnyAnimationRoot + "/EliteOnly/lod@Jump Attack.fbx";
            ConfigureAnimationEvent(eliteJump, "ShowcaseLungeStart", 0.24f);
            ConfigureAnimationEvent(eliteJump, "ShowcaseLungeEnd", 0.62f);

            string[] catches =
            {
                Chr00AnimationRoot + "/Approved/CHR00_Catch_Small_Center.fbx",
                Chr00AnimationRoot + "/Approved/CHR00_Catch_Small_Diving_L.fbx",
                Chr00AnimationRoot + "/Approved/CHR00_Catch_Small_Diving_R.fbx",
                Chr00AnimationRoot + "/Approved/CHR00_Catch_Medium.fbx"
            };
            foreach (string path in catches)
            {
                ConfigureAnimationEvent(path, "ShowcaseCarryContact", 0.52f);
                ConfigureAnimationEvent(path, "ShowcaseActionComplete", 0.88f);
            }

            foreach ((string path, float releaseTime) in new[]
            {
                (Chr00AnimationRoot + "/Approved/CHR00_Throw_Small.fbx", 0.48f),
                (Chr00AnimationRoot + "/Approved/CHR00_Throw_Medium.fbx", 0.52f)
            })
            {
                ConfigureAnimationEvent(path, "ShowcaseThrowRelease", releaseTime);
                ConfigureAnimationEvent(path, "ShowcaseActionComplete", 0.88f);
            }

            foreach (string path in new[]
            {
                Chr00AnimationRoot + "/Approved/CHR00_Melee_Hit_Body.fbx",
                Chr00AnimationRoot + "/Approved/CHR00_Knockdown_B.fbx",
                Chr00AnimationRoot + "/Approved/CHR00_Knockdown_F.fbx",
                Chr00AnimationRoot + "/Approved/CHR00_Getup_B.fbx",
                Chr00AnimationRoot + "/Approved/CHR00_Getup_F.fbx",
                EnyAnimationRoot + "/Shared/hit reaction.fbx",
                EnyAnimationRoot + "/ApprovedHit/Hit_Body.fbx",
                EnyAnimationRoot + "/ApprovedHit/Hit_Rib.fbx",
                EnyAnimationRoot + "/ApprovedHit/Hit_Stomach.fbx",
                EnyAnimationRoot + "/ApprovedHit/Hit_BigHeadHit.fbx"
            })
            {
                ConfigureAnimationEvent(path, "ShowcaseActionComplete", 0.88f);
            }

            ConfigureAnimationEvent(
                Chr01AnimationRoot + "/Approved/CHR01_Supply_Binder_Release.fbx",
                "ShowcaseAllySupplyRelease",
                0.48f);
            ConfigureAnimationEvent(
                Chr01AnimationRoot + "/Approved/CHR01_Supply_Binder_Release.fbx",
                "ShowcaseActionComplete",
                0.88f);
        }

        private static void ConfigurePlayerMeleeSubclips()
        {
            string path = Chr00AnimationRoot + "/Legacy/Fist Fight A.fbx";
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
                0f,
                36f,
                existing);
            ModelImporterClipAnimation cross = CreateRuntimeSubclip(
                "CHR00_Melee_Cross_Runtime",
                source,
                37f,
                70f,
                existing);
            ModelImporterClipAnimation[] desired = { jab, cross };

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
        }

        private static ModelImporterClipAnimation CreateRuntimeSubclip(
            string name,
            ModelImporterClipAnimation source,
            float firstFrame,
            float lastFrame,
            ModelImporterClipAnimation[] existing)
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
                loopTime = false,
                loopPose = false,
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
                new("ThrowLightAction", Chr00AnimationRoot + "/Approved/CHR00_Throw_Small.fbx", 1.6f),
                new("ThrowMediumAction", Chr00AnimationRoot + "/Approved/CHR00_Throw_Medium.fbx", 1.5f),
                new("MeleeJabAction", Chr00AnimationRoot + "/Legacy/Fist Fight A.fbx", 1.25f, false, "CHR00_Melee_Jab_Runtime"),
                new("MeleeCrossAction", Chr00AnimationRoot + "/Legacy/Fist Fight A.fbx", 1.25f, false, "CHR00_Melee_Cross_Runtime"),
                new("MeleeHookAction", Chr00AnimationRoot + "/Legacy/Hook Punch.fbx", 1.4f),
                new("HitBodyAction", Chr00AnimationRoot + "/Approved/CHR00_Melee_Hit_Body.fbx"),
                new("KnockdownBackAction", Chr00AnimationRoot + "/Approved/CHR00_Knockdown_B.fbx"),
                new("KnockdownFrontAction", Chr00AnimationRoot + "/Approved/CHR00_Knockdown_F.fbx"),
                new("GetupBackAction", Chr00AnimationRoot + "/Approved/CHR00_Getup_B.fbx"),
                new("GetupFrontAction", Chr00AnimationRoot + "/Approved/CHR00_Getup_F.fbx"),
                new("Death1Action", Chr00AnimationRoot + "/Legacy/Dying1.fbx"),
                new("Death2Action", Chr00AnimationRoot + "/Legacy/Dying2.fbx")
            };
            AvatarMask upperBodyMask = EnsurePlayerAimUpperBodyMask();
            EnsureAnimatorLayerIkPass(Characters[0].ControllerPath);
            if (PlayerControllerMatchesRuntimeContract(
                    Characters[0].ControllerPath,
                    states,
                    upperBodyMask))
            {
                return;
            }

            AnimatorController controller = CreateController(Characters[0].ControllerPath);
            RemoveAdditionalControllerLayers(controller);
            AddCoreParameters(controller, true, false);
            BuildRuntimeStates(controller.layers[0].stateMachine, states);
            EnsureAnimatorLayerIkPass(Characters[0].ControllerPath);
            AddPlayerAimUpperBodyLayer(controller, upperBodyMask);
            EditorUtility.SetDirty(controller);
        }

        private static void CreateAllyController()
        {
            RuntimeStateContract[] states =
            {
                new("Idle", Chr01AnimationRoot + "/SharedLocomotion/CHR00_Idle.fbx", 1f, true),
                new("Walk", Chr01AnimationRoot + "/SharedLocomotion/CHR00_Walk_F.fbx", 1f, true),
                new("Run", Chr01AnimationRoot + "/SharedLocomotion/CHR00_Run_F.fbx", 1f, true),
                new("AcquireAction", Chr00AnimationRoot + "/Approved/CHR00_Catch_Small_Center.fbx", 1.5f),
                new("SupplyReleaseAction", Chr01AnimationRoot + "/Approved/CHR01_Supply_Binder_Release.fbx")
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
                new("Idle", EnyAnimationRoot + "/Shared/idle.fbx", 1f, true),
                new("Walk", EnyAnimationRoot + "/Shared/walking.fbx", 1f, true),
                new("HitLightAction", EnyAnimationRoot + "/Shared/hit reaction.fbx"),
                new("HitBodyAction", EnyAnimationRoot + "/ApprovedHit/Hit_Body.fbx"),
                new("HitRibAction", EnyAnimationRoot + "/ApprovedHit/Hit_Rib.fbx"),
                new("HitStomachAction", EnyAnimationRoot + "/ApprovedHit/Hit_Stomach.fbx"),
                new("HitBigHeadAction", EnyAnimationRoot + "/ApprovedHit/Hit_BigHeadHit.fbx"),
                new("AttackBoxingAction", EnyAnimationRoot + "/MeleeCandidates/Melee_candidate_Boxing.fbx", 1.25f),
                new("AttackBoxing2Action", EnyAnimationRoot + "/MeleeCandidates/Melee_candidate_Boxing_2.fbx", 1.3f),
                new("AttackKickingAction", EnyAnimationRoot + "/MeleeCandidates/Melee_candidate_Kicking.fbx", 1.2f),
                new("AttackKicking2Action", EnyAnimationRoot + "/MeleeCandidates/Melee_candidate_Kicking_2.fbx", 1.2f),
                new("KnockdownBackAction", Chr00AnimationRoot + "/Approved/CHR00_Knockdown_B.fbx"),
                new("KnockdownFrontAction", Chr00AnimationRoot + "/Approved/CHR00_Knockdown_F.fbx"),
                new("GetupBackAction", Chr00AnimationRoot + "/Approved/CHR00_Getup_B.fbx"),
                new("GetupFrontAction", Chr00AnimationRoot + "/Approved/CHR00_Getup_F.fbx"),
                new("Death1Action", Chr00AnimationRoot + "/Legacy/Dying1.fbx"),
                new("Death2Action", Chr00AnimationRoot + "/Legacy/Dying2.fbx")
            };
            if (elite)
            {
                states.Add(new RuntimeStateContract(
                    "EliteMutantPunchAction",
                    EnyAnimationRoot + "/EliteOnly/lod@Mutant Punch.fbx",
                    1f));
                states.Add(new RuntimeStateContract(
                    "EliteMutantSwipingAction",
                    EnyAnimationRoot + "/EliteOnly/lod@Mutant Swiping.fbx",
                    1f));
                states.Add(new RuntimeStateContract(
                    "EliteJumpAttackAction",
                    EnyAnimationRoot + "/EliteOnly/lod@Jump Attack.fbx",
                    1f));
            }
            if (ControllerMatchesRuntimeContract(definition.ControllerPath, states))
            {
                return;
            }

            AnimatorController controller = CreateController(definition.ControllerPath);
            AddCoreParameters(controller, false, false);
            BuildRuntimeStates(controller.layers[0].stateMachine, states);
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
            AvatarMask upperBodyMask)
        {
            RuntimeStateContract[] contracts = authoredContracts.ToArray();
            if (contracts.Any(contract =>
                    IsForbiddenPlayerRuntimeClip(contract.AssetPath)))
            {
                throw new InvalidOperationException(
                    "CHR00 runtime controller cannot reference PREVIEW, Turn, or ALT clips.");
            }

            AnimatorController controller =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null || controller.layers.Length != 2 ||
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
            return upperState.name == PlayerAimUpperBodyStateName &&
                   upperState.motion == LoadClip(
                       Chr00AnimationRoot + "/Approved/CHR00_Aim_Idle.fbx") &&
                   Mathf.Abs(upperState.speed - 1f) <= 0.0001f &&
                   Mathf.Abs(upperState.cycleOffset) <= 0.0001f &&
                   !upperState.speedParameterActive &&
                   !upperState.iKOnFeet && upperState.writeDefaultValues &&
                   upperState.transitions.Length == 0;
        }

        private static bool IsForbiddenPlayerRuntimeClip(string assetPath)
        {
            string clipName = Path.GetFileNameWithoutExtension(assetPath);
            return clipName.IndexOf("PREVIEW", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   clipName.IndexOf("Turn", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   clipName.IndexOf("ALT", StringComparison.OrdinalIgnoreCase) >= 0;
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
            NormalizeVisual(actor.transform, instance);

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

        private static void NormalizeVisual(Transform actor, GameObject instance)
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
            string[] selectedEliteStates =
            {
                "EliteMutantPunchAction",
                "EliteMutantSwipingAction",
                "EliteJumpAttackAction"
            };
            string[] reviewOnlyEliteStates =
            {
                "EliteMutantWalkingAction",
                "EliteClubComboAction",
                "EliteStabbingAction"
            };
            if (selectedEliteStates.Any(name => ControllerHasState(normal, name)) ||
                selectedEliteStates.Any(name => !ControllerHasState(elite, name)) ||
                reviewOnlyEliteStates.Any(name => ControllerHasState(normal, name) ||
                                                   ControllerHasState(elite, name)))
            {
                throw new InvalidOperationException(
                    "ENY01 runtime eligibility mismatch: Normal must exclude all " +
                    "Elite-only attacks; Elite must include only the three selected " +
                    "unarmed attacks. Review-only sources must remain unreferenced.");
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
