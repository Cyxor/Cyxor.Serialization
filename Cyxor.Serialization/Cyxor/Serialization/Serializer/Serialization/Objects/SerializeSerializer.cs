﻿namespace Cyxor.Serialization
{
    partial class Serializer
    {
        public void Serialize(Serializer? value)
            => InternalSerialize(_memory.Span.Slice(0, _length), raw: AutoRaw, nullValue: value == null);

        public void SerializeRaw(Serializer? value)
            => InternalSerialize(_memory.Span.Slice(0, _length), raw: true, nullValue: value == null);
    }
}