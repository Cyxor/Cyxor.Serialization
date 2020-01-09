using System;
using System.Runtime.InteropServices;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        void InternalSerialize<T>(in ReadOnlySpan<T> readOnlySpan, bool raw) where T : unmanaged
        {
            if (readOnlySpan.IsEmpty)
            {
                if (!raw)
                    Serialize(ObjectProperties.EmptyMap);

                return;
            }

            var bytesReadOnlySpan = MemoryMarshal.Cast<T, byte>(readOnlySpan);

            if (!raw)
                SerializeOp(bytesReadOnlySpan.Length);

            EnsureCapacity(bytesReadOnlySpan.Length, SerializerOperation.Serialize);

            bytesReadOnlySpan.CopyTo(new Span<byte>(buffer, position, bytesReadOnlySpan.Length));
        }

        public void Serialize<T>(Span<T> span) where T : unmanaged
            => InternalSerialize((ReadOnlySpan<T>)span, AutoRaw);

        public void SerializeRaw<T>(Span<T> span) where T : unmanaged
            => InternalSerialize((ReadOnlySpan<T>)span, raw: true);

        public void Serialize<T>(ReadOnlySpan<T> readOnlySpan) where T : unmanaged
            => InternalSerialize(readOnlySpan, AutoRaw);

        public void SerializeRaw<T>(ReadOnlySpan<T> readOnlySpan) where T : unmanaged
            => InternalSerialize(readOnlySpan, raw: true);
    }
}