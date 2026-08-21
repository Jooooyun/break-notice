using System.Collections;
using System.Linq;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestRunnerValidationRosterPlayModeTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/20_Showcase/" +
            "Contest_OperationsFloor_Showcase_v007.unity";

        [UnityTearDown]
        public IEnumerator TearDownLoadedScene()
        {
            Scene cleanup = SceneManager.CreateScene(
                "RUNNER_VALIDATION_ROSTER_TEST_CLEANUP");
            SceneManager.SetActiveScene(cleanup);
            for (int index = SceneManager.sceneCount - 1; index >= 0; index--)
            {
                Scene loaded = SceneManager.GetSceneAt(index);
                if (loaded == cleanup || !loaded.isLoaded)
                {
                    continue;
                }
                AsyncOperation unload = SceneManager.UnloadSceneAsync(loaded);
                if (unload != null)
                {
                    yield return unload;
                }
            }
        }

        [UnityTest]
        public IEnumerator ManualValidationRoster_SpawnsThreeNormalAndOneEliteInDistinctLanes()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));

            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            Assert.That(
                SceneComponents<ShowcaseRunnerValidationRosterSpawner>(),
                Is.Empty,
                "Batch verification must not auto-install the manual roster.");
            Assert.That(pressure.Runners, Has.Length.EqualTo(2));

            ShowcaseRunnerAgent normalTemplate = pressure.Runners
                .Single(runner => !runner.IsElite);
            ShowcaseRunnerValidationRosterSpawner spawner =
                pressure.gameObject.AddComponent<
                    ShowcaseRunnerValidationRosterSpawner>();
            spawner.Configure(
                pressure,
                pressure.PlayerTransform,
                normalTemplate);
            Assert.That(
                spawner.TrySpawnManualValidationRoster(true),
                Is.True);
            yield return null;

            ShowcaseRunnerAgent[] roster = pressure.Runners;
            Assert.That(roster, Has.Length.EqualTo(4));
            Assert.That(roster.Count(runner => !runner.IsElite), Is.EqualTo(3));
            Assert.That(roster.Count(runner => runner.IsElite), Is.EqualTo(1));
            Assert.That(spawner.RuntimeCloneCount, Is.EqualTo(2));
            Assert.That(roster.Select(runner => runner.RunnerId).Distinct().Count(),
                Is.EqualTo(4));
            Assert.That(pressure.TargetableRunners, Has.Length.EqualTo(4));

            Transform player = pressure.PlayerTransform;
            Vector3 forward = Vector3.ProjectOnPlane(
                player.forward,
                Vector3.up).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            bool hasLeft = false;
            bool hasRight = false;
            bool hasForward = false;
            bool hasRear = false;
            for (int index = 0; index < roster.Length; index++)
            {
                ShowcaseRunnerAgent runner = roster[index];
                Assert.That(runner, Is.Not.Null);
                Assert.That(runner.gameObject.activeInHierarchy, Is.True);
                Assert.That(runner.Health, Is.Not.Null);
                Assert.That(runner.GetComponent<Rigidbody>(), Is.Not.Null);
                Assert.That(runner.GetComponent<ShowcaseBoneCombatRig>(), Is.Not.Null);

                Vector3 offset = Vector3.ProjectOnPlane(
                    runner.transform.position - player.position,
                    Vector3.up);
                Assert.That(offset.magnitude, Is.InRange(6.5f, 10.5f),
                    runner.RunnerId);
                float side = Vector3.Dot(offset, right);
                float depth = Vector3.Dot(offset, forward);
                hasLeft |= side < -2f;
                hasRight |= side > 2f;
                hasForward |= depth > 2f;
                hasRear |= depth < -0.25f;

                for (int otherIndex = index + 1;
                     otherIndex < roster.Length;
                     otherIndex++)
                {
                    float separation = Vector3.ProjectOnPlane(
                        roster[otherIndex].transform.position -
                            runner.transform.position,
                        Vector3.up).magnitude;
                    Assert.That(separation, Is.GreaterThan(2f),
                        $"{runner.RunnerId} overlaps " +
                        $"{roster[otherIndex].RunnerId}.");
                }
            }

            Assert.That(hasLeft && hasRight && hasForward && hasRear, Is.True,
                "The temporary roster must cover both flanks and front/rear lanes.");

            Vector3[] baselines = roster
                .Select(runner => runner.BaselinePosition)
                .ToArray();
            pressure.ResetToRunBaseline();
            for (int index = 0; index < roster.Length; index++)
            {
                Rigidbody body = roster[index].GetComponent<Rigidbody>();
                Assert.That(Vector3.Distance(
                        body.position,
                        baselines[index]),
                    Is.LessThan(0.001f),
                    roster[index].RunnerId);
            }
        }

        private static T[] SceneComponents<T>() where T : Component
        {
            return SceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }
    }
}
