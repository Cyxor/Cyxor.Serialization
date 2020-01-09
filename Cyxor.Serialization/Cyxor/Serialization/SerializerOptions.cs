namespace Cyxor.Serialization
{
    public readonly struct SerializerOptions : System.IEquatable<SerializerOptions>
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly bool ReadOnly;
        public readonly bool Pooling;
        public readonly bool PrefixObjectLength;
        public readonly bool HandleCircularReferences;
        public readonly int PoolThreshold;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public SerializerOptions(bool readOnly = false, bool pooling = false, int poolThreshold = 0, bool prefixObjectLength = false,
            bool handleCircularReferences = false)
        {
            ReadOnly = readOnly;
            Pooling = pooling;
            PoolThreshold = poolThreshold;
            PrefixObjectLength = prefixObjectLength;
            HandleCircularReferences = handleCircularReferences;
        }

        public override bool Equals(object? obj)
            => obj == null ? false : Equals((SerializerOptions)obj);

        public override int GetHashCode()
            // TODO:
            => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);

        public static bool operator ==(SerializerOptions left, SerializerOptions right)
            => left.Equals(right);

        public static bool operator !=(SerializerOptions left, SerializerOptions right)
            => !(left == right);

        public bool Equals(SerializerOptions other)
            => ReadOnly == other.ReadOnly
            && Pooling == other.Pooling
            && PrefixObjectLength == other.PrefixObjectLength
            && HandleCircularReferences == other.HandleCircularReferences
            && PoolThreshold == other.PoolThreshold;
    }
}