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

        private readonly struct RunnerAttackMatrixCase
        {
            public RunnerAttackMatrixCase(
                string name,
                bool elite,
                bool kickSlot,
                int randomSlot,
                ShowcaseRunnerAnimationCue expectedCue,
                int expectedDamage,
                params HumanBodyBones[] expectedStrikeBones)
            {
                Name = name;
                Elite = elite;
                KickSlot = kickSlot;
                RandomSlot = randomSlot;
                ExpectedCue = expectedCue;
                ExpectedDamage = expectedDamage;
                ExpectedStrikeBones = expectedStrikeBones;
            }

            public string Name { get; }
            public bool Elite { get; }
            public bool KickSlot { get; }
            public int RandomSlot { get; }
            public ShowcaseRunnerAnimationCue ExpectedCue { get; }
            public int ExpectedDamage { get; }
            public HumanBodyBones[] ExpectedStrikeBones { get; }
        }

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
        public IEnumerator PlayerJumpTakeoff_StartsAuthoredJumpAtPhysicalImpulseFrame()
        {
            yield return LoadScene();
            DisableRunners();

            ShowcasePlayerCombatController combat =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            ThirdPersonLocomotion locomotion = combat.GetComponent<ThirdPersonLocomotion>();
            InputSystemLogicalInputAdapter input =
                combat.GetComponent<InputSystemLogicalInputAdapter>();
            ShowcaseCharacterAnimationDriver driver =
                combat.GetComponent<ShowcaseCharacterAnimationDriver>();
            Assert.That(locomotion.IsGrounded, Is.True);
            input.enabled = false;

            uint takeoffBefore = locomotion.JumpTakeoffRevision;
            SetInputSnapshot(input, new LogicalInputSnapshot(
                Vector2.zero,
                Vector2.zero,
                false,
                false,
                false,
                false,
                false,
                false,
                jump: true));
            yield return null;

            Assert.That(locomotion.JumpTakeoffRevision,
                Is.EqualTo(takeoffBefore + 1u));
            Assert.That(locomotion.VerticalVelocity, Is.GreaterThan(0f));

            Assert.That(driver.CurrentBaseStateName, Is.EqualTo("JumpAction"),
                "The physical takeoff revision must start JumpAction in the same frame.");

            driver.Animator.Update(0f);
            int jumpHash = Animator.StringToHash("Base Layer.JumpAction");
            AnimatorStateInfo jumpState = driver.Animator.IsInTransition(0)
                ? driver.Animator.GetNextAnimatorStateInfo(0)
                : driver.Animator.GetCurrentAnimatorStateInfo(0);
            Assert.That(jumpState.fullPathHash, Is.EqualTo(jumpHash));
            Assert.That(jumpState.normalizedTime,
                Is.GreaterThanOrEqualTo(ShowcaseChr00AnimationTiming.JumpImpulse - 0.02f),
                "JumpAction must enter at the authored takeoff frame, not its grounded lead-in.");
            SetInputSnapshot(input, default);
        }

        [UnityTest]
        public IEnumerator PlayerHitReaction_InterruptsAttachedMediumBeforeAnimatorEntry()
        {
            yield return LoadScene();
            DisableRunners();

            ShowcasePlayerCombatController combat =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            ShowcaseCargoStrapController cargo =
                combat.GetComponent<ShowcaseCargoStrapController>();
            PrototypeInteractionStateOwner owner =
                combat.GetComponent<PrototypeInteractionStateOwner>();
            ShowcaseCharacterAnimationDriver driver =
                combat.GetComponent<ShowcaseCharacterAnimationDriver>();
            ShowcaseRunnerAgent source =
                SceneComponents<ShowcaseRunnerAgent>().First(item => !item.IsElite);
            PrototypeInteractionTarget medium =
                SceneComponents<PrototypeInteractionTarget>().First(target =>
                    target.Role == PrototypeInteractionRole.DirectChair);

            combat.transform.position = medium.transform.position +
                new Vector3(0f, 0.9f, -2f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            Assert.That(cargo.TryBeginDirectAttach(medium), Is.True);
            CompleteStrapAttachSequence(cargo);
            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Tensioned));
            Assert.That(cargo.IsAttached, Is.True);

            Assert.That(combat.ReceiveDamage(new ShowcaseDamageRequest(
                ShowcaseCombatTeam.Enemy,
                source.RunnerId,
                97001,
                5,
                ShowcaseHitReaction.Body,
                Vector3.back)), Is.True);
            driver.ApplyLocalHitStop(0.05f);

            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Releasing),
                "An attached Tensioned strap must yield to the accepted hit reaction.");
            Assert.That(owner.StrapState,
                Is.EqualTo(PrototypeStrapOwnershipState.ReleaseRequested));
            Assert.That(combat.AnimationCue, Is.EqualTo(ShowcasePlayerAnimationCue.HitBody));
            yield return WaitUntilOrFail(
                () => driver.CurrentBaseStateName == "HitBodyAction",
                0.75f,
                "HitBodyAction did not win the cargo/player cue race after hit-stop.");
        }

        [UnityTest]
        public IEnumerator PlayerMelee_MovementKeepsFullBodyStrikeWithDirectionalLegs()
        {
            yield return LoadScene();
            DisableRunners();

            ShowcasePlayerCombatController combat =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            ThirdPersonLocomotion locomotion = combat.GetComponent<ThirdPersonLocomotion>();
            InputSystemLogicalInputAdapter input =
                combat.GetComponent<InputSystemLogicalInputAdapter>();
            ShowcaseCharacterAnimationDriver driver =
                combat.GetComponent<ShowcaseCharacterAnimationDriver>();
            input.enabled = false;

            Assert.That(combat.TryRequestMelee(null), Is.True);
            SetInputSnapshot(input, new LogicalInputSnapshot(
                Vector2.up,
                Vector2.zero,
                false,
                false,
                false,
                false,
                false,
                false));
            Vector3 start = combat.transform.position;
            yield return WaitUntilOrFail(
                () => driver.MeleeMovingLegsLayerWeight >= 0.99f &&
                      driver.CurrentBaseStateName == "MeleeJabAction",
                0.75f,
                "Melee did not preserve the authored full-body Base strike with moving legs.");
            yield return new WaitForSeconds(0.1f);

            Assert.That(Vector3.ProjectOnPlane(
                    combat.transform.position - start,
                    Vector3.up).magnitude,
                Is.GreaterThan(0.05f));
            Assert.That(locomotion.HasMobilityLock(
                PlayerMobilityLockReason.PlayerAction), Is.False);
            Assert.That(locomotion.HasJumpLock(
                PlayerMobilityLockReason.MeleeAction), Is.True);
            SetInputSnapshot(input, default);
        }

        [UnityTest]
        public IEnumerator PlayerMelee_ReticleRayOwnsTargetOverPreviousActorYaw()
        {
            yield return LoadScene();

            ShowcasePlayerCombatController combat =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            ThirdPersonCameraRig cameraRig =
                SceneComponents<ThirdPersonCameraRig>().Single();
            ShowcaseRunnerAgent target = pressure.Runners.First(item => !item.IsElite);
            ShowcaseRunnerAgent other = pressure.Runners.First(item => item.IsElite);
            foreach (ShowcaseRunnerAgent runner in pressure.Runners)
            {
                runner.enabled = false;
            }

            Vector3 playerForward = Vector3.ProjectOnPlane(
                combat.transform.forward,
                Vector3.up).normalized;
            Vector3 reticleDirection = Quaternion.AngleAxis(
                80f,
                Vector3.up) * playerForward;
            SetRunnerPose(
                target,
                combat.transform.position + reticleDirection * 0.89f,
                Quaternion.LookRotation(-reticleDirection, Vector3.up));
            SetRunnerPose(
                other,
                combat.transform.position - playerForward * 4f,
                Quaternion.LookRotation(playerForward, Vector3.up));
            Physics.SyncTransforms();

            Assert.That(
                pressure.ResolvePlayerMeleeTargetForCombo(),
                Is.Null,
                "The legacy actor-forward selector unexpectedly owned the side target.");
            Collider targetCollider = target.GetComponent<Collider>();
            Assert.That(targetCollider, Is.Not.Null);
            Vector3 reticleOrigin = cameraRig.OutputCamera.transform.position;
            Vector3 belowTarget = targetCollider.bounds.center -
                Vector3.up * (targetCollider.bounds.extents.y + 1.5f);
            Assert.That(
                pressure.ResolvePlayerMeleeTargetForReticle(new Ray(
                    reticleOrigin,
                    belowTarget - reticleOrigin)),
                Is.Null,
                "A ray visibly below the victim still acquired the melee target.");
            Ray reticleRay = new(
                reticleOrigin,
                targetCollider.bounds.center - reticleOrigin);

            Assert.That(
                pressure.ResolvePlayerMeleeTargetForReticle(reticleRay),
                Is.SameAs(target),
                "The centered optical ray did not own the melee target.");
            Assert.That(pressure.TryPlayerShove(reticleRay), Is.True);
            Assert.That(combat.ActiveMeleeTarget, Is.SameAs(target));
            Assert.That(
                Vector3.Dot(
                    Vector3.ProjectOnPlane(
                        combat.transform.forward,
                        Vector3.up).normalized,
                    reticleDirection.normalized),
                Is.GreaterThan(0.99f),
                "CHR00 did not snap the authored punch lane to the centered reticle target.");
        }

        [UnityTest]
        public IEnumerator PlayerCombo_RepeatsJabCrossAndUsesHookOnlyAsLethalFinisher()
        {
            yield return LoadScene();

            ShowcasePlayerCombatController combat =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            PrototypeInteractionController interaction =
                SceneComponents<PrototypeInteractionController>().Single();
            ShowcaseRunnerAgent target = pressure.Runners.Single(item => !item.IsElite);
            ShowcaseAnimationEventReceiver receiver =
                combat.GetComponent<BreakNotice.Presentation.ShowcaseCharacterAnimationDriver>()
                    .Animator.GetComponent<ShowcaseAnimationEventReceiver>();
            ShowcaseBoneCombatRig combatRig =
                combat.GetComponent<ShowcaseBoneCombatRig>();
            ShowcaseRunnerPressurePresenter pressurePresenter =
                SceneComponents<ShowcaseRunnerPressurePresenter>().Single();
            ShowcaseHitFeedbackController hitFeedback =
                SceneComponents<ShowcaseHitFeedbackController>().Single();
            ShowcaseHitRotationImpulseExtension rotationImpulse =
                SceneComponents<ShowcaseHitRotationImpulseExtension>().Single();
            ThirdPersonCameraRig cameraRig =
                SceneComponents<ThirdPersonCameraRig>().Single();

            Assert.That(hitFeedback.PlayCount, Is.Zero);
            Assert.That(hitFeedback.LightProfile.PositionAmplitude,
                Is.LessThan(hitFeedback.MediumProfile.PositionAmplitude));
            Assert.That(hitFeedback.MediumProfile.PositionAmplitude,
                Is.LessThan(hitFeedback.HeavyProfile.PositionAmplitude));
            Assert.That(hitFeedback.LightProfile.VictimGlowIntensity,
                Is.LessThan(hitFeedback.MediumProfile.VictimGlowIntensity));
            Assert.That(hitFeedback.MediumProfile.VictimGlowIntensity,
                Is.LessThan(hitFeedback.HeavyProfile.VictimGlowIntensity));

            foreach (ShowcaseRunnerAgent runner in pressure.Runners)
            {
                runner.enabled = false;
            }
            Assert.That(pressure.PlayerGrabbed, Is.False);
            PlaceRunnerInFront(combat.transform, target);
            Collider comboTargetCollider = target.GetComponent<Collider>();
            Assert.That(comboTargetCollider, Is.Not.Null);
            cameraRig.OutputCamera.transform.rotation = Quaternion.LookRotation(
                comboTargetCollider.bounds.center -
                    cameraRig.OutputCamera.transform.position,
                Vector3.up);
            Assert.That(
                pressure.ResolvePlayerMeleeTargetForReticle(new Ray(
                    cameraRig.OutputCamera.transform.position,
                    cameraRig.OutputCamera.transform.forward)),
                Is.SameAs(target),
                "The combo fixture must place its first victim under the optical reticle.");

            Assert.That(interaction.WeaponSelection,
                Is.EqualTo(PrototypeWeaponSelection.Fists));
            float comboStartedAt = Time.time;
            Assert.That(interaction.ProcessAttackPressed(), Is.True);
            Assert.That(combat.ActiveMeleeStep, Is.EqualTo(1));
            Assert.That(receiver.ComboBufferOpenInvocationCount, Is.EqualTo(0));
            Assert.That(combat.CanBufferMelee, Is.True,
                "Jab must remember the second press from its first active frame.");
            Assert.That(pressure.TryPlayerShove(), Is.True);
            Assert.That(combat.HasBufferedMelee, Is.True,
                "An early second LMB press must queue Cross instead of being discarded.");
            Assert.That(combat.TryRequestMelee(target), Is.True,
                "A rapid third press during Jab must reserve the next regular strike.");
            yield return WaitUntilOrFail(
                () => receiver.ComboBufferOpenInvocationCount >= 1,
                2f,
                "Jab combo-buffer AnimationEvent did not fire.");
            yield return WaitUntilOrFail(
                () => receiver.AttackWindowOpenInvocationCount >= 1,
                2f,
                "Jab attack-window AnimationEvent did not fire.");
            Assert.That(combatRig.TryGetPrimaryActiveStrikeVolume(
                out Vector3 activeStrikeCenter,
                out float activeStrikeRadius), Is.True);
            Assert.That(activeStrikeRadius, Is.GreaterThan(0f));
            Assert.That(pressurePresenter.TryGetPlayerStrikeMarker(
                out Vector3 presentedStrikeCenter,
                out float presentedStrikeRadius), Is.True);
            Assert.That(Vector3.Distance(activeStrikeCenter, presentedStrikeCenter),
                Is.LessThanOrEqualTo(0.001f));
            Assert.That(presentedStrikeRadius,
                Is.EqualTo(activeStrikeRadius).Within(0.001f));
            yield return WaitUntilOrFail(
                () => combatRig.AcceptedHitCount >= 1,
                1f,
                "Jab bones did not overlap the runner hurtbox.");
            Assert.That(combatRig.HitConfirmationSequence, Is.EqualTo(1));
            Assert.That(pressurePresenter.IsContactMarkerVisible, Is.True);
            Assert.That(Vector3.Distance(
                    pressurePresenter.LastContactMarkerWorldPoint,
                    combatRig.LastAcceptedHitPoint),
                Is.LessThanOrEqualTo(0.001f));
            Assert.That(target.Health.CurrentHealth, Is.EqualTo(85));
            Assert.That(hitFeedback.PlayCount, Is.EqualTo(1));
            Assert.That(hitFeedback.LastLevel, Is.EqualTo(ShowcaseHitVfxTier.Light));
            Assert.That(hitFeedback.GlowPlaybackCount, Is.EqualTo(1));
            Assert.That(hitFeedback.IsVictimGlowActive(target.transform), Is.True);
            AssertVictimEmissionVisible(target.transform);
            Assert.That(hitFeedback.LastImpulseDuration, Is.EqualTo(0.075f).Within(0.001f));
            yield return null;
            Assert.That(rotationImpulse.PeakAppliedEulerMagnitude, Is.GreaterThan(0f));
            Assert.That(rotationImpulse.PeakAppliedPositionMagnitude, Is.GreaterThan(0f));
            yield return WaitUntilOrFail(
                () => receiver.AttackWindowCloseInvocationCount >= 1,
                1f,
                "Jab attack-window close AnimationEvent did not fire.");
            yield return WaitUntilOrFail(
                () => combat.ActiveMeleeStep == 2,
                2f,
                "Jab completion did not start Cross.");

            Assert.That(combat.ActiveMeleeStep, Is.EqualTo(2));
            Assert.That(Time.time - comboStartedAt, Is.LessThanOrEqualTo(0.55f),
                "Jab-to-Cross transition remained too slow.");
            Assert.That(receiver.ComboBufferOpenInvocationCount, Is.EqualTo(1));
            Assert.That(combat.CanBufferMelee, Is.False,
                "The already-reserved follow-up must reject additional presses.");
            Assert.That(pressure.TryPlayerShove(), Is.False);
            Assert.That(combat.HasBufferedMelee, Is.True,
                "The rapid third press must remain reserved through Cross.");
            yield return WaitUntilOrFail(
                () => receiver.ComboBufferOpenInvocationCount >= 2,
                2f,
                "Cross combo-buffer AnimationEvent did not fire.");
            yield return WaitUntilOrFail(
                () => receiver.AttackWindowOpenInvocationCount >= 2,
                2f,
                "Cross attack-window AnimationEvent did not fire.");
            yield return WaitUntilOrFail(
                () => combatRig.AcceptedHitCount >= 2 ||
                      combat.ActiveMeleeStep != 2,
                1f,
                "Cross bones did not overlap the runner hurtbox.");
            Assert.That(combatRig.AcceptedHitCount, Is.EqualTo(2),
                $"Cross missed before returning to Jab. Minimum surface " +
                $"separation: {combatRig.MinimumWindowSurfaceSeparation:F4} m; " +
                $"rejectedDamage={target.Health.RejectedDamageCount}.");
            Assert.That(target.Health.CurrentHealth, Is.EqualTo(65));
            Assert.That(hitFeedback.PlayCount, Is.EqualTo(2));
            Assert.That(hitFeedback.LastLevel, Is.EqualTo(ShowcaseHitVfxTier.Medium));
            yield return WaitUntilOrFail(
                () => combat.ActiveMeleeStep == 1,
                2f,
                "A non-lethal Cross must loop back to Jab instead of starting Hook.");
            Assert.That(target.Health.CurrentHealth, Is.EqualTo(65));
            Assert.That(combat.TryRequestMelee(target), Is.True);
            Assert.That(combat.TryRequestMelee(target), Is.True,
                "Rapid input must reserve Cross and the following finisher decision.");
            yield return WaitUntilOrFail(
                () => receiver.AttackWindowOpenInvocationCount >= 3,
                2f,
                "Second Jab attack-window AnimationEvent did not fire.");
            yield return WaitUntilOrFail(
                () => combatRig.AcceptedHitCount >= 3,
                1f,
                "Second Jab bones did not overlap the runner hurtbox.");
            Assert.That(target.Health.CurrentHealth, Is.EqualTo(50));
            Assert.That(hitFeedback.LastLevel, Is.EqualTo(ShowcaseHitVfxTier.Light));

            yield return WaitUntilOrFail(
                () => combat.ActiveMeleeStep == 2,
                2f,
                "Second Jab completion did not start Cross.");
            yield return WaitUntilOrFail(
                () => receiver.AttackWindowOpenInvocationCount >= 4,
                2f,
                "Second Cross attack-window AnimationEvent did not fire.");
            yield return WaitUntilOrFail(
                () => combatRig.AcceptedHitCount >= 4,
                1f,
                "Second Cross bones did not overlap the runner hurtbox.");
            Assert.That(target.Health.CurrentHealth, Is.EqualTo(30));
            Assert.That(hitFeedback.LastLevel, Is.EqualTo(ShowcaseHitVfxTier.Medium));

            yield return WaitUntilOrFail(
                () => combat.ActiveMeleeStep == 3,
                2f,
                "Cross at lethal Hook health did not start the finisher.");
            yield return WaitUntilOrFail(
                () => receiver.AttackWindowOpenInvocationCount >= 5,
                2f,
                "Finisher Hook attack-window AnimationEvent did not fire.");
            yield return WaitUntilOrFail(
                () => combatRig.AcceptedHitCount >= 5,
                1f,
                "Finisher Hook bones did not overlap the runner hurtbox.");
            Assert.That(target.Health.CurrentHealth, Is.Zero);
            Assert.That(target.State, Is.EqualTo(ShowcaseRunnerState.Dead));
            Assert.That(hitFeedback.PlayCount, Is.EqualTo(5));
            Assert.That(hitFeedback.LastLevel, Is.EqualTo(ShowcaseHitVfxTier.Heavy));
            Assert.That(Time.time - comboStartedAt, Is.LessThanOrEqualTo(3.2f),
                "The health-gated five-hit finisher chain was not responsive.");
            yield return WaitUntilOrFail(
                () => !hitFeedback.IsShakeActive &&
                      hitFeedback.ActiveVictimGlowCount == 0,
                0.35f,
                "Rapid hit feedback left camera impulse or victim glow residue.");
            Assert.That(cameraRig.PostBrainTransformWriteCount, Is.Zero,
                "Hit feedback must remain inside the Cinemachine pipeline.");
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
            ShowcaseHitFeedbackController hitFeedback =
                SceneComponents<ShowcaseHitFeedbackController>().Single();
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
            Assert.That(hitFeedback.PlayCount, Is.Zero);
            Assert.That(hitFeedback.GlowPlaybackCount, Is.Zero);
            Assert.That(hitFeedback.ActiveVictimGlowCount, Is.Zero);
            Assert.That(hitFeedback.IsShakeActive, Is.False);
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
            ShowcaseHitFeedbackController hitFeedback =
                SceneComponents<ShowcaseHitFeedbackController>().Single();
            foreach (ShowcaseRunnerAgent candidate in pressure.Runners)
            {
                candidate.enabled = candidate == runner;
            }

            Vector3 position = player.transform.position +
                player.transform.forward * runner.AttackStandoffDistance;
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
            Assert.That(runner.PreparedAttackCue,
                Is.EqualTo(ShowcaseRunnerAnimationCue.Boxing));
            Assert.That(
                runner.PreparedAttackCue,
                Is.Not.EqualTo(ShowcaseRunnerAnimationCue.EliteJumpAttack));
            Assert.That(rig.IsAttackWindowOpen, Is.False);
            Assert.That(rig.AcceptedHitCount, Is.Zero);
            Assert.That(player.Health.CurrentHealth, Is.EqualTo(startingHealth));
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
            Assert.That(hitFeedback.PlayCount, Is.EqualTo(1));
            Assert.That(hitFeedback.LastLevel, Is.EqualTo(ShowcaseHitVfxTier.Light));
            Assert.That(hitFeedback.GlowPlaybackCount, Is.EqualTo(1));
            Assert.That(hitFeedback.IsVictimGlowActive(player.transform), Is.True);
            AssertVictimEmissionVisible(player.transform);
        }

        [UnityTest]
        public IEnumerator RunnerApproach_ClosesToNaturalBoxingContactDistanceBeforeAttack()
        {
            yield return LoadScene();

            ShowcasePlayerCombatController player =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            ShowcaseRunnerAgent runner = pressure.Runners.First(item => !item.IsElite);
            ShowcaseCharacterAnimationDriver driver =
                runner.GetComponent<ShowcaseCharacterAnimationDriver>();
            foreach (ShowcaseRunnerAgent candidate in pressure.Runners)
            {
                candidate.enabled = candidate == runner;
            }

            Vector3 position = player.transform.position +
                player.transform.forward * 0.9f;
            Quaternion rotation = Quaternion.LookRotation(
                player.transform.position - position,
                Vector3.up);
            Rigidbody body = runner.GetComponent<Rigidbody>();
            body.position = position;
            body.rotation = rotation;
            if (!body.isKinematic)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
            runner.transform.SetPositionAndRotation(position, rotation);
            Physics.SyncTransforms();

            int startingHealth = player.Health.CurrentHealth;
            float approachDeadline = Time.realtimeSinceStartup + 4f;
            while (runner.AnimationCue != ShowcaseRunnerAnimationCue.Boxing &&
                   Time.realtimeSinceStartup < approachDeadline)
            {
                yield return null;
            }
            float distanceAtCommit = Vector3.ProjectOnPlane(
                runner.transform.position - player.transform.position,
                Vector3.up).magnitude;
            Assert.That(
                runner.AnimationCue,
                Is.EqualTo(ShowcaseRunnerAnimationCue.Boxing),
                $"Runner never completed its approach: state={runner.State}, " +
                $"distance={distanceAtCommit:F3}, pressure={pressure.CanSimulatePressure()}, " +
                $"token={pressure.AttackTokenOwner?.RunnerId ?? "none"}.");

            yield return WaitUntilOrFail(
                () => driver.CurrentBaseStateName == "AttackBoxingAction",
                0.5f,
                "The runner dropped through Idle instead of beginning the prepared Boxing wind-up.");

            float planarDistance = Vector3.ProjectOnPlane(
                runner.transform.position - player.transform.position,
                Vector3.up).magnitude;
            Assert.That(
                planarDistance,
                Is.LessThanOrEqualTo(runner.AttackStandoffDistance + 0.06f),
                "Runner telegraphed before reaching natural limb-contact distance.");
            Assert.That(
                planarDistance,
                Is.GreaterThanOrEqualTo(0.795f),
                "Runner penetrated the player capsule to manufacture attack contact.");

            yield return WaitUntilOrFail(
                () => player.Health.CurrentHealth == startingHealth - 20,
                2f,
                "Natural Boxing did not damage the player after the autonomous approach.");
        }

        [UnityTest]
        public IEnumerator RunnerAttackRecovery_UsesCooldownAndTelegraphBeforeNextPunch()
        {
            yield return LoadScene();

            ShowcasePlayerCombatController player =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            ShowcaseRunnerAgent runner = pressure.Runners.First(item => !item.IsElite);
            foreach (ShowcaseRunnerAgent candidate in pressure.Runners)
            {
                candidate.enabled = candidate == runner;
            }

            PositionRunnerAtAttackStandoff(player.transform, runner);
            ShowcaseCharacterAnimationDriver driver =
                runner.GetComponent<ShowcaseCharacterAnimationDriver>();
            ShowcaseAnimationEventReceiver receiver =
                driver.Animator.GetComponent<ShowcaseAnimationEventReceiver>();
            Assert.That(pressure.TryAcquireAttackToken(runner), Is.True);
            runner.SendMessage("CommitNextAttack", SendMessageOptions.RequireReceiver);
            Assert.That(runner.PreparedAttackCue,
                Is.EqualTo(ShowcaseRunnerAnimationCue.Boxing));
            yield return WaitUntilOrFail(
                () => runner.AnimationCue ==
                    ShowcaseRunnerAnimationCue.Boxing &&
                    !runner.IsAttackWindup,
                1f,
                "The prepared Boxing cue did not commit after Telegraph.");
            uint firstRevision = runner.AnimationCueRevision;

            yield return WaitUntilOrFail(
                () => receiver.AttackWindowCloseInvocationCount == 1,
                3f,
                "The first punch window never closed.");

            yield return WaitUntilOrFail(
                () => runner.IsAttackWindup,
                2f,
                "The next attack never entered its cooldown-gated Telegraph.");
            Assert.That(runner.AnimationCueRevision, Is.EqualTo(firstRevision),
                "The next strike clip must not begin before Telegraph completes.");
            float telegraphStartedAt = Time.time;
            float expectedTelegraph = runner.ActiveTelegraphDuration;
            float chainDeadline = Time.realtimeSinceStartup + 2f;
            while (runner.AnimationCueRevision == firstRevision &&
                   Time.realtimeSinceStartup < chainDeadline)
            {
                yield return null;
            }

            Assert.That(runner.AnimationCueRevision, Is.GreaterThan(firstRevision),
                "The next authored punch did not chain from recovery.");
            Assert.That(Time.time - telegraphStartedAt,
                Is.GreaterThanOrEqualTo(expectedTelegraph - 0.06f),
                "The next strike clip began before the deterministic Telegraph ended.");
            Assert.That(runner.AnimationCue,
                Is.EqualTo(ShowcaseRunnerAnimationCue.Boxing),
                "The normal cadence must begin with two or three punches before a kick.");
            Assert.That(driver.CurrentBaseStateName,
                Is.EqualTo("AttackBoxingAction"));
        }

        [UnityTest]
        public IEnumerator ApprovedEnemyAttackMatrix_AllTenUnarmedCuesUseNaturalBoneWindowsOnce()
        {
            RunnerAttackMatrixCase[] cases =
            {
                new(
                    "Normal Kicking",
                    false,
                    true,
                    0,
                    ShowcaseRunnerAnimationCue.Kicking,
                    30,
                    HumanBodyBones.LeftFoot),
                new(
                    "Normal Boxing",
                    false,
                    false,
                    0,
                    ShowcaseRunnerAnimationCue.Boxing,
                    20,
                    HumanBodyBones.LeftHand),
                new(
                    "Normal Kicking2",
                    false,
                    true,
                    1,
                    ShowcaseRunnerAnimationCue.Kicking2,
                    30,
                    HumanBodyBones.RightFoot),
                new(
                    "Normal Knee Strike",
                    false,
                    true,
                    2,
                    ShowcaseRunnerAnimationCue.Boxing2,
                    30,
                    HumanBodyBones.RightFoot),
                new(
                    "Elite Kicking",
                    true,
                    true,
                    0,
                    ShowcaseRunnerAnimationCue.Kicking,
                    30,
                    HumanBodyBones.LeftFoot),
                new(
                    "Elite Boxing",
                    true,
                    false,
                    0,
                    ShowcaseRunnerAnimationCue.Boxing,
                    20,
                    HumanBodyBones.LeftHand),
                new(
                    "Elite Kicking2",
                    true,
                    true,
                    1,
                    ShowcaseRunnerAnimationCue.Kicking2,
                    30,
                    HumanBodyBones.RightFoot),
                new(
                    "Elite Knee Strike",
                    true,
                    true,
                    2,
                    ShowcaseRunnerAnimationCue.Boxing2,
                    30,
                    HumanBodyBones.RightFoot),
                new(
                    "Elite Mutant Punch",
                    true,
                    false,
                    4,
                    ShowcaseRunnerAnimationCue.EliteMutantPunch,
                    25,
                    HumanBodyBones.RightHand),
                new(
                    "Elite Mutant Swiping",
                    true,
                    false,
                    6,
                    ShowcaseRunnerAnimationCue.EliteMutantSwiping,
                    25,
                    HumanBodyBones.RightHand,
                    HumanBodyBones.LeftHand)
            };

            foreach (RunnerAttackMatrixCase attackCase in cases)
            {
                yield return ReloadSceneForAttackMatrixCase();

                ShowcasePlayerCombatController player =
                    SceneComponents<ShowcasePlayerCombatController>().Single();
                ShowcaseRunnerPressureController pressure =
                    SceneComponents<ShowcaseRunnerPressureController>().Single();
                ShowcaseRunnerAgent runner = pressure.Runners
                    .Single(item => item.IsElite == attackCase.Elite);
                foreach (ShowcaseRunnerAgent candidate in pressure.Runners)
                {
                    candidate.enabled = candidate == runner;
                }

                SeedRunnerAttackSelector(runner, attackCase);
                PositionRunnerAtAttackStandoff(player.transform, runner);

                ShowcaseAnimationEventReceiver receiver = runner
                    .GetComponent<ShowcaseCharacterAnimationDriver>()
                    .Animator.GetComponent<ShowcaseAnimationEventReceiver>();
                ShowcaseBoneCombatRig rig = runner.GetComponent<ShowcaseBoneCombatRig>();
                int startingHealth = player.Health.CurrentHealth;
                int startingDamageCount = player.Health.AcceptedDamageCount;

                Assert.That(pressure.TryAcquireAttackToken(runner), Is.True, attackCase.Name);
                runner.SendMessage("CommitNextAttack", SendMessageOptions.RequireReceiver);
                Assert.That(runner.PreparedAttackCue,
                    Is.EqualTo(attackCase.ExpectedCue), attackCase.Name);
                Assert.That(rig.IsAttackWindowOpen, Is.False, attackCase.Name);
                Assert.That(rig.AcceptedHitCount, Is.Zero, attackCase.Name);
                Assert.That(player.Health.CurrentHealth,
                    Is.EqualTo(startingHealth), attackCase.Name);
                Assert.That(player.Health.AcceptedDamageCount,
                    Is.EqualTo(startingDamageCount), attackCase.Name);

                float centerDistance = Vector3.ProjectOnPlane(
                    runner.transform.position - player.transform.position,
                    Vector3.up).magnitude;
                Assert.That(centerDistance, Is.GreaterThanOrEqualTo(0.795f),
                    $"{attackCase.Name}: runner penetrated the player capsule before contact.");

                yield return WaitUntilOrFail(
                    () => receiver.AttackWindowOpenInvocationCount == 1 &&
                          rig.IsAttackWindowOpen,
                    3f,
                    $"{attackCase.Name}: natural attack-window event did not open.");
                CollectionAssert.AreEqual(
                    attackCase.ExpectedStrikeBones,
                    ResolveActiveStrikeBones(rig),
                    $"{attackCase.Name}: wrong humanoid strike bone selection.");

                yield return WaitUntilOrFail(
                    () => rig.AcceptedHitCount == 1,
                    1f,
                    $"{attackCase.Name}: approved strike bones did not overlap player hurtboxes. " +
                    $"Minimum surface separation: {rig.MinimumWindowSurfaceSeparation:F4} m.");
                Assert.That(rig.ContactCount, Is.EqualTo(1), attackCase.Name);
                Assert.That(player.Health.CurrentHealth,
                    Is.EqualTo(startingHealth - attackCase.ExpectedDamage),
                    attackCase.Name);
                Assert.That(player.Health.AcceptedDamageCount,
                    Is.EqualTo(startingDamageCount + 1), attackCase.Name);
                if (attackCase.KickSlot)
                {
                    Assert.That(new[]
                    {
                        ShowcasePlayerAnimationCue.KnockdownBack,
                        ShowcasePlayerAnimationCue.KnockdownFront
                    }, Does.Contain(player.AnimationCue),
                        $"{attackCase.Name}: kick-class attacks must remain the only knockdowns.");
                }
                else
                {
                    Assert.That(player.AnimationCue,
                        Is.EqualTo(ShowcasePlayerAnimationCue.HitBody),
                        $"{attackCase.Name}: punch-class attacks must not knock the player down.");
                }

                yield return WaitUntilOrFail(
                    () => receiver.AttackWindowCloseInvocationCount == 1 &&
                          !rig.IsAttackWindowOpen,
                    3f,
                    $"{attackCase.Name}: natural attack-window close event did not fire.");
                Assert.That(pressure.AttackTokenOwner, Is.Null,
                    $"{attackCase.Name}: the next runner was blocked through the harmless recovery tail.");
                int healthAtClose = player.Health.CurrentHealth;
                yield return new WaitForSeconds(0.2f);
                Assert.That(rig.AcceptedHitCount, Is.EqualTo(1),
                    $"{attackCase.Name}: duplicate hit committed after window close.");
                Assert.That(player.Health.CurrentHealth, Is.EqualTo(healthAtClose),
                    $"{attackCase.Name}: duplicate damage committed after window close.");
                Assert.That(player.Health.AcceptedDamageCount,
                    Is.EqualTo(startingDamageCount + 1), attackCase.Name);
            }
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
        public IEnumerator GroundPickup_ActionCompleteSettlesOneMissingCarryContactDuringHitch()
        {
            yield return LoadScene();

            ShowcasePlayerCombatController combat =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            PrototypeInteractionStateOwner owner =
                combat.GetComponent<PrototypeInteractionStateOwner>();
            PrototypePhysicalObjectLifecycle lifecycle =
                combat.GetComponent<PrototypePhysicalObjectLifecycle>();
            PrototypeInteractionTarget chair =
                SceneComponents<PrototypeInteractionTarget>()
                    .First(item =>
                        item.IsScoopEligible &&
                        item.WeightClass == PrototypeWeightClass.Medium);

            DisableRunners();
            chair.ConfigureDeferredAcquisitionContact(true);
            Assert.That(owner.TryCommit(PrototypeInteractionDecision.Accept(
                PrototypeInteractionOutcome.Scoop,
                chair)), Is.True);
            Assert.That(combat.TryBeginAcquisition(
                chair,
                PrototypeInteractionOutcome.Scoop), Is.True);
            Assert.That(lifecycle.CarriedTarget, Is.Null);

            combat.CompleteAnimationAction();

            Assert.That(lifecycle.CarriedTarget, Is.SameAs(chair),
                "Acquire ActionComplete must settle one skipped CarryContact " +
                "callback even when a hitch delivers completion before LateUpdate " +
                "can acknowledge the new Animator state.");
            Assert.That(owner.HandState,
                Is.EqualTo(PrototypeHandOwnershipState.Carrying));
            Assert.That(combat.IsActionLocked, Is.False);
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
            Collider binderCollider = target.GetComponent<Collider>();
            CharacterController playerCollider = combat.GetComponent<CharacterController>();
            Assert.That(binderCollider, Is.Not.Null);
            Assert.That(playerCollider, Is.Not.Null);
            bool originalIsKinematic = body.isKinematic;
            bool originalUseGravity = body.useGravity;
            RigidbodyConstraints originalConstraints = body.constraints;
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
            Assert.That(body.constraints, Is.EqualTo(originalConstraints));
            Assert.That(Physics.GetIgnoreCollision(playerCollider, binderCollider), Is.True);

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
            Assert.That(body.constraints,
                Is.EqualTo(originalConstraints |
                    RigidbodyConstraints.FreezePositionX |
                    RigidbodyConstraints.FreezePositionZ |
                    RigidbodyConstraints.FreezeRotation));
            Assert.That(Physics.GetIgnoreCollision(playerCollider, binderCollider), Is.True);
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
            Assert.That(body.constraints,
                Is.EqualTo(originalConstraints |
                    RigidbodyConstraints.FreezePositionX |
                    RigidbodyConstraints.FreezePositionZ |
                    RigidbodyConstraints.FreezeRotation));

            lifecycle.enabled = false;
            Physics.SyncTransforms();
            Assert.That(target.gameObject.activeSelf, Is.True,
                "A queued-throw cancellation with a valid fallback pose must not " +
                "quarantine the Binder.");
            Assert.That(body.constraints, Is.EqualTo(originalConstraints));
            Assert.That(Physics.GetIgnoreCollision(playerCollider, binderCollider), Is.False);
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
            Scene existing = SceneManager.GetSceneByPath(ScenePath);
            if (existing.IsValid() && existing.isLoaded)
            {
                AsyncOperation unloadExisting = SceneManager.UnloadSceneAsync(existing);
                if (unloadExisting != null)
                {
                    yield return unloadExisting;
                }
                yield return null;
            }

            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
            Assert.That(load, Is.Not.Null);
            yield return load;
            loadedScene = SceneManager.GetSceneByPath(ScenePath);
            Assert.That(loadedScene.IsValid() && loadedScene.isLoaded, Is.True);
            yield return null;
        }

        private IEnumerator ReloadSceneForAttackMatrixCase()
        {
            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(loadedScene);
                if (unload != null)
                {
                    yield return unload;
                }
                loadedScene = default;
                yield return null;
            }

            yield return LoadScene();
        }

        private static void SeedRunnerAttackSelector(
            ShowcaseRunnerAgent runner,
            RunnerAttackMatrixCase attackCase)
        {
            int modulus = attackCase.KickSlot ? 3 : 8;
            SetPrivateIntField(runner, "punchesUntilKick",
                attackCase.KickSlot ? 0 : 1);
            SetPrivateUIntField(
                runner,
                "attackRandomState",
                FindRandomStateForSlot(attackCase.RandomSlot, modulus));
        }

        private static void SetPrivateIntField(
            ShowcaseRunnerAgent runner,
            string fieldName,
            int value)
        {
            FieldInfo field = typeof(ShowcaseRunnerAgent).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing runner selector field: {fieldName}.");
            field.SetValue(runner, value);
        }

        private static void SetPrivateUIntField(
            ShowcaseRunnerAgent runner,
            string fieldName,
            uint value)
        {
            FieldInfo field = typeof(ShowcaseRunnerAgent).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing runner selector field: {fieldName}.");
            field.SetValue(runner, value);
        }

        private static uint FindRandomStateForSlot(int slot, int modulus)
        {
            for (uint seed = 1; seed < 100000; seed++)
            {
                uint state = seed;
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;
                if (state % (uint)modulus == (uint)slot)
                {
                    return seed;
                }
            }

            Assert.Fail($"No deterministic selector seed found for slot {slot}/{modulus}.");
            return 1;
        }

        private static HumanBodyBones[] ResolveActiveStrikeBones(
            ShowcaseBoneCombatRig rig)
        {
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo countField = typeof(ShowcaseBoneCombatRig).GetField(
                "activeStrikeCount",
                Flags);
            FieldInfo indicesField = typeof(ShowcaseBoneCombatRig).GetField(
                "activeStrikeIndices",
                Flags);
            FieldInfo strikesField = typeof(ShowcaseBoneCombatRig).GetField(
                "strikes",
                Flags);
            Assert.That(countField, Is.Not.Null);
            Assert.That(indicesField, Is.Not.Null);
            Assert.That(strikesField, Is.Not.Null);

            int count = (int)countField.GetValue(rig);
            int[] indices = (int[])indicesField.GetValue(rig);
            Array strikes = (Array)strikesField.GetValue(rig);
            HumanBodyBones[] bones = new HumanBodyBones[count];
            for (int index = 0; index < count; index++)
            {
                object strike = strikes.GetValue(indices[index]);
                PropertyInfo boneProperty = strike.GetType().GetProperty(
                    "Bone",
                    BindingFlags.Instance | BindingFlags.Public);
                Assert.That(boneProperty, Is.Not.Null);
                bones[index] = (HumanBodyBones)boneProperty.GetValue(strike);
            }
            return bones;
        }

        private static void PositionRunnerAtAttackStandoff(
            Transform player,
            ShowcaseRunnerAgent runner)
        {
            Vector3 position = player.position +
                player.forward * runner.AttackStandoffDistance;
            Quaternion rotation = Quaternion.LookRotation(
                player.position - position,
                Vector3.up);
            Rigidbody body = runner.GetComponent<Rigidbody>();
            body.position = position;
            body.rotation = rotation;
            runner.transform.SetPositionAndRotation(position, rotation);
            Physics.SyncTransforms();
        }

        private static void SetRunnerPose(
            ShowcaseRunnerAgent runner,
            Vector3 position,
            Quaternion rotation)
        {
            Rigidbody body = runner.GetComponent<Rigidbody>();
            body.position = position;
            body.rotation = rotation;
            runner.transform.SetPositionAndRotation(position, rotation);
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

        private static void AssertVictimEmissionVisible(Transform victim)
        {
            Transform visual = victim.Find("RUNTIME_DESIGNER_A_VISUAL");
            Assert.That(visual, Is.Not.Null);
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            Assert.That(renderers, Is.Not.Empty);
            int emissionColorId = Shader.PropertyToID("_EmissionColor");
            int emissionMapId = Shader.PropertyToID("_EmissionMap");
            bool visibleEmissionFound = false;
            var block = new MaterialPropertyBlock();
            var diagnostics = new System.Collections.Generic.List<string>();
            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0;
                     materialIndex < materials.Length;
                     materialIndex++)
                {
                    block.Clear();
                    renderer.GetPropertyBlock(block, materialIndex);
                    Color emission = block.GetColor(emissionColorId);
                    Texture emissionMap = block.GetTexture(emissionMapId);
                    visibleEmissionFound |= emission.maxColorComponent >= 1.5f &&
                        emissionMap == Texture2D.whiteTexture;
                    diagnostics.Add(
                        $"{renderer.name}[{materialIndex}] emission={emission} " +
                        $"map={(emissionMap != null ? emissionMap.name : "null")}");
                }
            }
            Assert.That(visibleEmissionFound, Is.True,
                "Accepted victim did not receive the visible full-body emission pulse. " +
                string.Join("; ", diagnostics));
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

        private static void CompleteStrapAttachSequence(
            ShowcaseCargoStrapController cargo)
        {
            uint prepareRevision = cargo.AnimationCueRevision;
            Assert.That(cargo.NotifyAnimationStateEntered(prepareRevision), Is.True);
            Assert.That(cargo.CompleteAnimationStrapAction(prepareRevision), Is.True);

            uint throwRevision = cargo.AnimationCueRevision;
            Assert.That(cargo.NotifyAnimationStateEntered(throwRevision), Is.True);
            Assert.That(cargo.CommitAnimationStrapThrowRelease(throwRevision), Is.True);
            Assert.That(cargo.CommitAnimationStrapAttachContact(throwRevision), Is.True);
            Assert.That(cargo.CompleteAnimationStrapAction(throwRevision), Is.True);

            uint pullRevision = cargo.AnimationCueRevision;
            Assert.That(cargo.NotifyAnimationStateEntered(pullRevision), Is.True);
            Assert.That(cargo.CommitAnimationStrapPullComplete(pullRevision), Is.True);
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
