using System;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ShowcaseCameraSoftOcclusionPresenterPlayModeTests
    {
        [Test]
        public void Resolve_HidesAndRestoresWholeRootWithoutChangingPhysicsTopology()
        {
            int softLayer = LayerMask.NameToLayer("CameraSoftOccluder");
            Assert.That(softLayer, Is.GreaterThanOrEqualTo(0));

            GameObject softRootObject = null;
            GameObject presenterObject = null;
            try
            {
                Vector3 origin = new Vector3(400f, 10f, 400f);
                softRootObject = new GameObject("SoftOcclusionPresenterTestRoot");
                softRootObject.layer = softLayer;
                softRootObject.transform.position = origin;
                softRootObject.AddComponent<ShowcaseCameraSoftOccluderRoot>();

                GameObject hitChild = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hitChild.name = "HitRenderer";
                hitChild.layer = softLayer;
                hitChild.transform.SetParent(softRootObject.transform, false);
                hitChild.transform.localPosition = new Vector3(0f, 0f, 2f);
                hitChild.transform.localScale = new Vector3(1f, 1f, 0.2f);
                Renderer hitRenderer = hitChild.GetComponent<Renderer>();
                Collider hitCollider = hitChild.GetComponent<Collider>();

                GameObject siblingChild = GameObject.CreatePrimitive(PrimitiveType.Cube);
                siblingChild.name = "SiblingRendererWithOriginalForceOff";
                siblingChild.layer = softLayer;
                siblingChild.transform.SetParent(softRootObject.transform, false);
                siblingChild.transform.localPosition = new Vector3(0.75f, 0f, 2f);
                siblingChild.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                Renderer siblingRenderer = siblingChild.GetComponent<Renderer>();
                siblingRenderer.forceRenderingOff = true;

                presenterObject = new GameObject("SoftOcclusionPresenterTest");
                ShowcaseCameraSoftOcclusionPresenter presenter =
                    presenterObject.AddComponent<ShowcaseCameraSoftOcclusionPresenter>();
                presenter.RefreshCache();
                Physics.SyncTransforms();

                int rootLayer = softRootObject.layer;
                int hitLayer = hitChild.layer;
                bool rootActive = softRootObject.activeSelf;
                bool hitActive = hitChild.activeSelf;
                bool colliderEnabled = hitCollider.enabled;
                bool colliderTrigger = hitCollider.isTrigger;
                bool rendererEnabled = hitRenderer.enabled;
                int refreshCount = presenter.CacheRefreshCount;

                Vector3 subjectPosition = origin;
                Vector3 blockedCameraPosition = origin + Vector3.forward * 4f;
                presenter.Resolve(subjectPosition, blockedCameraPosition);

                Assert.That(presenter.ActiveOccluderCount, Is.EqualTo(1));
                Assert.That(hitRenderer.forceRenderingOff, Is.True);
                Assert.That(siblingRenderer.forceRenderingOff, Is.True);
                Assert.That(presenter.CacheRefreshCount, Is.EqualTo(refreshCount));

                presenter.RefreshCache();
                Assert.That(
                    hitRenderer.forceRenderingOff,
                    Is.True,
                    "Atomic refresh must preserve an already hidden root.");
                Assert.That(siblingRenderer.forceRenderingOff, Is.True);
                Assert.That(presenter.ActiveOccluderCount, Is.EqualTo(1));

                Vector3 clearSubjectPosition = origin + Vector3.right * 10f;
                presenter.Resolve(
                    clearSubjectPosition,
                    clearSubjectPosition + Vector3.forward * 4f);

                Assert.That(presenter.ActiveOccluderCount, Is.Zero);
                Assert.That(hitRenderer.forceRenderingOff, Is.False);
                Assert.That(
                    siblingRenderer.forceRenderingOff,
                    Is.True,
                    "Presenter must restore the exact pre-hide forceRenderingOff state.");
                Assert.That(softRootObject.activeSelf, Is.EqualTo(rootActive));
                Assert.That(softRootObject.activeInHierarchy, Is.True);
                Assert.That(hitChild.activeSelf, Is.EqualTo(hitActive));
                Assert.That(hitCollider.enabled, Is.EqualTo(colliderEnabled));
                Assert.That(hitCollider.isTrigger, Is.EqualTo(colliderTrigger));
                Assert.That(hitRenderer.enabled, Is.EqualTo(rendererEnabled));
                Assert.That(softRootObject.layer, Is.EqualTo(rootLayer));
                Assert.That(hitChild.layer, Is.EqualTo(hitLayer));
            }
            finally
            {
                if (presenterObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(presenterObject);
                }
                if (softRootObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(softRootObject);
                }
            }
        }

        [Test]
        public void Resolve_RepeatedSteadyState_DoesNotAllocateOrRefreshCache()
        {
            int softLayer = LayerMask.NameToLayer("CameraSoftOccluder");
            Assert.That(softLayer, Is.GreaterThanOrEqualTo(0));

            GameObject softRootObject = null;
            GameObject presenterObject = null;
            try
            {
                Vector3 origin = new Vector3(420f, 10f, 420f);
                softRootObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                softRootObject.name = "SoftOcclusionAllocationTestRoot";
                softRootObject.layer = softLayer;
                softRootObject.transform.position = origin + Vector3.forward * 2f;
                softRootObject.transform.localScale = new Vector3(1f, 1f, 0.2f);
                softRootObject.AddComponent<ShowcaseCameraSoftOccluderRoot>();

                presenterObject = new GameObject("SoftOcclusionAllocationPresenter");
                ShowcaseCameraSoftOcclusionPresenter presenter =
                    presenterObject.AddComponent<ShowcaseCameraSoftOcclusionPresenter>();
                presenter.RefreshCache();
                Physics.SyncTransforms();

                Vector3 cameraPosition = origin + Vector3.forward * 4f;
                for (int index = 0; index < 16; index++)
                {
                    presenter.Resolve(origin, cameraPosition);
                }

                int refreshCount = presenter.CacheRefreshCount;
                long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
                for (int index = 0; index < 128; index++)
                {
                    presenter.Resolve(origin, cameraPosition);
                }
                long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

                Assert.That(
                    allocatedAfter - allocatedBefore,
                    Is.Zero,
                    "Steady-state Resolve must not allocate managed memory.");
                Assert.That(presenter.CacheRefreshCount, Is.EqualTo(refreshCount));
                Assert.That(presenter.ActiveOccluderCount, Is.EqualTo(1));
            }
            finally
            {
                if (presenterObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(presenterObject);
                }
                if (softRootObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(softRootObject);
                }
            }
        }
    }
}
