using System;

namespace Cyxor.Serialization
{
    public readonly struct SerializerOptions : IEquatable<SerializerOptions>
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public readonly bool Pooling;
        public readonly bool ReadOnly;
        public readonly bool FixedBuffer;
        public readonly bool ClearMemory;
        public readonly bool ReverseEndianness;
        public readonly bool PrefixObjectLength;
        public readonly bool HandleCircularReferences;
        internal readonly bool NeedDisposeBuffer;

        public readonly int MaxCapacity;
        public readonly int PoolThreshold;
        public readonly int InitialCapacity;

        // TODO: Compact boolean properties into BitSerializer?
#pragma warning restore CA1051 // Do not declare visible instance fields

        public SerializerOptions(
            bool pooling = false,
            bool readOnly = false,
            bool fixedBuffer = false,
            bool clearMemory = false,
            bool reverseEndianness = false,
            bool prefixObjectLength = false,
            bool handleCircularReferences = false,
            int maxCapacity = int.MaxValue,
            int poolThreshold = 0,
            int initialCapacity = 0)
        {
            Pooling = pooling;
            ReadOnly = readOnly;
            FixedBuffer = fixedBuffer;
            ClearMemory = clearMemory;
            ReverseEndianness = reverseEndianness;
            PrefixObjectLength = prefixObjectLength;
            HandleCircularReferences = handleCircularReferences;
            NeedDisposeBuffer = false;

            MaxCapacity = maxCapacity;
            PoolThreshold = poolThreshold;
            InitialCapacity = initialCapacity;
        }

        internal SerializerOptions(in SerializerOptions options, bool needDisposeBuffer)
        {
            Pooling = options.Pooling;
            ReadOnly = options.ReadOnly;
            FixedBuffer = options.FixedBuffer;
            ClearMemory = options.ClearMemory;
            ReverseEndianness = options.ReverseEndianness;
            PrefixObjectLength = options.PrefixObjectLength;
            HandleCircularReferences = options.HandleCircularReferences;
            NeedDisposeBuffer = needDisposeBuffer;

            MaxCapacity = options.MaxCapacity;
            PoolThreshold = options.PoolThreshold;
            InitialCapacity = options.InitialCapacity;
        }

        public override bool Equals(object? obj)
            => obj == null ? false : Equals((SerializerOptions)obj);

        public override int GetHashCode()
        {
            var bitSerializer = new BitSerializer();
            bitSerializer[0] = Pooling;
            bitSerializer[1] = ReadOnly;
            bitSerializer[2] = FixedBuffer;
            bitSerializer[3] = ClearMemory;
            bitSerializer[4] = ReverseEndianness;
            bitSerializer[5] = PrefixObjectLength;
            bitSerializer[6] = HandleCircularReferences;
            bitSerializer[7] = NeedDisposeBuffer;

            return HashCode.Combine(
                bitSerializer,
                MaxCapacity,
                PoolThreshold,
                InitialCapacity);
        }

        public static bool operator ==(SerializerOptions left, SerializerOptions right)
            => left.Equals(right);

        public static bool operator !=(SerializerOptions left, SerializerOptions right)
            => !(left == right);

        public bool Equals(SerializerOptions other)
            => Pooling == other.Pooling
            && ReadOnly == other.ReadOnly
            && FixedBuffer == other.FixedBuffer
            && ClearMemory == other.ClearMemory
            && ReverseEndianness == other.ReverseEndianness
            && PrefixObjectLength == other.PrefixObjectLength
            && HandleCircularReferences == other.HandleCircularReferences
            && NeedDisposeBuffer == other.NeedDisposeBuffer
            && MaxCapacity == other.MaxCapacity
            && PoolThreshold == other.PoolThreshold
            && InitialCapacity == other.InitialCapacity;
    }
}