using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BreakNotice.Gameplay.Encounter;
using BreakNotice.Gameplay.Input;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestShowcaseMobileAllyPlayModeTests
    {
        [UnityTest]
        public IEnumerator MobileAlly_HoldsUntilRegroup_ThenCatchesUpWithoutTeleport()
        {
            using var fixture = MobileFixture.Create(
                new Vector3(0f, 1f, 0f),
                new Vector3(0f, 1f, 10f));
            Vector3 authoredHoldPosition = fixture.Ally.transform.position;

            for (int step = 0; step < 20; step++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(fixture.Mobile.State,
                Is.EqualTo(ShowcaseMobileAllyState.HoldingPosition));
            Assert.That(fixture.Mobile.IsFollowCommanded, Is.False);
            Assert.That(PlanarDistance(authoredHoldPosition, fixture.Ally.transform.position),
                Is.LessThan(0.001f));
            Assert.That(fixture.Mobile.TotalPathLength, Is.Zero.Within(0.001f));

            Assert.That(fixture.Mobile.TryToggleFollow(), Is.True);
            Assert.That(fixture.Mobile.IsFollowCommanded, Is.True);
            bool reachedFollowTarget = false;

            for (int step = 0; step < 180; step++)
            {
                Vector3 before = fixture.Ally.transform.position;
                yield return new WaitForFixedUpdate();
                float stepDistance = PlanarDistance(before, fixture.Ally.transform.position);
                Assert.That(
                    stepDistance,
                    Is.LessThanOrEqualTo(fixture.Mobile.FastCatchUpSpeed * Time.fixedDeltaTime + 0.002f),
                    $"step {step}");

                if (PlanarDistance(
                        fixture.Ally.transform.position,
                        fixture.Mobile.CalculateFollowPosition(fixture.Mobile.FollowSide)) <= 0.25f)
                {
                    reachedFollowTarget = true;
                    break;
                }
            }

            Assert.That(reachedFollowTarget, Is.True);
            Assert.That(fixture.Mobile.TotalPathLength, Is.GreaterThan(5f));
            Assert.That(
                fixture.Mobile.MaximumObservedStepDistance,
                Is.LessThanOrEqualTo(fixture.Mobile.FastCatchUpSpeed * Time.fixedDeltaTime + 0.002f));
            Assert.That(
                PlanarDistance(fixture.Ally.transform.position, fixture.Player.transform.position),
                Is.InRange(2.5f, 3.2f));
            Assert.That(fixture.Ally.transform.position.y, Is.InRange(0.94f, 1.01f));

            Assert.That(fixture.Mobile.TryToggleFollow(), Is.True);
            Assert.That(fixture.Mobile.IsFollowCommanded, Is.False);
            Vector3 heldPosition = fixture.Ally.transform.position;
            fixture.Player.transform.position += Vector3.forward * 5f;
            Physics.SyncTransforms();
            for (int step = 0; step < 20; step++)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(fixture.Mobile.State,
                Is.EqualTo(ShowcaseMobileAllyState.HoldingPosition));
            Assert.That(PlanarDistance(heldPosition, fixture.Ally.transform.position),
                Is.LessThan(0.001f));
        }

        [UnityTest]
        public IEnumerator MobileAlly_UsesOppositeRearSide_WhenPreferredPathIsBlocked()
        {
            using var fixture = MobileFixture.Create(
                new Vector3(0f, 1f, 0f),
                new Vector3(0f, 1f, 6f));
            fixture.CreateObstacle(
                "PreferredSideBlocker",
                new Vector3(-0.45f, 1f, 1.7f),
                new Vector3(0.25f, 2f, 0.8f));
            Physics.SyncTransforms();

            Assert.That(fixture.Mobile.TryToggleFollow(), Is.True);

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.That(fixture.Mobile.FollowSide, Is.EqualTo(ShowcaseAllyFollowSide.Right));
            Assert.That(fixture.Mobile.SideSwitchCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(fixture.Mobile.LastStepDistance, Is.GreaterThan(0f));
            Assert.That(fixture.Mobile.State, Is.Not.EqualTo(ShowcaseMobileAllyState.Blocked));
            Assert.That(fixture.Mobile.MaximumObservedStepDistance,
                Is.LessThanOrEqualTo(fixture.Mobile.CatchUpSpeed * Time.fixedDeltaTime + 0.002f));
        }

        [UnityTest]
        public IEnumerator MobileAlly_BlockedFollowToggleRejectsSafely_WithoutMovement()
        {
            using var fixture = MobileFixture.Create(
                new Vector3(0f, 1f, 0f),
                new Vector3(0f, 1f, 6f));
            fixture.CreateObstacle("NorthBlocker", new Vector3(0f, 1f, 0.8f),
                new Vector3(2f, 2f, 0.2f));
            fixture.CreateObstacle("SouthBlocker", new Vector3(0f, 1f, -0.8f),
                new Vector3(2f, 2f, 0.2f));
            fixture.CreateObstacle("EastBlocker", new Vector3(0.8f, 1f, 0f),
                new Vector3(0.2f, 2f, 2f));
            fixture.CreateObstacle("WestBlocker", new Vector3(-0.8f, 1f, 0f),
                new Vector3(0.2f, 2f, 2f));
            Physics.SyncTransforms();
            Vector3 baseline = fixture.Ally.transform.position;

            for (int step = 0; step < 20; step++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(fixture.Mobile.State,
                Is.EqualTo(ShowcaseMobileAllyState.HoldingPosition));
            Assert.That(fixture.Mobile.IsFollowCommanded, Is.False);
            Assert.That(fixture.Mobile.BlockedCount, Is.Zero);
            Assert.That(PlanarDistance(baseline, fixture.Ally.transform.position),
                Is.LessThan(0.001f));

            bool accepted = fixture.Mobile.TryToggleFollow();

            Assert.That(accepted, Is.False);
            Assert.That(fixture.Mobile.FollowToggleAttemptSequence, Is.EqualTo(1));
            Assert.That(fixture.Mobile.LastFollowToggleAccepted, Is.False);
            Assert.That(fixture.Mobile.IsFollowCommanded, Is.False);
            Assert.That(fixture.Mobile.State,
                Is.EqualTo(ShowcaseMobileAllyState.HoldingPosition));
            Assert.That(fixture.Mobile.FollowSide, Is.EqualTo(ShowcaseAllyFollowSide.Left));
            Assert.That(PlanarDistance(baseline, fixture.Ally.transform.position),
                Is.LessThan(0.001f));
        }

        [UnityTest]
        public IEnumerator QRoutesToFollowToggle_GRoutesToRegroup_AndResetRestoresHold()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "Contest_SingleRoomGraybox",
                LoadSceneMode.Additive);
            yield return load;
            yield return null;

            Scene scene = SceneManager.GetSceneByName("Contest_SingleRoomGraybox");
            AsyncOperation unload = null;
            try
            {
                GameObject[] roots = scene.GetRootGameObjects();
                GameObject player = roots.Single(root => root.name == "Player");
                GameObject ally = roots.Single(root => root.name == "AllyProxy");
                PrototypeInteractionStateOwner owner =
                    player.GetComponent<PrototypeInteractionStateOwner>();
                PrototypeInteractionController interaction =
                    player.GetComponent<PrototypeInteractionController>();
                AuthoredEncounterResetController reset = roots
                    .SelectMany(root =>
                        root.GetComponentsInChildren<AuthoredEncounterResetController>(true))
                    .Single();
                InputSystemLogicalInputAdapter input = roots
                    .SelectMany(root =>
                        root.GetComponentsInChildren<InputSystemLogicalInputAdapter>(true))
                    .Single();
                CapsuleCollider legacyCollider = ally.GetComponent<CapsuleCollider>();
                legacyCollider.enabled = false;
                CharacterController allyController = ally.AddComponent<CharacterController>();
                allyController.center = Vector3.zero;
                allyController.radius = 0.48f;
                allyController.height = 1.9f;
                ShowcaseMobileAllyController mobile =
                    ally.AddComponent<ShowcaseMobileAllyController>();
                mobile.Configure(
                    owner,
                    reset,
                    ally.transform,
                    player.transform,
                    allyController,
                    Array.Empty<Transform>(),
                    Array.Empty<Vector2Int>());
                interaction.ConfigureShowcaseMobileAlly(mobile);
                Vector3 allyBaseline = ally.transform.position;
                uint legacyCallSequence = interaction.AllyCallAttemptSequence;

                SetSnapshot(input, new LogicalInputSnapshot(
                    Vector2.zero,
                    Vector2.zero,
                    false,
                    false,
                    false,
                    true,
                    false,
                    false));
                interaction.SendMessage("LateUpdate", SendMessageOptions.DontRequireReceiver);
                SetSnapshot(input, default);

                Assert.That(mobile.FollowToggleAttemptSequence, Is.EqualTo(1));
                Assert.That(mobile.RegroupAttemptSequence, Is.Zero);
                Assert.That(mobile.LastFollowToggleAccepted,
                    Is.EqualTo(mobile.IsFollowCommanded));
                Assert.That(interaction.AllyCallAttemptSequence, Is.EqualTo(legacyCallSequence));

                SetSnapshot(input, new LogicalInputSnapshot(
                    Vector2.zero,
                    Vector2.zero,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    regroupAlly: true));
                interaction.SendMessage("LateUpdate", SendMessageOptions.DontRequireReceiver);
                SetSnapshot(input, default);

                Assert.That(mobile.RegroupAttemptSequence, Is.EqualTo(1));
                Assert.That(mobile.FollowToggleAttemptSequence, Is.EqualTo(1));

                allyController.enabled = false;
                ally.transform.position += new Vector3(1.5f, 0f, 0f);
                allyController.enabled = true;
                Physics.SyncTransforms();
                Assert.That(PlanarDistance(ally.transform.position, allyBaseline),
                    Is.GreaterThan(1f));

                Assert.That(reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
                yield return null;
                yield return new WaitForFixedUpdate();

                Assert.That(reset.ResetCount, Is.EqualTo(1));
                Assert.That(owner.GlobalState, Is.EqualTo(PrototypeGlobalInteractionState.Playable));
                Assert.That(PlanarDistance(ally.transform.position, allyBaseline),
                    Is.LessThanOrEqualTo(mobile.NormalSpeed * Time.fixedDeltaTime + 0.002f));
                Assert.That(Mathf.Abs(ally.transform.position.y - allyBaseline.y),
                    Is.LessThanOrEqualTo(allyController.skinWidth + 0.001f));
                Assert.That(mobile.State,
                    Is.EqualTo(ShowcaseMobileAllyState.HoldingPosition));
                Assert.That(mobile.IsFollowCommanded, Is.False);
                Assert.That(mobile.FollowSide, Is.EqualTo(ShowcaseAllyFollowSide.Left));
                Assert.That(mobile.RegroupAttemptSequence, Is.Zero);
                Assert.That(mobile.FollowToggleAttemptSequence, Is.Zero);
                Assert.That(mobile.SideSwitchCount, Is.Zero);
                Assert.That(mobile.AnchorRouteCount, Is.Zero);
                Assert.That(mobile.BlockedCount, Is.Zero);
                Assert.That(mobile.TotalPathLength, Is.Zero.Within(0.001f));
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

        private static void SetSnapshot(
            InputSystemLogicalInputAdapter input,
            LogicalInputSnapshot snapshot)
        {
            PropertyInfo property = typeof(InputSystemLogicalInputAdapter).GetProperty(
                nameof(InputSystemLogicalInputAdapter.Snapshot),
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(property, Is.Not.Null);
            MethodInfo setter = property.GetSetMethod(true);
            Assert.That(setter, Is.Not.Null);
            setter.Invoke(input, new object[] { snapshot });
        }

        private static float PlanarDistance(Vector3 first, Vector3 second)
        {
            first.y = 0f;
            second.y = 0f;
            return Vector3.Distance(first, second);
        }

        private sealed class MobileFixture : IDisposable
        {
            private readonly List<GameObject> created = new List<GameObject>();

            private MobileFixture()
            {
            }

            public GameObject Player { get; private set; }

            public GameObject Ally { get; private set; }

            public ShowcaseMobileAllyController Mobile { get; private set; }

            public static MobileFixture Create(Vector3 allyPosition, Vector3 playerPosition)
            {
                var fixture = new MobileFixture();
                fixture.CreateObstacle(
                    "Floor",
                    new Vector3(0f, -0.25f, 4f),
                    new Vector3(30f, 0.5f, 30f));
                fixture.Player = fixture.CreateObject("Player", playerPosition);
                PrototypeInteractionStateOwner owner =
                    fixture.Player.AddComponent<PrototypeInteractionStateOwner>();
                fixture.Ally = fixture.CreateObject("Ally", allyPosition);
                CharacterController characterController =
                    fixture.Ally.AddComponent<CharacterController>();
                characterController.center = Vector3.zero;
                characterController.radius = 0.48f;
                characterController.height = 1.9f;
                characterController.skinWidth = 0.06f;
                fixture.Mobile = fixture.Ally.AddComponent<ShowcaseMobileAllyController>();
                fixture.Mobile.Configure(
                    owner,
                    null,
                    fixture.Ally.transform,
                    fixture.Player.transform,
                    characterController,
                    Array.Empty<Transform>(),
                    Array.Empty<Vector2Int>());
                Physics.SyncTransforms();
                return fixture;
            }

            public GameObject CreateObstacle(string name, Vector3 position, Vector3 scale)
            {
                GameObject obstacle = CreateObject(name, position);
                BoxCollider collider = obstacle.AddComponent<BoxCollider>();
                collider.size = scale;
                return obstacle;
            }

            public void Dispose()
            {
                for (int index = created.Count - 1; index >= 0; index--)
                {
                    if (created[index] != null)
                    {
                        UnityEngine.Object.DestroyImmediate(created[index]);
                    }
                }
                Physics.SyncTransforms();
            }

            private GameObject CreateObject(string name, Vector3 position)
            {
                var gameObject = new GameObject(name);
                gameObject.transform.position = position;
                created.Add(gameObject);
                return gameObject;
            }
        }
    }
}
