using System;

namespace BreakNotice.Core.Identity
{
    public sealed class RuntimeIdentityAllocator
    {
        private ulong _lastAllocatedValue;

        public RuntimeIdentityAllocator(EncounterGenerationId generationId)
        {
            if (!generationId.IsValid)
            {
                throw new ArgumentException("Encounter generation ID must be valid.", nameof(generationId));
            }

            GenerationId = generationId;
        }

        public EncounterGenerationId GenerationId { get; }

        public RuntimeEntityId Allocate()
        {
            if (_lastAllocatedValue == ulong.MaxValue)
            {
                throw new InvalidOperationException("Runtime entity ID allocation is exhausted.");
            }

            _lastAllocatedValue++;
            return new RuntimeEntityId(_lastAllocatedValue);
        }
    }
}
