using System;
using System.Collections.Generic;
using System.Linq;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestDesignerACharacterIntegrationTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v003.unity";
        private const string VisualName = "RUNTIME_DESIGNER_A_VISUAL";
        private const string ControllerRoot =
            "Assets/_Project/Art/Animation/DesignerAControllers";
        private const string AnimationRoot =
            "Assets/_Project/Art/Animation";

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
        public void ShowcaseScene_UsesFourUniqueTexturedHumanoidRuntimeVisuals()
        {
            ShowcaseCharacterAnimationDriver[] drivers = Components<
                ShowcaseCharacterAnimationDriver>();

            Assert.That(drivers, Has.Length.EqualTo(4));
            Assert.That(drivers.Select(driver => driver.Role), Is.Unique);
            Assert.That(drivers.Select(driver => driver.Role), Is.EquivalentTo(new[]
            {
                ShowcaseCharacterAnimationRole.Player,
                ShowcaseCharacterAnimationRole.Ally,
                ShowcaseCharacterAnimationRole.EnemyNormal,
                ShowcaseCharacterAnimationRole.EnemyElite
            }));

            foreach (ShowcaseCharacterAnimationDriver driver in drivers)
            {
                Transform visual = driver.transform.Find(VisualName);
                Assert.That(visual, Is.Not.Null, driver.Role.ToString());
                Assert.That(driver.IsRuntimeConfigured, Is.True, driver.Role.ToString());
                Assert.That(driver.Animator.applyRootMotion, Is.False, driver.Role.ToString());
                Assert.That(driver.Animator.avatar, Is.Not.Null, driver.Role.ToString());
                Assert.That(driver.Animator.avatar.isValid, Is.True, driver.Role.ToString());
                Assert.That(driver.Animator.avatar.isHuman, Is.True, driver.Role.ToString());

                Renderer[] productionRenderers = visual.GetComponentsInChildren<Renderer>(true);
                Assert.That(productionRenderers, Is.Not.Empty, driver.Role.ToString());
                Assert.That(productionRenderers.All(renderer => renderer.enabled), Is.True,
                    driver.Role.ToString());
                Assert.That(productionRenderers.SelectMany(renderer => renderer.sharedMaterials)
                    .Where(material => material != null)
                    .All(material => material.mainTexture != null), Is.True,
                    driver.Role.ToString());

                Renderer[] grayboxRenderers = driver.GetComponentsInChildren<Renderer>(true)
                    .Where(renderer => !renderer.transform.IsChildOf(visual))
                    .ToArray();
                Assert.That(grayboxRenderers.All(renderer => !renderer.enabled), Is.True,
                    driver.Role.ToString());
            }
        }

        [Test]
        public void AnimationCatalogs_ReferenceEveryApprovedRuntimeClip()
        {
            AssertCatalog("CHR00_Player.controller", "CHR00", 61);
            AssertCatalog("CHR01_Ally.controller", "CHR01", 51);
            AssertCatalog("ENY01_Normal.controller", "ENY01_SHARED", 54);
            AssertCatalog("ENY01_Elite.controller", "ENY01_SHARED", 54);
        }

        [Test]
        public void EnemyEligibility_NormalExcludesAndEliteIncludesSixEliteOnlyAnimations()
        {
            AnimatorController normal = LoadController("ENY01_Normal.controller");
            AnimatorController elite = LoadController("ENY01_Elite.controller");

            Assert.That(StateNames(normal).Count(name =>
                name.StartsWith("ENY01_ELITE_ONLY__", StringComparison.Ordinal)), Is.Zero);
            Assert.That(StateNames(elite).Count(name =>
                name.StartsWith("ENY01_ELITE_ONLY__", StringComparison.Ordinal)), Is.EqualTo(6));

            string[] normalShared = StateNames(normal)
                .Where(name => name.StartsWith("ENY01_SHARED__", StringComparison.Ordinal))
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            string[] eliteShared = StateNames(elite)
                .Where(name => name.StartsWith("ENY01_SHARED__", StringComparison.Ordinal))
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            Assert.That(eliteShared, Is.EqualTo(normalShared));
        }

        [Test]
        public void ImportedDelivery_ContainsFourModelsAndOneHundredSeventyTwoHumanoidClips()
        {
            string[] models = AssetDatabase.FindAssets(
                    "t:Model",
                    new[] { "Assets/_Project/Art/Characters" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.Contains("/DesignerA/", StringComparison.Ordinal))
                .ToArray();
            string[] animations = AssetDatabase.FindAssets(
                    "t:Model",
                    new[] { AnimationRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.Contains("/DesignerA/", StringComparison.Ordinal))
                .ToArray();

            Assert.That(models, Has.Length.EqualTo(4));
            Assert.That(animations, Has.Length.EqualTo(172));
            foreach (string path in models.Concat(animations))
            {
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                Assert.That(importer, Is.Not.Null, path);
                Assert.That(importer.animationType, Is.EqualTo(ModelImporterAnimationType.Human),
                    path);
            }
        }

        private void AssertCatalog(string fileName, string prefix, int expectedCount)
        {
            AnimatorController controller = LoadController(fileName);
            HashSet<AnimationClip> clips = new HashSet<AnimationClip>();
            foreach (AnimatorState state in controller.layers
                         .SelectMany(layer => layer.stateMachine.states)
                         .Select(child => child.state)
                         .Where(state => state.name.StartsWith(prefix + "__", StringComparison.Ordinal)))
            {
                Assert.That(state.motion, Is.TypeOf<AnimationClip>(), state.name);
                clips.Add((AnimationClip)state.motion);
            }

            Assert.That(clips, Has.Count.EqualTo(expectedCount), fileName);
        }

        private static AnimatorController LoadController(string fileName)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                ControllerRoot + "/" + fileName);
            Assert.That(controller, Is.Not.Null, fileName);
            return controller;
        }

        private static string[] StateNames(AnimatorController controller)
        {
            return controller.layers
                .SelectMany(layer => layer.stateMachine.states)
                .Select(child => child.state.name)
                .ToArray();
        }

        private T[] Components<T>() where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }
    }
}
