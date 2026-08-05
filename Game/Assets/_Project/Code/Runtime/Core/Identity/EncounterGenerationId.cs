using System;

namespace BreakNotice.Core.Identity
{
    public readonly struct EncounterGenerationId : IEquatable<EncounterGenerationId>
    {
        private readonly uint _value;

        public EncounterGenerationId(uint value)
        {
            if (value == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Encounter generation ID must be non-zero.");
            }

            _value = value;
        }

        public uint Value => _value;

        public bool IsValid => _value != 0;

        public bool Equals(EncounterGenerationId other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            return obj is EncounterGenerationId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public static bool operator ==(EncounterGenerationId left, EncounterGenerationId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EncounterGenerationId left, EncounterGenerationId right)
        {
            return !left.Equals(right);
        }
    }
}
