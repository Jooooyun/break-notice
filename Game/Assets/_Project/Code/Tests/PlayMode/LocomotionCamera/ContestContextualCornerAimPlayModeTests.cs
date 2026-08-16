using System.Collections;
using System.Linq;
using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Input;
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
    public sealed class ContestContextualCornerAimPlayModeTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v007.unity";

        [UnityTest]
        public IEnumerator V007RightStaticWall_UsesLeanOnlyInAim_ThenReturnsWithoutJitter()
        {
            yield return EnsureShowcaseSceneUnloaded();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            GameObject wall = null;
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
                ThirdPersonCameraRig cameraRig =
                    GetSceneComponents<ThirdPersonCameraRig>(scene).Single();
                Transform player =
                    GetSceneComponents<ThirdPersonLocomotion>(scene).Single().transform;
                foreach (ShowcaseRunnerAgent runner in
                    GetSceneComponents<ShowcaseRunnerAgent>(scene))
                {
                    runner.enabled = false;
                }
                foreach (ShowcaseMobileAllyController ally in
                    GetSceneComponents<ShowcaseMobileAllyController>(scene))
                {
                    ally.enabled = false;
                }

                cameraRig.OutputCamera.aspect = 16f / 9f;
                cameraRig.HardResetCameraState();
                QueueGamepadState(gamepad, default);
                for (int frame = 0; frame < 12; frame++)
                {
                    yield return null;
                }

                int staticWorldLayer = LayerMask.NameToLayer("CameraStaticWorld");
                Assert.That(staticWorldLayer, Is.GreaterThanOrEqualTo(0));
                Quaternion heading = Quaternion.Euler(0f, cameraRig.Yaw, 0f);
                Vector3 right = heading * Vector3.right;
                Vector3 forward = heading * Vector3.forward;
                wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = "TEST_ContextualCorner_RightStaticWall";
                wall.layer = staticWorldLayer;
                wall.transform.SetPositionAndRotation(
                    player.position + right * 0.65f + Vector3.up - forward * 0.8f,
                    heading);
                wall.transform.localScale = new Vector3(0.15f, 3f, 4f);
                Physics.SyncTransforms();

                // Static geometry alone must not activate C in Normal.
                for (int frame = 0; frame < 16; frame++)
                {
                    yield return null;
                    Assert.That(
                        cameraRig.ContextualCornerMode,
                        Is.EqualTo(ShowcaseContextualCornerMode.RightDefault));
                }

                QueueGamepadState(gamepad, new GamepadState { leftTrigger = 1f });
                float enterDeadline = Time.realtimeSinceStartup + 1f;
                while (cameraRig.ContextualCornerMode ==
                           ShowcaseContextualCornerMode.RightDefault &&
                       Time.realtimeSinceStartup < enterDeadline)
                {
                    yield return null;
                }

                string enterContext = Describe(cameraRig);
                Assert.That(
                    cameraRig.CoreMode,
                    Is.EqualTo(CameraCoreMode.Aim),
                    enterContext);
                Assert.That(
                    cameraRig.ContextualCornerMode,
                    Is.EqualTo(ShowcaseContextualCornerMode.RightLean),
                    enterContext);
                Assert.That(
                    cameraRig.ContextualCornerShoulderFactor,
                    Is.GreaterThanOrEqualTo(0f),
                    "RightLean must never cross to the left shoulder.");

                float aimSettleDeadline = Time.realtimeSinceStartup + 0.5f;
                while (cameraRig.IsTransitioning &&
                       Time.realtimeSinceStartup < aimSettleDeadline)
                {
                    yield return null;
                }
                Assert.That(
                    cameraRig.CurrentFieldOfView,
                    Is.EqualTo(48f).Within(0.01f),
                    "Checkpoint C must not change the Checkpoint A Aim lens.");

                Object.Destroy(wall);
                wall = null;
                Physics.SyncTransforms();
                float returnStarted = Time.realtimeSinceStartup;
                float returnDeadline = returnStarted + 0.40f;
                while ((cameraRig.ContextualCornerMode !=
                            ShowcaseContextualCornerMode.RightDefault ||
                        cameraRig.ContextualCornerShoulderFactor < 0.999f) &&
                       Time.realtimeSinceStartup < returnDeadline)
                {
                    yield return null;
                }

                string returnContext = Describe(cameraRig);
                Assert.That(
                    cameraRig.ContextualCornerMode,
                    Is.EqualTo(ShowcaseContextualCornerMode.RightDefault),
                    returnContext);
                Assert.That(
                    cameraRig.ContextualCornerShoulderFactor,
                    Is.EqualTo(1f).Within(0.001f),
                    returnContext);
                Assert.That(
                    Time.realtimeSinceStartup - returnStarted,
                    Is.LessThanOrEqualTo(0.40f),
                    returnContext);

                yield return AssertSettledCameraStability(cameraRig, 120);
            }
            finally
            {
                if (wall != null)
                {
                    Object.Destroy(wall);
                }
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
        public IEnumerator V007AllyAndEnemyProximity_DoNotActivateContextualCornerIntent()
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
                ThirdPersonCameraRig cameraRig =
                    GetSceneComponents<ThirdPersonCameraRig>(scene).Single();
                ShowcaseMobileAllyController ally =
                    GetSceneComponents<ShowcaseMobileAllyController>(scene).Single();
                ShowcaseRunnerAgent runner =
                    GetSceneComponents<ShowcaseRunnerAgent>(scene).First();
                ally.enabled = false;
                foreach (ShowcaseRunnerAgent value in
                    GetSceneComponents<ShowcaseRunnerAgent>(scene))
                {
                    value.enabled = false;
                }

                cameraRig.OutputCamera.aspect = 16f / 9f;
                cameraRig.HardResetCameraState();
                QueueGamepadState(gamepad, new GamepadState { leftTrigger = 1f });
                for (int frame = 0; frame < 20; frame++)
                {
                    yield return null;
                }

                MoveActorToCamera(ally.transform, cameraRig.OutputCamera.transform.position);
                MoveActorToCamera(runner.transform, cameraRig.OutputCamera.transform.position);
                for (int frame = 0; frame < 90; frame++)
                {
                    yield return null;
                    Assert.That(
                        cameraRig.ContextualCornerMode,
                        Is.EqualTo(ShowcaseContextualCornerMode.RightDefault),
                        Describe(cameraRig));
                }
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

        private static void MoveActorToCamera(Transform actor, Vector3 cameraPosition)
        {
            CharacterController controller = actor.GetComponent<CharacterController>();
            CapsuleCollider capsule = actor.GetComponent<CapsuleCollider>();
            bool controllerEnabled = controller != null && controller.enabled;
            bool capsuleEnabled = capsule != null && capsule.enabled;
            if (controller != null)
            {
                controller.enabled = false;
            }
            if (capsule != null)
            {
                capsule.enabled = false;
            }
            actor.position = cameraPosition;
            if (controller != null)
            {
                controller.enabled = controllerEnabled;
            }
            if (capsule != null)
            {
                capsule.enabled = capsuleEnabled;
            }
            Physics.SyncTransforms();
        }

        private static IEnumerator AssertSettledCameraStability(
            ThirdPersonCameraRig cameraRig,
            int frames)
        {
            for (int frame = 0; frame < 24; frame++)
            {
                yield return null;
            }
            Vector3 minimum = cameraRig.OutputCamera.transform.position;
            Vector3 maximum = minimum;
            Quaternion rotation = cameraRig.OutputCamera.transform.rotation;
            float maximumAngle = 0f;
            for (int frame = 0; frame < frames; frame++)
            {
                yield return null;
                Vector3 position = cameraRig.OutputCamera.transform.position;
                minimum = Vector3.Min(minimum, position);
                maximum = Vector3.Max(maximum, position);
                maximumAngle = Mathf.Max(
                    maximumAngle,
                    Quaternion.Angle(rotation, cameraRig.OutputCamera.transform.rotation));
            }
            Assert.That((maximum - minimum).magnitude, Is.LessThanOrEqualTo(0.01f));
            Assert.That(maximumAngle, Is.LessThanOrEqualTo(0.10f));
        }

        private static string Describe(ThirdPersonCameraRig rig)
        {
            return
                $"core={rig.CoreMode}, corner={rig.ContextualCornerMode}, " +
                $"factor={rig.ContextualCornerShoulderFactor:0.000}, " +
                $"blend={rig.ContextualCornerBlendProgress:0.000}, " +
                $"constraint={rig.ConstraintStatus}/{rig.NoValidPoseReason}, " +
                $"blocker={rig.CurrentBlockingCollider?.name ?? "<none>"}, " +
                $"actor={rig.CurrentActorProxy?.name ?? "<none>"}.";
        }

        private static void QueueGamepadState(Gamepad gamepad, GamepadState state)
        {
            InputSystem.QueueStateEvent(gamepad, state);
            InputSystem.Update();
        }

        private static T[] GetSceneComponents<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }

        private static void DisablePointerCapture(Scene scene)
        {
            foreach (MonoBehaviour capture in GetSceneComponents<MonoBehaviour>(scene)
                .Where(component =>
                    component.GetType().FullName ==
                    "BreakNotice.Contest.BrowserPointerCapture"))
            {
                capture.enabled = false;
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
