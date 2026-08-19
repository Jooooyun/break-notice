using System.Collections;
using System.Reflection;
using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Input;
using BreakNotice.Gameplay.Interaction;
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
        private GameObject interactionTarget;
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
            locomotion.ConfigureCamera(cameraRig.GameplayMovementReference);

            Physics.SyncTransforms();
            QueueGamepadState(default);
            for (int frame = 0; frame < 3; frame++)
            {
                yield return null;
            }
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

            if (interactionTarget != null)
            {
                Object.Destroy(interactionTarget);
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
            Assert.That(new Vector2(displacement.x, displacement.z).magnitude, Is.GreaterThan(0.006f));
        }

        [UnityTest]
        public IEnumerator SprintInput_UsesHigherSpeedAndAimCancelsSprint()
        {
            QueueMobilityGamepadState(Vector2.up, true, false);
            yield return null;
            yield return null;

            Assert.That(locomotion.IsSprinting, Is.True);
            Assert.That(locomotion.CurrentHorizontalSpeed,
                Is.EqualTo(locomotion.SprintSpeed).Within(0.001f));
            Assert.That(locomotion.SprintSpeed, Is.GreaterThan(locomotion.MoveSpeed));

            QueueMobilityGamepadState(Vector2.up, true, false, 1f);
            yield return null;

            Assert.That(logicalInput.Snapshot.Aim, Is.True);
            Assert.That(locomotion.IsSprinting, Is.False);
            Assert.That(locomotion.CurrentHorizontalSpeed,
                Is.EqualTo(locomotion.MoveSpeed).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator LightCarry_AllowsSprintJumpAndReturnsToGround()
        {
            ConfigureLogicalCarry(PrototypeWeightClass.Light);
            float groundedY = GetExpectedGroundedPlayerY();
            uint takeoffRevisionBefore = locomotion.JumpTakeoffRevision;

            ProcessMobilitySnapshotNow(Vector2.up, true, true);

            Assert.That(locomotion.IsSprinting, Is.True);
            Assert.That(
                locomotion.JumpTakeoffRevision,
                Is.EqualTo(takeoffRevisionBefore + 1u),
                "An accepted jump must publish its physical takeoff in the same frame.");

            QueueMobilityGamepadState(Vector2.zero, false, false);
            float maximumY = player.transform.position.y;
            bool launched = false;
            bool becameAirborne = false;
            bool returnedToGround = false;
            float deadline = Time.realtimeSinceStartup + 2f;
            while (Time.realtimeSinceStartup < deadline)
            {
                yield return null;
                maximumY = Mathf.Max(maximumY, player.transform.position.y);
                launched |= locomotion.VerticalVelocity > 0f;
                becameAirborne |= launched && !IsAtGroundContact();
                if (becameAirborne &&
                    locomotion.VerticalVelocity <= 0f &&
                    IsAtGroundContact())
                {
                    returnedToGround = true;
                    break;
                }
            }

            Assert.That(launched, Is.True);
            Assert.That(maximumY, Is.GreaterThan(groundedY + 0.5f));
            Assert.That(
                returnedToGround,
                Is.True,
                $"groundedY={groundedY:F3} maximumY={maximumY:F3} " +
                $"finalY={player.transform.position.y:F3} " +
                $"verticalVelocity={locomotion.VerticalVelocity:F3} " +
                $"isGrounded={locomotion.IsGrounded} contact={IsAtGroundContact()}");
            Assert.That(player.transform.position.y, Is.EqualTo(groundedY).Within(0.12f));
        }

        [UnityTest]
        public IEnumerator MediumCarry_BlocksSprintAndJumpWithoutBlockingWalk()
        {
            ConfigureLogicalCarry(PrototypeWeightClass.Medium);
            Vector3 start = player.transform.position;
            float groundedY = GetExpectedGroundedPlayerY();
            uint takeoffRevisionBefore = locomotion.JumpTakeoffRevision;

            ProcessMobilitySnapshotNow(Vector2.up, true, true);

            Assert.That(locomotion.IsSprinting, Is.False);
            Assert.That(locomotion.VerticalVelocity, Is.LessThanOrEqualTo(0f));
            Assert.That(
                locomotion.JumpTakeoffRevision,
                Is.EqualTo(takeoffRevisionBefore),
                "A blocked Medium-carry jump must not publish a takeoff revision.");

            QueueMobilityGamepadState(Vector2.up, true, false);
            float maximumY = player.transform.position.y;
            for (int frame = 0; frame < 12; frame++)
            {
                yield return null;
                maximumY = Mathf.Max(maximumY, player.transform.position.y);
            }

            Assert.That(locomotion.IsSprinting, Is.False);
            Assert.That(locomotion.CurrentHorizontalSpeed,
                Is.EqualTo(locomotion.MoveSpeed).Within(0.001f));
            Assert.That(maximumY, Is.LessThan(groundedY + 0.25f));
            Assert.That(player.transform.position.y, Is.EqualTo(groundedY).Within(0.12f));
            Assert.That(
                new Vector2(
                    player.transform.position.x - start.x,
                    player.transform.position.z - start.z).magnitude,
                Is.GreaterThan(0.01f));
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
        public IEnumerator NormalCamera_UsesLeonCoreProfile()
        {
            yield return null;

            Assert.That(cameraRig.CoreMode, Is.EqualTo(CameraCoreMode.Normal));
            Assert.That(cameraRig.CurrentDistance, Is.EqualTo(2.55f).Within(0.001f));
            Assert.That(cameraRig.CurrentFieldOfView, Is.EqualTo(50f).Within(0.001f));
            Assert.That(cameraRig.CurrentShoulderOffset.x, Is.EqualTo(0.85f).Within(0.001f));
            Assert.That(cameraRig.CurrentShoulderOffset.y, Is.EqualTo(0f).Within(0.001f));
            Assert.That(cameraRig.CurrentVerticalArmLength, Is.EqualTo(0.1f).Within(0.001f));
            Assert.That(IsFinite(cameraRig.OutputCamera.transform.position), Is.True);
            Assert.That(IsFinite(cameraRig.OutputCamera.transform.rotation), Is.True);
        }

        [UnityTest]
        public IEnumerator SprintCamera_PullsBackWithoutFovPumpingAndReturnsToNormal()
        {
            QueueMobilityGamepadState(Vector2.up, true, false);
            yield return new WaitForSecondsRealtime(0.25f);

            Assert.That(locomotion.IsSprinting, Is.True);
            Assert.That(cameraRig.CoreMode, Is.EqualTo(CameraCoreMode.Sprint));
            Assert.That(cameraRig.SprintBlend, Is.EqualTo(1f).Within(0.001f));
            Assert.That(cameraRig.CurrentDistance, Is.EqualTo(2.7f).Within(0.001f));
            Assert.That(cameraRig.CurrentFieldOfView, Is.EqualTo(50f).Within(0.001f));
            Assert.That(cameraRig.CurrentShoulderOffset.x, Is.EqualTo(0.85f).Within(0.001f));

            QueueGamepadState(default);
            yield return new WaitForSecondsRealtime(0.25f);

            Assert.That(cameraRig.CoreMode, Is.EqualTo(CameraCoreMode.Normal));
            Assert.That(cameraRig.SprintBlend, Is.EqualTo(0f).Within(0.001f));
            Assert.That(cameraRig.CurrentDistance, Is.EqualTo(2.55f).Within(0.001f));
            Assert.That(cameraRig.CurrentFieldOfView, Is.EqualTo(50f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator NormalCamera_KeepsGrayboxProxyVisibleAtSixteenByNine()
        {
            cameraRig.OutputCamera.aspect = 16f / 9f;
            yield return null;
            yield return null;

            Vector3 lowerProxyViewport = cameraRig.OutputCamera.WorldToViewportPoint(
                player.transform.position + Vector3.up * 0.5f);
            Vector3 upperProxyViewport = cameraRig.OutputCamera.WorldToViewportPoint(
                player.transform.position + Vector3.up * 0.9f);

            string framingContext =
                $"player={player.transform.position} camera={cameraRig.OutputCamera.transform.position} " +
                $"cameraForward={cameraRig.OutputCamera.transform.forward} " +
                $"lowerProxy={lowerProxyViewport} upperProxy={upperProxyViewport}";
            Assert.That(lowerProxyViewport.z, Is.GreaterThan(0f), framingContext);
            Assert.That(upperProxyViewport.z, Is.GreaterThan(0f), framingContext);
            Assert.That(lowerProxyViewport.x, Is.InRange(0.2f, 0.42f), framingContext);
            Assert.That(upperProxyViewport.x, Is.InRange(0.2f, 0.42f), framingContext);
            Assert.That(lowerProxyViewport.y, Is.InRange(0.2f, 0.8f), framingContext);
            Assert.That(upperProxyViewport.y, Is.InRange(0.35f, 0.95f), framingContext);
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
            Assert.That(cameraRig.CoreMode, Is.EqualTo(CameraCoreMode.Aim));
            Assert.That(cameraRig.CurrentDistance, Is.EqualTo(1.75f).Within(0.001f));
            Assert.That(cameraRig.CurrentFieldOfView, Is.EqualTo(48f).Within(0.001f));
            Assert.That(cameraRig.CurrentShoulderOffset.x, Is.EqualTo(0.75f).Within(0.001f));
            Assert.That(cameraRig.CurrentShoulderOffset.y, Is.EqualTo(0f).Within(0.001f));
            Assert.That(cameraRig.CurrentVerticalArmLength, Is.EqualTo(0.08f).Within(0.001f));

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
        public IEnumerator AimMovement_UsesCameraRelativeStrafeAndBoundedBodyAlignment()
        {
            player.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            Vector3 start = player.transform.position;
            QueueGamepadState(new GamepadState
            {
                leftTrigger = 1f,
                leftStick = Vector2.right
            });

            yield return new WaitForSecondsRealtime(0.3f);

            Vector3 cameraForward = Vector3.ProjectOnPlane(
                cameraRig.OutputCamera.transform.forward,
                Vector3.up).normalized;
            float yawError = Vector3.Angle(player.transform.forward, cameraForward);
            Vector3 displacement = player.transform.position - start;
            displacement.y = 0f;
            Assert.That(cameraRig.IsAiming, Is.True);
            Assert.That(yawError, Is.LessThan(8f));
            Assert.That(displacement.magnitude, Is.GreaterThan(0.05f));
            Assert.That(
                Mathf.Abs(Vector3.Dot(displacement.normalized, cameraForward)),
                Is.LessThan(0.35f),
                "Aim left/right input must strafe rather than turn into forward locomotion.");
        }

        [UnityTest]
        public IEnumerator FinalCameraSnapshot_IsCurrentBrainOutputAndCenterOpticalRay()
        {
            FinalCameraSnapshot snapshot = FinalCameraSnapshot.Invalid(-1);
            void CaptureSnapshot(CinemachineBrain brain)
            {
                if (brain != null && brain.OutputCamera == cameraRig.OutputCamera)
                {
                    cameraRig.TryGetFinalCameraSnapshot(out snapshot);
                }
            }

            CinemachineCore.CameraUpdatedEvent.AddListener(CaptureSnapshot);
            try
            {
                yield return null;
                yield return null;
            }
            finally
            {
                CinemachineCore.CameraUpdatedEvent.RemoveListener(CaptureSnapshot);
            }

            Assert.That(snapshot.IsValid, Is.True);
            Assert.That(snapshot.FrameId, Is.InRange(Time.frameCount - 1, Time.frameCount));
            Assert.That(snapshot.ProjectionMode,
                Is.EqualTo(FinalCameraProjectionMode.Perspective));
            Assert.That(snapshot.ActualCameraTransformPosition,
                Is.EqualTo(cameraRig.OutputCamera.transform.position));
            Assert.That(
                Vector3.Dot(snapshot.CenterViewRay.direction,
                    cameraRig.OutputCamera.transform.forward),
                Is.GreaterThan(0.9999f));
            Assert.That(
                Vector3.Distance(
                    snapshot.CenterViewRay.origin,
                    snapshot.ActualCameraTransformPosition),
                Is.EqualTo(snapshot.NearClipPlane).Within(0.002f));

            Vector3 centerPoint = snapshot.CenterViewRay.GetPoint(10f);
            Vector3 viewport = cameraRig.OutputCamera.WorldToViewportPoint(centerPoint);
            float horizontalPixelError =
                Mathf.Abs(viewport.x - 0.5f) * cameraRig.OutputCamera.pixelWidth;
            float verticalPixelError =
                Mathf.Abs(viewport.y - 0.5f) * cameraRig.OutputCamera.pixelHeight;
            Assert.That(horizontalPixelError, Is.LessThanOrEqualTo(1f));
            Assert.That(verticalPixelError, Is.LessThanOrEqualTo(1f));
        }

        [UnityTest]
        public IEnumerator AimCamera_RotationRemainsFiniteAndFree()
        {
            float initialYaw = cameraRig.Yaw;
            QueueGamepadState(new GamepadState
            {
                leftTrigger = 1f,
                rightStick = new Vector2(0.8f, 0.2f)
            });

            yield return new WaitForSecondsRealtime(0.25f);
            yield return null;

            Assert.That(cameraRig.AimBlend, Is.EqualTo(1f).Within(0.001f));
            Assert.That(cameraRig.Yaw, Is.Not.EqualTo(initialYaw).Within(0.001f));
            Assert.That(IsFinite(cameraRig.OutputCamera.transform.position), Is.True);
            Assert.That(IsFinite(cameraRig.OutputCamera.transform.rotation), Is.True);
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
        public IEnumerator CameraCollision_UsesSinglePreBrainSolverAndIgnoresDynamicProps()
        {
            CinemachineThirdPersonFollow thirdPersonFollow =
                cameraRigObject.GetComponentInChildren<CinemachineThirdPersonFollow>();
            Assert.That(thirdPersonFollow, Is.Not.Null);
            Assert.That(thirdPersonFollow.AvoidObstacles.Enabled, Is.False);
            Assert.That(thirdPersonFollow.AvoidObstacles.CollisionFilter.value, Is.Zero);
            Assert.That(cameraRig.PostBrainTransformWriteCount, Is.Zero);

            Transform orbitTarget = cameraRigObject.transform.Find("OrbitTarget");
            Assert.That(orbitTarget, Is.Not.Null);
            yield return new WaitForSecondsRealtime(0.1f);

            thirdPersonFollow.GetRigPositions(
                out _,
                out _,
                out Vector3 handPosition);
            Vector3 boomDirection =
                (cameraRig.OutputCamera.transform.position - handPosition).normalized;

            GameObject carriedObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            carriedObject.name = "CameraSoftDynamicProp";
            carriedObject.transform.position = handPosition + boomDirection * 1f;
            carriedObject.transform.localScale = Vector3.one * 0.8f;

            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "CameraBlockingWall";
            wall.transform.position = handPosition + boomDirection * 2.2f;
            wall.transform.localScale = new Vector3(4f, 4f, 0.25f);
            int staticWorldLayer = LayerMask.NameToLayer("CameraStaticWorld");
            Assert.That(staticWorldLayer, Is.GreaterThanOrEqualTo(0));
            wall.layer = staticWorldLayer;
            Collider wallCollider = wall.GetComponent<Collider>();

            try
            {
                Physics.SyncTransforms();
                yield return null;
                yield return null;
                yield return null;

                Assert.That(cameraRig.CurrentBlockingCollider, Is.SameAs(wallCollider));
                Assert.That(cameraRig.HardConstraintActiveThisFrame, Is.True);
                Assert.That(cameraRig.ConstraintEvaluationsThisFrame, Is.EqualTo(1));
                Assert.That(cameraRig.PostBrainTransformWriteCount, Is.Zero);
                Assert.That(IsFinite(cameraRig.OutputCamera.transform.position), Is.True);
                Assert.That(IsFinite(cameraRig.OutputCamera.transform.rotation), Is.True);

                wall.transform.position += Vector3.up * 20f;
                Physics.SyncTransforms();
                float recoveryDeadline = Time.realtimeSinceStartup + 1.2f;
                while ((cameraRig.HardConstraintActiveThisFrame ||
                        cameraRig.SolvedShoulderScale < 0.999f ||
                        cameraRig.SafeBoomFraction < 0.999f) &&
                       Time.realtimeSinceStartup < recoveryDeadline)
                {
                    yield return null;
                }

                Assert.That(cameraRig.HardConstraintActiveThisFrame, Is.False);
                Assert.That(cameraRig.SolvedShoulderScale, Is.EqualTo(1f).Within(0.001f));
                Assert.That(cameraRig.SafeBoomFraction, Is.EqualTo(1f).Within(0.001f));
                Assert.That(cameraRig.CurrentBlockingCollider, Is.Null);
                Assert.That(locomotion.MovementReference,
                    Is.SameAs(cameraRig.GameplayMovementReference));
                Assert.That(locomotion.MovementReference,
                    Is.Not.SameAs(cameraRig.OutputCamera.transform));
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
            Assert.That(locomotion.JumpHeight, Is.EqualTo(1.15f).Within(0.001f));
            Assert.That(locomotion.CoyoteTime, Is.InRange(0.05f, 0.15f));
            Assert.That(locomotion.JumpBufferTime, Is.InRange(0.05f, 0.15f));
        }

        private void QueueGamepadState(GamepadState state)
        {
            InputSystem.QueueStateEvent(gamepad, state);
            InputSystem.Update();
        }

        private void QueueMobilityGamepadState(
            Vector2 move,
            bool sprint,
            bool jump,
            float aim = 0f)
        {
            var state = new GamepadState
            {
                leftStick = move,
                leftTrigger = aim
            };
            if (sprint)
            {
                state = state.WithButton(GamepadButton.LeftStick);
            }
            if (jump)
            {
                state = state.WithButton(GamepadButton.North);
            }

            QueueGamepadState(state);
        }

        private void ProcessMobilitySnapshotNow(Vector2 move, bool sprint, bool jump)
        {
            PropertyInfo snapshotProperty = typeof(InputSystemLogicalInputAdapter)
                .GetProperty(nameof(InputSystemLogicalInputAdapter.Snapshot));
            Assert.That(snapshotProperty, Is.Not.Null);
            MethodInfo snapshotSetter = snapshotProperty.GetSetMethod(true);
            Assert.That(snapshotSetter, Is.Not.Null);
            snapshotSetter.Invoke(
                logicalInput,
                new object[]
                {
                    new LogicalInputSnapshot(
                        move,
                        Vector2.zero,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        sprint: sprint,
                        jump: jump)
                });

            MethodInfo updateMethod = typeof(ThirdPersonLocomotion).GetMethod(
                "Update",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(updateMethod, Is.Not.Null);
            updateMethod.Invoke(locomotion, null);
        }

        private void ConfigureLogicalCarry(PrototypeWeightClass weightClass)
        {
            PrototypeInteractionStateOwner owner =
                player.GetComponent<PrototypeInteractionStateOwner>() ??
                player.AddComponent<PrototypeInteractionStateOwner>();
            locomotion.ConfigureInteractionOwner(owner);

            interactionTarget = new GameObject($"Mobility{weightClass}Target");
            PrototypeInteractionTarget target =
                interactionTarget.AddComponent<PrototypeInteractionTarget>();
            target.Configure(
                weightClass == PrototypeWeightClass.Light ? "OBJ-P-L-001" : "OBJ-P-M-001",
                $"MOBILITY-{weightClass.ToString().ToUpperInvariant()}-01",
                weightClass == PrototypeWeightClass.Light ? "FILE BINDER" : "OFFICE CHAIR",
                weightClass,
                weightClass == PrototypeWeightClass.Light
                    ? PrototypeInteractionRole.DirectBinder
                    : PrototypeInteractionRole.DirectChair,
                true,
                false,
                false,
                weightClass == PrototypeWeightClass.Medium);

            Assert.That(
                owner.TryCommit(PrototypeInteractionDecision.Accept(
                    PrototypeInteractionOutcome.Scoop,
                    target)),
                Is.True);
            Assert.That(owner.TryCompleteAcquisition(target), Is.True);
        }

        private float GetExpectedGroundedPlayerY()
        {
            CharacterController characterController =
                player.GetComponent<CharacterController>();
            Collider floorCollider = floor.GetComponent<Collider>();
            return floorCollider.bounds.max.y +
                (characterController.height * 0.5f) -
                characterController.center.y;
        }

        private bool IsAtGroundContact()
        {
            CharacterController characterController =
                player.GetComponent<CharacterController>();
            Collider floorCollider = floor.GetComponent<Collider>();
            float separation = characterController.bounds.min.y - floorCollider.bounds.max.y;
            return locomotion.IsGrounded ||
                separation <= characterController.skinWidth + 0.02f;
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
