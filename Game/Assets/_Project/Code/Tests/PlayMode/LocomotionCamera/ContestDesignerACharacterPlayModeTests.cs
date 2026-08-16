using System.Collections;
using System.Collections.Generic;
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
                    Assert.That(animator.stabilizeFeet, Is.False, driver.Role.ToString());
                    Assert.That(animator.GetCurrentAnimatorStateInfo(0).fullPathHash,
                        Is.Not.Zero, driver.Role.ToString());

                    Transform visual = driver.transform.Find("RUNTIME_DESIGNER_A_VISUAL");
                    Assert.That(visual, Is.Not.Null, driver.Role.ToString());
                    Transform model = visual.GetChild(0);
                    Assert.That(Vector3.Dot(model.forward, driver.transform.forward),
                        Is.GreaterThan(0.999f), driver.Role.ToString());
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

        [UnityTest]
        public IEnumerator AuthoredShowcase_WalkDoesNotMutateAuthoredVisualRoot()
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
                    .OrderBy(driver => driver.Role)
                    .ToArray();
                Assert.That(drivers, Has.Length.EqualTo(4));

                yield return null;
                yield return null;

                int idleState = Animator.StringToHash("Base Layer.Idle");
                int walkState = Animator.StringToHash("Base Layer.Walk");
                foreach (ShowcaseCharacterAnimationDriver driver in drivers)
                {
                    Animator animator = driver.Animator;
                    Transform visual = driver.transform.Find("RUNTIME_DESIGNER_A_VISUAL");
                    Assert.That(animator, Is.Not.Null, driver.Role.ToString());
                    Assert.That(visual, Is.Not.Null, driver.Role.ToString());

                    SkinnedMeshRenderer[] skinnedRenderers = visual
                        .GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
                    {
                        renderer.updateWhenOffscreen = true;
                    }

                    driver.enabled = false;
                    AnimatorCullingMode previousCullingMode = animator.cullingMode;
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    Vector3 visualBaseline = visual.localPosition;
                    animator.Play(idleState, 0, 0f);
                    animator.Update(0f);
                    Transform leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                    Transform rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                    Assert.That(leftFoot, Is.Not.Null, driver.Role.ToString());
                    Assert.That(rightFoot, Is.Not.Null, driver.Role.ToString());

                    animator.Play(walkState, 0, 0f);
                    animator.Update(0f);
                    Assert.That(animator.GetCurrentAnimatorStateInfo(0).fullPathHash,
                        Is.EqualTo(walkState), driver.Role.ToString());
                    for (int frame = 0; frame < 60; frame++)
                    {
                        animator.Update(1f / 60f);
                        Assert.That(Vector3.Distance(visual.localPosition, visualBaseline),
                            Is.LessThan(0.0001f),
                            driver.Role + " presentation must not rewrite the authored root.");
                    }

                    animator.Play(idleState, 0, 0f);
                    animator.Update(0f);
                    animator.cullingMode = previousCullingMode;
                    driver.enabled = true;
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
