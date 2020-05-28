using System;

namespace Cyxor.Serialization
{
    partial class Serializer
    {
        void InternalSerialize(System.Version? value, bool raw)
        {
            if (value == null)
            {
                if (!raw)
                    Serialize((byte)0);

                return;
            }

            var major = value.Major;
            var minor = value.Minor;
            var build = value.Build;
            var revision = value.Revision;

            var isEmpty = major == 0 && minor == 0 && build == 0 && revision == 0;

            if (isEmpty)
            {
                if (!raw)
                    Serialize(EmptyMap);

                return;
            }

            Span<byte> span = stackalloc byte[20];

            var length = 0;
            length += InternalSerializeCompressedInt((ulong)major, ref span, spanOffset: length);
            length += InternalSerializeCompressedInt((ulong)minor, ref span, spanOffset: length);
            length += InternalSerializeCompressedInt((ulong)build, ref span, spanOffset: length);
            length += InternalSerializeCompressedInt((ulong)revision, ref span, spanOffset: length);

            if (!raw)
                InternalSerializeSequenceHeader(length);

            if (_stream != null)
                _stream.Write(span.Slice(0, length));
            else
            {
                span.CopyTo(_memory.Span.Slice(_position, length));
                _position += length;
            }
        }

        public void Serialize(System.Version? value)
            => InternalSerialize(value, AutoRaw);

        public void SerializeRaw(System.Version? value)
            => InternalSerialize(value, raw: true);
    }
}