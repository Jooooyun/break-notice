using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Input;
using BreakNotice.Presentation;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class LogicalInputContractTests
    {
        private const string InputAssetPath = "Assets/_Project/Input/BreakNoticePrototype.inputactions";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Graybox/PlayerGraybox.prefab";
        private const string CameraPrefabPath = "Assets/_Project/Prefabs/Graybox/GameplayCameraRig.prefab";
        private const string ScenePath = "Assets/_Project/Scenes/10_Prototype/G4A_LocomotionCameraGraybox.unity";

        [OneTimeSetUp]
        public void CreateAuthorizedGrayboxAssets()
        {
            EnsureAuthorizedGrayboxAssets();
        }

        [Test]
        public void LogicalActionInventory_IsExactlyApprovedEight()
        {
            InputActionAsset asset = LoadInputAsset();
            Assert.That(asset.actionMaps.Count, Is.EqualTo(1));

            InputActionMap map = asset.actionMaps[0];
            Assert.That(map.name, Is.EqualTo("PrototypePlayer"));
            Assert.That(map.actions.Count, Is.EqualTo(8));
            Assert.That(
                map.actions.Select(action => action.name),
                Is.EquivalentTo(Enum.GetNames(typeof(PrototypeLogicalAction))));
        }

        [Test]
        public void MoveLookAim_ValueShapesMatchContract()
        {
            InputActionMap map = LoadInputAsset().FindActionMap("PrototypePlayer", true);

            Assert.That(map.FindAction("Move", true).type, Is.EqualTo(InputActionType.Value));
            Assert.That(map.FindAction("Move", true).expectedControlType, Is.EqualTo("Vector2"));
            Assert.That(map.FindAction("Look", true).type, Is.EqualTo(InputActionType.Value));
            Assert.That(map.FindAction("Look", true).expectedControlType, Is.EqualTo("Vector2"));
            Assert.That(map.FindAction("Aim", true).type, Is.EqualTo(InputActionType.Button));
            Assert.That(map.FindAction("Aim", true).expectedControlType, Is.EqualTo("Button"));
        }

        [Test]
        public void ApprovedInteractionAction_HasBoundedConsumerAndLaterActionsRemainPassive()
        {
            string[] passiveActions =
            {
                "Throw",
                "CallAlly",
                "StrapRelease",
                "ResetEncounter"
            };

            InputActionMap map = LoadInputAsset().FindActionMap("PrototypePlayer", true);
            foreach (string actionName in passiveActions)
            {
                Assert.That(map.FindAction(actionName, true).type, Is.EqualTo(InputActionType.Button));
            }

            string consumerSource =
                File.ReadAllText("Assets/_Project/Code/Runtime/Player/ThirdPersonLocomotion.cs") +
                File.ReadAllText("Assets/_Project/Code/Runtime/Presentation/ThirdPersonCameraRig.cs") +
                File.ReadAllText("Assets/_Project/Code/Runtime/Player/PrototypeInteractionController.cs");
            StringAssert.Contains("Snapshot.InteractPrimary", consumerSource);
            foreach (string actionName in passiveActions)
            {
                StringAssert.DoesNotContain(actionName, consumerSource);
            }
        }

        [Test]
        public void DeferredAndVerticalSliceActions_AreAbsent()
        {
            string[] absentActions = { "Sprint", "Evade", "Vault", "Shove", "HeavyAction" };
            InputActionMap map = LoadInputAsset().FindActionMap("PrototypePlayer", true);

            foreach (string actionName in absentActions)
            {
                Assert.That(map.FindAction(actionName, false), Is.Null);
            }
        }

        [Test]
        public void RuntimeAssemblyDependencies_AreOneWayAndBounded()
        {
            string gameplayAsmdef = File.ReadAllText(
                "Assets/_Project/Code/Runtime/Player/BreakNotice.Gameplay.asmdef");
            string presentationAsmdef = File.ReadAllText(
                "Assets/_Project/Code/Runtime/Presentation/BreakNotice.Presentation.asmdef");

            StringAssert.DoesNotContain("BreakNotice.Presentation", gameplayAsmdef);
            StringAssert.DoesNotContain("Unity.Cinemachine", gameplayAsmdef);
            StringAssert.DoesNotContain("UnityEditor", gameplayAsmdef);
            StringAssert.Contains("BreakNotice.Gameplay", presentationAsmdef);
            StringAssert.Contains("Unity.Cinemachine", presentationAsmdef);
            StringAssert.DoesNotContain("UnityEditor", presentationAsmdef);

            AssertNoMutableStaticFields(typeof(ThirdPersonLocomotion).Assembly);
            AssertNoMutableStaticFields(typeof(ThirdPersonCameraRig).Assembly);
        }

        [Test]
        public void GrayboxScene_LoadsAndContainsExactContractObjects()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);

            IReadOnlyList<GameObject> objects = GetSceneObjects(scene);
            Assert.That(objects.Count(item => item.name == "Player"), Is.EqualTo(1));
            Assert.That(objects.Count(item => item.name == "GameplayCameraRig"), Is.EqualTo(1));
            Assert.That(objects.Count(item => item.name == "AllyProxy"), Is.EqualTo(1));
            Assert.That(objects.Count(item => item.name.StartsWith("EnemyProxy_", StringComparison.Ordinal)), Is.EqualTo(2));
            Assert.That(objects.Count(item => item.name.StartsWith("ObjectProxy_", StringComparison.Ordinal)), Is.EqualTo(7));

            foreach (GameObject item in objects)
            {
                Assert.That(
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(item),
                    Is.Zero,
                    $"Missing script on {item.name}.");
            }

            LogAssert.NoUnexpectedReceived();
        }

        private static InputActionAsset LoadInputAsset()
        {
            InputActionAsset asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputAssetPath);
            Assert.That(asset, Is.Not.Null, $"Input asset was not found at {InputAssetPath}.");
            return asset;
        }

        private static void AssertNoMutableStaticFields(Assembly assembly)
        {
            IEnumerable<FieldInfo> fields = assembly
                .GetTypes()
                .SelectMany(type => type.GetFields(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Static |
                    BindingFlags.DeclaredOnly));

            FieldInfo[] mutableStaticFields = fields
                .Where(field => !field.IsLiteral && !field.IsInitOnly)
                .ToArray();

            Assert.That(mutableStaticFields, Is.Empty);
        }

        private static void EnsureAuthorizedGrayboxAssets()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/Graybox"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Graybox");
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath) == null)
            {
                CreatePlayerPrefab();
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(CameraPrefabPath) == null)
            {
                CreateCameraPrefab();
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                CreateGrayboxScene();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreatePlayerPrefab()
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "PlayerGraybox";
            UnityEngine.Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.center = Vector3.zero;
            controller.height = 2f;
            controller.radius = 0.45f;
            controller.stepOffset = 0.3f;

            InputSystemLogicalInputAdapter adapter = player.AddComponent<InputSystemLogicalInputAdapter>();
            player.AddComponent<ThirdPersonLocomotion>();

            Transform trackingTarget = new GameObject("CameraTrackingTarget").transform;
            trackingTarget.SetParent(player.transform, false);
            trackingTarget.localPosition = new Vector3(0f, 1.45f, 0f);

            SerializedObject adapterObject = new SerializedObject(adapter);
            adapterObject.FindProperty("inputActions").objectReferenceValue = LoadInputAsset();
            adapterObject.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
            UnityEngine.Object.DestroyImmediate(player);
        }

        private static void CreateCameraPrefab()
        {
            GameObject root = new GameObject("GameplayCameraRig");
            ThirdPersonCameraRig rig = root.AddComponent<ThirdPersonCameraRig>();

            Transform orbitTarget = new GameObject("OrbitTarget").transform;
            orbitTarget.SetParent(root.transform, false);

            GameObject outputCameraObject = new GameObject("OutputCamera");
            outputCameraObject.transform.SetParent(root.transform, false);
            Camera outputCamera = outputCameraObject.AddComponent<Camera>();
            outputCameraObject.AddComponent<CinemachineBrain>();

            GameObject virtualCameraObject = new GameObject("CinemachineCamera");
            virtualCameraObject.transform.SetParent(root.transform, false);
            CinemachineCamera virtualCamera = virtualCameraObject.AddComponent<CinemachineCamera>();
            CinemachineThirdPersonFollow thirdPersonFollow =
                virtualCameraObject.AddComponent<CinemachineThirdPersonFollow>();
            virtualCamera.Follow = orbitTarget;
            thirdPersonFollow.CameraDistance = 4.5f;
            thirdPersonFollow.ShoulderOffset = new Vector3(0.55f, 0.15f, 0f);
            CinemachineThirdPersonFollow.ObstacleSettings obstacleSettings =
                thirdPersonFollow.AvoidObstacles;
            obstacleSettings.Enabled = true;
            obstacleSettings.CollisionFilter = ~0;
            obstacleSettings.IgnoreTag = string.Empty;
            obstacleSettings.CameraRadius = 0.25f;
            obstacleSettings.DampingIntoCollision = 0f;
            obstacleSettings.DampingFromCollision = 0.5f;
            thirdPersonFollow.AvoidObstacles = obstacleSettings;

            SerializedObject rigObject = new SerializedObject(rig);
            rigObject.FindProperty("orbitTarget").objectReferenceValue = orbitTarget;
            rigObject.FindProperty("outputCamera").objectReferenceValue = outputCamera;
            rigObject.FindProperty("virtualCamera").objectReferenceValue = virtualCamera;
            rigObject.FindProperty("thirdPersonFollow").objectReferenceValue = thirdPersonFollow;
            rigObject.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, CameraPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static void CreateGrayboxScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateStaticPrimitive("Floor", PrimitiveType.Cube, new Vector3(0f, -0.25f, 0f), new Vector3(24f, 0.5f, 20f));
            CreateStaticPrimitive("Wall_North", PrimitiveType.Cube, new Vector3(0f, 2f, 10f), new Vector3(24f, 4f, 0.5f));
            CreateStaticPrimitive("Wall_South", PrimitiveType.Cube, new Vector3(0f, 2f, -10f), new Vector3(24f, 4f, 0.5f));
            CreateStaticPrimitive("Wall_East", PrimitiveType.Cube, new Vector3(12f, 2f, 0f), new Vector3(0.5f, 4f, 20f));
            CreateStaticPrimitive("Wall_West", PrimitiveType.Cube, new Vector3(-12f, 2f, 0f), new Vector3(0.5f, 4f, 20f));
            CreateStaticPrimitive("Obstacle_WideRoute", PrimitiveType.Cube, new Vector3(-3f, 1f, -1f), new Vector3(2f, 2f, 5f));
            CreateStaticPrimitive("Obstacle_NarrowRoute_A", PrimitiveType.Cube, new Vector3(3.5f, 1.25f, 1f), new Vector3(1.5f, 2.5f, 7f));
            CreateStaticPrimitive("Obstacle_NarrowRoute_B", PrimitiveType.Cube, new Vector3(7f, 1.25f, 1f), new Vector3(1.5f, 2.5f, 7f));
            CreateStaticPrimitive("ForegroundOccluder", PrimitiveType.Cube, new Vector3(-8f, 2f, -3f), new Vector3(1.25f, 4f, 3f));
            CreateStaticPrimitive("StartMarker", PrimitiveType.Cylinder, new Vector3(0f, 0.02f, -7f), new Vector3(1.2f, 0.04f, 1.2f));

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            GameObject cameraPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CameraPrefabPath);

            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            player.name = "Player";
            player.transform.position = new Vector3(0f, 1f, -7f);

            GameObject cameraRigObject = (GameObject)PrefabUtility.InstantiatePrefab(cameraPrefab, scene);
            cameraRigObject.name = "GameplayCameraRig";
            ThirdPersonCameraRig cameraRig = cameraRigObject.GetComponent<ThirdPersonCameraRig>();
            InputSystemLogicalInputAdapter logicalInput = player.GetComponent<InputSystemLogicalInputAdapter>();
            cameraRig.Configure(player.transform, logicalInput);
            player.GetComponent<ThirdPersonLocomotion>().ConfigureCamera(cameraRig.OutputCamera.transform);

            CreateProxy("AllyProxy", PrimitiveType.Capsule, new Vector3(-5f, 1f, 5f), new Vector3(0.85f, 1f, 0.85f));
            CreateProxy("EnemyProxy_01", PrimitiveType.Cylinder, new Vector3(3f, 1f, 6f), new Vector3(0.75f, 1f, 0.75f));
            CreateProxy("EnemyProxy_02", PrimitiveType.Cylinder, new Vector3(7f, 1f, 6f), new Vector3(0.75f, 1f, 0.75f));

            CreateProxy("ObjectProxy_01", PrimitiveType.Cube, new Vector3(-7f, 0.3f, 3f), new Vector3(0.6f, 0.6f, 0.25f));
            CreateProxy("ObjectProxy_02", PrimitiveType.Cube, new Vector3(-6f, 0.3f, 4f), new Vector3(0.6f, 0.6f, 0.25f));
            CreateProxy("ObjectProxy_03", PrimitiveType.Cube, new Vector3(-5f, 0.3f, 3f), new Vector3(0.6f, 0.6f, 0.25f));
            CreateProxy("ObjectProxy_04", PrimitiveType.Cube, new Vector3(0f, 0.65f, 4f), new Vector3(1.2f, 1.3f, 1.2f));
            CreateProxy("ObjectProxy_05", PrimitiveType.Cube, new Vector3(2f, 0.65f, 4f), new Vector3(1.2f, 1.3f, 1.2f));
            CreateProxy("ObjectProxy_06", PrimitiveType.Cube, new Vector3(5f, 0.9f, -5f), new Vector3(1.4f, 1.8f, 0.9f));
            CreateProxy("ObjectProxy_07", PrimitiveType.Cube, new Vector3(-5f, 1.1f, -5f), new Vector3(1.8f, 2.2f, 1.1f));

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static GameObject CreateStaticPrimitive(
            string name,
            PrimitiveType primitiveType,
            Vector3 position,
            Vector3 scale)
        {
            GameObject item = GameObject.CreatePrimitive(primitiveType);
            item.name = name;
            item.transform.position = position;
            item.transform.localScale = scale;
            item.isStatic = true;
            return item;
        }

        private static GameObject CreateProxy(
            string name,
            PrimitiveType primitiveType,
            Vector3 position,
            Vector3 scale)
        {
            return CreateStaticPrimitive(name, primitiveType, position, scale);
        }

        private static IReadOnlyList<GameObject> GetSceneObjects(Scene scene)
        {
            List<GameObject> objects = new List<GameObject>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
                {
                    objects.Add(transform.gameObject);
                }
            }

            return objects;
        }
    }
}
