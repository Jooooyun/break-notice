using System.Collections;
using System.Linq;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay;
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
    public sealed class ContestShowcaseS7PlayModeTests
    {
        private const string S6ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v006.unity";
        private const string S7ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v007.unity";

        [UnityTearDown]
        public IEnumerator TearDownLoadedShowcaseScene()
        {
            Scene cleanup = SceneManager.CreateScene("S7_TEST_CLEANUP");
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
        public IEnumerator S6RemainsPreservedAndS7HasOneObjectiveComposition()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S6ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            Assert.That(SceneComponents<ShowcaseObjectiveController>(), Is.Empty);

            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S7ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseObjectiveController objective =
                SceneComponents<ShowcaseObjectiveController>().Single();
            ShowcaseDrawerObjectiveReceiver receiver =
                SceneComponents<ShowcaseDrawerObjectiveReceiver>().Single();
            ShowcaseObjectivePresenter presenter =
                SceneComponents<ShowcaseObjectivePresenter>().Single();

            Assert.That(receiver, Is.Not.Null);
            Assert.That(presenter.ObjectiveController, Is.SameAs(objective));
            Assert.That(objective.DrawerTarget.RuntimeInstanceKey, Is.EqualTo("DRAWER-01"));
            Assert.That(objective.DrawerBody, Is.Not.Null);
            Assert.That(objective.DrawerBody.isKinematic, Is.False);
            Assert.That(Vector3.Distance(objective.ParkingBayCenter.position,
                new Vector3(22f, 0f, 15f)), Is.LessThan(0.001f));
            Assert.That(Vector3.Distance(objective.ExitCenter.position,
                new Vector3(26f, 0f, 21f)), Is.LessThan(0.001f));
        }

        [UnityTest]
        public IEnumerator DirectChairImpact_ParkingAndExitCompleteTheRun()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S7ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseObjectiveController objective =
                SceneComponents<ShowcaseObjectiveController>().Single();

            Assert.That(objective.TryRegisterDrawerImpact(
                ShowcaseDrawerSolution.DirectChairThrow,
                objective.ParkingBayCenter.position - objective.DrawerBody.position,
                6f), Is.True);
            MoveBody(objective.DrawerBody,
                objective.ParkingBayCenter.position + Vector3.up * 0.5f);
            objective.EvaluateObjectiveState();
            Assert.That(objective.DrawerSolved, Is.True);
            Assert.That(objective.ExitOpen, Is.True);
            Assert.That(objective.DrawerSolution,
                Is.EqualTo(ShowcaseDrawerSolution.DirectChairThrow));
            Assert.That(objective.DrawerBody.isKinematic, Is.True);

            MoveCharacter(objective.ExitCenter.position);
            objective.EvaluateObjectiveState();
            Assert.That(objective.RunComplete, Is.True);
            Assert.That(objective.ClearTimeSeconds, Is.GreaterThanOrEqualTo(0f));
            Assert.That(ShowcaseObjectivePresenter.BuildObjectiveText(objective),
                Is.EqualTo("DRAWER PARKED — EXIT OPEN — RUN TO THE GREEN EXIT"));
        }

        [UnityTest]
        public IEnumerator AuthoredReset_RestoresDrawerAndClearsObjectiveResult()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S7ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseObjectiveController objective =
                SceneComponents<ShowcaseObjectiveController>().Single();
            AuthoredEncounterResetController reset =
                SceneComponents<AuthoredEncounterResetController>().Single();
            Vector3 baseline = objective.DrawerBody.position;

            Assert.That(objective.TryRegisterDrawerImpact(
                ShowcaseDrawerSolution.RunnerCausalCollision,
                Vector3.right,
                5f), Is.True);
            MoveBody(objective.DrawerBody,
                objective.ParkingBayCenter.position + Vector3.up * 0.5f);
            objective.EvaluateObjectiveState();
            Assert.That(objective.DrawerSolved, Is.True);
            Assert.That(reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
            yield return null;
            yield return null;
            yield return new WaitForFixedUpdate();

            Assert.That(objective.DrawerSolved, Is.False);
            Assert.That(objective.RunComplete, Is.False);
            Assert.That(objective.FailureTriggered, Is.False);
            Assert.That(objective.DrawerSolution, Is.EqualTo(ShowcaseDrawerSolution.None));
            Assert.That(Vector3.Distance(objective.DrawerBody.position, baseline),
                Is.LessThan(0.01f));
            Assert.That(objective.DrawerBody.isKinematic, Is.False);
        }

        [UnityTest]
        public IEnumerator PlayerHealthDepletion_EntersFailureAndResetRestoresPlayableState()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S7ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseObjectiveController objective =
                SceneComponents<ShowcaseObjectiveController>().Single();
            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            PrototypeInteractionStateOwner owner =
                SceneComponents<PrototypeInteractionStateOwner>().Single();
            AuthoredEncounterResetController reset =
                SceneComponents<AuthoredEncounterResetController>().Single();
            ShowcaseRunnerAgent runner = pressure.Runners.First();
            ShowcaseCombatHealth playerHealth =
                SceneComponents<ShowcaseCombatHealth>()
                    .Single(candidate => candidate.Team == ShowcaseCombatTeam.Player);

            for (int index = 0; index < 5; index++)
            {
                Assert.That(playerHealth.TryApplyDamage(
                    new ShowcaseDamageRequest(
                        ShowcaseCombatTeam.Enemy,
                        runner.RunnerId,
                        (uint)(index + 1),
                        20,
                        ShowcaseHitReaction.Body,
                        Vector3.back),
                    index), Is.True);
            }
            objective.EvaluateObjectiveState();
            Assert.That(objective.FailureTriggered, Is.True);
            Assert.That(owner.GlobalState,
                Is.EqualTo(PrototypeGlobalInteractionState.FailurePending));
            Assert.That(pressure.IsExternallySuspended, Is.True);

            Assert.That(reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
            yield return null;
            yield return null;
            Assert.That(objective.FailureTriggered, Is.False);
            Assert.That(owner.GlobalState, Is.EqualTo(PrototypeGlobalInteractionState.Playable));
            Assert.That(pressure.IsExternallySuspended, Is.False);
            Assert.That(playerHealth.CurrentHealth, Is.EqualTo(100));
        }

        private static void MoveBody(Rigidbody body, Vector3 position)
        {
            body.position = position;
            body.transform.position = position;
            Physics.SyncTransforms();
        }

        private static void MoveCharacter(Vector3 position)
        {
            ThirdPersonLocomotion locomotion =
                SceneComponents<ThirdPersonLocomotion>().Single();
            CharacterController controller = locomotion.GetComponent<CharacterController>();
            bool wasEnabled = controller != null && controller.enabled;
            if (wasEnabled)
            {
                controller.enabled = false;
            }
            locomotion.transform.position = position;
            if (wasEnabled)
            {
                controller.enabled = true;
            }
            Physics.SyncTransforms();
        }

        private static T[] SceneComponents<T>() where T : Component
        {
            return SceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }
    }
}
