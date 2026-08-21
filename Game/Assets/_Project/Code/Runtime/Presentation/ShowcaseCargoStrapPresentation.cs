using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Presentation
{
    [DefaultExecutionOrder(210)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseCargoStrapPresentation : MonoBehaviour
    {
        public const float ReleaseRetractSeconds = 0.2f;
        public const float StrapWorldWidthMeters = 0.016f;

        public static Color32 StrapYellowColor =>
            new Color32(184, 151, 64, 255);

        public static Color32 StrapBlackColor =>
            new Color32(28, 29, 26, 255);

        private const int PathPointCount = 12;
        private const int HandRoutePointCount = 6;
        private const int TetherPointCount = 7;
        private const float PrimaryGripSurfaceOffset = 0.032f;
        private const float TetherGripSurfaceOffset = 0.035f;
        private const float RightSideRouteClearance = 0.045f;
        private const float RightSideHandClearance = 0.025f;
        private const float MaximumSlackSag = 0.34f;
        private const float PrepareDrawStart01 = 0.05f;
        private const float PrepareHandCapture01 = 0.7f;

        public static Vector3 CarabinerModelLocalPosition =>
            ShowcaseCargoStrapController.AuthoredCarabinerModelLocalPosition;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField]
        private ShowcaseCargoStrapController source;

        [SerializeField]
        private Transform reelSocket;

        [SerializeField]
        private Transform rightHandSocket;

        [SerializeField]
        private LineRenderer strapLine;

        [SerializeField]
        private LineRenderer objectTetherLine;

        [SerializeField]
        private Transform reviewClampMarker;

        private readonly Vector3[] pathPoints = new Vector3[PathPointCount];
        private readonly Vector3[] tetherPoints = new Vector3[TetherPointCount];
        private Material runtimeStrapMaterial;
        private Texture2D runtimeStripeTexture;
        private Renderer clampRenderer;
        private MaterialPropertyBlock clampProperties;
        private Animator resolvedHumanoidAnimator;
        private Transform resolvedRightHandBone;
        private Transform resolvedCarabinerSocket;
        private bool carabinerPoseCalibrated;
        private bool ownsRuntimeCarabinerSocket;
        private bool ownsRuntimeObjectTether;

        public ShowcaseCargoStrapController Source => source;
        public Transform ReelSocket => reelSocket;
        public Transform ResolvedCarabinerSocket => resolvedCarabinerSocket;
        public Transform RightHandSocket => rightHandSocket;
        public Transform ResolvedRightHandBone => resolvedRightHandBone;
        public LineRenderer StrapLine => strapLine;
        public LineRenderer ObjectTetherLine => objectTetherLine;
        public Transform ReviewClampMarker => reviewClampMarker;
        public float ConfiguredWorldWidth => StrapWorldWidthMeters;
        public Vector3 LastGripWorldPosition { get; private set; }
        public Vector3 LastPalmCenterWorldPosition { get; private set; }
        public Vector3 LastRouteClearanceWorldPosition { get; private set; }
        public float LastRouteOutwardClearanceMeters { get; private set; }
        public float LastDrawFraction { get; private set; }
        public int LastGripPointIndex { get; private set; } = -1;
        public bool IsUsingSupportedRuntimeMaterial =>
            runtimeStrapMaterial != null &&
            runtimeStrapMaterial.shader != null &&
            runtimeStrapMaterial.shader.isSupported;

        public void Configure(
            ShowcaseCargoStrapController authoredSource,
            Transform authoredReelSocket,
            Transform authoredRightHandSocket,
            LineRenderer authoredStrapLine,
            Transform authoredReviewClampMarker,
            LineRenderer authoredObjectTetherLine = null)
        {
            source = authoredSource;
            reelSocket = authoredReelSocket;
            rightHandSocket = authoredRightHandSocket;
            strapLine = authoredStrapLine;
            objectTetherLine = authoredObjectTetherLine;
            reviewClampMarker = authoredReviewClampMarker;
            carabinerPoseCalibrated = false;
            clampRenderer = reviewClampMarker != null
                ? reviewClampMarker.GetComponentInChildren<Renderer>(true)
                : null;

            ResolveRuntimeRightHand();
            ResolveRuntimeCarabinerSocket();
            EnsureObjectTetherLine();
            ConfigureReviewHardware();
            ConfigureStrapLine();
            SetVisible(false);
        }

        private void Awake()
        {
            clampRenderer = reviewClampMarker != null
                ? reviewClampMarker.GetComponentInChildren<Renderer>(true)
                : null;
            ResolveRuntimeRightHand();
            ResolveRuntimeCarabinerSocket();
            EnsureObjectTetherLine();
            ConfigureReviewHardware();
            ConfigureStrapLine();
            SetVisible(false);
        }

        private void LateUpdate()
        {
            if (source == null)
            {
                SetVisible(false);
                return;
            }

            ResolveRuntimeRightHand();
            ResolveRuntimeCarabinerSocket();
            Present(source.PresentationSnapshot);
        }

        private void OnDisable()
        {
            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (runtimeStrapMaterial != null)
            {
                DestroyUnityObject(runtimeStrapMaterial);
                runtimeStrapMaterial = null;
            }
            if (runtimeStripeTexture != null)
            {
                DestroyUnityObject(runtimeStripeTexture);
                runtimeStripeTexture = null;
            }
            if (ownsRuntimeCarabinerSocket && resolvedCarabinerSocket != null)
            {
                DestroyUnityObject(resolvedCarabinerSocket.gameObject);
            }
            if (ownsRuntimeObjectTether && objectTetherLine != null)
            {
                DestroyUnityObject(objectTetherLine.gameObject);
            }
            resolvedCarabinerSocket = null;
            ownsRuntimeCarabinerSocket = false;
            objectTetherLine = null;
            ownsRuntimeObjectTether = false;
        }

        public void Present(ShowcaseCargoStrapPresentationSnapshot snapshot)
        {
            if (strapLine == null || reviewClampMarker == null)
            {
                return;
            }

            ShowcaseCargoStrapPhase phase = snapshot.Phase;
            if (phase == ShowcaseCargoStrapPhase.Stored)
            {
                SetVisible(false);
                return;
            }

            if (resolvedCarabinerSocket == null)
            {
                SetVisible(false);
                return;
            }
            Vector3 reel = resolvedCarabinerSocket.position;
            Vector3 palmCenter = ResolveGripWorldPosition(reel);
            LastPalmCenterWorldPosition = palmCenter;
            Vector3 primaryGrip = ResolvePrimaryGripSurface(reel, palmCenter);
            LastGripWorldPosition = primaryGrip;
            bool usesHandGuide = UsesHandGuide(snapshot);
            Vector3 guide = usesHandGuide ? palmCenter : reel;
            Vector3 endpoint = ResolveEndpoint(snapshot, guide);
            float slack01 = ResolveSlack(snapshot);
            float drawFraction = ResolveDrawFraction(snapshot);
            LastDrawFraction = drawFraction;

            if (drawFraction <= 0.001f)
            {
                reviewClampMarker.SetPositionAndRotation(
                    reel,
                    ResolveClampRotation(reel, palmCenter));
                SetVisible(false);
                return;
            }

            Color lineTint;
            if (snapshot.HasCharge)
            {
                lineTint = snapshot.Charge01 >=
                    ShowcaseCargoChargeModel.FullLowerThreshold01
                        ? new Color(1f, 0.72f, 0.28f, 1f)
                        : snapshot.Charge01 >=
                          ShowcaseCargoChargeModel.QuickUpperThreshold01
                            ? new Color(0.96f, 0.84f, 0.48f, 1f)
                            : new Color(0.86f, 0.9f, 0.95f, 1f);
            }
            else
            {
                lineTint = snapshot.IsWarning
                    ? new Color(0.82f, 0.64f, 0.42f, 1f)
                    : Color.white;
            }
            SetClampVisible(true);
            strapLine.startColor = lineTint;
            strapLine.endColor = lineTint;
            if (objectTetherLine != null)
            {
                objectTetherLine.startColor = lineTint;
                objectTetherLine.endColor = lineTint;
            }

            Vector3 visibleEndpoint;
            if (snapshot.Phase == ShowcaseCargoStrapPhase.Prepare)
            {
                SetPrimaryVisible(true);
                SetTetherVisible(false);
                visibleEndpoint = WritePreparePath(reel, primaryGrip, drawFraction);
            }
            else if (usesHandGuide)
            {
                SetPrimaryVisible(true);
                WriteFullHandRoute(reel, primaryGrip);
                strapLine.positionCount = HandRoutePointCount;
                strapLine.SetPositions(pathPoints);
                LastGripPointIndex = HandRoutePointCount - 1;

                bool hasObjectSegment = objectTetherLine != null &&
                    Vector3.Distance(palmCenter, endpoint) >
                        TetherGripSurfaceOffset * 1.25f;
                SetTetherVisible(hasObjectSegment);
                if (hasObjectSegment)
                {
                    WriteObjectTether(palmCenter, endpoint, slack01);
                }
                visibleEndpoint = hasObjectSegment ? endpoint : primaryGrip;
            }
            else
            {
                SetPrimaryVisible(true);
                SetTetherVisible(false);
                visibleEndpoint = WriteDirectPrimary(reel, endpoint, slack01);
            }
            reviewClampMarker.SetPositionAndRotation(
                visibleEndpoint,
                ResolveClampRotation(guide, visibleEndpoint));
        }

        private void ConfigureReviewHardware()
        {
            if (clampRenderer == null)
            {
                return;
            }

            clampProperties ??= new MaterialPropertyBlock();
            clampRenderer.GetPropertyBlock(clampProperties);
            Color silver = new Color(0.72f, 0.76f, 0.8f, 1f);
            clampProperties.SetColor(BaseColorId, silver);
            clampProperties.SetColor(ColorId, silver);
            clampRenderer.SetPropertyBlock(clampProperties);
        }

        private void ConfigureStrapLine()
        {
            if (strapLine == null)
            {
                return;
            }

            ConfigureLineRenderer(strapLine, PathPointCount);
            if (objectTetherLine != null)
            {
                ConfigureLineRenderer(objectTetherLine, TetherPointCount);
            }

            if (runtimeStrapMaterial == null)
            {
                // Sprites/Default is already used by the accepted showcase
                // trajectory line and renders correctly through the current URP
                // renderer. Runtime-created URP/Unlit was magenta on the target
                // scene, so keep the serialized Default-Line material as the
                // safe solid-color fallback instead of selecting that shader.
                Shader shader = Shader.Find("Sprites/Default");
                Material authoredFallback = strapLine.sharedMaterial;
                if (shader != null && shader.isSupported)
                {
                    runtimeStrapMaterial = new Material(shader);
                }
                else if (authoredFallback != null)
                {
                    runtimeStrapMaterial = new Material(authoredFallback);
                }
                else
                {
                    return;
                }

                runtimeStrapMaterial.name =
                    "GEAR01_BLACK_YELLOW_STRAP_RUNTIME_REVIEW";
                runtimeStrapMaterial.hideFlags = HideFlags.DontSave;
                if (runtimeStrapMaterial.HasProperty("_MainTex"))
                {
                    runtimeStripeTexture = CreateStripeTexture();
                    runtimeStrapMaterial.SetTexture("_MainTex", runtimeStripeTexture);
                    runtimeStrapMaterial.SetTextureScale(
                        "_MainTex",
                        new Vector2(4f, 1f));
                }
                if (runtimeStrapMaterial.HasProperty(ColorId))
                {
                    runtimeStrapMaterial.SetColor(ColorId, Color.white);
                }
            }

            strapLine.sharedMaterial = runtimeStrapMaterial;
            if (objectTetherLine != null)
            {
                objectTetherLine.sharedMaterial = runtimeStrapMaterial;
            }
        }

        private static void ConfigureLineRenderer(
            LineRenderer line,
            int pointCount)
        {
            if (line == null)
            {
                return;
            }

            line.useWorldSpace = true;
            line.loop = false;
            line.positionCount = pointCount;
            line.startWidth = StrapWorldWidthMeters;
            line.endWidth = StrapWorldWidthMeters;
            line.numCapVertices = 2;
            line.numCornerVertices = 2;
            line.alignment = LineAlignment.View;
            line.textureMode = LineTextureMode.Tile;
            line.startColor = Color.white;
            line.endColor = Color.white;
        }

        private void EnsureObjectTetherLine()
        {
            if (objectTetherLine != null || strapLine == null ||
                !Application.isPlaying)
            {
                return;
            }

            var tetherObject = new GameObject(
                "GEAR01_OBJECT_TETHER_RUNTIME_REVIEW");
            tetherObject.transform.SetParent(strapLine.transform.parent, false);
            objectTetherLine = tetherObject.AddComponent<LineRenderer>();
            ownsRuntimeObjectTether = true;
        }

        private static Texture2D CreateStripeTexture()
        {
            const int width = 64;
            const int height = 4;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "GEAR01_BLACK_YELLOW_STRIPE_RUNTIME_REVIEW",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat,
                hideFlags = HideFlags.DontSave
            };
            var pixels = new Color32[width * height];
            Color32 yellow = StrapYellowColor;
            Color32 black = StrapBlackColor;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels[(y * width) + x] = ((x / 8) & 1) == 0
                        ? yellow
                        : black;
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private Vector3 WritePreparePath(
            Vector3 reel,
            Vector3 hand,
            float drawFraction)
        {
            float clampedDraw = Mathf.Clamp01(drawFraction);
            int pointCount = Mathf.Clamp(
                Mathf.CeilToInt(clampedDraw * (HandRoutePointCount - 1)) + 1,
                2,
                HandRoutePointCount);
            ResolveHandRouteControls(
                reel,
                hand,
                out Vector3 controlA,
                out Vector3 controlB);
            for (int index = 0; index < pointCount; index++)
            {
                float sample01 = index / (float)(pointCount - 1);
                float route01 = sample01 * clampedDraw;
                pathPoints[index] = CubicBezier(
                    reel,
                    controlA,
                    controlB,
                    hand,
                    route01);
            }

            strapLine.positionCount = pointCount;
            for (int index = 0; index < pointCount; index++)
            {
                strapLine.SetPosition(index, pathPoints[index]);
            }
            LastGripPointIndex = clampedDraw >= 0.999f ? pointCount - 1 : -1;
            return pathPoints[pointCount - 1];
        }

        private void WriteObjectTether(
            Vector3 grip,
            Vector3 endpoint,
            float slack01)
        {
            Vector3 direction = endpoint - grip;
            Vector3 tetherStart = direction.sqrMagnitude > 0.0001f
                ? grip + (direction.normalized * TetherGripSurfaceOffset)
                : grip;
            WriteSagLine(
                objectTetherLine,
                tetherPoints,
                tetherStart,
                endpoint,
                slack01,
                TetherPointCount);
        }

        private Vector3 WriteDirectPrimary(
            Vector3 reel,
            Vector3 endpoint,
            float slack01)
        {
            LastGripPointIndex = -1;
            LastRouteClearanceWorldPosition = reel;
            LastRouteOutwardClearanceMeters = 0f;
            WriteSagLine(
                strapLine,
                pathPoints,
                reel,
                endpoint,
                slack01,
                TetherPointCount);
            return endpoint;
        }

        private static Vector3 ResolvePrimaryGripSurface(
            Vector3 reel,
            Vector3 palmCenter)
        {
            Vector3 incoming = palmCenter - reel;
            return incoming.sqrMagnitude > 0.0001f
                ? palmCenter -
                    (incoming.normalized * PrimaryGripSurfaceOffset)
                : palmCenter;
        }

        private static void WriteSagLine(
            LineRenderer line,
            Vector3[] points,
            Vector3 start,
            Vector3 endpoint,
            float slack01,
            int pointCount)
        {
            float length = Vector3.Distance(start, endpoint);
            float sag = Mathf.Min(MaximumSlackSag, length * 0.16f) *
                Mathf.Clamp01(slack01);
            for (int index = 0; index < pointCount; index++)
            {
                float t = index / (float)(pointCount - 1);
                Vector3 point = Vector3.Lerp(start, endpoint, t);
                point += Vector3.down * (Mathf.Sin(Mathf.PI * t) * sag);
                points[index] = point;
            }
            line.positionCount = pointCount;
            line.SetPositions(points);
        }

        private void WriteFullHandRoute(Vector3 reel, Vector3 hand)
        {
            ResolveHandRouteControls(
                reel,
                hand,
                out Vector3 controlA,
                out Vector3 controlB);
            for (int index = 0; index < HandRoutePointCount; index++)
            {
                float t = index / (float)(HandRoutePointCount - 1);
                pathPoints[index] = CubicBezier(
                    reel,
                    controlA,
                    controlB,
                    hand,
                    t);
            }
        }

        private void ResolveHandRouteControls(
            Vector3 reel,
            Vector3 hand,
            out Vector3 controlA,
            out Vector3 controlB)
        {
            Transform actor = source != null ? source.transform : transform;
            Vector3 actorRight = Vector3.ProjectOnPlane(actor.right, Vector3.up);
            if (actorRight.sqrMagnitude <= 0.0001f)
            {
                actorRight = Vector3.right;
            }
            actorRight.Normalize();

            Vector3 actorForward = Vector3.ProjectOnPlane(actor.forward, Vector3.up);
            if (actorForward.sqrMagnitude <= 0.0001f)
            {
                actorForward = Vector3.forward;
            }
            actorForward.Normalize();

            controlA = reel +
                (actorRight * RightSideRouteClearance) +
                (actorForward * 0.015f);
            controlB = hand +
                (actorRight * RightSideHandClearance) +
                (actorForward * 0.015f);

            Vector3 midpoint = CubicBezier(
                reel,
                controlA,
                controlB,
                hand,
                0.5f);
            LastRouteClearanceWorldPosition = midpoint;
            LastRouteOutwardClearanceMeters = Mathf.Max(
                0f,
                Vector3.Dot(
                    midpoint - Vector3.Lerp(reel, hand, 0.5f),
                    actorRight));
        }

        private static Vector3 CubicBezier(
            Vector3 start,
            Vector3 controlA,
            Vector3 controlB,
            Vector3 end,
            float t)
        {
            float clamped = Mathf.Clamp01(t);
            float inverse = 1f - clamped;
            return (inverse * inverse * inverse * start) +
                (3f * inverse * inverse * clamped * controlA) +
                (3f * inverse * clamped * clamped * controlB) +
                (clamped * clamped * clamped * end);
        }

        private static Vector3 ResolveEndpoint(
            ShowcaseCargoStrapPresentationSnapshot snapshot,
            Vector3 hand)
        {
            float progress = Mathf.Clamp01(snapshot.PhaseProgress01);
            switch (snapshot.Phase)
            {
                case ShowcaseCargoStrapPhase.Prepare:
                case ShowcaseCargoStrapPhase.Rolling:
                    return hand;
                case ShowcaseCargoStrapPhase.Throw:
                {
                    if (snapshot.IsTensioned &&
                        snapshot.ActiveTarget?.WeightClass ==
                            PrototypeWeightClass.Light)
                    {
                        // Small is already clamped and physically orbiting the
                        // hand. Keep the line on the real object until the
                        // release event; this Throw is not an attach flight.
                        return snapshot.AnchorWorldPosition;
                    }

                    if (!snapshot.IsClampReleased)
                    {
                        return hand;
                    }

                    const float releaseNormalized = 13f / 14f;
                    const float contactNormalized = 0.985f;
                    float flightProgress = Mathf.InverseLerp(
                        releaseNormalized,
                        contactNormalized,
                        progress);
                    float arcProgress = Mathf.SmoothStep(
                        0f,
                        1f,
                        flightProgress);
                    Vector3 endpoint = Vector3.Lerp(hand, snapshot.AnchorWorldPosition, arcProgress);
                    endpoint += Vector3.up * (Mathf.Sin(Mathf.PI * arcProgress) * 0.32f);
                    return endpoint;
                }
                case ShowcaseCargoStrapPhase.Releasing:
                {
                    float retractProgress = Mathf.SmoothStep(0f, 1f, progress);
                    return Vector3.Lerp(
                        snapshot.ReleaseStartWorldPosition,
                        snapshot.ReleaseEndWorldPosition,
                        retractProgress);
                }
                default:
                    return snapshot.AnchorWorldPosition;
            }
        }

        private static float ResolveSlack(
            ShowcaseCargoStrapPresentationSnapshot snapshot)
        {
            if (snapshot.ActiveTarget?.WeightClass ==
                    PrototypeWeightClass.Medium &&
                snapshot.HasDynamicTension)
            {
                return 1f - snapshot.Tension01;
            }
            switch (snapshot.Phase)
            {
                case ShowcaseCargoStrapPhase.Prepare:
                case ShowcaseCargoStrapPhase.Rolling:
                case ShowcaseCargoStrapPhase.Throw:
                    return snapshot.IsTensioned &&
                           snapshot.ActiveTarget?.WeightClass ==
                               PrototypeWeightClass.Light
                        ? 0f
                        : 1f;
                case ShowcaseCargoStrapPhase.Pull:
                    return 1f - Mathf.Clamp01(snapshot.PhaseProgress01);
                case ShowcaseCargoStrapPhase.SmallSpinning:
                    return 0f;
                case ShowcaseCargoStrapPhase.AttackLeft:
                case ShowcaseCargoStrapPhase.AttackRight:
                case ShowcaseCargoStrapPhase.Charging:
                case ShowcaseCargoStrapPhase.Slam:
                case ShowcaseCargoStrapPhase.MediumExtending:
                    return 0f;
                case ShowcaseCargoStrapPhase.MediumRecovering:
                    return 0.15f;
                case ShowcaseCargoStrapPhase.Tensioned:
                    return snapshot.ActiveTarget?.WeightClass ==
                        PrototypeWeightClass.Medium
                            ? 0.25f
                            : 0f;
                case ShowcaseCargoStrapPhase.Releasing:
                    return 0.35f;
                default:
                    return snapshot.IsTensioned ? 0f : 0.55f;
            }
        }

        private static float ResolveDrawFraction(
            ShowcaseCargoStrapPresentationSnapshot snapshot)
        {
            if (snapshot.Phase != ShowcaseCargoStrapPhase.Prepare)
            {
                return 1f;
            }

            float normalized = Mathf.InverseLerp(
                PrepareDrawStart01,
                PrepareHandCapture01,
                Mathf.Clamp01(snapshot.PhaseProgress01));
            return Mathf.SmoothStep(0f, 1f, normalized);
        }

        private static bool UsesHandGuide(
            ShowcaseCargoStrapPresentationSnapshot snapshot)
        {
            return snapshot.Phase == ShowcaseCargoStrapPhase.Prepare ||
                snapshot.Phase == ShowcaseCargoStrapPhase.Rolling ||
                snapshot.Phase == ShowcaseCargoStrapPhase.Throw ||
                snapshot.Phase == ShowcaseCargoStrapPhase.Pull ||
                snapshot.Phase == ShowcaseCargoStrapPhase.SmallSpinning ||
                snapshot.WeaponSelection == PrototypeWeaponSelection.ObjectWeapon;
        }

        private void ResolveRuntimeRightHand()
        {
            if (resolvedHumanoidAnimator != null &&
                resolvedHumanoidAnimator.isHuman)
            {
                Transform current = resolvedHumanoidAnimator.GetBoneTransform(
                    HumanBodyBones.RightHand);
                if (current != null)
                {
                    resolvedRightHandBone = current;
                    return;
                }
            }

            resolvedHumanoidAnimator = null;
            resolvedRightHandBone = null;

            ShowcaseCharacterAnimationDriver driver = source != null
                ? source.GetComponent<ShowcaseCharacterAnimationDriver>()
                : GetComponent<ShowcaseCharacterAnimationDriver>();
            Animator candidate = driver != null ? driver.Animator : null;
            if (TryResolveRightHand(candidate, out Transform rightHand))
            {
                resolvedHumanoidAnimator = candidate;
                resolvedRightHandBone = rightHand;
                return;
            }

            Animator[] animators = source != null
                ? source.GetComponentsInChildren<Animator>(true)
                : GetComponentsInChildren<Animator>(true);
            foreach (Animator animator in animators)
            {
                if (!TryResolveRightHand(animator, out rightHand))
                {
                    continue;
                }

                resolvedHumanoidAnimator = animator;
                resolvedRightHandBone = rightHand;
                return;
            }
        }

        private void ResolveRuntimeCarabinerSocket()
        {
            if (resolvedCarabinerSocket != null)
            {
                CalibrateCarabinerSocketFromModel();
                return;
            }

            if (source == null)
            {
                resolvedCarabinerSocket = reelSocket;
                return;
            }

            Transform[] children = source.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child != null && child.name == "GEAR01_CARABINER_SOCKET")
                {
                    resolvedCarabinerSocket = child;
                    reelSocket = child;
                    CalibrateCarabinerSocketFromModel();
                    return;
                }
            }

            ResolveRuntimeRightHand();
            Animator animator = resolvedHumanoidAnimator;
            Transform hips = animator != null && animator.isHuman
                ? animator.GetBoneTransform(HumanBodyBones.Hips)
                : null;
            if (hips == null)
            {
                // No root-offset fallback: a line detached from the authored
                // waist hardware is more misleading than no line.
                return;
            }

            var socketObject = new GameObject("GEAR01_CARABINER_SOCKET_RUNTIME");
            Vector3 worldPosition = animator.transform.TransformPoint(
                CarabinerModelLocalPosition);
            socketObject.transform.SetPositionAndRotation(
                worldPosition,
                source.transform.rotation);
            socketObject.transform.SetParent(hips, true);
            resolvedCarabinerSocket = socketObject.transform;
            reelSocket = resolvedCarabinerSocket;
            ownsRuntimeCarabinerSocket = true;
            carabinerPoseCalibrated = true;
        }

        private void CalibrateCarabinerSocketFromModel()
        {
            if (carabinerPoseCalibrated || resolvedCarabinerSocket == null)
            {
                return;
            }

            ResolveRuntimeRightHand();
            Animator animator = resolvedHumanoidAnimator;
            if (animator == null || !animator.isHuman)
            {
                return;
            }

            resolvedCarabinerSocket.SetPositionAndRotation(
                animator.transform.TransformPoint(CarabinerModelLocalPosition),
                animator.transform.rotation);
            carabinerPoseCalibrated = true;
        }

        private Vector3 ResolveGripWorldPosition(Vector3 fallback)
        {
            if (ShowcaseCargoStrapController.TryResolveRightHandGripWorldPosition(
                    resolvedHumanoidAnimator,
                    out Vector3 gripWorldPosition))
            {
                return gripWorldPosition;
            }

            return rightHandSocket != null ? rightHandSocket.position : fallback;
        }

        private static bool TryResolveRightHand(
            Animator animator,
            out Transform rightHand)
        {
            rightHand = animator != null && animator.isHuman
                ? animator.GetBoneTransform(HumanBodyBones.RightHand)
                : null;
            return rightHand != null;
        }

        private static Quaternion ResolveClampRotation(Vector3 hand, Vector3 endpoint)
        {
            Vector3 direction = endpoint - hand;
            return direction.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(direction.normalized, Vector3.up)
                : Quaternion.identity;
        }

        private void SetVisible(bool visible)
        {
            SetPrimaryVisible(visible);
            SetTetherVisible(false);
            SetClampVisible(visible);
        }

        private void SetPrimaryVisible(bool visible)
        {
            if (strapLine != null)
            {
                strapLine.enabled = visible;
            }
        }

        private void SetTetherVisible(bool visible)
        {
            if (objectTetherLine != null)
            {
                objectTetherLine.enabled = visible;
            }
        }

        private void SetClampVisible(bool visible)
        {
            if (reviewClampMarker != null)
            {
                reviewClampMarker.gameObject.SetActive(visible);
            }
        }

        private static void DestroyUnityObject(Object target)
        {
            if (target == null)
            {
                return;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(target);
                return;
            }
#endif
            Object.Destroy(target);
        }
    }
}
