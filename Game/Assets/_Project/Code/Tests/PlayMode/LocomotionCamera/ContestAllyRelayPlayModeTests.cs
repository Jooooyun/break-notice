using System.Collections;
using System.Linq;
using BreakNotice.Gameplay.Encounter;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestAllyRelayPlayModeTests
    {
        [UnityTest]
        public IEnumerator BinderRelease_StartsAtCurrentWorldPose()
        {
            RelayFixture fixture = new RelayFixture();
            try
            {
                Vector3 start = fixture.BinderOne.transform.position;
                Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderOne, out _), Is.True);
                Assert.That(fixture.BinderOne.transform.position, Is.EqualTo(start));
                yield return fixture.WaitForRelease();

                Assert.That(fixture.Relay.LastReleasePosition, Is.EqualTo(start));
                Assert.That(fixture.Relay.ReleaseCount, Is.EqualTo(1));
                Assert.That(fixture.BinderOne.GetComponent<Rigidbody>().isKinematic, Is.False);
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator BinderRelease_DisplacementIsContinuousNotTeleport()
        {
            RelayFixture fixture = new RelayFixture();
            try
            {
                Vector3 start = fixture.BinderOne.transform.position;
                Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderOne, out _), Is.True);
                yield return fixture.WaitForRelease();
                yield return new WaitForFixedUpdate();

                float displacement = Vector3.Distance(start, fixture.BinderOne.transform.position);
                Assert.That(displacement, Is.GreaterThan(0.01f));
                Assert.That(displacement, Is.LessThan(0.5f));
                Assert.That(fixture.BinderOne.gameObject.activeInHierarchy, Is.True);
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ReleasedBinder_HasOneLaunchAndNoRemoteSteering()
        {
            RelayFixture fixture = new RelayFixture();
            try
            {
                Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderOne, out _), Is.True);
                yield return fixture.WaitForRelease();
                Vector3 initialHorizontal = Vector3.ProjectOnPlane(
                    fixture.BinderOne.GetComponent<Rigidbody>().linearVelocity,
                    Vector3.up).normalized;

                fixture.Player.transform.position += Vector3.right * 5f;
                fixture.Player.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                for (int step = 0; step < 3; step++)
                {
                    yield return new WaitForFixedUpdate();
                }

                Vector3 laterHorizontal = Vector3.ProjectOnPlane(
                    fixture.BinderOne.GetComponent<Rigidbody>().linearVelocity,
                    Vector3.up).normalized;
                Assert.That(fixture.Relay.ReleaseCount, Is.EqualTo(1));
                Assert.That(Vector3.Dot(initialHorizontal, laterHorizontal), Is.GreaterThan(0.99f));
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ReleasedBinder_IsBlockedBySolidWorldCollider()
        {
            RelayFixture fixture = new RelayFixture();
            GameObject wall = null;
            try
            {
                Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderOne, out _), Is.True);
                yield return fixture.WaitForRelease();
                wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = "RelayBinderBlockingWall";
                wall.transform.position = new Vector3(0f, 1f, 2f);
                wall.transform.localScale = new Vector3(3f, 3f, 0.2f);
                Physics.SyncTransforms();

                for (int step = 0; step < 20; step++)
                {
                    yield return new WaitForFixedUpdate();
                }

                Collider binderCollider = fixture.BinderOne.GetComponent<Collider>();
                Collider wallCollider = wall.GetComponent<Collider>();
                Assert.That(
                    binderCollider.bounds.min.z,
                    Is.GreaterThanOrEqualTo(wallCollider.bounds.max.z - 0.02f));
                Assert.That(fixture.BinderOne.gameObject.activeInHierarchy, Is.True);
            }
            finally
            {
                fixture.Dispose();
                Object.Destroy(wall);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator Catch_RequiresActualColliderRange()
        {
            RelayFixture fixture = new RelayFixture();
            try
            {
                Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderOne, out _), Is.True);
                yield return fixture.WaitForRelease();
                Rigidbody body = fixture.BinderOne.GetComponent<Rigidbody>();
                body.linearVelocity = Vector3.zero;
                body.position = new Vector3(0f, 1f, 10f);
                fixture.BinderOne.transform.position = body.position;
                Physics.SyncTransforms();
                yield return new WaitForFixedUpdate();

                Assert.That(fixture.Relay.CanCatch(fixture.BinderOne), Is.False);
                Assert.That(fixture.Owner.RelayReservedTarget, Is.SameAs(fixture.BinderOne));
                Assert.That(fixture.Owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator Catch_IsRejectedThroughWallWithoutOwnershipMutation()
        {
            RelayFixture fixture = new RelayFixture();
            GameObject wall = null;
            try
            {
                Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderOne, out _), Is.True);
                yield return fixture.WaitForRelease();
                fixture.AimAt(fixture.BinderOne);
                Vector3 midpoint = Vector3.Lerp(
                    fixture.View.transform.position,
                    fixture.BinderOne.GetComponent<Collider>().bounds.center,
                    0.5f);
                wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.transform.position = midpoint;
                wall.transform.localScale = new Vector3(2f, 2f, 0.1f);
                wall.transform.rotation = Quaternion.LookRotation(
                    fixture.BinderOne.transform.position - fixture.View.transform.position);
                Physics.SyncTransforms();

                PrototypeInteractionDecision decision =
                    fixture.Interaction.ProcessInteractPrimary(false);
                Assert.That(decision.IsAccepted, Is.False);
                Assert.That(fixture.Owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
                Assert.That(fixture.Owner.RelayReservedTarget, Is.SameAs(fixture.BinderOne));
            }
            finally
            {
                fixture.Dispose();
                Object.Destroy(wall);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator NormalCatch_CarriesTheSameVisibleBinderInstance()
        {
            RelayFixture fixture = new RelayFixture();
            try
            {
                int instanceId = fixture.BinderOne.gameObject.GetInstanceID();
                Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderOne, out _), Is.True);
                yield return fixture.WaitForRelease();
                fixture.AimAt(fixture.BinderOne);

                PrototypeInteractionDecision decision =
                    fixture.Interaction.ProcessInteractPrimary(false);

                Assert.That(decision.Outcome, Is.EqualTo(PrototypeInteractionOutcome.Catch));
                Assert.That(fixture.Owner.CarriedTarget, Is.SameAs(fixture.BinderOne));
                Assert.That(fixture.Lifecycle.CarriedTarget, Is.SameAs(fixture.BinderOne));
                Assert.That(fixture.Owner.CarriedTarget.gameObject.GetInstanceID(), Is.EqualTo(instanceId));
                Assert.That(fixture.Relay.RelayPhase, Is.EqualTo(PrototypeRelayPhase.None));
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator QueuedCatch_ProducesOnePlayerReleaseAndOneOrigin()
        {
            RelayFixture fixture = new RelayFixture();
            try
            {
                Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderOne, out _), Is.True);
                Assert.That(fixture.Interaction.ProcessThrow(), Is.True);
                yield return fixture.WaitForRelease();
                fixture.AimAt(fixture.BinderOne);

                PrototypeInteractionDecision decision =
                    fixture.Interaction.ProcessInteractPrimary(false);

                Assert.That(decision.Outcome, Is.EqualTo(PrototypeInteractionOutcome.Catch));
                Assert.That(fixture.Owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
                Assert.That(fixture.Lifecycle.ThrowOriginCount, Is.EqualTo(1));
                Assert.That(fixture.Lifecycle.LastThrowOrigin.IsPlayerOwned, Is.True);
                Assert.That(fixture.Relay.QueuedThrowCount, Is.EqualTo(1));
                Assert.That(fixture.Relay.CatchCount, Is.EqualTo(1));
                Assert.That(fixture.Owner.QueuedThrowArmed, Is.False);
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator MissedBinder_RemainsInRoomAndCannotReplenishBeforeReset()
        {
            RelayFixture fixture = new RelayFixture();
            try
            {
                int instanceId = fixture.BinderOne.gameObject.GetInstanceID();
                Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderOne, out _), Is.True);
                yield return fixture.WaitForRelease();
                fixture.Player.transform.position += Vector3.right * 8f;
                for (int step = 0; step < 28; step++)
                {
                    yield return new WaitForFixedUpdate();
                }

                Assert.That(fixture.Relay.RelayPhase, Is.EqualTo(PrototypeRelayPhase.None));
                Assert.That(fixture.BinderOne.gameObject.activeInHierarchy, Is.True);
                Assert.That(fixture.BinderOne.gameObject.GetInstanceID(), Is.EqualTo(instanceId));
                Assert.That(
                    fixture.Relay.TryRequestSupply(fixture.BinderOne, out var rejection),
                    Is.False);
                Assert.That(rejection, Is.EqualTo(PrototypeAllyRelayRejection.SupplyAlreadyConsumed));
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ChairFeed_RemainsFloorSupportedAndPlanar()
        {
            RelayFixture fixture = new RelayFixture();
            try
            {
                float baselineBottom = fixture.Chair.GetComponent<Collider>().bounds.min.y;
                Assert.That(fixture.Relay.TryRequestSupply(fixture.Chair, out _), Is.True);
                yield return fixture.WaitForRelease();
                Vector3 launch = fixture.Relay.LastLaunchVelocity;
                for (int step = 0; step < 5; step++)
                {
                    yield return new WaitForFixedUpdate();
                }

                Assert.That(Mathf.Abs(launch.y), Is.LessThan(0.001f));
                Assert.That(Vector3.ProjectOnPlane(launch, Vector3.up).magnitude, Is.GreaterThan(1f));
                Assert.That(
                    fixture.Chair.GetComponent<Collider>().bounds.min.y,
                    Is.EqualTo(baselineBottom).Within(0.08f));
                Assert.That(fixture.Chair.GetComponent<Rigidbody>().useGravity, Is.True);
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ChairFeed_IsNeverCatchEligible()
        {
            RelayFixture fixture = new RelayFixture();
            try
            {
                Assert.That(fixture.Relay.TryRequestSupply(fixture.Chair, out _), Is.True);
                yield return fixture.WaitForRelease();

                Assert.That(fixture.Relay.CanCatch(fixture.Chair), Is.False);
                Assert.That(fixture.Chair.CatchEligible, Is.False);
                Assert.That(fixture.Owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ChairFeed_CannotArmQueuedThrow()
        {
            RelayFixture fixture = new RelayFixture();
            try
            {
                Assert.That(fixture.Relay.TryRequestSupply(fixture.Chair, out _), Is.True);
                Assert.That(fixture.Interaction.ProcessThrow(), Is.False);
                Assert.That(fixture.Owner.QueuedThrowArmed, Is.False);
                yield return fixture.WaitForRelease();
                Assert.That(fixture.Interaction.ProcessThrow(), Is.False);
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ActiveRelay_RejectsSecondCallWithZeroSideEffect()
        {
            RelayFixture fixture = new RelayFixture();
            try
            {
                Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderOne, out _), Is.True);
                Vector3 secondPosition = fixture.BinderTwo.transform.position;

                Assert.That(
                    fixture.Relay.TryRequestSupply(fixture.BinderTwo, out var rejection),
                    Is.False);
                Assert.That(rejection, Is.EqualTo(PrototypeAllyRelayRejection.RelayAlreadyActive));
                Assert.That(fixture.Owner.RelayReservedTarget, Is.SameAs(fixture.BinderOne));
                Assert.That(fixture.BinderTwo.transform.position, Is.EqualTo(secondPosition));
                Assert.That(fixture.BinderTwo.GetComponent<Rigidbody>().isKinematic, Is.False);
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ResetDuringPreRelease_RestoresExactBaseline()
        {
            RelayFixture fixture = new RelayFixture();
            try
            {
                Vector3 baseline = fixture.BinderOne.transform.position;
                Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderOne, out _), Is.True);
                Assert.That(fixture.Reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
                yield return null;

                fixture.AssertResetBaseline(fixture.BinderOne, baseline);
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ResetDuringReleasedBinder_RestoresExactBaseline()
        {
            RelayFixture fixture = new RelayFixture();
            try
            {
                Vector3 baseline = fixture.BinderOne.transform.position;
                Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderOne, out _), Is.True);
                yield return fixture.WaitForRelease();
                yield return new WaitForFixedUpdate();
                Assert.That(fixture.BinderOne.transform.position, Is.Not.EqualTo(baseline));
                Assert.That(fixture.Reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
                yield return null;

                fixture.AssertResetBaseline(fixture.BinderOne, baseline);
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ResetWhileQueueArmed_ClearsQueueAndReservation()
        {
            RelayFixture fixture = new RelayFixture();
            try
            {
                Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderOne, out _), Is.True);
                Assert.That(fixture.Interaction.ProcessThrow(), Is.True);
                Assert.That(fixture.Owner.QueuedThrowArmed, Is.True);
                Assert.That(fixture.Reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
                yield return null;

                Assert.That(fixture.Owner.QueuedThrowArmed, Is.False);
                Assert.That(fixture.Owner.RelayReservedTarget, Is.Null);
                Assert.That(fixture.Relay.HasBaselineState, Is.True);
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ResetAfterNormalCatch_ClearsCarryAndRestoresBinder()
        {
            RelayFixture fixture = new RelayFixture();
            try
            {
                Vector3 baseline = fixture.BinderOne.transform.position;
                Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderOne, out _), Is.True);
                yield return fixture.WaitForRelease();
                fixture.AimAt(fixture.BinderOne);
                Assert.That(fixture.Interaction.ProcessInteractPrimary(false).IsAccepted, Is.True);
                yield return new WaitForFixedUpdate();
                Assert.That(fixture.Owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Carrying));

                Assert.That(fixture.Reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
                yield return null;

                fixture.AssertResetBaseline(fixture.BinderOne, baseline);
                Assert.That(fixture.Lifecycle.CarriedTarget, Is.Null);
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ResetAfterQueuedThrow_RestoresBinderAndOriginBaseline()
        {
            RelayFixture fixture = new RelayFixture();
            try
            {
                Vector3 baseline = fixture.BinderOne.transform.position;
                Assert.That(fixture.Relay.TryRequestSupply(fixture.BinderOne, out _), Is.True);
                Assert.That(fixture.Interaction.ProcessThrow(), Is.True);
                yield return fixture.WaitForRelease();
                fixture.AimAt(fixture.BinderOne);
                Assert.That(fixture.Interaction.ProcessInteractPrimary(false).IsAccepted, Is.True);
                Assert.That(fixture.Lifecycle.ThrowOriginCount, Is.EqualTo(1));

                Assert.That(fixture.Reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
                yield return null;

                fixture.AssertResetBaseline(fixture.BinderOne, baseline);
                Assert.That(fixture.Lifecycle.ThrowOriginCount, Is.Zero);
                Assert.That(fixture.Lifecycle.LastThrowOrigin.IsPlayerOwned, Is.False);
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ResetAudit_HasNoObjectLossDuplicationOrOwnershipLeak()
        {
            RelayFixture fixture = new RelayFixture();
            try
            {
                Assert.That(fixture.Relay.TryRequestSupply(fixture.Chair, out _), Is.True);
                yield return fixture.WaitForRelease();
                Assert.That(fixture.Reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
                yield return null;

                Assert.That(fixture.Reset.ObjectLossCount, Is.Zero);
                Assert.That(fixture.Reset.ObjectDuplicationCount, Is.Zero);
                Assert.That(fixture.Reset.OwnershipLeakCount, Is.Zero);
                Assert.That(fixture.Reset.UnexpectedBaselineDifferenceCount, Is.Zero);
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator DirectBinderAndChair_RemainC2CandidatesOutsideRelayPool()
        {
            RelayFixture fixture = new RelayFixture();
            try
            {
                foreach (PrototypeInteractionTarget target in new[]
                         { fixture.DirectBinder, fixture.DirectChair })
                {
                    fixture.AimAt(target);
                    PrototypeInteractionDecision decision =
                        fixture.Interaction.ProcessInteractPrimary(false);
                    Assert.That(decision.Outcome, Is.EqualTo(PrototypeInteractionOutcome.Scoop));
                    yield return new WaitForFixedUpdate();
                    Assert.That(fixture.Lifecycle.AcquisitionPathLength, Is.GreaterThan(0f));
                    Assert.That(fixture.Interaction.ProcessThrow(), Is.True);
                    Assert.That(fixture.Reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
                    yield return null;
                    Assert.That(fixture.Owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
                }

                Assert.That(
                    fixture.Relay.CanRequestSupply(fixture.DirectBinder, out var rejection),
                    Is.False);
                Assert.That(rejection, Is.EqualTo(PrototypeAllyRelayRejection.NoValidSupply));
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ContestScene_HasOneFixedRelayAndExactThreeObjectSupplyPool()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "Contest_SingleRoomGraybox",
                LoadSceneMode.Additive);
            yield return load;
            Scene scene = SceneManager.GetSceneByName("Contest_SingleRoomGraybox");
            AsyncOperation unloadOperation = null;
            try
            {
                PrototypeAllyRelayController[] relays = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<PrototypeAllyRelayController>(true))
                    .ToArray();
                PrototypeInteractionTarget[] targets = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<PrototypeInteractionTarget>(true))
                    .ToArray();
                PrototypeInteractionTarget[] supply = targets.Where(target =>
                    target.Role == PrototypeInteractionRole.AllyBinder ||
                    target.Role == PrototypeInteractionRole.AllyChair).ToArray();

                Assert.That(relays, Has.Length.EqualTo(1));
                Assert.That(targets, Has.Length.EqualTo(7));
                Assert.That(supply, Has.Length.EqualTo(3));
                Assert.That(supply.Count(target => target.Role == PrototypeInteractionRole.AllyBinder), Is.EqualTo(2));
                Assert.That(supply.Count(target => target.Role == PrototypeInteractionRole.AllyChair), Is.EqualTo(1));
                Assert.That(supply.All(target => target.GetComponent<Rigidbody>() != null), Is.True);
                Assert.That(relays[0].HasBaselineState, Is.True);
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    unloadOperation = SceneManager.UnloadSceneAsync(scene);
                }
            }

            if (unloadOperation != null)
            {
                yield return unloadOperation;
            }
        }

        private sealed class RelayFixture
        {
            private readonly GameObject[] roots;
            private readonly Vector3[] targetBaselines;

            public RelayFixture()
            {
                Player = new GameObject("Player");
                Player.transform.position = Vector3.zero;
                Player.transform.rotation = Quaternion.identity;
                Player.AddComponent<CharacterController>();
                Owner = Player.AddComponent<PrototypeInteractionStateOwner>();
                Lifecycle = Player.AddComponent<PrototypePhysicalObjectLifecycle>();
                Transform carryAnchor = new GameObject("CarryAnchor").transform;
                carryAnchor.SetParent(Player.transform, false);
                carryAnchor.localPosition = new Vector3(0f, 1f, 1f);
                Lifecycle.Configure(Owner, carryAnchor);

                View = new GameObject("View");
                View.transform.position = new Vector3(0f, 1f, -0.25f);
                View.transform.rotation = Quaternion.identity;
                Ally = new GameObject("Ally");
                Ally.transform.position = new Vector3(0f, 0f, 4f);
                Transform enemyOne = new GameObject("EnemyOne").transform;
                enemyOne.position = new Vector3(3f, 0f, 4f);
                Transform enemyTwo = new GameObject("EnemyTwo").transform;
                enemyTwo.position = new Vector3(-3f, 0f, 4f);
                Floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Floor.name = "Floor";
                Floor.transform.position = new Vector3(0f, -0.25f, 2f);
                Floor.transform.localScale = new Vector3(20f, 0.5f, 20f);

                BinderOne = CreateTarget(
                    "BINDER-ALLY-01", PrototypeWeightClass.Light,
                    PrototypeInteractionRole.AllyBinder, false, true, false, false,
                    new Vector3(0f, 1f, 3f), true);
                BinderTwo = CreateTarget(
                    "BINDER-ALLY-02", PrototypeWeightClass.Light,
                    PrototypeInteractionRole.AllyBinder, false, true, false, false,
                    new Vector3(1.3f, 1f, 3.2f), true);
                Chair = CreateTarget(
                    "CHAIR-ALLY-01", PrototypeWeightClass.Medium,
                    PrototypeInteractionRole.AllyChair, false, false, true, true,
                    new Vector3(-1.3f, 0.5f, 3.2f), true);
                DirectBinder = CreateTarget(
                    "BINDER-DIRECT-01", PrototypeWeightClass.Light,
                    PrototypeInteractionRole.DirectBinder, true, false, false, false,
                    new Vector3(2f, 0.5f, 0.8f), true);
                DirectChair = CreateTarget(
                    "CHAIR-DIRECT-01", PrototypeWeightClass.Medium,
                    PrototypeInteractionRole.DirectChair, true, false, true, true,
                    new Vector3(-2f, 0.5f, 0.8f), true);
                Drawer = CreateTarget(
                    "DRAWER-01", PrototypeWeightClass.Medium,
                    PrototypeInteractionRole.Drawer, false, false, false, false,
                    new Vector3(4f, 0.5f, 4f), false);
                Copier = CreateTarget(
                    "COPIER-01", PrototypeWeightClass.Heavy,
                    PrototypeInteractionRole.Copier, false, false, false, false,
                    new Vector3(-4f, 0.5f, 4f), false);
                Targets = new[]
                {
                    BinderOne, BinderTwo, Chair, DirectBinder, DirectChair, Drawer, Copier
                };
                targetBaselines = Targets.Select(target => target.transform.position).ToArray();

                Relay = Ally.AddComponent<PrototypeAllyRelayController>();
                Relay.Configure(
                    Owner,
                    Lifecycle,
                    Ally.transform,
                    Player.transform,
                    new[] { BinderOne, BinderTwo, Chair },
                    0.02f,
                    6f,
                    3f,
                    2.8f,
                    0.2f,
                    0.5f,
                    0.1f,
                    0.4f);
                Interaction = Player.AddComponent<PrototypeInteractionController>();
                Interaction.Configure(
                    null,
                    Owner,
                    View.transform,
                    Targets,
                    2.8f,
                    false,
                    Lifecycle,
                    Relay);

                GameObject resetRoot = new GameObject("ResetRoot");
                Reset = resetRoot.AddComponent<AuthoredEncounterResetController>();
                Reset.Configure(
                    null,
                    Owner,
                    Lifecycle,
                    null,
                    Player.transform,
                    new[] { Ally.transform, enemyOne, enemyTwo },
                    Targets,
                    Relay);
                roots = new[]
                {
                    Player, View, Ally, enemyOne.gameObject, enemyTwo.gameObject,
                    Floor, BinderOne.gameObject, BinderTwo.gameObject, Chair.gameObject,
                    DirectBinder.gameObject, DirectChair.gameObject, Drawer.gameObject,
                    Copier.gameObject, resetRoot
                };
                Physics.SyncTransforms();
            }

            public GameObject Player { get; }
            public GameObject Ally { get; }
            public GameObject View { get; }
            public GameObject Floor { get; }
            public PrototypeInteractionStateOwner Owner { get; }
            public PrototypePhysicalObjectLifecycle Lifecycle { get; }
            public PrototypeAllyRelayController Relay { get; }
            public PrototypeInteractionController Interaction { get; }
            public AuthoredEncounterResetController Reset { get; }
            public PrototypeInteractionTarget BinderOne { get; }
            public PrototypeInteractionTarget BinderTwo { get; }
            public PrototypeInteractionTarget Chair { get; }
            public PrototypeInteractionTarget DirectBinder { get; }
            public PrototypeInteractionTarget DirectChair { get; }
            public PrototypeInteractionTarget Drawer { get; }
            public PrototypeInteractionTarget Copier { get; }
            public PrototypeInteractionTarget[] Targets { get; }

            public IEnumerator WaitForRelease()
            {
                for (int step = 0; step < 5 && Relay.RelayPhase != PrototypeRelayPhase.Released; step++)
                {
                    yield return new WaitForFixedUpdate();
                }

                Assert.That(Relay.RelayPhase, Is.EqualTo(PrototypeRelayPhase.Released));
            }

            public void AimAt(PrototypeInteractionTarget target)
            {
                View.transform.position = Player.transform.position + Vector3.up;
                View.transform.rotation = Quaternion.LookRotation(
                    target.GetComponent<Collider>().bounds.center - View.transform.position);
                Physics.SyncTransforms();
                Interaction.RefreshTarget(false);
            }

            public void AssertResetBaseline(
                PrototypeInteractionTarget focus,
                Vector3 expectedPosition)
            {
                Assert.That(focus.transform.position, Is.EqualTo(expectedPosition));
                Assert.That(focus.GetComponent<Rigidbody>().linearVelocity, Is.EqualTo(Vector3.zero));
                Assert.That(focus.GetComponent<Rigidbody>().angularVelocity, Is.EqualTo(Vector3.zero));
                Assert.That(Owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
                Assert.That(Owner.RelayState, Is.EqualTo(PrototypeRelayReservationState.None));
                Assert.That(Owner.RelayReservedTarget, Is.Null);
                Assert.That(Owner.QueuedThrowArmed, Is.False);
                Assert.That(Relay.HasBaselineState, Is.True);
            }

            public void Dispose()
            {
                foreach (GameObject root in roots)
                {
                    if (root != null)
                    {
                        Object.Destroy(root);
                    }
                }
            }

            private static PrototypeInteractionTarget CreateTarget(
                string key,
                PrototypeWeightClass weightClass,
                PrototypeInteractionRole role,
                bool direct,
                bool catchEligible,
                bool strapEligible,
                bool anchor,
                Vector3 position,
                bool bodyRequired)
            {
                GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
                item.name = key;
                item.transform.position = position;
                if (bodyRequired)
                {
                    Rigidbody body = item.AddComponent<Rigidbody>();
                    body.useGravity = false;
                    body.isKinematic = false;
                    body.interpolation = RigidbodyInterpolation.Interpolate;
                    body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                }

                PrototypeInteractionTarget target =
                    item.AddComponent<PrototypeInteractionTarget>();
                target.Configure(
                    weightClass == PrototypeWeightClass.Light ? "OBJ-P-L-001" :
                    weightClass == PrototypeWeightClass.Medium ? "OBJ-P-M-001" : "OBJ-P-H-001",
                    key,
                    role.ToString(),
                    weightClass,
                    role,
                    direct,
                    catchEligible,
                    strapEligible,
                    anchor);
                return target;
            }
        }
    }
}
