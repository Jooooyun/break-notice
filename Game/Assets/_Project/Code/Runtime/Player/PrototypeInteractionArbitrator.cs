namespace BreakNotice.Gameplay.Interaction
{
    public sealed class PrototypeInteractionArbitrator
    {
        public PrototypeInteractionDecision Decide(PrototypeInteractionContext context)
        {
            if (context.GlobalState != PrototypeGlobalInteractionState.Playable)
            {
                return PrototypeInteractionDecision.Reject(
                    PrototypeInteractionRejection.GlobalStateBlocked,
                    context.Target);
            }

            if (context.HandState != PrototypeHandOwnershipState.Free)
            {
                return PrototypeInteractionDecision.Reject(
                    PrototypeInteractionRejection.HandOwnershipBlocked,
                    context.Target);
            }

            if (context.StrapState != PrototypeStrapOwnershipState.Stored)
            {
                return PrototypeInteractionDecision.Reject(
                    PrototypeInteractionRejection.StrapOwnershipBlocked,
                    context.Target);
            }

            if (context.RelayState == PrototypeRelayReservationState.Active)
            {
                return DecideActiveRelay(context);
            }

            if (context.ExplicitStrapContext)
            {
                return IsValidStrapTarget(context.Target)
                    ? PrototypeInteractionDecision.Accept(
                        PrototypeInteractionOutcome.AttachStrap,
                        context.Target)
                    : PrototypeInteractionDecision.Reject(
                        PrototypeInteractionRejection.InvalidExplicitStrapContext,
                        context.Target);
            }

            if (context.Target == null)
            {
                return PrototypeInteractionDecision.Reject(
                    PrototypeInteractionRejection.NoValidTarget);
            }

            if (context.Target.WeightClass == PrototypeWeightClass.Heavy)
            {
                return PrototypeInteractionDecision.Reject(
                    PrototypeInteractionRejection.HeavyRecognitionOnly,
                    context.Target);
            }

            return context.Target.IsScoopEligible
                ? PrototypeInteractionDecision.Accept(
                    PrototypeInteractionOutcome.Scoop,
                    context.Target)
                : PrototypeInteractionDecision.Reject(
                    PrototypeInteractionRejection.DirectAcquisitionProhibited,
                    context.Target);
        }

        private static PrototypeInteractionDecision DecideActiveRelay(
            PrototypeInteractionContext context)
        {
            if (context.ExplicitStrapContext)
            {
                return IsValidStrapTarget(context.Target)
                    ? PrototypeInteractionDecision.Accept(
                        PrototypeInteractionOutcome.AttachStrap,
                        context.Target)
                    : PrototypeInteractionDecision.Reject(
                        PrototypeInteractionRejection.InvalidExplicitStrapContext,
                        context.Target);
            }

            if (context.Target != null &&
                context.Target == context.RelayReservedTarget &&
                context.Target.CatchEligible)
            {
                return PrototypeInteractionDecision.Accept(
                    PrototypeInteractionOutcome.Catch,
                    context.Target);
            }

            return PrototypeInteractionDecision.Reject(
                PrototypeInteractionRejection.InvalidRelayContext,
                context.Target);
        }

        private static bool IsValidStrapTarget(PrototypeInteractionTarget target)
        {
            return target != null &&
                   target.WeightClass == PrototypeWeightClass.Medium &&
                   target.StrapEligible &&
                   target.HasLoadBearingMetalAnchor;
        }
    }
}
