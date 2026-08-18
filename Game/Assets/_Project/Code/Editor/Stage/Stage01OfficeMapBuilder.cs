using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace BreakNotice.Stage.Editor
{
    public static class Stage01OfficeMapBuilder
    {
        public const string ScenePath =
            "Assets/_Project/Scenes/20_VerticalSlice/Stage01_OfficeMap.unity";

        private const string MaterialRoot = "Assets/_Project/Art/Materials/Stage01";
        private const string RenderSettingsRoot = "Assets/_Project/Settings/Stage01";
        private const string RendererDataPath =
            RenderSettingsRoot + "/Stage01_UniversalRenderer.asset";
        private const string PipelineAssetPath =
            RenderSettingsRoot + "/Stage01_UniversalRenderPipeline.asset";
        private const string EnvironmentRoot = "Assets/_Project/Art/Environment/OfficeCore";
        private const string GameplayPropRoot = "Assets/_Project/Art/Props/Gameplay";

        private const string Floor = EnvironmentRoot + "/Architecture/floor.fbx";
        private const string Wall2 = EnvironmentRoot + "/Architecture/Wall/ENV_Wall_2m_A_v001.fbx";
        private const string Wall4 = EnvironmentRoot + "/Architecture/Wall/ENV_Wall_4m_A_v001.fbx";
        private const string Wall6 = EnvironmentRoot + "/Architecture/Wall/ENV_Wall_6m_A_v001.fbx";
        private const string WallCorner = EnvironmentRoot + "/Architecture/Wall/ENV_Wall_Corner_A_v001.fbx";
        private const string WallPillar = EnvironmentRoot + "/Architecture/Wall/ENV_Wall_pillar_v001.fbx";
        private const string LowPartition1 = EnvironmentRoot + "/Architecture/LowPartition/ENV_LowPartition_1m_v001.fbx";
        private const string LowPartition2 = EnvironmentRoot + "/Architecture/LowPartition/ENV_LowPartition_2m_v001.fbx";
        private const string LowPartition3 = EnvironmentRoot + "/Architecture/LowPartition/ENV_LowPartition_3m_v001.fbx";
        private const string LowPartitionT = EnvironmentRoot + "/Architecture/LowPartition/ENV_LowPartition_TJunction_v001.fbx";

        private const string DeskA = EnvironmentRoot + "/Furniture/Desk/ENV_FixedDesk_A_v001.fbx";
        private const string DeskB = EnvironmentRoot + "/Furniture/Desk/ENV_FixedDesk_B_v001.fbx";
        private const string DeskC = EnvironmentRoot + "/Furniture/Desk/ENV_FixedDesk_C_v001.fbx";
        private const string Cabinet = EnvironmentRoot + "/Furniture/Common/ENV_cabinet_v001.fbx";
        private const string Door = EnvironmentRoot + "/Furniture/Common/ENV_door_v001.fbx";

        private const string BoxA = EnvironmentRoot + "/Decorative/ENV_Box_A_v001.fbx";
        private const string BoxB = EnvironmentRoot + "/Decorative/ENV_Box_B_v001.fbx";
        private const string BoxC = EnvironmentRoot + "/Decorative/ENV_Box_C_v001.fbx";
        private const string Cart = EnvironmentRoot + "/Decorative/ENV_cart_v001.fbx";
        private const string DeskLamp = EnvironmentRoot + "/Decorative/ENV_desklamp_v001.fbx";
        private const string Keyboard = EnvironmentRoot + "/Decorative/ENV_keyboard_v001.fbx";
        private const string CeilingLight = EnvironmentRoot + "/Decorative/ENV_light_v001.fbx";
        private const string Shelf = EnvironmentRoot + "/Decorative/ENV_metal_shelving_v001.fbx";
        private const string Monitor = EnvironmentRoot + "/Decorative/ENV_Monitor_v001.fbx";
        private const string Papers = EnvironmentRoot + "/Decorative/ENV_papers_A_v001.fbx";
        private const string Phone = EnvironmentRoot + "/Decorative/ENV_phone_v001.fbx";
        private const string Plant = EnvironmentRoot + "/Decorative/ENV_plant_A_v001.fbx";
        private const string TrashCan = EnvironmentRoot + "/Decorative/ENV_trashcan_v001.fbx";
        private const string WaterDispenser = EnvironmentRoot + "/Decorative/ENV_WaterDispenser_v001.fbx";

        private const string BinderA = GameplayPropRoot + "/PRP_FileBinder/PRP_FileBinder_A.fbx";
        private const string BinderB = GameplayPropRoot + "/PRP_FileBinder/PRP_FileBinder_B.fbx";
        private const string BinderC = GameplayPropRoot + "/PRP_FileBinder/PRP_FileBinder_C.fbx";
        private const string Chair = GameplayPropRoot + "/PRP_OfficeChair/PRP_OfficeChair_A_v001.fbx";
        private const string Drawer = GameplayPropRoot + "/PRP_DrawerCabinet/PRP_DrawerCabinet_v001.fbx";
        private const string Copier = GameplayPropRoot + "/PRP_Copier/PRP_Copier.fbx";

        private static readonly Dictionary<string, Material> Materials =
            new Dictionary<string, Material>(StringComparer.Ordinal);

        private enum PhysicsKind
        {
            None,
            Static,
            Kinematic
        }

        [MenuItem("BREAK NOTICE/Stage/Build Stage 01 Office Map %#m")]
        public static void Build()
        {
            RequireAssets();
            EnsureFolder("Assets/_Project/Scenes/20_VerticalSlice");
            EnsureFolder(MaterialRoot);
            ConfigureUniversalRenderPipeline();
            CreateMaterials();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject stageRoot = new GameObject("Stage01_OfficeMap");
            SceneManager.MoveGameObjectToScene(stageRoot, scene);

            Transform architecture = CreateGroup("01_Architecture", stageRoot.transform);
            Transform zonesRoot = CreateGroup("02_Zones", stageRoot.transform);
            Transform lighting = CreateGroup("03_Lighting", stageRoot.transform);
            Transform routes = CreateGroup("04_RouteMarkers", stageRoot.transform);

            Dictionary<int, Transform> zones = CreateZones(zonesRoot);
            BuildArchitecture(architecture);
            BuildRouteMarkers(routes);
            BuildZone01(zones[1]);
            BuildZone02(zones[2]);
            BuildZone03(zones[3]);
            BuildZone04(zones[4]);
            BuildZone05(zones[5]);
            BuildZone06(zones[6]);
            BuildZone07(zones[7]);
            BuildZone08(zones[8]);
            BuildZone09(zones[9]);
            BuildZone10(zones[10]);
            BuildZone11(zones[11]);
            BuildSideLanes(zonesRoot);
            BuildLighting(lighting);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"Could not save {ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ValidateScene();
            Debug.Log($"Stage 01 office map built successfully: {ScenePath}");
        }

        public static void BuildFromCommandLine()
        {
            Build();
        }

        [MenuItem("BREAK NOTICE/Stage/Validate Stage 01 Office Map")]
        public static void ValidateScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject root = scene.GetRootGameObjects()
                .SingleOrDefault(item => item.name == "Stage01_OfficeMap");
            if (root == null)
            {
                throw new InvalidOperationException("Stage01_OfficeMap root is missing.");
            }

            for (int index = 1; index <= 11; index++)
            {
                string zoneName = $"Zone_{index:00}";
                if (FindDescendant(root.transform, zoneName) == null)
                {
                    throw new InvalidOperationException($"Required zone is missing: {zoneName}");
                }
            }

            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            Rigidbody[] rigidbodies = root.GetComponentsInChildren<Rigidbody>(true);
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (colliders.Length < 170)
            {
                throw new InvalidOperationException(
                    $"Expected at least 170 colliders, found {colliders.Length}.");
            }

            if (rigidbodies.Length < 30)
            {
                throw new InvalidOperationException(
                    $"Expected at least 30 rigidbodies, found {rigidbodies.Length}.");
            }

            if (renderers.Length < 220)
            {
                throw new InvalidOperationException(
                    $"Expected at least 220 renderers, found {renderers.Length}.");
            }

            Transform[] fixedTransforms = root.GetComponentsInChildren<Transform>(true)
                .Where(item => item.name.StartsWith("FIXED_Wall_", StringComparison.Ordinal) ||
                    item.name.StartsWith("FIXED_Door_", StringComparison.Ordinal) ||
                    item.name.StartsWith("FIXED_LowPartition_", StringComparison.Ordinal))
                .ToArray();
            if (fixedTransforms.Any(item => !Approximately(item.localScale, Vector3.one)))
            {
                throw new InvalidOperationException(
                    "A fixed wall, door or LowPartition root has non-unit scale.");
            }

            foreach (Rigidbody body in rigidbodies)
            {
                if (body.GetComponentsInChildren<Collider>(true).Length == 0)
                {
                    throw new InvalidOperationException(
                        $"Rigidbody has no collider in its hierarchy: {body.name}");
                }
            }

            Debug.Log(
                $"Stage 01 validation passed: zones=11, renderers={renderers.Length}, " +
                $"colliders={colliders.Length}, rigidbodies={rigidbodies.Length}, " +
                $"fixedRoots={fixedTransforms.Length}.");
        }

        private static Dictionary<int, Transform> CreateZones(Transform parent)
        {
            Dictionary<int, Transform> result = new Dictionary<int, Transform>();
            for (int index = 1; index <= 11; index++)
            {
                Transform zone = CreateGroup($"Zone_{index:00}", parent);
                result.Add(index, zone);
            }

            return result;
        }

        private static void BuildArchitecture(Transform parent)
        {
            Transform floorRoot = CreateGroup("Floor", parent);
            GameObject floor = PlaceModel(
                Floor,
                "Floor_Office_60x44m",
                floorRoot,
                Vector3.zero,
                0f,
                0f,
                PhysicsKind.Static,
                "Floor",
                false);
            FitFootprint(floor, 60f, 44f);
            AlignBottom(floor, 0f);
            ReplaceCollider(floor);

            GameObject safetyFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            safetyFloor.name = "WalkableFloorCollider";
            safetyFloor.transform.SetParent(floorRoot, false);
            safetyFloor.transform.position = new Vector3(0f, -0.11f, 0f);
            safetyFloor.transform.localScale = new Vector3(60f, 0.2f, 44f);
            UnityEngine.Object.DestroyImmediate(safetyFloor.GetComponent<MeshRenderer>());
            safetyFloor.isStatic = true;

            Transform outer = CreateGroup("OuterShell", parent);
            for (int index = 0; index < 10; index++)
            {
                float x = -27f + index * 6f;
                PlaceFixed(Wall6, $"Wall_N_{index:00}", outer, new Vector3(x, 0f, 22f), 0f);
                PlaceFixed(Wall6, $"Wall_S_{index:00}", outer, new Vector3(x, 0f, -22f), 180f);
            }

            for (int index = 0; index < 7; index++)
            {
                float z = -18f + index * 6f;
                PlaceFixed(Wall6, $"Wall_W_{index:00}", outer, new Vector3(-30f, 0f, z), 90f);
                PlaceFixed(Wall6, $"Wall_E_{index:00}", outer, new Vector3(30f, 0f, z), -90f);
            }

            PlaceFixed(WallCorner, "Wall_Corner_NW", outer, new Vector3(-30f, 0f, 22f), 0f);
            PlaceFixed(WallCorner, "Wall_Corner_NE", outer, new Vector3(30f, 0f, 22f), 90f);
            PlaceFixed(WallCorner, "Wall_Corner_SW", outer, new Vector3(-30f, 0f, -22f), -90f);
            PlaceFixed(WallCorner, "Wall_Corner_SE", outer, new Vector3(30f, 0f, -22f), 180f);

            Transform dividers = CreateGroup("RoomDividers", parent);
            foreach (float x in new[] { -27f, -21f, -15f, 15f, 21f, 27f })
            {
                PlaceFixed(Wall6, $"Wall_NorthDivider_{x:0}", dividers, new Vector3(x, 0f, 12f), 0f);
            }

            foreach (float z in new[] { 15f, 21f })
            {
                PlaceFixed(Wall6, $"Wall_Zone01Side_{z:0}", dividers, new Vector3(-10f, 0f, z), 90f);
                PlaceFixed(Wall6, $"Wall_Zone03Side_{z:0}", dividers, new Vector3(10f, 0f, z), 90f);
            }

            foreach (float x in new[] { -27f, -21f, -15f, 15f, 21f, 27f })
            {
                PlaceFixed(Wall6, $"Wall_SouthDivider_{x:0}", dividers, new Vector3(x, 0f, -14f), 0f);
            }

            foreach (float z in new[] { -19f, -15f })
            {
                PlaceFixed(Wall4, $"Wall_Reception_W_{z:0}", dividers, new Vector3(-10f, 0f, z), 90f);
                PlaceFixed(Wall4, $"Wall_Reception_E_{z:0}", dividers, new Vector3(10f, 0f, z), 90f);
            }

            foreach (float z in new[] { -9f, -3f, 3f, 9f })
            {
                PlaceFixed(Wall4, $"Wall_WestLane_{z:0}", dividers, new Vector3(-18f, 0f, z), 90f);
                PlaceFixed(Wall4, $"Wall_EastLane_{z:0}", dividers, new Vector3(18f, 0f, z), 90f);
            }

            Transform doors = CreateGroup("Doors", parent);
            PlaceDoor(doors, "Zone01", new Vector3(-11.2f, 0f, 12f), 70f);
            PlaceDoor(doors, "Zone03", new Vector3(11.2f, 0f, 12f), 110f);
            PlaceDoor(doors, "Zone08", new Vector3(-18f, 0f, -14f), 20f);
            PlaceDoor(doors, "Zone11", new Vector3(18f, 0f, -14f), 160f);
            PlaceDoor(doors, "Reception", new Vector3(0f, 0f, -21.7f), 70f);
            PlaceDoor(doors, "Zone04West", new Vector3(-5f, 0f, 8.5f), 30f);
            PlaceDoor(doors, "Zone04East", new Vector3(5f, 0f, 8.5f), 150f);

            Transform pillars = CreateGroup("Pillars", parent);
            foreach (Vector3 position in new[]
            {
                new Vector3(-18f, 0f, 12f), new Vector3(18f, 0f, 12f),
                new Vector3(-18f, 0f, -14f), new Vector3(18f, 0f, -14f),
                new Vector3(-10f, 0f, -14f), new Vector3(10f, 0f, -14f)
            })
            {
                PlaceFixed(WallPillar, $"Wall_Pillar_{pillars.childCount:00}", pillars, position, 0f);
            }
        }

        private static void BuildRouteMarkers(Transform parent)
        {
            Transform west = CreateGroup("West_Logistics_Loop", parent);
            Transform east = CreateGroup("East_Physics_Lane", parent);
            foreach (float z in new[] { -11f, -5f, 1f, 7f })
            {
                CreateFloorStrip(west, new Vector3(-24f, 0.012f, z), new Vector3(0.35f, 0.02f, 4f), "RouteWest");
                CreateFloorStrip(east, new Vector3(24f, 0.012f, z), new Vector3(0.35f, 0.02f, 4f), "RouteEast");
            }

            CreateFloorStrip(west, new Vector3(-21f, 0.012f, 10f), new Vector3(6f, 0.02f, 0.35f), "RouteWest");
            CreateFloorStrip(east, new Vector3(21f, 0.012f, 10f), new Vector3(6f, 0.02f, 0.35f), "RouteEast");
            CreateFloorStrip(west, new Vector3(-21f, 0.012f, -12f), new Vector3(6f, 0.02f, 0.35f), "RouteWest");
            CreateFloorStrip(east, new Vector3(21f, 0.012f, -12f), new Vector3(6f, 0.02f, 0.35f), "RouteEast");
        }

        private static void BuildZone01(Transform zone)
        {
            AddZoneMarker(zone, 1, new Vector3(-20f, 0.02f, 16f));
            PlaceShelfCluster(zone, new Vector3(-27f, 0f, 19f), 90f, 10);
            PlaceShelfCluster(zone, new Vector3(-24f, 0f, 21f), 180f, 11);
            PlaceShelfCluster(zone, new Vector3(-18f, 0f, 21f), 180f, 12);
            PlaceDeskCluster(zone, new Vector3(-20f, 0f, 16.5f), 0f, 1, true);
            PlaceModel(Cabinet, "Cabinet_01", zone, new Vector3(-13f, 0f, 19.5f), 90f, 1.2f, PhysicsKind.Static, "Cabinet");
            PlaceGameplayBinder(zone, new Vector3(-19.6f, 0.78f, 16.3f), 15f, 1);
            PlaceBoxStack(zone, new Vector3(-14.5f, 0f, 14f), 0f, 3);
        }

        private static void BuildZone02(Transform zone)
        {
            AddZoneMarker(zone, 2, new Vector3(0f, 0.02f, 16f));
            PlaceShelfCluster(zone, new Vector3(-8f, 0f, 20.5f), 180f, 20);
            PlaceShelfCluster(zone, new Vector3(8f, 0f, 20.5f), 180f, 21);
            PlaceCartCluster(zone, new Vector3(-7f, 0f, 14.5f), 25f, 20);
            PlaceCartCluster(zone, new Vector3(7f, 0f, 14.5f), -25f, 21);
            PlaceModel(TrashCan, "TrashCan_02_A", zone, new Vector3(-4.5f, 0f, 18.5f), 0f, 0.65f, PhysicsKind.Static, "Metal");
            PlaceModel(TrashCan, "TrashCan_02_B", zone, new Vector3(4.5f, 0f, 18.5f), 0f, 0.65f, PhysicsKind.Static, "Metal");
            PlaceGameplayBinder(zone, new Vector3(0f, 0f, 18.2f), 0f, 2);
        }

        private static void BuildZone03(Transform zone)
        {
            AddZoneMarker(zone, 3, new Vector3(20f, 0.02f, 16f));
            PlaceShelfCluster(zone, new Vector3(27f, 0f, 19f), -90f, 30);
            PlaceShelfCluster(zone, new Vector3(24f, 0f, 21f), 180f, 31);
            PlaceDeskCluster(zone, new Vector3(22f, 0f, 18f), 180f, 3, true);
            PlaceModel(Cabinet, "Cabinet_03", zone, new Vector3(13f, 0f, 19f), -90f, 1.2f, PhysicsKind.Static, "Cabinet");
            PlaceModel(Plant, "Plant_03_A", zone, new Vector3(14f, 0f, 14f), 0f, 1.25f, PhysicsKind.Static, "Plant");
            PlaceModel(Plant, "Plant_03_B", zone, new Vector3(26f, 0f, 14f), 0f, 1.25f, PhysicsKind.Static, "Plant");
        }

        private static void BuildZone04(Transform zone)
        {
            AddZoneMarker(zone, 4, new Vector3(0f, 0.02f, 9f));
            foreach ((Vector3 position, float yaw) in new[]
            {
                (new Vector3(-6f, 0f, 9f), 35f), (new Vector3(-3f, 0f, 8f), -35f),
                (new Vector3(3f, 0f, 8f), 35f), (new Vector3(6f, 0f, 9f), -35f)
            })
            {
                PlaceFixed(LowPartition2, $"LowPartition_Checkpoint_{zone.childCount:00}", zone, position, yaw);
            }

            PlaceModel(Cabinet, "Cabinet_04_W", zone, new Vector3(-9f, 0f, 9f), 90f, 1.2f, PhysicsKind.Static, "Cabinet");
            PlaceModel(Cabinet, "Cabinet_04_E", zone, new Vector3(9f, 0f, 9f), -90f, 1.2f, PhysicsKind.Static, "Cabinet");
            foreach (Vector3 position in new[]
            {
                new Vector3(-8f, 0f, 7f), new Vector3(8f, 0f, 7f),
                new Vector3(-5f, 0f, 11f), new Vector3(5f, 0f, 11f)
            })
            {
                PlaceModel(Plant, $"Plant_04_{zone.childCount:00}", zone, position, 0f, 1.15f, PhysicsKind.Static, "Plant");
            }
        }

        private static void BuildZone05(Transform zone)
        {
            AddZoneMarker(zone, 5, new Vector3(0f, 0.02f, 0f));
            float[] xPositions = { -13f, -7.8f, -2.6f, 2.6f, 7.8f, 13f };
            float[] zPositions = { -3.5f, 1.2f, 5.5f };
            int cluster = 0;
            foreach (float z in zPositions)
            {
                foreach (float x in xPositions)
                {
                    if (Mathf.Abs(x) < 4f && Mathf.Abs(z - 1.2f) < 0.1f)
                    {
                        continue;
                    }

                    float yaw = (cluster % 2 == 0) ? 0f : 180f;
                    PlaceDeskCluster(zone, new Vector3(x, 0f, z), yaw, cluster, cluster % 3 != 0);
                    cluster++;
                }
            }

            foreach (float x in new[] { -10.5f, -5.2f, 5.2f, 10.5f })
            {
                PlaceFixed(LowPartition3, $"LowPartition_Cubicle_N_{x:0.0}", zone, new Vector3(x, 0f, 7f), 0f);
                PlaceFixed(LowPartition3, $"LowPartition_Cubicle_S_{x:0.0}", zone, new Vector3(x, 0f, -5.5f), 0f);
            }

            PlaceFixed(LowPartitionT, "LowPartition_Cubicle_Center", zone, new Vector3(0f, 0f, 2f), 0f);
            foreach (float x in new[] { -4f, -2f, 0f, 2f, 4f })
            {
                PlaceModel(Plant, $"Planter_Center_{x:0}", zone, new Vector3(x, 0f, -0.5f), 0f, 1.05f, PhysicsKind.Static, "Plant");
            }
        }

        private static void BuildZone06(Transform zone)
        {
            AddZoneMarker(zone, 6, new Vector3(0f, 0.02f, -7f));
            foreach (float x in new[] { -13f, -8f, 8f, 13f })
            {
                PlaceModel(Cabinet, $"Cabinet_06_{x:0}", zone, new Vector3(x, 0f, -7f), 0f, 1.2f, PhysicsKind.Static, "Cabinet");
            }

            PlaceShelfCluster(zone, new Vector3(-3.5f, 0f, -7.5f), 0f, 60);
            PlaceShelfCluster(zone, new Vector3(3.5f, 0f, -7.5f), 0f, 61);
        }

        private static void BuildZone07(Transform zone)
        {
            AddZoneMarker(zone, 7, new Vector3(0f, 0.02f, -11f));
            foreach (float x in new[] { -5f, 5f })
            {
                PlaceModel(DeskC, $"ConferenceTable_{x:0}", zone, new Vector3(x, 0f, -11f), 90f, 0.78f, PhysicsKind.Static, "DeskC");
                PlaceModel(Papers, $"Papers_07_{x:0}", zone, new Vector3(x, 0.8f, -11f), 20f, 0.035f, PhysicsKind.None, "Paper");
                PlaceModel(BinderA, $"Binder_07_{x:0}", zone, new Vector3(x + 0.8f, 0.8f, -11f), -15f, 0.28f, PhysicsKind.None, "BinderA");
                for (int seat = -1; seat <= 1; seat += 2)
                {
                    PlaceModel(Chair, $"PROP_KINEMATIC_ConferenceChair_{x:0}_{seat}", zone, new Vector3(x, 0f, -11f + seat * 1.7f), seat > 0 ? 180f : 0f, 1.1f, PhysicsKind.Kinematic, "Chair", true, 15f);
                }
            }
        }

        private static void BuildZone08(Transform zone)
        {
            AddZoneMarker(zone, 8, new Vector3(-24f, 0.02f, -18f));
            PlaceModel(Copier, "PROP_KINEMATIC_Copier_08_A", zone, new Vector3(-27f, 0f, -19f), 90f, 1.25f, PhysicsKind.Kinematic, "Copier", true, 90f);
            PlaceModel(Copier, "PROP_KINEMATIC_Copier_08_B", zone, new Vector3(-24.5f, 0f, -20f), 0f, 1.25f, PhysicsKind.Kinematic, "Copier", true, 90f);
            PlaceModel(Cabinet, "Cabinet_08_A", zone, new Vector3(-20f, 0f, -20.5f), 180f, 1.2f, PhysicsKind.Static, "Cabinet");
            PlaceModel(Papers, "Papers_08_A", zone, new Vector3(-20f, 1.22f, -20.5f), 12f, 0.05f, PhysicsKind.None, "Paper");
            PlaceModel(TrashCan, "TrashCan_08", zone, new Vector3(-22f, 0f, -16f), 0f, 0.65f, PhysicsKind.Static, "Metal");
        }

        private static void BuildZone09(Transform zone)
        {
            AddZoneMarker(zone, 9, new Vector3(-14f, 0.02f, -18f));
            foreach (float x in new[] { -17f, -14f, -11f })
            {
                PlaceShelfCluster(zone, new Vector3(x, 0f, -20f), 180f, 90 + Mathf.RoundToInt(x));
                PlaceShelfCluster(zone, new Vector3(x, 0f, -16f), 0f, 100 + Mathf.RoundToInt(x));
            }
        }

        private static void BuildZone10(Transform zone)
        {
            AddZoneMarker(zone, 10, new Vector3(0f, 0.02f, -18f));
            PlaceDeskCluster(zone, new Vector3(-3.3f, 0f, -18f), 180f, 100, true);
            PlaceDeskCluster(zone, new Vector3(3.3f, 0f, -18f), 180f, 101, true);
            PlaceModel(Cabinet, "ReceptionCabinet", zone, new Vector3(0f, 0f, -14.8f), 0f, 1.2f, PhysicsKind.Static, "Cabinet");
            PlaceModel(Plant, "Plant_10_W", zone, new Vector3(-8f, 0f, -20f), 0f, 1.25f, PhysicsKind.Static, "Plant");
            PlaceModel(Plant, "Plant_10_E", zone, new Vector3(8f, 0f, -20f), 0f, 1.25f, PhysicsKind.Static, "Plant");
        }

        private static void BuildZone11(Transform zone)
        {
            AddZoneMarker(zone, 11, new Vector3(22f, 0.02f, -18f));
            PlaceDeskCluster(zone, new Vector3(23f, 0f, -18f), 90f, 110, true);
            PlaceModel(Cabinet, "Cabinet_11_A", zone, new Vector3(27f, 0f, -20f), 90f, 1.2f, PhysicsKind.Static, "Cabinet");
            PlaceShelfCluster(zone, new Vector3(19f, 0f, -20f), 180f, 110);
            PlaceBoxStack(zone, new Vector3(26f, 0f, -15.5f), 0f, 2);
            PlaceModel(Plant, "Plant_11", zone, new Vector3(19f, 0f, -16f), 0f, 1.2f, PhysicsKind.Static, "Plant");
        }

        private static void BuildSideLanes(Transform parent)
        {
            Transform west = CreateGroup("West_Logistics_Loop_Dressing", parent);
            Transform east = CreateGroup("East_Physics_Lane_Dressing", parent);

            foreach (float z in new[] { -10f, -3f, 4f, 10f })
            {
                PlaceShelfCluster(west, new Vector3(-28f, 0f, z), 90f, 200 + Mathf.RoundToInt(z));
                PlaceShelfCluster(east, new Vector3(28f, 0f, z), -90f, 300 + Mathf.RoundToInt(z));
            }

            PlaceCartCluster(west, new Vector3(-23f, 0f, 6f), -10f, 210);
            PlaceCartCluster(west, new Vector3(-25f, 0f, -8f), 15f, 211);
            PlaceCartCluster(east, new Vector3(23f, 0f, 5f), 12f, 310);
            PlaceCartCluster(east, new Vector3(25f, 0f, -9f), -15f, 311);
            PlaceModel(Drawer, "PROP_KINEMATIC_Drawer_West", west, new Vector3(-21f, 0f, -1f), 90f, 1.15f, PhysicsKind.Kinematic, "Drawer", true, 55f);
            PlaceModel(Drawer, "PROP_KINEMATIC_Drawer_East", east, new Vector3(21f, 0f, 1f), -90f, 1.15f, PhysicsKind.Kinematic, "Drawer", true, 55f);
            PlaceModel(WaterDispenser, "WaterDispenser_West", west, new Vector3(-21f, 0f, 9f), 0f, 1.35f, PhysicsKind.Static, "Water");
            PlaceModel(WaterDispenser, "WaterDispenser_East", east, new Vector3(21f, 0f, 9f), 0f, 1.35f, PhysicsKind.Static, "Water");
            PlaceBoxStack(west, new Vector3(-22f, 0f, -11f), 0f, 4);
            PlaceBoxStack(east, new Vector3(22f, 0f, -11f), 0f, 4);
        }

        private static void BuildLighting(Transform parent)
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.38f, 0.40f, 0.42f);
            RenderSettings.ambientEquatorColor = new Color(0.24f, 0.25f, 0.26f);
            RenderSettings.ambientGroundColor = new Color(0.12f, 0.12f, 0.12f);
            RenderSettings.ambientIntensity = 0.8f;

            GameObject sun = new GameObject("Neutral_DirectionalLight");
            sun.transform.SetParent(parent, false);
            sun.transform.rotation = Quaternion.Euler(52f, -28f, 0f);
            Light sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = new Color(0.92f, 0.95f, 1f);
            sunLight.intensity = 0.45f;
            sunLight.shadows = LightShadows.Soft;

            int lightIndex = 0;
            foreach (float z in new[] { -18f, -10f, -2f, 6f, 14f, 20f })
            {
                foreach (float x in new[] { -24f, -12f, 0f, 12f, 24f })
                {
                    GameObject fixture = PlaceModel(
                        CeilingLight,
                        $"CeilingFixture_{lightIndex:00}",
                        parent,
                        new Vector3(x, 3.15f, z),
                        0f,
                        0.12f,
                        PhysicsKind.None,
                        "LightFixture",
                        false);
                    fixture.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

                    GameObject lightObject = new GameObject($"OfficeLight_{lightIndex:00}");
                    lightObject.transform.SetParent(parent, false);
                    lightObject.transform.position = new Vector3(x, 3.05f, z);
                    Light light = lightObject.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = new Color(0.86f, 0.91f, 1f);
                    light.range = 9.5f;
                    light.intensity = 190f;
                    light.shadows = (lightIndex % 3 == 0) ? LightShadows.Soft : LightShadows.None;
                    lightIndex++;
                }
            }
        }

        private static void PlaceDeskCluster(
            Transform parent,
            Vector3 position,
            float yaw,
            int index,
            bool detailed)
        {
            string[] deskVariants = { DeskA, DeskB, DeskC };
            string[] deskMaterials = { "DeskA", "DeskB", "DeskC" };
            int variant = Math.Abs(index) % deskVariants.Length;
            GameObject cluster = new GameObject($"DeskCluster_{index:000}");
            cluster.transform.SetParent(parent, false);
            cluster.transform.position = position;
            cluster.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            PlaceModel(deskVariants[variant], "Desk", cluster.transform, position, yaw, 0.76f, PhysicsKind.Static, deskMaterials[variant]);
            Vector3 chairPosition = position + RotateOffset(new Vector3(0f, 0f, -1.15f), yaw);
            PlaceModel(Chair, $"PROP_KINEMATIC_Chair_{index:000}", cluster.transform, chairPosition, yaw, 1.1f, PhysicsKind.Kinematic, "Chair", true, 15f);

            Vector3 monitorPosition = position + RotateOffset(new Vector3(0f, 0.77f, 0.18f), yaw);
            PlaceModel(Monitor, $"Monitor_{index:000}", cluster.transform, monitorPosition, yaw, 0.42f, PhysicsKind.None, "Monitor");
            Vector3 keyboardPosition = position + RotateOffset(new Vector3(0f, 0.77f, -0.3f), yaw);
            PlaceModel(Keyboard, $"Keyboard_{index:000}", cluster.transform, keyboardPosition, yaw, 0.06f, PhysicsKind.None, "Keyboard");

            if (!detailed)
            {
                return;
            }

            Vector3 paperPosition = position + RotateOffset(new Vector3(0.55f, 0.78f, -0.05f), yaw);
            PlaceModel(Papers, $"Papers_{index:000}", cluster.transform, paperPosition, yaw + 8f, 0.035f, PhysicsKind.None, "Paper");
            Vector3 phonePosition = position + RotateOffset(new Vector3(-0.55f, 0.78f, 0.05f), yaw);
            PlaceModel(Phone, $"Phone_{index:000}", cluster.transform, phonePosition, yaw - 12f, 0.22f, PhysicsKind.None, "Phone");

            if (index % 4 == 0)
            {
                Vector3 lampPosition = position + RotateOffset(new Vector3(0.65f, 0.78f, 0.22f), yaw);
                PlaceModel(DeskLamp, $"DeskLamp_{index:000}", cluster.transform, lampPosition, yaw, 0.48f, PhysicsKind.None, "DeskLamp");
            }

            if (index % 3 == 0)
            {
                Vector3 binderPosition = position + RotateOffset(new Vector3(-0.45f, 0.78f, -0.08f), yaw);
                PlaceModel(BinderA, $"DeskBinder_{index:000}", cluster.transform, binderPosition, yaw + 20f, 0.28f, PhysicsKind.None, "BinderA");
            }

            if (index % 5 == 0)
            {
                Vector3 notesPosition = position + RotateOffset(new Vector3(0.35f, 0.78f, 0.2f), yaw);
                PlaceModel(Papers, $"DeskPapers_{index:000}", cluster.transform, notesPosition, yaw - 10f, 0.035f, PhysicsKind.None, "Paper");
            }
        }

        private static void PlaceShelfCluster(Transform parent, Vector3 position, float yaw, int index)
        {
            GameObject shelf = PlaceModel(
                Shelf,
                $"ShelfCluster_{index:000}",
                parent,
                position,
                yaw,
                2.1f,
                PhysicsKind.Static,
                "Shelf");

            Vector3[] localPositions =
            {
                new Vector3(-0.55f, 0.28f, 0f), new Vector3(0.45f, 0.28f, 0f),
                new Vector3(-0.45f, 0.88f, 0f), new Vector3(0.5f, 0.88f, 0f),
                new Vector3(-0.5f, 1.48f, 0f), new Vector3(0.45f, 1.48f, 0f)
            };
            string[] assets = { BoxA, BinderA, BinderB, BoxB, BinderC, Papers };
            string[] materials = { "BoxA", "BinderA", "BinderB", "BoxB", "BinderC", "Paper" };
            float[] heights = { 0.42f, 0.32f, 0.32f, 0.38f, 0.32f, 0.04f };
            for (int item = 0; item < localPositions.Length; item++)
            {
                Vector3 world = position + RotateOffset(localPositions[item], yaw);
                PlaceModel(
                    assets[item],
                    $"ShelfItem_{index:000}_{item:00}",
                    shelf.transform,
                    world,
                    yaw + ((item % 2 == 0) ? -4f : 5f),
                    heights[item],
                    PhysicsKind.None,
                    materials[item],
                    true);
            }
        }

        private static void PlaceCartCluster(Transform parent, Vector3 position, float yaw, int index)
        {
            GameObject cart = PlaceModel(
                Cart,
                $"PROP_KINEMATIC_Cart_{index:000}",
                parent,
                position,
                yaw,
                1.05f,
                PhysicsKind.Kinematic,
                "Cart",
                true,
                45f);

            foreach ((string asset, string material, Vector3 offset, float height, float extraYaw) in new[]
            {
                (BoxA, "BoxA", new Vector3(-0.3f, 0.62f, 0f), 0.38f, -5f),
                (BoxC, "BoxC", new Vector3(0.3f, 0.62f, 0f), 0.34f, 8f),
                (BinderA, "BinderA", new Vector3(-0.2f, 0.9f, 0f), 0.3f, -8f),
                (Papers, "Paper", new Vector3(0.25f, 0.9f, 0f), 0.04f, 5f)
            })
            {
                Vector3 world = position + RotateOffset(offset, yaw);
                PlaceModel(asset, $"CartItem_{index:000}_{cart.transform.childCount:00}", cart.transform, world, yaw + extraYaw, height, PhysicsKind.None, material, true);
            }
        }

        private static void PlaceBoxStack(Transform parent, Vector3 position, float yaw, int count)
        {
            string[] assets = { BoxA, BoxB, BoxC };
            string[] materials = { "BoxA", "BoxB", "BoxC" };
            float currentY = 0f;
            for (int index = 0; index < count; index++)
            {
                float height = 0.34f + (index % 2) * 0.06f;
                PlaceModel(
                    assets[index % assets.Length],
                    $"BoxStack_{parent.childCount:000}_{index:00}",
                    parent,
                    position + new Vector3((index % 2) * 0.08f, currentY, 0f),
                    yaw + index * 7f,
                    height,
                    PhysicsKind.Static,
                    materials[index % materials.Length],
                    true);
                currentY += height * 0.92f;
            }
        }

        private static void PlaceGameplayBinder(Transform parent, Vector3 position, float yaw, int variant)
        {
            string[] assets = { BinderA, BinderB, BinderC };
            string[] materials = { "BinderA", "BinderB", "BinderC" };
            int resolved = Mathf.Abs(variant) % assets.Length;
            PlaceModel(
                assets[resolved],
                $"PROP_KINEMATIC_GameplayBinder_{parent.childCount:000}",
                parent,
                position,
                yaw,
                0.32f,
                PhysicsKind.Kinematic,
                materials[resolved],
                true,
                1.5f);
        }

        private static GameObject PlaceFixed(
            string assetPath,
            string name,
            Transform parent,
            Vector3 position,
            float yaw)
        {
            string category = assetPath.Contains("LowPartition", StringComparison.Ordinal)
                ? "Partition"
                : "Wall";
            string prefix = assetPath.Contains("LowPartition", StringComparison.Ordinal)
                ? "FIXED_LowPartition_"
                : "FIXED_Wall_";
            return PlaceModel(
                assetPath,
                prefix + name,
                parent,
                position,
                yaw,
                0f,
                PhysicsKind.Static,
                category,
                true);
        }

        private static void PlaceDoor(Transform parent, string suffix, Vector3 position, float yaw)
        {
            PlaceModel(
                Door,
                "FIXED_Door_" + suffix,
                parent,
                position,
                yaw,
                0f,
                PhysicsKind.Kinematic,
                "Door",
                true,
                40f);
        }

        private static GameObject PlaceModel(
            string assetPath,
            string name,
            Transform parent,
            Vector3 position,
            float yaw,
            float targetHeight,
            PhysicsKind physics,
            string materialKey,
            bool addCollider = true,
            float mass = 10f)
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (source == null)
            {
                throw new FileNotFoundException($"Required model is missing: {assetPath}");
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            instance.name = name;
            instance.transform.SetParent(parent, true);
            instance.transform.position = position;
            instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            instance.transform.localScale = Vector3.one;

            if (targetHeight > 0f)
            {
                Bounds rawBounds = GetWorldBounds(instance);
                if (rawBounds.size.y > 0.0001f)
                {
                    float factor = targetHeight / rawBounds.size.y;
                    instance.transform.localScale = Vector3.one * factor;
                }
            }

            AlignBottom(instance, position.y);
            ApplyMaterial(instance, materialKey);

            if (addCollider || physics != PhysicsKind.None)
            {
                ReplaceCollider(instance);
            }

            if (physics == PhysicsKind.Kinematic)
            {
                Rigidbody body = instance.AddComponent<Rigidbody>();
                body.isKinematic = true;
                body.useGravity = false;
                body.mass = mass;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }

            if (physics == PhysicsKind.Static)
            {
                SetStaticRecursively(instance);
            }

            return instance;
        }

        private static void ReplaceCollider(GameObject root)
        {
            foreach (Collider existing in root.GetComponentsInChildren<Collider>(true))
            {
                UnityEngine.Object.DestroyImmediate(existing);
            }

            Bounds localBounds = GetLocalBounds(root);
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = localBounds.center;
            collider.size = new Vector3(
                Mathf.Max(localBounds.size.x, 0.02f),
                Mathf.Max(localBounds.size.y, 0.02f),
                Mathf.Max(localBounds.size.z, 0.02f));
        }

        private static void FitFootprint(GameObject root, float targetWidth, float targetDepth)
        {
            Bounds bounds = GetWorldBounds(root);
            Vector3 scale = root.transform.localScale;
            if (bounds.size.x > 0.001f)
            {
                scale.x *= targetWidth / bounds.size.x;
            }

            if (bounds.size.z > 0.001f)
            {
                scale.z *= targetDepth / bounds.size.z;
            }

            root.transform.localScale = scale;
        }

        private static void AlignBottom(GameObject root, float desiredBottom)
        {
            Bounds bounds = GetWorldBounds(root);
            Vector3 position = root.transform.position;
            position.y += desiredBottom - bounds.min.y;
            root.transform.position = position;
        }

        private static Bounds GetWorldBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(root.transform.position, Vector3.one * 0.1f);
            }

            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return bounds;
        }

        private static Bounds GetLocalBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(Vector3.zero, Vector3.one * 0.1f);
            }

            bool initialized = false;
            Bounds result = default;
            foreach (Renderer renderer in renderers)
            {
                Bounds world = renderer.bounds;
                Vector3 center = world.center;
                Vector3 extents = world.extents;
                for (int x = -1; x <= 1; x += 2)
                {
                    for (int y = -1; y <= 1; y += 2)
                    {
                        for (int z = -1; z <= 1; z += 2)
                        {
                            Vector3 corner = center + Vector3.Scale(extents, new Vector3(x, y, z));
                            Vector3 local = root.transform.InverseTransformPoint(corner);
                            if (!initialized)
                            {
                                result = new Bounds(local, Vector3.zero);
                                initialized = true;
                            }
                            else
                            {
                                result.Encapsulate(local);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static void AddZoneMarker(Transform parent, int zone, Vector3 position)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = $"ZoneMarker_{zone:00}";
            marker.transform.SetParent(parent, false);
            marker.transform.position = position;
            marker.transform.localScale = new Vector3(0.8f, 0.015f, 0.8f);
            marker.GetComponent<Renderer>().sharedMaterial = Materials["ZoneMarker"];
            UnityEngine.Object.DestroyImmediate(marker.GetComponent<Collider>());

            GameObject label = new GameObject($"ZoneLabel_{zone:00}");
            label.transform.SetParent(marker.transform, false);
            label.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            label.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            TextMesh text = label.AddComponent<TextMesh>();
            text.text = zone.ToString("00");
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.characterSize = 0.28f;
            text.fontSize = 64;
            text.color = Color.white;
        }

        private static void CreateFloorStrip(Transform parent, Vector3 position, Vector3 scale, string material)
        {
            GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strip.name = $"RouteStrip_{parent.childCount:00}";
            strip.transform.SetParent(parent, false);
            strip.transform.position = position;
            strip.transform.localScale = scale;
            strip.GetComponent<Renderer>().sharedMaterial = Materials[material];
            UnityEngine.Object.DestroyImmediate(strip.GetComponent<Collider>());
        }

        private static Transform CreateGroup(string name, Transform parent)
        {
            GameObject group = new GameObject(name);
            group.transform.SetParent(parent, false);
            return group.transform;
        }

        private static Vector3 RotateOffset(Vector3 value, float yaw)
        {
            return Quaternion.Euler(0f, yaw, 0f) * value;
        }

        private static void SetStaticRecursively(GameObject root)
        {
            foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
            {
                item.gameObject.isStatic = true;
            }
        }

        private static void ApplyMaterial(GameObject root, string key)
        {
            if (!Materials.TryGetValue(key, out Material material) || material == null)
            {
                return;
            }

            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                int count = Math.Max(1, renderer.sharedMaterials.Length);
                renderer.sharedMaterials = Enumerable.Repeat(material, count).ToArray();
            }
        }

        private static void CreateMaterials()
        {
            Materials.Clear();
            CreateMaterial("Floor", new Color(0.34f, 0.35f, 0.35f), 0.08f, 0.28f);
            CreateMaterial("Wall", new Color(0.55f, 0.54f, 0.50f), 0f, 0.18f);
            CreateMaterial("Partition", new Color(0.32f, 0.39f, 0.42f), 0.1f, 0.3f);
            CreateMaterial("Door", new Color(0.36f, 0.22f, 0.13f), 0f, 0.25f,
                EnvironmentRoot + "/Textures/ENV_door_BaseColor_v001.jpg");
            CreateMaterial("DeskA", new Color(0.38f, 0.27f, 0.17f), 0f, 0.22f,
                EnvironmentRoot + "/Textures/ENV_FixedDesk_BaseColor_v001.png");
            CreateMaterial("DeskB", new Color(0.37f, 0.29f, 0.19f), 0f, 0.22f,
                EnvironmentRoot + "/Textures/ENV_FixedDesk_B_BaseColor_v001.png");
            CreateMaterial("DeskC", new Color(0.42f, 0.30f, 0.18f), 0f, 0.22f,
                EnvironmentRoot + "/Textures/ENV_FixedDesk_C_BaseColor_v001.png");
            CreateMaterial("Cabinet", new Color(0.36f, 0.39f, 0.38f), 0.45f, 0.26f,
                EnvironmentRoot + "/Textures/ENV_cabinet__BaseColor_v001.png");
            CreateMaterial("Chair", new Color(0.09f, 0.10f, 0.11f), 0.2f, 0.3f,
                GameplayPropRoot + "/PRP_OfficeChair/PRP_OfficeChair_A_BaseColor_v001.png");
            CreateMaterial("Drawer", new Color(0.25f, 0.29f, 0.31f), 0.55f, 0.3f,
                GameplayPropRoot + "/PRP_DrawerCabinet/PRP_DrawerCabinet_BaseColor.png");
            CreateMaterial("Copier", new Color(0.61f, 0.62f, 0.60f), 0.1f, 0.22f,
                GameplayPropRoot + "/PRP_Copier/PRP_Copier.png");
            CreateMaterial("BinderA", new Color(0.15f, 0.25f, 0.35f), 0f, 0.2f,
                GameplayPropRoot + "/PRP_FileBinder/PRP_binder_A_v001.jpg");
            CreateMaterial("BinderB", new Color(0.35f, 0.24f, 0.13f), 0f, 0.2f,
                GameplayPropRoot + "/PRP_FileBinder/PRP_binder_B_v001.png");
            CreateMaterial("BinderC", new Color(0.25f, 0.32f, 0.22f), 0f, 0.2f,
                GameplayPropRoot + "/PRP_FileBinder/PRP_binder_C_v001.png");
            CreateMaterial("Shelf", new Color(0.18f, 0.20f, 0.21f), 0.7f, 0.24f,
                EnvironmentRoot + "/Textures/ENVmetalshelving__v001.png");
            CreateMaterial("Cart", new Color(0.20f, 0.25f, 0.27f), 0.65f, 0.28f,
                EnvironmentRoot + "/Textures/ENV_cart_v001.png");
            CreateMaterial("BoxA", new Color(0.42f, 0.29f, 0.16f), 0f, 0.12f,
                EnvironmentRoot + "/Textures/ENV_Box_A_v001.png");
            CreateMaterial("BoxB", new Color(0.48f, 0.34f, 0.19f), 0f, 0.12f,
                EnvironmentRoot + "/Textures/ENV_Box_B_v001.png");
            CreateMaterial("BoxC", new Color(0.36f, 0.26f, 0.16f), 0f, 0.12f,
                EnvironmentRoot + "/Textures/ENV_Box_C_v001.jpg");
            CreateMaterial("Paper", new Color(0.78f, 0.76f, 0.68f), 0f, 0.1f,
                EnvironmentRoot + "/Textures/ENV_papers_A_v001.jpg");
            CreateMaterial("Monitor", new Color(0.05f, 0.07f, 0.08f), 0.15f, 0.35f,
                EnvironmentRoot + "/Textures/ENV_Monitor_v001.png");
            CreateMaterial("Keyboard", new Color(0.08f, 0.09f, 0.09f), 0.15f, 0.25f,
                EnvironmentRoot + "/Textures/ENV_keyboard__v001.png");
            CreateMaterial("Phone", new Color(0.10f, 0.11f, 0.11f), 0.1f, 0.25f,
                EnvironmentRoot + "/Textures/ENV_phone_v001.png");
            CreateMaterial("DeskLamp", new Color(0.14f, 0.15f, 0.14f), 0.6f, 0.32f,
                EnvironmentRoot + "/Textures/ENV_desklamp_v001.png");
            CreateMaterial("Plant", new Color(0.16f, 0.28f, 0.14f), 0f, 0.16f,
                EnvironmentRoot + "/Textures/ENV_plant_A_v001.png");
            CreateMaterial("Metal", new Color(0.25f, 0.28f, 0.28f), 0.7f, 0.25f,
                EnvironmentRoot + "/Textures/ENV_trashcan_v001.png");
            CreateMaterial("Water", new Color(0.37f, 0.45f, 0.50f), 0.2f, 0.3f,
                EnvironmentRoot + "/Textures/ENV_WaterDispenser_v001.png");
            CreateMaterial("LightFixture", new Color(0.76f, 0.79f, 0.80f), 0.45f, 0.35f,
                EnvironmentRoot + "/Textures/ENV_light_v001.png");
            CreateMaterial("ZoneMarker", new Color(0.28f, 0.27f, 0.24f), 0.2f, 0.25f);
            CreateMaterial("RouteWest", new Color(0.18f, 0.35f, 0.20f), 0f, 0.18f);
            CreateMaterial("RouteEast", new Color(0.17f, 0.30f, 0.42f), 0f, 0.18f);
        }

        private static void ConfigureUniversalRenderPipeline()
        {
            EnsureFolder(RenderSettingsRoot);
            UniversalRendererData rendererData =
                AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererDataPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                rendererData.name = "Stage01_UniversalRenderer";
                AssetDatabase.CreateAsset(rendererData, RendererDataPath);
            }

            UniversalRenderPipelineAsset pipeline =
                AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelineAssetPath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(rendererData);
                pipeline.name = "Stage01_UniversalRenderPipeline";
                AssetDatabase.CreateAsset(pipeline, PipelineAssetPath);
            }

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            EditorUtility.SetDirty(rendererData);
            EditorUtility.SetDirty(pipeline);
            AssetDatabase.SaveAssets();
        }

        private static void CreateMaterial(
            string key,
            Color color,
            float metallic,
            float smoothness,
            string texturePath = null)
        {
            string path = $"{MaterialRoot}/M_Stage01_{key}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                throw new InvalidOperationException("URP/Lit shader is unavailable.");
            }

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            if (!string.IsNullOrWhiteSpace(texturePath))
            {
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture != null)
                {
                    material.SetTexture("_BaseMap", texture);
                }
            }

            EditorUtility.SetDirty(material);
            Materials[key] = material;
        }

        private static void RequireAssets()
        {
            string[] required =
            {
                Floor, Wall2, Wall4, Wall6, WallCorner, WallPillar,
                LowPartition1, LowPartition2, LowPartition3, LowPartitionT,
                DeskA, DeskB, DeskC, Cabinet, Door, BoxA, BoxB, BoxC, Cart,
                DeskLamp, Keyboard, CeilingLight, Shelf, Monitor, Papers, Phone,
                Plant, TrashCan, WaterDispenser, BinderA, BinderB, BinderC,
                Chair, Drawer, Copier
            };
            foreach (string path in required)
            {
                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) == null)
                {
                    throw new FileNotFoundException($"Required asset is missing: {path}");
                }
            }
        }

        private static void EnsureFolder(string path)
        {
            string normalized = path.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(normalized))
            {
                return;
            }

            string parent = Path.GetDirectoryName(normalized)?.Replace('\\', '/');
            string name = Path.GetFileName(normalized);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException($"Invalid Unity folder path: {path}");
            }

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                {
                    return child;
                }
            }

            return null;
        }

        private static bool Approximately(Vector3 left, Vector3 right)
        {
            return Mathf.Approximately(left.x, right.x) &&
                Mathf.Approximately(left.y, right.y) &&
                Mathf.Approximately(left.z, right.z);
        }
    }
}
