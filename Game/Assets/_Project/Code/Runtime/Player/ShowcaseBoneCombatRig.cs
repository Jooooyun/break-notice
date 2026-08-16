using System;
using System.Collections.Generic;
using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Gameplay.Combat
{
    public enum ShowcaseHurtboxRegion
    {
        Head,
        Torso,
        Pelvis,
        LeftArm,
        RightArm,
        LeftLeg,
        RightLeg
    }

    [DefaultExecutionOrder(-280)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseBoneCombatRig : MonoBehaviour
    {
        private const float ContactTolerance = 0.015f;
        private readonly struct PendingContact
        {
            public PendingContact(ShowcaseBoneCombatRig victim, uint sequence)
            {
                Victim = victim;
                Sequence = sequence;
            }

            public ShowcaseBoneCombatRig Victim { get; }
            public uint Sequence { get; }
        }
        [Serializable]
        private sealed class HurtboxBinding
        {
            [SerializeField]
            private ShowcaseHurtboxRegion region;

            [SerializeField]
            private HumanBodyBones bone;

            [SerializeField, Min(0.01f)]
            private float radius = 0.2f;

            [NonSerialized]
            private Transform boneTransform;

            [NonSerialized]
            private Transform tipTransform;

            public ShowcaseHurtboxRegion Region => region;
            public float Radius => radius;
            public bool IsBound => boneTransform != null;
            public Vector3 WorldCenter => tipTransform != null
                ? tipTransform.position
                : boneTransform != null
                ? boneTransform.position
                : Vector3.zero;

            public HurtboxBinding(
                ShowcaseHurtboxRegion authoredRegion,
                HumanBodyBones authoredBone,
                float authoredRadius)
            {
                region = authoredRegion;
                bone = authoredBone;
                radius = Mathf.Max(0.01f, authoredRadius);
                boneTransform = null;
            }

            public bool Bind(Animator authoredAnimator)
            {
                boneTransform = authoredAnimator != null && authoredAnimator.isHuman
                    ? authoredAnimator.GetBoneTransform(bone)
                    : null;
                tipTransform = authoredAnimator != null && authoredAnimator.isHuman
                    ? authoredAnimator.GetBoneTransform(ResolveTipBone(bone))
                    : null;
                return boneTransform != null;
            }

            private static HumanBodyBones ResolveTipBone(HumanBodyBones source)
            {
                return source switch
                {
                    HumanBodyBones.LeftHand => HumanBodyBones.LeftMiddleDistal,
                    HumanBodyBones.RightHand => HumanBodyBones.RightMiddleDistal,
                    HumanBodyBones.LeftFoot => HumanBodyBones.LeftToes,
                    HumanBodyBones.RightFoot => HumanBodyBones.RightToes,
                    _ => source
                };
            }
        }

        [Serializable]
        private sealed class StrikeBinding
        {
            [SerializeField]
            private HumanBodyBones bone;

            [SerializeField, Min(0.01f)]
            private float radius = 0.18f;

            [SerializeField, Min(0f)]
            private float forwardOffset = 0.2f;

            [NonSerialized]
            private Transform boneTransform;

            public HumanBodyBones Bone => bone;
            public float Radius => radius;
            public float ForwardOffset => forwardOffset;
            public bool IsBound => boneTransform != null;
            public Vector3 WorldCenter => boneTransform != null
                ? boneTransform.position
                : Vector3.zero;

            public StrikeBinding(
                HumanBodyBones authoredBone,
                float authoredRadius,
                float authoredForwardOffset)
            {
                bone = authoredBone;
                radius = Mathf.Max(0.01f, authoredRadius);
                forwardOffset = Mathf.Max(0f, authoredForwardOffset);
                boneTransform = null;
            }

            public bool Bind(Animator authoredAnimator)
            {
                boneTransform = authoredAnimator != null && authoredAnimator.isHuman
                    ? authoredAnimator.GetBoneTransform(bone)
                    : null;
                return boneTransform != null;
            }
        }

        private static readonly List<ShowcaseBoneCombatRig> ActiveRigs = new();

        [SerializeField]
        private ShowcaseCombatTeam team;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private ShowcasePlayerCombatController playerCombat;

        [SerializeField]
        private ShowcaseRunnerAgent runnerAgent;

        [SerializeField]
        private HurtboxBinding[] hurtboxes = Array.Empty<HurtboxBinding>();

        [SerializeField]
        private StrikeBinding[] strikes = Array.Empty<StrikeBinding>();

        [SerializeField]
        private bool debugDrawVolumes;

        private readonly HashSet<int> contactedVictims = new();
        private readonly List<PendingContact> pendingContacts = new();
        private readonly int[] activeStrikeIndices = new int[4];
        private readonly Vector3[] previousStrikePositions = new Vector3[4];
        private int activeStrikeCount;
        private uint activeAttackSequence;

        public ShowcaseCombatTeam Team => team;
        public Animator Animator => animator;
        public ShowcasePlayerCombatController PlayerCombat => playerCombat;
        public ShowcaseRunnerAgent RunnerAgent => runnerAgent;
        public bool IsAttackWindowOpen { get; private set; }
        public int AttackWindowOpenCount { get; private set; }
        public int AttackWindowCloseCount { get; private set; }
        public int ContactCount { get; private set; }
        public int AcceptedHitCount { get; private set; }
        public uint ActiveAttackSequence => activeAttackSequence;
        public float MinimumWindowSurfaceSeparation { get; private set; } =
            float.PositiveInfinity;

        private void Awake()
        {
            animator ??= GetComponentInChildren<Animator>(true);
            playerCombat ??= GetComponent<ShowcasePlayerCombatController>();
            runnerAgent ??= GetComponent<ShowcaseRunnerAgent>();
            hurtboxes = Array.Empty<HurtboxBinding>();
            strikes = Array.Empty<StrikeBinding>();
            EnsureDefaultDefinitions();
            BindHumanoidBones();
        }

        private void OnEnable()
        {
            if (!ActiveRigs.Contains(this))
            {
                ActiveRigs.Add(this);
            }
        }

        private void OnDisable()
        {
            CloseAttackWindow();
            ActiveRigs.Remove(this);
        }

        private void LateUpdate()
        {
            if (!IsAttackWindowOpen || activeStrikeCount <= 0)
            {
                return;
            }

            for (int activeIndex = 0; activeIndex < activeStrikeCount; activeIndex++)
            {
                int strikeIndex = activeStrikeIndices[activeIndex];
                if (strikeIndex < 0 || strikeIndex >= strikes.Length ||
                    !strikes[strikeIndex].IsBound)
                {
                    continue;
                }

                StrikeBinding strike = strikes[strikeIndex];
                Vector3 current = strike.WorldCenter +
                    transform.forward * strike.ForwardOffset;
                SweepStrike(previousStrikePositions[activeIndex], current, strike.Radius);
                previousStrikePositions[activeIndex] = current;
            }
        }

        private void Update()
        {
            if (pendingContacts.Count == 0)
            {
                return;
            }

            for (int index = 0; index < pendingContacts.Count; index++)
            {
                PendingContact contact = pendingContacts[index];
                if (TryCommitActualContact(contact.Victim, contact.Sequence))
                {
                    AcceptedHitCount++;
                }
            }
            pendingContacts.Clear();
        }

        public void Configure(
            Animator authoredAnimator,
            ShowcaseCombatTeam authoredTeam,
            ShowcasePlayerCombatController authoredPlayerCombat,
            ShowcaseRunnerAgent authoredRunnerAgent)
        {
            animator = authoredAnimator;
            team = authoredTeam;
            playerCombat = authoredPlayerCombat;
            runnerAgent = authoredRunnerAgent;
            hurtboxes = Array.Empty<HurtboxBinding>();
            strikes = Array.Empty<StrikeBinding>();
            EnsureDefaultDefinitions();
            BindHumanoidBones();
        }

        public bool OpenAttackWindow()
        {
            if (animator == null || !animator.isHuman ||
                !TryResolveActiveStrikeBones(out HumanBodyBones[] bones,
                    out activeAttackSequence))
            {
                CloseAttackWindow();
                return false;
            }

            contactedVictims.Clear();
            MinimumWindowSurfaceSeparation = float.PositiveInfinity;
            activeStrikeCount = 0;
            for (int boneIndex = 0;
                 boneIndex < bones.Length && activeStrikeCount < activeStrikeIndices.Length;
                 boneIndex++)
            {
                int strikeIndex = FindStrikeIndex(bones[boneIndex]);
                if (strikeIndex < 0 || !strikes[strikeIndex].IsBound)
                {
                    continue;
                }

                activeStrikeIndices[activeStrikeCount] = strikeIndex;
                previousStrikePositions[activeStrikeCount] =
                    strikes[strikeIndex].WorldCenter +
                    transform.forward * strikes[strikeIndex].ForwardOffset;
                activeStrikeCount++;
            }

            IsAttackWindowOpen = activeStrikeCount > 0;
            if (IsAttackWindowOpen)
            {
                AttackWindowOpenCount++;
                playerCombat?.NotifyAnimationAttackWindowOpened();
            }
            return IsAttackWindowOpen;
        }

        public void CloseAttackWindow()
        {
            if (IsAttackWindowOpen)
            {
                AttackWindowCloseCount++;
            }
            IsAttackWindowOpen = false;
            activeStrikeCount = 0;
            activeAttackSequence = 0;
            contactedVictims.Clear();
        }

        public void ResetRuntimeState()
        {
            CloseAttackWindow();
            AttackWindowOpenCount = 0;
            AttackWindowCloseCount = 0;
            ContactCount = 0;
            AcceptedHitCount = 0;
            pendingContacts.Clear();
            MinimumWindowSurfaceSeparation = float.PositiveInfinity;
        }

        public static bool SweptSphereOverlapsSphere(
            Vector3 segmentStart,
            Vector3 segmentEnd,
            float sweepRadius,
            Vector3 sphereCenter,
            float sphereRadius)
        {
            Vector3 segment = segmentEnd - segmentStart;
            float lengthSquared = segment.sqrMagnitude;
            float t = lengthSquared <= 0.000001f
                ? 0f
                : Mathf.Clamp01(Vector3.Dot(
                    sphereCenter - segmentStart,
                    segment) / lengthSquared);
            Vector3 closest = segmentStart + segment * t;
            float combinedRadius = Mathf.Max(0f, sweepRadius) +
                Mathf.Max(0f, sphereRadius) + ContactTolerance;
            return (sphereCenter - closest).sqrMagnitude <=
                combinedRadius * combinedRadius;
        }

        private void SweepStrike(Vector3 start, Vector3 end, float radius)
        {
            if (playerCombat != null && contactedVictims.Count > 0)
            {
                return;
            }

            for (int rigIndex = 0; rigIndex < ActiveRigs.Count; rigIndex++)
            {
                ShowcaseBoneCombatRig victim = ActiveRigs[rigIndex];
                if (victim == null || victim == this || victim.team == team ||
                    contactedVictims.Contains(victim.GetInstanceID()))
                {
                    continue;
                }

                bool overlaps = false;
                for (int hurtboxIndex = 0;
                     hurtboxIndex < victim.hurtboxes.Length;
                     hurtboxIndex++)
                {
                    HurtboxBinding hurtbox = victim.hurtboxes[hurtboxIndex];
                    if (!hurtbox.IsBound)
                    {
                        continue;
                    }

                    Vector3 segment = end - start;
                    float lengthSquared = segment.sqrMagnitude;
                    float t = lengthSquared <= 0.000001f
                        ? 0f
                        : Mathf.Clamp01(Vector3.Dot(
                            hurtbox.WorldCenter - start,
                            segment) / lengthSquared);
                    Vector3 closest = start + segment * t;
                    MinimumWindowSurfaceSeparation = Mathf.Min(
                        MinimumWindowSurfaceSeparation,
                        Vector3.Distance(hurtbox.WorldCenter, closest) -
                        radius - hurtbox.Radius);
                    if (!SweptSphereOverlapsSphere(
                            start,
                            end,
                            radius,
                            hurtbox.WorldCenter,
                            hurtbox.Radius))
                    {
                        continue;
                    }

                    overlaps = true;
                    break;
                }

                if (!overlaps)
                {
                    continue;
                }

                contactedVictims.Add(victim.GetInstanceID());
                ContactCount++;
                pendingContacts.Add(new PendingContact(victim, activeAttackSequence));
            }
        }

        private bool TryCommitActualContact(ShowcaseBoneCombatRig victim, uint sequence)
        {
            if (playerCombat != null)
            {
                return playerCombat.TryCommitBoneHit(
                    victim.runnerAgent,
                    sequence);
            }
            if (runnerAgent != null)
            {
                return runnerAgent.TryCommitBoneHit(
                    victim.playerCombat,
                    sequence);
            }
            return false;
        }

        private bool TryResolveActiveStrikeBones(
            out HumanBodyBones[] bones,
            out uint sequence)
        {
            if (playerCombat != null && playerCombat.IsMeleeActionActive)
            {
                sequence = playerCombat.ActiveAttackSequence;
                bones = playerCombat.ActiveMeleeStep switch
                {
                    1 => new[] { HumanBodyBones.RightHand },
                    2 => new[] { HumanBodyBones.RightHand },
                    _ => new[] { HumanBodyBones.RightHand }
                };
                return sequence != 0;
            }

            if (runnerAgent != null && runnerAgent.CanOpenAttackWindow)
            {
                sequence = runnerAgent.ActiveAttackSequence;
                bones = runnerAgent.AnimationCue switch
                {
                    ShowcaseRunnerAnimationCue.Kicking =>
                        new[] { HumanBodyBones.LeftFoot },
                    ShowcaseRunnerAnimationCue.Kicking2 =>
                        new[] { HumanBodyBones.RightFoot },
                    ShowcaseRunnerAnimationCue.EliteJumpAttack =>
                        new[] { HumanBodyBones.RightHand, HumanBodyBones.LeftHand },
                    ShowcaseRunnerAnimationCue.EliteMutantSwiping =>
                        new[] { HumanBodyBones.RightHand, HumanBodyBones.LeftHand },
                    _ => new[] { HumanBodyBones.RightHand }
                };
                return sequence != 0;
            }

            sequence = 0;
            bones = Array.Empty<HumanBodyBones>();
            return false;
        }

        private int FindStrikeIndex(HumanBodyBones bone)
        {
            for (int index = 0; index < strikes.Length; index++)
            {
                if (strikes[index].Bone == bone)
                {
                    return index;
                }
            }
            return -1;
        }

        private void EnsureDefaultDefinitions()
        {
            if (hurtboxes == null || hurtboxes.Length == 0)
            {
                hurtboxes = new[]
                {
                    new HurtboxBinding(
                        ShowcaseHurtboxRegion.Head,
                        HumanBodyBones.Head,
                        0.16f),
                    new HurtboxBinding(
                        ShowcaseHurtboxRegion.Torso,
                        HumanBodyBones.Chest,
                        0.32f),
                    new HurtboxBinding(
                        ShowcaseHurtboxRegion.Pelvis,
                        HumanBodyBones.Hips,
                        0.23f),
                    new HurtboxBinding(
                        ShowcaseHurtboxRegion.LeftArm,
                        HumanBodyBones.LeftLowerArm,
                        0.13f),
                    new HurtboxBinding(
                        ShowcaseHurtboxRegion.RightArm,
                        HumanBodyBones.RightLowerArm,
                        0.13f),
                    new HurtboxBinding(
                        ShowcaseHurtboxRegion.LeftLeg,
                        HumanBodyBones.LeftLowerLeg,
                        0.14f),
                    new HurtboxBinding(
                        ShowcaseHurtboxRegion.RightLeg,
                        HumanBodyBones.RightLowerLeg,
                        0.14f)
                };
            }

            if (strikes == null || strikes.Length == 0)
            {
                strikes = new[]
                {
                    new StrikeBinding(HumanBodyBones.LeftHand, 0.16f, 0.07f),
                    new StrikeBinding(HumanBodyBones.RightHand, 0.16f, 0.07f),
                    new StrikeBinding(HumanBodyBones.LeftFoot, 0.18f, 0.05f),
                    new StrikeBinding(HumanBodyBones.RightFoot, 0.18f, 0.05f)
                };
            }
        }

        private void BindHumanoidBones()
        {
            if (animator == null || !animator.isHuman)
            {
                return;
            }

            for (int index = 0; index < hurtboxes.Length; index++)
            {
                hurtboxes[index].Bind(animator);
            }
            for (int index = 0; index < strikes.Length; index++)
            {
                strikes[index].Bind(animator);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!debugDrawVolumes)
            {
                return;
            }
            Animator previewAnimator = animator != null
                ? animator
                : GetComponentInChildren<Animator>(true);
            if (previewAnimator == null || !previewAnimator.isHuman)
            {
                return;
            }

            EnsureDefaultDefinitions();
            BindHumanoidBones();
            Gizmos.color = new Color(0.1f, 0.75f, 1f, 0.45f);
            for (int index = 0; index < hurtboxes.Length; index++)
            {
                if (hurtboxes[index].IsBound)
                {
                    Gizmos.DrawWireSphere(
                        hurtboxes[index].WorldCenter,
                        hurtboxes[index].Radius);
                }
            }

            Gizmos.color = new Color(1f, 0.2f, 0.1f, 0.65f);
            for (int index = 0; index < strikes.Length; index++)
            {
                if (strikes[index].IsBound)
                {
                    Gizmos.DrawWireSphere(
                        strikes[index].WorldCenter +
                        transform.forward * strikes[index].ForwardOffset,
                        strikes[index].Radius);
                }
            }
        }
#endif
    }
}
