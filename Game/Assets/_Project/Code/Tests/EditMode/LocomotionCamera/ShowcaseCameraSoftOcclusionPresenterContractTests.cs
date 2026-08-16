using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ShowcaseCameraSoftOcclusionPresenterContractTests
    {
        private const string PresenterPath =
            "Assets/_Project/Code/Runtime/Presentation/" +
            "ShowcaseCameraSoftOcclusionPresenter.cs";

        [Test]
        public void Resolve_UsesPreparedCachesWithoutHierarchyTraversalOrCollections()
        {
            string absolutePath = Path.Combine(
                Directory.GetParent(Application.dataPath).FullName,
                PresenterPath);
            string source = File.ReadAllText(absolutePath);
            int resolveStart = source.IndexOf(
                "public void Resolve(Vector3 subjectPosition, Vector3 cameraPosition)",
                System.StringComparison.Ordinal);
            int resetStart = source.IndexOf(
                "public void ResetVisibility()",
                resolveStart,
                System.StringComparison.Ordinal);

            Assert.That(resolveStart, Is.GreaterThanOrEqualTo(0));
            Assert.That(resetStart, Is.GreaterThan(resolveStart));
            string resolveSource = source.Substring(
                resolveStart,
                resetStart - resolveStart);

            Assert.That(resolveSource, Does.Not.Contain("GetComponentsInChildren"));
            Assert.That(resolveSource, Does.Not.Contain("GetComponentInParent"));
            Assert.That(resolveSource, Does.Not.Contain("new List"));
            Assert.That(resolveSource, Does.Not.Contain("new Dictionary"));
            Assert.That(resolveSource, Does.Not.Contain("RefreshCache()"));
            Assert.That(source, Does.Contain("public void RefreshCache()"));
            Assert.That(source, Does.Contain("ForceRenderingOffBeforeHide"));
        }
    }
}
