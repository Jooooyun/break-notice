using System.Collections;
using System.Linq;
using BreakNotice.Gameplay.Encounter;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestShowcaseS6PlayModeTests
    {
        private const string S5ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v005.unity";
        private const string S6ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v006.unity";

        [UnityTearDown]
        public IEnumerator TearDownLoadedShowcaseScene()
        {
            Scene cleanup = SceneManager.CreateScene("S6_TEST_CLEANUP");
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
        public IEnumerator S5RemainsPreservedAndS6HasTwoRunnersOnePressureOwner()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S5ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            Assert.That(SceneComponents<ShowcaseRunnerPressureController>(), Is.Empty);

            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S6ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            ShowcaseRunnerAgent[] runners = pressure.Runners;
            ShowcaseRunnerPressurePresenter presenter =
                SceneComponents<ShowcaseRunnerPressurePresenter>().Single();
            PrototypeInteractionController interaction =
                SceneComponents<PrototypeInteractionController>().Single();

            Assert.That(runners.Length, Is.EqualTo(2));
            Assert.That(runners.Select(runner => runner.RunnerId).Distinct().Count(), Is.EqualTo(2));
            Assert.That(runners.All(runner => runner.GetComponent<Rigidbody>().isKinematic), Is.True);
            Assert.That(presenter.PressureController, Is.SameAs(pressure));
            Assert.That(interaction.ShowcaseRunnerPressureController, Is.SameAs(pressure));
            Assert.That(pressure.ActiveAttackTokenCount, Is.LessThanOrEqualTo(1));
        }

        [UnityTest]
        public IEnumerator ShortGrabQRescueAndCombatTagAwardOneBreakBadge()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S6ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            ShowcaseRunnerAgent runner = SceneComponents<ShowcaseRunnerAgent>()
                .OrderBy(item => item.RunnerId).First();
            PrototypeInteractionController interaction =
                SceneComponents<PrototypeInteractionController>().Single();
            ShowcaseTeamContextController team =
                SceneComponents<ShowcaseTeamContextController>().Single();
            ShowcaseMobileAllyController ally =
                SceneComponents<ShowcaseMobileAllyController>().Single();

            interaction.transform.position = runner.transform.position +
                new Vector3(0f, 0f, -1f);
            ally.transform.position = interaction.transform.position + Vector3.right * 1.5f;
            Physics.SyncTransforms();
            Assert.That(pressure.TryAcquireAttackToken(runner), Is.True);
            Assert.That(runner.TryCommitShortGrab(), Is.True);
            Assert.That(pressure.PlayerGrabbed, Is.True);
            Assert.That(team.ResolveCurrentQContext(), Is.EqualTo(ShowcaseQContext.BreakGrab));
            Assert.That(interaction.ProcessTeamContextQ(), Is.True);
            Assert.That(pressure.PlayerGrabbed, Is.False);
            Assert.That(team.HasBreak, Is.True);
            Assert.That(runner.State, Is.EqualTo(ShowcaseRunnerState.Staggered));

            Assert.That(team.ResolveCurrentQContext(), Is.EqualTo(ShowcaseQContext.CombatTag));
            Assert.That(interaction.ProcessTeamContextQ(), Is.True);
            Assert.That(team.BadgeCount, Is.EqualTo(1));
            Assert.That(pressure.RescueCount, Is.EqualTo(1));
            Assert.That(pressure.CombatTagCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ResetRestoresRunnerBaselinesAndClearsAttackToken()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S6ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            ShowcaseRunnerAgent runner = SceneComponents<ShowcaseRunnerAgent>().First();
            AuthoredEncounterResetController reset =
                SceneComponents<AuthoredEncounterResetController>().Single();
            Vector3 baseline = runner.BaselinePosition;

            Assert.That(pressure.TryAcquireAttackToken(runner), Is.True);
            runner.ReceivePhysicalStagger(true);
            runner.transform.position += Vector3.right * 3f;
            Assert.That(reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
            yield return null;

            Assert.That(pressure.ActiveAttackTokenCount, Is.Zero);
            Assert.That(pressure.PlayerGrabbed, Is.False);
            Assert.That(runner.State, Is.EqualTo(ShowcaseRunnerState.Patrol));
            Assert.That(Vector3.Distance(runner.transform.position, baseline), Is.LessThan(0.001f));
        }

        private static T[] SceneComponents<T>() where T : Component
        {
            return SceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }
    }
}
