using System;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        public void Serialize(Serializer? value)
            => InternalSerialize(new ReadOnlySpan<byte>(value?._buffer, 0, value?._length ?? 0), raw: AutoRaw, containsNullPointer: value == null);

        public void SerializeRaw(Serializer? value)
            => InternalSerialize(new ReadOnlySpan<byte>(value?._buffer, 0, value?._length ?? 0), raw: true, containsNullPointer: value == null);
    }
}