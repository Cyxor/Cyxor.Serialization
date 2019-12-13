#if !NET20 && !NET35 && !NET40 && !NETSTANDARD1_0

using System;

namespace Cyxor.Serialization
{
    partial class SerializationStream
    {
        public void Serialize<T>(Memory<T> memory) where T : unmanaged
            => InternalSerialize((ReadOnlySpan<T>)memory.Span, AutoRaw);

        public void SerializeRaw<T>(Memory<T> memory) where T : unmanaged
            => InternalSerialize((ReadOnlySpan<T>)memory.Span, raw: true);

        public void Serialize<T>(ReadOnlyMemory<T> readOnlyMemory) where T : unmanaged
            => InternalSerialize(readOnlyMemory.Span, AutoRaw);

        public void SerializeRaw<T>(ReadOnlyMemory<T> readOnlyMemory) where T : unmanaged
            => InternalSerialize(readOnlyMemory.Span, raw: true);

        public void Serialize<T>(Memory<T>? memory) where T : unmanaged
            => SerializeNullableValue(memory, Serialize);

        public void Serialize<T>(ReadOnlyMemory<T>? readOnlyMemory) where T : unmanaged
            => SerializeNullableValue(readOnlyMemory, Serialize);
    }
}

#endif