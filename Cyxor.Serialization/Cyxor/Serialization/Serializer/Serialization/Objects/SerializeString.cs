using System;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        public void Serialize(string? value)
            => InternalSerialize(value == null ? ReadOnlySpan<char>.Empty : value.AsSpan(),
                raw: AutoRaw, containsNullPointer: value == null);

        public void Serialize(string? value, bool utf8Encoding)
            => InternalSerialize(value == null ? ReadOnlySpan<char>.Empty : value.AsSpan(),
                raw: AutoRaw, containsNullPointer: value == null, utf8Encoding: utf8Encoding);

        public void Serialize(string? value, int start, bool utf8Encoding = true)
            => InternalSerialize(value == null ? ReadOnlySpan<char>.Empty : value.AsSpan(start),
                raw: false, containsNullPointer: value == null, utf8Encoding: utf8Encoding);

        public void Serialize(string? value, int start, int length, bool utf8Encoding = true)
            => InternalSerialize(value == null ? ReadOnlySpan<char>.Empty : value.AsSpan(start, length),
                raw: false, containsNullPointer: value == null, utf8Encoding: utf8Encoding);

        public void SerializeRaw(string? value, bool utf8Encoding = true)
            => InternalSerialize(value == null ? ReadOnlySpan<char>.Empty : value.AsSpan(),
                raw: true, containsNullPointer: value == null, utf8Encoding: utf8Encoding);

        public void SerializeRaw(string? value, int start, bool utf8Encoding = true)
            => InternalSerialize(value == null ? ReadOnlySpan<char>.Empty : value.AsSpan(start),
                raw: true, containsNullPointer: value == null, utf8Encoding: utf8Encoding);

        public void SerializeRaw(string? value, int start, int length, bool utf8Encoding = true)
            => InternalSerialize(value == null ? ReadOnlySpan<char>.Empty : value.AsSpan(start, length),
                raw: true, containsNullPointer: value == null, utf8Encoding: utf8Encoding);
    }
}