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
    public sealed class ContestResetPlayModeTests
    {
        [UnityTest]
        public IEnumerator ManualResetTenTimes_HasExactBaselineAndNoLeaks()
        {
            ResetFixture fixture = new ResetFixture();
            try
            {
                Vector3 playerBaseline = fixture.Player.transform.position;
                Vector3 actorBaseline = fixture.Actors[0].position;
                Vector3 objectBaseline = fixture.Targets[0].transform.position;

                for (int attempt = 0; attempt < 10; attempt++)
                {
                    fixture.Player.transform.position += Vector3.right * (attempt + 1);
                    fixture.Actors[0].position += Vector3.forward * (attempt + 1);
                    fixture.Targets[0].transform.position += Vector3.up * (attempt + 1);
                    Rigidbody body = fixture.Targets[0].GetComponent<Rigidbody>();
                    body.linearVelocity = new Vector3(2f, 3f, 4f);
                    body.angularVelocity = Vector3.one * 2f;
                    Physics.SyncTransforms();

                    Assert.That(
                        fixture.Reset.RequestReset(EncounterResetCause.ManualInput),
                        Is.True);
                    Assert.That(
                        fixture.Owner.GlobalState,
                        Is.EqualTo(PrototypeGlobalInteractionState.ResetPending));
                    Assert.That(fixture.Player.transform.position, Is.EqualTo(playerBaseline));
                    Assert.That(fixture.Actors[0].position, Is.EqualTo(actorBaseline));
                    Assert.That(fixture.Targets[0].transform.position, Is.EqualTo(objectBaseline));
                    yield return null;
                    Assert.That(
                        fixture.Owner.GlobalState,
                        Is.EqualTo(PrototypeGlobalInteractionState.Playable));
                }

                Assert.That(fixture.Reset.ResetCount, Is.EqualTo(10));
                Assert.That(fixture.Reset.UnexpectedBaselineDifferenceCount, Is.Zero);
                Assert.That(fixture.Reset.DuplicateHandlerCount, Is.Zero);
                Assert.That(fixture.Reset.ObjectLossCount, Is.Zero);
                Assert.That(fixture.Reset.ObjectDuplicationCount, Is.Zero);
                Assert.That(fixture.Reset.OwnershipLeakCount, Is.Zero);
                Assert.That(fixture.Reset.SubscriptionCount, Is.Zero);
                Assert.That(fixture.Reset.FaultQuarantineCount, Is.Zero);
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ResetDuringCarry_ClearsOwnershipAndRestoresSameObject()
        {
            ResetFixture fixture = new ResetFixture();
            GameObject wall = null;
            try
            {
                PrototypeInteractionTarget binder = fixture.Targets[0];
                Vector3 baseline = binder.transform.position;
                int originalLayer = binder.gameObject.layer;
                int instanceId = binder.gameObject.GetInstanceID();
                for (int index = 1; index < fixture.Targets.Length; index++)
                {
                    fixture.Targets[index].transform.position += Vector3.forward * 20f;
                }

                Vector3 horizontalCarryDirection = Vector3.ProjectOnPlane(
                    fixture.Anchor.position - baseline,
                    Vector3.up).normalized;
                wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = "ResetDuringBlockedCarryWall";
                wall.transform.position = Vector3.Lerp(baseline, fixture.Anchor.position, 0.5f);
                wall.transform.rotation = Quaternion.LookRotation(horizontalCarryDirection);
                wall.transform.localScale = new Vector3(4f, 4f, 0.2f);
                Physics.SyncTransforms();

                var arbitrator = new PrototypeInteractionArbitrator();
                Assert.That(fixture.Owner.TryCommit(
                    arbitrator.Decide(fixture.Owner.CreateContext(false, binder))), Is.True);
                Assert.That(fixture.Lifecycle.BeginCarry(binder), Is.True);
                for (int step = 0; step < 30; step++)
                {
                    yield return new WaitForFixedUpdate();
                }

                Assert.That(binder.transform.position, Is.Not.EqualTo(baseline));
                bool penetrates = Physics.ComputePenetration(
                    binder.GetComponent<Collider>(),
                    binder.transform.position,
                    binder.transform.rotation,
                    wall.GetComponent<Collider>(),
                    wall.transform.position,
                    wall.transform.rotation,
                    out _,
                    out float penetrationDistance);
                Assert.That(penetrates && penetrationDistance > 0.001f, Is.False);
                Assert.That(fixture.Owner.CarriedTarget, Is.SameAs(binder));

                Assert.That(
                    fixture.Reset.RequestReset(EncounterResetCause.ManualInput),
                    Is.True);
                Assert.That(binder.transform.position, Is.EqualTo(baseline));
                Assert.That(binder.gameObject.activeInHierarchy, Is.True);
                Assert.That(binder.gameObject.layer, Is.EqualTo(originalLayer));
                Assert.That(binder.gameObject.GetInstanceID(), Is.EqualTo(instanceId));
                Assert.That(binder.GetComponent<Rigidbody>().isKinematic, Is.False);
                Assert.That(binder.GetComponent<Rigidbody>().useGravity, Is.False);
                Assert.That(binder.GetComponent<Rigidbody>().linearVelocity, Is.EqualTo(Vector3.zero));
                Assert.That(binder.GetComponent<Rigidbody>().angularVelocity, Is.EqualTo(Vector3.zero));
                yield return null;

                Assert.That(fixture.Owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
                Assert.That(fixture.Owner.CarriedTarget, Is.Null);
                Assert.That(fixture.Lifecycle.CarriedTarget, Is.Null);
                Assert.That(fixture.Reset.OwnershipLeakCount, Is.Zero);
            }
            finally
            {
                fixture.Dispose();
                Object.Destroy(wall);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator AutomaticFailureCause_UsesSharedBarrierAndBlocksInteraction()
        {
            ResetFixture fixture = new ResetFixture();
            try
            {
                Assert.That(fixture.Owner.TryBeginFailurePending(), Is.True);
                Assert.That(
                    fixture.Reset.RequestReset(EncounterResetCause.AutomaticFailureRestart),
                    Is.True);
                Assert.That(
                    fixture.Reset.LastResetCause,
                    Is.EqualTo(EncounterResetCause.AutomaticFailureRestart));

                var arbitrator = new PrototypeInteractionArbitrator();
                PrototypeInteractionDecision blocked = arbitrator.Decide(
                    fixture.Owner.CreateContext(false, fixture.Targets[0]));
                Assert.That(blocked.Rejection,
                    Is.EqualTo(PrototypeInteractionRejection.GlobalStateBlocked));
                yield return null;
                Assert.That(fixture.Owner.GlobalState,
                    Is.EqualTo(PrototypeGlobalInteractionState.Playable));
            }
            finally
            {
                fixture.Dispose();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator ContestScene_ResetInventoryIsExactlyAuthoredPrototypeContent()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "Contest_SingleRoomGraybox",
                LoadSceneMode.Additive);
            yield return load;
            Scene scene = SceneManager.GetSceneByName("Contest_SingleRoomGraybox");
            AuthoredEncounterResetController[] resetControllers = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<AuthoredEncounterResetController>(true))
                .ToArray();

            Assert.That(resetControllers, Has.Length.EqualTo(1));
            Assert.That(resetControllers[0].BaselineObjectCount, Is.EqualTo(7));
            Assert.That(resetControllers[0].ResetCount, Is.Zero);
            Assert.That(resetControllers[0].SubscriptionCount, Is.Zero);

            AsyncOperation unload = SceneManager.UnloadSceneAsync(scene);
            yield return unload;
        }

        private sealed class ResetFixture
        {
            private readonly GameObject resetRoot;

            public ResetFixture()
            {
                Player = new GameObject("Player");
                Player.transform.position = new Vector3(0f, 1f, -2f);
                Player.AddComponent<CharacterController>();
                Owner = Player.AddComponent<PrototypeInteractionStateOwner>();
                Lifecycle = Player.AddComponent<PrototypePhysicalObjectLifecycle>();
                Anchor = new GameObject("CarryAnchor").transform;
                Anchor.SetParent(Player.transform, false);
                Anchor.localPosition = new Vector3(0f, 1f, 1f);
                Lifecycle.Configure(Owner, Anchor);

                Actors = new[]
                {
                    CreateActor("Ally", new Vector3(-2f, 1f, 3f)),
                    CreateActor("Runner01", new Vector3(2f, 1f, 3f)),
                    CreateActor("Runner02", new Vector3(4f, 1f, 3f))
                };

                Targets = new PrototypeInteractionTarget[7];
                for (int index = 0; index < Targets.Length; index++)
                {
                    bool direct = index == 0 || index == 3;
                    PrototypeWeightClass weight = index < 3
                        ? PrototypeWeightClass.Light
                        : index < 6
                            ? PrototypeWeightClass.Medium
                            : PrototypeWeightClass.Heavy;
                    PrototypeInteractionRole role = index switch
                    {
                        0 => PrototypeInteractionRole.DirectBinder,
                        1 => PrototypeInteractionRole.AllyBinder,
                        2 => PrototypeInteractionRole.AllyBinder,
                        3 => PrototypeInteractionRole.DirectChair,
                        4 => PrototypeInteractionRole.AllyChair,
                        5 => PrototypeInteractionRole.Drawer,
                        _ => PrototypeInteractionRole.Copier
                    };
                    GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    item.name = $"Object{index + 1}";
                    item.transform.position = new Vector3(index - 3f, 0.5f, 2f);
                    if (direct)
                    {
                        Rigidbody body = item.AddComponent<Rigidbody>();
                        body.useGravity = false;
                    }

                    PrototypeInteractionTarget target =
                        item.AddComponent<PrototypeInteractionTarget>();
                    target.Configure(
                        $"DEF-{weight}",
                        $"INSTANCE-{index + 1}",
                        role.ToString(),
                        weight,
                        role,
                        direct,
                        false,
                        false,
                        false);
                    Targets[index] = target;
                }

                resetRoot = new GameObject("ResetRoot");
                Reset = resetRoot.AddComponent<AuthoredEncounterResetController>();
                Reset.Configure(
                    null,
                    Owner,
                    Lifecycle,
                    null,
                    Player.transform,
                    Actors,
                    Targets);
                Physics.SyncTransforms();
            }

            public GameObject Player { get; }
            public Transform[] Actors { get; }
            public PrototypeInteractionTarget[] Targets { get; }
            public PrototypeInteractionStateOwner Owner { get; }
            public PrototypePhysicalObjectLifecycle Lifecycle { get; }
            public AuthoredEncounterResetController Reset { get; }
            public Transform Anchor { get; }

            public void Dispose()
            {
                Object.Destroy(Player);
                foreach (Transform actor in Actors)
                {
                    Object.Destroy(actor.gameObject);
                }

                foreach (PrototypeInteractionTarget target in Targets)
                {
                    Object.Destroy(target.gameObject);
                }

                Object.Destroy(resetRoot);
            }

            private static Transform CreateActor(string name, Vector3 position)
            {
                GameObject actor = new GameObject(name);
                actor.transform.position = position;
                return actor.transform;
            }
        }
    }
}
