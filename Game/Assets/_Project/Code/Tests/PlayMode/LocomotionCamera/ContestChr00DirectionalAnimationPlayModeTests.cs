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
                ThirdPersonLocomotion locomotion =
                    driver.GetComponent<ThirdPersonLocomotion>();
                PrototypeInteractionController interaction =
                    driver.GetComponent<PrototypeInteractionController>();
                ThirdPersonCameraRig cameraRig =
                    SceneComponents<ThirdPersonCameraRig>(scene).Single();
                Camera gameplayCamera = cameraRig.OutputCamera;
                Animator animator = driver.Animator;
                Assert.That(animator, Is.Not.Null);
                Assert.That(locomotion, Is.Not.Null);
                Assert.That(interaction, Is.Not.Null);
                Assert.That(gameplayCamera, Is.Not.Null);
                Assert.That(animator.layerCount, Is.EqualTo(4));
                Assert.That(animator.GetLayerName(1), Is.EqualTo("Aim Upper Body"));
                Assert.That(animator.GetLayerName(2), Is.EqualTo("Strap Upper Body"));
                Assert.That(animator.GetLayerName(3), Is.EqualTo("Melee Moving Legs"));
                Assert.That(driver.HasAimUpperBodyLayer, Is.True);

                Transform visual = driver.transform.Find("RUNTIME_DESIGNER_A_VISUAL");
                Assert.That(visual, Is.Not.Null);
                Vector3 visualPosition = visual.localPosition;
                Quaternion visualRotation = visual.localRotation;
                Vector3 visualScale = visual.localScale;
                Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                Assert.That(hips, Is.Not.Null);

                QueueGamepadState(gamepad, default);
                for (int frame = 0; frame < 4; frame++)
                {
                    yield return null;
                }

                QueueGamepadState(gamepad, new GamepadState { leftTrigger = 1f });
                yield return WaitForState(
                    driver,
                    "Idle",
                    ShowcasePlayerLocomotionDirection.None,
                    1f);

                // Let the visual body finish its bounded yaw alignment before
                // interpreting the four camera-relative movement axes.
                for (int frame = 0; frame < 20; frame++)
                {
                    yield return null;
                }
                Vector3 hipsLocalBeforeAimTurn =
                    driver.transform.InverseTransformPoint(hips.position);

                Vector3 playerPositionBeforeAimTurn = driver.transform.position;
                for (int frame = 0; frame < 12; frame++)
                {
                    QueueGamepadState(gamepad, new GamepadState
                    {
                        leftTrigger = 1f,
                        rightStick = new Vector2(0.8f, 0f)
                    });
                    yield return null;
                }
                QueueGamepadState(gamepad, new GamepadState { leftTrigger = 1f });
                yield return null;

                Ray centerRay = gameplayCamera.ViewportPointToRay(
                    new Vector3(0.5f, 0.5f, 0f));
                GameObject aimMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                aimMarker.name = "TEST_RETICLE_AIM_POINT";
                SceneManager.MoveGameObjectToScene(aimMarker, scene);
                aimMarker.transform.position = centerRay.GetPoint(4f);
                aimMarker.transform.localScale = Vector3.one * 0.4f;
                Collider aimMarkerCollider = aimMarker.GetComponent<Collider>();
                Physics.SyncTransforms();

                float facingDeadline = Time.realtimeSinceStartup + 1f;
                Vector3 reticleDirection = Vector3.zero;
                Vector3 reticleAimPoint = Vector3.zero;
                while (Time.realtimeSinceStartup < facingDeadline)
                {
                    QueueGamepadState(gamepad, new GamepadState { leftTrigger = 1f });
                    yield return null;
                    reticleAimPoint = interaction.ResolveReticleAimPoint();
                    reticleDirection = Vector3.ProjectOnPlane(
                        reticleAimPoint - driver.transform.position,
                        Vector3.up).normalized;
                    if (reticleDirection.sqrMagnitude > 0.001f &&
                        Vector3.Angle(driver.transform.forward, reticleDirection) <= 1.5f)
                    {
                        break;
                    }
                }
                Assert.That(reticleDirection.sqrMagnitude, Is.GreaterThan(0.001f));
                Assert.That(Vector3.Distance(
                        aimMarkerCollider.ClosestPoint(reticleAimPoint),
                        reticleAimPoint),
                    Is.LessThanOrEqualTo(0.02f),
                    "The gameplay aim authority did not use the optical center ray.");
                Assert.That(Vector3.Angle(driver.transform.forward, reticleDirection),
                    Is.LessThanOrEqualTo(1.5f),
                    "Aim root did not face the actual center-reticle world point.");
                Assert.That(Vector3.Angle(
                        locomotion.AimFacingWorldDirection,
                        reticleDirection),
                    Is.LessThanOrEqualTo(0.25f));
                Assert.That(Vector3.Angle(visual.forward, reticleDirection),
                    Is.LessThanOrEqualTo(1.5f),
                    "The visible CHR00 body diverged from gameplay aim facing.");
                Assert.That(Vector3.Angle(animator.transform.forward, reticleDirection),
                    Is.LessThanOrEqualTo(1.5f),
                    "The animated CHR00 model diverged from gameplay aim facing.");
                Assert.That(Vector3.ProjectOnPlane(
                        driver.transform.position - playerPositionBeforeAimTurn,
                        Vector3.up).magnitude,
                    Is.LessThanOrEqualTo(0.02f),
                    "Aim facing moved the player body instead of rotating it in place.");
                Vector3 hipsLocalDuringAim =
                    driver.transform.InverseTransformPoint(hips.position);
                Assert.That(Vector2.Distance(
                        new Vector2(hipsLocalBeforeAimTurn.x, hipsLocalBeforeAimTurn.z),
                        new Vector2(hipsLocalDuringAim.x, hipsLocalDuringAim.z)),
                    Is.LessThanOrEqualTo(0.03f),
                    "Stationary Aim displaced the visible pelvis away from the player root.");
                Assert.That(Mathf.Abs(hipsLocalDuringAim.y - hipsLocalBeforeAimTurn.y),
                    Is.LessThanOrEqualTo(0.03f),
                    "Stationary Aim changed the visible body support height.");

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
                        "Idle",
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
        public IEnumerator V007MovingMelee_UsesFullBodyStrikeAndReticleFacingWithMovingLegs()
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
                PrototypeInteractionController interaction =
                    combat.GetComponent<PrototypeInteractionController>();
                ShowcaseAnimationEventReceiver receiver =
                    driver.Animator.GetComponent<ShowcaseAnimationEventReceiver>();
                Assert.That(interaction, Is.Not.Null);
                Assert.That(receiver, Is.Not.Null);

                QueueGamepadState(gamepad,
                    new GamepadState { leftStick = Vector2.up });
                yield return WaitForState(
                    driver,
                    "Walk",
                    ShowcasePlayerLocomotionDirection.None,
                    0f);

                int completedBefore = receiver.ActionCompleteInvocationCount;
                Vector3 reticleDirection = Vector3.ProjectOnPlane(
                    interaction.ResolveReticleAimPoint() - combat.transform.position,
                    Vector3.up).normalized;
                combat.transform.rotation = Quaternion.LookRotation(
                    Vector3.Cross(Vector3.up, reticleDirection),
                    Vector3.up);
                Assert.That(combat.TryRequestMelee(null), Is.True,
                    "The real player action owner must accept the melee cue while moving.");
                Assert.That(combat.IsActionLocked, Is.True);
                Assert.That(Vector3.Angle(combat.transform.forward, reticleDirection),
                    Is.LessThanOrEqualTo(1.5f),
                    "A targetless punch must snap to the optical reticle, not the old move yaw.");
                ThirdPersonLocomotion locomotion = combat.GetComponent<ThirdPersonLocomotion>();
                Assert.That(locomotion.HasMobilityLock(
                    PlayerMobilityLockReason.PlayerAction), Is.False);
                Assert.That(locomotion.HasJumpLock(
                    PlayerMobilityLockReason.MeleeAction), Is.True);

                Assert.That(driver.HasMeleeMovingLegsLayer, Is.True);
                int meleeLayer = driver.Animator.GetLayerIndex("Melee Moving Legs");
                int jabHash = Animator.StringToHash(
                    "Base Layer.MeleeJabAction");
                int jabReturnHash = Animator.StringToHash(
                    "Base Layer.MeleeJabReturnAction");
                int[] movingLegStateHashes =
                {
                    Animator.StringToHash("Melee Moving Legs.MeleeLegsForward"),
                    Animator.StringToHash("Melee Moving Legs.MeleeLegsBackward"),
                    Animator.StringToHash("Melee Moving Legs.MeleeLegsLeft"),
                    Animator.StringToHash("Melee Moving Legs.MeleeLegsRight"),
                    Animator.StringToHash("Melee Moving Legs.MeleeLegsRun")
                };
                float jabDeadline = Time.realtimeSinceStartup + 2f;
                while ((driver.Animator.GetCurrentAnimatorStateInfo(0)
                            .fullPathHash != jabHash ||
                        driver.MeleeMovingLegsLayerWeight < 0.99f) &&
                       Time.realtimeSinceStartup < jabDeadline)
                {
                    yield return null;
                }
                Assert.That(
                    driver.Animator.GetCurrentAnimatorStateInfo(0).fullPathHash,
                    Is.EqualTo(jabHash));
                Assert.That(driver.MeleeMovingLegsLayerWeight,
                    Is.EqualTo(1f).Within(0.001f));
                int currentLegState = driver.Animator
                    .GetCurrentAnimatorStateInfo(meleeLayer).fullPathHash;
                int nextLegState = driver.Animator.IsInTransition(meleeLayer)
                    ? driver.Animator.GetNextAnimatorStateInfo(meleeLayer).fullPathHash
                    : 0;
                Assert.That(movingLegStateHashes.Contains(currentLegState) ||
                            movingLegStateHashes.Contains(nextLegState),
                    Is.True,
                    "Moving melee must use a directional locomotion state on the legs layer.");
                Assert.That(driver.CurrentBaseStateName,
                    Is.EqualTo("MeleeJabAction"),
                    "The original full-body punch must remain the Base action owner.");

                float completionDeadline = Time.realtimeSinceStartup + 3f;
                bool sawJabReturn = false;
                while (combat.IsActionLocked &&
                       Time.realtimeSinceStartup < completionDeadline)
                {
                    yield return null;
                    sawJabReturn |= driver.Animator
                        .GetCurrentAnimatorStateInfo(0).fullPathHash ==
                        jabReturnHash;
                }
                Assert.That(sawJabReturn, Is.True,
                    "An unbuffered Jab must play the authored 14-to-1 return state.");
                Assert.That(combat.IsActionLocked, Is.False,
                    "The natural action-complete event did not release player ownership.");
                Assert.That(receiver.ActionCompleteInvocationCount,
                    Is.GreaterThan(completedBefore),
                    "The action must complete through the authored AnimationEvent path.");
                float legsReleaseDeadline = Time.realtimeSinceStartup + 0.5f;
                while (driver.MeleeMovingLegsLayerWeight > 0.001f &&
                       Time.realtimeSinceStartup < legsReleaseDeadline)
                {
                    yield return null;
                }
                Assert.That(driver.MeleeMovingLegsLayerWeight,
                    Is.LessThanOrEqualTo(0.001f));

                QueueGamepadState(gamepad, default);
                bool returnedToLocomotionOwnership = false;
                for (int frame = 0; frame < 20; frame++)
                {
                    yield return null;
                    returnedToLocomotionOwnership |=
                        driver.CurrentBaseStateName == "WalkStopAction" ||
                        driver.CurrentBaseStateName == "Idle";
                }
                Assert.That(returnedToLocomotionOwnership, Is.True,
                    $"Player never returned to locomotion ownership after melee; " +
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
                Assert.That(driver.Animator.layerCount, Is.EqualTo(4));

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
