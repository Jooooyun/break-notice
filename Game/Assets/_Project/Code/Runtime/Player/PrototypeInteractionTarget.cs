using System;
using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    public readonly struct PrototypeInteractionCollisionSnapshot
    {
        public PrototypeInteractionCollisionSnapshot(
            PrototypeInteractionTarget target,
            Collider otherCollider,
            Vector3 point,
            Vector3 normal,
            float relativeSpeed,
            int frame,
            uint sequence)
        {
            Target = target;
            OtherCollider = otherCollider;
            Point = point;
            Normal = normal;
            RelativeSpeed = Mathf.Max(0f, relativeSpeed);
            Frame = frame;
            Sequence = sequence;
        }

        public PrototypeInteractionTarget Target { get; }

        public Collider OtherCollider { get; }

        public Vector3 Point { get; }

        public Vector3 Normal { get; }

        public float RelativeSpeed { get; }

        public int Frame { get; }

        public uint Sequence { get; }
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeInteractionTarget : MonoBehaviour
    {
        [SerializeField]
        private string contentDefinitionId;

        [SerializeField]
        private string runtimeInstanceKey;

        [SerializeField]
        private string displayName;

        [SerializeField]
        private PrototypeWeightClass weightClass;

        [SerializeField]
        private PrototypeInteractionRole role;

        [SerializeField]
        private bool directAcquisitionEligible;

        [SerializeField]
        private bool catchEligible;

        [SerializeField]
        private bool strapEligible;

        [SerializeField]
        private bool hasLoadBearingMetalAnchor;

        [SerializeField]
        private Transform strapClampSocket;

        private int committedPlayerThrowUses;
        private uint lastCommittedPlayerThrowSequence;
        private uint lastCommittedCargoStrapSequence;
        private bool throwDepletionPending;
        private bool throwDepletionCollisionObserved;
        private bool deferredCargoStrapDepletion;
        private uint collisionSequence;
        private Collider lastCollisionOther;
        private float lastCollisionRelativeSpeed;
        private uint observedCargoStrapAttackSequence;
        private Transform cargoStrapIgnoredHierarchy;
        private float cargoStrapMinimumCollisionSpeed;
        private bool cargoStrapCollisionLatched;
        private Collider cargoStrapCollisionOther;
        private float cargoStrapCollisionSpeed;
        private PhysicalReleaseSnapshot lastPhysicalReleaseImpactSnapshot;
        private float lastPhysicalReleaseKineticEnergy;

        public string ContentDefinitionId => contentDefinitionId;

        public string RuntimeInstanceKey => runtimeInstanceKey;

        public string DisplayName => displayName;

        public PrototypeWeightClass WeightClass => weightClass;

        public PrototypeInteractionRole Role => role;

        public bool DirectAcquisitionEligible =>
            directAcquisitionEligible && !IsPlayerThrowExhausted;

        public bool CatchEligible => catchEligible && !IsPlayerThrowExhausted;

        public bool StrapEligible => strapEligible && !IsPlayerThrowExhausted;

        public bool HasLoadBearingMetalAnchor => hasLoadBearingMetalAnchor;

        public Transform StrapClampSocket => strapClampSocket;

        public bool HasStrapClampSocket => strapClampSocket != null;

        public bool IsCargoStrapEligible => StrapEligible &&
            weightClass != PrototypeWeightClass.Heavy &&
            (weightClass == PrototypeWeightClass.Light
                ? HasStrapClampSocket
                : hasLoadBearingMetalAnchor);

        public int MaximumPlayerThrowUses => weightClass switch
        {
            PrototypeWeightClass.Light => 1,
            PrototypeWeightClass.Medium => 2,
            PrototypeWeightClass.Heavy => 3,
            _ => 1
        };

        public int CommittedPlayerThrowUses => committedPlayerThrowUses;

        public int RemainingPlayerThrowUses =>
            Mathf.Max(0, MaximumPlayerThrowUses - committedPlayerThrowUses);

        public bool IsPlayerThrowExhausted => RemainingPlayerThrowUses == 0;

        public int MaximumPlayerUses => MaximumPlayerThrowUses;

        public int CommittedPlayerUses => CommittedPlayerThrowUses;

        public int RemainingPlayerUses => RemainingPlayerThrowUses;

        public bool IsPlayerUseExhausted => IsPlayerThrowExhausted;

        public bool IsThrowDepletionPending => throwDepletionPending;

        public bool IsCargoStrapDepletionDeferred =>
            deferredCargoStrapDepletion;

        public uint LastCommittedPlayerThrowSequence =>
            lastCommittedPlayerThrowSequence;

        public uint LastCommittedCargoStrapSequence =>
            lastCommittedCargoStrapSequence;

        public uint CollisionSequence => collisionSequence;

        public Collider LastCollisionOther => lastCollisionOther;

        public float LastCollisionRelativeSpeed => lastCollisionRelativeSpeed;

        public PhysicalReleaseSnapshot LastPhysicalReleaseImpactSnapshot =>
            lastPhysicalReleaseImpactSnapshot;

        public float LastPhysicalReleaseKineticEnergy =>
            lastPhysicalReleaseKineticEnergy;

        public event Action<PrototypeInteractionCollisionSnapshot> CollisionEntered;

        public bool IsScoopEligible =>
            DirectAcquisitionEligible &&
            (weightClass == PrototypeWeightClass.Light ||
             weightClass == PrototypeWeightClass.Medium) &&
            role != PrototypeInteractionRole.Drawer;

        private void OnCollisionEnter(Collision collision)
        {
            collisionSequence++;
            lastCollisionOther = collision != null ? collision.collider : null;
            lastCollisionRelativeSpeed = collision != null
                ? collision.relativeVelocity.magnitude
                : 0f;
            ContactPoint contact = collision != null && collision.contactCount > 0
                ? collision.GetContact(0)
                : default;
            CollisionEntered?.Invoke(new PrototypeInteractionCollisionSnapshot(
                this,
                lastCollisionOther,
                collision != null && collision.contactCount > 0
                    ? contact.point
                    : transform.position,
                collision != null && collision.contactCount > 0
                    ? contact.normal
                    : Vector3.zero,
                lastCollisionRelativeSpeed,
                Time.frameCount,
                collisionSequence));
            TryLatchCargoStrapCollision(
                lastCollisionOther,
                lastCollisionRelativeSpeed);
            if (throwDepletionPending && !deferredCargoStrapDepletion)
            {
                throwDepletionCollisionObserved = true;
            }
        }

        private void LateUpdate()
        {
            if (!throwDepletionPending || !throwDepletionCollisionObserved)
            {
                return;
            }

            throwDepletionPending = false;
            throwDepletionCollisionObserved = false;
            gameObject.SetActive(false);
        }

        public void Configure(
            string definitionId,
            string instanceKey,
            string readableName,
            PrototypeWeightClass assignedWeightClass,
            PrototypeInteractionRole assignedRole,
            bool canDirectlyAcquire,
            bool canCatch,
            bool canStrap,
            bool hasValidAnchor)
        {
            if (string.IsNullOrWhiteSpace(definitionId))
            {
                throw new ArgumentException("A stable content definition ID is required.", nameof(definitionId));
            }

            if (string.IsNullOrWhiteSpace(instanceKey))
            {
                throw new ArgumentException("A distinct runtime instance key is required.", nameof(instanceKey));
            }

            if (string.IsNullOrWhiteSpace(readableName))
            {
                throw new ArgumentException("A readable target name is required.", nameof(readableName));
            }

            contentDefinitionId = definitionId;
            runtimeInstanceKey = instanceKey;
            displayName = readableName;
            weightClass = assignedWeightClass;
            role = assignedRole;
            directAcquisitionEligible = canDirectlyAcquire;
            catchEligible = canCatch;
            strapEligible = canStrap;
            hasLoadBearingMetalAnchor = hasValidAnchor;
            RestorePlayerThrowUseBaseline();
        }

        public void ConfigureStrapClampSocket(Transform socket)
        {
            strapClampSocket = socket;
        }

        internal bool TryCommitSuccessfulPlayerThrow(uint sequence)
        {
            if (sequence == 0 ||
                sequence == lastCommittedPlayerThrowSequence ||
                IsPlayerThrowExhausted)
            {
                return false;
            }

            lastCommittedPlayerThrowSequence = sequence;
            return CommitSharedPlayerUse(false, false);
        }

        internal bool TryCommitSuccessfulCargoStrapCollision(uint sequence)
        {
            if (sequence == 0 ||
                sequence == lastCommittedCargoStrapSequence ||
                IsPlayerThrowExhausted)
            {
                return false;
            }

            lastCommittedCargoStrapSequence = sequence;
            return CommitSharedPlayerUse(true, false);
        }

        internal bool TryCommitSuccessfulCargoStrapImpact(uint sequence)
        {
            if (sequence == 0 ||
                sequence == lastCommittedCargoStrapSequence ||
                IsPlayerThrowExhausted)
            {
                return false;
            }

            lastCommittedCargoStrapSequence = sequence;
            return CommitSharedPlayerUse(false, true);
        }

        internal void FinalizeDeferredCargoStrapDepletion()
        {
            if (!deferredCargoStrapDepletion)
            {
                return;
            }

            deferredCargoStrapDepletion = false;
            if (throwDepletionPending)
            {
                throwDepletionCollisionObserved = true;
            }
        }

        internal void RecordPhysicalReleaseImpact(
            PhysicalReleaseSnapshot snapshot,
            float kineticEnergy)
        {
            if (!snapshot.IsValid || snapshot.Target != this || snapshot.Sequence == 0)
            {
                return;
            }

            lastPhysicalReleaseImpactSnapshot = snapshot;
            lastPhysicalReleaseKineticEnergy = Mathf.Max(0f, kineticEnergy);
        }

        internal bool TryGetPhysicalReleaseImpact(
            uint attackSequence,
            out PhysicalReleaseSnapshot snapshot,
            out float kineticEnergy)
        {
            snapshot = lastPhysicalReleaseImpactSnapshot;
            kineticEnergy = lastPhysicalReleaseKineticEnergy;
            return snapshot.IsValid &&
                   snapshot.Target == this &&
                   snapshot.Sequence == attackSequence;
        }

        internal void BeginCargoStrapCollisionObservation(
            uint attackSequence,
            Transform ignoredHierarchy,
            float minimumRelativeSpeed)
        {
            observedCargoStrapAttackSequence = attackSequence;
            cargoStrapIgnoredHierarchy = ignoredHierarchy;
            cargoStrapMinimumCollisionSpeed = Mathf.Max(0f, minimumRelativeSpeed);
            cargoStrapCollisionLatched = false;
            cargoStrapCollisionOther = null;
            cargoStrapCollisionSpeed = 0f;
        }

        internal bool TryConsumeCargoStrapCollision(
            uint attackSequence,
            out Collider other,
            out float relativeSpeed)
        {
            if (attackSequence == 0 ||
                attackSequence != observedCargoStrapAttackSequence ||
                !cargoStrapCollisionLatched)
            {
                other = null;
                relativeSpeed = 0f;
                return false;
            }

            other = cargoStrapCollisionOther;
            relativeSpeed = cargoStrapCollisionSpeed;
            CancelCargoStrapCollisionObservation(attackSequence);
            return true;
        }

        internal void CancelCargoStrapCollisionObservation(uint attackSequence)
        {
            if (attackSequence != 0 &&
                attackSequence != observedCargoStrapAttackSequence)
            {
                return;
            }

            observedCargoStrapAttackSequence = 0;
            cargoStrapIgnoredHierarchy = null;
            cargoStrapMinimumCollisionSpeed = 0f;
            cargoStrapCollisionLatched = false;
            cargoStrapCollisionOther = null;
            cargoStrapCollisionSpeed = 0f;
        }

        private void TryLatchCargoStrapCollision(
            Collider other,
            float relativeSpeed)
        {
            if (observedCargoStrapAttackSequence == 0 ||
                cargoStrapCollisionLatched || other == null ||
                relativeSpeed < cargoStrapMinimumCollisionSpeed ||
                other.transform == transform ||
                other.transform.IsChildOf(transform) ||
                (cargoStrapIgnoredHierarchy != null &&
                 (other.transform == cargoStrapIgnoredHierarchy ||
                  other.transform.IsChildOf(cargoStrapIgnoredHierarchy))))
            {
                return;
            }

            cargoStrapCollisionLatched = true;
            cargoStrapCollisionOther = other;
            cargoStrapCollisionSpeed = relativeSpeed;
        }

        private bool CommitSharedPlayerUse(
            bool collisionAlreadyObserved,
            bool deferDepletion)
        {
            committedPlayerThrowUses++;
            if (!IsPlayerThrowExhausted)
            {
                return true;
            }

            throwDepletionPending = true;
            deferredCargoStrapDepletion = deferDepletion;
            throwDepletionCollisionObserved =
                collisionAlreadyObserved && !deferDepletion;
            return true;
        }

        internal void RestorePlayerThrowUseBaseline()
        {
            committedPlayerThrowUses = 0;
            lastCommittedPlayerThrowSequence = 0;
            lastCommittedCargoStrapSequence = 0;
            throwDepletionPending = false;
            throwDepletionCollisionObserved = false;
            deferredCargoStrapDepletion = false;
            collisionSequence = 0;
            lastCollisionOther = null;
            lastCollisionRelativeSpeed = 0f;
            lastPhysicalReleaseImpactSnapshot = default;
            lastPhysicalReleaseKineticEnergy = 0f;
            CancelCargoStrapCollisionObservation(0);
        }
    }
}
