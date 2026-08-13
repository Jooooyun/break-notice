using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public sealed class ContestFinalStageTraversalPlayModeTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_NorthOffice_Stage01_v001.unity";
        private const string SceneName = "Contest_NorthOffice_Stage01_v001";
        private const string MainRootName = "FINAL_NORTH_OFFICE_STAGE_01";

        private Scene scene;
        private GameObject player;
        private CharacterController playerController;
        private ThirdPersonCameraRig cameraRig;
        private ShowcaseMobileAllyController mobileAlly;
        private CharacterController allyController;
        private AuthoredEncounterResetController reset;
        private Collider[] staticColliders;
        private Vector3 playerSpawn;
        private Vector3 allySpawn;
        private float previousCaptureDeltaTime;

        [UnitySetUp]
        public IEnumerator SetUpFixture()
        {
            previousCaptureDeltaTime = Time.captureDeltaTime;
            Time.captureDeltaTime = 1f / 60f;
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
            Assert.That(load, Is.Not.Null);
            while (!load.isDone)
            {
                yield return null;
            }

            scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.IsValid())
            {
                scene = SceneManager.GetSceneByName(SceneName);
            }
            Assert.That(scene.IsValid(), Is.True);
            Assert.That(scene.isLoaded, Is.True);

            player = SceneComponents<PrototypeInteractionStateOwner>().Single().gameObject;
            playerController = player.GetComponent<CharacterController>();
            cameraRig = SceneComponents<ThirdPersonCameraRig>().Single();
            mobileAlly = SceneComponents<ShowcaseMobileAllyController>().Single();
            allyController = mobileAlly.GetComponent<CharacterController>();
            reset = SceneComponents<AuthoredEncounterResetController>().Single();
            Transform root = RequireRoot(MainRootName);
            staticColliders = root.GetComponentsInChildren<Collider>(true)
                .Where(collider => collider.enabled &&
                                   !collider.isTrigger &&
                                   collider.name != "STATIC_FLOOR_48X34")
                .ToArray();

            foreach (ShowcaseRunnerPressureController pressure in
                     SceneComponents<ShowcaseRunnerPressureController>())
            {
                pressure.enabled = false;
            }
            foreach (ShowcaseRunnerAgent runner in SceneComponents<ShowcaseRunnerAgent>())
            {
                runner.enabled = false;
            }
            foreach (PrototypeInteractionTarget target in
                     SceneComponents<PrototypeInteractionTarget>())
            {
                Rigidbody body = target.GetComponent<Rigidbody>();
                if (body != null)
                {
                    body.isKinematic = true;
                    body.useGravity = false;
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
                target.GetComponent<Collider>().enabled = false;
            }

            Assert.That(playerController, Is.Not.Null);
            Assert.That(cameraRig.OutputCamera, Is.Not.Null);
            Assert.That(allyController, Is.Not.Null);
            Assert.That(mobileAlly.AnchorCount, Is.EqualTo(22));
            Assert.That(mobileAlly.IsAnchorNetworkConnected(), Is.True);
            Assert.That(reset.ExpectedObjectCount, Is.EqualTo(10));
            Assert.That(reset.BaselineObjectCount, Is.EqualTo(10));

            playerSpawn = player.transform.position;
            allySpawn = mobileAlly.transform.position;
            for (int frame = 0; frame < 12; frame++)
            {
                yield return null;
            }
            Physics.SyncTransforms();
            ValidatePlayableFrame("initial settle");
        }

        [UnityTearDown]
        public IEnumerator TearDownFixture()
        {
            if (scene.IsValid() && scene.isLoaded)
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(scene);
                while (unload != null && !unload.isDone)
                {
                    yield return null;
                }
            }
            Time.captureDeltaTime = previousCaptureDeltaTime;
        }

        [UnityTest]
        public IEnumerator CharacterControllerCameraAndAlly_TraverseBothLoops_ResetAndRepeat()
        {
            RoutePoint[] route =
            {
                new RoutePoint("Conference south", 0f, -12f),
                new RoutePoint("Conference north gate clear", 3.5f, -6.5f),
                new RoutePoint("Central transition", 0f, -5f),
                new RoutePoint("Central south connector", 0f, -2.5f),
                new RoutePoint("West south connector", -11.5f, -2.5f),
                new RoutePoint("West forced encounter", -18.5f, -3f),
                new RoutePoint("West north turn", -19f, 6.5f),
                new RoutePoint("Key room entry", -19f, 9.5f),
                new RoutePoint("Required Key 01", -19f, 13f),
                new RoutePoint("Return west north", -19f, 6.5f),
                new RoutePoint("Zone 01 south clear", -13.2f, 7f),
                new RoutePoint("Security west", -11f, 7f),
                new RoutePoint("Security center", 0f, 7f),
                new RoutePoint("Central cross", 0f, 2f),
                new RoutePoint("East connector", 11f, 2f),
                new RoutePoint("East south connector", 11.5f, -2.5f),
                new RoutePoint("East forced encounter", 18.5f, -3f),
                new RoutePoint("East north turn", 19f, 7.5f),
                new RoutePoint("Security east", 11f, 7f),
                new RoutePoint("Exit south clear", 17.5f, 8.5f),
                new RoutePoint("Exit room entry", 19.5f, 10.8f),
                new RoutePoint("Final Exit 03", 20f, 14f)
            };

            Assert.That(mobileAlly.TryToggleFollow(), Is.True);
            yield return TraverseRoute(route, "first traversal");
            Assert.That(mobileAlly.IsFollowCommanded, Is.True);
            Assert.That(mobileAlly.MaximumObservedStepDistance, Is.LessThan(0.5f));

            Assert.That(
                reset.RequestReset(EncounterResetCause.ManualInput),
                Is.True,
                "Final-stage authored Reset must accept after the first traversal.");
            Physics.SyncTransforms();
            AssertPlanarNear(player.transform.position, playerSpawn, 0.02f, "Player synchronous Reset");
            AssertPlanarNear(mobileAlly.transform.position, allySpawn, 0.08f, "Ally synchronous Reset");
            yield return null;
            yield return new WaitForFixedUpdate();
            yield return null;
            Physics.SyncTransforms();
            AssertPlanarNear(player.transform.position, playerSpawn, 0.06f, "Player settled Reset");
            AssertPlanarNear(mobileAlly.transform.position, allySpawn, 0.25f, "Ally settled Reset");
            ValidatePlayableFrame("post Reset settle");

            Assert.That(mobileAlly.TryToggleFollow(), Is.True);
            yield return TraverseRoute(route, "second traversal");
            Assert.That(reset.ResetCount, Is.EqualTo(1));
            Assert.That(reset.UnexpectedBaselineDifferenceCount, Is.Zero);
            Assert.That(reset.ObjectDuplicationCount, Is.Zero);
            Assert.That(reset.ObjectLossCount, Is.Zero);
            Assert.That(reset.OwnershipLeakCount, Is.Zero);
        }

        private IEnumerator TraverseRoute(IEnumerable<RoutePoint> route, string pass)
        {
            foreach (RoutePoint point in route)
            {
                yield return DriveTo(point, pass);
            }
            for (int frame = 0; frame < 3; frame++)
            {
                yield return null;
                ValidatePlayableFrame(pass + " completion settle");
            }
        }

        private IEnumerator DriveTo(RoutePoint point, string pass)
        {
            const float arrivalTolerance = 0.48f;
            const float moveSpeed = 4.2f;
            const int maximumFrames = 1200;
            const int stallWindow = 90;
            float bestDistance = PlanarDistance(player.transform.position, point.Position);
            int stalledFrames = 0;

            for (int frame = 0; frame < maximumFrames; frame++)
            {
                float distance = PlanarDistance(player.transform.position, point.Position);
                if (distance <= arrivalTolerance)
                {
                    yield return null;
                    ValidatePlayableFrame($"{pass} / {point.Name}");
                    yield break;
                }

                Vector3 direction = point.Position - player.transform.position;
                direction.y = 0f;
                direction.Normalize();
                float step = Mathf.Min(moveSpeed * Mathf.Max(Time.deltaTime, 0.001f), distance);
                CollisionFlags flags = playerController.Move(
                    direction * step + Vector3.down * 0.05f);
                Assert.That((flags & CollisionFlags.Sides) != 0 && stalledFrames > 12,
                    Is.False,
                    $"{pass} / {point.Name} is pressing a static side collider.");

                yield return null;
                Physics.SyncTransforms();
                ValidatePlayableFrame($"{pass} / {point.Name} / frame {frame}");

                float newDistance = PlanarDistance(player.transform.position, point.Position);
                if (newDistance < bestDistance - 0.01f)
                {
                    bestDistance = newDistance;
                    stalledFrames = 0;
                }
                else
                {
                    stalledFrames++;
                }
                Assert.That(stalledFrames, Is.LessThan(stallWindow),
                    $"{pass} stalled approaching {point.Name}; distance {newDistance:F3}m.");
            }

            Assert.Fail($"{pass} exceeded frame budget approaching {point.Name}.");
        }

        private void ValidatePlayableFrame(string context)
        {
            AssertFinite(player.transform.position, context + " player");
            AssertFinite(mobileAlly.transform.position, context + " ally");
            Camera camera = cameraRig.OutputCamera;
            AssertFinite(camera.transform.position, context + " camera");
            Assert.That(player.transform.position.x, Is.InRange(-23.6f, 23.6f), context);
            Assert.That(player.transform.position.z, Is.InRange(-16.6f, 16.6f), context);
            Assert.That(mobileAlly.transform.position.x, Is.InRange(-23.6f, 23.6f), context);
            Assert.That(mobileAlly.transform.position.z, Is.InRange(-16.6f, 16.6f), context);
            Assert.That(playerController.isGrounded, Is.True, context + " player grounded");

            AssertNoStaticPenetration(playerController, context + " player");
            AssertNoStaticPenetration(allyController, context + " ally");

            Collider[] cameraOverlaps = Physics.OverlapSphere(
                    camera.transform.position,
                    0.08f,
                    ~0,
                    QueryTriggerInteraction.Ignore)
                .Where(collider => staticColliders.Contains(collider))
                .ToArray();
            Assert.That(cameraOverlaps, Is.Empty,
                context + $" player={player.transform.position} camera={camera.transform.position} " +
                "camera penetrates " +
                string.Join(", ", cameraOverlaps.Select(collider => collider.name)));
        }

        private void AssertNoStaticPenetration(Collider actor, string context)
        {
            foreach (Collider authoredStatic in staticColliders)
            {
                bool penetrates = Physics.ComputePenetration(
                    actor,
                    actor.transform.position,
                    actor.transform.rotation,
                    authoredStatic,
                    authoredStatic.transform.position,
                    authoredStatic.transform.rotation,
                    out _,
                    out float depth);
                Assert.That(penetrates && depth > 0.002f, Is.False,
                    $"{context} penetrates {authoredStatic.name} by {depth:F4}m.");
            }
        }

        private Transform RequireRoot(string name)
        {
            Transform root = scene.GetRootGameObjects()
                .Select(gameObject => gameObject.transform)
                .SingleOrDefault(transform => string.Equals(
                    transform.name,
                    name,
                    StringComparison.Ordinal));
            Assert.That(root, Is.Not.Null, $"Missing root {name}");
            return root;
        }

        private T[] SceneComponents<T>() where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }

        private static float PlanarDistance(Vector3 a, Vector3 b)
        {
            return Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
        }

        private static void AssertPlanarNear(
            Vector3 actual,
            Vector3 expected,
            float tolerance,
            string context)
        {
            Assert.That(PlanarDistance(actual, expected), Is.LessThanOrEqualTo(tolerance),
                $"{context}: expected {expected}, actual {actual}");
        }

        private static void AssertFinite(Vector3 value, string context)
        {
            Assert.That(float.IsNaN(value.x) || float.IsInfinity(value.x), Is.False, context);
            Assert.That(float.IsNaN(value.y) || float.IsInfinity(value.y), Is.False, context);
            Assert.That(float.IsNaN(value.z) || float.IsInfinity(value.z), Is.False, context);
        }

        private readonly struct RoutePoint
        {
            public RoutePoint(string name, float x, float z)
            {
                Name = name;
                Position = new Vector3(x, 0f, z);
            }
            public string Name { get; }
            public Vector3 Position { get; }
        }
    }
}
