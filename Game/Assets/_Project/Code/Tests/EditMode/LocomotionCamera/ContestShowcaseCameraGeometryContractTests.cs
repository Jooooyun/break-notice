using System.Collections.Generic;
using System.Linq;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestShowcaseCameraGeometryContractTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v007.unity";

        private static readonly HashSet<string> ExpectedHardColliders = new()
        {
            "PROXY_Floor_64x44",
            "PROXY_Wall_South",
            "PROXY_Wall_West",
            "PROXY_Wall_East",
            "PROXY_Wall_North_West",
            "PROXY_Wall_North_East",
            "PROXY_Exit_Frame_Lintel",
            "PROXY_PhysicsColumn"
        };

        private static readonly HashSet<string> ExpectedSoftColliders = new()
        {
            "PROXY_Partition_-1_West",
            "PROXY_Partition_-1_East",
            "PROXY_Partition_11_West",
            "PROXY_Partition_11_East",
            "PROXY_Partition_West_1.5",
            "PROXY_Partition_East_1.5",
            "PROXY_Partition_West_8.5",
            "PROXY_Partition_East_8.5",
            "PROXY_Partition_Internal_West_3",
            "PROXY_Partition_Internal_East_3",
            "PROXY_Partition_Internal_West_7",
            "PROXY_Partition_Internal_East_7",
            "PROXY_Partition_Internal_-3.5_South",
            "PROXY_Partition_Internal_-3.5_North",
            "PROXY_Partition_Internal_3.5_South",
            "PROXY_Partition_Internal_3.5_North",
            "PROXY_FixedDesk_-6.5_1",
            "PROXY_FixedDesk_6.5_1",
            "PROXY_FixedDesk_-6.5_5",
            "PROXY_FixedDesk_6.5_5",
            "PROXY_FixedDesk_-6.5_9",
            "PROXY_FixedDesk_6.5_9",
            "PROXY_SupplyDesk_West",
            "PROXY_FixedDesk_East_Inner",
            "PROXY_FixedDesk_East_Outer",
            "PROXY_CrashDesk_North"
        };

        private Scene scene;

        [SetUp]
        public void SetUp()
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath), Is.Not.Null);
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);
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
        public void V007CameraGeometry_UsesDisjointHardAndSoftAllowLists()
        {
            int hardLayer = LayerMask.NameToLayer("CameraStaticWorld");
            int softLayer = LayerMask.NameToLayer("CameraSoftOccluder");
            Assert.That(hardLayer, Is.GreaterThanOrEqualTo(0));
            Assert.That(softLayer, Is.GreaterThanOrEqualTo(0));
            Assert.That(softLayer, Is.Not.EqualTo(hardLayer));

            ShowcaseCameraWorldBlockerRoot[] hardRoots = ComponentsInScene<
                ShowcaseCameraWorldBlockerRoot>();
            ShowcaseCameraSoftOccluderRoot[] softRoots = ComponentsInScene<
                ShowcaseCameraSoftOccluderRoot>();

            Assert.That(hardRoots, Has.Length.EqualTo(2));
            Assert.That(softRoots, Has.Length.EqualTo(5));
            Assert.That(hardRoots.Select(root => root.name), Is.EquivalentTo(new[]
            {
                "STATIC_ENVELOPE",
                "PROXY_PhysicsColumn"
            }));
            Assert.That(softRoots.Select(root => root.name), Is.EquivalentTo(new[]
            {
                "STATIC_CUBICLE_COLLISION",
                "PROXY_SupplyDesk_West",
                "PROXY_FixedDesk_East_Inner",
                "PROXY_FixedDesk_East_Outer",
                "PROXY_CrashDesk_North"
            }));

            Collider[] hardColliders = hardRoots
                .SelectMany(root => root.GetComponentsInChildren<Collider>(true))
                .Distinct()
                .ToArray();
            Collider[] softColliders = softRoots
                .SelectMany(root => root.GetComponentsInChildren<Collider>(true))
                .Distinct()
                .ToArray();

            Assert.That(hardColliders.Select(collider => collider.name),
                Is.EquivalentTo(ExpectedHardColliders));
            Assert.That(softColliders.Select(collider => collider.name),
                Is.EquivalentTo(ExpectedSoftColliders));
            Assert.That(hardColliders.Intersect(softColliders), Is.Empty);
            Assert.That(hardColliders.All(collider => collider.gameObject.layer == hardLayer),
                Is.True);
            Assert.That(softColliders.All(collider => collider.gameObject.layer == softLayer),
                Is.True);
        }

        [Test]
        public void V007SoftOccluders_KeepGameplayCollisionAndRenderingEnabled()
        {
            ShowcaseCameraSoftOccluderRoot[] softRoots = ComponentsInScene<
                ShowcaseCameraSoftOccluderRoot>();
            Collider[] colliders = softRoots
                .SelectMany(root => root.GetComponentsInChildren<Collider>(true))
                .Distinct()
                .ToArray();
            Renderer[] renderers = softRoots
                .SelectMany(root => root.GetComponentsInChildren<Renderer>(true))
                .Distinct()
                .ToArray();

            Assert.That(colliders, Has.Length.EqualTo(26));
            Assert.That(colliders.All(collider =>
                    collider.enabled && !collider.isTrigger && collider.gameObject.activeInHierarchy),
                Is.True,
                "Soft camera occluders must keep their gameplay collision enabled.");
            Assert.That(renderers, Has.Length.EqualTo(26));
            Assert.That(renderers.All(renderer =>
                    renderer.enabled && renderer.gameObject.activeInHierarchy),
                Is.True,
                "Soft visibility treatment must remain renderer-only and must not deactivate geometry.");
            Assert.That(renderers.All(renderer =>
                    (GameObjectUtility.GetStaticEditorFlags(renderer.gameObject) &
                     StaticEditorFlags.OccluderStatic) == 0),
                Is.True,
                "Soft renderer geometry must not remain a baked hard occluder.");
        }

        [Test]
        public void V007ShowcaseRoot_IsNotAnImplicitHardBlocker()
        {
            GameObject showcaseRoot = scene.GetRootGameObjects().Single(root =>
                root.name == "SHOWCASE_V2_PROVISIONAL_LOCAL_REVIEW_ONLY");
            Assert.That(showcaseRoot.GetComponent<ShowcaseCameraWorldBlockerRoot>(), Is.Null,
                "The complete showcase hierarchy must never be promoted to hard camera world.");
            Assert.That(showcaseRoot.GetComponent<ShowcaseCameraSoftOccluderRoot>(), Is.Null);
        }

        private T[] ComponentsInScene<T>() where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }
    }
}
