using System;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        public void Serialize<T>(Memory<T> value) where T : unmanaged
            => InternalSerialize<T>(value.Span, AutoRaw);

        public void SerializeRaw<T>(Memory<T> value) where T : unmanaged
            => InternalSerialize<T>(value.Span, raw: true);

        public void Serialize<T>(ReadOnlyMemory<T> value) where T : unmanaged
            => InternalSerialize(value.Span, AutoRaw);

        public void SerializeRaw<T>(ReadOnlyMemory<T> value) where T : unmanaged
            => InternalSerialize(value.Span, raw: true);

        //public void Serialize<T>(Memory<T>? value) where T : unmanaged
        //    => SerializeNullableValue(value, Serialize);

        //public void Serialize<T>(ReadOnlyMemory<T>? value) where T : unmanaged
        //    => SerializeNullableValue(value, Serialize);
    }
}