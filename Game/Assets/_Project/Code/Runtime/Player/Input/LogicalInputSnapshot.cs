using UnityEngine;

namespace BreakNotice.Gameplay.Input
{
    public readonly struct LogicalInputSnapshot
    {
        public LogicalInputSnapshot(
            Vector2 move,
            Vector2 look,
            bool aim,
            bool interactPrimary,
            bool throwRequested,
            bool callAlly,
            bool strapRelease,
            bool resetEncounter,
            bool throwHeld = false,
            bool throwReleased = false,
            bool allyTask = false)
        {
            Move = move;
            Look = look;
            Aim = aim;
            InteractPrimary = interactPrimary;
            Throw = throwRequested;
            CallAlly = callAlly;
            StrapRelease = strapRelease;
            ResetEncounter = resetEncounter;
            ThrowHeld = throwHeld;
            ThrowReleased = throwReleased;
            AllyTask = allyTask;
        }

        public Vector2 Move { get; }

        public Vector2 Look { get; }

        public bool Aim { get; }

        public bool InteractPrimary { get; }

        public bool Throw { get; }

        public bool ThrowHeld { get; }

        public bool ThrowReleased { get; }

        public bool AllyTask { get; }

        public bool CallAlly { get; }

        public bool StrapRelease { get; }

        public bool ResetEncounter { get; }
    }
}
