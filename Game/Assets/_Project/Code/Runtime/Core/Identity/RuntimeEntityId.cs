using System;

namespace BreakNotice.Core.Identity
{
    public readonly struct RuntimeEntityId : IEquatable<RuntimeEntityId>
    {
        private readonly ulong _value;

        public RuntimeEntityId(ulong value)
        {
            if (value == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Runtime entity ID must be non-zero.");
            }

            _value = value;
        }

        public ulong Value => _value;

        public bool IsValid => _value != 0;

        public bool Equals(RuntimeEntityId other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            return obj is RuntimeEntityId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public static bool operator ==(RuntimeEntityId left, RuntimeEntityId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RuntimeEntityId left, RuntimeEntityId right)
        {
            return !left.Equals(right);
        }
    }
}
