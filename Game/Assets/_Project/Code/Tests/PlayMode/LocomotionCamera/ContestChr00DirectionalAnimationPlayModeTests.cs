using System.Collections;
using System.Linq;
using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestChr00DirectionalAnimationPlayModeTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v007.unity";

        [UnityTest]
        public IEnumerator V007ActualGamepadAim_SelectsDirectionalBaseMotionAndMaskedAimPose()
        {
            yield return EnsureShowcaseSceneUnloaded();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
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
                DisablePointerCapture(scene);
                foreach (ShowcaseRunnerAgent runner in
                    SceneComponents<ShowcaseRunnerAgent>(scene))
                {
                    runner.enabled = false;
                }
                foreach (ShowcaseMobileAllyController ally in
                    SceneComponents<ShowcaseMobileAllyController>(scene))
                {
                    ally.enabled = false;
                }

                ShowcaseCharacterAnimationDriver driver =
                    SceneComponents<ShowcaseCharacterAnimationDriver>(scene)
                        .Single(candidate =>
                            candidate.Role == ShowcaseCharacterAnimationRole.Player);
                Animator animator = driver.Animator;
                Assert.That(animator, Is.Not.Null);
                Assert.That(animator.layerCount, Is.EqualTo(2));
                Assert.That(animator.GetLayerName(1), Is.EqualTo("Aim Upper Body"));
                Assert.That(driver.HasAimUpperBodyLayer, Is.True);

                Transform visual = driver.transform.Find("RUNTIME_DESIGNER_A_VISUAL");
                Assert.That(visual, Is.Not.Null);
                Vector3 visualPosition = visual.localPosition;
                Quaternion visualRotation = visual.localRotation;
                Vector3 visualScale = visual.localScale;

                QueueGamepadState(gamepad, default);
                for (int frame = 0; frame < 4; frame++)
                {
                    yield return null;
                }

                QueueGamepadState(gamepad, new GamepadState { leftTrigger = 1f });
                yield return WaitForState(
                    driver,
                    "AimIdle",
                    ShowcasePlayerLocomotionDirection.None,
                    1f);

                // Let the visual body finish its bounded yaw alignment before
                // interpreting the four camera-relative movement axes.
                for (int frame = 0; frame < 20; frame++)
                {
                    yield return null;
                }

                foreach ((Vector2 move, string state,
                             ShowcasePlayerLocomotionDirection direction) in new[]
                         {
                             (Vector2.up, "AimForward",
                                 ShowcasePlayerLocomotionDirection.Forward),
                             (Vector2.down, "AimBackward",
                                 ShowcasePlayerLocomotionDirection.Backward),
                             (Vector2.left, "AimStrafeLeft",
                                 ShowcasePlayerLocomotionDirection.Left),
                             (Vector2.right, "AimStrafeRight",
                                 ShowcasePlayerLocomotionDirection.Right)
                         })
                {
                    QueueGamepadState(gamepad, new GamepadState
                    {
                        leftTrigger = 1f,
                        leftStick = move
                    });
                    yield return WaitForState(driver, state, direction, 1f);
                    if (direction == ShowcasePlayerLocomotionDirection.Backward)
                    {
                        yield return AssertBackwardFallbackRunsInReverse(animator);
                    }
                    AssertVisualRootUnchanged(
                        visual,
                        visualPosition,
                        visualRotation,
                        visualScale,
                        state);

                    QueueGamepadState(gamepad,
                        new GamepadState { leftTrigger = 1f });
                    yield return WaitForState(
                        driver,
                        "AimIdle",
                        ShowcasePlayerLocomotionDirection.None,
                        1f);
                }

                QueueGamepadState(gamepad, default);
                yield return WaitForState(
                    driver,
                    "Idle",
                    ShowcasePlayerLocomotionDirection.None,
                    0f);

                QueueGamepadState(gamepad,
                    new GamepadState { leftStick = Vector2.up });
                yield return WaitForState(
                    driver,
                    "Walk",
                    ShowcasePlayerLocomotionDirection.None,
                    0f);
                AssertVisualRootUnchanged(
                    visual,
                    visualPosition,
                    visualRotation,
                    visualScale,
                    "normal Walk after Aim release");

                QueueGamepadState(gamepad, default);
            }
            finally
            {
                if (gamepad != null && gamepad.added)
                {
                    InputSystem.RemoveDevice(gamepad);
                }
                if (scene.IsValid() && scene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        [UnityTest]
        public IEnumerator V007MovingActionCompletion_DoesNotSynthesizeAStopWithoutInput()
        {
            yield return EnsureShowcaseSceneUnloaded();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
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
                DisablePointerCapture(scene);
                DisableAutonomousActors(scene);

                ShowcaseCharacterAnimationDriver driver =
                    SceneComponents<ShowcaseCharacterAnimationDriver>(scene)
                        .Single(candidate =>
                            candidate.Role == ShowcaseCharacterAnimationRole.Player);
                ShowcasePlayerCombatController combat =
                    SceneComponents<ShowcasePlayerCombatController>(scene).Single();
                ShowcaseAnimationEventReceiver receiver =
                    driver.Animator.GetComponent<ShowcaseAnimationEventReceiver>();
                Assert.That(receiver, Is.Not.Null);

                QueueGamepadState(gamepad,
                    new GamepadState { leftStick = Vector2.up });
                yield return WaitForState(
                    driver,
                    "Walk",
                    ShowcasePlayerLocomotionDirection.None,
                    0f);

                int completedBefore = receiver.ActionCompleteInvocationCount;
                Assert.That(combat.TryRequestMelee(null), Is.True,
                    "The real player action owner must accept the melee cue while moving.");
                Assert.That(combat.IsActionLocked, Is.True);
                QueueGamepadState(gamepad, default);

                yield return WaitForState(
                    driver,
                    "MeleeJabAction",
                    ShowcasePlayerLocomotionDirection.None,
                    0f);

                float completionDeadline = Time.realtimeSinceStartup + 3f;
                while (combat.IsActionLocked &&
                       Time.realtimeSinceStartup < completionDeadline)
                {
                    yield return null;
                }
                Assert.That(combat.IsActionLocked, Is.False,
                    "The natural action-complete event did not release player ownership.");
                Assert.That(receiver.ActionCompleteInvocationCount,
                    Is.GreaterThan(completedBefore),
                    "The action must complete through the authored AnimationEvent path.");

                bool reachedIdle = false;
                for (int frame = 0; frame < 45; frame++)
                {
                    yield return null;
                    Assert.That(driver.CurrentBaseStateName,
                        Is.Not.EqualTo("WalkStopAction"),
                        "Unlock synthesized a WalkStop from movement hidden by the action.");
                    Assert.That(driver.CurrentBaseStateName,
                        Is.Not.EqualTo("RunStopAction"),
                        "Unlock synthesized a RunStop from movement hidden by the action.");
                    reachedIdle |= driver.CurrentBaseStateName == "Idle";
                }
                Assert.That(reachedIdle, Is.True,
                    $"Player never returned to Idle after action completion; " +
                    $"state={driver.CurrentBaseStateName}.");
            }
            finally
            {
                if (gamepad != null && gamepad.added)
                {
                    InputSystem.RemoveDevice(gamepad);
                }
                if (scene.IsValid() && scene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        [UnityTest]
        public IEnumerator V007AimMovementSimultaneousRelease_ClearsUpperBodyForEntireStop()
        {
            yield return EnsureShowcaseSceneUnloaded();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
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
                DisablePointerCapture(scene);
                DisableAutonomousActors(scene);

                ShowcaseCharacterAnimationDriver driver =
                    SceneComponents<ShowcaseCharacterAnimationDriver>(scene)
                        .Single(candidate =>
                            candidate.Role == ShowcaseCharacterAnimationRole.Player);
                Assert.That(driver.Animator.layerCount, Is.EqualTo(2));

                QueueGamepadState(gamepad, new GamepadState
                {
                    leftTrigger = 1f,
                    leftStick = Vector2.up
                });
                yield return WaitForState(
                    driver,
                    "AimForward",
                    ShowcasePlayerLocomotionDirection.Forward,
                    1f);

                // Aim and movement are released in the same physical input sample.
                QueueGamepadState(gamepad, default);
                float stopDeadline = Time.realtimeSinceStartup + 1.5f;
                while (driver.CurrentBaseStateName != "WalkStopAction" &&
                       Time.realtimeSinceStartup < stopDeadline)
                {
                    yield return null;
                    Assert.That(driver.CurrentBaseStateName,
                        Is.Not.EqualTo("RunStopAction"),
                        "Non-sprint Aim movement must enter WalkStop, not RunStop.");
                }

                Assert.That(driver.CurrentBaseStateName, Is.EqualTo("WalkStopAction"));
                Assert.That(driver.AimUpperBodyLayerWeight,
                    Is.EqualTo(0f).Within(0.001f),
                    "Aim Upper Body must be cleared on the exact Stop entry frame.");
                Assert.That(driver.Animator.GetLayerWeight(1),
                    Is.EqualTo(0f).Within(0.001f),
                    "Animator layer weight must be cleared on the exact Stop entry frame.");

                int sampledStopFrames = 0;
                float finishDeadline = Time.realtimeSinceStartup + 3f;
                while (driver.CurrentBaseStateName == "WalkStopAction" &&
                       Time.realtimeSinceStartup < finishDeadline)
                {
                    Assert.That(driver.AimUpperBodyLayerWeight,
                        Is.EqualTo(0f).Within(0.001f),
                        $"Aim overlay returned during Stop frame {sampledStopFrames}.");
                    Assert.That(driver.Animator.GetLayerWeight(1),
                        Is.EqualTo(0f).Within(0.001f),
                        $"Animator Aim layer returned during Stop frame {sampledStopFrames}.");
                    sampledStopFrames++;
                    yield return null;
                }

                Assert.That(sampledStopFrames, Is.GreaterThan(1),
                    "The test did not observe the Stop action over multiple frames.");
                Assert.That(driver.CurrentBaseStateName, Is.EqualTo("Idle"),
                    "Stop must complete into the non-Aim Idle state.");
                Assert.That(driver.AimUpperBodyLayerWeight,
                    Is.EqualTo(0f).Within(0.001f));
            }
            finally
            {
                if (gamepad != null && gamepad.added)
                {
                    InputSystem.RemoveDevice(gamepad);
                }
                if (scene.IsValid() && scene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        private static IEnumerator AssertBackwardFallbackRunsInReverse(
            Animator animator)
        {
            Assert.That(
                animator.GetFloat(Animator.StringToHash("LocomotionPlayback")),
                Is.LessThan(-0.1f),
                "AimBackward must make the code-owned playback parameter negative.");
            float before = Mathf.Repeat(
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime,
                1f);
            float elapsed = 0f;
            float deadline = Time.realtimeSinceStartup + 1f;
            while (elapsed < 0.08f && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }
            Assert.That(elapsed, Is.GreaterThanOrEqualTo(0.08f));

            float after = Mathf.Repeat(
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime,
                1f);
            float signedCycleDelta = Mathf.DeltaAngle(before * 360f, after * 360f) /
                360f;
            Assert.That(
                signedCycleDelta,
                Is.LessThan(-0.02f),
                $"AimBackward must run the approved Walk_F fallback in reverse. " +
                $"before={before:0.000}, after={after:0.000}, " +
                $"delta={signedCycleDelta:0.000}.");
        }

        private static IEnumerator WaitForState(
            ShowcaseCharacterAnimationDriver driver,
            string stateName,
            ShowcasePlayerLocomotionDirection direction,
            float expectedUpperBodyWeight)
        {
            Animator animator = driver.Animator;
            float deadline = Time.realtimeSinceStartup + 1.5f;
            string fullStateName = "Base Layer." + stateName;
            while (Time.realtimeSinceStartup < deadline)
            {
                AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(0);
                if (driver.CurrentBaseStateName == stateName &&
                    driver.PlayerLocomotionDirection == direction &&
                    !animator.IsInTransition(0) &&
                    current.IsName(fullStateName) &&
                    Mathf.Abs(driver.AimUpperBodyLayerWeight -
                        expectedUpperBodyWeight) <= 0.01f)
                {
                    break;
                }
                yield return null;
            }

            string context =
                $"requested={stateName}/{direction}, " +
                $"driver={driver.CurrentBaseStateName}/" +
                $"{driver.PlayerLocomotionDirection}, " +
                $"baseHash={animator.GetCurrentAnimatorStateInfo(0).fullPathHash}, " +
                $"transition={animator.IsInTransition(0)}, " +
                $"upperWeight={driver.AimUpperBodyLayerWeight:0.000}.";
            Assert.That(driver.CurrentBaseStateName, Is.EqualTo(stateName), context);
            Assert.That(driver.PlayerLocomotionDirection, Is.EqualTo(direction), context);
            Assert.That(animator.IsInTransition(0), Is.False, context);
            Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName(fullStateName),
                Is.True, context);
            Assert.That(driver.AimUpperBodyLayerWeight,
                Is.EqualTo(expectedUpperBodyWeight).Within(0.01f), context);
            Assert.That(animator.GetLayerWeight(1),
                Is.EqualTo(expectedUpperBodyWeight).Within(0.01f), context);
            if (expectedUpperBodyWeight > 0.5f)
            {
                Assert.That(animator.GetCurrentAnimatorStateInfo(1)
                    .IsName("Aim Upper Body.AimUpperBodyPose"), Is.True, context);
            }
        }

        private static void AssertVisualRootUnchanged(
            Transform visual,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            string context)
        {
            Assert.That(Vector3.Distance(visual.localPosition, position),
                Is.LessThan(0.0001f), context);
            Assert.That(Quaternion.Angle(visual.localRotation, rotation),
                Is.LessThan(0.01f), context);
            Assert.That(Vector3.Distance(visual.localScale, scale),
                Is.LessThan(0.0001f), context);
        }

        private static void QueueGamepadState(Gamepad gamepad, GamepadState state)
        {
            InputSystem.QueueStateEvent(gamepad, state);
            InputSystem.Update();
        }

        private static T[] SceneComponents<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }

        private static void DisablePointerCapture(Scene scene)
        {
            foreach (MonoBehaviour capture in SceneComponents<MonoBehaviour>(scene)
                .Where(component =>
                    component.GetType().FullName ==
                    "BreakNotice.Contest.BrowserPointerCapture"))
            {
                capture.enabled = false;
            }
        }

        private static void DisableAutonomousActors(Scene scene)
        {
            foreach (ShowcaseRunnerAgent runner in
                SceneComponents<ShowcaseRunnerAgent>(scene))
            {
                runner.enabled = false;
            }
            foreach (ShowcaseMobileAllyController ally in
                SceneComponents<ShowcaseMobileAllyController>(scene))
            {
                ally.enabled = false;
            }
        }

        private static IEnumerator EnsureShowcaseSceneUnloaded()
        {
            Scene existing = SceneManager.GetSceneByPath(ScenePath);
            if (!existing.IsValid() || !existing.isLoaded)
            {
                yield break;
            }

            AsyncOperation unload = SceneManager.UnloadSceneAsync(existing);
            while (unload != null && !unload.isDone)
            {
                yield return null;
            }
            yield return null;
        }
    }
}
