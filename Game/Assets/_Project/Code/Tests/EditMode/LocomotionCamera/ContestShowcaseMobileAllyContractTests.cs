using System;
using System.Collections.Generic;
using System.Linq;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestShowcaseMobileAllyContractTests
    {
        private const string ShowcaseScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v001.unity";

        private Scene scene;

        [SetUp]
        public void SetUp()
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(ShowcaseScenePath), Is.Not.Null);
            scene = EditorSceneManager.OpenScene(ShowcaseScenePath, OpenSceneMode.Additive);
        }

        [TearDown]
        public void TearDown()
        {
            if (scene.IsValid() && scene.isLoaded)
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void ShowcaseScene_UsesOneMobileAlly_AndKeepsLegacyRelayDisabledOnlyHere()
        {
            GameObject player = RequireRoot("Player");
            GameObject ally = RequireRoot("AllyProxy");
            GameObject browserInput = RequireRoot("ContestBrowserInput");
            ShowcaseMobileAllyController mobile =
                ally.GetComponent<ShowcaseMobileAllyController>();
            PrototypeAllyRelayController relay =
                ally.GetComponent<PrototypeAllyRelayController>();
            CharacterController characterController = ally.GetComponent<CharacterController>();
            CapsuleCollider legacyCollider = ally.GetComponent<CapsuleCollider>();
            PrototypeInteractionController interaction =
                player.GetComponent<PrototypeInteractionController>();
            ShowcaseMobileAllyPresenter presenter =
                browserInput.GetComponent<ShowcaseMobileAllyPresenter>();

            Assert.That(mobile, Is.Not.Null);
            Assert.That(mobile.enabled, Is.True);
            Assert.That(relay, Is.Not.Null);
            Assert.That(relay.enabled, Is.False);
            Assert.That(characterController, Is.Not.Null);
            Assert.That(characterController.enabled, Is.True);
            Assert.That(characterController.radius, Is.EqualTo(0.48f).Within(0.001f));
            Assert.That(characterController.height, Is.EqualTo(1.9f).Within(0.001f));
            Assert.That(legacyCollider, Is.Not.Null);
            Assert.That(legacyCollider.enabled, Is.False);
            Assert.That(interaction.ShowcaseMobileAllyController, Is.SameAs(mobile));
            Assert.That(presenter, Is.Not.Null);
            Assert.That(presenter.MobileAlly, Is.SameAs(mobile));
            Assert.That(presenter.BuildStatusText(), Is.EqualTo("ALLY FOLLOWING | [Q] REGROUP"));
            Assert.That(
                scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<ShowcaseMobileAllyController>(true))
                    .Count(),
                Is.EqualTo(1));
        }

        [Test]
        public void FollowThresholdsAndSpeeds_MatchOwnerLockedS2Values()
        {
            ShowcaseMobileAllyController mobile = RequireMobileAlly();

            Assert.That(mobile.FollowDistance, Is.EqualTo(2.7f).Within(0.001f));
            Assert.That(mobile.LateralOffset, Is.EqualTo(0.85f).Within(0.001f));
            Assert.That(mobile.CatchUpDistance, Is.EqualTo(5.5f).Within(0.001f));
            Assert.That(mobile.FastCatchUpDistance, Is.EqualTo(8f).Within(0.001f));
            Assert.That(mobile.MaximumNormalDistance, Is.EqualTo(12f).Within(0.001f));
            Assert.That(mobile.NormalSpeed, Is.EqualTo(2.8f).Within(0.001f));
            Assert.That(mobile.CatchUpSpeed, Is.EqualTo(4.2f).Within(0.001f));
            Assert.That(mobile.FastCatchUpSpeed, Is.EqualTo(6f).Within(0.001f));

            Assert.That(mobile.EvaluateMovementState(5.499f),
                Is.EqualTo(ShowcaseMobileAllyState.Following));
            Assert.That(mobile.EvaluateMovementState(5.5f),
                Is.EqualTo(ShowcaseMobileAllyState.CatchingUp));
            Assert.That(mobile.EvaluateMovementState(7.999f),
                Is.EqualTo(ShowcaseMobileAllyState.CatchingUp));
            Assert.That(mobile.EvaluateMovementState(8f),
                Is.EqualTo(ShowcaseMobileAllyState.FastCatchingUp));
            Assert.That(mobile.SelectSpeed(4f), Is.EqualTo(2.8f).Within(0.001f));
            Assert.That(mobile.SelectSpeed(6f), Is.EqualTo(4.2f).Within(0.001f));
            Assert.That(mobile.SelectSpeed(12f), Is.EqualTo(6f).Within(0.001f));
        }

        [Test]
        public void FollowPositions_AreBehindPlayer_OnDistinctLeftAndRightSides()
        {
            ShowcaseMobileAllyController mobile = RequireMobileAlly();
            Transform player = RequireRoot("Player").transform;
            Vector3 forward = player.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            Vector3 left = mobile.CalculateFollowPosition(ShowcaseAllyFollowSide.Left);
            Vector3 rightPosition = mobile.CalculateFollowPosition(ShowcaseAllyFollowSide.Right);
            Vector3 leftOffset = Flatten(left - player.position);
            Vector3 rightOffset = Flatten(rightPosition - player.position);

            Assert.That(leftOffset.magnitude, Is.InRange(2.5f, 3.2f));
            Assert.That(rightOffset.magnitude, Is.InRange(2.5f, 3.2f));
            Assert.That(Vector3.Dot(leftOffset.normalized, forward), Is.LessThan(0f));
            Assert.That(Vector3.Dot(rightOffset.normalized, forward), Is.LessThan(0f));
            Assert.That(Vector3.Dot(leftOffset, right), Is.LessThan(0f));
            Assert.That(Vector3.Dot(rightOffset, right), Is.GreaterThan(0f));
        }

        [Test]
        public void FollowAnchorNetwork_HasExactTwelveOwnerPositions_AndConnectedLinks()
        {
            ShowcaseMobileAllyController mobile = RequireMobileAlly();
            Vector2[] expected =
            {
                new Vector2(-2f, -14f),
                new Vector2(-3f, -10f),
                new Vector2(-4f, -5f),
                new Vector2(0f, 3f),
                new Vector2(-14f, -5f),
                new Vector2(-17f, 7f),
                new Vector2(-7f, 12f),
                new Vector2(12f, -4f),
                new Vector2(14f, 5f),
                new Vector2(11f, 9f),
                new Vector2(14f, 11f),
                new Vector2(19f, 15f)
            };

            Assert.That(mobile.AnchorCount, Is.EqualTo(expected.Length));
            for (int index = 0; index < expected.Length; index++)
            {
                Transform anchor = mobile.FollowAnchors[index];
                Assert.That(anchor, Is.Not.Null, $"FA-{index:00}");
                Assert.That(anchor.name, Is.EqualTo($"FA-{index:00}"));
                Assert.That(anchor.position.x, Is.EqualTo(expected[index].x).Within(0.001f));
                Assert.That(anchor.position.z, Is.EqualTo(expected[index].y).Within(0.001f));
            }

            Assert.That(mobile.AnchorLinks.Count, Is.EqualTo(13));
            var uniqueLinks = new HashSet<string>(StringComparer.Ordinal);
            foreach (Vector2Int link in mobile.AnchorLinks)
            {
                Assert.That(link.x, Is.InRange(0, expected.Length - 1));
                Assert.That(link.y, Is.InRange(0, expected.Length - 1));
                Assert.That(link.x, Is.Not.EqualTo(link.y));
                string key = $"{Math.Min(link.x, link.y)}-{Math.Max(link.x, link.y)}";
                Assert.That(uniqueLinks.Add(key), Is.True, key);
            }
            Assert.That(mobile.IsAnchorNetworkConnected(), Is.True);
        }

        private ShowcaseMobileAllyController RequireMobileAlly()
        {
            ShowcaseMobileAllyController mobile =
                RequireRoot("AllyProxy").GetComponent<ShowcaseMobileAllyController>();
            Assert.That(mobile, Is.Not.Null);
            return mobile;
        }

        private GameObject RequireRoot(string name)
        {
            GameObject root = scene.GetRootGameObjects().SingleOrDefault(candidate =>
                string.Equals(candidate.name, name, StringComparison.Ordinal));
            Assert.That(root, Is.Not.Null, name);
            return root;
        }

        private static Vector3 Flatten(Vector3 value)
        {
            value.y = 0f;
            return value;
        }
    }
}
