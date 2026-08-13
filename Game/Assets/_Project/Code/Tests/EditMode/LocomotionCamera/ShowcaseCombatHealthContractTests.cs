using BreakNotice.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ShowcaseCombatHealthContractTests
    {
        private GameObject owner;
        private ShowcaseCombatHealth health;

        [SetUp]
        public void SetUp()
        {
            owner = new GameObject("HEALTH_TEST");
            health = owner.AddComponent<ShowcaseCombatHealth>();
            health.Configure(ShowcaseCombatTeam.Enemy, 100, 0.2f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(owner);
        }

        [Test]
        public void Damage_UsesInvulnerabilityAndAttackSequenceDeduplication()
        {
            ShowcaseDamageRequest first = Request("CHR00", 1, 15);
            ShowcaseDamageRequest second = Request("CHR00", 2, 20);

            Assert.That(health.TryApplyDamage(first, 1f), Is.True);
            Assert.That(health.CurrentHealth, Is.EqualTo(85));
            Assert.That(health.TryApplyDamage(second, 1.1f), Is.False);
            Assert.That(health.TryApplyDamage(first, 1.3f), Is.False);
            Assert.That(health.TryApplyDamage(second, 1.3f), Is.True);
            Assert.That(health.CurrentHealth, Is.EqualTo(65));
        }

        [Test]
        public void FriendlyFire_IsRejectedWithoutHealthMutation()
        {
            var request = new ShowcaseDamageRequest(
                ShowcaseCombatTeam.Enemy,
                "ALLY_ENEMY",
                1,
                35,
                ShowcaseHitReaction.BigHead,
                Vector3.forward);

            Assert.That(health.TryApplyDamage(request, 1f), Is.False);
            Assert.That(health.CurrentHealth, Is.EqualTo(100));
        }

        [Test]
        public void LethalDamageAndReset_RestoreExactBaseline()
        {
            Assert.That(health.TryApplyDamage(Request("CHR00", 1, 100), 1f), Is.True);
            Assert.That(health.IsDead, Is.True);
            Assert.That(health.NormalizedHealth, Is.Zero);

            health.ResetToBaseline();

            Assert.That(health.IsDead, Is.False);
            Assert.That(health.CurrentHealth, Is.EqualTo(100));
            Assert.That(health.AcceptedDamageCount, Is.Zero);
            Assert.That(health.RejectedDamageCount, Is.Zero);
        }

        private static ShowcaseDamageRequest Request(string source, uint sequence, int damage)
        {
            return new ShowcaseDamageRequest(
                ShowcaseCombatTeam.Player,
                source,
                sequence,
                damage,
                ShowcaseHitReaction.Body,
                Vector3.forward);
        }
    }
}
