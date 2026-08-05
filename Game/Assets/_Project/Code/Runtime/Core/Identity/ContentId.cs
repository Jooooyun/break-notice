using System;

namespace BreakNotice.Core.Identity
{
    public readonly struct ContentId : IEquatable<ContentId>
    {
        private readonly string _value;

        public ContentId(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Length == 0 || string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Content ID must contain a non-whitespace value.", nameof(value));
            }

            _value = value;
        }

        public string Value
        {
            get
            {
                if (!IsValid)
                {
                    throw new InvalidOperationException("The default ContentId is invalid.");
                }

                return _value;
            }
        }

        public bool IsValid => !string.IsNullOrWhiteSpace(_value);

        public bool Equals(ContentId other)
        {
            return StringComparer.Ordinal.Equals(_value, other._value);
        }

        public override bool Equals(object obj)
        {
            return obj is ContentId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value == null ? 0 : StringComparer.Ordinal.GetHashCode(_value);
        }

        public override string ToString()
        {
            return _value ?? string.Empty;
        }

        public static bool operator ==(ContentId left, ContentId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ContentId left, ContentId right)
        {
            return !left.Equals(right);
        }
    }
}
