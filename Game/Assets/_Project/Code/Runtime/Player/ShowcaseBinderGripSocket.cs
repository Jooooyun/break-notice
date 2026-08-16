using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    [DisallowMultipleComponent]
    public sealed class ShowcaseBinderGripSocket : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

        [SerializeField]
        private Transform actorRoot;

        [SerializeField]
        private Vector3 localGripOffset = new Vector3(0f, -0.13f, 0.03f);

        [SerializeField]
        private Vector3 localGripRotationEuler = new Vector3(0f, 90f, 0f);

        private Transform rightHand;
        private Transform rightLowerArm;

        public bool IsValid => animator != null && animator.isHuman &&
            rightHand != null && rightLowerArm != null;

        public Vector3 WorldPosition { get; private set; }

        public Quaternion WorldRotation { get; private set; } = Quaternion.identity;

        private void Awake()
        {
            ResolveBones();
            RefreshPose();
        }

        private void LateUpdate()
        {
            RefreshPose();
        }

        public void Configure(
            Animator authoredAnimator,
            Transform authoredActorRoot,
            Vector3 authoredLocalOffset,
            Vector3 authoredLocalRotationEuler)
        {
            animator = authoredAnimator;
            actorRoot = authoredActorRoot != null ? authoredActorRoot : transform;
            localGripOffset = authoredLocalOffset;
            localGripRotationEuler = authoredLocalRotationEuler;
            ResolveBones();
            RefreshPose();
        }

        public bool TryGetWorldPose(out Vector3 position, out Quaternion rotation)
        {
            RefreshPose();
            position = WorldPosition;
            rotation = WorldRotation;
            return IsValid;
        }

        private void ResolveBones()
        {
            if (animator == null || !animator.isHuman)
            {
                rightHand = null;
                rightLowerArm = null;
                return;
            }

            rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        }

        private void RefreshPose()
        {
            if (!IsValid)
            {
                return;
            }

            Vector3 forearmUp = rightHand.position - rightLowerArm.position;
            if (forearmUp.sqrMagnitude <= 0.000001f)
            {
                forearmUp = rightHand.up;
            }
            forearmUp.Normalize();

            Vector3 referenceForward = actorRoot != null
                ? actorRoot.forward
                : transform.forward;
            Vector3 gripForward = Vector3.ProjectOnPlane(referenceForward, forearmUp);
            if (gripForward.sqrMagnitude <= 0.000001f)
            {
                gripForward = Vector3.ProjectOnPlane(rightHand.forward, forearmUp);
            }
            if (gripForward.sqrMagnitude <= 0.000001f)
            {
                gripForward = Vector3.Cross(forearmUp, Vector3.right);
            }
            gripForward.Normalize();

            Quaternion forearmFrame = Quaternion.LookRotation(gripForward, forearmUp);
            WorldRotation = forearmFrame * Quaternion.Euler(localGripRotationEuler);
            WorldPosition = rightHand.position + forearmFrame * localGripOffset;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                ResolveBones();
                RefreshPose();
            }
            if (!IsValid)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(WorldPosition, new Vector3(0.29f, 0.32f, 0.08f));
            Gizmos.DrawLine(rightLowerArm.position, rightHand.position);
            Gizmos.DrawRay(WorldPosition, WorldRotation * Vector3.forward * 0.25f);
        }
#endif
    }
}
