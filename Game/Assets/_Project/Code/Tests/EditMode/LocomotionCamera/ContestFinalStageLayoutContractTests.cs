using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestFinalStageLayoutContractTests
    {
        private const string MainScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_NorthOffice_Stage01_v001.unity";
        private const string TutorialScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_Tutorial_HoldingRoom_v001.unity";
        private const string MainRootName = "FINAL_NORTH_OFFICE_STAGE_01";
        private const string MainHooksRootName = "FINAL_STAGE_SEMANTIC_HOOKS";
        private const string TutorialRootName = "TUTORIAL_HOLDING_ROOM_STAGE";
        private const string TutorialHooksRootName = "TUTORIAL_SEMANTIC_HOOKS";

        private static readonly Dictionary<string, string> ProtectedSceneHashes =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                {
                    "Assets/_Project/Scenes/10_Prototype/Contest_SingleRoomGraybox.unity",
                    "0ED96AE20A74A8D91888DD108915EB9EEAA56DA7D3BA6581B660A1E966E8DE3A"
                },
                {
                    "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v001.unity",
                    "1C8A306EC11559E718303C17254FA280A4A9001BC721B64A4B5BD1D144650D1A"
                },
                {
                    "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v002.unity",
                    "E1595A6AEDF05FCD7318B85E8DC21558FF0F4DA01A2F768987FB2A4EC5071AA4"
                },
                {
                    "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v007.unity",
                    "1E7A0F381F93A1F846ACF2242444090DEE4D767235EE3BF06F166959AF488FEC"
                }
            };

        private Scene scene;
        private Transform mainRoot;

        [SetUp]
        public void SetUp()
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScenePath), Is.Not.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(TutorialScenePath), Is.Not.Null);
            scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Additive);
            Assert.That(scene.IsValid(), Is.True);
            Assert.That(scene.isLoaded, Is.True);
            mainRoot = RequireRoot(scene, MainRootName);
            Physics.SyncTransforms();
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
        public void ProtectedAcceptedScenes_RemainByteIdentical()
        {
            foreach (KeyValuePair<string, string> protectedScene in ProtectedSceneHashes)
            {
                string absolute = Path.GetFullPath(
                    Path.Combine(Application.dataPath, "..", protectedScene.Key));
                Assert.That(File.Exists(absolute), Is.True, protectedScene.Key);
                string actual;
                using (SHA256 sha = SHA256.Create())
                using (FileStream stream = File.OpenRead(absolute))
                {
                    actual = BitConverter.ToString(sha.ComputeHash(stream))
                        .Replace("-", string.Empty);
                }
                Assert.That(actual, Is.EqualTo(protectedScene.Value), protectedScene.Key);
            }
        }

        [Test]
        public void MainEnvelopeZonesAndPartitions_MatchFinalSpatialContract()
        {
            BoxCollider floor = RequireChild(
                    mainRoot,
                    "01_STATIC_ARCHITECTURE/ENVELOPE_48X34/STATIC_FLOOR_48X34")
                .GetComponent<BoxCollider>();
            Assert.That(floor, Is.Not.Null);
            Assert.That(floor.bounds.size.x, Is.EqualTo(48f).Within(0.001f));
            Assert.That(floor.bounds.size.z, Is.EqualTo(34f).Within(0.001f));
            Assert.That(floor.bounds.max.y, Is.EqualTo(0f).Within(0.001f));

            Transform zones = RequireChild(mainRoot, "02_ZONES_01_11");
            string[] expectedZones =
            {
                "ZONE_01_SUPPLY_POWER_KEY",
                "ZONE_02_OPTIONAL_ELITE_HALL",
                "ZONE_03_FINAL_EXIT",
                "ZONE_04_SECURITY_CHECKPOINT",
                "ZONE_05_CENTRAL_CUBICLE_FIELD",
                "ZONE_06_NARROW_TRANSITION",
                "ZONE_07_CONFERENCE_TEACHING",
                "ZONE_08_UTILITY_COPIER",
                "ZONE_09_ARCHIVE_STORAGE",
                "ZONE_10_NORTH_OFFICE_LOBBY_ENTRY",
                "ZONE_11_SECONDARY_OBJECTIVE"
            };
            CollectionAssert.AreEquivalent(
                expectedZones,
                zones.Cast<Transform>().Select(child => child.name));
            Assert.That(zones.childCount, Is.EqualTo(11));
            foreach (Transform zone in zones)
            {
                Assert.That(RequireChild(zone, "ZONE_BOUNDARY_BAND").GetComponent<LineRenderer>(),
                    Is.Not.Null, zone.name);
                Assert.That(RequireChild(zone, "ZONE_FLOOR_HEADER").GetComponent<TextMesh>(),
                    Is.Not.Null, zone.name);
            }

            Transform partitions = RequireChild(
                mainRoot,
                "01_STATIC_ARCHITECTURE/PARTITIONS_2_1M_BOTTOM_0");
            BoxCollider[] partitionColliders = partitions.GetComponentsInChildren<BoxCollider>(true);
            Assert.That(partitionColliders, Has.Length.EqualTo(18));
            foreach (BoxCollider partition in partitionColliders)
            {
                Assert.That(partition.name, Does.StartWith("PARTITION_"));
                Assert.That(partition.bounds.size.y, Is.EqualTo(2.1f).Within(0.001f),
                    partition.name);
                Assert.That(partition.bounds.min.y, Is.EqualTo(0f).Within(0.001f),
                    partition.name);
            }

            Transform walls = RequireChild(mainRoot, "01_STATIC_ARCHITECTURE");
            BoxCollider[] wallColliders = walls.GetComponentsInChildren<BoxCollider>(true)
                .Where(collider => collider.name.StartsWith("WALL_", StringComparison.Ordinal))
                .ToArray();
            Assert.That(wallColliders.Length, Is.GreaterThanOrEqualTo(25));
            foreach (BoxCollider wall in wallColliders)
            {
                Assert.That(wall.bounds.size.y, Is.EqualTo(4.2f).Within(0.001f), wall.name);
                Assert.That(wall.bounds.min.y, Is.EqualTo(0f).Within(0.001f), wall.name);
            }
        }

        [Test]
        public void SemanticHooks_EncodeKeyExitForcedEncountersAndNoDirectShortcut()
        {
            Transform hooks = RequireRoot(scene, MainHooksRootName);
            string[] expectedHooks =
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
            Transform[] authoredHooks = hooks.Cast<Transform>()
                .Where(child => child.name.StartsWith("HOOK_", StringComparison.Ordinal))
                .ToArray();
            CollectionAssert.AreEquivalent(expectedHooks, authoredHooks.Select(child => child.name));
            Assert.That(authoredHooks, Has.Length.EqualTo(13));
            foreach (Transform hook in authoredHooks)
            {
                BoxCollider trigger = hook.GetComponent<BoxCollider>();
                Assert.That(trigger, Is.Not.Null, hook.name);
                Assert.That(trigger.isTrigger, Is.True, hook.name);
                Assert.That(RequireChild(hook, "VISIBLE_REVIEW_MARKER").GetComponent<Renderer>(),
                    Is.Not.Null, hook.name);
            }

            Transform key = RequireChild(hooks, "HOOK_REQUIRED_KEY_01");
            Transform exit = RequireChild(hooks, "HOOK_EXIT_03");
            AssertVector(key.position, new Vector3(-19f, 0f, 13f));
            AssertVector(exit.position, new Vector3(20f, 0f, 14f));

            Vector3 origin = key.position + Vector3.up;
            Vector3 direction = (exit.position - key.position).normalized;
            float distance = Vector3.Distance(key.position, exit.position);
            int blockingWalls = Physics.RaycastAll(origin, direction, distance)
                .Count(hit => !hit.collider.isTrigger &&
                              hit.collider.transform.IsChildOf(mainRoot) &&
                              hit.collider.name.StartsWith("WALL_", StringComparison.Ordinal));
            Assert.That(blockingWalls, Is.GreaterThanOrEqualTo(2),
                "01 Key and 03 Exit must not have a direct penetrable shortcut.");
        }

        [Test]
        public void FigureEightRoutes_AreContinuousCapsuleClearAndCoverBothForcedEncounters()
        {
            Transform routes = RequireChild(mainRoot, "03_FIGURE_EIGHT_ROUTES");
            Transform west = RequireChild(routes, "ROUTE_WEST_LOOP_TO_KEY_AND_EXIT");
            Transform east = RequireChild(routes, "ROUTE_EAST_LOOP_TO_KEY_AND_EXIT");
            Transform central = RequireChild(routes, "ROUTE_CENTRAL_CROSS");

            AssertRouteClear(west);
            AssertRouteClear(east);
            AssertRouteClear(central);

            string[] westNames = RoutePoints(west).Select(point => point.name).ToArray();
            string[] eastNames = RoutePoints(east).Select(point => point.name).ToArray();
            Assert.That(westNames.Any(name => name.Contains("WEST_FORCED_ENCOUNTER")), Is.True);
            Assert.That(eastNames.Any(name => name.Contains("EAST_FORCED_ENCOUNTER")), Is.True);
            Assert.That(westNames.Any(name => name.Contains("REQUIRED_KEY_01")), Is.True);
            Assert.That(eastNames.Any(name => name.Contains("REQUIRED_KEY_01")), Is.True);
            Assert.That(westNames.Last(), Does.Contain("FINAL_EXIT_03"));
            Assert.That(eastNames.Last(), Does.Contain("FINAL_EXIT_03"));
        }

        [Test]
        public void GameplayActorsObjectsAndCamera_StartInsideBoundsWithoutStaticPenetration()
        {
            Collider[] staticColliders = mainRoot.GetComponentsInChildren<Collider>(true)
                .Where(collider => collider.enabled &&
                                   !collider.isTrigger &&
                                   collider.name != "STATIC_FLOOR_48X34")
                .ToArray();

            GameObject[] actorRoots =
            {
                RequireRootAny(scene, "GAMEPLAY_CHR00_PLAYER", "Player"),
                RequireRootAny(scene, "GAMEPLAY_CHR01_MOBILE_ALLY", "AllyProxy"),
                RequireRootAny(scene, "GAMEPLAY_ENY01_RUNNER_01", "EnemyProxy_01"),
                RequireRootAny(scene, "GAMEPLAY_ENY01_RUNNER_02", "EnemyProxy_02")
            };
            PrototypeInteractionTarget[] targets = SceneComponents<PrototypeInteractionTarget>(scene);
            Assert.That(targets, Has.Length.EqualTo(10));
            Collider[] gameplayColliders = actorRoots
                .SelectMany(root => root.GetComponentsInChildren<Collider>(true))
                .Concat(targets.SelectMany(target => target.GetComponentsInChildren<Collider>(true)))
                .Where(collider => collider.enabled && !collider.isTrigger)
                .Distinct()
                .ToArray();

            foreach (Collider gameplay in gameplayColliders)
            {
                Assert.That(gameplay.bounds.min.x, Is.GreaterThanOrEqualTo(-24f), gameplay.name);
                Assert.That(gameplay.bounds.max.x, Is.LessThanOrEqualTo(24f), gameplay.name);
                Assert.That(gameplay.bounds.min.z, Is.GreaterThanOrEqualTo(-17f), gameplay.name);
                Assert.That(gameplay.bounds.max.z, Is.LessThanOrEqualTo(17f), gameplay.name);
                foreach (Collider authoredStatic in staticColliders)
                {
                    bool penetrates = Physics.ComputePenetration(
                        gameplay,
                        gameplay.transform.position,
                        gameplay.transform.rotation,
                        authoredStatic,
                        authoredStatic.transform.position,
                        authoredStatic.transform.rotation,
                        out _,
                        out float depth);
                    Assert.That(penetrates && depth > 0.002f, Is.False,
                        $"{gameplay.name} penetrates {authoredStatic.name} by {depth:F4}m.");
                }
            }

            Camera camera = SceneComponents<Camera>(scene).Single();
            AssertFinite(camera.transform.position, "Camera position");
            Assert.That(camera.transform.position.x, Is.InRange(-24f, 24f));
            Assert.That(camera.transform.position.z, Is.InRange(-17f, 17f));
        }

        [Test]
        public void ExternalAssets_AreVisibleLicensedFamiliesWithNoImportedCollision()
        {
            Transform external = RequireChild(mainRoot, "07_EXTERNAL_ASSET_INTEGRATION");
            Assert.That(RequireChild(external, "POLY_HAVEN_CC0"), Is.Not.Null);
            Assert.That(RequireChild(external, "KENNEY_CC0"), Is.Not.Null);
            Assert.That(RequireChild(external, "DESIGNER_B_RUNTIME_VISUALS"), Is.Not.Null);
            Transform provenance = RequireChild(external, "ASSET_PROVENANCE");
            Assert.That(RequireChild(
                    provenance,
                    "DESIGNER_B_SOURCE_COMMIT_7536F551FCA146E225794219BC219D7F07DD5314"),
                Is.Not.Null);
            Assert.That(RequireChild(provenance, "POLY_HAVEN_CC0_POWERED_BY_POLY_HAVEN"),
                Is.Not.Null);
            Assert.That(RequireChild(provenance, "KENNEY_FURNITURE_KIT_2_0_CC0"),
                Is.Not.Null);

            Transform[] allTransforms = SceneComponents<Transform>(scene);
            Transform[] designerVisuals = allTransforms
                .Where(transform => transform.name.StartsWith(
                    "VISUAL_DESIGNER_B_",
                    StringComparison.Ordinal))
                .ToArray();
            Transform[] polyInstances = allTransforms
                .Where(transform => transform.name.StartsWith(
                    "POLYHAVEN_",
                    StringComparison.Ordinal))
                .ToArray();
            Transform[] kenneyInstances = allTransforms
                .Where(transform => transform.name.StartsWith(
                    "KENNEY_",
                    StringComparison.Ordinal))
                .ToArray();
            Assert.That(designerVisuals, Has.Length.EqualTo(10));
            Assert.That(polyInstances.Length, Is.GreaterThanOrEqualTo(18));
            Assert.That(kenneyInstances.Length, Is.GreaterThanOrEqualTo(40));
            foreach (Transform importedInstance in designerVisuals
                         .Concat(polyInstances)
                         .Concat(kenneyInstances))
            {
                Assert.That(importedInstance.GetComponentsInChildren<Collider>(true), Is.Empty,
                    importedInstance.name);
            }

            string[] requiredAssets =
            {
                "Assets/_Project/Art/Environment/FinalStage/DesignerB/DesignerB_Copier.fbx",
                "Assets/_Project/Art/Environment/FinalStage/DesignerB/DesignerB_DrawerCabinet.fbx",
                "Assets/_Project/Art/Environment/FinalStage/DesignerB/DesignerB_FileBinder.fbx",
                "Assets/_Project/Art/Environment/FinalStage/DesignerB/DesignerB_OfficeChair.fbx",
                "Assets/_Project/Art/Environment/FinalStage/ThirdParty/PolyHaven/metal_office_desk_1k.fbx",
                "Assets/_Project/Art/Environment/FinalStage/ThirdParty/PolyHaven/office_notepads_1k.fbx",
                "Assets/_Project/Art/Environment/FinalStage/ThirdParty/KenneyFurnitureKit/bookcaseClosed.fbx",
                "Assets/_Project/Art/Environment/FinalStage/ThirdParty/KenneyFurnitureKit/computerScreen.fbx"
            };
            foreach (string path in requiredAssets)
            {
                Assert.That(AssetDatabase.LoadMainAssetAtPath(path), Is.Not.Null, path);
            }
        }

        [Test]
        public void TutorialScene_ProvidesAllLearningSpacesAndMainStageTransitionHooks()
        {
            Scene tutorial = EditorSceneManager.OpenScene(TutorialScenePath, OpenSceneMode.Additive);
            try
            {
                Transform tutorialRoot = RequireRoot(tutorial, TutorialRootName);
                Transform hooks = RequireRoot(tutorial, TutorialHooksRootName);
                string[] expectedHooks =
                {
                    "HOOK_TUTORIAL_PLAYER_TIED_CHAIR_START",
                    "HOOK_NON_HOSTILE_GUARD_REACTION_AREA",
                    "HOOK_INPUT_WASD",
                    "HOOK_INPUT_SPACE",
                    "HOOK_INPUT_SHIFT",
                    "HOOK_INPUT_LMB",
                    "HOOK_INPUT_RMB",
                    "HOOK_COMBAT_STRAP_LEARNING",
                    "HOOK_GUARD_CLEAR_GATE",
                    "HOOK_STRAP_BARRICADE",
                    "HOOK_MAIN_STAGE_10_TRANSITION",
                    "HOOK_TUTORIAL_RESET_BASELINE"
                };
                Transform[] authoredHooks = hooks.Cast<Transform>()
                    .Where(child => child.name.StartsWith("HOOK_", StringComparison.Ordinal))
                    .ToArray();
                CollectionAssert.AreEquivalent(expectedHooks, authoredHooks.Select(child => child.name));
                Assert.That(authoredHooks, Has.Length.EqualTo(12));

                BoxCollider floor = RequireChild(
                        tutorialRoot,
                        "01_STATIC_ARCHITECTURE/STATIC_TUTORIAL_FLOOR_30X24")
                    .GetComponent<BoxCollider>();
                Assert.That(floor.bounds.size.x, Is.EqualTo(30f).Within(0.001f));
                Assert.That(floor.bounds.size.z, Is.EqualTo(24f).Within(0.001f));

                Transform learning = RequireChild(tutorialRoot, "02_LEARNING_SEGMENTS");
                Assert.That(learning.childCount, Is.EqualTo(4));
                Transform route = RequireChild(
                    tutorialRoot,
                    "06_TUTORIAL_ROUTE/ROUTE_TUTORIAL_TO_MAIN_STAGE_10");
                Assert.That(RoutePoints(route), Has.Length.EqualTo(6));
                Assert.That(RoutePoints(route).Last().name, Does.Contain("MAIN_STAGE_10_GATE"));

                Transform boundChair = RequireChild(
                    tutorialRoot,
                    "07_EXTERNAL_ASSET_INTEGRATION/DESIGNER_B_RUNTIME_VISUALS/" +
                    "DESIGNER_B_BOUND_CHAIR_SOCKET_VISUAL");
                Assert.That(boundChair.GetComponentsInChildren<Renderer>(true), Is.Not.Empty);
                Assert.That(boundChair.GetComponentsInChildren<Collider>(true), Is.Empty);

                GameObject ally = RequireRootAny(
                    tutorial,
                    "GAMEPLAY_CHR01_MOBILE_ALLY",
                    "AllyProxy");
                Assert.That(ally.activeSelf, Is.False);
                Assert.That(
                    SceneComponents<PrototypeInteractionTarget>(tutorial)
                        .All(target => !target.gameObject.activeSelf),
                    Is.True);
                Assert.That(
                    SceneComponents<MonoBehaviour>(tutorial)
                        .Where(component => component.GetType().Name.StartsWith(
                            "Showcase",
                            StringComparison.Ordinal))
                        .All(component => !component.enabled),
                    Is.True);
            }
            finally
            {
                EditorSceneManager.CloseScene(tutorial, true);
            }
        }

        private void AssertRouteClear(Transform route)
        {
            Transform[] points = RoutePoints(route);
            Assert.That(points.Length, Is.GreaterThanOrEqualTo(4), route.name);
            for (int pointIndex = 1; pointIndex < points.Length; pointIndex++)
            {
                Vector3 start = points[pointIndex - 1].position;
                Vector3 end = points[pointIndex].position;
                float distance = Vector3.Distance(start, end);
                int samples = Mathf.Max(1, Mathf.CeilToInt(distance / 0.25f));
                for (int sample = 0; sample <= samples; sample++)
                {
                    Vector3 point = Vector3.Lerp(start, end, sample / (float)samples);
                    Collider[] overlaps = Physics.OverlapCapsule(
                            new Vector3(point.x, 0.50f, point.z),
                            new Vector3(point.x, 1.50f, point.z),
                            0.48f,
                            ~0,
                            QueryTriggerInteraction.Ignore)
                        .Where(collider => collider.enabled &&
                                           collider.transform.IsChildOf(mainRoot) &&
                                           collider.name != "STATIC_FLOOR_48X34")
                        .ToArray();
                    Assert.That(overlaps, Is.Empty,
                        $"{route.name} blocked between {points[pointIndex - 1].name} and " +
                        $"{points[pointIndex].name} at {point}: " +
                        string.Join(", ", overlaps.Select(collider => collider.name)));
                }
            }
        }

        private static Transform[] RoutePoints(Transform route)
        {
            return route.Cast<Transform>()
                .Where(child => child.name.StartsWith("P", StringComparison.Ordinal))
                .OrderBy(child => child.name, StringComparer.Ordinal)
                .ToArray();
        }

        private static Transform RequireRoot(Scene targetScene, string name)
        {
            Transform root = targetScene.GetRootGameObjects()
                .Select(gameObject => gameObject.transform)
                .SingleOrDefault(transform => string.Equals(
                    transform.name,
                    name,
                    StringComparison.Ordinal));
            Assert.That(root, Is.Not.Null, $"Missing root {name} in {targetScene.path}");
            return root;
        }

        private static GameObject RequireRootAny(Scene targetScene, params string[] names)
        {
            foreach (string name in names)
            {
                GameObject root = targetScene.GetRootGameObjects()
                    .SingleOrDefault(gameObject => string.Equals(
                        gameObject.name,
                        name,
                        StringComparison.Ordinal));
                if (root != null)
                {
                    return root;
                }
            }
            Assert.Fail("Missing root: " + string.Join(" or ", names));
            return null;
        }

        private static Transform RequireChild(Transform parent, string path)
        {
            Transform child = parent.Find(path);
            Assert.That(child, Is.Not.Null, $"Missing {path} under {parent.name}");
            return child;
        }

        private static T[] SceneComponents<T>(Scene targetScene) where T : Component
        {
            return targetScene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }

        private static void AssertVector(Vector3 actual, Vector3 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.001f));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.001f));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.001f));
        }

        private static void AssertFinite(Vector3 value, string label)
        {
            Assert.That(float.IsNaN(value.x) || float.IsInfinity(value.x), Is.False, label);
            Assert.That(float.IsNaN(value.y) || float.IsInfinity(value.y), Is.False, label);
            Assert.That(float.IsNaN(value.z) || float.IsInfinity(value.z), Is.False, label);
        }
    }
}
