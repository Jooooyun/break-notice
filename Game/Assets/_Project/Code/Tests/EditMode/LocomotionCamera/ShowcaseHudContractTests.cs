using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ShowcaseHudContractTests
    {
        private GameObject createdObject;

        [TearDown]
        public void TearDown()
        {
            if (createdObject != null)
            {
                Object.DestroyImmediate(createdObject);
            }
        }

        [Test]
        public void PlayerDamage_IsImmediateWhileTrailSettlesLater()
        {
            ShowcaseHudPresenter presenter = CreatePresenter();

            presenter.SetPlayerHealthNormalized(0.4f);

            Assert.That(presenter.PlayerHealthVisualNormalized, Is.EqualTo(0.4f));
            Assert.That(presenter.PlayerHealthTrailNormalized, Is.EqualTo(1f));
            presenter.TickPresentation(0.08f);
            Assert.That(presenter.PlayerHealthTrailNormalized, Is.EqualTo(1f));
            presenter.TickPresentation(0.16f);
            Assert.That(presenter.PlayerHealthTrailNormalized, Is.LessThan(1f));
            Assert.That(presenter.PlayerHealthTrailNormalized, Is.GreaterThan(0.4f));
            presenter.TickPresentation(0.32f);
            Assert.That(presenter.PlayerHealthTrailNormalized, Is.EqualTo(0.4f));
        }

        [Test]
        public void PlayerRecovery_FillsSmoothlyInsteadOfJumping()
        {
            ShowcaseHudPresenter presenter = CreatePresenter();
            presenter.SetPlayerHealthNormalized(0.25f);
            presenter.TickPresentation(1f);

            presenter.SetPlayerHealthNormalized(1f);

            Assert.That(presenter.PlayerHealthVisualNormalized, Is.EqualTo(0.25f));
            presenter.TickPresentation(0.11f);
            Assert.That(presenter.PlayerHealthVisualNormalized, Is.EqualTo(0.75f).Within(0.01f));
            presenter.TickPresentation(0.11f);
            Assert.That(presenter.PlayerHealthVisualNormalized, Is.EqualTo(1f));
            Assert.That(presenter.PlayerHealthTrailNormalized, Is.EqualTo(1f));
        }

        [TestCase(-1f, 0)]
        [TestCase(0f, 0)]
        [TestCase(0.01f, 1)]
        [TestCase(0.5f, 6)]
        [TestCase(0.75f, 9)]
        [TestCase(1f, 12)]
        [TestCase(2f, 12)]
        public void ChargeSegments_ClampAndAdvance(float charge01, int expected)
        {
            Assert.That(
                ShowcaseHudPresenter.CalculateChargeSegmentCount(charge01),
                Is.EqualTo(expected));
        }

        [Test]
        public void WorldHealthAnchor_AcceptsNormalizedAndCurrentMaximumInputs()
        {
            createdObject = new GameObject("WorldHealthAnchor");
            ShowcaseWorldHealthAnchor anchor =
                createdObject.AddComponent<ShowcaseWorldHealthAnchor>();

            anchor.SetNormalizedHealth(2f);
            Assert.That(anchor.NormalizedHealth, Is.EqualTo(1f));
            anchor.SetHealth(25f, 100f);
            Assert.That(anchor.NormalizedHealth, Is.EqualTo(0.25f));
            anchor.SetHealth(10f, 0f);
            Assert.That(anchor.NormalizedHealth, Is.EqualTo(0f));
        }

        private ShowcaseHudPresenter CreatePresenter()
        {
            createdObject = new GameObject("ShowcaseHudPresenter");
            return createdObject.AddComponent<ShowcaseHudPresenter>();
        }
    }
}
