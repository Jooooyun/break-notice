using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BreakNotice.Gameplay;
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
        public const string RuntimeVisualName = "RUNTIME_DESIGNER_A_VISUAL";
        public const string ControllerRoot =
            "Assets/_Project/Art/Animation/DesignerAControllers";

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

        public static void ValidateCurrentIntegration()
        {
            Scene scene = EditorSceneManager.OpenScene(
                ContestShowcaseSceneAuthoring.ShowcaseS3ScenePath,
                OpenSceneMode.Single);
            ValidateScene(scene);
            Debug.Log("DESIGNER A RUNTIME INTEGRATION VALIDATION PASS.");
        }

        [MenuItem("BREAK NOTICE/Contest/Repair Designer A Walk-Only Locomotion")]
        public static void RepairRuntimeFacingAndLocomotion()
        {
            foreach (CharacterDefinition definition in Characters)
            {
                AnimatorController controller =
                    AssetDatabase.LoadAssetAtPath<AnimatorController>(
                        definition.ControllerPath);
                if (controller == null)
                {
                    throw new InvalidOperationException(
                        $"Designer A controller is missing: {definition.ControllerPath}");
                }

                EnsureLocomotionParameter(controller);
                AnimatorStateMachine machine = controller.layers[0].stateMachine;
                RemovePrimaryRunState(machine);
                AnimatorState[] states = machine.states
                    .Select(child => child.state)
                    .ToArray();
                ConfigureLocomotion(
                    machine,
                    states.Single(state => state.name == "Idle"),
                    states.Single(state => state.name == "Walk"));
                EditorUtility.SetDirty(controller);
            }

            AssetDatabase.SaveAssets();
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
                "DESIGNER A WALK-ONLY LOCOMOTION REPAIR PASS: +Z forward, " +
                "Idle-Walk runtime path, physical support-plane grounding and bounded natural " +
                "walk cadence applied to all four roles; Run and Foot IK are inactive.");
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

                importer.importAnimation = true;
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.sourceAvatar = null;
                importer.importCameras = false;
                importer.importLights = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.SaveAndReimport();

                ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
                foreach (ModelImporterClipAnimation clip in clips)
                {
                    clip.loopTime = IsLoopingClip(path);
                    clip.loopPose = clip.loopTime;
                    clip.lockRootRotation = true;
                    clip.keepOriginalOrientation = false;
                    clip.lockRootHeightY = true;
                    clip.keepOriginalPositionY = false;
                    clip.lockRootPositionXZ = true;
                    clip.keepOriginalPositionXZ = false;
                    clip.heightFromFeet = true;
                }

                importer.clipAnimations = clips;
                importer.SaveAndReimport();
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
            CreatePlayerController();
            CreateAllyController();
            CreateEnemyController(false);
            CreateEnemyController(true);
            AssetDatabase.SaveAssets();
        }

        private static void CreatePlayerController()
        {
            AnimatorController controller = CreateController(Characters[0].ControllerPath);
            AddCoreParameters(controller, true, false);
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState idle = AddState(machine, "Idle", LoadClip(
                Chr00AnimationRoot + "/Approved/CHR00_Idle.fbx"));
            AnimatorState walk = AddState(machine, "Walk", LoadClip(
                Chr00AnimationRoot + "/Approved/CHR00_Walk_F.fbx"));
            ConfigureLocomotion(machine, idle, walk);

            AnimatorState aim = AddState(machine, "AimIdle", LoadClip(
                Chr00AnimationRoot + "/Approved/CHR00_Aim_Idle.fbx"));
            AddBoolAnyState(machine, aim, "Aim");
            AddBoolExit(aim, idle, "Aim");
            AddOneShot(machine, idle, "CatchLight", LoadClip(
                Chr00AnimationRoot + "/Approved/CHR00_Catch_Small_Center.fbx"));
            AddOneShot(machine, idle, "CatchMedium", LoadClip(
                Chr00AnimationRoot + "/Approved/CHR00_Catch_Medium.fbx"));
            AddOneShot(machine, idle, "ThrowLight", LoadClip(
                Chr00AnimationRoot + "/Approved/CHR00_Throw_Small.fbx"));
            AddOneShot(machine, idle, "ThrowMedium", LoadClip(
                Chr00AnimationRoot + "/Approved/CHR00_Throw_Medium.fbx"));
            AddCatalogStates(machine, new[]
            {
                Chr00AnimationRoot + "/Approved",
                Chr00AnimationRoot + "/Legacy"
            }, "CHR00");
        }

        private static void CreateAllyController()
        {
            AnimatorController controller = CreateController(Characters[1].ControllerPath);
            AddCoreParameters(controller, false, true);
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState idle = AddState(machine, "Idle", LoadClip(
                Chr01AnimationRoot + "/SharedLocomotion/CHR00_Idle.fbx"));
            AnimatorState walk = AddState(machine, "Walk", LoadClip(
                Chr01AnimationRoot + "/SharedLocomotion/CHR00_Walk_F.fbx"));
            ConfigureLocomotion(machine, idle, walk);

            AnimatorState ready = AddState(machine, "SupplyReady", LoadClip(
                Chr01AnimationRoot + "/Approved/CHR01_Supply_Binder_Windup.fbx"));
            AddBoolAnyState(machine, ready, "SupplyReady");
            AddBoolExit(ready, idle, "SupplyReady");
            AddOneShot(machine, idle, "SupplyRelease", LoadClip(
                Chr01AnimationRoot + "/Approved/CHR01_Supply_Binder_Release.fbx"));
            AddCatalogStates(machine, new[]
            {
                Chr01AnimationRoot + "/SharedLocomotion",
                Chr01AnimationRoot + "/Approved",
                Chr01AnimationRoot + "/Legacy"
            }, "CHR01");
        }

        private static void CreateEnemyController(bool elite)
        {
            CharacterDefinition definition = Characters[elite ? 3 : 2];
            AnimatorController controller = CreateController(definition.ControllerPath);
            AddCoreParameters(controller, false, false);
            controller.AddParameter("HitLight", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("MeleePrimary", AnimatorControllerParameterType.Trigger);
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState idle = AddState(machine, "Idle", LoadClip(
                EnyAnimationRoot + "/Shared/idle.fbx"));
            AnimatorState walk = AddState(machine, "Walk", LoadClip(
                EnyAnimationRoot + "/Shared/walking.fbx"));
            ConfigureLocomotion(machine, idle, walk);
            AddOneShot(machine, idle, "HitLight", LoadClip(
                EnyAnimationRoot + "/ApprovedHit/Hit_Body.fbx"));
            AddOneShot(machine, idle, "MeleePrimary", LoadClip(
                EnyAnimationRoot + "/MeleeCandidates/Melee_candidate_Boxing.fbx"));
            AddCatalogStates(machine, new[]
            {
                EnyAnimationRoot + "/Shared",
                EnyAnimationRoot + "/ApprovedHit",
                EnyAnimationRoot + "/MeleeCandidates"
            }, "ENY01_SHARED");

            if (elite)
            {
                string[] elitePaths = GetAnimationPaths(
                    EnyAnimationRoot + "/EliteOnly");
                for (int index = 0; index < elitePaths.Length; index++)
                {
                    string parameter = "EliteAttack" + (index + 1);
                    controller.AddParameter(parameter, AnimatorControllerParameterType.Trigger);
                    AddOneShot(
                        machine,
                        idle,
                        parameter,
                        LoadClip(elitePaths[index]));
                }

                AddCatalogStates(
                    machine,
                    new[] { EnyAnimationRoot + "/EliteOnly" },
                    "ENY01_ELITE_ONLY");
            }
        }

        private static AnimatorController CreateController(string path)
        {
            if (AssetDatabase.LoadMainAssetAtPath(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
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
            if (player)
            {
                controller.AddParameter("Aim", AnimatorControllerParameterType.Bool);
                controller.AddParameter("CatchLight", AnimatorControllerParameterType.Trigger);
                controller.AddParameter("CatchMedium", AnimatorControllerParameterType.Trigger);
                controller.AddParameter("ThrowLight", AnimatorControllerParameterType.Trigger);
                controller.AddParameter("ThrowMedium", AnimatorControllerParameterType.Trigger);
            }

            if (ally)
            {
                controller.AddParameter("SupplyReady", AnimatorControllerParameterType.Bool);
                controller.AddParameter("SupplyRelease", AnimatorControllerParameterType.Trigger);
            }
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

        private static void AddOneShot(
            AnimatorStateMachine machine,
            AnimatorState idle,
            string trigger,
            AnimationClip clip)
        {
            AnimatorState state = AddState(machine, trigger + "Action", clip);
            AnimatorStateTransition enter = machine.AddAnyStateTransition(state);
            enter.hasExitTime = false;
            enter.duration = 0.08f;
            enter.canTransitionToSelf = false;
            enter.AddCondition(AnimatorConditionMode.If, 0f, trigger);
            AnimatorStateTransition exit = state.AddTransition(idle);
            exit.hasExitTime = true;
            exit.exitTime = 0.9f;
            exit.duration = 0.1f;
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

            EditorUtility.SetDirty(actor);
            EditorUtility.SetDirty(instance);
            EditorUtility.SetDirty(animator);
            EditorUtility.SetDirty(driver);
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
            int normalEliteStates = CountStatesWithPrefix(normal, "ENY01_ELITE_ONLY__");
            int eliteOnlyStates = CountStatesWithPrefix(elite, "ENY01_ELITE_ONLY__");
            if (normalEliteStates != 0 || eliteOnlyStates != 6)
            {
                throw new InvalidOperationException(
                    "ENY01 eligibility mismatch: Normal must exclude and Elite must include " +
                    "all six Elite-only clips.");
            }
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
