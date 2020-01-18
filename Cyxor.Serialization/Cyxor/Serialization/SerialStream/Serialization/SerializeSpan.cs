using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void InternalSerialize(in ReadOnlySpan<byte> value, bool raw, bool containsNullPointer = false)
        {
            if (containsNullPointer)
            {
                if (!raw)
                    Serialize((byte)0);

                return;
            }

            if (value.IsEmpty)
            {
                if (!raw)
                    Serialize(EmptyMap);

                return;
            }

            var count = value.Length;

            if (!raw)
                SerializeSequenceHeader(count);

            EnsureCapacity(count, SerializerOperation.Serialize);

            value.CopyTo(_buffer.AsSpan(_position..count));

            _position += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InternalSerialize<T>(in ReadOnlySpan<T> value, bool raw, bool containsNullPointer = false) where T : unmanaged
            => InternalSerialize(MemoryMarshal.Cast<T, byte>(value), raw, containsNullPointer);

        public void Serialize<T>(Span<T> value) where T : unmanaged
            => InternalSerialize((ReadOnlySpan<T>)value, AutoRaw);

        public void SerializeRaw<T>(Span<T> value) where T : unmanaged
            => InternalSerialize((ReadOnlySpan<T>)value, raw: true);

        public void Serialize<T>(ReadOnlySpan<T> value) where T : unmanaged
            => InternalSerialize(value, AutoRaw);

        public void SerializeRaw<T>(ReadOnlySpan<T> value) where T : unmanaged
            => InternalSerialize(value, raw: true);
    }
}