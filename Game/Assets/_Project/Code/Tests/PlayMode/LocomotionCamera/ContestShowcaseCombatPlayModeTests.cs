using System.Collections;
using System.Linq;
using System.Reflection;
using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Encounter;
using BreakNotice.Gameplay.Input;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestShowcaseCombatPlayModeTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v007.unity";

        private Scene loadedScene;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(loadedScene);
                if (unload != null)
                {
                    yield return unload;
                }
            }

            loadedScene = default;
        }

        [UnityTest]
        public IEnumerator Scene_ComposesExactPlayerNormalAndEliteHealthContracts()
        {
            yield return LoadScene();

            ShowcaseCombatHealth[] health = SceneComponents<ShowcaseCombatHealth>();
            Assert.That(health, Has.Length.EqualTo(3));
            Assert.That(health.Single(item => item.Team == ShowcaseCombatTeam.Player)
                .MaximumHealth, Is.EqualTo(100));

            ShowcaseRunnerAgent[] runners = SceneComponents<ShowcaseRunnerAgent>();
            Assert.That(runners, Has.Length.EqualTo(2));
            Assert.That(runners.Single(item => !item.IsElite).Health.MaximumHealth,
                Is.EqualTo(100));
            Assert.That(runners.Single(item => item.IsElite).Health.MaximumHealth,
                Is.EqualTo(180));
            Assert.That(runners.All(item => !item.IsDead), Is.True);
        }

        [UnityTest]
        public IEnumerator PlayerCombo_DealsFifteenTwentyThirtyFiveAtAnimationHitEvents()
        {
            yield return LoadScene();

            ShowcasePlayerCombatController combat =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            ShowcaseRunnerAgent target = pressure.Runners.Single(item => !item.IsElite);
            ShowcaseAnimationEventReceiver receiver =
                combat.GetComponent<BreakNotice.Presentation.ShowcaseCharacterAnimationDriver>()
                    .Animator.GetComponent<ShowcaseAnimationEventReceiver>();

            foreach (ShowcaseRunnerAgent runner in pressure.Runners)
            {
                runner.enabled = false;
            }
            PlaceRunnerInFront(combat.transform, target);
            PrototypeInteractionController interaction =
                combat.GetComponent<PrototypeInteractionController>();
            InputSystemLogicalInputAdapter input =
                combat.GetComponent<InputSystemLogicalInputAdapter>();

            SubmitMeleeInput(interaction, input);
            Assert.That(combat.ActiveMeleeStep, Is.EqualTo(1));
            yield return new WaitForSeconds(0.4f);
            SubmitMeleeInput(interaction, input);
            Assert.That(combat.HasBufferedMelee, Is.True);
            receiver.ShowcaseCombatHit();
            Assert.That(target.Health.CurrentHealth, Is.EqualTo(85));
            yield return new WaitForSeconds(0.7f);
            yield return null;

            Assert.That(combat.ActiveMeleeStep, Is.EqualTo(2));
            yield return new WaitForSeconds(0.4f);
            SubmitMeleeInput(interaction, input);
            Assert.That(combat.HasBufferedMelee, Is.True);
            receiver.ShowcaseCombatHit();
            Assert.That(target.Health.CurrentHealth, Is.EqualTo(65));
            yield return new WaitForSeconds(0.75f);
            yield return null;

            Assert.That(combat.ActiveMeleeStep, Is.EqualTo(3));
            receiver.ShowcaseCombatHit();
            Assert.That(target.Health.CurrentHealth, Is.EqualTo(30));
            Assert.That(target.State, Is.EqualTo(ShowcaseRunnerState.KnockedDown));
        }

        [UnityTest]
        public IEnumerator DirectThrow_PhysicalReleaseWaitsForAnimationEvent()
        {
            yield return LoadScene();

            ShowcasePlayerCombatController combat =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            PrototypeInteractionStateOwner owner =
                combat.GetComponent<PrototypeInteractionStateOwner>();
            PrototypePhysicalObjectLifecycle lifecycle =
                combat.GetComponent<PrototypePhysicalObjectLifecycle>();
            PrototypeInteractionTarget target =
                SceneComponents<PrototypeInteractionTarget>()
                    .First(item => item.IsScoopEligible &&
                                   item.WeightClass == PrototypeWeightClass.Light);
            ShowcaseAnimationEventReceiver receiver =
                combat.GetComponent<BreakNotice.Presentation.ShowcaseCharacterAnimationDriver>()
                    .Animator.GetComponent<ShowcaseAnimationEventReceiver>();

            Assert.That(owner.TryCommit(PrototypeInteractionDecision.Accept(
                PrototypeInteractionOutcome.Scoop,
                target)), Is.True);
            Assert.That(lifecycle.BeginCarry(target), Is.True);
            uint before = lifecycle.ThrowOriginCount;

            Assert.That(combat.TryBeginDirectThrow(target, Vector3.forward, 0.5f), Is.True);
            Assert.That(lifecycle.ThrowOriginCount, Is.EqualTo(before));
            Assert.That(lifecycle.CarriedTarget, Is.EqualTo(target));
            Assert.That(combat.GetComponent<ThirdPersonLocomotion>()
                .IsMovementExternallyBlocked, Is.True);

            receiver.ShowcaseThrowRelease();

            Assert.That(lifecycle.ThrowOriginCount, Is.EqualTo(before + 1));
            Assert.That(lifecycle.CarriedTarget, Is.Null);
            Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
        }

        [UnityTest]
        public IEnumerator GroundPickup_PhysicalCarryWaitsForAnimationContactEvent()
        {
            yield return LoadScene();

            ShowcasePlayerCombatController combat =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            PrototypeInteractionStateOwner owner =
                combat.GetComponent<PrototypeInteractionStateOwner>();
            PrototypePhysicalObjectLifecycle lifecycle =
                combat.GetComponent<PrototypePhysicalObjectLifecycle>();
            PrototypeInteractionTarget target =
                SceneComponents<PrototypeInteractionTarget>()
                    .First(item => item.IsScoopEligible &&
                                   item.WeightClass == PrototypeWeightClass.Light);
            ShowcaseAnimationEventReceiver receiver =
                combat.GetComponent<BreakNotice.Presentation.ShowcaseCharacterAnimationDriver>()
                    .Animator.GetComponent<ShowcaseAnimationEventReceiver>();

            Assert.That(owner.TryCommit(PrototypeInteractionDecision.Accept(
                PrototypeInteractionOutcome.Scoop,
                target)), Is.True);
            Assert.That(combat.TryBeginAcquisition(
                target,
                PrototypeInteractionOutcome.Scoop), Is.True);
            Assert.That(lifecycle.CarriedTarget, Is.Null);

            receiver.ShowcaseCarryContact();

            Assert.That(lifecycle.CarriedTarget, Is.SameAs(target));
            Assert.That(owner.CarriedTarget, Is.SameAs(target));
        }

        [UnityTest]
        public IEnumerator Reset_RestoresAllThreeHealthOwnersAndCombatState()
        {
            yield return LoadScene();

            ShowcasePlayerCombatController player =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            ShowcaseRunnerAgent normal =
                SceneComponents<ShowcaseRunnerAgent>().Single(item => !item.IsElite);
            AuthoredEncounterResetController reset =
                SceneComponents<AuthoredEncounterResetController>().Single();

            Assert.That(player.Health.TryApplyDamage(
                new ShowcaseDamageRequest(
                    ShowcaseCombatTeam.Enemy,
                    normal.RunnerId,
                    1,
                    30,
                    ShowcaseHitReaction.Knockdown,
                    Vector3.back),
                0f), Is.True);
            Assert.That(normal.Health.TryApplyDamage(
                new ShowcaseDamageRequest(
                    ShowcaseCombatTeam.Player,
                    "CHR00",
                    1,
                    35,
                    ShowcaseHitReaction.BigHead,
                    Vector3.forward),
                0f), Is.True);

            Assert.That(reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
            yield return null;
            yield return null;

            Assert.That(player.Health.CurrentHealth, Is.EqualTo(100));
            Assert.That(normal.Health.CurrentHealth, Is.EqualTo(100));
            Assert.That(player.IsDead, Is.False);
            Assert.That(player.IsActionLocked, Is.False);
        }

        private IEnumerator LoadScene()
        {
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
            Assert.That(load, Is.Not.Null);
            yield return load;
            loadedScene = SceneManager.GetSceneByPath(ScenePath);
            Assert.That(loadedScene.IsValid() && loadedScene.isLoaded, Is.True);
            yield return null;
        }

        private static void PlaceRunnerInFront(
            Transform player,
            ShowcaseRunnerAgent runner)
        {
            Vector3 position = player.position + player.forward * 1.25f;
            Rigidbody body = runner.GetComponent<Rigidbody>();
            body.position = position;
            runner.transform.position = position;
            Physics.SyncTransforms();
        }

        private static void SubmitMeleeInput(
            PrototypeInteractionController interaction,
            InputSystemLogicalInputAdapter input)
        {
            PropertyInfo snapshot = typeof(InputSystemLogicalInputAdapter).GetProperty(
                nameof(InputSystemLogicalInputAdapter.Snapshot),
                BindingFlags.Instance | BindingFlags.Public);
            MethodInfo setter = snapshot?.GetSetMethod(true);
            Assert.That(setter, Is.Not.Null);
            setter.Invoke(input, new object[]
            {
                new LogicalInputSnapshot(
                    Vector2.zero,
                    Vector2.zero,
                    false,
                    false,
                    true,
                    false,
                    false,
                    false,
                    throwHeld: true)
            });
            interaction.SendMessage("LateUpdate", SendMessageOptions.DontRequireReceiver);
            setter.Invoke(input, new object[] { default(LogicalInputSnapshot) });
        }

        private T[] SceneComponents<T>() where T : Component
        {
            return loadedScene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }
    }
}
