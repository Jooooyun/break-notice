using System.Collections;
using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Input;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEditor;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class LocomotionCameraGrayboxTests
    {
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Graybox/PlayerGraybox.prefab";
        private const string CameraPrefabPath = "Assets/_Project/Prefabs/Graybox/GameplayCameraRig.prefab";

        private GameObject player;
        private GameObject cameraRigObject;
        private GameObject floor;
        private Gamepad gamepad;
        private InputSystemLogicalInputAdapter logicalInput;
        private ThirdPersonLocomotion locomotion;
        private ThirdPersonCameraRig cameraRig;

        [UnitySetUp]
        public IEnumerator SetUpFixture()
        {
            gamepad = InputSystem.AddDevice<Gamepad>();

            floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "TestFloor";
            floor.transform.position = new Vector3(0f, -0.25f, 0f);
            floor.transform.localScale = new Vector3(20f, 0.5f, 20f);

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            GameObject cameraPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CameraPrefabPath);
            Assert.That(playerPrefab, Is.Not.Null);
            Assert.That(cameraPrefab, Is.Not.Null);

            player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            cameraRigObject = (GameObject)PrefabUtility.InstantiatePrefab(cameraPrefab);
            player.transform.position = new Vector3(0f, 1f, 0f);

            logicalInput = player.GetComponent<InputSystemLogicalInputAdapter>();
            locomotion = player.GetComponent<ThirdPersonLocomotion>();
            cameraRig = cameraRigObject.GetComponent<ThirdPersonCameraRig>();
            cameraRig.Configure(player.transform, logicalInput);
            locomotion.ConfigureCamera(cameraRig.OutputCamera.transform);

            QueueGamepadState(default);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDownFixture()
        {
            if (player != null)
            {
                Object.Destroy(player);
            }

            if (cameraRigObject != null)
            {
                Object.Destroy(cameraRigObject);
            }

            if (floor != null)
            {
                Object.Destroy(floor);
            }

            yield return null;

            if (gamepad != null && gamepad.added)
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }

        [UnityTest]
        public IEnumerator MoveInput_ChangesPlayerPlanarPosition()
        {
            Vector3 initialPosition = player.transform.position;
            QueueGamepadState(new GamepadState { leftStick = Vector2.up });

            for (int frame = 0; frame < 8; frame++)
            {
                yield return null;
            }

            Vector3 displacement = player.transform.position - initialPosition;
            Assert.That(new Vector2(displacement.x, displacement.z).magnitude, Is.GreaterThan(0.01f));
        }

        [UnityTest]
        public IEnumerator LookInput_ChangesCameraOrientation()
        {
            float initialYaw = cameraRig.Yaw;
            float initialPitch = cameraRig.Pitch;
            QueueGamepadState(new GamepadState { rightStick = new Vector2(0.75f, -0.5f) });

            yield return null;
            yield return null;

            bool orientationChanged =
                !Mathf.Approximately(initialYaw, cameraRig.Yaw) ||
                !Mathf.Approximately(initialPitch, cameraRig.Pitch);
            Assert.That(orientationChanged, Is.True);
        }

        [UnityTest]
        public IEnumerator AimInput_ChangesPresentationWithoutGameplayMutation()
        {
            Vector3 initialPosition = player.transform.position;
            float initialMoveSpeed = locomotion.MoveSpeed;
            float initialRotationResponse = locomotion.RotationResponse;
            float initialGravity = locomotion.Gravity;
            float normalDistance = cameraRig.CurrentDistance;

            QueueGamepadState(new GamepadState { leftTrigger = 1f });
            yield return null;
            yield return null;

            Assert.That(cameraRig.IsAiming, Is.True);
            Assert.That(cameraRig.CurrentDistance, Is.LessThan(normalDistance));
            Assert.That(cameraRig.AimBlend, Is.GreaterThan(0f).And.LessThan(1f));
            Assert.That(player.transform.position.x, Is.EqualTo(initialPosition.x).Within(0.001f));
            Assert.That(player.transform.position.z, Is.EqualTo(initialPosition.z).Within(0.001f));
            Assert.That(locomotion.MoveSpeed, Is.EqualTo(initialMoveSpeed));
            Assert.That(locomotion.RotationResponse, Is.EqualTo(initialRotationResponse));
            Assert.That(locomotion.Gravity, Is.EqualTo(initialGravity));

            yield return new WaitForSecondsRealtime(0.25f);
            Assert.That(cameraRig.AimBlend, Is.EqualTo(1f).Within(0.001f));

            QueueGamepadState(default);
            yield return null;
            yield return null;

            Assert.That(cameraRig.IsAiming, Is.False);
            Assert.That(cameraRig.AimBlend, Is.GreaterThan(0f).And.LessThan(1f));

            yield return new WaitForSecondsRealtime(0.25f);
            Assert.That(cameraRig.AimBlend, Is.EqualTo(0f).Within(0.001f));
            Assert.That(cameraRig.CurrentDistance, Is.EqualTo(normalDistance).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator RapidAimToggle_KeepsPresentationContinuousAndBounded()
        {
            float normalDistance = cameraRig.CurrentDistance;
            SerializedObject cameraRigSettings = new SerializedObject(cameraRig);
            float aimDistance = cameraRigSettings.FindProperty("aimDistance").floatValue;

            for (int toggle = 0; toggle < 10; toggle++)
            {
                bool aiming = toggle % 2 == 0;
                QueueGamepadState(aiming
                    ? new GamepadState { leftTrigger = 1f }
                    : default);
                yield return null;
                yield return null;

                Assert.That(cameraRig.IsAiming, Is.EqualTo(aiming));
                Assert.That(cameraRig.AimBlend, Is.InRange(0f, 1f));
                Assert.That(cameraRig.CurrentDistance, Is.InRange(aimDistance, normalDistance));
            }

            Assert.That(cameraRig.AimBlend, Is.LessThan(1f));
        }

        [UnityTest]
        public IEnumerator CameraCollision_IgnoresCarryLayerButStillResolvesAgainstWalls()
        {
            CinemachineThirdPersonFollow thirdPersonFollow =
                cameraRigObject.GetComponentInChildren<CinemachineThirdPersonFollow>();
            Assert.That(thirdPersonFollow, Is.Not.Null);

            int collisionMask = thirdPersonFollow.AvoidObstacles.CollisionFilter.value;
            Assert.That(collisionMask & (1 << 0), Is.Not.EqualTo(0));
            Assert.That(
                collisionMask & Physics.IgnoreRaycastLayer,
                Is.EqualTo(0));

            cameraRig.enabled = false;
            Transform orbitTarget = cameraRigObject.transform.Find("OrbitTarget");
            Assert.That(orbitTarget, Is.Not.Null);
            orbitTarget.position = player.transform.position + Vector3.up * 1.45f;
            orbitTarget.rotation = Quaternion.Euler(0f, 180f, 0f);

            yield return null;
            yield return null;

            thirdPersonFollow.GetRigPositions(
                out _,
                out _,
                out Vector3 handPosition);
            Vector3 boomDirection = orbitTarget.forward * -1f;

            GameObject carriedObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            carriedObject.name = "CameraIgnoredCarriedObject";
            carriedObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            carriedObject.transform.position = handPosition + boomDirection * 1f;
            carriedObject.transform.localScale = Vector3.one * 0.8f;

            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "CameraBlockingWall";
            wall.transform.position = handPosition + boomDirection * 2.2f;
            wall.transform.localScale = new Vector3(4f, 4f, 0.25f);
            Collider wallCollider = wall.GetComponent<Collider>();

            try
            {
                Physics.SyncTransforms();
                yield return null;
                yield return null;
                yield return null;

                Assert.That(thirdPersonFollow.CurrentObstacle, Is.SameAs(wallCollider));
                Assert.That(IsFinite(cameraRig.OutputCamera.transform.position), Is.True);
                Assert.That(IsFinite(cameraRig.OutputCamera.transform.rotation), Is.True);
            }
            finally
            {
                Object.Destroy(carriedObject);
                Object.Destroy(wall);
            }
        }

        [UnityTest]
        public IEnumerator Locomotion_RemainsGroundedWithoutUncontrolledVerticalVelocity()
        {
            QueueGamepadState(new GamepadState { leftStick = new Vector2(0.5f, 0.75f) });

            for (int frame = 0; frame < 30; frame++)
            {
                yield return null;
                Assert.That(Mathf.Abs(locomotion.VerticalVelocity), Is.LessThan(6f));
            }

            QueueGamepadState(default);
            yield return null;
            yield return null;

            CharacterController controller = player.GetComponent<CharacterController>();
            Collider floorCollider = floor.GetComponent<Collider>();
            float groundSeparation = controller.bounds.min.y - floorCollider.bounds.max.y;
            bool returnedToGroundContact = groundSeparation <= controller.skinWidth + 0.02f;
            Assert.That(locomotion.IsGrounded || returnedToGroundContact, Is.True);
            Assert.That(typeof(ThirdPersonLocomotion).GetMethod("Jump"), Is.Null);
        }

        private void QueueGamepadState(GamepadState state)
        {
            InputSystem.QueueStateEvent(gamepad, state);
            InputSystem.Update();
        }

        private static bool IsFinite(Vector3 value)
        {
            return float.IsFinite(value.x) &&
                   float.IsFinite(value.y) &&
                   float.IsFinite(value.z);
        }

        private static bool IsFinite(Quaternion value)
        {
            return float.IsFinite(value.x) &&
                   float.IsFinite(value.y) &&
                   float.IsFinite(value.z) &&
                   float.IsFinite(value.w);
        }
    }
}
