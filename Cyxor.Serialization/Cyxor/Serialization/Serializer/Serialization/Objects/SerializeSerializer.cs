using System;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        public void Serialize(Serializer? value)
            => InternalSerialize(value == null ? ReadOnlySpan<byte>.Empty : value.AsSpan<byte>(),
                raw: AutoRaw, containsNullPointer: value == null);

        public void Serialize(Serializer? value, int start)
            => InternalSerialize(value == null ? ReadOnlySpan<byte>.Empty : value.AsSpan<byte>(start),
                raw: false, containsNullPointer: value == null);

        public void Serialize(Serializer? value, int start, int length)
            => InternalSerialize(value == null ? ReadOnlySpan<byte>.Empty : value.AsSpan<byte>(start, length),
                raw: false, containsNullPointer: value == null);

        public void SerializeRaw(Serializer? value)
            => InternalSerialize(value == null ? ReadOnlySpan<byte>.Empty : value.AsSpan<byte>(),
                raw: true, containsNullPointer: value == null);

        public void SerializeRaw(Serializer? value, int start)
            => InternalSerialize(value == null ? ReadOnlySpan<byte>.Empty : value.AsSpan<byte>(start),
                raw: true, containsNullPointer: value == null);

        public void SerializeRaw(Serializer? value, int start, int length)
            => InternalSerialize(value == null ? ReadOnlySpan<byte>.Empty : value.AsSpan<byte>(start, length),
                raw: true, containsNullPointer: value == null);
    }
}