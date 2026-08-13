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
    public sealed class ContestShowcaseS4PlayModeTests
    {
        private const string S3ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v003.unity";
        private const string S4ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v004.unity";

        [UnityTearDown]
        public IEnumerator TearDownLoadedShowcaseScene()
        {
            Scene cleanup = SceneManager.CreateScene("S4_TEST_CLEANUP");
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
        public IEnumerator AcceptedS3SceneRemainsPreservedAndS4HasSingularComposition()
        {
            AsyncOperation s3Load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                S3ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return s3Load;
            Scene s3Scene = SceneManager.GetActiveScene();
            Assert.That(SceneComponents<ShowcaseTeamContextController>(s3Scene), Is.Empty);
            Assert.That(SceneComponents<ShowcaseTeamContextPresenter>(s3Scene), Is.Empty);

            AsyncOperation s4Load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                S4ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return s4Load;
            Scene s4Scene = SceneManager.GetActiveScene();
            ShowcaseTeamContextController team =
                SceneComponents<ShowcaseTeamContextController>(s4Scene).Single();
            ShowcaseTeamContextPresenter presenter =
                SceneComponents<ShowcaseTeamContextPresenter>(s4Scene).Single();
            InteractionReadabilityPresenter interactionPresenter =
                SceneComponents<InteractionReadabilityPresenter>(s4Scene).Single();
            ShowcaseMobileAllyPresenter mobilePresenter =
                SceneComponents<ShowcaseMobileAllyPresenter>(s4Scene).Single();
            PrototypeInteractionController interaction =
                SceneComponents<PrototypeInteractionController>(s4Scene).Single();

            Assert.That(interaction.ShowcaseTeamContextController, Is.SameAs(team));
            Assert.That(presenter.TeamContext, Is.SameAs(team));
            Assert.That(team.ResolveCurrentQContext(),
                Is.EqualTo(ShowcaseQContext.ToggleFollow));
            Assert.That(presenter.BuildQPromptText(), Is.EqualTo("[Q] FOLLOW"));
            Assert.That(interactionPresenter.BuildStatusText(), Does.Not.Contain("[Q]"));
            Assert.That(mobilePresenter.BuildStatusText(), Does.Not.Contain("[Q]"));
            Assert.That(presenter.BuildReadinessText(),
                Is.EqualTo("PASS [ ]  BRACE [ ]  BREAK [ ]"));
        }

        [UnityTest]
        public IEnumerator SceneQFallbackTogglesFollowAndResetClearsTeamReadiness()
        {
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                S4ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return load;
            Scene scene = SceneManager.GetActiveScene();
            ShowcaseTeamContextController team =
                SceneComponents<ShowcaseTeamContextController>(scene).Single();
            ShowcaseTeamContextPresenter presenter =
                SceneComponents<ShowcaseTeamContextPresenter>(scene).Single();
            PrototypeInteractionController interaction =
                SceneComponents<PrototypeInteractionController>(scene).Single();
            ShowcaseMobileAllyController mobile =
                SceneComponents<ShowcaseMobileAllyController>(scene).Single();
            AuthoredEncounterResetController reset =
                SceneComponents<AuthoredEncounterResetController>(scene).Single();

            Assert.That(mobile.IsFollowCommanded, Is.False);
            Assert.That(interaction.ProcessTeamContextQ(), Is.True);
            Assert.That(team.LastQContext, Is.EqualTo(ShowcaseQContext.ToggleFollow));
            Assert.That(team.LastQAccepted, Is.True);
            Assert.That(mobile.IsFollowCommanded, Is.True);
            Assert.That(presenter.BuildQPromptText(), Is.EqualTo("[Q] HOLD POSITION"));

            Assert.That(team.RecordPassSuccess(), Is.True);
            Assert.That(team.RecordBreakSuccess(), Is.True);
            Assert.That(team.IsTeamReady, Is.True);
            Assert.That(reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
            yield return null;

            Assert.That(team.Badges, Is.EqualTo(ShowcaseTeamReadinessBadge.None));
            Assert.That(team.IsTeamReady, Is.False);
            Assert.That(team.QAttemptSequence, Is.Zero);
        }

        private static T[] SceneComponents<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }
    }
}
