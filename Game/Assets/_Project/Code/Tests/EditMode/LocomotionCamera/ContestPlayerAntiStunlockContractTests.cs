using BreakNotice.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestPlayerAntiStunlockContractTests
    {
        private GameObject playerObject;
        private ShowcaseCombatHealth health;
        private ShowcasePlayerCombatController combat;

        [SetUp]
        public void SetUp()
        {
            playerObject = new GameObject("ANTI_STUNLOCK_PLAYER");
            health = playerObject.AddComponent<ShowcaseCombatHealth>();
            health.Configure(ShowcaseCombatTeam.Player, 100, 0.6f);
            combat = playerObject.AddComponent<ShowcasePlayerCombatController>();
            combat.Configure(
                health,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void SameFrameDifferentAttackers_DeduplicateSecondDamage()
        {
            Assert.That(combat.DamageImmunitySeconds, Is.EqualTo(0.12f).Within(0.001f));
            Assert.That(health.InvulnerabilitySeconds, Is.EqualTo(0.12f).Within(0.001f));

            Assert.That(combat.ReceiveDamage(
                Request("NORMAL 1", 1, 10, ShowcaseHitReaction.Body),
                1f,
                100), Is.True);
            Assert.That(combat.ReceiveDamage(
                Request("ELITE 1", 1, 30, ShowcaseHitReaction.Knockdown),
                1f,
                100), Is.False);

            Assert.That(health.CurrentHealth, Is.EqualTo(90));
            Assert.That(health.AcceptedDamageCount, Is.EqualTo(1));
            Assert.That(combat.SameFrameDamageDedupCount, Is.EqualTo(1));
            Assert.That(combat.AnimationCue,
                Is.EqualTo(ShowcasePlayerAnimationCue.HitBody));
        }

        [Test]
        public void DamageAndStaggerProtection_AreSeparateWindows()
        {
            Assert.That(combat.ReceiveDamage(
                Request("NORMAL 1", 1, 10, ShowcaseHitReaction.Body),
                2f,
                200), Is.True);
            uint firstCueRevision = combat.AnimationCueRevision;

            Assert.That(combat.ReceiveDamage(
                Request("NORMAL 2", 1, 15, ShowcaseHitReaction.Knockdown),
                2.13f,
                201), Is.True,
                "Damage should resume after 0.12 seconds even while stagger remains protected.");

            Assert.That(health.CurrentHealth, Is.EqualTo(75));
            Assert.That(health.AcceptedDamageCount, Is.EqualTo(2));
            Assert.That(combat.ProtectedStaggerCount, Is.EqualTo(1));
            Assert.That(combat.AnimationCueRevision, Is.EqualTo(firstCueRevision));
            Assert.That(combat.AnimationCue,
                Is.EqualTo(ShowcasePlayerAnimationCue.HitBody),
                "The protected second contact must not restart or upgrade the reaction.");
        }

        [Test]
        public void KnockdownAndGetup_CapLaunchButNeverOverrideLethalPriority()
        {
            Assert.That(combat.ReceiveDamage(
                Request("NORMAL 1", 10, 30, ShowcaseHitReaction.Knockdown),
                3f,
                300), Is.True);
            combat.QueueReactionDisplacement(Vector3.back, 0.65f, 0.30f);
            Assert.That(combat.PendingReactionDistance, Is.EqualTo(0.65f).Within(0.001f));
            Assert.That(combat.IsKnockdownOrGetup, Is.True);

            Assert.That(combat.ReceiveDamage(
                Request("NORMAL 2", 20, 10, ShowcaseHitReaction.Body),
                3.13f,
                301), Is.True);
            combat.QueueReactionDisplacement(Vector3.forward, 0.40f, 0.20f);
            Assert.That(combat.PendingReactionDistance, Is.EqualTo(0.65f).Within(0.001f),
                "An additional contact must not overwrite the initial knockdown launch.");
            Assert.That(combat.SuppressedAdditionalLaunchCount, Is.EqualTo(1));
            Assert.That(combat.AdditionalKnockdownLaunchDistance,
                Is.LessThanOrEqualTo(combat.KnockdownAdditionalLaunchCap + 0.001f));

            combat.CompleteAnimationAction();
            Assert.That(combat.AnimationCue == ShowcasePlayerAnimationCue.GetupBack ||
                        combat.AnimationCue == ShowcasePlayerAnimationCue.GetupFront,
                Is.True);
            uint getupCueRevision = combat.AnimationCueRevision;
            Assert.That(combat.ReceiveDamage(
                Request("ELITE 1", 30, 10, ShowcaseHitReaction.Knockdown),
                3.26f,
                302), Is.True);
            Assert.That(combat.AnimationCueRevision, Is.EqualTo(getupCueRevision),
                "Get-up protection blocks stagger replacement but not damage.");

            Assert.That(combat.ReceiveDamage(
                Request("ELITE 1", 31, 100, ShowcaseHitReaction.Knockdown),
                3.39f,
                303), Is.True);
            Assert.That(health.IsDead, Is.True);
            Assert.That(new[]
            {
                ShowcasePlayerAnimationCue.Death1,
                ShowcasePlayerAnimationCue.Death2
            }, Does.Contain(combat.AnimationCue),
                "KO/death must outrank every reaction-protection window.");
        }

        private static ShowcaseDamageRequest Request(
            string sourceId,
            uint sequence,
            int amount,
            ShowcaseHitReaction reaction)
        {
            return new ShowcaseDamageRequest(
                ShowcaseCombatTeam.Enemy,
                sourceId,
                sequence,
                amount,
                reaction,
                Vector3.back);
        }
    }
}
