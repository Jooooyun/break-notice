using System;
using System.Collections.Generic;
using System.IO;
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
                if (driver.Role == ShowcaseCharacterAnimationRole.EnemyNormal ||
                    driver.Role == ShowcaseCharacterAnimationRole.EnemyElite)
                {
                    Assert.That(
                        new Vector2(model.localPosition.x, model.localPosition.z).magnitude,
                        Is.LessThanOrEqualTo(0.001f),
                        driver.Role +
                        " Humanoid root must align with the gameplay actor on XZ.");
                }
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
                         ("CHR00_Player.controller", 49, true),
                         ("CHR01_Ally.controller", 5, true),
                         ("ENY01_Normal.controller", 17, true),
                         ("ENY01_Elite.controller", 19, true)
                     })
            {
                AnimatorController controller = LoadController(fileName);
                AnimatorState[] states = controller.layers
                    .SelectMany(layer => layer.stateMachine.states)
                    .Select(child => child.state)
                    .ToArray();
                AnimatorState idle = states.Single(state => state.name == "Idle");
                string locomotionStateName = fileName.StartsWith(
                    "ENY01_",
                    StringComparison.Ordinal)
                    ? "Run"
                    : "Walk";
                AnimatorState locomotion = states.Single(
                    state => state.name == locomotionStateName);

                Assert.That(states, Has.Length.EqualTo(stateCount), fileName);
                Assert.That(states.Any(state => state.name == "Run"),
                    Is.EqualTo(expectsRun), fileName);
                Assert.That(idle.iKOnFeet, Is.False, fileName);
                Assert.That(locomotion.iKOnFeet, Is.False, fileName);
                Assert.That(locomotion.speedParameterActive, Is.True, fileName);
                Assert.That(locomotion.speedParameter,
                    Is.EqualTo("LocomotionPlayback"), fileName);
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
        public void PlayerController_UsesFullBodyMeleeAndDirectionalMovingLegsLayer()
        {
            AnimatorController controller = LoadController("CHR00_Player.controller");
            Assert.That(controller.layers, Has.Length.EqualTo(4));

            AnimatorControllerLayer baseLayer = controller.layers[0];
            AnimatorControllerLayer upperBodyLayer = controller.layers[1];
            AnimatorControllerLayer strapUpperBodyLayer = controller.layers[2];
            AnimatorControllerLayer meleeMovingLegsLayer = controller.layers[3];
            Assert.That(baseLayer.name, Is.EqualTo("Base Layer"));
            Assert.That(baseLayer.iKPass, Is.True);
            Assert.That(baseLayer.stateMachine.states, Has.Length.EqualTo(38));

            Dictionary<string, AnimatorState> baseStates = baseLayer.stateMachine.states
                .Select(child => child.state)
                .ToDictionary(state => state.name, StringComparer.Ordinal);
            foreach (string stateName in new[]
                     {
                         "AimForward",
                         "AimBackward",
                         "AimStrafeLeft",
                         "AimStrafeRight",
                         "WalkStopAction",
                         "StrapPrepareAction",
                         "StrapRollingAction",
                         "StrapThrowAction",
                         "StrapPullAction",
                         "StrapTensionHold",
                         "StrapAttackLeftAction",
                         "StrapAttackRightAction"
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
            Assert.That(baseStates["StrapAttackLeftAction"].speed,
                Is.EqualTo(0.65f).Within(0.001f));
            Assert.That(baseStates["StrapAttackRightAction"].speed,
                Is.EqualTo(0.65f).Within(0.001f));

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

            Assert.That(strapUpperBodyLayer.name, Is.EqualTo("Strap Upper Body"));
            Assert.That(strapUpperBodyLayer.blendingMode,
                Is.EqualTo(AnimatorLayerBlendingMode.Override));
            Assert.That(strapUpperBodyLayer.defaultWeight,
                Is.EqualTo(0f).Within(0.001f));
            Assert.That(strapUpperBodyLayer.iKPass, Is.False);
            Assert.That(strapUpperBodyLayer.avatarMask, Is.SameAs(upperBodyLayer.avatarMask));
            Assert.That(strapUpperBodyLayer.stateMachine.states, Has.Length.EqualTo(4));
            Assert.That(strapUpperBodyLayer.stateMachine.defaultState, Is.Not.Null);
            Assert.That(strapUpperBodyLayer.stateMachine.defaultState.name,
                Is.EqualTo("StrapRollingUpperBodyPose"));
            Dictionary<string, AnimatorState> strapUpperBodyStates =
                strapUpperBodyLayer.stateMachine.states
                    .Select(child => child.state)
                    .ToDictionary(state => state.name, StringComparer.Ordinal);
            Assert.That(strapUpperBodyStates.Keys, Is.EquivalentTo(new[]
            {
                "StrapRollingUpperBodyPose",
                "StrapTensionUpperBodyPose",
                "StrapAttackLeftUpperBodyAction",
                "StrapAttackRightUpperBodyAction"
            }));
            AnimatorState strapRollingState =
                strapUpperBodyStates["StrapRollingUpperBodyPose"];
            AssertStateMotionPath(
                strapRollingState,
                AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Strap_Rolling.fbx");
            AnimatorState strapTensionState =
                strapUpperBodyStates["StrapTensionUpperBodyPose"];
            AssertStateMotionPath(
                strapTensionState,
                AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Strap_Pull.fbx");
            Assert.That(strapTensionState.motion.name,
                Is.EqualTo("CHR00_Strap_Tension_Hold_Runtime"));
            AnimatorState strapAttackLeft =
                strapUpperBodyStates["StrapAttackLeftUpperBodyAction"];
            AssertStateMotionPath(
                strapAttackLeft,
                AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Strap_Attack_L.fbx");
            Assert.That(strapAttackLeft.motion.name,
                Is.EqualTo("CHR00_Strap_Attack_L"));
            Assert.That(strapAttackLeft.speed, Is.EqualTo(0.65f).Within(0.001f));
            AnimatorState strapAttackRight =
                strapUpperBodyStates["StrapAttackRightUpperBodyAction"];
            AssertStateMotionPath(
                strapAttackRight,
                AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Strap_Attack_R.fbx");
            Assert.That(strapAttackRight.motion.name,
                Is.EqualTo("CHR00_Strap_Attack_R"));
            Assert.That(strapAttackRight.speed, Is.EqualTo(0.65f).Within(0.001f));
            foreach (AnimatorState attackState in new[]
                     {
                         strapAttackLeft,
                         strapAttackRight
                     })
            {
                AnimationEvent[] attackEvents = AnimationUtility.GetAnimationEvents(
                    (AnimationClip)attackState.motion);
                Assert.That(attackEvents.Count(item => item.functionName ==
                    "ShowcaseStrapAttackImpulse"), Is.EqualTo(1), attackState.name);
                Assert.That(attackEvents.Count(item => item.functionName ==
                    "ShowcaseStrapActionComplete"), Is.EqualTo(1), attackState.name);
            }
            Assert.That(strapUpperBodyStates.Values.SelectMany(state => state.transitions),
                Is.Empty);
            Assert.That(strapUpperBodyLayer.stateMachine.anyStateTransitions, Is.Empty);

            Assert.That(meleeMovingLegsLayer.name, Is.EqualTo("Melee Moving Legs"));
            Assert.That(meleeMovingLegsLayer.blendingMode,
                Is.EqualTo(AnimatorLayerBlendingMode.Override));
            Assert.That(meleeMovingLegsLayer.defaultWeight,
                Is.EqualTo(0f).Within(0.001f));
            Assert.That(meleeMovingLegsLayer.avatarMask, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(meleeMovingLegsLayer.avatarMask),
                Is.EqualTo(ControllerRoot + "/CHR00_MeleeMovingLegs.mask"));
            Assert.That(meleeMovingLegsLayer.avatarMask,
                Is.Not.SameAs(upperBodyLayer.avatarMask));
            Assert.That(meleeMovingLegsLayer.stateMachine.states, Has.Length.EqualTo(6));
            Assert.That(meleeMovingLegsLayer.stateMachine.defaultState.name,
                Is.EqualTo("MeleeLegsIdle"));
            Assert.That(meleeMovingLegsLayer.stateMachine.states
                .Select(child => child.state.name), Is.EquivalentTo(new[]
            {
                "MeleeLegsIdle", "MeleeLegsForward", "MeleeLegsBackward",
                "MeleeLegsLeft", "MeleeLegsRight", "MeleeLegsRun"
            }));
            Assert.That(meleeMovingLegsLayer.stateMachine.states
                .SelectMany(child => child.state.transitions), Is.Empty);
            Assert.That(meleeMovingLegsLayer.stateMachine.anyStateTransitions, Is.Empty);
            Dictionary<string, AnimatorState> meleeLegStates =
                meleeMovingLegsLayer.stateMachine.states
                    .Select(child => child.state)
                    .ToDictionary(state => state.name, StringComparer.Ordinal);
            AssertStateMotionPath(meleeLegStates["MeleeLegsForward"],
                AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Walk_F.fbx");
            AssertStateMotionPath(meleeLegStates["MeleeLegsBackward"],
                AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Walk_F.fbx");
            Assert.That(meleeLegStates["MeleeLegsBackward"].cycleOffset,
                Is.EqualTo(1f).Within(0.001f));
            AssertStateMotionPath(meleeLegStates["MeleeLegsLeft"],
                AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Strafe_L.fbx");
            AssertStateMotionPath(meleeLegStates["MeleeLegsRight"],
                AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Strafe_R.fbx");
            AssertStateMotionPath(meleeLegStates["MeleeLegsRun"],
                AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Run_F.fbx");
            Assert.That(meleeLegStates.Values.All(state =>
                    state.speedParameterActive &&
                    state.speedParameter == "LocomotionPlayback"),
                Is.True);

            AvatarMask meleeLegMask = meleeMovingLegsLayer.avatarMask;
            foreach (AvatarMaskBodyPart part in new[]
                     {
                         AvatarMaskBodyPart.LeftLeg,
                         AvatarMaskBodyPart.RightLeg
                     })
            {
                Assert.That(meleeLegMask.GetHumanoidBodyPartActive(part),
                    Is.True, part.ToString());
            }
            foreach (AvatarMaskBodyPart part in new[]
                     {
                         AvatarMaskBodyPart.Root,
                         AvatarMaskBodyPart.Body,
                         AvatarMaskBodyPart.Head,
                         AvatarMaskBodyPart.LeftArm,
                         AvatarMaskBodyPart.RightArm,
                         AvatarMaskBodyPart.LeftFootIK,
                         AvatarMaskBodyPart.RightFootIK
                     })
            {
                Assert.That(meleeLegMask.GetHumanoidBodyPartActive(part),
                    Is.False, part.ToString());
            }

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
                Assert.That(path.IndexOf("_REF", StringComparison.OrdinalIgnoreCase),
                    Is.EqualTo(-1), state.name);
                Assert.That(path.IndexOf("candidate", StringComparison.OrdinalIgnoreCase),
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
                         ("PickupLowSmallAction", AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Pickup_Low_Small.fbx"),
                         ("PickupLowMediumAction", AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Pickup_Low_Medium.fbx"),
                         ("ThrowLightAction", AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Throw_Small.fbx"),
                         ("ThrowMediumAction", AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Throw_Medium.fbx")
                     })
            {
                AssertStateMotionPath(states[state], path);
            }

            foreach ((string path, float contact, float complete) in new[]
                     {
                         (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Pickup_Low_Small.fbx",
                             ShowcaseChr00AnimationTiming.PickupLowSmallContact,
                             ShowcaseChr00AnimationTiming.PickupLowSmallComplete),
                         (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Pickup_Low_Medium.fbx",
                             ShowcaseChr00AnimationTiming.PickupLowMediumContact,
                             ShowcaseChr00AnimationTiming.PickupLowMediumComplete),
                         (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Catch_Small_Center.fbx",
                             ShowcaseChr00AnimationTiming.CatchSmallCenterContact,
                             ShowcaseChr00AnimationTiming.CatchSmallCenterComplete),
                         (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Catch_Small_Diving_L.fbx",
                             ShowcaseChr00AnimationTiming.CatchSmallDivingContact,
                             ShowcaseChr00AnimationTiming.CatchSmallDivingComplete),
                         (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Catch_Small_Diving_R.fbx",
                             ShowcaseChr00AnimationTiming.CatchSmallDivingContact,
                             ShowcaseChr00AnimationTiming.CatchSmallDivingComplete),
                         (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Catch_Medium.fbx",
                             ShowcaseChr00AnimationTiming.CatchMediumContact,
                             ShowcaseChr00AnimationTiming.CatchMediumComplete)
                     })
            {
                AssertOwnedEventPair(
                    path,
                    "ShowcaseCarryContact",
                    contact,
                    "ShowcaseActionComplete",
                    complete);
            }

            AssertOwnedEventPair(
                AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Throw_Small.fbx",
                "ShowcaseThrowRelease",
                ShowcaseChr00AnimationTiming.ThrowSmallRelease,
                "ShowcaseActionComplete",
                ShowcaseChr00AnimationTiming.ThrowSmallComplete);
            AssertOwnedEventPair(
                AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Throw_Medium.fbx",
                "ShowcaseThrowRelease",
                ShowcaseChr00AnimationTiming.ThrowMediumRelease,
                "ShowcaseActionComplete",
                ShowcaseChr00AnimationTiming.ThrowMediumComplete);
        }

        [Test]
        public void AllySupplyController_UsesOnlyApprovedBinderWindupAndRelease()
        {
            AnimatorController controller = LoadController("CHR01_Ally.controller");
            Dictionary<string, AnimatorState> states = controller.layers[0].stateMachine.states
                .Select(child => child.state)
                .ToDictionary(state => state.name, StringComparer.Ordinal);

            Assert.That(states.Keys, Is.EquivalentTo(new[]
            {
                "Idle",
                "Walk",
                "Run",
                "BinderWindupReady",
                "SupplyReleaseAction"
            }));
            AssertStateMotionPath(
                states["BinderWindupReady"],
                AnimationRoot + "/CHR01/CHR01_Supply_Binder_Release.fbx");
            AssertStateMotionPath(
                states["SupplyReleaseAction"],
                AnimationRoot + "/CHR01/CHR01_Supply_Binder_Release.fbx");

            AssertStateMotionPath(
                states["Idle"],
                AnimationRoot + "/CHR01/CHR01_Idle.fbx");
            AssertStateMotionPath(
                states["Walk"],
                AnimationRoot + "/CHR01/CHR01_Walk_F.fbx");
            AssertStateMotionPath(
                states["Run"],
                AnimationRoot + "/CHR01/CHR01_Run_F.fbx");
            Assert.That(states["SupplyReleaseAction"].speed,
                Is.EqualTo(ShowcaseNonPlayerAnimationTiming
                    .AllySupplyReleaseStateSpeed).Within(0.0001f));

            AnimationClip windup = (AnimationClip)states["BinderWindupReady"].motion;
            Assert.That(windup.name,
                Is.EqualTo("CHR01_Supply_Binder_Windup_Runtime"));
            Assert.That(windup.isHumanMotion, Is.True,
                "Runtime ready pose must use the valid Humanoid Release source.");
            Assert.That(windup.isLooping, Is.True);
            Assert.That(AnimationUtility.GetAnimationEvents(windup), Is.Empty,
                "The one-frame ready pose must not own physical contact timing.");

            AnimationClip release = (AnimationClip)states["SupplyReleaseAction"].motion;
            Assert.That(release.name,
                Is.EqualTo("CHR01_Supply_Binder_Release_Runtime"));
            AnimationEvent[] releaseEvents = AnimationUtility.GetAnimationEvents(release);
            Assert.That(releaseEvents.Select(item => item.functionName),
                Is.EqualTo(new[] { "ShowcaseAllySupplyRelease" }));
            Assert.That(releaseEvents, Has.Length.EqualTo(1));
            Assert.That(releaseEvents[0].time,
                Is.EqualTo(release.length *
                    ShowcaseNonPlayerAnimationTiming.AllySupplyRelease)
                    .Within(Mathf.Max(0.001f, 1f / release.frameRate)));
        }

        [Test]
        public void Chr01FlatDelivery_HasExactIndependentHumanoidImportContract()
        {
            const string root = AnimationRoot + "/CHR01";
            string[] expectedNames =
            {
                "CHR01_Getup_B",
                "CHR01_Getup_F",
                "CHR01_Idle",
                "CHR01_Idle_Variant_Stretch",
                "CHR01_Jump",
                "CHR01_Knockdown_B",
                "CHR01_Knockdown_F",
                "CHR01_Melee_Hit_Body",
                "CHR01_Run_F",
                "CHR01_Run_Stop",
                "CHR01_Strafe_L",
                "CHR01_Strafe_R",
                "CHR01_Supply_Binder_Release",
                "CHR01_Supply_Binder_Windup",
                "CHR01_Turn_180_Running",
                "CHR01_Turn_180_Walking",
                "CHR01_Turn_L_90",
                "CHR01_Turn_R_90",
                "CHR01_Walk_F",
                "CHR01_Walk_Stop"
            };
            HashSet<string> loopNames = new HashSet<string>(
                new[]
                {
                    "CHR01_Idle",
                    "CHR01_Walk_F",
                    "CHR01_Run_F",
                    "CHR01_Strafe_L",
                    "CHR01_Strafe_R",
                    "CHR01_Supply_Binder_Windup"
                },
                StringComparer.Ordinal);

            string[] paths = AssetDatabase.FindAssets("t:Model", new[] { root })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path =>
                    path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        Path.GetDirectoryName(path)?.Replace('\\', '/'),
                        root,
                        StringComparison.Ordinal))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            Assert.That(paths, Has.Length.EqualTo(20));
            Assert.That(paths.Select(Path.GetFileNameWithoutExtension),
                Is.EquivalentTo(expectedNames));

            foreach (string path in paths)
            {
                string sourceName = Path.GetFileNameWithoutExtension(path);
                bool release = sourceName == "CHR01_Supply_Binder_Release";
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                Assert.That(importer, Is.Not.Null, path);
                Assert.That(importer.importAnimation, Is.True, path);
                Assert.That(importer.animationType,
                    Is.EqualTo(ModelImporterAnimationType.Human), path);
                Assert.That(importer.avatarSetup,
                    Is.EqualTo(ModelImporterAvatarSetup.CreateFromThisModel), path);
                Assert.That(importer.sourceAvatar, Is.Null, path);
                Assert.That(importer.humanDescription.hasTranslationDoF, Is.False, path);

                Assert.That(AssetDatabase.LoadAllAssetsAtPath(path).OfType<Mesh>(),
                    Is.Not.Empty,
                    path + " must retain its own skinned model/bones.");
                Avatar[] avatars = AssetDatabase.LoadAllAssetsAtPath(path)
                    .OfType<Avatar>()
                    .ToArray();
                Assert.That(avatars, Has.Length.EqualTo(1), path);
                Assert.That(avatars[0].isValid, Is.True, path);
                Assert.That(avatars[0].isHuman, Is.True, path);

                ModelImporterClipAnimation[] defaults = importer.defaultClipAnimations;
                ModelImporterClipAnimation[] configured = importer.clipAnimations;
                Assert.That(defaults, Has.Length.EqualTo(1), path);
                AnimationClip[] activeClips = AssetDatabase.LoadAllAssetsAtPath(path)
                    .OfType<AnimationClip>()
                    .Where(clip => !clip.name.StartsWith(
                        "__preview__",
                        StringComparison.Ordinal))
                    .ToArray();
                if (release)
                {
                    Assert.That(configured.Select(clip => clip.name),
                        Is.EquivalentTo(new[]
                        {
                            "CHR01_Supply_Binder_Windup_Runtime",
                            "CHR01_Supply_Binder_Release_Runtime"
                        }), path);
                    Assert.That(activeClips.Select(clip => clip.name),
                        Is.EquivalentTo(configured.Select(clip => clip.name)), path);
                    Assert.That(activeClips.All(clip => clip.isHumanMotion),
                        Is.True, path);
                    ModelImporterClipAnimation windup = configured.Single(clip =>
                        clip.name == "CHR01_Supply_Binder_Windup_Runtime");
                    ModelImporterClipAnimation releaseClip = configured.Single(clip =>
                        clip.name == "CHR01_Supply_Binder_Release_Runtime");
                    Assert.That(windup.firstFrame, Is.EqualTo(21f).Within(0.001f));
                    Assert.That(windup.lastFrame, Is.EqualTo(22f).Within(0.001f));
                    Assert.That(windup.loopTime, Is.True);
                    Assert.That(windup.loopPose, Is.True);
                    Assert.That(windup.events ?? Array.Empty<AnimationEvent>(), Is.Empty);
                    Assert.That(releaseClip.firstFrame,
                        Is.EqualTo(21f).Within(0.001f));
                    Assert.That(releaseClip.lastFrame,
                        Is.EqualTo(44f).Within(0.001f));
                    Assert.That(releaseClip.loopTime, Is.False);
                    Assert.That(releaseClip.loopPose, Is.False);
                    AnimationEvent[] events = releaseClip.events ??
                        Array.Empty<AnimationEvent>();
                    Assert.That(events, Has.Length.EqualTo(1), path);
                    Assert.That(events[0].functionName,
                        Is.EqualTo("ShowcaseAllySupplyRelease"), path);
                    Assert.That(events[0].time,
                        Is.EqualTo(ShowcaseNonPlayerAnimationTiming
                            .AllySupplyRelease).Within(0.0001f), path);
                }
                else
                {
                    Assert.That(configured, Has.Length.EqualTo(1), path);
                    Assert.That(configured[0].name, Is.EqualTo(sourceName), path);
                    Assert.That(configured[0].takeName,
                        Is.EqualTo(defaults[0].takeName), path);
                    Assert.That(configured[0].firstFrame,
                        Is.EqualTo(defaults[0].firstFrame).Within(0.001f), path);
                    Assert.That(configured[0].lastFrame,
                        Is.EqualTo(defaults[0].lastFrame).Within(0.001f), path);
                    Assert.That(configured[0].loopTime,
                        Is.EqualTo(loopNames.Contains(sourceName)), path);
                    Assert.That(configured[0].loopPose,
                        Is.EqualTo(loopNames.Contains(sourceName)), path);
                    Assert.That(configured[0].events ?? Array.Empty<AnimationEvent>(),
                        Is.Empty, path);
                    bool dedicatedOneFrameReadyPose = sourceName ==
                        "CHR01_Supply_Binder_Windup";
                    Assert.That(
                        activeClips.All(clip => clip.isHumanMotion) ||
                        dedicatedOneFrameReadyPose,
                        Is.True,
                        path);
                }
            }
        }

        [Test]
        public void EnyFlatDeliveries_HaveExactIndependentHumanoidImportAndEventContracts()
        {
            string[] suffixes =
            {
                "Hit_BigHeadHit", "Hit_Body", "Hit_Head", "Hit_Reaction",
                "Hit_Rib", "Hit_Stomach", "Idle",
                "Injured_Backwards_Turn_Left", "Injured_Backwards_Turn_Right",
                "Injured_Hurting_Idle", "Injured_Idle", "Injured_Run_Backwards",
                "Injured_Run_Backwards_Left_Turn",
                "Injured_Run_Backwards_Right_Turn", "Injured_Run_Jump",
                "Injured_Run_Left_Turn", "Injured_Run_Right_Turn", "Injured_Run",
                "Injured_Standing_Jump", "Injured_Stumble_Idle",
                "Injured_Turn_Left", "Injured_Turn_Right", "Injured_Walk_Backwards",
                "Injured_Walk_Left_Turn", "Injured_Walk_Right_Turn", "Injured_Walk",
                "Injured_Wave_Idle", "Jump", "Left_Strafe",
                "Left_Strafe_Walking", "Left_Turn_90", "Lod_Mutant_Punch",
                "Lod_Mutant_Swiping", "Lod_Mutant_Walking",
                "Lod_One_Hand_Club_Combo", "Melee_Candidate_Boxing",
                "Melee_Candidate_Boxing_2", "Melee_Candidate_Kicking",
                "Melee_Candidate_Kicking_2", "Right_Strafe",
                "Right_Strafe_Walking", "Right_Turn_90", "Run_Backwards",
                "Standard_Run", "Turn_Left", "Turning_Right_45_Degrees",
                "Walking", "Walking_Backwards"
            };
            HashSet<string> loopSuffixes = new HashSet<string>(new[]
            {
                "Idle", "Walking", "Walking_Backwards", "Standard_Run",
                "Run_Backwards", "Left_Strafe", "Right_Strafe",
                "Left_Strafe_Walking", "Right_Strafe_Walking",
                "Lod_Mutant_Walking", "Injured_Hurting_Idle", "Injured_Idle",
                "Injured_Stumble_Idle", "Injured_Wave_Idle", "Injured_Walk",
                "Injured_Walk_Backwards", "Injured_Run", "Injured_Run_Backwards"
            }, StringComparer.Ordinal);

            foreach (string variant in new[] { "Normal", "Elite" })
            {
                string root = AnimationRoot + "/ENY01/" + variant;
                string prefix = "ENY01_" + variant + "_";
                string[] paths = AssetDatabase.FindAssets("t:Model", new[] { root })
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(path =>
                        path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(
                            Path.GetDirectoryName(path)?.Replace('\\', '/'),
                            root,
                            StringComparison.Ordinal))
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .ToArray();
                Assert.That(paths, Has.Length.EqualTo(48), variant);
                Assert.That(paths.Select(Path.GetFileNameWithoutExtension),
                    Is.EquivalentTo(suffixes.Select(suffix => prefix + suffix)),
                    variant);

                foreach (string path in paths)
                {
                    string sourceName = Path.GetFileNameWithoutExtension(path);
                    string suffix = sourceName.Substring(prefix.Length);
                    ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                    Assert.That(importer, Is.Not.Null, path);
                    Assert.That(importer.importAnimation, Is.True, path);
                    Assert.That(importer.animationType,
                        Is.EqualTo(ModelImporterAnimationType.Human), path);
                    Assert.That(importer.avatarSetup,
                        Is.EqualTo(ModelImporterAvatarSetup.CreateFromThisModel), path);
                    Assert.That(importer.sourceAvatar, Is.Null, path);
                    Assert.That(importer.humanDescription.hasTranslationDoF, Is.False, path);
                    Assert.That(importer.importCameras, Is.False, path);
                    Assert.That(importer.importLights, Is.False, path);
                    Assert.That(importer.materialImportMode,
                        Is.EqualTo(ModelImporterMaterialImportMode.None), path);

                    Avatar[] avatars = AssetDatabase.LoadAllAssetsAtPath(path)
                        .OfType<Avatar>()
                        .ToArray();
                    Assert.That(avatars, Has.Length.EqualTo(1), path);
                    Assert.That(avatars[0].isHuman, Is.True, path);
                    Assert.That(avatars[0].isValid, Is.True, path);

                    ModelImporterClipAnimation[] defaults = importer.defaultClipAnimations;
                    ModelImporterClipAnimation[] configured = importer.clipAnimations;
                    Assert.That(defaults, Has.Length.EqualTo(1), path);
                    Assert.That(configured, Has.Length.EqualTo(1), path);
                    Assert.That(configured[0].name, Is.EqualTo(sourceName), path);
                    Assert.That(configured[0].takeName,
                        Is.EqualTo(defaults[0].takeName), path);
                    Assert.That(configured[0].firstFrame,
                        Is.EqualTo(defaults[0].firstFrame).Within(0.001f), path);
                    Assert.That(configured[0].lastFrame,
                        Is.EqualTo(defaults[0].lastFrame).Within(0.001f), path);
                    Assert.That(configured[0].loopTime,
                        Is.EqualTo(loopSuffixes.Contains(suffix)), path);
                    Assert.That(configured[0].loopPose,
                        Is.EqualTo(loopSuffixes.Contains(suffix)), path);

                    (string functionName, float normalizedTime)[] expectedEvents =
                        ExpectedEnemyEvents(variant == "Elite", suffix);
                    AnimationEvent[] actualEvents = configured[0].events ??
                        Array.Empty<AnimationEvent>();
                    Assert.That(actualEvents, Has.Length.EqualTo(expectedEvents.Length), path);
                    foreach ((string functionName, float normalizedTime) in expectedEvents)
                    {
                        AnimationEvent actual = actualEvents.Single(item =>
                            item.functionName == functionName);
                        Assert.That(actual.time,
                            Is.EqualTo(normalizedTime).Within(0.0001f),
                            path + " / " + functionName);
                    }
                }
            }
        }

        [Test]
        public void PlayerAndEnemyControllers_ExposeApprovedCombatStateContracts()
        {
            AnimatorController player = LoadController("CHR00_Player.controller");
            string[] playerStates = StateNames(player);
            foreach (string state in new[]
                     {
                         "Run", "JumpAction", "MeleeJabAction", "MeleeJabReturnAction",
                         "MeleeCrossAction", "PickupLowSmallAction", "PickupLowMediumAction",
                         "MeleeHookAction", "HitBodyAction", "KnockdownBackAction",
                         "KnockdownFrontAction", "GetupBackAction", "GetupFrontAction",
                         "Death1Action", "Death2Action"
                     })
            {
                Assert.That(playerStates, Does.Contain(state));
            }

            const string jabCrossPath = AnimationRoot +
                "/CHR00/DesignerA/Approved/CHR00_Melee_JabCross_raw.fbx";
            ModelImporter jabCrossImporter = AssetImporter.GetAtPath(jabCrossPath) as
                ModelImporter;
            Assert.That(jabCrossImporter, Is.Not.Null);
            Dictionary<string, ModelImporterClipAnimation> meleeClips =
                jabCrossImporter.clipAnimations.ToDictionary(clip => clip.name);
            Assert.That(meleeClips.Keys, Is.EquivalentTo(new[]
            {
                "CHR00_Melee_Jab_Runtime",
                "CHR00_Melee_Jab_Return_Runtime",
                "CHR00_Melee_Cross_Runtime"
            }));
            Assert.That(meleeClips["CHR00_Melee_Jab_Runtime"].firstFrame,
                Is.EqualTo(1f));
            Assert.That(meleeClips["CHR00_Melee_Jab_Runtime"].lastFrame,
                Is.EqualTo(14f));
            Assert.That(meleeClips["CHR00_Melee_Jab_Return_Runtime"].firstFrame,
                Is.EqualTo(1f));
            Assert.That(meleeClips["CHR00_Melee_Jab_Return_Runtime"].lastFrame,
                Is.EqualTo(14f));
            Assert.That(meleeClips["CHR00_Melee_Cross_Runtime"].firstFrame,
                Is.EqualTo(14f));
            Assert.That(meleeClips["CHR00_Melee_Cross_Runtime"].lastFrame,
                Is.EqualTo(60f));

            Dictionary<string, AnimatorState> playerBaseStates = player.layers[0]
                .stateMachine.states.Select(child => child.state)
                .ToDictionary(state => state.name);
            AssertStateMotionPath(playerBaseStates["MeleeJabAction"], jabCrossPath);
            AssertStateMotionPath(playerBaseStates["MeleeJabReturnAction"], jabCrossPath);
            AssertStateMotionPath(playerBaseStates["MeleeCrossAction"], jabCrossPath);
            Assert.That(playerBaseStates["MeleeJabReturnAction"].cycleOffset,
                Is.EqualTo(1f).Within(0.001f));
            Assert.That(player.layers.Select(layer => layer.name), Is.EqualTo(new[]
            {
                "Base Layer", "Aim Upper Body", "Strap Upper Body", "Melee Moving Legs"
            }));
            AnimatorControllerLayer meleeLayer = player.layers[3];
            Assert.That(meleeLayer.avatarMask, Is.Not.SameAs(player.layers[1].avatarMask));
            Assert.That(meleeLayer.defaultWeight, Is.EqualTo(0f).Within(0.001f));
            Dictionary<string, AnimatorState> playerMeleeStates = meleeLayer
                .stateMachine.states.Select(child => child.state)
                .ToDictionary(state => state.name);
            Assert.That(playerMeleeStates.Keys, Is.EquivalentTo(new[]
            {
                "MeleeLegsIdle", "MeleeLegsForward", "MeleeLegsBackward",
                "MeleeLegsLeft", "MeleeLegsRight", "MeleeLegsRun"
            }));
            AssertStateMotionPath(playerMeleeStates["MeleeLegsForward"],
                AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Walk_F.fbx");
            AssertStateMotionPath(playerMeleeStates["MeleeLegsLeft"],
                AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Strafe_L.fbx");

            foreach (string fileName in new[]
                     {
                         "ENY01_Normal.controller", "ENY01_Elite.controller"
                     })
            {
                string[] states = StateNames(LoadController(fileName));
                foreach (string state in new[]
                         {
                             "AttackBoxingAction", "AttackKneeStrikeAction",
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
            Assert.That(eliteStates, Does.Not.Contain("EliteJumpAttackAction"));
        }

        [Test]
        public void EnemyControllers_UseOnlyTheirMatchingFlatVariantRuntimeClips()
        {
            foreach (string variant in new[] { "Normal", "Elite" })
            {
                AnimatorController controller = LoadController(
                    "ENY01_" + variant + ".controller");
                Dictionary<string, AnimatorState> states = controller.layers[0]
                    .stateMachine.states
                    .Select(child => child.state)
                    .ToDictionary(state => state.name, StringComparer.Ordinal);
                var expected = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["Idle"] = "Idle",
                    ["Run"] = "Standard_Run",
                    ["HitLightAction"] = "Hit_Head",
                    ["HitBodyAction"] = "Hit_Body",
                    ["HitRibAction"] = "Hit_Rib",
                    ["HitStomachAction"] = "Hit_Stomach",
                    ["HitBigHeadAction"] = "Hit_BigHeadHit",
                    ["AttackBoxingAction"] = "Melee_Candidate_Boxing",
                    ["AttackKneeStrikeAction"] = "Melee_Candidate_Boxing_2",
                    ["AttackKickingAction"] = "Melee_Candidate_Kicking",
                    ["AttackKicking2Action"] = "Melee_Candidate_Kicking_2"
                };
                if (variant == "Elite")
                {
                    expected["EliteMutantPunchAction"] = "Lod_Mutant_Punch";
                    expected["EliteMutantSwipingAction"] = "Lod_Mutant_Swiping";
                }
                string root = AnimationRoot + "/ENY01/" + variant + "/ENY01_" +
                    variant + "_";
                foreach (KeyValuePair<string, string> pair in expected)
                {
                    AssertStateMotionPath(
                        states[pair.Key],
                        root + pair.Value + ".fbx");
                }

                string[] referencedEnemyPaths = states.Values
                    .Select(state => AssetDatabase.GetAssetPath(state.motion))
                    .Where(path => path.Contains("/ENY01/", StringComparison.Ordinal))
                    .ToArray();
                Assert.That(referencedEnemyPaths, Has.Length.EqualTo(expected.Count), variant);
                Assert.That(referencedEnemyPaths.All(path => path.StartsWith(
                    root,
                    StringComparison.Ordinal)), Is.True, variant);
                AssertStateMotionPath(
                    states["Death1Action"],
                    AnimationRoot +
                    "/CHR00/DesignerA/Approved/CHR00_Knockdown_F.fbx");
                AssertStateMotionPath(
                    states["Death2Action"],
                    AnimationRoot +
                    "/CHR00/DesignerA/Approved/CHR00_Knockdown_B.fbx");
                Assert.That(states.Keys, Does.Not.Contain("EliteJumpAttackAction"), variant);
            }
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
                (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Melee_JabCross_raw.fbx", "CHR00_Melee_Jab_Runtime", "ShowcaseAttackWindowOpen", ShowcaseChr00AnimationTiming.MeleeJabWindowOpen),
                (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Melee_JabCross_raw.fbx", "CHR00_Melee_Jab_Runtime", "ShowcaseAttackWindowClose", ShowcaseChr00AnimationTiming.MeleeJabWindowClose),
                (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Melee_JabCross_raw.fbx", "CHR00_Melee_Jab_Runtime", "ShowcaseActionComplete", ShowcaseChr00AnimationTiming.MeleeJabComplete),
                (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Melee_JabCross_raw.fbx", "CHR00_Melee_Jab_Return_Runtime", "ShowcaseActionComplete", ShowcaseChr00AnimationTiming.MeleeJabReturnComplete),
                (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Melee_JabCross_raw.fbx", "CHR00_Melee_Cross_Runtime", "ShowcaseAttackWindowOpen", ShowcaseChr00AnimationTiming.MeleeCrossWindowOpen),
                (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Melee_JabCross_raw.fbx", "CHR00_Melee_Cross_Runtime", "ShowcaseActionComplete", ShowcaseChr00AnimationTiming.MeleeCrossComplete),
                (AnimationRoot + "/CHR00/DesignerA/Legacy/Hook Punch.fbx", null, "ShowcaseAttackWindowOpen", 0.38f),
                (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Melee_JabCross_raw.fbx", "CHR00_Melee_Jab_Runtime", "ShowcaseComboBufferOpen", ShowcaseChr00AnimationTiming.MeleeJabBufferOpen),
                (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Melee_JabCross_raw.fbx", "CHR00_Melee_Cross_Runtime", "ShowcaseComboBufferClose", ShowcaseChr00AnimationTiming.MeleeCrossBufferClose),
                (AnimationRoot + "/ENY01/Normal/ENY01_Normal_Melee_Candidate_Boxing.fbx", null, "ShowcaseAttackWindowOpen", ShowcaseNonPlayerAnimationTiming.BoxingWindowOpen),
                (AnimationRoot + "/ENY01/Normal/ENY01_Normal_Melee_Candidate_Boxing.fbx", null, "ShowcaseAttackWindowClose", ShowcaseNonPlayerAnimationTiming.BoxingWindowClose),
                (AnimationRoot + "/ENY01/Elite/ENY01_Elite_Melee_Candidate_Boxing_2.fbx", null, "ShowcaseAttackWindowOpen", ShowcaseNonPlayerAnimationTiming.Boxing2WindowOpen),
                (AnimationRoot + "/ENY01/Elite/ENY01_Elite_Lod_Mutant_Punch.fbx", null, "ShowcaseAttackWindowOpen", ShowcaseNonPlayerAnimationTiming.MutantPunchWindowOpen),
                (AnimationRoot + "/ENY01/Elite/ENY01_Elite_Lod_Mutant_Swiping.fbx", null, "ShowcaseAttackWindowClose", ShowcaseNonPlayerAnimationTiming.MutantSwipeWindowClose),
                (AnimationRoot + "/ENY01/Elite/ENY01_Elite_Hit_Head.fbx", null, "ShowcaseActionComplete", ShowcaseNonPlayerAnimationTiming.EnemyHitComplete),
                (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Pickup_Low_Small.fbx", null, "ShowcaseCarryContact", ShowcaseChr00AnimationTiming.PickupLowSmallContact),
                (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Pickup_Low_Medium.fbx", null, "ShowcaseCarryContact", ShowcaseChr00AnimationTiming.PickupLowMediumContact),
                (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Catch_Medium.fbx", null, "ShowcaseCarryContact", ShowcaseChr00AnimationTiming.CatchMediumContact),
                (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Throw_Small.fbx", null, "ShowcaseThrowRelease", ShowcaseChr00AnimationTiming.ThrowSmallRelease),
                (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Throw_Medium.fbx", null, "ShowcaseThrowRelease", ShowcaseChr00AnimationTiming.ThrowMediumRelease),
                (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Knockdown_B.fbx", null, "ShowcaseActionComplete", ShowcaseChr00AnimationTiming.KnockdownBackComplete),
                (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Getup_B.fbx", null, "ShowcaseActionComplete", ShowcaseChr00AnimationTiming.GetupBackComplete),
                (AnimationRoot + "/ENY01/Elite/ENY01_Elite_Hit_Body.fbx", null, "ShowcaseActionComplete", ShowcaseNonPlayerAnimationTiming.EnemyHitComplete),
                (AnimationRoot + "/CHR01/CHR01_Supply_Binder_Release.fbx", "CHR01_Supply_Binder_Release_Runtime", "ShowcaseAllySupplyRelease", ShowcaseNonPlayerAnimationTiming.AllySupplyRelease)
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
        public void RuntimeControllers_ExcludeReviewCatalogAndUnapprovedAllySupplyClips()
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
            AnimationClip[] allyClips = ally.layers
                .SelectMany(layer => layer.stateMachine.states)
                .Select(child => child.state.motion)
                .OfType<AnimationClip>()
                .ToArray();
            Assert.That(allyClips.Any(clip => AssetDatabase.GetAssetPath(clip)
                    .Contains("/CHR00/", StringComparison.OrdinalIgnoreCase)),
                Is.False, "CHR01 must not reuse CHR00 Catch clips.");
            Assert.That(allyClips.Any(clip => AssetDatabase.GetAssetPath(clip)
                    .Contains("Supply_candidate_", StringComparison.OrdinalIgnoreCase)),
                Is.False, "Review supply candidates must remain outside runtime playback.");
        }

        [Test]
        public void EnemyEligibility_AllowsEliteTechniquesButExcludesWeaponPropAndReviewMotion()
        {
            string[] forbiddenStateNames =
            {
                "Walk", "AttackBoxing2Action", "EliteJumpAttackAction",
                "EliteMutantWalkingAction", "EliteClubComboAction",
                "EliteStabbingAction"
            };
            string[] forbiddenSourceSuffixes =
            {
                "_Walking.fbx", "_Walking_Backwards.fbx", "_Run_Backwards.fbx",
                "_Turn_Left.fbx", "_Turning_Right_45_Degrees.fbx",
                "_Hit_Reaction.fbx", "_Lod_Mutant_Walking.fbx",
                "_Lod_One_Hand_Club_Combo.fbx"
            };

            foreach (string fileName in new[]
                     {
                         "ENY01_Normal.controller", "ENY01_Elite.controller"
                     })
            {
                AnimatorController controller = LoadController(fileName);
                string[] stateNames = StateNames(controller);
                Assert.That(stateNames, Has.None.Matches<string>(name =>
                    forbiddenStateNames.Contains(name)), fileName);
                Assert.That(stateNames, Does.Contain("Run"), fileName);
                Assert.That(stateNames, Does.Contain("AttackKneeStrikeAction"), fileName);
                bool eliteController = fileName.Contains("Elite", StringComparison.Ordinal);
                Assert.That(stateNames.Contains("EliteMutantPunchAction"),
                    Is.EqualTo(eliteController), fileName);
                Assert.That(stateNames.Contains("EliteMutantSwipingAction"),
                    Is.EqualTo(eliteController), fileName);

                string[] motionPaths = controller.layers
                    .SelectMany(layer => layer.stateMachine.states)
                    .Select(child => AssetDatabase.GetAssetPath(child.state.motion))
                    .Where(path => !string.IsNullOrEmpty(path))
                    .ToArray();
                Assert.That(motionPaths.Any(path => forbiddenSourceSuffixes.Any(
                    suffix => path.EndsWith(suffix, StringComparison.Ordinal))),
                    Is.False,
                    fileName);
                Assert.That(motionPaths.Any(path => path.Contains(
                    "/Legacy/Dying",
                    StringComparison.Ordinal)), Is.False, fileName);
            }
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
            AssertStateSpeed(player, "PickupLowSmallAction",
                ShowcaseChr00AnimationTiming.PickupLowSmallStateSpeed);
            AssertStateSpeed(player, "PickupLowMediumAction",
                ShowcaseChr00AnimationTiming.PickupLowMediumStateSpeed);
            AssertStateSpeed(player, "MeleeJabReturnAction", -1.25f);
            AssertStateSpeed(player, "MeleeHookAction",
                ShowcaseChr00AnimationTiming.MeleeHookStateSpeed);
            AssertStateSpeed(normal, "AttackBoxingAction",
                ShowcaseNonPlayerAnimationTiming.BoxingStateSpeed);
            AssertStateSpeed(normal, "AttackKneeStrikeAction",
                ShowcaseNonPlayerAnimationTiming.Boxing2StateSpeed);
            AssertStateSpeed(normal, "AttackKickingAction",
                ShowcaseNonPlayerAnimationTiming.KickingStateSpeed);
            AssertStateSpeed(normal, "AttackKicking2Action",
                ShowcaseNonPlayerAnimationTiming.Kicking2StateSpeed);
            AssertStateSpeed(elite, "AttackBoxingAction",
                ShowcaseNonPlayerAnimationTiming.BoxingStateSpeed);
            AssertStateSpeed(elite, "AttackKneeStrikeAction",
                ShowcaseNonPlayerAnimationTiming.Boxing2StateSpeed);
            AssertStateSpeed(elite, "AttackKickingAction",
                ShowcaseNonPlayerAnimationTiming.KickingStateSpeed);
            AssertStateSpeed(elite, "AttackKicking2Action",
                ShowcaseNonPlayerAnimationTiming.Kicking2StateSpeed);
            AssertStateSpeed(elite, "EliteMutantPunchAction",
                ShowcaseNonPlayerAnimationTiming.MutantPunchStateSpeed);
            AssertStateSpeed(elite, "EliteMutantSwipingAction",
                ShowcaseNonPlayerAnimationTiming.MutantSwipeStateSpeed);
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
                             (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Melee_JabCross_raw.fbx", "CHR00_Melee_Jab_Runtime", HumanBodyBones.LeftHand),
                             (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Melee_JabCross_raw.fbx", "CHR00_Melee_Jab_Runtime", HumanBodyBones.RightHand),
                             (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Melee_JabCross_raw.fbx", "CHR00_Melee_Cross_Runtime", HumanBodyBones.RightHand),
                             (AnimationRoot + "/CHR00/DesignerA/Approved/CHR00_Melee_JabCross_raw.fbx", "CHR00_Melee_Cross_Runtime", HumanBodyBones.LeftHand),
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
                             (AnimationRoot + "/ENY01/Normal/ENY01_Normal_Melee_Candidate_Kicking.fbx", HumanBodyBones.LeftFoot),
                             (AnimationRoot + "/ENY01/Normal/ENY01_Normal_Melee_Candidate_Kicking_2.fbx", HumanBodyBones.RightFoot),
                             (AnimationRoot + "/ENY01/Normal/ENY01_Normal_Melee_Candidate_Boxing.fbx", HumanBodyBones.LeftHand),
                             (AnimationRoot + "/ENY01/Normal/ENY01_Normal_Melee_Candidate_Boxing_2.fbx", HumanBodyBones.RightFoot),
                             (AnimationRoot + "/ENY01/Elite/ENY01_Elite_Lod_Mutant_Punch.fbx", HumanBodyBones.RightHand),
                             (AnimationRoot + "/ENY01/Elite/ENY01_Elite_Lod_Mutant_Swiping.fbx", HumanBodyBones.RightHand),
                             (AnimationRoot + "/ENY01/Elite/ENY01_Elite_Lod_Mutant_Swiping.fbx", HumanBodyBones.LeftHand)
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
        public void Chr00UpdatedAnimationDelivery_HasExactSourceOwnedHumanoidAndClipContract()
        {
            const string approvedRoot =
                AnimationRoot + "/CHR00/DesignerA/Approved";
            string[] expectedBasenames =
            {
                "CHR00_Aim_Idle",
                "CHR00_Catch_Large",
                "CHR00_Catch_Medium",
                "CHR00_Catch_Small_Center",
                "CHR00_Catch_Small_Diving_L",
                "CHR00_Catch_Small_Diving_R",
                "CHR00_Getup_B",
                "CHR00_Getup_F",
                "CHR00_Idle",
                "CHR00_Idle_Variant_Stretch",
                "CHR00_Jump",
                "CHR00_Knockdown_B",
                "CHR00_Knockdown_F",
                "CHR00_Melee_Hit_Body",
                "CHR00_Melee_JabCross_raw",
                "CHR00_Pickup_Low_Medium",
                "CHR00_Pickup_Low_Small",
                "CHR00_Run_F",
                "CHR00_Run_Stop",
                "CHR00_Strafe_L",
                "CHR00_Strafe_L_Short_ALT",
                "CHR00_Strafe_R",
                "CHR00_Strafe_R_Short_ALT",
                "CHR00_Strap_Attack_L",
                "CHR00_Strap_Attack_R",
                "CHR00_Strap_Prepare",
                "CHR00_Strap_Pull",
                "CHR00_Strap_Rolling",
                "CHR00_Strap_Tension_Loop_candidate",
                "CHR00_Strap_Throw",
                "CHR00_Throw_Large",
                "CHR00_Throw_Medium",
                "CHR00_Throw_Small",
                "CHR00_Throw_TwoHand_base_REF",
                "CHR00_Turn_180_Running",
                "CHR00_Turn_180_Walking",
                "CHR00_Turn_L_90",
                "CHR00_Turn_L_Loop_ALT",
                "CHR00_Turn_R_90",
                "CHR00_Turn_R_Loop_ALT",
                "CHR00_Walk_F",
                "CHR00_Walk_Stop",
                "PREVIEW_Strafe_plus_Aim_inplace"
            };
            HashSet<string> loopingBasenames = new HashSet<string>(
                new[]
                {
                    "CHR00_Aim_Idle",
                    "CHR00_Idle",
                    "CHR00_Idle_Variant_Stretch",
                    "CHR00_Run_F",
                    "CHR00_Strafe_L",
                    "CHR00_Strafe_R",
                    "CHR00_Strap_Rolling",
                    "CHR00_Strap_Tension_Loop_candidate",
                    "CHR00_Turn_L_Loop_ALT",
                    "CHR00_Turn_R_Loop_ALT",
                    "CHR00_Walk_F"
                },
                StringComparer.Ordinal);
            (string Name, float FirstFrame, float LastFrame)[] jabSubclips =
            {
                ("CHR00_Melee_Jab_Runtime", 1f, 14f),
                ("CHR00_Melee_Jab_Return_Runtime", 1f, 14f),
                ("CHR00_Melee_Cross_Runtime", 14f, 60f)
            };

            string[] paths = AssetDatabase.FindAssets(
                    "t:Model",
                    new[] { approvedRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            Assert.That(paths.Select(Path.GetFileNameWithoutExtension),
                Is.EquivalentTo(expectedBasenames));

            foreach (string basename in expectedBasenames)
            {
                string path = approvedRoot + "/" + basename + ".fbx";
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                Assert.That(importer, Is.Not.Null, path);
                Assert.That(importer.importAnimation, Is.True, path);
                Assert.That(importer.animationType,
                    Is.EqualTo(ModelImporterAnimationType.Human), path);
                Assert.That(importer.avatarSetup,
                    Is.EqualTo(ModelImporterAvatarSetup.CreateFromThisModel), path);
                Assert.That(importer.sourceAvatar, Is.Null, path);
                if (basename.StartsWith("CHR00_Strap_", StringComparison.Ordinal) &&
                    !basename.Contains("candidate", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.That(importer.humanDescription.hasTranslationDoF,
                        Is.True,
                        path + " must preserve its source-owned translated joints");
                }

                Avatar[] avatars = AssetDatabase.LoadAllAssetsAtPath(path)
                    .OfType<Avatar>()
                    .ToArray();
                Assert.That(avatars, Has.Length.EqualTo(1), path);
                Assert.That(avatars[0].isValid, Is.True, path);
                Assert.That(avatars[0].isHuman, Is.True, path);

                ModelImporterClipAnimation[] defaults = importer.defaultClipAnimations;
                Assert.That(defaults, Has.Length.EqualTo(1), path);
                if (basename != "CHR00_Pickup_Low_Small")
                {
                    Assert.That(defaults[0].takeName, Is.EqualTo("Scene"), path);
                }

                ModelImporterClipAnimation[] configured = importer.clipAnimations;
                AnimationClip[] activeClips = AssetDatabase.LoadAllAssetsAtPath(path)
                    .OfType<AnimationClip>()
                    .Where(clip => !clip.name.StartsWith(
                        "__preview__",
                        StringComparison.Ordinal))
                    .ToArray();
                Assert.That(activeClips.All(clip => clip.isHumanMotion), Is.True, path);

                if (basename == "CHR00_Melee_JabCross_raw")
                {
                    Assert.That(configured.Select(clip => clip.name),
                        Is.EquivalentTo(jabSubclips.Select(clip => clip.Name)), path);
                    Assert.That(activeClips.Select(clip => clip.name),
                        Is.EquivalentTo(jabSubclips.Select(clip => clip.Name)), path);
                    foreach ((string name, float firstFrame, float lastFrame) in jabSubclips)
                    {
                        ModelImporterClipAnimation clip = configured.Single(candidate =>
                            candidate.name == name);
                        Assert.That(clip.firstFrame,
                            Is.EqualTo(firstFrame).Within(0.001f), path + " / " + name);
                        Assert.That(clip.lastFrame,
                            Is.EqualTo(lastFrame).Within(0.001f), path + " / " + name);
                    }
                }
                else if (basename == "CHR00_Strap_Pull")
                {
                    Assert.That(configured.Select(clip => clip.name),
                        Is.EquivalentTo(new[]
                        {
                            "CHR00_Strap_Pull",
                            "CHR00_Strap_Tension_Hold_Runtime"
                        }), path);
                    Assert.That(activeClips.Select(clip => clip.name),
                        Is.EquivalentTo(new[]
                        {
                            "CHR00_Strap_Pull",
                            "CHR00_Strap_Tension_Hold_Runtime"
                        }), path);
                    ModelImporterClipAnimation hold = configured.Single(clip =>
                        clip.name == "CHR00_Strap_Tension_Hold_Runtime");
                    Assert.That(hold.firstFrame, Is.EqualTo(34f).Within(0.001f));
                    Assert.That(hold.lastFrame, Is.EqualTo(35f).Within(0.001f));
                    Assert.That(hold.loopTime, Is.True);
                }
                else
                {
                    Assert.That(configured, Has.Length.EqualTo(1), path);
                    Assert.That(configured[0].name, Is.EqualTo(basename), path);
                    Assert.That(activeClips, Has.Length.EqualTo(1), path);
                    Assert.That(activeClips[0].name, Is.EqualTo(basename), path);
                    if (basename == "CHR00_Pickup_Low_Small")
                    {
                        Assert.That(configured[0].firstFrame,
                            Is.EqualTo(0f).Within(0.001f), path);
                        Assert.That(configured[0].lastFrame,
                            Is.EqualTo(143f).Within(0.001f), path);
                    }
                    if (basename == "CHR00_Strap_Attack_L" ||
                        basename == "CHR00_Strap_Attack_R")
                    {
                        float expectedImpulse = basename.EndsWith(
                            "_L",
                            StringComparison.Ordinal)
                                ? 8f / 20f
                                : 8f / 22f;
                        AnimationEvent impulse = configured[0].events.Single(item =>
                            item.functionName == "ShowcaseStrapAttackImpulse");
                        AnimationEvent complete = configured[0].events.Single(item =>
                            item.functionName == "ShowcaseStrapActionComplete");
                        Assert.That(impulse.time,
                            Is.EqualTo(expectedImpulse).Within(0.0001f), path);
                        Assert.That(complete.time,
                            Is.EqualTo(0.95f).Within(0.0001f), path);
                    }
                }

                if (basename != "CHR00_Strap_Pull")
                {
                    bool expectedLoop = loopingBasenames.Contains(basename);
                    Assert.That(configured.All(clip =>
                            clip.takeName == defaults[0].takeName &&
                            clip.loopTime == expectedLoop &&
                            clip.loopPose == expectedLoop),
                        Is.True,
                        path);
                }
            }
        }

        [Test]
        public void ImportedDelivery_ContainsFourModelsAndOneHundredEightyOneHumanoidSources()
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
            Assert.That(animations, Has.Length.EqualTo(181));
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

        private static (string functionName, float normalizedTime)[]
            ExpectedEnemyEvents(bool elite, string suffix)
        {
            if (suffix == "Standard_Run")
            {
                return new[]
                {
                    ("ShowcaseLeftFootstep",
                        ShowcaseNonPlayerAnimationTiming.EnemyRunLeftFootstep),
                    ("ShowcaseRightFootstep",
                        ShowcaseNonPlayerAnimationTiming.EnemyRunRightFootstep)
                };
            }

            (float open, float close)? attack = suffix switch
            {
                "Melee_Candidate_Boxing" => (
                    ShowcaseNonPlayerAnimationTiming.BoxingWindowOpen,
                    ShowcaseNonPlayerAnimationTiming.BoxingWindowClose),
                "Melee_Candidate_Boxing_2" => (
                    ShowcaseNonPlayerAnimationTiming.Boxing2WindowOpen,
                    ShowcaseNonPlayerAnimationTiming.Boxing2WindowClose),
                "Melee_Candidate_Kicking" => (
                    ShowcaseNonPlayerAnimationTiming.KickingWindowOpen,
                    ShowcaseNonPlayerAnimationTiming.KickingWindowClose),
                "Melee_Candidate_Kicking_2" => (
                    ShowcaseNonPlayerAnimationTiming.Kicking2WindowOpen,
                    ShowcaseNonPlayerAnimationTiming.Kicking2WindowClose),
                "Lod_Mutant_Punch" when elite => (
                    ShowcaseNonPlayerAnimationTiming.MutantPunchWindowOpen,
                    ShowcaseNonPlayerAnimationTiming.MutantPunchWindowClose),
                "Lod_Mutant_Swiping" when elite => (
                    ShowcaseNonPlayerAnimationTiming.MutantSwipeWindowOpen,
                    ShowcaseNonPlayerAnimationTiming.MutantSwipeWindowClose),
                _ => null
            };
            if (attack.HasValue)
            {
                return new[]
                {
                    ("ShowcaseAttackWindowOpen", attack.Value.open),
                    ("ShowcaseAttackWindowClose", attack.Value.close),
                    ("ShowcaseActionComplete",
                        ShowcaseNonPlayerAnimationTiming.EnemyActionComplete)
                };
            }

            bool connectedHit = suffix == "Hit_Head" ||
                suffix == "Hit_Body" ||
                suffix == "Hit_Rib" ||
                suffix == "Hit_Stomach" ||
                suffix == "Hit_BigHeadHit";
            return connectedHit
                ? new[]
                {
                    ("ShowcaseActionComplete",
                        ShowcaseNonPlayerAnimationTiming.EnemyHitComplete)
                }
                : Array.Empty<(string functionName, float normalizedTime)>();
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
