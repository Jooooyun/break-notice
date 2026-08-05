using System;
using System.Collections.Generic;

namespace BreakNotice.Core.Identity
{
    public sealed class RuntimeEntityRegistry
    {
        private readonly Dictionary<RuntimeEntityId, RuntimeIdentity> _identities;

        public RuntimeEntityRegistry(EncounterGenerationId generationId)
        {
            if (!generationId.IsValid)
            {
                throw new ArgumentException("Encounter generation ID must be valid.", nameof(generationId));
            }

            GenerationId = generationId;
            _identities = new Dictionary<RuntimeEntityId, RuntimeIdentity>();
        }

        public EncounterGenerationId GenerationId { get; }

        public int Count => _identities.Count;

        public void Register(RuntimeIdentity identity)
        {
            if (!identity.ContentId.IsValid
                || !identity.RuntimeEntityId.IsValid
                || !identity.EncounterGenerationId.IsValid)
            {
                throw new ArgumentException("Runtime identity must be valid.", nameof(identity));
            }

            if (identity.EncounterGenerationId != GenerationId)
            {
                throw new ArgumentException("Runtime identity belongs to a different encounter generation.", nameof(identity));
            }

            if (_identities.ContainsKey(identity.RuntimeEntityId))
            {
                throw new InvalidOperationException("Runtime entity ID is already registered.");
            }

            _identities.Add(identity.RuntimeEntityId, identity);
        }

        public bool TryGet(RuntimeEntityId runtimeEntityId, out RuntimeIdentity identity)
        {
            ValidateRuntimeEntityId(runtimeEntityId);
            return _identities.TryGetValue(runtimeEntityId, out identity);
        }

        public bool Remove(RuntimeEntityId runtimeEntityId)
        {
            ValidateRuntimeEntityId(runtimeEntityId);
            return _identities.Remove(runtimeEntityId);
        }

        public void Clear()
        {
            _identities.Clear();
        }

        private static void ValidateRuntimeEntityId(RuntimeEntityId runtimeEntityId)
        {
            if (!runtimeEntityId.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(runtimeEntityId), "Runtime entity ID must be valid.");
            }
        }
    }
}
