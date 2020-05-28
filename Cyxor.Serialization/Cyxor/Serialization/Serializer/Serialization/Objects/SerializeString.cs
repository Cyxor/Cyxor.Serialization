using System;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        public void Serialize(string? value)
            => InternalSerialize(value == null ? ReadOnlySpan<char>.Empty : value.AsSpan(), raw: AutoRaw, containsNullPointer: value == null);

        public void Serialize(string value, int start)
            => InternalSerialize(value.AsSpan(start), raw: false);

        public void Serialize(string value, int start, int length)
            => InternalSerialize(value.AsSpan(start, length), raw: false);

        public void SerializeRaw(string? value)
            => InternalSerialize(value == null ? ReadOnlySpan<char>.Empty : value.AsSpan(), raw: true, containsNullPointer: value == null);

        public void SerializeRaw(string value, int start)
            => InternalSerialize(value.AsSpan(start), raw: true);

        public void SerializeRaw(string value, int start, int length)
            => InternalSerialize(value.AsSpan(start, length), raw: true);
    }
}