using System;
using System.Collections;
using System.Linq;
using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Encounter;
using BreakNotice.Gameplay.Input;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestShowcaseTraversalPlayModeTests
    {
        private const string ShowcaseScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v001.unity";

        private const string ShowcaseSceneName =
            "Contest_OperationsFloor_Showcase_v001";

        private const float WaypointTolerance = 0.45f;
        private const float PenetrationTolerance = 0.0015f;

        private static readonly RouteSegment[] TraversalSegments =
        {
            new RouteSegment(
                "Entry to South Bowl",
                new Vector2(0f, -5f)),
            new RouteSegment(
                "West loop outer leg",
                new Vector2(-9f, -5f),
                new Vector2(-9f, 14f),
                new Vector2(0f, 14f),
                new Vector2(0f, 10.5f)),
            new RouteSegment(
                "West loop central closure",
                new Vector2(-1.5f, 9f),
                new Vector2(-1.5f, -3f),
                new Vector2(0f, -5f)),
            new RouteSegment(
                "East loop outer leg",
                new Vector2(9f, -5f),
                new Vector2(9f, 9f),
                new Vector2(13f, 9f),
                new Vector2(13f, 14f),
                new Vector2(0f, 14f),
                new Vector2(0f, 10.5f)),
            new RouteSegment(
                "East loop central closure",
                new Vector2(-1.5f, 9f),
                new Vector2(-1.5f, -3f),
                new Vector2(0f, -5f)),
            new RouteSegment(
                "Central crossing to North Cross",
                new Vector2(-1.5f, -3f),
                new Vector2(-1.5f, 9f),
                new Vector2(0f, 10.5f)),
            new RouteSegment(
                "North Cross to Exit approach",
                new Vector2(0f, 14f),
                new Vector2(20f, 14.5f),
                new Vector2(20.5f, 16.5f))
        };

        private Scene showcaseScene;
        private Gamepad gamepad;
        private Transform showcaseRoot;
        private Transform playerTransform;
        private CharacterController playerController;
        private InputSystemLogicalInputAdapter logicalInput;
        private ThirdPersonLocomotion locomotion;
        private ThirdPersonCameraRig cameraRig;
        private CinemachineThirdPersonFollow thirdPersonFollow;
        private AuthoredEncounterResetController resetController;
        private Collider floorCollider;
        private Collider[] blockingColliders = Array.Empty<Collider>();
        private Collider[] cameraBlockingColliders = Array.Empty<Collider>();
        private GameObject lateFrameProbeObject;
        private ContestShowcaseTraversalLateFrameProbe lateFrameProbe;
        private ContestShowcaseTraversalFrame latestFrame;
        private Vector3 playerBaselinePosition;
        private Quaternion playerBaselineRotation;

        [UnitySetUp]
        public IEnumerator SetUpFixture()
        {
            gamepad = InputSystem.AddDevice<Gamepad>();
            QueueGamepadState(default);

            AsyncOperation loadOperation = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ShowcaseScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
            Assert.That(loadOperation, Is.Not.Null, "The real Showcase Scene could not be loaded.");
            yield return loadOperation;

            showcaseScene = SceneManager.GetSceneByPath(ShowcaseScenePath);
            if (!showcaseScene.IsValid())
            {
                showcaseScene = SceneManager.GetSceneByName(ShowcaseSceneName);
            }

            Assert.That(showcaseScene.IsValid(), Is.True);
            Assert.That(showcaseScene.isLoaded, Is.True);

            showcaseRoot = RequireRoot("SHOWCASE_S1_GRAYBOX");
            playerTransform = RequireRoot("Player");
            playerController = playerTransform.GetComponent<CharacterController>();
            logicalInput = playerTransform.GetComponent<InputSystemLogicalInputAdapter>();
            locomotion = playerTransform.GetComponent<ThirdPersonLocomotion>();
            cameraRig = FindSingleComponent<ThirdPersonCameraRig>();
            thirdPersonFollow = FindSingleComponent<CinemachineThirdPersonFollow>();
            resetController = FindSingleComponent<AuthoredEncounterResetController>();
            floorCollider = showcaseRoot.GetComponentsInChildren<Collider>(true)
                .Single(collider => string.Equals(
                    collider.name,
                    "Floor_48x34",
                    StringComparison.Ordinal));

            Assert.That(playerController, Is.Not.Null);
            Assert.That(logicalInput, Is.Not.Null);
            Assert.That(locomotion, Is.Not.Null);
            Assert.That(cameraRig, Is.Not.Null);
            Assert.That(cameraRig.OutputCamera, Is.Not.Null);
            Assert.That(thirdPersonFollow, Is.Not.Null);
            Assert.That(thirdPersonFollow.AvoidObstacles.Enabled, Is.True);
            Assert.That(
                thirdPersonFollow.AvoidObstacles.CameraRadius,
                Is.GreaterThan(0f));
            Assert.That(resetController, Is.Not.Null);
            Assert.That(resetController.BaselineObjectCount, Is.EqualTo(7));

            blockingColliders = showcaseScene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Collider>(true))
                .Where(collider => collider != null &&
                                   collider.enabled &&
                                   !collider.isTrigger &&
                                   collider != playerController &&
                                   collider != floorCollider &&
                                   !collider.transform.IsChildOf(playerTransform))
                .ToArray();

            cameraBlockingColliders = showcaseRoot
                .GetComponentsInChildren<Collider>(true)
                .Where(collider => collider != null &&
                                   collider.enabled &&
                                   !collider.isTrigger &&
                                   collider != floorCollider)
                .ToArray();

            lateFrameProbeObject = new GameObject("ContestShowcaseTraversalLateFrameProbe");
            SceneManager.MoveGameObjectToScene(lateFrameProbeObject, showcaseScene);
            lateFrameProbe = lateFrameProbeObject
                .AddComponent<ContestShowcaseTraversalLateFrameProbe>();
            lateFrameProbe.Configure(
                playerTransform,
                playerController,
                locomotion,
                logicalInput,
                cameraRig.OutputCamera.transform);

            for (int frame = 0; frame < 4; frame++)
            {
                QueueGamepadState(default);
                yield return CaptureNextPlayableFrame();
            }

            playerBaselinePosition = latestFrame.PlayerPosition;
            playerBaselineRotation = latestFrame.PlayerRotation;
            AssertPlanarNear(playerBaselinePosition, new Vector2(0f, -15f), 0.02f, "authored spawn");
            AssertFrameState("authored spawn");
        }

        [UnityTearDown]
        public IEnumerator TearDownFixture()
        {
            if (gamepad != null && gamepad.added)
            {
                QueueGamepadState(default);
            }

            if (showcaseScene.IsValid() && showcaseScene.isLoaded)
            {
                AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(showcaseScene);
                if (unloadOperation != null)
                {
                    yield return unloadOperation;
                }
            }

            if (gamepad != null && gamepad.added)
            {
                InputSystem.RemoveDevice(gamepad);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator OperationsFloor_CurrentLocomotionAndCamera_TraversesFigureEightBeforeAndAfterReset()
        {
            int resetCountBeforeTraversal = resetController.ResetCount;

            yield return TraverseOperationsFloor(passNumber: 1);

            Assert.That(
                resetController.RequestReset(EncounterResetCause.ManualInput),
                Is.True,
                "The authored Reset barrier rejected the between-pass reset.");
            QueueGamepadState(default);
            yield return CaptureNextPlayableFrame();

            Assert.That(resetController.IsResetInProgress, Is.False);
            Assert.That(resetController.ResetCount, Is.EqualTo(resetCountBeforeTraversal + 1));
            Assert.That(resetController.UnexpectedBaselineDifferenceCount, Is.Zero);
            Assert.That(resetController.OwnershipLeakCount, Is.Zero);
            Assert.That(
                Vector2.Distance(
                    Planar(latestFrame.PlayerPosition),
                    Planar(playerBaselinePosition)),
                Is.LessThanOrEqualTo(0.01f),
                $"Reset did not restore the authored planar player spawn; " +
                $"baseline was {playerBaselinePosition}, actual was {latestFrame.PlayerPosition}.");
            Assert.That(
                Quaternion.Angle(latestFrame.PlayerRotation, playerBaselineRotation),
                Is.LessThanOrEqualTo(0.1f),
                "Reset did not restore the authored player rotation.");
            AssertFrameState("post-reset authored baseline");

            yield return TraverseOperationsFloor(passNumber: 2);

            Assert.That(resetController.UnexpectedBaselineDifferenceCount, Is.Zero);
            Assert.That(resetController.ObjectLossCount, Is.Zero);
            Assert.That(resetController.ObjectDuplicationCount, Is.Zero);
            Assert.That(resetController.OwnershipLeakCount, Is.Zero);
        }

        private IEnumerator TraverseOperationsFloor(int passNumber)
        {
            Assert.That(
                Vector2.Distance(Planar(latestFrame.PlayerPosition), new Vector2(0f, -15f)),
                Is.LessThanOrEqualTo(0.05f),
                $"Pass {passNumber} did not begin at the authored spawn.");

            foreach (RouteSegment segment in TraversalSegments)
            {
                yield return TraverseSegment(segment, passNumber);
            }

            AssertPlanarNear(
                latestFrame.PlayerPosition,
                new Vector2(20.5f, 16.5f),
                WaypointTolerance,
                $"pass {passNumber} Exit approach completion");
        }

        private IEnumerator TraverseSegment(RouteSegment segment, int passNumber)
        {
            for (int waypointIndex = 0; waypointIndex < segment.Waypoints.Length; waypointIndex++)
            {
                yield return MoveToWaypoint(
                    segment.Name,
                    waypointIndex,
                    segment.Waypoints[waypointIndex],
                    passNumber);
            }

            QueueGamepadState(default);
            yield return CaptureNextPlayableFrame();

            Vector2 endpoint = segment.Waypoints[segment.Waypoints.Length - 1];
            AssertPlanarNear(
                latestFrame.PlayerPosition,
                endpoint,
                WaypointTolerance,
                $"pass {passNumber} {segment.Name}");
            AssertFrameState($"pass {passNumber} {segment.Name} completion");
        }

        private IEnumerator MoveToWaypoint(
            string segmentName,
            int waypointIndex,
            Vector2 waypoint,
            int passNumber)
        {
            float initialDistance = Vector2.Distance(Planar(latestFrame.PlayerPosition), waypoint);
            float maximumDuration = Mathf.Max(
                3f,
                (initialDistance / Mathf.Max(locomotion.MoveSpeed, 0.01f)) * 2.5f + 2f);
            float elapsed = 0f;
            float progressWindowElapsed = 0f;
            float progressWindowStartDistance = initialDistance;
            bool observedLogicalMove = false;

            while (Vector2.Distance(Planar(latestFrame.PlayerPosition), waypoint) > WaypointTolerance)
            {
                Vector2 current = Planar(latestFrame.PlayerPosition);
                Vector2 desiredWorldDirection = (waypoint - current).normalized;
                Vector2 moveInput = WorldDirectionToCameraInput(desiredWorldDirection);
                QueueGamepadState(new GamepadState { leftStick = moveInput });
                yield return CaptureNextPlayableFrame();

                float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
                elapsed += deltaTime;
                progressWindowElapsed += deltaTime;
                observedLogicalMove |= latestFrame.LogicalMove.sqrMagnitude > 0.05f;

                string location =
                    $"pass {passNumber} {segmentName} waypoint {waypointIndex + 1} " +
                    $"({waypoint.x:0.0}, {waypoint.y:0.0})";
                AssertFrameState(location);

                float remainingDistance = Vector2.Distance(Planar(latestFrame.PlayerPosition), waypoint);
                if (progressWindowElapsed >= 0.75f && remainingDistance > 1f)
                {
                    Assert.That(
                        remainingDistance,
                        Is.LessThan(progressWindowStartDistance - 0.2f),
                        $"Traversal stalled at {location}; remaining distance " +
                        $"was {remainingDistance:0.000} m.");
                    progressWindowElapsed = 0f;
                    progressWindowStartDistance = remainingDistance;
                }

                Assert.That(
                    elapsed,
                    Is.LessThanOrEqualTo(maximumDuration),
                    $"Traversal timed out at {location}; current position was " +
                    $"{latestFrame.PlayerPosition}.");
            }

            Assert.That(
                observedLogicalMove || initialDistance <= WaypointTolerance,
                Is.True,
                $"The logical Move action was never observed for {segmentName}.");
        }

        private Vector2 WorldDirectionToCameraInput(Vector2 desiredWorldDirection)
        {
            Vector3 cameraForward = Vector3.ProjectOnPlane(
                latestFrame.CameraRotation * Vector3.forward,
                Vector3.up).normalized;
            Vector3 cameraRight = Vector3.ProjectOnPlane(
                latestFrame.CameraRotation * Vector3.right,
                Vector3.up).normalized;
            Vector3 desired = new Vector3(
                desiredWorldDirection.x,
                0f,
                desiredWorldDirection.y);
            Vector2 input = new Vector2(
                Vector3.Dot(desired, cameraRight),
                Vector3.Dot(desired, cameraForward));
            return input.sqrMagnitude > 1f ? input.normalized : input;
        }

        private void AssertFrameState(string location)
        {
            Assert.That(IsFinite(latestFrame.PlayerPosition), Is.True, $"Player position is non-finite at {location}.");
            Assert.That(IsFinite(latestFrame.PlayerRotation), Is.True, $"Player rotation is non-finite at {location}.");
            Assert.That(IsFinite(latestFrame.CameraPosition), Is.True, $"Camera position is non-finite at {location}.");
            Assert.That(IsFinite(latestFrame.CameraRotation), Is.True, $"Camera rotation is non-finite at {location}.");

            Bounds playerBounds = latestFrame.PlayerBounds;
            Assert.That(playerBounds.min.x, Is.GreaterThanOrEqualTo(-24.001f), $"Player left room bounds at {location}.");
            Assert.That(playerBounds.max.x, Is.LessThanOrEqualTo(24.001f), $"Player left room bounds at {location}.");
            Assert.That(playerBounds.min.z, Is.GreaterThanOrEqualTo(-17.001f), $"Player left room bounds at {location}.");
            Assert.That(playerBounds.max.z, Is.LessThanOrEqualTo(17.001f), $"Player left room bounds at {location}.");
            Assert.That(playerBounds.min.y, Is.GreaterThanOrEqualTo(-0.02f), $"Player fell below the floor at {location}.");
            Assert.That(playerBounds.max.y, Is.LessThanOrEqualTo(4.22f), $"Player exceeded the room height at {location}.");

            float groundSeparation = playerBounds.min.y - floorCollider.bounds.max.y;
            bool hasGroundContact = latestFrame.IsGrounded ||
                                    Mathf.Abs(groundSeparation) <= playerController.skinWidth + 0.03f;
            Assert.That(
                hasGroundContact,
                Is.True,
                $"Player lost grounded contact at {location}; separation was " +
                $"{groundSeparation:0.000} m.");

            Vector3 cameraPosition = latestFrame.CameraPosition;
            Assert.That(cameraPosition.x, Is.InRange(-24.001f, 24.001f), $"Camera left room bounds at {location}.");
            Assert.That(cameraPosition.z, Is.InRange(-17.001f, 17.001f), $"Camera left room bounds at {location}.");
            Assert.That(cameraPosition.y, Is.InRange(-0.001f, 4.201f), $"Camera left room height at {location}.");

            foreach (Collider blocker in blockingColliders)
            {
                if (blocker == null || !blocker.enabled || !blocker.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (playerBounds.Intersects(blocker.bounds))
                {
                    bool overlaps = Physics.ComputePenetration(
                        playerController,
                        latestFrame.PlayerPosition,
                        latestFrame.PlayerRotation,
                        blocker,
                        blocker.transform.position,
                        blocker.transform.rotation,
                        out _,
                        out float penetrationDistance);
                    Assert.That(
                        !overlaps || penetrationDistance <= PenetrationTolerance,
                        Is.True,
                        $"Player penetrated {HierarchyPath(blocker.transform)} by " +
                        $"{penetrationDistance:0.0000} m at {location}.");
                }

            }

            float cameraRadius = thirdPersonFollow.AvoidObstacles.CameraRadius;
            foreach (Collider blocker in cameraBlockingColliders)
            {
                if (blocker == null || !blocker.enabled || !blocker.gameObject.activeInHierarchy)
                {
                    continue;
                }

                float cameraClearance = Vector3.Distance(
                    blocker.ClosestPoint(cameraPosition),
                    cameraPosition);
                Assert.That(
                    cameraClearance,
                    Is.GreaterThanOrEqualTo(cameraRadius - PenetrationTolerance),
                    $"Camera collision sphere penetrated {HierarchyPath(blocker.transform)}; " +
                    $"clearance was {cameraClearance:0.0000} m for radius " +
                    $"{cameraRadius:0.0000} m at {location}.");
            }
        }

        private IEnumerator CaptureNextPlayableFrame()
        {
            int previousSequence = lateFrameProbe.Sequence;
            yield return null;
            Assert.That(
                lateFrameProbe.Sequence,
                Is.GreaterThan(previousSequence),
                "The post-Cinemachine LateUpdate probe did not capture a playable frame.");
            latestFrame = lateFrameProbe.Latest;
        }

        private void QueueGamepadState(GamepadState state)
        {
            InputSystem.QueueStateEvent(gamepad, state);
            InputSystem.Update();
        }

        private Transform RequireRoot(string rootName)
        {
            Transform root = showcaseScene.GetRootGameObjects()
                .Where(gameObject => string.Equals(gameObject.name, rootName, StringComparison.Ordinal))
                .Select(gameObject => gameObject.transform)
                .SingleOrDefault();
            Assert.That(root, Is.Not.Null, $"Missing Showcase root: {rootName}");
            return root;
        }

        private T FindSingleComponent<T>() where T : Component
        {
            T[] components = showcaseScene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
            Assert.That(components, Has.Length.EqualTo(1), typeof(T).Name);
            return components[0];
        }

        private static void AssertPlanarNear(
            Vector3 actual,
            Vector2 expected,
            float tolerance,
            string location)
        {
            Assert.That(
                Vector2.Distance(Planar(actual), expected),
                Is.LessThanOrEqualTo(tolerance),
                $"Planar position did not reach {location}; actual was {actual}.");
        }

        private static Vector2 Planar(Vector3 position)
        {
            return new Vector2(position.x, position.z);
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

        private static string HierarchyPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }

        private sealed class RouteSegment
        {
            public RouteSegment(string name, params Vector2[] waypoints)
            {
                Name = name;
                Waypoints = waypoints;
            }

            public string Name { get; }

            public Vector2[] Waypoints { get; }
        }
    }

    [DefaultExecutionOrder(32000)]
    internal sealed class ContestShowcaseTraversalLateFrameProbe : MonoBehaviour
    {
        private Transform playerTransform;
        private CharacterController playerController;
        private ThirdPersonLocomotion locomotion;
        private InputSystemLogicalInputAdapter logicalInput;
        private Transform cameraTransform;

        public int Sequence { get; private set; }

        public ContestShowcaseTraversalFrame Latest { get; private set; }

        public void Configure(
            Transform player,
            CharacterController controller,
            ThirdPersonLocomotion playerLocomotion,
            InputSystemLogicalInputAdapter input,
            Transform outputCamera)
        {
            playerTransform = player;
            playerController = controller;
            locomotion = playerLocomotion;
            logicalInput = input;
            cameraTransform = outputCamera;
        }

        private void LateUpdate()
        {
            if (playerTransform == null ||
                playerController == null ||
                locomotion == null ||
                logicalInput == null ||
                cameraTransform == null)
            {
                return;
            }

            Latest = new ContestShowcaseTraversalFrame(
                playerTransform.position,
                playerTransform.rotation,
                playerController.bounds,
                locomotion.IsGrounded || playerController.isGrounded,
                logicalInput.Snapshot.Move,
                cameraTransform.position,
                cameraTransform.rotation);
            Sequence++;
        }
    }

    internal readonly struct ContestShowcaseTraversalFrame
    {
        public ContestShowcaseTraversalFrame(
            Vector3 playerPosition,
            Quaternion playerRotation,
            Bounds playerBounds,
            bool isGrounded,
            Vector2 logicalMove,
            Vector3 cameraPosition,
            Quaternion cameraRotation)
        {
            PlayerPosition = playerPosition;
            PlayerRotation = playerRotation;
            PlayerBounds = playerBounds;
            IsGrounded = isGrounded;
            LogicalMove = logicalMove;
            CameraPosition = cameraPosition;
            CameraRotation = cameraRotation;
        }

        public Vector3 PlayerPosition { get; }

        public Quaternion PlayerRotation { get; }

        public Bounds PlayerBounds { get; }

        public bool IsGrounded { get; }

        public Vector2 LogicalMove { get; }

        public Vector3 CameraPosition { get; }

        public Quaternion CameraRotation { get; }
    }
}
