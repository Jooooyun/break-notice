using System;

namespace BreakNotice.Core.Identity
{
    public readonly struct RuntimeIdentity : IEquatable<RuntimeIdentity>
    {
        public RuntimeIdentity(
            ContentId contentId,
            RuntimeEntityId runtimeEntityId,
            EncounterGenerationId encounterGenerationId)
        {
            if (!contentId.IsValid)
            {
                throw new ArgumentException("Content ID must be valid.", nameof(contentId));
            }

            if (!runtimeEntityId.IsValid)
            {
                throw new ArgumentException("Runtime entity ID must be valid.", nameof(runtimeEntityId));
            }

            if (!encounterGenerationId.IsValid)
            {
                throw new ArgumentException("Encounter generation ID must be valid.", nameof(encounterGenerationId));
            }

            ContentId = contentId;
            RuntimeEntityId = runtimeEntityId;
            EncounterGenerationId = encounterGenerationId;
        }

        public ContentId ContentId { get; }

        public RuntimeEntityId RuntimeEntityId { get; }

        public EncounterGenerationId EncounterGenerationId { get; }

        public bool Equals(RuntimeIdentity other)
        {
            return ContentId.Equals(other.ContentId)
                && RuntimeEntityId.Equals(other.RuntimeEntityId)
                && EncounterGenerationId.Equals(other.EncounterGenerationId);
        }

        public override bool Equals(object obj)
        {
            return obj is RuntimeIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ContentId.GetHashCode();
                hashCode = (hashCode * 397) ^ RuntimeEntityId.GetHashCode();
                hashCode = (hashCode * 397) ^ EncounterGenerationId.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{ContentId}:{RuntimeEntityId}@{EncounterGenerationId}";
        }

        public static bool operator ==(RuntimeIdentity left, RuntimeIdentity right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RuntimeIdentity left, RuntimeIdentity right)
        {
            return !left.Equals(right);
        }
    }
}
