using System;
using System.Collections;
using System.Collections.Generic;
using BreakNotice.Gameplay.Encounter;
using BreakNotice.Gameplay.Interaction;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace BreakNotice.Contest
{
    /// <summary>
    /// Composes the proven v007 gameplay Scene with the pinned Stage01 art Scene.
    ///
    /// Authority is deliberately simple:
    /// - v007 keeps gameplay, actors, anchors, and the expanded support floor.
    /// - Stage01 contributes visuals, lighting, map collision and gameplay props.
    ///
    /// The pinned FBX imports intentionally ship with addColliders disabled. Runtime
    /// authoring therefore creates collision data for solid props and exact static
    /// MeshColliders only for architecture whose openings must stay open. Ordinary
    /// prop colliders are spatially streamed around CHR00 instead of all remaining
    /// registered in PhysX for the entire map.
    /// The visual floor is the only exception: the already-proven
    /// PROXY_Floor_64x44 remains its physical authority and expands to the complete
    /// Stage01 footprint, avoiding duplicate floor contacts and spawn regressions.
    /// Office Chairs become normal Medium interaction targets immediately. The map
    /// contains thousands of decorative File Binders, so a Binder collider is lazily
    /// allocated only when its spatial cell becomes relevant and it becomes a Light
    /// interaction target only when the player actually aims at it. This keeps every
    /// reachable Binder usable without thousands of colliders, Rigidbodies or target
    /// scans remaining active every frame.
    ///
    /// Startup is fail-closed. A dedicated black curtain Camera is installed in
    /// Awake before additive loading begins, gameplay Cameras/Canvases/Listeners
    /// are suspended as soon as v007 activates, and simulation/audio remain paused
    /// until the final map is recentered, material-compatible and ready. This
    /// prevents the graybox -> pink shader -> final map sequence from ever reaching
    /// a shipping frame.
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseStage01Bootstrap : MonoBehaviour
    {
        public const string IntegrationSourceCommit =
            "223d48de8be514bd8235c76c9fe32d1270927675";

        public const string BootstrapSceneName =
            "Contest_OperationsFloor_Showcase_Stage01";

        public const string BootstrapScenePath =
            "Assets/_Project/Scenes/20_Showcase/" +
            "Contest_OperationsFloor_Showcase_Stage01.unity";

        public const string GameplaySceneName =
            "Contest_OperationsFloor_Showcase_v007";

        public const string GameplayScenePath =
            "Assets/_Project/Scenes/20_Showcase/" +
            "Contest_OperationsFloor_Showcase_v007.unity";

        public const string EnvironmentSceneName = "Stage01_OfficeMap";

        public const string EnvironmentScenePath =
            "Packages/com.breaknotice.stage01.scene/Stage01_OfficeMap.unity";

        public const string EnvironmentPreviewCharacterAssetPath =
            "Packages/com.breaknotice.stage01.preview-character/" +
            "CHR00_Aim_Idle.fbx";

        public const string EnvironmentFloorObjectName = "floor";

        public const string LegacyEnvironmentRootName =
            "SHOWCASE_V2_PROVISIONAL_LOCAL_REVIEW_ONLY";

        public const string LegacyFloorColliderName = "PROXY_Floor_64x44";

        public const float LegacyFloorCoverageMargin = 4f;

        public const string StageChairNamePrefix = "PRP_OfficeChair_";

        public const string StageBinderNamePrefix = "PRP_FileBinder_";

        public const float StageChairMass = 8f;

        public const float StageBinderMass = 1f;

        public const float StageBinderColliderActivationRadius = 4.5f;

        public const float StageStaticColliderActivationRadius = 14f;

        public const float StageBinderVisualActivationRadius = 11f;

        public const float StageSmallPropVisualActivationRadius = 16f;

        public const float StageMediumPropVisualActivationRadius = 24f;

        public const float StageLargePropVisualActivationRadius = 32f;

        public const float StageLightActivationRadius = 18f;

        public const float StageColliderMinimumWorldThickness = 0.025f;

        public const string LoadingCurtainCameraName =
            "STAGE01_STARTUP_CURTAIN_CAMERA";

        public const float LoadingCurtainCameraDepth = 32767f;

        public const string RuntimeBootstrapRootName =
            "SHOWCASE_STAGE01_FINAL_MAP_BOOTSTRAP";

        private const string StageChairRuntimeKeyPrefix = "STAGE01-CHAIR-";

        private const string StageBinderRuntimeKeyPrefix = "STAGE01-BINDER-";

        private const float StageSpatialCellSize = 6f;

        private const float StageSpatialRefreshInterval = 0.25f;

        private const float StageSpatialRefreshMovement = 1.25f;

        private const float StageBinderReticleMaximumDistance = 12f;

        private const float StageVisualDeactivationHysteresis = 2f;

        private const int StageBinderReticleHitCapacity = 128;

        private static ShowcaseStage01Bootstrap activeInstance;
        private static readonly List<Material> RuntimeStageMaterials =
            new List<Material>();
        private static readonly HashSet<Material> RuntimeStageInstancedMaterials =
            new HashSet<Material>();

        private readonly List<Camera> suspendedGameplayCameras =
            new List<Camera>();
        private readonly List<Canvas> suspendedGameplayCanvases =
            new List<Canvas>();
        private readonly List<AudioListener> suspendedGameplayListeners =
            new List<AudioListener>();
        private readonly List<PrototypeInteractionTarget> registeredInteractionTargets =
            new List<PrototypeInteractionTarget>();
        private readonly List<PrototypeInteractionTarget> promotedStageBinderTargets =
            new List<PrototypeInteractionTarget>();
        private readonly Dictionary<Vector2Int, List<StageBinderCandidate>>
            stageBinderGrid =
                new Dictionary<Vector2Int, List<StageBinderCandidate>>();
        private readonly HashSet<StageBinderCandidate> activeStageBinderCandidates =
            new HashSet<StageBinderCandidate>();
        private readonly HashSet<StageBinderCandidate> desiredStageBinderCandidates =
            new HashSet<StageBinderCandidate>();
        private readonly List<StageBinderCandidate> stageBinderRejectBuffer =
            new List<StageBinderCandidate>();
        private readonly Dictionary<Vector2Int, List<StageStreamedColliderCandidate>>
            stageStreamedColliderGrid =
                new Dictionary<Vector2Int, List<StageStreamedColliderCandidate>>();
        private readonly HashSet<StageStreamedColliderCandidate>
            activeStageStreamedColliders =
                new HashSet<StageStreamedColliderCandidate>();
        private readonly HashSet<StageStreamedColliderCandidate>
            desiredStageStreamedColliders =
                new HashSet<StageStreamedColliderCandidate>();
        private readonly List<StageStreamedColliderCandidate>
            stageStreamedDeactivateBuffer =
                new List<StageStreamedColliderCandidate>();
        private readonly List<StageStreamedColliderCandidate>
            stageStreamedRejectBuffer =
                new List<StageStreamedColliderCandidate>();
        private readonly Dictionary<Vector2Int, List<StageVisualCandidate>>
            stageVisualGrid =
                new Dictionary<Vector2Int, List<StageVisualCandidate>>();
        private readonly HashSet<StageVisualCandidate> activeStageVisuals =
            new HashSet<StageVisualCandidate>();
        private readonly HashSet<StageVisualCandidate> desiredStageVisuals =
            new HashSet<StageVisualCandidate>();
        private readonly List<StageVisualCandidate> stageVisualDeactivateBuffer =
            new List<StageVisualCandidate>();
        private readonly List<StageVisualCandidate> stageVisualRejectBuffer =
            new List<StageVisualCandidate>();
        private readonly HashSet<StageBinderCandidate> activeStageBinderVisuals =
            new HashSet<StageBinderCandidate>();
        private readonly HashSet<StageBinderCandidate> desiredStageBinderVisuals =
            new HashSet<StageBinderCandidate>();
        private readonly List<StageBinderCandidate>
            stageBinderVisualDeactivateBuffer =
                new List<StageBinderCandidate>();
        private readonly List<StageBinderCandidate> stageBinderVisualRejectBuffer =
            new List<StageBinderCandidate>();
        private readonly Dictionary<Vector2Int, List<StageLightCandidate>>
            stageLightGrid =
                new Dictionary<Vector2Int, List<StageLightCandidate>>();
        private readonly HashSet<StageLightCandidate> activeStageLights =
            new HashSet<StageLightCandidate>();
        private readonly HashSet<StageLightCandidate> desiredStageLights =
            new HashSet<StageLightCandidate>();
        private readonly List<StageLightCandidate> stageLightDeactivateBuffer =
            new List<StageLightCandidate>();
        private readonly List<StageLightCandidate> stageLightRejectBuffer =
            new List<StageLightCandidate>();
        private readonly List<StageDynamicPropBaseline> stageDynamicPropBaselines =
            new List<StageDynamicPropBaseline>();
        private readonly RaycastHit[] stageBinderReticleHits =
            new RaycastHit[StageBinderReticleHitCapacity];

        private bool compositionStarted;
        private bool compositionCompleted;
        private string compositionFailure = string.Empty;
        private Camera loadingCurtainCamera;
        private bool startupSimulationPaused;
        private float timeScaleBeforeStartupGate = 1f;
        private bool audioPauseBeforeStartupGate;
        private bool gameplayPresentationSuspended;
        private bool gameplaySceneActivatedUnderCurtain;
        private bool environmentSceneActivatedUnderCurtain;
        private int gameplaySceneActivationFrame = -1;
        private int environmentSceneActivationFrame = -1;
        private int finalMapPreparedFrame = -1;
        private int presentationReleasedFrame = -1;
        private int suspendedGameplayCameraCount;
        private int suspendedGameplayCanvasCount;
        private int suspendedGameplayListenerCount;
        private Vector3 appliedEnvironmentOffset;
        private int convertedStageMaterialCount;
        private int disabledFinalMapColliderCount;
        private int enabledFinalMapColliderCount;
        private int generatedFinalMapColliderCount;
        private int totalFinalMapColliderCount;
        private int stageStaticColliderCount;
        private int activeStreamedStageColliderCount;
        private int activeStageVisualRootCount;
        private int activeStageBinderVisualCount;
        private int activeStageLightCount;
        private int totalStreamedStageVisualRootCount;
        private int totalStageLightCount;
        private int stageChairTargetCount;
        private int stageBinderRootCount;
        private int promotedStageBinderCount;
        private int activeDormantBinderColliderCount;
        private int preservedLegacyColliderCount;
        private Bounds expandedLegacyFloorBounds;
        private PrototypeInteractionController interactionController;
        private AuthoredEncounterResetController encounterResetController;
        private Transform interactionPlayer;
        private float nextSpatialRefreshTime = float.NegativeInfinity;
        private Vector3 lastSpatialRefreshPosition;
        private bool hasSpatialRefreshPosition;

        public bool CompositionCompleted => compositionCompleted;

        public string CompositionFailure => compositionFailure;

        public Vector3 AppliedEnvironmentOffset => appliedEnvironmentOffset;

        public int ConvertedStageMaterialCount => convertedStageMaterialCount;

        public int DisabledFinalMapColliderCount => disabledFinalMapColliderCount;

        public int EnabledFinalMapColliderCount => enabledFinalMapColliderCount;

        public int GeneratedFinalMapColliderCount => generatedFinalMapColliderCount;

        public int TotalFinalMapColliderCount => totalFinalMapColliderCount;

        public int StageChairTargetCount => stageChairTargetCount;

        public int ActiveStreamedStageColliderCount =>
            activeStreamedStageColliderCount;

        public int ActiveStageVisualRootCount => activeStageVisualRootCount;

        public int TotalStreamedStageVisualRootCount =>
            totalStreamedStageVisualRootCount;

        public int ActiveStageBinderVisualCount =>
            activeStageBinderVisualCount;

        public int ActiveStageLightCount => activeStageLightCount;

        public int TotalStageLightCount => totalStageLightCount;

        public int StageBinderRootCount => stageBinderRootCount;

        public int DormantStageBinderCount =>
            Mathf.Max(0, stageBinderRootCount - promotedStageBinderCount);

        public int PromotedStageBinderCount => promotedStageBinderCount;

        public int ActiveDormantBinderColliderCount =>
            activeDormantBinderColliderCount;

        public int RegisteredInteractionTargetCount =>
            registeredInteractionTargets.Count;

        public int PreservedLegacyColliderCount => preservedLegacyColliderCount;

        public Bounds ExpandedLegacyFloorBounds => expandedLegacyFloorBounds;

        public Camera LoadingCurtainCamera => loadingCurtainCamera;

        public bool LoadingCurtainActive =>
            loadingCurtainCamera != null &&
            loadingCurtainCamera.enabled &&
            loadingCurtainCamera.gameObject.activeInHierarchy;

        public bool StartupSimulationPaused => startupSimulationPaused;

        public bool GameplayPresentationSuspended =>
            gameplayPresentationSuspended;

        public bool GameplaySceneActivatedUnderCurtain =>
            gameplaySceneActivatedUnderCurtain;

        public bool EnvironmentSceneActivatedUnderCurtain =>
            environmentSceneActivatedUnderCurtain;

        public int GameplaySceneActivationFrame => gameplaySceneActivationFrame;

        public int EnvironmentSceneActivationFrame =>
            environmentSceneActivationFrame;

        public int FinalMapPreparedFrame => finalMapPreparedFrame;

        public int PresentationReleasedFrame => presentationReleasedFrame;

        public int SuspendedGameplayCameraCount =>
            suspendedGameplayCameraCount;

        public int SuspendedGameplayCanvasCount =>
            suspendedGameplayCanvasCount;

        public int SuspendedGameplayListenerCount =>
            suspendedGameplayListenerCount;

        public static string[] CreateBuildScenePaths()
        {
            return new[]
            {
                BootstrapScenePath,
                GameplayScenePath,
                EnvironmentScenePath
            };
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            ReleaseRuntimeStageMaterials();
            activeInstance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallForLoadedBootstrapScene()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            TryCreateForScene(SceneManager.GetActiveScene());
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryCreateForScene(scene);
            if (activeInstance != null)
            {
                activeInstance.HandleAdditiveSceneActivated(scene);
            }
        }

        private static void TryCreateForScene(Scene scene)
        {
            if (!scene.IsValid() ||
                !scene.isLoaded ||
                !string.Equals(
                    scene.name,
                    BootstrapSceneName,
                    StringComparison.Ordinal) ||
                activeInstance != null)
            {
                return;
            }

            var bootstrapRoot = new GameObject(RuntimeBootstrapRootName);
            SceneManager.MoveGameObjectToScene(bootstrapRoot, scene);
            activeInstance = bootstrapRoot.AddComponent<ShowcaseStage01Bootstrap>();
        }

        private void Awake()
        {
            if (activeInstance != null && activeInstance != this)
            {
                Destroy(gameObject);
                return;
            }

            activeInstance = this;
            BeginStartupPresentationGate();
        }

        private void Start()
        {
            if (!compositionStarted)
            {
                StartCoroutine(ComposeScenes());
            }
        }

        private void Update()
        {
            if (!compositionCompleted)
            {
                return;
            }

            RefreshSpatialColliders(false);
        }

        public void RefreshRuntimeSpatialStateNow()
        {
            if (compositionCompleted)
            {
                RefreshSpatialColliders(true);
            }
        }

        private void OnDestroy()
        {
            if (encounterResetController != null)
            {
                encounterResetController.ResetPerformed -= HandleEncounterReset;
            }
            interactionController?.ConfigureRuntimeReticleTargetResolver(null);

            if (activeInstance != this)
            {
                return;
            }

            RestoreGameplayPresentation();
            EndStartupSimulationPause();
            activeInstance = null;
            ReleaseRuntimeStageMaterials();
        }

        private void BeginStartupPresentationGate()
        {
            if (loadingCurtainCamera == null)
            {
                var curtainObject = new GameObject(LoadingCurtainCameraName);
                curtainObject.transform.SetParent(transform, false);
                loadingCurtainCamera = curtainObject.AddComponent<Camera>();
                loadingCurtainCamera.clearFlags = CameraClearFlags.SolidColor;
                loadingCurtainCamera.backgroundColor = Color.black;
                loadingCurtainCamera.cullingMask = 0;
                loadingCurtainCamera.depth = LoadingCurtainCameraDepth;
                loadingCurtainCamera.rect = new Rect(0f, 0f, 1f, 1f);
                loadingCurtainCamera.targetDisplay = 0;
                loadingCurtainCamera.allowHDR = false;
                loadingCurtainCamera.allowMSAA = false;
                loadingCurtainCamera.useOcclusionCulling = false;
                loadingCurtainCamera.stereoTargetEye = StereoTargetEyeMask.None;
            }

            loadingCurtainCamera.enabled = true;

            if (startupSimulationPaused)
            {
                return;
            }

            timeScaleBeforeStartupGate = Time.timeScale;
            audioPauseBeforeStartupGate = AudioListener.pause;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            startupSimulationPaused = true;
        }

        private void EndStartupSimulationPause()
        {
            if (!startupSimulationPaused)
            {
                return;
            }

            Time.timeScale = timeScaleBeforeStartupGate;
            AudioListener.pause = audioPauseBeforeStartupGate;
            startupSimulationPaused = false;
        }

        private void ReleaseLoadingCurtain()
        {
            if (loadingCurtainCamera != null)
            {
                loadingCurtainCamera.enabled = false;
            }
        }

        private void HandleAdditiveSceneActivated(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            if (SceneMatches(scene, GameplaySceneName, GameplayScenePath))
            {
                if (gameplaySceneActivationFrame < 0)
                {
                    gameplaySceneActivationFrame = Time.frameCount;
                    gameplaySceneActivatedUnderCurtain = LoadingCurtainActive;
                }

                SuspendGameplayPresentation(scene);
                return;
            }

            if (!SceneMatches(scene, EnvironmentSceneName, EnvironmentScenePath))
            {
                return;
            }

            if (environmentSceneActivationFrame < 0)
            {
                environmentSceneActivationFrame = Time.frameCount;
                environmentSceneActivatedUnderCurtain = LoadingCurtainActive;
            }

            // The art Scene should never own a startup Camera, Overlay Canvas or
            // AudioListener. Disable them in the activation callback itself so an
            // imported preview setup cannot bypass the curtain contract.
            DisableComponents(GetSceneComponents<Camera>(scene));
            DisableComponents(GetSceneComponents<Canvas>(scene));
            DisableComponents(GetSceneComponents<AudioListener>(scene));
        }

        private void SuspendGameplayPresentation(Scene gameplayScene)
        {
            if (!gameplayScene.IsValid() || !gameplayScene.isLoaded)
            {
                return;
            }

            Camera[] cameras = GetSceneComponents<Camera>(gameplayScene);
            for (int index = 0; index < cameras.Length; index++)
            {
                Camera camera = cameras[index];
                if (camera == null ||
                    !camera.enabled ||
                    suspendedGameplayCameras.Contains(camera))
                {
                    continue;
                }

                suspendedGameplayCameras.Add(camera);
                camera.enabled = false;
            }

            Canvas[] canvases = GetSceneComponents<Canvas>(gameplayScene);
            for (int index = 0; index < canvases.Length; index++)
            {
                Canvas canvas = canvases[index];
                if (canvas == null ||
                    !canvas.enabled ||
                    suspendedGameplayCanvases.Contains(canvas))
                {
                    continue;
                }

                suspendedGameplayCanvases.Add(canvas);
                canvas.enabled = false;
            }

            AudioListener[] listeners =
                GetSceneComponents<AudioListener>(gameplayScene);
            for (int index = 0; index < listeners.Length; index++)
            {
                AudioListener listener = listeners[index];
                if (listener == null ||
                    !listener.enabled ||
                    suspendedGameplayListeners.Contains(listener))
                {
                    continue;
                }

                suspendedGameplayListeners.Add(listener);
                listener.enabled = false;
            }

            gameplayPresentationSuspended = true;
        }

        private void RestoreGameplayPresentation()
        {
            suspendedGameplayCameraCount = suspendedGameplayCameras.Count;
            suspendedGameplayCanvasCount = suspendedGameplayCanvases.Count;
            suspendedGameplayListenerCount = suspendedGameplayListeners.Count;

            for (int index = 0; index < suspendedGameplayCameras.Count; index++)
            {
                Camera camera = suspendedGameplayCameras[index];
                if (camera != null)
                {
                    camera.enabled = true;
                }
            }

            for (int index = 0; index < suspendedGameplayCanvases.Count; index++)
            {
                Canvas canvas = suspendedGameplayCanvases[index];
                if (canvas != null)
                {
                    canvas.enabled = true;
                }
            }

            for (int index = 0; index < suspendedGameplayListeners.Count; index++)
            {
                AudioListener listener = suspendedGameplayListeners[index];
                if (listener != null)
                {
                    listener.enabled = true;
                }
            }

            suspendedGameplayCameras.Clear();
            suspendedGameplayCanvases.Clear();
            suspendedGameplayListeners.Clear();
            gameplayPresentationSuspended = false;
        }

        private IEnumerator ComposeScenes()
        {
            compositionStarted = true;
            BeginStartupPresentationGate();

            yield return LoadSceneIfNeeded(GameplaySceneName, GameplayScenePath);
            if (!string.IsNullOrEmpty(compositionFailure))
            {
                yield break;
            }

            Scene gameplayScene = ResolveLoadedScene(
                GameplaySceneName,
                GameplayScenePath);
            if (!gameplayScene.IsValid() || !gameplayScene.isLoaded)
            {
                Fail($"Loaded gameplay Scene was not available: {GameplaySceneName}.");
                yield break;
            }

            // The sceneLoaded callback normally performs this in the same
            // activation frame. Keep this idempotent call for already-loaded
            // editor/test compositions as well.
            SuspendGameplayPresentation(gameplayScene);

            yield return LoadSceneIfNeeded(EnvironmentSceneName, EnvironmentScenePath);
            if (!string.IsNullOrEmpty(compositionFailure))
            {
                yield break;
            }

            Scene environmentScene = ResolveLoadedScene(
                EnvironmentSceneName,
                EnvironmentScenePath);
            if (!environmentScene.IsValid() ||
                !environmentScene.isLoaded ||
                environmentScene.GetRootGameObjects().Length == 0)
            {
                Fail($"Loaded Stage01 Scene was empty or unavailable: {EnvironmentSceneName}.");
                yield break;
            }

            GameObject legacyEnvironment = FindRoot(
                gameplayScene,
                LegacyEnvironmentRootName);
            if (legacyEnvironment == null)
            {
                Fail(
                    "The preserved v007 gameplay Scene did not contain its collision/" +
                    $"anchor root: {LegacyEnvironmentRootName}.");
                yield break;
            }

            try
            {
                appliedEnvironmentOffset = RecenterStageEnvironment(environmentScene);
                stageStaticColliderCount = PrepareStageEnvironment(
                    environmentScene,
                    out disabledFinalMapColliderCount,
                    out generatedFinalMapColliderCount,
                    out convertedStageMaterialCount);
                totalFinalMapColliderCount =
                    GetSceneComponents<Collider>(environmentScene).Length;

                ConfigureStageGameplayProps(gameplayScene, environmentScene);

                SuppressLegacyEnvironment(
                    legacyEnvironment,
                    out preservedLegacyColliderCount);

                Collider support = FindLegacyFloorCollider(legacyEnvironment);
                if (support == null ||
                    !support.enabled ||
                    !support.gameObject.activeInHierarchy)
                {
                    throw new InvalidOperationException(
                        $"Required v007 support collider is unavailable: {LegacyFloorColliderName}.");
                }

                expandedLegacyFloorBounds = ExpandLegacyFloorToCoverStage(
                    support,
                    environmentScene,
                    LegacyFloorCoverageMargin);

                if (!SceneManager.SetActiveScene(gameplayScene))
                {
                    throw new InvalidOperationException(
                        $"Could not activate gameplay Scene {GameplaySceneName}.");
                }

                ApplyStage01Lighting();
                Physics.SyncTransforms();
                RefreshSpatialColliders(true);
                UpdateStageColliderStatistics();

                finalMapPreparedFrame = Time.frameCount;
                RestoreGameplayPresentation();
            }
            catch (Exception exception)
            {
                Fail(
                    "Could not compose Stage01 visuals over the proven v007 " +
                    $"gameplay/collision Scene: {exception.Message}");
                yield break;
            }

            // Render one complete hidden frame with the final map and restored
            // gameplay Camera behind the curtain. The curtain is removed only on
            // the next frame, so no graybox or unsupported-shader frame can leak.
            yield return null;

            EndStartupSimulationPause();
            ReleaseLoadingCurtain();
            presentationReleasedFrame = Time.frameCount;
            compositionCompleted = true;
            Debug.Log(
                "BREAK NOTICE Stage01 collision integration ready. " +
                $"source={IntegrationSourceCommit}; gameplay={GameplaySceneName}; " +
                $"environment={EnvironmentSceneName}; " +
                $"environmentOffset={appliedEnvironmentOffset}; " +
                $"legacyCollidersPreserved={preservedLegacyColliderCount}; " +
                $"legacyFloorCenter={expandedLegacyFloorBounds.center}; " +
                $"legacyFloorSize={expandedLegacyFloorBounds.size}; " +
                $"finalMapCollidersEnabled={enabledFinalMapColliderCount}; " +
                $"finalMapCollidersDisabled={disabledFinalMapColliderCount}; " +
                $"finalMapCollidersGenerated={generatedFinalMapColliderCount}; " +
                $"finalMapCollidersTotal={totalFinalMapColliderCount}; " +
                $"stageAlwaysOnColliders={stageStaticColliderCount}; " +
                $"stageStreamedCollidersActive={activeStreamedStageColliderCount}; " +
                $"stageVisualRootsActive={activeStageVisualRootCount}; " +
                $"stageVisualRootsStreamed={totalStreamedStageVisualRootCount}; " +
                $"stageBinderVisualsActive={activeStageBinderVisualCount}; " +
                $"stageLightsActive={activeStageLightCount}; " +
                $"stageLightsTotal={totalStageLightCount}; " +
                $"stageChairTargets={stageChairTargetCount}; " +
                $"stageBinderRoots={stageBinderRootCount}; " +
                $"stageBindersPromoted={promotedStageBinderCount}; " +
                $"stageBinderCollidersActive={activeDormantBinderColliderCount}; " +
                $"stageMaterialsConverted={convertedStageMaterialCount}; " +
                $"startupCurtainGameplayFrame={gameplaySceneActivationFrame}; " +
                $"startupCurtainEnvironmentFrame={environmentSceneActivationFrame}; " +
                $"finalPreparedFrame={finalMapPreparedFrame}; " +
                $"presentationReleasedFrame={presentationReleasedFrame}; " +
                $"suspendedCameras={suspendedGameplayCameraCount}; " +
                $"suspendedCanvases={suspendedGameplayCanvasCount}; " +
                $"suspendedListeners={suspendedGameplayListenerCount}.");
        }

        private IEnumerator LoadSceneIfNeeded(string sceneName, string scenePath)
        {
            Scene existing = ResolveLoadedScene(sceneName, scenePath);
            if (existing.IsValid() && existing.isLoaded)
            {
                yield break;
            }

            AsyncOperation operation;
            try
            {
                operation = SceneManager.LoadSceneAsync(
                    scenePath,
                    LoadSceneMode.Additive);
            }
            catch (Exception exception)
            {
                Fail($"Could not request additive Scene {sceneName}: {exception.Message}");
                yield break;
            }

            if (operation == null)
            {
                Fail($"Unity returned no load operation for additive Scene {sceneName}.");
                yield break;
            }

            while (!operation.isDone)
            {
                yield return null;
            }

            Scene loaded = ResolveLoadedScene(sceneName, scenePath);
            if (!loaded.IsValid() || !loaded.isLoaded)
            {
                Fail(
                    $"Additive Scene did not finish loading: {scenePath}. " +
                    $"Loaded Scenes: {DescribeLoadedScenes()}.");
            }
        }

        public static Scene ResolveLoadedScene(string sceneName, string scenePath)
        {
            if (!string.IsNullOrEmpty(scenePath))
            {
                Scene byPath = SceneManager.GetSceneByPath(scenePath);
                if (byPath.IsValid() && byPath.isLoaded)
                {
                    return byPath;
                }
            }

            if (!string.IsNullOrEmpty(sceneName))
            {
                Scene byName = SceneManager.GetSceneByName(sceneName);
                if (byName.IsValid() && byName.isLoaded)
                {
                    return byName;
                }
            }

            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene candidate = SceneManager.GetSceneAt(index);
                if (!candidate.IsValid() || !candidate.isLoaded)
                {
                    continue;
                }

                if ((!string.IsNullOrEmpty(sceneName) &&
                     string.Equals(
                         candidate.name,
                         sceneName,
                         StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(scenePath) &&
                     string.Equals(
                         candidate.path,
                         scenePath,
                         StringComparison.OrdinalIgnoreCase)))
                {
                    return candidate;
                }
            }

            return default;
        }

        private static bool SceneMatches(
            Scene scene,
            string expectedName,
            string expectedPath)
        {
            return scene.IsValid() &&
                ((!string.IsNullOrEmpty(expectedName) &&
                  string.Equals(
                      scene.name,
                      expectedName,
                      StringComparison.OrdinalIgnoreCase)) ||
                 (!string.IsNullOrEmpty(expectedPath) &&
                  string.Equals(
                      scene.path,
                      expectedPath,
                      StringComparison.OrdinalIgnoreCase)));
        }

        public static int SuppressLegacyEnvironment(GameObject legacyRoot)
        {
            return SuppressLegacyEnvironment(legacyRoot, out _);
        }

        /// <summary>
        /// Hides the provisional map and disables every legacy environment collider
        /// except the single support floor. Gameplay actors, interactables and
        /// controller roots live outside this legacy environment root and remain
        /// untouched. Empty anchor transforms stay alive for existing references.
        /// </summary>
        public static int SuppressLegacyEnvironment(
            GameObject legacyRoot,
            out int preservedColliders)
        {
            if (legacyRoot == null)
            {
                throw new ArgumentNullException(nameof(legacyRoot));
            }

            int changed = 0;
            changed += DisableComponents(
                legacyRoot.GetComponentsInChildren<Renderer>(true));
            changed += DisableComponents(
                legacyRoot.GetComponentsInChildren<Light>(true));
            changed += DisableComponents(
                legacyRoot.GetComponentsInChildren<AudioSource>(true));
            changed += DisableComponents(
                legacyRoot.GetComponentsInChildren<Canvas>(true));

            Animator[] animators = legacyRoot.GetComponentsInChildren<Animator>(true);
            changed += DisableComponents(animators);

            preservedColliders = 0;
            Collider[] colliders = legacyRoot.GetComponentsInChildren<Collider>(true);
            for (int index = 0; index < colliders.Length; index++)
            {
                Collider collider = colliders[index];
                if (collider == null)
                {
                    continue;
                }

                if (IsLegacyFloorCollider(collider))
                {
                    if (!collider.enabled)
                    {
                        collider.enabled = true;
                        changed++;
                    }
                    preservedColliders++;
                }
                else if (collider.enabled)
                {
                    collider.enabled = false;
                    changed++;
                }
            }

            Rigidbody[] bodies = legacyRoot.GetComponentsInChildren<Rigidbody>(true);
            for (int index = 0; index < bodies.Length; index++)
            {
                Rigidbody body = bodies[index];
                if (body == null || HasEnabledCollider(body.gameObject))
                {
                    continue;
                }

                if (!body.isKinematic)
                {
                    body.isKinematic = true;
                    changed++;
                }
                if (body.useGravity)
                {
                    body.useGravity = false;
                    changed++;
                }
                if (body.detectCollisions)
                {
                    body.detectCollisions = false;
                    changed++;
                }
            }

            return changed;
        }

        public static int PrepareStageEnvironment(Scene environmentScene)
        {
            return PrepareStageEnvironment(environmentScene, out _, out _, out _);
        }

        /// <summary>
        /// Preserves the historical call shape while returning the number of active
        /// Stage01 colliders after runtime authoring. disabledColliders includes the
        /// preview actor and dormant Binder pool.
        /// </summary>
        public static int PrepareStageEnvironment(
            Scene environmentScene,
            out int disabledColliders,
            out int convertedMaterials)
        {
            return PrepareStageEnvironment(
                environmentScene,
                out disabledColliders,
                out _,
                out convertedMaterials);
        }

        /// <summary>
        /// Builds collision data for every active Stage01 FBX hierarchy without
        /// registering the whole office in PhysX at once. Structural walls,
        /// partitions and door openings remain continuously solid. Ordinary props
        /// receive one tight dormant BoxCollider and are enabled only around the
        /// player. Binder colliders are not even allocated until their spatial cell
        /// becomes relevant. The visual floor delegates to the proven expanded v007
        /// support floor and the preview character remains disabled.
        /// </summary>
        public static int PrepareStageEnvironment(
            Scene environmentScene,
            out int disabledColliders,
            out int generatedColliders,
            out int convertedMaterials)
        {
            if (!environmentScene.IsValid() || !environmentScene.isLoaded)
            {
                throw new ArgumentException(
                    "A valid loaded Stage01 Scene is required.",
                    nameof(environmentScene));
            }

            convertedMaterials = ConvertStageMaterialsForCurrentPipeline(
                environmentScene);

            DisableComponents(GetSceneComponents<Camera>(environmentScene));
            DisableComponents(GetSceneComponents<AudioListener>(environmentScene));
            DisableComponents(GetSceneComponents<AudioSource>(environmentScene));
            DisableComponents(GetSceneComponents<Canvas>(environmentScene));

            disabledColliders = 0;
            generatedColliders = 0;

            Animator[] animators = GetSceneComponents<Animator>(environmentScene);
            for (int index = 0; index < animators.Length; index++)
            {
                Animator animator = animators[index];
                if (animator == null)
                {
                    continue;
                }

                animator.enabled = false;
                DisableComponents(animator.GetComponentsInChildren<Renderer>(true));
                disabledColliders += DisableComponents(
                    animator.GetComponentsInChildren<Collider>(true));
                ConfigureExistingStageBodies(animator.gameObject, false);
            }

            int enabledColliders = 0;
            GameObject[] roots = environmentScene.GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                GameObject root = roots[index];
                if (root == null)
                {
                    continue;
                }

                bool previewHierarchy =
                    root.GetComponentInChildren<Animator>(true) != null;
                if (previewHierarchy || !root.activeInHierarchy)
                {
                    disabledColliders += DisableComponents(
                        root.GetComponentsInChildren<Collider>(true));
                    ConfigureExistingStageBodies(root, false);
                    continue;
                }

                if (IsStageFloorRoot(root))
                {
                    disabledColliders += DisableComponents(
                        root.GetComponentsInChildren<Collider>(true));
                    ConfigureExistingStageBodies(root, false);
                    continue;
                }

                bool binder = IsStageBinderRoot(root);
                if (binder)
                {
                    disabledColliders += DisableComponents(
                        root.GetComponentsInChildren<Collider>(true));
                    ConfigureExistingStageBodies(root, false);
                    continue;
                }

                if (!TryCalculateActiveLocalMeshBounds(
                        root,
                        out Bounds localBounds))
                {
                    disabledColliders += DisableComponents(
                        root.GetComponentsInChildren<Collider>(true));
                    ConfigureExistingStageBodies(root, false);
                    continue;
                }

                bool chair = IsStageChairRoot(root);
                if (!chair && RequiresDetailedStageCollision(root))
                {
                    int detailedColliderCount = ConfigureDetailedStageCollision(
                        root,
                        ref disabledColliders,
                        ref generatedColliders);
                    if (detailedColliderCount > 0)
                    {
                        enabledColliders += detailedColliderCount;
                        ConfigureExistingStageBodies(root, true);
                        continue;
                    }
                }

                BoxCollider rootCollider = root.GetComponent<BoxCollider>();
                if (rootCollider == null)
                {
                    rootCollider = root.AddComponent<BoxCollider>();
                    generatedColliders++;
                }

                ConfigureRootBoxCollider(rootCollider, localBounds);
                rootCollider.isTrigger = false;

                Collider[] hierarchyColliders =
                    root.GetComponentsInChildren<Collider>(true);
                for (int colliderIndex = 0;
                     colliderIndex < hierarchyColliders.Length;
                     colliderIndex++)
                {
                    Collider collider = hierarchyColliders[colliderIndex];
                    if (collider == null || collider == rootCollider)
                    {
                        continue;
                    }

                    if (collider.enabled)
                    {
                        collider.enabled = false;
                        disabledColliders++;
                    }
                }

                if (rootCollider.enabled)
                {
                    rootCollider.enabled = false;
                    disabledColliders++;
                }

                ConfigureExistingStageBodies(root, false);
            }

            Physics.SyncTransforms();
            return enabledColliders;
        }

        private void ConfigureStageGameplayProps(
            Scene gameplayScene,
            Scene environmentScene)
        {
            PrototypeInteractionController[] controllers =
                GetSceneComponents<PrototypeInteractionController>(gameplayScene);
            if (controllers.Length != 1 || controllers[0] == null)
            {
                throw new InvalidOperationException(
                    "Stage01 requires exactly one gameplay interaction controller, " +
                    $"found {controllers.Length}.");
            }

            interactionController = controllers[0];
            interactionPlayer = interactionController.transform;
            registeredInteractionTargets.Clear();
            promotedStageBinderTargets.Clear();
            stageBinderGrid.Clear();
            activeStageBinderCandidates.Clear();
            desiredStageBinderCandidates.Clear();
            stageStreamedColliderGrid.Clear();
            activeStageStreamedColliders.Clear();
            desiredStageStreamedColliders.Clear();
            stageVisualGrid.Clear();
            activeStageVisuals.Clear();
            desiredStageVisuals.Clear();
            activeStageBinderVisuals.Clear();
            desiredStageBinderVisuals.Clear();
            stageLightGrid.Clear();
            activeStageLights.Clear();
            desiredStageLights.Clear();
            stageDynamicPropBaselines.Clear();
            activeStreamedStageColliderCount = 0;
            activeStageVisualRootCount = 0;
            activeStageBinderVisualCount = 0;
            activeStageLightCount = 0;
            totalStreamedStageVisualRootCount = 0;
            totalStageLightCount = 0;
            stageChairTargetCount = 0;
            stageBinderRootCount = 0;
            promotedStageBinderCount = 0;
            activeDormantBinderColliderCount = 0;
            hasSpatialRefreshPosition = false;
            nextSpatialRefreshTime = float.NegativeInfinity;
            interactionController.ConfigureRuntimeReticleTargetResolver(
                ResolveRuntimeStageBinderTarget);

            PrototypeInteractionTarget[] authoredTargets =
                GetSceneComponents<PrototypeInteractionTarget>(gameplayScene);
            for (int index = 0; index < authoredTargets.Length; index++)
            {
                RegisterInteractionTarget(authoredTargets[index]);
            }

            GameObject[] roots = environmentScene.GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                GameObject root = roots[index];
                if (root == null || !root.activeInHierarchy)
                {
                    continue;
                }

                if (IsStageBinderRoot(root))
                {
                    if (!TryCalculateActiveLocalMeshBounds(
                            root,
                            out Bounds binderLocalBounds))
                    {
                        throw new InvalidOperationException(
                            $"Stage01 Binder has no valid visual bounds: {root.name}.");
                    }

                    Bounds binderWorldBounds = TransformLocalBoundsToWorld(
                        root.transform,
                        binderLocalBounds);
                    Renderer[] binderRenderers =
                        GetEnabledStageRenderers(root);
                    ConfigureStreamedRendererCost(
                        binderRenderers,
                        true);
                    var candidate = new StageBinderCandidate(
                        root,
                        binderLocalBounds,
                        binderWorldBounds,
                        binderRenderers);
                    AddCandidateToSpatialGrid(
                        stageBinderGrid,
                        binderWorldBounds,
                        candidate);
                    stageBinderRootCount++;
                    continue;
                }

                BoxCollider rootCollider = root.GetComponent<BoxCollider>();
                if (IsStageChairRoot(root))
                {
                    if (rootCollider == null)
                    {
                        throw new InvalidOperationException(
                            $"Stage01 Chair has no authored runtime BoxCollider: {root.name}.");
                    }

                    PrototypeInteractionTarget chair = ConfigureStageDynamicProp(
                        root,
                        rootCollider,
                        false);
                    RegisterInteractionTarget(chair);
                    stageChairTargetCount++;
                    continue;
                }

                if (rootCollider == null ||
                    RequiresDetailedStageCollision(root) ||
                    IsStageFloorRoot(root))
                {
                    continue;
                }

                Bounds streamedLocalBounds = new Bounds(
                    rootCollider.center,
                    rootCollider.size);
                Bounds streamedWorldBounds = TransformLocalBoundsToWorld(
                    root.transform,
                    streamedLocalBounds);
                var streamedCandidate = new StageStreamedColliderCandidate(
                    root,
                    rootCollider,
                    streamedWorldBounds);
                AddCandidateToSpatialGrid(
                    stageStreamedColliderGrid,
                    streamedWorldBounds,
                    streamedCandidate);

                Renderer[] streamedRenderers =
                    GetEnabledStageRenderers(root);
                if (streamedRenderers.Length > 0)
                {
                    float visualRadius = ResolveStageVisualActivationRadius(
                        streamedWorldBounds);
                    ConfigureStreamedRendererCost(
                        streamedRenderers,
                        visualRadius <= StageSmallPropVisualActivationRadius);
                    var visualCandidate = new StageVisualCandidate(
                        root,
                        streamedRenderers,
                        streamedWorldBounds,
                        visualRadius);
                    AddCandidateToSpatialGrid(
                        stageVisualGrid,
                        streamedWorldBounds,
                        visualCandidate);
                    totalStreamedStageVisualRootCount++;
                }
            }

            Light[] stageLights = GetSceneComponents<Light>(environmentScene);
            for (int index = 0; index < stageLights.Length; index++)
            {
                Light light = stageLights[index];
                if (light == null ||
                    !light.enabled ||
                    !light.gameObject.activeInHierarchy ||
                    light.type == LightType.Directional)
                {
                    continue;
                }

                light.shadows = LightShadows.None;
                float activationRadius = Mathf.Max(
                    StageLightActivationRadius,
                    light.range + 10f);
                var lightCandidate = new StageLightCandidate(
                    light,
                    light.transform.position,
                    activationRadius);
                AddCandidateToSpatialGrid(
                    stageLightGrid,
                    new Bounds(light.transform.position, Vector3.zero),
                    lightCandidate);
                totalStageLightCount++;
            }

            RefreshInteractionTargetRegistration();

            AuthoredEncounterResetController[] resetControllers =
                GetSceneComponents<AuthoredEncounterResetController>(gameplayScene);
            if (resetControllers.Length > 1)
            {
                throw new InvalidOperationException(
                    "Stage01 found more than one authored encounter Reset controller.");
            }

            if (encounterResetController != null)
            {
                encounterResetController.ResetPerformed -= HandleEncounterReset;
            }
            encounterResetController = resetControllers.Length == 1
                ? resetControllers[0]
                : null;
            if (encounterResetController != null)
            {
                encounterResetController.ResetPerformed += HandleEncounterReset;
            }
        }

        private PrototypeInteractionTarget ConfigureStageDynamicProp(
            GameObject root,
            BoxCollider rootCollider,
            bool binder)
        {
            if (root == null || rootCollider == null)
            {
                throw new ArgumentNullException(
                    root == null ? nameof(root) : nameof(rootCollider));
            }

            root.isStatic = false;
            rootCollider.enabled = true;
            rootCollider.isTrigger = false;

            Rigidbody body = root.GetComponent<Rigidbody>();
            if (body == null)
            {
                body = root.AddComponent<Rigidbody>();
            }

            body.isKinematic = true;
            body.useGravity = false;
            body.detectCollisions = true;
            body.mass = binder ? StageBinderMass : StageChairMass;
            body.constraints = RigidbodyConstraints.None;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode =
                CollisionDetectionMode.ContinuousSpeculative;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;

            PrototypeInteractionTarget target =
                root.GetComponent<PrototypeInteractionTarget>();
            if (target == null)
            {
                target = root.AddComponent<PrototypeInteractionTarget>();
            }

            string definitionId = binder ? "OBJ-P-L-001" : "OBJ-P-M-001";
            string runtimeKey = BuildStageRuntimeKey(
                binder ? StageBinderRuntimeKeyPrefix : StageChairRuntimeKeyPrefix,
                root);
            string displayName = binder ? "FILE BINDER" : "OFFICE CHAIR";
            PrototypeWeightClass weightClass = binder
                ? PrototypeWeightClass.Light
                : PrototypeWeightClass.Medium;
            PrototypeInteractionRole role = binder
                ? PrototypeInteractionRole.DirectBinder
                : PrototypeInteractionRole.DirectChair;
            target.Configure(
                definitionId,
                runtimeKey,
                displayName,
                weightClass,
                role,
                true,
                false,
                !binder,
                !binder);
            target.ConfigureStrapClampSocket(null);
            target.ConfigureDeferredAcquisitionContact(true);
            SetStageRenderersVisible(
                GetStageRenderers(root),
                true);

            stageDynamicPropBaselines.Add(new StageDynamicPropBaseline(
                root.transform,
                rootCollider,
                body,
                target,
                definitionId,
                runtimeKey,
                displayName,
                weightClass,
                role,
                body.mass,
                !binder,
                !binder,
                true));
            return target;
        }

        private void RegisterInteractionTarget(PrototypeInteractionTarget target)
        {
            if (target == null || registeredInteractionTargets.Contains(target))
            {
                return;
            }

            registeredInteractionTargets.Add(target);
        }

        private void RefreshInteractionTargetRegistration()
        {
            if (interactionController == null)
            {
                return;
            }

            for (int index = registeredInteractionTargets.Count - 1;
                 index >= 0;
                 index--)
            {
                if (registeredInteractionTargets[index] == null)
                {
                    registeredInteractionTargets.RemoveAt(index);
                }
            }

            registeredInteractionTargets.Sort((left, right) =>
                string.CompareOrdinal(
                    left != null ? left.RuntimeInstanceKey : string.Empty,
                    right != null ? right.RuntimeInstanceKey : string.Empty));
            interactionController.ConfigureShowcaseS3(
                interactionController.ShowcaseMobileAllyController,
                interactionController.AllyRelayController,
                registeredInteractionTargets.ToArray());
        }

        private void RefreshSpatialColliders(bool force)
        {
            if (interactionPlayer == null)
            {
                return;
            }

            float now = Time.unscaledTime;
            Vector3 playerPosition = interactionPlayer.position;
            bool playerMoved = !hasSpatialRefreshPosition ||
                Vector3.SqrMagnitude(
                    playerPosition - lastSpatialRefreshPosition) >=
                StageSpatialRefreshMovement * StageSpatialRefreshMovement;
            if (!force &&
                (now < nextSpatialRefreshTime || !playerMoved))
            {
                return;
            }

            bool changed = RefreshNearbyStageColliders(playerPosition);
            changed |= RefreshNearbyBinderColliders(playerPosition);
            RefreshStagePresentation(playerPosition);

            lastSpatialRefreshPosition = playerPosition;
            hasSpatialRefreshPosition = true;
            nextSpatialRefreshTime = now + StageSpatialRefreshInterval;
            UpdateStageColliderStatistics();
            if (changed)
            {
                Physics.SyncTransforms();
            }
        }

        private bool RefreshNearbyStageColliders(Vector3 playerPosition)
        {
            CollectSpatialCandidates(
                stageStreamedColliderGrid,
                playerPosition,
                StageStaticColliderActivationRadius,
                desiredStageStreamedColliders);

            float radiusSquared = StageStaticColliderActivationRadius *
                StageStaticColliderActivationRadius;
            stageStreamedRejectBuffer.Clear();
            foreach (StageStreamedColliderCandidate candidate in
                     desiredStageStreamedColliders)
            {
                if (candidate == null ||
                    candidate.Root == null ||
                    candidate.Collider == null ||
                    !candidate.Root.activeInHierarchy ||
                    SqrDistanceToBounds(playerPosition, candidate.WorldBounds) >
                        radiusSquared)
                {
                    stageStreamedRejectBuffer.Add(candidate);
                }
            }
            for (int index = 0;
                 index < stageStreamedRejectBuffer.Count;
                 index++)
            {
                desiredStageStreamedColliders.Remove(
                    stageStreamedRejectBuffer[index]);
            }

            bool changed = false;
            stageStreamedDeactivateBuffer.Clear();
            foreach (StageStreamedColliderCandidate candidate in
                     activeStageStreamedColliders)
            {
                if (!desiredStageStreamedColliders.Contains(candidate))
                {
                    stageStreamedDeactivateBuffer.Add(candidate);
                }
            }

            for (int index = 0;
                 index < stageStreamedDeactivateBuffer.Count;
                 index++)
            {
                StageStreamedColliderCandidate candidate =
                    stageStreamedDeactivateBuffer[index];
                if (candidate != null && candidate.Collider != null &&
                    candidate.Collider.enabled)
                {
                    candidate.Collider.enabled = false;
                    changed = true;
                }
                activeStageStreamedColliders.Remove(candidate);
            }

            foreach (StageStreamedColliderCandidate candidate in
                     desiredStageStreamedColliders)
            {
                if (candidate == null ||
                    candidate.Collider == null ||
                    !activeStageStreamedColliders.Add(candidate))
                {
                    continue;
                }

                if (!candidate.Collider.enabled)
                {
                    candidate.Collider.enabled = true;
                    changed = true;
                }
            }

            activeStreamedStageColliderCount =
                activeStageStreamedColliders.Count;
            return changed;
        }

        private bool RefreshNearbyBinderColliders(Vector3 playerPosition)
        {
            CollectSpatialCandidates(
                stageBinderGrid,
                playerPosition,
                StageBinderColliderActivationRadius,
                desiredStageBinderCandidates);

            float radiusSquared = StageBinderColliderActivationRadius *
                StageBinderColliderActivationRadius;
            stageBinderRejectBuffer.Clear();
            foreach (StageBinderCandidate candidate in
                     desiredStageBinderCandidates)
            {
                if (candidate == null ||
                    candidate.Root == null ||
                    candidate.Promoted ||
                    !candidate.Root.activeInHierarchy ||
                    SqrDistanceToBounds(playerPosition, candidate.WorldBounds) >
                        radiusSquared)
                {
                    stageBinderRejectBuffer.Add(candidate);
                }
            }
            for (int index = 0; index < stageBinderRejectBuffer.Count; index++)
            {
                desiredStageBinderCandidates.Remove(
                    stageBinderRejectBuffer[index]);
            }

            activeStageBinderCandidates.Clear();
            foreach (StageBinderCandidate candidate in desiredStageBinderCandidates)
            {
                if (candidate != null)
                {
                    activeStageBinderCandidates.Add(candidate);
                }
            }

            // Dormant Binders remain pure bounds records. A BoxCollider and
            // Rigidbody are allocated only for the one exact reticle candidate
            // promoted into the interaction system.
            activeDormantBinderColliderCount = 0;
            return false;
        }

        private void RefreshStagePresentation(Vector3 playerPosition)
        {
            RefreshStageBinderVisibility(playerPosition);
            RefreshStageVisualVisibility(playerPosition);
            RefreshStageLightVisibility(playerPosition);
        }

        private void RefreshStageBinderVisibility(Vector3 playerPosition)
        {
            float collectRadius = StageBinderVisualActivationRadius +
                StageVisualDeactivationHysteresis;
            CollectSpatialCandidates(
                stageBinderGrid,
                playerPosition,
                collectRadius,
                desiredStageBinderVisuals);

            float activationSquared = StageBinderVisualActivationRadius *
                StageBinderVisualActivationRadius;
            stageBinderVisualRejectBuffer.Clear();
            foreach (StageBinderCandidate candidate in desiredStageBinderVisuals)
            {
                if (candidate == null ||
                    candidate.Promoted ||
                    candidate.Root == null ||
                    !candidate.Root.activeInHierarchy ||
                    SqrDistanceToBounds(
                        playerPosition,
                        candidate.WorldBounds) > activationSquared)
                {
                    stageBinderVisualRejectBuffer.Add(candidate);
                }
            }
            for (int index = 0;
                 index < stageBinderVisualRejectBuffer.Count;
                 index++)
            {
                desiredStageBinderVisuals.Remove(
                    stageBinderVisualRejectBuffer[index]);
            }

            float deactivationRadius = StageBinderVisualActivationRadius +
                StageVisualDeactivationHysteresis;
            float deactivationSquared = deactivationRadius * deactivationRadius;
            stageBinderVisualDeactivateBuffer.Clear();
            foreach (StageBinderCandidate candidate in activeStageBinderVisuals)
            {
                if (candidate == null || candidate.Promoted)
                {
                    stageBinderVisualDeactivateBuffer.Add(candidate);
                    continue;
                }

                if (!desiredStageBinderVisuals.Contains(candidate) &&
                    SqrDistanceToBounds(
                        playerPosition,
                        candidate.WorldBounds) > deactivationSquared)
                {
                    stageBinderVisualDeactivateBuffer.Add(candidate);
                }
            }

            for (int index = 0;
                 index < stageBinderVisualDeactivateBuffer.Count;
                 index++)
            {
                StageBinderCandidate candidate =
                    stageBinderVisualDeactivateBuffer[index];
                if (candidate != null && !candidate.Promoted)
                {
                    candidate.SetVisible(false);
                }
                activeStageBinderVisuals.Remove(candidate);
            }

            foreach (StageBinderCandidate candidate in desiredStageBinderVisuals)
            {
                if (candidate == null || candidate.Promoted)
                {
                    continue;
                }

                candidate.SetVisible(true);
                activeStageBinderVisuals.Add(candidate);
            }

            activeStageBinderVisualCount =
                activeStageBinderVisuals.Count + promotedStageBinderCount;
        }

        private void RefreshStageVisualVisibility(Vector3 playerPosition)
        {
            float collectRadius = StageLargePropVisualActivationRadius +
                StageVisualDeactivationHysteresis;
            CollectSpatialCandidates(
                stageVisualGrid,
                playerPosition,
                collectRadius,
                desiredStageVisuals);

            stageVisualRejectBuffer.Clear();
            foreach (StageVisualCandidate candidate in desiredStageVisuals)
            {
                if (candidate == null ||
                    candidate.Root == null ||
                    !candidate.Root.activeInHierarchy ||
                    SqrDistanceToBounds(
                        playerPosition,
                        candidate.WorldBounds) >
                        candidate.ActivationRadius * candidate.ActivationRadius)
                {
                    stageVisualRejectBuffer.Add(candidate);
                }
            }
            for (int index = 0; index < stageVisualRejectBuffer.Count; index++)
            {
                desiredStageVisuals.Remove(stageVisualRejectBuffer[index]);
            }

            stageVisualDeactivateBuffer.Clear();
            foreach (StageVisualCandidate candidate in activeStageVisuals)
            {
                float deactivationRadius = candidate != null
                    ? candidate.ActivationRadius +
                      StageVisualDeactivationHysteresis
                    : 0f;
                if (candidate == null ||
                    candidate.Root == null ||
                    !candidate.Root.activeInHierarchy ||
                    (!desiredStageVisuals.Contains(candidate) &&
                     SqrDistanceToBounds(
                         playerPosition,
                         candidate.WorldBounds) >
                         deactivationRadius * deactivationRadius))
                {
                    stageVisualDeactivateBuffer.Add(candidate);
                }
            }

            for (int index = 0;
                 index < stageVisualDeactivateBuffer.Count;
                 index++)
            {
                StageVisualCandidate candidate =
                    stageVisualDeactivateBuffer[index];
                candidate?.SetVisible(false);
                activeStageVisuals.Remove(candidate);
            }

            foreach (StageVisualCandidate candidate in desiredStageVisuals)
            {
                if (candidate == null)
                {
                    continue;
                }

                candidate.SetVisible(true);
                activeStageVisuals.Add(candidate);
            }

            activeStageVisualRootCount = activeStageVisuals.Count;
        }

        private void RefreshStageLightVisibility(Vector3 playerPosition)
        {
            float collectRadius = StageLightActivationRadius +
                StageVisualDeactivationHysteresis;
            CollectSpatialCandidates(
                stageLightGrid,
                playerPosition,
                collectRadius,
                desiredStageLights);

            stageLightRejectBuffer.Clear();
            foreach (StageLightCandidate candidate in desiredStageLights)
            {
                if (candidate == null ||
                    candidate.Light == null ||
                    !candidate.Light.gameObject.activeInHierarchy ||
                    Vector3.SqrMagnitude(
                        playerPosition - candidate.Position) >
                        candidate.ActivationRadius * candidate.ActivationRadius)
                {
                    stageLightRejectBuffer.Add(candidate);
                }
            }
            for (int index = 0; index < stageLightRejectBuffer.Count; index++)
            {
                desiredStageLights.Remove(stageLightRejectBuffer[index]);
            }

            stageLightDeactivateBuffer.Clear();
            foreach (StageLightCandidate candidate in activeStageLights)
            {
                float deactivationRadius = candidate != null
                    ? candidate.ActivationRadius +
                      StageVisualDeactivationHysteresis
                    : 0f;
                if (candidate == null ||
                    candidate.Light == null ||
                    (!desiredStageLights.Contains(candidate) &&
                     Vector3.SqrMagnitude(
                         playerPosition - candidate.Position) >
                         deactivationRadius * deactivationRadius))
                {
                    stageLightDeactivateBuffer.Add(candidate);
                }
            }

            for (int index = 0;
                 index < stageLightDeactivateBuffer.Count;
                 index++)
            {
                StageLightCandidate candidate =
                    stageLightDeactivateBuffer[index];
                candidate?.SetEnabled(false);
                activeStageLights.Remove(candidate);
            }

            foreach (StageLightCandidate candidate in desiredStageLights)
            {
                if (candidate == null)
                {
                    continue;
                }

                candidate.SetEnabled(true);
                activeStageLights.Add(candidate);
            }

            activeStageLightCount = activeStageLights.Count;
        }

        private BoxCollider EnsureBinderCollider(StageBinderCandidate candidate)
        {
            if (candidate == null || candidate.Root == null)
            {
                return null;
            }

            if (candidate.Collider != null)
            {
                return candidate.Collider;
            }

            BoxCollider collider = candidate.Root.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = candidate.Root.AddComponent<BoxCollider>();
                generatedFinalMapColliderCount++;
                totalFinalMapColliderCount++;
            }

            ConfigureRootBoxCollider(collider, candidate.LocalBounds);
            collider.isTrigger = false;
            collider.enabled = true;
            candidate.Collider = collider;
            return collider;
        }

        public PrototypeInteractionTarget ResolveRuntimeStageBinderTarget(
            Ray reticleRay,
            float requestedInteractionRange)
        {
            if (interactionController == null ||
                interactionPlayer == null ||
                (activeStageBinderCandidates.Count == 0 &&
                 promotedStageBinderTargets.Count == 0) ||
                reticleRay.direction.sqrMagnitude <= 0.0001f)
            {
                return null;
            }

            float interactionRange = Mathf.Max(0.5f, requestedInteractionRange);
            float maximumRayDistance = Mathf.Min(
                StageBinderReticleMaximumDistance,
                interactionRange + Vector3.Distance(
                    reticleRay.origin,
                    interactionPlayer.position + Vector3.up) + 1f);
            Vector3 interactionOrigin = interactionPlayer.position + Vector3.up;
            float interactionRangeSquared = interactionRange * interactionRange;
            StageBinderCandidate nearestBinder = null;
            PrototypeInteractionTarget nearestPromotedTarget = null;
            Bounds nearestBounds = default;
            float nearestDistance = float.PositiveInfinity;

            PrototypeInteractionTarget carriedTarget =
                interactionController.InteractionOwner?.CarriedTarget;
            for (int index = promotedStageBinderTargets.Count - 1;
                 index >= 0;
                 index--)
            {
                PrototypeInteractionTarget target =
                    promotedStageBinderTargets[index];
                if (target == null)
                {
                    promotedStageBinderTargets.RemoveAt(index);
                    continue;
                }
                if (target == carriedTarget ||
                    !target.gameObject.activeInHierarchy ||
                    !target.IsScoopEligible)
                {
                    continue;
                }

                Collider collider = target.GetComponent<Collider>();
                if (collider == null || !collider.enabled)
                {
                    continue;
                }

                Bounds candidateBounds = collider.bounds;
                if (!candidateBounds.IntersectRay(
                        reticleRay,
                        out float rayDistance) ||
                    rayDistance < 0f ||
                    rayDistance > maximumRayDistance ||
                    SqrDistanceToBounds(
                        interactionOrigin,
                        candidateBounds) > interactionRangeSquared)
                {
                    continue;
                }

                bool closer = rayDistance < nearestDistance - 0.0001f;
                bool sameDistance =
                    Mathf.Abs(rayDistance - nearestDistance) <= 0.0001f;
                bool stableTieWinner = sameDistance &&
                    (nearestPromotedTarget == null ||
                     string.CompareOrdinal(
                         target.RuntimeInstanceKey,
                         nearestPromotedTarget.RuntimeInstanceKey) < 0);
                if (closer || stableTieWinner)
                {
                    nearestDistance = rayDistance;
                    nearestPromotedTarget = target;
                    nearestBinder = null;
                    nearestBounds = candidateBounds;
                }
            }

            foreach (StageBinderCandidate candidate in activeStageBinderCandidates)
            {
                if (candidate == null ||
                    candidate.Promoted ||
                    candidate.Root == null ||
                    !candidate.Root.activeInHierarchy ||
                    !candidate.WorldBounds.IntersectRay(
                        reticleRay,
                        out float rayDistance) ||
                    rayDistance < 0f ||
                    rayDistance > maximumRayDistance ||
                    SqrDistanceToBounds(
                        interactionOrigin,
                        candidate.WorldBounds) > interactionRangeSquared)
                {
                    continue;
                }

                bool closer = rayDistance < nearestDistance - 0.0001f;
                bool sameDistance =
                    Mathf.Abs(rayDistance - nearestDistance) <= 0.0001f;
                bool stableTieWinner = sameDistance &&
                    (nearestBinder == null ||
                     candidate.Root.transform.GetSiblingIndex() <
                         nearestBinder.Root.transform.GetSiblingIndex());
                if (closer || stableTieWinner)
                {
                    nearestDistance = rayDistance;
                    nearestBinder = candidate;
                    nearestPromotedTarget = null;
                    nearestBounds = candidate.WorldBounds;
                }
            }

            if ((nearestBinder == null && nearestPromotedTarget == null) ||
                IsStageBinderOccluded(
                    reticleRay,
                    nearestDistance,
                    nearestBounds))
            {
                return null;
            }

            return nearestPromotedTarget != null
                ? nearestPromotedTarget
                : PromoteStageBinder(nearestBinder);
        }

        private bool IsStageBinderOccluded(
            Ray reticleRay,
            float binderDistance,
            Bounds binderBounds)
        {
            if (binderDistance <= 0f)
            {
                return false;
            }

            int hitCount = Physics.RaycastNonAlloc(
                reticleRay.origin,
                reticleRay.direction.normalized,
                stageBinderReticleHits,
                binderDistance + 0.03f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            if (hitCount >= StageBinderReticleHitCapacity)
            {
                return true;
            }

            float nearestBlockingDistance = float.PositiveInfinity;
            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = stageBinderReticleHits[index];
                Collider collider = hit.collider;
                if (collider == null ||
                    collider.transform == interactionPlayer ||
                    collider.transform.IsChildOf(interactionPlayer) ||
                    collider.bounds.Intersects(binderBounds))
                {
                    continue;
                }

                nearestBlockingDistance = Mathf.Min(
                    nearestBlockingDistance,
                    hit.distance);
            }

            return nearestBlockingDistance < binderDistance - 0.02f;
        }

        private PrototypeInteractionTarget PromoteStageBinder(
            StageBinderCandidate binder)
        {
            if (binder == null ||
                binder.Promoted ||
                binder.Root == null)
            {
                return null;
            }

            BoxCollider collider = EnsureBinderCollider(binder);
            if (collider == null)
            {
                return null;
            }

            PrototypeInteractionTarget target = ConfigureStageDynamicProp(
                binder.Root,
                collider,
                true);
            binder.Promoted = true;
            activeStageBinderCandidates.Remove(binder);
            desiredStageBinderCandidates.Remove(binder);
            activeStageBinderVisuals.Remove(binder);
            desiredStageBinderVisuals.Remove(binder);
            binder.SetVisible(true);
            promotedStageBinderCount++;
            promotedStageBinderTargets.Add(target);
            RegisterInteractionTarget(target);
            RefreshInteractionTargetRegistration();
            RefreshSpatialColliders(true);
            Physics.SyncTransforms();
            return target;
        }

        private void HandleEncounterReset(EncounterResetCause cause)
        {
            _ = cause;
            for (int index = 0; index < stageDynamicPropBaselines.Count; index++)
            {
                stageDynamicPropBaselines[index]?.Restore();
            }

            Physics.SyncTransforms();
            RefreshSpatialColliders(true);
        }

        private void UpdateStageColliderStatistics()
        {
            enabledFinalMapColliderCount = stageStaticColliderCount +
                activeStreamedStageColliderCount +
                stageChairTargetCount +
                activeDormantBinderColliderCount +
                promotedStageBinderCount;
            disabledFinalMapColliderCount = Mathf.Max(
                0,
                totalFinalMapColliderCount - enabledFinalMapColliderCount);
        }

        private static bool IsStageFloorRoot(GameObject root)
        {
            return root != null && string.Equals(
                root.name,
                EnvironmentFloorObjectName,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsStageChairRoot(GameObject root)
        {
            return root != null && root.name.StartsWith(
                StageChairNamePrefix,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsStageBinderRoot(GameObject root)
        {
            return root != null && root.name.StartsWith(
                StageBinderNamePrefix,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool RequiresDetailedStageCollision(GameObject root)
        {
            if (root == null)
            {
                return false;
            }

            string name = root.name;
            return name.IndexOf("Wall", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("Partition", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("Door", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("pillar", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("ceiling", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int ConfigureDetailedStageCollision(
            GameObject root,
            ref int disabledColliders,
            ref int generatedColliders)
        {
            Collider[] existingColliders =
                root.GetComponentsInChildren<Collider>(true);
            for (int index = 0; index < existingColliders.Length; index++)
            {
                Collider existing = existingColliders[index];
                if (existing != null && existing.enabled)
                {
                    existing.enabled = false;
                    disabledColliders++;
                }
            }

            int enabled = 0;
            MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
            for (int index = 0; index < meshFilters.Length; index++)
            {
                MeshFilter meshFilter = meshFilters[index];
                if (meshFilter == null ||
                    meshFilter.sharedMesh == null ||
                    !meshFilter.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Renderer renderer = meshFilter.GetComponent<Renderer>();
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                MeshCollider meshCollider =
                    meshFilter.GetComponent<MeshCollider>();
                if (meshCollider == null)
                {
                    meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                    generatedColliders++;
                }

                meshCollider.sharedMesh = meshFilter.sharedMesh;
                meshCollider.convex = false;
                meshCollider.isTrigger = false;
                meshCollider.enabled = true;
                enabled++;
            }

            return enabled;
        }

        private static bool TryCalculateActiveLocalMeshBounds(
            GameObject root,
            out Bounds bounds)
        {
            if (root == null)
            {
                bounds = default;
                return false;
            }

            Vector3 localMin = new Vector3(
                float.PositiveInfinity,
                float.PositiveInfinity,
                float.PositiveInfinity);
            Vector3 localMax = new Vector3(
                float.NegativeInfinity,
                float.NegativeInfinity,
                float.NegativeInfinity);
            bool hasBounds = false;

            MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
            for (int index = 0; index < meshFilters.Length; index++)
            {
                MeshFilter meshFilter = meshFilters[index];
                if (meshFilter == null ||
                    meshFilter.sharedMesh == null ||
                    !meshFilter.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Renderer renderer = meshFilter.GetComponent<Renderer>();
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                EncapsulateBoundsInRootSpace(
                    meshFilter.sharedMesh.bounds,
                    meshFilter.transform,
                    root.transform,
                    ref localMin,
                    ref localMax,
                    ref hasBounds);
            }

            if (!hasBounds)
            {
                Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
                for (int index = 0; index < renderers.Length; index++)
                {
                    Renderer renderer = renderers[index];
                    if (renderer == null ||
                        !renderer.enabled ||
                        !renderer.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    EncapsulateWorldBoundsInRootSpace(
                        renderer.bounds,
                        root.transform,
                        ref localMin,
                        ref localMax,
                        ref hasBounds);
                }
            }

            if (!hasBounds || !IsFinite(localMin) || !IsFinite(localMax))
            {
                bounds = default;
                return false;
            }

            Vector3 localSize = localMax - localMin;
            Vector3 worldScale = root.transform.lossyScale;
            localSize.x = Mathf.Max(
                localSize.x,
                StageColliderMinimumWorldThickness /
                Mathf.Max(0.0001f, Mathf.Abs(worldScale.x)));
            localSize.y = Mathf.Max(
                localSize.y,
                StageColliderMinimumWorldThickness /
                Mathf.Max(0.0001f, Mathf.Abs(worldScale.y)));
            localSize.z = Mathf.Max(
                localSize.z,
                StageColliderMinimumWorldThickness /
                Mathf.Max(0.0001f, Mathf.Abs(worldScale.z)));
            if (!IsFinite(localSize) ||
                localSize.x <= 0f ||
                localSize.y <= 0f ||
                localSize.z <= 0f)
            {
                bounds = default;
                return false;
            }

            bounds = new Bounds((localMin + localMax) * 0.5f, localSize);
            return true;
        }

        private static void EncapsulateBoundsInRootSpace(
            Bounds sourceBounds,
            Transform sourceTransform,
            Transform rootTransform,
            ref Vector3 localMin,
            ref Vector3 localMax,
            ref bool hasBounds)
        {
            Vector3 sourceMin = sourceBounds.min;
            Vector3 sourceMax = sourceBounds.max;
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        Vector3 sourceCorner = new Vector3(
                            x == 0 ? sourceMin.x : sourceMax.x,
                            y == 0 ? sourceMin.y : sourceMax.y,
                            z == 0 ? sourceMin.z : sourceMax.z);
                        Vector3 worldCorner =
                            sourceTransform.TransformPoint(sourceCorner);
                        Vector3 rootLocalCorner =
                            rootTransform.InverseTransformPoint(worldCorner);
                        if (!IsFinite(rootLocalCorner))
                        {
                            continue;
                        }

                        localMin = Vector3.Min(localMin, rootLocalCorner);
                        localMax = Vector3.Max(localMax, rootLocalCorner);
                        hasBounds = true;
                    }
                }
            }
        }

        private static void EncapsulateWorldBoundsInRootSpace(
            Bounds worldBounds,
            Transform rootTransform,
            ref Vector3 localMin,
            ref Vector3 localMax,
            ref bool hasBounds)
        {
            Vector3 worldMin = worldBounds.min;
            Vector3 worldMax = worldBounds.max;
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        Vector3 worldCorner = new Vector3(
                            x == 0 ? worldMin.x : worldMax.x,
                            y == 0 ? worldMin.y : worldMax.y,
                            z == 0 ? worldMin.z : worldMax.z);
                        Vector3 rootLocalCorner =
                            rootTransform.InverseTransformPoint(worldCorner);
                        if (!IsFinite(rootLocalCorner))
                        {
                            continue;
                        }

                        localMin = Vector3.Min(localMin, rootLocalCorner);
                        localMax = Vector3.Max(localMax, rootLocalCorner);
                        hasBounds = true;
                    }
                }
            }
        }

        private static void ConfigureRootBoxCollider(
            BoxCollider rootCollider,
            Bounds localBounds)
        {
            if (rootCollider == null ||
                !IsFinite(localBounds.center) ||
                !IsFinite(localBounds.size) ||
                localBounds.size.x <= 0f ||
                localBounds.size.y <= 0f ||
                localBounds.size.z <= 0f)
            {
                throw new InvalidOperationException(
                    "Stage01 could not derive a valid root BoxCollider.");
            }

            rootCollider.center = localBounds.center;
            rootCollider.size = localBounds.size;
        }

        private static void ConfigureExistingStageBodies(
            GameObject root,
            bool detectCollisions)
        {
            if (root == null)
            {
                return;
            }

            Rigidbody[] bodies = root.GetComponentsInChildren<Rigidbody>(true);
            for (int index = 0; index < bodies.Length; index++)
            {
                Rigidbody body = bodies[index];
                if (body == null)
                {
                    continue;
                }

                body.isKinematic = true;
                body.useGravity = false;
                body.detectCollisions = detectCollisions;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }

        private static Renderer[] GetEnabledStageRenderers(GameObject root)
        {
            if (root == null)
            {
                return Array.Empty<Renderer>();
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            var result = new List<Renderer>(renderers.Length);
            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer != null &&
                    renderer.enabled &&
                    renderer.gameObject.activeInHierarchy)
                {
                    result.Add(renderer);
                }
            }
            return result.ToArray();
        }

        private static Renderer[] GetStageRenderers(GameObject root)
        {
            if (root == null)
            {
                return Array.Empty<Renderer>();
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            var result = new List<Renderer>(renderers.Length);
            for (int index = 0; index < renderers.Length; index++)
            {
                if (renderers[index] != null)
                {
                    result.Add(renderers[index]);
                }
            }
            return result.ToArray();
        }

        private static void SetStageRenderersVisible(
            Renderer[] renderers,
            bool visible)
        {
            if (renderers == null)
            {
                return;
            }

            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer != null && renderer.enabled != visible)
                {
                    renderer.enabled = visible;
                }
            }
        }

        private static void ConfigureStreamedRendererCost(
            Renderer[] renderers,
            bool lightweight)
        {
            if (renderers == null)
            {
                return;
            }

            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer == null)
                {
                    continue;
                }

                renderer.motionVectorGenerationMode =
                    MotionVectorGenerationMode.ForceNoMotion;
                renderer.allowOcclusionWhenDynamic = true;
                if (lightweight)
                {
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    renderer.lightProbeUsage = LightProbeUsage.Off;
                    renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                }

                Material[] materials = renderer.sharedMaterials;
                for (int materialIndex = 0;
                     materialIndex < materials.Length;
                     materialIndex++)
                {
                    Material material = materials[materialIndex];
                    if (material != null &&
                        RuntimeStageInstancedMaterials.Add(material))
                    {
                        material.enableInstancing = true;
                    }
                }
            }
        }

        private static float ResolveStageVisualActivationRadius(
            Bounds worldBounds)
        {
            Vector3 size = worldBounds.size;
            float largestDimension = Mathf.Max(size.x, size.y, size.z);
            if (largestDimension <= 1.5f)
            {
                return StageSmallPropVisualActivationRadius;
            }
            if (largestDimension <= 4.5f)
            {
                return StageMediumPropVisualActivationRadius;
            }
            return StageLargePropVisualActivationRadius;
        }

        private static Bounds TransformLocalBoundsToWorld(
            Transform transform,
            Bounds localBounds)
        {
            if (transform == null ||
                !IsFinite(localBounds.center) ||
                !IsFinite(localBounds.size))
            {
                throw new InvalidOperationException(
                    "Stage01 could not transform invalid local bounds.");
            }

            Vector3 worldMin = new Vector3(
                float.PositiveInfinity,
                float.PositiveInfinity,
                float.PositiveInfinity);
            Vector3 worldMax = new Vector3(
                float.NegativeInfinity,
                float.NegativeInfinity,
                float.NegativeInfinity);
            Vector3 localMin = localBounds.min;
            Vector3 localMax = localBounds.max;
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        Vector3 corner = transform.TransformPoint(new Vector3(
                            x == 0 ? localMin.x : localMax.x,
                            y == 0 ? localMin.y : localMax.y,
                            z == 0 ? localMin.z : localMax.z));
                        worldMin = Vector3.Min(worldMin, corner);
                        worldMax = Vector3.Max(worldMax, corner);
                    }
                }
            }

            if (!IsFinite(worldMin) || !IsFinite(worldMax))
            {
                throw new InvalidOperationException(
                    "Stage01 could not derive finite world bounds.");
            }

            var result = new Bounds();
            result.SetMinMax(worldMin, worldMax);
            return result;
        }

        private static void AddCandidateToSpatialGrid<T>(
            Dictionary<Vector2Int, List<T>> grid,
            Bounds worldBounds,
            T candidate)
            where T : class
        {
            if (grid == null || candidate == null)
            {
                return;
            }

            int minX = Mathf.FloorToInt(worldBounds.min.x / StageSpatialCellSize);
            int maxX = Mathf.FloorToInt(worldBounds.max.x / StageSpatialCellSize);
            int minZ = Mathf.FloorToInt(worldBounds.min.z / StageSpatialCellSize);
            int maxZ = Mathf.FloorToInt(worldBounds.max.z / StageSpatialCellSize);
            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    var key = new Vector2Int(x, z);
                    if (!grid.TryGetValue(key, out List<T> cell))
                    {
                        cell = new List<T>();
                        grid.Add(key, cell);
                    }
                    cell.Add(candidate);
                }
            }
        }

        private static void CollectSpatialCandidates<T>(
            Dictionary<Vector2Int, List<T>> grid,
            Vector3 center,
            float radius,
            HashSet<T> result)
            where T : class
        {
            result.Clear();
            if (grid == null || grid.Count == 0)
            {
                return;
            }

            float clampedRadius = Mathf.Max(0f, radius);
            int minX = Mathf.FloorToInt(
                (center.x - clampedRadius) / StageSpatialCellSize);
            int maxX = Mathf.FloorToInt(
                (center.x + clampedRadius) / StageSpatialCellSize);
            int minZ = Mathf.FloorToInt(
                (center.z - clampedRadius) / StageSpatialCellSize);
            int maxZ = Mathf.FloorToInt(
                (center.z + clampedRadius) / StageSpatialCellSize);
            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    if (!grid.TryGetValue(
                            new Vector2Int(x, z),
                            out List<T> cell))
                    {
                        continue;
                    }

                    for (int index = 0; index < cell.Count; index++)
                    {
                        T candidate = cell[index];
                        if (candidate != null)
                        {
                            result.Add(candidate);
                        }
                    }
                }
            }
        }

        private static float SqrDistanceToBounds(
            Vector3 point,
            Bounds bounds)
        {
            Vector3 closest = bounds.ClosestPoint(point);
            return Vector3.SqrMagnitude(point - closest);
        }

        private static string BuildStageRuntimeKey(
            string prefix,
            GameObject root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            return prefix + root.transform.GetSiblingIndex().ToString("D5");
        }

        private sealed class StageBinderCandidate
        {
            public StageBinderCandidate(
                GameObject root,
                Bounds localBounds,
                Bounds worldBounds,
                Renderer[] renderers)
            {
                Root = root;
                LocalBounds = localBounds;
                WorldBounds = worldBounds;
                Renderers = renderers ?? Array.Empty<Renderer>();
                SetVisible(false);
            }

            public GameObject Root { get; }

            public Bounds LocalBounds { get; }

            public Bounds WorldBounds { get; }

            public Renderer[] Renderers { get; }

            public BoxCollider Collider { get; set; }

            public bool Promoted { get; set; }

            public void SetVisible(bool visible)
            {
                SetStageRenderersVisible(Renderers, visible);
            }
        }

        private sealed class StageStreamedColliderCandidate
        {
            public StageStreamedColliderCandidate(
                GameObject root,
                BoxCollider collider,
                Bounds worldBounds)
            {
                Root = root;
                Collider = collider;
                WorldBounds = worldBounds;
            }

            public GameObject Root { get; }

            public BoxCollider Collider { get; }

            public Bounds WorldBounds { get; }
        }

        private sealed class StageVisualCandidate
        {
            public StageVisualCandidate(
                GameObject root,
                Renderer[] renderers,
                Bounds worldBounds,
                float activationRadius)
            {
                Root = root;
                Renderers = renderers ?? Array.Empty<Renderer>();
                WorldBounds = worldBounds;
                ActivationRadius = Mathf.Max(1f, activationRadius);
                SetVisible(false);
            }

            public GameObject Root { get; }

            public Renderer[] Renderers { get; }

            public Bounds WorldBounds { get; }

            public float ActivationRadius { get; }

            public void SetVisible(bool visible)
            {
                SetStageRenderersVisible(Renderers, visible);
            }
        }

        private sealed class StageLightCandidate
        {
            public StageLightCandidate(
                Light light,
                Vector3 position,
                float activationRadius)
            {
                Light = light;
                Position = position;
                ActivationRadius = Mathf.Max(1f, activationRadius);
                SetEnabled(false);
            }

            public Light Light { get; }

            public Vector3 Position { get; }

            public float ActivationRadius { get; }

            public void SetEnabled(bool enabled)
            {
                if (Light != null)
                {
                    Light.enabled = enabled;
                }
            }
        }

        private sealed class StageDynamicPropBaseline
        {
            private readonly Transform root;
            private readonly BoxCollider collider;
            private readonly Rigidbody body;
            private readonly PrototypeInteractionTarget target;
            private readonly string definitionId;
            private readonly string runtimeKey;
            private readonly string displayName;
            private readonly PrototypeWeightClass weightClass;
            private readonly PrototypeInteractionRole role;
            private readonly float mass;
            private readonly bool strapEligible;
            private readonly bool hasAnchor;
            private readonly bool deferredAcquisitionContact;
            private readonly Vector3 position;
            private readonly Quaternion rotation;
            private readonly Vector3 localScale;

            public StageDynamicPropBaseline(
                Transform rootTransform,
                BoxCollider rootCollider,
                Rigidbody rootBody,
                PrototypeInteractionTarget interactionTarget,
                string contentDefinitionId,
                string instanceKey,
                string readableName,
                PrototypeWeightClass assignedWeightClass,
                PrototypeInteractionRole assignedRole,
                float assignedMass,
                bool canStrap,
                bool validAnchor,
                bool allowDeferredContact)
            {
                root = rootTransform;
                collider = rootCollider;
                body = rootBody;
                target = interactionTarget;
                definitionId = contentDefinitionId;
                runtimeKey = instanceKey;
                displayName = readableName;
                weightClass = assignedWeightClass;
                role = assignedRole;
                mass = assignedMass;
                strapEligible = canStrap;
                hasAnchor = validAnchor;
                deferredAcquisitionContact = allowDeferredContact;
                position = rootTransform != null
                    ? rootTransform.position
                    : Vector3.zero;
                rotation = rootTransform != null
                    ? rootTransform.rotation
                    : Quaternion.identity;
                localScale = rootTransform != null
                    ? rootTransform.localScale
                    : Vector3.one;
            }

            public void Restore()
            {
                if (root == null ||
                    collider == null ||
                    body == null ||
                    target == null)
                {
                    return;
                }

                root.gameObject.SetActive(true);
                root.gameObject.isStatic = false;
                body.isKinematic = true;
                body.useGravity = false;
                body.detectCollisions = true;
                body.mass = mass;
                body.constraints = RigidbodyConstraints.None;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode =
                    CollisionDetectionMode.ContinuousSpeculative;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                root.SetPositionAndRotation(position, rotation);
                root.localScale = localScale;
                body.position = position;
                body.rotation = rotation;
                collider.enabled = true;
                collider.isTrigger = false;
                target.Configure(
                    definitionId,
                    runtimeKey,
                    displayName,
                    weightClass,
                    role,
                    true,
                    false,
                    strapEligible,
                    hasAnchor);
                target.ConfigureStrapClampSocket(null);
                target.ConfigureDeferredAcquisitionContact(
                    deferredAcquisitionContact);
                SetStageRenderersVisible(
                    GetStageRenderers(root.gameObject),
                    true);
            }
        }

        public static Vector3 RecenterStageEnvironment(Scene environmentScene)
        {
            if (!environmentScene.IsValid() || !environmentScene.isLoaded)
            {
                throw new ArgumentException(
                    "A valid loaded Stage01 Scene is required.",
                    nameof(environmentScene));
            }

            Transform floor = FindStageFloor(environmentScene);
            if (floor == null)
            {
                throw new InvalidOperationException(
                    $"Pinned Stage01 floor object was not found: {EnvironmentFloorObjectName}.");
            }

            Vector3 authoredOrigin = floor.position;
            if (!IsFinite(authoredOrigin) ||
                Mathf.Abs(authoredOrigin.x) > 200f ||
                Mathf.Abs(authoredOrigin.y) > 50f ||
                Mathf.Abs(authoredOrigin.z) > 200f)
            {
                throw new InvalidOperationException(
                    $"Pinned Stage01 floor origin was invalid: {authoredOrigin}.");
            }

            Vector3 offset = -authoredOrigin;
            GameObject[] roots = environmentScene.GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                GameObject root = roots[index];
                if (root != null)
                {
                    root.transform.position += offset;
                }
            }

            return offset;
        }

        /// <summary>
        /// Expands the one preserved v007 support floor so its horizontal bounds
        /// cover every active Stage01 renderer plus a safety margin. The existing
        /// support top and thickness are preserved; only X/Z coverage changes.
        /// </summary>
        public static Bounds ExpandLegacyFloorToCoverStage(
            Collider legacyFloor,
            Scene environmentScene,
            float horizontalMargin = LegacyFloorCoverageMargin)
        {
            if (legacyFloor == null)
            {
                throw new ArgumentNullException(nameof(legacyFloor));
            }
            if (!(legacyFloor is BoxCollider boxCollider))
            {
                throw new InvalidOperationException(
                    $"{LegacyFloorColliderName} must remain a BoxCollider.");
            }
            if (!environmentScene.IsValid() || !environmentScene.isLoaded)
            {
                throw new ArgumentException(
                    "A valid loaded Stage01 Scene is required.",
                    nameof(environmentScene));
            }
            if (!TryCalculateActiveSceneBounds(environmentScene, out Bounds stageBounds))
            {
                throw new InvalidOperationException(
                    "Stage01 had no active renderer bounds for support-floor sizing.");
            }

            float margin = Mathf.Max(0f, horizontalMargin);
            Bounds currentBounds = boxCollider.bounds;
            float supportTop = currentBounds.max.y;
            float supportThickness = Mathf.Max(0.1f, currentBounds.size.y);

            float targetMinX = Mathf.Min(
                currentBounds.min.x,
                stageBounds.min.x - margin);
            float targetMaxX = Mathf.Max(
                currentBounds.max.x,
                stageBounds.max.x + margin);
            float targetMinZ = Mathf.Min(
                currentBounds.min.z,
                stageBounds.min.z - margin);
            float targetMaxZ = Mathf.Max(
                currentBounds.max.z,
                stageBounds.max.z + margin);

            var targetWorldBounds = new Bounds();
            targetWorldBounds.SetMinMax(
                new Vector3(
                    targetMinX,
                    supportTop - supportThickness,
                    targetMinZ),
                new Vector3(
                    targetMaxX,
                    supportTop,
                    targetMaxZ));

            SetBoxColliderWorldBounds(boxCollider, targetWorldBounds);
            boxCollider.enabled = true;
            Physics.SyncTransforms();

            Bounds expandedBounds = boxCollider.bounds;
            const float CoverageTolerance = 0.05f;
            if (expandedBounds.min.x > stageBounds.min.x - margin + CoverageTolerance ||
                expandedBounds.max.x < stageBounds.max.x + margin - CoverageTolerance ||
                expandedBounds.min.z > stageBounds.min.z - margin + CoverageTolerance ||
                expandedBounds.max.z < stageBounds.max.z + margin - CoverageTolerance)
            {
                throw new InvalidOperationException(
                    "Expanded v007 support floor did not cover the Stage01 visual bounds. " +
                    $"stage={stageBounds}; support={expandedBounds}; margin={margin:0.###}.");
            }

            return expandedBounds;
        }

        public static Material CreateBuiltInCompatibleMaterial(Material source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            Shader standardShader = Shader.Find("Standard");
            if (standardShader == null)
            {
                throw new InvalidOperationException(
                    "Built-in Standard shader is unavailable for Stage01 material conversion.");
            }

            var converted = new Material(standardShader)
            {
                name = source.name + "_Stage01BuiltInRuntime",
                hideFlags = HideFlags.HideAndDontSave,
                enableInstancing = true,
                doubleSidedGI = source.doubleSidedGI,
                globalIlluminationFlags = source.globalIlluminationFlags
            };

            Color baseColor = Color.white;
            if (source.HasProperty("_BaseColor"))
            {
                baseColor = source.GetColor("_BaseColor");
            }
            else if (source.HasProperty("_Color"))
            {
                baseColor = source.GetColor("_Color");
            }
            converted.SetColor("_Color", baseColor);

            CopyTextureProperty(source, "_BaseMap", converted, "_MainTex");
            if (converted.GetTexture("_MainTex") == null)
            {
                CopyTextureProperty(source, "_MainTex", converted, "_MainTex");
            }

            CopyTextureProperty(source, "_BumpMap", converted, "_BumpMap");
            if (converted.GetTexture("_BumpMap") != null)
            {
                converted.EnableKeyword("_NORMALMAP");
                if (source.HasProperty("_BumpScale"))
                {
                    converted.SetFloat("_BumpScale", source.GetFloat("_BumpScale"));
                }
            }

            CopyTextureProperty(
                source,
                "_MetallicGlossMap",
                converted,
                "_MetallicGlossMap");
            if (converted.GetTexture("_MetallicGlossMap") != null)
            {
                converted.EnableKeyword("_METALLICGLOSSMAP");
            }

            CopyTextureProperty(source, "_OcclusionMap", converted, "_OcclusionMap");
            if (source.HasProperty("_OcclusionStrength"))
            {
                converted.SetFloat(
                    "_OcclusionStrength",
                    source.GetFloat("_OcclusionStrength"));
            }

            CopyTextureProperty(source, "_EmissionMap", converted, "_EmissionMap");
            if (source.HasProperty("_EmissionColor"))
            {
                Color emission = source.GetColor("_EmissionColor");
                converted.SetColor("_EmissionColor", emission);
                if (converted.GetTexture("_EmissionMap") != null ||
                    emission.maxColorComponent > 0.001f)
                {
                    converted.EnableKeyword("_EMISSION");
                }
            }

            if (source.HasProperty("_Metallic"))
            {
                converted.SetFloat("_Metallic", source.GetFloat("_Metallic"));
            }
            if (source.HasProperty("_Smoothness"))
            {
                converted.SetFloat("_Glossiness", source.GetFloat("_Smoothness"));
            }
            else if (source.HasProperty("_Glossiness"))
            {
                converted.SetFloat("_Glossiness", source.GetFloat("_Glossiness"));
            }
            if (source.HasProperty("_Cutoff"))
            {
                converted.SetFloat("_Cutoff", source.GetFloat("_Cutoff"));
            }
            if (source.HasProperty("_Cull"))
            {
                converted.SetFloat("_Cull", source.GetFloat("_Cull"));
            }

            bool transparent =
                source.HasProperty("_Surface") && source.GetFloat("_Surface") > 0.5f;
            bool alphaClip =
                source.HasProperty("_AlphaClip") && source.GetFloat("_AlphaClip") > 0.5f;
            ConfigureBuiltInSurface(converted, transparent, alphaClip);
            return converted;
        }

        public static void ApplyStage01Lighting()
        {
            RenderSettings.fog = false;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor =
                new Color(0.72f, 0.65f, 0.54f, 1f);
            RenderSettings.ambientEquatorColor =
                new Color(0.56f, 0.50f, 0.42f, 1f);
            RenderSettings.ambientGroundColor =
                new Color(0.40f, 0.36f, 0.31f, 1f);
            RenderSettings.ambientIntensity = 1.55f;
            RenderSettings.reflectionIntensity = 1f;
        }

        private static string DescribeLoadedScenes()
        {
            if (SceneManager.sceneCount == 0)
            {
                return "<none>";
            }

            var descriptions = new string[SceneManager.sceneCount];
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                descriptions[index] =
                    $"{scene.name}|{scene.path}|loaded={scene.isLoaded}";
            }

            return string.Join(", ", descriptions);
        }

        private void Fail(string message)
        {
            compositionFailure = message ?? "Unknown Stage01 integration failure.";
            Debug.LogError($"BREAK NOTICE Stage01 integration failed: {compositionFailure}");
        }

        private static GameObject FindRoot(Scene scene, string rootName)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                GameObject root = roots[index];
                if (root != null &&
                    string.Equals(root.name, rootName, StringComparison.Ordinal))
                {
                    return root;
                }
            }

            return null;
        }

        private static Collider FindLegacyFloorCollider(GameObject legacyRoot)
        {
            Collider[] colliders = legacyRoot.GetComponentsInChildren<Collider>(true);
            for (int index = 0; index < colliders.Length; index++)
            {
                Collider collider = colliders[index];
                if (collider != null &&
                    string.Equals(
                        collider.name,
                        LegacyFloorColliderName,
                        StringComparison.Ordinal))
                {
                    return collider;
                }
            }

            return null;
        }

        private static bool IsLegacyFloorCollider(Collider collider)
        {
            return collider != null &&
                string.Equals(
                    collider.name,
                    LegacyFloorColliderName,
                    StringComparison.Ordinal);
        }

        private static bool HasEnabledCollider(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int index = 0; index < colliders.Length; index++)
            {
                Collider collider = colliders[index];
                if (collider != null && collider.enabled)
                {
                    return true;
                }
            }

            return false;
        }

        private static Transform FindStageFloor(Scene scene)
        {
            Transform[] transforms = GetSceneComponents<Transform>(scene);
            Transform best = null;
            float bestArea = -1f;
            for (int index = 0; index < transforms.Length; index++)
            {
                Transform candidate = transforms[index];
                if (candidate == null ||
                    !string.Equals(
                        candidate.name,
                        EnvironmentFloorObjectName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!TryCalculateWorldBounds(candidate.gameObject, out Bounds bounds))
                {
                    if (best == null)
                    {
                        best = candidate;
                    }
                    continue;
                }

                float area = bounds.size.x * bounds.size.z;
                if (area > bestArea)
                {
                    best = candidate;
                    bestArea = area;
                }
            }

            return best;
        }

        private static bool TryCalculateWorldBounds(
            GameObject owner,
            out Bounds bounds)
        {
            Renderer[] renderers = owner.GetComponentsInChildren<Renderer>(true);
            bool hasBounds = false;
            bounds = default;
            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer == null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return hasBounds &&
                IsFinite(bounds.center) &&
                IsFinite(bounds.size) &&
                bounds.size.sqrMagnitude > 0.000001f;
        }

        private static bool TryCalculateActiveSceneBounds(
            Scene scene,
            out Bounds bounds)
        {
            Renderer[] renderers = GetSceneComponents<Renderer>(scene);
            bool hasBounds = false;
            bounds = default;
            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer == null ||
                    !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Bounds candidate = renderer.bounds;
                if (!IsFinite(candidate.center) ||
                    !IsFinite(candidate.size) ||
                    candidate.size.sqrMagnitude <= 0.000001f)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = candidate;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(candidate);
                }
            }

            return hasBounds;
        }

        private static void SetBoxColliderWorldBounds(
            BoxCollider boxCollider,
            Bounds worldBounds)
        {
            Transform transform = boxCollider.transform;
            Vector3 localMin = new Vector3(
                float.PositiveInfinity,
                float.PositiveInfinity,
                float.PositiveInfinity);
            Vector3 localMax = new Vector3(
                float.NegativeInfinity,
                float.NegativeInfinity,
                float.NegativeInfinity);

            Vector3 worldMin = worldBounds.min;
            Vector3 worldMax = worldBounds.max;
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        Vector3 worldCorner = new Vector3(
                            x == 0 ? worldMin.x : worldMax.x,
                            y == 0 ? worldMin.y : worldMax.y,
                            z == 0 ? worldMin.z : worldMax.z);
                        Vector3 localCorner =
                            transform.InverseTransformPoint(worldCorner);
                        localMin = Vector3.Min(localMin, localCorner);
                        localMax = Vector3.Max(localMax, localCorner);
                    }
                }
            }

            Vector3 localSize = localMax - localMin;
            if (!IsFinite(localMin) ||
                !IsFinite(localMax) ||
                !IsFinite(localSize) ||
                localSize.x <= 0.0001f ||
                localSize.y <= 0.0001f ||
                localSize.z <= 0.0001f)
            {
                throw new InvalidOperationException(
                    $"Could not derive a valid local floor collider from {worldBounds}.");
            }

            boxCollider.center = (localMin + localMax) * 0.5f;
            boxCollider.size = localSize;
        }

        private static int ConvertStageMaterialsForCurrentPipeline(Scene scene)
        {
            if (GraphicsSettings.currentRenderPipeline != null)
            {
                return 0;
            }

            Renderer[] renderers = GetSceneComponents<Renderer>(scene);
            var conversions = new Dictionary<Material, Material>();
            for (int rendererIndex = 0;
                 rendererIndex < renderers.Length;
                 rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                if (renderer == null)
                {
                    continue;
                }

                Material[] materials = renderer.sharedMaterials;
                bool changed = false;
                for (int materialIndex = 0;
                     materialIndex < materials.Length;
                     materialIndex++)
                {
                    Material source = materials[materialIndex];
                    if (!IsUniversalPipelineMaterial(source))
                    {
                        continue;
                    }

                    if (!conversions.TryGetValue(source, out Material converted))
                    {
                        converted = CreateBuiltInCompatibleMaterial(source);
                        conversions.Add(source, converted);
                        RuntimeStageMaterials.Add(converted);
                    }

                    materials[materialIndex] = converted;
                    changed = true;
                }

                if (changed)
                {
                    renderer.sharedMaterials = materials;
                }
            }

            return conversions.Count;
        }

        private static bool IsUniversalPipelineMaterial(Material material)
        {
            return material != null &&
                material.shader != null &&
                material.shader.name.StartsWith(
                    "Universal Render Pipeline/",
                    StringComparison.Ordinal);
        }

        private static void CopyTextureProperty(
            Material source,
            string sourceProperty,
            Material target,
            string targetProperty)
        {
            if (!source.HasProperty(sourceProperty) ||
                !target.HasProperty(targetProperty))
            {
                return;
            }

            target.SetTexture(targetProperty, source.GetTexture(sourceProperty));
            target.SetTextureScale(
                targetProperty,
                source.GetTextureScale(sourceProperty));
            target.SetTextureOffset(
                targetProperty,
                source.GetTextureOffset(sourceProperty));
        }

        private static void ConfigureBuiltInSurface(
            Material material,
            bool transparent,
            bool alphaClip)
        {
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

            if (transparent)
            {
                material.SetFloat("_Mode", 3f);
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.EnableKeyword("_ALPHABLEND_ON");
                material.renderQueue = (int)RenderQueue.Transparent;
                return;
            }

            if (alphaClip)
            {
                material.SetFloat("_Mode", 1f);
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetInt("_SrcBlend", (int)BlendMode.One);
                material.SetInt("_DstBlend", (int)BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.EnableKeyword("_ALPHATEST_ON");
                material.renderQueue = (int)RenderQueue.AlphaTest;
                return;
            }

            material.SetFloat("_Mode", 0f);
            material.SetOverrideTag("RenderType", "Opaque");
            material.SetInt("_SrcBlend", (int)BlendMode.One);
            material.SetInt("_DstBlend", (int)BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            material.renderQueue = -1;
        }

        private static bool IsFinite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
                !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
                !float.IsNaN(value.z) && !float.IsInfinity(value.z);
        }

        private static void ReleaseRuntimeStageMaterials()
        {
            for (int index = 0; index < RuntimeStageMaterials.Count; index++)
            {
                Material material = RuntimeStageMaterials[index];
                if (material == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(material);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(material);
                }
            }

            RuntimeStageMaterials.Clear();
            RuntimeStageInstancedMaterials.Clear();
        }

        private static T[] GetSceneComponents<T>(Scene scene)
            where T : Component
        {
            GameObject[] roots = scene.GetRootGameObjects();
            int total = 0;
            var groups = new T[roots.Length][];
            for (int index = 0; index < roots.Length; index++)
            {
                T[] components = roots[index].GetComponentsInChildren<T>(true);
                groups[index] = components;
                total += components.Length;
            }

            var result = new T[total];
            int writeIndex = 0;
            for (int index = 0; index < groups.Length; index++)
            {
                T[] components = groups[index];
                Array.Copy(components, 0, result, writeIndex, components.Length);
                writeIndex += components.Length;
            }

            return result;
        }

        private static int DisableComponents<T>(T[] components)
            where T : Behaviour
        {
            int changed = 0;
            for (int index = 0; index < components.Length; index++)
            {
                T component = components[index];
                if (component != null && component.enabled)
                {
                    component.enabled = false;
                    changed++;
                }
            }

            return changed;
        }

        private static int DisableComponents(Collider[] colliders)
        {
            int changed = 0;
            for (int index = 0; index < colliders.Length; index++)
            {
                Collider collider = colliders[index];
                if (collider != null && collider.enabled)
                {
                    collider.enabled = false;
                    changed++;
                }
            }

            return changed;
        }

        private static int DisableComponents(Renderer[] renderers)
        {
            int changed = 0;
            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer != null && renderer.enabled)
                {
                    renderer.enabled = false;
                    changed++;
                }
            }

            return changed;
        }
    }
}
