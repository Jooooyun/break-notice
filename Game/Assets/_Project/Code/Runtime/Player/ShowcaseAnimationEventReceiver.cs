using System;
using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Gameplay.Combat
{
    [DisallowMultipleComponent]
    public sealed class ShowcaseAnimationEventReceiver : MonoBehaviour
    {
        [SerializeField]
        private ShowcasePlayerCombatController playerCombat;

        [SerializeField]
        private ShowcaseRunnerAgent runnerAgent;

        [SerializeField]
        private PrototypeAllyRelayController allyRelay;

        [SerializeField]
        private ShowcaseCargoStrapController cargoStrap;

        [SerializeField]
        private ShowcaseBoneCombatRig boneCombatRig;

        private uint cargoStrapAnimationRevision;

        public int CombatHitInvocationCount { get; private set; }
        public int ThrowReleaseInvocationCount { get; private set; }
        public int CarryContactInvocationCount { get; private set; }
        public int ActionCompleteInvocationCount { get; private set; }
        public int AllySupplyReleaseInvocationCount { get; private set; }
        public int AttackWindowOpenInvocationCount { get; private set; }
        public int AttackWindowCloseInvocationCount { get; private set; }
        public int ComboBufferOpenInvocationCount { get; private set; }
        public int ComboBufferCloseInvocationCount { get; private set; }
        public int LungeWindowOpenInvocationCount { get; private set; }
        public int LungeWindowCloseInvocationCount { get; private set; }
        public int StrapThrowReleaseInvocationCount { get; private set; }
        public int StrapAttachContactInvocationCount { get; private set; }
        public int StrapPullCompleteInvocationCount { get; private set; }
        public int StrapAttackImpulseInvocationCount { get; private set; }
        public int StrapActionCompleteInvocationCount { get; private set; }
        public int FootstepInvocationCount { get; private set; }
        public int ChairSwingWhooshInvocationCount { get; private set; }
        public uint CargoStrapAnimationRevision => cargoStrapAnimationRevision;

        public event Action<ShowcaseAnimationEventReceiver, int> FootstepContact;

        private void Awake()
        {
            cargoStrap ??= GetComponentInParent<ShowcaseCargoStrapController>();
            boneCombatRig ??= GetComponentInParent<ShowcaseBoneCombatRig>();
        }

        public void Configure(
            ShowcasePlayerCombatController authoredPlayerCombat,
            ShowcaseRunnerAgent authoredRunnerAgent,
            PrototypeAllyRelayController authoredAllyRelay = null,
            ShowcaseCargoStrapController authoredCargoStrap = null)
        {
            playerCombat = authoredPlayerCombat;
            runnerAgent = authoredRunnerAgent;
            allyRelay = authoredAllyRelay;
            cargoStrap = authoredCargoStrap ??
                GetComponentInParent<ShowcaseCargoStrapController>();
            boneCombatRig = GetComponentInParent<ShowcaseBoneCombatRig>();
        }

        public void SetCargoStrapAnimationRevision(uint revision)
        {
            cargoStrapAnimationRevision = revision;
        }

        public void ShowcaseCombatHit()
        {
            CombatHitInvocationCount++;
            // Legacy event compatibility only. Damage is never committed here.
            // Runtime combat is authorized exclusively by an open bone window and
            // a swept strike-versus-hurtbox overlap.
        }

        public void ShowcaseAttackWindowOpen()
        {
            AttackWindowOpenInvocationCount++;
            boneCombatRig?.OpenAttackWindow();
        }

        public void ShowcaseAttackWindowClose()
        {
            AttackWindowCloseInvocationCount++;
            uint closingSequence = boneCombatRig != null
                ? boneCombatRig.ActiveAttackSequence
                : 0;
            boneCombatRig?.CloseAttackWindow();
            runnerAgent?.NotifyAttackWindowClosed(closingSequence);
        }

        public void ShowcaseComboBufferOpen()
        {
            ComboBufferOpenInvocationCount++;
            playerCombat?.SetComboBufferOpen(true);
        }

        public void ShowcaseComboBufferClose()
        {
            ComboBufferCloseInvocationCount++;
            playerCombat?.SetComboBufferOpen(false);
        }

        public void ShowcaseLungeStart()
        {
            LungeWindowOpenInvocationCount++;
            runnerAgent?.SetAnimationLungeWindow(true);
        }

        public void ShowcaseLungeEnd()
        {
            LungeWindowCloseInvocationCount++;
            runnerAgent?.SetAnimationLungeWindow(false);
        }

        public void ShowcaseThrowRelease()
        {
            ThrowReleaseInvocationCount++;
            playerCombat?.CommitAnimationThrowRelease();
        }

        public void ShowcaseChairSwingWhoosh()
        {
            ChairSwingWhooshInvocationCount++;
            playerCombat?.CommitAnimationChairSwingWhoosh();
        }

        public void ShowcaseFootstep(int foot)
        {
            if (foot < 0 || foot > 1)
            {
                return;
            }

            FootstepInvocationCount++;
            FootstepContact?.Invoke(this, foot);
        }

        public void ShowcaseLeftFootstep()
        {
            ShowcaseFootstep(0);
        }

        public void ShowcaseRightFootstep()
        {
            ShowcaseFootstep(1);
        }

        public void ShowcaseCarryContact()
        {
            if (playerCombat == null)
            {
                return;
            }

            CarryContactInvocationCount++;
            playerCombat.CommitAnimationCarryContact();
        }

        public void ShowcaseActionComplete()
        {
            ActionCompleteInvocationCount++;
            if (playerCombat != null)
            {
                playerCombat.CompleteAnimationAction();
            }
            else
            {
                if (runnerAgent != null)
                {
                    runnerAgent.CompleteAnimationAction();
                }
            }
        }

        public void ShowcaseAllySupplyRelease()
        {
            AllySupplyReleaseInvocationCount++;
            allyRelay?.CommitAnimationSupplyRelease();
        }

        public void ShowcaseStrapThrowRelease()
        {
            StrapThrowReleaseInvocationCount++;
            cargoStrap?.CommitAnimationStrapThrowRelease(
                cargoStrapAnimationRevision);
        }

        public void ShowcaseStrapAttachContact()
        {
            StrapAttachContactInvocationCount++;
            cargoStrap?.CommitAnimationStrapAttachContact(
                cargoStrapAnimationRevision);
        }

        public void ShowcaseStrapPullComplete()
        {
            StrapPullCompleteInvocationCount++;
            cargoStrap?.CommitAnimationStrapPullComplete(
                cargoStrapAnimationRevision);
        }

        public void ShowcaseStrapAttackImpulse()
        {
            StrapAttackImpulseInvocationCount++;
            cargoStrap?.CommitAnimationStrapAttackImpulse(
                cargoStrapAnimationRevision);
        }

        public void ShowcaseStrapActionComplete()
        {
            StrapActionCompleteInvocationCount++;
            cargoStrap?.CompleteAnimationStrapAction(
                cargoStrapAnimationRevision);
        }
    }
}
