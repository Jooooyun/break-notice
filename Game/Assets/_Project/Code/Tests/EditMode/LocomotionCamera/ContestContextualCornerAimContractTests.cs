using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestContextualCornerAimContractTests
    {
        private const float Step = 0.02f;

        [Test]
        public void NormalAndSprintEligibilityNeverEnterContextualShoulderModes()
        {
            ShowcaseContextualCornerAimResolver resolver = new();
            int frame = 0;
            ShowcaseContextualCornerCandidate right = Candidate(
                ShowcaseContextualCornerMode.RightDefault,
                0.45f,
                true,
                false,
                true,
                31);
            ShowcaseContextualCornerCandidate lean = Candidate(
                ShowcaseContextualCornerMode.RightLean,
                2f,
                true,
                true,
                false,
                0);
            ShowcaseContextualCornerCandidate left = Candidate(
                ShowcaseContextualCornerMode.TemporaryLeft,
                2.5f,
                true,
                true,
                false,
                0);

            for (int index = 0; index < 30; index++)
            {
                ShowcaseContextualCornerSelection selection = resolver.Evaluate(
                    Frame(++frame, resolver, isAim: false),
                    right,
                    lean,
                    left);
                Assert.That(
                    selection.Mode,
                    Is.EqualTo(ShowcaseContextualCornerMode.RightDefault));
                Assert.That(selection.ShoulderIntentFactor, Is.EqualTo(1f));
            }
        }

        [Test]
        public void StaticRightSideLoss_UsesLeanBeforeOppositeAfterEvidenceDwell()
        {
            ShowcaseContextualCornerAimResolver resolver = new();
            int frame = 0;
            ShowcaseContextualCornerCandidate right = Candidate(
                ShowcaseContextualCornerMode.RightDefault,
                0.8f,
                true,
                false,
                true,
                44);
            ShowcaseContextualCornerCandidate lean = Candidate(
                ShowcaseContextualCornerMode.RightLean,
                2f,
                true,
                true,
                false,
                0);
            ShowcaseContextualCornerCandidate left = Candidate(
                ShowcaseContextualCornerMode.TemporaryLeft,
                3f,
                true,
                true,
                false,
                0);

            for (int index = 0; index < 4; index++)
            {
                resolver.Evaluate(Frame(++frame, resolver), right, lean, left);
            }
            Assert.That(
                resolver.CurrentMode,
                Is.EqualTo(ShowcaseContextualCornerMode.RightDefault));

            resolver.Evaluate(Frame(++frame, resolver), right, lean, left);
            resolver.Evaluate(Frame(++frame, resolver), right, lean, left);
            Assert.That(
                resolver.CurrentMode,
                Is.EqualTo(ShowcaseContextualCornerMode.RightDefault));
            Assert.That(resolver.IsTransitioning, Is.True);
            Assert.That(
                resolver.TransitionTargetMode,
                Is.EqualTo(ShowcaseContextualCornerMode.RightLean));

            for (int index = 0; index < 7; index++)
            {
                resolver.Evaluate(Frame(++frame, resolver), right, lean, left);
            }
            Assert.That(resolver.CurrentShoulderFactor,
                Is.EqualTo(ShowcaseContextualCornerAimResolver.RightLeanShoulderFactor)
                    .Within(0.001f));
            Assert.That(resolver.ActiveBlockerId, Is.EqualTo(44));
        }

        [Test]
        public void OppositeShoulderRequiresFailedLeanAndStricterImprovement()
        {
            ShowcaseContextualCornerAimResolver resolver = new();
            int frame = 0;
            ShowcaseContextualCornerCandidate right = Candidate(
                ShowcaseContextualCornerMode.RightDefault,
                0.4f,
                true,
                false,
                true,
                55);
            ShowcaseContextualCornerCandidate failedLean = Candidate(
                ShowcaseContextualCornerMode.RightLean,
                0f,
                false,
                false,
                true,
                55);
            ShowcaseContextualCornerCandidate left = Candidate(
                ShowcaseContextualCornerMode.TemporaryLeft,
                2f,
                true,
                true,
                false,
                0);

            for (int index = 0; index < 13; index++)
            {
                resolver.Evaluate(
                    Frame(++frame, resolver),
                    right,
                    failedLean,
                    left);
            }
            Assert.That(
                resolver.CurrentMode,
                Is.EqualTo(ShowcaseContextualCornerMode.TemporaryLeft));

            ShowcaseContextualCornerAimResolver leanFirstResolver = new();
            frame = 0;
            ShowcaseContextualCornerCandidate safeLean = Candidate(
                ShowcaseContextualCornerMode.RightLean,
                1.1f,
                true,
                true,
                false,
                0);
            for (int index = 0; index < 10; index++)
            {
                leanFirstResolver.Evaluate(
                    Frame(++frame, leanFirstResolver),
                    right,
                    safeLean,
                    left);
            }
            Assert.That(
                leanFirstResolver.CurrentMode,
                Is.Not.EqualTo(ShowcaseContextualCornerMode.TemporaryLeft));
        }

        [Test]
        public void ClearEvidenceReturnsToDefaultWithinApprovedRecoveryWindow()
        {
            ShowcaseContextualCornerAimResolver resolver = new();
            int frame = 0;
            ShowcaseContextualCornerCandidate rightBlocked = Candidate(
                ShowcaseContextualCornerMode.RightDefault,
                0.5f,
                true,
                false,
                true,
                66);
            ShowcaseContextualCornerCandidate lean = Candidate(
                ShowcaseContextualCornerMode.RightLean,
                2f,
                true,
                true,
                false,
                0);
            ShowcaseContextualCornerCandidate left = Candidate(
                ShowcaseContextualCornerMode.TemporaryLeft,
                2f,
                true,
                true,
                false,
                0);
            for (int index = 0; index < 13; index++)
            {
                resolver.Evaluate(
                    Frame(++frame, resolver),
                    rightBlocked,
                    lean,
                    left);
            }
            Assert.That(
                resolver.CurrentMode,
                Is.EqualTo(ShowcaseContextualCornerMode.RightLean));

            ShowcaseContextualCornerCandidate rightClear = Candidate(
                ShowcaseContextualCornerMode.RightDefault,
                2f,
                true,
                true,
                false,
                0);
            float clearStartedAt = frame * Step;
            for (int index = 0; index < 18; index++)
            {
                resolver.Evaluate(
                    Frame(++frame, resolver),
                    rightClear,
                    lean,
                    left);
            }

            Assert.That(
                resolver.CurrentMode,
                Is.EqualTo(ShowcaseContextualCornerMode.RightDefault));
            Assert.That(resolver.CurrentShoulderFactor, Is.EqualTo(1f).Within(0.001f));
            Assert.That(
                frame * Step - clearStartedAt,
                Is.LessThanOrEqualTo(
                    ShowcaseContextualCornerAimResolver.ExitEvidenceDuration +
                    ShowcaseContextualCornerAimResolver.ReturnBlendDuration +
                    Step * 2f));
        }

        [Test]
        public void ResetRevisionRestoresDefaultImmediatelyAndSameFrameIsIdempotent()
        {
            ShowcaseContextualCornerAimResolver resolver = new();
            int frame = 1;
            ShowcaseContextualCornerCandidate right = Candidate(
                ShowcaseContextualCornerMode.RightDefault,
                0.5f,
                true,
                false,
                true,
                77);
            ShowcaseContextualCornerCandidate lean = Candidate(
                ShowcaseContextualCornerMode.RightLean,
                2f,
                true,
                true,
                false,
                0);
            ShowcaseContextualCornerCandidate left = Candidate(
                ShowcaseContextualCornerMode.TemporaryLeft,
                2f,
                true,
                true,
                false,
                0);

            ShowcaseContextualCornerFrameSnapshot sameFrame =
                Frame(frame, resolver);
            for (int index = 0; index < 20; index++)
            {
                resolver.Evaluate(sameFrame, right, lean, left);
            }
            Assert.That(
                resolver.CurrentMode,
                Is.EqualTo(ShowcaseContextualCornerMode.RightDefault));

            for (int index = 0; index < 13; index++)
            {
                resolver.Evaluate(Frame(++frame, resolver), right, lean, left);
            }
            Assert.That(
                resolver.CurrentMode,
                Is.EqualTo(ShowcaseContextualCornerMode.RightLean));

            resolver.Evaluate(
                Frame(++frame, resolver, resetRevision: 2),
                right,
                lean,
                left);
            Assert.That(
                resolver.CurrentMode,
                Is.EqualTo(ShowcaseContextualCornerMode.RightDefault));
            Assert.That(resolver.CurrentShoulderFactor, Is.EqualTo(1f));
            Assert.That(resolver.TransitionCount, Is.Zero);
        }

        [Test]
        public void TransitionKeepsCommittedModeAndTargetIsImmutableUntilBlendCompletes()
        {
            ShowcaseContextualCornerAimResolver resolver = new();
            int frame = 0;
            ShowcaseContextualCornerCandidate rightBlocked = Candidate(
                ShowcaseContextualCornerMode.RightDefault,
                0.5f,
                true,
                false,
                true,
                91);
            ShowcaseContextualCornerCandidate lean = Candidate(
                ShowcaseContextualCornerMode.RightLean,
                2f,
                true,
                true,
                false,
                0);
            ShowcaseContextualCornerCandidate left = Candidate(
                ShowcaseContextualCornerMode.TemporaryLeft,
                3f,
                true,
                true,
                false,
                0);

            for (int index = 0; index < 6; index++)
            {
                resolver.Evaluate(
                    Frame(++frame, resolver),
                    rightBlocked,
                    lean,
                    left);
            }

            Assert.That(resolver.IsTransitioning, Is.True);
            Assert.That(
                resolver.CurrentMode,
                Is.EqualTo(ShowcaseContextualCornerMode.RightDefault));
            Assert.That(
                resolver.TransitionTargetMode,
                Is.EqualTo(ShowcaseContextualCornerMode.RightLean));

            ShowcaseContextualCornerCandidate rightClear = Candidate(
                ShowcaseContextualCornerMode.RightDefault,
                2f,
                true,
                true,
                false,
                0);
            for (int index = 0; index < 4; index++)
            {
                resolver.Evaluate(
                    Frame(++frame, resolver),
                    rightClear,
                    lean,
                    left);
                Assert.That(
                    resolver.TransitionTargetMode,
                    Is.EqualTo(ShowcaseContextualCornerMode.RightLean));
            }

            for (int index = 0; index < 3; index++)
            {
                resolver.Evaluate(
                    Frame(++frame, resolver),
                    rightClear,
                    lean,
                    left);
            }
            Assert.That(resolver.IsTransitioning, Is.False);
            Assert.That(
                resolver.CurrentMode,
                Is.EqualTo(ShowcaseContextualCornerMode.RightLean));
        }

        [Test]
        public void LeanSchmittBandDoesNotOscillateAtSeamThreshold()
        {
            ShowcaseContextualCornerAimResolver resolver = new();
            int frame = 0;
            ShowcaseContextualCornerCandidate right = Candidate(
                ShowcaseContextualCornerMode.RightDefault,
                1f,
                true,
                false,
                true,
                101);
            ShowcaseContextualCornerCandidate strongLean = Candidate(
                ShowcaseContextualCornerMode.RightLean,
                2f,
                true,
                true,
                false,
                0);
            ShowcaseContextualCornerCandidate left = Candidate(
                ShowcaseContextualCornerMode.TemporaryLeft,
                0f,
                false,
                false,
                true,
                101);
            for (int index = 0; index < 13; index++)
            {
                resolver.Evaluate(
                    Frame(++frame, resolver),
                    right,
                    strongLean,
                    left);
            }
            Assert.That(
                resolver.CurrentMode,
                Is.EqualTo(ShowcaseContextualCornerMode.RightLean));
            int transitionCount = resolver.TransitionCount;

            ShowcaseContextualCornerCandidate seamLean = Candidate(
                ShowcaseContextualCornerMode.RightLean,
                1.25f,
                true,
                true,
                false,
                0);
            for (int index = 0; index < 40; index++)
            {
                resolver.Evaluate(
                    Frame(++frame, resolver),
                    right,
                    seamLean,
                    left);
            }

            Assert.That(
                resolver.CurrentMode,
                Is.EqualTo(ShowcaseContextualCornerMode.RightLean));
            Assert.That(resolver.IsTransitioning, Is.False);
            Assert.That(resolver.TransitionCount, Is.EqualTo(transitionCount));
        }

        private static ShowcaseContextualCornerFrameSnapshot Frame(
            int frameId,
            ShowcaseContextualCornerAimResolver resolver,
            bool isAim = true,
            int resetRevision = 1)
        {
            return new ShowcaseContextualCornerFrameSnapshot(
                frameId,
                Step,
                resetRevision,
                0f,
                0f,
                isAim,
                false,
                CameraCarryMode.None,
                PrototypeThrowPresentationPhase.None,
                0,
                new Pose(Vector3.zero, Quaternion.identity),
                new Pose(Vector3.zero, Quaternion.identity),
                0.25f,
                1.75f,
                0.75f,
                0f,
                0.08f,
                48f,
                16f / 9f,
                0.1f,
                resolver.CurrentMode,
                resolver.CurrentBlendProgress,
                resolver.ActiveBlockerId,
                8f);
        }

        private static ShowcaseContextualCornerCandidate Candidate(
            ShowcaseContextualCornerMode mode,
            float effectiveClearance,
            bool solved,
            bool fullyPreserved,
            bool staticWorldConstraint,
            int blockerId)
        {
            const float requestedBoom = 2f;
            float preservation = Mathf.Clamp01(effectiveClearance / requestedBoom);
            return new ShowcaseContextualCornerCandidate(
                mode,
                mode == ShowcaseContextualCornerMode.RightDefault
                    ? ShowcaseContextualCornerAimResolver.RightDefaultShoulderFactor
                    : mode == ShowcaseContextualCornerMode.RightLean
                        ? ShowcaseContextualCornerAimResolver.RightLeanShoulderFactor
                        : ShowcaseContextualCornerAimResolver.TemporaryLeftShoulderFactor,
                solved,
                fullyPreserved,
                staticWorldConstraint,
                solved,
                solved,
                requestedBoom,
                solved ? requestedBoom : 0f,
                preservation,
                8f,
                blockerId);
        }
    }
}
