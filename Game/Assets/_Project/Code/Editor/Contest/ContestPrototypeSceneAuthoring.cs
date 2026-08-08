using System;
using System.Collections.Generic;
using System.Linq;
using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Input;
using BreakNotice.Gameplay.Encounter;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.Contest.Editor
{
    public static class ContestPrototypeSceneAuthoring
    {
        private const string ContestScenePath =
            "Assets/_Project/Scenes/10_Prototype/Contest_SingleRoomGraybox.unity";

        public static void PrepareCurrentContestScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ContestScenePath, OpenSceneMode.Single);
            ApplyC3Composition(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ContestScenePath))
            {
                throw new InvalidOperationException($"Could not save {ContestScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("Contest C3 authored Reset composition prepared.");
        }

        public static void ApplyC3Composition(Scene scene)
        {
            ApplyC2Composition(scene);

            GameObject player = RequireRoot(scene, "Player");
            GameObject browserInput = RequireRoot(scene, "ContestBrowserInput");
            PrototypeInteractionTarget[] targets = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<PrototypeInteractionTarget>(true))
                .OrderBy(target => target.RuntimeInstanceKey, StringComparer.Ordinal)
                .ToArray();
            Transform[] actors =
            {
                RequireRoot(scene, "AllyProxy").transform,
                RequireRoot(scene, "EnemyProxy_01").transform,
                RequireRoot(scene, "EnemyProxy_02").transform
            };

            AuthoredEncounterResetController resetController =
                GetOrAddComponent<AuthoredEncounterResetController>(browserInput);
            resetController.Configure(
                player.GetComponent<InputSystemLogicalInputAdapter>(),
                player.GetComponent<PrototypeInteractionStateOwner>(),
                player.GetComponent<PrototypePhysicalObjectLifecycle>(),
                player.GetComponent<ThirdPersonLocomotion>(),
                player.transform,
                actors,
                targets);
            EditorUtility.SetDirty(resetController);
        }

        public static void ApplyC2Composition(Scene scene)
        {
            ApplyC1Composition(scene);

            GameObject player = RequireRoot(scene, "Player");
            PrototypeInteractionStateOwner owner =
                player.GetComponent<PrototypeInteractionStateOwner>();
            PrototypeInteractionController controller =
                player.GetComponent<PrototypeInteractionController>();
            PrototypeInteractionTarget[] targets = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<PrototypeInteractionTarget>(true))
                .OrderBy(target => target.RuntimeInstanceKey, StringComparer.Ordinal)
                .ToArray();

            ConfigureDynamicTarget(
                targets.Single(target => target.Role == PrototypeInteractionRole.DirectBinder),
                1f);
            ConfigureDynamicTarget(
                targets.Single(target => target.Role == PrototypeInteractionRole.DirectChair),
                8f);

            Transform carryAnchor = player.transform.Find("ContestCarryAnchor");
            if (carryAnchor == null)
            {
                carryAnchor = new GameObject("ContestCarryAnchor").transform;
                carryAnchor.SetParent(player.transform, false);
            }

            carryAnchor.localPosition = new Vector3(0f, 1.05f, 1.15f);
            carryAnchor.localRotation = Quaternion.identity;

            PrototypePhysicalObjectLifecycle lifecycle =
                GetOrAddComponent<PrototypePhysicalObjectLifecycle>(player);
            lifecycle.Configure(owner, carryAnchor);

            InputSystemLogicalInputAdapter logicalInput =
                player.GetComponent<InputSystemLogicalInputAdapter>();
            GameObject cameraRig = RequireRoot(scene, "GameplayCameraRig");
            Camera outputCamera = cameraRig.GetComponentInChildren<Camera>(true);
            controller.Configure(
                logicalInput,
                owner,
                outputCamera.transform,
                targets,
                2.8f,
                0.25f,
                false,
                lifecycle);

            EditorUtility.SetDirty(lifecycle);
            EditorUtility.SetDirty(controller);
        }

        public static void ApplyC1Composition(Scene scene)
        {
            GameObject player = RequireRoot(scene, "Player");
            GameObject cameraRig = RequireRoot(scene, "GameplayCameraRig");
            GameObject browserInput = RequireRoot(scene, "ContestBrowserInput");

            PrototypeInteractionTarget[] targets = ConfigureTargets(scene);
            PrototypeInteractionStateOwner owner =
                GetOrAddComponent<PrototypeInteractionStateOwner>(player);
            PrototypeInteractionController controller =
                GetOrAddComponent<PrototypeInteractionController>(player);
            InputSystemLogicalInputAdapter logicalInput =
                player.GetComponent<InputSystemLogicalInputAdapter>();
            Camera outputCamera = cameraRig.GetComponentInChildren<Camera>(true);

            if (logicalInput == null || outputCamera == null)
            {
                throw new InvalidOperationException(
                    "The authorized player input and gameplay output camera are required.");
            }

            controller.Configure(
                logicalInput,
                owner,
                outputCamera.transform,
                targets,
                2.8f,
                0.25f,
                false);

            InteractionReadabilityPresenter presenter =
                GetOrAddComponent<InteractionReadabilityPresenter>(browserInput);
            presenter.Configure(controller);

            EditorUtility.SetDirty(owner);
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(presenter);
        }

        private static PrototypeInteractionTarget[] ConfigureTargets(Scene scene)
        {
            var definitions = new[]
            {
                new TargetDefinition("ObjectProxy_01", "OBJ-P-L-001", "BINDER-DIRECT-01", "FILE BINDER", PrototypeWeightClass.Light, PrototypeInteractionRole.DirectBinder, true, false, false, false),
                new TargetDefinition("ObjectProxy_02", "OBJ-P-L-001", "BINDER-ALLY-01", "FILE BINDER", PrototypeWeightClass.Light, PrototypeInteractionRole.AllyBinder, false, true, false, false),
                new TargetDefinition("ObjectProxy_03", "OBJ-P-L-001", "BINDER-ALLY-02", "FILE BINDER", PrototypeWeightClass.Light, PrototypeInteractionRole.AllyBinder, false, true, false, false),
                new TargetDefinition("ObjectProxy_04", "OBJ-P-M-001", "CHAIR-DIRECT-01", "OFFICE CHAIR", PrototypeWeightClass.Medium, PrototypeInteractionRole.DirectChair, true, false, true, true),
                new TargetDefinition("ObjectProxy_05", "OBJ-P-M-001", "CHAIR-ALLY-01", "OFFICE CHAIR", PrototypeWeightClass.Medium, PrototypeInteractionRole.AllyChair, false, false, true, true),
                new TargetDefinition("ObjectProxy_06", "OBJ-P-M-002", "DRAWER-01", "ROLLING DRAWER", PrototypeWeightClass.Medium, PrototypeInteractionRole.Drawer, false, false, false, false),
                new TargetDefinition("ObjectProxy_07", "OBJ-P-H-001", "COPIER-01", "COPIER", PrototypeWeightClass.Heavy, PrototypeInteractionRole.Copier, false, false, false, false)
            };

            var targets = new List<PrototypeInteractionTarget>(definitions.Length);
            foreach (TargetDefinition definition in definitions)
            {
                GameObject targetObject = RequireRoot(scene, definition.ObjectName);
                PrototypeInteractionTarget target =
                    GetOrAddComponent<PrototypeInteractionTarget>(targetObject);
                target.Configure(
                    definition.ContentDefinitionId,
                    definition.RuntimeInstanceKey,
                    definition.DisplayName,
                    definition.WeightClass,
                    definition.Role,
                    definition.DirectAcquisitionEligible,
                    definition.CatchEligible,
                    definition.StrapEligible,
                    definition.HasValidAnchor);
                EditorUtility.SetDirty(target);
                targets.Add(target);
            }

            return targets
                .OrderBy(target => target.RuntimeInstanceKey, StringComparer.Ordinal)
                .ToArray();
        }

        private static GameObject RequireRoot(Scene scene, string name)
        {
            GameObject result = scene.GetRootGameObjects()
                .FirstOrDefault(root => string.Equals(root.name, name, StringComparison.Ordinal));
            if (result == null)
            {
                throw new InvalidOperationException(
                    $"Required authored contest object is missing: {name}");
            }

            return result;
        }

        private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        private static void ConfigureDynamicTarget(
            PrototypeInteractionTarget target,
            float provisionalMass)
        {
            target.gameObject.isStatic = false;
            Rigidbody body = GetOrAddComponent<Rigidbody>(target.gameObject);
            body.mass = provisionalMass;
            body.useGravity = true;
            body.isKinematic = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.constraints = RigidbodyConstraints.None;
            EditorUtility.SetDirty(body);
        }

        private readonly struct TargetDefinition
        {
            public TargetDefinition(
                string objectName,
                string contentDefinitionId,
                string runtimeInstanceKey,
                string displayName,
                PrototypeWeightClass weightClass,
                PrototypeInteractionRole role,
                bool directAcquisitionEligible,
                bool catchEligible,
                bool strapEligible,
                bool hasValidAnchor)
            {
                ObjectName = objectName;
                ContentDefinitionId = contentDefinitionId;
                RuntimeInstanceKey = runtimeInstanceKey;
                DisplayName = displayName;
                WeightClass = weightClass;
                Role = role;
                DirectAcquisitionEligible = directAcquisitionEligible;
                CatchEligible = catchEligible;
                StrapEligible = strapEligible;
                HasValidAnchor = hasValidAnchor;
            }

            public string ObjectName { get; }
            public string ContentDefinitionId { get; }
            public string RuntimeInstanceKey { get; }
            public string DisplayName { get; }
            public PrototypeWeightClass WeightClass { get; }
            public PrototypeInteractionRole Role { get; }
            public bool DirectAcquisitionEligible { get; }
            public bool CatchEligible { get; }
            public bool StrapEligible { get; }
            public bool HasValidAnchor { get; }
        }
    }
}
