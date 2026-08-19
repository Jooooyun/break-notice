namespace BreakNotice.Gameplay.Combat
{
    /// <summary>
    /// Source-frame authority for CHR00 animation-owned gameplay boundaries.
    /// FBX AnimationEvents and runtime prop interpolation must both use these
    /// values so contact/release facts cannot drift apart.
    /// </summary>
    public static class ShowcaseChr00AnimationTiming
    {
        public const float PickupLowSmallStateSpeed = 3.4f;
        public const float PickupLowMediumStateSpeed = 3.0f;
        public const float MeleeHookStateSpeed = 1.8f;

        public static float PickupLowSmallContact => Normalize(37, 1, 144);
        public static float PickupLowSmallStabilize => Normalize(106, 1, 144);
        public static float PickupLowSmallComplete => Normalize(132, 1, 144);
        public static float PickupLowMediumContact => Normalize(37, 1, 144);
        public static float PickupLowMediumStabilize => Normalize(106, 1, 144);
        public static float PickupLowMediumComplete => Normalize(132, 1, 144);

        public static float CatchSmallCenterContact => Normalize(12, 1, 39);
        public static float CatchSmallCenterStabilize => Normalize(26, 1, 39);
        public static float CatchSmallCenterComplete => Normalize(39, 1, 39);
        public static float CatchSmallDivingContact => Normalize(48, 1, 82);
        public static float CatchSmallDivingStabilize => Normalize(66, 1, 82);
        public static float CatchSmallDivingComplete => Normalize(82, 1, 82);
        public static float CatchMediumContact => Normalize(12, 1, 39);
        public static float CatchMediumStabilize => Normalize(26, 1, 39);
        public static float CatchMediumComplete => Normalize(39, 1, 39);

        public static float ThrowSmallRelease => Normalize(48, 1, 86);
        public static float ThrowSmallComplete => Normalize(86, 1, 86);
        public static float ThrowMediumRelease => Normalize(49, 1, 90);
        public static float ThrowMediumComplete => Normalize(90, 1, 90);

        public static float MeleeJabWindowOpen => Normalize(7, 1, 14);
        public const float MeleeJabWindowClose = 0.97f;
        public const float MeleeJabComplete = 0.98f;
        public const float MeleeJabReturnComplete = 0.02f;
        public static float MeleeCrossWindowOpen => Normalize(17, 14, 60);
        public static float MeleeCrossWindowClose => Normalize(24, 14, 60);
        public static float MeleeCrossComplete => Normalize(36, 14, 60);

        // Combo buffer placement remains an input-feel policy. These values are
        // retained, but runtime input acceptance is required to honor them.
        public const float MeleeJabBufferOpen = 0.52f;
        public const float MeleeJabBufferClose = 0.78f;
        public static float MeleeCrossBufferOpen => Normalize(25, 14, 60);
        public static float MeleeCrossBufferClose => Normalize(34, 14, 60);

        public static float HitBodyComplete => Normalize(36, 1, 37);
        public static float KnockdownFrontComplete => Normalize(39, 1, 39);
        public static float KnockdownBackComplete => Normalize(29, 1, 33);
        public static float GetupFrontComplete => Normalize(69, 1, 70);
        public static float GetupBackComplete => Normalize(113, 1, 125);

        public static float JumpImpulse => Normalize(10, 1, 21);
        public static float RunStopComplete => Normalize(28, 1, 28);
        public static float WalkStopComplete => Normalize(90, 1, 91);
        public static float IdleVariantComplete => Normalize(266, 1, 267);

        public static bool TryGetAcquisitionTiming(
            ShowcasePlayerAnimationCue cue,
            out float contact,
            out float stabilize,
            out float complete)
        {
            switch (cue)
            {
                case ShowcasePlayerAnimationCue.PickupLowSmall:
                    contact = PickupLowSmallContact;
                    stabilize = PickupLowSmallStabilize;
                    complete = PickupLowSmallComplete;
                    return true;
                case ShowcasePlayerAnimationCue.PickupLowMedium:
                    contact = PickupLowMediumContact;
                    stabilize = PickupLowMediumStabilize;
                    complete = PickupLowMediumComplete;
                    return true;
                case ShowcasePlayerAnimationCue.CatchSmallCenter:
                    contact = CatchSmallCenterContact;
                    stabilize = CatchSmallCenterStabilize;
                    complete = CatchSmallCenterComplete;
                    return true;
                case ShowcasePlayerAnimationCue.CatchSmallLeft:
                case ShowcasePlayerAnimationCue.CatchSmallRight:
                    contact = CatchSmallDivingContact;
                    stabilize = CatchSmallDivingStabilize;
                    complete = CatchSmallDivingComplete;
                    return true;
                case ShowcasePlayerAnimationCue.CatchMedium:
                    contact = CatchMediumContact;
                    stabilize = CatchMediumStabilize;
                    complete = CatchMediumComplete;
                    return true;
                default:
                    contact = 0f;
                    stabilize = 0f;
                    complete = 0f;
                    return false;
            }
        }

        public static bool TryGetThrowTiming(
            ShowcasePlayerAnimationCue cue,
            out float release,
            out float complete)
        {
            switch (cue)
            {
                case ShowcasePlayerAnimationCue.ThrowSmall:
                    release = ThrowSmallRelease;
                    complete = ThrowSmallComplete;
                    return true;
                case ShowcasePlayerAnimationCue.ThrowMedium:
                    release = ThrowMediumRelease;
                    complete = ThrowMediumComplete;
                    return true;
                default:
                    release = 0f;
                    complete = 0f;
                    return false;
            }
        }

        private static float Normalize(int frame, int firstFrame, int lastFrame)
        {
            if (lastFrame <= firstFrame)
            {
                return 0f;
            }

            float normalized = (frame - firstFrame) / (float)(lastFrame - firstFrame);
            return normalized < 0f ? 0f : normalized > 1f ? 1f : normalized;
        }
    }
}
