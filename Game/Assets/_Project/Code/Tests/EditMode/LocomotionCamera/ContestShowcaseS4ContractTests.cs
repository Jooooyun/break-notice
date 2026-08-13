using System.IO;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestShowcaseS4ContractTests
    {
        private GameObject ownerObject;

        [TearDown]
        public void TearDown()
        {
            if (ownerObject != null)
            {
                Object.DestroyImmediate(ownerObject);
            }
        }

        [Test]
        public void QArbitration_UsesExactOwnerApprovedPriority()
        {
            Assert.That(Resolve(true, true, true, true, true, true, true),
                Is.EqualTo(ShowcaseQContext.Blocked));
            Assert.That(Resolve(false, true, true, true, true, true, true),
                Is.EqualTo(ShowcaseQContext.BreakGrab));
            Assert.That(Resolve(false, false, true, true, true, true, true),
                Is.EqualTo(ShowcaseQContext.TeamFinish));
            Assert.That(Resolve(false, false, false, true, true, true, true),
                Is.EqualTo(ShowcaseQContext.AssistAttach));
            Assert.That(Resolve(false, false, false, false, true, true, true),
                Is.EqualTo(ShowcaseQContext.CombatTag));
            Assert.That(Resolve(false, false, false, false, false, true, true),
                Is.EqualTo(ShowcaseQContext.SendSupply));
            Assert.That(Resolve(false, false, false, false, false, false, true),
                Is.EqualTo(ShowcaseQContext.ToggleFollow));
            Assert.That(Resolve(false, false, false, false, false, false, false),
                Is.EqualTo(ShowcaseQContext.None));
        }

        [Test]
        public void ReadinessBadges_AreDistinctIdempotentAndAnyTwoCreateTeamReady()
        {
            ShowcaseTeamContextController team = CreateTeamContext();

            Assert.That(team.RecordPassSuccess(), Is.True);
            Assert.That(team.RecordPassSuccess(), Is.False);
            Assert.That(team.BadgeCount, Is.EqualTo(1));
            Assert.That(team.IsTeamReady, Is.False);

            Assert.That(team.RecordBraceSuccess(), Is.True);
            Assert.That(team.BadgeCount, Is.EqualTo(2));
            Assert.That(team.IsTeamReady, Is.True);
            Assert.That(team.HasPass, Is.True);
            Assert.That(team.HasBrace, Is.True);
            Assert.That(team.HasBreak, Is.False);
        }

        [Test]
        public void RunReset_ClearsBadgesFutureFactsAndAttemptState()
        {
            ShowcaseTeamContextController team = CreateTeamContext();
            team.RecordPassSuccess();
            team.RecordBreakSuccess();
            team.SetFutureContextAvailability(true, true, true, true);
            team.RecordQAttempt(ShowcaseQContext.BreakGrab, false);

            team.ResetToRunBaseline();

            Assert.That(team.Badges, Is.EqualTo(ShowcaseTeamReadinessBadge.None));
            Assert.That(team.BadgeCount, Is.Zero);
            Assert.That(team.IsTeamReady, Is.False);
            Assert.That(team.QAttemptSequence, Is.Zero);
            Assert.That(team.LastQContext, Is.EqualTo(ShowcaseQContext.None));
            Assert.That(team.ResolveCurrentQContext(), Is.EqualTo(ShowcaseQContext.Blocked));
        }

        [Test]
        public void QPrompt_HasOnePresentationOwner()
        {
            string interactionPresenter = File.ReadAllText(
                "Assets/_Project/Code/Runtime/Presentation/InteractionReadabilityPresenter.cs");
            string mobilePresenter = File.ReadAllText(
                "Assets/_Project/Code/Runtime/Presentation/ShowcaseMobileAllyPresenter.cs");
            string teamPresenter = File.ReadAllText(
                "Assets/_Project/Code/Runtime/Presentation/ShowcaseTeamContextPresenter.cs");

            StringAssert.Contains("ShowcaseTeamContextController", interactionPresenter);
            StringAssert.Contains("ShowcaseTeamContextController", mobilePresenter);
            StringAssert.Contains("[Q] BREAK GRAB", teamPresenter);
            StringAssert.Contains("[Q] TEAM FINISH", teamPresenter);
            StringAssert.Contains("[Q] ASSIST ATTACH", teamPresenter);
            StringAssert.Contains("[Q] TAG SHOVE / SET HIM UP", teamPresenter);
            StringAssert.Contains("[Q] SEND BINDER", teamPresenter);
        }

        private ShowcaseTeamContextController CreateTeamContext()
        {
            ownerObject = new GameObject("S4TeamContextOwner");
            return ownerObject.AddComponent<ShowcaseTeamContextController>();
        }

        private static ShowcaseQContext Resolve(
            bool blocked,
            bool grabbed,
            bool finisher,
            bool attach,
            bool combatTag,
            bool supply,
            bool follow)
        {
            return ShowcaseQContextArbitrator.Resolve(
                new ShowcaseQContextSnapshot(
                    blocked,
                    grabbed,
                    finisher,
                    attach,
                    combatTag,
                    supply,
                    follow));
        }
    }
}
