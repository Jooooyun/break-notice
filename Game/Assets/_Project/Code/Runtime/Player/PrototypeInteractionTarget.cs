using System;
using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
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

        public string ContentDefinitionId => contentDefinitionId;

        public string RuntimeInstanceKey => runtimeInstanceKey;

        public string DisplayName => displayName;

        public PrototypeWeightClass WeightClass => weightClass;

        public PrototypeInteractionRole Role => role;

        public bool DirectAcquisitionEligible => directAcquisitionEligible;

        public bool CatchEligible => catchEligible;

        public bool StrapEligible => strapEligible;

        public bool HasLoadBearingMetalAnchor => hasLoadBearingMetalAnchor;

        public bool IsScoopEligible =>
            directAcquisitionEligible &&
            (weightClass == PrototypeWeightClass.Light ||
             weightClass == PrototypeWeightClass.Medium) &&
            role != PrototypeInteractionRole.Drawer;

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
        }
    }
}
