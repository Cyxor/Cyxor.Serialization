using System;

namespace Cyxor.Serialization
{
    public readonly struct SerializerOptions : IEquatable<SerializerOptions>
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly bool ReadOnly;
        public readonly bool Pooling;
        public readonly bool ReverseByteOrder;
        public readonly bool PrefixObjectLength;
        public readonly bool HandleCircularReferences;

        public readonly int PoolThreshold;
        public readonly int InitialCapacity;
        public readonly int MaxCapacity;

        // TODO: Compact boolean properties into BitSerializer?
#pragma warning restore CA1051 // Do not declare visible instance fields

        public SerializerOptions(
            bool readOnly = false,
            bool pooling = false,
            bool reverseByteOrder = false,
            bool prefixObjectLength = false,
            bool handleCircularReferences = false,
            int poolThreshold = 0,
            int initialCapacity = 0,
            int maxCapacity = int.MaxValue)
        {
            ReadOnly = readOnly;
            Pooling = pooling;
            ReverseByteOrder = reverseByteOrder;
            PrefixObjectLength = prefixObjectLength;
            HandleCircularReferences = handleCircularReferences;
            PoolThreshold = poolThreshold;
            InitialCapacity = initialCapacity;
            MaxCapacity = maxCapacity;
        }

        public override bool Equals(object? obj)
            => obj == null ? false : Equals((SerializerOptions)obj);

        public override int GetHashCode()
            => HashCode.Combine(
                ReadOnly, 
                Pooling,
                ReverseByteOrder,
                PrefixObjectLength, 
                HandleCircularReferences, 
                PoolThreshold,
                InitialCapacity,
                MaxCapacity);

        public static bool operator ==(SerializerOptions left, SerializerOptions right)
            => left.Equals(right);

        public static bool operator !=(SerializerOptions left, SerializerOptions right)
            => !(left == right);

        public bool Equals(SerializerOptions other)
            => ReadOnly == other.ReadOnly
            && Pooling == other.Pooling
            && ReverseByteOrder == other.ReverseByteOrder
            && PrefixObjectLength == other.PrefixObjectLength
            && HandleCircularReferences == other.HandleCircularReferences
            && PoolThreshold == other.PoolThreshold
            && InitialCapacity == other.InitialCapacity
            && MaxCapacity == other.MaxCapacity;
    }
}