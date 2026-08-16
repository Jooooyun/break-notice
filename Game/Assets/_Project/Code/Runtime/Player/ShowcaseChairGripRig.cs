using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    public enum ShowcaseChairGripPhase
    {
        None = 0,
        BlendIn = 1,
        Holding = 2,
        BlendOut = 3
    }

    [DisallowMultipleComponent]
    public sealed class ShowcaseChairGripRig : MonoBehaviour
    {
        private const float GripBlendDuration = 0.12f;
        private const float PalmClearance = 0.025f;
        private const float WristLift = 0.12f;
        private const float WristRetraction = 0.14f;
        private const float WristInboardOffset = 0.13f;
        private const float ReachBendMargin = 0.04f;
        private const int PlantEvidenceFrames = 3;
        private const float PlantStabilityTolerance = 0.002f;
        private const float RenderedContactGoalEnvelope = 0.05f;
        private const float WeightEpsilon = 0.0001f;

        [SerializeField]
        private Animator animator;

        [SerializeField, Range(0.1f, 1f)]
        private float backrestHeight = 0.78f;

        [SerializeField, Range(0.05f, 0.5f)]
        private float handInset = 0.22f;

        private BoxCollider trackedChairCollider;
        private BoxCollider releaseChairCollider;
        private Transform leftHand;
        private Transform rightHand;
        private Transform leftUpperArm;
        private Transform rightUpperArm;
        private Transform leftLowerArm;
        private Transform rightLowerArm;
        private float leftArmLength;
        private float rightArmLength;
        private BoxCollider plantedChairCollider;
        private Vector3 plantedLeftLocal;
        private Vector3 plantedRightLocal;
        private bool plantedPoseLatched;
        private bool contactCalibrationReady;
        private BoxCollider pendingPlantChairCollider;
        private Vector3 pendingPlantLeftLocal;
        private Vector3 pendingPlantRightLocal;
        private int plantEvidenceCount;
        private int pendingPlantEvidenceFrame = -1;
        private ShowcaseChairGripPhase requestedPhase;
        private float requestedWeight;
        private int requestedBlendStartCount;
        private GripPose releasePose;
        private GripPose releaseRenderedContact;
        private bool releaseRenderedContactValid;
        private int evaluatedFrame = -1;
        private ShowcaseChairGripPhase evaluatedPhase;
        private float evaluatedWeight;
        private int evaluatedBlendStartCount;
        private BoxCollider evaluatedTarget;
        private GripPose evaluatedPose;

        public bool IsAvailable => animator != null &&
            animator.isActiveAndEnabled && animator.isHuman &&
            leftHand != null && rightHand != null &&
            leftUpperArm != null && rightUpperArm != null &&
            leftLowerArm != null && rightLowerArm != null &&
            IsValidArmLength(leftArmLength) &&
            IsValidArmLength(rightArmLength);
        public bool IsActive => IsAvailable &&
            requestedPhase != ShowcaseChairGripPhase.None;

        // GripTarget and HandError expose the analytic IK control goal and its
        // raw post-solver residual. RenderedGripContact is a separate one-shot
        // observed contact and never feeds back into IK or object authority.
        public ShowcaseChairGripPhase Phase { get; private set; }
        public float CurrentWeight { get; private set; }
        public BoxCollider TargetCollider { get; private set; }
        public Vector3 LeftGripTarget { get; private set; }
        public Vector3 RightGripTarget { get; private set; }
        public float LeftHandError { get; private set; }
        public float RightHandError { get; private set; }
        public bool HasRenderedGripContact { get; private set; }
        public Vector3 LeftRenderedGripContact { get; private set; }
        public Vector3 RightRenderedGripContact { get; private set; }
        public float LeftRenderedGripContactError { get; private set; }
        public float RightRenderedGripContactError { get; private set; }
        public int LastAppliedFrame { get; private set; } = -1;
        public int BlendStartCount { get; private set; }

        private void Awake()
        {
            ResolveBones();
            ResetGrip();
        }

        private void Update()
        {
            if (!IsAvailable)
            {
                ResetGrip();
                return;
            }

            float step = GripBlendDuration > 0f
                ? Time.deltaTime / GripBlendDuration
                : 1f;
            switch (requestedPhase)
            {
                case ShowcaseChairGripPhase.BlendIn:
                    requestedWeight = Mathf.MoveTowards(requestedWeight, 1f, step);
                    if (requestedWeight >= 1f - WeightEpsilon)
                    {
                        requestedWeight = 1f;
                        requestedPhase = ShowcaseChairGripPhase.Holding;
                    }
                    break;

                case ShowcaseChairGripPhase.Holding:
                    requestedWeight = 1f;
                    break;

                case ShowcaseChairGripPhase.BlendOut:
                    requestedWeight = Mathf.MoveTowards(requestedWeight, 0f, step);
                    if (requestedWeight <= WeightEpsilon)
                    {
                        ClearRequestedGrip();
                    }
                    break;
            }
        }

        private void LateUpdate()
        {
            if (!IsAvailable)
            {
                ResetGrip();
                return;
            }

            if (evaluatedFrame != Time.frameCount)
            {
                ResetAppliedDiagnostics();
                return;
            }

            Phase = evaluatedPhase;
            CurrentWeight = evaluatedWeight;
            TargetCollider = evaluatedTarget;
            LeftGripTarget = evaluatedPose.LeftHand;
            RightGripTarget = evaluatedPose.RightHand;
            LeftHandError = evaluatedWeight > WeightEpsilon && leftHand != null
                ? Vector3.Distance(leftHand.position, evaluatedPose.LeftHand)
                : 0f;
            RightHandError = evaluatedWeight > WeightEpsilon && rightHand != null
                ? Vector3.Distance(rightHand.position, evaluatedPose.RightHand)
                : 0f;
            TryLatchRenderedContact(
                evaluatedPhase,
                evaluatedWeight,
                evaluatedTarget,
                evaluatedPose);
            HasRenderedGripContact = TryGetRenderedContact(
                evaluatedPhase,
                evaluatedTarget,
                out Vector3 leftContact,
                out Vector3 rightContact);
            LeftRenderedGripContact = HasRenderedGripContact
                ? leftContact
                : Vector3.zero;
            RightRenderedGripContact = HasRenderedGripContact
                ? rightContact
                : Vector3.zero;
            LeftRenderedGripContactError = HasRenderedGripContact && leftHand != null
                ? Vector3.Distance(leftHand.position, leftContact)
                : 0f;
            RightRenderedGripContactError = HasRenderedGripContact && rightHand != null
                ? Vector3.Distance(rightHand.position, rightContact)
                : 0f;
            BlendStartCount = evaluatedBlendStartCount;
            LastAppliedFrame = Time.frameCount;
        }

        private void OnDisable()
        {
            ResetGrip();
        }

        private void OnDestroy()
        {
            ResetGrip();
        }

        public void Configure(Animator authoredAnimator)
        {
            ResetGrip();
            animator = authoredAnimator;
            ResolveBones();
        }

        public bool BeginGrip(BoxCollider authoredChair)
        {
            ResolveBones();
            if (!IsAvailable ||
                !ShowcaseOrientedBoxGeometry.TryCreate(authoredChair, out _))
            {
                ClearRequestedGrip();
                return false;
            }

            if (trackedChairCollider == authoredChair &&
                (requestedPhase == ShowcaseChairGripPhase.BlendIn ||
                 requestedPhase == ShowcaseChairGripPhase.Holding))
            {
                return true;
            }

            trackedChairCollider = authoredChair;
            ClearRenderedContactState();
            releaseChairCollider = null;
            releasePose = default;
            if (requestedPhase == ShowcaseChairGripPhase.None)
            {
                requestedWeight = 0f;
            }
            requestedPhase = ShowcaseChairGripPhase.BlendIn;
            requestedBlendStartCount++;
            return true;
        }

        public void EndGrip()
        {
            if (requestedPhase == ShowcaseChairGripPhase.None ||
                requestedPhase == ShowcaseChairGripPhase.BlendOut)
            {
                return;
            }

            if (!TryBuildGripPose(trackedChairCollider, false, out releasePose))
            {
                ClearRequestedGrip();
                return;
            }

            releaseRenderedContactValid = TryGetActiveRenderedContact(
                trackedChairCollider,
                out releaseRenderedContact);
            releaseChairCollider = trackedChairCollider;
            trackedChairCollider = null;
            ClearActiveRenderedContact();
            requestedPhase = ShowcaseChairGripPhase.BlendOut;
            requestedBlendStartCount++;
        }

        public void ResetGrip()
        {
            ClearRequestedGrip();
            requestedBlendStartCount = 0;
            ResetAppliedDiagnostics();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (layerIndex != 0 || animator == null || !animator.isHuman)
            {
                return;
            }

            if (!IsActive || !IsAvailable || requestedWeight <= WeightEpsilon)
            {
                ApplyZeroWeights();
                ResetAppliedDiagnostics();
                return;
            }

            BoxCollider diagnosticTarget;
            GripPose pose;
            if (requestedPhase == ShowcaseChairGripPhase.BlendOut)
            {
                diagnosticTarget = releaseChairCollider;
                pose = releasePose;
            }
            else
            {
                diagnosticTarget = trackedChairCollider;
                bool shouldPlant = requestedPhase == ShowcaseChairGripPhase.Holding &&
                    requestedWeight >= 1f - WeightEpsilon;
                if (!TryBuildGripPose(trackedChairCollider, shouldPlant, out pose))
                {
                    ApplyZeroWeights();
                    ClearRequestedGrip();
                    ResetAppliedDiagnostics();
                    return;
                }
            }

            float weight = Mathf.Clamp01(requestedWeight);
            ApplyHandGoal(AvatarIKGoal.LeftHand, pose.LeftHand, weight);
            ApplyHandGoal(AvatarIKGoal.RightHand, pose.RightHand, weight);
            animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0f);
            animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0f);
            StageAppliedDiagnostics(requestedPhase, weight, diagnosticTarget, pose);
        }

        public void GetGripPositions(out Vector3 left, out Vector3 right)
        {
            if (requestedPhase == ShowcaseChairGripPhase.BlendOut)
            {
                left = releasePose.LeftHand;
                right = releasePose.RightHand;
                return;
            }

            if (TryBuildGripPose(trackedChairCollider, false, out GripPose pose))
            {
                left = pose.LeftHand;
                right = pose.RightHand;
                return;
            }

            left = Vector3.zero;
            right = Vector3.zero;
        }

        private bool TryBuildGripPose(
            BoxCollider chair,
            bool allowPlant,
            out GripPose pose)
        {
            pose = default;
            if (leftUpperArm == null || rightUpperArm == null ||
                !ShowcaseOrientedBoxGeometry.TryCreate(chair, out var geometry))
            {
                return false;
            }

            // Retain the legacy serialized field for scene compatibility. The
            // authored grip now uses the backrest top as its contact height.
            _ = backrestHeight;
            float surfaceLateral = Mathf.Max(
                0.08f,
                geometry.HalfExtents.x * (1f - handInset));
            float wristLateral = Mathf.Max(
                0.08f,
                surfaceLateral - WristInboardOffset);
            Vector3 nearBackrestSurface = geometry.Center +
                geometry.Up * geometry.HalfExtents.y -
                geometry.Forward * (geometry.HalfExtents.z + PalmClearance);
            Vector3 wristCenter = nearBackrestSurface +
                geometry.Up * WristLift -
                geometry.Forward * WristRetraction;
            Vector3 idealLeft = wristCenter - geometry.Right * wristLateral;
            Vector3 idealRight = wristCenter + geometry.Right * wristLateral;
            if (!TryResolvePlantedHands(
                    chair,
                    idealLeft,
                    idealRight,
                    allowPlant,
                    out Vector3 left,
                    out Vector3 right))
            {
                return false;
            }

            pose = new GripPose(left, right);
            return true;
        }

        private bool TryResolvePlantedHands(
            BoxCollider chair,
            Vector3 idealLeft,
            Vector3 idealRight,
            bool allowPlant,
            out Vector3 left,
            out Vector3 right)
        {
            left = Vector3.zero;
            right = Vector3.zero;
            if (chair == null || leftUpperArm == null || rightUpperArm == null ||
                !IsValidArmLength(leftArmLength) ||
                !IsValidArmLength(rightArmLength))
            {
                return false;
            }

            left = ProjectToArmReach(
                leftUpperArm.position,
                idealLeft,
                leftArmLength);
            right = ProjectToArmReach(
                rightUpperArm.position,
                idealRight,
                rightArmLength);
            if (!IsFinite(left) || !IsFinite(right))
            {
                return false;
            }

            if (!allowPlant || plantedPoseLatched)
            {
                return true;
            }

            Vector3 leftLocal = chair.transform.InverseTransformPoint(left);
            Vector3 rightLocal = chair.transform.InverseTransformPoint(right);
            if (!IsFinite(leftLocal) || !IsFinite(rightLocal))
            {
                return false;
            }

            bool sameCandidate = pendingPlantChairCollider == chair &&
                TransformDistance(
                    chair.transform,
                    pendingPlantLeftLocal,
                    leftLocal) <= PlantStabilityTolerance &&
                TransformDistance(
                    chair.transform,
                    pendingPlantRightLocal,
                    rightLocal) <= PlantStabilityTolerance;
            if (pendingPlantEvidenceFrame == Time.frameCount)
            {
                if (!sameCandidate)
                {
                    // A second evaluation in this render frame may invalidate
                    // the prior evidence, but it cannot become another evidence
                    // sample or replace the candidate until the next frame.
                    plantEvidenceCount = 0;
                    contactCalibrationReady = false;
                }
                return true;
            }

            pendingPlantEvidenceFrame = Time.frameCount;
            if (sameCandidate)
            {
                plantEvidenceCount++;
            }
            else
            {
                pendingPlantChairCollider = chair;
                pendingPlantLeftLocal = leftLocal;
                pendingPlantRightLocal = rightLocal;
                plantEvidenceCount = 1;
                contactCalibrationReady = false;
            }

            contactCalibrationReady = plantEvidenceCount >= PlantEvidenceFrames;
            return true;
        }

        private void TryLatchRenderedContact(
            ShowcaseChairGripPhase phase,
            float weight,
            BoxCollider chair,
            GripPose requestedGoal)
        {
            if (plantedPoseLatched || !contactCalibrationReady ||
                phase != ShowcaseChairGripPhase.Holding ||
                requestedPhase != ShowcaseChairGripPhase.Holding ||
                weight < 1f - WeightEpsilon ||
                evaluatedFrame != Time.frameCount ||
                chair == null || chair != trackedChairCollider ||
                leftHand == null || rightHand == null)
            {
                return;
            }

            Vector3 leftContact = leftHand.position;
            Vector3 rightContact = rightHand.position;
            if (!IsFinite(leftContact) || !IsFinite(rightContact) ||
                Vector3.Distance(leftContact, requestedGoal.LeftHand) >
                    RenderedContactGoalEnvelope ||
                Vector3.Distance(rightContact, requestedGoal.RightHand) >
                    RenderedContactGoalEnvelope)
            {
                return;
            }

            Vector3 leftLocal = chair.transform.InverseTransformPoint(leftContact);
            Vector3 rightLocal = chair.transform.InverseTransformPoint(rightContact);
            if (!IsFinite(leftLocal) || !IsFinite(rightLocal))
            {
                return;
            }

            // This is a rendered-contact diagnostic, not an IK control target.
            // Capture both hands atomically once; never feed these points back
            // into TryBuildGripPose or into the Chair/actor transforms.
            plantedChairCollider = chair;
            plantedLeftLocal = leftLocal;
            plantedRightLocal = rightLocal;
            plantedPoseLatched = true;
        }

        private bool TryGetRenderedContact(
            ShowcaseChairGripPhase phase,
            BoxCollider diagnosticTarget,
            out Vector3 left,
            out Vector3 right)
        {
            if (phase == ShowcaseChairGripPhase.BlendOut)
            {
                left = releaseRenderedContact.LeftHand;
                right = releaseRenderedContact.RightHand;
                return releaseRenderedContactValid &&
                    IsFinite(left) && IsFinite(right);
            }

            if (!TryGetActiveRenderedContact(
                    diagnosticTarget,
                    out GripPose contact))
            {
                left = Vector3.zero;
                right = Vector3.zero;
                return false;
            }

            left = contact.LeftHand;
            right = contact.RightHand;
            return true;
        }

        private bool TryGetActiveRenderedContact(
            BoxCollider chair,
            out GripPose contact)
        {
            contact = default;
            if (!plantedPoseLatched || chair == null ||
                plantedChairCollider != chair)
            {
                return false;
            }

            Vector3 left = chair.transform.TransformPoint(plantedLeftLocal);
            Vector3 right = chair.transform.TransformPoint(plantedRightLocal);
            if (!IsFinite(left) || !IsFinite(right))
            {
                return false;
            }

            contact = new GripPose(left, right);
            return true;
        }

        private static float TransformDistance(
            Transform source,
            Vector3 firstLocal,
            Vector3 secondLocal)
        {
            Vector3 worldDelta = source.TransformVector(secondLocal - firstLocal);
            return IsFinite(worldDelta) ? worldDelta.magnitude : float.PositiveInfinity;
        }

        private static Vector3 ProjectToArmReach(
            Vector3 upperArmPosition,
            Vector3 idealTarget,
            float chainLength)
        {
            Vector3 displacement = idealTarget - upperArmPosition;
            float distance = displacement.magnitude;
            float maximumReach = chainLength - ReachBendMargin;
            if (!IsFinite(distance) || !IsFinite(maximumReach) ||
                maximumReach <= WeightEpsilon)
            {
                return new Vector3(float.NaN, float.NaN, float.NaN);
            }

            if (distance <= maximumReach || distance <= WeightEpsilon)
            {
                return idealTarget;
            }

            return upperArmPosition + displacement * (maximumReach / distance);
        }

        private void ApplyHandGoal(
            AvatarIKGoal goal,
            Vector3 position,
            float weight)
        {
            animator.SetIKPositionWeight(goal, weight);
            animator.SetIKRotationWeight(goal, 0f);
            animator.SetIKPosition(goal, position);
        }

        private void ApplyZeroWeights()
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);
            animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0f);
            animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0f);
        }

        private void StageAppliedDiagnostics(
            ShowcaseChairGripPhase phase,
            float weight,
            BoxCollider target,
            GripPose pose)
        {
            evaluatedFrame = Time.frameCount;
            evaluatedPhase = phase;
            evaluatedWeight = weight;
            evaluatedBlendStartCount = requestedBlendStartCount;
            evaluatedTarget = target;
            evaluatedPose = pose;
        }

        private void ClearRequestedGrip()
        {
            trackedChairCollider = null;
            releaseChairCollider = null;
            releasePose = default;
            ClearRenderedContactState();
            requestedPhase = ShowcaseChairGripPhase.None;
            requestedWeight = 0f;
        }

        private void ClearRenderedContactState()
        {
            ClearActiveRenderedContact();
            releaseRenderedContact = default;
            releaseRenderedContactValid = false;
        }

        private void ClearActiveRenderedContact()
        {
            plantedChairCollider = null;
            plantedLeftLocal = Vector3.zero;
            plantedRightLocal = Vector3.zero;
            plantedPoseLatched = false;
            contactCalibrationReady = false;
            pendingPlantChairCollider = null;
            pendingPlantLeftLocal = Vector3.zero;
            pendingPlantRightLocal = Vector3.zero;
            plantEvidenceCount = 0;
            pendingPlantEvidenceFrame = -1;
        }

        private void ResetAppliedDiagnostics()
        {
            evaluatedFrame = -1;
            evaluatedPhase = ShowcaseChairGripPhase.None;
            evaluatedWeight = 0f;
            evaluatedBlendStartCount = 0;
            evaluatedTarget = null;
            evaluatedPose = default;
            Phase = ShowcaseChairGripPhase.None;
            CurrentWeight = 0f;
            TargetCollider = null;
            LeftGripTarget = Vector3.zero;
            RightGripTarget = Vector3.zero;
            LeftHandError = 0f;
            RightHandError = 0f;
            HasRenderedGripContact = false;
            LeftRenderedGripContact = Vector3.zero;
            RightRenderedGripContact = Vector3.zero;
            LeftRenderedGripContactError = 0f;
            RightRenderedGripContactError = 0f;
            LastAppliedFrame = -1;
            BlendStartCount = 0;
        }

        private void ResolveBones()
        {
            if (animator == null || !animator.isHuman)
            {
                leftHand = null;
                rightHand = null;
                leftUpperArm = null;
                rightUpperArm = null;
                leftLowerArm = null;
                rightLowerArm = null;
                leftArmLength = 0f;
                rightArmLength = 0f;
                return;
            }

            leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            leftArmLength = CalculateArmLength(leftUpperArm, leftLowerArm, leftHand);
            rightArmLength = CalculateArmLength(rightUpperArm, rightLowerArm, rightHand);
        }

        private static float CalculateArmLength(
            Transform upperArm,
            Transform lowerArm,
            Transform hand)
        {
            if (upperArm == null || lowerArm == null || hand == null)
            {
                return 0f;
            }

            float upperLength = Vector3.Distance(upperArm.position, lowerArm.position);
            float lowerLength = Vector3.Distance(lowerArm.position, hand.position);
            float total = upperLength + lowerLength;
            return IsFinite(total) ? total : 0f;
        }

        private static bool IsValidArmLength(float value)
        {
            return IsFinite(value) && value > ReachBendMargin + WeightEpsilon;
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private readonly struct GripPose
        {
            public GripPose(Vector3 leftHand, Vector3 rightHand)
            {
                LeftHand = leftHand;
                RightHand = rightHand;
            }

            public Vector3 LeftHand { get; }
            public Vector3 RightHand { get; }
        }
    }
}
