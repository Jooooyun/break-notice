namespace BreakNotice.Gameplay.Combat
{
    /// <summary>
    /// Source-owned timing authority for the latest CHR01 and ENY01 delivery.
    /// Keep importer AnimationEvents and runtime state speeds on this surface so
    /// physical release and bone-hit facts cannot drift from their FBX poses.
    /// </summary>
    public static class ShowcaseNonPlayerAnimationTiming
    {
        public const float EnemyRunLeftFootstep = 8f / 22f;
        public const float EnemyRunRightFootstep = 19f / 22f;
        // CHR01 Supply Release is trimmed from Unity source frames 21..44
        // (Blender 22..45). The hand opens at Blender frame 28 / Unity 27.
        public const float AllySupplyReleaseStateSpeed = 1.15f;
        public const float AllySupplyRelease = 6f / 23f;

        public const float BoxingStateSpeed = 1.25f;
        public const float BoxingWindowOpen = 0.36f;
        public const float BoxingWindowClose = 0.64f;

        public const float Boxing2StateSpeed = 1.30f;
        public const float Boxing2WindowOpen = 0.36f;
        public const float Boxing2WindowClose = 0.58f;

        public const float KickingStateSpeed = 1.20f;
        public const float KickingWindowOpen = 0.48f;
        public const float KickingWindowClose = 0.68f;

        public const float Kicking2StateSpeed = 1.20f;
        public const float Kicking2WindowOpen = 0.34f;
        public const float Kicking2WindowClose = 0.58f;

        public const float MutantPunchStateSpeed = 1f;
        public const float MutantPunchWindowOpen = 0.29f;
        public const float MutantPunchWindowClose = 0.54f;

        public const float MutantSwipeStateSpeed = 1f;
        public const float MutantSwipeWindowOpen = 0.42f;
        public const float MutantSwipeWindowClose = 0.64f;

        public const float EnemyActionComplete = 0.98f;
        public const float EnemyHitComplete = 0.98f;
    }
}
