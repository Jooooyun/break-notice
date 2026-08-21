using System.Collections;
using System.Linq;
using System.Reflection;
using BreakNotice.Gameplay;
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
    public sealed class ContestShowcaseS5PlayModeTests
    {
        private static int cleanupSequence;

        private const string S4ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v004.unity";
        private const string S5ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v005.unity";
        private const string S7ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v007.unity";

        [UnityTearDown]
        public IEnumerator TearDownLoadedShowcaseScene()
        {
            Scene cleanup = SceneManager.CreateScene(
                $"S5_TEST_CLEANUP_{++cleanupSequence}");
            SceneManager.SetActiveScene(cleanup);
            for (int index = SceneManager.sceneCount - 1; index >= 0; index--)
            {
                Scene loaded = SceneManager.GetSceneAt(index);
                if (loaded == cleanup || !loaded.isLoaded)
                {
                    continue;
                }

                AsyncOperation unload = SceneManager.UnloadSceneAsync(loaded);
                if (unload != null)
                {
                    yield return unload;
                }
            }
        }

        [UnityTest]
        public IEnumerator S4RemainsPreservedAndS5HasSingularStrapComposition()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S4ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            Assert.That(SceneComponents<ShowcaseCargoStrapController>(), Is.Empty);

            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S5ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            ShowcaseCargoStrapController cargo =
                SceneComponents<ShowcaseCargoStrapController>().Single();
            PrototypeInteractionController interaction =
                SceneComponents<PrototypeInteractionController>().Single();
            ShowcaseTeamContextController team =
                SceneComponents<ShowcaseTeamContextController>().Single();

            Assert.That(interaction.ShowcaseCargoStrapController, Is.SameAs(cargo));
            Assert.That(team.CargoStrapController, Is.SameAs(cargo));
            LineRenderer strapLine = SceneComponents<LineRenderer>().Single(line =>
                line.name == "GEAR01_SEGMENTED_STRAP_GRAYBOX");
            ShowcaseCargoStrapPresentation presentation =
                SceneComponents<ShowcaseCargoStrapPresentation>().Single();
            Assert.That(presentation.Source, Is.SameAs(cargo));
            Assert.That(presentation.StrapLine, Is.SameAs(strapLine));
            Assert.That(presentation.ReelSocket, Is.Not.Null);
            Assert.That(presentation.RightHandSocket, Is.Not.Null);
            Assert.That(presentation.ReviewClampMarker, Is.Not.Null);
            Assert.That(presentation.ReviewClampMarker
                .GetComponentInChildren<Collider>(true), Is.Null);
            Animator playerAnimator = cargo.GetComponentsInChildren<Animator>(true)
                .First(animator => animator.isHuman);
            Transform actualRightHand = playerAnimator.GetBoneTransform(
                HumanBodyBones.RightHand);
            Assert.That(presentation.ResolvedRightHandBone, Is.SameAs(actualRightHand));
            Assert.That(Vector3.Distance(
                    presentation.ResolvedRightHandBone.position,
                    actualRightHand.position),
                Is.LessThan(0.001f));
            Assert.That(presentation.IsUsingSupportedRuntimeMaterial, Is.True);
            Assert.That(presentation.StrapLine.sharedMaterial.shader.isSupported,
                Is.True);
            PrototypeInteractionTarget[] strappedObjects =
                SceneComponents<PrototypeInteractionTarget>()
                    .Where(target =>
                        target.Role == PrototypeInteractionRole.DirectBinder ||
                        target.Role == PrototypeInteractionRole.AllyBinder ||
                        target.Role == PrototypeInteractionRole.DirectChair ||
                        target.Role == PrototypeInteractionRole.AllyChair)
                    .ToArray();
            Assert.That(strappedObjects, Is.Not.Empty);
            Assert.That(strappedObjects.All(target =>
                target.IsCargoStrapEligible && target.HasStrapClampSocket), Is.True);
        }

        [UnityTest]
        public IEnumerator AssistAttachAwardsBraceAndExplicitReleaseRestoresPhysicalState()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S5ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseCargoStrapController cargo =
                SceneComponents<ShowcaseCargoStrapController>().Single();
            PrototypeInteractionController interaction =
                SceneComponents<PrototypeInteractionController>().Single();
            ShowcaseTeamContextController team =
                SceneComponents<ShowcaseTeamContextController>().Single();
            PrototypeInteractionStateOwner owner =
                SceneComponents<PrototypeInteractionStateOwner>().Single();
            ShowcaseCargoStrapPresentation presentation =
                SceneComponents<ShowcaseCargoStrapPresentation>().Single();
            ShowcaseMobileAllyController mobile =
                SceneComponents<ShowcaseMobileAllyController>().Single();
            PrototypeInteractionTarget chair = SceneComponents<PrototypeInteractionTarget>()
                .First(target => target.Role == PrototypeInteractionRole.DirectChair);
            Rigidbody body = chair.GetComponent<Rigidbody>();
            Collider chairCollider = chair.GetComponent<Collider>();
            Collider playerCollider = owner.GetComponent<Collider>();
            RigidbodyConstraints originalConstraints = body.constraints;
            CollisionDetectionMode originalCollision = body.collisionDetectionMode;
            RigidbodyInterpolation originalInterpolation = body.interpolation;

            owner.transform.position = chair.transform.position + new Vector3(0f, 0.9f, -2f);
            mobile.transform.position = chair.transform.position + new Vector3(1.5f, 0f, 0f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            Assert.That(cargo.CanAssistAttach(), Is.True);
            Assert.That(team.ResolveCurrentQContext(), Is.EqualTo(ShowcaseQContext.AssistAttach));
            Assert.That(interaction.ProcessTeamContextQ(), Is.True);
            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Prepare));
            Assert.That(owner.StrapTarget, Is.Null);
            Assert.That(cargo.ActiveTarget, Is.SameAs(chair));
            Assert.That(team.HasBrace, Is.False);
            Assert.That(Physics.GetIgnoreCollision(playerCollider, chairCollider), Is.False);

            CompleteAttachSequence(cargo);
            Assert.That(owner.StrapState,
                Is.EqualTo(PrototypeStrapOwnershipState.Tensioned));
            Assert.That(owner.StrapTarget, Is.SameAs(chair));
            Assert.That(owner.WeaponSelection,
                Is.EqualTo(PrototypeWeaponSelection.ObjectWeapon));
            Assert.That(team.HasBrace, Is.True);
            Assert.That(Physics.GetIgnoreCollision(playerCollider, chairCollider), Is.True);

            Assert.That(interaction.ProcessStrapRelease(), Is.True);
            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Releasing));
            Assert.That(owner.StrapState,
                Is.EqualTo(PrototypeStrapOwnershipState.ReleaseRequested));
            Assert.That(cargo.ActiveTarget, Is.SameAs(chair));
            yield return new WaitForSeconds(0.22f);
            yield return null;

            Assert.That(owner.StrapState, Is.EqualTo(PrototypeStrapOwnershipState.Stored));
            Assert.That(cargo.ActiveTarget, Is.Null);
            Assert.That(body.constraints, Is.EqualTo(originalConstraints));
            Assert.That(body.collisionDetectionMode, Is.EqualTo(originalCollision));
            Assert.That(body.interpolation, Is.EqualTo(originalInterpolation));
            Assert.That(Physics.GetIgnoreCollision(playerCollider, chairCollider), Is.False);
        }

        [UnityTest]
        public IEnumerator AuthoredResetClearsStrapAndBraceReadiness()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S5ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseCargoStrapController cargo =
                SceneComponents<ShowcaseCargoStrapController>().Single();
            ShowcaseTeamContextController team =
                SceneComponents<ShowcaseTeamContextController>().Single();
            PrototypeInteractionStateOwner owner =
                SceneComponents<PrototypeInteractionStateOwner>().Single();
            AuthoredEncounterResetController reset =
                SceneComponents<AuthoredEncounterResetController>().Single();
            PrototypeInteractionTarget chair = SceneComponents<PrototypeInteractionTarget>()
                .First(target => target.Role == PrototypeInteractionRole.DirectChair);

            owner.transform.position = chair.transform.position + new Vector3(0f, 0.9f, -2f);
            SceneComponents<ShowcaseMobileAllyController>().Single().transform.position =
                chair.transform.position + new Vector3(1.5f, 0f, 0f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            Assert.That(team.TryExecuteAssistAttach(), Is.True);
            Assert.That(team.HasBrace, Is.False);
            CompleteAttachSequence(cargo);
            Assert.That(team.HasBrace, Is.True);

            Assert.That(reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
            yield return null;

            Assert.That(owner.StrapState, Is.EqualTo(PrototypeStrapOwnershipState.Stored));
            Assert.That(cargo.ActiveTarget, Is.Null);
            Assert.That(team.Badges, Is.EqualTo(ShowcaseTeamReadinessBadge.None));
        }

        [UnityTest]
        public IEnumerator SmallBinderSpinsAtActualHandPausesOnOneAndThrowsOnNextLmb()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S5ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseCargoStrapController cargo =
                SceneComponents<ShowcaseCargoStrapController>().Single();
            ShowcaseCargoStrapPresentation presentation =
                SceneComponents<ShowcaseCargoStrapPresentation>().Single();
            PrototypeInteractionStateOwner owner =
                SceneComponents<PrototypeInteractionStateOwner>().Single();
            PrototypeInteractionTarget binder = SceneComponents<PrototypeInteractionTarget>()
                .First(target => target.Role == PrototypeInteractionRole.DirectBinder);
            Rigidbody body = binder.GetComponent<Rigidbody>();

            owner.transform.position = binder.transform.position +
                new Vector3(0f, 0.9f, -2f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            Assert.That(binder.WeightClass, Is.EqualTo(PrototypeWeightClass.Light));
            Assert.That(binder.StrapEligible, Is.True);
            Assert.That(binder.HasStrapClampSocket, Is.True);
            Assert.That(binder.IsCargoStrapEligible, Is.True);
            Assert.That(cargo.TryBeginDirectAttach(binder), Is.True);
            Assert.That(owner.StrapState, Is.EqualTo(PrototypeStrapOwnershipState.Stored));
            Assert.That(owner.WeaponSelection, Is.EqualTo(PrototypeWeaponSelection.Fists));

            CompleteAttachSequence(cargo);
            Assert.That(owner.StrapState,
                Is.EqualTo(PrototypeStrapOwnershipState.Tensioned));
            Assert.That(cargo.Phase,
                Is.EqualTo(ShowcaseCargoStrapPhase.SmallSpinning));
            Assert.That(owner.ObjectWeaponTarget, Is.SameAs(binder));
            Assert.That(owner.WeaponSelection,
                Is.EqualTo(PrototypeWeaponSelection.ObjectWeapon));

            uint spinRevision = cargo.AnimationCueRevision;
            Assert.That(cargo.AnimationCue,
                Is.EqualTo(ShowcaseCargoStrapAnimationCue.Rolling));
            Assert.That(cargo.NotifyAnimationStateEntered(spinRevision), Is.True);
            Vector3 spinStart = body.worldCenterOfMass;
            float spinSettleDeadline = Time.time + 1.2f;
            float handDistance = float.PositiveInfinity;
            while (handDistance > 1f && Time.time < spinSettleDeadline)
            {
                yield return new WaitForFixedUpdate();
                if (presentation.ResolvedRightHandBone != null)
                {
                    handDistance = Vector3.Distance(
                        body.worldCenterOfMass,
                        presentation.ResolvedRightHandBone.position);
                }
            }
            Assert.That(Vector3.Distance(body.worldCenterOfMass, spinStart),
                Is.GreaterThan(0.05f));
            Assert.That(presentation.ResolvedRightHandBone, Is.Not.Null);
            Assert.That(cargo.ResolvedRightHandTransform,
                Is.SameAs(presentation.ResolvedRightHandBone));
            Assert.That(handDistance, Is.InRange(0.5f, 1.6f),
                "D-036 moves the physical Small orbit above the Head; it must " +
                "remain tethered to the hand without returning to hand-height.");
            Assert.That(presentation.ResolvedCarabinerSocket, Is.Not.Null);
            Animator playerAnimator = owner.GetComponent<
                ShowcaseCharacterAnimationDriver>().Animator;
            Vector3 expectedCarabinerWorld =
                cargo.PresentationSnapshot.ReelWorldPosition;
            Assert.That(Vector3.Distance(
                    presentation.ResolvedCarabinerSocket.position,
                    expectedCarabinerWorld),
                Is.LessThan(0.005f));
            Vector3 carabinerPlayerLocal = owner.transform.InverseTransformPoint(
                presentation.ResolvedCarabinerSocket.position);
            Assert.That(carabinerPlayerLocal.y, Is.InRange(-0.05f, 0.15f),
                "The visible model is offset below the CharacterController root; " +
                "using model Y as player-root Y raises the strap by about 0.89m.");
            Assert.That(
                ShowcaseCargoStrapController.TryResolveRightHandGripWorldPosition(
                    playerAnimator,
                    out Vector3 expectedGripWorld),
                Is.True);
            Assert.That(Vector3.Distance(
                    presentation.LastPalmCenterWorldPosition,
                    expectedGripWorld),
                Is.LessThan(0.005f));
            Assert.That(Vector3.Distance(
                    presentation.LastGripWorldPosition,
                    presentation.LastPalmCenterWorldPosition),
                Is.InRange(0.025f, 0.04f));
            Assert.That(presentation.StrapLine.GetPosition(
                    presentation.StrapLine.positionCount - 1),
                Is.EqualTo(presentation.LastGripWorldPosition));
            Assert.That(presentation.ObjectTetherLine, Is.Not.Null);
            Assert.That(presentation.ObjectTetherLine.enabled, Is.True);
            float tetherSurfaceOffset = Vector3.Distance(
                presentation.ObjectTetherLine.GetPosition(0),
                presentation.LastPalmCenterWorldPosition);
            Assert.That(tetherSurfaceOffset, Is.InRange(0.025f, 0.05f));
            Assert.That(presentation.StrapLine.GetPosition(0),
                Is.EqualTo(presentation.ResolvedCarabinerSocket.position));

            Assert.That(owner.TrySelectWeapon(PrototypeWeaponSelection.Fists), Is.True);
            yield return null;
            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Tensioned));
            Assert.That(owner.StrapState,
                Is.EqualTo(PrototypeStrapOwnershipState.Tensioned));
            Assert.That(owner.StrapTarget, Is.SameAs(binder));
            Assert.That(owner.ObjectWeaponTarget, Is.SameAs(binder));
            Assert.That(owner.TrySelectWeapon(
                PrototypeWeaponSelection.ObjectWeapon), Is.True);
            yield return null;
            Assert.That(cargo.Phase,
                Is.EqualTo(ShowcaseCargoStrapPhase.SmallSpinning));

            Vector3 throwAim = binder.transform.position +
                (owner.transform.forward * 10f) + Vector3.up;
            Assert.That(cargo.TryBeginAttack(throwAim), Is.True);
            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Throw));
            uint throwRevision = cargo.AnimationCueRevision;
            Assert.That(cargo.CompleteAnimationStrapAction(throwRevision), Is.False,
                "An outgoing state tail event must not complete Throw before state entry.");
            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Throw));
            Assert.That(cargo.NotifyAnimationStateEntered(throwRevision), Is.True);
            Assert.That(cargo.CommitAnimationStrapThrowRelease(throwRevision), Is.True);
            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Stored));
            Assert.That(owner.StrapState, Is.EqualTo(PrototypeStrapOwnershipState.Stored));
            Assert.That(owner.WeaponSelection, Is.EqualTo(PrototypeWeaponSelection.Fists));
            Assert.That(binder.gameObject.activeSelf, Is.True);
            Assert.That(binder.CommittedPlayerUses, Is.EqualTo(1));
            Assert.That(body.linearVelocity.magnitude, Is.GreaterThan(4f));
            float depletionDeadline = Time.time + 2f;
            while (binder.gameObject.activeSelf && Time.time < depletionDeadline)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(binder.gameObject.activeSelf, Is.False);
            for (int index = 0; index < 3; index++)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        [UnityTest]
        public IEnumerator ReleasedSmallThrowUsesSharedDurabilityAndDeactivatesAfterCollision()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S5ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            PrototypePhysicalObjectLifecycle lifecycle =
                SceneComponents<PrototypePhysicalObjectLifecycle>().Single();
            PrototypeInteractionTarget binder = SceneComponents<PrototypeInteractionTarget>()
                .First(target => target.Role == PrototypeInteractionRole.DirectBinder);
            Rigidbody body = binder.GetComponent<Rigidbody>();
            Vector3 aim = binder.transform.position + Vector3.forward * 8f + Vector3.up;

            Assert.That(lifecycle.TryLaunchReleasedStrapTargetAt(binder, aim), Is.True);
            Assert.That(binder.CommittedPlayerUses, Is.EqualTo(1));
            Assert.That(binder.IsPlayerUseExhausted, Is.True);
            Assert.That(body.collisionDetectionMode,
                Is.EqualTo(CollisionDetectionMode.ContinuousDynamic));
            Assert.That(body.linearVelocity.magnitude, Is.GreaterThan(4f));

            float collisionDeadline = Time.time + 2f;
            while (binder.gameObject.activeSelf && Time.time < collisionDeadline)
            {
                yield return null;
            }
            Assert.That(binder.gameObject.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator ActualHumanoidStrapClipsDriveNaturalAttachAttackAndReleaseEvents()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S7ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseCargoStrapController cargo =
                SceneComponents<ShowcaseCargoStrapController>().Single();
            ShowcaseCargoStrapPresentation presentation =
                SceneComponents<ShowcaseCargoStrapPresentation>().Single();
            PrototypeInteractionStateOwner owner =
                SceneComponents<PrototypeInteractionStateOwner>().Single();
            InputSystemLogicalInputAdapter input =
                SceneComponents<InputSystemLogicalInputAdapter>().Single();
            PrototypeInteractionTarget chair = SceneComponents<PrototypeInteractionTarget>()
                .First(target => target.Role == PrototypeInteractionRole.DirectChair);
            Rigidbody chairBody = chair.GetComponent<Rigidbody>();
            Collider chairCollider = chair.GetComponent<Collider>();
            ShowcaseCharacterAnimationDriver driver =
                cargo.GetComponent<ShowcaseCharacterAnimationDriver>();
            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            ShowcaseHitFeedbackController hitFeedback =
                SceneComponents<ShowcaseHitFeedbackController>().Single();
            ShowcaseRunnerAgent targetRunner = pressure.Runners
                .Single(runner => !runner.IsElite);
            ShowcaseRunnerAgent otherRunner = pressure.Runners
                .Single(runner => runner.IsElite);
            float targetRunnerY = targetRunner.transform.position.y;

            owner.transform.position = chair.transform.position +
                new Vector3(0f, 0.9f, -2f);
            targetRunner.transform.position = owner.transform.position +
                new Vector3(12f, 0f, 12f);
            otherRunner.transform.position = owner.transform.position +
                new Vector3(-12f, 0f, 12f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            Assert.That(cargo.TryBeginDirectAttach(chair), Is.True);
            float attachDeadline = Time.time + 5f;
            while (cargo.Phase != ShowcaseCargoStrapPhase.Tensioned &&
                   Time.time < attachDeadline)
            {
                yield return null;
            }

            ShowcaseCargoStrapPhase expectedPhase =
                cargo.ActiveTarget?.WeightClass == PrototypeWeightClass.Light
                    ? ShowcaseCargoStrapPhase.SmallSpinning
                    : ShowcaseCargoStrapPhase.Tensioned;
            Assert.That(cargo.Phase, Is.EqualTo(expectedPhase));
            Assert.That(owner.StrapState,
                Is.EqualTo(PrototypeStrapOwnershipState.Tensioned));
            Assert.That(owner.WeaponSelection,
                Is.EqualTo(PrototypeWeaponSelection.ObjectWeapon));
            Assert.That(cargo.FaultCount, Is.Zero, cargo.LastFault);
            Assert.That(cargo.PullDistanceReduced, Is.GreaterThan(0.35f));
            Assert.That(presentation.StrapLine.enabled, Is.True);

            float rearDeadline = Time.time + 2f;
            while (!cargo.IsMediumRearReady && Time.time < rearDeadline)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(cargo.IsMediumRearReady, Is.True);
            Assert.That(cargo.IsMediumFloorSupported, Is.True);
            Assert.That(cargo.PresentationSnapshot.HasDynamicTension, Is.True);
            Assert.That(cargo.PresentationSnapshot.Tension01,
                Is.EqualTo(0.55f).Within(0.001f));
            Assert.That(cargo.PresentationSnapshot.EffectiveTetherLength,
                Is.GreaterThan(0f));
            Vector3 rearOffset = Vector3.ProjectOnPlane(
                chairBody.worldCenterOfMass - owner.transform.position,
                Vector3.up);
            Assert.That(Vector3.Dot(
                    rearOffset.normalized,
                    owner.transform.forward),
                Is.LessThan(-0.5f));

            input.enabled = false;
            SetSnapshot(input, new LogicalInputSnapshot(
                Vector2.up,
                Vector2.zero,
                false,
                false,
                false,
                false,
                false,
                false));
            Vector3 tensionedMoveStart = owner.transform.position;
            Vector3 rearFollowPrevious = chairBody.worldCenterOfMass;
            float maximumRearFollowStep = 0f;
            for (int index = 0; index < 20; index++)
            {
                yield return new WaitForFixedUpdate();
                Vector3 currentRear = chairBody.worldCenterOfMass;
                maximumRearFollowStep = Mathf.Max(
                    maximumRearFollowStep,
                    Vector3.Distance(rearFollowPrevious, currentRear));
                rearFollowPrevious = currentRear;
            }
            Assert.That(Vector3.Distance(
                    tensionedMoveStart,
                    owner.transform.position),
                Is.GreaterThan(0.1f));
            Assert.That(driver.CurrentBaseStateName, Is.EqualTo("Walk"));
            Assert.That(driver.StrapRollingUpperBodyLayerWeight,
                Is.GreaterThan(0.9f));
            int strapUpperBodyLayer = driver.Animator.GetLayerIndex(
                "Strap Upper Body");
            Assert.That(strapUpperBodyLayer, Is.GreaterThanOrEqualTo(0));
            Assert.That(driver.Animator.GetCurrentAnimatorStateInfo(
                    strapUpperBodyLayer).IsName(
                    "Strap Upper Body.StrapTensionUpperBodyPose"),
                Is.True);
            SetSnapshot(input, new LogicalInputSnapshot(
                Vector2.up,
                Vector2.zero,
                false,
                false,
                false,
                false,
                false,
                false,
                sprint: true));
            for (int index = 0; index < 12; index++)
            {
                yield return new WaitForFixedUpdate();
                Vector3 currentRear = chairBody.worldCenterOfMass;
                maximumRearFollowStep = Mathf.Max(
                    maximumRearFollowStep,
                    Vector3.Distance(rearFollowPrevious, currentRear));
                rearFollowPrevious = currentRear;
            }
            Assert.That(driver.CurrentBaseStateName, Is.EqualTo("Run"));
            Assert.That(driver.Animator.GetCurrentAnimatorStateInfo(
                    strapUpperBodyLayer).IsName(
                    "Strap Upper Body.StrapTensionUpperBodyPose"),
                Is.True);
            Assert.That(maximumRearFollowStep, Is.LessThan(0.2f));
            SetSnapshot(input, default);
            owner.transform.rotation = Quaternion.Euler(
                0f,
                owner.transform.eulerAngles.y + 65f,
                0f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            float rotatedRearDeadline = Time.time + 2f;
            while (!cargo.IsMediumRearReady &&
                   Time.time < rotatedRearDeadline)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(cargo.IsMediumRearReady, Is.True);
            Assert.That(cargo.IsMediumFloorSupported, Is.True);
            rearOffset = Vector3.ProjectOnPlane(
                chairBody.worldCenterOfMass - owner.transform.position,
                Vector3.up);
            Assert.That(Vector3.Dot(
                    rearOffset.normalized,
                    owner.transform.forward),
                Is.LessThan(-0.5f));

            targetRunner.enabled = false;
            otherRunner.enabled = false;
            targetRunner.transform.position = owner.transform.position +
                (owner.transform.forward * 3f) -
                (owner.transform.right * 1.1f);
            targetRunner.transform.position = new Vector3(
                targetRunner.transform.position.x,
                targetRunnerY,
                targetRunner.transform.position.z);
            otherRunner.transform.position = owner.transform.position +
                (owner.transform.forward * 10f) +
                (owner.transform.right * 10f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            int healthBefore = targetRunner.Health.CurrentHealth;
            int impactBefore = pressure.PhysicalImpactCount;
            int impulseBefore = cargo.AttackImpulseCount;
            Vector3 swingStart = chairBody.worldCenterOfMass;
            Vector3 lastPosition = swingStart;
            float maximumStep = 0f;
            float maximumSideExcursion = 0f;
            Vector3 attackAim = targetRunner.GetComponent<Collider>().bounds.center;
            Assert.That(cargo.IsMediumRearReady, Is.True);
            Assert.That(cargo.IsStrapWeaponSelected, Is.True);
            bool resolvedTarget = pressure.TryResolveCargoStrapTarget(
                attackAim,
                5.8f,
                out ShowcaseRunnerAgent resolvedRunner,
                out _,
                out bool beyondRange);
            Assert.That(resolvedTarget, Is.True,
                $"target resolve failed; beyond={beyondRange}");
            Assert.That(resolvedRunner, Is.SameAs(targetRunner));
            Assert.That(cargo.TryBeginAttack(attackAim), Is.True,
                $"{cargo.LastMediumAttackRejection}; " +
                $"max={cargo.MediumMaximumAttackRange:0.000}");
            Assert.That(cargo.AnimationCue,
                Is.EqualTo(ShowcaseCargoStrapAnimationCue.AttackLeft));
            Assert.That(cargo.PresentationSnapshot.Tension01,
                Is.EqualTo(1f).Within(0.001f));
            targetRunner.transform.position += owner.transform.forward * 0.3f;
            Physics.SyncTransforms();

            float impactDeadline = Time.time + 4.5f;
            while (targetRunner.Health.CurrentHealth == healthBefore &&
                   Time.time < impactDeadline)
            {
                yield return new WaitForFixedUpdate();
                Vector3 current = chairBody.worldCenterOfMass;
                maximumStep = Mathf.Max(maximumStep,
                    Vector3.Distance(lastPosition, current));
                maximumSideExcursion = Mathf.Max(
                    maximumSideExcursion,
                    Mathf.Abs(Vector3.Dot(
                        current - owner.transform.position,
                        owner.transform.right)));
                lastPosition = current;
                Assert.That(cargo.CurrentStrapLength,
                    Is.LessThanOrEqualTo(cargo.MaximumStrapLength + 0.1f));
            }

            float useCommitDeadline = Time.time + 0.5f;
            while (chair.CommittedPlayerUses == 0 &&
                   Time.time < useCommitDeadline)
            {
                yield return null;
            }

            Assert.That(cargo.AttackImpulseCount, Is.EqualTo(impulseBefore + 1));
            Assert.That(targetRunner.Health.CurrentHealth,
                Is.EqualTo(healthBefore - 25));
            Assert.That(targetRunner.AnimationCue,
                Is.EqualTo(ShowcaseRunnerAnimationCue.HitBody));
            Assert.That(pressure.PhysicalImpactCount,
                Is.EqualTo(impactBefore + 1));
            Assert.That(hitFeedback.PlayCount, Is.EqualTo(1));
            Assert.That(hitFeedback.LastLevel,
                Is.EqualTo(ShowcaseHitVfxTier.Medium));
            pressure.RegisterPhysicalImpact(targetRunner, chair, 6f);
            pressure.RegisterPhysicalImpact(targetRunner, chair, 6f);
            Assert.That(targetRunner.Health.CurrentHealth,
                Is.EqualTo(healthBefore - 25),
                "The same swing sequence must not damage one Runner twice.");
            Assert.That(hitFeedback.PlayCount, Is.EqualTo(1),
                "Rejected duplicate swing contacts must not replay hit feedback.");
            Assert.That(chair.CommittedPlayerUses, Is.EqualTo(1));
            Assert.That(cargo.MediumSwingStepCount, Is.GreaterThan(5));
            float targetCorrection = Vector3.Distance(
                cargo.MediumSwingSnapshotTargetPoint,
                cargo.MediumSwingCorrectedTargetPoint);
            Assert.That(targetCorrection, Is.InRange(0.05f, 0.61f));
            Assert.That(maximumSideExcursion, Is.GreaterThan(0.75f));
            Assert.That(maximumStep, Is.LessThan(0.5f));

            float recoveryDeadline = Time.time + 4f;
            while ((cargo.Phase != ShowcaseCargoStrapPhase.Tensioned ||
                    !cargo.IsMediumRearReady) &&
                   Time.time < recoveryDeadline)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Tensioned));
            Assert.That(cargo.IsMediumRearReady, Is.True);
            Assert.That(cargo.IsMediumFloorSupported, Is.True);
            rearOffset = Vector3.ProjectOnPlane(
                chairBody.worldCenterOfMass - owner.transform.position,
                Vector3.up);
            Assert.That(Vector3.Dot(
                    rearOffset.normalized,
                    owner.transform.forward),
                Is.LessThan(-0.5f));
            Assert.That(Mathf.Abs(chairCollider.bounds.min.y),
                Is.LessThan(0.2f));
            float rearSettlePeakSpeed = 0f;
            float rearSettlePeakStep = 0f;
            Vector3 rearSettlePrevious = chairBody.worldCenterOfMass;
            for (int index = 0; index < 20; index++)
            {
                yield return new WaitForFixedUpdate();
                Vector3 currentRear = chairBody.worldCenterOfMass;
                rearSettlePeakSpeed = Mathf.Max(
                    rearSettlePeakSpeed,
                    chairBody.linearVelocity.magnitude);
                rearSettlePeakStep = Mathf.Max(
                    rearSettlePeakStep,
                    Vector3.Distance(rearSettlePrevious, currentRear));
                rearSettlePrevious = currentRear;
            }
            Assert.That(rearSettlePeakSpeed, Is.LessThan(1.5f));
            Assert.That(rearSettlePeakStep, Is.LessThan(0.08f));
            Assert.That(owner.StrapTarget, Is.SameAs(chair));
            Assert.That(owner.WeaponSelection,
                Is.EqualTo(PrototypeWeaponSelection.ObjectWeapon));
            Assert.That(cargo.TryExplicitRelease(), Is.True);
            float releaseDeadline = Time.time + 0.4f;
            while (cargo.Phase != ShowcaseCargoStrapPhase.Stored &&
                   Time.time < releaseDeadline)
            {
                yield return null;
            }

            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Stored));
            Assert.That(owner.StrapState, Is.EqualTo(PrototypeStrapOwnershipState.Stored));
            yield return null;
            Assert.That(presentation.StrapLine.enabled, Is.False);
        }

        [UnityTest]
        public IEnumerator MediumSwing_NearMidMaxLeftRightActualCollisionMatrix()
        {
            float[] ranges = { 1.6f, 3.4f, 5.55f };
            string[] rangeNames = { "NEAR", "MID", "MAX" };
            float[] sides = { -0.6f, 0.6f };
            for (int rangeIndex = 0; rangeIndex < ranges.Length; rangeIndex++)
            {
                for (int sideIndex = 0; sideIndex < sides.Length; sideIndex++)
                {
                    yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                        S7ScenePath,
                        new LoadSceneParameters(LoadSceneMode.Single));
                    ShowcaseCargoStrapController cargo =
                        SceneComponents<ShowcaseCargoStrapController>().Single();
                    PrototypeInteractionStateOwner owner =
                        SceneComponents<PrototypeInteractionStateOwner>().Single();
                    ShowcaseRunnerPressureController pressure =
                        SceneComponents<ShowcaseRunnerPressureController>().Single();
                    PrototypeInteractionTarget chair =
                        SceneComponents<PrototypeInteractionTarget>()
                            .First(target => target.Role ==
                                PrototypeInteractionRole.DirectChair);
                    foreach (PrototypeInteractionTarget obstacle in
                             SceneComponents<PrototypeInteractionTarget>())
                    {
                        if (obstacle != chair)
                        {
                            obstacle.transform.position +=
                                new Vector3(40f, 0f, 40f);
                        }
                    }
                    Rigidbody chairBody = chair.GetComponent<Rigidbody>();
                    ShowcaseRunnerAgent targetRunner = pressure.Runners
                        .Single(runner => !runner.IsElite);
                    ShowcaseRunnerAgent otherRunner = pressure.Runners
                        .Single(runner => runner.IsElite);
                    float runnerY = targetRunner.transform.position.y;

                    owner.transform.position = chair.transform.position +
                        new Vector3(0f, 0.9f, -2f);
                    targetRunner.transform.position = owner.transform.position +
                        new Vector3(12f, 0f, 12f);
                    otherRunner.transform.position = owner.transform.position +
                        new Vector3(-12f, 0f, 12f);
                    Physics.SyncTransforms();
                    yield return new WaitForFixedUpdate();
                    Assert.That(cargo.TryBeginDirectAttach(chair), Is.True);
                    CompleteAttachSequence(cargo);
                    float rearDeadline = Time.time + 2.5f;
                    while (!cargo.IsMediumRearReady &&
                           Time.time < rearDeadline)
                    {
                        yield return new WaitForFixedUpdate();
                    }
                    string label = $"{rangeNames[rangeIndex]}_" +
                        (sides[sideIndex] < 0f ? "LEFT" : "RIGHT");
                    Assert.That(cargo.IsMediumRearReady, Is.True, label);
                    Assert.That(cargo.IsMediumFloorSupported, Is.True, label);

                    targetRunner.enabled = false;
                    otherRunner.enabled = false;
                    float range = ranges[rangeIndex];
                    float side = sides[sideIndex];
                    float forwardDistance = Mathf.Sqrt(
                        Mathf.Max(0.1f, (range * range) - (side * side)));
                    Vector3 targetPosition = owner.transform.position +
                        (owner.transform.forward * forwardDistance) +
                        (owner.transform.right * side);
                    targetRunner.transform.position = new Vector3(
                        targetPosition.x,
                        runnerY,
                        targetPosition.z);
                    otherRunner.transform.position = owner.transform.position +
                        (owner.transform.forward * 10f) -
                        (owner.transform.right * 10f);
                    Physics.SyncTransforms();
                    yield return new WaitForFixedUpdate();

                    int healthBefore = targetRunner.Health.CurrentHealth;
                    int impactBefore = pressure.PhysicalImpactCount;
                    Vector3 aimPoint = targetRunner.GetComponent<Collider>()
                        .bounds.center;
                    Assert.That(cargo.TryBeginAttack(aimPoint), Is.True,
                        $"{label}: {cargo.LastMediumAttackRejection}");
                    ShowcaseCargoStrapAnimationCue expectedCue = side < 0f
                        ? ShowcaseCargoStrapAnimationCue.AttackLeft
                        : ShowcaseCargoStrapAnimationCue.AttackRight;
                    Assert.That(cargo.AnimationCue, Is.EqualTo(expectedCue), label);
                    float targetPlanarDistance = Vector3.ProjectOnPlane(
                        aimPoint - cargo.PresentationSnapshot.ReelWorldPosition,
                        Vector3.up).magnitude;
                    float expectedDesiredLength = Mathf.Clamp(
                        targetPlanarDistance + 0.55f,
                        1.25f,
                        cargo.MaximumStrapLength - 0.05f);
                    Assert.That(cargo.MediumDesiredTetherLength,
                        Is.EqualTo(expectedDesiredLength).Within(0.1f), label);

                    Vector3 previous = chairBody.worldCenterOfMass;
                    float maximumStep = 0f;
                    float maximumLength = 0f;
                    float previousEffectiveLength =
                        cargo.MediumEffectiveTetherLength;
                    float hitDeadline = Time.time + 4.5f;
                    while (targetRunner.Health.CurrentHealth == healthBefore &&
                           Time.time < hitDeadline)
                    {
                        yield return new WaitForFixedUpdate();
                        Vector3 current = chairBody.worldCenterOfMass;
                        maximumStep = Mathf.Max(
                            maximumStep,
                            Vector3.Distance(previous, current));
                        maximumLength = Mathf.Max(
                            maximumLength,
                            cargo.CurrentStrapLength);
                        if (cargo.ActiveMediumSwingSequence != 0)
                        {
                            Assert.That(cargo.CurrentStrapLength,
                                Is.LessThanOrEqualTo(
                                    cargo.MediumEffectiveTetherLength + 0.45f),
                                $"{label}: effective tether constraint");
                        }
                        if (targetRunner.Health.CurrentHealth == healthBefore &&
                            cargo.ActiveMediumSwingSequence != 0)
                        {
                            Assert.That(
                                cargo.MediumEffectiveTetherLength + 0.001f,
                                Is.GreaterThanOrEqualTo(previousEffectiveLength),
                                $"{label}: tether length jumped backward");
                            previousEffectiveLength =
                                cargo.MediumEffectiveTetherLength;
                        }
                        previous = current;
                    }

                    Assert.That(targetRunner.Health.CurrentHealth,
                        Is.EqualTo(healthBefore - 25), label);
                    Assert.That(pressure.PhysicalImpactCount,
                        Is.EqualTo(impactBefore + 1), label);
                    Assert.That(maximumLength,
                        Is.LessThanOrEqualTo(cargo.MaximumStrapLength + 0.1f),
                        label);
                    Assert.That(maximumStep, Is.LessThan(0.5f), label);
                    yield return new WaitForSeconds(0.25f);
                    Assert.That(targetRunner.Health.AcceptedDamageCount,
                        Is.EqualTo(1), $"{label}: duplicate victim hit");

                    float recoveryDeadline = Time.time + 4f;
                    while ((cargo.Phase != ShowcaseCargoStrapPhase.Tensioned ||
                            !cargo.IsMediumRearReady) &&
                           Time.time < recoveryDeadline)
                    {
                        yield return new WaitForFixedUpdate();
                    }
                    Assert.That(cargo.IsMediumRearReady, Is.True, label);
                    Assert.That(cargo.IsMediumFloorSupported, Is.True, label);
                }
            }

            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S7ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseCargoStrapController rejectCargo =
                SceneComponents<ShowcaseCargoStrapController>().Single();
            PrototypeInteractionStateOwner rejectOwner =
                SceneComponents<PrototypeInteractionStateOwner>().Single();
            ShowcaseRunnerPressureController rejectPressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            PrototypeInteractionTarget rejectChair =
                SceneComponents<PrototypeInteractionTarget>()
                    .First(target => target.Role ==
                        PrototypeInteractionRole.DirectChair);
            foreach (PrototypeInteractionTarget obstacle in
                     SceneComponents<PrototypeInteractionTarget>())
            {
                if (obstacle != rejectChair)
                {
                    obstacle.transform.position += new Vector3(40f, 0f, 40f);
                }
            }
            ShowcaseRunnerAgent rejectRunner = rejectPressure.Runners
                .Single(runner => !runner.IsElite);
            rejectRunner.enabled = false;
            rejectOwner.transform.position = rejectChair.transform.position +
                new Vector3(0f, 0.9f, -2f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            Assert.That(rejectCargo.TryBeginDirectAttach(rejectChair), Is.True);
            CompleteAttachSequence(rejectCargo);
            float rejectRearDeadline = Time.time + 2.5f;
            while (!rejectCargo.IsMediumRearReady &&
                   Time.time < rejectRearDeadline)
            {
                yield return new WaitForFixedUpdate();
            }
            float rejectY = rejectRunner.transform.position.y;
            Vector3 rejectPosition = rejectOwner.transform.position +
                (rejectOwner.transform.forward *
                 (rejectCargo.MediumMaximumAttackRange + 0.4f));
            rejectRunner.transform.position = new Vector3(
                rejectPosition.x,
                rejectY,
                rejectPosition.z);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            Vector3 velocityBefore = rejectChair.GetComponent<Rigidbody>()
                .linearVelocity;
            int usesBefore = rejectChair.CommittedPlayerUses;
            Assert.That(rejectCargo.TryBeginAttack(
                    rejectRunner.GetComponent<Collider>().bounds.center),
                Is.False);
            Assert.That(rejectCargo.Phase,
                Is.EqualTo(ShowcaseCargoStrapPhase.Tensioned));
            Assert.That(rejectChair.CommittedPlayerUses, Is.EqualTo(usesBefore));
            Assert.That(Vector3.Distance(
                    velocityBefore,
                    rejectChair.GetComponent<Rigidbody>().linearVelocity),
                Is.LessThan(0.1f));

            Vector3 validMissPosition = rejectOwner.transform.position +
                (rejectOwner.transform.forward * 3.2f);
            rejectRunner.transform.position = new Vector3(
                validMissPosition.x,
                rejectY,
                validMissPosition.z);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            int missHealthBefore = rejectRunner.Health.CurrentHealth;
            Assert.That(rejectCargo.TryBeginAttack(
                    rejectRunner.GetComponent<Collider>().bounds.center),
                Is.True,
                rejectCargo.LastMediumAttackRejection);
            rejectRunner.transform.position += rejectOwner.transform.right * 4f;
            Physics.SyncTransforms();
            float maximumCorrection = 0f;
            float missDeadline = Time.time + 5f;
            while ((rejectCargo.Phase != ShowcaseCargoStrapPhase.Tensioned ||
                    !rejectCargo.IsMediumRearReady) &&
                   Time.time < missDeadline)
            {
                yield return new WaitForFixedUpdate();
                maximumCorrection = Mathf.Max(
                    maximumCorrection,
                    Vector3.Distance(
                        rejectCargo.MediumSwingSnapshotTargetPoint,
                        rejectCargo.MediumSwingCorrectedTargetPoint));
            }
            Assert.That(rejectRunner.Health.CurrentHealth,
                Is.EqualTo(missHealthBefore));
            Assert.That(maximumCorrection, Is.LessThanOrEqualTo(0.61f));
            Assert.That(rejectCargo.IsMediumRearReady, Is.True);
        }

        [UnityTest]
        public IEnumerator MediumSwing_LastThirtyFivePercentBuffersExactlyOnce()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S7ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseCargoStrapController cargo =
                SceneComponents<ShowcaseCargoStrapController>().Single();
            PrototypeInteractionStateOwner owner =
                SceneComponents<PrototypeInteractionStateOwner>().Single();
            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            PrototypeInteractionTarget chair =
                SceneComponents<PrototypeInteractionTarget>()
                    .First(target => target.Role ==
                        PrototypeInteractionRole.DirectChair);
            ShowcaseRunnerAgent target = pressure.Runners
                .Single(runner => !runner.IsElite);
            ShowcaseRunnerAgent other = pressure.Runners
                .Single(runner => runner.IsElite);
            float runnerY = target.transform.position.y;

            owner.transform.position = chair.transform.position +
                new Vector3(0f, 0.9f, -2f);
            target.transform.position = owner.transform.position +
                new Vector3(12f, 0f, 12f);
            other.transform.position = owner.transform.position +
                new Vector3(-12f, 0f, 12f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            Assert.That(cargo.TryBeginDirectAttach(chair), Is.True);
            CompleteAttachSequence(cargo);
            float rearDeadline = Time.time + 2.5f;
            while (!cargo.IsMediumRearReady && Time.time < rearDeadline)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(cargo.IsMediumRearReady, Is.True);

            target.enabled = false;
            other.enabled = false;
            Vector3 targetPosition = owner.transform.position +
                (owner.transform.forward * 3f) -
                (owner.transform.right * 0.7f);
            target.transform.position = new Vector3(
                targetPosition.x,
                runnerY,
                targetPosition.z);
            other.transform.position = owner.transform.position +
                new Vector3(12f, 0f, 12f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            Vector3 aimPoint = target.GetComponent<Collider>().bounds.center;
            Assert.That(cargo.TryBeginAttack(aimPoint), Is.True);
            uint firstRevision = cargo.AnimationCueRevision;
            float bufferDeadline = Time.time + 2.5f;
            while (!cargo.AttackBufferWindowOpen && Time.time < bufferDeadline)
            {
                yield return null;
            }
            Assert.That(cargo.AttackBufferWindowOpen, Is.True);
            Assert.That(cargo.TryBeginAttack(aimPoint), Is.True);
            Assert.That(cargo.TryBeginAttack(aimPoint), Is.False);
            Assert.That(cargo.BufferedAttackCount, Is.EqualTo(1));

            float secondAttackDeadline = Time.time + 6f;
            while ((!cargo.IsAttackActive ||
                    cargo.AnimationCueRevision == firstRevision) &&
                   Time.time < secondAttackDeadline)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(cargo.IsAttackActive, Is.True);
            Assert.That(cargo.AnimationCueRevision, Is.GreaterThan(firstRevision));
            Assert.That(cargo.TryExplicitRelease(), Is.True);
        }

        [UnityTest]
        public IEnumerator MediumAttackAndRecoveryAllowSprintMoveBlockJumpAndKeepTargetFacing()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S7ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseCargoStrapController cargo =
                SceneComponents<ShowcaseCargoStrapController>().Single();
            PrototypeInteractionStateOwner owner =
                SceneComponents<PrototypeInteractionStateOwner>().Single();
            InputSystemLogicalInputAdapter input =
                SceneComponents<InputSystemLogicalInputAdapter>().Single();
            ThirdPersonLocomotion locomotion =
                owner.GetComponent<ThirdPersonLocomotion>();
            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            PrototypeInteractionTarget chair =
                SceneComponents<PrototypeInteractionTarget>()
                    .First(target => target.Role ==
                        PrototypeInteractionRole.DirectChair);
            Rigidbody chairBody = chair.GetComponent<Rigidbody>();
            ShowcaseRunnerAgent target = pressure.Runners
                .Single(runner => !runner.IsElite);
            ShowcaseRunnerAgent other = pressure.Runners
                .Single(runner => runner.IsElite);
            float runnerY = target.transform.position.y;

            owner.transform.position = chair.transform.position +
                new Vector3(0f, 0.9f, -2f);
            target.transform.position = owner.transform.position +
                new Vector3(12f, 0f, 12f);
            other.transform.position = owner.transform.position +
                new Vector3(-12f, 0f, 12f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            Assert.That(cargo.TryBeginDirectAttach(chair), Is.True);
            CompleteAttachSequence(cargo);
            float rearDeadline = Time.time + 2.5f;
            while (!cargo.IsMediumRearReady && Time.time < rearDeadline)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(cargo.IsMediumRearReady, Is.True);
            Assert.That(cargo.IsMediumFloorSupported, Is.True);

            target.enabled = false;
            other.enabled = false;
            Vector3 targetPosition = owner.transform.position +
                (owner.transform.forward * 4.6f) -
                (owner.transform.right * 0.8f);
            target.transform.position = new Vector3(
                targetPosition.x,
                runnerY,
                targetPosition.z);
            other.transform.position = owner.transform.position +
                new Vector3(12f, 0f, 12f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            Vector3 aimPoint = target.GetComponent<Collider>().bounds.center;
            int healthBefore = target.Health.CurrentHealth;
            float playerStartY = owner.transform.position.y;
            Vector3 playerStart = owner.transform.position;
            Vector3 previousPlayer = playerStart;
            float attackTravel = 0f;
            float extendingTravel = 0f;
            float recoveryTravel = 0f;
            float maximumYDelta = 0f;
            float maximumFacingError = 0f;
            bool sawAttack = false;
            bool sawExtending = false;
            bool sawRecovering = false;
            bool sawSprint = false;
            int attackMovementSteps = 0;
            int extendingMovementSteps = 0;
            int recoveryMovementSteps = 0;

            input.enabled = false;
            SetSnapshot(input, new LogicalInputSnapshot(
                Vector2.up,
                Vector2.zero,
                false,
                false,
                false,
                false,
                false,
                false,
                sprint: true,
                jump: true));
            Assert.That(cargo.TryBeginAttack(aimPoint), Is.True,
                cargo.LastMediumAttackRejection);
            Vector3 frozenTargetPoint = cargo.MediumSwingSnapshotTargetPoint;

            float deadline = Time.time + 8f;
            while ((cargo.Phase != ShowcaseCargoStrapPhase.Tensioned ||
                    !cargo.IsMediumRearReady) &&
                   Time.time < deadline)
            {
                yield return new WaitForFixedUpdate();
                Vector3 currentPlayer = owner.transform.position;
                float planarStep = Vector3.ProjectOnPlane(
                    currentPlayer - previousPlayer,
                    Vector3.up).magnitude;
                if (cargo.IsAttackActive)
                {
                    sawAttack = true;
                    attackTravel += planarStep;
                    attackMovementSteps++;
                    if (attackMovementSteps == 4)
                    {
                        SetSnapshot(input, default);
                    }
                }
                else if (cargo.Phase == ShowcaseCargoStrapPhase.MediumExtending)
                {
                    sawExtending = true;
                    extendingTravel += planarStep;
                    extendingMovementSteps++;
                    if (extendingMovementSteps == 1)
                    {
                        SetSnapshot(input, new LogicalInputSnapshot(
                            Vector2.right,
                            Vector2.zero,
                            false,
                            false,
                            false,
                            false,
                            false,
                            false,
                            sprint: true,
                            jump: true));
                    }
                    else if (extendingMovementSteps == 5)
                    {
                        SetSnapshot(input, default);
                    }
                }
                else if (cargo.Phase == ShowcaseCargoStrapPhase.MediumRecovering)
                {
                    sawRecovering = true;
                    recoveryTravel += planarStep;
                    recoveryMovementSteps++;
                    if (recoveryMovementSteps == 1)
                    {
                        SetSnapshot(input, new LogicalInputSnapshot(
                            Vector2.down,
                            Vector2.zero,
                            false,
                            false,
                            false,
                            false,
                            false,
                            false,
                            sprint: true,
                            jump: true));
                    }
                    else if (recoveryMovementSteps == 10)
                    {
                        SetSnapshot(input, default);
                    }
                }
                if (cargo.IsMediumAttackMobilityPhase)
                {
                    sawSprint |= locomotion.IsSprinting;
                    Vector3 toSnapshot = Vector3.ProjectOnPlane(
                        frozenTargetPoint - owner.transform.position,
                        Vector3.up);
                    if (toSnapshot.sqrMagnitude > 0.001f)
                    {
                        maximumFacingError = Mathf.Max(
                            maximumFacingError,
                            Vector3.Angle(owner.transform.forward,
                                toSnapshot.normalized));
                    }
                }
                maximumYDelta = Mathf.Max(
                    maximumYDelta,
                    Mathf.Abs(currentPlayer.y - playerStartY));
                previousPlayer = currentPlayer;
                Assert.That(cargo.CurrentStrapLength,
                    Is.LessThanOrEqualTo(cargo.MaximumStrapLength + 0.1f));
            }
            SetSnapshot(input, default);

            Assert.That(target.Health.CurrentHealth,
                Is.EqualTo(healthBefore - 25));
            Assert.That(sawAttack, Is.True);
            Assert.That(sawExtending, Is.True);
            Assert.That(sawRecovering, Is.True);
            Assert.That(sawSprint, Is.True);
            Assert.That(attackTravel, Is.GreaterThan(0.05f));
            Assert.That(extendingTravel, Is.GreaterThan(0.01f));
            Assert.That(recoveryTravel, Is.GreaterThan(0.05f));
            Assert.That(Vector3.ProjectOnPlane(
                    owner.transform.position - playerStart,
                    Vector3.up).magnitude,
                Is.GreaterThan(0.5f));
            Assert.That(maximumYDelta, Is.LessThan(0.15f),
                "Jump must remain suppressed during Medium attack mobility phases.");
            Assert.That(maximumFacingError, Is.LessThan(35f));
            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Tensioned));
            Assert.That(cargo.IsMediumRearReady, Is.True);
            Assert.That(cargo.IsMediumFloorSupported, Is.True);
            Vector3 rearOffset = Vector3.ProjectOnPlane(
                chairBody.worldCenterOfMass - owner.transform.position,
                Vector3.up);
            Assert.That(Vector3.Dot(
                    rearOffset.normalized,
                    owner.transform.forward),
                Is.LessThan(-0.5f));

        }

        [UnityTest]
        public IEnumerator MediumMovementBeyondSnapshotRangeRecoversWithoutDamageOrUse()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S7ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseCargoStrapController cargo =
                SceneComponents<ShowcaseCargoStrapController>().Single();
            PrototypeInteractionStateOwner owner =
                SceneComponents<PrototypeInteractionStateOwner>().Single();
            InputSystemLogicalInputAdapter input =
                SceneComponents<InputSystemLogicalInputAdapter>().Single();
            ThirdPersonLocomotion locomotion =
                owner.GetComponent<ThirdPersonLocomotion>();
            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            PrototypeInteractionTarget chair =
                SceneComponents<PrototypeInteractionTarget>()
                    .First(target => target.Role ==
                        PrototypeInteractionRole.DirectChair);
            Rigidbody chairBody = chair.GetComponent<Rigidbody>();
            ShowcaseRunnerAgent target = pressure.Runners
                .Single(runner => !runner.IsElite);
            ShowcaseRunnerAgent other = pressure.Runners
                .Single(runner => runner.IsElite);
            float runnerY = target.transform.position.y;

            owner.transform.position = chair.transform.position +
                new Vector3(0f, 0.9f, -2f);
            target.transform.position = owner.transform.position +
                new Vector3(12f, 0f, 12f);
            other.transform.position = owner.transform.position +
                new Vector3(-12f, 0f, 12f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            Assert.That(cargo.TryBeginDirectAttach(chair), Is.True);
            CompleteAttachSequence(cargo);
            float rearDeadline = Time.time + 2.5f;
            while (!cargo.IsMediumRearReady && Time.time < rearDeadline)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(cargo.IsMediumRearReady, Is.True);

            target.enabled = false;
            other.enabled = false;
            Vector3 targetPosition = owner.transform.position +
                (owner.transform.forward *
                 (cargo.MediumMaximumAttackRange - 0.1f));
            target.transform.position = new Vector3(
                targetPosition.x,
                runnerY,
                targetPosition.z);
            other.transform.position = owner.transform.position +
                new Vector3(12f, 0f, 12f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            int healthBefore = target.Health.CurrentHealth;
            int usesBefore = chair.CommittedPlayerUses;
            float startY = owner.transform.position.y;
            Vector3 moveStart = owner.transform.position;
            bool sawSprint = false;
            bool sawRecovering = false;
            float recoveryTravel = 0f;
            Vector3 previous = owner.transform.position;
            input.enabled = false;
            SetSnapshot(input, new LogicalInputSnapshot(
                Vector2.down,
                Vector2.zero,
                false,
                false,
                false,
                false,
                false,
                false,
                sprint: true,
                jump: true));
            Vector3 aimPoint = target.GetComponent<Collider>().bounds.center;
            Assert.That(cargo.TryBeginAttack(aimPoint), Is.True,
                cargo.LastMediumAttackRejection);

            float recoverDeadline = Time.time + 5f;
            while (!sawRecovering && Time.time < recoverDeadline)
            {
                yield return new WaitForFixedUpdate();
                sawSprint |= locomotion.IsSprinting;
                sawRecovering = cargo.Phase ==
                    ShowcaseCargoStrapPhase.MediumRecovering;
                Assert.That(cargo.IsMediumAttackMobilityPhase, Is.True);
                Assert.That(Mathf.Abs(owner.transform.position.y - startY),
                    Is.LessThan(0.15f));
            }
            Assert.That(sawRecovering, Is.True,
                "Moving the reel beyond the frozen target range must enter Recover.");
            for (int index = 0; index < 10; index++)
            {
                yield return new WaitForFixedUpdate();
                Vector3 current = owner.transform.position;
                recoveryTravel += Vector3.ProjectOnPlane(
                    current - previous,
                    Vector3.up).magnitude;
                previous = current;
                sawSprint |= locomotion.IsSprinting;
            }
            SetSnapshot(input, default);

            Assert.That(target.Health.CurrentHealth, Is.EqualTo(healthBefore));
            Assert.That(chair.CommittedPlayerUses, Is.EqualTo(usesBefore));
            Assert.That(sawSprint, Is.True);
            Assert.That(Vector3.ProjectOnPlane(
                    owner.transform.position - moveStart,
                    Vector3.up).magnitude,
                Is.GreaterThan(0.5f));
            Assert.That(recoveryTravel, Is.GreaterThan(0.05f));

            float readyDeadline = Time.time + 4f;
            while ((cargo.Phase != ShowcaseCargoStrapPhase.Tensioned ||
                    !cargo.IsMediumRearReady) &&
                   Time.time < readyDeadline)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Tensioned));
            Assert.That(cargo.IsMediumRearReady, Is.True);
            Assert.That(cargo.IsMediumFloorSupported, Is.True);
            Vector3 rearOffset = Vector3.ProjectOnPlane(
                chairBody.worldCenterOfMass - owner.transform.position,
                Vector3.up);
            Assert.That(Vector3.Dot(
                    rearOffset.normalized,
                    owner.transform.forward),
                Is.LessThan(-0.5f));
        }

        [UnityTest]
        public IEnumerator MediumLateTargetEscapeAfterCorrectionWindowRecoversWithoutHit()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S7ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseCargoStrapController cargo =
                SceneComponents<ShowcaseCargoStrapController>().Single();
            PrototypeInteractionStateOwner owner =
                SceneComponents<PrototypeInteractionStateOwner>().Single();
            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            PrototypeInteractionTarget chair =
                SceneComponents<PrototypeInteractionTarget>()
                    .First(target => target.Role ==
                        PrototypeInteractionRole.DirectChair);
            foreach (PrototypeInteractionTarget obstacle in
                     SceneComponents<PrototypeInteractionTarget>())
            {
                if (obstacle != chair)
                {
                    obstacle.transform.position += new Vector3(40f, 0f, 40f);
                }
            }
            ShowcaseRunnerAgent targetRunner = pressure.Runners
                .Single(runner => !runner.IsElite);
            ShowcaseRunnerAgent otherRunner = pressure.Runners
                .Single(runner => runner.IsElite);
            float runnerY = targetRunner.transform.position.y;

            owner.transform.position = chair.transform.position +
                new Vector3(0f, 0.9f, -2f);
            targetRunner.transform.position = owner.transform.position +
                new Vector3(12f, 0f, 12f);
            otherRunner.transform.position = owner.transform.position +
                new Vector3(-12f, 0f, 12f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            Assert.That(cargo.TryBeginDirectAttach(chair), Is.True);
            CompleteAttachSequence(cargo);
            float rearDeadline = Time.time + 2.5f;
            while (!cargo.IsMediumRearReady && Time.time < rearDeadline)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(cargo.IsMediumRearReady, Is.True);

            targetRunner.enabled = false;
            otherRunner.enabled = false;
            Vector3 targetPosition = owner.transform.position +
                (owner.transform.forward *
                 (cargo.MediumMaximumAttackRange - 0.5f));
            targetRunner.transform.position = new Vector3(
                targetPosition.x,
                runnerY,
                targetPosition.z);
            otherRunner.transform.position = owner.transform.position +
                new Vector3(12f, 0f, 12f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            Collider targetCollider = targetRunner.GetComponent<Collider>();
            int healthBefore = targetRunner.Health.CurrentHealth;
            int usesBefore = chair.CommittedPlayerUses;
            Assert.That(cargo.TryBeginAttack(targetCollider.bounds.center), Is.True,
                cargo.LastMediumAttackRejection);
            targetCollider.isTrigger = true;
            float extensionDeadline = Time.time + 3f;
            while (cargo.Phase != ShowcaseCargoStrapPhase.MediumExtending &&
                   Time.time < extensionDeadline)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(cargo.Phase,
                Is.EqualTo(ShowcaseCargoStrapPhase.MediumExtending));

            Vector3 escapedPosition = owner.transform.position +
                (owner.transform.forward *
                 (cargo.MediumMaximumAttackRange + 1f));
            targetRunner.transform.position = new Vector3(
                escapedPosition.x,
                runnerY,
                escapedPosition.z);
            Physics.SyncTransforms();
            float recoverDeadline = Time.time + 1f;
            while (cargo.Phase != ShowcaseCargoStrapPhase.MediumRecovering &&
                   Time.time < recoverDeadline)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(cargo.Phase,
                Is.EqualTo(ShowcaseCargoStrapPhase.MediumRecovering));
            Assert.That(targetRunner.Health.CurrentHealth, Is.EqualTo(healthBefore));
            Assert.That(chair.CommittedPlayerUses, Is.EqualTo(usesBefore));
        }

        [UnityTest]
        public IEnumerator SmallOverheadObstructionPausesResumesAndPreservesExactThrowUse()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S7ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseCargoStrapController cargo =
                SceneComponents<ShowcaseCargoStrapController>().Single();
            ShowcaseCargoStrapPresentation presentation =
                SceneComponents<ShowcaseCargoStrapPresentation>().Single();
            PrototypeInteractionStateOwner owner =
                SceneComponents<PrototypeInteractionStateOwner>().Single();
            PrototypeInteractionTarget binder =
                SceneComponents<PrototypeInteractionTarget>()
                    .First(target => target.Role ==
                        PrototypeInteractionRole.DirectBinder);
            Rigidbody body = binder.GetComponent<Rigidbody>();
            Collider binderCollider = binder.GetComponent<Collider>();
            Animator humanoid = cargo.GetComponentsInChildren<Animator>(true)
                .First(candidate => candidate.isHuman);
            Transform head = humanoid.GetBoneTransform(HumanBodyBones.Head);

            owner.transform.position = binder.transform.position +
                new Vector3(0f, 0.9f, -2f);
            Physics.SyncTransforms();
            float requiredCenterY = head.position.y + 0.2f +
                binderCollider.bounds.extents.y + 0.15f;
            float ceilingHalfHeight = 0.1f;
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "SMALL_OVERHEAD_TEST_LOW_CEILING";
            ceiling.transform.position = new Vector3(
                head.position.x,
                requiredCenterY + binderCollider.bounds.extents.y +
                    0.01f + ceilingHalfHeight,
                head.position.z);
            ceiling.transform.localScale = new Vector3(3f, 0.2f, 3f);
            Collider ceilingCollider = ceiling.GetComponent<Collider>();

            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            Assert.That(cargo.TryBeginDirectAttach(binder), Is.True);

            float attachDeadline = Time.time + 5f;
            while (cargo.Phase != ShowcaseCargoStrapPhase.SmallSpinning &&
                   Time.time < attachDeadline)
            {
                yield return null;
            }
            Assert.That(cargo.Phase,
                Is.EqualTo(ShowcaseCargoStrapPhase.SmallSpinning));
            float blockedDeadline = Time.time + 1.5f;
            while (!cargo.IsSmallSpinObstructed && Time.time < blockedDeadline)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(cargo.IsSmallSpinObstructed, Is.True);
            yield return null;
            for (int pointIndex = 0;
                 pointIndex < presentation.ObjectTetherLine.positionCount;
                 pointIndex++)
            {
                Assert.That(
                    presentation.ObjectTetherLine.GetPosition(pointIndex).y,
                    Is.LessThan(ceilingCollider.bounds.min.y + 0.01f),
                    "A blocked Small tether route must remain below the ceiling.");
            }
            Assert.That(binder.CommittedPlayerUses, Is.Zero);
            Vector3 heldStart = body.worldCenterOfMass;
            float maximumBlockedStep = 0f;
            Vector3 previous = heldStart;
            for (int index = 0; index < 20; index++)
            {
                yield return new WaitForFixedUpdate();
                Vector3 current = body.worldCenterOfMass;
                maximumBlockedStep = Mathf.Max(
                    maximumBlockedStep,
                    Vector3.Distance(previous, current));
                previous = current;
                Assert.That(Physics.ComputePenetration(
                        binderCollider,
                        binderCollider.transform.position,
                        binderCollider.transform.rotation,
                        ceilingCollider,
                        ceilingCollider.transform.position,
                        ceilingCollider.transform.rotation,
                        out _,
                        out _),
                    Is.False,
                    "Obstructed overhead orbit must pause before penetrating the ceiling. " +
                    $"body={binderCollider.bounds}; ceiling={ceilingCollider.bounds}; " +
                    $"velocity={body.linearVelocity}; center={cargo.SmallSpinCenterWorld}; " +
                    $"clearance={cargo.SmallSpinVerticalClearance:0.000}");
                Assert.That(binder.CommittedPlayerUses, Is.Zero);
            }
            Assert.That(maximumBlockedStep, Is.LessThan(0.35f));

            Object.Destroy(ceiling);
            yield return null;
            Physics.SyncTransforms();
            Vector3 resumeStart = body.worldCenterOfMass;
            float resumeDeadline = Time.time + 1.5f;
            while (cargo.IsSmallSpinObstructed && Time.time < resumeDeadline)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(cargo.IsSmallSpinObstructed, Is.False);
            yield return null;
            for (int index = 0; index < 15; index++)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(Vector3.Distance(resumeStart, body.worldCenterOfMass),
                Is.GreaterThan(0.2f));
            Assert.That(binder.CommittedPlayerUses, Is.Zero);

            Vector3 throwAim = binder.transform.position +
                (owner.transform.forward * 10f) + Vector3.up;
            Assert.That(cargo.TryBeginAttack(throwAim), Is.True);
            float throwDeadline = Time.time + 1.5f;
            while (cargo.Phase != ShowcaseCargoStrapPhase.Stored &&
                   Time.time < throwDeadline)
            {
                yield return null;
            }
            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Stored));
            Assert.That(owner.StrapState, Is.EqualTo(PrototypeStrapOwnershipState.Stored));
            Assert.That(binder.CommittedPlayerUses, Is.EqualTo(1));
            Assert.That(body.linearVelocity.magnitude, Is.GreaterThan(4f));
            float depletionDeadline = Time.time + 2f;
            while (binder.gameObject.activeSelf && Time.time < depletionDeadline)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(binder.gameObject.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator ActualHumanoidSmallRollingAndThrowReleaseDrivePhysicalBinder()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S7ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseCargoStrapController cargo =
                SceneComponents<ShowcaseCargoStrapController>().Single();
            PrototypeInteractionStateOwner owner =
                SceneComponents<PrototypeInteractionStateOwner>().Single();
            ShowcaseCargoStrapPresentation presentation =
                SceneComponents<ShowcaseCargoStrapPresentation>().Single();
            PrototypeInteractionTarget binder = SceneComponents<PrototypeInteractionTarget>()
                .First(target => target.Role == PrototypeInteractionRole.DirectBinder);
            Rigidbody body = binder.GetComponent<Rigidbody>();
            Collider binderCollider = binder.GetComponent<Collider>();
            Collider playerCollider = owner.GetComponent<Collider>();
            ThirdPersonLocomotion locomotion =
                owner.GetComponent<ThirdPersonLocomotion>();
            Animator humanoid = cargo.GetComponentsInChildren<Animator>(true)
                .First(candidate => candidate.isHuman);
            Transform head = humanoid.GetBoneTransform(HumanBodyBones.Head);

            owner.transform.position = binder.transform.position +
                new Vector3(0f, 0.9f, -2f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            Assert.That(cargo.TryBeginDirectAttach(binder), Is.True);

            float attachDeadline = Time.time + 5f;
            while (cargo.Phase != ShowcaseCargoStrapPhase.SmallSpinning &&
                   Time.time < attachDeadline)
            {
                yield return null;
            }
            Assert.That(cargo.Phase,
                Is.EqualTo(ShowcaseCargoStrapPhase.SmallSpinning));
            Assert.That(cargo.AnimationCue,
                Is.EqualTo(ShowcaseCargoStrapAnimationCue.Rolling));
            Assert.That(owner.WeaponSelection,
                Is.EqualTo(PrototypeWeaponSelection.ObjectWeapon));
            Assert.That(cargo.PullDistanceReduced, Is.GreaterThan(0.35f));

            float overheadSettleDeadline = Time.time + 0.6f;
            while ((cargo.SmallSpinVerticalClearance < 0.15f ||
                    ResolvePlanarColliderClearance(
                        playerCollider,
                        binderCollider) < 0.05f) &&
                   Time.time < overheadSettleDeadline)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(cargo.SmallSpinVerticalClearance,
                Is.GreaterThanOrEqualTo(0.15f));
            Assert.That(ResolvePlanarColliderClearance(
                    playerCollider,
                    binderCollider),
                Is.GreaterThanOrEqualTo(0.05f));
            Vector3 beforeSpin = body.worldCenterOfMass;
            float minimumVerticalClearance = float.PositiveInfinity;
            float minimumBodyRadialClearance = float.PositiveInfinity;
            for (int index = 0; index < 30; index++)
            {
                yield return new WaitForFixedUpdate();
                minimumVerticalClearance = Mathf.Min(
                    minimumVerticalClearance,
                    cargo.SmallSpinVerticalClearance);
                minimumBodyRadialClearance = Mathf.Min(
                    minimumBodyRadialClearance,
                    ResolvePlanarColliderClearance(
                        playerCollider,
                        binderCollider));
                Assert.That(Physics.ComputePenetration(
                        playerCollider,
                        playerCollider.transform.position,
                        playerCollider.transform.rotation,
                        binderCollider,
                        binderCollider.transform.position,
                        binderCollider.transform.rotation,
                        out _,
                        out _),
                    Is.False,
                    "The overhead Small orbit must not intersect the player body.");
            }
            Assert.That(Vector3.Distance(beforeSpin, body.worldCenterOfMass),
                Is.GreaterThan(0.1f));
            Assert.That(Vector3.Distance(
                    body.worldCenterOfMass,
                    cargo.ResolvedRightHandTransform.position),
                Is.LessThan(1.1f));
            Assert.That(minimumVerticalClearance,
                Is.GreaterThanOrEqualTo(0.15f));
            Assert.That(minimumBodyRadialClearance,
                Is.GreaterThanOrEqualTo(0.05f));
            Assert.That(cargo.SmallSpinCenterWorld.y,
                Is.GreaterThan(head.position.y));
            Assert.That(cargo.IsSmallSpinObstructed, Is.False);
            Assert.That(Vector3.Distance(
                    presentation.ResolvedCarabinerSocket.position,
                    cargo.PresentationSnapshot.ReelWorldPosition),
                Is.LessThan(0.005f));
            Vector3 carabinerPlayerLocal = owner.transform.InverseTransformPoint(
                presentation.ResolvedCarabinerSocket.position);
            Assert.That(carabinerPlayerLocal.y, Is.InRange(-0.05f, 0.15f));
            Assert.That(Vector3.Distance(
                    presentation.LastGripWorldPosition,
                    presentation.LastPalmCenterWorldPosition),
                Is.InRange(0.025f, 0.04f));
            Assert.That(Vector3.Distance(
                    presentation.ObjectTetherLine.GetPosition(0),
                    presentation.LastPalmCenterWorldPosition),
                Is.InRange(0.025f, 0.05f));
            Vector3 tetherStart = presentation.ObjectTetherLine.GetPosition(0);
            Vector3 tetherEnd = presentation.ObjectTetherLine.GetPosition(
                presentation.ObjectTetherLine.positionCount - 1);
            Vector3 directSpan = tetherEnd - tetherStart;
            Assert.That(directSpan.magnitude, Is.GreaterThan(0.1f));
            for (int pointIndex = 1;
                 pointIndex < presentation.ObjectTetherLine.positionCount - 1;
                 pointIndex++)
            {
                Vector3 pointOffset =
                    presentation.ObjectTetherLine.GetPosition(pointIndex) -
                    tetherStart;
                float distanceFromDirectHandTether = Vector3.Cross(
                    directSpan,
                    pointOffset).magnitude / directSpan.magnitude;
                Assert.That(distanceFromDirectHandTether,
                    Is.LessThan(0.01f),
                    "Small object tether must run directly from the Hand gap " +
                    "to the Clamp without an overhead kink.");
            }

            InputSystemLogicalInputAdapter input =
                SceneComponents<InputSystemLogicalInputAdapter>().Single();
            ShowcaseCharacterAnimationDriver driver =
                cargo.GetComponent<ShowcaseCharacterAnimationDriver>();
            input.enabled = false;
            SetSnapshot(input, new LogicalInputSnapshot(
                Vector2.right,
                Vector2.zero,
                false,
                false,
                false,
                false,
                false,
                false,
                sprint: true));
            Vector3 movementStart = owner.transform.position;
            Quaternion rotationStart = owner.transform.rotation;
            bool sawSprint = false;
            for (int index = 0; index < 20; index++)
            {
                yield return new WaitForFixedUpdate();
                sawSprint |= locomotion.IsSprinting;
                Assert.That(cargo.SmallSpinVerticalClearance,
                    Is.GreaterThanOrEqualTo(0.15f));
                Assert.That(ResolvePlanarColliderClearance(
                        playerCollider,
                        binderCollider),
                    Is.GreaterThanOrEqualTo(0.05f));
                Assert.That(Physics.ComputePenetration(
                        playerCollider,
                        playerCollider.transform.position,
                        playerCollider.transform.rotation,
                        binderCollider,
                        binderCollider.transform.position,
                        binderCollider.transform.rotation,
                        out _,
                        out _),
                    Is.False);
            }
            Assert.That(Vector3.Distance(movementStart, owner.transform.position),
                Is.GreaterThan(0.1f));
            Assert.That(Quaternion.Angle(rotationStart, owner.transform.rotation),
                Is.GreaterThan(5f));
            Assert.That(sawSprint, Is.True);
            Assert.That(cargo.Phase,
                Is.EqualTo(ShowcaseCargoStrapPhase.SmallSpinning));
            Assert.That(cargo.BlocksPlayerMobility, Is.False);
            Assert.That(driver.HasStrapRollingUpperBodyLayer, Is.True);
            Assert.That(driver.StrapRollingUpperBodyLayerWeight,
                Is.GreaterThan(0.9f));
            Assert.That(driver.CurrentBaseStateName,
                Is.EqualTo("Walk").Or.EqualTo("Run"));
            SetSnapshot(input, default);

            Vector3 throwAim = binder.transform.position +
                owner.transform.forward * 10f + Vector3.up;
            Assert.That(cargo.TryBeginAttack(throwAim), Is.True);
            float throwDeadline = Time.time + 1.5f;
            while (cargo.Phase != ShowcaseCargoStrapPhase.Stored &&
                   Time.time < throwDeadline)
            {
                yield return null;
            }
            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Stored));
            Assert.That(owner.StrapState, Is.EqualTo(PrototypeStrapOwnershipState.Stored));
            Assert.That(binder.CommittedPlayerUses, Is.EqualTo(1));
            Assert.That(body.linearVelocity.magnitude, Is.GreaterThan(4f));
            Assert.That(binder.LastPhysicalReleaseImpactSnapshot.IsValid, Is.True);
            Assert.That(binder.LastPhysicalReleaseImpactSnapshot.AttackOriginType,
                Is.EqualTo(ShowcaseObjectImpactAttackOriginType.LightStrapThrow));
            Assert.That(binder.LastPhysicalReleaseImpactSnapshot.Charge01,
                Is.EqualTo(0f).Within(0.001f));
            Assert.That(binder.LastPhysicalReleaseKineticEnergy, Is.GreaterThan(0f));

            float depletionDeadline = Time.time + 2f;
            while (binder.gameObject.activeSelf && Time.time < depletionDeadline)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(binder.gameObject.activeSelf, Is.False);
            for (int index = 0; index < 3; index++)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        private static void CompleteAttachSequence(ShowcaseCargoStrapController cargo)
        {
            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Prepare));
            uint prepareRevision = cargo.AnimationCueRevision;
            Assert.That(cargo.NotifyAnimationStateEntered(prepareRevision), Is.True);
            Assert.That(cargo.CompleteAnimationStrapAction(prepareRevision), Is.True);

            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Throw));
            uint throwRevision = cargo.AnimationCueRevision;
            Assert.That(cargo.NotifyAnimationStateEntered(throwRevision), Is.True);
            Assert.That(cargo.CommitAnimationStrapThrowRelease(throwRevision), Is.True);
            Assert.That(cargo.CommitAnimationStrapAttachContact(throwRevision), Is.True);
            Assert.That(cargo.CompleteAnimationStrapAction(throwRevision), Is.True);

            Assert.That(cargo.Phase, Is.EqualTo(ShowcaseCargoStrapPhase.Pull));
            uint pullRevision = cargo.AnimationCueRevision;
            Assert.That(cargo.NotifyAnimationStateEntered(pullRevision), Is.True);
            Assert.That(cargo.CommitAnimationStrapPullComplete(pullRevision), Is.True);
            ShowcaseCargoStrapPhase expectedPhase =
                cargo.ActiveTarget?.WeightClass == PrototypeWeightClass.Light
                    ? ShowcaseCargoStrapPhase.SmallSpinning
                    : ShowcaseCargoStrapPhase.Tensioned;
            Assert.That(cargo.Phase, Is.EqualTo(expectedPhase));
        }

        private static float ResolvePlanarColliderClearance(
            Collider first,
            Collider second)
        {
            Vector3 offset = Vector3.ProjectOnPlane(
                second.bounds.center - first.bounds.center,
                Vector3.up);
            float firstRadius = Mathf.Max(
                first.bounds.extents.x,
                first.bounds.extents.z);
            float secondRadius = Mathf.Sqrt(
                (second.bounds.extents.x * second.bounds.extents.x) +
                (second.bounds.extents.z * second.bounds.extents.z));
            return offset.magnitude - firstRadius - secondRadius;
        }

        private static T[] SceneComponents<T>() where T : Component
        {
            Scene scene = SceneManager.GetActiveScene();
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }

        private static void SetSnapshot(
            InputSystemLogicalInputAdapter input,
            LogicalInputSnapshot snapshot)
        {
            PropertyInfo property = typeof(InputSystemLogicalInputAdapter)
                .GetProperty(nameof(InputSystemLogicalInputAdapter.Snapshot));
            MethodInfo setter = property?.GetSetMethod(true);
            Assert.That(setter, Is.Not.Null);
            setter.Invoke(input, new object[] { snapshot });
        }
    }
}
