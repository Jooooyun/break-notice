using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Encounter;
using BreakNotice.Gameplay.Input;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestShowcaseCombatPlayModeTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v007.unity";

        private Scene loadedScene;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(loadedScene);
                if (unload != null)
                {
                    yield return unload;
                }
            }

            loadedScene = default;
        }

        [UnityTest]
        public IEnumerator Scene_ComposesExactPlayerNormalAndEliteHealthContracts()
        {
            yield return LoadScene();

            ShowcaseCombatHealth[] health = SceneComponents<ShowcaseCombatHealth>();
            Assert.That(health, Has.Length.EqualTo(3));
            Assert.That(health.Single(item => item.Team == ShowcaseCombatTeam.Player)
                .MaximumHealth, Is.EqualTo(100));

            ShowcaseRunnerAgent[] runners = SceneComponents<ShowcaseRunnerAgent>();
            Assert.That(runners, Has.Length.EqualTo(2));
            Assert.That(runners.Single(item => !item.IsElite).Health.MaximumHealth,
                Is.EqualTo(100));
            Assert.That(runners.Single(item => item.IsElite).Health.MaximumHealth,
                Is.EqualTo(180));
            Assert.That(runners.All(item => !item.IsDead), Is.True);
            ShowcaseCharacterAnimationDriver allyDriver = SceneComponents<
                    ShowcaseCharacterAnimationDriver>()
                .Single(item => item.Role == ShowcaseCharacterAnimationRole.Ally);
            ShowcaseChairGripRig chairGrip =
                allyDriver.Animator.GetComponent<ShowcaseChairGripRig>();
            Assert.That(chairGrip, Is.Not.Null);
            Assert.That(chairGrip.IsAvailable, Is.True);
        }

        [UnityTest]
        public IEnumerator PlayerCombo_DealsFifteenTwentyThirtyFiveOnlyAtBoneOverlapWindows()
        {
            yield return LoadScene();

            ShowcasePlayerCombatController combat =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            ShowcaseRunnerAgent target = pressure.Runners.Single(item => !item.IsElite);
            ShowcaseAnimationEventReceiver receiver =
                combat.GetComponent<BreakNotice.Presentation.ShowcaseCharacterAnimationDriver>()
                    .Animator.GetComponent<ShowcaseAnimationEventReceiver>();
            ShowcaseBoneCombatRig combatRig =
                combat.GetComponent<ShowcaseBoneCombatRig>();

            foreach (ShowcaseRunnerAgent runner in pressure.Runners)
            {
                runner.enabled = false;
            }
            PlaceRunnerInFront(combat.transform, target);
            PrototypeInteractionController interaction =
                combat.GetComponent<PrototypeInteractionController>();
            InputSystemLogicalInputAdapter input =
                combat.GetComponent<InputSystemLogicalInputAdapter>();

            Assert.That(combat.TryRequestMelee(target), Is.True);
            Assert.That(combat.ActiveMeleeStep, Is.EqualTo(1));
            yield return WaitUntilOrFail(
                () => receiver.ComboBufferOpenInvocationCount >= 1,
                2f,
                "Jab combo-buffer AnimationEvent did not fire.");
            Assert.That(combat.TryRequestMelee(target), Is.True);
            Assert.That(combat.HasBufferedMelee, Is.True);
            yield return WaitUntilOrFail(
                () => receiver.AttackWindowOpenInvocationCount >= 1,
                2f,
                "Jab attack-window AnimationEvent did not fire.");
            yield return WaitUntilOrFail(
                () => receiver.AttackWindowCloseInvocationCount >= 1,
                1f,
                "Jab attack-window close AnimationEvent did not fire.");
            Assert.That(combatRig.AcceptedHitCount, Is.GreaterThanOrEqualTo(1),
                $"Jab bones did not overlap the runner hurtbox. " +
                $"Minimum surface separation: {combatRig.MinimumWindowSurfaceSeparation:F4} m; " +
                $"bounded approach: {combat.MeleeApproachDistance:F4} m; " +
                $"root separation: {Vector3.Distance(combat.transform.position, target.transform.position):F4} m.");
            Assert.That(target.Health.CurrentHealth, Is.EqualTo(85));
            yield return WaitUntilOrFail(
                () => combat.ActiveMeleeStep == 2,
                2f,
                "Jab completion did not start Cross.");

            Assert.That(combat.ActiveMeleeStep, Is.EqualTo(2));
            yield return WaitUntilOrFail(
                () => receiver.ComboBufferOpenInvocationCount >= 2,
                2f,
                "Cross combo-buffer AnimationEvent did not fire.");
            Assert.That(combat.TryRequestMelee(target), Is.True);
            Assert.That(combat.HasBufferedMelee, Is.True);
            yield return WaitUntilOrFail(
                () => receiver.AttackWindowOpenInvocationCount >= 2,
                2f,
                "Cross attack-window AnimationEvent did not fire.");
            yield return WaitUntilOrFail(
                () => combatRig.AcceptedHitCount >= 2,
                1f,
                "Cross bones did not overlap the runner hurtbox.");
            Assert.That(target.Health.CurrentHealth, Is.EqualTo(65));
            yield return WaitUntilOrFail(
                () => combat.ActiveMeleeStep == 3,
                2f,
                "Cross completion did not start Hook.");

            Assert.That(combat.ActiveMeleeStep, Is.EqualTo(3));
            yield return WaitUntilOrFail(
                () => receiver.AttackWindowOpenInvocationCount >= 3,
                2f,
                "Hook attack-window AnimationEvent did not fire.");
            yield return WaitUntilOrFail(
                () => combatRig.AcceptedHitCount >= 3,
                1f,
                "Hook bones did not overlap the runner hurtbox.");
            Assert.That(target.Health.CurrentHealth, Is.EqualTo(30));
            Assert.That(target.State, Is.EqualTo(ShowcaseRunnerState.KnockedDown));
        }

        [UnityTest]
        public IEnumerator PlayerMelee_OpenWindowWithoutBoneOverlapDealsNoDamage()
        {
            yield return LoadScene();

            ShowcasePlayerCombatController combat =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            ShowcaseRunnerAgent target = SceneComponents<ShowcaseRunnerAgent>()
                .First(item => !item.IsElite);
            foreach (ShowcaseRunnerAgent runner in SceneComponents<ShowcaseRunnerAgent>())
            {
                runner.enabled = false;
            }
            Rigidbody body = target.GetComponent<Rigidbody>();
            Vector3 farPosition = combat.transform.position + combat.transform.forward * 4f;
            body.position = farPosition;
            target.transform.position = farPosition;
            Physics.SyncTransforms();

            ShowcaseAnimationEventReceiver receiver =
                combat.GetComponent<BreakNotice.Presentation.ShowcaseCharacterAnimationDriver>()
                    .Animator.GetComponent<ShowcaseAnimationEventReceiver>();
            ShowcaseBoneCombatRig rig = combat.GetComponent<ShowcaseBoneCombatRig>();
            Assert.That(combat.TryRequestMelee(null), Is.True);
            yield return WaitUntilOrFail(
                () => receiver.AttackWindowOpenInvocationCount == 1,
                2f,
                "Melee attack window did not open.");
            yield return WaitUntilOrFail(
                () => !combat.IsActionLocked,
                2f,
                "Missed melee action did not complete safely.");

            Assert.That(target.Health.CurrentHealth, Is.EqualTo(100));
            Assert.That(rig.ContactCount, Is.Zero);
            Assert.That(rig.AcceptedHitCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator RunnerAttack_DamagesPlayerOnlyAtNaturalBoneOverlapWindow()
        {
            yield return LoadScene();

            ShowcasePlayerCombatController player =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            ShowcaseRunnerAgent runner = pressure.Runners.First(item => !item.IsElite);
            foreach (ShowcaseRunnerAgent candidate in pressure.Runners)
            {
                candidate.enabled = false;
            }

            Vector3 position = player.transform.position + player.transform.forward * 0.81f;
            runner.transform.position = position;
            runner.transform.rotation = Quaternion.LookRotation(
                player.transform.position - position,
                Vector3.up);
            Rigidbody body = runner.GetComponent<Rigidbody>();
            body.position = position;
            body.rotation = runner.transform.rotation;
            Physics.SyncTransforms();

            ShowcaseAnimationEventReceiver receiver = runner
                .GetComponent<BreakNotice.Presentation.ShowcaseCharacterAnimationDriver>()
                .Animator.GetComponent<ShowcaseAnimationEventReceiver>();
            ShowcaseBoneCombatRig rig = runner.GetComponent<ShowcaseBoneCombatRig>();
            Assert.That(pressure.TryAcquireAttackToken(runner), Is.True);
            runner.SendMessage("CommitNextAttack", SendMessageOptions.RequireReceiver);
            int startingHealth = player.Health.CurrentHealth;
            yield return WaitUntilOrFail(
                () => receiver.AttackWindowOpenInvocationCount == 1,
                3f,
                "Runner attack-window AnimationEvent did not fire.");
            float runnerDeadline = Time.realtimeSinceStartup + 1f;
            while (rig.AcceptedHitCount != 1 &&
                   Time.realtimeSinceStartup < runnerDeadline)
            {
                yield return null;
            }
            Assert.That(rig.AcceptedHitCount, Is.EqualTo(1),
                $"Runner strike bone did not overlap player hurtboxes. " +
                $"Minimum surface separation: {rig.MinimumWindowSurfaceSeparation:F4} m.");

            Assert.That(player.Health.CurrentHealth, Is.LessThan(startingHealth));
            Assert.That(rig.AcceptedHitCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DirectThrow_PhysicalReleaseWaitsForAnimationEvent()
        {
            yield return LoadScene();

            ShowcasePlayerCombatController combat =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            PrototypeInteractionStateOwner owner =
                combat.GetComponent<PrototypeInteractionStateOwner>();
            PrototypePhysicalObjectLifecycle lifecycle =
                combat.GetComponent<PrototypePhysicalObjectLifecycle>();
            PrototypeInteractionTarget target =
                SceneComponents<PrototypeInteractionTarget>()
                    .First(item => item.IsScoopEligible &&
                                   item.WeightClass == PrototypeWeightClass.Light);
            ShowcaseAnimationEventReceiver receiver =
                combat.GetComponent<BreakNotice.Presentation.ShowcaseCharacterAnimationDriver>()
                    .Animator.GetComponent<ShowcaseAnimationEventReceiver>();

            Assert.That(owner.TryCommit(PrototypeInteractionDecision.Accept(
                PrototypeInteractionOutcome.Scoop,
                target)), Is.True);
            Assert.That(owner.TryCompleteAcquisition(target), Is.True);
            Assert.That(lifecycle.BeginCarry(target), Is.True);
            uint before = lifecycle.ThrowOriginCount;

            Assert.That(combat.TryBeginDirectThrow(target, Vector3.forward, 0.5f), Is.True);
            Assert.That(lifecycle.ThrowOriginCount, Is.EqualTo(before));
            Assert.That(lifecycle.CarriedTarget, Is.EqualTo(target));
            Assert.That(combat.GetComponent<ThirdPersonLocomotion>()
                .IsMovementExternallyBlocked, Is.True);

            yield return new WaitForSeconds(0.2f);
            Assert.That(receiver.ThrowReleaseInvocationCount, Is.Zero);
            Assert.That(lifecycle.ThrowOriginCount, Is.EqualTo(before));
            yield return WaitUntilOrFail(
                () => receiver.ThrowReleaseInvocationCount == 1,
                3f,
                "Throw release AnimationEvent did not fire.");

            Assert.That(lifecycle.ThrowOriginCount, Is.EqualTo(before + 1));
            Assert.That(lifecycle.CarriedTarget, Is.Null);
            Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
            yield return WaitUntilOrFail(
                () => !combat.IsActionLocked,
                3f,
                "Throw completion AnimationEvent did not release action ownership.");
        }

        [UnityTest]
        public IEnumerator GroundPickup_PhysicalCarryWaitsForAnimationContactEvent()
        {
            yield return LoadScene();

            ShowcasePlayerCombatController combat =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            PrototypeInteractionStateOwner owner =
                combat.GetComponent<PrototypeInteractionStateOwner>();
            PrototypePhysicalObjectLifecycle lifecycle =
                combat.GetComponent<PrototypePhysicalObjectLifecycle>();
            PrototypeInteractionTarget target =
                SceneComponents<PrototypeInteractionTarget>()
                    .First(item => item.IsScoopEligible &&
                                   item.WeightClass == PrototypeWeightClass.Light);
            ShowcaseAnimationEventReceiver receiver =
                combat.GetComponent<BreakNotice.Presentation.ShowcaseCharacterAnimationDriver>()
                    .Animator.GetComponent<ShowcaseAnimationEventReceiver>();

            Assert.That(owner.TryCommit(PrototypeInteractionDecision.Accept(
                PrototypeInteractionOutcome.Scoop,
                target)), Is.True);
            Assert.That(combat.TryBeginAcquisition(
                target,
                PrototypeInteractionOutcome.Scoop), Is.True);
            Assert.That(lifecycle.CarriedTarget, Is.Null);
            Assert.That(owner.HandState,
                Is.EqualTo(PrototypeHandOwnershipState.AcquisitionCommitted));
            Assert.That(owner.AcquisitionCommittedTarget, Is.SameAs(target));

            yield return new WaitForSeconds(0.15f);
            Assert.That(receiver.CarryContactInvocationCount, Is.Zero);
            Assert.That(lifecycle.CarriedTarget, Is.Null);
            yield return WaitUntilOrFail(
                () => receiver.CarryContactInvocationCount == 1,
                2f,
                "Carry contact AnimationEvent did not fire.");

            Assert.That(lifecycle.CarriedTarget, Is.SameAs(target));
            Assert.That(owner.CarriedTarget, Is.SameAs(target));
            Assert.That(lifecycle.GripSeparation, Is.LessThanOrEqualTo(0.18f),
                "The carried Binder must remain on the CHR00 bone-tracked grip socket.");
            Assert.That(lifecycle.IsGripBlocked, Is.False);
        }

        [UnityTest]
        public IEnumerator AllySupply_PhysicalReleaseWaitsForAllyAnimationEvent()
        {
            yield return LoadScene();

            PrototypeAllyRelayController relay =
                SceneComponents<PrototypeAllyRelayController>().Single();
            PrototypeInteractionController interaction =
                SceneComponents<PrototypeInteractionController>().Single();
            BreakNotice.Presentation.ShowcaseCharacterAnimationDriver allyDriver =
                SceneComponents<BreakNotice.Presentation.ShowcaseCharacterAnimationDriver>()
                    .Single(driver =>
                        driver.Role == BreakNotice.Presentation.ShowcaseCharacterAnimationRole.Ally);
            ShowcaseAnimationEventReceiver receiver =
                allyDriver.Animator.GetComponent<ShowcaseAnimationEventReceiver>();
            ShowcaseBinderGripSocket gripSocket =
                allyDriver.GetComponentInParent<ShowcaseBinderGripSocket>();

            Assert.That(relay.RequiresAnimationRelease, Is.True);
            Assert.That(gripSocket, Is.Not.Null);
            Assert.That(interaction.ProcessAllyTask(), Is.True,
                "F Fetch must assign an available supply target before Q release.");
            yield return WaitUntilOrFail(
                () => relay.IsReadyForCall,
                12f,
                "Ally did not prepare a supply target.");
            yield return WaitUntilOrFail(
                () => relay.PreparedTarget != null &&
                      gripSocket.TryGetWorldPose(
                          out Vector3 currentGripPosition,
                          out _) &&
                      Vector3.Distance(
                          relay.PreparedTarget.transform.position,
                          currentGripPosition) <= 0.03f,
                1f,
                "Ready Binder did not converge to the current-frame CHR01 grip pose.");
            Assert.That(gripSocket.TryGetWorldPose(
                out Vector3 readyGripPosition,
                out _), Is.True);
            Assert.That(Vector3.Distance(
                    relay.PreparedTarget.transform.position,
                    readyGripPosition),
                Is.LessThanOrEqualTo(0.03f),
                "Ready Binder must be held by the CHR01 bone-tracked grip socket.");
            int releaseBefore = relay.ReleaseCount;
            Assert.That(interaction.ProcessCallAlly(), Is.True);
            Assert.That(relay.RelayPhase, Is.EqualTo(PrototypeRelayPhase.PreRelease));
            yield return new WaitForSeconds(0.1f);
            Assert.That(relay.ReleaseCount, Is.EqualTo(releaseBefore));
            Assert.That(relay.RelayPhase, Is.EqualTo(PrototypeRelayPhase.PreRelease));
            Assert.That(gripSocket.TryGetWorldPose(
                out Vector3 preReleaseGripPosition,
                out _), Is.True);
            Assert.That(Vector3.Distance(
                    relay.ActiveTarget.transform.position,
                    preReleaseGripPosition),
                Is.LessThanOrEqualTo(0.03f),
                "Pre-release Binder must follow the CHR01 hand until the release event.");

            yield return WaitUntilOrFail(
                () => receiver.AllySupplyReleaseInvocationCount == 1,
                3f,
                "Ally supply release AnimationEvent did not fire.");
            Assert.That(relay.ReleaseCount, Is.EqualTo(releaseBefore + 1));
            Assert.That(relay.RelayPhase, Is.EqualTo(PrototypeRelayPhase.Released));
        }

        [UnityTest]
        public IEnumerator OrdinaryRelayCatch_PreservesExactTransactionUntilNaturalCarryContact()
        {
            yield return LoadScene();

            ShowcasePlayerCombatController combat =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            PrototypeInteractionController interaction =
                combat.GetComponent<PrototypeInteractionController>();
            PrototypeInteractionStateOwner owner =
                combat.GetComponent<PrototypeInteractionStateOwner>();
            PrototypePhysicalObjectLifecycle lifecycle =
                combat.GetComponent<PrototypePhysicalObjectLifecycle>();
            ShowcasePlayerPropGripRig gripRig = combat
                .GetComponent<BreakNotice.Presentation.ShowcaseCharacterAnimationDriver>()
                .Animator.GetComponent<ShowcasePlayerPropGripRig>();
            ShowcaseTeamContextController team =
                SceneComponents<ShowcaseTeamContextController>().Single();
            PrototypeAllyRelayController relay =
                SceneComponents<PrototypeAllyRelayController>().Single();
            ShowcaseAnimationEventReceiver playerReceiver = combat
                .GetComponent<BreakNotice.Presentation.ShowcaseCharacterAnimationDriver>()
                .Animator.GetComponent<ShowcaseAnimationEventReceiver>();
            ShowcaseAnimationEventReceiver allyReceiver =
                SceneComponents<BreakNotice.Presentation.ShowcaseCharacterAnimationDriver>()
                    .Single(driver =>
                        driver.Role ==
                            BreakNotice.Presentation.ShowcaseCharacterAnimationRole.Ally)
                    .Animator.GetComponent<ShowcaseAnimationEventReceiver>();

            DisableRunners();
            PrototypeInteractionTarget target = ResolveRelayBinder(relay);
            yield return BeginReleasedBinderRelay(
                relay,
                interaction.transform,
                target,
                allyReceiver);
            PositionIncomingBinderForCatch(interaction.transform, target);
            Assert.That(interaction.RefreshTarget(false), Is.SameAs(target),
                "The actual v007 interaction query must resolve the positioned relay Binder.");
            interaction.SendMessage("LateUpdate", SendMessageOptions.DontRequireReceiver);
            Assert.That(interaction.RefreshTarget(false), Is.SameAs(target));
            Assert.That(relay.CanCatch(target), Is.True);

            int contactBefore = playerReceiver.CarryContactInvocationCount;
            int catchBefore = relay.CatchCount;
            uint readinessBefore = team.ReadinessRevision;
            PrototypeInteractionDecision decision =
                interaction.ProcessInteractPrimary(false);

            Assert.That(decision.IsAccepted, Is.True);
            Assert.That(decision.Outcome, Is.EqualTo(PrototypeInteractionOutcome.Catch));
            Assert.That(decision.Target, Is.SameAs(target));
            Assert.That(owner.HandState,
                Is.EqualTo(PrototypeHandOwnershipState.AcquisitionCommitted));
            Assert.That(owner.AcquisitionCommittedTarget, Is.SameAs(target));
            Assert.That(owner.RelayState,
                Is.EqualTo(PrototypeRelayReservationState.CatchCommitted));
            Assert.That(owner.RelayReservedTarget, Is.SameAs(target));
            Assert.That(relay.RelayPhase, Is.EqualTo(PrototypeRelayPhase.Released));
            Assert.That(relay.ActiveTarget, Is.SameAs(target));
            Assert.That(lifecycle.CarriedTarget, Is.Null);
            Assert.That(gripRig.Target, Is.SameAs(target));
            Assert.That(gripRig.Phase,
                Is.EqualTo(ShowcasePlayerPropGripPhase.AcquisitionPending));
            Assert.That(playerReceiver.CarryContactInvocationCount,
                Is.EqualTo(contactBefore));
            Assert.That(relay.CatchCount, Is.EqualTo(catchBefore));
            Assert.That(team.ReadinessRevision, Is.EqualTo(readinessBefore));

            yield return new WaitForFixedUpdate();
            Assert.That(owner.RelayState,
                Is.EqualTo(PrototypeRelayReservationState.CatchCommitted),
                "Relay FixedUpdate must not erase a catch owned by the delayed animation event.");
            Assert.That(owner.RelayReservedTarget, Is.SameAs(target));
            Assert.That(relay.ActiveTarget, Is.SameAs(target));

            yield return WaitUntilOrFail(
                () => playerReceiver.CarryContactInvocationCount == contactBefore + 1,
                3f,
                "Natural CHR00 CarryContact AnimationEvent did not settle the relay catch.");

            Assert.That(lifecycle.CarriedTarget, Is.SameAs(target));
            Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Carrying));
            Assert.That(owner.CarriedTarget, Is.SameAs(target));
            Assert.That(owner.RelayState, Is.EqualTo(PrototypeRelayReservationState.None));
            Assert.That(owner.RelayReservedTarget, Is.Null);
            Assert.That(relay.RelayPhase, Is.EqualTo(PrototypeRelayPhase.None));
            Assert.That(relay.ActiveTarget, Is.Null);
            Assert.That(relay.CatchCount, Is.EqualTo(catchBefore + 1));
            Assert.That(team.HasPass, Is.True);
            Assert.That(team.ReadinessRevision, Is.EqualTo(readinessBefore + 1));

            combat.CommitAnimationCarryContact();
            Assert.That(relay.CatchCount, Is.EqualTo(catchBefore + 1),
                "A duplicate contact callback must not complete the same catch twice.");
            Assert.That(team.ReadinessRevision, Is.EqualTo(readinessBefore + 1),
                "Pass success must be awarded exactly once for the same relay catch.");
        }

        [UnityTest]
        public IEnumerator OrdinaryRelayCatch_CancelBeforeContactRollsBackInSameFrame()
        {
            yield return LoadScene();

            ShowcasePlayerCombatController combat =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            PrototypeInteractionController interaction =
                combat.GetComponent<PrototypeInteractionController>();
            PrototypeInteractionStateOwner owner =
                combat.GetComponent<PrototypeInteractionStateOwner>();
            PrototypePhysicalObjectLifecycle lifecycle =
                combat.GetComponent<PrototypePhysicalObjectLifecycle>();
            ShowcasePlayerPropGripRig gripRig = combat
                .GetComponent<BreakNotice.Presentation.ShowcaseCharacterAnimationDriver>()
                .Animator.GetComponent<ShowcasePlayerPropGripRig>();
            ShowcaseTeamContextController team =
                SceneComponents<ShowcaseTeamContextController>().Single();
            PrototypeAllyRelayController relay =
                SceneComponents<PrototypeAllyRelayController>().Single();
            ShowcaseRunnerAgent damageSource =
                SceneComponents<ShowcaseRunnerAgent>().First(item => !item.IsElite);
            ShowcaseAnimationEventReceiver playerReceiver = combat
                .GetComponent<BreakNotice.Presentation.ShowcaseCharacterAnimationDriver>()
                .Animator.GetComponent<ShowcaseAnimationEventReceiver>();
            ShowcaseAnimationEventReceiver allyReceiver =
                SceneComponents<BreakNotice.Presentation.ShowcaseCharacterAnimationDriver>()
                    .Single(driver =>
                        driver.Role ==
                            BreakNotice.Presentation.ShowcaseCharacterAnimationRole.Ally)
                    .Animator.GetComponent<ShowcaseAnimationEventReceiver>();

            DisableRunners();
            PrototypeInteractionTarget target = ResolveRelayBinder(relay);
            yield return BeginReleasedBinderRelay(
                relay,
                interaction.transform,
                target,
                allyReceiver);
            PositionIncomingBinderForCatch(interaction.transform, target);
            Assert.That(interaction.RefreshTarget(false), Is.SameAs(target),
                "The actual v007 interaction query must resolve the positioned relay Binder.");
            interaction.SendMessage("LateUpdate", SendMessageOptions.DontRequireReceiver);
            Assert.That(interaction.RefreshTarget(false), Is.SameAs(target));
            Assert.That(relay.CanCatch(target), Is.True);

            int contactBefore = playerReceiver.CarryContactInvocationCount;
            int catchBefore = relay.CatchCount;
            uint readinessBefore = team.ReadinessRevision;
            PrototypeInteractionDecision decision =
                interaction.ProcessInteractPrimary(false);
            Assert.That(decision.IsAccepted, Is.True);
            Assert.That(owner.RelayState,
                Is.EqualTo(PrototypeRelayReservationState.CatchCommitted));
            Assert.That(owner.RelayReservedTarget, Is.SameAs(target));
            Assert.That(relay.ActiveTarget, Is.SameAs(target));

            Assert.That(combat.ReceiveDamage(new ShowcaseDamageRequest(
                ShowcaseCombatTeam.Enemy,
                damageSource.RunnerId,
                99001,
                5,
                ShowcaseHitReaction.Body,
                Vector3.back)), Is.True);

            Assert.That(playerReceiver.CarryContactInvocationCount,
                Is.EqualTo(contactBefore));
            Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
            Assert.That(owner.AcquisitionCommittedTarget, Is.Null);
            Assert.That(owner.RelayState, Is.EqualTo(PrototypeRelayReservationState.None));
            Assert.That(owner.RelayReservedTarget, Is.Null);
            Assert.That(lifecycle.CarriedTarget, Is.Null);
            Assert.That(gripRig.Target, Is.Null);
            Assert.That(gripRig.Phase, Is.EqualTo(ShowcasePlayerPropGripPhase.None));
            Assert.That(relay.RelayPhase, Is.EqualTo(PrototypeRelayPhase.None));
            Assert.That(relay.ActiveTarget, Is.Null);
            Assert.That(relay.CatchCount, Is.EqualTo(catchBefore));
            Assert.That(team.HasPass, Is.False);
            Assert.That(team.ReadinessRevision, Is.EqualTo(readinessBefore));

            combat.CommitAnimationCarryContact();
            Assert.That(lifecycle.CarriedTarget, Is.Null,
                "A late stale contact callback must not recreate cancelled ownership.");
            Assert.That(relay.CatchCount, Is.EqualTo(catchBefore));
            Assert.That(team.ReadinessRevision, Is.EqualTo(readinessBefore));
        }

        [UnityTest]
        public IEnumerator AnimatorDisableDuringAcquisition_CleansPlayerTransactionWithinOneSecond()
        {
            yield return LoadScene();

            ShowcasePlayerCombatController combat =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            PrototypeInteractionStateOwner owner =
                combat.GetComponent<PrototypeInteractionStateOwner>();
            PrototypePhysicalObjectLifecycle lifecycle =
                combat.GetComponent<PrototypePhysicalObjectLifecycle>();
            ThirdPersonLocomotion locomotion = combat.GetComponent<ThirdPersonLocomotion>();
            BreakNotice.Presentation.ShowcaseCharacterAnimationDriver driver = combat
                .GetComponent<BreakNotice.Presentation.ShowcaseCharacterAnimationDriver>();
            Animator animator = driver.Animator;
            ShowcasePlayerPropGripRig gripRig =
                animator.GetComponent<ShowcasePlayerPropGripRig>();
            PrototypeInteractionTarget target =
                SceneComponents<PrototypeInteractionTarget>()
                    .First(item => item.IsScoopEligible &&
                                   item.WeightClass == PrototypeWeightClass.Light);

            DisableRunners();
            Assert.That(owner.TryCommit(PrototypeInteractionDecision.Accept(
                PrototypeInteractionOutcome.Scoop,
                target)), Is.True);
            Assert.That(combat.TryBeginAcquisition(
                target,
                PrototypeInteractionOutcome.Scoop), Is.True);
            Assert.That(combat.IsActionLocked, Is.True);
            Assert.That(locomotion.HasMobilityLock(
                PlayerMobilityLockReason.PlayerAction), Is.True);
            Assert.That(combat.AnimationCue, Is.Not.EqualTo(ShowcasePlayerAnimationCue.None));
            Assert.That(gripRig.Phase,
                Is.EqualTo(ShowcasePlayerPropGripPhase.AcquisitionPending));
            Assert.That(gripRig.Target, Is.SameAs(target));

            LogAssert.Expect(
                LogType.Error,
                "SHOWCASE PLAYER PROP RUNTIME UNAVAILABLE: Acquire. " +
                "The pending action and mobility lock are being cancelled.");
            animator.enabled = false;

            yield return WaitUntilOrFail(
                () => !combat.IsActionLocked &&
                      !locomotion.HasMobilityLock(PlayerMobilityLockReason.PlayerAction) &&
                      combat.AnimationCue == ShowcasePlayerAnimationCue.None &&
                      owner.HandState == PrototypeHandOwnershipState.Free &&
                      owner.AcquisitionCommittedTarget == null &&
                      owner.CarriedTarget == null &&
                      owner.ThrowCommittedTarget == null &&
                      lifecycle.CarriedTarget == null &&
                      gripRig.Phase == ShowcasePlayerPropGripPhase.None &&
                      gripRig.Target == null &&
                      !gripRig.IsAuthoritative,
                1f,
                "Disabling the CHR00 Animator must clear combat, PlayerAction, " +
                "ownership, lifecycle, cue, and grip state within one second.");

            Assert.That(combat.IsActionLocked, Is.False);
            Assert.That(locomotion.HasMobilityLock(
                PlayerMobilityLockReason.PlayerAction), Is.False);
            Assert.That(combat.AnimationCue, Is.EqualTo(ShowcasePlayerAnimationCue.None));
            Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
            Assert.That(owner.AcquisitionCommittedTarget, Is.Null);
            Assert.That(owner.CarriedTarget, Is.Null);
            Assert.That(owner.ThrowCommittedTarget, Is.Null);
            Assert.That(lifecycle.CarriedTarget, Is.Null);
            Assert.That(gripRig.Phase, Is.EqualTo(ShowcasePlayerPropGripPhase.None));
            Assert.That(gripRig.Target, Is.Null);
            Assert.That(gripRig.IsAuthoritative, Is.False);
        }

        [UnityTest]
        public IEnumerator QueuedRelayThrow_CancelBeforeReleasePreservesTrackedPoseAndClearsInboundVelocity()
        {
            yield return LoadScene();

            ShowcasePlayerCombatController combat =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            PrototypeInteractionStateOwner owner =
                combat.GetComponent<PrototypeInteractionStateOwner>();
            PrototypePhysicalObjectLifecycle lifecycle =
                combat.GetComponent<PrototypePhysicalObjectLifecycle>();
            PrototypeInteractionController interaction =
                combat.GetComponent<PrototypeInteractionController>();
            PrototypeAllyRelayController relay =
                SceneComponents<PrototypeAllyRelayController>().Single();
            ShowcasePlayerPropGripRig gripRig = combat
                .GetComponent<BreakNotice.Presentation.ShowcaseCharacterAnimationDriver>()
                .Animator.GetComponent<ShowcasePlayerPropGripRig>();
            ShowcaseAnimationEventReceiver allyReceiver =
                SceneComponents<BreakNotice.Presentation.ShowcaseCharacterAnimationDriver>()
                    .Single(driver =>
                        driver.Role ==
                            BreakNotice.Presentation.ShowcaseCharacterAnimationRole.Ally)
                    .Animator.GetComponent<ShowcaseAnimationEventReceiver>();

            DisableRunners();
            PrototypeInteractionTarget target = ResolveRelayBinder(relay);
            yield return BeginReleasedBinderRelay(
                relay,
                interaction.transform,
                target,
                allyReceiver);

            Rigidbody body = target.GetComponent<Rigidbody>();
            Assert.That(body, Is.Not.Null);
            bool originalIsKinematic = body.isKinematic;
            bool originalUseGravity = body.useGravity;
            Vector3 inboundLinearVelocity = body.linearVelocity;
            Vector3 inboundAngularVelocity = body.angularVelocity;
            Vector3 inboundPlanarVelocity = Vector3.ProjectOnPlane(
                inboundLinearVelocity,
                Vector3.up);
            Assert.That(inboundPlanarVelocity.magnitude, Is.GreaterThan(0.1f),
                "The actual released relay Binder must provide an inbound velocity " +
                "that the cancellation path can prove it did not restore.");

            Assert.That(relay.TryArmQueuedThrow(), Is.True);
            Assert.That(owner.TryCommit(PrototypeInteractionDecision.Accept(
                PrototypeInteractionOutcome.Catch,
                target)), Is.True);
            Assert.That(owner.HandState,
                Is.EqualTo(PrototypeHandOwnershipState.ThrowCommitted));
            Assert.That(owner.ThrowCommittedTarget, Is.SameAs(target));
            Assert.That(owner.RelayState,
                Is.EqualTo(PrototypeRelayReservationState.CatchCommitted));
            Assert.That(owner.RelayReservedTarget, Is.SameAs(target));

            uint throwOriginBefore = lifecycle.ThrowOriginCount;
            Assert.That(combat.TryBeginQueuedRelayThrow(
                target,
                combat.transform.forward,
                0.5f), Is.True);
            Assert.That(body.isKinematic, Is.True);
            Assert.That(body.useGravity, Is.False);
            Assert.That(body.linearVelocity, Is.EqualTo(Vector3.zero));
            Assert.That(body.angularVelocity, Is.EqualTo(Vector3.zero));

            yield return WaitUntilOrFail(
                () => gripRig.Phase == ShowcasePlayerPropGripPhase.Throw &&
                      gripRig.Target == target &&
                      gripRig.LastAppliedPoseFrameId >= Time.frameCount - 1 &&
                      gripRig.CurrentGripPositionError <= 0.03f &&
                      gripRig.CurrentGripRotationError <= 5f,
                1f,
                "The queued Binder did not reach the finalized CHR00 hand-tracked " +
                "Throw pose before cancellation.");

            Pose trackedPose = new(body.position, body.rotation);
            Assert.That(lifecycle.ThrowOriginCount, Is.EqualTo(throwOriginBefore));
            Assert.That(lifecycle.LastPhysicalReleaseSnapshot.IsValid, Is.False);

            combat.enabled = false;

            Assert.That(Vector3.Distance(body.position, trackedPose.position),
                Is.LessThanOrEqualTo(0.001f),
                "Pre-release cancellation must retain the finalized hand-tracked position.");
            Assert.That(Quaternion.Angle(body.rotation, trackedPose.rotation),
                Is.LessThanOrEqualTo(0.1f),
                "Pre-release cancellation must retain the finalized hand-tracked rotation.");
            Assert.That(body.isKinematic, Is.EqualTo(originalIsKinematic));
            Assert.That(body.useGravity, Is.EqualTo(originalUseGravity));
            Assert.That(body.linearVelocity, Is.EqualTo(Vector3.zero));
            Assert.That(body.angularVelocity, Is.EqualTo(Vector3.zero));
            Assert.That(lifecycle.ThrowOriginCount, Is.EqualTo(throwOriginBefore));
            Assert.That(lifecycle.LastPhysicalReleaseSnapshot.IsValid, Is.False);
            Assert.That(owner.HandState, Is.EqualTo(PrototypeHandOwnershipState.Free));
            Assert.That(owner.ThrowCommittedTarget, Is.Null);
            Assert.That(owner.RelayState, Is.EqualTo(PrototypeRelayReservationState.None));
            Assert.That(owner.RelayReservedTarget, Is.Null);
            Assert.That(relay.RelayPhase, Is.EqualTo(PrototypeRelayPhase.None));
            Assert.That(relay.ActiveTarget, Is.Null);
            Assert.That(gripRig.Phase, Is.EqualTo(ShowcasePlayerPropGripPhase.None));
            Assert.That(gripRig.Target, Is.Null);

            Vector3 cancelledPosition = body.position;
            yield return new WaitForFixedUpdate();
            Assert.That(Vector3.ProjectOnPlane(
                    body.linearVelocity,
                    Vector3.up).magnitude,
                Is.LessThanOrEqualTo(0.1f),
                $"Old inbound velocity was relaunched after cancellation. " +
                $"Before={inboundLinearVelocity}; after={body.linearVelocity}.");
            Assert.That(body.angularVelocity.magnitude, Is.LessThanOrEqualTo(0.1f),
                $"Old inbound angular velocity was relaunched after cancellation. " +
                $"Before={inboundAngularVelocity}; after={body.angularVelocity}.");
            Assert.That(Vector3.ProjectOnPlane(
                    body.position - cancelledPosition,
                    Vector3.up).magnitude,
                Is.LessThanOrEqualTo(0.01f),
                "The cancelled Binder must not resume its old inbound planar trajectory.");
        }

        [UnityTest]
        public IEnumerator PlayerKnockdown_BlocksMovementThroughGetupCompletion()
        {
            yield return LoadScene();

            ShowcasePlayerCombatController combat =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            ThirdPersonLocomotion locomotion = combat.GetComponent<ThirdPersonLocomotion>();
            InputSystemLogicalInputAdapter input =
                combat.GetComponent<InputSystemLogicalInputAdapter>();
            ShowcaseAnimationEventReceiver receiver =
                combat.GetComponent<BreakNotice.Presentation.ShowcaseCharacterAnimationDriver>()
                    .Animator.GetComponent<ShowcaseAnimationEventReceiver>();
            ShowcaseRunnerAgent source =
                SceneComponents<ShowcaseRunnerAgent>().First(item => !item.IsElite);

            Assert.That(combat.ReceiveDamage(new ShowcaseDamageRequest(
                ShowcaseCombatTeam.Enemy,
                source.RunnerId,
                9001,
                30,
                ShowcaseHitReaction.Knockdown,
                Vector3.back)), Is.True);
            Assert.That(locomotion.HasMobilityLock(PlayerMobilityLockReason.Knockdown), Is.True);
            Vector3 start = locomotion.transform.position;
            SetInputSnapshot(input, new LogicalInputSnapshot(
                Vector2.up,
                Vector2.zero,
                false,
                false,
                false,
                false,
                false,
                false));
            for (int frame = 0; frame < 12; frame++)
            {
                locomotion.SendMessage("Update", SendMessageOptions.DontRequireReceiver);
                yield return null;
            }
            Vector3 displacement = Vector3.ProjectOnPlane(
                locomotion.transform.position - start,
                Vector3.up);
            Assert.That(displacement.magnitude, Is.LessThan(0.01f));

            yield return WaitUntilOrFail(
                () => receiver.ActionCompleteInvocationCount >= 1,
                3f,
                "Knockdown completion AnimationEvent did not fire.");
            Assert.That(locomotion.HasMobilityLock(PlayerMobilityLockReason.Knockdown), Is.True,
                "Getup must retain the knockdown lock.");
            yield return WaitUntilOrFail(
                () => !combat.IsActionLocked,
                6f,
                "Getup completion AnimationEvent did not release the action.");
            Assert.That(locomotion.HasMobilityLock(PlayerMobilityLockReason.Knockdown), Is.False);
            SetInputSnapshot(input, default);
        }

        [UnityTest]
        public IEnumerator Reset_RestoresAllThreeHealthOwnersAndCombatState()
        {
            yield return LoadScene();

            ShowcasePlayerCombatController player =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            ShowcaseRunnerAgent normal =
                SceneComponents<ShowcaseRunnerAgent>().Single(item => !item.IsElite);
            AuthoredEncounterResetController reset =
                SceneComponents<AuthoredEncounterResetController>().Single();

            Assert.That(player.Health.TryApplyDamage(
                new ShowcaseDamageRequest(
                    ShowcaseCombatTeam.Enemy,
                    normal.RunnerId,
                    1,
                    30,
                    ShowcaseHitReaction.Knockdown,
                    Vector3.back),
                0f), Is.True);
            Assert.That(normal.Health.TryApplyDamage(
                new ShowcaseDamageRequest(
                    ShowcaseCombatTeam.Player,
                    "CHR00",
                    1,
                    35,
                    ShowcaseHitReaction.BigHead,
                    Vector3.forward),
                0f), Is.True);

            Assert.That(reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
            yield return null;
            yield return null;

            Assert.That(player.Health.CurrentHealth, Is.EqualTo(100));
            Assert.That(normal.Health.CurrentHealth, Is.EqualTo(100));
            Assert.That(player.IsDead, Is.False);
            Assert.That(player.IsActionLocked, Is.False);
        }

        private IEnumerator LoadScene()
        {
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
            Assert.That(load, Is.Not.Null);
            yield return load;
            loadedScene = SceneManager.GetSceneByPath(ScenePath);
            Assert.That(loadedScene.IsValid() && loadedScene.isLoaded, Is.True);
            yield return null;
        }

        private static void PlaceRunnerInFront(
            Transform player,
            ShowcaseRunnerAgent runner)
        {
            Vector3 position = player.position + player.forward * 0.89f;
            Rigidbody body = runner.GetComponent<Rigidbody>();
            body.position = position;
            runner.transform.position = position;
            Physics.SyncTransforms();
        }

        private static void SubmitMeleeInput(
            PrototypeInteractionController interaction,
            InputSystemLogicalInputAdapter input)
        {
            SetInputSnapshot(input, new LogicalInputSnapshot(
                Vector2.zero,
                Vector2.zero,
                false,
                false,
                true,
                false,
                false,
                false,
                throwHeld: true));
            interaction.SendMessage("Update", SendMessageOptions.DontRequireReceiver);
            SetInputSnapshot(input, default);
        }

        private IEnumerator BeginReleasedBinderRelay(
            PrototypeAllyRelayController relay,
            Transform player,
            PrototypeInteractionTarget target,
            ShowcaseAnimationEventReceiver allyReceiver)
        {
            float readyDeadline = Time.realtimeSinceStartup + 1f;
            while ((relay.AllyState != PrototypeAllySupportState.Ready ||
                    relay.RelayPhase != PrototypeRelayPhase.None ||
                    relay.ActiveTarget != null) &&
                   Time.realtimeSinceStartup < readyDeadline)
            {
                yield return null;
            }
            Assert.That(relay.AllyState, Is.EqualTo(PrototypeAllySupportState.Ready),
                "Actual v007 ally relay did not become available before relay setup.");
            Assert.That(relay.RelayPhase, Is.EqualTo(PrototypeRelayPhase.None));
            Assert.That(relay.ActiveTarget, Is.Null);

            Collider targetCollider = target.GetComponent<Collider>();
            Rigidbody body = target.GetComponent<Rigidbody>();
            Assert.That(targetCollider, Is.Not.Null);
            Assert.That(body, Is.Not.Null);

            Vector3 destination = player.position +
                player.forward * 0.75f + Vector3.up * 1.05f;
            Vector3 positionedRoot = body.position +
                (destination - targetCollider.bounds.center);
            body.position = positionedRoot;
            target.transform.position = positionedRoot;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            Physics.SyncTransforms();

            Assert.That(relay.HasValidPhysicalLane(target), Is.True);
            int releaseBefore = allyReceiver.AllySupplyReleaseInvocationCount;
            Assert.That(
                relay.TryRequestSupply(target, out PrototypeAllyRelayRejection rejection),
                Is.True,
                $"Actual v007 Binder relay setup failed: {rejection}.");
            yield return WaitUntilOrFail(
                () => allyReceiver.AllySupplyReleaseInvocationCount == releaseBefore + 1,
                3f,
                "Natural CHR01 supply release AnimationEvent did not fire.");
            Assert.That(relay.RelayPhase, Is.EqualTo(PrototypeRelayPhase.Released));
            Assert.That(relay.ActiveTarget, Is.SameAs(target));
        }

        private void PositionIncomingBinderForCatch(
            Transform player,
            PrototypeInteractionTarget target)
        {
            Rigidbody body = target.GetComponent<Rigidbody>();
            Collider targetCollider = target.GetComponent<Collider>();
            Camera outputCamera = SceneComponents<Camera>()
                .Single(camera => camera.isActiveAndEnabled);
            Vector3 forward = Vector3.ProjectOnPlane(
                outputCamera.transform.forward,
                Vector3.up).normalized;
            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = player.forward;
            }

            player.rotation = Quaternion.LookRotation(forward, Vector3.up);
            Vector3 desiredCenter = player.position +
                forward * 0.9f + Vector3.up;
            Vector3 positionedRoot = body.position +
                (desiredCenter - targetCollider.bounds.center);
            body.position = positionedRoot;
            target.transform.position = positionedRoot;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            Physics.SyncTransforms();
        }

        private static PrototypeInteractionTarget ResolveRelayBinder(
            PrototypeAllyRelayController relay)
        {
            PrototypeInteractionTarget target = relay.SupplyTargets.FirstOrDefault(item =>
                item != null &&
                item.Role == PrototypeInteractionRole.AllyBinder &&
                item.WeightClass == PrototypeWeightClass.Light &&
                item.CatchEligible);
            Assert.That(target, Is.Not.Null);
            return target;
        }

        private void DisableRunners()
        {
            foreach (ShowcaseRunnerAgent runner in SceneComponents<ShowcaseRunnerAgent>())
            {
                runner.enabled = false;
            }
        }

        private static void SetInputSnapshot(
            InputSystemLogicalInputAdapter input,
            LogicalInputSnapshot value)
        {
            PropertyInfo snapshot = typeof(InputSystemLogicalInputAdapter).GetProperty(
                nameof(InputSystemLogicalInputAdapter.Snapshot),
                BindingFlags.Instance | BindingFlags.Public);
            MethodInfo setter = snapshot?.GetSetMethod(true);
            Assert.That(setter, Is.Not.Null);
            setter.Invoke(input, new object[] { value });
        }

        private static IEnumerator WaitUntilOrFail(
            Func<bool> predicate,
            float timeoutSeconds,
            string message)
        {
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!predicate() && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }
            Assert.That(predicate(), Is.True, message);
        }

        private T[] SceneComponents<T>() where T : Component
        {
            return loadedScene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }
    }
}
