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
        public IEnumerator AuthoredShowcase_WalkKeepsVisibleFeetNearAuthoredGround()
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
                var failures = new List<string>();

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
                    float groundY = FindAuthoredGroundY(driver.transform);
                    animator.Play(idleState, 0, 0f);
                    animator.Update(0f);
                    Assert.That(driver.ApplyVisualGroundingCorrection(), Is.True,
                        driver.Role.ToString());
                    float idleClearance = VisibleBounds(visual).min.y - groundY;
                    Transform leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                    Transform rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                    Assert.That(leftFoot, Is.Not.Null, driver.Role.ToString());
                    Assert.That(rightFoot, Is.Not.Null, driver.Role.ToString());
                    float idleLowestFoot = Mathf.Min(leftFoot.position.y, rightFoot.position.y) -
                        groundY;

                    animator.Play(walkState, 0, 0f);
                    animator.Update(0f);
                    Assert.That(animator.GetCurrentAnimatorStateInfo(0).fullPathHash,
                        Is.EqualTo(walkState), driver.Role.ToString());
                    float minimumClearance = float.PositiveInfinity;
                    float maximumClearance = float.NegativeInfinity;
                    float minimumLowestFoot = float.PositiveInfinity;
                    float maximumLowestFoot = float.NegativeInfinity;
                    for (int frame = 0; frame < 60; frame++)
                    {
                        animator.Update(1f / 60f);
                        Assert.That(driver.ApplyVisualGroundingCorrection(), Is.True,
                            driver.Role.ToString());

                        float clearance = VisibleBounds(visual).min.y - groundY;
                        float lowestFoot = Mathf.Min(
                            leftFoot.position.y,
                            rightFoot.position.y) - groundY;
                        minimumClearance = Mathf.Min(minimumClearance, clearance);
                        maximumClearance = Mathf.Max(maximumClearance, clearance);
                        minimumLowestFoot = Mathf.Min(minimumLowestFoot, lowestFoot);
                        maximumLowestFoot = Mathf.Max(maximumLowestFoot, lowestFoot);
                    }

                    Debug.Log(
                        $"DESIGNER_A_WALK_GROUND_AUDIT role={driver.Role} " +
                        $"groundY={groundY:F4} idleClearance={idleClearance:F4} " +
                        $"walkClearance=[{minimumClearance:F4},{maximumClearance:F4}] " +
                        $"idleLowestFoot={idleLowestFoot:F4} " +
                        $"walkLowestFoot=[{minimumLowestFoot:F4},{maximumLowestFoot:F4}] " +
                        $"modelLocalY={visual.GetChild(0).localPosition.y:F4}");

                    if (idleClearance < -0.01f || idleClearance > 0.01f)
                    {
                        failures.Add(
                            $"{driver.Role} idle clearance {idleClearance:F4}m is not grounded.");
                    }
                    if (minimumClearance < -0.01f || maximumClearance > 0.01f)
                    {
                        failures.Add(
                            $"{driver.Role} walk clearance " +
                            $"[{minimumClearance:F4},{maximumClearance:F4}]m is not grounded.");
                    }
                    if (maximumClearance - minimumClearance > 0.18f)
                    {
                        failures.Add(
                            $"{driver.Role} walk vertical swing " +
                            $"{maximumClearance - minimumClearance:F4}m is excessive.");
                    }

                    animator.Play(idleState, 0, 0f);
                    animator.Update(0f);
                    animator.cullingMode = previousCullingMode;
                    driver.enabled = true;
                }

                Assert.That(failures, Is.Empty, string.Join("\n", failures));
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        private static float FindAuthoredGroundY(Transform actor)
        {
            RaycastHit[] hits = Physics.RaycastAll(
                actor.position + Vector3.up * 2f,
                Vector3.down,
                8f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            RaycastHit groundHit = hits
                .Where(hit => !hit.collider.transform.IsChildOf(actor))
                .OrderBy(hit => hit.distance)
                .FirstOrDefault();
            Assert.That(groundHit.collider, Is.Not.Null,
                "No authored ground collider was found below " + actor.name + ".");
            return groundHit.point.y;
        }

        private static Bounds VisibleBounds(Transform visual)
        {
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true)
                .Where(renderer => renderer.enabled)
                .ToArray();
            Assert.That(renderers, Is.Not.Empty, visual.name);
            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return bounds;
        }
    }
}
