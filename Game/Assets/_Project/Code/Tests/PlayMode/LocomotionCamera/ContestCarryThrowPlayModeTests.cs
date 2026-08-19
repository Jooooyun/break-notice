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
                Collider targetCollider = fixture.Target.GetComponent<Collider>();
                Assert.That(
                    Physics.GetIgnoreCollision(fixture.PlayerController, targetCollider),
                    Is.True,
                    "Player/carried collision ownership must remain ignored for the full carry.");
                Assert.That(fixture.Controller.ProcessThrow(), Is.True);
                Assert.That(fixture.Target.gameObject.layer, Is.EqualTo(originalLayer));
                Assert.That(
                    Physics.GetIgnoreCollision(fixture.PlayerController, targetCollider),
                    Is.True,
                    "Physical release must enter pending restore instead of restoring immediately.");
                yield return WaitForCollisionRestored(
                    fixture.PlayerController,
                    targetCollider,
                    "Collision must restore after two consecutive clear FixedUpdates.");

                Rigidbody body = fixture.Target.GetComponent<Rigidbody>();
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                fixture.Target.transform.position = new Vector3(0f, 1f, 1.6f);
                Physics.SyncTransforms();

                Assert.That(fixture.Controller.ProcessInteractPrimary(false).IsAccepted, Is.True);
                Assert.That(
                    fixture.Target.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Ignore Raycast")));

                Assert.That(fixture.Controller.BeginThrowCharge(), Is.True);
                Assert.That(fixture.Controller.HasThrowCharge, Is.True);
                fixture.Lifecycle.DetachForReset();
                Assert.That(fixture.Target.gameObject.layer, Is.EqualTo(originalLayer));
                yield return null;
                Assert.That(fixture.Controller.HasThrowCharge, Is.False);
                Assert.That(
                    Physics.GetIgnoreCollision(fixture.PlayerController, targetCollider),
                    Is.True,
                    "Reset detach must not restore collision before authored relocation.");
                fixture.Target.transform.position = new Vector3(0f, 1f, 3f);
                Physics.SyncTransforms();
                yield return WaitForCollisionRestored(
                    fixture.PlayerController,
                    targetCollider,
                    "Reset detach collision lease did not restore after authored relocation.");
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
        public IEnumerator CarryCollisionLease_IgnoresEveryHierarchyPairUntilClearDetach()
        {
            CarryFixture fixture = new CarryFixture(PrototypeWeightClass.Medium);
            GameObject playerChild = new GameObject("PlayerChildCollider");
            playerChild.transform.SetParent(fixture.PlayerTransform, false);
            playerChild.transform.localPosition = new Vector3(0f, 1f, 0.15f);
            BoxCollider playerChildCollider = playerChild.AddComponent<BoxCollider>();
            playerChildCollider.size = new Vector3(0.35f, 0.35f, 0.35f);
            GameObject targetChild = new GameObject("TargetChildCollider");
            targetChild.transform.SetParent(fixture.Target.transform, false);
            targetChild.transform.localPosition = new Vector3(0.2f, 0f, 0f);
            BoxCollider targetChildCollider = targetChild.AddComponent<BoxCollider>();
            targetChildCollider.size = new Vector3(0.2f, 0.2f, 0.2f);
            Physics.SyncTransforms();

            try
            {
                Assert.That(fixture.Controller.ProcessInteractPrimary(false).IsAccepted, Is.True);
                Collider[] playerColliders =
                    fixture.PlayerTransform.GetComponentsInChildren<Collider>(true);
                Collider[] targetColliders =
                    fixture.Target.GetComponentsInChildren<Collider>(true);
                for (int cycle = 0; cycle < 20; cycle++)
                {
                    fixture.PlayerTransform.rotation = Quaternion.Euler(
                        0f,
                        cycle % 2 == 0 ? 90f : -90f,
                        0f);
                    yield return new WaitForFixedUpdate();
                    AssertAllPairsIgnored(playerColliders, targetColliders, true);
                    Assert.That(fixture.Lifecycle.GripWorldBlocked, Is.False,
                        "Player/carried self geometry must never become a world grip block.");
                }

                Assert.That(fixture.Controller.ProcessThrow(), Is.True);
                AssertAllPairsIgnored(playerColliders, targetColliders, true);
                Rigidbody body = fixture.Target.GetComponent<Rigidbody>();
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.position = fixture.PlayerTransform.position + Vector3.forward * 4f;
                Physics.SyncTransforms();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                AssertAllPairsIgnored(playerColliders, targetColliders, false);
            }
            finally
            {
                fixture.Dispose();
            }
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
                yield return null;

                Assert.That(fixture.Target.transform.position, Is.Not.EqualTo(releasePosition));
                Assert.That(fixture.Target.transform.position.z, Is.LessThan(3.7f));
                Assert.That(fixture.Target.gameObject.activeInHierarchy, Is.False);
                Assert.That(fixture.Lifecycle.ThrowOriginCount, Is.EqualTo(1));
                Assert.That(fixture.Target.CommittedPlayerThrowUses, Is.EqualTo(1));
                Assert.That(fixture.Target.RemainingPlayerThrowUses, Is.Zero);
                Assert.That(fixture.Target.IsPlayerThrowExhausted, Is.True);
                Assert.That(fixture.Target.IsThrowDepletionPending, Is.False);
                Assert.That(fixture.Target.LastCommittedPlayerThrowSequence, Is.EqualTo(1));
            }
            finally
            {
                fixture.Dispose();
                Object.Destroy(wall);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator HeldThrowCharge_IncreasesReleaseSpeedAndPhysicalKineticEnergy()
        {
            float tapSpeed = 0f;
            float tapEnergy = 0f;
            CarryFixture tapFixture = new CarryFixture(PrototypeWeightClass.Light);
            try
            {
                Assert.That(tapFixture.Controller.ProcessInteractPrimary(false).IsAccepted, Is.True);
                Assert.That(tapFixture.Controller.BeginThrowCharge(), Is.True);
                Assert.That(tapFixture.Controller.ReleaseThrowCharge(), Is.True);
                tapSpeed = tapFixture.Lifecycle.LastReleaseSpeed;
                tapEnergy = tapFixture.Lifecycle.LastReleaseKineticEnergy;
                Assert.That(tapFixture.Lifecycle.LastThrowCharge01, Is.LessThan(0.05f));
                Assert.That(tapFixture.Lifecycle.LastThrowSpeedMultiplier, Is.EqualTo(1f).Within(0.03f));
            }
            finally
            {
                tapFixture.Dispose();
            }

            yield return null;

            CarryFixture chargedFixture = new CarryFixture(PrototypeWeightClass.Light);
            try
            {
                Assert.That(chargedFixture.Controller.ProcessInteractPrimary(false).IsAccepted, Is.True);
                Assert.That(chargedFixture.Controller.BeginThrowCharge(), Is.True);
                Assert.That(chargedFixture.Controller.IsThrowCharging, Is.True);
                yield return new WaitForSecondsRealtime(1.25f);
                Assert.That(chargedFixture.Controller.ThrowCharge01, Is.EqualTo(1f).Within(0.01f));
                Assert.That(chargedFixture.Controller.ReleaseThrowCharge(), Is.True);

                Assert.That(chargedFixture.Lifecycle.LastThrowCharge01, Is.EqualTo(1f).Within(0.01f));
                Assert.That(chargedFixture.Lifecycle.LastThrowSpeedMultiplier, Is.EqualTo(1.6f).Within(0.01f));
                Assert.That(chargedFixture.Lifecycle.LastReleaseSpeed, Is.GreaterThan(tapSpeed * 1.55f));
                Assert.That(chargedFixture.Lifecycle.LastReleaseKineticEnergy, Is.GreaterThan(tapEnergy * 2.4f));
                Assert.That(chargedFixture.Lifecycle.ThrowOriginCount, Is.EqualTo(1));
                Assert.That(chargedFixture.Owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
            }
            finally
            {
                chargedFixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator RepeatedAcquisition_ReusesSameObjectWithoutDuplication()
        {
            CarryFixture fixture = new CarryFixture(PrototypeWeightClass.Medium);
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = new Vector3(0f, 1f, 3.2f);
            wall.transform.localScale = new Vector3(4f, 2f, 0.3f);
            try
            {
                int instanceId = fixture.Target.gameObject.GetInstanceID();
                Assert.That(fixture.Controller.ProcessInteractPrimary(false).IsAccepted, Is.True);
                yield return new WaitForFixedUpdate();
                Assert.That(fixture.Controller.ProcessThrow(), Is.True);
                Assert.That(fixture.Target.CommittedPlayerThrowUses, Is.EqualTo(1));
                Assert.That(fixture.Target.RemainingPlayerThrowUses, Is.EqualTo(1));
                Assert.That(fixture.Target.IsPlayerThrowExhausted, Is.False);
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

                Assert.That(fixture.Controller.ProcessThrow(), Is.True);
                Assert.That(fixture.Target.CommittedPlayerThrowUses, Is.EqualTo(2));
                Assert.That(fixture.Target.RemainingPlayerThrowUses, Is.Zero);
                Assert.That(fixture.Target.IsPlayerThrowExhausted, Is.True);
                for (int step = 0; step < 25; step++)
                {
                    yield return new WaitForFixedUpdate();
                }
                yield return null;

                Assert.That(fixture.Target.gameObject.activeInHierarchy, Is.False);
                Assert.That(fixture.Target.gameObject.GetInstanceID(), Is.EqualTo(instanceId));
                Assert.That(fixture.Target.IsThrowDepletionPending, Is.False);
            }
            finally
            {
                fixture.Dispose();
                Object.Destroy(wall);
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

        [UnityTest]
        public IEnumerator ContestScene_GroundedDirectObjectsLiftOffWhenCarryBegins()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "Contest_SingleRoomGraybox",
                LoadSceneMode.Additive);
            yield return load;
            Scene scene = SceneManager.GetSceneByName("Contest_SingleRoomGraybox");
            AsyncOperation unload = null;
            try
            {
                PrototypeInteractionStateOwner owner = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<PrototypeInteractionStateOwner>(true))
                    .Single();
                PrototypePhysicalObjectLifecycle lifecycle = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<PrototypePhysicalObjectLifecycle>(true))
                    .Single();
                PrototypeInteractionTarget[] directTargets = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<PrototypeInteractionTarget>(true))
                    .Where(target => target.DirectAcquisitionEligible)
                    .OrderBy(target => target.RuntimeInstanceKey)
                    .ToArray();

                yield return new WaitForFixedUpdate();
                foreach (PrototypeInteractionTarget target in directTargets)
                {
                    Rigidbody body = target.GetComponent<Rigidbody>();
                    Vector3 baselinePosition = body.position;
                    Quaternion baselineRotation = body.rotation;
                    int baselineLayer = target.gameObject.layer;

                    Assert.That(
                        owner.TryCommit(PrototypeInteractionDecision.Accept(
                            PrototypeInteractionOutcome.Scoop,
                            target)),
                        Is.True,
                        target.RuntimeInstanceKey);
                    Assert.That(owner.TryCompleteAcquisition(target), Is.True,
                        target.RuntimeInstanceKey);
                    Assert.That(lifecycle.BeginCarry(target), Is.True, target.RuntimeInstanceKey);

                    for (int step = 0; step < 4; step++)
                    {
                        yield return new WaitForFixedUpdate();
                    }

                    Assert.That(
                        lifecycle.AcquisitionPathLength,
                        Is.GreaterThan(0.05f),
                        $"{target.RuntimeInstanceKey} remained frozen on its authored floor contact.");
                    Assert.That(
                        Vector3.Distance(body.position, baselinePosition),
                        Is.GreaterThan(0.05f),
                        $"{target.RuntimeInstanceKey} did not move toward the visible carry anchor.");

                    lifecycle.DetachForReset();
                    Assert.That(owner.CancelCarry(target), Is.True, target.RuntimeInstanceKey);
                    body.position = baselinePosition;
                    body.rotation = baselineRotation;
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                    target.gameObject.layer = baselineLayer;
                    Physics.SyncTransforms();
                }
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    unload = SceneManager.UnloadSceneAsync(scene);
                }
            }

            if (unload != null)
            {
                yield return unload;
            }
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
                    Is.True,
                    $"{weightClass} carry must keep every player/object pair ignored until detach.");
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
                    Is.True,
                    "Release collision restoration must be deferred while clearance is unresolved.");
                Rigidbody releasedBody = fixture.Target.GetComponent<Rigidbody>();
                releasedBody.linearVelocity = Vector3.zero;
                releasedBody.angularVelocity = Vector3.zero;
                releasedBody.position = fixture.PlayerTransform.position +
                    Vector3.up + Vector3.forward * 3f;
                Physics.SyncTransforms();
                yield return WaitForCollisionRestored(
                    fixture.PlayerController,
                    targetCollider,
                    $"{weightClass} release collision lease did not restore after clearance.");
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
            bool hasMeaningfulSelfOverlap = overlapsPlayer &&
                playerPenetrationDistance > 0.001f;
            Assert.That(
                fixture.Lifecycle.GripSelfOverlap,
                Is.EqualTo(hasMeaningfulSelfOverlap),
                $"{weightClass} carry self-overlap diagnostics drifted from physics.");
        }

        private static void AssertAllPairsIgnored(
            Collider[] playerColliders,
            Collider[] targetColliders,
            bool expected)
        {
            foreach (Collider playerCollider in playerColliders)
            {
                foreach (Collider targetCollider in targetColliders)
                {
                    Assert.That(
                        Physics.GetIgnoreCollision(playerCollider, targetCollider),
                        Is.EqualTo(expected),
                        $"{playerCollider.name} x {targetCollider.name}");
                }
            }
        }

        private static IEnumerator WaitForCollisionRestored(
            Collider playerCollider,
            Collider targetCollider,
            string failureMessage,
            int maximumFixedSteps = 8)
        {
            for (int step = 0; step < maximumFixedSteps; step++)
            {
                yield return new WaitForFixedUpdate();
                if (!Physics.GetIgnoreCollision(playerCollider, targetCollider))
                {
                    yield break;
                }
            }

            Assert.That(
                Physics.GetIgnoreCollision(playerCollider, targetCollider),
                Is.False,
                failureMessage);
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
