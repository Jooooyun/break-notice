using System;
using System.Linq;
using BreakNotice.Gameplay.Encounter;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestShowcaseS3ContractTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v003.unity";

        private Scene scene;

        [SetUp]
        public void SetUp()
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath), Is.Not.Null);
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
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
        public void S3Scene_MaterializesExactOwnerApprovedPhysicalInventoryAndPools()
        {
            PrototypeInteractionTarget[] targets = Components<PrototypeInteractionTarget>();

            Assert.That(targets, Has.Length.EqualTo(10));
            Assert.That(targets.Count(target => target.ContentDefinitionId == "OBJ-P-L-001"),
                Is.EqualTo(5));
            Assert.That(targets.Count(target => target.ContentDefinitionId == "OBJ-P-M-001"),
                Is.EqualTo(3));
            Assert.That(targets.Count(target => target.Role == PrototypeInteractionRole.DirectBinder),
                Is.EqualTo(2));
            Assert.That(targets.Count(target => target.Role == PrototypeInteractionRole.AllyBinder),
                Is.EqualTo(3));
            Assert.That(targets.Count(target => target.Role == PrototypeInteractionRole.DirectChair),
                Is.EqualTo(2));
            Assert.That(targets.Count(target => target.Role == PrototypeInteractionRole.AllyChair),
                Is.EqualTo(1));
            Assert.That(targets.Select(target => target.RuntimeInstanceKey), Is.Unique);
            PrototypeInteractionTarget[] physicalSupply = targets.Where(target =>
                target.ContentDefinitionId == "OBJ-P-L-001" ||
                target.ContentDefinitionId == "OBJ-P-M-001").ToArray();
            Assert.That(physicalSupply.All(target => target.GetComponent<Rigidbody>() != null),
                Is.True);
            Assert.That(physicalSupply.All(target => target.GetComponent<Collider>() is BoxCollider),
                Is.True);
        }

        [Test]
        public void S3Scene_EnablesOneFControlledAutomaticFetchRelayAndTenObjectReset()
        {
            PrototypeAllyRelayController relay = Single<PrototypeAllyRelayController>();
            ShowcaseMobileAllyController mobile = Single<ShowcaseMobileAllyController>();
            AuthoredEncounterResetController reset = Single<AuthoredEncounterResetController>();
            PrototypeInteractionController interaction = Single<PrototypeInteractionController>();

            Assert.That(relay.enabled, Is.True);
            Assert.That(relay.RequiresExplicitFetch, Is.True);
            Assert.That(relay.SupplyTargets.Count, Is.EqualTo(4));
            Assert.That(relay.SupplyTargets.Count(target =>
                target.Role == PrototypeInteractionRole.AllyBinder), Is.EqualTo(3));
            Assert.That(relay.SupplyTargets.Count(target =>
                target.Role == PrototypeInteractionRole.AllyChair), Is.EqualTo(1));
            Assert.That(relay.PreparedTarget, Is.Null);
            Assert.That(relay.IsAwaitingFetchTask, Is.True);
            Assert.That(mobile.HasTaskMovement, Is.False);
            Assert.That(mobile.State, Is.EqualTo(ShowcaseMobileAllyState.HoldingPosition));
            Assert.That(mobile.IsFollowCommanded, Is.False);
            Assert.That(interaction.AllyRelayController, Is.SameAs(relay));
            Assert.That(interaction.ShowcaseMobileAllyController, Is.SameAs(mobile));
            Assert.That(reset.ExpectedObjectCount, Is.EqualTo(10));
        }

        [Test]
        public void S3Scene_RemovesReviewOnlyProxiesAndShowsExplicitCommandPrompt()
        {
            Assert.That(scene.GetRootGameObjects().SingleOrDefault(root =>
                root.name == "SHOWCASE_S3_FETCH_SUPPLY"), Is.Not.Null);
            Assert.That(scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Any(item => item.name.Contains("S3_NOT_IMPLEMENTED", StringComparison.Ordinal)),
                Is.False);

            InteractionReadabilityPresenter presenter =
                Single<InteractionReadabilityPresenter>();
            Assert.That(presenter.BuildStatusText(), Does.Contain("[F] FETCH"));
            Assert.That(presenter.BuildStatusText(), Does.Not.Contain("AIM AT"));
            Assert.That(presenter.BuildStatusText(), Does.Contain("NEAREST REACHABLE"));
            Assert.That(presenter.BuildStatusText(), Does.Contain("[Q] FOLLOW"));
            Assert.That(presenter.BuildStatusText(), Does.Contain("[G] REGROUP"));
        }

        private T Single<T>() where T : Component
        {
            T[] items = Components<T>();
            Assert.That(items, Has.Length.EqualTo(1), typeof(T).Name);
            return items[0];
        }

        private T[] Components<T>() where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }
    }
}
