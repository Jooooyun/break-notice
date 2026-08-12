using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Encounter;
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
    public sealed class ContestShowcaseV2TraversalPlayModeTests
    {
        private const string V2ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v002.unity";

        private const string V2SceneName =
            "Contest_OperationsFloor_Showcase_v002";

        private const string V2RootName =
            "SHOWCASE_V2_PROVISIONAL_LOCAL_REVIEW_ONLY";

        private readonly List<GameObject> temporaryBlockers = new List<GameObject>();

        private Scene scene;
        private Gamepad gamepad;
        private GameObject player;
        private ThirdPersonLocomotion locomotion;
        private InputSystemLogicalInputAdapter logicalInput;
        private CharacterController playerController;
        private ThirdPersonCameraRig cameraRig;
        private ShowcaseMobileAllyController mobileAlly;
        private CharacterController allyController;
        private AuthoredEncounterResetController reset;
        private Transform v2Root;
        private Collider[] staticColliders;
        private Vector3 playerSpawn;
        private Vector3 allySpawn;
        private bool observedCatchUp;
        private float previousCaptureDeltaTime;
        private float diagnosticMaximumDeltaTime;
        private float diagnosticMaximumPlayerStep;
        private int diagnosticStartFrame;
        private int diagnosticStartRenderedFrame;

        [UnitySetUp]
        public IEnumerator SetUpFixture()
        {
            previousCaptureDeltaTime = Time.captureDeltaTime;
            Time.captureDeltaTime = 1f / 60f;
            gamepad = InputSystem.AddDevice<Gamepad>();
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                V2ScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
            Assert.That(load, Is.Not.Null);
            while (!load.isDone)
            {
                yield return null;
            }

            scene = SceneManager.GetSceneByPath(V2ScenePath);
            if (!scene.IsValid())
            {
                scene = SceneManager.GetSceneByName(V2SceneName);
            }
            Assert.That(scene.IsValid(), Is.True);
            Assert.That(scene.isLoaded, Is.True);

            player = GetSceneComponents<PrototypeInteractionStateOwner>().Single().gameObject;
            locomotion = player.GetComponent<ThirdPersonLocomotion>();
            logicalInput = player.GetComponent<InputSystemLogicalInputAdapter>();
            playerController = player.GetComponent<CharacterController>();
            cameraRig = GetSceneComponents<ThirdPersonCameraRig>().Single();
            mobileAlly = GetSceneComponents<ShowcaseMobileAllyController>().Single();
            allyController = mobileAlly.GetComponent<CharacterController>();
            reset = GetSceneComponents<AuthoredEncounterResetController>().Single();
            v2Root = RequireRoot(V2RootName);
            staticColliders = v2Root.GetComponentsInChildren<Collider>(true)
                .Where(collider => collider.enabled && !collider.isTrigger &&
                                   !IsFloor(collider))
                .ToArray();

            Assert.That(locomotion, Is.Not.Null);
            Assert.That(logicalInput, Is.Not.Null);
            Assert.That(playerController, Is.Not.Null);
            Assert.That(cameraRig.OutputCamera, Is.Not.Null);
            Assert.That(allyController, Is.Not.Null);
            Assert.That(mobileAlly.AnchorCount, Is.EqualTo(18));
            Assert.That(mobileAlly.IsAnchorNetworkConnected(), Is.True);

            playerSpawn = player.transform.position;
            allySpawn = mobileAlly.FollowAnchors[0].position;
            allySpawn.y = mobileAlly.transform.position.y;
            QueueGamepadState(default);
            for (int frame = 0; frame < 6; frame++)
            {
                yield return null;
            }
            Physics.SyncTransforms();
            ValidatePlayableFrame("initial settle");
        }

        [UnityTearDown]
        public IEnumerator TearDownFixture()
        {
            QueueGamepadState(default);
            foreach (GameObject blocker in temporaryBlockers)
            {
                if (blocker != null)
                {
                    UnityEngine.Object.Destroy(blocker);
                }
            }
            temporaryBlockers.Clear();
            yield return null;

            if (scene.IsValid() && scene.isLoaded)
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(scene);
                while (unload != null && !unload.isDone)
                {
                    yield return null;
                }
            }

            if (gamepad != null && gamepad.added)
            {
                InputSystem.RemoveDevice(gamepad);
            }
            Time.captureDeltaTime = previousCaptureDeltaTime;
        }

        [UnityTest]
        public IEnumerator ActualCharacterControllerAndCamera_TraverseBothLoops_ResetAndRepeat()
        {
            RoutePoint[] route =
            {
                new RoutePoint("Entry center lane", 0f, -12f),
                new RoutePoint("West south connector", -12f, -8f),
                new RoutePoint("West outer south", -24f, -8f),
                new RoutePoint("West turn pocket", -26f, 3f),
                new RoutePoint("West outer north", -24f, 14f),
                new RoutePoint("West north connector", -12f, 15f),
                new RoutePoint("Central north junction", 0f, 15f),
                new RoutePoint("Central cut", 0f, 8f),
                new RoutePoint("Central bowl", 0f, -8f),
                new RoutePoint("East south connector", 12f, -8f),
                new RoutePoint("East outer south", 24f, -8f),
                new RoutePoint("East turn pocket", 26f, 3f),
                new RoutePoint("East outer north", 24f, 14f),
                new RoutePoint("North Drawer approach", 22f, 17f),
                new RoutePoint("North Cross", 16f, 17f),
                new RoutePoint("Exit Run", 26f, 19.5f)
            };

            Assert.That(mobileAlly.State,
                Is.EqualTo(ShowcaseMobileAllyState.HoldingPosition));
            Assert.That(mobileAlly.IsFollowCommanded, Is.False);
            Assert.That(mobileAlly.TryToggleFollow(), Is.True);
            observedCatchUp = false;
            yield return TraverseRoute(route, "first traversal");
            yield return WaitForAllyFollowBand("first traversal");
            Assert.That(observedCatchUp, Is.True,
                "The actual expanded route must exercise S2 catch-up at least once.");
            Assert.That(mobileAlly.MaximumObservedStepDistance, Is.LessThan(0.5f));

            Assert.That(
                reset.RequestReset(EncounterResetCause.ManualInput),
                Is.True,
                "Authored Reset must accept after the first traversal.");
            Physics.SyncTransforms();
            AssertPlanarNear(player.transform.position, playerSpawn, 0.01f, "Player synchronous Reset");
            AssertPlanarNear(mobileAlly.transform.position, allySpawn, 0.01f, "Ally synchronous Reset");
            QueueGamepadState(default);
            yield return null;
            yield return new WaitForFixedUpdate();
            yield return null;
            Physics.SyncTransforms();
            AssertPlanarNear(player.transform.position, playerSpawn, 0.05f, "Player Reset");
            Assert.That(PlanarDistance(mobileAlly.transform.position, allySpawn), Is.LessThan(0.2f));
            Assert.That(mobileAlly.FollowSide, Is.EqualTo(ShowcaseAllyFollowSide.Left));
            Assert.That(mobileAlly.State,
                Is.EqualTo(ShowcaseMobileAllyState.HoldingPosition));
            Assert.That(mobileAlly.IsFollowCommanded, Is.False);
            ValidatePlayableFrame("post-Reset settled frame");

            Assert.That(mobileAlly.TryToggleFollow(), Is.True);
            observedCatchUp = false;
            yield return TraverseRoute(route, "second traversal");
            yield return WaitForAllyFollowBand("second traversal");
            Assert.That(observedCatchUp, Is.True,
                "The identical post-Reset route must exercise S2 catch-up.");
            Assert.That(reset.ResetCount, Is.EqualTo(1));
            Assert.That(reset.UnexpectedBaselineDifferenceCount, Is.Zero);
            Assert.That(reset.ObjectDuplicationCount, Is.Zero);
            Assert.That(reset.ObjectLossCount, Is.Zero);
            Assert.That(
                GetSceneComponents<PrototypeInteractionTarget>()
                    .Select(target => target.RuntimeInstanceKey)
                    .Distinct(StringComparer.Ordinal)
                    .Count(),
                Is.EqualTo(7));
        }

        [UnityTest]
        public IEnumerator S2ExplicitRegroupSideSwitchBlockedRecoveryAndReset_WorkInActualV2Scene()
        {
            ShowcaseAllyFollowSide initialSide = mobileAlly.FollowSide;
            Assert.That(mobileAlly.State,
                Is.EqualTo(ShowcaseMobileAllyState.HoldingPosition));
            Assert.That(mobileAlly.IsFollowCommanded, Is.False);
            Assert.That(mobileAlly.TryToggleFollow(), Is.True);
            Assert.That(mobileAlly.IsFollowCommanded, Is.True);
            Assert.That(mobileAlly.FollowSide, Is.EqualTo(initialSide));
            Assert.That(mobileAlly.SideSwitchCount, Is.Zero);
            Assert.That(mobileAlly.TryRequestRegroup(), Is.True);
            Assert.That(mobileAlly.FollowSide, Is.Not.EqualTo(initialSide));
            Assert.That(mobileAlly.SideSwitchCount, Is.EqualTo(1));
            for (int frame = 0; frame < 20; frame++)
            {
                yield return new WaitForFixedUpdate();
                ValidatePlayableFrame("clear regroup");
            }

            Assert.That(reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
            Physics.SyncTransforms();
            AssertPlanarNear(mobileAlly.transform.position, allySpawn, 0.01f, "Ally synchronous Reset");
            yield return null;
            yield return new WaitForFixedUpdate();
            Assert.That(PlanarDistance(mobileAlly.transform.position, allySpawn), Is.LessThan(0.2f));
            Assert.That(mobileAlly.FollowSide, Is.EqualTo(ShowcaseAllyFollowSide.Left));
            Assert.That(mobileAlly.State,
                Is.EqualTo(ShowcaseMobileAllyState.HoldingPosition));
            Assert.That(mobileAlly.IsFollowCommanded, Is.False);

            CreateAllyEnclosure();
            Physics.SyncTransforms();
            for (int frame = 0; frame < 90; frame++)
            {
                yield return new WaitForFixedUpdate();
                ValidatePlayableFrame("blocked enclosure");
            }

            Assert.That(mobileAlly.State,
                Is.EqualTo(ShowcaseMobileAllyState.HoldingPosition));
            Assert.That(mobileAlly.BlockedCount, Is.Zero);
            Assert.That(mobileAlly.TryToggleFollow(), Is.False);
            Assert.That(mobileAlly.LastRegroupAccepted, Is.False);
            Assert.That(mobileAlly.IsFollowCommanded, Is.False);

            foreach (GameObject blocker in temporaryBlockers)
            {
                UnityEngine.Object.Destroy(blocker);
            }
            temporaryBlockers.Clear();
            yield return null;
            yield return new WaitForFixedUpdate();
            Physics.SyncTransforms();

            Assert.That(mobileAlly.TryRequestRegroup(), Is.True,
                "Regroup must recover after the objective test obstruction is removed.");
            Assert.That(mobileAlly.LastRegroupAccepted, Is.True);
            Assert.That(reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
            Physics.SyncTransforms();
            AssertPlanarNear(player.transform.position, playerSpawn, 0.01f, "Player synchronous Reset");
            AssertPlanarNear(mobileAlly.transform.position, allySpawn, 0.01f, "Ally synchronous Reset");
            yield return null;
            yield return new WaitForFixedUpdate();
            yield return null;
            ValidatePlayableFrame("blocked/regroup Reset");
            AssertPlanarNear(player.transform.position, playerSpawn, 0.05f, "Player Reset");
            Assert.That(PlanarDistance(mobileAlly.transform.position, allySpawn), Is.LessThan(0.2f));
            Assert.That(mobileAlly.RegroupAttemptSequence, Is.Zero);
            Assert.That(mobileAlly.SideSwitchCount, Is.Zero);
            Assert.That(mobileAlly.BlockedCount, Is.Zero);
        }

        private IEnumerator TraverseRoute(IEnumerable<RoutePoint> route, string passName)
        {
            foreach (RoutePoint point in route)
            {
                yield return DriveTo(point, passName);
            }
            QueueGamepadState(default);
            yield return null;
            yield return null;
            ValidatePlayableFrame(passName + " completion");
        }

        private IEnumerator DriveTo(RoutePoint point, string passName)
        {
            const float arrivalTolerance = 0.7f;
            const int maximumFrames = 900;
            const int stallWindow = 45;
            float checkpointDistance = PlanarDistance(player.transform.position, point.Position);
            Vector3 checkpointPosition = player.transform.position;
            float bestDistance = checkpointDistance;
            int framesWithoutProgress = 0;
            Vector2 lastStick = Vector2.zero;
            Vector3 lastCameraForward = Vector3.zero;
            Vector3 previousPlayerPosition = player.transform.position;
            diagnosticMaximumDeltaTime = 0f;
            diagnosticMaximumPlayerStep = 0f;
            diagnosticStartFrame = Time.frameCount;
            diagnosticStartRenderedFrame = Time.renderedFrameCount;

            for (int frame = 0; frame < maximumFrames; frame++)
            {
                float distance = PlanarDistance(player.transform.position, point.Position);
                if (distance <= arrivalTolerance)
                {
                    QueueGamepadState(default);
                    yield return null;
                    ValidatePlayableFrame($"{passName} / {point.Name}");
                    yield break;
                }

                Vector3 desired = point.Position - player.transform.position;
                desired.y = 0f;
                desired.Normalize();
                Transform view = cameraRig.OutputCamera.transform;
                Vector3 forward = Vector3.ProjectOnPlane(view.forward, Vector3.up).normalized;
                Vector3 right = Vector3.ProjectOnPlane(view.right, Vector3.up).normalized;
                Vector2 stick = new Vector2(
                    Vector3.Dot(desired, right),
                    Vector3.Dot(desired, forward));
                if (stick.sqrMagnitude > 1f)
                {
                    stick.Normalize();
                }
                float estimatedFrameStep = locomotion.MoveSpeed * Mathf.Max(Time.deltaTime, 0.001f);
                if (distance < estimatedFrameStep * 1.5f)
                {
                    float approachScale = Mathf.Clamp01(
                        distance / Mathf.Max(estimatedFrameStep * 1.25f, 0.01f));
                    stick *= approachScale;
                }
                lastStick = stick;
                lastCameraForward = forward;

                QueueGamepadState(new GamepadState { leftStick = stick });
                yield return null;
                Physics.SyncTransforms();
                diagnosticMaximumDeltaTime = Mathf.Max(
                    diagnosticMaximumDeltaTime,
                    Time.deltaTime);
                diagnosticMaximumPlayerStep = Mathf.Max(
                    diagnosticMaximumPlayerStep,
                    PlanarDistance(previousPlayerPosition, player.transform.position));
                previousPlayerPosition = player.transform.position;
                ValidatePlayableFrame($"{passName} / {point.Name} / frame {frame}");
                observedCatchUp |=
                    mobileAlly.State == ShowcaseMobileAllyState.CatchingUp ||
                    mobileAlly.State == ShowcaseMobileAllyState.FastCatchingUp;

                float updatedDistance = PlanarDistance(player.transform.position, point.Position);
                if (updatedDistance < bestDistance - 0.05f)
                {
                    bestDistance = updatedDistance;
                    framesWithoutProgress = 0;
                }
                else
                {
                    framesWithoutProgress++;
                }
                if (framesWithoutProgress >= 90)
                {
                    Assert.Fail(
                        $"{passName} made no goal progress at {point.Name} for " +
                        $"{framesWithoutProgress} frames; {BuildTraversalDiagnostic(point, lastStick, lastCameraForward)}");
                }

                if ((frame + 1) % stallWindow == 0)
                {
                    float currentDistance = PlanarDistance(player.transform.position, point.Position);
                    float checkpointMovement = PlanarDistance(
                        player.transform.position,
                        checkpointPosition);
                    Assert.That(
                        currentDistance < checkpointDistance - 0.1f || checkpointMovement > 0.25f,
                        Is.True,
                        $"{passName} stalled at {point.Name}: " +
                        $"distance {currentDistance:0.00} m, movement {checkpointMovement:0.00} m.");
                    checkpointDistance = currentDistance;
                    checkpointPosition = player.transform.position;
                }
            }

            float finalDistance = PlanarDistance(player.transform.position, point.Position);
            Assert.Fail(
                $"{passName} could not reach {point.Name} within {maximumFrames} frames; " +
                $"remaining={finalDistance:0.00} m; " +
                BuildTraversalDiagnostic(point, lastStick, lastCameraForward));
        }

        private IEnumerator WaitForAllyFollowBand(string passName)
        {
            QueueGamepadState(default);
            for (int frame = 0; frame < 600; frame++)
            {
                yield return new WaitForFixedUpdate();
                ValidatePlayableFrame(passName + " ally settle");
                float distance = PlanarDistance(
                    mobileAlly.transform.position,
                    mobileAlly.CalculateFollowPosition(mobileAlly.FollowSide));
                if (distance <= 0.75f)
                {
                    Assert.That(mobileAlly.State, Is.Not.EqualTo(ShowcaseMobileAllyState.Blocked));
                    yield break;
                }
            }

            Assert.Fail(passName + " ally did not return to the follow band.");
        }

        private void ValidatePlayableFrame(string context)
        {
            Transform camera = cameraRig.OutputCamera.transform;
            Assert.That(IsFinite(player.transform.position), Is.True, context + " player position");
            Assert.That(IsFinite(player.transform.rotation), Is.True, context + " player rotation");
            Assert.That(IsFinite(mobileAlly.transform.position), Is.True, context + " ally position");
            Assert.That(IsFinite(mobileAlly.transform.rotation), Is.True, context + " ally rotation");
            Assert.That(IsFinite(camera.position), Is.True, context + " camera position");
            Assert.That(IsFinite(camera.rotation), Is.True, context + " camera rotation");

            AssertInsideRoom(player.transform.position, 0.55f, context + " player");
            AssertInsideRoom(mobileAlly.transform.position, 0.55f, context + " ally");
            AssertInsideRoom(camera.position, 0.05f, context + " camera");

            float playerGroundSeparation = playerController.bounds.min.y;
            Assert.That(playerGroundSeparation, Is.InRange(-0.02f, 0.12f),
                context + " player floor separation");
            bool grounded = locomotion.IsGrounded || playerGroundSeparation <= 0.12f;
            Assert.That(grounded, Is.True, context + " player grounded");
            float allyGroundSeparation = allyController.bounds.min.y;
            Assert.That(allyGroundSeparation, Is.InRange(-0.02f, 0.12f),
                context + " ally floor separation");
            bool allyGrounded = allyController.isGrounded || allyGroundSeparation <= 0.12f;
            Assert.That(allyGrounded, Is.True, context + " ally grounded");

            AssertNoStaticPenetration(player.transform, context + " player");
            AssertNoStaticPenetration(mobileAlly.transform, context + " ally");
            AssertCameraClear(camera.position, context);
        }

        private void AssertNoStaticPenetration(Transform subject, string context)
        {
            Collider[] subjectColliders = subject.GetComponentsInChildren<Collider>(true)
                .Where(collider => collider.enabled && !collider.isTrigger)
                .ToArray();
            Assert.That(subjectColliders, Is.Not.Empty, context);
            foreach (Collider subjectCollider in subjectColliders)
            {
                foreach (Collider fixedCollider in staticColliders)
                {
                    if (!subjectCollider.bounds.Intersects(fixedCollider.bounds))
                    {
                        continue;
                    }

                    bool overlaps = Physics.ComputePenetration(
                        subjectCollider,
                        subjectCollider.transform.position,
                        subjectCollider.transform.rotation,
                        fixedCollider,
                        fixedCollider.transform.position,
                        fixedCollider.transform.rotation,
                        out _,
                        out float depth);
                    Assert.That(!overlaps || depth <= 0.002f, Is.True,
                        $"{context} penetrates {fixedCollider.name} by {depth:0.000} m.");
                }
            }
        }

        private void AssertCameraClear(Vector3 cameraPosition, string context)
        {
            const float requiredRadius = 0.22f;
            foreach (Collider fixedCollider in staticColliders)
            {
                Vector3 closest = fixedCollider.ClosestPoint(cameraPosition);
                float distance = Vector3.Distance(cameraPosition, closest);
                Assert.That(distance, Is.GreaterThanOrEqualTo(requiredRadius - 0.002f),
                    $"{context} camera clearance to {fixedCollider.name} is {distance:0.000} m.");
            }
        }

        private string DescribeNearbyColliders()
        {
            Bounds bounds = playerController.bounds;
            return string.Join(", ", Physics.OverlapBox(
                    bounds.center,
                    bounds.extents + new Vector3(0.35f, 0.2f, 0.35f),
                    player.transform.rotation,
                    ~0,
                    QueryTriggerInteraction.Ignore)
                .Where(collider => collider != null &&
                                   !collider.transform.IsChildOf(player.transform))
                .Select(collider =>
                    $"{collider.name}@({collider.bounds.center.x:0.0},{collider.bounds.center.z:0.0})")
                .Distinct());
        }

        private string BuildTraversalDiagnostic(
            RoutePoint point,
            Vector2 requestedStick,
            Vector3 cameraForward)
        {
            PrototypeInteractionStateOwner owner =
                player.GetComponent<PrototypeInteractionStateOwner>();
            Vector2 snapshot = logicalInput.Snapshot.Move;
            Vector2 device = gamepad != null && gamepad.added
                ? gamepad.leftStick.ReadValue()
                : Vector2.zero;
            return
                $"player=({player.transform.position.x:0.00}, {player.transform.position.z:0.00}), " +
                $"playerY={player.transform.position.y:0.000}, " +
                $"controllerBottom={playerController.bounds.min.y:0.000}, " +
                $"verticalVelocity={locomotion.VerticalVelocity:0.00}, " +
                $"goal=({point.Position.x:0.00}, {point.Position.z:0.00}), " +
                $"remaining={PlanarDistance(player.transform.position, point.Position):0.00} m, " +
                $"requested=({requestedStick.x:0.00}, {requestedStick.y:0.00}), " +
                $"device=({device.x:0.00}, {device.y:0.00}), " +
                $"snapshot=({snapshot.x:0.00}, {snapshot.y:0.00}), " +
                $"cameraForward=({cameraForward.x:0.00}, {cameraForward.z:0.00}), " +
                $"maxDeltaTime={diagnosticMaximumDeltaTime:0.0000}, " +
                $"maxPlayerStep={diagnosticMaximumPlayerStep:0.00} m, " +
                $"playerLoopFrames={Time.frameCount - diagnosticStartFrame}, " +
                $"renderedFrames={Time.renderedFrameCount - diagnosticStartRenderedFrame}, " +
                $"deltaTime={Time.deltaTime:0.000000}, unscaledDeltaTime={Time.unscaledDeltaTime:0.000000}, " +
                $"timeScale={Time.timeScale:0.00}, active={player.activeInHierarchy}, " +
                $"locomotionEnabled={locomotion.enabled}, controllerEnabled={playerController.enabled}, " +
                $"globalState={owner.GlobalState}, nearby=[{DescribeNearbyColliders()}].";
        }

        private void AssertInsideRoom(Vector3 position, float margin, string context)
        {
            Assert.That(position.x, Is.InRange(-32f + margin, 32f - margin), context + " X");
            Assert.That(position.z, Is.InRange(-22f + margin, 22f - margin), context + " Z");
        }

        private void CreateAllyEnclosure()
        {
            Vector3 center = mobileAlly.transform.position;
            CreateTemporaryBlocker("TestBlocker_North",
                center + new Vector3(0f, 1.5f, 1.15f), new Vector3(3.6f, 3f, 0.2f));
            CreateTemporaryBlocker("TestBlocker_South",
                center + new Vector3(0f, 1.5f, -1.15f), new Vector3(3.6f, 3f, 0.2f));
            CreateTemporaryBlocker("TestBlocker_West",
                center + new Vector3(-1.15f, 1.5f, 0f), new Vector3(0.2f, 3f, 2.5f));
            CreateTemporaryBlocker("TestBlocker_East",
                center + new Vector3(1.15f, 1.5f, 0f), new Vector3(0.2f, 3f, 2.5f));
        }

        private void CreateTemporaryBlocker(string name, Vector3 position, Vector3 scale)
        {
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = name;
            blocker.transform.SetPositionAndRotation(position, Quaternion.identity);
            blocker.transform.localScale = scale;
            SceneManager.MoveGameObjectToScene(blocker, scene);
            temporaryBlockers.Add(blocker);
        }

        private void QueueGamepadState(GamepadState state)
        {
            if (gamepad == null || !gamepad.added)
            {
                return;
            }
            InputSystem.QueueStateEvent(gamepad, state);
            InputSystem.Update();
        }

        private T[] GetSceneComponents<T>() where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }

        private Transform RequireRoot(string name)
        {
            GameObject root = scene.GetRootGameObjects()
                .SingleOrDefault(candidate => string.Equals(
                    candidate.name,
                    name,
                    StringComparison.Ordinal));
            Assert.That(root, Is.Not.Null, name);
            return root.transform;
        }

        private static bool IsFloor(Collider collider)
        {
            return string.Equals(collider.name, "PROXY_Floor_64x44", StringComparison.Ordinal);
        }

        private static void AssertPlanarNear(
            Vector3 actual,
            Vector3 expected,
            float tolerance,
            string context)
        {
            Assert.That(PlanarDistance(actual, expected), Is.LessThanOrEqualTo(tolerance), context);
        }

        private static float PlanarDistance(Vector3 left, Vector3 right)
        {
            return Vector2.Distance(
                new Vector2(left.x, left.z),
                new Vector2(right.x, right.z));
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

        private readonly struct RoutePoint
        {
            public RoutePoint(string name, float x, float z)
            {
                Name = name;
                Position = new Vector3(x, 1f, z);
            }

            public string Name { get; }

            public Vector3 Position { get; }
        }
    }
}
