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

        public void Configure(
            ShowcasePlayerCombatController authoredPlayerCombat,
            ShowcaseRunnerAgent authoredRunnerAgent)
        {
            playerCombat = authoredPlayerCombat;
            runnerAgent = authoredRunnerAgent;
        }

        public void ShowcaseCombatHit()
        {
            if (playerCombat != null)
            {
                playerCombat.CommitAnimationCombatHit();
            }
            else
            {
                runnerAgent?.CommitAnimationAttackHit();
            }
        }

        public void ShowcaseThrowRelease()
        {
            playerCombat?.CommitAnimationThrowRelease();
        }

        public void ShowcaseCarryContact()
        {
            playerCombat?.CommitAnimationCarryContact();
        }
    }
}
