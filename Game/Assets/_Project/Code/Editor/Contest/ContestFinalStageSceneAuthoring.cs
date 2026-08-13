using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Encounter;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace BreakNotice.Contest.Editor
{
    public static class ContestFinalStageSceneAuthoring
    {
        public const string MainScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_NorthOffice_Stage01_v001.unity";

        public const string TutorialScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_Tutorial_HoldingRoom_v001.unity";

        public const string MainRootName = "FINAL_NORTH_OFFICE_STAGE_01";
        public const string MainHooksRootName = "FINAL_STAGE_SEMANTIC_HOOKS";
        public const string TutorialRootName = "TUTORIAL_HOLDING_ROOM_STAGE";
        public const string TutorialHooksRootName = "TUTORIAL_SEMANTIC_HOOKS";

        private const string SourceScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v007.unity";

        private const string AssetRoot =
            "Assets/_Project/Art/Environment/FinalStage";

        private const string MaterialRoot = AssetRoot + "/Materials";
        private const string DesignerBRoot = AssetRoot + "/DesignerB";
        private const string PolyHavenRoot = AssetRoot + "/ThirdParty/PolyHaven";
        private const string KenneyRoot = AssetRoot + "/ThirdParty/KenneyFurnitureKit";
        private const string CaptureFolderName = "break-notice-final-north-office-stage";

        private static readonly string[] ReplacedEnvironmentRoots =
        {
            "SHOWCASE_S1_GRAYBOX",
            "SHOWCASE_V2_PROVISIONAL_LOCAL_REVIEW_ONLY",
            "SHOWCASE_S7_OBJECTIVE_EXIT",
            MainRootName,
            MainHooksRootName,
            TutorialRootName,
            TutorialHooksRootName,
            "Floor",
            "Wall_North",
            "Wall_South",
            "Wall_East",
            "Wall_West",
            "Obstacle_NarrowRoute_A",
            "Obstacle_NarrowRoute_B",
            "Obstacle_WideRoute",
            "ForegroundOccluder",
            "StartMarker"
        };

        private static readonly string[] FinalHookNames =
        {
            "HOOK_REQUIRED_KEY_01",
            "HOOK_OPTIONAL_ELITE_02",
            "HOOK_EXIT_03",
            "HOOK_WEST_FORCED_ENCOUNTER",
            "HOOK_EAST_FORCED_ENCOUNTER",
            "HOOK_OPTIONAL_ALLY_RESCUE_11",
            "HOOK_STAGE_ENTRY_10",
            "HOOK_TUTORIAL_GATE_TO_10",
            "HOOK_TASK_TRACKER_REQUIRED",
            "HOOK_TASK_TRACKER_OPTIONAL",
            "HOOK_SOUND_INVESTIGATION_WEST",
            "HOOK_SOUND_INVESTIGATION_EAST",
            "HOOK_RESET_BASELINE"
        };

        private static readonly Color[] ZoneColors =
        {
            new Color(0.95f, 0.72f, 0.18f, 1f),
            new Color(0.92f, 0.28f, 0.20f, 1f),
            new Color(0.20f, 0.95f, 0.55f, 1f),
            new Color(0.22f, 0.68f, 1.00f, 1f),
            new Color(0.74f, 0.42f, 1.00f, 1f),
            new Color(1.00f, 0.52f, 0.18f, 1f),
            new Color(0.30f, 0.92f, 0.96f, 1f),
            new Color(0.45f, 0.68f, 1.00f, 1f),
            new Color(0.34f, 0.78f, 0.58f, 1f),
            new Color(0.92f, 0.38f, 0.70f, 1f),
            new Color(0.48f, 0.86f, 0.30f, 1f)
        };

        private static readonly Dictionary<string, Material> Materials =
            new Dictionary<string, Material>(StringComparer.Ordinal);

        [MenuItem("BREAK NOTICE/Contest/Final Stage/Prepare Final North Office Scenes")]
        public static void PrepareFinalStageScenes()
        {
            RequireAsset<SceneAsset>(SourceScenePath);
            RequireExternalAssets();
            EnsureFolderPath(MaterialRoot);
            ConfigureNormalMap(PolyHavenRoot + "/metal_office_desk_nor_gl_1k.jpg");
            ConfigureNormalMap(PolyHavenRoot + "/office_notepads_nor_gl_1k.jpg");
            ConfigureNormalMap(PolyHavenRoot + "/smooth_concrete_floor_nor_gl_1k.jpg");
            BuildMaterials();

            PrepareSceneCopy(MainScenePath, ApplyMainStage);
            PrepareSceneCopy(TutorialScenePath, ApplyTutorialStage);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log(
                "Final North Office Stage prepared: new Main and Tutorial scenes, " +
                "48m x 34m figure-eight main footprint, zones 01-11, semantic hooks, " +
                "Designer B visuals, Poly Haven CC0 and Kenney CC0 dressing. " +
                "No new runtime gameplay system was authored.");
        }

        [MenuItem("BREAK NOTICE/Contest/Final Stage/Capture Final Stage Review")]
        public static void CaptureFinalStageReview()
        {
            RequireAsset<SceneAsset>(MainScenePath);
            Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            GameObject player = RequireSingleSceneComponent<PrototypeInteractionStateOwner>(scene)
                .gameObject;
            Camera sourceCamera = RequireSingleSceneComponent<Camera>(scene);
            float sourceFieldOfView = sourceCamera.fieldOfView;
            string outputFolder = Path.Combine(Path.GetTempPath(), CaptureFolderName);
            Directory.CreateDirectory(outputFolder);

            string overviewPath = Path.Combine(outputFolder, "overview-1920x1080.png");
            string playerPath = Path.Combine(outputFolder, "player-camera-1920x1080.png");
            string tutorialPath = Path.Combine(outputFolder, "tutorial-overview-1920x1080.png");

            CaptureFrame(
                scene,
                overviewPath,
                new Vector3(0f, 46f, -1f),
                Quaternion.Euler(90f, 0f, 0f),
                true,
                21f,
                sourceFieldOfView);

            Vector3 playerCameraPosition =
                player.transform.position + new Vector3(1.2f, 2.2f, -1.8f);
            Vector3 focus = player.transform.position + new Vector3(0f, 0.55f, 6.5f);
            CaptureFrame(
                scene,
                playerPath,
                playerCameraPosition,
                Quaternion.LookRotation(focus - playerCameraPosition, Vector3.up),
                false,
                0f,
                sourceFieldOfView);

            Scene tutorial = EditorSceneManager.OpenScene(
                TutorialScenePath,
                OpenSceneMode.Single);
            CaptureFrame(
                tutorial,
                tutorialPath,
                new Vector3(0f, 34f, 0f),
                Quaternion.Euler(90f, 0f, 0f),
                true,
                15f,
                sourceFieldOfView);

            Debug.Log(
                $"Final Stage captures: {overviewPath} | {playerPath} | {tutorialPath}");
        }

        private static void PrepareSceneCopy(string destination, Action<Scene> apply)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(destination) == null)
            {
                if (!AssetDatabase.CopyAsset(SourceScenePath, destination))
                {
                    throw new InvalidOperationException(
                        $"Could not copy {SourceScenePath} to {destination}.");
                }

                AssetDatabase.ImportAsset(
                    destination,
                    ImportAssetOptions.ForceSynchronousImport);
            }

            Scene scene = EditorSceneManager.OpenScene(destination, OpenSceneMode.Single);
            apply(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, destination))
            {
                throw new InvalidOperationException($"Could not save {destination}.");
            }
        }

        private static void ApplyMainStage(Scene scene)
        {
            ValidateLoadedScene(scene);
            RemoveReplacedEnvironment(scene);
            StripLegacyS7ObjectiveOnly(scene);

            GameObject root = CreateRoot(scene, MainRootName);
            Transform status = CreateGroup(root.transform, "00_STATUS");
            Transform architecture = CreateGroup(root.transform, "01_STATIC_ARCHITECTURE");
            Transform zones = CreateGroup(root.transform, "02_ZONES_01_11");
            Transform routes = CreateGroup(root.transform, "03_FIGURE_EIGHT_ROUTES");
            Transform fixedFurniture = CreateGroup(root.transform, "04_FIXED_FURNITURE");
            Transform dressing = CreateGroup(root.transform, "05_DRESSING_NO_COLLISION");
            Transform lighting = CreateGroup(root.transform, "06_LIGHTING");
            Transform external = CreateGroup(root.transform, "07_EXTERNAL_ASSET_INTEGRATION");
            Transform gameplayVisuals = CreateGroup(root.transform, "08_GAMEPLAY_VISUAL_ATTACHMENTS");
            Transform allyAnchors = CreateGroup(root.transform, "09_S2_FOLLOW_ANCHORS");

            BuildMainStatus(status);
            BuildMainArchitecture(architecture);
            BuildMainZones(zones);
            BuildMainRoutes(routes);
            BuildMainFurniture(fixedFurniture, dressing, external);
            BuildMainLighting(lighting);
            BuildExternalProvenance(external);
            RepositionMainGameplay(scene);
            ConfigureMainMobileAlly(scene, allyAnchors);
            AttachDesignerBGameplayVisuals(scene, gameplayVisuals);
            BuildFinalStageHooks(scene);
            RepositionSceneCamera(scene, new Vector3(4.6f, 3.4f, -15.4f));
        }

        private static void ApplyTutorialStage(Scene scene)
        {
            ValidateLoadedScene(scene);
            RemoveReplacedEnvironment(scene);
            StripLegacyS7ObjectiveOnly(scene);
            DisableTutorialGameplaySystems(scene);

            GameObject root = CreateRoot(scene, TutorialRootName);
            Transform status = CreateGroup(root.transform, "00_STATUS");
            Transform architecture = CreateGroup(root.transform, "01_STATIC_ARCHITECTURE");
            Transform learning = CreateGroup(root.transform, "02_LEARNING_SEGMENTS");
            Transform furniture = CreateGroup(root.transform, "03_FIXED_FURNITURE");
            Transform dressing = CreateGroup(root.transform, "04_DRESSING_NO_COLLISION");
            Transform lighting = CreateGroup(root.transform, "05_LIGHTING");
            Transform route = CreateGroup(root.transform, "06_TUTORIAL_ROUTE");
            Transform external = CreateGroup(root.transform, "07_EXTERNAL_ASSET_INTEGRATION");

            CreateMarker(status, "MAP_LANE_SPACE_AND_HOOKS_ONLY", Vector3.zero);
            CreateMarker(status, "NO_TUTORIAL_SCRIPTING", Vector3.zero);
            CreateMarker(status, "NO_INPUT_ACTION_CHANGE", Vector3.zero);
            CreateMarker(status, "MAIN_STAGE_TRANSITION_TARGET_ZONE_10", Vector3.zero);
            BuildTutorialArchitecture(architecture);
            BuildTutorialLearningSegments(learning);
            BuildTutorialFurniture(furniture, dressing, external);
            BuildTutorialLighting(lighting);
            BuildTutorialRoute(route);
            BuildTutorialHooks(scene);
            RepositionTutorialGameplay(scene);
            BuildExternalProvenance(external);
            RepositionSceneCamera(scene, new Vector3(4.5f, 3.2f, -10.7f));
        }

        private static void BuildMainStatus(Transform parent)
        {
            string[] markers =
            {
                "HUMAN_OWNER_FINAL_STAGE_LAYOUT",
                "SOURCE_TOPDOWN_EF7310980D95D3AC",
                "SOURCE_SPEC_988FC51CCF9B1254",
                "FOOTPRINT_48M_X_34M",
                "FIGURE_EIGHT_WEST_AND_EAST_LOOPS",
                "PARTITIONS_2.1M_BOTTOM_Y_0",
                "EXTERNAL_ASSET_LICENSE_VERIFIED",
                "GAMEPLAY_RUNTIME_UNCHANGED",
                "SOUND_RADIUS_REVIEW_REQUIRED",
                "TASK_UI_ANIMATION_REVIEW_REQUIRED"
            };

            foreach (string marker in markers)
            {
                CreateMarker(parent, marker, Vector3.zero);
            }
        }

        private static void BuildMainArchitecture(Transform parent)
        {
            Material floor = GetMaterial("Floor");
            Material wall = GetMaterial("Wall");
            Material partition = GetMaterial("Partition");
            Transform envelope = CreateGroup(parent, "ENVELOPE_48X34");
            CreateBox(
                envelope,
                "STATIC_FLOOR_48X34",
                new Vector3(0f, -0.1f, 0f),
                new Vector3(48f, 0.2f, 34f),
                floor,
                true);

            Transform outer = CreateGroup(envelope, "OUTER_WALLS_4.2M");
            CreateWall(outer, "WALL_NORTH", new Vector3(0f, 16.85f), new Vector2(48f, 0.3f));
            CreateWall(outer, "WALL_WEST", new Vector3(-23.85f, 0f), new Vector2(0.3f, 34f));
            CreateWall(outer, "WALL_EAST", new Vector3(23.85f, 0f), new Vector2(0.3f, 34f));
            CreateWall(outer, "WALL_SOUTH_WEST", new Vector3(-13f, -16.85f), new Vector2(22f, 0.3f));
            CreateWall(outer, "WALL_SOUTH_EAST", new Vector3(13f, -16.85f), new Vector2(22f, 0.3f));
            CreateDoorLintel(outer, "DOOR_FRAME_TUTORIAL_TO_10", new Vector3(0f, -16.82f), 4f, 0f);

            Transform internalWalls = CreateGroup(parent, "INTERNAL_FIXED_WALLS_4.2M");
            CreateWall(internalWalls, "WALL_01_EAST", new Vector3(-12.5f, 12.35f), new Vector2(0.3f, 8.7f));
            CreateWall(internalWalls, "WALL_01_SOUTH_WEST", new Vector3(-22.1f, 8f), new Vector2(3.2f, 0.3f));
            CreateWall(internalWalls, "WALL_01_SOUTH_EAST", new Vector3(-15f, 8f), new Vector2(5f, 0.3f));
            CreateDoorLintel(internalWalls, "DOOR_FRAME_01_KEY", new Vector3(-19f, 8f), 3f, 0f);

            CreateWall(internalWalls, "WALL_02_WEST", new Vector3(-10.5f, 13.35f), new Vector2(0.3f, 6.7f));
            CreateWall(internalWalls, "WALL_02_EAST", new Vector3(9.5f, 13.35f), new Vector2(0.3f, 6.7f));
            CreateWall(internalWalls, "WALL_02_SOUTH_WEST", new Vector3(-6.25f, 10f), new Vector2(8.5f, 0.3f));
            CreateWall(internalWalls, "WALL_02_SOUTH_EAST", new Vector3(5.75f, 10f), new Vector2(7.5f, 0.3f));
            CreateDoorLintel(internalWalls, "DOOR_FRAME_02_ELITE", new Vector3(0f, 10f), 4f, 0f);

            CreateWall(internalWalls, "WALL_03_WEST", new Vector3(12.5f, 13.35f), new Vector2(0.3f, 6.7f));
            CreateWall(internalWalls, "WALL_03_SOUTH_WEST", new Vector3(15.25f, 10f), new Vector2(5.5f, 0.3f));
            CreateWall(internalWalls, "WALL_03_SOUTH_EAST", new Vector3(22.35f, 10f), new Vector2(2.7f, 0.3f));
            CreateDoorLintel(internalWalls, "DOOR_FRAME_03_EXIT", new Vector3(19.5f, 10f), 3f, 0f);

            CreateWall(internalWalls, "WALL_07_WEST", new Vector3(-8f, -10f), new Vector2(0.3f, 6f));
            CreateWall(internalWalls, "WALL_07_EAST", new Vector3(8f, -10f), new Vector2(0.3f, 6f));
            CreateWall(internalWalls, "WALL_07_SOUTH_WEST", new Vector3(-5.5f, -13f), new Vector2(5f, 0.3f));
            CreateWall(internalWalls, "WALL_07_SOUTH_EAST", new Vector3(5.5f, -13f), new Vector2(5f, 0.3f));
            CreateWall(internalWalls, "WALL_07_NORTH_WEST", new Vector3(-4.5f, -7f), new Vector2(7f, 0.3f));
            CreateWall(internalWalls, "WALL_07_NORTH_EAST", new Vector3(6.5f, -7f), new Vector2(3f, 0.3f));
            CreateDoorLintel(internalWalls, "DOOR_FRAME_10_TO_07", new Vector3(0f, -13f), 6f, 0f);
            CreateDoorLintel(internalWalls, "DOOR_FRAME_07_TO_06", new Vector3(2f, -7f), 6f, 0f);

            CreateWall(internalWalls, "WALL_06_WEST", new Vector3(-5f, -5f), new Vector2(0.3f, 4f));
            CreateWall(internalWalls, "WALL_06_EAST", new Vector3(5f, -5f), new Vector2(0.3f, 4f));

            CreateWall(internalWalls, "WALL_08_EAST_SOUTH", new Vector3(-17f, -14.5f), new Vector2(0.3f, 4.4f));
            CreateWall(internalWalls, "WALL_08_NORTH_WEST", new Vector3(-21.85f, -10f), new Vector2(3.7f, 0.3f));
            CreateWall(internalWalls, "WALL_09_EAST_SOUTH", new Vector3(-9f, -14.2f), new Vector2(0.3f, 5f));
            CreateWall(internalWalls, "WALL_09_NORTH_WEST", new Vector3(-15.75f, -10f), new Vector2(2.5f, 0.3f));
            CreateWall(internalWalls, "WALL_09_NORTH_EAST", new Vector3(-10.25f, -10f), new Vector2(2.5f, 0.3f));
            CreateDoorLintel(internalWalls, "DOOR_FRAME_09_ARCHIVE", new Vector3(-13f, -10f), 3f, 0f);

            CreateWall(internalWalls, "WALL_11_WEST_SOUTH", new Vector3(10f, -13.4f), new Vector2(0.3f, 6.6f));
            CreateWall(internalWalls, "WALL_11_NORTH_WEST", new Vector3(13f, -8f), new Vector2(6f, 0.3f));
            CreateWall(internalWalls, "WALL_11_NORTH_EAST", new Vector3(21.35f, -8f), new Vector2(4.7f, 0.3f));
            CreateDoorLintel(internalWalls, "DOOR_FRAME_11_RESCUE", new Vector3(17.5f, -8f), 3f, 0f);

            Transform partitions = CreateGroup(parent, "PARTITIONS_2_1M_BOTTOM_0");
            CreatePartition(partitions, "PARTITION_05_WEST_SOUTH", new Vector3(-7f, -1.5f), new Vector2(5.5f, 0.2f));
            CreatePartition(partitions, "PARTITION_05_WEST_NORTH", new Vector3(-7f, 4.5f), new Vector2(5.5f, 0.2f));
            CreatePartition(partitions, "PARTITION_05_EAST_SOUTH", new Vector3(7f, -1.5f), new Vector2(5.5f, 0.2f));
            CreatePartition(partitions, "PARTITION_05_EAST_NORTH", new Vector3(7f, 4.5f), new Vector2(5.5f, 0.2f));
            CreatePartition(partitions, "PARTITION_05_WEST_OUTER_SOUTH", new Vector3(-9.75f, -0.5f), new Vector2(0.2f, 2f));
            CreatePartition(partitions, "PARTITION_05_WEST_OUTER_NORTH", new Vector3(-9.75f, 4f), new Vector2(0.2f, 1f));
            CreatePartition(partitions, "PARTITION_05_EAST_OUTER_SOUTH", new Vector3(9.75f, -0.5f), new Vector2(0.2f, 2f));
            CreatePartition(partitions, "PARTITION_05_EAST_OUTER_NORTH", new Vector3(9.75f, 4f), new Vector2(0.2f, 1f));
            CreatePartition(partitions, "PARTITION_05_WEST_INNER_SOUTH", new Vector3(-4.25f, 0f), new Vector2(0.2f, 2.5f));
            CreatePartition(partitions, "PARTITION_05_WEST_INNER_NORTH", new Vector3(-4.25f, 4f), new Vector2(0.2f, 2.5f));
            CreatePartition(partitions, "PARTITION_05_EAST_INNER_SOUTH", new Vector3(4.25f, 0f), new Vector2(0.2f, 2.5f));
            CreatePartition(partitions, "PARTITION_05_EAST_INNER_NORTH", new Vector3(4.25f, 4f), new Vector2(0.2f, 2.5f));
            CreatePartition(partitions, "PARTITION_04_CHECKPOINT_WEST", new Vector3(-7.5f, 8.6f), new Vector2(5f, 0.2f));
            CreatePartition(partitions, "PARTITION_04_CHECKPOINT_EAST", new Vector3(7.5f, 8.6f), new Vector2(5f, 0.2f));
            CreatePartition(partitions, "PARTITION_01_KEY_DESK", new Vector3(-19f, 13.9f), new Vector2(4f, 0.2f));
            CreatePartition(partitions, "PARTITION_11_WORKSTATION_A", new Vector3(14f, -12f), new Vector2(3.5f, 0.2f));
            CreatePartition(partitions, "PARTITION_11_WORKSTATION_B", new Vector3(19f, -12f), new Vector2(3.5f, 0.2f));
            CreatePartition(partitions, "PARTITION_09_ARCHIVE_SCREEN", new Vector3(-13f, -13.5f), new Vector2(5f, 0.2f));

            foreach (Renderer renderer in partitions.GetComponentsInChildren<Renderer>(true))
            {
                renderer.sharedMaterial = partition;
            }

            foreach (Renderer renderer in outer.GetComponentsInChildren<Renderer>(true)
                         .Concat(internalWalls.GetComponentsInChildren<Renderer>(true)))
            {
                renderer.sharedMaterial = wall;
            }
        }

        private static void BuildMainZones(Transform parent)
        {
            ZoneSpec[] zones =
            {
                new ZoneSpec("ZONE_01_SUPPLY_POWER_KEY", "01  SUPPLY / POWER / KEY", new Vector2(-18.25f, 12.35f), new Vector2(10.5f, 8.3f)),
                new ZoneSpec("ZONE_02_OPTIONAL_ELITE_HALL", "02  OPTIONAL ELITE HALL", new Vector2(-0.5f, 13.35f), new Vector2(19f, 6.3f)),
                new ZoneSpec("ZONE_03_FINAL_EXIT", "03  FINAL EXIT", new Vector2(18.25f, 13.35f), new Vector2(10.5f, 6.3f)),
                new ZoneSpec("ZONE_04_SECURITY_CHECKPOINT", "04  SECURITY CHECKPOINT", new Vector2(0f, 8.5f), new Vector2(25f, 2.6f)),
                new ZoneSpec("ZONE_05_CENTRAL_CUBICLE_FIELD", "05  CENTRAL CUBICLE FIELD", new Vector2(0f, 2f), new Vector2(21f, 9f)),
                new ZoneSpec("ZONE_06_NARROW_TRANSITION", "06  CENTRAL TRANSITION", new Vector2(0f, -5f), new Vector2(10f, 4f)),
                new ZoneSpec("ZONE_07_CONFERENCE_TEACHING", "07  CONFERENCE / FIRST COMBAT", new Vector2(0f, -10f), new Vector2(16f, 6f)),
                new ZoneSpec("ZONE_08_UTILITY_COPIER", "08  UTILITY / COPIER", new Vector2(-20.35f, -13.35f), new Vector2(6.7f, 6.7f)),
                new ZoneSpec("ZONE_09_ARCHIVE_STORAGE", "09  ARCHIVE / STORAGE", new Vector2(-13f, -13.35f), new Vector2(7.6f, 6.7f)),
                new ZoneSpec("ZONE_10_NORTH_OFFICE_LOBBY_ENTRY", "10  NORTH OFFICE LOBBY", new Vector2(0f, -15f), new Vector2(10f, 3.5f)),
                new ZoneSpec("ZONE_11_SECONDARY_OBJECTIVE", "11  SECONDARY OBJECTIVE / RESCUE", new Vector2(17f, -12f), new Vector2(13.5f, 8f))
            };

            for (int index = 0; index < zones.Length; index++)
            {
                ZoneSpec zone = zones[index];
                Transform group = CreateGroup(parent, zone.Name);
                group.position = new Vector3(zone.Center.x, 0f, zone.Center.y);
                CreateRectangleLine(
                    group,
                    "ZONE_BOUNDARY_BAND",
                    zone.Center,
                    zone.Size,
                    ZoneColors[index],
                    0.10f);
                CreateLabel(
                    group,
                    "ZONE_FLOOR_HEADER",
                    zone.Label,
                    new Vector3(zone.Center.x, 0.035f, zone.Center.y),
                    Quaternion.Euler(90f, 0f, 0f),
                    ZoneColors[index],
                    0.11f);
            }
        }

        private static void BuildMainRoutes(Transform parent)
        {
            BuildRoute(
                parent,
                "ROUTE_WEST_LOOP_TO_KEY_AND_EXIT",
                new Color(0.22f, 0.68f, 1f, 1f),
                new[]
                {
                    new RoutePointSpec("P00_STAGE_ENTRY_10", 0f, -14.2f),
                    new RoutePointSpec("P01_CONFERENCE_SOUTH", 0f, -12f),
                    new RoutePointSpec("P02_CONFERENCE_NORTH_GATE_CLEAR", 3.5f, -6.5f),
                    new RoutePointSpec("P03_CENTRAL_TRANSITION", 0f, -5f),
                    new RoutePointSpec("P04_CENTRAL_FIELD", 0f, -2.5f),
                    new RoutePointSpec("P05_WEST_SOUTH_CONNECTOR", -11.5f, -2.5f),
                    new RoutePointSpec("P06_WEST_FORCED_ENCOUNTER", -18.5f, -3f),
                    new RoutePointSpec("P07_WEST_NORTH_TURN", -19f, 6.5f),
                    new RoutePointSpec("P08_KEY_ROOM_ENTRY", -19f, 9.5f),
                    new RoutePointSpec("P09_REQUIRED_KEY_01", -19f, 13f),
                    new RoutePointSpec("P10_RETURN_WEST_NORTH", -19f, 6.5f),
                    new RoutePointSpec("P11_ZONE_01_SOUTH_CLEAR", -13.2f, 7f),
                    new RoutePointSpec("P12_SECURITY_WEST", -11f, 7f),
                    new RoutePointSpec("P13_SECURITY_CENTER", 0f, 7f),
                    new RoutePointSpec("P14_SECURITY_EAST", 11f, 7f),
                    new RoutePointSpec("P15_EXIT_SOUTH_CLEAR", 17.5f, 8.5f),
                    new RoutePointSpec("P16_EXIT_ROOM_ENTRY", 19.5f, 10.8f),
                    new RoutePointSpec("P17_FINAL_EXIT_03", 20f, 14f)
                });

            BuildRoute(
                parent,
                "ROUTE_EAST_LOOP_TO_KEY_AND_EXIT",
                new Color(1f, 0.48f, 0.22f, 1f),
                new[]
                {
                    new RoutePointSpec("P00_STAGE_ENTRY_10", 0f, -14.2f),
                    new RoutePointSpec("P01_CONFERENCE_SOUTH", 0f, -12f),
                    new RoutePointSpec("P02_CONFERENCE_NORTH_GATE_CLEAR", 3.5f, -6.5f),
                    new RoutePointSpec("P03_CENTRAL_TRANSITION", 0f, -5f),
                    new RoutePointSpec("P04_CENTRAL_FIELD", 0f, -2.5f),
                    new RoutePointSpec("P05_EAST_SOUTH_CONNECTOR", 11.5f, -2.5f),
                    new RoutePointSpec("P06_EAST_FORCED_ENCOUNTER", 18.5f, -3f),
                    new RoutePointSpec("P07_EAST_NORTH_TURN", 19f, 7.5f),
                    new RoutePointSpec("P08_SECURITY_EAST", 11f, 7f),
                    new RoutePointSpec("P09_SECURITY_CENTER", 0f, 7f),
                    new RoutePointSpec("P10_SECURITY_WEST", -11f, 7f),
                    new RoutePointSpec("P11_WEST_NORTH_TURN", -19f, 6.5f),
                    new RoutePointSpec("P12_KEY_ROOM_ENTRY", -19f, 9.5f),
                    new RoutePointSpec("P13_REQUIRED_KEY_01", -19f, 13f),
                    new RoutePointSpec("P14_RETURN_WEST_NORTH", -19f, 6.5f),
                    new RoutePointSpec("P15_ZONE_01_SOUTH_CLEAR", -13.2f, 7f),
                    new RoutePointSpec("P16_SECURITY_WEST_RETURN", -11f, 7f),
                    new RoutePointSpec("P17_SECURITY_CENTER_RETURN", 0f, 7f),
                    new RoutePointSpec("P18_SECURITY_EAST_RETURN", 11f, 7f),
                    new RoutePointSpec("P19_EXIT_SOUTH_CLEAR", 17.5f, 8.5f),
                    new RoutePointSpec("P20_EXIT_ROOM_ENTRY", 19.5f, 10.8f),
                    new RoutePointSpec("P21_FINAL_EXIT_03", 20f, 14f)
                });

            BuildRoute(
                parent,
                "ROUTE_CENTRAL_CROSS",
                new Color(0.76f, 0.52f, 1f, 1f),
                new[]
                {
                    new RoutePointSpec("P00_WEST_CONNECTOR", -11f, 2f),
                    new RoutePointSpec("P01_CENTRAL_CUT", 0f, 2f),
                    new RoutePointSpec("P02_EAST_CONNECTOR", 11f, 2f),
                    new RoutePointSpec("P03_CENTRAL_RETURN", 0f, 2f),
                    new RoutePointSpec("P04_NORTH_CHECKPOINT", 0f, 8.5f)
                });

            CreateMarker(parent, "CONTRACT_NO_DIRECT_01_TO_03_SHORTCUT", new Vector3(0f, 0f, 13f));
            CreateMarker(parent, "CONTRACT_WEST_FORCED_ENCOUNTER_MIN_1", new Vector3(-18.5f, 0f, -3f));
            CreateMarker(parent, "CONTRACT_EAST_FORCED_ENCOUNTER_MIN_1", new Vector3(18.5f, 0f, -3f));
        }

        private static void BuildMainFurniture(
            Transform fixedParent,
            Transform dressingParent,
            Transform externalParent)
        {
            Transform collision = CreateGroup(fixedParent, "STATIC_FURNITURE_COLLISION");
            Transform poly = CreateGroup(externalParent, "POLY_HAVEN_CC0");
            Transform kenney = CreateGroup(externalParent, "KENNEY_CC0");
            Transform designer = CreateGroup(externalParent, "DESIGNER_B_RUNTIME_VISUALS");
            CreateMarker(designer, "ATTACHMENTS_LIVE_UNDER_GAMEPLAY_OBJECTS", Vector3.zero);

            Vector3[] deskPositions =
            {
                new Vector3(-7f, 0f, -0.5f),
                new Vector3(-7f, 0f, 3.2f),
                new Vector3(-7f, 0f, 5.4f),
                new Vector3(7f, 0f, -0.5f),
                new Vector3(7f, 0f, 3.2f),
                new Vector3(7f, 0f, 5.4f),
                new Vector3(-20.5f, 0f, 13f),
                new Vector3(-5f, 0f, 13.2f),
                new Vector3(4.5f, 0f, 13.2f),
                new Vector3(13f, 0f, -13.5f),
                new Vector3(17f, 0f, -13.5f),
                new Vector3(21f, 0f, -13.5f)
            };

            for (int index = 0; index < deskPositions.Length; index++)
            {
                float yaw = index >= 9 ? 180f : index % 2 == 0 ? 0f : 180f;
                PlaceStaticModelWithCollider(
                    poly,
                    collision,
                    PolyHavenRoot + "/metal_office_desk_1k.fbx",
                    $"POLYHAVEN_METAL_OFFICE_DESK_{index:00}",
                    deskPositions[index],
                    Quaternion.Euler(0f, yaw, 0f),
                    new Vector3(1.85f, 0.78f, 0.82f),
                    GetMaterial("PolyDesk"));
            }

            Vector3[] notepadPositions =
            {
                new Vector3(-7f, 0.79f, -0.5f),
                new Vector3(-7f, 0.79f, 3.2f),
                new Vector3(7f, 0.79f, -0.5f),
                new Vector3(7f, 0.79f, 3.2f),
                new Vector3(-20.5f, 0.79f, 13f),
                new Vector3(17f, 0.79f, -13.5f)
            };

            for (int index = 0; index < notepadPositions.Length; index++)
            {
                InstantiateNormalizedModel(
                    poly,
                    PolyHavenRoot + "/office_notepads_1k.fbx",
                    $"POLYHAVEN_OFFICE_NOTEPADS_{index:00}",
                    notepadPositions[index],
                    Quaternion.identity,
                    new Vector3(0.72f, 0.055f, 0.38f),
                    GetMaterial("PolyPaper"),
                    true);
            }

            PlaceStaticModelWithCollider(
                kenney,
                collision,
                KenneyRoot + "/table.fbx",
                "KENNEY_CONFERENCE_TABLE_07",
                new Vector3(-2.5f, 0f, -10f),
                Quaternion.identity,
                new Vector3(4.6f, 0.80f, 1.55f),
                GetMaterial("KenneyFurniture"));

            Vector3[] conferenceChairs =
            {
                new Vector3(-4.2f, 0f, -11.25f),
                new Vector3(-1.6f, 0f, -11.25f),
                new Vector3(-4.2f, 0f, -8.75f),
                new Vector3(-1.6f, 0f, -8.75f)
            };
            for (int index = 0; index < conferenceChairs.Length; index++)
            {
                InstantiateNormalizedModel(
                    dressingParent,
                    KenneyRoot + "/chairModernFrameCushion.fbx",
                    $"KENNEY_CONFERENCE_CHAIR_{index:00}",
                    conferenceChairs[index],
                    Quaternion.Euler(0f, index < 2 ? 0f : 180f, 0f),
                    new Vector3(0.58f, 0.92f, 0.58f),
                    GetMaterial("KenneyAccent"),
                    true);
            }

            Vector3[] archiveShelves =
            {
                new Vector3(-15.8f, 0f, -15.8f),
                new Vector3(-13.2f, 0f, -15.8f),
                new Vector3(-10.5f, 0f, -15.8f),
                new Vector3(-22.3f, 0f, -11f),
                new Vector3(-18.2f, 0f, -15.8f),
                new Vector3(-21.5f, 0f, 15.8f)
            };
            for (int index = 0; index < archiveShelves.Length; index++)
            {
                PlaceStaticModelWithCollider(
                    kenney,
                    collision,
                    KenneyRoot + "/bookcaseClosed.fbx",
                    $"KENNEY_ARCHIVE_BOOKCASE_{index:00}",
                    archiveShelves[index],
                    Quaternion.identity,
                    new Vector3(1.65f, 2.25f, 0.48f),
                    GetMaterial("KenneyStorage"));
            }

            Vector3[] boxPositions =
            {
                new Vector3(-15.2f, 0f, -11f),
                new Vector3(-12.1f, 0f, -11.2f),
                new Vector3(-20.7f, 0f, -15.4f),
                new Vector3(-18.8f, 0f, 11f),
                new Vector3(-16.4f, 0f, 11f),
                new Vector3(14.2f, 0f, -9.2f),
                new Vector3(21.6f, 0f, -9.4f),
                new Vector3(-5.8f, 0f, 15.2f)
            };
            for (int index = 0; index < boxPositions.Length; index++)
            {
                InstantiateNormalizedModel(
                    dressingParent,
                    KenneyRoot + "/cardboardBoxClosed.fbx",
                    $"KENNEY_ARCHIVE_BOX_{index:00}",
                    boxPositions[index],
                    Quaternion.Euler(0f, index * 23f, 0f),
                    new Vector3(0.62f, 0.52f, 0.52f),
                    GetMaterial("KenneyBox"),
                    true);
            }

            for (int index = 0; index < deskPositions.Length; index++)
            {
                InstantiateNormalizedModel(
                    dressingParent,
                    KenneyRoot + "/computerScreen.fbx",
                    $"KENNEY_COMPUTER_SCREEN_{index:00}",
                    deskPositions[index] + new Vector3(0f, 0.79f, 0f),
                    Quaternion.Euler(0f, index >= 9 ? 180f : index % 2 == 0 ? 0f : 180f, 0f),
                    new Vector3(0.52f, 0.48f, 0.18f),
                    GetMaterial("KenneyMonitor"),
                    true);
            }

            Vector3[] plants =
            {
                new Vector3(-6.5f, 0f, -15.2f),
                new Vector3(6.5f, 0f, -15.2f),
                new Vector3(-21.8f, 0f, 7f),
                new Vector3(21.8f, 0f, 8.2f),
                new Vector3(-9.5f, 0f, 7.2f),
                new Vector3(9.5f, 0f, 7.2f)
            };
            for (int index = 0; index < plants.Length; index++)
            {
                InstantiateNormalizedModel(
                    dressingParent,
                    KenneyRoot + (index % 2 == 0 ? "/pottedPlant.fbx" : "/plantSmall1.fbx"),
                    $"KENNEY_PLANT_{index:00}",
                    plants[index],
                    Quaternion.Euler(0f, index * 37f, 0f),
                    new Vector3(0.65f, index % 2 == 0 ? 1.35f : 0.78f, 0.65f),
                    GetMaterial("KenneyPlant"),
                    true);
            }

            InstantiateNormalizedModel(
                dressingParent,
                KenneyRoot + "/coatRackStanding.fbx",
                "KENNEY_COAT_RACK_LOBBY",
                new Vector3(-3.8f, 0f, -15.4f),
                Quaternion.identity,
                new Vector3(0.55f, 1.9f, 0.55f),
                GetMaterial("KenneyFurniture"),
                true);
            InstantiateNormalizedModel(
                dressingParent,
                KenneyRoot + "/kitchenCoffeeMachine.fbx",
                "KENNEY_COFFEE_MACHINE_08",
                new Vector3(-20f, 0.78f, -15.3f),
                Quaternion.identity,
                new Vector3(0.42f, 0.52f, 0.38f),
                GetMaterial("KenneyMonitor"),
                true);

            Vector3[] bins =
            {
                new Vector3(-8.5f, 0f, -2.5f),
                new Vector3(8.5f, 0f, -2.5f),
                new Vector3(-15.5f, 0f, 7f),
                new Vector3(15.5f, 0f, 7f),
                new Vector3(22f, 0f, -15f)
            };
            for (int index = 0; index < bins.Length; index++)
            {
                InstantiateNormalizedModel(
                    dressingParent,
                    KenneyRoot + "/trashcan.fbx",
                    $"KENNEY_TRASHCAN_{index:00}",
                    bins[index],
                    Quaternion.identity,
                    new Vector3(0.42f, 0.62f, 0.42f),
                    GetMaterial("KenneyStorage"),
                    true);
            }
        }

        private static void BuildMainLighting(Transform parent)
        {
            Vector3[] positions =
            {
                new Vector3(-18f, 3.8f, 13f),
                new Vector3(0f, 3.8f, 13f),
                new Vector3(18f, 3.8f, 13f),
                new Vector3(-8f, 3.8f, 2f),
                new Vector3(8f, 3.8f, 2f),
                new Vector3(-18f, 3.8f, -4f),
                new Vector3(18f, 3.8f, -4f),
                new Vector3(0f, 3.8f, -10f),
                new Vector3(-14f, 3.8f, -13f),
                new Vector3(16f, 3.8f, -13f)
            };

            for (int index = 0; index < positions.Length; index++)
            {
                GameObject fixture = CreateBox(
                    parent,
                    $"CEILING_FIXTURE_{index:00}",
                    positions[index],
                    new Vector3(2.8f, 0.08f, 0.32f),
                    GetMaterial("LightFixture"),
                    false);
                Light light = fixture.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 11f;
                light.intensity = 1.35f;
                light.color = index == 7
                    ? new Color(1f, 0.76f, 0.62f)
                    : new Color(0.76f, 0.88f, 1f);
                light.shadows = LightShadows.Soft;
            }
        }

        private static void BuildExternalProvenance(Transform parent)
        {
            Transform provenance = FindDirectChild(parent, "ASSET_PROVENANCE") ??
                                   CreateGroup(parent, "ASSET_PROVENANCE");
            CreateMarker(
                provenance,
                "DESIGNER_B_SOURCE_COMMIT_7536F551FCA146E225794219BC219D7F07DD5314",
                Vector3.zero);
            CreateMarker(
                provenance,
                "POLY_HAVEN_CC0_POWERED_BY_POLY_HAVEN",
                Vector3.zero);
            CreateMarker(
                provenance,
                "KENNEY_FURNITURE_KIT_2_0_CC0",
                Vector3.zero);
            CreateMarker(
                provenance,
                "EXTERNAL_MESH_COLLIDER_COUNT_ZERO",
                Vector3.zero);
        }

        private static void BuildFinalStageHooks(Scene scene)
        {
            GameObject root = CreateRoot(scene, MainHooksRootName);
            var positions = new Dictionary<string, Vector3>(StringComparer.Ordinal)
            {
                { "HOOK_REQUIRED_KEY_01", new Vector3(-19f, 0f, 13f) },
                { "HOOK_OPTIONAL_ELITE_02", new Vector3(0f, 0f, 13.5f) },
                { "HOOK_EXIT_03", new Vector3(20f, 0f, 14f) },
                { "HOOK_WEST_FORCED_ENCOUNTER", new Vector3(-18.5f, 0f, -3f) },
                { "HOOK_EAST_FORCED_ENCOUNTER", new Vector3(18.5f, 0f, -3f) },
                { "HOOK_OPTIONAL_ALLY_RESCUE_11", new Vector3(18f, 0f, -12f) },
                { "HOOK_STAGE_ENTRY_10", new Vector3(0f, 0f, -14.2f) },
                { "HOOK_TUTORIAL_GATE_TO_10", new Vector3(0f, 0f, -16.2f) },
                { "HOOK_TASK_TRACKER_REQUIRED", new Vector3(-4f, 0f, -14.5f) },
                { "HOOK_TASK_TRACKER_OPTIONAL", new Vector3(4f, 0f, -14.5f) },
                { "HOOK_SOUND_INVESTIGATION_WEST", new Vector3(-16f, 0f, 2f) },
                { "HOOK_SOUND_INVESTIGATION_EAST", new Vector3(16f, 0f, 2f) },
                { "HOOK_RESET_BASELINE", new Vector3(0f, 0f, -14.2f) }
            };

            foreach (string name in FinalHookNames)
            {
                Vector3 scale = name.Contains("ENCOUNTER")
                    ? new Vector3(5f, 2f, 5f)
                    : name.Contains("SOUND")
                        ? new Vector3(7f, 2f, 7f)
                        : new Vector3(1.5f, 2f, 1.5f);
                Color color = name.Contains("EXIT")
                    ? new Color(0.25f, 1f, 0.45f)
                    : name.Contains("KEY")
                        ? new Color(1f, 0.75f, 0.12f)
                        : new Color(0.35f, 0.8f, 1f);
                CreateHook(root.transform, name, positions[name], scale, color);
            }

            CreateMarker(root.transform, "REVIEW_REQUIRED_SOUND_RADIUS_AND_WALL_ATTENUATION", Vector3.zero);
            CreateMarker(root.transform, "REVIEW_REQUIRED_TASK_UI_ANIMATION", Vector3.zero);
            CreateMarker(root.transform, "CONTRACT_KEY_01_REQUIRED_BEFORE_EXIT_03", Vector3.zero);
        }

        private static void RepositionMainGameplay(Scene scene)
        {
            GameObject player = RequireSingleSceneComponent<PrototypeInteractionStateOwner>(scene)
                .gameObject;
            ShowcaseMobileAllyController mobile =
                RequireSingleSceneComponent<ShowcaseMobileAllyController>(scene);
            SetActorPosition(player, new Vector3(0f, 1f, -14.2f), Quaternion.identity);
            SetActorPosition(mobile.gameObject, new Vector3(-1.8f, 1f, -13.6f), Quaternion.identity);

            GameObject runnerOne = RequireRootAny(
                scene,
                "GAMEPLAY_ENY01_RUNNER_01",
                "EnemyProxy_01");
            GameObject runnerTwo = RequireRootAny(
                scene,
                "GAMEPLAY_ENY01_RUNNER_02",
                "EnemyProxy_02");
            SetActorPosition(runnerOne, new Vector3(-18.5f, 1f, -2.5f), Quaternion.Euler(0f, 90f, 0f));
            SetActorPosition(runnerTwo, new Vector3(18.5f, 1f, -2.5f), Quaternion.Euler(0f, -90f, 0f));

            var targetPositions = new Dictionary<string, Vector3>(StringComparer.Ordinal)
            {
                { "GAMEPLAY_BINDER_ALLY_01", new Vector3(-18.4f, 0.16f, 11.4f) },
                { "GAMEPLAY_BINDER_ALLY_02", new Vector3(-7f, 0.16f, 0.8f) },
                { "GAMEPLAY_BINDER_ALLY_03", new Vector3(15f, 0.16f, -11.5f) },
                { "GAMEPLAY_BINDER_DIRECT_01", new Vector3(-14f, 0.16f, -12f) },
                { "GAMEPLAY_BINDER_DIRECT_02", new Vector3(7f, 0.16f, 0.8f) },
                { "GAMEPLAY_CHAIR_ALLY_01", new Vector3(-4.2f, 0.5f, -11.2f) },
                { "GAMEPLAY_CHAIR_DIRECT_01", new Vector3(-1.6f, 0.5f, -8.75f) },
                { "GAMEPLAY_CHAIR_DIRECT_02", new Vector3(15.5f, 0.5f, -10f) },
                { "GAMEPLAY_DRAWER_GATE_01", new Vector3(10.8f, 0.5f, 7f) },
                { "GAMEPLAY_COPIER_RECOGNITION_01", new Vector3(-20.5f, 0.49f, -13.5f) }
            };

            foreach (PrototypeInteractionTarget target in GetSceneComponents<PrototypeInteractionTarget>(scene))
            {
                if (!targetPositions.TryGetValue(target.gameObject.name, out Vector3 position))
                {
                    throw new InvalidOperationException(
                        $"No final-stage position authored for gameplay target {target.gameObject.name}.");
                }
                target.transform.SetPositionAndRotation(position, Quaternion.identity);
                EditorUtility.SetDirty(target.transform);
            }
        }

        private static void ConfigureMainMobileAlly(Scene scene, Transform parent)
        {
            ShowcaseMobileAllyController mobile =
                RequireSingleSceneComponent<ShowcaseMobileAllyController>(scene);
            PrototypeInteractionStateOwner owner =
                RequireSingleSceneComponent<PrototypeInteractionStateOwner>(scene);
            AuthoredEncounterResetController reset =
                RequireSingleSceneComponent<AuthoredEncounterResetController>(scene);
            PrototypeInteractionController interaction =
                owner.GetComponent<PrototypeInteractionController>();
            ShowcaseMobileAllyPresenter presenter =
                RequireSingleSceneComponent<ShowcaseMobileAllyPresenter>(scene);
            CharacterController controller = mobile.GetComponent<CharacterController>();
            if (interaction == null || controller == null)
            {
                throw new InvalidOperationException(
                    "Final Stage requires the accepted S3 player interaction and Ally controller.");
            }

            Vector2[] positions =
            {
                new Vector2(-1.8f, -13.6f),
                new Vector2(0f, -12f),
                new Vector2(3.5f, -6.5f),
                new Vector2(0f, -5f),
                new Vector2(0f, -2.5f),
                new Vector2(-11.5f, -2.5f),
                new Vector2(-18.5f, -3f),
                new Vector2(-19f, 6.5f),
                new Vector2(-19f, 9.5f),
                new Vector2(-19f, 13f),
                new Vector2(-11f, 7f),
                new Vector2(0f, 7f),
                new Vector2(11f, 7f),
                new Vector2(19.5f, 10.8f),
                new Vector2(20f, 14f),
                new Vector2(11.5f, -2.5f),
                new Vector2(18.5f, -3f),
                new Vector2(19f, 7.5f),
                new Vector2(-13f, -11f),
                new Vector2(18f, -12f),
                new Vector2(-13.2f, 7f),
                new Vector2(17.5f, 8.5f)
            };
            var anchors = new Transform[positions.Length];
            for (int index = 0; index < positions.Length; index++)
            {
                anchors[index] = CreateMarker(
                    parent,
                    $"FINAL_FA_{index:00}",
                    new Vector3(positions[index].x, 1f, positions[index].y));
            }

            Vector2Int[] links =
            {
                new Vector2Int(0, 1),
                new Vector2Int(1, 2),
                new Vector2Int(2, 3),
                new Vector2Int(3, 4),
                new Vector2Int(4, 5),
                new Vector2Int(5, 6),
                new Vector2Int(6, 7),
                new Vector2Int(7, 8),
                new Vector2Int(8, 9),
                new Vector2Int(7, 20),
                new Vector2Int(20, 10),
                new Vector2Int(10, 11),
                new Vector2Int(11, 12),
                new Vector2Int(12, 21),
                new Vector2Int(21, 13),
                new Vector2Int(13, 14),
                new Vector2Int(4, 15),
                new Vector2Int(15, 16),
                new Vector2Int(16, 17),
                new Vector2Int(17, 12),
                new Vector2Int(5, 18),
                new Vector2Int(16, 19),
                new Vector2Int(10, 12)
            };

            mobile.Configure(
                owner,
                reset,
                mobile.transform,
                owner.transform,
                controller,
                anchors,
                links);
            interaction.ConfigureShowcaseMobileAlly(mobile);
            presenter.Configure(mobile);
            EditorUtility.SetDirty(mobile);
            EditorUtility.SetDirty(interaction);
            EditorUtility.SetDirty(presenter);
        }

        private static void AttachDesignerBGameplayVisuals(Scene scene, Transform indexParent)
        {
            PrototypeInteractionTarget[] targets =
                GetSceneComponents<PrototypeInteractionTarget>(scene)
                    .OrderBy(target => target.gameObject.name, StringComparer.Ordinal)
                    .ToArray();
            if (targets.Length != 10)
            {
                throw new InvalidOperationException(
                    $"Final Stage requires exactly ten S3 gameplay objects, found {targets.Length}.");
            }

            foreach (PrototypeInteractionTarget target in targets)
            {
                string assetPath;
                Material material;
                string visualName;
                if (target.gameObject.name.Contains("BINDER", StringComparison.Ordinal))
                {
                    assetPath = DesignerBRoot + "/DesignerB_FileBinder.fbx";
                    material = GetMaterial("DesignerBinder");
                    visualName = "VISUAL_DESIGNER_B_FILE_BINDER";
                }
                else if (target.gameObject.name.Contains("CHAIR", StringComparison.Ordinal))
                {
                    assetPath = DesignerBRoot + "/DesignerB_OfficeChair.fbx";
                    material = GetMaterial("DesignerChair");
                    visualName = "VISUAL_DESIGNER_B_OFFICE_CHAIR";
                }
                else if (target.gameObject.name.Contains("DRAWER", StringComparison.Ordinal))
                {
                    assetPath = DesignerBRoot + "/DesignerB_DrawerCabinet.fbx";
                    material = GetMaterial("DesignerDrawer");
                    visualName = "VISUAL_DESIGNER_B_DRAWER_CABINET";
                }
                else if (target.gameObject.name.Contains("COPIER", StringComparison.Ordinal))
                {
                    assetPath = DesignerBRoot + "/DesignerB_Copier.fbx";
                    material = GetMaterial("DesignerCopier");
                    visualName = "VISUAL_DESIGNER_B_COPIER";
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Unsupported final-stage gameplay visual target {target.gameObject.name}.");
                }

                Transform existing = target.transform.Cast<Transform>()
                    .FirstOrDefault(child => child.name.StartsWith(
                        "VISUAL_DESIGNER_B_",
                        StringComparison.Ordinal));
                if (existing != null)
                {
                    UnityEngine.Object.DestroyImmediate(existing.gameObject);
                }

                foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>(true))
                {
                    renderer.enabled = false;
                }

                Collider targetCollider = target.GetComponent<Collider>();
                Vector3 size = targetCollider != null
                    ? targetCollider.bounds.size
                    : Vector3.Max(target.transform.lossyScale, new Vector3(0.1f, 0.1f, 0.1f));
                GameObject visual = InstantiateNormalizedModel(
                    target.transform,
                    assetPath,
                    visualName,
                    target.transform.position,
                    target.transform.rotation,
                    size * 0.94f,
                    material,
                    false);
                visual.transform.SetSiblingIndex(0);
                CreateMarker(indexParent, $"ATTACHED_{target.gameObject.name}", target.transform.position);
            }
        }

        private static void BuildTutorialArchitecture(Transform parent)
        {
            CreateBox(
                parent,
                "STATIC_TUTORIAL_FLOOR_30X24",
                new Vector3(0f, -0.1f, 0f),
                new Vector3(30f, 0.2f, 24f),
                GetMaterial("Floor"),
                true);

            Transform walls = CreateGroup(parent, "TUTORIAL_WALLS_4.2M");
            CreateWall(walls, "WALL_TUTORIAL_WEST", new Vector3(-14.85f, 0f), new Vector2(0.3f, 24f));
            CreateWall(walls, "WALL_TUTORIAL_EAST", new Vector3(14.85f, 0f), new Vector2(0.3f, 24f));
            CreateWall(walls, "WALL_TUTORIAL_SOUTH", new Vector3(0f, -11.85f), new Vector2(30f, 0.3f));
            CreateWall(walls, "WALL_TUTORIAL_NORTH_WEST", new Vector3(-8.5f, 11.85f), new Vector2(13f, 0.3f));
            CreateWall(walls, "WALL_TUTORIAL_NORTH_EAST", new Vector3(8.5f, 11.85f), new Vector2(13f, 0.3f));
            CreateDoorLintel(walls, "DOOR_FRAME_TO_MAIN_STAGE_10", new Vector3(0f, 11.82f), 4f, 0f);

            CreateWall(walls, "WALL_HOLDING_NORTH_WEST", new Vector3(-8.5f, -6f), new Vector2(13f, 0.3f));
            CreateWall(walls, "WALL_HOLDING_NORTH_EAST", new Vector3(8.5f, -6f), new Vector2(13f, 0.3f));
            CreateDoorLintel(walls, "DOOR_FRAME_HOLDING_TO_INPUT", new Vector3(0f, -6f), 4f, 0f);
            CreateWall(walls, "WALL_INPUT_NORTH_WEST", new Vector3(-8.5f, 0f), new Vector2(13f, 0.3f));
            CreateWall(walls, "WALL_INPUT_NORTH_EAST", new Vector3(8.5f, 0f), new Vector2(13f, 0.3f));
            CreateDoorLintel(walls, "DOOR_FRAME_INPUT_TO_COMBAT", new Vector3(0f, 0f), 4f, 0f);
            CreateWall(walls, "WALL_BARRICADE_WEST", new Vector3(-9f, 7f), new Vector2(12f, 0.3f));
            CreateWall(walls, "WALL_BARRICADE_EAST", new Vector3(9f, 7f), new Vector2(12f, 0.3f));
            CreateDoorLintel(walls, "DOOR_FRAME_BARRICADE", new Vector3(0f, 7f), 6f, 0f);

            foreach (Renderer renderer in walls.GetComponentsInChildren<Renderer>(true))
            {
                renderer.sharedMaterial = GetMaterial("Wall");
            }
        }

        private static void BuildTutorialLearningSegments(Transform parent)
        {
            ZoneSpec[] segments =
            {
                new ZoneSpec("SEGMENT_01_BOUND_CHAIR_WAKE", "BOUND CHAIR / WAKE", new Vector2(0f, -9f), new Vector2(28f, 5f)),
                new ZoneSpec("SEGMENT_02_MOVEMENT_INPUT", "WASD / SPACE / SHIFT / LMB / RMB", new Vector2(0f, -3f), new Vector2(28f, 5.5f)),
                new ZoneSpec("SEGMENT_03_COMBAT_STRAP", "COMBAT / STRAP LEARNING", new Vector2(0f, 3.5f), new Vector2(28f, 6.5f)),
                new ZoneSpec("SEGMENT_04_BARRICADE_EXIT", "STRAP BARRICADE / MAIN STAGE 10", new Vector2(0f, 9.5f), new Vector2(28f, 4.5f))
            };

            for (int index = 0; index < segments.Length; index++)
            {
                ZoneSpec segment = segments[index];
                Transform group = CreateGroup(parent, segment.Name);
                CreateRectangleLine(
                    group,
                    "SEGMENT_BOUNDARY",
                    segment.Center,
                    segment.Size,
                    ZoneColors[index],
                    0.10f);
                CreateLabel(
                    group,
                    "SEGMENT_HEADER",
                    segment.Label,
                    new Vector3(segment.Center.x, 0.035f, segment.Center.y),
                    Quaternion.Euler(90f, 0f, 0f),
                    ZoneColors[index],
                    0.10f);
            }
        }

        private static void BuildTutorialFurniture(
            Transform fixedParent,
            Transform dressingParent,
            Transform externalParent)
        {
            Transform collision = CreateGroup(fixedParent, "STATIC_FURNITURE_COLLISION");
            Transform designer = CreateGroup(externalParent, "DESIGNER_B_RUNTIME_VISUALS");
            Transform kenney = CreateGroup(externalParent, "KENNEY_CC0");
            Transform poly = CreateGroup(externalParent, "POLY_HAVEN_CC0");

            InstantiateNormalizedModel(
                designer,
                DesignerBRoot + "/DesignerB_OfficeChair.fbx",
                "DESIGNER_B_BOUND_CHAIR_SOCKET_VISUAL",
                new Vector3(0f, 0f, -9.8f),
                Quaternion.Euler(0f, 180f, 0f),
                new Vector3(0.72f, 1f, 0.70f),
                GetMaterial("DesignerChair"),
                true);

            PlaceStaticModelWithCollider(
                poly,
                collision,
                PolyHavenRoot + "/metal_office_desk_1k.fbx",
                "POLYHAVEN_GUARD_REACTION_DESK",
                new Vector3(-7f, 0f, -8.5f),
                Quaternion.Euler(0f, 90f, 0f),
                new Vector3(1.85f, 0.78f, 0.82f),
                GetMaterial("PolyDesk"));

            Vector3[] coverPositions =
            {
                new Vector3(-8f, 0f, -2f),
                new Vector3(8f, 0f, -2f),
                new Vector3(-9f, 0f, 3.5f),
                new Vector3(9f, 0f, 3.5f)
            };
            for (int index = 0; index < coverPositions.Length; index++)
            {
                PlaceStaticModelWithCollider(
                    kenney,
                    collision,
                    KenneyRoot + "/bookcaseClosed.fbx",
                    $"KENNEY_TUTORIAL_COVER_{index:00}",
                    coverPositions[index],
                    Quaternion.Euler(0f, index % 2 == 0 ? 90f : -90f, 0f),
                    new Vector3(1.55f, 2.1f, 0.45f),
                    GetMaterial("KenneyStorage"));
            }

            for (int index = 0; index < 6; index++)
            {
                InstantiateNormalizedModel(
                    dressingParent,
                    KenneyRoot + "/cardboardBoxClosed.fbx",
                    $"KENNEY_TUTORIAL_BOX_{index:00}",
                    new Vector3(-11f + index * 4.4f, 0f, 5.5f),
                    Quaternion.Euler(0f, index * 19f, 0f),
                    new Vector3(0.62f, 0.52f, 0.52f),
                    GetMaterial("KenneyBox"),
                    true);
            }

            GameObject barricade = CreateBox(
                fixedParent,
                "SOCKET_STRAP_REMOVABLE_BARRICADE",
                new Vector3(0f, 1f, 7f),
                new Vector3(5.8f, 2f, 0.28f),
                GetMaterial("Hook"),
                true);
            barricade.GetComponent<Collider>().isTrigger = true;
        }

        private static void BuildTutorialLighting(Transform parent)
        {
            for (int index = 0; index < 6; index++)
            {
                Vector3 position = new Vector3(index % 2 == 0 ? -6f : 6f, 3.8f, -9f + (index / 2) * 8f);
                GameObject fixture = CreateBox(
                    parent,
                    $"TUTORIAL_CEILING_FIXTURE_{index:00}",
                    position,
                    new Vector3(2.8f, 0.08f, 0.32f),
                    GetMaterial("LightFixture"),
                    false);
                Light light = fixture.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 10f;
                light.intensity = 1.25f;
                light.color = index < 2
                    ? new Color(0.72f, 0.82f, 1f)
                    : new Color(1f, 0.82f, 0.68f);
                light.shadows = LightShadows.Soft;
            }
        }

        private static void BuildTutorialRoute(Transform parent)
        {
            BuildRoute(
                parent,
                "ROUTE_TUTORIAL_TO_MAIN_STAGE_10",
                new Color(0.35f, 0.85f, 1f),
                new[]
                {
                    new RoutePointSpec("P00_BOUND_CHAIR_START", 0f, -9.8f),
                    new RoutePointSpec("P01_GUARD_REACTION", -4f, -8f),
                    new RoutePointSpec("P02_MOVEMENT_INPUT", 0f, -3f),
                    new RoutePointSpec("P03_COMBAT_STRAP", 0f, 3.5f),
                    new RoutePointSpec("P04_BARRICADE", 0f, 7f),
                    new RoutePointSpec("P05_MAIN_STAGE_10_GATE", 0f, 11f)
                });
        }

        private static void BuildTutorialHooks(Scene scene)
        {
            GameObject root = CreateRoot(scene, TutorialHooksRootName);
            var hooks = new Dictionary<string, Vector3>(StringComparer.Ordinal)
            {
                { "HOOK_TUTORIAL_PLAYER_TIED_CHAIR_START", new Vector3(0f, 0f, -9.8f) },
                { "HOOK_NON_HOSTILE_GUARD_REACTION_AREA", new Vector3(-4f, 0f, -8f) },
                { "HOOK_INPUT_WASD", new Vector3(-6f, 0f, -3f) },
                { "HOOK_INPUT_SPACE", new Vector3(-3f, 0f, -3f) },
                { "HOOK_INPUT_SHIFT", new Vector3(0f, 0f, -3f) },
                { "HOOK_INPUT_LMB", new Vector3(3f, 0f, -3f) },
                { "HOOK_INPUT_RMB", new Vector3(6f, 0f, -3f) },
                { "HOOK_COMBAT_STRAP_LEARNING", new Vector3(0f, 0f, 3.5f) },
                { "HOOK_GUARD_CLEAR_GATE", new Vector3(0f, 0f, 6f) },
                { "HOOK_STRAP_BARRICADE", new Vector3(0f, 0f, 7f) },
                { "HOOK_MAIN_STAGE_10_TRANSITION", new Vector3(0f, 0f, 11f) },
                { "HOOK_TUTORIAL_RESET_BASELINE", new Vector3(0f, 0f, -9.8f) }
            };

            foreach (KeyValuePair<string, Vector3> hook in hooks)
            {
                CreateHook(
                    root.transform,
                    hook.Key,
                    hook.Value,
                    hook.Key.Contains("AREA") || hook.Key.Contains("LEARNING")
                        ? new Vector3(5f, 2f, 5f)
                        : new Vector3(1.5f, 2f, 1.5f),
                    new Color(0.35f, 0.85f, 1f));
            }

            CreateMarker(root.transform, "NO_TUTORIAL_RUNTIME_STATE_MACHINE", Vector3.zero);
            CreateMarker(root.transform, "NO_NEW_INPUT_BINDING", Vector3.zero);
        }

        private static void RepositionTutorialGameplay(Scene scene)
        {
            GameObject player = RequireSingleSceneComponent<PrototypeInteractionStateOwner>(scene)
                .gameObject;
            SetActorPosition(player, new Vector3(0f, 1f, -9f), Quaternion.identity);

            GameObject ally = RequireRootAny(
                scene,
                "GAMEPLAY_CHR01_MOBILE_ALLY",
                "AllyProxy");
            ally.SetActive(false);

            PrototypeInteractionTarget[] targets = GetSceneComponents<PrototypeInteractionTarget>(scene);
            foreach (PrototypeInteractionTarget target in targets)
            {
                target.gameObject.SetActive(false);
            }

            GameObject runnerOne = RequireRootAny(
                scene,
                "GAMEPLAY_ENY01_RUNNER_01",
                "EnemyProxy_01");
            GameObject runnerTwo = RequireRootAny(
                scene,
                "GAMEPLAY_ENY01_RUNNER_02",
                "EnemyProxy_02");
            SetActorPosition(runnerOne, new Vector3(-4f, 1f, -8f), Quaternion.Euler(0f, 90f, 0f));
            SetActorPosition(runnerTwo, new Vector3(4f, 1f, 3.5f), Quaternion.Euler(0f, -90f, 0f));
        }

        private static void DisableTutorialGameplaySystems(Scene scene)
        {
            foreach (MonoBehaviour behaviour in GetSceneComponents<MonoBehaviour>(scene))
            {
                string typeName = behaviour.GetType().Name;
                if (typeName.StartsWith("Showcase", StringComparison.Ordinal))
                {
                    behaviour.enabled = false;
                    EditorUtility.SetDirty(behaviour);
                }
            }
        }

        private static void StripLegacyS7ObjectiveOnly(Scene scene)
        {
            string[] componentNames =
            {
                "ShowcaseDrawerObjectiveReceiver",
                "ShowcaseObjectivePresenter"
            };
            foreach (MonoBehaviour component in GetSceneComponents<MonoBehaviour>(scene).ToArray())
            {
                if (componentNames.Contains(component.GetType().Name, StringComparer.Ordinal))
                {
                    UnityEngine.Object.DestroyImmediate(component);
                }
            }
        }

        private static void RemoveReplacedEnvironment(Scene scene)
        {
            foreach (string rootName in ReplacedEnvironmentRoots)
            {
                GameObject root = FindRoot(scene, rootName);
                if (root != null)
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }

        private static void BuildMaterials()
        {
            Materials.Clear();
            AddMaterial("Floor", EnsureMaterial(
                "M_FinalStage_Floor",
                new Color(0.48f, 0.50f, 0.54f),
                0f,
                0.28f,
                PolyHavenRoot + "/smooth_concrete_floor_diff_1k.jpg",
                PolyHavenRoot + "/smooth_concrete_floor_nor_gl_1k.jpg",
                new Vector2(12f, 8f)));
            AddMaterial("Wall", EnsureMaterial(
                "M_FinalStage_Wall",
                new Color(0.23f, 0.27f, 0.33f),
                0f,
                0.24f));
            AddMaterial("Partition", EnsureMaterial(
                "M_FinalStage_Partition",
                new Color(0.18f, 0.43f, 0.62f),
                0.05f,
                0.32f));
            AddMaterial("PolyDesk", EnsureMaterial(
                "M_PolyHaven_MetalOfficeDesk",
                Color.white,
                0.74f,
                0.23f,
                PolyHavenRoot + "/metal_office_desk_diff_1k.jpg",
                PolyHavenRoot + "/metal_office_desk_nor_gl_1k.jpg"));
            AddMaterial("PolyPaper", EnsureMaterial(
                "M_PolyHaven_OfficeNotepads",
                Color.white,
                0f,
                0.18f,
                PolyHavenRoot + "/office_notepads_diff_1k.jpg",
                PolyHavenRoot + "/office_notepads_nor_gl_1k.jpg"));
            AddMaterial("DesignerBinder", EnsureMaterial(
                "M_DesignerB_Binder",
                new Color(0.84f, 0.18f, 0.12f),
                0f,
                0.28f));
            AddMaterial("DesignerChair", EnsureMaterial(
                "M_DesignerB_Chair",
                new Color(0.08f, 0.11f, 0.15f),
                0.32f,
                0.38f));
            AddMaterial("DesignerDrawer", EnsureMaterial(
                "M_DesignerB_Drawer",
                new Color(0.31f, 0.36f, 0.42f),
                0.68f,
                0.24f));
            AddMaterial("DesignerCopier", EnsureMaterial(
                "M_DesignerB_Copier",
                new Color(0.72f, 0.74f, 0.76f),
                0.18f,
                0.30f));
            AddMaterial("KenneyFurniture", EnsureMaterial(
                "M_Kenney_Furniture",
                new Color(0.35f, 0.27f, 0.21f),
                0.05f,
                0.30f));
            AddMaterial("KenneyAccent", EnsureMaterial(
                "M_Kenney_Accent",
                new Color(0.18f, 0.43f, 0.52f),
                0.10f,
                0.35f));
            AddMaterial("KenneyStorage", EnsureMaterial(
                "M_Kenney_Storage",
                new Color(0.24f, 0.29f, 0.33f),
                0.40f,
                0.20f));
            AddMaterial("KenneyBox", EnsureMaterial(
                "M_Kenney_Box",
                new Color(0.48f, 0.32f, 0.18f),
                0f,
                0.18f));
            AddMaterial("KenneyMonitor", EnsureMaterial(
                "M_Kenney_Monitor",
                new Color(0.06f, 0.09f, 0.12f),
                0.24f,
                0.42f));
            AddMaterial("KenneyPlant", EnsureMaterial(
                "M_Kenney_Plant",
                new Color(0.18f, 0.46f, 0.24f),
                0f,
                0.24f));
            AddMaterial("LightFixture", EnsureMaterial(
                "M_FinalStage_LightFixture",
                new Color(0.80f, 0.90f, 1f),
                0f,
                0.15f,
                null,
                null,
                Vector2.one,
                new Color(0.32f, 0.38f, 0.46f)));
            AddMaterial("Hook", EnsureMaterial(
                "M_FinalStage_Hook",
                new Color(1f, 0.62f, 0.12f),
                0.05f,
                0.32f,
                null,
                null,
                Vector2.one,
                new Color(0.45f, 0.18f, 0.02f)));
        }

        private static Material EnsureMaterial(
            string name,
            Color color,
            float metallic,
            float smoothness,
            string diffusePath = null,
            string normalPath = null,
            Vector2 textureScale = default,
            Color emission = default)
        {
            string path = MaterialRoot + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Standard");
            if (shader == null)
            {
                throw new InvalidOperationException("Built-in Standard shader is required.");
            }
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }

            material.SetColor("_Color", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Glossiness", smoothness);
            if (!string.IsNullOrEmpty(diffusePath))
            {
                Texture2D diffuse = RequireAsset<Texture2D>(diffusePath);
                material.SetTexture("_MainTex", diffuse);
                material.SetTextureScale(
                    "_MainTex",
                    textureScale == default ? Vector2.one : textureScale);
            }
            if (!string.IsNullOrEmpty(normalPath))
            {
                Texture2D normal = RequireAsset<Texture2D>(normalPath);
                material.SetTexture("_BumpMap", normal);
                material.EnableKeyword("_NORMALMAP");
            }
            if (emission.maxColorComponent > 0f)
            {
                material.SetColor("_EmissionColor", emission);
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ConfigureNormalMap(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException($"Texture importer missing: {path}");
            }
            if (importer.textureType != TextureImporterType.NormalMap)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.SaveAndReimport();
            }
        }

        private static void RequireExternalAssets()
        {
            string[] modelPaths =
            {
                DesignerBRoot + "/DesignerB_Copier.fbx",
                DesignerBRoot + "/DesignerB_DrawerCabinet.fbx",
                DesignerBRoot + "/DesignerB_FileBinder.fbx",
                DesignerBRoot + "/DesignerB_OfficeChair.fbx",
                PolyHavenRoot + "/metal_office_desk_1k.fbx",
                PolyHavenRoot + "/office_notepads_1k.fbx",
                KenneyRoot + "/bookcaseClosed.fbx",
                KenneyRoot + "/cardboardBoxClosed.fbx",
                KenneyRoot + "/chairModernFrameCushion.fbx",
                KenneyRoot + "/computerScreen.fbx",
                KenneyRoot + "/plantSmall1.fbx",
                KenneyRoot + "/pottedPlant.fbx",
                KenneyRoot + "/table.fbx",
                KenneyRoot + "/trashcan.fbx",
                KenneyRoot + "/coatRackStanding.fbx",
                KenneyRoot + "/kitchenCoffeeMachine.fbx"
            };
            foreach (string path in modelPaths)
            {
                RequireAsset<GameObject>(path);
            }
            string[] texturePaths =
            {
                PolyHavenRoot + "/metal_office_desk_diff_1k.jpg",
                PolyHavenRoot + "/metal_office_desk_nor_gl_1k.jpg",
                PolyHavenRoot + "/office_notepads_diff_1k.jpg",
                PolyHavenRoot + "/office_notepads_nor_gl_1k.jpg",
                PolyHavenRoot + "/smooth_concrete_floor_diff_1k.jpg",
                PolyHavenRoot + "/smooth_concrete_floor_nor_gl_1k.jpg"
            };
            foreach (string path in texturePaths)
            {
                RequireAsset<Texture2D>(path);
            }
        }

        private static void AddMaterial(string key, Material material)
        {
            Materials.Add(key, material);
        }

        private static Material GetMaterial(string key)
        {
            if (!Materials.TryGetValue(key, out Material material) || material == null)
            {
                throw new InvalidOperationException($"Final Stage material missing: {key}");
            }
            return material;
        }

        private static void CreateWall(
            Transform parent,
            string name,
            Vector2 center,
            Vector2 footprint)
        {
            CreateBox(
                parent,
                name,
                new Vector3(center.x, 2.1f, center.y),
                new Vector3(footprint.x, 4.2f, footprint.y),
                GetMaterial("Wall"),
                true);
        }

        private static void CreatePartition(
            Transform parent,
            string name,
            Vector2 center,
            Vector2 footprint)
        {
            CreateBox(
                parent,
                name,
                new Vector3(center.x, 1.05f, center.y),
                new Vector3(footprint.x, 2.1f, footprint.y),
                GetMaterial("Partition"),
                true);
        }

        private static void CreateDoorLintel(
            Transform parent,
            string name,
            Vector2 center,
            float width,
            float yaw)
        {
            Vector3 scale = Mathf.Abs(Mathf.DeltaAngle(yaw, 90f)) < 1f
                ? new Vector3(0.3f, 0.4f, width)
                : new Vector3(width, 0.4f, 0.3f);
            GameObject lintel = CreateBox(
                parent,
                name,
                new Vector3(center.x, 4f, center.y),
                scale,
                GetMaterial("Wall"),
                true);
            lintel.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private static void PlaceStaticModelWithCollider(
            Transform visualParent,
            Transform collisionParent,
            string assetPath,
            string name,
            Vector3 position,
            Quaternion rotation,
            Vector3 size,
            Material material)
        {
            InstantiateNormalizedModel(
                visualParent,
                assetPath,
                name,
                position,
                rotation,
                size,
                material,
                true);
            CreateInvisibleCollider(
                collisionParent,
                "STATIC_COLLIDER_" + name,
                position + Vector3.up * (size.y * 0.5f),
                rotation,
                Vector3.Scale(size, new Vector3(0.92f, 0.96f, 0.90f)));
        }

        private static GameObject InstantiateNormalizedModel(
            Transform parent,
            string assetPath,
            string name,
            Vector3 position,
            Quaternion rotation,
            Vector3 targetSize,
            Material material,
            bool markStatic)
        {
            GameObject prefab = RequireAsset<GameObject>(assetPath);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            if (instance == null)
            {
                instance = UnityEngine.Object.Instantiate(prefab, parent);
            }
            instance.name = name;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                UnityEngine.Object.DestroyImmediate(instance);
                throw new InvalidOperationException($"External model has no renderer: {assetPath}");
            }

            Bounds initial = CombineBounds(renderers);
            if (initial.size.x < 0.0001f || initial.size.y < 0.0001f || initial.size.z < 0.0001f)
            {
                UnityEngine.Object.DestroyImmediate(instance);
                throw new InvalidOperationException($"External model has invalid bounds: {assetPath}");
            }

            instance.transform.localScale = Vector3.Scale(
                instance.transform.localScale,
                new Vector3(
                    targetSize.x / initial.size.x,
                    targetSize.y / initial.size.y,
                    targetSize.z / initial.size.z));
            instance.transform.SetPositionAndRotation(position, rotation);
            Bounds fitted = CombineBounds(renderers);
            instance.transform.position += new Vector3(
                position.x - fitted.center.x,
                position.y - fitted.min.y,
                position.z - fitted.center.z);

            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = true;
                renderer.sharedMaterials = Enumerable.Repeat(
                    material,
                    Mathf.Max(1, renderer.sharedMaterials.Length)).ToArray();
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
                renderer.lightProbeUsage = LightProbeUsage.BlendProbes;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
            }

            if (markStatic)
            {
                foreach (Transform transform in instance.GetComponentsInChildren<Transform>(true))
                {
                    GameObjectUtility.SetStaticEditorFlags(
                        transform.gameObject,
                        StaticEditorFlags.BatchingStatic |
                        StaticEditorFlags.OccluderStatic |
                        StaticEditorFlags.OccludeeStatic |
                        StaticEditorFlags.ReflectionProbeStatic);
                }
            }
            return instance;
        }

        private static Bounds CombineBounds(IReadOnlyList<Renderer> renderers)
        {
            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Count; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }
            return bounds;
        }

        private static void CreateInvisibleCollider(
            Transform parent,
            string name,
            Vector3 position,
            Quaternion rotation,
            Vector3 size)
        {
            Transform marker = CreateMarker(parent, name, position);
            marker.rotation = rotation;
            BoxCollider collider = marker.gameObject.AddComponent<BoxCollider>();
            collider.size = size;
            GameObjectUtility.SetStaticEditorFlags(
                marker.gameObject,
                StaticEditorFlags.OccluderStatic |
                StaticEditorFlags.OccludeeStatic);
        }

        private static void BuildRoute(
            Transform parent,
            string name,
            Color color,
            IReadOnlyList<RoutePointSpec> points)
        {
            Transform route = CreateGroup(parent, name);
            var positions = new Vector3[points.Count];
            for (int index = 0; index < points.Count; index++)
            {
                positions[index] = new Vector3(points[index].X, 0.06f, points[index].Z);
                CreateMarker(route, points[index].Name, positions[index]);
            }
            CreateReviewLine(route, "ROUTE_LINE", positions, color, 0.12f, false);
        }

        private static void CreateRectangleLine(
            Transform parent,
            string name,
            Vector2 center,
            Vector2 size,
            Color color,
            float width)
        {
            float halfX = size.x * 0.5f;
            float halfZ = size.y * 0.5f;
            CreateReviewLine(
                parent,
                name,
                new[]
                {
                    new Vector3(center.x - halfX, 0.04f, center.y - halfZ),
                    new Vector3(center.x + halfX, 0.04f, center.y - halfZ),
                    new Vector3(center.x + halfX, 0.04f, center.y + halfZ),
                    new Vector3(center.x - halfX, 0.04f, center.y + halfZ)
                },
                color,
                width,
                true);
        }

        private static LineRenderer CreateReviewLine(
            Transform parent,
            string name,
            IReadOnlyList<Vector3> points,
            Color color,
            float width,
            bool loop)
        {
            Transform lineTransform = CreateGroup(parent, name);
            LineRenderer line = lineTransform.gameObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.loop = loop;
            line.positionCount = points.Count;
            for (int index = 0; index < points.Count; index++)
            {
                line.SetPosition(index, points[index]);
            }
            line.startWidth = width;
            line.endWidth = width;
            line.startColor = color;
            line.endColor = color;
            line.numCornerVertices = 2;
            line.numCapVertices = 2;
            line.alignment = LineAlignment.View;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            Material lineMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Line.mat");
            if (lineMaterial != null)
            {
                line.sharedMaterial = lineMaterial;
            }
            return line;
        }

        private static TextMesh CreateLabel(
            Transform parent,
            string name,
            string text,
            Vector3 position,
            Quaternion rotation,
            Color color,
            float characterSize)
        {
            Transform transform = CreateMarker(parent, name, position);
            transform.rotation = rotation;
            TextMesh label = transform.gameObject.AddComponent<TextMesh>();
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = characterSize;
            label.fontSize = 64;
            label.fontStyle = FontStyle.Bold;
            label.color = color;
            label.richText = false;
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                label.font = font;
                MeshRenderer renderer = label.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = font.material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
            return label;
        }

        private static void CreateHook(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 triggerSize,
            Color color)
        {
            Transform hook = CreateMarker(parent, name, position);
            BoxCollider trigger = hook.gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, triggerSize.y * 0.5f, 0f);
            trigger.size = triggerSize;
            GameObject marker = CreateBox(
                hook,
                "VISIBLE_REVIEW_MARKER",
                position + new Vector3(0f, 0.06f, 0f),
                new Vector3(Mathf.Min(triggerSize.x, 1.25f), 0.08f, Mathf.Min(triggerSize.z, 1.25f)),
                GetMaterial("Hook"),
                false);
            marker.GetComponent<Renderer>().sharedMaterial.color = color;
        }

        private static GameObject CreateBox(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            Material material,
            bool collidable)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.position = position;
            cube.transform.localScale = scale;
            if (!collidable)
            {
                UnityEngine.Object.DestroyImmediate(cube.GetComponent<Collider>());
            }
            Renderer renderer = cube.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }
            GameObjectUtility.SetStaticEditorFlags(
                cube,
                StaticEditorFlags.BatchingStatic |
                StaticEditorFlags.OccluderStatic |
                StaticEditorFlags.OccludeeStatic);
            return cube;
        }

        private static void SetActorPosition(
            GameObject actor,
            Vector3 position,
            Quaternion rotation)
        {
            CharacterController controller = actor.GetComponent<CharacterController>();
            bool enabled = controller != null && controller.enabled;
            if (enabled)
            {
                controller.enabled = false;
            }
            actor.transform.SetPositionAndRotation(position, rotation);
            if (enabled)
            {
                controller.enabled = true;
            }
            EditorUtility.SetDirty(actor.transform);
        }

        private static void RepositionSceneCamera(Scene scene, Vector3 position)
        {
            Camera camera = RequireSingleSceneComponent<Camera>(scene);
            camera.transform.position = position;
            camera.transform.rotation = Quaternion.Euler(18f, 155f, 0f);
            camera.nearClipPlane = Mathf.Min(camera.nearClipPlane, 0.08f);
            EditorUtility.SetDirty(camera);
            EditorUtility.SetDirty(camera.transform);
        }

        private static void CaptureFrame(
            Scene scene,
            string outputPath,
            Vector3 position,
            Quaternion rotation,
            bool orthographic,
            float orthographicSize,
            float fieldOfView)
        {
            GameObject cameraObject = new GameObject("TEMP_FINAL_STAGE_CAPTURE_CAMERA");
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            cameraObject.transform.SetPositionAndRotation(position, rotation);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.035f, 0.05f, 1f);
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 120f;
            camera.fieldOfView = fieldOfView;
            camera.orthographic = orthographic;
            camera.orthographicSize = orthographicSize;
            camera.aspect = 16f / 9f;
            camera.allowHDR = true;
            camera.allowMSAA = true;

            RenderTexture renderTexture = new RenderTexture(
                1920,
                1080,
                24,
                RenderTextureFormat.ARGB32)
            {
                name = "FINAL_STAGE_CAPTURE_1920x1080",
                antiAliasing = 1
            };
            Texture2D image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            RenderTexture previous = RenderTexture.active;
            try
            {
                renderTexture.Create();
                camera.targetTexture = renderTexture;
                camera.Render();
                RenderTexture.active = renderTexture;
                image.ReadPixels(new Rect(0f, 0f, 1920f, 1080f), 0, 0, false);
                image.Apply(false, false);
                byte[] png = image.EncodeToPNG();
                if (png == null || png.Length == 0)
                {
                    throw new InvalidOperationException($"Could not encode {outputPath}.");
                }
                File.WriteAllBytes(outputPath, png);
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(image);
                UnityEngine.Object.DestroyImmediate(renderTexture);
                UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }

        private static void EnsureFolderPath(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    string guid = AssetDatabase.CreateFolder(current, parts[index]);
                    if (string.IsNullOrWhiteSpace(guid))
                    {
                        throw new InvalidOperationException($"Could not create asset folder {next}.");
                    }
                }
                current = next;
            }
        }

        private static T RequireAsset<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new InvalidOperationException($"Required asset missing: {path}");
            }
            return asset;
        }

        private static void ValidateLoadedScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new ArgumentException("A valid loaded Scene is required.", nameof(scene));
            }
        }

        private static GameObject CreateRoot(Scene scene, string name)
        {
            GameObject root = new GameObject(name);
            SceneManager.MoveGameObjectToScene(root, scene);
            return root;
        }

        private static Transform CreateGroup(Transform parent, string name)
        {
            Transform group = new GameObject(name).transform;
            group.SetParent(parent, false);
            return group;
        }

        private static Transform CreateMarker(Transform parent, string name, Vector3 position)
        {
            Transform marker = CreateGroup(parent, name);
            marker.position = position;
            return marker;
        }

        private static Transform FindDirectChild(Transform parent, string name)
        {
            return parent.Cast<Transform>()
                .FirstOrDefault(child => string.Equals(child.name, name, StringComparison.Ordinal));
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            return scene.GetRootGameObjects()
                .FirstOrDefault(root => string.Equals(root.name, name, StringComparison.Ordinal));
        }

        private static GameObject RequireRootAny(Scene scene, params string[] names)
        {
            foreach (string name in names)
            {
                GameObject root = FindRoot(scene, name);
                if (root != null)
                {
                    return root;
                }
            }
            throw new InvalidOperationException(
                "Required Scene root missing: " + string.Join(" or ", names));
        }

        private static T[] GetSceneComponents<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }

        private static T RequireSingleSceneComponent<T>(Scene scene) where T : Component
        {
            T[] components = GetSceneComponents<T>(scene);
            if (components.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Scene requires exactly one {typeof(T).Name}, found {components.Length}.");
            }
            return components[0];
        }

        private readonly struct ZoneSpec
        {
            public ZoneSpec(string name, string label, Vector2 center, Vector2 size)
            {
                Name = name;
                Label = label;
                Center = center;
                Size = size;
            }
            public string Name { get; }
            public string Label { get; }
            public Vector2 Center { get; }
            public Vector2 Size { get; }
        }

        private readonly struct RoutePointSpec
        {
            public RoutePointSpec(string name, float x, float z)
            {
                Name = name;
                X = x;
                Z = z;
            }
            public string Name { get; }
            public float X { get; }
            public float Z { get; }
        }
    }
}
