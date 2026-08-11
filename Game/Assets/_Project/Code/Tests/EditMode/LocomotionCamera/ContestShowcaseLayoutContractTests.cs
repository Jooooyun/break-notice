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

        private Scene scene;

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
                { "Z5_EAST_PHYSICS_LANE", new Vector3(17f, 0f, 1f) },
                { "Z6_NORTH_TAG_ARENA_DRAWER_GATE", new Vector3(14.5f, 0f, 11f) },
                { "Z7_EXIT_RUN_NEXT_AREA_TEASE", new Vector3(20f, 0f, 15f) }
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
