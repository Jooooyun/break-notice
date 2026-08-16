using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    [DisallowMultipleComponent]
    public sealed class PrototypeInteractionStateOwner : MonoBehaviour
    {
        public PrototypeGlobalInteractionState GlobalState { get; private set; } =
            PrototypeGlobalInteractionState.Playable;

        public PrototypeHandOwnershipState HandState { get; private set; } =
            PrototypeHandOwnershipState.Free;

        public PrototypeStrapOwnershipState StrapState { get; private set; } =
            PrototypeStrapOwnershipState.Stored;

        public PrototypeRelayReservationState RelayState { get; private set; } =
            PrototypeRelayReservationState.None;

        public PrototypeInteractionTarget CarriedTarget { get; private set; }

        public PrototypeInteractionTarget AcquisitionCommittedTarget { get; private set; }

        public PrototypeInteractionTarget ThrowCommittedTarget { get; private set; }

        public PrototypeInteractionTarget StrapTarget { get; private set; }

        public PrototypeInteractionTarget RelayReservedTarget { get; private set; }

        public bool QueuedThrowArmed { get; private set; }

        public bool BlocksNewInteraction =>
            GlobalState == PrototypeGlobalInteractionState.FailurePending ||
            GlobalState == PrototypeGlobalInteractionState.ResetPending;

        public PrototypeInteractionContext CreateContext(
            bool explicitStrapContext,
            PrototypeInteractionTarget target)
        {
            return new PrototypeInteractionContext(
                GlobalState,
                HandState,
                StrapState,
                RelayState,
                RelayReservedTarget,
                explicitStrapContext,
                target);
        }

        public bool TryCommit(PrototypeInteractionDecision decision)
        {
            if (!decision.IsAccepted || decision.Target == null || BlocksNewInteraction)
            {
                return false;
            }

            if (HandState != PrototypeHandOwnershipState.Free ||
                StrapState != PrototypeStrapOwnershipState.Stored)
            {
                return false;
            }

            switch (decision.Outcome)
            {
                case PrototypeInteractionOutcome.Scoop:
                    if (!decision.Target.IsScoopEligible || RelayState != PrototypeRelayReservationState.None)
                    {
                        return false;
                    }

                    AcquisitionCommittedTarget = decision.Target;
                    HandState = PrototypeHandOwnershipState.AcquisitionCommitted;
                    return true;

                case PrototypeInteractionOutcome.Catch:
                    if (!decision.Target.CatchEligible ||
                        RelayState != PrototypeRelayReservationState.Active ||
                        RelayReservedTarget != decision.Target)
                    {
                        return false;
                    }

                    bool consumeQueuedThrow = QueuedThrowArmed;
                    ClearRelayReservation();
                    if (consumeQueuedThrow)
                    {
                        ThrowCommittedTarget = decision.Target;
                        HandState = PrototypeHandOwnershipState.ThrowCommitted;
                    }
                    else
                    {
                        AcquisitionCommittedTarget = decision.Target;
                        HandState = PrototypeHandOwnershipState.AcquisitionCommitted;
                    }

                    return true;

                case PrototypeInteractionOutcome.AttachStrap:
                    if (!decision.Target.StrapEligible ||
                        !decision.Target.HasLoadBearingMetalAnchor ||
                        decision.Target.WeightClass != PrototypeWeightClass.Medium)
                    {
                        return false;
                    }

                    StrapTarget = decision.Target;
                    StrapState = PrototypeStrapOwnershipState.Attached;
                    QueuedThrowArmed = false;
                    ClearRelayReservation();
                    return true;

                default:
                    return false;
            }
        }

        public bool TryCommitThrow(out PrototypeInteractionTarget target)
        {
            target = CarriedTarget;
            if (BlocksNewInteraction ||
                HandState != PrototypeHandOwnershipState.Carrying ||
                target == null ||
                StrapState != PrototypeStrapOwnershipState.Stored)
            {
                return false;
            }

            CarriedTarget = null;
            ThrowCommittedTarget = target;
            HandState = PrototypeHandOwnershipState.ThrowCommitted;
            return true;
        }

        public bool CancelCarry(PrototypeInteractionTarget target)
        {
            if (HandState != PrototypeHandOwnershipState.Carrying || CarriedTarget != target)
            {
                return false;
            }

            CarriedTarget = null;
            HandState = PrototypeHandOwnershipState.Free;
            return true;
        }

        public bool TryCompleteAcquisition(PrototypeInteractionTarget target)
        {
            if (BlocksNewInteraction ||
                HandState != PrototypeHandOwnershipState.AcquisitionCommitted ||
                AcquisitionCommittedTarget != target ||
                target == null)
            {
                return false;
            }

            AcquisitionCommittedTarget = null;
            CarriedTarget = target;
            HandState = PrototypeHandOwnershipState.Carrying;
            return true;
        }

        public bool CancelAcquisition(PrototypeInteractionTarget target)
        {
            if (HandState != PrototypeHandOwnershipState.AcquisitionCommitted ||
                AcquisitionCommittedTarget != target)
            {
                return false;
            }

            AcquisitionCommittedTarget = null;
            HandState = PrototypeHandOwnershipState.Free;
            return true;
        }

        public bool CompleteThrow(PrototypeInteractionTarget target)
        {
            if (HandState != PrototypeHandOwnershipState.ThrowCommitted ||
                ThrowCommittedTarget != target)
            {
                return false;
            }

            ThrowCommittedTarget = null;
            HandState = PrototypeHandOwnershipState.Free;
            return true;
        }

        public bool TryReserveRelay(PrototypeInteractionTarget target)
        {
            if (target == null ||
                BlocksNewInteraction ||
                HandState != PrototypeHandOwnershipState.Free ||
                StrapState != PrototypeStrapOwnershipState.Stored ||
                RelayState != PrototypeRelayReservationState.None ||
                (!target.CatchEligible && !target.StrapEligible))
            {
                return false;
            }

            RelayReservedTarget = target;
            RelayState = PrototypeRelayReservationState.Active;
            return true;
        }

        public void ClearRelayReservation()
        {
            RelayReservedTarget = null;
            RelayState = PrototypeRelayReservationState.None;
            QueuedThrowArmed = false;
        }

        public bool TryArmQueuedThrow()
        {
            if (BlocksNewInteraction ||
                RelayState != PrototypeRelayReservationState.Active ||
                RelayReservedTarget == null ||
                !RelayReservedTarget.CatchEligible ||
                HandState != PrototypeHandOwnershipState.Free ||
                StrapState != PrototypeStrapOwnershipState.Stored ||
                QueuedThrowArmed)
            {
                return false;
            }

            QueuedThrowArmed = true;
            return true;
        }

        public bool TryReleaseStrap()
        {
            if (StrapState == PrototypeStrapOwnershipState.Stored)
            {
                return false;
            }

            StrapTarget = null;
            StrapState = PrototypeStrapOwnershipState.Stored;
            return true;
        }

        public bool TryBeginStrapTension(PrototypeInteractionTarget target)
        {
            if (BlocksNewInteraction ||
                StrapState != PrototypeStrapOwnershipState.Attached ||
                StrapTarget == null ||
                StrapTarget != target)
            {
                return false;
            }

            StrapState = PrototypeStrapOwnershipState.Tensioned;
            return true;
        }

        public bool TryBeginFailurePending()
        {
            if (GlobalState != PrototypeGlobalInteractionState.Playable)
            {
                return false;
            }

            GlobalState = PrototypeGlobalInteractionState.FailurePending;
            return true;
        }

        public bool TryBeginReset()
        {
            if (GlobalState == PrototypeGlobalInteractionState.ResetPending)
            {
                return false;
            }

            GlobalState = PrototypeGlobalInteractionState.ResetPending;
            return true;
        }

        public bool RestoreInteractionBaseline()
        {
            if (GlobalState != PrototypeGlobalInteractionState.ResetPending)
            {
                return false;
            }

            HandState = PrototypeHandOwnershipState.Free;
            StrapState = PrototypeStrapOwnershipState.Stored;
            RelayState = PrototypeRelayReservationState.None;
            CarriedTarget = null;
            AcquisitionCommittedTarget = null;
            ThrowCommittedTarget = null;
            StrapTarget = null;
            RelayReservedTarget = null;
            QueuedThrowArmed = false;
            GlobalState = PrototypeGlobalInteractionState.Playable;
            return true;
        }
    }
}
