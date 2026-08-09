using System.Collections;
using System.Linq;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestCarryThrowPlayModeTests
    {
        [UnityTest]
        public IEnumerator AcquireLight_MovesContinuouslyToVisibleCarryAnchor()
        {
            CarryFixture fixture = new CarryFixture(PrototypeWeightClass.Light);
            try
            {
                Vector3 initial = fixture.Target.transform.position;
                Assert.That(fixture.Controller.ProcessInteractPrimary(false).IsAccepted, Is.True);
                Assert.That(fixture.Target.transform.position, Is.EqualTo(initial));
                Assert.That(
                    fixture.Target.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Ignore Raycast")));

                yield return new WaitForFixedUpdate();
                Vector3 afterFirstStep = fixture.Target.transform.position;
                Assert.That(afterFirstStep, Is.Not.EqualTo(initial));
                Assert.That(afterFirstStep, Is.Not.EqualTo(fixture.Anchor.position));

                for (int step = 0; step < 20; step++)
                {
                    yield return new WaitForFixedUpdate();
                }

                Assert.That(fixture.Lifecycle.AcquisitionPathLength, Is.GreaterThan(0.1f));
                Assert.That(fixture.Target.gameObject.activeInHierarchy, Is.True);
                Assert.That(fixture.Target.GetComponent<Renderer>().enabled, Is.True);
                Assert.That(fixture.Owner.CarriedTarget, Is.SameAs(fixture.Target));
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator CarryCameraCollisionLayer_RestoresExactlyOnThrowAndReset()
        {
            CarryFixture fixture = new CarryFixture(PrototypeWeightClass.Medium);
            try
            {
                int originalLayer = fixture.Target.gameObject.layer;
                Assert.That(fixture.Controller.ProcessInteractPrimary(false).IsAccepted, Is.True);
                Assert.That(
                    fixture.Target.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Ignore Raycast")));

                yield return new WaitForFixedUpdate();
                Assert.That(fixture.Controller.ProcessThrow(), Is.True);
                Assert.That(fixture.Target.gameObject.layer, Is.EqualTo(originalLayer));

                Rigidbody body = fixture.Target.GetComponent<Rigidbody>();
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                fixture.Target.transform.position = new Vector3(0f, 1f, 1.6f);
                Physics.SyncTransforms();

                Assert.That(fixture.Controller.ProcessInteractPrimary(false).IsAccepted, Is.True);
                Assert.That(
                    fixture.Target.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Ignore Raycast")));

                fixture.Lifecycle.DetachForReset();
                Assert.That(fixture.Target.gameObject.layer, Is.EqualTo(originalLayer));
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator CarryBinderAgainstSolidWall_StopsBeforeWallWithoutOwnershipLoss()
        {
            yield return AssertCarryStopsBeforeSolidWall(
                PrototypeWeightClass.Light,
                new Vector3(0.6f, 0.6f, 0.25f));
        }

        [UnityTest]
        public IEnumerator CarryChairAgainstSolidWall_StopsBeforeWallWithoutOwnershipLoss()
        {
            yield return AssertCarryStopsBeforeSolidWall(
                PrototypeWeightClass.Medium,
                new Vector3(1.2f, 1.3f, 1.2f));
        }

        [UnityTest]
        public IEnumerator ThrowRelease_ProducesPlayerOwnedTrajectoryAndPhysicalCollision()
        {
            CarryFixture fixture = new CarryFixture(PrototypeWeightClass.Light);
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = new Vector3(0f, 1f, 3.2f);
            wall.transform.localScale = new Vector3(4f, 2f, 0.3f);
            try
            {
                Assert.That(fixture.Controller.ProcessInteractPrimary(false).IsAccepted, Is.True);
                for (int step = 0; step < 16; step++)
                {
                    yield return new WaitForFixedUpdate();
                }

                Vector3 releasePosition = fixture.Target.transform.position;
                Assert.That(fixture.Controller.ProcessThrow(), Is.True);
                Assert.That(fixture.Owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
                Assert.That(fixture.Lifecycle.LastThrowOrigin.IsPlayerOwned, Is.True);
                Assert.That(fixture.Target.GetComponent<Rigidbody>().isKinematic, Is.False);

                for (int step = 0; step < 25; step++)
                {
                    yield return new WaitForFixedUpdate();
                }

                Assert.That(fixture.Target.transform.position, Is.Not.EqualTo(releasePosition));
                Assert.That(fixture.Target.transform.position.z, Is.LessThan(3.7f));
                Assert.That(fixture.Target.gameObject.activeInHierarchy, Is.True);
                Assert.That(fixture.Lifecycle.ThrowOriginCount, Is.EqualTo(1));
            }
            finally
            {
                fixture.Dispose();
                Object.Destroy(wall);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator RepeatedAcquisition_ReusesSameObjectWithoutDuplication()
        {
            CarryFixture fixture = new CarryFixture(PrototypeWeightClass.Medium);
            try
            {
                int instanceId = fixture.Target.gameObject.GetInstanceID();
                Assert.That(fixture.Controller.ProcessInteractPrimary(false).IsAccepted, Is.True);
                yield return new WaitForFixedUpdate();
                Assert.That(fixture.Controller.ProcessThrow(), Is.True);
                yield return new WaitForFixedUpdate();

                Rigidbody body = fixture.Target.GetComponent<Rigidbody>();
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                fixture.Target.transform.position = new Vector3(0f, 1f, 1.6f);
                Physics.SyncTransforms();

                Assert.That(fixture.Controller.ProcessInteractPrimary(false).IsAccepted, Is.True);
                Assert.That(fixture.Owner.CarriedTarget.gameObject.GetInstanceID(), Is.EqualTo(instanceId));
                Assert.That(
                    Object.FindObjectsByType<PrototypeInteractionTarget>(FindObjectsSortMode.None)
                        .Count(target => target.RuntimeInstanceKey == fixture.Target.RuntimeInstanceKey),
                    Is.EqualTo(1));
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ContestScene_HasOnlyBinderAndChairDirectPhysicalCandidates()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "Contest_SingleRoomGraybox",
                LoadSceneMode.Additive);
            yield return load;
            Scene scene = SceneManager.GetSceneByName("Contest_SingleRoomGraybox");

            PrototypeInteractionTarget[] targets = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<PrototypeInteractionTarget>(true))
                .ToArray();
            PrototypeInteractionTarget[] direct = targets
                .Where(target => target.DirectAcquisitionEligible)
                .ToArray();

            Assert.That(direct, Has.Length.EqualTo(2));
            Assert.That(direct.Select(target => target.Role), Is.EquivalentTo(new[]
            {
                PrototypeInteractionRole.DirectBinder,
                PrototypeInteractionRole.DirectChair
            }));
            Assert.That(direct.All(target => target.GetComponent<Rigidbody>() != null), Is.True);
            Assert.That(targets.Single(target => target.Role == PrototypeInteractionRole.Drawer).GetComponent<Rigidbody>(), Is.Null);
            Assert.That(targets.Single(target => target.Role == PrototypeInteractionRole.Copier).GetComponent<Rigidbody>(), Is.Null);

            AsyncOperation unload = SceneManager.UnloadSceneAsync(scene);
            yield return unload;
        }

        private static IEnumerator AssertCarryStopsBeforeSolidWall(
            PrototypeWeightClass weightClass,
            Vector3 targetScale)
        {
            CarryFixture fixture = new CarryFixture(weightClass);
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = $"CarryBlockingWall-{weightClass}";
            wall.transform.position = new Vector3(0f, 1f, 2.65f);
            wall.transform.localScale = new Vector3(8f, 2f, 0.2f);
            fixture.Target.transform.localScale = targetScale;
            Physics.SyncTransforms();

            try
            {
                int originalLayer = fixture.Target.gameObject.layer;
                Assert.That(fixture.Controller.ProcessInteractPrimary(false).IsAccepted, Is.True);
                for (int step = 0; step < 24; step++)
                {
                    yield return new WaitForFixedUpdate();
                }

                Collider targetCollider = fixture.Target.GetComponent<Collider>();
                Collider wallCollider = wall.GetComponent<Collider>();
                Assert.That(
                    Physics.GetIgnoreCollision(fixture.PlayerController, targetCollider),
                    Is.False,
                    $"{weightClass} carry must restore player collision after acquisition clears the player.");
                for (int step = 0; step < 36; step++)
                {
                    fixture.PlayerController.Move(Vector3.forward * 0.08f);
                    yield return new WaitForFixedUpdate();
                    AssertCarryRemainsOnNearSide(fixture, wallCollider, weightClass);
                }

                float clearance = wallCollider.bounds.min.z - targetCollider.bounds.max.z;
                Assert.That(clearance, Is.GreaterThanOrEqualTo(-0.001f));
                Assert.That(clearance, Is.LessThanOrEqualTo(0.05f));

                float beforeSlideX = fixture.Target.transform.position.x;
                fixture.PlayerTransform.rotation = Quaternion.Euler(0f, 30f, 0f);
                for (int step = 0; step < 24; step++)
                {
                    fixture.PlayerController.Move(Vector3.right * 0.08f);
                    yield return new WaitForFixedUpdate();
                    AssertCarryRemainsOnNearSide(fixture, wallCollider, weightClass);
                }

                Assert.That(
                    fixture.Target.transform.position.x,
                    Is.GreaterThan(beforeSlideX + 0.5f),
                    $"{weightClass} carry must keep moving along a blocking wall.");
                Assert.That(fixture.Controller.ProcessThrow(), Is.True);
                Assert.That(fixture.Owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
                Assert.That(fixture.Owner.CarriedTarget, Is.Null);
                Assert.That(fixture.Lifecycle.CarriedTarget, Is.Null);
                Assert.That(fixture.Target.gameObject.layer, Is.EqualTo(originalLayer));
                Assert.That(fixture.Target.GetComponent<Rigidbody>().isKinematic, Is.False);
                Assert.That(fixture.Target.GetComponent<Rigidbody>().useGravity, Is.True);
                Assert.That(fixture.Lifecycle.LastThrowOrigin.IsPlayerOwned, Is.True);
                Assert.That(fixture.Lifecycle.ThrowOriginCount, Is.EqualTo(1));
                Assert.That(
                    Physics.GetIgnoreCollision(fixture.PlayerController, targetCollider),
                    Is.False);
            }
            finally
            {
                fixture.Dispose();
                Object.Destroy(wall);
            }

            yield return null;
        }

        private static void AssertCarryRemainsOnNearSide(
            CarryFixture fixture,
            Collider wallCollider,
            PrototypeWeightClass weightClass)
        {
            Collider targetCollider = fixture.Target.GetComponent<Collider>();
            bool penetrates = Physics.ComputePenetration(
                targetCollider,
                fixture.Target.transform.position,
                fixture.Target.transform.rotation,
                wallCollider,
                wallCollider.transform.position,
                wallCollider.transform.rotation,
                out _,
                out float penetrationDistance);

            Assert.That(
                penetrates && penetrationDistance > 0.001f,
                Is.False,
                $"{weightClass} carry penetration depth was {penetrationDistance}.");
            Assert.That(
                targetCollider.bounds.max.z,
                Is.LessThanOrEqualTo(wallCollider.bounds.min.z + 0.001f),
                $"{weightClass} carry crossed to the far side of the wall.");
            Assert.That(fixture.Owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Carrying));
            Assert.That(fixture.Owner.CarriedTarget, Is.SameAs(fixture.Target));
            Assert.That(fixture.Lifecycle.CarriedTarget, Is.SameAs(fixture.Target));
            Assert.That(fixture.Target.gameObject.layer, Is.EqualTo(LayerMask.NameToLayer("Ignore Raycast")));
            Assert.That(targetCollider.enabled, Is.True);
            Assert.That(fixture.Target.gameObject.activeInHierarchy, Is.True);

            bool overlapsPlayer = Physics.ComputePenetration(
                targetCollider,
                fixture.Target.transform.position,
                fixture.Target.transform.rotation,
                fixture.PlayerController,
                fixture.PlayerController.transform.position,
                fixture.PlayerController.transform.rotation,
                out _,
                out float playerPenetrationDistance);
            Assert.That(
                overlapsPlayer && playerPenetrationDistance > 0.001f,
                Is.False,
                $"{weightClass} carry penetrated the player by {playerPenetrationDistance}.");
        }

        private sealed class CarryFixture
        {
            private readonly GameObject player;
            private readonly GameObject targetObject;

            public CarryFixture(PrototypeWeightClass weightClass)
            {
                player = new GameObject("Player");
                PlayerController = player.AddComponent<CharacterController>();
                Owner = player.AddComponent<PrototypeInteractionStateOwner>();
                Lifecycle = player.AddComponent<PrototypePhysicalObjectLifecycle>();
                Controller = player.AddComponent<PrototypeInteractionController>();

                Anchor = new GameObject("CarryAnchor").transform;
                Anchor.SetParent(player.transform, false);
                Anchor.localPosition = new Vector3(0f, 1f, 1.15f);
                Lifecycle.Configure(Owner, Anchor);

                targetObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                targetObject.transform.position = new Vector3(0f, 1f, 1.8f);
                Rigidbody body = targetObject.AddComponent<Rigidbody>();
                body.useGravity = false;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                Target = targetObject.AddComponent<PrototypeInteractionTarget>();
                PrototypeInteractionRole role = weightClass == PrototypeWeightClass.Light
                    ? PrototypeInteractionRole.DirectBinder
                    : PrototypeInteractionRole.DirectChair;
                Target.Configure(
                    weightClass == PrototypeWeightClass.Light ? "OBJ-P-L-001" : "OBJ-P-M-001",
                    $"{role}-TEST",
                    role.ToString(),
                    weightClass,
                    role,
                    true,
                    false,
                    weightClass == PrototypeWeightClass.Medium,
                    weightClass == PrototypeWeightClass.Medium);

                Controller.Configure(
                    null,
                    Owner,
                    null,
                    new[] { Target },
                    2.8f,
                    false,
                    Lifecycle);
                Physics.SyncTransforms();
            }

            public PrototypeInteractionStateOwner Owner { get; }
            public PrototypePhysicalObjectLifecycle Lifecycle { get; }
            public PrototypeInteractionController Controller { get; }
            public PrototypeInteractionTarget Target { get; }
            public Transform Anchor { get; }
            public Transform PlayerTransform => player.transform;
            public CharacterController PlayerController { get; }

            public void Dispose()
            {
                Object.Destroy(player);
                Object.Destroy(targetObject);
            }
        }
    }
}
