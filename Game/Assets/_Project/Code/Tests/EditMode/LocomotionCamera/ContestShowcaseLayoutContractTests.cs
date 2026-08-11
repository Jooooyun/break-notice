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
    public sealed class ContestShowcaseLayoutContractTests
    {
        private const string SourceScenePath =
            "Assets/_Project/Scenes/10_Prototype/Contest_SingleRoomGraybox.unity";

        private const string ShowcaseScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v001.unity";

        private const string SourceSceneSha256 =
            "0ed96ae20a74a8d91888dd108915eb9eeaa56da7d3ba6581b660a1e966e8de3a";

        private const float RouteGridStep = 0.5f;

        private Scene scene;
        private readonly Collider[] overlapBuffer = new Collider[256];

        [SetUp]
        public void SetUp()
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(ShowcaseScenePath), Is.Not.Null);
            scene = EditorSceneManager.OpenScene(ShowcaseScenePath, OpenSceneMode.Additive);
            Assert.That(scene.IsValid(), Is.True);
            Assert.That(scene.isLoaded, Is.True);
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
        public void Envelope_IsExactly48By34_FlatAtZero_With4Point2MeterCeilingReference()
        {
            Transform root = RequireRoot("SHOWCASE_S1_GRAYBOX");
            Transform floor = RequireChild(root, "00_ENVELOPE/Floor_48x34");
            BoxCollider floorCollider = floor.GetComponent<BoxCollider>();

            Assert.That(floorCollider, Is.Not.Null);
            Assert.That(floorCollider.bounds.size.x, Is.EqualTo(48f).Within(0.001f));
            Assert.That(floorCollider.bounds.size.z, Is.EqualTo(34f).Within(0.001f));
            Assert.That(floorCollider.bounds.max.y, Is.EqualTo(0f).Within(0.001f));
            AssertVector(
                RequireChild(root, "00_ENVELOPE/FlatFloorDatum_0m").position,
                Vector3.zero);
            Assert.That(
                RequireChild(root, "00_ENVELOPE/CeilingHeight_4.2m").position.y,
                Is.EqualTo(4.2f).Within(0.001f));

            Transform exit = RequireChild(root, "04_LAYOUT_MARKERS/ExitTarget");
            AssertVector(exit.position, new Vector3(20.5f, 0f, 16.5f));
            Assert.That(exit.localScale.x, Is.InRange(1.8f, 2f));
        }

        [Test]
        public void Zones_Z0ThroughZ7_ArePresentAtLockedCenters()
        {
            Transform zones = RequireChild(RequireRoot("SHOWCASE_S1_GRAYBOX"), "01_ZONES");
            var expected = new Dictionary<string, Vector3>
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

            Assert.That(zones.childCount, Is.EqualTo(8));
            foreach (KeyValuePair<string, Vector3> zone in expected)
            {
                AssertVector(RequireChild(zones, zone.Key).position, zone.Value);
            }
        }

        [Test]
        public void FigureEight_AuthorsDistinctWestAndEastLoops_BetweenSharedSouthAndNorth()
        {
            Transform topology = RequireChild(
                RequireRoot("SHOWCASE_S1_GRAYBOX"),
                "02_FIGURE_EIGHT");
            Transform south = RequireChild(topology, "SOUTH_BOWL");
            Transform west = RequireChild(topology, "WEST_LOOP");
            Transform east = RequireChild(topology, "EAST_LANE");
            Transform north = RequireChild(topology, "NORTH_CROSS");
            Transform centralCut = RequireChild(topology, "CENTRAL_CUT");

            Assert.That(west.position.x, Is.LessThan(-10f));
            Assert.That(east.position.x, Is.GreaterThan(10f));
            Assert.That(south.position.z, Is.LessThan(west.position.z));
            Assert.That(north.position.z, Is.GreaterThan(west.position.z));
            Assert.That(south.localScale.x, Is.GreaterThanOrEqualTo(20f));
            AssertVector(north.position, new Vector3(0f, 0f, 10.5f));
            Assert.That(north.localScale.z, Is.GreaterThanOrEqualTo(5f));
            Assert.That(west.localScale.x, Is.GreaterThanOrEqualTo(4.5f));
            Assert.That(east.localScale.x, Is.GreaterThanOrEqualTo(6f));
            Assert.That(centralCut.localScale.x, Is.GreaterThanOrEqualTo(6f));
        }

        [Test]
        public void CubicleIsland_Is16By12_WithSixteenLowPartitionsAndSixFixedDesks()
        {
            Transform island = RequireChild(
                RequireRoot("SHOWCASE_S1_GRAYBOX"),
                "03_CUBICLE_ISLAND");
            Transform bounds = RequireChild(island, "IslandBounds_16x12");
            Transform partitions = RequireChild(island, "LowPartitions_16");
            Transform desks = RequireChild(island, "FixedDesks_6");

            AssertVector(bounds.position, new Vector3(0f, 0f, 4f));
            Assert.That(bounds.localScale.x, Is.EqualTo(16f).Within(0.001f));
            Assert.That(bounds.localScale.z, Is.EqualTo(12f).Within(0.001f));
            Assert.That(partitions.childCount, Is.EqualTo(16));
            Assert.That(desks.childCount, Is.EqualTo(6));

            foreach (Transform partition in partitions)
            {
                BoxCollider collider = partition.GetComponent<BoxCollider>();
                Assert.That(collider, Is.Not.Null, partition.name);
                Assert.That(collider.bounds.max.y, Is.LessThanOrEqualTo(1.25f), partition.name);
            }
        }

        [Test]
        public void PreservedC0C4GameplayRoots_AreRepositionedWithoutNewGameplayTargets()
        {
            AssertPlanar("Player", 0f, -15f);
            AssertPlanar("AllyProxy", -2.4f, -14.2f);
            AssertPlanar("EnemyProxy_01", 1f, 1.5f);
            AssertPlanar("EnemyProxy_02", 15f, 13f);

            var expected = new Dictionary<string, Vector2>
            {
                { "BINDER-DIRECT-01", new Vector2(-6f, -11.5f) },
                { "BINDER-ALLY-01", new Vector2(-19f, -2.5f) },
                { "BINDER-ALLY-02", new Vector2(-7f, 12.5f) },
                { "CHAIR-DIRECT-01", new Vector2(6f, -11f) },
                { "CHAIR-ALLY-01", new Vector2(-18f, 4f) },
                { "DRAWER-01", new Vector2(15.5f, 10.5f) },
                { "COPIER-01", new Vector2(-20.5f, -10f) }
            };

            PrototypeInteractionTarget[] targets = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<PrototypeInteractionTarget>(true))
                .ToArray();
            Assert.That(targets, Has.Length.EqualTo(7));
            Assert.That(
                targets.Select(target => target.RuntimeInstanceKey).Distinct(StringComparer.Ordinal).Count(),
                Is.EqualTo(7));

            foreach (PrototypeInteractionTarget target in targets)
            {
                Assert.That(expected.ContainsKey(target.RuntimeInstanceKey), Is.True);
                Vector2 position = expected[target.RuntimeInstanceKey];
                Assert.That(target.transform.position.x, Is.EqualTo(position.x).Within(0.001f));
                Assert.That(target.transform.position.z, Is.EqualTo(position.y).Within(0.001f));
            }
        }

        [Test]
        public void FutureContentPositions_AreMarkersOnly_AndDoNotImplementLaterPackages()
        {
            Transform markers = RequireChild(
                RequireRoot("SHOWCASE_S1_GRAYBOX"),
                "04_LAYOUT_MARKERS");
            string[] expectedMarkerNames =
            {
                "FutureBinder_B-D2",
                "FutureBinder_B-A3",
                "FutureChair_C-D2",
                "ParkingBay",
                "StrapAssistPocket",
                "RedirectArc",
                "FinisherPocket_8x7",
                "ExitTarget"
            };

            foreach (string markerName in expectedMarkerNames)
            {
                Transform marker = RequireChild(markers, markerName);
                Assert.That(marker.GetComponents<Component>(), Has.Length.EqualTo(1), markerName);
            }

            AssertVector(RequireChild(markers, "FutureBinder_B-D2").position,
                new Vector3(12f, 0f, -4.5f));
            AssertVector(RequireChild(markers, "FutureBinder_B-A3").position,
                new Vector3(9.5f, 0f, 10.5f));
            AssertVector(RequireChild(markers, "FutureChair_C-D2").position,
                new Vector3(13f, 0f, -2.5f));
        }

        [Test]
        public void FigureEight_CurrentCharacterControllerCapsule_HasContinuousWestEastAndCentralConnections()
        {
            Transform player = RequireRoot("Player");
            CharacterController controller = player.GetComponent<CharacterController>();
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.radius, Is.EqualTo(0.45f).Within(0.001f));
            Assert.That(controller.skinWidth, Is.EqualTo(0.08f).Within(0.001f));
            Physics.SyncTransforms();

            var southBowl = new Vector2(0f, -5f);
            var northCross = new Vector2(0f, 10.5f);

            AssertMaskedRoute(
                "west figure-eight loop",
                controller,
                southBowl,
                northCross,
                IsInsideWestLoopMask);
            AssertMaskedRoute(
                "east figure-eight loop",
                controller,
                southBowl,
                northCross,
                IsInsideEastLoopMask);
            AssertMaskedRoute(
                "central shared connection",
                controller,
                southBowl,
                northCross,
                point => point.x >= -2.5f && point.x <= 2.5f &&
                         point.y >= -5f && point.y <= 10.5f);
        }

        [Test]
        public void LockedLanes_HaveRequiredGeometryClearance_AndCentralBowlIsClear()
        {
            Physics.SyncTransforms();

            for (float z = -4.5f; z <= 9.5f; z += 0.5f)
            {
                Assert.That(
                    FindWidestFreeHorizontalRun(z, -23.75f, -8.25f),
                    Is.GreaterThanOrEqualTo(4.5f),
                    $"West lane pinches below 4.5 m at Z {z:0.0}.");
                Assert.That(
                    FindWidestFreeHorizontalRun(z, 8.25f, 23.75f),
                    Is.GreaterThanOrEqualTo(6f),
                    $"East lane pinches below 6 m at Z {z:0.0}.");
            }

            Assert.That(
                FindWidestFreeVerticalRun(0f, 10.25f, 16.75f),
                Is.GreaterThanOrEqualTo(5f),
                "North Cross must retain a 5 m clear connection at the shared junction.");

            foreach (float z in new[] { 12.5f, 14.5f, 15.5f })
            {
                Assert.That(
                    FindWidestFreeHorizontalRun(z, 18.25f, 22.75f),
                    Is.GreaterThanOrEqualTo(3.5f),
                    $"Exit run pinches below 3.5 m at Z {z:0.0}.");
            }

            Collider[] bowlBlockers = GetSceneBlockingColliders()
                .Where(collider => !IsFloor(collider))
                .Where(collider =>
                    collider.bounds.max.x > -9.5f && collider.bounds.min.x < 9.5f &&
                    collider.bounds.max.z > -7.5f && collider.bounds.min.z < -2.5f)
                .ToArray();
            Assert.That(
                bowlBlockers,
                Is.Empty,
                "Central Clash Bowl must remain free of colliding architecture and prop proxies: " +
                string.Join(", ", bowlBlockers.Select(collider => collider.name)));
        }

        [Test]
        public void AuthoredActorsObjectsSpursAndExit_HaveSafeColliderClearance()
        {
            Physics.SyncTransforms();
            Transform showcase = RequireRoot("SHOWCASE_S1_GRAYBOX");
            Collider[] fixedColliders = showcase.GetComponentsInChildren<Collider>(true)
                .Where(collider => collider.enabled && !collider.isTrigger && !IsFloor(collider))
                .ToArray();

            foreach (string actorName in new[]
                     {
                         "Player",
                         "AllyProxy",
                         "EnemyProxy_01",
                         "EnemyProxy_02"
                     })
            {
                AssertSubjectDoesNotPenetrateFixedGeometry(
                    RequireRoot(actorName),
                    fixedColliders,
                    actorName);
            }

            PrototypeInteractionTarget[] targets = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<PrototypeInteractionTarget>(true))
                .ToArray();
            foreach (PrototypeInteractionTarget target in targets)
            {
                AssertSubjectDoesNotPenetrateFixedGeometry(
                    target.transform,
                    fixedColliders,
                    target.RuntimeInstanceKey);
            }

            Transform markers = RequireChild(showcase, "04_LAYOUT_MARKERS");
            AssertCapsuleClear(
                RequireChild(markers, "FutureBinder_B-D2").position,
                0.25f,
                0.6f,
                "Future Binder B-D2");
            AssertCapsuleClear(
                RequireChild(markers, "FutureBinder_B-A3").position,
                0.25f,
                0.6f,
                "Future Binder B-A3");
            AssertCapsuleClear(
                RequireChild(markers, "FutureChair_C-D2").position,
                0.6f,
                1.2f,
                "Future Chair C-D2");

            CharacterController controller = RequireRoot("Player").GetComponent<CharacterController>();
            float playerRadius = controller.radius + controller.skinWidth;
            AssertCapsuleClear(
                RequireChild(markers, "ParkingBay").position,
                playerRadius,
                controller.height,
                "Parking Bay");
            AssertCapsuleClear(
                RequireChild(markers, "ExitTarget").position,
                playerRadius,
                controller.height,
                "Exit target");

            AssertMaskedRoute(
                "north supply spur approach",
                controller,
                new Vector2(0f, 10.5f),
                new Vector2(-7f, 11.5f),
                point => point.x >= -18f && point.x <= 0.5f &&
                         point.y >= 10.5f && point.y <= 15f);
            AssertMaskedRoute(
                "south recognition spur approach",
                controller,
                new Vector2(0f, -10.5f),
                new Vector2(-18f, -10f),
                point => point.x >= -21.5f && point.x <= 0.5f &&
                         point.y >= -14f && point.y <= -8.5f);
        }

        [Test]
        public void EntrySightlines_AreNotBlockedByShowcaseStaticGeometry()
        {
            Physics.SyncTransforms();
            Transform showcase = RequireRoot("SHOWCASE_S1_GRAYBOX");
            Vector3 observer = RequireRoot("Player").position + new Vector3(0f, 2.2f, 0.25f);

            AssertStaticLineOfSight(
                observer,
                RequireInteractionTarget("BINDER-DIRECT-01").transform.position + Vector3.up * 0.35f,
                showcase,
                "direct Binder");
            AssertStaticLineOfSight(
                observer,
                RequireInteractionTarget("CHAIR-DIRECT-01").transform.position + Vector3.up * 0.6f,
                showcase,
                "direct Chair");
            AssertStaticLineOfSight(
                observer,
                RequireRoot("AllyProxy").position + Vector3.up * 1.5f,
                showcase,
                "Ally");
            AssertStaticLineOfSight(
                observer,
                RequireRoot("EnemyProxy_01").position + Vector3.up * 1.65f,
                showcase,
                "Runner 1 upper body");
            AssertStaticLineOfSight(
                observer,
                RequireChild(showcase, "04_LAYOUT_MARKERS/ExitTarget").position + Vector3.up * 2.2f,
                showcase,
                "Exit sign line");
        }

        [Test]
        public void PreservedPrototypeScene_RemainsByteIdenticalToAcceptedRegressionBaseline()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string sourcePath = Path.GetFullPath(Path.Combine(projectRoot, SourceScenePath));
            byte[] bytes = File.ReadAllBytes(sourcePath);

            Assert.That(bytes, Has.Length.EqualTo(89175));
            using SHA256 sha256 = SHA256.Create();
            string hash = string.Concat(
                sha256.ComputeHash(bytes).Select(value => value.ToString("x2")));
            Assert.That(hash, Is.EqualTo(SourceSceneSha256));
        }

        [Test]
        public void ShowcaseScene_UsesGrayboxDependencies_AndIsNotAddedToBuildSettings()
        {
            string[] dependencies = AssetDatabase.GetDependencies(ShowcaseScenePath, true);
            Assert.That(dependencies, Has.None.Matches<string>(path =>
                path.IndexOf("_LocalArtReview", StringComparison.OrdinalIgnoreCase) >= 0 ||
                path.IndexOf("ArtSource", StringComparison.OrdinalIgnoreCase) >= 0 ||
                path.IndexOf("ProductionArt", StringComparison.OrdinalIgnoreCase) >= 0));
            Assert.That(
                EditorBuildSettings.scenes.Any(buildScene =>
                    string.Equals(buildScene.path, ShowcaseScenePath, StringComparison.OrdinalIgnoreCase)),
                Is.False);
        }

        private void AssertMaskedRoute(
            string routeName,
            CharacterController controller,
            Vector2 start,
            Vector2 goal,
            Func<Vector2, bool> isInsideMask)
        {
            Vector2Int startCell = ToGrid(start);
            Vector2Int goalCell = ToGrid(goal);
            var queue = new Queue<Vector2Int>();
            var visited = new HashSet<Vector2Int>();
            var clearance = new Dictionary<Vector2Int, bool>();

            bool IsOpen(Vector2Int cell)
            {
                Vector2 point = ToWorld(cell);
                if (Mathf.Abs(point.x) > 23.25f || Mathf.Abs(point.y) > 16.25f ||
                    !isInsideMask(point))
                {
                    return false;
                }

                if (!clearance.TryGetValue(cell, out bool isClear))
                {
                    isClear = IsVerticalCapsuleClear(
                        new Vector3(point.x, 0f, point.y),
                        controller.radius + controller.skinWidth + 0.02f,
                        controller.height,
                        controller.transform,
                        out _);
                    clearance.Add(cell, isClear);
                }

                return isClear;
            }

            Assert.That(isInsideMask(start), Is.True, $"{routeName} start is outside its mask.");
            Assert.That(isInsideMask(goal), Is.True, $"{routeName} goal is outside its mask.");
            Assert.That(IsOpen(startCell), Is.True, $"{routeName} start is blocked.");
            Assert.That(IsOpen(goalCell), Is.True, $"{routeName} goal is blocked.");

            queue.Enqueue(startCell);
            visited.Add(startCell);
            Vector2Int[] directions =
            {
                Vector2Int.left,
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.down
            };

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                if (current == goalCell)
                {
                    return;
                }

                foreach (Vector2Int direction in directions)
                {
                    Vector2Int next = current + direction;
                    if (visited.Add(next) && IsOpen(next))
                    {
                        queue.Enqueue(next);
                    }
                }
            }

            Assert.Fail(
                $"{routeName} is not continuously traversable by the current CharacterController " +
                $"capsule. Reachable samples: {visited.Count}.");
        }

        private static bool IsInsideWestLoopMask(Vector2 point)
        {
            bool southConnection = point.x >= -18.5f && point.x <= 0.5f &&
                                   point.y >= -6.5f && point.y <= -2.5f;
            bool outerLoop = point.x >= -22.5f && point.x <= -8.5f &&
                             point.y >= -6.5f && point.y <= 14f;
            bool northConnection = point.x >= -18.5f && point.x <= 0.5f &&
                                   point.y >= 10.5f && point.y <= 14f;
            bool junction = point.x >= -2.5f && point.x <= 0.5f &&
                            point.y >= 9.5f && point.y <= 11f;
            return southConnection || outerLoop || northConnection || junction;
        }

        private static bool IsInsideEastLoopMask(Vector2 point)
        {
            bool southConnection = point.x >= -0.5f && point.x <= 18.5f &&
                                   point.y >= -6.5f && point.y <= -2.5f;
            bool outerLoop = point.x >= 8.5f && point.x <= 22.5f &&
                             point.y >= -6.5f && point.y <= 14f;
            bool northConnection = point.x >= -0.5f && point.x <= 18.5f &&
                                   point.y >= 10.5f && point.y <= 14f;
            bool junction = point.x >= -0.5f && point.x <= 2.5f &&
                            point.y >= 9.5f && point.y <= 11f;
            return southConnection || outerLoop || northConnection || junction;
        }

        private float FindWidestFreeHorizontalRun(float z, float minimumX, float maximumX)
        {
            return FindWidestFreeRun(
                minimumX,
                maximumX,
                value => new Vector3(value, 0f, z));
        }

        private float FindWidestFreeVerticalRun(float x, float minimumZ, float maximumZ)
        {
            return FindWidestFreeRun(
                minimumZ,
                maximumZ,
                value => new Vector3(x, 0f, value));
        }

        private float FindWidestFreeRun(
            float minimum,
            float maximum,
            Func<float, Vector3> positionFactory)
        {
            const float sampleStep = 0.25f;
            int currentFreeSamples = 0;
            int maximumFreeSamples = 0;
            for (float value = minimum; value <= maximum + 0.001f; value += sampleStep)
            {
                bool isClear = IsVerticalCapsuleClear(
                    positionFactory(value),
                    0.01f,
                    2f,
                    null,
                    out _);
                if (isClear)
                {
                    currentFreeSamples++;
                    maximumFreeSamples = Math.Max(maximumFreeSamples, currentFreeSamples);
                }
                else
                {
                    currentFreeSamples = 0;
                }
            }

            return maximumFreeSamples * sampleStep;
        }

        private void AssertSubjectDoesNotPenetrateFixedGeometry(
            Transform subject,
            IEnumerable<Collider> fixedColliders,
            string subjectName)
        {
            Collider[] subjectColliders = subject.GetComponentsInChildren<Collider>(true)
                .Where(collider => collider.enabled && !collider.isTrigger)
                .ToArray();
            Assert.That(subjectColliders, Is.Not.Empty, $"{subjectName} has no physical proxy.");

            foreach (Collider subjectCollider in subjectColliders)
            {
                foreach (Collider fixedCollider in fixedColliders)
                {
                    if (!subjectCollider.bounds.Intersects(fixedCollider.bounds))
                    {
                        continue;
                    }

                    bool overlaps = Physics.ComputePenetration(
                        subjectCollider,
                        subjectCollider.transform.position,
                        subjectCollider.transform.rotation,
                        fixedCollider,
                        fixedCollider.transform.position,
                        fixedCollider.transform.rotation,
                        out _,
                        out float distance);
                    Assert.That(
                        !overlaps || distance <= 0.001f,
                        Is.True,
                        $"{subjectName} penetrates {fixedCollider.name} by {distance:0.000} m.");
                }
            }
        }

        private void AssertCapsuleClear(
            Vector3 floorPosition,
            float radius,
            float height,
            string subjectName)
        {
            bool isClear = IsVerticalCapsuleClear(
                floorPosition,
                radius,
                height,
                null,
                out string blocker);
            Assert.That(isClear, Is.True, $"{subjectName} clearance is blocked by {blocker}.");
        }

        private bool IsVerticalCapsuleClear(
            Vector3 floorPosition,
            float radius,
            float height,
            Transform ignoredRoot,
            out string blocker)
        {
            float safeHeight = Mathf.Max(height, radius * 2f);
            Vector3 bottom = floorPosition + Vector3.up * (radius + 0.02f);
            Vector3 top = floorPosition + Vector3.up * (safeHeight - radius + 0.02f);
            int hitCount = Physics.OverlapCapsuleNonAlloc(
                bottom,
                top,
                radius,
                overlapBuffer,
                ~0,
                QueryTriggerInteraction.Ignore);
            Assert.That(hitCount, Is.LessThan(overlapBuffer.Length), "Capsule overlap buffer overflowed.");

            for (int index = 0; index < hitCount; index++)
            {
                Collider collider = overlapBuffer[index];
                if (collider == null || collider.gameObject.scene != scene || IsFloor(collider))
                {
                    continue;
                }

                if (ignoredRoot != null && collider.transform.IsChildOf(ignoredRoot))
                {
                    continue;
                }

                blocker = collider.name;
                return false;
            }

            blocker = string.Empty;
            return true;
        }

        private Collider[] GetSceneBlockingColliders()
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Collider>(true))
                .Where(collider => collider.enabled && !collider.isTrigger &&
                                   collider.gameObject.activeInHierarchy)
                .ToArray();
        }

        private static bool IsFloor(Collider collider)
        {
            return string.Equals(collider.name, "Floor_48x34", StringComparison.Ordinal);
        }

        private void AssertStaticLineOfSight(
            Vector3 observer,
            Vector3 target,
            Transform showcaseRoot,
            string targetName)
        {
            Vector3 offset = target - observer;
            float distance = offset.magnitude;
            Collider[] blockers = Physics.RaycastAll(
                    observer,
                    offset / distance,
                    distance - 0.02f,
                    ~0,
                    QueryTriggerInteraction.Ignore)
                .Select(hit => hit.collider)
                .Where(collider => collider.gameObject.scene == scene &&
                                   collider.transform.IsChildOf(showcaseRoot) &&
                                   !IsFloor(collider))
                .Distinct()
                .ToArray();
            Assert.That(
                blockers,
                Is.Empty,
                $"Entry sightline to {targetName} is blocked by " +
                string.Join(", ", blockers.Select(collider => collider.name)) + ".");
        }

        private PrototypeInteractionTarget RequireInteractionTarget(string runtimeInstanceKey)
        {
            PrototypeInteractionTarget target = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<PrototypeInteractionTarget>(true))
                .SingleOrDefault(item => string.Equals(
                    item.RuntimeInstanceKey,
                    runtimeInstanceKey,
                    StringComparison.Ordinal));
            Assert.That(target, Is.Not.Null, $"Missing gameplay target: {runtimeInstanceKey}");
            return target;
        }

        private static Vector2Int ToGrid(Vector2 point)
        {
            return new Vector2Int(
                Mathf.RoundToInt(point.x / RouteGridStep),
                Mathf.RoundToInt(point.y / RouteGridStep));
        }

        private static Vector2 ToWorld(Vector2Int cell)
        {
            return new Vector2(cell.x * RouteGridStep, cell.y * RouteGridStep);
        }

        private Transform RequireRoot(string name)
        {
            GameObject root = scene.GetRootGameObjects()
                .SingleOrDefault(item => string.Equals(item.name, name, StringComparison.Ordinal));
            Assert.That(root, Is.Not.Null, $"Missing Scene root: {name}");
            return root.transform;
        }

        private static Transform RequireChild(Transform root, string path)
        {
            Transform child = root.Find(path);
            Assert.That(child, Is.Not.Null, $"Missing authored transform: {root.name}/{path}");
            return child;
        }

        private void AssertPlanar(string rootName, float x, float z)
        {
            Transform root = RequireRoot(rootName);
            Assert.That(root.position.x, Is.EqualTo(x).Within(0.001f));
            Assert.That(root.position.z, Is.EqualTo(z).Within(0.001f));
        }

        private static void AssertVector(Vector3 actual, Vector3 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.001f));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.001f));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.001f));
        }
    }
}
