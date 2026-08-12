using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BreakNotice.Gameplay.Encounter;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestShowcaseS3PlayModeTests
    {
        private const string S3ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v003.unity";

        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (GameObject item in createdObjects)
            {
                if (item != null)
                {
                    UnityEngine.Object.Destroy(item);
                }
            }
            createdObjects.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator ExplicitBinderFetch_ReturnsReadyAndQReleasesPhysicalObject()
        {
            Fixture fixture = CreateFixture(
                PrototypeWeightClass.Light,
                PrototypeInteractionRole.AllyBinder);

            Assert.That(fixture.Controller.ProcessAllyTask(), Is.True);
            Assert.That(fixture.Relay.PreparedTarget, Is.SameAs(fixture.FetchTarget));
            Assert.That(fixture.Mobile.IsFetchTask, Is.True);
            yield return WaitForReady(fixture);

            Assert.That(fixture.Mobile.IsReturnTask, Is.True);
            Assert.That(fixture.Relay.IsReadyForCall, Is.True);
            Assert.That(fixture.FetchBody.isKinematic, Is.True);
            Assert.That(fixture.Controller.ProcessCallAlly(), Is.True);
            yield return WaitForRelease(fixture);

            Assert.That(fixture.Relay.ReleaseCount, Is.EqualTo(1));
            Assert.That(fixture.Relay.IsBinderIncoming, Is.True);
            Assert.That(fixture.FetchBody.isKinematic, Is.False);
            Assert.That(fixture.FetchBody.useGravity, Is.True);
            Assert.That(fixture.Relay.LastBinderHorizontalSpeed, Is.GreaterThan(0.1f));
            Assert.That(fixture.Relay.ConsumedTargetCount, Is.EqualTo(1));
            Assert.That(fixture.Mobile.HasTaskMovement, Is.False);

            fixture.Relay.ResolveCatchFailure(fixture.FetchTarget);
            yield return new WaitForFixedUpdate();
            AimViewAt(fixture.View.transform, fixture.FetchTarget.transform.position);
            Assert.That(fixture.Relay.AllyState, Is.EqualTo(PrototypeAllySupportState.Depleted));
            Assert.That(fixture.Relay.IsAwaitingFetchTask, Is.False);
            Assert.That(fixture.Relay.IsFetchTargetAvailable(fixture.FetchTarget), Is.False);
            Assert.That(fixture.Controller.ProcessAllyTask(), Is.False);
            Assert.That(fixture.Controller.LastAllyTaskRejection,
                Is.EqualTo(PrototypeAllyRelayRejection.NoValidSupply));
        }

        [UnityTest]
        public IEnumerator ExplicitChairFetchAndFeed_StayGroundedWithoutPushingPlayer()
        {
            Fixture fixture = CreateFixture(
                PrototypeWeightClass.Medium,
                PrototypeInteractionRole.AllyChair);
            Vector3 playerStart = fixture.Player.transform.position;
            float groundHeight = fixture.FetchTarget.transform.position.y;

            Assert.That(fixture.Controller.ProcessAllyTask(), Is.True);
            for (int frame = 0; frame < 360 && !fixture.Relay.IsReadyForCall; frame++)
            {
                yield return new WaitForFixedUpdate();
                Assert.That(
                    Mathf.Abs(fixture.FetchTarget.transform.position.y - groundHeight),
                    Is.LessThan(0.025f),
                    $"Chair floated during fetch frame {frame}.");
                Assert.That(
                    PlanarDistance(fixture.Player.transform.position, playerStart),
                    Is.LessThan(0.03f),
                    $"Chair displaced the player during fetch frame {frame}.");
                Assert.That(fixture.Relay.AllyState,
                    Is.Not.EqualTo(PrototypeAllySupportState.Blocked));
            }

            Assert.That(fixture.Relay.IsReadyForCall, Is.True);
            Assert.That(fixture.Controller.ProcessCallAlly(), Is.True);
            for (int frame = 0; frame < 240 && fixture.Relay.PhysicalResolutionCount == 0; frame++)
            {
                yield return new WaitForFixedUpdate();
                Assert.That(
                    Mathf.Abs(fixture.FetchTarget.transform.position.y - groundHeight),
                    Is.LessThan(0.035f),
                    $"Chair floated during feed frame {frame}.");
                Assert.That(
                    PlanarDistance(fixture.Player.transform.position, playerStart),
                    Is.LessThan(0.03f),
                    $"Chair pushed the player during feed frame {frame}.");
            }

            Assert.That(fixture.Relay.ReleaseCount, Is.EqualTo(1));
            Assert.That(fixture.Relay.PhysicalResolutionCount, Is.EqualTo(1));
            Assert.That(fixture.FetchBody.useGravity, Is.True);
            Assert.That(
                (fixture.FetchBody.constraints & RigidbodyConstraints.FreezePositionY) == 0,
                Is.True);
            Assert.That(
                PlanarDistance(fixture.Player.transform.position, playerStart),
                Is.LessThan(0.03f));
        }

        [UnityTest]
        public IEnumerator NoTargetDuplicateTaskAndReset_AreDeterministicAndLeakFree()
        {
            Fixture fixture = CreateFixture(
                PrototypeWeightClass.Light,
                PrototypeInteractionRole.AllyBinder);
            Vector3[] baselines = fixture.AllTargets
                .Select(target => target.transform.position)
                .ToArray();

            fixture.View.transform.rotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
            Assert.That(fixture.Controller.ProcessAllyTask(), Is.False);
            Assert.That(fixture.Controller.LastAllyTaskRejection,
                Is.EqualTo(PrototypeAllyRelayRejection.NoValidSupply));
            Assert.That(fixture.Mobile.HasTaskMovement, Is.False);
            Assert.That(fixture.Relay.PreparedTarget, Is.Null);

            AimViewAt(fixture.View.transform, fixture.FetchTarget.transform.position);
            Assert.That(fixture.Controller.ProcessAllyTask(), Is.True);
            Assert.That(fixture.Controller.ProcessAllyTask(), Is.False);
            Assert.That(fixture.Controller.LastAllyTaskRejection,
                Is.EqualTo(PrototypeAllyRelayRejection.RelayAlreadyActive));
            Assert.That(fixture.Relay.PreparedTarget, Is.SameAs(fixture.FetchTarget));

            for (int frame = 0; frame < 12; frame++)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(fixture.Reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
            yield return null;
            yield return new WaitForFixedUpdate();

            Assert.That(fixture.Reset.BaselineObjectCount, Is.EqualTo(10));
            Assert.That(fixture.Reset.UnexpectedBaselineDifferenceCount, Is.Zero);
            Assert.That(fixture.Reset.ObjectDuplicationCount, Is.Zero);
            Assert.That(fixture.Reset.ObjectLossCount, Is.Zero);
            Assert.That(fixture.Reset.OwnershipLeakCount, Is.Zero);
            Assert.That(fixture.Relay.HasBaselineState, Is.True);
            Assert.That(fixture.Mobile.HasTaskMovement, Is.False);
            Assert.That(fixture.Owner.GlobalState, Is.EqualTo(PrototypeGlobalInteractionState.Playable));
            Assert.That(fixture.Owner.RelayState, Is.EqualTo(PrototypeRelayReservationState.None));
            for (int index = 0; index < fixture.AllTargets.Length; index++)
            {
                Assert.That(
                    Vector3.Distance(fixture.AllTargets[index].transform.position, baselines[index]),
                    Is.LessThan(0.002f),
                    fixture.AllTargets[index].RuntimeInstanceKey);
            }
        }

        [UnityTest]
        public IEnumerator SolidBlockedFetch_RejectsQWithoutMovingOrConsumingObject()
        {
            Fixture fixture = CreateFixture(
                PrototypeWeightClass.Light,
                PrototypeInteractionRole.AllyBinder);
            Vector3 objectStart = fixture.FetchTarget.transform.position;

            Assert.That(fixture.Controller.ProcessAllyTask(), Is.True);
            EncloseAlly(fixture.Ally.transform.position);
            Physics.SyncTransforms();
            for (int frame = 0; frame < 30; frame++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(fixture.Mobile.State, Is.EqualTo(ShowcaseMobileAllyState.Blocked));
            Assert.That(fixture.Relay.AllyState, Is.EqualTo(PrototypeAllySupportState.Blocked));
            Assert.That(fixture.Controller.ProcessCallAlly(), Is.False);
            Assert.That(fixture.Controller.LastAllyCallRejection,
                Is.EqualTo(PrototypeAllyRelayRejection.InvalidPhysicalLane));
            Assert.That(fixture.Relay.ReleaseCount, Is.Zero);
            Assert.That(fixture.Relay.ConsumedTargetCount, Is.Zero);
            Assert.That(
                Vector3.Distance(fixture.FetchTarget.transform.position, objectStart),
                Is.LessThan(0.002f));
            Assert.That(fixture.Owner.RelayState, Is.EqualTo(PrototypeRelayReservationState.None));
        }

        [UnityTest]
        public IEnumerator AuthoredS3Scene_FetchCompletesSupplyAndResetWithoutLeaks()
        {
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                S3ScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
            Assert.That(load, Is.Not.Null);
            while (!load.isDone)
            {
                yield return null;
            }

            Scene scene = SceneManager.GetSceneByPath(S3ScenePath);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);
            try
            {
                PrototypeInteractionController controller =
                    SceneComponents<PrototypeInteractionController>(scene).Single();
                PrototypeAllyRelayController relay =
                    SceneComponents<PrototypeAllyRelayController>(scene).Single();
                ShowcaseMobileAllyController mobile =
                    SceneComponents<ShowcaseMobileAllyController>(scene).Single();
                AuthoredEncounterResetController reset =
                    SceneComponents<AuthoredEncounterResetController>(scene).Single();
                PrototypeInteractionTarget[] targets =
                    SceneComponents<PrototypeInteractionTarget>(scene);

                Assert.That(controller.AllyRelayController, Is.SameAs(relay));
                PrototypeInteractionTarget selected = relay.SupplyTargets
                    .Where(target => target.Role == PrototypeInteractionRole.AllyBinder)
                    .OrderBy(target => PlanarDistance(
                        mobile.transform.position,
                        target.transform.position))
                    .ThenBy(target => target.RuntimeInstanceKey, StringComparer.Ordinal)
                    .First();
                Assert.That(relay.TryRequestFetch(selected, out var fetchRejection), Is.True,
                    fetchRejection.ToString());
                for (int frame = 0; frame < 2400 && !relay.IsReadyForCall; frame++)
                {
                    yield return new WaitForFixedUpdate();
                    Assert.That(mobile.TaskBlocked, Is.False,
                        $"Authored fetch route blocked at frame {frame} for " +
                        selected.RuntimeInstanceKey);
                }

                Assert.That(relay.IsReadyForCall, Is.True, selected.RuntimeInstanceKey);
                Assert.That(controller.ProcessCallAlly(), Is.True);
                for (int frame = 0; frame < 300 && relay.ReleaseCount == 0; frame++)
                {
                    yield return new WaitForFixedUpdate();
                }
                Assert.That(relay.ReleaseCount, Is.EqualTo(1));

                Assert.That(reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
                yield return null;
                yield return new WaitForFixedUpdate();
                Assert.That(reset.BaselineObjectCount, Is.EqualTo(10));
                Assert.That(reset.ObjectDuplicationCount, Is.Zero);
                Assert.That(reset.ObjectLossCount, Is.Zero);
                Assert.That(reset.OwnershipLeakCount, Is.Zero);
                Assert.That(targets, Has.Length.EqualTo(10));
                Assert.That(relay.HasBaselineState, Is.True);
                Assert.That(mobile.HasTaskMovement, Is.False);
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        private Fixture CreateFixture(
            PrototypeWeightClass weight,
            PrototypeInteractionRole role)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Track(floor);
            floor.name = "Floor";
            floor.transform.SetPositionAndRotation(new Vector3(0f, -0.25f, 0f), Quaternion.identity);
            floor.transform.localScale = new Vector3(40f, 0.5f, 40f);

            GameObject player = Track(new GameObject("Player"));
            player.transform.SetPositionAndRotation(new Vector3(-4f, 1f, 0f), Quaternion.identity);
            CharacterController playerController = player.AddComponent<CharacterController>();
            playerController.center = Vector3.zero;
            playerController.height = 2f;
            playerController.radius = 0.45f;
            PrototypeInteractionStateOwner owner =
                player.AddComponent<PrototypeInteractionStateOwner>();
            PrototypePhysicalObjectLifecycle lifecycle =
                player.AddComponent<PrototypePhysicalObjectLifecycle>();
            Transform carryAnchor = Track(new GameObject("CarryAnchor")).transform;
            carryAnchor.SetParent(player.transform, false);
            carryAnchor.localPosition = new Vector3(0f, 1f, 1f);
            lifecycle.Configure(owner, carryAnchor);
            AuthoredEncounterResetController reset =
                player.AddComponent<AuthoredEncounterResetController>();

            GameObject view = Track(new GameObject("View"));
            view.transform.position = new Vector3(-4f, 1.5f, -2f);

            GameObject ally = Track(new GameObject("Ally"));
            ally.transform.SetPositionAndRotation(new Vector3(-1f, 1f, -3f), Quaternion.identity);
            CharacterController allyCharacter = ally.AddComponent<CharacterController>();
            allyCharacter.center = Vector3.zero;
            allyCharacter.height = 1.9f;
            allyCharacter.radius = 0.48f;
            ShowcaseMobileAllyController mobile =
                ally.AddComponent<ShowcaseMobileAllyController>();
            mobile.Configure(
                owner,
                reset,
                ally.transform,
                player.transform,
                allyCharacter,
                new Transform[0],
                new Vector2Int[0],
                authoredNormalSpeed: 12f,
                authoredCatchUpSpeed: 12f,
                authoredFastCatchUpSpeed: 12f);

            PrototypeInteractionTarget fetchTarget = CreateTarget(
                "FETCH-01",
                weight,
                role,
                new Vector3(0f, weight == PrototypeWeightClass.Light ? 0.16f : 0.499f, 4f));
            PrototypeInteractionTarget[] allTargets = new PrototypeInteractionTarget[10];
            allTargets[0] = fetchTarget;
            for (int index = 1; index < allTargets.Length; index++)
            {
                allTargets[index] = CreateTarget(
                    $"RESET-{index:00}",
                    PrototypeWeightClass.Light,
                    PrototypeInteractionRole.DirectBinder,
                    new Vector3(8f + index, 0.16f, -8f));
            }

            PrototypeAllyRelayController relay =
                ally.AddComponent<PrototypeAllyRelayController>();
            relay.Configure(
                owner,
                lifecycle,
                ally.transform,
                player.transform,
                new[] { fetchTarget },
                authoredPreReleaseDelay: 0.02f,
                authoredBinderHorizontalSpeed: 8f,
                authoredChairFeedSpeed: 4f,
                authoredMaximumRelayDuration: 3f,
                authoredAllyMoveSpeed: 12f,
                authoredSupplyAcquisitionSpeed: 12f);
            relay.ConfigureShowcaseFetch(mobile, new[] { fetchTarget }, true);

            PrototypeInteractionController interaction =
                player.AddComponent<PrototypeInteractionController>();
            interaction.Configure(
                null,
                owner,
                view.transform,
                allTargets,
                2.8f,
                false,
                lifecycle,
                relay);
            interaction.ConfigureShowcaseS3(mobile, relay, allTargets);

            GameObject runnerOne = Track(new GameObject("RunnerOne"));
            GameObject runnerTwo = Track(new GameObject("RunnerTwo"));
            reset.Configure(
                null,
                owner,
                lifecycle,
                null,
                player.transform,
                new[] { ally.transform, runnerOne.transform, runnerTwo.transform },
                allTargets,
                relay,
                10);

            Physics.SyncTransforms();
            AimViewAt(view.transform, fetchTarget.transform.position);
            return new Fixture(
                player,
                ally,
                view,
                owner,
                lifecycle,
                interaction,
                mobile,
                relay,
                reset,
                fetchTarget,
                fetchTarget.GetComponent<Rigidbody>(),
                allTargets);
        }

        private static T[] SceneComponents<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }

        private PrototypeInteractionTarget CreateTarget(
            string key,
            PrototypeWeightClass weight,
            PrototypeInteractionRole role,
            Vector3 position)
        {
            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Track(item);
            item.name = key;
            item.transform.SetPositionAndRotation(position, Quaternion.identity);
            item.transform.localScale = weight == PrototypeWeightClass.Medium
                ? new Vector3(0.729f, 0.998f, 0.697f)
                : new Vector3(0.29f, 0.32f, 0.08f);
            Rigidbody body = item.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            PrototypeInteractionTarget target = item.AddComponent<PrototypeInteractionTarget>();
            bool allyBinder = role == PrototypeInteractionRole.AllyBinder;
            bool allyChair = role == PrototypeInteractionRole.AllyChair;
            target.Configure(
                weight == PrototypeWeightClass.Medium ? "OBJ-P-M-001" : "OBJ-P-L-001",
                key,
                weight == PrototypeWeightClass.Medium ? "OFFICE CHAIR" : "FILE BINDER",
                weight,
                role,
                role == PrototypeInteractionRole.DirectBinder ||
                role == PrototypeInteractionRole.DirectChair,
                allyBinder,
                allyChair || role == PrototypeInteractionRole.DirectChair,
                allyChair || role == PrototypeInteractionRole.DirectChair);
            return target;
        }

        private IEnumerator WaitForReady(Fixture fixture)
        {
            for (int frame = 0; frame < 360; frame++)
            {
                yield return new WaitForFixedUpdate();
                Assert.That(fixture.Relay.AllyState,
                    Is.Not.EqualTo(PrototypeAllySupportState.Blocked),
                    $"Fetch blocked at frame {frame}.");
                if (fixture.Relay.IsReadyForCall)
                {
                    yield break;
                }
            }

            Assert.Fail("Ally did not acquire and return the requested supply.");
        }

        private IEnumerator WaitForRelease(Fixture fixture)
        {
            for (int frame = 0; frame < 30; frame++)
            {
                yield return new WaitForFixedUpdate();
                if (fixture.Relay.ReleaseCount > 0)
                {
                    yield break;
                }
            }

            Assert.Fail("Q did not release the prepared supply.");
        }

        private void EncloseAlly(Vector3 center)
        {
            CreateBlocker(center + new Vector3(0f, 1.5f, 1.1f), new Vector3(3f, 3f, 0.2f));
            CreateBlocker(center + new Vector3(0f, 1.5f, -1.1f), new Vector3(3f, 3f, 0.2f));
            CreateBlocker(center + new Vector3(1.1f, 1.5f, 0f), new Vector3(0.2f, 3f, 3f));
            CreateBlocker(center + new Vector3(-1.1f, 1.5f, 0f), new Vector3(0.2f, 3f, 3f));
        }

        private void CreateBlocker(Vector3 position, Vector3 scale)
        {
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Track(blocker);
            blocker.transform.SetPositionAndRotation(position, Quaternion.identity);
            blocker.transform.localScale = scale;
        }

        private GameObject Track(GameObject item)
        {
            createdObjects.Add(item);
            return item;
        }

        private static void AimViewAt(Transform view, Vector3 target)
        {
            view.rotation = Quaternion.LookRotation(target - view.position, Vector3.up);
        }

        private static float PlanarDistance(Vector3 left, Vector3 right)
        {
            return Vector2.Distance(
                new Vector2(left.x, left.z),
                new Vector2(right.x, right.z));
        }

        private sealed class Fixture
        {
            public Fixture(
                GameObject player,
                GameObject ally,
                GameObject view,
                PrototypeInteractionStateOwner owner,
                PrototypePhysicalObjectLifecycle lifecycle,
                PrototypeInteractionController controller,
                ShowcaseMobileAllyController mobile,
                PrototypeAllyRelayController relay,
                AuthoredEncounterResetController reset,
                PrototypeInteractionTarget fetchTarget,
                Rigidbody fetchBody,
                PrototypeInteractionTarget[] allTargets)
            {
                Player = player;
                Ally = ally;
                View = view;
                Owner = owner;
                Lifecycle = lifecycle;
                Controller = controller;
                Mobile = mobile;
                Relay = relay;
                Reset = reset;
                FetchTarget = fetchTarget;
                FetchBody = fetchBody;
                AllTargets = allTargets;
            }

            public GameObject Player { get; }
            public GameObject Ally { get; }
            public GameObject View { get; }
            public PrototypeInteractionStateOwner Owner { get; }
            public PrototypePhysicalObjectLifecycle Lifecycle { get; }
            public PrototypeInteractionController Controller { get; }
            public ShowcaseMobileAllyController Mobile { get; }
            public PrototypeAllyRelayController Relay { get; }
            public AuthoredEncounterResetController Reset { get; }
            public PrototypeInteractionTarget FetchTarget { get; }
            public Rigidbody FetchBody { get; }
            public PrototypeInteractionTarget[] AllTargets { get; }
        }
    }
}
