using System;
using System.Collections.Generic;
using System.Linq;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestDesignerACharacterIntegrationTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v007.unity";
        private const string VisualName = "RUNTIME_DESIGNER_A_VISUAL";
        private const string ControllerRoot =
            "Assets/_Project/Art/Animation/DesignerAControllers";
        private const string AnimationRoot =
            "Assets/_Project/Art/Animation";

        private Scene scene;

        [SetUp]
        public void SetUp()
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath), Is.Not.Null);
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        }

        [TearDown]
        public void TearDown()
        {
            if (scene.IsValid() && scene.isLoaded)
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void ShowcaseScene_UsesFourUniqueTexturedHumanoidRuntimeVisuals()
        {
            ShowcaseCharacterAnimationDriver[] drivers = Components<
                ShowcaseCharacterAnimationDriver>();

            Assert.That(drivers, Has.Length.EqualTo(4));
            Assert.That(drivers.Select(driver => driver.Role), Is.Unique);
            Assert.That(drivers.Select(driver => driver.Role), Is.EquivalentTo(new[]
            {
                ShowcaseCharacterAnimationRole.Player,
                ShowcaseCharacterAnimationRole.Ally,
                ShowcaseCharacterAnimationRole.EnemyNormal,
                ShowcaseCharacterAnimationRole.EnemyElite
            }));

            foreach (ShowcaseCharacterAnimationDriver driver in drivers)
            {
                Transform visual = driver.transform.Find(VisualName);
                Assert.That(visual, Is.Not.Null, driver.Role.ToString());
                Assert.That(visual.childCount, Is.EqualTo(1), driver.Role.ToString());
                Transform model = visual.GetChild(0);
                Assert.That(Quaternion.Angle(model.localRotation, Quaternion.identity),
                    Is.LessThan(0.01f), driver.Role.ToString());
                Assert.That(Vector3.Dot(model.forward, driver.transform.forward),
                    Is.GreaterThan(0.999f), driver.Role.ToString());
                Assert.That(driver.IsRuntimeConfigured, Is.True, driver.Role.ToString());
                Assert.That(driver.Animator.applyRootMotion, Is.False, driver.Role.ToString());
                Assert.That(driver.Animator.stabilizeFeet, Is.False, driver.Role.ToString());
                Assert.That(driver.Animator.avatar, Is.Not.Null, driver.Role.ToString());
                Assert.That(driver.Animator.avatar.isValid, Is.True, driver.Role.ToString());
                Assert.That(driver.Animator.avatar.isHuman, Is.True, driver.Role.ToString());

                Renderer[] productionRenderers = visual.GetComponentsInChildren<Renderer>(true);
                Assert.That(productionRenderers, Is.Not.Empty, driver.Role.ToString());
                Assert.That(productionRenderers.All(renderer => renderer.enabled), Is.True,
                    driver.Role.ToString());
                Assert.That(productionRenderers.SelectMany(renderer => renderer.sharedMaterials)
                    .Where(material => material != null)
                    .All(material => material.mainTexture != null), Is.True,
                    driver.Role.ToString());

                Physics.SyncTransforms();
                RaycastHit groundHit = Physics.RaycastAll(
                        driver.transform.position + Vector3.up * 2f,
                        Vector3.down,
                        8f,
                        Physics.DefaultRaycastLayers,
                        QueryTriggerInteraction.Ignore)
                    .Where(hit => !hit.collider.transform.IsChildOf(driver.transform))
                    .OrderBy(hit => hit.distance)
                    .FirstOrDefault();
                Assert.That(groundHit.collider, Is.Not.Null, driver.Role.ToString());
                float visualBottomY = productionRenderers.Min(renderer => renderer.bounds.min.y);
                float authoredPrecompensation = visualBottomY - groundHit.point.y;
                Assert.That(authoredPrecompensation,
                    Is.InRange(-0.02f, 0.02f),
                    driver.Role + " authored visual must start on the floor.");

                Renderer[] grayboxRenderers = driver.GetComponentsInChildren<Renderer>(true)
                    .Where(renderer => !renderer.transform.IsChildOf(visual))
                    .ToArray();
                Assert.That(grayboxRenderers.All(renderer => !renderer.enabled), Is.True,
                    driver.Role.ToString());
            }

            ShowcaseBoneCombatRig[] rigs = Components<ShowcaseBoneCombatRig>();
            Assert.That(rigs, Has.Length.EqualTo(4));
            Assert.That(rigs.All(rig => rig.Animator != null && rig.Animator.isHuman), Is.True);
            Assert.That(rigs.Count(rig => rig.Team == ShowcaseCombatTeam.Player), Is.EqualTo(1));
            Assert.That(rigs.Count(rig => rig.Team == ShowcaseCombatTeam.Ally), Is.EqualTo(1));
            Assert.That(rigs.Count(rig => rig.Team == ShowcaseCombatTeam.Enemy), Is.EqualTo(2));
        }

        [Test]
        public void RuntimeControllers_AreMinimalCodeOwnedPlaybackEngines()
        {
            foreach ((string fileName, int stateCount, bool expectsRun) in new[]
                     {
                         ("CHR00_Player.controller", 29, true),
                         ("CHR01_Ally.controller", 5, true),
                         ("ENY01_Normal.controller", 17, false),
                         ("ENY01_Elite.controller", 20, false)
                     })
            {
                AnimatorController controller = LoadController(fileName);
                AnimatorState[] states = controller.layers
                    .SelectMany(layer => layer.stateMachine.states)
                    .Select(child => child.state)
                    .ToArray();
                AnimatorState idle = states.Single(state => state.name == "Idle");
                AnimatorState walk = states.Single(state => state.name == "Walk");

                Assert.That(states, Has.Length.EqualTo(stateCount), fileName);
                Assert.That(states.Any(state => state.name == "Run"),
                    Is.EqualTo(expectsRun), fileName);
                Assert.That(idle.iKOnFeet, Is.False, fileName);
                Assert.That(walk.iKOnFeet, Is.False, fileName);
                Assert.That(walk.speedParameterActive, Is.True, fileName);
                Assert.That(walk.speedParameter, Is.EqualTo("LocomotionPlayback"), fileName);
                Assert.That(controller.parameters.Any(parameter =>
                    parameter.name == "LocomotionPlayback" &&
                    parameter.type == AnimatorControllerParameterType.Float), Is.True, fileName);
                Assert.That(controller.parameters.Select(parameter => parameter.name),
                    Is.EquivalentTo(new[] { "Speed", "LocomotionPlayback" }), fileName);
                Assert.That(controller.parameters.All(parameter =>
                    parameter.type == AnimatorControllerParameterType.Float), Is.True, fileName);
                Assert.That(states.SelectMany(state => state.transitions), Is.Empty, fileName);
                Assert.That(controller.layers.SelectMany(layer =>
                    layer.stateMachine.anyStateTransitions), Is.Empty, fileName);
            }
        }

        [Test]
        public void PlayerController_UsesDirectionalAimBaseStatesAndMaskedUpperBodyLayer()
        {
            AnimatorController controller = LoadController("CHR00_Player.controller");
            Assert.That(controller.layers, Has.Length.EqualTo(2));

            AnimatorControllerLayer baseLayer = controller.layers[0];
            AnimatorControllerLayer upperBodyLayer = controller.layers[1];
            Assert.That(baseLayer.name, Is.EqualTo("Base Layer"));
            Assert.That(baseLayer.iKPass, Is.True);
            Assert.That(baseLayer.stateMachine.states, Has.Length.EqualTo(28));

            Dictionary<string, AnimatorState> baseStates = baseLayer.stateMachine.states
                .Select(child => child.state)
                .ToDictionary(state => state.name, StringComparer.Ordinal);
            foreach (string stateName in new[]
                     {
                         "AimForward",
                         "AimBackward",
                         "AimStrafeLeft",
                         "AimStrafeRight",
                         "WalkStopAction"
                     })
            {
                Assert.That(baseStates, Contains.Key(stateName));
            }

            AssertStateMotionPath(
                baseStates["AimForward"],
                AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Walk_F.fbx");
            AssertStateMotionPath(
                baseStates["AimBackward"],
                AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Walk_F.fbx");
            Assert.That(baseStates["AimBackward"].speed, Is.EqualTo(1f).Within(0.001f));
            Assert.That(baseStates["AimBackward"].cycleOffset,
                Is.EqualTo(1f).Within(0.001f));
            AssertStateMotionPath(
                baseStates["AimStrafeLeft"],
                AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Strafe_L.fbx");
            AssertStateMotionPath(
                baseStates["AimStrafeRight"],
                AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Strafe_R.fbx");
            AssertStateMotionPath(
                baseStates["WalkStopAction"],
                AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Walk_Stop.fbx");

            foreach (string locomotionState in new[]
                     {
                         "AimForward",
                         "AimBackward",
                         "AimStrafeLeft",
                         "AimStrafeRight"
                     })
            {
                AnimatorState state = baseStates[locomotionState];
                Assert.That(state.speedParameterActive, Is.True, locomotionState);
                Assert.That(state.speedParameter, Is.EqualTo("LocomotionPlayback"),
                    locomotionState);
            }
            Assert.That(baseStates["WalkStopAction"].speedParameterActive, Is.False);

            Assert.That(upperBodyLayer.name, Is.EqualTo("Aim Upper Body"));
            Assert.That(upperBodyLayer.blendingMode,
                Is.EqualTo(AnimatorLayerBlendingMode.Override));
            Assert.That(upperBodyLayer.defaultWeight, Is.EqualTo(0f).Within(0.001f));
            Assert.That(upperBodyLayer.iKPass, Is.False);
            Assert.That(upperBodyLayer.avatarMask, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(upperBodyLayer.avatarMask),
                Is.EqualTo(ControllerRoot + "/CHR00_AimUpperBody.mask"));
            Assert.That(upperBodyLayer.stateMachine.states, Has.Length.EqualTo(1));
            Assert.That(upperBodyLayer.stateMachine.defaultState, Is.Not.Null);
            Assert.That(upperBodyLayer.stateMachine.defaultState.name,
                Is.EqualTo("AimUpperBodyPose"));
            Assert.That(upperBodyLayer.stateMachine.states[0].state.transitions, Is.Empty);
            Assert.That(upperBodyLayer.stateMachine.anyStateTransitions, Is.Empty);

            AvatarMask mask = upperBodyLayer.avatarMask;
            foreach (AvatarMaskBodyPart part in new[]
                     {
                         AvatarMaskBodyPart.Body,
                         AvatarMaskBodyPart.Head,
                         AvatarMaskBodyPart.LeftArm,
                         AvatarMaskBodyPart.RightArm,
                         AvatarMaskBodyPart.LeftFingers,
                         AvatarMaskBodyPart.RightFingers
                     })
            {
                Assert.That(mask.GetHumanoidBodyPartActive(part), Is.True, part.ToString());
            }
            foreach (AvatarMaskBodyPart part in new[]
                     {
                         AvatarMaskBodyPart.Root,
                         AvatarMaskBodyPart.LeftLeg,
                         AvatarMaskBodyPart.RightLeg,
                         AvatarMaskBodyPart.LeftFootIK,
                         AvatarMaskBodyPart.RightFootIK
                     })
            {
                Assert.That(mask.GetHumanoidBodyPartActive(part), Is.False, part.ToString());
            }

            foreach (AnimatorState state in controller.layers
                         .SelectMany(layer => layer.stateMachine.states)
                         .Select(child => child.state))
            {
                string path = AssetDatabase.GetAssetPath(state.motion);
                Assert.That(path.IndexOf("PREVIEW", StringComparison.OrdinalIgnoreCase),
                    Is.EqualTo(-1), state.name);
                Assert.That(path.IndexOf("_ALT", StringComparison.OrdinalIgnoreCase),
                    Is.EqualTo(-1), state.name);
                Assert.That(path.IndexOf("Turn", StringComparison.OrdinalIgnoreCase),
                    Is.EqualTo(-1), state.name);
            }
        }

        [Test]
        public void PlayerPropGripRig_IsAuthoredOnAnimatorAndOwnedByPhysicalLifecycle()
        {
            ShowcaseCharacterAnimationDriver player = Components<
                    ShowcaseCharacterAnimationDriver>()
                .Single(driver => driver.Role == ShowcaseCharacterAnimationRole.Player);
            ShowcasePlayerPropGripRig rig =
                player.Animator.GetComponent<ShowcasePlayerPropGripRig>();
            PrototypePhysicalObjectLifecycle lifecycle =
                player.GetComponent<PrototypePhysicalObjectLifecycle>();
            ShowcaseBinderGripSocket lightSocket =
                player.GetComponent<ShowcaseBinderGripSocket>();

            Assert.That(rig, Is.Not.Null);
            Assert.That(lifecycle, Is.Not.Null);
            Assert.That(lightSocket, Is.Not.Null);

            var rigObject = new SerializedObject(rig);
            Assert.That(rigObject.FindProperty("animator").objectReferenceValue,
                Is.SameAs(player.Animator));
            Assert.That(rigObject.FindProperty("actorRoot").objectReferenceValue,
                Is.SameAs(player.transform));
            Assert.That(rigObject.FindProperty("carryAnchor").objectReferenceValue,
                Is.SameAs(player.transform.Find("ContestCarryAnchor")));
            Assert.That(rigObject.FindProperty("stableLightSocket").objectReferenceValue,
                Is.SameAs(lightSocket));

            var lifecycleObject = new SerializedObject(lifecycle);
            Assert.That(lifecycleObject.FindProperty("playerPropGripRig")
                .objectReferenceValue, Is.SameAs(rig));
        }

        [Test]
        public void PlayerObjectActions_UseExactClipsAndSingleOrderedOwnedEvents()
        {
            AnimatorController controller = LoadController("CHR00_Player.controller");
            Dictionary<string, AnimatorState> states = controller.layers[0].stateMachine.states
                .Select(child => child.state)
                .ToDictionary(state => state.name, StringComparer.Ordinal);
            foreach ((string state, string path) in new[]
                     {
                         ("CatchLightAction", AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Catch_Small_Center.fbx"),
                         ("CatchSmallLeftAction", AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Catch_Small_Diving_L.fbx"),
                         ("CatchSmallRightAction", AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Catch_Small_Diving_R.fbx"),
                         ("CatchMediumAction", AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Catch_Medium.fbx"),
                         ("ThrowLightAction", AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Throw_Small.fbx"),
                         ("ThrowMediumAction", AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Throw_Medium.fbx")
                     })
            {
                AssertStateMotionPath(states[state], path);
            }

            foreach (string path in new[]
                     {
                         AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Catch_Small_Center.fbx",
                         AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Catch_Small_Diving_L.fbx",
                         AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Catch_Small_Diving_R.fbx",
                         AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Catch_Medium.fbx"
                     })
            {
                AssertOwnedEventPair(
                    path,
                    "ShowcaseCarryContact",
                    0.52f,
                    "ShowcaseActionComplete",
                    0.88f);
            }

            AssertOwnedEventPair(
                AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Throw_Small.fbx",
                "ShowcaseThrowRelease",
                0.48f,
                "ShowcaseActionComplete",
                0.88f);
            AssertOwnedEventPair(
                AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Throw_Medium.fbx",
                "ShowcaseThrowRelease",
                0.52f,
                "ShowcaseActionComplete",
                0.88f);
        }

        [Test]
        public void PlayerAndEnemyControllers_ExposeApprovedCombatStateContracts()
        {
            AnimatorController player = LoadController("CHR00_Player.controller");
            string[] playerStates = StateNames(player);
            foreach (string state in new[]
                     {
                         "Run", "JumpAction", "MeleeJabAction", "MeleeCrossAction",
                         "MeleeHookAction", "HitBodyAction", "KnockdownBackAction",
                         "KnockdownFrontAction", "GetupBackAction", "GetupFrontAction",
                         "Death1Action", "Death2Action"
                     })
            {
                Assert.That(playerStates, Does.Contain(state));
            }

            foreach (string fileName in new[]
                     {
                         "ENY01_Normal.controller", "ENY01_Elite.controller"
                     })
            {
                string[] states = StateNames(LoadController(fileName));
                foreach (string state in new[]
                         {
                             "AttackBoxingAction", "AttackBoxing2Action",
                             "AttackKickingAction", "AttackKicking2Action",
                             "HitBodyAction", "HitRibAction", "HitStomachAction",
                             "HitBigHeadAction", "Death1Action", "Death2Action"
                         })
                {
                    Assert.That(states, Does.Contain(state), fileName);
                }
            }

            string[] normalStates = StateNames(LoadController("ENY01_Normal.controller"));
            string[] eliteStates = StateNames(LoadController("ENY01_Elite.controller"));
            Assert.That(normalStates, Does.Not.Contain("EliteMutantPunchAction"));
            Assert.That(normalStates, Does.Not.Contain("EliteMutantSwipingAction"));
            Assert.That(normalStates, Does.Not.Contain("EliteJumpAttackAction"));
            Assert.That(eliteStates, Does.Contain("EliteMutantPunchAction"));
            Assert.That(eliteStates, Does.Contain("EliteMutantSwipingAction"));
            Assert.That(eliteStates, Does.Contain("EliteJumpAttackAction"));
        }

        [Test]
        public void LocomotionPlayback_UsesStateSpecificNaturalCadence()
        {
            ShowcaseCharacterAnimationDriver driver = Components<
                    ShowcaseCharacterAnimationDriver>()
                .Single(candidate =>
                    candidate.Role == ShowcaseCharacterAnimationRole.Player);

            Assert.That(driver.CalculateLocomotionPlaybackRate(0f), Is.EqualTo(1f));
            Assert.That(driver.CalculateLocomotionPlaybackRate(2.8f, 2.8f),
                Is.EqualTo(1f).Within(0.001f));
            Assert.That(driver.CalculateLocomotionPlaybackRate(3.2f, 3.2f),
                Is.EqualTo(1f).Within(0.001f));
            Assert.That(driver.CalculateLocomotionPlaybackRate(5.5f, 5.5f),
                Is.EqualTo(1f).Within(0.001f));
            Assert.That(driver.CalculateLocomotionPlaybackRate(6f, 4.2f),
                Is.EqualTo(6f / 4.2f).Within(0.001f));
        }

        [Test]
        public void RuntimeAnimationEvents_AreSingleInRangeAndPreserveCompletionOrder()
        {
            foreach ((string path, string clipName, string functionName, float normalizedTime) in new[]
            {
                (AnimationRoot + "/CHR00/DesignerA/Legacy/Fist Fight A.fbx", "CHR00_Melee_Jab_Runtime", "ShowcaseAttackWindowOpen", 0.42f),
                (AnimationRoot + "/CHR00/DesignerA/Legacy/Fist Fight A.fbx", "CHR00_Melee_Jab_Runtime", "ShowcaseAttackWindowClose", 0.82f),
                (AnimationRoot + "/CHR00/DesignerA/Legacy/Fist Fight A.fbx", "CHR00_Melee_Cross_Runtime", "ShowcaseAttackWindowOpen", 0.42f),
                (AnimationRoot + "/CHR00/DesignerA/Legacy/Hook Punch.fbx", null, "ShowcaseAttackWindowOpen", 0.38f),
                (AnimationRoot + "/CHR00/DesignerA/Legacy/Fist Fight A.fbx", "CHR00_Melee_Jab_Runtime", "ShowcaseComboBufferOpen", 0.52f),
                (AnimationRoot + "/CHR00/DesignerA/Legacy/Fist Fight A.fbx", "CHR00_Melee_Cross_Runtime", "ShowcaseComboBufferClose", 0.76f),
                (AnimationRoot + "/ENY01/DesignerA/MeleeCandidates/Melee_candidate_Boxing.fbx", null, "ShowcaseAttackWindowOpen", 0.36f),
                (AnimationRoot + "/ENY01/DesignerA/EliteOnly/lod@Jump Attack.fbx", null, "ShowcaseAttackWindowClose", 0.60f),
                (AnimationRoot + "/ENY01/DesignerA/EliteOnly/lod@Jump Attack.fbx", null, "ShowcaseLungeStart", 0.24f),
                (AnimationRoot + "/ENY01/DesignerA/EliteOnly/lod@Jump Attack.fbx", null, "ShowcaseLungeEnd", 0.62f),
                (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Catch_Medium.fbx", null, "ShowcaseCarryContact", 0.52f),
                (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Throw_Small.fbx", null, "ShowcaseThrowRelease", 0.48f),
                (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Throw_Medium.fbx", null, "ShowcaseThrowRelease", 0.52f),
                (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Knockdown_B.fbx", null, "ShowcaseActionComplete", 0.88f),
                (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Getup_B.fbx", null, "ShowcaseActionComplete", 0.88f),
                (AnimationRoot + "/ENY01/DesignerA/ApprovedHit/Hit_Body.fbx", null, "ShowcaseActionComplete", 0.88f),
                (AnimationRoot + "/CHR01/DesignerA/Approved/CHR01_Supply_Binder_Release.fbx", null, "ShowcaseAllySupplyRelease", 0.48f)
            })
            {
                AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path)
                    .OfType<AnimationClip>()
                    .Single(candidate => !candidate.name.StartsWith("__preview__") &&
                        (string.IsNullOrEmpty(clipName) || candidate.name == clipName));
                AnimationEvent[] matches = AnimationUtility.GetAnimationEvents(clip)
                    .Where(animationEvent => animationEvent.functionName == functionName)
                    .ToArray();
                Assert.That(matches, Has.Length.EqualTo(1), path + " / " + functionName);
                float tolerance = Mathf.Max(0.001f, 1f / Mathf.Max(1f, clip.frameRate));
                Assert.That(matches[0].time,
                    Is.EqualTo(clip.length * normalizedTime).Within(tolerance), path);
                Assert.That(matches[0].time, Is.LessThanOrEqualTo(clip.length + tolerance), path);
            }
        }

        [Test]
        public void RuntimeControllers_ExcludeReviewCatalogAndBrokenReadyClip()
        {
            foreach (string fileName in new[]
                     {
                         "CHR00_Player.controller", "CHR01_Ally.controller",
                         "ENY01_Normal.controller", "ENY01_Elite.controller"
                     })
            {
                Assert.That(StateNames(LoadController(fileName)).Any(name =>
                    name.Contains("__", StringComparison.Ordinal)), Is.False, fileName);
            }

            AnimatorController ally = LoadController("CHR01_Ally.controller");
            Assert.That(ally.layers.SelectMany(layer => layer.stateMachine.states)
                .Select(child => child.state.motion)
                .OfType<AnimationClip>()
                .Any(clip => clip.name.Contains("Windup", StringComparison.OrdinalIgnoreCase)),
                Is.False);
        }

        [Test]
        public void EnemyEligibility_NormalExcludesAndEliteUsesOnlyThreeApprovedUnarmedAttacks()
        {
            AnimatorController normal = LoadController("ENY01_Normal.controller");
            AnimatorController elite = LoadController("ENY01_Elite.controller");

            string[] selected =
            {
                "EliteMutantPunchAction", "EliteMutantSwipingAction",
                "EliteJumpAttackAction"
            };
            Assert.That(StateNames(normal), Has.None.Matches<string>(name =>
                selected.Contains(name)));
            Assert.That(StateNames(elite), Does.Contain(selected[0]));
            Assert.That(StateNames(elite), Does.Contain(selected[1]));
            Assert.That(StateNames(elite), Does.Contain(selected[2]));
            Assert.That(StateNames(elite), Has.None.Matches<string>(name =>
                name.Contains("Walking", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("Stabbing", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("Club", StringComparison.OrdinalIgnoreCase)));
        }

        [Test]
        public void RuntimeActionSpeedsAndAllyChairIk_MatchApprovedCombatTuning()
        {
            AnimatorController player = LoadController("CHR00_Player.controller");
            AnimatorController ally = LoadController("CHR01_Ally.controller");
            AnimatorController normal = LoadController("ENY01_Normal.controller");
            AnimatorController elite = LoadController("ENY01_Elite.controller");

            AssertStateSpeed(player, "ThrowLightAction", 1.6f);
            AssertStateSpeed(player, "ThrowMediumAction", 1.5f);
            AssertStateSpeed(player, "MeleeHookAction", 1.4f);
            AssertStateSpeed(normal, "AttackBoxingAction", 1.25f);
            AssertStateSpeed(normal, "AttackBoxing2Action", 1.3f);
            AssertStateSpeed(normal, "AttackKickingAction", 1.2f);
            AssertStateSpeed(normal, "AttackKicking2Action", 1.2f);
            AssertStateSpeed(elite, "EliteMutantPunchAction", 1f);
            AssertStateSpeed(elite, "EliteMutantSwipingAction", 1f);
            AssertStateSpeed(elite, "EliteJumpAttackAction", 1f);
            Assert.That(ally.layers[0].iKPass, Is.True);

            ShowcaseCharacterAnimationDriver allyDriver = Components<
                    ShowcaseCharacterAnimationDriver>()
                .Single(driver => driver.Role == ShowcaseCharacterAnimationRole.Ally);
            ShowcaseChairGripRig chairGrip =
                allyDriver.Animator.GetComponent<ShowcaseChairGripRig>();
            Assert.That(chairGrip, Is.Not.Null);
        }

        [Test]
        public void BoneCombatRig_SweptSphereRequiresActualGeometricOverlap()
        {
            Assert.That(ShowcaseBoneCombatRig.SweptSphereOverlapsSphere(
                new Vector3(-2f, 0f, 0f),
                new Vector3(2f, 0f, 0f),
                0.1f,
                Vector3.zero,
                0.2f), Is.True, "Fast strike must not tunnel through a hurtbox.");
            Assert.That(ShowcaseBoneCombatRig.SweptSphereOverlapsSphere(
                new Vector3(-2f, 1f, 0f),
                new Vector3(2f, 1f, 0f),
                0.1f,
                Vector3.zero,
                0.2f), Is.False, "A near miss must not deal damage.");
        }

        [Test]
        public void PlayerMeleeRuntimeClips_ExposeMeasuredHandTrajectories()
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Art/Characters/CHR00/DesignerA/CHR00_Player.fbx");
            Assert.That(model, Is.Not.Null);
            GameObject instance = UnityEngine.Object.Instantiate(model);
            try
            {
                Animator animator = instance.GetComponentInChildren<Animator>(true);
                Assert.That(animator, Is.Not.Null);
                Assert.That(animator.isHuman, Is.True);
                foreach ((string path, string clipName, HumanBodyBones bone) in new[]
                         {
                             (AnimationRoot + "/CHR00/DesignerA/Legacy/Fist Fight A.fbx", "CHR00_Melee_Jab_Runtime", HumanBodyBones.LeftHand),
                             (AnimationRoot + "/CHR00/DesignerA/Legacy/Fist Fight A.fbx", "CHR00_Melee_Jab_Runtime", HumanBodyBones.RightHand),
                             (AnimationRoot + "/CHR00/DesignerA/Legacy/Fist Fight A.fbx", "CHR00_Melee_Cross_Runtime", HumanBodyBones.RightHand),
                             (AnimationRoot + "/CHR00/DesignerA/Legacy/Fist Fight A.fbx", "CHR00_Melee_Cross_Runtime", HumanBodyBones.LeftHand),
                             (AnimationRoot + "/CHR00/DesignerA/Legacy/Hook Punch.fbx", null, HumanBodyBones.RightHand),
                             (AnimationRoot + "/CHR00/DesignerA/Legacy/Hook Punch.fbx", null, HumanBodyBones.LeftHand)
                         })
                {
                    AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path)
                        .OfType<AnimationClip>()
                        .Single(candidate => !candidate.name.StartsWith("__preview__") &&
                            (string.IsNullOrEmpty(clipName) || candidate.name == clipName));
                    Transform hand = animator.GetBoneTransform(bone);
                    Assert.That(hand, Is.Not.Null, clip.name);
                    Vector3 min = Vector3.one * float.PositiveInfinity;
                    Vector3 max = Vector3.one * float.NegativeInfinity;
                    Vector3 windowMin = Vector3.one * float.PositiveInfinity;
                    Vector3 windowMax = Vector3.one * float.NegativeInfinity;
                    float maxForwardTime = 0f;
                    for (int sample = 0; sample <= 60; sample++)
                    {
                        clip.SampleAnimation(instance, clip.length * sample / 60f);
                        Vector3 local = instance.transform.InverseTransformPoint(hand.position);
                        min = Vector3.Min(min, local);
                        if (local.z >= max.z)
                        {
                            maxForwardTime = sample / 60f;
                        }
                        max = Vector3.Max(max, local);
                        if (sample >= 23 && sample <= 35)
                        {
                            windowMin = Vector3.Min(windowMin, local);
                            windowMax = Vector3.Max(windowMax, local);
                        }
                    }
                    TestContext.WriteLine(
                        $"{clip.name}/{bone}: length={clip.length:F3}, handMin={min}, " +
                        $"handMax={max}, maxForwardTime={maxForwardTime:F3}, " +
                        $"windowMin={windowMin}, windowMax={windowMax}");
                    Assert.That((max - min).magnitude, Is.GreaterThan(0.05f), clip.name);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void EnemyMeleeRuntimeClips_ExposeMeasuredStrikeTrajectories()
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Art/Characters/ENY01/DesignerA/ENY01_Normal.fbx");
            Assert.That(model, Is.Not.Null);
            GameObject instance = UnityEngine.Object.Instantiate(model);
            try
            {
                Animator animator = instance.GetComponentInChildren<Animator>(true);
                Assert.That(animator, Is.Not.Null);
                foreach ((string path, HumanBodyBones bone) in new[]
                         {
                             (AnimationRoot + "/ENY01/DesignerA/MeleeCandidates/Melee_candidate_Kicking.fbx", HumanBodyBones.RightFoot),
                             (AnimationRoot + "/ENY01/DesignerA/MeleeCandidates/Melee_candidate_Kicking.fbx", HumanBodyBones.LeftFoot),
                             (AnimationRoot + "/ENY01/DesignerA/MeleeCandidates/Melee_candidate_Kicking_2.fbx", HumanBodyBones.RightFoot),
                             (AnimationRoot + "/ENY01/DesignerA/MeleeCandidates/Melee_candidate_Kicking_2.fbx", HumanBodyBones.LeftFoot),
                             (AnimationRoot + "/ENY01/DesignerA/MeleeCandidates/Melee_candidate_Boxing.fbx", HumanBodyBones.RightHand)
                         })
                {
                    AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path)
                        .OfType<AnimationClip>()
                        .Single(candidate => !candidate.name.StartsWith("__preview__"));
                    Transform strike = animator.GetBoneTransform(bone);
                    Assert.That(strike, Is.Not.Null, clip.name);
                    Vector3 windowMin = Vector3.one * float.PositiveInfinity;
                    Vector3 windowMax = Vector3.one * float.NegativeInfinity;
                    Vector3 overallMax = Vector3.one * float.NegativeInfinity;
                    float maxForwardTime = 0f;
                    for (int sample = 0; sample <= 60; sample++)
                    {
                        clip.SampleAnimation(instance, clip.length * sample / 60f);
                        Vector3 local = instance.transform.InverseTransformPoint(strike.position);
                        if (local.z >= overallMax.z)
                        {
                            maxForwardTime = sample / 60f;
                        }
                        overallMax = Vector3.Max(overallMax, local);
                        if (sample >= 23 && sample <= 35)
                        {
                            windowMin = Vector3.Min(windowMin, local);
                            windowMax = Vector3.Max(windowMax, local);
                        }
                    }
                    TestContext.WriteLine(
                        $"{clip.name}/{bone}: length={clip.length:F3}, " +
                        $"maxForward={overallMax}, maxForwardTime={maxForwardTime:F3}, " +
                        $"windowMin={windowMin}, windowMax={windowMax}");
                    Assert.That((windowMax - windowMin).magnitude,
                        Is.GreaterThan(0.02f), clip.name);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void ImportedDelivery_ContainsFourModelsAndOneHundredSeventyTwoHumanoidClips()
        {
            string[] models = AssetDatabase.FindAssets(
                    "t:Model",
                    new[] { "Assets/_Project/Art/Characters" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.Contains("/DesignerA/", StringComparison.Ordinal))
                .ToArray();
            string[] animations = AssetDatabase.FindAssets(
                    "t:Model",
                    new[] { AnimationRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.Contains("/DesignerA/", StringComparison.Ordinal))
                .ToArray();

            Assert.That(models, Has.Length.EqualTo(4));
            Assert.That(animations, Has.Length.EqualTo(172));
            foreach (string path in models.Concat(animations))
            {
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                Assert.That(importer, Is.Not.Null, path);
                Assert.That(importer.animationType, Is.EqualTo(ModelImporterAnimationType.Human),
                    path);
            }
        }

        private void AssertCatalog(string fileName, string prefix, int expectedCount)
        {
            AnimatorController controller = LoadController(fileName);
            HashSet<AnimationClip> clips = new HashSet<AnimationClip>();
            foreach (AnimatorState state in controller.layers
                         .SelectMany(layer => layer.stateMachine.states)
                         .Select(child => child.state)
                         .Where(state => state.name.StartsWith(prefix + "__", StringComparison.Ordinal)))
            {
                Assert.That(state.motion, Is.TypeOf<AnimationClip>(), state.name);
                clips.Add((AnimationClip)state.motion);
            }

            Assert.That(clips, Has.Count.EqualTo(expectedCount), fileName);
        }

        private static AnimatorController LoadController(string fileName)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                ControllerRoot + "/" + fileName);
            Assert.That(controller, Is.Not.Null, fileName);
            return controller;
        }

        private static void AssertStateSpeed(
            AnimatorController controller,
            string stateName,
            float expected)
        {
            AnimatorState state = controller.layers
                .SelectMany(layer => layer.stateMachine.states)
                .Select(child => child.state)
                .Single(candidate => candidate.name == stateName);
            Assert.That(state.speed, Is.EqualTo(expected).Within(0.001f), stateName);
        }

        private static void AssertStateMotionPath(
            AnimatorState state,
            string expectedPath)
        {
            Assert.That(state.motion, Is.TypeOf<AnimationClip>(), state.name);
            Assert.That(AssetDatabase.GetAssetPath(state.motion), Is.EqualTo(expectedPath),
                state.name);
        }

        private static void AssertOwnedEventPair(
            string path,
            string firstFunction,
            float firstNormalizedTime,
            string secondFunction,
            float secondNormalizedTime)
        {
            AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .Single(candidate => !candidate.name.StartsWith("__preview__"));
            AnimationEvent[] events = AnimationUtility.GetAnimationEvents(clip);
            AnimationEvent[] first = events
                .Where(item => item.functionName == firstFunction)
                .ToArray();
            AnimationEvent[] second = events
                .Where(item => item.functionName == secondFunction)
                .ToArray();
            float tolerance = Mathf.Max(0.001f, 1f / Mathf.Max(1f, clip.frameRate));

            Assert.That(first, Has.Length.EqualTo(1), path + " / " + firstFunction);
            Assert.That(second, Has.Length.EqualTo(1), path + " / " + secondFunction);
            Assert.That(first[0].time,
                Is.EqualTo(clip.length * firstNormalizedTime).Within(tolerance), path);
            Assert.That(second[0].time,
                Is.EqualTo(clip.length * secondNormalizedTime).Within(tolerance), path);
            Assert.That(first[0].time, Is.LessThan(second[0].time), path);
        }

        private static string[] StateNames(AnimatorController controller)
        {
            return controller.layers
                .SelectMany(layer => layer.stateMachine.states)
                .Select(child => child.state.name)
                .ToArray();
        }

        private T[] Components<T>() where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }
    }
}
