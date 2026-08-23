using System.Collections;
using System.Linq;
using BreakNotice.Contest;
using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestStage01EnvironmentIntegrationPlayModeTests
    {
        [UnityTearDown]
        public IEnumerator TearDownLoadedScenes()
        {
            Scene cleanup = SceneManager.CreateScene(
                "STAGE01_INTEGRATION_TEST_CLEANUP");
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
        public IEnumerator Bootstrap_LoadsFinalMapVisuals_WhileV007KeepsPlayerGrounded()
        {
            float timeScaleBeforeBootstrap = Time.timeScale;
            bool audioPauseBeforeBootstrap = AudioListener.pause;

            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                ShowcaseStage01Bootstrap.BootstrapScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));

            float deadline = Time.realtimeSinceStartup + 180f;
            ShowcaseStage01Bootstrap bootstrap = null;
            while (Time.realtimeSinceStartup < deadline)
            {
                bootstrap = Object.FindFirstObjectByType<ShowcaseStage01Bootstrap>(
                    FindObjectsInactive.Include);
                if (bootstrap != null &&
                    (bootstrap.CompositionCompleted ||
                     !string.IsNullOrEmpty(bootstrap.CompositionFailure)))
                {
                    break;
                }

                yield return null;
            }

            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(
                bootstrap.CompositionFailure,
                Is.Empty,
                bootstrap.CompositionFailure);
            Assert.That(bootstrap.CompositionCompleted, Is.True);
            Assert.That(bootstrap.LoadingCurtainCamera, Is.Not.Null);
            Assert.That(bootstrap.LoadingCurtainActive, Is.False);
            Assert.That(bootstrap.LoadingCurtainCamera.enabled, Is.False);
            Assert.That(
                bootstrap.LoadingCurtainCamera.clearFlags,
                Is.EqualTo(CameraClearFlags.SolidColor));
            Assert.That(
                bootstrap.LoadingCurtainCamera.backgroundColor,
                Is.EqualTo(Color.black));
            Assert.That(bootstrap.LoadingCurtainCamera.cullingMask, Is.EqualTo(0));
            Assert.That(
                bootstrap.LoadingCurtainCamera.depth,
                Is.EqualTo(ShowcaseStage01Bootstrap.LoadingCurtainCameraDepth));
            Assert.That(bootstrap.GameplaySceneActivatedUnderCurtain, Is.True);
            Assert.That(bootstrap.EnvironmentSceneActivatedUnderCurtain, Is.True);
            Assert.That(
                bootstrap.GameplaySceneActivationFrame,
                Is.GreaterThanOrEqualTo(0));
            Assert.That(
                bootstrap.EnvironmentSceneActivationFrame,
                Is.GreaterThanOrEqualTo(
                    bootstrap.GameplaySceneActivationFrame));
            Assert.That(
                bootstrap.FinalMapPreparedFrame,
                Is.GreaterThanOrEqualTo(
                    bootstrap.EnvironmentSceneActivationFrame));
            Assert.That(
                bootstrap.PresentationReleasedFrame,
                Is.GreaterThan(bootstrap.FinalMapPreparedFrame),
                "The final gameplay Camera must remain behind the black curtain " +
                "for one complete frame after Stage01 preparation.");
            Assert.That(
                bootstrap.SuspendedGameplayCameraCount,
                Is.GreaterThan(0),
                "At least the authored gameplay Camera must be suspended during load.");
            Assert.That(bootstrap.GameplayPresentationSuspended, Is.False);
            Assert.That(bootstrap.StartupSimulationPaused, Is.False);
            Assert.That(
                Time.timeScale,
                Is.EqualTo(timeScaleBeforeBootstrap).Within(0.0001f));
            Assert.That(AudioListener.pause, Is.EqualTo(audioPauseBeforeBootstrap));

            Scene gameplay = ShowcaseStage01Bootstrap.ResolveLoadedScene(
                ShowcaseStage01Bootstrap.GameplaySceneName,
                ShowcaseStage01Bootstrap.GameplayScenePath);
            Scene environment = ShowcaseStage01Bootstrap.ResolveLoadedScene(
                ShowcaseStage01Bootstrap.EnvironmentSceneName,
                ShowcaseStage01Bootstrap.EnvironmentScenePath);
            Assert.That(gameplay.IsValid() && gameplay.isLoaded, Is.True);
            Assert.That(environment.IsValid() && environment.isLoaded, Is.True);
            Assert.That(
                SceneManager.GetActiveScene().name,
                Is.EqualTo(ShowcaseStage01Bootstrap.GameplaySceneName));

            Assert.That(
                bootstrap.AppliedEnvironmentOffset.x,
                Is.EqualTo(7.39f).Within(0.02f));
            Assert.That(
                bootstrap.AppliedEnvironmentOffset.y,
                Is.EqualTo(0f).Within(0.02f));
            Assert.That(
                bootstrap.AppliedEnvironmentOffset.z,
                Is.EqualTo(40.955f).Within(0.02f));
            Assert.That(
                bootstrap.PreservedLegacyColliderCount,
                Is.EqualTo(1),
                "Only the v007 support floor may remain from the old map; " +
                "Stage01 now owns wall, partition, furniture and prop collision.");
            if (GraphicsSettings.currentRenderPipeline == null)
            {
                Assert.That(
                    bootstrap.ConvertedStageMaterialCount,
                    Is.GreaterThan(0),
                    "Stage01 URP materials must be converted for the Built-in project.");
            }

            ShowcasePlayerCombatController playerCombat =
                SceneComponents<ShowcasePlayerCombatController>(gameplay).Single();
            PrototypeInteractionController interaction =
                SceneComponents<PrototypeInteractionController>(gameplay).Single();
            ShowcaseCargoStrapController cargoStrap =
                SceneComponents<ShowcaseCargoStrapController>(gameplay).Single();
            ShowcaseRunnerPressureController runnerPressure =
                SceneComponents<ShowcaseRunnerPressureController>(gameplay).Single();
            ThirdPersonLocomotion locomotion =
                playerCombat.GetComponent<ThirdPersonLocomotion>();
            CharacterController playerController =
                playerCombat.GetComponent<CharacterController>();

            Assert.That(playerCombat.enabled, Is.True);
            Assert.That(interaction.enabled, Is.True);
            Assert.That(cargoStrap.enabled, Is.True);
            Assert.That(runnerPressure.enabled, Is.True);
            Assert.That(locomotion, Is.Not.Null);
            Assert.That(playerController, Is.Not.Null);
            Assert.That(playerCombat.gameObject.scene, Is.EqualTo(gameplay));
            Assert.That(
                SceneComponents<Camera>(gameplay)
                    .Any(camera =>
                        camera != null &&
                        camera.enabled &&
                        camera.gameObject.activeInHierarchy),
                Is.True,
                "The gameplay Camera must be restored only after final presentation release.");

            GameObject legacyRoot = gameplay.GetRootGameObjects().Single(root =>
                root.name == ShowcaseStage01Bootstrap.LegacyEnvironmentRootName);
            Assert.That(legacyRoot.activeInHierarchy, Is.True);
            Assert.That(
                legacyRoot.GetComponentsInChildren<Renderer>(true)
                    .All(renderer => renderer == null || !renderer.enabled),
                Is.True,
                "The provisional map may keep collision but may not remain visible.");

            Collider legacyFloor = legacyRoot
                .GetComponentsInChildren<Collider>(true)
                .SingleOrDefault(collider =>
                    collider != null &&
                    collider.name == ShowcaseStage01Bootstrap.LegacyFloorColliderName);
            Assert.That(legacyFloor, Is.Not.Null);
            Assert.That(legacyFloor.enabled, Is.True);
            Assert.That(legacyFloor.gameObject.activeInHierarchy, Is.True);
            Collider[] enabledLegacyColliders = legacyRoot
                .GetComponentsInChildren<Collider>(true)
                .Where(collider =>
                    collider != null &&
                    collider.enabled &&
                    collider.gameObject.activeInHierarchy)
                .ToArray();
            Assert.That(
                enabledLegacyColliders,
                Is.EqualTo(new[] { legacyFloor }),
                "Every old wall, desk, cubicle and proxy collider must be removed; " +
                "only PROXY_Floor_64x44 may remain.");
            AssertHorizontalContainment(
                legacyFloor.bounds,
                playerCombat.transform.position,
                playerCombat.name);

            Transform allyAnchors = legacyRoot.transform.Find("07_S2_MOBILE_ALLY");
            Assert.That(allyAnchors, Is.Not.Null);
            Assert.That(allyAnchors.gameObject.activeInHierarchy, Is.True);

            Collider[] finalMapColliders = SceneComponents<Collider>(environment);
            Assert.That(
                finalMapColliders.Length,
                Is.EqualTo(bootstrap.TotalFinalMapColliderCount));
            Assert.That(
                finalMapColliders.Count(collider =>
                    collider != null && collider.enabled),
                Is.EqualTo(bootstrap.EnabledFinalMapColliderCount));
            Assert.That(
                finalMapColliders.Count(collider =>
                    collider != null && !collider.enabled),
                Is.EqualTo(bootstrap.DisabledFinalMapColliderCount));
            Assert.That(
                bootstrap.EnabledFinalMapColliderCount,
                Is.GreaterThan(40),
                "Stage01 must provide real collision instead of a visual-only map.");
            Assert.That(
                bootstrap.EnabledFinalMapColliderCount,
                Is.LessThan(bootstrap.TotalFinalMapColliderCount / 2),
                "The complete office collider set may not remain registered in " +
                "PhysX; ordinary props must be spatially streamed.");
            Assert.That(
                bootstrap.ActiveStreamedStageColliderCount,
                Is.GreaterThan(0),
                "At least the props near CHR00 must enter the streamed collision set.");
            Assert.That(
                bootstrap.GeneratedFinalMapColliderCount,
                Is.GreaterThan(200),
                "The pinned FBX imports have addColliders disabled, so runtime " +
                "authoring must create the missing collider set.");

            GameObject[] stageRoots = environment.GetRootGameObjects();
            GameObject[] chairRoots = stageRoots
                .Where(root =>
                    root != null &&
                    root.name.StartsWith(
                        ShowcaseStage01Bootstrap.StageChairNamePrefix,
                        System.StringComparison.OrdinalIgnoreCase))
                .ToArray();
            GameObject[] binderRoots = stageRoots
                .Where(root =>
                    root != null &&
                    root.name.StartsWith(
                        ShowcaseStage01Bootstrap.StageBinderNamePrefix,
                        System.StringComparison.OrdinalIgnoreCase))
                .ToArray();
            Assert.That(chairRoots, Is.Not.Empty);
            Assert.That(binderRoots, Is.Not.Empty);
            Assert.That(
                bootstrap.StageChairTargetCount,
                Is.EqualTo(chairRoots.Length));
            Assert.That(
                bootstrap.StageBinderRootCount,
                Is.EqualTo(binderRoots.Length));
            Assert.That(
                bootstrap.DormantStageBinderCount +
                bootstrap.PromotedStageBinderCount,
                Is.EqualTo(binderRoots.Length));
            Assert.That(
                bootstrap.RegisteredInteractionTargetCount,
                Is.GreaterThanOrEqualTo(chairRoots.Length + 10));

            Assert.That(
                chairRoots.All(root =>
                {
                    BoxCollider collider = root.GetComponent<BoxCollider>();
                    Rigidbody body = root.GetComponent<Rigidbody>();
                    PrototypeInteractionTarget target =
                        root.GetComponent<PrototypeInteractionTarget>();
                    return collider != null &&
                           collider.enabled &&
                           body != null &&
                           body.isKinematic &&
                           !body.useGravity &&
                           target != null &&
                           target.Role == PrototypeInteractionRole.DirectChair &&
                           target.WeightClass == PrototypeWeightClass.Medium &&
                           target.IsScoopEligible &&
                           target.AllowDeferredAcquisitionContact;
                }),
                Is.True,
                "Every Stage01 office Chair must be a normal Medium pickup target " +
                "whose collision-safe pose may converge after the contact event.");
            int allocatedBinderColliders = binderRoots.Count(root =>
                root.GetComponent<BoxCollider>() != null);
            Assert.That(
                allocatedBinderColliders,
                Is.EqualTo(
                    bootstrap.ActiveDormantBinderColliderCount +
                    bootstrap.PromotedStageBinderCount),
                "Dormant Binders must remain bounds-only; Collider allocation is " +
                "reserved for exact reticle promotion.");
            Assert.That(
                bootstrap.ActiveDormantBinderColliderCount,
                Is.Zero,
                "Nearby decorative Binders may not allocate dormant PhysX shapes.");
            Assert.That(
                allocatedBinderColliders,
                Is.LessThan(binderRoots.Length / 10),
                "Thousands of decorative Binder colliders may not be allocated at startup.");

            PrototypeInteractionTarget[] promotedBinders =
                SceneComponents<PrototypeInteractionTarget>(environment)
                    .Where(target =>
                        target != null &&
                        target.Role == PrototypeInteractionRole.DirectBinder)
                    .ToArray();
            Assert.That(
                promotedBinders.Length,
                Is.EqualTo(bootstrap.PromotedStageBinderCount));
            Assert.That(
                promotedBinders.All(target =>
                    target.WeightClass == PrototypeWeightClass.Light &&
                    target.IsScoopEligible &&
                    target.GetComponent<Rigidbody>() != null &&
                    target.GetComponent<BoxCollider>() != null),
                Is.True);

            Transform visualFloor = SceneComponents<Transform>(environment)
                .Where(item =>
                    item != null &&
                    item.name == ShowcaseStage01Bootstrap.EnvironmentFloorObjectName)
                .OrderByDescending(FootprintArea)
                .FirstOrDefault();
            Assert.That(visualFloor, Is.Not.Null);
            Assert.That(visualFloor.position.x, Is.EqualTo(0f).Within(0.02f));
            Assert.That(visualFloor.position.y, Is.EqualTo(0f).Within(0.02f));
            Assert.That(visualFloor.position.z, Is.EqualTo(0f).Within(0.02f));

            Renderer[] liveStageRenderers = SceneComponents<Renderer>(environment)
                .Where(renderer =>
                    renderer != null &&
                    renderer.enabled &&
                    renderer.gameObject.activeInHierarchy)
                .ToArray();
            Renderer[] allStageRenderers = SceneComponents<Renderer>(environment)
                .Where(renderer =>
                    renderer != null &&
                    renderer.gameObject.activeInHierarchy)
                .ToArray();
            Assert.That(liveStageRenderers.Length, Is.GreaterThan(50));
            Assert.That(
                liveStageRenderers.Length,
                Is.LessThan(allStageRenderers.Length / 2),
                "The complete six-thousand-object office may not remain rendered " +
                "at once; decorative roots must be distance streamed.");
            Assert.That(
                bootstrap.TotalStreamedStageVisualRootCount,
                Is.GreaterThan(500));
            Assert.That(
                bootstrap.ActiveStageVisualRootCount,
                Is.LessThan(bootstrap.TotalStreamedStageVisualRootCount));
            Assert.That(
                bootstrap.ActiveStageBinderVisualCount,
                Is.LessThan(binderRoots.Length / 2),
                "Thousands of Binder renderers may not remain visible across the map.");
            Bounds stageVisualBounds = CombineRendererBounds(allStageRenderers);
            Bounds supportBounds = legacyFloor.bounds;
            float floorMargin = ShowcaseStage01Bootstrap.LegacyFloorCoverageMargin;
            Assert.That(
                supportBounds.min.x,
                Is.LessThanOrEqualTo(stageVisualBounds.min.x - floorMargin + 0.1f));
            Assert.That(
                supportBounds.max.x,
                Is.GreaterThanOrEqualTo(stageVisualBounds.max.x + floorMargin - 0.1f));
            Assert.That(
                supportBounds.min.z,
                Is.LessThanOrEqualTo(stageVisualBounds.min.z - floorMargin + 0.1f));
            Assert.That(
                supportBounds.max.z,
                Is.GreaterThanOrEqualTo(stageVisualBounds.max.z + floorMargin - 0.1f));
            Assert.That(
                supportBounds.size.x > 64.05f || supportBounds.size.z > 44.05f,
                Is.True,
                "The original 64x44 support floor was not expanded for Stage01.");
            Assert.That(
                Vector3.Distance(
                    bootstrap.ExpandedLegacyFloorBounds.center,
                    supportBounds.center),
                Is.LessThan(0.001f));
            Assert.That(
                Vector3.Distance(
                    bootstrap.ExpandedLegacyFloorBounds.size,
                    supportBounds.size),
                Is.LessThan(0.001f));
            if (GraphicsSettings.currentRenderPipeline == null)
            {
                Assert.That(
                    liveStageRenderers
                        .SelectMany(renderer => renderer.sharedMaterials)
                        .Where(material => material != null)
                        .All(material =>
                            material.shader != null &&
                            material.shader.isSupported &&
                            !material.shader.name.StartsWith(
                                "Universal Render Pipeline/")),
                    Is.True,
                    "No visible Stage01 renderer may remain on an unsupported URP shader.");
            }

            Assert.That(
                SceneComponents<Camera>(environment)
                    .All(camera => camera == null || !camera.enabled),
                Is.True);
            Assert.That(
                SceneComponents<AudioListener>(environment)
                    .All(listener => listener == null || !listener.enabled),
                Is.True);
            Assert.That(
                SceneComponents<Animator>(environment)
                    .All(animator => animator == null || !animator.enabled),
                Is.True,
                "The art-scene preview character must not duplicate CHR00.");

            Light[] allStageLights = SceneComponents<Light>(environment)
                .Where(light =>
                    light != null &&
                    light.gameObject.activeInHierarchy)
                .ToArray();
            Light[] stageLights = allStageLights
                .Where(light =>
                    light.enabled &&
                    light.gameObject.activeInHierarchy)
                .ToArray();
            Assert.That(allStageLights.Length, Is.EqualTo(bootstrap.TotalStageLightCount));
            Assert.That(stageLights.Length, Is.EqualTo(bootstrap.ActiveStageLightCount));
            Assert.That(stageLights.Length, Is.GreaterThan(0));
            Assert.That(stageLights.Length, Is.LessThan(allStageLights.Length));
            Assert.That(
                allStageLights.All(light => light.shadows == LightShadows.None),
                Is.True,
                "Stage01 local lights must remain shadow-free under the runtime budget.");

            float initialPlayerY = playerCombat.transform.position.y;
            for (int frame = 0; frame < 45; frame++)
            {
                yield return new WaitForFixedUpdate();
            }

            float finalPlayerY = playerCombat.transform.position.y;
            float playerBottomY = ResolveCharacterBottomY(playerController);
            Assert.That(
                finalPlayerY,
                Is.GreaterThan(0.5f),
                "CHR00 fell through the preserved v007 support floor.");
            Assert.That(
                Mathf.Abs(finalPlayerY - initialPlayerY),
                Is.LessThan(0.15f),
                "CHR00 vertical position drifted after Stage01 composition.");
            Assert.That(
                playerBottomY,
                Is.GreaterThanOrEqualTo(legacyFloor.bounds.max.y - 0.08f));
            Assert.That(
                locomotion.IsGrounded,
                Is.True,
                "CHR00 must remain grounded after the final-map visuals load.");

            ShowcaseRunnerAgent[] activeRunners =
                SceneComponents<ShowcaseRunnerAgent>(gameplay)
                    .Where(runner =>
                        runner != null &&
                        !runner.DisablesInBatchMode &&
                        runner.gameObject.activeInHierarchy)
                    .ToArray();
            Assert.That(activeRunners, Is.Not.Empty);
            for (int index = 0; index < activeRunners.Length; index++)
            {
                ShowcaseRunnerAgent runner = activeRunners[index];
                Transform runnerVisual = runner.transform.Find(
                    "RUNTIME_DESIGNER_A_VISUAL");
                Assert.That(runnerVisual, Is.Not.Null, runner.RunnerId);
                Assert.That(
                    runner.PresentationScaleResolved,
                    Is.True,
                    runner.RunnerId);
                Assert.That(
                    Mathf.Abs(runner.PresentationGroundGap),
                    Is.LessThanOrEqualTo(0.01f),
                    $"{runner.RunnerId} animated shoe sole was not locked to " +
                    $"the physical floor. gap={runner.PresentationGroundGap:0.000} m.");
                Assert.That(
                    Mathf.Abs(runner.PresentationCalibrationOffsetY),
                    Is.LessThanOrEqualTo(0.451f),
                    $"{runner.RunnerId} received an unsafe floor correction of " +
                    $"{runner.PresentationCalibrationOffsetY:0.000} m.");
                Assert.That(
                    runnerVisual.localPosition.y,
                    Is.InRange(-0.451f, 0.451f),
                    $"{runner.RunnerId} visual root was shifted far enough to " +
                    "float or bury the complete mesh.");
                Assert.That(
                    runner.PresentationFootCalibrationResolved,
                    Is.True,
                    $"{runner.RunnerId} did not resolve animated foot grounding.");

                float actualSoleY = ResolveActualSkinnedSoleY(runner.transform);
                Assert.That(
                    actualSoleY,
                    Is.InRange(
                        supportBounds.max.y - 0.015f,
                        supportBounds.max.y + 0.02f),
                    $"{runner.RunnerId} visible shoe mesh was not on the floor. " +
                    $"soleY={actualSoleY:0.000}; floorY={supportBounds.max.y:0.000}.");
            }

            Vector3[] stageEdgeProbes =
            {
                new Vector3(stageVisualBounds.min.x, 0f, stageVisualBounds.min.z),
                new Vector3(stageVisualBounds.min.x, 0f, stageVisualBounds.max.z),
                new Vector3(stageVisualBounds.max.x, 0f, stageVisualBounds.min.z),
                new Vector3(stageVisualBounds.max.x, 0f, stageVisualBounds.max.z)
            };
            for (int index = 0; index < stageEdgeProbes.Length; index++)
            {
                var probeObject = new GameObject($"STAGE01_FLOOR_EDGE_PROBE_{index:00}");
                SceneManager.MoveGameObjectToScene(probeObject, gameplay);
                CharacterController probe =
                    probeObject.AddComponent<CharacterController>();
                probe.radius = 0.25f;
                probe.height = 1.8f;
                probe.center = new Vector3(0f, 0.9f, 0f);
                probe.stepOffset = 0.2f;
                probe.skinWidth = 0.02f;
                probeObject.transform.position = new Vector3(
                    stageEdgeProbes[index].x,
                    supportBounds.max.y + 2.5f,
                    stageEdgeProbes[index].z);
                Physics.SyncTransforms();

                for (int step = 0; step < 24; step++)
                {
                    probe.Move(Vector3.down * 0.2f);
                    yield return new WaitForFixedUpdate();
                }

                float probeBottomY = ResolveCharacterBottomY(probe);
                Assert.That(
                    probeBottomY,
                    Is.InRange(
                        supportBounds.max.y - 0.06f,
                        supportBounds.max.y + 0.12f),
                    $"Stage01 edge probe {index} fell through at " +
                    $"({stageEdgeProbes[index].x:0.00}, {stageEdgeProbes[index].z:0.00}).");
                Assert.That(
                    probe.isGrounded,
                    Is.True,
                    $"Stage01 edge probe {index} was not grounded.");

                Object.Destroy(probeObject);
                yield return null;
            }

            // Exercise the actual Stage01 Binder acquisition path. Dormant Binders
            // have no Collider, so the bootstrap must resolve the optical ray from
            // its spatial bounds, promote exactly one target, register it with the
            // existing interaction controller and allow the ordinary E/Scoop
            // transaction to commit.
            runnerPressure.enabled = false;
            for (int index = 0; index < activeRunners.Length; index++)
            {
                activeRunners[index].enabled = false;
            }

            PrototypePhysicalObjectLifecycle lifecycle =
                playerCombat.GetComponent<PrototypePhysicalObjectLifecycle>();
            PrototypeInteractionTarget resolvedBinder = null;
            Ray resolvedBinderRay = default;
            float originalPlayerY = playerCombat.transform.position.y;
            bool controllerWasEnabled = playerController.enabled;
            bool locomotionWasEnabled = locomotion.enabled;
            playerController.enabled = false;
            locomotion.enabled = false;

            foreach (GameObject binderRoot in binderRoots
                         .OrderBy(root => Vector3.SqrMagnitude(
                             CombineRendererBounds(
                                 root.GetComponentsInChildren<Renderer>(true)).center -
                             playerCombat.transform.position))
                         .Take(128))
            {
                Bounds binderBounds = CombineRendererBounds(
                    binderRoot.GetComponentsInChildren<Renderer>(true));
                Vector3 approach = Vector3.ProjectOnPlane(
                    playerCombat.transform.position - binderBounds.center,
                    Vector3.up);
                if (approach.sqrMagnitude <= 0.0001f)
                {
                    approach = Vector3.back;
                }
                approach.Normalize();

                playerCombat.transform.position = new Vector3(
                    binderBounds.center.x + approach.x * 1.35f,
                    originalPlayerY,
                    binderBounds.center.z + approach.z * 1.35f);
                Physics.SyncTransforms();
                bootstrap.RefreshRuntimeSpatialStateNow();

                Vector3 rayOrigin = playerCombat.transform.position + Vector3.up;
                Vector3 rayDirection = binderBounds.center - rayOrigin;
                if (rayDirection.sqrMagnitude <= 0.0001f)
                {
                    continue;
                }

                var candidateRay = new Ray(
                    rayOrigin,
                    rayDirection.normalized);
                PrototypeInteractionTarget candidate =
                    bootstrap.ResolveRuntimeStageBinderTarget(
                        candidateRay,
                        interaction.InteractionRange);
                if (candidate == null)
                {
                    continue;
                }

                resolvedBinder = candidate;
                resolvedBinderRay = candidateRay;
                break;
            }

            Assert.That(resolvedBinder, Is.Not.Null,
                "No visible Stage01 Binder could be promoted from the exact " +
                "reticle ray within normal interaction range.");
            Assert.That(resolvedBinder.WeightClass,
                Is.EqualTo(PrototypeWeightClass.Light));
            Assert.That(resolvedBinder.Role,
                Is.EqualTo(PrototypeInteractionRole.DirectBinder));
            Assert.That(resolvedBinder.IsScoopEligible, Is.True);
            Assert.That(resolvedBinder.AllowDeferredAcquisitionContact, Is.True);
            Assert.That(resolvedBinder.GetComponent<BoxCollider>(), Is.Not.Null);
            Assert.That(resolvedBinder.GetComponent<Rigidbody>(), Is.Not.Null);

            Transform view = interaction.ViewTransform;
            Assert.That(view, Is.Not.Null);
            view.SetPositionAndRotation(
                resolvedBinderRay.origin,
                Quaternion.LookRotation(resolvedBinderRay.direction, Vector3.up));
            Physics.SyncTransforms();
            Assert.That(
                interaction.RefreshTarget(false),
                Is.SameAs(resolvedBinder),
                "The promoted Binder must immediately become the ordinary center-" +
                "reticle interaction target.");
            interaction.SendMessage(
                "LateUpdate",
                SendMessageOptions.DontRequireReceiver);

            PrototypeInteractionDecision binderDecision =
                interaction.ProcessInteractPrimary(false);
            Assert.That(binderDecision.IsAccepted, Is.True);
            Assert.That(binderDecision.Outcome,
                Is.EqualTo(PrototypeInteractionOutcome.Scoop));
            Assert.That(binderDecision.Target, Is.SameAs(resolvedBinder));

            playerCombat.CommitAnimationCarryContact();
            Assert.That(lifecycle.CarriedTarget, Is.SameAs(resolvedBinder),
                "The Stage01 Binder E transaction did not commit physical carry.");
            playerCombat.CompleteAnimationAction();
            Assert.That(playerCombat.IsActionLocked, Is.False);

            locomotion.enabled = locomotionWasEnabled;
            playerController.enabled = controllerWasEnabled;
        }

        private static float ResolveCharacterBottomY(CharacterController controller)
        {
            Vector3 scale = controller.transform.lossyScale;
            float radius = controller.radius * Mathf.Max(
                Mathf.Abs(scale.x),
                Mathf.Abs(scale.z));
            float height = Mathf.Max(
                controller.height * Mathf.Abs(scale.y),
                radius * 2f);
            Vector3 center = controller.transform.TransformPoint(controller.center);
            return center.y - height * 0.5f;
        }

        private static float FootprintArea(Transform root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return 0f;
            }

            Renderer first = renderers.FirstOrDefault(renderer => renderer != null);
            if (first == null)
            {
                return 0f;
            }

            Bounds bounds = first.bounds;
            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer != null)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return bounds.size.x * bounds.size.z;
        }

        private static Bounds CombineRendererBounds(Renderer[] renderers)
        {
            Renderer first = renderers.FirstOrDefault(renderer => renderer != null);
            Assert.That(first, Is.Not.Null);

            Bounds bounds = first.bounds;
            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer != null)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return bounds;
        }

        private static void AssertHorizontalContainment(
            Bounds bounds,
            Vector3 point,
            string label)
        {
            const float Margin = 0.5f;
            Assert.That(
                point.x,
                Is.InRange(bounds.min.x - Margin, bounds.max.x + Margin),
                $"{label} x={point.x} was outside {bounds}.");
            Assert.That(
                point.z,
                Is.InRange(bounds.min.z - Margin, bounds.max.z + Margin),
                $"{label} z={point.z} was outside {bounds}.");
        }

        private static float ResolveActualSkinnedSoleY(Transform actor)
        {
            Transform visual = actor.Find("RUNTIME_DESIGNER_A_VISUAL");
            Assert.That(visual, Is.Not.Null, actor.name);
            SkinnedMeshRenderer[] renderers =
                visual.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                    .Where(renderer =>
                        renderer != null &&
                        renderer.enabled &&
                        renderer.gameObject.activeInHierarchy)
                    .ToArray();
            Assert.That(renderers, Is.Not.Empty, actor.name);

            float minimumY = float.PositiveInfinity;
            for (int rendererIndex = 0;
                 rendererIndex < renderers.Length;
                 rendererIndex++)
            {
                SkinnedMeshRenderer renderer = renderers[rendererIndex];
                var baked = new Mesh();
                try
                {
                    renderer.BakeMesh(baked, true);
                    Vector3[] vertices = baked.vertices;
                    for (int vertexIndex = 0;
                         vertexIndex < vertices.Length;
                         vertexIndex++)
                    {
                        float worldY = renderer.transform
                            .TransformPoint(vertices[vertexIndex]).y;
                        minimumY = Mathf.Min(minimumY, worldY);
                    }
                }
                finally
                {
                    Object.Destroy(baked);
                }
            }

            return minimumY;
        }

        private static T[] SceneComponents<T>(Scene scene)
            where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }
    }
}
