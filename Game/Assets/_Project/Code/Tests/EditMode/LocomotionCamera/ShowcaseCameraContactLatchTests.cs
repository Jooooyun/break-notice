using BreakNotice.Presentation;
using NUnit.Framework;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ShowcaseCameraContactLatchTests
    {
        private const float RequestedBoom = 3f;
        private const float ReleaseEvidenceDuration = 0.18f;
        private const float ReturnDuration = 0.25f;

        [TestCase(30)]
        [TestCase(60)]
        [TestCase(120)]
        public void ContactAndRelease_AreLatchedAndMonotonicAtConfiguredFrameRates(
            int framesPerSecond)
        {
            float deltaTime = 1f / framesPerSecond;
            var latch = new ShowcaseCameraContactLatch();
            latch.Reset(RequestedBoom);

            float previousBoom = latch.Advance(
                true,
                2.2f,
                RequestedBoom,
                deltaTime,
                ReleaseEvidenceDuration,
                ReturnDuration);
            Assert.That(latch.Phase, Is.EqualTo(ShowcaseStaticCameraContactPhase.Contact));
            Assert.That(latch.ContactEpisodeId, Is.EqualTo(1));
            Assert.That(latch.RestoreStartCount, Is.Zero);

            // Collider/plane churn within one contact episode must never expand the boom.
            float[] changingSafeBooms = { 2.35f, 2.1f, 2.3f, 2.05f, 2.2f };
            foreach (float safeBoom in changingSafeBooms)
            {
                float currentBoom = latch.Advance(
                    true,
                    safeBoom,
                    RequestedBoom,
                    deltaTime,
                    ReleaseEvidenceDuration,
                    ReturnDuration);
                Assert.That(currentBoom, Is.LessThanOrEqualTo(previousBoom + 0.000001f));
                Assert.That(latch.ContactEpisodeId, Is.EqualTo(1));
                Assert.That(latch.RestoreStartCount, Is.Zero);
                previousBoom = currentBoom;
            }

            // A short seam-sized clear gap is evidence, not a new release episode.
            float clearElapsed = 0f;
            while (clearElapsed + deltaTime < ReleaseEvidenceDuration - 0.000001f)
            {
                float currentBoom = latch.Advance(
                    false,
                    RequestedBoom,
                    RequestedBoom,
                    deltaTime,
                    ReleaseEvidenceDuration,
                    ReturnDuration);
                clearElapsed += deltaTime;
                Assert.That(latch.Phase, Is.EqualTo(ShowcaseStaticCameraContactPhase.Contact));
                Assert.That(currentBoom, Is.EqualTo(previousBoom).Within(0.000001f));
                Assert.That(latch.RestoreStartCount, Is.Zero);
            }

            previousBoom = latch.Advance(
                true,
                previousBoom,
                RequestedBoom,
                deltaTime,
                ReleaseEvidenceDuration,
                ReturnDuration);
            Assert.That(latch.Phase, Is.EqualTo(ShowcaseStaticCameraContactPhase.Contact));
            Assert.That(latch.ClearEvidence, Is.Zero);
            Assert.That(latch.ContactEpisodeId, Is.EqualTo(1));
            Assert.That(latch.RestoreStartCount, Is.Zero);

            // Continuous full-pose clearance starts exactly one restoration.
            int guard = framesPerSecond;
            while (latch.Phase != ShowcaseStaticCameraContactPhase.Restoring && guard-- > 0)
            {
                previousBoom = latch.Advance(
                    false,
                    RequestedBoom,
                    RequestedBoom,
                    deltaTime,
                    ReleaseEvidenceDuration,
                    ReturnDuration);
            }

            Assert.That(latch.Phase, Is.EqualTo(ShowcaseStaticCameraContactPhase.Restoring));
            Assert.That(latch.RestoreStartCount, Is.EqualTo(1));
            Assert.That(latch.ContactEpisodeId, Is.EqualTo(1));

            guard = framesPerSecond;
            while (latch.Phase != ShowcaseStaticCameraContactPhase.Clear && guard-- > 0)
            {
                float currentBoom = latch.Advance(
                    false,
                    RequestedBoom,
                    RequestedBoom,
                    deltaTime,
                    ReleaseEvidenceDuration,
                    ReturnDuration);
                Assert.That(currentBoom, Is.GreaterThanOrEqualTo(previousBoom - 0.000001f));
                Assert.That(currentBoom, Is.LessThanOrEqualTo(RequestedBoom + 0.002f));
                Assert.That(latch.RestoreStartCount, Is.EqualTo(1));
                previousBoom = currentBoom;
            }

            Assert.That(latch.Phase, Is.EqualTo(ShowcaseStaticCameraContactPhase.Clear));
            Assert.That(latch.CurrentBoom, Is.EqualTo(RequestedBoom).Within(0.002f));
            Assert.That(latch.RestoreStartCount, Is.EqualTo(1));
        }

        [TestCase(30)]
        [TestCase(60)]
        [TestCase(120)]
        public void ReblockedRestoration_CompressesImmediatelyWithoutStartingNewEpisode(
            int framesPerSecond)
        {
            float deltaTime = 1f / framesPerSecond;
            var latch = new ShowcaseCameraContactLatch();
            latch.Reset(RequestedBoom);
            latch.Advance(
                true,
                2.1f,
                RequestedBoom,
                deltaTime,
                ReleaseEvidenceDuration,
                ReturnDuration);

            int guard = framesPerSecond;
            while (latch.Phase != ShowcaseStaticCameraContactPhase.Restoring && guard-- > 0)
            {
                latch.Advance(
                    false,
                    RequestedBoom,
                    RequestedBoom,
                    deltaTime,
                    ReleaseEvidenceDuration,
                    ReturnDuration);
            }

            Assert.That(latch.Phase, Is.EqualTo(ShowcaseStaticCameraContactPhase.Restoring));
            int episode = latch.ContactEpisodeId;
            int restoration = latch.RestoreStartCount;
            float restoredBoom = latch.Advance(
                false,
                RequestedBoom,
                RequestedBoom,
                deltaTime,
                ReleaseEvidenceDuration,
                ReturnDuration);

            float recompressedBoom = latch.Advance(
                true,
                1.9f,
                RequestedBoom,
                deltaTime,
                ReleaseEvidenceDuration,
                ReturnDuration);

            Assert.That(latch.Phase, Is.EqualTo(ShowcaseStaticCameraContactPhase.Contact));
            Assert.That(latch.ContactEpisodeId, Is.EqualTo(episode));
            Assert.That(latch.RestoreStartCount, Is.EqualTo(restoration));
            Assert.That(recompressedBoom, Is.LessThanOrEqualTo(1.9f + 0.000001f));
            Assert.That(recompressedBoom, Is.LessThanOrEqualTo(restoredBoom));
        }
    }
}
