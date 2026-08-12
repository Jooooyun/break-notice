using System.Collections;
using System.Linq;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestDesignerACharacterPlayModeTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v003.unity";

        [UnityTest]
        public IEnumerator AuthoredShowcase_FourDesignerAAnimatorsInitializeWithoutRootMotion()
        {
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
            Assert.That(load, Is.Not.Null);
            while (!load.isDone)
            {
                yield return null;
            }

            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);
            try
            {
                ShowcaseCharacterAnimationDriver[] drivers = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        ShowcaseCharacterAnimationDriver>(true))
                    .ToArray();
                Assert.That(drivers, Has.Length.EqualTo(4));

                yield return null;
                yield return null;

                foreach (ShowcaseCharacterAnimationDriver driver in drivers)
                {
                    Animator animator = driver.Animator;
                    Assert.That(animator, Is.Not.Null, driver.Role.ToString());
                    Assert.That(animator.isInitialized, Is.True, driver.Role.ToString());
                    Assert.That(animator.runtimeAnimatorController, Is.Not.Null,
                        driver.Role.ToString());
                    Assert.That(animator.applyRootMotion, Is.False, driver.Role.ToString());
                    Assert.That(animator.GetCurrentAnimatorStateInfo(0).fullPathHash,
                        Is.Not.Zero, driver.Role.ToString());
                }
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }
    }
}
