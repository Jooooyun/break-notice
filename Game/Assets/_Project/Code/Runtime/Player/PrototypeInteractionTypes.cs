namespace BreakNotice.Gameplay.Interaction
{
    public enum PrototypeWeightClass
    {
        Light,
        Medium,
        Heavy
    }

    public enum PrototypeInteractionRole
    {
        DirectBinder,
        AllyBinder,
        DirectChair,
        AllyChair,
        Drawer,
        Copier
    }

    public enum PrototypeGlobalInteractionState
    {
        Playable,
        FailurePending,
        ResetPending
    }

    public enum PrototypeHandOwnershipState
    {
        Free,
        Carrying,
        ThrowCommitted,
        HeavyCommitted
    }

    public enum PrototypeStrapOwnershipState
    {
        Stored,
        Attached,
        Tensioned,
        ReleaseRequested
    }

    public enum PrototypeRelayReservationState
    {
        None,
        Active
    }

    public enum PrototypeInteractionOutcome
    {
        None,
        Scoop,
        Catch,
        AttachStrap
    }

    public enum PrototypeInteractionRejection
    {
        None,
        NoValidTarget,
        GlobalStateBlocked,
        HandOwnershipBlocked,
        StrapOwnershipBlocked,
        InvalidExplicitStrapContext,
        InvalidRelayContext,
        DirectAcquisitionProhibited,
        HeavyRecognitionOnly,
        OwnershipConflict
    }

    public readonly struct PrototypeInteractionContext
    {
        public PrototypeInteractionContext(
            PrototypeGlobalInteractionState globalState,
            PrototypeHandOwnershipState handState,
            PrototypeStrapOwnershipState strapState,
            PrototypeRelayReservationState relayState,
            bool explicitStrapContext,
            PrototypeInteractionTarget target)
        {
            GlobalState = globalState;
            HandState = handState;
            StrapState = strapState;
            RelayState = relayState;
            ExplicitStrapContext = explicitStrapContext;
            Target = target;
        }

        public PrototypeGlobalInteractionState GlobalState { get; }

        public PrototypeHandOwnershipState HandState { get; }

        public PrototypeStrapOwnershipState StrapState { get; }

        public PrototypeRelayReservationState RelayState { get; }

        public bool ExplicitStrapContext { get; }

        public PrototypeInteractionTarget Target { get; }
    }

    public readonly struct PrototypeInteractionDecision
    {
        private PrototypeInteractionDecision(
            PrototypeInteractionOutcome outcome,
            PrototypeInteractionRejection rejection,
            PrototypeInteractionTarget target)
        {
            Outcome = outcome;
            Rejection = rejection;
            Target = target;
        }

        public PrototypeInteractionOutcome Outcome { get; }

        public PrototypeInteractionRejection Rejection { get; }

        public PrototypeInteractionTarget Target { get; }

        public bool IsAccepted => Outcome != PrototypeInteractionOutcome.None;

        public static PrototypeInteractionDecision Accept(
            PrototypeInteractionOutcome outcome,
            PrototypeInteractionTarget target)
        {
            return new PrototypeInteractionDecision(
                outcome,
                PrototypeInteractionRejection.None,
                target);
        }

        public static PrototypeInteractionDecision Reject(
            PrototypeInteractionRejection rejection,
            PrototypeInteractionTarget target = null)
        {
            return new PrototypeInteractionDecision(
                PrototypeInteractionOutcome.None,
                rejection,
                target);
        }
    }
}
