using System;
using BreakNotice.Gameplay.Encounter;
using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    public enum ShowcaseQContext
    {
        None,
        Blocked,
        BreakGrab,
        TeamFinish,
        AssistAttach,
        CombatTag,
        SendSupply,
        ToggleFollow
    }

    public enum ShowcaseAllyAction
    {
        Idle,
        Blocked,
        Follow,
        Supply,
        Brace,
        Rescue,
        CombatTag,
        Finisher
    }

    [Flags]
    public enum ShowcaseTeamReadinessBadge
    {
        None = 0,
        Pass = 1 << 0,
        Brace = 1 << 1,
        Break = 1 << 2
    }

    public readonly struct ShowcaseQContextSnapshot
    {
        public ShowcaseQContextSnapshot(
            bool globalBlocked,
            bool playerGrabbed,
            bool finisherWindow,
            bool attachAssist,
            bool combatTag,
            bool supplyContext,
            bool followContext)
        {
            GlobalBlocked = globalBlocked;
            PlayerGrabbed = playerGrabbed;
            FinisherWindow = finisherWindow;
            AttachAssist = attachAssist;
            CombatTag = combatTag;
            SupplyContext = supplyContext;
            FollowContext = followContext;
        }

        public bool GlobalBlocked { get; }

        public bool PlayerGrabbed { get; }

        public bool FinisherWindow { get; }

        public bool AttachAssist { get; }

        public bool CombatTag { get; }

        public bool SupplyContext { get; }

        public bool FollowContext { get; }
    }

    public static class ShowcaseQContextArbitrator
    {
        public static ShowcaseQContext Resolve(ShowcaseQContextSnapshot snapshot)
        {
            if (snapshot.GlobalBlocked)
            {
                return ShowcaseQContext.Blocked;
            }

            if (snapshot.PlayerGrabbed)
            {
                return ShowcaseQContext.BreakGrab;
            }

            if (snapshot.FinisherWindow)
            {
                return ShowcaseQContext.TeamFinish;
            }

            if (snapshot.AttachAssist)
            {
                return ShowcaseQContext.AssistAttach;
            }

            if (snapshot.CombatTag)
            {
                return ShowcaseQContext.CombatTag;
            }

            if (snapshot.SupplyContext)
            {
                return ShowcaseQContext.SendSupply;
            }

            return snapshot.FollowContext
                ? ShowcaseQContext.ToggleFollow
                : ShowcaseQContext.None;
        }
    }

    [DefaultExecutionOrder(80)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseTeamContextController : MonoBehaviour
    {
        [SerializeField]
        private PrototypeInteractionStateOwner interactionOwner;

        [SerializeField]
        private PrototypeAllyRelayController allyRelayController;

        [SerializeField]
        private ShowcaseMobileAllyController mobileAllyController;

        [SerializeField]
        private AuthoredEncounterResetController resetController;

        [SerializeField]
        private ShowcaseCargoStrapController cargoStrapController;

        [SerializeField]
        private ShowcaseRunnerPressureController runnerPressureController;

        private PrototypeAllyRelayController subscribedRelay;
        private ShowcaseCargoStrapController subscribedCargoStrap;
        private int observedResetCount;
        private bool playerGrabbedAvailable;
        private bool finisherWindowAvailable;
        private bool attachAssistAvailable;
        private bool combatTagAvailable;

        public ShowcaseTeamReadinessBadge Badges { get; private set; }

        public bool HasPass => HasBadge(ShowcaseTeamReadinessBadge.Pass);

        public bool HasBrace => HasBadge(ShowcaseTeamReadinessBadge.Brace);

        public bool HasBreak => HasBadge(ShowcaseTeamReadinessBadge.Break);

        public int BadgeCount =>
            (HasPass ? 1 : 0) +
            (HasBrace ? 1 : 0) +
            (HasBreak ? 1 : 0);

        public bool IsTeamReady => BadgeCount >= 2;

        public uint ReadinessRevision { get; private set; }

        public ShowcaseQContext CurrentQContext { get; private set; }

        public ShowcaseQContext LastQContext { get; private set; }

        public bool LastQAccepted { get; private set; }

        public uint QAttemptSequence { get; private set; }

        public ShowcaseAllyAction CurrentAllyAction
        {
            get
            {
                ShowcaseQContext context = ResolveCurrentQContext();
                return context switch
                {
                    ShowcaseQContext.Blocked => ShowcaseAllyAction.Blocked,
                    ShowcaseQContext.BreakGrab => ShowcaseAllyAction.Rescue,
                    ShowcaseQContext.TeamFinish => ShowcaseAllyAction.Finisher,
                    ShowcaseQContext.AssistAttach => ShowcaseAllyAction.Brace,
                    ShowcaseQContext.CombatTag => ShowcaseAllyAction.CombatTag,
                    ShowcaseQContext.SendSupply => ShowcaseAllyAction.Supply,
                    _ when mobileAllyController != null &&
                           mobileAllyController.IsFollowCommanded =>
                        ShowcaseAllyAction.Follow,
                    _ => ShowcaseAllyAction.Idle
                };
            }
        }

        public PrototypeAllyRelayController AllyRelayController => allyRelayController;

        public ShowcaseMobileAllyController MobileAllyController => mobileAllyController;

        public ShowcaseCargoStrapController CargoStrapController => cargoStrapController;

        public ShowcaseRunnerPressureController RunnerPressureController =>
            runnerPressureController;

        private void OnEnable()
        {
            RefreshRelaySubscription();
            RefreshCargoStrapSubscription();
        }

        private void OnDisable()
        {
            RemoveRelaySubscription();
            RemoveCargoStrapSubscription();
        }

        private void Update()
        {
            if (resetController != null && resetController.ResetCount != observedResetCount)
            {
                observedResetCount = resetController.ResetCount;
                ResetToRunBaseline();
            }

            ResolveCurrentQContext();
        }

        public void Configure(
            PrototypeInteractionStateOwner owner,
            PrototypeAllyRelayController relay,
            ShowcaseMobileAllyController mobileAlly,
            AuthoredEncounterResetController reset)
        {
            RemoveRelaySubscription();
            interactionOwner = owner;
            allyRelayController = relay;
            mobileAllyController = mobileAlly;
            resetController = reset;
            observedResetCount = resetController != null ? resetController.ResetCount : 0;
            RefreshRelaySubscription();
            ResolveCurrentQContext();
        }

        public void ConfigureCargoStrap(ShowcaseCargoStrapController cargoStrap)
        {
            RemoveCargoStrapSubscription();
            cargoStrapController = cargoStrap;
            RefreshCargoStrapSubscription();
            ResolveCurrentQContext();
        }

        public void ConfigureRunnerPressure(ShowcaseRunnerPressureController runnerPressure)
        {
            runnerPressureController = runnerPressure;
            ResolveCurrentQContext();
        }

        public ShowcaseQContext ResolveCurrentQContext()
        {
            bool supplyContext = allyRelayController != null &&
                (allyRelayController.PreparedTarget != null ||
                 allyRelayController.RelayPhase != PrototypeRelayPhase.None);
            bool followContext = mobileAllyController != null &&
                mobileAllyController.OwnsRegroupFallback;
            CurrentQContext = ShowcaseQContextArbitrator.Resolve(
                new ShowcaseQContextSnapshot(
                    interactionOwner == null || interactionOwner.BlocksNewInteraction,
                    playerGrabbedAvailable ||
                        (runnerPressureController != null &&
                         runnerPressureController.PlayerGrabbed),
                    finisherWindowAvailable,
                    attachAssistAvailable ||
                        (cargoStrapController != null &&
                         cargoStrapController.CanAssistAttach()),
                    combatTagAvailable ||
                        (runnerPressureController != null &&
                         runnerPressureController.HasCombatTagTarget),
                    supplyContext,
                    followContext));
            return CurrentQContext;
        }

        public bool IsContextAvailable(ShowcaseQContext context)
        {
            if (context != ResolveCurrentQContext())
            {
                return false;
            }

            return context switch
            {
                ShowcaseQContext.SendSupply =>
                    allyRelayController != null && allyRelayController.IsReadyForCall,
                ShowcaseQContext.ToggleFollow =>
                    mobileAllyController != null && mobileAllyController.OwnsRegroupFallback,
                ShowcaseQContext.BreakGrab => playerGrabbedAvailable ||
                    (runnerPressureController != null &&
                     runnerPressureController.PlayerGrabbed),
                ShowcaseQContext.TeamFinish => finisherWindowAvailable,
                ShowcaseQContext.AssistAttach =>
                    attachAssistAvailable ||
                    (cargoStrapController != null &&
                     cargoStrapController.CanAssistAttach()),
                ShowcaseQContext.CombatTag => combatTagAvailable ||
                    (runnerPressureController != null &&
                     runnerPressureController.HasCombatTagTarget),
                _ => false
            };
        }

        public void RecordQAttempt(ShowcaseQContext context, bool accepted)
        {
            QAttemptSequence++;
            LastQContext = context;
            LastQAccepted = accepted;
            ResolveCurrentQContext();
        }

        public bool TryExecuteAssistAttach()
        {
            if (ResolveCurrentQContext() != ShowcaseQContext.AssistAttach ||
                cargoStrapController == null ||
                !cargoStrapController.TryAssistAttach(out _))
            {
                return false;
            }

            return true;
        }

        public bool TryExecuteBreakGrab()
        {
            if (ResolveCurrentQContext() != ShowcaseQContext.BreakGrab ||
                runnerPressureController == null ||
                !runnerPressureController.TryRescuePlayer())
            {
                return false;
            }

            RecordBreakSuccess();
            return true;
        }

        public bool TryExecuteCombatTag()
        {
            if (ResolveCurrentQContext() != ShowcaseQContext.CombatTag ||
                runnerPressureController == null ||
                !runnerPressureController.TryCombatTag())
            {
                return false;
            }

            RecordBreakSuccess();
            return true;
        }

        public void SetFutureContextAvailability(
            bool playerGrabbed,
            bool finisherWindow,
            bool attachAssist,
            bool combatTag)
        {
            playerGrabbedAvailable = playerGrabbed;
            finisherWindowAvailable = finisherWindow;
            attachAssistAvailable = attachAssist;
            combatTagAvailable = combatTag;
            ResolveCurrentQContext();
        }

        public bool RecordPassSuccess()
        {
            return TryAwardBadge(ShowcaseTeamReadinessBadge.Pass);
        }

        public bool RecordBraceSuccess()
        {
            return TryAwardBadge(ShowcaseTeamReadinessBadge.Brace);
        }

        public bool RecordBreakSuccess()
        {
            return TryAwardBadge(ShowcaseTeamReadinessBadge.Break);
        }

        public void ResetToRunBaseline()
        {
            bool readinessChanged = Badges != ShowcaseTeamReadinessBadge.None;
            Badges = ShowcaseTeamReadinessBadge.None;
            playerGrabbedAvailable = false;
            finisherWindowAvailable = false;
            attachAssistAvailable = false;
            combatTagAvailable = false;
            CurrentQContext = ShowcaseQContext.None;
            LastQContext = ShowcaseQContext.None;
            LastQAccepted = false;
            QAttemptSequence = 0;
            if (readinessChanged)
            {
                ReadinessRevision++;
            }

            cargoStrapController?.ResetToRunBaseline();

            ResolveCurrentQContext();
        }

        private bool HasBadge(ShowcaseTeamReadinessBadge badge)
        {
            return (Badges & badge) != 0;
        }

        private bool TryAwardBadge(ShowcaseTeamReadinessBadge badge)
        {
            if (badge == ShowcaseTeamReadinessBadge.None || HasBadge(badge))
            {
                return false;
            }

            Badges |= badge;
            ReadinessRevision++;
            return true;
        }

        private void HandleCatchCompleted(PrototypeInteractionTarget target)
        {
            if (target != null &&
                target.Role == PrototypeInteractionRole.AllyBinder &&
                target.WeightClass == PrototypeWeightClass.Light)
            {
                RecordPassSuccess();
            }
        }

        private void HandleAssistedAttachCompleted(PrototypeInteractionTarget target)
        {
            if (target != null)
            {
                RecordBraceSuccess();
            }
        }

        private void RefreshCargoStrapSubscription()
        {
            if (!isActiveAndEnabled || subscribedCargoStrap == cargoStrapController)
            {
                return;
            }

            RemoveCargoStrapSubscription();
            subscribedCargoStrap = cargoStrapController;
            if (subscribedCargoStrap != null)
            {
                subscribedCargoStrap.AssistedAttachCompleted +=
                    HandleAssistedAttachCompleted;
            }
        }

        private void RemoveCargoStrapSubscription()
        {
            if (subscribedCargoStrap != null)
            {
                subscribedCargoStrap.AssistedAttachCompleted -=
                    HandleAssistedAttachCompleted;
                subscribedCargoStrap = null;
            }
        }

        private void RefreshRelaySubscription()
        {
            if (!isActiveAndEnabled || subscribedRelay == allyRelayController)
            {
                return;
            }

            RemoveRelaySubscription();
            subscribedRelay = allyRelayController;
            if (subscribedRelay != null)
            {
                subscribedRelay.CatchCompleted += HandleCatchCompleted;
            }
        }

        private void RemoveRelaySubscription()
        {
            if (subscribedRelay != null)
            {
                subscribedRelay.CatchCompleted -= HandleCatchCompleted;
                subscribedRelay = null;
            }
        }
    }
}
