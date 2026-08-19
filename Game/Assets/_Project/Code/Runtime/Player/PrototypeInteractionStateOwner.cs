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

        public PrototypeWeaponSelection WeaponSelection { get; private set; } =
            PrototypeWeaponSelection.Fists;

        public PrototypeInteractionTarget CarriedTarget { get; private set; }

        public PrototypeInteractionTarget AcquisitionCommittedTarget { get; private set; }

        public PrototypeInteractionTarget ThrowCommittedTarget { get; private set; }

        public PrototypeInteractionTarget StrapTarget { get; private set; }

        public PrototypeInteractionTarget RelayReservedTarget { get; private set; }

        public bool QueuedThrowArmed { get; private set; }

        public bool BlocksNewInteraction =>
            GlobalState == PrototypeGlobalInteractionState.FailurePending ||
            GlobalState == PrototypeGlobalInteractionState.ResetPending;

        public PrototypeInteractionTarget ObjectWeaponTarget
        {
            get
            {
                if (IsAvailableObjectWeaponTarget(CarriedTarget))
                {
                    return CarriedTarget;
                }

                if (StrapState != PrototypeStrapOwnershipState.Stored &&
                    IsAvailableObjectWeaponTarget(StrapTarget))
                {
                    return StrapTarget;
                }

                return RelayState == PrototypeRelayReservationState.Active &&
                       IsAvailableObjectWeaponTarget(RelayReservedTarget)
                    ? RelayReservedTarget
                    : null;
            }
        }

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
                    WeaponSelection = PrototypeWeaponSelection.ObjectWeapon;
                    return true;

                case PrototypeInteractionOutcome.Catch:
                    if (!decision.Target.CatchEligible ||
                        decision.Target.WeightClass == PrototypeWeightClass.Heavy ||
                        RelayState != PrototypeRelayReservationState.Active ||
                        RelayReservedTarget != decision.Target)
                    {
                        return false;
                    }

                    bool consumeQueuedThrow = QueuedThrowArmed;
                    QueuedThrowArmed = false;
                    RelayState = PrototypeRelayReservationState.CatchCommitted;
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

                    WeaponSelection = PrototypeWeaponSelection.ObjectWeapon;

                    return true;

                case PrototypeInteractionOutcome.AttachStrap:
                    if (!decision.Target.IsCargoStrapEligible)
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
                WeaponSelection != PrototypeWeaponSelection.ObjectWeapon ||
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
            SynchronizeWeaponSelection();
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
            WeaponSelection = PrototypeWeaponSelection.ObjectWeapon;
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
            SynchronizeWeaponSelection();
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
            SynchronizeWeaponSelection();
            return true;
        }

        public bool TryReserveRelay(PrototypeInteractionTarget target)
        {
            if (target == null ||
                BlocksNewInteraction ||
                target.WeightClass == PrototypeWeightClass.Heavy ||
                HandState != PrototypeHandOwnershipState.Free ||
                StrapState != PrototypeStrapOwnershipState.Stored ||
                RelayState != PrototypeRelayReservationState.None ||
                (!target.CatchEligible && !target.StrapEligible))
            {
                return false;
            }

            RelayReservedTarget = target;
            RelayState = PrototypeRelayReservationState.Active;
            WeaponSelection = PrototypeWeaponSelection.ObjectWeapon;
            return true;
        }

        public void ClearRelayReservation()
        {
            RelayReservedTarget = null;
            RelayState = PrototypeRelayReservationState.None;
            QueuedThrowArmed = false;
            SynchronizeWeaponSelection();
        }

        public bool TryCompleteRelayCatchReservation(
            PrototypeInteractionTarget target)
        {
            if (target == null ||
                RelayState != PrototypeRelayReservationState.CatchCommitted ||
                RelayReservedTarget != target)
            {
                return false;
            }

            RelayReservedTarget = null;
            RelayState = PrototypeRelayReservationState.None;
            QueuedThrowArmed = false;
            SynchronizeWeaponSelection();
            return true;
        }

        public bool TryCancelRelayReservation(PrototypeInteractionTarget target)
        {
            if (target == null || RelayReservedTarget != target ||
                (RelayState != PrototypeRelayReservationState.Active &&
                 RelayState != PrototypeRelayReservationState.CatchCommitted))
            {
                return false;
            }

            RelayReservedTarget = null;
            RelayState = PrototypeRelayReservationState.None;
            QueuedThrowArmed = false;
            SynchronizeWeaponSelection();
            return true;
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
            SynchronizeWeaponSelection();
            return true;
        }

        public bool TryBeginStrapRelease(PrototypeInteractionTarget target)
        {
            if (BlocksNewInteraction || target == null || StrapTarget != target ||
                (StrapState != PrototypeStrapOwnershipState.Attached &&
                 StrapState != PrototypeStrapOwnershipState.Tensioned))
            {
                return false;
            }

            StrapState = PrototypeStrapOwnershipState.ReleaseRequested;
            return true;
        }

        public bool TryCompleteStrapRelease(PrototypeInteractionTarget target)
        {
            if (target == null || StrapTarget != target ||
                StrapState != PrototypeStrapOwnershipState.ReleaseRequested)
            {
                return false;
            }

            StrapTarget = null;
            StrapState = PrototypeStrapOwnershipState.Stored;
            SynchronizeWeaponSelection();
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

        public bool TrySelectWeapon(PrototypeWeaponSelection selection)
        {
            if (BlocksNewInteraction)
            {
                return false;
            }

            if (selection == PrototypeWeaponSelection.Fists)
            {
                if (HandState != PrototypeHandOwnershipState.Free ||
                    RelayState != PrototypeRelayReservationState.None)
                {
                    return false;
                }

                WeaponSelection = PrototypeWeaponSelection.Fists;
                return true;
            }

            PrototypeInteractionTarget objectTarget = ObjectWeaponTarget;
            if (!IsAvailableObjectWeaponTarget(objectTarget))
            {
                return false;
            }

            WeaponSelection = PrototypeWeaponSelection.ObjectWeapon;
            return true;
        }

        public void SynchronizeWeaponSelection()
        {
            if (WeaponSelection != PrototypeWeaponSelection.ObjectWeapon ||
                ObjectWeaponTarget != null ||
                AcquisitionCommittedTarget != null ||
                ThrowCommittedTarget != null)
            {
                return;
            }

            WeaponSelection = PrototypeWeaponSelection.Fists;
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
            WeaponSelection = PrototypeWeaponSelection.Fists;
            GlobalState = PrototypeGlobalInteractionState.Playable;
            return true;
        }

        private static bool IsAvailableObjectWeaponTarget(
            PrototypeInteractionTarget target)
        {
            return target != null &&
                   target.WeightClass != PrototypeWeightClass.Heavy &&
                   !target.IsPlayerThrowExhausted &&
                   target.gameObject.activeInHierarchy;
        }
    }
}
