using System.Collections;
using System.Linq;
using System.Reflection;
using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Input;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestShowcaseCameraPlayModeTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v007.unity";

        [UnityTest]
        public IEnumerator V007AcceptedHitFeedback_ReachesOutputCameraAndRestoresExactly()
        {
            yield return EnsureShowcaseSceneUnloaded();
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
            Assert.That(load, Is.Not.Null);
            while (!load.isDone)
            {
                yield return null;
            }

            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);
            try
            {
                DisablePointerCapture(scene);
                ThirdPersonCameraRig cameraRig =
                    GetSceneComponents<ThirdPersonCameraRig>(scene).Single();
                ShowcaseCameraConstraintSolver solver =
                    GetSceneComponents<ShowcaseCameraConstraintSolver>(scene).Single();
                ShowcaseHitFeedbackController feedback =
                    GetSceneComponents<ShowcaseHitFeedbackController>(scene).Single();
                foreach (ShowcaseRunnerAgent runner in
                         GetSceneComponents<ShowcaseRunnerAgent>(scene))
                {
                    runner.enabled = false;
                }

                cameraRig.HardResetCameraState();
                for (int frame = 0; frame < 6; frame++)
                {
                    yield return null;
                }

                feedback.Play(
                    ShowcaseHitVfxTier.Heavy,
                    cameraRig.OutputCamera.transform.position +
                        cameraRig.OutputCamera.transform.forward,
                    cameraRig.OutputCamera.transform.forward);
                float maximumPositionDelta = 0f;
                float maximumRotationDelta = 0f;
                float deadline = Time.realtimeSinceStartup + 0.3f;
                while (Time.realtimeSinceStartup < deadline)
                {
                    yield return null;
                    if (!solver.HasLastSolvedPose)
                    {
                        continue;
                    }
                    maximumPositionDelta = Mathf.Max(
                        maximumPositionDelta,
                        Vector3.Distance(
                            cameraRig.OutputCamera.transform.position,
                            solver.LastSolvedPose.position));
                    maximumRotationDelta = Mathf.Max(
                        maximumRotationDelta,
                        Quaternion.Angle(
                            cameraRig.OutputCamera.transform.rotation,
                            solver.LastSolvedPose.rotation));
                }

                Assert.That(maximumPositionDelta, Is.GreaterThan(0.04f),
                    "The accepted Heavy impulse never reached OutputCamera position.");
                Assert.That(maximumRotationDelta, Is.GreaterThan(0.25f),
                    "The accepted Heavy impulse never reached OutputCamera rotation.");
                yield return new WaitForSecondsRealtime(0.18f);
                yield return null;
                Assert.That(Vector3.Distance(
                        cameraRig.OutputCamera.transform.position,
                        solver.LastSolvedPose.position),
                    Is.LessThanOrEqualTo(0.002f));
                Assert.That(Quaternion.Angle(
                        cameraRig.OutputCamera.transform.rotation,
                        solver.LastSolvedPose.rotation),
                    Is.LessThanOrEqualTo(0.02f));
                Assert.That(feedback.ActiveVictimGlowCount, Is.Zero,
                    "A direct camera-only impulse must not light a character.");
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        [UnityTest]
        public IEnumerator V007ActualChr00_LeonCoreNormalAndAimLandmarksMatchApprovedViewportRanges()
        {
            yield return EnsureShowcaseSceneUnloaded();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
            Assert.That(load, Is.Not.Null);
            while (!load.isDone)
            {
                yield return null;
            }

            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);
            try
            {
                DisablePointerCapture(scene);
                ThirdPersonCameraRig cameraRig = GetSceneComponents<ThirdPersonCameraRig>(scene)
                    .Single();
                ShowcaseCharacterAnimationDriver playerDriver =
                    GetSceneComponents<ShowcaseCharacterAnimationDriver>(scene)
                        .Single(driver =>
                            driver.Role == ShowcaseCharacterAnimationRole.Player);
                ShowcaseRunnerAgent[] runners = GetSceneComponents<ShowcaseRunnerAgent>(scene);
                foreach (ShowcaseRunnerAgent runner in runners)
                {
                    runner.enabled = false;
                }

                Animator animator = playerDriver.Animator;
                Assert.That(animator, Is.Not.Null);
                playerDriver.enabled = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.Play(Animator.StringToHash("Base Layer.Idle"), 0, 0f);
                animator.Update(0f);
                animator.speed = 0f;

                Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
                Transform leftShoulder = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
                Transform rightShoulder = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
                Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                Transform leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                Transform rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                Assert.That(head, Is.Not.Null);
                Assert.That(leftShoulder, Is.Not.Null);
                Assert.That(rightShoulder, Is.Not.Null);
                Assert.That(hips, Is.Not.Null);
                Assert.That(leftFoot, Is.Not.Null);
                Assert.That(rightFoot, Is.Not.Null);

                cameraRig.OutputCamera.aspect = 16f / 9f;
                cameraRig.HardResetCameraState();
                QueueGamepadState(gamepad, default);
                for (int frame = 0; frame < 8; frame++)
                {
                    yield return null;
                }

                Vector3 normalHead = cameraRig.OutputCamera.WorldToViewportPoint(head.position);
                Vector3 normalShoulder = cameraRig.OutputCamera.WorldToViewportPoint(
                    (leftShoulder.position + rightShoulder.position) * 0.5f);
                Vector3 normalHips = cameraRig.OutputCamera.WorldToViewportPoint(hips.position);
                Vector3 normalFeet = cameraRig.OutputCamera.WorldToViewportPoint(
                    (leftFoot.position + rightFoot.position) * 0.5f);
                string normalContext = DescribeLandmarks(
                    cameraRig,
                    normalHead,
                    normalShoulder,
                    normalHips,
                    normalFeet);
                Assert.That(normalHead.z, Is.GreaterThan(0f), normalContext);
                Assert.That(normalHead.x, Is.InRange(0.29f, 0.36f), normalContext);
                Assert.That(normalHead.y, Is.InRange(0.63f, 0.73f), normalContext);
                Assert.That(normalShoulder.z, Is.GreaterThan(0f), normalContext);
                Assert.That(normalHips.z, Is.GreaterThan(0f), normalContext);
                Assert.That(normalFeet.z, Is.GreaterThan(0f), normalContext);

                QueueGamepadState(gamepad, new GamepadState { leftTrigger = 1f });
                yield return new WaitForSecondsRealtime(0.25f);
                yield return null;

                Vector3 aimHead = cameraRig.OutputCamera.WorldToViewportPoint(head.position);
                Vector3 aimShoulder = cameraRig.OutputCamera.WorldToViewportPoint(
                    (leftShoulder.position + rightShoulder.position) * 0.5f);
                Vector3 aimHips = cameraRig.OutputCamera.WorldToViewportPoint(hips.position);
                Vector3 aimFeet = cameraRig.OutputCamera.WorldToViewportPoint(
                    (leftFoot.position + rightFoot.position) * 0.5f);
                string aimContext = DescribeLandmarks(
                    cameraRig,
                    aimHead,
                    aimShoulder,
                    aimHips,
                    aimFeet);
                Assert.That(cameraRig.CoreMode, Is.EqualTo(CameraCoreMode.Aim), aimContext);
                Assert.That(aimHead.z, Is.GreaterThan(0f), aimContext);
                Assert.That(aimHead.x, Is.InRange(0.16f, 0.23f), aimContext);
                Assert.That(aimHead.y, Is.InRange(0.64f, 0.76f), aimContext);
            }
            finally
            {
                if (gamepad != null && gamepad.added)
                {
                    InputSystem.RemoveDevice(gamepad);
                }
                if (scene.IsValid() && scene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        [UnityTest]
        public IEnumerator V007CarryAndThrowOverrides_PreserveLensAndCentralSafeArea()
        {
            yield return EnsureShowcaseSceneUnloaded();
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
            Assert.That(load, Is.Not.Null);
            while (!load.isDone)
            {
                yield return null;
            }

            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);
            try
            {
                DisablePointerCapture(scene);
                ThirdPersonCameraRig cameraRig =
                    GetSceneComponents<ThirdPersonCameraRig>(scene).Single();
                PrototypeInteractionStateOwner owner =
                    GetSceneComponents<PrototypeInteractionStateOwner>(scene).Single();
                PrototypePhysicalObjectLifecycle lifecycle =
                    GetSceneComponents<PrototypePhysicalObjectLifecycle>(scene).Single();
                PrototypeInteractionController interaction =
                    GetSceneComponents<PrototypeInteractionController>(scene).Single();
                InputSystemLogicalInputAdapter logicalInput =
                    GetSceneComponents<InputSystemLogicalInputAdapter>(scene).Single();
                logicalInput.SetPointerCaptureRequired(false);
                Assert.That(
                    logicalInput.InputActions.FindActionMap("PrototypePlayer", true)
                        .FindAction("Throw", true).bindings
                        .Select(binding => binding.effectivePath),
                    Does.Contain("<Mouse>/leftButton"));
                PrototypeInteractionTarget light =
                    GetSceneComponents<PrototypeInteractionTarget>(scene)
                        .Single(target => target.RuntimeInstanceKey == "BINDER-DIRECT-01");
                PrototypeInteractionTarget medium =
                    GetSceneComponents<PrototypeInteractionTarget>(scene)
                        .Single(target => target.RuntimeInstanceKey == "CHAIR-DIRECT-01");
                foreach (ShowcaseRunnerAgent runner in GetSceneComponents<ShowcaseRunnerAgent>(scene))
                {
                    runner.enabled = false;
                }

                cameraRig.OutputCamera.aspect = 16f / 9f;
                cameraRig.HardResetCameraState();
                Vector3 lightStartPosition = light.transform.position;
                Quaternion lightStartRotation = light.transform.rotation;
                yield return CarryAndAssert(
                    cameraRig,
                    owner,
                    lifecycle,
                    light,
                    CameraCarryMode.Light,
                    2.70f);
                logicalInput.enabled = false;
                Collider[] playerCarryColliders =
                    owner.GetComponentsInChildren<Collider>(true);
                Collider[] binderColliders =
                    light.GetComponentsInChildren<Collider>(true);
                for (int cycle = 0; cycle < 20; cycle++)
                {
                    SetLogicalSnapshot(logicalInput, new LogicalInputSnapshot(
                        Vector2.zero,
                        Vector2.zero,
                        true,
                        false,
                        false,
                        false,
                        false,
                        false));
                    for (int frame = 0; frame < 12; frame++)
                    {
                        yield return null;
                        AssertAllCarryPairsIgnored(playerCarryColliders, binderColliders);
                    }
                    SetLogicalSnapshot(logicalInput, default);
                    for (int frame = 0; frame < 12; frame++)
                    {
                        yield return null;
                        AssertAllCarryPairsIgnored(playerCarryColliders, binderColliders);
                    }
                }
                Assert.That(lifecycle.GripWorldBlocked, Is.False);
                yield return ResetCarry(
                    owner,
                    lifecycle,
                    light,
                    lightStartPosition,
                    lightStartRotation);
                yield return CarryAndAssert(
                    cameraRig,
                    owner,
                    lifecycle,
                    medium,
                    CameraCarryMode.Medium,
                    3.00f);

                SetLogicalSnapshot(logicalInput, new LogicalInputSnapshot(
                    Vector2.zero,
                    Vector2.zero,
                    false,
                    false,
                    true,
                    false,
                    false,
                    false,
                    throwHeld: true));
                yield return null;
                Assert.That(
                    logicalInput.Snapshot.ThrowHeld,
                    Is.True,
                    "The LMB logical hold state was not preserved for the interaction owner.");
                Assert.That(
                    logicalInput.Snapshot.Throw,
                    Is.True,
                    "The LMB logical press edge was not delivered to the interaction owner.");
                yield return new WaitForSecondsRealtime(0.14f);
                yield return null;

                Assert.That(cameraRig.CarryMode, Is.EqualTo(CameraCarryMode.Medium));
                Assert.That(cameraRig.IsThrowWindup, Is.True);
                Assert.That(
                    interaction.ThrowPresentation.Phase,
                    Is.EqualTo(PrototypeThrowPresentationPhase.ChargingDirectCarry));
                Assert.That(interaction.ThrowPresentation.Target, Is.SameAs(medium));
                Assert.That(
                    owner.HandState,
                    Is.EqualTo(PrototypeHandOwnershipState.Carrying),
                    "ThrowWindup must begin during LMB hold, before ThrowCommitted exists.");
                Assert.That(cameraRig.ThrowWindupBlend, Is.EqualTo(1f).Within(0.02f));
                Assert.That(cameraRig.CurrentDistance, Is.EqualTo(3.18f).Within(0.03f));
                Assert.That(cameraRig.CurrentFieldOfView, Is.EqualTo(50f).Within(0.01f));
                AssertCentralSafeAreaClear(cameraRig.OutputCamera, medium);

                SetLogicalSnapshot(logicalInput, new LogicalInputSnapshot(
                    Vector2.zero,
                    Vector2.zero,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    throwReleased: true));
                yield return null;
                Assert.That(
                    interaction.ThrowPresentation.Phase,
                    Is.EqualTo(PrototypeThrowPresentationPhase.ReleaseAccepted));
                Assert.That(cameraRig.IsThrowWindup, Is.True);
                float timeoutAt = Time.realtimeSinceStartup + 5f;
                while (lifecycle.CarriedTarget != null &&
                       Time.realtimeSinceStartup < timeoutAt)
                {
                    yield return null;
                }
                Assert.That(lifecycle.CarriedTarget, Is.Null,
                    "The natural ThrowRelease event did not physically release the chair.");
                yield return new WaitForSecondsRealtime(0.14f);
                Assert.That(cameraRig.CarryMode, Is.EqualTo(CameraCarryMode.None));
                Assert.That(cameraRig.IsThrowWindup, Is.False);
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        [UnityTest]
        public IEnumerator V007ActualPartition_ChairCarryAimReleaseTwentyTimes_RemainsStable()
        {
            yield return EnsureShowcaseSceneUnloaded();
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
            Assert.That(load, Is.Not.Null);
            while (!load.isDone)
            {
                yield return null;
            }

            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);
            try
            {
                DisablePointerCapture(scene);
                ThirdPersonCameraRig cameraRig =
                    GetSceneComponents<ThirdPersonCameraRig>(scene).Single();
                ShowcaseCameraConstraintSolver solver =
                    GetSceneComponents<ShowcaseCameraConstraintSolver>(scene).Single();
                PrototypeInteractionStateOwner owner =
                    GetSceneComponents<PrototypeInteractionStateOwner>(scene).Single();
                PrototypePhysicalObjectLifecycle lifecycle =
                    GetSceneComponents<PrototypePhysicalObjectLifecycle>(scene).Single();
                InputSystemLogicalInputAdapter logicalInput =
                    GetSceneComponents<InputSystemLogicalInputAdapter>(scene).Single();
                ThirdPersonLocomotion locomotion = owner.GetComponent<ThirdPersonLocomotion>();
                Assert.That(locomotion, Is.Not.Null);
                PrototypeInteractionTarget chair =
                    GetSceneComponents<PrototypeInteractionTarget>(scene)
                        .Single(target => target.RuntimeInstanceKey == "CHAIR-DIRECT-01");
                foreach (ShowcaseRunnerAgent runner in GetSceneComponents<ShowcaseRunnerAgent>(scene))
                {
                    runner.enabled = false;
                }
                ShowcaseMobileAllyController ally =
                    GetSceneComponents<ShowcaseMobileAllyController>(scene).Single();
                ally.enabled = false;
                logicalInput.enabled = false;

                cameraRig.OutputCamera.aspect = 16f / 9f;
                cameraRig.HardResetCameraState();
                yield return CarryAndAssert(
                    cameraRig,
                    owner,
                    lifecycle,
                    chair,
                    CameraCarryMode.Medium,
                    3.00f,
                    assertCentralSafeArea: false);

                Collider partition = GetSceneComponents<Collider>(scene)
                    .Where(collider =>
                        collider != null &&
                        collider.gameObject.isStatic &&
                        collider.bounds.size.y >= 1f &&
                        collider.name.ToUpperInvariant().Contains("PARTITION"))
                    .OrderBy(collider =>
                        Vector3.Distance(collider.bounds.center, owner.transform.position))
                    .FirstOrDefault();
                Assert.That(partition, Is.Not.Null,
                    "The actual v007 authored partition fixture was not found.");
                Assert.That(
                    partition.GetComponentInParent<ShowcaseCameraSoftOccluderRoot>(),
                    Is.Not.Null,
                    "Authored partition geometry must remain a gameplay collider but be " +
                    "classified as a soft camera occluder.");
                Assert.That(
                    partition.GetComponentInParent<ShowcaseCameraWorldBlockerRoot>(),
                    Is.Null,
                    "Soft partition geometry must not enter the hard camera-world allow-list.");
                Vector3 openPlayerPosition = owner.transform.position;
                Quaternion openPlayerRotation = owner.transform.rotation;
                PlacePlayerBesidePartition(owner, partition, cameraRig);
                yield return new WaitForSecondsRealtime(0.45f);

                Vector3 stationaryPlayerPosition = owner.transform.position;
                Collider[] playerColliders =
                    owner.GetComponentsInChildren<Collider>(true);
                Collider[] chairColliders =
                    chair.GetComponentsInChildren<Collider>(true);
                for (int cycle = 0; cycle < 20; cycle++)
                {
                    SetLogicalSnapshot(logicalInput, new LogicalInputSnapshot(
                        Vector2.zero,
                        Vector2.zero,
                        true,
                        false,
                        false,
                        false,
                        false,
                        false));
                    for (int frame = 0; frame < 12; frame++)
                    {
                        yield return null;
                        AssertSingleAuthorityFrame(cameraRig, solver);
                        AssertAllCarryPairsIgnored(playerColliders, chairColliders);
                    }
                    SetLogicalSnapshot(logicalInput, default);
                    for (int frame = 0; frame < 12; frame++)
                    {
                        yield return null;
                        AssertSingleAuthorityFrame(cameraRig, solver);
                        AssertAllCarryPairsIgnored(playerColliders, chairColliders);
                    }
                }

                SetLogicalSnapshot(logicalInput, default);
                yield return new WaitForSecondsRealtime(0.5f);
                Assert.That(cameraRig.CarryMode, Is.EqualTo(CameraCarryMode.Medium));
                Assert.That(cameraRig.CoreMode, Is.EqualTo(CameraCoreMode.Normal));
                Assert.That(
                    Vector3.Distance(owner.transform.position, stationaryPlayerPosition),
                    Is.LessThanOrEqualTo(0.005f),
                    "Camera correction fed back into the stationary player root.");
                Assert.That(
                    cameraRig.CurrentBlockingCollider == null ||
                    !IsInHierarchy(cameraRig.CurrentBlockingCollider.transform, owner.transform),
                    Is.True);
                Assert.That(
                    cameraRig.CurrentBlockingCollider == null ||
                    !IsInHierarchy(cameraRig.CurrentBlockingCollider.transform, chair.transform),
                    Is.True);
                yield return AssertCameraRemainsStable(
                    cameraRig,
                    120,
                    0.01f,
                    0.1f,
                    solver);

                // Exit naturally in sub-teleport increments.  A HardReset here would
                // hide the historical shared-recovery-timer latch.
                yield return MovePlayerWithoutCameraReset(
                    owner,
                    openPlayerPosition,
                    openPlayerRotation,
                    0.45f);
                yield return WaitForOpenCameraRecovery(cameraRig, 2f);
                Assert.That(lifecycle.GripWorldBlocked, Is.False,
                    "Open-space Aim release must not be classified as a grip/world obstruction.");

                Assert.That(locomotion.MovementReference,
                    Is.SameAs(cameraRig.GameplayMovementReference));
                Assert.That(locomotion.MovementReference,
                    Is.Not.SameAs(cameraRig.OutputCamera.transform),
                    "WASD must not use a collision-compressed output camera as its heading authority.");

                // Rotate the gameplay heading while carrying, leave Aim, then prove
                // forward movement still follows that stable heading rather than a
                // compressed/recovering output-camera pose.
                SetLogicalSnapshot(logicalInput, new LogicalInputSnapshot(
                    Vector2.zero,
                    new Vector2(90f, 0f),
                    true,
                    false,
                    false,
                    false,
                    false,
                    false));
                yield return null;
                SetLogicalSnapshot(logicalInput, new LogicalInputSnapshot(
                    Vector2.zero,
                    Vector2.zero,
                    true,
                    false,
                    false,
                    false,
                    false,
                    false));
                yield return new WaitForSecondsRealtime(0.22f);
                SetLogicalSnapshot(logicalInput, default);
                yield return new WaitForSecondsRealtime(0.22f);

                Vector3 expectedForward = cameraRig.GameplayMovementReference.forward;
                expectedForward.y = 0f;
                expectedForward.Normalize();
                Vector3 movementStart = owner.transform.position;
                SetLogicalSnapshot(logicalInput, new LogicalInputSnapshot(
                    Vector2.up,
                    Vector2.zero,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false));
                for (int frame = 0; frame < 8; frame++)
                {
                    yield return null;
                    AssertSingleAuthorityFrame(cameraRig, solver);
                }
                SetLogicalSnapshot(logicalInput, default);
                Vector3 displacement = owner.transform.position - movementStart;
                displacement.y = 0f;
                Assert.That(displacement.magnitude, Is.GreaterThan(0.02f));
                Assert.That(
                    Vector3.Dot(displacement.normalized, expectedForward),
                    Is.GreaterThan(0.9f),
                    "Carry Aim release scrambled WASD relative to the stable gameplay heading.");
                yield return WaitForOpenCameraRecovery(cameraRig, 2f);
                yield return AssertCameraRemainsStable(cameraRig, 60, 0.02f, 0.1f);
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        [UnityTest]
        public IEnumerator V007ActualWASDRoomEntryExit_SoftPartitionRemainsCameraTransparent()
        {
            yield return EnsureShowcaseSceneUnloaded();
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
            Assert.That(load, Is.Not.Null);
            while (!load.isDone)
            {
                yield return null;
            }

            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);
            try
            {
                DisablePointerCapture(scene);
                ThirdPersonCameraRig cameraRig =
                    GetSceneComponents<ThirdPersonCameraRig>(scene).Single();
                ShowcaseCameraConstraintSolver solver =
                    GetSceneComponents<ShowcaseCameraConstraintSolver>(scene).Single();
                Assert.That(solver, Is.Not.Null);
                PrototypeInteractionStateOwner owner =
                    GetSceneComponents<PrototypeInteractionStateOwner>(scene).Single();
                PrototypePhysicalObjectLifecycle lifecycle =
                    GetSceneComponents<PrototypePhysicalObjectLifecycle>(scene).Single();
                PrototypeInteractionController interaction =
                    GetSceneComponents<PrototypeInteractionController>(scene).Single();
                InputSystemLogicalInputAdapter logicalInput =
                    GetSceneComponents<InputSystemLogicalInputAdapter>(scene).Single();
                PrototypeInteractionTarget binder =
                    GetSceneComponents<PrototypeInteractionTarget>(scene)
                        .Single(target => target.RuntimeInstanceKey == "BINDER-DIRECT-01");
                foreach (ShowcaseRunnerAgent runner in
                         GetSceneComponents<ShowcaseRunnerAgent>(scene))
                {
                    runner.enabled = false;
                }
                GetSceneComponents<ShowcaseMobileAllyController>(scene).Single().enabled = false;
                logicalInput.enabled = false;
                cameraRig.OutputCamera.aspect = 16f / 9f;

                Assert.That(
                    TryFindAdjacentPartitionSeam(
                        scene,
                        owner.transform.position,
                        out Vector3 seam,
                        out _,
                        out Vector3 outward),
                    Is.True,
                    "No adjacent authored partition seam fixture was found in v007.");
                CharacterController controller = owner.GetComponent<CharacterController>();
                Assert.That(controller, Is.Not.Null);
                float passageOffset =
                    controller.radius + controller.skinWidth + 0.18f;
                Vector3 pointA = seam + outward * passageOffset;
                Vector3 pointB = seam - outward * passageOffset;
                pointA.y = owner.transform.position.y;
                pointB.y = owner.transform.position.y;
                Quaternion facing = Quaternion.LookRotation(outward, Vector3.up);
                bool controllerEnabled = controller.enabled;
                controller.enabled = false;
                owner.transform.SetPositionAndRotation(pointA, facing);
                controller.enabled = controllerEnabled;
                Physics.SyncTransforms();
                cameraRig.HardResetCameraState();
                yield return new WaitForSecondsRealtime(0.35f);
                int normalTransitionBaseline = cameraRig.ContextualCornerTransitionCount;
                yield return ShuttleAcrossSeam(
                    owner,
                    cameraRig,
                    solver,
                    logicalInput,
                    pointA,
                    pointB,
                    4,
                    expectContextualCorner: false);
                Assert.That(
                    cameraRig.ContextualCornerTransitionCount,
                    Is.EqualTo(normalTransitionBaseline),
                    "Normal camera activated contextual corner state at a partition seam.");
                yield return AssertCameraRemainsStable(
                    cameraRig,
                    120,
                    0.01f,
                    0.1f,
                    solver);

                SetLogicalSnapshot(logicalInput, new LogicalInputSnapshot(
                    Vector2.zero,
                    Vector2.zero,
                    true,
                    false,
                    false,
                    false,
                    false,
                    false));
                yield return new WaitForSecondsRealtime(0.30f);
                yield return ShuttleAcrossSeam(
                    owner,
                    cameraRig,
                    solver,
                    logicalInput,
                    pointA,
                    pointB,
                    4,
                    expectContextualCorner: false);
                yield return AssertCameraRemainsStable(
                    cameraRig,
                    120,
                    0.01f,
                    0.1f,
                    solver);

                SetLogicalSnapshot(logicalInput, default);
                yield return new WaitForSecondsRealtime(0.25f);
                PrototypeInteractionDecision carryDecision =
                    PrototypeInteractionDecision.Accept(
                        PrototypeInteractionOutcome.Scoop,
                        binder);
                ShowcaseBinderGripSocket gripSocket =
                    owner.GetComponent<ShowcaseBinderGripSocket>();
                Assert.That(gripSocket, Is.Not.Null);
                Assert.That(
                    gripSocket.TryGetWorldPose(
                        out Vector3 gripPosition,
                        out Quaternion gripRotation),
                    Is.True);
                binder.transform.SetPositionAndRotation(gripPosition, gripRotation);
                Rigidbody binderBody = binder.GetComponent<Rigidbody>();
                Assert.That(binderBody, Is.Not.Null);
                binderBody.position = gripPosition;
                binderBody.rotation = gripRotation;
                binderBody.linearVelocity = Vector3.zero;
                binderBody.angularVelocity = Vector3.zero;
                Physics.SyncTransforms();
                Assert.That(owner.TryCommit(carryDecision), Is.True);
                Assert.That(owner.TryCompleteAcquisition(binder), Is.True);
                Assert.That(lifecycle.BeginCarry(binder), Is.True);
                yield return new WaitForSecondsRealtime(0.22f);
                Assert.That(cameraRig.CarryMode, Is.EqualTo(CameraCarryMode.Light));
                yield return ShuttleAcrossSeam(
                    owner,
                    cameraRig,
                    solver,
                    logicalInput,
                    pointA,
                    pointB,
                    3,
                    expectContextualCorner: false);

                SetLogicalSnapshot(logicalInput, new LogicalInputSnapshot(
                    Vector2.zero,
                    Vector2.zero,
                    false,
                    false,
                    true,
                    false,
                    false,
                    false,
                    throwHeld: true));
                yield return new WaitForSecondsRealtime(0.15f);
                Assert.That(
                    interaction.ThrowPresentation.Phase,
                    Is.EqualTo(PrototypeThrowPresentationPhase.ChargingDirectCarry));
                Assert.That(cameraRig.IsThrowWindup, Is.True);
                yield return ShuttleAcrossSeam(
                    owner,
                    cameraRig,
                    solver,
                    logicalInput,
                    pointA,
                    pointB,
                    3,
                    expectContextualCorner: false);
                yield return AssertCameraRemainsStable(
                    cameraRig,
                    120,
                    0.01f,
                    0.1f,
                    solver);
                Assert.That(solver.QuerySaturationCount, Is.Zero);
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        [UnityTest]
        public IEnumerator V007ActualWASD_CarryMediumRightHardWallContactAndReleaseAreBoomOnly()
        {
            yield return EnsureShowcaseSceneUnloaded();
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
            Assert.That(load, Is.Not.Null);
            while (!load.isDone)
            {
                yield return null;
            }

            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);
            try
            {
                DisablePointerCapture(scene);
                ThirdPersonCameraRig cameraRig =
                    GetSceneComponents<ThirdPersonCameraRig>(scene).Single();
                ShowcaseCameraConstraintSolver solver =
                    GetSceneComponents<ShowcaseCameraConstraintSolver>(scene).Single();
                PrototypeInteractionStateOwner owner =
                    GetSceneComponents<PrototypeInteractionStateOwner>(scene).Single();
                PrototypePhysicalObjectLifecycle lifecycle =
                    GetSceneComponents<PrototypePhysicalObjectLifecycle>(scene).Single();
                InputSystemLogicalInputAdapter logicalInput =
                    GetSceneComponents<InputSystemLogicalInputAdapter>(scene).Single();
                PrototypeInteractionTarget chair =
                    GetSceneComponents<PrototypeInteractionTarget>(scene)
                        .Single(target => target.RuntimeInstanceKey == "CHAIR-DIRECT-01");
                foreach (ShowcaseRunnerAgent runner in
                         GetSceneComponents<ShowcaseRunnerAgent>(scene))
                {
                    runner.enabled = false;
                }
                GetSceneComponents<ShowcaseMobileAllyController>(scene).Single().enabled = false;
                logicalInput.enabled = false;
                cameraRig.OutputCamera.aspect = 16f / 9f;

                yield return CarryAndAssert(
                    cameraRig,
                    owner,
                    lifecycle,
                    chair,
                    CameraCarryMode.Medium,
                    3.00f,
                    assertCentralSafeArea: false);

                Collider eastWall = GetSceneComponents<Collider>(scene).Single(collider =>
                    collider.name == "PROXY_Wall_East");
                Assert.That(eastWall.GetComponentInParent<ShowcaseCameraWorldBlockerRoot>(),
                    Is.Not.Null);
                Assert.That(eastWall.GetComponentInParent<ShowcaseCameraSoftOccluderRoot>(),
                    Is.Null);
                CharacterController controller = owner.GetComponent<CharacterController>();
                Assert.That(controller, Is.Not.Null);

                Bounds wallBounds = eastWall.bounds;
                float startZ = Mathf.Clamp(
                    -2f,
                    wallBounds.min.z + 3f,
                    wallBounds.max.z - 7f);
                Vector3 wallStart = new Vector3(
                    wallBounds.min.x -
                    (controller.radius + controller.skinWidth + 0.06f),
                    owner.transform.position.y,
                    startZ);
                PlacePlayerForCameraRoute(
                    owner,
                    controller,
                    wallStart,
                    Quaternion.LookRotation(Vector3.forward, Vector3.up),
                    cameraRig);

                // A hard reset must atomically preserve the current semantic camera
                // state; restarting Normal->Carry here reintroduces perpendicular
                // drift that is unrelated to the wall contact.
                yield return null;
                AssertSingleAuthorityFrame(cameraRig, solver);
                Assert.That(cameraRig.IsTransitioning, Is.False,
                    "Hard reset restarted a CarryMedium camera transition.");
                Assert.That(cameraRig.CarryBlend, Is.EqualTo(1f).Within(0.001f));
                Assert.That(cameraRig.CurrentDistance, Is.EqualTo(3f).Within(0.001f));

                float contactDeadline = Time.realtimeSinceStartup + 1f;
                while (solver.StaticContactPhase !=
                           ShowcaseStaticCameraContactPhase.Contact &&
                       Time.realtimeSinceStartup < contactDeadline)
                {
                    yield return null;
                    AssertSingleAuthorityFrame(cameraRig, solver);
                }
                Assert.That(
                    solver.StaticContactPhase,
                    Is.EqualTo(ShowcaseStaticCameraContactPhase.Contact),
                    DescribeSolverFrame(cameraRig, solver));

                int contactEpisode = solver.ContactEpisodeId;
                int restoreStartBaseline = solver.RestoreStartCount;
                float previousBoom = solver.ResolvedBoomDistance;
                Vector3 contactPerpendicular = BoomPerpendicularOffset(
                    solver.FinalStableAnchorLocalCameraOffset);
                bool emergencyShoulder = solver.EmergencyShoulderLatched;
                System.Action sampleHardContact = () =>
                {
                    string context = DescribeSolverFrame(cameraRig, solver);
                    AssertSingleAuthorityFrame(cameraRig, solver);
                    Assert.That(solver.StaticContactPhase,
                        Is.EqualTo(ShowcaseStaticCameraContactPhase.Contact), context);
                    Assert.That(solver.HardConstraintActiveThisFrame, Is.True, context);
                    Assert.That(solver.ActiveHardBlocker, Is.Not.Null, context);
                    Assert.That(
                        IsInHierarchy(solver.ActiveHardBlocker.transform, eastWall.transform) ||
                        IsInHierarchy(eastWall.transform, solver.ActiveHardBlocker.transform),
                        Is.True,
                        context);
                    Assert.That(solver.ContactEpisodeId, Is.EqualTo(contactEpisode), context);
                    Assert.That(solver.RestoreStartCount,
                        Is.EqualTo(restoreStartBaseline), context);
                    Assert.That(solver.ResolvedGameplayWeight,
                        Is.EqualTo(1f).Within(0.001f), context);
                    if (!emergencyShoulder)
                    {
                        Assert.That(solver.ResolvedShoulderScale,
                            Is.EqualTo(1f).Within(0.001f), context);
                    }
                    Assert.That(solver.ResolvedBoomDistance,
                        Is.LessThanOrEqualTo(previousBoom + 0.002f), context);
                    Assert.That(
                        Vector3.Distance(
                            BoomPerpendicularOffset(
                                solver.FinalStableAnchorLocalCameraOffset),
                            contactPerpendicular),
                        Is.LessThanOrEqualTo(0.005f),
                        context);
                    previousBoom = solver.ResolvedBoomDistance;
                };

                Vector3 firstLegEnd = wallStart + Vector3.forward * 2f;
                yield return WalkByLogicalWASD(
                    owner,
                    cameraRig,
                    logicalInput,
                    firstLegEnd,
                    4f,
                    sampleHardContact);

                Renderer[] wallRenderers = eastWall.GetComponentsInChildren<Renderer>(true);
                Assert.That(wallRenderers, Is.Not.Empty,
                    "The hard-wall renderer-hide continuity fixture is missing.");
                bool[] rendererStates = wallRenderers
                    .Select(renderer => renderer.enabled)
                    .ToArray();
                try
                {
                    foreach (Renderer renderer in wallRenderers)
                    {
                        renderer.enabled = false;
                    }
                    Assert.That(eastWall.enabled, Is.True,
                        "Renderer-only visibility changes must preserve the hard collider.");
                    Vector3 secondLegEnd = wallStart + Vector3.forward * 4f;
                    yield return WalkByLogicalWASD(
                        owner,
                        cameraRig,
                        logicalInput,
                        secondLegEnd,
                        4f,
                        sampleHardContact);
                }
                finally
                {
                    for (int index = 0; index < wallRenderers.Length; index++)
                    {
                        if (wallRenderers[index] != null)
                        {
                            wallRenderers[index].enabled = rendererStates[index];
                        }
                    }
                }

                Assert.That(
                    PlanarDistance(owner.transform.position, wallStart),
                    Is.GreaterThanOrEqualTo(3.8f),
                    "Actual CarryMedium WASD traversal did not cover the required hard-wall distance.");

                bool sawRestoring = false;
                bool sawClearAfterRestoring = false;
                bool reenteredContactAfterRestoring = false;
                float previousReleaseBoom = solver.ResolvedBoomDistance;
                System.Action sampleRelease = () =>
                {
                    AssertSingleAuthorityFrame(cameraRig, solver);
                    string context = DescribeSolverFrame(cameraRig, solver);
                    ShowcaseStaticCameraContactPhase phase = solver.StaticContactPhase;
                    if (phase == ShowcaseStaticCameraContactPhase.Restoring)
                    {
                        sawRestoring = true;
                        Assert.That(solver.ResolvedBoomDistance,
                            Is.GreaterThanOrEqualTo(previousReleaseBoom - 0.002f), context);
                        Assert.That(solver.ResolvedBoomDistance,
                            Is.LessThanOrEqualTo(solver.RequestedBoomDistance + 0.002f), context);
                    }
                    else if (phase == ShowcaseStaticCameraContactPhase.Clear && sawRestoring)
                    {
                        sawClearAfterRestoring = true;
                    }
                    else if (phase == ShowcaseStaticCameraContactPhase.Contact && sawRestoring)
                    {
                        reenteredContactAfterRestoring = true;
                    }
                    previousReleaseBoom = solver.ResolvedBoomDistance;
                };

                Vector3 releaseDestination = owner.transform.position + Vector3.left * 1.5f;
                yield return WalkByLogicalWASD(
                    owner,
                    cameraRig,
                    logicalInput,
                    releaseDestination,
                    4f,
                    sampleRelease);
                float releaseDeadline = Time.realtimeSinceStartup + 1.5f;
                while (solver.StaticContactPhase != ShowcaseStaticCameraContactPhase.Clear &&
                       Time.realtimeSinceStartup < releaseDeadline)
                {
                    yield return null;
                    sampleRelease();
                }

                string releaseContext = DescribeSolverFrame(cameraRig, solver);
                Assert.That(sawRestoring, Is.True, releaseContext);
                Assert.That(sawClearAfterRestoring, Is.True, releaseContext);
                Assert.That(reenteredContactAfterRestoring, Is.False, releaseContext);
                Assert.That(solver.RestoreStartCount - restoreStartBaseline,
                    Is.EqualTo(1), releaseContext);
                Assert.That(solver.StaticContactPhase,
                    Is.EqualTo(ShowcaseStaticCameraContactPhase.Clear), releaseContext);
                Assert.That(solver.ResolvedBoomDistance,
                    Is.EqualTo(solver.RequestedBoomDistance).Within(0.002f),
                    releaseContext);
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        [UnityTest]
        public IEnumerator V007AllyNormalElite_CrossCameraBoundary_SubjectBoomLatchIsMonotonic()
        {
            yield return EnsureShowcaseSceneUnloaded();
            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
            Assert.That(load, Is.Not.Null);
            while (!load.isDone)
            {
                yield return null;
            }

            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);
            try
            {
                DisablePointerCapture(scene);
                ThirdPersonCameraRig cameraRig =
                    GetSceneComponents<ThirdPersonCameraRig>(scene).Single();
                ShowcaseCameraConstraintSolver solver =
                    GetSceneComponents<ShowcaseCameraConstraintSolver>(scene).Single();
                PrototypeInteractionStateOwner owner =
                    GetSceneComponents<PrototypeInteractionStateOwner>(scene).Single();
                InputSystemLogicalInputAdapter logicalInput =
                    GetSceneComponents<InputSystemLogicalInputAdapter>(scene).Single();
                logicalInput.enabled = false;
                ShowcaseRunnerAgent[] runners =
                    GetSceneComponents<ShowcaseRunnerAgent>(scene);
                ShowcaseMobileAllyController ally =
                    GetSceneComponents<ShowcaseMobileAllyController>(scene).Single();
                Assert.That(runners.Length, Is.EqualTo(2));
                foreach (ShowcaseRunnerAgent runner in runners)
                {
                    runner.enabled = false;
                }
                ally.enabled = false;

                cameraRig.OutputCamera.aspect = 16f / 9f;
                cameraRig.HardResetCameraState();
                SetLogicalSnapshot(logicalInput, default);
                yield return new WaitForSecondsRealtime(0.2f);
                Vector3 stationaryPlayerPosition = owner.transform.position;
                Assert.That(solver.StaticContactPhase,
                    Is.EqualTo(ShowcaseStaticCameraContactPhase.Clear),
                    DescribeSolverFrame(cameraRig, solver));
                Assert.That(solver.SubjectContactPhase,
                    Is.EqualTo(ShowcaseStaticCameraContactPhase.Clear),
                    DescribeSolverFrame(cameraRig, solver));

                Transform[] actorRoots =
                {
                    runners.Single(runner => !runner.IsElite).transform,
                    runners.Single(runner => runner.IsElite).transform,
                    ally.transform
                };
                foreach (Transform actorRoot in actorRoots)
                {
                    Vector3 originalPosition = actorRoot.position;
                    Quaternion originalRotation = actorRoot.rotation;
                    Vector3 right = cameraRig.StableAnchor.right;
                    right.y = 0f;
                    right.Normalize();
                    Vector3 crossingCenter = cameraRig.OutputCamera.transform.position;
                    crossingCenter.y = originalPosition.y;
                    Vector3 crossingStart = crossingCenter + right * 1.5f;
                    Vector3 crossingEnd = crossingCenter - right * 1.5f;
                    MoveActorToPosition(actorRoot, crossingStart, originalRotation);
                    yield return null;
                    yield return null;

                    Assert.That(solver.StaticContactPhase,
                        Is.EqualTo(ShowcaseStaticCameraContactPhase.Clear),
                        DescribeSolverFrame(cameraRig, solver));
                    Assert.That(solver.SubjectContactPhase,
                        Is.EqualTo(ShowcaseStaticCameraContactPhase.Clear),
                        DescribeSolverFrame(cameraRig, solver));

                    ShowcaseStaticCameraContactPhase staticPhase =
                        solver.StaticContactPhase;
                    int staticEpisode = solver.ContactEpisodeId;
                    int staticRestores = solver.RestoreStartCount;
                    int subjectEpisode = solver.SubjectContactEpisodeId;
                    int subjectRestores = solver.SubjectRestoreStartCount;
                    float previousSubjectBoom = solver.SubjectResolvedBoomDistance;
                    Vector3 perpendicularBaseline = BoomPerpendicularOffset(
                        solver.FinalStableAnchorLocalCameraOffset);
                    bool sawContact = false;
                    bool sawRestoring = false;
                    bool sawClearAfterRestoring = false;
                    bool reenteredContactAfterRestoring = false;

                    System.Action sampleSubjectFrame = () =>
                    {
                        AssertSingleAuthorityFrame(cameraRig, solver);
                        string context = DescribeSolverFrame(cameraRig, solver);
                        Assert.That(solver.StaticContactPhase,
                            Is.EqualTo(staticPhase), context);
                        Assert.That(solver.ContactEpisodeId,
                            Is.EqualTo(staticEpisode), context);
                        Assert.That(solver.RestoreStartCount,
                            Is.EqualTo(staticRestores), context);
                        Assert.That(solver.HardConstraintActiveThisFrame,
                            Is.False, context);
                        Assert.That(solver.ResolvedGameplayWeight,
                            Is.EqualTo(1f).Within(0.001f), context);
                        Assert.That(solver.ResolvedShoulderScale,
                            Is.EqualTo(1f).Within(0.001f), context);
                        Assert.That(
                            Vector3.Distance(
                                BoomPerpendicularOffset(
                                    solver.FinalStableAnchorLocalCameraOffset),
                                perpendicularBaseline),
                            Is.LessThanOrEqualTo(0.005f),
                            context);

                        if (solver.SubjectContactPhase ==
                            ShowcaseStaticCameraContactPhase.Contact)
                        {
                            if (sawRestoring)
                            {
                                reenteredContactAfterRestoring = true;
                            }
                            sawContact = true;
                            Assert.That(solver.SubjectResolvedBoomDistance,
                                Is.LessThanOrEqualTo(previousSubjectBoom + 0.002f),
                                context);
                            Assert.That(solver.CurrentActorProxy,
                                Is.SameAs(actorRoot), context);
                        }
                        else if (solver.SubjectContactPhase ==
                                 ShowcaseStaticCameraContactPhase.Restoring)
                        {
                            sawRestoring = true;
                            Assert.That(solver.SubjectResolvedBoomDistance,
                                Is.GreaterThanOrEqualTo(previousSubjectBoom - 0.002f),
                                context);
                            Assert.That(solver.SubjectResolvedBoomDistance,
                                Is.LessThanOrEqualTo(
                                    solver.RequestedBoomDistance + 0.002f),
                                context);
                        }
                        else if (sawRestoring)
                        {
                            sawClearAfterRestoring = true;
                        }
                        previousSubjectBoom = solver.SubjectResolvedBoomDistance;
                    };

                    const int crossingSteps = 60;
                    for (int step = 1; step <= crossingSteps; step++)
                    {
                        MoveActorToPosition(
                            actorRoot,
                            Vector3.Lerp(
                                crossingStart,
                                crossingEnd,
                                step / (float)crossingSteps),
                            originalRotation);
                        yield return null;
                        sampleSubjectFrame();
                    }

                    float clearDeadline = Time.realtimeSinceStartup + 1.5f;
                    while (solver.SubjectContactPhase !=
                               ShowcaseStaticCameraContactPhase.Clear &&
                           Time.realtimeSinceStartup < clearDeadline)
                    {
                        yield return null;
                        sampleSubjectFrame();
                    }

                    string actorContext = DescribeSolverFrame(cameraRig, solver);
                    Assert.That(sawContact, Is.True, actorContext);
                    Assert.That(sawRestoring, Is.True, actorContext);
                    Assert.That(sawClearAfterRestoring, Is.True, actorContext);
                    Assert.That(reenteredContactAfterRestoring, Is.False, actorContext);
                    Assert.That(solver.SubjectContactEpisodeId - subjectEpisode,
                        Is.EqualTo(1), actorContext);
                    Assert.That(solver.SubjectRestoreStartCount - subjectRestores,
                        Is.EqualTo(1), actorContext);
                    Assert.That(solver.SubjectContactPhase,
                        Is.EqualTo(ShowcaseStaticCameraContactPhase.Clear), actorContext);
                    Assert.That(solver.CurrentActorProxy, Is.Null, actorContext);

                    MoveActorToPosition(actorRoot, originalPosition, originalRotation);
                    yield return null;
                }

                Assert.That(
                    Vector3.Distance(owner.transform.position, stationaryPlayerPosition),
                    Is.LessThanOrEqualTo(0.005f),
                    "Actor proxy camera correction moved the player root.");
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        private static IEnumerator ShuttleAcrossSeam(
            PrototypeInteractionStateOwner owner,
            ThirdPersonCameraRig cameraRig,
            ShowcaseCameraConstraintSolver solver,
            InputSystemLogicalInputAdapter logicalInput,
            Vector3 pointA,
            Vector3 pointB,
            int cycles,
            bool expectContextualCorner)
        {
            CharacterController controller = owner.GetComponent<CharacterController>();
            Assert.That(controller, Is.Not.Null);
            int contextualTransitions = cameraRig.ContextualCornerTransitionCount;
            yield return MoveAcrossSeamHalf(
                owner,
                cameraRig,
                solver,
                logicalInput,
                pointA,
                expectContextualCorner);
            for (int cycle = 0; cycle < cycles; cycle++)
            {
                yield return MoveAcrossSeamHalf(
                    owner,
                    cameraRig,
                    solver,
                    logicalInput,
                    pointB,
                    expectContextualCorner);
                yield return MoveAcrossSeamHalf(
                    owner,
                    cameraRig,
                    solver,
                    logicalInput,
                    pointA,
                    expectContextualCorner);
            }

            int transitionDelta = cameraRig.ContextualCornerTransitionCount -
                contextualTransitions;
            Assert.That(
                transitionDelta,
                Is.LessThanOrEqualTo(expectContextualCorner ? 2 : 0),
                "Partition seam repeatedly reversed the contextual shoulder mode.");
        }

        private static IEnumerator MoveAcrossSeamHalf(
            PrototypeInteractionStateOwner owner,
            ThirdPersonCameraRig cameraRig,
            ShowcaseCameraConstraintSolver solver,
            InputSystemLogicalInputAdapter logicalInput,
            Vector3 destination,
            bool expectContextualCorner)
        {
            Vector3 routeStart = owner.transform.position;
            Vector3 travelDirection = destination - routeStart;
            travelDirection.y = 0f;
            float routeLength = travelDirection.magnitude;
            if (routeLength > 0.0001f)
            {
                travelDirection /= routeLength;
            }
            float deadline = Time.realtimeSinceStartup + 3f;
            Vector3 minimumLocalOffset = solver.FinalStableAnchorLocalCameraOffset;
            Vector3 maximumLocalOffset = minimumLocalOffset;
            while (Vector3.Dot(
                       destination - owner.transform.position,
                       travelDirection) > 0f &&
                   Time.realtimeSinceStartup < deadline)
            {
                Vector3 direction = destination - owner.transform.position;
                direction.y = 0f;
                direction.Normalize();
                Vector2 move = WorldDirectionToMoveInput(
                    direction,
                    cameraRig.GameplayMovementReference);
                SetLogicalSnapshot(
                    logicalInput,
                    WithMove(logicalInput.Snapshot, move));
                yield return null;
                AssertSingleAuthorityFrame(cameraRig, solver);
                Assert.That(solver.QuerySaturationCount, Is.Zero);
                string context = DescribeSolverFrame(cameraRig, solver);
                Assert.That(solver.HardConstraintActiveThisFrame, Is.False, context);
                Assert.That(solver.ActiveHardBlocker, Is.Null, context);
                Assert.That(
                    solver.ResolvedBoomDistance,
                    Is.EqualTo(solver.RequestedBoomDistance).Within(0.005f),
                    context);
                Assert.That(solver.ResolvedGameplayWeight,
                    Is.EqualTo(1f).Within(0.001f), context);
                Assert.That(solver.ResolvedShoulderScale,
                    Is.EqualTo(1f).Within(0.001f), context);
                Vector3 localOffset = solver.FinalStableAnchorLocalCameraOffset;
                minimumLocalOffset = Vector3.Min(minimumLocalOffset, localOffset);
                maximumLocalOffset = Vector3.Max(maximumLocalOffset, localOffset);
                if (!expectContextualCorner)
                {
                    Assert.That(
                        cameraRig.ContextualCornerMode,
                        Is.EqualTo(ShowcaseContextualCornerMode.RightDefault),
                        context);
                }
            }
            SetLogicalSnapshot(logicalInput, WithMove(logicalInput.Snapshot, Vector2.zero));
            float routeProgress = Vector3.Dot(
                owner.transform.position - routeStart,
                travelDirection);
            Assert.That(
                routeProgress,
                Is.GreaterThanOrEqualTo(routeLength - 0.2f),
                "Actual locomotion did not complete the authored soft-partition passage. " +
                $"start={routeStart}, destination={destination}, " +
                $"player={owner.transform.position}, core={cameraRig.CoreMode}, " +
                $"carry={cameraRig.CarryMode}, throw={cameraRig.IsThrowWindup}, " +
                $"move={logicalInput.Snapshot.Move}, " +
                $"mobilityLocks={owner.GetComponent<ThirdPersonLocomotion>().MobilityLocks}.");
            Assert.That(
                routeProgress,
                Is.LessThanOrEqualTo(routeLength + 1f),
                "A single rendered frame overshot the soft-partition passage by more " +
                "than the bounded full-speed WASD allowance.");
            Assert.That(
                (maximumLocalOffset - minimumLocalOffset).magnitude,
                Is.LessThanOrEqualTo(0.005f),
                "Soft partition movement changed the StableAnchor-local camera pose: " +
                $"min={minimumLocalOffset}, max={maximumLocalOffset}.");
        }

        private static bool TryFindAdjacentPartitionSeam(
            Scene scene,
            Vector3 playerPosition,
            out Vector3 seamSurface,
            out Vector3 tangent,
            out Vector3 outward)
        {
            Collider[] partitions = GetSceneComponents<Collider>(scene)
                .Where(collider =>
                    collider != null &&
                    collider.gameObject.isStatic &&
                    collider.bounds.size.y >= 1f &&
                    collider.name.ToUpperInvariant().Contains("PARTITION") &&
                    collider.GetComponentInParent<ShowcaseCameraSoftOccluderRoot>() != null &&
                    collider.GetComponentInParent<ShowcaseCameraWorldBlockerRoot>() == null)
                .ToArray();
            seamSurface = default;
            tangent = default;
            outward = default;
            float bestScore = float.PositiveInfinity;
            for (int first = 0; first < partitions.Length; first++)
            {
                Bounds a = partitions[first].bounds;
                bool aThinX = a.size.x <= a.size.z;
                float aThickness = aThinX ? a.size.x : a.size.z;
                if (aThickness > 1f)
                {
                    continue;
                }
                for (int second = first + 1; second < partitions.Length; second++)
                {
                    Bounds b = partitions[second].bounds;
                    bool bThinX = b.size.x <= b.size.z;
                    float bThickness = bThinX ? b.size.x : b.size.z;
                    if (aThinX != bThinX || bThickness > 1f)
                    {
                        continue;
                    }

                    float normalDelta = aThinX
                        ? Mathf.Abs(a.center.x - b.center.x)
                        : Mathf.Abs(a.center.z - b.center.z);
                    if (normalDelta > Mathf.Max(aThickness, bThickness) + 0.08f)
                    {
                        continue;
                    }

                    float aAlong = aThinX ? a.center.z : a.center.x;
                    float bAlong = aThinX ? b.center.z : b.center.x;
                    float aExtent = aThinX ? a.extents.z : a.extents.x;
                    float bExtent = aThinX ? b.extents.z : b.extents.x;
                    float centerSeparation = Mathf.Abs(aAlong - bAlong);
                    float edgeGap = centerSeparation - (aExtent + bExtent);
                    // The authored low-partition proxy leaves a 2 m passage
                    // between the west/east modules.  This is the real v007
                    // seam where the right-shoulder boom can alternate between
                    // the two coplanar collider edges.
                    if (edgeGap < 1.9f || edgeGap > 2.05f ||
                        centerSeparation < Mathf.Max(aExtent, bExtent) * 0.75f)
                    {
                        continue;
                    }

                    float aEdge = aAlong + Mathf.Sign(bAlong - aAlong) * aExtent;
                    float bEdge = bAlong + Mathf.Sign(aAlong - bAlong) * bExtent;
                    float seamAlong = (aEdge + bEdge) * 0.5f;
                    Vector3 normalAxis = aThinX ? Vector3.right : Vector3.forward;
                    Vector3 basePoint = (a.center + b.center) * 0.5f;
                    if (aThinX) basePoint.z = seamAlong;
                    else basePoint.x = seamAlong;
                    Vector3 candidateOutward =
                        Vector3.Dot(playerPosition - basePoint, normalAxis) >= 0f
                            ? normalAxis
                            : -normalAxis;
                    Vector3 candidateSurface = basePoint + candidateOutward *
                        (Mathf.Max(aThickness, bThickness) * 0.5f);
                    float score = Vector3.SqrMagnitude(
                        playerPosition - candidateSurface) +
                        Mathf.Abs(edgeGap - 2f);
                    if (score >= bestScore)
                    {
                        continue;
                    }

                    bestScore = score;
                    seamSurface = basePoint;
                    tangent = aThinX ? Vector3.forward : Vector3.right;
                    outward = candidateOutward;
                }
            }
            return bestScore < float.PositiveInfinity;
        }

        private static void PlacePlayerBesidePartition(
            PrototypeInteractionStateOwner owner,
            Collider partition,
            ThirdPersonCameraRig cameraRig)
        {
            CharacterController controller = owner.GetComponent<CharacterController>();
            Assert.That(controller, Is.Not.Null);

            Vector3 outward = owner.transform.position - partition.bounds.center;
            outward.y = 0f;
            if (outward.sqrMagnitude <= 0.0001f)
            {
                outward = partition.transform.forward;
                outward.y = 0f;
            }
            outward.Normalize();

            Vector3 surface = partition.ClosestPoint(
                partition.bounds.center + outward * 100f);
            Vector3 destination = surface + outward *
                (controller.radius + controller.skinWidth + 0.12f);
            destination.y = owner.transform.position.y;

            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            owner.transform.SetPositionAndRotation(
                destination,
                Quaternion.LookRotation(outward, Vector3.up));
            controller.enabled = wasEnabled;
            Physics.SyncTransforms();
            cameraRig.HardResetCameraState();
        }

        private static void PlacePlayerForCameraRoute(
            PrototypeInteractionStateOwner owner,
            CharacterController controller,
            Vector3 position,
            Quaternion rotation,
            ThirdPersonCameraRig cameraRig)
        {
            bool controllerWasEnabled = controller.enabled;
            controller.enabled = false;
            owner.transform.SetPositionAndRotation(position, rotation);
            controller.enabled = controllerWasEnabled;
            Physics.SyncTransforms();
            cameraRig.HardResetCameraState();
        }

        private static void MoveActorToPosition(
            Transform actor,
            Vector3 position,
            Quaternion rotation)
        {
            CharacterController controller =
                actor.GetComponentInChildren<CharacterController>(true);
            CapsuleCollider capsule =
                actor.GetComponentInChildren<CapsuleCollider>(true);
            bool controllerWasEnabled = controller != null && controller.enabled;
            bool capsuleWasEnabled = capsule != null && capsule.enabled;
            if (controller != null)
            {
                controller.enabled = false;
            }
            if (capsule != null)
            {
                capsule.enabled = false;
            }
            actor.SetPositionAndRotation(position, rotation);
            if (controller != null)
            {
                controller.enabled = controllerWasEnabled;
            }
            if (capsule != null)
            {
                capsule.enabled = capsuleWasEnabled;
            }
            Physics.SyncTransforms();
        }

        private static IEnumerator WalkByLogicalWASD(
            PrototypeInteractionStateOwner owner,
            ThirdPersonCameraRig cameraRig,
            InputSystemLogicalInputAdapter logicalInput,
            Vector3 destination,
            float timeoutSeconds,
            System.Action sampleFrame)
        {
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (PlanarDistance(owner.transform.position, destination) > 0.12f &&
                   Time.realtimeSinceStartup < deadline)
            {
                Vector3 direction = destination - owner.transform.position;
                direction.y = 0f;
                direction.Normalize();
                Vector2 move = WorldDirectionToMoveInput(
                    direction,
                    cameraRig.GameplayMovementReference);
                SetLogicalSnapshot(logicalInput, WithMove(logicalInput.Snapshot, move));
                yield return null;
                sampleFrame?.Invoke();
            }

            SetLogicalSnapshot(logicalInput, WithMove(logicalInput.Snapshot, Vector2.zero));
            Assert.That(
                PlanarDistance(owner.transform.position, destination),
                Is.LessThanOrEqualTo(0.2f),
                $"Actual logical WASD route timed out before {destination}; " +
                $"player={owner.transform.position}.");
        }

        private static Vector2 WorldDirectionToMoveInput(
            Vector3 worldDirection,
            Transform movementReference)
        {
            Assert.That(movementReference, Is.Not.Null);
            Vector3 forward = movementReference.forward;
            Vector3 right = movementReference.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            Vector2 move = new Vector2(
                Vector3.Dot(worldDirection, right),
                Vector3.Dot(worldDirection, forward));
            return move.sqrMagnitude > 1f ? move.normalized : move;
        }

        private static LogicalInputSnapshot WithMove(
            LogicalInputSnapshot source,
            Vector2 move)
        {
            return new LogicalInputSnapshot(
                move,
                source.Look,
                source.Aim,
                source.InteractPrimary,
                source.Throw,
                source.CallAlly,
                source.StrapRelease,
                source.ResetEncounter,
                source.ThrowHeld,
                source.ThrowReleased,
                source.AllyTask,
                source.RegroupAlly,
                source.Sprint,
                source.Jump,
                source.Restart);
        }

        private static float PlanarDistance(Vector3 first, Vector3 second)
        {
            first.y = 0f;
            second.y = 0f;
            return Vector3.Distance(first, second);
        }

        private static Vector3 BoomPerpendicularOffset(Vector3 localCameraOffset)
        {
            Vector3 boomAxis = Vector3.back;
            return localCameraOffset -
                Vector3.Dot(localCameraOffset, boomAxis) * boomAxis;
        }

        private static bool IsInHierarchy(Transform candidate, Transform root)
        {
            return candidate != null && root != null &&
                (candidate == root || candidate.IsChildOf(root));
        }

        private static IEnumerator MovePlayerWithoutCameraReset(
            PrototypeInteractionStateOwner owner,
            Vector3 destination,
            Quaternion destinationRotation,
            float maximumStep)
        {
            CharacterController controller = owner.GetComponent<CharacterController>();
            Assert.That(controller, Is.Not.Null);
            Vector3 source = owner.transform.position;
            Quaternion sourceRotation = owner.transform.rotation;
            int stepCount = Mathf.Max(
                1,
                Mathf.CeilToInt(Vector3.Distance(source, destination) /
                    Mathf.Max(0.1f, maximumStep)));
            bool wasEnabled = controller.enabled;
            for (int step = 1; step <= stepCount; step++)
            {
                float amount = step / (float)stepCount;
                controller.enabled = false;
                owner.transform.SetPositionAndRotation(
                    Vector3.Lerp(source, destination, amount),
                    Quaternion.Slerp(sourceRotation, destinationRotation, amount));
                controller.enabled = wasEnabled;
                Physics.SyncTransforms();
                yield return null;
            }
        }

        private static IEnumerator WaitForOpenCameraRecovery(
            ThirdPersonCameraRig cameraRig,
            float timeoutSeconds)
        {
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while ((cameraRig.HardConstraintActiveThisFrame ||
                    cameraRig.GameplayOverrideSpatialWeight < 0.999f ||
                    cameraRig.SolvedShoulderScale < 0.999f ||
                    cameraRig.SafeBoomFraction < 0.999f ||
                    cameraRig.CurrentActorProxy != null) &&
                   Time.realtimeSinceStartup < deadline)
            {
                yield return null;
                AssertSingleAuthorityFrame(cameraRig);
            }

            string context = DescribeGameplayOverride(cameraRig);
            Assert.That(cameraRig.HardConstraintActiveThisFrame, Is.False, context);
            Assert.That(cameraRig.GameplayOverrideSpatialWeight,
                Is.EqualTo(1f).Within(0.001f), context);
            Assert.That(cameraRig.SolvedShoulderScale,
                Is.EqualTo(1f).Within(0.001f), context);
            Assert.That(cameraRig.SafeBoomFraction,
                Is.EqualTo(1f).Within(0.001f), context);
            Assert.That(cameraRig.CurrentActorProxy, Is.Null, context);
            Assert.That(cameraRig.CurrentBlockingCollider, Is.Null, context);
        }

        private static void AssertSingleAuthorityFrame(
            ThirdPersonCameraRig cameraRig,
            ShowcaseCameraConstraintSolver solver = null)
        {
            string solverContext = solver == null
                ? "constraint solver diagnostics not requested"
                : $"queries={solver.PhysicsQueriesThisFrame}/" +
                  $"{solver.PhysicsQueryBudgetPerFrame}, " +
                  $"fullSolves={solver.FullSolvesThisFrame}, " +
                  $"tupleCommits={solver.TupleCommitsThisFrame}, " +
                  $"cornerCandidates={solver.CornerCandidateEvaluationsThisFrame}, " +
                  $"budgetExceeded={solver.QueryBudgetExceededThisFrame}, " +
                  $"family={solver.ActiveSolutionFamily}";
            Assert.That(
                cameraRig.ConstraintEvaluationsThisFrame,
                Is.EqualTo(1),
                "The camera constraint solver must evaluate exactly once per rendered frame.");
            Assert.That(
                cameraRig.PostBrainTransformWriteCount,
                Is.EqualTo(0),
                "No system may overwrite the output Camera transform after the Brain.");
            Assert.That(
                cameraRig.ConstraintStatus,
                Is.Not.EqualTo(ShowcaseCameraConstraintStatus.NoValidPose),
                $"No valid camera pose: {cameraRig.NoValidPoseReason}; {solverContext}.");
            Assert.That(
                cameraRig.ConstraintStatus,
                Is.Not.EqualTo(ShowcaseCameraConstraintStatus.DynamicActorNoSolution),
                $"No actor-safe camera pose: {cameraRig.NoValidPoseReason}.");
            Assert.That(
                IsFinite(cameraRig.OutputCamera.transform.position),
                Is.True,
                "The final output camera pose must remain finite.");
            if (solver != null)
            {
                Assert.That(
                    solver.PhysicsQueriesThisFrame,
                    Is.LessThanOrEqualTo(solver.PhysicsQueryBudgetPerFrame),
                    solverContext);
                Assert.That(
                    solver.FullSolvesThisFrame,
                    Is.LessThanOrEqualTo(1),
                    solverContext);
                Assert.That(
                    solver.TupleCommitsThisFrame,
                    Is.LessThanOrEqualTo(1),
                    solverContext);
                Assert.That(
                    solver.CornerCandidateEvaluationsThisFrame,
                    Is.LessThanOrEqualTo(1),
                    solverContext);
            }
        }

        private static IEnumerator AssertCameraRemainsStable(
            ThirdPersonCameraRig cameraRig,
            int sampledFrames,
            float positionPeakToPeakTolerance,
            float rotationPeakToPeakTolerance,
            ShowcaseCameraConstraintSolver solver = null)
        {
            // Deliberately sample the very next rendered frame. Waiting for a
            // settled branch hid entry, exit, seam and recovery oscillation.
            Vector3 minimum = cameraRig.OutputCamera.transform.position;
            Vector3 maximum = minimum;
            Quaternion baselineRotation = cameraRig.OutputCamera.transform.rotation;
            float maximumAngularDelta = 0f;
            float minimumGameplayWeight = cameraRig.GameplayOverrideSpatialWeight;
            float maximumGameplayWeight = minimumGameplayWeight;
            float minimumShoulderScale = cameraRig.SolvedShoulderScale;
            float maximumShoulderScale = minimumShoulderScale;
            float minimumBoomDistance = cameraRig.SolvedBoomDistance;
            float maximumBoomDistance = minimumBoomDistance;
            int maximumQueries = 0;
            int fullSolveFrames = 0;
            int budgetExceededFrames = 0;
            int familyTransitions = 0;
            ShowcaseCameraSolutionFamily previousFamily = solver != null
                ? solver.ActiveSolutionFamily
                : default;
            for (int frame = 0; frame < sampledFrames; frame++)
            {
                yield return null;
                AssertSingleAuthorityFrame(cameraRig, solver);
                if (solver != null)
                {
                    maximumQueries = Mathf.Max(
                        maximumQueries,
                        solver.PhysicsQueriesThisFrame);
                    if (solver.FullSolvesThisFrame > 0) fullSolveFrames++;
                    if (solver.QueryBudgetExceededThisFrame) budgetExceededFrames++;
                    if (solver.ActiveSolutionFamily != previousFamily)
                    {
                        familyTransitions++;
                        previousFamily = solver.ActiveSolutionFamily;
                    }
                }
                Vector3 position = cameraRig.OutputCamera.transform.position;
                minimum = Vector3.Min(minimum, position);
                maximum = Vector3.Max(maximum, position);
                maximumAngularDelta = Mathf.Max(
                    maximumAngularDelta,
                    Quaternion.Angle(
                        baselineRotation,
                        cameraRig.OutputCamera.transform.rotation));
                minimumGameplayWeight = Mathf.Min(
                    minimumGameplayWeight,
                    cameraRig.GameplayOverrideSpatialWeight);
                maximumGameplayWeight = Mathf.Max(
                    maximumGameplayWeight,
                    cameraRig.GameplayOverrideSpatialWeight);
                minimumShoulderScale = Mathf.Min(
                    minimumShoulderScale,
                    cameraRig.SolvedShoulderScale);
                maximumShoulderScale = Mathf.Max(
                    maximumShoulderScale,
                    cameraRig.SolvedShoulderScale);
                minimumBoomDistance = Mathf.Min(
                    minimumBoomDistance,
                    cameraRig.SolvedBoomDistance);
                maximumBoomDistance = Mathf.Max(
                    maximumBoomDistance,
                    cameraRig.SolvedBoomDistance);
            }

            Assert.That(
                (maximum - minimum).magnitude,
                Is.LessThanOrEqualTo(positionPeakToPeakTolerance),
                $"Camera position oscillated from {minimum} to {maximum}; " +
                $"maxQueries={maximumQueries}, fullSolveFrames={fullSolveFrames}, " +
                $"budgetExceededFrames={budgetExceededFrames}, " +
                $"familyTransitions={familyTransitions}.");
            Assert.That(
                maximumAngularDelta,
                Is.LessThanOrEqualTo(rotationPeakToPeakTolerance),
                "Camera rotation oscillated during immediate per-frame sampling.");
            Assert.That(
                maximumGameplayWeight - minimumGameplayWeight,
                Is.LessThanOrEqualTo(0.005f),
                "Atomic camera gameplay weight oscillated during sampling.");
            Assert.That(
                maximumShoulderScale - minimumShoulderScale,
                Is.LessThanOrEqualTo(0.005f),
                "Atomic camera shoulder scale oscillated during sampling.");
            Assert.That(
                maximumBoomDistance - minimumBoomDistance,
                Is.LessThanOrEqualTo(0.01f),
                "Atomic camera boom distance oscillated during sampling.");
        }

        private static void AssertAllCarryPairsIgnored(
            Collider[] playerColliders,
            Collider[] targetColliders)
        {
            foreach (Collider playerCollider in playerColliders)
            {
                foreach (Collider targetCollider in targetColliders)
                {
                    Assert.That(
                        Physics.GetIgnoreCollision(playerCollider, targetCollider),
                        Is.True,
                        $"Carry collision lease lost: {playerCollider.name} x {targetCollider.name}");
                }
            }
        }

        private static bool IsFinite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
                !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
                !float.IsNaN(value.z) && !float.IsInfinity(value.z);
        }

        private static T[] GetSceneComponents<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }

        private static void DisablePointerCapture(Scene scene)
        {
            foreach (MonoBehaviour capture in GetSceneComponents<MonoBehaviour>(scene)
                .Where(component =>
                    component.GetType().FullName ==
                    "BreakNotice.Contest.BrowserPointerCapture"))
            {
                capture.enabled = false;
            }
        }

        private static IEnumerator CarryAndAssert(
            ThirdPersonCameraRig cameraRig,
            PrototypeInteractionStateOwner owner,
            PrototypePhysicalObjectLifecycle lifecycle,
            PrototypeInteractionTarget target,
            CameraCarryMode expectedMode,
            float expectedDistance,
            bool assertCentralSafeArea = true)
        {
            PrototypeInteractionDecision decision = PrototypeInteractionDecision.Accept(
                PrototypeInteractionOutcome.Scoop,
                target);
            Assert.That(
                owner.TryCommit(decision),
                Is.True,
                $"global={owner.GlobalState}, hand={owner.HandState}, " +
                $"carried={owner.CarriedTarget?.name}, target={target.name}");
            Assert.That(owner.TryCompleteAcquisition(target), Is.True);
            Assert.That(lifecycle.BeginCarry(target), Is.True);
            yield return new WaitForSecondsRealtime(0.22f);
            yield return null;

            Assert.That(cameraRig.CarryMode, Is.EqualTo(expectedMode));
            Assert.That(cameraRig.IsThrowWindup, Is.False);
            Assert.That(cameraRig.CarryBlend, Is.EqualTo(1f).Within(0.02f));
            Assert.That(
                cameraRig.CurrentDistance,
                Is.EqualTo(expectedDistance).Within(0.03f),
                DescribeGameplayOverride(cameraRig));
            Assert.That(cameraRig.CurrentFieldOfView, Is.EqualTo(50f).Within(0.01f));
            // Renderer bounds treat the open space between chair legs as an
            // opaque rectangle.  The dedicated B composition test owns that
            // conservative proxy check; partition/Aim-release regressions must
            // be allowed to reach their actual stability assertions.
            if (assertCentralSafeArea)
            {
                AssertCentralSafeAreaClear(cameraRig.OutputCamera, target);
            }
        }

        private static IEnumerator ResetCarry(
            PrototypeInteractionStateOwner owner,
            PrototypePhysicalObjectLifecycle lifecycle,
            PrototypeInteractionTarget target,
            Vector3 restoredPosition,
            Quaternion restoredRotation)
        {
            Assert.That(owner.TryBeginReset(), Is.True);
            lifecycle.DetachForReset();
            Assert.That(owner.RestoreInteractionBaseline(), Is.True);
            target.transform.SetPositionAndRotation(restoredPosition, restoredRotation);
            Rigidbody body = target.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.position = restoredPosition;
                body.rotation = restoredRotation;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
            Physics.SyncTransforms();
            yield return new WaitForSecondsRealtime(0.22f);
            yield return null;
        }

        private static void AssertCentralSafeAreaClear(
            Camera camera,
            PrototypeInteractionTarget target)
        {
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true)
                .Where(renderer => renderer.enabled)
                .ToArray();
            Assert.That(renderers, Is.Not.Empty);
            Rect rect = CalculateViewportRect(camera, renderers);
            float closestX = Mathf.Clamp(0.5f, rect.xMin, rect.xMax);
            float closestY = Mathf.Clamp(0.5f, rect.yMin, rect.yMax);
            float normalizedX = (closestX - 0.5f) / 0.03375f;
            float normalizedY = (closestY - 0.5f) / 0.06f;
            Assert.That(
                normalizedX * normalizedX + normalizedY * normalizedY,
                Is.GreaterThanOrEqualTo(1f),
                $"{target.name} viewport rect {rect} overlaps the 6%-of-height center safe circle.");
        }

        private static Rect CalculateViewportRect(Camera camera, Renderer[] renderers)
        {
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            foreach (Renderer renderer in renderers)
            {
                Bounds bounds = renderer.bounds;
                Vector3 center = bounds.center;
                Vector3 extents = bounds.extents;
                for (int x = -1; x <= 1; x += 2)
                {
                    for (int y = -1; y <= 1; y += 2)
                    {
                        for (int z = -1; z <= 1; z += 2)
                        {
                            Vector3 viewport = camera.WorldToViewportPoint(
                                center + Vector3.Scale(extents, new Vector3(x, y, z)));
                            if (viewport.z <= 0f)
                            {
                                continue;
                            }
                            minX = Mathf.Min(minX, viewport.x);
                            minY = Mathf.Min(minY, viewport.y);
                            maxX = Mathf.Max(maxX, viewport.x);
                            maxY = Mathf.Max(maxY, viewport.y);
                        }
                    }
                }
            }
            Assert.That(float.IsInfinity(minX), Is.False);
            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        private static void QueueGamepadState(Gamepad gamepad, GamepadState state)
        {
            InputSystem.QueueStateEvent(gamepad, state);
            InputSystem.Update();
        }

        private static void SetLogicalSnapshot(
            InputSystemLogicalInputAdapter logicalInput,
            LogicalInputSnapshot value)
        {
            PropertyInfo snapshot = typeof(InputSystemLogicalInputAdapter).GetProperty(
                nameof(InputSystemLogicalInputAdapter.Snapshot),
                BindingFlags.Instance | BindingFlags.Public);
            MethodInfo setter = snapshot?.GetSetMethod(true);
            Assert.That(setter, Is.Not.Null);
            setter.Invoke(logicalInput, new object[] { value });
        }

        private static IEnumerator EnsureShowcaseSceneUnloaded()
        {
            Scene existing = SceneManager.GetSceneByPath(ScenePath);
            if (!existing.IsValid() || !existing.isLoaded)
            {
                yield break;
            }

            AsyncOperation unload = SceneManager.UnloadSceneAsync(existing);
            while (unload != null && !unload.isDone)
            {
                yield return null;
            }
            yield return null;
        }

        private static string DescribeGameplayOverride(ThirdPersonCameraRig cameraRig)
        {
            Collider blocker = cameraRig.GameplayOverrideBlockingCollider;
            return
                $"mode={cameraRig.CoreMode}/{cameraRig.CarryMode}, " +
                $"throw={cameraRig.IsThrowWindup}, carryBlend={cameraRig.CarryBlend:0.000}, " +
                $"throwBlend={cameraRig.ThrowWindupBlend:0.000}, " +
                $"spatialWeight={cameraRig.GameplayOverrideSpatialWeight:0.000}, " +
                $"shoulderScale={cameraRig.SolvedShoulderScale:0.000}, " +
                $"safeBoom={cameraRig.SafeBoomFraction:0.000}, " +
                $"hardNow={cameraRig.HardConstraintActiveThisFrame}, " +
                $"status={cameraRig.ConstraintStatus}, reason={cameraRig.NoValidPoseReason}, " +
                $"distance={cameraRig.CurrentDistance:0.000}, " +
                $"blocker={(blocker != null ? blocker.name : "<none>")}, " +
                $"blockerLayer={(blocker != null ? blocker.gameObject.layer : -1)}.";
        }

        private static string DescribeSolverFrame(
            ThirdPersonCameraRig cameraRig,
            ShowcaseCameraConstraintSolver solver)
        {
            Collider blocker = solver.ActiveHardBlocker;
            return
                $"frame={Time.frameCount}, mode={cameraRig.CoreMode}/{cameraRig.CarryMode}, " +
                $"throw={cameraRig.IsThrowWindup}, phase={solver.StaticContactPhase}, " +
                $"episode={solver.ContactEpisodeId}, restores={solver.RestoreStartCount}, " +
                $"subjectPhase={solver.SubjectContactPhase}, " +
                $"subjectEpisode={solver.SubjectContactEpisodeId}, " +
                $"subjectRestores={solver.SubjectRestoreStartCount}, " +
                $"subjectBoom={solver.SubjectResolvedBoomDistance:0.000}, " +
                $"requestedBoom={solver.RequestedBoomDistance:0.000}, " +
                $"resolvedBoom={solver.ResolvedBoomDistance:0.000}, " +
                $"gameplayWeight={solver.ResolvedGameplayWeight:0.000}, " +
                $"shoulderScale={solver.ResolvedShoulderScale:0.000}, " +
                $"emergencyShoulder={solver.EmergencyShoulderLatched}, " +
                $"hard={solver.HardConstraintActiveThisFrame}, " +
                $"hardBlocker={(blocker != null ? blocker.name : "<none>")}, " +
                $"softCount={solver.ActiveSoftOccluderCount}, " +
                $"stableLocal={solver.FinalStableAnchorLocalCameraOffset}.";
        }

        private static string DescribeLandmarks(
            ThirdPersonCameraRig cameraRig,
            Vector3 head,
            Vector3 shoulder,
            Vector3 hips,
            Vector3 feet)
        {
            return
                $"mode={cameraRig.CoreMode}, fov={cameraRig.CurrentFieldOfView:0.00}, " +
                $"distance={cameraRig.CurrentDistance:0.00}, " +
                $"shoulderOffset={cameraRig.CurrentShoulderOffset}, " +
                $"head={head}, shoulder={shoulder}, hips={hips}, feet={feet}, " +
                $"camera={cameraRig.OutputCamera.transform.position}.";
        }
    }
}
