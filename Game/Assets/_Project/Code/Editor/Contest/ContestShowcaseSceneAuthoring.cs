using System;
using System.Collections.Generic;
using System.Linq;
using BreakNotice.Gameplay.Interaction;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.Contest.Editor
{
    public static class ContestShowcaseSceneAuthoring
    {
        public const string SourceScenePath =
            "Assets/_Project/Scenes/10_Prototype/Contest_SingleRoomGraybox.unity";

        public const string ShowcaseSceneFolder =
            "Assets/_Project/Scenes/20_Showcase";

        public const string ShowcaseScenePath =
            ShowcaseSceneFolder + "/Contest_OperationsFloor_Showcase_v001.unity";

        public const string ShowcaseRootName = "SHOWCASE_S1_GRAYBOX";

        private static readonly string[] LegacyEnvironmentRootNames =
        {
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

        [MenuItem("BREAK NOTICE/Contest/Prepare S1 Showcase Graybox")]
        public static void PrepareShowcaseScene()
        {
            RequireSceneAsset(SourceScenePath);
            EnsureShowcaseFolder();

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ShowcaseScenePath) == null)
            {
                if (!AssetDatabase.CopyAsset(SourceScenePath, ShowcaseScenePath))
                {
                    throw new InvalidOperationException(
                        $"Could not copy the preserved C0-C4 Scene to {ShowcaseScenePath}.");
                }

                AssetDatabase.ImportAsset(
                    ShowcaseScenePath,
                    ImportAssetOptions.ForceSynchronousImport);
            }

            Scene scene = EditorSceneManager.OpenScene(ShowcaseScenePath, OpenSceneMode.Single);
            ApplyS1Layout(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ShowcaseScenePath))
            {
                throw new InvalidOperationException($"Could not save {ShowcaseScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log(
                "S1 Showcase graybox prepared: 48m x 34m envelope, eight zones, " +
                "figure-eight topology, preserved C0-C4 gameplay roots, no Production Art.");
        }

        public static void ApplyS1Layout(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new ArgumentException("A valid loaded Scene is required.", nameof(scene));
            }

            ValidatePreservedGameplayRoots(scene);
            RemoveLegacyEnvironment(scene);

            GameObject previousRoot = FindRoot(scene, ShowcaseRootName);
            if (previousRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(previousRoot);
            }

            GameObject showcaseRoot = CreateRoot(scene, ShowcaseRootName);
            Transform envelope = CreateGroup(showcaseRoot.transform, "00_ENVELOPE");
            Transform zones = CreateGroup(showcaseRoot.transform, "01_ZONES");
            Transform topology = CreateGroup(showcaseRoot.transform, "02_FIGURE_EIGHT");
            Transform cubicleIsland = CreateGroup(showcaseRoot.transform, "03_CUBICLE_ISLAND");
            Transform layoutMarkers = CreateGroup(showcaseRoot.transform, "04_LAYOUT_MARKERS");
            Transform staticFurniture = CreateGroup(showcaseRoot.transform, "05_STATIC_FURNITURE");

            BuildEnvelope(envelope);
            BuildZoneMarkers(zones);
            BuildFigureEight(topology);
            BuildCubicleIsland(cubicleIsland);
            BuildLayoutMarkers(layoutMarkers);
            BuildStaticFurniture(staticFurniture);
            RepositionPreservedGameplay(scene);
            ConfigureShowcaseCamera(scene);

            ValidatePreservedGameplayRoots(scene);
        }

        private static void ConfigureShowcaseCamera(Scene scene)
        {
            GameObject cameraRoot = RequireRoot(scene, "GameplayCameraRig");
            CinemachineThirdPersonFollow follow =
                cameraRoot.GetComponentInChildren<CinemachineThirdPersonFollow>(true);
            if (follow == null)
            {
                throw new InvalidOperationException(
                    "The Showcase camera rig requires CinemachineThirdPersonFollow.");
            }

            // The S1 spawn sits against the south camera boundary.  Horizontal
            // follow lag must not carry the pre-Reset route state across the
            // authored teleport and into that wall; retain vertical damping.
            follow.Damping = new Vector3(0f, follow.Damping.y, 0f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(follow);
            EditorUtility.SetDirty(follow);
        }

        private static void BuildEnvelope(Transform parent)
        {
            CreateCube(parent, "Floor_48x34", new Vector3(0f, -0.25f, 0f),
                new Vector3(48f, 0.5f, 34f), true);
            CreateCube(parent, "Wall_South", new Vector3(0f, 2.1f, -17.25f),
                new Vector3(48.5f, 4.2f, 0.5f), true);
            CreateCube(parent, "Wall_West", new Vector3(-24.25f, 2.1f, 0f),
                new Vector3(0.5f, 4.2f, 34.5f), true);
            CreateCube(parent, "Wall_East", new Vector3(24.25f, 2.1f, 0f),
                new Vector3(0.5f, 4.2f, 34.5f), true);

            // The north wall keeps the locked 2 m Exit opening at X 20.5.
            CreateCube(parent, "Wall_North_West", new Vector3(-2.25f, 2.1f, 17.25f),
                new Vector3(43.5f, 4.2f, 0.5f), true);
            CreateCube(parent, "Wall_North_East", new Vector3(22.75f, 2.1f, 17.25f),
                new Vector3(2.5f, 4.2f, 0.5f), true);
            CreateCube(parent, "Exit_Frame_Lintel", new Vector3(20.5f, 3.6f, 17.25f),
                new Vector3(2f, 1.2f, 0.5f), true);

            CreateMarker(parent, "FlatFloorDatum_0m", Vector3.zero, Vector3.one);
            CreateMarker(parent, "CeilingHeight_4.2m", new Vector3(0f, 4.2f, 0f), Vector3.one);
        }

        private static void BuildZoneMarkers(Transform parent)
        {
            var zones = new Dictionary<string, Vector3>
            {
                { "Z0_ENTRY_STAGING", new Vector3(0f, 0f, -15f) },
                { "Z1_DIRECT_EQUIPMENT_BAY", new Vector3(0f, 0f, -10.5f) },
                { "Z2_CENTRAL_CLASH_BOWL", new Vector3(0f, 0f, -5f) },
                { "Z3_WEST_LOGISTICS_LOOP", new Vector3(-17f, 0f, 1f) },
                { "Z4_CENTRAL_CUBICLE_RING", new Vector3(0f, 0f, 4f) },
                { "Z5_EAST_PHYSICS_LANE", new Vector3(17f, 0f, 0.5f) },
                { "Z6_NORTH_TAG_ARENA_DRAWER_GATE", new Vector3(14.5f, 0f, 11f) },
                { "Z7_EXIT_RUN_NEXT_AREA_TEASE", new Vector3(20f, 0f, 14.5f) }
            };

            foreach (KeyValuePair<string, Vector3> zone in zones)
            {
                CreateMarker(parent, zone.Key, zone.Value, Vector3.one);
            }
        }

        private static void BuildFigureEight(Transform parent)
        {
            CreateMarker(parent, "SOUTH_BOWL", new Vector3(0f, 0f, -5f),
                new Vector3(20f, 1f, 6f));
            CreateMarker(parent, "WEST_LOOP", new Vector3(-17f, 0f, 1f),
                new Vector3(14f, 1f, 20f));
            CreateMarker(parent, "EAST_LANE", new Vector3(17f, 0f, 1f),
                new Vector3(14f, 1f, 20f));
            CreateMarker(parent, "NORTH_CROSS", new Vector3(0f, 0f, 10.5f),
                new Vector3(32f, 1f, 5f));
            CreateMarker(parent, "CENTRAL_CUT", new Vector3(0f, 0f, 4f),
                new Vector3(6f, 1f, 12f));

            // Non-colliding floor ribbons expose both loops in Scene view without
            // adding gameplay obstacles or authoring future navigation behavior.
            CreateCube(parent, "Path_West_South", new Vector3(-13f, 0.02f, -5f),
                new Vector3(8f, 0.04f, 5f), false);
            CreateCube(parent, "Path_West_Long", new Vector3(-17f, 0.02f, 2.5f),
                new Vector3(5f, 0.04f, 15f), false);
            CreateCube(parent, "Path_West_North", new Vector3(-10f, 0.02f, 11f),
                new Vector3(14f, 0.04f, 5f), false);
            CreateCube(parent, "Path_East_South", new Vector3(13f, 0.02f, -5f),
                new Vector3(8f, 0.04f, 5f), false);
            CreateCube(parent, "Path_East_Long", new Vector3(17f, 0.02f, 2.5f),
                new Vector3(6f, 0.04f, 15f), false);
            CreateCube(parent, "Path_East_North", new Vector3(10f, 0.02f, 11f),
                new Vector3(14f, 0.04f, 5f), false);
        }

        private static void BuildCubicleIsland(Transform parent)
        {
            Transform partitions = CreateGroup(parent, "LowPartitions_16");
            Transform desks = CreateGroup(parent, "FixedDesks_6");
            CreateMarker(parent, "IslandBounds_16x12", new Vector3(0f, 0f, 4f),
                new Vector3(16f, 1f, 12f));

            float[] horizontalCenters = { -6.25f, -3.75f, 3.75f, 6.25f };
            float[] horizontalLengths = { 3.5f, 1.5f, 1.5f, 3.5f };
            for (int index = 0; index < horizontalCenters.Length; index++)
            {
                CreateCube(
                    partitions,
                    $"Partition_South_{index + 1:00}",
                    new Vector3(horizontalCenters[index], 0.6f, -2f),
                    new Vector3(horizontalLengths[index], 1.2f, 0.2f),
                    true);
                CreateCube(
                    partitions,
                    $"Partition_North_{index + 1:00}",
                    new Vector3(horizontalCenters[index], 0.6f, 10f),
                    new Vector3(horizontalLengths[index], 1.2f, 0.2f),
                    true);
            }

            float[] verticalCenters = { -0.75f, 2f, 6f, 8.75f };
            float[] verticalLengths = { 2.5f, 1.5f, 1.5f, 2.5f };
            for (int index = 0; index < verticalCenters.Length; index++)
            {
                CreateCube(
                    partitions,
                    $"Partition_West_{index + 1:00}",
                    new Vector3(-8f, 0.6f, verticalCenters[index]),
                    new Vector3(0.2f, 1.2f, verticalLengths[index]),
                    true);
                CreateCube(
                    partitions,
                    $"Partition_East_{index + 1:00}",
                    new Vector3(8f, 0.6f, verticalCenters[index]),
                    new Vector3(0.2f, 1.2f, verticalLengths[index]),
                    true);
            }

            Vector3[] deskPositions =
            {
                new Vector3(-5.4f, 0.4f, 0.5f),
                new Vector3(5.4f, 0.4f, 0.5f),
                new Vector3(-5.4f, 0.4f, 4f),
                new Vector3(5.4f, 0.4f, 4f),
                new Vector3(-5.4f, 0.4f, 7.5f),
                new Vector3(5.4f, 0.4f, 7.5f)
            };

            for (int index = 0; index < deskPositions.Length; index++)
            {
                CreateCube(
                    desks,
                    $"FixedDesk_{index + 1:00}",
                    deskPositions[index],
                    new Vector3(2.6f, 0.8f, 1.2f),
                    true);
            }
        }

        private static void BuildLayoutMarkers(Transform parent)
        {
            CreateMarker(parent, "FutureBinder_B-D2", new Vector3(12f, 0f, -4.5f), Vector3.one);
            CreateMarker(parent, "FutureBinder_B-A3", new Vector3(9.5f, 0f, 10.5f), Vector3.one);
            CreateMarker(parent, "FutureChair_C-D2", new Vector3(13f, 0f, -2.5f), Vector3.one);
            CreateMarker(parent, "ParkingBay", new Vector3(20f, 0f, 10.5f),
                new Vector3(3f, 1f, 3f));
            CreateMarker(parent, "StrapAssistPocket", new Vector3(13f, 0f, 1.5f),
                new Vector3(6f, 1f, 6f));
            CreateMarker(parent, "RedirectArc", new Vector3(13f, 0f, 5.5f),
                new Vector3(6f, 1f, 6f));
            CreateMarker(parent, "FinisherPocket_8x7", new Vector3(13.5f, 0f, 9f),
                new Vector3(8f, 1f, 7f));
            CreateMarker(parent, "ExitTarget", new Vector3(20.5f, 0f, 16.5f),
                new Vector3(2f, 1f, 1f));
        }

        private static void BuildStaticFurniture(Transform parent)
        {
            CreateCube(parent, "SupplyDesk_West", new Vector3(-20f, 0.5f, -1f),
                new Vector3(3f, 1f, 1.2f), true);
            CreateCube(parent, "PhysicsColumn", new Vector3(12f, 2.1f, 2f),
                new Vector3(0.7f, 4.2f, 0.7f), true);
            GameObject angledDesk = CreateCube(parent, "AngledDesk_East",
                new Vector3(16f, 0.5f, 4f), new Vector3(3f, 1f, 1.2f), true);
            angledDesk.transform.rotation = Quaternion.Euler(0f, 25f, 0f);
            CreateCube(parent, "CrashDesk_North", new Vector3(10f, 0.5f, 12f),
                new Vector3(3f, 1f, 1.2f), true);
            CreateCube(parent, "ImpactPartition", new Vector3(18f, 0.6f, 8f),
                new Vector3(0.2f, 1.2f, 3f), true);
        }

        private static void RepositionPreservedGameplay(Scene scene)
        {
            SetPlanarPosition(RequireRoot(scene, "Player"), 0f, -15f);
            SetPlanarPosition(RequireRoot(scene, "AllyProxy"), -2.4f, -14.2f);
            SetPlanarPosition(RequireRoot(scene, "EnemyProxy_01"), 1f, 1.5f);
            SetPlanarPosition(RequireRoot(scene, "EnemyProxy_02"), 15f, 13f);

            var targetPositions = new Dictionary<string, Vector2>
            {
                { "BINDER-DIRECT-01", new Vector2(-6f, -11.5f) },
                { "BINDER-ALLY-01", new Vector2(-19f, -2.5f) },
                { "BINDER-ALLY-02", new Vector2(-7f, 12.5f) },
                { "CHAIR-DIRECT-01", new Vector2(6f, -11f) },
                { "CHAIR-ALLY-01", new Vector2(-18f, 4f) },
                { "DRAWER-01", new Vector2(15.5f, 10.5f) },
                { "COPIER-01", new Vector2(-20.5f, -10f) }
            };

            PrototypeInteractionTarget[] targets = GetInteractionTargets(scene);
            foreach (PrototypeInteractionTarget target in targets)
            {
                if (!targetPositions.TryGetValue(target.RuntimeInstanceKey, out Vector2 position))
                {
                    throw new InvalidOperationException(
                        $"Unexpected preserved gameplay target: {target.RuntimeInstanceKey}");
                }

                SetPlanarPosition(target.gameObject, position.x, position.y);
            }
        }

        private static void ValidatePreservedGameplayRoots(Scene scene)
        {
            RequireRoot(scene, "Player");
            RequireRoot(scene, "GameplayCameraRig");
            RequireRoot(scene, "ContestBrowserInput");
            RequireRoot(scene, "AllyProxy");
            RequireRoot(scene, "EnemyProxy_01");
            RequireRoot(scene, "EnemyProxy_02");

            PrototypeInteractionTarget[] targets = GetInteractionTargets(scene);
            string[] expectedKeys =
            {
                "BINDER-ALLY-01",
                "BINDER-ALLY-02",
                "BINDER-DIRECT-01",
                "CHAIR-ALLY-01",
                "CHAIR-DIRECT-01",
                "COPIER-01",
                "DRAWER-01"
            };

            string[] actualKeys = targets
                .Select(target => target.RuntimeInstanceKey)
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();
            if (!actualKeys.SequenceEqual(expectedKeys))
            {
                throw new InvalidOperationException(
                    "S1 must preserve exactly the seven C0-C4 gameplay targets without duplication.");
            }
        }

        private static PrototypeInteractionTarget[] GetInteractionTargets(Scene scene)
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<PrototypeInteractionTarget>(true))
                .OrderBy(target => target.RuntimeInstanceKey, StringComparer.Ordinal)
                .ToArray();
        }

        private static void RemoveLegacyEnvironment(Scene scene)
        {
            foreach (string rootName in LegacyEnvironmentRootNames)
            {
                GameObject root = FindRoot(scene, rootName);
                if (root != null)
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }

        private static void EnsureShowcaseFolder()
        {
            if (AssetDatabase.IsValidFolder(ShowcaseSceneFolder))
            {
                return;
            }

            string guid = AssetDatabase.CreateFolder(
                "Assets/_Project/Scenes",
                "20_Showcase");
            if (string.IsNullOrWhiteSpace(guid))
            {
                throw new InvalidOperationException(
                    $"Could not create {ShowcaseSceneFolder}.");
            }
        }

        private static void RequireSceneAsset(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
            {
                throw new InvalidOperationException($"Required Scene does not exist: {path}");
            }
        }

        private static GameObject CreateRoot(Scene scene, string name)
        {
            var root = new GameObject(name);
            SceneManager.MoveGameObjectToScene(root, scene);
            return root;
        }

        private static Transform CreateGroup(Transform parent, string name)
        {
            var group = new GameObject(name).transform;
            group.SetParent(parent, false);
            return group;
        }

        private static Transform CreateMarker(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale)
        {
            Transform marker = CreateGroup(parent, name);
            marker.position = position;
            marker.localScale = scale;
            return marker;
        }

        private static GameObject CreateCube(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
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

            GameObjectUtility.SetStaticEditorFlags(
                cube,
                StaticEditorFlags.BatchingStatic |
                StaticEditorFlags.OccluderStatic |
                StaticEditorFlags.OccludeeStatic);
            return cube;
        }

        private static void SetPlanarPosition(GameObject gameObject, float x, float z)
        {
            Vector3 position = gameObject.transform.position;
            gameObject.transform.position = new Vector3(x, position.y, z);
            EditorUtility.SetDirty(gameObject.transform);
        }

        private static GameObject RequireRoot(Scene scene, string name)
        {
            GameObject root = FindRoot(scene, name);
            if (root == null)
            {
                throw new InvalidOperationException($"Required Scene root is missing: {name}");
            }

            return root;
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            return scene.GetRootGameObjects()
                .FirstOrDefault(root => string.Equals(root.name, name, StringComparison.Ordinal));
        }
    }
}
