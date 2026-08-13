using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Encounter;
using BreakNotice.Gameplay.Input;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace BreakNotice.Contest.Editor
{
    public static class ContestShowcaseSceneAuthoring
    {
        public const string SourceScenePath =
            "Assets/_Project/Scenes/10_Prototype/Contest_SingleRoomGraybox.unity";

        public const string ShowcaseSceneFolder =
            "Assets/_Project/Scenes/20_Showcase";

        public const string ShowcaseScenePath =
            ShowcaseSceneFolder + "/Contest_OperationsFloor_Showcase_v001.unity";

        public const string ShowcaseV2ScenePath =
            ShowcaseSceneFolder + "/Contest_OperationsFloor_Showcase_v002.unity";

        public const string ShowcaseS3ScenePath =
            ShowcaseSceneFolder + "/Contest_OperationsFloor_Showcase_v003.unity";

        public const string ShowcaseS4ScenePath =
            ShowcaseSceneFolder + "/Contest_OperationsFloor_Showcase_v004.unity";

        public const string ShowcaseRootName = "SHOWCASE_S1_GRAYBOX";

        public const string ShowcaseV2RootName =
            "SHOWCASE_V2_PROVISIONAL_LOCAL_REVIEW_ONLY";

        public const string MobileAllyRootName = "SHOWCASE_S2_MOBILE_ALLY";

        public const string ShowcaseS3RootName = "SHOWCASE_S3_FETCH_SUPPLY";

        public const string ShowcaseS4RootName = "SHOWCASE_S4_TEAM_CONTEXT";

        private const string V2ReviewCaptureFolderName =
            "break-notice-showcase-v2-readability";

        private static readonly Color[] V2ZoneColors =
        {
            new Color(0.20f, 0.82f, 1.00f, 1f),
            new Color(0.35f, 0.95f, 0.72f, 1f),
            new Color(1.00f, 0.77f, 0.22f, 1f),
            new Color(0.30f, 0.62f, 1.00f, 1f),
            new Color(0.78f, 0.50f, 1.00f, 1f),
            new Color(1.00f, 0.46f, 0.30f, 1f),
            new Color(1.00f, 0.32f, 0.58f, 1f),
            new Color(0.48f, 1.00f, 0.40f, 1f)
        };

        private static readonly string[] LegacyEnvironmentRootNames =
        {
            "Floor",
            "Wall_North",
            "Wall_South",
            "Wall_East",
            "Wall_West",
            "Obstacle_NarrowRoute_A",
            "Obstacle_NarrowRoute_B",
            "Obstacle_WideRoute",
            "ForegroundOccluder",
            "StartMarker"
        };

        [MenuItem("BREAK NOTICE/Contest/Prepare S1 Showcase Graybox")]
        public static void PrepareShowcaseScene()
        {
            RequireSceneAsset(SourceScenePath);
            EnsureShowcaseFolder();

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ShowcaseScenePath) == null)
            {
                if (!AssetDatabase.CopyAsset(SourceScenePath, ShowcaseScenePath))
                {
                    throw new InvalidOperationException(
                        $"Could not copy the preserved C0-C4 Scene to {ShowcaseScenePath}.");
                }

                AssetDatabase.ImportAsset(
                    ShowcaseScenePath,
                    ImportAssetOptions.ForceSynchronousImport);
            }

            Scene scene = EditorSceneManager.OpenScene(ShowcaseScenePath, OpenSceneMode.Single);
            ApplyS1Layout(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ShowcaseScenePath))
            {
                throw new InvalidOperationException($"Could not save {ShowcaseScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log(
                "S1 Showcase graybox prepared: 48m x 34m envelope, eight zones, " +
                "figure-eight topology, preserved C0-C4 gameplay roots, no Production Art.");
        }

        [MenuItem("BREAK NOTICE/Contest/Prepare S2 Mobile Ally")]
        public static void PrepareS2MobileAlly()
        {
            RequireSceneAsset(ShowcaseScenePath);
            Scene scene = EditorSceneManager.OpenScene(ShowcaseScenePath, OpenSceneMode.Single);
            ApplyS2MobileAlly(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ShowcaseScenePath))
            {
                throw new InvalidOperationException($"Could not save {ShowcaseScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log(
                "S2 Showcase mobile ally prepared: 12 authored follow anchors, " +
                "side switching, catch-up, blocked/regroup and Reset-safe restoration.");
        }

        [MenuItem("BREAK NOTICE/Contest/Prepare Provisional V2 Layout")]
        public static void PrepareV2ShowcaseScene()
        {
            RequireSceneAsset(ShowcaseScenePath);
            EnsureShowcaseFolder();

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ShowcaseV2ScenePath) == null)
            {
                if (!AssetDatabase.CopyAsset(ShowcaseScenePath, ShowcaseV2ScenePath))
                {
                    throw new InvalidOperationException(
                        $"Could not copy the preserved v001 Scene to {ShowcaseV2ScenePath}.");
                }

                AssetDatabase.ImportAsset(
                    ShowcaseV2ScenePath,
                    ImportAssetOptions.ForceSynchronousImport);
            }

            Scene scene = EditorSceneManager.OpenScene(
                ShowcaseV2ScenePath,
                OpenSceneMode.Single);
            ApplyV2Layout(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ShowcaseV2ScenePath))
            {
                throw new InvalidOperationException(
                    $"Could not save {ShowcaseV2ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log(
                "Provisional v002 layout prepared: LOCAL_REVIEW_ONLY 64m x 44m envelope, " +
                "eight-zone figure-eight, S2 mobile ally, 60 Designer B replacement " +
                "sockets, zero Production Art integration and no S3 implementation.");
        }

        [MenuItem("BREAK NOTICE/Contest/Prepare S3 Fetch And Supply")]
        public static void PrepareS3ShowcaseScene()
        {
            RequireSceneAsset(ShowcaseV2ScenePath);
            EnsureShowcaseFolder();

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ShowcaseS3ScenePath) == null)
            {
                if (!AssetDatabase.CopyAsset(ShowcaseV2ScenePath, ShowcaseS3ScenePath))
                {
                    throw new InvalidOperationException(
                        $"Could not copy the accepted v002 Scene to {ShowcaseS3ScenePath}.");
                }

                AssetDatabase.ImportAsset(
                    ShowcaseS3ScenePath,
                    ImportAssetOptions.ForceSynchronousImport);
            }

            Scene scene = EditorSceneManager.OpenScene(
                ShowcaseS3ScenePath,
                OpenSceneMode.Single);
            ApplyS3FetchAndSupply(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ShowcaseS3ScenePath))
            {
                throw new InvalidOperationException(
                    $"Could not save {ShowcaseS3ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log(
                "S3 Showcase prepared: explicit F fetch, mobile Binder/Chair return, " +
                "Q supply, ten physical objects and authored Reset restoration.");
        }

        [MenuItem("BREAK NOTICE/Contest/Prepare S4 Team Context")]
        public static void PrepareS4ShowcaseScene()
        {
            RequireSceneAsset(ShowcaseS3ScenePath);
            EnsureShowcaseFolder();

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ShowcaseS4ScenePath) == null)
            {
                if (!AssetDatabase.CopyAsset(ShowcaseS3ScenePath, ShowcaseS4ScenePath))
                {
                    throw new InvalidOperationException(
                        $"Could not copy the accepted S3 Scene to {ShowcaseS4ScenePath}.");
                }

                AssetDatabase.ImportAsset(
                    ShowcaseS4ScenePath,
                    ImportAssetOptions.ForceSynchronousImport);
            }

            Scene scene = EditorSceneManager.OpenScene(
                ShowcaseS4ScenePath,
                OpenSceneMode.Single);
            ApplyS4TeamContext(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ShowcaseS4ScenePath))
            {
                throw new InvalidOperationException(
                    $"Could not save {ShowcaseS4ScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log(
                "S4 Showcase prepared: singular Q arbitration, live PASS badge, " +
                "TEAM READY ownership and bounded S5-S6 readiness hooks.");
        }

        [MenuItem("BREAK NOTICE/Contest/Capture Provisional V2 Readability Review")]
        public static void CaptureV2ReadabilityReview()
        {
            RequireSceneAsset(ShowcaseV2ScenePath);
            Scene scene = EditorSceneManager.OpenScene(
                ShowcaseV2ScenePath,
                OpenSceneMode.Single);

            string outputFolder = Path.Combine(
                Path.GetTempPath(),
                V2ReviewCaptureFolderName);
            Directory.CreateDirectory(outputFolder);

            string overviewPath = Path.Combine(
                outputFolder,
                "overview-1920x1080.png");
            string playerCameraPath = Path.Combine(
                outputFolder,
                "player-camera-1920x1080.png");

            GameObject player = RequireRootAny(
                scene,
                "GAMEPLAY_CHR00_PLAYER",
                "Player");
            Camera sourceCamera = RequireSingleSceneComponent<Camera>(scene);

            CaptureV2ReviewFrame(
                scene,
                overviewPath,
                "TEMP_REVIEW_OVERVIEW_CAMERA",
                new Vector3(0f, 48f, -1f),
                Quaternion.Euler(90f, 0f, 0f),
                true,
                24f,
                sourceCamera.fieldOfView);

            Vector3 playerReviewCameraPosition =
                player.transform.position + new Vector3(4.2f, 2.4f, -2.8f);
            Vector3 playerReviewFocus =
                player.transform.position + new Vector3(0f, -0.2f, 7f);
            CaptureV2ReviewFrame(
                scene,
                playerCameraPath,
                "TEMP_REVIEW_PLAYER_CAMERA",
                playerReviewCameraPosition,
                Quaternion.LookRotation(
                    playerReviewFocus - playerReviewCameraPosition,
                    Vector3.up),
                false,
                0f,
                sourceCamera.fieldOfView);

            Debug.Log(
                $"V2 readability review captures: {overviewPath} | {playerCameraPath}");
        }

        public static void ApplyS1Layout(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new ArgumentException("A valid loaded Scene is required.", nameof(scene));
            }

            ValidatePreservedGameplayRoots(scene);
            RemoveS2MobileAlly(scene);
            RemoveLegacyEnvironment(scene);

            GameObject previousRoot = FindRoot(scene, ShowcaseRootName);
            if (previousRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(previousRoot);
            }

            GameObject showcaseRoot = CreateRoot(scene, ShowcaseRootName);
            Transform envelope = CreateGroup(showcaseRoot.transform, "00_ENVELOPE");
            Transform zones = CreateGroup(showcaseRoot.transform, "01_ZONES");
            Transform topology = CreateGroup(showcaseRoot.transform, "02_FIGURE_EIGHT");
            Transform cubicleIsland = CreateGroup(showcaseRoot.transform, "03_CUBICLE_ISLAND");
            Transform layoutMarkers = CreateGroup(showcaseRoot.transform, "04_LAYOUT_MARKERS");
            Transform staticFurniture = CreateGroup(showcaseRoot.transform, "05_STATIC_FURNITURE");

            BuildEnvelope(envelope);
            BuildZoneMarkers(zones);
            BuildFigureEight(topology);
            BuildCubicleIsland(cubicleIsland);
            BuildLayoutMarkers(layoutMarkers);
            BuildStaticFurniture(staticFurniture);
            RepositionPreservedGameplay(scene);

            ValidatePreservedGameplayRoots(scene);
        }

        public static void ApplyS2MobileAlly(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new ArgumentException("A valid loaded Scene is required.", nameof(scene));
            }

            RemoveS2MobileAlly(scene);
            GameObject player = RequireRoot(scene, "Player");
            GameObject ally = RequireRoot(scene, "AllyProxy");
            GameObject browserInput = RequireRoot(scene, "ContestBrowserInput");
            GameObject mobileRoot = CreateRoot(scene, MobileAllyRootName);
            Transform[] anchors = BuildFollowAnchorNetwork(mobileRoot.transform);
            Vector2Int[] links = BuildFollowAnchorLinks();

            CapsuleCollider legacyCollider = ally.GetComponent<CapsuleCollider>();
            if (legacyCollider != null)
            {
                legacyCollider.enabled = false;
            }

            CharacterController allyController = ally.AddComponent<CharacterController>();
            allyController.center = Vector3.zero;
            allyController.radius = 0.48f;
            allyController.height = 1.9f;
            allyController.skinWidth = 0.06f;
            allyController.stepOffset = 0.2f;
            allyController.slopeLimit = 45f;

            PrototypeAllyRelayController relay = ally.GetComponent<PrototypeAllyRelayController>();
            if (relay == null)
            {
                throw new InvalidOperationException("The preserved C4 Ally relay is required.");
            }
            relay.enabled = false;

            PrototypeInteractionStateOwner owner =
                player.GetComponent<PrototypeInteractionStateOwner>();
            PrototypeInteractionController interaction =
                player.GetComponent<PrototypeInteractionController>();
            AuthoredEncounterResetController reset = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<AuthoredEncounterResetController>(true))
                .Single();

            ShowcaseMobileAllyController mobile =
                ally.AddComponent<ShowcaseMobileAllyController>();
            mobile.Configure(
                owner,
                reset,
                ally.transform,
                player.transform,
                allyController,
                anchors,
                links);
            interaction.ConfigureShowcaseMobileAlly(mobile);

            ShowcaseMobileAllyPresenter presenter =
                browserInput.AddComponent<ShowcaseMobileAllyPresenter>();
            presenter.Configure(mobile);
            EditorUtility.SetDirty(ally);
            EditorUtility.SetDirty(player);
            EditorUtility.SetDirty(browserInput);
        }

        public static void ApplyV2Layout(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new ArgumentException("A valid loaded Scene is required.", nameof(scene));
            }

            ValidatePreservedGameplayRoots(scene);
            RequireSingleSceneComponent<ShowcaseMobileAllyController>(scene);

            GameObject previousV2Root = FindRoot(scene, ShowcaseV2RootName);
            if (previousV2Root != null)
            {
                UnityEngine.Object.DestroyImmediate(previousV2Root);
            }

            GameObject previousV1Root = FindRoot(scene, ShowcaseRootName);
            if (previousV1Root != null)
            {
                UnityEngine.Object.DestroyImmediate(previousV1Root);
            }

            GameObject previousS2Root = FindRoot(scene, MobileAllyRootName);
            if (previousS2Root != null)
            {
                UnityEngine.Object.DestroyImmediate(previousS2Root);
            }

            RemoveLegacyEnvironment(scene);

            GameObject root = CreateRoot(scene, ShowcaseV2RootName);
            Transform status = CreateGroup(root.transform, "00_STATUS");
            Transform staticCollision = CreateGroup(root.transform, "01_STATIC_COLLISION");
            Transform zones = CreateGroup(root.transform, "02_ZONES");
            Transform topology = CreateGroup(root.transform, "03_FIGURE_EIGHT");
            Transform cubicleIsland = CreateGroup(root.transform, "04_CUBICLE_ISLAND");
            Transform layoutMarkers = CreateGroup(root.transform, "05_LAYOUT_MARKERS");
            Transform gameplayIndex = CreateGroup(root.transform, "06_GAMEPLAY_INDEX");
            Transform s2MobileAlly = CreateGroup(root.transform, "07_S2_MOBILE_ALLY");
            Transform reviewProxies = CreateGroup(root.transform, "08_S3_NOT_IMPLEMENTED_REVIEW_PROXIES");
            Transform assetSockets = CreateGroup(root.transform, "09_DESIGNER_B_REPLACEMENT_SOCKETS");
            Transform visualReadability = CreateGroup(root.transform, "10_VISUAL_READABILITY");

            BuildV2Status(status);
            BuildV2Envelope(staticCollision);
            BuildV2ZoneMarkers(zones);
            BuildV2FigureEight(topology);
            BuildV2CubicleIsland(cubicleIsland);
            BuildV2StaticFurniture(staticCollision);
            BuildV2LayoutMarkers(layoutMarkers);
            RepositionV2Gameplay(scene, gameplayIndex);
            ConfigureV2MobileAlly(scene, s2MobileAlly);
            BuildV2FutureS3ReviewProxies(reviewProxies);
            BuildV2AssetSockets(assetSockets);
            BuildV2VisualReadability(visualReadability);

            ValidatePreservedGameplayRoots(scene);
            if (RequireSingleSceneComponent<ShowcaseMobileAllyController>(scene).AnchorCount != 18)
            {
                throw new InvalidOperationException(
                    "The provisional v002 Scene requires exactly 18 connected S2 anchors.");
            }
        }

        public static void ApplyS3FetchAndSupply(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new ArgumentException("A valid loaded Scene is required.", nameof(scene));
            }

            RemoveS3RuntimeContent(scene);
            ValidatePreservedGameplayRoots(scene);

            GameObject player = RequireRootAny(scene, "GAMEPLAY_CHR00_PLAYER", "Player");
            GameObject ally = RequireRootAny(scene, "GAMEPLAY_CHR01_MOBILE_ALLY", "AllyProxy");
            GameObject runnerOne = RequireRootAny(
                scene,
                "GAMEPLAY_ENY01_RUNNER_01",
                "EnemyProxy_01");
            GameObject runnerTwo = RequireRootAny(
                scene,
                "GAMEPLAY_ENY01_RUNNER_02",
                "EnemyProxy_02");
            GameObject s3Root = CreateRoot(scene, ShowcaseS3RootName);
            Transform s3Status = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .SingleOrDefault(item => string.Equals(
                    item.name,
                    "S3_NOT_IMPLEMENTED",
                    StringComparison.Ordinal));
            if (s3Status != null)
            {
                s3Status.name = "S3_IMPLEMENTED_FETCH_SUPPLY";
                EditorUtility.SetDirty(s3Status);
            }

            CreateS3Target(
                scene,
                s3Root.transform,
                "BINDER-DIRECT-01",
                "GAMEPLAY_BINDER_DIRECT_02",
                "OBJ-P-L-001",
                "BINDER-DIRECT-02",
                "FILE BINDER",
                PrototypeWeightClass.Light,
                PrototypeInteractionRole.DirectBinder,
                new Vector3(15f, 0.16f, -11f),
                true,
                false,
                false,
                false);
            CreateS3Target(
                scene,
                s3Root.transform,
                "BINDER-ALLY-01",
                "GAMEPLAY_BINDER_ALLY_03",
                "OBJ-P-L-001",
                "BINDER-ALLY-03",
                "FILE BINDER",
                PrototypeWeightClass.Light,
                PrototypeInteractionRole.AllyBinder,
                new Vector3(12f, 0.16f, 17.5f),
                false,
                true,
                false,
                false);
            CreateS3Target(
                scene,
                s3Root.transform,
                "CHAIR-DIRECT-01",
                "GAMEPLAY_CHAIR_DIRECT_02",
                "OBJ-P-M-001",
                "CHAIR-DIRECT-02",
                "OFFICE CHAIR",
                PrototypeWeightClass.Medium,
                PrototypeInteractionRole.DirectChair,
                new Vector3(17f, 0.499f, -12f),
                true,
                false,
                true,
                true);

            PrototypeInteractionTarget[] targets = GetInteractionTargets(scene);
            ValidateS3GameplayTargets(targets);
            PrototypeInteractionTarget[] supplyTargets = targets
                .Where(target => target.Role == PrototypeInteractionRole.AllyBinder ||
                                 target.Role == PrototypeInteractionRole.AllyChair)
                .OrderBy(target => target.RuntimeInstanceKey, StringComparer.Ordinal)
                .ToArray();

            PrototypeInteractionStateOwner owner =
                player.GetComponent<PrototypeInteractionStateOwner>();
            PrototypeInteractionController interaction =
                player.GetComponent<PrototypeInteractionController>();
            PrototypePhysicalObjectLifecycle lifecycle =
                player.GetComponent<PrototypePhysicalObjectLifecycle>();
            ThirdPersonLocomotion locomotion = player.GetComponent<ThirdPersonLocomotion>();
            InputSystemLogicalInputAdapter input =
                player.GetComponent<InputSystemLogicalInputAdapter>();
            PrototypeAllyRelayController relay =
                ally.GetComponent<PrototypeAllyRelayController>();
            ShowcaseMobileAllyController mobile =
                ally.GetComponent<ShowcaseMobileAllyController>();
            AuthoredEncounterResetController reset =
                RequireSingleSceneComponent<AuthoredEncounterResetController>(scene);
            if (owner == null || interaction == null || lifecycle == null || locomotion == null ||
                input == null || relay == null || mobile == null)
            {
                throw new InvalidOperationException(
                    "S3 requires the preserved player, relay, mobile Ally and input composition.");
            }

            relay.enabled = true;
            relay.ConfigureShowcaseFetch(mobile, supplyTargets, true);
            interaction.ConfigureShowcaseS3(mobile, relay, targets);
            reset.Configure(
                input,
                owner,
                lifecycle,
                locomotion,
                player.transform,
                new[] { ally.transform, runnerOne.transform, runnerTwo.transform },
                targets,
                relay,
                10);

            EditorUtility.SetDirty(player);
            EditorUtility.SetDirty(ally);
            EditorUtility.SetDirty(interaction);
            EditorUtility.SetDirty(relay);
            EditorUtility.SetDirty(mobile);
            EditorUtility.SetDirty(reset);
            ValidateS3GameplayTargets(GetInteractionTargets(scene));
        }

        public static void ApplyS4TeamContext(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new ArgumentException("A valid loaded Scene is required.", nameof(scene));
            }

            GameObject previousRoot = FindRoot(scene, ShowcaseS4RootName);
            if (previousRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(previousRoot);
            }
            CreateRoot(scene, ShowcaseS4RootName);

            GameObject player = RequireRootAny(
                scene,
                "GAMEPLAY_CHR00_PLAYER",
                "Player");
            GameObject ally = RequireRootAny(
                scene,
                "GAMEPLAY_CHR01_MOBILE_ALLY",
                "GAMEPLAY_CHR01_ALLY",
                "AllyProxy");
            GameObject browserInput = RequireRoot(scene, "ContestBrowserInput");
            PrototypeInteractionStateOwner owner =
                player.GetComponent<PrototypeInteractionStateOwner>();
            PrototypeInteractionController interaction =
                player.GetComponent<PrototypeInteractionController>();
            PrototypeAllyRelayController relay =
                ally.GetComponent<PrototypeAllyRelayController>();
            ShowcaseMobileAllyController mobile =
                ally.GetComponent<ShowcaseMobileAllyController>();
            AuthoredEncounterResetController reset =
                RequireSingleSceneComponent<AuthoredEncounterResetController>(scene);
            if (owner == null || interaction == null || relay == null || mobile == null)
            {
                throw new InvalidOperationException(
                    "S4 requires the accepted S3 player, relay and mobile Ally composition.");
            }

            ShowcaseTeamContextController teamContext =
                player.GetComponent<ShowcaseTeamContextController>();
            if (teamContext == null)
            {
                teamContext = player.AddComponent<ShowcaseTeamContextController>();
            }
            teamContext.Configure(owner, relay, mobile, reset);
            interaction.ConfigureShowcaseS4(teamContext);

            ShowcaseTeamContextPresenter teamPresenter =
                browserInput.GetComponent<ShowcaseTeamContextPresenter>();
            if (teamPresenter == null)
            {
                teamPresenter = browserInput.AddComponent<ShowcaseTeamContextPresenter>();
            }
            teamPresenter.Configure(teamContext);
            ShowcaseMobileAllyPresenter mobilePresenter =
                browserInput.GetComponent<ShowcaseMobileAllyPresenter>();
            mobilePresenter?.Configure(mobile, teamContext);

            EditorUtility.SetDirty(player);
            EditorUtility.SetDirty(browserInput);
            EditorUtility.SetDirty(interaction);
            EditorUtility.SetDirty(teamContext);
            EditorUtility.SetDirty(teamPresenter);
            if (mobilePresenter != null)
            {
                EditorUtility.SetDirty(mobilePresenter);
            }
        }

        private static Transform[] BuildFollowAnchorNetwork(Transform parent)
        {
            Vector2[] positions =
            {
                new Vector2(-2f, -14f),
                new Vector2(-3f, -10f),
                new Vector2(-4f, -5f),
                new Vector2(0f, 3f),
                new Vector2(-14f, -5f),
                new Vector2(-17f, 7f),
                new Vector2(-7f, 12f),
                new Vector2(12f, -4f),
                new Vector2(14f, 5f),
                new Vector2(11f, 9f),
                new Vector2(14f, 11f),
                new Vector2(19f, 15f)
            };
            var anchors = new Transform[positions.Length];
            for (int index = 0; index < positions.Length; index++)
            {
                anchors[index] = CreateMarker(
                    parent,
                    $"FA-{index:00}",
                    new Vector3(positions[index].x, 1f, positions[index].y),
                    Vector3.one);
            }
            return anchors;
        }

        private static Vector2Int[] BuildFollowAnchorLinks()
        {
            return new[]
            {
                new Vector2Int(0, 1),
                new Vector2Int(1, 2),
                new Vector2Int(2, 3),
                new Vector2Int(2, 4),
                new Vector2Int(4, 5),
                new Vector2Int(5, 6),
                new Vector2Int(6, 3),
                new Vector2Int(3, 7),
                new Vector2Int(7, 8),
                new Vector2Int(8, 9),
                new Vector2Int(9, 10),
                new Vector2Int(10, 11),
                new Vector2Int(6, 9)
            };
        }

        private static void RemoveS2MobileAlly(Scene scene)
        {
            GameObject player = FindRoot(scene, "Player");
            if (player != null)
            {
                PrototypeInteractionController interaction =
                    player.GetComponent<PrototypeInteractionController>();
                interaction?.ConfigureShowcaseMobileAlly(null);
            }

            GameObject ally = FindRoot(scene, "AllyProxy");
            if (ally != null)
            {
                ShowcaseMobileAllyController mobile =
                    ally.GetComponent<ShowcaseMobileAllyController>();
                if (mobile != null)
                {
                    UnityEngine.Object.DestroyImmediate(mobile);
                }
                CharacterController controller = ally.GetComponent<CharacterController>();
                if (controller != null)
                {
                    UnityEngine.Object.DestroyImmediate(controller);
                }
                CapsuleCollider legacyCollider = ally.GetComponent<CapsuleCollider>();
                if (legacyCollider != null)
                {
                    legacyCollider.enabled = true;
                }
                PrototypeAllyRelayController relay = ally.GetComponent<PrototypeAllyRelayController>();
                if (relay != null)
                {
                    relay.enabled = true;
                }
            }

            GameObject browserInput = FindRoot(scene, "ContestBrowserInput");
            if (browserInput != null)
            {
                ShowcaseMobileAllyPresenter presenter =
                    browserInput.GetComponent<ShowcaseMobileAllyPresenter>();
                if (presenter != null)
                {
                    UnityEngine.Object.DestroyImmediate(presenter);
                }
            }

            GameObject previousRoot = FindRoot(scene, MobileAllyRootName);
            if (previousRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(previousRoot);
            }
        }

        private static void RemoveS3RuntimeContent(Scene scene)
        {
            GameObject existing = FindRoot(scene, ShowcaseS3RootName);
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing);
            }

            GameObject v2Root = FindRoot(scene, ShowcaseV2RootName);
            Transform reviewProxies = v2Root != null
                ? v2Root.transform.Find("08_S3_NOT_IMPLEMENTED_REVIEW_PROXIES")
                : null;
            if (reviewProxies != null)
            {
                UnityEngine.Object.DestroyImmediate(reviewProxies.gameObject);
            }
        }

        private static PrototypeInteractionTarget CreateS3Target(
            Scene scene,
            Transform parent,
            string sourceKey,
            string objectName,
            string definitionId,
            string instanceKey,
            string displayName,
            PrototypeWeightClass weightClass,
            PrototypeInteractionRole role,
            Vector3 position,
            bool direct,
            bool catchEligible,
            bool strapEligible,
            bool hasAnchor)
        {
            PrototypeInteractionTarget source = GetInteractionTargets(scene)
                .SingleOrDefault(target => string.Equals(
                    target.RuntimeInstanceKey,
                    sourceKey,
                    StringComparison.Ordinal));
            if (source == null)
            {
                throw new InvalidOperationException(
                    $"S3 source target is missing: {sourceKey}");
            }

            GameObject clone = UnityEngine.Object.Instantiate(source.gameObject, parent);
            clone.name = objectName;
            clone.transform.SetPositionAndRotation(position, Quaternion.identity);
            PrototypeInteractionTarget target = clone.GetComponent<PrototypeInteractionTarget>();
            target.Configure(
                definitionId,
                instanceKey,
                displayName,
                weightClass,
                role,
                direct,
                catchEligible,
                strapEligible,
                hasAnchor);
            EditorUtility.SetDirty(clone);
            EditorUtility.SetDirty(target);
            return target;
        }

        private static void ValidateS3GameplayTargets(
            PrototypeInteractionTarget[] targets)
        {
            string[] expectedKeys =
            {
                "BINDER-ALLY-01",
                "BINDER-ALLY-02",
                "BINDER-ALLY-03",
                "BINDER-DIRECT-01",
                "BINDER-DIRECT-02",
                "CHAIR-ALLY-01",
                "CHAIR-DIRECT-01",
                "CHAIR-DIRECT-02",
                "COPIER-01",
                "DRAWER-01"
            };
            string[] actualKeys = targets
                .Select(target => target.RuntimeInstanceKey)
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();
            if (!actualKeys.SequenceEqual(expectedKeys))
            {
                throw new InvalidOperationException(
                    "S3 requires exactly five Binders, three Chairs, one Drawer and one Copier.");
            }

            if (targets.Count(target => target.Role == PrototypeInteractionRole.AllyBinder) != 3 ||
                targets.Count(target => target.Role == PrototypeInteractionRole.AllyChair) != 1 ||
                targets.Count(target => target.Role == PrototypeInteractionRole.DirectBinder) != 2 ||
                targets.Count(target => target.Role == PrototypeInteractionRole.DirectChair) != 2)
            {
                throw new InvalidOperationException(
                    "S3 player-only and Ally-fetch object pools do not match the Owner decision.");
            }
        }

        private static void BuildV2Status(Transform parent)
        {
            string[] statusMarkers =
            {
                "LOCAL_REVIEW_ONLY",
                "PROVISIONAL_LAYOUT",
                "NOT_OWNER_APPROVED",
                "NOT_FINAL_MAP_LOCK",
                "NOT_PRODUCTION_ART",
                "NOT_S9_INTEGRATED",
                "S2_INCLUDED",
                "S3_NOT_IMPLEMENTED",
                "DESIGNER_B_RUNTIME_INTEGRATION_0"
            };

            foreach (string marker in statusMarkers)
            {
                CreateMarker(parent, marker, Vector3.zero, Vector3.one);
            }
        }

        private static void BuildV2Envelope(Transform parent)
        {
            Transform envelope = CreateGroup(parent, "STATIC_ENVELOPE");
            CreateCube(envelope, "PROXY_Floor_64x44", new Vector3(0f, -0.25f, 0f),
                new Vector3(64f, 0.5f, 44f), true);
            CreateCube(envelope, "PROXY_Wall_South", new Vector3(0f, 2.1f, -22.25f),
                new Vector3(64.5f, 4.2f, 0.5f), true);
            CreateCube(envelope, "PROXY_Wall_West", new Vector3(-32.25f, 2.1f, 0f),
                new Vector3(0.5f, 4.2f, 44.5f), true);
            CreateCube(envelope, "PROXY_Wall_East", new Vector3(32.25f, 2.1f, 0f),
                new Vector3(0.5f, 4.2f, 44.5f), true);

            // The provisional Exit opening is 4.5 m wide at X 26.0.
            CreateCube(envelope, "PROXY_Wall_North_West", new Vector3(-4.125f, 2.1f, 22.25f),
                new Vector3(55.75f, 4.2f, 0.5f), true);
            CreateCube(envelope, "PROXY_Wall_North_East", new Vector3(30.125f, 2.1f, 22.25f),
                new Vector3(3.75f, 4.2f, 0.5f), true);
            CreateCube(envelope, "PROXY_Exit_Frame_Lintel", new Vector3(26f, 3.6f, 22.25f),
                new Vector3(4.5f, 1.2f, 0.5f), true);

            CreateMarker(envelope, "PROXY_FlatFloorDatum_0m", Vector3.zero, Vector3.one);
            CreateMarker(envelope, "PROXY_CeilingReference_4.2m",
                new Vector3(0f, 4.2f, 0f), Vector3.one);
            CreateMarker(envelope, "PROXY_ExteriorBounds_X-32_TO_32_Z-22_TO_22",
                Vector3.zero, new Vector3(64f, 1f, 44f));
        }

        private static void BuildV2ZoneMarkers(Transform parent)
        {
            var zones = new Dictionary<string, Vector3>
            {
                { "Z0_ENTRY_STAGING", new Vector3(0f, 0f, -19f) },
                { "Z1_DIRECT_EQUIPMENT_BAY", new Vector3(0f, 0f, -15f) },
                { "Z2_CENTRAL_CLASH_BOWL", new Vector3(0f, 0f, -8f) },
                { "Z3_WEST_LOGISTICS_LOOP", new Vector3(-24f, 0f, 3f) },
                { "Z4_CENTRAL_CUBICLE_RING", new Vector3(0f, 0f, 5f) },
                { "Z5_EAST_PHYSICS_LANE", new Vector3(23f, 0f, 3f) },
                { "Z6_NORTH_TAG_ARENA_DRAWER_GATE", new Vector3(17f, 0f, 15f) },
                { "Z7_EXIT_RUN_NEXT_AREA_TEASE", new Vector3(26f, 0f, 19f) }
            };

            foreach (KeyValuePair<string, Vector3> zone in zones)
            {
                CreateMarker(parent, zone.Key, zone.Value, Vector3.one);
            }
        }

        private static void BuildV2FigureEight(Transform parent)
        {
            CreateMarker(parent, "SOUTH_BOWL_24x10", new Vector3(0f, 0f, -8f),
                new Vector3(24f, 1f, 10f));
            CreateMarker(parent, "WEST_LOOP_MIN_5.5m", new Vector3(-24f, 0f, 3f),
                new Vector3(8f, 1f, 22f));
            CreateMarker(parent, "EAST_LANE_9m", new Vector3(23f, 0f, 3f),
                new Vector3(9f, 1f, 22f));
            CreateMarker(parent, "NORTH_CROSS_MIN_6m", new Vector3(0f, 0f, 15f),
                new Vector3(48f, 1f, 6f));
            CreateMarker(parent, "CENTRAL_CUT_6m", new Vector3(0f, 0f, 6f),
                new Vector3(6f, 1f, 18f));
            CreateMarker(parent, "DRAWER_CONNECTOR_BLOCKED_REVIEW",
                new Vector3(18f, 0f, 15f), new Vector3(4f, 1f, 6f));
            CreateMarker(parent, "DRAWER_CONNECTOR_OPEN_REVIEW",
                new Vector3(22f, 0f, 15f), new Vector3(4f, 1f, 6f));
            CreateMarker(parent, "EXIT_RUN_MIN_4m", new Vector3(26f, 0f, 18.5f),
                new Vector3(4.5f, 1f, 7f));

            // Non-colliding route ribbons expose the two loops and shared connections.
            CreateCube(parent, "PROXY_ROUTE_West_South", new Vector3(-12f, 0.02f, -8f),
                new Vector3(24f, 0.04f, 6f), false);
            CreateCube(parent, "PROXY_ROUTE_West_Outer", new Vector3(-24f, 0.02f, 3f),
                new Vector3(8f, 0.04f, 22f), false);
            CreateCube(parent, "PROXY_ROUTE_West_North", new Vector3(-12f, 0.02f, 15f),
                new Vector3(24f, 0.04f, 6f), false);
            CreateCube(parent, "PROXY_ROUTE_East_South", new Vector3(12f, 0.02f, -8f),
                new Vector3(24f, 0.04f, 6f), false);
            CreateCube(parent, "PROXY_ROUTE_East_Outer", new Vector3(23f, 0.02f, 3f),
                new Vector3(9f, 0.04f, 22f), false);
            CreateCube(parent, "PROXY_ROUTE_East_North", new Vector3(12f, 0.02f, 15f),
                new Vector3(24f, 0.04f, 6f), false);
            CreateCube(parent, "PROXY_ROUTE_Central_Cut", new Vector3(0f, 0.02f, 6f),
                new Vector3(6f, 0.04f, 18f), false);
            CreateCube(parent, "PROXY_ROUTE_Drawer_Connector", new Vector3(20f, 0.02f, 15f),
                new Vector3(8f, 0.04f, 6f), false);
            CreateCube(parent, "PROXY_ROUTE_Exit_Run", new Vector3(26f, 0.02f, 18.5f),
                new Vector3(4.5f, 0.04f, 7f), false);
        }

        private static void BuildV2CubicleIsland(Transform parent)
        {
            Transform collision = CreateGroup(parent, "STATIC_CUBICLE_COLLISION");
            Transform partitions = CreateGroup(collision, "PROXY_LowPartitions_16");
            Transform desks = CreateGroup(collision, "PROXY_FixedDesks_6");
            CreateMarker(parent, "PROXY_IslandBounds_20x12", new Vector3(0f, 0f, 5f),
                new Vector3(20f, 1f, 12f));
            CreateMarker(parent, "PROXY_CentralCutClear_6m", new Vector3(0f, 0f, 5f),
                new Vector3(6f, 1f, 12f));

            foreach (float z in new[] { -1f, 11f })
            {
                CreateCube(partitions, $"PROXY_Partition_{z:0}_West",
                    new Vector3(-7.5f, 1.05f, z), new Vector3(5f, 2.1f, 0.2f), true);
                CreateCube(partitions, $"PROXY_Partition_{z:0}_East",
                    new Vector3(7.5f, 1.05f, z), new Vector3(5f, 2.1f, 0.2f), true);
            }

            foreach (float z in new[] { 1.5f, 8.5f })
            {
                CreateCube(partitions, $"PROXY_Partition_West_{z:0.0}",
                    new Vector3(-10f, 1.05f, z), new Vector3(0.2f, 2.1f, 5f), true);
                CreateCube(partitions, $"PROXY_Partition_East_{z:0.0}",
                    new Vector3(10f, 1.05f, z), new Vector3(0.2f, 2.1f, 5f), true);
            }

            foreach (float z in new[] { 3f, 7f })
            {
                CreateCube(partitions, $"PROXY_Partition_Internal_West_{z:0}",
                    new Vector3(-6.5f, 1.05f, z), new Vector3(6f, 2.1f, 0.2f), true);
                CreateCube(partitions, $"PROXY_Partition_Internal_East_{z:0}",
                    new Vector3(6.5f, 1.05f, z), new Vector3(6f, 2.1f, 0.2f), true);
            }

            foreach (float x in new[] { -3.5f, 3.5f })
            {
                CreateCube(partitions, $"PROXY_Partition_Internal_{x:0.0}_South",
                    new Vector3(x, 1.05f, 1f), new Vector3(0.2f, 2.1f, 4f), true);
                CreateCube(partitions, $"PROXY_Partition_Internal_{x:0.0}_North",
                    new Vector3(x, 1.05f, 9f), new Vector3(0.2f, 2.1f, 4f), true);
            }

            foreach (float x in new[] { -6.5f, 6.5f })
            {
                foreach (float z in new[] { 1f, 5f, 9f })
                {
                    CreateCube(desks, $"PROXY_FixedDesk_{x:0.0}_{z:0}",
                        new Vector3(x, 0.4f, z), new Vector3(2.8f, 0.8f, 1.4f), true);
                }
            }
        }

        private static void BuildV2StaticFurniture(Transform parent)
        {
            Transform furniture = CreateGroup(parent, "STATIC_FIXED_FURNITURE");
            CreateCube(furniture, "PROXY_SupplyDesk_West", new Vector3(-28f, 0.5f, -3f),
                new Vector3(3f, 1f, 1.2f), true);
            CreateCube(furniture, "PROXY_PhysicsColumn", new Vector3(20f, 2.1f, 3f),
                new Vector3(0.7f, 4.2f, 0.7f), true);
            CreateCube(furniture, "PROXY_FixedDesk_East_Inner", new Vector3(14f, 0.5f, 5f),
                new Vector3(3f, 1f, 1.2f), true);
            CreateCube(furniture, "PROXY_FixedDesk_East_Outer", new Vector3(28f, 0.5f, 7f),
                new Vector3(3f, 1f, 1.2f), true);
            CreateCube(furniture, "PROXY_CrashDesk_North", new Vector3(-5f, 0.5f, 19f),
                new Vector3(3f, 1f, 1.2f), true);
        }

        private static void BuildV2VisualReadability(Transform parent)
        {
            Transform boundary = CreateGroup(parent, "BOUNDARY_AND_OPENING_READABILITY");
            Transform zoneCues = CreateGroup(parent, "ZONE_VISUAL_CUES_8");
            Transform wayfinding = CreateGroup(parent, "DIRECTIONAL_WAYFINDING");
            Transform density = CreateGroup(parent, "OFFICE_DENSITY_NON_COLLIDING");

            BuildV2BoundaryReadability(boundary);
            BuildV2ZoneVisualCues(zoneCues);
            BuildV2Wayfinding(wayfinding);
            BuildV2OfficeDensity(density);
        }

        private static void BuildV2BoundaryReadability(Transform parent)
        {
            Color boundaryColor = new Color(0.20f, 0.82f, 1f, 1f);
            Color exitColor = new Color(0.48f, 1f, 0.40f, 1f);

            CreateReviewLine(
                parent,
                "PERIMETER_CEILING_REFERENCE_4.2m",
                new[]
                {
                    new Vector3(-32f, 4.18f, -22f),
                    new Vector3(32f, 4.18f, -22f),
                    new Vector3(32f, 4.18f, 22f),
                    new Vector3(-32f, 4.18f, 22f)
                },
                boundaryColor,
                0.16f,
                true);

            foreach (Vector3 corner in new[]
                     {
                         new Vector3(-32f, 0f, -22f),
                         new Vector3(32f, 0f, -22f),
                         new Vector3(32f, 0f, 22f),
                         new Vector3(-32f, 0f, 22f)
                     })
            {
                CreateReviewLine(
                    parent,
                    $"PERIMETER_CORNER_{corner.x:0}_{corner.z:0}",
                    new[] { corner, corner + Vector3.up * 4.2f },
                    boundaryColor,
                    0.12f,
                    false);
            }

            CreateReviewLine(
                parent,
                "AUTHORIZED_EXIT_OPENING_4.5m_OUTLINE",
                new[]
                {
                    new Vector3(23.75f, 0.05f, 21.96f),
                    new Vector3(23.75f, 3.55f, 21.96f),
                    new Vector3(28.25f, 3.55f, 21.96f),
                    new Vector3(28.25f, 0.05f, 21.96f)
                },
                exitColor,
                0.22f,
                false);

            CreateReviewLabel(
                parent,
                "BOUNDARY_HEADER_SOUTH_ENTRY",
                "ENTRY STAGING / INTERIOR START",
                new Vector3(0f, 3.15f, -21.9f),
                Quaternion.Euler(0f, 180f, 0f),
                boundaryColor,
                0.055f);
            CreateReviewLabel(
                parent,
                "BOUNDARY_HEADER_NORTH_EXIT",
                "AUTHORIZED EXIT OPENING 4.5m",
                new Vector3(26f, 3.75f, 21.9f),
                Quaternion.identity,
                exitColor,
                0.052f);

            Transform ceiling = CreateGroup(parent, "CEILING_GRID_SILHOUETTES_16");
            int ceilingIndex = 0;
            foreach (float z in new[] { -18f, -12f, -6f, 0f, 6f, 12f, 18f })
            {
                CreateCube(ceiling, $"CEILING_REFERENCE_BEAM_X_{ceilingIndex:00}",
                    new Vector3(0f, 4.12f, z), new Vector3(62f, 0.06f, 0.08f), false);
                ceilingIndex++;
            }
            foreach (float x in new[] { -28f, -21f, -14f, -7f, 0f, 7f, 14f, 21f, 28f })
            {
                CreateCube(ceiling, $"CEILING_REFERENCE_BEAM_Z_{ceilingIndex:00}",
                    new Vector3(x, 4.12f, 0f), new Vector3(0.08f, 0.06f, 42f), false);
                ceilingIndex++;
            }
        }

        private static void BuildV2ZoneVisualCues(Transform parent)
        {
            CreateZoneVisualCue(parent, 0, "ENTRY STAGING", new Vector3(0f, 0f, -19f),
                new Vector2(14f, 4f), new Vector3(0f, 2.45f, -21.82f),
                Quaternion.Euler(0f, 180f, 0f));
            CreateZoneVisualCue(parent, 1, "DIRECT EQUIPMENT BAY", new Vector3(0f, 0f, -15f),
                new Vector2(14f, 4f), new Vector3(-7.5f, 2.1f, -14.8f),
                Quaternion.Euler(0f, -90f, 0f));
            CreateZoneVisualCue(parent, 2, "CENTRAL CLASH BOWL", new Vector3(0f, 0f, -8f),
                new Vector2(24f, 10f), new Vector3(-11.8f, 2.2f, -3.1f),
                Quaternion.identity);
            CreateZoneVisualCue(parent, 3, "WEST LOGISTICS LOOP", new Vector3(-24f, 0f, 3f),
                new Vector2(8f, 22f), new Vector3(-31.82f, 2.65f, 3f),
                Quaternion.Euler(0f, -90f, 0f));
            CreateZoneVisualCue(parent, 4, "CENTRAL CUBICLE RING", new Vector3(0f, 0f, 5f),
                new Vector2(20f, 12f), new Vector3(0f, 1.62f, 10.85f),
                Quaternion.identity);
            CreateZoneVisualCue(parent, 5, "EAST PHYSICS LANE", new Vector3(23f, 0f, 3f),
                new Vector2(9f, 22f), new Vector3(31.82f, 2.65f, 3f),
                Quaternion.Euler(0f, 90f, 0f));
            CreateZoneVisualCue(parent, 6, "NORTH TAG / DRAWER", new Vector3(17f, 0f, 15f),
                new Vector2(18f, 6f), new Vector3(15f, 2.65f, 21.82f),
                Quaternion.identity);
            CreateZoneVisualCue(parent, 7, "EXIT RUN", new Vector3(26f, 0f, 19f),
                new Vector2(4.5f, 6f), new Vector3(26f, 3.25f, 21.78f),
                Quaternion.identity);
        }

        private static void CreateZoneVisualCue(
            Transform parent,
            int zoneIndex,
            string displayName,
            Vector3 center,
            Vector2 size,
            Vector3 headerPosition,
            Quaternion headerRotation)
        {
            Color color = V2ZoneColors[zoneIndex];
            Transform cue = CreateGroup(
                parent,
                $"ZONE_CUE_Z{zoneIndex}_{displayName.Replace(' ', '_').Replace('/', '_')}");
            float halfX = size.x * 0.5f;
            float halfZ = size.y * 0.5f;
            CreateReviewLine(
                cue,
                $"FLOOR_BAND_Z{zoneIndex}",
                new[]
                {
                    center + new Vector3(-halfX, 0.055f, -halfZ),
                    center + new Vector3(halfX, 0.055f, -halfZ),
                    center + new Vector3(halfX, 0.055f, halfZ),
                    center + new Vector3(-halfX, 0.055f, halfZ)
                },
                color,
                0.14f,
                true);
            CreateReviewLabel(
                cue,
                $"FLOOR_HEADER_Z{zoneIndex}",
                $"Z{zoneIndex}  {displayName}",
                center + Vector3.up * 0.075f,
                Quaternion.Euler(90f, 0f, 0f),
                color,
                0.052f);
            CreateReviewLabel(
                cue,
                $"VERTICAL_HEADER_Z{zoneIndex}",
                $"Z{zoneIndex}  {displayName}",
                headerPosition,
                headerRotation,
                color,
                0.064f);
        }

        private static void BuildV2Wayfinding(Transform parent)
        {
            Color routeColor = new Color(1f, 0.90f, 0.28f, 1f);
            CreateReviewLabel(parent, "WAYFINDING_WEST", "WEST LOOP  <",
                new Vector3(-13f, 0.08f, -9.5f), Quaternion.Euler(90f, 0f, 0f),
                routeColor, 0.075f);
            CreateReviewLabel(parent, "WAYFINDING_CENTRAL", "CENTRAL CUT  ^",
                new Vector3(0f, 0.08f, 0f), Quaternion.Euler(90f, 0f, 0f),
                routeColor, 0.075f);
            CreateReviewLabel(parent, "WAYFINDING_EAST", ">  EAST PHYSICS",
                new Vector3(13f, 0.08f, -9.5f), Quaternion.Euler(90f, 0f, 0f),
                routeColor, 0.075f);
            CreateReviewLabel(parent, "WAYFINDING_NORTH", "NORTH CROSS  /  DRAWER",
                new Vector3(0f, 0.08f, 16.8f), Quaternion.Euler(90f, 0f, 0f),
                routeColor, 0.065f);
            CreateReviewLabel(parent, "WAYFINDING_EXIT", "EXIT  >>>",
                new Vector3(25.8f, 0.08f, 19.5f), Quaternion.Euler(90f, 0f, 0f),
                V2ZoneColors[7], 0.085f);
        }

        private static void BuildV2OfficeDensity(Transform parent)
        {
            Transform workstations = CreateGroup(parent, "WORKSTATION_SILHOUETTES_12");
            foreach (float x in new[] { -6.5f, 6.5f })
            {
                foreach (float z in new[] { 1f, 5f, 9f })
                {
                    CreateCube(workstations, $"DENSITY_PROXY_MONITOR_{x:0.0}_{z:0}",
                        new Vector3(x, 1.18f, z), new Vector3(0.9f, 0.58f, 0.16f), false);
                    CreateCube(workstations, $"DENSITY_PROXY_TOWER_{x:0.0}_{z:0}",
                        new Vector3(x + Mathf.Sign(x) * 0.9f, 0.43f, z),
                        new Vector3(0.34f, 0.7f, 0.55f), false);
                }
            }

            Transform archive = CreateGroup(parent, "ARCHIVE_STORAGE_SILHOUETTES_10");
            int archiveIndex = 0;
            foreach (float z in new[] { -9f, -4f, 2f, 8f, 14f })
            {
                CreateCube(archive, $"DENSITY_PROXY_ARCHIVE_CABINET_{archiveIndex:00}",
                    new Vector3(-30.4f, 1.05f, z), new Vector3(1.4f, 2.1f, 1.15f), false);
                CreateCube(archive, $"DENSITY_PROXY_ARCHIVE_BOX_{archiveIndex:00}",
                    new Vector3(-29.8f, 0.32f, z + 1.35f),
                    new Vector3(0.75f, 0.64f, 0.65f), false);
                archiveIndex++;
            }

            Transform equipment = CreateGroup(parent, "EQUIPMENT_SILHOUETTES_8");
            int equipmentIndex = 0;
            foreach (float z in new[] { -10f, -5f, 0f, 6f, 12f, 18f })
            {
                CreateCube(equipment, $"DENSITY_PROXY_EQUIPMENT_{equipmentIndex:00}",
                    new Vector3(30.4f, 0.72f, z), new Vector3(1.35f, 1.44f, 0.9f), false);
                equipmentIndex++;
            }
            CreateCube(equipment, "DENSITY_PROXY_RECEIVING_BIN_WEST",
                new Vector3(-27.5f, 0.6f, -20.4f), new Vector3(2.2f, 1.2f, 1.1f), false);
            CreateCube(equipment, "DENSITY_PROXY_RECEIVING_BIN_EAST",
                new Vector3(27.5f, 0.6f, -20.4f), new Vector3(2.2f, 1.2f, 1.1f), false);

            Transform dressing = CreateGroup(parent, "PLANT_BIN_DRESSING_SILHOUETTES_8");
            Vector3[] dressingPositions =
            {
                new Vector3(-29.5f, 0.75f, 19.5f),
                new Vector3(29.5f, 0.75f, 19.5f),
                new Vector3(-29.5f, 0.75f, -18.5f),
                new Vector3(29.5f, 0.75f, -18.5f),
                new Vector3(-11.2f, 0.35f, 11f),
                new Vector3(11.2f, 0.35f, 11f),
                new Vector3(-11.2f, 0.35f, -1f),
                new Vector3(11.2f, 0.35f, -1f)
            };
            for (int index = 0; index < dressingPositions.Length; index++)
            {
                bool plant = index < 4;
                CreateCube(
                    dressing,
                    plant
                        ? $"DENSITY_PROXY_PLANT_{index:00}"
                        : $"DENSITY_PROXY_WASTE_BIN_{index - 4:00}",
                    dressingPositions[index],
                    plant ? new Vector3(0.75f, 1.5f, 0.75f) : new Vector3(0.55f, 0.7f, 0.55f),
                    false);
            }

            Transform perimeterWorkstations = CreateGroup(
                parent,
                "PERIMETER_WORKSTATION_SILHOUETTES_32");
            int workstationIndex = 0;
            foreach (float x in new[] { -28f, -24f, -20f, -16f, -12f, -8f, -4f, 4f })
            {
                CreateCube(
                    perimeterWorkstations,
                    $"DENSITY_PROXY_PERIMETER_DESK_{workstationIndex:00}",
                    new Vector3(x, 0.4f, 20.1f),
                    new Vector3(2.6f, 0.8f, 1.1f),
                    false);
                CreateCube(
                    perimeterWorkstations,
                    $"DENSITY_PROXY_PERIMETER_PARTITION_{workstationIndex:00}",
                    new Vector3(x, 1.05f, 20.72f),
                    new Vector3(2.8f, 2.1f, 0.12f),
                    false);
                workstationIndex++;
            }
            foreach (float x in new[] { -22f, -16f, -10f, 10f, 16f, 22f })
            {
                CreateCube(
                    perimeterWorkstations,
                    $"DENSITY_PROXY_PERIMETER_DESK_{workstationIndex:00}",
                    new Vector3(x, 0.4f, -20.1f),
                    new Vector3(2.6f, 0.8f, 1.1f),
                    false);
                CreateCube(
                    perimeterWorkstations,
                    $"DENSITY_PROXY_PERIMETER_PARTITION_{workstationIndex:00}",
                    new Vector3(x, 1.05f, -20.72f),
                    new Vector3(2.8f, 2.1f, 0.12f),
                    false);
                workstationIndex++;
            }
            foreach (float x in new[] { -30.4f, 30.4f })
            {
                GameObject desk = CreateCube(
                    perimeterWorkstations,
                    $"DENSITY_PROXY_PERIMETER_DESK_{workstationIndex:00}",
                    new Vector3(x, 0.4f, -15f),
                    new Vector3(2.6f, 0.8f, 1.1f),
                    false);
                GameObject partition = CreateCube(
                    perimeterWorkstations,
                    $"DENSITY_PROXY_PERIMETER_PARTITION_{workstationIndex:00}",
                    new Vector3(x + Mathf.Sign(x) * 0.62f, 1.05f, -15f),
                    new Vector3(2.8f, 2.1f, 0.12f),
                    false);
                desk.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                partition.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                workstationIndex++;
            }
        }

        private static void BuildV2LayoutMarkers(Transform parent)
        {
            CreateMarker(parent, "APPROACH_01_WEST_NORTH", new Vector3(-12f, 0f, 15f),
                new Vector3(8f, 1f, 6f));
            CreateMarker(parent, "APPROACH_02_EAST_NORTH", new Vector3(12f, 0f, 15f),
                new Vector3(8f, 1f, 6f));
            CreateMarker(parent, "APPROACH_03_CENTRAL_CUT", new Vector3(0f, 0f, 10f),
                new Vector3(6f, 1f, 8f));
            CreateMarker(parent, "TURN_POCKET_WEST_8x8", new Vector3(-24f, 0f, 3f),
                new Vector3(8f, 1f, 8f));
            CreateMarker(parent, "TURN_POCKET_EAST_9x9", new Vector3(24f, 0f, 3f),
                new Vector3(9f, 1f, 9f));
            CreateMarker(parent, "TURN_POCKET_Z6_9x9", new Vector3(17f, 0f, 15f),
                new Vector3(9f, 1f, 9f));
            CreateMarker(parent, "PARKING_BAY_REVIEW", new Vector3(22f, 0f, 15f),
                new Vector3(4f, 1f, 4f));
            CreateMarker(parent, "FINISHER_POCKET_REVIEW_ONLY", new Vector3(14f, 0f, 18f),
                new Vector3(8f, 1f, 7f));
            CreateMarker(parent, "EXIT_TARGET", new Vector3(26f, 0f, 21f),
                new Vector3(4.5f, 1f, 1f));
            CreateMarker(parent, "DIRECT_ROUTE_AROUND_COLUMN", new Vector3(15f, 0f, 3f),
                new Vector3(7f, 1f, 8f));
            CreateMarker(parent, "REDIRECT_ROUTE_AROUND_COLUMN", new Vector3(25f, 0f, 3f),
                new Vector3(7f, 1f, 8f));
        }

        private static void RepositionV2Gameplay(Scene scene, Transform gameplayIndex)
        {
            GameObject player = RequireRootAny(scene, "Player", "GAMEPLAY_CHR00_PLAYER");
            GameObject ally = RequireRootAny(scene, "AllyProxy", "GAMEPLAY_CHR01_MOBILE_ALLY");
            GameObject runnerOne = RequireRootAny(
                scene,
                "EnemyProxy_01",
                "GAMEPLAY_ENY01_RUNNER_01");
            GameObject runnerTwo = RequireRootAny(
                scene,
                "EnemyProxy_02",
                "GAMEPLAY_ENY01_RUNNER_02");

            SetActorReviewTransform(player, "GAMEPLAY_CHR00_PLAYER", new Vector2(0f, -17f));
            SetActorReviewTransform(ally, "GAMEPLAY_CHR01_MOBILE_ALLY", new Vector2(-2.5f, -17.2f));
            SetActorReviewTransform(runnerOne, "GAMEPLAY_ENY01_RUNNER_01", new Vector2(6f, -3.5f));
            SetActorReviewTransform(runnerTwo, "GAMEPLAY_ENY01_RUNNER_02", new Vector2(12f, 19f));

            CreateMarker(gameplayIndex, "GAMEPLAY_INDEX_CHR00_PLAYER",
                player.transform.position, Vector3.one);
            CreateMarker(gameplayIndex, "GAMEPLAY_INDEX_CHR01_MOBILE_ALLY",
                ally.transform.position, Vector3.one);
            CreateMarker(gameplayIndex, "GAMEPLAY_INDEX_ENY01_RUNNER_01",
                runnerOne.transform.position, Vector3.one);
            CreateMarker(gameplayIndex, "GAMEPLAY_INDEX_ENY01_RUNNER_02",
                runnerTwo.transform.position, Vector3.one);

            var specs = new Dictionary<string, GameplayReviewSpec>(StringComparer.Ordinal)
            {
                {
                    "BINDER-DIRECT-01",
                    new GameplayReviewSpec(
                        "GAMEPLAY_BINDER_DIRECT_01",
                        new Vector2(-5f, -15f),
                        new Vector3(0.29f, 0.32f, 0.08f))
                },
                {
                    "BINDER-ALLY-01",
                    new GameplayReviewSpec(
                        "GAMEPLAY_BINDER_ALLY_01",
                        new Vector2(-28f, -5f),
                        new Vector3(0.29f, 0.32f, 0.08f))
                },
                {
                    "BINDER-ALLY-02",
                    new GameplayReviewSpec(
                        "GAMEPLAY_BINDER_ALLY_02",
                        new Vector2(-12f, 18.5f),
                        new Vector3(0.29f, 0.32f, 0.08f))
                },
                {
                    "CHAIR-DIRECT-01",
                    new GameplayReviewSpec(
                        "GAMEPLAY_CHAIR_DIRECT_01",
                        new Vector2(5f, -15f),
                        new Vector3(0.729f, 0.998f, 0.697f))
                },
                {
                    "CHAIR-ALLY-01",
                    new GameplayReviewSpec(
                        "GAMEPLAY_CHAIR_ALLY_01",
                        new Vector2(-27f, 5f),
                        new Vector3(0.729f, 0.998f, 0.697f))
                },
                {
                    "DRAWER-01",
                    new GameplayReviewSpec(
                        "GAMEPLAY_DRAWER_GATE_01",
                        new Vector2(18f, 15f),
                        new Vector3(0.760f, 0.998f, 0.744f))
                },
                {
                    "COPIER-01",
                    new GameplayReviewSpec(
                        "GAMEPLAY_COPIER_RECOGNITION_01",
                        new Vector2(-29f, -14f),
                        new Vector3(0.614f, 0.979f, 0.655f))
                }
            };

            foreach (PrototypeInteractionTarget target in GetInteractionTargets(scene))
            {
                if (!specs.TryGetValue(target.RuntimeInstanceKey, out GameplayReviewSpec spec))
                {
                    throw new InvalidOperationException(
                        $"Unexpected preserved gameplay target: {target.RuntimeInstanceKey}");
                }

                target.gameObject.name = spec.Name;
                target.transform.SetPositionAndRotation(
                    new Vector3(spec.Position.x, spec.Bounds.y * 0.5f, spec.Position.y),
                    Quaternion.identity);
                target.transform.localScale = spec.Bounds;
                EditorUtility.SetDirty(target.gameObject);
                EditorUtility.SetDirty(target.transform);

                CreateMarker(
                    gameplayIndex,
                    $"GAMEPLAY_INDEX_{spec.Name}__{target.RuntimeInstanceKey}",
                    target.transform.position,
                    spec.Bounds);
            }
        }

        private static void ConfigureV2MobileAlly(Scene scene, Transform parent)
        {
            ShowcaseMobileAllyController mobile =
                RequireSingleSceneComponent<ShowcaseMobileAllyController>(scene);
            PrototypeInteractionStateOwner owner =
                RequireSingleSceneComponent<PrototypeInteractionStateOwner>(scene);
            AuthoredEncounterResetController reset =
                RequireSingleSceneComponent<AuthoredEncounterResetController>(scene);
            PrototypeInteractionController interaction =
                owner.GetComponent<PrototypeInteractionController>();
            ShowcaseMobileAllyPresenter presenter =
                RequireSingleSceneComponent<ShowcaseMobileAllyPresenter>(scene);
            CharacterController controller = mobile.GetComponent<CharacterController>();

            if (interaction == null || controller == null)
            {
                throw new InvalidOperationException(
                    "The preserved S2 player interaction and ally CharacterController are required.");
            }

            Transform anchorsParent = CreateGroup(parent, "S2_FOLLOW_ANCHORS_18");
            Vector2[] positions =
            {
                new Vector2(-2.5f, -17.2f),
                new Vector2(-4f, -14f),
                new Vector2(-6f, -8f),
                new Vector2(0f, -8f),
                new Vector2(-16f, -8f),
                new Vector2(-24f, -8f),
                new Vector2(-25f, 3f),
                new Vector2(-24f, 14f),
                new Vector2(-12f, 15f),
                new Vector2(0f, 15f),
                new Vector2(0f, 8f),
                new Vector2(0f, 0f),
                new Vector2(14f, -8f),
                new Vector2(24f, -8f),
                new Vector2(25f, 3f),
                new Vector2(24f, 14f),
                new Vector2(16f, 17f),
                new Vector2(26f, 19f)
            };
            var anchors = new Transform[positions.Length];
            for (int index = 0; index < positions.Length; index++)
            {
                anchors[index] = CreateMarker(
                    anchorsParent,
                    $"V2-FA-{index:00}",
                    new Vector3(positions[index].x, 1f, positions[index].y),
                    Vector3.one);
            }

            Vector2Int[] links =
            {
                new Vector2Int(0, 1),
                new Vector2Int(1, 2),
                new Vector2Int(2, 3),
                new Vector2Int(3, 4),
                new Vector2Int(4, 5),
                new Vector2Int(5, 6),
                new Vector2Int(6, 7),
                new Vector2Int(7, 8),
                new Vector2Int(8, 9),
                new Vector2Int(3, 11),
                new Vector2Int(11, 10),
                new Vector2Int(10, 9),
                new Vector2Int(3, 12),
                new Vector2Int(12, 13),
                new Vector2Int(13, 14),
                new Vector2Int(14, 15),
                new Vector2Int(15, 16),
                new Vector2Int(16, 9),
                new Vector2Int(16, 17),
                new Vector2Int(8, 16)
            };

            mobile.Configure(
                owner,
                reset,
                mobile.transform,
                owner.transform,
                controller,
                anchors,
                links);
            interaction.ConfigureShowcaseMobileAlly(mobile);
            presenter.Configure(mobile);
            EditorUtility.SetDirty(mobile);
            EditorUtility.SetDirty(interaction);
            EditorUtility.SetDirty(presenter);
        }

        private static void BuildV2FutureS3ReviewProxies(Transform parent)
        {
            CreateCube(parent, "PROXY_S3_NOT_IMPLEMENTED_BINDER_04",
                new Vector3(15f, 0.16f, -11f), new Vector3(0.29f, 0.32f, 0.08f), false);
            CreateCube(parent, "PROXY_S3_NOT_IMPLEMENTED_BINDER_05",
                new Vector3(12f, 0.16f, 17.5f), new Vector3(0.29f, 0.32f, 0.08f), false);
            CreateCube(parent, "PROXY_S3_NOT_IMPLEMENTED_CHAIR_03",
                new Vector3(17f, 0.499f, -12f), new Vector3(0.729f, 0.998f, 0.697f), false);
        }

        private static void BuildV2AssetSockets(Transform parent)
        {
            Transform architecture = CreateGroup(parent, "ARCHITECTURE_19");
            Transform fixedDesk = CreateGroup(parent, "FIXED_DESK_3");
            Transform gameplay = CreateGroup(parent, "GAMEPLAY_RECOGNITION_4");
            Transform dressing = CreateGroup(parent, "DRESSING_22");
            Transform sharedMaterial = CreateGroup(parent, "SHARED_MATERIAL_9");
            Transform decalAtlas = CreateGroup(parent, "DECAL_ATLAS_3");

            SocketSpec[] architectureSpecs =
            {
                MetricSocket("ENV_Wall_2m_v001", new Vector3(-31.9f, 0f, -16f), new Vector3(2f, 4.2f, 0.2f), "GROUND_CENTER"),
                MetricSocket("ENV_Wall_4m_v001", new Vector3(-31.9f, 0f, -10f), new Vector3(4f, 4.2f, 0.2f), "GROUND_CENTER"),
                MetricSocket("ENV_Wall_6m_v001", new Vector3(-31.9f, 0f, -3f), new Vector3(6f, 4.2f, 0.2f), "GROUND_CENTER"),
                MetricSocket("ENV_Wall_InnerCorner_v001", new Vector3(-31.9f, 0f, -21.9f), new Vector3(0.4f, 4.2f, 0.4f), "GROUND_INNER_CORNER"),
                MetricSocket("ENV_Wall_OuterCorner_v001", new Vector3(31.9f, 0f, -21.9f), new Vector3(0.4f, 4.2f, 0.4f), "GROUND_OUTER_CORNER"),
                MetricSocket("ENV_Wall_DoorModule_v001", new Vector3(26f, 0f, 21.9f), new Vector3(4.5f, 4.2f, 0.3f), "GROUND_CENTER"),
                MetricSocket("ENV_LowPartition_1m_v001", new Vector3(-3.5f, 0f, 1f), new Vector3(0.2f, 2.1f, 1f), "GROUND_CENTER"),
                MetricSocket("ENV_LowPartition_2m_v001", new Vector3(-3.5f, 0f, 5f), new Vector3(0.2f, 2.1f, 2f), "GROUND_CENTER"),
                MetricSocket("ENV_LowPartition_3m_v001", new Vector3(-6.5f, 0f, 3f), new Vector3(3f, 2.1f, 0.2f), "GROUND_CENTER"),
                MetricSocket("ENV_LowPartition_Corner_v001", new Vector3(-10f, 0f, -1f), new Vector3(0.4f, 2.1f, 0.4f), "GROUND_CORNER"),
                MetricSocket("ENV_LowPartition_TJunction_v001", new Vector3(-3.5f, 0f, 3f), new Vector3(0.6f, 2.1f, 0.6f), "GROUND_CENTER"),
                MetricSocket("ENV_LowPartition_EndCap_v001", new Vector3(3.5f, 0f, 11f), new Vector3(0.3f, 2.1f, 0.3f), "GROUND_CENTER"),
                MetricSocket("ENV_Floor_4m_v001", new Vector3(-28f, 0f, -18f), new Vector3(4f, 0.2f, 4f), "TOP_CENTER"),
                MetricSocket("ENV_Ceiling_4m_v001", new Vector3(-28f, 4.2f, -18f), new Vector3(4f, 0.2f, 4f), "BOTTOM_CENTER"),
                MetricSocket("ENV_Column_070_v001", new Vector3(20f, 0f, 3f), new Vector3(0.7f, 4.2f, 0.7f), "GROUND_CENTER"),
                MetricSocket("ENV_ExitDoor_Frame_v001", new Vector3(26f, 0f, 21.9f), new Vector3(4.5f, 4.2f, 0.3f), "GROUND_CENTER"),
                MetricSocket("ENV_SecurityShutter_VistaOnly_v001", new Vector3(26f, 0f, 22.1f), new Vector3(4.5f, 3f, 0.2f), "GROUND_CENTER"),
                MetricSocket("ENV_FluorescentFixture_A_v001", new Vector3(-8f, 4.1f, 5f), new Vector3(1.2f, 0.1f, 0.3f), "CEILING_CENTER"),
                MetricSocket("ENV_FluorescentFixture_B_v001", new Vector3(8f, 4.1f, 5f), new Vector3(1.2f, 0.1f, 0.3f), "CEILING_CENTER")
            };

            SocketSpec[] fixedDeskSpecs =
            {
                MetricSocket("PRP_FixedDesk_Single_v001", new Vector3(-6.5f, 0f, 1f), new Vector3(1.4f, 0.75f, 0.7f), "GROUND_CENTER"),
                MetricSocket("PRP_FixedDesk_Double_v001", new Vector3(6.5f, 0f, 5f), new Vector3(2.8f, 0.75f, 0.7f), "GROUND_CENTER"),
                MetricSocket("PRP_FixedDesk_LShape_v001", new Vector3(-6.5f, 0f, 9f), new Vector3(2.1f, 0.75f, 1.6f), "GROUND_INNER_CORNER")
            };

            SocketSpec[] gameplaySpecs =
            {
                MetricSocket("PRP_OfficeChair_Gameplay_v002", new Vector3(5f, 0f, -15f), new Vector3(0.729f, 0.998f, 0.697f), "GROUND_CENTER"),
                MetricSocket("PRP_RollingDrawer_Gameplay_v002", new Vector3(18f, 0f, 15f), new Vector3(0.760f, 0.998f, 0.744f), "GROUND_CENTER"),
                MetricSocket("PRP_Copier_Recognition_v002", new Vector3(-29f, 0f, -14f), new Vector3(0.614f, 0.979f, 0.655f), "GROUND_CENTER"),
                MetricSocket("PRP_FileBinder_Gameplay_v002", new Vector3(-5f, 0f, -15f), new Vector3(0.29f, 0.32f, 0.08f), "GROUND_CENTER")
            };

            SocketSpec[] dressingSpecs =
            {
                MetricSocket("DEC_MonitorCluster_A_v001", new Vector3(-6.5f, 0.8f, 1f), new Vector3(0.9f, 0.55f, 0.35f), "DESK_CENTER"),
                MetricSocket("DEC_MonitorCluster_B_v001", new Vector3(6.5f, 0.8f, 1f), new Vector3(1.1f, 0.55f, 0.35f), "DESK_CENTER"),
                MetricSocket("DEC_MonitorCluster_C_v001", new Vector3(-6.5f, 0.8f, 5f), new Vector3(1.2f, 0.6f, 0.4f), "DESK_CENTER"),
                MetricSocket("DEC_KeyboardPhoneCluster_v001", new Vector3(6.5f, 0.8f, 5f), new Vector3(0.8f, 0.18f, 0.45f), "DESK_CENTER"),
                MetricSocket("DEC_PaperStack_A_v001", new Vector3(-6f, 0.8f, 9f), new Vector3(0.32f, 0.16f, 0.24f), "DESK_CENTER"),
                MetricSocket("DEC_PaperStack_B_v001", new Vector3(6f, 0.8f, 9f), new Vector3(0.45f, 0.2f, 0.3f), "DESK_CENTER"),
                MetricSocket("DEC_ArchiveBox_A_v001", new Vector3(-30f, 0f, 12f), new Vector3(0.45f, 0.35f, 0.35f), "GROUND_CENTER"),
                MetricSocket("DEC_ArchiveBox_B_v001", new Vector3(-29f, 0f, 13f), new Vector3(0.55f, 0.4f, 0.4f), "GROUND_CENTER"),
                MetricSocket("DEC_Plant_A_v001", new Vector3(-29f, 0f, 19f), new Vector3(0.7f, 1.5f, 0.7f), "GROUND_CENTER"),
                MetricSocket("DEC_Plant_B_v001", new Vector3(29f, 0f, 18f), new Vector3(0.9f, 1.8f, 0.9f), "GROUND_CENTER"),
                MetricSocket("DEC_WasteBin_A_v001", new Vector3(-9f, 0f, 2f), new Vector3(0.4f, 0.55f, 0.4f), "GROUND_CENTER"),
                MetricSocket("DEC_WasteBin_B_v001", new Vector3(9f, 0f, 8f), new Vector3(0.45f, 0.6f, 0.45f), "GROUND_CENTER"),
                MetricSocket("DEC_CableBundle_VisualOnly_v001", new Vector3(-6.5f, 0.02f, 3f), new Vector3(1.8f, 0.04f, 0.35f), "FLOOR_CENTER"),
                MetricSocket("DEC_FloorTape_Set_v001", new Vector3(0f, 0.02f, -13f), new Vector3(8f, 0.02f, 1f), "FLOOR_CENTER"),
                MetricSocket("DEC_AssetTag_Set_v001", new Vector3(-28f, 1.2f, -5f), new Vector3(0.5f, 0.3f, 0.02f), "SURFACE_CENTER"),
                MetricSocket("DEC_KEEP_CLEAR_v001", new Vector3(18f, 0.02f, 15f), new Vector3(4f, 0.02f, 2f), "FLOOR_CENTER"),
                MetricSocket("DEC_EXIT_DIRECTION_v001", new Vector3(23f, 2.1f, 21.9f), new Vector3(1.2f, 0.4f, 0.02f), "WALL_CENTER"),
                MetricSocket("DEC_EQUIPMENT_TRANSFER_v001", new Vector3(-27f, 1.6f, -3f), new Vector3(1.5f, 0.5f, 0.02f), "WALL_CENTER"),
                MetricSocket("DEC_MOBILE_STORAGE_PARKING_v001", new Vector3(22f, 0.02f, 15f), new Vector3(4f, 0.02f, 3f), "FLOOR_CENTER"),
                MetricSocket("DEC_WheelMarks_v001", new Vector3(23f, 0.02f, 7f), new Vector3(6f, 0.02f, 2f), "FLOOR_CENTER"),
                MetricSocket("DEC_WallScuffs_v001", new Vector3(-31.9f, 1.2f, 5f), new Vector3(0.02f, 1.5f, 3f), "WALL_CENTER"),
                MetricSocket("DEC_ImpactMarks_v001", new Vector3(31.9f, 1.2f, 9f), new Vector3(0.02f, 1.8f, 3f), "WALL_CENTER")
            };

            SocketSpec[] materialSpecs =
            {
                CatalogSocket("MAT_PaintedGypsumWall_v001"),
                CatalogSocket("MAT_SuspendedCeilingTile_v001"),
                CatalogSocket("MAT_CommercialCarpet_v001"),
                CatalogSocket("MAT_WornVinylFloor_v001"),
                CatalogSocket("MAT_LaminateCompositeDesk_v001"),
                CatalogSocket("MAT_PaintedSteel_v001"),
                CatalogSocket("MAT_WornPlasticRubber_v001"),
                CatalogSocket("MAT_PaperCardboard_v001"),
                CatalogSocket("MAT_OfficeFabricUpholstery_v001")
            };

            SocketSpec[] decalSpecs =
            {
                CatalogSocket("DECAL_SignageAssetLabel_Atlas_v001"),
                CatalogSocket("DECAL_WearScuffWheelMark_Atlas_v001"),
                CatalogSocket("DECAL_DustTonerContactMark_Atlas_v001")
            };

            CreateMetricSockets(architecture, architectureSpecs, "SOCKET_",
                new Color(0.20f, 0.82f, 1f, 1f));
            CreateMetricSockets(fixedDesk, fixedDeskSpecs, "SOCKET_",
                new Color(0.78f, 0.50f, 1f, 1f));
            CreateMetricSockets(gameplay, gameplaySpecs, "SOCKET_",
                new Color(1f, 0.77f, 0.22f, 1f));
            CreateMetricSockets(dressing, dressingSpecs, "DRESSING_SOCKET_",
                new Color(0.48f, 1f, 0.40f, 1f));
            CreateMetricSockets(sharedMaterial, materialSpecs, "SOCKET_",
                new Color(0.74f, 0.78f, 0.84f, 1f));
            CreateMetricSockets(decalAtlas, decalSpecs, "SOCKET_",
                new Color(1f, 0.46f, 0.30f, 1f));
        }

        private static SocketSpec MetricSocket(
            string assetId,
            Vector3 position,
            Vector3 intendedBounds,
            string pivotReference)
        {
            return new SocketSpec(assetId, position, intendedBounds, pivotReference, true);
        }

        private static SocketSpec CatalogSocket(string assetId)
        {
            return new SocketSpec(
                assetId,
                Vector3.zero,
                Vector3.one,
                "NON_SPATIAL_CATALOG_ONLY",
                false);
        }

        private static void CreateMetricSockets(
            Transform parent,
            IEnumerable<SocketSpec> specs,
            string prefix,
            Color reviewColor)
        {
            int index = 0;
            foreach (SocketSpec spec in specs)
            {
                Vector3 socketPosition = spec.IsSpatial
                    ? spec.Position
                    : GetCatalogReviewPosition(parent.name, index);
                Transform socket = CreateMarker(
                    parent,
                    prefix + spec.AssetId,
                    socketPosition,
                    Vector3.one);
                if (!spec.IsSpatial)
                {
                    socket.rotation = Quaternion.Euler(0f, 180f, 0f);
                }
                Transform bounds = CreateGroup(
                    socket,
                    spec.IsSpatial
                        ? "INTENDED_BOUNDS_PROVISIONAL_REVIEW_REQUIRED"
                        : "INTENDED_BOUNDS_NON_SPATIAL_CATALOG_ONLY");
                bounds.localPosition = Vector3.zero;
                bounds.localScale = spec.IntendedBounds;
                Transform pivot = CreateGroup(socket, "PIVOT_" + spec.PivotReference);
                pivot.localPosition = Vector3.zero;
                pivot.localScale = Vector3.one;

                if (spec.IsSpatial)
                {
                    CreateVisibleSocketProxy(socket, spec, reviewColor);
                }
                else
                {
                    CreateCatalogReviewSwatch(socket, spec.AssetId, reviewColor);
                }
                index++;
            }
        }

        private static Vector3 GetCatalogReviewPosition(string categoryName, int index)
        {
            if (categoryName == "SHARED_MATERIAL_9")
            {
                return new Vector3(-14f + index * 3.45f, 3.6f, -21.84f);
            }

            return new Vector3(19f + index * 3.45f, 3.6f, -21.84f);
        }

        private static void CreateVisibleSocketProxy(
            Transform socket,
            SocketSpec spec,
            Color reviewColor)
        {
            Vector3 localCenter = Vector3.zero;
            if (spec.PivotReference.StartsWith("GROUND", StringComparison.Ordinal))
            {
                localCenter.y = spec.IntendedBounds.y * 0.5f;
            }
            else if (spec.PivotReference == "TOP_CENTER")
            {
                localCenter.y = -spec.IntendedBounds.y * 0.5f;
            }
            else if (spec.PivotReference == "BOTTOM_CENTER")
            {
                localCenter.y = spec.IntendedBounds.y * 0.5f;
            }

            GameObject proxy = CreateCube(
                socket,
                "VISIBLE_REVIEW_PROXY_" + spec.AssetId,
                socket.TransformPoint(localCenter),
                spec.IntendedBounds,
                false);
            proxy.transform.localRotation = Quaternion.identity;

            float halfX = spec.IntendedBounds.x * 0.5f;
            float halfZ = spec.IntendedBounds.z * 0.5f;
            float topY = localCenter.y + spec.IntendedBounds.y * 0.5f + 0.015f;
            CreateReviewLine(
                socket,
                "VISIBLE_REVIEW_OUTLINE_" + spec.AssetId,
                new[]
                {
                    new Vector3(-halfX, topY, -halfZ),
                    new Vector3(halfX, topY, -halfZ),
                    new Vector3(halfX, topY, halfZ),
                    new Vector3(-halfX, topY, halfZ)
                },
                reviewColor,
                0.055f,
                true,
                false);
        }

        private static void CreateCatalogReviewSwatch(
            Transform socket,
            string assetId,
            Color reviewColor)
        {
            CreateReviewLine(
                socket,
                "REVIEW_SWATCH_NON_RUNTIME_OUTLINE",
                new[]
                {
                    new Vector3(-1.45f, -0.3f, 0f),
                    new Vector3(1.45f, -0.3f, 0f),
                    new Vector3(1.45f, 0.3f, 0f),
                    new Vector3(-1.45f, 0.3f, 0f)
                },
                reviewColor,
                0.065f,
                true,
                false);
            CreateReviewLabel(
                socket,
                "REVIEW_SWATCH_NON_RUNTIME_LABEL",
                "REVIEW ONLY\n" + assetId,
                Vector3.zero,
                Quaternion.identity,
                reviewColor,
                0.026f,
                false);
        }

        private static void BuildEnvelope(Transform parent)
        {
            CreateCube(parent, "Floor_48x34", new Vector3(0f, -0.25f, 0f),
                new Vector3(48f, 0.5f, 34f), true);
            CreateCube(parent, "Wall_South", new Vector3(0f, 2.1f, -17.25f),
                new Vector3(48.5f, 4.2f, 0.5f), true);
            CreateCube(parent, "Wall_West", new Vector3(-24.25f, 2.1f, 0f),
                new Vector3(0.5f, 4.2f, 34.5f), true);
            CreateCube(parent, "Wall_East", new Vector3(24.25f, 2.1f, 0f),
                new Vector3(0.5f, 4.2f, 34.5f), true);

            // The north wall keeps the locked 2 m Exit opening at X 20.5.
            CreateCube(parent, "Wall_North_West", new Vector3(-2.25f, 2.1f, 17.25f),
                new Vector3(43.5f, 4.2f, 0.5f), true);
            CreateCube(parent, "Wall_North_East", new Vector3(22.75f, 2.1f, 17.25f),
                new Vector3(2.5f, 4.2f, 0.5f), true);
            CreateCube(parent, "Exit_Frame_Lintel", new Vector3(20.5f, 3.6f, 17.25f),
                new Vector3(2f, 1.2f, 0.5f), true);

            CreateMarker(parent, "FlatFloorDatum_0m", Vector3.zero, Vector3.one);
            CreateMarker(parent, "CeilingHeight_4.2m", new Vector3(0f, 4.2f, 0f), Vector3.one);
        }

        private static void BuildZoneMarkers(Transform parent)
        {
            var zones = new Dictionary<string, Vector3>
            {
                { "Z0_ENTRY_STAGING", new Vector3(0f, 0f, -15f) },
                { "Z1_DIRECT_EQUIPMENT_BAY", new Vector3(0f, 0f, -10.5f) },
                { "Z2_CENTRAL_CLASH_BOWL", new Vector3(0f, 0f, -5f) },
                { "Z3_WEST_LOGISTICS_LOOP", new Vector3(-17f, 0f, 1f) },
                { "Z4_CENTRAL_CUBICLE_RING", new Vector3(0f, 0f, 4f) },
                { "Z5_EAST_PHYSICS_LANE", new Vector3(17f, 0f, 0.5f) },
                { "Z6_NORTH_TAG_ARENA_DRAWER_GATE", new Vector3(14.5f, 0f, 11f) },
                { "Z7_EXIT_RUN_NEXT_AREA_TEASE", new Vector3(20f, 0f, 14.5f) }
            };

            foreach (KeyValuePair<string, Vector3> zone in zones)
            {
                CreateMarker(parent, zone.Key, zone.Value, Vector3.one);
            }
        }

        private static void BuildFigureEight(Transform parent)
        {
            CreateMarker(parent, "SOUTH_BOWL", new Vector3(0f, 0f, -5f),
                new Vector3(20f, 1f, 6f));
            CreateMarker(parent, "WEST_LOOP", new Vector3(-17f, 0f, 1f),
                new Vector3(14f, 1f, 20f));
            CreateMarker(parent, "EAST_LANE", new Vector3(17f, 0f, 1f),
                new Vector3(14f, 1f, 20f));
            CreateMarker(parent, "NORTH_CROSS", new Vector3(0f, 0f, 10.5f),
                new Vector3(32f, 1f, 5f));
            CreateMarker(parent, "CENTRAL_CUT", new Vector3(0f, 0f, 4f),
                new Vector3(6f, 1f, 12f));

            // Non-colliding floor ribbons expose both loops in Scene view without
            // adding gameplay obstacles or authoring future navigation behavior.
            CreateCube(parent, "Path_West_South", new Vector3(-13f, 0.02f, -5f),
                new Vector3(8f, 0.04f, 5f), false);
            CreateCube(parent, "Path_West_Long", new Vector3(-17f, 0.02f, 2.5f),
                new Vector3(5f, 0.04f, 15f), false);
            CreateCube(parent, "Path_West_North", new Vector3(-10f, 0.02f, 11f),
                new Vector3(14f, 0.04f, 5f), false);
            CreateCube(parent, "Path_East_South", new Vector3(13f, 0.02f, -5f),
                new Vector3(8f, 0.04f, 5f), false);
            CreateCube(parent, "Path_East_Long", new Vector3(17f, 0.02f, 2.5f),
                new Vector3(6f, 0.04f, 15f), false);
            CreateCube(parent, "Path_East_North", new Vector3(10f, 0.02f, 11f),
                new Vector3(14f, 0.04f, 5f), false);
        }

        private static void BuildCubicleIsland(Transform parent)
        {
            Transform partitions = CreateGroup(parent, "LowPartitions_16");
            Transform desks = CreateGroup(parent, "FixedDesks_6");
            CreateMarker(parent, "IslandBounds_16x12", new Vector3(0f, 0f, 4f),
                new Vector3(16f, 1f, 12f));

            float[] horizontalCenters = { -6.25f, -3.75f, 3.75f, 6.25f };
            float[] horizontalLengths = { 3.5f, 1.5f, 1.5f, 3.5f };
            for (int index = 0; index < horizontalCenters.Length; index++)
            {
                CreateCube(
                    partitions,
                    $"Partition_South_{index + 1:00}",
                    new Vector3(horizontalCenters[index], 0.6f, -2f),
                    new Vector3(horizontalLengths[index], 1.2f, 0.2f),
                    true);
                CreateCube(
                    partitions,
                    $"Partition_North_{index + 1:00}",
                    new Vector3(horizontalCenters[index], 0.6f, 10f),
                    new Vector3(horizontalLengths[index], 1.2f, 0.2f),
                    true);
            }

            float[] verticalCenters = { -0.75f, 2f, 6f, 8.75f };
            float[] verticalLengths = { 2.5f, 1.5f, 1.5f, 2.5f };
            for (int index = 0; index < verticalCenters.Length; index++)
            {
                CreateCube(
                    partitions,
                    $"Partition_West_{index + 1:00}",
                    new Vector3(-8f, 0.6f, verticalCenters[index]),
                    new Vector3(0.2f, 1.2f, verticalLengths[index]),
                    true);
                CreateCube(
                    partitions,
                    $"Partition_East_{index + 1:00}",
                    new Vector3(8f, 0.6f, verticalCenters[index]),
                    new Vector3(0.2f, 1.2f, verticalLengths[index]),
                    true);
            }

            Vector3[] deskPositions =
            {
                new Vector3(-5.4f, 0.4f, 0.5f),
                new Vector3(5.4f, 0.4f, 0.5f),
                new Vector3(-5.4f, 0.4f, 4f),
                new Vector3(5.4f, 0.4f, 4f),
                new Vector3(-5.4f, 0.4f, 7.5f),
                new Vector3(5.4f, 0.4f, 7.5f)
            };

            for (int index = 0; index < deskPositions.Length; index++)
            {
                CreateCube(
                    desks,
                    $"FixedDesk_{index + 1:00}",
                    deskPositions[index],
                    new Vector3(2.6f, 0.8f, 1.2f),
                    true);
            }
        }

        private static void BuildLayoutMarkers(Transform parent)
        {
            CreateMarker(parent, "FutureBinder_B-D2", new Vector3(12f, 0f, -4.5f), Vector3.one);
            CreateMarker(parent, "FutureBinder_B-A3", new Vector3(9.5f, 0f, 10.5f), Vector3.one);
            CreateMarker(parent, "FutureChair_C-D2", new Vector3(13f, 0f, -2.5f), Vector3.one);
            CreateMarker(parent, "ParkingBay", new Vector3(20f, 0f, 10.5f),
                new Vector3(3f, 1f, 3f));
            CreateMarker(parent, "StrapAssistPocket", new Vector3(13f, 0f, 1.5f),
                new Vector3(6f, 1f, 6f));
            CreateMarker(parent, "RedirectArc", new Vector3(13f, 0f, 5.5f),
                new Vector3(6f, 1f, 6f));
            CreateMarker(parent, "FinisherPocket_8x7", new Vector3(13.5f, 0f, 9f),
                new Vector3(8f, 1f, 7f));
            CreateMarker(parent, "ExitTarget", new Vector3(20.5f, 0f, 16.5f),
                new Vector3(2f, 1f, 1f));
        }

        private static void BuildStaticFurniture(Transform parent)
        {
            CreateCube(parent, "SupplyDesk_West", new Vector3(-20f, 0.5f, -1f),
                new Vector3(3f, 1f, 1.2f), true);
            CreateCube(parent, "PhysicsColumn", new Vector3(12f, 2.1f, 2f),
                new Vector3(0.7f, 4.2f, 0.7f), true);
            GameObject angledDesk = CreateCube(parent, "AngledDesk_East",
                new Vector3(16f, 0.5f, 4f), new Vector3(3f, 1f, 1.2f), true);
            angledDesk.transform.rotation = Quaternion.Euler(0f, 25f, 0f);
            CreateCube(parent, "CrashDesk_North", new Vector3(10f, 0.5f, 12f),
                new Vector3(3f, 1f, 1.2f), true);
            CreateCube(parent, "ImpactPartition", new Vector3(18f, 0.6f, 8f),
                new Vector3(0.2f, 1.2f, 3f), true);
        }

        private static void RepositionPreservedGameplay(Scene scene)
        {
            SetPlanarPosition(RequireRoot(scene, "Player"), 0f, -15f);
            SetPlanarPosition(RequireRoot(scene, "AllyProxy"), -2.4f, -14.2f);
            SetPlanarPosition(RequireRoot(scene, "EnemyProxy_01"), 1f, 1.5f);
            SetPlanarPosition(RequireRoot(scene, "EnemyProxy_02"), 15f, 13f);

            var targetPositions = new Dictionary<string, Vector2>
            {
                { "BINDER-DIRECT-01", new Vector2(-6f, -11.5f) },
                { "BINDER-ALLY-01", new Vector2(-19f, -2.5f) },
                { "BINDER-ALLY-02", new Vector2(-7f, 12.5f) },
                { "CHAIR-DIRECT-01", new Vector2(6f, -11f) },
                { "CHAIR-ALLY-01", new Vector2(-18f, 4f) },
                { "DRAWER-01", new Vector2(15.5f, 10.5f) },
                { "COPIER-01", new Vector2(-20.5f, -10f) }
            };

            PrototypeInteractionTarget[] targets = GetInteractionTargets(scene);
            foreach (PrototypeInteractionTarget target in targets)
            {
                if (!targetPositions.TryGetValue(target.RuntimeInstanceKey, out Vector2 position))
                {
                    throw new InvalidOperationException(
                        $"Unexpected preserved gameplay target: {target.RuntimeInstanceKey}");
                }

                SetPlanarPosition(target.gameObject, position.x, position.y);
            }
        }

        private static void ValidatePreservedGameplayRoots(Scene scene)
        {
            RequireRootAny(scene, "Player", "GAMEPLAY_CHR00_PLAYER");
            RequireRoot(scene, "GameplayCameraRig");
            RequireRoot(scene, "ContestBrowserInput");
            RequireRootAny(scene, "AllyProxy", "GAMEPLAY_CHR01_MOBILE_ALLY");
            RequireRootAny(scene, "EnemyProxy_01", "GAMEPLAY_ENY01_RUNNER_01");
            RequireRootAny(scene, "EnemyProxy_02", "GAMEPLAY_ENY01_RUNNER_02");

            PrototypeInteractionTarget[] targets = GetInteractionTargets(scene);
            string[] expectedKeys =
            {
                "BINDER-ALLY-01",
                "BINDER-ALLY-02",
                "BINDER-DIRECT-01",
                "CHAIR-ALLY-01",
                "CHAIR-DIRECT-01",
                "COPIER-01",
                "DRAWER-01"
            };

            string[] actualKeys = targets
                .Select(target => target.RuntimeInstanceKey)
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();
            if (!actualKeys.SequenceEqual(expectedKeys))
            {
                throw new InvalidOperationException(
                    "S1 must preserve exactly the seven C0-C4 gameplay targets without duplication.");
            }
        }

        private static PrototypeInteractionTarget[] GetInteractionTargets(Scene scene)
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<PrototypeInteractionTarget>(true))
                .OrderBy(target => target.RuntimeInstanceKey, StringComparer.Ordinal)
                .ToArray();
        }

        private static void RemoveLegacyEnvironment(Scene scene)
        {
            foreach (string rootName in LegacyEnvironmentRootNames)
            {
                GameObject root = FindRoot(scene, rootName);
                if (root != null)
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }

        private static void EnsureShowcaseFolder()
        {
            if (AssetDatabase.IsValidFolder(ShowcaseSceneFolder))
            {
                return;
            }

            string guid = AssetDatabase.CreateFolder(
                "Assets/_Project/Scenes",
                "20_Showcase");
            if (string.IsNullOrWhiteSpace(guid))
            {
                throw new InvalidOperationException(
                    $"Could not create {ShowcaseSceneFolder}.");
            }
        }

        private static void RequireSceneAsset(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
            {
                throw new InvalidOperationException($"Required Scene does not exist: {path}");
            }
        }

        private static GameObject CreateRoot(Scene scene, string name)
        {
            var root = new GameObject(name);
            SceneManager.MoveGameObjectToScene(root, scene);
            return root;
        }

        private static Transform CreateGroup(Transform parent, string name)
        {
            var group = new GameObject(name).transform;
            group.SetParent(parent, false);
            return group;
        }

        private static Transform CreateMarker(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale)
        {
            Transform marker = CreateGroup(parent, name);
            marker.position = position;
            marker.localScale = scale;
            return marker;
        }

        private static LineRenderer CreateReviewLine(
            Transform parent,
            string name,
            IReadOnlyList<Vector3> points,
            Color color,
            float width,
            bool loop,
            bool worldSpace = true)
        {
            Transform lineTransform = CreateGroup(parent, name);
            var line = lineTransform.gameObject.AddComponent<LineRenderer>();
            line.useWorldSpace = worldSpace;
            line.loop = loop;
            line.positionCount = points.Count;
            for (int index = 0; index < points.Count; index++)
            {
                line.SetPosition(index, points[index]);
            }

            line.startWidth = width;
            line.endWidth = width;
            line.startColor = color;
            line.endColor = color;
            line.numCornerVertices = 2;
            line.numCapVertices = 2;
            line.alignment = LineAlignment.View;
            line.textureMode = LineTextureMode.Stretch;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.lightProbeUsage = LightProbeUsage.Off;
            line.reflectionProbeUsage = ReflectionProbeUsage.Off;
            Material lineMaterial = AssetDatabase.GetBuiltinExtraResource<Material>(
                "Default-Line.mat");
            if (lineMaterial != null)
            {
                line.sharedMaterial = lineMaterial;
            }
            return line;
        }

        private static TextMesh CreateReviewLabel(
            Transform parent,
            string name,
            string text,
            Vector3 position,
            Quaternion rotation,
            Color color,
            float characterSize,
            bool worldPosition = true)
        {
            Transform labelTransform = CreateGroup(parent, name);
            if (worldPosition)
            {
                labelTransform.SetPositionAndRotation(position, rotation);
            }
            else
            {
                labelTransform.localPosition = position;
                labelTransform.localRotation = rotation;
            }

            var label = labelTransform.gameObject.AddComponent<TextMesh>();
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = characterSize;
            label.fontSize = 64;
            label.fontStyle = FontStyle.Bold;
            label.color = color;
            label.richText = false;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                label.font = font;
                MeshRenderer renderer = label.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = font.material;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    renderer.lightProbeUsage = LightProbeUsage.Off;
                    renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                }
            }
            return label;
        }

        private static void CaptureV2ReviewFrame(
            Scene scene,
            string outputPath,
            string cameraName,
            Vector3 position,
            Quaternion rotation,
            bool orthographic,
            float orthographicSize,
            float fieldOfView)
        {
            var cameraObject = new GameObject(cameraName);
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            cameraObject.transform.SetPositionAndRotation(position, rotation);
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.045f, 0.06f, 1f);
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 120f;
            camera.fieldOfView = fieldOfView;
            camera.orthographic = orthographic;
            camera.orthographicSize = orthographicSize;
            camera.aspect = 16f / 9f;
            camera.allowHDR = false;
            camera.allowMSAA = true;

            var renderTexture = new RenderTexture(
                1920,
                1080,
                24,
                RenderTextureFormat.ARGB32)
            {
                name = cameraName + "_1920x1080",
                antiAliasing = 1
            };
            var image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            RenderTexture previousActive = RenderTexture.active;
            try
            {
                renderTexture.Create();
                camera.targetTexture = renderTexture;
                camera.Render();
                RenderTexture.active = renderTexture;
                image.ReadPixels(new Rect(0f, 0f, 1920f, 1080f), 0, 0, false);
                image.Apply(false, false);
                byte[] png = image.EncodeToPNG();
                if (png == null || png.Length == 0)
                {
                    throw new InvalidOperationException(
                        $"Could not encode readability capture: {outputPath}");
                }
                File.WriteAllBytes(outputPath, png);
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previousActive;
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(image);
                UnityEngine.Object.DestroyImmediate(renderTexture);
                UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }

        private static GameObject CreateCube(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            bool collidable)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.position = position;
            cube.transform.localScale = scale;
            if (!collidable)
            {
                UnityEngine.Object.DestroyImmediate(cube.GetComponent<Collider>());
            }

            GameObjectUtility.SetStaticEditorFlags(
                cube,
                StaticEditorFlags.BatchingStatic |
                StaticEditorFlags.OccluderStatic |
                StaticEditorFlags.OccludeeStatic);
            return cube;
        }

        private static void SetActorReviewTransform(
            GameObject actor,
            string reviewName,
            Vector2 position)
        {
            actor.name = reviewName;
            actor.transform.SetPositionAndRotation(
                new Vector3(position.x, 1f, position.y),
                Quaternion.identity);
            EditorUtility.SetDirty(actor);
            EditorUtility.SetDirty(actor.transform);
        }

        private static void SetPlanarPosition(GameObject gameObject, float x, float z)
        {
            Vector3 position = gameObject.transform.position;
            gameObject.transform.position = new Vector3(x, position.y, z);
            EditorUtility.SetDirty(gameObject.transform);
        }

        private static GameObject RequireRoot(Scene scene, string name)
        {
            GameObject root = FindRoot(scene, name);
            if (root == null)
            {
                throw new InvalidOperationException($"Required Scene root is missing: {name}");
            }

            return root;
        }

        private static GameObject RequireRootAny(Scene scene, params string[] names)
        {
            foreach (string name in names)
            {
                GameObject root = FindRoot(scene, name);
                if (root != null)
                {
                    return root;
                }
            }

            throw new InvalidOperationException(
                "Required Scene root is missing: " + string.Join(" or ", names));
        }

        private static T RequireSingleSceneComponent<T>(Scene scene) where T : Component
        {
            T[] components = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
            if (components.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Scene requires exactly one {typeof(T).Name}, found {components.Length}.");
            }

            return components[0];
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            return scene.GetRootGameObjects()
                .FirstOrDefault(root => string.Equals(root.name, name, StringComparison.Ordinal));
        }

        private readonly struct GameplayReviewSpec
        {
            public GameplayReviewSpec(string name, Vector2 position, Vector3 bounds)
            {
                Name = name;
                Position = position;
                Bounds = bounds;
            }

            public string Name { get; }

            public Vector2 Position { get; }

            public Vector3 Bounds { get; }
        }

        private readonly struct SocketSpec
        {
            public SocketSpec(
                string assetId,
                Vector3 position,
                Vector3 intendedBounds,
                string pivotReference,
                bool isSpatial)
            {
                AssetId = assetId;
                Position = position;
                IntendedBounds = intendedBounds;
                PivotReference = pivotReference;
                IsSpatial = isSpatial;
            }

            public string AssetId { get; }

            public Vector3 Position { get; }

            public Vector3 IntendedBounds { get; }

            public string PivotReference { get; }

            public bool IsSpatial { get; }
        }
    }
}
