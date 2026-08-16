using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    [DisallowMultipleComponent]
    public sealed class ShowcaseChairGripRig : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

        [SerializeField, Range(0.1f, 1f)]
        private float backrestHeight = 0.78f;

        [SerializeField, Range(0.05f, 0.5f)]
        private float handInset = 0.22f;

        private BoxCollider chairCollider;
        private Transform leftHand;
        private Transform rightHand;

        public bool IsActive => chairCollider != null;
        public bool IsAvailable => animator != null && animator.isHuman &&
            leftHand != null && rightHand != null;

        private void Awake()
        {
            ResolveBones();
        }

        public void Configure(Animator authoredAnimator)
        {
            animator = authoredAnimator;
            ResolveBones();
        }

        public bool BeginGrip(BoxCollider authoredChair)
        {
            ResolveBones();
            if (!IsAvailable || authoredChair == null)
            {
                chairCollider = null;
                return false;
            }

            chairCollider = authoredChair;
            return true;
        }

        public void EndGrip()
        {
            chairCollider = null;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!IsActive || !IsAvailable)
            {
                return;
            }

            GetGripPositions(out Vector3 left, out Vector3 right);
            Quaternion rotation = Quaternion.LookRotation(
                -chairCollider.transform.forward,
                Vector3.up);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0.75f);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, left);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, rotation);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0.75f);
            animator.SetIKPosition(AvatarIKGoal.RightHand, right);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rotation);
        }

        public void GetGripPositions(out Vector3 left, out Vector3 right)
        {
            Bounds bounds = chairCollider.bounds;
            Vector3 center = bounds.center;
            Vector3 up = chairCollider.transform.up;
            Vector3 side = chairCollider.transform.right;
            float vertical = bounds.extents.y * backrestHeight;
            float lateral = Mathf.Max(0.08f, bounds.extents.x * (1f - handInset));
            Vector3 backrestCenter = center + up * vertical;
            left = backrestCenter - side * lateral;
            right = backrestCenter + side * lateral;
        }

        private void ResolveBones()
        {
            if (animator == null || !animator.isHuman)
            {
                leftHand = null;
                rightHand = null;
                return;
            }
            leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        }
    }
}
