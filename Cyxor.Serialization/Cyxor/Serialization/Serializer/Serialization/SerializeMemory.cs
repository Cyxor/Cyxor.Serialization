using System;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        #region Byte

        public void Serialize(Memory<byte> value)
            => InternalSerialize(value.Span, AutoRaw);

        public void SerializeRaw(Memory<byte> value)
            => InternalSerialize(value.Span, raw: true);

        public void Serialize(ReadOnlyMemory<byte> value)
            => InternalSerialize(value.Span, AutoRaw);

        public void SerializeRaw(ReadOnlyMemory<byte> value)
            => InternalSerialize(value.Span, raw: true);

        public void Serialize(Memory<byte>? value)
            => InternalSerializeNullableValue(value, Serialize);

        public void SerializeRaw(Memory<byte>? value)
            => InternalSerializeNullableValue(value, SerializeRaw);

        public void Serialize(ReadOnlyMemory<byte>? value)
            => InternalSerializeNullableValue(value, Serialize);

        public void SerializeRaw(ReadOnlyMemory<byte>? value)
            => InternalSerializeNullableValue(value, SerializeRaw);

        #endregion Byte

        #region Char

        public void Serialize(Memory<char> value)
            => InternalSerialize(value.Span, AutoRaw);

        public void Serialize(Memory<char> value, bool utf8Encoding)
            => InternalSerialize(value.Span, AutoRaw, utf8Encoding: utf8Encoding);

        public void SerializeRaw(Memory<char> value, bool utf8Encoding = true)
            => InternalSerialize(value.Span, raw: true, utf8Encoding: utf8Encoding);

        public void Serialize(ReadOnlyMemory<char> value)
            => InternalSerialize(value.Span, AutoRaw);

        public void Serialize(ReadOnlyMemory<char> value, bool utf8Encoding)
            => InternalSerialize(value.Span, AutoRaw, utf8Encoding: utf8Encoding);

        public void SerializeRaw(ReadOnlyMemory<char> value, bool utf8Encoding)
            => InternalSerialize(value.Span, raw: true, utf8Encoding: utf8Encoding);

        public void Serialize(Memory<char>? value)
            => InternalSerializeNullableValue(value, Serialize);

        public void Serialize(Memory<char>? value, bool utf8Encoding)
            => InternalSerializeNullableValue(value, utf8Encoding, Serialize);

        public void SerializeRaw(Memory<char>? value, bool utf8Encoding = true)
            => InternalSerializeNullableValue(value, utf8Encoding, SerializeRaw);

        public void Serialize(ReadOnlyMemory<char>? value)
            => InternalSerializeNullableValue(value, Serialize);

        public void Serialize(ReadOnlyMemory<char>? value, bool utf8Encoding)
            => InternalSerializeNullableValue(value, utf8Encoding, Serialize);

        public void SerializeRaw(ReadOnlyMemory<char>? value, bool utf8Encoding = true)
            => InternalSerializeNullableValue(value, utf8Encoding, SerializeRaw);

        #endregion Char

        #region T

        public void Serialize<T>(Memory<T> value) where T : unmanaged
            => InternalSerializeGeneric<T>(value.Span, AutoRaw);

        public void SerializeRaw<T>(Memory<T> value) where T : unmanaged
            => InternalSerializeGeneric<T>(value.Span, raw: true);

        public void Serialize<T>(ReadOnlyMemory<T> value) where T : unmanaged
            => InternalSerializeGeneric(value.Span, AutoRaw);

        public void SerializeRaw<T>(ReadOnlyMemory<T> value) where T : unmanaged
            => InternalSerializeGeneric(value.Span, raw: true);

        public void Serialize<T>(Memory<T>? value) where T : unmanaged
            => InternalSerializeNullableValue(value, Serialize);

        public void SerializeRaw<T>(Memory<T>? value) where T : unmanaged
            => InternalSerializeNullableValue(value, SerializeRaw);

        public void Serialize<T>(ReadOnlyMemory<T>? value) where T : unmanaged
            => InternalSerializeNullableValue(value, Serialize);

        public void SerializeRaw<T>(ReadOnlyMemory<T>? value) where T : unmanaged
            => InternalSerializeNullableValue(value, SerializeRaw);

        #endregion T
    }
}