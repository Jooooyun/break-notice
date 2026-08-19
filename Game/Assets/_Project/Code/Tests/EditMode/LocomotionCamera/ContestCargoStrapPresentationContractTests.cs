using System.IO;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestCargoStrapPresentationContractTests
    {
        private GameObject root;

        [TearDown]
        public void TearDown()
        {
            if (root != null)
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ConfigureBuildsBlackYellowReviewStrapAndSilverColliderlessClamp()
        {
            root = new GameObject("STRAP_PRESENTATION_TEST");
            Transform reel = CreateSocket("REEL", new Vector3(0.2f, 0.9f, 0f));
            Transform hand = CreateSocket("HAND", new Vector3(0.4f, 1.2f, 0.2f));
            GameObject lineObject = new GameObject("STRAP_LINE");
            lineObject.transform.SetParent(root.transform, false);
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            LineRenderer tether = CreateLine("OBJECT_TETHER");
            GameObject clamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            clamp.name = "REVIEW_CLAMP";
            clamp.transform.SetParent(root.transform, false);
            Object.DestroyImmediate(clamp.GetComponent<Collider>());

            ShowcaseCargoStrapPresentation presentation =
                root.AddComponent<ShowcaseCargoStrapPresentation>();
            presentation.Configure(
                null,
                reel,
                hand,
                line,
                clamp.transform,
                tether);

            Assert.That(presentation.ReelSocket, Is.SameAs(reel));
            Assert.That(presentation.RightHandSocket, Is.SameAs(hand));
            Assert.That(presentation.StrapLine, Is.SameAs(line));
            Assert.That(presentation.ObjectTetherLine, Is.SameAs(tether));
            Assert.That(presentation.ReviewClampMarker, Is.SameAs(clamp.transform));
            Assert.That(line.positionCount, Is.EqualTo(12));
            Assert.That(line.startWidth,
                Is.EqualTo(ShowcaseCargoStrapPresentation.StrapWorldWidthMeters)
                    .Within(0.0001f));
            Assert.That(line.endWidth,
                Is.EqualTo(ShowcaseCargoStrapPresentation.StrapWorldWidthMeters)
                    .Within(0.0001f));
            Assert.That(line.textureMode, Is.EqualTo(LineTextureMode.Tile));
            Assert.That(line.sharedMaterial, Is.Not.Null);
            Assert.That(line.sharedMaterial.mainTexture, Is.Not.Null);
            Assert.That(line.sharedMaterial.mainTexture.wrapMode,
                Is.EqualTo(TextureWrapMode.Repeat));
            Assert.That(ShowcaseCargoStrapPresentation.StrapYellowColor,
                Is.EqualTo(new Color32(184, 151, 64, 255)));
            Assert.That(ShowcaseCargoStrapPresentation.StrapBlackColor,
                Is.EqualTo(new Color32(28, 29, 26, 255)));
            Assert.That(line.enabled, Is.False);
            Assert.That(clamp.activeSelf, Is.False);
            Assert.That(clamp.GetComponentInChildren<Collider>(true), Is.Null);
            var clampProperties = new MaterialPropertyBlock();
            clamp.GetComponent<Renderer>().GetPropertyBlock(clampProperties);
            Color silver = clampProperties.GetColor(
                Shader.PropertyToID("_BaseColor"));
            Assert.That(silver.r, Is.EqualTo(0.72f).Within(0.001f));
            Assert.That(silver.g, Is.EqualTo(0.76f).Within(0.001f));
            Assert.That(silver.b, Is.EqualTo(0.8f).Within(0.001f));
        }

        [Test]
        public void PresentationSourceHasNoGameplayPhysicsWrites()
        {
            string source = File.ReadAllText(
                "Assets/_Project/Code/Runtime/Presentation/" +
                "ShowcaseCargoStrapPresentation.cs");

            StringAssert.DoesNotContain("Rigidbody", source);
            StringAssert.DoesNotContain("Joint", source);
            StringAssert.DoesNotContain("AddForce", source);
            StringAssert.DoesNotContain("TryCommit", source);
            StringAssert.DoesNotContain("TryReleaseStrap", source);
            StringAssert.DoesNotContain("TryBeginStrapTension", source);
            StringAssert.Contains("ReleaseRetractSeconds = 0.2f", source);
        }

        [Test]
        public void SnapshotDrivesHandGuideFistBypassAndReleaseRetraction()
        {
            root = new GameObject("STRAP_PATH_TEST");
            Transform reel = CreateSocket("REEL", new Vector3(0f, 1f, 0f));
            Transform hand = CreateSocket("HAND", new Vector3(0.5f, 1f, 0f));
            var lineObject = new GameObject("STRAP_LINE");
            lineObject.transform.SetParent(root.transform, false);
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            LineRenderer tether = CreateLine("OBJECT_TETHER");
            GameObject clamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            clamp.transform.SetParent(root.transform, false);
            Object.DestroyImmediate(clamp.GetComponent<Collider>());
            ShowcaseCargoStrapPresentation presentation =
                root.AddComponent<ShowcaseCargoStrapPresentation>();
            presentation.Configure(
                null,
                reel,
                hand,
                line,
                clamp.transform,
                tether);

            presentation.Present(CreateSnapshot(
                ShowcaseCargoStrapPhase.Tensioned,
                1f,
                new Vector3(3f, 1f, 0f),
                PrototypeWeaponSelection.ObjectWeapon));
            Assert.That(line.enabled, Is.True);
            Assert.That(line.GetPosition(0), Is.EqualTo(reel.position));
            Assert.That(presentation.LastGripPointIndex, Is.GreaterThan(0));
            Assert.That(
                line.GetPosition(presentation.LastGripPointIndex),
                Is.EqualTo(presentation.LastGripWorldPosition));
            Assert.That(presentation.LastRouteOutwardClearanceMeters,
                Is.GreaterThan(0.015f));
            Assert.That(line.GetPosition(line.positionCount - 1),
                Is.EqualTo(presentation.LastGripWorldPosition));
            Assert.That(Vector3.Distance(
                    presentation.LastGripWorldPosition,
                    hand.position),
                Is.InRange(0.025f, 0.04f));
            Assert.That(tether.enabled, Is.True);
            Assert.That(Vector3.Distance(tether.GetPosition(0), hand.position),
                Is.InRange(0.025f, 0.05f));
            Assert.That(tether.GetPosition(tether.positionCount - 1),
                Is.EqualTo(new Vector3(3f, 1f, 0f)));
            Assert.That(clamp.transform.position,
                Is.EqualTo(new Vector3(3f, 1f, 0f)));

            presentation.Present(CreateSnapshot(
                ShowcaseCargoStrapPhase.Tensioned,
                1f,
                new Vector3(3f, 1f, 0f),
                PrototypeWeaponSelection.Fists));
            Assert.That(line.GetPosition(0), Is.EqualTo(reel.position));
            Assert.That(presentation.LastGripPointIndex, Is.EqualTo(-1));
            Assert.That(tether.enabled, Is.False);

            presentation.Present(new ShowcaseCargoStrapPresentationSnapshot(
                ShowcaseCargoStrapPhase.Releasing,
                0.5f,
                null,
                reel.position,
                new Vector3(3f, 1f, 0f),
                new Vector3(3f, 1f, 0f),
                reel.position,
                true,
                false,
                false,
                PrototypeWeaponSelection.Fists));
            Assert.That(clamp.transform.position.x, Is.EqualTo(1.5f).Within(0.001f));
        }

        [Test]
        public void PrepareRevealsThinRightSideRouteTowardExactGrip()
        {
            root = new GameObject("STRAP_PREPARE_ROUTE_TEST");
            Transform reel = CreateSocket("REEL", new Vector3(0f, 1f, 0f));
            Transform hand = CreateSocket("HAND", new Vector3(0.5f, 1.35f, 0.15f));
            var lineObject = new GameObject("STRAP_LINE");
            lineObject.transform.SetParent(root.transform, false);
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            LineRenderer tether = CreateLine("OBJECT_TETHER");
            GameObject clamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            clamp.transform.SetParent(root.transform, false);
            Object.DestroyImmediate(clamp.GetComponent<Collider>());
            ShowcaseCargoStrapPresentation presentation =
                root.AddComponent<ShowcaseCargoStrapPresentation>();
            presentation.Configure(
                null,
                reel,
                hand,
                line,
                clamp.transform,
                tether);

            presentation.Present(CreateSnapshot(
                ShowcaseCargoStrapPhase.Prepare,
                0f,
                Vector3.zero,
                PrototypeWeaponSelection.Fists));
            Assert.That(line.enabled, Is.False);
            Assert.That(presentation.LastDrawFraction, Is.EqualTo(0f));

            presentation.Present(CreateSnapshot(
                ShowcaseCargoStrapPhase.Prepare,
                0.35f,
                Vector3.zero,
                PrototypeWeaponSelection.Fists));
            Assert.That(line.enabled, Is.True);
            Assert.That(presentation.LastDrawFraction, Is.InRange(0.01f, 0.99f));
            Assert.That(line.GetPosition(line.positionCount - 1),
                Is.Not.EqualTo(hand.position));
            Assert.That(presentation.LastRouteOutwardClearanceMeters,
                Is.GreaterThan(0.015f));

            presentation.Present(CreateSnapshot(
                ShowcaseCargoStrapPhase.Prepare,
                1f,
                Vector3.zero,
                PrototypeWeaponSelection.Fists));
            Assert.That(presentation.LastDrawFraction, Is.EqualTo(1f));
            Assert.That(line.GetPosition(line.positionCount - 1),
                Is.EqualTo(presentation.LastGripWorldPosition));
            Assert.That(presentation.LastGripWorldPosition,
                Is.Not.EqualTo(hand.position));
            Assert.That(presentation.LastPalmCenterWorldPosition,
                Is.EqualTo(hand.position));
            Assert.That(line.startWidth, Is.EqualTo(0.016f).Within(0.0001f));
            Assert.That(tether.enabled, Is.False);
        }

        [Test]
        public void MediumDynamicTensionChangesSagWithoutMovingActualEndpoint()
        {
            root = new GameObject("STRAP_DYNAMIC_TENSION_TEST");
            Transform reel = CreateSocket("REEL", new Vector3(0f, 1f, 0f));
            Transform hand = CreateSocket("HAND", new Vector3(0.5f, 1f, 0f));
            LineRenderer line = CreateLine("PRIMARY");
            LineRenderer tether = CreateLine("TETHER");
            GameObject clamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            clamp.transform.SetParent(root.transform, false);
            Object.DestroyImmediate(clamp.GetComponent<Collider>());
            var targetObject = new GameObject("MEDIUM_TARGET");
            targetObject.transform.SetParent(root.transform, false);
            PrototypeInteractionTarget target =
                targetObject.AddComponent<PrototypeInteractionTarget>();
            target.Configure(
                "OBJ-P-M-TEST",
                "MEDIUM-TENSION-TEST",
                "Medium Test",
                PrototypeWeightClass.Medium,
                PrototypeInteractionRole.DirectChair,
                true,
                false,
                true,
                true);
            ShowcaseCargoStrapPresentation presentation =
                root.AddComponent<ShowcaseCargoStrapPresentation>();
            presentation.Configure(
                null,
                reel,
                hand,
                line,
                clamp.transform,
                tether);
            Vector3 anchor = new Vector3(3f, 1f, 0f);

            presentation.Present(new ShowcaseCargoStrapPresentationSnapshot(
                ShowcaseCargoStrapPhase.Tensioned,
                1f,
                target,
                reel.position,
                anchor,
                Vector3.zero,
                Vector3.zero,
                true,
                true,
                false,
                PrototypeWeaponSelection.ObjectWeapon,
                3f,
                0.2f));
            float slackMidpointY = tether.GetPosition(3).y;

            presentation.Present(new ShowcaseCargoStrapPresentationSnapshot(
                ShowcaseCargoStrapPhase.AttackLeft,
                0.7f,
                target,
                reel.position,
                anchor,
                Vector3.zero,
                Vector3.zero,
                true,
                true,
                false,
                PrototypeWeaponSelection.ObjectWeapon,
                3f,
                1f));
            float tautMidpointY = tether.GetPosition(3).y;

            Assert.That(tautMidpointY, Is.GreaterThan(slackMidpointY + 0.05f));
            Assert.That(tether.GetPosition(tether.positionCount - 1),
                Is.EqualTo(anchor));
        }

        private LineRenderer CreateLine(string name)
        {
            var lineObject = new GameObject(name);
            lineObject.transform.SetParent(root.transform, false);
            return lineObject.AddComponent<LineRenderer>();
        }

        private Transform CreateSocket(string name, Vector3 position)
        {
            var socket = new GameObject(name);
            socket.transform.SetParent(root.transform, false);
            socket.transform.position = position;
            return socket.transform;
        }

        private static ShowcaseCargoStrapPresentationSnapshot CreateSnapshot(
            ShowcaseCargoStrapPhase phase,
            float progress,
            Vector3 anchor,
            PrototypeWeaponSelection weaponSelection)
        {
            return new ShowcaseCargoStrapPresentationSnapshot(
                phase,
                progress,
                null,
                Vector3.zero,
                anchor,
                Vector3.zero,
                Vector3.zero,
                true,
                true,
                false,
                weaponSelection);
        }
    }
}
