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
        private ShowcaseBoneCombatRig boneCombatRig;

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

        public void Configure(
            ShowcasePlayerCombatController authoredPlayerCombat,
            ShowcaseRunnerAgent authoredRunnerAgent,
            PrototypeAllyRelayController authoredAllyRelay = null)
        {
            playerCombat = authoredPlayerCombat;
            runnerAgent = authoredRunnerAgent;
            allyRelay = authoredAllyRelay;
            boneCombatRig = GetComponentInParent<ShowcaseBoneCombatRig>();
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
            boneCombatRig?.CloseAttackWindow();
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
    }
}
