using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestShowcaseV2LayoutContractTests
    {
        private const string V2ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v002.unity";

        private const string V1ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v001.unity";

        private const string GrayboxScenePath =
            "Assets/_Project/Scenes/10_Prototype/Contest_SingleRoomGraybox.unity";

        private const string V1Sha256 =
            "1c8a306ec11559e718303c17254fa280a4a9001bc721b64a4b5bd1d144650d1a";

        private const string GrayboxSha256 =
            "0ed96ae20a74a8d91888dd108915eb9eeaa56da7d3ba6581b660a1e966e8de3a";

        private const string V2RootName =
            "SHOWCASE_V2_PROVISIONAL_LOCAL_REVIEW_ONLY";

        private const float GridStep = 0.5f;

        private readonly Collider[] overlapBuffer = new Collider[256];
        private Scene scene;

        [SetUp]
        public void SetUp()
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(V2ScenePath), Is.Not.Null);
            scene = EditorSceneManager.OpenScene(V2ScenePath, OpenSceneMode.Additive);
            Assert.That(scene.IsValid(), Is.True);
            Assert.That(scene.isLoaded, Is.True);
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
        public void StatusAndEnvelope_AreProvisional64By44_With4Point2MeterReference()
        {
            Transform root = RequireRoot(V2RootName);
            Transform status = RequireChild(root, "00_STATUS");
            string[] expectedStatus =
            {
                "LOCAL_REVIEW_ONLY",
                "PROVISIONAL_LAYOUT",
                "NOT_OWNER_APPROVED",
                "NOT_FINAL_MAP_LOCK",
                "NOT_PRODUCTION_ART",
                "NOT_S9_INTEGRATED",
                "S2_INCLUDED",
                "S3_NOT_IMPLEMENTED",
                "DESIGNER_B_RUNTIME_INTEGRATION_0"
            };

            Assert.That(status.childCount, Is.EqualTo(expectedStatus.Length));
            foreach (string marker in expectedStatus)
            {
                Assert.That(RequireChild(status, marker).GetComponents<Component>(), Has.Length.EqualTo(1));
            }

            Transform envelope = RequireChild(root, "01_STATIC_COLLISION/STATIC_ENVELOPE");
            BoxCollider floor = RequireChild(envelope, "PROXY_Floor_64x44")
                .GetComponent<BoxCollider>();
            Assert.That(floor, Is.Not.Null);
            Assert.That(floor.bounds.size.x, Is.EqualTo(64f).Within(0.001f));
            Assert.That(floor.bounds.size.z, Is.EqualTo(44f).Within(0.001f));
            Assert.That(floor.bounds.max.y, Is.EqualTo(0f).Within(0.001f));
            Assert.That(
                RequireChild(envelope, "PROXY_CeilingReference_4.2m").position.y,
                Is.EqualTo(4.2f).Within(0.001f));
            AssertVector(
                RequireChild(envelope, "PROXY_ExteriorBounds_X-32_TO_32_Z-22_TO_22").localScale,
                new Vector3(64f, 1f, 44f));
        }

        [Test]
        public void Zones_AreExactlyZ0ThroughZ7_AtProvisionalReviewCenters()
        {
            Transform zones = RequireChild(RequireRoot(V2RootName), "02_ZONES");
            var expected = new Dictionary<string, Vector3>
            {
                { "Z0_ENTRY_STAGING", new Vector3(0f, 0f, -19f) },
                { "Z1_DIRECT_EQUIPMENT_BAY", new Vector3(0f, 0f, -15f) },
                { "Z2_CENTRAL_CLASH_BOWL", new Vector3(0f, 0f, -8f) },
                { "Z3_WEST_LOGISTICS_LOOP", new Vector3(-24f, 0f, 3f) },
                { "Z4_CENTRAL_CUBICLE_RING", new Vector3(0f, 0f, 5f) },
                { "Z5_EAST_PHYSICS_LANE", new Vector3(23f, 0f, 3f) },
                { "Z6_NORTH_TAG_ARENA_DRAWER_GATE", new Vector3(17f, 0f, 15f) },
                { "Z7_EXIT_RUN_NEXT_AREA_TEASE", new Vector3(26f, 0f, 19f) }
            };

            Assert.That(zones.childCount, Is.EqualTo(8));
            foreach (KeyValuePair<string, Vector3> zone in expected)
            {
                AssertVector(RequireChild(zones, zone.Key).position, zone.Value);
            }
        }

        [Test]
        public void VisualReadability_HasEightZoneCuesBoundaryOpeningAndDenseOfficeSilhouettes()
        {
            Transform visual = RequireChild(RequireRoot(V2RootName), "10_VISUAL_READABILITY");
            Assert.That(visual.GetComponentsInChildren<Collider>(true), Is.Empty,
                "Readability geometry must never become decorative collision.");

            Transform boundary = RequireChild(visual, "BOUNDARY_AND_OPENING_READABILITY");
            Assert.That(
                RequireChild(boundary, "PERIMETER_CEILING_REFERENCE_4.2m")
                    .GetComponent<LineRenderer>(),
                Is.Not.Null);
            LineRenderer exitOpening = RequireChild(
                    boundary,
                    "AUTHORIZED_EXIT_OPENING_4.5m_OUTLINE")
                .GetComponent<LineRenderer>();
            Assert.That(exitOpening, Is.Not.Null);
            Assert.That(exitOpening.positionCount, Is.EqualTo(4));
            Assert.That(boundary.GetComponentsInChildren<LineRenderer>(true), Has.Length.EqualTo(6));
            Assert.That(boundary.GetComponentsInChildren<TextMesh>(true), Has.Length.EqualTo(2));
            Assert.That(
                RequireChild(boundary, "CEILING_GRID_SILHOUETTES_16").childCount,
                Is.EqualTo(16));

            Transform zoneCues = RequireChild(visual, "ZONE_VISUAL_CUES_8");
            Assert.That(zoneCues.childCount, Is.EqualTo(8));
            for (int zoneIndex = 0; zoneIndex < 8; zoneIndex++)
            {
                Transform cue = zoneCues.Cast<Transform>().Single(child =>
                    child.name.StartsWith($"ZONE_CUE_Z{zoneIndex}_", StringComparison.Ordinal));
                LineRenderer[] bands = cue.GetComponentsInChildren<LineRenderer>(true);
                TextMesh[] headers = cue.GetComponentsInChildren<TextMesh>(true);
                Assert.That(bands, Has.Length.EqualTo(1), cue.name);
                Assert.That(bands[0].name, Is.EqualTo($"FLOOR_BAND_Z{zoneIndex}"));
                Assert.That(bands[0].loop, Is.True, cue.name);
                Assert.That(headers, Has.Length.EqualTo(2), cue.name);
                Assert.That(headers.Any(header => header.name == $"FLOOR_HEADER_Z{zoneIndex}"),
                    Is.True, cue.name);
                Assert.That(headers.Any(header => header.name == $"VERTICAL_HEADER_Z{zoneIndex}"),
                    Is.True, cue.name);
            }

            Transform wayfinding = RequireChild(visual, "DIRECTIONAL_WAYFINDING");
            Assert.That(wayfinding.GetComponentsInChildren<TextMesh>(true), Has.Length.EqualTo(5));

            Transform density = RequireChild(visual, "OFFICE_DENSITY_NON_COLLIDING");
            Assert.That(RequireChild(density, "WORKSTATION_SILHOUETTES_12").childCount,
                Is.EqualTo(12));
            Assert.That(RequireChild(density, "ARCHIVE_STORAGE_SILHOUETTES_10").childCount,
                Is.EqualTo(10));
            Assert.That(RequireChild(density, "EQUIPMENT_SILHOUETTES_8").childCount,
                Is.EqualTo(8));
            Assert.That(RequireChild(density, "PLANT_BIN_DRESSING_SILHOUETTES_8").childCount,
                Is.EqualTo(8));
            Assert.That(
                RequireChild(density, "PERIMETER_WORKSTATION_SILHOUETTES_32").childCount,
                Is.EqualTo(32));
            Assert.That(density.GetComponentsInChildren<Renderer>(true), Has.Length.EqualTo(70));
            Assert.That(density.GetComponentsInChildren<Collider>(true), Is.Empty);
        }

        [Test]
        public void AllV2PartitionSemanticProxies_Are2Point1MetersGroundedAndKeepBaselineFootprints()
        {
            Transform root = RequireRoot(V2RootName);
            var expectedMeshNames = new HashSet<string>(StringComparer.Ordinal);
            Transform central = RequireChild(
                root,
                "04_CUBICLE_ISLAND/STATIC_CUBICLE_COLLISION/PROXY_LowPartitions_16");
            var centralExpected = new Dictionary<string, (Vector3 Position, Vector3 Scale)>
            {
                { "PROXY_Partition_-1_West", (new Vector3(-7.5f, 1.05f, -1f), new Vector3(5f, 2.1f, 0.2f)) },
                { "PROXY_Partition_-1_East", (new Vector3(7.5f, 1.05f, -1f), new Vector3(5f, 2.1f, 0.2f)) },
                { "PROXY_Partition_11_West", (new Vector3(-7.5f, 1.05f, 11f), new Vector3(5f, 2.1f, 0.2f)) },
                { "PROXY_Partition_11_East", (new Vector3(7.5f, 1.05f, 11f), new Vector3(5f, 2.1f, 0.2f)) },
                { "PROXY_Partition_West_1.5", (new Vector3(-10f, 1.05f, 1.5f), new Vector3(0.2f, 2.1f, 5f)) },
                { "PROXY_Partition_East_1.5", (new Vector3(10f, 1.05f, 1.5f), new Vector3(0.2f, 2.1f, 5f)) },
                { "PROXY_Partition_West_8.5", (new Vector3(-10f, 1.05f, 8.5f), new Vector3(0.2f, 2.1f, 5f)) },
                { "PROXY_Partition_East_8.5", (new Vector3(10f, 1.05f, 8.5f), new Vector3(0.2f, 2.1f, 5f)) },
                { "PROXY_Partition_Internal_West_3", (new Vector3(-6.5f, 1.05f, 3f), new Vector3(6f, 2.1f, 0.2f)) },
                { "PROXY_Partition_Internal_East_3", (new Vector3(6.5f, 1.05f, 3f), new Vector3(6f, 2.1f, 0.2f)) },
                { "PROXY_Partition_Internal_West_7", (new Vector3(-6.5f, 1.05f, 7f), new Vector3(6f, 2.1f, 0.2f)) },
                { "PROXY_Partition_Internal_East_7", (new Vector3(6.5f, 1.05f, 7f), new Vector3(6f, 2.1f, 0.2f)) },
                { "PROXY_Partition_Internal_-3.5_South", (new Vector3(-3.5f, 1.05f, 1f), new Vector3(0.2f, 2.1f, 4f)) },
                { "PROXY_Partition_Internal_-3.5_North", (new Vector3(-3.5f, 1.05f, 9f), new Vector3(0.2f, 2.1f, 4f)) },
                { "PROXY_Partition_Internal_3.5_South", (new Vector3(3.5f, 1.05f, 1f), new Vector3(0.2f, 2.1f, 4f)) },
                { "PROXY_Partition_Internal_3.5_North", (new Vector3(3.5f, 1.05f, 9f), new Vector3(0.2f, 2.1f, 4f)) }
            };
            Assert.That(central.childCount, Is.EqualTo(16));
            CollectionAssert.AreEquivalent(
                centralExpected.Keys,
                central.Cast<Transform>().Select(child => child.name));
            foreach (KeyValuePair<string, (Vector3 Position, Vector3 Scale)> expected in centralExpected)
            {
                Transform partition = RequireChild(central, expected.Key);
                AssertPartitionProxy(
                    partition,
                    expected.Value.Position,
                    expected.Value.Scale,
                    Quaternion.identity,
                    true);
                expectedMeshNames.Add(partition.name);
            }

            Transform perimeter = RequireChild(
                root,
                "10_VISUAL_READABILITY/OFFICE_DENSITY_NON_COLLIDING/" +
                "PERIMETER_WORKSTATION_SILHOUETTES_32");
            var perimeterExpected =
                new Dictionary<string, (Vector3 Position, Vector3 Scale, Quaternion Rotation)>();
            int perimeterIndex = 0;
            foreach (float x in new[] { -28f, -24f, -20f, -16f, -12f, -8f, -4f, 4f })
            {
                perimeterExpected.Add(
                    $"DENSITY_PROXY_PERIMETER_PARTITION_{perimeterIndex:00}",
                    (new Vector3(x, 1.05f, 20.72f),
                        new Vector3(2.8f, 2.1f, 0.12f), Quaternion.identity));
                perimeterIndex++;
            }
            foreach (float x in new[] { -22f, -16f, -10f, 10f, 16f, 22f })
            {
                perimeterExpected.Add(
                    $"DENSITY_PROXY_PERIMETER_PARTITION_{perimeterIndex:00}",
                    (new Vector3(x, 1.05f, -20.72f),
                        new Vector3(2.8f, 2.1f, 0.12f), Quaternion.identity));
                perimeterIndex++;
            }
            foreach (float x in new[] { -30.4f, 30.4f })
            {
                perimeterExpected.Add(
                    $"DENSITY_PROXY_PERIMETER_PARTITION_{perimeterIndex:00}",
                    (new Vector3(x + Mathf.Sign(x) * 0.62f, 1.05f, -15f),
                        new Vector3(2.8f, 2.1f, 0.12f),
                        Quaternion.Euler(0f, 90f, 0f)));
                perimeterIndex++;
            }
            Transform[] perimeterPartitions = perimeter.Cast<Transform>()
                .Where(child => child.name.StartsWith(
                    "DENSITY_PROXY_PERIMETER_PARTITION_",
                    StringComparison.Ordinal))
                .ToArray();
            Assert.That(perimeter.childCount, Is.EqualTo(32));
            Assert.That(perimeterPartitions, Has.Length.EqualTo(16));
            Assert.That(perimeter.Cast<Transform>().Count(child =>
                child.name.StartsWith("DENSITY_PROXY_PERIMETER_DESK_", StringComparison.Ordinal)),
                Is.EqualTo(16));
            CollectionAssert.AreEquivalent(
                perimeterExpected.Keys,
                perimeterPartitions.Select(child => child.name));
            foreach (KeyValuePair<string, (Vector3 Position, Vector3 Scale, Quaternion Rotation)>
                     expected in perimeterExpected)
            {
                Transform partition = RequireChild(perimeter, expected.Key);
                AssertPartitionProxy(
                    partition,
                    expected.Value.Position,
                    expected.Value.Scale,
                    expected.Value.Rotation,
                    false);
                expectedMeshNames.Add(partition.name);
            }

            Transform architecture = RequireChild(
                root,
                "09_DESIGNER_B_REPLACEMENT_SOCKETS/ARCHITECTURE_19");
            var socketExpected = new Dictionary<string, (Vector3 Position, Vector3 Scale)>
            {
                { "ENV_LowPartition_1m_v001", (new Vector3(-3.5f, 0f, 1f), new Vector3(0.2f, 2.1f, 1f)) },
                { "ENV_LowPartition_2m_v001", (new Vector3(-3.5f, 0f, 5f), new Vector3(0.2f, 2.1f, 2f)) },
                { "ENV_LowPartition_3m_v001", (new Vector3(-6.5f, 0f, 3f), new Vector3(3f, 2.1f, 0.2f)) },
                { "ENV_LowPartition_Corner_v001", (new Vector3(-10f, 0f, -1f), new Vector3(0.4f, 2.1f, 0.4f)) },
                { "ENV_LowPartition_TJunction_v001", (new Vector3(-3.5f, 0f, 3f), new Vector3(0.6f, 2.1f, 0.6f)) },
                { "ENV_LowPartition_EndCap_v001", (new Vector3(3.5f, 0f, 11f), new Vector3(0.3f, 2.1f, 0.3f)) }
            };
            Transform[] partitionSockets = architecture.Cast<Transform>()
                .Where(child => child.name.StartsWith(
                    "SOCKET_ENV_LowPartition_",
                    StringComparison.Ordinal))
                .ToArray();
            Assert.That(partitionSockets, Has.Length.EqualTo(6));
            CollectionAssert.AreEquivalent(
                socketExpected.Keys.Select(id => "SOCKET_" + id),
                partitionSockets.Select(child => child.name));
            foreach (KeyValuePair<string, (Vector3 Position, Vector3 Scale)> expected in socketExpected)
            {
                Transform socket = RequireChild(architecture, "SOCKET_" + expected.Key);
                AssertVector(socket.position, expected.Value.Position);
                Assert.That(Quaternion.Angle(socket.rotation, Quaternion.identity),
                    Is.LessThan(0.01f), expected.Key);
                AssertVector(
                    RequireChild(socket, "INTENDED_BOUNDS_PROVISIONAL_REVIEW_REQUIRED").localScale,
                    expected.Value.Scale);
                Transform proxy = RequireChild(socket, "VISIBLE_REVIEW_PROXY_" + expected.Key);
                AssertPartitionProxy(
                    proxy,
                    expected.Value.Position + Vector3.up * 1.05f,
                    expected.Value.Scale,
                    Quaternion.identity,
                    false);
                expectedMeshNames.Add(proxy.name);
            }

            string[] allPartitionMeshNames = root.GetComponentsInChildren<MeshRenderer>(true)
                .Where(renderer => renderer.name.IndexOf(
                    "Partition",
                    StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(renderer => renderer.name)
                .ToArray();
            Assert.That(allPartitionMeshNames, Has.Length.EqualTo(38));
            CollectionAssert.AreEquivalent(expectedMeshNames, allPartitionMeshNames,
                "Every v2 partition semantic mesh must be enumerated exactly once.");
        }

        [Test]
        public void FigureEight_CurrentPlayerCapsule_HasWestEastCentralAndNorthConnectivity()
        {
            CharacterController playerController = RequirePlayer()
                .GetComponent<CharacterController>();
            Assert.That(playerController, Is.Not.Null);
            Assert.That(playerController.radius, Is.EqualTo(0.45f).Within(0.001f));

            var south = new Vector2(0f, -8f);
            var north = new Vector2(0f, 15f);
            AssertMaskedRoute(
                "west loop",
                playerController,
                south,
                north,
                IsInsideWestLoopMask);
            AssertMaskedRoute(
                "east loop",
                playerController,
                south,
                north,
                IsInsideEastLoopMask);
            AssertMaskedRoute(
                "central cut",
                playerController,
                south,
                north,
                point => point.x >= -3f && point.x <= 3f &&
                         point.y >= -10f && point.y <= 18f);

            Transform topology = RequireChild(RequireRoot(V2RootName), "03_FIGURE_EIGHT");
            Assert.That(RequireChild(topology, "WEST_LOOP_MIN_5.5m").localScale.x,
                Is.GreaterThanOrEqualTo(5.5f));
            Assert.That(RequireChild(topology, "EAST_LANE_9m").localScale.x,
                Is.InRange(7f, 9f));
            Assert.That(RequireChild(topology, "NORTH_CROSS_MIN_6m").localScale.z,
                Is.GreaterThanOrEqualTo(6f));
            Assert.That(RequireChild(topology, "EXIT_RUN_MIN_4m").localScale.x,
                Is.GreaterThanOrEqualTo(4f));
        }

        [Test]
        public void ReviewLanesPocketsAndCentralBowl_HaveRequiredStaticClearance()
        {
            for (float z = -8f; z <= 13f; z += 1f)
            {
                Assert.That(
                    FindWidestFreeHorizontalRun(z, -31.5f, -10.5f),
                    Is.GreaterThanOrEqualTo(5.5f),
                    $"West loop pinches below 5.5 m at Z {z:0.0}.");
                Assert.That(
                    FindWidestFreeHorizontalRun(z, 10.5f, 31.5f),
                    Is.GreaterThanOrEqualTo(7f),
                    $"East lane pinches below 7 m at Z {z:0.0}.");
            }

            for (float x = -20f; x <= 20f; x += 2f)
            {
                Assert.That(
                    FindWidestFreeVerticalRun(x, 12f, 18f),
                    Is.GreaterThanOrEqualTo(6f),
                    $"North Cross pinches below 6 m at X {x:0.0}.");
            }

            Assert.That(
                FindWidestFreeHorizontalRun(19f, 23.75f, 28.25f),
                Is.GreaterThanOrEqualTo(4f),
                "Exit Run pinches below 4 m.");

            Collider[] bowlBlockers = GetV2BlockingColliders()
                .Where(collider => !IsFloor(collider))
                .Where(collider =>
                    collider.bounds.max.x > -12f && collider.bounds.min.x < 12f &&
                    collider.bounds.max.z > -13f && collider.bounds.min.z < -3f)
                .ToArray();
            Assert.That(
                bowlBlockers,
                Is.Empty,
                "The 24 x 10 m Central Clash Bowl must contain no static collision: " +
                string.Join(", ", bowlBlockers.Select(collider => collider.name)));

            Transform markers = RequireChild(RequireRoot(V2RootName), "05_LAYOUT_MARKERS");
            foreach (string pocketName in new[]
                     {
                         "TURN_POCKET_WEST_8x8",
                         "TURN_POCKET_EAST_9x9",
                         "TURN_POCKET_Z6_9x9"
                     })
            {
                Transform pocket = RequireChild(markers, pocketName);
                Assert.That(pocket.localScale.x, Is.GreaterThanOrEqualTo(8f), pocketName);
                Assert.That(pocket.localScale.z, Is.GreaterThanOrEqualTo(8f), pocketName);
                Assert.That(IsVerticalCapsuleClear(pocket.position, 0.48f, 1.9f),
                    Is.True, pocketName);
            }

            Assert.That(
                new[]
                {
                    "APPROACH_01_WEST_NORTH",
                    "APPROACH_02_EAST_NORTH",
                    "APPROACH_03_CENTRAL_CUT"
                }.Count(name => markers.Find(name) != null),
                Is.EqualTo(3));
        }

        [Test]
        public void ActorsAndObjects_HaveExactCountsPositionsAndReviewDimensions()
        {
            ShowcaseMobileAllyController[] allies = GetSceneComponents<ShowcaseMobileAllyController>();
            Assert.That(allies, Has.Length.EqualTo(1));
            Assert.That(RequirePlayer().name, Is.EqualTo("GAMEPLAY_CHR00_PLAYER"));
            Assert.That(RequireRoot("GAMEPLAY_ENY01_RUNNER_01"), Is.Not.Null);
            Assert.That(RequireRoot("GAMEPLAY_ENY01_RUNNER_02"), Is.Not.Null);

            PrototypeInteractionTarget[] targets = GetSceneComponents<PrototypeInteractionTarget>();
            Assert.That(targets, Has.Length.EqualTo(7));
            Assert.That(
                targets.Select(target => target.RuntimeInstanceKey)
                    .Distinct(StringComparer.Ordinal)
                    .Count(),
                Is.EqualTo(7));
            Assert.That(targets.Count(target => target.ContentDefinitionId == "OBJ-P-L-001"),
                Is.EqualTo(3));
            Assert.That(targets.Count(target => target.ContentDefinitionId == "OBJ-P-M-001"),
                Is.EqualTo(2));
            Assert.That(targets.Count(target => target.ContentDefinitionId == "OBJ-P-M-002"),
                Is.EqualTo(1));
            Assert.That(targets.Count(target => target.ContentDefinitionId == "OBJ-P-H-001"),
                Is.EqualTo(1));
            Assert.That(targets.All(target => target.name.StartsWith("GAMEPLAY_", StringComparison.Ordinal)),
                Is.True);

            AssertBounds("BINDER-DIRECT-01", new Vector3(0.29f, 0.32f, 0.08f));
            AssertBounds("BINDER-ALLY-01", new Vector3(0.29f, 0.32f, 0.08f));
            AssertBounds("BINDER-ALLY-02", new Vector3(0.29f, 0.32f, 0.08f));
            AssertBounds("CHAIR-DIRECT-01", new Vector3(0.729f, 0.998f, 0.697f));
            AssertBounds("CHAIR-ALLY-01", new Vector3(0.729f, 0.998f, 0.697f));
            AssertBounds("DRAWER-01", new Vector3(0.760f, 0.998f, 0.744f));
            AssertBounds("COPIER-01", new Vector3(0.614f, 0.979f, 0.655f));

            Transform future = RequireChild(
                RequireRoot(V2RootName),
                "08_S3_NOT_IMPLEMENTED_REVIEW_PROXIES");
            Assert.That(future.childCount, Is.EqualTo(3));
            Assert.That(future.Cast<Transform>().Count(child => child.name.Contains("BINDER")),
                Is.EqualTo(2));
            Assert.That(future.Cast<Transform>().Count(child => child.name.Contains("CHAIR")),
                Is.EqualTo(1));
            Assert.That(future.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(future.GetComponentsInChildren<PrototypeInteractionTarget>(true), Is.Empty);

            Assert.That(3 + 2, Is.EqualTo(5), "Binder 3 runtime + 2 S3 review proxies.");
            Assert.That(2 + 1, Is.EqualTo(3), "Chair 2 runtime + 1 S3 review proxy.");
        }

        [Test]
        public void S2MobileAlly_IsSingularConnectedAndClearAcrossExpandedLayout()
        {
            ShowcaseMobileAllyController mobile =
                GetSceneComponents<ShowcaseMobileAllyController>().Single();
            CharacterController controller = mobile.GetComponent<CharacterController>();
            PrototypeAllyRelayController legacyRelay =
                mobile.GetComponent<PrototypeAllyRelayController>();

            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.radius, Is.EqualTo(0.48f).Within(0.001f));
            Assert.That(controller.height, Is.EqualTo(1.9f).Within(0.001f));
            Assert.That(legacyRelay, Is.Not.Null);
            Assert.That(legacyRelay.enabled, Is.False);
            Assert.That(mobile.AnchorCount, Is.EqualTo(18));
            Assert.That(mobile.AnchorLinks.Count, Is.EqualTo(20));
            Assert.That(mobile.IsAnchorNetworkConnected(), Is.True);

            for (int index = 0; index < mobile.AnchorCount; index++)
            {
                Transform anchor = mobile.FollowAnchors[index];
                Assert.That(anchor.name, Is.EqualTo($"V2-FA-{index:00}"));
                Assert.That(anchor.position.x, Is.InRange(-31f, 31f));
                Assert.That(anchor.position.z, Is.InRange(-21f, 21f));
                Assert.That(
                    IsVerticalCapsuleClear(anchor.position, controller.radius, controller.height),
                    Is.True,
                    anchor.name);
            }
        }

        [Test]
        public void DesignerBSockets_HaveExact60UniqueIdsVisibleCoverageAndNoRuntimeOrCollisionAuthority()
        {
            Transform root = RequireChild(
                RequireRoot(V2RootName),
                "09_DESIGNER_B_REPLACEMENT_SOCKETS");
            var expected = new Dictionary<string, string[]>
            {
                {
                    "ARCHITECTURE_19",
                    new[]
                    {
                        "ENV_Wall_2m_v001", "ENV_Wall_4m_v001", "ENV_Wall_6m_v001",
                        "ENV_Wall_InnerCorner_v001", "ENV_Wall_OuterCorner_v001",
                        "ENV_Wall_DoorModule_v001", "ENV_LowPartition_1m_v001",
                        "ENV_LowPartition_2m_v001", "ENV_LowPartition_3m_v001",
                        "ENV_LowPartition_Corner_v001", "ENV_LowPartition_TJunction_v001",
                        "ENV_LowPartition_EndCap_v001", "ENV_Floor_4m_v001",
                        "ENV_Ceiling_4m_v001", "ENV_Column_070_v001",
                        "ENV_ExitDoor_Frame_v001", "ENV_SecurityShutter_VistaOnly_v001",
                        "ENV_FluorescentFixture_A_v001", "ENV_FluorescentFixture_B_v001"
                    }
                },
                {
                    "FIXED_DESK_3",
                    new[]
                    {
                        "PRP_FixedDesk_Single_v001", "PRP_FixedDesk_Double_v001",
                        "PRP_FixedDesk_LShape_v001"
                    }
                },
                {
                    "GAMEPLAY_RECOGNITION_4",
                    new[]
                    {
                        "PRP_OfficeChair_Gameplay_v002", "PRP_RollingDrawer_Gameplay_v002",
                        "PRP_Copier_Recognition_v002", "PRP_FileBinder_Gameplay_v002"
                    }
                },
                {
                    "DRESSING_22",
                    new[]
                    {
                        "DEC_MonitorCluster_A_v001", "DEC_MonitorCluster_B_v001",
                        "DEC_MonitorCluster_C_v001", "DEC_KeyboardPhoneCluster_v001",
                        "DEC_PaperStack_A_v001", "DEC_PaperStack_B_v001",
                        "DEC_ArchiveBox_A_v001", "DEC_ArchiveBox_B_v001",
                        "DEC_Plant_A_v001", "DEC_Plant_B_v001", "DEC_WasteBin_A_v001",
                        "DEC_WasteBin_B_v001", "DEC_CableBundle_VisualOnly_v001",
                        "DEC_FloorTape_Set_v001", "DEC_AssetTag_Set_v001",
                        "DEC_KEEP_CLEAR_v001", "DEC_EXIT_DIRECTION_v001",
                        "DEC_EQUIPMENT_TRANSFER_v001", "DEC_MOBILE_STORAGE_PARKING_v001",
                        "DEC_WheelMarks_v001", "DEC_WallScuffs_v001", "DEC_ImpactMarks_v001"
                    }
                },
                {
                    "SHARED_MATERIAL_9",
                    new[]
                    {
                        "MAT_PaintedGypsumWall_v001", "MAT_SuspendedCeilingTile_v001",
                        "MAT_CommercialCarpet_v001", "MAT_WornVinylFloor_v001",
                        "MAT_LaminateCompositeDesk_v001", "MAT_PaintedSteel_v001",
                        "MAT_WornPlasticRubber_v001", "MAT_PaperCardboard_v001",
                        "MAT_OfficeFabricUpholstery_v001"
                    }
                },
                {
                    "DECAL_ATLAS_3",
                    new[]
                    {
                        "DECAL_SignageAssetLabel_Atlas_v001",
                        "DECAL_WearScuffWheelMark_Atlas_v001",
                        "DECAL_DustTonerContactMark_Atlas_v001"
                    }
                }
            };

            var allIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, string[]> category in expected)
            {
                Transform categoryRoot = RequireChild(root, category.Key);
                Assert.That(categoryRoot.childCount, Is.EqualTo(category.Value.Length), category.Key);
                var actualIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (Transform socket in categoryRoot)
                {
                    string prefix = category.Key == "DRESSING_22"
                        ? "DRESSING_SOCKET_"
                        : "SOCKET_";
                    Assert.That(socket.name.StartsWith(prefix, StringComparison.Ordinal), Is.True);
                    string id = socket.name.Substring(prefix.Length);
                    Assert.That(actualIds.Add(id), Is.True, id);
                    Assert.That(allIds.Add(id), Is.True, id);
                    Assert.That(socket.localScale, Is.EqualTo(Vector3.one));
                    Assert.That(socket.GetComponents<Component>(), Has.Length.EqualTo(1), id);
                    Assert.That(socket.GetComponentsInChildren<Collider>(true), Is.Empty, id);
                    Assert.That(socket.GetComponentsInChildren<PrototypeInteractionTarget>(true), Is.Empty, id);
                    Assert.That(
                        socket.Cast<Transform>().Count(child =>
                            child.name.StartsWith("INTENDED_BOUNDS_", StringComparison.Ordinal)),
                        Is.EqualTo(1), id);
                    Assert.That(
                        socket.Cast<Transform>().Count(child =>
                            child.name.StartsWith("PIVOT_", StringComparison.Ordinal)),
                        Is.EqualTo(1), id);

                    bool isCatalog = category.Key == "SHARED_MATERIAL_9" ||
                                     category.Key == "DECAL_ATLAS_3";
                    if (isCatalog)
                    {
                        Assert.That(
                            socket.Cast<Transform>().Any(child =>
                                child.name.StartsWith("VISIBLE_REVIEW_PROXY_", StringComparison.Ordinal)),
                            Is.False,
                            id + " must remain a non-spatial review swatch.");
                        Assert.That(
                            socket.Cast<Transform>().Count(child =>
                                child.name.StartsWith("REVIEW_SWATCH_NON_RUNTIME_", StringComparison.Ordinal)),
                            Is.EqualTo(2),
                            id);
                        Assert.That(socket.GetComponentsInChildren<Renderer>(true).Length,
                            Is.GreaterThanOrEqualTo(2), id);
                    }
                    else
                    {
                        Assert.That(
                            socket.Find("VISIBLE_REVIEW_PROXY_" + id),
                            Is.Not.Null,
                            id);
                        Assert.That(
                            socket.Find("VISIBLE_REVIEW_OUTLINE_" + id),
                            Is.Not.Null,
                            id);
                        Assert.That(socket.GetComponentsInChildren<Renderer>(true).Length,
                            Is.GreaterThanOrEqualTo(2), id);
                    }
                }

                CollectionAssert.AreEquivalent(category.Value, actualIds, category.Key);
            }

            Assert.That(allIds.Count, Is.EqualTo(60));
        }

        [Test]
        public void StaticCollisionAndSightlines_AvoidDecorativeBlockersAndTargetOcclusion()
        {
            Collider[] staticColliders = GetV2BlockingColliders()
                .Where(collider => !IsFloor(collider))
                .ToArray();
            Assert.That(staticColliders, Is.Not.Empty);
            foreach (Collider collider in staticColliders)
            {
                Assert.That(collider.name.StartsWith("PROXY_", StringComparison.Ordinal),
                    Is.True, collider.name);
                Assert.That(HasStaticAncestor(collider.transform), Is.True, collider.name);
            }

            Transform root = RequireRoot(V2RootName);
            Transform sockets = RequireChild(root, "09_DESIGNER_B_REPLACEMENT_SOCKETS");
            Assert.That(sockets.GetComponentsInChildren<Collider>(true), Is.Empty);

            Vector3 entryObserver = new Vector3(0f, 1.55f, -19f);
            AssertStaticSightline(entryObserver, new Vector3(0f, 1f, -8f), "Central Bowl");
            AssertStaticSightline(entryObserver,
                RequireTarget("BINDER-DIRECT-01").transform.position + Vector3.up * 0.15f,
                "Direct Binder");
            AssertStaticSightline(entryObserver,
                RequireTarget("CHAIR-DIRECT-01").transform.position + Vector3.up * 0.5f,
                "Direct Chair");

            Vector3 northObserver = new Vector3(16f, 1.55f, 17f);
            Transform markers = RequireChild(root, "05_LAYOUT_MARKERS");
            AssertStaticSightline(northObserver,
                RequireTarget("DRAWER-01").transform.position + Vector3.up * 0.5f,
                "Drawer Gate");
            AssertStaticSightline(northObserver,
                RequireChild(markers, "PARKING_BAY_REVIEW").position + Vector3.up,
                "Parking Bay");
            AssertStaticSightline(northObserver,
                RequireChild(markers, "EXIT_TARGET").position + Vector3.up,
                "Exit");
        }

        [Test]
        public void PreservedScenesRemainByteIdentical_AndV2HasNoProductionAssetDependency()
        {
            Assert.That(ComputeSha256(V1ScenePath), Is.EqualTo(V1Sha256));
            Assert.That(ComputeSha256(GrayboxScenePath), Is.EqualTo(GrayboxSha256));

            string[] dependencies = AssetDatabase.GetDependencies(V2ScenePath, true);
            string[] forbiddenProductionDependencies = dependencies
                .Where(path => path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                .Where(path =>
                    path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".obj", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".tga", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".psd", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            Assert.That(forbiddenProductionDependencies, Is.Empty,
                "v002 must not integrate Production Art: " +
                string.Join(", ", forbiddenProductionDependencies));
        }

        private void AssertMaskedRoute(
            string routeName,
            CharacterController controller,
            Vector2 start,
            Vector2 goal,
            Func<Vector2, bool> mask)
        {
            Vector2Int startCell = ToGrid(start);
            Vector2Int goalCell = ToGrid(goal);
            var queue = new Queue<Vector2Int>();
            var visited = new HashSet<Vector2Int>();

            bool IsOpen(Vector2Int cell)
            {
                Vector2 point = ToWorld(cell);
                return mask(point) && IsVerticalCapsuleClear(
                    new Vector3(point.x, 0f, point.y),
                    controller.radius + controller.skinWidth,
                    controller.height);
            }

            Assert.That(IsOpen(startCell), Is.True, routeName + " start");
            Assert.That(IsOpen(goalCell), Is.True, routeName + " goal");
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

            Assert.Fail($"{routeName} is disconnected; reachable samples: {visited.Count}.");
        }

        private static bool IsInsideWestLoopMask(Vector2 point)
        {
            bool south = point.x >= -28f && point.x <= 1f &&
                         point.y >= -11f && point.y <= -5f;
            bool outer = point.x >= -29f && point.x <= -19f &&
                         point.y >= -11f && point.y <= 18f;
            bool north = point.x >= -28f && point.x <= 1f &&
                         point.y >= 12f && point.y <= 18f;
            return south || outer || north;
        }

        private static bool IsInsideEastLoopMask(Vector2 point)
        {
            bool south = point.x >= -1f && point.x <= 29f &&
                         point.y >= -11f && point.y <= -5f;
            bool outer = point.x >= 18f && point.x <= 29f &&
                         point.y >= -11f && point.y <= 18f;
            bool north = point.x >= -1f && point.x <= 29f &&
                         point.y >= 12f && point.y <= 18f;
            return south || outer || north;
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
            int current = 0;
            int widest = 0;
            for (float value = minimum; value <= maximum + 0.001f; value += sampleStep)
            {
                if (IsVerticalCapsuleClear(positionFactory(value), 0.53f, 1.9f))
                {
                    current++;
                    widest = Math.Max(widest, current);
                }
                else
                {
                    current = 0;
                }
            }

            return widest * sampleStep;
        }

        private bool IsVerticalCapsuleClear(Vector3 floorPosition, float radius, float height)
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
            Assert.That(hitCount, Is.LessThan(overlapBuffer.Length));
            for (int index = 0; index < hitCount; index++)
            {
                Collider collider = overlapBuffer[index];
                if (collider == null || collider.gameObject.scene != scene || IsFloor(collider))
                {
                    continue;
                }

                if (collider.transform.IsChildOf(RequireRoot(V2RootName)))
                {
                    return false;
                }
            }

            return true;
        }

        private Collider[] GetV2BlockingColliders()
        {
            return RequireRoot(V2RootName)
                .GetComponentsInChildren<Collider>(true)
                .Where(collider => collider.enabled && !collider.isTrigger &&
                                   collider.gameObject.activeInHierarchy)
                .ToArray();
        }

        private void AssertStaticSightline(Vector3 observer, Vector3 target, string targetName)
        {
            Vector3 offset = target - observer;
            float distance = offset.magnitude;
            Collider[] blockers = Physics.RaycastAll(
                    observer,
                    offset / distance,
                    Mathf.Max(0f, distance - 0.05f),
                    ~0,
                    QueryTriggerInteraction.Ignore)
                .Select(hit => hit.collider)
                .Where(collider => collider != null &&
                                   collider.gameObject.scene == scene &&
                                   collider.transform.IsChildOf(RequireRoot(V2RootName)) &&
                                   !IsFloor(collider))
                .Distinct()
                .ToArray();
            Assert.That(blockers, Is.Empty,
                $"{targetName} sightline is blocked by " +
                string.Join(", ", blockers.Select(collider => collider.name)));
        }

        private void AssertBounds(string runtimeKey, Vector3 expected)
        {
            PrototypeInteractionTarget target = RequireTarget(runtimeKey);
            AssertVector(target.transform.lossyScale, expected);
            Collider collider = target.GetComponent<Collider>();
            Assert.That(collider, Is.Not.Null, runtimeKey);
            AssertVector(collider.bounds.size, expected);
            Assert.That(collider.bounds.min.y, Is.EqualTo(0f).Within(0.002f), runtimeKey);
        }

        private static void AssertPartitionProxy(
            Transform partition,
            Vector3 expectedPosition,
            Vector3 expectedScale,
            Quaternion expectedRotation,
            bool expectsCollider)
        {
            AssertVector(partition.position, expectedPosition);
            AssertVector(partition.localScale, expectedScale);
            Assert.That(Quaternion.Angle(partition.rotation, expectedRotation),
                Is.LessThan(0.01f), partition.name);

            MeshRenderer renderer = partition.GetComponent<MeshRenderer>();
            Assert.That(renderer, Is.Not.Null, partition.name);
            Assert.That(renderer.bounds.min.y, Is.EqualTo(0f).Within(0.002f), partition.name);
            Assert.That(renderer.bounds.size.y, Is.EqualTo(2.1f).Within(0.002f), partition.name);

            Collider[] colliders = partition.GetComponents<Collider>();
            if (expectsCollider)
            {
                Assert.That(colliders, Has.Length.EqualTo(1), partition.name);
                Assert.That(colliders[0], Is.TypeOf<BoxCollider>(), partition.name);
                Assert.That(colliders[0].bounds.min.y, Is.EqualTo(0f).Within(0.002f),
                    partition.name);
                Assert.That(colliders[0].bounds.size.y, Is.EqualTo(2.1f).Within(0.002f),
                    partition.name);
            }
            else
            {
                Assert.That(colliders, Is.Empty, partition.name);
            }
        }

        private PrototypeInteractionTarget RequireTarget(string runtimeKey)
        {
            PrototypeInteractionTarget target = GetSceneComponents<PrototypeInteractionTarget>()
                .SingleOrDefault(candidate => string.Equals(
                    candidate.RuntimeInstanceKey,
                    runtimeKey,
                    StringComparison.Ordinal));
            Assert.That(target, Is.Not.Null, runtimeKey);
            return target;
        }

        private GameObject RequirePlayer()
        {
            PrototypeInteractionStateOwner owner =
                GetSceneComponents<PrototypeInteractionStateOwner>().SingleOrDefault();
            Assert.That(owner, Is.Not.Null);
            return owner.gameObject;
        }

        private T[] GetSceneComponents<T>() where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }

        private Transform RequireRoot(string name)
        {
            GameObject root = scene.GetRootGameObjects()
                .SingleOrDefault(candidate =>
                    string.Equals(candidate.name, name, StringComparison.Ordinal));
            Assert.That(root, Is.Not.Null, "Missing Scene root: " + name);
            return root.transform;
        }

        private static Transform RequireChild(Transform root, string path)
        {
            Transform child = root.Find(path);
            Assert.That(child, Is.Not.Null, $"Missing authored transform: {root.name}/{path}");
            return child;
        }

        private static bool HasStaticAncestor(Transform transform)
        {
            Transform current = transform.parent;
            while (current != null)
            {
                if (current.name.StartsWith("STATIC_", StringComparison.Ordinal))
                {
                    return true;
                }
                current = current.parent;
            }
            return false;
        }

        private static bool IsFloor(Collider collider)
        {
            return string.Equals(collider.name, "PROXY_Floor_64x44", StringComparison.Ordinal);
        }

        private static Vector2Int ToGrid(Vector2 point)
        {
            return new Vector2Int(
                Mathf.RoundToInt(point.x / GridStep),
                Mathf.RoundToInt(point.y / GridStep));
        }

        private static Vector2 ToWorld(Vector2Int cell)
        {
            return new Vector2(cell.x * GridStep, cell.y * GridStep);
        }

        private static string ComputeSha256(string assetPath)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string fullPath = Path.GetFullPath(Path.Combine(projectRoot, assetPath));
            using var stream = File.OpenRead(fullPath);
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(stream))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private static void AssertVector(Vector3 actual, Vector3 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.002f));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.002f));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.002f));
        }
    }
}
